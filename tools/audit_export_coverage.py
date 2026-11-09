"""Audit every exported entry on Actions; never emit paths or file contents."""
import collections
import io
import json
import pathlib
import zipfile

LIMIT = 20 * 1024 * 1024
FIELDS = (
    'szGameBuf', 'gameBuf', 'nGameBuf', 'iOriginSize',
    'stUnitProtoValueBuf', 'stAbilityTileWallInfo', 'astTileInfos',
    'tileInfos', 'iTileFace', 'tileFace', 'stHandTiles', 'nHandTilesNum',
    'ExtraTiles', 'ProtoIntelligenceAdvice', 'IntelligenceAdviceTing',
    'ProtocolDataList', 'BinaryArray', 'ClassName', 'objectJson',
    'PlaybackRecord', 'RecordVersion', 'so_notify_msg', 'round_env',
)
PATH_HINTS = ('replay', 'record', 'tracebinlogs', 'crash', 'bugly', 'tss', 'tersafe',
              'msdk', 'gcloud', 'tsdk', 'unity', 'pandora', 'log398')
EXTENSIONS = {'log', 'xlog', 'clog', 'replay', 'trace', 'bin', 'data',
              'xml', 'json', 'txt', 'dat', 'bytes', 'patch', 'dll'}


def path_facts(name):
    lower = name.lower()
    extension = pathlib.PurePosixPath(lower).suffix.lstrip('.')
    return {'category': category(name),
            'extension': extension if extension in EXTENSIONS else 'other',
            'path_hints': [hint for hint in PATH_HINTS if hint in lower]}


def category(name):
    name = name.lower()
    if 'tracebinlogs/' in name:
        return 'trace_binary'
    if 'replay' in name or 'record' in name:
        return 'replay_or_record'
    if '/log398/' in '/' + name:
        return 'main_log'
    if '/gcloudsdklog/' in '/' + name:
        return 'cloud_log'
    if '/pandora/' in '/' + name:
        return 'activity_log'
    if 'patch' in name:
        return 'patch'
    if name.endswith('.data'):
        return 'configuration_or_data'
    return 'other'


def file_facts(raw):
    facts = {'bytes': len(raw)}
    if raw.startswith(b'PK\x03\x04'):
        facts['format'] = 'zip'
        try:
            with zipfile.ZipFile(io.BytesIO(raw)) as nested:
                entries = [i for i in nested.infolist() if not i.is_dir()]
                facts['nested_entry_count'] = len(entries)
                facts['nested_declared_bytes'] = sum(i.file_size for i in entries)
        except zipfile.BadZipFile:
            facts['format'] = 'invalid_zip_signature'
    elif raw.startswith(b'\x1f\x8b'):
        facts['format'] = 'gzip'
    elif raw.startswith(b'bplist00'):
        facts['format'] = 'binary_plist'
    else:
        try:
            text = raw.decode('utf-8-sig')
            controls = sum(ord(c) < 32 and c not in '\r\n\t' for c in text)
            if controls:
                facts['format'] = 'utf8_with_binary_controls'
                facts['control_characters'] = controls
            else:
                facts['format'] = 'utf8_text'
                facts['line_count'] = len(text.splitlines())
                try:
                    obj = json.loads(text)
                    facts['json_root_type'] = type(obj).__name__
                except (ValueError, RecursionError):
                    pass
        except UnicodeDecodeError:
            facts['format'] = 'non_utf8_binary'
            decoded = raw.decode('utf-8', errors='replace')
            facts['utf8_replacement_characters'] = decoded.count('\ufffd')
            facts['line_count_with_replacement'] = len(decoded.splitlines())
    facts['zero_byte_count'] = raw.count(b'\x00')
    # Byte searches cover all formats without exposing matched values.
    lower = raw.lower()
    facts['known_field_mentions'] = {
        key: lower.count(key.lower().encode('ascii')) for key in FIELDS
    }
    return facts


def audit(path):
    with zipfile.ZipFile(path) as archive:
        infos = [i for i in archive.infolist() if not i.is_dir()]
        if len(infos) > 10000 or sum(i.file_size for i in infos) > 64 * 1024 * 1024:
            raise ValueError('Input exceeds export audit limits')
        if archive.testzip() is not None:
            raise ValueError('ZIP CRC validation failed')
        manifest = json.loads(archive.read('manifest.json'))
        declared = manifest.get('exported_files', [])
        candidates = [f for s in manifest.get('scans', []) for f in s.get('files', [])]
        actual = [i for i in infos if i.filename != 'manifest.json']
        actual_names = {i.filename for i in actual}
        reported = manifest.get('coverage', {})
        coverage_status = reported.get('status') if reported.get('status') in ('complete', 'partial', 'manifest_only') else None
        selection_mode = manifest.get('selection_mode') if manifest.get('selection_mode') in ('manual', 'complete_candidates') else None
        expected_names = {'files/' + pathlib.PurePosixPath(f.get('container', '')).name + '/' + f.get('relative', '') for f in candidates}
        claim_verified = None
        if coverage_status == 'complete':
            claim_verified = (actual_names == expected_names and len(actual) == len(candidates)
                              and len(infos) == len({i.filename for i in infos})
                              and set(declared) == actual_names and len(declared) == len(actual)
                              and not manifest.get('skipped_files') and not manifest.get('errors')
                              and not any(s.get('truncated') or s.get('errors') for s in manifest.get('scans', [])))
        exported_categories = collections.Counter(category(i.filename) for i in actual)
        candidate_categories = collections.Counter(category(f.get('relative', '')) for f in candidates)
        rows = []
        for index, entry in enumerate(actual, 1):
            if entry.file_size > LIMIT:
                rows.append({'entry': index, 'category': category(entry.filename), 'skipped': 'size_limit'})
                continue
            rows.append({'entry': index, **path_facts(entry.filename), **file_facts(archive.read(entry))})
        return {
            'zip_crc_valid': True,
            'reported_selection_mode': selection_mode,
            'reported_coverage_status': coverage_status,
            'complete_claim_independently_verified': claim_verified,
            'manifest_declared_export_count': len(declared),
            'actual_export_count': len(actual),
            'duplicate_zip_entry_count': len(infos) - len({i.filename for i in infos}),
            'declared_but_missing_count': sum(n not in actual_names for n in declared),
            'undeclared_entry_count': sum(i.filename not in declared for i in actual),
            'candidate_count': len(candidates),
            'candidate_categories': dict(candidate_categories),
            'exported_categories': dict(exported_categories),
            'unexported_candidate_count': sum(
                not any(n.endswith('/' + f.get('relative', '')) for n in actual_names)
                for f in candidates
            ),
            'unexported_candidates': [
                {**path_facts(f.get('relative', '')),
                 'bytes': f.get('size') if isinstance(f.get('size'), int) else None}
                for f in candidates
                if not any(n.endswith('/' + f.get('relative', '')) for n in actual_names)
            ],
            'files': rows,
            'limits': [
                'Entry numbers identify archive order only; paths and contents are omitted.',
                'All exported entries are inventoried; missing candidates cannot be read from this ZIP.',
                'Known-field byte matches do not prove structured payloads or real opponent tiles.',
                'Binary formats are classified, not guessed or treated as decrypted messages.',
                'Nested ZIP entries are counted but their contents are not inspected.',
            ],
        }


if __name__ == '__main__':
    result = audit('private-input/device.zip')
    out = pathlib.Path('report/device')
    out.mkdir(parents=True, exist_ok=True)
    (out / 'export-coverage.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))
