"""Count explicit final-hand logs and their timing without publishing hand values."""
import collections
import json
import pathlib
import re
import zipfile

from analyze_fan_recommend import first_object


def analyze(lines, placeholder):
    phase = 'before_deal'
    rows = []
    self_seats = collections.Counter()
    markers = 0
    unparsed = []
    for number, line in enumerate(lines, 1):
        event = re.search(r'RecvGameMsg msgType:(ID_SO_[A-Z_0-9]+)', line)
        if event:
            kind = event.group(1)
            if kind == 'ID_SO_NOTIFY_DEAL':
                phase = 'live'
            elif kind == 'ID_SO_NOTIFY_HU':
                phase = 'hu_notification'
            elif kind in ('ID_SO_NOTIFY_FINISH', 'ID_SO_NOTIFY_FINISH_V2', 'ID_SO_NOTIFY_FINISH_V3'):
                phase = 'settled'
        if 'HandleSelfRedraw' in line:
            obj = first_object(line)
            seat = obj.get('nSeatID', obj.get('nSeatId')) if obj else None
            if type(seat) is int and 0 <= seat <= 3:
                self_seats[seat] += 1
        if 'Balance UserHand Tiles seat:' not in line:
            continue
        markers += 1
        match = re.search(r'Balance UserHand Tiles seat:\s*([0-3])\s+tiles:\s*([^\r\n]*)', line)
        if not match:
            unparsed.append(number)
            continue
        text = match.group(2).strip()
        if not re.fullmatch(r'(?:-?\d{1,3}(?:\s*,\s*-?\d{1,3})*)?', text):
            unparsed.append(number)
            continue
        tiles = [int(n) for n in re.findall(r'-?\d+', text)]
        if len(tiles) > 144 or not all(-128 <= n <= 127 for n in tiles):
            unparsed.append(number)
            continue
        rows.append({'line': number, 'phase': phase, 'seat': int(match.group(1)),
                     'tile_count': len(tiles), 'placeholder_entries': sum(n == placeholder for n in tiles),
                     'positive_entries': sum(n > 0 for n in tiles)})
    self_seat = next(iter(self_seats)) if len(self_seats) == 1 else None
    for row in rows:
        row['is_self'] = row['seat'] == self_seat if self_seat is not None else None
    return {'self_seat_observations': dict(self_seats), 'unambiguous_self_seat': self_seat,
            'final_hand_markers': markers, 'unparsed_final_hand_lines': unparsed,
            'pre_hu_final_hand_entries': sum(r['phase'] in ('before_deal', 'live') for r in rows),
            'final_hands': rows,
            'limits': [
                'Seat identity is inferred only when all parsed HandleSelfRedraw nSeatID observations agree.',
                'Final-hand logging is inspected independently of whether a finish notification has arrived.',
                'Post-Hu final hands cannot establish visibility during ordinary live play.',
                'Counts omit actual tile values and do not include melds absent from this field.',
            ]}


if __name__ == '__main__':
    placeholder = json.loads(pathlib.Path('report/device/round-analysis.json').read_text())['placeholder_value_from_MJDef']
    if type(placeholder) is not int:
        raise ValueError('Placeholder definition unavailable')
    files = []
    with zipfile.ZipFile('private-input/device.zip') as archive:
        for index, entry in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1):
            raw = archive.read(entry)
            if b'\0' not in raw:
                files.append({'entry': index, **analyze(raw.decode('utf-8', errors='replace').splitlines(), placeholder)})
    result = {'placeholder': placeholder, 'files': files}
    pathlib.Path('report/device/finish-hand-check.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
