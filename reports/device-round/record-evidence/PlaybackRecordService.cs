using System.IO;
using HLMJPlayback;
using HappyMahjong.Common;
using HappyMahjong.Loading;
using HappyMahjong.TSDKMessage;
using IFix;
using ProtoBuf;
using TSDK4CSharp;

namespace HappyMahjong.PlayerRecord;

public static class PlaybackRecordService
{
	private static bool m_isOpenBalancePanel = false;

	public static bool onlyShowBalance = false;

	public static bool IsFriendAssistBalanceDetail = false;

	public static bool onlyShowBalanceGMSwitch = true;

	private static string CGI_METHOD_RECORD_LIVE_BUILD_HTTPS = "https://happyapp.huanle.qq.com/hlmj/getplayback";

	private static string CGI_METHOD_RECORD_DEBUG = "https://hlmjtestcgi.huanle.qq.com/hlmj/getplayback";

	public static bool IsOpenBalancePanel
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(35037))
			{
				return WrappersManagerImpl.GetPatch(35037).__Gen_Wrap_11();
			}
			return m_isOpenBalancePanel;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(53708))
			{
				WrappersManagerImpl.GetPatch(53708).__Gen_Wrap_192(value);
			}
			else
			{
				m_isOpenBalancePanel = value;
			}
		}
	}

	public static void FetchPlaybackRecordList(uint uin)
	{
		if (WrappersManagerImpl.IsPatched(35672))
		{
			WrappersManagerImpl.GetPatch(35672).__Gen_Wrap_194(uin);
			return;
		}
		Log.Info("TSDKService::Send Fetch PlayBack Record !", "Shop");
		GetAllPlayerPlaybackRecordReq getAllPlayerPlaybackRecordReq = new GetAllPlayerPlaybackRecordReq();
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		getAllPlayerPlaybackRecordReq.UserUin = uin;
		getAllPlayerPlaybackRecordReq.Accounttype = (int)selfInfoData.eAccountType;
		getAllPlayerPlaybackRecordReq.Clienttype = tSDKGameData.nClientType;
		getAllPlayerPlaybackRecordReq.Supportmorethan32k = 1;
		Util.SendProtoBufferMsg(getAllPlayerPlaybackRecordReq, 18371);
	}

	public static void AddFavoriteRecord(int history_index, string gameTag = "")
	{
		if (WrappersManagerImpl.IsPatched(58702))
		{
			WrappersManagerImpl.GetPatch(58702).__Gen_Wrap_880(history_index, gameTag);
			return;
		}
		Log.Info("TSDKService::Send Add Favorite PlayBack Record !", "Shop");
		Util.SendProtoBufferMsg(new AddFavoritePlaybackRecordReq
		{
			HistoryIndex = history_index,
			GameTag = gameTag
		}, 18372);
	}

	public static void DeleteFavoriteRecord(int favorite_index)
	{
		if (WrappersManagerImpl.IsPatched(58703))
		{
			WrappersManagerImpl.GetPatch(58703).__Gen_Wrap_145(favorite_index);
			return;
		}
		Log.Info("TSDKService::Send Delete Favorite PlayBack Record !", "Shop");
		Util.SendProtoBufferMsg(new DeleteFavoritePlaybackRecordReq
		{
			Index = favorite_index
		}, 18373);
	}

	public static void MakeRecordPublic(bool isPublic)
	{
		if (WrappersManagerImpl.IsPatched(35912))
		{
			WrappersManagerImpl.GetPatch(35912).__Gen_Wrap_192(isPublic);
			return;
		}
		Log.Info("TSDKService::Send Make PlayBack Record Public : " + isPublic, "Shop");
		Util.SendProtoBufferMsg(new SetPlaybackViewSwitchReq
		{
			IsEnabled = isPublic
		}, 18382);
	}

	public static void SavePermanentRecord(PlaybackRecordType type, string gameTag, uint uin)
	{
		if (WrappersManagerImpl.IsPatched(58683))
		{
			WrappersManagerImpl.GetPatch(58683).__Gen_Wrap_3916(type, gameTag, uin);
			return;
		}
		Log.Info($"SavePermanentRecord gameTag: {gameTag}, uin: {uin}, recordType: {type}", "CSReplay");
		Util.SendProtoBufferMsg(new SavePermanentRecordReq
		{
			PlaybackRecordType = type,
			UserUin = uin,
			GameTag = gameTag
		}, 18641);
	}

	public static bool CheckPlaybackCGIIsOpen()
	{
		if (WrappersManagerImpl.IsPatched(58704))
		{
			return WrappersManagerImpl.GetPatch(58704).__Gen_Wrap_11();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("Playback", "CGIIsOpen");
	}

	public static bool PlaybackRecordReq(int recordIndex, PlaybackRecordType recordType, string gameTag = null, uint uin = 0u, bool isOpenBalancePanel = false, int gameType = 0, int recordSize = -1)
	{
		if (WrappersManagerImpl.IsPatched(58705))
		{
			return WrappersManagerImpl.GetPatch(58705).__Gen_Wrap_3924(recordIndex, recordType, gameTag, uin, isOpenBalancePanel, gameType, recordSize);
		}
		IsFriendAssistBalanceDetail = false;
		if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.Game || SceneSwitch.CurrentScene() == SceneSwitch.Scene.Loading)
		{
			SingletonBase<Toast>.GetInstance().showUGUI(LanguageKey.UNABLE_TO_VIEW_PLAYBACK_IN_OFFICE);
			Log.Info("PlaybackRecordReq already in game!", "CSReplay");
			return false;
		}
		if ((recordIndex < 0 && string.IsNullOrEmpty(gameTag)) || AppRoot.GetInstance().InGameBridgeBase.GetRecordReplaying())
		{
			Log.Info("PlaybackRecordReq recordIndex<0 or gameTag is null, or is in replaying", "CSReplay");
			return false;
		}
		Log.Info($"PlaybackRecordReq recordSize: {recordSize}", "CSReplay");
		if ((recordSize == -1 || recordSize > 29696) && CheckPlaybackCGIIsOpen())
		{
			PlaybackRecordReqByCGI(recordIndex, recordType, gameTag, uin, isOpenBalancePanel, gameType);
			return true;
		}
		bool flag = false;
		if (gameType != 0)
		{
			flag = AppRoot.GetInstance().InGameBridgeBase.CSReplayFunc(10, gameType);
		}
		Log.Info($"PlaybackRecordReq onlyReplayBalance: {flag}, isOpenBalancePanel: {isOpenBalancePanel} ", "CSReplay");
		if (!flag || isOpenBalancePanel)
		{
			onlyShowBalance = false;
			PlaybackRecordReqInBalance(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
		}
		else
		{
			onlyShowBalance = true;
			OnlyShowPlaybackBalanceView(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
		}
		return true;
	}

	public static void OnlyShowPlaybackBalanceView(int recordIndex, PlaybackRecordType recordType, string gameTag = null, uint uin = 0u, bool isOpenBalancePanel = false)
	{
		if (WrappersManagerImpl.IsPatched(58710))
		{
			WrappersManagerImpl.GetPatch(58710).__Gen_Wrap_3922(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
			return;
		}
		Log.Info($"OnlyShowPlaybackBalanceView index: {recordIndex}, gameTag:{gameTag}", "CSReplay");
		Util.SendProtoBufferMsg(new GetPlaybackRecordReq
		{
			PlaybackRecordType = recordType,
			Index = recordIndex,
			GameTag = gameTag,
			UserUin = ((uin == 0) ? PlayerDataMgr.GetInstance().GetSelfInfo().playerUin : uin)
		}, 18378);
	}

	public static void PlaybackRecordReqInBalance(int recordIndex, PlaybackRecordType recordType, string gameTag = null, uint uin = 0u, bool isOpenBalancePanel = false)
	{
		if (WrappersManagerImpl.IsPatched(58709))
		{
			WrappersManagerImpl.GetPatch(58709).__Gen_Wrap_3922(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
			return;
		}
		Log.Info($"PlaybackRecordReq index: {recordIndex}, gameTag:{gameTag}", "CSReplay");
		IsOpenBalancePanel = isOpenBalancePanel;
		Util.SendProtoBufferMsg(new GetPlaybackRecordReq
		{
			PlaybackRecordType = recordType,
			Index = recordIndex,
			GameTag = gameTag,
			UserUin = ((uin == 0) ? PlayerDataMgr.GetInstance().GetSelfInfo().playerUin : uin)
		}, 18378);
	}

	public static void PlaybackRecordDataRes(byte[] buffer, int bufferLen)
	{
		if (WrappersManagerImpl.IsPatched(35036))
		{
			WrappersManagerImpl.GetPatch(35036).__Gen_Wrap_10(buffer, bufferLen);
			return;
		}
		Log.Info("ReplayRecordDataRes ReplayAllRecordRes isOpenPanel: " + IsOpenBalancePanel, "CSReplay");
		using MemoryStream source = new MemoryStream(buffer, 0, bufferLen);
		OnResPlaybackRecordData(Serializer.Deserialize<GetPlaybackRecordRes>(source));
	}

	public static void OnResPlaybackRecordData(GetPlaybackRecordRes rsp)
	{
		if (WrappersManagerImpl.IsPatched(35038))
		{
			WrappersManagerImpl.GetPatch(35038).__Gen_Wrap_0(rsp);
		}
		else if (onlyShowBalance)
		{
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.RecvReplayRecordDataAndPlay, rsp);
		}
		else if (IsOpenBalancePanel)
		{
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.RecvReplayRecordDataAndOpenStreamPanel, rsp);
		}
		else
		{
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.RecvReplayRecordDataAndPlay, rsp);
		}
	}

	public static bool PlaybackRecordReqByCGI(int recordIndex, PlaybackRecordType recordType, string gameTag = null, uint uin = 0u, bool isOpenBalancePanel = false, int gameType = 0)
	{
		if (WrappersManagerImpl.IsPatched(58706))
		{
			return WrappersManagerImpl.GetPatch(58706).__Gen_Wrap_3923(recordIndex, recordType, gameTag, uin, isOpenBalancePanel, gameType);
		}
		IsFriendAssistBalanceDetail = false;
		if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.Game || SceneSwitch.CurrentScene() == SceneSwitch.Scene.Loading)
		{
			SingletonBase<Toast>.GetInstance().showUGUI(LanguageKey.UNABLE_TO_VIEW_PLAYBACK_IN_OFFICE);
			Log.Info("PlaybackRecordReqByCGI already in game!", "CSReplay");
			return false;
		}
		if ((recordIndex < 0 && string.IsNullOrEmpty(gameTag)) || AppRoot.GetInstance().InGameBridgeBase.GetRecordReplaying())
		{
			Log.Info("PlaybackRecordReqByCGI recordIndex<0 or gameTag is null, or is in replaying", "CSReplay");
			return false;
		}
		bool flag = false;
		if (gameType != 0)
		{
			flag = AppRoot.GetInstance().InGameBridgeBase.CSReplayFunc(10, gameType);
		}
		Log.Debug($"PlaybackRecordReqByCGI onlyReplayBalance: {flag}, isOpenBalancePanel: {isOpenBalancePanel} ", "CSReplay");
		if (!flag || isOpenBalancePanel)
		{
			onlyShowBalance = false;
			PlaybackRecordReqInBalanceByCGI(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
		}
		else
		{
			onlyShowBalance = true;
			OnlyShowPlaybackBalanceViewByCGI(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
		}
		return true;
	}

	public static void PlaybackRecordReqInBalanceByCGI(int recordIndex, PlaybackRecordType recordType, string gameTag = null, uint uin = 0u, bool isOpenBalancePanel = false)
	{
		if (WrappersManagerImpl.IsPatched(58707))
		{
			WrappersManagerImpl.GetPatch(58707).__Gen_Wrap_3922(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
			return;
		}
		Log.Debug($"PlaybackRecordReqInBalanceByCGI index: {recordIndex}, gameTag:{gameTag}", "CSReplay");
		IsOpenBalancePanel = isOpenBalancePanel;
		uint num = ((uin == 0) ? PlayerDataMgr.GetInstance().GetSelfInfo().playerUin : uin);
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		string[] obj = new string[10] { "playbackrecordtype=", null, null, null, null, null, null, null, null, null };
		int num2 = (int)recordType;
		obj[1] = num2.ToString();
		obj[2] = "&index=";
		obj[3] = recordIndex.ToString();
		obj[4] = "&accounttype=";
		obj[5] = ((int)selfInfoData.eAccountType).ToString();
		obj[6] = "&clienttype=";
		obj[7] = tSDKGameData.nClientType.ToString();
		obj[8] = "&useruin=";
		obj[9] = num.ToString();
		string text = string.Concat(obj);
		if (!string.IsNullOrEmpty(gameTag))
		{
			text = text + "&gametag=" + gameTag;
		}
		SendPlaybackRecordCGI(text);
	}

	public static void OnlyShowPlaybackBalanceViewByCGI(int recordIndex, PlaybackRecordType recordType, string gameTag = null, uint uin = 0u, bool isOpenBalancePanel = false)
	{
		if (WrappersManagerImpl.IsPatched(58708))
		{
			WrappersManagerImpl.GetPatch(58708).__Gen_Wrap_3922(recordIndex, recordType, gameTag, uin, isOpenBalancePanel);
			return;
		}
		Log.Debug($"OnlyShowPlaybackBalanceViewByCGI index: {recordIndex}, gameTag:{gameTag}", "CSReplay");
		uint num = ((uin == 0) ? PlayerDataMgr.GetInstance().GetSelfInfo().playerUin : uin);
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		string[] obj = new string[12]
		{
			"playbackrecordtype=", null, null, null, null, null, null, null, null, null,
			null, null
		};
		int num2 = (int)recordType;
		obj[1] = num2.ToString();
		obj[2] = "&index=";
		obj[3] = recordIndex.ToString();
		obj[4] = "&accounttype=";
		obj[5] = ((int)selfInfoData.eAccountType).ToString();
		obj[6] = "&clienttype=";
		obj[7] = tSDKGameData.nClientType.ToString();
		obj[8] = "&useruin=";
		obj[9] = num.ToString();
		obj[10] = "&gametag=";
		obj[11] = gameTag;
		SendPlaybackRecordCGI(string.Concat(obj));
	}

	public static void SendPlaybackRecordCGI(string param)
	{
		if (WrappersManagerImpl.IsPatched(53710))
		{
			WrappersManagerImpl.GetPatch(53710).__Gen_Wrap_0(param);
			return;
		}
		TSDK4CSharp.TSDKService.GetSelfInfoData();
		EventReporter.ReportBegin(EventReporter.EEventName.TakeActivityAward);
		string text = Util.SelectUrlDependOnConfig(CGI_METHOD_RECORD_LIVE_BUILD_HTTPS, CGI_METHOD_RECORD_LIVE_BUILD_HTTPS, CGI_METHOD_RECORD_DEBUG);
		text = text + "?" + param;
		Log.Debug("SendPlaybackRecordCGI url: " + text, "CSReplay");
		TSDK4CSharp.TSDKService.SendCgiRequset(689, text, param, HttpRequestType.enHttpPost, 120u, false, "");
	}

	public static void PlaybackRecordDataResCGI(CgiRspInfo cgiInfo)
	{
		if (WrappersManagerImpl.IsPatched(35080))
		{
			WrappersManagerImpl.GetPatch(35080).__Gen_Wrap_0(cgiInfo);
			return;
		}
		Log.Debug("PlaybackRecordDataResCGI ReplayAllRecordRes isOpenPanel: " + IsOpenBalancePanel, "CSReplay");
		if (cgiInfo.lRspCode != 200)
		{
			return;
		}
		using MemoryStream source = new MemoryStream(cgiInfo.strRspData, 0, cgiInfo.strRspData.Length);
		GetPlaybackRecordRes data = Serializer.Deserialize<GetPlaybackRecordRes>(source);
		if (onlyShowBalance)
		{
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.RecvReplayRecordDataAndPlay, data);
		}
		else if (IsOpenBalancePanel)
		{
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.RecvReplayRecordDataAndOpenStreamPanel, data);
		}
		else
		{
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.RecvReplayRecordDataAndPlay, data);
		}
	}
}
