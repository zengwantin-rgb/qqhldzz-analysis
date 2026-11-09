using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using HappyMahjong.Core.Common;
using MemoryMonitor;
using UnityEngine;

namespace HappyMahjong.Common;

public class MemoryMonitorUtil
{
	private static bool _isInitialized = false;

	private static bool _isEnabled = false;

	private static readonly AndroidJavaClass _debugClass = new AndroidJavaClass("android.os.Debug");

	private static readonly AndroidJavaObject _memoryInfo = new AndroidJavaObject("android.os.Debug$MemoryInfo");

	private static readonly List<Dictionary<string, string>> _historyStats = new List<Dictionary<string, string>>();

	private static readonly object _lock = new object();

	private static Dictionary<string, string> currentStats = new Dictionary<string, string>(10);

	public static bool IsEnabled
	{
		get
		{
			if (!_isInitialized)
			{
				LoadConfig();
			}
			return _isEnabled;
		}
	}

	private static void LoadConfig()
	{
		_isInitialized = true;
		_isEnabled = PlayerPrefs.GetInt("MEMORY_MONITOR_ENABLED", 0) == 1;
		Log.Info("[MemoryMonitor] Memory monitor: {_isEnabled}", "MemoryMonitor");
	}

	public static void CaptureMemoryStats(string eventStr)
	{
		if (!IsEnabled)
		{
			return;
		}
		currentStats.Clear();
		currentStats.Add("key", eventStr);
		try
		{
			if (Util.IsAndroidPlatform())
			{
				_debugClass.CallStatic("getMemoryInfo", _memoryInfo);
				AndroidJavaObject androidJavaObject = _memoryInfo.Call<AndroidJavaObject>("getMemoryStats", Array.Empty<object>()).Call<AndroidJavaObject>("entrySet", Array.Empty<object>()).Call<AndroidJavaObject>("iterator", Array.Empty<object>());
				while (androidJavaObject.Call<bool>("hasNext", Array.Empty<object>()))
				{
					AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("next", Array.Empty<object>());
					string key = androidJavaObject2.Call<string>("getKey", Array.Empty<object>());
					string value = androidJavaObject2.Call<string>("getValue", Array.Empty<object>());
					currentStats[key] = value;
				}
				lock (_lock)
				{
					_historyStats.Add(new Dictionary<string, string>(currentStats));
					return;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("MemoryMonitor Error: " + ex.Message);
		}
	}

	private static string FormatDictionary(Dictionary<string, string> dict)
	{
		return string.Join(" ; ", dict.Select((KeyValuePair<string, string> kv) => kv.Key + " : " + kv.Value));
	}

	public static bool WriteHistoryStatsToFile(string fileName = "memory_stats")
	{
		List<Dictionary<string, string>> snapshots;
		lock (_lock)
		{
			if (_historyStats.Count == 0)
			{
				return false;
			}
			snapshots = new List<Dictionary<string, string>>(_historyStats);
			_historyStats.Clear();
		}
		try
		{
			byte[] array = ToProtobufConfig(snapshots).ToByteArray();
			string text = Path.Combine(Application.persistentDataPath, "MemoryStats");
			Directory.CreateDirectory(text);
			string text2 = Path.Combine(text, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.data");
			File.WriteAllBytes(text2, array);
			Log.Info($"Memory data saved: {text2} ({array.Length} bytes)", "MemoryMonitor");
			SingletonBase<Toast>.GetInstance().showUGUI("");
			return true;
		}
		catch (Exception arg)
		{
			Log.Error($"Write failed: {arg}", "MemoryMonitor");
			return false;
		}
	}

	private static MemoryMonitorConfig ReadFromFile(string path)
	{
		try
		{
			byte[] data = File.ReadAllBytes(path);
			return MemoryMonitorConfig.Parser.ParseFrom(data);
		}
		catch (Exception arg)
		{
			Log.Error($"ReadFromFile ex: {arg}", "MemoryMonitor");
			return null;
		}
	}

	public static void ReadFromFiles()
	{
		string text = Path.Combine(Application.persistentDataPath, "MemoryStats");
		if (!Directory.Exists(text))
		{
			Log.Info("MemoryStats directory not found: " + text, "MemoryMonitor");
			return;
		}
		string[] files = Directory.GetFiles(text, "*.data");
		if (files.Length == 0)
		{
			Log.Info("No memory data files found in: " + text, "MemoryMonitor");
			return;
		}
		string text2 = files.OrderByDescending((string f) => File.GetLastWriteTime(f)).FirstOrDefault();
		if (text2 == null)
		{
			Log.Info("Failed to find latest memory data file", "MemoryMonitor");
			return;
		}
		try
		{
			MemoryMonitorConfig memoryMonitorConfig = ReadFromFile(text2);
			Log.Info($"Parsed latest file: {Path.GetFileName(text2)}, Records: {memoryMonitorConfig.statLists.Count}", "MemoryMonitor");
		}
		catch (Exception ex)
		{
			Log.Error("Failed to parse latest file " + text2 + ": " + ex.GetBaseException().Message, "MemoryMonitor");
		}
	}

	public static void ClearHistoryStats()
	{
		lock (_lock)
		{
			_historyStats.Clear();
		}
	}

	private static MemoryMonitorConfig ToProtobufConfig(List<Dictionary<string, string>> snapshots)
	{
		MemoryMonitorConfig memoryMonitorConfig = new MemoryMonitorConfig();
		foreach (Dictionary<string, string> snapshot in snapshots)
		{
			StatItem statItem = new StatItem();
			if (snapshot.TryGetValue("key", out var value))
			{
				statItem.statKey = value;
			}
			foreach (KeyValuePair<string, string> item in snapshot)
			{
				if (!(item.Key == "key"))
				{
					statItem.items.Add(new MemoryItem
					{
						key = item.Key,
						value = item.Value
					});
				}
			}
			memoryMonitorConfig.statLists.Add(statItem);
		}
		return memoryMonitorConfig;
	}

	private static List<Dictionary<string, string>> FromProtobufConfig(MemoryMonitorConfig config)
	{
		List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
		foreach (StatItem statList in config.statLists)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(statList.statKey))
			{
				dictionary["key"] = statList.statKey;
			}
			foreach (MemoryItem item in statList.items)
			{
				dictionary[item.key] = item.value;
			}
			list.Add(dictionary);
		}
		return list;
	}
}
