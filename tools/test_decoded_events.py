import json,tempfile,unittest
from pathlib import Path
from inspect_decoded_events import inspect
class DecodedEventTests(unittest.TestCase):
    def test_counts_phase_and_no_private_values(self):
        with tempfile.TemporaryDirectory() as temp:
            base={'iType':3,'stValue':{'stNotifyGive':{'astTLVInfo':[{'nType':3,'nLen':20,'stValue':{'stHandTiles':{'nSeat':1,'nHandTilesNum':3,'szTiles':[29,28,0]},'private_id':'fixture-token'}}]}}}
            p=Path(temp);(p/'entry-0001-seq-0001.tdr.json').write_text(json.dumps(base));base['iType']=50;(p/'entry-0001-seq-0002.tdr.json').write_text(json.dumps(base))
            result=inspect(temp);self.assertEqual(result['decoded_frame_count'],2)
            self.assertEqual(result['frames'][0]['hand_fields'][0]['nonzero_tiles'],2)
            self.assertEqual(result['frames'][0]['phase'],'before_hu');self.assertEqual(result['frames'][1]['phase'],'after_hu_or_finish')
            public=json.dumps(result);self.assertNotIn('fixture-token',public);self.assertNotIn('szTiles',public);self.assertNotIn('29',public)
    def test_invalid_hand_ownership_not_reported(self):
        with tempfile.TemporaryDirectory() as temp:
            (Path(temp)/'entry-0001-seq-0001.tdr.json').write_text(json.dumps({'iType':3,'nSeat':99,'nHandTilesNum':2,'szTiles':[12,13]}))
            self.assertEqual(inspect(temp)['frames'][0]['hand_fields'],[])
if __name__=='__main__':unittest.main()
