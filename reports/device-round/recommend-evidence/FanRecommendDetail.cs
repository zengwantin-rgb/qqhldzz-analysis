using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuanLeGameSo;
using IFix;

namespace HappyMahjong.Game;

[Serializable]
public class FanRecommendDetail
{
	public int targetIndex;

	public int fanIndex;

	public string fanName;

	public int fanCount;

	public int maxFanCount;

	public string fanDesc;

	public string fanDescEx;

	public List<int> proximateMjTiles;

	public List<int> proximateMjTags;

	public List<int> proximateMjSplit;

	public List<int> curTilesInHand;

	public List<int> curTagsInHand;

	public bool isSelected;

	public List<int> proximateMjSplitShow;

	public int score;

	public string paiDesc;

	public List<int> paiList = new List<int>();

	public AchieveTypeEnum achieveType;

	public int noticeType;

	public List<int> destroyMJActions = new List<int>();

	public List<int> destoryMJs = new List<int>();

	public override bool Equals(object obj)
	{
		if (WrappersManagerImpl.IsPatched(37446))
		{
			return WrappersManagerImpl.GetPatch(37446).__Gen_Wrap_460(this, obj);
		}
		if (obj == null)
		{
			return false;
		}
		if (obj is FanRecommendDetail fanRecommendDetail)
		{
			return fanIndex == fanRecommendDetail.fanIndex;
		}
		return false;
	}

	public override string ToString()
	{
		if (WrappersManagerImpl.IsPatched(37447))
		{
			return WrappersManagerImpl.GetPatch(37447).__Gen_Wrap_47(this);
		}
		string text = string.Join(' ', curTilesInHand);
		string text2 = string.Join(' ', curTagsInHand);
		return $"【 FanIndex {fanIndex}, Fanname {fanName}, FanCount {fanCount}, FanDesc {fanDesc}, targetTiles {GetNewProximateMjTileStr()}, CurHandTile {text}, CurHandTag {text2} 】";
	}

	public string GetProximateMjTileStr()
	{
		if (WrappersManagerImpl.IsPatched(37448))
		{
			return WrappersManagerImpl.GetPatch(37448).__Gen_Wrap_47(this);
		}
		int count = proximateMjTiles.Count;
		string[] array = new string[count];
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Clear();
			stringBuilder.Append(proximateMjTiles[i].ToString());
			if ((proximateMjTags[i] & 1) != 0)
			{
				stringBuilder.Append('q');
			}
			if ((proximateMjTags[i] & 2) != 0)
			{
				stringBuilder.Append('t');
			}
			array[i] = stringBuilder.ToString();
		}
		if (proximateMjSplit.Count == 0)
		{
			return string.Join(' ', array);
		}
		int num = 0;
		string[] array2 = new string[proximateMjSplit.Count];
		for (int j = 0; j < proximateMjSplit.Count; j++)
		{
			int num2 = proximateMjSplit[j];
			array2[j] = string.Join(' ', array, num, num2) + ";";
			num += num2;
		}
		return string.Join(' ', array2);
	}

	public string GetNewProximateMjTileStr()
	{
		if (WrappersManagerImpl.IsPatched(30665))
		{
			return WrappersManagerImpl.GetPatch(30665).__Gen_Wrap_47(this);
		}
		int count = proximateMjTiles.Count;
		string[] array = new string[count];
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Clear();
			stringBuilder.Append(proximateMjTiles[i].ToString());
			if ((proximateMjTags[i] & 1) != 0)
			{
				stringBuilder.Append('q');
			}
			if ((proximateMjTags[i] & 2) != 0)
			{
				stringBuilder.Append('t');
			}
			array[i] = stringBuilder.ToString();
		}
		if (proximateMjSplit.Count == 0)
		{
			return string.Join(' ', array);
		}
		int num = 0;
		string[] array2 = new string[proximateMjSplit.Count];
		for (int j = 0; j < proximateMjSplit.Count; j++)
		{
			int num2 = proximateMjSplit[j];
			if (proximateMjSplitShow[j] != 0)
			{
				if (proximateMjSplitShow[j] == 2)
				{
					array2[j] = array[num] + "mg;";
				}
				else if (proximateMjSplitShow[j] == 4)
				{
					array2[j] = array[num] + "bg;";
				}
				else if (proximateMjSplitShow[j] == 3)
				{
					array2[j] = array[num] + "ag;";
				}
				else
				{
					array2[j] = string.Join("d ", array, num, num2) + "d;";
				}
			}
			else
			{
				array2[j] = string.Join(' ', array, num, num2) + ";";
			}
			num += num2;
		}
		return string.Join(' ', array2);
	}

	public string GetSubPaiMjTileStr()
	{
		if (WrappersManagerImpl.IsPatched(30718))
		{
			return WrappersManagerImpl.GetPatch(30718).__Gen_Wrap_47(this);
		}
		int count = paiList.Count;
		string[] array = new string[count];
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Clear();
			stringBuilder.Append(paiList[i].ToString());
			array[i] = stringBuilder.ToString();
		}
		return string.Join(' ', array);
	}

	public static FanRecommendDetail BuildVOData(TOneNewRecommendFanTarget source)
	{
		if (WrappersManagerImpl.IsPatched(36373))
		{
			return WrappersManagerImpl.GetPatch(36373).__Gen_Wrap_2746(source);
		}
		return new FanRecommendDetail
		{
			targetIndex = source.iTargetIdx,
			fanIndex = source.iFanIdx,
			fanCount = source.iFanMulti,
			fanName = source.szFanName,
			fanDesc = source.szFanDesc,
			fanDescEx = source.szExtDesc,
			maxFanCount = source.iMaxFanMulti,
			proximateMjTiles = source.TargetTiles.ToList(),
			proximateMjTags = source.TargetTilesFlag.ToList(),
			proximateMjSplit = source.TargetTilesShowSplit.ToList(),
			proximateMjSplitShow = source.TargetTilesShowSplitShow.ToList(),
			curTilesInHand = source.CurHandTiles.ToList(),
			curTagsInHand = source.CurHandTilesFlagNew.ToList(),
			isSelected = (source.iIsChosen == 1),
			score = source.iScore,
			paiDesc = source.szPaiDesc,
			paiList = source.PaiList.ToList(),
			achieveType = (AchieveTypeEnum)source.iAchieveType,
			noticeType = source.iNoticeType,
			destroyMJActions = source.DestoryNoticeTilesType.ToList(),
			destoryMJs = source.DestoryNoticeTiles.ToList()
		};
	}

	private int ConverToFlag(UserActionType actionType)
	{
		if (WrappersManagerImpl.IsPatched(37385))
		{
			return WrappersManagerImpl.GetPatch(37385).__Gen_Wrap_3556(this, actionType);
		}
		int num = 0;
		return actionType switch
		{
			UserActionType.Drop => 0, 
			UserActionType.Chi => 1, 
			UserActionType.Peng => 2, 
			UserActionType.MingGang => 3, 
			UserActionType.AnGang => 4, 
			UserActionType.BuGang => 5, 
			_ => 31, 
		};
	}

	public bool CheckShowDestroyNotice(int mjValue, UserActionType type)
	{
		if (WrappersManagerImpl.IsPatched(37384))
		{
			return WrappersManagerImpl.GetPatch(37384).__Gen_Wrap_3557(this, mjValue, type);
		}
		for (int i = 0; i < destoryMJs.Count; i++)
		{
			if (destoryMJs[i] == mjValue && (destroyMJActions[i] & (1 << ConverToFlag(type))) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckShowBaseNotice(List<int> list, int mjValue, UserActionType type, IRoundModel roundModel, out int key, out string tips)
	{
		if (WrappersManagerImpl.IsPatched(37379))
		{
			return WrappersManagerImpl.GetPatch(37379).__Gen_Wrap_3555(this, list, mjValue, type, roundModel, out key, out tips);
		}
		int mjValue2 = mjValue;
		tips = string.Empty;
		key = 0;
		if ((noticeType & 4) == 0 || roundModel == null)
		{
			return false;
		}
		List<int> source = curTilesInHand;
		if (source.Where((int tile) => CheckIsZhiPai(tile)).ToList().Count > 1 && (mjValue2 + 31 == roundModel.roundWind || mjValue2 + 31 == roundModel.menFeng) && type == UserActionType.Drop)
		{
			tips = "手上已有其他字牌的单牌,确定打出场风/门风吗？";
			key = 1;
			return true;
		}
		if (source.Count((int tile) => tile == mjValue2) >= 3 && type == UserActionType.Drop)
		{
			tips = "打出这张牌会拆掉刻子，确定吗？";
			key = 2;
			return true;
		}
		return false;
	}

	private bool CheckIsZhiPai(int mjValue)
	{
		if (WrappersManagerImpl.IsPatched(37382))
		{
			return WrappersManagerImpl.GetPatch(37382).__Gen_Wrap_1659(this, mjValue);
		}
		if (mjValue <= 37)
		{
			return mjValue >= 31;
		}
		return false;
	}

	public bool _003C_003EiFixBaseProxy_Equals(object P0)
	{
		return base.Equals(P0);
	}

	public string _003C_003EiFixBaseProxy_ToString()
	{
		return base.ToString();
	}
}
