"""Publish sampled log context only when it matches bytes in supplied code."""
import collections
import json
import pathlib
import re
import zipfile


def fragments(line):
    body = line.split('|', 4)[-1][:500]
    choices = re.findall(r'[A-Za-z_][A-Za-z_ .:-]{7,99}|[\u4e00-\u9fff]{4,40}', body)
    choices += re.findall(r'[A-Za-z_]{8,80}', body)
    return {s.strip(' .:-') for s in choices if len(s.strip(' .:-')) >= 8 and '.cpp' not in s}


def matches(data, choices, label):
    result = {}
    for fragment in choices:
        for encoding in ('utf-8', 'utf-16-le'):
            offset = data.find(fragment.encode(encoding))
            if offset >= 0:
                result.setdefault(fragment, []).append({'binary': label, 'encoding': encoding, 'offset': offset})
    return result


def inspect(device_path, ipa_path, assemblies, index_path):
    index = json.loads(pathlib.Path(index_path).read_text())
    sampled = {}
    with zipfile.ZipFile(device_path) as device:
        entries = [i for i in device.infolist() if not i.is_dir() and i.filename != 'manifest.json']
        for item in index['files']:
            locations = {line for row in item.get('non_json_numeric_sequences', []) if row['length'] >= 10
                         for line in row['first_lines']}
            if not locations:
                continue
            lines = device.read(entries[item['entry'] - 1]).decode('utf-8', errors='replace').splitlines()
            for number in sorted(locations):
                sampled[(item['entry'], number)] = fragments(lines[number - 1])
    choices = set().union(*sampled.values()) if sampled else set()
    references = collections.defaultdict(list)
    scanned = []

    def scan(data, label):
        scanned.append(label)
        for fragment, refs in matches(data, choices, label).items():
            references[fragment].extend(refs)

    with zipfile.ZipFile(ipa_path) as ipa:
        for entry in ipa.infolist():
            if entry.filename.endswith('/UnityFramework.framework/UnityFramework') or entry.filename.endswith('/global-metadata.dat'):
                scan(ipa.read(entry), pathlib.PurePosixPath(entry.filename).name)
    for file in sorted(pathlib.Path(assemblies).glob('*.dll')):
        scan(file.read_bytes(), file.name)
    return {'sampled_lines': len(sampled), 'candidate_fragment_count': len(choices),
            'scanned_sources': scanned,
            'contexts': [{'entry': entry, 'line': number,
                          'source_confirmed_fragments': [
                              {'fragment': fragment, 'references': references[fragment]}
                              for fragment in sorted(values) if fragment in references]}
                         for (entry, number), values in sorted(sampled.items())],
            'limits': [
                'Only fragments also found in supplied binary/metadata/assembly bytes are published.',
                'Matching a substring does not prove a caller or ownership of any tile list.',
                'Only sampled non-JSON sequence lines of length at least ten are traced.',
                'Unmatched text, numbers, paths and actual log payloads are not emitted.',
            ]}


if __name__ == '__main__':
    result = inspect('private-input/device.zip', 'inputs/_Decrypted.ipa',
                     'recovered-input/recovered/assemblies', 'report/device/json-gameplay-index.json')
    pathlib.Path('report/device/sequence-source-context.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({'sampled_lines': result['sampled_lines'], 'candidate_fragment_count': result['candidate_fragment_count']}, indent=2))
