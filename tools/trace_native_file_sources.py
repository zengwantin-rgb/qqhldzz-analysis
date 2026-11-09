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


def binary_matches(data, candidates, binary_label):
    rows = []
    for candidate in candidates:
        name = candidate['_basename']
        if not name:
            continue
        for encoding in ('utf-8', 'utf-16-le'):
            needle = name.encode(encoding)
            offsets = []
            start = 0
            while True:
                offset = data.find(needle, start)
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
                    row['matched_static_basename'] = name
                rows.append(row)
        # Names generated with IDs may only occur as a printf-style template.
        # Suffix matches remain a separate, weaker result than exact matches.
        suffix_match = re.search(r'([A-Za-z_][A-Za-z_.-]*\.[A-Za-z]+)$', name)
        if suffix_match and suffix_match.group(1) != name and 'record' in suffix_match.group(1).lower():
            suffix = suffix_match.group(1).encode('ascii')
            start = 0
            while True:
                offset = data.find(suffix, start)
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
        for name in sorted(names):
            if name.endswith('/config/VedioRecordConfig.data'):
                raw = ipa.read(name)
                packaged_records.append({'static_basename': 'VedioRecordConfig.data',
                                         'bytes': len(raw),
                                         'source': 'packaged_config',
                                         **small_binary_facts(raw)})
    return {'candidates': [{k: v for k, v in c.items() if not k.startswith('_')} for c in candidates],
            'small_record_file_checks': small_files, 'packaged_record_configs': packaged_records,
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
