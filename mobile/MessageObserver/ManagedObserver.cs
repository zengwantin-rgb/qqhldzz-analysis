using System;
using Google.Protobuf;
using HappyMahjong;
using strange.extensions.dispatcher.eventdispatcher.api;
using tsf4g_tdr_csharp;

namespace MJDiagnostic
{
    public static class Observer
    {
        static readonly object Gate = new object();
        static EventCallback tdrCallback;
        static EventCallback protobufCallback;
        static IEventDispatcher dispatcher;
        static ObserverJournal journal;
        static byte[] tdrBuffer;
        static bool started;
        static bool captureFailed;
        static int serializationFailures;

        public static bool Start(string events, string status)
        {
            lock (Gate)
            {
                if (started) return true;
                IEventDispatcher active = TSDK4CSharpService.dispatcher;
                if (active == null) return false;
                ObserverJournal next = new ObserverJournal(events, status, 18L * 1024 * 1024, 512);
                EventCallback onTdr = new EventCallback(OnTdr);
                EventCallback onProtobuf = new EventCallback(OnProtobuf);
                bool first = false;
                try
                {
                    journal = next;
                    tdrBuffer = new byte[ObserverJournal.MaxPayload];
                    active.AddListener(TSDKMessageEvent.OnTSDKGameMessage, onTdr); first = true;
                    active.AddListener(TSDKMessageEvent.OnTSDKGamePbMessage, onProtobuf);
                    dispatcher = active; tdrCallback = onTdr; protobufCallback = onProtobuf;
                    started = true; captureFailed = false; serializationFailures = 0;
                    journal.Status("observer_started", 0);
                    return true;
                }
                catch
                {
                    if (first) { try { active.RemoveListener(TSDKMessageEvent.OnTSDKGameMessage, onTdr); } catch { } }
                    try { active.RemoveListener(TSDKMessageEvent.OnTSDKGamePbMessage, onProtobuf); } catch { }
                    started = false; journal = null; tdrBuffer = null;
                    return false;
                }
            }
        }
        static void Failure(bool serialization)
        {
            if (serialization) serializationFailures++;
            else captureFailed = true;
            try { if (journal != null && serializationFailures <= 8) journal.Status(serialization ? "serialization_failed" : "capture_failed", serialization ? serializationFailures : 1); } catch { }
        }
        static void OnTdr(IEvent evt)
        {
            lock (Gate)
            {
                if (!started || captureFailed || journal == null || journal.LimitReached) return;
                try
                {
                    HuanLeGameSo.SoNotifyMsgTV message = evt.data as HuanLeGameSo.SoNotifyMsgTV;
                    if (message == null) return;
                    byte[] bytes = tdrBuffer; int used = 0;
                    TdrError.ErrorType result = message.pack(ref bytes, bytes.Length, ref used, 0);
                    if (result != TdrError.ErrorType.TDR_NO_ERROR) { Failure(true); return; }
                    journal.Append("tdr", message.iType, bytes, used);
                }
                catch { Failure(false); }
            }
        }
        static void OnProtobuf(IEvent evt)
        {
            lock (Gate)
            {
                if (!started || captureFailed || journal == null || journal.LimitReached) return;
                try
                {
                    MJSoPb.SoNotifyMsgTV message = evt.data as MJSoPb.SoNotifyMsgTV;
                    if (message == null) return;
                    byte[] bytes = MessageExtensions.ToByteArray(message);
                    journal.Append("protobuf", message.type, bytes, bytes.Length);
                }
                catch { Failure(false); }
            }
        }
        public static void Stop()
        {
            lock (Gate)
            {
                if (!started) return;
                started = false;
                try { dispatcher.RemoveListener(TSDKMessageEvent.OnTSDKGameMessage, tdrCallback); } catch { }
                try { dispatcher.RemoveListener(TSDKMessageEvent.OnTSDKGamePbMessage, protobufCallback); } catch { }
                try { journal.Status("observer_stopped", journal.FrameCount); } catch { }
            }
        }
    }
}
