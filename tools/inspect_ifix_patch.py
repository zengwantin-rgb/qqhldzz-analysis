"""Read combined/IFix serialization as data, never load or execute a patch."""
import json
import pathlib
import re
import struct
import zipfile

IFIX_MAGIC = 8881892042087324221
CRITICAL_TYPES = {
    'HappyMahjong.Game.NextToDoHelper', 'HappyMahjong.Game.RevcDealCommand',
    'HappyMahjong.Game.RecvGameFinishCommand', 'HappyMahjong.Game.FanRecommendDetail',
    'HappyMahjong.Game.NewFanRecommendDataModel', 'HappyMahjong.Game.RecvChooseNewRecommendFanTargtCommand',
    'GameModule.MJGameBaseConfigDetail', 'GameModule.MJGameBaseConfigUtil',
    'HappyMahjong.Common.Util', 'HappyMahjong.NewGame.MJGameContext', 'Pb2TdrConverter',
}


class Reader:
    def __init__(self, data):
        self.data, self.pos = data, 0

    def take(self, size):
        if size < 0 or self.pos + size > len(self.data):
            raise ValueError('Truncated patch section')
        value = self.data[self.pos:self.pos + size]
        self.pos += size
        return value

    def integer(self):
        return struct.unpack('<i', self.take(4))[0]

    def count(self, maximum=100000):
        value = self.integer()
        if not 0 <= value <= maximum:
            raise ValueError('Patch count outside limits')
        return value

    def boolean(self):
        value = self.take(1)[0]
        if value not in (0, 1):
            raise ValueError('Invalid serialized boolean')
        return bool(value)

    def string(self):
        size = 0
        for shift in range(0, 35, 7):
            value = self.take(1)[0]
            size |= (value & 127) << shift
            if value < 128:
                if size > 1024 * 1024:
                    raise ValueError('Patch string limit')
                return self.take(size).decode('utf-8')
        raise ValueError('Invalid serialized string length')


def split_type(value):
    depth = 0
    for index, char in enumerate(value):
        if char == '[':
            depth += 1
        elif char == ']':
            depth -= 1
        elif char == ',' and depth == 0:
            return value[:index].split('[', 1)[0].rstrip('&*'), value[index + 1:].split(',')[0].strip()
    return value.split('[', 1)[0].rstrip('&*'), None


def resolve_type(value, catalog):
    name, assembly = split_type(value)
    key = name + ', ' + assembly if assembly else None
    if key in catalog:
        return key
    choices = [key for key in catalog if key.startswith(name + ', ')]
    return choices[0] if len(choices) == 1 else None


def inspect_block(data, catalog):
    reader = Reader(data)
    if struct.unpack('<Q', reader.take(8))[0] != IFIX_MAGIC:
        raise ValueError('IFix instruction magic mismatch')
    bridge = reader.string()
    extern_types = [reader.string() for _ in range(reader.count())]

    def type_at(index):
        if not 0 <= index < len(extern_types):
            raise ValueError('External type index outside table')
        return resolve_type(extern_types[index], catalog)

    def method():
        generic = reader.boolean()
        owner = type_at(reader.integer())
        name = reader.string()
        generic_count = reader.count(1000) if generic else 0
        for _ in range(generic_count):
            type_at(reader.integer())
        param_count = reader.count(1000)
        for _ in range(param_count):
            if generic and reader.boolean():
                reader.string()
            else:
                type_at(reader.integer())
        safe_name = name if owner and re.fullmatch(r'[A-Za-z_.$<>][A-Za-z0-9_.$<>+`-]{0,199}', name) else '$unresolved_method'
        return {'declaring_type': owner or '$unresolved_type', 'method': safe_name,
                'parameter_count': param_count, 'generic_argument_count': generic_count}

    method_count = reader.count()
    instruction_counts = []
    for _ in range(method_count):
        count = reader.count(1000000)
        instruction_counts.append(count)
        reader.take(count * 8)
        reader.take(reader.count() * 24)
    external_methods = [method() for _ in range(reader.count())]
    string_count = reader.count()
    for _ in range(string_count):
        reader.string()
    field_count = reader.count()
    for _ in range(field_count):
        new = reader.boolean()
        type_at(reader.integer())
        reader.string()
        if new:
            type_at(reader.integer())
            reader.integer()
    static_field_count = reader.count()
    for _ in range(static_field_count):
        type_at(reader.integer())
        reader.integer()
    anonymous_count = reader.count()
    slot_types = []
    for _ in range(anonymous_count):
        reader.take(reader.count() * 4)
        reader.integer()
        reader.integer()
        for _ in range(reader.count()):
            owner = type_at(reader.integer())
            if owner is None:
                raise ValueError('Anonymous interface method count unavailable')
            count = catalog[owner]
            reader.take(count * 4)
            slot_types.append({'type': owner, 'declared_public_instance_methods': count})
        reader.take(reader.count() * 4)
    wrapper = reader.string()
    reader.string()  # IDMAP assembly suffix, no runtime class resolution.
    bindings = []
    for _ in range(reader.count()):
        target = method()
        vm_id = reader.integer()
        if not 0 <= vm_id < method_count:
            raise ValueError('Patched VM method index outside table')
        bindings.append({**target, 'vm_method_id': vm_id})
    new_classes = None
    if reader.pos < len(data):
        new_classes = reader.count()
        for _ in range(new_classes):
            reader.string()
    if reader.pos != len(data):
        raise ValueError('Unconsumed patch trailer')
    critical = [row for row in bindings if row['declaring_type'].split(',')[0] in CRITICAL_TYPES]
    external_critical = [row for row in external_methods if row['declaring_type'].split(',')[0] in CRITICAL_TYPES]
    return {'bytes': len(data), 'fully_consumed': True,
            'interface_bridge': resolve_type(bridge, catalog) or '$unresolved_type',
            'wrapper_manager': resolve_type(wrapper, catalog) or '$unresolved_type',
            'external_type_count': len(extern_types), 'vm_method_count': method_count,
            'unresolved_external_type_count': sum(resolve_type(value, catalog) is None for value in extern_types),
            'instruction_count': sum(instruction_counts), 'external_method_count': len(external_methods),
            'intern_string_count': string_count, 'field_count': field_count,
            'static_field_count': static_field_count, 'anonymous_storey_count': anonymous_count,
            'resolved_slot_types': slot_types, 'new_class_count': new_classes,
            'patched_methods': bindings, 'critical_owner_bindings': critical,
            'critical_owner_external_references': external_critical,
            'unresolved_binding_count': sum(row['declaring_type'] == '$unresolved_type' for row in bindings)}


def inspect_combined(data, magic, catalog):
    reader = Reader(data)
    if struct.unpack('<Q', reader.take(8))[0] != magic:
        raise ValueError('Combined patch magic mismatch')
    version, count = struct.unpack('<HH', reader.take(4))
    if version != 1:
        raise ValueError('Unsupported combined patch version')
    spans = [(reader.integer(), reader.integer()) for _ in range(count)]
    end = reader.pos
    gaps = 0
    blocks = []
    for offset, length in sorted(spans):
        if offset < end or length < 8 or offset + length > len(data):
            raise ValueError('Invalid or overlapping combined patch span')
        gaps += offset - end
        blocks.append({'offset': offset, **inspect_block(data[offset:offset + length], catalog)})
        end = offset + length
    return {'format': 'combined_ifix', 'version': version, 'subpatch_count': count,
            'last_subpatch_end': end, 'file_bytes': len(data), 'trailing_bytes': len(data) - end,
            'padding_or_gap_bytes': gaps, 'fully_consumed': end == len(data) and gaps == 0,
            'subpatches': blocks}


if __name__ == '__main__':
    out = pathlib.Path('report/device')
    metadata = json.loads((out / 'patch-format-metadata.json').read_text())
    catalog = json.loads((out / 'patch-type-method-counts.json').read_text())
    if 'combined_magic' not in metadata:
        result = {'status': 'format_metadata_unavailable'}
    else:
        with zipfile.ZipFile('private-input/device.zip') as archive:
            names = [n for n in archive.namelist() if pathlib.PurePosixPath(n).name == 'PatchIOS.bytes']
            if len(names) != 1:
                raise ValueError('Expected exactly one device patch')
            data = archive.read(names[0])
        try:
            result = {'status': 'parsed', **inspect_combined(data, int(metadata['combined_magic']), catalog)}
        except (ValueError, UnicodeError, struct.error) as error:
            result = {'status': 'not_fully_parsed', 'error_category': str(error) if isinstance(error, ValueError) and not isinstance(error, UnicodeError) else type(error).__name__}
    result['limits'] = ['Patch bytes are read as serialized data; no patch, VM instruction, reflection binding or game code is executed.',
                        'Successful structural parsing does not prove that this patch was applied in the recorded session.',
                        'Only resolved source type names and restricted method metadata are emitted; intern strings and raw patch data are omitted.']
    (out / 'device-patch-summary.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
