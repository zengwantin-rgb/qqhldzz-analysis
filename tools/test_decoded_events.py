import json,tempfile,unittest,zlib
from pathlib import Path
from inspect_decoded_events import inspect,embedded,proto_fields,lz4_block,advice_schema,lua_shape
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
        result=embedded(data);self.assertEqual(result['top_level_field_counts'],{'1':1});self.assertTrue(result['only_known_advice_field']);self.assertEqual(result['advice_schema']['unknown_field_count'],0)
        data['iOriginSize']=4*1024*1024+1;self.assertEqual(embedded(data)['status'],'invalid_buffer_shape')
        with self.assertRaises(ValueError):proto_fields(bytes([10,100]))

    def test_lz4_literals_overlap_and_bad_offsets(self):
        self.assertEqual(lz4_block(bytes([80])+b'hello',5),b'hello')
        self.assertEqual(lz4_block(bytes([16])+b'a'+bytes([1,0,80])+b'hello',10),b'aaaaahello')
        with self.assertRaises(ValueError):lz4_block(bytes([0,0,0]),4)
        with self.assertRaises(ValueError):lz4_block(bytes([80])+b'hello',4)
        raw=bytes([10,2,8,1]);result=embedded({'nGameBuf':5,'iOriginSize':4,'szGameBuf':[64]+list(raw)})
        self.assertEqual(result['compression'],'lz4_block');self.assertTrue(result['only_known_advice_field'])

    def test_unknown_nested_advice_tags_are_reported(self):
        result=advice_schema(bytes([10,2,80,1]));self.assertEqual(result['unknown_field_count'],1);self.assertTrue(result['parsed'])

    def test_direct_deal_shapes_and_private_recommendation_comparison(self):
        with tempfile.TemporaryDirectory() as temp:
            p=Path(temp);name='entry-0002-seq-0003.tdr.json'
            data={'iType':1,'stValue':{'stNotifyDeal':{'nTilesNum':3,'szTiles':[987654321,12,13],'targets':[{'CurHandTiles':[13,12,987654321]},{'CurHandTiles':[1,2,3]}],'players':[{'nSeat':1,'nHandTilesNum':13}]}}}
            (p/name).write_text(json.dumps(data));(p/'capture-formats.json').write_text(json.dumps({name:'public_field_snapshot'}))
            result=inspect(temp);frame=result['frames'][0];details=frame['snapshot_details']
            self.assertEqual(frame['source_encoding'],'public_field_snapshot')
            self.assertEqual(details['deal_self_tiles']['recommendation_hands_compared'],2)
            self.assertEqual(details['deal_self_tiles']['recommendation_hands_matching'],1)
            self.assertFalse(details['seat_hand_counts'][0]['has_tile_array'])
            self.assertNotIn('987654321',json.dumps(result))

    def test_config_shape_does_not_publish_keys_or_values(self):
        raw=b'{"private-user-token":12345678,"randomSeed":42,"tiles":[1,2]}'
        result=lua_shape({'nLuaBuffLen':len(raw),'szLuaBuff':list(raw)})
        self.assertEqual(result['gameplay_key_signals']['seed'],1)
        self.assertEqual(result['numeric_array_lengths'],[2])
        self.assertNotIn('private-user-token',json.dumps(result));self.assertNotIn('12345678',json.dumps(result))

    def test_lua_rule_vocabulary_validation_without_execution(self):
        raw=b'return condition_7() and (condition_3() or condition_4())\0'
        result=lua_shape({'iLuaType':1,'nLuaBuffLen':len(raw),'szLuaBuff':list(raw)})
        self.assertEqual(result['status'],'known_rule_condition_expression');self.assertEqual(result['condition_reference_count'],3)
        raw=b'return condition_7() or private_player_token'
        result=lua_shape({'iLuaType':1,'nLuaBuffLen':len(raw),'szLuaBuff':list(raw)})
        self.assertEqual(result['status'],'unclassified_utf8');self.assertNotIn('private_player_token',json.dumps(result))

    def test_invalid_hand_ownership_not_reported(self):
        with tempfile.TemporaryDirectory() as temp:
            (Path(temp)/'entry-0001-seq-0001.tdr.json').write_text(json.dumps({'iType':3,'nSeat':99,'nHandTilesNum':2,'szTiles':[12,13]}))
            self.assertEqual(inspect(temp)['frames'][0]['hand_fields'],[])
if __name__=='__main__':unittest.main()
