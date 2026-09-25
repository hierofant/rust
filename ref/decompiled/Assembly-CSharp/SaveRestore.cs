#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Facepunch.Rust;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.Assertions;

public class SaveRestore : SingletonComponent<SaveRestore>
{
	[JsonModel]
	public class SaveExtraData
	{
		public string WipeId;
	}

	[CompilerGenerated]
	private sealed class _003CDoAutomatedSave_003Ed__30 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool AndWait;

		public SaveRestore _003C_003E4__this;

		private string _003Cfolder_003E5__2;

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
		public _003CDoAutomatedSave_003Ed__30(int _003C_003E1__state)
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
			SaveRestore saveRestore = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				IsSaving = true;
				_003Cfolder_003E5__2 = ConVar.Server.rootFolder;
				if (!AndWait)
				{
					_003C_003E2__current = CoroutineEx.waitForEndOfFrame;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0061;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0061;
			case 2:
				_003C_003E1__state = -1;
				goto IL_00d0;
			case 3:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_0061:
				if (AndWait)
				{
					IEnumerator enumerator = Save(_003Cfolder_003E5__2 + "/" + World.SaveFileName, AndWait);
					while (enumerator.MoveNext())
					{
					}
					goto IL_00d0;
				}
				_003C_003E2__current = saveRestore.StartCoroutine(Save(_003Cfolder_003E5__2 + "/" + World.SaveFileName, AndWait));
				_003C_003E1__state = 2;
				return true;
				IL_00d0:
				if (!AndWait)
				{
					_003C_003E2__current = CoroutineEx.waitForEndOfFrame;
					_003C_003E1__state = 3;
					return true;
				}
				break;
			}
			UnityEngine.Debug.Log("Saving complete");
			IsSaving = false;
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

	public static bool IsSaving = false;

	public static DateTime SaveCreatedTime;

	private static RealTimeSince TimeSinceLastSave;

	private static MemoryStream SaveBuffer = new MemoryStream(33554432);

	private static Action<Stream> onSaveComplete;

	private static object callbackLock = new object();

	private static Queue<Stream> saveQueue = new Queue<Stream>();

	private static object saveQueueLock = new object();

	public static string WipeId { get; private set; }

	public static List<BaseEntity> FindMapEntities()
	{
		List<BaseEntity> list = new List<BaseEntity>(UnityEngine.Object.FindObjectsByType<BaseEntity>(FindObjectsSortMode.None));
		SceneToPrefabSpawner[] array = UnityEngine.Object.FindObjectsByType<SceneToPrefabSpawner>(FindObjectsSortMode.None);
		foreach (SceneToPrefabSpawner sceneToPrefabSpawner in array)
		{
			if (sceneToPrefabSpawner.Entities.Count > 0)
			{
				list.AddRange(sceneToPrefabSpawner.Entities);
				continue;
			}
			foreach (SceneToPrefabSpawner.EntitySpawnInfo item in sceneToPrefabSpawner.EntitiesToSpawn)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(item.PrefabName, sceneToPrefabSpawner.transform.TransformPoint(item.Position), sceneToPrefabSpawner.transform.rotation * item.Rotation);
				if (baseEntity is ApartmentRoom apartmentRoom)
				{
					apartmentRoom.RoomNumber = item.ApartmentRoomNumber;
				}
				list.Add(baseEntity);
				sceneToPrefabSpawner.Entities.Add(baseEntity);
			}
		}
		return list;
	}

	public static void ClearMapEntities(List<BaseEntity> entities)
	{
		int count = entities.Count;
		DebugEx.Log("Destroying " + count + " old entities");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int num = count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = entities[num];
			if (baseEntity.enableSaving || !(baseEntity.GetComponent<DisableSave>() != null))
			{
				baseEntity.KillAsMapEntity();
				if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
				{
					stopwatch.Reset();
					stopwatch.Start();
					DebugEx.Log("\t" + (count - num) + " / " + count);
				}
				entities.RemoveAt(num);
			}
		}
		ItemManager.Heartbeat();
		DebugEx.Log("\tdone.");
	}

	public static void SpawnMapEntities(List<BaseEntity> entities)
	{
		DebugEx.Log("Spawning " + entities.Count + " entities from map");
		foreach (BaseEntity entity in entities)
		{
			if (!(entity == null))
			{
				entity.SpawnAsMapEntity();
			}
		}
		DebugEx.Log("\tdone.");
		DebugEx.Log("Postprocessing " + entities.Count + " entities from map");
		foreach (BaseEntity entity2 in entities)
		{
			if (!(entity2 == null))
			{
				entity2.PostMapEntitySpawn();
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static bool Load(string strFilename = "", bool allowOutOfDateSaves = false)
	{
		SaveCreatedTime = DateTime.UtcNow;
		try
		{
			if (strFilename == "")
			{
				strFilename = World.SaveFolderName + "/" + World.SaveFileName;
			}
			if (!File.Exists(strFilename))
			{
				Interface.CallHook("OnNewSave", strFilename);
				if (!File.Exists("TestSaves/" + strFilename))
				{
					UnityEngine.Debug.LogWarning("Couldn't load " + strFilename + " - file doesn't exist");
					return false;
				}
				strFilename = "TestSaves/" + strFilename;
			}
			List<BaseEntity> list = FindMapEntities();
			Dictionary<BaseEntity, ProtoBuf.Entity> dictionary = new Dictionary<BaseEntity, ProtoBuf.Entity>();
			using (FileStream fileStream = File.OpenRead(strFilename))
			{
				using BinaryReader binaryReader = new BinaryReader(fileStream);
				SaveCreatedTime = File.GetCreationTime(strFilename);
				if (binaryReader.ReadSByte() != 83 || binaryReader.ReadSByte() != 65 || binaryReader.ReadSByte() != 86 || binaryReader.ReadSByte() != 82)
				{
					UnityEngine.Debug.LogWarning("Invalid save (missing header)");
					return false;
				}
				if (binaryReader.PeekChar() == 74)
				{
					binaryReader.ReadChar();
					WipeId = JsonConvert.DeserializeObject<SaveExtraData>(binaryReader.ReadString()).WipeId;
				}
				if (binaryReader.PeekChar() == 68)
				{
					binaryReader.ReadChar();
					SaveCreatedTime = Epoch.ToDateTime(binaryReader.ReadInt32());
				}
				if (binaryReader.ReadUInt32() != 288)
				{
					if (allowOutOfDateSaves)
					{
						UnityEngine.Debug.LogWarning("This save is from an older (possibly incompatible) version!");
					}
					else
					{
						UnityEngine.Debug.LogWarning("This save is from an older version. It might not load properly.");
					}
				}
				ClearMapEntities(list);
				Assert.IsTrue(BaseEntity.saveList.Count == 0, "BaseEntity.saveList isn't empty!");
				Network.Net.sv.Reset();
				Rust.Application.isLoadingSave = true;
				HashSet<NetworkableId> hashSet = new HashSet<NetworkableId>();
				using PooledList<ulong> pooledList = Facepunch.Pool.Get<PooledList<ulong>>();
				while (fileStream.Position < fileStream.Length)
				{
					RCon.Update();
					uint num = binaryReader.ReadUInt32();
					long position = fileStream.Position;
					ProtoBuf.Entity entData = Facepunch.Pool.Get<ProtoBuf.Entity>();
					try
					{
						entData.ReadFromStream(fileStream, (int)num);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogWarning("Skipping entity since it could not be deserialized - stream position: " + position + " size: " + num);
						UnityEngine.Debug.LogException(exception);
						fileStream.Position = position + num;
						entData.Dispose();
						continue;
					}
					if (entData.baseNetworkable.uid.IsValid && hashSet.Contains(entData.baseNetworkable.uid))
					{
						string[] obj = new string[5] { "Skipping entity ", null, null, null, null };
						NetworkableId uid = entData.baseNetworkable.uid;
						obj[1] = uid.ToString();
						obj[2] = " ";
						obj[3] = StringPool.Get(entData.baseNetworkable.prefabID);
						obj[4] = " - uid is used multiple times";
						UnityEngine.Debug.LogWarning(string.Concat(obj));
						entData.Dispose();
						continue;
					}
					if (entData.basePlayer != null)
					{
						if (dictionary.Any((KeyValuePair<BaseEntity, ProtoBuf.Entity> x) => x.Value.basePlayer != null && x.Value.basePlayer.userid == entData.basePlayer.userid))
						{
							UnityEngine.Debug.LogWarning($"Skipping entity {entData.baseNetworkable.uid} - it's a player {entData.basePlayer.userid} who is in the save multiple times");
							entData.Dispose();
							continue;
						}
						if (BasePlayer.IsBotId(entData.basePlayer.userid))
						{
							pooledList.Add(entData.basePlayer.userid);
						}
					}
					if (entData.baseNetworkable.uid.IsValid)
					{
						hashSet.Add(entData.baseNetworkable.uid);
					}
					BaseEntity baseEntity = GameManager.server.CreateEntity(StringPool.Get(entData.baseNetworkable.prefabID), entData.baseEntity.pos, Quaternion.Euler(entData.baseEntity.rot));
					if ((bool)baseEntity)
					{
						baseEntity.InitLoad(entData.baseNetworkable.uid);
						baseEntity.PreServerLoad();
						dictionary.Add(baseEntity, entData);
					}
				}
				BasePlayer.ReserveBotIds(pooledList);
			}
			DebugEx.Log("Spawning " + list.Count + " entities from map");
			foreach (BaseEntity item in list)
			{
				if (!(item == null))
				{
					item.SpawnAsMapEntity();
				}
			}
			DebugEx.Log("\tdone.");
			DebugEx.Log("Spawning " + dictionary.Count + " entities from save");
			object obj2 = Interface.CallHook("OnSaveLoad", dictionary);
			if (obj2 is bool)
			{
				return (bool)obj2;
			}
			BaseNetworkable.LoadInfo info = default(BaseNetworkable.LoadInfo);
			info.fromDisk = true;
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num2 = 0;
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item2 in dictionary)
			{
				BaseEntity key = item2.Key;
				if (key == null)
				{
					continue;
				}
				RCon.Update();
				info.msg = item2.Value;
				key.Spawn();
				key.Load(info);
				if (key.IsValid())
				{
					num2++;
					if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
					{
						stopwatch.Reset();
						stopwatch.Start();
						DebugEx.Log("\t" + num2 + " / " + dictionary.Count);
					}
				}
			}
			DebugEx.Log("\tdone.");
			DebugEx.Log("Postprocessing " + list.Count + " entities from map");
			foreach (BaseEntity item3 in list)
			{
				if (!(item3 == null))
				{
					item3.PostMapEntitySpawn();
				}
			}
			DebugEx.Log("\tdone.");
			DebugEx.Log("Postprocessing " + list.Count + " entities from save");
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item4 in dictionary)
			{
				BaseEntity key2 = item4.Key;
				if (!(key2 == null))
				{
					RCon.Update();
					if (key2.IsValid())
					{
						key2.UpdateNetworkGroup();
						key2.PostServerLoad();
					}
				}
			}
			DebugEx.Log("\tdone.");
			if (!AI.useUnityNavmesh && !AiManager.nav_disable && (bool)RustNavigation.Instance && RustNavigation.Instance.enabled)
			{
				RustNavigation.Instance.Load(Path.ChangeExtension(strFilename, ".navmesh"));
			}
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item5 in dictionary)
			{
				_ = item5.Value;
				item5.Value.Dispose();
			}
			dictionary.Clear();
			if ((bool)SingletonComponent<SpawnHandler>.Instance)
			{
				DebugEx.Log("Enforcing SpawnPopulation Limits");
				SingletonComponent<SpawnHandler>.Instance.EnforceLimits();
				DebugEx.Log("\tdone.");
			}
			InitializeWipeId();
			Rust.Application.isLoadingSave = false;
			return true;
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogWarning("Error loading save (" + strFilename + ")");
			UnityEngine.Debug.LogException(exception2);
			return false;
		}
	}

	public static void GetSaveCache()
	{
		BaseEntity[] array = BaseEntity.saveList.ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " entity save caches");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			BaseEntity baseEntity = array[i];
			if (baseEntity.IsValid())
			{
				baseEntity.GetSaveCache();
				if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
				{
					stopwatch.Reset();
					stopwatch.Start();
					DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
				}
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeEntityLinks()
	{
		BaseEntity[] array = (from x in BaseNetworkable.serverEntities
			where x is BaseEntity
			select x as BaseEntity).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " entity links");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			RCon.Update();
			array[i].RefreshEntityLinks();
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeEntitySupports()
	{
		if (!ConVar.Server.stability)
		{
			return;
		}
		StabilityEntity[] array = (from x in BaseNetworkable.serverEntities
			where x is StabilityEntity
			select x as StabilityEntity).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " stability supports");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			RCon.Update();
			array[i].InitializeSupports();
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeEntityConditionals()
	{
		BuildingBlock[] array = (from x in BaseNetworkable.serverEntities
			where x is BuildingBlock
			select x as BuildingBlock).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		DebugEx.Log("Initializing " + array.Length + " conditional models");
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < array.Length; i++)
		{
			RCon.Update();
			array[i].UpdateSkin(force: true);
			if (stopwatch.Elapsed.TotalMilliseconds > 2000.0)
			{
				stopwatch.Reset();
				stopwatch.Start();
				DebugEx.Log("\t" + (i + 1) + " / " + array.Length);
			}
		}
		DebugEx.Log("\tdone.");
	}

	public static void InitializeWipeId()
	{
		if (WipeId == null)
		{
			WipeId = Guid.NewGuid().ToString("N");
		}
	}

	public static void AddOnSaveCallback(Action<Stream> callback)
	{
		lock (callbackLock)
		{
			onSaveComplete = (Action<Stream>)Delegate.Combine(onSaveComplete, callback);
		}
	}

	public static void RemoveOnSaveCallback(Action<Stream> callback)
	{
		lock (callbackLock)
		{
			onSaveComplete = (Action<Stream>)Delegate.Remove(onSaveComplete, callback);
		}
	}

	public static IEnumerator Save(string strFilename, bool AndWait = false)
	{
		if (Rust.Application.isQuitting)
		{
			yield break;
		}
		Stopwatch timerCache = new Stopwatch();
		Stopwatch timerWrite = new Stopwatch();
		Stopwatch timerDisk = new Stopwatch();
		SaveBuffer.Position = 0L;
		SaveBuffer.SetLength(0L);
		InitializeWipeId();
		EventRecord eventRecord = EventRecord.New("save");
		eventRecord.AddField("name", strFilename);
		eventRecord.Submit();
		if (AndWait)
		{
			IEnumerator enumerator = WarmUpEntityCaches(AndWait, timerCache);
			while (enumerator.MoveNext())
			{
			}
		}
		else
		{
			yield return SingletonComponent<SaveRestore>.Instance.StartCoroutine(WarmUpEntityCaches(AndWait, timerCache));
		}
		timerWrite.Start();
		int iEnts = 0;
		using (TimeWarning.New("SaveWrite", 100))
		{
			BinaryWriter writer = new BinaryWriter(SaveBuffer);
			WriteHeader(writer);
			if (!AndWait)
			{
				yield return CoroutineEx.waitForEndOfFrame;
			}
			iEnts = WriteEntities(writer);
		}
		timerWrite.Stop();
		if (!AI.useUnityNavmesh && !AiManager.nav_disable && (bool)RustNavigation.Instance && RustNavigation.Instance.enabled)
		{
			RustNavigation.Instance.Save(Path.ChangeExtension(strFilename, ".navmesh"));
		}
		if (!AndWait)
		{
			yield return CoroutineEx.waitForEndOfFrame;
		}
		timerDisk.Start();
		using (TimeWarning.New("SaveBackup", 100))
		{
			ShiftSaveBackups(strFilename);
		}
		using (TimeWarning.New("SaveDisk", 100))
		{
			try
			{
				string text = strFilename + ".new";
				if (File.Exists(text))
				{
					File.Delete(text);
				}
				try
				{
					using FileStream destination = File.OpenWrite(text);
					SaveBuffer.Position = 0L;
					SaveBuffer.CopyTo(destination);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Couldn't write save file! We got an exception: " + ex);
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					yield break;
				}
				File.Copy(text, strFilename, overwrite: true);
				File.Delete(text);
			}
			catch (Exception ex2)
			{
				UnityEngine.Debug.LogError("Error when saving to disk: " + ex2);
				yield break;
			}
		}
		timerDisk.Stop();
		UnityEngine.Debug.LogFormat("Saved {0} ents, cache({1}), write({2}), disk({3}).", iEnts.ToString("N0"), timerCache.Elapsed.TotalSeconds.ToString("0.00"), timerWrite.Elapsed.TotalSeconds.ToString("0.00"), timerDisk.Elapsed.TotalSeconds.ToString("0.00"));
		PerformanceLogging.server?.SetTiming("save.cache", timerCache.Elapsed);
		PerformanceLogging.server?.SetTiming("save.write", timerWrite.Elapsed);
		PerformanceLogging.server?.SetTiming("save.disk", timerDisk.Elapsed);
		NexusServer.PostGameSaved();
	}

	private static IEnumerator SaveToStream(Stream stream)
	{
		InitializeWipeId();
		yield return SingletonComponent<SaveRestore>.Instance.StartCoroutine(WarmUpEntityCaches());
		using (TimeWarning.New("SaveWrite", 100))
		{
			BinaryWriter writer = new BinaryWriter(stream);
			WriteHeader(writer);
			yield return CoroutineEx.waitForEndOfFrame;
			WriteEntities(writer);
		}
		Action<Stream> action;
		lock (callbackLock)
		{
			action = onSaveComplete;
		}
		action(stream);
	}

	private static void ShiftSaveBackups(string fileName)
	{
		int num = Mathf.Max(ConVar.Server.saveBackupCount, 2);
		if (!File.Exists(fileName))
		{
			return;
		}
		try
		{
			int num2 = 0;
			for (int j = 1; j <= num; j++)
			{
				if (!File.Exists(fileName + "." + j))
				{
					break;
				}
				num2++;
			}
			string text = GetBackupName(num2 + 1);
			for (int num3 = num2; num3 > 0; num3--)
			{
				string text2 = GetBackupName(num3);
				if (num3 == num)
				{
					File.Delete(text2);
				}
				else if (File.Exists(text2))
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
					File.Move(text2, text);
				}
				text = text2;
			}
			File.Copy(fileName, text, overwrite: true);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Error while backing up old saves: " + ex.Message);
			UnityEngine.Debug.LogException(ex);
			throw;
		}
		string GetBackupName(int i)
		{
			return $"{fileName}.{i}";
		}
	}

	private void Start()
	{
		StartCoroutine(SaveRegularly());
		StartCoroutine(ProcessStreamRequests());
	}

	private IEnumerator SaveRegularly()
	{
		while (true)
		{
			yield return CoroutineEx.waitForSeconds(1f);
			if ((float)TimeSinceLastSave >= (float)ConVar.Server.saveinterval || NexusServer.NeedsJournalFlush || NexusServer.NeedTransferFlush)
			{
				yield return StartCoroutine(DoAutomatedSave());
				TimeSinceLastSave = 0f;
			}
		}
	}

	private IEnumerator ProcessStreamRequests()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			bool flag = false;
			Stream result = null;
			lock (saveQueueLock)
			{
				flag = saveQueue.TryDequeue(out result);
			}
			if (flag)
			{
				StartCoroutine(SaveToStream(result));
			}
		}
	}

	[IteratorStateMachine(typeof(_003CDoAutomatedSave_003Ed__30))]
	private IEnumerator DoAutomatedSave(bool AndWait = false)
	{
		Interface.CallHook("OnServerSave");
		return new _003CDoAutomatedSave_003Ed__30(0)
		{
			_003C_003E4__this = this,
			AndWait = AndWait
		};
	}

	public static bool Save(bool AndWait)
	{
		if (SingletonComponent<SaveRestore>.Instance == null)
		{
			return false;
		}
		if (IsSaving)
		{
			return false;
		}
		IEnumerator enumerator = SingletonComponent<SaveRestore>.Instance.DoAutomatedSave(AndWait: true);
		while (enumerator.MoveNext())
		{
		}
		return true;
	}

	public static void RequestSave(Stream stream)
	{
		lock (saveQueueLock)
		{
			saveQueue.Enqueue(stream);
		}
	}

	private static IEnumerator WarmUpEntityCaches(bool isBlocking = false, Stopwatch cacheTimer = null)
	{
		cacheTimer?.Start();
		using (TimeWarning.New("SaveCache", 100))
		{
			Stopwatch sw = Stopwatch.StartNew();
			BaseEntity[] array = BaseEntity.saveList.ToArray();
			foreach (BaseEntity baseEntity in array)
			{
				if (baseEntity == null || !baseEntity.IsValid())
				{
					continue;
				}
				try
				{
					baseEntity.GetSaveCache();
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				if (sw.Elapsed.TotalMilliseconds > (double)ConVar.Server.saveframebudget)
				{
					if (!isBlocking)
					{
						yield return CoroutineEx.waitForEndOfFrame;
					}
					sw.Reset();
					sw.Start();
				}
			}
		}
		cacheTimer?.Stop();
	}

	private static void WriteHeader(BinaryWriter writer)
	{
		writer.Write((sbyte)83);
		writer.Write((sbyte)65);
		writer.Write((sbyte)86);
		writer.Write((sbyte)82);
		SaveExtraData saveExtraData = new SaveExtraData();
		saveExtraData.WipeId = WipeId;
		writer.Write((sbyte)74);
		writer.Write(JsonConvert.SerializeObject(saveExtraData));
		writer.Write((sbyte)68);
		writer.Write(Epoch.FromDateTime(SaveCreatedTime));
		writer.Write(288u);
	}

	private static int WriteEntities(BinaryWriter writer)
	{
		int num = 0;
		foreach (BaseEntity save in BaseEntity.saveList)
		{
			if (save == null || save.IsDestroyed)
			{
				UnityEngine.Debug.LogWarning("Entity is NULL but is still in saveList - not destroyed properly? " + save, save);
				continue;
			}
			MemoryStream memoryStream = null;
			try
			{
				memoryStream = save.GetSaveCache();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			if (memoryStream == null || memoryStream.Length <= 0)
			{
				UnityEngine.Debug.LogWarningFormat("Skipping saving entity {0} - because {1}", save, (memoryStream == null) ? "savecache is null" : "savecache is 0");
			}
			else
			{
				writer.Write((uint)memoryStream.Length);
				writer.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
				num++;
			}
		}
		return num;
	}
}
