using IFix;
using IFix.Core;
using tsf4g_tdr_csharp;

namespace HuanLeGameSo;

public class TMJPlayerInfo : tsf4g_csharp_interface, IPackable, IUnpackable
{
	public short nSeat;

	public short nHandTilesNum;

	public sbyte chStatus;

	public sbyte chFlag;

	public static readonly uint BASEVERSION = 1u;

	public static readonly uint CURRVERSION = 1u;

	public TdrError.ErrorType construct()
	{
		if (WrappersManagerImpl.IsPatched(17913))
		{
			return WrappersManagerImpl.GetPatch(17913).__Gen_Wrap_11(this);
		}
		return TdrError.ErrorType.TDR_NO_ERROR;
	}

	[IDTag(1)]
	public TdrError.ErrorType pack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17914))
		{
			return WrappersManagerImpl.GetPatch(17914).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
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
		if (WrappersManagerImpl.IsPatched(17915))
		{
			return WrappersManagerImpl.GetPatch(17915).__Gen_Wrap_1521(this, ref destBuf, cutVer);
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
		errorType = destBuf.writeInt16(nSeat);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt16(nHandTilesNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = destBuf.writeInt8(chStatus);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		return destBuf.writeInt8(chFlag);
	}

	[IDTag(1)]
	public TdrError.ErrorType unpack(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17916))
		{
			return WrappersManagerImpl.GetPatch(17916).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
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
		if (WrappersManagerImpl.IsPatched(17917))
		{
			return WrappersManagerImpl.GetPatch(17917).__Gen_Wrap_1524(this, ref srcBuf, cutVer);
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
		errorType = srcBuf.readInt16(ref nSeat);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt16(ref nHandTilesNum);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		errorType = srcBuf.readInt8(ref chStatus);
		if (errorType != TdrError.ErrorType.TDR_NO_ERROR)
		{
			return errorType;
		}
		return srcBuf.readInt8(ref chFlag);
	}

	[IDTag(0)]
	public TdrError.ErrorType load(ref byte[] buffer, int size, ref int usedSize, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17918))
		{
			return WrappersManagerImpl.GetPatch(17918).__Gen_Wrap_1522(this, ref buffer, size, ref usedSize, cutVer);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType load(ref TdrReadBuf srcBuf, uint cutVer)
	{
		if (WrappersManagerImpl.IsPatched(17919))
		{
			return WrappersManagerImpl.GetPatch(17919).__Gen_Wrap_1524(this, ref srcBuf, cutVer);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(0)]
	public TdrError.ErrorType visualize(ref string buffer, int indent, char separator)
	{
		if (WrappersManagerImpl.IsPatched(17920))
		{
			return WrappersManagerImpl.GetPatch(17920).__Gen_Wrap_1525(this, ref buffer, indent, separator);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType visualize(ref TdrVisualBuf destBuf, int indent, char separator)
	{
		if (WrappersManagerImpl.IsPatched(17921))
		{
			return WrappersManagerImpl.GetPatch(17921).__Gen_Wrap_1526(this, ref destBuf, indent, separator);
		}
		return TdrError.ErrorType.TDR_ERR_METHOD_NOT_IMPLEMENTED;
	}

	[IDTag(1)]
	public TdrError.ErrorType getSizeInfo(byte[] buffer, int size, ref uint sizeInfo)
	{
		if (WrappersManagerImpl.IsPatched(17922))
		{
			return WrappersManagerImpl.GetPatch(17922).__Gen_Wrap_1528(this, buffer, size, ref sizeInfo);
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
		if (WrappersManagerImpl.IsPatched(17923))
		{
			return WrappersManagerImpl.GetPatch(17923).__Gen_Wrap_1527(this, srcBuf, ref sizeInfo);
		}
		return TdrError.ErrorType.TDR_ERR_HAVE_NOT_SET_SIZEINFO;
	}
}
