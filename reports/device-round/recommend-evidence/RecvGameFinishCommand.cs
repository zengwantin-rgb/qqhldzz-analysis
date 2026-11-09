using System.Collections.Generic;
using HappyMahjong.Common;
using HappyMahjong.ShopAndBag;
using HuanLeGameSo;
using IFix;
using strange.extensions.command.impl;

namespace HappyMahjong.Game;

public class RecvGameFinishCommand : EventCommand
{
	[Inject]
	public IRoundModel roundModel { get; set; }

	[Inject]
	public IProcessModel processModel { get; set; }

	[Inject]
	public IGlobalModel globalModel { get; set; }

	[Inject]
	public ICommonRule iComRule { get; set; }

	[Inject]
	public IShopModel shopModel { get; set; }

	public override void Execute()
	{
		if (WrappersManagerImpl.IsPatched(36270))
		{
			WrappersManagerImpl.GetPatch(36270).__Gen_Wrap_0(this);
			return;
		}
		CFinalHuNotify cFinalHuNotify = (CFinalHuNotify)base.evt.data;
		Util.CancelTimeoutReconnectAfterGive();
		if (globalModel.enterGameInfo.enterGameSecretRoomVO.isSecretRoom && roundModel.gameStatus == GameStatus.NotPlaying)
		{
			return;
		}
		if (globalModel.enterGameInfo.enterGameSecretRoomVO.isSecretRoom && (roundModel.IsFriendFroomTerminate || (roundModel.PrivateTableFinishInfo != null && roundModel.PrivateTableFinishInfo.chIsPrivateRoomTerminate == 1)))
		{
			if (processModel.curProcedure != null)
			{
				processModel.ProcedureCompleted(processModel.curProcedure.type);
				processModel.Reset();
			}
			base.dispatcher.Dispatch(GameEvent.OnClearModule);
		}
		roundModel.currentTingInfo = null;
		iComRule.LastTingInfo = null;
		SetOtherPlayerHandMJ(cFinalHuNotify);
		for (int i = 0; i < cFinalHuNotify.chPlayerNum && i < cFinalHuNotify.astHandTiles.Length; i++)
		{
			Log.Info("Balance UserHand Tiles seat:" + cFinalHuNotify.astHandTiles[i].nSeat + " tiles:" + FormatTiles(cFinalHuNotify.astHandTiles[i].szTiles, cFinalHuNotify.astHandTiles[i].nHandTilesNum), "Balance");
		}
		PushMjWallVO pushMjWallVO = new PushMjWallVO();
		pushMjWallVO.dressInfo = BalanceCommonUtil.GetViewSeatAvatarInfo(shopModel);
		pushMjWallVO.gameType = roundModel.gameType;
		for (int j = 0; j < cFinalHuNotify.chWinSeatNum && j < cFinalHuNotify.szWinSeat.Length; j++)
		{
			pushMjWallVO.winViewSeatIds.Add(roundModel.GetViewSeatBySeat(cFinalHuNotify.szWinSeat[j]));
		}
		ProcedureVO procedureVO = new ProcedureVO();
		procedureVO.type = ProcessType.PushMajiang_V2;
		procedureVO.data = pushMjWallVO;
		processModel.PushProcedure(procedureVO);
		base.dispatcher.Dispatch(GameEvent.ExecuteProcessCmd);
	}

	private string FormatTiles(sbyte[] szTiles, int nHandTilesNum)
	{
		if (WrappersManagerImpl.IsPatched(36277))
		{
			return WrappersManagerImpl.GetPatch(36277).__Gen_Wrap_2805(this, szTiles, nHandTilesNum);
		}
		string text = "";
		for (int i = 0; i < szTiles.Length && i < nHandTilesNum; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			text += szTiles[i];
		}
		return text;
	}

	private void SetOtherPlayerHandMJ(CFinalHuNotify data)
	{
		if (WrappersManagerImpl.IsPatched(36275))
		{
			WrappersManagerImpl.GetPatch(36275).__Gen_Wrap_1(this, data);
			return;
		}
		for (int i = 0; i < data.chPlayerNum && i < data.astHandTiles.Length; i++)
		{
			THandTiles obj = data.astHandTiles[i];
			sbyte[] szTiles = obj.szTiles;
			int nHandTilesNum = obj.nHandTilesNum;
			int nSeat = obj.nSeat;
			int viewSeatBySeat = roundModel.GetViewSeatBySeat(nSeat);
			if (viewSeatBySeat > 3 || viewSeatBySeat < 0)
			{
				Log.Error("Illegal viewSeat. playerSeat:" + nSeat, "Game");
				continue;
			}
			List<int> list = new List<int>();
			for (int j = 0; j < nHandTilesNum; j++)
			{
				list.Add(szTiles[j]);
			}
			UserPaiVO userPaiVO = new UserPaiVO();
			userPaiVO.viewSeat = viewSeatBySeat;
			userPaiVO.mjList.handpaiList = list;
			AddSetUserPai(userPaiVO);
		}
	}

	private void AddSetUserPai(UserPaiVO payLoad)
	{
		if (WrappersManagerImpl.IsPatched(36276))
		{
			WrappersManagerImpl.GetPatch(36276).__Gen_Wrap_1(this, payLoad);
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
