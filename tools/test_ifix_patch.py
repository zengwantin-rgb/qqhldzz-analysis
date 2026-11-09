import json
import struct
import unittest

from inspect_ifix_patch import IFIX_MAGIC, inspect_combined, Reader, readiness


def integer(n):
    return struct.pack('<i', n)


def string(value):
    data = value.encode()
    if len(data) >= 128:
        raise ValueError('Fixture string too long')
    return bytes([len(data)]) + data


OWNER = 'HappyMahjong.Game.NextToDoHelper, Fixture'
BRIDGE = 'IFix.TestBridge, Fixture'
INTERFACE = 'IFix.TestInterface, Fixture'
MAGIC = 1234567890


def block(anonymous=False):
    data = struct.pack('<Q', IFIX_MAGIC) + string(BRIDGE) + integer(2 if anonymous else 1) + string(OWNER)
    if anonymous:
        data += string(INTERFACE)
    data += integer(1) + integer(0) + integer(0)  # One VM method, no code or exception handlers.
    data += integer(0) + integer(1) + string('private-intern-sentinel')
    data += integer(0) * 2  # Fields and static fields.
    data += b''.join(integer(n) for n in [1, 0, 0, 0, 1, 1, 0, 0, 0]) if anonymous else integer(0)
    data += string(BRIDGE) + string(', Fixture') + integer(1)
    data += b'\0' + integer(0) + string('HandleWait') + integer(0) + integer(0)
    return data + integer(0)


class IfixPatchTests(unittest.TestCase):
    def test_complete_container_bindings_without_intern_values(self):
        raw = block()
        combined = struct.pack('<QHHii', MAGIC, 1, 1, 20, len(raw)) + raw
        result = inspect_combined(combined, MAGIC, {OWNER: 0, BRIDGE: 0})
        self.assertEqual(result['trailing_bytes'], 0)
        self.assertTrue(result['subpatches'][0]['fully_consumed'])
        self.assertEqual(result['subpatches'][0]['critical_owner_bindings'][0]['method'], 'HandleWait')
        self.assertNotIn('private-intern-sentinel', json.dumps(result))

    def test_invalid_spans_and_truncated_subpatch_rejected(self):
        raw = block()
        with self.assertRaises(ValueError):
            inspect_combined(struct.pack('<QHHii', MAGIC, 1, 1, 10, len(raw)) + raw, MAGIC, {OWNER: 0, BRIDGE: 0})
        with self.assertRaises(ValueError):
            inspect_combined(struct.pack('<QHHii', MAGIC, 1, 1, 20, len(raw)) + raw[:-1], MAGIC, {OWNER: 0, BRIDGE: 0})

    def test_reader_rejects_excessive_counts_and_strings(self):
        with self.assertRaises(ValueError):
            Reader(integer(-1)).count()
        with self.assertRaises(ValueError):
            Reader(b'\xff' * 5).string()

    def test_anonymous_slot_counts_use_recovered_type_metadata(self):
        raw = block(anonymous=True)
        combined = struct.pack('<QHHii', MAGIC, 1, 1, 20, len(raw)) + raw
        result = inspect_combined(combined, MAGIC, {OWNER: 0, BRIDGE: 0, INTERFACE: 2})
        self.assertEqual(result['subpatches'][0]['resolved_slot_types'][0]['declared_public_instance_methods'], 2)
        with self.assertRaises(ValueError):
            inspect_combined(combined, MAGIC, {OWNER: 0, BRIDGE: 0})

    def test_readiness_version_checks_do_not_emit_other_log_content(self):
        result = readiness([
            '2026-11-09 22:00:00 iFix ready version =8.19.112',
            'IFix version check false, unload private-user-sentinel',
            'PatchManagerLoad exception: private-token-sentinel',
        ], {'8.19.112'})
        self.assertTrue(result['events'][0]['matches_manifest_version'])
        self.assertEqual(result['counts']['version_check_unload'], 1)
        self.assertEqual(result['counts']['patch_load_exception'], 1)
        self.assertNotIn('private-', json.dumps(result))


class MissingArchivePatchTests(unittest.TestCase):
    def test_missing_patch_keeps_runtime_audit(self):
        import tempfile, zipfile, json
        from pathlib import Path
        from inspect_ifix_patch import inspect_archive
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'empty.zip'
            with zipfile.ZipFile(path,'w') as archive:
                archive.writestr('manifest.json',json.dumps({'app_version':{'build':'8.19.112'}}))
                archive.writestr('a.log','ordinary fixture')
            result=inspect_archive(path,0,{})
            self.assertEqual(result['status'],'device_patch_not_exported')
            self.assertEqual(len(result['runtime_readiness']),1)


if __name__ == '__main__':
    unittest.main()
