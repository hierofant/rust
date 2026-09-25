#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust.Profiling;
using Network;
using Network.Relay;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Registry;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BaseNetworkable : BaseMonoBehaviour, IPrefabPostProcess, IEntity, NetworkHandler
{
	public struct ThreadSafeTime
	{
		public DateTime Now;

		public int FrameCount;

		public float Time;

		public float FixedTime;

		public float RealTimeSinceStartup;

		public static ThreadSafeTime TakeSnapshot()
		{
			ThreadSafeTime result = default(ThreadSafeTime);
			result.Now = DateTime.Now;
			result.FrameCount = UnityEngine.Time.frameCount;
			result.Time = UnityEngine.Time.time;
			result.FixedTime = UnityEngine.Time.fixedTime;
			result.RealTimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			return result;
		}
	}

	public struct SaveInfo
	{
		public ProtoBuf.Entity msg;

		public bool forDisk;

		public bool forTransfer;

		public Connection forConnection;

		public ThreadSafeTime cachedTime;

		internal bool SendingTo(Connection ownerConnection)
		{
			if (ownerConnection == null)
			{
				return false;
			}
			if (forConnection == null)
			{
				return false;
			}
			return forConnection == ownerConnection;
		}
	}

	public struct LoadInfo
	{
		public ProtoBuf.Entity msg;

		public bool fromDisk;

		public bool fromCopy;

		public bool fromTransfer;
	}

	public class EntityRealmServer : EntityRealm
	{
		protected override Manager visibilityManager
		{
			get
			{
				if (Network.Net.sv == null)
				{
					return null;
				}
				return Network.Net.sv.visibility;
			}
		}
	}

	public abstract class EntityRealm : IEnumerable<BaseNetworkable>, IEnumerable
	{
		public HiddenValue<ListDictionary<NetworkableId, BaseNetworkable>> entityList;

		public int Count => entityList.Get().Count;

		protected abstract Manager visibilityManager { get; }

		public EntityRealm()
		{
			entityList = new HiddenValue<ListDictionary<NetworkableId, BaseNetworkable>>(new ListDictionary<NetworkableId, BaseNetworkable>());
		}

		public bool Contains(NetworkableId uid)
		{
			return entityList.Get().Contains(uid);
		}

		public BaseNetworkable Find(NetworkableId uid)
		{
			using (TimeWarning.New("BaseNetworkable.Find"))
			{
				BaseNetworkable val = null;
				if (!entityList.Get().TryGetValue(uid, out val))
				{
					return null;
				}
				return val;
			}
		}

		public bool TryGetEntity(NetworkableId uid, out BaseEntity entity)
		{
			using (TimeWarning.New("BaseNetworkable.TryGetEntity"))
			{
				entity = null;
				if (!(Find(uid) is BaseEntity baseEntity))
				{
					return false;
				}
				entity = baseEntity;
				return true;
			}
		}

		public bool TryGetEntity<T>(NetworkableId uid, out T entity) where T : BaseEntity
		{
			using (TimeWarning.New("BaseNetworkable.TryGetEntity<T>"))
			{
				entity = null;
				if (!(Find(uid) is T val))
				{
					return false;
				}
				entity = val;
				return true;
			}
		}

		public void RegisterID(BaseNetworkable ent)
		{
			if (ent.net != null)
			{
				ListDictionary<NetworkableId, BaseNetworkable> listDictionary = entityList.Get();
				if (listDictionary.Contains(ent.net.ID))
				{
					listDictionary[ent.net.ID] = ent;
				}
				else
				{
					listDictionary.Add(ent.net.ID, ent);
				}
			}
		}

		public void UnregisterID(BaseNetworkable ent)
		{
			if (ent.net != null)
			{
				entityList.Get().Remove(ent.net.ID);
			}
		}

		public Group FindGroup(uint uid)
		{
			return visibilityManager?.Get(uid);
		}

		public Group TryFindGroup(uint uid)
		{
			return visibilityManager?.TryGet(uid);
		}

		public void FindInGroup(uint uid, List<BaseNetworkable> list)
		{
			Group group = TryFindGroup(uid);
			if (group == null || CollectionEx.IsNullOrEmpty(group.networkables))
			{
				return;
			}
			int count = group.networkables.Values.Count;
			Networkable[] buffer = group.networkables.Values.Buffer;
			for (int i = 0; i < count; i++)
			{
				Networkable networkable = buffer[i];
				BaseNetworkable baseNetworkable = Find(networkable.ID);
				if (!(baseNetworkable == null) && baseNetworkable.net != null && baseNetworkable.net.group != null)
				{
					if (baseNetworkable.net.group.ID != uid)
					{
						Debug.LogWarning("Group ID mismatch: " + baseNetworkable.ToString());
					}
					else
					{
						list.Add(baseNetworkable);
					}
				}
			}
		}

		public BufferList<BaseNetworkable>.Enumerator GetEnumerator()
		{
			return entityList.Get().Values.GetEnumerator();
		}

		IEnumerator<BaseNetworkable> IEnumerable<BaseNetworkable>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual void Clear()
		{
			entityList.Get().Clear();
		}
	}

	public enum DestroyMode : byte
	{
		None,
		Gib
	}

	public List<Component> postNetworkUpdateComponents = new List<Component>();

	public bool _limitedNetworking;

	[NonSerialized]
	public EntityRef parentEntity;

	[NonSerialized]
	public readonly List<BaseEntity> children = new List<BaseEntity>();

	[NonSerialized]
	public bool canTriggerParent = true;

	public int creationFrame;

	public bool isSpawned;

	public MemoryStream _NetworkCache;

	public static ConcurrentQueue<MemoryStream> EntityMemoryStreamPool = new ConcurrentQueue<MemoryStream>();

	private MemoryStream _SaveCache;

	private ServerOcclusion.Group occlusionGroup;

	private ListHashSet<BaseNetworkable> occlusionGroupRefs;

	private const bool UsePlayerOnlyOnMediumLayerShortcut = true;

	[ReadOnly]
	[Header("BaseNetworkable")]
	public uint prefabID;

	[Tooltip("If enabled the entity will send to everyone on the server - regardless of position")]
	public bool globalBroadcast;

	[Tooltip("What region of the server should the entity globally network to")]
	public GlobalNetworkBehavior globalNetworkBehavior;

	[Tooltip("Global broadcast a cut down version of the entity to show buildings across the map")]
	public bool globalBuildingBlock;

	[Tooltip("How far away this entity should network to clients")]
	public EntityNetworkRange networkRange = EntityNetworkRange.Medium;

	[NonSerialized]
	public Networkable net;

	[NonSerialized]
	private BaseEntity _prefab;

	private string _prefabName;

	private string _prefabNameWithoutExtension;

	private TransformHandle _transformHandle;

	public static EntityRealm serverEntities = new EntityRealmServer();

	private const bool isServersideEntity = true;

	public static List<Connection> connectionsInSphereList = new List<Connection>();

	public bool limitNetworking
	{
		get
		{
			return _limitedNetworking;
		}
		set
		{
			if (value != _limitedNetworking)
			{
				_limitedNetworking = value;
				if (_limitedNetworking)
				{
					OnNetworkLimitStart();
				}
				else
				{
					OnNetworkLimitEnd();
				}
				UpdateNetworkGroup();
			}
		}
	}

	public int ChildCount => children.Count;

	public GameManager gameManager
	{
		get
		{
			if (isServer)
			{
				return GameManager.server;
			}
			throw new NotImplementedException("Missing gameManager path");
		}
	}

	public PrefabAttribute.Library prefabAttribute
	{
		get
		{
			if (isServer)
			{
				return PrefabAttribute.server;
			}
			throw new NotImplementedException("Missing prefabAttribute path");
		}
	}

	public static Group GlobalNetworkGroup => Network.Net.sv.visibility.Get(0u);

	public static Group LimboNetworkGroup => Network.Net.sv.visibility.Get(1u);

	public static Group MainIslandGroup => Network.Net.sv.visibility.Get(2u);

	public static Group DeepSeaGroup => Network.Net.sv.visibility.Get(3u);

	public bool HasNetworkCache => _NetworkCache != null;

	public ServerOcclusion.Group OcclusionGroup => occlusionGroup;

	public ListHashSet<BaseNetworkable> OcclusionGroupRefs => occlusionGroupRefs;

	public bool IsDestroyed { get; private set; }

	public string PrefabName
	{
		get
		{
			if (_prefabName == null)
			{
				_prefabName = StringPool.Get(prefabID);
			}
			return _prefabName;
		}
	}

	public string ShortPrefabName
	{
		get
		{
			if (_prefabNameWithoutExtension == null)
			{
				_prefabNameWithoutExtension = Path.GetFileNameWithoutExtension(PrefabName);
			}
			return _prefabNameWithoutExtension;
		}
	}

	public TransformHandle TransformHandle => _transformHandle;

	public static bool UseParallelSaves => ConVar.Server.UsePlayerUpdateJobs >= 4;

	public bool isServer => true;

	public bool isClient => false;

	public void BroadcastOnPostNetworkUpdate(BaseEntity entity)
	{
		foreach (Component postNetworkUpdateComponent in postNetworkUpdateComponents)
		{
			(postNetworkUpdateComponent as IOnPostNetworkUpdate)?.OnPostNetworkUpdate(entity);
		}
		foreach (BaseEntity child in children)
		{
			child.BroadcastOnPostNetworkUpdate(entity);
		}
	}

	public virtual void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!serverside)
		{
			postNetworkUpdateComponents = GetComponentsInChildren<IOnPostNetworkUpdate>(includeInactive: true).Cast<Component>().ToList();
		}
	}

	private void OnNetworkLimitStart()
	{
		LogEntry(RustLog.EntryType.Network, 2, "OnNetworkLimitStart");
		List<Connection> subscribers = GetSubscribers();
		if (subscribers == null || CollectionEx.IsEmpty(subscribers))
		{
			return;
		}
		List<Connection> obj = Facepunch.Pool.Get<List<Connection>>();
		foreach (Connection item in subscribers)
		{
			if (!ShouldNetworkTo(item.player as BasePlayer))
			{
				obj.Add(item);
			}
		}
		OnNetworkSubscribersLeave(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.OnNetworkLimitStart();
		}
	}

	private void OnNetworkLimitEnd()
	{
		LogEntry(RustLog.EntryType.Network, 2, "OnNetworkLimitEnd");
		List<Connection> subscribers = GetSubscribers();
		if (subscribers == null)
		{
			return;
		}
		OnNetworkSubscribersEnter(subscribers);
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.OnNetworkLimitEnd();
		}
	}

	public BaseEntity GetParentEntity()
	{
		return parentEntity.Get(isServer);
	}

	public BaseEntity GetRootParentEntity()
	{
		BaseEntity baseEntity = this as BaseEntity;
		BaseEntity baseEntity2 = GetParentEntity();
		while (baseEntity2 != null)
		{
			baseEntity = baseEntity2;
			baseEntity2 = baseEntity.GetParentEntity();
		}
		return baseEntity;
	}

	public bool HasParent()
	{
		return parentEntity.IsValid(isServer);
	}

	public void AddChild(BaseEntity child)
	{
		if (!children.Contains(child))
		{
			children.Add(child);
			OnChildAdded(child);
		}
	}

	protected virtual void OnChildAdded(BaseEntity child)
	{
	}

	public void RemoveChild(BaseEntity child)
	{
		children.Remove(child);
		OnChildRemoved(child);
	}

	protected virtual void OnChildRemoved(BaseEntity child)
	{
	}

	public static Group GetGlobalNetworkGroup(GlobalNetworkBehavior mode)
	{
		return mode switch
		{
			GlobalNetworkBehavior.MainIsland => MainIslandGroup, 
			GlobalNetworkBehavior.DeepSea => DeepSeaGroup, 
			_ => GlobalNetworkGroup, 
		};
	}

	public virtual float GetNetworkTime()
	{
		return UnityEngine.Time.time;
	}

	public virtual float GetNetworkTime(in ThreadSafeTime time)
	{
		return time.Time;
	}

	public virtual void Spawn()
	{
		EntityProfiler.spawned++;
		if (EntityProfiler.mode >= 2)
		{
			EntityProfiler.OnSpawned(this);
		}
		SpawnShared();
		if (net == null)
		{
			net = Network.Net.sv.CreateNetworkable();
		}
		creationFrame = UnityEngine.Time.frameCount;
		PreInitShared();
		InitShared();
		ServerInit();
		PostInitShared();
		UpdateNetworkGroup();
		ServerInitPostNetworkGroupAssign();
		isSpawned = true;
		Interface.CallHook("OnEntitySpawned", this);
		SendNetworkUpdateImmediate();
		Invoke(SendGlobalNetworkUpdate, 0f);
		if (Rust.Application.isLoading && !Rust.Application.isLoadingSave)
		{
			base.gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
		}
	}

	private void SendGlobalNetworkUpdate()
	{
		GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
	}

	public bool IsFullySpawned()
	{
		return isSpawned;
	}

	public virtual void ServerInit()
	{
		serverEntities.RegisterID(this);
		if (net != null)
		{
			net.handler = this;
		}
	}

	public virtual void ServerInitPostNetworkGroupAssign()
	{
	}

	public List<Connection> GetSubscribers()
	{
		if (net == null)
		{
			return null;
		}
		if (net.group == null)
		{
			return null;
		}
		return net.group.subscribers;
	}

	public void KillMessage()
	{
		Kill();
	}

	public virtual void AdminKill()
	{
		Kill(DestroyMode.Gib);
	}

	public virtual void OnKilled()
	{
	}

	public void Kill(DestroyMode mode = DestroyMode.None, bool callOnKilled = true)
	{
		if (IsDestroyed)
		{
			Debug.LogWarning("Calling kill - but already IsDestroyed!? " + this);
		}
		else if (Interface.CallHook("OnEntityKill", this) == null)
		{
			EntityProfiler.killed++;
			if (EntityProfiler.mode >= 2)
			{
				EntityProfiler.OnKilled(this);
			}
			OnParentDestroyingEx.BroadcastOnParentDestroying(base.gameObject);
			if (callOnKilled)
			{
				OnKilled();
			}
			DoEntityDestroy();
			TerminateOnClient(mode);
			TerminateOnServer();
			EntityDestroy();
		}
	}

	public void KillAsMapEntity()
	{
		if (IsFullySpawned())
		{
			Kill();
			return;
		}
		IsDestroyed = true;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void TerminateOnClient(DestroyMode mode)
	{
		if (net != null && net.group != null && Network.Net.sv.IsConnected())
		{
			LogEntry(RustLog.EntryType.Network, 2, "Term {0}", mode);
			NetWrite netWrite = Network.Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityDestroy);
			netWrite.EntityID(net.ID);
			netWrite.UInt8((byte)mode);
			netWrite.Send(new SendInfo(net.group.subscribers));
			GlobalNetworkHandler.server?.OnEntityKilled(this);
		}
	}

	public void TerminateOnServer()
	{
		if (net != null)
		{
			InvalidateNetworkCache();
			serverEntities.UnregisterID(this);
			Network.Net.sv.DestroyNetworkable(ref net);
			StopAllCoroutines();
			base.gameObject.SetActive(value: false);
		}
	}

	internal virtual void DoServerDestroy()
	{
		isSpawned = false;
	}

	public virtual bool ShouldNetworkTo(BasePlayer player)
	{
		object obj = Interface.CallHook("CanNetworkTo", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (net.group == null)
		{
			return true;
		}
		return player.net.subscriber.IsSubscribed(net.group);
	}

	public void SendNetworkGroupChange()
	{
		if (isSpawned && Network.Net.sv.IsConnected())
		{
			if (net.group == null)
			{
				Debug.LogWarning(ToString() + " changed its network group to null");
				return;
			}
			NetWrite netWrite = Network.Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.GroupChange);
			netWrite.EntityID(net.ID);
			netWrite.GroupID(net.group.ID);
			netWrite.Send(new SendInfo(net.group.subscribers));
		}
	}

	public void SendAsSnapshot(Connection connection, bool ordered = true)
	{
		if (Interface.CallHook("OnEntitySnapshot", this, connection) == null)
		{
			NetWrite netWrite = Network.Net.sv.StartWrite();
			uint val = (ordered ? (++connection.validate.entityUpdates) : uint.MaxValue);
			SaveInfo saveInfo = default(SaveInfo);
			saveInfo.forConnection = connection;
			saveInfo.forDisk = false;
			saveInfo.cachedTime = ThreadSafeTime.TakeSnapshot();
			SaveInfo saveInfo2 = saveInfo;
			netWrite.PacketID(Message.Type.Entities);
			netWrite.UInt32(val);
			ToStreamForNetwork(netWrite, saveInfo2);
			NetProfileCapture.Annotate(netWrite, net.ID.Value, prefabID);
			netWrite.Send(new SendInfo(connection));
		}
	}

	public void SendAsSnapshot(Connection connection, in ThreadSafeTime time, bool ordered = true)
	{
		NetWrite netWrite = Network.Net.sv.StartWrite();
		uint val = (ordered ? (++connection.validate.entityUpdates) : uint.MaxValue);
		SaveInfo saveInfo = default(SaveInfo);
		saveInfo.forConnection = connection;
		saveInfo.forDisk = false;
		saveInfo.cachedTime = time;
		SaveInfo saveInfo2 = saveInfo;
		netWrite.PacketID(Message.Type.Entities);
		netWrite.UInt32(val);
		ToStreamForNetwork(netWrite, saveInfo2);
		NetProfileCapture.Annotate(netWrite, net.ID.Value, prefabID);
		netWrite.Send(new SendInfo(connection));
	}

	public void SendAsSnapshot(Connection connection, NetWrite write, in ThreadSafeTime time, bool ordered = true)
	{
		uint val = (ordered ? (++connection.validate.entityUpdates) : uint.MaxValue);
		if (Interface.CallHook("OnEntitySnapshot", this, connection) == null)
		{
			SaveInfo saveInfo = default(SaveInfo);
			saveInfo.forConnection = connection;
			saveInfo.forDisk = false;
			saveInfo.cachedTime = time;
			SaveInfo saveInfo2 = saveInfo;
			write.PacketID(Message.Type.Entities);
			write.UInt32(val);
			ToStreamForNetwork(write, saveInfo2);
			NetProfileCapture.Annotate(write, net.ID.Value, prefabID);
			write.Send(new SendInfo(connection));
		}
	}

	public void SendAsSnapshotWithChildren(BasePlayer player, bool ordered = true)
	{
		Connection connection = player.net.connection;
		SendAsSnapshot(connection, ordered);
		SendChildren(children, player, ordered);
		static void SendChildren(List<BaseEntity> children, BasePlayer player, bool ordered)
		{
			Connection connection2 = player.net.connection;
			foreach (BaseEntity child in children)
			{
				if (child.ShouldNetworkTo(player))
				{
					child.SendAsSnapshot(connection2, ordered);
					SendChildren(child.children, player, ordered);
				}
			}
		}
	}

	public void SendNetworkUpdate(BasePlayer.NetworkQueue queue = BasePlayer.NetworkQueue.Update)
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdate");
			InvalidateNetworkCache();
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0)
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					BasePlayer basePlayer = subscribers[i].player as BasePlayer;
					if (!(basePlayer == null) && ShouldNetworkTo(basePlayer))
					{
						basePlayer.QueueUpdate(queue, this);
					}
				}
			}
		}
		base.gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
	}

	public void SendNetworkUpdateImmediate()
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdateImmediate"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdateImmediate");
			InvalidateNetworkCache();
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null && subscribers.Count > 0)
			{
				for (int i = 0; i < subscribers.Count; i++)
				{
					Connection connection = subscribers[i];
					BasePlayer basePlayer = connection.player as BasePlayer;
					if (!(basePlayer == null) && ShouldNetworkTo(basePlayer))
					{
						SendAsSnapshot(connection);
					}
				}
			}
		}
		base.gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
	}

	public void SendNetworkUpdate_Position()
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate_Position"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdate_Position");
			List<Connection> subscribers = GetSubscribers();
			List<Connection> obj = subscribers;
			if (subscribers == null || subscribers.Count <= 0)
			{
				return;
			}
			bool flag = ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion();
			bool stall_position_restrictions = ConVar.AntiHack.stall_position_restrictions;
			if (flag || stall_position_restrictions)
			{
				List<Connection> list = Facepunch.Pool.Get<List<Connection>>();
				foreach (Connection item in obj)
				{
					BasePlayer basePlayer = item.player as BasePlayer;
					if (!(basePlayer == null) && (!flag || ShouldNetworkTo(basePlayer)) && (!stall_position_restrictions || !basePlayer.isStalled))
					{
						list.Add(item);
					}
				}
				obj = list;
			}
			if (obj.Count > 0)
			{
				NetWrite netWrite = Network.Net.sv.StartWrite();
				netWrite.PacketID(Message.Type.EntityPosition);
				netWrite.EntityID(net.ID);
				Vector3 obj2 = GetNetworkPosition();
				netWrite.Vector3(in obj2);
				obj2 = GetNetworkRotation().eulerAngles;
				netWrite.Vector3(in obj2);
				netWrite.Float(GetNetworkTime());
				NetworkableId uid = parentEntity.uid;
				if (uid.IsValid)
				{
					netWrite.EntityID(uid);
				}
				SendInfo sendInfo = new SendInfo(obj);
				sendInfo.method = SendMethod.ReliableUnordered;
				sendInfo.priority = Priority.Immediate;
				SendInfo info = sendInfo;
				netWrite.Send(info);
			}
			if (obj != subscribers)
			{
				Facepunch.Pool.FreeUnmanaged(ref obj);
			}
		}
	}

	public void ToStream(Stream stream, SaveInfo saveInfo)
	{
		using (saveInfo.msg = Facepunch.Pool.Get<ProtoBuf.Entity>())
		{
			Save(saveInfo);
			if (saveInfo.msg.baseEntity == null)
			{
				Debug.LogError(this?.ToString() + ": ToStream - no BaseEntity!?");
			}
			if (saveInfo.msg.baseNetworkable == null)
			{
				Debug.LogError(this?.ToString() + ": ToStream - no baseNetworkable!?");
			}
			Interface.CallHook("IOnEntitySaved", this, saveInfo);
			saveInfo.msg.WriteToStream(stream);
			PostSave(saveInfo);
		}
	}

	public virtual bool CanUseNetworkCache(Connection connection)
	{
		return ConVar.Server.netcache;
	}

	public void ToStreamForNetwork(Stream stream, SaveInfo saveInfo)
	{
		if (!CanUseNetworkCache(saveInfo.forConnection))
		{
			ToStream(stream, saveInfo);
			return;
		}
		if (_NetworkCache == null)
		{
			if (!EntityMemoryStreamPool.TryDequeue(out var result))
			{
				result = new MemoryStream(8);
			}
			try
			{
				ToStream(result, saveInfo);
			}
			catch
			{
				result.SetLength(0L);
				EntityMemoryStreamPool.Enqueue(result);
				throw;
			}
			if (Interlocked.CompareExchange(ref _NetworkCache, result, null) == null)
			{
				_NetworkCache = result;
				ConVar.Server.netcachesize += (int)result.Length;
			}
			else
			{
				result.SetLength(0L);
				EntityMemoryStreamPool.Enqueue(result);
			}
		}
		_NetworkCache.WriteTo(stream);
	}

	public void InvalidateNetworkCache()
	{
		using (TimeWarning.New("InvalidateNetworkCache"))
		{
			if (_SaveCache != null)
			{
				ConVar.Server.savecachesize -= (int)_SaveCache.Length;
				_SaveCache.SetLength(0L);
				_SaveCache.Position = 0L;
				EntityMemoryStreamPool.Enqueue(_SaveCache);
				_SaveCache = null;
			}
			if (_NetworkCache != null)
			{
				ConVar.Server.netcachesize -= (int)_NetworkCache.Length;
				_NetworkCache.SetLength(0L);
				_NetworkCache.Position = 0L;
				EntityMemoryStreamPool.Enqueue(_NetworkCache);
				_NetworkCache = null;
			}
			LogEntry(RustLog.EntryType.Network, 3, "InvalidateNetworkCache");
		}
	}

	public MemoryStream GetSaveCache()
	{
		if (_SaveCache == null)
		{
			if (!EntityMemoryStreamPool.TryDequeue(out _SaveCache))
			{
				_SaveCache = new MemoryStream(8);
			}
			SaveInfo saveInfo = default(SaveInfo);
			saveInfo.forDisk = true;
			saveInfo.cachedTime = ThreadSafeTime.TakeSnapshot();
			SaveInfo saveInfo2 = saveInfo;
			ToStream(_SaveCache, saveInfo2);
			ConVar.Server.savecachesize += (int)_SaveCache.Length;
		}
		return _SaveCache;
	}

	public virtual void UpdateNetworkGroup()
	{
		Assert.IsTrue(isServer, "UpdateNetworkGroup called on clientside entity!");
		if (net == null)
		{
			return;
		}
		using (TimeWarning.New("UpdateGroups"))
		{
			if (net.UpdateGroups(base.transform.position, networkRange))
			{
				SendNetworkGroupChange();
			}
		}
	}

	public virtual bool SupportsServerOcclusion()
	{
		return false;
	}

	protected void OcclusionInitGroup(bool canBeInAGroup)
	{
		if (occlusionGroup != null)
		{
			return;
		}
		occlusionGroup = Facepunch.Pool.Get<ServerOcclusion.Group>();
		occlusionGroup.Add(this);
		OcclusionAddGroupRef(this);
		if (ServerOcclusion.Occludees.TryGetValue(net.group, out var value) && value.Contains(this))
		{
			return;
		}
		OcclusionEnterGroup(net.group);
		List<Connection> subscribers = net.group.subscribers;
		if (subscribers == null)
		{
			return;
		}
		foreach (Connection item in subscribers)
		{
			if (item != net.connection && !RustRelay.IsFakeConnection(item))
			{
				BaseNetworkable baseNetworkable = item.player as BaseNetworkable;
				baseNetworkable.occlusionGroup.Add(this);
				OcclusionAddGroupRef(baseNetworkable);
			}
		}
	}

	protected void OcclusionTransitionNetGroup(Group oldGroup, Group newGroup)
	{
		if (newGroup == null)
		{
			if (occlusionGroup != null)
			{
				OcclusionOnDestroy(oldGroup);
			}
			return;
		}
		OcclusionLeaveGroup(oldGroup);
		if (oldGroup == null && occlusionGroup == null)
		{
			OcclusionInitGroup(canBeInAGroup: false);
		}
		else
		{
			OcclusionEnterGroup(newGroup);
		}
		List<Connection> obj = null;
		List<Connection> obj2 = null;
		int num;
		if (oldGroup != null)
		{
			List<Connection> subscribers = oldGroup.subscribers;
			num = ((subscribers != null && !CollectionEx.IsEmpty(subscribers)) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		List<Connection> subscribers2 = newGroup.subscribers;
		bool flag2 = subscribers2 != null && !CollectionEx.IsEmpty(subscribers2);
		if (flag && flag2)
		{
			obj = Facepunch.Pool.Get<List<Connection>>();
			obj2 = Facepunch.Pool.Get<List<Connection>>();
			oldGroup.subscribers.Compare(newGroup.subscribers, obj, obj2, null);
		}
		else if (flag)
		{
			obj2 = oldGroup.subscribers;
		}
		else if (flag2)
		{
			obj = newGroup.subscribers;
		}
		if (obj != null)
		{
			foreach (Connection item in obj)
			{
				if (item != net.connection && !RustRelay.IsFakeConnection(item))
				{
					BaseNetworkable baseNetworkable = item.player as BaseNetworkable;
					baseNetworkable.occlusionGroup.Add(this);
					OcclusionAddGroupRef(baseNetworkable);
				}
			}
			if (obj != newGroup.subscribers)
			{
				Facepunch.Pool.FreeUnmanaged(ref obj);
			}
		}
		if (obj2 == null)
		{
			return;
		}
		foreach (Connection item2 in obj2)
		{
			if (item2 != net.connection && !RustRelay.IsFakeConnection(item2))
			{
				(item2.player as BaseNetworkable).OcclusionLeavePlayersGroup(this);
			}
		}
		if (obj2 != oldGroup.subscribers)
		{
			Facepunch.Pool.FreeUnmanaged(ref obj2);
		}
	}

	private void OcclusionEnterGroup(Group newGroup)
	{
		if (!ServerOcclusion.Occludees.TryGetValue(newGroup, out var value))
		{
			value = Facepunch.Pool.Get<ServerOcclusion.Group>();
			ServerOcclusion.Occludees[newGroup] = value;
		}
		value.TryAdd(this);
	}

	private void OcclusionLeaveGroup(Group oldGroup)
	{
		if (oldGroup != null && ServerOcclusion.Occludees.TryGetValue(oldGroup, out var value))
		{
			OcclusionLeaveOcclGroup(value);
			if (CollectionEx.IsEmpty(value))
			{
				ServerOcclusion.Occludees.Remove(oldGroup);
				Facepunch.Pool.Free(ref value);
			}
		}
	}

	private void OcclusionOnDestroy(Group oldGroup)
	{
		OcclusionLeaveGroup(oldGroup);
		occlusionGroup.Remove(this);
		OcclusionRemoveGroupRef(this);
		foreach (BaseNetworkable item in occlusionGroup)
		{
			item.OcclusionRemoveGroupRef(this);
		}
		Facepunch.Pool.Free(ref occlusionGroup);
		while (occlusionGroupRefs != null && !CollectionEx.IsEmpty(occlusionGroupRefs))
		{
			occlusionGroupRefs[0].OcclusionLeavePlayersGroup(this);
		}
	}

	private bool OcclusionLeaveOcclGroup(ServerOcclusion.Group globalGroup)
	{
		return globalGroup.Remove(this);
	}

	public void OcclusionSubscribedTo(Group group)
	{
		if (!Network.Net.sv.visibility.IsGroupIdSpecial(group.ID))
		{
			int item = Network.Net.sv.visibility.DeconstructGroupId((int)group.ID).layer;
			if (item == 2 && item == 0)
			{
				return;
			}
		}
		if (!ServerOcclusion.Occludees.TryGetValue(group, out var value))
		{
			return;
		}
		foreach (BaseNetworkable item2 in value)
		{
			if ((object)this != item2)
			{
				occlusionGroup.Add(item2);
				item2.OcclusionAddGroupRef(this);
			}
		}
	}

	private void OcclusionUnsubscribedFrom(Group group)
	{
		if (!Network.Net.sv.visibility.IsGroupIdSpecial(group.ID))
		{
			int item = Network.Net.sv.visibility.DeconstructGroupId((int)group.ID).layer;
			if (item == 2 && item == 0)
			{
				return;
			}
		}
		if (!ServerOcclusion.Occludees.TryGetValue(group, out var value))
		{
			return;
		}
		foreach (BaseNetworkable item2 in value)
		{
			if ((object)this != item2)
			{
				OcclusionLeavePlayersGroup(item2);
			}
		}
	}

	protected virtual bool OcclusionLeavePlayersGroup(BaseNetworkable other)
	{
		bool num = occlusionGroup.Remove(other);
		if (num)
		{
			other.OcclusionRemoveGroupRef(this);
		}
		return num;
	}

	private void OcclusionAddGroupRef(BaseNetworkable other)
	{
		if (occlusionGroupRefs == null)
		{
			occlusionGroupRefs = Facepunch.Pool.Get<ListHashSet<BaseNetworkable>>();
		}
		occlusionGroupRefs.TryAdd(other);
	}

	private void OcclusionRemoveGroupRef(BaseNetworkable other)
	{
		occlusionGroupRefs.Remove(other);
		if (CollectionEx.IsEmpty(occlusionGroupRefs))
		{
			Facepunch.Pool.FreeUnmanaged(ref occlusionGroupRefs);
		}
	}

	protected void OcclusionOldRemoveFromOcclusion()
	{
		if (occlusionGroup != null && net.group != null)
		{
			ulong value = net.ID.Value;
			foreach (BaseNetworkable item in occlusionGroup)
			{
				BasePlayer basePlayer = item as BasePlayer;
				if (basePlayer != null)
				{
					basePlayer.lastPlayerVisibility.Remove(value);
				}
			}
			occlusionGroup = null;
			ListHashSet<Group> obj = Facepunch.Pool.Get<ListHashSet<Group>>();
			net.SubStrategy.GatherSubscriptions(net, obj);
			foreach (Group item2 in obj)
			{
				if (ServerOcclusion.Occludees.TryGetValue(item2, out var value2))
				{
					value2.Remove(this);
					if (value2.Count == 0)
					{
						ServerOcclusion.Occludees.Remove(item2);
					}
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		BasePlayer obj2 = this as BasePlayer;
		obj2.lastPlayerVisibility.Clear();
		obj2.FreeUnoccludedSubscribers();
	}

	protected void OcclusionOnDisconnect()
	{
		foreach (BaseNetworkable item in occlusionGroup)
		{
			if (!(item == this))
			{
				item.OcclusionRemoveGroupRef(this);
			}
		}
		occlusionGroup.Clear();
		occlusionGroup.Add(this);
	}

	public virtual Vector3 GetNetworkPosition()
	{
		if (UseParallelSaves)
		{
			return Facepunch.Extend.TransformEx.Unsafe.GetLocalPosMT(in _transformHandle);
		}
		return _transformHandle.localPosition;
	}

	public virtual Quaternion GetNetworkRotation()
	{
		if (UseParallelSaves)
		{
			return Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in _transformHandle);
		}
		return _transformHandle.localRotation;
	}

	public string InvokeString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<InvokeAction> obj = Facepunch.Pool.Get<List<InvokeAction>>();
		InvokeHandler.FindInvokes(this, obj);
		foreach (InvokeAction item in obj)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.action.Method.Name);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return stringBuilder.ToString();
	}

	public BaseEntity LookupPrefab()
	{
		if (_prefab == null)
		{
			_prefab = GameObjectEx.ToBaseEntity(gameManager.FindPrefab(PrefabName));
		}
		return _prefab;
	}

	public T LookupPrefab<T>() where T : BaseEntity
	{
		return LookupPrefab() as T;
	}

	public bool EqualNetID(BaseNetworkable other)
	{
		if (!other.IsRealNull() && other.net != null && net != null)
		{
			return other.net.ID == net.ID;
		}
		return false;
	}

	public bool EqualNetID(NetworkableId otherID)
	{
		if (net != null)
		{
			return otherID == net.ID;
		}
		return false;
	}

	public virtual void ResetState()
	{
		if (children.Count > 0)
		{
			children.Clear();
		}
		if (this is ILootableEntity lootableEntity)
		{
			lootableEntity.LastLootedBy = 0uL;
		}
	}

	public virtual void InitShared()
	{
	}

	public virtual void PreInitShared()
	{
	}

	public virtual void PostInitShared()
	{
	}

	public virtual void DestroyShared()
	{
	}

	public virtual void OnNetworkGroupEnter(Group group)
	{
		Interface.CallHook("OnNetworkGroupEntered", this, group);
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionSubscribedTo(group);
		}
	}

	public virtual void OnNetworkGroupLeave(Group group)
	{
		Interface.CallHook("OnNetworkGroupLeft", this, group);
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionUnsubscribedFrom(group);
		}
	}

	public void OnNetworkGroupChange(Group oldGroup)
	{
		if (isServer)
		{
			InvalidateNetworkCache();
		}
		if (children != null && net.group != null)
		{
			foreach (BaseEntity child in children)
			{
				if (child.IsRealNull())
				{
					Debug.LogError("Child is null when switching groups", this);
				}
				else if (child.net != null)
				{
					if (child.ShouldInheritNetworkGroup() && ShouldChildrenInheritNetworkGroup())
					{
						child.net.SwitchGroup(net.group);
					}
					else if (isServer)
					{
						child.UpdateNetworkGroup();
					}
				}
			}
		}
		if (isServer && ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionTransitionNetGroup(oldGroup, net.group);
		}
	}

	public virtual bool ShouldChildrenInheritNetworkGroup()
	{
		return true;
	}

	public void OnNetworkSubscribersEnter(List<Connection> connections)
	{
		if (!Network.Net.sv.IsConnected() || !isServer || CollectionEx.IsEmpty(connections))
		{
			return;
		}
		foreach (Connection connection in connections)
		{
			BasePlayer basePlayer = connection.player as BasePlayer;
			if (!(basePlayer == null))
			{
				basePlayer.QueueUpdate(BasePlayer.NetworkQueue.Update, this as BaseEntity);
			}
		}
	}

	public void OnNetworkSubscribersLeave(List<Connection> connections)
	{
		if (Network.Net.sv.IsConnected() && isServer && connections != null && !CollectionEx.IsEmpty(connections))
		{
			LogEntry(RustLog.EntryType.Network, 2, "LeaveVisibility");
			NetWrite netWrite = Network.Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityDestroy);
			netWrite.EntityID(net.ID);
			netWrite.UInt8(0);
			netWrite.Send(new SendInfo(connections));
		}
	}

	public void EntityDestroy()
	{
		if ((bool)base.gameObject)
		{
			ResetState();
			gameManager.Retire(base.gameObject);
		}
	}

	private void DoEntityDestroy()
	{
		if (IsDestroyed)
		{
			return;
		}
		IsDestroyed = true;
		if (Rust.Application.isQuitting)
		{
			return;
		}
		DestroyShared();
		if (isServer)
		{
			DoServerDestroy();
		}
		using (TimeWarning.New("Registry.Entity.Unregister"))
		{
			Rust.Registry.Entity.Unregister(base.gameObject);
		}
	}

	private void SpawnShared()
	{
		IsDestroyed = false;
		_transformHandle = base.gameObject.transformHandle;
		using (TimeWarning.New("Registry.Entity.Register"))
		{
			Rust.Registry.Entity.Register(base.gameObject, this);
		}
	}

	public virtual void Save(SaveInfo info)
	{
		if (prefabID == 0)
		{
			Debug.LogError("PrefabID is 0! " + UnityEngine.TransformEx.GetRecursiveName(base.transform), base.gameObject);
		}
		info.msg.baseNetworkable = Facepunch.Pool.Get<ProtoBuf.BaseNetworkable>();
		info.msg.baseNetworkable.uid = net.ID;
		info.msg.baseNetworkable.prefabID = prefabID;
		if (net.group != null)
		{
			info.msg.baseNetworkable.group = net.group.ID;
		}
		if (!info.forDisk)
		{
			info.msg.createdThisFrame = creationFrame == info.cachedTime.FrameCount;
		}
	}

	public virtual void PostSave(SaveInfo info)
	{
	}

	public void InitLoad(NetworkableId entityID)
	{
		net = Network.Net.sv.CreateNetworkable(entityID);
		serverEntities.RegisterID(this);
	}

	public virtual void PreServerLoad()
	{
	}

	public virtual void Load(LoadInfo info)
	{
		if (info.msg.baseNetworkable != null)
		{
			Interface.CallHook("OnEntityLoaded", this, info);
			ProtoBuf.BaseNetworkable baseNetworkable = info.msg.baseNetworkable;
			if (prefabID != baseNetworkable.prefabID && 0 == 0)
			{
				Debug.LogError("Prefab IDs don't match! " + prefabID + "/" + baseNetworkable.prefabID + " -> " + base.gameObject, base.gameObject);
			}
		}
	}

	public virtual void PostServerLoad()
	{
		base.gameObject.SendOnSendNetworkUpdate(this as BaseEntity);
	}

	public T ToServer<T>() where T : BaseNetworkable
	{
		if (isServer)
		{
			return this as T;
		}
		return null;
	}

	public virtual bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		return false;
	}

	protected virtual bool OnSyncVar(byte syncVar, NetRead reader, bool fromAutoSave = false)
	{
		return false;
	}

	protected virtual bool WriteSyncVar(byte id, NetWrite writer)
	{
		return false;
	}

	protected virtual void WriteAutoSaveSyncVars(NetWrite writer)
	{
	}

	protected virtual void ReadAutoSaveSyncVars(NetRead reader)
	{
	}

	protected virtual bool AutoSaveSyncVars(SaveInfo save)
	{
		return false;
	}

	protected virtual bool AutoLoadSyncVars(LoadInfo load)
	{
		return false;
	}

	protected virtual void ResetSyncVars()
	{
	}

	protected virtual bool ShouldInvalidateCache(byte id)
	{
		return false;
	}

	protected virtual bool IsSyncVarEqual<T>(T oldValue, T newValue)
	{
		return EqualityComparer<T>.Default.Equals(oldValue, newValue);
	}

	public static List<Connection> GetConnectionsWithin(Vector3 position, float distance, bool includeInvisPlayers = false)
	{
		connectionsInSphereList.Clear();
		using PooledList<BasePlayer> pooledList = Facepunch.Pool.Get<PooledList<BasePlayer>>();
		BaseEntity.Query.Server.GetPlayersInSphere(position, distance, pooledList);
		float num = distance * distance;
		foreach (BasePlayer item in pooledList)
		{
			if (item == null || item.isClient || item.Connection == null)
			{
				continue;
			}
			if (!connectionsInSphereList.Contains(item.Connection))
			{
				connectionsInSphereList.Add(item.Connection);
			}
			if (!item.IsBeingSpectated)
			{
				continue;
			}
			ReadOnlySpan<BasePlayer> spectators = item.GetSpectators();
			for (int i = 0; i < spectators.Length; i++)
			{
				BasePlayer basePlayer = spectators[i];
				if (!connectionsInSphereList.Contains(basePlayer.Connection))
				{
					connectionsInSphereList.Add(basePlayer.Connection);
				}
			}
		}
		if (includeInvisPlayers)
		{
			foreach (BasePlayer invisPlayer in BasePlayer.invisPlayers)
			{
				if ((invisPlayer.transform.position - position).sqrMagnitude <= num && !connectionsInSphereList.Contains(invisPlayer.Connection))
				{
					connectionsInSphereList.Add(invisPlayer.Connection);
				}
			}
		}
		return connectionsInSphereList;
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Vector3 position, float distance, List<Connection> foundConnections)
	{
		if (Network.Net.sv != null && Network.Net.sv.visibility != null)
		{
			GetCloseConnections(GetGroupForSubscriberChecks(position, distance), position, distance, foundConnections);
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Group group, Vector3 position, float distance, List<Connection> foundConnections)
	{
		if (Network.Net.sv == null || Network.Net.sv.visibility == null || group == null || group.subscribers == null)
		{
			return;
		}
		List<Connection> subscribers = group.subscribers;
		float num = distance * distance;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!(basePlayer == null) && !(basePlayer.SqrDistance(position) > num) && !foundConnections.Contains(basePlayer.Connection))
				{
					foundConnections.Add(basePlayer.Connection);
				}
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Vector3 position, float distance, List<BasePlayer> players)
	{
		if (Network.Net.sv != null && Network.Net.sv.visibility != null)
		{
			GetCloseConnections(GetGroupForSubscriberChecks(position, distance), position, distance, players);
		}
	}

	[PoolAnalyzerNonCaching]
	public static void GetCloseConnections(Group group, Vector3 position, float distance, List<BasePlayer> players)
	{
		if (Network.Net.sv == null || Network.Net.sv.visibility == null || group == null || group.subscribers == null)
		{
			return;
		}
		List<Connection> subscribers = group.subscribers;
		float num = distance * distance;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!(basePlayer == null) && !(basePlayer.SqrDistance(position) > num) && !players.Contains(basePlayer))
				{
					players.Add(basePlayer);
				}
			}
		}
	}

	public static bool HasCloseConnections(Vector3 position, float distance)
	{
		if (Network.Net.sv == null)
		{
			return false;
		}
		if (Network.Net.sv.visibility == null)
		{
			return false;
		}
		return HasCloseConnections(GetGroupForSubscriberChecks(position, distance), position, distance);
	}

	private static Group GetGroupForSubscriberChecks(Vector3 position, float distance)
	{
		float farDistanceForRange = Network.Net.sv.visibility.GetFarDistanceForRange(EntityNetworkRange.Small);
		if (distance < farDistanceForRange)
		{
			return Network.Net.sv.visibility.GetGroup(position, EntityNetworkRange.Small);
		}
		float farDistanceForRange2 = Network.Net.sv.visibility.GetFarDistanceForRange(EntityNetworkRange.Medium);
		if (distance < farDistanceForRange2)
		{
			return Network.Net.sv.visibility.GetGroup(position, EntityNetworkRange.Medium);
		}
		return Network.Net.sv.visibility.GetGroup(position, EntityNetworkRange.Large);
	}

	public static bool HasCloseConnections(Group group, Vector3 position, float distance)
	{
		if (Network.Net.sv == null)
		{
			return false;
		}
		if (Network.Net.sv.visibility == null)
		{
			return false;
		}
		if (group == null || group.subscribers == null)
		{
			return false;
		}
		List<Connection> subscribers = group.subscribers;
		float num = distance * distance;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active)
			{
				BasePlayer basePlayer = connection.player as BasePlayer;
				if (!(basePlayer == null) && !(basePlayer.SqrDistance(position) > num))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool HasConnections(Vector3 position)
	{
		if (Network.Net.sv == null)
		{
			return false;
		}
		if (Network.Net.sv.visibility == null)
		{
			return false;
		}
		return HasConnections(Network.Net.sv.visibility.GetGroup(position, EntityNetworkRange.Small), position);
	}

	public static bool HasConnections(Group group, Vector3 position)
	{
		if (Network.Net.sv == null)
		{
			return false;
		}
		if (Network.Net.sv.visibility == null)
		{
			return false;
		}
		if (group == null || group.subscribers == null)
		{
			return false;
		}
		List<Connection> subscribers = group.subscribers;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Connection connection = subscribers[i];
			if (connection.active && !(connection.player as BasePlayer == null))
			{
				return true;
			}
		}
		return false;
	}
}
