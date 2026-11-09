using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using MJDiagnostic;
namespace HappyMahjong.Game
{
    public class GuoBiaoRoundModel
    {
        private int m_gameType;
        private int m_gameStatus;
        private int m_playerCount=4;
        protected int m_selfSeat;
        protected object[] m_playerGameInfo;
        public int PropertyCalls;
        public string PlayerAccountSecret="never include account fixture";
        public object GetTilesByViewSeat(int seat) { PropertyCalls++;throw new Exception("Game getter must not run"); }
        public int selfSeat { get { PropertyCalls++;throw new Exception("Game getter must not run"); } }
        public GuoBiaoRoundModel(IList[] hands,int type=149,int status=1,int self=0)
        {
            m_gameType=type;m_gameStatus=status;m_selfSeat=self;m_playerGameInfo=new object[4];
            for(int i=0;i<4;i++)m_playerGameInfo[i]=new ModelFixturePlayer { tiles=new ModelFixtureTiles(hands[i]) };
        }
    }
    public sealed class ModelFixturePlayer { public object tiles; }
    public sealed class ModelFixtureTiles
    {
        private IList m_inHandList;
        public IList inHandList { get { throw new Exception("Game getter must not run"); } }
        public ModelFixtureTiles(IList values){m_inHandList=values;}
    }
    public sealed class DerivedRoundFixture : GuoBiaoRoundModel { public DerivedRoundFixture(IList[] values):base(values){} }
}
static class ModelFixture
{
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    static IList[] Hands(){return new IList[]{new List<int>{1,2,3},new List<int>{0,0,0},new List<int>{11,21,35},new List<int>{0,31}};}
    public static void Run()
    {
        var lists=Hands();var model=new HappyMahjong.Game.GuoBiaoRoundModel(lists,self:2);
        ModelState live=GameModelReader.Capture(model,"live");
        Check(live.Status=="ok" && model.PropertyCalls==0,"read private fields without calling game accessors");
        Check(live.Hands[0].Seat==2 && live.Hands[1].Seat==3 && live.Hands[2].Seat==0,"convert view seats with actual self seat");
        string panel=GameModelReader.Panel(live);
        Check(panel.Contains("牌背 ×3") && panel.Contains("1条 1筒 中") && panel.Contains("另有牌背 ×1"),"show masked, known and mixed lists faithfully");
        Check(!panel.Contains("一万"),"opponent rows must not use own hand values");
        ((List<int>)lists[2])[0]=19;
        Check(live.Hands[2].Tiles[0]==11,"copy runtime list instead of keeping mutable game data");
        string json=GameModelReader.Json(live);using(var parsed=JsonDocument.Parse(json))Check(parsed.RootElement.GetProperty("kind").GetString()=="runtime_model","runtime snapshot JSON");
        Check(!json.Contains("account fixture"),"no unrelated game fields serialized");
        var after=GameModelReader.Capture(model,"after_hu");Check(GameModelReader.Panel(after).Contains("胡牌/结算"),"post-Hu faces explicitly labeled");
        Check(!GameModelReader.Panel(GameModelReader.Capture(model,"unanchored")).Contains("9条"),"unanchored phase does not show faces");
        Check(!GameModelReader.Panel(GameModelReader.Capture(model,"stale")).Contains("9条"),"stale phase does not show faces");
        Check(GameModelReader.Capture(null,"live").Status=="waiting_for_table","clear panel when model absent");
        Check(GameModelReader.Capture(new HappyMahjong.Game.GuoBiaoRoundModel(Hands(),status:0),"live").Status=="not_playing","clear panel when model no longer playing");
        Check(GameModelReader.Capture(new HappyMahjong.Game.GuoBiaoRoundModel(Hands(),type:150),"live").Status=="unsupported_game","game type guard");
        Check(GameModelReader.Capture(new HappyMahjong.Game.DerivedRoundFixture(Hands()),"live").Status=="ok","inherited fields found");
        var oversized=Hands();oversized[1]=new List<int>(new int[33]);Check(GameModelReader.Capture(new HappyMahjong.Game.GuoBiaoRoundModel(oversized),"live").Status=="invalid_hand_list","bounded model reads");
        var invalid=Hands();invalid[2]=new List<int>{10};Check(GameModelReader.Panel(GameModelReader.Capture(new HappyMahjong.Game.GuoBiaoRoundModel(invalid),"live")).Contains("含未识别牌值"),"never invent a label for invalid tile code");
        Check(!GameModelReader.ValidTile(0) && !GameModelReader.ValidTile(10) && GameModelReader.ValidTile(37),"source tile code ranges");
    }
}
