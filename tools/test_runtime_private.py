import json,tempfile,unittest
from pathlib import Path
from inspect_runtime_private import inspect
class PrivateRuntimeTests(unittest.TestCase):
    def test_self_comparison_waits_for_ready_hand_and_omits_values(self):
        with tempfile.TemporaryDirectory() as temp:
            p=Path(temp)
            deal={'iType':1,'stValue':{'stNotifyDeal':{'nTilesNum':2,'szTiles':[31,32]}}}
            model={'status':'ok','phase':'live','self_seat':0,'seats':[{'view_seat':0,'hand':[]},{'view_seat':1,'hand':[0],'aux_fields':{'LastMJ':[-1],'curRedrawMjValue':[0]}}]}
            (p/'entry-0001-seq-0001.snapshot.json').write_text(json.dumps(deal));(p/'entry-0001-seq-0002.runtime.json').write_text(json.dumps(model))
            result=inspect(temp);self.assertEqual(result['self_hand_comparisons'][0]['status'],'model_not_ready_after_deal')
            self.assertEqual(result['opponent_auxiliary_value_categories']['LastMJ']['negative_one_sentinel'],1)
            model['seats'][0]['hand']=[32,31];(p/'entry-0001-seq-0002.runtime.json').write_text(json.dumps(model))
            result=inspect(temp);self.assertTrue(result['self_hand_comparisons'][0]['multiset_equal'])
            self.assertNotIn('szTiles',json.dumps(result));self.assertNotIn('[32, 31]',json.dumps(result))
    def test_no_model_between_packet_and_next_wire_is_not_compared(self):
        with tempfile.TemporaryDirectory() as temp:
            p=Path(temp);(p/'entry-0001-seq-0001.snapshot.json').write_text(json.dumps({'iType':8}))
            (p/'entry-0001-seq-0002.tdr').write_bytes(b'fixture')
            (p/'entry-0001-seq-0003.runtime.json').write_text(json.dumps({'status':'ok','seats':[]}))
            self.assertEqual(inspect(temp)['self_hand_comparisons'][0]['status'],'no_model_before_next_wire_event')
if __name__=='__main__':unittest.main()
