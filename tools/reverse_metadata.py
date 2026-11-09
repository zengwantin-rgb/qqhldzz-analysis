"""Read version-29 metadata tables; run only on GitHub Actions."""
import collections, json, pathlib, re, struct, zipfile
out = pathlib.Path('report/reverse')
out.mkdir(parents=True, exist_ok=True)
with zipfile.ZipFile('inputs/_Decrypted.ipa') as z:
    metadata_path = next(n for n in z.namelist() if n.endswith('/global-metadata.dat'))
    data = z.read(metadata_path)
    pathlib.Path('inputs/global-metadata.dat').write_bytes(data)
    assets = [{'path': i.filename, 'size': i.file_size} for i in z.infolist()
              if re.search(r'(?i)(lua|hotfix|hybrid|\.dll$|\.bytes$|\.proto$|replay|mahjong|mjgame)', i.filename)]
assert struct.unpack_from('<II', data) == (0xfab11baf, 29)
def table(index, stride):
    offset, size = struct.unpack_from('<II', data, 8+index*8)
    assert offset+size <= len(data) and size % stride == 0, (index, offset, size, stride)
    return range(offset, offset+size, stride)
string_offset, string_size = struct.unpack_from('<II', data, 24)
def name(index):
    assert 0 <= index < string_size
    start = string_offset+index
    end = data.index(b'\0', start, string_offset+string_size)
    return data[start:end].decode('utf-8', errors='replace')
fields = []
for p in table(11,12):
    n, typ, token = struct.unpack_from('<IiI', data,p)
    fields.append({'name':name(n),'type_index':typ,'token':hex(token)})
methods = []
for p in table(5,32):
    n, owner, result, params, generic, token, flags, iflags, slot, count = struct.unpack_from('<IiiiiIHHHH',data,p)
    methods.append({'name':name(n),'owner_index':owner,'return_type_index':result,'parameter_count':count,'token':hex(token)})
types = []
for p in table(19,88):
    row = struct.unpack_from('<8I8i8H2I',data,p)
    fs, ms = row[8:10]
    mc, pc, fc = row[16:19]
    assert not fc or 0 <= fs <= fs+fc <= len(fields)
    assert not mc or 0 <= ms <= ms+mc <= len(methods)
    types.append({'name':name(row[0]),'namespace':name(row[1]),'token':hex(row[-1]),
                  'fields':fields[fs:fs+fc] if fc else [], 'methods':methods[ms:ms+mc] if mc else []})
images = []
for p in table(20,40):
    n, assembly, start, count = struct.unpack_from('<IiiI',data,p)
    images.append({'name':name(n),'type_start':start,'type_count':count})
    assert 0 <= start <= start+count <= len(types)
    for t in types[start:start+count]:
        t['image'] = name(n)
# Names are leads, not evidence that a live network message contains hidden cards.
patterns = {
 'hand': r'(?i)(hand.?cards?|hand.?tiles?|hold.?cards?|hold.?tiles?|shou.?pai|手牌|暗牌)',
 'deal': r'(?i)(deal.?cards?|send.?cards?|draw.?cards?|摸牌|发牌)',
 'replay_spectate': r'(?i)(replay|record.?game|spectat|watch.?game|observer|回放|观战)',
 'mahjong': r'(?i)(mahjong|majiang|mjgame|mjplayer|麻将)',
}
candidates=[]
for i,t in enumerate(types):
    typename = t['namespace']+'.'+t['name']
    hits={}
    for category, pattern in patterns.items():
        found=[]
        if re.search(pattern,typename): found.append('type:'+typename)
        found += ['field:'+f['name'] for f in t['fields'] if re.search(pattern,f['name'])]
        found += ['method:'+m['name'] for m in t['methods'] if re.search(pattern,m['name'])]
        if found: hits[category]=found
    if hits: candidates.append(dict(t,index=i,hits=hits))
# Record only relevant complete strings, not arbitrary string-table windows.
string_hits=collections.defaultdict(list)
pos=0
blob=data[string_offset:string_offset+string_size]
for part in blob.split(b'\0'):
    s=part.decode('utf-8',errors='replace')
    if len(s) <= 500:
        for category,pattern in patterns.items():
            if re.search(pattern,s): string_hits[category].append({'index':pos,'value':s})
    pos+=len(part)+1
summary={'metadata_version':29,'type_count':len(types),'method_count':len(methods),'field_count':len(fields),
         'images':images,'candidate_count':len(candidates),'keyword_string_counts':{k:len(v) for k,v in string_hits.items()},
         'asset_candidates':assets,'limits':['Name matches do not establish live protocol contents.', 'Replay, spectator and settlement structures may expose cards only after a round or with separate privileges.', 'No runtime traffic or device memory was captured.']}
for filename,value in [('summary.json',summary),('candidates.json',candidates),('string-hits.json',string_hits)]:
    (out/filename).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n')
lines=['# IL2CPP metadata reverse inspection','',f'Types: {len(types)}; methods: {len(methods)}; fields: {len(fields)}; candidate types: {len(candidates)}.', '', 'Names alone do not establish whether live opponents cards are sent to this client.', '']
for t in candidates:
    lines += [f"## {t.get('image')} :: {t['namespace']}.{t['name']}", '']
    lines += ['- '+k+': '+', '.join(v) for k,v in t['hits'].items()]
    lines += ['', 'Fields: '+', '.join(f['name'] for f in t['fields']), '', 'Methods: '+', '.join(m['name'] for m in t['methods']), '']
(out/'CANDIDATES.md').write_text('\n'.join(lines))
print(json.dumps({'types':len(types),'methods':len(methods),'candidates':len(candidates),'strings':summary['keyword_string_counts']},ensure_ascii=False))
