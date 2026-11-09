import base64,json,tempfile,unittest
from pathlib import Path
from inspect_message_observer import events,statuses

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
