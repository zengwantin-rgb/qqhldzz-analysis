using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MJDiagnostic
{
    public sealed class ModelCacheCandidate
    {
        public string SourceType;
        public string FieldPath;
        public string OwnerField;
        public int Owner = -1;
        public int[] Values;
    }
    public sealed class ModelCacheScan
    {
        public string Status = "empty_or_unsupported";
        public ModelCacheCandidate[] Candidates = new ModelCacheCandidate[0];
    }
    // Only the current round's blackboard. Never scans Unity objects, static singletons, dictionary keys or accounts.
    public static class ModelCacheReader
    {
        static FieldInfo Find(Type type, string name)
        {
            for (int level = 0; type != null && level < 8; level++, type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }
        sealed class Scan
        {
            internal readonly List<ModelCacheCandidate> Results = new List<ModelCacheCandidate>();
            readonly List<object> visited = new List<object>();
            internal bool Limited;
            int integers;
            static bool Scope(Type type)
            {
                string name = type.FullName ?? "";
                return name == "HappyMahjong.Common.Blackboard" || name.StartsWith("HappyMahjong.Game.", StringComparison.Ordinal) || name.StartsWith("GameModule.", StringComparison.Ordinal) || name.StartsWith("HuanLeGameSo.", StringComparison.Ordinal);
            }
            static bool TileField(string name)
            {
                name = name.ToLowerInvariant();
                return (name.Contains("hand") || name.Contains("wall") || name.Contains("tile")) && !name.Contains("account") && !name.Contains("token") && !name.Contains("playerid") && !name.Contains("uin");
            }
            int[] Integers(object value)
            {
                IList list = value as IList;
                if (list == null || list.Count == 0 || list.Count > 200) return null;
                if (integers + list.Count > 1024) { Limited = true; return null; }
                var values = new int[list.Count];
                for (int i = 0; i < values.Length; i++) { object item = list[i]; if (!(item is int)) return null; values[i] = (int)item; }
                integers += values.Length; return values;
            }
            void Add(object parent, string path, object value)
            {
                if (Results.Count >= 16) { Limited = true; return; }
                int[] values = Integers(value); if (values == null) return;
                string ownerField = "none"; int owner = -1;
                foreach (string name in new string[] { "nSeatID", "nSeat", "seatID", "seatId", "viewSeat" })
                {
                    var field = Find(parent.GetType(), name); if (field == null || field.FieldType != typeof(int)) continue;
                    int number = (int)field.GetValue(parent); if (number < 0 || number > 3) continue;
                    ownerField = name; owner = number; break;
                }
                Results.Add(new ModelCacheCandidate { SourceType = parent.GetType().FullName, FieldPath = path, OwnerField = ownerField, Owner = owner, Values = values });
            }
            internal void Walk(object value, int depth, string path)
            {
                if (value == null || depth > 4) return;
                Type type = value.GetType();
                if (type.IsPrimitive || type.IsEnum || value is string) return;
                Type dictionaryType = type;
                for (int level = 0; dictionaryType != null && level < 8; level++, dictionaryType = dictionaryType.BaseType)
                    if (dictionaryType.IsGenericType && dictionaryType.GetGenericTypeDefinition().FullName == "System.Collections.Generic.Dictionary`2") break;
                bool dictionary = dictionaryType != null && dictionaryType.IsGenericType && dictionaryType.GetGenericTypeDefinition().FullName == "System.Collections.Generic.Dictionary`2";
                if (!dictionary && !Scope(type)) return;
                foreach (object previous in visited) if (Object.ReferenceEquals(previous, value)) return;
                if (visited.Count >= 32) { Limited = true; return; }
                visited.Add(value);
                if (dictionary)
                {
                    var entriesField = Find(type, "entries") ?? Find(type, "_entries");
                    Array entries = entriesField == null ? null : entriesField.GetValue(value) as Array;
                    if (entries == null || entries.Rank != 1) return;
                    if (entries.Length > 512) Limited = true;
                    for (int i = 0; i < Math.Min(entries.Length, 512); i++)
                    {
                        object entry = entries.GetValue(i); if (entry == null) continue;
                        var member = Find(entry.GetType(), "value");
                        if (member != null) Walk(member.GetValue(entry), depth + 1, path + "[value]");
                    }
                    return;
                }
                if (!Scope(type)) return;
                int fields = 0;
                for (int level = 0; type != null && level < 4; level++, type = type.BaseType)
                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        if (++fields > 128) { Limited = true; return; }
                        object child = field.GetValue(value); if (child == null) continue;
                        string next = path + "." + field.Name;
                        if (next.Length > 256) { Limited = true; continue; }
                        if (TileField(field.Name))
                        {
                            Add(value, next, child);
                            IList outer = child as IList;
                            if (outer != null && outer.Count <= 4)
                                for (int index = 0; index < outer.Count; index++) Add(value, next + "[" + index + "]", outer[index]);
                        }
                        Walk(child, depth + 1, next);
                    }
            }
        }
        public static ModelCacheScan Capture(object round)
        {
            var result = new ModelCacheScan();
            try
            {
                var board = Find(round.GetType(), "m_blackboard");
                if (board == null) { result.Status = "no_blackboard_field"; return result; }
                var scan = new Scan(); scan.Walk(board.GetValue(round), 0, "blackboard");
                result.Candidates = scan.Results.ToArray();
                result.Status = scan.Limited ? "budget_limited" : result.Candidates.Length == 0 ? "empty_or_unsupported" : "candidates_recorded";
            }
            catch { result.Status = "cache_read_failed"; }
            return result;
        }
    }
}
