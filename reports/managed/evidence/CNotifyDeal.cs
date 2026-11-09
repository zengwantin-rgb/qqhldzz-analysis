using System;
using IFix;
using IFix.Core;
using tsf4g_tdr_csharp;

namespace HuanLeGameSo;

public class CNotifyDeal : tsf4g_csharp_interface, IPackable, IUnpackable
{
	public short nTilesNum;

	public sbyte[] szTiles;

	public sbyte chDealer;

	public sbyte chRoundWind;

	public sbyte chSubRoundWind;

	public sbyte[] szDice;

	public sbyte chDoorWind;

	public int iContinueWin;

	public short nTLVNum;

	public TSOTLVUNIT[] astTLVInfo;

	public static readonly uint BASEVERSION = 1u;

	public static readonly uint CURRVERSION = 1u;

	public TdrError.ErrorType construct()
	{
		if (WrappersManagerImpl.IsPatched(14833))
		{
			return WrappersManagerImpl.GetPatch(14833).__Gen_Wrap_11(this);
		}
		TdrError.ErrorType errorType = TdrError.ErrorType.TDR_NO_ERROR;
		nTilesNum = 0;
		chDealer = 0;
		chRoundWind = 0;
		chSubRoundWind = 0;
		chDoorWind = 0;
		iContinueWin = 0;
		nTLVNum = 0;
		for (int i = 0; i < 32; i++)
		{
			errorType = astTLVInfo[i].construct();
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
		}
		return errorType;
	}

	[IDTag(1)]
	public TdrError.ErrorType pack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(14836))
		{
			return WrappersManagerImpl.GetPatch(14836).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
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
		if (WrappersManagerImpl.IsPatched(14837))
		{
			return WrappersManagerImpl.GetPatch(14837).__Gen_Wrap_1521(this, ref destBuf, cutVer);
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
		errorType = destBuf.writeInt16(nTilesNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (17 < nTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		if (szTiles != null && szTiles.Length < nTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_VAR_ARRAY_CONFLICT;
		}
		if (nTilesNum > 0)
		{
			int num = nTilesNum;
			byte[] src = new byte[num];
			Buffer.BlockCopy(szTiles, 0, src, 0, num);
			errorType = destBuf.writeBytes(ref src, num);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
		}
		errorType = destBuf.writeInt8(chDealer);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chRoundWind);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chSubRoundWind);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		int num2 = 2;
		byte[] src2 = new byte[num2];
		Buffer.BlockCopy(szDice, 0, src2, 0, num2);
		errorType = destBuf.writeBytes(ref src2, num2);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chDoorWind);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt32(iContinueWin);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt16(nTLVNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nTLVNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (32 < nTLVNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		if (astTLVInfo != null && astTLVInfo.Length < nTLVNum)
		{
			return TdrError.ErrorType.TDR_ERR_VAR_ARRAY_CONFLICT;
		}
		for (int i = 0; i < nTLVNum; i++)
		{
			errorType = astTLVInfo[i].pack(ref destBuf, cutVer);
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
		if (WrappersManagerImpl.IsPatched(14840))
		{
			return WrappersManagerImpl.GetPatch(14840).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
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
		if (WrappersManagerImpl.IsPatched(14841))
		{
			return WrappersManagerImpl.GetPatch(14841).__Gen_Wrap_1524(this, ref srcBuf, cutVer);
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
		errorType = srcBuf.readInt16(ref nTilesNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (17 < nTilesNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		szTiles = new sbyte[nTilesNum];
		if (nTilesNum > 0)
		{
			int num = nTilesNum;
			byte[] dest = new byte[num];
			errorType = srcBuf.readBytes(ref dest, num);
			if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
			{
				return errorType;
			}
			Buffer.BlockCopy(dest, 0, szTiles, 0, num);
		}
		errorType = srcBuf.readInt8(ref chDealer);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt8(ref chRoundWind);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt8(ref chSubRoundWind);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		szDice = new sbyte[2];
		int num2 = 2;
		byte[] dest2 = new byte[num2];
		errorType = srcBuf.readBytes(ref dest2, num2);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		Buffer.BlockCopy(dest2, 0, szDice, 0, num2);
		errorType = srcBuf.readInt8(ref chDoorWind);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt32(ref iContinueWin);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt16(ref nTLVNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		if (0 > nTLVNum)
		{
			return TdrError.ErrorType.TDR_ERR_MINUS_REFER_VALUE;
		}
		if (32 < nTLVNum)
		{
			return TdrError.ErrorType.TDR_ERR_REFER_SURPASS_COUNT;
		}
		astTLVInfo = new TSOTLVUNIT[nTLVNum];
		for (int i = 0; i < nTLVNum; i++)
		{
			astTLVInfo[i] = new TSOTLVUNIT();
		}
		for (int j = 0; j < nTLVNum; j++)
		{
			errorType = astTLVInfo[j].unpack(ref srcBuf, cutVer);
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
		if (WrappersManagerImpl.IsPatched(14844))
		{
			return WrappersManagerImpl.GetPatch(14844).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType load(ref TdrReadBuf srcBuf, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(14845))
		{
			return WrappersManagerImpl.GetPatch(14845).__Gen_Wrap_1524(this, ref srcBuf, cutVer);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(0)]
	public TdrError.ErrorType visualize(ref string buffer, int indent, char separator)
	{
		if (WrappersManagerImpl.IsPatched(14846))
		{
			return WrappersManagerImpl.GetPatch(14846).__Gen_Wrap_1525(this, ref buffer, indent, separator);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType visualize(ref TdrVisualBuf destBuf, int indent, char separator)
	{
		if (WrappersManagerImpl.IsPatched(14847))
		{
			return WrappersManagerImpl.GetPatch(14847).__Gen_Wrap_1526(this, ref destBuf, indent, separator);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType getSizeInfo(byte[] buffer, int size, ref uint sizeInfo)
	{
		if (WrappersManagerImpl.IsPatched(14848))
		{
			return WrappersManagerImpl.GetPatch(14848).__Gen_Wrap_1528(this, buffer, size, ref sizeInfo);
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
		if (WrappersManagerImpl.IsPatched(14849))
		{
			return WrappersManagerImpl.GetPatch(14849).__Gen_Wrap_1527(this, srcBuf, ref sizeInfo);
		}
		return TdrError.ErrorType.TDR_ERR_HAVE_NOT_SET_SIZEINFO;
	}
}
