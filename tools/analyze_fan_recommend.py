"""Check documented recommendation fields against known self checkpoints; no values emitted."""
import collections
import json
import pathlib
import re
import zipfile

DETAIL = re.compile(r'\bCurHandTile\s+([0-9,;\s-]*)\s+CurHandTag\s+([0-9,;\s-]*)\s*】')
LABELS = ('SetNewFanRecommendDetailData', 'Set Active FanRecommendDeatil')


def numbers(text):
    tokens = re.findall(r'-?\d+', text)
    if len(tokens) > 512 or any(len(n) > 12 for n in tokens):
        raise ValueError('Numeric list limit')
    return [int(n) for n in tokens]


def first_object(line):
    decoder = json.JSONDecoder()
    for match in re.finditer(r'\{', line):
        try:
            value, _ = decoder.raw_decode(line, match.start())
        except (ValueError, RecursionError):
            continue
        if isinstance(value, dict):
            return value
    return None


def analyze(lines):
    initial = None
    first_draw = None
    private_rows = []
    failed_lines = []
    for number, line in enumerate(lines, 1):
        if initial is None and 'Recv Deal Self mjValue:' in line:
            text = line.split('Recv Deal Self mjValue:', 1)[1]
            match = re.match(r'\s*[\[({]?\s*([0-9;, \-]+)', text)
            if match:
                values = numbers(match.group(1))
                if len(values) == 13 and all(0 <= n <= 255 for n in values):
                    initial = (number, values)
        if initial and first_draw is None and 'HandleSelfRedraw' in line:
            obj = first_object(line)
            if obj and type(obj.get('chTile')) is int and 0 < obj['chTile'] <= 255:
                draws = obj.get('DrawTiles')
                if isinstance(draws, list) and draws == [obj['chTile']]:
                    first_draw = (number, initial[1] + draws)
        label = next((s for s in LABELS if s in line), None)
        if label is None:
            continue
        details = []
        for match in DETAIL.finditer(line):
            hand, tags = numbers(match.group(1)), numbers(match.group(2))
            if all(-1 <= n <= 255 for n in hand):
                details.append((hand, tags))
        if not details:
            failed_lines.append(number)
        private_rows.append((number, label, details))
    published = []
    for number, label, details in private_rows:
        multisets = {tuple(sorted(collections.Counter(hand).items())) for hand, tags in details}
        published.append({'line': number, 'kind': label, 'detail_count': len(details),
                          'hand_lengths': dict(collections.Counter(str(len(hand)) for hand, tags in details)),
                          'tag_lengths': dict(collections.Counter(str(len(tags)) for hand, tags in details)),
                          'distinct_hand_multisets': len(multisets)})
    checkpoints = []
    for label, baseline, end in (
        ('initial_self_hand', initial, first_draw[0] if first_draw else len(lines) + 1),
        ('first_self_draw_hand', first_draw, len(lines) + 1),
    ):
        if baseline is None:
            continue
        row = next((r for r in private_rows if baseline[0] < r[0] < end and r[2]), None)
        if row is None:
            checkpoints.append({'baseline': label, 'baseline_line': baseline[0], 'comparison_available': False})
            continue
        expected = collections.Counter(baseline[1])
        same_length = [hand for hand, tags in row[2] if len(hand) == len(baseline[1])]
        exact = sum(collections.Counter(hand) == expected for hand in same_length)
        checkpoints.append({'baseline': label, 'baseline_line': baseline[0], 'baseline_count': len(baseline[1]),
                            'recommendation_line': row[0], 'comparison_available': True,
                            'detail_count': len(row[2]), 'same_length_entries': len(same_length),
                            'exact_self_multiset_matches': exact, 'same_length_nonmatches': len(same_length) - exact})
    return {'recommendation_log_lines': len(private_rows), 'detail_entries': sum(len(r[2]) for r in private_rows),
            'unparsed_recommendation_lines': failed_lines, 'messages': published, 'self_checkpoints': checkpoints,
            'limits': [
                'Fields are parsed using the recovered FanRecommendDetail.ToString format.',
                'Only the first recommendation after each known self checkpoint is compared; later hand evolution is not reconstructed.',
                'Repeated recommendations and candidate patterns are not independent player hands.',
                'Equality proves matching tile multisets at those checkpoints, not absence of all other unlogged data.',
                'No hand values, tags, fan names, account IDs or raw lines are emitted.',
            ]}


if __name__ == '__main__':
    files = []
    with zipfile.ZipFile('private-input/device.zip') as archive:
        for index, entry in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1):
            raw = archive.read(entry)
            if b'\0' in raw:
                continue
            result = analyze(raw.decode('utf-8', errors='replace').splitlines())
            files.append({'entry': index, **result})
    result = {'files': files}
    pathlib.Path('report/device/fan-recommend-check.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({'files': [{'entry': f['entry'], 'recommendation_log_lines': f['recommendation_log_lines'],
                               'detail_entries': f['detail_entries'], 'unparsed_recommendation_lines': f['unparsed_recommendation_lines'],
                               'self_checkpoints': f['self_checkpoints']} for f in files]}, indent=2))
