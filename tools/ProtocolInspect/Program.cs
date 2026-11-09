using Mono.Cecil;
using System.Text.Json;
using System.IO.Compression;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> types){foreach(var t in types){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var targetTypes=new HashSet<string>{"MJSoPb.UnitProtoValue","MJSoPb.TProtoIntelligenceAdvice","MJSoPb.TProtoIntelligenceAdviceTing","MJSoPb.TProtoOneDiscardTingAdvice","MJSoPb.TProtoSingleTingTile","HuanLeGameSo.TAbilityTileWallInfo","HuanLeGameSo.TSingleTileShowInfo"};
var targetFields=new HashSet<string>{"stUnitProtoValueBuf","stAbilityTileWallInfo","iTileFace","iTileBack","astTileInfos"};
var schemas=new List<object>();var consumers=new List<object>();int scanned=0,bodies=0;
var candidateNames=new Dictionary<string,List<int>>(StringComparer.OrdinalIgnoreCase);
var fileReferences=new List<object>();
var recordIoCandidates=new List<object>();
var gameplayFieldNames=new HashSet<string>(StringComparer.Ordinal);
var gameplayNamePattern=new System.Text.RegularExpressions.Regex("hand|tile|card|seat|chair|deal|draw|wall|replay|record|playback",System.Text.RegularExpressions.RegexOptions.IgnoreCase);
if(File.Exists("private-input/device.zip")){
 using var archive=ZipFile.OpenRead("private-input/device.zip");
 using var manifestStream=archive.GetEntry("manifest.json")!.Open();
 using var manifest=JsonDocument.Parse(manifestStream);
 int candidateIndex=0;
 foreach(var scan in manifest.RootElement.GetProperty("scans").EnumerateArray())
 foreach(var candidate in scan.GetProperty("files").EnumerateArray()){
  candidateIndex++;
  string name=Path.GetFileName(candidate.GetProperty("relative").GetString()!.Replace('\\','/'));
  if(!candidateNames.TryGetValue(name,out var indices))candidateNames[name]=indices=new();
  indices.Add(candidateIndex);
 }
}
foreach(var file in Directory.GetFiles(args[0],"*.dll")){
 using var asm=AssemblyDefinition.ReadAssembly(file);
 foreach(var t in Flatten(asm.MainModule.Types)){
  foreach(string member in t.Fields.Select(f=>f.Name).Concat(t.Properties.Select(p=>p.Name))){
   string key=member.Trim('_');
   if(key.Length<=80 && System.Text.RegularExpressions.Regex.IsMatch(key,@"^[A-Za-z_][A-Za-z0-9_]*$") && !key.EndsWith("FieldNumber") && !key.EndsWith("DefaultValue") && gameplayNamePattern.IsMatch(key))gameplayFieldNames.Add(key);
  }
  if(targetTypes.Contains(t.FullName) || t.FullName is "Configuration.VedioRecordConfig" or "Configuration.VedioRecordConfigCfg")schemas.Add(new{assembly=asm.Name.Name,type=t.FullName,fields=t.Fields.Where(f=>!f.Name.StartsWith("_")).Select(f=>new{name=f.Name,type=f.FieldType.FullName,constant=f.HasConstant?f.Constant:null}).ToArray()});
  foreach(var m in t.Methods){
   scanned++;if(!m.HasBody)continue;bodies++;
   var hits=new List<object>();
   var ioCalls=new HashSet<string>();
   bool recordLiteral=false,byteArray16=false;
   var recordFileLiterals=new HashSet<string>();
   foreach(var ins in m.Body.Instructions){
    if(ins.Operand is MethodReference called && (called.DeclaringType.FullName.StartsWith("System.IO.") || called.DeclaringType.Name.Contains("FileHelper") || called.DeclaringType.Name.Contains("PathManager")))ioCalls.Add(called.FullName);
    if(ins.OpCode.Name=="newarr" && ins.Operand is TypeReference element && element.FullName=="System.Byte" && ins.Previous!=null){
     var prev=ins.Previous;
     if((prev.OpCode.Name=="ldc.i4.s" && Convert.ToInt32(prev.Operand)==16) || (prev.OpCode.Name=="ldc.i4" && Convert.ToInt32(prev.Operand)==16))byteArray16=true;
    }
    if(ins.OpCode.Name=="ldstr" && ins.Operand is string literal){
     if(literal.Contains("record",StringComparison.OrdinalIgnoreCase)){
      recordLiteral=true;
      string leaf=Path.GetFileName(literal.Replace('\\','/'));
      if(System.Text.RegularExpressions.Regex.IsMatch(leaf,@"^[A-Za-z0-9_.%+\-]{1,100}\.(dat|data|bin)$",System.Text.RegularExpressions.RegexOptions.IgnoreCase))recordFileLiterals.Add(leaf);
     }
     string name=Path.GetFileName(literal.Replace('\\','/'));
     if(candidateNames.TryGetValue(name,out var indices))
      fileReferences.Add(new{candidate_indices=indices.ToArray(),assembly=asm.Name.Name,method=m.FullName,offset=ins.Offset});
    }
    if(ins.Operand is FieldReference f && targetFields.Contains(f.Name))
      hits.Add(new{offset=ins.Offset,opcode=ins.OpCode.Name,field=f.FullName});
   }
   if(ioCalls.Count>0 && (recordLiteral || byteArray16))recordIoCandidates.Add(new{assembly=asm.Name.Name,method=m.FullName,record_literal_present=recordLiteral,sixteen_byte_array_allocation=byteArray16,static_record_filenames=recordFileLiterals.ToArray(),io_calls=ioCalls.ToArray()});
   if(hits.Count>0 && !t.Namespace.StartsWith("HuanLeGameSo") && !t.Namespace.StartsWith("MJSoPb") && !t.Namespace.StartsWith("IFix"))
    consumers.Add(new{assembly=asm.Name.Name,method=m.FullName,hits});
  }
 }
}
Directory.CreateDirectory(args[1]);
File.WriteAllText(Path.Combine(args[1],"gameplay-field-catalog.json"),JsonSerializer.Serialize(new{fields=gameplayFieldNames.OrderBy(n=>n,StringComparer.Ordinal).ToArray(),limits=new[]{"Static recovered field and property names selected by gameplay terms; names alone do not establish gameplay semantics."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
var result=new{scanned_methods=scanned,methods_with_bodies=bodies,schemas,consumers,limits=new[]{"Only the recovered managed assemblies were scanned; native AOT implementation and active runtime patches are not covered.","A schema definition does not establish actual network payload contents."}};
File.WriteAllText(Path.Combine(args[1],"protocol-consumers.json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+"\n");
File.WriteAllText(Path.Combine(args[1],"candidate-file-references.json"),JsonSerializer.Serialize(new{references=fileReferences,limits=new[]{"Only exact basename matches to managed method string literals are reported; filenames and private paths are omitted.","No match does not rule out native SDK files, dynamic path construction or runtime patches.","Candidate indices refer to manifest scan order, not exported ZIP entry order."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
File.WriteAllText(Path.Combine(args[1],"record-io-candidates.json"),JsonSerializer.Serialize(new{candidates=recordIoCandidates,limits=new[]{"Candidates contain file/path IO calls and either a record literal or an immediate 16-byte allocation.","A candidate is not proven to read or write the device file; indirect calls and dynamic allocation sizes are not covered.","Only method signatures and restricted static record filenames are emitted."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"Scanned {scanned} methods, {bodies} bodies; {schemas.Count} schemas; {consumers.Count} consumer methods.");
