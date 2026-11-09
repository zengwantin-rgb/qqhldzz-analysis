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
    'observer_start_failed observer_started observer_not_ready unsupported_game_build diagnostic_files_failed objective_c_error').split())
MANAGED = frozenset(('observer_started observer_stopped first_event_recorded capture_progress capture_limit_reached '
                     'serialization_failed capture_failed unexpected_payload').split())


def integer(value,minimum,maximum):
    return type(value) is int and minimum<=value<=maximum


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
        if not isinstance(obj,dict) or obj.get('kind')!='game_event' or obj.get('schema')!=1 or obj.get('encoding') not in ('reencoded_decoded_event','public_field_snapshot'):invalid+=1;continue
        channel,sequence,msg,declared,payload=(obj.get(k) for k in ('channel','sequence','message_type','bytes','payload_b64'))
        if channel not in ('tdr','protobuf') or not integer(sequence,1,512) or not integer(msg,-2147483648,2147483647) or not integer(declared,1,256*1024) or not isinstance(payload,str):invalid+=1;continue
        try: clear=base64.b64decode(payload,validate=True)
        except (ValueError,binascii.Error):invalid+=1;continue
        if len(clear)!=declared:invalid+=1;continue
        if sequence!=expected:discontinuities+=1
        expected=sequence+1
        if private_directory is not None:
            directory=pathlib.Path(private_directory);directory.mkdir(parents=True,exist_ok=True,mode=0o700)
            (directory/('entry-%04d-seq-%04d.%s'%(entry,sequence,channel))).write_bytes(clear)
        records.append({'sequence':sequence,'channel':channel,'message_type':msg,'bytes':declared,'encoding':obj['encoding']})
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
    (out/'message-observer-summary.json').write_text(json.dumps({'status_files':status_files,'event_files':event_files,
        'total_valid_frames':sum(f.get('valid_frame_count',0) for f in event_files), 'limits':[
        'Raw/base64 packet payloads and private output paths are never emitted; binary packets remain in the runner private-input directory.',
        'Captured bytes are re-encoded decoded events, not original network bytes. Original listener ordering is not established.',
        'A subscription status is separate from the observed valid frame count; valid frames do not establish live opponent hand visibility.',
        'Each event file is a separate capture. It is not automatically attributed to the latest status-log session.'
    ]},indent=2)+'\n')
