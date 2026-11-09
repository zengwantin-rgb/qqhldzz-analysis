import struct
import unittest
import zlib
from inspect_xlog import inspect, frame, decode, LIMIT


def record(magic, data):
    return bytes([magic])+b'\x01\x00\x00\x00'+struct.pack('<I',len(data))+bytes(4 if magic<=5 else 64)+data+b'\x00'


class XlogTests(unittest.TestCase):
    def test_plain_and_compressed_chain(self):
        compressor=zlib.compressobj(wbits=-15)
        payload=compressor.compress(b'MSDK Login\n')+compressor.flush(zlib.Z_SYNC_FLUSH)
        r=inspect(record(9,payload)+record(8,b'HTTP\n'))
        self.assertEqual(r['frame_count'],2)
        self.assertEqual(r['known_field_mentions']['MSDK'],1)
        self.assertEqual(r['decoded_bytes'],16)
        self.assertEqual(r['deflate_frames_without_final_marker'],1)
        self.assertEqual(r['unparsed_suffix_bytes'],0)

    def test_encrypted_never_counted_as_plaintext(self):
        r=inspect(record(7,b'stHandTiles MSDK'))
        self.assertEqual(r['decoded_bytes'],0)
        self.assertEqual(r['decode_counts'],{'encrypted_variant':1})

    def test_truncation_and_false_candidate(self):
        self.assertIsNone(frame(record(8,b'hello')[:-1],0))
        self.assertEqual(inspect(b'garbage'+record(8,b'hello'))['status'],'no_validated_mars_frame_chain')

    def test_chain_prefix_and_suffix(self):
        r=inspect(b'prefix'+record(3,b'a')+record(3,b'b')+b'tail')
        self.assertEqual((r['prefix_bytes'],r['unparsed_suffix_bytes']),(6,4))

    def test_chunk_bounds(self):
        with self.assertRaises(ValueError): decode(5,b'\xff\xffshort')
        with self.assertRaises(ValueError): decode(5,b'\x01')

    def test_decompression_budget(self):
        c=zlib.compressobj(wbits=-15)
        data=c.compress(b'A'*(LIMIT+1))+c.flush()
        with self.assertRaises(ValueError): decode(9,data)

if __name__=='__main__': unittest.main()
