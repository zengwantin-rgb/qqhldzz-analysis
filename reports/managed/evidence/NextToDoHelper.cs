using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Configuration;
using GameModule;
using Google.Protobuf;
using HappyBridge.Desk;
using HappyMahjong.CSReplay;
using HappyMahjong.Common;
using HappyMahjong.Common.GM;
using HappyMahjong.Game.CompareTile;
using HappyMahjong.Game.MultiTile;
using HappyMahjong.Game.NewHongZhong;
using HappyMahjong.Game.XueLiuBaoPai;
using HappyMahjong.Setting;
using HappyMahjong.TSDKMessage;
using HappyMahjong.Tutorial;
using HuanLeGameSo;
using IFix;
using IFix.Core;
using LitJson;
using MJSoPb;
using TSDK4CSharp;
using UnityEngine;
using strange.extensions.dispatcher.eventdispatcher.api;

namespace HappyMahjong.Game;

public class NextToDoHelper
{
	[CompilerGenerated]
	private sealed class _003CShowRoomInfoUpdate_003Ed__46 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string text;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CShowRoomInfoUpdate_003Ed__46(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = Yielders.GetWaitForSeconds(1f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				SingletonBase<Toast>.GetInstance().showUGUI(text, 2f);
				return false;
			}
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

	public IRoundModel m_roundModel;

	public IProcessModel m_processModel;

	public ICommonRule m_commonRule;

	public IEventDispatcher dispatcher;

	private int obOtherPai = -1;

	private bool m_isFakeHu;

	public List<ProcedureVO> m_toAddProcess = new List<ProcedureVO>();

	public NextToDoHelper(IEventDispatcher eventDispatcher, IRoundModel roundModel, IProcessModel processModel = null, ICommonRule mjRule = null)
	{
		dispatcher = eventDispatcher;
		m_roundModel = roundModel;
		m_processModel = processModel;
		m_commonRule = mjRule;
	}

	private int GetProcessPriority(ProcedureVO vo)
	{
		if (WrappersManagerImpl.IsPatched(35913))
		{
			return WrappersManagerImpl.GetPatch(35913).__Gen_Wrap_1557(this, vo);
		}
		int num = 0;
		return vo.type switch
		{
			ProcessType.RedrawProcess => 5, 
			ProcessType.EnterGiveProcess => 0, 
			ProcessType.LaiziProcess => 4, 
			ProcessType.Change3MJProcess => 3, 
			ProcessType.BuyHorseProcess => 1, 
			ProcessType.DaPiaoProcess => 1, 
			ProcessType.ShowSelfIgnoreProcess => 2, 
			_ => 0, 
		};
	}

	public void SortAndAddProcess()
	{
		if (WrappersManagerImpl.IsPatched(35911))
		{
			WrappersManagerImpl.GetPatch(35911).__Gen_Wrap_0(this);
			return;
		}
		m_toAddProcess.Sort(delegate(ProcedureVO vo1, ProcedureVO vo2)
		{
			int processPriority = GetProcessPriority(vo1);
			int processPriority2 = GetProcessPriority(vo2);
			return processPriority - processPriority2;
		});
		for (int num = 0; num < m_toAddProcess.Count; num++)
		{
			m_processModel.PushProcedure(m_toAddProcess[num]);
		}
		m_toAddProcess.Clear();
	}

	public static HuanLeGameSo.USOTLVValue TLVGetValueByType(int tlvCount, TSOTLVUNIT[] tlvInfo, HuanLeGameSo.TLV_TYPE type)
	{
		if (WrappersManagerImpl.IsPatched(36342))
		{
			return WrappersManagerImpl.GetPatch(36342).__Gen_Wrap_3487(tlvCount, tlvInfo, type);
		}
		for (int i = 0; i < tlvCount; i++)
		{
			if ((HuanLeGameSo.TLV_TYPE)tlvInfo[i].nType == type)
			{
				return tlvInfo[i].stValue;
			}
		}
		return null;
	}

	public static bool TryAddCharmSettleEffectProcess(HuanLeGameSo.TNewSectionScoreJieSuan newSectionScoreJieSuan, IRoundModel roundModel, IProcessModel processModel, bool isEarlySettle)
	{
		if (WrappersManagerImpl.IsPatched(35893))
		{
			return WrappersManagerImpl.GetPatch(35893).__Gen_Wrap_3017(newSectionScoreJieSuan, roundModel, processModel, isEarlySettle);
		}
		if (roundModel == null || processModel == null)
		{
			HappyMahjong.Common.Log.Error("TryAddCharmSettleEffectProcess: model is null", "Game");
			return false;
		}
		if (roundModel.hasPushedCharmSettleEffectProcess)
		{
			return false;
		}
		int charmScore;
		string scoreSource;
		bool flag = TryGetCharmScoreInfo(newSectionScoreJieSuan, out charmScore, out scoreSource);
		bool flag2 = roundModel.charmTargetSeatId >= 0;
		if (!flag && !flag2)
		{
			HappyMahjong.Common.Log.Info("TryAddCharmSettleEffectProcess: charm skill was not triggered and no score detail exists", "Game");
			return false;
		}
		if (!flag || charmScore <= 0)
		{
			if (!flag2)
			{
				HappyMahjong.Common.Log.Info($"TryAddCharmSettleEffectProcess: ignore non-positive charm score without triggered skill, score={charmScore}", "Game");
				return false;
			}
			charmScore = 0;
			scoreSource = (flag ? scoreSource : "LocalTriggeredWithoutScoreDetail");
		}
		CharmSettleInfoVO charmSettleInfoVO = new CharmSettleInfoVO();
		charmSettleInfoVO.OwnerSeat = roundModel.selfSeat;
		charmSettleInfoVO.TargetSeat = roundModel.charmTargetSeatId;
		charmSettleInfoVO.BaseScore = 0;
		charmSettleInfoVO.GainScore = charmScore;
		charmSettleInfoVO.ScoreLevel = ((charmScore > 0) ? 1 : 0);
		charmSettleInfoVO.EndReason = 0;
		charmSettleInfoVO.IsEarlySettle = isEarlySettle;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowCharmSettleEffectProcess;
		procedureVO.data = charmSettleInfoVO;
		if (!processModel.PushProcedure(procedureVO))
		{
			HappyMahjong.Common.Log.Error("TryAddCharmSettleEffectProcess: failed to push process", "Game");
			return false;
		}
		roundModel.hasPushedCharmSettleEffectProcess = true;
		HappyMahjong.Common.Log.Info($"TryAddCharmSettleEffectProcess: source={scoreSource}, targetSeat={charmSettleInfoVO.TargetSeat}, gainScore={charmSettleInfoVO.GainScore}, isEarlySettle={isEarlySettle}", "Game");
		return true;
	}

	private static bool TryGetCharmScoreInfo(HuanLeGameSo.TNewSectionScoreJieSuan newSectionScoreJieSuan, out int charmScore, out string scoreSource)
	{
		if (WrappersManagerImpl.IsPatched(35894))
		{
			return WrappersManagerImpl.GetPatch(35894).__Gen_Wrap_3421(newSectionScoreJieSuan, out charmScore, out scoreSource);
		}
		charmScore = 0;
		scoreSource = string.Empty;
		if (newSectionScoreJieSuan == null)
		{
			return false;
		}
		if (TryGetCharmScoreFromItemAddScore(newSectionScoreJieSuan.stPerformanceScore, out charmScore))
		{
			scoreSource = "ItemAddScore";
			return true;
		}
		return false;
	}

	private static bool TryGetCharmScoreFromItemAddScore(HuanLeGameSo.NewSectionPerformanceAddScore performanceScore, out int charmScore)
	{
		if (WrappersManagerImpl.IsPatched(35895))
		{
			return WrappersManagerImpl.GetPatch(35895).__Gen_Wrap_3420(performanceScore, out charmScore);
		}
		charmScore = 0;
		if (performanceScore == null || performanceScore.astItemAddScoreList == null)
		{
			return false;
		}
		int validScoreDetailCount = GetValidScoreDetailCount(performanceScore.nItemAddScoreNum, performanceScore.astItemAddScoreList.Length);
		for (int i = 0; i < validScoreDetailCount; i++)
		{
			HuanLeGameSo.NewSectionItemAddScore newSectionItemAddScore = performanceScore.astItemAddScoreList[i];
			if (newSectionItemAddScore != null && IsCharmScoreDetail(newSectionItemAddScore.iItemAddScoreType))
			{
				charmScore = newSectionItemAddScore.iScore;
				return true;
			}
		}
		return false;
	}

	private static int GetValidScoreDetailCount(int protocolCount, int arrayLength)
	{
		if (WrappersManagerImpl.IsPatched(35896))
		{
			return WrappersManagerImpl.GetPatch(35896).__Gen_Wrap_3419(protocolCount, arrayLength);
		}
		if (arrayLength <= 0)
		{
			return 0;
		}
		if (protocolCount <= 0 || protocolCount > arrayLength)
		{
			return arrayLength;
		}
		return protocolCount;
	}

	private static bool IsCharmScoreDetail(int scoreType)
	{
		if (WrappersManagerImpl.IsPatched(35897))
		{
			return WrappersManagerImpl.GetPatch(35897).__Gen_Wrap_1622(scoreType);
		}
		return scoreType == 310;
	}

	public void HandleNextToDo(int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel, IProcessModel processModel, ICommonRule mjRule = null, int othersPai = -1, HuanLeGameSo.UMJMsgTVValue msgTVValue = null)
	{
		if (WrappersManagerImpl.IsPatched(35805))
		{
			WrappersManagerImpl.GetPatch(35805).__Gen_Wrap_3425(this, tlvCount, tlvInfo, roundModel, processModel, mjRule, othersPai, msgTVValue);
		}
		else
		{
			if (tlvCount <= 0)
			{
				return;
			}
			if (roundModel == null || processModel == null)
			{
				HappyMahjong.Common.Log.Error("HandleNextToDo wrong parameters", "Game");
				return;
			}
			m_toAddProcess.Clear();
			m_roundModel = roundModel;
			m_processModel = processModel;
			obOtherPai = othersPai;
			m_commonRule = mjRule;
			m_isFakeHu = false;
			bool flag = false;
			for (int i = 0; i < tlvCount; i++)
			{
				TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
				switch ((HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType)
				{
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_FAKE_HU_INFO:
					m_isFakeHu = CheckSelfFakeHuTLV(tSOTLVUNIT.stValue.stFakeHuInfoList);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_USER_JI_INFO:
					HandleUserJiInfo(tSOTLVUNIT.stValue.stGuiZhouJiInfo, isReplay: false);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_SYSTEM_CHANGE_QUE_CARD:
					AddTSystemChangeCardInfoProcess(tSOTLVUNIT.stValue.stSystemChangeCardInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_AVATAR_SKILL_INFO:
					HandleAvatarSkillPre(tSOTLVUNIT.stValue.stAvatarSkillInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_TILES_SHOW_INFO_IN_CARD_WALL:
					ShowInfoInCardWall(tSOTLVUNIT.stValue.stTilesShowInfoInCardWall, tlvCount, tlvInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_VARIABLE_TILE_RESULT_INFO:
					flag = true;
					break;
				}
			}
			List<HuanLeGameSo.THuTipsInfo> list = new List<HuanLeGameSo.THuTipsInfo>();
			for (int j = 0; j < tlvCount; j++)
			{
				TSOTLVUNIT tSOTLVUNIT = tlvInfo[j];
				HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
				HappyMahjong.Common.Log.Info("Handle TLV, Type:" + nType, "Game");
				if (nType == HuanLeGameSo.TLV_TYPE.EM_UP_TLV_HU_TIPS)
				{
					HuanLeGameSo.THuTipsInfo stHuTipsInfo = tSOTLVUNIT.stValue.stHuTipsInfo;
					list.Add(stHuTipsInfo);
				}
			}
			if (list.Count > 0)
			{
				HandleHuTipsInfo(list);
			}
			for (int j = 0; j < tlvCount; j++)
			{
				TSOTLVUNIT tSOTLVUNIT = tlvInfo[j];
				HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
				HappyMahjong.Common.Log.Info("Handle TLV, Type:" + nType, "Game");
				switch (nType)
				{
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_GIVE_KEY:
					HandleSelfRedraw(tSOTLVUNIT.stValue.stDoGiveInfo, tlvCount, tlvInfo);
					HandleDiscardTips(tSOTLVUNIT.stValue.stDoGiveInfo);
					AddEnterGiveProcess(m_roundModel.selfSeat, tlvCount, tlvInfo, 0, needSort: true);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_WAIT_KEY:
					HandleWait(tSOTLVUNIT.stValue.stDoWaitInfo, tlvCount, tlvInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_BLOCK_KEY:
					if (!flag)
					{
						AddEnterBlockProcess(tSOTLVUNIT.stValue.stDoBlockInfo);
					}
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DINGQUE:
					AddEnterIgnoreProcess(tSOTLVUNIT.stValue.stDingque);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_RECHARGE:
					AddChargeProcess(tSOTLVUNIT.stValue.stRecharge);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_LAIZI:
					AddLaiziProcess(tSOTLVUNIT.stValue.stLaiZi);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_CHANGECARD:
					AddChange3MJsProcess(tSOTLVUNIT.stValue.stChangeCard);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_CONFIG_INFO:
					AddFixConfigInfo(tSOTLVUNIT.stValue.stFixConfigInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_WINTIPS_INFO:
					AddWinTipsInfo();
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_ZN:
					AddZhaNiaoProcess(tSOTLVUNIT.stValue.stDoZN);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_LAIZIPI_INFO:
					AddPiziProcess(tSOTLVUNIT.stValue.stPiZi);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MA_INFO:
					AddBuyHorseProcess(tSOTLVUNIT.stValue.stPlayerMaInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MA_RESULT:
					AddBuyHorseResultProcess(tSOTLVUNIT.stValue.stPlayerMaResult);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MJPRIAVATE_TABLE_INFO:
					SetprivateTableInfo(tSOTLVUNIT.stValue.stNtfMJPrivateTableInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_RAIN_INFO:
					AddWHMJRainInfo(tSOTLVUNIT.stValue.stRainInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_QISHOUHU:
					AddChangShaMjDealHuInfo(tSOTLVUNIT.stValue.stNotifyCheckQishouhu);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_ROAM_INFO:
					AddHaiDiProcess(tSOTLVUNIT.stValue.stRoamInfo.chChair, tSOTLVUNIT.stValue.stRoamInfo.nAllClientTime, tSOTLVUNIT.stValue.stRoamInfo.nMaxShowTime);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_NOTIFY_GANGYAO_TILE:
					AddChangShaMJGangYaoTileAndDice(tSOTLVUNIT.stValue.stNotifyGangyaoTileAndDice);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_GANGYAO_BLOCK_KEY:
					AddEnterGangYaoBlockProcess(tSOTLVUNIT.stValue.stDoGangyaoBlockInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_SHOW_SCORE_INFO:
					AddEnterDapiaoProcess(tSOTLVUNIT.stValue.stShowScoreInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_TABLE_CONF_INFO:
					HandleModuleTableConfigInfo(tSOTLVUNIT.stValue.stTableConfigInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_LUA_CONFIG:
					HandleLuaString(tSOTLVUNIT.stValue.stLuaConfigInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CAIPIAO_INFO:
					HandleCaiPiao(tSOTLVUNIT.stValue.stCaiPiaoInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_PLAYER_WIN_TIMES:
					HandlePlayerWinTimes(tSOTLVUNIT.stValue.stPlayerWinTimes);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_BAO_PAI:
					HandleShowBaoPai(tSOTLVUNIT.stValue.stBaoPai);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_BAO_PAI_ACTION:
					AddEnterBaoPaiBlockProcess(tSOTLVUNIT.stValue.stBaoPaiAction);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CHARGE_STATUS:
					ChargeStatueChange(tSOTLVUNIT.stValue.stChargeStatus);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_ROUND_INTELLIGENCE_ADVICE_SWITCH:
					HandleTingAdvice(tSOTLVUNIT);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_TREASURE_BOX:
					AddPandoraBoxInfoProcess(tSOTLVUNIT.stValue.stTreasuseBoxInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_AVATAR_SKILL_INFO:
					AddAvatarSkillInfoProcess(tSOTLVUNIT.stValue.stAvatarSkillInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DEALER_CARD_INFO:
					HandleDealerCardInfo(tSOTLVUNIT.stValue.stDealerCardInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_WHETHER_MINGPAI_INFO:
					AddMingPaiInfoProcess(tSOTLVUNIT.stValue.stWhetherMingpai);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MINGPAI:
					AddWaitMingPaiProcess(tSOTLVUNIT.stValue.stWaitMingPai);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_ROUND_FEATURE:
					HandleRoundFeature(tSOTLVUNIT.stValue.stRoundFeature);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_TABLE_DONNER_INFO:
					HandleTableDonnerInfo(tSOTLVUNIT.stValue.stTableDonnerInfo);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_NEW_SECTION_SCORE_JIE_SUAN:
				{
					if (InGameRankGradeUtil.IsSectionDataOnSelfLeave(roundModel.NewSectionScoreJieSuan))
					{
						break;
					}
					HuanLeGameSo.TNewSectionScoreJieSuan tNewSectionScoreJieSuan = tSOTLVUNIT.stValue?.stNewSectionScoreJieSuan;
					if (tNewSectionScoreJieSuan == null)
					{
						HappyMahjong.Common.Log.Error("HandleNextToDo: new section score data is null", "Game");
						break;
					}
					roundModel.NewSectionScoreJieSuan = tNewSectionScoreJieSuan;
					ProcedureVO procedureVO = new ProcedureVO();
					procedureVO.type = ProcessType.CommonProcess;
					CommonProcessData commonProcessData = new CommonProcessData();
					commonProcessData.callback = delegate
					{
						dispatcher.Dispatch(GameEvent.UpdateNewSectionInfo);
					};
					procedureVO.data = commonProcessData;
					m_processModel.PushProcedure(procedureVO);
					if (InGameRankGradeUtil.IsSectionDataOnSelfLeave(tNewSectionScoreJieSuan))
					{
						TryAddCharmSettleEffectProcess(tNewSectionScoreJieSuan, roundModel, m_processModel, isEarlySettle: true);
					}
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_RECOMMEND_FAN:
				{
					HuanLeGameSo.TRecommendFanInfo stRecommendFanInfo = tSOTLVUNIT.stValue.stRecommendFanInfo;
					HandleRecommendFanInfo(stRecommendFanInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CLIENT_REQUEST_TING_INFO:
				{
					HuanLeGameSo.TClientRequestTingInfo stClientRequestTingInfo = tSOTLVUNIT.stValue.stClientRequestTingInfo;
					HandleClientRequestTingInfo(stClientRequestTingInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DESK_TOP_PRAY_INFO:
				{
					HuanLeGameSo.TDeskTopPrayInfo stDeskTopPrayInfo = tSOTLVUNIT.stValue.stDeskTopPrayInfo;
					DeskTopPrayInfo(stDeskTopPrayInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DESK_TOP_PRAY_AWARD_INFO:
				{
					HuanLeGameSo.TDeskTopPrayAwardInfo stDeskTopPrayAwardInfo = tSOTLVUNIT.stValue.stDeskTopPrayAwardInfo;
					DeskTopPrayAward(stDeskTopPrayAwardInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DESK_TOP_FISH_INFO:
				{
					HuanLeGameSo.TDeskTopFishInfo stDeskTopFishInfo = tSOTLVUNIT.stValue.stDeskTopFishInfo;
					DeskTopFishInfo(stDeskTopFishInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DESK_TOP_FISH_AWARD_INFO:
				{
					HuanLeGameSo.TDeskTopFishAwardInfo stDeskTopFishAwardInfo = tSOTLVUNIT.stValue.stDeskTopFishAwardInfo;
					DeskTopFishAward(stDeskTopFishAwardInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DESK_TOP_FISH_AWARD_HISTORY:
				{
					HuanLeGameSo.TDeskTopFishAwardHistory stDeskTopFishAwardHistory = tSOTLVUNIT.stValue.stDeskTopFishAwardHistory;
					DeskTopFishAwardHistory(stDeskTopFishAwardHistory);
					break;
				}
				}
			}
			SortAndAddProcess();
			foreach (IGameEventListener item in SingletonBase<GameModuleRegistry>.GetInstance().GetGameEventsHandler())
			{
				item.HandleNextToDoAllTLV(tlvCount, tlvInfo, this, msgTVValue);
			}
			DoChooseTile(tlvInfo);
			for (int num = 0; num < tlvCount; num++)
			{
				TSOTLVUNIT tSOTLVUNIT = tlvInfo[num];
				switch ((HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType)
				{
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_NEW_RECOMMEND_FAN_TARGET:
				{
					HuanLeGameSo.TNewRecommendFanTarget stNewRecommendFanTarget = tSOTLVUNIT.stValue.stNewRecommendFanTarget;
					HandleNewRecommendFanDetails(stNewRecommendFanTarget);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_BANKRUPT_KILL_INFO:
				{
					HuanLeGameSo.TBankruptKillInfo stBankruptKillInfo = tSOTLVUNIT.stValue.stBankruptKillInfo;
					HandleBankruptKillInfo(stBankruptKillInfo);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_INTELLIGENCE_ADVICE:
					if (tSOTLVUNIT.stValue.stIntelligenceAdvice != null && tSOTLVUNIT.stValue.stIntelligenceAdvice.iAdviceType == 1)
					{
						m_commonRule.IsTingFromServer = true;
					}
					HandleServerTingAndRecommend(m_roundModel, m_processModel, dispatcher, tSOTLVUNIT.stValue.stIntelligenceAdvice);
					break;
				case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PROTO_BUF:
				{
					HuanLeGameSo.TUnitProtoValueBuf stUnitProtoValueBuf = tSOTLVUNIT.stValue.stUnitProtoValueBuf;
					byte[] array = new byte[stUnitProtoValueBuf.nGameBuf];
					Buffer.BlockCopy(stUnitProtoValueBuf.szGameBuf, 0, array, 0, stUnitProtoValueBuf.nGameBuf);
					byte[] array2 = TSDK4CSharp.TSDKService.Uncompress(1, array, stUnitProtoValueBuf.nGameBuf, stUnitProtoValueBuf.iOriginSize);
					MJSoPb.UnitProtoValue unitProtoValue = null;
					try
					{
						unitProtoValue = new MessageParser<MJSoPb.UnitProtoValue>(() => new MJSoPb.UnitProtoValue()).ParseFrom(array2, 0, array2.Length);
					}
					catch (Exception ex)
					{
						HappyMahjong.Common.Log.Error("[NextToDoHelper] ParseFrom error: " + ex.Message, "Game");
						unitProtoValue = new MJSoPb.UnitProtoValue();
					}
					if (unitProtoValue == null)
					{
						unitProtoValue = new MJSoPb.UnitProtoValue();
					}
					if (unitProtoValue.ProtoIntelligenceAdvice != null && unitProtoValue.ProtoIntelligenceAdvice.AdviceType == 1)
					{
						m_commonRule.IsTingFromServer = true;
					}
					HandleProtoServerTingAndRecommend(m_roundModel, m_processModel, dispatcher, unitProtoValue.ProtoIntelligenceAdvice);
					break;
				}
				case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_GRADE_SCORE_INFO:
					if (SectionUtil.IsNewRankScoreScene())
					{
						CheckShowPerformanceScoreChange(roundModel, processModel, tSOTLVUNIT.stValue.stGradeScoreInfo);
					}
					break;
				}
			}
		}
	}

	private void HandleBankruptKillInfo(HuanLeGameSo.TBankruptKillInfo details)
	{
		if (WrappersManagerImpl.IsPatched(35916))
		{
			WrappersManagerImpl.GetPatch(35916).__Gen_Wrap_1(this, details);
			return;
		}
		HappyMahjong.Common.Log.Info("HandleBankruptKillInfo", "Game");
		BigRSlayInfo bigRSlayInfo = new BigRSlayInfo();
		for (int i = 0; i < details.nWinnerNum; i++)
		{
			bigRSlayInfo.winnerList.Add(m_roundModel.GetViewSeatBySeat(details.WinnerList[i]));
		}
		for (int j = 0; j < details.nBankruptNum; j++)
		{
			bigRSlayInfo.loserList.Add(m_roundModel.GetViewSeatBySeat(details.BankruptList[j]));
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.BigRSlayProcess;
		procedureVO.data = bigRSlayInfo;
		procedureVO.viewSeat = 0;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("Add BigRSlayProcess", "Game");
		dispatcher.Dispatch(GameEvent.AddBigRPlayCache, procedureVO);
	}

	public void HandleNewRecommendFanDetails(HuanLeGameSo.TNewRecommendFanTarget details)
	{
		if (WrappersManagerImpl.IsPatched(35915))
		{
			WrappersManagerImpl.GetPatch(35915).__Gen_Wrap_1(this, details);
			return;
		}
		HappyMahjong.Common.Log.Info("HandleNewRecommendFanDetails", "Game");
		dispatcher.Dispatch(GameEvent.RecvFanRecommend, details);
	}

	private void HandleClientRequestTingInfo(HuanLeGameSo.TClientRequestTingInfo requestTingInfo)
	{
		if (WrappersManagerImpl.IsPatched(35904))
		{
			WrappersManagerImpl.GetPatch(35904).__Gen_Wrap_1(this, requestTingInfo);
			return;
		}
		HappyMahjong.Common.Log.Info("HandleClientRequestTingInfo", "Game");
		dispatcher.Dispatch(GameEvent.HandleClientRequestTingInfo);
	}

	private void HandleRecommendFanInfo(HuanLeGameSo.TRecommendFanInfo recommendFanInfo)
	{
		if (WrappersManagerImpl.IsPatched(35898))
		{
			WrappersManagerImpl.GetPatch(35898).__Gen_Wrap_1(this, recommendFanInfo);
		}
		else
		{
			RecvRecommendFanCommand.HandleRecommendFanInfo(recommendFanInfo, m_roundModel, dispatcher);
		}
	}

	public void HandleHuTipsInfo(List<HuanLeGameSo.THuTipsInfo> huTipsInfos)
	{
		if (WrappersManagerImpl.IsPatched(35813))
		{
			WrappersManagerImpl.GetPatch(35813).__Gen_Wrap_1(this, huTipsInfos);
			return;
		}
		List<ChoiceDetailInfo> list = new List<ChoiceDetailInfo>();
		for (int i = 0; i < huTipsInfos.Count; i++)
		{
			ChoiceDetailInfo choiceDetailInfo = new ChoiceDetailInfo();
			choiceDetailInfo.type = UserActionType.Hu;
			choiceDetailInfo.targetMJValue = huTipsInfos[i].chTiles;
			choiceDetailInfo.totalFanWithFengDing = huTipsInfos[i].iWinFanCount;
			switch (huTipsInfos[i].iShowType)
			{
			case 0:
				choiceDetailInfo.fanHuKind = FanHuKind.None;
				break;
			case 1:
				choiceDetailInfo.fanHuKind = FanHuKind.Soft;
				break;
			case 2:
				choiceDetailInfo.fanHuKind = FanHuKind.Hard;
				break;
			case 3:
				choiceDetailInfo.fanHuKind = FanHuKind.Normal;
				break;
			default:
				choiceDetailInfo.fanHuKind = FanHuKind.None;
				break;
			}
			list.Add(choiceDetailInfo);
		}
		CPGActionDataModel cPGActionDataModel = (CPGActionDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel("CPGHModuleName");
		if (cPGActionDataModel != null)
		{
			HappyMahjong.Common.Log.Info("CPGActionDataModel SetInfoList: " + Util.Stringify(list), "Game");
			cPGActionDataModel.SetInfoList(list);
		}
	}

	private void HandleTingAdvice(TSOTLVUNIT tlvUnit)
	{
		if (WrappersManagerImpl.IsPatched(35883))
		{
			WrappersManagerImpl.GetPatch(35883).__Gen_Wrap_1(this, tlvUnit);
			return;
		}
		m_commonRule.IsTingFromServer = tlvUnit.stValue.stRoundIntelligenceAdviceSwitch.iClientCalDiscardTingBasic == 0;
		m_commonRule.IsClientCalTingRecommend = tlvUnit.stValue.stRoundIntelligenceAdviceSwitch.iClientCalDiscardTingAdvice == 1;
		m_commonRule.IsReportTingResult = tlvUnit.stValue.stRoundIntelligenceAdviceSwitch.iClientReportDiscardTingBasic == 1;
		int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("GameAI", "SwitchReportUserAction", 50);
		int num2 = UnityEngine.Random.Range(0, 100);
		m_commonRule.IsReportUserSelect = num2 <= num;
		HappyMahjong.Common.Log.Info($"client calculate ting: {!m_commonRule.IsTingFromServer}, tingAdve: {m_commonRule.IsClientCalTingRecommend}, reportTing: {m_commonRule.IsReportTingResult}, SvrHasCPGAdvice:{tlvUnit.stValue.stRoundIntelligenceAdviceSwitch.iIsOpenIntelligenceAdviceMultiChiPengGang}", "AutoPlay");
		SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().ServerCallTing = m_commonRule.IsTingFromServer;
		SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().ServerToldClientCallAdvice = m_commonRule.IsClientCalTingRecommend;
		SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().ServerHasRecommendCPG = tlvUnit.stValue.stRoundIntelligenceAdviceSwitch.iIsOpenIntelligenceAdviceMultiChiPengGang == 1;
	}

	private void AddPandoraBoxInfoProcess(HuanLeGameSo.TTreasuseBoxInfo stValueStTreasureBoxInfo)
	{
		if (WrappersManagerImpl.IsPatched(35884))
		{
			WrappersManagerImpl.GetPatch(35884).__Gen_Wrap_1(this, stValueStTreasureBoxInfo);
			return;
		}
		HappyMahjong.Common.Log.Info("AddPandoraBoxInfoProcess", "PandoraBox");
		if (stValueStTreasureBoxInfo != null && stValueStTreasureBoxInfo.nPlayerNum > 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.PandoraBoxProcess;
			procedureVO.data = stValueStTreasureBoxInfo;
			m_processModel.PushProcedure(procedureVO);
		}
	}

	private void HandleAvatarSkillPre(HuanLeGameSo.TAvatarSkillInfo stAvatarSkillInfo)
	{
		if (WrappersManagerImpl.IsPatched(35810))
		{
			WrappersManagerImpl.GetPatch(35810).__Gen_Wrap_1(this, stAvatarSkillInfo);
		}
		else
		{
			if (stAvatarSkillInfo == null || stAvatarSkillInfo.nSkillCount <= 0)
			{
				return;
			}
			HuanLeGameSo.TSingleAvatarSkill[] astAvatarSkillList = stAvatarSkillInfo.astAvatarSkillList;
			foreach (HuanLeGameSo.TSingleAvatarSkill tSingleAvatarSkill in astAvatarSkillList)
			{
				if (tSingleAvatarSkill.iSkillID == 5)
				{
					dispatcher.Dispatch(CompareTileEvent.HandleRepeatedVarTile, tSingleAvatarSkill);
				}
				else
				{
					if (tSingleAvatarSkill.iSkillID != 38)
					{
						continue;
					}
					for (int j = 0; j < tSingleAvatarSkill.nEffectNum; j++)
					{
						HuanLeGameSo.TAvatarSkillEffect tAvatarSkillEffect = tSingleAvatarSkill.astAvatarSkillEffectList[j];
						int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(tAvatarSkillEffect.iSeatID);
						string name = ProtoConfigLoader<ItemSkill>.getInstance().getConfigByKey(tSingleAvatarSkill.iSkillID).name;
						if (viewSeatBySeat == 0 && tAvatarSkillEffect.iChgSectionScore > 0)
						{
							m_roundModel.blackboard.SetData<string>("DRAW_TILE_ADD_SCORE_NAME", name);
							m_roundModel.blackboard.SetData<int>("DRAW_TILE_ADD_SCORE", tAvatarSkillEffect.iChgSectionScore);
							HappyMahjong.Common.Log.Info($"DRAW_TILE_ADD_SCORE set value: {tAvatarSkillEffect.iChgSectionScore}", "AvatarSkillInfo");
						}
					}
				}
			}
		}
	}

	private void AddAvatarSkillInfoProcess(HuanLeGameSo.TAvatarSkillInfo stAvatarSkillInfo)
	{
		if (WrappersManagerImpl.IsPatched(35885))
		{
			WrappersManagerImpl.GetPatch(35885).__Gen_Wrap_1(this, stAvatarSkillInfo);
		}
		else if (stAvatarSkillInfo != null)
		{
			HappyMahjong.Common.Log.Info("AddAvatarSkillInfoProcess Skill Num" + stAvatarSkillInfo.nSkillCount, "AvatarSkillInfo");
			if (stAvatarSkillInfo.nSkillCount > 0)
			{
				dispatcher.Dispatch(GameEvent.ExecuteAvatarSkillInfoProcess, stAvatarSkillInfo);
			}
		}
	}

	public void AddEvenTileAddScoreProcess(HuanLeGameSo.TAvatarSkillInfo stAvatarSkillInfo)
	{
		if (WrappersManagerImpl.IsPatched(36343))
		{
			WrappersManagerImpl.GetPatch(36343).__Gen_Wrap_1(this, stAvatarSkillInfo);
		}
		else if (stAvatarSkillInfo != null)
		{
			HappyMahjong.Common.Log.Info("AddAvatarSkillInfoProcess Skill Num" + stAvatarSkillInfo.nSkillCount, "AvatarSkillInfo");
			if (stAvatarSkillInfo.nSkillCount > 0)
			{
				dispatcher.Dispatch(GameEvent.RecvEvenTileAddScoreInfo, stAvatarSkillInfo);
			}
		}
	}

	private void AddMingPaiInfoProcess(HuanLeGameSo.TWhetherMingpai mingPaiInfo)
	{
		if (WrappersManagerImpl.IsPatched(35887))
		{
			WrappersManagerImpl.GetPatch(35887).__Gen_Wrap_1(this, mingPaiInfo);
			return;
		}
		HappyMahjong.Common.Log.Info($"AddMingPaiInfoProcess SeatID is {mingPaiInfo.iSeatID}, Beishu is {mingPaiInfo.iMingPaiBeishu}, Status is {mingPaiInfo.iStatus}", "Game");
		int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(mingPaiInfo.iSeatID);
		MingPaiVO mingPaiVO = new MingPaiVO();
		mingPaiVO.viewSeat = viewSeatBySeat;
		mingPaiVO.mingPaiBeishu = mingPaiInfo.iMingPaiBeishu;
		mingPaiVO.status = mingPaiInfo.iStatus;
		mingPaiVO.mingPaiEnd = mingPaiInfo.iMingPaiEnd;
		m_roundModel.IsMingPai = true;
		if (mingPaiVO.mingPaiEnd == 1)
		{
			VPTimer.CancelAll("DelayReqMingPaiEnd");
		}
		if (mingPaiVO.status == 2)
		{
			dispatcher.Dispatch(GameEvent.SetMingPaiTips, viewSeatBySeat);
		}
		if (viewSeatBySeat == 0 && !CSReplayRecordService.sIsReplaying())
		{
			dispatcher.Dispatch(GameEvent.SetMingPaiDoubleBtn, mingPaiVO);
		}
	}

	private void AddTSystemChangeCardInfoProcess(HuanLeGameSo.TSystemChangeCardInfo systemChangeCardInfo)
	{
		if (WrappersManagerImpl.IsPatched(35809))
		{
			WrappersManagerImpl.GetPatch(35809).__Gen_Wrap_1(this, systemChangeCardInfo);
		}
		else if (systemChangeCardInfo != null)
		{
			HappyMahjong.Common.Log.Info($"AddTSystemChangeCardInfoProcess Change Card Num : {systemChangeCardInfo.nSystemChangeCardNum}", "ChangeQuePai");
			if (systemChangeCardInfo.nSystemChangeCardNum > 0)
			{
				dispatcher.Dispatch(GameEvent.ExecuteChangeQuePaiProcess, systemChangeCardInfo);
			}
		}
	}

	private void AddWaitMingPaiProcess(HuanLeGameSo.TWaitMingPai waitMingPai)
	{
		if (WrappersManagerImpl.IsPatched(35888))
		{
			WrappersManagerImpl.GetPatch(35888).__Gen_Wrap_1(this, waitMingPai);
			return;
		}
		HappyMahjong.Common.Log.Info("AddWaitMingPaiProcess MaxShowTime is " + waitMingPai.nMaxShowTime, "Game");
		dispatcher.Dispatch(GameEvent.ShowTimeCountDown, (int)waitMingPai.nMaxShowTime);
		SingletonBase<Toast>.GetInstance().showUGUI("请选择是否明牌", waitMingPai.nMaxShowTime);
		HappyMahjong.Common.Log.Info($"AddWaitMingPaiProcess SeatID is {waitMingPai.stWhetherMingpai.iSeatID}, Beishu is {waitMingPai.stWhetherMingpai.iMingPaiBeishu}, Status is {waitMingPai.stWhetherMingpai.iStatus}", "Game");
		int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(waitMingPai.stWhetherMingpai.iSeatID);
		MingPaiVO mingPaiVO = new MingPaiVO();
		mingPaiVO.viewSeat = viewSeatBySeat;
		mingPaiVO.mingPaiBeishu = waitMingPai.stWhetherMingpai.iMingPaiBeishu;
		mingPaiVO.status = waitMingPai.stWhetherMingpai.iStatus;
		mingPaiVO.mingPaiEnd = waitMingPai.stWhetherMingpai.iMingPaiEnd;
		if (mingPaiVO.mingPaiEnd == 0)
		{
			VPTimer.CancelAll("DelayReqMingPaiEnd");
			VPTimer.In(waitMingPai.nMaxShowTime, DelayReqMingPaiEnd);
		}
		else
		{
			VPTimer.CancelAll("DelayReqMingPaiEnd");
		}
		if (viewSeatBySeat == 0 && !CSReplayRecordService.sIsReplaying())
		{
			dispatcher.Dispatch(GameEvent.SetMingPaiDoubleBtn, mingPaiVO);
		}
	}

	private void ShowInfoInCardWall(HuanLeGameSo.TTilesShowInfoInCardWall showInfoInCardWall, int tlvNum, TSOTLVUNIT[] tlvList)
	{
		if (WrappersManagerImpl.IsPatched(35811))
		{
			WrappersManagerImpl.GetPatch(35811).__Gen_Wrap_2840(this, showInfoInCardWall, tlvNum, tlvList);
			return;
		}
		HuanLeGameSo.TTilesShowInfoInCardWall tTilesShowInfoInCardWall = showInfoInCardWall;
		TSOTLVUNIT targetTypeTLVUnit = GameUtil.GetTargetTypeTLVUnit(tlvNum, tlvList, HuanLeGameSo.TLV_TYPE.EM_DN_TLV_PARALLEL_QUICK_GIVE_STAGE_WALL);
		if (targetTypeTLVUnit != null)
		{
			HuanLeGameSo.TParallelQuickGiveRoundWall stParallelQuickGiveRoundWall = targetTypeTLVUnit.stValue.stParallelQuickGiveRoundWall;
			List<(int, int)> gaiZhangListInQuickPickWall = GetGaiZhangListInQuickPickWall(stParallelQuickGiveRoundWall);
			tTilesShowInfoInCardWall = new HuanLeGameSo.TTilesShowInfoInCardWall
			{
				nPlayerNum = showInfoInCardWall.nPlayerNum,
				astSingleShowInfoInCardWall = new HuanLeGameSo.TSingleShowInfoInCardWall[showInfoInCardWall.nPlayerNum]
			};
			for (int i = 0; i < tTilesShowInfoInCardWall.nPlayerNum; i++)
			{
				tTilesShowInfoInCardWall.astSingleShowInfoInCardWall[i] = new HuanLeGameSo.TSingleShowInfoInCardWall();
				HuanLeGameSo.TSingleShowInfoInCardWall tSingleShowInfoInCardWall = tTilesShowInfoInCardWall.astSingleShowInfoInCardWall[i];
				List<HuanLeGameSo.TSingleTileShowInfo> list = new List<HuanLeGameSo.TSingleTileShowInfo>();
				for (int j = 0; j < showInfoInCardWall.astSingleShowInfoInCardWall[i].nSingleTileShowInfoNum; j++)
				{
					HuanLeGameSo.TSingleTileShowInfo item = showInfoInCardWall.astSingleShowInfoInCardWall[i].astSingleTileShowInfo[j];
					list.Add(item);
				}
				foreach (var item2 in gaiZhangListInQuickPickWall)
				{
					list.Add(new HuanLeGameSo.TSingleTileShowInfo
					{
						iTileFace = item2.Item1,
						iTileBack = 0,
						iIndex = item2.Item2
					});
				}
				tSingleShowInfoInCardWall.iSeatID = showInfoInCardWall.astSingleShowInfoInCardWall[i].iSeatID;
				tSingleShowInfoInCardWall.nSingleTileShowInfoNum = (short)list.Count;
				tSingleShowInfoInCardWall.astSingleTileShowInfo = list.ToArray();
			}
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowAbilityWallTile;
		procedureVO.data = tTilesShowInfoInCardWall;
		int procedureIndexBackward = m_processModel.GetProcedureIndexBackward(ProcessType.CreateEncounterMascotInWall);
		if (procedureIndexBackward >= 0)
		{
			m_processModel.InsertProcedureToIndex(procedureVO, procedureIndexBackward);
		}
		else
		{
			m_processModel.PushProcedure(procedureVO);
		}
	}

	private List<(int, int)> GetGaiZhangListInQuickPickWall(HuanLeGameSo.TParallelQuickGiveRoundWall source)
	{
		if (WrappersManagerImpl.IsPatched(35812))
		{
			return WrappersManagerImpl.GetPatch(35812).__Gen_Wrap_3409(this, source);
		}
		List<(int, int)> list = new List<(int, int)>();
		for (int i = 0; i < source.nQuickGiveTilesNum; i++)
		{
			HuanLeGameSo.TQuickGiveTile tQuickGiveTile = source.astQuickGiveTiles[i];
			if (tQuickGiveTile.iIs_gaizhang_tile == 1)
			{
				list.Add((tQuickGiveTile.iTile, tQuickGiveTile.iTile_index));
			}
		}
		if (source.stDrawTile.iIs_gaizhang_tile == 1)
		{
			list.Add((source.stDrawTile.iTile, source.stDrawTile.iTile_index));
		}
		return list;
	}

	private void DelayReqMingPaiEnd()
	{
		if (WrappersManagerImpl.IsPatched(35889))
		{
			WrappersManagerImpl.GetPatch(35889).__Gen_Wrap_0(this);
			return;
		}
		SingletonBase<Toast>.GetInstance().Clear();
		dispatcher.Dispatch(GameEvent.RequestMingPai, HuanLeGameSo.SO_MINGPAI_TYPE.END_MINGPAI);
	}

	public static void HandleServerTingAndRecommend(IRoundModel roundModel, IProcessModel processModel, IEventDispatcher dispatcher, HuanLeGameSo.TIntelligenceAdvice advice)
	{
		if (WrappersManagerImpl.IsPatched(35917))
		{
			WrappersManagerImpl.GetPatch(35917).__Gen_Wrap_1635(roundModel, processModel, dispatcher, advice);
			return;
		}
		MJSoPb.TProtoIntelligenceAdvice tProtoIntelligenceAdvice = new MJSoPb.TProtoIntelligenceAdvice();
		tProtoIntelligenceAdvice.AdviceTile = advice.iAdviceTile;
		tProtoIntelligenceAdvice.AdviceType = advice.iAdviceType;
		tProtoIntelligenceAdvice.IntelligenceAdviceTing = new MJSoPb.TProtoIntelligenceAdviceTing
		{
			DiscardTingAdvice = { (IEnumerable<MJSoPb.TProtoOneDiscardTingAdvice>)new List<MJSoPb.TProtoOneDiscardTingAdvice>() }
		};
		for (int i = 0; i < advice.stIntelligenceAdviceTing.nDiscardTingAdviceNum; i++)
		{
			MJSoPb.TProtoOneDiscardTingAdvice tProtoOneDiscardTingAdvice = new MJSoPb.TProtoOneDiscardTingAdvice
			{
				CanTingTile = { (IEnumerable<MJSoPb.TProtoSingleTingTile>)new List<MJSoPb.TProtoSingleTingTile>() }
			};
			tProtoIntelligenceAdvice.IntelligenceAdviceTing.DiscardTingAdvice.Add(tProtoOneDiscardTingAdvice);
			tProtoOneDiscardTingAdvice.DiscardTile = advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].iDiscardTile;
			tProtoOneDiscardTingAdvice.RecommendDiscardFlag = advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].iRecommendDiscardFlag;
			for (int j = 0; j < advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].nCanTingTileNum; j++)
			{
				MJSoPb.TProtoSingleTingTile tProtoSingleTingTile = new MJSoPb.TProtoSingleTingTile();
				tProtoOneDiscardTingAdvice.CanTingTile.Add(tProtoSingleTingTile);
				tProtoSingleTingTile.ShowType = advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].astCanTingTile[j].iShowType;
				tProtoSingleTingTile.TileLeftNum = advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].astCanTingTile[j].iTileLeftNum;
				tProtoSingleTingTile.TingTile = advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].astCanTingTile[j].iTingTile;
				tProtoSingleTingTile.WinFanCount = advice.stIntelligenceAdviceTing.astDiscardTingAdvice[i].astCanTingTile[j].iWinFanCount;
			}
		}
		HandleProtoServerTingAndRecommend(roundModel, processModel, dispatcher, tProtoIntelligenceAdvice);
	}

	public static void HandleProtoServerTingAndRecommend(IRoundModel roundModel, IProcessModel processModel, IEventDispatcher dispatcher, MJSoPb.TProtoIntelligenceAdvice advice)
	{
		if (WrappersManagerImpl.IsPatched(35918))
		{
			WrappersManagerImpl.GetPatch(35918).__Gen_Wrap_1635(roundModel, processModel, dispatcher, advice);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.SetTingInfoFromServer;
		procedureVO.data = advice;
		procedureVO.viewSeat = 0;
		List<IProcessModel> playerProcessModels = roundModel.GetPlayerProcessModels();
		IProcessModel processModel2 = processModel;
		bool flag = false;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			ProcedureVO lastProcedureByType = processModel.GetLastProcedureByType(ProcessType.ParallelProcess);
			if (lastProcedureByType != null)
			{
				HappyMahjong.Common.Log.Info("HandleProtoServerTingAndRecommend Handle RoundProcess Procedures in Queue", "Game");
				for (int i = 0; i < lastProcedureByType.SubProcedures.Count; i++)
				{
					if (lastProcedureByType.SubProcedures[i].viewSeat == 0 && lastProcedureByType.SubProcedures[i].type == ProcessType.EnterGiveProcess)
					{
						lastProcedureByType.SubProcedures.Insert(i, procedureVO);
						flag = true;
						break;
					}
				}
			}
			if (playerProcessModels.Count > 0)
			{
				processModel2 = playerProcessModels[0];
			}
		}
		if (!flag)
		{
			int procedureIndexBackward = processModel2.GetProcedureIndexBackward(ProcessType.EnterGiveProcess, 0);
			if (procedureIndexBackward == -1)
			{
				procedureIndexBackward = processModel2.GetProcedureIndexBackward(ProcessType.DropProcess, 0);
			}
			if (procedureIndexBackward != -1)
			{
				processModel2.InsertProcedureToIndex(procedureVO, procedureIndexBackward);
			}
			else
			{
				processModel2.PushProcedure(procedureVO);
			}
		}
		dispatcher.Dispatch(GameEvent.ExecuteProcessCmd);
	}

	private void DoChooseTile(TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(35914))
		{
			WrappersManagerImpl.GetPatch(35914).__Gen_Wrap_1(this, tlvInfo);
		}
		else if (ChooseTileHelper.GetChooseTileDataModel() != null)
		{
			dispatcher.Dispatch(GameEvent.ExecuteChooseTileNotify, new ChooseTileNotifyData
			{
				tlvInfo = tlvInfo
			});
		}
	}

	private bool CheckSelfFakeHuTLV(HuanLeGameSo.TFakeHuInfoList fakeHuInfo)
	{
		if (WrappersManagerImpl.IsPatched(35806))
		{
			return WrappersManagerImpl.GetPatch(35806).__Gen_Wrap_460(this, fakeHuInfo);
		}
		bool result = false;
		for (int i = 0; i < fakeHuInfo.nFakeHuPlayerNum; i++)
		{
			if (m_roundModel.GetViewSeatBySeat(fakeHuInfo.astFakeHuInfo[i].nSeatID) == 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void HandleModuleTableConfigInfo(HuanLeGameSo.TTableConfigInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35868))
		{
			WrappersManagerImpl.GetPatch(35868).__Gen_Wrap_1(this, info);
			return;
		}
		for (int i = 0; i < info.iTVNum; i++)
		{
			if (info.astTVValue[i].iType == 1)
			{
				SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.totalCardNum = info.astTVValue[i].stValue.stTilesInfo.iCurrentTilesNum;
				dispatcher.Dispatch(GameEvent.UpdateTotalMJCardNum);
			}
		}
	}

	public void HandleCaiPiao(HuanLeGameSo.TCaiPiaoInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35876))
		{
			WrappersManagerImpl.GetPatch(35876).__Gen_Wrap_1(this, info);
			return;
		}
		int num = -1;
		for (int i = 0; i < m_roundModel.GetPlayerCount(); i++)
		{
			num = m_roundModel.GetViewSeatBySeat(i);
			m_roundModel.SetCaiPiao(num, info.szCaiPiaoCount[i]);
		}
	}

	public void HandleLuaString(HuanLeGameSo.TLuaConfigInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35869))
		{
			WrappersManagerImpl.GetPatch(35869).__Gen_Wrap_1(this, info);
		}
		else if (info.iLuaType == 1)
		{
			byte[] bytes = Array.ConvertAll(info.szLuaBuff, (sbyte a) => (byte)a);
			string ruleCondition = Encoding.UTF8.GetString(bytes, 0, info.nLuaBuffLen);
			m_commonRule.SetRuleCondition(ruleCondition);
		}
	}

	public void HandleTableDonnerInfo(HuanLeGameSo.TTableDonnerInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35891))
		{
			WrappersManagerImpl.GetPatch(35891).__Gen_Wrap_1(this, info);
			return;
		}
		for (int i = 0; i < info.nSeatNum; i++)
		{
			int num = info.SeatId[i];
			int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(num);
			long num2 = info.CurDonner[i];
			T2UMsgChangeMoney t2UMsgChangeMoney = new T2UMsgChangeMoney();
			t2UMsgChangeMoney.playerId = PlayerDataMgr.GetInstance().GetPlayerByViewSeat(viewSeatBySeat).playerId;
			t2UMsgChangeMoney.seatId = (short)num;
			t2UMsgChangeMoney.moneyChgReason = 0;
			t2UMsgChangeMoney.moneyChangeValue = num2;
			t2UMsgChangeMoney.moneyCurValue = num2;
			t2UMsgChangeMoney.serviceTag = 0;
			PlayerDataMgr.GetInstance().SetPlayerHappyBeanNumByPlayerId(t2UMsgChangeMoney.playerId, t2UMsgChangeMoney.moneyCurValue, Util.enterGameVO.roomInfo);
			TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKBeanNumberChg, t2UMsgChangeMoney);
		}
	}

	public void HandleRoundFeature(HuanLeGameSo.TRoundFeature roundFeature)
	{
		if (WrappersManagerImpl.IsPatched(35890))
		{
			WrappersManagerImpl.GetPatch(35890).__Gen_Wrap_1(this, roundFeature);
			return;
		}
		m_roundModel.IsCrossScene = roundFeature.dwMatchFeature == 1;
		if (roundFeature.iReportFourPlayerTableMode != 0)
		{
			GameUtil.ReportClientFourPlayerTableMode(SingletonBase<TableLayoutConfig>.GetInstance().CurrentMode == TableLayoutMode.FourPlayers);
		}
	}

	public void HandleUserAllJiInfo(HuanLeGameSo.TUserGZJiInfo info, bool inBalance)
	{
		if (WrappersManagerImpl.IsPatched(36206))
		{
			WrappersManagerImpl.GetPatch(36206).__Gen_Wrap_2721(this, info, inBalance);
			return;
		}
		ZhuoJiDataModel obj = (ZhuoJiDataModel)m_roundModel.GetModel("ZhuojiModuleName");
		obj.ClearAllJiCard();
		obj.UserGZJiInfo = info;
		for (int i = 0; i < info.nUserNum; i++)
		{
			HandleUserJiInfo(info.astGZJiInfo[i], isReplay: true);
		}
		if (inBalance)
		{
			dispatcher.Dispatch(GameEvent.RecvZhuoJiNotify, info);
		}
		else
		{
			dispatcher.Dispatch(GameEvent.ReplayShowJiInfo);
		}
	}

	public void HandleUserJiInfo(HuanLeGameSo.TGuiZhouJiUnit info, bool isReplay)
	{
		if (WrappersManagerImpl.IsPatched(35807))
		{
			WrappersManagerImpl.GetPatch(35807).__Gen_Wrap_2721(this, info, isReplay);
			return;
		}
		ZhuoJiDataModel zhuoJiDataModel = (ZhuoJiDataModel)m_roundModel.GetModel("ZhuojiModuleName");
		if (zhuoJiDataModel == null)
		{
			HappyMahjong.Common.Log.Error("ZhuoJiDataModel is null", "Game");
			return;
		}
		int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(info.chChair);
		for (int i = 0; i < info.nJiTypeNum; i++)
		{
			zhuoJiDataModel.AddJiCard(info.astJiInfo[i], viewSeatBySeat);
		}
		if (!isReplay)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.maxTime = 5;
			procedureVO.type = ProcessType.ShowZhuoJiAniProcess;
			procedureVO.data = info;
			m_processModel.PushProcedure(procedureVO);
		}
	}

	public void AddWinTipsInfo()
	{
		if (WrappersManagerImpl.IsPatched(35855))
		{
			WrappersManagerImpl.GetPatch(35855).__Gen_Wrap_0(this);
		}
		else if (m_roundModel.gameType == GameType.Guobiao || m_roundModel.gameType == GameType.RealGuoBiao)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (m_roundModel.PrivateTableInfo != null)
			{
				stringBuilder.Append(LanguageKey.THIS_ROOM_IS_FULL_OF_FANS);
			}
			else
			{
				stringBuilder.Append(LanguageKey.THIS_MATCH_HAS_BEEN_A_LONG_TIME_COMING);
			}
			stringBuilder.Append((int)m_roundModel.chMinWinFan);
			stringBuilder.Append(LanguageKey.SOME);
			SingletonBase<Toast>.GetInstance().showUGUI(stringBuilder.ToString(), 3f, 1);
		}
		else
		{
			dispatcher.Dispatch(GameEvent.ShowHelpTips, HelpTipsData.CreateData(LanguageKey.NOT_ENOUGH_HUFAN_WITH_MY_FAMILY));
		}
	}

	public void AddFixConfigInfo(HuanLeGameSo.TFixConfigInfo tFixConfigInfo)
	{
		if (WrappersManagerImpl.IsPatched(35847))
		{
			WrappersManagerImpl.GetPatch(35847).__Gen_Wrap_1(this, tFixConfigInfo);
			return;
		}
		m_roundModel.chMinWinFan = tFixConfigInfo.chMinWinFan;
		m_roundModel.ticketBean = Util.GetInt64OrInt32(tFixConfigInfo.llTicketV2, tFixConfigInfo.iTicket);
		m_roundModel.fengDingBeiCount = tFixConfigInfo.iFengDing;
		m_roundModel.softHuFengDingBeiCount = tFixConfigInfo.iSoftFengding;
		m_roundModel.hardHuFengDingBeiCount = tFixConfigInfo.iHardFengding;
		m_roundModel.bigHuFengDingBeiCount = tFixConfigInfo.iDaHuFengDing;
		m_roundModel.topHuFengDingBeiCount = tFixConfigInfo.iDingHuFengDing;
		m_roundModel.zhaNiaoWinType = tFixConfigInfo.nZhongniaoTyle;
		m_roundModel.horseUseNewSetTiles = tFixConfigInfo.chHorseUseNewSetTiles != 0;
		m_roundModel.sectionBaseBet = (float)Util.GetInt64OrInt32(tFixConfigInfo.llSectionBaseBetV2, tFixConfigInfo.iSectionBaseBet) / (float)((tFixConfigInfo.iNewSectionBasebetFactor == 0) ? 1 : tFixConfigInfo.iNewSectionBasebetFactor);
		m_roundModel.sectionAddition = tFixConfigInfo.iSectionAddtion;
		m_roundModel.sectionFengDingBei = tFixConfigInfo.iSectionFengDingBei;
		m_roundModel.sectionFengDingScore = tFixConfigInfo.iSectionFengDingScore;
		m_roundModel.RoomTicketType = (HuanLeGameSo.TicketType)tFixConfigInfo.iTicketType;
		m_roundModel.NewWinSectionFengDing = tFixConfigInfo.iNewSectionWinFengdingScore;
		m_roundModel.NewLoseSectionFengDing = tFixConfigInfo.iNewSectionLoseFengdingScore;
		m_roundModel.WinBeanUpLimitMinValue = tFixConfigInfo.llWinBeanUpLimitMinValue;
		m_roundModel.iWarmReason = tFixConfigInfo.iWarmReason;
		m_roundModel.JiHuZiMo = tFixConfigInfo.iJiHuOnlyZimo == 1;
		PlayerDataMgr.GetInstance().GetSelfInfo().IsFriendAssist = tFixConfigInfo.iIsInFriendAssist == 1;
		dispatcher.Dispatch(GameEvent.SetUserFriendAssistStatus, new KeyValuePair<int, bool>(0, tFixConfigInfo.iIsInFriendAssist == 1));
		if (m_roundModel.gameType == GameType.ModuleCompareTile && SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleCompareTile) is CompareTileDataModel compareTileDataModel)
		{
			compareTileDataModel.ParallelQiYuTileIndices.Clear();
			for (int i = 0; i < tFixConfigInfo.nParallelQiYuTileNum; i++)
			{
				int item = tFixConfigInfo.ParallelQiYuTileIdx[i];
				compareTileDataModel.ParallelQiYuTileIndices.Add(item);
			}
			compareTileDataModel.HasParallelQiYuRoundFunc = tFixConfigInfo.iHasParallelQiYuRoundFunc != 0;
			HappyMahjong.Common.Log.Info($"CompareTile Enable ParallelQiYuRoundFunc: {compareTileDataModel.HasParallelQiYuRoundFunc}, TileIndices Count: {compareTileDataModel.ParallelQiYuTileIndices.Count}", "Game");
		}
		XueLiuBaoPaiDataModel xueLiuBaoPaiDataModel = (XueLiuBaoPaiDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel("XueLiuBaoPaiModuleName");
		if (xueLiuBaoPaiDataModel != null)
		{
			xueLiuBaoPaiDataModel.RoundEndCompareTileBeginLastNTile = tFixConfigInfo.iRoundEndCompareTileBeginLastNTile;
			dispatcher.Dispatch(GameEvent.ShowXueLiuPaoPaiMjCountTips, xueLiuBaoPaiDataModel.RoundEndCompareTileBeginLastNTile);
		}
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleFanRecommend) is NewFanRecommendDataModel newFanRecommendDataModel)
		{
			newFanRecommendDataModel.JihuZimo = tFixConfigInfo.iJiHuOnlyZimo == 1;
		}
		m_roundModel.baseBet = Util.GetInt64OrInt32(tFixConfigInfo.llBaseBetV2, tFixConfigInfo.iBaseBet);
		m_roundModel.teamBaseBet = Util.GetInt64OrInt32(tFixConfigInfo.llTeamBaseBetV2, tFixConfigInfo.iTeamBaseBet);
		if (Util.enterGameVO != null && SingletonBase<GlobalDataModel>.GetInstance() != null && !GameReplayTutorialController.InTutorial)
		{
			long num = (Util.IsTeamBattlePlaying((int)Util.enterGameVO.gameType) ? m_roundModel.teamBaseBet : m_roundModel.baseBet);
			bool flag = Util.IsPrivateRoom();
			bool flag2 = Util.IsMatchRoom();
			RoomConfig roomInfo = Util.enterGameVO.roomInfo;
			if (roomInfo != null && roomInfo.point != num && !flag && !flag2 && !SingletonBase<GlobalDataModel>.GetInstance().bEnterByQuickGame)
			{
				AppRoot.GetInstance().StartCoroutine(ShowRoomInfoUpdate("底分已更新，请检查桌面展示"));
			}
			SingletonBase<GlobalDataModel>.GetInstance().bEnterByQuickGame = false;
		}
		m_roundModel.teamFengDing = tFixConfigInfo.iTeamFengDing;
		if (m_roundModel.gameType == GameType.ChangSha)
		{
			((ChangShaRoundModel)m_roundModel).m_gangYaoTileNum = tFixConfigInfo.nGangyaoTileNum;
			HappyMahjong.Common.Log.Info("NextToDoHelper AddFixConfigInfo, tFixConfigInfo.nGangyaoTileNum is: " + tFixConfigInfo.nGangyaoTileNum, "Game");
		}
		HappyMahjong.Common.Log.Info($"NextToDoHelper AddFixConfigInfo, tFixConfigInfo.iFengDing is: {tFixConfigInfo.iFengDing} " + $"chMinWinFan: {tFixConfigInfo.chMinWinFan} " + $"iSoftFengding: {tFixConfigInfo.iSoftFengding} " + $"iHardFengding: {tFixConfigInfo.iHardFengding} " + $"iSectionFengDingBei: {tFixConfigInfo.iSectionFengDingBei} " + $"iSectionFengDingScore: {tFixConfigInfo.iSectionFengDingScore} " + $"llWinBeanUpLimitMinValue: {tFixConfigInfo.llWinBeanUpLimitMinValue}" + $"tFixConfigInfo.iIsInFriendAssist: {tFixConfigInfo.iIsInFriendAssist}", "Game");
		if (tFixConfigInfo.chHaveZipai == 0)
		{
			MJDef.GUANGDONG_TOTAL_MJ_COUNT = 108;
		}
		else
		{
			MJDef.GUANGDONG_TOTAL_MJ_COUNT = 136;
		}
		if (m_roundModel.gameType == GameType.ModuleHongZhongXueLiu || m_roundModel.gameType == GameType.Module2V2HongZhongXueLiu)
		{
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleHongZhongXueLiuUpdate) is NewHongZhongDataModel newHongZhongDataModel)
			{
				newHongZhongDataModel.isChoosingTileEnabled = tFixConfigInfo.iHasFaCaiGang == 1;
			}
			HappyMahjong.Common.Log.Info($"NewHongZhongXueLiu Enable FaCaiSelectTile: {tFixConfigInfo.iHasFaCaiGang == 1}", "Game");
		}
	}

	[IteratorStateMachine(typeof(_003CShowRoomInfoUpdate_003Ed__46))]
	private IEnumerator ShowRoomInfoUpdate(string text)
	{
		if (WrappersManagerImpl.IsPatched(35848))
		{
			return WrappersManagerImpl.GetPatch(35848).__Gen_Wrap_2894(this, text);
		}
		return new _003CShowRoomInfoUpdate_003Ed__46(0)
		{
			text = text
		};
	}

	public void AddLaiziProcess(HuanLeGameSo.TLaiZi laiziInfo)
	{
		if (WrappersManagerImpl.IsPatched(35843))
		{
			WrappersManagerImpl.GetPatch(35843).__Gen_Wrap_1(this, laiziInfo);
			return;
		}
		HappyMahjong.Common.Log.Debug("laiziInfo.nLaiZiNum:" + laiziInfo.nLaiZiNum, "Game");
		List<LaiziInfo> list = new List<LaiziInfo>();
		for (int i = 0; i < laiziInfo.nLaiZiNum; i++)
		{
			int num = laiziInfo.szLaiZi[i];
			if (m_roundModel.gameType == GameType.WuHan)
			{
				((WHMJRoundModel)m_roundModel).m_nLaiziMJValue = num;
				((WHMJRoundModel)m_roundModel).m_laiziType = laiziInfo.LaiZiType[i];
			}
			else if (m_roundModel.gameType == GameType.GuangDong || (m_roundModel.gameType >= GameType.TuiDaoHu && m_roundModel.gameType <= GameType.HuNanZhuanZhuanZhang))
			{
				((GuangDongRoundModel)m_roundModel).m_nLaiziMJValue = num;
			}
			else if (m_roundModel.gameType >= GameType.ModuleTuidaohu)
			{
				((LaiziDataModel)m_roundModel.GetModel("LaiziModuleName")).LaiziList.Add(num);
			}
			LaiziInfo laiziInfo2 = new LaiziInfo();
			laiziInfo2.LaiziValue = num;
			laiziInfo2.LaiziSourceValue = laiziInfo.szLaiZiSource[i];
			list.Add(laiziInfo2);
		}
		if (!GameUtil.IsShowMJWall(m_roundModel.gameType))
		{
			dispatcher.Dispatch(GameEvent.ResetLaizi, list);
		}
		else if (laiziInfo.nLaiZiNum > 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.LaiziProcess;
			procedureVO.data = (int)laiziInfo.szLaiZiSource[0];
			procedureVO.maxTime = 25;
			m_processModel.PushProcedure(procedureVO);
		}
	}

	public void AddPiziProcess(HuanLeGameSo.TPiZi piziInfo)
	{
		if (WrappersManagerImpl.IsPatched(35857))
		{
			WrappersManagerImpl.GetPatch(35857).__Gen_Wrap_1(this, piziInfo);
		}
		else if (m_roundModel.gameType == GameType.WuHan)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < piziInfo.nPiziNum; i++)
			{
				list.Add(piziInfo.szPizi[i]);
			}
			((WHMJRoundModel)m_roundModel).m_piList.Clear();
			((WHMJRoundModel)m_roundModel).m_piList = list;
		}
	}

	public void AddBuyHorseProcess(HuanLeGameSo.TPlayerMaInfo playerMaInfo)
	{
		if (WrappersManagerImpl.IsPatched(35858))
		{
			WrappersManagerImpl.GetPatch(35858).__Gen_Wrap_1(this, playerMaInfo);
			return;
		}
		((MaimaDataModel)m_roundModel.GetModel("MaimaModuleName")).tPlayerMaInfo = playerMaInfo;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.BuyHorseProcess;
		procedureVO.data = playerMaInfo;
		procedureVO.maxTime = 15;
		m_roundModel.rePaly = false;
		m_processModel.PushProcedure(procedureVO);
	}

	public void SetprivateTableInfo(HuanLeGameSo.TNtfMJPrivateTableInfo mTNtfMJPrivateTableInfo)
	{
		if (WrappersManagerImpl.IsPatched(35860))
		{
			WrappersManagerImpl.GetPatch(35860).__Gen_Wrap_1(this, mTNtfMJPrivateTableInfo);
		}
		else
		{
			m_roundModel.PrivateTableInfo = mTNtfMJPrivateTableInfo;
		}
	}

	public void AddWHMJRainInfo(HuanLeGameSo.TRainInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35861))
		{
			WrappersManagerImpl.GetPatch(35861).__Gen_Wrap_1(this, info);
			return;
		}
		int num = ((WHMJRoundModel)m_roundModel).SetWHMJRain(info.szSeatRain);
		if (num != 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.WHMJRain;
			WHMJRainVO wHMJRainVO = new WHMJRainVO();
			wHMJRainVO.Delta = num;
			wHMJRainVO.Replay = false;
			procedureVO.data = wHMJRainVO;
			procedureVO.maxTime = 15;
			m_processModel.PushProcedure(procedureVO);
		}
	}

	public void AddChangShaMjDealHuInfo(HuanLeGameSo.CNotifyCheckQishouhu info)
	{
		if (WrappersManagerImpl.IsPatched(35864))
		{
			WrappersManagerImpl.GetPatch(35864).__Gen_Wrap_1(this, info);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CSMJCheckDealHuProcess;
		procedureVO.data = info;
		procedureVO.maxTime = 25;
		m_roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		HappyMahjong.Common.Log.Info("AddChangShaMjDealHuInfo info.nAllClientTime is: " + info.nAllClientTime + ", info.nMaxShowTime is: " + info.nMaxShowTime, "Game");
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("Add AddChangShaMjDealHuInfo", "Game");
	}

	public void AddChangShaMJGangYaoTileAndDice(HuanLeGameSo.TNotifyGangyaoTileAndDice info)
	{
		if (WrappersManagerImpl.IsPatched(35865))
		{
			WrappersManagerImpl.GetPatch(35865).__Gen_Wrap_1(this, info);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CSMJGangYaoDisplayTileAndDiceProcess;
		GangYaoTileAndDiceVO gangYaoTileAndDiceVO = new GangYaoTileAndDiceVO();
		gangYaoTileAndDiceVO.TNotifyGangyaoTileAndDice = info;
		gangYaoTileAndDiceVO.isReplay = false;
		procedureVO.data = gangYaoTileAndDiceVO;
		procedureVO.maxTime = 18;
		m_roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("Add AddChangShaMJGangYaoTileAndDice", "Game");
	}

	public void AddHaiDiProcess(int seat, int clientTime, int maxShowTime)
	{
		if (WrappersManagerImpl.IsPatched(35837))
		{
			WrappersManagerImpl.GetPatch(35837).__Gen_Wrap_1628(this, seat, clientTime, maxShowTime);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CSMJHaiDiManYouProcess;
		EnterGiveVO enterGiveVO = new EnterGiveVO();
		enterGiveVO.playerViewSeat = m_roundModel.GetViewSeatBySeat(seat);
		procedureVO.data = enterGiveVO;
		procedureVO.maxTime = 18;
		HappyMahjong.Common.Log.Debug("AddHaiDiProcess viewSeat is " + enterGiveVO.playerViewSeat + " allTime " + clientTime + " maxShowTime " + maxShowTime, "Game");
		m_roundModel.SetRoundTime(clientTime, maxShowTime);
		m_processModel.PushProcedure(procedureVO);
	}

	public void AddBuyHorseResultProcess(HuanLeGameSo.TPlayerMaResult playerMaResult)
	{
		if (WrappersManagerImpl.IsPatched(35859))
		{
			WrappersManagerImpl.GetPatch(35859).__Gen_Wrap_1(this, playerMaResult);
		}
		else
		{
			((MaimaDataModel)m_roundModel.GetModel("MaimaModuleName")).tPlayerMaResult = playerMaResult;
		}
	}

	public void AddZhaNiaoProcess(HuanLeGameSo.TDoZN info)
	{
		if (WrappersManagerImpl.IsPatched(35856))
		{
			WrappersManagerImpl.GetPatch(35856).__Gen_Wrap_1(this, info);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ZhaNiaoProcess;
		procedureVO.maxTime = 15;
		m_roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		m_processModel.PushProcedure(procedureVO);
	}

	public void CheckAddZhaNiaoResult(int tlvCount, TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(36344))
		{
			WrappersManagerImpl.GetPatch(36344).__Gen_Wrap_457(this, tlvCount, tlvInfo);
			return;
		}
		for (int i = 0; i < tlvCount; i++)
		{
			if (tlvInfo[i].nType == 62)
			{
				HuanLeGameSo.TZaNiaoInfo stZaNiaoInfo = tlvInfo[i].stValue.stZaNiaoInfo;
				AddZhaNiaoResultFromTZaNiaoInfo(stZaNiaoInfo);
				AdjustZhaNiaoProcessOrder(m_processModel, m_roundModel);
			}
		}
	}

	public void AddZhaNiaoResultFromTZaNiaoInfo(HuanLeGameSo.TZaNiaoInfo tlvValue)
	{
		if (WrappersManagerImpl.IsPatched(36345))
		{
			WrappersManagerImpl.GetPatch(36345).__Gen_Wrap_1(this, tlvValue);
			return;
		}
		if (tlvValue == null)
		{
			HappyMahjong.Common.Log.Error("AddZhaNiaoResultFromTZaNiaoInfo: stZaNiaoInfo is null", "Game");
			return;
		}
		ProcedureVO lastProcedureByType = m_processModel.GetLastProcedureByType(ProcessType.WindAndRainProcess);
		if (lastProcedureByType == null)
		{
			HappyMahjong.Common.Log.Error("AddZhaNiaoResultFromTZaNiaoInfo: can't find windAndRain in procedure queue", "Game");
			return;
		}
		SCMJWindRainVO sCMJWindRainVO = lastProcedureByType.data as SCMJWindRainVO;
		HappyMahjong.Common.Log.Info($"AddZhaNiaoResultFromTZaNiaoInfo: chair={tlvValue.chChair} num={tlvValue.nZaNiaoNum}", "Game");
		ZhaNiaoVO zhaNiaoVO = new ZhaNiaoVO();
		zhaNiaoVO.viewSeat = m_roundModel.GetViewSeatBySeat(tlvValue.chChair);
		for (int i = 0; i < tlvValue.nZaNiaoNum && i < tlvValue.szZaNiao.Length; i++)
		{
			int item = -1;
			if (tlvValue.szZhaNiaoBingo != null && i < tlvValue.szZhaNiaoBingo.Length && tlvValue.szZhaNiaoBingo[i] >= 0)
			{
				item = m_roundModel.GetViewSeatBySeat(tlvValue.szZhaNiaoBingo[i]);
			}
			zhaNiaoVO.targetViewSeats.Add(item);
			zhaNiaoVO.resultMJs.Add(tlvValue.szZaNiao[i]);
		}
		long[] array = new long[4];
		int num = 0;
		for (int j = 0; j < sCMJWindRainVO.beanChangeList.Count; j++)
		{
			array[sCMJWindRainVO.beanChangeList[j].viewSeat] = sCMJWindRainVO.beanChangeList[j].changeValue;
			if (sCMJWindRainVO.beanChangeList[j].changeValue > 0)
			{
				num++;
			}
		}
		for (int k = 0; k < zhaNiaoVO.targetViewSeats.Count; k++)
		{
			int num2 = zhaNiaoVO.targetViewSeats[k];
			if (array[num2] > 0)
			{
				for (int l = 0; l < array.Length; l++)
				{
					if (array[l] < 0)
					{
						zhaNiaoVO.times[num2]++;
						zhaNiaoVO.times[l] += -1;
					}
				}
			}
			else
			{
				if (array[num2] >= 0)
				{
					continue;
				}
				for (int m = 0; m < array.Length; m++)
				{
					if (array[m] > 0)
					{
						zhaNiaoVO.times[num2]--;
						zhaNiaoVO.times[m]++;
					}
				}
			}
		}
		bool flag = true;
		if (sCMJWindRainVO.actionType == UserActionType.Hu || (sCMJWindRainVO.actionType == UserActionType.DianHu && num == 1))
		{
			flag = false;
		}
		for (int n = 0; n < sCMJWindRainVO.beanChangeList.Count; n++)
		{
			if (flag)
			{
				sCMJWindRainVO.beanChangeList[n].birdCount = ((zhaNiaoVO.times[sCMJWindRainVO.beanChangeList[n].viewSeat] > 0) ? zhaNiaoVO.times[sCMJWindRainVO.beanChangeList[n].viewSeat] : 0);
			}
			else
			{
				sCMJWindRainVO.beanChangeList[n].birdCount = ((zhaNiaoVO.times[sCMJWindRainVO.beanChangeList[n].viewSeat] < 0) ? (-zhaNiaoVO.times[sCMJWindRainVO.beanChangeList[n].viewSeat]) : 0);
			}
		}
		MJTileData tilesByViewSeat = m_roundModel.GetTilesByViewSeat(zhaNiaoVO.viewSeat);
		for (int num3 = 0; num3 < zhaNiaoVO.resultMJs.Count; num3++)
		{
			tilesByViewSeat.AddZhaNiao(zhaNiaoVO.resultMJs[num3]);
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ZhaNiaoResultProcess;
		procedureVO.maxTime = 20;
		procedureVO.data = zhaNiaoVO;
		m_processModel.PushProcedure(procedureVO);
		((ZhaNiaoDataModel)m_roundModel.GetModel("ZhaNiaoModuleName")).zhaniaoVo = zhaNiaoVO;
	}

	public long GetXueLiuWinValue(int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel, IProcessModel processModel, UserActionType actionType)
	{
		if (WrappersManagerImpl.IsPatched(36347))
		{
			return WrappersManagerImpl.GetPatch(36347).__Gen_Wrap_3488(this, tlvCount, tlvInfo, roundModel, processModel, actionType);
		}
		long result = 0L;
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			if ((nType != HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DONNNERINFO && nType != HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PRIVATE_SCORE) || tSOTLVUNIT.stValue.stDonnerInfo == null)
			{
				continue;
			}
			new List<SCMJBeanChangeVO>();
			HuanLeGameSo.TDonnerInfo stDonnerInfo = tSOTLVUNIT.stValue.stDonnerInfo;
			for (int j = 0; j < stDonnerInfo.nSeatNum; j++)
			{
				if (new SCMJBeanChangeVO
				{
					viewSeat = roundModel.GetViewSeatBySeat(stDonnerInfo.szSeatId[j])
				}.viewSeat == 0)
				{
					result = stDonnerInfo.ChgDonner[j];
				}
				HappyMahjong.Common.Log.Debug("GetXueLiuWinValue result:" + result, "Game");
			}
		}
		return result;
	}

	public HuanLeGameSo.TDonnerInfo GetDonnerInfo(int tlvCount, TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(36348))
		{
			return WrappersManagerImpl.GetPatch(36348).__Gen_Wrap_3489(this, tlvCount, tlvInfo);
		}
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			if ((nType == HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DONNNERINFO || nType == HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PRIVATE_SCORE) && tSOTLVUNIT.stValue.stDonnerInfo != null)
			{
				return tSOTLVUNIT.stValue.stDonnerInfo;
			}
		}
		return null;
	}

	private void HandleFlyBean(HuanLeGameSo.TDonnerInfo stDonnerInfo)
	{
		if (WrappersManagerImpl.IsPatched(35207))
		{
			WrappersManagerImpl.GetPatch(35207).__Gen_Wrap_1(this, stDonnerInfo);
			return;
		}
		int num = 0;
		List<SCMJBeanChangeVO> list = null;
		if (stDonnerInfo == null)
		{
			return;
		}
		list = new List<SCMJBeanChangeVO>();
		for (int i = 0; i < stDonnerInfo.nSeatNum; i++)
		{
			SCMJBeanChangeVO sCMJBeanChangeVO = new SCMJBeanChangeVO();
			sCMJBeanChangeVO.viewSeat = m_roundModel.GetViewSeatBySeat(stDonnerInfo.szSeatId[i]);
			sCMJBeanChangeVO.changeValue = stDonnerInfo.ChgDonner[i];
			if (sCMJBeanChangeVO.changeValue < 0)
			{
				num++;
			}
			sCMJBeanChangeVO.curBeanValue = stDonnerInfo.CurDonner[i];
			list.Add(sCMJBeanChangeVO);
		}
		if (num <= 0)
		{
			return;
		}
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].changeValue < 0)
			{
				list3.Add(list[j].viewSeat);
			}
			if (list[j].changeValue > 0)
			{
				list2.Add(list[j].viewSeat);
			}
		}
		m_processModel.PushProcedure(ProcedureVOBuilder.BuildFlyBeanProcedureVo(list2, list3));
	}

	[IDTag(0)]
	public bool AddWindRainProcess(int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel, IProcessModel processModel, UserActionType actionType, IGlobalModel globalModel, float timeComplete = -1f)
	{
		if (WrappersManagerImpl.IsPatched(35192))
		{
			return WrappersManagerImpl.GetPatch(35192).__Gen_Wrap_3383(this, tlvCount, tlvInfo, roundModel, processModel, actionType, globalModel, timeComplete);
		}
		if ((actionType == UserActionType.ZiMoRankToolsScore || actionType == UserActionType.ChiPengRankToolsScore) && SectionUtil.IsNewRankScoreScene())
		{
			return false;
		}
		List<SCMJBeanChangeVO> list = null;
		List<MatchScoreChangeVO> list2 = null;
		bool flag = false;
		List<int> list3 = new List<int>();
		long num = 0L;
		HuanLeGameSo.TWinFanInfo winTilesFromTLV = BalanceCommonUtil.GetWinTilesFromTLV(tlvInfo, tlvCount);
		HuanLeGameSo.TWinTileTypeInfoV winTileTypeInfoVFromTLV = BalanceCommonUtil.GetWinTileTypeInfoVFromTLV(tlvInfo, tlvCount);
		HuAniData aniData = SingletonBase<FanAniHelper>.GetInstance().GetAniData(roundModel, winTileTypeInfoVFromTLV, winTilesFromTLV);
		bool needMoveFanPos = true;
		foreach (SingleHuAniData allHuAniDatum in aniData.AllHuAniData)
		{
			if (allHuAniDatum.viewSeat != 2)
			{
				continue;
			}
			foreach (FanAniData allAniDatum in allHuAniDatum.AllAniData)
			{
				if ((allAniDatum.AniLevel.Contains("A") || allAniDatum.AniLevel.Contains("B")) && allAniDatum.name != "")
				{
					needMoveFanPos = false;
				}
				if (allAniDatum.AniLevel.Contains("S") && !SingletonBase<EffectHandler>.GetInstance().CanShowSFan(allHuAniDatum.viewSeat, allAniDatum.name))
				{
					needMoveFanPos = false;
				}
			}
		}
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			HappyMahjong.Common.Log.Info("NextToDoHelper AddWindRainProcess type is: " + nType, "Game");
			switch (nType)
			{
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DONNNERINFO:
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PRIVATE_SCORE:
			{
				if (tSOTLVUNIT.stValue.stDonnerInfo == null)
				{
					break;
				}
				list = new List<SCMJBeanChangeVO>();
				HuanLeGameSo.TDonnerInfo stDonnerInfo = tSOTLVUNIT.stValue.stDonnerInfo;
				if (NeedFlyBean(roundModel.gameType))
				{
					HandleFlyBean(tSOTLVUNIT.stValue.stDonnerInfo);
				}
				for (int k = 0; k < stDonnerInfo.nSeatNum; k++)
				{
					SCMJBeanChangeVO sCMJBeanChangeVO2 = new SCMJBeanChangeVO();
					sCMJBeanChangeVO2.viewSeat = roundModel.GetViewSeatBySeat(stDonnerInfo.szSeatId[k]);
					sCMJBeanChangeVO2.changeValue = stDonnerInfo.ChgDonner[k];
					sCMJBeanChangeVO2.curBeanValue = stDonnerInfo.CurDonner[k];
					list.Add(sCMJBeanChangeVO2);
					flag = true;
					HappyMahjong.Common.Log.Debug("AddWindRainProcess 1, Add Bean change. viewSeat:" + sCMJBeanChangeVO2.viewSeat + " change:" + sCMJBeanChangeVO2.changeValue + " curBeanValue:" + sCMJBeanChangeVO2.curBeanValue, "Game");
					if (roundModel.gameType == GameType.ModuleCompareTile)
					{
						if (sCMJBeanChangeVO2.changeValue > num)
						{
							num = sCMJBeanChangeVO2.changeValue;
							list3.Clear();
							list3.Add(sCMJBeanChangeVO2.viewSeat);
						}
						else if (sCMJBeanChangeVO2.changeValue == num)
						{
							list3.Add(sCMJBeanChangeVO2.viewSeat);
						}
					}
				}
				if (list3.Count == 0)
				{
					break;
				}
				for (int l = 0; l < list.Count; l++)
				{
					for (int m = 0; m < list3.Count; m++)
					{
						list[l].isMaxWin = list[l].viewSeat == list3[m];
					}
				}
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_MATCH_SCORE:
			{
				if (tSOTLVUNIT.stValue.stMatchScoreChgInfo == null)
				{
					break;
				}
				MatchScoreChangeVO matchScoreChangeVO = null;
				HuanLeGameSo.TMatchScoreChgInfo stMatchScoreChgInfo = tSOTLVUNIT.stValue.stMatchScoreChgInfo;
				for (int n = 0; n < 4; n++)
				{
					if (stMatchScoreChgInfo.ChgScore[n] != 0)
					{
						if (list2 == null)
						{
							list2 = new List<MatchScoreChangeVO>();
						}
						matchScoreChangeVO = new MatchScoreChangeVO();
						matchScoreChangeVO.viewSeat = roundModel.GetViewSeatBySeat(n);
						matchScoreChangeVO.changeValue = stMatchScoreChgInfo.ChgScore[n];
						list2.Add(matchScoreChangeVO);
						flag = true;
						HappyMahjong.Common.Log.Debug("Add Score change. viewSeat:" + matchScoreChangeVO.viewSeat + " change:" + matchScoreChangeVO.changeValue, "Game");
					}
				}
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_TEAM_DONNNERINFO:
			{
				if (tSOTLVUNIT.stValue.stTeamDonnerInfo == null)
				{
					break;
				}
				list = new List<SCMJBeanChangeVO>();
				HuanLeGameSo.TTeamDonnerInfo stTeamDonnerInfo = tSOTLVUNIT.stValue.stTeamDonnerInfo;
				int num2 = -1;
				for (int j = 0; j < stTeamDonnerInfo.nSeatNum; j++)
				{
					SCMJBeanChangeVO sCMJBeanChangeVO = new SCMJBeanChangeVO();
					sCMJBeanChangeVO.viewSeat = roundModel.GetViewSeatBySeat(j);
					sCMJBeanChangeVO.changeValue = stTeamDonnerInfo.ChgDonner[j];
					sCMJBeanChangeVO.curBeanValue = stTeamDonnerInfo.CurDonner[j];
					if (sCMJBeanChangeVO.changeValue > 0)
					{
						num2 = sCMJBeanChangeVO.viewSeat;
						flag = true;
					}
					list.Add(sCMJBeanChangeVO);
					HappyMahjong.Common.Log.Debug("AddWindRainProcess teamDonnerInfo, Add Bean change. viewSeat:" + sCMJBeanChangeVO.viewSeat + " change:" + sCMJBeanChangeVO.changeValue + " curBeanValue:" + sCMJBeanChangeVO.curBeanValue, "Game");
				}
				if (stTeamDonnerInfo.iChengBaoType == 1 && num2 != -1)
				{
					AddShanghaizhuanyiProcess(num2);
				}
				break;
			}
			}
		}
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			if (nType != HuanLeGameSo.TLV_TYPE.EM_DN_TLV_GRADE_SCORE_INFO || (!SectionUtil.IsNewRankScoreScene() && (tSOTLVUNIT.stValue.stGradeScoreInfo == null || SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomBean || SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomGoldCoin)))
			{
				continue;
			}
			if (list == null)
			{
				list = new List<SCMJBeanChangeVO>();
			}
			HappyMahjong.Common.Log.Info("Handle tlv: EM_DN_TLV_GRADE_SCORE_INFO!", "Game");
			HuanLeGameSo.TGradeScoreInfo stGradeScoreInfo = tSOTLVUNIT.stValue.stGradeScoreInfo;
			long num3 = 0L;
			for (int num4 = 0; num4 < roundModel.GetPlayerCount(); num4++)
			{
				num3 = GetScoreChangeValue(num4, actionType, stGradeScoreInfo);
				if (num3 == 0L)
				{
					continue;
				}
				bool flag2 = false;
				if (list.Count != 0)
				{
					for (int num5 = 0; num5 < list.Count; num5++)
					{
						if (roundModel.GetViewSeatBySeat(num4) == list[num5].viewSeat)
						{
							list[num5].sectionScoreChangeValue = num3;
							list[num5].curSectionScoreValue = stGradeScoreInfo.CurScore[num4];
							flag = true;
							flag2 = true;
							HappyMahjong.Common.Log.Debug("Add grade score change. viewSeat:" + list[num5].viewSeat + " change:" + num3 + " curScoreValue:" + stGradeScoreInfo.CurScore[num4], "Game");
						}
					}
				}
				if (!flag2)
				{
					SCMJBeanChangeVO sCMJBeanChangeVO3 = new SCMJBeanChangeVO();
					sCMJBeanChangeVO3.viewSeat = roundModel.GetViewSeatBySeat(num4);
					sCMJBeanChangeVO3.sectionScoreChangeValue = num3;
					sCMJBeanChangeVO3.curSectionScoreValue = stGradeScoreInfo.CurScore[num4];
					list.Add(sCMJBeanChangeVO3);
					flag = true;
					HappyMahjong.Common.Log.Debug("Add grade score change. viewSeat:" + sCMJBeanChangeVO3.viewSeat + " change:" + num3 + " curScoreValue:" + stGradeScoreInfo.CurScore[num4], "Game");
				}
			}
			for (int num6 = 0; num6 < roundModel.GetPlayerCount(); num6++)
			{
				if (list.Count == 0)
				{
					continue;
				}
				for (int num7 = 0; num7 < list.Count; num7++)
				{
					if (roundModel.GetViewSeatBySeat(num6) == list[num7].viewSeat)
					{
						list[num7].curSectionScoreValue = stGradeScoreInfo.CurScore[num6];
					}
				}
			}
			if (flag && !SectionUtil.IsNewRankScoreScene() && (actionType == UserActionType.FirstTingScore || actionType == UserActionType.FirstTingSkill))
			{
				ProcedureVO procedureVO = new ProcedureVO();
				procedureVO.maxTime = 5;
				procedureVO.type = ProcessType.FirstTingAnimationProcess;
				processModel.PushProcedure(procedureVO);
			}
		}
		if (flag)
		{
			SCMJWindRainVO sCMJWindRainVO = new SCMJWindRainVO();
			sCMJWindRainVO.actionType = actionType;
			if (list != null && list.Count > 0)
			{
				sCMJWindRainVO.beanChangeList = list;
				if (actionType == UserActionType.ZiMoRankToolsScore || actionType == UserActionType.ChiPengRankToolsScore)
				{
					ClearUnusedValueInBeanChangeList(sCMJWindRainVO.beanChangeList);
				}
			}
			if (list2 != null && list2.Count > 0)
			{
				sCMJWindRainVO.scoreChangeList = list2;
			}
			sCMJWindRainVO.timeComplete = timeComplete;
			sCMJWindRainVO.nodeStyle = 6;
			sCMJWindRainVO.needMoveFanPos = needMoveFanPos;
			CheckAddExtraActionAnimationProcess(roundModel, processModel, sCMJWindRainVO);
			ProcedureVO procedureVO2 = new ProcedureVO();
			procedureVO2.type = ProcessType.WindAndRainProcess;
			procedureVO2.data = sCMJWindRainVO;
			processModel.PushProcedure(procedureVO2);
			HappyMahjong.Common.Log.Info("Add Rain And Wind Process, actionType is: " + actionType, "Game");
			return true;
		}
		return false;
	}

	private void CheckAddExtraActionAnimationProcess(IRoundModel roundModel, IProcessModel processModel, SCMJWindRainVO data)
	{
		if (WrappersManagerImpl.IsPatched(35213))
		{
			WrappersManagerImpl.GetPatch(35213).__Gen_Wrap_1635(this, roundModel, processModel, data);
			return;
		}
		SCMJWindRainVO data2 = data;
		IRoundModel roundModel2 = roundModel;
		if (data2.actionType != UserActionType.GenZhuang || data2.beanChangeList == null)
		{
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CommonProcess;
		CommonProcessData commonProcessData = new CommonProcessData();
		commonProcessData.callback = delegate
		{
			int playerIndex = 0;
			for (int i = 0; i < data2.beanChangeList.Count; i++)
			{
				if (data2.beanChangeList[i].changeValue < 0)
				{
					playerIndex = i;
					break;
				}
			}
			SingletonBase<EffectHandler>.GetInstance().PlayWordEffect(UserActionType.GenZhuang, playerIndex, roundModel2.gameType);
		};
		commonProcessData.duration = 1f;
		procedureVO.data = commonProcessData;
		processModel.PushProcedure(procedureVO);
	}

	private void CheckShowPerformanceScoreChange(IRoundModel roundModel, IProcessModel processModel, HuanLeGameSo.TGradeScoreInfo stGradeScoreInfo)
	{
		if (WrappersManagerImpl.IsPatched(35919))
		{
			WrappersManagerImpl.GetPatch(35919).__Gen_Wrap_1635(this, roundModel, processModel, stGradeScoreInfo);
			return;
		}
		List<PerformanceScoreChangeInfo> list = new List<PerformanceScoreChangeInfo>();
		HuanLeGameSo.TGradeScoreChgInfoDetail tGradeScoreChgInfoDetail = null;
		for (int i = 0; i < roundModel.GetPlayerCount(); i++)
		{
			for (int j = 0; j < stGradeScoreInfo.astGradeScoreChgInfo[i].nNum; j++)
			{
				tGradeScoreChgInfoDetail = stGradeScoreInfo.astGradeScoreChgInfo[i].astGradeScoreChgInfoDetail[j];
				if (tGradeScoreChgInfoDetail.llChgScore != 0L && ((tGradeScoreChgInfoDetail.nType >= 24 && tGradeScoreChgInfoDetail.nType <= 44) || (tGradeScoreChgInfoDetail.nType >= 7 && tGradeScoreChgInfoDetail.nType <= 11) || (tGradeScoreChgInfoDetail.nType >= 45 && tGradeScoreChgInfoDetail.nType <= 46) || tGradeScoreChgInfoDetail.nType == 51))
				{
					PerformanceScoreChangeInfo performanceScoreChangeInfo = new PerformanceScoreChangeInfo();
					performanceScoreChangeInfo.viewSeat = roundModel.GetViewSeatBySeat(i);
					performanceScoreChangeInfo.changeScore = tGradeScoreChgInfoDetail.llChgScore;
					performanceScoreChangeInfo.type = tGradeScoreChgInfoDetail.nType;
					if (performanceScoreChangeInfo.viewSeat == 0)
					{
						list.Add(performanceScoreChangeInfo);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.CommonProcess;
			CommonProcessData commonProcessData = new CommonProcessData();
			commonProcessData.callback = delegate
			{
				dispatcher.Dispatch(GameEvent.ShowPerformanceScoreChange, list);
			};
			procedureVO.data = commonProcessData;
			m_processModel.PushProcedure(procedureVO);
		}
	}

	private bool NeedFlyBean(GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(35206))
		{
			return WrappersManagerImpl.GetPatch(35206).__Gen_Wrap_2881(this, gameType);
		}
		if (gameType == GameType.ModuleCompareTile || gameType == GameType.ModuleRichTile || gameType == GameType.ModuleDragon)
		{
			return true;
		}
		return false;
	}

	public void AddShowHuTimesProcess(int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel, IProcessModel processModel)
	{
		if (WrappersManagerImpl.IsPatched(35254))
		{
			WrappersManagerImpl.GetPatch(35254).__Gen_Wrap_2880(this, tlvCount, tlvInfo, roundModel, processModel);
			return;
		}
		int num = 0;
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			short nType = tSOTLVUNIT.nType;
			HappyMahjong.Common.Log.Info("NextToDoHelper AddShowHuTimesProcess", "Game");
			if (nType != 88 || tSOTLVUNIT.stValue.stPlayerWinTimes == null)
			{
				continue;
			}
			HuanLeGameSo.TPlayerWinTimes stPlayerWinTimes = tSOTLVUNIT.stValue.stPlayerWinTimes;
			for (int j = 0; j < stPlayerWinTimes.SeatWinTimes.Length; j++)
			{
				if (roundModel.GetViewSeatBySeat(j) == 0)
				{
					num = stPlayerWinTimes.SeatWinTimes[j];
				}
			}
		}
		if (num > 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.ShowHuTimesProcess;
			procedureVO.data = num;
			processModel.PushProcedure(procedureVO);
			HappyMahjong.Common.Log.Info("Add Show Hu Times Process, selfHuTimes is: " + num, "Game");
		}
	}

	public void AddShowWinTimesProcess(int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel, IProcessModel processModel)
	{
		if (WrappersManagerImpl.IsPatched(36349))
		{
			WrappersManagerImpl.GetPatch(36349).__Gen_Wrap_2880(this, tlvCount, tlvInfo, roundModel, processModel);
			return;
		}
		int num = 0;
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			short nType = tSOTLVUNIT.nType;
			HappyMahjong.Common.Log.Info("NextToDoHelper AddShowHuTimesProcess", "Game");
			if (nType != 88 || tSOTLVUNIT.stValue.stPlayerWinTimes == null)
			{
				continue;
			}
			HuanLeGameSo.TPlayerWinTimes stPlayerWinTimes = tSOTLVUNIT.stValue.stPlayerWinTimes;
			for (int j = 0; j < stPlayerWinTimes.SeatVictoryTimes.Length; j++)
			{
				if (roundModel.GetViewSeatBySeat(j) == 0)
				{
					num = stPlayerWinTimes.SeatVictoryTimes[j];
					break;
				}
			}
		}
		if (num >= 3)
		{
			dispatcher.Dispatch(GameEvent.ShowHandTileFireEffect, 0);
		}
	}

	private void ClearUnusedValueInBeanChangeList(List<SCMJBeanChangeVO> beanChangeList)
	{
		if (WrappersManagerImpl.IsPatched(35212))
		{
			WrappersManagerImpl.GetPatch(35212).__Gen_Wrap_1(this, beanChangeList);
		}
		else
		{
			if (beanChangeList == null || beanChangeList.Count <= 0)
			{
				return;
			}
			for (int num = beanChangeList.Count - 1; num >= 0; num--)
			{
				beanChangeList[num].changeValue = 0L;
				beanChangeList[num].curBeanValue = 0L;
				if (beanChangeList[num].viewSeat != 0)
				{
					beanChangeList.Remove(beanChangeList[num]);
				}
			}
		}
	}

	private long GetScoreChangeValue(int seat, UserActionType actionType, HuanLeGameSo.TGradeScoreInfo stGradeScoreInfo)
	{
		if (WrappersManagerImpl.IsPatched(35211))
		{
			return WrappersManagerImpl.GetPatch(35211).__Gen_Wrap_3363(this, seat, actionType, stGradeScoreInfo);
		}
		long num = 0L;
		switch (actionType)
		{
		case UserActionType.ZiMoRankToolsScore:
			return InGameRankGradeUtil.GetChangeScore(seat, stGradeScoreInfo, HuanLeGameSo.SO_GRADE_SCORE_CHG_TYPE.SO_GRADE_SCORE_RANKITEM_ZIMO_ADD);
		case UserActionType.ChiPengRankToolsScore:
			return InGameRankGradeUtil.GetChangeScore(seat, stGradeScoreInfo, HuanLeGameSo.SO_GRADE_SCORE_CHG_TYPE.SO_GRADE_SCORE_RANKITEM_CHOWPONG_ADD);
		case UserActionType.FirstTingScore:
			num = InGameRankGradeUtil.GetChangeScore(seat, stGradeScoreInfo, HuanLeGameSo.SO_GRADE_SCORE_CHG_TYPE.SO_GRADE_SCORE_CAN_TING);
			return num + InGameRankGradeUtil.GetRoundChangeScore(seat, stGradeScoreInfo);
		case UserActionType.FirstTingSkill:
			num = InGameRankGradeUtil.GetChangeScore(seat, stGradeScoreInfo, HuanLeGameSo.SO_GRADE_SCORE_CHG_TYPE.SO_GRADE_SCORE_FIRST_TING_SKILL);
			return num + InGameRankGradeUtil.GetRoundChangeScore(seat, stGradeScoreInfo);
		default:
			return InGameRankGradeUtil.GetRoundChangeScore(seat, stGradeScoreInfo);
		}
	}

	[IDTag(1)]
	public bool AddWindRainProcess(HuanLeGameSo.TDonnerInfo stDonnerInfo, int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel, IProcessModel processModel, UserActionType actionType, IGlobalModel globalModel, float timeComplete = -1f)
	{
		if (WrappersManagerImpl.IsPatched(36350))
		{
			return WrappersManagerImpl.GetPatch(36350).__Gen_Wrap_3490(this, stDonnerInfo, tlvCount, tlvInfo, roundModel, processModel, actionType, globalModel, timeComplete);
		}
		List<SCMJBeanChangeVO> list = null;
		bool flag = false;
		if (stDonnerInfo == null)
		{
			return false;
		}
		HuanLeGameSo.TWinFanInfo winTilesFromTLV = BalanceCommonUtil.GetWinTilesFromTLV(tlvInfo, tlvCount);
		HuanLeGameSo.TWinTileTypeInfoV winTileTypeInfoVFromTLV = BalanceCommonUtil.GetWinTileTypeInfoVFromTLV(tlvInfo, tlvCount);
		HuAniData aniData = SingletonBase<FanAniHelper>.GetInstance().GetAniData(roundModel, winTileTypeInfoVFromTLV, winTilesFromTLV);
		if (aniData != null && aniData.AllHuAniData != null && aniData.AllHuAniData.Count > 0 && aniData.AllHuAniData[0].AllAniData.Count > 0)
		{
			_ = aniData.AllHuAniData[0].AllAniData[0].nodeStyle;
			_ = 6;
		}
		list = new List<SCMJBeanChangeVO>();
		for (int i = 0; i < stDonnerInfo.nSeatNum; i++)
		{
			SCMJBeanChangeVO sCMJBeanChangeVO = new SCMJBeanChangeVO();
			sCMJBeanChangeVO.viewSeat = roundModel.GetViewSeatBySeat(stDonnerInfo.szSeatId[i]);
			sCMJBeanChangeVO.changeValue = stDonnerInfo.ChgDonner[i];
			sCMJBeanChangeVO.curBeanValue = stDonnerInfo.CurDonner[i];
			list.Add(sCMJBeanChangeVO);
			flag = true;
			HappyMahjong.Common.Log.Debug("AddWindRainProcess 2, Add Bean change. viewSeat:" + sCMJBeanChangeVO.viewSeat + " change:" + sCMJBeanChangeVO.changeValue + " curBeanValue:" + sCMJBeanChangeVO.curBeanValue, "Game");
		}
		for (int j = 0; j < tlvCount; j++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[j];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			HappyMahjong.Common.Log.Info("NextToDoHelper AddWindRainProcess type is: " + nType, "Game");
			if (nType != HuanLeGameSo.TLV_TYPE.EM_DN_TLV_GRADE_SCORE_INFO || tSOTLVUNIT.stValue.stGradeScoreInfo == null || SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomBean || SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomGoldCoin)
			{
				continue;
			}
			if (list == null)
			{
				list = new List<SCMJBeanChangeVO>();
			}
			HappyMahjong.Common.Log.Info("AddWindRainProcess DealHu Handle tlv: EM_DN_TLV_GRADE_SCORE_INFO!", "Game");
			HuanLeGameSo.TGradeScoreInfo stGradeScoreInfo = tSOTLVUNIT.stValue.stGradeScoreInfo;
			long num = 0L;
			for (int k = 0; k < roundModel.GetPlayerCount(); k++)
			{
				num = InGameRankGradeUtil.GetRoundChangeScore(k, stGradeScoreInfo);
				if (num == 0L)
				{
					continue;
				}
				bool flag2 = false;
				if (list.Count != 0)
				{
					for (int l = 0; l < list.Count; l++)
					{
						if (roundModel.GetViewSeatBySeat(k) == list[l].viewSeat)
						{
							list[l].sectionScoreChangeValue = num;
							list[l].curSectionScoreValue = stGradeScoreInfo.CurScore[k];
							flag = true;
							flag2 = true;
							HappyMahjong.Common.Log.Debug("Add grade score change. viewSeat:" + list[l].viewSeat + " change:" + num + " curScoreValue:" + stGradeScoreInfo.CurScore[k], "Game");
						}
					}
				}
				if (!flag2)
				{
					SCMJBeanChangeVO sCMJBeanChangeVO = new SCMJBeanChangeVO();
					sCMJBeanChangeVO.viewSeat = roundModel.GetViewSeatBySeat(k);
					sCMJBeanChangeVO.sectionScoreChangeValue = num;
					sCMJBeanChangeVO.curSectionScoreValue = stGradeScoreInfo.CurScore[k];
					list.Add(sCMJBeanChangeVO);
					flag = true;
					HappyMahjong.Common.Log.Debug("Add grade score change. viewSeat:" + sCMJBeanChangeVO.viewSeat + " change:" + num + " curScoreValue:" + stGradeScoreInfo.CurScore[k], "Game");
				}
			}
		}
		if (flag)
		{
			SCMJWindRainVO sCMJWindRainVO = new SCMJWindRainVO();
			sCMJWindRainVO.actionType = actionType;
			if (list != null && list.Count > 0)
			{
				sCMJWindRainVO.beanChangeList = list;
			}
			sCMJWindRainVO.timeComplete = timeComplete;
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.WindAndRainProcess;
			procedureVO.data = sCMJWindRainVO;
			processModel.PushProcedure(procedureVO);
			HappyMahjong.Common.Log.Info("Add Rain And Wind Process", "Game");
			return true;
		}
		return false;
	}

	private void AddEnterIgnoreProcess(HuanLeGameSo.TDingque tDingque)
	{
		if (WrappersManagerImpl.IsPatched(35841))
		{
			WrappersManagerImpl.GetPatch(35841).__Gen_Wrap_1(this, tDingque);
			return;
		}
		m_roundModel.SetRoundTime(tDingque.nAllClientTime, tDingque.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowSelfIgnoreProcess;
		procedureVO.data = new DingqueProcessData
		{
			DefaultIgnoreType = tDingque.chDfDingQue,
			CountTime = tDingque.nMaxShowTime
		};
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterIgnoreProcess", "Game");
	}

	public void AddChange3MJsProcess(HuanLeGameSo.TChangeCard tChangeCard)
	{
		if (WrappersManagerImpl.IsPatched(35844))
		{
			WrappersManagerImpl.GetPatch(35844).__Gen_Wrap_1(this, tChangeCard);
		}
		else if (m_roundModel == null)
		{
			HappyMahjong.Common.Log.Error("AddChange3MJsProcess m_roundModel is null", "Game");
		}
		else
		{
			if (tChangeCard.chFlag != 2 && tChangeCard.chFlag != 3 && tChangeCard.chFlag != 1 && tChangeCard.chFlag != 4 && tChangeCard.chFlag != 5 && tChangeCard.chFlag != 6)
			{
				return;
			}
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.bSystemChangeCard)
			{
				HuanNZhangDataModel obj = (HuanNZhangDataModel)m_roundModel.GetModel("HuanNZhangModuleName");
				obj.InChangeNMJMode = true;
				obj.InChangeNMJProcess = true;
				m_roundModel.SetRoundTime(tChangeCard.nAllClientTime, tChangeCard.nMaxShowTime);
				SCMJChangeNMJVO sCMJChangeNMJVO = new SCMJChangeNMJVO();
				sCMJChangeNMJVO.gameType = m_roundModel.gameType;
				for (int i = 0; i < tChangeCard.nDfTileNum; i++)
				{
					HappyMahjong.Common.Log.Info("AddChangeNMJsProcess tile " + tChangeCard.szDfTiles[i], "Game");
					sCMJChangeNMJVO.changeOutList.Add(tChangeCard.szDfTiles[i]);
				}
				sCMJChangeNMJVO.time = (int)m_roundModel.GetTimeout().GetTotalTimeOut();
				sCMJChangeNMJVO.limitNum = tChangeCard.nLimitNum;
				sCMJChangeNMJVO.ConverChangeOutRecommendTilesList(tChangeCard);
				ProcedureVO procedureVO = new ProcedureVO();
				procedureVO.type = ProcessType.ChangeNMJProcess;
				procedureVO.data = sCMJChangeNMJVO;
				procedureVO.maxTime = sCMJChangeNMJVO.time + 5;
				m_processModel.PushProcedure(procedureVO);
				HappyMahjong.Common.Log.Info("Add ChangeNMJProcess", "Game");
			}
			else
			{
				HuanSanZhangDataModel obj2 = (HuanSanZhangDataModel)m_roundModel.GetModel("HuanSanZhangModuleName");
				obj2.InChange3MJMode = true;
				obj2.InChange3MJProcess = true;
				m_roundModel.SetRoundTime(tChangeCard.nAllClientTime, tChangeCard.nMaxShowTime);
				SCMJChangeMJVO sCMJChangeMJVO = new SCMJChangeMJVO();
				for (int j = 0; j < tChangeCard.nDfTileNum; j++)
				{
					HappyMahjong.Common.Log.Info("AddChange3MJsProcess tile " + tChangeCard.szDfTiles[j], "Game");
					sCMJChangeMJVO.changeOutList.Add(tChangeCard.szDfTiles[j]);
				}
				sCMJChangeMJVO.time = (int)m_roundModel.GetTimeout().GetTotalTimeOut();
				if (Util.IsTeamBattlePlaying((int)m_roundModel.gameType))
				{
					sCMJChangeMJVO.bIs2V2 = true;
				}
				ProcedureVO procedureVO2 = new ProcedureVO();
				procedureVO2.type = ProcessType.Change3MJProcess;
				procedureVO2.data = sCMJChangeMJVO;
				procedureVO2.maxTime = sCMJChangeMJVO.time + 5;
				m_processModel.PushProcedure(procedureVO2);
				HappyMahjong.Common.Log.Info("Add Change3MJProcess", "Game");
			}
		}
	}

	public void HandleSelfRedraw(HuanLeGameSo.TDoGiveInfo giveInfo, int tlvCount, TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(35815))
		{
			WrappersManagerImpl.GetPatch(35815).__Gen_Wrap_2840(this, giveInfo, tlvCount, tlvInfo);
			return;
		}
		HappyMahjong.Common.Log.Debug("HandleSelfRedraw nAllClientTime:" + giveInfo.nAllClientTime + " nMaxShowTime:" + giveInfo.nMaxShowTime, "Game");
		HappyMahjong.Common.Log.Debug("HandleSelfRedraw " + JsonUtility.ToJson(giveInfo), "Game");
		ExitMultiTileModule();
		if (m_roundModel.gameType == GameType.ModuleCompareTile)
		{
			if ((SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleCompareTile) as CompareTileDataModel).isInQuickPickRound)
			{
				int num = (int)m_roundModel.GetTimeout().GetTotalTimeOutWithoutClamp();
				m_roundModel.SetRoundTime(num, num);
			}
			else
			{
				m_roundModel.SetRoundTime(giveInfo.nAllClientTime, giveInfo.nMaxShowTime);
			}
		}
		else
		{
			m_roundModel.SetRoundTime(giveInfo.nAllClientTime, giveInfo.nMaxShowTime);
		}
		m_roundModel.doGiveFlag = giveInfo.nFlag;
		if (giveInfo.chTile != 0)
		{
			bool isFlowerRedraw = false;
			List<int> flowerInfo = GetFlowerInfo(tlvCount, tlvInfo);
			if (!SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.isFlowerAsLaiziGang && flowerInfo != null && flowerInfo.Count > 0)
			{
				for (int i = 0; i < flowerInfo.Count; i++)
				{
					isFlowerRedraw = true;
					if (i == 0)
					{
						AddRedrawProcess(m_roundModel.selfSeat, flowerInfo[i], isFlowerRedraw: false);
					}
					else
					{
						AddRedrawProcess(m_roundModel.selfSeat, flowerInfo[i], isFlowerRedraw: true);
					}
					AddBuHuaDropProcess(m_roundModel.selfSeat, flowerInfo[i]);
				}
			}
			if (m_roundModel.gameType == GameType.WuHan)
			{
				((WHMJRoundModel)m_roundModel).m_nFlag = giveInfo.nFlag;
			}
			int tileIndex = GetTileIndex(giveInfo);
			AddRedrawProcess(m_roundModel.selfSeat, giveInfo.chTile, isFlowerRedraw, tileIndex);
		}
		else if (giveInfo.nDrawTileNum > 0 && giveInfo.DrawTiles != null)
		{
			AddRedrawMultiMJsProcess(m_roundModel.selfSeat, giveInfo.DrawTiles, isFlowerRedraw: false, giveInfo.TileIndex);
		}
	}

	[IDTag(0)]
	private int GetTileIndex(HuanLeGameSo.TDoGiveInfo giveInfo)
	{
		if (WrappersManagerImpl.IsPatched(35821))
		{
			return WrappersManagerImpl.GetPatch(35821).__Gen_Wrap_1557(this, giveInfo);
		}
		int result = -1;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			if (giveInfo.TileIndex.Length != 0)
			{
				result = giveInfo.TileIndex[0];
			}
			else
			{
				HappyMahjong.Common.Log.Error("giveInfo TileIndex Length is 0", "Game");
			}
		}
		return result;
	}

	public void HandleOtherRedraw(int seat, int tlvCount, TSOTLVUNIT[] tlvInfo, int chExCmd = 0, int count = 1, List<int> mjIndexList = null, int[] extraTiles = null)
	{
		if (WrappersManagerImpl.IsPatched(35832))
		{
			WrappersManagerImpl.GetPatch(35832).__Gen_Wrap_3417(this, seat, tlvCount, tlvInfo, chExCmd, count, mjIndexList, extraTiles);
			return;
		}
		ExitMultiTileModule();
		bool isFlowerRedraw = false;
		List<int> flowerInfo = GetFlowerInfo(tlvCount, tlvInfo);
		if (flowerInfo != null && flowerInfo.Count > 0)
		{
			for (int i = 0; i < flowerInfo.Count; i++)
			{
				isFlowerRedraw = true;
				if (i == 0)
				{
					AddRedrawProcess(seat, flowerInfo[i], isFlowerRedraw: false);
				}
				else
				{
					AddRedrawProcess(seat, flowerInfo[i], isFlowerRedraw: true);
				}
				AddBuHuaDropProcess(seat, flowerInfo[i]);
			}
		}
		if (ShowOtherRedrawTile(seat, tlvCount, tlvInfo, isFlowerRedraw))
		{
			return;
		}
		if (CSReplayRecordService.sIsReplaying() && chExCmd != 0)
		{
			obOtherPai = chExCmd;
		}
		if (obOtherPai == -1)
		{
			if (CSReplayRecordService.sIsReplaying() && extraTiles != null && extraTiles.Length != 0)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < extraTiles.Length; j++)
				{
					list.Add(extraTiles[j]);
				}
				AddOtherRedrawMJsProcess(seat, list, isFlowerRedraw, mjIndexList);
			}
			else if (count > 1)
			{
				List<int> list2 = new List<int>();
				for (int k = 0; k < count; k++)
				{
					list2.Add(MJDef.MJ_BACK);
				}
				AddOtherRedrawMJsProcess(seat, list2, isFlowerRedraw, mjIndexList);
			}
			else
			{
				int tileIndex = GetTileIndex(mjIndexList);
				AddRedrawProcess(seat, MJDef.MJ_BACK, isFlowerRedraw, tileIndex);
			}
		}
		else if (mjIndexList != null && mjIndexList.Count > 0)
		{
			AddRedrawProcess(seat, obOtherPai, isFlowerRedraw, mjIndexList[0]);
		}
		else
		{
			AddRedrawProcess(seat, obOtherPai, isFlowerRedraw);
		}
	}

	[IDTag(1)]
	private int GetTileIndex(List<int> list)
	{
		if (WrappersManagerImpl.IsPatched(35835))
		{
			return WrappersManagerImpl.GetPatch(35835).__Gen_Wrap_1557(this, list);
		}
		int result = -1;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw && list != null && list.Count > 0)
		{
			result = list[0];
		}
		return result;
	}

	private void ExitMultiTileModule()
	{
		if (WrappersManagerImpl.IsPatched(35816))
		{
			WrappersManagerImpl.GetPatch(35816).__Gen_Wrap_0(this);
			return;
		}
		MultiTileModel multiTileModel = (MultiTileModel)m_roundModel.GetModel("MultiTileModuleName");
		if (multiTileModel != null)
		{
			multiTileModel.IsMultiTileRedraw = false;
			dispatcher.Dispatch(MultiTileEvent.ShowEnterMultiTileEffect, false);
			HappyMahjong.Common.Log.Info("IsMultiTileRedraw: false", "Game");
		}
	}

	private bool ShowOtherRedrawTile(int seat, int tlvCount, TSOTLVUNIT[] tlvInfo, bool isFlowerRedraw)
	{
		if (WrappersManagerImpl.IsPatched(35833))
		{
			return WrappersManagerImpl.GetPatch(35833).__Gen_Wrap_3416(this, seat, tlvCount, tlvInfo, isFlowerRedraw);
		}
		if (Util.IsTeamBattlePlaying((int)m_roundModel.gameType))
		{
			for (int i = 0; i < tlvCount; i++)
			{
				TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
				if (tSOTLVUNIT.nType == 27)
				{
					AddRedrawProcess(seat, tSOTLVUNIT.stValue.stSendCardInfo.chCard, isFlowerRedraw);
					return true;
				}
			}
		}
		else if (m_roundModel.GetSelfGameInfo().tingStatus == TingType.Concealed || m_roundModel.GetSelfGameInfo().tingStatus == TingType.RequestConcealed)
		{
			for (int j = 0; j < tlvCount; j++)
			{
				TSOTLVUNIT tSOTLVUNIT2 = tlvInfo[j];
				if (tSOTLVUNIT2.nType == 27)
				{
					AddRedrawProcess(seat, tSOTLVUNIT2.stValue.stSendCardInfo.chCard, isFlowerRedraw);
					return true;
				}
			}
		}
		else if (m_roundModel.IsMingPaiViewSeat(m_roundModel.GetViewSeatBySeat(seat)))
		{
			for (int k = 0; k < tlvCount; k++)
			{
				TSOTLVUNIT tSOTLVUNIT3 = tlvInfo[k];
				if (tSOTLVUNIT3.nType == 27)
				{
					AddRedrawProcess(seat, tSOTLVUNIT3.stValue.stSendCardInfo.chCard, isFlowerRedraw);
					return true;
				}
			}
		}
		else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			for (int l = 0; l < tlvCount; l++)
			{
				TSOTLVUNIT tSOTLVUNIT4 = tlvInfo[l];
				if (tSOTLVUNIT4.nType != 134)
				{
					continue;
				}
				HuanLeGameSo.TParallelDoGiveInfo stParallelDoGiveInfo = tSOTLVUNIT4.stValue.stParallelDoGiveInfo;
				for (int m = 0; m < stParallelDoGiveInfo.nParallelDoGiveNum; m++)
				{
					HuanLeGameSo.TDoGiveInfo tDoGiveInfo = stParallelDoGiveInfo.astSingleParallelDoGiveInfo[m];
					if (tDoGiveInfo.nSeatID == seat)
					{
						int tileIndex = GetTileIndex(tDoGiveInfo);
						AddRedrawProcess(seat, tDoGiveInfo.chTile, isFlowerRedraw, tileIndex);
						return true;
					}
				}
			}
		}
		return false;
	}

	private List<int> GetFlowerInfo(int tlvCount, TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(35818))
		{
			return WrappersManagerImpl.GetPatch(35818).__Gen_Wrap_3410(this, tlvCount, tlvInfo);
		}
		List<int> list = new List<int>();
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			if (tSOTLVUNIT.nType != 1)
			{
				continue;
			}
			HuanLeGameSo.TFlowerInfo stFlowerInfo = tSOTLVUNIT.stValue.stFlowerInfo;
			int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(stFlowerInfo.nSeat);
			if (stFlowerInfo.nTilesFlowerNum > 0)
			{
				for (int j = 0; j < stFlowerInfo.nTilesFlowerNum; j++)
				{
					list.Add(stFlowerInfo.szFlower[j]);
					HappyMahjong.Common.Log.Info("ViewSeat:" + viewSeatBySeat + " Add Flower:" + stFlowerInfo.szFlower[j], "Game");
				}
			}
		}
		return list;
	}

	[IDTag(1)]
	public static List<ChoiceDetailInfo> GetGangHuList(int tlvCount, TSOTLVUNIT[] tlvInfo, IRoundModel roundModel)
	{
		if (WrappersManagerImpl.IsPatched(35826))
		{
			return WrappersManagerImpl.GetPatch(35826).__Gen_Wrap_3414(tlvCount, tlvInfo, roundModel);
		}
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			if (tSOTLVUNIT.nType == 59)
			{
				return GetGangHuList(tSOTLVUNIT.stValue.stCheckInfoResult, roundModel);
			}
		}
		return new List<ChoiceDetailInfo>();
	}

	[IDTag(0)]
	public static List<ChoiceDetailInfo> GetGangHuList(HuanLeGameSo.TCheckInfoResult info, IRoundModel roundModel)
	{
		if (WrappersManagerImpl.IsPatched(35827))
		{
			return WrappersManagerImpl.GetPatch(35827).__Gen_Wrap_3413(info, roundModel);
		}
		List<ChoiceDetailInfo> list = new List<ChoiceDetailInfo>();
		for (int i = 0; i < info.nCheckInfoNum; i++)
		{
			ChoiceDetailInfo choiceDetailInfo = new ChoiceDetailInfo();
			HuanLeGameSo.SO_CHECK_TYPE chType = (HuanLeGameSo.SO_CHECK_TYPE)info.astCheckInfo[i].chType;
			HappyMahjong.Common.Log.Info("GetGangHuList EM_DN_TLV_CHECK_RESULT type:" + chType.ToString() + " tile:" + info.astCheckInfo[i].chTile, "Game");
			choiceDetailInfo.doubleCheckType = info.astCheckInfo[i].iDoubleConfirmType;
			if ((roundModel.gameType == GameType.ModuleCompareTile && info.astCheckInfo[i].chTile == 74) || GMUtil.OpenRedPacketDebugMode)
			{
				choiceDetailInfo.multiplier = 1f;
			}
			if (info.astCheckInfo[i].chType == 2)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.AnGang;
				choiceDetailInfo.gangNum = info.astCheckInfo[i].chGangNum;
				HappyMahjong.Common.Log.Info("GetGangHuList gangNum " + choiceDetailInfo.gangNum, "Game");
			}
			else if (info.astCheckInfo[i].chType == 1)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.BuGang;
				choiceDetailInfo.gangNum = info.astCheckInfo[i].chGangNum;
				HappyMahjong.Common.Log.Info("GetGangHuList gangNum " + choiceDetailInfo.gangNum, "Game");
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (choiceDetailInfo.targetMJValue == list[j].targetMJValue && choiceDetailInfo.gangNum == list[j].gangNum && choiceDetailInfo.type == list[j].type)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					HappyMahjong.Common.Log.Info("SO_CHECK_BUGANG findSame!", "Game");
					continue;
				}
			}
			else if (info.astCheckInfo[i].chType == 3)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.Hu;
			}
			else if (info.astCheckInfo[i].chType == 4)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.HuCannotPass;
				list.Add(choiceDetailInfo);
				choiceDetailInfo = new ChoiceDetailInfo();
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.Hu;
			}
			else if (info.astCheckInfo[i].chType == 5)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.MingDan;
			}
			else if (info.astCheckInfo[i].chType == 6)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.BuDan;
			}
			else if (info.astCheckInfo[i].chType == 9)
			{
				choiceDetailInfo.targetMJValue = info.astCheckInfo[i].chTile;
				choiceDetailInfo.targetPlayerViewSeat = 0;
				choiceDetailInfo.type = UserActionType.Buhua;
			}
			if (info.astCheckInfo[i].chType == 3)
			{
				choiceDetailInfo.totalFanWithFengDing = info.astCheckInfo[i].iWinFanCount;
				choiceDetailInfo.fengDingType = (HappyMahjong.Common.FengDingType)info.astCheckInfo[i].iFengDingType;
				SetHuTipInfo(choiceDetailInfo, info.astCheckInfo[i].astFanInfo, roundModel);
			}
			list.Add(choiceDetailInfo);
		}
		CPGActionDataModel cPGActionDataModel = (CPGActionDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel("CPGHModuleName");
		if (cPGActionDataModel != null)
		{
			HappyMahjong.Common.Log.Info("CPGActionDataModel SetInfoList: " + Util.Stringify(list), "Game");
			cPGActionDataModel.SetInfoList(list);
		}
		CompareTileDataModel compareTileDataModel = (CompareTileDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel("CompareTileModuleName");
		if (compareTileDataModel != null && !GameUtil.ShowNewHuFanStyle(SingletonBase<ShopDataHelper>.GetInstance().globalModel))
		{
			HappyMahjong.Common.Log.Info("CompareTileDataModel SetInfoList: " + Util.Stringify(list), "Game");
			compareTileDataModel.SetInfoList(list);
		}
		return list;
	}

	private static void SetHuTipInfo(ChoiceDetailInfo detail, HuanLeGameSo.TFanV[] astFanInfo, IRoundModel roundModel)
	{
		if (WrappersManagerImpl.IsPatched(35828))
		{
			WrappersManagerImpl.GetPatch(35828).__Gen_Wrap_12(detail, astFanInfo, roundModel);
		}
		else
		{
			if (astFanInfo == null)
			{
				return;
			}
			detail.huTipsInfo = new List<HuTipInfo>();
			for (int i = 0; i < astFanInfo.Length; i++)
			{
				HuTipInfo huTipInfo = new HuTipInfo();
				HuanLeGameSo.TFanV tFanV = astFanInfo[i];
				huTipInfo.type = tFanV.chType;
				huTipInfo.fan = GameUtil.GetFanCount(tFanV);
				huTipInfo.fanName = roundModel.BalanceConfig.GetNameByIndex(tFanV.iFanIdx);
				detail.huTipsInfo.Add(huTipInfo);
			}
			bool hasFengDing = false;
			FanHuKind huKind = FanHuKind.Normal;
			detail.totalFanWithFengDing = GameUtil.GetTotalFanCountWithFengDing(astFanInfo, roundModel, ref hasFengDing, ref huKind, detail.totalFanWithFengDing, ref detail.fengDingType);
			detail.fanHuKind = huKind;
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw && GameUtil.ShowNewHuFanStyle(SingletonBase<ShopDataHelper>.GetInstance().globalModel))
			{
				detail.huTipsInfo = new List<HuTipInfo>();
				for (int j = 0; j < astFanInfo.Length; j++)
				{
					HuTipInfo huTipInfo2 = new HuTipInfo();
					HuanLeGameSo.TFanV tFanV2 = astFanInfo[j];
					huTipInfo2.type = tFanV2.chType;
					huTipInfo2.fan = GameUtil.GetFanCount(tFanV2);
					huTipInfo2.fanName = roundModel.BalanceConfig.GetNameByIndex(tFanV2.iFanIdx);
					detail.huTipsInfo.Add(huTipInfo2);
				}
				hasFengDing = false;
				huKind = FanHuKind.Normal;
				detail.totalFanWithFengDing = GameUtil.GetTotalFanCountWithFengDing(astFanInfo, roundModel, ref hasFengDing, ref huKind, detail.totalFanWithFengDing, ref detail.fengDingType);
				detail.fanHuKind = huKind;
			}
		}
	}

	public void AddEnterGiveProcess(int seat, int tlvCount, TSOTLVUNIT[] tlvInfo, int chExCmd = 0, bool needSort = false)
	{
		if (WrappersManagerImpl.IsPatched(35825))
		{
			WrappersManagerImpl.GetPatch(35825).__Gen_Wrap_3415(this, seat, tlvCount, tlvInfo, chExCmd, needSort);
			return;
		}
		EnterGiveVO enterGiveVO = new EnterGiveVO();
		enterGiveVO.playerViewSeat = m_roundModel.GetViewSeatBySeat(seat);
		if (seat == m_roundModel.selfSeat)
		{
			List<ChoiceDetailInfo> gangHuList = GetGangHuList(tlvCount, tlvInfo, m_roundModel);
			enterGiveVO.GangHuList = gangHuList;
		}
		enterGiveVO.mWaitExCmdVO = new WaitExCmdVO(enterGiveVO.playerViewSeat, chExCmd);
		if (m_roundModel.gameType == GameType.RealGuoBiao && m_isFakeHu)
		{
			enterGiveVO.isDetectHu = false;
		}
		else
		{
			enterGiveVO.isDetectHu = true;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterGiveProcess;
		procedureVO.data = enterGiveVO;
		procedureVO.viewSeat = enterGiveVO.playerViewSeat;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			m_processModel.PushProcedure(procedureVO);
		}
		else if (!needSort)
		{
			m_processModel.PushProcedure(procedureVO);
		}
		else
		{
			m_toAddProcess.Add(procedureVO);
		}
		m_roundModel.curSvrPlayerViewSeat = enterGiveVO.playerViewSeat;
		HappyMahjong.Common.Log.Info("NextToDoHelper AddEnterGiveProcess seat:" + seat + " viewSeat:" + enterGiveVO.playerViewSeat, "Game");
	}

	private void AddRedrawProcess(int seat, int mjValue, bool isFlowerRedraw, int tileIndexInWall = -1)
	{
		if (WrappersManagerImpl.IsPatched(35819))
		{
			WrappersManagerImpl.GetPatch(35819).__Gen_Wrap_3411(this, seat, mjValue, isFlowerRedraw, tileIndexInWall);
			return;
		}
		RedrawMJVO redrawMJVO = new RedrawMJVO();
		redrawMJVO.viewSeat = m_roundModel.GetViewSeatBySeat(seat);
		redrawMJVO.mjList = new List<int> { mjValue };
		redrawMJVO.needXMargin = true;
		redrawMJVO.showAnimation = true;
		redrawMJVO.sortAfterRedraw = false;
		redrawMJVO.isFlowerRedraw = isFlowerRedraw;
		redrawMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			redrawMJVO.mjIndexListInWall = new List<int>();
			redrawMJVO.mjIndexListInWall.Add(tileIndexInWall);
		}
		ProcedureVO data = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.RedrawProcess, redrawMJVO, redrawMJVO.viewSeat);
		m_processModel.PushProcedure(data);
		HappyMahjong.Common.Log.Info("Redraw MJ ViewSeat:" + redrawMJVO.viewSeat + " mjValue:" + mjValue, "Game");
		DiscardTipUIHandler.tileLastRedraw = mjValue;
	}

	private void AddOtherRedrawMJsProcess(int seat, List<int> mjList, bool isFlowerRedraw, List<int> mjIndexList)
	{
		if (WrappersManagerImpl.IsPatched(35834))
		{
			WrappersManagerImpl.GetPatch(35834).__Gen_Wrap_3412(this, seat, mjList, isFlowerRedraw, mjIndexList);
			return;
		}
		RedrawMJVO redrawMJVO = new RedrawMJVO();
		redrawMJVO.viewSeat = m_roundModel.GetViewSeatBySeat(seat);
		redrawMJVO.mjList = mjList;
		redrawMJVO.needXMargin = true;
		redrawMJVO.showAnimation = true;
		redrawMJVO.sortAfterRedraw = (CSReplayRecordService.sIsReplaying() ? true : false);
		redrawMJVO.isFlowerRedraw = isFlowerRedraw;
		redrawMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			redrawMJVO.mjIndexListInWall = mjIndexList;
		}
		ProcedureVO data = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.RedrawProcess, redrawMJVO, redrawMJVO.viewSeat);
		m_processModel.PushProcedure(data);
		HappyMahjong.Common.Log.Info("Redraw MJ ViewSeat:" + redrawMJVO.viewSeat + " mjValue:" + mjList.ToArray(), "Game");
		DiscardTipUIHandler.tileLastRedraw = mjList[0];
	}

	private void AddRedrawMultiMJsProcess(int seat, int[] mjList, bool isFlowerRedraw, int[] tileIndex)
	{
		if (WrappersManagerImpl.IsPatched(35822))
		{
			WrappersManagerImpl.GetPatch(35822).__Gen_Wrap_3412(this, seat, mjList, isFlowerRedraw, tileIndex);
			return;
		}
		RedrawMJVO redrawMJVO = new RedrawMJVO();
		redrawMJVO.viewSeat = m_roundModel.GetViewSeatBySeat(seat);
		redrawMJVO.mjList = new List<int>();
		for (int i = 0; i < mjList.Length; i++)
		{
			redrawMJVO.mjList.Add(mjList[i]);
			HappyMahjong.Common.Log.Info("Redraw MJ ViewSeat:" + redrawMJVO.viewSeat + " mjValue:" + mjList[i], "Game");
			DiscardTipUIHandler.tileLastRedraw = mjList[i];
		}
		redrawMJVO.onlyLastMJXMargin = true;
		redrawMJVO.needXMargin = true;
		redrawMJVO.showAnimation = true;
		redrawMJVO.sortAfterRedraw = false;
		redrawMJVO.isFlowerRedraw = isFlowerRedraw;
		redrawMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw && tileIndex != null)
		{
			redrawMJVO.mjIndexListInWall = new List<int>();
			redrawMJVO.mjIndexListInWall.AddRange(tileIndex);
		}
		ProcedureVO data = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.RedrawProcess, redrawMJVO, redrawMJVO.viewSeat);
		m_processModel.PushProcedure(data);
	}

	private void AddEnterBlockProcess(HuanLeGameSo.TDoBlockInfo blockInfo)
	{
		if (WrappersManagerImpl.IsPatched(35840))
		{
			WrappersManagerImpl.GetPatch(35840).__Gen_Wrap_1(this, blockInfo);
			return;
		}
		HappyMahjong.Common.Log.Debug("nAllClientTime:" + blockInfo.nAllClientTime + " nMaxShowTime:" + blockInfo.nMaxShowTime, "Game");
		m_roundModel.SetRoundTime(blockInfo.nAllClientTime, blockInfo.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterBlockProcess;
		procedureVO.data = blockInfo;
		procedureVO.viewSeat = 0;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterBlockProcess", "Game");
	}

	private void AddEnterBaoPaiBlockProcess(HuanLeGameSo.TBaoPaiAction info)
	{
		if (WrappersManagerImpl.IsPatched(35881))
		{
			WrappersManagerImpl.GetPatch(35881).__Gen_Wrap_1(this, info);
			return;
		}
		HappyMahjong.Common.Log.Debug("TBaoPaiAction nAllClientTime:" + info.nAllClientTime + " nMaxShowTime:" + info.nMaxShowTime, "Game");
		m_roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterBaoPaiBlockProcess;
		procedureVO.data = info;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterBaoPaiBlockProcess", "Game");
	}

	private void AddEnterWaitProcess()
	{
		if (WrappersManagerImpl.IsPatched(35836))
		{
			WrappersManagerImpl.GetPatch(35836).__Gen_Wrap_0(this);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterWaitProcess;
		procedureVO.data = WaitProcessType.Normal;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterWaitProcess", "Game");
	}

	private void AddEnterWaitDealHuProcess()
	{
		if (WrappersManagerImpl.IsPatched(35838))
		{
			WrappersManagerImpl.GetPatch(35838).__Gen_Wrap_0(this);
			return;
		}
		_ = string.Empty;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterWaitProcess;
		procedureVO.data = WaitProcessType.WaitDealHu;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterWaitProcess", "Game");
	}

	private void AddEnterWaitGangYaoBlockProcess()
	{
		if (WrappersManagerImpl.IsPatched(35839))
		{
			WrappersManagerImpl.GetPatch(35839).__Gen_Wrap_0(this);
			return;
		}
		_ = string.Empty;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterWaitProcess;
		procedureVO.data = WaitProcessType.WaitGangYaoBlock;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterWaitProcess", "Game");
	}

	private void HandleWait(HuanLeGameSo.TDoWaitInfo doWaitInfo, int tlvCount, TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(35830))
		{
			WrappersManagerImpl.GetPatch(35830).__Gen_Wrap_2840(this, doWaitInfo, tlvCount, tlvInfo);
			return;
		}
		HappyMahjong.Common.Log.Info("HandleWait WaitInfo.chFlag:" + doWaitInfo.chFlag + ", " + JsonUtility.ToJson(doWaitInfo), "Game");
		m_roundModel.curSvrPlayerViewSeat = -1;
		if (doWaitInfo.chFlag == 0)
		{
			List<int> indexList = GetIndexList(doWaitInfo);
			HandleOtherRedraw(doWaitInfo.chChair, tlvCount, tlvInfo, doWaitInfo.chExCmd, doWaitInfo.chDrawTileNum, indexList, doWaitInfo.ExtraTiles);
			AddEnterGiveProcess(doWaitInfo.chChair, tlvCount, tlvInfo, doWaitInfo.chExCmd);
		}
		else if (doWaitInfo.chFlag == 2)
		{
			AddEnterGiveProcess(doWaitInfo.chChair, tlvCount, tlvInfo, doWaitInfo.chExCmd);
		}
		else if (doWaitInfo.chFlag == 1)
		{
			AddEnterWaitProcess();
		}
		else if (doWaitInfo.chFlag == 6)
		{
			AddHaiDiProcess(doWaitInfo.chChair, doWaitInfo.nFixTime + 5, doWaitInfo.nFixTime);
		}
		else if (doWaitInfo.chFlag != 3)
		{
			if (doWaitInfo.chFlag == 4)
			{
				AddEnterWaitDealHuProcess();
			}
			else if (doWaitInfo.chFlag != 5 && doWaitInfo.chFlag == 7)
			{
				AddEnterWaitGangYaoBlockProcess();
			}
		}
	}

	private List<int> GetIndexList(HuanLeGameSo.TDoWaitInfo doWaitInfo)
	{
		if (WrappersManagerImpl.IsPatched(35831))
		{
			return WrappersManagerImpl.GetPatch(35831).__Gen_Wrap_3117(this, doWaitInfo);
		}
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			return new List<int>(doWaitInfo.TileIndex);
		}
		return null;
	}

	private void AddBuHuaDropProcess(int seat, int mjValue)
	{
		if (WrappersManagerImpl.IsPatched(35820))
		{
			WrappersManagerImpl.GetPatch(35820).__Gen_Wrap_1678(this, seat, mjValue);
			return;
		}
		DetailData detailData = new DetailData();
		detailData.viewSeat = m_roundModel.GetViewSeatBySeat(seat);
		detailData.mjList = new List<int> { mjValue };
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.BuHuaDropProcess;
		procedureVO.data = detailData;
		procedureVO.viewSeat = detailData.viewSeat;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddBuHuaDropProcess viewSeat:" + detailData.viewSeat + " mjValue:" + detailData.mjValue, "Game");
	}

	public void AddChargeProcess(HuanLeGameSo.TRecharge data)
	{
		if (WrappersManagerImpl.IsPatched(35842))
		{
			WrappersManagerImpl.GetPatch(35842).__Gen_Wrap_1(this, data);
			return;
		}
		HappyMahjong.Common.Log.Info("AddChargeProcess nPlayerTime:" + data.nPlayerTime + " nMaxShowTime:" + data.nMaxShowTime + " nAllClientTime:" + data.nAllClientTime, "Game");
		m_roundModel.SetRoundTime(data.nAllClientTime, data.nMaxShowTime);
		SCMJChargeVO sCMJChargeVO = new SCMJChargeVO();
		sCMJChargeVO.viewSeatList = new List<int>();
		sCMJChargeVO.countDown = data.nMaxShowTime;
		int nRechargeNum = data.nRechargeNum;
		for (int i = 0; i < nRechargeNum; i++)
		{
			int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(data.szSeatId[i]);
			sCMJChargeVO.viewSeatList.Add(viewSeatBySeat);
			if (viewSeatBySeat == 0 && data.szNeedAskShareToReliefSvr[i] == 1)
			{
				sCMJChargeVO.NeedAskShareToReliefSvr = true;
			}
			HappyMahjong.Common.Log.Info("AddChargeProcess viewSeat:" + viewSeatBySeat, "Game");
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ChargeProcess;
		procedureVO.data = sCMJChargeVO;
		procedureVO.maxTime = 30;
		m_processModel.PushProcedure(procedureVO);
		GameBeanEnoughHelper.beginRecharge();
		HappyMahjong.Common.Log.Info("AddChargeProcess", "Game");
	}

	private void AddEnterGangYaoBlockProcess(HuanLeGameSo.TDoGangyaoBlockInfo blockInfo)
	{
		if (WrappersManagerImpl.IsPatched(35866))
		{
			WrappersManagerImpl.GetPatch(35866).__Gen_Wrap_1(this, blockInfo);
			return;
		}
		HappyMahjong.Common.Log.Debug("nAllClientTime:" + blockInfo.nAllClientTime + " nMaxShowTime:" + blockInfo.nMaxShowTime, "Game");
		m_roundModel.SetRoundTime(blockInfo.nAllClientTime, blockInfo.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterGangYaoBlockProcess;
		procedureVO.data = blockInfo;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterBlockProcess", "Game");
	}

	private void HandleOtherGangYaoRedraw(int seat, int tlvCount, TSOTLVUNIT[] tlvInfo, TSOTLVUNIT tlvUnit)
	{
		if (WrappersManagerImpl.IsPatched(36351))
		{
			WrappersManagerImpl.GetPatch(36351).__Gen_Wrap_3491(this, seat, tlvCount, tlvInfo, tlvUnit);
		}
		else if (!ShowOtherRedrawTile(seat, tlvCount, tlvInfo, isFlowerRedraw: false))
		{
			if (obOtherPai == -1)
			{
				AddRedrawProcess(seat, MJDef.MJ_BACK, isFlowerRedraw: false);
			}
			else
			{
				AddRedrawProcess(seat, obOtherPai, isFlowerRedraw: false);
			}
		}
	}

	private void AddEnterDapiaoProcess(HuanLeGameSo.TShowScoreInfo daPiaoInfo)
	{
		if (WrappersManagerImpl.IsPatched(35867))
		{
			WrappersManagerImpl.GetPatch(35867).__Gen_Wrap_1(this, daPiaoInfo);
			return;
		}
		m_roundModel.isDapiaoRoom = true;
		m_roundModel.SetRoundTime(daPiaoInfo.nAllClientTime, daPiaoInfo.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.DaPiaoProcess;
		procedureVO.data = daPiaoInfo;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("AddEnterDapiaoProcess", "Game");
	}

	public void HandlePlayerWinTimes(HuanLeGameSo.TPlayerWinTimes winTimes, bool reconnect = false)
	{
		if (WrappersManagerImpl.IsPatched(35877))
		{
			WrappersManagerImpl.GetPatch(35877).__Gen_Wrap_2721(this, winTimes, reconnect);
			return;
		}
		for (int i = 0; i < winTimes.SeatWinTimes.Length; i++)
		{
			int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(i);
			m_roundModel.huTimes[viewSeatBySeat] = winTimes.SeatWinTimes[i];
		}
		KeyValuePair<HuanLeGameSo.TPlayerWinTimes, bool> kvp = new KeyValuePair<HuanLeGameSo.TPlayerWinTimes, bool>(winTimes, reconnect);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CommonProcess;
		CommonProcessData commonProcessData = new CommonProcessData();
		commonProcessData.callback = delegate
		{
			dispatcher.Dispatch(GameEvent.ShowPlayerWinTimes, kvp);
		};
		procedureVO.data = commonProcessData;
		m_processModel.InsertProcessBeforeProcessType(ProcessType.HuProcess, procedureVO);
	}

	public void HandleDiscardTips(HuanLeGameSo.TDoGiveInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35823))
		{
			WrappersManagerImpl.GetPatch(35823).__Gen_Wrap_1(this, info);
		}
		else
		{
			if (CSReplayRecordService.sIsReplaying() || !DropTileHelp.IsOpen((int)m_roundModel.gameType))
			{
				return;
			}
			IGlobalModel globalModel = SingletonBase<ShopDataHelper>.GetInstance().globalModel;
			if ((globalModel != null && (globalModel.enterGameInfo.matchType != MatchType.None || globalModel.enterGameInfo.enterGameSecretRoomVO.isSecretRoom || (SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) != RoomSectionType.RoomBean && SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) != RoomSectionType.RoomGoldCoin) || globalModel.enterGameInfo.enterTeamBattleGameVO != null)) || !DropTileHelp.isUserServer())
			{
				return;
			}
			PlayerGameInfoVO playerGameInfoByViewSeat = m_roundModel.GetPlayerGameInfoByViewSeat(0);
			HappyMahjong.Common.Log.Info("HandleDiscardTips iActionSeq:" + info.iActionSeq + "tingStatus:" + playerGameInfoByViewSeat.tingStatus, "Game");
			if (playerGameInfoByViewSeat.tingStatus != TingType.Win && playerGameInfoByViewSeat.tingStatus != TingType.Concealed)
			{
				if (info.iActionSeq < 0)
				{
					dispatcher.Dispatch(GameEvent.OnServerDiscardTip, -2);
					return;
				}
				DropTileHelp.AciontSeq = info.iActionSeq;
				MJSoPb.UMJReqMsgTVValue uMJReqMsgTVValue = new MJSoPb.UMJReqMsgTVValue();
				uMJReqMsgTVValue.RequestGiveCardTips = new CRequestGiveCardTips();
				uMJReqMsgTVValue.RequestGiveCardTips.action_seq = info.iActionSeq;
				GameUtil.SendGamePbMessage(MJSoPb.SO_MJ_MSG_ID.ID_SO_REQUEST_GIVE_CARD_TIPS, uMJReqMsgTVValue);
			}
		}
	}

	public void HandleShowBaoPai(HuanLeGameSo.TBaoPai baoPai)
	{
		if (WrappersManagerImpl.IsPatched(35880))
		{
			WrappersManagerImpl.GetPatch(35880).__Gen_Wrap_1(this, baoPai);
			return;
		}
		BaoPaiDataModel baoPaiDataModel = (BaoPaiDataModel)m_roundModel.GetModel("BaoPaiModuleName");
		if (baoPaiDataModel == null)
		{
			HappyMahjong.Common.Log.Error("HandleShowBaoPai round model is null!", "Game");
			return;
		}
		if (baoPaiDataModel.baoPaiValue == baoPai.chTile)
		{
			if (baoPai.chTile == 0)
			{
				SingletonBase<Toast>.GetInstance().showUGUI("更换宝牌中", 4.5f);
			}
			else
			{
				HappyMahjong.Common.Log.Info($"HandleShowBaoPai baoPaiValue is same! Value: {baoPai.chTile}, isReplay: {CSReplayRecordService.sIsReplaying()}", "Game");
			}
			return;
		}
		BaoPaiVO baoPaiVO = new BaoPaiVO();
		baoPaiVO.viewSeat = 0;
		baoPaiVO.mjValue = baoPai.chTile;
		baoPaiVO.prevMjValue = baoPaiDataModel.baoPaiValue;
		baoPaiDataModel.baoPaiValue = baoPai.chTile;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.BaoPaiProcess;
		procedureVO.data = baoPaiVO;
		m_processModel.PushProcedure(procedureVO);
	}

	private void ChargeStatueChange(HuanLeGameSo.TChargeStatus info)
	{
		if (WrappersManagerImpl.IsPatched(35882))
		{
			WrappersManagerImpl.GetPatch(35882).__Gen_Wrap_1(this, info);
		}
		else if (info.iStatus == 1)
		{
			int viewSeatBySeat = m_roundModel.GetViewSeatBySeat(info.iSeatID);
			dispatcher.Dispatch(GameEvent.ChangeChargeStatue, viewSeatBySeat);
		}
	}

	private void HandleDealerCardInfo(HuanLeGameSo.TDealerCardInfo info)
	{
		if (WrappersManagerImpl.IsPatched(35886))
		{
			WrappersManagerImpl.GetPatch(35886).__Gen_Wrap_1(this, info);
			return;
		}
		DiesVO diesVO = new DiesVO();
		diesVO.dealerViewSeat = m_roundModel.GetViewSeatBySeat(info.iDealer);
		diesVO.nRoundNum = info.iRoundWind;
		diesVO.nSubRoundNum = info.iSubRoundWind;
		diesVO.continueWin = m_roundModel.continueWin;
		diesVO.nMenFengNum = ((m_roundModel.selfSeat > info.iDealer) ? (m_roundModel.selfSeat - info.iDealer) : (m_roundModel.selfSeat + m_roundModel.GetPlayerCount() - info.iDealer));
		diesVO.num1 = m_roundModel.dicesList[0];
		diesVO.num2 = m_roundModel.dicesList[1];
		m_roundModel.dealerViewSeat = diesVO.dealerViewSeat;
		HappyMahjong.Common.Log.Info("HandleDealerCardInfo dealerviewseat " + diesVO.dealerViewSeat, "Game");
		if (info.nChooseDealerNum > 1)
		{
			DingZhuangVO dingZhuangVO = new DingZhuangVO();
			dingZhuangVO.diesVo = diesVO;
			dingZhuangVO.zhuangSeat = info.iDealer;
			dingZhuangVO.playersList.Clear();
			for (int i = 0; i < info.nChooseDealerNum; i++)
			{
				dingZhuangVO.playersList.Add(info.szChooseDealerSeatID[i]);
			}
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.DingZhuangProcess;
			procedureVO.data = dingZhuangVO;
			m_processModel.PushProcedure(procedureVO);
		}
		else
		{
			dispatcher.Dispatch(GameEvent.ShowDeskDirections, true);
			dispatcher.Dispatch(GameEvent.SetMenFeng, diesVO);
		}
		RedrawMJVO redrawMJVO = new RedrawMJVO();
		redrawMJVO.viewSeat = diesVO.dealerViewSeat;
		redrawMJVO.mjList = new List<int>();
		for (int j = 0; j < info.nDealerCardNum; j++)
		{
			redrawMJVO.mjList.Add(info.szTiles[j]);
		}
		redrawMJVO.needXMargin = true;
		redrawMJVO.showAnimation = true;
		redrawMJVO.sortAfterRedraw = false;
		redrawMJVO.isFlowerRedraw = false;
		redrawMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
		ProcedureVO data = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.RedrawProcess, redrawMJVO, redrawMJVO.viewSeat);
		m_processModel.PushProcedure(data);
	}

	public void CheckAbnormalMJNum(int nTLVNum, TSOTLVUNIT[] astTLVInfo, IRoundModel roundModel)
	{
		if (WrappersManagerImpl.IsPatched(36352))
		{
			WrappersManagerImpl.GetPatch(36352).__Gen_Wrap_2879(this, nTLVNum, astTLVInfo, roundModel);
			return;
		}
		for (int i = 0; i < nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = astTLVInfo[i];
			if (tSOTLVUNIT.nType == 127)
			{
				HuanLeGameSo.TAbnormalTileConfigInfo stAbnormalTileConfigInfo = tSOTLVUNIT.stValue.stAbnormalTileConfigInfo;
				MJDef.MAX_IN_HAND_COUNT = stAbnormalTileConfigInfo.iPlayer_hand_tile_num;
				MJDef.INITIAL_MJ_COUNT = MJDef.MAX_IN_HAND_COUNT - 1;
				roundModel.leftMJNumInWall = stAbnormalTileConfigInfo.iTotal_tiles_num;
				roundModel.leftValidMJNumInWall = stAbnormalTileConfigInfo.iTotal_tiles_num - stAbnormalTileConfigInfo.iLast_not_draw_tiles_num;
				roundModel.lastNotDrawTilesNum = stAbnormalTileConfigInfo.iLast_not_draw_tiles_num;
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				for (int j = 0; j < tSOTLVUNIT.stValue.stAbnormalTileConfigInfo.nTileNum; j++)
				{
					HappyMahjong.Common.Log.Info("Game", "stAbnormalTileConfigInfo tile {0} num {1}", tSOTLVUNIT.stValue.stAbnormalTileConfigInfo.abnormal_tiles[j], tSOTLVUNIT.stValue.stAbnormalTileConfigInfo.abnormal_tiles_num[j]);
					dictionary[tSOTLVUNIT.stValue.stAbnormalTileConfigInfo.abnormal_tiles[j]] = tSOTLVUNIT.stValue.stAbnormalTileConfigInfo.abnormal_tiles_num[j];
				}
				roundModel.SetAbnormalMJConfig(dictionary);
				dispatcher.Dispatch(TableViewEvent.SetLastNotDrawTilesNum, stAbnormalTileConfigInfo.iLast_not_draw_tiles_num);
			}
		}
	}

	public void AddVariableHideCPGUIBtnProcess()
	{
		if (WrappersManagerImpl.IsPatched(36353))
		{
			WrappersManagerImpl.GetPatch(36353).__Gen_Wrap_0(this);
			return;
		}
		m_processModel.RemoveLastProcedureByType(ProcessType.EnterBlockProcess);
		m_processModel.RemoveLastProcedureByType(ProcessType.VariableEnterBlockProcess);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.VariableHideCPGUIBtn;
		procedureVO.viewSeat = 0;
		m_processModel.PushProcedure(procedureVO);
		HappyMahjong.Common.Log.Info("EM_DN_TLV_VARIABLE_TILE_RESULT_INFO VariableHideCPGUIBtn!", "Game");
	}

	public void DeskTopPrayInfo(HuanLeGameSo.TDeskTopPrayInfo deskInfo)
	{
		if (WrappersManagerImpl.IsPatched(35905))
		{
			WrappersManagerImpl.GetPatch(35905).__Gen_Wrap_1(this, deskInfo);
			return;
		}
		string data = JsonMapper.ToJson(new DeskTopPrayInfoVO
		{
			OwnerSeatID = m_roundModel.GetViewSeatBySeat(deskInfo.iOwnerSeatID),
			PrayValue = deskInfo.iPrayValue,
			MaxPrayValue = deskInfo.iMaxPrayValue
		});
		dispatcher.Dispatch("UpdateDeskPray_Info", data);
	}

	public void DeskTopPrayAward(HuanLeGameSo.TDeskTopPrayAwardInfo deskAward)
	{
		if (WrappersManagerImpl.IsPatched(35906))
		{
			WrappersManagerImpl.GetPatch(35906).__Gen_Wrap_1(this, deskAward);
			return;
		}
		DeskTopPrayAwardVO deskTopPrayAwardVO = new DeskTopPrayAwardVO();
		deskTopPrayAwardVO.OwnerViewSeatID = m_roundModel.GetViewSeatBySeat(deskAward.iOwnerSeatID);
		deskTopPrayAwardVO.HasOwnerAward = deskAward.iHasOwnerAward;
		if (deskAward.stOwnerAward != null)
		{
			deskTopPrayAwardVO.OwnerAward = new DeskAwardItem();
			deskTopPrayAwardVO.OwnerAward.ItemType = deskAward.stOwnerAward.iItemType;
			deskTopPrayAwardVO.OwnerAward.ItemID = deskAward.stOwnerAward.iItemID;
			deskTopPrayAwardVO.OwnerAward.ItemCount = deskAward.stOwnerAward.iItemCount;
		}
		deskTopPrayAwardVO.AwardUserViewSeatID = m_roundModel.GetViewSeatBySeat(deskAward.iAwardUserSeatID);
		deskTopPrayAwardVO.HasUserAward = deskAward.iHasUserAward;
		if (deskAward.stUserAward != null)
		{
			deskTopPrayAwardVO.UserAward = new DeskAwardItem();
			deskTopPrayAwardVO.UserAward.ItemType = deskAward.stUserAward.iItemType;
			deskTopPrayAwardVO.UserAward.ItemID = deskAward.stUserAward.iItemID;
			deskTopPrayAwardVO.UserAward.ItemCount = deskAward.stUserAward.iItemCount;
		}
		string data = JsonMapper.ToJson(deskTopPrayAwardVO);
		dispatcher.Dispatch("UpdateDeskPray_Award", data);
	}

	public void AddShanghaizhuanyiProcess(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(35209))
		{
			WrappersManagerImpl.GetPatch(35209).__Gen_Wrap_466(this, viewSeat);
			return;
		}
		SpecialAniProcessData specialAniProcessData = new SpecialAniProcessData();
		specialAniProcessData.kType = SpecialAniProcessType.TeamRankCoinFly;
		specialAniProcessData.viewSeat = viewSeat;
		specialAniProcessData.time = 0.6f;
		AddSpecialWordProcess(specialAniProcessData);
		specialAniProcessData = new SpecialAniProcessData();
		specialAniProcessData.kType = SpecialAniProcessType.Shanghaizhuanyi;
		specialAniProcessData.viewSeat = viewSeat;
		specialAniProcessData.time = 0.6f;
		AddSpecialWordProcess(specialAniProcessData);
	}

	public void AddSpecialWordProcess(SpecialAniProcessData data)
	{
		if (WrappersManagerImpl.IsPatched(35210))
		{
			WrappersManagerImpl.GetPatch(35210).__Gen_Wrap_1(this, data);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowSpecialWordsProcess;
		procedureVO.data = data;
		procedureVO.maxTime = 3;
		m_processModel.PushProcedure(procedureVO);
		dispatcher.Dispatch(GameEvent.ExecuteProcessCmd);
	}

	public void DeskTopFishInfo(HuanLeGameSo.TDeskTopFishInfo fishInfo)
	{
		if (WrappersManagerImpl.IsPatched(35907))
		{
			WrappersManagerImpl.GetPatch(35907).__Gen_Wrap_1(this, fishInfo);
			return;
		}
		string data = JsonMapper.ToJson(new DeskTopFishInfoVO
		{
			ownerViewSeatID = m_roundModel.GetViewSeatBySeat(fishInfo.iOwnerSeatID),
			currentValue = fishInfo.iCurrentValue,
			maxValue = fishInfo.iMaxValue,
			countDown = fishInfo.iCountDown
		});
		dispatcher.Dispatch("UpdateDeskFish_Info", data);
	}

	public void DeskTopFishAward(HuanLeGameSo.TDeskTopFishAwardInfo fishAward)
	{
		if (WrappersManagerImpl.IsPatched(35908))
		{
			WrappersManagerImpl.GetPatch(35908).__Gen_Wrap_1(this, fishAward);
			return;
		}
		string data = JsonMapper.ToJson(MakeDeskTopFishAwardVO(fishAward));
		dispatcher.Dispatch("UpdateDeskFish_Award", data);
	}

	private DeskTopFishAwardVO MakeDeskTopFishAwardVO(HuanLeGameSo.TDeskTopFishAwardInfo fishAward)
	{
		if (WrappersManagerImpl.IsPatched(35909))
		{
			return WrappersManagerImpl.GetPatch(35909).__Gen_Wrap_3422(this, fishAward);
		}
		DeskTopFishAwardVO result = new DeskTopFishAwardVO
		{
			OwnerViewSeatID = m_roundModel.GetViewSeatBySeat(fishAward.iOwnerSeatID),
			HasOwnerAward = fishAward.iHasOwnerAward
		};
		if (fishAward.stOwnerAward != null)
		{
			result.OwnerAward = new DeskAwardItem();
			result.OwnerAward.ItemType = fishAward.stOwnerAward.iItemType;
			result.OwnerAward.ItemID = fishAward.stOwnerAward.iItemID;
			result.OwnerAward.ItemCount = fishAward.stOwnerAward.iItemCount;
		}
		result.AwardUserViewSeatID = m_roundModel.GetViewSeatBySeat(fishAward.iAwardUserSeatID);
		result.HasUserAward = fishAward.iHasUserAward;
		if (fishAward.stUserAward != null)
		{
			result.UserAward = new DeskAwardItem();
			result.UserAward.ItemType = fishAward.stUserAward.iItemType;
			result.UserAward.ItemID = fishAward.stUserAward.iItemID;
			result.UserAward.ItemCount = fishAward.stUserAward.iItemCount;
		}
		return result;
	}

	public void DeskTopFishAwardHistory(HuanLeGameSo.TDeskTopFishAwardHistory history)
	{
		if (WrappersManagerImpl.IsPatched(35910))
		{
			WrappersManagerImpl.GetPatch(35910).__Gen_Wrap_1(this, history);
		}
		else if (history.nAwardHistoryNum > 0)
		{
			DeskTopFishAwardHistoryVO deskTopFishAwardHistoryVO = new DeskTopFishAwardHistoryVO
			{
				awardInfoArray = new DeskTopFishAwardVO[history.nAwardHistoryNum]
			};
			for (int i = 0; i < history.nAwardHistoryNum; i++)
			{
				HuanLeGameSo.TDeskTopFishAwardInfo fishAward = history.astAwardHistory[i];
				DeskTopFishAwardVO deskTopFishAwardVO = MakeDeskTopFishAwardVO(fishAward);
				deskTopFishAwardHistoryVO.awardInfoArray[i] = deskTopFishAwardVO;
			}
			string data = JsonMapper.ToJson(deskTopFishAwardHistoryVO);
			dispatcher.Dispatch("UpdateDeskFish_AwardHistory", data);
		}
	}

	public static HuanLeGameSo.TBlockInfoExList GetBlockInfoExListFromTLV(TSOTLVUNIT[] astTLVInfo, int nTLVNum)
	{
		if (WrappersManagerImpl.IsPatched(36354))
		{
			return WrappersManagerImpl.GetPatch(36354).__Gen_Wrap_3492(astTLVInfo, nTLVNum);
		}
		HuanLeGameSo.TBlockInfoExList result = null;
		for (int i = 0; i < nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = astTLVInfo[i];
			if (tSOTLVUNIT.nType == 189)
			{
				result = tSOTLVUNIT.stValue.stBlockInfoExList;
			}
		}
		return result;
	}

	public static void AdjustZhaNiaoProcessOrder(IProcessModel processModel, IRoundModel roundModel)
	{
		if (WrappersManagerImpl.IsPatched(36346))
		{
			WrappersManagerImpl.GetPatch(36346).__Gen_Wrap_1(processModel, roundModel);
		}
		else
		{
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
			{
				return;
			}
			ProcedureVO lastProcedureByType = processModel.GetLastProcedureByType(ProcessType.ZhaNiaoResultProcess);
			if (lastProcedureByType == null)
			{
				return;
			}
			processModel.RemoveProcedureByVO(lastProcedureByType);
			if (processModel.GetLastProcedureByType(ProcessType.WindAndRainProcess) == null)
			{
				return;
			}
			processModel.InsertProcessBeforeProcessType(ProcessType.WindAndRainProcess, lastProcedureByType);
			ProcedureVO lastProcedureByType2 = processModel.GetLastProcedureByType(ProcessType.HuProcess);
			if (lastProcedureByType2 == null)
			{
				HappyMahjong.Common.Log.Info("AddZhaNiaoResultFromTZaNiaoInfo: can't find huProcess in procedure queue", "Game");
			}
			else
			{
				HuVO huVO = lastProcedureByType2.data as HuVO;
				float num = 1f;
				for (int i = 0; i < huVO.HuAniData.AllHuAniData.Count; i++)
				{
					if (huVO.HuAniData.AllHuAniData[i].AllAniData.Count > 0 && huVO.HuAniData.AllHuAniData[i].AllAniData[0].Time > num)
					{
						num = huVO.HuAniData.AllHuAniData[i].AllAniData[0].Time;
					}
				}
				lastProcedureByType2.extraAniTime = num;
			}
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.EmptyBlockProcess;
			procedureVO.data = new List<ProcessType> { ProcessType.HuProcess };
			processModel.InsertProcessBeforeProcessType(ProcessType.ZhaNiaoResultProcess, procedureVO);
		}
	}

	public void AddHuIconDelayShowProcess()
	{
		if (WrappersManagerImpl.IsPatched(36355))
		{
			WrappersManagerImpl.GetPatch(36355).__Gen_Wrap_0(this);
			return;
		}
		bool num = m_roundModel.gameType >= GameType.ModuleTuidaohu && SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.huEndGame == 2;
		bool flag = m_processModel.GetLastProcedureByType(ProcessType.HuProcess) != null;
		if (num && flag)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.EmptyBlockProcess;
			procedureVO.extraAniTime = 0.5f;
			procedureVO.data = new List<ProcessType> { ProcessType.WindAndRainProcess };
			m_processModel.PushProcedure(procedureVO);
			ProcedureVO procedureVO2 = new ProcedureVO();
			procedureVO2.type = ProcessType.CommonProcess;
			procedureVO2.data = new CommonProcessData
			{
				callback = delegate
				{
					SingletonBase<EffectHandler>.GetInstance().ShowDelayDestoryHuEffect(bShow: true);
				}
			};
			m_processModel.PushProcedure(procedureVO2);
		}
	}
}
