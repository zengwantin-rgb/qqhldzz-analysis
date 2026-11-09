import json
import unittest
from inspect_round_anomalies import analyze

class RoundAnomalyTests(unittest.TestCase):
    def test_plain_wait_marker_is_distinct_from_json(self):
        result=analyze(['HandleWait WaitInfo.chFlag: diagnostic-marker'],[])
        row=result['marker_contexts'][0]
        self.assertFalse(row['marker_line_has_json_start'])
        self.assertFalse(row['marker_line_has_parsed_object'])

    def test_hand_count_and_shape_do_not_emit_payload(self):
        result=analyze(['EM_UP_TLV_HAND_TILES','nHandTilesNum: 13','{"stHandTiles":[19,21,25],"secret":"sensitive-user-value"}'],['stHandTiles'])
        self.assertEqual(result['marker_contexts'][1]['hand_count_literals'],[13])
        self.assertNotIn('sensitive-user-value',json.dumps(result))
        self.assertEqual(result['marker_contexts'][0]['context_structure']['recognized_fields'][0]['array_lengths'],{'3':1})

    def test_explicit_hand_tlv_is_checked_against_self(self):
        tiles=','.join(str(n) for n in range(1,14))
        lines=['Recv Deal Self mjValue:'+tiles,
               'RecvGameMsg msgType:ID_SO_TABLE_ALL_INFO',
               'Replay, EM_UP_TLV_HAND_TILES nSeat = 0 tiles: '+tiles,
               'SetPlayerHandTile nHandTilesNum 13 inHandListCount 0',
               'SetPlayerHandTile info:'+tiles,
               'HandleSelfRedraw {"nSeatID":0,"chTile":7}']
        result=analyze(lines,[])
        checks=result['hand_tlv_payload_checks']
        self.assertTrue(checks[0]['matches_initial_self_multiset'])
        self.assertTrue(checks[0]['agrees_with_self_draw_seat'])
        self.assertEqual(checks[0]['source_dispatch'],'ID_SO_TABLE_ALL_INFO')
        self.assertTrue(checks[1]['matches_nearby_explicit_tlv'])
        self.assertEqual(result['self_hand_restore_counts'][0]['declared_count'],13)

if __name__=='__main__':unittest.main()
