using System;
using System.IO;
using System.Reflection;
using HappyMahjong;
using HappyMahjong.Common;
using HappyMahjong.Core.Common;
using HappyMahjong.ResHotUpdate;
using IFix;
using ProtoBuf;

namespace ProtocolHierarchy;

public class ProtocolSerializer
{
	private static Assembly subDomainAssembly;

	public static string SerializeToFile(ProtocolList info, string filePath = "")
	{
		if (WrappersManagerImpl.IsPatched(20008))
		{
			return WrappersManagerImpl.GetPatch(20008).__Gen_Wrap_52(info, filePath);
		}
		if (string.IsNullOrEmpty(filePath))
		{
			string path = string.Format("{0}_ProtocolRecord_{1}.json", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin(), DateTime.Now.ToString("yyMMddHHmm"));
			filePath = Path.Combine(PathManager.GetPath(), path);
		}
		foreach (ProtocolData protocolData in info.ProtocolDataList)
		{
			byte[] inArray;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				Serializer.Serialize(memoryStream, protocolData.Data);
				inArray = memoryStream.ToArray();
			}
			protocolData.BinaryArray = Convert.ToBase64String(inArray);
			if (string.IsNullOrEmpty(protocolData.ClassName))
			{
				protocolData.ClassName = protocolData.Data.GetType().ToString();
			}
		}
		ProtocolList.SaveToFile(filePath, info);
		return filePath;
	}

	public static ProtocolList DeSerializeFromFile(string filePath = "")
	{
		if (WrappersManagerImpl.IsPatched(20023))
		{
			return WrappersManagerImpl.GetPatch(20023).__Gen_Wrap_1019(filePath);
		}
		if (string.IsNullOrEmpty(filePath))
		{
			string path = string.Format("{0}_ProtocolRecord_{1}.json", SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin(), DateTime.Now.ToString("yyMMddHHmm"));
			filePath = Path.Combine(PathManager.GetPath(), path);
		}
		if (!File.Exists(filePath))
		{
			return null;
		}
		ProtocolList protocolList = ProtocolList.CreateFromFile(filePath);
		foreach (ProtocolData protocolData in protocolList.ProtocolDataList)
		{
			byte[] array = Convert.FromBase64String(protocolData.BinaryArray);
			protocolData.Data = DeSerialize(GetTypeEx(protocolData.ClassName), array, 0, array.Length);
		}
		return protocolList;
	}

	public static object DeSerialize(Type type, byte[] buffer, int index, int bufferLen)
	{
		if (WrappersManagerImpl.IsPatched(20000))
		{
			return WrappersManagerImpl.GetPatch(20000).__Gen_Wrap_1020(type, buffer, index, bufferLen);
		}
		MemoryStream source = new MemoryStream(buffer, index, bufferLen);
		return Serializer.Deserialize(type, source);
	}

	private static Type GetTypeSubDomain(string typeName)
	{
		if (WrappersManagerImpl.IsPatched(19999))
		{
			return WrappersManagerImpl.GetPatch(19999).__Gen_Wrap_177(typeName);
		}
		string path = Path.Combine(SingletonBase<UIUpdateManager>.GetInstance().GetPufferUIHotloadRoot("ActivityRes"), "HotUpdateCode/ClassLibrary.dll.bytes");
		Log.Info(typeName, "ProtocolHierarchy");
		if (subDomainAssembly == null && File.Exists(path))
		{
			subDomainAssembly = Assembly.Load(File.ReadAllBytes(path));
		}
		if (subDomainAssembly != null)
		{
			return subDomainAssembly.GetType(typeName);
		}
		return null;
	}

	public static Type GetTypeEx(string typeName)
	{
		if (WrappersManagerImpl.IsPatched(19998))
		{
			return WrappersManagerImpl.GetPatch(19998).__Gen_Wrap_177(typeName);
		}
		Type type = ReflectionManager.GetType(typeName);
		if (type != null)
		{
			return type;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			type = assemblies[i].GetType(typeName);
			if (type != null)
			{
				return type;
			}
		}
		return GetTypeSubDomain(typeName);
	}
}
