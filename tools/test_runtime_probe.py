import json
import unittest
from inspect_runtime_probe import analyze


def encoded(*rows):
    return b'MJRuntimeProbe/1\n'+b'\n'.join(json.dumps({'kind':'MJRuntimeProbe','schema':1,'probe_version':'0.1.0',**r}).encode() for r in rows)+b'\n'


class RuntimeProbeTests(unittest.TestCase):
    def test_latest_session_does_not_inherit_old_success(self):
        result=analyze(encoded({'stage':'library_loaded'},{'stage':'runtime_snapshot','lookup_status':'metadata_checked'}, {'stage':'library_loaded'}))
        self.assertEqual(result['session_count'],2)
        self.assertFalse(result['latest_session_metadata_checked'])

    def test_unknown_fields_and_strings_are_not_emitted(self):
        result=analyze(encoded({'stage':'library_loaded','capturing_game_messages':False},
            {'stage':'runtime_snapshot','lookup_status':'metadata_checked','native_apis':{'il2cpp_domain_get':1,'secret':'private-user-token'},
             'classes':[{'type':'HappyMahjong.TSDK4CSharpService','found':True,'secret':'private-user-token',
                         'methods':[{'name':'get_dispatcher','parameter_count':0},{'name':'private-user-token','parameter_count':999}]},
                        {'type':'private-user-token','found':True}]}))
        self.assertTrue(result['latest_session_metadata_checked'])
        self.assertNotIn('private-user-token',json.dumps(result))
        self.assertTrue(result['records'][1]['native_apis']['il2cpp_domain_get'])
        self.assertFalse(result['game_message_subscription_implemented'])

    def test_bad_header_truncated_record_and_limit(self):
        self.assertEqual(analyze(b'private-user-token')['status'],'unrecognized_probe_header')
        self.assertEqual(analyze(encoded({'stage':'library_loaded'})+b'{incomplete')['invalid_record_count'],1)
        self.assertEqual(analyze(b'A'*(64*1024+1))['status'],'probe_file_limit_exceeded')

if __name__=='__main__':unittest.main()
