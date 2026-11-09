using IFix;
using IFix.Core;
using tsf4g_tdr_csharp;

namespace HuanLeGameSo;

public class TDoWaitInfo : tsf4g_csharp_interface, IPackable, IUnpackable
{
	public sbyte chFlag;

	public sbyte chChair;

	public short nFixTime;

	public sbyte chExCmd;

	public sbyte chDrawTileNum;

	public short nTileIndexNum;

	public int[] TileIndex;

	public short nExtraTilesNum;

	public int[] ExtraTiles;

	public static readonly uint BASEVERSION = 1u;

	public static readonly uint CURRVERSION = 1u;

	public TdrError.ErrorType construct()
	{
		if (WrappersManagerImpl.IsPatched(80))
		{
			return WrappersManagerImpl.GetPatch(80).__Gen_Wrap_11(this);
		}
		chFlag = 0;
		chChair = -1;
		nFixTime = 0;
		chExCmd = 0;
		chDrawTileNum = 0;
		nTileIndexNum = 0;
		nExtraTilesNum = 0;
		return TdrError.ErrorType.TDR_NO_ERROR;
	}

	[IDTag(1)]
	public TdrError.ErrorType pack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17573))
		{
			return WrappersManagerImpl.GetPatch(17573).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
		}
		if (buffer == null || buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
		{
			return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
		}
		TdrWriteBuf destBuf = new TdrWriteBuf(ref buffer, size);
		TdrError.ErrorType num = pack(ref destBuf, cutVer);
		if (num == TdrError.ErrorType.TDR_NO_ERROR)
		{
			buffer = destBuf.getBeginPtr();
			usedSize = destBuf.getUsedSize();
		}
		return num;
	}

	[IDTag(0)]
	public TdrError.ErrorType pack(ref TdrWriteBuf destBuf, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17574))
		{
			return WrappersManagerImpl.GetPatch(17574).__Gen_Wrap_1521(this, ref destBuf, cutVer);
		}
		TdrError.ErrorType errorType = TdrError.ErrorType.TDR_NO_ERROR;
		if (cutVer == 0 || CURRVERSION < cutVer)
		{
			cutVer = CURRVERSION;
		}
		if (BASEVERSION > cutVer)
		{
			return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
		}
		errorType = destBuf.writeInt8(chFlag);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chChair);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt16(nFixTime);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chExCmd);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chDrawTileNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt16(nTileIndexNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nTileIndexNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (20 < nTileIndexNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		if (TileIndex != null && TileIndex.Length < nTileIndexNum)
		{
			return TdrError.ErrorType.TDR_ERR_VAR_ARRAY_CONFLICT;
		}
		for (int i = 0; i < nTileIndexNum; i++)
		{
			errorType = destBuf.writeInt32(TileIndex[i]);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
		}
		errorType = destBuf.writeInt16(nExtraTilesNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nExtraTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (20 < nExtraTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		if (ExtraTiles != null && ExtraTiles.Length < nExtraTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_VAR_ARRAY_CONFLICT;
		}
		for (int j = 0; j < nExtraTilesNum; j++)
		{
			errorType = destBuf.writeInt32(ExtraTiles[j]);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
		}
		return errorType;
	}

	[IDTag(1)]
	public TdrError.ErrorType unpack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17575))
		{
			return WrappersManagerImpl.GetPatch(17575).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
		}
		if (buffer == null || buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
		{
			return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
		}
		TdrReadBuf srcBuf = new TdrReadBuf(ref buffer, size);
		TdrError.ErrorType result = unpack(ref srcBuf, cutVer);
		usedSize = srcBuf.getUsedSize();
		return result;
	}

	[IDTag(0)]
	public TdrError.ErrorType unpack(ref TdrReadBuf srcBuf, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17576))
		{
			return WrappersManagerImpl.GetPatch(17576).__Gen_Wrap_1524(this, ref srcBuf, cutVer);
		}
		TdrError.ErrorType errorType = TdrError.ErrorType.TDR_NO_ERROR;
		if (cutVer == 0 || CURRVERSION < cutVer)
		{
			cutVer = CURRVERSION;
		}
		if (BASEVERSION > cutVer)
		{
			return TdrError.ErrorType.TDR_ERR_CUTVER_TOO_SMALL;
		}
		errorType = srcBuf.readInt8(ref chFlag);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt8(ref chChair);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt16(ref nFixTime);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt8(ref chExCmd);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt8(ref chDrawTileNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt16(ref nTileIndexNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nTileIndexNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (20 < nTileIndexNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		TileIndex = new int[nTileIndexNum];
		for (int i = 0; i < nTileIndexNum; i++)
		{
			errorType = srcBuf.readInt32(ref TileIndex[i]);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
		}
		errorType = srcBuf.readInt16(ref nExtraTilesNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nExtraTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (20 < nExtraTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		ExtraTiles = new int[nExtraTilesNum];
		for (int j = 0; j < nExtraTilesNum; j++)
		{
			errorType = srcBuf.readInt32(ref ExtraTiles[j]);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
		}
		return errorType;
	}

	[IDTag(0)]
	public TdrError.ErrorType load(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17577))
		{
			return WrappersManagerImpl.GetPatch(17577).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType load(ref TdrReadBuf srcBuf, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17578))
		{
			return WrappersManagerImpl.GetPatch(17578).__Gen_Wrap_1524(this, ref srcBuf, cutVer);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(0)]
	public TdrError.ErrorType visualize(ref string buffer, int indent, char separator)
	{
		if (WrappersManagerImpl.IsPatched(17579))
		{
			return WrappersManagerImpl.GetPatch(17579).__Gen_Wrap_1525(this, ref buffer, indent, separator);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType visualize(ref TdrVisualBuf destBuf, int indent, char separator)
	{
		if (WrappersManagerImpl.IsPatched(17580))
		{
			return WrappersManagerImpl.GetPatch(17580).__Gen_Wrap_1526(this, ref destBuf, indent, separator);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType getSizeInfo(byte[] buffer, int size, ref uint sizeInfo)
	{
		if (WrappersManagerImpl.IsPatched(17581))
		{
			return WrappersManagerImpl.GetPatch(17581).__Gen_Wrap_1528(this, buffer, size, ref sizeInfo);
		}
		if (buffer.GetLength(0) == 0 || size > buffer.GetLength(0))
		{
			return TdrError.ErrorType.TDR_ERR_INVALID_BUFFER_PARAMETER;
		}
		TdrReadBuf srcBuf = new TdrReadBuf(ref buffer, size);
		return getSizeInfo(srcBuf, ref sizeInfo);
	}

	[IDTag(0)]
	public TdrError.ErrorType getSizeInfo(TdrReadBuf srcBuf, ref uint sizeInfo)
	{
		if (WrappersManagerImpl.IsPatched(17582))
		{
			return WrappersManagerImpl.GetPatch(17582).__Gen_Wrap_1527(this, srcBuf, ref sizeInfo);
		}
		return TdrError.ErrorType.TDR_ERR_HAVE_NOT_SET_SIZEINFO;
	}
}
