using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HappyMahjong.Common;
using IFix;
using IFix.Core;
using TSDK4CSharp;
using UnityEngine;

namespace HappyMahjong;

public class IFixManager : SingletonBase<IFixManager>
{
	private List<(Assembly, VirtualMachine)> assemblies = new List<(Assembly, VirtualMachine)>();

	public int IsOpenBeforeLogin { get; set; }

	public string IFixControllerURL { get; set; }

	public int IsReloadOpen
	{
		get
		{
			return PlayerPrefs.GetInt("int_iFixReloadOpen", 1);
		}
		set
		{
			PlayerPrefs.SetInt("int_iFixReloadOpen", value);
		}
	}

	public string IFixReadyVersion
	{
		get
		{
			return PlayerPrefs.GetString("str_iFixReadyVersion");
		}
		set
		{
			PlayerPrefs.SetString("str_iFixReadyVersion", value);
		}
	}

	public bool CrashFlag
	{
		get
		{
			return PlayerPrefs.GetInt("int_iFixCrashFlag", 0) == 1;
		}
		set
		{
			if (value)
			{
				LastCrashTime = (int)ServerTime.localTime();
			}
			PlayerPrefs.SetInt("int_iFixCrashFlag", value ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	public int LastCrashTime
	{
		get
		{
			return PlayerPrefs.GetInt("int_iFixLastCrashTime", 0);
		}
		set
		{
			PlayerPrefs.SetInt("int_iFixLastCrashTime", value);
		}
	}

	public int CrashTimes
	{
		get
		{
			return PlayerPrefs.GetInt("int_iFixCrashTimes", 0);
		}
		set
		{
			PlayerPrefs.SetInt("int_iFixCrashTimes", value);
		}
	}

	public int RepairCrashTimes
	{
		get
		{
			return PlayerPrefs.GetInt("int_iFixRepairCrashTimes", 3);
		}
		set
		{
			PlayerPrefs.SetInt("int_iFixRepairCrashTimes", value);
		}
	}

	public bool isFix => assemblies.Count > 0;

	public void CheckCrashTimes()
	{
		if (LastCrashTime < SingletonBase<ServerTime>.GetInstance().GetTodayZeroTimeStamp())
		{
			CrashTimes = 0;
			HappyMahjong.Common.Log.Info("CheckCrashTimes crashFlag old! clear!", "IFix");
		}
		else if (CrashFlag)
		{
			int crashTimes = CrashTimes + 1;
			CrashTimes = crashTimes;
			HappyMahjong.Common.Log.Info("CheckCrashTimes crashFlag is true! times: " + CrashTimes, "IFix");
		}
	}

	public bool NeedAutoRepair()
	{
		return CrashTimes >= RepairCrashTimes;
	}

	public void FixBegin(byte[] pBuff, bool isChange = false)
	{
		HappyMahjong.Common.Log.Info("IFixManager FixBegin bChanged " + isChange, "IFix");
		if (pBuff == null)
		{
			HappyMahjong.Common.Log.Info("IFixManager donot flush", "IFix");
			return;
		}
		try
		{
			foreach (var assembly2 in assemblies)
			{
				if (assembly2.Item1 != null)
				{
					if (!SingletonBase<DynamicConfig>.GetInstance().GetBool("Rebuild", "IsFixCanChange"))
					{
						HappyMahjong.Common.Log.Info("IFixManager already exist and IsFixCanChange false", "IFix");
						return;
					}
					PatchManager.Unload(assembly2.Item1);
				}
			}
			assemblies.Clear();
			using MemoryStream stream = new MemoryStream(pBuff);
			CombinedPatchProcessor.ProcessCombinedPatchStream(stream, delegate(Stream stream2)
			{
				try
				{
					VirtualMachine virtualMachine = PatchManager.Load(stream2);
					if (virtualMachine != null)
					{
						Assembly assembly = virtualMachine.WrappersManager.GetType().Assembly;
						assemblies.Add((assembly, virtualMachine));
					}
					else
					{
						HappyMahjong.Common.Log.Error("IFixManager FixBegin load vm null", "IFix");
					}
				}
				catch (Exception arg)
				{
					HappyMahjong.Common.Log.Error($"PatchManagerLoad exception:{arg}", "IFix");
				}
			});
			string softwareVersion = QGDeviceInfo.getSoftwareVersion();
			string value = "Patch" + softwareVersion;
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var assembly3 in assemblies)
			{
				stringBuilder.Clear();
				stringBuilder.Append("iFix InternStrings internStr = ");
				string[] internStrings = assembly3.Item2.InternStrings;
				foreach (string text in internStrings)
				{
					stringBuilder.Append(text + "\n");
				}
				HappyMahjong.Common.Log.Info(stringBuilder.ToString(), "IFix");
				internStrings = assembly3.Item2.InternStrings;
				for (int num = 0; num < internStrings.Length; num++)
				{
					if (internStrings[num].Contains(value))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				HappyMahjong.Common.Log.Info($"========== Warning: IFix version check false, unload, version: {softwareVersion} =========", "IFix");
				assemblies.ForEach(delegate((Assembly, VirtualMachine) tup)
				{
					PatchManager.Unload(tup.Item1);
				});
				assemblies.Clear();
				return;
			}
			HappyMahjong.Common.Log.Info("iFix ready version =" + softwareVersion, "IFix");
			IFixReadyVersion = softwareVersion;
		}
		catch (Exception ex)
		{
			Debug.LogError("IFixManager exception.\n" + ex.Message + "\n" + ex.StackTrace);
		}
		HappyMahjong.Common.Log.Info("IFixManager FixBegin end ", "IFix");
	}

	[Patch]
	private static string GetRevision()
	{
		return "PatchRevision7.5.5x.0001";
	}

	[Patch]
	private string Version()
	{
		return "Patch8.15.122";
	}
}
