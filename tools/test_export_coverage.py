"""Synthetic regression cases; no device data is required."""
import io
import json
import pathlib
import tempfile
import unittest
import zipfile

from audit_export_coverage import audit, file_facts


class ExportCoverageTests(unittest.TestCase):
    def test_all_entries_and_missing_candidates(self):
        manifest = {
            'exported_files': ['files/c/Log398/a.log', 'files/c/TraceBinLogs/b.bin'],
            'scans': [{'files': [
                {'relative': 'Log398/a.log'},
                {'relative': 'TraceBinLogs/b.bin'},
                {'relative': 'Replay/c.data'},
            ]}],
        }
        with tempfile.TemporaryDirectory() as tmp:
            path = pathlib.Path(tmp) / 'fixture.zip'
            with zipfile.ZipFile(path, 'w') as z:
                z.writestr('manifest.json', json.dumps(manifest))
                z.writestr('files/c/Log398/a.log', 'ordinary text')
                z.writestr('files/c/TraceBinLogs/b.bin', b'\xff\x00iTileFace')
                z.writestr('files/c/extra.txt', 'private-sensitive-sentinel')
            result = audit(path)
        self.assertEqual(result['actual_export_count'], 3)
        self.assertEqual(result['unexported_candidate_count'], 1)
        self.assertEqual(result['undeclared_entry_count'], 1)
        self.assertEqual(result['files'][1]['known_field_mentions']['iTileFace'], 1)
        self.assertEqual(result['files'][1]['format'], 'non_utf8_binary')
        self.assertNotIn('private-sensitive-sentinel', json.dumps(result))
        self.assertNotIn('a.log', json.dumps(result))

    def test_binary_and_nested_archive_classification(self):
        self.assertEqual(file_facts(b'\x00\x01abc')['format'], 'utf8_with_binary_controls')
        stream = io.BytesIO()
        with zipfile.ZipFile(stream, 'w') as z:
            z.writestr('unpublished-name', 'unpublished-content')
        facts = file_facts(stream.getvalue())
        self.assertEqual(facts['format'], 'zip')
        self.assertEqual(facts['nested_entry_count'], 1)
        self.assertNotIn('unpublished-', json.dumps(facts))


if __name__ == '__main__':
    unittest.main()
