using Mono.Cecil;
using System.Text.Json;
using System.IO.Compression;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> types){foreach(var t in types){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var targetTypes=new HashSet<string>{"MJSoPb.UnitProtoValue","MJSoPb.TProtoIntelligenceAdvice","MJSoPb.TProtoIntelligenceAdviceTing","MJSoPb.TProtoOneDiscardTingAdvice","MJSoPb.TProtoSingleTingTile","HuanLeGameSo.TAbilityTileWallInfo","HuanLeGameSo.TSingleTileShowInfo"};
var targetFields=new HashSet<string>{"stUnitProtoValueBuf","stAbilityTileWallInfo","iTileFace","iTileBack","astTileInfos"};
var schemas=new List<object>();var consumers=new List<object>();int scanned=0,bodies=0;
var candidateNames=new Dictionary<string,List<int>>(StringComparer.OrdinalIgnoreCase);
var fileReferences=new List<object>();
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
foreach(var file in Directory.GetFiles(args[0],"*.dll").Where(p=>new[]{"HappyMahjong.InGameScripts.dll","HappyMahjong.Hall.dll","Assembly-CSharp.dll","HappyMahjong.Bootstrap.dll"}.Contains(Path.GetFileName(p)))){
 using var asm=AssemblyDefinition.ReadAssembly(file);
 foreach(var t in Flatten(asm.MainModule.Types)){
  if(targetTypes.Contains(t.FullName))schemas.Add(new{assembly=asm.Name.Name,type=t.FullName,fields=t.Fields.Where(f=>!f.Name.StartsWith("_")).Select(f=>new{name=f.Name,type=f.FieldType.FullName,constant=f.HasConstant?f.Constant:null}).ToArray()});
  foreach(var m in t.Methods){
   scanned++;if(!m.HasBody)continue;bodies++;
   var hits=new List<object>();
   foreach(var ins in m.Body.Instructions){
    if(ins.OpCode.Name=="ldstr" && ins.Operand is string literal){
     string name=Path.GetFileName(literal.Replace('\\','/'));
     if(candidateNames.TryGetValue(name,out var indices))
      fileReferences.Add(new{candidate_indices=indices.ToArray(),assembly=asm.Name.Name,method=m.FullName,offset=ins.Offset});
    }
    if(ins.Operand is FieldReference f && targetFields.Contains(f.Name))
      hits.Add(new{offset=ins.Offset,opcode=ins.OpCode.Name,field=f.FullName});
   }
   if(hits.Count>0 && !t.Namespace.StartsWith("HuanLeGameSo") && !t.Namespace.StartsWith("MJSoPb") && !t.Namespace.StartsWith("IFix"))
    consumers.Add(new{assembly=asm.Name.Name,method=m.FullName,hits});
  }
 }
}
Directory.CreateDirectory(args[1]);
var result=new{scanned_methods=scanned,methods_with_bodies=bodies,schemas,consumers,limits=new[]{"Only the recovered managed assemblies were scanned; native AOT implementation and active runtime patches are not covered.","A schema definition does not establish actual network payload contents."}};
File.WriteAllText(Path.Combine(args[1],"protocol-consumers.json"),JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+"\n");
File.WriteAllText(Path.Combine(args[1],"candidate-file-references.json"),JsonSerializer.Serialize(new{references=fileReferences,limits=new[]{"Only exact basename matches to managed method string literals are reported; filenames and private paths are omitted.","No match does not rule out native SDK files, dynamic path construction or runtime patches.","Candidate indices refer to manifest scan order, not exported ZIP entry order."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"Scanned {scanned} methods, {bodies} bodies; {schemas.Count} schemas; {consumers.Count} consumer methods.");
