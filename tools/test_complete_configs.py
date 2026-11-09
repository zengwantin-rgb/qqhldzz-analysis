import struct
import unittest
import zlib

from inspect_complete_configs import base_config_rows, compare_base_configs, facts, runtime_config_matches


def varint(value):
    out = bytearray()
    while value >= 128:
        out.append((value & 127) | 128)
        value >>= 7
    return bytes(out + bytes([value]))


def fixture(rows):
    raw = b''
    for identifier, values in rows:
        value = values.encode()
        row = b'\x08' + varint(identifier) + b'\x12' + varint(len(value)) + value
        raw += b'\x0a' + varint(len(row)) + row
    return zlib.compress(raw)


class CompleteConfigTests(unittest.TestCase):
    def test_named_flags_and_row_differences_without_arbitrary_strings(self):
        old = fixture([(149, '4,136,0'), (1, '0')])
        new = fixture([(149, '4,136,1'), (2, '0')])
        result = compare_base_configs(new, old, 'case 2: afterTingShowAllHandTile = value == 1;')
        self.assertEqual(result['added_row_ids'], [2])
        self.assertEqual(result['removed_row_ids'], [1])
        self.assertEqual(result['changed_numeric_config_row_ids'], [149])
        self.assertEqual(result['section_comparisons'][0]['device_entries'], 2)
        self.assertFalse(result['section_comparisons'][0]['serialized_entries_identical'])
        self.assertEqual(result['target_game_rows'][0]['named_config_values']['afterTingShowAllHandTile'], 1)

    def test_trailing_data_and_duplicate_ids_rejected(self):
        with self.assertRaises(ValueError):
            base_config_rows(fixture([(149, '0')]) + b'trailer')
        with self.assertRaises(ValueError):
            base_config_rows(fixture([(149, '0'), (149, '1')]))

    def test_patch_magic_uses_recovered_constant(self):
        self.assertTrue(facts(struct.pack('<Q', 8881892042087324221))['ifix_magic_matches_recovered_loader'])
        self.assertFalse(facts(b'other-format')['ifix_magic_matches_recovered_loader'])

    def test_runtime_matching_reports_ambiguous_ids_without_guessing_game(self):
        rows = base_config_rows(fixture([(149, '4;136;0'), (398, '4;136;1'), (399, '4;136;1')]))
        result = runtime_config_matches(['MJGameBaseConfigDetail GetBaseConifgByGameID configValues = 4;136;1'], rows,
                                        'case 2: afterTingShowAllHandTile = value == 1;')
        self.assertEqual(result[0]['matching_device_row_ids'], [398, 399])
        self.assertEqual(result[0]['named_config_values']['afterTingShowAllHandTile'], 1)


if __name__ == '__main__':
    unittest.main()
