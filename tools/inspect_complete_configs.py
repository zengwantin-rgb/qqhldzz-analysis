"""Inventory supplied configs and patch without emitting arbitrary strings or data."""
import hashlib
import json
import pathlib
import re
import struct
import zipfile
import zlib

from trace_native_file_sources import wire_fields

NAMES = {'MJGameBaseConfig.data', 'ModuleFanConfig.data', 'SvrNotifyTips.data', 'VedioRecordConfig.data', 'PatchIOS.bytes'}
LABELS = ('NextToDoHelper', 'HandleSelfRedraw', 'RevcDealCommand', 'RecvGameFinishCommand',
          'FanRecommendDetail', 'NewFanRecommendDataModel', 'MJGameContext', 'TDoWaitInfo')
SELECTED_FLAGS = {'afterTingShowAllHandTile', 'afterTingShowHuHandTile', 'showRecommendFanPanel',
                  'IsParallelDraw', 'HasSingleWall', 'canTing', 'showTingGiveCard', 'totalPlayerNum', 'totalCardNum'}


def field_mapping(source):
    return {int(index): name for index, name in re.findall(r'case\s+(\d+):\s*([A-Za-z_][A-Za-z0-9_]*)\s*=', source)}


def named_values(values, source):
    return {name: values[index] for index, name in field_mapping(source).items()
            if name in SELECTED_FLAGS and index < len(values)}


def runtime_config_matches(lines, rows, source):
    result = []
    lookup = {}
    for identifier, row in rows.items():
        if row['values'] is not None:
            lookup.setdefault(tuple(row['values']), []).append(identifier)
    for number, line in enumerate(lines, 1):
        if 'GetBaseConifgByGameID configValues' not in line:
            continue
        match = re.search(r'configValues\s*=\s*([-0-9;,|\s]+)', line)
        if not match:
            continue
        tokens = [s for s in re.split(r'[,;|\s]+', match.group(1).strip()) if s]
        if len(tokens) > 512 or not all(re.fullmatch(r'-?\d{1,10}', s) for s in tokens):
            continue
        values = [int(s) for s in tokens]
        result.append({'line': number, 'value_count': len(values),
                       'matching_device_row_ids': sorted(lookup.get(tuple(values), [])),
                       'named_config_values': named_values(values, source)})
    return result


def inflate(raw):
    decoder = zlib.decompressobj()
    clear = decoder.decompress(raw, 4 * 1024 * 1024 + 1)
    if not decoder.eof or decoder.unused_data or decoder.unconsumed_tail or len(clear) > 4 * 1024 * 1024:
        raise ValueError('Config compression limits')
    return clear


def base_config_rows(raw):
    result = {}
    for number, kind, row in wire_fields(inflate(raw)):
        if number != 1 or kind != 2:
            continue
        fields = wire_fields(row)
        ids = [v for n, k, v in fields if n == 1 and k == 0]
        strings = [v for n, k, v in fields if n == 2 and k == 2]
        if len(ids) != 1 or len(strings) != 1 or ids[0] in result:
            raise ValueError('Ambiguous config row identifiers or values')
        text = strings[0].decode('utf-8')
        values = None
        if re.fullmatch(r'[0-9,;|\s-]*', text):
            tokens = [s for s in re.split(r'[,;|\s]+', text.strip()) if s]
            if len(tokens) <= 512 and all(re.fullmatch(r'-?\d{1,10}', s) for s in tokens):
                values = [int(s) for s in tokens]
        result[ids[0]] = {'values': values, 'row_sha256': hashlib.sha256(row).hexdigest()}
    return result


def compare_base_configs(raw, original, source):
    device, bundled = base_config_rows(raw), base_config_rows(original)
    device_sections, bundled_sections = {}, {}
    for blob, sections in ((raw, device_sections), (original, bundled_sections)):
        for number, kind, value in wire_fields(inflate(blob)):
            if kind == 2:
                sections.setdefault(number, []).append(value)
    section_comparisons = [{'field_number': number,
                            'device_entries': len(device_sections.get(number, [])),
                            'bundled_entries': len(bundled_sections.get(number, [])),
                            'serialized_entries_identical': device_sections.get(number, []) == bundled_sections.get(number, [])}
                           for number in sorted(device_sections.keys() | bundled_sections.keys())]
    rows = []
    for label, data in [('device', device), ('bundled', bundled)]:
        row = data.get(149)
        values = row['values'] if row else None
        rows.append({'source': label, 'row_id': 149, 'found': row is not None,
                     'value_count': len(values) if values is not None else None,
                     'named_config_values': named_values(values, source) if values is not None else {}})
    common = device.keys() & bundled.keys()
    return {'device_sheet1_rows': len(device), 'bundled_sheet1_rows': len(bundled),
            'section_comparisons': section_comparisons,
            'added_row_ids': sorted(device.keys() - bundled.keys()), 'removed_row_ids': sorted(bundled.keys() - device.keys()),
            'changed_serialized_row_ids': sorted(i for i in common if device[i]['row_sha256'] != bundled[i]['row_sha256']),
            'changed_numeric_config_row_ids': sorted(i for i in common if device[i]['values'] != bundled[i]['values']),
            'target_game_rows': rows,
            'limits': ['Row 149 matches the recovered GameType enum; effective room overrides and patches require separate verification.',
                       'Only selected named rule flags and row IDs are emitted; other strings and field values are omitted.']}


def facts(raw):
    result = {'bytes': len(raw), 'sha256': hashlib.sha256(raw).hexdigest(),
              'known_code_name_mentions': {name: raw.count(name.encode()) + raw.count(name.encode('utf-16-le')) for name in LABELS}}
    result['ifix_magic_matches_recovered_loader'] = len(raw) >= 8 and struct.unpack_from('<Q', raw)[0] == 8881892042087324221
    for method, mode in [('zlib', 15), ('gzip', 31), ('raw_deflate', -15)]:
        try:
            decoder = zlib.decompressobj(mode)
            clear = decoder.decompress(raw, 4 * 1024 * 1024 + 1)
            if not decoder.eof or decoder.unused_data or decoder.unconsumed_tail or len(clear) > 4 * 1024 * 1024:
                continue
        except zlib.error:
            continue
        row = {'format': method, 'decoded_bytes': len(clear)}
        try:
            fields = wire_fields(clear)
            row['protobuf_top_level_field_numbers'] = sorted({n for n, k, v in fields})
            row['length_delimited_entries'] = sum(k == 2 for n, k, v in fields)
        except ValueError:
            row['protobuf_wire_parseable'] = False
        result.setdefault('whole_stream_decompression', []).append(row)
    return result


if __name__ == '__main__':
    rows = []
    with zipfile.ZipFile('private-input/device.zip') as device, zipfile.ZipFile('inputs/_Decrypted.ipa') as ipa:
        for index, entry in enumerate((i for i in device.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1):
            name = pathlib.PurePosixPath(entry.filename).name
            if name not in NAMES:
                continue
            raw = device.read(entry)
            bundled = [n for n in ipa.namelist() if '/Data/Raw/' in n and pathlib.PurePosixPath(n).name == name]
            comparisons = []
            for match in bundled:
                original = ipa.read(match)
                comparisons.append({'bytes': len(original), 'sha256': hashlib.sha256(original).hexdigest(), 'byte_identical': raw == original})
            row = {'entry': index, 'known_static_basename': name, **facts(raw), 'bundled_comparisons': comparisons}
            if name == 'MJGameBaseConfig.data' and len(bundled) == 1:
                source = pathlib.Path('report/device/record-evidence/MJGameBaseConfigDetail.cs').read_text()
                row['base_config_comparison'] = compare_base_configs(raw, ipa.read(bundled[0]), source)
                parsed_rows = base_config_rows(raw)
                row['runtime_config_observations'] = [
                    {'entry': log_index, 'observations': runtime_config_matches(device.read(log_info).decode('utf-8', errors='replace').splitlines(), parsed_rows, source)}
                    for log_index, log_info in enumerate((i for i in device.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1)
                    if '/Log398/' in log_info.filename and log_info.filename.endswith('.log')]
            rows.append(row)
    result = {'files': rows, 'limits': ['Only five known static code/config basenames are inventoried.', 'Wire parseability does not establish a schema or runtime use.', 'No arbitrary strings, field values, player identifiers or raw patch bytes are emitted.']}
    pathlib.Path('report/device/complete-config-facts.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
