using System;
using System.Collections.Generic;
using Configuration;
using HappyMahjong.Common;
using HappyMahjong.Game;
using HappyMahjong.PrivateRoom;
using HappyMahjong.update;
using IFix;

namespace GameModule;

public class MJGameBaseConfigUtil
{
	public static void FetchConfig(bool loadLocalConfig = false)
	{
		if (WrappersManagerImpl.IsPatched(1964))
		{
			WrappersManagerImpl.GetPatch(1964).__Gen_Wrap_192(loadLocalConfig);
			return;
		}
		CsResMgr.CsResReq csResReq = new CsResMgr.CsResReq();
		csResReq.filename = "MJGameBaseConfig.data";
		csResReq.nCallBack = CsNotify;
		CsResMgr.CsResReq csResReq2 = new CsResMgr.CsResReq();
		csResReq2.filename = "ModuleFanConfig.data";
		csResReq2.nCallBack = CsFanNotify;
		CsResMgr.CsResReq csResReq3 = new CsResMgr.CsResReq();
		csResReq3.filename = "SvrNotifyTips.data";
		csResReq3.nCallBack = CsTipsNotify;
		if (loadLocalConfig)
		{
			CsResMgr.GetInstance().AddLoadConfig(csResReq);
			CsResMgr.GetInstance().AddLoadConfig(csResReq2);
			CsResMgr.GetInstance().AddLoadConfig(csResReq3);
		}
		else
		{
			CsResMgr.GetInstance().GetConfig(csResReq);
			CsResMgr.GetInstance().GetConfig(csResReq2);
			CsResMgr.GetInstance().GetConfig(csResReq3);
		}
	}

	private static void CsNotify(string name, byte[] pBuff, int iRuleID, bool changed, bool successed)
	{
		if (WrappersManagerImpl.IsPatched(1965))
		{
			WrappersManagerImpl.GetPatch(1965).__Gen_Wrap_427(name, pBuff, iRuleID, changed, successed);
		}
		else if (successed && (ProtoConfigLoader<MJGameBaseConfig>.getInstance().getAllCachedConfig().Count <= 0 || ProtoConfigLoader<EncounterRoundSkill>.getInstance().getAllCachedConfig().Count <= 0 || ProtoConfigLoader<EncounterRoundBaseAbility>.getInstance().getAllCachedConfig().Count <= 0 || changed))
		{
			ProtoConfigLoader<MJGameBaseConfig>.getInstance().releaseConfig();
			ProtoConfigLoader<MJGameBaseConfig>.getInstance().load(pBuff);
			SingletonBase<AutoPlayConfigManager>.GetInstance().ParseAutoPlayConfig();
		}
	}

	private static void CsFanNotify(string name, byte[] pBuff, int iRuleID, bool changed, bool successed)
	{
		if (WrappersManagerImpl.IsPatched(1970))
		{
			WrappersManagerImpl.GetPatch(1970).__Gen_Wrap_427(name, pBuff, iRuleID, changed, successed);
		}
		else if (successed && (ProtoConfigLoader<ModuleFanConfig>.getInstance().getAllCachedConfig().Count <= 0 || changed))
		{
			ProtoConfigLoader<ModuleFanConfig>.getInstance().releaseConfig();
			ProtoConfigLoader<ModuleFanConfig>.getInstance().load(pBuff);
		}
	}

	private static void CsTipsNotify(string name, byte[] pBuff, int iRuleID, bool changed, bool successed)
	{
		if (WrappersManagerImpl.IsPatched(1971))
		{
			WrappersManagerImpl.GetPatch(1971).__Gen_Wrap_427(name, pBuff, iRuleID, changed, successed);
		}
		else if (successed && (ProtoConfigLoader<SvrNotifyTips>.getInstance().getAllCachedConfig().Count <= 0 || changed))
		{
			ProtoConfigLoader<SvrNotifyTips>.getInstance().releaseConfig();
			ProtoConfigLoader<SvrNotifyTips>.getInstance().load(pBuff);
		}
	}

	public static MJGameBaseConfigDetail GetBaseConfigByGameID(int gameID)
	{
		if (WrappersManagerImpl.IsPatched(1963))
		{
			return WrappersManagerImpl.GetPatch(1963).__Gen_Wrap_428(gameID);
		}
		MJGameBaseConfigDetail mJGameBaseConfigDetail = new MJGameBaseConfigDetail();
		mJGameBaseConfigDetail.GameId = gameID;
		List<MJGameBaseConfig> allCachedConfig = ProtoConfigLoader<MJGameBaseConfig>.getInstance().getAllCachedConfig();
		if (allCachedConfig.Count == 0)
		{
			Log.Info("MJGameBaseConfigDetail GetBaseConifgByGameID listCount zero", "GameData");
			FetchConfig();
			allCachedConfig = ProtoConfigLoader<MJGameBaseConfig>.getInstance().getAllCachedConfig();
		}
		Log.Info("MJGameBaseConfigDetail GetBaseConifgByGameID listCount = " + allCachedConfig.Count, "GameData");
		for (int i = 0; i < allCachedConfig.Count; i++)
		{
			if (allCachedConfig[i].id == gameID)
			{
				Log.Info("MJGameBaseConfigDetail GetBaseConifgByGameID configValues = " + allCachedConfig[i].configValues, "GameData");
				string[] array = allCachedConfig[i].configValues.Split(';');
				for (int j = 0; j < array.Length; j++)
				{
					mJGameBaseConfigDetail.SetConfigValue(j, Convert.ToInt32(array[j]));
				}
				break;
			}
		}
		return mJGameBaseConfigDetail;
	}

	public static void ChangeBaseConfigBySvrConfig(ref MJGameBaseConfigDetail baseConfig, PersonalRoomByteConfig byteConfig)
	{
		if (WrappersManagerImpl.IsPatched(20557))
		{
			WrappersManagerImpl.GetPatch(20557).__Gen_Wrap_1059(ref baseConfig, byteConfig);
			return;
		}
		List<MJGameBaseChangeConfig> mJGameBaseConfigList = SingletonBase<PersonalRoomController>.GetInstance().GetMJGameBaseConfigList(baseConfig.GameId);
		if (mJGameBaseConfigList == null || mJGameBaseConfigList.Count == 0)
		{
			return;
		}
		foreach (MJGameBaseChangeConfig item in mJGameBaseConfigList)
		{
			if (item.BitType == 1)
			{
				if (item.ByteConfigValue == -1 || item.GameBaseConfigValue == -1)
				{
					bool bit = byteConfig.GetBit(item.ByteConfigIndex);
					baseConfig.SetConfigValue(item.GameBaseConfigIndex, bit ? 1 : 0);
				}
				else if (byteConfig.GetBit(item.ByteConfigIndex) == (item.ByteConfigValue == 1))
				{
					int gameBaseConfigValue = item.GameBaseConfigValue;
					baseConfig.SetConfigValue(item.GameBaseConfigIndex, gameBaseConfigValue);
				}
			}
			else if (item.BitType == 2)
			{
				if (item.ByteConfigValue == -1 || item.GameBaseConfigValue == -1)
				{
					byte value = byteConfig.GetByte(item.ByteConfigIndex);
					baseConfig.SetConfigValue(item.GameBaseConfigIndex, value);
				}
				else if (byteConfig.GetByte(item.ByteConfigIndex) == item.ByteConfigValue)
				{
					int gameBaseConfigValue2 = item.GameBaseConfigValue;
					baseConfig.SetConfigValue(item.GameBaseConfigIndex, gameBaseConfigValue2);
				}
			}
		}
	}
}
