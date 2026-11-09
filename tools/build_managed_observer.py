"""Compile only our observer against supplied recovered metadata; never execute it."""
import os
import pathlib
import subprocess
root=pathlib.Path(os.environ['DOTNET_ROOT'])/'sdk'
compiler=sorted(root.glob('*/Roslyn/bincore/csc.dll'),key=lambda p:tuple(int(x) for x in p.parts[-4].split('.') if x.isdigit()))[-1]
out=pathlib.Path('build/message-observer');out.mkdir(parents=True,exist_ok=True)
references=sorted(pathlib.Path('recovered-input/recovered/assemblies').glob('*.dll'))
args=['dotnet',str(compiler),'/nologo','/noconfig','/nostdlib+','/langversion:7.3','/target:library','/optimize+','/debug-',f'/out:{out}/MJMessageObserver.Managed.dll']
args.extend('/reference:'+str(p) for p in references)
args.extend(['mobile/MessageObserver/ObserverJournal.cs','mobile/MessageObserver/ManagedObserver.cs','mobile/MessageObserver/FieldSnapshot.cs','mobile/MessageObserver/ProtocolUnions.cs'])
subprocess.run(args,check=True)
print('Compiled our managed observer against',len(references),'recovered metadata assemblies; no game code executed.')
