using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using MJDiagnostic;
static class ObserverModelFixture
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static HappyMahjong.Game.GuoBiaoRoundModel Model(){return new HappyMahjong.Game.GuoBiaoRoundModel(new IList[]{new List<int>{1,2,3},new List<int>{0,0,0},new List<int>{31,32},new List<int>{0,0}});}
    static string Phase(){using(var json=JsonDocument.Parse(Observer.ReadModel()))return json.RootElement.GetProperty("phase").GetString();}
    public static void Run(string root)
    {
        string events=Path.Combine(root,"game-events-model-fixture.log"),status=Path.Combine(root,"observer-status.log");
        File.WriteAllText(events,ObserverJournal.EventsHeader,new UTF8Encoding(false));
        var dispatcher=new strange.extensions.dispatcher.eventdispatcher.api.FixtureDispatcher();HappyMahjong.TSDK4CSharpService.dispatcher=dispatcher;
        HappyMahjong.Game.RoundModeUtil.RoundModel=Model();Check(Observer.Start(events,status),"model observer start");
        Check(Phase()=="unanchored","existing model has no claimed live phase before a new deal");
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV {iType=1});
        Check(Phase()=="live","observed deal anchors current model");
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV {iType=50});
        Check(Phase()=="after_hu","Hu phase is applied before the game changes its status");
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV {iType=1});
        Check(Phase()=="live","next deal clears the old Hu label");
        HappyMahjong.Game.RoundModeUtil.RoundModel=Model();Check(Phase()=="unanchored","different model cannot reuse the previous table's live anchor");
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV {iType=1});
        Check(Phase()=="live","new model can be anchored by its own deal");
        for(int i=0;i<513;i++)dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV {iType=3});
        dispatcher.Send(HappyMahjong.TSDKMessageEvent.OnTSDKGameMessage,new HuanLeGameSo.SoNotifyMsgTV {iType=50});
        Check(Phase()=="after_hu","journal budget must not freeze the live panel phase");
        string[] lines=File.ReadAllLines(events);int runtime=0;
        foreach(string line in lines){if(!line.StartsWith("{"))continue;using(var json=JsonDocument.Parse(line)){if(json.RootElement.GetProperty("kind").GetString()=="runtime_model_event"){runtime++;Check(json.RootElement.GetProperty("encoding").GetString()=="runtime_model_snapshot","separate runtime record encoding");}}}
        Check(runtime>0 && lines.Length==513,"runtime rows preserved inside the shared bounded journal");
        Observer.Stop();HappyMahjong.Game.RoundModeUtil.RoundModel=null;
        using(var json=JsonDocument.Parse(Observer.ReadModel()))Check(json.RootElement.GetProperty("status").GetString()=="waiting_for_table","stopped reader clears display");
    }
}
