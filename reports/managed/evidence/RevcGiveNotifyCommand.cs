using System.Collections.Generic;
using GameModule;
using HappyMahjong.Common;
using HappyMahjong.Common.GM;
using HappyMahjong.Game.CompareTile;
using HappyMahjong.Game.MultiTile;
using HuanLeGameSo;
using IFix;
using UnityEngine;
using strange.extensions.command.impl;

namespace HappyMahjong.Game;

public class RevcGiveNotifyCommand : EventCommand
{
	private NextToDoHelper m_nextToDoHelper;

	private CNotifyGive m_data;

	[Inject]
	public IProcessModel processModel { get; set; }

	[Inject]
	public IRoundModel roundModel { get; set; }

	[Inject]
	public ICommonRule iComRule { get; set; }

	[Inject]
	public IGlobalModel globalModel { get; set; }

	public override void Execute()
	{
		if (WrappersManagerImpl.IsPatched(36724))
		{
			WrappersManagerImpl.GetPatch(36724).__Gen_Wrap_0(this);
			return;
		}
		m_nextToDoHelper = new NextToDoHelper(base.dispatcher, roundModel, processModel, iComRule);
		m_data = (CNotifyGive)base.evt.data;
		BalanceCommonUtil.HandlePrivateRoomTips(globalModel.enterGameInfo.enterGameSecretRoomVO.isSecretRoom, base.dispatcher);
		HandleCurAction();
		m_nextToDoHelper.CheckAddZhaNiaoResult(m_data.nTLVNum, m_data.astTLVInfo);
		m_nextToDoHelper.AddHuIconDelayShowProcess();
		CheckPlayerDoTing();
		foreach (IGameEventListener item in SingletonBase<GameModuleRegistry>.GetInstance().GetGameEventsHandler())
		{
			item.HandleAfterDrop(m_data, this);
		}
		HandleNextToDo();
		HandleRestrictInfo();
		HandleMissHuInfo();
		HandleLastAction();
		base.dispatcher.Dispatch(GameEvent.ExecuteProcessCmd);
	}

	private void HandleLastAction()
	{
		if (WrappersManagerImpl.IsPatched(36752))
		{
			WrappersManagerImpl.GetPatch(36752).__Gen_Wrap_0(this);
			return;
		}
		SO_DEAL_TYPE chType = (SO_DEAL_TYPE)m_data.chType;
		Log.Info("HandleLastAction Type:" + chType.ToString() + " mjValue:" + m_data.chTile, "Game");
		if (m_data.chTile == 0 && chType != SO_DEAL_TYPE.SO_DEAL_TYPE_NONE)
		{
			Log.Error("Wrong Notify", "Game");
		}
		if (chType == SO_DEAL_TYPE.SO_DEAL_TYPE_DISCARD)
		{
			HandleFirstTingScore();
		}
		HandleEvenTileAddScore();
	}

	private void HandleCurAction()
	{
		if (WrappersManagerImpl.IsPatched(36729))
		{
			WrappersManagerImpl.GetPatch(36729).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("RevcGiveNotifyCommand seat:" + m_data.chChair + " type:" + m_data.chType + " mjValue:" + m_data.chTile, "Game");
		SO_DEAL_TYPE chType = (SO_DEAL_TYPE)m_data.chType;
		Log.Info("HandleCurAction Type:" + chType.ToString() + " mjValue:" + m_data.chTile, "Game");
		if (m_data.chTile == 0 && chType != SO_DEAL_TYPE.SO_DEAL_TYPE_NONE)
		{
			Log.Error("Wrong Notify", "Game");
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(m_data.chChair);
		if (viewSeatBySeat == 0)
		{
			Util.CancelTimeoutReconnectAfterGive();
		}
		switch (chType)
		{
		case SO_DEAL_TYPE.SO_DEAL_TYPE_DISCARD:
		case SO_DEAL_TYPE.SO_DEAL_TYPE_SPECIALTILE_EXT:
			HandleDiscard();
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_ANGANG:
			HandleGang(UserActionType.AnGang, isGangYao: false);
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_BUGANG:
			HandleGang(UserActionType.BuGang, isGangYao: false);
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_WIN:
			HandleHu();
			if (!SectionUtil.IsNewRankScoreScene())
			{
				HandleZimoRankToolAddScore();
			}
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_SPECIALTILE:
			HandleDiscardSpecial();
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_GENZHUANG:
			HandleGenZhuang();
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_ANGANG_GANGYAO:
			HandleGang(UserActionType.AnGang, isGangYao: true);
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_BUGANG_GANGYAO:
			HandleGang(UserActionType.BuGang, isGangYao: true);
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_FAKE_HU:
			HandleFakeHu();
			break;
		default:
			Log.Error("Unexpected Give Type:" + chType, "Game");
			break;
		case SO_DEAL_TYPE.SO_DEAL_TYPE_NONE:
			break;
		}
		if (viewSeatBySeat == 0)
		{
			ResetEnterGiveStatus();
		}
		HandleWinTimes();
	}

	private void ResetEnterGiveStatus()
	{
		if (WrappersManagerImpl.IsPatched(36743))
		{
			WrappersManagerImpl.GetPatch(36743).__Gen_Wrap_0(this);
		}
		else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleCompareTile) is CompareTileDataModel compareTileDataModel)
		{
			compareTileDataModel.isInGiveStatus = false;
		}
	}

	private void HandleWinTimes()
	{
		if (WrappersManagerImpl.IsPatched(36744))
		{
			WrappersManagerImpl.GetPatch(36744).__Gen_Wrap_0(this);
		}
		else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			m_nextToDoHelper.AddShowWinTimesProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel);
		}
	}

	private void HandleNextToDo()
	{
		if (WrappersManagerImpl.IsPatched(36749))
		{
			WrappersManagerImpl.GetPatch(36749).__Gen_Wrap_0(this);
		}
		else if (m_data.nTLVNum > 0)
		{
			m_nextToDoHelper.HandleNextToDo(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, iComRule);
		}
	}

	private bool CheckHasMingLou()
	{
		if (WrappersManagerImpl.IsPatched(36746))
		{
			return WrappersManagerImpl.GetPatch(36746).__Gen_Wrap_452(this);
		}
		bool result = false;
		int nTLVNum = m_data.nTLVNum;
		TSOTLVUNIT[] astTLVInfo = m_data.astTLVInfo;
		for (int i = 0; i < nTLVNum; i++)
		{
			if (astTLVInfo[i].nType == 66)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void CheckPlayerDoTing()
	{
		if (WrappersManagerImpl.IsPatched(36745))
		{
			WrappersManagerImpl.GetPatch(36745).__Gen_Wrap_0(this);
			return;
		}
		int nTLVNum = m_data.nTLVNum;
		TSOTLVUNIT[] astTLVInfo = m_data.astTLVInfo;
		for (int i = 0; i < nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = astTLVInfo[i];
			switch ((TLV_TYPE)tSOTLVUNIT.nType)
			{
			case TLV_TYPE.EM_UP_TLV_NOTIFY_TING_KEY:
			{
				TNotifyTing stNotifyTing = tSOTLVUNIT.stValue.stNotifyTing;
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(stNotifyTing.chChair);
				roundModel.GetPlayerGameInfoByViewSeat(viewSeatBySeat).chTingFlag = stNotifyTing.chFlag;
				DetailData detailData = new DetailData();
				detailData.viewSeat = viewSeatBySeat;
				if (CheckHasMingLou() && roundModel.gameType >= GameType.ModuleTuidaohu)
				{
					if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.tingType == TingShowType.Bai)
					{
						detailData.extraFlag = ExtraFlagType.Bai;
					}
					else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.tingType == TingShowType.Tang)
					{
						detailData.extraFlag = ExtraFlagType.Tang;
					}
					else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.tingType == TingShowType.LiangDao)
					{
						detailData.extraFlag = ExtraFlagType.LiangDao;
					}
					else
					{
						detailData.extraFlag = ExtraFlagType.MingLou;
					}
				}
				ProcedureVO procedureVO = new ProcedureVO();
				procedureVO.type = ProcessType.TingProcess;
				procedureVO.data = detailData;
				processModel.PushProcedure(procedureVO);
				if (roundModel.gameSubType == GameSubType.ErRenGuoHuJiaBei)
				{
					base.dispatcher.Dispatch(GameEvent.ShowHandTilesAfterTingTips, viewSeatBySeat);
				}
				AddTingAnimationProcess(detailData);
				Log.Info("AddTingProcess viewSeat:" + detailData.viewSeat, "Game");
				break;
			}
			case TLV_TYPE.EM_UP_TLV_HAND_TILES:
			{
				THandTiles stHandTiles = tSOTLVUNIT.stValue.stHandTiles;
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(stHandTiles.nSeat);
				sbyte[] szTiles = stHandTiles.szTiles;
				int nHandTilesNum = stHandTiles.nHandTilesNum;
				switch (viewSeatBySeat)
				{
				default:
					Log.Error("Illegal viewSeat. playerSeat:" + viewSeatBySeat, "Game");
					break;
				case 1:
				case 2:
				case 3:
				{
					List<int> list = new List<int>();
					for (int j = 0; j < nHandTilesNum; j++)
					{
						list.Add(szTiles[j]);
					}
					UserPaiVO userPaiVO = new UserPaiVO();
					userPaiVO.viewSeat = viewSeatBySeat;
					userPaiVO.mjList.handpaiList = list;
					userPaiVO.bNeedShowTips = !CheckContainFirstTingScore();
					userPaiVO.reason = SetUserPaiReason.GuoHu;
					base.dispatcher.Dispatch(GameEvent.ShowHandTileProcess, userPaiVO);
					break;
				}
				case 0:
					break;
				}
				Log.Info("ShowHandTileAfterTing viewseat is " + viewSeatBySeat + " handtile count " + nHandTilesNum, "Game");
				break;
			}
			case TLV_TYPE.EM_DN_TLV_KEY_HAND_TILES:
			{
				TKeyHandTiles stHandKeyTiles = tSOTLVUNIT.stValue.stHandKeyTiles;
				THandTiles stHandTiles = stHandKeyTiles.stHandTiles;
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(stHandTiles.nSeat);
				sbyte[] szTiles = stHandTiles.szTiles;
				int nHandTilesNum = stHandTiles.nHandTilesNum;
				if (viewSeatBySeat > 3 || viewSeatBySeat < 0)
				{
					Log.Error("Illegal viewSeat. playerSeat:" + viewSeatBySeat, "Game");
				}
				else
				{
					List<int> list2 = new List<int>();
					for (int k = 0; k < nHandTilesNum; k++)
					{
						list2.Add(szTiles[k]);
					}
					UserPaiVO userPaiVO2 = new UserPaiVO();
					userPaiVO2.viewSeat = viewSeatBySeat;
					userPaiVO2.mjList.handpaiList = list2;
					if (stHandKeyTiles.nTingTilesNum != 0)
					{
						userPaiVO2.tingList = new List<int>();
						for (int l = 0; l < stHandKeyTiles.nTingTilesNum && l < stHandKeyTiles.szTingTiles.Length; l++)
						{
							userPaiVO2.tingList.Add(stHandKeyTiles.szTingTiles[l]);
						}
					}
					if (stHandKeyTiles.chType == 0)
					{
						userPaiVO2.reason = SetUserPaiReason.MingLouQuanLiang;
					}
					else if (1 == stHandKeyTiles.chType)
					{
						userPaiVO2.reason = SetUserPaiReason.MingLou;
					}
					else if (2 == stHandKeyTiles.chType)
					{
						((ShowHandTilesDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleShowHandTiles))?.SetTingList(userPaiVO2.viewSeat, userPaiVO2.tingList, stHandKeyTiles.chCanGiveTingTile == 1);
						userPaiVO2.reason = SetUserPaiReason.Liangdao;
					}
					else if (3 == stHandKeyTiles.chType)
					{
						userPaiVO2.reason = SetUserPaiReason.Teammate;
					}
					userPaiVO2.bNeedShowTips = !CheckContainFirstTingScore();
					base.dispatcher.Dispatch(GameEvent.ShowHandTileProcess, userPaiVO2);
				}
				Log.Info("ShowHandTileAfterTing viewseat is " + viewSeatBySeat + " handtile count " + nHandTilesNum, "Game");
				break;
			}
			case TLV_TYPE.EM_UP_TLV_PASS_TIMES:
			{
				TPassTimesInfo stPassTimesInfo = tSOTLVUNIT.stValue.stPassTimesInfo;
				GuoHuVO guoHuVO = new GuoHuVO();
				guoHuVO.passTimes = stPassTimesInfo.chTimes;
				guoHuVO.passMul = stPassTimesInfo.nPassMul;
				guoHuVO.maxPassTimes = stPassTimesInfo.chMaxTimes;
				guoHuVO.viewSeat = roundModel.GetViewSeatBySeat(stPassTimesInfo.chSeatId);
				if (roundModel.GetPlayerGameInfoByViewSeat(guoHuVO.viewSeat).guoHuVO.passTimes != guoHuVO.passTimes)
				{
					base.dispatcher.Dispatch(GameEvent.ShowGuoHuInfo, guoHuVO);
				}
				roundModel.GetPlayerGameInfoByViewSeat(guoHuVO.viewSeat).guoHuVO = guoHuVO;
				Log.Info("RevcGiveNotify guohuVO passTimes " + guoHuVO.passTimes + " passmul " + guoHuVO.passMul + " maxTimes " + guoHuVO.maxPassTimes + " viewSeat " + guoHuVO.viewSeat, "Game");
				break;
			}
			}
		}
	}

	private void HandleGenZhuang()
	{
		if (WrappersManagerImpl.IsPatched(36738))
		{
			WrappersManagerImpl.GetPatch(36738).__Gen_Wrap_0(this);
			return;
		}
		HandleDiscard();
		m_nextToDoHelper.AddWindRainProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, UserActionType.GenZhuang, globalModel);
	}

	private void HandleDiscard()
	{
		if (WrappersManagerImpl.IsPatched(36730))
		{
			WrappersManagerImpl.GetPatch(36730).__Gen_Wrap_0(this);
			return;
		}
		DropVO dropVO = new DropVO();
		dropVO.isFromSvr = true;
		dropVO.viewSeat = roundModel.GetViewSeatBySeat(m_data.chChair);
		dropVO.mjValue = m_data.chTile;
		dropVO.removedMjValue = m_data.chTile;
		dropVO.actionType = UserActionType.Drop;
		dropVO.isObstate = PlayerDataMgr.GetInstance().isObState;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.DropProcess;
		procedureVO.data = dropVO;
		procedureVO.viewSeat = dropVO.viewSeat;
		processModel.PushProcedure(procedureVO);
		if (dropVO.viewSeat == 0)
		{
			base.dispatcher.Dispatch(GameEvent.CleanDiscardTip, -1);
		}
	}

	private bool CheckContainFirstTingScore()
	{
		if (WrappersManagerImpl.IsPatched(36748))
		{
			return WrappersManagerImpl.GetPatch(36748).__Gen_Wrap_452(this);
		}
		bool result = false;
		if (roundModel.selfSeat != m_data.chChair)
		{
			return result;
		}
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType != 74)
			{
				continue;
			}
			TGradeScoreChgInfo[] astGradeScoreChgInfo = tSOTLVUNIT.stValue.stGradeScoreInfo.astGradeScoreChgInfo;
			for (int j = 0; j < astGradeScoreChgInfo.Length; j++)
			{
				TGradeScoreChgInfoDetail[] astGradeScoreChgInfoDetail = astGradeScoreChgInfo[j].astGradeScoreChgInfoDetail;
				for (int k = 0; k < astGradeScoreChgInfoDetail.Length; k++)
				{
					if (astGradeScoreChgInfoDetail[k].nType == 6)
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	private void HandleFirstTingScore()
	{
		if (WrappersManagerImpl.IsPatched(36753))
		{
			WrappersManagerImpl.GetPatch(36753).__Gen_Wrap_0(this);
		}
		else
		{
			if (roundModel.selfSeat != m_data.chChair)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < m_data.nTLVNum; i++)
			{
				TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
				if (tSOTLVUNIT.nType != 74)
				{
					continue;
				}
				TGradeScoreChgInfo[] astGradeScoreChgInfo = tSOTLVUNIT.stValue.stGradeScoreInfo.astGradeScoreChgInfo;
				for (int j = 0; j < astGradeScoreChgInfo.Length; j++)
				{
					TGradeScoreChgInfoDetail[] astGradeScoreChgInfoDetail = astGradeScoreChgInfo[j].astGradeScoreChgInfoDetail;
					foreach (TGradeScoreChgInfoDetail tGradeScoreChgInfoDetail in astGradeScoreChgInfoDetail)
					{
						if (tGradeScoreChgInfoDetail.nType == 6)
						{
							flag = true;
							break;
						}
						if (tGradeScoreChgInfoDetail.nType == 18)
						{
							flag = true;
							flag2 = true;
							break;
						}
					}
				}
			}
			if (flag)
			{
				m_nextToDoHelper.AddWindRainProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, flag2 ? UserActionType.FirstTingSkill : UserActionType.FirstTingScore, globalModel);
			}
		}
	}

	private void HandleEvenTileAddScore()
	{
		if (WrappersManagerImpl.IsPatched(36754))
		{
			WrappersManagerImpl.GetPatch(36754).__Gen_Wrap_0(this);
			return;
		}
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType == 112)
			{
				m_nextToDoHelper.AddEvenTileAddScoreProcess(tSOTLVUNIT.stValue.stAvatarSkillInfo);
				break;
			}
		}
	}

	private void HandleZimoRankToolAddScore()
	{
		if (WrappersManagerImpl.IsPatched(36736))
		{
			WrappersManagerImpl.GetPatch(36736).__Gen_Wrap_0(this);
			return;
		}
		bool flag = false;
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType == 74)
			{
				TGradeScoreInfo stGradeScoreInfo = tSOTLVUNIT.stValue.stGradeScoreInfo;
				long num = 0L;
				num = InGameRankGradeUtil.GetChangeScore(roundModel.selfSeat, stGradeScoreInfo, SO_GRADE_SCORE_CHG_TYPE.SO_GRADE_SCORE_RANKITEM_ZIMO_ADD);
				if (num != 0L)
				{
					Log.Info("HandleZimoRankToolAddScore changeValue is: " + num + " roundModel.selfSeat is: " + roundModel.selfSeat, "RankTools");
					flag = true;
					break;
				}
			}
		}
		Log.Info("HandleZimoRankToolAddScore isContainZimoRankToolAddScore is: " + flag, "RankTools");
		if (flag)
		{
			m_nextToDoHelper.AddWindRainProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, UserActionType.ZiMoRankToolsScore, globalModel);
		}
	}

	private void AddTingAnimationProcess(DetailData data)
	{
		if (WrappersManagerImpl.IsPatched(36747))
		{
			WrappersManagerImpl.GetPatch(36747).__Gen_Wrap_1(this, data);
			return;
		}
		VPTimer.CancelAll("AddTingAnimation");
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.TingAnimationProcess;
		procedureVO.data = data;
		processModel.PushProcedure(procedureVO);
		Log.Info("AddTingAnimation AddTingProcess", "Game");
	}

	private void HandleDiscardSpecial()
	{
		if (WrappersManagerImpl.IsPatched(36737))
		{
			WrappersManagerImpl.GetPatch(36737).__Gen_Wrap_0(this);
			return;
		}
		DropVO dropVO = new DropVO();
		dropVO.isFromSvr = true;
		dropVO.viewSeat = roundModel.GetViewSeatBySeat(m_data.chChair);
		dropVO.mjValue = m_data.chTile;
		dropVO.removedMjValue = m_data.chTile;
		dropVO.actionType = UserActionType.DropSpecial;
		dropVO.isObstate = PlayerDataMgr.GetInstance().isObState;
		ActionRecordVO actionRecordVO = new ActionRecordVO();
		actionRecordVO.viewSeat = dropVO.viewSeat;
		actionRecordVO.type = UserActionType.DropSpecial;
		actionRecordVO.mjValue = m_data.chTile;
		actionRecordVO.targetViewSeat = 0;
		roundModel.AddActionRecord(actionRecordVO);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.DropProcess;
		procedureVO.data = dropVO;
		procedureVO.viewSeat = dropVO.viewSeat;
		processModel.PushProcedure(procedureVO);
		m_nextToDoHelper.AddWindRainProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, UserActionType.DropSpecial, globalModel);
		roundModel.GetTilesByViewSeat(dropVO.viewSeat).AddSpecialGangTile(m_data.chTile);
	}

	private void HandleGang(UserActionType type, bool isGangYao)
	{
		if (WrappersManagerImpl.IsPatched(36731))
		{
			WrappersManagerImpl.GetPatch(36731).__Gen_Wrap_3510(this, type, isGangYao);
			return;
		}
		GangVO gangVO = new GangVO();
		gangVO.type = type;
		gangVO.viewSeat = roundModel.GetViewSeatBySeat(m_data.chChair);
		gangVO.mjValue = m_data.chTile;
		gangVO.targetViewSeat = MJDef.INVALID_VIEW_SEAT;
		gangVO.isGangYao = isGangYao;
		gangVO.isGangshanggang = BalanceCommonUtil.IsGang2Gang(m_data.astTLVInfo, m_data.nTLVNum);
		InGameUtil.SetGiveGangInfo(gangVO.ExtraGangInfo, m_data.astTLVInfo, m_data.nTLVNum);
		switch (type)
		{
		case UserActionType.AnGang:
		{
			ProcedureVO procedureVO2 = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.AnGangProcess, gangVO, gangVO.viewSeat);
			processModel.PushProcedure(procedureVO2);
			break;
		}
		case UserActionType.BuGang:
		{
			ProcedureVO procedureVO = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.BuGangProcess, gangVO, gangVO.viewSeat);
			processModel.PushProcedure(procedureVO);
			break;
		}
		}
		bool hasWindOrRain = m_nextToDoHelper.AddWindRainProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, gangVO.type, globalModel);
		gangVO.hasWindOrRain = hasWindOrRain;
		if (roundModel.gameType == GameType.GuangDong || (roundModel.gameType >= GameType.TuiDaoHu && roundModel.gameType <= GameType.HuNanZhuanZhuanZhang))
		{
			gangVO.hasWindOrRain = false;
		}
	}

	private void HandleHu()
	{
		if (WrappersManagerImpl.IsPatched(36732))
		{
			WrappersManagerImpl.GetPatch(36732).__Gen_Wrap_0(this);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(m_data.chChair);
		base.dispatcher.Dispatch(GameEvent.HideCPGUI, viewSeatBySeat);
		HuVO huVO = new HuVO();
		huVO.type = UserActionType.Hu;
		huVO.viewSeats.Add(viewSeatBySeat);
		huVO.mjValue = m_data.chTile;
		huVO.targetViewSeat = MJDef.INVALID_VIEW_SEAT;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			TWinTileTypeInfoV winTileTypeInfoVFromTLV = BalanceCommonUtil.GetWinTileTypeInfoVFromTLV(m_data.astTLVInfo, m_data.nTLVNum);
			huVO.HuAniData = new HuAniData();
			if (winTileTypeInfoVFromTLV != null)
			{
				GameUtil.CalculateFinalFanViewSeatList(roundModel, base.dispatcher, winTileTypeInfoVFromTLV, huVO);
			}
			else
			{
				Log.Error("Hu Without WinInfo", "Game");
			}
			for (int i = 0; i < winTileTypeInfoVFromTLV.astWinTileType.Length; i++)
			{
				if (roundModel.GetViewSeatBySeat(winTileTypeInfoVFromTLV.astWinTileType[i].chSeat) == huVO.viewSeats[0])
				{
					huVO.multiplier = GameUtil.FormatFengRandBeiShu(winTileTypeInfoVFromTLV.astWinTileType[i].fPoFengRandBeiShu);
					huVO.poFengCalType = (PoFengBeiShuCalcType)winTileTypeInfoVFromTLV.astWinTileType[i].iPoFengBeiShuCalcType;
					huVO.QiYuMultiply = GameUtil.ExtractQiYuMultiply(winTileTypeInfoVFromTLV.astWinTileType[i]);
					if (GMUtil.OpenRedPacketDebugMode)
					{
						huVO.multiplier = 1f * (float)Mathf.CeilToInt(Random.Range(1f, 2.9f) * 10f) / 10f;
						huVO.QiYuMultiply = 1f * (float)Mathf.CeilToInt(Random.Range(0.5f, 1.5f) * 10f) / 10f;
					}
				}
			}
			roundModel.BalanceConfig.GetLevelFanConfig(roundModel.gameType);
			if (roundModel.topHuFengDingBeiCount > 0)
			{
				for (int j = 0; j < winTileTypeInfoVFromTLV.astWinTileType.Length; j++)
				{
					if (roundModel.GetViewSeatBySeat(winTileTypeInfoVFromTLV.astWinTileType[j].chSeat) == huVO.viewSeats[0])
					{
						huVO.fanHuKind = GameUtil.GetFanHuKindForCompareTile(winTileTypeInfoVFromTLV.astWinTileType[j]);
						huVO.isFengDing = GameUtil.GetFengDingStateForCompareTile(winTileTypeInfoVFromTLV.astWinTileType[j]);
						huVO.HuAniData.totalFanCount = winTileTypeInfoVFromTLV.astWinTileType[j].iTotalWinFanCount;
						ParseAdventureAbilityInfo(winTileTypeInfoVFromTLV.astWinTileType[j], huVO);
						break;
					}
				}
			}
			else
			{
				TWinFanInfo winTilesFromTLV = BalanceCommonUtil.GetWinTilesFromTLV(m_data.astTLVInfo, m_data.nTLVNum);
				for (int k = 0; k < winTilesFromTLV.astWinTileType.Length; k++)
				{
					TWinTileType tWinTileType = winTilesFromTLV.astWinTileType[k];
					if (roundModel.GetViewSeatBySeat(tWinTileType.chSeat) != huVO.viewSeats[0])
					{
						continue;
					}
					for (int l = 0; l < tWinTileType.astFanInfo.Length; l++)
					{
						if (GameUtil.GetFanHuKind(tWinTileType.astFanInfo, roundModel) == FanHuKind.Hard)
						{
							huVO.fanHuKind = CompareTileHuType.Hard;
							break;
						}
					}
				}
			}
		}
		else
		{
			TWinFanInfo winTilesFromTLV2 = BalanceCommonUtil.GetWinTilesFromTLV(m_data.astTLVInfo, m_data.nTLVNum);
			TWinTileTypeInfoV winTileTypeInfoVFromTLV2 = BalanceCommonUtil.GetWinTileTypeInfoVFromTLV(m_data.astTLVInfo, m_data.nTLVNum);
			huVO.HuAniData = SingletonBase<FanAniHelper>.GetInstance().GetAniData(roundModel, winTileTypeInfoVFromTLV2, winTilesFromTLV2);
			huVO.HuAniData.HuKind = BalanceCommonUtil.GetHuKindFromWinTilesInTLV(m_data.astTLVInfo, m_data.nTLVNum, roundModel, (huVO.viewSeats.Count >= 1) ? huVO.viewSeats[0] : 0);
		}
		if (roundModel.gameType == GameType.XuanLiu)
		{
			huVO.winCount = m_nextToDoHelper.GetXueLiuWinValue(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, huVO.type);
		}
		else
		{
			huVO.winCount = 0L;
		}
		ProcedureVO procedureVO = ProcedureVOBuilder.BuildHuProcedureVO(huVO);
		processModel.PushProcedure(procedureVO);
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw && huVO.multiplier != 0f)
		{
			CompareTileShowHuFanVO compareTileShowHuFanVO = new CompareTileShowHuFanVO();
			compareTileShowHuFanVO.viewSeat = viewSeatBySeat;
			compareTileShowHuFanVO.visible = true;
			compareTileShowHuFanVO.compareHuType = huVO.fanHuKind;
			compareTileShowHuFanVO.isFengDing = huVO.isFengDing;
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleCompareTile) is CompareTileDataModel compareTileDataModel)
			{
				compareTileShowHuFanVO.inQuickStage = compareTileDataModel.isInQuickPickRound;
			}
			compareTileShowHuFanVO.poFengMultiply = huVO.multiplier;
			compareTileShowHuFanVO.QiYuMultiply = huVO.QiYuMultiply;
			compareTileShowHuFanVO.poFengCalType = huVO.poFengCalType;
			compareTileShowHuFanVO.fanNum = huVO.HuAniData.totalFanCount;
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleMultiTile) is MultiTileModel multiTileModel)
			{
				compareTileShowHuFanVO.inMultiTile = multiTileModel.IsMultiTileRedraw;
			}
			List<CompareTileShowHuFanVO> list = new List<CompareTileShowHuFanVO> { compareTileShowHuFanVO };
			base.dispatcher.Dispatch(GameEvent.PushOpenRedPacketProcess, list);
		}
		if (huVO.viewSeats[0] == 0)
		{
			base.dispatcher.Dispatch(GameEvent.HideTingTips);
		}
		if (GameUtil.IsNeedWinRainProcess(roundModel))
		{
			m_nextToDoHelper.AddWindRainProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, huVO.type, globalModel, 1.4f);
		}
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.showPlayerHuTimes && roundModel.GetViewSeatBySeat(m_data.chChair) == 0)
		{
			m_nextToDoHelper.AddShowHuTimesProcess(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel);
		}
	}

	private void ParseAdventureAbilityInfo(TWinTileTypeV winTileType, HuVO huData)
	{
		if (WrappersManagerImpl.IsPatched(36733))
		{
			WrappersManagerImpl.GetPatch(36733).__Gen_Wrap_12(this, winTileType, huData);
		}
		else
		{
			if (winTileType == null || winTileType.astFanInfo == null)
			{
				return;
			}
			huData.adventureAbilityInfos.Clear();
			for (int i = 0; i < winTileType.nFanCount && i < winTileType.astFanInfo.Length; i++)
			{
				TFanV tFanV = winTileType.astFanInfo[i];
				int iFanIdx = tFanV.iFanIdx;
				if (iFanIdx == 295 || iFanIdx == 296 || iFanIdx == 297)
				{
					AdventureAbilityInfo adventureAbilityInfo = new AdventureAbilityInfo
					{
						abilityType = (AdventureAbilityType)iFanIdx,
						fanValue = tFanV.iFan,
						fanNumDiv = tFanV.iFanNumDiv
					};
					huData.adventureAbilityInfos.Add(adventureAbilityInfo);
					Log.Info($"ParseAdventureAbilityInfo: type={adventureAbilityInfo.abilityType}, fan={adventureAbilityInfo.fanValue}, fanNumDiv={adventureAbilityInfo.fanNumDiv}, multiplier={adventureAbilityInfo.Multiplier}", "Game");
				}
			}
		}
	}

	private void HandleRestrictInfo()
	{
		if (WrappersManagerImpl.IsPatched(36750))
		{
			WrappersManagerImpl.GetPatch(36750).__Gen_Wrap_0(this);
			return;
		}
		int nTLVNum = m_data.nTLVNum;
		int seatByViewSeat = roundModel.GetSeatByViewSeat(0);
		for (int i = 0; i < nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType == 44)
			{
				int[] restrictFlag = tSOTLVUNIT.stValue.stRestrictInfo.RestrictFlag;
				Log.Info("RevcGiveNotiyCommand self restrictInfo is:" + restrictFlag[seatByViewSeat], "Game");
				if (restrictFlag[seatByViewSeat] != 0)
				{
					base.dispatcher.Dispatch(GameEvent.HuRestrictMsgTips, restrictFlag[seatByViewSeat]);
				}
			}
		}
	}

	private void HandleMissHuInfo()
	{
		if (WrappersManagerImpl.IsPatched(36751))
		{
			WrappersManagerImpl.GetPatch(36751).__Gen_Wrap_0(this);
			return;
		}
		int nTLVNum = m_data.nTLVNum;
		for (int i = 0; i < nTLVNum; i++)
		{
			if (m_data.astTLVInfo[i].nType == 47)
			{
				base.dispatcher.Dispatch(GameEvent.ShowMissHuInfo);
				break;
			}
		}
	}

	private void HandleFakeHu()
	{
		if (WrappersManagerImpl.IsPatched(36739))
		{
			WrappersManagerImpl.GetPatch(36739).__Gen_Wrap_0(this);
		}
		else
		{
			if (m_data.nTLVNum <= 0)
			{
				return;
			}
			for (int i = 0; i < m_data.nTLVNum; i++)
			{
				TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
				if (tSOTLVUNIT.nType == 65)
				{
					HandleFakeHuTLV(tSOTLVUNIT.stValue.stFakeHuInfoList);
				}
			}
		}
	}

	private void HandleFakeHuTLV(TFakeHuInfoList fakeHuInfo)
	{
		if (WrappersManagerImpl.IsPatched(36740))
		{
			WrappersManagerImpl.GetPatch(36740).__Gen_Wrap_1(this, fakeHuInfo);
			return;
		}
		Log.Info("RevcGiveNotifyCommand HandleFakeHu start!", "Game");
		roundModel.SetRoundTime(10, 15);
		for (int i = 0; i < fakeHuInfo.nFakeHuPlayerNum; i++)
		{
			GuoBiaoFakeHuVO guoBiaoFakeHuVO = new GuoBiaoFakeHuVO();
			guoBiaoFakeHuVO.FakeHuList = new List<int>();
			guoBiaoFakeHuVO.ViewSeatID = roundModel.GetViewSeatBySeat(fakeHuInfo.astFakeHuInfo[i].nSeatID);
			guoBiaoFakeHuVO.FakeHuTilesNum = fakeHuInfo.astFakeHuInfo[i].nFakeHuTilesNum;
			guoBiaoFakeHuVO.FakeHuMJValue = fakeHuInfo.astFakeHuInfo[i].chTile;
			guoBiaoFakeHuVO.IsNeedAndToHandTiles = false;
			guoBiaoFakeHuVO.IsReplay = false;
			for (int j = 0; j < fakeHuInfo.astFakeHuInfo[i].nFakeHuTilesNum; j++)
			{
				guoBiaoFakeHuVO.FakeHuList.Add(fakeHuInfo.astFakeHuInfo[i].szFakeHuTiles[j]);
			}
			if (guoBiaoFakeHuVO.FakeHuList != null && guoBiaoFakeHuVO.FakeHuList.Count != 0)
			{
				ProcedureVO procedureVO = new ProcedureVO();
				procedureVO.type = ProcessType.FakeHuPushMJProcess;
				procedureVO.data = guoBiaoFakeHuVO;
				procedureVO.maxTime = 15;
				processModel.PushProcedure(procedureVO);
				Log.Info("Add FakeHuPushMJProcess i: " + i, "Game");
				if (guoBiaoFakeHuVO.ViewSeatID != 0)
				{
					SetOtherPlayerHandMJ(guoBiaoFakeHuVO);
				}
			}
		}
	}

	private void SetOtherPlayerHandMJ(GuoBiaoFakeHuVO data)
	{
		if (WrappersManagerImpl.IsPatched(36741))
		{
			WrappersManagerImpl.GetPatch(36741).__Gen_Wrap_1(this, data);
			return;
		}
		UserPaiVO userPaiVO = new UserPaiVO();
		userPaiVO.viewSeat = data.ViewSeatID;
		userPaiVO.mjList.handpaiList = data.FakeHuList;
		userPaiVO.reason = SetUserPaiReason.None;
		AddSetUserPai(userPaiVO);
	}

	private void AddSetUserPai(UserPaiVO payLoad)
	{
		if (WrappersManagerImpl.IsPatched(36742))
		{
			WrappersManagerImpl.GetPatch(36742).__Gen_Wrap_1(this, payLoad);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.SetUserPai;
		procedureVO.data = payLoad;
		procedureVO.maxTime = 5;
		processModel.PushProcedure(procedureVO);
	}

	public void _003C_003EiFixBaseProxy_Execute()
	{
		base.Execute();
	}
}
