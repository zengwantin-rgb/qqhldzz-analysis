"""Compare the new export to its predecessor; publish only bounded diagnostics."""
import collections
import hashlib
import json
import pathlib
import re
import zipfile
from audit_export_coverage import category, file_facts


def boolean(value):
    return value if type(value) is bool else None


def inspect(current, previous=None):
    old = {}
    if previous:
        with zipfile.ZipFile(previous) as archive:
            for index, item in enumerate((i for i in archive.infolist() if not i.is_dir() and i.filename != 'manifest.json'), 1):
                old[item.filename] = (index, hashlib.sha256(archive.read(item)).digest())
    with zipfile.ZipFile(current) as archive:
        manifest = json.loads(archive.read('manifest.json'))
        entries = [i for i in archive.infolist() if not i.is_dir() and i.filename != 'manifest.json']
        if len(entries) > 10000 or sum(i.file_size for i in entries) > 64 * 1024 * 1024:
            raise ValueError('Diagnostic input limit')
        scans = manifest.get('scans', [])
        diagnostics = manifest.get('trace_diagnostics', [])
        if not isinstance(diagnostics, list):
            diagnostics = []
        safe = []
        activation = None
        for diagnostic in diagnostics:
            if not isinstance(diagnostic, dict):
                continue
            level = diagnostic.get('disk_trace_level')
            started = diagnostic.get('activation_time')
            valid_started = type(started) in (int, float) and 1577836800 <= started <= 2145916800
            safe.append({'reported_preference_present': boolean(diagnostic.get('preference_present')),
                         'reported_disk_trace_level': level if type(level) is int and -1 <= level <= 3 else None,
                         'reported_restore_available': boolean(diagnostic.get('restore_available')),
                         'reported_backup_file_present': boolean(diagnostic.get('backup_file_present')),
                         'reported_runtime_effect_verified': boolean(diagnostic.get('runtime_effect_verified')),
                         'activation_time_present': valid_started})
            if len(scans) == 1 and len(diagnostics) == 1 and valid_started:
                activation = started
        modified = {}
        for scan in scans:
            for item in scan.get('files', []):
                path = 'files/' + pathlib.PurePosixPath(item.get('container', '')).name + '/' + item.get('relative', '')
                modified[path] = item.get('modified')
        traces = []
        changes = []
        for index, item in enumerate(entries, 1):
            raw = archive.read(item)
            prior = old.get(item.filename)
            changes.append({'entry': index, 'category': category(item.filename),
                            'previous_entry': prior[0] if prior else None,
                            'comparison': 'unchanged' if prior and hashlib.sha256(raw).digest() == prior[1] else 'modified' if prior else 'added' if previous else 'not_compared'})
            if category(item.filename) != 'trace_binary':
                continue
            timestamp = modified.get(item.filename)
            newer = (timestamp >= activation if activation is not None and type(timestamp) in (int, float) else None)
            traces.append({'entry': index, 'nonempty': bool(raw),
                           'modified_at_or_after_activation': newer, **file_facts(raw)})
        version = manifest.get('tool_version')
        version = version if isinstance(version, str) and re.fullmatch(r'[0-9]{1,3}(?:\.[0-9]{1,3}){1,2}', version) else None
        return {'reported_tool_version': version, 'container_scan_count': len(scans),
                'diagnostic_state_count': len(safe), 'reported_diagnostics': safe,
                'trace_file_count': len(traces), 'nonempty_trace_file_count': sum(t['nonempty'] for t in traces),
                'nonempty_trace_files_updated_after_activation': sum(t['nonempty'] and t['modified_at_or_after_activation'] is True for t in traces) if activation is not None else None,
                'trace_files': traces, 'comparison_available': bool(previous),
                'file_changes': changes, 'change_counts': dict(collections.Counter(row['comparison'] for row in changes)),
                'previous_files_absent_from_current_count': sum(name not in {item.filename for item in entries} for name in old),
                'limits': ['Diagnostic values are reports from the exporter, not an independent in-process preference read.',
                           'A requested disk level or updated file does not establish runtime trace semantics or opponent hand visibility.',
                           'Only one-container, one-diagnostic manifests can attribute modification times to an activation.',
                           'Byte equality is compared privately; file paths, digests, raw payloads and actual tile values are omitted.',
                           'No trace file in an export does not prove that no unexported or transient trace ever existed.']}


if __name__ == '__main__':
    prior = pathlib.Path('private-input/previous.zip')
    result = inspect('private-input/device.zip', prior if prior.exists() else None)
    out = pathlib.Path('report/device'); out.mkdir(parents=True, exist_ok=True)
    (out/'phone-diagnostics.json').write_text(json.dumps(result, indent=2)+'\n')
    print(json.dumps({key: result[key] for key in ('reported_tool_version','reported_diagnostics','trace_file_count','nonempty_trace_files_updated_after_activation','change_counts')}, indent=2))
