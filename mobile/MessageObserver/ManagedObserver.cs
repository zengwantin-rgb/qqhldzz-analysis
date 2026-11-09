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
        static string modelPhase = "unanchored";
        static object modelAnchor;
        static int modelGeneration;
        static int modelRecords;
        static bool modelAvailable;
        static string lastModelJson;
        static DateTime lastGameMessage;


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
                    modelPhase = "unanchored"; modelAnchor = null; modelGeneration = 0; modelRecords = 0; modelAvailable = false; lastModelJson = null;
                    unexpectedTdr = false; unexpectedProtobuf = false; lastGameMessage = DateTime.MinValue;
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
                if (!started || journal == null) return;
                HuanLeGameSo.SoNotifyMsgTV message;
                try { message = evt.data as HuanLeGameSo.SoNotifyMsgTV; }
                catch (Exception error) { Diagnostic("tdr_pack", error, 0); return; }
                if (message == null) { if (!unexpectedTdr) { unexpectedTdr = true; try { journal.Status("unexpected_payload", 9); } catch { } } return; }
                AnchorModel(message.iType);
                if (captureFailed || journal.LimitReached) return;
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
                if (!started || journal == null) return;
                MJSoPb.SoNotifyMsgTV message;
                try { message = evt.data as MJSoPb.SoNotifyMsgTV; }
                catch (Exception error) { Diagnostic("protobuf_encode", error, 0); return; }
                if (message == null) { if (!unexpectedProtobuf) { unexpectedProtobuf = true; try { journal.Status("unexpected_payload", 217); } catch { } } return; }
                AnchorModel(message.type);
                if (captureFailed || journal.LimitReached) return;
                byte[] bytes;
                try { bytes = MessageExtensions.ToByteArray(message); }
                catch (Exception error) { Diagnostic("protobuf_encode", error, message.type); return; }
                if (bytes == null || bytes.Length == 0 || bytes.Length > ObserverJournal.MaxPayload) { Diagnostic("protobuf_encode", new InvalidOperationException(), message.type); return; }
                Save("protobuf", message.type, bytes, bytes.Length, "reencoded_decoded_event");
            }
        }
        static void AnchorModel(int messageType)
        {
            lastGameMessage = DateTime.UtcNow;
            if (messageType != 1 && messageType != 50 && messageType != 51 && messageType != 54) return;
            try { modelAnchor = HappyMahjong.Game.RoundModeUtil.RoundModel; }
            catch { modelAnchor = null; }
            modelPhase = messageType == 1 ? "live" : "after_hu";
            if (messageType == 1 && modelGeneration < 10000) modelGeneration++;
        }
        // Native code calls only on the main queue. Reads known game fields; never game getters or setters.
        public static string ReadModel()
        {
            lock (Gate)
            {
                if (!started || journal == null) return GameModelReader.Json(GameModelReader.Capture(null, "unanchored"));
                object model = null;
                try { model = HappyMahjong.Game.RoundModeUtil.RoundModel; } catch { }
                string phase = model != null && Object.ReferenceEquals(model, modelAnchor) ? modelPhase : "unanchored";
                TimeSpan age = DateTime.UtcNow - lastGameMessage;
                if (phase != "unanchored" && (age < TimeSpan.Zero || age > TimeSpan.FromSeconds(45))) phase = "stale";
                ModelState state = GameModelReader.Capture(model, phase); state.Generation = modelGeneration;
                string json = GameModelReader.Json(state);
                if (state.Status == "ok" && !modelAvailable)
                {
                    modelAvailable = true;
                    try { journal.Status("model_reader_available", 1); } catch { }
                }
                if (!captureFailed && !journal.LimitReached && json != lastModelJson && modelRecords < 128)
                {
                    try
                    {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                        if (journal.Append("runtime", 0, bytes, bytes.Length, "runtime_model_snapshot")) { modelRecords++; lastModelJson = json; }
                        if (modelRecords == 128) journal.Status("runtime_model_limit_reached", 128);
                    }
                    catch { /* Model logging must not disable message capture or the read-only panel. */ }
                }
                return GameModelReader.Json(state, false);
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
