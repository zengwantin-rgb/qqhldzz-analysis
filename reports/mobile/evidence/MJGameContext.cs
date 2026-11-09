using System;
using Google.Protobuf;
using Happy.Blueprint;
using Happy.Blueprint.Genericeventparam;
using Happy.Blueprint.Localchannel;
using Happy.Blueprint.RiichiStdIvP;
using Happy.Blueprint.System;
using HappyMahjong.Common;
using HappyMahjong.Common.GM;
using HappyMahjong.Game;
using HappyMahjong.Game.RiichiGame;
using HappyMahjong.ShopAndBag;
using HuanLeGameSvr;
using IFix;
using IFix.Core;
using UnityEngine;
using strange.extensions.dispatcher.api;

namespace HappyMahjong.NewGame;

public class MJGameContext : MonoBehaviour
{
	private BehaviorRegistry m_awaitRegistry;

	private AtomRegistry m_atomActionRegistry;

	private CustomEventHandleRegistry m_customEventHandleRegistry;

	private string m_vmKey = "mj";

	private HbpEngineService m_hbpEngine;

	private GameContext m_gameContext;

	private bool m_bInited;

	private static MJGameContext s_mjGameContext;

	private HBPGameModel m_hbpDataModel;

	private IRoundModel m_roundModel;

	private IGlobalModel m_globalModel;

	private ICommonRule m_commonRule;

	private BehaviourContext m_behaviourContext;

	private RiichiGameUIView m_gameUIView;

	private RiichiGameTableView m_gameTableView;

	private WaitConditionManager m_waitConditionManager;

	private VMRunMode m_vmRunMode;

	private static int s_vmLogLevel = 1;

	public IDispatcher IoCDispatcher
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(21574))
			{
				return WrappersManagerImpl.GetPatch(21574).__Gen_Wrap_1582(this);
			}
			return m_gameContext.dispatcher;
		}
	}

	public RiichiGameUIView GameUIView
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(33481))
			{
				return WrappersManagerImpl.GetPatch(33481).__Gen_Wrap_3170(this);
			}
			return m_gameUIView;
		}
	}

	public RiichiGameTableView GameTableView
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(33482))
			{
				return WrappersManagerImpl.GetPatch(33482).__Gen_Wrap_3171(this);
			}
			return m_gameTableView;
		}
	}

	public IRoundModel RoundModel
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(21510))
			{
				return WrappersManagerImpl.GetPatch(21510).__Gen_Wrap_1573(this);
			}
			return m_roundModel;
		}
	}

	public HBPGameModel HBPGameModel
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(22054))
			{
				return WrappersManagerImpl.GetPatch(22054).__Gen_Wrap_1705(this);
			}
			return m_hbpDataModel;
		}
	}

	public ICommonRule CommonRule
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(21578))
			{
				return WrappersManagerImpl.GetPatch(21578).__Gen_Wrap_1583(this);
			}
			return m_commonRule;
		}
	}

	public string VMKey
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(21394))
			{
				return WrappersManagerImpl.GetPatch(21394).__Gen_Wrap_47(this);
			}
			return m_vmKey;
		}
	}

	public bool IsVMInitialized()
	{
		if (WrappersManagerImpl.IsPatched(32711))
		{
			return WrappersManagerImpl.GetPatch(32711).__Gen_Wrap_452(this);
		}
		return m_bInited;
	}

	public void Init(GameContext context)
	{
		if (WrappersManagerImpl.IsPatched(33483))
		{
			WrappersManagerImpl.GetPatch(33483).__Gen_Wrap_1(this, context);
			return;
		}
		if (m_bInited)
		{
			HBPUtil.HBPLog("Init  return!");
			return;
		}
		s_vmLogLevel = HBPUtil.GetVMLogLevelByDeviceType();
		SingletonBase<HBPMJBridge>.GetInstance().Init(this);
		m_awaitRegistry = new BehaviorRegistry();
		m_atomActionRegistry = new AtomRegistry();
		m_customEventHandleRegistry = new CustomEventHandleRegistry();
		m_gameContext = context;
		s_mjGameContext = this;
		m_waitConditionManager = new WaitConditionManager();
		m_behaviourContext = new BehaviourContext();
		m_behaviourContext.vmContext = this;
		m_hbpEngine = AppRoot.GetInstance().GetHbpEngineService();
		m_behaviourContext.dispatcher = m_gameContext.dispatcher;
		m_roundModel = m_gameContext.injectionBinder.GetInstance<IRoundModel>();
		m_globalModel = m_gameContext.injectionBinder.GetInstance<IGlobalModel>();
		m_commonRule = m_gameContext.injectionBinder.GetInstance<ICommonRule>();
		m_gameTableView = GameObject.Find("GameController").GetComponent<RiichiGameTableView>();
		m_gameUIView = SingletonBase<PopUpManager>.GetInstance().GetUIRoot().GetComponent<RiichiGameUIView>();
		m_behaviourContext.roundModel = m_roundModel;
		m_behaviourContext.globalModel = m_globalModel;
		m_behaviourContext.shopModel = m_gameContext.injectionBinder.GetInstance<IShopModel>();
		m_behaviourContext.dispatcher = m_gameContext.dispatcher;
	}

	private void InitIOCFrame()
	{
		if (WrappersManagerImpl.IsPatched(32720))
		{
			WrappersManagerImpl.GetPatch(32720).__Gen_Wrap_0(this);
			return;
		}
		HBPGameModule hBPGameModule = new HBPGameModule();
		m_gameContext.AddAndRegisterGameModule(hBPGameModule);
		BindGameModuleBinders(hBPGameModule.GetContext());
		m_behaviourContext.gameModule = SingletonBase<GameModuleRegistry>.GetInstance().GetGameModuleByName("HBPGameModule") as HBPGameModule;
		m_hbpDataModel = SingletonBase<ModuleRegistry>.GetInstance().GetModel(typeof(HBPGameModel), m_gameContext) as HBPGameModel;
		ResetRoundModel();
	}

	private void BindGameModuleBinders(IStrangeContext m_context)
	{
		if (WrappersManagerImpl.IsPatched(32723))
		{
			WrappersManagerImpl.GetPatch(32723).__Gen_Wrap_1(this, m_context);
			return;
		}
		m_context.CommandBinder(m_gameContext.commandBinder, m_gameContext);
		m_context.InjectionBinder(m_gameContext.injectionBinder, m_gameContext);
		m_context.MediationBinder(m_gameContext.mediationBinder, m_gameContext);
		m_context.CrossContextBridgeBind(m_gameContext.crossContextBridge, m_gameContext);
	}

	private void ResetRoundModel()
	{
		if (WrappersManagerImpl.IsPatched(32724))
		{
			WrappersManagerImpl.GetPatch(32724).__Gen_Wrap_0(this);
			return;
		}
		SingletonBase<GMManager>.GetInstance().Model.ResetInNewGame();
		m_roundModel.Reset();
		PlayerDataVO selfInfo = PlayerDataMgr.GetInstance().GetSelfInfo();
		if (selfInfo != null)
		{
			m_roundModel.selfSeat = selfInfo.seatId;
			HappyMahjong.Common.Log.Info("ResetRoundModel Set Self Seat: " + m_roundModel.selfSeat, "Game");
		}
	}

	public void OnRecvData(uint uin, byte[] data)
	{
		if (WrappersManagerImpl.IsPatched(31861))
		{
			WrappersManagerImpl.GetPatch(31861).__Gen_Wrap_1551(this, uin, data);
		}
		else if (m_hbpEngine != null)
		{
			m_hbpEngine.OnRecvCSMsg(m_vmKey, data, data.Length);
		}
	}

	public void InitVM(string jsonPath, VMBPLoadMode loadMode = VMBPLoadMode.VM_BP_LOAD_MODE_NORMAL, string md5 = "")
	{
		if (WrappersManagerImpl.IsPatched(32719))
		{
			WrappersManagerImpl.GetPatch(32719).__Gen_Wrap_3081(this, jsonPath, loadMode, md5);
			return;
		}
		HappyMahjong.Common.Log.Info("InitVM jsonPath is: " + jsonPath, "Game");
		if (m_bInited)
		{
			HBPUtil.HBPLog("InitVM  m_bInited return!");
			return;
		}
		SingletonBase<HBPMJBridgeInfo>.GetInstance().InHBPGameSceneID = true;
		InitIOCFrame();
		m_bInited = true;
		byte[] array = FileHelper.ReadAllBytes(jsonPath);
		string text = md5;
		if (string.IsNullOrEmpty(text))
		{
			text = Util.GetMD5HashByByte(array);
		}
		HBPUtil.HBPLog("blueprint json file length = " + array.Length);
		TraceBinLevel traceBinLevelByDeviceType = (TraceBinLevel)HBPUtil.GetTraceBinLevelByDeviceType();
		try
		{
			Happy.Blueprint.RiichiStdIvP.RegisterCustomNodes.AutoGenRegisterAllCustomNodes();
			HBPUtil.HBPLog($"CreateVirtualMachine before mode: {loadMode}, traceBinLvel: {traceBinLevelByDeviceType} ");
			m_hbpEngine.CreateVirtualMachine(m_vmKey, BPFmtType.BP_FMT_TYPE_BINARY, array, jsonPath, text, m_vmRunMode, loadMode, SendDataToSvr, OnRecvEventBufferAPI, traceBinLevelByDeviceType);
			HBPUtil.HBPLog("CreateVirtualMachine after");
		}
		catch (Exception ex)
		{
			HappyMahjong.Common.Log.Error(ex.Message, "NewGame");
		}
	}

	public int OnRecvEventBuffer(EventMsgType eventMsgType, byte[] messageBuffer, byte[] recvGenericInfoBuffer)
	{
		if (WrappersManagerImpl.IsPatched(32727))
		{
			return WrappersManagerImpl.GetPatch(32727).__Gen_Wrap_3079(this, eventMsgType, messageBuffer, recvGenericInfoBuffer);
		}
		HBPUtil.HBPLog("OnRecvEventBuffer start");
		HostSideRecvGenericInfo hostSideRecvGenericInfo = null;
		AwaitData awaitData = new AwaitData();
		if (eventMsgType == EventMsgType.EVENT_MSG_TYPE_BINARY)
		{
			hostSideRecvGenericInfo = HostSideRecvGenericInfo.Parser.ParseFrom(recvGenericInfoBuffer);
			awaitData.buffer = messageBuffer;
		}
		Happy.Blueprint.RiichiStdIvP.HBPEventId eventId = (Happy.Blueprint.RiichiStdIvP.HBPEventId)hostSideRecvGenericInfo.EventId;
		awaitData.sessionId = hostSideRecvGenericInfo.SessionId;
		awaitData.completeDelgate = SendClientBhvEndEvent;
		awaitData.terminalDelgate = SendClientBhvTerminalEvent;
		awaitData.vmKey = m_vmKey;
		IClientBehaviour awaitNodeByType = m_awaitRegistry.GetAwaitNodeByType((uint)eventId);
		if (awaitNodeByType != null)
		{
			awaitNodeByType.Context = m_behaviourContext;
			awaitNodeByType?.Begin(awaitData);
			m_hbpDataModel.StartBehaviour(awaitNodeByType);
		}
		else if (!m_customEventHandleRegistry.ExecuteEvent((uint)eventId, awaitData.bufferString))
		{
			HandleSystemEvent((SystemEventID)eventId, awaitData);
		}
		HBPUtil.HBPLog($"OnRecvEventBuffer id is {eventId} sessionid is {awaitData.sessionId} messageBuffer is {messageBuffer}");
		return 0;
	}

	private void HandleSystemEvent(SystemEventID eventId, AwaitData data)
	{
		if (WrappersManagerImpl.IsPatched(32737))
		{
			WrappersManagerImpl.GetPatch(32737).__Gen_Wrap_3078(this, eventId, data);
			return;
		}
		switch (eventId)
		{
		case SystemEventID.EventIdAwaitAbort:
		{
			Evt_AwaitAbort evt_AwaitAbort = data.ToEventMessage<Evt_AwaitAbort>();
			if (evt_AwaitAbort.AbortReason == Enum_AwaitAbortReason.AwaitAbortReasonTimeout)
			{
				AwaitNodeTimeout(evt_AwaitAbort.SessionId);
			}
			else if (evt_AwaitAbort.AbortReason == Enum_AwaitAbortReason.AwaitAbortReasonCancel)
			{
				TerminalAwaitNode(evt_AwaitAbort.SessionId);
			}
			break;
		}
		}
		HBPUtil.HBPLog($"HandleSystemEvent id is {eventId} sessionid is {data.sessionId} messageBuffer is {data.bufferString}");
	}

	public static int OnRecvEventBufferAPI(EventMsgType eventMsgType, byte[] message, byte[] genericInf)
	{
		if (WrappersManagerImpl.IsPatched(32726))
		{
			return WrappersManagerImpl.GetPatch(32726).__Gen_Wrap_3080(eventMsgType, message, genericInf);
		}
		int result = 0;
		try
		{
			if (s_mjGameContext != null)
			{
				result = s_mjGameContext.OnRecvEventBuffer(eventMsgType, message, genericInf);
			}
		}
		catch (Exception ex)
		{
			HBPUtil.HBPLog("OnRecvEventBufferAPI " + ex.Message);
		}
		return result;
	}

	private static void LogFunction(string tag, Happy.Blueprint.LogLevel level, string log)
	{
		if (WrappersManagerImpl.IsPatched(33484))
		{
			WrappersManagerImpl.GetPatch(33484).__Gen_Wrap_3172(tag, level, log);
		}
		else if ((int)((uint)s_vmLogLevel & (uint)level) > 0)
		{
			if (level == Happy.Blueprint.LogLevel.LOG_LEVEL_ERROR)
			{
				HappyMahjong.Common.Log.Error(log, "HBPVM");
			}
			else
			{
				HappyMahjong.Common.Log.Info(log, "HBPVM");
			}
		}
	}

	private static void SendDataToSvr(byte[] buffer, int len)
	{
		if (WrappersManagerImpl.IsPatched(32725))
		{
			WrappersManagerImpl.GetPatch(32725).__Gen_Wrap_466(buffer, len);
			return;
		}
		CBPEngineGameDataHGPara obj = new CBPEngineGameDataHGPara
		{
			nGameDataLen = (short)len,
			szGameData = Util.ByteArrayToSbyteArray(buffer)
		};
		int usedSize = 0;
		obj.pack(ref TSDK4CSharpService.msgBuffer, 102400, ref usedSize, 0u);
		GameUtil.SendMsg2GameSvr(15884, TSDK4CSharpService.msgBuffer, usedSize, 0u);
	}

	public void DestroyVM()
	{
		if (WrappersManagerImpl.IsPatched(32713))
		{
			WrappersManagerImpl.GetPatch(32713).__Gen_Wrap_0(this);
			return;
		}
		Clear();
		if (m_hbpEngine != null && m_bInited)
		{
			m_hbpEngine.DeleteVirtualMachine(m_vmKey);
		}
		m_gameContext.ClearAndUnBindModule("HBPGameModule");
		m_bInited = false;
		HBPUtil.HBPLog("DestroyVM");
	}

	public void Clear()
	{
		if (WrappersManagerImpl.IsPatched(21732))
		{
			WrappersManagerImpl.GetPatch(21732).__Gen_Wrap_0(this);
			return;
		}
		HappyMahjong.Common.Log.Info("GameContext Clear", "NewGame");
		if (m_hbpDataModel != null)
		{
			m_hbpDataModel.Clear();
		}
		m_waitConditionManager.Reset();
	}

	public void OnlineStatusChange(bool isOffline)
	{
		if (WrappersManagerImpl.IsPatched(31855))
		{
			WrappersManagerImpl.GetPatch(31855).__Gen_Wrap_453(this, isOffline);
		}
		else if (m_hbpEngine != null && m_bInited)
		{
			uint playerUin = PlayerDataMgr.GetInstance().GetSelfInfo().playerUin;
			HostSideEmitGenericInfo hostSideEmitGenericInfo = new HostSideEmitGenericInfo();
			hostSideEmitGenericInfo.Uin = playerUin;
			hostSideEmitGenericInfo.OnilneStatusChangeParam = new EventOnlineStatusChange();
			hostSideEmitGenericInfo.OnilneStatusChangeParam.Uin = playerUin;
			hostSideEmitGenericInfo.OnilneStatusChangeParam.OnlineStatus = ((!isOffline) ? OnlineStatus.Online : OnlineStatus.Offline);
			m_hbpEngine.SendEvent(m_vmKey, 5u, null, hostSideEmitGenericInfo);
		}
	}

	private void OnDestroy()
	{
		if (WrappersManagerImpl.IsPatched(33485))
		{
			WrappersManagerImpl.GetPatch(33485).__Gen_Wrap_0(this);
			return;
		}
		s_mjGameContext = null;
		DestroyVM();
	}

	private int SendClientBhvEndEvent(IClientBehaviour node, uint eventId, IMessage message, HostSideEmitGenericInfo eventGenericInfo)
	{
		if (WrappersManagerImpl.IsPatched(32728))
		{
			return WrappersManagerImpl.GetPatch(32728).__Gen_Wrap_3074(this, node, eventId, message, eventGenericInfo);
		}
		m_hbpDataModel.EndBehaviour(node);
		HBPUtil.HBPLog($"SendClientBhvEndEvent eventId {eventId} sessionid {eventGenericInfo.SessionId} message {message}");
		return m_hbpEngine.SendEvent(m_vmKey, eventId, message, eventGenericInfo);
	}

	private int SendClientBhvTerminalEvent(IClientBehaviour node, uint eventId, IMessage message, HostSideEmitGenericInfo eventGenericInfo)
	{
		if (WrappersManagerImpl.IsPatched(32729))
		{
			return WrappersManagerImpl.GetPatch(32729).__Gen_Wrap_3074(this, node, eventId, message, eventGenericInfo);
		}
		HBPUtil.HBPLog($"SendClientBhvTerminalEvent eventId {eventId} sessionid {eventGenericInfo.SessionId} message {message}");
		return m_hbpEngine.SendEvent(m_vmKey, eventId, message, eventGenericInfo);
	}

	public OutputData ExcuteAtomNode(CommonDef.AtomTypeDef atomType, FlowData vo)
	{
		if (WrappersManagerImpl.IsPatched(21855))
		{
			return WrappersManagerImpl.GetPatch(21855).__Gen_Wrap_1646(this, atomType, vo);
		}
		return m_atomActionRegistry.ExcuteAtomNode(atomType, vo, m_behaviourContext);
	}

	public OutputData AwaitNodeTimeout(uint sessionId)
	{
		if (WrappersManagerImpl.IsPatched(21447))
		{
			return WrappersManagerImpl.GetPatch(21447).__Gen_Wrap_1561(this, sessionId);
		}
		HBPUtil.HBPLog($"AwaitNodeTimeout id is {sessionId}");
		IClientBehaviour behaviourBySessionId = m_hbpDataModel.GetBehaviourBySessionId(sessionId);
		OutputData result = null;
		if (behaviourBySessionId != null)
		{
			result = m_hbpDataModel.GetBehaviourBySessionId(sessionId).OnTimeOut();
			m_hbpDataModel.EndBehaviour(behaviourBySessionId);
		}
		return result;
	}

	public void TerminalAwaitNode(uint sessionId)
	{
		if (WrappersManagerImpl.IsPatched(21385))
		{
			WrappersManagerImpl.GetPatch(21385).__Gen_Wrap_654(this, sessionId);
			return;
		}
		HBPUtil.HBPLog($"TermialAwaitNode id is {sessionId}");
		m_hbpDataModel.TerminalBehaviour(sessionId);
	}

	public WaitConditionManager GetWaitConditionManager()
	{
		if (WrappersManagerImpl.IsPatched(21994))
		{
			return WrappersManagerImpl.GetPatch(21994).__Gen_Wrap_1677(this);
		}
		return m_waitConditionManager;
	}

	public void SetSelfSeat(int seat)
	{
		if (WrappersManagerImpl.IsPatched(33486))
		{
			WrappersManagerImpl.GetPatch(33486).__Gen_Wrap_466(this, seat);
			return;
		}
		HBPUtil.HBPLog($"SetSelfSeat {seat}");
		m_roundModel.selfSeat = seat;
	}

	public PlayerAccessor GetAuthorizedPlayer()
	{
		if (WrappersManagerImpl.IsPatched(21663))
		{
			return WrappersManagerImpl.GetPatch(21663).__Gen_Wrap_1601(this);
		}
		uint playerUin = PlayerDataMgr.GetInstance().GetSelfInfo().playerUin;
		uint playerInstanceIdByUin = m_hbpEngine.GetPlayerInstanceIdByUin(m_vmKey, playerUin);
		return HbpSingleton<HbpManager>.Instance.GetEntity<PlayerAccessor>(m_vmKey, playerInstanceIdByUin);
	}

	public GameScene GetGameScene()
	{
		if (WrappersManagerImpl.IsPatched(33487))
		{
			return WrappersManagerImpl.GetPatch(33487).__Gen_Wrap_3173(this);
		}
		return SingletonBase<HBPMJBridge>.GetInstance().GetVMAdapter().GetGameScene();
	}

	[IDTag(0)]
	public void SendEventToVM(Happy.Blueprint.RiichiStdIvP.HBPEventId eventId, IMessage message, IMessage eventGenericInfo)
	{
		if (WrappersManagerImpl.IsPatched(31858))
		{
			WrappersManagerImpl.GetPatch(31858).__Gen_Wrap_2958(this, eventId, message, eventGenericInfo);
		}
		else
		{
			m_hbpEngine.SendEvent(m_vmKey, (uint)eventId, message, eventGenericInfo);
		}
	}

	[IDTag(1)]
	public void SendEventToVM(SystemEventID eventId, IMessage message, IMessage eventGenericInfo)
	{
		if (WrappersManagerImpl.IsPatched(33489))
		{
			WrappersManagerImpl.GetPatch(33489).__Gen_Wrap_3174(this, eventId, message, eventGenericInfo);
		}
		else
		{
			m_hbpEngine.SendEvent(m_vmKey, (uint)eventId, message, eventGenericInfo);
		}
	}

	public void SendEventToGameScene(Happy.Blueprint.RiichiStdIvP.HBPEventId eventId, IMessage message)
	{
		if (WrappersManagerImpl.IsPatched(33490))
		{
			WrappersManagerImpl.GetPatch(33490).__Gen_Wrap_3175(this, eventId, message);
			return;
		}
		HostSideEmitGenericInfo hostSideEmitGenericInfo = new HostSideEmitGenericInfo();
		GameScene gameScene = SingletonBase<HBPMJBridge>.GetInstance().GetVMAdapter().GetGameScene();
		if (gameScene != null)
		{
			hostSideEmitGenericInfo.EntityId = gameScene.InstanceID;
			SendEventToVM(eventId, message, hostSideEmitGenericInfo);
		}
	}

	public void GMSetGameEnd()
	{
		if (WrappersManagerImpl.IsPatched(31857))
		{
			WrappersManagerImpl.GetPatch(31857).__Gen_Wrap_0(this);
			return;
		}
		HostSideEmitGenericInfo eventGenericInfo = new HostSideEmitGenericInfo();
		SendEventToVM(Happy.Blueprint.RiichiStdIvP.HBPEventId.RiichiStdIvPEvtGmSetGameEnd, null, eventGenericInfo);
	}
}
