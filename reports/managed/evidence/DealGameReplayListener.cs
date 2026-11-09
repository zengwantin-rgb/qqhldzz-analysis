using HappyMahjong.Common;
using HuanLeGameSo;
using IFix;
using MJSoPb;

namespace HappyMahjong.CSReplay;

public class DealGameReplayListener : BaseGameReplayListener
{
	private HuanLeGameSo.CNotifyDeal m_dealMsg;

	private RoomSectionType m_roomSectionType;

	private TRoundEnv m_roundEnv;

	public override void InitFiller(CSReplayRecordData data)
	{
		if (WrappersManagerImpl.IsPatched(34851))
		{
			WrappersManagerImpl.GetPatch(34851).__Gen_Wrap_1(this, data);
			return;
		}
		m_dealMsg = null;
		m_roomSectionType = data.GetRoomSectionType();
		m_roundEnv = data.GetRoundEnv();
	}

	public override void CacheAllPlayerGameMsg(HuanLeGameSo.SoNotifyMsgTV data, bool isSelfMsg)
	{
		if (WrappersManagerImpl.IsPatched(34852))
		{
			WrappersManagerImpl.GetPatch(34852).__Gen_Wrap_2721(this, data, isSelfMsg);
		}
		else if (isSelfMsg && m_dealMsg == null && data.iType == 1)
		{
			m_dealMsg = data.stValue.stNotifyDeal;
		}
	}

	public override void FillMyGameMsg(HuanLeGameSo.SoNotifyMsgTV[] myMsgList)
	{
		if (WrappersManagerImpl.IsPatched(34853))
		{
			WrappersManagerImpl.GetPatch(34853).__Gen_Wrap_1(this, myMsgList);
		}
		else
		{
			AppendDealTileTlv(m_dealMsg);
		}
	}

	private bool needPayTicket()
	{
		if (WrappersManagerImpl.IsPatched(34855))
		{
			return WrappersManagerImpl.GetPatch(34855).__Gen_Wrap_452(this);
		}
		if (((HuanLeGameSo.SO_ROOM_TYPE)m_roundEnv.GameType).ToClientGameType() == GameType.ModuleHongZhongXueLiu)
		{
			return false;
		}
		if (m_roomSectionType != RoomSectionType.RoomBean && m_roomSectionType != RoomSectionType.RoomGoldCoin)
		{
			return m_roomSectionType == RoomSectionType.RoomSectionScoreAndBean;
		}
		return true;
	}

	private void AppendDealTileTlv(HuanLeGameSo.CNotifyDeal msg)
	{
		if (WrappersManagerImpl.IsPatched(34854))
		{
			WrappersManagerImpl.GetPatch(34854).__Gen_Wrap_1(this, msg);
		}
		else
		{
			if (msg == null)
			{
				return;
			}
			bool flag = needPayTicket();
			TRoundEnv roundEnv = m_roundEnv;
			int num = msg.astTLVInfo.Length;
			int num2 = roundEnv.PlayerEnv.Count + (flag ? 1 : 0) + num;
			msg.nTLVNum = CSReplayRecordHelper.AppendTlv(ref msg.astTLVInfo, num2, roundEnv.PlayerEnv.Count);
			TSOTLVUNIT[] astTLVInfo = msg.astTLVInfo;
			for (int i = 0; i < roundEnv.PlayerEnv.Count; i++)
			{
				TPlayerEnv tPlayerEnv = roundEnv.PlayerEnv[i];
				astTLVInfo[i] = CSReplayRecordHelper.CreateTlv(10);
				astTLVInfo[i].stValue.stHandTiles = new HuanLeGameSo.THandTiles();
				HuanLeGameSo.THandTiles stHandTiles = astTLVInfo[i].stValue.stHandTiles;
				stHandTiles.nSeat = (short)i;
				stHandTiles.nHandTilesNum = (short)Util.AssignSByteArrayByName(stHandTiles, "szTiles", tPlayerEnv, "InitTiles");
				Log.Info($"AppendDealTileTlv Seat: {stHandTiles.nSeat} hand: {Util.Stringify(tPlayerEnv.InitTiles)}", "CSReplay");
			}
			if (flag)
			{
				int num3 = num2 - 1;
				astTLVInfo[num3] = CSReplayRecordHelper.CreateTlv(16);
				astTLVInfo[num3].stValue.stDonnerInfo = new HuanLeGameSo.TDonnerInfo();
				HuanLeGameSo.TDonnerInfo stDonnerInfo = astTLVInfo[num3].stValue.stDonnerInfo;
				stDonnerInfo.nSeatNum = (short)m_roundEnv.PlayerEnv.Count;
				stDonnerInfo.szSeatId = new sbyte[stDonnerInfo.nSeatNum];
				stDonnerInfo.ChgDonner = new long[stDonnerInfo.nSeatNum];
				stDonnerInfo.CurDonner = new long[stDonnerInfo.nSeatNum];
				int ticket = m_roundEnv.Ticket;
				for (int j = 0; j < m_roundEnv.PlayerEnv.Count; j++)
				{
					TPlayerEnv tPlayerEnv2 = m_roundEnv.PlayerEnv[j];
					stDonnerInfo.szSeatId[j] = (sbyte)j;
					stDonnerInfo.ChgDonner[j] = -ticket;
					stDonnerInfo.CurDonner[j] = tPlayerEnv2.EnterMoney - ticket;
				}
			}
		}
	}

	public void _003C_003EiFixBaseProxy_InitFiller(CSReplayRecordData P0)
	{
		base.InitFiller(P0);
	}

	public void _003C_003EiFixBaseProxy_CacheAllPlayerGameMsg(HuanLeGameSo.SoNotifyMsgTV P0, bool P1)
	{
		base.CacheAllPlayerGameMsg(P0, P1);
	}

	public void _003C_003EiFixBaseProxy_FillMyGameMsg(HuanLeGameSo.SoNotifyMsgTV[] P0)
	{
		base.FillMyGameMsg(P0);
	}
}
