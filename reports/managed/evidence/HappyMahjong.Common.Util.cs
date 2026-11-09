using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Configuration;
using CrashSight;
using DBAssistSvr;
using DG.Tweening;
using GCloud.MSDK;
using GCloud.MSDK.MiniJSON;
using GameSvrBase;
using Google.Protobuf;
using Google.Protobuf.Collections;
using HLFetchRelationRankExtInfoMsgPara;
using HappyBridge.Util;
using HappyMahjong.Audio;
using HappyMahjong.Avatar;
using HappyMahjong.Bootstrap;
using HappyMahjong.BuyBeans;
using HappyMahjong.Cameramove;
using HappyMahjong.ChoiceSex;
using HappyMahjong.CloudGame;
using HappyMahjong.Common.Finder;
using HappyMahjong.Common.GM;
using HappyMahjong.Core;
using HappyMahjong.Core.Common;
using HappyMahjong.Effect;
using HappyMahjong.FazaiNew;
using HappyMahjong.Game;
using HappyMahjong.Loading;
using HappyMahjong.Login;
using HappyMahjong.OlympicMatch;
using HappyMahjong.Operation;
using HappyMahjong.PC;
using HappyMahjong.PlatformServices;
using HappyMahjong.PlayerRecord;
using HappyMahjong.PrivateRoom;
using HappyMahjong.Puffer;
using HappyMahjong.RankGrading;
using HappyMahjong.SSRInstituteSpace;
using HappyMahjong.SelectionScene;
using HappyMahjong.Setting;
using HappyMahjong.ShopAndBag;
using HappyMahjong.TSDKMessage;
using HappyMahjong.Tutorial;
using HappyMahjong.TwoVsTwo;
using HappyMahjong.update;
using IFix;
using IFix.Core;
using ProtoBuf;
using RankedBattlePassSpace;
using TMPro;
using TSDK4CSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using WebSocketSharp;
using strange.extensions.dispatcher.api;

namespace HappyMahjong.Common;

public class Util
{
	public enum KefuEnterScene
	{
		Login,
		Recharge,
		Setting,
		Activity
	}

	private class URLParam
	{
		public const string key_accountType = "$ACCOUNTTYPE$";

		public const string key_uin = "$UIN$";

		public const string key_clientType = "$CLIENTTYPE$";

		public const string key_clientVersion = "$CLIENTVERSION$";

		public const string key_gameID = "$GAMEID$";

		public const string key_channelID = "$CHANNELID$";

		public const string key_systemVersion = "$SYSTEMVERSION$";

		public const string key_payToken = "$PAYTOKEN$";

		public const string key_openid = "$OPENID$";

		public const string key_openKey = "$OPENKEY$";

		public const string key_pfKey = "$PFKEY$";

		public const string key_pf = "$PF$";

		public const string key_zoneID = "$ZONEID$";
	}

	public delegate void BytesDelegalte(byte[] bytes);

	public enum BeanStringFormat
	{
		Normal,
		WanExtend
	}

	public enum RoomInfoSearchType
	{
		IgnoreRoomType,
		BeanRoom,
		GoldCoinRoom
	}

	[CompilerGenerated]
	private sealed class <LoadConfigByPath>d__417 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private object <>2__current;

		public string path;

		public BytesDelegalte cb;

		private WWW <www>5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return <>2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return <>2__current;
			}
		}

		[DebuggerHidden]
		public <LoadConfigByPath>d__417(int <>1__state)
		{
			this.<>1__state = <>1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			switch (<>1__state)
			{
			default:
				return false;
			case 0:
			{
				<>1__state = -1;
				if (path.Contains("://"))
				{
					<www>5__2 = new WWW(path);
					<>2__current = <www>5__2;
					<>1__state = 1;
					return true;
				}
				byte[] bytes = File.ReadAllBytes(path);
				if (cb != null)
				{
					cb(bytes);
				}
				break;
			}
			case 1:
				<>1__state = -1;
				try
				{
					if (cb != null)
					{
						cb(<www>5__2.bytes);
					}
				}
				catch (Exception)
				{
				}
				<www>5__2.Dispose();
				<www>5__2 = null;
				<www>5__2 = null;
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private static readonly string LoginKefuUrl = "kf.qq.com/touch/scene_product.html?scene_id=kf8445";

	private static readonly string RechargeKefuUrl = "kf.qq.com/touch/scene_product.html?scene_id=kf8446";

	private static readonly string SettingKefuUrl = "kf.qq.com/touch/scene_product.html?scene_id=kf8448";

	private static readonly string ActivityKefuUrl = "kf.qq.com/touch/scene_product.html?scene_id=kf8447";

	private const string BEACON_APP_KEY_IOS = "i1101070761";

	private static readonly int[] s_clothLevelWeights = new int[5] { 1, 1, 2, 4, 6 };

	public static readonly string[] AvatarLevelNames = new string[5] { "c", "b", "a", "s", "ssr" };

	private static GameObject scene360FXObj;

	private static GameObject scene360Obj;

	private static GameObject hotloadObj;

	private static Color WhiteColor = new Color(1f, 1f, 1f, 1f);

	private static Color GrayColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	public static readonly float DesignWidth = 1336f;

	public static readonly float DesignHeight = 752f;

	public const string key_LobbyCameraScale = "LobbyCameraScale_784";

	public const string UI_MainPos_near = "MainPos_near";

	public const string UI_MainPos_far = "MainPos_far";

	public static readonly Vector3 Camera_Pad_Shift = new Vector3(-80f, 0f, 18f);

	private static float _current_LobbyCameraScale = -1f;

	private static string SceneAvatarCamSettingKey = "MainSceneAvartarCamPosCustomSettings";

	public static float specialRatio = 1.86f;

	public const float IPhoneXWidth = 2436f;

	public const float IPhoneXHeight = 1125f;

	public static bool CheckTingErrorNeedUpdateLog = false;

	public const string EDITOR_VERSION = "21.0.0";

	public static EnterGameVO enterGameVO = null;

	public static int LaunchTime = -1;

	public static bool IsNewUserAgreementOpen = false;

	public static int NewUserAgreementTime = 0;

	public static string NewUserAgreementText = "";

	public const int HarmonyChannelID = 39000001;

	public const int PCChannelID = 10024293;

	public const int IOSOnMacChannelID = 99999999;

	private static Rect m_iosSafeAreaRect = Rect.zero;

	private static PostProcessVolume postProcessVolume;

	private static PostProcessLayer postProcessLayer;

	private static Dictionary<string, string> m_languageCache = new Dictionary<string, string>();

	public static bool inShare = false;

	private const string DEFAULT_TABLE_RESOURCE_VERSION = "v2";

	private static bool s_isShowingMatchingLoading;

	private static List<int> m_9destNumPos = new List<int> { 2, 7, 1, 6, 8, 5, 0, 3, 4 };

	private static List<int> m_10destNumPos = new List<int> { 2, 7, 1, 9, 6, 8, 5, 0, 3, 4 };

	private static List<int> m_11destNumPos = new List<int>
	{
		2, 7, 1, 9, 6, 10, 8, 5, 0, 3,
		4
	};

	public const uint EM_AI_SEQUENCE_RANGE_FOR_HLMJ_START = 1466242167u;

	public const uint EM_AI_SEQUENCE_RANGE_FOR_HLMJ_END = 1466642163u;

	public const uint AI_SEQUENCE_STEP = 4u;

	public const string GM_STRING_APPLET_GUIDE_TO_DEV = "GM_AppletGuide_Dev";

	[ThreadStatic]
	private static StringBuilder s_pooledSB;

	[ThreadStatic]
	private static bool s_pooledSBInUse;

	private const int LOG_SB_INITIAL_CAPACITY = 256;

	private const int LOG_SB_MAX_RETAIN_CAPACITY = 4096;

	private static string[] MJNames = new string[34]
	{
		"一万", "二万", "三万", "四万", "五万", "六万", "七万", "八万", "九万", "一条",
		"二条", "三条", "四条", "五条", "六条", "七条", "八条", "九条", "一筒", "二筒",
		"三筒", "四筒", "五筒", "六筒", "七筒", "八筒", "九筒", "东风", "南风", "西风",
		"北风", "红中", "发财", "白板"
	};

	public static CMsg msg
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(2241))
			{
				return WrappersManagerImpl.GetPatch(2241).__Gen_Wrap_499();
			}
			return HappyMahjong.Core.Common.Util.msg;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(2243))
			{
				WrappersManagerImpl.GetPatch(2243).__Gen_Wrap_0(value);
			}
			else
			{
				HappyMahjong.Core.Common.Util.msg = value;
			}
		}
	}

	public static CUnknowMsgPara unKnowMsgParam
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(2242))
			{
				return WrappersManagerImpl.GetPatch(2242).__Gen_Wrap_500();
			}
			return HappyMahjong.Core.Common.Util.unKnowMsgParam;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(2244))
			{
				WrappersManagerImpl.GetPatch(2244).__Gen_Wrap_0(value);
			}
			else
			{
				HappyMahjong.Core.Common.Util.unKnowMsgParam = value;
			}
		}
	}

	public static Action<object, short> OnProtocolSend
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(61870))
			{
				return WrappersManagerImpl.GetPatch(61870).__Gen_Wrap_4163();
			}
			return HappyMahjong.Core.Common.Util.OnProtocolSend;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(20002))
			{
				WrappersManagerImpl.GetPatch(20002).__Gen_Wrap_0(value);
			}
			else
			{
				HappyMahjong.Core.Common.Util.OnProtocolSend = value;
			}
		}
	}

	public static bool IsFirstLauch
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(36618))
			{
				return WrappersManagerImpl.GetPatch(36618).__Gen_Wrap_11();
			}
			return HappyMahjong.Core.Common.Util.IsFirstLaunch;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(61871))
			{
				WrappersManagerImpl.GetPatch(61871).__Gen_Wrap_192(value);
			}
			else
			{
				HappyMahjong.Core.Common.Util.IsFirstLaunch = value;
			}
		}
	}

	public static int IphoneXMove
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(1406))
			{
				return WrappersManagerImpl.GetPatch(1406).__Gen_Wrap_101();
			}
			if (IsAndroidPlatform())
			{
				if (IsFoldableScreen() && IsPad())
				{
					if (SystemInfo.deviceModel.IndexOf("HUAWEI", StringComparison.OrdinalIgnoreCase) < 0)
					{
						return 0;
					}
					int notchHeight = GetNotchHeight();
					if (notchHeight == 0)
					{
						return 0;
					}
					if (notchHeight >= 96)
					{
						return 67;
					}
					return notchHeight;
				}
				int notchHeight2 = GetNotchHeight();
				if (notchHeight2 != 0 && notchHeight2 < 96)
				{
					return notchHeight2;
				}
			}
			if (Device.GetInstance().HasDynamicIsland())
			{
				return 96;
			}
			if (IsIphoneX())
			{
				return 67;
			}
			return 0;
		}
	}

	private static string GetBasicKefuUrl(KefuEnterScene kefuEnterScene)
	{
		if (WrappersManagerImpl.IsPatched(21842))
		{
			return WrappersManagerImpl.GetPatch(21842).__Gen_Wrap_1202(kefuEnterScene);
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (NetworkConfig.isAndroidOpenHttps || NetworkConfig.isIosOpenHttps || NetworkConfig.isPCOpenHttps)
		{
			stringBuilder.Append("https://");
		}
		else
		{
			stringBuilder.Append("http://");
		}
		switch (kefuEnterScene)
		{
		case KefuEnterScene.Login:
			stringBuilder.Append(LoginKefuUrl);
			break;
		case KefuEnterScene.Recharge:
			stringBuilder.Append(RechargeKefuUrl);
			break;
		case KefuEnterScene.Setting:
			stringBuilder.Append(SettingKefuUrl);
			break;
		case KefuEnterScene.Activity:
			stringBuilder.Append(ActivityKefuUrl);
			break;
		default:
			return null;
		}
		return stringBuilder.ToString();
	}

	public static string GetKefuURL(KefuEnterScene kefuEnterScene)
	{
		if (WrappersManagerImpl.IsPatched(21841))
		{
			return WrappersManagerImpl.GetPatch(21841).__Gen_Wrap_1202(kefuEnterScene);
		}
		string basicKefuUrl = GetBasicKefuUrl(kefuEnterScene);
		if (basicKefuUrl == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(basicKefuUrl);
		stringBuilder.Append("&qi=");
		stringBuilder.Append(IsAppInstalled("QQ") ? "1" : "0");
		stringBuilder.Append("&wi=");
		stringBuilder.Append(IsAppInstalled("WeChat") ? "1" : "0");
		stringBuilder.Append("&platid=");
		stringBuilder.Append("0");
		PlayerDataVO selfInfo = PlayerDataMgr.GetInstance().GetSelfInfo();
		stringBuilder.Append("&role=");
		stringBuilder.Append(WWW.EscapeURL(selfInfo.nickName));
		stringBuilder.Append("&roleid=");
		if (selfInfo.playerUin != 0)
		{
			stringBuilder.Append(selfInfo.playerUin.ToString());
		}
		string openId = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetOpenId();
		stringBuilder.Append("&openid=");
		stringBuilder.Append(openId);
		int loginType = (int)SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType();
		stringBuilder.Append("&channelid=");
		if (loginType != 0)
		{
			stringBuilder.Append(loginType);
		}
		stringBuilder.Append("&z=&zn=");
		Log.Info("URL: " + stringBuilder.ToString(), "Kefu");
		return stringBuilder.ToString();
	}

	public static void OpenKefu(KefuEnterScene kefuEnterScene)
	{
		if (WrappersManagerImpl.IsPatched(21840))
		{
			WrappersManagerImpl.GetPatch(21840).__Gen_Wrap_1203(kefuEnterScene);
			return;
		}
		string kefuURL = GetKefuURL(kefuEnterScene);
		if (IsPCPlatform())
		{
			string basicKefuUrl = GetBasicKefuUrl(kefuEnterScene);
			if (!string.IsNullOrEmpty(basicKefuUrl))
			{
				PCWebViewHandler.InstanceHandler()?.LoadURL(basicKefuUrl);
			}
		}
		else if (kefuURL != string.Empty)
		{
			MSDKWebView.OpenUrl(kefuURL);
		}
	}

	public static bool IsDisplayHighEndFX()
	{
		if (WrappersManagerImpl.IsPatched(61775))
		{
			return WrappersManagerImpl.GetPatch(61775).__Gen_Wrap_11();
		}
		switch (GetUserGraphicSettingLevel())
		{
		case 3:
			if (Device.GetInstance().OrigType >= DeviceType.High)
			{
				return true;
			}
			break;
		case 2:
			if (Device.GetInstance().OrigType == DeviceType.SuperHigh)
			{
				return true;
			}
			break;
		case 1:
			return false;
		}
		return false;
	}

	public static bool IsDisplayLowEndFX()
	{
		if (WrappersManagerImpl.IsPatched(61776))
		{
			return WrappersManagerImpl.GetPatch(61776).__Gen_Wrap_11();
		}
		switch (GetUserGraphicSettingLevel())
		{
		case 3:
			if (Device.GetInstance().OrigType <= DeviceType.Normal)
			{
				return true;
			}
			break;
		case 2:
			if (Device.GetInstance().OrigType <= DeviceType.High)
			{
				return true;
			}
			break;
		case 1:
			if (Device.GetInstance().OrigType <= DeviceType.SuperHigh)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public static void ReportBalanceDevicePerformance()
	{
		if (WrappersManagerImpl.IsPatched(61777))
		{
			WrappersManagerImpl.GetPatch(61777).__Gen_Wrap_46();
		}
		else if (EventReportUtil.IsNeedReportGameBalancePerformanceData)
		{
			EventReportUtil.IsNeedReportGameBalancePerformanceData = false;
			EventReportUtil.ReportDevicePerformance("GameBalance", AppRoot.GetInstance().LagTimes, AppRoot.GetInstance().BigLagTimes);
		}
	}

	public static void ReportGameSceneDevicePerformance()
	{
		if (WrappersManagerImpl.IsPatched(61778))
		{
			WrappersManagerImpl.GetPatch(61778).__Gen_Wrap_46();
		}
		else if (EventReportUtil.IsNeedReportGameScenePerformanceData)
		{
			EventReportUtil.IsNeedReportGameScenePerformanceData = false;
			EventReportUtil.ReportDevicePerformance("GameScene", AppRoot.GetInstance().LagTimes, AppRoot.GetInstance().BigLagTimes);
		}
	}

	public static void ReportMainSceneDevicePerformance()
	{
		if (WrappersManagerImpl.IsPatched(41489))
		{
			WrappersManagerImpl.GetPatch(41489).__Gen_Wrap_46();
		}
		else if (EventReportUtil.IsNeedReportMainScenePerformanceData)
		{
			EventReportUtil.ReportDevicePerformance("MainScene", AppRoot.GetInstance().LagTimes, AppRoot.GetInstance().BigLagTimes);
			EventReportUtil.IsNeedReportMainScenePerformanceData = false;
		}
	}

	public static bool IsNeedDisplaySkyboxBackground()
	{
		if (WrappersManagerImpl.IsPatched(61779))
		{
			return WrappersManagerImpl.GetPatch(61779).__Gen_Wrap_11();
		}
		switch (GetUserGraphicSettingLevel())
		{
		case 3:
			return false;
		case 2:
			if (Device.GetInstance().OrigType <= DeviceType.LowNormal)
			{
				return true;
			}
			break;
		case 1:
			if (Device.GetInstance().OrigType <= DeviceType.Normal)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public static int GetTotalMemory()
	{
		if (WrappersManagerImpl.IsPatched(1352))
		{
			return WrappersManagerImpl.GetPatch(1352).__Gen_Wrap_101();
		}
		return HappyMahjong.Core.Common.Util.GetTotalMemory();
	}

	public static int GetCPUCores()
	{
		if (WrappersManagerImpl.IsPatched(33406))
		{
			return WrappersManagerImpl.GetPatch(33406).__Gen_Wrap_101();
		}
		return SystemInfo.processorCount;
	}

	public static int GetMemoryScore()
	{
		if (WrappersManagerImpl.IsPatched(61780))
		{
			return WrappersManagerImpl.GetPatch(61780).__Gen_Wrap_101();
		}
		return HappyMahjong.Core.Common.Util.GetMemoryScore();
	}

	public static double GetCPUMutiplyFactor()
	{
		if (WrappersManagerImpl.IsPatched(61781))
		{
			return WrappersManagerImpl.GetPatch(61781).__Gen_Wrap_893();
		}
		return HappyMahjong.Core.Common.Util.GetCPUMutiplyFactor();
	}

	public static string GetDeviceModel()
	{
		if (WrappersManagerImpl.IsPatched(33398))
		{
			return WrappersManagerImpl.GetPatch(33398).__Gen_Wrap_51();
		}
		return SystemInfo.deviceModel;
	}

	public static int GetFrequency()
	{
		if (WrappersManagerImpl.IsPatched(61285))
		{
			return WrappersManagerImpl.GetPatch(61285).__Gen_Wrap_101();
		}
		int num = SystemInfo.processorFrequency;
		if (num > SingletonBase<DynamicConfig>.GetInstance().GetInt("CPUFrequencyBenchmark", "upLimit", 4000))
		{
			num /= 2;
		}
		Log.Info("process frequency = " + SystemInfo.processorFrequency, "Framework");
		return num;
	}

	public static int GetFrequencyScore()
	{
		if (WrappersManagerImpl.IsPatched(61782))
		{
			return WrappersManagerImpl.GetPatch(61782).__Gen_Wrap_101();
		}
		return HappyMahjong.Core.Common.Util.GetFrequencyScore();
	}

	public static double GetResolutionMultiplyFactor(int level)
	{
		if (WrappersManagerImpl.IsPatched(61783))
		{
			return WrappersManagerImpl.GetPatch(61783).__Gen_Wrap_4152(level);
		}
		return HappyMahjong.Core.Common.Util.GetResolutionMultiplyFactor(level);
	}

	public static int GetResolutionLevel()
	{
		if (WrappersManagerImpl.IsPatched(61784))
		{
			return WrappersManagerImpl.GetPatch(61784).__Gen_Wrap_101();
		}
		Vector2 screenResolution = GetScreenResolution();
		float num = screenResolution.x / screenResolution.y;
		Log.Info("Framework", "GetResolutionLevel {0} {1}", screenResolution.x, screenResolution.y);
		if ((screenResolution.y >= 1080f && (double)num >= 2.1) || screenResolution.y >= 2000f)
		{
			return 2;
		}
		if (screenResolution.y >= 1080f && (double)num > 1.7 && (double)num < 2.1)
		{
			return 1;
		}
		return 0;
	}

	public static string GetGPUName()
	{
		if (WrappersManagerImpl.IsPatched(61286))
		{
			return WrappersManagerImpl.GetPatch(61286).__Gen_Wrap_51();
		}
		return SystemInfo.graphicsDeviceName;
	}

	public static int GetGPUScore(string GPUDeviceName = "")
	{
		if (WrappersManagerImpl.IsPatched(61786))
		{
			return WrappersManagerImpl.GetPatch(61786).__Gen_Wrap_85(GPUDeviceName);
		}
		return HappyMahjong.Core.Common.Util.GetGPUScore(GPUDeviceName);
	}

	public static int GetBenchmarkRawScore()
	{
		if (WrappersManagerImpl.IsPatched(36453))
		{
			return WrappersManagerImpl.GetPatch(36453).__Gen_Wrap_101();
		}
		return HappyMahjong.Core.Common.Util.GetBenchmarkRawScore();
	}

	public static int GetRankLevel()
	{
		if (WrappersManagerImpl.IsPatched(61284))
		{
			return WrappersManagerImpl.GetPatch(61284).__Gen_Wrap_101();
		}
		return HappyMahjong.Core.Common.Util.GetRankLevel();
	}

	public static Vector2 GetScreenResolution()
	{
		if (WrappersManagerImpl.IsPatched(61785))
		{
			return WrappersManagerImpl.GetPatch(61785).__Gen_Wrap_316();
		}
		return new Vector2(AppRoot.width, AppRoot.height);
	}

	public static string GetQIMEI()
	{
		if (WrappersManagerImpl.IsPatched(34102))
		{
			return WrappersManagerImpl.GetPatch(34102).__Gen_Wrap_51();
		}
		string text = "";
		try
		{
			text = PlatformUtil.GetQIMEIiOSImpl("i1101070761");
			Log.Info("Get QIMEI from native: " + text, "Framework");
		}
		catch (Exception ex)
		{
			Log.Error("Get QIMEI from native failed: " + ex.Message, "Framework");
			text = "";
		}
		return text;
	}

	public static uint BKDRHash(string str)
	{
		if (WrappersManagerImpl.IsPatched(61768))
		{
			return WrappersManagerImpl.GetPatch(61768).__Gen_Wrap_98(str);
		}
		uint num = 131u;
		uint num2 = 0u;
		uint num3 = 0u;
		for (num3 = 0u; num3 < str.Length; num3++)
		{
			num2 = num2 * num + (byte)str[(int)num3];
		}
		return num2;
	}

	public static uint GetBucketID(string info)
	{
		if (WrappersManagerImpl.IsPatched(61767))
		{
			return WrappersManagerImpl.GetPatch(61767).__Gen_Wrap_98(info);
		}
		return BKDRHash(info) % 100;
	}

	public unsafe static void ToLower(string str)
	{
		if (WrappersManagerImpl.IsPatched(61787))
		{
			WrappersManagerImpl.GetPatch(61787).__Gen_Wrap_0(str);
			return;
		}
		fixed (char* ptr = str)
		{
			int length = str.Length;
			for (int i = 0; i < length; i++)
			{
				ptr[i] = char.ToLower(ptr[i]);
			}
		}
	}

	public static bool GetQualityConfig(string sceneType, string controlType, int deviceType, int qualityType, out int value)
	{
		if (WrappersManagerImpl.IsPatched(1357))
		{
			return WrappersManagerImpl.GetPatch(1357).__Gen_Wrap_304(sceneType, controlType, deviceType, qualityType, out value);
		}
		List<QualityConfig> allCachedConfig = ProtoConfigLoader<QualityConfig>.getInstance().getAllCachedConfig();
		value = -1;
		if (allCachedConfig == null)
		{
			return false;
		}
		bool result = false;
		foreach (QualityConfig item in allCachedConfig)
		{
			if (item.scene.Equals(sceneType) && item.controlType.Equals(controlType) && item.deviceType == deviceType)
			{
				switch (qualityType)
				{
				case 4:
					result = int.TryParse(item.Super, out value);
					break;
				case 3:
					result = int.TryParse(item.High, out value);
					break;
				case 2:
					result = int.TryParse(item.Mid, out value);
					break;
				case 1:
					result = int.TryParse(item.Low, out value);
					break;
				case 0:
					result = int.TryParse(item.Effecient, out value);
					break;
				}
			}
		}
		return result;
	}

	public static List<FrameRateConfigPerDevice> GetFrameRateConfigsPerDevice()
	{
		if (WrappersManagerImpl.IsPatched(33395))
		{
			return WrappersManagerImpl.GetPatch(33395).__Gen_Wrap_2083();
		}
		List<object> list = (List<object>)SingletonBase<DynamicConfig>.GetInstance().GetObject("FrameRateConfig", "PerDevice");
		if (list == null)
		{
			return null;
		}
		List<object> list2 = list;
		List<FrameRateConfigPerDevice> list3 = new List<FrameRateConfigPerDevice>();
		for (int i = 0; i < list2.Count; i++)
		{
			FrameRateConfigPerDevice frameRateConfigPerDevice = new FrameRateConfigPerDevice();
			Dictionary<string, object> dictionary = list2[i] as Dictionary<string, object>;
			frameRateConfigPerDevice.deviceModel = (string)dictionary["DeviceModel"];
			frameRateConfigPerDevice.scene = (string)dictionary["Scene"];
			frameRateConfigPerDevice.lowFrameRateLevel = Convert.ToSingle(dictionary["LowFrameRateLevel"]);
			frameRateConfigPerDevice.highFrameRateLeve = Convert.ToSingle(dictionary["HighFrameRateLevel"]);
			list3.Add(frameRateConfigPerDevice);
		}
		return list3;
	}

	public static List<FrameRateConfigGeneral> GetFrameRateConfigsGeneral()
	{
		if (WrappersManagerImpl.IsPatched(33396))
		{
			return WrappersManagerImpl.GetPatch(33396).__Gen_Wrap_2084();
		}
		List<object> list = (List<object>)SingletonBase<DynamicConfig>.GetInstance().GetObject("FrameRateConfig", "General");
		if (list == null)
		{
			return null;
		}
		List<object> list2 = list;
		List<FrameRateConfigGeneral> list3 = new List<FrameRateConfigGeneral>();
		for (int i = 0; i < list2.Count; i++)
		{
			FrameRateConfigGeneral frameRateConfigGeneral = new FrameRateConfigGeneral();
			Dictionary<string, object> dictionary = list2[i] as Dictionary<string, object>;
			frameRateConfigGeneral.deviceScoreLow = Convert.ToSingle(dictionary["DeviceScoreLow"]);
			frameRateConfigGeneral.deviceScoreHigh = Convert.ToSingle(dictionary["DeviceScoreHigh"]);
			frameRateConfigGeneral.platform = (string)dictionary["Platform"];
			frameRateConfigGeneral.displayRefrestRate = Convert.ToInt32(dictionary["DisplayRefrestRate"]);
			frameRateConfigGeneral.scene = (string)dictionary["Scene"];
			frameRateConfigGeneral.lowFrameRateLevel = Convert.ToSingle(dictionary["LowFrameRateLevel"]);
			frameRateConfigGeneral.highFrameRateLeve = Convert.ToSingle(dictionary["HighFrameRateLevel"]);
			list3.Add(frameRateConfigGeneral);
		}
		return list3;
	}

	public static bool GetAvatarOnTableQualityConfig(string controlType, int deviceType, int qualityType, string clothType, out int value)
	{
		if (WrappersManagerImpl.IsPatched(61788))
		{
			return WrappersManagerImpl.GetPatch(61788).__Gen_Wrap_4153(controlType, deviceType, qualityType, clothType, out value);
		}
		value = -1;
		ProtoConfigLoader<QualityConfigCfg> instance = ProtoConfigLoader<QualityConfigCfg>.getInstance();
		if (!instance.containsChild("Sheet2"))
		{
			Log.Info("GetAvatarOnTableQualityConfig: Sheet2 not found, fallback to Sheet1", "Setting");
			return GetQualityConfig("avatarOnTable", controlType, deviceType, qualityType, out value);
		}
		List<QualityConfig> allCachedConfig = instance.getChild<QualityConfig>("Sheet2").getAllCachedConfig();
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			Log.Info("GetAvatarOnTableQualityConfig: Sheet2 is empty, fallback to Sheet1", "Setting");
			return GetQualityConfig("avatarOnTable", controlType, deviceType, qualityType, out value);
		}
		int num = CalcClothTypeScore(clothType);
		QualityConfig qualityConfig = null;
		QualityConfig qualityConfig2 = null;
		int num2 = int.MaxValue;
		int num3 = int.MaxValue;
		foreach (QualityConfig item in allCachedConfig)
		{
			if (!item.scene.Equals("avatarOnTable") || !item.controlType.Equals(controlType) || item.deviceType != deviceType)
			{
				continue;
			}
			if (!string.IsNullOrEmpty(item.ClothType) && item.ClothType.Length == 3)
			{
				int num4 = CalcClothTypeScore(item.ClothType);
				int num5 = Math.Abs(num4 - num);
				if (num5 < num2 || (num5 == num2 && num4 < num3))
				{
					qualityConfig = item;
					num2 = num5;
					num3 = num4;
				}
			}
			else if (qualityConfig2 == null)
			{
				qualityConfig2 = item;
			}
		}
		QualityConfig qualityConfig3 = qualityConfig ?? qualityConfig2;
		if (qualityConfig3 == null)
		{
			return false;
		}
		return TryParseQualityValue(qualityConfig3, qualityType, out value);
	}

	private static int CalcClothTypeScore(string clothType)
	{
		if (WrappersManagerImpl.IsPatched(34362))
		{
			return WrappersManagerImpl.GetPatch(34362).__Gen_Wrap_85(clothType);
		}
		if (string.IsNullOrEmpty(clothType))
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < clothType.Length; i++)
		{
			int num2 = clothType[i] - 48;
			if (num2 >= 0 && num2 < s_clothLevelWeights.Length)
			{
				num += s_clothLevelWeights[num2];
			}
		}
		return num;
	}

	internal static bool TryParseQualityValue(QualityConfig config, int qualityType, out int value)
	{
		if (WrappersManagerImpl.IsPatched(34363))
		{
			return WrappersManagerImpl.GetPatch(34363).__Gen_Wrap_2211(config, qualityType, out value);
		}
		value = -1;
		string s;
		switch ((QualityType)qualityType)
		{
		case QualityType.Super:
			s = config.Super;
			break;
		case QualityType.High:
			s = config.High;
			break;
		case QualityType.Mid:
			s = config.Mid;
			break;
		case QualityType.Low:
			s = config.Low;
			break;
		case QualityType.Effecient:
			s = config.Effecient;
			break;
		default:
			return false;
		}
		return int.TryParse(s, out value);
	}

	public static Dictionary<string, List<QualityConfig>> GetAllAvatarOnTableQualityConfigs(int deviceType)
	{
		if (WrappersManagerImpl.IsPatched(34359))
		{
			return WrappersManagerImpl.GetPatch(34359).__Gen_Wrap_2210(deviceType);
		}
		ProtoConfigLoader<QualityConfigCfg> instance = ProtoConfigLoader<QualityConfigCfg>.getInstance();
		if (!instance.containsChild("Sheet2"))
		{
			Log.Info("GetAllAvatarOnTableQualityConfigs: Sheet2 not found", "Setting");
			return null;
		}
		List<QualityConfig> allCachedConfig = instance.getChild<QualityConfig>("Sheet2").getAllCachedConfig();
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			Log.Info("GetAllAvatarOnTableQualityConfigs: Sheet2 is empty", "Setting");
			return null;
		}
		Dictionary<string, List<QualityConfig>> dictionary = new Dictionary<string, List<QualityConfig>>();
		foreach (QualityConfig item in allCachedConfig)
		{
			if (item.scene.Equals("avatarOnTable") && item.deviceType == deviceType)
			{
				if (!dictionary.TryGetValue(item.controlType, out var value))
				{
					value = new List<QualityConfig>();
					dictionary[item.controlType] = value;
				}
				value.Add(item);
			}
		}
		Log.Info("GetAllAvatarOnTableQualityConfigs: cached " + dictionary.Count + " controlTypes for device=" + deviceType, "Setting");
		return dictionary;
	}

	public static bool ResolveConfigByClothType(List<QualityConfig> candidates, string clothType, int qualityType, out int value)
	{
		if (WrappersManagerImpl.IsPatched(34361))
		{
			return WrappersManagerImpl.GetPatch(34361).__Gen_Wrap_2212(candidates, clothType, qualityType, out value);
		}
		value = -1;
		if (candidates == null || candidates.Count == 0)
		{
			return false;
		}
		int num = CalcClothTypeScore(clothType);
		QualityConfig qualityConfig = null;
		QualityConfig qualityConfig2 = null;
		int num2 = int.MaxValue;
		int num3 = int.MaxValue;
		foreach (QualityConfig candidate in candidates)
		{
			if (!string.IsNullOrEmpty(candidate.ClothType) && candidate.ClothType.Length == 3)
			{
				int num4 = CalcClothTypeScore(candidate.ClothType);
				int num5 = Math.Abs(num4 - num);
				if (num5 < num2 || (num5 == num2 && num4 < num3))
				{
					qualityConfig = candidate;
					num2 = num5;
					num3 = num4;
				}
			}
			else if (qualityConfig2 == null)
			{
				qualityConfig2 = candidate;
			}
		}
		QualityConfig qualityConfig3 = qualityConfig ?? qualityConfig2;
		if (qualityConfig3 == null)
		{
			Log.Info("ResolveConfigByClothType: no match found", "Setting");
			return false;
		}
		Log.Info(string.Format("ResolveConfigByClothType: matched ClothType={0}, bestDiff={1}", qualityConfig3.ClothType ?? "(fallback)", num2), "Setting");
		return TryParseQualityValue(qualityConfig3, qualityType, out value);
	}

	public static int AvatarLevelNameToIndex(string levelName, int defaultLevel = 0)
	{
		if (WrappersManagerImpl.IsPatched(61296))
		{
			return WrappersManagerImpl.GetPatch(61296).__Gen_Wrap_74(levelName, defaultLevel);
		}
		if (string.IsNullOrEmpty(levelName))
		{
			return defaultLevel;
		}
		for (int i = 0; i < AvatarLevelNames.Length; i++)
		{
			if (string.Equals(levelName, AvatarLevelNames[i], StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return defaultLevel;
	}

	public static bool IsAvatarLevelTargetItem(ItemConfig cfg)
	{
		if (WrappersManagerImpl.IsPatched(61294))
		{
			return WrappersManagerImpl.GetPatch(61294).__Gen_Wrap_3(cfg);
		}
		if (cfg == null || cfg.type == null || cfg.type.Count == 0)
		{
			return false;
		}
		if (!cfg.type.Contains(0) && !cfg.type.Contains(1) && !cfg.type.Contains(2))
		{
			return cfg.type.Contains(3);
		}
		return true;
	}

	public static bool IsAvatarLevelTargetGender(ItemConfig cfg, UserGender avatarGender)
	{
		if (WrappersManagerImpl.IsPatched(61295))
		{
			return WrappersManagerImpl.GetPatch(61295).__Gen_Wrap_3440(cfg, avatarGender);
		}
		if (cfg == null)
		{
			return false;
		}
		if (avatarGender == UserGender.UnKnown || avatarGender == UserGender.NotRecvServer)
		{
			return true;
		}
		return CheckGender(cfg, avatarGender);
	}

	public static int DetectAvatarLevelFromItemList(List<int> itemList, UserGender avatarGender, int fallback = 0)
	{
		if (WrappersManagerImpl.IsPatched(61293))
		{
			return WrappersManagerImpl.GetPatch(61293).__Gen_Wrap_4129(itemList, avatarGender, fallback);
		}
		if (itemList == null || itemList.Count == 0)
		{
			return fallback;
		}
		int num = fallback;
		foreach (int item in itemList)
		{
			ItemConfig configByKey = ProtoConfigLoader<ItemConfig>.getInstance().getConfigByKey(item);
			if (IsAvatarLevelTargetItem(configByKey) && IsAvatarLevelTargetGender(configByKey, avatarGender) && !string.IsNullOrEmpty(configByKey.level))
			{
				int num2 = AvatarLevelNameToIndex(configByKey.level, fallback);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public static bool CheckIsTimeMore(long time, int maxWeekCardDays)
	{
		if (WrappersManagerImpl.IsPatched(49706))
		{
			return WrappersManagerImpl.GetPatch(49706).__Gen_Wrap_3392(time, maxWeekCardDays);
		}
		_ = DateTime.Now;
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		long num = (long)(DateTime.Now - dateTime).TotalSeconds;
		if (time - num >= (long)maxWeekCardDays * 86400L)
		{
			return true;
		}
		return false;
	}

	public static DateTime ConverIntToDateTime(uint time)
	{
		if (WrappersManagerImpl.IsPatched(21177))
		{
			return WrappersManagerImpl.GetPatch(21177).__Gen_Wrap_1109(time);
		}
		return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(time).ToLocalTime();
	}

	public static long ConvertDateTimeToInt(DateTime time)
	{
		if (WrappersManagerImpl.IsPatched(61789))
		{
			return WrappersManagerImpl.GetPatch(61789).__Gen_Wrap_3050(time);
		}
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
		return (time.Ticks - dateTime.Ticks) / 10000;
	}

	public static long GetUnixTimestamp(DateTime time)
	{
		if (WrappersManagerImpl.IsPatched(44671))
		{
			return WrappersManagerImpl.GetPatch(44671).__Gen_Wrap_3050(time);
		}
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
		return (long)(time - dateTime).TotalSeconds;
	}

	public static string GetSecretGameOverTimeShow(DateTime currentTime)
	{
		if (WrappersManagerImpl.IsPatched(52971))
		{
			return WrappersManagerImpl.GetPatch(52971).__Gen_Wrap_2732(currentTime);
		}
		return string.Concat(string.Concat(string.Concat(currentTime.Year + "/" + currentTime.Month + "/" + currentTime.Day, " "), currentTime.Hour.ToString(), ":", (currentTime.Minute >= 0 && currentTime.Minute <= 9) ? ("0" + currentTime.Minute) : currentTime.Minute.ToString()), " ");
	}

	public static bool HasTimePass(long time)
	{
		if (WrappersManagerImpl.IsPatched(49733))
		{
			return WrappersManagerImpl.GetPatch(49733).__Gen_Wrap_2311(time);
		}
		DateTime now = DateTime.Now;
		DateTime t = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(time);
		if (DateTime.Compare(now, t) > 0)
		{
			return true;
		}
		return false;
	}

	[IDTag(0)]
	public static bool IsInTimeSpan(string beginTimeStr, string endTimeStr)
	{
		if (WrappersManagerImpl.IsPatched(895))
		{
			return WrappersManagerImpl.GetPatch(895).__Gen_Wrap_15(beginTimeStr, endTimeStr);
		}
		if (string.IsNullOrEmpty(beginTimeStr) || string.IsNullOrEmpty(endTimeStr))
		{
			return false;
		}
		int serverTime = SingletonBase<ServerTime>.GetInstance().GetServerTime();
		if (!DateTime.TryParse(beginTimeStr, out var result))
		{
			Log.Error("IsInTimeSpan parse beginTimeStr failed, beginTimeStr=" + beginTimeStr, "Framework");
			return false;
		}
		if (!DateTime.TryParse(endTimeStr, out var result2))
		{
			Log.Error("IsInTimeSpan parse endTimeStr failed, endTimeStr=" + endTimeStr, "Framework");
			return false;
		}
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		long num = (long)(result - dateTime).TotalSeconds;
		long num2 = (long)(result2 - dateTime).TotalSeconds;
		if (serverTime > num && serverTime < num2)
		{
			return true;
		}
		return false;
	}

	[IDTag(2)]
	public static bool IsInTimeSpan(DateTime beginTime, DateTime endTime)
	{
		if (WrappersManagerImpl.IsPatched(44670))
		{
			return WrappersManagerImpl.GetPatch(44670).__Gen_Wrap_2855(beginTime, endTime);
		}
		int serverTime = SingletonBase<ServerTime>.GetInstance().GetServerTime();
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		long num = (long)(beginTime - dateTime).TotalSeconds;
		long num2 = (long)(endTime - dateTime).TotalSeconds;
		if (serverTime > num && serverTime < num2)
		{
			return true;
		}
		return false;
	}

	[IDTag(1)]
	public static bool IsInTimeSpan(uint beginTime, uint endTime)
	{
		if (WrappersManagerImpl.IsPatched(21176))
		{
			return WrappersManagerImpl.GetPatch(21176).__Gen_Wrap_1110(beginTime, endTime);
		}
		if (beginTime == 0 && endTime == 0)
		{
			return false;
		}
		DateTime now = DateTime.Now;
		now = ConverIntToDateTime(beginTime);
		DateTime t = ConverIntToDateTime(endTime);
		if (DateTime.Compare(ConverIntToDateTime((uint)SingletonBase<ServerTime>.GetInstance().GetServerTime()), now) > 0 && DateTime.Compare(ConverIntToDateTime((uint)SingletonBase<ServerTime>.GetInstance().GetServerTime()), t) < 0)
		{
			return true;
		}
		return false;
	}

	public static bool IsExceedTimeSpan(string beginTimeStr)
	{
		if (WrappersManagerImpl.IsPatched(3947))
		{
			return WrappersManagerImpl.GetPatch(3947).__Gen_Wrap_3(beginTimeStr);
		}
		if (string.IsNullOrEmpty(beginTimeStr))
		{
			return false;
		}
		if (!DateTime.TryParse(beginTimeStr, out var result))
		{
			return false;
		}
		int serverTime = SingletonBase<ServerTime>.GetInstance().GetServerTime();
		DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
		long num = (long)(result - dateTime).TotalSeconds;
		if (serverTime >= num)
		{
			return true;
		}
		return false;
	}

	public static string SecondsToHMSM(long secs)
	{
		if (WrappersManagerImpl.IsPatched(61790))
		{
			return WrappersManagerImpl.GetPatch(61790).__Gen_Wrap_1190(secs);
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds(secs);
		return $"{timeSpan.Hours:D2}hour:{timeSpan.Minutes:D2}minute:{timeSpan.Seconds:D2}seconds:{timeSpan.Milliseconds:D3}milliseconds";
	}

	public static string TimeToTimerStr(float time)
	{
		if (WrappersManagerImpl.IsPatched(61791))
		{
			return WrappersManagerImpl.GetPatch(61791).__Gen_Wrap_2501(time);
		}
		return HappyMahjong.Core.Common.Util.TimeToTimerStr(time);
	}

	public static int GetCreatetime(DateTime time)
	{
		if (WrappersManagerImpl.IsPatched(21179))
		{
			return WrappersManagerImpl.GetPatch(21179).__Gen_Wrap_1111(time);
		}
		return HappyMahjong.Core.Common.Util.GetCreatetime(time);
	}

	public static DateTime GetDateTime(uint totalSecond)
	{
		if (WrappersManagerImpl.IsPatched(21173))
		{
			return WrappersManagerImpl.GetPatch(21173).__Gen_Wrap_1109(totalSecond);
		}
		return HappyMahjong.Core.Common.Util.GetDateTime(totalSecond);
	}

	public static GameObject GetNodeByNameInChild(string name, GameObject obj)
	{
		if (WrappersManagerImpl.IsPatched(35475))
		{
			return WrappersManagerImpl.GetPatch(35475).__Gen_Wrap_73(name, obj);
		}
		return HappyMahjong.Core.Common.Util.GetNodeByNameInChild(name, obj);
	}

	public static void DestroyAndClearObjectInList(List<GameObject> list)
	{
		if (WrappersManagerImpl.IsPatched(61792))
		{
			WrappersManagerImpl.GetPatch(61792).__Gen_Wrap_0(list);
		}
		else
		{
			HappyMahjong.Core.Common.Util.DestroyAndClearObjectInList(list);
		}
	}

	public static void ClearAndDestroyAllChild(Transform parent)
	{
		if (WrappersManagerImpl.IsPatched(45517))
		{
			WrappersManagerImpl.GetPatch(45517).__Gen_Wrap_0(parent);
		}
		else
		{
			HappyMahjong.Core.Common.Util.ClearAndDestroyAllChild(parent);
		}
	}

	public static void ResetParent(Transform parent, Transform child)
	{
		if (WrappersManagerImpl.IsPatched(241))
		{
			WrappersManagerImpl.GetPatch(241).__Gen_Wrap_1(parent, child);
		}
		else
		{
			HappyMahjong.Core.Common.Util.ResetParent(parent, child);
		}
	}

	public static void ResetParentWithOutScales(Transform parent, Transform child)
	{
		if (WrappersManagerImpl.IsPatched(21023))
		{
			WrappersManagerImpl.GetPatch(21023).__Gen_Wrap_1(parent, child);
		}
		else
		{
			HappyMahjong.Core.Common.Util.ResetParentWithOutScales(parent, child);
		}
	}

	public static void ResetLayer(GameObject go, int layer, bool changeChild = true)
	{
		if (WrappersManagerImpl.IsPatched(242))
		{
			WrappersManagerImpl.GetPatch(242).__Gen_Wrap_84(go, layer, changeChild);
		}
		else
		{
			HappyMahjong.Core.Common.Util.ResetLayer(go, layer, changeChild);
		}
	}

	public static GameObject GetNodeByName(string name, GameObject obj)
	{
		if (WrappersManagerImpl.IsPatched(4157))
		{
			return WrappersManagerImpl.GetPatch(4157).__Gen_Wrap_73(name, obj);
		}
		return HappyMahjong.Core.Common.Util.GetNodeByName(name, obj);
	}

	public static void SetLayer(GameObject obj, int layer)
	{
		if (WrappersManagerImpl.IsPatched(479))
		{
			WrappersManagerImpl.GetPatch(479).__Gen_Wrap_10(obj, layer);
		}
		else
		{
			HappyMahjong.Core.Common.Util.SetLayer(obj, layer);
		}
	}

	public static void RemoveChildren(Transform trans)
	{
		if (WrappersManagerImpl.IsPatched(49684))
		{
			WrappersManagerImpl.GetPatch(49684).__Gen_Wrap_0(trans);
		}
		else
		{
			HappyMahjong.Core.Common.Util.RemoveChildren(trans);
		}
	}

	public static string GetFullName(GameObject go)
	{
		if (WrappersManagerImpl.IsPatched(21397))
		{
			return WrappersManagerImpl.GetPatch(21397).__Gen_Wrap_19(go);
		}
		return HappyMahjong.Core.Common.Util.GetFullName(go);
	}

	public static void SetAllChildState(Transform parent, bool state)
	{
		if (WrappersManagerImpl.IsPatched(61793))
		{
			WrappersManagerImpl.GetPatch(61793).__Gen_Wrap_8(parent, state);
		}
		else
		{
			HappyMahjong.Core.Common.Util.SetAllChildState(parent, state);
		}
	}

	public static void SetAllChildDeActive(Transform parent)
	{
		if (WrappersManagerImpl.IsPatched(39834))
		{
			WrappersManagerImpl.GetPatch(39834).__Gen_Wrap_0(parent);
		}
		else
		{
			HappyMahjong.Core.Common.Util.SetAllChildDeActive(parent);
		}
	}

	public static void SetAllChildActive(Transform parent)
	{
		if (WrappersManagerImpl.IsPatched(61794))
		{
			WrappersManagerImpl.GetPatch(61794).__Gen_Wrap_0(parent);
		}
		else
		{
			SetAllChildState(parent, state: true);
		}
	}

	public static void SetOnlyThisChildActive(Transform parent, string childName)
	{
		if (WrappersManagerImpl.IsPatched(21181))
		{
			WrappersManagerImpl.GetPatch(21181).__Gen_Wrap_1(parent, childName);
			return;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			if (parent.GetChild(i).name.Equals(childName))
			{
				parent.GetChild(i).gameObject.SetActive(value: true);
			}
			else
			{
				parent.GetChild(i).gameObject.SetActive(value: false);
			}
		}
	}

	public static void SetOnlyThisChildDeactive(Transform parent, string childName)
	{
		if (WrappersManagerImpl.IsPatched(21183))
		{
			WrappersManagerImpl.GetPatch(21183).__Gen_Wrap_1(parent, childName);
			return;
		}
		for (int i = 0; i < parent.childCount; i++)
		{
			if (parent.GetChild(i).name.Equals(childName))
			{
				parent.GetChild(i).gameObject.SetActive(value: false);
			}
			else
			{
				parent.GetChild(i).gameObject.SetActive(value: true);
			}
		}
	}

	public static void MakeDirty(GameObject go)
	{
		if (WrappersManagerImpl.IsPatched(51203))
		{
			WrappersManagerImpl.GetPatch(51203).__Gen_Wrap_0(go);
		}
		else if (go.activeSelf)
		{
			go.SetActive(value: false);
			go.SetActive(value: true);
		}
	}

	[IDTag(0)]
	public static GameObject LoadAndInstantiate(string path)
	{
		if (WrappersManagerImpl.IsPatched(44771))
		{
			return WrappersManagerImpl.GetPatch(44771).__Gen_Wrap_68(path);
		}
		return UnityEngine.Object.Instantiate(Resources.Load<GameObject>(path));
	}

	[IDTag(1)]
	public static GameObject LoadAndInstantiate(string path, Transform tfParent)
	{
		if (WrappersManagerImpl.IsPatched(61795))
		{
			return WrappersManagerImpl.GetPatch(61795).__Gen_Wrap_73(path, tfParent);
		}
		return UnityEngine.Object.Instantiate(Resources.Load<GameObject>(path), tfParent);
	}

	public static void ActivateOneOf(IEnumerable<GameObject> gos, int index)
	{
		if (WrappersManagerImpl.IsPatched(48022))
		{
			WrappersManagerImpl.GetPatch(48022).__Gen_Wrap_10(gos, index);
			return;
		}
		int num = 0;
		foreach (GameObject go in gos)
		{
			if (go != null)
			{
				go.SetActive(num == index);
			}
			num++;
		}
	}

	public static void BumpSortingLayer(Transform parent, int sortingOrder)
	{
		if (WrappersManagerImpl.IsPatched(21141))
		{
			WrappersManagerImpl.GetPatch(21141).__Gen_Wrap_10(parent, sortingOrder);
		}
		else
		{
			if (!(parent != null))
			{
				return;
			}
			List<MeshRenderer> value;
			using (CollectionPool<List<MeshRenderer>, MeshRenderer>.Get(out value))
			{
				parent.GetComponentsInChildren(includeInactive: true, value);
				foreach (MeshRenderer item in value)
				{
					item.sortingOrder += sortingOrder;
				}
				List<SortingGroup> value2;
				using (CollectionPool<List<SortingGroup>, SortingGroup>.Get(out value2))
				{
					parent.GetComponentsInChildren(includeInactive: true, value2);
					foreach (SortingGroup item2 in value2)
					{
						item2.sortingOrder += sortingOrder;
					}
					List<ParticleSystemRenderer> value3;
					using (CollectionPool<List<ParticleSystemRenderer>, ParticleSystemRenderer>.Get(out value3))
					{
						parent.GetComponentsInChildren(includeInactive: true, value3);
						foreach (ParticleSystemRenderer item3 in value3)
						{
							item3.sortingOrder += sortingOrder;
						}
						List<SpriteRenderer> value4;
						using (CollectionPool<List<SpriteRenderer>, SpriteRenderer>.Get(out value4))
						{
							parent.GetComponentsInChildren(includeInactive: true, value4);
							foreach (SpriteRenderer item4 in value4)
							{
								item4.sortingOrder += sortingOrder;
							}
							List<SkinnedMeshRenderer> value5;
							using (CollectionPool<List<SkinnedMeshRenderer>, SkinnedMeshRenderer>.Get(out value5))
							{
								parent.GetComponentsInChildren(includeInactive: true, value5);
								foreach (SkinnedMeshRenderer item5 in value5)
								{
									item5.sortingOrder += sortingOrder;
								}
							}
						}
					}
				}
			}
		}
	}

	[IDTag(1)]
	public static GameObject FindChild(GameObject go, string name)
	{
		if (WrappersManagerImpl.IsPatched(199))
		{
			return WrappersManagerImpl.GetPatch(199).__Gen_Wrap_73(go, name);
		}
		if (null == go)
		{
			return null;
		}
		Transform transform = go.transform;
		if (transform == null || string.IsNullOrEmpty(name))
		{
			Log.Error("FindChild: parent or name have a mistake", "Framework");
			return null;
		}
		Transform transform2 = transform.Find(name);
		if (transform2 == null)
		{
			foreach (Transform item in transform)
			{
				transform2 = FindChild(item, name);
				if (transform2 != null)
				{
					break;
				}
			}
		}
		if (transform2 != null)
		{
			return transform2.gameObject;
		}
		return null;
	}

	[IDTag(0)]
	public static Transform FindChild(Transform parent, string name)
	{
		if (WrappersManagerImpl.IsPatched(200))
		{
			return WrappersManagerImpl.GetPatch(200).__Gen_Wrap_72(parent, name);
		}
		if (parent == null || string.IsNullOrEmpty(name))
		{
			Log.Error("FindChild: parent or name have a mistake", "Framework");
			return null;
		}
		Transform transform = parent.Find(name);
		if (transform == null)
		{
			foreach (Transform item in parent)
			{
				transform = FindChild(item, name);
				if (transform != null)
				{
					break;
				}
			}
		}
		return transform;
	}

	public static void SetAnimatorSpeed(Animator animator, float speed)
	{
		if (WrappersManagerImpl.IsPatched(58996))
		{
			WrappersManagerImpl.GetPatch(58996).__Gen_Wrap_28(animator, speed);
		}
		else
		{
			animator.speed = speed;
		}
	}

	public static Transform GetSelectionViewTransform()
	{
		if (WrappersManagerImpl.IsPatched(21170))
		{
			return WrappersManagerImpl.GetPatch(21170).__Gen_Wrap_82();
		}
		GameObject gameObject = GameObject.Find("SelectionSceneRoot");
		if (gameObject != null)
		{
			return gameObject.transform.Find("UI Root");
		}
		return null;
	}

	public static string GetTransPath(Transform trans)
	{
		if (WrappersManagerImpl.IsPatched(61796))
		{
			return WrappersManagerImpl.GetPatch(61796).__Gen_Wrap_19(trans);
		}
		if (null == trans)
		{
			return string.Empty;
		}
		if (null == trans.parent)
		{
			return trans.name;
		}
		return GetTransPath(trans.parent) + "/" + trans.name;
	}

	public static void SetMainSceneActive(bool status)
	{
		if (WrappersManagerImpl.IsPatched(44519))
		{
			WrappersManagerImpl.GetPatch(44519).__Gen_Wrap_192(status);
			return;
		}
		try
		{
			if (scene360Obj != null)
			{
				scene360Obj.SetActive(status);
			}
			else
			{
				scene360Obj = GameObject.Find(SingletonBase<DynamicConfig>.GetInstance().GetString("MainSceneObj", "SceneBuilding", "360_SceneBuilding"));
				if (scene360Obj != null)
				{
					scene360Obj.SetActive(status);
				}
			}
			if (scene360FXObj != null)
			{
				scene360FXObj.SetActive(status);
			}
			else
			{
				GameObject gameObject = GameObject.Find("SceneFXLod");
				if (gameObject != null)
				{
					MainSceneEffectMediator component = gameObject.GetComponent<MainSceneEffectMediator>();
					if (component != null)
					{
						if (!status)
						{
							component.Reset();
						}
						else
						{
							component.RefreshSettings();
						}
					}
				}
			}
			if (hotloadObj != null)
			{
				hotloadObj.SetActive(status);
				return;
			}
			hotloadObj = GameObject.Find(SingletonBase<DynamicConfig>.GetInstance().GetString("MainSceneObj", "HotupdateObj", "HotupdateObj(Clone)"));
			if (hotloadObj != null)
			{
				hotloadObj.SetActive(status);
			}
		}
		catch (Exception ex)
		{
			Log.Info(ex.Message, "Selection");
			throw;
		}
	}

	public static GameObject GetUIRootGameObject()
	{
		if (WrappersManagerImpl.IsPatched(1395))
		{
			return WrappersManagerImpl.GetPatch(1395).__Gen_Wrap_311();
		}
		return SingletonBase<PopUpManager>.GetInstance().GetUIRoot();
	}

	[IDTag(0)]
	public static Transform GetUIRootTransform()
	{
		if (WrappersManagerImpl.IsPatched(5202))
		{
			return WrappersManagerImpl.GetPatch(5202).__Gen_Wrap_82();
		}
		GameObject uIRootGameObject = GetUIRootGameObject();
		if (!(uIRootGameObject != null))
		{
			return null;
		}
		return uIRootGameObject.transform;
	}

	[IDTag(1)]
	public static Transform GetUIRootTransform(SceneSwitch.Scene currentScene)
	{
		if (WrappersManagerImpl.IsPatched(21167))
		{
			return WrappersManagerImpl.GetPatch(21167).__Gen_Wrap_1108(currentScene);
		}
		GameObject uIRoot = SingletonBase<PopUpManager>.GetInstance().GetUIRoot();
		if (!(uIRoot != null))
		{
			return null;
		}
		return uIRoot.transform;
	}

	public static Transform GetPopupCanvasTransform(SceneSwitch.Scene currentScene)
	{
		if (WrappersManagerImpl.IsPatched(54999))
		{
			return WrappersManagerImpl.GetPatch(54999).__Gen_Wrap_1108(currentScene);
		}
		if (SSRInstituteUtil.isInSSRInstitute)
		{
			return SSRInstituteUtil.GetCanvasTransform();
		}
		return HappyMahjong.Core.Common.Util.GetPopupCanvasTransform(currentScene);
	}

	private static void DoLog(string info, string module, bool isError)
	{
		if (WrappersManagerImpl.IsPatched(342))
		{
			WrappersManagerImpl.GetPatch(342).__Gen_Wrap_80(info, module, isError);
		}
		else if (isError)
		{
			Log.Error(info, module);
		}
		else
		{
			Log.Info(info, module);
		}
	}

	public static bool SetName(Transform transform, string name, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61797))
		{
			return WrappersManagerImpl.GetPatch(61797).__Gen_Wrap_806(transform, name, isMistakeToError);
		}
		if (transform != null)
		{
			transform.gameObject.name = name;
			return true;
		}
		DoLog("SetName: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetClickUIEvent(Transform transform, UIEventListener.VoidDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(35712))
		{
			return WrappersManagerImpl.GetPatch(35712).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener.Get(transform.gameObject).onClick = clickDelegate;
			return true;
		}
		DoLog("SetClickUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool AddClickUIEvent(Transform transform, UIEventListener.VoidDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(41169))
		{
			return WrappersManagerImpl.GetPatch(41169).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(transform.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, clickDelegate);
			return true;
		}
		DoLog("AddClickUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool RemoveClickUIEvent(Transform transform, UIEventListener.VoidDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61798))
		{
			return WrappersManagerImpl.GetPatch(61798).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(transform.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, clickDelegate);
			return true;
		}
		DoLog("RemoveClickUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetDragStartUIEvent(Transform transform, UIEventListener.VoidDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61799))
		{
			return WrappersManagerImpl.GetPatch(61799).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener.Get(transform.gameObject).onDragStart = clickDelegate;
			return true;
		}
		DoLog("SetDragStartUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetDragEndUIEvent(Transform transform, UIEventListener.VectorDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61800))
		{
			return WrappersManagerImpl.GetPatch(61800).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener.Get(transform.gameObject).onDragEnd = clickDelegate;
			return true;
		}
		DoLog("SetDragEndUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUITexture(Transform transform, Texture texture, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61801))
		{
			return WrappersManagerImpl.GetPatch(61801).__Gen_Wrap_806(transform, texture, isMistakeToError);
		}
		if (transform != null)
		{
			UITexture component = transform.GetComponent<UITexture>();
			if (component != null)
			{
				component.mainTexture = texture;
				return true;
			}
		}
		DoLog("SetUITexture: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIText(Transform transform, string text, bool isMistakeToError = true, int size = -1)
	{
		if (WrappersManagerImpl.IsPatched(1014))
		{
			return WrappersManagerImpl.GetPatch(1014).__Gen_Wrap_245(transform, text, isMistakeToError, size);
		}
		if (transform != null)
		{
			TextMeshProUGUI component = transform.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				component.text = text;
				if (size != -1)
				{
					component.fontSize = size;
				}
				return true;
			}
			Text component2 = transform.GetComponent<Text>();
			if (component2 != null)
			{
				component2.text = text;
				if (size != -1)
				{
					component2.fontSize = size;
				}
				return true;
			}
		}
		return false;
	}

	public static bool SetUITextColor(Transform transform, Color color)
	{
		if (WrappersManagerImpl.IsPatched(36156))
		{
			return WrappersManagerImpl.GetPatch(36156).__Gen_Wrap_2443(transform, color);
		}
		if (transform != null)
		{
			TextMeshProUGUI component = transform.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				component.color = color;
				return true;
			}
			Text component2 = transform.GetComponent<Text>();
			if (component2 != null)
			{
				component2.color = color;
				return true;
			}
		}
		return false;
	}

	public static string GetUIText(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(4757))
		{
			return WrappersManagerImpl.GetPatch(4757).__Gen_Wrap_20(transform, isMistakeToError);
		}
		if (transform != null)
		{
			TextMeshProUGUI component = transform.GetComponent<TextMeshProUGUI>();
			if (component != null)
			{
				return component.text;
			}
			Text component2 = transform.GetComponent<Text>();
			if (component2 != null)
			{
				return component2.text;
			}
		}
		DoLog("GetUIText: UI have a mistake", "Framework", isMistakeToError);
		return "";
	}

	public static bool AddProfixUIText(Transform transform, string text, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61802))
		{
			return WrappersManagerImpl.GetPatch(61802).__Gen_Wrap_806(transform, text, isMistakeToError);
		}
		if (transform != null)
		{
			UILabel component = transform.GetComponent<UILabel>();
			if (component != null)
			{
				component.text = text + component.text;
				return true;
			}
		}
		DoLog("AddProfixUIText: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool AddProfixUITextUgui(Transform transform, string text, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(54815))
		{
			return WrappersManagerImpl.GetPatch(54815).__Gen_Wrap_806(transform, text, isMistakeToError);
		}
		if (transform != null)
		{
			Text component = transform.GetComponent<Text>();
			if (component != null)
			{
				component.text = text + component.text;
				return true;
			}
		}
		DoLog("AddProfixUIText: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	[IDTag(1)]
	public static bool SetUIActive(string name, bool isActive, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(21166))
		{
			return WrappersManagerImpl.GetPatch(21166).__Gen_Wrap_115(name, isActive, isMistakeToError);
		}
		Transform uIRootTransform = GetUIRootTransform(SceneSwitch.CurrentScene());
		if (uIRootTransform != null && !string.IsNullOrEmpty(name))
		{
			return SetUIActive(uIRootTransform.Find(name), isActive, isMistakeToError);
		}
		DoLog("SetUIActive: uiRoot is empty or name is invalid", "Framework", isMistakeToError);
		return false;
	}

	[IDTag(0)]
	public static bool SetUIActive(Transform transform, bool isActive, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(341))
		{
			return WrappersManagerImpl.GetPatch(341).__Gen_Wrap_115(transform, isActive, isMistakeToError);
		}
		if (transform != null)
		{
			transform.gameObject.SetActive(isActive);
			return true;
		}
		DoLog("SetUIActive: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUISwitchActive(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61803))
		{
			return WrappersManagerImpl.GetPatch(61803).__Gen_Wrap_126(transform, isMistakeToError);
		}
		if (transform != null)
		{
			transform.gameObject.SetActive(!transform.gameObject.activeSelf);
			return true;
		}
		DoLog("SetUISwitchActive: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool IsUIActive(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(59858))
		{
			return WrappersManagerImpl.GetPatch(59858).__Gen_Wrap_126(transform, isMistakeToError);
		}
		if (transform != null)
		{
			return transform.gameObject.activeSelf;
		}
		DoLog("IsUIActive: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	[IDTag(0)]
	public static bool SetUIAnchor(Transform transform, GameObject targetGameObject, int left = 0, int bottom = 0, int right = 0, int top = 0, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61804))
		{
			return WrappersManagerImpl.GetPatch(61804).__Gen_Wrap_4154(transform, targetGameObject, left, bottom, right, top, isMistakeToError);
		}
		if (transform != null && targetGameObject != null)
		{
			UIRect component = transform.gameObject.GetComponent<UIRect>();
			if (component != null)
			{
				component.SetAnchor(targetGameObject, left, bottom, right, top);
				return true;
			}
		}
		DoLog("SetUIAnchor: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	[IDTag(1)]
	public static bool SetUIAnchor(Transform transform, Transform targetTransform, int left = 0, int bottom = 0, int right = 0, int top = 0, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61805))
		{
			return WrappersManagerImpl.GetPatch(61805).__Gen_Wrap_4154(transform, targetTransform, left, bottom, right, top, isMistakeToError);
		}
		if (targetTransform != null)
		{
			return SetUIAnchor(transform, targetTransform.gameObject, left, bottom, right, top, isMistakeToError);
		}
		DoLog("SetUIAnchor: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUISprite(Transform transform, string spriteName, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(35389))
		{
			return WrappersManagerImpl.GetPatch(35389).__Gen_Wrap_806(transform, spriteName, isMistakeToError);
		}
		if (transform != null)
		{
			UISprite component = transform.GetComponent<UISprite>();
			if (component != null)
			{
				component.spriteName = spriteName;
				return true;
			}
		}
		DoLog("SetUISprite: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIColor(Transform transform, Color color, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61806))
		{
			return WrappersManagerImpl.GetPatch(61806).__Gen_Wrap_4155(transform, color, isMistakeToError);
		}
		if (transform != null)
		{
			UIWidget component = transform.GetComponent<UIWidget>();
			if (component != null)
			{
				component.color = color;
				return true;
			}
		}
		DoLog("SetUIColor: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static void SetUIImage(Transform transform, string atlasName, string spriteName, bool nativeSize = false)
	{
		if (WrappersManagerImpl.IsPatched(5033))
		{
			WrappersManagerImpl.GetPatch(5033).__Gen_Wrap_49(transform, atlasName, spriteName, nativeSize);
		}
		else
		{
			if (!(transform != null))
			{
				return;
			}
			Image component = transform.GetComponent<Image>();
			if (component != null)
			{
				component.sprite = UIUtil.GetSprite(atlasName, spriteName);
				if (nativeSize)
				{
					component.SetNativeSize();
				}
			}
		}
	}

	[IDTag(0)]
	public static bool SetUIBtnSprite(Transform transform, string spriteName, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61807))
		{
			return WrappersManagerImpl.GetPatch(61807).__Gen_Wrap_806(transform, spriteName, isMistakeToError);
		}
		if (transform != null)
		{
			UIButton component = transform.GetComponent<UIButton>();
			if (component != null)
			{
				component.normalSprite = spriteName;
				return true;
			}
		}
		DoLog("SetUIBtnSprite: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	[IDTag(1)]
	public static bool SetUIBtnSprite(Transform transform, string spriteNormal, string spriteDisable, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61808))
		{
			return WrappersManagerImpl.GetPatch(61808).__Gen_Wrap_652(transform, spriteNormal, spriteDisable, isMistakeToError);
		}
		if (transform != null)
		{
			UIButton component = transform.GetComponent<UIButton>();
			if (component != null)
			{
				component.normalSprite = spriteNormal;
				component.hoverSprite = spriteNormal;
				component.pressedSprite = spriteNormal;
				component.disabledSprite = spriteDisable;
				return true;
			}
		}
		DoLog("SetUIBtnSprite: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIBtnEnable(Transform transform, bool isEnable, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61809))
		{
			return WrappersManagerImpl.GetPatch(61809).__Gen_Wrap_115(transform, isEnable, isMistakeToError);
		}
		if (transform != null)
		{
			BoxCollider component = transform.GetComponent<BoxCollider>();
			if (component != null)
			{
				component.enabled = isEnable;
				UIButton component2 = transform.GetComponent<UIButton>();
				if (component2 != null)
				{
					component2.SetState((!isEnable) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, immediate: true);
				}
				return true;
			}
		}
		DoLog("SetUIEnable: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool ChangeUIAtlas(Transform transform, Transform atlasTransform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61810))
		{
			return WrappersManagerImpl.GetPatch(61810).__Gen_Wrap_806(transform, atlasTransform, isMistakeToError);
		}
		if (transform != null && atlasTransform != null)
		{
			UISprite component = transform.GetComponent<UISprite>();
			UIAtlas component2 = atlasTransform.GetComponent<UIAtlas>();
			if (component2 != null && component != null)
			{
				component.atlas = component2;
				return true;
			}
		}
		DoLog("GetUIAtlas: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIEnable(Transform transform, bool isEnable, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61811))
		{
			return WrappersManagerImpl.GetPatch(61811).__Gen_Wrap_115(transform, isEnable, isMistakeToError);
		}
		if (transform != null)
		{
			BoxCollider component = transform.GetComponent<BoxCollider>();
			if (component != null)
			{
				component.enabled = isEnable;
				return true;
			}
		}
		DoLog("SetUIEnable: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static int GetUIWidth(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61812))
		{
			return WrappersManagerImpl.GetPatch(61812).__Gen_Wrap_498(transform, isMistakeToError);
		}
		if (transform != null)
		{
			UIWidget component = transform.GetComponent<UIWidget>();
			if (component != null)
			{
				return component.width;
			}
		}
		DoLog("GetUIWidth: UI have a mistake", "Framework", isMistakeToError);
		return 0;
	}

	public static bool SetUIWidth(Transform transform, int width, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(55223))
		{
			return WrappersManagerImpl.GetPatch(55223).__Gen_Wrap_3192(transform, width, isMistakeToError);
		}
		if (transform != null)
		{
			UIWidget component = transform.GetComponent<UIWidget>();
			if (component != null)
			{
				component.width = width;
				return true;
			}
		}
		DoLog("SetUIWidth: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIHeight(Transform transform, int height, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61813))
		{
			return WrappersManagerImpl.GetPatch(61813).__Gen_Wrap_3192(transform, height, isMistakeToError);
		}
		if (transform != null)
		{
			UIWidget component = transform.GetComponent<UIWidget>();
			if (component != null)
			{
				component.height = height;
				return true;
			}
		}
		DoLog("SetUIHeight: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetGridCellW(Transform transform, int width, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(55224))
		{
			return WrappersManagerImpl.GetPatch(55224).__Gen_Wrap_3192(transform, width, isMistakeToError);
		}
		if (transform != null)
		{
			UIGrid component = transform.GetComponent<UIGrid>();
			if (component != null)
			{
				component.cellWidth = width;
				component.Reposition();
				return true;
			}
		}
		DoLog("SetGridCellW: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static int GetGridCellW(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61814))
		{
			return WrappersManagerImpl.GetPatch(61814).__Gen_Wrap_498(transform, isMistakeToError);
		}
		if (transform != null)
		{
			UIGrid component = transform.GetComponent<UIGrid>();
			if (component != null)
			{
				return (int)component.cellWidth;
			}
		}
		DoLog("SetGridCellW: UI have a mistake", "Framework", isMistakeToError);
		return 0;
	}

	public static bool DeleteAllChild(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(54814))
		{
			return WrappersManagerImpl.GetPatch(54814).__Gen_Wrap_126(transform, isMistakeToError);
		}
		if (transform != null)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
			}
			transform.DetachChildren();
			return true;
		}
		DoLog("DeleteAllChild: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIXPosition(Transform transform, int xPos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61815))
		{
			return WrappersManagerImpl.GetPatch(61815).__Gen_Wrap_3192(transform, xPos, isMistakeToError);
		}
		if (transform != null)
		{
			Vector3 localPosition = transform.localPosition;
			transform.localPosition = new Vector3(xPos, localPosition.y, localPosition.z);
			return true;
		}
		DoLog("SetUIXPosition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIYPosition(Transform transform, int yPos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61816))
		{
			return WrappersManagerImpl.GetPatch(61816).__Gen_Wrap_3192(transform, yPos, isMistakeToError);
		}
		if (transform != null)
		{
			Vector3 localPosition = transform.localPosition;
			transform.localPosition = new Vector3(localPosition.x, yPos, localPosition.z);
			return true;
		}
		DoLog("SetUIYPosition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIGridReposition(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61817))
		{
			return WrappersManagerImpl.GetPatch(61817).__Gen_Wrap_126(transform, isMistakeToError);
		}
		if (transform != null)
		{
			UIGrid component = transform.GetComponent<UIGrid>();
			if (component != null)
			{
				component.Reposition();
				return true;
			}
		}
		DoLog("SetUIGridReposition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIScrollReposition(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61818))
		{
			return WrappersManagerImpl.GetPatch(61818).__Gen_Wrap_126(transform, isMistakeToError);
		}
		if (transform != null)
		{
			UIScrollView component = transform.GetComponent<UIScrollView>();
			if (component != null)
			{
				component.ResetPosition();
				return true;
			}
		}
		DoLog("SetUIScrollReposition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIScrollMoveAbsolute(Transform transform, Vector3 pos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61819))
		{
			return WrappersManagerImpl.GetPatch(61819).__Gen_Wrap_4156(transform, pos, isMistakeToError);
		}
		if (transform != null)
		{
			UIScrollView component = transform.GetComponent<UIScrollView>();
			if (component != null)
			{
				component.MoveAbsolute(pos);
				return true;
			}
		}
		DoLog("SetUIScrollMoveAbsolute: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUITableReposition(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61820))
		{
			return WrappersManagerImpl.GetPatch(61820).__Gen_Wrap_126(transform, isMistakeToError);
		}
		if (transform != null)
		{
			UITable component = transform.GetComponent<UITable>();
			if (component != null)
			{
				component.Reposition();
				return true;
			}
		}
		DoLog("SetUIGridReposition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUIProgress(Transform transform, float progress, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61821))
		{
			return WrappersManagerImpl.GetPatch(61821).__Gen_Wrap_4157(transform, progress, isMistakeToError);
		}
		if (transform != null)
		{
			UIProgressBar component = transform.GetComponent<UIProgressBar>();
			if (component != null)
			{
				component.value = progress;
				return true;
			}
		}
		DoLog("SetUIProgress: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static float GetUIProgress(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61822))
		{
			return WrappersManagerImpl.GetPatch(61822).__Gen_Wrap_4158(transform, isMistakeToError);
		}
		if (transform != null)
		{
			UIProgressBar component = transform.GetComponent<UIProgressBar>();
			if (component != null)
			{
				return component.value;
			}
		}
		DoLog("GetUIProgress: UI have a mistake", "Framework", isMistakeToError);
		return 0f;
	}

	public static void SetUIAlpha(Transform transform, float alpha, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61823))
		{
			WrappersManagerImpl.GetPatch(61823).__Gen_Wrap_1113(transform, alpha, isMistakeToError);
			return;
		}
		if (transform != null)
		{
			UIRect component = transform.GetComponent<UIRect>();
			if (component != null)
			{
				component.alpha = alpha;
				return;
			}
		}
		DoLog("SetUIAlpha: UI have a mistake", "Framework", isMistakeToError);
	}

	[IDTag(0)]
	public static void SetUGUISprite(Transform tran, string abName, string spriteName)
	{
		if (WrappersManagerImpl.IsPatched(21751))
		{
			WrappersManagerImpl.GetPatch(21751).__Gen_Wrap_17(tran, abName, spriteName);
		}
		else
		{
			tran.GetComponent<Image>().sprite = UIUtil.GetSprite(abName, spriteName);
		}
	}

	public static bool SetUGUISlider(Transform transform, float progress, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61824))
		{
			return WrappersManagerImpl.GetPatch(61824).__Gen_Wrap_4157(transform, progress, isMistakeToError);
		}
		if (transform != null)
		{
			Slider component = transform.GetComponent<Slider>();
			if (component != null)
			{
				component.value = progress;
				return true;
			}
		}
		DoLog("SetUGUISlider: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static float GetUGUISlider(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61825))
		{
			return WrappersManagerImpl.GetPatch(61825).__Gen_Wrap_4158(transform, isMistakeToError);
		}
		if (transform != null)
		{
			Slider component = transform.GetComponent<Slider>();
			if (component != null)
			{
				return component.value;
			}
		}
		DoLog("GetUGUISlider: UI have a mistake", "Framework", isMistakeToError);
		return 0f;
	}

	public static bool SetPressEventEx(Transform transform, UIEventListenerEx.BoolDelegate uiDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61826))
		{
			return WrappersManagerImpl.GetPatch(61826).__Gen_Wrap_806(transform, uiDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListenerEx.Get(transform.gameObject).onPress = uiDelegate;
			return true;
		}
		DoLog("SetPressEventEx: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetHoverEventEx(Transform transform, UIEventListenerEx.BoolDelegate uiDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61827))
		{
			return WrappersManagerImpl.GetPatch(61827).__Gen_Wrap_806(transform, uiDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListenerEx.Get(transform.gameObject).onHover = uiDelegate;
			return true;
		}
		DoLog("SetPressEventEx: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetDragStartUGUIEventEx(Transform transform, UIEventListenerEx.VoidDelegate uiDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61828))
		{
			return WrappersManagerImpl.GetPatch(61828).__Gen_Wrap_806(transform, uiDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListenerEx.Get(transform.gameObject).onDragStart = uiDelegate;
			return true;
		}
		DoLog("SetDragStartUGUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetDragEndUGUIEventEx(Transform transform, UIEventListenerEx.VoidDelegate uiDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61829))
		{
			return WrappersManagerImpl.GetPatch(61829).__Gen_Wrap_806(transform, uiDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListenerEx.Get(transform.gameObject).onDragEnd = uiDelegate;
			return true;
		}
		DoLog("SetDragEndUGUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetDragUGUIEventEx(Transform transform, UIEventListenerEx.VectorDelegate uiDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61830))
		{
			return WrappersManagerImpl.GetPatch(61830).__Gen_Wrap_806(transform, uiDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListenerEx.Get(transform.gameObject).onDrag = uiDelegate;
			return true;
		}
		DoLog("SetDragUGUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetClickUGUIEventEx(Transform transform, UIEventListener.VoidDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61831))
		{
			return WrappersManagerImpl.GetPatch(61831).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener.Get(transform.gameObject).onClick = clickDelegate;
			return true;
		}
		DoLog("SetClickUGUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetClickUGUIEvent(Transform transform, UIEventListener.VoidDelegate clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(39256))
		{
			return WrappersManagerImpl.GetPatch(39256).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			UIEventListener.Get(transform.gameObject).onClick = clickDelegate;
			return true;
		}
		DoLog("SetClickUGUIEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIWidth(Transform transform, int width, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61832))
		{
			return WrappersManagerImpl.GetPatch(61832).__Gen_Wrap_3192(transform, width, isMistakeToError);
		}
		if (transform != null)
		{
			RectTransform rectTransform = transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
				return true;
			}
		}
		DoLog("SetUGUIWidth: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIHeight(Transform transform, int height, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61833))
		{
			return WrappersManagerImpl.GetPatch(61833).__Gen_Wrap_3192(transform, height, isMistakeToError);
		}
		if (transform != null)
		{
			RectTransform rectTransform = transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
				return true;
			}
		}
		DoLog("SetUGUIHeight: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static Vector3 GetUGUIVector3(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61834))
		{
			return WrappersManagerImpl.GetPatch(61834).__Gen_Wrap_4159(transform, isMistakeToError);
		}
		if (transform != null)
		{
			return (transform as RectTransform).anchoredPosition3D;
		}
		DoLog("GetUGUIVector2: UI have a mistake", "Framework", isMistakeToError);
		return Vector3.zero;
	}

	public static bool SetUGUIVector3(Transform transform, Vector3 pos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61835))
		{
			return WrappersManagerImpl.GetPatch(61835).__Gen_Wrap_4156(transform, pos, isMistakeToError);
		}
		if (transform != null)
		{
			(transform as RectTransform).anchoredPosition3D = pos;
			return true;
		}
		DoLog("SetUGUIVector2: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static Vector2 GetUGUIVector2(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61836))
		{
			return WrappersManagerImpl.GetPatch(61836).__Gen_Wrap_4160(transform, isMistakeToError);
		}
		if (transform != null)
		{
			return (transform as RectTransform).anchoredPosition;
		}
		DoLog("GetUGUIVector2: UI have a mistake", "Framework", isMistakeToError);
		return Vector2.zero;
	}

	public static bool SetUGUIVector2(Transform transform, Vector2 pos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61837))
		{
			return WrappersManagerImpl.GetPatch(61837).__Gen_Wrap_3191(transform, pos, isMistakeToError);
		}
		if (transform != null)
		{
			(transform as RectTransform).anchoredPosition = pos;
			return true;
		}
		DoLog("SetUGUIVector2: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIYPosition(Transform transform, int yPos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(46730))
		{
			return WrappersManagerImpl.GetPatch(46730).__Gen_Wrap_3192(transform, yPos, isMistakeToError);
		}
		if (transform != null)
		{
			RectTransform obj = transform as RectTransform;
			obj.anchoredPosition = new Vector2(obj.anchoredPosition.x, yPos);
			return true;
		}
		DoLog("SetUGUIYPosition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIXPosition(Transform transform, int xPos, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61838))
		{
			return WrappersManagerImpl.GetPatch(61838).__Gen_Wrap_3192(transform, xPos, isMistakeToError);
		}
		if (transform != null)
		{
			RectTransform rectTransform = transform as RectTransform;
			rectTransform.anchoredPosition = new Vector2(xPos, rectTransform.anchoredPosition.y);
			return true;
		}
		DoLog("SetUGUIXPosition: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIPivot(Transform transform, Vector2 pivot, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(46729))
		{
			return WrappersManagerImpl.GetPatch(46729).__Gen_Wrap_3191(transform, pivot, isMistakeToError);
		}
		if (transform != null)
		{
			(transform as RectTransform).pivot = pivot;
			return true;
		}
		DoLog("SetUGUIPivot: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static void SetUGUIPivotKeepPosition(Transform transform, Vector2 newPivot)
	{
		if (WrappersManagerImpl.IsPatched(61839))
		{
			WrappersManagerImpl.GetPatch(61839).__Gen_Wrap_327(transform, newPivot);
		}
		else if (!(transform == null))
		{
			RectTransform rectTransform = transform as RectTransform;
			if (!(rectTransform == null))
			{
				Vector2 size = rectTransform.rect.size;
				Vector2 vector = newPivot - rectTransform.pivot;
				Vector2 vector2 = new Vector2(vector.x * size.x, vector.y * size.y);
				rectTransform.pivot = newPivot;
				rectTransform.anchoredPosition += vector2;
			}
		}
	}

	public static bool SetUGUIEnable(Transform transform, bool isEnable, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61840))
		{
			return WrappersManagerImpl.GetPatch(61840).__Gen_Wrap_115(transform, isEnable, isMistakeToError);
		}
		if (transform != null)
		{
			Graphic component = transform.GetComponent<Graphic>();
			if (component != null)
			{
				component.raycastTarget = isEnable;
				return true;
			}
		}
		DoLog("SetUGUIEnable: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIBtnEnable(Transform transform, bool isEnable, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61841))
		{
			return WrappersManagerImpl.GetPatch(61841).__Gen_Wrap_115(transform, isEnable, isMistakeToError);
		}
		if (transform != null)
		{
			Graphic component = transform.GetComponent<Graphic>();
			if (component != null)
			{
				component.raycastTarget = isEnable;
			}
			ButtonListener component2 = transform.GetComponent<ButtonListener>();
			if (component2 != null)
			{
				component2.interactable = isEnable;
				if (component2.transition == Selectable.Transition.SpriteSwap)
				{
					component2.image.sprite = (isEnable ? component2.spriteState.highlightedSprite : component2.spriteState.disabledSprite);
				}
			}
			if (component != null || component2 != null)
			{
				return true;
			}
		}
		DoLog("SetUIEnable: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static bool SetUGUIBtnSprite(Transform transform, SpriteState spriteState, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61842))
		{
			return WrappersManagerImpl.GetPatch(61842).__Gen_Wrap_4161(transform, spriteState, isMistakeToError);
		}
		if (transform != null)
		{
			ButtonListener component = transform.GetComponent<ButtonListener>();
			if (component != null)
			{
				component.spriteState = spriteState;
				return true;
			}
		}
		DoLog("SetUGUIBtnSprite: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static void SetUGUIAlpha(Transform transform, float alpha, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61843))
		{
			WrappersManagerImpl.GetPatch(61843).__Gen_Wrap_1113(transform, alpha, isMistakeToError);
			return;
		}
		if (transform != null)
		{
			CanvasGroup component = transform.GetComponent<CanvasGroup>();
			if (component != null)
			{
				component.alpha = alpha;
				return;
			}
			Graphic component2 = transform.GetComponent<Graphic>();
			if (component2 != null)
			{
				component2.color = new Color(component2.color.r, component2.color.g, component2.color.b, alpha);
				return;
			}
		}
		DoLog("SetUGUIAlpha: UI have a mistake", "Framework", isMistakeToError);
	}

	[IDTag(1)]
	public static bool SetUGUISprite(Transform transform, Sprite sprite, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(58947))
		{
			return WrappersManagerImpl.GetPatch(58947).__Gen_Wrap_806(transform, sprite, isMistakeToError);
		}
		if (transform != null)
		{
			Image component = transform.GetComponent<Image>();
			if (component != null)
			{
				component.sprite = sprite;
				return true;
			}
		}
		DoLog("SetUISprite: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static int GetUGUILayoutW(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61844))
		{
			return WrappersManagerImpl.GetPatch(61844).__Gen_Wrap_498(transform, isMistakeToError);
		}
		if (transform != null)
		{
			transform.GetComponent<HorizontalLayoutGroup>();
			LayoutGroup component = transform.GetComponent<LayoutGroup>();
			if (component != null)
			{
				return (int)component.minWidth;
			}
		}
		DoLog("GetUGUILayoutW: UI have a mistake", "Framework", isMistakeToError);
		return 0;
	}

	public static int GetUGUILayoutCellW(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61845))
		{
			return WrappersManagerImpl.GetPatch(61845).__Gen_Wrap_498(transform, isMistakeToError);
		}
		if (transform != null)
		{
			LayoutElement component = transform.GetComponent<LayoutElement>();
			if (component != null)
			{
				return (int)component.preferredWidth;
			}
		}
		DoLog("GetUGUILayoutW: UI have a mistake", "Framework", isMistakeToError);
		return 0;
	}

	public static Transform GetUGUIGameRootTransform()
	{
		if (WrappersManagerImpl.IsPatched(61846))
		{
			return WrappersManagerImpl.GetPatch(61846).__Gen_Wrap_82();
		}
		GameObject gameObject = GameObject.Find("GameUIRoot");
		if (!(gameObject != null))
		{
			return null;
		}
		return gameObject.transform.Find("Canvas");
	}

	public static void UpdateNearCanvas(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61847))
		{
			WrappersManagerImpl.GetPatch(61847).__Gen_Wrap_8(transform, isMistakeToError);
		}
		else
		{
			Canvas.ForceUpdateCanvases();
		}
	}

	public static void UpdateRectTrasform(Transform transform, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(21201))
		{
			WrappersManagerImpl.GetPatch(21201).__Gen_Wrap_8(transform, isMistakeToError);
		}
		else
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
		}
	}

	private static bool SetClickUGUIButtonEvent(Transform transform, UnityAction clickDelegate, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61848))
		{
			return WrappersManagerImpl.GetPatch(61848).__Gen_Wrap_806(transform, clickDelegate, isMistakeToError);
		}
		if (transform != null)
		{
			ButtonListener component = transform.GetComponent<ButtonListener>();
			if (component != null)
			{
				component.onClick.RemoveAllListeners();
				component.onClick.AddListener(clickDelegate);
				return true;
			}
			return false;
		}
		DoLog("SetClickUIButtonEvent: UI have a mistake", "Framework", isMistakeToError);
		return false;
	}

	public static void SetScrollRectContent(Transform transform, RectTransform content, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61849))
		{
			WrappersManagerImpl.GetPatch(61849).__Gen_Wrap_80(transform, content, isMistakeToError);
		}
		else if (transform != null)
		{
			ScrollRect component = transform.GetComponent<ScrollRect>();
			if (component != null)
			{
				component.content = content;
			}
		}
		else
		{
			DoLog("SetScrollRectContent: UI have a mistake", "Framework", isMistakeToError);
		}
	}

	public static void SetEffectSortingLayer(Transform tsfEffect)
	{
		if (WrappersManagerImpl.IsPatched(5086))
		{
			WrappersManagerImpl.GetPatch(5086).__Gen_Wrap_0(tsfEffect);
			return;
		}
		Canvas componentInParent = tsfEffect.GetComponentInParent<Canvas>();
		if (componentInParent == null)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = tsfEffect.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer component = componentsInChildren[i].GetComponent<Renderer>();
			if (component != null)
			{
				component.sortingLayerName = componentInParent.sortingLayerName;
			}
		}
		MeshRenderer[] componentsInChildren2 = tsfEffect.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].sortingLayerName = componentInParent.sortingLayerName;
		}
		Canvas[] componentsInChildren3 = tsfEffect.GetComponentsInChildren<Canvas>();
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].sortingLayerName = componentInParent.sortingLayerName;
		}
		TrailRenderer[] componentsInChildren4 = tsfEffect.GetComponentsInChildren<TrailRenderer>();
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			componentsInChildren4[i].sortingLayerName = componentInParent.sortingLayerName;
		}
	}

	public static Vector3 GetLocalPosRelativeToTarget(Transform local, Transform target)
	{
		if (WrappersManagerImpl.IsPatched(61850))
		{
			return WrappersManagerImpl.GetPatch(61850).__Gen_Wrap_166(local, target);
		}
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(local.parent.GetComponent<RectTransform>(), screenPoint, null, out var localPoint);
		return localPoint;
	}

	public static Vector2 GetAnchorRootSize()
	{
		if (WrappersManagerImpl.IsPatched(48225))
		{
			return WrappersManagerImpl.GetPatch(48225).__Gen_Wrap_316();
		}
		GameObject gameObject = GameObject.Find("Anchor Root");
		if (gameObject != null)
		{
			UIWidget component = gameObject.GetComponent<UIWidget>();
			if (component != null)
			{
				return new Vector2(component.width, component.height);
			}
			return gameObject.GetComponent<RectTransform>().rect.size;
		}
		return new Vector2(0f, 0f);
	}

	public static UICamera CurrentUICamera()
	{
		if (WrappersManagerImpl.IsPatched(198))
		{
			return WrappersManagerImpl.GetPatch(198).__Gen_Wrap_71();
		}
		GameObject gameObject = GameObject.Find("UICamera");
		if (gameObject != null)
		{
			gameObject.GetComponent<UICamera>();
		}
		return null;
	}

	public static void SetUICameraAble(bool bAble)
	{
		if (WrappersManagerImpl.IsPatched(61851))
		{
			WrappersManagerImpl.GetPatch(61851).__Gen_Wrap_192(bAble);
			return;
		}
		GameObject gameObject = GameObject.Find("UICamera");
		UICamera uICamera = null;
		if (gameObject != null)
		{
			Transform transform = gameObject.transform;
			if (transform != null)
			{
				uICamera = transform.GetComponent<UICamera>();
			}
		}
		if (uICamera != null)
		{
			uICamera.useTouch = bAble;
		}
	}

	public static void SetBtnWithLabelEnable(GameObject btn, bool enable)
	{
		if (WrappersManagerImpl.IsPatched(40008))
		{
			WrappersManagerImpl.GetPatch(40008).__Gen_Wrap_8(btn, enable);
		}
		else
		{
			if (btn == null)
			{
				return;
			}
			UIButton component = btn.GetComponent<UIButton>();
			if (component == null)
			{
				return;
			}
			UILabel[] componentsInChildren = btn.GetComponentsInChildren<UILabel>();
			if (componentsInChildren == null)
			{
				return;
			}
			if (enable)
			{
				component.isEnabled = true;
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].color = WhiteColor;
				}
			}
			else
			{
				component.isEnabled = false;
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].color = GrayColor;
				}
			}
		}
	}

	public static void addEffectClip(Transform tfNode)
	{
		if (WrappersManagerImpl.IsPatched(21747))
		{
			WrappersManagerImpl.GetPatch(21747).__Gen_Wrap_0(tfNode);
			return;
		}
		MeshRenderer[] componentsInChildren = tfNode.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (!meshRenderer.gameObject.GetComponent<ParticleMask>())
			{
				meshRenderer.gameObject.AddComponent<ParticleMask>();
			}
		}
		ParticleSystem[] componentsInChildren2 = tfNode.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren2)
		{
			if (!particleSystem.gameObject.GetComponent<ParticleMask>())
			{
				particleSystem.gameObject.AddComponent<ParticleMask>();
			}
		}
	}

	public static void AdjustScrollviewSize(UIPanel scrollPanel)
	{
		if (WrappersManagerImpl.IsPatched(61852))
		{
			WrappersManagerImpl.GetPatch(61852).__Gen_Wrap_0(scrollPanel);
			return;
		}
		float num = (float)Screen.width / (float)Screen.height;
		if (num > 1.78f)
		{
			scrollPanel.baseClipRegion = new Vector4(scrollPanel.baseClipRegion.x, scrollPanel.baseClipRegion.y, ((num / 1.78f - 1f) * 0.6f + 1f) * 1150f, scrollPanel.baseClipRegion.w);
		}
	}

	public static void SetWebTexture(Transform transform, string url, string defaultImage = null)
	{
		if (WrappersManagerImpl.IsPatched(61853))
		{
			WrappersManagerImpl.GetPatch(61853).__Gen_Wrap_17(transform, url, defaultImage);
			return;
		}
		if (transform != null)
		{
			UITexture component = transform.GetComponent<UITexture>();
			if (component != null)
			{
				Texture coreData = DownloadPicManager.GetInstance().GetCoreData(url);
				if (coreData == null)
				{
					DownloadAndSetTexture(component, url, defaultImage, null);
				}
				else
				{
					component.mainTexture = coreData;
				}
				return;
			}
		}
		Log.Error("SetWebTexture is invalid", "Framework");
	}

	public static void SetWebTextureUIUGUI(Transform transform, string url, string defaultImage = null, DownloadPicManager.URLTextureDelegate callback = null, bool needCache = true, bool isActivity = false)
	{
		if (WrappersManagerImpl.IsPatched(5647))
		{
			WrappersManagerImpl.GetPatch(5647).__Gen_Wrap_902(transform, url, defaultImage, callback, needCache, isActivity);
			return;
		}
		if (transform != null)
		{
			RawImage component = transform.GetComponent<RawImage>();
			if (component != null)
			{
				Texture coreData = DownloadPicManager.GetInstance().GetCoreData(url);
				if (coreData == null)
				{
					DownloadAndSetTexture(component, url, defaultImage, null, callback, needCache, isActivity);
					return;
				}
				component.texture = coreData;
				callback?.Invoke(url, coreData);
				return;
			}
		}
		Log.Error("SetWebTexture is invalid", "Framework");
	}

	public static void LoadWebTextureUIUGUI(string url, string defaultImage = null, DownloadPicManager.URLTextureDelegate callback = null, bool needCache = true, bool isActivity = false)
	{
		if (WrappersManagerImpl.IsPatched(53213))
		{
			WrappersManagerImpl.GetPatch(53213).__Gen_Wrap_257(url, defaultImage, callback, needCache, isActivity);
			return;
		}
		Texture coreData = DownloadPicManager.GetInstance().GetCoreData(url);
		if (coreData == null)
		{
			DownloadAndSetTexture(null, url, defaultImage, null, callback, needCache, isActivity);
		}
		else
		{
			callback?.Invoke(url, coreData);
		}
	}

	public static void LoadWebSpriteUIUGUI(string url, string defaultImage = null, DownloadPicManager.URLSpriteDelegate callback = null, bool needCache = true, bool isActivity = false, Func<Texture, Sprite> transFunc = null)
	{
		if (WrappersManagerImpl.IsPatched(4767))
		{
			WrappersManagerImpl.GetPatch(4767).__Gen_Wrap_774(url, defaultImage, callback, needCache, isActivity, transFunc);
			return;
		}
		Func<Texture, Sprite> transFunc2 = transFunc;
		bool needCache2 = needCache;
		DownloadPicManager.URLSpriteDelegate callback2 = callback;
		string url2 = url;
		Sprite coreDataSprite = DownloadPicManager.GetInstance().GetCoreDataSprite(url2);
		if (coreDataSprite == null)
		{
			DownloadAndSetTexture(null, url2, defaultImage, null, delegate(string reqUrl, Texture texture)
			{
				Sprite sprite = ((transFunc2 != null) ? transFunc2(texture) : Sprite.Create(texture as Texture2D, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
				if (needCache2)
				{
					DownloadPicManager.GetInstance().SetCoreDataSprite(reqUrl, sprite);
				}
				callback2?.Invoke(url2, sprite);
			}, needCache2, isActivity);
		}
		else
		{
			callback2?.Invoke(url2, coreDataSprite);
		}
	}

	public static bool ContainsTextureReqTask(string mUrl)
	{
		if (WrappersManagerImpl.IsPatched(53211))
		{
			return WrappersManagerImpl.GetPatch(53211).__Gen_Wrap_3(mUrl);
		}
		return DownloadPicManager.GetInstance().ContainsReqTask(mUrl.GetHashCode().ToString());
	}

	public static void SetWebTextureUIUGUIWithTexture(Transform transform, Texture texture)
	{
		if (WrappersManagerImpl.IsPatched(61855))
		{
			WrappersManagerImpl.GetPatch(61855).__Gen_Wrap_1(transform, texture);
			return;
		}
		if (transform != null)
		{
			RawImage component = transform.GetComponent<RawImage>();
			if (component != null)
			{
				component.texture = texture;
				return;
			}
		}
		Log.Error("SetWebTexture is invalid", "Framework");
	}

	[IDTag(2)]
	public static void DownloadAndSetTexture(UITexture texture, byte[] url)
	{
		if (WrappersManagerImpl.IsPatched(61856))
		{
			WrappersManagerImpl.GetPatch(61856).__Gen_Wrap_1(texture, url);
			return;
		}
		string rawPathFromServer = Encoding.UTF8.GetString(url).TrimEnd('\0');
		DownloadAndSetTexture(texture, CommonTools.GetValidPlayerIconPath(rawPathFromServer), null, null);
	}

	[IDTag(0)]
	public static void DownloadAndSetTextureUrl(UITexture texture, string rawIconUrl)
	{
		if (WrappersManagerImpl.IsPatched(61857))
		{
			WrappersManagerImpl.GetPatch(61857).__Gen_Wrap_1(texture, rawIconUrl);
		}
		else
		{
			DownloadAndSetTexture(texture, CommonTools.GetValidPlayerIconPath(rawIconUrl), null, null);
		}
	}

	[IDTag(1)]
	public static void DownloadAndSetTextureUrl(RawImage texture, string rawIconUrl)
	{
		if (WrappersManagerImpl.IsPatched(61858))
		{
			WrappersManagerImpl.GetPatch(61858).__Gen_Wrap_1(texture, rawIconUrl);
		}
		else
		{
			DownloadAndSetTexture(texture, CommonTools.GetValidPlayerIconPath(rawIconUrl), null, null);
		}
	}

	[IDTag(3)]
	public static void DownloadAndSetTexture(RawImage texture, byte[] url)
	{
		if (WrappersManagerImpl.IsPatched(61859))
		{
			WrappersManagerImpl.GetPatch(61859).__Gen_Wrap_1(texture, url);
			return;
		}
		string rawPathFromServer = Encoding.UTF8.GetString(url).TrimEnd('\0');
		DownloadAndSetTexture(texture, CommonTools.GetValidPlayerIconPath(rawPathFromServer), null, null);
	}

	[IDTag(1)]
	public static void DownloadAndSetTexture(UITexture texture, string url, string defaultImage, DownloadPicManager.DownloadTextureCallback callback)
	{
		if (WrappersManagerImpl.IsPatched(61854))
		{
			WrappersManagerImpl.GetPatch(61854).__Gen_Wrap_226(texture, url, defaultImage, callback);
			return;
		}
		DownloadPicReq downloadPicReq = new DownloadPicReq();
		downloadPicReq.mUrl = url;
		downloadPicReq.mTarUITexture = texture;
		if (callback != null)
		{
			downloadPicReq.mCallBack = callback;
		}
		if (defaultImage != null)
		{
			downloadPicReq.defaultImage = defaultImage;
		}
		DownloadPicManager.GetInstance().AddDownloadReq(downloadPicReq);
	}

	[IDTag(0)]
	public static void DownloadAndSetTexture(RawImage texture, string url, string defaultImage, DownloadPicManager.DownloadTextureCallback callback, DownloadPicManager.URLTextureDelegate textureCallback = null, bool needCache = true, bool isActivity = false)
	{
		if (WrappersManagerImpl.IsPatched(2517))
		{
			WrappersManagerImpl.GetPatch(2517).__Gen_Wrap_514(texture, url, defaultImage, callback, textureCallback, needCache, isActivity);
			return;
		}
		DownloadPicReq downloadPicReq = new DownloadPicReq();
		downloadPicReq.mUrl = url;
		downloadPicReq.mRawUIImage = texture;
		downloadPicReq.mNeedCache = needCache;
		downloadPicReq.mIsActivity = isActivity;
		if (textureCallback != null)
		{
			downloadPicReq.delegateURLTexture = textureCallback;
		}
		if (callback != null)
		{
			downloadPicReq.mCallBack = callback;
		}
		if (defaultImage != null)
		{
			downloadPicReq.defaultImage = defaultImage;
		}
		DownloadPicManager.GetInstance().AddDownloadReq(downloadPicReq);
	}

	public static void AppendClickOnButton(Transform tf, EventDelegate.Callback action)
	{
		if (WrappersManagerImpl.IsPatched(49716))
		{
			WrappersManagerImpl.GetPatch(49716).__Gen_Wrap_1(tf, action);
		}
		else
		{
			tf.GetComponent<UIButton>().onClick.Add(new EventDelegate(action));
		}
	}

	public static void AppendClickOnButtonUGUI(Transform tf, EventDelegate.Callback action)
	{
		if (WrappersManagerImpl.IsPatched(49644))
		{
			WrappersManagerImpl.GetPatch(49644).__Gen_Wrap_1(tf, action);
			return;
		}
		EventDelegate.Callback action2 = action;
		UIEventListener.Get(tf.gameObject).onClick = delegate
		{
			action2();
		};
	}

	public static void LoadAndPlayAnimation(Transform transform, string effectUIName, string effectPathName, string effectName, string name, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(58727))
		{
			WrappersManagerImpl.GetPatch(58727).__Gen_Wrap_3872(transform, effectUIName, effectPathName, effectName, name, isMistakeToError);
			return;
		}
		if (transform != null)
		{
			Transform transform2 = transform.Find(effectUIName);
			if (transform2 == null)
			{
				GameObject gameObject = UIUtil.LoadAndInstantiateSync(effectPathName, effectName, transform.gameObject);
				ResetParent(transform, gameObject.transform);
				gameObject.name = effectUIName;
				transform2 = gameObject.transform;
			}
			if (transform2 != null)
			{
				if (string.IsNullOrEmpty(name))
				{
					transform2.gameObject.SetActive(value: true);
					return;
				}
				Animation component = transform2.GetComponent<Animation>();
				if (component != null)
				{
					component.Play(name);
					return;
				}
			}
		}
		if (isMistakeToError)
		{
			Log.Error("PlayAnimator: UI have a mistake", "Framework");
		}
		else
		{
			Log.Info("PlayAnimator: UI have a mistake", "Framework");
		}
	}

	public static void PlayAnimator(Transform trasform, string name, int layer = 0, float normalizedTime = 0f, bool isMistakeToError = true)
	{
		if (WrappersManagerImpl.IsPatched(61860))
		{
			WrappersManagerImpl.GetPatch(61860).__Gen_Wrap_4162(trasform, name, layer, normalizedTime, isMistakeToError);
			return;
		}
		if (trasform != null)
		{
			Animator component = trasform.GetComponent<Animator>();
			if (component != null)
			{
				component.Play(name, layer, normalizedTime);
				return;
			}
		}
		if (isMistakeToError)
		{
			Log.Error("PlayAnimator: UI have a mistake", "Framework");
		}
		else
		{
			Log.Info("PlayAnimator: UI have a mistake", "Framework");
		}
	}

	public static TextMeshProUGUI FindTMPText(GameObject parent, string name)
	{
		if (WrappersManagerImpl.IsPatched(36020))
		{
			return WrappersManagerImpl.GetPatch(36020).__Gen_Wrap_1187(parent, name);
		}
		GameObject gameObject = FindChild(parent, name);
		if (gameObject != null)
		{
			return gameObject.GetComponent<TextMeshProUGUI>();
		}
		return null;
	}

	public static Text FindUGUIText(GameObject parent, string name)
	{
		if (WrappersManagerImpl.IsPatched(47555))
		{
			return WrappersManagerImpl.GetPatch(47555).__Gen_Wrap_1974(parent, name);
		}
		GameObject gameObject = FindChild(parent, name);
		if (gameObject != null)
		{
			return gameObject.GetComponent<Text>();
		}
		return null;
	}

	public static void ShowPanelCloseAnimation(GameObject go, TweenCallback action)
	{
		if (WrappersManagerImpl.IsPatched(5613))
		{
			WrappersManagerImpl.GetPatch(5613).__Gen_Wrap_1(go, action);
		}
		else
		{
			FindChild(go, "GamePanel").GetComponent<DlgDoTweenScale>().PlayDlgCloseAnimation(action);
		}
	}

	public static Image FindUIImage(GameObject parent, string name)
	{
		if (WrappersManagerImpl.IsPatched(36118))
		{
			return WrappersManagerImpl.GetPatch(36118).__Gen_Wrap_2435(parent, name);
		}
		GameObject gameObject = FindChild(parent, name);
		if (gameObject != null)
		{
			return gameObject.GetComponent<Image>();
		}
		return null;
	}

	public static void FillFace(Transform face, string url)
	{
		if (WrappersManagerImpl.IsPatched(44380))
		{
			WrappersManagerImpl.GetPatch(44380).__Gen_Wrap_1(face, url);
		}
		else if (!string.IsNullOrEmpty(url))
		{
			DownloadPicReq downloadPicReq = new DownloadPicReq();
			if (!url.Contains("http") || url.Contains("1http"))
			{
				url = "";
			}
			downloadPicReq.mUrl = CommonTools.GetValidPlayerIconPath(url.Trim('\0'));
			downloadPicReq.mRawUIImage = face.GetComponent<RawImage>();
			downloadPicReq.defaultImage = "default_face";
			DownloadPicManager.GetInstance().AddDownloadReq(downloadPicReq);
		}
	}

	public static void AnchorToCenter(Transform transform, Vector3 position)
	{
		if (WrappersManagerImpl.IsPatched(61861))
		{
			WrappersManagerImpl.GetPatch(61861).__Gen_Wrap_765(transform, position);
			return;
		}
		transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
		transform.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
		transform.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
		transform.GetComponent<RectTransform>().anchoredPosition = position;
	}

	public static void AnchorToRight(Transform transform, Vector3 position)
	{
		if (WrappersManagerImpl.IsPatched(61862))
		{
			WrappersManagerImpl.GetPatch(61862).__Gen_Wrap_765(transform, position);
			return;
		}
		transform.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
		transform.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
		transform.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
		transform.GetComponent<RectTransform>().anchoredPosition = position;
	}

	public static void AnchorToLeft(Transform transform, Vector3 position)
	{
		if (WrappersManagerImpl.IsPatched(61863))
		{
			WrappersManagerImpl.GetPatch(61863).__Gen_Wrap_765(transform, position);
			return;
		}
		transform.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
		transform.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.5f);
		transform.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0.5f);
		transform.GetComponent<RectTransform>().anchoredPosition = position;
	}

	public static void AdapterBackGround(Transform background, float width, float height, bool bIgnoreUIRoot = false)
	{
		if (WrappersManagerImpl.IsPatched(1424))
		{
			WrappersManagerImpl.GetPatch(1424).__Gen_Wrap_317(background, width, height, bIgnoreUIRoot);
			return;
		}
		RectTransform rectTransform = ((!SSRInstituteUtil.isInSSRInstitute) ? SingletonBase<PopUpManager>.GetInstance().GetUIRoot()?.GetComponent<RectTransform>() : (SSRInstituteUtil.GetCanvasTransform() as RectTransform));
		Vector2 vector = GetScreenSize();
		if (!bIgnoreUIRoot && SceneManager.GetActiveScene().name != "LoginScene" && rectTransform != null)
		{
			vector = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		}
		float num = Mathf.Max(vector.x / width, vector.y / height);
		background.transform.localScale = Vector3.one * (num + 0.02f);
	}

	public static void AdapterLoginBackGround(Transform background, float width, float height)
	{
		if (WrappersManagerImpl.IsPatched(48980))
		{
			WrappersManagerImpl.GetPatch(48980).__Gen_Wrap_1106(background, width, height);
			return;
		}
		RectTransform rectTransform = SingletonBase<PopUpManager>.GetInstance().GetUIRoot()?.GetComponent<RectTransform>();
		Vector2 vector = GetScreenSize();
		if (rectTransform != null)
		{
			vector = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
		}
		float num = width * 0.8f;
		Vector2 vector2 = new Vector2(num, num * 0.5625f);
		float num2 = 1f;
		float num3 = 1f * (float)Screen.width / (float)Screen.height;
		num2 = ((!(num3 < 2.2f)) ? Mathf.Max(vector.x / width, vector.y / height) : ((num3 >= 1.777f) ? (vector.y / vector2.y) : ((!(num3 < 1f)) ? (vector.x / vector2.x) : Mathf.Max(vector.x / vector2.x, vector.y / vector2.y))));
		background.transform.localScale = Vector3.one * (num2 + 0.02f);
	}

	private static Vector2 GetScreenSize()
	{
		if (WrappersManagerImpl.IsPatched(1425))
		{
			return WrappersManagerImpl.GetPatch(1425).__Gen_Wrap_316();
		}
		float num = Screen.width;
		float num2 = Screen.height;
		if (num < num2)
		{
			float num3 = num;
			num = num2;
			num2 = num3;
		}
		if (!(num / DesignWidth >= num2 / DesignHeight))
		{
			return new Vector2(DesignWidth, DesignWidth * num2 / num);
		}
		return new Vector2(DesignHeight * num / num2, DesignHeight);
	}

	public static void AddLeftBarIconEffect(string effectName, Transform tsfParent, Action<GameObject> onEffectAdded = null)
	{
		if (WrappersManagerImpl.IsPatched(21138))
		{
			WrappersManagerImpl.GetPatch(21138).__Gen_Wrap_17(effectName, tsfParent, onEffectAdded);
			return;
		}
		string effectName2 = effectName;
		Transform tsfParent2 = tsfParent;
		Action<GameObject> onEffectAdded2 = onEffectAdded;
		bool useLod = false;
		if (Device.GetInstance().OrigType <= DeviceType.LowNormal)
		{
			useLod = true;
		}
		if (!string.IsNullOrEmpty(effectName2))
		{
			if (OperationHandler.m_effectAssetBundle != null)
			{
				GameObject gameObject = null;
				if (useLod)
				{
					string name = string.Format(effectName2 + "_lod_low");
					gameObject = OperationHandler.m_effectAssetBundle.LoadAsset<GameObject>(name);
				}
				if (gameObject == null)
				{
					gameObject = OperationHandler.m_effectAssetBundle.LoadAsset<GameObject>(effectName2);
				}
				if (gameObject != null)
				{
					if (tsfParent2 != null)
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
						gameObject2.SetActive(value: true);
						BumpSortingLayer(gameObject2.transform, 1);
						ResetParent(tsfParent2, gameObject2.transform);
						onEffectAdded2?.Invoke(gameObject2);
					}
					else
					{
						Log.Info("tsfParent is null", "Operation");
					}
				}
				return;
			}
			SingletonBase<ResourcesLoader>.GetInstance().LoadAssetBundle("pufferoperationeffect", delegate(object data2, Message message, AssetBundle resources)
			{
				if (message == Message.success)
				{
					if (resources != null)
					{
						GameObject gameObject3 = null;
						if (useLod)
						{
							string name2 = string.Format(effectName2 + "_lod_low");
							gameObject3 = resources.LoadAsset<GameObject>(name2);
						}
						if (gameObject3 == null)
						{
							gameObject3 = resources.LoadAsset<GameObject>(effectName2);
						}
						if (gameObject3 != null)
						{
							if (tsfParent2 != null)
							{
								GameObject gameObject4 = UnityEngine.Object.Instantiate(gameObject3);
								gameObject4.SetActive(value: true);
								BumpSortingLayer(gameObject4.transform, 1);
								ResetParent(tsfParent2, gameObject4.transform);
								onEffectAdded2?.Invoke(gameObject4);
							}
							else
							{
								Log.Info("tsfParent is null", "Operation");
							}
						}
						OperationHandler.m_effectAssetBundle = resources;
					}
					else
					{
						Log.Info("resources is null", "Operation");
					}
				}
				else
				{
					Log.Info("pufferoperationeffect load fail", "Operation");
				}
			}, false, EnmPriority.Normal, EnmResTag.Main);
		}
		else
		{
			Log.Info("effectName is null or Empty", "Operation");
		}
	}

	public static Transform GetCameraPos(UserGender gender)
	{
		if (WrappersManagerImpl.IsPatched(41598))
		{
			return WrappersManagerImpl.GetPatch(41598).__Gen_Wrap_2802(gender);
		}
		GameObject cameraPosObj = SingletonBase<MainUIStateCenter>.GetInstance().GetCameraPosObj();
		if (cameraPosObj == null)
		{
			return null;
		}
		_ = 1;
		return cameraPosObj.transform.Find("CameraPosBoy");
	}

	public static void AdjustCamera(Transform camera, float scale = -1f)
	{
		if (WrappersManagerImpl.IsPatched(41560))
		{
			WrappersManagerImpl.GetPatch(41560).__Gen_Wrap_28(camera, scale);
		}
		else
		{
			if (camera == null || camera.parent == null)
			{
				return;
			}
			Transform transform = camera.parent.Find("MainPos_near");
			Transform transform2 = camera.parent.Find("MainPos_far");
			if (transform == null || transform2 == null)
			{
				return;
			}
			if (scale < 0f)
			{
				if (_current_LobbyCameraScale < 0f)
				{
					scale = SingletonBase<FazaiFactory>.GetInstance().GetCurrentSceneScale();
					TwoTouchScaleComponent scaleComponent = GetScaleComponent();
					if (scaleComponent != null)
					{
						scaleComponent.SetScaleInfo(0f, 1f, scale);
					}
				}
				else
				{
					scale = _current_LobbyCameraScale;
				}
			}
			_current_LobbyCameraScale = scale;
			Vector3 farLocal = transform2.localPosition;
			Vector3 nearLocal = transform.localPosition;
			CustomSettingFarNearOffset(ref farLocal, ref nearLocal);
			Vector3 localPosition = farLocal - (farLocal - nearLocal) * scale;
			camera.localPosition = localPosition;
			if (Screen.height > 0 && (double)((float)Screen.width / (float)Screen.height) < 1.4)
			{
				camera.localPosition += Camera_Pad_Shift;
			}
			camera.rotation = transform2.rotation;
		}
	}

	private static void CustomSettingFarNearOffset(ref Vector3 farLocal, ref Vector3 nearLocal)
	{
		if (WrappersManagerImpl.IsPatched(41563))
		{
			WrappersManagerImpl.GetPatch(41563).__Gen_Wrap_2799(ref farLocal, ref nearLocal);
			return;
		}
		float num = SingletonBase<DynamicConfig>.GetInstance().GetFloat(SceneAvatarCamSettingKey, "FarOffsetX", 37f);
		float num2 = SingletonBase<DynamicConfig>.GetInstance().GetFloat(SceneAvatarCamSettingKey, "FarOffsetZ");
		if (num != 0f)
		{
			farLocal.x += num;
		}
		if (num2 != 0f)
		{
			farLocal.z += num2;
		}
		float num3 = SingletonBase<DynamicConfig>.GetInstance().GetFloat(SceneAvatarCamSettingKey, "NearOffsetX", 26f);
		float num4 = SingletonBase<DynamicConfig>.GetInstance().GetFloat(SceneAvatarCamSettingKey, "NearOffsetZ");
		if (num3 != 0f)
		{
			nearLocal.x += num3;
		}
		if (num4 != 0f)
		{
			nearLocal.z += num3;
		}
	}

	public static TwoTouchScaleComponent GetScaleComponent()
	{
		if (WrappersManagerImpl.IsPatched(41562))
		{
			return WrappersManagerImpl.GetPatch(41562).__Gen_Wrap_2798();
		}
		GameObject uIRoot = SingletonBase<PopUpManager>.GetInstance().GetUIRoot();
		if (uIRoot == null)
		{
			return null;
		}
		if (uIRoot.transform.Find("MainUI") == null)
		{
			return null;
		}
		Transform transform = uIRoot.transform.Find("AvatarRotate");
		if (transform == null)
		{
			return null;
		}
		return transform.gameObject.GetComponent<TwoTouchScaleComponent>();
	}

	public static float GetLobbyCameraScale()
	{
		if (WrappersManagerImpl.IsPatched(1344))
		{
			return WrappersManagerImpl.GetPatch(1344).__Gen_Wrap_27();
		}
		float result = 0f;
		object obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("LobbyCameraScale_784");
		if (obj != null)
		{
			result = Convert.ToSingle(obj);
		}
		return result;
	}

	public static void SetLobbyCameraScale(float scale)
	{
		if (WrappersManagerImpl.IsPatched(41586))
		{
			WrappersManagerImpl.GetPatch(41586).__Gen_Wrap_193(scale);
		}
		else
		{
			SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("LobbyCameraScale_784", scale, updateSvr: true);
		}
	}

	public static Transform GetPhotoCameraPos(UserGender gender)
	{
		if (WrappersManagerImpl.IsPatched(61864))
		{
			return WrappersManagerImpl.GetPatch(61864).__Gen_Wrap_2802(gender);
		}
		if (gender == UserGender.Male)
		{
			return GameObject.Find("CameraPos").transform.Find("CameraPosBoy").transform;
		}
		return GameObject.Find("CameraPos").transform.Find("CameraPosGirl").transform;
	}

	public static Transform GetSceneAvatarPos(UserGender gender)
	{
		if (WrappersManagerImpl.IsPatched(61865))
		{
			return WrappersManagerImpl.GetPatch(61865).__Gen_Wrap_2802(gender);
		}
		if (gender == UserGender.Male)
		{
			return GameObject.Find("SceneAvatarPos/MainPos/Boy").transform;
		}
		return GameObject.Find("SceneAvatarPos/MainPos/Girl").transform;
	}

	public static void SetUITextFont(GameObject gameObject, string abName, string fontName)
	{
		if (WrappersManagerImpl.IsPatched(61866))
		{
			WrappersManagerImpl.GetPatch(61866).__Gen_Wrap_17(gameObject, abName, fontName);
			return;
		}
		Font font = SingletonBase<ResourcesLoader>.GetInstance().Load<Font>(abName, fontName, ".TTF");
		Text component = gameObject.GetComponent<Text>();
		if (component != null)
		{
			component.font = font;
		}
	}

	public static void SetUITextTMProFont(GameObject gameObject, string abName, string fontName)
	{
		if (WrappersManagerImpl.IsPatched(61867))
		{
			WrappersManagerImpl.GetPatch(61867).__Gen_Wrap_17(gameObject, abName, fontName);
			return;
		}
		TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
		if (component != null)
		{
			TMP_FontAsset font = SingletonBase<ResourcesLoader>.GetInstance().Load<TMP_FontAsset>(abName, fontName, ".asset");
			if (component != null)
			{
				component.font = font;
			}
		}
	}

	public static T GetComponentInParent<T>(Transform transform) where T : Component
	{
		if (transform == null)
		{
			return null;
		}
		T val = null;
		for (int i = 0; i < 100; i++)
		{
			val = transform.GetComponent<T>();
			if (val != null)
			{
				break;
			}
			transform = transform.parent;
			if (transform == null)
			{
				break;
			}
		}
		return val;
	}

	public static void SetNickForBP(Text labelName, BPNickNameColorType colorType)
	{
		if (WrappersManagerImpl.IsPatched(35932))
		{
			WrappersManagerImpl.GetPatch(35932).__Gen_Wrap_2398(labelName, colorType);
		}
		else if (colorType != BPNickNameColorType.None && (bool)labelName)
		{
			labelName.color = Color.white;
			switch (colorType)
			{
			case BPNickNameColorType.Colorful:
				RankedBattlePassUtil.ApplyBPColorfulNickName(labelName.transform, needOutline: false);
				break;
			case BPNickNameColorType.Orange:
			{
				RankedBattlePassUtil.DisableBPColorfulNickName(labelName.transform);
				UnityEngine.UI.Extensions.Gradient orAddComponent = labelName.GetOrAddComponent<UnityEngine.UI.Extensions.Gradient>();
				ColorUtility.TryParseHtmlString("#FF5E00FF", out var color);
				ColorUtility.TryParseHtmlString("#FFAE00FF", out var color2);
				orAddComponent.Vertex1 = color;
				orAddComponent.Vertex2 = color2;
				orAddComponent.GradientMode = UnityEngine.UI.Extensions.GradientMode.Local;
				orAddComponent.enabled = true;
				break;
			}
			}
		}
	}

	public static void SetNickForBPTMP(TMP_Text labelName, BPNickNameColorType colorType)
	{
		if (WrappersManagerImpl.IsPatched(61868))
		{
			WrappersManagerImpl.GetPatch(61868).__Gen_Wrap_2398(labelName, colorType);
		}
		else if (colorType != BPNickNameColorType.None && (bool)labelName)
		{
			labelName.color = Color.white;
			switch (colorType)
			{
			case BPNickNameColorType.Colorful:
				labelName.colorGradient = new VertexGradient(Color.white);
				labelName.enableVertexGradient = false;
				RankedBattlePassUtil.ApplyBPColorfulNickNameTMP(labelName.transform, needOutline: false);
				break;
			case BPNickNameColorType.Orange:
			{
				RankedBattlePassUtil.DisableBPColorfulNickName(labelName.transform);
				ColorUtility.TryParseHtmlString("#FF5E00FF", out var color);
				ColorUtility.TryParseHtmlString("#FFAE00FF", out var color2);
				VertexGradient colorGradient = default(VertexGradient);
				colorGradient.topLeft = (colorGradient.topRight = color);
				colorGradient.bottomLeft = (colorGradient.bottomRight = color2);
				labelName.colorGradient = colorGradient;
				labelName.enableVertexGradient = true;
				break;
			}
			}
		}
		else
		{
			RankedBattlePassUtil.DisableBPColorfulNickName(labelName.transform);
			labelName.color = Color.white;
			labelName.colorGradient = new VertexGradient(Color.white);
			labelName.enableVertexGradient = false;
		}
	}

	public static void CheckAndSetNickForBP(Text labelName)
	{
		if (WrappersManagerImpl.IsPatched(57119))
		{
			WrappersManagerImpl.GetPatch(57119).__Gen_Wrap_0(labelName);
		}
		else
		{
			SetNickForBP(labelName, SingletonBase<ShopDataHelper>.GetInstance().GetSelfBPNickNameColorType());
		}
	}

	public static void SetMyNickForBPWithOutLine(Transform tsfNickname, bool needOutline = true)
	{
		if (WrappersManagerImpl.IsPatched(5296))
		{
			WrappersManagerImpl.GetPatch(5296).__Gen_Wrap_8(tsfNickname, needOutline);
		}
		else
		{
			ApplyNickForBPWithOutLine(SingletonBase<ShopDataHelper>.GetInstance().GetSelfBPNickNameColorType(), tsfNickname, needOutline);
		}
	}

	public static void SetOthersNickForBPWithOutLine(List<int> items, Transform tsfNickname, bool needOutline = true)
	{
		if (WrappersManagerImpl.IsPatched(61869))
		{
			WrappersManagerImpl.GetPatch(61869).__Gen_Wrap_80(items, tsfNickname, needOutline);
		}
		else
		{
			ApplyNickForBPWithOutLine(SingletonBase<ShopDataHelper>.GetInstance().GetBPNickNameColorType(items), tsfNickname, needOutline);
		}
	}

	public static void ApplyNickForBPWithOutLine(BPNickNameColorType type, Transform tsfNickname, bool needOutline = true)
	{
		if (WrappersManagerImpl.IsPatched(5297))
		{
			WrappersManagerImpl.GetPatch(5297).__Gen_Wrap_861(type, tsfNickname, needOutline);
			return;
		}
		switch (type)
		{
		case BPNickNameColorType.Colorful:
			HandleBPNickNameOrange(active: false, tsfNickname, needOutline);
			RankedBattlePassUtil.ApplyBPColorfulNickName(tsfNickname, needOutline);
			break;
		case BPNickNameColorType.Orange:
			RankedBattlePassUtil.DisableBPColorfulNickName(tsfNickname);
			HandleBPNickNameOrange(active: true, tsfNickname, needOutline);
			break;
		default:
			RankedBattlePassUtil.DisableBPColorfulNickName(tsfNickname);
			HandleBPNickNameOrange(active: false, tsfNickname, needOutline);
			break;
		}
	}

	private static void HandleBPNickNameOrange(bool active, Transform tsfNickname, bool needOutline = true)
	{
		if (WrappersManagerImpl.IsPatched(5298))
		{
			WrappersManagerImpl.GetPatch(5298).__Gen_Wrap_860(active, tsfNickname, needOutline);
			return;
		}
		Text component = tsfNickname.GetComponent<Text>();
		Color color = component.color;
		color.a = 1f;
		component.color = color;
		if (active)
		{
			ColorUtility.TryParseHtmlString("#FFF356FF", out var color2);
			ColorUtility.TryParseHtmlString("#FCFFE5FF", out var color3);
			UnityEngine.UI.Extensions.Gradient orAddComponent = tsfNickname.GetOrAddComponent<UnityEngine.UI.Extensions.Gradient>();
			orAddComponent.GradientMode = UnityEngine.UI.Extensions.GradientMode.Local;
			orAddComponent.Vertex1 = color2;
			orAddComponent.Vertex2 = color3;
			if (needOutline)
			{
				tsfNickname.GetOrAddComponent<Outline>().enabled = true;
			}
		}
		else
		{
			UnityEngine.UI.Extensions.Gradient orAddComponent2 = tsfNickname.GetOrAddComponent<UnityEngine.UI.Extensions.Gradient>();
			orAddComponent2.GradientMode = UnityEngine.UI.Extensions.GradientMode.Local;
			orAddComponent2.Vertex1 = Color.white;
			orAddComponent2.Vertex2 = Color.white;
			if (needOutline)
			{
				tsfNickname.GetOrAddComponent<Outline>().enabled = false;
			}
		}
	}

	public static void AdapterWidth(Transform item, float scale = 1f, bool right = true)
	{
		if (WrappersManagerImpl.IsPatched(21192))
		{
			WrappersManagerImpl.GetPatch(21192).__Gen_Wrap_1113(item, scale, right);
			return;
		}
		float num = (GetScreenSize().x - 1334f) * 0.5f * scale;
		int num2 = (right ? 1 : (-1));
		item.localPosition = new Vector3(item.localPosition.x - (float)num2 * num, item.localPosition.y, item.localPosition.z);
	}

	public static GameObject GetSceneRoot()
	{
		if (WrappersManagerImpl.IsPatched(35205))
		{
			return WrappersManagerImpl.GetPatch(35205).__Gen_Wrap_311();
		}
		GameObject result = null;
		if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.Main || SceneSwitch.CurrentScene() == SceneSwitch.Scene.Shop)
		{
			result = GameObject.Find("SelectionSceneRoot");
		}
		else if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.Game)
		{
			result = GameSceneNodeFinder.GetSceneRootObj();
		}
		else if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.Login)
		{
			result = GameObject.Find("LoginSceneRoot");
		}
		else if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.ChoiceSex)
		{
			result = GameObject.Find("ChoiceSex_UI");
		}
		else if (SceneSwitch.CurrentScene() == SceneSwitch.Scene.Loading)
		{
			result = GameObject.Find("LoadingSceneRoot");
		}
		return result;
	}

	public static int GetMJType(int mjValue)
	{
		if (WrappersManagerImpl.IsPatched(61872))
		{
			return WrappersManagerImpl.GetPatch(61872).__Gen_Wrap_144(mjValue);
		}
		return mjValue / 10;
	}

	public static int GetMJSubValue(int mjValue)
	{
		if (WrappersManagerImpl.IsPatched(61873))
		{
			return WrappersManagerImpl.GetPatch(61873).__Gen_Wrap_144(mjValue);
		}
		return mjValue % 10;
	}

	public static string GetUTF8String(byte[] buffer)
	{
		if (WrappersManagerImpl.IsPatched(61874))
		{
			return WrappersManagerImpl.GetPatch(61874).__Gen_Wrap_19(buffer);
		}
		if (buffer == null)
		{
			return string.Empty;
		}
		if (buffer.Length <= 3)
		{
			return Encoding.UTF8.GetString(buffer);
		}
		byte[] array = new byte[3] { 239, 187, 191 };
		if (buffer[0] == array[0] && buffer[1] == array[1] && buffer[2] == array[2])
		{
			return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetString(buffer, 3, buffer.Length - 3);
		}
		return Encoding.UTF8.GetString(buffer);
	}

	public static string GetSignedString(int num)
	{
		if (WrappersManagerImpl.IsPatched(61875))
		{
			return WrappersManagerImpl.GetPatch(61875).__Gen_Wrap_12(num);
		}
		if (num > 0)
		{
			return $"+{num}";
		}
		if (num < 0)
		{
			return $"-{-num}";
		}
		return num.ToString();
	}

	public static void ModifyChildRenderLayer(GameObject obj, string sortingLayer = "", int sortingOrder = -100)
	{
		if (WrappersManagerImpl.IsPatched(474))
		{
			WrappersManagerImpl.GetPatch(474).__Gen_Wrap_159(obj, sortingLayer, sortingOrder);
			return;
		}
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!sortingLayer.IsNullOrEmpty())
			{
				renderer.sortingLayerName = sortingLayer;
			}
			if (sortingOrder != -100)
			{
				renderer.sortingOrder = sortingOrder;
			}
		}
		Canvas[] componentsInChildren2 = obj.GetComponentsInChildren<Canvas>(includeInactive: true);
		foreach (Canvas canvas in componentsInChildren2)
		{
			if (!sortingLayer.IsNullOrEmpty())
			{
				canvas.overrideSorting = true;
				canvas.sortingLayerName = sortingLayer;
			}
			if (sortingOrder != -100)
			{
				canvas.sortingOrder = sortingOrder;
			}
		}
	}

	public static int CountCodePoints(string str)
	{
		if (WrappersManagerImpl.IsPatched(2499))
		{
			return WrappersManagerImpl.GetPatch(2499).__Gen_Wrap_85(str);
		}
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (char.IsLowSurrogate(str[i]))
			{
				i += 2;
			}
			num++;
		}
		return num;
	}

	public static int GetStrLength(string str)
	{
		if (WrappersManagerImpl.IsPatched(36012))
		{
			return WrappersManagerImpl.GetPatch(36012).__Gen_Wrap_85(str);
		}
		string text = str.TrimEnd('\0');
		Regex regex = new Regex("\\p{IsCJKUnifiedIdeographs}");
		int num = CountCodePoints(text);
		return regex.Matches(text).Count + num;
	}

	public static string CutNameStr(int maxLength, string str)
	{
		if (WrappersManagerImpl.IsPatched(2498))
		{
			return WrappersManagerImpl.GetPatch(2498).__Gen_Wrap_508(maxLength, str);
		}
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		string text = str.TrimEnd('\0');
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		Regex regex = new Regex("\\p{IsCJKUnifiedIdeographs}");
		int num = CountCodePoints(text);
		if (regex.Matches(text).Count + num <= maxLength)
		{
			return text;
		}
		for (int num2 = text.Length; num2 > 0; num2--)
		{
			if (!char.IsHighSurrogate(text[num2 - 1]))
			{
				text = text.Substring(0, num2);
				if (regex.Matches(text).Count + num <= maxLength - 1)
				{
					return text + "…";
				}
				num--;
			}
		}
		return "…";
	}

	public static string GetLanguageStrByKey(string strID, List<object> otherParam = null)
	{
		if (WrappersManagerImpl.IsPatched(1158))
		{
			return WrappersManagerImpl.GetPatch(1158).__Gen_Wrap_52(strID, otherParam);
		}
		string empty = string.Empty;
		if (m_languageCache.ContainsKey(strID))
		{
			empty = m_languageCache[strID];
		}
		else
		{
			FieldInfo field = typeof(LanguageKey).GetField(strID);
			if (null == field)
			{
				Log.Error("Util.GetLanguageStrByKey don't have this ID " + strID, "Config");
				return null;
			}
			empty = field.GetValue(null).ToString();
			m_languageCache.Add(strID, empty);
		}
		Log.Info($"Util.GetLanguageStrByKey GetStr, Key = {strID}, Str = {empty}", "Config");
		if (otherParam != null && otherParam.Count != 0)
		{
			return string.Format(empty, otherParam.ToArray());
		}
		return empty;
	}

	public static bool CheckIDNumCorrect(string num)
	{
		if (WrappersManagerImpl.IsPatched(51918))
		{
			return WrappersManagerImpl.GetPatch(51918).__Gen_Wrap_3(num);
		}
		if (num.Length != 18)
		{
			return false;
		}
		if (CheckAllNum(num.Substring(0, 17)))
		{
			if ((num[17] >= '0' && num[17] <= '9') || num[17] == 'X' || num[17] == 'x')
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool CheckPhoneNum(string num)
	{
		if (WrappersManagerImpl.IsPatched(51919))
		{
			return WrappersManagerImpl.GetPatch(51919).__Gen_Wrap_3(num);
		}
		if (num.Length != 11)
		{
			return false;
		}
		return CheckAllNum(num);
	}

	public static bool CheckPostCode(string num)
	{
		if (WrappersManagerImpl.IsPatched(61876))
		{
			return WrappersManagerImpl.GetPatch(61876).__Gen_Wrap_3(num);
		}
		if (num.Length != 6)
		{
			return false;
		}
		return CheckAllNum(num);
	}

	public static bool CheckQQNum(string str)
	{
		if (WrappersManagerImpl.IsPatched(51920))
		{
			return WrappersManagerImpl.GetPatch(51920).__Gen_Wrap_3(str);
		}
		if (str.Length >= 5 && CheckAllNum(str))
		{
			return true;
		}
		return false;
	}

	public static bool CheckAllNum(string str)
	{
		if (WrappersManagerImpl.IsPatched(3788))
		{
			return WrappersManagerImpl.GetPatch(3788).__Gen_Wrap_3(str);
		}
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] > '9' || str[i] < '0')
			{
				return false;
			}
		}
		return true;
	}

	public static string SignatureURL(string url, bool needAppendGameid = true)
	{
		if (WrappersManagerImpl.IsPatched(21194))
		{
			return WrappersManagerImpl.GetPatch(21194).__Gen_Wrap_20(url, needAppendGameid);
		}
		if (url.Contains("?"))
		{
			return SignatureURLWithFormat(url);
		}
		return SignatureURLNoFormat(url, needAppendGameid);
	}

	public static string SignatureURLNoFormat(string url, bool needAppendGameid = true)
	{
		if (WrappersManagerImpl.IsPatched(21199))
		{
			return WrappersManagerImpl.GetPatch(21199).__Gen_Wrap_20(url, needAppendGameid);
		}
		Log.Info("Url before SignatureURLNoFormat, the url is: " + url, "Activity");
		string text = url;
		text = text + "?uin=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin();
		text = text + "&accounttype=" + (int)SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType();
		text = ((SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() != LoginType.LoginByWeiXin) ? (text + "&acctype=qq") : (text + "&acctype=wx"));
		text = text + "&clienttype=" + ClientType();
		if (needAppendGameid)
		{
			text += "&gameid=398";
		}
		text = text + "&channelid=" + GetChannelId();
		text = text.Trim('\0');
		text = text + "&systemversion=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAppVersion();
		text = ((SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() != LoginType.LoginByWeiXin && SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() != LoginType.LoginByGuest) ? (text + "&zoneid=2") : (text + "&zoneid=1"));
		text = ((!IsAndroidPlatform() && !IsIOSPlatform() && !IsOpenHarmonyPlatform()) ? (text + "&appid=1101070761") : ((SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByWeiXin) ? (text + "&appid=wx3bef52104e238bff") : ((SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() != LoginType.LoginByQQ) ? (text + "&appid=G_1101070761") : (text + "&appid=1101070761"))));
		if (IsPCPlatform())
		{
			text = text + "&openid=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetOpenId();
			text = text + "&pf=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().pf;
			text = text + "&pfkey=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().pfKey;
			text = text + "&accesstoken=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAccessToken();
			text = text + "&access_token=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAccessToken();
		}
		else if (IsAndroidPlatform() || IsIOSPlatform() || IsOpenHarmonyPlatform())
		{
			MSDKLoginRet mSDKLoginInfo = GetMSDKLoginInfo();
			if (mSDKLoginInfo != null && mSDKLoginInfo.RetCode == 0)
			{
				text = text + "&openid=" + mSDKLoginInfo.OpenId;
				text = text + "&pf=" + mSDKLoginInfo.Pf;
				text = text + "&pfkey=" + mSDKLoginInfo.PfKey;
				text = text + "&accesstoken=" + mSDKLoginInfo.Token;
				text = text + "&access_token=" + mSDKLoginInfo.Token;
				if (mSDKLoginInfo.Channel == "WeChat" || mSDKLoginInfo.Channel == "Guest")
				{
					text = text + "&paytoken=" + GetPayToken(mSDKLoginInfo);
				}
				if (mSDKLoginInfo.Channel == "QQ")
				{
					text = text + "&paytoken=" + GetPayToken(mSDKLoginInfo);
				}
			}
			else if (mSDKLoginInfo == null)
			{
				Log.Info("MSDKLoginRet is null", "Activity");
			}
			else
			{
				Log.Info($"MSDKLoginRet code {mSDKLoginInfo.RetCode}, {mSDKLoginInfo.ToString()}", "Activity");
			}
		}
		if (!string.IsNullOrEmpty(SingletonBase<PlayerLoginDataMgr>.GetInstance().GetWebSkey()))
		{
			text = text + "&webskey=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetWebSkey();
		}
		Log.Info("Url SignatureURLNoFormat, the url is: " + text, "Activity");
		return text;
	}

	private static bool IsUnityEditorOrStandaloneWin()
	{
		if (WrappersManagerImpl.IsPatched(21197))
		{
			return WrappersManagerImpl.GetPatch(21197).__Gen_Wrap_11();
		}
		return false;
	}

	public static int ClientType()
	{
		if (WrappersManagerImpl.IsPatched(21196))
		{
			return WrappersManagerImpl.GetPatch(21196).__Gen_Wrap_101();
		}
		if (IsIOSPlatform())
		{
			return 13;
		}
		if (IsOpenHarmonyPlatform())
		{
			if (IsOpenHarmony2In1Device())
			{
				return 34;
			}
			return 70;
		}
		if (IsAndroidPlatform())
		{
			if (SingletonBase<CloudGameController>.GetInstance().IsFakeIOS())
			{
				return 13;
			}
			return 64;
		}
		if (IsPCPlatform())
		{
			return 32;
		}
		if (IsUnityEditorOrStandaloneWin())
		{
			return 64;
		}
		return 32;
	}

	public static string SignatureURLWithFormat(string url)
	{
		if (WrappersManagerImpl.IsPatched(21195))
		{
			return WrappersManagerImpl.GetPatch(21195).__Gen_Wrap_19(url);
		}
		string text = url;
		if (text.Contains("$ACCOUNTTYPE$"))
		{
			text = text.Replace("$ACCOUNTTYPE$", ((int)SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType()).ToString());
		}
		if (text.Contains("$UIN$"))
		{
			text = text.Replace("$UIN$", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin().ToString());
		}
		if (text.Contains("$CHANNELID$"))
		{
			text = text.Replace("$CHANNELID$", GetChannelId().ToString());
			text = text.Trim('\0');
		}
		if (text.Contains("$ZONEID$"))
		{
			text = ((SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() != LoginType.LoginByWeiXin) ? text.Replace("$ZONEID$", "2") : text.Replace("$ZONEID$", "1"));
		}
		if (text.Contains("$CLIENTTYPE$"))
		{
			text = text.Replace("$CLIENTTYPE$", ClientType().ToString());
		}
		if (text.Contains("$CLIENTVERSION$"))
		{
			text = text.Replace("$CLIENTVERSION$", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAppVersion());
		}
		if (text.Contains("$SYSTEMVERSION$"))
		{
			text = text.Replace("$SYSTEMVERSION$", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAppVersion());
		}
		if (text.Contains("$GAMEID$"))
		{
			text = text.Replace("$GAMEID$", "398");
		}
		if (IsPCPlatform())
		{
			if (text.Contains("$OPENID$"))
			{
				text = text.Replace("$OPENID$", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetOpenId());
			}
			if (text.Contains("$PF$"))
			{
				text = text.Replace("$PF$", SingletonBase<PlayerLoginDataMgr>.GetInstance().pf);
			}
			if (text.Contains("$PFKEY$"))
			{
				text = text.Replace("$PFKEY$", SingletonBase<PlayerLoginDataMgr>.GetInstance().pfKey);
			}
			if (text.Contains("$PAYTOKEN$"))
			{
				text = text.Replace("$PAYTOKEN$", SingletonBase<PlayerLoginDataMgr>.GetInstance().payToken);
			}
			if (text.Contains("$OPENKEY$"))
			{
				text = text.Replace("$OPENKEY$", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAccessToken());
			}
		}
		else
		{
			MSDKLoginRet mSDKLoginInfo = GetMSDKLoginInfo();
			string payToken = GetPayToken(mSDKLoginInfo);
			if (mSDKLoginInfo != null && mSDKLoginInfo.RetCode == 0)
			{
				if (text.Contains("$OPENID$"))
				{
					text = text.Replace("$OPENID$", mSDKLoginInfo.OpenId);
				}
				if (text.Contains("$PF$"))
				{
					text = text.Replace("$PF$", mSDKLoginInfo.Pf);
				}
				if (text.Contains("$PFKEY$"))
				{
					text = text.Replace("$PFKEY$", mSDKLoginInfo.PfKey);
				}
				if (text.Contains("$PAYTOKEN$"))
				{
					if (mSDKLoginInfo.Channel == "WeChat" || mSDKLoginInfo.Channel == "Guest")
					{
						text = text.Replace("$PAYTOKEN$", payToken);
					}
					else if (mSDKLoginInfo.Channel == "QQ")
					{
						text = text.Replace("$PAYTOKEN$", payToken);
					}
				}
				if (text.Contains("$OPENKEY$"))
				{
					text = text.Replace("$OPENKEY$", payToken);
				}
			}
		}
		Log.Info("Url SignatureURLWithFormat, the url is: " + text, "Activity");
		return text;
	}

	public static string GetMsdkEncodeParam()
	{
		if (WrappersManagerImpl.IsPatched(61877))
		{
			return WrappersManagerImpl.GetPatch(61877).__Gen_Wrap_51();
		}
		string text = "";
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByWeiXin)
		{
			text += "acctype=wx";
			text += "&appid=wx3bef52104e238bff";
		}
		else if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByQQ)
		{
			text += "acctype=qq";
			text += "&appid=1101070761";
		}
		else
		{
			text += "acctype=wx";
			text += "&appid=G_1101070761";
		}
		if (IsPCPlatform())
		{
			text = text + "&openid=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetOpenId();
			text = text + "&access_token=" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAccessToken();
		}
		else if (IsAndroidPlatform() || IsIOSPlatform())
		{
			MSDKLoginRet loginRet = MSDKLogin.GetLoginRet();
			if (loginRet.RetCode == 0)
			{
				text = text + "&openid=" + loginRet.OpenId;
				text = text + "&access_token=" + loginRet.Token;
			}
		}
		if (IsIOSPlatform())
		{
			text += "&platid=0";
		}
		else if (IsAndroidPlatform())
		{
			text += "&platid=1";
		}
		if (IsAndroidPlatform() || IsIOSPlatform())
		{
			return MSDKWebView.GetEncodeUrl(text);
		}
		return UrlEncode(text);
	}

	public static string GetPayToken(MSDKLoginRet loginRet)
	{
		if (WrappersManagerImpl.IsPatched(21198))
		{
			return WrappersManagerImpl.GetPatch(21198).__Gen_Wrap_19(loginRet);
		}
		string result = string.Empty;
		if (loginRet.Channel == "WeChat")
		{
			result = loginRet.Token;
		}
		else if (loginRet.Channel == "QQ")
		{
			result = GetPayTokenByChannelInfo(loginRet.ChannelInfo);
		}
		else if (loginRet.Channel == "Guest")
		{
			result = "pfKey";
		}
		return result;
	}

	public static string GetPayTokenByChannelInfo(string channInfo)
	{
		if (WrappersManagerImpl.IsPatched(58))
		{
			return WrappersManagerImpl.GetPatch(58).__Gen_Wrap_19(channInfo);
		}
		string result = string.Empty;
		Dictionary<string, object> dictionary = Json.Deserialize(channInfo) as Dictionary<string, object>;
		if (dictionary.ContainsKey("pay_token"))
		{
			result = dictionary["pay_token"] as string;
		}
		return result;
	}

	public static string GuestName()
	{
		if (WrappersManagerImpl.IsPatched(53594))
		{
			return WrappersManagerImpl.GetPatch(53594).__Gen_Wrap_51();
		}
		return "v" + SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin();
	}

	public static void WidgetDoAlphaTween(UIWidget widget, float targetAlpha, float duration)
	{
		if (WrappersManagerImpl.IsPatched(61878))
		{
			WrappersManagerImpl.GetPatch(61878).__Gen_Wrap_1106(widget, targetAlpha, duration);
			return;
		}
		TweenAlpha tweenAlpha = widget.gameObject.AddComponent<TweenAlpha>();
		tweenAlpha.from = widget.color.a;
		tweenAlpha.to = targetAlpha;
		tweenAlpha.duration = duration;
		tweenAlpha.PlayForward();
	}

	public static CompressData DeCompress(byte[] buffer)
	{
		if (WrappersManagerImpl.IsPatched(61879))
		{
			return WrappersManagerImpl.GetPatch(61879).__Gen_Wrap_4164(buffer);
		}
		return new CompressionManager().DeCompress(buffer, buffer.Length);
	}

	public static string UrlEncode(string str)
	{
		if (WrappersManagerImpl.IsPatched(51873))
		{
			return WrappersManagerImpl.GetPatch(51873).__Gen_Wrap_19(str);
		}
		char[] array = new char[10] { '$', '&', '+', ',', '/', ':', ';', '=', '?', '@' };
		for (int i = 0; i < array.Length; i++)
		{
			str = str.Replace(array[i].ToString(), "%" + Convert.ToByte(array[i]).ToString("x2"));
		}
		return str;
	}

	public static string UrlEncode2(string str)
	{
		if (WrappersManagerImpl.IsPatched(40862))
		{
			return WrappersManagerImpl.GetPatch(40862).__Gen_Wrap_19(str);
		}
		return WebUtility.UrlEncode(str);
	}

	[IDTag(0)]
	public static string ExpandNumStr(string numStr, int digit)
	{
		if (WrappersManagerImpl.IsPatched(52955))
		{
			return WrappersManagerImpl.GetPatch(52955).__Gen_Wrap_76(numStr, digit);
		}
		if (digit < numStr.Length)
		{
			return numStr.Substring(0, digit);
		}
		int num = digit - numStr.Length;
		for (int i = 0; i < num; i++)
		{
			numStr = numStr.Insert(0, "0");
		}
		return numStr;
	}

	[IDTag(1)]
	public static string ExpandNumStr(int num, int digit)
	{
		if (WrappersManagerImpl.IsPatched(53027))
		{
			return WrappersManagerImpl.GetPatch(53027).__Gen_Wrap_175(num, digit);
		}
		return ExpandNumStr(num.ToString(), digit);
	}

	[IDTag(0)]
	public static sbyte[] ByteArrayToSbyteArray(byte[] array)
	{
		if (WrappersManagerImpl.IsPatched(372))
		{
			return WrappersManagerImpl.GetPatch(372).__Gen_Wrap_130(array);
		}
		if (array == null)
		{
			return null;
		}
		sbyte[] array2 = new sbyte[array.Length];
		Buffer.BlockCopy(array, 0, array2, 0, array.Length);
		return array2;
	}

	public static byte[] SbyteArrayToByteArray(sbyte[] array, int length = -1)
	{
		if (WrappersManagerImpl.IsPatched(2057))
		{
			return WrappersManagerImpl.GetPatch(2057).__Gen_Wrap_445(array, length);
		}
		return HappyMahjong.Core.Common.Util.SbyteArrayToByteArray(array, length);
	}

	[IDTag(1)]
	public static void ByteArrayToSbyteArray(byte[] byteArray, sbyte[] sbyteArray)
	{
		if (WrappersManagerImpl.IsPatched(39989))
		{
			WrappersManagerImpl.GetPatch(39989).__Gen_Wrap_1(byteArray, sbyteArray);
		}
		else
		{
			HappyMahjong.Core.Common.Util.ByteArrayToSbyteArray(byteArray, sbyteArray);
		}
	}

	public static int SendGRPC(string strMethodName, byte[] data, int msgLen, string metaStr = "", string routeKey = "")
	{
		if (WrappersManagerImpl.IsPatched(1781))
		{
			return WrappersManagerImpl.GetPatch(1781).__Gen_Wrap_399(strMethodName, data, msgLen, metaStr, routeKey);
		}
		return HappyMahjong.Core.Common.Util.SendGRPC(strMethodName, data, msgLen, metaStr, routeKey);
	}

	public static void SendProtoBufferMsg<T>(T pbData, short Msgid)
	{
		HappyMahjong.Core.Common.Util.SendProtoBufferMsg(pbData, Msgid);
	}

	public static void SendGoogleProtoBufferMsg<T>(T pbData, short Msgid) where T : IMessage<T>
	{
		if (OnProtocolSend != null)
		{
			OnProtocolSend(pbData, Msgid);
		}
		int num = 0;
		byte[] array = pbData.ToByteArray();
		num = array.Length;
		SendMsg(Msgid, array, num, ENM_SVRMSGTYPE.eumSvrMsgTypeRequest, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeClient, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeLogicServiceSvr);
	}

	public static byte[] EncodingArrangePlayerLoginInfo(out short length)
	{
		if (WrappersManagerImpl.IsPatched(3883))
		{
			return WrappersManagerImpl.GetPatch(3883).__Gen_Wrap_638(out length);
		}
		PlayerDataVO selfInfo = PlayerDataMgr.GetInstance().GetSelfInfo();
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		ArrangePlayerLoginInfo arrangePlayerLoginInfo = new ArrangePlayerLoginInfo();
		ClientInfo clientInfo = new ClientInfo();
		if (!string.IsNullOrEmpty(selfInfo.nickName))
		{
			clientInfo.Nick = Encoding.UTF8.GetBytes(selfInfo.nickName);
		}
		clientInfo.Age = 0;
		clientInfo.Gender = selfInfo.gender;
		clientInfo.AccountType = (int)selfInfoData.eAccountType;
		clientInfo.ClientType = tSDKGameData.nClientType;
		clientInfo.Network = (IsWiFiNetwork() ? 1 : 3);
		clientInfo.Channel = tSDKGameData.strChannelID;
		clientInfo.Clientversion = (int)HappyMahjong.update.ConfigRequest.GetVersion();
		clientInfo.Osversion = QGDeviceInfo.getOSVersionString();
		clientInfo.Terminal = QGDeviceInfo.getDeviceVersion();
		clientInfo.Openid = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetOpenId();
		clientInfo.Headurl = Encoding.UTF8.GetBytes(selfInfo.headPicUrl);
		clientInfo.Openkey = selfInfoData.strAccessToken;
		PlayerTLVInfo playerTLVInfo = new PlayerTLVInfo();
		PlayerCapability playerCapability = new PlayerCapability();
		playerCapability.Clientcapability = TSDK4CSharpService.CAPABILITY;
		playerTLVInfo.PlayerCapability = playerCapability;
		MJPlayerCapability mJPlayerCapability = new MJPlayerCapability();
		mJPlayerCapability.Clientcapability = TSDK4CSharpService.GetMJCapability();
		playerTLVInfo.MjPlayerCapability = mJPlayerCapability;
		PlayerDiamondNum playerDiamondNum = new PlayerDiamondNum();
		playerDiamondNum.Diamondnum = selfInfo.diamondNum;
		playerTLVInfo.PlayerDiamondNum = playerDiamondNum;
		PlayerDeviceID playerDeviceID = new PlayerDeviceID();
		playerDeviceID.Deviceid = QGDeviceInfo.getDeviceID();
		playerTLVInfo.PlayerDeviceID = playerDeviceID;
		GameSvrIPPort gameSvrIPPort = new GameSvrIPPort();
		playerTLVInfo.GameSvrIPPort = gameSvrIPPort;
		TransparentInfo transparentInfo = new TransparentInfo();
		playerTLVInfo.TransparentInfo = transparentInfo;
		GameSvrBase.LocationInfo locationInfo = new GameSvrBase.LocationInfo();
		playerTLVInfo.LocationInfo = locationInfo;
		arrangePlayerLoginInfo.ClientInfo = clientInfo;
		arrangePlayerLoginInfo.PlayerTLVInfo = playerTLVInfo;
		using MemoryStream memoryStream = new MemoryStream();
		Serializer.Serialize(memoryStream, arrangePlayerLoginInfo);
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Position = 0L;
		memoryStream.Read(array, 0, array.Length);
		length = (short)array.Length;
		return array;
	}

	public static void SendProtoBufferMsg(object pbData, short Msgid)
	{
		if (WrappersManagerImpl.IsPatched(20015))
		{
			WrappersManagerImpl.GetPatch(20015).__Gen_Wrap_1022(pbData, Msgid);
			return;
		}
		using MemoryStream memoryStream = new MemoryStream();
		int num = 0;
		Serializer.Serialize(memoryStream, pbData);
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Position = 0L;
		memoryStream.Read(array, 0, array.Length);
		num = array.Length;
		SendMsg(Msgid, array, num, ENM_SVRMSGTYPE.eumSvrMsgTypeRequest, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeClient, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeLogicServiceSvr);
	}

	public static T DeserializeProtoBufferMsg<T>(byte[] buffer, int bufLen) where T : new()
	{
		try
		{
			if (bufLen == 0)
			{
				return new T();
			}
			using MemoryStream source = new MemoryStream(buffer, 0, bufLen);
			return Serializer.Deserialize<T>(source);
		}
		catch (Exception ex)
		{
			Log.Info("[Util:DeserializeProtoBufferMsg] Exception = " + ex.ToString(), "Deserialize");
			return new T();
		}
	}

	public static void SendMsg(short msgId, byte[] msgBuffer, int msgLen, ENM_SVRMSGTYPE svrMsgType, ENM_FUNCTIONALENTITYTYPE SrcFE, ENM_FUNCTIONALENTITYTYPE DstFE, bool bUserTrigger = true, uint unInterval = 8000u)
	{
		if (WrappersManagerImpl.IsPatched(117))
		{
			WrappersManagerImpl.GetPatch(117).__Gen_Wrap_34(msgId, msgBuffer, msgLen, svrMsgType, SrcFE, DstFE, bUserTrigger, unInterval);
		}
		else
		{
			HappyMahjong.Core.Common.Util.SendMsg(msgId, msgBuffer, msgLen, svrMsgType, SrcFE, DstFE, bUserTrigger, unInterval);
		}
	}

	public static void BuyGoods(BuyGoodsMessage buyGoods)
	{
		if (WrappersManagerImpl.IsPatched(21160))
		{
			WrappersManagerImpl.GetPatch(21160).__Gen_Wrap_0(buyGoods);
			return;
		}
		CReqBuyGoods cReqBuyGoods = new CReqBuyGoods();
		cReqBuyGoods.dwSrcUin = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin();
		cReqBuyGoods.dwDstUin = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin();
		cReqBuyGoods.iAppID = 398;
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		cReqBuyGoods.iClientType = tSDKGameData.nClientType;
		cReqBuyGoods.iAccountType = (int)selfInfoData.eAccountType;
		string strOpenId = selfInfoData.strOpenId;
		sbyte[] array = ByteArrayToSbyteArray(Encoding.ASCII.GetBytes(strOpenId));
		if (array != null)
		{
			cReqBuyGoods.stDiamondReqInfo.nOpenIdLen = (short)array.Length;
			cReqBuyGoods.stDiamondReqInfo.szOpenId = array;
		}
		string strAccessToken = selfInfoData.strAccessToken;
		sbyte[] array2 = ByteArrayToSbyteArray(Encoding.ASCII.GetBytes(strAccessToken));
		if (array2 != null)
		{
			cReqBuyGoods.stDiamondReqInfo.nOpenKeyLen = (short)array2.Length;
			cReqBuyGoods.stDiamondReqInfo.szOpenKey = array2;
		}
		string pfKey = selfInfoData.strPfKey;
		UpdatePfKeyIfEditorOrSimulator(ref pfKey);
		sbyte[] array3 = ByteArrayToSbyteArray(Encoding.ASCII.GetBytes(pfKey));
		if (array3 != null)
		{
			cReqBuyGoods.stDiamondReqInfo.nPfKeyLen = (short)array3.Length;
			cReqBuyGoods.stDiamondReqInfo.szPfKey = array3;
		}
		string pf = selfInfoData.strPf;
		UpdatePfIfEditorOrSimulator(ref pf);
		sbyte[] array4 = ByteArrayToSbyteArray(Encoding.ASCII.GetBytes(pf));
		if (array4 != null)
		{
			cReqBuyGoods.stDiamondReqInfo.nPfLen = (short)array4.Length;
			cReqBuyGoods.stDiamondReqInfo.szPf = array4;
		}
		string strPayToken = selfInfoData.strPayToken;
		sbyte[] array5 = ByteArrayToSbyteArray(Encoding.ASCII.GetBytes(strPayToken));
		if (array5 != null)
		{
			cReqBuyGoods.stDiamondReqInfo.nPayTokenLen = (short)array5.Length;
			cReqBuyGoods.stDiamondReqInfo.szPayToken = array5;
		}
		cReqBuyGoods.nGoodsCount = (short)Math.Min(buyGoods.goodsList.Length, 8);
		cReqBuyGoods.astGoodsItem = new TGoodsItem[cReqBuyGoods.nGoodsCount];
		cReqBuyGoods.szBuyType = new byte[cReqBuyGoods.nGoodsCount];
		for (int i = 0; i < cReqBuyGoods.nGoodsCount; i++)
		{
			cReqBuyGoods.astGoodsItem[i] = new TGoodsItem();
			cReqBuyGoods.astGoodsItem[i].iCount = buyGoods.goodsList[i].count;
			cReqBuyGoods.astGoodsItem[i].iGoodsID = buyGoods.goodsList[i].nID;
			cReqBuyGoods.astGoodsItem[i].iMoneyType = buyGoods.goodsList[i].menoyType;
			cReqBuyGoods.szBuyType[i] = (byte)buyGoods.buyFrom;
		}
		cReqBuyGoods.nTransparentLen = 1;
		cReqBuyGoods.szTransparent = new sbyte[1];
		cReqBuyGoods.szTransparent[0] = (sbyte)(buyGoods.enableBuyOtherGender ? 1 : 0);
		int usedSize = 0;
		cReqBuyGoods.pack(ref TSDK4CSharpService.msgBuffer, 102400, ref usedSize, 0u);
		SendMsg(16045, TSDK4CSharpService.msgBuffer, usedSize, ENM_SVRMSGTYPE.eumSvrMsgTypeRequest, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeClient, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeLogicServiceSvr);
	}

	public static void ReqGoodsLimitInfo(List<int> limitGoodsID)
	{
		if (WrappersManagerImpl.IsPatched(21203))
		{
			WrappersManagerImpl.GetPatch(21203).__Gen_Wrap_0(limitGoodsID);
			return;
		}
		CReqQueryGoods cReqQueryGoods = new CReqQueryGoods();
		cReqQueryGoods.dwUin = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin();
		cReqQueryGoods.iGameID = 398;
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		cReqQueryGoods.iClientType = tSDKGameData.nClientType;
		cReqQueryGoods.iAccountType = (int)selfInfoData.eAccountType;
		cReqQueryGoods.iGoodsCount = limitGoodsID.Count;
		cReqQueryGoods.GoodsIDList = new int[limitGoodsID.Count];
		for (int i = 0; i < limitGoodsID.Count; i++)
		{
			cReqQueryGoods.GoodsIDList[i] = limitGoodsID[i];
		}
		int usedSize = 0;
		cReqQueryGoods.pack(ref TSDK4CSharpService.msgBuffer, 102400, ref usedSize, 0u);
		SendMsg(18692, TSDK4CSharpService.msgBuffer, usedSize, ENM_SVRMSGTYPE.eumSvrMsgTypeRequest, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeClient, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeLogicServiceSvr);
	}

	public static void ReqMysteryGoods(int mysteryGoodsID)
	{
		if (WrappersManagerImpl.IsPatched(49764))
		{
			WrappersManagerImpl.GetPatch(49764).__Gen_Wrap_145(mysteryGoodsID);
			return;
		}
		CReqQueryMysteryGoods obj = new CReqQueryMysteryGoods
		{
			dwUin = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin(),
			iGameID = 398
		};
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		obj.iClientType = tSDKGameData.nClientType;
		obj.iAccountType = (int)selfInfoData.eAccountType;
		obj.iGoodsID = mysteryGoodsID;
		int usedSize = 0;
		obj.pack(ref TSDK4CSharpService.msgBuffer, 102400, ref usedSize, 0u);
		SendMsg(18647, TSDK4CSharpService.msgBuffer, usedSize, ENM_SVRMSGTYPE.eumSvrMsgTypeRequest, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeClient, ENM_FUNCTIONALENTITYTYPE.eumFuncTypeLogicServiceSvr);
	}

	[IDTag(1)]
	public static void GetQQVipInfo(byte[] identify, ref T2UQQVIPINFO vipInfo)
	{
		if (WrappersManagerImpl.IsPatched(53498))
		{
			WrappersManagerImpl.GetPatch(53498).__Gen_Wrap_446(identify, ref vipInfo);
		}
		else if (identify != null && identify.Length >= 3)
		{
			vipInfo.isQQVip = (((identify[0] & 0x80) == 128) ? ((byte)1) : ((byte)0));
			vipInfo.isQQVip4Year = (((identify[1] & 1) == 1) ? ((byte)1) : ((byte)0));
			vipInfo.isQVip4Super = (((identify[1] & 2) == 2) ? ((byte)1) : ((byte)0));
			vipInfo.vipLevel = (byte)(identify[1] >> 2) & 0xF;
			vipInfo.soureType = (((identify[2] & 0x20) == 32) ? 1 : 0);
		}
	}

	[IDTag(0)]
	public static void GetQQVipInfo(sbyte[] identify, ref T2UQQVIPINFO vipInfo)
	{
		if (WrappersManagerImpl.IsPatched(2058))
		{
			WrappersManagerImpl.GetPatch(2058).__Gen_Wrap_446(identify, ref vipInfo);
		}
		else if (identify != null && identify.Length >= 3)
		{
			vipInfo.isQQVip = (((identify[0] & 0x80) == 128) ? ((byte)1) : ((byte)0));
			vipInfo.isQQVip4Year = (((identify[1] & 1) == 1) ? ((byte)1) : ((byte)0));
			vipInfo.isQVip4Super = (((identify[1] & 2) == 2) ? ((byte)1) : ((byte)0));
			vipInfo.vipLevel = (byte)(identify[1] >> 2) & 0xF;
			vipInfo.soureType = (((identify[2] & 0x20) == 32) ? 1 : 0);
		}
	}

	[IDTag(1)]
	public static void ParseLanVipInfo(ref T2ULANZUANINFO lanzuanInfo, byte[] szIdentInfo)
	{
		if (WrappersManagerImpl.IsPatched(33792))
		{
			WrappersManagerImpl.GetPatch(33792).__Gen_Wrap_2121(ref lanzuanInfo, szIdentInfo);
		}
		else if (szIdentInfo.Length >= 3)
		{
			byte chLevel = 0;
			byte bits = szIdentInfo[1];
			byte bits2 = szIdentInfo[2];
			lanzuanInfo = ParseLanVipInfo(ref chLevel, bits, bits2);
		}
	}

	[IDTag(2)]
	public static void ParseLanVipInfo(ref T2ULANZUANINFO lanzuanInfo, sbyte[] szIdentInfo)
	{
		if (WrappersManagerImpl.IsPatched(61880))
		{
			WrappersManagerImpl.GetPatch(61880).__Gen_Wrap_2121(ref lanzuanInfo, szIdentInfo);
		}
		else if (szIdentInfo.Length >= 3)
		{
			byte chLevel = 0;
			byte bits = (byte)szIdentInfo[1];
			byte bits2 = (byte)szIdentInfo[2];
			lanzuanInfo = ParseLanVipInfo(ref chLevel, bits, bits2);
		}
	}

	[IDTag(0)]
	private static T2ULANZUANINFO ParseLanVipInfo(ref byte chLevel, byte bits1, byte bits2)
	{
		if (WrappersManagerImpl.IsPatched(33793))
		{
			return WrappersManagerImpl.GetPatch(33793).__Gen_Wrap_2120(ref chLevel, bits1, bits2);
		}
		T2ULANZUANINFO result = default(T2ULANZUANINFO);
		result.isLanzuan = (byte)(((bits1 & 0x40) != 0) ? 1u : 0u);
		result.isLanzuanYear = (byte)(((bits1 & 0x80) != 0) ? 1u : 0u);
		result.isLanzuanSuper = (byte)(((bits2 & 1) != 0) ? 1u : 0u);
		chLevel = (byte)(chLevel | ((bits2 & 2) >> 1));
		chLevel = (byte)(chLevel | ((bits2 & 4) >> 1));
		chLevel = (byte)(chLevel | ((bits2 & 8) >> 1));
		chLevel = (byte)(chLevel | ((bits2 & 0x10) >> 1));
		result.lanzuanLevel = chLevel;
		return result;
	}

	public static string GetMd5Hash(string input)
	{
		if (WrappersManagerImpl.IsPatched(61881))
		{
			return WrappersManagerImpl.GetPatch(61881).__Gen_Wrap_19(input);
		}
		byte[] array = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}

	[IDTag(0)]
	public static string GetLowcaseMd5Hash(string input)
	{
		if (WrappersManagerImpl.IsPatched(22120))
		{
			return WrappersManagerImpl.GetPatch(22120).__Gen_Wrap_19(input);
		}
		return HappyMahjong.Core.Common.Util.GetLowcaseMd5Hash(input);
	}

	[IDTag(1)]
	public static string GetLowcaseMd5Hash(byte[] input)
	{
		if (WrappersManagerImpl.IsPatched(33545))
		{
			return WrappersManagerImpl.GetPatch(33545).__Gen_Wrap_19(input);
		}
		return HappyMahjong.Core.Common.Util.GetLowcaseMd5Hash(input);
	}

	public static string CalculateFileMD5(string filePath)
	{
		if (WrappersManagerImpl.IsPatched(61882))
		{
			return WrappersManagerImpl.GetPatch(61882).__Gen_Wrap_19(filePath);
		}
		try
		{
			using MD5 mD = MD5.Create();
			using BufferedStream inputStream = new BufferedStream(File.OpenRead(filePath), 1200000);
			return BitConverter.ToString(mD.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
		}
		catch (FileNotFoundException ex)
		{
			Log.Info("CalculateMD5 Error: File not found - " + ex.Message, "Common");
			return string.Empty;
		}
		catch (UnauthorizedAccessException ex2)
		{
			Log.Info("CalculateMD5 Error: Access denied - " + ex2.Message, "Common");
			return string.Empty;
		}
		catch (IOException ex3)
		{
			Log.Info("CalculateMD5 Error: IO exception - " + ex3.Message, "Common");
			return string.Empty;
		}
		catch (Exception ex4)
		{
			Log.Info("CalculateMD5 Error: An unexpected error occurred - " + ex4.Message, "Common");
			return string.Empty;
		}
	}

	public static bool IsSupportUniwebView()
	{
		if (WrappersManagerImpl.IsPatched(21440))
		{
			return WrappersManagerImpl.GetPatch(21440).__Gen_Wrap_11();
		}
		return true;
	}

	public static int GetDisplayRefreshRate()
	{
		if (WrappersManagerImpl.IsPatched(33400))
		{
			return WrappersManagerImpl.GetPatch(33400).__Gen_Wrap_101();
		}
		return Screen.currentResolution.refreshRate;
	}

	public static string GetPlatformName()
	{
		if (WrappersManagerImpl.IsPatched(33399))
		{
			return WrappersManagerImpl.GetPatch(33399).__Gen_Wrap_51();
		}
		return Application.platform.ToString();
	}

	public static bool IsAndroidPlatform()
	{
		if (WrappersManagerImpl.IsPatched(52))
		{
			return WrappersManagerImpl.GetPatch(52).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsAndroidPlatform();
	}

	public static bool IsOpenHarmonyPlatform()
	{
		if (WrappersManagerImpl.IsPatched(129))
		{
			return WrappersManagerImpl.GetPatch(129).__Gen_Wrap_11();
		}
		return false;
	}

	public static bool IsOpenHarmony2In1Device()
	{
		if (WrappersManagerImpl.IsPatched(130))
		{
			return WrappersManagerImpl.GetPatch(130).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsOpenHarmony2In1Device();
	}

	public static bool IsIOSPlatform()
	{
		if (WrappersManagerImpl.IsPatched(36))
		{
			return WrappersManagerImpl.GetPatch(36).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsIOSPlatform();
	}

	public static bool IsEditorPlatform()
	{
		if (WrappersManagerImpl.IsPatched(465))
		{
			return WrappersManagerImpl.GetPatch(465).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsEditorPlatform();
	}

	public static string GetSoftwareVersion()
	{
		if (WrappersManagerImpl.IsPatched(61883))
		{
			return WrappersManagerImpl.GetPatch(61883).__Gen_Wrap_51();
		}
		return QGDeviceInfo.getSoftwareVersion();
	}

	public static uint GetSoftwareVersionValue()
	{
		if (WrappersManagerImpl.IsPatched(61884))
		{
			return WrappersManagerImpl.GetPatch(61884).__Gen_Wrap_651();
		}
		return QGDeviceInfo.versionToUint(GetSoftwareVersion());
	}

	public static bool IsIOSOnMacPlatform()
	{
		if (WrappersManagerImpl.IsPatched(50))
		{
			return WrappersManagerImpl.GetPatch(50).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsIOSOnMacPlatform();
	}

	public static bool CheckContainSymbol(string defineSymbol)
	{
		if (WrappersManagerImpl.IsPatched(30))
		{
			return WrappersManagerImpl.GetPatch(30).__Gen_Wrap_3(defineSymbol);
		}
		return false;
	}

	public static List<string> LoadFile(string path)
	{
		if (WrappersManagerImpl.IsPatched(61885))
		{
			return WrappersManagerImpl.GetPatch(61885).__Gen_Wrap_1299(path);
		}
		StreamReader streamReader = null;
		try
		{
			streamReader = File.OpenText(path);
		}
		catch (Exception)
		{
			return null;
		}
		List<string> list = new List<string>();
		string item;
		while ((item = streamReader.ReadLine()) != null)
		{
			list.Add(item);
		}
		streamReader.Close();
		streamReader.Dispose();
		return list;
	}

	public static bool IsPCPlatform()
	{
		if (WrappersManagerImpl.IsPatched(180))
		{
			return WrappersManagerImpl.GetPatch(180).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsPCPlatform();
	}

	public static bool IsWinSimulator()
	{
		if (WrappersManagerImpl.IsPatched(48984))
		{
			return WrappersManagerImpl.GetPatch(48984).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsWinSimulator();
	}

	public static string GetRandomTimeStamp()
	{
		if (WrappersManagerImpl.IsPatched(33568))
		{
			return WrappersManagerImpl.GetPatch(33568).__Gen_Wrap_51();
		}
		return "?time=" + GetCreatetime(DateTime.Now);
	}

	public static string AppendUrlParameter(string url, string param)
	{
		if (WrappersManagerImpl.IsPatched(21441))
		{
			return WrappersManagerImpl.GetPatch(21441).__Gen_Wrap_52(url, param);
		}
		string text = url;
		if (url != null && url != "")
		{
			text = ((url.IndexOf("?") < 0) ? (url + "?" + param) : (url + "&" + param));
		}
		Log.Debug("AppendUrlParameter result:" + text, "Framework");
		return text;
	}

	public static string JsonKeyEncode(string str)
	{
		if (WrappersManagerImpl.IsPatched(41942))
		{
			return WrappersManagerImpl.GetPatch(41942).__Gen_Wrap_19(str);
		}
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		char[] array = new char[8] { '{', '}', '[', ']', '"', '\'', ':', ',' };
		for (int i = 0; i < array.Length; i++)
		{
			str = str.Replace(array[i].ToString(), "%" + Convert.ToByte(array[i]).ToString("x2"));
		}
		return str;
	}

	public static string JsonKeyDecode(string str)
	{
		if (WrappersManagerImpl.IsPatched(41940))
		{
			return WrappersManagerImpl.GetPatch(41940).__Gen_Wrap_19(str);
		}
		char[] array = new char[8] { '{', '}', '[', ']', '"', '\'', ':', ',' };
		for (int i = 0; i < array.Length; i++)
		{
			str = str.Replace("%" + Convert.ToByte(array[i]).ToString("x2"), array[i].ToString());
		}
		return str;
	}

	public static string SelectUrlDependOnConfig(string liveHttpsString, string liveNoHttpsString, string debugString)
	{
		if (WrappersManagerImpl.IsPatched(389))
		{
			return WrappersManagerImpl.GetPatch(389).__Gen_Wrap_138(liveHttpsString, liveNoHttpsString, debugString);
		}
		string text = debugString;
		text = (SingletonBase<DynamicConfig>.GetInstance().GetBool("Https", "CgiIsOpen", defaultValue: true) ? liveHttpsString : liveNoHttpsString);
		Log.Info("Util SelectUrlDependOnConfig, the url is: " + text, "Game");
		return text;
	}

	public static Canvas RootCanvas(GameObject go)
	{
		if (WrappersManagerImpl.IsPatched(203))
		{
			return WrappersManagerImpl.GetPatch(203).__Gen_Wrap_75(go);
		}
		if (go.transform == null)
		{
			return null;
		}
		return go.GetComponent<Canvas>();
	}

	public static void CheckReplay(string playerPrefsKey, int enterRoomDirectMaxTime = 2)
	{
		if (WrappersManagerImpl.IsPatched(61886))
		{
			WrappersManagerImpl.GetPatch(61886).__Gen_Wrap_10(playerPrefsKey, enterRoomDirectMaxTime);
			return;
		}
		int num = PlayerPrefsHelper.GetInt(playerPrefsKey, 0);
		if (num < enterRoomDirectMaxTime)
		{
			PlayerPrefsHelper.SetInt(playerPrefsKey, num + 1);
			Log.Info("CheckReplay RequestReplay", "Game");
			TSDK4CSharpService.RequestEnterRoomDirect((uint)TSDK4CSharpService.svrId, (short)TSDK4CSharpService.roomId, 0, 0);
		}
	}

	public static void CheckUploadLog(string key)
	{
		if (WrappersManagerImpl.IsPatched(40143))
		{
			WrappersManagerImpl.GetPatch(40143).__Gen_Wrap_0(key);
		}
		else if (PlayerPrefsHelper.GetInt(key, 0) == 0)
		{
			if (SingletonBase<DynamicConfig>.GetInstance().GetBool("UpLoadLogSwitch", "IsOpen"))
			{
				TSDK4CSharpService.UpLoadCurrFile();
			}
			PlayerPrefsHelper.SetInt(key, 1);
		}
	}

	public static void CheckUploadLogAndReplay(string key, string playerPrefsKey, int enterRoomDirectMaxTime = 2, bool needReplay = true)
	{
		if (WrappersManagerImpl.IsPatched(36072))
		{
			WrappersManagerImpl.GetPatch(36072).__Gen_Wrap_824(key, playerPrefsKey, enterRoomDirectMaxTime, needReplay);
			return;
		}
		int num = PlayerPrefsHelper.GetInt(key, 0);
		Log.Info("CheckUploadLogAndReplay hasUpload " + num, "Game");
		if (num == 0)
		{
			if (SingletonBase<DynamicConfig>.GetInstance().GetBool("UpLoadLogSwitch", "IsOpen"))
			{
				TSDK4CSharpService.UpLoadCurrFile();
			}
			PlayerPrefsHelper.SetInt(key, 1);
		}
		if (!(AppRoot.GetInstance().InGameBridgeBase != null) || !AppRoot.GetInstance().InGameBridgeBase.GetRecordReplaying())
		{
			Log.Info("RequestReplay", "Game");
			if (needReplay)
			{
				TSDK4CSharpService.RequestEnterRoomDirect((uint)TSDK4CSharpService.svrId, (short)TSDK4CSharpService.roomId, 0, 0);
			}
		}
	}

	public static bool IsValidJson(string str)
	{
		if (WrappersManagerImpl.IsPatched(48270))
		{
			return WrappersManagerImpl.GetPatch(48270).__Gen_Wrap_3(str);
		}
		if (string.IsNullOrEmpty(str))
		{
			return false;
		}
		str = str.Trim();
		if ((str.StartsWith("{") && str.EndsWith("}")) || (str.StartsWith("[") && str.EndsWith("]")))
		{
			Regex regex = new Regex("^[\\],:{}\\s]*$");
			Regex regex2 = new Regex("\\\\['\\\\\\/bfnrtu]");
			Regex regex3 = new Regex("[^\"\\\n\r]*\"|true|false|null|-?\\d+(?:\\.\\d*)?(?:[eE][+\\-]?\\d+)?");
			Regex regex4 = new Regex("(?:^|:|,)(?:\\s*\\[)+");
			str = regex2.Replace(str, "@");
			str = regex3.Replace(str, "]");
			str = regex4.Replace(str, string.Empty);
			if (regex.IsMatch(str))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static string GetMD5WithFilePath(string filePath, string wCase = "X2")
	{
		if (WrappersManagerImpl.IsPatched(33533))
		{
			return WrappersManagerImpl.GetPatch(33533).__Gen_Wrap_52(filePath, wCase);
		}
		try
		{
			if (!File.Exists(filePath))
			{
				return string.Empty;
			}
			MD5 mD = MD5.Create();
			byte[] buffer = File.ReadAllBytes(filePath);
			byte[] array = mD.ComputeHash(buffer);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString(wCase));
			}
			return stringBuilder.ToString();
		}
		catch (Exception ex)
		{
			throw new Exception("GetMD5WithFilePath() fail,error:" + ex.Message);
		}
	}

	public static string GetMD5HashByByte(byte[] bytedata)
	{
		if (WrappersManagerImpl.IsPatched(35606))
		{
			return WrappersManagerImpl.GetPatch(35606).__Gen_Wrap_19(bytedata);
		}
		try
		{
			SHA512Managed sHA512Managed = new SHA512Managed();
			byte[] array = sHA512Managed.ComputeHash(bytedata);
			sHA512Managed.Clear();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
		catch (Exception ex)
		{
			throw new Exception("GetMD5HashByByte() fail,error:" + ex.Message);
		}
	}

	public static string FormatIntWithSign(long num)
	{
		if (WrappersManagerImpl.IsPatched(61887))
		{
			return WrappersManagerImpl.GetPatch(61887).__Gen_Wrap_1190(num);
		}
		if (num >= 0)
		{
			return $"+{num.ToString()}";
		}
		return $"{num.ToString()}";
	}

	public static void JumpApolloUrl(string url, bool useUniWebView = false, bool isUseURLEncode = true, bool useSystemBrowser = false)
	{
		if (WrappersManagerImpl.IsPatched(21157))
		{
			WrappersManagerImpl.GetPatch(21157).__Gen_Wrap_1107(url, useUniWebView, isUseURLEncode, useSystemBrowser);
			return;
		}
		if (IsPCPlatform())
		{
			PCWebViewHandler.InstanceHandler()?.LoadURL(url);
			return;
		}
		if (Application.isEditor)
		{
			Application.OpenURL(url);
			return;
		}
		Log.Info("JumpApolloUrl " + url, "Framework");
		MSDKWebView.WebViewRetEvent += delegate
		{
			AudioController.Instance.UnmuteAllLayers();
		};
		string text = url;
		text = ((!useUniWebView) ? AppendUrlParameter(url, "isuniwebview=0") : AppendUrlParameter(url, "isuniwebview=1"));
		MSDKWebView.OpenUrl(text, MSDKWebViewOrientation.Auto, isFullScreen: false, isUseURLEncode, "", useSystemBrowser);
		AudioController.Instance.MuteAllLayers();
	}

	public static int GetChannelId()
	{
		if (WrappersManagerImpl.IsPatched(286))
		{
			return WrappersManagerImpl.GetPatch(286).__Gen_Wrap_101();
		}
		if (SingletonBase<GMManager>.GetInstance().Model.OpenChannelFlag)
		{
			return SingletonBase<GMManager>.GetInstance().Model.ChannelID;
		}
		if (IsOpenHarmonyPlatform())
		{
			return 39000001;
		}
		if (IsPCPlatform())
		{
			return 10024293;
		}
		return HappyMahjong.Core.Common.Util.GetChannelId();
	}

	public bool CheckAndroidVerWhetherAbove(int version)
	{
		if (WrappersManagerImpl.IsPatched(61888))
		{
			return WrappersManagerImpl.GetPatch(61888).__Gen_Wrap_122(this, version);
		}
		Log.Info("CheckAndroidVerAbove5 version is:" + version, "VedioRecord");
		return true;
	}

	public static int ChineseCount(string str)
	{
		if (WrappersManagerImpl.IsPatched(61889))
		{
			return WrappersManagerImpl.GetPatch(61889).__Gen_Wrap_85(str);
		}
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (char.ConvertToUtf32(str, i) >= Convert.ToInt32("4e00", 16) && char.ConvertToUtf32(str, i) <= Convert.ToInt32("9fff", 16))
			{
				num++;
			}
		}
		return num;
	}

	public static string StrLimit(string str, int limit, string ingore = "...")
	{
		if (WrappersManagerImpl.IsPatched(61890))
		{
			return WrappersManagerImpl.GetPatch(61890).__Gen_Wrap_2921(str, limit, ingore);
		}
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		string result = str;
		int num = ChineseCount(str);
		if (str.Length + num > limit)
		{
			result = str.Substring(0, limit - num) + ingore;
		}
		return result;
	}

	public static bool IsPad()
	{
		if (WrappersManagerImpl.IsPatched(1407))
		{
			return WrappersManagerImpl.GetPatch(1407).__Gen_Wrap_11();
		}
		return 1f * (float)Screen.width / (float)Screen.height < 1.5f;
	}

	public static bool IsRatioLessThan87()
	{
		if (WrappersManagerImpl.IsPatched(4300))
		{
			return WrappersManagerImpl.GetPatch(4300).__Gen_Wrap_11();
		}
		return (double)(1f * (float)Screen.width / (float)Screen.height) < 1.15;
	}

	public static bool IsRatioLargerThan2()
	{
		if (WrappersManagerImpl.IsPatched(523))
		{
			return WrappersManagerImpl.GetPatch(523).__Gen_Wrap_11();
		}
		return 1f * (float)Screen.width / (float)Screen.height > 2f;
	}

	public static bool IsRatioLargerThan2p5()
	{
		if (WrappersManagerImpl.IsPatched(4301))
		{
			return WrappersManagerImpl.GetPatch(4301).__Gen_Wrap_11();
		}
		return 1f * (float)Screen.width / (float)Screen.height >= 2.5f;
	}

	public static bool CheckGender(ItemConfig itemConfig, UserGender gender)
	{
		if (WrappersManagerImpl.IsPatched(51440))
		{
			return WrappersManagerImpl.GetPatch(51440).__Gen_Wrap_3440(itemConfig, gender);
		}
		return Item.GenderByInt(itemConfig.gender) == gender;
	}

	public static int GetNotchHeight()
	{
		if (WrappersManagerImpl.IsPatched(1408))
		{
			return WrappersManagerImpl.GetPatch(1408).__Gen_Wrap_101();
		}
		return 0;
	}

	public static bool IsIphoneX()
	{
		if (WrappersManagerImpl.IsPatched(1402))
		{
			return WrappersManagerImpl.GetPatch(1402).__Gen_Wrap_11();
		}
		string text = SystemInfo.deviceModel.ToLower();
		Log.Info("DeviceModel is " + text, "SpecialPhone");
		if (CheckIgnorePhone())
		{
			return false;
		}
		if (Device.m_iphoneDeviceName == "iPhone10,3" || Device.m_iphoneDeviceName == "iPhone10,6")
		{
			return true;
		}
		if ((AppRoot.width == 3168 && AppRoot.height == 1440) || (AppRoot.width == 2340 && AppRoot.height == 1080))
		{
			return true;
		}
		if (text.Contains("vivo") && (text.Contains("x21a") || text.Contains("y85a") || text.Contains("z1") || text.Contains("v1809a")))
		{
			return true;
		}
		if (text.Contains("redmi 6 pro") || text.Contains("galaxy s10") || text.Contains("honor v20") || text.Contains("honor 20") || text.Contains("mi 8") || text.Contains("mi play") || text.Contains("ONEPLUS A6000"))
		{
			return true;
		}
		if (IsSpecialPhone(text))
		{
			return true;
		}
		return false;
	}

	public static bool IsUseOldScreenAdapter()
	{
		if (WrappersManagerImpl.IsPatched(1399))
		{
			return WrappersManagerImpl.GetPatch(1399).__Gen_Wrap_11();
		}
		if (IsOldAndroid())
		{
			return true;
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("SpecialPhone", "UseOldScreenAdapter");
	}

	public static bool IsOldAndroid()
	{
		if (WrappersManagerImpl.IsPatched(1400))
		{
			return WrappersManagerImpl.GetPatch(1400).__Gen_Wrap_11();
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			string operatingSystem = SystemInfo.operatingSystem;
			Log.Info("Android system version: " + operatingSystem, "ScreenAdapter");
			string[] array = operatingSystem.Split(' ');
			if (array.Length > 2)
			{
				string text = array[1];
				int result = -1;
				if (!int.TryParse(text, out result) && array.Length > 3)
				{
					text = array[2];
					Log.Info("IsOldAndroid androidVersion: " + text, "ScreenAdapter");
					if (!int.TryParse(text, out result))
					{
						string[] array2 = text.Split('.');
						if (array2.Length <= 1)
						{
							return false;
						}
						if (!int.TryParse(array2[0], out result))
						{
							return false;
						}
					}
				}
				Log.Info("Android version: " + text, "ScreenAdapter");
				if (IsVersionGreaterThan(result, 8))
				{
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private string GetAndroidVersion(string osVersion)
	{
		if (WrappersManagerImpl.IsPatched(61891))
		{
			return WrappersManagerImpl.GetPatch(61891).__Gen_Wrap_52(this, osVersion);
		}
		int num = osVersion.IndexOf("Android OS");
		if (num != -1)
		{
			string[] array = osVersion.Substring(num).Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "OS" && i + 1 < array.Length)
				{
					return array[i + 1];
				}
			}
		}
		return null;
	}

	private static bool IsVersionGreaterThan(int currentVersion, int targetVersion)
	{
		if (WrappersManagerImpl.IsPatched(1401))
		{
			return WrappersManagerImpl.GetPatch(1401).__Gen_Wrap_312(currentVersion, targetVersion);
		}
		return currentVersion > targetVersion;
	}

	private static bool CheckIgnorePhone()
	{
		if (WrappersManagerImpl.IsPatched(1403))
		{
			return WrappersManagerImpl.GetPatch(1403).__Gen_Wrap_11();
		}
		string text = SingletonBase<DynamicConfig>.GetInstance().GetString("SpecialPhone", "IgnoreList");
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		string[] array = text.Split(new char[1] { '#' });
		foreach (string text2 in array)
		{
			if (Device.m_iphoneDeviceName == text2)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsFoldableScreen()
	{
		if (WrappersManagerImpl.IsPatched(1405))
		{
			return WrappersManagerImpl.GetPatch(1405).__Gen_Wrap_11();
		}
		string text = SystemInfo.deviceModel.ToLower();
		string defaultValue = "huawei tet-an00";
		string text2 = SingletonBase<DynamicConfig>.GetInstance().GetString("SpecialPhone", "Foldable", defaultValue);
		if (text2 == string.Empty)
		{
			return false;
		}
		string[] array = text2.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (text.Contains(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsXiaomiTablet()
	{
		if (WrappersManagerImpl.IsPatched(58429))
		{
			return WrappersManagerImpl.GetPatch(58429).__Gen_Wrap_11();
		}
		return false;
	}

	private static bool IsSpecialPhone(string deviceModel)
	{
		if (WrappersManagerImpl.IsPatched(1404))
		{
			return WrappersManagerImpl.GetPatch(1404).__Gen_Wrap_3(deviceModel);
		}
		string defaultValue = "iphone12,iphone11,cor-al00,v1801a0,vivo z1,vivo y85a,vivo v1809a,oppo pbbm00,are-al00,oppo pacm00,oppo pbam00,vivo x21,oppo paam00,oppo pbdm00,huawei hma-al00,samsung sm-g9730,samsung SM-G9750,huawei pct-al10,xiaomi mi 9,xiaomi mi 8 se,reno,oppo pafm00,pcgm00,tel-an00a,pdpm00";
		string text = SingletonBase<DynamicConfig>.GetInstance().GetString("SpecialPhone", "PhoneList", defaultValue);
		if (text == string.Empty)
		{
			return false;
		}
		string[] array = text.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			if (deviceModel.Contains(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNotSupportReflectionPhone(string deviceModel)
	{
		if (WrappersManagerImpl.IsPatched(34347))
		{
			return WrappersManagerImpl.GetPatch(34347).__Gen_Wrap_3(deviceModel);
		}
		return HappyMahjong.Core.Common.Util.IsNotSupportReflectionPhone(deviceModel);
	}

	public static bool IsSpecialScreen()
	{
		if (WrappersManagerImpl.IsPatched(61892))
		{
			return WrappersManagerImpl.GetPatch(61892).__Gen_Wrap_11();
		}
		if ((float)Screen.width * 1f / (float)Screen.height * 1f > specialRatio)
		{
			return true;
		}
		return false;
	}

	public static ScreenOrientation GetPhoneDirection()
	{
		if (WrappersManagerImpl.IsPatched(61893))
		{
			return WrappersManagerImpl.GetPatch(61893).__Gen_Wrap_4165();
		}
		return Device.m_NowDirection;
	}

	public static string GetQQAppId()
	{
		if (WrappersManagerImpl.IsPatched(44274))
		{
			return WrappersManagerImpl.GetPatch(44274).__Gen_Wrap_51();
		}
		return "1101070761";
	}

	public static string GetWXAppId()
	{
		if (WrappersManagerImpl.IsPatched(44275))
		{
			return WrappersManagerImpl.GetPatch(44275).__Gen_Wrap_51();
		}
		return "wx3bef52104e238bff";
	}

	public static string GetMSDKKey()
	{
		if (WrappersManagerImpl.IsPatched(22119))
		{
			return WrappersManagerImpl.GetPatch(22119).__Gen_Wrap_51();
		}
		return "3e6e25a515b4d104d4575f11698c258a";
	}

	public static long GetGCloudGameID()
	{
		if (WrappersManagerImpl.IsPatched(33451))
		{
			return WrappersManagerImpl.GetPatch(33451).__Gen_Wrap_439();
		}
		return HappyMahjong.Core.Common.Util.GetGCloudGameID();
	}

	public static string GetGCloudGameKey()
	{
		if (WrappersManagerImpl.IsPatched(33452))
		{
			return WrappersManagerImpl.GetPatch(33452).__Gen_Wrap_51();
		}
		return "444e7216c1e596f4e8d5362c36629895";
	}

	public static int GetOSVersionFirstNum()
	{
		if (WrappersManagerImpl.IsPatched(61894))
		{
			return WrappersManagerImpl.GetPatch(61894).__Gen_Wrap_101();
		}
		string oSVersionString = QGDeviceInfo.getOSVersionString();
		Log.Info("GetOSVersion is: " + oSVersionString, "Login");
		string[] array = oSVersionString.Split('.');
		int result = 0;
		if (array.Length >= 1 && !string.IsNullOrEmpty(array[0]))
		{
			int.TryParse(array[0], out result);
			return result;
		}
		return result;
	}

	public static string GetStringFromBufRemoveBom(byte[] data)
	{
		if (WrappersManagerImpl.IsPatched(4305))
		{
			return WrappersManagerImpl.GetPatch(4305).__Gen_Wrap_19(data);
		}
		return HappyMahjong.Core.Common.Util.GetStringFromBufRemoveBom(data);
	}

	public static string GetPatchAppVersion()
	{
		if (WrappersManagerImpl.IsPatched(34830))
		{
			return WrappersManagerImpl.GetPatch(34830).__Gen_Wrap_51();
		}
		int num = 0;
		HappyMahjong.Bootstrap.Version version = PathUtil.SubResVFSInfo.GetVersion();
		if (version != null)
		{
			num = version.patch;
		}
		return string.Format($"{HappyMahjong.Core.Common.Util.GetAppBaseVersion()}({num})");
	}

	public static string GetAndPrintAppBuildVersion()
	{
		if (WrappersManagerImpl.IsPatched(34829))
		{
			return WrappersManagerImpl.GetPatch(34829).__Gen_Wrap_51();
		}
		string empty = string.Empty;
		empty = GetPatchAppVersion() + "." + SvnRevision.SvnLastNum.ToString();
		Log.Info("The AppBuildVersion is: " + empty, "Selection");
		return empty;
	}

	public static bool IsOldVersion(string version)
	{
		if (WrappersManagerImpl.IsPatched(49679))
		{
			return WrappersManagerImpl.GetPatch(49679).__Gen_Wrap_3(version);
		}
		string[] array = GetAndPrintAppBuildVersion().Split(".");
		string[] array2 = version.Split(".");
		if (array.Length < 3 || array2.Length < 3)
		{
			Log.Info("version error", "MJ_SYSTEM");
			return true;
		}
		int result = 0;
		int result2 = 0;
		for (int i = 0; i < 3; i++)
		{
			if (!int.TryParse(array[i], out result))
			{
				return true;
			}
			if (!int.TryParse(array2[i], out result2))
			{
				return true;
			}
			if (result < result2)
			{
				return false;
			}
			if (result > result2)
			{
				return true;
			}
		}
		return true;
	}

	public static void ShowDebugStackInfo()
	{
		if (WrappersManagerImpl.IsPatched(61895))
		{
			WrappersManagerImpl.GetPatch(61895).__Gen_Wrap_46();
			return;
		}
		string text = new StackTrace().ToString();
		Log.Info("ShowDebugSrackInfo ：" + text, "Game");
	}

	public static string ConvertGB2312BytesToUTF8String(byte[] chars)
	{
		if (WrappersManagerImpl.IsPatched(33764))
		{
			return WrappersManagerImpl.GetPatch(33764).__Gen_Wrap_19(chars);
		}
		if (chars.Length == 0)
		{
			return "";
		}
		return Encoding.UTF8.GetString(GBKCode.GB2312ToUTF_8(chars)).TrimEnd('\0');
	}

	public static string ToSBC(string input)
	{
		if (WrappersManagerImpl.IsPatched(61896))
		{
			return WrappersManagerImpl.GetPatch(61896).__Gen_Wrap_19(input);
		}
		char[] array = input.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == ' ')
			{
				array[i] = '\u3000';
			}
			else if (array[i] < '\u007f')
			{
				array[i] = (char)(array[i] + 65248);
			}
		}
		return new string(array);
	}

	public static string ToDBC(string input)
	{
		if (WrappersManagerImpl.IsPatched(47982))
		{
			return WrappersManagerImpl.GetPatch(47982).__Gen_Wrap_19(input);
		}
		char[] array = input.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '\u3000')
			{
				array[i] = ' ';
			}
			else if (array[i] > '\uff00' && array[i] < '｟')
			{
				array[i] = (char)(array[i] - 65248);
			}
		}
		return new string(array);
	}

	public static void PlayClickAudio()
	{
		if (WrappersManagerImpl.IsPatched(47717))
		{
			WrappersManagerImpl.GetPatch(47717).__Gen_Wrap_46();
		}
		else
		{
			AudioController.Instance.PlayAuto("wanfa", AudioLayers.Oneshot);
		}
	}

	public static void UpdatePfIfEditorOrSimulator(ref string pf)
	{
		if (WrappersManagerImpl.IsPatched(374))
		{
			WrappersManagerImpl.GetPatch(374).__Gen_Wrap_131(ref pf);
		}
	}

	public static void UpdatePfKeyIfEditorOrSimulator(ref string pfKey)
	{
		if (WrappersManagerImpl.IsPatched(373))
		{
			WrappersManagerImpl.GetPatch(373).__Gen_Wrap_131(ref pfKey);
		}
	}

	public static string GetWakeUpShareDetailString()
	{
		if (WrappersManagerImpl.IsPatched(3787))
		{
			return WrappersManagerImpl.GetPatch(3787).__Gen_Wrap_51();
		}
		string result = "";
		GCloudWakeupInfo wakeUpInfo = SingletonBase<PlayerLoginDataMgr>.GetInstance().WakeUpInfo;
		if (wakeUpInfo != null)
		{
			if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByQQ || SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByPC)
			{
				if (wakeUpInfo.ExtensionInfo.Count > 0)
				{
					for (int i = 0; i < wakeUpInfo.ExtensionInfo.Count; i++)
					{
						GCloudKVPair gCloudKVPair = wakeUpInfo.ExtensionInfo[i];
						if (gCloudKVPair.Key == "gamedata")
						{
							if (gCloudKVPair.Value == null)
							{
								Log.Debug("gamedata key value is null.", "SecretRoom");
								break;
							}
							Log.Debug("SecretRoom", "gamedata key value is {0}.", gCloudKVPair.Value);
							result = gCloudKVPair.Value;
							break;
						}
					}
				}
				else
				{
					if (string.IsNullOrEmpty(wakeUpInfo.MessageExt))
					{
						Log.Info("info.MessageExt null or empty", "SecretRoom");
					}
					result = wakeUpInfo.MessageExt;
				}
			}
			else if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByWeiXin)
			{
				if (string.IsNullOrEmpty(wakeUpInfo.MessageExt))
				{
					Log.Info("info.MessageExt null or empty", "SecretRoom");
				}
				result = wakeUpInfo.MessageExt;
			}
		}
		return result;
	}

	public static string Stringify<T>(IEnumerable<T> sequence, string sep = ", ")
	{
		if (sequence == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in sequence)
		{
			stringBuilder.Append(item);
			stringBuilder.Append(sep);
		}
		return stringBuilder.ToString();
	}

	public static string Stringify<T>(IEnumerable<T> sequence, StringBuilder builder, string sep = ", ")
	{
		if (sequence == null)
		{
			return string.Empty;
		}
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		builder.Clear();
		foreach (T item in sequence)
		{
			builder.Append(item);
			builder.Append(sep);
		}
		return builder.ToString();
	}

	public static bool CheckOnlineInvitePreConditionOK()
	{
		if (WrappersManagerImpl.IsPatched(57934))
		{
			return WrappersManagerImpl.GetPatch(57934).__Gen_Wrap_11();
		}
		if (SingletonBase<MainUIStateCenter>.GetInstance().currentState == 15)
		{
			return false;
		}
		if (SingletonBase<MainUIStateCenter>.GetInstance().blockUIStateList.Contains(SingletonBase<MainUIStateCenter>.GetInstance().currentState))
		{
			return false;
		}
		if (SingletonBase<MainUIStateCenter>.GetInstance().currentState == 7)
		{
			return false;
		}
		if (SingletonBase<MainUIStateCenter>.GetInstance().currentState == 20)
		{
			return false;
		}
		if (SingletonBase<MainUIStateCenter>.GetInstance().currentState == 8)
		{
			return false;
		}
		if (PlayerDataMgr.GetInstance().seasonChangePanel != null || SingletonBase<RankGradingController>.GetInstance().rankGradingSeasonChangeLoading != null)
		{
			return false;
		}
		List<PopUpItem> popUpList = SingletonBase<PopUpManager>.GetInstance().popUpList;
		if (popUpList != null)
		{
			foreach (PopUpItem item in popUpList)
			{
				if (item.name.Equals("Activetied") || item.name.Equals("SocialLiveHomePanel"))
				{
					return false;
				}
			}
		}
		if (SingletonBase<TutorialController>.GetInstance().HasTutorialGameObject())
		{
			Log.Info("OnShowTeamBattleView HasTutorialGameObject is:" + SingletonBase<TutorialController>.GetInstance().HasTutorialGameObject(), "TeamBattle");
			return false;
		}
		return true;
	}

	public static void ShowInviteEmail(InviteDetail detail)
	{
		if (WrappersManagerImpl.IsPatched(53692))
		{
			WrappersManagerImpl.GetPatch(53692).__Gen_Wrap_0(detail);
			return;
		}
		GameObject gameObject = UIUtil.Instantiate(UIUtil.LoadPrefab("RawResources_New/BuyBeans_ab", "InviteEmail"));
		gameObject.GetComponent<InviteEmailHandler>().Init(detail);
		SingletonBase<PopUpManager>.GetInstance().AddPopUp(gameObject, PopUpType.UGUI);
	}

	public static string GetFormatedBytesString(int bytesLength)
	{
		if (WrappersManagerImpl.IsPatched(36537))
		{
			return WrappersManagerImpl.GetPatch(36537).__Gen_Wrap_12(bytesLength);
		}
		string text = "";
		if (bytesLength > 1048576)
		{
			return $"{(double)bytesLength / 1048576.0:F1}MB";
		}
		return $"{bytesLength / 1024}KB";
	}

	public static void SafeUnload(AssetBundle ab)
	{
		if (WrappersManagerImpl.IsPatched(61897))
		{
			WrappersManagerImpl.GetPatch(61897).__Gen_Wrap_0(ab);
		}
		else if (ab != null)
		{
			ab.Unload(unloadAllLoadedObjects: true);
		}
	}

	public static UserPrivilegeStatus GetWXExtraInfo(TRankExtInfo mTRankExtInfo)
	{
		if (WrappersManagerImpl.IsPatched(53569))
		{
			return WrappersManagerImpl.GetPatch(53569).__Gen_Wrap_3636(mTRankExtInfo);
		}
		int num = 0;
		bool isLittleEndian = BitConverter.IsLittleEndian;
		int num2 = 0;
		long num3 = 0L;
		if (mTRankExtInfo.szData.Length < 6)
		{
			Log.Info("Analyse WXExtraPrivilegInfo error, the mTRankExtInfo.szData.Length is: " + mTRankExtInfo.szData.Length, "Selection");
			return UserPrivilegeStatus.NullStatus;
		}
		if (isLittleEndian)
		{
			BitConverter.ToInt16(new byte[2]
			{
				(byte)mTRankExtInfo.szData[num + 1],
				(byte)mTRankExtInfo.szData[num]
			}, 0);
			num += 2;
			BitConverter.ToInt16(new byte[2]
			{
				(byte)mTRankExtInfo.szData[num + 1],
				(byte)mTRankExtInfo.szData[num]
			}, 0);
			num += 2;
			num2 = BitConverter.ToInt16(new byte[2]
			{
				(byte)mTRankExtInfo.szData[num + 1],
				(byte)mTRankExtInfo.szData[num]
			}, 0);
			num += 2;
			byte[] array = new byte[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = (byte)mTRankExtInfo.szData[num + (num2 - 1 - i)];
			}
			switch (num2)
			{
			case 2:
				num3 = BitConverter.ToInt16(array, 0);
				break;
			case 4:
				num3 = BitConverter.ToInt32(array, 0);
				break;
			case 8:
				num3 = BitConverter.ToInt64(array, 0);
				break;
			default:
				Log.Info("Analyse WXExtraPrivilegInfo error, the lenght is: " + num2, "Selection");
				num3 = 0L;
				break;
			}
		}
		else
		{
			BitConverter.ToInt16(new byte[2]
			{
				(byte)mTRankExtInfo.szData[num],
				(byte)mTRankExtInfo.szData[num + 1]
			}, 0);
			num += 2;
			BitConverter.ToInt16(new byte[2]
			{
				(byte)mTRankExtInfo.szData[num],
				(byte)mTRankExtInfo.szData[num + 1]
			}, 0);
			num += 2;
			num2 = BitConverter.ToInt16(new byte[2]
			{
				(byte)mTRankExtInfo.szData[num],
				(byte)mTRankExtInfo.szData[num + 1]
			}, 0);
			num += 2;
			byte[] array2 = new byte[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = (byte)mTRankExtInfo.szData[num + j];
			}
			switch (num2)
			{
			case 2:
				num3 = BitConverter.ToInt16(array2, 0);
				break;
			case 4:
				num3 = BitConverter.ToInt32(array2, 0);
				break;
			case 8:
				num3 = BitConverter.ToInt64(array2, 0);
				break;
			default:
				Log.Info("Analyse WXExtraPrivilegInfo error, the lenght is: " + num2, "Selection");
				num3 = 0L;
				break;
			}
		}
		return (UserPrivilegeStatus)num3;
	}

	public static bool IsNull(UnityEngine.Object o)
	{
		if (WrappersManagerImpl.IsPatched(61898))
		{
			return WrappersManagerImpl.GetPatch(61898).__Gen_Wrap_3(o);
		}
		return o == null;
	}

	public static bool BContainsChinese(string str)
	{
		if (WrappersManagerImpl.IsPatched(40154))
		{
			return WrappersManagerImpl.GetPatch(40154).__Gen_Wrap_3(str);
		}
		return HappyMahjong.Core.Common.Util.BContainsChinese(str);
	}

	private static void TriggerAppIconChange()
	{
		if (WrappersManagerImpl.IsPatched(36761))
		{
			WrappersManagerImpl.GetPatch(36761).__Gen_Wrap_46();
			return;
		}
		try
		{
			string currentIcon = SingletonBase<DynamicConfig>.GetInstance().GetString("AppIconSetting", "currentIcon");
			if (string.IsNullOrEmpty(currentIcon))
			{
				Log.Info("[AppIcon] AppIconSetting/currentIcon is empty, skip icon change", "Framework");
				return;
			}
			Log.Info("[AppIcon] Triggering app icon change to: " + currentIcon, "Framework");
			DynamicAppIconManager.Instance.SetAppIcon(currentIcon, delegate(bool success, string message)
			{
				if (success)
				{
					Log.Info("[AppIcon] Icon changed successfully to: " + currentIcon + ", message: " + message, "Framework");
				}
				else
				{
					Log.Error("[AppIcon] Failed to change icon to: " + currentIcon + ", error: " + message, "Framework");
				}
			});
		}
		catch (Exception ex)
		{
			Log.Error("[AppIcon] Exception in TriggerAppIconChange: " + ex.Message + "\n" + ex.StackTrace, "Framework");
		}
	}

	public static void DoRestartUpdate(Action onClose = null, string hint = "", int oneOrTwoButtonStyle = 1)
	{
		if (WrappersManagerImpl.IsPatched(36758))
		{
			WrappersManagerImpl.GetPatch(36758).__Gen_Wrap_159(onClose, hint, oneOrTwoButtonStyle);
			return;
		}
		Action onClose2 = onClose;
		Log.Info("TSGame force update dialog show", "TSGameUpdate");
		DialogCallbacks callbacks = new DialogCallbacks
		{
			ConfirmCallback = delegate
			{
				try
				{
					TriggerAppIconChange();
					onClose2?.Invoke();
				}
				catch (Exception ex)
				{
					Log.Error(ex.Message, "Framework");
				}
				RestartApp();
			},
			CloseCallback = delegate
			{
				onClose2?.Invoke();
			}
		};
		if (string.IsNullOrEmpty(hint))
		{
			hint = ((!HappyMahjong.Core.Common.Util.IsIOSPlatform()) ? LanguageKey.FORCE_UPDATE_TIPS : LanguageKey.FORCE_UPDATE_TIPS_IOS);
		}
		DialogManager.GetInstance().ShowDialogUgui(oneOrTwoButtonStyle, callbacks, hint, LanguageKey.SURE, LanguageKey.CANCLE);
	}

	public static string GetVersionUpdateTips()
	{
		if (WrappersManagerImpl.IsPatched(37152))
		{
			return WrappersManagerImpl.GetPatch(37152).__Gen_Wrap_51();
		}
		string result = LanguageKey.FORCE_VERSION_UPDATE_TIPS;
		if (IsIOSPlatform())
		{
			result = LanguageKey.FORCE_VERSION_UPDATE_TIPS_IOS;
		}
		return result;
	}

	public static bool IsArrayEquals(Array array1, Array array2)
	{
		if (WrappersManagerImpl.IsPatched(61899))
		{
			return WrappersManagerImpl.GetPatch(61899).__Gen_Wrap_15(array1, array2);
		}
		if ((object)array1.GetType() != array2.GetType())
		{
			return false;
		}
		if (array1.GetLength(0) != array2.GetLength(0))
		{
			return false;
		}
		for (int i = 0; i < array1.GetLength(0); i++)
		{
			System.ValueType obj = (System.ValueType)array1.GetValue(i);
			System.ValueType obj2 = (System.ValueType)array2.GetValue(i);
			if (!obj.Equals(obj2))
			{
				return false;
			}
		}
		return true;
	}

	public static void SendLeaveRoomRequest()
	{
		if (WrappersManagerImpl.IsPatched(34158))
		{
			WrappersManagerImpl.GetPatch(34158).__Gen_Wrap_46();
			return;
		}
		Log.Info("Send leaveRoom req !", "Game");
		int num = TSDK4CSharp.TSDKService.LeaveRoom();
		Log.Debug("LeaveRoom nRet:" + num, "Game");
		if (num != 0)
		{
			TSDK4CSharp.TSDKService.CancelEnterRoom();
			TSDK4CSharpService.CancelArrange();
		}
	}

	public static bool GetAdsShowState(string platform)
	{
		if (WrappersManagerImpl.IsPatched(22249))
		{
			return WrappersManagerImpl.GetPatch(22249).__Gen_Wrap_3(platform);
		}
		bool flag = SingletonBase<DynamicConfig>.GetInstance().GetInt("GDT", platform + "Show") == 1;
		bool flag2 = ChannelController.IsAdsShow(platform);
		bool flag3 = SingletonBase<DynamicConfig>.GetInstance().GetInt("GDT", platform + "PlatformShow") == 1;
		flag2 = flag2 && flag3;
		return ChannelController.IsSupportGDT() && flag && flag2;
	}

	public static string RemoveLastChar(string str, string value)
	{
		if (WrappersManagerImpl.IsPatched(35697))
		{
			return WrappersManagerImpl.GetPatch(35697).__Gen_Wrap_52(str, value);
		}
		int num = str.LastIndexOf(value);
		if (num != -1)
		{
			return str.Substring(0, num);
		}
		return str;
	}

	public static void RequestShareInternal(bool isFriend, string title, string media, string content, OnMSDKRetEventHandler<MSDKBaseRet> callBack = null, string extInfo = null)
	{
		if (WrappersManagerImpl.IsPatched(61416))
		{
			WrappersManagerImpl.GetPatch(61416).__Gen_Wrap_4133(isFriend, title, media, content, callBack, extInfo);
			return;
		}
		Log.Info("Util RequestShareInternal begin", "Framework");
		RuntimePlatform platform = Application.platform;
		PlayerDataVO selfInfo = PlayerDataMgr.GetInstance().GetSelfInfo();
		LoginType loginType = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType();
		uint loginDataUin = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin();
		if (selfInfo == null)
		{
			return;
		}
		string text = "https://gamecenter.qq.com/gcjump?appid=1101070761&pf=invite&from=iphoneqq&plat=qq&originuin=";
		string text2 = "https://gamecenter.qq.com/gcjump?appid=1101070761&pf=invite&from=androidqq&plat=qq&originuin=";
		string text3 = ((platform == RuntimePlatform.IPhonePlayer) ? text : (text2 + loginDataUin));
		if (loginType == LoginType.LoginByQQ)
		{
			extInfo = "OpenGame?" + extInfo;
		}
		text3 += "&ADTAG=gameobj.msg_invite";
		if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer || IsOpenHarmonyPlatform())
		{
			string sUrl = ((loginType == LoginType.LoginByQQ) ? text3 : null);
			if (loginType == LoginType.LoginByWeiXin)
			{
				SingletonBase<SNS>.GetInstance().SendMessage(title, content, null, media, isFriend, callBack, sUrl, extInfo);
			}
			else
			{
				SingletonBase<SNS>.GetInstance().SendMessage(title, content, null, isFriend, callBack, sUrl, extInfo);
			}
			Log.Info("Util RequestShareInternal", "Framework");
		}
		else
		{
			callBack?.Invoke(null);
		}
	}

	public static System.Type GetTypeByString(string typeName)
	{
		if (WrappersManagerImpl.IsPatched(61900))
		{
			return WrappersManagerImpl.GetPatch(61900).__Gen_Wrap_177(typeName);
		}
		string typeName2 = typeName;
		return (from asm in AppDomain.CurrentDomain.GetAssemblies()
			from type in asm.GetTypes()
			where (type.IsClass || type.IsEnum || type.IsValueType) && type.Name == typeName2
			select type).Single();
	}

	public static T[] GetActionBySerialize<T>(string recordFileName)
	{
		if (string.IsNullOrEmpty(recordFileName))
		{
			return null;
		}
		object[] array = DeSerializeObject<object[]>(recordFileName);
		if (array == null)
		{
			Log.Debug("Framework", "after DeSerializeObject:{0}, null == userActionsType", recordFileName);
			return null;
		}
		List<System.Type> list = new List<System.Type>();
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			XmlNode[] array3 = array2[i] as XmlNode[];
			if (array3.Length == 0 || string.IsNullOrEmpty(array3[0].Value))
			{
				Log.Debug("Framework", "nodes.Length:{0}, nodes.Length == 0 || string.IsNullOrEmpty(nodes[0].Value)", array3.Length);
				return null;
			}
			System.Type typeByString = GetTypeByString(array3[0].Value);
			if (null == typeByString)
			{
				Log.Debug("Framework", "after GetTypeByString:{0}, null == genType", array3[0].Value);
				return null;
			}
			list.Add(typeByString);
		}
		T[] array4 = DeSerializeObject<T[]>(recordFileName, list.ToArray());
		if (array4 == null)
		{
			Log.Debug("Framework", "after Deserialize:{0}, null == userActions, outputTypes.Count:{1}", recordFileName, list.Count);
			return null;
		}
		return array4;
	}

	public static T DeSerializeObject<T>(string fileName, System.Type[] types)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return default(T);
		}
		T result = default(T);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		using StringReader stringReader = new StringReader(xmlDocument.OuterXml);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), types);
		using (XmlReader xmlReader = new XmlTextReader(stringReader))
		{
			result = (T)xmlSerializer.Deserialize(xmlReader);
			xmlReader.Close();
		}
		stringReader.Close();
		return result;
	}

	public static T DeSerializeObject<T>(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return default(T);
		}
		T result = default(T);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		using StringReader stringReader = new StringReader(xmlDocument.OuterXml);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using (XmlReader xmlReader = new XmlTextReader(stringReader))
		{
			result = (T)xmlSerializer.Deserialize(xmlReader);
			xmlReader.Close();
		}
		stringReader.Close();
		return result;
	}

	public static List<int> ConvertToOldGangList(List<GangInfo> gangList)
	{
		if (WrappersManagerImpl.IsPatched(59261))
		{
			return WrappersManagerImpl.GetPatch(59261).__Gen_Wrap_142(gangList);
		}
		List<int> list = new List<int>();
		for (int i = 0; i < gangList.Count; i++)
		{
			for (int j = 0; j < gangList[i].gangNum; j++)
			{
				list.Add(gangList[i].mjValue);
			}
		}
		return list;
	}

	public static bool BetaVersionBlockSwitch()
	{
		if (WrappersManagerImpl.IsPatched(34222))
		{
			return WrappersManagerImpl.GetPatch(34222).__Gen_Wrap_11();
		}
		return true;
	}

	public static string GetQQAppPackageName()
	{
		if (WrappersManagerImpl.IsPatched(1146))
		{
			return WrappersManagerImpl.GetPatch(1146).__Gen_Wrap_51();
		}
		return "com.tencent.mobileqq";
	}

	public static string GetWXAppPackageName()
	{
		if (WrappersManagerImpl.IsPatched(1147))
		{
			return WrappersManagerImpl.GetPatch(1147).__Gen_Wrap_51();
		}
		return "com.tencent.mm";
	}

	public static string TryGettingHeadPicUrlInMemory(uint userUin)
	{
		if (WrappersManagerImpl.IsPatched(59960))
		{
			return WrappersManagerImpl.GetPatch(59960).__Gen_Wrap_2422(userUin);
		}
		string result = string.Empty;
		PlayerDataVO playerDataByUin = PlayerDataMgr.GetInstance().GetPlayerDataByUin(userUin);
		if (playerDataByUin != null && playerDataByUin.headPicUrl != null)
		{
			result = playerDataByUin.headPicUrl.Trim('\0');
		}
		return result;
	}

	public static bool isOpenByConfig(string key, bool defaultVal = false)
	{
		if (WrappersManagerImpl.IsPatched(61901))
		{
			return WrappersManagerImpl.GetPatch(61901).__Gen_Wrap_126(key, defaultVal);
		}
		bool flag = SingletonBase<DynamicConfig>.GetInstance().GetBool(key, "isOpen", defaultVal);
		if (!flag)
		{
			return false;
		}
		string text = SingletonBase<DynamicConfig>.GetInstance().GetString(key, "beginTime");
		string text2 = SingletonBase<DynamicConfig>.GetInstance().GetString(key, "endTime");
		if (text != null && text.Length > 0 && text2 != null && text2.Length > 0)
		{
			int serverTime = SingletonBase<ServerTime>.GetInstance().GetServerTime();
			if (serverTime < ServerTime.DateToTicket(text) || serverTime > ServerTime.DateToTicket(text2))
			{
				return false;
			}
		}
		return flag;
	}

	public static string GetCsConfigPath()
	{
		if (WrappersManagerImpl.IsPatched(33431))
		{
			return WrappersManagerImpl.GetPatch(33431).__Gen_Wrap_51();
		}
		return HappyMahjong.Core.PathUtil.GetCsConfigPath();
	}

	public static void WarmUpShaders(string collectionsFilePath)
	{
		if (WrappersManagerImpl.IsPatched(34842))
		{
			WrappersManagerImpl.GetPatch(34842).__Gen_Wrap_0(collectionsFilePath);
			return;
		}
		ShaderVariantCollection shaderVariantCollection = Resources.Load<ShaderVariantCollection>(collectionsFilePath);
		if (shaderVariantCollection != null)
		{
			Log.Info("Start WarmUp Shader Collection: " + collectionsFilePath, "Framework");
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			shaderVariantCollection.WarmUp();
			Log.Info("WarmUpShaders Cost: " + (Time.realtimeSinceStartup - realtimeSinceStartup), "Framework");
		}
		else
		{
			Log.Info("Load ShaderVariantCollection Failed: " + collectionsFilePath, "Framework");
		}
	}

	[IteratorStateMachine(typeof(<LoadConfigByPath>d__417))]
	public static IEnumerator LoadConfigByPath(string path, BytesDelegalte cb)
	{
		if (WrappersManagerImpl.IsPatched(61902))
		{
			return WrappersManagerImpl.GetPatch(61902).__Gen_Wrap_158(path, cb);
		}
		return new <LoadConfigByPath>d__417(0)
		{
			path = path,
			cb = cb
		};
	}

	public static bool IsFirstToday(string id)
	{
		if (WrappersManagerImpl.IsPatched(56630))
		{
			return WrappersManagerImpl.GetPatch(56630).__Gen_Wrap_3(id);
		}
		bool flag = false;
		string text = PlayerPrefsHelper.GetString(id);
		Log.Info(id + " " + text, "Activity");
		Log.Info("NowTime " + DateTime.Now.ToString("yyyyMMdd"), "Activity");
		Log.Info("result " + DateTime.Now.ToString("yyyyMMdd").CompareTo(text), "Activity");
		if (text == "" || DateTime.Now.ToString("yyyyMMdd").CompareTo(text) != 0)
		{
			return true;
		}
		return false;
	}

	public static void SetFirstToday(string id)
	{
		if (WrappersManagerImpl.IsPatched(56631))
		{
			WrappersManagerImpl.GetPatch(56631).__Gen_Wrap_0(id);
		}
		else
		{
			PlayerPrefsHelper.SetString(id, DateTime.Now.ToString("yyyyMMdd"));
		}
	}

	public static string GetChannel()
	{
		if (WrappersManagerImpl.IsPatched(61909))
		{
			return WrappersManagerImpl.GetPatch(61909).__Gen_Wrap_51();
		}
		string result = "QQ";
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByWeiXin)
		{
			result = "WeChat";
		}
		else if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByQQ)
		{
			result = "QQ";
		}
		return result;
	}

	public static MSDKLoginRet GetMSDKLoginInfo()
	{
		if (WrappersManagerImpl.IsPatched(46))
		{
			return WrappersManagerImpl.GetPatch(46).__Gen_Wrap_18();
		}
		return GetLoginInfo();
	}

	private static MSDKLoginRet GetLoginInfo()
	{
		if (WrappersManagerImpl.IsPatched(47))
		{
			return WrappersManagerImpl.GetPatch(47).__Gen_Wrap_18();
		}
		MSDKLoginRet loginRet = MSDKLogin.GetLoginRet();
		if (loginRet != null)
		{
			Log.Debug("accountService.GetRecord, the result is: " + loginRet.RetCode, "Activity");
			if (loginRet.RetCode == 0)
			{
				if (SingletonBase<CloudGameController>.GetInstance().IsFakeIOS())
				{
					loginRet.Pf = loginRet.Pf.Replace("android", "iap");
				}
				else if (IsIOSOnMacPlatform())
				{
					loginRet.Pf = loginRet.Pf.Replace("-iap-1001-", "-iap-" + 99999999 + "-");
				}
			}
		}
		return loginRet;
	}

	public static bool CheckListSame<T>(List<T> a, List<T> b)
	{
		a.Sort();
		b.Sort();
		if (a.Count == b.Count)
		{
			for (int i = 0; i < a.Count; i++)
			{
				if (!a[i].Equals(b[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public static bool CheckItemConflict(List<ItemType> a, List<ItemType> b)
	{
		if (WrappersManagerImpl.IsPatched(36138))
		{
			return WrappersManagerImpl.GetPatch(36138).__Gen_Wrap_15(a, b);
		}
		foreach (ItemType item in a)
		{
			if (b.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetQrCodeSpriteStr()
	{
		if (WrappersManagerImpl.IsPatched(5312))
		{
			return WrappersManagerImpl.GetPatch(5312).__Gen_Wrap_51();
		}
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByWeiXin)
		{
			return "WXCode";
		}
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByQQ)
		{
			return "QQCode";
		}
		return string.Empty;
	}

	public static string GetFriendCircleQrCodeSpriteStr()
	{
		if (WrappersManagerImpl.IsPatched(5321))
		{
			return WrappersManagerImpl.GetPatch(5321).__Gen_Wrap_51();
		}
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByWeiXin)
		{
			return "WXFriendCircleCode";
		}
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType() == LoginType.LoginByQQ)
		{
			return "QQCode";
		}
		return string.Empty;
	}

	public static int GetNormalTargetFrameRate()
	{
		if (WrappersManagerImpl.IsPatched(61910))
		{
			return WrappersManagerImpl.GetPatch(61910).__Gen_Wrap_101();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetInt("ExtraConfig", "FrameRateNormal", 30);
	}

	public static int GetHighTargetFrameRate()
	{
		if (WrappersManagerImpl.IsPatched(61911))
		{
			return WrappersManagerImpl.GetPatch(61911).__Gen_Wrap_101();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetInt("ExtraConfig", "FrameRateHigh", 30);
	}

	public static int GetSuperHighTargetFrameRate()
	{
		if (WrappersManagerImpl.IsPatched(61912))
		{
			return WrappersManagerImpl.GetPatch(61912).__Gen_Wrap_101();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetInt("ExtraConfig", "FrameRateSuperHigh", 60);
	}

	public static int GetNormalQualityLevel()
	{
		if (WrappersManagerImpl.IsPatched(61913))
		{
			return WrappersManagerImpl.GetPatch(61913).__Gen_Wrap_101();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetInt("ExtraConfig", "NormalQuality", 2);
	}

	public static int GetHighQualityLevel()
	{
		if (WrappersManagerImpl.IsPatched(61914))
		{
			return WrappersManagerImpl.GetPatch(61914).__Gen_Wrap_101();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetInt("ExtraConfig", "HighQuality", 3);
	}

	public static int GetSuperHighQualityLevel()
	{
		if (WrappersManagerImpl.IsPatched(61915))
		{
			return WrappersManagerImpl.GetPatch(61915).__Gen_Wrap_101();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetInt("ExtraConfig", "SuperHighQuality", 4);
	}

	public static bool IsWiFiNetwork()
	{
		if (WrappersManagerImpl.IsPatched(3885))
		{
			return WrappersManagerImpl.GetPatch(3885).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsWiFiNetwork();
	}

	public static bool IsNeedPushNewUserAgreement()
	{
		if (WrappersManagerImpl.IsPatched(49116))
		{
			return WrappersManagerImpl.GetPatch(49116).__Gen_Wrap_11();
		}
		if (IsPCPlatform())
		{
			return false;
		}
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("ForcePopupAgreement", "IsOpen", 1);
		if (num == 0)
		{
			Log.Info("IsOpen = " + num, "ChangeIdentity");
			return false;
		}
		int num2 = PlayerPrefs.GetInt("NEW_IDENTITY_POPUP");
		Log.Info("dialogPushded = " + num2, "ChangeIdentity");
		if (num2 == 1)
		{
			return false;
		}
		int newIdentitySwithTime = GetNewIdentitySwithTime();
		int createtime = GetCreatetime(DateTime.Now);
		Log.Info("switch time = " + newIdentitySwithTime + ", popupTime = " + createtime, "ChangeIdentity");
		if (newIdentitySwithTime != 0 && createtime >= newIdentitySwithTime)
		{
			return true;
		}
		return false;
	}

	public static int GetNewIdentitySwithTime()
	{
		if (WrappersManagerImpl.IsPatched(49004))
		{
			return WrappersManagerImpl.GetPatch(49004).__Gen_Wrap_101();
		}
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("ForcePopupAgreement", "SwitchTime");
		if (num == 0)
		{
			return PlayerPrefs.GetInt("ForceSwitchTime", 0);
		}
		return num;
	}

	public static void PopupNewUserAgreement()
	{
		if (WrappersManagerImpl.IsPatched(55529))
		{
			WrappersManagerImpl.GetPatch(55529).__Gen_Wrap_46();
			return;
		}
		Log.Info("PopupNewUserAgreement", "ChangeIdentity");
		PopupCurrentDayNotice(PopupNewUserAgreementImpl);
	}

	private static void PopupNewUserAgreementImpl()
	{
		if (WrappersManagerImpl.IsPatched(55530))
		{
			WrappersManagerImpl.GetPatch(55530).__Gen_Wrap_46();
		}
	}

	public static void PopupSevenDaysNotice()
	{
		if (WrappersManagerImpl.IsPatched(57912))
		{
			WrappersManagerImpl.GetPatch(57912).__Gen_Wrap_46();
			return;
		}
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("ForcePopupAgreement", "SevenDayPopBeginTime");
		int num2 = SingletonBase<DynamicConfig>.GetInstance().GetInt("ForcePopupAgreement", "SevenDayPopEndTime");
		int createtime = GetCreatetime(DateTime.Now);
		Log.Info(string.Format($"PopupSevenDaysNotice: startTime={num}, endTime={num2}, nowTime = {createtime}"), "ChangeIdentity");
		if (num != 0 && num2 != 0 && num <= createtime && createtime <= num2 && IsFirstToday("SevenDayPopup"))
		{
			SetFirstToday("SevenDayPopup");
			GameObject gameObject = UIUtil.Instantiate(UIUtil.LoadPrefab("RawResources_New/Order_ab/", "NoticeSevenDay"));
			SingletonBase<PopUpManager>.GetInstance().AddPopUp(gameObject, PopUpType.UGUI, null, null, -400f, isAlwaysTop: true, useBlurBKInGame: true, 9, overrideOrder: true);
			gameObject.GetComponent<NoticeIdentityChangeHandler>().ConfigSevenDays();
		}
	}

	public static void PopupCurrentDayNotice(VoidDelegate callback)
	{
		if (WrappersManagerImpl.IsPatched(55531))
		{
			WrappersManagerImpl.GetPatch(55531).__Gen_Wrap_0(callback);
			return;
		}
		Log.Info("PopupCurrentDayNotice", "ChangeIdentity");
		GameObject gameObject = UIUtil.Instantiate(UIUtil.LoadPrefab("RawResources_New/Order_ab/", "NoticeCurrentDay"));
		SingletonBase<PopUpManager>.GetInstance().AddPopUp(gameObject, PopUpType.UGUI, null, null, -400f, isAlwaysTop: true, useBlurBKInGame: true, 9, overrideOrder: true);
		gameObject.GetComponent<NoticeIdentityChangeHandler>().ConfigOneDay(callback);
	}

	public static bool IsNeedPushPrivateUpdateAgreement()
	{
		if (WrappersManagerImpl.IsPatched(49118))
		{
			return WrappersManagerImpl.GetPatch(49118).__Gen_Wrap_11();
		}
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("ForcePopupPrivateAgreement", "IsOpen", 1);
		if (num == 0)
		{
			Log.Info("IsOpen = " + num, "PrivateUpdate");
			return false;
		}
		int num2 = PlayerPrefs.GetInt("LAST_AGREE_PRIVATE");
		int privateUpdateTime = GetPrivateUpdateTime();
		int createtime = GetCreatetime(DateTime.Now);
		Log.Info("switch time = " + privateUpdateTime + ", popupTime = " + createtime, "PrivateUpdate");
		if (privateUpdateTime != 0)
		{
			if (num2 >= privateUpdateTime)
			{
				return false;
			}
			if (createtime >= privateUpdateTime)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetPrivateUpdateTime()
	{
		if (WrappersManagerImpl.IsPatched(49119))
		{
			return WrappersManagerImpl.GetPatch(49119).__Gen_Wrap_101();
		}
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("ForcePopupPrivateAgreement", "PrivateUpdateTime");
		if (num == 0)
		{
			return PlayerPrefs.GetInt("PrivateUpdateTime", 0);
		}
		return num;
	}

	public static void PopupPrivateUpdateAgreement(bool isInGame)
	{
		if (WrappersManagerImpl.IsPatched(49120))
		{
			WrappersManagerImpl.GetPatch(49120).__Gen_Wrap_192(isInGame);
			return;
		}
		Log.Info("PopupNewUserAgreement", "ChangeIdentity");
		PopupPrivateUpdateAgreementImpl(isInGame);
	}

	private static void PopupPrivateUpdateAgreementImpl(bool isInGame)
	{
		if (WrappersManagerImpl.IsPatched(49121))
		{
			WrappersManagerImpl.GetPatch(49121).__Gen_Wrap_192(isInGame);
			return;
		}
		GameObject gameObject = UIUtil.Instantiate(UIUtil.LoadPrefab("RawResources_New/PrivateUpdate_ab", "PrivateUpdatePanel"));
		SingletonBase<PopUpManager>.GetInstance().AddPopUp(gameObject, PopUpType.UGUI, null, null, -400f, isAlwaysTop: true, useBlurBKInGame: true, 8, overrideOrder: true);
		gameObject.GetComponent<PrivateUpdateHandler>().Init(isInGame);
	}

	public static bool CheckIsMeetCondition(string minVerStr, string maxVerStr, string curVerStr)
	{
		if (WrappersManagerImpl.IsPatched(61916))
		{
			return WrappersManagerImpl.GetPatch(61916).__Gen_Wrap_105(minVerStr, maxVerStr, curVerStr);
		}
		uint versionByStr = TSDK4CSharpService.GetVersionByStr(minVerStr);
		uint versionByStr2 = TSDK4CSharpService.GetVersionByStr(maxVerStr);
		uint versionByStr3 = TSDK4CSharpService.GetVersionByStr(curVerStr);
		if (versionByStr3 >= versionByStr && versionByStr3 <= versionByStr2)
		{
			return true;
		}
		return false;
	}

	public static int GetUserGraphicSettingLevel()
	{
		if (WrappersManagerImpl.IsPatched(1421))
		{
			return WrappersManagerImpl.GetPatch(1421).__Gen_Wrap_101();
		}
		return QualityHelper.GetPlayerPrefsOrRecommendQualityLevel();
	}

	public static List<int> GetEnumValues(System.Type type)
	{
		if (WrappersManagerImpl.IsPatched(61917))
		{
			return WrappersManagerImpl.GetPatch(61917).__Gen_Wrap_142(type);
		}
		List<int> list = new List<int>();
		System.Type underlyingType = Enum.GetUnderlyingType(type);
		Array values = Enum.GetValues(type);
		for (int i = 0; i < values.Length; i++)
		{
			object obj = Convert.ChangeType(values.GetValue(i), underlyingType);
			list.Add((int)obj);
		}
		return list;
	}

	public static bool ByteEquals(byte[] a1, byte[] a2)
	{
		if (WrappersManagerImpl.IsPatched(61918))
		{
			return WrappersManagerImpl.GetPatch(61918).__Gen_Wrap_15(a1, a2);
		}
		return HappyMahjong.Core.Common.Util.ByteEquals(a1, a2);
	}

	public static void SetCastShadow(GameObject mj, bool on)
	{
		if (WrappersManagerImpl.IsPatched(61919))
		{
			WrappersManagerImpl.GetPatch(61919).__Gen_Wrap_8(mj, on);
		}
	}

	public static bool HasEquipTable()
	{
		if (WrappersManagerImpl.IsPatched(49643))
		{
			return WrappersManagerImpl.GetPatch(49643).__Gen_Wrap_11();
		}
		foreach (int item2 in SingletonBase<ShopDataHelper>.GetInstance().GetFitonItemId())
		{
			Item item = SingletonBase<ShopDataHelper>.GetInstance().GetItem(item2);
			if (item.pType.Contains(ItemType.Desk) || item.pType.Contains(ItemType.DeskPro) || item.pType.Contains(ItemType.PanoramicDesk))
			{
				return true;
			}
		}
		return false;
	}

	public static Item GetEquipTableItem()
	{
		if (WrappersManagerImpl.IsPatched(20594))
		{
			return WrappersManagerImpl.GetPatch(20594).__Gen_Wrap_401();
		}
		foreach (int item2 in SingletonBase<ShopDataHelper>.GetInstance().GetFitonItemId())
		{
			Item item = SingletonBase<ShopDataHelper>.GetInstance().GetItem(item2);
			if (item.pType.Contains(ItemType.Desk) || item.pType.Contains(ItemType.DeskPro) || item.pType.Contains(ItemType.PanoramicDesk))
			{
				return item;
			}
		}
		return null;
	}

	public static List<Item> GetFitonDeskProAccessories(int deskID, int viewSeatID)
	{
		if (WrappersManagerImpl.IsPatched(21215))
		{
			return WrappersManagerImpl.GetPatch(21215).__Gen_Wrap_1117(deskID, viewSeatID);
		}
		Item item = SingletonBase<ShopDataHelper>.GetInstance().GetItem(deskID);
		if (item == null)
		{
			return null;
		}
		List<int> list = item.SpecialItems();
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		PlayerDataVO playerByViewSeat = PlayerDataMgr.GetInstance().GetPlayerByViewSeat(viewSeatID);
		if (playerByViewSeat == null || playerByViewSeat.avatarInfo == null || playerByViewSeat.avatarInfo.itemList == null)
		{
			return null;
		}
		List<Item> list2 = new List<Item>();
		foreach (int item3 in playerByViewSeat.avatarInfo.itemList)
		{
			if (list.Contains(item3))
			{
				Item item2 = SingletonBase<ShopDataHelper>.GetInstance().GetItem(item3);
				if (item2 != null)
				{
					list2.Add(item2);
				}
			}
		}
		return list2;
	}

	public static bool DeskIsAllGamesUse()
	{
		if (WrappersManagerImpl.IsPatched(20585))
		{
			return WrappersManagerImpl.GetPatch(20585).__Gen_Wrap_11();
		}
		object obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("AllGamesUse");
		if (obj == null || Convert.ToInt32(obj) == 0)
		{
			return false;
		}
		return true;
	}

	public static bool DeskIsFeatOldScenes()
	{
		if (WrappersManagerImpl.IsPatched(20590))
		{
			return WrappersManagerImpl.GetPatch(20590).__Gen_Wrap_11();
		}
		object obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("FeatOldScenes");
		if (obj == null || Convert.ToInt32(obj) == 0)
		{
			return false;
		}
		return true;
	}

	[IDTag(1)]
	public static string GetTableResourceVersion(Item deskItem)
	{
		if (WrappersManagerImpl.IsPatched(21932))
		{
			return WrappersManagerImpl.GetPatch(21932).__Gen_Wrap_19(deskItem);
		}
		return GetTableResourceVersion(deskItem?.ID ?? 0);
	}

	[IDTag(0)]
	public static string GetTableResourceVersion(int tableItemId)
	{
		if (WrappersManagerImpl.IsPatched(21933))
		{
			return WrappersManagerImpl.GetPatch(21933).__Gen_Wrap_12(tableItemId);
		}
		TableConfig tableConfig = ((tableItemId <= 0) ? null : ProtoConfigLoader<TableConfig>.getInstance().getConfigByKey(tableItemId));
		if (tableConfig != null && !string.IsNullOrWhiteSpace(tableConfig.version))
		{
			return tableConfig.version;
		}
		string text = SingletonBase<DynamicConfig>.GetInstance().GetString("TableConfig", "DefaultVersion", "v2");
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return "v2";
	}

	public static void PreLoadRankDefaultDesk(bool status)
	{
		if (WrappersManagerImpl.IsPatched(41277))
		{
			WrappersManagerImpl.GetPatch(41277).__Gen_Wrap_192(status);
			return;
		}
		if (status)
		{
			ItemConfig configByKey = ProtoConfigLoader<ItemConfig>.getInstance().getConfigByKey(36003);
			if (configByKey != null && string.IsNullOrEmpty(configByKey.LocalMod))
			{
				ResourcesLoaderV2.GetInstance(GetTableResourceVersion(configByKey.id)).LoadAssetBundle(configByKey.model, null, null, EnmPriority.Normal, EnmResTag.Game);
			}
		}
		Log.Info($"My mahjong table is status {status}", "Game");
	}

	public static void ResetAnimation(Animation ani, string name)
	{
		if (WrappersManagerImpl.IsPatched(61920))
		{
			WrappersManagerImpl.GetPatch(61920).__Gen_Wrap_1(ani, name);
			return;
		}
		AnimationState animationState = ani[name];
		ani.Play(name);
		animationState.time = 0f;
		ani.Sample();
		animationState.enabled = false;
	}

	public static void PreloadMyDeskItem(IShopDataQuery query)
	{
		if (WrappersManagerImpl.IsPatched(49743))
		{
			WrappersManagerImpl.GetPatch(49743).__Gen_Wrap_0(query);
			return;
		}
		Item item = query.GetFitonItem().Find((Item item2) => item2.IsMahjongTable());
		if (item != null && item.model != string.Empty)
		{
			ResourcesLoaderV2.GetInstance(GetTableResourceVersion(item)).LoadAssetBundle(GetDeskProAbName(item.model), null, null, EnmPriority.Normal, EnmResTag.Game);
			Log.Info($"My mahjong table is {item.model}", "Game");
		}
		else
		{
			Log.Info("My mahjong table is null", "Game");
		}
	}

	public static string GetDeskProAbName(string model)
	{
		if (WrappersManagerImpl.IsPatched(21931))
		{
			return WrappersManagerImpl.GetPatch(21931).__Gen_Wrap_19(model);
		}
		string result = model;
		if (model.Contains("|"))
		{
			string[] array = model.Split('|');
			if (array.Length != 0 && !string.IsNullOrEmpty(array[1]))
			{
				result = array[0];
			}
		}
		return result;
	}

	public static string GetDeskProZhuanChangEffect(string model)
	{
		if (WrappersManagerImpl.IsPatched(54986))
		{
			return WrappersManagerImpl.GetPatch(54986).__Gen_Wrap_19(model);
		}
		string result = model;
		if (model.Contains("|"))
		{
			string[] array = model.Split('|');
			if (array.Length != 0 && !string.IsNullOrEmpty(array[1]))
			{
				result = array[1];
			}
		}
		return result;
	}

	public static bool IsMatchUseNewStyleGameScene()
	{
		if (WrappersManagerImpl.IsPatched(41478))
		{
			return WrappersManagerImpl.GetPatch(41478).__Gen_Wrap_11();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("NewStyleSceneInfo", "IsMatchOpen");
	}

	public static bool IsPrivateRoomUseNewStyleGameScene()
	{
		if (WrappersManagerImpl.IsPatched(41477))
		{
			return WrappersManagerImpl.GetPatch(41477).__Gen_Wrap_11();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("NewStyleSceneInfo", "IsPrivateRoomOpen", defaultValue: true);
	}

	public static bool CheckUseNewStyleGameScene()
	{
		if (WrappersManagerImpl.IsPatched(41476))
		{
			return WrappersManagerImpl.GetPatch(41476).__Gen_Wrap_11();
		}
		if (IsPCPlatform())
		{
			return true;
		}
		if (AppRoot.GetInstance().InGameBridgeBase.PreviewGameFunc(0, null))
		{
			return true;
		}
		if (enterGameVO == null || enterGameVO.roomInfo == null)
		{
			Log.Info("IsNewStyeGameScene enterGameVO is null ", "Config");
			return false;
		}
		if (IsPrivateRoom())
		{
			return IsPrivateRoomUseNewStyleGameScene();
		}
		if (DeskIsAllGamesUse())
		{
			return true;
		}
		if (AppRoot.GetInstance().InGameBridgeBase.GameReplayTutorialControllerFunc(2))
		{
			return SingletonBase<DynamicConfig>.GetInstance().GetBool("WorldEffect", "InTutorial", defaultValue: true);
		}
		if (IsMatchRoom())
		{
			return IsMatchUseNewStyleGameScene();
		}
		return enterGameVO.roomInfo.isNewStyleScene;
	}

	public static bool IsNewStyleGameScene()
	{
		if (WrappersManagerImpl.IsPatched(1100))
		{
			return WrappersManagerImpl.GetPatch(1100).__Gen_Wrap_11();
		}
		if (!IsPCPlatform() && !NewSceneManager.IsUseNewStyleGameScene)
		{
			return SceneSwitch.CurrentScene() == SceneSwitch.Scene.TSGame;
		}
		return true;
	}

	public static bool UseNewMahjongMat()
	{
		if (WrappersManagerImpl.IsPatched(41494))
		{
			return WrappersManagerImpl.GetPatch(41494).__Gen_Wrap_11();
		}
		if (IsNewStyleGameScene())
		{
			return GameQualityHelper.mjMaterialLevel == MjMaterialLevel.New;
		}
		return false;
	}

	public static bool TryShowEnterLowerLevelRoomDialog(IDispatcher gameDispatcher, bool isQuickEnterGameAfterCancel, GameType currentGameType, bool isGoldCoinRoom, VoidDelegate closeCallback = null)
	{
		if (WrappersManagerImpl.IsPatched(21620))
		{
			return WrappersManagerImpl.GetPatch(21620).__Gen_Wrap_1179(gameDispatcher, isQuickEnterGameAfterCancel, currentGameType, isGoldCoinRoom, closeCallback);
		}
		IDispatcher gameDispatcher2 = gameDispatcher;
		VoidDelegate closeCallback2 = closeCallback;
		if (!CanShowEnterLowerLevelRoomDialog(isQuickEnterGameAfterCancel, currentGameType, isGoldCoinRoom))
		{
			return false;
		}
		RoomConfig roomInfo = GetQuickGameRoomConfig((int)currentGameType, isEnterNextLevel: false, GetSearchType(isGoldCoinRoom));
		DialogCallbacks dialogCallbacks = new DialogCallbacks();
		dialogCallbacks.ConfirmCallback = delegate
		{
			gameDispatcher2.Dispatch(GameEvent.ExecuteLowerMinBeanContinueCmd, roomInfo);
		};
		dialogCallbacks.CloseCallback = delegate
		{
			gameDispatcher2.Dispatch(GameEvent.ExecuteLowerMinBeanContinueCmd);
			if (closeCallback2 != null)
			{
				closeCallback2();
			}
		};
		gameDispatcher2.Dispatch(GameEvent.ExecuteOverMaxBeanLeaveCmd, roomInfo);
		string hint = string.Format(LanguageKey.INSUFFICIENT_BEANS_THE_SYSTEM_WILL_RECOMMEND_YOU_TO_PLAY_LOW_TIMES_GAMES, isGoldCoinRoom ? LanguageKey.GOLDCOINS : LanguageKey.BEAN_ZI);
		DialogManager.GetInstance().ShowDialogUgui(1, dialogCallbacks, hint, LanguageKey.WELL_1_744);
		return true;
	}

	public static bool CanShowEnterLowerLevelRoomDialog(bool isQuickEnterGameAfterCancel, GameType currentGameType, bool isGoldCoinRoom)
	{
		if (WrappersManagerImpl.IsPatched(21624))
		{
			return WrappersManagerImpl.GetPatch(21624).__Gen_Wrap_1178(isQuickEnterGameAfterCancel, currentGameType, isGoldCoinRoom);
		}
		Log.Info($"TryShowEnterLowerLevelRoomDialog try show! curType: {currentGameType}, isGoldCoinRoom: {isGoldCoinRoom}", "EnterLowerLevelRoom");
		if (!isQuickEnterGameAfterCancel)
		{
			return false;
		}
		long currentCurrencyNum = PlayerDataMgr.GetInstance().GetSelfInfo().GetCurrentCurrencyNum(enterGameVO.roomInfo);
		Log.Info($"TryShowEnterLowerLevelRoomDialog currencyNum: {currentCurrencyNum}, sceneId:{enterGameVO.roomInfo.sceneId} ", "EnterLowerLevelRoom");
		if (currentCurrencyNum < 2000)
		{
			return false;
		}
		bool flag = SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomSectionScore || SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomSectionScoreAndBean;
		RoomConfig quickGameRoomConfig = GetQuickGameRoomConfig((int)currentGameType, isEnterNextLevel: false, GetSearchType(isGoldCoinRoom));
		if (quickGameRoomConfig == null)
		{
			return false;
		}
		Log.Info("TryShowEnterLowerLevelRoomDialog sceneID: " + quickGameRoomConfig.sceneId, "EnterLowerLevelRoom");
		if (quickGameRoomConfig.sceneId == enterGameVO.roomInfo.sceneId || (flag && quickGameRoomConfig.ShowRankMode != 1) || (!flag && quickGameRoomConfig.ShowRankMode == 1))
		{
			return false;
		}
		return true;
	}

	public static bool IsUseNewRankDesk()
	{
		if (WrappersManagerImpl.IsPatched(20584))
		{
			return WrappersManagerImpl.GetPatch(20584).__Gen_Wrap_11();
		}
		if (!DeskIsAllGamesUse() && !IsNewStyleGameScene())
		{
			return false;
		}
		return true;
	}

	public static bool IsUseNewArrange(int sceneId)
	{
		if (WrappersManagerImpl.IsPatched(34437))
		{
			return WrappersManagerImpl.GetPatch(34437).__Gen_Wrap_172(sceneId);
		}
		List<int> listInt = SingletonBase<DynamicConfig>.GetInstance().GetListInt("NewArrange", "SceneList");
		List<int> listInt2 = SingletonBase<DynamicConfig>.GetInstance().GetListInt("NewArrange", "BlockSceneList");
		if (listInt2 != null && listInt2.Contains(sceneId))
		{
			return false;
		}
		return listInt?.Contains(sceneId) ?? true;
	}

	public static void SetScrollSensitivity(GameObject obj, float sensitivity)
	{
		if (WrappersManagerImpl.IsPatched(32164))
		{
			WrappersManagerImpl.GetPatch(32164).__Gen_Wrap_28(obj, sensitivity);
		}
		else if ((bool)obj)
		{
			ScrollRect[] componentsInChildren = obj.GetComponentsInChildren<ScrollRect>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].scrollSensitivity = sensitivity;
			}
		}
	}

	public static bool IsDeskProLODEnabled()
	{
		if (WrappersManagerImpl.IsPatched(55303))
		{
			return WrappersManagerImpl.GetPatch(55303).__Gen_Wrap_11();
		}
		if (Device.GetInstance().OrigType == DeviceType.SuperLow)
		{
			return true;
		}
		int playerPrefsOrRecommendQualityLevel = QualityHelper.GetPlayerPrefsOrRecommendQualityLevel();
		double benchmarkScore = Device.GetInstance().GetBenchmarkScore();
		if (benchmarkScore >= 50.0 || (benchmarkScore < 50.0 && benchmarkScore > 35.0 && playerPrefsOrRecommendQualityLevel >= 2))
		{
			return false;
		}
		return true;
	}

	public static bool IsEmulator()
	{
		if (WrappersManagerImpl.IsPatched(1350))
		{
			return WrappersManagerImpl.GetPatch(1350).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsEmulator();
	}

	public static bool IsAndroidEmulator()
	{
		if (WrappersManagerImpl.IsPatched(5388))
		{
			return WrappersManagerImpl.GetPatch(5388).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsAndroidEmulator();
	}

	public static bool IsMacOSEditor()
	{
		if (WrappersManagerImpl.IsPatched(48802))
		{
			return WrappersManagerImpl.GetPatch(48802).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.IsMacOSPlatform();
	}

	public static bool IsInMatchingStage()
	{
		if (WrappersManagerImpl.IsPatched(21208))
		{
			return WrappersManagerImpl.GetPatch(21208).__Gen_Wrap_11();
		}
		if (SceneSwitch.CurrentScene() != SceneSwitch.Scene.Game)
		{
			return false;
		}
		GameObject gameObject = GameObject.Find("GameUIRoot");
		if (gameObject == null)
		{
			return false;
		}
		GameObject gameObject2 = gameObject.transform.Find("UI Root/System_icons").gameObject;
		GameObject sceneLoaderObj = GameSceneNodeFinder.GetSceneLoaderObj();
		Transform transform = null;
		if (sceneLoaderObj != null)
		{
			transform = sceneLoaderObj.transform.Find("Scene");
		}
		if (!gameObject2.activeInHierarchy && transform != null && transform.gameObject.activeInHierarchy)
		{
			return true;
		}
		return false;
	}

	public static bool IsInGameMatchingStage()
	{
		if (WrappersManagerImpl.IsPatched(53683))
		{
			return WrappersManagerImpl.GetPatch(53683).__Gen_Wrap_11();
		}
		if (SceneSwitch.CurrentScene() != SceneSwitch.Scene.Game)
		{
			return false;
		}
		if (IsInMatchingStage())
		{
			return true;
		}
		return s_isShowingMatchingLoading;
	}

	public static void SetMatchingLoadingShowing(bool showing)
	{
		if (WrappersManagerImpl.IsPatched(61921))
		{
			WrappersManagerImpl.GetPatch(61921).__Gen_Wrap_192(showing);
		}
		else
		{
			s_isShowingMatchingLoading = showing;
		}
	}

	public static bool GetWhiteListShopResFlag()
	{
		if (WrappersManagerImpl.IsPatched(20911))
		{
			return WrappersManagerImpl.GetPatch(20911).__Gen_Wrap_11();
		}
		return HappyMahjong.Core.Common.Util.GetWhiteListShopResFlag();
	}

	public static bool Get18813TimeoutFlag()
	{
		if (WrappersManagerImpl.IsPatched(54369))
		{
			return WrappersManagerImpl.GetPatch(54369).__Gen_Wrap_11();
		}
		if (!SingletonBase<HappyBridge.Util.PlayerDataMgr>.GetInstance().GetIsActivityWhiteList())
		{
			return false;
		}
		if (PlayerPrefs.GetInt("18813_TIMEOUT", 0) != 1)
		{
			return false;
		}
		return true;
	}

	[IDTag(0)]
	public static string GetCountStr(long count)
	{
		if (WrappersManagerImpl.IsPatched(21735))
		{
			return WrappersManagerImpl.GetPatch(21735).__Gen_Wrap_1190(count);
		}
		string text = "";
		if (count >= 10000)
		{
			if (count % 10000 != 0L)
			{
				return string.Format(LanguageKey.ANY_TEN_THOUSAND, ((float)count / 10000f).ToString("0.0"));
			}
			return string.Format(LanguageKey.ANY_TEN_THOUSAND, (count / 10000).ToString());
		}
		return count.ToString();
	}

	public static string GetFengDingCountStr(float fengDingNum)
	{
		if (WrappersManagerImpl.IsPatched(36894))
		{
			return WrappersManagerImpl.GetPatch(36894).__Gen_Wrap_2501(fengDingNum);
		}
		string text = "";
		if (fengDingNum >= 10000f)
		{
			if (fengDingNum % 10000f == 0f)
			{
				return $"{(int)fengDingNum / 10000}万";
			}
			if (fengDingNum % 1000f == 0f)
			{
				return string.Format("{0}万", (fengDingNum / 10000f).ToString("F1"));
			}
			if (fengDingNum % 100f == 0f)
			{
				return string.Format("{0}万", (fengDingNum / 10000f).ToString("F2"));
			}
			if (fengDingNum % 10f == 0f)
			{
				return string.Format("{0}万", (fengDingNum / 10000f).ToString("F3"));
			}
			return string.Format("{0}万", (fengDingNum / 10000f).ToString("F4"));
		}
		return fengDingNum.ToString();
	}

	public static bool IsLiveBuild()
	{
		if (WrappersManagerImpl.IsPatched(33310))
		{
			return WrappersManagerImpl.GetPatch(33310).__Gen_Wrap_11();
		}
		return true;
	}

	public static bool UseProtoTingAdvice()
	{
		if (WrappersManagerImpl.IsPatched(2024))
		{
			return WrappersManagerImpl.GetPatch(2024).__Gen_Wrap_11();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("TingAdvice", "UseProto", defaultValue: true);
	}

	private static bool IsAppInstalledWrapper(string channel)
	{
		if (WrappersManagerImpl.IsPatched(1145))
		{
			return WrappersManagerImpl.GetPatch(1145).__Gen_Wrap_3(channel);
		}
		if (IsOpenHarmonyPlatform())
		{
			return false;
		}
		return MSDKTools.IsAppInstalled(channel);
	}

	public static bool IsAppInstalled(string channel)
	{
		if (WrappersManagerImpl.IsPatched(1144))
		{
			return WrappersManagerImpl.GetPatch(1144).__Gen_Wrap_3(channel);
		}
		bool flag = channel == "QQ";
		bool flag2 = channel == "WeChat";
		bool result = false;
		if (IsIOSPlatform() || IsOpenHarmonyPlatform())
		{
			if (flag)
			{
				result = IsAppInstalledWrapper("QQ");
			}
			else if (flag2)
			{
				result = IsAppInstalledWrapper("WeChat");
			}
		}
		else if (flag)
		{
			result = MSDKTools.IsAppInstalled(GetQQAppPackageName());
		}
		else if (flag2)
		{
			result = MSDKTools.IsAppInstalled(GetWXAppPackageName());
		}
		return result;
	}

	public static bool IsClientRequestTing()
	{
		if (WrappersManagerImpl.IsPatched(2023))
		{
			return WrappersManagerImpl.GetPatch(2023).__Gen_Wrap_11();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("TingAdvice", "ClientRequest", defaultValue: true);
	}

	public static bool UseFirstTingScoreSplit()
	{
		if (WrappersManagerImpl.IsPatched(2025))
		{
			return WrappersManagerImpl.GetPatch(2025).__Gen_Wrap_11();
		}
		return SingletonBase<DynamicConfig>.GetInstance().GetBool("FirstTingScore", "isSplitOpen", defaultValue: true);
	}

	public static void CopyToClipboard(string str)
	{
		if (WrappersManagerImpl.IsPatched(53869))
		{
			WrappersManagerImpl.GetPatch(53869).__Gen_Wrap_0(str);
			return;
		}
		Log.Info("CopyToClipboard: " + str, "Framework");
		if (!string.IsNullOrEmpty(str))
		{
			GUIUtility.systemCopyBuffer = str;
		}
	}

	public static string GetMD5Hash(string input)
	{
		if (WrappersManagerImpl.IsPatched(920))
		{
			return WrappersManagerImpl.GetPatch(920).__Gen_Wrap_19(input);
		}
		using MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static Dictionary<string, string> GetKeyValueDict(string rawStr, char secSplit = '&', char KeyValueSplit = '=')
	{
		if (WrappersManagerImpl.IsPatched(61922))
		{
			return WrappersManagerImpl.GetPatch(61922).__Gen_Wrap_4167(rawStr, secSplit, KeyValueSplit);
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = SplitWithoutEmpty(rawStr, secSplit);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = SplitWithoutEmpty(array[i], KeyValueSplit);
			if (array2.Length >= 2)
			{
				dictionary.Add(array2[0], array2[1]);
			}
		}
		return dictionary;
	}

	public static string[] SplitWithoutEmpty(string rawStr, char token = '&')
	{
		if (WrappersManagerImpl.IsPatched(61923))
		{
			return WrappersManagerImpl.GetPatch(61923).__Gen_Wrap_4166(rawStr, token);
		}
		return rawStr.Split(new char[1] { token }, StringSplitOptions.RemoveEmptyEntries);
	}

	public static string ReplaceSpaceCharacter(string orignStr)
	{
		if (WrappersManagerImpl.IsPatched(41681))
		{
			return WrappersManagerImpl.GetPatch(41681).__Gen_Wrap_19(orignStr);
		}
		if (orignStr.Contains(" "))
		{
			return orignStr.Replace(" ", "\u00a0");
		}
		return orignStr;
	}

	public static void LogOut()
	{
		if (WrappersManagerImpl.IsPatched(48842))
		{
			WrappersManagerImpl.GetPatch(48842).__Gen_Wrap_46();
		}
		else if (IsAndroidPlatform() || IsIOSPlatform())
		{
			MSDKLogin.Logout();
		}
	}

	public static string FormatWithStringBuilder(StringBuilder sb, params object[] args)
	{
		if (WrappersManagerImpl.IsPatched(21766))
		{
			return WrappersManagerImpl.GetPatch(21766).__Gen_Wrap_52(sb, args);
		}
		if (sb == null)
		{
			Log.Error("StringBuilder cannot be null.", "EventReport");
		}
		if (args == null)
		{
			Log.Error("Args cannot be null.", "EventReport");
		}
		try
		{
			foreach (object obj in args)
			{
				if (obj == null)
				{
					sb.Append("null");
				}
				else
				{
					sb.Append(obj);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("An error occurred: " + ex.Message, "EventReport");
		}
		return sb.ToString();
	}

	public static bool IsDeskProBtnInMenuPanal(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(61924))
		{
			return WrappersManagerImpl.GetPatch(61924).__Gen_Wrap_2318(gameType);
		}
		return true;
	}

	public static bool IsLowDeviceIgnoreDeskProEffect()
	{
		if (WrappersManagerImpl.IsPatched(2034))
		{
			return WrappersManagerImpl.GetPatch(2034).__Gen_Wrap_11();
		}
		return Device.GetInstance().GetBenchmarkScore() < (double)SingletonBase<DynamicConfig>.GetInstance().GetInt("DeskProShareInfo", "LowDeviceScore", 50);
	}

	public static bool CheckSupportRealtimeVoice(int clientType)
	{
		if (WrappersManagerImpl.IsPatched(34034))
		{
			return WrappersManagerImpl.GetPatch(34034).__Gen_Wrap_172(clientType);
		}
		if (clientType == 64 || clientType == 13 || clientType == 70)
		{
			return true;
		}
		return false;
	}

	public static string GetUserPosNameByViewSeat(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(44228))
		{
			return WrappersManagerImpl.GetPatch(44228).__Gen_Wrap_12(viewSeat);
		}
		return viewSeat switch
		{
			1 => LanguageKey.DOWN_HOME, 
			2 => LanguageKey.FOR_HOME, 
			3 => LanguageKey.UP_HOME, 
			_ => LanguageKey.SELF_HOME, 
		};
	}

	public static GameType GetGameTypeByXueLiuBaoPaiSelect(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(37031))
		{
			return WrappersManagerImpl.GetPatch(37031).__Gen_Wrap_2069(gameType);
		}
		if (IsGamteTypeXueLiuBaoPai(gameType))
		{
			return (GameType)SingletonBase<DynamicConfig>.GetInstance().GetInt("XueLiuBaoPai", "ShowGuideGameRoomSelect", 321);
		}
		return gameType;
	}

	public static bool IsGamteTypeXueLiuBaoPai(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(37032))
		{
			return WrappersManagerImpl.GetPatch(37032).__Gen_Wrap_2318(gameType);
		}
		GameType gameType2 = (GameType)SingletonBase<DynamicConfig>.GetInstance().GetInt("XueLiuBaoPai", "ShowGuideGameRooms", 379);
		if (gameType == gameType2)
		{
			return true;
		}
		return false;
	}

	public static GameTutorialType GetTutorialType(GameTutorialType tutorialType)
	{
		if (WrappersManagerImpl.IsPatched(61925))
		{
			return WrappersManagerImpl.GetPatch(61925).__Gen_Wrap_4168(tutorialType);
		}
		switch (tutorialType)
		{
		case GameTutorialType.XueLiu:
			return GameTutorialType.XueLiuBaoPai;
		case GameTutorialType.DaZhongV2:
			if (!SingletonBase<DynamicConfig>.GetInstance().GetBool("TutorialOpenCtrl", "NewDaZhong"))
			{
				return GameTutorialType.DaZhong;
			}
			break;
		}
		return tutorialType;
	}

	public static bool IsGuideGameRoomSelect(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(56954))
		{
			return WrappersManagerImpl.GetPatch(56954).__Gen_Wrap_172(gameType);
		}
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("XueLiuBaoPai", "ShowGuideGameRoomSelect", 321);
		if (gameType == num)
		{
			return true;
		}
		return false;
	}

	public static List<int> ParseIntFromHongZhongTagStr(string str)
	{
		if (WrappersManagerImpl.IsPatched(44854))
		{
			return WrappersManagerImpl.GetPatch(44854).__Gen_Wrap_142(str);
		}
		if (string.IsNullOrEmpty(str))
		{
			return new List<int>();
		}
		return (from item in str.Split(';')
			select int.Parse(item)).ToList();
	}

	public static string GetHongZhongRoomTagContentByIndex(int index)
	{
		if (WrappersManagerImpl.IsPatched(44856))
		{
			return WrappersManagerImpl.GetPatch(44856).__Gen_Wrap_12(index);
		}
		return index switch
		{
			0 => "任意换", 
			1 => "发财选牌", 
			2 => "真癞子", 
			3 => "红中单吊", 
			4 => "测试服", 
			_ => "", 
		};
	}

	public static bool IsTouchOnUI(Vector2 position)
	{
		if (WrappersManagerImpl.IsPatched(21211))
		{
			return WrappersManagerImpl.GetPatch(21211).__Gen_Wrap_1116(position);
		}
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = position
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		return list.Count > 1;
	}

	public static string UinEncode(uint uin)
	{
		if (WrappersManagerImpl.IsPatched(36042))
		{
			return WrappersManagerImpl.GetPatch(36042).__Gen_Wrap_2422(uin);
		}
		if (IsAi(uin, 1466242167u, 1466642163u, 4u))
		{
			return Encode(uin.ToString());
		}
		return uin.ToString();
	}

	public static string UinDecode(string encodeUin)
	{
		if (WrappersManagerImpl.IsPatched(36047))
		{
			return WrappersManagerImpl.GetPatch(36047).__Gen_Wrap_19(encodeUin);
		}
		return Decode(encodeUin);
	}

	public static string Encode(string number)
	{
		if (WrappersManagerImpl.IsPatched(36045))
		{
			return WrappersManagerImpl.GetPatch(36045).__Gen_Wrap_19(number);
		}
		int length = number.Length;
		char[] array = new char[length];
		int num = -1;
		int num2 = -1;
		(int, int) key = GetKey(number);
		num2 = key.Item1;
		num = key.Item2;
		for (int i = 0; i < length; i++)
		{
			int num3 = number[i] - 48;
			if (i == 0)
			{
				num3 = (num3 + num) % 4;
				array[i] = (char)(num3 + 48);
			}
			else if (i >= length - 5)
			{
				array[i] = number[i];
			}
			else
			{
				num3 = (num3 + num2) % 10;
				array[i] = (char)(num3 + 48);
			}
		}
		return new string(array);
	}

	public static string Decode(string encodedNumber)
	{
		if (WrappersManagerImpl.IsPatched(36048))
		{
			return WrappersManagerImpl.GetPatch(36048).__Gen_Wrap_19(encodedNumber);
		}
		int length = encodedNumber.Length;
		char[] array = new char[length];
		int num = -1;
		int num2 = -1;
		(int, int) key = GetKey(encodedNumber);
		num2 = key.Item1;
		num = key.Item2;
		for (int i = 0; i < length; i++)
		{
			int num3 = encodedNumber[i] - 48;
			if (i == 0)
			{
				num3 = (num3 - num) % 4;
				array[i] = (char)(num3 + 48);
			}
			else if (i >= length - 5)
			{
				array[i] = encodedNumber[i];
			}
			else
			{
				num3 = (num3 - num2 + 10) % 10;
				array[i] = (char)(num3 + 48);
			}
		}
		return new string(array);
	}

	private static (int, int) GetKey(string number)
	{
		if (WrappersManagerImpl.IsPatched(36046))
		{
			return WrappersManagerImpl.GetPatch(36046).__Gen_Wrap_2421(number);
		}
		int num = -1;
		int num2 = -1;
		for (int num3 = number.Length - 1; num3 >= number.Length - 5; num3--)
		{
			int num4 = number[num3] - 48;
			if (num2 == -1 && num4 != 0)
			{
				num2 = num4;
			}
			if (num == -1 && num4 < 3)
			{
				num = num4;
			}
		}
		if (num == -1)
		{
			num = 2;
		}
		if (num2 == -1)
		{
			num2 = 5;
		}
		return (num2, num);
	}

	public static string ShuffleString(string str)
	{
		if (WrappersManagerImpl.IsPatched(61926))
		{
			return WrappersManagerImpl.GetPatch(61926).__Gen_Wrap_19(str);
		}
		char[] array = str.ToCharArray();
		char[] array2 = new char[array.Length];
		List<int> list = m_9destNumPos;
		if (array.Length == 10)
		{
			list = m_10destNumPos;
		}
		else if (array.Length == 11)
		{
			list = m_11destNumPos;
		}
		int num = 0;
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			num = list[num2];
			array2[num] = array[num2];
		}
		return new string(array2);
	}

	public static string RestoreString(string shuffledStr)
	{
		if (WrappersManagerImpl.IsPatched(61927))
		{
			return WrappersManagerImpl.GetPatch(61927).__Gen_Wrap_19(shuffledStr);
		}
		char[] array = shuffledStr.ToCharArray();
		char[] array2 = new char[array.Length];
		List<int> list = m_9destNumPos;
		if (array.Length == 10)
		{
			list = m_10destNumPos;
		}
		else if (array.Length == 11)
		{
			list = m_11destNumPos;
		}
		int num = 0;
		for (int num2 = array.Length - 1; num2 >= 0; num2--)
		{
			num = list.IndexOf(num2);
			array2[num] = array[num2];
		}
		return new string(array2);
	}

	public static bool IsAi(uint uin, uint iAISeqStart, uint iAISeqEnd, uint iStep)
	{
		if (WrappersManagerImpl.IsPatched(36043))
		{
			return WrappersManagerImpl.GetPatch(36043).__Gen_Wrap_2420(uin, iAISeqStart, iAISeqEnd, iStep);
		}
		uint num = GameUinToSeq(uin);
		if (iStep == 0)
		{
			return false;
		}
		if (num > iAISeqEnd)
		{
			return false;
		}
		if (num < iAISeqStart)
		{
			return false;
		}
		if ((num - iAISeqStart) % iStep != 0)
		{
			return false;
		}
		return true;
	}

	public static uint GameUinToSeq(uint uin)
	{
		if (WrappersManagerImpl.IsPatched(36044))
		{
			return WrappersManagerImpl.GetPatch(36044).__Gen_Wrap_2419(uin);
		}
		return (uin & 0xFF000000u) | ((uin & 0xFFFF) << 8) | ((uin & 0xFF0000) >> 16);
	}

	public static uint GetUin(uint num)
	{
		if (WrappersManagerImpl.IsPatched(61928))
		{
			return WrappersManagerImpl.GetPatch(61928).__Gen_Wrap_2419(num);
		}
		return (num & 0xFF000000u) | ((num & 0xFFFF00) >> 8) | ((num & 0xFF) << 16);
	}

	public static void SendMsg2GameSvr(short msgId, byte[] msgBuffer, int msgLen, uint unInterval = 8000u)
	{
		if (WrappersManagerImpl.IsPatched(2240))
		{
			WrappersManagerImpl.GetPatch(2240).__Gen_Wrap_501(msgId, msgBuffer, msgLen, unInterval);
			return;
		}
		Log.Info("SendMsg2GameSvr " + msgId, "Framework");
		CMsg cMsg = msg;
		CUnknowMsgPara cUnknowMsgPara = unKnowMsgParam;
		if (cMsg == null)
		{
			cMsg = (msg = new CMsg());
		}
		stPlayerBaseInfo selfInfoData = TSDK4CSharp.TSDKService.GetSelfInfoData();
		stTsdkBaseInfo tSDKGameData = TSDK4CSharp.TSDKService.GetTSDKGameData();
		cMsg.shMsgID = msgId;
		cMsg.shMsgType = ENM_SVRMSGTYPE.eumSvrMsgTypeRequest;
		cMsg.bSrcFE = ENM_FUNCTIONALENTITYTYPE.eumFuncTypeClient;
		cMsg.bDstFE = ENM_FUNCTIONALENTITYTYPE.eumFuncTypeRoom;
		cMsg.shSrcID = (short)selfInfoData.unPlayerId;
		cMsg.shDstID = tSDKGameData.shRoomID;
		if (cUnknowMsgPara == null)
		{
			cUnknowMsgPara = (unKnowMsgParam = new CUnknowMsgPara());
		}
		cUnknowMsgPara.setDecodeBuf((short)msgLen, msgBuffer);
		cMsg.pMsgPara = cUnknowMsgPara;
		TSDK4CSharp.TSDKService.SendMsg(cMsg, unInterval);
	}

	public static bool IsUinOpen(string abilityName, float enableRange, int openCurrentVersion)
	{
		if (WrappersManagerImpl.IsPatched(61929))
		{
			return WrappersManagerImpl.GetPatch(61929).__Gen_Wrap_3992(abilityName, enableRange, openCurrentVersion);
		}
		_ = string.Empty;
		if (CrashSightEntry.IsWhiteUser)
		{
			string message = "IsAbilityOpen " + CrashSightEntry.LastLoginUin + " isWhiteUser!";
			Log.Info(message, abilityName);
			UnityEngine.Debug.Log(message);
			return true;
		}
		switch (openCurrentVersion)
		{
		case 1:
			Log.Info("openCurrentVersion: True!", abilityName);
			UnityEngine.Debug.Log("openCurrentVersion: True!");
			return true;
		case 0:
			Log.Info("openCurrentVersion: False!", abilityName);
			UnityEngine.Debug.Log("openCurrentVersion: False!");
			return false;
		default:
		{
			string lastLoginUin = CrashSightEntry.LastLoginUin;
			if (string.IsNullOrEmpty(lastLoginUin) || enableRange < 0f)
			{
				return CrashSightEntry.CheckRandomNumber(enableRange);
			}
			string text = SingletonBase<DynamicConfig>.GetInstance().GetString("CrashSightConfig", "OpenUinList", "2269402364,454468738");
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(',');
				if (array != null && array.Length != 0)
				{
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						if (array2[i].Equals(lastLoginUin))
						{
							return true;
						}
					}
				}
			}
			float randomProbability = CrashSightEntry.GetRandomProbability(lastLoginUin);
			string message2 = $"IsAbilityOpen uin: {lastLoginUin}, probability: {randomProbability}";
			Log.Info(message2, abilityName);
			UnityEngine.Debug.Log(message2);
			return randomProbability < enableRange;
		}
		}
	}

	public static void DestroyAllChildren(Transform parent)
	{
		if (WrappersManagerImpl.IsPatched(38135))
		{
			WrappersManagerImpl.GetPatch(38135).__Gen_Wrap_0(parent);
			return;
		}
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.DestroyImmediate(parent.GetChild(num).gameObject);
		}
	}

	public static int ActiveChildCount(Transform parent)
	{
		if (WrappersManagerImpl.IsPatched(58419))
		{
			return WrappersManagerImpl.GetPatch(58419).__Gen_Wrap_85(parent);
		}
		int num = 0;
		if (parent != null)
		{
			for (int num2 = parent.childCount - 1; num2 >= 0; num2--)
			{
				if (parent.GetChild(num2).gameObject.activeSelf)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static void TeamBattleLogoutWhenGameEnd()
	{
		if (WrappersManagerImpl.IsPatched(61930))
		{
			WrappersManagerImpl.GetPatch(61930).__Gen_Wrap_46();
			return;
		}
		VPTimer.In(0.5f, delegate
		{
			if (PlayerDataMgr.GetInstance().GetSelfPlayerStatus() != PlayerClientStatus.LoginMSDK && PlayerDataMgr.GetInstance().GetSelfPlayerStatus() != PlayerClientStatus.NotLoginMSDK)
			{
				Log.Info("2V2对局结束退房", "Game");
				SendLeaveRoomRequest();
			}
		});
	}

	public static void RestartApp()
	{
		if (WrappersManagerImpl.IsPatched(36764))
		{
			WrappersManagerImpl.GetPatch(36764).__Gen_Wrap_46();
		}
		else
		{
			RestartIOS();
		}
	}

	private static void RestartEditor()
	{
		if (WrappersManagerImpl.IsPatched(61932))
		{
			WrappersManagerImpl.GetPatch(61932).__Gen_Wrap_46();
		}
	}

	private static void RestartPC()
	{
		if (WrappersManagerImpl.IsPatched(61933))
		{
			WrappersManagerImpl.GetPatch(61933).__Gen_Wrap_46();
		}
	}

	private static void RestartEmulator()
	{
		if (WrappersManagerImpl.IsPatched(61934))
		{
			WrappersManagerImpl.GetPatch(61934).__Gen_Wrap_46();
		}
	}

	private static void RestartAndroid()
	{
		if (WrappersManagerImpl.IsPatched(61935))
		{
			WrappersManagerImpl.GetPatch(61935).__Gen_Wrap_46();
		}
	}

	private static void RestartIOS()
	{
		if (WrappersManagerImpl.IsPatched(36765))
		{
			WrappersManagerImpl.GetPatch(36765).__Gen_Wrap_46();
		}
		else
		{
			Application.Quit();
		}
	}

	private static void RestartOpenHarmony()
	{
		if (WrappersManagerImpl.IsPatched(61936))
		{
			WrappersManagerImpl.GetPatch(61936).__Gen_Wrap_46();
		}
	}

	public static void SetMSDKData()
	{
		if (WrappersManagerImpl.IsPatched(48804))
		{
			WrappersManagerImpl.GetPatch(48804).__Gen_Wrap_46();
		}
		else if (IsAndroidPlatform())
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			string value = androidJavaObject.Call<string>("getIMei", Array.Empty<object>());
			string value2 = androidJavaObject.Call<string>("getAndroid", Array.Empty<object>());
			string value3 = androidJavaObject.Call<string>("getWifiMac", Array.Empty<object>());
			androidJavaObject.Call("sortMarketApkListWhenInit");
			androidJavaClass.Dispose();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{\"AndroidID\":\"");
			stringBuilder.Append(value2);
			stringBuilder.Append("\", \"WiFiMacAddress\":\"");
			stringBuilder.Append(value3);
			stringBuilder.Append("\", \"Imei\":\"");
			stringBuilder.Append(value);
			stringBuilder.Append("\"}");
			Log.Info("Android SetSensitiveInfo", "MainRoot");
			MSDKSensitive.SetCouldCollectSensitiveInfo(couldCollect: true);
			MSDKSensitive.SetSensitiveInfo(stringBuilder.ToString());
			Log.Info("SetSensitiveInfo " + stringBuilder.ToString(), "MainRoot");
		}
	}

	public static void OpenMiniApp(bool isRelease, int targetScene, int gameType, int sceneId = 26)
	{
		if (WrappersManagerImpl.IsPatched(33315))
		{
			WrappersManagerImpl.GetPatch(33315).__Gen_Wrap_2072(isRelease, targetScene, gameType, sceneId);
			return;
		}
		string openId = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetOpenId();
		string accessToken = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetAccessToken();
		string text = WWW.EscapeURL(openId);
		string text2 = WWW.EscapeURL(accessToken);
		MSDKFriendReqInfo mSDKFriendReqInfo = new MSDKFriendReqInfo();
		mSDKFriendReqInfo.Type = 10007;
		LoginType loginType = SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginType();
		int num = 0;
		long num2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		uint playerUin = PlayerDataMgr.GetInstance().GetSelfInfo().playerUin;
		if (gameType == 124)
		{
			gameType = 125;
		}
		int num3 = gameType;
		if (targetScene != 0 && gameType == 0)
		{
			Log.Info($"OpenMiniApp targetScene {targetScene}  gametype is:zero, use default", "AppletGuide");
			num3 = 150;
		}
		switch (loginType)
		{
		case LoginType.LoginByWeiXin:
		{
			num = ((!isRelease) ? 2 : 0);
			if (IsLiveBuild() && GMUtil.IsServerWhiteUinUser())
			{
				num = ((PlayerPrefsHelper.HasKey("GM_AppletGuide_Dev") && PlayerPrefsHelper.GetInt("GM_AppletGuide_Dev") == 1) ? 2 : 0);
			}
			string text4 = $"pages/home/index?appGuide=1&uin={playerUin}&scene_id={sceneId}&open_id={text}&access_token={text2}&targetScene={targetScene}&gameType={num3}&handleCode={num2}";
			Log.Info("OpenMiniApp path is: " + WWW.UnEscapeURL(text4), "TSDKServices");
			mSDKFriendReqInfo.MediaPath = text4;
			mSDKFriendReqInfo.ExtraJson = string.Format("{{\"weapp_id\":\"{0}\",\"with_share_ticket\":1,\"mini_program_type\":{1}}}", "gh_a33468880529", num);
			MSDKFriend.SendMessage(mSDKFriendReqInfo, "WeChat");
			break;
		}
		case LoginType.LoginByQQ:
		{
			num = ((!IsIOSPlatform()) ? ((!isRelease) ? 2 : 0) : ((!isRelease) ? 1 : 3));
			if (IsLiveBuild() && GMUtil.IsServerWhiteUinUser())
			{
				bool flag = PlayerPrefsHelper.HasKey("GM_AppletGuide_Dev") && PlayerPrefsHelper.GetInt("GM_AppletGuide_Dev") == 1;
				num = ((!IsIOSPlatform()) ? (flag ? 2 : 0) : (flag ? 1 : 3));
			}
			string text3 = $"pages/home/index?appGuide=2&uin={playerUin}&scene_id={sceneId}&open_id={text}&access_token={text2}&targetScene={targetScene}&gameType={num3}&handleCode={num2}";
			mSDKFriendReqInfo.ExtraJson = string.Format("{{\"mini_appid\":\"{0}\",\"mini_path\":\"{1}\",\"with_share_ticket\":1,\"mini_program_type\":{2}}}", "1108840788", text3, num);
			Log.Info("OpenMiniApp path is: " + WWW.UnEscapeURL(text3), "TSDKServices");
			MSDKFriend.SendMessage(mSDKFriendReqInfo, "QQ");
			break;
		}
		}
	}

	public static StringBuilder AcquireSB()
	{
		if (WrappersManagerImpl.IsPatched(61937))
		{
			return WrappersManagerImpl.GetPatch(61937).__Gen_Wrap_4169();
		}
		if (s_pooledSBInUse || s_pooledSB == null)
		{
			if (s_pooledSB == null)
			{
				s_pooledSB = new StringBuilder(256);
				s_pooledSBInUse = true;
				return s_pooledSB;
			}
			return new StringBuilder(256);
		}
		s_pooledSBInUse = true;
		s_pooledSB.Length = 0;
		return s_pooledSB;
	}

	public static void ReleaseSB(StringBuilder sb)
	{
		if (WrappersManagerImpl.IsPatched(61938))
		{
			WrappersManagerImpl.GetPatch(61938).__Gen_Wrap_0(sb);
		}
		else if (sb != null && sb == s_pooledSB)
		{
			if (sb.Capacity > 4096)
			{
				s_pooledSB = null;
			}
			else
			{
				sb.Length = 0;
			}
			s_pooledSBInUse = false;
		}
	}

	public static string JoinToString<T>(IList<T> list, string separator = ", ", int maxCount = -1)
	{
		if (list == null || list.Count == 0)
		{
			return string.Empty;
		}
		if (list.Count == 1)
		{
			if (list[0] == null)
			{
				return string.Empty;
			}
			return list[0].ToString();
		}
		int count = list.Count;
		int num = ((maxCount > 0 && count > maxCount) ? maxCount : count);
		StringBuilder stringBuilder = AcquireSB();
		try
		{
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(separator);
				}
				T val = list[i];
				if (val != null)
				{
					stringBuilder.Append(val.ToString());
				}
			}
			if (num < count)
			{
				stringBuilder.Append(separator);
				stringBuilder.Append("...(+").Append(count - num).Append(" more)");
			}
			return stringBuilder.ToString();
		}
		finally
		{
			ReleaseSB(stringBuilder);
		}
	}

	public static bool IsHongZhongXueLiu(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(59491))
		{
			return WrappersManagerImpl.GetPatch(59491).__Gen_Wrap_2318(gameType);
		}
		if (gameType != GameType.ModuleTuidaohu60 && gameType != GameType.ModuleTuidaohu97 && gameType != GameType.ModuleHongZhongXueLiu && gameType != GameType.ModuleBaoPaiMahjong && gameType != GameType.ModuleUniversalXueliu && gameType != GameType.ModuleHuoDong && gameType != GameType.ModuleSecondaryTileXueliu && gameType != GameType.ModuleBazhangpai)
		{
			return gameType == GameType.ModuleHongZhongXueZhan;
		}
		return true;
	}

	public static string GetBeanStringWithSymbol(long mBeanNum, int charNumLimit = 5, BeanStringFormat format = BeanStringFormat.Normal)
	{
		if (WrappersManagerImpl.IsPatched(52058))
		{
			return WrappersManagerImpl.GetPatch(52058).__Gen_Wrap_3527(mBeanNum, charNumLimit, format);
		}
		string empty = string.Empty;
		if (mBeanNum > 0)
		{
			return "+" + GetBeanString(mBeanNum, charNumLimit, format);
		}
		if (mBeanNum == 0L)
		{
			return "0";
		}
		long value = Math.Abs(mBeanNum);
		return "-" + GetBeanString(value, charNumLimit, format);
	}

	[IDTag(1)]
	public static string GetBeanString(long? mBeanNum, int charNumLimit = 5)
	{
		if (WrappersManagerImpl.IsPatched(4731))
		{
			return WrappersManagerImpl.GetPatch(4731).__Gen_Wrap_768(mBeanNum, charNumLimit);
		}
		return GetBeanString(mBeanNum, charNumLimit, BeanStringFormat.Normal);
	}

	[IDTag(0)]
	public static string GetBeanString(long? mBeanNum, int charNumLimit, BeanStringFormat format)
	{
		if (WrappersManagerImpl.IsPatched(4732))
		{
			return WrappersManagerImpl.GetPatch(4732).__Gen_Wrap_767(mBeanNum, charNumLimit, format);
		}
		string text = "";
		string text2 = "";
		string text3 = ((mBeanNum >= 0) ? "" : "-");
		mBeanNum = Math.Abs(mBeanNum.Value);
		int num = ((format == BeanStringFormat.WanExtend) ? 100000 : 10000);
		if (mBeanNum >= num && mBeanNum < 100000000)
		{
			text2 = LanguageKey.TEN_THOUSAND;
			double num2 = (double)mBeanNum.Value / 10000.0;
			text = $"{num2:F4}";
		}
		else if (mBeanNum >= 100000000 && mBeanNum < 1000000000000L)
		{
			text2 = LanguageKey.BILLION;
			double num3 = (double)mBeanNum.Value / 100000000.0;
			text = $"{num3:F4}";
		}
		else if (mBeanNum >= 1000000000000L && mBeanNum < 10000000000000000L)
		{
			text2 = LanguageKey.ZHAO;
			double num4 = (double)mBeanNum.Value / 1000000000000.0;
			text = $"{num4:F4}";
		}
		else if (mBeanNum >= 10000000000000000L)
		{
			text2 = LanguageKey.BILLION_BILLION;
			double num5 = (double)mBeanNum.Value / 10000000000000000.0;
			text = $"{num5:F4}";
		}
		else if (mBeanNum < num)
		{
			return text3 + mBeanNum;
		}
		text = text.Substring(0, charNumLimit);
		int num6 = text.Length - 1;
		if (text.Contains("."))
		{
			for (int num7 = text.Length - 1; num7 >= 0; num7--)
			{
				if (text[num7] != '0')
				{
					num6 = ((text[num7] != '.') ? num7 : (num7 - 1));
					break;
				}
			}
			int num8 = text.IndexOf('.');
			if (num6 - num8 > 2)
			{
				num6 = num8 + 2;
			}
		}
		return text3 + text.Substring(0, num6 + 1) + text2;
	}

	public static string GetPiaoFenBeanString(long mBeanNum, int charNumLimit = 5)
	{
		if (WrappersManagerImpl.IsPatched(61939))
		{
			return WrappersManagerImpl.GetPatch(61939).__Gen_Wrap_2746(mBeanNum, charNumLimit);
		}
		string text = "";
		string text2 = "";
		string text3 = ((mBeanNum > 0) ? "+" : "-");
		mBeanNum = Math.Abs(mBeanNum);
		if ((double)mBeanNum >= Math.Pow(10.0, 8.0) && (double)mBeanNum < Math.Pow(10.0, 16.0))
		{
			text2 = LanguageKey.BILLION;
			double num = (double)mBeanNum / (Math.Pow(10.0, 8.0) * 1.0);
			text = $"{num:F4}";
		}
		else if ((double)mBeanNum >= Math.Pow(10.0, 16.0))
		{
			text2 = "京";
			double num2 = (double)mBeanNum / (Math.Pow(10.0, 16.0) * 1.0);
			text = $"{num2:F4}";
		}
		else if ((double)mBeanNum < Math.Pow(10.0, 8.0))
		{
			return text3 + mBeanNum;
		}
		text = text.Substring(0, charNumLimit);
		int num3 = text.Length - 1;
		if (text.Contains("."))
		{
			for (int num4 = text.Length - 1; num4 >= 0; num4--)
			{
				if (text[num4] != '0')
				{
					num3 = ((text[num4] != '.') ? num4 : (num4 - 1));
					break;
				}
			}
			int num5 = text.IndexOf('.');
			if (num3 - num5 > 2)
			{
				num3 = num5 + 2;
			}
		}
		return text3 + text.Substring(0, num3 + 1) + text2;
	}

	public static string GetDifenString(long? mBeanNum, int charNumLimit = 5)
	{
		if (WrappersManagerImpl.IsPatched(36892))
		{
			return WrappersManagerImpl.GetPatch(36892).__Gen_Wrap_768(mBeanNum, charNumLimit);
		}
		string text = "";
		string text2 = "";
		if ((float?)mBeanNum > Mathf.Pow(10f, 5f) && (float?)mBeanNum < Mathf.Pow(10f, 8f))
		{
			text2 = LanguageKey.TEN_THOUSAND;
			double num = (double)mBeanNum.Value / 10000.0;
			text = $"{num:F4}";
			text = text.Substring(0, charNumLimit);
			int num2 = text.Length - 1;
			if (text.Contains("."))
			{
				for (int num3 = text.Length - 1; num3 >= 0; num3--)
				{
					if (text[num3] != '0')
					{
						num2 = ((text[num3] != '.') ? num3 : (num3 - 1));
						break;
					}
				}
				int num4 = text.IndexOf('.');
				if (num2 - num4 > 2)
				{
					num2 = num4 + 2;
				}
			}
			return text.Substring(0, num2 + 1) + text2;
		}
		if ((float?)mBeanNum <= Mathf.Pow(10f, 5f))
		{
			return mBeanNum.ToString();
		}
		return GetBeanString(mBeanNum);
	}

	public static bool IsRuleV2(int typeID)
	{
		if (WrappersManagerImpl.IsPatched(35314))
		{
			return WrappersManagerImpl.GetPatch(35314).__Gen_Wrap_172(typeID);
		}
		List<int> listInt = SingletonBase<DynamicConfig>.GetInstance().GetListInt("NewRuleV2Config", "UseNewRuleGameType");
		foreach (string item in SingletonBase<DynamicConfig>.GetInstance().GetList<string>("NewRuleV2Config", "NewRuleAdapter"))
		{
			if (item.StartsWith(typeID.ToString()))
			{
				typeID = int.Parse(item.Split(':')[1]);
				break;
			}
		}
		return listInt?.Contains(typeID) ?? false;
	}

	public static bool RuleV2UseAddressables(int typeID)
	{
		if (WrappersManagerImpl.IsPatched(35308))
		{
			return WrappersManagerImpl.GetPatch(35308).__Gen_Wrap_172(typeID);
		}
		List<int> listInt = SingletonBase<DynamicConfig>.GetInstance().GetListInt("NewRuleV2Config", "UseAddressables");
		if (listInt != null && listInt.Count > 0)
		{
			return listInt.Contains(typeID);
		}
		return false;
	}

	[IDTag(1)]
	public static int RuleV2Adapter(int typeID, bool isPrivateRoom, int sceneID = -1)
	{
		if (WrappersManagerImpl.IsPatched(35315))
		{
			return WrappersManagerImpl.GetPatch(35315).__Gen_Wrap_2315(typeID, isPrivateRoom, sceneID);
		}
		if (SingletonBase<DynamicConfig>.GetInstance().GetBool("PersonalRoomNew", "HideRule"))
		{
			Log.Info($"HideRule typeID: {typeID}", "Game");
			return RuleV2Adapter(typeID, sceneID);
		}
		return RuleV2Adapter(typeID, sceneID) * ((!isPrivateRoom) ? 1 : 1000);
	}

	[IDTag(0)]
	public static int RuleV2Adapter(int typeID, int sceneID = -1)
	{
		if (WrappersManagerImpl.IsPatched(35316))
		{
			return WrappersManagerImpl.GetPatch(35316).__Gen_Wrap_219(typeID, sceneID);
		}
		foreach (string item in SingletonBase<DynamicConfig>.GetInstance().GetList<string>("NewRuleV2Config", "NewRuleAdapter"))
		{
			if (item.StartsWith(typeID.ToString()))
			{
				typeID = int.Parse(item.Split(':')[1]);
				break;
			}
		}
		if (sceneID != -1)
		{
			Log.Info("SceneAdapter: SceneID" + sceneID, "Game");
			foreach (string item2 in SingletonBase<DynamicConfig>.GetInstance().GetList<string>("NewRuleV2Config", "SceneAdapter"))
			{
				if (item2.StartsWith(sceneID.ToString()))
				{
					typeID = int.Parse(item2.Split(':')[1]);
					break;
				}
			}
		}
		return typeID;
	}

	public static void RemoveRulePanel(PopUpType type = PopUpType.None)
	{
		if (WrappersManagerImpl.IsPatched(345))
		{
			WrappersManagerImpl.GetPatch(345).__Gen_Wrap_119(type);
			return;
		}
		if (type == PopUpType.UGUI)
		{
			GameObject gameObject = GameObject.Find("Canvas");
			if (!(gameObject != null))
			{
				return;
			}
			RulePanel componentInChildren = gameObject.transform.GetComponentInChildren<RulePanel>();
			if (componentInChildren != null)
			{
				componentInChildren.CloseRulePanel();
				return;
			}
			CommonRulePanel componentInChildren2 = gameObject.transform.GetComponentInChildren<CommonRulePanel>();
			if (componentInChildren2 != null)
			{
				SingletonBase<PopUpManager>.GetInstance().RemovePopUp(componentInChildren2.gameObject);
			}
			return;
		}
		GameObject gameObject2 = GameObject.Find("UI Root/UICamera");
		if (!(gameObject2 != null))
		{
			return;
		}
		RulePanel componentInChildren3 = gameObject2.transform.GetComponentInChildren<RulePanel>();
		if (componentInChildren3 != null)
		{
			componentInChildren3.CloseRulePanel();
			return;
		}
		CommonRulePanel componentInChildren4 = gameObject2.transform.GetComponentInChildren<CommonRulePanel>();
		if (componentInChildren4 != null)
		{
			SingletonBase<PopUpManager>.GetInstance().RemovePopUp(componentInChildren4.gameObject);
		}
	}

	public static int SetSceondType(string typeStr)
	{
		if (WrappersManagerImpl.IsPatched(44453))
		{
			return WrappersManagerImpl.GetPatch(44453).__Gen_Wrap_85(typeStr);
		}
		int num = 0;
		if (typeStr == null || typeStr == string.Empty)
		{
			return 0;
		}
		if (typeStr == "0" || typeStr == "0|0|0|0|0|0")
		{
			return 0;
		}
		typeStr = typeStr.Replace("|", "");
		for (int i = 0; i < typeStr.Length; i++)
		{
			if (typeStr[i] == '1')
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	[IDTag(0)]
	public static bool IsSecondType(GameSecondType type, int m_secondType)
	{
		if (WrappersManagerImpl.IsPatched(44454))
		{
			return WrappersManagerImpl.GetPatch(44454).__Gen_Wrap_3037(type, m_secondType);
		}
		if ((m_secondType & (1 << (int)type)) != 0)
		{
			return true;
		}
		return false;
	}

	[IDTag(1)]
	public static bool IsSecondType(GameSecondType type, string config)
	{
		if (WrappersManagerImpl.IsPatched(44452))
		{
			return WrappersManagerImpl.GetPatch(44452).__Gen_Wrap_3038(type, config);
		}
		int secondType = SetSceondType(config);
		return IsSecondType(type, secondType);
	}

	public static HandKind GetHandKind(DressItem item, UserGender gender)
	{
		if (WrappersManagerImpl.IsPatched(35853))
		{
			return WrappersManagerImpl.GetPatch(35853).__Gen_Wrap_2388(item, gender);
		}
		if (item == null)
		{
			if (gender == UserGender.Female)
			{
				return HandKind.GirlHand;
			}
			return HandKind.BoyHand;
		}
		switch (item.id)
		{
		case 19001:
			return HandKind.BoyGloveHand1;
		case 29001:
			return HandKind.GirlGloveHand1;
		case 19002:
			return HandKind.BoyGloveHand2;
		case 29002:
			return HandKind.GirlGloveHand2;
		default:
			if (item.gender == UserGender.Male)
			{
				return HandKind.BoyHand;
			}
			if (item.gender == UserGender.Female)
			{
				return HandKind.GirlHand;
			}
			return HandKind.HandNone;
		}
	}

	[IDTag(1)]
	public static RoomConfig GetQuickGameRoomConfig(List<RoomConfig> roomList, int openType, bool isEnterNextLevel, bool bNeedRankFreeRoom = false)
	{
		if (WrappersManagerImpl.IsPatched(21633))
		{
			return WrappersManagerImpl.GetPatch(21633).__Gen_Wrap_1175(roomList, openType, isEnterNextLevel, bNeedRankFreeRoom);
		}
		int index = 0;
		return GetQuickGameRoomConfig(roomList, openType, ref index, isEnterNextLevel, bNeedRankFreeRoom);
	}

	public static RoomConfig HasRoomSectionScore(int gametype)
	{
		if (WrappersManagerImpl.IsPatched(57781))
		{
			return WrappersManagerImpl.GetPatch(57781).__Gen_Wrap_3093(gametype);
		}
		PlayingUIConfig configByKey = ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey(gametype);
		if (configByKey == null)
		{
			return null;
		}
		for (int i = 0; i < configByKey.rooms.Count; i++)
		{
			RoomConfig configByKey2 = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(configByKey.rooms[i]);
			if (SectionUtil.GetRoomSectionType(configByKey2) == RoomSectionType.RoomSectionScore)
			{
				return configByKey2;
			}
		}
		return null;
	}

	private static List<RoomConfig> GetSubGamRoomConfigs(List<RoomConfig> allRoomList, int subGameType)
	{
		if (WrappersManagerImpl.IsPatched(41665))
		{
			return WrappersManagerImpl.GetPatch(41665).__Gen_Wrap_2806(allRoomList, subGameType);
		}
		List<RoomConfig> list = new List<RoomConfig>();
		foreach (RoomConfig allRoom in allRoomList)
		{
			if (allRoom.playingUI_ID == subGameType)
			{
				list.Add(allRoom);
			}
		}
		return list;
	}

	private static int GetIndexOfGameConfigs(List<RoomConfig> allRoomList, RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(41666))
		{
			return WrappersManagerImpl.GetPatch(41666).__Gen_Wrap_21(allRoomList, config);
		}
		if (allRoomList == null)
		{
			return 0;
		}
		int result = 0;
		for (int i = 0; i < allRoomList.Count; i++)
		{
			if (allRoomList[i] == config)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	[IDTag(4)]
	public static RoomConfig GetQuickGameRoomConfig(List<RoomConfig> allRoomList, PlayingUIConfig playingUIConfig, ref int index, bool isEnterNextLevel, bool bNeedRankFreeRoom = false)
	{
		if (WrappersManagerImpl.IsPatched(41664))
		{
			return WrappersManagerImpl.GetPatch(41664).__Gen_Wrap_2807(allRoomList, playingUIConfig, ref index, isEnterNextLevel, bNeedRankFreeRoom);
		}
		if (playingUIConfig.gameTypeList.Count == 2)
		{
			List<RoomConfig> subGamRoomConfigs = GetSubGamRoomConfigs(allRoomList, playingUIConfig.gameTypeList[0]);
			List<RoomConfig> subGamRoomConfigs2 = GetSubGamRoomConfigs(allRoomList, playingUIConfig.gameTypeList[1]);
			int index2 = 0;
			RoomConfig quickGameRoomConfig = GetQuickGameRoomConfig(subGamRoomConfigs, playingUIConfig.openType, ref index2, isEnterNextLevel, bNeedRankFreeRoom);
			int index3 = 0;
			RoomConfig quickGameRoomConfig2 = GetQuickGameRoomConfig(subGamRoomConfigs2, playingUIConfig.openType, ref index3, isEnterNextLevel, bNeedRankFreeRoom);
			GameType gameType = (GameType)playingUIConfig.gameType;
			int gameMode = 0;
			int gametype = 0;
			bool flag = false;
			if (GetRemoteCurrentSelectedData(out gametype, out gameMode))
			{
				gameType = (GameType)gametype;
				if (gameType == (GameType)playingUIConfig.gameTypeList[0] || gameType == (GameType)playingUIConfig.gameTypeList[1])
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (gameType == (GameType)playingUIConfig.gameTypeList[0])
				{
					if (quickGameRoomConfig != null)
					{
						index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig);
						return quickGameRoomConfig;
					}
					index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig2);
					return quickGameRoomConfig2;
				}
				if (quickGameRoomConfig2 != null)
				{
					index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig2);
					return quickGameRoomConfig2;
				}
				index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig);
				return quickGameRoomConfig;
			}
			if (quickGameRoomConfig != null && quickGameRoomConfig2 != null)
			{
				if (index2 == index3)
				{
					index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig2);
					return quickGameRoomConfig2;
				}
				index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig);
				return quickGameRoomConfig;
			}
			if (quickGameRoomConfig == null)
			{
				index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig2);
				return quickGameRoomConfig2;
			}
			index = GetIndexOfGameConfigs(allRoomList, quickGameRoomConfig);
			return quickGameRoomConfig;
		}
		return GetQuickGameRoomConfig(allRoomList, playingUIConfig.openType, ref index, isEnterNextLevel, bNeedRankFreeRoom);
	}

	[IDTag(0)]
	public static RoomConfig GetQuickGameRoomConfig(List<RoomConfig> roomList, int openType, ref int index, bool isEnterNextLevel, bool bNeedRankFreeRoom = false)
	{
		if (WrappersManagerImpl.IsPatched(21634))
		{
			return WrappersManagerImpl.GetPatch(21634).__Gen_Wrap_1174(roomList, openType, ref index, isEnterNextLevel, bNeedRankFreeRoom);
		}
		if (roomList == null || roomList.Count == 0)
		{
			Log.Info("QuickGame Error no RoomConfig", "Selection");
			return null;
		}
		PlayerDataVO selfInfo = PlayerDataMgr.GetInstance().GetSelfInfo();
		index = 0;
		RoomConfig orgCfg = null;
		List<int> list = new List<int>();
		GetRoomRatio(roomList[0].playingUI_ID);
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("RecommendRoomInfo", "type", 2);
		for (int i = 0; i < roomList.Count; i++)
		{
			RoomConfig roomConfig = roomList[i];
			if (roomConfig == null)
			{
				continue;
			}
			long currentCurrencyNumWithAssist = selfInfo.GetCurrentCurrencyNumWithAssist(roomConfig);
			if (!isRoomRecommend(roomConfig, currentCurrencyNumWithAssist, openType, bNeedRankFreeRoom))
			{
				continue;
			}
			if (isEnterNextLevel)
			{
				if (num == 1)
				{
					if (roomConfig.maxBean != -1)
					{
						if (currentCurrencyNumWithAssist >= roomConfig.minBean && currentCurrencyNumWithAssist <= roomConfig.maxBean)
						{
							list.Add(i);
						}
					}
					else if (currentCurrencyNumWithAssist >= roomConfig.minBean)
					{
						list.Add(i);
					}
					continue;
				}
				int num2 = typeCompare(orgCfg, roomConfig);
				if (num2 != 1 && num2 != 0)
				{
					continue;
				}
				if (roomConfig.maxBean != -1)
				{
					if (currentCurrencyNumWithAssist >= roomConfig.recommendMinBean && selfInfo.GetCurrentCurrencyNum(roomConfig) <= roomConfig.recommendMaxBean)
					{
						orgCfg = roomConfig;
						index = i;
					}
				}
				else if (currentCurrencyNumWithAssist >= roomConfig.recommendMinBean)
				{
					orgCfg = roomConfig;
					index = i;
				}
				continue;
			}
			int num3 = typeCompare(orgCfg, roomConfig);
			if (num3 != 1 && num3 != 0)
			{
				continue;
			}
			if (roomConfig.maxBean != -1)
			{
				if (selfInfo.GetCurrentCurrencyNum(roomConfig) >= roomConfig.recommendMinBean && selfInfo.GetCurrentCurrencyNum(roomConfig) <= roomConfig.recommendMaxBean)
				{
					orgCfg = roomConfig;
					index = i;
				}
			}
			else if (selfInfo.GetCurrentCurrencyNum(roomConfig) >= roomConfig.recommendMinBean)
			{
				orgCfg = roomConfig;
				index = i;
			}
		}
		if (isEnterNextLevel)
		{
			if (num == 1)
			{
				if (list.Count == 0)
				{
					return null;
				}
				int index2 = list[list.Count - 1];
				for (int num4 = list.Count - 1; num4 >= 0; num4--)
				{
					if (roomList[list[num4]].point < roomList[index2].point)
					{
						index2 = list[num4];
					}
				}
				return roomList[index2];
			}
			return roomList[index];
		}
		return roomList[index];
	}

	private static int typeCompare(RoomConfig orgCfg, RoomConfig newCfg)
	{
		if (WrappersManagerImpl.IsPatched(21642))
		{
			return WrappersManagerImpl.GetPatch(21642).__Gen_Wrap_21(orgCfg, newCfg);
		}
		if (orgCfg == null || (orgCfg.type <= 0 && newCfg.type > 0))
		{
			return 1;
		}
		if (orgCfg.type == newCfg.type || (orgCfg.type > 0 && newCfg.type > 0))
		{
			return 0;
		}
		if (orgCfg.type > 0 && newCfg.type <= 0)
		{
			return -1;
		}
		return 0;
	}

	private static bool IsSameTypeConfig(RoomConfig config1, RoomConfig config2)
	{
		if (WrappersManagerImpl.IsPatched(61940))
		{
			return WrappersManagerImpl.GetPatch(61940).__Gen_Wrap_15(config1, config2);
		}
		if (config1.type == config2.type)
		{
			return true;
		}
		return false;
	}

	public static int GetRoomRatio(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(21635))
		{
			return WrappersManagerImpl.GetPatch(21635).__Gen_Wrap_144(gameType);
		}
		int result = SingletonBase<DynamicConfig>.GetInstance().GetInt("RoomRatio", "RoomBaseRatio", 25);
		if (SingletonBase<DynamicConfig>.GetInstance().GetObject("RoomRatio", "GameTypeRatio") is List<object> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is List<object> { Count: >=2 } list2 && Convert.ToInt64(list2[0]) == gameType)
				{
					return (int)Convert.ToInt64(list2[1]);
				}
			}
		}
		return result;
	}

	public static RoomConfig GetOverMaxBeanNextRoom(IGlobalModel globalModel)
	{
		if (WrappersManagerImpl.IsPatched(35597))
		{
			return WrappersManagerImpl.GetPatch(35597).__Gen_Wrap_1816(globalModel);
		}
		int gameType = globalModel.lastPlaying.gameType;
		if (globalModel.lastPlaying.gameType == 301)
		{
			RoomConfig roomConfigBySceneId = GetRoomConfigBySceneId((uint)GetLastPlaySceneID(GameType.ModuleHuoDong, notFree: true));
			foreach (PlayingUIConfig item in ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig())
			{
				if (item.rooms.Contains(roomConfigBySceneId.id))
				{
					gameType = item.gameType;
					break;
				}
				if (item.unionRoom.Contains(roomConfigBySceneId.id))
				{
					break;
				}
			}
		}
		return GetQuickGameRoomConfig(gameType, isEnterNextLevel: true);
	}

	[IDTag(1)]
	public static RoomInfoSearchType GetSearchType(RoomConfig roomInfo)
	{
		if (WrappersManagerImpl.IsPatched(34369))
		{
			return WrappersManagerImpl.GetPatch(34369).__Gen_Wrap_2213(roomInfo);
		}
		if (!IsGoldCoinRoom(roomInfo))
		{
			return RoomInfoSearchType.BeanRoom;
		}
		return RoomInfoSearchType.GoldCoinRoom;
	}

	[IDTag(0)]
	public static RoomInfoSearchType GetSearchType(bool isGoldCoinRoom)
	{
		if (WrappersManagerImpl.IsPatched(21626))
		{
			return WrappersManagerImpl.GetPatch(21626).__Gen_Wrap_1170(isGoldCoinRoom);
		}
		if (!isGoldCoinRoom)
		{
			return RoomInfoSearchType.BeanRoom;
		}
		return RoomInfoSearchType.GoldCoinRoom;
	}

	[IDTag(3)]
	public static RoomConfig GetQuickGameRoomConfig(int type, bool isEnterNextLevel, RoomInfoSearchType searchType = RoomInfoSearchType.IgnoreRoomType)
	{
		if (WrappersManagerImpl.IsPatched(21627))
		{
			return WrappersManagerImpl.GetPatch(21627).__Gen_Wrap_1177(type, isEnterNextLevel, searchType);
		}
		List<PlayingUIConfig> allCachedConfig = ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig();
		if (allCachedConfig == null)
		{
			return null;
		}
		PlayingUIConfig playingUIConfig = null;
		for (int i = 0; i < allCachedConfig.Count; i++)
		{
			if (IsPlayingUIContainGameType(allCachedConfig[i], type))
			{
				playingUIConfig = allCachedConfig[i];
				break;
			}
		}
		return GetQuickGameRoomConfig(playingUIConfig, isEnterNextLevel, bNeedRankFreeRoom: false, searchType);
	}

	public static string Icon2NameXueLiuBaoPai(string icon)
	{
		if (WrappersManagerImpl.IsPatched(45434))
		{
			return WrappersManagerImpl.GetPatch(45434).__Gen_Wrap_19(icon);
		}
		return icon switch
		{
			"Cainiaochang_Bk" => "菜鸟场", 
			"Pingmingchang_Bk" => "初级场", 
			"Guanjiachang_Bk" => "中级场", 
			"Tuhaochang_Bk" => "高级场", 
			"Fushanchang_Bk" => "大师场", 
			"Zhunjuechang_Bk" => "尊爵场", 
			"Huodongchang_Bk" => "限时场", 
			"Tiyanchang_Bk" => "体验场", 
			_ => "", 
		};
	}

	public static string Icon2NameByGameType(int gameType, string icon)
	{
		if (WrappersManagerImpl.IsPatched(45433))
		{
			return WrappersManagerImpl.GetPatch(45433).__Gen_Wrap_508(gameType, icon);
		}
		if (gameType != 321)
		{
			return Icon2Name(icon);
		}
		return Icon2NameXueLiuBaoPai(icon);
	}

	public static string Icon2Name(string icon)
	{
		if (WrappersManagerImpl.IsPatched(34370))
		{
			return WrappersManagerImpl.GetPatch(34370).__Gen_Wrap_19(icon);
		}
		return icon switch
		{
			"Cainiaochang_Bk" => "菜鸟场", 
			"Pingmingchang_Bk" => "平民场", 
			"Guanjiachang_Bk" => "官甲场", 
			"Tuhaochang_Bk" => "土豪场", 
			"Fushanchang_Bk" => "富商场", 
			"Zhunjuechang_Bk" => "尊爵场", 
			"Huodongchang_Bk" => "限时场", 
			"Tiyanchang_Bk" => "体验场", 
			_ => "", 
		};
	}

	[IDTag(2)]
	public static RoomConfig GetQuickGameRoomConfig(PlayingUIConfig playingUIConfig, bool isEnterNextLevel, bool bNeedRankFreeRoom = false, RoomInfoSearchType searchType = RoomInfoSearchType.IgnoreRoomType)
	{
		if (WrappersManagerImpl.IsPatched(21628))
		{
			return WrappersManagerImpl.GetPatch(21628).__Gen_Wrap_1176(playingUIConfig, isEnterNextLevel, bNeedRankFreeRoom, searchType);
		}
		if (playingUIConfig == null)
		{
			return null;
		}
		return GetQuickGameRoomConfig(GetRecommendedRoomConfigList(playingUIConfig, searchType), playingUIConfig.openType, isEnterNextLevel, bNeedRankFreeRoom);
	}

	[IDTag(0)]
	private static bool GetLastPlayGameType(PlayingUIConfig config, out GameType hitType)
	{
		if (WrappersManagerImpl.IsPatched(21630))
		{
			return WrappersManagerImpl.GetPatch(21630).__Gen_Wrap_1171(config, out hitType);
		}
		bool result = false;
		IGlobalModel globalModel = SingletonBase<GlobalDataModel>.GetInstance().globalModel;
		hitType = (GameType)config.gameType;
		if (globalModel != null && globalModel.enterGameInfo != null)
		{
			hitType = globalModel.enterGameInfo.gameType;
			result = true;
		}
		else
		{
			int gameMode = 0;
			int gametype = 0;
			if (GetRemoteCurrentSelectedData(out gametype, out gameMode))
			{
				hitType = (GameType)gametype;
				if (config.gameTypeList.Count == 2)
				{
					if (hitType == (GameType)config.gameTypeList[0] || hitType == (GameType)config.gameTypeList[1])
					{
						result = true;
					}
				}
				else if (hitType == (GameType)config.gameType)
				{
					result = true;
				}
			}
		}
		return result;
	}

	private static List<RoomConfig> GetRecommendedRoomConfigList(PlayingUIConfig playingUIConfig, RoomInfoSearchType searchType = RoomInfoSearchType.IgnoreRoomType)
	{
		if (WrappersManagerImpl.IsPatched(21629))
		{
			return WrappersManagerImpl.GetPatch(21629).__Gen_Wrap_1172(playingUIConfig, searchType);
		}
		GameType hitType = (GameType)playingUIConfig.gameType;
		bool lastPlayGameType = GetLastPlayGameType(playingUIConfig, out hitType);
		List<RoomConfig> list = new List<RoomConfig>();
		for (int i = 0; i < playingUIConfig.rooms.Count; i++)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(playingUIConfig.rooms[i]);
			if (configByKey == null)
			{
				continue;
			}
			if (playingUIConfig.gameTypeList.Count > 1)
			{
				if (lastPlayGameType)
				{
					if (hitType == (GameType)configByKey.playingUI_ID)
					{
						list.Add(configByKey);
					}
				}
				else
				{
					list.Add(configByKey);
				}
			}
			else
			{
				list.Add(configByKey);
			}
		}
		if (searchType != RoomInfoSearchType.IgnoreRoomType)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				bool flag = IsGoldCoinRoom(list[num]);
				if ((flag && searchType == RoomInfoSearchType.BeanRoom) || (!flag && searchType == RoomInfoSearchType.GoldCoinRoom))
				{
					list.RemoveAt(num);
				}
			}
		}
		return list;
	}

	public static List<RoomConfig> GetRoomConfigsOfGameType(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(61941))
		{
			return WrappersManagerImpl.GetPatch(61941).__Gen_Wrap_4170(gameType);
		}
		List<RoomConfig> allCachedRoomConfigList = GetAllCachedRoomConfigList();
		List<RoomConfig> list = new List<RoomConfig>();
		for (int i = 0; i < allCachedRoomConfigList.Count; i++)
		{
			RoomConfig roomConfig = allCachedRoomConfigList[i];
			if (roomConfig != null && roomConfig.playingUI_ID == gameType)
			{
				list.Add(roomConfig);
			}
		}
		return list;
	}

	public static List<RoomConfig> GetUnionRoomConfigs(PlayingUIConfig playingUIConfig)
	{
		if (WrappersManagerImpl.IsPatched(55718))
		{
			return WrappersManagerImpl.GetPatch(55718).__Gen_Wrap_3115(playingUIConfig);
		}
		List<RoomConfig> list = new List<RoomConfig>();
		for (int i = 0; i < playingUIConfig.unionRoom.Count; i++)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(playingUIConfig.unionRoom[i]);
			if (configByKey != null)
			{
				list.Add(configByKey);
			}
		}
		return list;
	}

	public static bool TryGetFirstActivityRuleInfo(out List<int> ruleInfo)
	{
		if (WrappersManagerImpl.IsPatched(41240))
		{
			return WrappersManagerImpl.GetPatch(41240).__Gen_Wrap_2451(out ruleInfo);
		}
		List<RoomConfig> allCachedRoomConfigList = GetAllCachedRoomConfigList();
		for (int i = 0; i < allCachedRoomConfigList.Count; i++)
		{
			RoomConfig roomConfig = allCachedRoomConfigList[i];
			if (roomConfig.ruleInfo == null || roomConfig.ruleInfo.Count <= 0)
			{
				continue;
			}
			ruleInfo = new List<int>();
			foreach (int item in roomConfig.ruleInfo)
			{
				ruleInfo.Add(item);
			}
			return true;
		}
		ruleInfo = null;
		return false;
	}

	public static bool TryGetAllActivityRuleInfo(out List<int> ruleInfo)
	{
		if (WrappersManagerImpl.IsPatched(36224))
		{
			return WrappersManagerImpl.GetPatch(36224).__Gen_Wrap_2451(out ruleInfo);
		}
		List<RoomConfig> allCachedRoomConfigList = GetAllCachedRoomConfigList();
		ruleInfo = new List<int>();
		for (int i = 0; i < allCachedRoomConfigList.Count; i++)
		{
			RoomConfig roomConfig = allCachedRoomConfigList[i];
			if (roomConfig.ruleInfo == null || roomConfig.ruleInfo.Count <= 0)
			{
				continue;
			}
			foreach (int item in roomConfig.ruleInfo)
			{
				if (!ruleInfo.Contains(item))
				{
					ruleInfo.Add(item);
				}
			}
		}
		if (ruleInfo.Count <= 0)
		{
			return false;
		}
		return true;
	}

	public static List<RoomConfig> GetRoomConfigs(PlayingUIConfig playingUIConfig)
	{
		if (WrappersManagerImpl.IsPatched(45536))
		{
			return WrappersManagerImpl.GetPatch(45536).__Gen_Wrap_3115(playingUIConfig);
		}
		List<RoomConfig> list = new List<RoomConfig>();
		for (int i = 0; i < playingUIConfig.rooms.Count; i++)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(playingUIConfig.rooms[i]);
			if (configByKey != null)
			{
				list.Add(configByKey);
			}
		}
		return list;
	}

	public static bool IsUnionRoomConfig(RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(61942))
		{
			return WrappersManagerImpl.GetPatch(61942).__Gen_Wrap_3(config);
		}
		int id = config.id;
		foreach (PlayingUIConfig item in ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig())
		{
			if (item.unionRoom.Contains(id))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsSceneOpen(PlayingUIConfig config, int sceneId)
	{
		if (WrappersManagerImpl.IsPatched(41661))
		{
			return WrappersManagerImpl.GetPatch(41661).__Gen_Wrap_122(config, sceneId);
		}
		if (config == null || !IsPlayingUIOpen(config))
		{
			return false;
		}
		foreach (int room in config.rooms)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(room);
			if (configByKey != null && configByKey.sceneId == sceneId)
			{
				return configByKey.openType;
			}
		}
		return false;
	}

	public static bool IsPlayingUIOpen(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(39117))
		{
			return WrappersManagerImpl.GetPatch(39117).__Gen_Wrap_3(config);
		}
		if (!IsPlayingUIOpenForce(config))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(config.ActivityBeginTime) || !string.IsNullOrEmpty(config.ActivityEndTime))
		{
			return IsInTimeSpan(config.ActivityBeginTime, config.ActivityEndTime);
		}
		return true;
	}

	public static bool CanShowPlayingUI(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(55802))
		{
			return WrappersManagerImpl.GetPatch(55802).__Gen_Wrap_3(config);
		}
		if (!IsPlayingUIOpenForce(config))
		{
			return false;
		}
		if ((!string.IsNullOrEmpty(config.ActivityBeginTime) || !string.IsNullOrEmpty(config.ActivityEndTime)) && config.ActivityOpenType != 1)
		{
			if (config.ActivityOpenType == 2)
			{
				return IsInTimeSpan(config.ActivityBeginTime, config.ActivityEndTime);
			}
			return false;
		}
		return true;
	}

	private static bool IsPlayingUIOpenForce(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(39118))
		{
			return WrappersManagerImpl.GetPatch(39118).__Gen_Wrap_3(config);
		}
		if (config == null || config.openType <= 0)
		{
			return false;
		}
		WanfaEntryConfig configByKey = ProtoConfigInnerLoader<WanfaEntryConfig>.getInstance().getConfigByKey(config.gameType);
		if (configByKey != null && configByKey.banChannels.Contains(GetChannelId()))
		{
			return false;
		}
		return true;
	}

	[IDTag(1)]
	public static RoomConfig GetQuickGameRoomConfigBasedOnLastPlay(PlayingUIConfig playingUIConfig)
	{
		if (WrappersManagerImpl.IsPatched(41659))
		{
			return WrappersManagerImpl.GetPatch(41659).__Gen_Wrap_1816(playingUIConfig);
		}
		int index = 0;
		return GetQuickGameRoomConfigBasedOnLastPlay(playingUIConfig, ref index);
	}

	[IDTag(0)]
	public static RoomConfig GetQuickGameRoomConfigBasedOnLastPlay(PlayingUIConfig playingUIConfig, ref int index)
	{
		if (WrappersManagerImpl.IsPatched(41660))
		{
			return WrappersManagerImpl.GetPatch(41660).__Gen_Wrap_2808(playingUIConfig, ref index);
		}
		if (playingUIConfig == null)
		{
			return null;
		}
		GameType gameType = (GameType)playingUIConfig.gameType;
		bool flag = false;
		if (SingletonBase<MainUIStateCenter>.GetInstance().currentState != 6 && playingUIConfig.unionRoom.Count > 0)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(playingUIConfig.unionRoom[0]);
			if (configByKey != null)
			{
				int playingUI_ID = configByKey.playingUI_ID;
				int gameMode = 0;
				int gametype = 0;
				if (GetRemoteCurrentSelectedData(out gametype, out gameMode) && gametype == playingUI_ID && IsPlayingUIOpen(ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey(gametype)))
				{
					flag = true;
					gameType = (GameType)gametype;
				}
			}
		}
		else if (playingUIConfig.gameTypeList.Count > 1)
		{
			int gameMode2 = 0;
			int gametype2 = 0;
			if (GetRemoteCurrentSelectedData(out gametype2, out gameMode2) && playingUIConfig.gameTypeList.Contains(gametype2) && IsPlayingUIOpen(ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey(gametype2)))
			{
				gameType = (GameType)gametype2;
			}
		}
		PlayingUIConfig configByKey2 = ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey((int)gameType);
		if (gameType == (GameType)playingUIConfig.gameType && !IsPlayingUIOpen(configByKey2))
		{
			playingUIConfig = ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey(180);
		}
		RepeatedField<int> repeatedField = playingUIConfig.rooms;
		if (flag)
		{
			repeatedField = playingUIConfig.unionRoom;
		}
		int lastPlaySceneID = GetLastPlaySceneID(gameType, notFree: false);
		if (lastPlaySceneID != 0 && IsSceneOpen(configByKey2, lastPlaySceneID) && !ForceRecommendSceneID())
		{
			for (int i = 0; i < repeatedField.Count; i++)
			{
				RoomConfig configByKey3 = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(repeatedField[i]);
				if (configByKey3 != null && configByKey3.sceneId == lastPlaySceneID)
				{
					RoomSectionType roomSectionType = SectionUtil.GetRoomSectionType(configByKey3);
					if ((SectionUtil.GetRoomSectionType(configByKey3) == RoomSectionType.RoomSectionScore && PlayerDataMgr.GetInstance().PlayerBeanNum >= 2000) || (roomSectionType == RoomSectionType.RoomGoldCoin && !IsSelfBeansFitRoom(configByKey3, PlayerDataMgr.GetInstance().PlayerGoldCoinNum)) || ((roomSectionType == RoomSectionType.RoomBean || roomSectionType == RoomSectionType.RoomSectionScoreAndBean) && !IsSelfBeansFitRoom(configByKey3, PlayerDataMgr.GetInstance().PlayerBeanNum)))
					{
						break;
					}
					index = i;
					return configByKey3;
				}
			}
		}
		List<RoomConfig> list = new List<RoomConfig>();
		for (int j = 0; j < playingUIConfig.rooms.Count; j++)
		{
			RoomConfig configByKey4 = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(playingUIConfig.rooms[j]);
			if (configByKey4 != null)
			{
				list.Add(configByKey4);
			}
		}
		return GetQuickGameRoomConfig(list, playingUIConfig, ref index, isEnterNextLevel: false);
	}

	private static bool ForceRecommendSceneID()
	{
		if (WrappersManagerImpl.IsPatched(41662))
		{
			return WrappersManagerImpl.GetPatch(41662).__Gen_Wrap_11();
		}
		if (PlayerPrefsHelper.HasKey("int_LastLoginSuccessTime"))
		{
			int serverTime = SingletonBase<ServerTime>.GetInstance().GetServerTime();
			int num = PlayerPrefsHelper.GetInt("int_LastLoginSuccessTime");
			int num2 = Math.Abs(serverTime - num) / 3600;
			int num3 = SingletonBase<DynamicConfig>.GetInstance().GetInt("RecommendCfg", "HourThreshold", 24);
			if (num2 >= num3)
			{
				Log.Info($"force recommend scene id, lastTime {num2}", "PlayerRecord");
				return true;
			}
			return false;
		}
		Log.Info("force recommend scene id", "PlayerRecord");
		return true;
	}

	public static RoomConfig GetGameRoomConfigForReturnUser(PlayingUIConfig playingUIConfig)
	{
		if (WrappersManagerImpl.IsPatched(43045))
		{
			return WrappersManagerImpl.GetPatch(43045).__Gen_Wrap_1816(playingUIConfig);
		}
		if (playingUIConfig == null)
		{
			return null;
		}
		RepeatedField<int> rooms = playingUIConfig.rooms;
		int num = -1;
		List<int> list = new List<int>();
		List<RoomConfig> list2 = new List<RoomConfig>();
		for (int i = 0; i < rooms.Count; i++)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(rooms[i]);
			switch (SectionUtil.GetRoomSectionType(configByKey))
			{
			case RoomSectionType.RoomSectionScore:
				num = rooms[i];
				break;
			case RoomSectionType.RoomBean:
			case RoomSectionType.RoomSectionScoreAndBean:
				if (configByKey != null)
				{
					list2.Add(configByKey);
				}
				break;
			}
		}
		list2.Sort((RoomConfig x, RoomConfig y) => x.minBean.CompareTo(y.minBean));
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			list.Add(list2[num2].id);
		}
		list2.Clear();
		if (num != -1 && PlayerDataMgr.GetInstance().PlayerBeanNum < 2000)
		{
			return ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(num);
		}
		if (list.Count == 1)
		{
			return ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(list[0]);
		}
		if (list.Count == 0)
		{
			Log.Error("GetGameRoomConfigForReturnUser: rooms is empty", "Game");
			return null;
		}
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			RoomConfig configByKey2 = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(list[num3]);
			if (configByKey2 != null && IsSelfBeansFitRoom(configByKey2, PlayerDataMgr.GetInstance().PlayerBeanNum))
			{
				return configByKey2;
			}
		}
		Log.Error("GetGameRoomConfigForReturnUser: no suitable rooms. ", "Game");
		return null;
	}

	public static bool IsSelfBeansFitRoom(RoomConfig config, long playerBean)
	{
		if (WrappersManagerImpl.IsPatched(41663))
		{
			return WrappersManagerImpl.GetPatch(41663).__Gen_Wrap_2805(config, playerBean);
		}
		bool result = false;
		if (playerBean >= config.minBean)
		{
			if (config.maxBean == -1)
			{
				return true;
			}
			if (playerBean <= config.maxBean)
			{
				result = true;
			}
		}
		return result;
	}

	public static bool IsSelfBeansFitRecommendRoom(RoomConfig config, long playerBean)
	{
		if (WrappersManagerImpl.IsPatched(61943))
		{
			return WrappersManagerImpl.GetPatch(61943).__Gen_Wrap_2805(config, playerBean);
		}
		bool result = false;
		if (playerBean >= config.recommendMinBean && playerBean <= config.recommendMaxBean)
		{
			return true;
		}
		return result;
	}

	public static RoomConfig GetLastNotFreeRoomConfig(PlayingUIConfig playingUIConfig)
	{
		if (WrappersManagerImpl.IsPatched(57795))
		{
			return WrappersManagerImpl.GetPatch(57795).__Gen_Wrap_1816(playingUIConfig);
		}
		if (playingUIConfig == null)
		{
			return null;
		}
		int lastPlaySceneID = GetLastPlaySceneID((GameType)playingUIConfig.gameType, notFree: true);
		RoomConfig roomConfig = null;
		if (lastPlaySceneID == 0)
		{
			for (int i = 0; i < playingUIConfig.rooms.Count; i++)
			{
				roomConfig = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(playingUIConfig.rooms[i]);
				if (SectionUtil.GetRoomSectionType(roomConfig) != RoomSectionType.RoomSectionScore)
				{
					return roomConfig;
				}
			}
		}
		else
		{
			roomConfig = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(lastPlaySceneID);
		}
		if (roomConfig != null)
		{
			return roomConfig;
		}
		return GetQuickGameRoomConfig(playingUIConfig, isEnterNextLevel: false);
	}

	public static bool isRoomRecommend(RoomConfig config, long happyBeanNum, int openType, bool bNeedRankFreeRoom = false)
	{
		if (WrappersManagerImpl.IsPatched(21637))
		{
			return WrappersManagerImpl.GetPatch(21637).__Gen_Wrap_1173(config, happyBeanNum, openType, bNeedRankFreeRoom);
		}
		if (config == null)
		{
			return false;
		}
		if (!config.openType)
		{
			return false;
		}
		if (!bNeedRankFreeRoom && SectionUtil.GetRoomSectionType(config) == RoomSectionType.RoomSectionScore)
		{
			return false;
		}
		if (openType == 1)
		{
			if (happyBeanNum >= config.minBean && (config.maxBean == -1 || happyBeanNum <= config.maxBean))
			{
				return true;
			}
			return false;
		}
		if (openType == config.type)
		{
			if (happyBeanNum > config.minBean && (config.maxBean == -1 || happyBeanNum <= config.maxBean))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static void OnShowRulePanelInGame(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(61944))
		{
			WrappersManagerImpl.GetPatch(61944).__Gen_Wrap_2836(gameType);
		}
		else if (IsRuleV2((int)gameType))
		{
			if (RuleV2UseAddressables((int)gameType) && AddressablesUtil.IsAddressablesOpen())
			{
				GameTutorialFactory.ShowTutorialDialogV2Addressables(RuleV2Adapter((int)gameType, IsPrivateRoom()), "RuleNewSingle", "基础规则");
			}
			else
			{
				GameTutorialFactory.ShowTutorialDialogV2((int)gameType, "RuleNewSingle", "基础规则");
			}
		}
		else
		{
			ShowRulePanelInGame(gameType, SceneSwitch.CurrentScene() == SceneSwitch.Scene.Game);
		}
	}

	public static void ShowRulePanelInGame(GameType gameType, bool useInGameStyle, List<int> selectRule = null)
	{
		if (WrappersManagerImpl.IsPatched(36188))
		{
			WrappersManagerImpl.GetPatch(36188).__Gen_Wrap_2452(gameType, useInGameStyle, selectRule);
			return;
		}
		if (RulePanelInGame.hasRuleView && GameObject.Find("RuleInGame") != null)
		{
			Log.Info("ShowRulePanelInGame return RulePanelInGame.hasRuleView is true", "Tutorial");
			return;
		}
		GameObject gameObject = UIUtil.LoadPrefabSync("RawResources_New/Prefabs/Game_ab", "RuleInGame", SingletonBase<PopUpManager>.GetInstance().GetUIRoot());
		if (null == gameObject)
		{
			Log.Error("RuleInGame load failed.", "Setting");
			return;
		}
		BackComponent backComponent = null;
		if (gameObject != null)
		{
			backComponent = gameObject.gameObject.GetComponent<BackComponent>();
		}
		if (backComponent == null)
		{
			gameObject.gameObject.AddComponent<BackComponent>();
		}
		if (gameObject != null)
		{
			GameObject gameObject2 = UIUtil.Instantiate(gameObject);
			gameObject2.name = gameObject.name;
			SingletonBase<PopUpManager>.GetInstance().PopUpWindow(gameObject2, PopUpType.UGUI);
			RulePanelInGame.hasRuleView = true;
			RulePanelInGame rulePanelInGame = gameObject2.GetComponent<RulePanelInGame>();
			if (rulePanelInGame == null)
			{
				rulePanelInGame = gameObject2.AddComponent<RulePanelInGame>();
			}
			rulePanelInGame.SetGameType(gameType, useInGameStyle);
			rulePanelInGame.SelectRule = selectRule;
		}
	}

	public static int GDMJGetTotalMJCount(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(61945))
		{
			return WrappersManagerImpl.GetPatch(61945).__Gen_Wrap_2317(gameType);
		}
		return 0;
	}

	public static SO_ROOM_TYPE GetRoomTypeByGameType(int type, bool inSecretRoom = false)
	{
		if (WrappersManagerImpl.IsPatched(3878))
		{
			return WrappersManagerImpl.GetPatch(3878).__Gen_Wrap_637(type, inSecretRoom);
		}
		SO_ROOM_TYPE result = SO_ROOM_TYPE.GAME_STYLE_2PMJ;
		switch (type)
		{
		case 1:
		case 149:
			if (inSecretRoom)
			{
				return SO_ROOM_TYPE.GAME_STYLE_PRIVATE_GUOBIAO;
			}
			return SO_ROOM_TYPE.GAME_STYLE_POP;
		case 2:
		case 378:
			return SO_ROOM_TYPE.GAME_STYLE_XZDD;
		case 3:
		case 379:
			return SO_ROOM_TYPE.GAME_STYLE_XLCH;
		case 5:
		case 145:
			return SO_ROOM_TYPE.GAME_STYLE_2PMJ;
		case 4:
			if (inSecretRoom)
			{
				return SO_ROOM_TYPE.GAME_STYLE_PRIVATE_WHMJ;
			}
			return SO_ROOM_TYPE.GAME_STYLE_WUHANMJ;
		case 6:
		case 377:
			return SO_ROOM_TYPE.GAME_STYLE_GDMJ;
		case 14:
			return SO_ROOM_TYPE.GAME_STYLE_GDMJ_TDH;
		case 15:
			return SO_ROOM_TYPE.GAME_STYLE_GDMJ_ZPTDH;
		case 17:
			return SO_ROOM_TYPE.GAME_STYLE_HNMJ_ZZMJ;
		case 16:
			return SO_ROOM_TYPE.GAME_STYLE_HNMJ_HZMJ;
		case 7:
			if (inSecretRoom)
			{
				return SO_ROOM_TYPE.GAME_STYLE_PRIVATE_CHANGSHA;
			}
			return SO_ROOM_TYPE.GAME_STYLE_CHANGSHAMJ;
		case 50:
			return SO_ROOM_TYPE.GAME_STYLE_MATCH_GUOBIAO;
		case 381:
			return SO_ROOM_TYPE.GAME_STYLE_XLCH_ZXQ;
		case 180:
			return SO_ROOM_TYPE.GAME_STYLE_HZXL;
		case 100:
			return SO_ROOM_TYPE.GAME_STYLE_COMM_TUIDAOHU;
		default:
			type = GetTeamBattleRealSvrGameType(type);
			if (type > 100)
			{
				return (SO_ROOM_TYPE)(type - 70);
			}
			return result;
		}
	}

	public static GameType GetGameTypeByRoomType(int type)
	{
		if (WrappersManagerImpl.IsPatched(2213))
		{
			return WrappersManagerImpl.GetPatch(2213).__Gen_Wrap_484(type);
		}
		GameType result = GameType.None;
		switch (type)
		{
		case 2:
		case 3:
		case 20:
			return GameType.ModuleTuidaohu49;
		case 5:
		case 9:
			return GameType.ModuleXueZhan;
		case 6:
		case 10:
			return GameType.ModuleXueLiu;
		case 7:
			return GameType.ModuleErRen;
		case 8:
		case 18:
			return GameType.WuHan;
		case 13:
			return GameType.ModuleGuangDong;
		case 14:
			return GameType.TuiDaoHu;
		case 15:
			return GameType.ZuoPaiTuiDaoHu;
		case 17:
			return GameType.ModuleZhuanZhuan;
		case 16:
			return GameType.HuNanHongZhong;
		case 21:
			return GameType.ChangSha;
		case 24:
			return GameType.RealGuoBiao;
		case 23:
			return GameType.ModuleTuidaohu;
		case 43:
			return GameType.ModuleDoubleXueLiu;
		case 44:
			return GameType.ModuleXueLiuZiXuanPai;
		case 110:
			return GameType.ModuleHongZhongXueLiu;
		case 203:
			return GameType.ModuleBaoPaiMahjong;
		case 19:
			return GameType.ChangSha;
		case 55:
			return GameType.ModuleGuiYangZhuoJi;
		case -1:
			return GameType.All;
		default:
			if (type >= 31)
			{
				result = (GameType)(type + 70);
			}
			return result;
		}
	}

	public static string ServerGameTypeToString(int type)
	{
		if (WrappersManagerImpl.IsPatched(47822))
		{
			return WrappersManagerImpl.GetPatch(47822).__Gen_Wrap_12(type);
		}
		switch (type)
		{
		case 2:
		case 3:
		case 20:
			return "大众";
		case 5:
			return "血战";
		case 9:
			return "血战 换三张";
		case 6:
			return "血流";
		case 10:
			return "血流 换三张";
		case 7:
			return "二人";
		case 8:
		case 18:
			return "武汉麻将";
		case 13:
			return "广东麻将";
		case 14:
			return "推倒胡";
		case 110:
			return "红中血流";
		default:
			return string.Empty;
		}
	}

	public static GameSubType GetSubGameTypeByRoomType(int type)
	{
		if (WrappersManagerImpl.IsPatched(47825))
		{
			return WrappersManagerImpl.GetPatch(47825).__Gen_Wrap_3271(type);
		}
		switch (type)
		{
		case 9:
		case 10:
			return GameSubType.Huan3Zhang;
		case 12:
			return GameSubType.DaZhongZhaNiao;
		default:
			return GameSubType.None;
		}
	}

	public static string GetPosString(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(61946))
		{
			return WrappersManagerImpl.GetPatch(61946).__Gen_Wrap_12(viewSeat);
		}
		string result = "";
		switch (viewSeat)
		{
		case 1:
			result = "（下家）";
			break;
		case 2:
			result = "（对家）";
			break;
		case 3:
			result = "（上家）";
			break;
		case 0:
			result = "（本家）";
			break;
		}
		return result;
	}

	public static string GetViewSeatString(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(61947))
		{
			return WrappersManagerImpl.GetPatch(61947).__Gen_Wrap_12(viewSeat);
		}
		string result = "";
		switch (viewSeat)
		{
		case 1:
			result = "下家";
			break;
		case 2:
			result = "对家";
			break;
		case 3:
			result = "上家";
			break;
		case 0:
			result = "本家";
			break;
		}
		return result;
	}

	public static void ShowSettingPanel(SettingPage page)
	{
		if (WrappersManagerImpl.IsPatched(61948))
		{
			WrappersManagerImpl.GetPatch(61948).__Gen_Wrap_4171(page);
			return;
		}
		if (SettingView.hasSettingView)
		{
			Log.Info("SettingView hasSettingView is true!", "Selection");
			Transform transform = GameObject.Find("Canvas").transform.Find("Setting");
			if (transform != null)
			{
				transform.GetComponent<SettingPanel>().ShowPage(page);
			}
			return;
		}
		GameObject gameObject = UIUtil.LoadPrefabSync("RawResources_New/Setting_ab", "SettingCommon", SingletonBase<PopUpManager>.GetInstance().GetUIRoot());
		if ((bool)gameObject)
		{
			SettingView.isShowSettingInGame = false;
			SettingView.StartSettingPage = page;
			GameObject gameObject2 = UIUtil.Instantiate(gameObject);
			SingletonBase<PopUpManager>.GetInstance().PopUpWindow(gameObject2, PopUpType.UGUI);
			gameObject2.name = gameObject.name;
			ResetLayer(gameObject, MJDef.LAYER_UI);
			SettingView.hasSettingView = true;
		}
	}

	public static int GetTeamBattleFriendViewSeat(GameType gameType, int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(36133))
		{
			return WrappersManagerImpl.GetPatch(36133).__Gen_Wrap_2438(gameType, viewSeat);
		}
		int num = 0;
		return viewSeat switch
		{
			0 => 2, 
			1 => 3, 
			2 => 0, 
			_ => 1, 
		};
	}

	public static bool GetRemoteCurrentSelectedData(out int gametype, out int gameMode)
	{
		if (WrappersManagerImpl.IsPatched(21187))
		{
			return WrappersManagerImpl.GetPatch(21187).__Gen_Wrap_1112(out gametype, out gameMode);
		}
		try
		{
			object obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("FirstEnter3.0Version");
			if (obj != null && Convert.ToInt32(obj) == 1)
			{
				if (SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndMode") != null)
				{
					string text = Convert.ToString(SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndMode"));
					if (text.Contains("|"))
					{
						string[] array = text.Split('|');
						if (int.TryParse(array[0].Split(':')[1], out gameMode))
						{
							if (gameMode == 0)
							{
								if (int.TryParse(array[1].Split(':')[1], out gametype))
								{
									return true;
								}
							}
							else if (gameMode == 1 && int.TryParse(array[2].Split(':')[1], out gametype))
							{
								return true;
							}
						}
					}
					gametype = 379;
					gameMode = 0;
					return false;
				}
				gametype = 379;
				gameMode = 0;
				return false;
			}
			obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelected");
			if (obj != null)
			{
				int num = Convert.ToInt32(obj);
				gametype = num;
				gameMode = 0;
				return true;
			}
			if (SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndMode") != null)
			{
				string text2 = Convert.ToString(SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndMode"));
				if (text2.Contains("|"))
				{
					string[] array2 = text2.Split('|');
					if (int.TryParse(array2[0].Split(':')[1], out gameMode))
					{
						if (gameMode == 0)
						{
							if (int.TryParse(array2[1].Split(':')[1], out gametype))
							{
								return true;
							}
						}
						else if (gameMode == 1 && int.TryParse(array2[2].Split(':')[1], out gametype))
						{
							return true;
						}
					}
				}
				gametype = 379;
				gameMode = 0;
				return false;
			}
			gametype = 379;
			gameMode = 0;
			return false;
		}
		catch (Exception ex)
		{
			SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndMode", "last:0|0:379|1:379");
			Log.Info("GetRemoteCurrentSelectedData ex: " + ex.ToString(), "Selection");
			gametype = 379;
			gameMode = 0;
			return false;
		}
	}

	public static bool GetRemoteCurrentSelectedDataForFavor(out int gametype, out int gameMode)
	{
		if (WrappersManagerImpl.IsPatched(21189))
		{
			return WrappersManagerImpl.GetPatch(21189).__Gen_Wrap_1112(out gametype, out gameMode);
		}
		try
		{
			object obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("FirstEnter3.0Version");
			if (obj != null && Convert.ToInt32(obj) == 1)
			{
				if (SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndModeForFavor") != null)
				{
					string text = Convert.ToString(SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndModeForFavor"));
					if (text.Contains("|"))
					{
						string[] array = text.Split('|');
						if (int.TryParse(array[0].Split(':')[1], out gameMode))
						{
							if (gameMode == 0)
							{
								if (int.TryParse(array[1].Split(':')[1], out gametype))
								{
									return true;
								}
							}
							else if (gameMode == 1 && int.TryParse(array[2].Split(':')[1], out gametype))
							{
								return true;
							}
						}
					}
					gametype = 379;
					gameMode = 0;
					return false;
				}
				gametype = 379;
				gameMode = 0;
				return false;
			}
			obj = SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelected");
			if (obj != null)
			{
				int num = Convert.ToInt32(obj);
				gametype = num;
				gameMode = 0;
				return true;
			}
			if (SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndModeForFavor") != null)
			{
				string text2 = Convert.ToString(SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndModeForFavor"));
				if (text2.Contains("|"))
				{
					string[] array2 = text2.Split('|');
					if (int.TryParse(array2[0].Split(':')[1], out gameMode))
					{
						if (gameMode == 0)
						{
							if (int.TryParse(array2[1].Split(':')[1], out gametype))
							{
								return true;
							}
						}
						else if (gameMode == 1 && int.TryParse(array2[2].Split(':')[1], out gametype))
						{
							return true;
						}
					}
				}
				gametype = 379;
				gameMode = 0;
				return false;
			}
			gametype = 379;
			gameMode = 0;
			return false;
		}
		catch (Exception ex)
		{
			SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndModeForFavor", "last:0|0:379|1:379");
			Log.Info("GetRemoteCurrentSelectedDataForQuickGame ex: " + ex.ToString(), "Selection");
			gametype = 379;
			gameMode = 0;
			return false;
		}
	}

	public static void StoreRemoteCurrentSelectedData(int gametype, int gameMode)
	{
		if (WrappersManagerImpl.IsPatched(35610))
		{
			WrappersManagerImpl.GetPatch(35610).__Gen_Wrap_306(gametype, gameMode);
			return;
		}
		try
		{
			if (SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndMode") != null)
			{
				string text = Convert.ToString(SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndMode"));
				if (text.Split('|').Length == 3 && text.Split('|')[1].Split(':').Length == 2)
				{
					int.Parse(text.Split('|')[1].Split(':')[1]);
					int.Parse(text.Split('|')[2].Split(':')[1]);
					string value = "last:" + gameMode + ((gameMode == 0) ? $"|0:{gametype}|1:{379}" : $"|0:{379}|1:{gametype}");
					SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndMode", value);
				}
				else
				{
					SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndMode", "last:0|0:379|1:379");
					Log.Info("StoreRemoteCurrentSelectedData data error!", "Selection");
				}
			}
			else
			{
				string value2 = "last:" + gameMode + ((gameMode == 0) ? $"|0:{gametype}|1:{379}" : $"|0:{379}|1:{gametype}");
				SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndMode", value2);
			}
		}
		catch (Exception ex)
		{
			SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndMode", "last:0|0:379|1:379");
			Log.Info("StoreRemoteCurrentSelectedData ex: " + ex.ToString(), "Selection");
		}
	}

	public static void StoreRemoteCurrentSelectedDataForFavor(int gametype, int gameMode)
	{
		if (WrappersManagerImpl.IsPatched(35608))
		{
			WrappersManagerImpl.GetPatch(35608).__Gen_Wrap_306(gametype, gameMode);
			return;
		}
		try
		{
			if (SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndModeForFavor") != null)
			{
				string text = Convert.ToString(SingletonBase<RemoteStoreService>.GetInstance().RemoteGetData("currentSelectedGameTypeAndModeForFavor"));
				if (text.Split('|').Length == 3 && text.Split('|')[1].Split(':').Length == 2)
				{
					int.Parse(text.Split('|')[1].Split(':')[1]);
					int.Parse(text.Split('|')[2].Split(':')[1]);
					string value = "last:" + gameMode + ((gameMode == 0) ? $"|0:{gametype}|1:{379}" : $"|0:{379}|1:{gametype}");
					SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndModeForFavor", value);
					SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("FirstEnter3.0Version", 1);
				}
				else
				{
					SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndModeForFavor", "last:0|0:379|1:379");
					SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("FirstEnter3.0Version", 1);
					Log.Info("StoreRemoteCurrentSelectedData data error!", "Selection");
				}
			}
			else
			{
				string value2 = "last:" + gameMode + ((gameMode == 0) ? $"|0:{gametype}|1:{379}" : $"|0:{379}|1:{gametype}");
				SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndModeForFavor", value2);
			}
		}
		catch (Exception ex)
		{
			SingletonBase<RemoteStoreService>.GetInstance().RemoteSetData("currentSelectedGameTypeAndModeForFavor", "last:0|0:379|1:379");
			Log.Info("StoreRemoteCurrentSelectedDataForFavor ex: " + ex.ToString(), "Selection");
		}
	}

	public static bool CanShowItemIcon(int id)
	{
		if (WrappersManagerImpl.IsPatched(45171))
		{
			return WrappersManagerImpl.GetPatch(45171).__Gen_Wrap_172(id);
		}
		if (id == 40000001)
		{
			return true;
		}
		if (id >= 100000000 && id <= 100099999)
		{
			return true;
		}
		switch (id)
		{
		case 0:
		case 40000010:
			return true;
		case 39800000:
		case 39800001:
		case 39800002:
		case 39800003:
		case 39800004:
		case 39800005:
		case 39800006:
		case 39800007:
		case 39800008:
		case 39800009:
		case 39800010:
		case 39800011:
		case 39800012:
		case 39800013:
		case 39800014:
		case 39800015:
		case 39800016:
		case 39800017:
		case 39800018:
		case 39800019:
		case 39800020:
		case 39800021:
		case 39800022:
		case 39800023:
		case 39800024:
		case 39800025:
		case 39800026:
		case 39800027:
		case 39800028:
		case 39800029:
		case 39800030:
		case 39800031:
		case 39800032:
		case 39800033:
		case 39800034:
		case 39800035:
		case 39800036:
		case 39800037:
		case 39800038:
		case 39800039:
		case 39800040:
		case 39800041:
		case 39800042:
		case 39800043:
		case 39800044:
		case 39800045:
		case 39800046:
		case 39800047:
		case 39800048:
		case 39800049:
		case 39800050:
		case 39800051:
		case 39800052:
		case 39800053:
		case 39800054:
		case 39800055:
		case 39800056:
		case 39800057:
		case 39800058:
		case 39800059:
		case 39800060:
		case 39800061:
		case 39800062:
		case 39800063:
		case 39800064:
		case 39800065:
		case 39800066:
		case 39800067:
		case 39800068:
		case 39800069:
		case 39800070:
		case 39800071:
		case 39800072:
		case 39800073:
		case 39800074:
		case 39800075:
		case 39800076:
		case 39800077:
		case 39800078:
		case 39800079:
		case 39800080:
		case 39800081:
		case 39800082:
		case 39800083:
		case 39800084:
		case 39800085:
		case 39800086:
		case 39800087:
		case 39800088:
		case 39800089:
		case 39800090:
		case 39800091:
		case 39800092:
		case 39800093:
		case 39800094:
		case 39800095:
		case 39800096:
		case 39800097:
		case 39800098:
		case 39800099:
			return true;
		default:
			if (SpecialItemHelper.IsTmtScore(id))
			{
				return true;
			}
			switch (id)
			{
			case 4601:
				return true;
			case -1:
				return true;
			default:
			{
				if (IsExchangeItem(id))
				{
					return true;
				}
				if (id >= 31000 && id <= 31999)
				{
					return true;
				}
				if (id >= 35001 && id <= 36000)
				{
					return true;
				}
				if (id == 38000)
				{
					return true;
				}
				ItemConfig configByKey = ProtoConfigLoader<ItemConfig>.getInstance().getConfigByKey(id);
				if (configByKey == null)
				{
					return false;
				}
				Item item = new Item(configByKey);
				if (item.gender == UserGender.UnKnown || item.gender == SingletonBase<ShopDataHelper>.GetInstance().getGender())
				{
					return true;
				}
				return false;
			}
			}
		}
	}

	public static bool IsExchangeItem(int id)
	{
		if (WrappersManagerImpl.IsPatched(45172))
		{
			return WrappersManagerImpl.GetPatch(45172).__Gen_Wrap_172(id);
		}
		if (id.ToString().StartsWith("39809") && id != 39808888 && id != 33002)
		{
			return true;
		}
		return false;
	}

	public static sbyte[] StringToSByte(string str, out int outLength)
	{
		if (WrappersManagerImpl.IsPatched(61949))
		{
			return WrappersManagerImpl.GetPatch(61949).__Gen_Wrap_4172(str, out outLength);
		}
		outLength = 0;
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		sbyte[] array = new sbyte[bytes.Length];
		Buffer.BlockCopy(bytes, 0, array, 0, array.Length);
		outLength = array.Length;
		return array;
	}

	public static int[] StringToIntArray(string str, out int outLength)
	{
		if (WrappersManagerImpl.IsPatched(61950))
		{
			return WrappersManagerImpl.GetPatch(61950).__Gen_Wrap_4173(str, out outLength);
		}
		outLength = 0;
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		int[] array = new int[bytes.Length];
		for (int i = 0; i < bytes.Length; i++)
		{
			array[i] = bytes[i];
		}
		outLength = array.Length;
		return array;
	}

	public static void AssignArrayByIntArray(object assignObject, string memberName, int[] intArray, System.Type elementType)
	{
		if (WrappersManagerImpl.IsPatched(61951))
		{
			WrappersManagerImpl.GetPatch(61951).__Gen_Wrap_226(assignObject, memberName, intArray, elementType);
		}
		else
		{
			if (assignObject == null || string.IsNullOrEmpty(memberName))
			{
				return;
			}
			Array array = null;
			if (intArray != null)
			{
				array = Array.CreateInstance(elementType, intArray.Length);
				for (int i = 0; i < intArray.Length; i++)
				{
					array.SetValue(Convert.ChangeType(intArray[i], elementType), i);
				}
			}
			SetPropertyOrField(assignObject, memberName, array);
		}
	}

	public static int AssignByteArrayByString(object assignObject, string memberName, string str)
	{
		if (WrappersManagerImpl.IsPatched(61953))
		{
			return WrappersManagerImpl.GetPatch(61953).__Gen_Wrap_896(assignObject, memberName, str);
		}
		if (assignObject == null || string.IsNullOrEmpty(memberName) || string.IsNullOrEmpty(str))
		{
			return 0;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		if (SetPropertyOrField(assignObject, memberName, bytes))
		{
			return bytes.Length;
		}
		return 0;
	}

	public static int AssignSByteArrayByString(object assignObject, string memberName, string str)
	{
		if (WrappersManagerImpl.IsPatched(61954))
		{
			return WrappersManagerImpl.GetPatch(61954).__Gen_Wrap_896(assignObject, memberName, str);
		}
		Log.Info("AssignSByteArrayByString memberName = " + memberName + " str " + str, "Framework");
		if (assignObject == null || string.IsNullOrEmpty(memberName))
		{
			return 0;
		}
		sbyte[] array = null;
		if (string.IsNullOrEmpty(str))
		{
			array = new sbyte[0];
		}
		else
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			array = new sbyte[bytes.Length];
			Buffer.BlockCopy(bytes, 0, array, 0, array.Length);
		}
		if (SetPropertyOrField(assignObject, memberName, array))
		{
			return array.Length;
		}
		return 0;
	}

	public static void CreateArrayAndAssign(object assignObject, string memberName, int length, System.Type elementType)
	{
		if (WrappersManagerImpl.IsPatched(61955))
		{
			WrappersManagerImpl.GetPatch(61955).__Gen_Wrap_26(assignObject, memberName, length, elementType);
			return;
		}
		Log.Info("CreateArrayAndAssign memberName = " + memberName + " length " + length + " elementType " + elementType, "Framework");
		if (assignObject != null && length > 0 && !string.IsNullOrEmpty(memberName))
		{
			Array valueObject = Array.CreateInstance(elementType, length);
			SetPropertyOrField(assignObject, memberName, valueObject);
		}
	}

	public static int[] GetIntArrayFromName(object assignObject, string memberName, out int outLength)
	{
		if (WrappersManagerImpl.IsPatched(61956))
		{
			return WrappersManagerImpl.GetPatch(61956).__Gen_Wrap_4174(assignObject, memberName, out outLength);
		}
		Log.Info("GetIntArrayFromName", "Framework");
		outLength = 0;
		if (!(GetPropertyOrField(assignObject, memberName) is Array array))
		{
			return null;
		}
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			int num = Convert.ToInt32(array.GetValue(i));
			array2[i] = num;
		}
		outLength = array2.Length;
		return array2;
	}

	private static object GetPropertyOrField(object assignObject, string memberName)
	{
		if (WrappersManagerImpl.IsPatched(61957))
		{
			return WrappersManagerImpl.GetPatch(61957).__Gen_Wrap_183(assignObject, memberName);
		}
		Log.Info("GetPropertyOrField", "Framework");
		if (assignObject == null || string.IsNullOrEmpty(memberName))
		{
			return null;
		}
		PropertyInfo property = assignObject.GetType().GetProperty(memberName);
		if (property != null)
		{
			return property.GetValue(assignObject, null);
		}
		FieldInfo field = assignObject.GetType().GetField(memberName);
		if (field != null)
		{
			return field.GetValue(assignObject);
		}
		Log.Info("GetPropertyOrField field error", "Framework");
		return null;
	}

	private static bool SetPropertyOrField(object assignObject, string memberName, object valueObject)
	{
		if (WrappersManagerImpl.IsPatched(61952))
		{
			return WrappersManagerImpl.GetPatch(61952).__Gen_Wrap_105(assignObject, memberName, valueObject);
		}
		Log.Info("SetPropertyOrField", "Framework");
		if (assignObject == null || string.IsNullOrEmpty(memberName))
		{
			return false;
		}
		PropertyInfo property = assignObject.GetType().GetProperty(memberName);
		if (property != null)
		{
			property.SetValue(assignObject, valueObject, null);
			return true;
		}
		FieldInfo field = assignObject.GetType().GetField(memberName);
		if (field != null)
		{
			field.SetValue(assignObject, valueObject);
			return true;
		}
		Log.Info("SetPropertyOrField property error", "Framework");
		return false;
	}

	public static int AssignByteArrayByName(object assignObject, string memberName, object assignObjectIni, string memberNameIni)
	{
		if (WrappersManagerImpl.IsPatched(61958))
		{
			return WrappersManagerImpl.GetPatch(61958).__Gen_Wrap_293(assignObject, memberName, assignObjectIni, memberNameIni);
		}
		Log.Info("AssignByteArrayByName", "Framework");
		if (assignObject == null || string.IsNullOrEmpty(memberName))
		{
			return 0;
		}
		if (!(GetPropertyOrField(assignObjectIni, memberNameIni) is Array array))
		{
			return 0;
		}
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = Convert.ToByte(array.GetValue(i));
		}
		if (SetPropertyOrField(assignObject, memberName, array2))
		{
			return array2.Length;
		}
		return 0;
	}

	public static int AssignSByteArrayByName(object assignObject, string memberName, object assignObjectIni, string memberNameIni)
	{
		if (WrappersManagerImpl.IsPatched(61959))
		{
			return WrappersManagerImpl.GetPatch(61959).__Gen_Wrap_293(assignObject, memberName, assignObjectIni, memberNameIni);
		}
		Log.Info("AssignSByteArrayByName", "Framework");
		if (assignObject == null || string.IsNullOrEmpty(memberName))
		{
			return 0;
		}
		if (!(GetPropertyOrField(assignObjectIni, memberNameIni) is Array array))
		{
			return 0;
		}
		sbyte[] array2 = new sbyte[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = Convert.ToSByte(array.GetValue(i));
		}
		if (SetPropertyOrField(assignObject, memberName, array2))
		{
			return array2.Length;
		}
		return 0;
	}

	public static bool SerializeObjectFoTest(object serializableObject, string fileName = null)
	{
		if (WrappersManagerImpl.IsPatched(61960))
		{
			return WrappersManagerImpl.GetPatch(61960).__Gen_Wrap_15(serializableObject, fileName);
		}
		return false;
	}

	public static Array ResizeArray(System.Type elementType, Array array, int num)
	{
		if (WrappersManagerImpl.IsPatched(61961))
		{
			return WrappersManagerImpl.GetPatch(61961).__Gen_Wrap_4175(elementType, array, num);
		}
		if (array == null || array.Length <= num || num < 0)
		{
			return array;
		}
		Array array2 = Array.CreateInstance(elementType, num);
		if (num > 0)
		{
			Array.Copy(array, array2, num);
		}
		return array2;
	}

	public static void CheckXluaDataType(object data)
	{
		if (WrappersManagerImpl.IsPatched(61962))
		{
			WrappersManagerImpl.GetPatch(61962).__Gen_Wrap_0(data);
		}
		else if (data != null)
		{
			System.Type type = data.GetType();
			Log.Info("CSReplayRecordService TestData typeName = " + type, "Framework");
			if (data is string)
			{
				Encoding.UTF8.GetBytes(data as string);
			}
			else if (type.IsArray && data is Array array)
			{
				Log.Info("CSReplayRecordService TestData array size = " + array.Length, "Framework");
			}
		}
		else
		{
			Log.Info("CSReplayRecordService data null", "Framework");
		}
	}

	public static int GetByteArrayLen(byte[] pBuffer)
	{
		if (WrappersManagerImpl.IsPatched(61963))
		{
			return WrappersManagerImpl.GetPatch(61963).__Gen_Wrap_85(pBuffer);
		}
		if (pBuffer != null)
		{
			return pBuffer.Length;
		}
		return -1;
	}

	public static int GetSByteArrayLen(sbyte[] pBuffer)
	{
		if (WrappersManagerImpl.IsPatched(61964))
		{
			return WrappersManagerImpl.GetPatch(61964).__Gen_Wrap_85(pBuffer);
		}
		if (pBuffer != null)
		{
			return pBuffer.Length;
		}
		return -1;
	}

	public static byte GetByteArrayByIndex(byte[] pBuffer, int index)
	{
		if (WrappersManagerImpl.IsPatched(61965))
		{
			return WrappersManagerImpl.GetPatch(61965).__Gen_Wrap_486(pBuffer, index);
		}
		if (pBuffer != null && pBuffer.Length < index)
		{
			return pBuffer[index];
		}
		return 0;
	}

	public static sbyte GetSByteArrayByIndex(sbyte[] pBuffer, int index)
	{
		if (WrappersManagerImpl.IsPatched(61966))
		{
			return WrappersManagerImpl.GetPatch(61966).__Gen_Wrap_4176(pBuffer, index);
		}
		if (pBuffer != null && pBuffer.Length < index)
		{
			return pBuffer[index];
		}
		return 0;
	}

	[IDTag(0)]
	public static string GetPlayingNameByGameID(int playingID)
	{
		if (WrappersManagerImpl.IsPatched(33302))
		{
			return WrappersManagerImpl.GetPatch(33302).__Gen_Wrap_12(playingID);
		}
		GameNameConfig gameNameConfig = GetGameNameConfig(playingID);
		if (gameNameConfig != null)
		{
			return gameNameConfig.name;
		}
		Log.Error("GetPlayingNameByGameID config is null " + playingID, "Framework");
		return "";
	}

	public static GameNameConfig GetGameNameConfig(int playingID)
	{
		if (WrappersManagerImpl.IsPatched(33303))
		{
			return WrappersManagerImpl.GetPatch(33303).__Gen_Wrap_2068(playingID);
		}
		if (ProtoConfigLoader<GameNameConfig>.getInstance().getAllCachedConfig().Count == 0)
		{
			HappyMahjong.update.CsResMgr.CsResReq csResReq = new HappyMahjong.update.CsResMgr.CsResReq();
			csResReq.filename = "GameNameConfig.data";
			csResReq.nCallBack = delegate(string name, byte[] pBuff, int iRuleID, bool changed, bool successed)
			{
				if (successed)
				{
					ProtoConfigLoader<GameNameConfig>.getInstance().load(pBuff);
				}
			};
			HappyMahjong.update.CsResMgr.GetInstance().GetConfig(csResReq);
		}
		playingID = GetTeamPlayingMainID(playingID);
		return ProtoConfigLoader<GameNameConfig>.getInstance().getConfigByKey(playingID);
	}

	[IDTag(1)]
	public static string GetPlayingNameByGameID(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(36035))
		{
			return WrappersManagerImpl.GetPatch(36035).__Gen_Wrap_2415(gameType);
		}
		return GetPlayingNameByGameID((int)gameType);
	}

	public static string GetWaterMaskSubName(GameType gameType, RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(61967))
		{
			return WrappersManagerImpl.GetPatch(61967).__Gen_Wrap_4177(gameType, config);
		}
		int type = config.type;
		string result = string.Empty;
		if (gameType == GameType.ModuleHuoDong || gameType == GameType.ModuleXueLiuBaoPai)
		{
			result = config.subName;
		}
		else
		{
			switch (type)
			{
			case 5:
				result = LanguageKey.CHANGE_THREE_COPIES_1_1280;
				break;
			case 6:
				result = LanguageKey.OVER_HU_PLUS_1_1023;
				break;
			case 11:
				result = LanguageKey.TIE_BIRD_3_829;
				break;
			}
		}
		return result;
	}

	public static RoomConfig GetRoomConfigByGameType(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(45122))
		{
			return WrappersManagerImpl.GetPatch(45122).__Gen_Wrap_3093(gameType);
		}
		RoomConfig result = null;
		List<RoomConfig> allCachedRoomConfigList = GetAllCachedRoomConfigList();
		for (int i = 0; i < allCachedRoomConfigList.Count; i++)
		{
			if (allCachedRoomConfigList[i].playingUI_ID == gameType)
			{
				result = allCachedRoomConfigList[i];
				break;
			}
		}
		return result;
	}

	public static bool CheckIsXueLiuWanfa(GameType gametype)
	{
		if (WrappersManagerImpl.IsPatched(61968))
		{
			return WrappersManagerImpl.GetPatch(61968).__Gen_Wrap_2318(gametype);
		}
		if (gametype == GameType.ModuleXueLiu || gametype == GameType.ModuleBaoZouXueLiu || gametype == GameType.ModuleHongZhongXueLiu || gametype == GameType.ModuleXueLiu2v2 || gametype == GameType.Module2V2XueLiu || gametype == GameType.Module2V2HongZhongXueLiu || gametype == GameType.ModuleDoubleXueLiu || gametype == GameType.ModuleUniversalXueliu || gametype == GameType.ModuleBazhangpai)
		{
			return true;
		}
		return false;
	}

	public static string GetRoomConfigBeanString(RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(44451))
		{
			return WrappersManagerImpl.GetPatch(44451).__Gen_Wrap_19(config);
		}
		string text = "";
		if (config.maxBean == -1 && config.minBean == -1)
		{
			return "";
		}
		if (config.maxBean == -1 && config.minBean >= 0)
		{
			long num = config.minBean / 1000;
			if (num >= 10)
			{
				return $"{(float)num / 10f}万以上";
			}
			return $"{num}千以上";
		}
		long num2 = config.minBean / 1000;
		long num3 = config.maxBean / 1000;
		text = ((num2 < 10) ? $"{num2}千" : $"{(float)num2 / 10f}万");
		if (num3 >= 10)
		{
			return $"{text}-{(float)num3 / 10f}万";
		}
		return $"{text}-{num3}千";
	}

	public static GameType GetGameType(int moduleFlag, int svrGameType)
	{
		if (WrappersManagerImpl.IsPatched(61969))
		{
			return WrappersManagerImpl.GetPatch(61969).__Gen_Wrap_4178(moduleFlag, svrGameType);
		}
		GameType result = GameType.None;
		switch (moduleFlag)
		{
		case 1:
			result = GetModuleGameTypeFromNoModuleType(svrGameType);
			break;
		case 2:
			result = GetGameTypeByRoomType(svrGameType);
			break;
		}
		return result;
	}

	public static bool IsPrivateRoomGameType(int gametype)
	{
		if (WrappersManagerImpl.IsPatched(61971))
		{
			return WrappersManagerImpl.GetPatch(61971).__Gen_Wrap_172(gametype);
		}
		bool num = (from x in ProtoConfigLoader<RoomConfig>.getInstance().getAllCachedConfig()
			where x.moneyType == 6
			select x.playingUI_ID).Contains(gametype);
		bool flag = gametype == 157 || gametype == 158 || gametype == 159 || gametype == 138 || gametype == 139 || gametype == 160 || gametype == 190 || gametype == 191 || gametype == 192 || gametype == 193;
		return num || flag;
	}

	public static bool IsGuangDongMaJiang(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(57862))
		{
			return WrappersManagerImpl.GetPatch(57862).__Gen_Wrap_172(gameType);
		}
		return new int[5] { 377, 153, 155, 190, 191 }.Contains(gameType);
	}

	public static bool IsPlayingUIContainGameType(PlayingUIConfig playingUIConfig, int gameType)
	{
		if (WrappersManagerImpl.IsPatched(3812))
		{
			return WrappersManagerImpl.GetPatch(3812).__Gen_Wrap_122(playingUIConfig, gameType);
		}
		if (playingUIConfig != null)
		{
			if (playingUIConfig.gameType != gameType)
			{
				return playingUIConfig.gameTypeList.Contains(gameType);
			}
			return true;
		}
		return false;
	}

	public static PlayingUIConfig FindPlayingUIConfig(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(39116))
		{
			return WrappersManagerImpl.GetPatch(39116).__Gen_Wrap_453(gameType);
		}
		List<PlayingUIConfig> allCachedConfig = ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig();
		foreach (PlayingUIConfig item in allCachedConfig)
		{
			if (item != null && item.gameType == gameType)
			{
				return item;
			}
		}
		foreach (PlayingUIConfig item2 in allCachedConfig)
		{
			if (item2 != null && item2.gameTypeList.Contains(gameType))
			{
				return item2;
			}
		}
		return null;
	}

	public static void ParseAtlasSprite(ItemConfig config, ref string atlasName, ref string spriteName)
	{
		if (WrappersManagerImpl.IsPatched(61974))
		{
			WrappersManagerImpl.GetPatch(61974).__Gen_Wrap_4179(config, ref atlasName, ref spriteName);
			return;
		}
		try
		{
			string[] array = config.icon.Split('|');
			atlasName = array[0];
			spriteName = array[1];
		}
		catch (Exception ex)
		{
			Log.Error($"{config.id}, {config.icon}, {ex.Message}", "RankTools");
		}
	}

	public static List<GoodsConfig> ParseRankGradeToolsConfigList()
	{
		if (WrappersManagerImpl.IsPatched(35392))
		{
			return WrappersManagerImpl.GetPatch(35392).__Gen_Wrap_2326();
		}
		List<GoodsConfig> allCachedConfig = ProtoConfigLoader<GoodsConfig>.getInstance().getAllCachedConfig();
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			ProtoConfigLoader<GoodsConfig>.getInstance().load(HappyMahjong.update.CsResMgr.GetInstance().GetConfig("GoodsConfig.data"));
			allCachedConfig = ProtoConfigLoader<GoodsConfig>.getInstance().getAllCachedConfig();
		}
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			Log.Info("ParseRankGradeToolsConfigList goods config is empty!", "RankTools");
			return new List<GoodsConfig>();
		}
		List<GoodsConfig> list = new List<GoodsConfig>();
		for (int i = 0; i < allCachedConfig.Count; i++)
		{
			GoodsConfig goodsConfig = allCachedConfig[i];
			if (goodsConfig.page != null && goodsConfig.page.Contains(9))
			{
				list.Add(goodsConfig);
			}
		}
		return list;
	}

	public static List<GoodsConfig> ParseRankToolsBagConfigList()
	{
		if (WrappersManagerImpl.IsPatched(44875))
		{
			return WrappersManagerImpl.GetPatch(44875).__Gen_Wrap_2326();
		}
		List<GoodsConfig> allCachedConfig = ProtoConfigLoader<GoodsConfig>.getInstance().getAllCachedConfig();
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			ProtoConfigLoader<GoodsConfig>.getInstance().load(HappyMahjong.update.CsResMgr.GetInstance().GetConfig("GoodsConfig.data"));
			allCachedConfig = ProtoConfigLoader<GoodsConfig>.getInstance().getAllCachedConfig();
		}
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			Log.Info("ParseRankToolsBagConfigList goods config is empty!", "RankTools");
			return new List<GoodsConfig>();
		}
		List<GoodsConfig> list = new List<GoodsConfig>();
		for (int i = 0; i < allCachedConfig.Count; i++)
		{
			GoodsConfig goodsConfig = allCachedConfig[i];
			if (goodsConfig.page != null && goodsConfig.page.Contains(81))
			{
				list.Add(goodsConfig);
			}
		}
		return list;
	}

	public static void RemoveUnusedConfig<T>(ref Hashtable configHashCache, ref List<T> configCache)
	{
	}

	public static GameType GetModuleGameTypeFromNoModuleType(int SrvGameType)
	{
		if (WrappersManagerImpl.IsPatched(61970))
		{
			return WrappersManagerImpl.GetPatch(61970).__Gen_Wrap_484(SrvGameType);
		}
		GameType gameType = GameType.None;
		switch (SrvGameType)
		{
		case 2:
		case 12:
			return GameType.ModuleTuidaohu49;
		case 5:
		case 9:
			return GameType.ModuleXueZhan;
		case 6:
		case 10:
			return GameType.ModuleXueLiu;
		case 7:
		case 11:
			return GameType.ModuleErRen;
		case 8:
			return GameType.ModuleWuHan;
		case 13:
			return GameType.ModuleGuangDong;
		case 21:
			return GameType.ModuleChangSha;
		default:
			return GetGameTypeByRoomType(SrvGameType);
		}
	}

	public static bool IsEntertainmentPlaying(int roomStyle)
	{
		if (WrappersManagerImpl.IsPatched(41702))
		{
			return WrappersManagerImpl.GetPatch(41702).__Gen_Wrap_172(roomStyle);
		}
		if (roomStyle != 3 && roomStyle != 4 && roomStyle != 5)
		{
			return roomStyle == 6;
		}
		return true;
	}

	public static bool IsAllRoomClose(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(45131))
		{
			return WrappersManagerImpl.GetPatch(45131).__Gen_Wrap_3(config);
		}
		if (config == null)
		{
			return false;
		}
		for (int i = 0; i < config.rooms.Count; i++)
		{
			RoomConfig configByKey = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(config.rooms[i]);
			if (configByKey != null && (configByKey.ShowRankMode == 1 || SingletonBase<RankGradingController>.GetInstance().IsInABTestTime(configByKey.sceneId)))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsNormal2V2Playing(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(61975))
		{
			return WrappersManagerImpl.GetPatch(61975).__Gen_Wrap_172(gameType);
		}
		bool result = false;
		PlayingUIConfig configByKey = ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey(gameType);
		if (configByKey != null)
		{
			result = configByKey.roomStyle == 4;
		}
		return result;
	}

	public static bool IsTeamBattlePlaying(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(3880))
		{
			return WrappersManagerImpl.GetPatch(3880).__Gen_Wrap_172(gameType);
		}
		bool flag = false;
		PlayingUIConfig teamBattlePlayingUIConfigByGameType = GetTeamBattlePlayingUIConfigByGameType(gameType);
		if (teamBattlePlayingUIConfigByGameType != null)
		{
			flag = teamBattlePlayingUIConfigByGameType.roomStyle == 4 || teamBattlePlayingUIConfigByGameType.roomStyle == 5;
		}
		if (!flag)
		{
			return CheckHasTeamBattleVO();
		}
		return true;
	}

	public static bool IsV1TeamBattlePlaying(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(61976))
		{
			return WrappersManagerImpl.GetPatch(61976).__Gen_Wrap_172(gameType);
		}
		bool result = false;
		PlayingUIConfig teamBattlePlayingUIConfigByGameType = GetTeamBattlePlayingUIConfigByGameType(gameType);
		if (teamBattlePlayingUIConfigByGameType != null)
		{
			return IsPlayingUIConfigTeamBattleV1(teamBattlePlayingUIConfigByGameType);
		}
		return result;
	}

	public static bool IsV2TeamBattlePlaying(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(2094))
		{
			return WrappersManagerImpl.GetPatch(2094).__Gen_Wrap_172(gameType);
		}
		bool result = false;
		PlayingUIConfig teamBattlePlayingUIConfigByGameType = GetTeamBattlePlayingUIConfigByGameType(gameType);
		if (teamBattlePlayingUIConfigByGameType != null)
		{
			return IsPlayingUIConfigTeamBattleV2(teamBattlePlayingUIConfigByGameType);
		}
		return result;
	}

	public static List<PlayingUIConfig> GetTeamBattlePlayingListV1()
	{
		if (WrappersManagerImpl.IsPatched(61977))
		{
			return WrappersManagerImpl.GetPatch(61977).__Gen_Wrap_4180();
		}
		return ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig().Where(IsPlayingUIConfigTeamBattleV1)
			.ToList();
	}

	public static List<PlayingUIConfig> GetTeamBattlePlayingListV2()
	{
		if (WrappersManagerImpl.IsPatched(61978))
		{
			return WrappersManagerImpl.GetPatch(61978).__Gen_Wrap_4180();
		}
		return ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig().Where(IsPlayingUIConfigTeamBattleV2)
			.ToList();
	}

	public static bool IsPlayingUIConfigTeamBattleV1(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(55972))
		{
			return WrappersManagerImpl.GetPatch(55972).__Gen_Wrap_3(config);
		}
		if (config != null && (config.roomStyle == 4 || config.roomStyle == 5))
		{
			return config.extraInfo == "v1";
		}
		return false;
	}

	public static bool IsPlayingUIConfigTeamBattleV2(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(2098))
		{
			return WrappersManagerImpl.GetPatch(2098).__Gen_Wrap_3(config);
		}
		if (config != null && (config.roomStyle == 4 || config.roomStyle == 5))
		{
			return config.extraInfo == "v2";
		}
		return false;
	}

	public static bool IsRoomConfigTeamBattleV1(RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(55969))
		{
			return WrappersManagerImpl.GetPatch(55969).__Gen_Wrap_3(config);
		}
		int scene_id = config?.sceneId ?? 0;
		if (scene_id > 0)
		{
			return IsPlayingUIConfigTeamBattleV1(ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig().Find((PlayingUIConfig p) => p.rooms.Contains(scene_id)));
		}
		return false;
	}

	public static bool IsRoomConfigTeamBattleV2(RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(55973))
		{
			return WrappersManagerImpl.GetPatch(55973).__Gen_Wrap_3(config);
		}
		int scene_id = config?.sceneId ?? 0;
		if (scene_id > 0)
		{
			return IsPlayingUIConfigTeamBattleV2(ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig().Find((PlayingUIConfig p) => p.rooms.Contains(scene_id)));
		}
		return false;
	}

	public static int GetTeamBattleRealSvrGameType(int gametype)
	{
		if (WrappersManagerImpl.IsPatched(3879))
		{
			return WrappersManagerImpl.GetPatch(3879).__Gen_Wrap_144(gametype);
		}
		if (IsTeamBattlePlaying(gametype) && gametype > 10000)
		{
			return gametype - 10000;
		}
		return gametype;
	}

	public static int SvrGameTypeToClientTeamBattleGameType(int svrGameType)
	{
		if (WrappersManagerImpl.IsPatched(61979))
		{
			return WrappersManagerImpl.GetPatch(61979).__Gen_Wrap_144(svrGameType);
		}
		return svrGameType switch
		{
			149 => 10149, 
			379 => 10379, 
			180 => 10180, 
			_ => svrGameType, 
		};
	}

	public static PlayingUIConfig GetTeamBattlePlayingUIConfigByGameType(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(2095))
		{
			return WrappersManagerImpl.GetPatch(2095).__Gen_Wrap_453(gameType);
		}
		List<PlayingUIConfig> allCachedConfig = ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig();
		for (int i = 0; i < allCachedConfig.Count; i++)
		{
			PlayingUIConfig playingUIConfig = allCachedConfig[i];
			if (playingUIConfig.gameType == gameType && (playingUIConfig.roomStyle == 4 || playingUIConfig.roomStyle == 5))
			{
				return playingUIConfig;
			}
		}
		return null;
	}

	public static int GetTeamPlayingMainID(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(33305))
		{
			return WrappersManagerImpl.GetPatch(33305).__Gen_Wrap_144(gameType);
		}
		List<PlayingUIConfig> allCachedConfig = ProtoConfigLoader<PlayingUIConfig>.getInstance().getAllCachedConfig();
		string text = (TeamBattleModel.IsTeamBattleV2Open ? "v2" : "v1");
		for (int i = 0; i < allCachedConfig.Count; i++)
		{
			PlayingUIConfig playingUIConfig = allCachedConfig[i];
			if ((playingUIConfig.gameType == gameType && (playingUIConfig.roomStyle == 4 || playingUIConfig.roomStyle == 5) && playingUIConfig.extraInfo == text) || (gameType > 10000 && gameType == playingUIConfig.id))
			{
				return playingUIConfig.id;
			}
		}
		return gameType;
	}

	public static bool IsMatchRoom()
	{
		if (WrappersManagerImpl.IsPatched(34126))
		{
			return WrappersManagerImpl.GetPatch(34126).__Gen_Wrap_11();
		}
		if (SectionUtil.GetCurrentRoomSectionType() != RoomSectionType.RoomBeanMatch)
		{
			return SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomMatch;
		}
		return true;
	}

	public static bool IsPrivateRoom()
	{
		if (WrappersManagerImpl.IsPatched(36182))
		{
			return WrappersManagerImpl.GetPatch(36182).__Gen_Wrap_11();
		}
		return SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomPrivateRoom;
	}

	[IDTag(0)]
	public static bool IsRankModeRoom()
	{
		if (WrappersManagerImpl.IsPatched(4022))
		{
			return WrappersManagerImpl.GetPatch(4022).__Gen_Wrap_11();
		}
		if (SectionUtil.GetCurrentRoomSectionType() != RoomSectionType.RoomSectionScore)
		{
			return SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomSectionScoreAndBean;
		}
		return true;
	}

	[IDTag(1)]
	public static bool IsRankModeRoom(RoomConfig roomConfig)
	{
		if (WrappersManagerImpl.IsPatched(61980))
		{
			return WrappersManagerImpl.GetPatch(61980).__Gen_Wrap_3(roomConfig);
		}
		if (SectionUtil.GetRoomSectionType(roomConfig) != RoomSectionType.RoomSectionScore)
		{
			return SectionUtil.GetRoomSectionType(roomConfig) == RoomSectionType.RoomSectionScoreAndBean;
		}
		return true;
	}

	public static bool IsTeamRankPlaying(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(33845))
		{
			return WrappersManagerImpl.GetPatch(33845).__Gen_Wrap_172(gameType);
		}
		bool flag = false;
		PlayingUIConfig teamBattlePlayingUIConfigByGameType = GetTeamBattlePlayingUIConfigByGameType(gameType);
		if (teamBattlePlayingUIConfigByGameType == null)
		{
			Log.Info("playingconfig is null " + gameType, "TeamBattle");
		}
		else
		{
			flag = teamBattlePlayingUIConfigByGameType.roomStyle == 5;
		}
		if (!flag)
		{
			return CheckHasTeamBattleVO();
		}
		return true;
	}

	public static bool CheckHasTeamBattleVO()
	{
		if (WrappersManagerImpl.IsPatched(3881))
		{
			return WrappersManagerImpl.GetPatch(3881).__Gen_Wrap_11();
		}
		if (SingletonBase<GlobalDataModel>.GetInstance().globalModel?.enterGameInfo != null)
		{
			return SingletonBase<GlobalDataModel>.GetInstance().globalModel?.enterGameInfo.enterTeamBattleGameVO != null;
		}
		return false;
	}

	public static bool IsOldBalanceGameType(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(61981))
		{
			return WrappersManagerImpl.GetPatch(61981).__Gen_Wrap_172(gameType);
		}
		if (!IsTeamRankPlaying(gameType))
		{
			return IsTeamBattlePlaying(gameType);
		}
		return true;
	}

	public static GameType CheckTo2v2GameType(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(61982))
		{
			return WrappersManagerImpl.GetPatch(61982).__Gen_Wrap_2069(gameType);
		}
		GameType result = gameType;
		if (IsTeamRankPlaying((int)gameType))
		{
			switch (gameType)
			{
			case GameType.ModuleXueLiu:
				result = GameType.ModuleXueLiu2v2;
				break;
			case GameType.ModuleXueZhan:
				result = GameType.ModuleXueZhan2v2;
				break;
			case GameType.ModuleTuidaohu49:
				result = GameType.ModuleDaZhong2v2;
				break;
			case GameType.ModuleTuidaohu2V2:
				result = GameType.ModuleTuidaohu2V2;
				break;
			case GameType.ModuleHongZhongXueLiu:
				result = GameType.Module2V2HongZhongXueLiu;
				break;
			}
		}
		return result;
	}

	public static bool IsEntertainmentGame(GameType gametype)
	{
		if (WrappersManagerImpl.IsPatched(61983))
		{
			return WrappersManagerImpl.GetPatch(61983).__Gen_Wrap_2318(gametype);
		}
		if (IsPCPlatform() && gametype == GameType.ModuleDoubleXueLiu)
		{
			return false;
		}
		if (gametype == GameType.ModuleXueZhanLaiZi || gametype == GameType.ModuleBaoZouXueLiu || gametype == GameType.ModuleXueLiu2v2 || gametype == GameType.ModuleXueZhan2v2 || gametype == GameType.ModuleDaZhong2v2 || gametype == GameType.ModuleTuidaohu2V2 || gametype == GameType.ModuleDaZhongXueZhan || gametype == GameType.ModuleXueLiuZiXuanPai || gametype == GameType.ModuleXueLiuDraw2Card || gametype == GameType.ModuleDoubleXueLiu || IsTeamBattlePlaying((int)gametype))
		{
			return true;
		}
		return false;
	}

	public static bool GetPlayName(RoomConfig roomConfig, out string playName)
	{
		if (WrappersManagerImpl.IsPatched(43047))
		{
			return WrappersManagerImpl.GetPatch(43047).__Gen_Wrap_641(roomConfig, out playName);
		}
		string icon = roomConfig.icon;
		if (string.IsNullOrEmpty(icon) || icon.IndexOf('_') == -1 || icon.Split('_').Length < 2)
		{
			playName = null;
			return false;
		}
		if (!string.IsNullOrEmpty(roomConfig.sceneName))
		{
			playName = roomConfig.sceneName;
			return true;
		}
		switch (icon.Split('_')[0])
		{
		case "Cainiaochang":
			playName = "菜鸟场";
			break;
		case "Pingmingchang":
			playName = "平民场";
			break;
		case "Guanjiachang":
			playName = "官甲场";
			break;
		case "Tuhaochang":
			playName = "土豪场";
			break;
		case "Fushanchang":
			playName = "富商场";
			break;
		case "Zhunjuechang":
			playName = "尊爵场";
			break;
		case "Tiyanchang":
			playName = "体验场";
			break;
		default:
			playName = "菜鸟场";
			break;
		}
		return true;
	}

	public static string GetPCRoomIcon2Name(string icon)
	{
		if (WrappersManagerImpl.IsPatched(61984))
		{
			return WrappersManagerImpl.GetPatch(61984).__Gen_Wrap_19(icon);
		}
		string empty = string.Empty;
		return icon switch
		{
			"cn" => "菜鸟场", 
			"pm" => "平民场", 
			"gj" => "官甲场", 
			"th" => "土豪场", 
			"fs" => "富商场", 
			"zj" => "尊爵场", 
			_ => "菜鸟场", 
		};
	}

	public static bool IsWeekTicket(BuyGoodsMessage data)
	{
		if (WrappersManagerImpl.IsPatched(49704))
		{
			return WrappersManagerImpl.GetPatch(49704).__Gen_Wrap_3(data);
		}
		for (int i = 0; i < data.goodsList.Length; i++)
		{
			if (data.goodsList[i].nID == 39808888 || data.goodsList[i].nID == 33002)
			{
				return true;
			}
		}
		return false;
	}

	public static string TypeToString(MatchGameType type)
	{
		if (WrappersManagerImpl.IsPatched(47882))
		{
			return WrappersManagerImpl.GetPatch(47882).__Gen_Wrap_3272(type);
		}
		switch (type)
		{
		case MatchGameType.Guobiao:
		case MatchGameType.LocalGuobiao:
			return "国标麻将";
		case MatchGameType.WuHan:
			return LanguageKey.RED_YORIKO;
		case MatchGameType.XuanLiu:
			return LanguageKey.SHED_BLOOD_LIKE_WATER;
		case MatchGameType.XuanZhan:
			return LanguageKey.FIGHT_TO_THE_BITTER_END;
		case MatchGameType.ErRen:
			return LanguageKey.TWO_PEOPLE_MAHJONG;
		case MatchGameType.XuanLiuChangeThree:
			return "血流·换三张";
		case MatchGameType.ErRenGuohu:
			return "二人·过胡加倍";
		case MatchGameType.XuanZhanChangeThree:
			return "血战·换三张";
		case MatchGameType.GuangDong:
			return "广东麻将";
		default:
			return "";
		}
	}

	public static bool IsViewSearValid(int index)
	{
		if (WrappersManagerImpl.IsPatched(61985))
		{
			return WrappersManagerImpl.GetPatch(61985).__Gen_Wrap_172(index);
		}
		if (index >= 0)
		{
			return index < 4;
		}
		return false;
	}

	public static RoomConfig GetRoomConfigByID(int id)
	{
		if (WrappersManagerImpl.IsPatched(49404))
		{
			return WrappersManagerImpl.GetPatch(49404).__Gen_Wrap_3093(id);
		}
		List<RoomConfig> allCachedRoomConfigList = GetAllCachedRoomConfigList();
		for (int i = 0; i < allCachedRoomConfigList.Count; i++)
		{
			if (allCachedRoomConfigList[i].id == id)
			{
				return allCachedRoomConfigList[i];
			}
		}
		return null;
	}

	public static string GetMJNameByValue(int mjValue)
	{
		if (WrappersManagerImpl.IsPatched(61986))
		{
			return WrappersManagerImpl.GetPatch(61986).__Gen_Wrap_12(mjValue);
		}
		if (mjValue < 0)
		{
			Log.Error($"GetMJNameByValue: invalid mjValue {mjValue}", "Game");
			return "";
		}
		if (mjValue < 40)
		{
			int num = mjValue / 10 * 9 + mjValue % 10 - 1;
			if (num >= MJNames.Length)
			{
				Log.Error($"GetMJNameByValue: invalid mjValue {mjValue}", "Game");
				return "";
			}
			return MJNames[num];
		}
		return "";
	}

	public static int GetGameTypeIDByProvinceID(int provinceId)
	{
		if (WrappersManagerImpl.IsPatched(39119))
		{
			return WrappersManagerImpl.GetPatch(39119).__Gen_Wrap_144(provinceId);
		}
		return GetGameTypeIDByAdCode(provinceId);
	}

	public static List<int> GetGameTypeIDListByProvinceID(int provinceId)
	{
		if (WrappersManagerImpl.IsPatched(56282))
		{
			return WrappersManagerImpl.GetPatch(56282).__Gen_Wrap_3817(provinceId);
		}
		List<int> list = new List<int>();
		int gameTypeIDByProvinceID = GetGameTypeIDByProvinceID(provinceId);
		if (gameTypeIDByProvinceID > 0)
		{
			list.Add(gameTypeIDByProvinceID);
			List<GameTypePosition> allCachedConfig = ProtoConfigLoader<GameTypePosition>.getInstance().getAllCachedConfig();
			if (allCachedConfig == null || allCachedConfig.Count == 0)
			{
				return list;
			}
			string text = provinceId.ToString();
			foreach (GameTypePosition item in allCachedConfig)
			{
				if (text.StartsWith(item.adCodePrefix.ToString()))
				{
					if (item.moreGameType.Count > 0)
					{
						list.AddRange(item.moreGameType);
					}
					break;
				}
			}
		}
		return list;
	}

	public static int GetGameTypeIDByAdCode(int adcode)
	{
		if (WrappersManagerImpl.IsPatched(39120))
		{
			return WrappersManagerImpl.GetPatch(39120).__Gen_Wrap_144(adcode);
		}
		Log.Info("GetGameTypeIDByAdCode adcode is " + adcode, "Game");
		if (adcode <= 0)
		{
			return -1;
		}
		List<GameTypePosition> allCachedConfig = ProtoConfigLoader<GameTypePosition>.getInstance().getAllCachedConfig();
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			return -1;
		}
		string text = adcode.ToString();
		foreach (GameTypePosition item in allCachedConfig)
		{
			if (text.StartsWith(item.adCodePrefix.ToString()))
			{
				return item.gameType;
			}
		}
		return -1;
	}

	public static GameTypePosition GetGameTypePositionByAdCode(int adcode)
	{
		if (WrappersManagerImpl.IsPatched(40309))
		{
			return WrappersManagerImpl.GetPatch(40309).__Gen_Wrap_2709(adcode);
		}
		Log.Info("GetGameTypePositionByAdCode adcode is " + adcode, "Game");
		if (adcode <= 0)
		{
			return null;
		}
		List<GameTypePosition> allCachedConfig = ProtoConfigLoader<GameTypePosition>.getInstance().getAllCachedConfig();
		if (allCachedConfig == null || allCachedConfig.Count == 0)
		{
			return null;
		}
		string text = adcode.ToString();
		foreach (GameTypePosition item in allCachedConfig)
		{
			if (text.StartsWith(item.adCodePrefix.ToString()))
			{
				return item;
			}
		}
		return null;
	}

	public static RoomConfig GetRespectGuangDongRankScene()
	{
		if (WrappersManagerImpl.IsPatched(44483))
		{
			return WrappersManagerImpl.GetPatch(44483).__Gen_Wrap_3040();
		}
		PlayingUIConfig configByKey = ProtoConfigLoader<PlayingUIConfig>.getInstance().getConfigByKey(377);
		if (configByKey != null)
		{
			for (int i = 0; i < configByKey.rooms.Count; i++)
			{
				RoomConfig configByKey2 = ProtoConfigLoader<RoomConfig>.getInstance().getConfigByKey(configByKey.rooms[i]);
				if (configByKey2 != null && configByKey2.playingUI_ID != 153 && IsSelfBeansFitRoom(configByKey2, PlayerDataMgr.GetInstance().PlayerBeanNum))
				{
					return configByKey2;
				}
			}
		}
		return null;
	}

	public static void SetLastPlayTypeAndSceneIDStr(GameType gameType, RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(61987))
		{
			WrappersManagerImpl.GetPatch(61987).__Gen_Wrap_2316(gameType, config);
			return;
		}
		int sceneId = config.sceneId;
		bool flag = SectionUtil.GetRoomSectionType(config) == RoomSectionType.RoomSectionScore;
		string empty = string.Empty;
		StringBuilder stringBuilder = new StringBuilder();
		if (!PlayerPrefsHelper.HasKey("lastPlayTypeAndSceneIDStr"))
		{
			int num = (int)gameType;
			stringBuilder.Append(num.ToString());
			stringBuilder.Append(":");
			stringBuilder.Append(sceneId.ToString());
			stringBuilder.Append(";");
			PlayerPrefsHelper.SetString("lastPlayTypeAndSceneIDStr", stringBuilder.ToString());
			if (!flag)
			{
				PlayerPrefsHelper.SetString("lastNotFreePlayTypeAndSceneIDStr", stringBuilder.ToString());
			}
			return;
		}
		empty = PlayerPrefsHelper.GetString("lastPlayTypeAndSceneIDStr");
		string[] array = empty.Split(';');
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		bool flag2 = false;
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length >= 2)
			{
				if (gameType == (GameType)int.Parse(array2[0]))
				{
					dictionary.Add((int)gameType, sceneId);
					flag2 = true;
				}
				else
				{
					dictionary.Add(int.Parse(array2[0]), int.Parse(array2[1]));
				}
			}
		}
		if (!flag2)
		{
			dictionary.Add((int)gameType, sceneId);
		}
		List<int> list = new List<int>(dictionary.Keys);
		for (int j = 0; j < list.Count; j++)
		{
			stringBuilder.Append(list[j].ToString());
			stringBuilder.Append(":");
			stringBuilder.Append(dictionary[list[j]].ToString());
			stringBuilder.Append(";");
		}
		PlayerPrefsHelper.SetString("lastPlayTypeAndSceneIDStr", stringBuilder.ToString());
		if (!flag)
		{
			PlayerPrefsHelper.SetString("lastNotFreePlayTypeAndSceneIDStr", stringBuilder.ToString());
		}
	}

	public static int GetLastPlaySceneID(GameType gameType, bool notFree)
	{
		if (WrappersManagerImpl.IsPatched(35598))
		{
			return WrappersManagerImpl.GetPatch(35598).__Gen_Wrap_2366(gameType, notFree);
		}
		string empty = string.Empty;
		string key = (notFree ? "lastNotFreePlayTypeAndSceneIDStr" : "lastPlayTypeAndSceneIDStr");
		if (!PlayerPrefsHelper.HasKey(key))
		{
			return 0;
		}
		empty = PlayerPrefsHelper.GetString(key);
		string[] array = empty.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length >= 2 && gameType == (GameType)int.Parse(array2[0]))
			{
				return int.Parse(array2[1]);
			}
		}
		return 0;
	}

	[IDTag(1)]
	public static GameType GetLastPlayGameType(bool bIncludeFree = true)
	{
		if (WrappersManagerImpl.IsPatched(61988))
		{
			return WrappersManagerImpl.GetPatch(61988).__Gen_Wrap_4181(bIncludeFree);
		}
		string empty = string.Empty;
		string key = (bIncludeFree ? "lastPlayTypeAndSceneIDStr" : "lastNotFreePlayTypeAndSceneIDStr");
		if (!PlayerPrefsHelper.HasKey(key))
		{
			return GameType.None;
		}
		empty = PlayerPrefsHelper.GetString(key);
		string[] array = empty.Split(';');
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (!string.IsNullOrEmpty(array[num]))
			{
				string[] array2 = array[num].Split(':');
				if (array2.Length >= 2)
				{
					return (GameType)int.Parse(array2[0]);
				}
			}
		}
		return GameType.None;
	}

	public static void GetRoomBaseInfo(RoomConfig roomConfig, out string fanName, out string innerType)
	{
		if (WrappersManagerImpl.IsPatched(61989))
		{
			WrappersManagerImpl.GetPatch(61989).__Gen_Wrap_4182(roomConfig, out fanName, out innerType);
			return;
		}
		fanName = "";
		innerType = "";
		if (roomConfig == null || roomConfig.type <= 0)
		{
			return;
		}
		long min = roomConfig.min;
		switch ((GameSubType)roomConfig.type)
		{
		case GameSubType.ZhongKaiKou:
			fanName = "开口番·无起胡番";
			break;
		case GameSubType.ZhongKoukou:
			fanName = "口口番·无起胡番";
			break;
		case GameSubType.ZhongFaKaikou:
			fanName = "中发杠·开口番·无起胡番";
			break;
		case GameSubType.ZhongFaKouKou:
			fanName = "中发杠·口口番·无起胡番";
			break;
		case GameSubType.ZhongKaiKouQihu:
			fanName = "开口番·无起胡番";
			if (min > 0)
			{
				fanName = "开口番·" + min + "倍起胡";
			}
			break;
		case GameSubType.ZhongKoukouQihu:
			fanName = "口口番·无起胡番";
			if (min > 0)
			{
				fanName = "口口番·" + min + "倍起胡";
			}
			break;
		case GameSubType.ZhongFaKaikouQihu:
			fanName = "中发杠·开口番·无起胡番";
			if (min > 0)
			{
				fanName = "中发杠·" + min + "倍起胡";
			}
			break;
		case GameSubType.ZhongFaKouKouQihu:
			fanName = "中发杠·口口番·无起胡番";
			if (min > 0)
			{
				fanName = "中发杠·" + min + "倍起胡";
			}
			break;
		case GameSubType.FaKaikou:
			innerType = "Label_Facai";
			fanName = "开口番·无起胡番";
			break;
		case GameSubType.FaKoukou:
			innerType = "Label_Facai";
			fanName = "口口番·无起胡番";
			break;
		case GameSubType.FaKaikouQihu:
			innerType = "Label_Facai";
			fanName = "开口番·无起胡番";
			if (min > 0)
			{
				fanName = "开口番·" + min + "倍起胡";
			}
			break;
		case GameSubType.FaKoukouQihu:
			innerType = "Label_Facai";
			fanName = "口口番·无起胡番";
			if (min > 0)
			{
				fanName = "口口番·" + min + "倍起胡";
			}
			break;
		case GameSubType.Huan3Zhang:
			innerType = "Huansanzhang_Icon";
			break;
		case GameSubType.RenYiHuan:
			innerType = "Label_Facai";
			fanName = "任意换";
			break;
		case GameSubType.ErRenGuoHuJiaBei:
			innerType = "Guohui_Label";
			break;
		case GameSubType.DaZhongZhaNiao:
			innerType = "Zhaniao_Label1";
			if (IsSecondType(GameSecondType.Dapiao, roomConfig.secondType))
			{
				innerType = "Label_Dapiaozhaniao";
			}
			break;
		case GameSubType.PiLai:
		{
			int secondType = SetSceondType(roomConfig.secondType);
			if (IsSecondType(GameSecondType.Hongzhong, secondType) && IsSecondType(GameSecondType.PiLai, secondType))
			{
				innerType = "Label_Hongzhong";
			}
			else
			{
				innerType = "Label_Qi";
			}
			string empty = string.Empty;
			empty = ((!IsSecondType(GameSecondType.Kaikou, secondType)) ? (empty + "口口番·") : (empty + "开口番·"));
			empty = ((!IsSecondType(GameSecondType.Qihu, secondType)) ? (empty + "无起胡番") : ((min <= 0) ? (empty + "无起胡番") : (empty + min + "倍起胡")));
			fanName = empty;
			break;
		}
		case GameSubType.DuSanLaoZhuang:
			innerType = "DuSanLaoZhuang";
			break;
		}
		if (roomConfig.playingUI_ID == 6)
		{
			if (IsSecondType(GameSecondType.Yuanlai, roomConfig.secondType))
			{
				innerType = "Lable_Wuzipai";
			}
			else if (GetSecondValue(GameSecondType.Kaikou, roomConfig.secondType) <= 0)
			{
				innerType = "Label_Wulaizi";
			}
			int secondValue = GetSecondValue(GameSecondType.Qihu, roomConfig.secondType);
			string text = "";
			if (secondValue > 0)
			{
				text = secondValue + "匹马";
			}
			int secondValue2 = GetSecondValue(GameSecondType.Kaikou, roomConfig.secondType);
			if (secondValue2 > 0)
			{
				if (secondValue > 0)
				{
					text += "·";
				}
				text += "有赖子";
			}
			if (GetSecondValue(GameSecondType.PiLai, roomConfig.secondType) > 0)
			{
				if (secondValue2 > 0 || secondValue > 0)
				{
					text += "·";
				}
				text += "可吃";
			}
			fanName = text;
		}
		else if (roomConfig.playingUI_ID == 7)
		{
			bool num = IsSecondType(GameSecondType.Dapiao, roomConfig.secondType);
			int secondValue3 = GetSecondValue(GameSecondType.ZhaNiaoNum, roomConfig.secondType);
			string text2 = "扎";
			text2 += secondValue3;
			text2 += "鸟";
			if (num)
			{
				text2 += ".打漂场";
			}
			fanName = text2;
		}
		else if (roomConfig.playingUI_ID == 124)
		{
			List<string> list = new List<string>();
			if (GetSecondValue(0, roomConfig.secondType) == 1)
			{
				list.Add("满堂鸡");
			}
			if (GetSecondValue(1, roomConfig.secondType) == 1)
			{
				list.Add("乌骨鸡");
			}
			if (GetSecondValue(2, roomConfig.secondType) == 1)
			{
				list.Add("上下鸡");
			}
			if (list.Count > 0)
			{
				fanName = string.Join(",", list.ToArray());
			}
		}
		else if (roomConfig.playingUI_ID == 135)
		{
			List<string> list2 = new List<string>();
			if (GetSecondValue(0, roomConfig.secondType) == 1)
			{
				list2.Add("奖马");
			}
			int secondValue4 = GetSecondValue(1, roomConfig.secondType);
			if (secondValue4 != 0)
			{
				list2.Add("封顶" + secondValue4 + "倍");
			}
			if (list2.Count > 0)
			{
				fanName = string.Join(",", list2.ToArray());
			}
		}
		else if (roomConfig.playingUI_ID == 370 || roomConfig.playingUI_ID == 371 || roomConfig.playingUI_ID == 380 || roomConfig.playingUI_ID == 381 || roomConfig.playingUI_ID == 390 || roomConfig.playingUI_ID == 389)
		{
			fanName = roomConfig.subName;
		}
	}

	public static string GetBeanRangeInfo(long maxBean, long minBean)
	{
		if (WrappersManagerImpl.IsPatched(61990))
		{
			return WrappersManagerImpl.GetPatch(61990).__Gen_Wrap_2500(maxBean, minBean);
		}
		string text = "";
		if (maxBean <= -1 && minBean <= -1)
		{
			return "";
		}
		if (maxBean <= -1 && minBean >= 0)
		{
			float num = (float)minBean / 1000f;
			if (num >= 10f)
			{
				return string.Format(LanguageKey.ANY_TEN_THOUSAND_BEAN, num / 10f);
			}
			return string.Format(LanguageKey.ANY_THOUSAND_BEAN, num);
		}
		float num2 = (float)minBean / 1000f;
		float num3 = (float)maxBean / 1000f;
		text = ((!(num2 >= 10f)) ? string.Format(LanguageKey.ANY_THOUSAND, num2) : string.Format(LanguageKey.ANY_TEN_THOUSAND, num2 / 10f));
		if (num3 >= 10f)
		{
			return string.Format(LanguageKey.ANY_AND_ANY_TEN_THOUSAND_BEAN, text, num3 / 10f);
		}
		return string.Format(LanguageKey.ANY_AND_ANY_THOUSAND, text, num3);
	}

	public static string GetFanBeiStrInfo(long fanBeiNum)
	{
		if (WrappersManagerImpl.IsPatched(61991))
		{
			return WrappersManagerImpl.GetPatch(61991).__Gen_Wrap_1190(fanBeiNum);
		}
		string text = "";
		float num = (float)fanBeiNum / 1000f;
		if (num >= 100000f && num < 1E+09f)
		{
			return string.Format(LanguageKey.ANY_YI, num / 100000f);
		}
		if (num >= 1E+09f && num < 1E+13f)
		{
			return string.Format(LanguageKey.ANY_ZHAO, num / 1E+09f);
		}
		if (num >= 1E+13f)
		{
			return string.Format(LanguageKey.ANY_JING, num / 1E+13f);
		}
		return fanBeiNum.ToString();
	}

	private static string GetNewBeanStrInfo(long beanNum)
	{
		if (WrappersManagerImpl.IsPatched(36890))
		{
			return WrappersManagerImpl.GetPatch(36890).__Gen_Wrap_1190(beanNum);
		}
		string text = "";
		float num = (float)beanNum / 1000f;
		if (num >= 10f && num < 100000f)
		{
			return string.Format(LanguageKey.ANY_TEN_THOUSAND, num / 10f);
		}
		if (num >= 100000f && num < 1E+09f)
		{
			return string.Format(LanguageKey.ANY_YI, num / 100000f);
		}
		if (num >= 1E+09f && num < 1E+13f)
		{
			return string.Format(LanguageKey.ANY_ZHAO, num / 1E+09f);
		}
		if (num >= 1E+13f)
		{
			return string.Format(LanguageKey.ANY_JING, num / 1E+13f);
		}
		return string.Format(LanguageKey.ANY_THOUSAND, num);
	}

	public static string GetNewBeanRangeInfo(long maxBean, long minBean)
	{
		if (WrappersManagerImpl.IsPatched(36889))
		{
			return WrappersManagerImpl.GetPatch(36889).__Gen_Wrap_2500(maxBean, minBean);
		}
		string text = "";
		if (maxBean <= -1 && minBean <= -1)
		{
			return "";
		}
		if (maxBean == -1)
		{
			return GetNewBeanStrInfo(minBean) + "以上";
		}
		string newBeanStrInfo = GetNewBeanStrInfo(maxBean);
		string newBeanStrInfo2 = GetNewBeanStrInfo(minBean);
		return $"{newBeanStrInfo2}-{newBeanStrInfo}";
	}

	public static CurrencyType GetCurrencyType(IGlobalModel globalModel)
	{
		if (WrappersManagerImpl.IsPatched(61992))
		{
			return WrappersManagerImpl.GetPatch(61992).__Gen_Wrap_4183(globalModel);
		}
		CurrencyType currencyType = CurrencyType.Bean;
		if (IsGoldCoinRoom(globalModel.enterGameInfo.roomInfo))
		{
			return CurrencyType.GoldCoin;
		}
		if (IsPracticeRoom(globalModel.enterGameInfo.roomInfo))
		{
			return CurrencyType.Practice;
		}
		if (globalModel.enterGameInfo.matchType != MatchType.None)
		{
			if (globalModel.enterGameInfo.olympicsMatchInfo.bBeanMatch)
			{
				return CurrencyType.BeanAndScore;
			}
			return CurrencyType.Score;
		}
		if (globalModel.enterGameInfo.enterGameSecretRoomVO.isSecretRoom)
		{
			return CurrencyType.Score;
		}
		if (SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomSectionScore)
		{
			return CurrencyType.SectionScore;
		}
		return CurrencyType.Bean;
	}

	public static List<MatchRewardItem> ConvertMatchReward(string reward)
	{
		if (WrappersManagerImpl.IsPatched(47635))
		{
			return WrappersManagerImpl.GetPatch(47635).__Gen_Wrap_3263(reward);
		}
		reward = reward.Trim();
		List<MatchRewardItem> list = new List<MatchRewardItem>();
		string[] array = reward.Split('+');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			if (array2.Length == 2)
			{
				MatchRewardItem item = default(MatchRewardItem);
				item.ID = int.Parse(array2[0]);
				item.Name = ItemIconHandlerUgui.GetItemName(item.ID);
				item.Num = int.Parse(array2[1]);
				list.Add(item);
			}
		}
		return list;
	}

	public static string ConvertMatchRewardString(List<MatchRewardItem> rewardName)
	{
		if (WrappersManagerImpl.IsPatched(47660))
		{
			return WrappersManagerImpl.GetPatch(47660).__Gen_Wrap_19(rewardName);
		}
		string text = "";
		for (int i = 0; i < rewardName.Count; i++)
		{
			text = text + rewardName[i].Name + "x" + rewardName[i].Num;
			text += " ";
		}
		return text.TrimEnd();
	}

	[IDTag(1)]
	public static string GetCountStr(ItemConfig item, int count)
	{
		if (WrappersManagerImpl.IsPatched(60367))
		{
			return WrappersManagerImpl.GetPatch(60367).__Gen_Wrap_76(item, count);
		}
		string result = string.Empty;
		if (item.itemOpType == 11 || item.itemOpType == 26)
		{
			float num = (float)count / 3600f;
			float num2 = num / 24f;
			float num3 = num % 24f;
			if ((double)num2 >= 1.0)
			{
				result = Mathf.Ceil(num2) + LanguageKey.DAY;
			}
			else if ((double)num2 < 1.0 && num3 >= 0f)
			{
				result = Mathf.Ceil(num3) + LanguageKey.HOUR;
			}
		}
		return result;
	}

	public static bool IsRiichiRoom(RoomConfig roomInfo)
	{
		if (WrappersManagerImpl.IsPatched(35522))
		{
			return WrappersManagerImpl.GetPatch(35522).__Gen_Wrap_3(roomInfo);
		}
		if (roomInfo.playingUI_ID != 319)
		{
			return roomInfo.playingUI_ID == 194;
		}
		return true;
	}

	public static bool CheckIsSupportRealTimeVoice(RealTimeScene scene, uint memberClientVersionNum, int ClientType)
	{
		if (WrappersManagerImpl.IsPatched(61993))
		{
			return WrappersManagerImpl.GetPatch(61993).__Gen_Wrap_4184(scene, memberClientVersionNum, ClientType);
		}
		bool result = true;
		if (ClientType != 64 && ClientType != 13 && ClientType != 70)
		{
			return false;
		}
		switch (scene)
		{
		case RealTimeScene.MainScene:
		{
			uint versionByStr3 = TSDK4CSharpService.GetVersionByStr("7.5.81");
			result = ((memberClientVersionNum >= versionByStr3) ? true : false);
			break;
		}
		case RealTimeScene.GameScene:
		{
			uint versionByStr = TSDK4CSharpService.GetVersionByStr("7.5.61");
			uint versionByStr2 = TSDK4CSharpService.GetVersionByStr("7.5.79");
			result = ((memberClientVersionNum < versionByStr || memberClientVersionNum > versionByStr2) ? true : false);
			break;
		}
		}
		return result;
	}

	public static int GetPersonalRoundLimit()
	{
		if (WrappersManagerImpl.IsPatched(53262))
		{
			return WrappersManagerImpl.GetPatch(53262).__Gen_Wrap_101();
		}
		if (SingletonBase<DynamicConfig>.GetInstance().GetObject("MiscConfigCfg.json", "misc_config") is Dictionary<string, object> dictionary && dictionary.ContainsKey("private_room_daily_match_times"))
		{
			uint result = Convert.ToUInt32(dictionary["private_room_daily_match_times"]);
			Log.Info("GetPersonalRoundLimit " + result, "PrivateRoom");
			return (int)result;
		}
		return 0;
	}

	public static int GetBalanceOrderIdByViewSeat(int bigWinnerViewSeat, int viewSeat, int playerCount)
	{
		if (WrappersManagerImpl.IsPatched(61994))
		{
			return WrappersManagerImpl.GetPatch(61994).__Gen_Wrap_303(bigWinnerViewSeat, viewSeat, playerCount);
		}
		switch (playerCount)
		{
		case 4:
			return (viewSeat - bigWinnerViewSeat + playerCount) % playerCount;
		case 3:
			if (bigWinnerViewSeat == viewSeat)
			{
				return 0;
			}
			switch (bigWinnerViewSeat)
			{
			case 0:
				return viewSeat;
			case 1:
				if (viewSeat != 3)
				{
					return 3;
				}
				return 1;
			case 3:
				if (viewSeat != 0)
				{
					return 3;
				}
				return 1;
			}
			break;
		case 2:
			if (bigWinnerViewSeat != viewSeat)
			{
				return 2;
			}
			return 0;
		}
		return 0;
	}

	public static int GetViewSeatByBalanceOrderId(int bigWinnerViewSeat, int orderId, int playerCount)
	{
		if (WrappersManagerImpl.IsPatched(61995))
		{
			return WrappersManagerImpl.GetPatch(61995).__Gen_Wrap_303(bigWinnerViewSeat, orderId, playerCount);
		}
		switch (playerCount)
		{
		case 4:
			return (bigWinnerViewSeat + orderId + playerCount) % playerCount;
		case 3:
			if (orderId == 3)
			{
				orderId = 2;
			}
			return (bigWinnerViewSeat + orderId + playerCount) % playerCount;
		case 2:
			return orderId switch
			{
				0 => 3, 
				2 => 1, 
				_ => (bigWinnerViewSeat + orderId - 1) % 4, 
			};
		default:
			return MJDef.INVALID_SEAT;
		}
	}

	public static bool HasPrivateRoomInviteInfo()
	{
		if (WrappersManagerImpl.IsPatched(3889))
		{
			return WrappersManagerImpl.GetPatch(3889).__Gen_Wrap_11();
		}
		if (SingletonBase<PlayerLoginDataMgr>.GetInstance().WakeUpInfo != null && (SingletonBase<OpenGameHelper>.GetInstance().HasPrivateInviteInfo() || SingletonBase<PersonalRoomController>.GetInstance().GetInviteInfo().roomId != ""))
		{
			return true;
		}
		return false;
	}

	public static string SelectWanfaUiAbName()
	{
		if (WrappersManagerImpl.IsPatched(36654))
		{
			return WrappersManagerImpl.GetPatch(36654).__Gen_Wrap_51();
		}
		return "WanfaNew";
	}

	public static float GetInGameAnimSpeedMultipler()
	{
		if (WrappersManagerImpl.IsPatched(61996))
		{
			return WrappersManagerImpl.GetPatch(61996).__Gen_Wrap_27();
		}
		return (float)SingletonBase<DynamicConfig>.GetInstance().GetDouble("InGameEffect", "Speed", 1.25);
	}

	public static float GetInGameAnimDurationMultiplier()
	{
		if (WrappersManagerImpl.IsPatched(61997))
		{
			return WrappersManagerImpl.GetPatch(61997).__Gen_Wrap_27();
		}
		float num = GetInGameAnimSpeedMultipler() - 1f;
		if (num >= 1f)
		{
			num = 0.9f;
		}
		if (num <= 0f)
		{
			num = 0f;
		}
		return 1f - num;
	}

	public static bool IsRankWafaLocked()
	{
		if (WrappersManagerImpl.IsPatched(57996))
		{
			return WrappersManagerImpl.GetPatch(57996).__Gen_Wrap_11();
		}
		int unLockRoundNum = WanfaHandler.GetUnLockRoundNum(MainUIState.RankGrade);
		PlayerRecordItem recordByUIN = SingletonBase<PlayerRecordModel>.GetInstance().GetRecordByUIN(SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin());
		if (recordByUIN != null && recordByUIN.commonInfo.totalRound < unLockRoundNum)
		{
			Log.Info($"CheckStateIsLock totalRound is:{recordByUIN.commonInfo.totalRound}, rankUnLockNum is: {unLockRoundNum}", "Selection");
			return true;
		}
		return false;
	}

	public static string GetRoomBeanRange(RoomConfig config)
	{
		if (WrappersManagerImpl.IsPatched(61998))
		{
			return WrappersManagerImpl.GetPatch(61998).__Gen_Wrap_19(config);
		}
		string empty = string.Empty;
		if (config.maxBean == -1 && config.minBean == -1)
		{
			return "";
		}
		if (config.maxBean == -1 && config.minBean >= 0)
		{
			long num = config.minBean / 1000;
			if (num >= 10)
			{
				return string.Format(LanguageKey.ANY_TEN_THOUSAND_BEAN, (float)num / 10f);
			}
			return string.Format(LanguageKey.ANY_THOUSAND_BEAN, num);
		}
		long num2 = config.minBean / 1000;
		long num3 = config.maxBean / 1000;
		empty = ((num2 < 10) ? string.Format(LanguageKey.ANY_THOUSAND, num2) : string.Format(LanguageKey.ANY_TEN_THOUSAND, (float)num2 / 10f));
		if (num3 >= 10)
		{
			return string.Format(LanguageKey.ANY_AND_ANY_TEN_THOUSAND_BEAN, empty, (float)num3 / 10f);
		}
		return string.Format(LanguageKey.ANY_AND_ANY_THOUSAND, empty, num3);
	}

	public static bool CanShowNextGameBtnAfterGiveUp(IGlobalModel globalModel, GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(61999))
		{
			return WrappersManagerImpl.GetPatch(61999).__Gen_Wrap_2181(globalModel, gameType);
		}
		return AppRoot.GetInstance().InGameBridgeBase.CanShowNextGameBtnAfterGiveUp(globalModel, gameType);
	}

	public static int GetTimeLineIdByOrderId(int orderId)
	{
		if (WrappersManagerImpl.IsPatched(62000))
		{
			return WrappersManagerImpl.GetPatch(62000).__Gen_Wrap_144(orderId);
		}
		return orderId switch
		{
			0 => 1, 
			1 => 2, 
			2 => 3, 
			_ => 0, 
		};
	}

	public static bool IsContainGoldCoinRoom(PlayingUIConfig config)
	{
		if (WrappersManagerImpl.IsPatched(49403))
		{
			return WrappersManagerImpl.GetPatch(49403).__Gen_Wrap_3(config);
		}
		if (config == null)
		{
			return false;
		}
		foreach (int room in config.rooms)
		{
			RoomConfig roomConfigByID = GetRoomConfigByID(room);
			if (roomConfigByID != null && IsGoldCoinRoom(roomConfigByID))
			{
				return true;
			}
		}
		foreach (int item in config.unionRoom)
		{
			RoomConfig roomConfigByID2 = GetRoomConfigByID(item);
			if (roomConfigByID2 != null && IsGoldCoinRoom(roomConfigByID2))
			{
				return true;
			}
		}
		return false;
	}

	public static long GetInt64OrInt32(long int64Value, int int32Value)
	{
		if (WrappersManagerImpl.IsPatched(62001))
		{
			return WrappersManagerImpl.GetPatch(62001).__Gen_Wrap_2740(int64Value, int32Value);
		}
		if (int64Value != 0L)
		{
			return int64Value;
		}
		return int32Value;
	}

	public static CurrencyType GetCurrencyTypeByRoomConfig(RoomConfig roomConfig)
	{
		if (WrappersManagerImpl.IsPatched(62002))
		{
			return WrappersManagerImpl.GetPatch(62002).__Gen_Wrap_4183(roomConfig);
		}
		if (IsGoldCoinRoom(roomConfig))
		{
			return CurrencyType.GoldCoin;
		}
		if (IsPracticeRoom(roomConfig))
		{
			return CurrencyType.Practice;
		}
		return CurrencyType.Bean;
	}

	public static List<RoomConfig> GetAllRoomConfigListInPrivateRoomConfigSheet()
	{
		if (WrappersManagerImpl.IsPatched(42333))
		{
			return WrappersManagerImpl.GetPatch(42333).__Gen_Wrap_449();
		}
		List<RoomConfig> allCachedConfig = ProtoConfigLoader<RoomConfig>.getInstance().getAllCachedConfig();
		List<RoomConfig> list = new List<RoomConfig>();
		foreach (RoomConfig item in allCachedConfig)
		{
			if (item.moneyType == 6)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static bool isSupportHuPaiGaiZhang(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(62003))
		{
			return WrappersManagerImpl.GetPatch(62003).__Gen_Wrap_2318(gameType);
		}
		if (gameType != GameType.ModuleCompareTile)
		{
			return gameType == GameType.ModuleRichTile;
		}
		return true;
	}

	public static bool IsHBPGame()
	{
		if (WrappersManagerImpl.IsPatched(62004))
		{
			return WrappersManagerImpl.GetPatch(62004).__Gen_Wrap_11();
		}
		if (SingletonBase<ShopDataHelper>.GetInstance().globalModel == null)
		{
			return false;
		}
		if (SingletonBase<ShopDataHelper>.GetInstance().globalModel.enterGameInfo == null)
		{
			return false;
		}
		List<int> listInt = SingletonBase<DynamicConfig>.GetInstance().GetListInt("HBPGame", "SceneList");
		if (listInt != null)
		{
			int sceneId = SingletonBase<ShopDataHelper>.GetInstance().globalModel.enterGameInfo.roomInfo.sceneId;
			if (listInt.Contains(sceneId))
			{
				return true;
			}
		}
		return false;
	}

	public static RoomConfig GetRoomConfigInPrivateRoomConfigSheet(int sceneId)
	{
		if (WrappersManagerImpl.IsPatched(52759))
		{
			return WrappersManagerImpl.GetPatch(52759).__Gen_Wrap_3093(sceneId);
		}
		foreach (RoomConfig item in (from x in ProtoConfigLoader<RoomConfig>.getInstance().getAllCachedConfig()
			where x.moneyType == 6
			select x).ToList())
		{
			if (item.sceneId == sceneId)
			{
				return item;
			}
		}
		return null;
	}

	public static RoomConfig GetRoomConfigInPrivateRoomConfigSheetByGametype(int gameType)
	{
		if (WrappersManagerImpl.IsPatched(62005))
		{
			return WrappersManagerImpl.GetPatch(62005).__Gen_Wrap_3093(gameType);
		}
		foreach (RoomConfig item in (from x in ProtoConfigLoader<RoomConfig>.getInstance().getAllCachedConfig()
			where x.moneyType == 6
			select x).ToList())
		{
			if (item.playingUI_ID == gameType)
			{
				return item;
			}
		}
		return null;
	}

	public static long VerifyRecommendBeans(BuyBeansFrom from, RoomConfig roomConfig, RecBuyBeansType recBuyBeansType)
	{
		if (WrappersManagerImpl.IsPatched(35527))
		{
			return WrappersManagerImpl.GetPatch(35527).__Gen_Wrap_2343(from, roomConfig, recBuyBeansType);
		}
		long num = roomConfig.enterBuyBean;
		if (BuyBeansFrom.EnterRoom == from)
		{
			num = ((recBuyBeansType == RecBuyBeansType.Special) ? roomConfig.enterSpecBuyBean : roomConfig.enterBuyBean);
		}
		else if (BuyBeansFrom.InGame == from)
		{
			num = ((recBuyBeansType == RecBuyBeansType.Special) ? roomConfig.bankRuptSpecBuyBean : roomConfig.bankRuptBuyBean);
		}
		if (num / 1000000 == 398 || num == 0L)
		{
			return roomConfig.minBean;
		}
		return num;
	}

	public static int GetTutorialIndex(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(35365))
		{
			return WrappersManagerImpl.GetPatch(35365).__Gen_Wrap_2317(gameType);
		}
		switch (gameType)
		{
		case GameType.ModuleXueLiuBaoPai:
			return 4;
		case GameType.ModuleTuidaohu49:
		case GameType.ModuleNewDazhongXueZhan:
			if (!SingletonBase<DynamicConfig>.GetInstance().GetBool("TutorialOpenCtrl", "NewDaZhong"))
			{
				return -1;
			}
			return 6;
		default:
			return -1;
		}
	}

	public static bool IsUseOldRankUI(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(44986))
		{
			return WrappersManagerImpl.GetPatch(44986).__Gen_Wrap_2318(gameType);
		}
		return !SectionUtil.IsNewRankScoreScene();
	}

	public static void StartTimeoutReconnectAfterGive()
	{
		if (WrappersManagerImpl.IsPatched(62007))
		{
			WrappersManagerImpl.GetPatch(62007).__Gen_Wrap_46();
			return;
		}
		Log.Info("StartTimeoutReconnectAfterGive", "Game");
		VPTimer.In(3f, delegate
		{
			Log.Info("TimeoutReconnectAfterGive RequestReplay", "Game");
			TSDK4CSharpService.RequestEnterRoomDirect((uint)TSDK4CSharpService.svrId, (short)TSDK4CSharpService.roomId, 0, 0);
		}, null, "TimeOutReconnectAfterGive");
	}

	public static void CancelTimeoutReconnectAfterGive()
	{
		if (WrappersManagerImpl.IsPatched(62009))
		{
			WrappersManagerImpl.GetPatch(62009).__Gen_Wrap_46();
			return;
		}
		Log.Info("CancelTimeoutReconnectAfterGive", "Game");
		VPTimer.CancelAll("TimeOutReconnectAfterGive");
	}

	[IDTag(0)]
	public static bool IsGoldCoinRoom(RoomConfig roomConfig)
	{
		if (WrappersManagerImpl.IsPatched(1982))
		{
			return WrappersManagerImpl.GetPatch(1982).__Gen_Wrap_3(roomConfig);
		}
		if (roomConfig == null)
		{
			return false;
		}
		if (roomConfig.moneyType == 5)
		{
			return true;
		}
		return false;
	}

	[IDTag(1)]
	public static bool IsGoldCoinRoom(int sceneId)
	{
		if (WrappersManagerImpl.IsPatched(21618))
		{
			return WrappersManagerImpl.GetPatch(21618).__Gen_Wrap_172(sceneId);
		}
		RoomConfig roomConfigBySceneId = GetRoomConfigBySceneId((uint)sceneId);
		if (roomConfigBySceneId != null)
		{
			if (roomConfigBySceneId.moneyType == 5)
			{
				return true;
			}
		}
		else
		{
			Log.Info($"RoomConfig is null, sceneId: {sceneId}", "GoldCoin");
		}
		return false;
	}

	[IDTag(1)]
	public static bool IsPracticeRoom(int sceneId)
	{
		if (WrappersManagerImpl.IsPatched(62010))
		{
			return WrappersManagerImpl.GetPatch(62010).__Gen_Wrap_172(sceneId);
		}
		RoomConfig roomConfigBySceneId = GetRoomConfigBySceneId((uint)sceneId);
		if (roomConfigBySceneId != null)
		{
			if (GetSecondValue(GameSecondType.SectionType, roomConfigBySceneId.secondType) == 1)
			{
				return SectionUtil.IsNewRankScoreScene();
			}
			return false;
		}
		Log.Info($"RoomConfig is null, sceneId: {sceneId}", "GoldCoin");
		return false;
	}

	[IDTag(0)]
	public static bool IsPracticeRoom(RoomConfig roomConfig)
	{
		if (WrappersManagerImpl.IsPatched(2085))
		{
			return WrappersManagerImpl.GetPatch(2085).__Gen_Wrap_3(roomConfig);
		}
		if (roomConfig != null)
		{
			if (GetSecondValue(GameSecondType.SectionType, roomConfig.secondType) == 1)
			{
				return SectionUtil.IsNewRankScoreScene();
			}
			return false;
		}
		Log.Info("RoomConfig is null", "GoldCoin");
		return false;
	}

	public static RoomConfig GetRoomConfigBySceneId(uint sceneID)
	{
		if (WrappersManagerImpl.IsPatched(2092))
		{
			return WrappersManagerImpl.GetPatch(2092).__Gen_Wrap_450(sceneID);
		}
		List<RoomConfig> allCachedRoomConfigList = GetAllCachedRoomConfigList();
		for (int i = 0; i < allCachedRoomConfigList.Count; i++)
		{
			if (allCachedRoomConfigList[i].sceneId == sceneID)
			{
				return allCachedRoomConfigList[i];
			}
		}
		return null;
	}

	public static List<RoomConfig> GetAllCachedRoomConfigList()
	{
		if (WrappersManagerImpl.IsPatched(2093))
		{
			return WrappersManagerImpl.GetPatch(2093).__Gen_Wrap_449();
		}
		return ProtoConfigLoader<RoomConfig>.getInstance().getAllCachedConfig();
	}

	[IDTag(0)]
	public static int GetSecondValue(GameSecondType type, string config)
	{
		if (WrappersManagerImpl.IsPatched(2014))
		{
			return WrappersManagerImpl.GetPatch(2014).__Gen_Wrap_436(type, config);
		}
		if (config == null)
		{
			return 0;
		}
		string[] array = config.Split('|');
		if (array.Length <= (int)type)
		{
			return 0;
		}
		return int.Parse(array[(int)type]);
	}

	[IDTag(1)]
	public static int GetSecondValue(int type, string config)
	{
		if (WrappersManagerImpl.IsPatched(2208))
		{
			return WrappersManagerImpl.GetPatch(2208).__Gen_Wrap_169(type, config);
		}
		return GetSecondValue((GameSecondType)type, config);
	}
}
