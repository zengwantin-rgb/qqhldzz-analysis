"""Validate observer journals; decoded packets stay under private-input only."""
import base64
import binascii
import collections
import json
import pathlib
import zipfile
from inspect_phone_diagnostics import boolean

NATIVE = frozenset(('library_loaded preparing_files required_api_unavailable domain_not_ready thread_attach_failed '
    'game_interfaces_not_ready corlib_not_ready loader_types_unavailable byte_array_loader_unavailable '
    'managed_payload_read_failed managed_assembly_load_failed managed_assembly_loaded observer_entry_not_visible '
    'observer_start_failed observer_started observer_not_ready unsupported_game_build diagnostic_files_failed objective_c_error model_reader_invoke_failed model_reader_api_unavailable model_reader_domain_unavailable model_reader_attach_failed').split())
MANAGED = frozenset(('observer_started observer_stopped first_event_recorded capture_progress capture_limit_reached '
                     'serialization_failed capture_failed unexpected_payload model_reader_available runtime_model_limit_reached').split())


def integer(value,minimum,maximum):
    return type(value) is int and minimum<=value<=maximum


def runtime_model(obj):
    statuses={'waiting_for_table','unsupported_model','unsupported_game','not_playing','invalid_seats','invalid_hand_list','model_fields_missing','model_read_failed','ok'}
    phases={'live','after_hu','stale','unanchored'}
    if not isinstance(obj,dict) or obj.get('kind')!='runtime_model' or obj.get('schema')!=1 or obj.get('source')!='RoundModeUtil.RoundModel' or not isinstance(obj.get('status'),str) or obj.get('status') not in statuses or not isinstance(obj.get('phase'),str) or obj.get('phase') not in phases:return None
    rows=obj.get('seats');own=obj.get('self_seat');generation=obj.get('generation')
    if not isinstance(rows,list) or len(rows) not in (0,4) or not integer(generation,0,10000):return None
    output={'status':obj['status'],'phase':obj['phase'],'generation':generation,'seats':[]}
    if obj['status']!='ok' and rows:return None
    if obj['status']=='ok':
        if obj.get('game_type')!=149 or not integer(obj.get('game_status'),1,1) or obj.get('player_count')!=4 or not integer(own,0,3) or len(rows)!=4:return None
        output['self_seat']=own
    for view,row in enumerate(rows):
        if not isinstance(row,dict) or not integer(row.get('view_seat'),view,view) or not integer(own,0,3) or not integer(row.get('seat'),(own+view)%4,(own+view)%4):return None
        cards=row.get('hand')
        if not isinstance(cards,list) or len(cards)>32 or not all(integer(c,-2147483648,2147483647) for c in cards):return None
        known=sum(1<=c<=9 or 11<=c<=19 or 21<=c<=29 or 31<=c<=37 for c in cards);backs=sum(c==0 for c in cards)
        seat={'view_seat':view,'seat':row['seat'],'tiles':len(cards),'known_tile_count':known,'placeholder_count':backs,'invalid_tile_count':len(cards)-known-backs}
        if row.get('aux_status') in ('not_read','read','aux_partial','aux_read_failed','unsupported_kong_entry','unsupported_kong_list','invalid_kong_count'):seat['aux_status']=row['aux_status']
        kongs=row.get('concealed_kongs',[])
        if not isinstance(kongs,list) or len(kongs)>4 or not all(integer(c,-2147483648,2147483647) for c in kongs):return None
        seat['concealed_kong_groups']=len(kongs);seat['known_concealed_kong_groups']=sum(1<=c<=9 or 11<=c<=19 or 21<=c<=29 or 31<=c<=37 for c in kongs)
        aux=row.get('aux_fields',{})
        if not isinstance(aux,dict):return None
        seat['aux_fields']={}
        for name,values in aux.items():
            if name not in ('curRedrawMjValue','LastMJ','m_tianHuTile','m_multiTileRedrawList','m_laiZhiList') or not isinstance(values,list) or len(values)>16 or not all(integer(c,-2147483648,2147483647) for c in values):return None
            seat['aux_fields'][name]={'entries':len(values),'nonzero_entries':sum(c!=0 for c in values),'valid_tile_code_count':sum(1<=c<=9 or 11<=c<=19 or 21<=c<=29 or 31<=c<=37 for c in values)}
        output['seats'].append(seat)
    cache=obj.get('cache_candidates',[])
    if not isinstance(cache,list) or len(cache)>48:return None
    total=0;counts=[]
    for candidate in cache:
        if not isinstance(candidate,dict):return None
        values=candidate.get('values')
        if not isinstance(values,list) or not 1<=len(values)<=200 or not all(integer(v,-2147483648,2147483647) for v in values):return None
        total+=len(values)
        if total>4096:return None
        row={'array_length':len(values),'zero_entries':sum(v==0 for v in values),'valid_tile_code_count':sum(1<=v<=9 or 11<=v<=19 or 21<=v<=29 or 31<=v<=37 for v in values)}
        if candidate.get('candidate_kind') in ('scalar','numeric_array'):row['candidate_kind']=candidate['candidate_kind']
        if candidate.get('origin') in ('model_cache','protocol_cache'):row['origin']=candidate['origin']
        if candidate.get('owner_field') in ('nSeatID','nSeat','seatID','seatId','viewSeat','dictionary_key_candidate','array_index_candidate','none') and integer(candidate.get('owner_value'),-1,3):row['owner_field']=candidate['owner_field'];row['owner_value']=candidate['owner_value']
        counts.append(row)
    output['cache_candidates']=counts
    if obj.get('cache_status') in ('empty_or_unsupported','no_blackboard_field','budget_limited','candidates_recorded','cache_read_failed','partial_read','unsupported_storage','no_numeric_candidates'):output['cache_status']=obj['cache_status']
    roots=obj.get('cache_roots')
    output['cache_root_coverage_provided']=isinstance(roots,list)
    if roots is not None:
        if not isinstance(roots,list) or len(roots)>2:return None
        output['cache_roots']=[]
        for root in roots:
            if not isinstance(root,dict) or root.get('name') not in ('module_models','blackboard'):return None
            if type(root.get('field_present')) is not bool or type(root.get('object_present')) is not bool:return None
            if not integer(root.get('registered_count'),-1,10000) or not integer(root.get('slots'),-1,100000) or not integer(root.get('values_visited'),0,512):return None
            if root.get('read_status') not in ('not_read','field_missing','null','empty','entries_missing','read','budget_limited','not_dictionary'):return None
            output['cache_roots'].append({k:root[k] for k in ('name','field_present','object_present','registered_count','slots','values_visited','read_status')})
    stats=obj.get('cache_stats')
    if stats is not None:
        if not isinstance(stats,dict) or not integer(stats.get('objects_visited'),0,96) or not integer(stats.get('unsupported_containers'),0,100000) or type(stats.get('time_budget_hit')) is not bool:return None
        output['cache_stats']={k:stats[k] for k in ('objects_visited','unsupported_containers','time_budget_hit')}
    return output

def statuses(raw):
    if len(raw)>64*1024 or not raw.startswith(b'MJMessageObserverStatus/1\n'):
        return {'status':'invalid_status_header_or_size'}
    records=[];invalid=0;session=0
    for line in raw.splitlines()[1:]:
        if not line.strip():continue
        try: obj=json.loads(line)
        except (ValueError,UnicodeError,RecursionError):invalid+=1;continue
        if not isinstance(obj,dict) or obj.get('schema')!=1:invalid+=1;continue
        kind,code=obj.get('kind'),obj.get('status')
        if not isinstance(code,str) or not ((kind=='native_status' and code in NATIVE) or (kind=='managed_status' and code in MANAGED)):
            invalid+=1;continue
        if kind=='native_status' and code=='library_loaded':session+=1
        row={'kind':kind,'status':code,'session':session}
        if kind=='native_status':
            row['attempt']=obj.get('attempt') if integer(obj.get('attempt'),0,3) else None
            row['reported_subscription_registered']=boolean(obj.get('subscription_registered'))
        else:
            value=obj.get('value')
            allowed=(value in (9,217) if type(value) is int and code=='unexpected_payload' else integer(value,0,512))
            row['count_or_channel']=value if allowed else None
        if kind=='managed_status' and code=='serialization_failed':
            if obj.get('stage') in ('tdr_pack','protobuf_encode','field_snapshot','journal_append'):row['stage']=obj['stage']
            if obj.get('category') in ('io','access','null_reference','argument','invalid_operation','not_supported','other'):row['category']=obj['category']
            if integer(obj.get('message_type'),-2147483648,2147483647):row['message_type']=obj['message_type']
        records.append(row)
    latest=[r for r in records if r['session']==session]
    return {'status':'recognized_status','session_count':session,'invalid_record_count':invalid,'records':records,
            'latest_session_subscription_reported':any(r['status']=='observer_started' for r in latest),
            'latest_session_first_event_reported':any(r['status']=='first_event_recorded' for r in latest)}


def events(raw, private_directory=None, entry=0):
    if len(raw)>18*1024*1024 or not raw.startswith(b'MJMessageObserverEvents/1\n'):
        return {'status':'invalid_event_header_or_size'}
    records=[];invalid=0;discontinuities=0;expected=1
    for line in raw.splitlines()[1:]:
        if not line.strip():continue
        if len(line)>400*1024:invalid+=1;continue
        try: obj=json.loads(line)
        except (ValueError,UnicodeError,RecursionError):invalid+=1;continue
        if not isinstance(obj,dict) or obj.get('kind') not in ('game_event','runtime_model_event') or obj.get('schema')!=1 or obj.get('encoding') not in ('reencoded_decoded_event','public_field_snapshot','runtime_model_snapshot'):invalid+=1;continue
        channel,sequence,msg,declared,payload=(obj.get(k) for k in ('channel','sequence','message_type','bytes','payload_b64'))
        model_frame=obj['encoding']=='runtime_model_snapshot'
        if model_frame and (channel!='runtime' or msg!=0 or obj['kind']!='runtime_model_event'):invalid+=1;continue
        if not model_frame and obj['kind']!='game_event':invalid+=1;continue
        if obj['encoding']=='public_field_snapshot' and channel!='tdr':invalid+=1;continue
        if channel not in ('tdr','protobuf','runtime') or not integer(sequence,1,512) or not integer(msg,-2147483648,2147483647) or not integer(declared,1,256*1024) or not isinstance(payload,str):invalid+=1;continue
        try: clear=base64.b64decode(payload,validate=True)
        except (ValueError,binascii.Error):invalid+=1;continue
        if len(clear)!=declared:invalid+=1;continue
        if obj['encoding']=='public_field_snapshot':
            try:snapshot=json.loads(clear)
            except (ValueError,UnicodeError,RecursionError):invalid+=1;continue
            if not isinstance(snapshot,dict) or snapshot.get('iType')!=msg:invalid+=1;continue
        model_summary=None
        if model_frame:
            try:model_summary=runtime_model(json.loads(clear))
            except (ValueError,UnicodeError,RecursionError):model_summary=None
            if model_summary is None:invalid+=1;continue
        elif channel=='runtime':invalid+=1;continue
        if sequence!=expected:discontinuities+=1
        expected=sequence+1
        if private_directory is not None:
            directory=pathlib.Path(private_directory);directory.mkdir(parents=True,exist_ok=True,mode=0o700)
            suffix='runtime.json' if model_frame else 'snapshot.json' if obj['encoding']=='public_field_snapshot' else channel
            (directory/('entry-%04d-seq-%04d.%s'%(entry,sequence,suffix))).write_bytes(clear)
        records.append({'sequence':sequence,'channel':channel,'message_type':msg,'bytes':declared,'encoding':obj['encoding']})
        if model_summary is not None:records[-1]['runtime_model']=model_summary
        if len(records)>=512:break
    return {'status':'recognized_events','valid_frame_count':len(records),'invalid_record_count':invalid,
            'sequence_discontinuities':discontinuities,'decoded_bytes':sum(r['bytes'] for r in records),
            'channel_counts':dict(collections.Counter(r['channel'] for r in records)),
            'message_type_counts':dict(collections.Counter(str(r['message_type']) for r in records)), 'frames':records}


if __name__=='__main__':
    status_files=[];event_files=[]
    with zipfile.ZipFile('private-input/device.zip') as archive:
        for entry,info in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename!='manifest.json'),1):
            if '/MJMessageObserver/' not in info.filename:continue
            if info.filename.endswith('/observer-status.log'):
                status_files.append({'entry':entry,**statuses(archive.read(info))} if info.file_size<=64*1024 else {'entry':entry,'status':'status_file_limit'})
            elif pathlib.PurePosixPath(info.filename).name.startswith('game-events-') and info.filename.endswith('.log'):
                event_files.append({'entry':entry,**events(archive.read(info),'private-input/decoded-game-frames',entry)} if info.file_size<=18*1024*1024 else {'entry':entry,'status':'event_file_limit'})
    out=pathlib.Path('report/device');out.mkdir(parents=True,exist_ok=True)
    model_rows=[{'entry':f['entry'],'sequence':r['sequence'],**r['runtime_model']} for f in event_files for r in f.get('frames',[]) if 'runtime_model' in r]
    (out/'runtime-model-summary.json').write_text(json.dumps({'observations':model_rows,'observation_count':len(model_rows),'limits':['Actual model tile values, cache values and panel text remain private.','View seats are distinguished from raw player seats using the sampled self seat.','Cache candidates are not asserted to be actual opponent hands; model values are separate from original wire bytes.']},indent=2)+'\n')
    (out/'message-observer-summary.json').write_text(json.dumps({'status_files':status_files,'event_files':event_files,
        'total_valid_frames':sum(f.get('valid_frame_count',0) for f in event_files), 'limits':[
        'Raw/base64 packet payloads and private output paths are never emitted; binary packets remain in the runner private-input directory.',
        'Captured entries are re-encoded events, direct protocol field snapshots or separately labeled runtime model observations, not original network bytes. Original listener ordering is not established.',
        'A subscription status is separate from the observed valid frame count; valid frames do not establish live opponent hand visibility.',
        'Each event file is a separate capture. It is not automatically attributed to the latest status-log session.'
    ]},indent=2)+'\n')
