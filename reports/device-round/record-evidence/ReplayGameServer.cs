using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using GameModule;
using Google.Protobuf;
using HappyMahjong.Common;
using HappyMahjong.Core.Common;
using HappyMahjong.Loading;
using HappyMahjong.TSDKMessage;
using HappyMahjong.Tutorial;
using HuanLeGameSo;
using IFix;
using IFix.Core;
using LitJson;
using MJSoPb;
using UnityEngine;

namespace HappyMahjong.Game;

public class ReplayGameServer : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStartGameRoutine_003Ed__43 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ReplayGameServer _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CStartGameRoutine_003Ed__43(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ReplayGameServer replayGameServer = _003C_003E4__this;
			if (num != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
			AppRoot.GetInstance().StartCoroutine(replayGameServer.AsynRunMsg());
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CAsynRunMsg_003Ed__44 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ReplayGameServer _003C_003E4__this;

		private object _003CheadMsg_003E5__2;

		private HuanLeGameSo.SoNotifyMsgTV _003CgameMsg_003E5__3;

		private int _003CiType_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CAsynRunMsg_003Ed__44(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			ReplayGameServer replayGameServer = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(1f);
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0061;
			case 2:
			{
				_003C_003E1__state = -1;
				_003CgameMsg_003E5__3 = null;
				MJSoPb.SoNotifyMsgTV soNotifyMsgTV = null;
				SimpleGameMsg simpleGameMsg = _003CheadMsg_003E5__2 as SimpleGameMsg;
				if (simpleGameMsg.isPBType == 0)
				{
					_003CgameMsg_003E5__3 = ((SimpleGameMsg)_003CheadMsg_003E5__2).Conver2GameMsg();
				}
				else if (simpleGameMsg.isPBType == 1)
				{
					soNotifyMsgTV = new MessageParser<MJSoPb.SoNotifyMsgTV>(() => new MJSoPb.SoNotifyMsgTV()).ParseFrom(simpleGameMsg.stValue, 0, simpleGameMsg.size);
				}
				else if (simpleGameMsg.isPBType == 2)
				{
					try
					{
						MJSoPb.SoNotifyMsgTV pbSoNotifyMsgTV = new MessageParser<MJSoPb.SoNotifyMsgTV>(() => new MJSoPb.SoNotifyMsgTV()).ParseFrom(simpleGameMsg.stValue, 0, simpleGameMsg.size);
						_003CgameMsg_003E5__3 = replayGameServer.m_Pb2tdrConverter.Convert2Tdr(pbSoNotifyMsgTV);
					}
					catch (Exception ex)
					{
						HappyMahjong.Common.Log.Info(ex.Message, "Replay");
					}
				}
				_003CiType_003E5__4 = 0;
				if (_003CgameMsg_003E5__3 != null)
				{
					_003CiType_003E5__4 = _003CgameMsg_003E5__3.iType;
					if (_003CiType_003E5__4 == 77)
					{
						_003C_003E2__current = new WaitForSeconds(4f);
						_003C_003E1__state = 3;
						return true;
					}
					goto IL_01ed;
				}
				if (soNotifyMsgTV != null)
				{
					_003CiType_003E5__4 = soNotifyMsgTV.type;
					TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKGamePbMessage, soNotifyMsgTV);
				}
				goto IL_024e;
			}
			case 3:
				_003C_003E1__state = -1;
				goto IL_01ed;
			case 4:
				_003C_003E1__state = -1;
				goto IL_0277;
			case 5:
				_003C_003E1__state = -1;
				goto IL_02ad;
			case 6:
				_003C_003E1__state = -1;
				goto IL_0470;
			case 7:
				_003C_003E1__state = -1;
				goto IL_0470;
			case 8:
				{
					_003C_003E1__state = -1;
					goto IL_0470;
				}
				IL_01ed:
				HappyMahjong.Common.Log.Info($"RecvGameMsgCommand current sequenceID: {_003CgameMsg_003E5__3.iSequenceID}", "CSReplay");
				TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKGameMessage, _003CgameMsg_003E5__3);
				goto IL_024e;
				IL_02ad:
				_003CgameMsg_003E5__3 = null;
				goto IL_0470;
				IL_0061:
				if (replayGameServer.m_replayActions.Count > 0)
				{
					_003CheadMsg_003E5__2 = replayGameServer.m_replayActions.Dequeue();
					if (_003CheadMsg_003E5__2.GetType() == typeof(SimpleGameMsg))
					{
						_003C_003E2__current = new WaitForSeconds(2f);
						_003C_003E1__state = 2;
						return true;
					}
					if (_003CheadMsg_003E5__2.GetType() == typeof(T2UMsgUserReady))
					{
						TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKUserReady, _003CheadMsg_003E5__2);
					}
					else
					{
						if (_003CheadMsg_003E5__2.GetType() == typeof(TSDKToUnity))
						{
							TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKStartGame);
							_003C_003E2__current = new WaitForSeconds(1f);
							_003C_003E1__state = 6;
							return true;
						}
						if (_003CheadMsg_003E5__2.GetType() == typeof(PlayerDataVO))
						{
							PlayerDataVO playerData = _003CheadMsg_003E5__2 as PlayerDataVO;
							PlayerDataMgr.GetInstance().AddPlayerByData(playerData);
							if (replayGameServer.m_replayActions.Peek().GetType() != typeof(PlayerDataVO))
							{
								TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKStartRefreshGameMode);
								_003C_003E2__current = new WaitForSeconds(1f);
								_003C_003E1__state = 7;
								return true;
							}
						}
						else
						{
							if (_003CheadMsg_003E5__2.GetType() == typeof(T2UEnterRooom))
							{
								TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKEnterRoomSuccesss, _003CheadMsg_003E5__2);
								replayGameServer.m_selfPlayerID = ((T2UEnterRooom)_003CheadMsg_003E5__2).playerID;
								_003C_003E2__current = new WaitForSeconds(0.3f);
								_003C_003E1__state = 8;
								return true;
							}
							if (_003CheadMsg_003E5__2.GetType() == typeof(T2UMsgUserEnter))
							{
								TSDK4CSharpService.dispatcher.Dispatch(TSDKMessageEvent.OnTSDKUserEnter, _003CheadMsg_003E5__2);
							}
						}
					}
					goto IL_0470;
				}
				return false;
				IL_0277:
				if (_003CgameMsg_003E5__3 != null && _003CgameMsg_003E5__3.iType == 1)
				{
					_003C_003E2__current = new WaitForSeconds(2f);
					_003C_003E1__state = 5;
					return true;
				}
				goto IL_02ad;
				IL_024e:
				if (_003CiType_003E5__4 == 1)
				{
					_003C_003E2__current = new WaitForSeconds(4f);
					_003C_003E1__state = 4;
					return true;
				}
				goto IL_0277;
				IL_0470:
				_003CheadMsg_003E5__2 = null;
				goto IL_0061;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private Pb2TdrConverter m_Pb2tdrConverter = new Pb2TdrConverter();

	private static ReplayGameServer _replayGameServer;

	private Queue<object> m_replayActions = new Queue<object>();

	private IGlobalModel m_globalModel;

	private Vector2 m_pos = new Vector2(0f, Screen.height / 20);

	private GameReplayMode m_gameReplayMode;

	private string m_replayFileName = "xxx";

	private bool m_collapse = true;

	private int m_selfPlayerID;

	public bool ShowGUI;

	public bool HasError;

	public bool IsWildScreen;

	private float m_lastModifyTimeScale;

	private FileInfo[] directories;

	public bool IsRecording
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(31896))
			{
				return WrappersManagerImpl.GetPatch(31896).__Gen_Wrap_452(this);
			}
			if (!SingletonBase<DynamicConfig>.GetInstance().GetBool("Record", "Open"))
			{
				return false;
			}
			return m_gameReplayMode == GameReplayMode.Record;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(43358))
			{
				WrappersManagerImpl.GetPatch(43358).__Gen_Wrap_453(this, value);
			}
			else if (value)
			{
				m_gameReplayMode = GameReplayMode.Record;
			}
			else
			{
				m_gameReplayMode = GameReplayMode.Normal;
			}
		}
	}

	public bool IsReplaying
	{
		get
		{
			if (WrappersManagerImpl.IsPatched(30587))
			{
				return WrappersManagerImpl.GetPatch(30587).__Gen_Wrap_452(this);
			}
			return m_gameReplayMode == GameReplayMode.Replay;
		}
		set
		{
			if (WrappersManagerImpl.IsPatched(43359))
			{
				WrappersManagerImpl.GetPatch(43359).__Gen_Wrap_453(this, value);
			}
			else if (value)
			{
				m_gameReplayMode = GameReplayMode.Replay;
			}
			else
			{
				m_gameReplayMode = GameReplayMode.Normal;
			}
		}
	}

	public int GetSelfPlayerID()
	{
		if (WrappersManagerImpl.IsPatched(43347))
		{
			return WrappersManagerImpl.GetPatch(43347).__Gen_Wrap_2(this);
		}
		return m_selfPlayerID;
	}

	private void Awake()
	{
		if (WrappersManagerImpl.IsPatched(43348))
		{
			WrappersManagerImpl.GetPatch(43348).__Gen_Wrap_0(this);
		}
		else
		{
			_replayGameServer = this;
		}
	}

	private static void InitReplayGameServer()
	{
		if (WrappersManagerImpl.IsPatched(30586))
		{
			WrappersManagerImpl.GetPatch(30586).__Gen_Wrap_459();
		}
		else if (_replayGameServer == null)
		{
			ReplayGameServer replayGameServer = AppRoot.GetInstance().gameObject.GetComponent<ReplayGameServer>();
			if (replayGameServer == null)
			{
				replayGameServer = AppRoot.GetInstance().gameObject.AddComponent<ReplayGameServer>();
			}
			_replayGameServer = replayGameServer;
		}
	}

	public static ReplayGameServer GetInstance()
	{
		if (WrappersManagerImpl.IsPatched(30585))
		{
			return WrappersManagerImpl.GetPatch(30585).__Gen_Wrap_2737();
		}
		InitReplayGameServer();
		return _replayGameServer;
	}

	public void SetGlobalModel(IGlobalModel globalModel)
	{
		if (WrappersManagerImpl.IsPatched(31901))
		{
			WrappersManagerImpl.GetPatch(31901).__Gen_Wrap_1(this, globalModel);
		}
		else
		{
			m_globalModel = globalModel;
		}
	}

	public IGlobalModel GetGlobalModel()
	{
		if (WrappersManagerImpl.IsPatched(31898))
		{
			return WrappersManagerImpl.GetPatch(31898).__Gen_Wrap_2966(this);
		}
		return m_globalModel;
	}

	public void StartReplayGame()
	{
		if (WrappersManagerImpl.IsPatched(43349))
		{
			WrappersManagerImpl.GetPatch(43349).__Gen_Wrap_0(this);
			return;
		}
		SingletonBase<NewSceneManager>.GetInstance().DebugSet(var: false);
		MJGameBaseConfigUtil.FetchConfig(loadLocalConfig: true);
		EnterGameVO enterGameVO = new EnterGameVO();
		bool num = GetInstance().PrepareReplayData(ref enterGameVO);
		enterGameVO.enterGameSecretRoomVO = new EnterGameSecretRoomVO();
		if (!num)
		{
			DialogCallbacks dialogCallbacks = new DialogCallbacks();
			dialogCallbacks.ConfirmCallback = delegate
			{
			};
			DialogManager.GetInstance().ShowDialogUgui(1, dialogCallbacks, LanguageKey.VIDEO_FILE_DAMAGE, LanguageKey.SURE);
			return;
		}
		HappyMahjong.Common.Log.Info("StartReplayGame m_replayFileName " + m_replayFileName, "Replay");
		SingletonBase<AutoTestManager>.GetInstance().WriteToFile($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} StartReplayFile curFileName is {m_replayFileName}");
		MJDef.SHOW_SHARE_BTN = enterGameVO.roomInfo.canShare;
		m_globalModel.enterGameInfo = enterGameVO;
		NewSceneManager.LoadLevel("LoadingScene");
		GetInstance().HasError = false;
		if (enterGameVO.enterOBRoomVO.targetUin == 1)
		{
			GetInstance().IsWildScreen = true;
		}
		else
		{
			GetInstance().IsWildScreen = false;
		}
	}

	public static void SerializeObject<T>(T serializableObject, string fileName)
	{
		if (serializableObject == null)
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		XmlSerializer xmlSerializer = new XmlSerializer(serializableObject.GetType());
		using MemoryStream memoryStream = new MemoryStream();
		xmlSerializer.Serialize(memoryStream, serializableObject);
		memoryStream.Position = 0L;
		xmlDocument.Load(memoryStream);
		xmlDocument.Save(fileName);
		memoryStream.Close();
	}

	public void SerializeObjectCollection<T>(T serializableObject, string fileName, Type[] types)
	{
		if (serializableObject == null)
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		XmlSerializer xmlSerializer = new XmlSerializer(serializableObject.GetType(), types);
		using MemoryStream memoryStream = new MemoryStream();
		xmlSerializer.Serialize(memoryStream, serializableObject);
		memoryStream.Position = 0L;
		xmlDocument.Load(memoryStream);
		((XmlDeclaration)xmlDocument.FirstChild).Encoding = "utf-8";
		xmlDocument.Save(fileName);
		memoryStream.Close();
	}

	public T Deserialize<T>(string fileName, Type[] types)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), types);
		FileStream stream = new FileStream(fileName, FileMode.Open);
		T val = (T)xmlSerializer.Deserialize(stream);
		xmlSerializer.Serialize(System.Console.Out, val);
		System.Console.ReadLine();
		return val;
	}

	public static T DeSerializeObject<T>(string fileName, Type[] types)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return default(T);
		}
		T result = default(T);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		using StringReader stringReader = new StringReader(xmlDocument.OuterXml);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), types);
		using (XmlReader xmlReader = new XmlTextReader(stringReader))
		{
			result = (T)xmlSerializer.Deserialize(xmlReader);
			xmlReader.Close();
		}
		stringReader.Close();
		return result;
	}

	public static T DeSerializeObject<T>(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return default(T);
		}
		T result = default(T);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(fileName);
		using StringReader stringReader = new StringReader(xmlDocument.OuterXml);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using (XmlReader xmlReader = new XmlTextReader(stringReader))
		{
			result = (T)xmlSerializer.Deserialize(xmlReader);
			xmlReader.Close();
		}
		stringReader.Close();
		return result;
	}

	public void RecordPlayerAction(object playerAction)
	{
		if (WrappersManagerImpl.IsPatched(31899))
		{
			WrappersManagerImpl.GetPatch(31899).__Gen_Wrap_1(this, playerAction);
		}
		else
		{
			m_replayActions.Enqueue(playerAction);
		}
	}

	private List<Type> GetCollectionTypes()
	{
		if (WrappersManagerImpl.IsPatched(43352))
		{
			return WrappersManagerImpl.GetPatch(43352).__Gen_Wrap_4032(this);
		}
		List<Type> list = new List<Type>();
		list.Add(typeof(object));
		foreach (object replayAction in m_replayActions)
		{
			list.Add(replayAction.GetType());
		}
		return list;
	}

	public void SaveRecord(string recordFileName = "")
	{
		if (WrappersManagerImpl.IsPatched(43353))
		{
			WrappersManagerImpl.GetPatch(43353).__Gen_Wrap_1(this, recordFileName);
		}
		else
		{
			if (m_replayActions.Count == 0)
			{
				return;
			}
			if (string.IsNullOrEmpty(recordFileName))
			{
				string text = string.Format("{1}_record_{0}.json", DateTime.Now.ToString("yyMMddHHmmss"), SingletonBase<PlayerLoginDataMgr>.GetInstance().GetLoginDataUin());
				recordFileName = PathManager.GetPath() + "/" + text;
			}
			HappyMahjong.Common.Log.Info("Framework", "SaveRecord to file:{0}", recordFileName);
			List<ReplayData> list = new List<ReplayData>();
			foreach (object replayAction in m_replayActions)
			{
				ReplayData replayData = new ReplayData();
				replayData.type = replayAction.GetType().FullName;
				if (replayAction.GetType() == typeof(TSDKToUnity))
				{
					replayData.objectJson = ((int)replayAction).ToString();
				}
				else
				{
					replayData.objectJson = JsonMapper.ToJson(replayAction);
				}
				list.Add(replayData);
			}
			File.WriteAllText(recordFileName, JsonMapper.ToJson(list.ToArray()));
			Queue<object> queue = new Queue<object>();
			while (m_replayActions.Count > 0)
			{
				object obj = m_replayActions.Dequeue();
				if (obj.GetType() == typeof(T2UMsgUserReady) || obj.GetType() == typeof(T2UEnterRooom) || obj.GetType() == typeof(T2UMsgUserEnter) || obj.GetType() == typeof(EnterGameVO))
				{
					queue.Enqueue(obj);
				}
			}
			m_replayActions = queue;
		}
	}

	public void StartRecord()
	{
		if (WrappersManagerImpl.IsPatched(31897))
		{
			WrappersManagerImpl.GetPatch(31897).__Gen_Wrap_0(this);
		}
		else
		{
			m_replayActions = new Queue<object>();
		}
	}

	public static void Test()
	{
		if (WrappersManagerImpl.IsPatched(43354))
		{
			WrappersManagerImpl.GetPatch(43354).__Gen_Wrap_459();
		}
	}

	public static void ConvertDirRecordFromXMLToJson(string dirName)
	{
		if (WrappersManagerImpl.IsPatched(43355))
		{
			WrappersManagerImpl.GetPatch(43355).__Gen_Wrap_0(dirName);
			return;
		}
		FileInfo[] files = new DirectoryInfo(dirName).GetFiles("*record.xml", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			ConvertRecordFromXMLToJson(files[i].FullName);
		}
	}

	public static bool ConvertRecordFromXMLToJson(string recordFileName)
	{
		if (WrappersManagerImpl.IsPatched(43356))
		{
			return WrappersManagerImpl.GetPatch(43356).__Gen_Wrap_452(recordFileName);
		}
		Queue<object> queue = new Queue<object>();
		object[] array = DeSerializeObject<object[]>(recordFileName);
		if (array == null)
		{
			HappyMahjong.Common.Log.Debug("Framework", "after DeSerializeObject:{0}, null == userActionsType", recordFileName);
			return false;
		}
		List<Type> list = new List<Type>();
		object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			XmlNode[] array3 = array2[i] as XmlNode[];
			if (array3.Length == 0 || string.IsNullOrEmpty(array3[0].Value))
			{
				HappyMahjong.Common.Log.Debug("Framework", "nodes.Length:{0}, nodes.Length == 0 || string.IsNullOrEmpty(nodes[0].Value)", array3.Length);
				return false;
			}
			Type typeByString = GetTypeByString(array3[0].Value);
			if (null == typeByString)
			{
				HappyMahjong.Common.Log.Debug("Framework", "after GetTypeByString:{0}, null == genType", array3[0].Value);
				return false;
			}
			list.Add(typeByString);
		}
		object[] array4 = DeSerializeObject<object[]>(recordFileName, list.ToArray());
		if (array4 == null)
		{
			HappyMahjong.Common.Log.Debug("Framework", "after Deserialize:{0}, null == userActions, outputTypes.Count:{1}", recordFileName, list.Count);
			return false;
		}
		array2 = array4;
		foreach (object item in array2)
		{
			queue.Enqueue(item);
		}
		List<ReplayData> list2 = new List<ReplayData>();
		array2 = array4;
		foreach (object obj in array2)
		{
			queue.Enqueue(obj);
			ReplayData replayData = new ReplayData();
			replayData.type = obj.GetType().FullName;
			if (obj.GetType() == typeof(TSDKToUnity))
			{
				replayData.objectJson = ((int)obj).ToString();
			}
			else
			{
				replayData.objectJson = JsonMapper.ToJson(obj);
			}
			list2.Add(replayData);
		}
		recordFileName = recordFileName.Replace(".xml", ".json");
		File.WriteAllText(recordFileName, JsonMapper.ToJson(list2.ToArray()));
		return true;
	}

	public static bool LoadRecord(string recordFileName, Queue<object> replayActions)
	{
		if (WrappersManagerImpl.IsPatched(43351))
		{
			return WrappersManagerImpl.GetPatch(43351).__Gen_Wrap_460(recordFileName, replayActions);
		}
		HappyMahjong.Common.Log.Info("LoadRecord " + recordFileName, "Replay");
		replayActions.Clear();
		ReplayData[] array = JsonMapper.ToObject<ReplayData[]>(File.ReadAllText(recordFileName));
		foreach (ReplayData replayData in array)
		{
			Type type = ReflectionManager.GetType(replayData.type);
			object obj = null;
			obj = ((!(type == typeof(TSDKToUnity))) ? JsonMapper.ToObject(type, replayData.objectJson) : ((object)(TSDKToUnity)Convert.ToInt32(replayData.objectJson)));
			replayActions.Enqueue(obj);
		}
		return true;
	}

	public void AsyncStartGame()
	{
		if (WrappersManagerImpl.IsPatched(43360))
		{
			WrappersManagerImpl.GetPatch(43360).__Gen_Wrap_0(this);
			return;
		}
		HappyMahjong.Common.Log.Info("->LocalGameServer::AsyncStartGame", "SinglePlayer");
		AppRoot.GetInstance().StartCoroutine(StartGameRoutine());
	}

	public static Type GetTypeByString(string typeName)
	{
		if (WrappersManagerImpl.IsPatched(43357))
		{
			return WrappersManagerImpl.GetPatch(43357).__Gen_Wrap_4033(typeName);
		}
		string typeName2 = typeName;
		return (from asm in AppDomain.CurrentDomain.GetAssemblies()
			from type in asm.GetTypes()
			where (type.IsClass || type.IsEnum || type.IsValueType) && type.Name == typeName2
			select type).Single();
	}

	public void TestSerializef2()
	{
		if (WrappersManagerImpl.IsPatched(43377))
		{
			WrappersManagerImpl.GetPatch(43377).__Gen_Wrap_0(this);
			return;
		}
		StartRecord();
		EnterGameVO playerAction = new EnterGameVO();
		RecordPlayerAction(playerAction);
		HuanLeGameSo.SoNotifyMsgTV playerAction2 = new HuanLeGameSo.SoNotifyMsgTV();
		RecordPlayerAction(playerAction2);
		T2UMsgUserReady playerAction3 = new T2UMsgUserReady();
		RecordPlayerAction(playerAction3);
		TSDKToUnity tSDKToUnity = TSDKToUnity.NotifyUserReady;
		RecordPlayerAction(tSDKToUnity);
		RecordPlayerAction(default(T2UEnterRooom));
		T2UMsgUserEnter playerAction4 = new T2UMsgUserEnter();
		RecordPlayerAction(playerAction4);
		SaveRecord();
	}

	public void TestSerializef()
	{
		if (WrappersManagerImpl.IsPatched(43378))
		{
			WrappersManagerImpl.GetPatch(43378).__Gen_Wrap_0(this);
			return;
		}
		Queue<object> queue = new Queue<object>();
		HuanLeGameSo.SoNotifyMsgTV item = new HuanLeGameSo.SoNotifyMsgTV();
		queue.Enqueue(item);
		T2UMsgUserReady item2 = new T2UMsgUserReady();
		queue.Enqueue(item2);
		TSDKToUnity tSDKToUnity = TSDKToUnity.NotifyUserReady;
		queue.Enqueue(tSDKToUnity);
		queue.Enqueue(default(T2UEnterRooom));
		new List<object>().AddRange(queue);
		Type[] types = new Type[5]
		{
			typeof(object),
			typeof(HuanLeGameSo.SoNotifyMsgTV),
			typeof(T2UMsgUserReady),
			typeof(TSDKToUnity),
			typeof(T2UEnterRooom)
		};
		SerializeObjectCollection(queue.ToArray(), Application.persistentDataPath + "/tempList2.xml", types);
	}

	public bool PrepareReplayData(ref EnterGameVO enterGameVO)
	{
		if (WrappersManagerImpl.IsPatched(43350))
		{
			return WrappersManagerImpl.GetPatch(43350).__Gen_Wrap_4031(this, ref enterGameVO);
		}
		bool flag = false;
		m_replayActions = new Queue<object>();
		if (!((!string.IsNullOrEmpty(m_replayFileName)) ? LoadRecord(m_replayFileName, m_replayActions) : LoadRecord("c:\\playeractions.xml", m_replayActions)))
		{
			return false;
		}
		if (m_replayActions.Count == 0)
		{
			return false;
		}
		object obj = m_replayActions.Dequeue();
		if (obj.GetType() == typeof(EnterGameVO))
		{
			enterGameVO = (EnterGameVO)obj;
			_ = enterGameVO.gameType;
			_ = 5;
			if (enterGameVO.gameType == GameType.GuangDong)
			{
				enterGameVO.gameType = GameType.ModuleGuangDong;
			}
			else if (enterGameVO.gameType == GameType.ModuleErRen)
			{
				enterGameVO.roomInfo.sceneId = 112398;
			}
			else if (enterGameVO.gameType == GameType.ModuleKaWuXing)
			{
				enterGameVO.roomInfo.sceneId = 351398;
			}
			if (enterGameVO.gameType == GameType.ModuleGuangDong)
			{
				enterGameVO.roomInfo.sceneId = 252398;
			}
			return true;
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CStartGameRoutine_003Ed__43))]
	public IEnumerator StartGameRoutine()
	{
		if (WrappersManagerImpl.IsPatched(43361))
		{
			return WrappersManagerImpl.GetPatch(43361).__Gen_Wrap_458(this);
		}
		return new _003CStartGameRoutine_003Ed__43(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CAsynRunMsg_003Ed__44))]
	public IEnumerator AsynRunMsg()
	{
		if (WrappersManagerImpl.IsPatched(43365))
		{
			return WrappersManagerImpl.GetPatch(43365).__Gen_Wrap_458(this);
		}
		return new _003CAsynRunMsg_003Ed__44(0)
		{
			_003C_003E4__this = this
		};
	}

	private void UpdateTimeScale(bool forward = true)
	{
		if (WrappersManagerImpl.IsPatched(43379))
		{
			WrappersManagerImpl.GetPatch(43379).__Gen_Wrap_453(this, forward);
		}
		else if (Time.realtimeSinceStartup - m_lastModifyTimeScale > 2f)
		{
			if (forward)
			{
				Time.timeScale *= 2f;
			}
			else
			{
				Time.timeScale /= 2f;
			}
			if (Time.timeScale > 16f)
			{
				Time.timeScale = 1f;
			}
			m_lastModifyTimeScale = Time.realtimeSinceStartup;
		}
	}

	public string GetCurReplayFileName()
	{
		if (WrappersManagerImpl.IsPatched(43380))
		{
			return WrappersManagerImpl.GetPatch(43380).__Gen_Wrap_47(this);
		}
		return m_replayFileName;
	}

	[IDTag(0)]
	public void ReplayFile(string path)
	{
		if (WrappersManagerImpl.IsPatched(43381))
		{
			WrappersManagerImpl.GetPatch(43381).__Gen_Wrap_1(this, path);
			return;
		}
		IsReplaying = true;
		m_replayFileName = path;
		GetInstance().StartReplayGame();
	}

	public string GetTestCasesRecordDirName()
	{
		if (WrappersManagerImpl.IsPatched(43382))
		{
			return WrappersManagerImpl.GetPatch(43382).__Gen_Wrap_47(this);
		}
		return PathManager.GetPath();
	}

	[IDTag(1)]
	public bool ReplayFile(int index)
	{
		if (WrappersManagerImpl.IsPatched(43383))
		{
			return WrappersManagerImpl.GetPatch(43383).__Gen_Wrap_1659(this, index);
		}
		if (directories == null)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GetInstance().GetTestCasesRecordDirName());
			directories = directoryInfo.GetFiles("*record.json", SearchOption.AllDirectories);
		}
		if (directories.Length > index)
		{
			IsReplaying = true;
			m_replayFileName = directories[index].FullName;
			GetInstance().StartReplayGame();
			return true;
		}
		HappyMahjong.Common.Log.Info("THE END", "RecordnReplay");
		return false;
	}

	public int ReplayAllFileFromPath(string name)
	{
		if (WrappersManagerImpl.IsPatched(43384))
		{
			return WrappersManagerImpl.GetPatch(43384).__Gen_Wrap_1557(this, name);
		}
		if (directories == null)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GetInstance().GetTestCasesRecordDirName());
			directories = directoryInfo.GetFiles("*record.json", SearchOption.AllDirectories);
		}
		for (int i = 0; i < directories.Length; i++)
		{
			if (name == directories[i].FullName)
			{
				IsReplaying = true;
				m_replayFileName = directories[i].FullName;
				GetInstance().StartReplayGame();
				return i;
			}
		}
		return 0;
	}

	public void DirectoryCopy(string sourceDirName, string destDirName)
	{
		if (WrappersManagerImpl.IsPatched(43385))
		{
			WrappersManagerImpl.GetPatch(43385).__Gen_Wrap_12(this, sourceDirName, destDirName);
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] array = directoryInfo.GetDirectories();
		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}
		FileInfo[] files = directoryInfo.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			if (!(fileInfo.Name == "cutImage.png"))
			{
				string text = Path.Combine(destDirName, fileInfo.Name);
				if (!File.Exists(text))
				{
					fileInfo.CopyTo(text, overwrite: false);
				}
			}
		}
		DirectoryInfo[] array2 = array;
		foreach (DirectoryInfo directoryInfo2 in array2)
		{
			string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
			DirectoryCopy(directoryInfo2.FullName, destDirName2);
		}
	}
}
