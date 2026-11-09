"""Remote structural recovery, requiring complete directory and block validation."""
import hashlib,json,pathlib,struct,zipfile
import lz4.block
import UnityPy
from unitybundle_common import valid_info,readz
out=pathlib.Path('report/recovered')
out.mkdir(parents=True,exist_ok=True)
results=[]
with zipfile.ZipFile('inputs/_Decrypted.ipa') as z:
    for name in z.namelist():
        if not any(name.endswith('/'+s+'_ab_v1.bundle') for s in ['bootstrap','aotdll','hotloaddll']):continue
        b=z.read(name)
        sig,p=readz(b,0);version=struct.unpack_from('>I',b,p)[0];p+=4
        _,p=readz(b,p);_,p=readz(b,p)
        hdr=p
        total,first,second,flags=struct.unpack_from('>QIII',b,p)
        start=(p+20+15)&~15
        r={'name':name,'declared_first_size':first,'declared_second_size':second,'attempts':[]}
        # Hypothesis: the two directory sizes were exchanged; XOR applies to actual compressed bytes only.
        source=b[start:start+second]
        for key in range(256):
            decoded=bytes(x^key for x in source)
            try:raw=lz4.block.decompress(decoded,uncompressed_size=first)
            except Exception:continue
            parsed=valid_info(raw)
            if not parsed:continue
            pos=start+second
            if flags&0x200:pos=(pos+15)&~15
            block_results=[]
            for block in parsed['blocks']:
                payload=b[pos:pos+block['compressed']];pos+=block['compressed']
                block_results.append({'size':len(payload),'expected':block['uncompressed'],'head':payload[:16].hex()})
            r['attempts'].append({'key':key,'directory':parsed,'block_layout_end':pos,'file_size':len(b),'blocks':block_results})
            # Build only a corrected copy: app input is untouched.
            corrected=bytearray(b)
            corrected[:8]=b'UnityFS\0'
            struct.pack_into('>II',corrected,hdr+8,second,first)
            corrected[start:start+second]=decoded
            try:
                env=UnityPy.load(bytes(corrected))
                texts=[]
                for obj in env.objects:
                    if obj.type.name!='TextAsset':continue
                    a=obj.parse_as_object();payload=a.m_Script
                    if isinstance(payload,str):payload=payload.encode('utf-8',errors='surrogateescape')
                    entry={'name':a.m_Name,'size':len(payload),'head':payload[:16].hex()}
                    if payload[:2]==b'MZ':
                        dest=out/'assemblies'/pathlib.Path(a.m_Name).name
                        if dest.suffix!='.dll':dest=dest.with_name(dest.name+'.dll')
                        dest.parent.mkdir(exist_ok=True)
                        dest.write_bytes(payload)
                        entry['path']=str(dest);entry['sha256']=hashlib.sha256(payload).hexdigest()
                    texts.append(entry)
                r['attempts'][-1]['text_assets']=texts
            except Exception as e:r['attempts'][-1]['asset_parse_error']=str(e)[:1000]
        results.append(r)
        assert len(r["attempts"]) == 1, "Expected one structurally valid directory"
        attempt=r["attempts"][0]
        assert attempt["block_layout_end"] == len(b), "Block ranges do not cover the complete bundle"
        assert "asset_parse_error" not in attempt and attempt.get("text_assets"), "Asset parsing did not succeed"
(out/'recovery.json').write_text(json.dumps(results,indent=2)+'\n')
print(json.dumps(results,indent=2))
