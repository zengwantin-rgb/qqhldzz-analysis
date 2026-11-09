using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace MJDiagnostic
{
    public sealed class ModelHand
    {
        public int ViewSeat;
        public int Seat;
        public int[] Tiles;
        public int[] ConcealedKongs = new int[0];
        public Dictionary<string, int[]> Aux = new Dictionary<string, int[]>();
        public string AuxStatus = "not_read";
    }
    public sealed class ModelState
    {
        public string Status;
        public string Phase;
        public int GameType = -1;
        public int GameStatus = -1;
        public int SelfSeat = -1;
        public int PlayerCount;
        public ModelHand[] Hands = new ModelHand[0];
        public int Generation;
        public ModelCacheScan Cache = new ModelCacheScan();
    }
    public static class GameModelReader
    {
        static FieldInfo Field(Type type, string name)
        {
            for (int level = 0; type != null && level < 8; level++, type = type.BaseType)
            {
                FieldInfo info = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (info != null) return info;
            }
            throw new MissingFieldException();
        }
        static object Value(object instance, string name) { if (instance == null) throw new InvalidOperationException(); return Field(instance.GetType(), name).GetValue(instance); }
        static int Number(object instance, string name) { return Convert.ToInt32(Value(instance, name), CultureInfo.InvariantCulture); }
        static bool KnownModel(Type type)
        {
            for (int level = 0; type != null && level < 8; level++, type = type.BaseType)
                if (type.FullName == "HappyMahjong.Game.GuoBiaoRoundModel") return true;
            return false;
        }
        static object Optional(object instance, string name)
        {
            try { return Value(instance, name); } catch (MissingFieldException) { return null; }
        }
        static void ReadAux(object tiles, ModelHand hand)
        {
            try
            {
                foreach (string name in new string[] { "curRedrawMjValue", "LastMJ", "m_tianHuTile" })
                {
                    object value = Optional(tiles, name); if (value is int) hand.Aux[name] = new int[] { (int)value };
                }
                foreach (string name in new string[] { "m_multiTileRedrawList", "m_laiZhiList" })
                {
                    object value = Optional(tiles, name); IList list = ModelCacheReader.IsCoreList(value) ? value as IList : null;
                    if (list == null || list.Count > 16) continue;
                    var values = new int[list.Count]; bool valid = true;
                    for (int i = 0; i < values.Length; i++) { if (!(list[i] is int)) { valid = false; break; } values[i] = (int)list[i]; }
                    if (valid) hand.Aux[name] = values;
                }
                object stored = Optional(tiles, "m_anGangList"); IList kongs = ModelCacheReader.IsCoreList(stored) ? stored as IList : null;
                if (stored != null && kongs == null) { hand.AuxStatus = "unsupported_kong_list"; return; }
                if (kongs != null && kongs.Count > 4) { hand.AuxStatus = "invalid_kong_count"; return; }
                if (kongs != null)
                {
                    var values = new int[kongs.Count];
                    for (int i = 0; i < values.Length; i++)
                    {
                        object kong = kongs[i];
                        if (kong == null || kong.GetType().FullName != "HappyMahjong.Game.GangInfo") { hand.AuxStatus = "unsupported_kong_entry"; return; }
                        values[i] = Number(kong, "mjValue");
                    }
                    hand.ConcealedKongs = values;
                }
                hand.AuxStatus = stored == null ? "aux_partial" : "read";
            }
            catch { hand.AuxStatus = "aux_read_failed"; }
        }
        static string Phase(string phase) { return phase == "live" || phase == "after_hu" || phase == "stale" ? phase : "unanchored"; }
        public static ModelState Capture(object model, string phase)
        {
            var state = new ModelState { Status = "waiting_for_table", Phase = Phase(phase) };
            if (model == null) return state;
            if (!KnownModel(model.GetType())) { state.Status = "unsupported_model"; return state; }
            try
            {
                state.GameType = Number(model, "m_gameType");
                if (state.GameType != 149) { state.Status = "unsupported_game"; return state; }
                state.GameStatus = Number(model, "m_gameStatus");
                if (state.GameStatus != 1) { state.Status = "not_playing"; return state; }
                state.PlayerCount = Number(model, "m_playerCount"); state.SelfSeat = Number(model, "m_selfSeat");
                if (state.PlayerCount != 4 || state.SelfSeat < 0 || state.SelfSeat >= 4) { state.Status = "invalid_seats"; return state; }
                Array players = Value(model, "m_playerGameInfo") as Array;
                if (players == null || players.Rank != 1 || players.Length != 4) { state.Status = "invalid_seats"; return state; }
                var hands = new ModelHand[4];
                for (int view = 0; view < 4; view++)
                {
                    object player = players.GetValue(view);
                    object tiles = Value(player, "tiles");
                    object stored = Value(tiles, "m_inHandList");
                    IList list = ModelCacheReader.IsCoreList(stored) ? stored as IList : null;
                    if (list == null || list.Count > 32) { state.Status = "invalid_hand_list"; return state; }
                    int count = list.Count; var values = new int[count];
                    for (int index = 0; index < count; index++)
                    {
                        object tile = list[index];
                        if (!(tile is int)) { state.Status = "invalid_hand_list"; return state; }
                        values[index] = (int)tile;
                    }
                    hands[view] = new ModelHand { ViewSeat = view, Seat = (state.SelfSeat + view) % 4, Tiles = values };
                    ReadAux(tiles, hands[view]);
                }
                state.Hands = hands; state.Cache = ModelCacheReader.Capture(model); state.Status = "ok"; return state;
            }
            catch (MissingFieldException) { state.Status = "model_fields_missing"; }
            catch { state.Status = "model_read_failed"; }
            state.Hands = new ModelHand[0]; return state;
        }
        public static bool ValidTile(int tile)
        {
            return tile >= 1 && tile <= 9 || tile >= 11 && tile <= 19 || tile >= 21 && tile <= 29 || tile >= 31 && tile <= 37;
        }
        static string TileName(int tile)
        {
            if (tile >= 1 && tile <= 9) return tile.ToString(CultureInfo.InvariantCulture) + "万";
            if (tile >= 11 && tile <= 19) return (tile - 10).ToString(CultureInfo.InvariantCulture) + "条";
            if (tile >= 21 && tile <= 29) return (tile - 20).ToString(CultureInfo.InvariantCulture) + "筒";
            string[] honors = { "东", "南", "西", "北", "中", "发", "白" };
            return tile >= 31 && tile <= 37 ? honors[tile - 31] : "?";
        }
        public static string Panel(ModelState state)
        {
            if (state.Status != "ok")
            {
                if (state.Status == "waiting_for_table" || state.Status == "not_playing") return "客户端模型：等待牌局";
                if (state.Status == "unsupported_game") return "客户端模型：当前仅核对大众麻将";
                return "客户端模型：读取未完成，请导出记录";
            }
            bool confirmed = state.Phase == "live" && state.GameStatus == 1 || state.Phase == "after_hu";
            string title = state.Phase == "after_hu" ? "客户端模型 · 胡牌/结算阶段" : confirmed ? "客户端模型 · 对局中" : "客户端模型 · 等待阶段确认";
            var output = new StringBuilder(title);
            string[] names = { "自己", "下家", "对家", "上家" };
            for (int index = 1; index < state.Hands.Length; index++)
            {
                ModelHand hand = state.Hands[index]; output.Append('\n').Append(names[hand.ViewSeat]).Append("：");
                if (!confirmed) { output.Append("状态待确认"); continue; }
                int unknown = 0, invalid = 0; var known = new List<string>();
                foreach (int tile in hand.Tiles) { if (tile == 0) unknown++; else if (ValidTile(tile)) known.Add(TileName(tile)); else invalid++; }
                if (invalid != 0) output.Append("含未识别牌值，未展示");
                else if (hand.Tiles.Length == 0) output.Append("暂无手牌");
                else if (known.Count == 0) output.Append("牌背 ×").Append(unknown).Append("，未读到牌值");
                else { output.Append(string.Join(" ", known.ToArray())); if (unknown != 0) output.Append("（另有牌背 ×").Append(unknown).Append('）'); }
                if (hand.ConcealedKongs.Length != 0)
                {
                    output.Append("；暗杠：");
                    for (int i = 0; i < hand.ConcealedKongs.Length; i++) { if (i != 0) output.Append(' '); int tile = hand.ConcealedKongs[i]; output.Append(tile == 0 ? "牌背" : ValidTile(tile) ? TileName(tile) : "?"); }
                }
            }
            return output.ToString();
        }
        static string Quote(string value)
        {
            var text = new StringBuilder("\"");
            foreach (char character in value ?? "")
            {
                if (character == '\\' || character == '"') text.Append('\\').Append(character);
                else if (character < 32) text.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                else text.Append(character);
            }
            return text.Append('"').ToString();
        }

        public static string Json(ModelState state, bool includeCaches = true)
        {
            var output = new StringBuilder("{\"kind\":\"runtime_model\",\"schema\":1,\"source\":\"RoundModeUtil.RoundModel\",\"status\":");
            output.Append(Quote(state.Status)).Append(",\"phase\":").Append(Quote(state.Phase)).Append(",\"generation\":").Append(state.Generation.ToString(CultureInfo.InvariantCulture));
            output.Append(",\"game_type\":").Append(state.GameType.ToString(CultureInfo.InvariantCulture)).Append(",\"game_status\":").Append(state.GameStatus.ToString(CultureInfo.InvariantCulture));
            output.Append(",\"self_seat\":").Append(state.SelfSeat.ToString(CultureInfo.InvariantCulture)).Append(",\"player_count\":").Append(state.PlayerCount.ToString(CultureInfo.InvariantCulture)).Append(",\"seats\":[");
            for (int i = 0; i < state.Hands.Length; i++)
            {
                if (i != 0) output.Append(','); var hand = state.Hands[i];
                output.Append("{\"view_seat\":").Append(hand.ViewSeat).Append(",\"seat\":").Append(hand.Seat).Append(",\"hand\":[");
                for (int j = 0; j < hand.Tiles.Length; j++) { if (j != 0) output.Append(','); output.Append(hand.Tiles[j].ToString(CultureInfo.InvariantCulture)); }
                output.Append("],\"concealed_kongs\":[");
                for (int j = 0; j < hand.ConcealedKongs.Length; j++) { if (j != 0) output.Append(','); output.Append(hand.ConcealedKongs[j].ToString(CultureInfo.InvariantCulture)); }
                output.Append("],\"aux_status\":").Append(Quote(hand.AuxStatus)).Append(",\"aux_fields\":{"); bool firstAux = true;
                foreach (var auxiliary in hand.Aux)
                {
                    if (!firstAux) output.Append(','); firstAux = false; output.Append(Quote(auxiliary.Key)).Append(":[");
                    for (int j = 0; j < auxiliary.Value.Length; j++) { if (j != 0) output.Append(','); output.Append(auxiliary.Value[j].ToString(CultureInfo.InvariantCulture)); }
                    output.Append(']');
                }
                output.Append("}}");
            }
            output.Append("],\"cache_status\":").Append(Quote(state.Cache.Status));
            if (includeCaches)
            {
                output.Append(",\"cache_candidates\":[");
                for (int i = 0; i < state.Cache.Candidates.Length; i++)
                {
                    if (i != 0) output.Append(','); var candidate = state.Cache.Candidates[i];
                    output.Append("{\"source_type\":").Append(Quote(candidate.SourceType)).Append(",\"field\":").Append(Quote(candidate.FieldPath));
                    output.Append(",\"owner_field\":").Append(Quote(candidate.OwnerField)).Append(",\"owner_value\":").Append(candidate.Owner.ToString(CultureInfo.InvariantCulture)).Append(",\"values\":[");
                    for (int j = 0; j < candidate.Values.Length; j++) { if (j != 0) output.Append(','); output.Append(candidate.Values[j].ToString(CultureInfo.InvariantCulture)); }
                    output.Append("]}");
                }
                output.Append(']');
            }
            return output.Append(",\"panel_text\":").Append(Quote(Panel(state))).Append('}').ToString();

        }
    }
}
