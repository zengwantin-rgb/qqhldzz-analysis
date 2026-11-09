"""Bounded container driver. No raw process output escapes the private directory."""
import json,pathlib,re,subprocess,os
root=pathlib.Path.cwd();out=root/'private-output';out.mkdir(exist_ok=True,mode=0o700)
formats={}
for source in (root/'private-input/decoded-game-frames').glob('entry-*-seq-*.snapshot.json'):
    if not re.fullmatch(r'entry-\d{4}-seq-\d{4}\.snapshot\.json',source.name):continue
    name=source.name.replace('.snapshot.json','.tdr.json')
    (out/name).write_bytes(source.read_bytes());formats[name]='public_field_snapshot'
(out/'capture-formats.json').write_text(json.dumps(formats))
def run(args,name):
    with (root/'private-input'/name).open('wb') as stream:return subprocess.run(args,stdout=stream,stderr=subprocess.STDOUT).returncode
result={}
result['image_pull_exit']=run(['docker','pull','mono:6.12'],'docker-pull.txt')
if result['image_pull_exit']==0:
    args=['docker','run','--rm','--user',str(os.getuid())+':'+str(os.getgid()),'--network','none','--read-only','--cap-drop','ALL','--security-opt','no-new-privileges','--pids-limit','128','--memory','768m','--cpus','1','--tmpfs','/tmp:rw,nosuid,size=64m','-v',str(root/'private-input')+':/input:ro','-v',str(out)+':/output:rw','-v',str(root/'tools/DecodeTdr')+':/code:ro','-v',str(root/'mobile/MessageObserver')+':/observer:ro','mono:6.12','sh','-c','mcs -out:/tmp/decode.exe /code/Program.cs /observer/FieldSnapshot.cs /observer/ObserverJournal.cs /observer/ProtocolUnions.cs && mono /tmp/decode.exe /input/decoder-assemblies /input/decoded-game-frames /output']
    result['decoder_exit']=run(args,'decoder-output.txt')
for name in ('docker-pull.txt','decoder-output.txt'):
    p=root/'private-input'/name
    if not p.exists():continue
    text=p.read_text(errors='replace')
    result[name]={'compiler_error_codes':sorted(set(re.findall(r'error (CS\d{4})',text))),
        'known_markers':[m for m in ('toomanyrequests','manifest unknown','not found','Permission denied','Read-only file system','No space left','TypeLoadException','FileNotFoundException','OutOfMemoryException','Failed to create','Unhandled Exception') if m in text]}
(root/'report/device/container-status.json').write_text(json.dumps(result,indent=2)+'\n')
p=out/'decode-summary.json'
if p.exists():(root/'report/device/decode-summary.json').write_bytes(p.read_bytes())
