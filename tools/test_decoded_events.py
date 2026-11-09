import json,tempfile,unittest,zlib
from pathlib import Path
from inspect_decoded_events import inspect,embedded,proto_fields,lz4_block
class DecodedEventTests(unittest.TestCase):
    def test_counts_phase_and_no_private_values(self):
        with tempfile.TemporaryDirectory() as temp:
            base={'iType':3,'stValue':{'stNotifyGive':{'astTLVInfo':[{'nType':3,'nLen':20,'stValue':{'stHandTiles':{'nSeat':1,'nHandTilesNum':3,'szTiles':[29,28,0]},'private_id':'fixture-token'}}]}}}
            p=Path(temp);(p/'entry-0001-seq-0001.tdr.json').write_text(json.dumps(base));base['iType']=50;(p/'entry-0001-seq-0002.tdr.json').write_text(json.dumps(base))
            result=inspect(temp);self.assertEqual(result['decoded_frame_count'],2)
            self.assertEqual(result['frames'][0]['hand_fields'][0]['nonzero_tiles'],2)
            self.assertEqual(result['frames'][0]['phase'],'before_hu');self.assertEqual(result['frames'][1]['phase'],'after_hu_or_finish')
            public=json.dumps(result);self.assertNotIn('fixture-token',public);self.assertNotIn('szTiles',public);self.assertNotIn('29',public)
    def test_bounded_embedded_advice_buffer_and_wire_lengths(self):
        raw=bytes([10,2,8,1]);compressed=zlib.compress(raw)
        data={'nGameBuf':len(compressed),'iOriginSize':len(raw),'szGameBuf':[b if b<128 else b-256 for b in compressed]}
        result=embedded(data);self.assertEqual(result['top_level_field_counts'],{'1':1});self.assertTrue(result['only_known_advice_field'])
        data['iOriginSize']=4*1024*1024+1;self.assertEqual(embedded(data)['status'],'invalid_buffer_shape')
        with self.assertRaises(ValueError):proto_fields(bytes([10,100]))

    def test_lz4_literals_overlap_and_bad_offsets(self):
        self.assertEqual(lz4_block(bytes([80])+b'hello',5),b'hello')
        self.assertEqual(lz4_block(bytes([16])+b'a'+bytes([1,0,80])+b'hello',10),b'aaaaahello')
        with self.assertRaises(ValueError):lz4_block(bytes([0,0,0]),4)
        with self.assertRaises(ValueError):lz4_block(bytes([80])+b'hello',4)
        raw=bytes([10,2,8,1]);result=embedded({'nGameBuf':5,'iOriginSize':4,'szGameBuf':[64]+list(raw)})
        self.assertEqual(result['compression'],'lz4_block');self.assertTrue(result['only_known_advice_field'])

    def test_invalid_hand_ownership_not_reported(self):
        with tempfile.TemporaryDirectory() as temp:
            (Path(temp)/'entry-0001-seq-0001.tdr.json').write_text(json.dumps({'iType':3,'nSeat':99,'nHandTilesNum':2,'szTiles':[12,13]}))
            self.assertEqual(inspect(temp)['frames'][0]['hand_fields'],[])
if __name__=='__main__':unittest.main()
