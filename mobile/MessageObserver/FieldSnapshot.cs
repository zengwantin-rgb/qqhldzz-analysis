using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace MJDiagnostic
{
    // Reads public fields only. Never invokes game properties, methods or serializers.
    public static class FieldSnapshot
    {
        sealed class Writer
        {
            internal readonly StringBuilder Text = new StringBuilder();
            int nodes;
            internal void Put(string value) { if (Text.Length + value.Length > 192 * 1024) throw new InvalidOperationException("Snapshot budget"); Text.Append(value); }
            void Quote(string value)
            {
                if (value.Length > 4096) throw new InvalidOperationException("String budget");
                Put("\""); foreach (char c in value) { if (c == '"' || c == '\\') Put("\\" + c); else if (c < 32) Put("\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture)); else Put(c.ToString()); } Put("\"");
            }
            internal void Value(object value, int depth)
            {
                if (++nodes > 16000 || depth > 16) throw new InvalidOperationException("Snapshot traversal budget");
                if (value == null) { Put("null"); return; }
                Type type = value.GetType();
                if (value is string) { Quote((string)value); return; }
                if (value is bool) { Put((bool)value ? "true" : "false"); return; }
                if (type.IsEnum) { Put(Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture)); return; }
                if (type.IsPrimitive || value is decimal) { Put(Convert.ToString(value, CultureInfo.InvariantCulture)); return; }
                Array array = value as Array;
                if (array != null)
                {
                    if (array.Rank != 1 || array.Length > 4096) throw new InvalidOperationException("Array budget");
                    Put("["); for (int i = 0; i < array.Length; i++) { if (i != 0) Put(","); Value(array.GetValue(i), depth + 1); } Put("]"); return;
                }
                string name = type.FullName;
                if (name == null || !name.StartsWith("HuanLeGameSo.", StringComparison.Ordinal)) throw new InvalidOperationException("Unsupported snapshot type");
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                if (fields.Length > 256) throw new InvalidOperationException("Field budget");
                Put("{"); for (int i = 0; i < fields.Length; i++) { if (i != 0) Put(","); Quote(fields[i].Name); Put(":"); Value(fields[i].GetValue(value), depth + 1); } Put("}");
            }
        }
        public static byte[] Encode(object message) { var writer = new Writer(); writer.Value(message, 0); byte[] bytes = Encoding.UTF8.GetBytes(writer.Text.ToString()); if (bytes.Length > ObserverJournal.MaxPayload) throw new InvalidOperationException("Encoded snapshot budget"); return bytes; }
    }
}
