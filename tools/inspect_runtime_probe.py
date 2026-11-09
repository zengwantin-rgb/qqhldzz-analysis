"""Read the metadata-only native probe with a closed output vocabulary."""
import json
import pathlib
import zipfile
from inspect_phone_diagnostics import boolean

APIS = frozenset(('il2cpp_domain_get il2cpp_thread_current il2cpp_thread_attach il2cpp_thread_detach '
    'il2cpp_domain_get_assemblies il2cpp_assembly_get_image il2cpp_image_get_name il2cpp_class_from_name '
    'il2cpp_class_get_methods il2cpp_method_get_name il2cpp_method_get_param_count il2cpp_class_get_field_from_name '
    'il2cpp_class_get_method_from_name il2cpp_field_static_get_value il2cpp_runtime_invoke il2cpp_object_new '
    'il2cpp_array_new il2cpp_gchandle_new il2cpp_get_corlib il2cpp_method_get_param il2cpp_type_get_type '
    'il2cpp_class_from_type il2cpp_class_get_element_class il2cpp_object_unbox il2cpp_string_new').split())
TYPES = frozenset(('HappyMahjong.TSDK4CSharpService','HappyMahjong.TSDKMessageEvent',
    'strange.extensions.dispatcher.eventdispatcher.api.IEventDispatcher',
    'strange.extensions.dispatcher.eventdispatcher.api.EventCallback',
    'strange.extensions.dispatcher.eventdispatcher.api.IEvent',
    'strange.extensions.dispatcher.eventdispatcher.impl.EventDispatcher','System.Reflection.Assembly'))
METHODS = frozenset(('get_dispatcher','AddListener','RemoveListener','Dispatch','Invoke','Load','get_data'))
STATUSES = frozenset(('required_api_unavailable','domain_not_ready','thread_attach_failed',
                     'assembly_snapshot_unavailable','metadata_checked'))


def bounded(value, low, high):
    return value if type(value) is int and low <= value <= high else None


def analyze(raw):
    if len(raw)>64*1024:
        return {'status':'probe_file_limit_exceeded'}
    if not raw.startswith(b'MJRuntimeProbe/1\n'):
        return {'status':'unrecognized_probe_header'}
    rows=[]; invalid=0; session=0
    for number,line in enumerate(raw.splitlines()[1:],2):
        if not line.strip():continue
        try: obj=json.loads(line)
        except (ValueError,UnicodeError):invalid+=1;continue
        if not isinstance(obj,dict) or obj.get('kind')!='MJRuntimeProbe' or type(obj.get('schema')) is not int or obj['schema']!=1 or obj.get('probe_version') not in (None,'0.1.0'):
            invalid+=1;continue
        stage=obj.get('stage')
        if stage not in ('library_loaded','runtime_snapshot','objective_c_error'):
            invalid+=1;continue
        if stage=='library_loaded':session+=1
        row={'line':number,'session':session,'stage':stage,'attempt':bounded(obj.get('attempt'),1,3),
             'reported_capturing_game_messages':boolean(obj.get('capturing_game_messages'))}
        if stage=='runtime_snapshot':
            row['lookup_status']=obj.get('lookup_status') if obj.get('lookup_status') in STATUSES else None
            row['unity_framework_loaded']=boolean(obj.get('unity_framework_loaded'))
            row['assembly_count']=bounded(obj.get('assembly_count'),0,100000)
            row['assembly_scan_capped']=boolean(obj.get('assembly_scan_capped'))
            symbols=obj.get('native_apis',{})
            row['native_apis']={k:boolean(v) for k,v in symbols.items() if k in APIS} if isinstance(symbols,dict) else {}
            classes=[]
            source=obj.get('classes',[])
            for cls in source if isinstance(source,list) else []:
                if not isinstance(cls,dict) or cls.get('type') not in TYPES:continue
                item={'type':cls['type']}
                for key in ('found','expected_image','method_scan_capped','dispatcher_backing_field_found','game_message_field_found','game_pb_message_field_found'):
                    if key in cls:item[key]=boolean(cls[key])
                methods=cls.get('methods',[])
                item['methods']=[{'name':m['name'],'parameter_count':bounded(m.get('parameter_count'),0,32)}
                    for m in methods if isinstance(m,dict) and m.get('name') in METHODS] if isinstance(methods,list) else []
                classes.append(item)
            row['classes']=classes
        rows.append(row)
    latest=[r for r in rows if r['session']==session]
    return {'status':'recognized_probe','session_count':session,'invalid_record_count':invalid,
            'library_loaded_observed':session>0,
            'latest_session_metadata_checked':any(r.get('lookup_status')=='metadata_checked' for r in latest),
            'records':rows,'game_message_subscription_implemented':False,'live_opponent_display_implemented':False}


if __name__=='__main__':
    files=[]
    with zipfile.ZipFile('private-input/device.zip') as archive:
        for entry,info in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename!='manifest.json'),1):
            if info.filename.endswith('/MJRuntimeProbe/runtime-probe.log'):
                files.append({'entry':entry,**(analyze(archive.read(info)) if info.file_size<=64*1024 else {'status':'probe_file_limit_exceeded'})})
    out=pathlib.Path('report/device');out.mkdir(parents=True,exist_ok=True)
    (out/'runtime-probe-summary.json').write_text(json.dumps({'files':files,'limits':[
        'Only fixed native API names, selected source types, bounded counts and booleans are emitted.',
        'Unknown strings and unexpected fields are discarded, not logged.',
        'The released probe queries metadata only; successful lookup does not prove message subscription or opponent hand availability.'
    ]},indent=2)+'\n')
