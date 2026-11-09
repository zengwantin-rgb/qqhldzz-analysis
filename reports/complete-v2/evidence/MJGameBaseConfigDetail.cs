using System.Collections.Generic;
using HappyMahjong.Game;
using IFix;

namespace GameModule;

public class MJGameBaseConfigDetail
{
	public int GameId;

	public int totalPlayerNum = 4;

	public int totalCardNum = 136;

	public RedrawLaiziPos redrawLaiziPos;

	public bool canGiveOutLaizi;

	public bool gangRedrawFromWallFront = true;

	public bool angangShowOneCard = true;

	public int haidiCardNum;

	public bool canBuhua;

	public bool canTing = true;

	public bool showTingGiveCard = true;

	public bool afterTingShowAllHandTile;

	public bool afterTingShowHuHandTile;

	public int qihuFanNum;

	public bool hasJujianJiesuan;

	public bool canRecharge;

	public LaiziGiveOutType laiziGiveOutType;

	public bool laizipiGang;

	public int huEndGame = 1;

	public IgnoreType ignoreType;

	public int tingTipsShowType;

	public bool hujiaozhuanyizhuangang;

	public bool fanMulti;

	public bool showBaojiaoBtn;

	public BaoJiaoType baojiaoType = BaoJiaoType.BaoTing;

	public bool zhuaBaoZi;

	public JiValueType jiValueType;

	public TingShowType tingType;

	public bool paoPaiCanGiveOut;

	public LaiZiType nLaiziType;

	public int nSanTanType = -1;

	public int tileIgnorTypes;

	public int enableGangAfterChiPeng;

	public int luoDiMJType;

	public bool h3zDifferentKind;

	public BalanceUIType balanceUIType;

	public HuLimit huLimit;

	public bool showPlayerHuTimes;

	public bool isHideTingEstimateBtn;

	public int moduleTag;

	public HuaType huaType;

	public bool isBigFan;

	public int ShowFanAniNum;

	public bool IsLaiziInPeng;

	public int LaiziNum = 6;

	public bool DaPiaoQiangZhuang;

	public int HuAddPerTimes;

	public bool bChangeQuePai;

	public int multiTileCount = 3;

	public bool newGangChooseUI;

	public bool showRecommendFanPanel;

	public bool HasSingleWall;

	public bool IsParallelDraw;

	public bool bSystemChangeCard;

	public int bBaoTingJiaFan;

	public bool showPlayerHuTotalWinFanCount;

	public bool disableMJCheckFunc;

	public bool disableMopaiAni;

	public bool disableQuickPickStageFlipWall;

	public bool isFlowerAsLaiziGang;

	public bool enableFaCaiDiscardLimit;

	public void SetConfigValue(int index, int value)
	{
		if (WrappersManagerImpl.IsPatched(1973))
		{
			WrappersManagerImpl.GetPatch(1973).__Gen_Wrap_37(this, index, value);
			return;
		}
		switch (index)
		{
		case 0:
			totalPlayerNum = value;
			break;
		case 1:
			totalCardNum = value;
			break;
		case 2:
			redrawLaiziPos = (RedrawLaiziPos)value;
			break;
		case 3:
			canGiveOutLaizi = value == 1;
			break;
		case 4:
			gangRedrawFromWallFront = value == 1;
			break;
		case 5:
			angangShowOneCard = value == 1;
			break;
		case 6:
			haidiCardNum = value;
			break;
		case 7:
			canBuhua = value == 1;
			break;
		case 8:
			canTing = value == 1;
			break;
		case 9:
			showTingGiveCard = value == 1;
			break;
		case 10:
			afterTingShowAllHandTile = value == 1;
			break;
		case 11:
			afterTingShowHuHandTile = value == 1;
			break;
		case 12:
			qihuFanNum = value;
			break;
		case 13:
			hasJujianJiesuan = value == 1;
			break;
		case 14:
			canRecharge = value == 1;
			break;
		case 15:
			laiziGiveOutType = (LaiziGiveOutType)value;
			break;
		case 16:
			laizipiGang = value == 1;
			break;
		case 17:
			huEndGame = value;
			break;
		case 18:
			ignoreType = (IgnoreType)value;
			break;
		case 19:
			tingTipsShowType = value;
			break;
		case 20:
			hujiaozhuanyizhuangang = value == 1;
			break;
		case 21:
			fanMulti = value == 1;
			break;
		case 23:
			showBaojiaoBtn = value == 1;
			break;
		case 24:
			baojiaoType = (BaoJiaoType)value;
			break;
		case 25:
			zhuaBaoZi = value == 1;
			break;
		case 26:
			jiValueType = (JiValueType)value;
			break;
		case 27:
			tingType = (TingShowType)value;
			break;
		case 28:
			paoPaiCanGiveOut = value == 1;
			break;
		case 29:
			nLaiziType = (LaiZiType)value;
			break;
		case 30:
			nSanTanType = value;
			break;
		case 31:
			tileIgnorTypes = value;
			break;
		case 32:
			enableGangAfterChiPeng = value;
			break;
		case 33:
			luoDiMJType = value;
			break;
		case 34:
			h3zDifferentKind = value == 1;
			break;
		case 35:
			balanceUIType = (BalanceUIType)value;
			break;
		case 36:
			huLimit = (HuLimit)value;
			break;
		case 37:
			showPlayerHuTimes = value == 1;
			break;
		case 38:
			isHideTingEstimateBtn = value == 1;
			break;
		case 39:
			moduleTag = value;
			break;
		case 40:
			huaType = (HuaType)value;
			break;
		case 41:
			isBigFan = value == 1;
			break;
		case 42:
			ShowFanAniNum = value;
			break;
		case 43:
			IsLaiziInPeng = value == 1;
			break;
		case 44:
			LaiziNum = value;
			break;
		case 45:
			DaPiaoQiangZhuang = value == 1;
			break;
		case 46:
			HuAddPerTimes = value;
			break;
		case 47:
			bChangeQuePai = value == 1;
			break;
		case 48:
			multiTileCount = value;
			break;
		case 49:
			newGangChooseUI = value == 1;
			break;
		case 50:
			showRecommendFanPanel = value == 1;
			break;
		case 51:
			HasSingleWall = value == 1;
			break;
		case 52:
			IsParallelDraw = value == 1;
			break;
		case 53:
			bSystemChangeCard = value == 1;
			break;
		case 54:
			bBaoTingJiaFan = ((value > 0) ? value : 0);
			break;
		case 55:
			showPlayerHuTotalWinFanCount = value == 1;
			break;
		case 56:
			disableMJCheckFunc = value == 1;
			break;
		case 57:
			disableMopaiAni = value == 1;
			break;
		case 58:
			disableQuickPickStageFlipWall = value == 1;
			break;
		case 59:
			isFlowerAsLaiziGang = value == 1;
			break;
		case 60:
			enableFaCaiDiscardLimit = value == 1;
			break;
		case 22:
			break;
		}
	}

	public bool IsSCMJEndGame()
	{
		if (WrappersManagerImpl.IsPatched(20554))
		{
			return WrappersManagerImpl.GetPatch(20554).__Gen_Wrap_3(this);
		}
		if (huEndGame != 2)
		{
			return huEndGame == 3;
		}
		return true;
	}

	public bool IsContainLuoDiMJ()
	{
		if (WrappersManagerImpl.IsPatched(20555))
		{
			return WrappersManagerImpl.GetPatch(20555).__Gen_Wrap_3(this);
		}
		return luoDiMJType == 1;
	}

	public bool IsContainTile(int tileValue, List<int> laiziList)
	{
		if (WrappersManagerImpl.IsPatched(20556))
		{
			return WrappersManagerImpl.GetPatch(20556).__Gen_Wrap_431(this, tileValue, laiziList);
		}
		if (tileIgnorTypes == 0)
		{
			return true;
		}
		if (laiziList != null)
		{
			for (int i = 0; i < laiziList.Count; i++)
			{
				if (tileValue == laiziList[i])
				{
					return true;
				}
			}
		}
		int num = tileValue / 10;
		return (tileIgnorTypes & (1 << num)) == 0;
	}
}
