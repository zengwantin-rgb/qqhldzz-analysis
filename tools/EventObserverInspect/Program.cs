using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var assemblies=Directory.GetFiles(args[0],"*.dll").Select(AssemblyDefinition.ReadAssembly).ToArray();
var types=assemblies.SelectMany(a=>Flatten(a.MainModule.Types).Select(t=>(assembly:a.Name.Name,type:t))).ToArray();
var names=new HashSet<string>{"TSDK4CSharpService","TSDKMessageEvent","EventCallback","IEventDispatcher","EventDispatcher","IEvent","SimpleGameMsg","SoNotifyMsgTV"};
var schemas=types.Where(p=>names.Contains(p.type.Name)).Select(p=>new{assembly=p.assembly,type=p.type.FullName,
 fields=p.type.Fields.Select(f=>new{name=f.Name,type=f.FieldType.FullName,is_static=f.IsStatic,constant=f.HasConstant && (f.Constant is not string || f.Name is "OnTSDKGameMessage" or "OnTSDKGamePbMessage")?f.Constant:null}).ToArray(),
 properties=p.type.Properties.Select(x=>new{name=x.Name,type=x.PropertyType.FullName}).ToArray(),
 methods=p.type.Methods.Select(m=>new{name=m.Name,signature=m.FullName,is_public=m.IsPublic,is_static=m.IsStatic,is_virtual=m.IsVirtual,parameters=m.Parameters.Select(x=>new{name=x.Name,type=x.ParameterType.FullName}).ToArray()}).ToArray()}).ToArray();
var candidates=new List<object>();var gameIds=new List<object>();var eventSites=new List<object>();
object Brief(Instruction i)=>new{offset=i.Offset,opcode=i.OpCode.Name,operand=i.Operand switch{
 MethodReference m=>(object)new{method=m.FullName},FieldReference f=>new{field=f.FullName},TypeReference t=>new{type=t.FullName},
 Instruction target=>new{branch_offset=target.Offset},VariableDefinition v=>new{local=v.Index},
 int n when n>=-1 && n<=65535=>(object)n,sbyte n=>(object)n,string _=>new{string_literal_omitted=true},
 null=>null,_=>new{operand_kind=i.Operand.GetType().Name}}};
foreach(var pair in types)foreach(var f in pair.type.Fields.Where(f=>f.HasConstant && f.Name.StartsWith("ID_SO_")))gameIds.Add(new{type=pair.type.FullName,name=f.Name,value=f.Constant});
foreach(var pair in types)foreach(var method in pair.type.Methods.Where(m=>m.HasBody)){
 var ins=method.Body.Instructions;
 for(int index=1;index<ins.Count;index++){
  if(ins[index].OpCode.Code!=Code.Box || ins[index].Operand is not TypeReference enumType || enumType.Name!="TSDKMessageEvent")continue;
  var previous=ins[index-1];int? value=previous.OpCode.Code is Code.Ldc_I4 or Code.Ldc_I4_S?Convert.ToInt32(previous.Operand):null;
  if(value is not (9 or 217 or 300))continue;
  eventSites.Add(new{assembly=pair.assembly,method=method.FullName,event_value=value,locals=method.Body.Variables.Select(v=>new{index=v.Index,type=v.VariableType.FullName}).ToArray(),instructions=(pair.type.Name=="TSDK4CSharpService"?ins.AsEnumerable():ins.Skip(Math.Max(0,index-3)).Take(20)).Select(Brief).ToArray()});
 }
 var hits=ins.Where(i=>i.Operand is FieldReference f && f.DeclaringType.Name=="TSDKMessageEvent" && f.Name is "OnTSDKGameMessage" or "OnTSDKGamePbMessage").ToArray();
 if(hits.Length>0)candidates.Add(new{assembly=pair.assembly,method=method.FullName,events=hits.Select(i=>((FieldReference)i.Operand).Name).Distinct().ToArray(),calls=ins.Where(i=>i.Operand is MethodReference).Select(i=>((MethodReference)i.Operand).FullName).Distinct().ToArray()});
}
Directory.CreateDirectory(args[1]);
File.WriteAllText(Path.Combine(args[1],"event-observer-schema.json"),JsonSerializer.Serialize(new{assembly_count=assemblies.Length,schemas,candidates,game_message_ids=gameIds,event_sites=eventSites,limits=new[]{"Static schema and direct source references only; no runtime event registration is performed.","Runtime field values, account data and arbitrary strings are not emitted."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
foreach(var asm in assemblies)asm.Dispose();
Console.WriteLine($"Read {schemas.Length} source types and {candidates.Count} event-reference methods.");
