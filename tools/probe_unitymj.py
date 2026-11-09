"""Remote-only structural probes and native references; does not execute app code."""
import json, pathlib, struct, zipfile
import lz4.block
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM
out=pathlib.Path('report/format')
out.mkdir(parents=True,exist_ok=True)
results=[]
from unitybundle_common import readz, valid_info
with zipfile.ZipFile('inputs/_Decrypted.ipa') as z:
    for name in z.namelist():
        if not any(name.endswith('/'+s+'_ab_v1.bundle') for s in ['bootstrap','aotdll','hotloaddll']):continue
        b=z.read(name)
        sig,p=readz(b,0)
        version=struct.unpack_from('>I',b,p)[0];p+=4
        player,p=readz(b,p);engine,p=readz(b,p)
        total,csize,usize,flags=struct.unpack_from('>QIII',b,p);p+=20
        start=(p+15)&~15
        info_start=len(b)-csize if flags&128 else start
        block=b[info_start:info_start+csize]
        row={'name':name,'signature':sig,'format_version':version,'player':player,'engine':engine,'file_size':len(b),'declared_size':total,'compressed_info_size':csize,'uncompressed_info_size':usize,'flags':hex(flags),'header_end':p,'aligned_header_end':start,'info_start':info_start,'info_head':block[:128].hex(),'file_head':b[:160].hex(),'successful_probes':[]}
        if not 0<csize<=len(b) or not 0<usize<10000000:
            row['error']='Implausible standard header sizes';results.append(row);continue
        # Standard offsets and adjacent positions test alignment rather than assume encryption.
        for offset in sorted(set([p,start,info_start]+list(range(max(p,start-16),min(len(b)-csize,start+17))))):
            source=b[offset:offset+csize]
            for key in range(256):
                candidate=bytes(x^key for x in source) if key else source
                try:
                    unpacked=lz4.block.decompress(candidate,uncompressed_size=usize) if flags&63 in (2,3) else candidate
                except Exception:continue
                parsed=valid_info(unpacked)
                if parsed:
                    row['successful_probes'].append({'offset':offset,'xor_key':key,'directory':parsed})
        results.append(row)
(out/'bundle-probes.json').write_text(json.dumps(results,indent=2)+'\n')
# Parse 64-bit thin Mach-O sections and locate native references to format/compression strings.
b=pathlib.Path('inputs/UnityFramework').read_bytes()
assert b[:4]==b'\xcf\xfa\xed\xfe'
count=struct.unpack_from('<I',b,16)[0];p=32;sections=[]
for i in range(count):
    cmd,size=struct.unpack_from('<II',b,p)
    if cmd==0x19:
        num=struct.unpack_from('<I',b,p+64)[0]
        for j in range(num):
            q=p+72+j*80
            section,segment,addr,length,off=struct.unpack_from('<16s16sQQI',b,q)
            sections.append({'section':section.rstrip(b'\0').decode(),'segment':segment.rstrip(b'\0').decode(),'address':addr,'size':length,'offset':off})
    p+=size
needles=[b'UnityMJ',b'UnityFS',b'LZ4',b'lz4',b'ArchiveStorage',b'ArchiveBlock']
strings=[]
for s in sections:
    if s['section'] not in ('__cstring','__const'):continue
    chunk=b[s['offset']:s['offset']+s['size']]
    for needle in needles:
        pos=0
        while (pos:=chunk.find(needle,pos))>=0:
            lo=max(0,chunk.rfind(b'\0',0,pos)+1);hi=chunk.find(b'\0',pos)
            if hi<0:hi=min(pos+200,len(chunk))
            strings.append({'needle':needle.decode(),'address':s['address']+pos,'string_start':s['address']+lo,'text':chunk[lo:min(hi,lo+300)].decode(errors='replace')})
            pos+=len(needle)
            if len(strings)>1000:break
pages={r['address']&~4095 for r in strings}
md=Cs(CS_ARCH_ARM64,CS_MODE_ARM)
refs=[]
for s in sections:
    if s['section']!='__text':continue
    code=memoryview(b)[s['offset']:s['offset']+s['size']]
    for i,(word,) in enumerate(struct.iter_unpack('<I',code[:len(code)//4*4])):
        if word&0x9f000000!=0x90000000:continue
        imm=((word>>5)&0x7ffff)*4+((word>>29)&3)
        if imm&(1<<20):imm-=1<<21
        pc=s['address']+i*4
        target=(pc&~4095)+(imm<<12)
        if target not in pages:continue
        reg=word&31
        for j in range(1,9):
            if (i+j)*4+4>len(code):break
            nextword=struct.unpack_from('<I',code,(i+j)*4)[0]
            if nextword&0xffc00000==0x91000000 and ((nextword>>5)&31)==reg:
                exact=target+((nextword>>10)&4095)
                matches=[r for r in strings if r['string_start']<=exact<=r['address']+len(r['needle'])]
                if matches:
                    lo=max(0,i*4-96);hi=min(len(code),i*4+192)
                    refs.append({'address':hex(pc),'target':hex(exact),'strings':matches,'disassembly':[f'{ins.address:#x}: {ins.mnemonic} {ins.op_str}' for ins in md.disasm(bytes(code[lo:hi]),s['address']+lo)]})
                    break
(out/'native-format-references.json').write_text(json.dumps({'sections':sections,'strings':strings,'references':refs},indent=2)+'\n')
print(json.dumps({'bundles':len(results),'successful_probes':[len(r['successful_probes']) for r in results],'native_strings':len(strings),'native_references':len(refs)}))
