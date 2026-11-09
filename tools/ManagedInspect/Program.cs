using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Text.RegularExpressions;
var root=args[0];var output=args[1];Directory.CreateDirectory(output);
var pattern=new Regex(@"hand.?tiles?|hand.?cards?|hold.?tiles?|hold.?cards?|shou.?pai|deal.?tiles?|deal.?cards?|opp(onent)?(?:tile|card|hand)|self.?tiles?|self.?cards?|others?.?(tiles|cards)|all.?hand|all.?tiles|all.?cards|replay|spectat|observe|game.?start|start.?game|reconnect|game.?end|end.?game|手牌|暗牌",RegexOptions.IgnoreCase);
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> types){foreach(var t in types){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
var assemblies=new List<object>();var candidates=new List<object>();var inventory=new List<object>();
foreach(var file in Directory.GetFiles(root,"*.dll").Where(p=>new[]{"HappyMahjong.InGameScripts.dll","HappyMahjong.Hall.dll","Assembly-CSharp.dll","HappyMahjong.Bootstrap.dll"}.Contains(Path.GetFileName(p)))){
 using var assembly=AssemblyDefinition.ReadAssembly(file);
 int count=0,methodCount=0,bodyCount=0;
 foreach(var t in Flatten(assembly.MainModule.Types)){
  count++;methodCount+=t.Methods.Count;bodyCount+=t.Methods.Count(m=>m.HasBody&&m.Body.Instructions.Count>0);
  var fs=t.Fields.Select(f=>new{name=f.Name,type=f.FieldType.FullName,constant=f.HasConstant?f.Constant:null}).ToList();
  var ms=t.Methods.Select(m=>new{name=m.Name,signature=m.FullName,instructions=m.HasBody?m.Body.Instructions.Count:0,token=m.MetadataToken.ToString()}).ToList();
  inventory.Add(new{assembly=assembly.Name.Name,name=t.FullName,fields=fs,methods=ms});
  var hits=new List<string>();
  if(pattern.IsMatch(t.FullName))hits.Add("type:"+t.FullName);
  hits.AddRange(t.Fields.Where(f=>pattern.IsMatch(f.Name)).Select(f=>"field:"+f.Name));
  hits.AddRange(t.Methods.Where(m=>pattern.IsMatch(m.Name)).Select(m=>"method:"+m.Name));
  if(hits.Count>0)candidates.Add(new{assembly=assembly.Name.Name,name=t.FullName,hits,fields=fs,methods=ms});
 }
 assemblies.Add(new{name=assembly.Name.Name,types=count,methods=methodCount,methods_with_bodies=bodyCount});
}
var options=new JsonSerializerOptions{WriteIndented=true};
File.WriteAllText(Path.Combine(output,"assemblies.json"),JsonSerializer.Serialize(assemblies,options));
File.WriteAllText(Path.Combine(output,"candidate-types.json"),JsonSerializer.Serialize(candidates,options));
File.WriteAllText(Path.Combine(output,"type-inventory.json"),JsonSerializer.Serialize(inventory,options));
Console.WriteLine(JsonSerializer.Serialize(assemblies,options));
Console.WriteLine($"Candidate types: {candidates.Count}");
