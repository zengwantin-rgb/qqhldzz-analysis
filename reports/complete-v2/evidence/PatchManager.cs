using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace IFix.Core;

public static class PatchManager
{
	private static Dictionary<Assembly, Action> removers = new Dictionary<Assembly, Action>();

	public static VirtualMachine Load(string filepath)
	{
		using FileStream stream = File.Open(filepath, FileMode.Open);
		return Load(stream);
	}

	private static MethodBase readMethod(BinaryReader reader, Type[] externTypes)
	{
		bool flag = reader.ReadBoolean();
		BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		if (flag)
		{
			Type type = externTypes[reader.ReadInt32()];
			string text = reader.ReadString();
			int num = reader.ReadInt32();
			Type[] array = new Type[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = externTypes[reader.ReadInt32()];
			}
			int num2 = reader.ReadInt32();
			object[] array2 = new object[num2];
			for (int j = 0; j < num2; j++)
			{
				bool flag2 = reader.ReadBoolean();
				array2[j] = ((!flag2) ? ((object)externTypes[reader.ReadInt32()]) : ((object)reader.ReadString()));
			}
			MethodInfo methodInfo = null;
			MethodInfo[] methods = type.GetMethods(bindingAttr);
			foreach (MethodInfo methodInfo2 in methods)
			{
				ParameterInfo[] parameters = methodInfo2.GetParameters();
				Type[] genericArgInfos = null;
				if (methodInfo2.IsGenericMethodDefinition)
				{
					genericArgInfos = methodInfo2.GetGenericArguments();
				}
				bool flag3 = parameters.Length == num2 && methodInfo2.Name == text;
				if (flag3 && num > 0 && (!methodInfo2.IsGenericMethodDefinition || genericArgInfos.Length != num))
				{
					flag3 = false;
				}
				if (flag3)
				{
					for (int l = 0; l < num2; l++)
					{
						if (array2[l] is string input)
						{
							if (!methodInfo2.IsGenericMethodDefinition)
							{
								flag3 = false;
								break;
							}
							string text2 = Regex.Replace(input, "!!\\d+", (Match m) => genericArgInfos[int.Parse(m.Value.Substring(2))].Name);
							if (text2 != parameters[l].ParameterType.ToString())
							{
								flag3 = false;
								break;
							}
						}
						else if (array2[l] as Type != parameters[l].ParameterType)
						{
							flag3 = false;
							break;
						}
					}
				}
				if (flag3)
				{
					methodInfo = methodInfo2;
					break;
				}
			}
			if (methodInfo == null)
			{
				throw new Exception("can not load generic method [" + text + "] of " + type);
			}
			return methodInfo.MakeGenericMethod(array);
		}
		Type type2 = externTypes[reader.ReadInt32()];
		string text3 = reader.ReadString();
		int num3 = reader.ReadInt32();
		Type[] array3 = new Type[num3];
		for (int num4 = 0; num4 < num3; num4++)
		{
			array3[num4] = externTypes[reader.ReadInt32()];
		}
		bool flag4 = text3 == ".ctor" || text3 == ".cctor";
		MethodBase methodBase = null;
		if (flag4)
		{
			methodBase = type2.GetConstructor((BindingFlags)(0x10 | ((!(text3 == ".ctor")) ? 8 : 4) | 0x20), null, array3, null);
		}
		else
		{
			MethodInfo[] methods2 = type2.GetMethods(bindingAttr);
			foreach (MethodInfo methodInfo3 in methods2)
			{
				if (methodInfo3.Name == text3 && !methodInfo3.IsGenericMethodDefinition && methodInfo3.GetParameters().Length == num3)
				{
					IEnumerable<Type> first = from p in methodInfo3.GetParameters()
						select p.ParameterType;
					if (first.SequenceEqual(array3))
					{
						methodBase = methodInfo3;
						break;
					}
				}
			}
		}
		if (methodBase == null)
		{
			throw new Exception("can not load method [" + text3 + "] of " + type2);
		}
		return methodBase;
	}

	private static FieldInfo getRedirectField(MethodBase method)
	{
		string text = "IFix.RedirectTo." + method.DeclaringType.AssemblyQualifiedName;
		Type type = Type.GetType(text);
		if (type == null)
		{
			throw new Exception("cat not find redirect type: " + text);
		}
		IDTagAttribute iDTagAttribute = Attribute.GetCustomAttribute(method, typeof(IDTagAttribute), inherit: false) as IDTagAttribute;
		string text2 = $"_rf_{method.Name}{iDTagAttribute?.ID ?? 0}";
		FieldInfo field = type.GetField(text2, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		if (field == null)
		{
			throw new Exception($"cat not find redirect field: {text2}, for {type}");
		}
		return field;
	}

	private static int getMapId(List<Type> idMapArray, MethodBase method)
	{
		int num = ((Attribute.GetCustomAttribute(method, typeof(IDTagAttribute), inherit: false) is IDTagAttribute iDTagAttribute) ? iDTagAttribute.ID : 0);
		string text = $"{method.DeclaringType.FullName.Replace('.', '-').Replace('+', '-')}-{method.Name}{num}";
		FieldInfo fieldInfo = null;
		for (int i = 0; i < idMapArray.Count; i++)
		{
			fieldInfo = idMapArray[i].GetField(text);
			if (fieldInfo != null)
			{
				break;
			}
		}
		if (fieldInfo == null)
		{
			throw new Exception($"cat not find id field: {text}, for {method}");
		}
		return (int)fieldInfo.GetValue(null);
	}

	private static int[] readSlotInfo(BinaryReader reader, Dictionary<MethodInfo, int> itfMethodToId, Type[] externTypes, int maxId)
	{
		int num = reader.ReadInt32();
		if (num == 0)
		{
			return null;
		}
		int[] array = new int[maxId + 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = -1;
		}
		for (int j = 0; j < num; j++)
		{
			int num2 = reader.ReadInt32();
			Type type = externTypes[num2];
			MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo methodInfo in methods)
			{
				int num3 = reader.ReadInt32();
				if (!itfMethodToId.ContainsKey(methodInfo))
				{
					throw new Exception(string.Concat("can not find slot for ", methodInfo, " of ", type));
				}
				array[itfMethodToId[methodInfo]] = num3;
			}
		}
		return array;
	}

	public unsafe static VirtualMachine Load(Stream stream, bool checkNew = true)
	{
		List<IntPtr> nativePointers = new List<IntPtr>();
		Instruction** ptr = null;
		List<ExceptionHandler[]> list = new List<ExceptionHandler[]>();
		Dictionary<int, NewFieldInfo> dictionary = new Dictionary<int, NewFieldInfo>();
		using BinaryReader binaryReader = new BinaryReader(stream);
		ulong num = binaryReader.ReadUInt64();
		if (num != 8881892042087324221L)
		{
			throw new Exception("instruction magic not match, expect " + 8881892042087324221uL + ", but got " + num);
		}
		string text = binaryReader.ReadString();
		Type type = Type.GetType(text);
		if (type == null)
		{
			throw new Exception("assembly may be not injected yet, cat find " + text);
		}
		int num2 = binaryReader.ReadInt32();
		Type[] array = new Type[num2];
		for (int i = 0; i < num2; i++)
		{
			string text2 = binaryReader.ReadString();
			array[i] = Type.GetType(text2);
			if (array[i] == null)
			{
				throw new Exception("can not load type [" + text2 + "]");
			}
		}
		int num3 = binaryReader.ReadInt32();
		IntPtr item = Marshal.AllocHGlobal(sizeof(Instruction*) * num3);
		ptr = (Instruction**)item.ToPointer();
		nativePointers.Add(item);
		for (int j = 0; j < num3; j++)
		{
			int num4 = binaryReader.ReadInt32();
			item = Marshal.AllocHGlobal(sizeof(Instruction) * num4);
			Instruction* ptr2 = (Instruction*)item.ToPointer();
			for (int k = 0; k < num4; k++)
			{
				ptr2[k].Code = (Code)binaryReader.ReadInt32();
				ptr2[k].Operand = binaryReader.ReadInt32();
			}
			ptr[j] = ptr2;
			nativePointers.Add(item);
			ExceptionHandler[] array2 = new ExceptionHandler[binaryReader.ReadInt32()];
			for (int l = 0; l < array2.Length; l++)
			{
				ExceptionHandler exceptionHandler = new ExceptionHandler();
				exceptionHandler.HandlerType = (ExceptionHandlerType)binaryReader.ReadInt32();
				exceptionHandler.CatchTypeId = binaryReader.ReadInt32();
				exceptionHandler.TryStart = binaryReader.ReadInt32();
				exceptionHandler.TryEnd = binaryReader.ReadInt32();
				exceptionHandler.HandlerStart = binaryReader.ReadInt32();
				exceptionHandler.HandlerEnd = binaryReader.ReadInt32();
				array2[l] = exceptionHandler;
				if (exceptionHandler.HandlerType == ExceptionHandlerType.Catch)
				{
					exceptionHandler.CatchType = ((exceptionHandler.CatchTypeId != -1) ? array[exceptionHandler.CatchTypeId] : typeof(object));
				}
			}
			list.Add(array2);
		}
		int num5 = binaryReader.ReadInt32();
		MethodBase[] array3 = new MethodBase[num5];
		for (int m = 0; m < num5; m++)
		{
			array3[m] = readMethod(binaryReader, array);
		}
		int num6 = binaryReader.ReadInt32();
		string[] array4 = new string[num6];
		for (int n = 0; n < num6; n++)
		{
			array4[n] = binaryReader.ReadString();
		}
		FieldInfo[] array5 = new FieldInfo[binaryReader.ReadInt32()];
		for (int num7 = 0; num7 < array5.Length; num7++)
		{
			bool flag = binaryReader.ReadBoolean();
			Type type2 = array[binaryReader.ReadInt32()];
			string text3 = binaryReader.ReadString();
			array5[num7] = type2.GetField(text3, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (!flag)
			{
				if (array5[num7] == null)
				{
					throw new Exception("can not load field [" + text3 + "]  of " + type2);
				}
				continue;
			}
			Type type3 = array[binaryReader.ReadInt32()];
			int methodId = binaryReader.ReadInt32();
			if (array5[num7] == null)
			{
				dictionary.Add(num7, new NewFieldInfo
				{
					Name = text3,
					FieldType = type3,
					DeclaringType = type2,
					MethodId = methodId
				});
				continue;
			}
			if (array5[num7].FieldType != type3)
			{
				throw new Exception(string.Concat("can not change existing field [", type2, ".", text3, "]'s type  from ", array5[num7].FieldType, " to ", type3));
			}
			if (checkNew)
			{
				throw new Exception(string.Concat(type2, ".", text3, " is expected to be a new field , but it already exists "));
			}
		}
		Type[] array6 = new Type[binaryReader.ReadInt32()];
		int[] array7 = new int[array6.Length];
		for (int num8 = 0; num8 < array6.Length; num8++)
		{
			array6[num8] = array[binaryReader.ReadInt32()];
			array7[num8] = binaryReader.ReadInt32();
		}
		Dictionary<MethodInfo, int> dictionary2 = new Dictionary<MethodInfo, int>();
		int num9 = 0;
		Type[] interfaces = type.GetInterfaces();
		foreach (Type interfaceType in interfaces)
		{
			InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceType);
			for (int num11 = 0; num11 < interfaceMap.InterfaceMethods.Length; num11++)
			{
				IDTagAttribute iDTagAttribute = Attribute.GetCustomAttribute(interfaceMap.TargetMethods[num11], typeof(IDTagAttribute), inherit: false) as IDTagAttribute;
				MethodInfo methodInfo = interfaceMap.InterfaceMethods[num11];
				if (iDTagAttribute == null)
				{
					throw new Exception($"can not find id for {methodInfo}");
				}
				int iD = iDTagAttribute.ID;
				num9 = ((iD <= num9) ? num9 : iD);
				dictionary2.Add(methodInfo, iD);
			}
		}
		AnonymousStoreyInfo[] array8 = new AnonymousStoreyInfo[binaryReader.ReadInt32()];
		for (int num12 = 0; num12 < array8.Length; num12++)
		{
			int num13 = binaryReader.ReadInt32();
			int[] array9 = new int[num13];
			for (int num14 = 0; num14 < num13; num14++)
			{
				array9[num14] = binaryReader.ReadInt32();
			}
			int ctorId = binaryReader.ReadInt32();
			int ctorParamNum = binaryReader.ReadInt32();
			int[] slots = readSlotInfo(binaryReader, dictionary2, array, num9);
			int num15 = binaryReader.ReadInt32();
			int[] array10 = new int[num15];
			for (int num16 = 0; num16 < num15; num16++)
			{
				array10[num16] = binaryReader.ReadInt32();
			}
			array8[num12] = new AnonymousStoreyInfo
			{
				CtorId = ctorId,
				FieldNum = num13,
				FieldTypes = array9,
				CtorParamNum = ctorParamNum,
				Slots = slots,
				VTable = array10
			};
		}
		VirtualMachine virtualMachine = new VirtualMachine(ptr, delegate
		{
			for (int num25 = 0; num25 < nativePointers.Count; num25++)
			{
				Marshal.FreeHGlobal(nativePointers[num25]);
			}
		});
		virtualMachine.ExternTypes = array;
		virtualMachine.ExternMethods = array3;
		virtualMachine.ExceptionHandlers = list.ToArray();
		virtualMachine.InternStrings = array4;
		virtualMachine.FieldInfos = array5;
		virtualMachine.NewFieldInfos = dictionary;
		virtualMachine.AnonymousStoreyInfos = array8;
		virtualMachine.StaticFieldTypes = array6;
		virtualMachine.Cctors = array7;
		VirtualMachine virtualMachine2 = virtualMachine;
		string typeName = binaryReader.ReadString();
		WrappersManager wrapperManager = Activator.CreateInstance(Type.GetType(typeName, throwOnError: true), virtualMachine2) as WrappersManager;
		if (wrapperManager == null)
		{
			throw new Exception("can not create WrappersManager!");
		}
		virtualMachine2.WrappersManager = wrapperManager;
		string text4 = binaryReader.ReadString();
		List<Type> list2 = new List<Type>();
		for (int num17 = 0; num17 < 100; num17++)
		{
			Type type4 = Type.GetType("IFix.IDMAP" + num17 + text4, throwOnError: false);
			if (type4 == null)
			{
				break;
			}
			list2.Add(type4);
		}
		lock (removers)
		{
			Assembly assembly = wrapperManager.GetType().Assembly;
			if (removers.TryGetValue(assembly, out var value))
			{
				removers.Remove(assembly);
				value();
			}
			int num18 = binaryReader.ReadInt32();
			int[] array11 = new int[num18];
			int[] array12 = new int[num18];
			int num19 = -1;
			for (int num20 = 0; num20 < num18; num20++)
			{
				MethodBase method = readMethod(binaryReader, array);
				int num21 = binaryReader.ReadInt32();
				int mapId = getMapId(list2, method);
				array11[num20] = num21;
				array12[num20] = mapId;
				if (mapId > num19)
				{
					num19 = mapId;
				}
			}
			Array array13 = wrapperManager.InitWrapperArray(num19 + 1) as Array;
			for (int num22 = 0; num22 < num18; num22++)
			{
				object obj = wrapperManager.CreateWrapper(array11[num22]);
				if (obj == null)
				{
					throw new Exception("create wrapper fail");
				}
				array13.SetValue(obj, array12[num22]);
			}
			removers[assembly] = delegate
			{
				wrapperManager.InitWrapperArray(0);
			};
		}
		if (checkNew)
		{
			int num23 = binaryReader.ReadInt32();
			for (int num24 = 0; num24 < num23; num24++)
			{
				string typeName2 = binaryReader.ReadString();
				Type type5 = Type.GetType(typeName2);
				if (type5 != null)
				{
					throw new Exception(string.Concat(type5, " class is expected to be a new class , but it already exists "));
				}
			}
		}
		return virtualMachine2;
	}

	public static void Unload(Assembly assembly)
	{
		lock (removers)
		{
			if (removers.TryGetValue(assembly, out var value))
			{
				removers.Remove(assembly);
				value();
			}
		}
	}
}
