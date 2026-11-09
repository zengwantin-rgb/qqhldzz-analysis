"""Only known gameplay buffer fields are decoded. Raw content is never output."""
import base64,collections,json,pathlib,re,zipfile,zlib
out=pathlib.Path('report/device');out.mkdir(parents=True,exist_ok=True)
known=['szGameBuf','nGameBuf','iOriginSize','stUnitProtoValueBuf','stAbilityTileWallInfo','iTotalTileCount','nTileCount','astTileInfos','iTileFace','iTileBack','ProtoIntelligenceAdvice','AdviceType','AdviceTile','IntelligenceAdviceTing','TileLeftNum']
counts=collections.Counter();buffers=[];wall=[];advice=[];parse_attempts=0

def walk(value,depth=0):
 if depth>35:return
 if isinstance(value,dict):
  yield value
  for v in value.values():yield from walk(v,depth+1)
 elif isinstance(value,list):
  for v in value[:1000]:yield from walk(v,depth+1)

def varint(b,p):
 n=0
 for shift in range(0,70,7):
  if p>=len(b):raise ValueError()
  v=b[p];p+=1;n|=(v&127)<<shift
  if not v&128:return n,p
 raise ValueError()

def fields(b):
 rows=[];p=0
 while p<len(b):
  tag,p=varint(b,p);number=tag>>3;kind=tag&7
  if not 0<number<100000:raise ValueError()
  if kind==0:value,p=varint(b,p)
  elif kind==2:
   length,p=varint(b,p)
   if p+length>len(b):raise ValueError()
   value=b[p:p+length];p+=length
  elif kind in (1,5):
   length=8 if kind==1 else 4
   if p+length>len(b):raise ValueError()
   value=b[p:p+length];p+=length
  else:raise ValueError()
  rows.append((number,kind,value))
 return rows

def buffer_facts(blob):
 facts={'bytes':len(blob),'decompression':[]}
 for label,wbits in [('zlib',15),('raw_deflate',-15),('gzip',31)]:
  try:
   d=zlib.decompressobj(wbits);clear=d.decompress(blob,4*1024*1024+1)
   if not d.eof or d.unconsumed_tail or len(clear)>4*1024*1024:continue
  except zlib.error:continue
  row={'method':label,'decoded_bytes':len(clear)}
  try:
   outer=fields(clear)
   row['protobuf_field_numbers']=sorted(set(n for n,k,v in outer))
   row['defined_advice_field_count']=sum(n==1 and k==2 for n,k,v in outer)
   row['unknown_top_level_field_count']=sum(n!=1 for n,k,v in outer)
  except ValueError:row['protobuf_parseable']=False
  facts['decompression'].append(row)
 return facts

with zipfile.ZipFile('private-input/device.zip') as z:
 name=next(n for n in z.namelist() if '/Log398/' in n and n.endswith('.log'))
 lines=z.read(name).decode('utf-8',errors='replace').splitlines()
 for number,line in enumerate(lines,1):
  for key in known:
   counts[key]+=len(re.findall(r'(?<![A-Za-z0-9_])'+re.escape(key)+r'(?![A-Za-z0-9_])',line,re.I))
  # Parse JSON only on lines explicitly mentioning a known gameplay field.
  if not any(k.lower() in line.lower() for k in known):continue
  for match in re.finditer(r'\{',line):
   try:value,_=json.JSONDecoder().raw_decode(line[match.start():])
   except json.JSONDecodeError:continue
   parse_attempts+=1
   for obj in walk(value):
    if 'szGameBuf' in obj:
     val=obj['szGameBuf'];blob=None
     if isinstance(val,list) and len(val)<=4*1024*1024 and all(isinstance(n,int) and -128<=n<=255 for n in val):blob=bytes(n&255 for n in val)
     elif isinstance(val,str) and len(val)<6*1024*1024:
      try:blob=base64.b64decode(val,validate=True)
      except Exception:pass
     if blob is not None:
      n=obj.get('nGameBuf')
      if isinstance(n,int) and 0<=n<=len(blob):blob=blob[:n]
      buffers.append({'line':number,**buffer_facts(blob)})
    if all(key in obj for key in ('iTotalTileCount','nTileCount','astTileInfos')):
     values=obj.get('astTileInfos');n=obj.get('nTileCount')
     if isinstance(values,list) and isinstance(n,int) and 0<=n<=512:
      visible=hidden=0
      for item in values[:n]:
       if isinstance(item,dict) and isinstance(item.get('iTileFace'),int):
        if item['iTileFace']<0:hidden+=1
        else:visible+=1
      wall.append({'line':number,'total_count':obj['iTotalTileCount'] if isinstance(obj['iTotalTileCount'],int) and 0<=obj['iTotalTileCount']<=512 else None,'declared_count':n,'array_length':len(values),'nonnegative_face_entries':visible,'negative_face_entries':hidden})
    if 'AdviceType' in obj and 'IntelligenceAdviceTing' in obj:
     advice.append({'line':number,'advice_type':obj['AdviceType'] if isinstance(obj['AdviceType'],int) and 0<=obj['AdviceType']<100 else None,'known_advice_object':True})
   break
result={'known_field_mentions':dict(counts),'matching_json_objects':parse_attempts,'known_buffer_candidates':buffers,'wall_objects':wall,'advice_objects':advice,'limits':['Only recognized gameplay field names are searched; arbitrary token/base64 strings are not decoded.','A type-name mention without its bytes is not a recoverable payload.','No real hand values or identifying raw log content are published.']}
(out/'embedded-payload-probe.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(result,ensure_ascii=False,indent=2))
