using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MJDiagnostic;
namespace tsf4g_tdr_csharp { public class TdrError { public enum ErrorType { TDR_NO_ERROR } } }
namespace strange.extensions.dispatcher.eventdispatcher.api
{
    public interface IEvent { object data { get; } }
    public delegate void EventCallback(IEvent evt);
    public interface IEventDispatcher { void AddListener(object key, EventCallback callback); void RemoveListener(object key, EventCallback callback); }
    public sealed class FixtureDispatcher : IEventDispatcher
    {
        public readonly Dictionary<object,EventCallback> Listeners = new Dictionary<object,EventCallback>();
        public void AddListener(object key,EventCallback callback) { Listeners.Add(key,callback); }
        public void RemoveListener(object key,EventCallback callback) { Listeners.Remove(key); }
        sealed class Event : IEvent { public object data { get; set; } }
        public void Send(object key,object value) { Listeners[key](new Event { data=value }); }
    }
}
namespace HappyMahjong
{
    public enum TSDKMessageEvent { OnTSDKGameMessage=9, OnTSDKGamePbMessage=217 }
    public static class TSDK4CSharpService { public static strange.extensions.dispatcher.eventdispatcher.api.IEventDispatcher dispatcher; }
}
namespace HuanLeGameSo
{
    public sealed class SoNotifyMsgTV
    {
        public int iType;
        public string text="quote\"\nfixture";
        public byte[] tiles=new byte[] {1,2,3};
        public tsf4g_tdr_csharp.TdrError.ErrorType pack(ref byte[] bytes,int size,ref int used,uint version)
        {
            if (iType==2) throw new NullReferenceException("private exception fixture must never be journaled");
            bytes[0]=7;used=1;return tsf4g_tdr_csharp.TdrError.ErrorType.TDR_NO_ERROR;
        }
    }
}
namespace MJSoPb { public sealed class SoNotifyMsgTV { public int type; } }
namespace Google.Protobuf { public static class MessageExtensions { public static byte[] ToByteArray(MJSoPb.SoNotifyMsgTV message) { return new byte[] {1}; } } }
static class CallbackFixture
{
    public static void Run(string root)
    {
        string events=Path.Combine(root,"game-events-callback.log"),status=Path.Combine(root,"observer-status.log");
        File.WriteAllText(events,ObserverJournal.EventsHeader,new UTF8Encoding(false));
        var dispatcher=new strange.extensions.dispatcher.eventdispatcher.api.FixtureDispatcher();HappyMahjong.TSDK4CSharpService.dispatcher=dispatcher;
        if (!Observer.Start(events,status)) throw new Exception("start fixture");
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV { iType=2 });
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV { iType=int.MaxValue });
        Observer.Stop();
        string[] lines=File.ReadAllLines(events);
        if (lines.Length!=3) throw new Exception("serializer failure must preserve fallback and subsequent events");
        using(var json=JsonDocument.Parse(lines[1]))
        {
            if (json.RootElement.GetProperty("encoding").GetString()!="public_field_snapshot") throw new Exception("snapshot label");
            using(var snapshot=JsonDocument.Parse(Convert.FromBase64String(json.RootElement.GetProperty("payload_b64").GetString())))
                if(snapshot.RootElement.GetProperty("text").GetString()!="quote\"\nfixture" || snapshot.RootElement.GetProperty("tiles").GetArrayLength()!=3) throw new Exception("snapshot escapes and fields");
        }
        using(var json=JsonDocument.Parse(lines[2])) if(json.RootElement.GetProperty("message_type").GetInt32()!=int.MaxValue) throw new Exception("full int32 message ID");
        string diagnostics=File.ReadAllText(status);
        if (!diagnostics.Contains("tdr_pack") || !diagnostics.Contains("null_reference") || diagnostics.Contains("private exception") || diagnostics.Contains("capture_failed")) throw new Exception("safe stage diagnostics and no global latch");
        bool bounded=false;try { FieldSnapshot.Encode(new byte[4097]); } catch(InvalidOperationException) { bounded=true; } if(!bounded) throw new Exception("snapshot array budget");
    }
}
