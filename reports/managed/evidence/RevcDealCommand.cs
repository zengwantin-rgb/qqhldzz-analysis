using System;
using System.Collections.Generic;
using System.Text;
using GameModule;
using HappyMahjong.CSReplay;
using HappyMahjong.Common;
using HappyMahjong.Game.DeskPro;
using HappyMahjong.PlayerRecord;
using HappyMahjong.ShopAndBag;
using HappyMahjong.Tutorial;
using HuanLeGameSo;
using IFix;
using UnityEngine;
using strange.extensions.command.impl;
using tsf4g_tdr_csharp;

namespace HappyMahjong.Game;

public class RevcDealCommand : EventCommand
{
	protected NextToDoHelper m_nextToDoHelper;

	protected CNotifyDeal m_data;

	protected int m_totalFlowerNum;

	protected List<int>[] m_allPlayerMJs;

	protected int m_the14thMJ;

	protected List<int>[] m_flowerMJs;

	protected List<int>[] m_redrawMJs;

	protected TFlowerInfo[] m_flowerInfos;

	private bool isDealer14 = true;

	[Inject]
	public IGlobalModel globalModel { get; set; }

	[Inject]
	public IProcessModel processModel { get; set; }

	[Inject]
	public IRoundModel roundModel { get; set; }

	[Inject]
	public IShopModel shopModel { get; set; }

	[Inject]
	public ICommonRule iComRule { get; set; }

	public override void Execute()
	{
		if (WrappersManagerImpl.IsPatched(36509))
		{
			WrappersManagerImpl.GetPatch(36509).__Gen_Wrap_0(this);
			return;
		}
		if (SingletonBase<MJGameRulesCenter>.GetInstance().CheckHasModule(MJGameModuleType.MJGameModuleChooseTile) || SingletonBase<MJGameRulesCenter>.GetInstance().CheckHasModule(MJGameModuleType.MJGameModuleChooseTileGet2Give))
		{
			isDealer14 = false;
		}
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.DaPiaoQiangZhuang)
		{
			isDealer14 = false;
			base.dispatcher.Dispatch(GameEvent.ShowDeskDirections, false);
		}
		m_nextToDoHelper = new NextToDoHelper(base.dispatcher, roundModel);
		m_data = (CNotifyDeal)base.evt.data;
		m_nextToDoHelper.CheckAbnormalMJNum(m_data.nTLVNum, m_data.astTLVInfo, roundModel);
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			isDealer14 = false;
		}
		if (!GameReplayTutorialController.InTutorial)
		{
			ChangeDeskByShare();
		}
		m_totalFlowerNum = 0;
		roundModel.PandoraBoxInfo = null;
		m_flowerInfos = new TFlowerInfo[4];
		m_flowerMJs = new List<int>[4];
		m_redrawMJs = new List<int>[4];
		GetAllPlayerMJ();
		if (Util.IsTeamBattlePlaying((int)globalModel.enterGameInfo.gameType))
		{
			List<int> teammateHandTiles = GetTeammateHandTiles(m_data.nTLVNum, m_data.astTLVInfo);
			m_allPlayerMJs[roundModel.teammateViewSeat] = teammateHandTiles;
		}
		if (Util.IsTeamBattlePlaying((int)globalModel.enterGameInfo.gameType))
		{
			base.dispatcher.Dispatch(GameEvent.InitTeamVoice);
		}
		if (SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomPrivateRoom)
		{
			base.dispatcher.Dispatch(GameEvent.InitPrivateRoomVoice);
		}
		GetFlowerInfo();
		CheckFlower();
		EventReporter.ReportEnd(EventReporter.EEventName.HighDeviceLoadMainSceneTime);
		if (globalModel.enterGameInfo.matchType == MatchType.None && LocalGameServer.GetInstance() == null)
		{
			Log.Info("RevcDealCommand AddOpeningAnimatioinProcess", "Game");
			AddOpeningAnimatioinProcess();
		}
		PlayerGameInfoVO playerGameInfoByViewSeat = roundModel.GetPlayerGameInfoByViewSeat(0);
		if (playerGameInfoByViewSeat != null)
		{
			Log.Info("Game", "CheckTingNow, ->RecvDealMsgCommand playerGameInfo.isTianTing:{0}", playerGameInfoByViewSeat.isTianTing);
			playerGameInfoByViewSeat.isTianTing = false;
		}
		GetRoundID();
		HandleTLVBeforeDealMJ();
		AddDiesProcess();
		AddDealProcess();
		HandleTLVBefor();
		DoBuHua();
		AddDealerLastRedraw();
		if (CSReplayRecordService.sIsReplaying())
		{
			if (CSReplayRecordService.NeedPushOtherMjInReplaying())
			{
				ProcedureVO procedureVO = new ProcedureVO();
				procedureVO.type = ProcessType.DealProcessReplayShowMJValue;
				if (roundModel.isDapiaoRoom)
				{
					processModel.AddProcedureDelay(procedureVO);
				}
				else
				{
					processModel.PushProcedure(procedureVO);
				}
			}
			HandleDonnerInfo();
		}
		if (Util.IsTeamBattlePlaying((int)globalModel.enterGameInfo.gameType))
		{
			HandleTeammateHandtile();
		}
		HandleNextToDo();
		base.dispatcher.Dispatch(GameEvent.ExecuteProcessCmd);
		base.dispatcher.Dispatch(GameEvent.ShowRoomInfo);
		if (!GameUtil.IsChangeCardRoom(globalModel.enterGameInfo.roomInfo) && globalModel.enterGameInfo.roomInfo.HasRecommendFan == 1)
		{
			base.dispatcher.Dispatch(RecommendFanEvent.PreRequestRecommendFan, false);
		}
		if (GameUtil.IsAutoPlayEnabled(globalModel.enterGameInfo.roomInfo.sceneId))
		{
			base.dispatcher.Dispatch(GameEvent.ShowLightIcon, true);
		}
		if (GameUtil.IsAutoNotHuEnabled(globalModel.enterGameInfo.roomInfo.sceneId))
		{
			base.dispatcher.Dispatch(GameEvent.SendNotHuFlagMsg);
		}
		Log.Info($"RevcDealCommand CSReplayRecordService.sIsOnlyReplayBalance() is: {CSReplayRecordService.sIsOnlyReplayBalance()}", "Game");
		if (CSReplayRecordService.sIsOnlyReplayBalance())
		{
			SingletonBase<CSReplayRecordService>.GetInstance().JumpBalanceReplay();
		}
	}

	private void ChangeDeskByShare()
	{
		if (WrappersManagerImpl.IsPatched(36511))
		{
			WrappersManagerImpl.GetPatch(36511).__Gen_Wrap_0(this);
			return;
		}
		if (SingletonBase<DynamicConfig>.GetInstance().GetInt("DeskPro", "Off") == 1)
		{
			Log.Info("DeskPro is Off by Config in ChangeDeskByShare", "Game");
			base.dispatcher.Dispatch(TableViewEvent.ShowDepthOfView);
			return;
		}
		Log.Info("ChangeDeskByShare", "Game");
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType != 116)
			{
				continue;
			}
			ShowAllDeskInfoVO showAllDeskInfoVO = VOBuilder.BuildShowAllDeskInfoVO(tSOTLVUNIT.stValue.stShareTableInfo, roundModel, SingletonBase<ShopDataHelper>.GetInstance().shopModel);
			base.dispatcher.Dispatch(DeskProEvent.CacheDeskProData, showAllDeskInfoVO);
			bool flag = false;
			DeskInfoItem deskInfoItem = default(DeskInfoItem);
			foreach (DeskInfoItem deskInfoItem2 in showAllDeskInfoVO.DeskInfoItems)
			{
				if (deskInfoItem2.IsCanShareItem && deskInfoItem2.CanUseByOthers)
				{
					flag = true;
					deskInfoItem = deskInfoItem2;
					break;
				}
			}
			base.dispatcher.Dispatch(GameEvent.DeskProShowInMenu, flag);
			base.dispatcher.Dispatch(DeskProEvent.ShowDeskProBtnTips);
			base.dispatcher.Dispatch(DeskProEvent.ChangeDesk);
			if (flag)
			{
				base.dispatcher.Dispatch(DeskProEvent.ChangeDeskProOwnerIcon, deskInfoItem.OwnerViewSeatID);
			}
			base.dispatcher.Dispatch(DeskProEvent.ReqLuckyDeskProGoodsId);
			return;
		}
		base.dispatcher.Dispatch(TableViewEvent.ShowDepthOfView);
	}

	protected void HandleTLVBefor()
	{
		if (WrappersManagerImpl.IsPatched(36543))
		{
			WrappersManagerImpl.GetPatch(36543).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("Deal, HandleTLVBefor", "Game");
		int num = 0;
		int nTLVNum = m_data.nTLVNum;
		for (num = 0; num < nTLVNum; num++)
		{
			if (m_data.astTLVInfo[num].nType == 57)
			{
				roundModel.isDapiaoRoom = true;
			}
		}
	}

	protected void HandleTLVBeforeDealMJ()
	{
		if (WrappersManagerImpl.IsPatched(36531))
		{
			WrappersManagerImpl.GetPatch(36531).__Gen_Wrap_0(this);
			return;
		}
		AddDeskProProcess();
		Log.Info("Deal, HandleTLVBeforeDealMJ", "Game");
		int num = 0;
		int nTLVNum = m_data.nTLVNum;
		for (num = 0; num < nTLVNum; num++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[num];
			switch ((TLV_TYPE)tSOTLVUNIT.nType)
			{
			case TLV_TYPE.EM_DN_TLV_RANK_ITEM:
				HandleRankToolsItems(tSOTLVUNIT.stValue.stSoRankItemInfo);
				break;
			case TLV_TYPE.EM_DN_TLV_CHANGE_FACE_SKILL:
				AddSuitAdditionProcess(tSOTLVUNIT.stValue.stChangeFaceSkill);
				break;
			case TLV_TYPE.EM_DN_TLV_DRAW_TILE_COLOR_SKILL:
				AddDrawTileColorSkillProcess(tSOTLVUNIT.stValue.stDrawTileColorSkill);
				break;
			case TLV_TYPE.EM_DN_TLV_GRADE_SCORE_INFO:
				if (SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomSectionScore || SectionUtil.GetRoomSectionType(globalModel.enterGameInfo.roomInfo) == RoomSectionType.RoomSectionScoreAndBean)
				{
					TGradeScoreInfo stGradeScoreInfo = tSOTLVUNIT.stValue.stGradeScoreInfo;
					if (stGradeScoreInfo != null)
					{
						PlayerDataMgr.GetInstance().GetSelfInfo().SectionInfo.iCurSectionScore = (int)stGradeScoreInfo.CurScore[roundModel.GetSeatByViewSeat(0)];
						PlayerDataMgr.GetInstance().GetSelfInfo().SectionInfo.iEnterSectionScore = (int)stGradeScoreInfo.EnterScore[roundModel.GetSeatByViewSeat(0)];
						PlayerDataMgr.GetInstance().GetSelfInfo().realEnterSectionScore = (int)stGradeScoreInfo.EnterScore[roundModel.GetSeatByViewSeat(0)];
						Log.Info($"RevcDealCommand HandleTLVBeforeDealMJ curScore is:{PlayerDataMgr.GetInstance().GetSelfInfo().SectionInfo.iCurSectionScore}, enterScore is:{PlayerDataMgr.GetInstance().GetSelfInfo().SectionInfo.iEnterSectionScore}", "Game");
					}
				}
				break;
			case TLV_TYPE.EM_DN_TLV_CARD_WALL_INFO:
				AddCreateWallProcess(tSOTLVUNIT.stValue.stCardWallInfo);
				break;
			case TLV_TYPE.EM_DN_TLV_NEW_SECTION_SCORE_JIE_SUAN:
				roundModel.NewSectionScoreJieSuan = tSOTLVUNIT.stValue.stNewSectionScoreJieSuan;
				base.dispatcher.Dispatch(GameEvent.UpdateNewSectionInfo);
				break;
			case TLV_TYPE.EM_DN_TLV_CHARM_SKILL_INFO:
				AddCharmSkillProcess(tSOTLVUNIT.stValue.stCharmSkillInfo);
				break;
			case TLV_TYPE.EM_DN_TLV_CHARM_SCORE_UPDATE:
				CharmTLVParser.DispatchCharmScoreUpdate(tSOTLVUNIT.stValue.stCharmScoreUpdate, base.dispatcher);
				break;
			}
		}
	}

	private void AddCreateWallProcess(TCardWallInfo wallCardInfo)
	{
		if (WrappersManagerImpl.IsPatched(36539))
		{
			WrappersManagerImpl.GetPatch(36539).__Gen_Wrap_1(this, wallCardInfo);
			return;
		}
		MJWallVO mJWallVO = new MJWallVO();
		mJWallVO.wallType = roundModel.gameType;
		mJWallVO.viewSeatOfZeroSeat = roundModel.GetViewSeatBySeat(0);
		mJWallVO.singleCardWallInfo = new List<TSingleCardWallInfo>(wallCardInfo.nCardWallNum);
		for (int i = 0; i < wallCardInfo.nCardWallNum; i++)
		{
			mJWallVO.singleCardWallInfo.Add(new TSingleCardWallInfo());
		}
		for (int j = 0; j < wallCardInfo.nCardWallNum; j++)
		{
			int viewSeatBySeat = roundModel.GetViewSeatBySeat(j);
			mJWallVO.singleCardWallInfo[viewSeatBySeat].iCurrentTilesNum = wallCardInfo.astSingleCardWallInfo[j].iCurrentTilesNum;
			mJWallVO.singleCardWallInfo[viewSeatBySeat].iHeadIndex = wallCardInfo.astSingleCardWallInfo[j].iHeadIndex;
			mJWallVO.singleCardWallInfo[viewSeatBySeat].iTailIndex = wallCardInfo.astSingleCardWallInfo[j].iTailIndex;
			mJWallVO.singleCardWallInfo[viewSeatBySeat].EmptyIndex = wallCardInfo.astSingleCardWallInfo[j].EmptyIndex;
			mJWallVO.singleCardWallInfo[viewSeatBySeat].nEmptyIndexNum = wallCardInfo.astSingleCardWallInfo[j].nEmptyIndexNum;
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
			{
				mJWallVO.singleCardWallInfo[viewSeatBySeat].iCurrentTilesNum -= MJDef.INITIAL_MJ_COUNT;
			}
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CreateWallProcess;
		procedureVO.data = mJWallVO;
		processModel.PushProcedure(procedureVO);
	}

	private void AddDeskProProcess()
	{
		if (WrappersManagerImpl.IsPatched(36532))
		{
			WrappersManagerImpl.GetPatch(36532).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("Add DeskPro Process in Deal", "Game");
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType != 116)
			{
				continue;
			}
			ShowAllDeskInfoVO showAllDeskInfoVO = VOBuilder.BuildShowAllDeskInfoVO(tSOTLVUNIT.stValue.stShareTableInfo, roundModel, SingletonBase<ShopDataHelper>.GetInstance().shopModel);
			base.dispatcher.Dispatch(DeskProEvent.CacheDeskProData, showAllDeskInfoVO);
			bool flag = false;
			foreach (DeskInfoItem deskInfoItem in showAllDeskInfoVO.DeskInfoItems)
			{
				if (deskInfoItem.IsCanShareItem && deskInfoItem.CanUseByOthers)
				{
					flag = true;
					break;
				}
			}
			base.dispatcher.Dispatch(GameEvent.DeskProShowInMenu, flag);
			base.dispatcher.Dispatch(DeskProEvent.ReqLuckyDeskProGoodsId);
			_ = showAllDeskInfoVO.RecommendDeskViewSeatID;
			_ = -1;
			break;
		}
	}

	protected void AddOpeningAnimatioinProcess()
	{
		if (WrappersManagerImpl.IsPatched(36528))
		{
			WrappersManagerImpl.GetPatch(36528).__Gen_Wrap_0(this);
		}
		else
		{
			if (CSReplayRecordService.sIsOnlyReplayBalance())
			{
				return;
			}
			HappyMahjong.Common.GameType gameType = globalModel.enterGameInfo.gameType;
			List<PlayerDataVO> getPlayerDataVO = globalModel.getPlayerDataVO;
			List<DressInfo> list = new List<DressInfo>();
			List<string> list2 = new List<string>();
			for (int i = 0; i < getPlayerDataVO.Count; i++)
			{
				if (getPlayerDataVO[i] == null)
				{
					list.Add(null);
					list2.Add(null);
					continue;
				}
				AvatarInfo avatarInfo = getPlayerDataVO[i].avatarInfo;
				if (avatarInfo == null)
				{
					list.Add(null);
					list2.Add(null);
					continue;
				}
				DressInfo dressInfo = new DressInfo();
				dressInfo.gender = avatarInfo.gender;
				dressInfo.baseDress = avatarInfo.baseDress;
				dressInfo.itemList = new List<int>();
				for (int j = 0; j < avatarInfo.itemList.Count; j++)
				{
					dressInfo.itemList.Add(avatarInfo.itemList[j]);
				}
				list.Add(dressInfo);
				list2.Add(getPlayerDataVO[i].nickName);
			}
			PlayerDressInfos playerDressInfos = new PlayerDressInfos();
			playerDressInfos.avatarInfos = list;
			playerDressInfos.nameList = list2;
			playerDressInfos.gameType = gameType;
			playerDressInfos.buckViewId = roundModel.GetViewSeatBySeat(m_data.chDealer);
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.OpeningAnimationProcess;
			procedureVO.maxTime += 5;
			procedureVO.data = playerDressInfos;
			processModel.PushProcedure(procedureVO);
		}
	}

	protected void AddDiesProcess()
	{
		if (WrappersManagerImpl.IsPatched(36541))
		{
			WrappersManagerImpl.GetPatch(36541).__Gen_Wrap_0(this);
		}
		else if (!CSReplayRecordService.sIsOnlyReplayBalance())
		{
			Log.Info("m_data.chDealer Seat:" + m_data.chDealer, "Game");
			DiesVO diesVO = new DiesVO();
			diesVO.dealerViewSeat = roundModel.GetViewSeatBySeat(m_data.chDealer);
			diesVO.num1 = m_data.szDice[0];
			diesVO.num2 = m_data.szDice[1];
			roundModel.dealerViewSeat = diesVO.dealerViewSeat;
			diesVO.nRoundNum = m_data.chRoundWind;
			diesVO.nSubRoundNum = m_data.chSubRoundWind;
			diesVO.continueWin = m_data.iContinueWin;
			diesVO.nMenFengNum = roundModel.menFeng;
			Log.Info("vo.nMenFengNum:" + diesVO.nMenFengNum, "Game");
			base.dispatcher.Dispatch(GameEvent.SetMenFeng, diesVO);
			if (!SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall)
			{
				ProcedureVO procedureVO = new ProcedureVO();
				procedureVO.type = ProcessType.RollDiesProcess;
				procedureVO.data = diesVO;
				processModel.PushProcedure(procedureVO);
			}
		}
	}

	protected void AddDealProcess()
	{
		if (WrappersManagerImpl.IsPatched(36542))
		{
			WrappersManagerImpl.GetPatch(36542).__Gen_Wrap_0(this);
		}
		else if (!CSReplayRecordService.sIsOnlyReplayBalance())
		{
			DealMJVO dealMJVO = new DealMJVO();
			dealMJVO.dealerViewSeat = roundModel.GetViewSeatBySeat(m_data.chDealer);
			dealMJVO.allPlayerMJs = m_allPlayerMJs;
			dealMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.DealProcess;
			procedureVO.data = dealMJVO;
			processModel.PushProcedure(procedureVO);
		}
	}

	protected void AddBuHuaDropProcess(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(36550))
		{
			WrappersManagerImpl.GetPatch(36550).__Gen_Wrap_466(this, viewSeat);
		}
		else if (!CSReplayRecordService.sIsOnlyReplayBalance())
		{
			DetailData detailData = new DetailData();
			detailData.viewSeat = viewSeat;
			detailData.mjList = m_flowerMJs[viewSeat];
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.BuHuaDropProcess;
			procedureVO.data = detailData;
			procedureVO.viewSeat = detailData.viewSeat;
			processModel.PushProcedure(procedureVO);
			Log.Info("AddBuHuaDropProcess viewSeat:" + viewSeat + " mjValue:" + detailData.mjValue, "Game");
		}
	}

	protected virtual void GetAllPlayerMJ()
	{
		if (WrappersManagerImpl.IsPatched(36519))
		{
			WrappersManagerImpl.GetPatch(36519).__Gen_Wrap_0(this);
			return;
		}
		m_allPlayerMJs = new List<int>[4];
		if (CSReplayRecordService.sIsReplayFromServer() && CSReplayRecordService.IsObStatus())
		{
			AddPlayerCardInReplay();
		}
		else
		{
			AddPlayerCard();
		}
		if (isDealer14)
		{
			Del14Card();
		}
	}

	private void AddPlayerCardInReplay()
	{
		if (WrappersManagerImpl.IsPatched(36520))
		{
			WrappersManagerImpl.GetPatch(36520).__Gen_Wrap_0(this);
			return;
		}
		THandTiles[] array = new THandTiles[4];
		int nTLVNum = m_data.nTLVNum;
		for (int i = 0; i < nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType == 10)
			{
				THandTiles stHandTiles = tSOTLVUNIT.stValue.stHandTiles;
				array[roundModel.GetViewSeatBySeat(stHandTiles.nSeat)] = stHandTiles;
			}
		}
		for (int j = 0; j < 4; j++)
		{
			if (array[j] != null)
			{
				List<int> list = new List<int>();
				m_allPlayerMJs[j] = list;
				for (int k = 0; k < array[j].szTiles.Length && k < array[j].nHandTilesNum; k++)
				{
					list.Add(array[j].szTiles[k]);
				}
			}
		}
	}

	private void AddPlayerCard()
	{
		if (WrappersManagerImpl.IsPatched(36521))
		{
			WrappersManagerImpl.GetPatch(36521).__Gen_Wrap_0(this);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(m_data.chDealer);
		for (int i = 0; i < 4; i++)
		{
			List<int> list = new List<int>();
			m_allPlayerMJs[i] = list;
			if (i == 0)
			{
				for (int j = 0; j < m_data.nTilesNum; j++)
				{
					list.Add(m_data.szTiles[j]);
				}
				Log.Info("Recv Deal Self mjValue:" + Util.Stringify(list), "Game");
				continue;
			}
			for (int k = 0; k < MJDef.INITIAL_MJ_COUNT; k++)
			{
				list.Add(MJDef.MJ_BACK);
			}
			if (i == viewSeatBySeat && isDealer14)
			{
				list.Add(MJDef.MJ_BACK);
			}
		}
	}

	private void Del14Card()
	{
		if (WrappersManagerImpl.IsPatched(36522))
		{
			WrappersManagerImpl.GetPatch(36522).__Gen_Wrap_0(this);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(m_data.chDealer);
		List<int> list = m_allPlayerMJs[viewSeatBySeat];
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.huaType == HuaType.NanJing)
			{
				if (list[num] < 35)
				{
					m_the14thMJ = list[num];
					roundModel.curRedrawMJValue = m_the14thMJ;
					roundModel.GetPlayerGameInfoByViewSeat(viewSeatBySeat).tiles.curRedrawMjValue = roundModel.curRedrawMJValue;
					list.RemoveAt(num);
					break;
				}
			}
			else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.huaType == HuaType.Normal)
			{
				if (globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.ModuleDragon)
				{
					m_the14thMJ = list[num];
					roundModel.curRedrawMJValue = m_the14thMJ;
					roundModel.GetPlayerGameInfoByViewSeat(viewSeatBySeat).tiles.curRedrawMjValue = roundModel.curRedrawMJValue;
					list.RemoveAt(num);
					break;
				}
				if (list[num] < 41)
				{
					m_the14thMJ = list[num];
					roundModel.curRedrawMJValue = m_the14thMJ;
					roundModel.GetPlayerGameInfoByViewSeat(viewSeatBySeat).tiles.curRedrawMjValue = roundModel.curRedrawMJValue;
					list.RemoveAt(num);
					break;
				}
			}
		}
	}

	protected void GetFlowerInfo()
	{
		if (WrappersManagerImpl.IsPatched(36525))
		{
			WrappersManagerImpl.GetPatch(36525).__Gen_Wrap_0(this);
			return;
		}
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
			if (tSOTLVUNIT.nType == 1)
			{
				TFlowerInfo stFlowerInfo = tSOTLVUNIT.stValue.stFlowerInfo;
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(stFlowerInfo.nSeat);
				if (stFlowerInfo.nTilesFlowerNum > 0)
				{
					m_flowerInfos[viewSeatBySeat] = stFlowerInfo;
					m_totalFlowerNum += stFlowerInfo.nTilesFlowerNum;
					Log.Info("ViewSeat:" + viewSeatBySeat + " FlowerNum:" + stFlowerInfo.nTilesFlowerNum, "Game");
				}
			}
		}
	}

	protected void CheckFlower()
	{
		if (WrappersManagerImpl.IsPatched(36526))
		{
			WrappersManagerImpl.GetPatch(36526).__Gen_Wrap_0(this);
		}
		else if (m_totalFlowerNum > 0)
		{
			for (int i = 0; i < 4; i++)
			{
				InsertFlower(i);
			}
		}
	}

	protected void InsertFlower(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(36527))
		{
			WrappersManagerImpl.GetPatch(36527).__Gen_Wrap_466(this, viewSeat);
			return;
		}
		TFlowerInfo tFlowerInfo = m_flowerInfos[viewSeat];
		if (tFlowerInfo == null || tFlowerInfo.nTilesFlowerNum <= 0)
		{
			return;
		}
		List<int> list = m_allPlayerMJs[viewSeat];
		List<int> list2 = new List<int>();
		m_redrawMJs[viewSeat] = list2;
		List<int> list3 = new List<int>();
		m_flowerMJs[viewSeat] = list3;
		int num = 0;
		for (num = 0; num < tFlowerInfo.nTilesFlowerNum; num++)
		{
			int item = tFlowerInfo.szFlower[num];
			list3.Add(item);
			if (list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count - 1);
				int item2 = list[index];
				list2.Add(item2);
				list.RemoveAt(index);
			}
		}
		for (num = 0; num < list3.Count; num++)
		{
			if (num < 13)
			{
				int item = list3[num];
				int index = UnityEngine.Random.Range(0, list.Count - 1);
				list.Insert(index, item);
			}
			else
			{
				list.Add(list3[num]);
			}
		}
	}

	protected void DoBuHua()
	{
		if (WrappersManagerImpl.IsPatched(36544))
		{
			WrappersManagerImpl.GetPatch(36544).__Gen_Wrap_0(this);
		}
		else if (m_totalFlowerNum > 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.MultiBuHuaProcess;
			procedureVO.data = m_flowerMJs;
			if (roundModel.isDapiaoRoom)
			{
				processModel.AddProcedureDelay(procedureVO);
			}
			else
			{
				processModel.PushProcedure(procedureVO);
			}
			Log.Info("AddMultiBuHuaDropProcess", "Game");
			procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.MultiRedrawProcess;
			procedureVO.data = m_redrawMJs;
			if (roundModel.isDapiaoRoom)
			{
				processModel.AddProcedureDelay(procedureVO);
			}
			else
			{
				processModel.PushProcedure(procedureVO);
			}
			Log.Info("AddMultiRedrawDropProcess", "Game");
		}
	}

	protected void AddDealerLastRedraw()
	{
		if (WrappersManagerImpl.IsPatched(36545))
		{
			WrappersManagerImpl.GetPatch(36545).__Gen_Wrap_0(this);
		}
		else if (isDealer14)
		{
			RedrawMJVO redrawMJVO = new RedrawMJVO();
			redrawMJVO.viewSeat = roundModel.GetViewSeatBySeat(m_data.chDealer);
			redrawMJVO.isNormalOrder = true;
			redrawMJVO.needXMargin = true;
			redrawMJVO.showAnimation = true;
			redrawMJVO.sortAfterRedraw = false;
			redrawMJVO.isFlowerRedraw = false;
			redrawMJVO.isDealerRedraw = true;
			redrawMJVO.mjList = new List<int> { m_the14thMJ };
			redrawMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
			ProcedureVO procedureVO = ProcedureVOBuilder.BuildInHandProcedureVO(ProcessType.RedrawProcess, redrawMJVO, redrawMJVO.viewSeat);
			processModel.PushProcedure(procedureVO);
			Log.Info("AddDealerLastRedraw viewSeat:" + redrawMJVO.viewSeat, "Game");
		}
	}

	protected void GetRoundID()
	{
		if (WrappersManagerImpl.IsPatched(36530))
		{
			WrappersManagerImpl.GetPatch(36530).__Gen_Wrap_0(this);
		}
		else
		{
			if (m_data == null)
			{
				return;
			}
			for (int i = 0; i < m_data.nTLVNum; i++)
			{
				TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[i];
				if (tSOTLVUNIT != null && tSOTLVUNIT.stValue != null && tSOTLVUNIT.nType == 75)
				{
					TRoundIDInfo stRoundIDInfo = tSOTLVUNIT.stValue.stRoundIDInfo;
					if (stRoundIDInfo != null)
					{
						string str = Encoding.UTF8.GetString(Util.SbyteArrayToByteArray(stRoundIDInfo.szRoundId));
						TdrTypeUtil.StringNoNULL(ref str);
						roundModel.roundID = str;
						Log.Info("RevcDealCommand  GetGroundID = " + roundModel.roundID, "Game");
						base.dispatcher.Dispatch(GameEvent.ResetRoundID);
					}
					break;
				}
			}
		}
	}

	protected virtual void HandleNextToDo()
	{
		if (WrappersManagerImpl.IsPatched(36548))
		{
			WrappersManagerImpl.GetPatch(36548).__Gen_Wrap_0(this);
		}
		else if (m_data.nTLVNum > 0)
		{
			m_nextToDoHelper.HandleNextToDo(m_data.nTLVNum, m_data.astTLVInfo, roundModel, processModel, iComRule);
		}
	}

	private void HandleTeammateHandtile()
	{
		if (WrappersManagerImpl.IsPatched(36547))
		{
			WrappersManagerImpl.GetPatch(36547).__Gen_Wrap_0(this);
			return;
		}
		int nTLVNum = m_data.nTLVNum;
		TSOTLVUNIT[] astTLVInfo = m_data.astTLVInfo;
		for (int i = 0; i < nTLVNum; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = astTLVInfo[i];
			if (tSOTLVUNIT.nType != 66)
			{
				continue;
			}
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
				List<int> list = new List<int>();
				for (int j = 0; j < nHandTilesNum; j++)
				{
					list.Add(szTiles[j]);
				}
				UserPaiVO userPaiVO = new UserPaiVO();
				userPaiVO.viewSeat = viewSeatBySeat;
				userPaiVO.mjList.handpaiList = list;
				if (stHandKeyTiles.nTingTilesNum != 0)
				{
					userPaiVO.tingList = new List<int>();
					for (int k = 0; k < stHandKeyTiles.nTingTilesNum && k < stHandKeyTiles.szTingTiles.Length; k++)
					{
						userPaiVO.tingList.Add(stHandKeyTiles.szTingTiles[k]);
					}
				}
				if (3 == stHandKeyTiles.chType)
				{
					userPaiVO.reason = SetUserPaiReason.Teammate;
				}
				base.dispatcher.Dispatch(GameEvent.ShowHandTileProcess, userPaiVO);
			}
			Log.Info("ShowTeammateHandTile viewseat is " + viewSeatBySeat + " handtile count " + nHandTilesNum, "Game");
		}
	}

	private void HandleDonnerInfo()
	{
		if (WrappersManagerImpl.IsPatched(36546))
		{
			WrappersManagerImpl.GetPatch(36546).__Gen_Wrap_0(this);
			return;
		}
		TSOTLVUNIT[] tlvList = CSReplayRecordHelper.GetTlvList(m_data.astTLVInfo, m_data.nTLVNum, TLV_TYPE.EM_UP_TLV_DONNNERINFO);
		List<SCMJBeanChangeVO> list = new List<SCMJBeanChangeVO>();
		TSOTLVUNIT[] array = tlvList;
		for (int i = 0; i < array.Length; i++)
		{
			TDonnerInfo stDonnerInfo = array[i].stValue.stDonnerInfo;
			for (int j = 0; j < stDonnerInfo.nSeatNum; j++)
			{
				SCMJBeanChangeVO sCMJBeanChangeVO = new SCMJBeanChangeVO();
				sCMJBeanChangeVO.viewSeat = roundModel.GetViewSeatBySeat(stDonnerInfo.szSeatId[j]);
				sCMJBeanChangeVO.changeValue = stDonnerInfo.ChgDonner[j];
				sCMJBeanChangeVO.curBeanValue = stDonnerInfo.CurDonner[j];
				list.Add(sCMJBeanChangeVO);
			}
		}
		if (list.Count > 0)
		{
			CSReplayRecordService.sUpdateHappyBean(new SCMJWindRainVO
			{
				beanChangeList = list
			}, isRefreshHeadBeanScore: true);
		}
	}

	private List<int> GetTeammateHandTiles(int tlvCount, TSOTLVUNIT[] tlvInfo)
	{
		if (WrappersManagerImpl.IsPatched(36524))
		{
			return WrappersManagerImpl.GetPatch(36524).__Gen_Wrap_3410(this, tlvCount, tlvInfo);
		}
		List<int> list = new List<int>();
		for (int i = 0; i < tlvCount; i++)
		{
			TSOTLVUNIT tSOTLVUNIT = tlvInfo[i];
			if (tSOTLVUNIT.nType == 66)
			{
				THandTiles stHandTiles = tSOTLVUNIT.stValue.stHandKeyTiles.stHandTiles;
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(stHandTiles.nSeat);
				roundModel.teammateViewSeat = viewSeatBySeat;
				Log.Info("roundModel.teammateViewSeat set to: " + viewSeatBySeat, "Game");
				base.dispatcher.Dispatch(GameEvent.SetFriendLabel, viewSeatBySeat);
				sbyte[] szTiles = stHandTiles.szTiles;
				int nHandTilesNum = stHandTiles.nHandTilesNum;
				for (int j = 0; j < nHandTilesNum; j++)
				{
					list.Add(szTiles[j]);
				}
			}
		}
		return list;
	}

	private void AddSuitAdditionProcess(TChangeFaceSkill info)
	{
		if (WrappersManagerImpl.IsPatched(36537))
		{
			WrappersManagerImpl.GetPatch(36537).__Gen_Wrap_1(this, info);
			return;
		}
		Log.Info($"Recv SuitAddition Data: chAnimationType:{info.chAnimationType}, chBingoTileType:{info.chBingoTileType}, nBingoTileNum:{info.nBingoTileNum}, iPerBingoTileScore:{info.iPerBingoTileScore}, iTotalBingoTileScore:{info.iTotalBingoTileScore}", "SuitSkill");
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowSuitAdditionProcess;
		procedureVO.data = info;
		processModel.PushProcedure(procedureVO);
	}

	private void AddDrawTileColorSkillProcess(TDrawTileColorSkill info)
	{
		if (WrappersManagerImpl.IsPatched(36538))
		{
			WrappersManagerImpl.GetPatch(36538).__Gen_Wrap_1(this, info);
			return;
		}
		if (info == null)
		{
			Log.Info("Recv DrawTileColorSkill Data is null", "SuitSkill");
			return;
		}
		string message = $"Recv DrawTileColorSkill Data: iSkillID:{info.iSkillID}, chBingoTileType:{info.chBingoTileType}, iPerTileScore:{info.iPerTileScore}, iRoundLimitScore:{info.iRoundLimitScore}, iDayLimitScore:{info.iDayLimitScore}";
		globalModel.blackboard.SetData<int>("DRAW_TILE_COLOR_SKILL_TYPE", (int)info.chBingoTileType);
		Log.Info(message, "SuitSkill");
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowDrawTileColorSkillProcess;
		procedureVO.data = info;
		processModel.PushProcedure(procedureVO);
	}

	private void AddCharmSkillProcess(TCharmSkillInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36540))
		{
			WrappersManagerImpl.GetPatch(36540).__Gen_Wrap_1(this, info);
			return;
		}
		if (info == null)
		{
			Log.Info("Recv CharmSkillInfo Data is null", "SuitSkill");
			return;
		}
		Log.Info($"Recv CharmSkillInfo Data: iSkillId:{info.iSkillId}, iThinkTime:{info.iThinkTime}, iRemainCount:{info.iRemainCount}, iStatus:{info.iStatus}", "SuitSkill");
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowCharmSkillProcess;
		procedureVO.data = info;
		processModel.PushProcedure(procedureVO);
	}

	private void HandleRankToolsItems(TSoRankItem info)
	{
		if (WrappersManagerImpl.IsPatched(36533))
		{
			WrappersManagerImpl.GetPatch(36533).__Gen_Wrap_1(this, info);
			return;
		}
		roundModel.UsedRankItemToolsList = new List<int>();
		for (int i = 0; i < info.nItemCnt; i++)
		{
			roundModel.UsedRankItemToolsList.Add(info.EffectiveItem[i]);
		}
		string text = string.Join(", ", Array.ConvertAll(roundModel.UsedRankItemToolsList.ToArray(), (int x) => x.ToString()));
		Log.Info("UsedRankItemToolsList is: " + text, "RankTools");
		base.dispatcher.Dispatch(GameEvent.UpdateUsedRankToolData);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ShowUsedRankToolsProcess;
		procedureVO.data = roundModel.UsedRankItemToolsList;
		processModel.PushProcedure(procedureVO);
		Log.Info("ShowConsumedRankToolsProcess", "Game");
	}

	public void _003C_003EiFixBaseProxy_Execute()
	{
		base.Execute();
	}
}
