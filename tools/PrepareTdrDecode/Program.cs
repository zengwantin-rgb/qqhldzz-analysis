using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
Directory.CreateDirectory(args[1]);Directory.CreateDirectory(args[2]);int guards=0;
IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> ts){foreach(var t in ts){yield return t;foreach(var n in Flatten(t.NestedTypes))yield return n;}}
foreach(var file in Directory.GetFiles(args[0],"*.dll")){
 using var a=AssemblyDefinition.ReadAssembly(file);
 foreach(var t in Flatten(a.MainModule.Types)){
  if(t.FullName=="IFix.WrappersManagerImpl")foreach(var m in t.Methods.Where(m=>m.Name=="IsPatched" && m.IsStatic && m.ReturnType.FullName=="System.Boolean" && m.HasBody)){
    m.Body=new MethodBody(m);m.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));m.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));guards++;
  }
  if(t.FullName is "HuanLeGameSo.UMJMsgTVValue" or "HuanLeGameSo.SoNotifyMsgTV"){
   var schema=new {type=t.FullName,fields=t.Fields.Where(f=>!f.IsStatic).Select(f=>new{name=f.Name,type=f.FieldType.FullName}),methods=t.Methods.Where(m=>m.HasBody && m.Name=="pack").Select(m=>new{signature=m.FullName,instructions=m.Body.Instructions.Select(i=>new{offset=i.Offset,opcode=i.OpCode.Name,operand=i.Operand is FieldReference f?f.FullName:i.Operand is MethodReference r?r.FullName:i.Operand is Instruction b?"IL_"+b.Offset:i.Operand is Instruction[] bs?string.Join(",",bs.Select(b=>b.Offset.ToString())):i.Operand is string?"<literal omitted>":i.Operand?.ToString()})})};
   File.WriteAllText(Path.Combine(args[2],t.Name+"-schema.json"),JsonSerializer.Serialize(schema,new JsonSerializerOptions{WriteIndented=true}));
  }
 }
 a.Write(Path.Combine(args[1],Path.GetFileName(file)));
}
if(guards==0)throw new Exception("No expected IFix guard found");
File.WriteAllText(Path.Combine(args[2],"decode-preparation.json"),JsonSerializer.Serialize(new{private_copies_only=true,ifix_boolean_guards_disabled=guards,other_changes=false})+"\n");
