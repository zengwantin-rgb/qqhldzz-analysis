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

if __name__=='__main__':unittest.main()
