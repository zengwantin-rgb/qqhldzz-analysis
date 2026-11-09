"""Compare manifest basenames to supplied app binaries, without exposing paths."""
import collections
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
    return {'candidates': [{k: v for k, v in c.items() if not k.startswith('_')} for c in candidates],
            'small_record_file_checks': small_files, 'binaries_scanned': scanned, 'references': refs,
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
