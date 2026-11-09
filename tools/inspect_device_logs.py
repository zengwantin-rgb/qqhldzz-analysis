"""Run on Actions; publish only allowlisted facts, never raw device log lines."""
import collections,json,pathlib,re,zipfile
out=pathlib.Path('report/device')
out.mkdir(parents=True,exist_ok=True)
keywords={
 'legacy_dispatch':'RecvGameMsg msgType:', 'legacy_deal':'Recv Deal Self mjValue:',
 'legacy_recv_deal':'RevcDealCommand', 'legacy_other_redraw':'HandleOtherRedraw',
 'new_game':'NewGame', 'new_vm_init':'InitVM jsonPath', 'new_vm_create':'CreateVirtualMachine',
 'new_wire_entry':'OnRecvCSMsg', 'new_card_heap':'HandCardHeap', 'new_rule_config':'NewRuleV2Config',
 'new_rule_list':'UseNewRuleGameType', 'new_rule_adapter':'NewRuleAdapter',
 'trace_path':'TraceBinLogs', 'deal_protocol':'ID_SO_NOTIFY_DEAL',
 'hand_struct':'THandTiles', 'hand_field':'stHandTiles', 'hand_count':'nHandTilesNum',
 'initial_count':'nTilesNum', 'tile_array':'szTiles', 'draw_extra':'chExCmd',
 'draw_extra_array':'ExtraTiles', 'key_hand':'KEY_HAND_TILES',
 'replay':'CSReplay', 'table_rebuild':'ID_SO_TABLE_ALL_INFO',
 'server_game_data':'GameSo', 'private_engine_event':'HBPEngine',
 'room_type':'ModuleTuidaohu49', 'blueprint':'blueprint',
 'protobuf':'protobuf', 'wire_hex':'HexDump', 'packet_dump':'DumpData',
}
summary={'manifest':{},'logs':[],'privacy':'Raw lines, user IDs, credentials, tokens, paths and packet bodies are not included in this public summary.'}
with zipfile.ZipFile('private-input/device.zip') as z:
 assert z.testzip() is None, 'ZIP CRC error'
 manifest=json.loads(z.read('manifest.json'))
 summary['manifest']={k:manifest.get(k) for k in ['device_system','app_version','tool_version']}
 summary['manifest'].update({'container_count':len(manifest.get('scans',[])), 'scan_errors':sum(len(s.get('errors',[])) for s in manifest.get('scans',[])), 'export_errors':len(manifest.get('skipped_files',[])), 'scan_truncated':any(s.get('truncated',False) for s in manifest.get('scans',[])), 'candidate_file_count':sum(len(s.get('files',[])) for s in manifest.get('scans',[])), 'exported_file_count':len(manifest.get('exported_files',[]))})
 for entry in z.infolist():
  if not entry.filename.endswith('.log'):continue
  raw=z.read(entry);data=raw.decode('utf-8',errors='replace');lines=data.splitlines()
  category='main' if '/Log398/' in entry.filename else 'cloud' if '/GCloudSDKLog/' in entry.filename else 'activity' if '/Pandora/' in entry.filename else 'push_or_other'
  timestamps=re.findall(r'\b(\d{4}-\d{2}-\d{2}) (\d{2}:\d{2}:\d{2})',data)
  counts={name:data.count(pattern) for name,pattern in keywords.items()}
  dispatch=collections.Counter()
  tags=collections.Counter();cpp=collections.Counter()
  events=[];arrays=[];field_facts=[]
  for number,line in enumerate(lines,1):
   parts=line.split('|',4)
   if len(parts)==5:
    source=re.search(r'([A-Za-z0-9_]+\.cpp):\d+$',parts[2])
    if source:
     cpp[source.group(1)]+=1
     if re.fullmatch(r'[A-Z_][A-Z_0-9:]{1,90}',parts[3]):tags[parts[3]]+=1
   m=re.search(r'RecvGameMsg msgType:(ID_SO_[A-Z_0-9]+) gameType:([A-Za-z_0-9]{1,60})',line)
   if m:
    dispatch[m.group(1)+' / '+m.group(2)]+=1
   hit=[name for name in ['legacy_dispatch','legacy_deal','new_vm_init','new_vm_create','new_card_heap','deal_protocol','hand_field','hand_struct','draw_extra','draw_extra_array'] if keywords[name] in line]
   if hit and len(events)<200:
    tm=re.search(r'\b(\d{2}:\d{2}:\d{2})(?:\.\d+)?',line)
    events.append({'line':number,'time':tm.group(1) if tm else None,'categories':hit})
   for field in ['nHandTilesNum','nTilesNum','nExtraTilesNum','chExCmd','nSeat','chChair']:
    for m in re.finditer(r'\b'+field+r'\b["\s]*[:=]\s*(-?\d{1,4})(?!\d)',line):
     n=int(m.group(1))
     if -1<=n<=200:field_facts.append({'line':number,'field':field,'value':n})
   m=re.search(r'Recv Deal Self mjValue:\s*\[([^\]]{1,250})\]',line)
   if m:
    values=re.findall(r'-?\d+',m.group(1))
    arrays.append({'line':number,'kind':'self_initial_tiles','count':len(values),'note':'Values omitted; this is the self hand log, not opponents evidence.'})
  summary['logs'].append({'category':category,'bytes':len(raw),'line_count':len(lines),'replacement_characters':data.count('\ufffd'),'time_start':timestamps[0] if timestamps else None,'time_end':timestamps[-1] if timestamps else None,'keyword_counts':counts,'dispatch_counts':dict(dispatch),'source_files':dict(cpp.most_common(25)),'native_tags':dict(tags.most_common(25)),'candidate_events':events,'field_facts':field_facts[:200],'self_hand_arrays':arrays})
(out/'log-summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(summary,ensure_ascii=False,indent=2))
