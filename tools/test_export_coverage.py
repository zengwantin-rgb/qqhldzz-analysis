"""Synthetic regression cases; no device data is required."""
import io
import json
import pathlib
import tempfile
import unittest
import zipfile
import base64
import subprocess
import sys
import zlib
import plistlib

from audit_export_coverage import audit, file_facts
from trace_native_file_sources import inspect, small_binary_facts


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

    def test_payload_probe_reads_second_text_file(self):
        script = pathlib.Path(__file__).with_name('probe_embedded_payloads.py').resolve()
        with tempfile.TemporaryDirectory() as tmp:
            root = pathlib.Path(tmp)
            (root / 'private-input').mkdir()
            with zipfile.ZipFile(root / 'private-input/device.zip', 'w') as z:
                z.writestr('manifest.json', '{}')
                z.writestr('files/c/Log398/first.log', 'ordinary text')
                z.writestr('files/c/record.dat', b'\x00' * 16)
                z.writestr('files/c/second.txt', json.dumps({
                    'gameBuf': base64.b64encode(zlib.compress(b'\x0a\x00')).decode('ascii')
                }))
            subprocess.run([sys.executable, str(script)], cwd=root, check=True, capture_output=True)
            result = json.loads((root / 'report/device/embedded-payload-probe.json').read_text())
        self.assertEqual(result['text_entries_scanned'], [1, 3])
        self.assertEqual(result['binary_entries_not_json_decoded'], [2])
        self.assertEqual(len(result['known_buffer_candidates']), 1)
        candidate = result['known_buffer_candidates'][0]
        self.assertEqual(candidate['entry'], 3)
        self.assertEqual(candidate['decompression'][0]['defined_advice_field_count'], 1)

    def test_native_file_reference_and_redaction(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = pathlib.Path(tmp)
            device = root / 'device.zip'
            ipa = root / 'app.ipa'
            with zipfile.ZipFile(device, 'w') as z:
                z.writestr('manifest.json', json.dumps({'scans': [{'files': [
                    {'relative': 'private-sensitive-dir/record.dat', 'size': 16}
                ]}]}))
                z.writestr('files/c/private-sensitive-dir/record.dat', b'\x00' * 16)
            with zipfile.ZipFile(ipa, 'w') as z:
                z.writestr('Payload/test.app/Info.plist', plistlib.dumps({'CFBundleExecutable': 'test'}))
                z.writestr('Payload/test.app/test', b'\0record.dat\0adjacent-secret-sentinel\0')
            result = inspect(device, ipa)
        self.assertEqual(result['references'][0]['matched_static_basename'], 'record.dat')
        self.assertTrue(result['candidates'][0]['exported'])
        self.assertEqual(result['small_record_file_checks'][0]['bytes'], 16)
        self.assertNotIn('private-sensitive-dir', json.dumps(result))
        self.assertNotIn('adjacent-secret-sentinel', json.dumps(result))

    def test_compression_requires_complete_stream(self):
        compressed = zlib.compress(b'synthetic')
        self.assertTrue(small_binary_facts(compressed)['whole_stream_compression_matches'])
        self.assertFalse(small_binary_facts(compressed + b'trailer')['whole_stream_compression_matches'])
        self.assertFalse(small_binary_facts(compressed[:-1])['whole_stream_compression_matches'])


if __name__ == '__main__':
    unittest.main()
