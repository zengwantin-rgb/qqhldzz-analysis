using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using GameModule;
using Google.Protobuf;
using HappyMahjong.Audio;
using HappyMahjong.CSReplay;
using HappyMahjong.Common;
using HappyMahjong.Common.Finder;
using HappyMahjong.Game.CompareTile;
using HappyMahjong.Game.NewHongZhong;
using HappyMahjong.Game.XueLiuBaoPai;
using HappyMahjong.GameJoy;
using HappyMahjong.OlympicMatch;
using HappyMahjong.PlayerRecord;
using HappyMahjong.ReturnUser.Model;
using HappyMahjong.ReturningUser;
using HappyMahjong.SSRInstituteSpace;
using HappyMahjong.Setting;
using HappyMahjong.Share;
using HappyMahjong.ShopAndBag;
using HappyMahjong.Tutorial;
using HuanLeGameSo;
using IFix;
using MJSoPb;
using TSDK4CSharp;
using UnityEngine;
using strange.extensions.command.impl;
using tsf4g_tdr_csharp;

namespace HappyMahjong.Game;

public class RevcGameReplayCommand : EventCommand
{
	protected HuanLeGameSo.CTableInfo m_data;

	protected RebuildTableUIVO m_tableUIVO;

	protected bool m_needToSplitRedraw;

	protected bool m_bInTingStatus;

	protected ProcedureVO m_todoProcedure;

	protected int m_14thMJ;

	protected int m_lastMJ;

	protected int m_removeFromTailNum;

	public NextToDoHelper m_nextToDoHelper;

	protected int[] m_playerTilsCount = new int[4];

	private bool m_inSectionReturn;

	protected HuAniData m_replayHuAniData;

	private HuanLeGameSo.TCharmSkillInfo m_replayCharmSkillInfo;

	private HuanLeGameSo.TCharmScoreUpdate m_replayCharmScoreUpdate;

	[Inject]
	public ReturnUserGameModel returnUserModel { get; set; }

	[Inject]
	public IReturningUserService returningUserService { get; set; }

	[Inject]
	public IGlobalModel globalModel { get; set; }

	[Inject]
	public IRoundModel roundModel { get; set; }

	[Inject]
	public IProcessModel processModel { get; set; }

	[Inject]
	public IShopModel shopModel { get; set; }

	[Inject]
	public IMJRule mjRule { get; set; }

	[Inject]
	public ICommonRule iComRule { get; set; }

	public override void Execute()
	{
		if (WrappersManagerImpl.IsPatched(36556))
		{
			WrappersManagerImpl.GetPatch(36556).__Gen_Wrap_0(this);
			return;
		}
		for (int i = 0; i < m_playerTilsCount.Length; i++)
		{
			m_playerTilsCount[i] = 0;
		}
		m_nextToDoHelper = new NextToDoHelper(base.dispatcher, roundModel, processModel, iComRule);
		m_14thMJ = -1;
		m_bInTingStatus = false;
		m_needToSplitRedraw = false;
		m_replayCharmSkillInfo = null;
		m_replayCharmScoreUpdate = null;
		m_data = (HuanLeGameSo.CTableInfo)base.evt.data;
		m_tableUIVO = new RebuildTableUIVO();
		roundModel.leftDelayTimes = 0;
		roundModel.RequestClientTingID = 0;
		m_removeFromTailNum = 0;
		m_inSectionReturn = false;
		ClearSectionShareData();
		iComRule.ClearTingInfo();
		HideFirstMatchAvatar();
		if (returnUserModel.DataAwardVo.IsReturnUser)
		{
			ReturningUserService.ReqQueryReturningUserInfo(0);
		}
		SingletonBase<MJResManager>.GetInstance().OnGameStart();
		if (m_data != null)
		{
			DOTween.Clear();
			VPTimer.CancelAll();
			Singleton<NetworkTips>.GetInstance().Clear();
			ClearDataAndTable();
			PlayerDataMgr.GetInstance().SetSelfPlayerStatus(PlayerClientStatus.DuringGame);
			HandlePrivateRoomInfo();
			HandleTLV();
			RebuildInfoUI();
			RebuildTableUI();
			GetRoundID();
			AddSelfJustRedraw();
			HandleTLVAfterRebuildTable();
			HandlePlayerStatus();
			ResetBeishu();
			roundModel.gameStatus = GameStatus.Playing;
			if (globalModel.enterGameInfo.matchType != MatchType.None)
			{
				ChallengeMatchRequest.ReqMacthPlayerInfo();
			}
			if (m_todoProcedure != null)
			{
				processModel.PushProcedure(m_todoProcedure);
			}
			if (CSReplayRecordService.sIsReplaying())
			{
				if (CSReplayRecordService.NeedPushOtherMjInReplaying())
				{
					ProcedureVO procedureVO = new ProcedureVO();
					procedureVO.type = ProcessType.DealProcessReplayShowMJValue;
					processModel.PushProcedure(procedureVO);
				}
				if (Util.IsTeamBattlePlaying((int)roundModel.gameType))
				{
					base.dispatcher.Dispatch(GameEvent.Replay2v2ReplayRefreshScore);
					base.dispatcher.Dispatch(GameEvent.RefreshTeamScore);
				}
			}
			base.dispatcher.Dispatch(GameEvent.ExecuteProcessCmd);
			PlayerDataMgr.GetInstance().SetSelfPlayerStatus(PlayerClientStatus.DuringGame);
			if (GameUtil.IsChangeCardRoom(globalModel.enterGameInfo.roomInfo) && globalModel.enterGameInfo.roomInfo.HasRecommendFan == 1)
			{
				base.dispatcher.Dispatch(RecommendFanEvent.PreRequestRecommendFan, true);
			}
			base.dispatcher.Dispatch(CompareTileEvent.TryInitNoHuStatusOnGameReplay);
			base.dispatcher.Dispatch(UIViewEvent.TryShowNewRecommendInGameBtn);
		}
		else
		{
			Log.Error("CTableInfo is null.", "Game");
		}
		base.dispatcher.Dispatch(UIViewEvent.UpdateLoadingTipsStatus, false);
		base.dispatcher.Dispatch(GameEvent.ReplayFinished);
		RestoreReplayCharmUIAfterRebuild();
		if (base.data != null)
		{
			base.dispatcher.Dispatch(GameEvent.ExecuteChooseTileNotify, new ChooseTileNotifyData
			{
				tlvInfo = m_data.astTLVInfo,
				isFromReplay = true
			});
		}
		if (Util.IsTeamBattlePlaying((int)globalModel.enterGameInfo.gameType))
		{
			base.dispatcher.Dispatch(GameEvent.InitTeamVoice);
			SingletonBase<TutorialController>.GetInstance().Enter2v2GameScene();
		}
		if (SectionUtil.GetCurrentRoomSectionType() == RoomSectionType.RoomPrivateRoom)
		{
			base.dispatcher.Dispatch(GameEvent.InitPrivateRoomVoice);
		}
		if (roundModel.gameType == HappyMahjong.Common.GameType.ModuleDragon)
		{
			base.dispatcher.Dispatch(GameEvent.SetDragonOrderBtnStatus, true);
		}
		if (roundModel.gameType >= HappyMahjong.Common.GameType.EntertainGameLianLianKan)
		{
			EntertainGameResourceLoader.GetInstance().PlayBgMusic(roundModel.gameType);
		}
		else if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.huEndGame == 1)
		{
			if (m_bInTingStatus)
			{
				AudioController.Instance.PlayAuto("HuMusic", AudioLayers.Background, -1f, bStartNetAudio: false, playwithoutSetting: true);
			}
			else
			{
				PlayBgm();
			}
		}
		else
		{
			bool isHuAudio = Util.enterGameVO.roomInfo.isHuAudio;
			int num = SingletonBase<DynamicConfig>.GetInstance().GetInt("InGameBgm", "audioMaxHuCount");
			if (!SSRInstituteInGameController.IsInSSRInstitute)
			{
				if (isHuAudio && GetTotalHuCount() >= num)
				{
					AudioController.Instance.PlayAuto("HuMusic", AudioLayers.Background, -1f, bStartNetAudio: false, playwithoutSetting: true);
				}
				else
				{
					PlayBgm();
				}
			}
		}
		PrepareShareData();
		PreLoadFanAniConfig();
		base.dispatcher.Dispatch(LongPressHideGameUIEvent.ChangeLongPressHideGameUISwitch, true);
		base.dispatcher.Dispatch(GameEvent.CheckAndRestoreBigRPlayCache);
		Log.Info("Iphone is " + Device.m_iphoneDeviceName, "ScreenRotate");
		Log.Info("NowDirection is " + Device.m_NowDirection, "ScreenRotate");
	}

	private void PlayBgm()
	{
		if (WrappersManagerImpl.IsPatched(36677))
		{
			WrappersManagerImpl.GetPatch(36677).__Gen_Wrap_0(this);
		}
		else if (!SSRInstituteInGameController.IsInSSRInstitute)
		{
			if (!Util.IsNewStyleGameScene())
			{
				AudioController.Instance.PlayBgm(BGMType.OutGame);
			}
			else
			{
				AudioController.Instance.PlayBgm(BGMType.InGame);
			}
		}
	}

	protected virtual void ClearDataAndTable()
	{
		if (WrappersManagerImpl.IsPatched(36566))
		{
			WrappersManagerImpl.GetPatch(36566).__Gen_Wrap_0(this);
			return;
		}
		base.dispatcher.Dispatch(GameEvent.HideFirstMatchUI);
		base.dispatcher.Dispatch(GameEvent.ResetGameDataAndUI);
		base.dispatcher.Dispatch(GameEvent.CleanDiscardTip, -1);
		PlayerDataVO selfInfo = PlayerDataMgr.GetInstance().GetSelfInfo();
		if (selfInfo != null)
		{
			roundModel.selfSeat = selfInfo.seatId;
			Log.Info("Game Replay ############## Set Self Seat: " + roundModel.selfSeat, "Game");
		}
		else
		{
			Log.Error("selfPlayerDetail is null.", "Game");
		}
		SingletonBase<GlobalDataModel>.GetInstance().cardModel.ClearCardItemUseState();
	}

	private void ClearSectionShareData()
	{
		if (WrappersManagerImpl.IsPatched(36560))
		{
			WrappersManagerImpl.GetPatch(36560).__Gen_Wrap_0(this);
			return;
		}
		globalModel.SectionScoreShareData = null;
		globalModel.SectionLoseScoreShareData = null;
	}

	protected void HandleTLVAfterRebuildTable()
	{
		if (WrappersManagerImpl.IsPatched(36638))
		{
			WrappersManagerImpl.GetPatch(36638).__Gen_Wrap_0(this);
			return;
		}
		int num = 0;
		int nTLVNum = m_data.nTLVNum;
		for (num = 0; num < nTLVNum; num++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[num];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			Log.Info("Replay, HandleTLVAfterRebuildTable type:" + nType, "Game");
			switch (nType)
			{
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_KEY_HAND_TILES:
			{
				HuanLeGameSo.TKeyHandTiles stHandKeyTiles = tSOTLVUNIT.stValue.stHandKeyTiles;
				SetUserPaiReason reason = SetUserPaiReason.Normal;
				if (stHandKeyTiles.chType == 0)
				{
					reason = SetUserPaiReason.MingLouQuanLiang;
				}
				else if (1 == stHandKeyTiles.chType)
				{
					reason = SetUserPaiReason.MingLou;
				}
				else if (2 == stHandKeyTiles.chType)
				{
					reason = SetUserPaiReason.Liangdao;
					if (CSReplayRecordService.sIsReplaying())
					{
						UserPaiVO userPaiVO = new UserPaiVO();
						userPaiVO.reason = reason;
						userPaiVO.viewSeat = roundModel.GetViewSeatBySeat(stHandKeyTiles.stHandTiles.nSeat);
						userPaiVO.tingList = new List<int>();
						for (int i = 0; i < stHandKeyTiles.nTingTilesNum; i++)
						{
							userPaiVO.tingList.Add(stHandKeyTiles.szTingTiles[i]);
						}
						userPaiVO.canGiveTingTile = stHandKeyTiles.chCanGiveTingTile == 1;
						base.dispatcher.Dispatch(GameEvent.ShowHandTilesPaoPaiTips, userPaiVO);
						break;
					}
				}
				else if (3 == stHandKeyTiles.chType)
				{
					reason = SetUserPaiReason.Teammate;
					roundModel.teammateViewSeat = roundModel.GetViewSeatBySeat(stHandKeyTiles.stHandTiles.nSeat);
					Log.Info("roundModel.teammateViewSeat set to: " + roundModel.teammateViewSeat, "Game");
					base.dispatcher.Dispatch(GameEvent.SetFriendLabel, roundModel.teammateViewSeat);
					base.dispatcher.Dispatch(GameEvent.RefreshTeamScore);
					if (!m_inSectionReturn)
					{
						base.dispatcher.Dispatch(GameEvent.ShowOppositeViewSeatHandTile, true);
					}
				}
				else if (5 == stHandKeyTiles.chType)
				{
					int num2 = SetOtherPlayerHandMj(stHandKeyTiles.stHandTiles);
					List<int> seat = new List<int> { num2 };
					roundModel.MingPaiViewSeat.Add(num2);
					base.dispatcher.Dispatch(GameEvent.SetMingPaiTips, num2);
					AddDealMingPaiProcess(seat);
					break;
				}
				SetOthersHandTileAfterSelfTing(stHandKeyTiles.stHandTiles, reason, stHandKeyTiles.nTingTilesNum, stHandKeyTiles.szTingTiles, stHandKeyTiles.chCanGiveTingTile);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_ALLUSER_JI_INFO:
				m_nextToDoHelper.HandleUserAllJiInfo(tSOTLVUNIT.stValue.stUserGZJiInfo, inBalance: false);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_HAND_TILES:
				if (roundModel.GetViewSeatBySeat(tSOTLVUNIT.stValue.stHandTiles.nSeat) != 0)
				{
					if (CSReplayRecordService.sIsReplaying())
					{
						SetUserPaiReason reason = SetUserPaiReason.MingPaiReplay;
						SetOthersHandTileAfterSelfTingInReplay(tSOTLVUNIT.stValue.stHandTiles, reason);
					}
					else
					{
						SetUserPaiReason reason = SetUserPaiReason.GuoHu;
						SetOthersHandTileAfterSelfTing(tSOTLVUNIT.stValue.stHandTiles, reason, 0, null, 0);
					}
				}
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_PLAYER_WIN_TIMES:
			{
				HuanLeGameSo.TPlayerWinTimes stPlayerWinTimes = tSOTLVUNIT.stValue.stPlayerWinTimes;
				m_nextToDoHelper.HandlePlayerWinTimes(stPlayerWinTimes, reconnect: true);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_BAO_PAI:
				HandleBaoPai(tSOTLVUNIT.stValue.stBaoPai);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_NEW_RECOMMEND_FAN_TARGET:
			{
				HuanLeGameSo.TNewRecommendFanTarget stNewRecommendFanTarget = tSOTLVUNIT.stValue.stNewRecommendFanTarget;
				m_nextToDoHelper.HandleNewRecommendFanDetails(stNewRecommendFanTarget);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_LIAN_GANG:
			{
				HuanLeGameSo.TLianGang stLianGang = tSOTLVUNIT.stValue.stLianGang;
				SingletonBase<EffectHandler>.GetInstance().ShowLianGangTips(roundModel.GetViewSeatBySeat(stLianGang.nLianGangSeatID), stLianGang.chLianGangNum);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_DRAW_TILE_COLOR_SKILL:
			{
				HuanLeGameSo.TDrawTileColorSkill stDrawTileColorSkill = tSOTLVUNIT.stValue.stDrawTileColorSkill;
				globalModel.blackboard.SetData<int>("DRAW_TILE_COLOR_SKILL_TYPE", (int)stDrawTileColorSkill.chBingoTileType);
				break;
			}
			}
			foreach (IGameEventListener item in SingletonBase<GameModuleRegistry>.GetInstance().GetGameEventsHandler())
			{
				item.HandleGameReplayTLVAfterRebuildUI(nType, tSOTLVUNIT, this);
			}
		}
		foreach (IGameEventListener item2 in SingletonBase<GameModuleRegistry>.GetInstance().GetGameEventsHandler())
		{
			item2.HandleGameReplayAllTLVAfterRebuildUI(nTLVNum, m_data.astTLVInfo, this, m_nextToDoHelper);
		}
		base.dispatcher.Dispatch(GameEvent.HuJiaoZhuangGangReplay);
	}

	protected virtual void HandleTLV()
	{
		if (WrappersManagerImpl.IsPatched(36569))
		{
			WrappersManagerImpl.GetPatch(36569).__Gen_Wrap_0(this);
			return;
		}
		m_nextToDoHelper.CheckAbnormalMJNum(m_data.nTLVNum, m_data.astTLVInfo, roundModel);
		int num = 0;
		int nTLVNum = m_data.nTLVNum;
		for (num = 0; num < nTLVNum; num++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[num];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			Log.Info("Replay, HandleTLV type Before:" + nType, "Game");
			if (nType == HuanLeGameSo.TLV_TYPE.EM_UP_TLV_CONFIG_INFO)
			{
				AddFixConfigInfo(tSOTLVUNIT.stValue.stFixConfigInfo);
				break;
			}
		}
		for (num = 0; num < nTLVNum; num++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[num];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			Log.Info("Replay, HandleTLV type:" + nType, "Game");
			switch (nType)
			{
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_ROUND_INFO:
				SetRoundData(tSOTLVUNIT.stValue.stRoundInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_HAND_TILES:
				Log.Info("Replay, EM_UP_TLV_HAND_TILES nSeat = " + tSOTLVUNIT.stValue.stHandTiles.nSeat + " tiles: " + string.Join(",", tSOTLVUNIT.stValue.stHandTiles.szTiles), "Game");
				if (roundModel.GetViewSeatBySeat(tSOTLVUNIT.stValue.stHandTiles.nSeat) == 0)
				{
					SetPlayerHandTile(tSOTLVUNIT.stValue.stHandTiles);
				}
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_GIVE_TILES:
				SetPlayerGive(tSOTLVUNIT.stValue.stGivenTiles);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_SET_TILES:
				SetPlayerAction(tSOTLVUNIT.stValue.stSetTiles);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_FLOWER_KEY:
				SetPlayerFlower(tSOTLVUNIT.stValue.stFlowerInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_GIVE_KEY:
				if (tSOTLVUNIT.stValue.stDoGiveInfo.chTile != 0)
				{
					Log.Info("Replay, Do Give Redraw mjValue:" + tSOTLVUNIT.stValue.stDoGiveInfo.chTile, "Game");
					roundModel.curRedrawMJValue = tSOTLVUNIT.stValue.stDoGiveInfo.chTile;
					roundModel.GetPlayerGameInfoByViewSeat(0).tiles.curRedrawMjValue = roundModel.curRedrawMJValue;
					m_needToSplitRedraw = true;
				}
				roundModel.SetRoundTime(tSOTLVUNIT.stValue.stDoGiveInfo.nAllClientTime, tSOTLVUNIT.stValue.stDoGiveInfo.nMaxShowTime);
				roundModel.doGiveFlag = tSOTLVUNIT.stValue.stDoGiveInfo.nFlag;
				m_nextToDoHelper.HandleDiscardTips(tSOTLVUNIT.stValue.stDoGiveInfo);
				AddEnterGiveProcess(roundModel.selfSeat, 0);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_WAIT_KEY:
				HandleWait(tSOTLVUNIT.stValue.stDoWaitInfo, 0, null, null);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_BLOCK_KEY:
				AddEnterBlockProcess(tSOTLVUNIT.stValue.stDoBlockInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_SHOW_SCORE_INFO:
				HandleDapiao(tSOTLVUNIT.stValue.stShowScoreInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DINGQUE:
				HandleDingque(tSOTLVUNIT.stValue.stDingque);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_RECHARGE:
				HandleCharge(tSOTLVUNIT.stValue.stRecharge);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_LAIZI:
				HandleLaizi(tSOTLVUNIT.stValue.stLaiZi);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_WINTIPS_INFO:
				AddWinTipsInfo();
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_CHANGECARD:
				HandleChangeCard(tSOTLVUNIT.stValue.stChangeCard);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PASS_TIMES:
				HandlePassInfo(tSOTLVUNIT.stValue.stPassTimesInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_BET_INFO:
				HandleBetInfo(tSOTLVUNIT.stValue.stBetInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_ZN:
				HandleZhaNiao(tSOTLVUNIT.stValue.stDoZN);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DELAY_INFO:
				HandleDelayInfo(tSOTLVUNIT.stValue.stDelayInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_LAIZIPI_INFO:
				AddPizi(tSOTLVUNIT.stValue.stPiZi);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MA_INFO:
				AddBuyHorseProcess(tSOTLVUNIT.stValue.stPlayerMaInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MA_RESULT:
				AddBuyHorseResultProcess(tSOTLVUNIT.stValue.stPlayerMaResult);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_FANGPAO:
				HandleFangPao(tSOTLVUNIT.stValue.stFangPaoInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MORE_FANGPAO:
				HandleYiPaoDuoXiang(tSOTLVUNIT.stValue.stOneMorePaoInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_VOIP_USERACCESS:
				HandleVoipRoominfo(tSOTLVUNIT.stValue.stVoipUserAccess);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PRIVATE_ROOM_REPLAY_INFO:
				HandleSecretRoominfo(tSOTLVUNIT.stValue.stPrivateTableReplayInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_RAIN_INFO:
				AddWHMJRainInfo(tSOTLVUNIT.stValue.stRainInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_ROAM_INFO:
				AddHaiDiProcess(tSOTLVUNIT.stValue.stRoamInfo.chChair, tSOTLVUNIT.stValue.stRoamInfo.nAllClientTime, tSOTLVUNIT.stValue.stRoamInfo.nMaxShowTime);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_QISHOUHU:
				AddChangShaMjDealHuInfo(tSOTLVUNIT.stValue.stNotifyCheckQishouhu);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_NOTIFY_GANGYAO_TILE:
				AddChangShaMJGangYaoTileAndDice(tSOTLVUNIT.stValue.stNotifyGangyaoTileAndDice);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_DO_GANGYAO_BLOCK_KEY:
				AddEnterGangYaoBlockProcess(tSOTLVUNIT.stValue.stDoGangyaoBlockInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_LUA_CONFIG:
				m_nextToDoHelper.HandleLuaString(tSOTLVUNIT.stValue.stLuaConfigInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_CALLBACK_SET_TILES:
				HandleCallBackTileInfo(tSOTLVUNIT.stValue.stCallBackdTileSet);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_FAKE_HU_INFO:
				HandleFakeHu(tSOTLVUNIT.stValue.stFakeHuInfoList);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CAIPIAO_INFO:
				m_nextToDoHelper.HandleCaiPiao(tSOTLVUNIT.stValue.stCaiPiaoInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_REPLAY_CAN_RETURN_SECTION_SCORE:
				HandleSectionReturn(tSOTLVUNIT.stValue.stReturnSectionScore);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_TEAM_INFO:
				HandleTeamInfo(tSOTLVUNIT.stValue.stMatchTeamInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_RANK_ITEM:
				HandleRankToolsItems(tSOTLVUNIT.stValue.stSoRankItemInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_EGG_TILES:
				HandleDanTile(tSOTLVUNIT.stValue.stEggTiles);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_ROUND_INTELLIGENCE_ADVICE_SWITCH:
				iComRule.IsTingFromServer = tSOTLVUNIT.stValue.stRoundIntelligenceAdviceSwitch.iClientCalDiscardTingBasic == 0;
				iComRule.IsClientCalTingRecommend = tSOTLVUNIT.stValue.stRoundIntelligenceAdviceSwitch.iClientCalDiscardTingAdvice == 1;
				iComRule.IsReportTingResult = tSOTLVUNIT.stValue.stRoundIntelligenceAdviceSwitch.iClientReportDiscardTingBasic == 1;
				Log.Info($"Replay: tingformsvr: {iComRule.IsTingFromServer}, tingAdve: {iComRule.IsClientCalTingRecommend}, reportTing: {iComRule.IsReportTingResult}, SvrHasCPGAdvice:{tSOTLVUNIT.stValue.stRoundIntelligenceAdviceSwitch.iIsOpenIntelligenceAdviceMultiChiPengGang}", "AutoPlay");
				SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().ServerCallTing = iComRule.IsTingFromServer;
				SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().ServerToldClientCallAdvice = iComRule.IsClientCalTingRecommend;
				SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().ServerHasRecommendCPG = tSOTLVUNIT.stValue.stRoundIntelligenceAdviceSwitch.iIsOpenIntelligenceAdviceMultiChiPengGang == 1;
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_INTELLIGENCE_ADVICE:
				HandleServerTingAndRecommend(tSOTLVUNIT.stValue.stIntelligenceAdvice);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_WHETHER_MINGPAI_INFO:
				AddMingPaiInfoProcess(tSOTLVUNIT.stValue.stWhetherMingpai);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_MINGPAI:
				AddWaitMingPaiProcess(tSOTLVUNIT.stValue.stWaitMingPai);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_ROUND_FEATURE:
				m_nextToDoHelper.HandleRoundFeature(tSOTLVUNIT.stValue.stRoundFeature);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_TABLE_DONNER_INFO:
				m_nextToDoHelper.HandleTableDonnerInfo(tSOTLVUNIT.stValue.stTableDonnerInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CARD_WALL_INFO:
				SetSingleWallCardInfo(tSOTLVUNIT.stValue.stCardWallInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_AVATAR_SKILL_INFO:
				HandleAddOneVarTileData(tSOTLVUNIT.stValue.stAvatarSkillInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_TILES_SHOW_INFO_IN_CARD_WALL:
				ShowInfoInCardWall(tSOTLVUNIT.stValue.stTilesShowInfoInCardWall);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_PLAYER_WIN_TIMES:
				ShowWinTimes(tSOTLVUNIT.stValue.stPlayerWinTimes);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_TABLE_CONF_INFO:
				m_nextToDoHelper.HandleModuleTableConfigInfo(tSOTLVUNIT.stValue.stTableConfigInfo);
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
					Log.Error("[RevcGameReplayCommand] ParseFrom error: " + ex.Message, "Replay");
					unitProtoValue = new MJSoPb.UnitProtoValue();
				}
				if (unitProtoValue == null)
				{
					unitProtoValue = new MJSoPb.UnitProtoValue();
				}
				if (unitProtoValue.ProtoIntelligenceAdvice != null && unitProtoValue.ProtoIntelligenceAdvice.AdviceType == 1)
				{
					iComRule.IsTingFromServer = true;
				}
				HandleProtoServerTingAndRecommend(unitProtoValue.ProtoIntelligenceAdvice);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_RECOMMEND_FAN:
			{
				HuanLeGameSo.TRecommendFanInfo stRecommendFanInfo = tSOTLVUNIT.stValue.stRecommendFanInfo;
				HandleRecommendFanInfo(stRecommendFanInfo);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_UP_TLV_NaNiao_INFO:
			{
				HuanLeGameSo.TZaNiaoInfo stZaNiaoInfo = tSOTLVUNIT.stValue.stZaNiaoInfo;
				HandleZhaNiaoInfo(stZaNiaoInfo);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_NEW_SECTION_SCORE_JIE_SUAN:
			{
				HuanLeGameSo.TNewSectionScoreJieSuan tNewSectionScoreJieSuan = tSOTLVUNIT.stValue?.stNewSectionScoreJieSuan;
				if (tNewSectionScoreJieSuan == null)
				{
					Log.Error("RevcGameReplayCommand: new section score data is null", "Game");
					break;
				}
				roundModel.NewSectionScoreJieSuan = tNewSectionScoreJieSuan;
				if (InGameRankGradeUtil.IsSectionDataOnSelfLeave(tNewSectionScoreJieSuan))
				{
					roundModel.hasPushedCharmSettleEffectProcess = true;
				}
				base.dispatcher.Dispatch(GameEvent.UpdateNewSectionInfo);
				break;
			}
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CHARM_SKILL_INFO:
				AddCharmSkillProcess(tSOTLVUNIT.stValue.stCharmSkillInfo);
				break;
			case HuanLeGameSo.TLV_TYPE.EM_DN_TLV_CHARM_SCORE_UPDATE:
				HandleCharmScoreUpdate(tSOTLVUNIT.stValue.stCharmScoreUpdate);
				break;
			}
			foreach (IGameEventListener item in SingletonBase<GameModuleRegistry>.GetInstance().GetGameEventsHandler())
			{
				item.HandleGameReplayTLV(nType, tSOTLVUNIT, this);
			}
		}
		List<HuanLeGameSo.THuTipsInfo> list = new List<HuanLeGameSo.THuTipsInfo>();
		for (num = 0; num < nTLVNum; num++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[num];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			Log.Info("Handle TLV, Type:" + nType, "Game");
			if (nType == HuanLeGameSo.TLV_TYPE.EM_UP_TLV_HU_TIPS)
			{
				HuanLeGameSo.THuTipsInfo stHuTipsInfo = tSOTLVUNIT.stValue.stHuTipsInfo;
				list.Add(stHuTipsInfo);
			}
		}
		if (list.Count > 0)
		{
			m_nextToDoHelper.HandleHuTipsInfo(list);
		}
		foreach (IGameEventListener item2 in SingletonBase<GameModuleRegistry>.GetInstance().GetGameEventsHandler())
		{
			item2.HandleGameReplayAllTLV(nTLVNum, m_data.astTLVInfo, this, m_nextToDoHelper);
		}
		HuanLeGameSo.TWinFanInfo winTilesFromTLV = BalanceCommonUtil.GetWinTilesFromTLV(m_data.astTLVInfo, m_data.nTLVNum);
		HuanLeGameSo.TWinTileTypeInfoV winTileTypeInfoVFromTLV = BalanceCommonUtil.GetWinTileTypeInfoVFromTLV(m_data.astTLVInfo, m_data.nTLVNum);
		if (winTileTypeInfoVFromTLV != null)
		{
			m_replayHuAniData = SingletonBase<FanAniHelper>.GetInstance().GetAniData(roundModel, winTileTypeInfoVFromTLV, winTilesFromTLV);
			Log.Info("Replay HandleTLV: pre-parsed HuAniData, AllHuAniData count=" + ((m_replayHuAniData.AllHuAniData != null) ? m_replayHuAniData.AllHuAniData.Count : 0), "Game");
		}
		else
		{
			m_replayHuAniData = null;
			Log.Info("Replay HandleTLV: no WinTileTypeInfoV found in TLV, HuAniData is null", "Game");
		}
	}

	private void HandleRecommendFanInfo(HuanLeGameSo.TRecommendFanInfo recommendFanInfo)
	{
		if (WrappersManagerImpl.IsPatched(36630))
		{
			WrappersManagerImpl.GetPatch(36630).__Gen_Wrap_1(this, recommendFanInfo);
		}
		else
		{
			RecvRecommendFanCommand.HandleRecommendFanInfo(recommendFanInfo, roundModel, base.dispatcher);
		}
	}

	private void HandleZhaNiaoInfo(HuanLeGameSo.TZaNiaoInfo tlvValue)
	{
		if (WrappersManagerImpl.IsPatched(36631))
		{
			WrappersManagerImpl.GetPatch(36631).__Gen_Wrap_1(this, tlvValue);
			return;
		}
		ZhaNiaoVO zhaNiaoVO = new ZhaNiaoVO();
		zhaNiaoVO.viewSeat = roundModel.GetViewSeatBySeat(tlvValue.chChair);
		for (int i = 0; i < tlvValue.nZaNiaoNum && i < tlvValue.szZaNiao.Length; i++)
		{
			int item = -1;
			if (tlvValue.szZhaNiaoBingo != null && i < tlvValue.szZhaNiaoBingo.Length && tlvValue.szZhaNiaoBingo[i] >= 0)
			{
				item = roundModel.GetViewSeatBySeat(tlvValue.szZhaNiaoBingo[i]);
			}
			zhaNiaoVO.targetViewSeats.Add(item);
			zhaNiaoVO.resultMJs.Add(tlvValue.szZaNiao[i]);
		}
		MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(zhaNiaoVO.viewSeat);
		for (int j = 0; j < zhaNiaoVO.resultMJs.Count; j++)
		{
			tilesByViewSeat.AddZhaNiao(zhaNiaoVO.resultMJs[j]);
		}
	}

	private void AddCharmSkillProcess(HuanLeGameSo.TCharmSkillInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36632))
		{
			WrappersManagerImpl.GetPatch(36632).__Gen_Wrap_1(this, info);
			return;
		}
		if (info == null)
		{
			Log.Info("Replay Recv CharmSkillInfo Data is null", "SuitSkill");
			return;
		}
		Log.Info($"Replay Recv CharmSkillInfo Data: iSkillId:{info.iSkillId}, iThinkTime:{info.iThinkTime}, iRemainCount:{info.iRemainCount}, iStatus:{info.iStatus}, iTargetSeat:{info.iTargetSeat}", "SuitSkill");
		m_replayCharmSkillInfo = info;
		roundModel.charmRemainCount = info.iRemainCount;
		if (info.iTargetSeat >= 0)
		{
			roundModel.charmTargetSeatId = info.iTargetSeat;
		}
	}

	private void HandleCharmScoreUpdate(HuanLeGameSo.TCharmScoreUpdate info)
	{
		if (WrappersManagerImpl.IsPatched(36633))
		{
			WrappersManagerImpl.GetPatch(36633).__Gen_Wrap_1(this, info);
			return;
		}
		if (info == null)
		{
			Log.Info("Replay CharmScoreUpdate Data is null", "SuitSkill");
			return;
		}
		m_replayCharmScoreUpdate = info;
		if (info.iTargetSeat >= 0)
		{
			roundModel.charmTargetSeatId = info.iTargetSeat;
		}
	}

	private void RestoreReplayCharmUIAfterRebuild()
	{
		if (WrappersManagerImpl.IsPatched(36676))
		{
			WrappersManagerImpl.GetPatch(36676).__Gen_Wrap_0(this);
			return;
		}
		if (m_replayCharmScoreUpdate != null)
		{
			CharmTLVParser.DispatchCharmScoreUpdate(m_replayCharmScoreUpdate, base.dispatcher);
		}
		if (m_replayCharmSkillInfo != null)
		{
			base.dispatcher.Dispatch(GameEvent.NotifyCharmSelect, new CharmSkillNotifyVO
			{
				skillInfo = m_replayCharmSkillInfo,
				playAppearSound = false
			});
		}
	}

	private void HandleServerTingAndRecommend(HuanLeGameSo.TIntelligenceAdvice advice)
	{
		if (WrappersManagerImpl.IsPatched(36620))
		{
			WrappersManagerImpl.GetPatch(36620).__Gen_Wrap_1(this, advice);
			return;
		}
		if (advice.iAdviceType == 1)
		{
			if (iComRule.IsTingFromServer)
			{
				iComRule.SetTingInfoFromServer(advice.stIntelligenceAdviceTing);
				if (roundModel.gameType == HappyMahjong.Common.GameType.ModuleCompareTile)
				{
					iComRule.CheckTingFromServer();
				}
			}
			return;
		}
		Log.Info("HandleServerTingAndRecommend is recommend " + advice.iAdviceType + ", tile:" + advice.iAdviceTile, "AutoPlay");
		if (!SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().RecommendUseClientSetting || SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().RecommendCPG)
		{
			RecommendActionVO recommendActionVO = new RecommendActionVO();
			recommendActionVO.type = 0uL;
			if (advice.iAdviceType == 3)
			{
				recommendActionVO.type = 64uL;
			}
			else if (advice.iAdviceType == 2)
			{
				recommendActionVO.type = 128uL;
				recommendActionVO.adviceTile = advice.iAdviceTile;
			}
			else if (advice.iAdviceType == 5)
			{
				recommendActionVO.type = 1uL;
			}
			else if (advice.iAdviceType == 4)
			{
				recommendActionVO.type = 8uL;
				recommendActionVO.adviceTile = advice.iAdviceTile;
			}
			if (recommendActionVO.type != 0L)
			{
				iComRule.RecommendAction = recommendActionVO;
				base.dispatcher.Dispatch(GameEvent.OnNotifyRecommendChuPai, recommendActionVO);
			}
		}
	}

	private void HandleProtoServerTingAndRecommend(MJSoPb.TProtoIntelligenceAdvice advice)
	{
		if (WrappersManagerImpl.IsPatched(36629))
		{
			WrappersManagerImpl.GetPatch(36629).__Gen_Wrap_1(this, advice);
		}
		else
		{
			if (advice == null)
			{
				return;
			}
			if (advice.AdviceType == 1)
			{
				if (iComRule.IsTingFromServer)
				{
					iComRule.SetTingInfoFromServer(advice.IntelligenceAdviceTing);
					if (roundModel.gameType == HappyMahjong.Common.GameType.ModuleCompareTile)
					{
						iComRule.CheckTingFromServer();
					}
				}
				return;
			}
			Log.Info("HandleServerTingAndRecommend is recommend " + advice.AdviceType + ", tile:" + advice.AdviceTile, "AutoPlay");
			if (!SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().RecommendUseClientSetting || SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().RecommendCPG)
			{
				RecommendActionVO recommendActionVO = new RecommendActionVO();
				recommendActionVO.type = 0uL;
				if (advice.AdviceType == 3)
				{
					recommendActionVO.type = 64uL;
				}
				else if (advice.AdviceType == 2)
				{
					recommendActionVO.type = 128uL;
					recommendActionVO.adviceTile = advice.AdviceTile;
				}
				else if (advice.AdviceType == 5)
				{
					recommendActionVO.type = 1uL;
				}
				else if (advice.AdviceType == 4)
				{
					recommendActionVO.type = 8uL;
					recommendActionVO.adviceTile = advice.AdviceTile;
				}
				if (recommendActionVO.type != 0L)
				{
					iComRule.RecommendAction = recommendActionVO;
					base.dispatcher.Dispatch(GameEvent.OnNotifyRecommendChuPai, recommendActionVO);
				}
			}
		}
	}

	private void AddMingPaiInfoProcess(HuanLeGameSo.TWhetherMingpai mingPaiInfo)
	{
		if (WrappersManagerImpl.IsPatched(36621))
		{
			WrappersManagerImpl.GetPatch(36621).__Gen_Wrap_1(this, mingPaiInfo);
			return;
		}
		Log.Info($"AddMingPaiInfoProcess SeatID is {mingPaiInfo.iSeatID}, Beishu is {mingPaiInfo.iMingPaiBeishu}, Status is {mingPaiInfo.iStatus}", "Game");
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(mingPaiInfo.iSeatID);
		MingPaiVO mingPaiVO = new MingPaiVO();
		mingPaiVO.viewSeat = viewSeatBySeat;
		mingPaiVO.mingPaiBeishu = mingPaiInfo.iMingPaiBeishu;
		mingPaiVO.status = mingPaiInfo.iStatus;
		mingPaiVO.mingPaiEnd = mingPaiInfo.iMingPaiEnd;
		roundModel.IsMingPai = true;
		if (mingPaiVO.mingPaiEnd == 1)
		{
			VPTimer.CancelAll("DelayReqMingPaiEnd");
		}
		if (mingPaiVO.status != 0)
		{
			roundModel.MingPaiViewSeat.Add(0);
		}
		if (mingPaiVO.status == 2)
		{
			base.dispatcher.Dispatch(GameEvent.SetMingPaiTips, viewSeatBySeat);
		}
		if (viewSeatBySeat == 0 && !CSReplayRecordService.sIsReplaying())
		{
			base.dispatcher.Dispatch(GameEvent.SetMingPaiDoubleBtn, mingPaiVO);
		}
		SetSelfMingPaiTips();
	}

	private void SetSelfMingPaiTips()
	{
		if (WrappersManagerImpl.IsPatched(36622))
		{
			WrappersManagerImpl.GetPatch(36622).__Gen_Wrap_0(this);
		}
		else if (roundModel.IsMingPaiViewSeat(0))
		{
			base.dispatcher.Dispatch(GameEvent.SetMingPaiTips, 0);
		}
	}

	private void AddWaitMingPaiProcess(HuanLeGameSo.TWaitMingPai waitMingPai)
	{
		if (WrappersManagerImpl.IsPatched(36623))
		{
			WrappersManagerImpl.GetPatch(36623).__Gen_Wrap_1(this, waitMingPai);
			return;
		}
		Log.Info("AddWaitMingPaiProcess MaxShowTime is " + waitMingPai.nMaxShowTime, "Game");
		base.dispatcher.Dispatch(GameEvent.ShowTimeCountDown, (int)waitMingPai.nMaxShowTime);
		SingletonBase<Toast>.GetInstance().showUGUI("请选择是否明牌", waitMingPai.nMaxShowTime);
		Log.Info($"AddWaitMingPaiProcess SeatID is {waitMingPai.stWhetherMingpai.iSeatID}, Beishu is {waitMingPai.stWhetherMingpai.iMingPaiBeishu}, Status is {waitMingPai.stWhetherMingpai.iStatus}", "Game");
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(waitMingPai.stWhetherMingpai.iSeatID);
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
			base.dispatcher.Dispatch(GameEvent.SetMingPaiDoubleBtn, mingPaiVO);
		}
	}

	private void DelayReqMingPaiEnd()
	{
		if (WrappersManagerImpl.IsPatched(36624))
		{
			WrappersManagerImpl.GetPatch(36624).__Gen_Wrap_0(this);
			return;
		}
		SingletonBase<Toast>.GetInstance().Clear();
		base.dispatcher.Dispatch(GameEvent.RequestMingPai, HuanLeGameSo.SO_MINGPAI_TYPE.END_MINGPAI);
	}

	private int SetOtherPlayerHandMj(HuanLeGameSo.THandTiles handTiles)
	{
		if (WrappersManagerImpl.IsPatched(36639))
		{
			return WrappersManagerImpl.GetPatch(36639).__Gen_Wrap_1557(this, handTiles);
		}
		sbyte[] szTiles = handTiles.szTiles;
		int nHandTilesNum = handTiles.nHandTilesNum;
		int nSeat = handTiles.nSeat;
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(nSeat);
		switch (viewSeatBySeat)
		{
		default:
			Log.Error("Illegal viewSeat. playerSeat:" + nSeat, "Game");
			break;
		case 1:
		case 2:
		case 3:
		{
			List<int> list = new List<int>();
			for (int i = 0; i < nHandTilesNum; i++)
			{
				list.Add(szTiles[i]);
			}
			UserPaiVO userPaiVO = new UserPaiVO();
			userPaiVO.viewSeat = viewSeatBySeat;
			userPaiVO.mjList.handpaiList = list;
			AddSetUserPai(userPaiVO);
			break;
		}
		case 0:
			break;
		}
		return viewSeatBySeat;
	}

	private void AddSetUserPai(UserPaiVO payLoad)
	{
		if (WrappersManagerImpl.IsPatched(36640))
		{
			WrappersManagerImpl.GetPatch(36640).__Gen_Wrap_1(this, payLoad);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.SetUserPai;
		procedureVO.data = payLoad;
		procedureVO.maxTime = 5;
		processModel.PushProcedure(procedureVO);
	}

	private void AddDealMingPaiProcess(List<int> seat)
	{
		if (WrappersManagerImpl.IsPatched(36641))
		{
			WrappersManagerImpl.GetPatch(36641).__Gen_Wrap_1(this, seat);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.MingPaiDouble;
		procedureVO.data = seat;
		procedureVO.maxTime = 5;
		processModel.PushProcedure(procedureVO);
	}

	private void SetSingleWallCardInfo(HuanLeGameSo.TCardWallInfo wallCardInfo)
	{
		if (WrappersManagerImpl.IsPatched(36625))
		{
			WrappersManagerImpl.GetPatch(36625).__Gen_Wrap_1(this, wallCardInfo);
			return;
		}
		m_tableUIVO.wallVO.singleCardWallInfo = new List<HuanLeGameSo.TSingleCardWallInfo>(wallCardInfo.nCardWallNum);
		for (int i = 0; i < wallCardInfo.nCardWallNum; i++)
		{
			m_tableUIVO.wallVO.singleCardWallInfo.Add(new HuanLeGameSo.TSingleCardWallInfo());
		}
		for (int j = 0; j < wallCardInfo.nCardWallNum; j++)
		{
			int viewSeatBySeat = roundModel.GetViewSeatBySeat(j);
			m_tableUIVO.wallVO.singleCardWallInfo[viewSeatBySeat] = wallCardInfo.astSingleCardWallInfo[j];
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
			{
				m_tableUIVO.wallVO.singleCardWallInfo[viewSeatBySeat].iCurrentTilesNum -= MJDef.INITIAL_MJ_COUNT;
			}
		}
	}

	private void HandleAddOneVarTileData(HuanLeGameSo.TAvatarSkillInfo stAvatarSkillInfo)
	{
		if (WrappersManagerImpl.IsPatched(36626))
		{
			WrappersManagerImpl.GetPatch(36626).__Gen_Wrap_1(this, stAvatarSkillInfo);
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
					base.dispatcher.Dispatch(CompareTileEvent.HandleRepeatedVarTile, tSingleAvatarSkill);
				}
			}
		}
	}

	private void ShowInfoInCardWall(HuanLeGameSo.TTilesShowInfoInCardWall showInfoInCardWall)
	{
		if (WrappersManagerImpl.IsPatched(36627))
		{
			WrappersManagerImpl.GetPatch(36627).__Gen_Wrap_1(this, showInfoInCardWall);
		}
		else
		{
			base.dispatcher.Dispatch(CompareTileEvent.ReplayAbilityWallTile, showInfoInCardWall);
		}
	}

	private void ShowWinTimes(HuanLeGameSo.TPlayerWinTimes winTimes)
	{
		if (WrappersManagerImpl.IsPatched(36628))
		{
			WrappersManagerImpl.GetPatch(36628).__Gen_Wrap_1(this, winTimes);
			return;
		}
		int num = 0;
		for (int i = 0; i < winTimes.SeatVictoryTimes.Length; i++)
		{
			if (roundModel.GetViewSeatBySeat(i) == 0)
			{
				num = winTimes.SeatVictoryTimes[i];
				break;
			}
		}
		if (num >= 3)
		{
			base.dispatcher.Dispatch(GameEvent.ShowHandTileFireEffect, 0);
		}
	}

	private void HandleCallBackTileInfo(HuanLeGameSo.TCallBackdTileSet info)
	{
		if (WrappersManagerImpl.IsPatched(36610))
		{
			WrappersManagerImpl.GetPatch(36610).__Gen_Wrap_1(this, info);
			return;
		}
		HuanLeGameSo.TCallBackdTileInfo tCallBackdTileInfo = null;
		int num = -1;
		int num2 = -1;
		List<ActionRecordVO> actionRecordList = roundModel.GetActionRecordList();
		for (int i = 0; i < info.nCallBackdTileInfoNum; i++)
		{
			tCallBackdTileInfo = info.astCallBackdTileInfo[i];
			Log.Info($"CallBackInfoTileInfo {i}, from {tCallBackdTileInfo.nFromSeat} to {tCallBackdTileInfo.nDstSeat}, tile {tCallBackdTileInfo.chTile}", "Game");
			num = roundModel.GetViewSeatBySeat(tCallBackdTileInfo.nFromSeat);
			num2 = roundModel.GetViewSeatBySeat(tCallBackdTileInfo.nDstSeat);
			for (int j = 0; j < actionRecordList.Count; j++)
			{
				if (actionRecordList[j].mjValue == tCallBackdTileInfo.chTile && GameUtil.IsActionTypeGang(actionRecordList[j].type) && actionRecordList[j].viewSeat == num)
				{
					actionRecordList[j].viewSeat = num2;
					roundModel.GetPlayerGameInfoByViewSeat(num2).ExtraCountNum *= 2;
					break;
				}
			}
			roundModel.GetTilesByViewSeat(num).HuJiaoZhuangGangValueList.Add(tCallBackdTileInfo.chTile);
		}
	}

	protected void HandleFangPao(HuanLeGameSo.TDianPaoHuTiles info)
	{
		if (WrappersManagerImpl.IsPatched(36602))
		{
			WrappersManagerImpl.GetPatch(36602).__Gen_Wrap_1(this, info);
		}
	}

	protected void HandleYiPaoDuoXiang(HuanLeGameSo.TMorePaoHuTiles info)
	{
		if (WrappersManagerImpl.IsPatched(36603))
		{
			WrappersManagerImpl.GetPatch(36603).__Gen_Wrap_1(this, info);
			return;
		}
		for (int i = 0; i < info.nPaoTilesNum; i++)
		{
			HuanLeGameSo.TMoreThanOnePaoInfo tMoreThanOnePaoInfo = info.astPaoInfo[i];
			Log.Info($"Replay, HandleYiPaoDuoXiang i:{i}, nNum = {tMoreThanOnePaoInfo.nNum}, chTiles = {tMoreThanOnePaoInfo.chTiles}, szWhoWinLen = {tMoreThanOnePaoInfo.szWhoWin.Length}", "Game");
			for (int j = 0; j < tMoreThanOnePaoInfo.nNum; j++)
			{
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(tMoreThanOnePaoInfo.szWhoWin[j]);
				MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat);
				for (int k = 0; k < tilesByViewSeat.YiPaoDuoXiangHuList.Count && k < tilesByViewSeat.huList.Count; k++)
				{
					if (j == 0)
					{
						if (tilesByViewSeat.YiPaoDuoXiangHuList[k] == MJDef.YIPAODUOXIANG_Hu && tilesByViewSeat.huList[k] == tMoreThanOnePaoInfo.chTiles)
						{
							tilesByViewSeat.YiPaoDuoXiangHuList[k] = MJDef.YIPAODUOXIANG_FirstHu;
							break;
						}
					}
					else if (tilesByViewSeat.YiPaoDuoXiangHuList[k] == MJDef.YIPAODUOXIANG_Hu && tilesByViewSeat.huList[k] == tMoreThanOnePaoInfo.chTiles)
					{
						tilesByViewSeat.YiPaoDuoXiangHuList[k] = MJDef.YIPAODUOXIANG_OtherHu;
						break;
					}
				}
			}
		}
	}

	protected void AddBuyHorseProcess(HuanLeGameSo.TPlayerMaInfo playerMaInfo)
	{
		if (WrappersManagerImpl.IsPatched(36600))
		{
			WrappersManagerImpl.GetPatch(36600).__Gen_Wrap_1(this, playerMaInfo);
			return;
		}
		((MaimaDataModel)roundModel.GetModel("MaimaModuleName")).tPlayerMaInfo = playerMaInfo;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.BuyHorseProcess;
		procedureVO.data = playerMaInfo;
		procedureVO.maxTime = 15;
		roundModel.rePaly = true;
		processModel.PushProcedure(procedureVO);
	}

	protected void AddBuyHorseResultProcess(HuanLeGameSo.TPlayerMaResult playerMaResult)
	{
		if (WrappersManagerImpl.IsPatched(36601))
		{
			WrappersManagerImpl.GetPatch(36601).__Gen_Wrap_1(this, playerMaResult);
		}
		else
		{
			((MaimaDataModel)roundModel.GetModel("MaimaModuleName")).tPlayerMaResult = playerMaResult;
		}
	}

	protected void AddWinTipsInfo()
	{
		if (WrappersManagerImpl.IsPatched(36591))
		{
			WrappersManagerImpl.GetPatch(36591).__Gen_Wrap_0(this);
		}
		else if (roundModel.chMinWinFan > 0)
		{
			if (roundModel.gameType == HappyMahjong.Common.GameType.Guobiao || roundModel.gameType == HappyMahjong.Common.GameType.RealGuoBiao)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(LanguageKey.THIS_MATCH_HAS_BEEN_A_LONG_TIME_COMING);
				stringBuilder.Append((int)roundModel.chMinWinFan);
				stringBuilder.Append(LanguageKey.SOME);
				SingletonBase<Toast>.GetInstance().showUGUI(stringBuilder.ToString(), 3f, 1);
			}
			else
			{
				base.dispatcher.Dispatch(GameEvent.ShowHelpTips, HelpTipsData.CreateData(LanguageKey.NOT_ENOUGH_HUFAN_WITH_MY_FAMILY));
			}
		}
	}

	public void AddFixConfigInfo(HuanLeGameSo.TFixConfigInfo tFixConfigInfo)
	{
		if (WrappersManagerImpl.IsPatched(36570))
		{
			WrappersManagerImpl.GetPatch(36570).__Gen_Wrap_1(this, tFixConfigInfo);
			return;
		}
		roundModel.chMinWinFan = tFixConfigInfo.chMinWinFan;
		roundModel.ticketBean = Util.GetInt64OrInt32(tFixConfigInfo.llTicketV2, tFixConfigInfo.iTicket);
		roundModel.fengDingBeiCount = tFixConfigInfo.iFengDing;
		if (!CSReplayRecordService.sIsReplaying())
		{
			roundModel.softHuFengDingBeiCount = tFixConfigInfo.iSoftFengding;
			roundModel.hardHuFengDingBeiCount = tFixConfigInfo.iHardFengding;
			roundModel.bigHuFengDingBeiCount = tFixConfigInfo.iDaHuFengDing;
			roundModel.topHuFengDingBeiCount = tFixConfigInfo.iDingHuFengDing;
		}
		if (roundModel.gameType == HappyMahjong.Common.GameType.ModuleCompareTile && SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleCompareTile) is CompareTileDataModel compareTileDataModel)
		{
			compareTileDataModel.HasParallelQiYuRoundFunc = tFixConfigInfo.iHasParallelQiYuRoundFunc != 0;
			compareTileDataModel.ParallelQiYuTileIndices.Clear();
			for (int i = 0; i < tFixConfigInfo.nParallelQiYuTileNum; i++)
			{
				int item = tFixConfigInfo.ParallelQiYuTileIdx[i];
				compareTileDataModel.ParallelQiYuTileIndices.Add(item);
			}
			Log.Info($"CompareTile GameReplay Enable ParallelQiYuRoundFunc: {compareTileDataModel.HasParallelQiYuRoundFunc}, TileIndices Count: {compareTileDataModel.ParallelQiYuTileIndices.Count}", "Game");
		}
		roundModel.zhaNiaoWinType = tFixConfigInfo.nZhongniaoTyle;
		roundModel.horseUseNewSetTiles = tFixConfigInfo.chHorseUseNewSetTiles != 0;
		roundModel.sectionBaseBet = (float)Util.GetInt64OrInt32(tFixConfigInfo.llSectionBaseBetV2, tFixConfigInfo.iSectionBaseBet) / (float)((tFixConfigInfo.iNewSectionBasebetFactor == 0) ? 1 : tFixConfigInfo.iNewSectionBasebetFactor);
		roundModel.sectionAddition = tFixConfigInfo.iSectionAddtion;
		roundModel.sectionFengDingBei = tFixConfigInfo.iSectionFengDingBei;
		roundModel.sectionFengDingScore = tFixConfigInfo.iSectionFengDingScore;
		roundModel.baseBet = Util.GetInt64OrInt32(tFixConfigInfo.llBaseBetV2, tFixConfigInfo.iBaseBet);
		roundModel.teamBaseBet = Util.GetInt64OrInt32(tFixConfigInfo.llTeamBaseBetV2, tFixConfigInfo.iTeamBaseBet);
		roundModel.teamFengDing = tFixConfigInfo.iTeamFengDing;
		roundModel.RoomTicketType = (HuanLeGameSo.TicketType)tFixConfigInfo.iTicketType;
		roundModel.NewWinSectionFengDing = tFixConfigInfo.iNewSectionWinFengdingScore;
		roundModel.NewLoseSectionFengDing = tFixConfigInfo.iNewSectionLoseFengdingScore;
		roundModel.WinBeanUpLimitMinValue = tFixConfigInfo.llWinBeanUpLimitMinValue;
		roundModel.iWarmReason = tFixConfigInfo.iWarmReason;
		roundModel.JiHuZiMo = tFixConfigInfo.iJiHuOnlyZimo == 1;
		PlayerDataMgr.GetInstance().GetSelfInfo().IsFriendAssist = tFixConfigInfo.iIsInFriendAssist == 1;
		base.dispatcher.Dispatch(GameEvent.SetUserFriendAssistStatus, new KeyValuePair<int, bool>(0, tFixConfigInfo.iIsInFriendAssist == 1));
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleFanRecommend) is NewFanRecommendDataModel newFanRecommendDataModel)
		{
			newFanRecommendDataModel.JihuZimo = tFixConfigInfo.iJiHuOnlyZimo == 1;
		}
		XueLiuBaoPaiDataModel xueLiuBaoPaiDataModel = (XueLiuBaoPaiDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel("XueLiuBaoPaiModuleName");
		if (xueLiuBaoPaiDataModel != null)
		{
			xueLiuBaoPaiDataModel.RoundEndCompareTileBeginLastNTile = tFixConfigInfo.iRoundEndCompareTileBeginLastNTile;
		}
		if (roundModel.gameType == HappyMahjong.Common.GameType.ChangSha)
		{
			((ChangShaRoundModel)roundModel).m_gangYaoTileNum = tFixConfigInfo.nGangyaoTileNum;
			Log.Info("Replay AddFixConfigInfo, tFixConfigInfo.nGangyaoTileNum is: " + tFixConfigInfo.nGangyaoTileNum, "Game");
		}
		if (roundModel.gameType == HappyMahjong.Common.GameType.ModuleHongZhongXueLiu || roundModel.gameType == HappyMahjong.Common.GameType.Module2V2HongZhongXueLiu)
		{
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleHongZhongXueLiuUpdate) is NewHongZhongDataModel newHongZhongDataModel)
			{
				newHongZhongDataModel.isChoosingTileEnabled = tFixConfigInfo.iHasFaCaiGang == 1;
			}
			Log.Info($"NewHongZhongXueLiu Enable FaCaiSelectTile: {tFixConfigInfo.iHasFaCaiGang == 1}", "Game");
		}
		Log.Info("Replay AddFixConfigInfo " + $"tFixConfigInfo.iFengDing: {tFixConfigInfo.iFengDing}, " + $"EntranceMode is: {tFixConfigInfo.nEntranceMode}, " + $"chMinWinFan is: {tFixConfigInfo.chMinWinFan}, " + $"iIsInFriendAssist: {tFixConfigInfo.iIsInFriendAssist}", "Game");
		if (tFixConfigInfo.chHaveZipai == 0)
		{
			MJDef.GUANGDONG_TOTAL_MJ_COUNT = 108;
		}
		else
		{
			MJDef.GUANGDONG_TOTAL_MJ_COUNT = 136;
		}
		base.dispatcher.Dispatch(GameEvent.ReplaysUpdateUIInfos, 1);
	}

	protected void HandleDapiao(HuanLeGameSo.TShowScoreInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36586))
		{
			WrappersManagerImpl.GetPatch(36586).__Gen_Wrap_1(this, info);
			return;
		}
		Log.Info("Replay, HandleDapiao", "Game");
		roundModel.isDapiaoRoom = true;
		roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		int seatByViewSeat = roundModel.GetSeatByViewSeat(0);
		if (seatByViewSeat >= info.nSitNum)
		{
			Log.Debug("replay Dapiao error sitnum:" + info.nSitNum + "local seat:" + seatByViewSeat, "Game");
			return;
		}
		if (info.ScoreInfo[seatByViewSeat] == -1)
		{
			Log.Info("Replay, HandleDapiao ShowSelfDapiao", "Game");
			roundModel.GetPlayerGameInfoByViewSeat(0).isSelfDapiao = true;
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.DaPiaoProcess;
			procedureVO.data = info;
			m_todoProcedure = procedureVO;
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.DaPiaoQiangZhuang)
			{
				base.dispatcher.Dispatch(GameEvent.ShowDeskDirections, false);
			}
			return;
		}
		roundModel.GetPlayerGameInfoByViewSeat(0).isSelfDapiao = false;
		for (int i = 0; i < info.nSitNum; i++)
		{
			if (roundModel.GetViewSeatBySeat(i) != 0 && info.ScoreInfo[i] == -1)
			{
				Log.Info("Replay, HandleDapiao ReplayOnWaitOtherDapiao", "Game");
				int num = info.ScoreInfo[seatByViewSeat];
				base.dispatcher.Dispatch(GameEvent.ReplayOnWaitOtherDapiao, num);
				if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.DaPiaoQiangZhuang)
				{
					base.dispatcher.Dispatch(GameEvent.ShowDeskDirections, false);
				}
				return;
			}
		}
		if (CSReplayRecordService.sIsReplaying())
		{
			ProcedureVO procedureDelayAndDel = processModel.GetProcedureDelayAndDel(ProcessType.DealProcessReplayShowMJValue);
			if (procedureDelayAndDel != null)
			{
				processModel.PushProcedure(procedureDelayAndDel);
			}
		}
		Log.Info("Replay, HandleDapiao ReplayOnResetDapiao", "Game");
		base.dispatcher.Dispatch(GameEvent.ReplayOnResetDapiao, info);
	}

	protected void HandleDingque(HuanLeGameSo.TDingque info)
	{
		if (WrappersManagerImpl.IsPatched(36587))
		{
			WrappersManagerImpl.GetPatch(36587).__Gen_Wrap_1(this, info);
		}
		else
		{
			if (((HuanSanZhangDataModel)roundModel.GetModel("HuanSanZhangModuleName")).InChange3MJProcess)
			{
				return;
			}
			int seatByViewSeat = roundModel.GetSeatByViewSeat(0);
			if (seatByViewSeat >= info.nSitNum)
			{
				Log.Debug("replay dingque error sitnum:" + info.nSitNum + "local seat:" + seatByViewSeat, "Game");
			}
			else
			{
				if (info.szDingqueType[seatByViewSeat] == -1 && info.chDfDingQue < 0)
				{
					return;
				}
				if (info.szDingqueType[seatByViewSeat] == -1)
				{
					Log.Info("On Replay show self ignore", "Game");
					Delete14thMJ();
					ProcedureVO procedureVO = new ProcedureVO();
					procedureVO.type = ProcessType.ShowSelfIgnoreProcess;
					procedureVO.data = new DingqueProcessData
					{
						DefaultIgnoreType = info.chDfDingQue,
						CountTime = info.nMaxShowTime
					};
					m_todoProcedure = procedureVO;
					return;
				}
				int num = info.szDingqueType[seatByViewSeat];
				IgnoreDataModel ignoreDataModel = (IgnoreDataModel)roundModel.GetModel("IgnoreModuleName");
				ignoreDataModel.m_ignoreKind[0] = num;
				ignoreDataModel.m_ignoreType = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.ignoreType;
				if (roundModel.IsMingPai)
				{
					for (int i = 1; i < 4; i++)
					{
						int seatByViewSeat2 = roundModel.GetSeatByViewSeat(i);
						ignoreDataModel.m_ignoreKind[i] = info.szDingqueType[seatByViewSeat2];
					}
				}
				for (int j = 0; j < info.nSitNum; j++)
				{
					if (roundModel.GetViewSeatBySeat(j) != 0 && info.szDingqueType[j] == -1)
					{
						int num2 = info.szDingqueType[seatByViewSeat];
						base.dispatcher.Dispatch(GameEvent.ReplayOnWaitOtherIgnore, num2);
						return;
					}
				}
				base.dispatcher.Dispatch(GameEvent.ReplayOnResetIgnore, info);
			}
		}
	}

	protected virtual void SetRoundData(HuanLeGameSo.TRoundInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36571))
		{
			WrappersManagerImpl.GetPatch(36571).__Gen_Wrap_1(this, info);
			return;
		}
		Log.Info("Replay, SetRoundData", "Game");
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(info.chDealer);
		roundModel.dealerViewSeat = viewSeatBySeat;
		roundModel.roundWind = info.chRoundWind;
		roundModel.subRound = info.chSubRoundWind;
		roundModel.leftMJNumInWall = (byte)info.chLeftTileNum;
		roundModel.curGiveOutMJValue = info.stLastGive.chTile;
		roundModel.lastDropPlayerViewSeat = roundModel.GetViewSeatBySeat(info.stLastGive.chSeatID);
		if (globalModel.enterGameInfo.enterGameSecretRoomVO.isSecretRoom)
		{
			globalModel.enterGameInfo.enterGameSecretRoomVO.isStartIngGame = true;
		}
		m_tableUIVO.wallVO.wallType = globalModel.enterGameInfo.gameType;
		m_tableUIVO.wallVO.viewSeatOfZeroSeat = roundModel.GetViewSeatBySeat(0);
		m_tableUIVO.wallVO.wallLeftNum = (byte)info.chLeftTileNum;
		if (!SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
		{
			if (NeedShowRealDrawWallCardNum(roundModel.gameType))
			{
				base.dispatcher.Dispatch(GameEvent.UpdateMJCountTips, m_tableUIVO.wallVO.wallLeftNum - roundModel.lastNotDrawTilesNum);
			}
			else
			{
				base.dispatcher.Dispatch(GameEvent.UpdateMJCountTips, m_tableUIVO.wallVO.wallLeftNum);
			}
		}
		if (roundModel.ShowRoundWind)
		{
			base.dispatcher.Dispatch(GameEvent.ShowQuanFeng, roundModel.roundWind);
		}
		m_tableUIVO.dealerViewSeat = viewSeatBySeat;
		m_tableUIVO.dies1 = info.Dice[0];
		m_tableUIVO.dies2 = info.Dice[1];
		roundModel.dicesList.Add(info.Dice[0]);
		roundModel.dicesList.Add(info.Dice[1]);
		switch ((HuanLeGameSo.ENM_ACTION_TYPE)info.stLastAction.chAction)
		{
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_EMPTY:
			roundModel.lastGiveAndBlock = UserActionType.Drop;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_COLLECT:
			roundModel.lastGiveAndBlock = UserActionType.Chi;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_TRIPLET:
			roundModel.lastGiveAndBlock = UserActionType.Peng;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET:
			roundModel.lastGiveAndBlock = UserActionType.MingGang;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET_REVEALED:
			roundModel.lastGiveAndBlock = UserActionType.BuGang;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET_CONCEALED:
			roundModel.lastGiveAndBlock = UserActionType.AnGang;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_FANGPAO_HU:
			roundModel.lastGiveAndBlock = UserActionType.DianHu;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_ZIMO_HU:
			roundModel.lastGiveAndBlock = UserActionType.Hu;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_SPECIAL_TILE:
			roundModel.lastGiveAndBlock = UserActionType.DropSpecial;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_EGG_ANGANG:
			roundModel.lastGiveAndBlock = UserActionType.MingDan;
			break;
		case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_EGG_BUGANG:
			roundModel.lastGiveAndBlock = UserActionType.BuDan;
			break;
		}
		roundModel.lastActorViewSeat = roundModel.GetViewSeatBySeat(info.stLastAction.chSeatID);
		roundModel.lastGiveOrBlockMJValue = info.stLastAction.chTile;
		PlayerGameInfoVO playerGameInfoByViewSeat = roundModel.GetPlayerGameInfoByViewSeat(roundModel.lastActorViewSeat);
		playerGameInfoByViewSeat.lastGiveAndBlock = roundModel.lastGiveAndBlock;
		int num = roundModel.GetPlayerCount();
		if (globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.ErRen)
		{
			num = 2;
		}
		for (int i = 0; i < num; i++)
		{
			HuanLeGameSo.TMJPlayerInfo tMJPlayerInfo = info.astPlayerInfo[i];
			int viewSeatBySeat2 = roundModel.GetViewSeatBySeat(tMJPlayerInfo.nSeat);
			playerGameInfoByViewSeat = roundModel.GetPlayerGameInfoByViewSeat(viewSeatBySeat2);
			Log.Info("Replay, viewSeat:" + viewSeatBySeat2 + " handNum:" + tMJPlayerInfo.nHandTilesNum + "status:" + tMJPlayerInfo.chStatus, "Game");
			m_playerTilsCount[viewSeatBySeat2] = tMJPlayerInfo.nHandTilesNum;
			if (viewSeatBySeat2 != 0)
			{
				MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat2);
				for (int j = 0; j < tMJPlayerInfo.nHandTilesNum; j++)
				{
					tilesByViewSeat.AddPaiInHand(MJDef.MJ_BACK);
				}
			}
			if (tMJPlayerInfo.chStatus == 4)
			{
				playerGameInfoByViewSeat.tingStatus = TingType.Concealed;
				m_bInTingStatus = true;
			}
			else if (tMJPlayerInfo.chStatus == 6)
			{
				playerGameInfoByViewSeat.tingStatus = TingType.GiveUp;
			}
			else if (tMJPlayerInfo.chStatus == 7)
			{
				playerGameInfoByViewSeat.tingStatus = TingType.Leave;
			}
			else if (tMJPlayerInfo.chStatus == 5)
			{
				playerGameInfoByViewSeat.tingStatus = TingType.Win;
			}
			playerGameInfoByViewSeat.chTingFlag = tMJPlayerInfo.chFlag;
		}
	}

	private bool NeedShowRealDrawWallCardNum(HappyMahjong.Common.GameType gameType)
	{
		if (WrappersManagerImpl.IsPatched(36572))
		{
			return WrappersManagerImpl.GetPatch(36572).__Gen_Wrap_2881(this, gameType);
		}
		if (gameType == HappyMahjong.Common.GameType.ModuleRichTile || gameType == HappyMahjong.Common.GameType.ModuleDragon)
		{
			return true;
		}
		return false;
	}

	protected virtual void SetPlayerHandTile(HuanLeGameSo.THandTiles info)
	{
		if (WrappersManagerImpl.IsPatched(36573))
		{
			WrappersManagerImpl.GetPatch(36573).__Gen_Wrap_1(this, info);
			return;
		}
		MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(0);
		Log.Info("SetPlayerHandTile nHandTilesNum " + info.nHandTilesNum + " inHandListCount " + tilesByViewSeat.inHandList.Count, "Game");
		if (tilesByViewSeat.inHandList.Count <= 0 && info.nHandTilesNum > 0)
		{
			for (int i = 0; i < info.nHandTilesNum; i++)
			{
				tilesByViewSeat.inHandList.Add(info.szTiles[i]);
			}
			tilesByViewSeat.LastMJ = info.szTiles[info.nHandTilesNum - 1];
			tilesByViewSeat.curRedrawMjValue = tilesByViewSeat.LastMJ;
			Log.Info("SetPlayerHandTile info:" + Util.Stringify(info.szTiles), "Game");
			if (info.nHandTilesNum == MJDef.MAX_IN_HAND_COUNT)
			{
				m_14thMJ = info.szTiles[MJDef.MAX_IN_HAND_COUNT - 1];
			}
		}
	}

	private void SetOthersHandTileAfterSelfTingInReplay(HuanLeGameSo.THandTiles handTiles, SetUserPaiReason reason)
	{
		if (WrappersManagerImpl.IsPatched(36644))
		{
			WrappersManagerImpl.GetPatch(36644).__Gen_Wrap_3504(this, handTiles, reason);
		}
		else if (CSReplayRecordService.IsObStatus())
		{
			SetOthersHandTileAfterSelfTing(handTiles, reason, 0, null, 0);
		}
	}

	private void SetOthersHandTileAfterSelfTing(HuanLeGameSo.THandTiles handTiles, SetUserPaiReason reason, int nTingTilesNum = 0, sbyte[] szTingTiles = null, sbyte chCanGiveTingTile = 0)
	{
		if (WrappersManagerImpl.IsPatched(36642))
		{
			WrappersManagerImpl.GetPatch(36642).__Gen_Wrap_3503(this, handTiles, reason, nTingTilesNum, szTingTiles, chCanGiveTingTile);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(handTiles.nSeat);
		sbyte[] szTiles = handTiles.szTiles;
		int nHandTilesNum = handTiles.nHandTilesNum;
		if (viewSeatBySeat > 3 || viewSeatBySeat < 0)
		{
			Log.Error("Illegal viewSeat. playerSeat:" + viewSeatBySeat, "Game");
			return;
		}
		string text = "[";
		for (int i = 0; i < handTiles.nHandTilesNum; i++)
		{
			text += handTiles.szTiles[i];
			if (i != handTiles.nHandTilesNum - 1)
			{
				text += ",";
			}
		}
		text += "]";
		Log.Info("SetOthersHandTileAfterSelfTing num:" + handTiles.nHandTilesNum + " tiles:" + text, "Game");
		if (viewSeatBySeat != 0)
		{
			MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat);
			tilesByViewSeat.inHandList.Clear();
			int num = ((reason == SetUserPaiReason.Teammate) ? m_playerTilsCount[viewSeatBySeat] : (m_playerTilsCount[viewSeatBySeat] - nHandTilesNum));
			for (int j = 0; j < num; j++)
			{
				tilesByViewSeat.AddPaiInHand(MJDef.MJ_BACK);
			}
			if (reason != SetUserPaiReason.Teammate)
			{
				for (int k = 0; k < nHandTilesNum; k++)
				{
					tilesByViewSeat.AddPaiInHand(szTiles[k]);
				}
			}
			if (nHandTilesNum > 0)
			{
				tilesByViewSeat.LastMJ = szTiles[nHandTilesNum - 1];
			}
		}
		List<int> list = new List<int>();
		for (int l = nHandTilesNum; l < m_playerTilsCount[viewSeatBySeat]; l++)
		{
			list.Add(MJDef.MJ_BACK);
		}
		for (int m = 0; m < nHandTilesNum; m++)
		{
			list.Add(szTiles[m]);
		}
		UserPaiVO userPaiVO = new UserPaiVO();
		userPaiVO.viewSeat = viewSeatBySeat;
		userPaiVO.mjList.handpaiList = list;
		userPaiVO.reason = reason;
		if (roundModel.gameSubType == GameSubType.DaZhongZhaNiao && (roundModel.gameType == HappyMahjong.Common.GameType.Guobiao || roundModel.gameType == HappyMahjong.Common.GameType.RealGuoBiao))
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.SetUserPai;
			procedureVO.data = userPaiVO;
			procedureVO.maxTime = 5;
			processModel.PushProcedure(procedureVO);
			return;
		}
		if (nTingTilesNum != 0)
		{
			userPaiVO.tingList = new List<int>();
			for (int n = 0; n < nTingTilesNum && n < szTingTiles.Length; n++)
			{
				userPaiVO.tingList.Add(szTingTiles[n]);
			}
		}
		if (userPaiVO.tingList != null)
		{
			((ShowHandTilesDataModel)SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleShowHandTiles))?.SetTingList(userPaiVO.viewSeat, userPaiVO.tingList, chCanGiveTingTile == 1);
		}
		userPaiVO.bNeedShowTips = false;
		base.dispatcher.Dispatch(GameEvent.ShowHandTilesAfterTing, userPaiVO);
	}

	protected void SetPlayerAction(HuanLeGameSo.TSetTiles info)
	{
		if (WrappersManagerImpl.IsPatched(36575))
		{
			WrappersManagerImpl.GetPatch(36575).__Gen_Wrap_1(this, info);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(info.nSeat);
		MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat);
		int num = 0;
		if (globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.TuiDaoHu || globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.ZuoPaiTuiDaoHu)
		{
			roundModel.isTianhu = false;
		}
		else if (globalModel.enterGameInfo.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu && info.nSetTilesNum > 0)
		{
			roundModel.isTianhu = false;
		}
		for (int i = 0; i < info.nSetTilesNum; i++)
		{
			HuanLeGameSo.TSetInfo tSetInfo = info.astSetTiles[i];
			HuanLeGameSo.TSetGangInfo tSetGangInfo;
			if (info.astSetGangInfo != null && info.nGangInfoNum == info.nSetTilesNum)
			{
				tSetGangInfo = info.astSetGangInfo[i];
				num = ((tSetGangInfo.chGangNum == 0) ? 4 : tSetGangInfo.chGangNum);
			}
			else
			{
				tSetGangInfo = null;
				num = 4;
			}
			HuanLeGameSo.ENM_ACTION_TYPE chFlag = (HuanLeGameSo.ENM_ACTION_TYPE)tSetInfo.chFlag;
			if (roundModel.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu && !SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.gangRedrawFromWallFront && (chFlag == HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET_REVEALED || chFlag == HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET || chFlag == HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET_CONCEALED || chFlag == HuanLeGameSo.ENM_ACTION_TYPE.ACTION_SPECIAL_TILE))
			{
				m_removeFromTailNum++;
			}
			Log.Info("Replay, SetPlayerAction viewSeat:" + viewSeatBySeat + " type:" + chFlag.ToString() + " chTiles: " + tSetInfo.chTiles, "Game");
			switch (chFlag)
			{
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_COLLECT:
			{
				tilesByViewSeat.AddChi(tSetInfo.chTiles, roundModel.GetViewSeatBySeat(tSetInfo.chChair), tSetInfo.chIsTriggeredChengBao == 1);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.Chi;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET_REVEALED:
			{
				UserActionType origType = UserActionType.Peng;
				int origCount = 3;
				if (tSetGangInfo != null)
				{
					if (tSetGangInfo.chOrigSetFlag == 18)
					{
						origType = UserActionType.MingGang;
					}
					else if (tSetGangInfo.chOrigSetFlag == 19)
					{
						origType = UserActionType.AnGang;
					}
					origCount = tSetGangInfo.chOriginGangNum;
				}
				tilesByViewSeat.AddBuGang(tSetInfo.chTiles, roundModel.GetViewSeatBySeat(tSetInfo.chChair), num, origType, origCount);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.BuGang;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				actionRecordVO.extendValue = num;
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET:
			{
				tilesByViewSeat.AddMingGang(tSetInfo.chTiles, roundModel.GetViewSeatBySeat(tSetInfo.chChair), num, tSetInfo.chIsTriggeredChengBao == 1);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.MingGang;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				actionRecordVO.extendValue = num;
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_QUADRUPLET_CONCEALED:
			{
				tilesByViewSeat.AddAnGang(tSetInfo.chTiles, num);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.AnGang;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				actionRecordVO.extendValue = num;
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_BAO_PAI_ANGANG:
			{
				tilesByViewSeat.AddAnGang(tSetInfo.chTiles);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.BaoDan;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_TRIPLET:
			{
				tilesByViewSeat.AddPeng(tSetInfo.chTiles, roundModel.GetViewSeatBySeat(tSetInfo.chChair), tSetInfo.chIsTriggeredChengBao == 1);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.Peng;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_ZIMO_HU:
			{
				tilesByViewSeat.AddHu(tSetInfo.chTiles);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.Hu;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				roundModel.AddActionRecord(actionRecordVO);
				tilesByViewSeat.YiPaoDuoXiangHuList.Add(MJDef.YIPAODUOXIANG_FirstHu);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_FANGPAO_HU:
			{
				tilesByViewSeat.AddHu(tSetInfo.chTiles);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.DianHu;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				roundModel.AddActionRecord(actionRecordVO);
				tilesByViewSeat.YiPaoDuoXiangHuList.Add(MJDef.YIPAODUOXIANG_Hu);
				break;
			}
			case HuanLeGameSo.ENM_ACTION_TYPE.ACTION_SPECIAL_TILE:
			{
				if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsLaiziInPeng)
				{
					tilesByViewSeat.AddSpecialTile(tSetInfo.chTiles);
				}
				tilesByViewSeat.AddSpecialGangTile(tSetInfo.chTiles);
				ActionRecordVO actionRecordVO = new ActionRecordVO();
				actionRecordVO.viewSeat = viewSeatBySeat;
				actionRecordVO.type = UserActionType.DropSpecial;
				actionRecordVO.mjValue = tSetInfo.chTiles;
				actionRecordVO.targetViewSeat = roundModel.GetViewSeatBySeat(tSetInfo.chChair);
				roundModel.AddActionRecord(actionRecordVO);
				break;
			}
			}
		}
	}

	protected void SetPlayerGive(HuanLeGameSo.TGiveTiles info)
	{
		if (WrappersManagerImpl.IsPatched(36574))
		{
			WrappersManagerImpl.GetPatch(36574).__Gen_Wrap_1(this, info);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(info.nSeat);
		MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat);
		if ((globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.XuanLiu || globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.XuanZhan || globalModel.enterGameInfo.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu) && info.nTilesNum > 0)
		{
			roundModel.isTianhu = false;
		}
		else if (globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.TuiDaoHu || globalModel.enterGameInfo.gameType == HappyMahjong.Common.GameType.ZuoPaiTuiDaoHu)
		{
			roundModel.isTianhu = false;
		}
		for (int i = 0; i < info.nTilesNum; i++)
		{
			tilesByViewSeat.DoGive(info.szGivenTiles[i]);
		}
		Log.Info("Replay szGivenTiles: " + Util.Stringify(info.szGivenTiles), "Game");
	}

	protected void SetPlayerFlower(HuanLeGameSo.TFlowerInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36576))
		{
			WrappersManagerImpl.GetPatch(36576).__Gen_Wrap_1(this, info);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(info.nSeat);
		MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat);
		for (int i = 0; i < info.nTilesFlowerNum; i++)
		{
			tilesByViewSeat.AddFlower(info.szFlower[i]);
		}
		if (roundModel.gameType == HappyMahjong.Common.GameType.ModuleNanJing)
		{
			HuaVo huaVo = new HuaVo
			{
				viewSeat = viewSeatBySeat,
				flowerCount = tilesByViewSeat.flowerList.Count
			};
			base.dispatcher.Dispatch(GameEvent.UpdateHuaInfo, huaVo);
		}
	}

	private void RebuildInfoUI()
	{
		if (WrappersManagerImpl.IsPatched(36634))
		{
			WrappersManagerImpl.GetPatch(36634).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("Replay, RebuildInfoUI", "Game");
		base.dispatcher.Dispatch(GameEvent.RebuildInfoUI);
	}

	protected void RebuildTableUI()
	{
		if (WrappersManagerImpl.IsPatched(36635))
		{
			WrappersManagerImpl.GetPatch(36635).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("Replay, RebuildTableUI", "Game");
		DiesVO diesVO = new DiesVO();
		diesVO.dealerViewSeat = roundModel.dealerViewSeat;
		diesVO.nRoundNum = roundModel.roundWind;
		diesVO.nSubRoundNum = roundModel.subRound;
		diesVO.nMenFengNum = roundModel.menFeng;
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.DaPiaoQiangZhuang)
		{
			diesVO.nMenFengNum = roundModel.GetPlayerCount() - roundModel.dealerViewSeat;
		}
		base.dispatcher.Dispatch(GameEvent.SetRoundDirection, diesVO);
		List<MJTileData> list = new List<MJTileData>();
		for (int i = 0; i < 4; i++)
		{
			MJTileData copy = roundModel.GetTilesByViewSeat(i).GetCopy();
			if (i == 0 && m_needToSplitRedraw)
			{
				copy.DelPaiInHand(roundModel.curRedrawMJValue);
			}
			list.Add(copy);
		}
		m_tableUIVO.tileList = list;
		m_tableUIVO.removeFromTailNum = m_removeFromTailNum;
		base.dispatcher.Dispatch(GameEvent.RebuildTableUI, m_tableUIVO);
	}

	protected void AddEnterGiveProcess(int seat, sbyte flag = 0)
	{
		if (WrappersManagerImpl.IsPatched(36577))
		{
			WrappersManagerImpl.GetPatch(36577).__Gen_Wrap_3501(this, seat, flag);
			return;
		}
		EnterGiveVO enterGiveVO = new EnterGiveVO();
		enterGiveVO.playerViewSeat = roundModel.GetViewSeatBySeat(seat);
		enterGiveVO.flag = flag;
		if (seat == roundModel.selfSeat)
		{
			List<ChoiceDetailInfo> gangHuList = NextToDoHelper.GetGangHuList(m_data.nTLVNum, m_data.astTLVInfo, roundModel);
			enterGiveVO.GangHuList = gangHuList;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterGiveProcess;
		procedureVO.data = enterGiveVO;
		procedureVO.viewSeat = enterGiveVO.playerViewSeat;
		processModel.PushProcedure(procedureVO);
		roundModel.curSvrPlayerViewSeat = enterGiveVO.playerViewSeat;
		Log.Info("AddEnterGiveProcess seat:" + seat + " viewSeat:" + enterGiveVO.playerViewSeat, "Game");
	}

	protected void AddEnterBlockProcess(HuanLeGameSo.TDoBlockInfo blockInfo)
	{
		if (WrappersManagerImpl.IsPatched(36585))
		{
			WrappersManagerImpl.GetPatch(36585).__Gen_Wrap_1(this, blockInfo);
			return;
		}
		roundModel.SetRoundTime(blockInfo.nAllClientTime, blockInfo.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterBlockProcess;
		procedureVO.data = blockInfo;
		procedureVO.viewSeat = 0;
		processModel.PushProcedure(procedureVO);
		Log.Info("AddEnterBlockProcess", "Game");
	}

	protected void HandleWait(HuanLeGameSo.TDoWaitInfo doWaitInfo, int tlvCount, TSOTLVUNIT[] tlvInfo, TSOTLVUNIT tlvUnit)
	{
		if (WrappersManagerImpl.IsPatched(36578))
		{
			WrappersManagerImpl.GetPatch(36578).__Gen_Wrap_2886(this, doWaitInfo, tlvCount, tlvInfo, tlvUnit);
			return;
		}
		Log.Info("Replay, HandleWait WaitInfo.chFlag:" + doWaitInfo.chFlag, "Game");
		if (doWaitInfo.chFlag == 0)
		{
			AddEnterGiveProcess(doWaitInfo.chChair, doWaitInfo.chFlag);
		}
		else if (doWaitInfo.chFlag == 2)
		{
			AddEnterGiveProcess(doWaitInfo.chChair, doWaitInfo.chFlag);
		}
		else if (doWaitInfo.chFlag != 1)
		{
			if (doWaitInfo.chFlag == 3)
			{
				int viewSeatBySeat = roundModel.GetViewSeatBySeat(doWaitInfo.chChair);
				HandleZhaNiaoHu(viewSeatBySeat);
				AddEnterGiveProcess(doWaitInfo.chChair, doWaitInfo.chFlag);
			}
			else if (doWaitInfo.chFlag == 4)
			{
				AddEnterWaitDealHuProcess();
			}
			else if (doWaitInfo.chFlag != 5 && doWaitInfo.chFlag == 7)
			{
				AddEnterWaitGangYaoBlockProcess();
			}
		}
	}

	protected void HandlePlayerStatus()
	{
		if (WrappersManagerImpl.IsPatched(36647))
		{
			WrappersManagerImpl.GetPatch(36647).__Gen_Wrap_0(this);
			return;
		}
		Log.Info("Replay, HandlePlayerStatus", "Game");
		for (int i = 0; i < 4; i++)
		{
			PlayerGameInfoVO playerGameInfoByViewSeat = roundModel.GetPlayerGameInfoByViewSeat(i);
			if (playerGameInfoByViewSeat.tingStatus == TingType.Concealed || (playerGameInfoByViewSeat.chTingFlag != 0 && playerGameInfoByViewSeat.tingStatus != TingType.Win))
			{
				DetailData detailData = new DetailData();
				detailData.viewSeat = i;
				base.dispatcher.Dispatch(GameEvent.ShowTingIcon, detailData);
				if (i == 0)
				{
					base.dispatcher.Dispatch(GameEvent.SetInHandTingStatus);
					MJTileData copy = roundModel.GetTilesByViewSeat(0).GetCopy();
					if (m_needToSplitRedraw)
					{
						copy.DelPaiInHand(roundModel.curRedrawMJValue);
					}
					TingInfo tingInfo = null;
					if (roundModel.gameType < HappyMahjong.Common.GameType.ModuleTuidaohu)
					{
						tingInfo = mjRule.GetTingInfo(copy.inHandList);
						roundModel.currentTingInfo = tingInfo;
						base.dispatcher.Dispatch(GameEvent.ShowLightIcon, tingInfo != null);
					}
					else if (roundModel.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu)
					{
						ReplayCalcTing();
					}
				}
			}
			else if (playerGameInfoByViewSeat.tingStatus == TingType.Win)
			{
				if (i == 0)
				{
					roundModel.GetTilesByViewSeat(0).GetCopy().inHandList.Remove(roundModel.curRedrawMJValue);
					ReplayCalcTing();
				}
				base.dispatcher.Dispatch(GameEvent.SetHandMJFaceDown, i);
				if (roundModel.gameType == HappyMahjong.Common.GameType.XuanZhan || (roundModel.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu && SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.huEndGame == 2))
				{
					if (m_replayHuAniData != null)
					{
						base.dispatcher.Dispatch(GameEvent.ShowUserHu, new ShowHuStateVO
						{
							viewSeat = i,
							huAniData = m_replayHuAniData
						});
					}
					else
					{
						base.dispatcher.Dispatch(GameEvent.ShowUserHu, i);
					}
				}
			}
			else if (playerGameInfoByViewSeat.tingStatus == TingType.GiveUp)
			{
				base.dispatcher.Dispatch(GameEvent.UserGiveUp, i);
			}
			else if (playerGameInfoByViewSeat.tingStatus == TingType.Leave)
			{
				base.dispatcher.Dispatch(GameEvent.UserLeave, i);
				if (!NeedShowHuIconWhenLeave())
				{
					continue;
				}
				MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(i);
				if (tilesByViewSeat != null && tilesByViewSeat.huList != null && tilesByViewSeat.huList.Count > 0)
				{
					if (m_replayHuAniData != null)
					{
						base.dispatcher.Dispatch(GameEvent.ShowUserHu, new ShowHuStateVO
						{
							viewSeat = i,
							huAniData = m_replayHuAniData
						});
					}
					else
					{
						base.dispatcher.Dispatch(GameEvent.ShowUserHu, i);
					}
				}
			}
			else if (i == 0)
			{
				if (roundModel.gameType == HappyMahjong.Common.GameType.WuHan)
				{
					MJTileData copy2 = roundModel.GetTilesByViewSeat(0).GetCopy();
					TingInfo firstTingInfo = ((WHMJRule)mjRule).GetFirstTingInfo(copy2.inHandList);
					roundModel.currentTingInfo = firstTingInfo;
					base.dispatcher.Dispatch(GameEvent.ShowLightIcon, firstTingInfo != null);
				}
				else if (roundModel.gameType == HappyMahjong.Common.GameType.ChangSha)
				{
					MJTileData copy3 = roundModel.GetTilesByViewSeat(0).GetCopy();
					TingInfo firstTingInfo2 = ((ChangShaRule)mjRule).GetFirstTingInfo(copy3.inHandList);
					roundModel.currentTingInfo = firstTingInfo2;
					base.dispatcher.Dispatch(GameEvent.ShowLightIcon, firstTingInfo2 != null);
				}
				else if (roundModel.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu)
				{
					ReplayCalcTing();
				}
			}
		}
		if (GameUtil.IsAutoPlayEnabled(globalModel.enterGameInfo.roomInfo.sceneId))
		{
			base.dispatcher.Dispatch(GameEvent.ShowLightIcon, true);
		}
	}

	private void ReplayCalcTing()
	{
		if (WrappersManagerImpl.IsPatched(36649))
		{
			WrappersManagerImpl.GetPatch(36649).__Gen_Wrap_0(this);
			return;
		}
		int discardTile = 0;
		if (!iComRule.IsTingFromServer && m_needToSplitRedraw)
		{
			discardTile = roundModel.curRedrawMJValue;
		}
		else if (roundModel.curSvrPlayerViewSeat == 0)
		{
			bool flag = false;
			List<TingInfo> tingInfoListFromSvr = iComRule.GetTingInfoListFromSvr();
			if (tingInfoListFromSvr != null)
			{
				foreach (TingInfo item in tingInfoListFromSvr)
				{
					if (item.m_valueOut == 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				PlayerGameInfoVO playerGameInfoByViewSeat = roundModel.GetPlayerGameInfoByViewSeat(0);
				discardTile = GameUtil.GetLastTileByView(roundModel);
				if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.IsParallelDraw)
				{
					discardTile = playerGameInfoByViewSeat.tiles.curRedrawMjValue;
				}
			}
		}
		CalculateTingDO calculateTingDO = new CalculateTingDO
		{
			discardTile = discardTile,
			refreshLightIcon = true
		};
		base.dispatcher.Dispatch(GameEvent.CalculateTingAndShowCmd, calculateTingDO);
	}

	private bool NeedShowHuIconWhenLeave()
	{
		if (WrappersManagerImpl.IsPatched(36650))
		{
			return WrappersManagerImpl.GetPatch(36650).__Gen_Wrap_452(this);
		}
		if (roundModel.gameType != HappyMahjong.Common.GameType.ModuleRichTile && roundModel.gameType != HappyMahjong.Common.GameType.ModuleCompareTile)
		{
			return roundModel.gameType != HappyMahjong.Common.GameType.ModuleDragon;
		}
		return false;
	}

	protected void AddSelfJustRedraw()
	{
		if (WrappersManagerImpl.IsPatched(36637))
		{
			WrappersManagerImpl.GetPatch(36637).__Gen_Wrap_0(this);
		}
		else if (m_needToSplitRedraw)
		{
			RedrawMJVO redrawMJVO = new RedrawMJVO();
			redrawMJVO.viewSeat = 0;
			redrawMJVO.mjList = new List<int> { roundModel.curRedrawMJValue };
			redrawMJVO.needRemoveFromWall = false;
			redrawMJVO.isNormalOrder = true;
			redrawMJVO.needXMargin = true;
			redrawMJVO.showAnimation = false;
			redrawMJVO.sortAfterRedraw = false;
			redrawMJVO.isFlowerRedraw = false;
			redrawMJVO.isSingleCardWall = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.HasSingleWall;
			if (roundModel.isDapiaoRoom)
			{
				redrawMJVO.isLaydownFace = true;
			}
			else
			{
				redrawMJVO.isLaydownFace = false;
			}
			base.dispatcher.Dispatch(GameEvent.ShowRedrawAnimation, redrawMJVO);
		}
	}

	protected void HandleCharge(HuanLeGameSo.TRecharge data)
	{
		if (WrappersManagerImpl.IsPatched(36589))
		{
			WrappersManagerImpl.GetPatch(36589).__Gen_Wrap_1(this, data);
			return;
		}
		roundModel.SetRoundTime(data.nAllClientTime, data.nMaxShowTime);
		SCMJChargeVO sCMJChargeVO = new SCMJChargeVO();
		sCMJChargeVO.viewSeatList = new List<int>();
		sCMJChargeVO.countDown = data.nMaxShowTime;
		int nRechargeNum = data.nRechargeNum;
		for (int i = 0; i < nRechargeNum; i++)
		{
			int viewSeatBySeat = roundModel.GetViewSeatBySeat(data.szSeatId[i]);
			sCMJChargeVO.viewSeatList.Add(viewSeatBySeat);
			if (viewSeatBySeat == 0 && data.szNeedAskShareToReliefSvr[i] == 1)
			{
				sCMJChargeVO.NeedAskShareToReliefSvr = true;
			}
			Log.Info("Add Charge viewSeat:" + viewSeatBySeat, "Game");
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ChargeProcess;
		procedureVO.data = sCMJChargeVO;
		procedureVO.maxTime = 30;
		processModel.PushProcedure(procedureVO);
		Log.Info("AddChargeProcess", "Game");
	}

	public void HandleLaizi(HuanLeGameSo.TLaiZi data)
	{
		if (WrappersManagerImpl.IsPatched(36590))
		{
			WrappersManagerImpl.GetPatch(36590).__Gen_Wrap_1(this, data);
			return;
		}
		List<LaiziInfo> list = new List<LaiziInfo>();
		for (int i = 0; i < data.nLaiZiNum; i++)
		{
			int num = data.szLaiZi[i];
			if (roundModel.gameType == HappyMahjong.Common.GameType.WuHan)
			{
				((WHMJRoundModel)roundModel).m_nLaiziMJValue = num;
				((WHMJRoundModel)roundModel).m_laiziType = data.LaiZiType[i];
			}
			else if (roundModel.gameType == HappyMahjong.Common.GameType.GuangDong || (roundModel.gameType >= HappyMahjong.Common.GameType.TuiDaoHu && roundModel.gameType <= HappyMahjong.Common.GameType.HuNanZhuanZhuanZhang))
			{
				((GuangDongRoundModel)roundModel).m_nLaiziMJValue = num;
			}
			else if (roundModel.gameType >= HappyMahjong.Common.GameType.ModuleTuidaohu)
			{
				((LaiziDataModel)roundModel.GetModel("LaiziModuleName")).LaiziList.Add(num);
			}
			LaiziInfo laiziInfo = new LaiziInfo();
			laiziInfo.LaiziValue = num;
			laiziInfo.LaiziSourceValue = data.szLaiZiSource[i];
			list.Add(laiziInfo);
		}
		base.dispatcher.Dispatch(GameEvent.ResetLaizi, list);
		for (int j = 0; j < data.szLaiZiSource.Length; j++)
		{
			if (data.szLaiZiSource[j] > 0)
			{
				roundModel.GetTilesByViewSeat(roundModel.dealerViewSeat).DoInsertGiveAtFront(data.szLaiZiSource[j]);
				break;
			}
		}
	}

	public void AddPizi(HuanLeGameSo.TPiZi piziInfo)
	{
		if (WrappersManagerImpl.IsPatched(36599))
		{
			WrappersManagerImpl.GetPatch(36599).__Gen_Wrap_1(this, piziInfo);
		}
		else if (roundModel.gameType == HappyMahjong.Common.GameType.WuHan)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < piziInfo.nPiziNum; i++)
			{
				list.Add(piziInfo.szPizi[i]);
			}
			((WHMJRoundModel)roundModel).m_piList.Clear();
			((WHMJRoundModel)roundModel).m_piList = list;
		}
	}

	protected void HandlePassInfo(HuanLeGameSo.TPassTimesInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36595))
		{
			WrappersManagerImpl.GetPatch(36595).__Gen_Wrap_1(this, info);
			return;
		}
		GuoHuVO guoHuVO = new GuoHuVO();
		guoHuVO.passTimes = info.chTimes;
		guoHuVO.passMul = info.nPassMul;
		guoHuVO.maxPassTimes = info.chMaxTimes;
		guoHuVO.viewSeat = roundModel.GetViewSeatBySeat(info.chSeatId);
		guoHuVO.needAnimation = false;
		if (guoHuVO.passTimes != roundModel.GetPlayerGameInfoByViewSeat(guoHuVO.viewSeat).guoHuVO.passTimes)
		{
			roundModel.GetPlayerGameInfoByViewSeat(guoHuVO.viewSeat).guoHuVO = guoHuVO;
			base.dispatcher.Dispatch(GameEvent.ShowGuoHuInfo, guoHuVO);
		}
	}

	protected void HandleBetInfo(HuanLeGameSo.CBetInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36596))
		{
			WrappersManagerImpl.GetPatch(36596).__Gen_Wrap_1(this, info);
			return;
		}
		BetDetailVO betDetailVO = new BetDetailVO();
		HuanLeGameSo.CBetDetail cBetDetail = null;
		for (int i = 0; i < info.nBetNum; i++)
		{
			cBetDetail = info.astBetDetail[i];
			if (cBetDetail.iStatus == 1)
			{
				betDetailVO.Result = 0;
				betDetailVO.Status = cBetDetail.iStatus;
				betDetailVO.Id = cBetDetail.iId;
				betDetailVO.Donner = (int)cBetDetail.llDonner;
			}
		}
		roundModel.curBetInfo = betDetailVO;
	}

	protected void HandleZhaNiao(HuanLeGameSo.TDoZN info)
	{
		if (WrappersManagerImpl.IsPatched(36597))
		{
			WrappersManagerImpl.GetPatch(36597).__Gen_Wrap_1(this, info);
			return;
		}
		HandleZhaNiaoHu(0);
		roundModel.curPlayerViewSeat = 0;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.ZhaNiaoProcess;
		procedureVO.maxTime = 15;
		roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		m_todoProcedure = procedureVO;
	}

	protected void HandleDelayInfo(HuanLeGameSo.CDelayInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36598))
		{
			WrappersManagerImpl.GetPatch(36598).__Gen_Wrap_1(this, info);
			return;
		}
		for (int i = 0; i < info.nDelayNum; i++)
		{
			roundModel.leftDelayTimes = info.astDelayDetail[i].iTimes;
		}
	}

	protected void HandleChangeCard(HuanLeGameSo.TChangeCard tChangeCard)
	{
		if (WrappersManagerImpl.IsPatched(36592))
		{
			WrappersManagerImpl.GetPatch(36592).__Gen_Wrap_1(this, tChangeCard);
		}
		else
		{
			if (tChangeCard.chFlag != 2 && tChangeCard.chFlag != 3 && tChangeCard.chFlag != 1 && tChangeCard.chFlag != 4 && tChangeCard.chFlag != 5 && tChangeCard.chFlag != 6)
			{
				return;
			}
			if (SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.bSystemChangeCard)
			{
				HuanNZhangDataModel huanNZhangDataModel = (HuanNZhangDataModel)roundModel.GetModel("HuanNZhangModuleName");
				if (huanNZhangDataModel == null || tChangeCard.nOtherTileNum > 0)
				{
					return;
				}
				if (tChangeCard.nSelfTileNum > 0)
				{
					huanNZhangDataModel.InChangeNMJMode = false;
					huanNZhangDataModel.InChangeNMJProcess = false;
					roundModel.SetRoundTime(tChangeCard.nAllClientTime, tChangeCard.nMaxShowTime);
					SCMJChangeNMJVO sCMJChangeNMJVO = new SCMJChangeNMJVO();
					sCMJChangeNMJVO.gameType = roundModel.gameType;
					for (int i = 0; i < tChangeCard.nSelfTileNum; i++)
					{
						Log.Info("AddChangeNMJsProcess tile " + tChangeCard.szSelfTiles[i], "Game");
						sCMJChangeNMJVO.changeOutList.Add(tChangeCard.szSelfTiles[i]);
					}
					sCMJChangeNMJVO.time = (int)roundModel.GetTimeout().GetTotalTimeOut();
					sCMJChangeNMJVO.limitNum = tChangeCard.nLimitNum;
					ProcedureVO procedureVO = new ProcedureVO();
					procedureVO.type = ProcessType.ChangeNMJProcess;
					procedureVO.data = sCMJChangeNMJVO;
					procedureVO.maxTime = sCMJChangeNMJVO.time + 5;
					processModel.PushProcedure(procedureVO);
					Log.Info("Add ChangeNMJProcess", "Game");
					base.dispatcher.Dispatch(GameEvent.ReplayShowSelfChangeNMJResult, sCMJChangeNMJVO.changeOutList);
				}
				else
				{
					huanNZhangDataModel.InChangeNMJMode = true;
					huanNZhangDataModel.InChangeNMJProcess = true;
					roundModel.SetRoundTime(tChangeCard.nAllClientTime, tChangeCard.nMaxShowTime);
					SCMJChangeNMJVO sCMJChangeNMJVO2 = new SCMJChangeNMJVO();
					sCMJChangeNMJVO2.gameType = roundModel.gameType;
					for (int j = 0; j < tChangeCard.nDfTileNum; j++)
					{
						Log.Info("AddChangeNMJsProcess tile " + tChangeCard.szDfTiles[j], "Game");
						sCMJChangeNMJVO2.changeOutList.Add(tChangeCard.szDfTiles[j]);
					}
					sCMJChangeNMJVO2.time = (int)roundModel.GetTimeout().GetTotalTimeOut();
					sCMJChangeNMJVO2.limitNum = tChangeCard.nLimitNum;
					sCMJChangeNMJVO2.ConverChangeOutRecommendTilesList(tChangeCard);
					ProcedureVO procedureVO2 = new ProcedureVO();
					procedureVO2.type = ProcessType.ChangeNMJProcess;
					procedureVO2.data = sCMJChangeNMJVO2;
					procedureVO2.maxTime = sCMJChangeNMJVO2.time + 5;
					processModel.PushProcedure(procedureVO2);
					Log.Info("Add ChangeNMJProcess", "Game");
				}
				return;
			}
			HuanSanZhangDataModel huanSanZhangDataModel = (HuanSanZhangDataModel)roundModel.GetModel("HuanSanZhangModuleName");
			huanSanZhangDataModel.InChange3MJMode = true;
			Log.Info("Replay HandleChangeCard tChangeCard.nOtherTileNum " + tChangeCard.nOtherTileNum + " tChangeCard.nSelfTileNum " + tChangeCard.nSelfTileNum, "Game");
			if (tChangeCard.nOtherTileNum == 3)
			{
				return;
			}
			huanSanZhangDataModel.InChange3MJProcess = true;
			Delete14thMJ();
			if (tChangeCard.nSelfTileNum == 3)
			{
				SCMJChangeMJVO vo = new SCMJChangeMJVO();
				for (int k = 0; k < tChangeCard.nSelfTileNum; k++)
				{
					vo.changeOutList.Add(tChangeCard.szSelfTiles[k]);
				}
				huanSanZhangDataModel.ChangeMJVo = vo;
				VPTimer.In(0f, delegate
				{
					base.dispatcher.Dispatch(GameEvent.ReplayShowSelfChangeMJResult, vo.changeOutList);
				});
				ProcedureVO procedureVO3 = new ProcedureVO();
				procedureVO3.type = ProcessType.Change3MJProcess;
				procedureVO3.data = vo;
				vo.Replay = true;
				procedureVO3.maxTime = 16;
				m_todoProcedure = procedureVO3;
				Log.Info("Add Change3MJProcess", "Game");
				if (!Util.IsTeamBattlePlaying((int)globalModel.enterGameInfo.gameType))
				{
					return;
				}
				List<int> list = new List<int>();
				short nTeamDefinedChgCardNum = tChangeCard.nTeamDefinedChgCardNum;
				if (nTeamDefinedChgCardNum > 0)
				{
					for (int num = 0; num < tChangeCard.astTeamChangeTiles[0].nChgTileNum; num++)
					{
						list.Add(tChangeCard.astTeamChangeTiles[0].szChgTiles[num]);
					}
				}
				KeyValuePair<short, List<int>> key = new KeyValuePair<short, List<int>>(nTeamDefinedChgCardNum, list);
				KeyValuePair<KeyValuePair<short, List<int>>, List<int>> keyValuePair = new KeyValuePair<KeyValuePair<short, List<int>>, List<int>>(key, vo.changeOutList);
				base.dispatcher.Dispatch(GameEvent.RecvGameReplayH3Z, keyValuePair);
				return;
			}
			roundModel.SetRoundTime(tChangeCard.nAllClientTime, tChangeCard.nMaxShowTime);
			SCMJChangeMJVO sCMJChangeMJVO = new SCMJChangeMJVO();
			for (int num2 = 0; num2 < tChangeCard.nDfTileNum; num2++)
			{
				Log.Info("AddChange3MJsProcess tile " + tChangeCard.szDfTiles[num2], "Game");
				sCMJChangeMJVO.changeOutList.Add(tChangeCard.szDfTiles[num2]);
			}
			sCMJChangeMJVO.time = tChangeCard.nAllClientTime;
			ProcedureVO procedureVO4 = new ProcedureVO();
			procedureVO4.type = ProcessType.Change3MJProcess;
			procedureVO4.data = sCMJChangeMJVO;
			procedureVO4.maxTime = 18;
			m_todoProcedure = procedureVO4;
			Log.Info("Add Change3MJProcess", "Game");
			if (!Util.IsTeamBattlePlaying((int)globalModel.enterGameInfo.gameType))
			{
				return;
			}
			List<int> list2 = new List<int>();
			short nTeamDefinedChgCardNum2 = tChangeCard.nTeamDefinedChgCardNum;
			if (nTeamDefinedChgCardNum2 > 0)
			{
				for (int num3 = 0; num3 < tChangeCard.astTeamChangeTiles[0].nChgTileNum; num3++)
				{
					list2.Add(tChangeCard.astTeamChangeTiles[0].szChgTiles[num3]);
				}
			}
			KeyValuePair<short, List<int>> key2 = new KeyValuePair<short, List<int>>(nTeamDefinedChgCardNum2, list2);
			KeyValuePair<KeyValuePair<short, List<int>>, List<int>> keyValuePair2 = new KeyValuePair<KeyValuePair<short, List<int>>, List<int>>(key2, sCMJChangeMJVO.changeOutList);
			base.dispatcher.Dispatch(GameEvent.RecvGameReplayH3Z, keyValuePair2);
		}
	}

	protected void Delete14thMJ()
	{
		if (WrappersManagerImpl.IsPatched(36588))
		{
			WrappersManagerImpl.GetPatch(36588).__Gen_Wrap_0(this);
			return;
		}
		PlayerGameInfoVO playerGameInfoByViewSeat = roundModel.GetPlayerGameInfoByViewSeat(0);
		Log.Info("playerGameInfo.tiles.inHandList.Count " + playerGameInfoByViewSeat.tiles.inHandList.Count + " 14th is " + m_14thMJ, "Game");
		if (playerGameInfoByViewSeat.tiles.inHandList.Count == MJDef.MAX_IN_HAND_COUNT && m_14thMJ != -1)
		{
			roundModel.curRedrawMJValue = m_14thMJ;
			playerGameInfoByViewSeat.tiles.curRedrawMjValue = roundModel.curRedrawMJValue;
			m_needToSplitRedraw = true;
		}
	}

	protected void HandlePrivateRoomInfo()
	{
		if (WrappersManagerImpl.IsPatched(36567))
		{
			WrappersManagerImpl.GetPatch(36567).__Gen_Wrap_0(this);
			return;
		}
		int num = 0;
		int nTLVNum = m_data.nTLVNum;
		for (num = 0; num < nTLVNum; num++)
		{
			TSOTLVUNIT tSOTLVUNIT = m_data.astTLVInfo[num];
			HuanLeGameSo.TLV_TYPE nType = (HuanLeGameSo.TLV_TYPE)tSOTLVUNIT.nType;
			Log.Info("Replay, HandlePrivateRoomInfo type:" + nType, "Game");
			if (nType == HuanLeGameSo.TLV_TYPE.EM_UP_TLV_PRIVATE_ROOM_REPLAY_INFO)
			{
				HandleSecretRoominfo(tSOTLVUNIT.stValue.stPrivateTableReplayInfo);
				break;
			}
		}
	}

	protected void ResetBeishu()
	{
		if (WrappersManagerImpl.IsPatched(36675))
		{
			WrappersManagerImpl.GetPatch(36675).__Gen_Wrap_0(this);
		}
		else if (roundModel.gameType == HappyMahjong.Common.GameType.WuHan)
		{
			for (int i = 0; i < 4; i++)
			{
				base.dispatcher.Dispatch(GameEvent.ShowPlayerBeiShu, i);
			}
		}
	}

	protected void HandleZhaNiaoHu(int winViewSeat)
	{
		if (WrappersManagerImpl.IsPatched(36579))
		{
			WrappersManagerImpl.GetPatch(36579).__Gen_Wrap_466(this, winViewSeat);
			return;
		}
		HuVO huVO = new HuVO();
		huVO.viewSeats.Add(winViewSeat);
		sbyte chHuFlag = roundModel.GetPlayerGameInfoByViewSeat(winViewSeat).chHuFlag;
		if (chHuFlag != 0)
		{
			List<int> inHandList = roundModel.GetTilesByViewSeat(winViewSeat).inHandList;
			if (chHuFlag == 1)
			{
				huVO.mjValue = roundModel.GetTilesByViewSeat(winViewSeat).LastMJ;
				huVO.type = UserActionType.Hu;
			}
			else
			{
				huVO.mjValue = roundModel.GetTilesByViewSeat(winViewSeat).LastMJ;
				inHandList.Remove(roundModel.GetTilesByViewSeat(winViewSeat).LastMJ);
				huVO.type = UserActionType.DianHu;
				huVO.targetViewSeat = roundModel.lastDropPlayerViewSeat;
			}
			huVO.isDahu = false;
			ProcedureVO procedureVO = ProcedureVOBuilder.BuildHuProcedureVO(huVO);
			processModel.PushProcedure(procedureVO);
			AddPushMJProcess(winViewSeat);
			base.dispatcher.Dispatch(GameEvent.UpdateHuInfo, huVO);
		}
	}

	protected void AddPushMJProcess(int viewSeat)
	{
		if (WrappersManagerImpl.IsPatched(36580))
		{
			WrappersManagerImpl.GetPatch(36580).__Gen_Wrap_466(this, viewSeat);
			return;
		}
		PushMjWallVO pushMjWallInfo = GetPushMjWallInfo(viewSeat);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.PushMajiang;
		procedureVO.data = pushMjWallInfo;
		processModel.PushProcedure(procedureVO);
	}

	protected PushMjWallVO GetPushMjWallInfo(int winViewSeat)
	{
		if (WrappersManagerImpl.IsPatched(36581))
		{
			return WrappersManagerImpl.GetPatch(36581).__Gen_Wrap_3502(this, winViewSeat);
		}
		return new PushMjWallVO
		{
			dressInfo = BalanceCommonUtil.GetViewSeatAvatarInfo(shopModel)
		};
	}

	protected void HandleVoipRoominfo(HuanLeGameSo.TVoipUserAccess userAccess)
	{
		if (WrappersManagerImpl.IsPatched(36604))
		{
			WrappersManagerImpl.GetPatch(36604).__Gen_Wrap_1(this, userAccess);
			return;
		}
		HuanLeGameSo.CNotifyVoiceInfo cNotifyVoiceInfo = new HuanLeGameSo.CNotifyVoiceInfo();
		cNotifyVoiceInfo.ullRoomid = userAccess.ullRoomid;
		cNotifyVoiceInfo.ullRoom_key = userAccess.ullRoom_key;
		cNotifyVoiceInfo.dwMember_id = userAccess.dwMember_id;
		cNotifyVoiceInfo.ullExtra_data = userAccess.ullExtra_data;
		cNotifyVoiceInfo.aszAccess_ip_list = userAccess.aszAccess_ip_list;
		cNotifyVoiceInfo.nAccess_ip_count = userAccess.nAccess_ip_count;
		base.dispatcher.Dispatch(GameEvent.NotifyVoiceRoomInfo, cNotifyVoiceInfo);
	}

	protected void HandleSecretRoominfo(HuanLeGameSo.TPrivateTableReplayInfo privateTableInfo)
	{
		if (WrappersManagerImpl.IsPatched(36568))
		{
			WrappersManagerImpl.GetPatch(36568).__Gen_Wrap_1(this, privateTableInfo);
			return;
		}
		GameUtil.HandleReplayInfo(roundModel, globalModel, privateTableInfo, m_data, base.dispatcher);
		base.dispatcher.Dispatch(GameEvent.NotifySecretRoomInfo, privateTableInfo.PlayerScoreInfo);
		if (privateTableInfo.chModuleMJVersion == 1)
		{
			bool fanMulti = (fanMulti = SingletonBase<MJGameRulesCenter>.GetInstance().GetRuleUnit().baseConfigs.fanMulti);
			iComRule.LoadFanConfigs(fanMulti);
			AudioController.Instance.SetSoundGameType(roundModel.gameType);
		}
		if (privateTableInfo.bIsInVotingTerminate != 1)
		{
			return;
		}
		TerminateVoteData terminateVoteData = new TerminateVoteData();
		int num = -1;
		for (int i = 0; i < roundModel.GetPlayerCount(); i++)
		{
			num = roundModel.GetViewSeatBySeat(i);
			terminateVoteData.ChoiceState[num] = privateTableInfo.szTerminateVoteChoice[i];
			if (i == privateTableInfo.nVoteSponsorSeat)
			{
				string nickName = PlayerDataMgr.GetInstance().GetPlayerByViewSeat(num).nickName;
				terminateVoteData.sponsorName = nickName;
			}
		}
		terminateVoteData.finalState = 0;
		terminateVoteData.leftTime = privateTableInfo.nVoteRemainTime;
		base.dispatcher.Dispatch(GameEvent.ShowVoteUI, terminateVoteData);
	}

	private void HandleSectionReturn(HuanLeGameSo.TReturnSectionScore info)
	{
		if (WrappersManagerImpl.IsPatched(36612))
		{
			WrappersManagerImpl.GetPatch(36612).__Gen_Wrap_1(this, info);
			return;
		}
		HuanLeGameSo.TGradeScoreInfo scoreInfo = null;
		for (int i = 0; i < m_data.nTLVNum; i++)
		{
			if (m_data.astTLVInfo[i].nType == 74)
			{
				scoreInfo = m_data.astTLVInfo[i].stValue.stGradeScoreInfo;
				break;
			}
		}
		SingletonBase<ResolutionManager>.GetInstance().SetFrontHandCameraState(state: false);
		AppRoot.GetInstance().InGameBridgeBase.SetReplayReturnSectionData(info, scoreInfo, roundModel.selfSeat);
		if (!SectionUtil.IsNewRankScoreScene())
		{
			base.dispatcher.Dispatch(GameEvent.SetSectionReturnBalanceUI);
		}
		BalanceCommonUtil.AddPushMJProcess(0, new List<int> { 0 }, processModel, roundModel, shopModel);
		m_inSectionReturn = true;
	}

	private void HandleFakeHu(HuanLeGameSo.TFakeHuInfoList fakeHuInfo)
	{
		if (WrappersManagerImpl.IsPatched(36611))
		{
			WrappersManagerImpl.GetPatch(36611).__Gen_Wrap_1(this, fakeHuInfo);
			return;
		}
		Log.Info("GameReplay HandleFakeHu start!", "Game");
		roundModel.SetRoundTime(10, 15);
		for (int i = 0; i < fakeHuInfo.nFakeHuPlayerNum; i++)
		{
			GuoBiaoFakeHuVO guoBiaoFakeHuVO = new GuoBiaoFakeHuVO();
			guoBiaoFakeHuVO.FakeHuList = new List<int>();
			guoBiaoFakeHuVO.ViewSeatID = roundModel.GetViewSeatBySeat(fakeHuInfo.astFakeHuInfo[i].nSeatID);
			guoBiaoFakeHuVO.FakeHuTilesNum = fakeHuInfo.astFakeHuInfo[i].nFakeHuTilesNum;
			guoBiaoFakeHuVO.FakeHuMJValue = fakeHuInfo.astFakeHuInfo[i].chTile;
			guoBiaoFakeHuVO.IsNeedAndToHandTiles = false;
			guoBiaoFakeHuVO.IsReplay = true;
			for (int j = 0; j < fakeHuInfo.astFakeHuInfo[i].nFakeHuTilesNum; j++)
			{
				guoBiaoFakeHuVO.FakeHuList.Add(fakeHuInfo.astFakeHuInfo[i].szFakeHuTiles[j]);
			}
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.FakeHuPushMJProcess;
			procedureVO.data = guoBiaoFakeHuVO;
			procedureVO.maxTime = 5;
			processModel.PushProcedure(procedureVO);
			Log.Info("Add FakeHuPushMJProcess i: " + i, "Game");
		}
	}

	private void HandleTeamInfo(HuanLeGameSo.TMatchTeamInfo teamInfo)
	{
		if (WrappersManagerImpl.IsPatched(36613))
		{
			WrappersManagerImpl.GetPatch(36613).__Gen_Wrap_1(this, teamInfo);
		}
		else
		{
			base.dispatcher.Dispatch(GameEvent.RecvTeammateBeanNum, teamInfo);
		}
	}

	public void AddWHMJRainInfo(HuanLeGameSo.TRainInfo info)
	{
		if (WrappersManagerImpl.IsPatched(36605))
		{
			WrappersManagerImpl.GetPatch(36605).__Gen_Wrap_1(this, info);
			return;
		}
		int num = ((WHMJRoundModel)roundModel).SetWHMJRain(info.szSeatRain);
		if (num != 0)
		{
			ProcedureVO procedureVO = new ProcedureVO();
			procedureVO.type = ProcessType.WHMJRain;
			WHMJRainVO wHMJRainVO = new WHMJRainVO();
			wHMJRainVO.Delta = num;
			wHMJRainVO.Replay = true;
			procedureVO.data = wHMJRainVO;
			procedureVO.maxTime = 15;
			processModel.PushProcedure(procedureVO);
		}
	}

	public void AddChangShaMjDealHuInfo(HuanLeGameSo.CNotifyCheckQishouhu info)
	{
		if (WrappersManagerImpl.IsPatched(36607))
		{
			WrappersManagerImpl.GetPatch(36607).__Gen_Wrap_1(this, info);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CSMJCheckDealHuProcess;
		procedureVO.data = info;
		procedureVO.maxTime = 25;
		roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		processModel.PushProcedure(procedureVO);
		Log.Info("RevcGameReplayCommand Add AddChangShaMjDealHuInfo", "Game");
	}

	public void AddChangShaMJGangYaoTileAndDice(HuanLeGameSo.TNotifyGangyaoTileAndDice info)
	{
		if (WrappersManagerImpl.IsPatched(36608))
		{
			WrappersManagerImpl.GetPatch(36608).__Gen_Wrap_1(this, info);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CSMJGangYaoDisplayTileAndDiceProcess;
		GangYaoTileAndDiceVO gangYaoTileAndDiceVO = new GangYaoTileAndDiceVO();
		gangYaoTileAndDiceVO.TNotifyGangyaoTileAndDice = info;
		gangYaoTileAndDiceVO.isReplay = true;
		procedureVO.data = gangYaoTileAndDiceVO;
		procedureVO.maxTime = 18;
		roundModel.SetRoundTime(info.nAllClientTime, info.nMaxShowTime);
		processModel.PushProcedure(procedureVO);
		Log.Info("RevcGameReplayCommand Add AddChangShaMJGangYaoTileAndDice", "Game");
	}

	private void AddEnterGangYaoBlockProcess(HuanLeGameSo.TDoGangyaoBlockInfo blockInfo)
	{
		if (WrappersManagerImpl.IsPatched(36609))
		{
			WrappersManagerImpl.GetPatch(36609).__Gen_Wrap_1(this, blockInfo);
			return;
		}
		Log.Debug("nAllClientTime:" + blockInfo.nAllClientTime + " nMaxShowTime:" + blockInfo.nMaxShowTime, "Game");
		roundModel.SetRoundTime(blockInfo.nAllClientTime, blockInfo.nMaxShowTime);
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterGangYaoBlockProcess;
		procedureVO.data = blockInfo;
		processModel.PushProcedure(procedureVO);
		Log.Info("RevcGameReplayCommand AddEnterGangYaoBlockProcess", "Game");
	}

	private void AddEnterWaitDealHuProcess()
	{
		if (WrappersManagerImpl.IsPatched(36583))
		{
			WrappersManagerImpl.GetPatch(36583).__Gen_Wrap_0(this);
			return;
		}
		_ = string.Empty;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterWaitProcess;
		procedureVO.data = WaitProcessType.WaitDealHu;
		processModel.PushProcedure(procedureVO);
		Log.Info("Replay AddEnterWaitProcess", "Game");
	}

	private void AddEnterWaitGangYaoBlockProcess()
	{
		if (WrappersManagerImpl.IsPatched(36584))
		{
			WrappersManagerImpl.GetPatch(36584).__Gen_Wrap_0(this);
			return;
		}
		_ = string.Empty;
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.EnterWaitProcess;
		procedureVO.data = WaitProcessType.WaitGangYaoBlock;
		processModel.PushProcedure(procedureVO);
		Log.Info("Replay AddEnterWaitProcess", "Game");
	}

	private void AddHaiDiProcess(int seat, int clientTime, int maxShowTime)
	{
		if (WrappersManagerImpl.IsPatched(36606))
		{
			WrappersManagerImpl.GetPatch(36606).__Gen_Wrap_1628(this, seat, clientTime, maxShowTime);
			return;
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.CSMJHaiDiManYouProcess;
		EnterGiveVO enterGiveVO = new EnterGiveVO();
		enterGiveVO.playerViewSeat = roundModel.GetViewSeatBySeat(seat);
		procedureVO.data = enterGiveVO;
		procedureVO.maxTime = 18;
		Log.Debug("AddHaiDiProcess viewSeat is " + enterGiveVO.playerViewSeat + " allTime " + clientTime + " maxShowTime " + maxShowTime, "Game");
		roundModel.SetRoundTime(clientTime, maxShowTime);
		processModel.PushProcedure(procedureVO);
	}

	private void HandleDanTile(HuanLeGameSo.TEggTiles eggTiles)
	{
		if (WrappersManagerImpl.IsPatched(36619))
		{
			WrappersManagerImpl.GetPatch(36619).__Gen_Wrap_1(this, eggTiles);
			return;
		}
		int viewSeatBySeat = roundModel.GetViewSeatBySeat(eggTiles.nSeat);
		MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(viewSeatBySeat);
		for (int i = 0; i < eggTiles.nEggTilesNum; i++)
		{
			HuanLeGameSo.TEggInfo tEggInfo = eggTiles.astEggTiles[i];
			List<int> list = new List<int>
			{
				tEggInfo.szTiles[0],
				tEggInfo.szTiles[1],
				tEggInfo.szTiles[2]
			};
			EggType danTypeFromDanList = XiaDanDataHelper.GetDanTypeFromDanList(list);
			tilesByViewSeat.AddMingDan(list, danTypeFromDanList);
			ActionRecordVO actionRecordVO = new ActionRecordVO();
			actionRecordVO.viewSeat = viewSeatBySeat;
			actionRecordVO.type = UserActionType.MingDan;
			actionRecordVO.mjValues = list;
			actionRecordVO.extendValue = (int)danTypeFromDanList;
			roundModel.AddActionRecord(actionRecordVO);
			for (int j = 3; j < tEggInfo.nTilesNum; j++)
			{
				int mjValue = tEggInfo.szTiles[j];
				tilesByViewSeat.AddBuDan(mjValue, danTypeFromDanList);
				ActionRecordVO actionRecordVO2 = new ActionRecordVO();
				actionRecordVO2.viewSeat = viewSeatBySeat;
				actionRecordVO2.type = UserActionType.BuDan;
				actionRecordVO2.mjValue = mjValue;
				actionRecordVO2.extendValue = (int)danTypeFromDanList;
				roundModel.AddActionRecord(actionRecordVO2);
			}
		}
	}

	private void HandleBaoPai(HuanLeGameSo.TBaoPai baoPai)
	{
		if (WrappersManagerImpl.IsPatched(36645))
		{
			WrappersManagerImpl.GetPatch(36645).__Gen_Wrap_1(this, baoPai);
			return;
		}
		BaoPaiDataModel baoPaiDataModel = (BaoPaiDataModel)roundModel.GetModel("BaoPaiModuleName");
		if (baoPaiDataModel == null)
		{
			Log.Error("HandleBaoPai round model is null!", "Game");
			return;
		}
		int chTile = baoPai.chTile;
		baoPaiDataModel.baoPaiValue = baoPai.chTile;
		base.dispatcher.Dispatch(GameEvent.ResetBaoPai, chTile);
	}

	private void PrepareShareData()
	{
		if (WrappersManagerImpl.IsPatched(36679))
		{
			WrappersManagerImpl.GetPatch(36679).__Gen_Wrap_0(this);
			return;
		}
		FetchMaxWinBeanData();
		ShareUtils.LoadShareTileConfig();
	}

	private void PreLoadFanAniConfig()
	{
		if (WrappersManagerImpl.IsPatched(36681))
		{
			WrappersManagerImpl.GetPatch(36681).__Gen_Wrap_0(this);
		}
		else
		{
			SingletonBase<FanAniHelper>.GetInstance().CheckAndLoadFanAniConfig();
		}
	}

	private void FetchMaxWinBeanData()
	{
		if (WrappersManagerImpl.IsPatched(36680))
		{
			WrappersManagerImpl.GetPatch(36680).__Gen_Wrap_0(this);
		}
		else
		{
			new PlayerRecordService().WinBeanMaxDataQuery();
		}
	}

	private void HandleRankToolsItems(HuanLeGameSo.TSoRankItem info)
	{
		if (WrappersManagerImpl.IsPatched(36614))
		{
			WrappersManagerImpl.GetPatch(36614).__Gen_Wrap_1(this, info);
			return;
		}
		Log.Info("RecvGameReplay HandleRankToolsItems, does not show animation!", "Game");
		roundModel.UsedRankItemToolsList = new List<int>();
		for (int i = 0; i < info.nItemCnt; i++)
		{
			roundModel.UsedRankItemToolsList.Add(info.EffectiveItem[i]);
		}
		string text = string.Join(", ", Array.ConvertAll(roundModel.UsedRankItemToolsList.ToArray(), (int x) => x.ToString()));
		Log.Info("UsedRankItemToolsList is: " + text, "RankTools");
		base.dispatcher.Dispatch(GameEvent.UpdateUsedRankToolData);
	}

	protected void GetRoundID()
	{
		if (WrappersManagerImpl.IsPatched(36636))
		{
			WrappersManagerImpl.GetPatch(36636).__Gen_Wrap_0(this);
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
					HuanLeGameSo.TRoundIDInfo stRoundIDInfo = tSOTLVUNIT.stValue.stRoundIDInfo;
					if (stRoundIDInfo != null)
					{
						string str = Encoding.UTF8.GetString(Util.SbyteArrayToByteArray(stRoundIDInfo.szRoundId));
						TdrTypeUtil.StringNoNULL(ref str);
						roundModel.roundID = str;
						Log.Info("RevcGameReplayCommand  GetGroundID = " + roundModel.roundID, "Game");
						base.dispatcher.Dispatch(GameEvent.ResetRoundID);
					}
					break;
				}
			}
		}
	}

	private int GetTotalHuCount()
	{
		if (WrappersManagerImpl.IsPatched(36678))
		{
			return WrappersManagerImpl.GetPatch(36678).__Gen_Wrap_2(this);
		}
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			MJTileData tilesByViewSeat = roundModel.GetTilesByViewSeat(i);
			if (tilesByViewSeat != null)
			{
				num += tilesByViewSeat.huList.Count;
			}
		}
		return num;
	}

	private void HideFirstMatchAvatar()
	{
		if (WrappersManagerImpl.IsPatched(36562))
		{
			WrappersManagerImpl.GetPatch(36562).__Gen_Wrap_0(this);
			return;
		}
		GameObject sceneLoaderObj = GameSceneNodeFinder.GetSceneLoaderObj();
		if (sceneLoaderObj == null)
		{
			return;
		}
		GameObject gameObject = sceneLoaderObj.transform.Find("SceneBuildingRoot").gameObject;
		if (gameObject != null && gameObject.activeInHierarchy)
		{
			Transform transform = gameObject.transform.Find("Kaiju");
			if (transform != null && transform.gameObject.activeInHierarchy)
			{
				transform.gameObject.SetActive(value: false);
			}
		}
	}

	public void _003C_003EiFixBaseProxy_Execute()
	{
		base.Execute();
	}
}
