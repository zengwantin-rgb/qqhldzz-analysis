using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using MJDiagnostic;
namespace GameModule
{
    public abstract class DataModel { public object roundModel; }
    public sealed class ModuleHandCacheFixture : DataModel
    {
        public Dictionary<int,List<int>> playerHands=new Dictionary<int,List<int>> {{1,new List<int>{11,12,13}},{3,new List<int>{0,0,0}}};
        public sbyte[] handBytes=new sbyte[]{-1,1,2};
        public List<TileFaceFixture> tileFaces=new List<TileFaceFixture>{new TileFaceFixture()};
        public List<int> handPlayerID=new List<int>{987654321};
        public object ui;
        public int GetterCalls;
        public int[] HandTiles {get{GetterCalls++;throw new Exception("Game getter must not run");}}
    }
    public sealed class TileFaceFixture { public short nSeatID=2;public sbyte iTileFace=21; }
    public sealed class IgnoredUnityCacheFixture : UnityEngine.Object { public int[] handTiles=new int[]{99887766}; }
}
namespace UnityEngine { public class Object { } }
namespace OtherModuleSpace
{
    public sealed class DerivedHandCacheFixture : GameModule.DataModel { public int[] otherHandTiles=new int[]{31,32}; }
}
static class ModuleCacheFixture
{
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    public static void Run()
    {
        var model=new GameModule.ModuleHandCacheFixture();var other=new OtherModuleSpace.DerivedHandCacheFixture();
        var modules=new Dictionary<string,object>{{"private-module-token",model},{"another-private-key",other}};
        var root=new HappyMahjong.Game.GuoBiaoRoundModel(new IList[]{new List<int>{1},new List<int>{0},new List<int>{0},new List<int>{0}},modules:modules);
        model.roundModel=root;other.roundModel=root;model.ui=new GameModule.IgnoredUnityCacheFixture();
        var scan=ModelCacheReader.Capture(root,1000);
        Check(scan.Status=="candidates_recorded" && scan.Roots.Length==2,"scan registered modules independently of an empty blackboard");
        Check(scan.Roots[0].Name=="module_models" && scan.Roots[0].RegisteredCount==2 && scan.Roots[0].ValuesVisited==2,"registered module coverage counters");
        bool dictionary=false,bytes=false,scalar=false,derived=false;
        foreach(var candidate in scan.Candidates)
        {
            Check(!candidate.FieldPath.Contains("roundModel") && !candidate.FieldPath.Contains("handPlayerID") && !candidate.SourceType.Contains("IgnoredUnity"),"skip round backlinks, identity fields and Unity objects");
            if(candidate.FieldPath.Contains("playerHands") && candidate.Owner==1 && candidate.OwnerField=="dictionary_key_candidate")dictionary=true;
            if(candidate.FieldPath.Contains("handBytes") && candidate.Values.Length==3 && candidate.Values[0]==-1)bytes=true;
            if(candidate.SourceType.EndsWith("TileFaceFixture") && candidate.Kind=="scalar" && candidate.Owner==2)scalar=true;
            if(candidate.SourceType.StartsWith("OtherModuleSpace."))derived=true;
        }
        Check(dictionary && bytes && scalar && derived,"dictionary hands, signed-byte arrays, tile objects and derived models covered");
        Check(scan.ModelTypes.Length==2 && scan.ReadFailures==0,"model type coverage and field read failures are explicit");
        Check(model.GetterCalls==0,"module scan never invokes game properties");
        var state=GameModelReader.Capture(root,"live");state.Cache=scan;string json=GameModelReader.Json(state);
        using(var parsed=JsonDocument.Parse(json))Check(parsed.RootElement.GetProperty("cache_roots")[0].GetProperty("registered_count").GetInt32()==2,"coverage JSON");
        Check(!json.Contains("private-module-token") && !json.Contains("another-private-key") && !json.Contains("987654321") && !json.Contains("99887766"),"keys, account arrays and scene data are not captured");
        Check(!GameModelReader.Panel(state).Contains("1条"),"unverified module candidates never become rendered opponent hand values");
    }
}
