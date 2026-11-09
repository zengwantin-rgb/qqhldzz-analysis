using System;
using System.IO;
using System.Text;
using System.Text.Json;
using MJDiagnostic;
class Tests
{
    static void Check(bool value,string message) { if (!value) throw new Exception(message); }
    static void Main()
    {
        string root=Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString("N"),"MJMessageObserver");Directory.CreateDirectory(root);
        string events=Path.Combine(root,"game-events-fixture.log"),status=Path.Combine(root,"observer-status.log");
        File.WriteAllText(events,ObserverJournal.EventsHeader,new UTF8Encoding(false));File.WriteAllText(status,ObserverJournal.StatusHeader,new UTF8Encoding(false));
        var journal=new ObserverJournal(events,status,2048,2);
        byte[] original=Encoding.UTF8.GetBytes("private fixture bytes");
        Check(journal.Append("tdr",31,original,original.Length),"append encoded frame");
        Check(journal.Append("protobuf",32,original,3),"append bounded subrange");
        string[] lines=File.ReadAllLines(events);
        using(var json=JsonDocument.Parse(lines[1])) Check(Convert.FromBase64String(json.RootElement.GetProperty("payload_b64").GetString()).Length==original.Length,"full frame roundtrip");
        using(var json=JsonDocument.Parse(lines[2])) Check(json.RootElement.GetProperty("bytes").GetInt32()==3 && Convert.FromBase64String(json.RootElement.GetProperty("payload_b64").GetString()).Length==3,"no unused buffer tail");
        string before=File.ReadAllText(events);
        Check(!journal.Append("tdr",33,original,original.Length) && journal.LimitReached && File.ReadAllText(events)==before,"frame limit cannot truncate existing frames");
        File.WriteAllText(events,ObserverJournal.EventsHeader,new UTF8Encoding(false));
        journal=new ObserverJournal(events,status,ObserverJournal.EventsHeader.Length+5,10);
        Check(!journal.Append("tdr",31,original,original.Length),"byte budget enforced before write");
        Check(File.ReadAllText(events)==ObserverJournal.EventsHeader,"byte budget leaves file unchanged");
        bool refused=false;try { journal.Append("authentication",31,original,original.Length); } catch(InvalidOperationException) { refused=true; } Check(refused,"reject unsupported channel");
        File.WriteAllText(events,"unrelated contents");refused=false;try { new ObserverJournal(events,status,2048,2); } catch(InvalidOperationException) { refused=true; }Check(refused && File.ReadAllText(events)=="unrelated contents","unrelated file guard");
        CallbackFixture.Run(root);
        ModelFixture.Run();
        ObserverModelFixture.Run(root);
        Directory.Delete(Path.GetDirectoryName(root),true);
        Console.WriteLine("PASS: frame encoding/subrange, frame and byte budgets, channel restriction and unrelated-file guard");
    }
}
