using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
Directory.CreateDirectory(args[1]);Directory.CreateDirectory(args[2]);int guards=0;int defaults=0;var sizes=new List<object>();var unions=new List<object>();
var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(args[0]);
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
foreach(var file in Directory.GetFiles(args[0],"*.dll")){
 using var a=AssemblyDefinition.ReadAssembly(file,new ReaderParameters{AssemblyResolver=resolver});bool changed=false;
 foreach(var t in Flatten(a.MainModule.Types)){
  if(t.FullName is "HuanLeGameSo.UMJMsgTVValue" or "HuanLeGameSo.USOTLVValue"){
   var map=new Dictionary<long,string>();
   foreach(var ctor in t.Methods.Where(m=>(m.Name==".cctor" || m.Name=="addPackDict") && m.HasBody)){
    var ins=ctor.Body.Instructions;
    for(int j=0;j<ins.Count;j++)if(ins[j].OpCode.Code==Code.Ldftn && ins[j].Operand is MethodReference callback){
      var target=t.Methods.FirstOrDefault(m=>m.FullName==callback.FullName);
      if(target==null || !target.HasBody || !target.Parameters.Any(p=>p.ParameterType.FullName.Contains("TdrWriteBuf")))continue;
      var fields=target.Body.Instructions.Where(i=>i.OpCode.Code==Code.Ldfld && i.Operand is FieldReference f && f.DeclaringType.FullName==t.FullName).Select(i=>((FieldReference)i.Operand).Name).Distinct().ToArray();
      long? key=null;
      for(int k=j-1;k>=Math.Max(0,j-8);k--){var q=ins[k];if(q.OpCode.Code==Code.Ldc_I8 || q.OpCode.Code==Code.Ldc_I4 || q.OpCode.Code==Code.Ldc_I4_S){key=Convert.ToInt64(q.Operand);break;} if(q.OpCode.Code>=Code.Ldc_I4_0 && q.OpCode.Code<=Code.Ldc_I4_8){key=(int)q.OpCode.Code-(int)Code.Ldc_I4_0;break;}}
      if(fields.Length==1 && key.HasValue)map[key.Value]=fields[0];
    }
   }
   unions.Add(new{type=t.FullName,selector=t.Name=="UMJMsgTVValue"?"iType":"nType",members=map});
  }
  if(t.FullName.StartsWith("HuanLeGameSo."))sizes.Add(new{type=t.FullName,public_field_count=t.Fields.Count(f=>f.IsPublic && !f.IsStatic)});
  if(t.FullName=="IFix.WrappersManagerImpl")foreach(var m in t.Methods.Where(m=>m.Name=="IsPatched" && m.IsStatic && m.ReturnType.FullName=="System.Boolean" && m.HasBody)){
    m.Body=new MethodBody(m);m.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));m.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));guards++;changed=true;
  }
  if(t.FullName is "HuanLeGameSo.UMJMsgTVValue" or "HuanLeGameSo.SoNotifyMsgTV"){
   var schema=new {type=t.FullName,fields=t.Fields.Where(f=>!f.IsStatic).Select(f=>new{name=f.Name,type=f.FieldType.FullName}),methods=t.Methods.Where(m=>m.HasBody && (m.Name=="pack" || m.Name=="addPackDict")).Select(m=>new{signature=m.FullName,instructions=m.Body.Instructions.Select(i=>new{offset=i.Offset,opcode=i.OpCode.Name,operand=i.Operand is FieldReference f?f.FullName:i.Operand is MethodReference r?r.FullName:i.Operand is Instruction b?"IL_"+b.Offset:i.Operand is Instruction[] bs?string.Join(",",bs.Select(b=>b.Offset.ToString())):i.Operand is string?"<literal omitted>":i.Operand?.ToString()})})};
   File.WriteAllText(Path.Combine(args[2],t.Name+"-schema.json"),JsonSerializer.Serialize(schema,new JsonSerializerOptions{WriteIndented=true}));
  }
 }
 if(changed){
  foreach(var type in Flatten(a.MainModule.Types))foreach(var method in type.Methods)foreach(var parameter in method.Parameters)if(parameter.HasConstant){parameter.Constant=null;parameter.HasConstant=false;defaults++;}
  a.Write(Path.Combine(args[1],Path.GetFileName(file)));
 }else File.Copy(file,Path.Combine(args[1],Path.GetFileName(file)),true);
}
if(guards==0)throw new Exception("No expected IFix guard found");
File.WriteAllText(Path.Combine(args[2],"decode-preparation.json"),JsonSerializer.Serialize(new{private_copies_only=true,ifix_boolean_guards_disabled=guards,optional_parameter_defaults_removed_in_modified_copies=defaults,method_bodies_otherwise_unchanged=true})+"\n");

File.WriteAllText(Path.Combine(args[2],"protocol-field-sizes.json"),JsonSerializer.Serialize(sizes,new JsonSerializerOptions{WriteIndented=true}));

File.WriteAllText(Path.Combine(args[2],"union-selectors.json"),JsonSerializer.Serialize(unions,new JsonSerializerOptions{WriteIndented=true}));
