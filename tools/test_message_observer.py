import base64,json,tempfile,unittest
from pathlib import Path
from inspect_message_observer import events,statuses,runtime_model

class MessageObserverTests(unittest.TestCase):
    def test_frame_integrity_and_private_extraction(self):
        payload=b'private fixture bytes'
        row={'kind':'game_event','schema':1,'sequence':1,'channel':'tdr','message_type':31,'encoding':'reencoded_decoded_event','bytes':len(payload),'payload_b64':base64.b64encode(payload).decode(),'secret':'private-user-token'}
        raw=b'MJMessageObserverEvents/1\n'+json.dumps(row).encode()+b'\n'
        with tempfile.TemporaryDirectory() as temp:
            result=events(raw,temp,4)
            self.assertEqual(result['valid_frame_count'],1)
            self.assertEqual((Path(temp)/'entry-0004-seq-0001.tdr').read_bytes(),payload)
            self.assertNotIn('private-user-token',json.dumps(result))
            self.assertNotIn(row['payload_b64'],json.dumps(result))
            row['bytes']=len(payload)+1
            self.assertEqual(events(b'MJMessageObserverEvents/1\n'+json.dumps(row).encode())['invalid_record_count'],1)

    def test_snapshot_encoding_int32_and_closed_error_metadata(self):
        payload=b'{"iType":2147483647,"tiles":[1,2]}'
        row={'kind':'game_event','schema':1,'sequence':1,'channel':'tdr','message_type':2147483647,'encoding':'public_field_snapshot','bytes':len(payload),'payload_b64':base64.b64encode(payload).decode()}
        result=events(b'MJMessageObserverEvents/1\n'+json.dumps(row).encode())
        self.assertEqual(result['valid_frame_count'],1)
        self.assertEqual(result['frames'][0]['encoding'],'public_field_snapshot')
        row={'kind':'managed_status','schema':1,'status':'serialization_failed','value':0,'stage':'tdr_pack','category':'null_reference','message_type':3,'message':'private exception path'}
        result=statuses(b'MJMessageObserverStatus/1\n'+json.dumps(row).encode())
        self.assertEqual(result['records'][0]['stage'],'tdr_pack')
        self.assertNotIn('private exception',json.dumps(result))
        row['category']='private arbitrary type';row['stage']='private arbitrary path'
        result=statuses(b'MJMessageObserverStatus/1\n'+json.dumps(row).encode())
        self.assertNotIn('private arbitrary',json.dumps(result))

    def test_snapshot_private_extraction_and_header_agreement(self):
        row={'kind':'game_event','schema':1,'sequence':1,'channel':'tdr','message_type':1,'encoding':'public_field_snapshot','bytes':11,'payload_b64':base64.b64encode(b'{"iType":1}').decode()}
        row['bytes']=len(b'{"iType":1}')
        with tempfile.TemporaryDirectory() as temp:
            result=events(b'MJMessageObserverEvents/1\n'+json.dumps(row).encode(),temp,2)
            self.assertEqual(result['valid_frame_count'],1)
            self.assertEqual((Path(temp)/'entry-0002-seq-0001.snapshot.json').read_bytes(),b'{"iType":1}')
            self.assertFalse((Path(temp)/'entry-0002-seq-0001.tdr').exists())
        row['message_type']=8
        self.assertEqual(events(b'MJMessageObserverEvents/1\n'+json.dumps(row).encode())['invalid_record_count'],1)

    def test_runtime_model_counts_never_publish_values_or_panel_text(self):
        model={'kind':'runtime_model','schema':1,'source':'RoundModeUtil.RoundModel','status':'ok','phase':'live','generation':1,'game_type':149,'game_status':1,'player_count':4,'self_seat':0,'seats':[{'view_seat':v,'seat':v,'hand':[0,1,987654321],'concealed_kongs':[35,0],'aux_status':'read','aux_fields':{'curRedrawMjValue':[19],'LastMJ':[987654321]}} for v in range(4)],'cache_status':'candidates_recorded','cache_candidates':[{'source_type':'private-type-token','field':'private-path-token','values':[0,1,987654321]}],'panel_text':'private card names and token'}
        payload=json.dumps(model).encode();row={'kind':'runtime_model_event','schema':1,'sequence':1,'channel':'runtime','message_type':0,'encoding':'runtime_model_snapshot','bytes':len(payload),'payload_b64':base64.b64encode(payload).decode()}
        with tempfile.TemporaryDirectory() as temp:
            result=events(b'MJMessageObserverEvents/1\n'+json.dumps(row).encode(),temp,5)
            self.assertEqual(result['valid_frame_count'],1)
            self.assertEqual((Path(temp)/'entry-0005-seq-0001.runtime.json').read_bytes(),payload)
            summary=result['frames'][0]['runtime_model'];self.assertEqual(summary['seats'][1]['placeholder_count'],1);self.assertEqual(summary['seats'][1]['known_tile_count'],1);self.assertEqual(summary['seats'][1]['invalid_tile_count'],1);self.assertEqual(summary['seats'][1]['known_concealed_kong_groups'],1);self.assertEqual(summary['seats'][1]['aux_fields']['LastMJ']['valid_tile_code_count'],0)
            public=json.dumps(result)
            for token in ('987654321','private-type-token','private-path-token','private card names',row['payload_b64']):self.assertNotIn(token,public)
        model['seats'][1]['seat']=3;self.assertIsNone(runtime_model(model))
        model['status']=[];self.assertIsNone(runtime_model(model))
        row['kind']='game_event'
        self.assertEqual(events(b'MJMessageObserverEvents/1\n'+json.dumps(row).encode())['invalid_record_count'],1)

    def test_registered_module_coverage_and_new_cache_budgets(self):
        model={'kind':'runtime_model','schema':1,'source':'RoundModeUtil.RoundModel','status':'ok','phase':'dealing','generation':1,'game_type':149,'game_status':1,'player_count':4,'self_seat':0,'seats':[{'view_seat':v,'seat':v,'hand':[]} for v in range(4)],'cache_status':'no_numeric_candidates','cache_candidates':[],'cache_stats':{'objects_visited':8,'unsupported_containers':0,'time_budget_hit':False,'read_failures':0},'cache_model_types':['private-type-metadata'],'cache_roots':[{'name':'module_models','field_present':True,'object_present':True,'registered_count':5,'slots':7,'values_visited':5,'read_status':'read'},{'name':'blackboard','field_present':True,'object_present':True,'registered_count':0,'slots':0,'values_visited':0,'read_status':'empty'}]}
        result=runtime_model(model);self.assertTrue(result['cache_root_coverage_provided']);self.assertFalse(result['model_hands_ready']);self.assertEqual(result['cache_roots'][0]['registered_count'],5);self.assertEqual(result['visited_model_type_count'],1);self.assertNotIn('private-type-metadata',json.dumps(result))
        model['cache_candidates']=[{'values':[1],'candidate_kind':'scalar','origin':'protocol_cache','owner_field':'dictionary_key_candidate','owner_value':2} for i in range(48)]
        self.assertEqual(len(runtime_model(model)['cache_candidates']),48)
        model['cache_candidates'].append({'values':[1]});self.assertIsNone(runtime_model(model))
        model['cache_candidates']=[{'values':[1]*200} for i in range(21)];self.assertIsNone(runtime_model(model))

    def test_subscription_is_not_inherited_across_sessions(self):
        rows=[{'kind':'native_status','schema':1,'status':'library_loaded'}, {'kind':'managed_status','schema':1,'status':'observer_started','value':0}, {'kind':'native_status','schema':1,'status':'library_loaded'}]
        result=statuses(b'MJMessageObserverStatus/1\n'+b'\n'.join(json.dumps(r).encode() for r in rows))
        self.assertEqual(result['session_count'],2)
        self.assertFalse(result['latest_session_subscription_reported'])

    def test_invalid_frames_and_unknown_status_are_not_success(self):
        self.assertEqual(events(b'MJMessageObserverEvents/1\n{"kind":"game_event"}')['valid_frame_count'],0)
        result=statuses(b'MJMessageObserverStatus/1\n{"kind":"native_status","schema":1,"status":"private-user-token"}')
        self.assertEqual(result['invalid_record_count'],1)
        self.assertNotIn('private-user-token',json.dumps(result))

if __name__=='__main__':unittest.main()
