using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Text.RegularExpressions;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var methods=new List<object>();var enums=new List<object>();var traceTypes=new List<object>();var callers=new List<object>();int assemblies=0;
object Operand(object operand) => operand switch {
 null => null,
 MethodReference m => new{method=m.FullName},
 FieldReference f => new{field=f.FullName},
 TypeReference t => new{type=t.FullName},
 Instruction i => new{branch_offset=i.Offset},
 Instruction[] a => new{branch_offsets=a.Select(i=>i.Offset).ToArray()},
 VariableDefinition v => new{local_index=v.Index},
 ParameterDefinition p => new{parameter_index=p.Index},
 string s when s is "" or "/" or "\\" or "." => new{static_path_token=s},
 string s when Regex.IsMatch(s,@"^\.[A-Za-z0-9]{1,12}$") => new{static_extension=s},
 string s when Regex.IsMatch(s,@"^-?[0-9]+(,-?[0-9]+){5}$") && s.Length<=64 => new{static_diagnostic_numeric_default=s},
 string s => Regex.IsMatch(s,@"^(HBP|VM|Trace|trace|Device|device|Enable|enable|Log|log|Keep|Max|Delete|Record|Retain|Reserve|Save|Expire|Clean)[A-Za-z0-9_./-]{0,80}$") ? new{static_config_literal=s} : (object)new{string_literal_omitted=true},
 int or long or short or byte or sbyte or uint or ulong or float or double => operand,
 _ => new{operand_kind=operand.GetType().Name}
};
foreach(var file in Directory.GetFiles(args[0],"*.dll")){
 using var asm=AssemblyDefinition.ReadAssembly(file);assemblies++;
 foreach(var t in Flatten(asm.MainModule.Types)){
  if(t.IsEnum && (t.Name.Contains("TraceBin") || t.Name=="LogBinOpType" || t.FullName=="HappyMahjong.Common.DeviceType"))enums.Add(new{assembly=asm.Name.Name,type=t.FullName,values=t.Fields.Where(f=>f.HasConstant).Select(f=>new{name=f.Name,value=f.Constant}).ToArray()});
  if(t.Namespace=="Happy.Blueprint" && t.Name.Contains("TraceBin"))traceTypes.Add(new{assembly=asm.Name.Name,type=t.FullName,fields=t.Fields.Select(f=>new{name=f.Name,type=f.FieldType.FullName,constant=f.HasConstant && f.Constant is not string?f.Constant:null}).ToArray(),methods=t.Methods.Select(m=>m.FullName).ToArray()});
  foreach(var m in t.Methods.Where(m=>m.HasBody)){
   bool selected=(t.Name=="HBPUtil" && (m.Name.Contains("Trace") || m.Name.Contains("LogLevel")))
    || (t.FullName=="HappyMahjong.Common.GM.GMModel" && (m.Name.Contains("VMTraceBinLevel") || m.Name.Contains("VMLogLevel") || m.Name==".ctor"))
    || (t.FullName=="HappyMahjong.Common.GM.GMVMSystemHandler" && (m.Name=="OnSetTraceBinLevel" || m.Name=="GetTraceBinLevel"))
    || (t.Namespace=="Happy.Blueprint" && t.Name.Contains("TraceBin"))
    || (t.FullName=="HappyMahjong.SelectionScene.SelectionStartCommand" && (m.Name=="InitHbpEngineService" || m.Name=="CheckDeleteOldRecord"))
    || (t.FullName=="HappyMahjong.Common.DynamicConfig" && m.Name=="GetString")
    || (t.FullName=="HappyMahjong.Common.PathManager" && m.Name=="GetPersistentPath");
   if(selected)methods.Add(new{assembly=asm.Name.Name,type=t.FullName,method=m.FullName,return_type=m.ReturnType.FullName,instructions=m.Body.Instructions.Select(i=>new{offset=i.Offset,opcode=i.OpCode.Name,operand=Operand(i.Operand)}).ToArray()});
   var hits=m.Body.Instructions.Where(i=>i.Operand is MethodReference r && (r.Name.Contains("TraceBin") || r.Name=="GetVMLogLevelByDeviceType")).Select(i=>new{offset=i.Offset,method=((MethodReference)i.Operand).FullName}).ToArray();
   if(hits.Length>0 && !t.FullName.StartsWith("IFix."))callers.Add(new{assembly=asm.Name.Name,method=m.FullName,calls=hits});
  }
 }
}
Directory.CreateDirectory(args[1]);
File.WriteAllText(Path.Combine(args[1],"trace-config.json"),JsonSerializer.Serialize(new{assemblies,methods,enums,trace_types=traceTypes,callers,limits=new[]{"Recovered static IL only; no runtime configuration or effect is proven.","Arbitrary string constants are omitted; only narrow static diagnostic-key names are retained."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
Console.WriteLine($"Scanned {assemblies} assemblies; {methods.Count} diagnostic methods, {enums.Count} enums, {callers.Count} callers.");
