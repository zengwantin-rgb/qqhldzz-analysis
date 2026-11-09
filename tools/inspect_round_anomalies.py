"""Inspect only safe shapes around previously unparsed or hand-TLV markers."""
import json
import pathlib
import re
import zipfile
from analyze_round_logs import payload
from index_json_gameplay import index_text

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
    return {'marker_contexts':rows}


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
