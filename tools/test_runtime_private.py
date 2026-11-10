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
            result=inspect(temp);self.assertEqual(result['self_hand_comparisons'][0]['status'],'model_not_ready_after_deal');self.assertNotIn('multiset_equal',result['self_hand_comparisons'][0])
            self.assertEqual(result['opponent_auxiliary_value_categories']['LastMJ']['negative_one_sentinel'],1)
            model['seats'][0]['hand']=[32,31];(p/'entry-0001-seq-0002.runtime.json').write_text(json.dumps(model))
            result=inspect(temp);self.assertTrue(result['self_hand_comparisons'][0]['multiset_equal'])
            self.assertNotIn('szTiles',json.dumps(result));self.assertNotIn('[32, 31]',json.dumps(result))
    def test_cache_classification_does_not_publish_values_or_paths(self):
        with tempfile.TemporaryDirectory() as temp:
            p=Path(temp)
            model={'status':'ok','seats':[{'view_seat':0,'hand':[31,32]}],'cache_candidates':[{'source_type':'HappyMahjong.Game.FanRecommendDetail','field':'private.root.curTilesInHand','values':[32,31]},{'source_type':'HappyMahjong.Game.FanRecommendDetail','field':'private.root.proximateMjTiles','values':[33,34]},{'source_type':'secret','field':'secret','values':[35]}]}
            (p/'entry-0001-seq-0001.runtime.json').write_text(json.dumps(model))
            result=inspect(temp);counts=result['cache_candidate_classification']['counts']
            self.assertEqual(counts['recommendation_hand_matches_current_self'],1)
            self.assertEqual(counts['recommendation_target'],1);self.assertEqual(counts['other_candidate'],1)
            for secret in ('private.root','secret','[32, 31]','[33, 34]'):self.assertNotIn(secret,json.dumps(result))
    def test_no_model_between_packet_and_next_wire_is_not_compared(self):
        with tempfile.TemporaryDirectory() as temp:
            p=Path(temp);(p/'entry-0001-seq-0001.snapshot.json').write_text(json.dumps({'iType':8}))
            (p/'entry-0001-seq-0002.tdr').write_bytes(b'fixture')
            (p/'entry-0001-seq-0003.runtime.json').write_text(json.dumps({'status':'ok','seats':[]}))
            self.assertEqual(inspect(temp)['self_hand_comparisons'][0]['status'],'no_model_before_next_wire_event')
if __name__=='__main__':unittest.main()
