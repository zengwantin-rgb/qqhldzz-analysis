import pathlib
import tempfile
import unittest
import zipfile

from analyze_round_logs import analyze_archive, analyze_lines


class MultipleMainLogTests(unittest.TestCase):
    def test_all_main_logs_are_analyzed_and_primary_is_explicit(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = pathlib.Path(tmp) / 'device.zip'
            with zipfile.ZipFile(path, 'w') as z:
                z.writestr('files/c/Log398/398_2026-11-09_21.log',
                           'RecvGameMsg msgType:ID_SO_NOTIFY_DEAL gameType:ModuleTuidaohu49\n'
                           'HandleWait WaitInfo.chFlag: {"chExCmd":0,"nExtraTilesNum":0,"ExtraTiles":[]}')
                z.writestr('files/c/Log398/398_2026-11-09_22.log',
                           'RecvGameMsg msgType:ID_SO_NOTIFY_DEAL gameType:ModuleTuidaohu49\n'
                           'HandleWait WaitInfo.chFlag: {"chExCmd":1,"nExtraTilesNum":1,"ExtraTiles":[7]}')
            result = analyze_archive(path, 0)
        self.assertEqual(result['main_log_count'], 2)
        self.assertEqual(result['primary_entry'], 2)
        self.assertEqual([r['wait_aggregate']['parsed_records'] for r in result['main_logs']], [1, 1])
        self.assertEqual(result['wait_aggregate']['nExtraTilesNum_values'], {'1': 1})

    def test_phase_does_not_leak_between_files(self):
        first = analyze_lines(['RecvGameMsg msgType:ID_SO_NOTIFY_HU gameType:ModuleTuidaohu49'], 0)
        second = analyze_lines(['HandleWait WaitInfo.chFlag: {"chExCmd":0,"ExtraTiles":[]}'], 0)
        self.assertEqual(first['dispatches'][0]['phase'], 'hu_notification')
        self.assertEqual(second['wait_messages'][0]['phase'], 'before_deal')


if __name__ == '__main__':
    unittest.main()
