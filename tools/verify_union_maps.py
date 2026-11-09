"""Ensure committed runtime selectors exactly match original protocol IL metadata."""
import json,pathlib,re
source=pathlib.Path('mobile/MessageObserver/ProtocolUnions.cs').read_text()
for union in json.loads(pathlib.Path('report/device/union-selectors.json').read_text()):
    block=re.search(r'if \(type == "'+re.escape(union['type'])+r'"\)(.*?)(?=\n            if|\n            return null)',source,re.S).group(1)
    parsed=dict(re.findall(r'case (\d+): return "([A-Za-z0-9_]+)";',block))
    assert parsed==union['members'],union['type']
assert 'case 1: return "stNotifyDeal"' in source and 'case 8: return "stTableInfo"' in source
