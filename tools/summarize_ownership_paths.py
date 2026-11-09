"""Cross-reference static paths with already published round observations."""
import json
import pathlib

root = pathlib.Path('report/ownership')
paths = json.loads((root / 'ownership-paths.json').read_text())
round_result = json.loads(pathlib.Path('reports/device-round/round-analysis.json').read_text())
types = {row['type'] for row in paths['schemas']}
required = {
    'HuanLeGameSo.TNewRecommendFanTarget', 'HuanLeGameSo.TOneNewRecommendFanTarget',
    'HuanLeGameSo.TPlayerTotalHandAbilityTileInfo', 'HuanLeGameSo.TPlayerHandAbilityTileInfo',
}
if not required <= types:
    raise ValueError('Required recovered schemas missing')
rows = []
for enum in paths['enums']:
    rows.append({**enum, 'recorded_mentions_by_phase': {
        phase: counts.get(enum['name'], 0)
        for phase, counts in round_result['tlv_mentions_by_phase'].items()
    }})
summary = {
    'schema_count': len(paths['schemas']), 'direct_consumer_methods': len(paths['consumers']),
    'recorded_tlv_mentions': rows,
    'seat_fields': [{
        'type': row['type'],
        'members': sorted({member['name'] for member in row['fields'] + row['properties']
                           if 'seat' in member['name'].lower() or 'chair' in member['name'].lower()}),
    } for row in paths['schemas']],
    'limits': [
        'Round observations are reused from the already validated published report; no new device sample was collected.',
        'TLV mention counts may include repeated logging and are not unique network packet counts.',
        'Zero mentions do not establish that unlogged messages or fields were absent.',
        'Seat fields alone do not establish whether tile entries represent actual concealed tiles, ability reveals or placeholders.',
    ],
}
(root / 'ownership-summary.json').write_text(json.dumps(summary, indent=2) + '\n')
print(json.dumps(summary, indent=2))
