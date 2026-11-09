using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
class Decode
{
 static string references;
 static Assembly Resolve(object sender,ResolveEventArgs e) { string path=Path.Combine(references,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null; }
 static string Quote(string s) { var b=new StringBuilder("\"");foreach(char c in s){if(c=='"'||c=='\\')b.Append('\\').Append(c);else if(c<32)b.Append("\\u").Append(((int)c).ToString("x4"));else b.Append(c);}return b.Append('"').ToString(); }
 static int nodes;
 static string Json(object o,int depth)
 {
  if(++nodes>200000 || depth>30)throw new InvalidOperationException();
  if(o==null)return "null";Type t=o.GetType();
  if(o is string || o is char)return Quote(Convert.ToString(o,CultureInfo.InvariantCulture));
  if(o is bool)return (bool)o?"true":"false";
  if(t.IsEnum)return Convert.ToInt64(o).ToString(CultureInfo.InvariantCulture);
  if(t.IsPrimitive || o is decimal)return Convert.ToString(o,CultureInfo.InvariantCulture);
  var a=o as Array;
  var parts=new List<string>();
  if(a!=null){if(a.Length>65536)throw new InvalidOperationException();foreach(var v in a)parts.Add(Json(v,depth+1));return "["+string.Join(",",parts.ToArray())+"]";}
  if(t.FullName==null || !t.FullName.StartsWith("HuanLeGameSo."))throw new NotSupportedException();
  foreach(var f in t.GetFields(BindingFlags.Public|BindingFlags.Instance))parts.Add(Quote(f.Name)+":"+Json(f.GetValue(o),depth+1));
  return "{"+string.Join(",",parts.ToArray())+"}";
 }
 static void Main(string[] args)
 {
  try { Run(args); }
  catch(Exception error) { Exception inner=error is TargetInvocationException && error.InnerException!=null?error.InnerException:error;File.WriteAllText(Path.Combine(args[2],"decode-summary.json"),"{\"startup_error_category\":"+Quote(inner.GetType().Name)+"}"); }
 }
 static void Run(string[] args)
 {
  references=args[0];AppDomain.CurrentDomain.AssemblyResolve+=Resolve;
  var a=Assembly.LoadFrom(Path.Combine(references,"HappyMahjong.InGameScripts.dll"));
  var t=a.GetType("HuanLeGameSo.SoNotifyMsgTV",true);
  var m=t.GetMethod("unpack",new Type[]{typeof(byte[]).MakeByRefType(),typeof(int),typeof(int).MakeByRefType(),typeof(uint)});
  var summary=new StringBuilder("{\"frames\":[");bool first=true;
  foreach(string file in Directory.GetFiles(args[1],"*.tdr")){
   string status="failed",category="none";int used=0,type=0;bool consumedAll=false,roundtrip=false;string bytesOut=null;
   try {
    byte[] bytes=File.ReadAllBytes(file);object msg=Activator.CreateInstance(t);object[] p={bytes,bytes.Length,0,(uint)0};object result=m.Invoke(msg,p);used=(int)p[2];consumedAll=used==bytes.Length;type=(int)t.GetField("iType").GetValue(msg);
    if(Convert.ToInt32(result)!=0){status="unpack_error";category=Convert.ToInt32(result).ToString(CultureInfo.InvariantCulture);}
    else {status="unpacked_snapshot_failed";nodes=0;bytesOut=Encoding.UTF8.GetString(MJDiagnostic.FieldSnapshot.Encode(msg));status="decoded";
      try {
       var pack=t.GetMethod("pack",new Type[]{typeof(byte[]).MakeByRefType(),typeof(int),typeof(int).MakeByRefType(),typeof(uint)});
       object[] encoded={new byte[256*1024],256*1024,0,(uint)0};object packed=pack.Invoke(msg,encoded);byte[] again=(byte[])encoded[0];
       roundtrip=Convert.ToInt32(packed)==0 && (int)encoded[2]==bytes.Length;
       if(roundtrip)for(int q=0;q<bytes.Length;q++)if(bytes[q]!=again[q]){roundtrip=false;break;}
      } catch { roundtrip=false; }
}
   } catch(Exception e){Exception inner=e is TargetInvocationException && e.InnerException!=null?e.InnerException:e;category=inner.GetType().Name;}
   // Only fixed counters and exception type metadata are written here. Decoded fields stay private.
   if(bytesOut!=null)File.WriteAllText(Path.Combine(args[2],Path.GetFileName(file)+".json"),bytesOut);
   if(!first)summary.Append(',');first=false;
   summary.Append("{\"file\":").Append(Quote(Path.GetFileName(file))).Append(",\"status\":").Append(Quote(status)).Append(",\"category\":").Append(Quote(category)).Append(",\"message_type\":").Append(type).Append(",\"used_bytes\":").Append(used).Append(",\"unpack_consumed_all\":").Append(consumedAll?"true":"false").Append(",\"roundtrip_equal\":").Append(roundtrip?"true":"false").Append('}');
  }
  summary.Append("]}");File.WriteAllText(Path.Combine(args[2],"decode-summary.json"),summary.ToString());
 }
}
