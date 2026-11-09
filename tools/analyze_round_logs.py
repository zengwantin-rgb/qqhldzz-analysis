"""Derive bounded public facts from known gameplay log records on Actions."""
import collections,json,pathlib,re,zipfile
def small_sequence(text):
 m=re.match(r'\s*[\[({]?\s*([0-9;, \-]+)',text)
 if not m:return []
 numbers=[int(n) for n in re.findall(r'-?\d+',m.group(1))]
 return numbers if len(numbers)<=100 and all(-1<=n<=255 for n in numbers) else []

def payload(line):
 dec=json.JSONDecoder()
 for m in re.finditer(r'\{',line):
  try:value,_=dec.raw_decode(line[m.start():])
  except json.JSONDecodeError:continue
  if isinstance(value,dict):return value
 return None

def analyze_lines(lines,back):
 result={'placeholder_value_from_MJDef':back,'dispatches':[],'wait_messages':[],'self_draw_summary':[],'initial_self_hand_counts':[],'model_snapshots':[], 'show_hand_events':[], 'replay_balance_flags':[]}
 tlv_by_phase=collections.defaultdict(collections.Counter)
 phase='before_deal';round_index=0;view=None
 for index,line in enumerate(lines,1):
  tm=re.search(r'\b(\d{2}:\d{2}:\d{2})(?:\.\d+)?',line);time=tm.group(1) if tm else None
  event=re.search(r'RecvGameMsg msgType:(ID_SO_[A-Z_0-9]+) gameType:([A-Za-z_0-9]+)',line)
  if event:
   kind=event.group(1)
   if kind=='ID_SO_NOTIFY_DEAL':phase='live';round_index+=1
   elif kind in ('ID_SO_NOTIFY_HU',):phase='hu_notification'
   elif kind in ('ID_SO_NOTIFY_FINISH','ID_SO_NOTIFY_FINISH_V2','ID_SO_NOTIFY_FINISH_V3'):phase='settled'
   result['dispatches'].append({'line':index,'time':time,'message':kind,'game_type':event.group(2),'round':round_index,'phase':phase})
  tlv=re.search(r'Handle TLV, Type:(EM_(?:UP|DN)_TLV_[A-Z_0-9]+)',line)
  if tlv:tlv_by_phase[phase][tlv.group(1)]+=1
  if 'HandleWait WaitInfo.chFlag:' in line:
   obj=payload(line)
   if obj is not None:
    facts={k:v for k,v in obj.items() if k in ('chFlag','chChair','chExCmd','chDrawTileNum','nTileIndexNum','nExtraTilesNum') and isinstance(v,int) and -1<=v<=255}
    for key in ('ExtraTiles','TileIndex'):
     if isinstance(obj.get(key),list):facts[key+'_length']=len(obj[key])
    result['wait_messages'].append({'line':index,'time':time,'phase':phase,**facts})
  if 'HandleSelfRedraw ' in line:
   obj=payload(line)
   if obj is not None:
    value=obj.get('chTile')
    result['self_draw_summary'].append({'line':index,'time':time,'phase':phase,'single_tile_has_positive_value':isinstance(value,int) and 0<value<100,'multi_tile_count':len(obj.get('DrawTiles',[])) if isinstance(obj.get('DrawTiles'),list) else None})
  if 'Recv Deal Self mjValue:' in line:
   values=small_sequence(line.split('Recv Deal Self mjValue:',1)[1])
   result['initial_self_hand_counts'].append({'line':index,'time':time,'count':len(values),'placeholder_entries':sum(v==back for v in values) if back is not None else None})
  vm=re.search(r'(?:CheckAllMJ viewSeat:\s*|\bviewSeat\s*=\s*)([0-3])\b',line)
  if vm:view=int(vm.group(1))
  hand=re.search(r'(?:CheckAllMJ inHandList:\s*|inHandList\x27s count\s*=\s*\d+,\s*tiles\s*:\s*)(.*)',line)
  if hand and view is not None:
   values=small_sequence(hand.group(1))
   result['model_snapshots'].append({'line':index,'time':time,'phase':phase,'view_seat':view,'tile_count':len(values),'placeholder_entries':sum(v==back for v in values) if back is not None else None,'non_placeholder_entries':sum(v!=back for v in values) if back is not None else None})
  show=re.search(r'ShowHandTileAfterTing viewseat is ([0-3]) handtile count (\d{1,2})',line)
  if show:result['show_hand_events'].append({'line':index,'time':time,'phase':phase,'view_seat':int(show.group(1)),'count':int(show.group(2))})
  flag=re.search(r'RevcDealCommand CSReplayRecordService.sIsOnlyReplayBalance\(\) is:\s*(True|False|true|false)',line)
  if flag:result['replay_balance_flags'].append({'line':index,'time':time,'value':flag.group(1).lower()=='true'})
 waits=result['wait_messages']
 result['wait_aggregate']={'parsed_records':len(waits),'chExCmd_values':dict(collections.Counter(str(r.get('chExCmd','missing')) for r in waits)), 'nExtraTilesNum_values':dict(collections.Counter(str(r.get('nExtraTilesNum','missing')) for r in waits)), 'extra_array_lengths':dict(collections.Counter(str(r.get('ExtraTiles_length','missing')) for r in waits)), 'with_positive_draw_count':sum(r.get('chDrawTileNum',0)>0 for r in waits)}
 result['wait_aggregate']['unparsed_wait_markers']=sum('HandleWait WaitInfo.chFlag:' in line for line in lines)-len(waits)
 result['tlv_mentions_by_phase']={phase:dict(counts) for phase,counts in tlv_by_phase.items()}
 result['hand_tlv_mentions']={phase:{key:counts.get(key,0) for key in ['EM_UP_TLV_HAND_TILES','EM_DN_TLV_KEY_HAND_TILES','EM_UP_TLV_SEND_CARD','EM_DN_TLV_SEND_CARD']} for phase,counts in tlv_by_phase.items()}
 result['self_draw_aggregate']={'parsed_records':len(result['self_draw_summary']),'positive_single_tile_records':sum(r['single_tile_has_positive_value'] for r in result['self_draw_summary'])}
 aggregates=collections.defaultdict(lambda:collections.Counter())
 for row in result['model_snapshots']:
  a=aggregates[row['phase']+' / view '+str(row['view_seat'])]
  a['snapshots']+=1;a['tile_entries']+=row['tile_count'];a['placeholder_entries']+=row['placeholder_entries'] or 0;a['non_placeholder_entries']+=row['non_placeholder_entries'] or 0
 result['model_aggregates']={k:dict(v) for k,v in aggregates.items()}
 result['limits']=['Round count and phases are limited to this individual main log.', 'Zero observed extra fields do not prove unlogged packet fields are absent.', 'TLV logging may repeat the same item in two loops; mention counts are not independent packet counts.', 'Repeated model snapshots are observations, not independent hands.', 'Post-Hu and settlement data must not be treated as ordinary concealed-hand data.', 'This report contains counts and placeholders, not player identifiers or actual hand values.']
 return result

def analyze_archive(path,back):
 logs=[]
 with zipfile.ZipFile(path) as z:
  for entry,info in enumerate((i for i in z.infolist() if not i.is_dir() and i.filename!='manifest.json'),1):
   if '/Log398/' not in info.filename or not info.filename.endswith('.log'):continue
   result=analyze_lines(z.read(info).decode('utf-8',errors='replace').splitlines(),back)
   logs.append((info.filename,{'entry':entry,**result}))
 if not logs:raise ValueError('No main game log in export')
 primary=max(logs,key=lambda row:row[0])[1]
 return {**primary,'primary_entry':primary['entry'],'main_log_count':len(logs),'main_logs':[row for name,row in logs],
         'main_log_scope':'Compatibility top-level facts describe the lexically latest main log; main_logs includes every main log independently.'}

if __name__=='__main__':
 source=pathlib.Path('private-input/MJDef.cs').read_text()
 m=re.search(r'\bMJ_BACK\s*=\s*(-?\d+)\s*;',source)
 if m is None:raise ValueError('Placeholder definition unavailable')
 result=analyze_archive('private-input/device.zip',int(m.group(1)))
 out=pathlib.Path('report/device');out.mkdir(parents=True,exist_ok=True)
 (out/'round-analysis.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
 print(json.dumps({'main_log_count':result['main_log_count'],'primary_entry':result['primary_entry'],
                   'logs':[{'entry':r['entry'],'dispatches':len(r['dispatches']),'wait_aggregate':r['wait_aggregate'],'self_draw_aggregate':r['self_draw_aggregate']} for r in result['main_logs']]},indent=2))
