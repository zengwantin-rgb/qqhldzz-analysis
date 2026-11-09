import collections, hashlib, json, pathlib, re, zipfile
import UnityPy
out = pathlib.Path('report/assets')
out.mkdir(parents=True,exist_ok=True)
assemblies=out/'assemblies'
assemblies.mkdir(exist_ok=True)
entries=[]
results=[]
with zipfile.ZipFile('inputs/_Decrypted.ipa') as z:
    for i in z.infolist():
        if '/Data/Raw/' not in i.filename or i.is_dir(): continue
        entries.append({'path':i.filename,'size':i.file_size})
        target = bool(re.search(r'(?i)(hotload|aotdll|bootstrap|\.dll\.bytes$)',i.filename))
        if not target or i.filename.endswith('.manifest'): continue
        b=z.read(i)
        r={'path':i.filename,'size':len(b),'head_hex':b[:32].hex(),'sha256':hashlib.sha256(b).hexdigest()}
        if b[:2]==b'MZ':
            destination=assemblies/(hashlib.sha256(b).hexdigest()[:12]+'_'+pathlib.Path(i.filename).name.replace('.bytes',''))
            destination.write_bytes(b)
            r['managed_candidate']=str(destination)
        else:
            try:
                env=UnityPy.load(b)
                counts=collections.Counter()
                texts=[]
                for obj in env.objects:
                    counts[obj.type.name]+=1
                    if obj.type.name != 'TextAsset': continue
                    asset=obj.parse_as_object()
                    payload=asset.m_Script
                    if isinstance(payload,str): payload=payload.encode('utf-8',errors='surrogateescape')
                    item={'name':asset.m_Name,'size':len(payload),'head_hex':payload[:16].hex()}
                    if payload[:2]==b'MZ':
                        basename=re.sub(r'[^A-Za-z0-9_.-]','_',asset.m_Name)
                        destination=assemblies/(hashlib.sha256(payload).hexdigest()[:12]+'_'+basename+'.dll')
                        destination.write_bytes(payload)
                        item['managed_candidate']=str(destination)
                    texts.append(item)
                r['objects']=dict(counts)
                r['text_assets']=texts
            except Exception as e:
                r['parse_error']=str(e)[:1000]
        results.append(r)
(out/'raw-inventory.json').write_text(json.dumps(entries,ensure_ascii=False,indent=2)+'\n')
(out/'hotload-inspection.json').write_text(json.dumps({'unitypy_version':UnityPy.__version__,'results':results},ensure_ascii=False,indent=2)+'\n')
print(json.dumps(results,ensure_ascii=False,indent=2))
