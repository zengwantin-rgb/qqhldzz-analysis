using System.Collections.Generic;
using System.Text;
using GameModule;
using HappyMahjong.Common;
using HuanLeGameSo;
using IFix;
using IFix.Core;
using strange.extensions.context.impl;

namespace HappyMahjong.Game;

public class NewFanRecommendDataModel : DataModel
{
	public List<FanRecommendDetail> lastFanRecommendDetails = new List<FanRecommendDetail>();

	public List<FanRecommendDetail> curFanRecommendDetails = new List<FanRecommendDetail>();

	public FanRecommendDetail CurSelectedRecommendDetail;

	public bool hasUnprocessedData;

	public bool disableFanRecommend;

	public bool needShowSelectedEffect;

	public bool recvCloseRecommendCommand;

	public bool ClickFromBtn;

	public int UIMode;

	public bool JihuZimo;

	private List<int> m_showedDestroyRecommendTips = new List<int>();

	public FanRecommendDetail GetCurSelectedRecommendDetail()
	{
		if (WrappersManagerImpl.IsPatched(36375))
		{
			return WrappersManagerImpl.GetPatch(36375).__Gen_Wrap_2746(this);
		}
		foreach (FanRecommendDetail curFanRecommendDetail in curFanRecommendDetails)
		{
			if (curFanRecommendDetail.isSelected)
			{
				return curFanRecommendDetail;
			}
		}
		return null;
	}

	public void SetNewFanRecommendDetailData(List<FanRecommendDetail> datas)
	{
		if (WrappersManagerImpl.IsPatched(36374))
		{
			WrappersManagerImpl.GetPatch(36374).__Gen_Wrap_1(this, datas);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (FanRecommendDetail data in datas)
		{
			stringBuilder.Append(data.ToString());
		}
		HappyMahjong.Common.Log.Info($"SetNewFanRecommendDetailData: count:{datas.Count}, recommendData: {stringBuilder.ToString()}", "NewFanRecommend");
		lastFanRecommendDetails = curFanRecommendDetails;
		curFanRecommendDetails = datas;
		CurSelectedRecommendDetail = GetCurSelectedRecommendDetail();
	}

	public void ToggleDisableFanRecommend(bool disable)
	{
		if (WrappersManagerImpl.IsPatched(37373))
		{
			WrappersManagerImpl.GetPatch(37373).__Gen_Wrap_453(this, disable);
			return;
		}
		HappyMahjong.Common.Log.Info($"SetDisableFanRecommend: disable:{disable}", "NewFanRecommend");
		disableFanRecommend = disable;
	}

	public bool GetShowEffectFlagAndReset()
	{
		if (WrappersManagerImpl.IsPatched(30642))
		{
			return WrappersManagerImpl.GetPatch(30642).__Gen_Wrap_452(this);
		}
		if (!needShowSelectedEffect)
		{
			return false;
		}
		needShowSelectedEffect = false;
		return true;
	}

	public override void Reset()
	{
		if (WrappersManagerImpl.IsPatched(37374))
		{
			WrappersManagerImpl.GetPatch(37374).__Gen_Wrap_0(this);
			return;
		}
		base.Reset();
		UIMode = 0;
		ClickFromBtn = false;
		curFanRecommendDetails.Clear();
		lastFanRecommendDetails.Clear();
		CurSelectedRecommendDetail = null;
		disableFanRecommend = false;
		recvCloseRecommendCommand = false;
		hasUnprocessedData = false;
		ClearDoubleCheckFlag();
		m_showedDestroyRecommendTips.Clear();
	}

	public bool CheckOpRecommend(UserActionType playerOp)
	{
		if (WrappersManagerImpl.IsPatched(37376))
		{
			return WrappersManagerImpl.GetPatch(37376).__Gen_Wrap_3553(this, playerOp);
		}
		return true;
	}

	public void ClearDoubleCheckFlag()
	{
		if (WrappersManagerImpl.IsPatched(37375))
		{
			WrappersManagerImpl.GetPatch(37375).__Gen_Wrap_0(this);
		}
	}

	private bool WillEnterTingAfterAction(List<int> mjList, UserActionType actionType)
	{
		if (WrappersManagerImpl.IsPatched(37377))
		{
			return WrappersManagerImpl.GetPatch(37377).__Gen_Wrap_3554(this, mjList, actionType);
		}
		MVCSContext gameContext = SingletonBase<GameModuleRegistry>.GetInstance().GameContext;
		if (gameContext == null || m_roundModel == null)
		{
			return false;
		}
		IMJRule instance = gameContext.injectionBinder.GetInstance<IMJRule>();
		if (instance == null)
		{
			return false;
		}
		MJTileData tilesByViewSeat = m_roundModel.GetTilesByViewSeat(0);
		if (tilesByViewSeat == null || tilesByViewSeat.inHandList == null)
		{
			return false;
		}
		List<int> list = new List<int>(tilesByViewSeat.inHandList);
		switch (actionType)
		{
		case UserActionType.Drop:
			list.Remove(mjList[0]);
			break;
		case UserActionType.Chi:
		{
			for (int k = 0; k < mjList.Count; k++)
			{
				list.Remove(mjList[k]);
			}
			break;
		}
		case UserActionType.Peng:
		{
			for (int l = 0; l < 2; l++)
			{
				list.Remove(mjList[0]);
			}
			break;
		}
		case UserActionType.MingGang:
		{
			for (int j = 0; j < 3; j++)
			{
				list.Remove(mjList[0]);
			}
			break;
		}
		case UserActionType.AnGang:
		{
			for (int i = 0; i < 4; i++)
			{
				list.Remove(mjList[0]);
			}
			break;
		}
		case UserActionType.BuGang:
			list.Remove(mjList[0]);
			break;
		default:
			return false;
		}
		List<int> tingList = instance.GetTingList(list, m_roundModel.selfSeat);
		if (tingList != null)
		{
			return tingList.Count > 0;
		}
		return false;
	}

	[IDTag(0)]
	private string GetDestroyRecommendFanTip(int mjValue, UserActionType type, List<int> actionTiles)
	{
		if (WrappersManagerImpl.IsPatched(37378))
		{
			return WrappersManagerImpl.GetPatch(37378).__Gen_Wrap_3558(this, mjValue, type, actionTiles);
		}
		int key = 0;
		string tips = string.Empty;
		for (int i = 0; i < curFanRecommendDetails.Count; i++)
		{
			FanRecommendDetail fanRecommendDetail = curFanRecommendDetails[i];
			if (fanRecommendDetail.CheckShowBaseNotice(actionTiles, mjValue, type, m_roundModel, out key, out tips))
			{
				HappyMahjong.Common.Log.Info("CheckShowBaseNotice " + key, "Game");
			}
			else if (fanRecommendDetail.CheckShowDestroyNotice(mjValue, type) && !WillEnterTingAfterAction(actionTiles, type))
			{
				HappyMahjong.Common.Log.Info("CheckShowDestroyRecommendNotice " + key, "Game");
				if (fanRecommendDetail.achieveType == AchieveTypeEnum.ACHIEVE_TYPE_RECOMMEND)
				{
					key = fanRecommendDetail.fanIndex * 1000 + mjValue * 10 + 2;
					tips = $"该操作可能不利于做出该番型：{fanRecommendDetail.fanName}";
					HappyMahjong.Common.Log.Info("GetDestroyRecommendFanTip destroyType==1 showedKey=" + key, "Game");
					break;
				}
				if (fanRecommendDetail.achieveType == AchieveTypeEnum.ACHIEVE_TYPE_ACHIEVE)
				{
					key = fanRecommendDetail.fanIndex * 1000 + mjValue * 10 + 3;
					tips = $"该操作可能不利于做出该番型：{fanRecommendDetail.fanName}";
					HappyMahjong.Common.Log.Info("GetDestroyRecommendFanTip destroyType==2 showedKey=" + key, "Game");
					break;
				}
			}
		}
		if (!m_showedDestroyRecommendTips.Contains(key))
		{
			m_showedDestroyRecommendTips.Add(key);
			return tips;
		}
		return string.Empty;
	}

	[IDTag(1)]
	private string GetDestroyRecommendFanTip(List<int> mjList, UserActionType type)
	{
		if (WrappersManagerImpl.IsPatched(37386))
		{
			return WrappersManagerImpl.GetPatch(37386).__Gen_Wrap_3559(this, mjList, type);
		}
		for (int i = 0; i < mjList.Count; i++)
		{
			string destroyRecommendFanTip = GetDestroyRecommendFanTip(mjList[i], type, mjList);
			if (!string.IsNullOrEmpty(destroyRecommendFanTip))
			{
				return destroyRecommendFanTip;
			}
		}
		return string.Empty;
	}

	public static string CheckGetDestroyRecommendFanTip(List<int> mjList, UserActionType actionType)
	{
		if (WrappersManagerImpl.IsPatched(37387))
		{
			return WrappersManagerImpl.GetPatch(37387).__Gen_Wrap_3560(mjList, actionType);
		}
		if (SingletonBase<MJGameRulesCenter>.GetInstance().GetModuleDataModel(MJGameModuleType.MJGameModuleFanRecommend) is NewFanRecommendDataModel newFanRecommendDataModel)
		{
			return newFanRecommendDataModel.GetDestroyRecommendFanTip(mjList, actionType);
		}
		return string.Empty;
	}

	public void _003C_003EiFixBaseProxy_Reset()
	{
		base.Reset();
	}
}
