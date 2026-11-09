import json
import unittest

from index_json_gameplay import index_text


class JsonGameplayTests(unittest.TestCase):
    def test_multiple_documents_nested_strings_and_redaction(self):
        text = '[INFO] {"handTiles":[0,3],"private-sentinel":"secret-sentinel"} ' + json.dumps({
            'wrapper': json.dumps({'OtherHandCards': [0, 0]})
        })
        result = index_text(text, ['handTiles', 'OtherHandCards'])
        paths = {r['path']: r for r in result['recognized_fields']}
        self.assertIn('$/handTiles', paths)
        self.assertIn('$/$unknown/$json/OtherHandCards', paths)
        self.assertEqual(paths['$/handTiles']['numeric_array_shapes'][0]['positive_entries'], 1)
        self.assertEqual(result['counters']['json_documents'], 2)
        self.assertEqual(result['counters']['embedded_json_strings'], 1)
        self.assertNotIn('private-sentinel', json.dumps(result))
        self.assertNotIn('secret-sentinel', json.dumps(result))

    def test_aggregate_survives_sample_limit_and_counts_truncation(self):
        result = index_text('\n'.join(['[]'] * 205 + ['[4,5]']), [])
        self.assertEqual(result['counters']['root_numeric_arrays'], 206)
        self.assertEqual(len(result['root_numeric_array_samples']), 200)
        self.assertEqual(result['counters']['root_array_sample_truncations'], 6)
        self.assertTrue(any(s['positive_entries'] == 2 for s in result['root_numeric_array_shapes']))

    def test_malformed_prefix_does_not_hide_later_json(self):
        result = index_text('prefix {bad} {"DrawTiles":[]}', ['DrawTiles'])
        self.assertEqual(result['recognized_fields'][0]['path'], '$/DrawTiles')
        self.assertEqual(result['counters']['json_documents'], 1)


if __name__ == '__main__':
    unittest.main()
