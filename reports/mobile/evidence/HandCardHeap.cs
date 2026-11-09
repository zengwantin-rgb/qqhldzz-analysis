using System.Collections.Generic;
using Happy.Blueprint;
using Happy.Blueprint.RiichiStdIvP;
using HappyMahjong.Common;
using HappyMahjong.Game;
using HappyMahjong.RiichiSpace;
using IFix;
using UnityEngine;

namespace HappyMahjong.NewGame;

public class HandCardHeap : CardHeap
{
	private RiichiInHandTileHandler m_handtileHandler;

	private bool m_fapaiStarted;

	public override void SetLayoutStyle(Layout layout)
	{
		if (WrappersManagerImpl.IsPatched(33301))
		{
			WrappersManagerImpl.GetPatch(33301).__Gen_Wrap_1(this, layout);
		}
		else
		{
			m_fapaiStarted = false;
		}
	}

	public override void Clear()
	{
		if (WrappersManagerImpl.IsPatched(33302))
		{
			WrappersManagerImpl.GetPatch(33302).__Gen_Wrap_0(this);
		}
		else
		{
			m_handtileHandler.Reset();
		}
	}

	public override void RemoveCards(CardMoveVO cardMoveVO)
	{
		if (WrappersManagerImpl.IsPatched(33303))
		{
			WrappersManagerImpl.GetPatch(33303).__Gen_Wrap_1(this, cardMoveVO);
			return;
		}
		for (int i = 0; i < cardMoveVO.cards.Count; i++)
		{
			m_lastRemovedCardPos = m_handtileHandler.RemoveMJByID((int)cardMoveVO.cards[i].InstanceID, cardMoveVO.positionList[i]);
			RemoveCardsById((int)cardMoveVO.cards[i].InstanceID);
		}
		if (cardMoveVO.moveInAniType == CardMoveAniType.RiichiAni)
		{
			m_handtileHandler.SetRiichiList(null);
		}
		RemoveMJComplete();
	}

	public override void AddCards(CardMoveVO cardMoveVO)
	{
		if (WrappersManagerImpl.IsPatched(33308))
		{
			WrappersManagerImpl.GetPatch(33308).__Gen_Wrap_1(this, cardMoveVO);
		}
		else
		{
			PlaceMJWitAni(cardMoveVO.cards, cardMoveVO.moveInAniType);
		}
	}

	public override void ReplaceCards(CardMoveVO cardMoveVO)
	{
		if (WrappersManagerImpl.IsPatched(33309))
		{
			WrappersManagerImpl.GetPatch(33309).__Gen_Wrap_1(this, cardMoveVO);
		}
	}

	public override void SetCards(string vmKey, uint entityID)
	{
		if (WrappersManagerImpl.IsPatched(33310))
		{
			WrappersManagerImpl.GetPatch(33310).__Gen_Wrap_1607(this, vmKey, entityID);
		}
		else
		{
			if (!(m_handtileHandler != null))
			{
				return;
			}
			HandCardZoneAccessor entity = HbpSingleton<HbpManager>.Instance.GetEntity<HandCardZoneAccessor>(vmKey, entityID);
			List<uint> list = new List<uint>();
			foreach (GameCardAccessor card in entity.HandCardPile.GetViewCard().CardList)
			{
				list.Add(card.KeywordInstanceId);
			}
			List<uint> cardList_IDs = entity.DrawCardPile.GetViewCard().CardList_IDs;
			bool flag = false;
			foreach (uint item in cardList_IDs)
			{
				if (list.Remove(item))
				{
					flag = true;
				}
			}
			if (flag)
			{
				list.AddRange(cardList_IDs);
			}
			Log.Info($"HandCardHeap OnReplay:{m_viewSeat}, entityID: {entityID}, card_list:{Util.Stringify(list)}, m_playerIndex:{m_viewSeat}, drawCards: {Util.Stringify(cardList_IDs)}  ", "NewGame");
			List<MJCard> mJCardList = GameHBPUtil.GetMJCardList(vmKey, list);
			m_handtileHandler.SetInHandMJ(mJCardList);
			if (flag)
			{
				m_handtileHandler.SeparateRightMostMJ(cardList_IDs.Count);
			}
		}
	}

	public override void SetInvisible(bool invisible)
	{
		if (WrappersManagerImpl.IsPatched(33312))
		{
			WrappersManagerImpl.GetPatch(33312).__Gen_Wrap_453(this, invisible);
		}
		else
		{
			m_handtileHandler.gameObject.SetActive(!invisible);
		}
	}

	public void SetDefaultLayout(int index)
	{
		if (WrappersManagerImpl.IsPatched(21533))
		{
			WrappersManagerImpl.GetPatch(21533).__Gen_Wrap_466(this, index);
			return;
		}
		m_viewSeat = index;
		GameObject gameObject = GameObject.Find("GameController").transform.Find($"User{index}/Containers/TilesHolder").gameObject;
		m_handtileHandler = gameObject.GetOrAddComponent<RiichiInHandTileHandler>();
		m_handtileHandler.InitController(m_viewSeat);
		m_handtileHandler.SetCompareDelegate(MJCompareFuncs.CompareRiiChiMJ);
	}

	protected override void PlaceMJWitAni(List<MJCard> mjs, CardMoveAniType aniType)
	{
		if (WrappersManagerImpl.IsPatched(33313))
		{
			WrappersManagerImpl.GetPatch(33313).__Gen_Wrap_3145(this, mjs, aniType);
			return;
		}
		switch (aniType)
		{
		case CardMoveAniType.RedrawInAni:
			m_handtileHandler.RedrawMJ(mjs, base.AddMJWithAniComplete);
			break;
		case CardMoveAniType.DealAni:
			m_handtileHandler.DealMJ(mjs);
			VPTimer.In(0.01f, base.AddMJWithAniComplete);
			if (!m_fapaiStarted && m_viewSeat == 0)
			{
				SingletonBase<RiichiGameAudioController>.GetInstance().PlayEffect("lz_junei_fapai");
				m_fapaiStarted = true;
			}
			break;
		case CardMoveAniType.NoAni:
		case CardMoveAniType.HandAni:
		case CardMoveAniType.DropAni:
			break;
		}
	}

	protected override void PlaceMJWithoutAni(List<MJCard> mjList)
	{
		if (WrappersManagerImpl.IsPatched(33327))
		{
			WrappersManagerImpl.GetPatch(33327).__Gen_Wrap_1(this, mjList);
		}
	}

	public void SetHandCardStatus(List<MJCard> list, MJStatus status)
	{
		if (WrappersManagerImpl.IsPatched(21754))
		{
			WrappersManagerImpl.GetPatch(21754).__Gen_Wrap_1639(this, list, status);
		}
		else
		{
			m_handtileHandler.SetListMJStatus(list, status);
		}
	}

	public void SetMJEnableClick(List<MJCard> mjList, bool enable = true)
	{
		if (WrappersManagerImpl.IsPatched(32930))
		{
			WrappersManagerImpl.GetPatch(32930).__Gen_Wrap_2721(this, mjList, enable);
		}
		else
		{
			m_handtileHandler.SetListMJEnable(mjList, enable);
		}
	}

	public void SetRiichiMJList(List<MJCard> mjList)
	{
		if (WrappersManagerImpl.IsPatched(32953))
		{
			WrappersManagerImpl.GetPatch(32953).__Gen_Wrap_1(this, mjList);
		}
		else
		{
			m_handtileHandler.SetRiichiList(mjList);
		}
	}

	public void CancelRiichi()
	{
		if (WrappersManagerImpl.IsPatched(32957))
		{
			WrappersManagerImpl.GetPatch(32957).__Gen_Wrap_0(this);
		}
		else
		{
			m_handtileHandler.CancelRiichi();
		}
	}

	public void LaydownHandTiles()
	{
		if (WrappersManagerImpl.IsPatched(33328))
		{
			WrappersManagerImpl.GetPatch(33328).__Gen_Wrap_0(this);
		}
		else
		{
			m_handtileHandler.LayDownAllMJs();
		}
	}

	public void SetMJUnEnable()
	{
		if (WrappersManagerImpl.IsPatched(21823))
		{
			WrappersManagerImpl.GetPatch(21823).__Gen_Wrap_0(this);
		}
		else
		{
			m_handtileHandler.SetMJEnable(value: false);
		}
	}

	public void RefreshDoraCardList(List<int> infoList)
	{
		if (WrappersManagerImpl.IsPatched(21649))
		{
			WrappersManagerImpl.GetPatch(21649).__Gen_Wrap_1(this, infoList);
		}
		else
		{
			m_handtileHandler.RefreshDoraCardList(infoList);
		}
	}

	public void HuFlpCards()
	{
		if (WrappersManagerImpl.IsPatched(21864))
		{
			WrappersManagerImpl.GetPatch(21864).__Gen_Wrap_0(this);
		}
		else
		{
			m_handtileHandler.HuFlipCards();
		}
	}

	public void ZimoHuCard(MJCard card)
	{
		if (WrappersManagerImpl.IsPatched(21882))
		{
			WrappersManagerImpl.GetPatch(21882).__Gen_Wrap_1(this, card);
		}
		else
		{
			m_handtileHandler.ZimoHuCard(card);
		}
	}

	public void FlipCards(Enum_ClientCardFace face)
	{
		if (WrappersManagerImpl.IsPatched(21458))
		{
			WrappersManagerImpl.GetPatch(21458).__Gen_Wrap_1571(this, face);
		}
		else
		{
			m_handtileHandler.FlipCards(face);
		}
	}

	public RiichiInHandTileHandler GetUIHandler()
	{
		if (WrappersManagerImpl.IsPatched(21934))
		{
			return WrappersManagerImpl.GetPatch(21934).__Gen_Wrap_1665(this);
		}
		return m_handtileHandler;
	}

	public void _003C_003EiFixBaseProxy_SetLayoutStyle(Layout P0)
	{
		base.SetLayoutStyle(P0);
	}

	public void _003C_003EiFixBaseProxy_Clear()
	{
		base.Clear();
	}

	public void _003C_003EiFixBaseProxy_RemoveCards(CardMoveVO P0)
	{
		base.RemoveCards(P0);
	}

	public void _003C_003EiFixBaseProxy_AddCards(CardMoveVO P0)
	{
		base.AddCards(P0);
	}

	public void _003C_003EiFixBaseProxy_ReplaceCards(CardMoveVO P0)
	{
		base.ReplaceCards(P0);
	}

	public void _003C_003EiFixBaseProxy_SetCards(string P0, uint P1)
	{
		base.SetCards(P0, P1);
	}

	public void _003C_003EiFixBaseProxy_SetInvisible(bool P0)
	{
		base.SetInvisible(P0);
	}

	public void _003C_003EiFixBaseProxy_PlaceMJWitAni(List<MJCard> P0, CardMoveAniType P1)
	{
		base.PlaceMJWitAni(P0, P1);
	}

	public void _003C_003EiFixBaseProxy_PlaceMJWithoutAni(List<MJCard> P0)
	{
		base.PlaceMJWithoutAni(P0);
	}
}
