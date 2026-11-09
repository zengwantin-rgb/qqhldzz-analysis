import json
import tempfile
import unittest
import zipfile
from pathlib import Path
from inspect_phone_diagnostics import inspect


def fixture(path, files, diagnostics):
    rows=[{'container':'/fixture/c','relative':name,'modified':1800000010} for name in files]
    manifest={'tool_version':'1.2','scans':[{'files':rows}], 'trace_diagnostics':diagnostics}
    with zipfile.ZipFile(path,'w') as z:
        z.writestr('manifest.json',json.dumps(manifest))
        for name,data in files.items(): z.writestr('files/c/'+name,data)


class PhoneDiagnosticsTests(unittest.TestCase):
    def test_reported_setting_and_new_trace(self):
        with tempfile.TemporaryDirectory() as temp:
            old=Path(temp)/'old.zip'; new=Path(temp)/'new.zip'
            fixture(old,{'Documents/Log398/a.log':b'old'},[])
            fixture(new,{'Documents/Log398/a.log':b'new','Documents/TraceBinLogs/custom.capture':b'\x01\x00'},[{'disk_trace_level':3,'activation_time':1800000000,'runtime_effect_verified':False}])
            result=inspect(new,old)
            self.assertEqual(result['trace_file_count'],1)
            self.assertEqual(result['nonempty_trace_files_updated_after_activation'],1)
            self.assertEqual(result['change_counts'],{'modified':1,'added':1})
            self.assertFalse(result['reported_diagnostics'][0]['reported_runtime_effect_verified'])
            self.assertNotIn('fixture',json.dumps(result))

    def test_objective_c_numeric_booleans(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'a.zip';fixture(path,{},[{'preference_present':1,'restore_available':1,'backup_file_present':0,'runtime_effect_verified':2}])
            row=inspect(path)['reported_diagnostics'][0]
            self.assertTrue(row['reported_preference_present'])
            self.assertTrue(row['reported_restore_available'])
            self.assertFalse(row['reported_backup_file_present'])
            self.assertIsNone(row['reported_runtime_effect_verified'])

    def test_absent_diagnostic_is_unknown(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'a.zip';fixture(path,{'Documents/Log398/a.log':b'old'},[])
            result=inspect(path)
            self.assertIsNone(result['nonempty_trace_files_updated_after_activation'])
            self.assertEqual(result['reported_diagnostics'],[])

    def test_invalid_values_are_not_published(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'a.zip';fixture(path,{},[{'disk_trace_level':'private-string','restore_available':'secret','activation_time':'secret'}])
            result=inspect(path)
            self.assertNotIn('secret',json.dumps(result))
            self.assertNotIn('private-string',json.dumps(result))
            self.assertIsNone(result['reported_diagnostics'][0]['reported_disk_trace_level'])

    def test_unchanged_input_and_zero_trace(self):
        with tempfile.TemporaryDirectory() as temp:
            path=Path(temp)/'a.zip';fixture(path,{'Documents/Log398/a.log':b'same'},[{'disk_trace_level':3,'activation_time':1800000000}])
            result=inspect(path,path)
            self.assertEqual(result['change_counts'],{'unchanged':1})
            self.assertEqual(result['nonempty_trace_files_updated_after_activation'],0)

if __name__=='__main__': unittest.main()
