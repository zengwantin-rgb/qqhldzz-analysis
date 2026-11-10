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
        public string Kind = "numeric_array";
        public string Origin = "model_cache";
        public int[] Values;
    }
    public sealed class ModelCacheRoot
    {
        public string Name;
        public bool FieldPresent;
        public bool ObjectPresent;
        public int RegisteredCount = -1;
        public int Slots = -1;
        public int ValuesVisited;
        public string ReadStatus = "not_read";
    }
    public sealed class ModelCacheScan
    {
        public string Status = "no_numeric_candidates";
        public ModelCacheCandidate[] Candidates = new ModelCacheCandidate[0];
        public ModelCacheRoot[] Roots = new ModelCacheRoot[0];
        public int ObjectsVisited;
        public int UnsupportedContainers;
        public bool TimeBudgetHit;
        public int ReadFailures;
        public string[] ModelTypes = new string[0];
    }
    // Reads existing round fields only. String dictionary keys and Unity scene objects are excluded.
    public static class ModelCacheReader
    {
        static readonly object MetadataGate = new object();
        static readonly Dictionary<Type, FieldInfo[]> Metadata = new Dictionary<Type, FieldInfo[]>();
        static int pass;
        static FieldInfo Find(Type type, string name)
        {
            for (int level = 0; type != null && level < 8; level++, type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }
        static FieldInfo[] Fields(Type type)
        {
            lock (MetadataGate)
            {
                FieldInfo[] known; if (Metadata.TryGetValue(type, out known)) return known;
                var fields = new List<FieldInfo>(); Type current = type;
                for (int level = 0; current != null && level < 4; level++, current = current.BaseType)
                    foreach (var field in current.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    { fields.Add(field); if (fields.Count >= 129) break; }
                known = fields.ToArray(); if (Metadata.Count < 256) Metadata[type] = known; return known;
            }
        }
        static bool Number(object value, out int result)
        {
            result = 0;
            if (value is int) result = (int)value;
            else if (value is sbyte) result = (sbyte)value;
            else if (value is byte) result = (byte)value;
            else if (value is short) result = (short)value;
            else if (value is ushort) result = (ushort)value;
            else if (value is uint) result = unchecked((int)(uint)value);
            else return false;
            return true;
        }
        public static bool IsCoreList(object value)
        {
            if (value is Array) return ((Array)value).Rank == 1;
            if (value == null) return false;
            Type type = value.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Collections.Generic.List`1";
        }
        static bool DictionaryType(Type type)
        {
            for (int level = 0; type != null && level < 8; level++, type = type.BaseType)
                if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == "System.Collections.Generic.Dictionary`2") return true;
            return false;
        }
        sealed class Scan
        {
            internal readonly List<ModelCacheCandidate> Results = new List<ModelCacheCandidate>();
            internal readonly List<ModelCacheRoot> Roots = new List<ModelCacheRoot>();
            internal readonly List<string> ModelTypes = new List<string>();
            readonly List<object> visited = new List<object>();
            readonly int start = Environment.TickCount;
            readonly int milliseconds;
            readonly int rotation;
            internal bool Limited;
            internal bool TimeHit;
            internal int Unsupported;
            internal int ReadFailures;
            int integers;
            internal int VisitedCount { get { return visited.Count; } }
            internal Scan(object round, int budget) { visited.Add(round); milliseconds = budget; rotation = pass++; }
            bool Expired()
            {
                if ((uint)(Environment.TickCount - start) <= milliseconds) return false;
                Limited = true; TimeHit = true; return true;
            }
            static bool Scope(Type type)
            {
                Type current = type; bool modelBase = false;
                for (int level = 0; current != null && level < 8; level++, current = current.BaseType)
                {
                    if (current.FullName == "UnityEngine.Object" || current.FullName == "UnityEngine.MonoBehaviour") return false;
                    if (current.FullName == "GameModule.DataModel") modelBase = true;
                }
                string name = type.FullName ?? "";
                return modelBase || name == "HappyMahjong.Common.Blackboard" || name.StartsWith("HappyMahjong.Game.", StringComparison.Ordinal) || name.StartsWith("GameModule.", StringComparison.Ordinal) || name.StartsWith("HuanLeGameSo.", StringComparison.Ordinal);
            }
            static bool TileField(string name)
            {
                name = name.ToLowerInvariant();
                return (name.Contains("hand") || name.Contains("wall") || name.Contains("tile") || name.Contains("mj") || name.Contains("card") || name.Contains("pai")) && !name.Contains("account") && !name.Contains("token") && !name.Contains("playerid") && !name.Contains("uin") && !name.Contains("idcard") && !name.Contains("time") && !name.Contains("duration") && !name.Contains("clock");
            }
            static bool FaceScalar(string name)
            {
                name = name.ToLowerInvariant();
                return TileField(name) && (name.Contains("value") || name.Contains("face") || name == "tile" || name == "itile" || name == "chtile") && !name.Contains("count") && !name.Contains("index");
            }
            bool Add(object parent, string path, object value, int owner, string ownerKind, string kind)
            {
                if (parent == null) return false;
                int[] values;
                if (kind == "scalar") { int number; if (!Number(value, out number)) return false; values = new int[] { number }; }
                else
                {
                    IList list = IsCoreList(value) ? value as IList : null;
                    if (list == null || list.Count == 0) return false;
                    if (list.Count > 200 || integers + list.Count > 4096) { Limited = true; return false; }
                    values = new int[list.Count];
                    for (int index = 0; index < values.Length; index++) if (!Number(list[index], out values[index])) return false;
                }
                if (Results.Count >= 48 || integers + values.Length > 4096) { Limited = true; return false; }
                string fieldName = ownerKind; int seat = owner;
                foreach (string name in new string[] { "nSeatID", "nSeat", "seatID", "seatId", "viewSeat" })
                {
                    var field = Find(parent.GetType(), name); int candidate;
                    if (field != null && Number(field.GetValue(parent), out candidate) && candidate >= 0 && candidate <= 3) { fieldName = name; seat = candidate; break; }
                }
                integers += values.Length;
                string source = parent.GetType().FullName ?? "unknown";
                Results.Add(new ModelCacheCandidate { SourceType = source, FieldPath = path, OwnerField = fieldName, Owner = seat, Kind = kind, Origin = source.StartsWith("HuanLeGameSo.", StringComparison.Ordinal) ? "protocol_cache" : "model_cache", Values = values });
                return true;
            }
            internal void Root(object round, string fieldName, string name)
            {
                var root = new ModelCacheRoot { Name = name }; Roots.Add(root);
                var field = Find(round.GetType(), fieldName); root.FieldPresent = field != null;
                if (field == null) { root.ReadStatus = "field_missing"; return; }
                object value = field.GetValue(round); root.ObjectPresent = value != null;
                if (value == null) { root.ReadStatus = "null"; return; }
                if (name == "blackboard")
                {
                    var data = Find(value.GetType(), "data");
                    if (data != null) value = data.GetValue(value);
                }
                Walk(value, 0, name, null, false, -1, "none", root, null);
                if (root.ReadStatus == "not_read") { root.ReadStatus = TimeHit ? "budget_limited" : "not_dictionary"; if (!TimeHit) Unsupported++; }
            }
            internal void Walk(object value, int depth, string path, object parent, bool tileContext, int owner, string ownerKind, ModelCacheRoot root, long? selector)
            {
                if (value == null || Expired()) return;
                if (depth > 6) { Limited = true; return; }
                Type type = value.GetType();
                if (type.IsPrimitive || type.IsEnum || value is string) return;
                bool dictionary = DictionaryType(type), list = IsCoreList(value);
                if (!dictionary && !list && !Scope(type)) return;
                foreach (object previous in visited) if (Object.ReferenceEquals(previous, value)) return;
                if (visited.Count >= 96) { Limited = true; return; }
                visited.Add(value);
                Type ancestor = type;
                for (int level = 0; ancestor != null && level < 8; level++, ancestor = ancestor.BaseType)
                    if (ancestor.FullName == "GameModule.DataModel")
                    {
                        string modelType = type.FullName ?? "unknown";
                        if (!ModelTypes.Contains(modelType) && ModelTypes.Count < 64) ModelTypes.Add(modelType);
                        break;
                    }
                if (list)
                {
                    if (tileContext && Add(parent, path, value, owner, ownerKind, "numeric_array")) return;
                    IList items = value as IList;
                    if (items == null) { Unsupported++; return; }
                    int count = Math.Min(items.Count, 32); if (items.Count > 32) Limited = true;
                    for (int i = 0; i < count; i++) Walk(items[i], depth + 1, path + "[" + i + "]", parent, tileContext, items.Count == 4 ? i : owner, items.Count == 4 ? "array_index_candidate" : ownerKind, null, null);
                    return;
                }
                if (dictionary)
                {
                    var entriesField = Find(type, "_entries") ?? Find(type, "entries");
                    var countField = Find(type, "_count") ?? Find(type, "count"); var freeField = Find(type, "_freeCount") ?? Find(type, "freeCount");
                    int count = -1, free = 0;
                    if (countField != null) Number(countField.GetValue(value), out count);
                    if (freeField != null) Number(freeField.GetValue(value), out free);
                    Array entries = entriesField == null ? null : entriesField.GetValue(value) as Array;
                    if (root != null) { root.RegisteredCount = count < 0 ? -1 : Math.Max(0, count - free); root.Slots = entries == null ? 0 : entries.Length; }
                    if (entries == null || entries.Rank != 1) { if (root != null) root.ReadStatus = count == 0 ? "empty" : "entries_missing"; if (count != 0) Unsupported++; return; }
                    if (root != null) root.ReadStatus = "read";
                    int slots = Math.Min(entries.Length, 512); if (entries.Length > 512) Limited = true;
                    for (int item = 0; item < slots; item++)
                    {
                        if (Expired()) { if (root != null) root.ReadStatus = "budget_limited"; return; }
                        int step = slots % 17 == 0 ? 19 : 17;
                        int index = (item + (int)(((long)(rotation & 0x7fffffff) * step) % Math.Max(1, slots))) % Math.Max(1, slots);
                        object entry = entries.GetValue(index); if (entry == null) continue;
                        var member = Find(entry.GetType(), "value"); if (member == null) { Unsupported++; continue; }
                        object child = member.GetValue(entry); if (child == null) continue;
                        if (root != null) root.ValuesVisited++;
                        int keyOwner = owner; string keyKind = ownerKind;
                        if (tileContext)
                        {
                            var key = Find(entry.GetType(), "key"); int number;
                            if (key != null && key.FieldType == typeof(int) && Number(key.GetValue(entry), out number) && number >= 0 && number <= 3) { keyOwner = number; keyKind = "dictionary_key_candidate"; }
                        }
                        Walk(child, depth + 1, path + "[value:" + index + "]", parent, tileContext, keyOwner, keyKind, null, null);
                    }
                    return;
                }
                FieldInfo[] fields = Fields(type);
                if (ProtocolUnions.IsUnion(type.FullName))
                {
                    string chosen = selector.HasValue ? ProtocolUnions.Select(type.FullName, selector.Value) : null;
                    var selected = chosen == null ? null : Find(type, chosen);
                    if (selected == null) { Unsupported++; return; } fields = new FieldInfo[] { selected };
                }
                if (fields.Length > 128) Limited = true;
                for (int i = 0; i < fields.Length && i < 128; i++)
                {
                    if (Expired()) return;
                    var field = fields[i]; string next = path + "." + field.Name;
                    if (next.Length > 512) { Limited = true; continue; }
                    object child;
                    try { child = field.GetValue(value); } catch { ReadFailures++; continue; }
                    if (child == null) continue;
                    bool target = tileContext || TileField(field.Name);
                    if (FaceScalar(field.Name)) Add(value, next, child, owner, ownerKind, "scalar");
                    long? childSelector = null;
                    if (field.Name == "stValue") { var tag = Find(type, "iType") ?? Find(type, "nType"); int number; if (tag != null && Number(tag.GetValue(value), out number)) childSelector = number; }
                    Walk(child, depth + 1, next, value, target, owner, ownerKind, null, childSelector);
                }
            }
        }
        public static ModelCacheScan Capture(object round, int budgetMilliseconds = 25)
        {
            var result = new ModelCacheScan();
            try
            {
                var scan = new Scan(round, Math.Max(1, Math.Min(1000, budgetMilliseconds)));
                // Registered rule DataModels are held here, not in the score/condition blackboard.
                try { scan.Root(round, "m_moduleDataModels", "module_models"); } catch { scan.ReadFailures++; }
                try { scan.Root(round, "m_blackboard", "blackboard"); } catch { scan.ReadFailures++; }
                var values = scan.Results.ToArray();
                for (int i = 0; i < values.Length; i++) for (int j = i + 1; j < values.Length; j++)
                {
                    int order = String.CompareOrdinal(values[j].SourceType, values[i].SourceType);
                    if (order == 0) order = String.CompareOrdinal(values[j].FieldPath, values[i].FieldPath);
                    if (order < 0) { var swap = values[i]; values[i] = values[j]; values[j] = swap; }
                }
                result.Candidates = values; result.Roots = scan.Roots.ToArray(); result.ObjectsVisited = scan.VisitedCount; result.UnsupportedContainers = scan.Unsupported; result.TimeBudgetHit = scan.TimeHit; result.ReadFailures = scan.ReadFailures; result.ModelTypes = scan.ModelTypes.ToArray();
                for (int i = 0; i < result.ModelTypes.Length; i++) for (int j = i + 1; j < result.ModelTypes.Length; j++)
                    if (String.CompareOrdinal(result.ModelTypes[j], result.ModelTypes[i]) < 0) { string swap = result.ModelTypes[i]; result.ModelTypes[i] = result.ModelTypes[j]; result.ModelTypes[j] = swap; }
                result.Status = scan.ReadFailures > 0 ? "partial_read" : scan.Limited ? "budget_limited" : values.Length > 0 ? "candidates_recorded" : scan.Unsupported > 0 ? "unsupported_storage" : "no_numeric_candidates";
            }
            catch { result.Status = "cache_read_failed"; }
            return result;
        }
    }
}
