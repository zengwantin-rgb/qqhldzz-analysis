"""Index JSON structures using source-derived keys; never publish values or raw keys."""
import collections
import json
import pathlib
import re
import zipfile

MARKERS = ('Recv Deal Self mjValue:', 'HandleSelfRedraw', 'HandleOtherRedraw',
           'HandleWait', 'CheckAllMJ', 'RevcDealCommand', 'RecvGameMsg', 'Handle TLV')
MAX_ARRAY = 4096
MAX_DEPTH = 24
MAX_STARTS = 128
NUMBER_SEQUENCE = re.compile(r'(?<![\w.])-?\d+(?:(?:[ \t]*[,;][ \t]*|[ \t]+)-?\d+){2,}(?![\w.])')


def numeric_array_facts(value):
    if not isinstance(value, list) or not all(type(n) in (int, float) for n in value):
        return None
    return {'length': len(value), 'zero_entries': sum(n == 0 for n in value),
            'positive_entries': sum(n > 0 for n in value),
            'negative_entries': sum(n < 0 for n in value)}


def index_text(text, catalog):
    canonical = {key.lower(): key for key in sorted(catalog)}
    stats = collections.defaultdict(lambda: {'occurrences': 0, 'kinds': collections.Counter(),
                                            'array_lengths': collections.Counter(),
                                            'numeric_array_shapes': collections.Counter(), 'locations': []})
    counters = collections.Counter()
    arrays = []
    root_shapes = collections.Counter()
    all_numeric_arrays = collections.defaultdict(lambda: {'shapes': collections.Counter(), 'locations': []})
    text_sequences = collections.defaultdict(lambda: {'occurrences': 0, 'first_lines': []})

    def shape_key(facts):
        return tuple(facts[k] for k in ('length', 'zero_entries', 'positive_entries', 'negative_entries'))

    def shape_rows(counter):
        return [dict(zip(('length', 'zero_entries', 'positive_entries', 'negative_entries'), shape), occurrences=count)
                for shape, count in sorted(counter.items())]

    def walk(value, path, line_number, context, depth=0):
        if depth > MAX_DEPTH:
            counters['depth_limit_hits'] += 1
            return
        array_facts = numeric_array_facts(value)
        if array_facts is not None:
            row = all_numeric_arrays[path]
            row['shapes'][shape_key(array_facts)] += 1
            if len(row['locations']) < 20:
                row['locations'].append({'line': line_number, 'context': context})
        if isinstance(value, dict):
            for key, child in value.items():
                name = canonical.get(key.lower())
                child_path = path + '/' + (name if name else '$unknown')
                if name:
                    row = stats[child_path]
                    row['occurrences'] += 1
                    row['kinds'][type(child).__name__] += 1
                    if isinstance(child, list):
                        row['array_lengths'][str(len(child))] += 1
                    facts = numeric_array_facts(child)
                    if facts is not None:
                        row['numeric_array_shapes'][shape_key(facts)] += 1
                    if len(row['locations']) < 20:
                        location = {'line': line_number, 'context': context}
                        if facts is not None:
                            location['numeric_array'] = facts
                        row['locations'].append(location)
                else:
                    counters['unknown_key_occurrences'] += 1
                walk(child, child_path, line_number, context, depth + 1)
        elif isinstance(value, list):
            if len(value) > MAX_ARRAY:
                counters['array_truncations'] += 1
            for child in value[:MAX_ARRAY]:
                walk(child, path + '/*', line_number, context, depth + 1)
        elif isinstance(value, str) and len(value) <= 256 * 1024 and value.lstrip().startswith(('{', '[')):
            try:
                embedded = json.loads(value)
            except (ValueError, RecursionError):
                return
            counters['embedded_json_strings'] += 1
            walk(embedded, path + '/$json', line_number, context, depth + 1)

    decoder = json.JSONDecoder()
    for line_number, line in enumerate(text.splitlines(), 1):
        context = next((m for m in MARKERS if m in line), 'unclassified')
        position = 0
        attempts = 0
        found = False
        json_spans = []
        while position < len(line):
            starts = [p for p in (line.find('{', position), line.find('[', position)) if p >= 0]
            if not starts:
                break
            if attempts >= MAX_STARTS:
                counters['line_start_limit_hits'] += 1
                break
            start = min(starts)
            attempts += 1
            counters['decode_attempts'] += 1
            try:
                value, end = decoder.raw_decode(line, start)
            except (ValueError, RecursionError):
                position = start + 1
                continue
            found = True
            json_spans.append((start, end))
            counters['json_documents'] += 1
            facts = numeric_array_facts(value)
            if facts is not None:
                counters['root_numeric_arrays'] += 1
                root_shapes[shape_key(facts)] += 1
                if len(arrays) < 200:
                    arrays.append({'line': line_number, 'context': context, **facts})
                else:
                    counters['root_array_sample_truncations'] += 1
            walk(value, '$', line_number, context)
            position = end
        if attempts and not found:
            counters['lines_with_starts_but_no_json'] += 1
        for match in NUMBER_SEQUENCE.finditer(line):
            if any(start <= match.start() and match.end() <= end for start, end in json_spans):
                continue
            values = [int(n) for n in re.findall(r'-?\d+', match.group())]
            if not 3 <= len(values) <= 512 or not all(-1 <= n <= 255 for n in values):
                continue
            counters['non_json_numeric_sequences'] += 1
            facts = numeric_array_facts(values)
            key = (context, *shape_key(facts))
            row = text_sequences[key]
            row['occurrences'] += 1
            if len(row['first_lines']) < 20:
                row['first_lines'].append(line_number)
    return {'counters': dict(counters),
            'recognized_fields': [{'path': key, **row, 'numeric_array_shapes': shape_rows(row['numeric_array_shapes'])}
                                  for key, row in sorted(stats.items())],
            'root_numeric_array_shapes': shape_rows(root_shapes),
            'all_numeric_array_paths': [{'path': path, 'shapes': shape_rows(row['shapes']), 'locations': row['locations']}
                                       for path, row in sorted(all_numeric_arrays.items())],
            'non_json_numeric_sequences': [dict(zip(('context', 'length', 'zero_entries', 'positive_entries', 'negative_entries'), key), **row)
                                           for key, row in sorted(text_sequences.items())],
            'root_numeric_array_samples': arrays}


def inspect(device_path, catalog):
    files = []
    with zipfile.ZipFile(device_path) as archive:
        for index, entry in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1):
            if entry.file_size > 20 * 1024 * 1024:
                files.append({'entry': index, 'skipped': 'size_limit'})
                continue
            raw = archive.read(entry)
            text = raw.decode('utf-8', errors='replace')
            if b'\0' in raw or text.count('\ufffd') > max(3, len(raw) // 1000):
                files.append({'entry': index, 'skipped': 'binary_not_json_decoded'})
                continue
            files.append({'entry': index, **index_text(text, catalog)})
    return {'catalog_field_count': len(catalog), 'files': files, 'limits': [
        'Published paths contain only recovered code field/property names; other keys become $unknown.',
        'No scalar values, actual array values, raw log prefixes, file paths or account identifiers are emitted.',
        'JSON in log text can be telemetry or user content; a matching key is not proof of gameplay provenance.',
        'All text entries are scanned; binary input is not interpreted as JSON.',
        'Depth, array length, candidate-start and sample limits are counted explicitly.',
        'Numeric array sign and length summaries do not identify valid tiles, owners or live opponent hands.',
        'Non-JSON integer sequences use comma, semicolon or whitespace separators and can also represent non-gameplay data.',
    ]}


if __name__ == '__main__':
    catalog = json.loads(pathlib.Path('report/device/gameplay-field-catalog.json').read_text())['fields']
    result = inspect('private-input/device.zip', catalog)
    pathlib.Path('report/device/json-gameplay-index.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({'catalog_field_count': result['catalog_field_count'],
                      'files': [{'entry': f['entry'], 'counters': f.get('counters'),
                                 'recognized_paths': len(f.get('recognized_fields', [])),
                                 'skipped': f.get('skipped')} for f in result['files']]}, indent=2))
