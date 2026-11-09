using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Configuration;
using Google.Protobuf;
using HLMJPlayback;
using HappyMahjong.Common;
using HappyMahjong.Game;
using HuanLeGameSo;
using IFix;
using MJSoPb;
using ProtoBuf;
using TSDK4CSharp;

namespace HappyMahjong.CSReplay;

public class CSReplayRecordData
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct HoldAction
	{
	}

	public struct NodeData
	{
		public bool isWin;

		public float process;

		public int index;

		public NodeData(bool _isWin, float _process, int _index)
		{
			isWin = _isWin;
			process = _process;
			index = _index;
		}
	}

	public struct RoundVO
	{
		public int iSequenceID;

		public int iType;

		public RoundVO(int _iSequenceID, int _iType)
		{
			iSequenceID = _iSequenceID;
			iType = _iType;
		}

		public static bool operator ==(RoundVO lhs, RoundVO rhs)
		{
			if (WrappersManagerImpl.IsPatched(34691))
			{
				return WrappersManagerImpl.GetPatch(34691).__Gen_Wrap_3283(lhs, rhs);
			}
			bool result = false;
			if (lhs.iSequenceID == rhs.iSequenceID && lhs.iType == rhs.iType)
			{
				result = true;
			}
			return result;
		}

		public static bool operator !=(RoundVO lhs, RoundVO rhs)
		{
			if (WrappersManagerImpl.IsPatched(34692))
			{
				return WrappersManagerImpl.GetPatch(34692).__Gen_Wrap_3283(lhs, rhs);
			}
			bool result = false;
			if (lhs.iSequenceID != rhs.iSequenceID || lhs.iType != rhs.iType)
			{
				result = true;
			}
			return result;
		}
	}

	public enum Version
	{
		NONE,
		V3
	}

	private const int MAX_BUF_LEN = 524288;

	private CSReplayRecordLocalServer m_server;

	private RoundRecordV3 m_recordV3;

	private TRoundEnv m_roundEnv;

	private int m_recordVersion;

	private uint m_uin;

	private int m_roomSectionType;

	private string m_gameTag = string.Empty;

	private List<HuanLeGameSo.SoNotifyMsgTV> m_localServerMsgList;

	private bool m_isLocalTestData;

	private bool m_isNewSection;

	public CSReplayRecordData(GetPlaybackRecordRes data, bool isLocalTestData = false)
	{
		m_isLocalTestData = isLocalTestData;
		m_recordVersion = data.RecordVersion;
		m_uin = data.UserUin;
		m_roomSectionType = data.RoomSectionType;
		Log.Info("CSReplayRecordData m_roomSectionType is: " + m_roomSectionType, "CSReplay");
		if (m_roomSectionType == 7)
		{
			m_roomSectionType = 0;
		}
		else if (m_roomSectionType == 11)
		{
			m_roomSectionType = 2;
		}
		else if (m_roomSectionType == 9)
		{
			m_roomSectionType = 0;
		}
		else if (m_roomSectionType == 12)
		{
			m_roomSectionType = 12;
		}
		m_gameTag = data.GameTag;
		int outBufferSize = GetOutBufferSize(data.OriginSize);
		byte[] array = TSDK4CSharp.TSDKService.Uncompress(1, data.PlaybackRecord, data.PlaybackRecord.Length, outBufferSize);
		using (MemoryStream source = new MemoryStream(array, 0, array.Length))
		{
			if (GetVersionType() == Version.V3)
			{
				m_roundEnv = (m_recordV3 = Serializer.Deserialize<RoundRecordV3>(source)).round_env;
			}
		}
		int selfSeatID = GetSelfSeatID();
		if (selfSeatID != -1)
		{
			m_isNewSection = (m_roundEnv.PlayerEnv[selfSeatID].MJCapacity & 0x100000) != 0;
		}
	}

	private int GetOutBufferSize(int origin_size)
	{
		if (WrappersManagerImpl.IsPatched(34655))
		{
			return WrappersManagerImpl.GetPatch(34655).__Gen_Wrap_1656(this, origin_size);
		}
		if (origin_size <= 0)
		{
			return 524288;
		}
		return origin_size + 1024;
	}

	public object GetRecord()
	{
		if (WrappersManagerImpl.IsPatched(34656))
		{
			return WrappersManagerImpl.GetPatch(34656).__Gen_Wrap_2955(this);
		}
		if (GetVersionType() == Version.V3)
		{
			return m_recordV3;
		}
		return null;
	}

	public Version GetVersionType()
	{
		if (WrappersManagerImpl.IsPatched(31117))
		{
			return WrappersManagerImpl.GetPatch(31117).__Gen_Wrap_2859(this);
		}
		if (m_recordVersion == 3)
		{
			return Version.V3;
		}
		return Version.NONE;
	}

	public bool IsNewSection()
	{
		if (WrappersManagerImpl.IsPatched(31085))
		{
			return WrappersManagerImpl.GetPatch(31085).__Gen_Wrap_452(this);
		}
		return m_isNewSection;
	}

	public int GetVersion()
	{
		if (WrappersManagerImpl.IsPatched(31159))
		{
			return WrappersManagerImpl.GetPatch(31159).__Gen_Wrap_2(this);
		}
		return m_recordVersion;
	}

	public TRoundEnv GetRoundEnv()
	{
		if (WrappersManagerImpl.IsPatched(30968))
		{
			return WrappersManagerImpl.GetPatch(30968).__Gen_Wrap_1481(this);
		}
		return m_roundEnv;
	}

	public RoomSectionType GetRoomSectionType()
	{
		if (WrappersManagerImpl.IsPatched(31084))
		{
			return WrappersManagerImpl.GetPatch(31084).__Gen_Wrap_2847(this);
		}
		return (RoomSectionType)m_roomSectionType;
	}

	public int GetRecordVersion()
	{
		if (WrappersManagerImpl.IsPatched(34657))
		{
			return WrappersManagerImpl.GetPatch(34657).__Gen_Wrap_2(this);
		}
		return m_recordVersion;
	}

	public string GetGameTag()
	{
		if (WrappersManagerImpl.IsPatched(31082))
		{
			return WrappersManagerImpl.GetPatch(31082).__Gen_Wrap_47(this);
		}
		return m_gameTag;
	}

	public bool IsFriendAssist()
	{
		if (WrappersManagerImpl.IsPatched(31086))
		{
			return WrappersManagerImpl.GetPatch(31086).__Gen_Wrap_452(this);
		}
		if (m_recordV3?.so_notify_msg == null)
		{
			return false;
		}
		Pb2TdrConverter pb2TdrConverter = new Pb2TdrConverter();
		foreach (byte[] item in m_recordV3.so_notify_msg)
		{
			HuanLeGameSo.SoNotifyMsgTV soNotifyMsgTV = pb2TdrConverter.Convert2Tdr(Bytes2Pb(item));
			if (soNotifyMsgTV != null)
			{
				TSOTLVUNIT childrenTlv = CSReplayRecordHelper.GetChildrenTlv(soNotifyMsgTV, HuanLeGameSo.TLV_TYPE.EM_UP_TLV_CONFIG_INFO);
				if (childrenTlv?.stValue?.stFixConfigInfo != null)
				{
					return childrenTlv.stValue.stFixConfigInfo.iIsInFriendAssist == 1;
				}
			}
		}
		return false;
	}

	public GameType GetGameType()
	{
		if (WrappersManagerImpl.IsPatched(30970))
		{
			return WrappersManagerImpl.GetPatch(30970).__Gen_Wrap_2789(this);
		}
		return ((HuanLeGameSo.SO_ROOM_TYPE)m_roundEnv.GameType).ToClientGameType();
	}

	public HuanLeGameSo.SO_ROOM_TYPE GetServerGameType()
	{
		if (WrappersManagerImpl.IsPatched(31162))
		{
			return WrappersManagerImpl.GetPatch(31162).__Gen_Wrap_2868(this);
		}
		return (HuanLeGameSo.SO_ROOM_TYPE)m_roundEnv.GameType;
	}

	public RoomConfig GetRoomConfig()
	{
		if (WrappersManagerImpl.IsPatched(31081))
		{
			return WrappersManagerImpl.GetPatch(31081).__Gen_Wrap_2846(this);
		}
		return Util.GetRoomConfigBySceneId((uint)m_roundEnv.SceneID);
	}

	public int GetSelfSeatID()
	{
		if (WrappersManagerImpl.IsPatched(30967))
		{
			return WrappersManagerImpl.GetPatch(30967).__Gen_Wrap_2(this);
		}
		TRoundEnv roundEnv = GetRoundEnv();
		for (int i = 0; i < roundEnv.PlayerEnv.Count; i++)
		{
			if (roundEnv.PlayerEnv[i].Uin == m_uin)
			{
				return i;
			}
		}
		Log.Error("GetSelfSeatID uin error! uin: " + m_uin, "CSReplay");
		return -1;
	}

	public object GetTableInfoByIndex(int index)
	{
		if (WrappersManagerImpl.IsPatched(34658))
		{
			return WrappersManagerImpl.GetPatch(34658).__Gen_Wrap_3281(this, index);
		}
		if (GetVersionType() == Version.V3)
		{
			if (m_server == null)
			{
				m_server = new CSReplayRecordLocalServer(GetRoundEnv(), m_localServerMsgList, GetSelfSeatID());
			}
			return m_server.GetTableInfoByIndexV3(index);
		}
		return null;
	}

	public List<object> GetPreDataTableInfo(int replayIndex)
	{
		if (WrappersManagerImpl.IsPatched(34689))
		{
			return WrappersManagerImpl.GetPatch(34689).__Gen_Wrap_3282(this, replayIndex);
		}
		List<object> preDataTableInfoByRecordDataTest = SingletonBase<CSReplayRecordService>.GetInstance().GetPreDataTableInfoByRecordDataTest(GetRoundEnv(), m_uin);
		if (GetVersionType() == Version.V3)
		{
			m_server.SetBeanAndScore(preDataTableInfoByRecordDataTest, replayIndex);
		}
		return preDataTableInfoByRecordDataTest;
	}

	private MJSoPb.SoNotifyMsgTV Bytes2Pb(byte[] bytes)
	{
		if (WrappersManagerImpl.IsPatched(30963))
		{
			return WrappersManagerImpl.GetPatch(30963).__Gen_Wrap_2786(this, bytes);
		}
		try
		{
			return new MessageParser<MJSoPb.SoNotifyMsgTV>(() => new MJSoPb.SoNotifyMsgTV()).ParseFrom(bytes, 0, bytes.Length);
		}
		catch (Exception ex)
		{
			Log.Error("[CSReplayRecordData] Bytes2Pb ParseFrom error: " + ex.Message, "Replay");
			return new MJSoPb.SoNotifyMsgTV();
		}
	}

	public HuanLeGameSo.SoNotifyMsgTV GetBalanceTlvV3()
	{
		if (WrappersManagerImpl.IsPatched(30962))
		{
			return WrappersManagerImpl.GetPatch(30962).__Gen_Wrap_2787(this);
		}
		List<MJSoPb.SO_MJ_MSG_ID> list = new List<MJSoPb.SO_MJ_MSG_ID>();
		Pb2TdrConverter pb2TdrConverter = new Pb2TdrConverter();
		for (int num = m_recordV3.so_notify_msg.Count - 1; num >= 0; num--)
		{
			MJSoPb.SoNotifyMsgTV soNotifyMsgTV = Bytes2Pb(m_recordV3.so_notify_msg[num]);
			list.Add((MJSoPb.SO_MJ_MSG_ID)soNotifyMsgTV.type);
			if ((soNotifyMsgTV.type == 54 || soNotifyMsgTV.type == 51) && soNotifyMsgTV.Seat.Contains(GetSelfSeatID()))
			{
				return pb2TdrConverter.Convert2Tdr(soNotifyMsgTV);
			}
		}
		return null;
	}

	private IRoundModel CreateRoundModel(GameType gameType, IGlobalModel globalModel)
	{
		if (WrappersManagerImpl.IsPatched(30972))
		{
			return WrappersManagerImpl.GetPatch(30972).__Gen_Wrap_2792(this, gameType, globalModel);
		}
		Log.Info("CreateRoundModel enter", "IFix");
		IRoundModel roundModel = null;
		switch (gameType)
		{
		case GameType.ModuleHongZhongXueLiu:
		case GameType.ModuleXueZhan:
		case GameType.ModuleXueLiu:
			roundModel = new SCMJRoundModel();
			break;
		case GameType.ModuleErRen:
			roundModel = new ErRenRoundModel();
			break;
		case GameType.ModuleGuangDong:
			roundModel = new GuangDongRoundModel();
			break;
		default:
			roundModel = new GuoBiaoRoundModel();
			break;
		}
		roundModel.Reset();
		roundModel.gameType = gameType;
		roundModel.selfSeat = GetSelfSeatID();
		if (gameType == GameType.ModuleErRen)
		{
			roundModel.SetPlayerCount(2);
		}
		if (m_roomSectionType == 0)
		{
			roundModel.gameSecondType = 0;
		}
		else if (m_roomSectionType == 12)
		{
			roundModel.gameSecondType = 7;
		}
		else if (m_roomSectionType == 1)
		{
			roundModel.gameSecondType = 1;
		}
		else if (m_roomSectionType == 2)
		{
			roundModel.gameSecondType = 2;
		}
		roundModel.BalanceConfig = new BalanceConfig();
		BalanceConfig balanceConfig = new BalanceConfig();
		balanceConfig.SetRoundModule(roundModel, m_roundEnv.SceneID);
		balanceConfig.LoadConfig();
		roundModel.BalanceConfig = balanceConfig;
		if (globalModel.enterGameInfo == null)
		{
			globalModel.enterGameInfo = new EnterGameVO();
			globalModel.enterGameInfo.enterGameSecretRoomVO = new EnterGameSecretRoomVO();
		}
		globalModel.enterGameInfo.gameType = gameType;
		RoomConfig roomConfigBySceneId = Util.GetRoomConfigBySceneId((uint)m_roundEnv.SceneID);
		globalModel.enterGameInfo.roomInfo = roomConfigBySceneId;
		return roundModel;
	}

	private BalanceDataParser CreateBalanceDataParser(HuanLeGameSo.SoNotifyMsgTV msg, IGlobalModel globalModel, IRoundModel roundModel)
	{
		if (WrappersManagerImpl.IsPatched(30980))
		{
			return WrappersManagerImpl.GetPatch(30980).__Gen_Wrap_2843(this, msg, globalModel, roundModel);
		}
		BalanceDataParser balanceDataParser = new BalanceDataParser(roundModel, globalModel, SingletonBase<ShopDataHelper>.GetInstance().shopModel);
		balanceDataParser.RoomSectionType = SectionUtil.GetCurrentRoomSectionType();
		List<ActionTotalChangeDonner> list = null;
		if (msg.iType == 54)
		{
			HuanLeGameSo.CNotifyFinishV3 stNotifyFinishV = msg.stValue.stNotifyFinishV3;
			balanceDataParser.SetData(stNotifyFinishV);
			list = balanceDataParser.GetTotalChangeListV3(stNotifyFinishV);
		}
		else
		{
			HuanLeGameSo.CNotifyFinishV2 stNotifyFinishV2 = msg.stValue.stNotifyFinishV2;
			balanceDataParser.SetData(stNotifyFinishV2);
			list = balanceDataParser.GetTotalChangeListV2(stNotifyFinishV2);
		}
		TSOTLVUNIT childrenTlv = CSReplayRecordHelper.GetChildrenTlv(msg, HuanLeGameSo.TLV_TYPE.EM_DN_TLV_GRADE_SCORE_INFO);
		if (childrenTlv != null && (balanceDataParser.RoomSectionType == RoomSectionType.RoomSectionScore || balanceDataParser.RoomSectionType == RoomSectionType.RoomSectionScoreAndBean))
		{
			HuanLeGameSo.TGradeScoreInfo stGradeScoreInfo = childrenTlv.stValue.stGradeScoreInfo;
			if (stGradeScoreInfo != null)
			{
				balanceDataParser.MySectionChangeInfo = InGameRankGradeUtil.GetSectionChangeInfo(stGradeScoreInfo, roundModel.selfSeat, roundModel.GetPlayerCount());
				balanceDataParser.MySectionChangeInfo.roomSectionType = SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo);
				if (SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomSectionScore || SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomSectionScoreAndBean)
				{
					balanceDataParser.playerJiFenFengdingInfo = new SectionScoreFengdingInfo[roundModel.GetPlayerCount()];
					for (int i = 0; i < roundModel.GetPlayerCount(); i++)
					{
						balanceDataParser.playerJiFenFengdingInfo[i] = new SectionScoreFengdingInfo();
						balanceDataParser.playerJiFenFengdingInfo[i].fengdingType = (HuanLeGameSo.ENM_DING_TYPE)stGradeScoreInfo.FengDingType[i];
						if (balanceDataParser.playerJiFenFengdingInfo[i].fengdingType == HuanLeGameSo.ENM_DING_TYPE.WH_DING_TYPE_SEASONSCORE)
						{
							balanceDataParser.playerJiFenFengdingInfo[i].scoreFengdingNum = stGradeScoreInfo.FengDingValue[i];
						}
					}
				}
			}
		}
		balanceDataParser.HandleDuanWeiChangeAction(list);
		if (balanceDataParser.MySectionChangeInfo != null && Util.IsTeamRankPlaying((int)roundModel.gameType))
		{
			balanceDataParser.MySectionChangeInfo.FanList = balanceDataParser.FormatSiChuanAccount(balanceDataParser.SelfSeat());
		}
		return balanceDataParser;
	}

	public BalanceDataParser GetBalanceParser(HuanLeGameSo.SoNotifyMsgTV msg, IGlobalModel globalModel, IRoundModel roundModel = null)
	{
		if (WrappersManagerImpl.IsPatched(30969))
		{
			return WrappersManagerImpl.GetPatch(30969).__Gen_Wrap_2843(this, msg, globalModel, roundModel);
		}
		GameType gameType = GetGameType();
		if (roundModel == null)
		{
			roundModel = CreateRoundModel(gameType, globalModel);
		}
		PreloadEnv();
		return CreateBalanceDataParser(msg, globalModel, roundModel);
	}

	private void PreloadEnv()
	{
		if (WrappersManagerImpl.IsPatched(30977))
		{
			WrappersManagerImpl.GetPatch(30977).__Gen_Wrap_0(this);
			return;
		}
		GameType gameType = GetGameType();
		SectionUtil.gameMode = ((m_roomSectionType != 0) ? GameMode.rankScoreMode : GameMode.classicMode);
		CSReplayRecordHelper.Set2v2Mode(gameType);
		CSReplayRecordHelper.SetGameRuleUnit(m_roundEnv.SceneID);
	}

	public List<BeanChangeInfo> GetBeanChangeList()
	{
		if (WrappersManagerImpl.IsPatched(31076))
		{
			return WrappersManagerImpl.GetPatch(31076).__Gen_Wrap_2845(this);
		}
		if (m_server == null)
		{
			int selfSeatID = GetSelfSeatID();
			if (m_localServerMsgList == null)
			{
				m_localServerMsgList = new List<HuanLeGameSo.SoNotifyMsgTV>();
			}
			if (m_localServerMsgList.Count == 0)
			{
				Pb2TdrConverter pb2TdrConverter = new Pb2TdrConverter();
				foreach (byte[] item in m_recordV3.so_notify_msg)
				{
					MJSoPb.SoNotifyMsgTV soNotifyMsgTV = Bytes2Pb(item);
					if (soNotifyMsgTV.Seat.Contains(selfSeatID))
					{
						HuanLeGameSo.SoNotifyMsgTV soNotifyMsgTV2 = pb2TdrConverter.Convert2Tdr(soNotifyMsgTV);
						if (soNotifyMsgTV2 != null)
						{
							m_localServerMsgList.Add(soNotifyMsgTV2);
						}
					}
				}
			}
			m_server = new CSReplayRecordLocalServer(GetRoundEnv(), m_localServerMsgList, selfSeatID);
		}
		return m_server.GetBeanChangeList();
	}

	public bool AppendTdrList(ref Queue<object> queue)
	{
		if (WrappersManagerImpl.IsPatched(31116))
		{
			return WrappersManagerImpl.GetPatch(31116).__Gen_Wrap_2862(this, ref queue);
		}
		if (GetVersionType() == Version.V3)
		{
			return AppendTdrListV3(ref queue);
		}
		return false;
	}

	private bool AppendTdrListV3(ref Queue<object> queue)
	{
		if (WrappersManagerImpl.IsPatched(31118))
		{
			return WrappersManagerImpl.GetPatch(31118).__Gen_Wrap_2862(this, ref queue);
		}
		if (m_recordV3 == null || m_recordV3.so_notify_msg == null || m_recordV3.so_notify_msg.Count == 0)
		{
			Log.Error("CSReplayRecordService AppendTdrListV3 pbPlayerRecord is null", "CSReplay");
			return false;
		}
		m_localServerMsgList = new List<HuanLeGameSo.SoNotifyMsgTV>();
		int selfSeatID = GetSelfSeatID();
		Pb2TdrConverter pb2TdrConverter = new Pb2TdrConverter();
		HuanLeGameSo.SoNotifyMsgTV soNotifyMsgTV = pb2TdrConverter.Convert2Tdr(Bytes2Pb(m_recordV3.so_notify_msg[m_recordV3.so_notify_msg.Count - 1]));
		int num = 0;
		if (soNotifyMsgTV != null)
		{
			num = soNotifyMsgTV.iSequenceID;
		}
		HuanLeGameSo.SoNotifyMsgTV[] array = new HuanLeGameSo.SoNotifyMsgTV[num + 1];
		List<IGameReplayFiller> gameReplayFillerList = CSReplayRecordHelper.GetGameReplayFillerList(GetGameType());
		foreach (IGameReplayFiller item in gameReplayFillerList)
		{
			item.InitFiller(this);
		}
		for (int i = 0; i < m_recordV3.so_notify_msg.Count; i++)
		{
			MJSoPb.SoNotifyMsgTV soNotifyMsgTV2 = Bytes2Pb(m_recordV3.so_notify_msg[i]);
			HuanLeGameSo.SoNotifyMsgTV soNotifyMsgTV3 = pb2TdrConverter.Convert2Tdr(soNotifyMsgTV2);
			bool flag = soNotifyMsgTV2.Seat.Contains(selfSeatID);
			if (soNotifyMsgTV3 != null)
			{
				foreach (IGameReplayFiller item2 in gameReplayFillerList)
				{
					item2.CacheAllPlayerGameMsg(soNotifyMsgTV3, flag);
				}
			}
			if (!flag)
			{
				continue;
			}
			if (soNotifyMsgTV3 == null)
			{
				Log.Info("CSReplayRecordService AppendTdrListV3 data is null!, i = " + i, "CSReplay");
				queue.Enqueue(default(HoldAction));
				continue;
			}
			if (soNotifyMsgTV3.iType == 10 || soNotifyMsgTV3.iType == 66 || soNotifyMsgTV3.iType == 84 || soNotifyMsgTV3.iType == 82 || soNotifyMsgTV3.iType == 92 || soNotifyMsgTV3.iType == 144 || CSReplayRecordHelper.GetChildrenTlv(soNotifyMsgTV3, HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_WAIT_KEY) != null || CSReplayRecordHelper.GetChildrenTlv(soNotifyMsgTV3, HuanLeGameSo.TLV_TYPE.EM_DN_TLV_MULTI_DO_WAIT_INFO) != null || CSReplayRecordHelper.GetChildrenTlv(soNotifyMsgTV3, HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_PARALLEL_GIVE_KEY) != null || CSReplayRecordHelper.GetChildrenTlv(soNotifyMsgTV3, HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_PARALLEL_MULTI_GIVE_KEY) != null || CSReplayRecordHelper.GetChildrenTlv(soNotifyMsgTV3, HuanLeGameSo.TLV_TYPE.EM_DN_TLV_USE_WILD_SHIFT_RESULT) != null)
			{
				array[soNotifyMsgTV3.iSequenceID] = soNotifyMsgTV3;
			}
			m_localServerMsgList.Add(soNotifyMsgTV3);
			queue.Enqueue(soNotifyMsgTV3);
		}
		foreach (IGameReplayFiller item3 in gameReplayFillerList)
		{
			item3.FillMyGameMsg(array);
		}
		return true;
	}
}
