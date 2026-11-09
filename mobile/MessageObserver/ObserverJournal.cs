using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MJDiagnostic
{
    public sealed class ObserverJournal
    {
        public const string EventsHeader = "MJMessageObserverEvents/1\n";
        public const string StatusHeader = "MJMessageObserverStatus/1\n";
        public const int MaxPayload = 256 * 1024;
        readonly string eventFile;
        readonly string statusFile;
        readonly long maxBytes;
        readonly int maxFrames;
        readonly object gate = new object();
        public int FrameCount { get; private set; }
        public bool LimitReached { get; private set; }

        public ObserverJournal(string events, string status, long byteLimit, int frameLimit)
        {
            if (Path.GetDirectoryName(events) != Path.GetDirectoryName(status) || Path.GetFileName(Path.GetDirectoryName(events)) != "MJMessageObserver" ||
                !Path.GetFileName(events).StartsWith("game-events-", StringComparison.Ordinal) || Path.GetExtension(events) != ".log" || Path.GetFileName(status) != "observer-status.log")
                throw new InvalidOperationException("Unexpected journal location");
            if ((File.GetAttributes(Path.GetDirectoryName(events)) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("Journal directory is a link");
            CheckHeader(events, EventsHeader);
            CheckHeader(status, StatusHeader);
            eventFile = events; statusFile = status;
            maxBytes = byteLimit; maxFrames = frameLimit;
        }
        static void CheckHeader(string path, string header)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Journal file is a link");
            byte[] expected = Encoding.UTF8.GetBytes(header);
            using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] actual = new byte[expected.Length];
                int used = 0, read;
                while (used < actual.Length && (read = input.Read(actual, used, actual.Length - used)) > 0) used += read;
                if (used != expected.Length) throw new InvalidOperationException("Incomplete journal header");
                for (int i = 0; i < expected.Length; i++) if (actual[i] != expected[i]) throw new InvalidOperationException("Unrelated journal file");
            }
        }
        public bool Append(string channel, int messageType, byte[] bytes, int used, string encoding = "reencoded_decoded_event")
        {
            if ((encoding != "reencoded_decoded_event" && encoding != "public_field_snapshot") || (channel != "tdr" && channel != "protobuf") || bytes == null || used <= 0 || used > bytes.Length || used > MaxPayload)
                throw new InvalidOperationException("Invalid event frame");
            lock (gate)
            {
                if (LimitReached) return false;
                string line = "{\"kind\":\"game_event\",\"schema\":1,\"sequence\":" + (FrameCount + 1).ToString(CultureInfo.InvariantCulture) +
                    ",\"channel\":\"" + channel + "\",\"message_type\":" + messageType.ToString(CultureInfo.InvariantCulture) +
                    ",\"encoding\":\"" + encoding + "\",\"bytes\":" + used.ToString(CultureInfo.InvariantCulture) +
                    ",\"payload_b64\":\"" + Convert.ToBase64String(bytes, 0, used) + "\"}\n";
                if (FrameCount >= maxFrames || new FileInfo(eventFile).Length + Encoding.UTF8.GetByteCount(line) > maxBytes)
                {
                    LimitReached = true;
                    Status("capture_limit_reached", FrameCount);
                    return false;
                }
                CheckHeader(eventFile, EventsHeader);
                File.AppendAllText(eventFile, line, new UTF8Encoding(false));
                FrameCount++;
                if (FrameCount == 1) Status("first_event_recorded", FrameCount);
                else if (FrameCount % 25 == 0) Status("capture_progress", FrameCount);
                return true;
            }
        }
        public void Error(string stage, Exception error, int messageType)
        {
            if (stage != "tdr_pack" && stage != "protobuf_encode" && stage != "field_snapshot" && stage != "journal_append") return;
            string category = error is System.IO.IOException ? "io" : error is UnauthorizedAccessException ? "access" : error is NullReferenceException ? "null_reference" : error is ArgumentException ? "argument" : error is InvalidOperationException ? "invalid_operation" : error is NotSupportedException ? "not_supported" : "other";
            lock (gate)
            {
                CheckHeader(statusFile, StatusHeader);
                string line = "{\"kind\":\"managed_status\",\"schema\":1,\"status\":\"serialization_failed\",\"value\":0,\"stage\":\"" + stage + "\",\"category\":\"" + category + "\",\"message_type\":" + messageType.ToString(CultureInfo.InvariantCulture) + "}\n";
                if (new FileInfo(statusFile).Length + Encoding.UTF8.GetByteCount(line) <= 64 * 1024) File.AppendAllText(statusFile, line, new UTF8Encoding(false));
            }
        }
        public void Status(string code, int value)
        {
            // Fixed caller codes only; never serialize exception messages or paths.
            if (code != "first_event_recorded" && code != "capture_progress" && code != "unexpected_payload" && code != "observer_started" && code != "capture_limit_reached" && code != "serialization_failed" && code != "capture_failed" && code != "observer_stopped") return;
            lock (gate)
            {
                CheckHeader(statusFile, StatusHeader);
                string line = "{\"kind\":\"managed_status\",\"schema\":1,\"status\":\"" + code + "\",\"value\":" + value.ToString(CultureInfo.InvariantCulture) + "}\n";
                if (new FileInfo(statusFile).Length + Encoding.UTF8.GetByteCount(line) <= 64 * 1024)
                    File.AppendAllText(statusFile, line, new UTF8Encoding(false));
            }
        }
    }
}
