"""Inspect only safe shapes around previously unparsed or hand-TLV markers."""
import json
import collections
import pathlib
import re
import zipfile
from analyze_round_logs import payload
from index_json_gameplay import index_text
from analyze_fan_recommend import DETAIL, numbers

MARKERS = ('EM_UP_TLV_HAND_TILES','nHandTilesNum')
CODE_MARKERS = ('RevcDealCommand','RevcWaitNotifyCommand','RevcGiveNotifyCommand',
                'CNotifyDeal','HandleWait','Handle TLV','HandleSelfRedraw',
                'GetBaseConifgByGameID','InitVM','CreateVirtualMachine','OnRecvCSMsg',
                'stHandTiles','nHandTilesNum','EM_UP_TLV_HAND_TILES','ExtraTiles')


def analyze(lines, catalog):
    rows = []
    for offset, line in enumerate(lines):
        reasons = [marker for marker in MARKERS if marker in line]
        unparsed = 'HandleWait WaitInfo.chFlag:' in line and payload(line) is None
        if unparsed:
            reasons.append('unparsed_wait_marker')
        if not reasons:
            continue
        start, end = max(0,offset-3), min(len(lines),offset+4)
        context = '\n'.join(lines[start:end])
        row = {'line':offset+1,'reasons':reasons,'context_first_line':start+1,
               'context_last_line':end,'marker_line_characters':len(line),
               'marker_line_has_json_start':'{' in line,
               'marker_line_has_parsed_object':payload(line) is not None,
               'known_code_marker_counts':{marker:context.count(marker) for marker in CODE_MARKERS if marker in context},
               'hand_count_literals':[int(value) for value in re.findall(r'\bnHandTilesNum[\s"\x27]*[:=]\s*(\d{1,3})\b',line) if int(value)<=144],
               'context_structure':index_text(context,catalog)}
        rows.append(row)
    initial = None
    self_seats = collections.Counter()
    checks = []
    counts = []
    last_dispatch = None
    last_tlv = None
    def tile_list(text):
        match = re.match(r'\s*[\[({]?\s*([0-9,;\s-]+)',text)
        if not match:
            return None
        values = numbers(match.group(1))
        return values if 0 < len(values) <= 144 and all(-1 <= n <= 255 for n in values) else None
    for index, line in enumerate(lines,1):
        if initial is None and 'Recv Deal Self mjValue:' in line:
            values = tile_list(line.split('Recv Deal Self mjValue:',1)[1])
            if values is not None:
                initial = (index, values)
        if 'HandleSelfRedraw' in line:
            obj = payload(line)
            seat = obj.get('nSeatID',obj.get('nSeatId')) if obj else None
            if type(seat) is int and 0 <= seat <= 3:
                self_seats[seat] += 1
        dispatch = re.search(r'RecvGameMsg msgType:(ID_SO_[A-Z_0-9]+)',line)
        if dispatch:
            last_dispatch = dispatch.group(1)
        match = re.search(r'Replay, EM_UP_TLV_HAND_TILES nSeat =\s*([0-3])\s+tiles:\s*(.*)',line)
        if match:
            values = tile_list(match.group(2))
            if values is not None:
                last_tlv = (index, int(match.group(1)), values)
                checks.append({'line':index,'kind':'explicit_replay_hand_tlv','seat':int(match.group(1)),
                               'tile_count':len(values),'source_dispatch':last_dispatch,
                               'initial_self_hand_line':initial[0] if initial else None,
                               'matches_initial_self_multiset':collections.Counter(values)==collections.Counter(initial[1]) if initial else None})
        if 'SetPlayerHandTile info:' in line:
            values = tile_list(line.split('SetPlayerHandTile info:',1)[1])
            if values is not None:
                near = last_tlv if last_tlv and 0 < index-last_tlv[0] <= 5 else None
                checks.append({'line':index,'kind':'self_hand_restore_log','tile_count':len(values),
                               'nearby_explicit_tlv_line':near[0] if near else None,
                               'matches_nearby_explicit_tlv':collections.Counter(values)==collections.Counter(near[2]) if near else None,
                               'matches_initial_self_multiset':collections.Counter(values)==collections.Counter(initial[1]) if initial else None})
        match = re.search(r'SetPlayerHandTile nHandTilesNum\s+(\d{1,3})\s+inHandListCount\s+(\d{1,3})',line)
        if match and all(int(n)<=144 for n in match.groups()):
            counts.append({'line':index,'declared_count':int(match.group(1)),'existing_hand_count':int(match.group(2))})
    self_seat = next(iter(self_seats)) if len(self_seats)==1 else None
    for row in checks:
        if 'seat' in row:
            row['agrees_with_self_draw_seat'] = row['seat']==self_seat if self_seat is not None else None
    return {'marker_contexts':rows,'self_draw_seat_observations':dict(self_seats),
            'hand_tlv_payload_checks':checks,'self_hand_restore_counts':counts}


if __name__ == '__main__':
    catalog=json.loads(pathlib.Path('reports/device-round/gameplay-field-catalog.json').read_text())['fields']
    files=[]
    with zipfile.ZipFile('private-input/device.zip') as archive:
        for entry, info in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename!='manifest.json'),1):
            if '/Log398/' not in info.filename or not info.filename.endswith('.log'):
                continue
            files.append({'entry':entry,**analyze(archive.read(info).decode('utf-8',errors='replace').splitlines(),catalog)})
    pathlib.Path('report/device/round-anomalies.json').write_text(json.dumps({'files':files,'limits':[
        'Only known source markers, counts and source-catalog JSON shapes are published; raw context and scalar tile values are omitted.',
        'A TLV marker does not establish a recoverable payload or its ownership.',
        'JSON locations in each context_structure are relative to context_first_line.'
    ]},indent=2)+'\n')
