"""Compare manifest basenames to supplied app binaries, without exposing paths."""
import json
import pathlib
import plistlib
import re
import zipfile
import zlib

from audit_export_coverage import path_facts


def small_binary_facts(raw):
    facts = {'bytes': len(raw), 'zero_bytes': raw.count(b'\0'),
             'whole_stream_compression_matches': []}
    for label, mode in [('zlib', 15), ('gzip', 31), ('raw_deflate', -15)]:
        try:
            decoder = zlib.decompressobj(mode)
            clear = decoder.decompress(raw, 65537)
            if decoder.eof and not decoder.unconsumed_tail and not decoder.unused_data and len(clear) <= 65536:
                facts['whole_stream_compression_matches'].append({'format': label, 'decoded_bytes': len(clear)})
        except zlib.error:
            pass
    return facts


def wire_fields(raw):
    def varint(pos):
        value = 0
        for shift in range(0, 70, 7):
            if pos >= len(raw):
                raise ValueError('Truncated protobuf varint')
            byte = raw[pos]
            pos += 1
            if shift == 63 and byte > 1:
                raise ValueError('Protobuf varint exceeds 64 bits')
            value |= (byte & 127) << shift
            if byte < 128:
                return value, pos
        raise ValueError('Invalid protobuf varint')
    pos = 0
    rows = []
    while pos < len(raw):
        tag, pos = varint(pos)
        number, kind = tag >> 3, tag & 7
        if not 0 < number < 2**29:
            raise ValueError('Invalid protobuf field number')
        if kind == 0:
            value, pos = varint(pos)
        else:
            if kind == 2:
                length, pos = varint(pos)
            elif kind in (1, 5):
                length = 8 if kind == 1 else 4
            else:
                raise ValueError('Unsupported protobuf wire type')
            if pos + length > len(raw):
                raise ValueError('Truncated protobuf value')
            value = raw[pos:pos + length]
            pos += length
        rows.append((number, kind, value))
    return rows


def config_shape(clear, number_to_name):
    outer = wire_fields(clear)
    rows = [wire_fields(value) for number, kind, value in outer if number == 1 and kind == 2]
    return {'decoded_bytes': len(clear), 'row_count': len(rows),
            'unknown_top_level_fields': sum(number != 1 or kind != 2 for number, kind, value in outer),
            'known_row_fields': sorted({number_to_name[number] for row in rows for number, kind, value in row if number in number_to_name}),
            'unknown_row_field_count': sum(number not in number_to_name for row in rows for number, kind, value in row),
            'row_wire_type_mismatch_count': sum(
                kind != (2 if number_to_name[number] == 'name' else 0)
                for row in rows for number, kind, value in row if number in number_to_name),
            'note': 'Only schema and row counts are emitted; no record values are included.'}


def binary_matches(data, candidates, binary_label):
    rows = []
    folded = data.lower()
    for candidate in candidates:
        name = candidate['_basename']
        if not name:
            continue
        for encoding in ('utf-8', 'utf-16-le'):
            needle = name.lower().encode(encoding)
            offsets = []
            start = 0
            while True:
                offset = folded.find(needle, start)
                if offset < 0:
                    break
                offsets.append(offset)
                start = offset + len(needle)
            if offsets:
                row = {'candidate': candidate['candidate'], 'binary': binary_label,
                       'encoding': encoding, 'match_count': len(offsets), 'first_offsets': offsets[:8]}
                # A matched alphabetic basename is now confirmed as an app-static string.
                # No device directory or arbitrary surrounding binary string is emitted.
                if re.fullmatch(r'[A-Za-z_.-]{1,80}', name):
                    row['matched_static_basename'] = data[offsets[0]:offsets[0] + len(needle)].decode(encoding)
                rows.append(row)
        # Names generated with IDs may only occur as a printf-style template.
        # Suffix matches remain a separate, weaker result than exact matches.
        suffix_match = re.search(r'(record[A-Za-z_.-]*\.(?:dat|data|bin))$', name, re.I)
        if suffix_match and suffix_match.group(1).lower() != name.lower():
            suffix = suffix_match.group(1).lower().encode('ascii')
            start = 0
            while True:
                offset = folded.find(suffix, start)
                if offset < 0:
                    break
                start = offset + len(suffix)
                left = data.rfind(b'\0', max(0, offset - 160), offset)
                right = data.find(b'\0', offset, offset + 160)
                if left < 0 or right < 0:
                    continue
                literal = data[left + 1:right]
                if not re.fullmatch(rb'[A-Za-z0-9_./%+\-]{4,160}', literal):
                    continue
                basename = pathlib.PurePosixPath(literal.decode('ascii')).name
                rows.append({'candidate': candidate['candidate'], 'binary': binary_label,
                             'reference_kind': 'suffix_only', 'offset': offset,
                             'static_basename_template': basename})
    return rows


def inspect(device_path, ipa_path):
    candidates = []
    small_files = []
    with zipfile.ZipFile(device_path) as device:
        manifest = json.loads(device.read('manifest.json'))
        exported = {i.filename: i for i in device.infolist() if not i.is_dir() and i.filename != 'manifest.json'}
        for scan in manifest.get('scans', []):
            for item in scan.get('files', []):
                relative = item.get('relative', '')
                matches = [i for n, i in exported.items() if n.endswith('/' + relative)]
                row = {'candidate': len(candidates) + 1, **path_facts(relative),
                       'bytes': item.get('size'), 'exported': bool(matches),
                       '_basename': pathlib.PurePosixPath(relative).name}
                candidates.append(row)
                if len(matches) == 1 and matches[0].file_size <= 4096 and row['category'] == 'replay_or_record':
                    small_files.append({'candidate': row['candidate'], **small_binary_facts(device.read(matches[0]))})
    refs = []
    scanned = []
    with zipfile.ZipFile(ipa_path) as ipa:
        names = set(ipa.namelist())
        targets = set()
        for name in names:
            if name.startswith('Payload/') and (name.endswith('.app/Info.plist') or name.endswith('.framework/Info.plist')):
                info = plistlib.loads(ipa.read(name))
                executable = info.get('CFBundleExecutable')
                if isinstance(executable, str):
                    target = str(pathlib.PurePosixPath(name).parent / executable)
                    if target in names:
                        targets.add(target)
            if name.endswith('.dylib') or name.endswith('/global-metadata.dat'):
                targets.add(name)
        for name in sorted(targets):
            data = ipa.read(name)
            label = pathlib.PurePosixPath(name).name
            scanned.append({'binary': label, 'bytes': len(data)})
            refs.extend(binary_matches(data, candidates, label))
        packaged_records = []
        packaged_comparisons = []
        for candidate in candidates:
            if candidate['category'] not in ('configuration_or_data', 'patch', 'replay_or_record'):
                continue
            matches = [n for n in names if '/Data/Raw/' in n and pathlib.PurePosixPath(n).name == candidate['_basename']]
            packaged_comparisons.append({
                'candidate': candidate['candidate'],
                'manifest_bytes': candidate['bytes'],
                'packaged_matches': [
                    {'packaged_bytes': ipa.getinfo(n).file_size,
                     'same_size_as_manifest': ipa.getinfo(n).file_size == candidate['bytes']}
                    for n in sorted(matches)],
                'note': 'Equal sizes do not establish identical contents or active runtime configuration.'})
        for name in sorted(names):
            if name.endswith('/config/VedioRecordConfig.data'):
                raw = ipa.read(name)
                record = {'static_basename': 'VedioRecordConfig.data',
                                         'bytes': len(raw),
                                         'source': 'packaged_config',
                                         **small_binary_facts(raw)}
                schema_path = pathlib.Path('report/device/protocol-consumers.json')
                if schema_path.exists() and any(r['format'] == 'zlib' for r in record['whole_stream_compression_matches']):
                    schemas = json.loads(schema_path.read_text())['schemas']
                    schema = next((s for s in schemas if s['type'] == 'Configuration.VedioRecordConfig'), None)
                    if schema:
                        mapping = {f['constant']: f['name'].removesuffix('FieldNumber')
                                   for f in schema['fields'] if f['name'].endswith('FieldNumber') and isinstance(f['constant'], int)}
                        try:
                            decoder = zlib.decompressobj()
                            clear = decoder.decompress(raw, 65537)
                            if not decoder.eof or decoder.unused_data or decoder.unconsumed_tail or len(clear) > 65536:
                                raise ValueError('Config compression limit or trailing data')
                            record['protobuf_config_shape'] = config_shape(clear, mapping)
                        except (ValueError, zlib.error):
                            record['protobuf_config_shape'] = {'parse_failed': True}
                packaged_records.append(record)
    return {'candidates': [{k: v for k, v in c.items() if not k.startswith('_')} for c in candidates],
            'small_record_file_checks': small_files, 'packaged_record_configs': packaged_records,
            'packaged_candidate_comparisons': packaged_comparisons,
            'binaries_scanned': scanned, 'references': refs,
            'limits': [
                'Only exact basename byte matches in supplied app executables and IL2CPP metadata are checked.',
                'A filename match identifies a static reference, not its behavior or actual file format.',
                'No match cannot rule out encoded strings, dynamic paths or other installed runtime patches.',
                'Whole-stream compression failures do not rule out headers, encryption or custom serialization.',
                'No device paths, file contents, hand values or adjacent binary strings are emitted.',
            ]}


if __name__ == '__main__':
    result = inspect('private-input/device.zip', 'inputs/_Decrypted.ipa')
    out = pathlib.Path('report/device')
    out.mkdir(parents=True, exist_ok=True)
    (out / 'native-file-sources.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
