"""Inventory supplied configs and patch without emitting arbitrary strings or data."""
import hashlib
import json
import pathlib
import zipfile
import zlib

from trace_native_file_sources import wire_fields

NAMES = {'MJGameBaseConfig.data', 'ModuleFanConfig.data', 'SvrNotifyTips.data', 'VedioRecordConfig.data', 'PatchIOS.bytes'}
LABELS = ('NextToDoHelper', 'HandleSelfRedraw', 'RevcDealCommand', 'RecvGameFinishCommand',
          'FanRecommendDetail', 'NewFanRecommendDataModel', 'MJGameContext', 'TDoWaitInfo')


def facts(raw):
    result = {'bytes': len(raw), 'sha256': hashlib.sha256(raw).hexdigest(),
              'known_code_name_mentions': {name: raw.count(name.encode()) + raw.count(name.encode('utf-16-le')) for name in LABELS}}
    for method, mode in [('zlib', 15), ('gzip', 31), ('raw_deflate', -15)]:
        try:
            decoder = zlib.decompressobj(mode)
            clear = decoder.decompress(raw, 4 * 1024 * 1024 + 1)
            if not decoder.eof or decoder.unused_data or decoder.unconsumed_tail or len(clear) > 4 * 1024 * 1024:
                continue
        except zlib.error:
            continue
        row = {'format': method, 'decoded_bytes': len(clear)}
        try:
            fields = wire_fields(clear)
            row['protobuf_top_level_field_numbers'] = sorted({n for n, k, v in fields})
            row['length_delimited_entries'] = sum(k == 2 for n, k, v in fields)
        except ValueError:
            row['protobuf_wire_parseable'] = False
        result.setdefault('whole_stream_decompression', []).append(row)
    return result


if __name__ == '__main__':
    rows = []
    with zipfile.ZipFile('private-input/device.zip') as device, zipfile.ZipFile('inputs/_Decrypted.ipa') as ipa:
        for index, entry in enumerate((i for i in device.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1):
            name = pathlib.PurePosixPath(entry.filename).name
            if name not in NAMES:
                continue
            raw = device.read(entry)
            bundled = [n for n in ipa.namelist() if '/Data/Raw/' in n and pathlib.PurePosixPath(n).name == name]
            comparisons = []
            for match in bundled:
                original = ipa.read(match)
                comparisons.append({'bytes': len(original), 'sha256': hashlib.sha256(original).hexdigest(), 'byte_identical': raw == original})
            rows.append({'entry': index, 'known_static_basename': name, **facts(raw), 'bundled_comparisons': comparisons})
    result = {'files': rows, 'limits': ['Only five known static code/config basenames are inventoried.', 'Wire parseability does not establish a schema or runtime use.', 'No arbitrary strings, field values, player identifiers or raw patch bytes are emitted.']}
    pathlib.Path('report/device/complete-config-facts.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
