"""Bounded Mars framing probe; private plaintext is never written or printed.
Format reference: Tencent/mars/mars/xlog/crypt/decode_mars_nocrypt_log_file.py.
Encrypted and zstd records are classified, not guessed to be plaintext.
"""
import collections
import json
import pathlib
import struct
import zipfile
import zlib

MAGICS = set(range(3, 14))
ENCRYPTED = {6, 7, 10, 12}
LIMIT = 4 * 1024 * 1024
KEYS = ('ID_SO_NOTIFY_DEAL', 'ID_SO_NOTIFY_WAIT', 'ID_SO_NOTIFY_FINISH',
        'HandleSelfRedraw', 'ExtraTiles', 'stHandTiles', 'nHandTilesNum',
        'stAbilityTileWallInfo', 'stPlayerTotalHandAbilityTileInfo',
        'stUnitProtoValueBuf', 'ProtocolDataList', 'MSDK', 'Login', 'HTTP')


def frame(raw, offset):
    if offset >= len(raw) or raw[offset] not in MAGICS:
        return None
    magic = raw[offset]
    header = 13 if magic <= 5 else 73
    if offset + header > len(raw):
        return None
    length = struct.unpack_from('<I', raw, offset + 5)[0]
    end = offset + header + length
    if end >= len(raw) or raw[end] != 0:
        return None
    return magic, offset + header, end, end + 1


def inflate(raw):
    decoder = zlib.decompressobj(-15)
    clear = decoder.decompress(raw, LIMIT + 1)
    if len(clear) > LIMIT or decoder.unconsumed_tail or decoder.unused_data:
        raise ValueError('bounded or trailing deflate')
    # Xlog's per-buffer sync flush may intentionally omit the final stream marker.
    return clear, decoder.eof


def decode(magic, payload):
    if magic in ENCRYPTED:
        return None, 'encrypted_variant', None
    if magic in (11, 13):
        return None, 'zstd_not_decoded', None
    if magic == 5:
        pos = 0
        chunks = []
        while pos < len(payload):
            if pos + 2 > len(payload):
                raise ValueError('truncated chunk length')
            length = struct.unpack_from('<H', payload, pos)[0]
            pos += 2
            if pos + length > len(payload):
                raise ValueError('truncated chunk')
            chunks.append(payload[pos:pos + length])
            pos += length
        clear, eof = inflate(b''.join(chunks))
        return clear, 'chunked_deflate', eof
    if magic in (4, 9):
        clear, eof = inflate(payload)
        return clear, 'deflate', eof
    return payload, 'uncompressed', None


def inspect(raw):
    # Require two adjacent valid frames, or one frame covering the complete file.
    start = None
    for pos in range(len(raw)):
        f = frame(raw, pos)
        if f and ((pos == 0 and f[3] == len(raw)) or frame(raw, f[3])):
            start = pos
            break
    if start is None:
        return {'bytes': len(raw), 'status': 'no_validated_mars_frame_chain'}
    pos = start
    counts = collections.Counter()
    magics = collections.Counter()
    mentions = collections.Counter({key: 0 for key in KEYS})
    decoded_bytes = printable = replacement = nonfinal = records = 0
    while pos < len(raw):
        f = frame(raw, pos)
        if f is None:
            break
        magic, begin, end, nxt = f
        magics[str(magic)] += 1
        records += 1
        try:
            clear, kind, eof = decode(magic, raw[begin:end])
            if clear is not None:
                if decoded_bytes + len(clear) > LIMIT:
                    counts['total_decode_limit'] += 1
                    break
                decoded_bytes += len(clear)
                printable += sum(b in (9, 10, 13) or 32 <= b < 127 for b in clear)
                text = clear.decode('utf-8', errors='replace')
                replacement += text.count('\ufffd')
                for key in KEYS:
                    mentions[key] += text.count(key)
                nonfinal += eof is False
            counts[kind] += 1
        except (ValueError, zlib.error):
            counts['decode_error'] += 1
        pos = nxt
    return {'bytes': len(raw), 'status': 'validated_mars_frame_chain',
            'prefix_bytes': start, 'unparsed_suffix_bytes': len(raw)-pos,
            'frame_count': records, 'magic_counts': dict(magics),
            'decode_counts': dict(counts), 'decoded_bytes': decoded_bytes,
            'decoded_ascii_printable_bytes': printable,
            'decoded_utf8_replacement_count': replacement,
            'deflate_frames_without_final_marker': nonfinal,
            'known_field_mentions': dict(mentions)}


def main():
    rows = []
    with zipfile.ZipFile('private-input/device.zip') as archive:
        entries = [i for i in archive.infolist() if not i.is_dir() and i.filename != 'manifest.json']
        for entry, info in enumerate(entries, 1):
            if info.filename.lower().endswith('.xlog'):
                rows.append({'entry': entry, **inspect(archive.read(info))})
    result = {'files': rows, 'limits': [
        'Framing follows the public Tencent Mars format; no arbitrary decryption is attempted.',
        'Encrypted variants are never interpreted as plaintext.',
        'Plaintext stays in runner memory; only counts and allowlisted key mentions are emitted.',
        'A key mention alone does not establish a structured gameplay payload or opponent hand.',
        'No validated chain does not exclude a different SDK format or encryption wrapper.',
        'Deflate sync-flushed records need not contain a final stream marker; nonfinal counts are explicit.'
    ]}
    pathlib.Path('report/device/xlog-summary.json').write_text(json.dumps(result, indent=2)+'\n')


if __name__ == '__main__':
    main()
