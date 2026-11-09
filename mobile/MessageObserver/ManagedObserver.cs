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
        static bool unexpectedTdr;
        static bool unexpectedProtobuf;

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
        static void Diagnostic(string stage, Exception error, int type)
        {
            serializationFailures++;
            if (serializationFailures <= 32) { try { journal.Error(stage, error, type); } catch { } }
        }
        static void Save(string channel, int type, byte[] bytes, int used, string encoding)
        {
            try { journal.Append(channel, type, bytes, used, encoding); }
            catch (Exception error) { Diagnostic("journal_append", error, type); Failure(false); }
        }
        static void OnTdr(IEvent evt)
        {
            lock (Gate)
            {
                if (!started || captureFailed || journal == null || journal.LimitReached) return;
                HuanLeGameSo.SoNotifyMsgTV message;
                try { message = evt.data as HuanLeGameSo.SoNotifyMsgTV; }
                catch (Exception error) { Diagnostic("tdr_pack", error, 0); return; }
                if (message == null) { if (!unexpectedTdr) { unexpectedTdr = true; try { journal.Status("unexpected_payload", 9); } catch { } } return; }
                byte[] bytes = tdrBuffer; int used = 0; bool packed = false;
                try
                {
                    TdrError.ErrorType result = message.pack(ref bytes, bytes.Length, ref used, 0);
                    packed = result == TdrError.ErrorType.TDR_NO_ERROR && bytes != null && used > 0 && used <= bytes.Length && used <= ObserverJournal.MaxPayload;
                    if (!packed) Diagnostic("tdr_pack", new InvalidOperationException(), message.iType);
                }
                catch (Exception error) { Diagnostic("tdr_pack", error, message.iType); }
                if (packed) { Save("tdr", message.iType, bytes, used, "reencoded_decoded_event"); return; }
                try { bytes = FieldSnapshot.Encode(message); }
                catch (Exception error) { Diagnostic("field_snapshot", error, message.iType); return; }
                Save("tdr", message.iType, bytes, bytes.Length, "public_field_snapshot");
            }
        }
        static void OnProtobuf(IEvent evt)
        {
            lock (Gate)
            {
                if (!started || captureFailed || journal == null || journal.LimitReached) return;
                MJSoPb.SoNotifyMsgTV message;
                try { message = evt.data as MJSoPb.SoNotifyMsgTV; }
                catch (Exception error) { Diagnostic("protobuf_encode", error, 0); return; }
                if (message == null) { if (!unexpectedProtobuf) { unexpectedProtobuf = true; try { journal.Status("unexpected_payload", 217); } catch { } } return; }
                byte[] bytes;
                try { bytes = MessageExtensions.ToByteArray(message); }
                catch (Exception error) { Diagnostic("protobuf_encode", error, message.type); return; }
                if (bytes == null || bytes.Length == 0 || bytes.Length > ObserverJournal.MaxPayload) { Diagnostic("protobuf_encode", new InvalidOperationException(), message.type); return; }
                Save("protobuf", message.type, bytes, bytes.Length, "reencoded_decoded_event");
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
