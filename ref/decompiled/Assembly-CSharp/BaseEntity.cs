#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Network;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Workshop;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class BaseEntity : BaseNetworkable, IOnParentSpawning, IPrefabPreProcess
{
	public class Menu : Attribute
	{
		[Serializable]
		public struct Option
		{
			public Translate.Phrase name;

			public Translate.Phrase description;

			public Sprite icon;

			public int order;

			public bool usableWhileWounded;
		}

		public class Description : Attribute
		{
			public string token;

			public string english;

			public Description(string t, string e)
			{
				token = t;
				english = e;
			}
		}

		public class Icon : Attribute
		{
			public string icon;

			public Icon(string i)
			{
				icon = i;
			}
		}

		public class ShowIf : Attribute
		{
			public string functionName;

			public ShowIf(string testFunc)
			{
				functionName = testFunc;
			}
		}

		public class DisabledIf : Attribute
		{
			public string functionName;

			public DisabledIf(string testFunc)
			{
				functionName = testFunc;
			}
		}

		public class Priority : Attribute
		{
			public string functionName;

			public Priority(string priorityFunc)
			{
				functionName = priorityFunc;
			}
		}

		public class UsableWhileWounded : Attribute
		{
		}

		public string TitleToken;

		public string TitleEnglish;

		public string UseVariable;

		public int Order;

		public string ProxyFunction;

		public float Time;

		public string OnStart;

		public string OnProgress;

		public string OnCancel;

		public bool LongUseOnly;

		public bool PrioritizeIfNotWhitelisted;

		public bool PrioritizeIfUnlocked;

		public Menu()
		{
		}

		public Menu(string menuTitleToken, string menuTitleEnglish)
		{
			TitleToken = menuTitleToken;
			TitleEnglish = menuTitleEnglish;
		}
	}

	[Flags]
	public enum Flags
	{
		Placeholder = 1,
		On = 2,
		OnFire = 4,
		Open = 8,
		Locked = 0x10,
		Debugging = 0x20,
		Disabled = 0x40,
		Reserved1 = 0x80,
		Reserved2 = 0x100,
		Reserved3 = 0x200,
		Reserved4 = 0x400,
		Reserved5 = 0x800,
		Broken = 0x1000,
		Busy = 0x2000,
		Reserved6 = 0x4000,
		Reserved7 = 0x8000,
		Reserved8 = 0x10000,
		Reserved9 = 0x20000,
		Reserved10 = 0x40000,
		Reserved11 = 0x80000,
		InUse = 0x100000,
		Reserved12 = 0x200000,
		Reserved13 = 0x400000,
		Unused23 = 0x800000,
		Protected = 0x1000000,
		Transferring = 0x2000000,
		Reserved14 = 0x4000000,
		Reserved15 = 0x8000000,
		Reserved16 = 0x10000000,
		Reserved17 = 0x20000000,
		Reserved18 = 0x40000000,
		Reserved19 = int.MinValue
	}

	public enum FlagsUpdateMode
	{
		Local,
		SendNetworkUpdate_Flags,
		SendNetworkUpdate,
		SendNetworkUpdateImmediate
	}

	public readonly struct FlagsUpdateScope : IDisposable
	{
		private readonly BaseEntity owner;

		private readonly Flags oldFlags;

		private readonly FlagsUpdateMode updateMode;

		public FlagsUpdateScope(BaseEntity owner, FlagsUpdateMode updateMode)
		{
			this.owner = owner;
			oldFlags = owner.flags;
			this.updateMode = updateMode;
		}

		public void Set(Flags f, bool b, bool recursive = false)
		{
			if (b)
			{
				if (owner.HasFlag(f))
				{
					return;
				}
				owner.flags |= f;
			}
			else
			{
				if (!owner.HasFlag(f))
				{
					return;
				}
				owner.flags &= ~f;
			}
			if (!recursive || owner.children == null)
			{
				return;
			}
			int i = 0;
			for (int count = owner.children.Count; i < count; i++)
			{
				using FlagsUpdateScope flagsUpdateScope = owner.children[i].StartSetFlags(updateMode);
				flagsUpdateScope.Set(f, b, recursive: true);
			}
		}

		void IDisposable.Dispose()
		{
			if (oldFlags != owner.flags)
			{
				owner.OnFlagsChanged(oldFlags, owner.flags);
				owner.HandleFlagsUpdateMode(updateMode);
			}
		}
	}

	[Serializable]
	public struct MovementModify
	{
		public float drag;
	}

	private readonly struct QueuedFileRequest : IEquatable<QueuedFileRequest>
	{
		public readonly BaseEntity Entity;

		public readonly FileStorage.Type Type;

		public readonly uint Part;

		public readonly uint Crc;

		public readonly uint ResponseFunction;

		public readonly bool? RespondIfNotFound;

		public readonly bool IsEntityImage;

		public QueuedFileRequest(BaseEntity entity, FileStorage.Type type, uint part, uint crc, uint responseFunction, bool? respondIfNotFound, bool isEntityImage = false)
		{
			Entity = entity;
			Type = type;
			Part = part;
			Crc = crc;
			ResponseFunction = responseFunction;
			RespondIfNotFound = respondIfNotFound;
			IsEntityImage = isEntityImage;
		}

		public bool Equals(QueuedFileRequest other)
		{
			if (object.Equals(Entity, other.Entity) && Type == other.Type && Part == other.Part && Crc == other.Crc && ResponseFunction == other.ResponseFunction && RespondIfNotFound == other.RespondIfNotFound)
			{
				return IsEntityImage == other.IsEntityImage;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is QueuedFileRequest other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			uint num = ((((((((((uint)(((Entity != null) ? Entity.GetHashCode() : 0) * 397) ^ (uint)Type) * 397) ^ Part) * 397) ^ Crc) * 397) ^ ResponseFunction) * 397) ^ (uint)RespondIfNotFound.GetHashCode()) * 397;
			bool isEntityImage = IsEntityImage;
			return (int)num ^ isEntityImage.GetHashCode();
		}
	}

	private readonly struct PendingFileRequest : IEquatable<PendingFileRequest>
	{
		public readonly FileStorage.Type Type;

		public readonly uint NumId;

		public readonly uint Crc;

		public readonly IServerFileReceiver Receiver;

		public readonly float Time;

		public PendingFileRequest(FileStorage.Type type, uint numId, uint crc, IServerFileReceiver receiver)
		{
			Type = type;
			NumId = numId;
			Crc = crc;
			Receiver = receiver;
			Time = UnityEngine.Time.realtimeSinceStartup;
		}

		public bool Equals(PendingFileRequest other)
		{
			if (Type == other.Type && NumId == other.NumId && Crc == other.Crc)
			{
				return object.Equals(Receiver, other.Receiver);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PendingFileRequest other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(((((uint)((int)Type * 397) ^ NumId) * 397) ^ Crc) * 397) ^ ((Receiver != null) ? Receiver.GetHashCode() : 0);
		}
	}

	public static class Query
	{
		public enum DistanceCheckType
		{
			None,
			OnlyCenter,
			Bounds
		}

		public class EntityTree
		{
			public Grid<BaseEntity> Grid;

			public Grid<BasePlayer> PlayerGrid;

			public Grid<BaseEntity> BrainGrid;

			public EntityTree(float worldSize)
			{
				Grid = new Grid<BaseEntity>(32, worldSize);
				PlayerGrid = new Grid<BasePlayer>(32, worldSize);
				BrainGrid = new Grid<BaseEntity>(32, worldSize);
			}

			public void Add(BaseEntity ent)
			{
				Vector3 position = ent.transform.position;
				Grid.Add(ent, position.x, position.z);
			}

			public void AddPlayer(BasePlayer player)
			{
				Vector3 position = player.transform.position;
				PlayerGrid.Add(player, position.x, position.z);
			}

			public void AddBrain(BaseEntity entity)
			{
				Vector3 position = entity.transform.position;
				BrainGrid.Add(entity, position.x, position.z);
			}

			public void Remove(BaseEntity ent, bool isPlayer = false)
			{
				Grid.Remove(ent);
				if (isPlayer)
				{
					BasePlayer basePlayer = ent as BasePlayer;
					if (basePlayer != null)
					{
						PlayerGrid.Remove(basePlayer);
					}
				}
			}

			public void RemovePlayer(BasePlayer player)
			{
				PlayerGrid.Remove(player);
			}

			public void RemoveBrain(BaseEntity entity)
			{
				if (!(entity == null))
				{
					BrainGrid.Remove(entity);
				}
			}

			public void Move(BaseEntity ent)
			{
				Vector3 position = ent.transform.position;
				Grid.Move(ent, position.x, position.z);
				BasePlayer basePlayer = ent as BasePlayer;
				if (basePlayer != null)
				{
					MovePlayer(basePlayer);
				}
				if (ent.HasBrain)
				{
					MoveBrain(ent);
				}
			}

			public void MovePlayer(BasePlayer player)
			{
				Vector3 position = player.transform.position;
				PlayerGrid.Move(player, position.x, position.z);
			}

			public void MoveBrain(BaseEntity entity)
			{
				Vector3 position = entity.transform.position;
				BrainGrid.Move(entity, position.x, position.z);
			}

			public void SubscribePlayerChanges(Vector3 position, float radius, Action callback)
			{
				PlayerGrid.Subscribe(position.x, position.z, radius, callback);
			}

			[PoolAnalyzerNonCaching]
			public void GetInSphere<T>(Vector3 position, float distance, List<T> results, DistanceCheckType distanceCheckType = DistanceCheckType.OnlyCenter) where T : BaseEntity
			{
				using (TimeWarning.New("GetInSphereList"))
				{
					Grid.Query(position.x, position.z, distance, results);
					if (distanceCheckType != 0)
					{
						NarrowPhaseReduce(position, distance, results, distanceCheckType == DistanceCheckType.OnlyCenter);
					}
				}
			}

			public int GetInSphere(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				int broadCount = Grid.Query(position.x, position.z, distance, results, filter);
				return NarrowPhaseReduce(position, distance, results, broadCount);
			}

			public int GetInSphereFast(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				return Grid.Query(position.x, position.z, distance, results, filter);
			}

			[PoolAnalyzerNonCaching]
			public void GetPlayersInSphere(Vector3 position, float distance, List<BasePlayer> results, DistanceCheckType distanceCheckType = DistanceCheckType.OnlyCenter, bool includeHumanoidNpcs = false)
			{
				using (TimeWarning.New("GetPlayersInSphereList"))
				{
					PlayerGrid.Query(position.x, position.z, distance, results);
					if (!includeHumanoidNpcs)
					{
						for (int num = results.Count - 1; num >= 0; num--)
						{
							if (results[num].IsNpc)
							{
								results.RemoveAt(num);
							}
						}
					}
					if (distanceCheckType != 0)
					{
						NarrowPhaseReduce(position, distance, results, distanceCheckType == DistanceCheckType.OnlyCenter);
					}
				}
			}

			public int GetPlayersInSphere(Vector3 position, float distance, BasePlayer[] results, Func<BasePlayer, bool> filter = null)
			{
				int broadCount = PlayerGrid.Query(position.x, position.z, distance, results, filter);
				return NarrowPhaseReduce(position, distance, results, broadCount);
			}

			public int GetPlayersInSphereFast(Vector3 position, float distance, BasePlayer[] results, Func<BasePlayer, bool> filter = null)
			{
				return PlayerGrid.Query(position.x, position.z, distance, results, filter);
			}

			[PoolAnalyzerNonCaching]
			public void GetPlayersInSphereFast(Vector3 position, float distance, List<BasePlayer> results, Func<BasePlayer, bool> filter = null)
			{
				PlayerGrid.Query(position.x, position.z, distance, results, filter);
			}

			public bool AnyPlayersInSphereFast(Vector3 position, float distance, out bool gridNodesEmpty, Func<BasePlayer, bool> ignoreFilter = null, Func<BasePlayer, bool> filter = null)
			{
				using (TimeWarning.New("AnyPlayersInSphereFast"))
				{
					return PlayerGrid.Any(position.x, position.z, distance, ignoreFilter, filter, out gridNodesEmpty);
				}
			}

			public void GetBrainsInSphere<T>(Vector3 position, float distance, List<T> results, bool filterPastDistance = true) where T : BaseEntity
			{
				using (TimeWarning.New("GetBrainsInSphereList"))
				{
					BrainGrid.Query(position.x, position.z, distance, results);
					if (filterPastDistance)
					{
						NarrowPhaseReduce(position, distance, results);
					}
				}
			}

			public int GetBrainsInSphere(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				int broadCount = BrainGrid.Query(position.x, position.z, distance, results, filter);
				return NarrowPhaseReduce(position, distance, results, broadCount);
			}

			public int GetBrainsInSphereFast(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				return BrainGrid.Query(position.x, position.z, distance, results, filter);
			}

			[PoolAnalyzerNonCaching]
			public void GetPlayersAndBrainsInSphere(Vector3 position, float distance, List<BaseEntity> results, DistanceCheckType distanceCheckType = DistanceCheckType.OnlyCenter)
			{
				using (TimeWarning.New("GetPlayersAndBrainsInSphereList"))
				{
					PlayerGrid.Query(position.x, position.z, distance, results);
					BrainGrid.Query(position.x, position.z, distance, results);
					if (distanceCheckType != 0)
					{
						NarrowPhaseReduce(position, distance, results, distanceCheckType == DistanceCheckType.OnlyCenter);
					}
				}
			}

			private int NarrowPhaseReduce<T>(Vector3 position, float radius, T[] results, int broadCount) where T : BaseEntity
			{
				using (TimeWarning.New("NarrowPhaseReduce"))
				{
					int num = broadCount;
					float num2 = radius * radius;
					for (int i = 0; i < num; i++)
					{
						T val = results[i];
						if (val == null)
						{
							results[i] = results[num - 1];
							num--;
							i--;
						}
						else if ((val.WorldSpaceBounds().ClosestPoint(position) - position).sqrMagnitude > num2)
						{
							results[i] = results[num - 1];
							num--;
							i--;
						}
					}
					return num;
				}
			}

			[PoolAnalyzerNonCaching]
			private static void NarrowPhaseReduce<T>(Vector3 position, float radius, List<T> results, bool onlyConsiderCenter = true) where T : BaseEntity
			{
				using (TimeWarning.New("NarrowPhaseReduceList"))
				{
					float num = radius * radius;
					for (int num2 = results.Count - 1; num2 >= 0; num2--)
					{
						T val = results[num2];
						if (val == null)
						{
							results.RemoveAt(num2);
						}
						else if (((onlyConsiderCenter ? val.transform.position : val.WorldSpaceBounds().ClosestPoint(position)) - position).sqrMagnitude > num)
						{
							results.RemoveAt(num2);
						}
					}
				}
			}

			private static bool IsEntityInRadius<T>(Vector3 position, float radiusSq, T entity) where T : BaseEntity
			{
				using (TimeWarning.New("IsEntityInRadius"))
				{
					if (entity == null)
					{
						return false;
					}
					return (entity.WorldSpaceBounds().ClosestPoint(position) - position).sqrMagnitude < radiusSq;
				}
			}
		}

		public static EntityTree Server;
	}

	public class RPC_Shared : Attribute
	{
	}

	public struct RPCMessage
	{
		public Connection connection;

		public BasePlayer player;

		public NetRead read;
	}

	public class RPC_Server : RPC_Shared
	{
		public abstract class Conditional : Attribute
		{
			public virtual string GetArgs()
			{
				return null;
			}
		}

		public class MaxDistance : Conditional
		{
			private float maximumDistance;

			public bool CheckParent { get; set; }

			public MaxDistance(float maxDist)
			{
				maximumDistance = maxDist;
			}

			public override string GetArgs()
			{
				return maximumDistance.ToString("0.00f") + (CheckParent ? ", true" : "");
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, float maximumDistance, bool checkParent = false)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityDistanceCheck", ent, player, id, debugName, maximumDistance, checkParent);
				if (obj is bool)
				{
					return (bool)obj;
				}
				bool flag = ent.Distance(player.eyes.position) <= maximumDistance;
				if (checkParent && !flag)
				{
					BaseEntity parentEntity = ent.GetParentEntity();
					flag = parentEntity != null && parentEntity.Distance(player.eyes.position) <= maximumDistance;
				}
				return flag;
			}
		}

		public class IsVisible : Conditional
		{
			private float maximumDistance;

			public IsVisible(float maxDist)
			{
				maximumDistance = maxDist;
			}

			public override string GetArgs()
			{
				return maximumDistance.ToString("0.00f");
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, float maximumDistance)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityVisibilityCheck", ent, player, id, debugName, maximumDistance);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (GamePhysics.LineOfSight(player.eyes.center, player.eyes.position, 1218519041))
				{
					if (!ent.IsVisible(player.eyes.HeadRay(), 1218519041, maximumDistance))
					{
						return ent.IsVisible(player.eyes.position, maximumDistance);
					}
					return true;
				}
				return false;
			}
		}

		public class FromOwner : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityFromOwnerCheck", ent, player, id, debugName);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (ent.net.ID == player.net.ID)
				{
					return true;
				}
				if (ent.parentEntity.uid != player.net.ID)
				{
					BaseEntity parentEntity = ent.GetParentEntity();
					if (parentEntity != null && parentEntity.parentEntity.uid == player.net.ID)
					{
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public class IsActiveItem : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityActiveCheck", ent, player, id, debugName);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (ent.net.ID == player.net.ID)
				{
					return true;
				}
				if (ent.parentEntity.uid != player.net.ID)
				{
					return false;
				}
				Item activeItem = player.GetActiveItem();
				if (activeItem == null)
				{
					return false;
				}
				if (activeItem.GetHeldEntity() != ent)
				{
					return false;
				}
				return true;
			}
		}

		public class FromMounted : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				BaseMountable baseMountable = ent as BaseMountable;
				if (baseMountable == null)
				{
					baseMountable = ent.parentEntity.Get(serverside: true) as BaseMountable;
				}
				if (baseMountable != null && baseMountable.GetMounted()?.net?.ID == player.net.ID)
				{
					return true;
				}
				return false;
			}
		}

		public class FromOwnerOrMounted : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (FromOwner.Test(id, debugName, ent, player))
				{
					return true;
				}
				return FromMounted.Test(id, debugName, ent, player);
			}
		}

		public class InputValidation : Conditional
		{
			private Type[] validTypes;

			public Type[] ValidTypes => validTypes;

			public static bool Test(float f)
			{
				return !f.IsNaNOrInfinity();
			}

			public static bool Test(Vector3 v)
			{
				if (Test(v.x) && Test(v.y))
				{
					return Test(v.z);
				}
				return false;
			}

			public static bool Test(Vector2 v)
			{
				if (Test(v.x))
				{
					return Test(v.y);
				}
				return false;
			}

			public static bool Test(Quaternion q)
			{
				if (Test(q.x) && Test(q.y) && Test(q.z))
				{
					return Test(q.w);
				}
				return false;
			}

			public InputValidation(params Type[] types)
			{
				validTypes = types;
			}
		}

		public class MaxRepeatedElements : Attribute
		{
			public int MaximumElements { get; }

			public MaxRepeatedElements(int maximumElements)
			{
				MaximumElements = maximumElements;
			}
		}

		public class IgnoreConditional : Attribute
		{
			public string functionName;

			public Type[] ignoredAttributeTypes;

			public IgnoreConditional(string testFunc, params Type[] ignoredAttributes)
			{
				functionName = testFunc;
				ignoredAttributeTypes = ignoredAttributes;
			}
		}

		public class IgnoreProtoFieldOrder : Attribute
		{
		}

		public class IgnoreProtoFieldOperationLimit : Attribute
		{
		}

		public class CallsPerSecond : Conditional
		{
			private ulong callsPerSecond;

			public CallsPerSecond(ulong limit)
			{
				callsPerSecond = limit;
			}

			public override string GetArgs()
			{
				return callsPerSecond.ToString();
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, ulong callsPerSecond)
			{
				if (ent == null || player == null)
				{
					return false;
				}
				return player.rpcHistory.TryIncrement(id, callsPerSecond);
			}
		}
	}

	public struct BaseEntityPreserveInfo
	{
		public Vector3 localPosition;

		public Quaternion localRotation;

		public BaseEntity parent;

		public EntityRef[] slots;

		public ulong ownerID;

		public List<ChildPreserveInfo> childPreserveInfos;
	}

	public struct ChildPreserveInfo
	{
		public BaseEntity targetEntity;

		public uint targetBone;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public string targetSocketName;
	}

	public enum Signal
	{
		Attack,
		Alt_Attack,
		DryFire,
		Reload,
		Deploy,
		Flinch_Head,
		Flinch_Chest,
		Flinch_Stomach,
		Flinch_RearHead,
		Flinch_RearTorso,
		Throw,
		Relax,
		Gesture,
		PhysImpact,
		Eat,
		Startled,
		Admire
	}

	public enum Slot
	{
		Lock,
		FireMod,
		UpperModifier,
		MiddleModifier,
		LowerModifier,
		CenterDecoration,
		LowerCenterDecoration,
		StorageMonitor,
		Count
	}

	[Flags]
	public enum TraitFlag
	{
		None = 0,
		Alive = 1,
		Animal = 2,
		Human = 4,
		Interesting = 8,
		Food = 0x10,
		Meat = 0x20,
		Water = 0x20
	}

	public static class Util
	{
		public static BaseEntity[] FindTargets(string strFilter, bool onlyPlayers)
		{
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BasePlayer)
					{
						BasePlayer basePlayer = x as BasePlayer;
						if (string.IsNullOrEmpty(strFilter))
						{
							return true;
						}
						if (strFilter == "!alive" && basePlayer.IsAlive())
						{
							return true;
						}
						if (strFilter == "!sleeping" && basePlayer.IsSleeping())
						{
							return true;
						}
						if (strFilter[0] != '!' && !basePlayer.displayName.Contains(strFilter, CompareOptions.IgnoreCase) && !basePlayer.UserIDString.Contains(strFilter))
						{
							return false;
						}
						return true;
					}
					if (onlyPlayers)
					{
						return false;
					}
					if (string.IsNullOrEmpty(strFilter))
					{
						return false;
					}
					return x.ShortPrefabName.Contains(strFilter) ? true : false;
				})
				select x as BaseEntity).ToArray();
		}

		public static BaseEntity[] FindTargetsOwnedBy(ulong ownedBy, string strFilter)
		{
			bool hasFilter = !string.IsNullOrEmpty(strFilter);
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BaseEntity baseEntity)
					{
						if (baseEntity.OwnerID != ownedBy)
						{
							return false;
						}
						if (!hasFilter || baseEntity.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					return false;
				})
				select x as BaseEntity).ToArray();
		}

		public static BaseEntity[] FindTargetsAuthedTo(ulong authId, string strFilter)
		{
			bool hasFilter = !string.IsNullOrEmpty(strFilter);
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BuildingPrivlidge buildingPrivlidge)
					{
						if (!buildingPrivlidge.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is SimplePrivilege simplePrivilege)
					{
						if (!simplePrivilege.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is AutoTurret autoTurret)
					{
						if (!autoTurret.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is CodeLock codeLock)
					{
						if (!codeLock.whitelistPlayers.Contains(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is KeyLock keyLock)
					{
						if (keyLock.OwnerID != authId)
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is ModularCar modularCar)
					{
						if (!modularCar.IsLockable || !modularCar.CarLock.HasLockPermission(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					return false;
				})
				select x as BaseEntity).ToArray();
		}

		public static T[] FindAll<T>() where T : BaseEntity
		{
			return BaseNetworkable.serverEntities.OfType<T>().ToArray();
		}
	}

	[Flags]
	public enum Axis : byte
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		XY = 3,
		XZ = 5,
		YZ = 6,
		XYZ = 7
	}

	public enum GiveItemReason
	{
		Generic,
		ResourceHarvested,
		PickedUp,
		Crafted
	}

	private static Queue<BaseEntity> globalBroadcastQueue = new Queue<BaseEntity>();

	private static uint globalBroadcastProtocol = 0u;

	private uint broadcastProtocol;

	public List<EntityLink> links = new List<EntityLink>();

	private bool linkedToNeighbours;

	internal const int FileRequestMinimumCost = 32768;

	private TimeUntil _transferProtectionRemaining;

	private Action _disableTransferProtectionAction;

	private float cachedBuildingPrivilegeTime;

	private BuildingPrivlidge cachedBuildingPrivilege;

	private Vector3 cachedBuildingPrivilegePosition;

	public const string RpcClientDeprecationNotice = "Use ClientRPC( RpcTarget ) overloads";

	private static bool transferProtectedRpcsResolved;

	private static uint clientLoadingCompleteRpc;

	private static uint clientKeepConnectionAliveRpc;

	[NonSerialized]
	public BaseEntity creatorEntity;

	private bool couldSaveOriginally;

	public int ticksSinceStopped;

	public bool isCallingUpdateNetworkGroup;

	private Action _updateNetworkGroupCallback;

	private int oldPosLSFrame = int.MinValue;

	private Vector3 oldPosLS = Vector3.negativeInfinity;

	private Axis hasMovedLS;

	private const float EpsilonSqr = 9.9999994E-11f;

	private EntityRef[] entitySlots = new EntityRef[8];

	private const float SYNC_VAR_QUEUE_UPDATE_INTERVAL = 0.0333f;

	private const int SYNC_VAR_QUEUE_MAX_SIZE = 32;

	private uint _serverSyncVarQueue;

	private Action _sendPackedSyncVarQueueAction;

	protected List<TriggerBase> triggers;

	private Action _forceUpdateTriggersCallback;

	protected bool isVisible = true;

	protected bool isAnimatorVisible = true;

	protected bool isShadowVisible = true;

	protected OccludeeSphere localOccludee = new OccludeeSphere(-1);

	[Header("BaseEntity")]
	public Bounds bounds;

	public GameObjectRef impactEffect;

	public bool enableSaving = true;

	public bool syncPosition;

	public Model model;

	public Flags flags;

	[NonSerialized]
	public uint parentBone;

	[NonSerialized]
	public ulong skinID;

	[NonSerialized]
	public ulong attachmentID;

	private List<EntityComponentBase> _components;

	[HideInInspector]
	public bool HasBrain;

	private float nextHeightCheckTime;

	private bool cachedUnderground;

	[NonSerialized]
	public string _name;

	[NonSerialized]
	public bool networkEntityScale;

	public Spawnable _spawnable;

	protected static ExactArrayPool<byte> _autosaveBufferPool = new ExactArrayPool<byte>();

	protected byte[] _autosaveBuffer;

	public static HashSet<BaseEntity> saveList = new HashSet<BaseEntity>();

	public virtual float RealisticMass => 100f;

	protected float TransferProtectionRemaining => _transferProtectionRemaining;

	protected Action DisableTransferProtectionAction => _disableTransferProtectionAction ?? (_disableTransferProtectionAction = DisableTransferProtection);

	public virtual bool PreserveChildrenWhenReskinning => false;

	public float radiationLevel
	{
		get
		{
			if (triggers == null)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerRadiation triggerRadiation = triggers[i] as TriggerRadiation;
				if (!(triggerRadiation == null))
				{
					Vector3 position = GetNetworkPosition();
					BaseEntity baseEntity = GetParentEntity();
					if (baseEntity != null)
					{
						position = baseEntity.transform.TransformPoint(position);
					}
					num = Mathf.Max(num, triggerRadiation.GetRadiationForPosition(position, RadiationProtection(), this));
				}
			}
			return num;
		}
	}

	public float currentTemperature
	{
		get
		{
			float num = Climate.GetTemperature(base.transform.position);
			if (triggers == null)
			{
				return num;
			}
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerTemperature triggerTemperature = triggers[i] as TriggerTemperature;
				if (!(triggerTemperature == null))
				{
					num = triggerTemperature.WorkoutTemperature(base.transform.position, num);
				}
			}
			return num;
		}
	}

	public float currentEnvironmentalWetness
	{
		get
		{
			if (triggers == null)
			{
				return 0f;
			}
			float num = 0f;
			Vector3 networkPosition = GetNetworkPosition();
			foreach (TriggerBase trigger in triggers)
			{
				if (trigger is TriggerWetness triggerWetness)
				{
					num += triggerWetness.WorkoutWetness(networkPosition);
				}
			}
			return Mathf.Clamp01(num);
		}
	}

	public virtual float PositionTickRate => 0.1f;

	public virtual bool PositionTickFixedTime => false;

	public Action NetworkPosTickCallback { get; protected set; }

	public virtual Vector3 ServerPosition
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			if (!(base.transform.localPosition == value))
			{
				base.transform.localPosition = value;
				base.transform.hasChanged = true;
			}
		}
	}

	public virtual Vector3 ServerWorldPosition
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			if (!(base.transform.position == value))
			{
				base.transform.position = value;
				base.transform.hasChanged = true;
			}
		}
	}

	public virtual Quaternion ServerRotation
	{
		get
		{
			return base.transform.localRotation;
		}
		set
		{
			if (!(base.transform.localRotation == value))
			{
				base.transform.localRotation = value;
				base.transform.hasChanged = true;
			}
		}
	}

	public virtual Vector3 ServerNavMeshPos
	{
		get
		{
			return ServerWorldPosition;
		}
		set
		{
			ServerWorldPosition = value;
		}
	}

	public virtual Matrix4x4 WorldToNavMeshSpace => Matrix4x4.identity;

	public virtual Matrix4x4 NavMeshToWorldSpace => Matrix4x4.identity;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public virtual TraitFlag Traits => TraitFlag.None;

	public bool IsForceUpdatingTriggers { get; private set; }

	public float Weight { get; protected set; }

	public List<EntityComponentBase> Components
	{
		get
		{
			if (_components == null)
			{
				_components = new List<EntityComponentBase>();
				GetComponentsInChildren(includeInactive: true, _components);
			}
			return _components;
		}
	}

	public virtual bool IsNpc => false;

	public virtual bool AlsoVisCheckParent => false;

	public virtual bool VisibilityPassesThroughParent => false;

	public ulong OwnerID { get; set; }

	public virtual bool ShouldTransferAssociatedFiles => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseEntity.OnRpcMessage"))
		{
			if (rpc == 1552640099 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - BroadcastSignalFromClient");
				}
				using (TimeWarning.New("BroadcastSignalFromClient"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwnerOrMounted.Test(1552640099u, "BroadcastSignalFromClient", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							BroadcastSignalFromClient(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in BroadcastSignalFromClient");
					}
				}
				return true;
			}
			if (rpc == 3645147041u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_RequestFile");
				}
				using (TimeWarning.New("SV_RequestFile"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							SV_RequestFile(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SV_RequestFile");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		throw new NotImplementedException();
	}

	protected void ReceiveCollisionMessages(bool b)
	{
		if (b)
		{
			base.gameObject.transform.GetOrAddComponent<EntityCollisionMessage>();
		}
		else
		{
			UnityEngine.TransformEx.RemoveComponent<EntityCollisionMessage>(base.gameObject.transform);
		}
	}

	public virtual void DebugServer(int rep, float time)
	{
		DebugText(base.transform.position + Vector3.up * 1f, $"{net?.ID.Value ?? 0}: {base.name}\n{DebugText()}", Color.white, time);
	}

	public virtual string DebugText()
	{
		return "";
	}

	public void OnDebugStart()
	{
		EntityDebug entityDebug = base.gameObject.GetComponent<EntityDebug>();
		if (entityDebug == null)
		{
			entityDebug = base.gameObject.AddComponent<EntityDebug>();
		}
		entityDebug.enabled = true;
	}

	protected void DebugText(Vector3 pos, string str, Color color, float time)
	{
		if (base.isServer)
		{
			ConsoleNetwork.BroadcastToAllClients("ddraw.text", time, color, pos, str);
		}
	}

	public bool HasFlag(Flags f)
	{
		return (flags & f) == f;
	}

	public bool HasAnyFlag(Flags f)
	{
		return (flags & f) > (Flags)0;
	}

	public bool ParentHasFlag(Flags f)
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity == null)
		{
			return false;
		}
		return baseEntity.HasFlag(f);
	}

	public bool IsOn()
	{
		return HasFlag(Flags.On);
	}

	public bool IsOpen()
	{
		return HasFlag(Flags.Open);
	}

	public bool IsOnFire()
	{
		return HasFlag(Flags.OnFire);
	}

	public bool IsLocked()
	{
		return HasFlag(Flags.Locked);
	}

	public override bool IsDebugging()
	{
		return HasFlag(Flags.Debugging);
	}

	public bool IsDisabled()
	{
		if (!HasFlag(Flags.Disabled))
		{
			return ParentHasFlag(Flags.Disabled);
		}
		return true;
	}

	public bool IsBroken()
	{
		return HasFlag(Flags.Broken);
	}

	public bool IsBusy()
	{
		return HasFlag(Flags.Busy);
	}

	public bool IsTransferProtected()
	{
		return HasFlag(Flags.Protected);
	}

	public bool IsTransferring()
	{
		return HasFlag(Flags.Transferring);
	}

	public override string GetLogColor()
	{
		if (base.isServer)
		{
			return "cyan";
		}
		return "yellow";
	}

	public virtual void OnFlagsChanged(Flags old, Flags next)
	{
		if (IsDebugging() && (old & Flags.Debugging) != (next & Flags.Debugging))
		{
			OnDebugStart();
		}
		if (base.isServer)
		{
			if ((next & Flags.OnFire) == Flags.OnFire && (old & Flags.OnFire) != Flags.OnFire)
			{
				SingletonComponent<NpcFireManager>.Instance.Add(this);
			}
			else if ((next & Flags.OnFire) != Flags.OnFire && (old & Flags.OnFire) == Flags.OnFire)
			{
				SingletonComponent<NpcFireManager>.Instance.Remove(this);
			}
		}
	}

	public void SetFlagLocal(Flags f, bool b, bool recursive = false)
	{
		Flags old = flags;
		if (b)
		{
			if (HasFlag(f))
			{
				return;
			}
			flags |= f;
		}
		else
		{
			if (!HasFlag(f))
			{
				return;
			}
			flags &= ~f;
		}
		OnFlagsChanged(old, flags);
		InvalidateNetworkCache();
		if (recursive && children != null)
		{
			int i = 0;
			for (int count = children.Count; i < count; i++)
			{
				children[i].SetFlagLocal(f, b, recursive: true);
			}
		}
	}

	public FlagsUpdateScope StartSetFlags(FlagsUpdateMode updateMode)
	{
		return new FlagsUpdateScope(this, updateMode);
	}

	private void HandleFlagsUpdateMode(FlagsUpdateMode updateMode)
	{
		switch (updateMode)
		{
		case FlagsUpdateMode.Local:
			InvalidateNetworkCache();
			break;
		case FlagsUpdateMode.SendNetworkUpdate_Flags:
			InvalidateNetworkCache();
			SendNetworkUpdate_Flags();
			break;
		case FlagsUpdateMode.SendNetworkUpdate:
			SendNetworkUpdate();
			GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
			break;
		case FlagsUpdateMode.SendNetworkUpdateImmediate:
			SendNetworkUpdateImmediate();
			GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
			break;
		}
	}

	public void SendNetworkUpdate_Flags()
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave || base.IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate_Flags"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdate_Flags");
			if (Interface.CallHook("OnEntityFlagsNetworkUpdate", this) == null)
			{
				List<Connection> subscribers = GetSubscribers();
				if (subscribers != null && subscribers.Count > 0)
				{
					NetWrite netWrite = Network.Net.sv.StartWrite();
					netWrite.PacketID(Message.Type.EntityFlags);
					netWrite.EntityID(net.ID);
					netWrite.Int32((int)flags);
					SendInfo info = new SendInfo(subscribers);
					netWrite.Send(info);
				}
				base.gameObject.SendOnSendNetworkUpdate(this);
			}
		}
	}

	public virtual bool IsOccupied(Socket_Base socket)
	{
		return FindLink(socket)?.IsOccupied() ?? false;
	}

	public bool IsOccupied(string socketName)
	{
		return FindLink(socketName)?.IsOccupied() ?? false;
	}

	public EntityLink FindLink(Socket_Base socket)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			if (entityLinks[i].socket == socket)
			{
				return entityLinks[i];
			}
		}
		return null;
	}

	public EntityLink FindLink(string socketName)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			if (entityLinks[i].socket.socketName == socketName)
			{
				return entityLinks[i];
			}
		}
		return null;
	}

	public EntityLink FindLink(string[] socketNames)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			for (int j = 0; j < socketNames.Length; j++)
			{
				if (entityLinks[i].socket.socketName == socketNames[j])
				{
					return entityLinks[i];
				}
			}
		}
		return null;
	}

	public T FindLinkedEntity<T>() where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					return entityLink2.owner as T;
				}
			}
		}
		return null;
	}

	public void EntityLinkMessage<T>(Action<T> action) where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					action(entityLink2.owner as T);
				}
			}
		}
	}

	public void EntityLinkMessage<T, TCaller, TArg>(Action<T, TCaller, TArg> action, TCaller caller, TArg arg, bool onlyBuildingConnections = false) where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			if (onlyBuildingConnections && !entityLink.socket.ConnectsBuildings)
			{
				continue;
			}
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					action(entityLink2.owner as T, caller, arg);
				}
			}
		}
	}

	public void EntityLinkBroadcast<T, S>(Action<T> action, Func<S, bool> canTraverseSocket) where T : BaseEntity where S : Socket_Base
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				if (!(entityLink.socket is S) || !canTraverseSocket(entityLink.socket as S))
				{
					continue;
				}
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast<T>(Action<T> action) where T : BaseEntity
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast<T, TArg>(Action<T, TArg> action, TArg arg, bool onlyBuildingConnections = false) where T : BaseEntity
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T, arg);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				if (onlyBuildingConnections && !entityLink.socket.ConnectsBuildings)
				{
					continue;
				}
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T, arg);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast(bool onlyBuildingConnections = false)
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				if (onlyBuildingConnections && !entityLink.socket.ConnectsBuildings)
				{
					continue;
				}
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
					}
				}
			}
		}
	}

	public bool ReceivedEntityLinkBroadcast()
	{
		return broadcastProtocol == globalBroadcastProtocol;
	}

	public List<EntityLink> GetEntityLinks(bool linkToNeighbours = true)
	{
		if (Rust.Application.isLoadingSave)
		{
			return links;
		}
		if (!linkedToNeighbours && linkToNeighbours)
		{
			LinkToNeighbours();
		}
		return links;
	}

	private void LinkToEntity(BaseEntity other)
	{
		if (this == other || links.Count == 0 || other.links.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("LinkToEntity"))
		{
			for (int i = 0; i < links.Count; i++)
			{
				EntityLink entityLink = links[i];
				for (int j = 0; j < other.links.Count; j++)
				{
					EntityLink entityLink2 = other.links[j];
					if (entityLink.CanConnect(entityLink2))
					{
						if (!entityLink.Contains(entityLink2))
						{
							entityLink.Add(entityLink2);
						}
						if (!entityLink2.Contains(entityLink))
						{
							entityLink2.Add(entityLink);
						}
					}
				}
			}
		}
	}

	private void LinkToNeighbours()
	{
		if (links.Count == 0)
		{
			return;
		}
		linkedToNeighbours = true;
		using (TimeWarning.New("LinkToNeighbours"))
		{
			List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
			OBB oBB = WorldSpaceBounds();
			Vis.Entities(oBB.position, oBB.extents.magnitude + 1f, obj);
			for (int i = 0; i < obj.Count; i++)
			{
				BaseEntity baseEntity = obj[i];
				if (baseEntity.isServer == base.isServer)
				{
					LinkToEntity(baseEntity);
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
	}

	private void InitEntityLinks()
	{
		using (TimeWarning.New("InitEntityLinks"))
		{
			if (base.isServer)
			{
				links.AddLinks(this, PrefabAttribute.server.FindAll<Socket_Base>(prefabID));
			}
		}
	}

	private void FreeEntityLinks()
	{
		using (TimeWarning.New("FreeEntityLinks"))
		{
			links.FreeLinks();
			linkedToNeighbours = false;
		}
	}

	public void RefreshEntityLinks()
	{
		using (TimeWarning.New("RefreshEntityLinks"))
		{
			links.ClearLinks();
			LinkToNeighbours();
		}
	}

	public MovementModify GetMovementModify()
	{
		MovementModify result = default(MovementModify);
		result.drag = 0f;
		if (triggers == null)
		{
			return result;
		}
		foreach (TriggerBase trigger in triggers)
		{
			TriggerMovement triggerMovement = trigger as TriggerMovement;
			if (!(triggerMovement == null))
			{
				result.drag = Mathf.Max(triggerMovement.movementModify.drag * triggerMovement.GetMovementScale(), result.drag);
			}
		}
		return result;
	}

	[RPC_Server]
	public void SV_RequestFile(RPCMessage msg)
	{
		uint crc = msg.read.UInt32();
		FileStorage.Type type = (FileStorage.Type)msg.read.UInt8();
		string responseFunction = StringPool.Get(msg.read.UInt32());
		uint part = ((msg.read.Unread > 0) ? msg.read.UInt32() : 0u);
		bool respondIfNotFound = msg.read.Unread > 0 && msg.read.Bit();
		ServerFileRequestQueue.Request(msg.connection, this, ServerFileRequestQueue.RequestKind.GenericFile, crc, type, responseFunction, part, respondIfNotFound);
	}

	internal int SendRequestedFile(Connection connection, string funcName, uint crc, FileStorage.Type type, uint part, bool respondIfNotFound)
	{
		byte[] array = FileStorage.server.Get(crc, type, net.ID, part);
		if (array == null)
		{
			if (!respondIfNotFound)
			{
				return 0;
			}
			array = Array.Empty<byte>();
		}
		SendInfo sendInfo = new SendInfo(connection);
		sendInfo.channel = 2;
		sendInfo.method = SendMethod.Reliable;
		SendInfo sendInfo2 = sendInfo;
		ClientRPC(RpcTarget.SendInfo(funcName, sendInfo2), crc, (uint)array.Length, array, part, (byte)type);
		return array.Length;
	}

	public virtual void EnableTransferProtection()
	{
		if (!IsTransferProtected())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Protected, b: true);
			}
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null)
			{
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
			}
			float protectionDuration = Nexus.protectionDuration;
			_transferProtectionRemaining = protectionDuration;
			Invoke(DisableTransferProtectionAction, protectionDuration);
		}
		foreach (BaseEntity child in children)
		{
			child.EnableTransferProtection();
		}
	}

	public virtual void DisableTransferProtection()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity != null && baseEntity.IsTransferProtected())
		{
			baseEntity.DisableTransferProtection();
		}
		if (IsTransferProtected())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Protected, b: false);
			}
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null)
			{
				OnNetworkSubscribersEnter(subscribers);
			}
			_transferProtectionRemaining = 0f;
			CancelInvoke(DisableTransferProtectionAction);
		}
		foreach (BaseEntity child in children)
		{
			child.DisableTransferProtection();
		}
	}

	public void SetParent(BaseEntity entity, bool worldPositionStays = false, bool sendImmediate = false)
	{
		SetParent(entity, 0u, worldPositionStays, sendImmediate);
	}

	public void SetParent(BaseEntity entity, string strBone, bool worldPositionStays = false, bool sendImmediate = false)
	{
		SetParent(entity, (!string.IsNullOrEmpty(strBone)) ? StringPool.Get(strBone) : 0u, worldPositionStays, sendImmediate);
	}

	public bool HasChild(BaseEntity c)
	{
		if (c == this)
		{
			return true;
		}
		BaseEntity baseEntity = c.GetParentEntity();
		if (baseEntity != null)
		{
			return HasChild(baseEntity);
		}
		return false;
	}

	public void SetParent(BaseEntity entity, uint boneID, bool worldPositionStays = false, bool sendImmediate = false)
	{
		if (entity != null)
		{
			if (entity == this)
			{
				Debug.LogError("Trying to parent to self " + this, base.gameObject);
				return;
			}
			if (HasChild(entity))
			{
				Debug.LogError("Trying to parent to child " + this, base.gameObject);
				return;
			}
		}
		LogEntry(RustLog.EntryType.Hierarchy, 2, "SetParent {0} {1}", entity, boneID);
		BaseEntity baseEntity = GetParentEntity();
		if ((bool)baseEntity)
		{
			baseEntity.RemoveChild(this);
		}
		if (base.limitNetworking && baseEntity != null && baseEntity != entity)
		{
			BasePlayer basePlayer = baseEntity as BasePlayer;
			if (basePlayer.IsValid())
			{
				DestroyOnClient(basePlayer.net.connection);
			}
		}
		if (entity == null)
		{
			OnParentChanging(baseEntity, null);
			parentEntity.Set(null);
			base.transform.SetParent(null, worldPositionStays);
			parentBone = 0u;
			UpdateNetworkGroup();
			if (sendImmediate)
			{
				SendNetworkUpdateImmediate();
				SendChildrenNetworkUpdateImmediate();
			}
			else
			{
				SendNetworkUpdate();
				SendChildrenNetworkUpdate();
			}
			return;
		}
		Debug.Assert(entity.isServer, "SetParent - child should be a SERVER entity");
		Debug.Assert(entity.net != null, "Setting parent to entity that hasn't spawned yet! (net is null)");
		Debug.Assert(entity.net.ID.IsValid, "Setting parent to entity that hasn't spawned yet! (id = 0)");
		entity.AddChild(this);
		OnParentChanging(baseEntity, entity);
		parentEntity.Set(entity);
		if (boneID != 0 && boneID != StringPool.closest)
		{
			Transform transform = entity.FindBone(StringPool.Get(boneID));
			if (transform != null && transform.TryGetComponent<ReparentToTargetBone>(out var component) && component.TargetBone != null)
			{
				uint num = StringPool.Get(component.TargetBone.name);
				if (num != 0)
				{
					boneID = num;
					transform = component.TargetBone;
				}
			}
			base.transform.SetParent((transform != null) ? transform : entity.transform, worldPositionStays);
		}
		else
		{
			base.transform.SetParent(entity.transform, worldPositionStays);
		}
		parentBone = boneID;
		UpdateNetworkGroup();
		if (sendImmediate)
		{
			SendNetworkUpdateImmediate();
			SendChildrenNetworkUpdateImmediate();
		}
		else
		{
			SendNetworkUpdate();
			SendChildrenNetworkUpdate();
		}
	}

	public void DestroyOnClient(Connection connection)
	{
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				child.DestroyOnClient(connection);
			}
		}
		if (Network.Net.sv.IsConnected())
		{
			if (net.connection == connection && this is BasePlayer)
			{
				Debug.LogError("Attempted to send EntityDestroy to connection's local player, this would cause chaos on the client. Skipping message");
				return;
			}
			NetWrite netWrite = Network.Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityDestroy);
			netWrite.EntityID(net.ID);
			netWrite.UInt8(0);
			netWrite.Send(new SendInfo(connection));
			LogEntry(RustLog.EntryType.Network, 2, "EntityDestroy");
		}
	}

	public void SendChildrenNetworkUpdate()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.UpdateNetworkGroup();
			child.SendNetworkUpdate();
		}
	}

	public void SendChildrenNetworkUpdateImmediate()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.UpdateNetworkGroup();
			child.SendNetworkUpdateImmediate();
		}
	}

	public virtual void SwitchParent(BaseEntity ent)
	{
		Log("SwitchParent Missed " + ent);
	}

	public virtual void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		if (!TryGetComponent<Rigidbody>(out var component) || !component || component.isKinematic)
		{
			return;
		}
		if (oldParent != null)
		{
			Rigidbody component2 = oldParent.GetComponent<Rigidbody>();
			if (component2 == null || component2.isKinematic)
			{
				component.linearVelocity += oldParent.GetWorldVelocity();
			}
		}
		if (newParent != null)
		{
			Rigidbody component3 = newParent.GetComponent<Rigidbody>();
			if (component3 == null || component3.isKinematic)
			{
				component.linearVelocity -= newParent.GetWorldVelocity();
			}
		}
	}

	protected bool PrivilegeCacheDefaultValue()
	{
		return base.isClient;
	}

	protected static bool IsCacheValid(float cacheTime, float cacheDuration)
	{
		if (cacheTime != 0f)
		{
			return UnityEngine.Time.time - cacheTime < cacheDuration;
		}
		return false;
	}

	protected static bool IsCacheValid(float cacheTime, float cacheDuration, Vector3 cachedPosition, Vector3 queryPosition)
	{
		if (IsCacheValid(cacheTime, cacheDuration))
		{
			return cachedPosition == queryPosition;
		}
		return false;
	}

	public virtual EntityPrivilege GetEntityBuildingPrivilege()
	{
		return null;
	}

	public virtual BuildingPrivlidge GetBuildingPrivilege()
	{
		return GetNearestBuildingPrivilege(PrivilegeCacheDefaultValue());
	}

	public virtual BuildingPrivlidge GetBuildingPrivilege(bool cached, float cacheDuration = 1f)
	{
		return GetNearestBuildingPrivilege(cached, cacheDuration);
	}

	public BuildingPrivlidge GetNearestBuildingPrivilege()
	{
		return GetBuildingPrivilege(PrivilegeCacheDefaultValue());
	}

	public BuildingPrivlidge GetNearestBuildingPrivilege(bool cached, float cacheDuration = 1f)
	{
		return GetBuildingPrivilege(WorldSpaceBounds(), cached, cacheDuration);
	}

	public BuildingPrivlidge GetBuildingPrivilege(OBB obb)
	{
		return GetBuildingPrivilege(obb, PrivilegeCacheDefaultValue());
	}

	public BuildingPrivlidge GetBuildingPrivilege(OBB obb, bool cached, float cacheDuration = 1f, BuildingPrivlidge exclude = null)
	{
		object obj = Interface.CallHook("OnBuildingPrivilege", this, obb, cached, cacheDuration, exclude);
		if (obj is BuildingPrivlidge)
		{
			return (BuildingPrivlidge)obj;
		}
		if (cached && IsCacheValid(cachedBuildingPrivilegeTime, cacheDuration, cachedBuildingPrivilegePosition, obb.position))
		{
			return cachedBuildingPrivilege;
		}
		BuildingBlock other = null;
		BuildingPrivlidge buildingPrivlidge = null;
		List<BuildingBlock> obj2 = Facepunch.Pool.Get<List<BuildingBlock>>();
		Vis.Entities(obb.position, 16f + obb.extents.magnitude, obj2, 2097152);
		uint num = ((exclude != null) ? exclude.buildingID : 0u);
		for (int i = 0; i < obj2.Count; i++)
		{
			BuildingBlock buildingBlock = obj2[i];
			if (buildingBlock.isServer != base.isServer || !buildingBlock.IsOlderThan(other) || obb.Distance(buildingBlock.WorldSpaceBounds()) > 16f)
			{
				continue;
			}
			BuildingManager.Building building = buildingBlock.GetBuilding();
			if (building != null && (num == 0 || num != building.ID))
			{
				BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
				if (!(dominatingBuildingPrivilege == null))
				{
					other = buildingBlock;
					buildingPrivlidge = dominatingBuildingPrivilege;
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		using (TimeWarning.New("InvisibleTC"))
		{
			if (BaseGameMode.GetActiveGameMode(base.isServer) is GameModeSoftcore && StorageContainer.dropCorpseOnDeath)
			{
				using PooledList<BuildingPrivlidge> pooledList = Facepunch.Pool.Get<PooledList<BuildingPrivlidge>>();
				BuildingPrivlidge.InvisibleAuthGrid.Query(obb.position.x, obb.position.z, 16f + obb.extents.magnitude, pooledList);
				foreach (BuildingPrivlidge item in pooledList)
				{
					if (!(item == null) && item.isServer == base.isServer && (!(exclude != null) || !(item == exclude)) && item.Distance(obb.position) < 16f && (buildingPrivlidge == null || item.IsOlderThan(buildingPrivlidge)))
					{
						buildingPrivlidge = item;
					}
				}
			}
		}
		cachedBuildingPrivilegeTime = UnityEngine.Time.time;
		cachedBuildingPrivilege = buildingPrivlidge;
		cachedBuildingPrivilegePosition = obb.position;
		return cachedBuildingPrivilege;
	}

	public void SV_RPCMessage(uint nameID, Message message)
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		BasePlayer basePlayer = NetworkPacketEx.Player(message);
		if (!basePlayer.IsValid())
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("SV_RPCMessage: From invalid player " + basePlayer);
			}
			return;
		}
		if (ConVar.AntiHack.rpcstallmode > 0 && basePlayer.isStalled)
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("SV_RPCMessage: player is stalled " + basePlayer);
			}
			return;
		}
		if (ConVar.AntiHack.rpcstallmode > 1 && basePlayer.wasStalled)
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("SV_RPCMessage: player was stalled " + basePlayer);
			}
			return;
		}
		if (basePlayer.IsTransferProtected() && !IsAllowedWhileTransferProtected(nameID))
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("SV_RPCMessage: player is transfer protected " + basePlayer);
			}
			return;
		}
		using (message.read.UseProtoDeserializationLimits())
		{
			(byte[], int) buffer = message.read.GetBuffer();
			if (OnRpcMessage(basePlayer, nameID, message))
			{
				if (!basePlayer.IsRealNull() && Facepunch.Rust.Analytics.Azure.ShouldLogRPC(StringPool.Get(nameID)))
				{
					Facepunch.Rust.Analytics.Azure.OnServerRPC(basePlayer, nameID, buffer.Item1, buffer.Item2);
				}
				return;
			}
			for (int i = 0; i < Components.Count; i++)
			{
				if (Components[i].OnRpcMessage(basePlayer, nameID, message))
				{
					if (!basePlayer.IsRealNull())
					{
						Facepunch.Rust.Analytics.Azure.OnServerRPC(basePlayer, nameID, buffer.Item1, buffer.Item2);
					}
					break;
				}
			}
		}
	}

	private static bool IsAllowedWhileTransferProtected(uint nameID)
	{
		if (!transferProtectedRpcsResolved)
		{
			clientLoadingCompleteRpc = StringPool.Get("ClientLoadingComplete");
			clientKeepConnectionAliveRpc = StringPool.Get("ClientKeepConnectionAlive");
			transferProtectedRpcsResolved = true;
			if (clientLoadingCompleteRpc == 0)
			{
				Debug.LogError("Couldn't resolve the ClientLoadingComplete RPC id - transfer protection will never be released!");
			}
		}
		if (nameID != clientLoadingCompleteRpc || nameID == 0)
		{
			if (nameID == clientKeepConnectionAliveRpc)
			{
				return nameID != 0;
			}
			return false;
		}
		return true;
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite write = ClientRPCStart(target.Function);
			ClientRPCSend(write, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MemoryStream stream)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			using (TimeWarning.New("Copy Buffer"))
			{
				netWrite.Write(stream.GetBuffer(), 0, (int)stream.Length);
			}
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetRpcTargetNetworkGroup(ref RpcTarget target)
	{
		if (target.ToNetworkGroup)
		{
			target.Connections = new SendInfo(net.group.subscribers);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void FreeRPCTarget(RpcTarget target)
	{
		if (target.UsingPooledConnections)
		{
			Facepunch.Pool.FreeUnmanaged(ref target.Connections.connections);
		}
	}

	protected NetWrite ClientRPCStart(string funcName)
	{
		NetWrite netWrite = Network.Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.RPCMessage);
		netWrite.EntityID(net.ID);
		netWrite.UInt32(StringPool.Get(funcName));
		return netWrite;
	}

	private void ClientRPCWrite<T>(NetWrite write, T arg)
	{
		NetworkWriteEx.WriteObject(write, arg);
	}

	protected void ClientRPCSend(NetWrite write, SendInfo sendInfo)
	{
		write.Send(sendInfo);
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPCList<T1>(RpcTarget target, List<T1> list)
	{
		if (!Network.Net.sv.IsConnected() || net == null)
		{
			return;
		}
		NetWrite netWrite = ClientRPCStart(target.Function);
		netWrite.Int32(list.Count);
		foreach (T1 item in list)
		{
			ClientRPCWrite(netWrite, item);
		}
		ClientRPCSend(netWrite, target.Connections);
	}

	public virtual bool CanBeReskinned(BasePlayer player)
	{
		return true;
	}

	public virtual bool CanBeRedirectSwapped(BasePlayer player)
	{
		return true;
	}

	public virtual void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		ref BaseEntityPreserveInfo baseEntityPreserve = ref preserveInfo.baseEntityPreserve;
		baseEntityPreserve.localPosition = base.transform.localPosition;
		baseEntityPreserve.localRotation = base.transform.localRotation;
		baseEntityPreserve.parent = GetParentEntity();
		baseEntityPreserve.slots = GetSlots();
		baseEntityPreserve.ownerID = OwnerID;
		if (this is IItemContainerEntity itemContainerEntity)
		{
			itemContainerEntity.Reskin_Preserve_Container(ref preserveInfo.containerPreserve, this, -1);
		}
		if (PreserveChildrenWhenReskinning)
		{
			Reskin_Preserve_Children(ref baseEntityPreserve);
			return;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is IItemContainerEntity itemContainerEntity2)
			{
				itemContainerEntity2.Reskin_Preserve_Container(ref preserveInfo.containerPreserve, children[i], i);
			}
		}
	}

	private void Reskin_Preserve_Children(ref BaseEntityPreserveInfo preserve)
	{
		preserve.childPreserveInfos = Facepunch.Pool.Get<List<ChildPreserveInfo>>();
		foreach (BaseEntity child in children)
		{
			ChildPreserveInfo childPreserveInfo = default(ChildPreserveInfo);
			childPreserveInfo.targetEntity = child;
			childPreserveInfo.targetBone = child.parentBone;
			childPreserveInfo.localPosition = child.transform.localPosition;
			childPreserveInfo.localRotation = child.transform.localRotation;
			ChildPreserveInfo item = childPreserveInfo;
			Socket_Specific socket_Specific = PrefabAttribute.server.Find<Socket_Base>(child.prefabID) as Socket_Specific;
			if (socket_Specific != null)
			{
				Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
				Socket_Specific_Female socket_Specific_Female = null;
				float num = float.MaxValue;
				Socket_Base[] array2 = array;
				foreach (Socket_Base socket_Base in array2)
				{
					if (socket_Base is Socket_Specific_Female socket_Specific_Female2 && Array.IndexOf(socket_Specific_Female2.allowedMaleSockets, socket_Specific.targetSocketName) >= 0)
					{
						Vector3 b = base.transform.position + base.transform.rotation * socket_Base.worldPosition;
						float num2 = Vector3.Distance(child.transform.position, b);
						if (num2 < num)
						{
							socket_Specific_Female = socket_Specific_Female2;
							num = num2;
						}
					}
				}
				if (socket_Specific_Female != null)
				{
					item.targetSocketName = socket_Specific_Female.socketName;
				}
			}
			preserve.childPreserveInfos.Add(item);
		}
		foreach (ChildPreserveInfo childPreserveInfo2 in preserve.childPreserveInfos)
		{
			childPreserveInfo2.targetEntity.SetParent(null, worldPositionStays: true);
		}
	}

	public virtual void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		ref BaseEntityPreserveInfo baseEntityPreserve = ref preserveInfo.baseEntityPreserve;
		base.transform.position = baseEntityPreserve.localPosition;
		base.transform.rotation = baseEntityPreserve.localRotation;
		SetParent(baseEntityPreserve.parent);
		SetSlots(baseEntityPreserve.slots);
		OwnerID = baseEntityPreserve.ownerID;
		if (this is IItemContainerEntity itemContainerEntity)
		{
			itemContainerEntity.Reskin_Restore_Container(ref preserveInfo.containerPreserve, this, -1);
		}
		if (PreserveChildrenWhenReskinning)
		{
			Reskin_Restore_Children(ref baseEntityPreserve);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] is IItemContainerEntity itemContainerEntity2)
				{
					itemContainerEntity2.Reskin_Restore_Container(ref preserveInfo.containerPreserve, children[i], i);
				}
			}
		}
		if (preserveInfo.containerPreserve.storageDict != null)
		{
			DropLeftoverItems(ref preserveInfo.containerPreserve);
			Debug.LogError("Was unable to cleanly transfer some items when reskinning to " + base.ShortPrefabName + ", dropped them instead");
		}
	}

	private void Reskin_Restore_Children(ref BaseEntityPreserveInfo preserve)
	{
		HashSet<IOEntity> obj = Facepunch.Pool.Get<HashSet<IOEntity>>();
		foreach (ChildPreserveInfo childPreserveInfo in preserve.childPreserveInfos)
		{
			childPreserveInfo.targetEntity.SetParent(this, childPreserveInfo.targetBone, worldPositionStays: true);
			bool flag = false;
			if (ConVar.Server.repositionAttachmentsOnReskin)
			{
				flag = TryRepositionChildEntity(childPreserveInfo.targetEntity, childPreserveInfo.targetSocketName, obj);
			}
			if (!flag)
			{
				childPreserveInfo.targetEntity.transform.localPosition = childPreserveInfo.localPosition;
				childPreserveInfo.targetEntity.transform.localRotation = childPreserveInfo.localRotation;
			}
		}
		foreach (IOEntity item in obj)
		{
			item.SendNetworkUpdateImmediate();
			item.NotifyClientsLineChanged();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref preserve.childPreserveInfos);
	}

	private void DropLeftoverItems(ref IItemContainerEntity.ContainerPreserveInfo preserve)
	{
		foreach (KeyValuePair<IItemContainerEntity.ContainerSet, List<Item>> item in preserve.storageDict)
		{
			foreach (Item item2 in item.Value)
			{
				item2.DropAndTossUpwards(base.transform.position + Vector3.up * 0.5f);
			}
			List<Item> obj = item.Value;
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		Facepunch.Pool.FreeUnmanaged(ref preserve.storageDict);
	}

	private bool TryRepositionChildEntity(BaseEntity childEntity, string targetSocketName, HashSet<IOEntity> toUpdate)
	{
		Socket_Specific socket_Specific = PrefabAttribute.server.Find<Socket_Base>(childEntity.prefabID) as Socket_Specific;
		if (socket_Specific == null)
		{
			return false;
		}
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
		Socket_Specific_Female socket_Specific_Female = null;
		float num = float.MaxValue;
		Socket_Base[] array2 = array;
		foreach (Socket_Base socket_Base in array2)
		{
			if (socket_Base is Socket_Specific_Female socket_Specific_Female2 && Array.IndexOf(socket_Specific_Female2.allowedMaleSockets, socket_Specific.targetSocketName) >= 0)
			{
				if (StringEx.EqualsAfterLastSeparator(targetSocketName, socket_Base.socketName, '/'))
				{
					socket_Specific_Female = socket_Specific_Female2;
					break;
				}
				Vector3 b = base.transform.position + base.transform.rotation * socket_Base.worldPosition;
				float num2 = Vector3.Distance(childEntity.transform.position, b);
				if (num2 < num)
				{
					socket_Specific_Female = socket_Specific_Female2;
					num = num2;
				}
			}
		}
		if (socket_Specific_Female == null)
		{
			return false;
		}
		Matrix4x4 localToWorldMatrix = childEntity.transform.localToWorldMatrix;
		childEntity.transform.SetLocalPositionAndRotation(socket_Specific_Female.localPosition, socket_Specific_Female.localRotation);
		Matrix4x4 matrix4x = childEntity.transform.worldToLocalMatrix * localToWorldMatrix;
		if (childEntity is IOEntity { outputs: var outputs } iOEntity)
		{
			foreach (IOEntity.IOSlot iOSlot in outputs)
			{
				if (iOSlot.IsConnected() && iOSlot.linePoints != null && iOSlot.linePoints.Length != 0)
				{
					for (int j = 0; j < iOSlot.linePoints.Length - 1; j++)
					{
						iOSlot.linePoints[j] = matrix4x.MultiplyPoint3x4(iOSlot.linePoints[j]);
					}
					toUpdate.Add(iOEntity);
				}
			}
			iOEntity.SnapLinesToHandlePositions(toUpdate);
		}
		return true;
	}

	public virtual float RadiationProtection()
	{
		return 0f;
	}

	public virtual float RadiationExposureFraction()
	{
		return 1f;
	}

	public virtual void SetCreatorEntity(BaseEntity newCreatorEntity)
	{
		creatorEntity = newCreatorEntity;
	}

	public virtual Vector3 GetLocalVelocityServer()
	{
		return Vector3.zero;
	}

	public virtual Quaternion GetAngularVelocityServer()
	{
		return Quaternion.identity;
	}

	public void EnableGlobalBroadcast(bool wants)
	{
		if (globalBroadcast != wants)
		{
			globalBroadcast = wants;
			UpdateNetworkGroup();
		}
	}

	public void EnableSaving(bool wants)
	{
		if (enableSaving != wants)
		{
			enableSaving = wants;
			if (enableSaving)
			{
				saveList.Add(this);
			}
			else
			{
				saveList.Remove(this);
			}
		}
	}

	public void RestoreCanSave()
	{
		EnableSaving(couldSaveOriginally);
	}

	public override void ServerInit()
	{
		_spawnable = GetComponent<Spawnable>();
		base.ServerInit();
		if (base.isServer)
		{
			couldSaveOriginally = enableSaving;
			if (enableSaving)
			{
				saveList.Add(this);
			}
			if (flags != 0)
			{
				OnFlagsChanged((Flags)0, flags);
			}
			if (syncPosition && PositionTickRate >= 0f)
			{
				syncPosition = false;
				ToggleNetworkPositionTick(isEnabled: true);
			}
			if (Query.Server != null)
			{
				Query.Server.Add(this);
			}
			if (this is SamSite.ISamSiteTarget item)
			{
				SamSite.ISamSiteTarget.serverList.Add(item);
			}
			if (this is IPowergridEntity powergridEntity)
			{
				PowergridManager.Server_AddPowergridEntity(powergridEntity);
			}
			if (Rust.Application.isServerStarted)
			{
				Facepunch.Rust.Analytics.Azure.OnEntitySpawned(this);
			}
		}
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!(Components[i] == null))
			{
				Components[i].ServerInitPostNetworkGroupAssign();
			}
		}
	}

	public virtual void OnPlaced(BasePlayer player)
	{
	}

	protected virtual bool ShouldUpdateNetworkGroup()
	{
		return syncPosition;
	}

	protected virtual bool ShouldUpdateNetworkPosition()
	{
		return syncPosition;
	}

	protected void ToggleNetworkPositionTick(bool isEnabled)
	{
		if (syncPosition == isEnabled)
		{
			return;
		}
		syncPosition = isEnabled;
		if (syncPosition)
		{
			if (NetworkPosTickCallback == null)
			{
				Action action2 = (NetworkPosTickCallback = NetworkPositionTick);
			}
			if (PositionTickFixedTime)
			{
				InvokeRepeatingFixedTime(NetworkPosTickCallback);
			}
			else
			{
				InvokeRandomized(NetworkPosTickCallback, PositionTickRate, PositionTickRate - PositionTickRate * 0.05f, PositionTickRate * 0.05f);
			}
		}
		else if (PositionTickFixedTime)
		{
			CancelInvokeFixedTime(NetworkPosTickCallback);
		}
		else
		{
			CancelInvoke(NetworkPosTickCallback);
		}
	}

	public void NetworkPositionTick()
	{
		if (!base.transform.hasChanged)
		{
			if (ticksSinceStopped >= 6)
			{
				return;
			}
			ticksSinceStopped++;
		}
		else
		{
			ticksSinceStopped = 0;
		}
		TransformChanged();
		base.transform.hasChanged = false;
	}

	protected virtual void TransformChanged()
	{
		if (Query.Server != null)
		{
			Query.Server.Move(this);
		}
		SingletonComponent<NpcFireManager>.Instance.Move(this);
		if (net == null)
		{
			return;
		}
		InvalidateNetworkCache();
		if (!globalBroadcast && !ValidBounds.Test(this, base.transform.position))
		{
			OnInvalidPosition();
			return;
		}
		TryScheduleUpdateNetworkGroup();
		if (ShouldUpdateNetworkPosition())
		{
			SendNetworkUpdate_Position();
			OnPositionalNetworkUpdate();
		}
	}

	protected void TryScheduleUpdateNetworkGroup()
	{
		if (ShouldUpdateNetworkGroup() && !isCallingUpdateNetworkGroup)
		{
			if (_updateNetworkGroupCallback == null)
			{
				_updateNetworkGroupCallback = UpdateNetworkGroup;
			}
			Invoke(_updateNetworkGroupCallback, 5f);
			isCallingUpdateNetworkGroup = true;
		}
	}

	public virtual void OnPositionalNetworkUpdate()
	{
	}

	public override void Spawn()
	{
		base.Spawn();
		if (base.isServer)
		{
			OnParentSpawningEx.BroadcastOnParentSpawning(base.gameObject);
		}
		for (int i = 0; i < entitySlots.Length; i++)
		{
			entitySlots[i] = default(EntityRef);
		}
	}

	public void OnParentSpawning()
	{
		if (net != null || base.IsDestroyed)
		{
			return;
		}
		if (Rust.Application.isLoadingSave)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (GameManager.server.preProcessed.NeedsProcessing(base.gameObject, PreProcessPrefabOptions.Default_NoResetPosition))
		{
			GameManager.server.preProcessed.ProcessObject(null, base.gameObject, PreProcessPrefabOptions.Default_NoResetPosition);
		}
		BaseEntity baseEntity = ((base.transform.parent != null) ? base.transform.parent.GetComponentInParent<BaseEntity>() : null);
		Spawn();
		if (baseEntity != null)
		{
			SetParent(baseEntity, worldPositionStays: true);
		}
	}

	public void SpawnAsMapEntity()
	{
		if (net == null && !base.IsDestroyed && ((base.transform.parent != null) ? base.transform.parent.GetComponentInParent<BaseEntity>() : null) == null)
		{
			if (GameManager.server.preProcessed.NeedsProcessing(base.gameObject, PreProcessPrefabOptions.Default_NoResetPosition))
			{
				GameManager.server.preProcessed.ProcessObject(null, base.gameObject, PreProcessPrefabOptions.Default_NoResetPosition);
			}
			base.transform.parent = null;
			SceneManager.MoveGameObjectToScene(base.gameObject, Rust.Server.EntityScene);
			base.gameObject.SetActive(value: true);
			Spawn();
		}
	}

	public virtual void PostMapEntitySpawn()
	{
	}

	internal override void DoServerDestroy()
	{
		if (Rust.Application.isServerStarted)
		{
			Facepunch.Rust.Analytics.Azure.OnEntityDestroyed(this);
		}
		if (Query.Server != null)
		{
			Query.Server.Remove(this);
		}
		ToggleNetworkPositionTick(isEnabled: false);
		if (enableSaving)
		{
			saveList.Remove(this);
		}
		enableSaving = couldSaveOriginally;
		RemoveFromTriggers();
		if (children != null)
		{
			BaseEntity[] array = children.ToArray();
			foreach (BaseEntity baseEntity in array)
			{
				if (!(baseEntity == null))
				{
					baseEntity.OnParentRemoved();
				}
			}
		}
		SetParent(null, worldPositionStays: true);
		SingletonComponent<NpcFireManager>.Instance.Remove(this);
		if (this is SamSite.ISamSiteTarget item)
		{
			SamSite.ISamSiteTarget.serverList.Remove(item);
		}
		if (this is IPowergridEntity powergridEntity)
		{
			PowergridManager.Server_RemovePowergridEntity(powergridEntity);
		}
		base.DoServerDestroy();
	}

	internal virtual void OnParentRemoved()
	{
		Kill();
	}

	public virtual void OnInvalidPosition()
	{
		Debug.Log("Invalid Position: " + this?.ToString() + " " + base.transform.position.ToString() + " (destroying)");
		Kill();
	}

	public BaseCorpse DropCorpse(string strCorpsePrefab, BasePlayer.PlayerFlags playerFlagsOnDeath = (BasePlayer.PlayerFlags)0, ModelState modelState = null)
	{
		return DropCorpse(strCorpsePrefab, base.transform.position, base.transform.rotation, playerFlagsOnDeath, modelState);
	}

	public BaseCorpse DropCorpse(string strCorpsePrefab, Vector3 posOnDeath, Quaternion rotOnDeath, BasePlayer.PlayerFlags playerFlagsOnDeath = (BasePlayer.PlayerFlags)0, ModelState modelState = null)
	{
		Assert.IsTrue(base.isServer, "DropCorpse called on client!");
		if (!ConVar.Server.corpses)
		{
			return null;
		}
		if (string.IsNullOrEmpty(strCorpsePrefab))
		{
			return null;
		}
		BaseCorpse baseCorpse = GameManager.server.CreateEntity(strCorpsePrefab) as BaseCorpse;
		if (baseCorpse == null)
		{
			Debug.LogWarning("Error creating corpse: " + base.gameObject?.ToString() + " - " + strCorpsePrefab);
			return null;
		}
		baseCorpse.ServerInitCorpse(this, posOnDeath, rotOnDeath, playerFlagsOnDeath, modelState);
		return baseCorpse;
	}

	public override void UpdateNetworkGroup()
	{
		Assert.IsTrue(base.isServer, "UpdateNetworkGroup called on clientside entity!");
		isCallingUpdateNetworkGroup = false;
		if (net == null || Network.Net.sv == null || Network.Net.sv.visibility == null)
		{
			return;
		}
		using (TimeWarning.New("UpdateNetworkGroup"))
		{
			if (globalBroadcast)
			{
				Group globalNetworkGroup = BaseNetworkable.GetGlobalNetworkGroup(globalNetworkBehavior);
				if (net.SwitchGroup(globalNetworkGroup))
				{
					SendNetworkGroupChange();
				}
				return;
			}
			BaseEntity baseEntity = GetParentEntity();
			if (parentEntity.IsSet() && !baseEntity.IsValid() && ShouldInheritNetworkGroup())
			{
				if (!Rust.Application.isLoadingSave)
				{
					Debug.LogWarning("UpdateNetworkGroup: Missing parent entity " + parentEntity.uid.ToString());
					if (_updateNetworkGroupCallback == null)
					{
						_updateNetworkGroupCallback = UpdateNetworkGroup;
					}
					Invoke(_updateNetworkGroupCallback, 2f);
					isCallingUpdateNetworkGroup = true;
				}
			}
			else if (ShouldInheritNetworkGroup() && parentEntity.IsSet() && baseEntity.IsValid() && baseEntity.ShouldChildrenInheritNetworkGroup())
			{
				if (baseEntity != null)
				{
					if (net.SwitchGroup(baseEntity.net.group))
					{
						SendNetworkGroupChange();
					}
				}
				else
				{
					Debug.LogWarning(base.gameObject?.ToString() + ": has parent id - but couldn't find parent! " + parentEntity);
				}
			}
			else if (base.limitNetworking && !(this is BasePlayer))
			{
				if (net.SwitchGroup(BaseNetworkable.LimboNetworkGroup))
				{
					SendNetworkGroupChange();
				}
			}
			else
			{
				base.UpdateNetworkGroup();
			}
		}
	}

	public virtual void Eat(BaseNpc baseNpc, float timeSpent)
	{
		baseNpc.AddCalories(100f);
	}

	public virtual void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		List<EntityComponentBase> components = Components;
		int i = 0;
		for (int count = components.Count; i < count; i++)
		{
			components[i].OnEntityDeployed(parent, deployedBy, fromItem);
		}
	}

	public override bool ShouldNetworkTo(BasePlayer player)
	{
		if (player == this)
		{
			return true;
		}
		if (IsTransferProtected())
		{
			return false;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (base.limitNetworking)
		{
			if (baseEntity == null)
			{
				return false;
			}
			if (baseEntity != player)
			{
				return false;
			}
		}
		if (ShouldInheritNetworkGroup() && baseEntity != null && baseEntity.ShouldChildrenInheritNetworkGroup())
		{
			return baseEntity.ShouldNetworkTo(player);
		}
		return base.ShouldNetworkTo(player);
	}

	public virtual void AttackerInfo(PlayerLifeStory.DeathInfo info)
	{
		info.attackerName = base.ShortPrefabName;
		info.attackerSteamID = 0uL;
		info.inflictorName = "";
	}

	public virtual void Push(Vector3 velocity)
	{
		SetVelocity(velocity);
	}

	public virtual void ApplyInheritedVelocity(Vector3 velocity)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.linearVelocity = Vector3.Lerp(component.linearVelocity, velocity, 10f * UnityEngine.Time.fixedDeltaTime);
			component.angularVelocity *= Mathf.Clamp01(1f - 10f * UnityEngine.Time.fixedDeltaTime);
			component.AddForce(-UnityEngine.Physics.gravity * Mathf.Clamp01(0.9f), ForceMode.Acceleration);
		}
	}

	public virtual void SetVelocity(Vector3 velocity)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.linearVelocity = velocity;
		}
	}

	public virtual void SetAngularVelocity(Vector3 velocity)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			component.angularVelocity = velocity;
		}
	}

	public virtual Vector3 GetDropPosition()
	{
		return base.transform.position;
	}

	public virtual Vector3 GetDropVelocity()
	{
		return GetInheritedDropVelocity() + Vector3.up;
	}

	public virtual bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		return true;
	}

	public virtual string Admin_Who()
	{
		return $"Owner ID: {OwnerID}";
	}

	public virtual bool BuoyancyWake()
	{
		return false;
	}

	public virtual bool BuoyancySleep(bool inWater)
	{
		return false;
	}

	public virtual bool AllowInitChildSupports()
	{
		return false;
	}

	public Axis HasMovedInLS(int frame)
	{
		if (oldPosLSFrame != frame)
		{
			Vector3 localPosition = base.transform.localPosition;
			hasMovedLS = ComparePos(oldPosLS, localPosition);
			oldPosLSFrame = frame;
			oldPosLS = localPosition;
		}
		return hasMovedLS;
	}

	public static Axis ComparePos(Vector3 from, Vector3 to, float epsilon = 9.9999994E-11f)
	{
		Axis axis = Axis.None;
		Vector3 vector = to - from;
		if (vector.x * vector.x >= epsilon)
		{
			axis |= Axis.X;
		}
		if (vector.y * vector.y >= epsilon)
		{
			axis |= Axis.Y;
		}
		if (vector.z * vector.z >= epsilon)
		{
			axis |= Axis.Z;
		}
		return axis;
	}

	[RPC_Server.FromOwnerOrMounted]
	[RPC_Server]
	private void BroadcastSignalFromClient(RPCMessage msg)
	{
		uint num = StringPool.Get("BroadcastSignalFromClient");
		if (num == 0)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!(player == null) && player.rpcHistory.TryIncrement(num, (ulong)ConVar.Server.maxpacketspersecond_rpc_signal))
		{
			Signal signal = (Signal)msg.read.Int32();
			string arg = msg.read.String();
			if (!BroadcastSignalFromClientFilter(signal))
			{
				SignalBroadcast(signal, arg, msg.connection);
				OnReceivedSignalServer(signal, arg);
			}
		}
	}

	protected virtual bool BroadcastSignalFromClientFilter(Signal signal)
	{
		return false;
	}

	protected virtual void OnReceivedSignalServer(Signal signal, string arg)
	{
		SingletonComponent<NpcFireManager>.Instance.OnReceivedSignalServer(this, signal, arg);
	}

	public void SignalBroadcast(Signal signal, string arg, Connection sourceConnection = null)
	{
		if (net != null && net.group != null && !base.limitNetworking && Interface.CallHook("OnSignalBroadcast", this, sourceConnection, signal, arg) == null)
		{
			ClientRPC(RpcTarget.NetworkGroup("SignalFromServerEx", this, SendMethod.Unreliable, Priority.Immediate), (int)signal, arg, sourceConnection?.userid ?? 0);
		}
	}

	public void SignalBroadcast(Signal signal, Connection sourceConnection = null)
	{
		if (net != null && net.group != null)
		{
			ClientRPC(RpcTarget.NetworkGroup("SignalFromServer", this, SendMethod.Unreliable, Priority.Immediate), (int)signal, sourceConnection?.userid ?? 0);
		}
	}

	private bool IsEffectVisibleTo(BasePlayer player)
	{
		if (IsUnderground())
		{
			return player.IsUnderground();
		}
		return true;
	}

	public void SignalBroadcast(Signal signal, string arg, Connection sourceConnection, string fallbackEffect, float maxDistance = 0f)
	{
		bool flag = maxDistance > 0f;
		if ((!flag || !ConVar.Server.long_distance_sounds) && !ServerOcclusion.OcclusionEnabled)
		{
			SignalBroadcast(signal, arg, sourceConnection);
		}
		else
		{
			if (net == null || net.group == null || net.group.subscribers == null)
			{
				return;
			}
			using PooledHashSet<ulong> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<ulong>>();
			using PooledList<Connection> pooledList = Facepunch.Pool.Get<PooledList<Connection>>();
			using PooledList<Connection> pooledList2 = Facepunch.Pool.Get<PooledList<Connection>>();
			foreach (Connection subscriber in net.group.subscribers)
			{
				if (subscriber.player is BasePlayer basePlayer && !(basePlayer == null))
				{
					pooledHashSet.Add(basePlayer.userID.Get());
					if (ShouldNetworkTo(basePlayer))
					{
						pooledList.Add(subscriber);
					}
					else if (IsEffectVisibleTo(basePlayer))
					{
						pooledList2.Add(subscriber);
					}
				}
			}
			if (flag)
			{
				using (TimeWarning.New("BaseEntity.Signal.LongDistanceSound"))
				{
					foreach (Connection item in BaseNetworkable.GetConnectionsWithin(base.transform.position, maxDistance))
					{
						if (item.player is BasePlayer basePlayer2 && !(basePlayer2 == null) && !pooledHashSet.Contains(basePlayer2.userID.Get()) && IsEffectVisibleTo(basePlayer2))
						{
							pooledList2.Add(item);
						}
					}
				}
			}
			if (pooledList.Count > 0)
			{
				ClientRPC(RpcTarget.Players("SignalFromServerEx", pooledList, SendMethod.Unreliable, Priority.Immediate), (int)signal, arg, sourceConnection?.userid ?? 0);
			}
			if (pooledList2.Count > 0)
			{
				Effect.server.Run(fallbackEffect, base.transform.position, base.transform.up, sourceConnection, broadcast: false, pooledList2);
			}
		}
	}

	protected virtual void OnSkinChanged(ulong oldSkinID, ulong newSkinID)
	{
		if (oldSkinID != newSkinID)
		{
			skinID = newSkinID;
		}
	}

	protected virtual void OnAttachmentChanged(ulong oldAttachment, ulong newAttachment)
	{
		if (attachmentID != newAttachment)
		{
			attachmentID = newAttachment;
		}
	}

	protected virtual void OnSkinPreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside && Skinnable.All != null && Skinnable.FindForEntity(name) != null)
		{
			Rust.Workshop.WorkshopSkin.Prepare(rootObj);
			MaterialReplacement.Prepare(rootObj);
		}
	}

	public virtual void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		OnSkinPreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
	}

	public virtual bool HasAnySlot()
	{
		for (int i = 0; i < entitySlots.Length; i++)
		{
			if (entitySlots[i].IsValid(base.isServer))
			{
				return true;
			}
		}
		return false;
	}

	public BaseEntity GetSlot(Slot slot)
	{
		return entitySlots[(int)slot].Get(base.isServer);
	}

	public BaseLock GetLock()
	{
		return GetSlot(Slot.Lock) as BaseLock;
	}

	public string GetSlotAnchorName(Slot slot)
	{
		return slot.ToString().ToLower();
	}

	public void SetSlot(Slot slot, BaseEntity ent)
	{
		entitySlots[(int)slot].Set(ent);
		SendNetworkUpdate();
	}

	public EntityRef[] GetSlots()
	{
		return entitySlots;
	}

	public void SetSlots(EntityRef[] newSlots)
	{
		entitySlots = newSlots;
	}

	public virtual bool HasSlot(Slot slot)
	{
		return false;
	}

	protected void QueueSyncVar(byte nameID)
	{
		if (base.isServer)
		{
			if (nameID >= 32)
			{
				Debug.LogError($"nameID {nameID} is out of bitmask range (must be 0-{31})");
				return;
			}
			WarmupSyncVars();
			_serverSyncVarQueue |= (uint)(1 << (int)nameID);
		}
	}

	private void SendPackedSyncVarQueue()
	{
		SV_PackedSyncVarSendQueue();
	}

	private void SyncVarNetSend(NetWrite write, SendInfo sendInfo)
	{
		write.Send(sendInfo);
	}

	public void WarmupSyncVars()
	{
		if (_sendPackedSyncVarQueueAction == null)
		{
			_sendPackedSyncVarQueueAction = SendPackedSyncVarQueue;
		}
		if (!IsInvoking(_sendPackedSyncVarQueueAction))
		{
			Invoke(_sendPackedSyncVarQueueAction, 0.0333f);
		}
	}

	public void StopSyncVars()
	{
		if (IsInvoking(_sendPackedSyncVarQueueAction))
		{
			CancelInvoke(_sendPackedSyncVarQueueAction);
		}
	}

	protected NetWrite SV_PackedSyncVarNetStart()
	{
		using (TimeWarning.New("PackedSyncVar"))
		{
			NetWrite netWrite = Network.Net.sv.StartWrite();
			using (TimeWarning.New("Headers"))
			{
				netWrite.PacketID(Message.Type.PackedSyncVar);
				netWrite.EntityID(net.ID);
				netWrite.UInt32(_serverSyncVarQueue);
				return netWrite;
			}
		}
	}

	protected void SV_PackedSyncVarSendQueue()
	{
		if (_serverSyncVarQueue == 0 || Network.Net.sv == null || !Network.Net.sv.IsConnected() || net == null)
		{
			return;
		}
		using (TimeWarning.New("PackedSyncVarQueue"))
		{
			NetWrite netWrite = SV_PackedSyncVarNetStart();
			for (byte b = 0; b < 32; b++)
			{
				if ((_serverSyncVarQueue & (uint)(1 << (int)b)) != 0)
				{
					WriteSyncVar(b, netWrite);
					HandleCache(b);
				}
			}
			_serverSyncVarQueue = 0u;
			SyncVarNetSend(netWrite, new SendInfo(net.group.subscribers));
		}
	}

	private NetWrite SV_SyncVarNetStart(byte nameID)
	{
		using (TimeWarning.New("SyncVar"))
		{
			NetWrite netWrite = Network.Net.sv.StartWrite();
			using (TimeWarning.New("Headers"))
			{
				netWrite.PacketID(Message.Type.SyncVar);
				netWrite.EntityID(net.ID);
				netWrite.UInt8(nameID);
				return netWrite;
			}
		}
	}

	protected void SV_SyncVarSend(byte nameID)
	{
		if (Network.Net.sv != null && Network.Net.sv.IsConnected() && net != null)
		{
			NetWrite netWrite = SV_SyncVarNetStart(nameID);
			WriteSyncVar(nameID, netWrite);
			HandleCache(nameID);
			SyncVarNetSend(netWrite, new SendInfo(net.group.subscribers));
		}
	}

	protected void SyncVarNetWrite<T>(NetWrite write, T arg)
	{
		using (TimeWarning.New("Objects"))
		{
			NetworkWriteEx.WriteObject(write, arg);
		}
	}

	private void HandleCache(byte nameID)
	{
		if (ShouldInvalidateCache(nameID))
		{
			InvalidateNetworkCache();
		}
	}

	public bool HasTrait(TraitFlag f)
	{
		return (Traits & f) == f;
	}

	public bool HasAnyTrait(TraitFlag f)
	{
		return (Traits & f) != 0;
	}

	public virtual bool EnterTrigger(TriggerBase trigger)
	{
		if (triggers == null)
		{
			triggers = Facepunch.Pool.Get<List<TriggerBase>>();
		}
		triggers.Add(trigger);
		return true;
	}

	public virtual void LeaveTrigger(TriggerBase trigger)
	{
		if (triggers != null)
		{
			triggers.Remove(trigger);
			if (triggers.Count == 0)
			{
				Facepunch.Pool.FreeUnmanaged(ref triggers);
			}
		}
	}

	public void RemoveFromTriggers()
	{
		if (triggers == null)
		{
			return;
		}
		using (TimeWarning.New("RemoveFromTriggers"))
		{
			List<TriggerBase> obj = triggers.ShallowClonePooled();
			foreach (TriggerBase item in obj)
			{
				if ((bool)item)
				{
					item.RemoveEntity(this);
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
			if (triggers != null && triggers.Count == 0)
			{
				Facepunch.Pool.FreeUnmanaged(ref triggers);
			}
		}
	}

	public T FindTrigger<T>() where T : TriggerBase
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (!(trigger as T == null))
			{
				return trigger as T;
			}
		}
		return null;
	}

	public TriggerSafeZoneOverride FindActiveCombatTrigger()
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (trigger is TriggerSafeZoneOverride { IsCombatActive: not false } triggerSafeZoneOverride)
			{
				return triggerSafeZoneOverride;
			}
		}
		return null;
	}

	public bool InSafeCombatZone()
	{
		if (BaseGameMode.TryGetActiveGameMode(base.isServer, out var gameMode) && !gameMode.safeZone)
		{
			return false;
		}
		return FindActiveCombatTrigger() != null;
	}

	public bool FindTrigger<T>(out T result) where T : TriggerBase
	{
		result = FindTrigger<T>();
		return result != null;
	}

	private void ForceUpdateTriggersAction()
	{
		if (!base.IsDestroyed)
		{
			ForceUpdateTriggers(enter: false, exit: true, invoke: false);
		}
	}

	public void ForceUpdateTriggers(bool enter = true, bool exit = true, bool invoke = true)
	{
		if (this is BasePlayer { isInvisible: not false })
		{
			return;
		}
		List<TriggerBase> obj = Facepunch.Pool.Get<List<TriggerBase>>();
		List<TriggerBase> obj2 = Facepunch.Pool.Get<List<TriggerBase>>();
		if (triggers != null)
		{
			obj.AddRange(triggers);
		}
		Collider componentInChildren = GetComponentInChildren<Collider>();
		if (componentInChildren is CapsuleCollider)
		{
			CapsuleCollider capsuleCollider = componentInChildren as CapsuleCollider;
			Vector3 point = base.transform.position + new Vector3(0f, capsuleCollider.radius, 0f);
			Vector3 point2 = base.transform.position + new Vector3(0f, capsuleCollider.height - capsuleCollider.radius, 0f);
			GamePhysics.OverlapCapsule(point, point2, capsuleCollider.radius, obj2, 262144, QueryTriggerInteraction.Collide);
		}
		else if (componentInChildren is BoxCollider)
		{
			BoxCollider boxCollider = componentInChildren as BoxCollider;
			GamePhysics.OverlapOBB(new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, new Bounds(boxCollider.center, boxCollider.size)), obj2, 262144, QueryTriggerInteraction.Collide);
		}
		else if (componentInChildren is SphereCollider)
		{
			SphereCollider sphereCollider = componentInChildren as SphereCollider;
			GamePhysics.OverlapSphere(base.transform.TransformPoint(sphereCollider.center), sphereCollider.radius, obj2, 262144, QueryTriggerInteraction.Collide);
		}
		else
		{
			obj2.AddRange(obj);
		}
		IsForceUpdatingTriggers = true;
		if (exit)
		{
			foreach (TriggerBase item in obj)
			{
				if (!obj2.Contains(item))
				{
					item.OnTriggerExit(componentInChildren);
				}
			}
		}
		if (enter)
		{
			foreach (TriggerBase item2 in obj2)
			{
				if (!obj.Contains(item2))
				{
					item2.OnTriggerEnter(componentInChildren);
				}
			}
		}
		IsForceUpdatingTriggers = false;
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		if (invoke)
		{
			if (_forceUpdateTriggersCallback == null)
			{
				_forceUpdateTriggersCallback = ForceUpdateTriggersAction;
			}
			Invoke(_forceUpdateTriggersCallback, UnityEngine.Time.time - UnityEngine.Time.fixedTime + UnityEngine.Time.fixedDeltaTime * 1.5f);
		}
	}

	public virtual bool InHostileWarningZone()
	{
		if (triggers == null)
		{
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			TriggerHostileWarningZone triggerHostileWarningZone = triggers[i] as TriggerHostileWarningZone;
			if (!(triggerHostileWarningZone == null) && triggerHostileWarningZone.WarningEnabled(this))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool InSafeZone()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null && !activeGameMode.safeZone)
		{
			return false;
		}
		float num = 0f;
		Vector3 position = base.transform.position;
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				if (triggers[i] is TriggerSafeZone triggerSafeZone)
				{
					float safeLevel = triggerSafeZone.GetSafeLevel(position);
					if (safeLevel > num)
					{
						num = safeLevel;
					}
				}
			}
		}
		return num > 0f;
	}

	public TriggerParent FindSuitableParent()
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (trigger is TriggerParent triggerParent && triggerParent.ShouldParent(this, bypassOtherTriggerCheck: true))
			{
				return triggerParent;
			}
		}
		return null;
	}

	public virtual BasePlayer ToPlayer()
	{
		return null;
	}

	public override void InitShared()
	{
		base.InitShared();
		InitEntityLinks();
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!(Components[i] == null))
			{
				Components[i].InitShared();
			}
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		FreeEntityLinks();
		StopSyncVars();
		if (_autosaveBuffer != null)
		{
			_autosaveBufferPool.Return(_autosaveBuffer);
		}
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!(Components[i] == null))
			{
				Components[i].DestroyShared();
			}
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		parentBone = 0u;
		OwnerID = 0uL;
		flags = (Flags)0;
		skinID = 0uL;
		attachmentID = 0uL;
		HasBrain = false;
		parentEntity = default(EntityRef);
		ResetSyncVars();
		LookupPrefab();
		if (base.isServer)
		{
			_spawnable = null;
		}
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!(Components[i] == null))
			{
				Components[i].ResetState();
			}
		}
	}

	public virtual float InheritedVelocityScale()
	{
		return 0f;
	}

	public virtual bool InheritedVelocityDirection()
	{
		return true;
	}

	public virtual Vector3 GetInheritedProjectileVelocity(Vector3 direction)
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (baseEntity == null)
		{
			return Vector3.zero;
		}
		if (baseEntity.InheritedVelocityDirection())
		{
			return GetParentVelocity() * baseEntity.InheritedVelocityScale();
		}
		return Mathf.Max(Vector3.Dot(GetParentVelocity() * baseEntity.InheritedVelocityScale(), direction), 0f) * direction;
	}

	public virtual Vector3 GetInheritedThrowVelocity(Vector3 direction)
	{
		return GetParentVelocity();
	}

	public virtual Vector3 GetInheritedDropVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return Vector3.zero;
		}
		return baseEntity.GetWorldVelocity();
	}

	public Vector3 GetParentVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return Vector3.zero;
		}
		return baseEntity.GetWorldVelocity() + (baseEntity.GetAngularVelocity() * base.transform.localPosition - base.transform.localPosition);
	}

	public Vector3 GetWorldVelocity()
	{
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!(baseEntity != null))
		{
			return GetLocalVelocity();
		}
		return baseEntity.GetWorldVelocity() + (baseEntity.GetAngularVelocity() * base.transform.localPosition - base.transform.localPosition) + baseEntity.transform.TransformDirection(GetLocalVelocity());
	}

	public Vector3 GetLocalVelocity()
	{
		if (base.isServer)
		{
			return GetLocalVelocityServer();
		}
		return Vector3.zero;
	}

	public Quaternion GetAngularVelocity()
	{
		if (base.isServer)
		{
			return GetAngularVelocityServer();
		}
		return Quaternion.identity;
	}

	public virtual OBB WorldSpaceBounds()
	{
		return new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, bounds);
	}

	public Vector3 PivotPoint()
	{
		return base.transform.position;
	}

	public Vector3 CenterPoint()
	{
		return WorldSpaceBounds().position;
	}

	public Vector3 ClosestPoint(Vector3 position)
	{
		return WorldSpaceBounds().ClosestPoint(position);
	}

	public virtual Vector3 TriggerPoint()
	{
		return CenterPoint();
	}

	public float Distance(Vector3 position)
	{
		return (ClosestPoint(position) - position).magnitude;
	}

	public float SqrDistance(Vector3 position)
	{
		return (ClosestPoint(position) - position).sqrMagnitude;
	}

	public float Distance(BaseEntity other)
	{
		return Distance(other.transform.position);
	}

	public float SqrDistance(BaseEntity other)
	{
		return SqrDistance(other.transform.position);
	}

	public float Distance2D(Vector3 position)
	{
		return (ClosestPoint(position) - position).Magnitude2D();
	}

	public float SqrDistance2D(Vector3 position)
	{
		return (ClosestPoint(position) - position).SqrMagnitude2D();
	}

	public float Distance2D(BaseEntity other)
	{
		return Distance(other.transform.position);
	}

	public float SqrDistance2D(BaseEntity other)
	{
		return SqrDistance(other.transform.position);
	}

	public bool IsVisible(Ray ray, int layerMask, float maxDistance)
	{
		if (ray.origin.IsNaNOrInfinity())
		{
			return false;
		}
		if (ray.direction.IsNaNOrInfinity())
		{
			return false;
		}
		if (ray.direction == Vector3.zero)
		{
			return false;
		}
		if (!WorldSpaceBounds().Trace(ray, out var hit, maxDistance))
		{
			return false;
		}
		if (GamePhysics.Trace(ray, 0f, out var hitInfo, maxDistance, layerMask))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (entity == this)
			{
				return true;
			}
			if (entity != null && (bool)GetParentEntity() && GetParentEntity().EqualNetID(entity) && (RaycastHitEx.IsOnLayer(hitInfo, Rust.Layer.Vehicle_Detailed) || VisibilityPassesThroughParent))
			{
				return true;
			}
			if (hitInfo.distance <= hit.distance)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsVisibleSpecificLayers(Vector3 position, Vector3 target, int layerMask, float maxDistance = float.PositiveInfinity)
	{
		Vector3 vector = target - position;
		float magnitude = vector.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return true;
		}
		Vector3 vector2 = vector / magnitude;
		Vector3 vector3 = vector2 * Mathf.Min(magnitude, 0.01f);
		return IsVisible(new Ray(position + vector3, vector2), layerMask, maxDistance);
	}

	public bool IsVisible(Vector3 position, Vector3 target, float maxDistance = float.PositiveInfinity)
	{
		Vector3 vector = target - position;
		float magnitude = vector.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return true;
		}
		Vector3 vector2 = vector / magnitude;
		Vector3 vector3 = vector2 * Mathf.Min(magnitude, 0.01f);
		maxDistance = Mathf.Min(maxDistance, magnitude + 0.2f);
		return IsVisible(new Ray(position + vector3, vector2), 1218519041, maxDistance);
	}

	public bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		Vector3 target = CenterPoint();
		if (IsVisible(position, target, maxDistance))
		{
			return true;
		}
		Vector3 target2 = ClosestPoint(position);
		if (IsVisible(position, target2, maxDistance))
		{
			return true;
		}
		return false;
	}

	public bool IsVisibleAndCanSee(Vector3 position)
	{
		Vector3 vector = CenterPoint();
		if (IsVisible(position, vector) && CanSee(vector, position))
		{
			return true;
		}
		Vector3 vector2 = ClosestPoint(position);
		if (IsVisible(position, vector2) && CanSee(vector2, position))
		{
			return true;
		}
		return false;
	}

	public bool IsVisibleAndCanSeeLegacy(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		Vector3 vector = CenterPoint();
		if (IsVisible(position, vector, maxDistance) && IsVisible(vector, position, maxDistance))
		{
			return true;
		}
		Vector3 vector2 = ClosestPoint(position);
		if (IsVisible(position, vector2, maxDistance) && IsVisible(vector2, position, maxDistance))
		{
			return true;
		}
		return false;
	}

	public bool CanSee(Vector3 fromPos, Vector3 targetPos)
	{
		return GamePhysics.LineOfSight(fromPos, targetPos, 1218519041, this);
	}

	public bool CanSee(Vector3 fromPos, Vector3 targetPos, LayerMask additionalLayers)
	{
		return GamePhysics.LineOfSight(fromPos, targetPos, 0x48A12001 | (int)additionalLayers, this);
	}

	public bool IsOlderThan(BaseEntity other)
	{
		if (other == null)
		{
			return true;
		}
		NetworkableId obj = net?.ID ?? default(NetworkableId);
		NetworkableId networkableId = other.net?.ID ?? default(NetworkableId);
		return obj.Value < networkableId.Value;
	}

	public virtual bool IsOutside()
	{
		return IsOutside(WorldSpaceBounds().position);
	}

	public bool IsOutside(Vector3 position)
	{
		bool result = true;
		Vector3 vector = position + Vector3.up * 100f;
		vector.y = Mathf.Max(vector.y, TerrainMeta.HeightMap.GetHeight(vector) + 1f);
		if (UnityEngine.Physics.Linecast(vector, position, out var hitInfo, 161546513, QueryTriggerInteraction.Ignore))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(hitInfo.collider);
			if (baseEntity == null || !baseEntity.HasEntityInParents(this))
			{
				result = false;
			}
		}
		return result;
	}

	public bool IsUnderground(bool cached = true)
	{
		if (!cached || UnityEngine.Time.realtimeSinceStartup > nextHeightCheckTime)
		{
			cachedUnderground = EnvironmentManager.Check(base.transform.position, EnvironmentType.Underground);
			nextHeightCheckTime = UnityEngine.Time.realtimeSinceStartup + 5f;
		}
		return cachedUnderground;
	}

	public virtual float WaterFactor()
	{
		return WaterLevel.Factor(WorldSpaceBounds().ToBounds(), waves: true, volumes: true, this);
	}

	public virtual float AirFactor()
	{
		if (!(WaterFactor() > 0.85f))
		{
			return 1f;
		}
		return 0f;
	}

	public bool WaterTestFromVolumes(Vector3 pos, out WaterLevel.WaterInfo info)
	{
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(pos, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool IsInWaterVolume(Vector3 pos, out bool natural)
	{
		natural = false;
		if (triggers == null)
		{
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(pos, out var _))
			{
				natural = waterVolume.naturalSource;
				return true;
			}
		}
		return false;
	}

	public bool WaterTestFromVolumes(Bounds bounds, out WaterLevel.WaterInfo info)
	{
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(bounds, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool WaterTestFromVolumes(Vector3 start, Vector3 end, float radius, out WaterLevel.WaterInfo info)
	{
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(start, end, radius, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public static void WaterTestFromVolumes(ReadOnlySpan<BaseEntity> entities, ReadOnlySpan<Vector3> starts, ReadOnlySpan<Vector3> ends, ReadOnlySpan<float> radii, Span<WaterLevel.WaterInfo> results)
	{
		for (int i = 0; i < entities.Length; i++)
		{
			BaseEntity baseEntity = entities[i];
			ref WaterLevel.WaterInfo reference = ref results[i];
			if (baseEntity.triggers == null || baseEntity.triggers.Count == 0)
			{
				reference.isValid = false;
				continue;
			}
			Vector3 start = starts[i];
			Vector3 end = ends[i];
			float radius = radii[i];
			for (int j = 0; j < baseEntity.triggers.Count && (!(baseEntity.triggers[j] is WaterVolume waterVolume) || !waterVolume.Test(start, end, radius, out reference)); j++)
			{
			}
		}
	}

	public static void WaterTestFromVolumesIndirect(ReadOnlySpan<BaseEntity> entities, ReadOnlySpan<Vector3> starts, ReadOnlySpan<Vector3> ends, ReadOnlySpan<float> radii, ReadOnlySpan<int> indices, Span<WaterLevel.WaterInfo> results)
	{
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			BaseEntity baseEntity = entities[index];
			ref WaterLevel.WaterInfo reference = ref results[index];
			if (baseEntity.triggers == null || baseEntity.triggers.Count == 0)
			{
				reference.isValid = false;
				continue;
			}
			Vector3 start = starts[index];
			Vector3 end = ends[index];
			float radius = radii[index];
			for (int j = 0; j < baseEntity.triggers.Count && (!(baseEntity.triggers[j] is WaterVolume waterVolume) || !waterVolume.Test(start, end, radius, out reference)); j++)
			{
			}
		}
	}

	public static void WaterTestFromVolumesIndirect(ReadOnlySpan<BaseEntity> entities, ReadOnlySpan<Vector3> poses, ReadOnlySpan<int> indices, Span<WaterLevel.WaterInfo> results)
	{
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			BaseEntity baseEntity = entities[index];
			ref WaterLevel.WaterInfo reference = ref results[index];
			if (baseEntity.triggers == null || baseEntity.triggers.Count == 0)
			{
				reference.isValid = false;
				continue;
			}
			Vector3 pos = poses[index];
			for (int j = 0; j < baseEntity.triggers.Count && (!(baseEntity.triggers[j] is WaterVolume waterVolume) || !waterVolume.Test(pos, out reference)); j++)
			{
			}
		}
	}

	public virtual bool BlocksWaterFor(BasePlayer player)
	{
		return false;
	}

	public virtual bool ForceChildFullStability()
	{
		return true;
	}

	public virtual float Health()
	{
		return 0f;
	}

	public virtual float MaxHealth()
	{
		return 0f;
	}

	public virtual float AntiHackVelocity()
	{
		return 0f;
	}

	public virtual float AntiHackPadding()
	{
		return 0.1f;
	}

	public virtual float PenetrationResistance(HitInfo info)
	{
		return 100f;
	}

	public virtual GameObjectRef GetImpactEffect(HitInfo info)
	{
		return impactEffect;
	}

	public virtual void OnAttacked(HitInfo info)
	{
	}

	public virtual Item GetItem()
	{
		return null;
	}

	public virtual Item GetItem(ItemId itemId)
	{
		return null;
	}

	public virtual void GiveItem(Item item, GiveItemReason reason = GiveItemReason.Generic, GiveItemOptions options = GiveItemOptions.None)
	{
		item.Remove();
	}

	public virtual bool CanBeLooted(BasePlayer player)
	{
		return !IsTransferring();
	}

	public virtual BaseEntity GetEntity()
	{
		return this;
	}

	public override string ToString()
	{
		if (_name == null)
		{
			if (base.isServer)
			{
				if (net == null)
				{
					return base.ShortPrefabName;
				}
				_name = $"{base.ShortPrefabName}[{net.ID}]";
			}
			else
			{
				_name = base.ShortPrefabName;
			}
		}
		return _name;
	}

	public virtual string Categorize()
	{
		return "entity";
	}

	public void Log(string str)
	{
		if (base.isClient)
		{
			Debug.Log("<color=#ffa>[" + ToString() + "] " + str + "</color>", base.gameObject);
		}
		else
		{
			Debug.Log("<color=#aff>[" + ToString() + "] " + str + "</color>", base.gameObject);
		}
	}

	public void SetModel(Model mdl)
	{
		if (!(model == mdl))
		{
			model = mdl;
		}
	}

	public Model GetModel()
	{
		return model;
	}

	public virtual Transform[] GetBones()
	{
		if ((bool)model)
		{
			return model.GetBones();
		}
		return null;
	}

	public virtual Transform FindBone(string strName)
	{
		if ((bool)model)
		{
			return model.FindBone(strName);
		}
		return base.transform;
	}

	public virtual uint FindBoneID(Transform boneTransform)
	{
		if ((bool)model)
		{
			return model.FindBoneID(boneTransform);
		}
		return StringPool.closest;
	}

	public virtual Transform FindClosestBone(Vector3 worldPos)
	{
		if ((bool)model)
		{
			return model.FindClosestBone(worldPos);
		}
		return base.transform;
	}

	public virtual bool ShouldBlockProjectiles()
	{
		return true;
	}

	public virtual bool ShouldInheritNetworkGroup()
	{
		return true;
	}

	public virtual bool SupportsChildDeployables()
	{
		return false;
	}

	public virtual bool ForceDeployableSetParent()
	{
		return false;
	}

	public virtual bool ShouldAlwaysBlockNoClipChecks()
	{
		return false;
	}

	public virtual bool ShouldUseCastNoClipChecks()
	{
		return GetWorldVelocity().magnitude > 0f;
	}

	public bool IsOnMovingObject()
	{
		if (syncPosition)
		{
			return true;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (!(baseEntity != null))
		{
			return false;
		}
		return baseEntity.IsOnMovingObject();
	}

	public bool HasParentBoat(out BaseBoat parentBoat)
	{
		BaseEntity baseEntity = GetParentEntity();
		parentBoat = null;
		while (baseEntity != null)
		{
			if (baseEntity is PlayerBoat || baseEntity is Tugboat)
			{
				parentBoat = baseEntity as BaseBoat;
				return true;
			}
			baseEntity = baseEntity.GetParentEntity();
		}
		return false;
	}

	public void BroadcastEntityMessage(string msg, float radius = 20f, int layerMask = 1218652417)
	{
		if (base.isClient)
		{
			return;
		}
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		Vis.Entities(base.transform.position, radius, obj, layerMask);
		foreach (BaseEntity item in obj)
		{
			if (item.isServer)
			{
				item.OnEntityMessage(this, msg);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public virtual void OnEntityMessage(BaseEntity from, string msg)
	{
	}

	public T AddComponent<T>() where T : EntityComponentBase
	{
		T val = base.gameObject.AddComponent<T>();
		_components.Add(val);
		return val;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		info.msg.baseEntity = Facepunch.Pool.Get<ProtoBuf.BaseEntity>();
		if (info.forDisk)
		{
			if (this is BasePlayer)
			{
				if (baseEntity == null || baseEntity.enableSaving)
				{
					info.msg.baseEntity.pos = base.transform.localPosition;
					info.msg.baseEntity.rot = base.transform.localRotation.eulerAngles;
				}
				else
				{
					info.msg.baseEntity.pos = base.transform.position;
					info.msg.baseEntity.rot = base.transform.rotation.eulerAngles;
				}
			}
			else
			{
				info.msg.baseEntity.pos = base.transform.localPosition;
				info.msg.baseEntity.rot = base.transform.localRotation.eulerAngles;
			}
		}
		else
		{
			info.msg.baseEntity.pos = GetNetworkPosition();
			info.msg.baseEntity.rot = GetNetworkRotation().eulerAngles;
			info.msg.baseEntity.time = GetNetworkTime(in info.cachedTime);
			if (networkEntityScale)
			{
				if (BaseNetworkable.UseParallelSaves)
				{
					ProtoBuf.BaseEntity baseEntity2 = info.msg.baseEntity;
					TransformHandle handle = base.TransformHandle;
					baseEntity2.scale = Facepunch.Extend.TransformEx.Unsafe.GetLocalScaleMT(in handle);
				}
				else
				{
					info.msg.baseEntity.scale = base.TransformHandle.localScale;
				}
			}
		}
		info.msg.baseEntity.flags = (int)flags;
		info.msg.baseEntity.skinid = skinID;
		info.msg.baseEntity.attachmentID = attachmentID;
		if (info.forDisk && this is BasePlayer)
		{
			if (baseEntity != null && baseEntity.enableSaving)
			{
				info.msg.parent = Facepunch.Pool.Get<ParentInfo>();
				info.msg.parent.uid = parentEntity.uid;
				info.msg.parent.bone = parentBone;
			}
		}
		else if (baseEntity != null)
		{
			info.msg.parent = Facepunch.Pool.Get<ParentInfo>();
			info.msg.parent.uid = parentEntity.uid;
			info.msg.parent.bone = parentBone;
		}
		if (HasAnySlot())
		{
			info.msg.entitySlots = Facepunch.Pool.Get<EntitySlots>();
			info.msg.entitySlots.slotLock = entitySlots[0].uid;
			info.msg.entitySlots.slotFireMod = entitySlots[1].uid;
			info.msg.entitySlots.slotUpperModification = entitySlots[2].uid;
			info.msg.entitySlots.centerDecoration = entitySlots[5].uid;
			info.msg.entitySlots.lowerCenterDecoration = entitySlots[6].uid;
			info.msg.entitySlots.storageMonitor = entitySlots[7].uid;
		}
		if (info.forDisk && (bool)_spawnable)
		{
			_spawnable.Save(info);
		}
		if (info.msg.baseEntity != null)
		{
			AutoSaveSyncVars(info);
		}
		if (ShouldNetworkOwnerInfo() || (OwnerID != 0L && info.forDisk))
		{
			info.msg.ownerInfo = Facepunch.Pool.Get<OwnerInfo>();
			if (info.forDisk)
			{
				info.msg.ownerInfo.steamid = OwnerID;
			}
			else
			{
				info.msg.ownerInfo.steamid = ((OwnerID == info.forConnection.userid) ? info.forConnection.userid : 0);
			}
		}
		if (Components != null)
		{
			for (int i = 0; i < Components.Count; i++)
			{
				if (!(Components[i] == null))
				{
					Components[i].SaveComponent(info);
				}
			}
		}
		if (info.forTransfer && ShouldTransferAssociatedFiles)
		{
			info.msg.associatedFiles = Facepunch.Pool.Get<AssociatedFiles>();
			info.msg.associatedFiles.files = Facepunch.Pool.Get<List<AssociatedFiles.AssociatedFile>>();
			info.msg.associatedFiles.files.AddRange(FileStorage.server.QueryAllByEntity(net.ID));
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		if (ShouldNetworkOwnerInfo())
		{
			return false;
		}
		return base.CanUseNetworkCache(connection);
	}

	public virtual bool ShouldNetworkOwnerInfo()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseEntity != null)
		{
			ProtoBuf.BaseEntity baseEntity = info.msg.baseEntity;
			Flags old = flags;
			if (base.isServer)
			{
				baseEntity.flags &= -33554433;
			}
			flags = (Flags)baseEntity.flags;
			OnFlagsChanged(old, flags);
			OnSkinChanged(skinID, info.msg.baseEntity.skinid);
			OnAttachmentChanged(attachmentID, info.msg.baseEntity.attachmentID);
			if (info.fromDisk)
			{
				if (baseEntity.pos.IsNaNOrInfinity())
				{
					Debug.LogWarning(ToString() + " has broken position - " + baseEntity.pos);
					baseEntity.pos = Vector3.zero;
				}
				base.transform.localPosition = baseEntity.pos;
				base.transform.localRotation = Quaternion.Euler(baseEntity.rot);
			}
		}
		if (info.msg.entitySlots != null)
		{
			entitySlots[0].uid = info.msg.entitySlots.slotLock;
			entitySlots[1].uid = info.msg.entitySlots.slotFireMod;
			entitySlots[2].uid = info.msg.entitySlots.slotUpperModification;
			entitySlots[5].uid = info.msg.entitySlots.centerDecoration;
			entitySlots[6].uid = info.msg.entitySlots.lowerCenterDecoration;
			entitySlots[7].uid = info.msg.entitySlots.storageMonitor;
		}
		else
		{
			for (int i = 0; i < entitySlots.Length; i++)
			{
				entitySlots[i] = default(EntityRef);
			}
		}
		if (info.msg.parent != null)
		{
			if (base.isServer)
			{
				BaseEntity entity = BaseNetworkable.serverEntities.Find(info.msg.parent.uid) as BaseEntity;
				SetParent(entity, info.msg.parent.bone);
			}
			parentEntity.uid = info.msg.parent.uid;
			parentBone = info.msg.parent.bone;
		}
		else
		{
			parentEntity.uid = default(NetworkableId);
			parentBone = 0u;
		}
		if (info.msg.ownerInfo != null)
		{
			OwnerID = info.msg.ownerInfo.steamid;
		}
		if ((bool)_spawnable)
		{
			_spawnable.Load(info);
		}
		if (info.fromTransfer && ShouldTransferAssociatedFiles && info.msg.associatedFiles != null && info.msg.associatedFiles.files != null)
		{
			foreach (AssociatedFiles.AssociatedFile file in info.msg.associatedFiles.files)
			{
				if (FileStorage.server.Store(file.data, (FileStorage.Type)file.type, net.ID, file.numID) != file.crc)
				{
					Debug.LogWarning("Associated file has a different CRC after transfer!");
				}
			}
		}
		if (info.fromDisk && info.msg.baseEntity != null && IsTransferProtected())
		{
			float num = ((info.msg.baseEntity.protection > 0f) ? info.msg.baseEntity.protection : Nexus.protectionDuration);
			_transferProtectionRemaining = num;
			Invoke(DisableTransferProtectionAction, num);
		}
		if (info.msg.baseEntity != null)
		{
			AutoLoadSyncVars(info);
		}
		if (Components == null)
		{
			return;
		}
		for (int j = 0; j < Components.Count; j++)
		{
			if (!(Components[j] == null))
			{
				Components[j].LoadComponent(info);
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, byte arg1, bool arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt8(arg1);
			netWrite.Bool(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, NetworkableId arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ChickenCoopStatusUpdate arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, float arg3, float arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ulong arg1, ulong arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt64(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ulong arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt64(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, float arg2, uint arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Float(arg2);
			netWrite.UInt32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, VendingMachineLongTermStats arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, VendingMachinePurchaseHistoryMessage arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Proto(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.VendingMachine.SellOrderContainer arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, sbyte arg1, sbyte arg2, sbyte arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int8(arg1);
			netWrite.Int8(arg2);
			netWrite.Int8(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, string arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ArcadeGame arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, EntityIdList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, bool arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Bool(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, string arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ConversationResponseStatesList arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Proto(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, bool arg3, ConversationResponseStatesList arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Bool(arg3);
			netWrite.Proto(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1, Vector3 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, float arg2, Vector3 arg3, float arg4, float arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Float(arg2);
			netWrite.Vector3(in arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, NetworkableId arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, WireReconnectMessage arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, BasePlayer arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite write = ClientRPCStart(target.Function);
			write.Player(arg1);
			ClientRPCSend(write, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1, ulong arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, Vector3 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, string arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, int arg3, Vector3 arg4, ReadOnlySpan<byte> arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			netWrite.Vector3(in arg4);
			netWrite.Bytes(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ReadOnlySpan<byte> arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bytes(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, int arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, uint arg2, ReadOnlySpan<byte> arg3, uint arg4, byte arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.UInt32(arg2);
			netWrite.Bytes(arg3);
			netWrite.UInt32(arg4);
			netWrite.UInt8(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, ReadOnlySpan<byte> arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.Bytes(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, CopyPasteEntityInfo arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.Proto(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.AIDesign arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, int arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ApartmentTerminalData arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, uint arg2, uint arg3, int arg4, int arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.UInt32(arg2);
			netWrite.UInt32(arg3);
			netWrite.Int32(arg4);
			netWrite.Int32(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ReadOnlySpan<byte> arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Bytes(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PhoneDirectory arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, uint arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.UInt32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, string arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ItemAmountList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, string arg2, ulong arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.String(arg2);
			netWrite.UInt64(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ulong arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, float arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Float(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ItemId arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.ItemID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, float arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Float(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, int arg3, float arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1, bool arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			netWrite.Bool(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ulong arg1, int arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt64(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.DisplayingBoxStorage arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, ReadOnlySpan<byte> arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Bytes(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, GlobalEntityCollection arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.GrowableEntity arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, Vector3 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CardGame.RoundResults arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CardGame.CardList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, Vector3 arg2, Vector3 arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.Vector3(in arg2);
			netWrite.Vector3(in arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.PlayerModifiers arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, NetworkableId arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, ReadOnlySpan<byte> arg2, string arg3, uint arg4, int arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Bytes(arg2);
			netWrite.String(arg3);
			netWrite.UInt32(arg4);
			netWrite.Int32(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerTeam arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, ulong arg2, ulong arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.UInt64(arg2);
			netWrite.UInt64(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MapNote arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MapNoteList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MissionAcceptStatesList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, int arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ModelState arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, ulong arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, NetworkableId arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, RespawnInformation arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, int arg2, int arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, OceanPaths arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, SpectateTeamInfo arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, Vector3 arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, float arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.Float(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, string arg2, string arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.String(arg2);
			netWrite.String(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, UpdateItemContainer arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerUpdateLoot arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.PlayerMetabolism arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, Vector3 arg2, int arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Vector3(in arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, int arg2, float arg3, int arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Int32(arg2);
			netWrite.Float(arg3);
			netWrite.Int32(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, float arg3, float arg4, float arg5, int arg6, float arg7, float arg8)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			netWrite.Int32(arg6);
			netWrite.Float(arg7);
			netWrite.Float(arg8);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, float arg3, float arg4, int arg5, float arg6, float arg7)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			netWrite.Int32(arg5);
			netWrite.Float(arg6);
			netWrite.Float(arg7);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, bool arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Bool(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, float arg4, byte arg5, float arg6)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.Float(arg4);
			netWrite.UInt8(arg5);
			netWrite.Float(arg6);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, float arg4, float arg5, float arg6)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			netWrite.Float(arg6);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, float arg4, float arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ushort arg1, byte arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt16(arg1);
			netWrite.UInt8(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, short arg1, int arg2, int arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int16(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, short arg1, short arg2)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int16(arg1);
			netWrite.Int16(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, byte arg4, byte arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.UInt8(arg4);
			netWrite.UInt8(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, byte arg2, float arg3, byte arg4, bool arg5)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.UInt8(arg2);
			netWrite.Float(arg3);
			netWrite.UInt8(arg4);
			netWrite.Bool(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, byte arg2, float arg3, float arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.UInt8(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, byte arg2, int arg3, float arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.UInt8(arg2);
			netWrite.Int32(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, sbyte arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int8(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, UpdateItem arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, IndustrialConveyorTransfer arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.IndustrialConveyor.ItemFilterList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanActionResult arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanLog arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.ClanScoreEvents arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanInvitations arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanLeaderboard arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, long arg1, string arg2, int arg3, Color32 arg4)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int64(arg1);
			netWrite.String(arg2);
			netWrite.Int32(arg3);
			netWrite.Color32(in arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, long arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int64(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, int arg2, bool arg3)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.Int32(arg2);
			netWrite.Bool(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.Tree arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, TreeList arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.RelationshipManager.PlayerRelationships arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ProtoBuf.Ragdoll arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CustomPie arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CustomVitals arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CommunityEntity_DestroyUIs arg1)
	{
		if (Network.Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}
}
