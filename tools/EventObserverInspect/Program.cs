using Mono.Cecil;
using System.Text.Json;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var assemblies=Directory.GetFiles(args[0],"*.dll").Select(AssemblyDefinition.ReadAssembly).ToArray();
var types=assemblies.SelectMany(a=>Flatten(a.MainModule.Types).Select(t=>(assembly:a.Name.Name,type:t))).ToArray();
var names=new HashSet<string>{"TSDK4CSharpService","TSDKMessageEvent","EventCallback","IEventDispatcher","EventDispatcher","IEvent"};
var schemas=types.Where(p=>names.Contains(p.type.Name)).Select(p=>new{assembly=p.assembly,type=p.type.FullName,
 fields=p.type.Fields.Select(f=>new{name=f.Name,type=f.FieldType.FullName,is_static=f.IsStatic,constant=f.HasConstant && (f.Constant is not string || f.Name is "OnTSDKGameMessage" or "OnTSDKGamePbMessage")?f.Constant:null}).ToArray(),
 properties=p.type.Properties.Select(x=>new{name=x.Name,type=x.PropertyType.FullName}).ToArray(),
 methods=p.type.Methods.Select(m=>new{name=m.Name,signature=m.FullName,is_public=m.IsPublic,is_static=m.IsStatic,is_virtual=m.IsVirtual,parameters=m.Parameters.Select(x=>new{name=x.Name,type=x.ParameterType.FullName}).ToArray()}).ToArray()}).ToArray();
var candidates=new List<object>();
foreach(var pair in types)foreach(var method in pair.type.Methods.Where(m=>m.HasBody)){
 var ins=method.Body.Instructions;
 var hits=ins.Where(i=>i.Operand is FieldReference f && f.DeclaringType.Name=="TSDKMessageEvent" && f.Name is "OnTSDKGameMessage" or "OnTSDKGamePbMessage").ToArray();
 if(hits.Length>0)candidates.Add(new{assembly=pair.assembly,method=method.FullName,events=hits.Select(i=>((FieldReference)i.Operand).Name).Distinct().ToArray(),calls=ins.Where(i=>i.Operand is MethodReference).Select(i=>((MethodReference)i.Operand).FullName).Distinct().ToArray()});
}
Directory.CreateDirectory(args[1]);
File.WriteAllText(Path.Combine(args[1],"event-observer-schema.json"),JsonSerializer.Serialize(new{assembly_count=assemblies.Length,schemas,candidates,limits=new[]{"Static schema and direct source references only; no runtime event registration is performed.","Runtime field values, account data and arbitrary strings are not emitted."}},new JsonSerializerOptions{WriteIndented=true})+"\n");
foreach(var asm in assemblies)asm.Dispose();
Console.WriteLine($"Read {schemas.Length} source types and {candidates.Count} event-reference methods.");
