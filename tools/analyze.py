import hashlib, json, pathlib, plistlib, struct, zipfile

root = pathlib.Path('inputs')
out = pathlib.Path('report')
out.mkdir(exist_ok=True)
def digest(data):
    return hashlib.sha256(data).hexdigest()
def macho(data):
    magic = data[:4]
    if magic in (b'\xca\xfe\xba\xbe', b'\xca\xfe\xba\xbf'):
        n = struct.unpack_from('>I', data, 4)[0]
        wide = magic == b'\xca\xfe\xba\xbf'
        result = []
        for i in range(n):
            fmt = '>IIQQII' if wide else '>IIIII'
            row = struct.unpack_from(fmt, data, 8+i*struct.calcsize(fmt))
            result.extend(macho(data[row[2]:row[2]+row[3]]))
        return result
    if magic not in (b'\xcf\xfa\xed\xfe', b'\xce\xfa\xed\xfe'):
        return [{'format': 'unrecognized', 'magic': magic.hex()}]
    wide = magic == b'\xcf\xfa\xed\xfe'
    cpu, subtype, kind, count, size, flags = struct.unpack_from('<6I', data, 4)
    pos = 32 if wide else 28
    result = {'cpu': hex(cpu), 'subtype': subtype, 'filetype': kind, 'commands': count, 'encryption': [], 'dependencies': []}
    for _ in range(count):
        cmd, length = struct.unpack_from('<II', data, pos)
        if length < 8 or pos+length > len(data):
            raise ValueError('Malformed Mach-O load command')
        if cmd in (0x21, 0x2c):
            offset, size, cryptid = struct.unpack_from('<III', data, pos+8)
            result['encryption'].append({'offset': offset, 'size': size, 'cryptid': cryptid})
        if cmd in (0xc, 0x18, 0x80000018, 0x8000001f, 0x80000023):
            off = struct.unpack_from('<I', data, pos+8)[0]
            result['dependencies'].append(data[pos+off:pos+length].split(b'\0')[0].decode(errors='replace'))
        pos += length
    return [result]

with zipfile.ZipFile(root/'_Decrypted.ipa') as z:
    bad = z.testzip()
    assert bad is None, f'ZIP CRC failure: {bad}'
    names = z.namelist()
    app = next(n for n in names if n.startswith('Payload/') and n.count('/') == 2 and n.endswith('/Info.plist')).rsplit('/', 1)[0]
    info_data = z.read(app+'/Info.plist')
    info = plistlib.loads(info_data)
    standalone_info = plistlib.loads((root/'Info.plist').read_bytes())
    unity = (root/'UnityFramework').read_bytes()
    packed_unity = z.read(app+'/Frameworks/UnityFramework.framework/UnityFramework')
    metadata_name = next(n for n in names if n.endswith('/global-metadata.dat'))
    metadata = z.read(metadata_name)
    main = z.read(app+'/'+info['CFBundleExecutable'])
    framework_info = plistlib.loads(z.read(app+'/Frameworks/UnityFramework.framework/Info.plist'))
    r = {
        'identity': {k: info.get(k) for k in ['CFBundleIdentifier','CFBundleDisplayName','CFBundleExecutable','CFBundleShortVersionString','CFBundleVersion','MinimumOSVersion','DTSDKName','DTXcode']},
        'zip_crc_ok': True, 'zip_entry_count': len(names),
        'standalone_plist_matches': standalone_info == info,
        'standalone_unity_matches': unity == packed_unity,
        'unity_sha256': digest(unity), 'main_sha256': digest(main),
        'unity_macho': macho(unity), 'main_macho': macho(main),
        'unity_framework_version': framework_info.get('CFBundleVersion'),
        'metadata': {'path': metadata_name, 'size': len(metadata), 'magic': hex(struct.unpack_from('<I',metadata)[0]), 'version': struct.unpack_from('<I',metadata,4)[0], 'sha256': digest(metadata)},
        'transport_security': info.get('NSAppTransportSecurity'),
        'permission_descriptions': {k:v for k,v in info.items() if k.startswith('NS') and k.endswith('UsageDescription')},
        'frameworks': sorted(n for n in names if n.startswith(app+'/Frameworks/') and n.endswith('.framework/Info.plist')),
        'limits': ['No application source project is supplied, so rebuilding the original game is unavailable.', 'cryptid=0 describes the header; it does not prove all code is usable.', 'Static inspection does not establish runtime behavior or exploitable vulnerabilities.'],
    }
(out/'analysis.json').write_text(json.dumps(r, ensure_ascii=False, indent=2)+'\n')
lines = ['# 静态分析结果', '', '```json', json.dumps(r, ensure_ascii=False, indent=2), '```']
(out/'REPORT.md').write_text('\n'.join(lines)+'\n')
assert r['standalone_plist_matches'], 'Standalone plist differs from IPA'
assert r['standalone_unity_matches'], 'Standalone UnityFramework differs from IPA'
assert r['metadata']['magic'] == '0xfab11baf', 'Nonstandard IL2CPP metadata header'
print(json.dumps(r, ensure_ascii=False, indent=2))
