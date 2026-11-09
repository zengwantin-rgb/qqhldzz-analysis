using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var names=new HashSet<string>{"RoundModeUtil","GuoBiaoRoundModel","MJTileData","PlayerGameInfoVO","Blackboard","GameModuleRegistry","MJGameRulesCenter","GameTableView","InHandTileHandler","MJWallHandler","MJHandler","AbilityTileDataModel","GameStatus"};
var rows=new List<object>();
foreach(var file in Directory.GetFiles(args[0],"*.dll")){
 using var a=AssemblyDefinition.ReadAssembly(file);
 foreach(var t in Flatten(a.MainModule.Types).Where(t=>names.Contains(t.Name)))rows.Add(new{assembly=a.Name.Name,type=t.FullName,base_type=t.BaseType?.FullName,
 fields=t.Fields.Select(f=>new{name=f.Name,type=f.FieldType.FullName,is_static=f.IsStatic,is_public=f.IsPublic,constant=f.HasConstant && f.Constant is not string?f.Constant:null}),
 accessors=t.Methods.Where(m=>m.HasBody && (m.Name is "GetTilesByViewSeat" or "GetSelfGameInfo" or "GetModel" or "GetDataModel" || m.Name.StartsWith("get_") && m.ReturnType.FullName.Contains("Hand"))).Select(m=>new{method=m.FullName,fields=m.Body.Instructions.Where(i=>i.Operand is FieldReference).Select(i=>((FieldReference)i.Operand).FullName).Distinct()})});
}
Directory.CreateDirectory(args[1]);File.WriteAllText(Path.Combine(args[1],"runtime-model-schema.json"),JsonSerializer.Serialize(new{types=rows,limits=new[]{"Static metadata only; no runtime game fields or account values read.","Runtime class field presence is not yet verified on phone."}},new JsonSerializerOptions{WriteIndented=true}));
