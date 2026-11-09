using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Happy.Blueprint;
using Happy.Blueprint.RiichiStdIvP;
using HappyMahjong.Game;
using HappyMahjong.Game.RiichiGameUI;
using HappyMahjong.Loading;
using HappyMahjong.NewGame;
using HappyMahjong.RiichiSpace;
using HappyMahjong.Setting;
using HuanLeGameSo;
using IFix;
using IFix.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace HappyMahjong.Common;

public class GameHBPUtil
{
	private static VfxGlowOffsetControl s_shineVfxGlowOffsetControl;

	public static bool IsProcessEmpty()
	{
		if (WrappersManagerImpl.IsPatched(35120))
		{
			return WrappersManagerImpl.GetPatch(35120).__Gen_Wrap_1621();
		}
		MJGameContext gameContext = SingletonBase<HBPMJBridge>.GetInstance().GameContext;
		if (gameContext == null)
		{
			return true;
		}
		HBPGameModel hBPGameModel = gameContext.HBPGameModel;
		if (hBPGameModel != null)
		{
			return hBPGameModel.GetCurrentAwaitNodeNum() <= 0;
		}
		return true;
	}

	public static List<C_PlayerOpChoice> GetOpArrayByType(List<C_PlayerOpChoice> canDoOpList, List<Enum_PlayerOperation> opTypeList)
	{
		if (WrappersManagerImpl.IsPatched(21397))
		{
			return WrappersManagerImpl.GetPatch(21397).__Gen_Wrap_1544(canDoOpList, opTypeList);
		}
		List<C_PlayerOpChoice> list = new List<C_PlayerOpChoice>();
		foreach (C_PlayerOpChoice canDoOp in canDoOpList)
		{
			foreach (Enum_PlayerOperation opType in opTypeList)
			{
				if (canDoOp.OpType == opType)
				{
					list.Add(canDoOp);
				}
			}
		}
		return list;
	}

	[IDTag(1)]
	public static List<MJCard> GetMJCardList(string vmKey, List<uint> cardList)
	{
		if (WrappersManagerImpl.IsPatched(33251))
		{
			return WrappersManagerImpl.GetPatch(33251).__Gen_Wrap_1558(vmKey, cardList);
		}
		RepeatedField<uint> repeatedField = new RepeatedField<uint>();
		repeatedField.AddRange(cardList);
		return GetMJCardList(vmKey, repeatedField);
	}

	public static List<int> GetMJCardValueList(string vmKey, List<uint> cardList)
	{
		if (WrappersManagerImpl.IsPatched(33064))
		{
			return WrappersManagerImpl.GetPatch(33064).__Gen_Wrap_3117(vmKey, cardList);
		}
		RepeatedField<uint> repeatedField = new RepeatedField<uint>();
		repeatedField.AddRange(cardList);
		List<MJCard> mJCardList = GetMJCardList(vmKey, repeatedField);
		List<int> list = new List<int>();
		for (int i = 0; i < mJCardList.Count; i++)
		{
			list.Add(mJCardList[i].mjValue);
		}
		return list;
	}

	[IDTag(0)]
	public static List<MJCard> GetMJCardList(string vmKey, RepeatedField<uint> cardList)
	{
		if (WrappersManagerImpl.IsPatched(21408))
		{
			return WrappersManagerImpl.GetPatch(21408).__Gen_Wrap_1558(vmKey, cardList);
		}
		List<MJCard> list = new List<MJCard>();
		MJCard mJCard = null;
		for (int i = 0; i < cardList.Count; i++)
		{
			GameCardAccessor entity = HbpSingleton<HbpManager>.Instance.GetEntity<GameCardAccessor>(vmKey, cardList[i]);
			if (entity == null)
			{
				Debug.LogError("GetMJCardList Accessor not found, entityId = " + cardList[i]);
				continue;
			}
			mJCard = SingletonBase<HBPMJBridge>.GetInstance().GetEntityById(entity.KeywordInstanceId) as MJCard;
			if (mJCard == null)
			{
				mJCard = new MJCard();
				SingletonBase<HBPMJBridge>.GetInstance().RegisterEntity(entity.KeywordInstanceId, mJCard);
				mJCard.mjValue = GetMJValue(entity.BasicInfo, entity.ExtendInfo);
			}
			mJCard.isRiichi = entity.ExtendInfo.IsRiichiCard;
			mJCard.isRedDora = entity.ExtendInfo.IsRedDora;
			mJCard.isDora = entity.ExtendInfo.IsDora;
			mJCard.mjStarus = MJStatus.Normal;
			list.Add(mJCard);
		}
		return list;
	}

	public static int GetMJValueByMJCardId(string vmKey, uint cardId)
	{
		if (WrappersManagerImpl.IsPatched(22020))
		{
			return WrappersManagerImpl.GetPatch(22020).__Gen_Wrap_1685(vmKey, cardId);
		}
		GameCardAccessor entity = HbpSingleton<HbpManager>.Instance.GetEntity<GameCardAccessor>(vmKey, cardId);
		if (entity == null)
		{
			Debug.LogError("GetMJCardList Accessor not found, entityId = " + cardId);
			return 0;
		}
		return GetMJValue(entity.BasicInfo, entity.ExtendInfo);
	}

	public static int GetMJValueWithRedDora(int mjValue, bool isRedDora)
	{
		if (WrappersManagerImpl.IsPatched(21887))
		{
			return WrappersManagerImpl.GetPatch(21887).__Gen_Wrap_1650(mjValue, isRedDora);
		}
		if (isRedDora)
		{
			switch (mjValue)
			{
			case 5:
				mjValue = 41;
				break;
			case 15:
				mjValue = 42;
				break;
			case 25:
				mjValue = 43;
				break;
			}
		}
		return mjValue;
	}

	[IDTag(0)]
	public static int GetMJValue(C_CardBasicInfoAccessor cardInfo, C_CardExtendInfoAccessor extendInfo)
	{
		if (WrappersManagerImpl.IsPatched(21419))
		{
			return WrappersManagerImpl.GetPatch(21419).__Gen_Wrap_1557(cardInfo, extendInfo);
		}
		int result = 0;
		if (cardInfo.Color != Enum_CardColor.CardColorSystem)
		{
			result = ((cardInfo.Color != Enum_CardColor.CardColorSpecial) ? (cardInfo.Seq + 10 * (int)cardInfo.Color) : cardInfo.Seq);
		}
		else if (cardInfo.Seq == 1)
		{
			result = MJDef.MJ_EMPTY;
		}
		else if (cardInfo.Seq == 2)
		{
			result = MJDef.MJ_BACK;
		}
		return result;
	}

	[IDTag(1)]
	public static int GetMJValue(C_CardBasicInfo cardInfo, C_CardExtendInfo extendInfo)
	{
		if (WrappersManagerImpl.IsPatched(21643))
		{
			return WrappersManagerImpl.GetPatch(21643).__Gen_Wrap_1557(cardInfo, extendInfo);
		}
		int result = 0;
		if (cardInfo.Color != Enum_CardColor.CardColorSystem)
		{
			result = ((cardInfo.Color != Enum_CardColor.CardColorSpecial) ? (cardInfo.Seq + 10 * (int)cardInfo.Color) : cardInfo.Seq);
		}
		else if (cardInfo.Seq == 1)
		{
			result = MJDef.MJ_EMPTY;
		}
		else if (cardInfo.Seq == 2)
		{
			result = MJDef.MJ_BACK;
		}
		return result;
	}

	public static int GetSeatByPlayerInstanceId(uint id)
	{
		if (WrappersManagerImpl.IsPatched(22018))
		{
			return WrappersManagerImpl.GetPatch(22018).__Gen_Wrap_1684(id);
		}
		HappyMahjong.NewGame.Player player = SingletonBase<HBPMJBridge>.GetInstance().GetEntityById(id) as HappyMahjong.NewGame.Player;
		return SingletonBase<HBPMJBridge>.GetInstance().GameContext.RoundModel.GetSeatByViewSeat(player.GetPlayerViewSeat());
	}

	public static void CopyGameObjectTransInfo(GameObject oldOne, GameObject newOne)
	{
		if (WrappersManagerImpl.IsPatched(33297))
		{
			WrappersManagerImpl.GetPatch(33297).__Gen_Wrap_1(oldOne, newOne);
		}
		else if (oldOne != null && newOne != null)
		{
			Util.ResetParent(oldOne.transform.parent, newOne.transform);
			newOne.transform.localPosition = oldOne.transform.localPosition;
			newOne.transform.localScale = oldOne.transform.localScale;
			newOne.transform.localRotation = oldOne.transform.localRotation;
		}
	}

	public static void AddShiningEffectToMJ(GameObject mj, int layer, float offsetZ = -0.001f, float scale = 1f)
	{
		if (WrappersManagerImpl.IsPatched(21651))
		{
			WrappersManagerImpl.GetPatch(21651).__Gen_Wrap_1598(mj, layer, offsetZ, scale);
			return;
		}
		GameObject gameObject = null;
		if (mj.transform.Find("fx_shine") != null)
		{
			gameObject = mj.transform.Find("fx_shine").gameObject;
			UnityEngine.Object.Destroy(gameObject);
		}
		if (layer == MJDef.LAYER_UI)
		{
			gameObject = UIUtil.InstantiateSimply("RawResources_New/RiichiGame/InGame_ab", "fx_shine_UI");
			Util.ResetParent(mj.transform, gameObject.transform);
		}
		else
		{
			gameObject = UIUtil.InstantiateSimply("RawResources_New/RiichiGame/InGame_ab", "fx_shine");
			Util.ResetParent(mj.transform, gameObject.transform);
			gameObject.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, offsetZ), Quaternion.Euler(-90f, 0f, 0f));
			gameObject.transform.localScale = new Vector3(scale, scale, scale);
		}
		gameObject.name = "fx_shine";
		SingletonBase<RiichiMJResManager>.GetInstance().AddAndSyncMJShineEffect(gameObject);
		Util.SetLayer(gameObject, mj.layer);
	}

	public static void XueLiuAddShiningEffectToMJ(GameObject mj, int layer, float offsetZ = -0.001f, float scale = 1f)
	{
		if (WrappersManagerImpl.IsPatched(35121))
		{
			WrappersManagerImpl.GetPatch(35121).__Gen_Wrap_1598(mj, layer, offsetZ, scale);
			return;
		}
		RemoveAllChildWithName(mj, "fx_shine");
		GameObject gameObject = UIUtil.InstantiateSimply("RawResources_New/XueLiuBaoPai_ab", "fx_shine");
		Util.ResetParent(mj.transform, gameObject.transform);
		gameObject.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, offsetZ), Quaternion.Euler(-90f, 0f, 0f));
		gameObject.transform.localScale = new Vector3(scale, scale, scale);
		gameObject.name = "fx_shine";
		InitShineGlowControl();
		Util.SetLayer(gameObject, mj.layer);
	}

	private static void InitShineGlowControl()
	{
		if (WrappersManagerImpl.IsPatched(35126))
		{
			WrappersManagerImpl.GetPatch(35126).__Gen_Wrap_459();
		}
		else if (s_shineVfxGlowOffsetControl == null)
		{
			Transform uIRootTransform = Util.GetUIRootTransform(SceneSwitch.Scene.Game);
			if (uIRootTransform != null)
			{
				GameObject gameObject = UIUtil.Instantiate(UIUtil.LoadPrefab("RawResources_New/Game/GameUI_ab", "ShineVfxGlowOffsetControl"));
				gameObject.transform.SetParent(uIRootTransform.transform, worldPositionStays: false);
				s_shineVfxGlowOffsetControl = gameObject.GetComponent<VfxGlowOffsetControl>();
			}
		}
	}

	public static void RemoveAllChildWithName(GameObject parent, string childName)
	{
		if (WrappersManagerImpl.IsPatched(35122))
		{
			WrappersManagerImpl.GetPatch(35122).__Gen_Wrap_1(parent, childName);
		}
		else
		{
			if (!(parent != null))
			{
				return;
			}
			List<GameObject> list = CollectionPool<List<GameObject>, GameObject>.Get();
			try
			{
				for (int i = 0; i < parent.transform.childCount; i++)
				{
					Transform child = parent.transform.GetChild(i);
					if (child.name == childName)
					{
						list.Add(child.gameObject);
					}
				}
				if (list.Count <= 0)
				{
					return;
				}
				list.ForEach(delegate(GameObject g)
				{
					if (g != null)
					{
						UnityEngine.Object.Destroy(g);
					}
				});
			}
			finally
			{
				CollectionPool<List<GameObject>, GameObject>.Release(list);
			}
		}
	}

	public static void AddSpecificalShiningEffectToMJ(GameObject mj, GameObject effect, int layer, float offsetZ = 0f, float scale = 0.93f)
	{
		if (WrappersManagerImpl.IsPatched(35127))
		{
			WrappersManagerImpl.GetPatch(35127).__Gen_Wrap_3349(mj, effect, layer, offsetZ, scale);
		}
		else if (effect != null)
		{
			RemoveAllChildWithName(mj, "fx_specifical_shine");
			Util.ResetParent(mj.transform, effect.transform);
			effect.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, offsetZ), Quaternion.Euler(0f, 0f, 0f));
			effect.transform.localScale = new Vector3(scale, scale, scale);
			effect.name = "fx_specifical_shine";
			Util.SetLayer(effect, layer);
		}
	}

	public static void AddBaopaiFlicker(GameObject mj, string path, Vector3 pos, Quaternion rotate)
	{
		if (WrappersManagerImpl.IsPatched(35128))
		{
			WrappersManagerImpl.GetPatch(35128).__Gen_Wrap_3350(mj, path, pos, rotate);
			return;
		}
		GameObject gameObject = UIUtil.Instantiate(UIUtil.LoadPrefab("RawResources_New/XueLiuBaoPai_ab", path), mj.transform);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = pos;
		gameObject.transform.localRotation = rotate;
		gameObject.AddComponent<DestroyHandler>().maxLifeTime = 3f;
		if (!SingletonBase<HappyMahjong.Setting.Setting>.GetInstance().effectOn)
		{
			gameObject.GetComponent<AudioSource>().enabled = false;
		}
	}

	public static void ShowDianpaoRemoveEffect(GameObject obj)
	{
		if (WrappersManagerImpl.IsPatched(21872))
		{
			WrappersManagerImpl.GetPatch(21872).__Gen_Wrap_0(obj);
			return;
		}
		string effectPath = "fx_hu_light01";
		PlayDianPaoAnimation(obj, effectPath);
	}

	private static void PlayDianPaoAnimation(GameObject target, string effectPath)
	{
		if (WrappersManagerImpl.IsPatched(21873))
		{
			WrappersManagerImpl.GetPatch(21873).__Gen_Wrap_1(target, effectPath);
			return;
		}
		SingletonBase<RiichiGameAudioController>.GetInstance().PlayEffect("lz_junei_jizhong");
		Log.Info("PlayDianPaoAnimation", "Game");
		GameObject gameObject = UIUtil.LoadAndInstantiateSync("RawResources_New/RiichiGame/InGame_ab", effectPath, target);
		Util.ResetParent(target.transform, gameObject.transform);
		gameObject.SetActive(value: true);
	}

	public static Wind GetWindType(Enum_WindType win)
	{
		if (WrappersManagerImpl.IsPatched(21676))
		{
			return WrappersManagerImpl.GetPatch(21676).__Gen_Wrap_1605(win);
		}
		return win switch
		{
			Enum_WindType.WindtypeEast => Wind.East, 
			Enum_WindType.WindtypeNorth => Wind.North, 
			Enum_WindType.WindtypeSouth => Wind.South, 
			Enum_WindType.WindtypeWest => Wind.West, 
			_ => Wind.East, 
		};
	}

	public static string GetRoundWindString(Wind wind, int cnt)
	{
		if (WrappersManagerImpl.IsPatched(21925))
		{
			return WrappersManagerImpl.GetPatch(21925).__Gen_Wrap_1664(wind, cnt);
		}
		string text = wind switch
		{
			Wind.East => "东", 
			Wind.South => "南", 
			Wind.West => "西", 
			Wind.North => "北", 
			_ => throw new ArgumentOutOfRangeException("wind", wind, null), 
		};
		string text2 = cnt.ToString();
		switch (cnt)
		{
		case 1:
			text2 = "一";
			break;
		case 2:
			text2 = "二";
			break;
		case 3:
			text2 = "三";
			break;
		case 4:
			text2 = "四";
			break;
		}
		return text + text2 + "局";
	}

	public static List<RoundDetail> GetRoundDetail(C_RoundWinInfo info)
	{
		if (WrappersManagerImpl.IsPatched(22021))
		{
			return WrappersManagerImpl.GetPatch(22021).__Gen_Wrap_1688(info);
		}
		List<RoundDetail> list = new List<RoundDetail>();
		RoundDetail roundDetail = new RoundDetail();
		roundDetail.ChgDonner = new long[info.PlayerWininfo.Count];
		roundDetail.FengDingType = new int[info.PlayerWininfo.Count];
		roundDetail.FengDingValue = new int[info.PlayerWininfo.Count];
		roundDetail.iActionType = (int)info.WininfoActionType;
		for (int i = 0; i < info.PlayerWininfo.Count; i++)
		{
			roundDetail.ChgDonner[i] = info.PlayerWininfo[i].ChangeBean;
			roundDetail.FengDingType[i] = (info.PlayerWininfo[i].IsBeanFengding ? 1 : 0);
			roundDetail.FengDingValue[i] = info.PlayerWininfo[i].TotalChangeFan;
			roundDetail.nStartIdx = 0;
			roundDetail.nEndIdx = (sbyte)(info.DetailList.Count - 1);
		}
		list.Add(roundDetail);
		return list;
	}

	public static TChangeDetail GetChangeDetail(string vmKey, C_RoundWinInfo info, int winTileTypeListStartIndex)
	{
		if (WrappersManagerImpl.IsPatched(22026))
		{
			return WrappersManagerImpl.GetPatch(22026).__Gen_Wrap_1689(vmKey, info, winTileTypeListStartIndex);
		}
		TChangeDetail tChangeDetail = new TChangeDetail();
		tChangeDetail.nChangeDetailNum = (short)info.DetailList.Count;
		tChangeDetail.astDetai = new TDetai[tChangeDetail.nChangeDetailNum];
		int num = -1;
		for (int i = 0; i < tChangeDetail.nChangeDetailNum; i++)
		{
			tChangeDetail.astDetai[i] = new TDetai();
			tChangeDetail.astDetai[i].chType = (sbyte)info.DetailList[i].DetailType;
			tChangeDetail.astDetai[i].chDstSeat = (sbyte)GetSeatByPlayerInstanceId(info.DetailList[i].DstPlayer);
			tChangeDetail.astDetai[i].chFromSeat = (sbyte)GetSeatByPlayerInstanceId(info.DetailList[i].FromPlayer);
			if (info.DetailList[i].RelatedCards.Count > 0)
			{
				tChangeDetail.astDetai[i].chTile = (sbyte)GetMJValueByMJCardId(vmKey, info.DetailList[i].RelatedCards[0]);
			}
			tChangeDetail.astDetai[i].nMulNum = 0;
			tChangeDetail.astDetai[i].iChangeFan = info.DetailList[i].ChangeFan * 10;
			tChangeDetail.astDetai[i].iReserveType = 0;
			tChangeDetail.astDetai[i].iReserveValue = 0;
			if (info.DetailList[i].DetailType == Enum_WinInfoDetailType.EnmWininfoDetailTypeZimoHu || info.DetailList[i].DetailType == Enum_WinInfoDetailType.EnmWininfoDetailTypeFangpaoHu)
			{
				num++;
				tChangeDetail.astDetai[i].iWinTileTypeIdx = num + winTileTypeListStartIndex;
			}
			tChangeDetail.astDetai[i].llChangeDonner = 0L;
		}
		return tChangeDetail;
	}

	public static TWinTileTypeV[] GetWinTileTypeV(string vmKey, C_RoundWinInfo info)
	{
		if (WrappersManagerImpl.IsPatched(22011))
		{
			return WrappersManagerImpl.GetPatch(22011).__Gen_Wrap_1686(vmKey, info);
		}
		List<TWinTileTypeV> list = new List<TWinTileTypeV>();
		TWinTileTypeV tWinTileTypeV = null;
		for (int i = 0; i < info.DetailList.Count; i++)
		{
			if (info.DetailList[i].DetailType != Enum_WinInfoDetailType.EnmWininfoDetailTypeZimoHu && info.DetailList[i].DetailType != Enum_WinInfoDetailType.EnmWininfoDetailTypeFangpaoHu)
			{
				continue;
			}
			tWinTileTypeV = new TWinTileTypeV();
			list.Add(tWinTileTypeV);
			tWinTileTypeV.nFanCount = (short)info.DetailList[i].FanNodeList.Count;
			tWinTileTypeV.astFanInfo = new TFanV[tWinTileTypeV.nFanCount];
			for (int j = 0; j < tWinTileTypeV.nFanCount; j++)
			{
				tWinTileTypeV.astFanInfo[j] = new TFanV();
				tWinTileTypeV.astFanInfo[j].chType = (sbyte)info.DetailList[i].FanNodeList[j].FanScoreType;
				if (info.DetailList[i].DetailType == Enum_WinInfoDetailType.EnmWininfoDetailTypeZimoHu)
				{
					tWinTileTypeV.astFanInfo[j].iFan = info.DetailList[i].FanNodeList[j].FanCountZimo;
				}
				else
				{
					tWinTileTypeV.astFanInfo[j].iFan = info.DetailList[i].FanNodeList[j].FanCount;
				}
				tWinTileTypeV.astFanInfo[j].iFanIdx = info.DetailList[i].FanNodeList[j].FanIdx;
				tWinTileTypeV.astFanInfo[j].iFanNumDiv = 1;
			}
			tWinTileTypeV.chSeat = (sbyte)GetSeatByPlayerInstanceId(info.DetailList[i].DstPlayer);
			tWinTileTypeV.chTile = (sbyte)GetMJValueByMJCardId(vmKey, info.DetailList[i].RelatedCards[0]);
			tWinTileTypeV.iCount = 1;
		}
		return list.ToArray();
	}

	public static TWinTileType[] GetWinTileTypeFromV(TWinTileTypeV[] vList)
	{
		if (WrappersManagerImpl.IsPatched(35129))
		{
			return WrappersManagerImpl.GetPatch(35129).__Gen_Wrap_3352(vList);
		}
		List<TWinTileType> list = new List<TWinTileType>();
		for (int i = 0; i < vList.Length; i++)
		{
			TWinTileType item = BalanceUtil.ConvertFromTWinTileTypeV(vList[i]);
			list.Add(item);
		}
		return list.ToArray();
	}
}
