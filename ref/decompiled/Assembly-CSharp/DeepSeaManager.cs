using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Network.Visibility;
using Oxide.Core;
using Prefabs.Misc;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Rendering;

public class DeepSeaManager : PointEntity<DeepSeaManager>
{
	[Serializable]
	public struct WipeStep
	{
		public float timeLeft;

		public Translate.Phrase message;

		public bool triggerFloatingCityAlarm;

		public GameObjectRef alarmEffect;
	}

	private struct PlacedPoint
	{
		public Vector2 pos;

		public float radius;

		public PlacedPoint(Vector2 pos, float radius)
		{
			this.pos = pos;
			this.radius = radius;
		}
	}

	[NonSerialized]
	public CardinalDirection ExitPortalDirection;

	public GameObjectRef PortalPrefab;

	public GameObjectRef MainIslandBillboardPrefab;

	public GameObjectRef DeepSeaBillboardPrefab;

	public List<BaseEntity> vehicleWhitelist;

	public static List<DeepSeaPortal> ServerPortals = new List<DeepSeaPortal>();

	public static List<DeepSeaPortal> ClientPortals = new List<DeepSeaPortal>();

	public static Transform PortalEntranceTransform;

	public static Transform PortalExitTransform;

	public static OBB PortalEntranceBounds;

	public static OBB PortalExitBounds;

	[NonSerialized]
	public static ListHashSet<IslandBillboard> ServerBillboards = new ListHashSet<IslandBillboard>();

	private readonly HashSet<uint> vehiclePrefabWhitelist = new HashSet<uint>();

	[SerializeField]
	private WipeStep[] wipeSteps;

	public static Translate.Phrase AboutToClose_Phrase = new Translate.Phrase("deepsea.abouttoclose", "The deep sea is no longer safe to enter");

	public static Translate.Phrase BoatNotStrongEnough_ToDeepSea_Phrase = new Translate.Phrase("deepsea.wrongboat-todeepsea", "Your boat cannot survive the journey to the deep sea");

	public static Translate.Phrase BoatNotStrongEnough_ToMainLand_Phrase = new Translate.Phrase("deepsea.wrongboat-tomainland", "Your boat cannot survive the journey to the main land");

	public static Translate.Phrase NeedBoat_ToDeepSea_Phrase = new Translate.Phrase("deepsea.needboat-todeepsea", "You cannot reach the deep sea without a sturdy boat");

	public static Translate.Phrase NeedBoat_ToMainLand_Phrase = new Translate.Phrase("deepsea.needboat-tomainland", "You cannot reach the main land without a sturdy boat");

	public static Translate.Phrase UnauthorizedVehicle_Phrase = new Translate.Phrase("deepsea.unauthorizedvehicle", "You cannot reach the deep sea with other vehicles on board");

	private Transform serverVolumesParent;

	private int nextWipeStepIndex;

	public const string ACHIEVEMENT_ENTER_DEEP_SEA_NAME = "ENTER_DEEP_SEA";

	private readonly List<ulong> foodTollPaid = new List<ulong>();

	public GameObjectRef[] islandRefs;

	public GameObjectRef[] floatingCityRefs;

	public GameObjectRef[] ghostShipRefs;

	public Material SeaFloorMaterial;

	[NonSerialized]
	public static ListHashSet<DeepSeaIsland> ServerIslands = new ListHashSet<DeepSeaIsland>();

	[NonSerialized]
	public static ListHashSet<GhostShip> ServerGhostShips = new ListHashSet<GhostShip>();

	[NonSerialized]
	public static ListHashSet<DeepSeaFloatingCity> ServerFloatingCities = new ListHashSet<DeepSeaFloatingCity>();

	[NonSerialized]
	public static ListHashSet<RHIB> ServerRHIBS = new ListHashSet<RHIB>();

	public static WaitForSeconds WaitSpawnGroupInterval;

	public static WaitForSeconds WaitEntitySpawnInterval;

	public static WaitForSeconds WaitNavMeshInterval;

	private static readonly List<PlacedPoint> placedPoints = new List<PlacedPoint>();

	private List<int>[,] grid;

	private int gridSizeX;

	private int gridSizeY;

	private float cellSize;

	private Queue<GameObjectRef> shuffledPrefabs;

	public AnimationCurve EndWipeProgressToWeatherLerp;

	public AnimationCurve EndWipeRadiationCurve;

	public static Bounds DeepSeaBounds = new Bounds(new Vector3(-5900f, 0f, 0f), new Vector3(4000f, 4000f, 4000f));

	public static float SeaFloorDepth = -50f;

	private const float RadVolumeWidth = 250f;

	private Transform sharedCollidersParent;

	private TriggerRadiation wipeRadiationVolume;

	public const Flags Flag_AboutToClose = Flags.Reserved1;

	public const Flags Flag_AlreadyOpenedOnce = Flags.Reserved2;

	private float __sync_TimeToWipe;

	private float __sync_TimeToNextOpening;

	private int __sync_CurrentEntrancePortalDirection;

	public CardinalDirection EntrancePortalDirection => (CardinalDirection)CurrentEntrancePortalDirection;

	[Sync(Autosave = true)]
	public float TimeToWipe
	{
		[CompilerGenerated]
		get
		{
			return __sync_TimeToWipe;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_TimeToWipe, value))
			{
				__sync_TimeToWipe = value;
				byte nameID = __GetWeaverID("TimeToWipe");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public float TimeToNextOpening
	{
		[CompilerGenerated]
		get
		{
			return __sync_TimeToNextOpening;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_TimeToNextOpening, value))
			{
				__sync_TimeToNextOpening = value;
				byte nameID = __GetWeaverID("TimeToNextOpening");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public int CurrentEntrancePortalDirection
	{
		[CompilerGenerated]
		get
		{
			return __sync_CurrentEntrancePortalDirection;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_CurrentEntrancePortalDirection, value))
			{
				__sync_CurrentEntrancePortalDirection = value;
				byte nameID = __GetWeaverID("CurrentEntrancePortalDirection");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DeepSeaManager.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static float GetPortalLerp(BaseNetworkable entity, DeepSeaPortal.PortalModeEnum portalMode)
	{
		if (entity == null)
		{
			return 0f;
		}
		if (portalMode == DeepSeaPortal.PortalModeEnum.None)
		{
			return 0f;
		}
		if (!IsInsidePortal(entity, portalMode))
		{
			return 0f;
		}
		Transform transform = ((portalMode == DeepSeaPortal.PortalModeEnum.Entrance) ? PortalEntranceTransform : PortalExitTransform);
		if (transform == null)
		{
			return 0f;
		}
		return Mathf.Clamp01(Mathf.InverseLerp(-0.5f, 0.5f, transform.InverseTransformPoint(entity.transform.position).z));
	}

	public static bool IsInsidePortal(BaseNetworkable entity, DeepSeaPortal.PortalModeEnum portalMode)
	{
		return IsInsidePortal(entity.transform.position, portalMode);
	}

	public static bool IsInsidePortal(Vector3 position, DeepSeaPortal.PortalModeEnum portalMode)
	{
		if (portalMode == DeepSeaPortal.PortalModeEnum.Entrance)
		{
			return PortalEntranceBounds.Contains(position);
		}
		return PortalExitBounds.Contains(position);
	}

	public static bool IsInsideAnyPortal(Vector3 position, DeepSeaPortal.PortalModeEnum portalMode, out DeepSeaPortal deepSeaPortal)
	{
		deepSeaPortal = null;
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == portalMode && serverPortal.WorldSpaceBounds().Contains(position))
			{
				deepSeaPortal = serverPortal;
				return true;
			}
		}
		return false;
	}

	public void GeneratePortalDirections()
	{
		CardinalDirection cardinalDirection = PickNewEntranceDirection();
		ExitPortalDirection = cardinalDirection.Opposite();
		CurrentEntrancePortalDirection = (int)cardinalDirection;
		CardinalDirection PickNewEntranceDirection()
		{
			if (DeepSea.forceEntrancePortalDirection > 0)
			{
				return (CardinalDirection)Mathf.Clamp(DeepSea.forceEntrancePortalDirection, 1, 4);
			}
			List<CardinalDirection> list = new List<CardinalDirection>();
			list.Add(CardinalDirection.North);
			list.Add(CardinalDirection.East);
			list.Add(CardinalDirection.South);
			list.Add(CardinalDirection.West);
			list.Remove((CardinalDirection)CurrentEntrancePortalDirection);
			return list.GetRandom();
		}
	}

	public void SpawnEntrancePortals()
	{
		if (PortalPrefab.isValid)
		{
			CardinalDirection[] array = new CardinalDirection[4]
			{
				CardinalDirection.North,
				CardinalDirection.East,
				CardinalDirection.South,
				CardinalDirection.West
			};
			foreach (CardinalDirection direction2 in array)
			{
				SpawnEntrancePortal(direction2);
			}
			Log("Spawned portals");
		}
		void SpawnEntrancePortal(CardinalDirection direction)
		{
			float depth = 300f;
			Bounds entrancePortalBounds = GetEntrancePortalBounds(direction, depth);
			Quaternion portalDirection = GetPortalDirection(direction);
			DeepSeaPortal obj = GameManager.server.CreateEntity(PortalPrefab.resourcePath, entrancePortalBounds.center, portalDirection) as DeepSeaPortal;
			obj.transform.localScale = entrancePortalBounds.size;
			obj.PortalMode = DeepSeaPortal.PortalModeEnum.Entrance;
			obj.PortalDirection = direction;
			obj.Spawn();
		}
	}

	public void ActivateEntrancePortal(CardinalDirection direction)
	{
		DeactivateAllEntrancePortals();
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && serverPortal.PortalDirection == direction)
			{
				using (FlagsUpdateScope flagsUpdateScope = serverPortal.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Open, b: true);
				}
				serverPortal.InitBounds();
				Log($"Activated entrance portal facing {direction}");
				return;
			}
		}
		Debug.LogError($"DeepSea: No entrance portal found for direction {direction}");
	}

	public void DeactivateAllEntrancePortals()
	{
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && serverPortal.HasFlag(Flags.Open))
			{
				using FlagsUpdateScope flagsUpdateScope = serverPortal.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.Open, b: false);
			}
		}
	}

	public void SpawnExitPortal()
	{
		if (ExitPortalDirection == CardinalDirection.None)
		{
			ExitPortalDirection = EntrancePortalDirection.Opposite();
		}
		float num = 300f;
		float offset = num / 2f;
		Bounds exitPortalBounds = GetExitPortalBounds(ExitPortalDirection, num, 300f, offset);
		Quaternion portalDirection = GetPortalDirection(ExitPortalDirection);
		Vector3 pos = exitPortalBounds.center + base.transform.position;
		DeepSeaPortal obj = GameManager.server.CreateEntity(PortalPrefab.resourcePath, pos, portalDirection) as DeepSeaPortal;
		obj.transform.localScale = exitPortalBounds.size;
		obj.PortalMode = DeepSeaPortal.PortalModeEnum.Exit;
		obj.PortalDirection = ExitPortalDirection;
		obj.Spawn();
	}

	private void KillExitPortal()
	{
		DeepSeaPortal[] array = ServerPortals.ToArray();
		foreach (DeepSeaPortal deepSeaPortal in array)
		{
			if (deepSeaPortal != null && deepSeaPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Exit)
			{
				deepSeaPortal.Kill();
				break;
			}
		}
	}

	public void KillAllPortals()
	{
		if (ServerPortals == null || ServerPortals.Count == 0)
		{
			return;
		}
		DeepSeaPortal[] array = ServerPortals.ToArray();
		foreach (DeepSeaPortal deepSeaPortal in array)
		{
			if (deepSeaPortal != null)
			{
				deepSeaPortal.Kill();
			}
		}
		ServerPortals.Clear();
	}

	private void SpawnBillboards()
	{
		KillAllBillboards();
		SpawnDeepSeaBillboards();
		SpawnMainIslandBillboard();
	}

	private void KillAllBillboards()
	{
		if (ServerBillboards == null || ServerBillboards.Count == 0)
		{
			return;
		}
		IslandBillboard[] array = ServerBillboards.ToArray();
		foreach (IslandBillboard islandBillboard in array)
		{
			if (islandBillboard != null)
			{
				islandBillboard.Kill();
			}
		}
		ServerBillboards.Clear();
	}

	public void SpawnDeepSeaBillboards(int count = 5)
	{
		Vector3 position = PortalEntranceTransform.position;
		float x = PortalEntranceBounds.extents.x;
		Quaternion portalDirection = GetPortalDirection(ExitPortalDirection);
		Vector3 vector = portalDirection * Vector3.back;
		Vector3 vector2 = portalDirection * Vector3.right;
		float num = x * 0.3f;
		float num2 = x * 0.2f;
		List<Vector3> obj = Facepunch.Pool.Get<List<Vector3>>();
		for (int i = 0; i < count; i++)
		{
			Vector3 vector3 = Vector3.zero;
			Vector3 lastCandidate = Vector3.zero;
			for (int j = 0; j < 20; j++)
			{
				float num3 = UnityEngine.Random.Range(0f - num, num);
				float num4 = UnityEngine.Random.Range(0f - num2, num2);
				lastCandidate = position + vector * 2000f + vector2 * num3 + portalDirection * Vector3.forward * num4;
				if (!obj.Any((Vector3 p) => Vector3.Distance(p, lastCandidate) < 250f))
				{
					vector3 = lastCandidate;
					break;
				}
			}
			if (vector3 == Vector3.zero)
			{
				vector3 = lastCandidate;
			}
			obj.Add(vector3);
			(GameManager.server.CreateEntity(DeepSeaBillboardPrefab.resourcePath, vector3, portalDirection) as IslandBillboard).Spawn();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void SpawnMainIslandBillboard()
	{
		Vector3 position = PortalExitTransform.position;
		Quaternion portalDirection = GetPortalDirection(ExitPortalDirection);
		Vector3 vector = portalDirection * Vector3.back;
		Vector3 pos = position + vector * -2000f;
		(GameManager.server.CreateEntity(MainIslandBillboardPrefab.resourcePath, pos, portalDirection) as IslandBillboard).Spawn();
	}

	public Bounds GetEntrancePortalBounds(CardinalDirection direction, float depth, float height = 300f, float offset = 0f)
	{
		float num = 60f;
		Vector3 extents = SingletonComponent<ValidBounds>.Instance.worldBounds.extents;
		Vector3 vector = TerrainMeta.MarginSize / 2f;
		float num2 = Mathf.Min(Mathf.Min(extents.x, extents.z), Mathf.Min(vector.x, vector.z));
		float num3 = vehicle.world_boundary_force_start_distance - vehicle.world_boundary_force_offset;
		float b = num2 - num3 - depth / 2f - num;
		float x = num2 * 2f;
		float num4 = Mathf.Min(TerrainMeta.Size.x / 2f + DeepSea.island_portal_terrain_distance, b);
		num4 += offset;
		Vector3 size = new Vector3(x, height, depth);
		return direction switch
		{
			CardinalDirection.North => new Bounds(new Vector3(0f, 0f, num4), size), 
			CardinalDirection.South => new Bounds(new Vector3(0f, 0f, 0f - num4), size), 
			CardinalDirection.East => new Bounds(new Vector3(num4, 0f, 0f), size), 
			CardinalDirection.West => new Bounds(new Vector3(0f - num4, 0f, 0f), size), 
			_ => default(Bounds), 
		};
	}

	public Bounds GetExitPortalBounds(CardinalDirection direction, float depth, float height = 300f, float offset = 0f)
	{
		return direction switch
		{
			CardinalDirection.North => new Bounds(new Vector3(0f, 0f, DeepSeaBounds.size.z / 2f - offset), new Vector3(DeepSeaBounds.size.x, height, depth)), 
			CardinalDirection.South => new Bounds(new Vector3(0f, 0f, DeepSeaBounds.size.z / -2f + offset), new Vector3(DeepSeaBounds.size.x, height, depth)), 
			CardinalDirection.East => new Bounds(new Vector3(DeepSeaBounds.size.x / 2f - offset, 0f, 0f), new Vector3(DeepSeaBounds.size.z, height, depth)), 
			CardinalDirection.West => new Bounds(new Vector3(DeepSeaBounds.size.x / -2f + offset, 0f, 0f), new Vector3(DeepSeaBounds.size.z, height, depth)), 
			_ => default(Bounds), 
		};
	}

	private Quaternion GetPortalDirection(CardinalDirection direction)
	{
		return Quaternion.LookRotation(direction.ToVectorDirection(), Vector3.up);
	}

	public Vector3 GetDeepSeaEntrancePosition(BaseEntity entity)
	{
		Vector3 vector = SetToOceanHeight(DeepSeaBounds.center);
		if (!FindPortals(out var entrancePortal, out var exitPortal))
		{
			Debug.LogWarning("No entrance/exit portals found in deep sea, defaulting to center of deep sea");
			return vector;
		}
		Vector3 normalizedPosition = ConvertInputPositionToOutputPosition(entrancePortal, exitPortal, entity.transform.position);
		if (FindUnoccupiedPortalSpawnLocation(normalizedPosition, exitPortal.WorldSpaceBounds().ToBounds(), vector, entity.bounds, out var spawnPos))
		{
			return spawnPos;
		}
		return exitPortal.transform.position;
	}

	public Vector3 GetDeepSeaExitPosition(BaseEntity entity)
	{
		if (!FindPortals(out var entrancePortal, out var exitPortal))
		{
			Debug.LogWarning("No entrance/exit portals found in deep sea, defaulting to spawn beach");
			return SpawnHandler.GetSpawnPoint()?.pos ?? Vector3.zero;
		}
		Vector3 point = ConvertInputPositionToOutputPosition(exitPortal, entrancePortal, entity.transform.position);
		Vector3 centerOfArea = SetToOceanHeight(TerrainMeta.Center);
		Bounds portalWorldBounds = entrancePortal.WorldSpaceBounds().ToBounds();
		Vector3 right = entrancePortal.transform.right;
		float num = entrancePortal.transform.position.magnitude - entrancePortal.transform.localScale.z * 0.5f - 50f;
		float num2 = Mathf.Max(0f, entrancePortal.transform.localScale.x - num * 2f);
		portalWorldBounds.Expand(-new Vector3(Mathf.Abs(right.x) * num2, 0f, Mathf.Abs(right.z) * num2));
		point = portalWorldBounds.ClosestPoint(point);
		if (FindUnoccupiedPortalSpawnLocation(point, portalWorldBounds, centerOfArea, entity.bounds, out var spawnPos))
		{
			return spawnPos;
		}
		return entrancePortal.transform.position;
	}

	public bool FindPortals(out DeepSeaPortal entrancePortal, out DeepSeaPortal exitPortal)
	{
		entrancePortal = null;
		exitPortal = null;
		foreach (DeepSeaPortal serverPortal in ServerPortals)
		{
			if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && serverPortal.HasFlag(Flags.Open))
			{
				entrancePortal = serverPortal;
			}
			else if (serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Exit)
			{
				exitPortal = serverPortal;
			}
		}
		if (entrancePortal != null)
		{
			return exitPortal != null;
		}
		return false;
	}

	private Vector3 ConvertInputPositionToOutputPosition(DeepSeaPortal inputPortal, DeepSeaPortal outputPortal, Vector3 worldPosition)
	{
		Vector3 position = inputPortal.transform.InverseTransformPoint(worldPosition);
		position.x *= -1f;
		Vector3 target = outputPortal.transform.TransformPoint(position);
		return outputPortal.WorldSpaceBounds().ClosestPoint(target);
	}

	private Vector3 AdjustPortalExitPosition(Vector3 position, Vector3 centerOfArea, float distanceToMove = 20f)
	{
		position = SetToOceanHeight(position);
		return Vector3.MoveTowards(position, centerOfArea, distanceToMove);
	}

	private Vector3 SetToOceanHeight(Vector3 position)
	{
		position.y = 0f;
		return position;
	}

	private bool FindUnoccupiedPortalSpawnLocation(Vector3 normalizedPosition, Bounds portalWorldBounds, Vector3 centerOfArea, Bounds entityBounds, out Vector3 spawnPos, float distanceToMove = 20f)
	{
		Vector3 vector = AdjustPortalExitPosition(normalizedPosition, centerOfArea, distanceToMove);
		Vector3 vector2 = AdjustPortalExitPosition(portalWorldBounds.min, centerOfArea, distanceToMove);
		Vector3 vector3 = AdjustPortalExitPosition(portalWorldBounds.max, centerOfArea, distanceToMove);
		float duration = 30f;
		if (DeepSea.debug_portal_spawnattempts)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(vector, duration, Color.yellow, 10f));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(vector, duration, Color.yellow, "Start"));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(vector2, duration, Color.yellow, 10f));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(vector2, duration, Color.yellow, "Min"));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(vector3, duration, Color.yellow, 10f));
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(vector3, duration, Color.yellow, "Max"));
		}
		float magnitude = entityBounds.extents.magnitude;
		if (TestPortalSpawnLocations(vector, vector2, magnitude, out spawnPos))
		{
			return true;
		}
		if (TestPortalSpawnLocations(vector, vector3, magnitude, out spawnPos))
		{
			return true;
		}
		spawnPos = default(Vector3);
		return false;
	}

	private bool TestPortalSpawnLocations(Vector3 startPos, Vector3 endPos, float radius, out Vector3 spawnPos)
	{
		Vector3 vector = startPos;
		int num = 0;
		while (Vector3.Distance(vector, endPos) > 1f && num++ < 100)
		{
			bool flag = IsValidPortalSpawnLocation(vector, radius);
			if (DeepSea.debug_portal_spawnattempts)
			{
				Color color = (flag ? Color.green : Color.red);
				float duration = 30f;
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(vector, duration, color, radius));
				ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Text(vector, duration, color, num.ToString()));
			}
			if (flag)
			{
				spawnPos = vector;
				return true;
			}
			vector = Vector3.MoveTowards(vector, endPos, radius);
		}
		spawnPos = default(Vector3);
		return false;
	}

	public bool IsValidPortalSpawnLocation(Vector3 position, float radius, BasePlayer ignorePlayer = null)
	{
		using (PooledList<Collider> pooledList = Facepunch.Pool.Get<PooledList<Collider>>())
		{
			GamePhysics.OverlapSphere(position, radius, pooledList, 1218511105);
			if (pooledList.Any())
			{
				foreach (Collider item in pooledList)
				{
					if (!(item == null) && (!GameObjectEx.IsOnLayer(item.gameObject, Rust.Layer.World) || !(item.gameObject.name == "TerrainMargin")) && !(GameObjectEx.ToBaseEntity(item) is DeepSeaPortal))
					{
						if (DeepSea.debug_portal_spawnattempts)
						{
							Debug.Log("Invalid portal spawn pos: " + item.name);
						}
						return false;
					}
				}
			}
		}
		using (PooledList<BasePlayer> pooledList2 = Facepunch.Pool.Get<PooledList<BasePlayer>>())
		{
			Vis.Entities(position, radius, pooledList2, 131072, QueryTriggerInteraction.Ignore);
			foreach (BasePlayer item2 in pooledList2)
			{
				if (!(item2 == ignorePlayer))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		bool flag = false;
		if (WaterSystem.Instance == null && !flag)
		{
			Kill();
			return;
		}
		if (!DeepSea.enabled)
		{
			StartCoroutine(CloseAndDestroyDeepSea());
			return;
		}
		FillVehicleWhitelist();
		KillAllPortals();
		SpawnEntrancePortals();
		CacheWaitForSeconds(UnityEngine.Application.isEditor);
		if (wipeSteps != null && wipeSteps.Length != 0)
		{
			Array.Sort(wipeSteps, (WipeStep a, WipeStep b) => b.timeLeft.CompareTo(a.timeLeft));
		}
		if (!Rust.Application.isLoadingSave)
		{
			StartServerTick();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (IsOpen())
		{
			if (serverVolumesParent == null)
			{
				CreateRadVolumes();
			}
			if (EntrancePortalDirection == CardinalDirection.None)
			{
				int currentEntrancePortalDirection = Mathf.RoundToInt(TerrainMeta.LootAxisAngle / 90f) + 1;
				CurrentEntrancePortalDirection = currentEntrancePortalDirection;
			}
			ActivateEntrancePortal(EntrancePortalDirection);
			ExitPortalDirection = EntrancePortalDirection.Opposite();
			SpawnExitPortal();
			RestoreIslandNavMeshes();
		}
		StartServerTick();
	}

	private void StartServerTick()
	{
		if (!HasFlag(Flags.Reserved2))
		{
			if (DeepSea.openOnServerWipe || UnityEngine.Application.isEditor)
			{
				SetTimeToNextOpening(0f);
			}
			else
			{
				ResetTimeToNextOpening();
			}
		}
		if (!IsInvoking(ServerTick))
		{
			InvokeRepeating(ServerTick, 5f, 1f);
		}
	}

	public void ServerTick()
	{
		if (!IsOpen() && !IsBusy())
		{
			if (TimeToNextOpening <= 0f)
			{
				OpenDeepSea();
			}
			else
			{
				TimeToNextOpening--;
			}
		}
		if (!IsOpen() || IsBusy())
		{
			return;
		}
		if (TimeToWipe <= 0f)
		{
			CloseDeepSea();
			return;
		}
		TimeToWipe--;
		ProcessWipeSteps();
		float wipeRadiationPhaseDuration = DeepSea.wipeRadiationPhaseDuration;
		bool flag = TimeToWipe <= wipeRadiationPhaseDuration && TimeToWipe > 0f;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, flag);
		}
		if (!(wipeRadiationVolume != null) || !(wipeRadiationPhaseDuration > 0f))
		{
			return;
		}
		if (flag)
		{
			float time = 1f - Mathf.Clamp01(TimeToWipe / wipeRadiationPhaseDuration);
			float radiationAmountOverride = Mathf.Lerp(0f, 60f, EndWipeRadiationCurve.Evaluate(time));
			wipeRadiationVolume.RadiationAmountOverride = radiationAmountOverride;
			if (!wipeRadiationVolume.gameObject.activeSelf)
			{
				wipeRadiationVolume.SetActive(active: true);
			}
		}
		else if (TimeToWipe > wipeRadiationPhaseDuration)
		{
			wipeRadiationVolume.SetActive(active: false);
			wipeRadiationVolume.RadiationAmountOverride = 0f;
		}
	}

	private void ResetWipeSteps()
	{
		nextWipeStepIndex = 0;
	}

	private void ProcessWipeSteps()
	{
		if (wipeSteps != null && wipeSteps.Length != 0)
		{
			while (nextWipeStepIndex < wipeSteps.Length && TimeToWipe > 0f && TimeToWipe <= wipeSteps[nextWipeStepIndex].timeLeft)
			{
				WipeStep step = wipeSteps[nextWipeStepIndex];
				OnWipeStepReached(step);
				nextWipeStepIndex++;
			}
		}
	}

	private void OnWipeStepReached(WipeStep step)
	{
		int num = Mathf.CeilToInt(step.timeLeft / 60f);
		if (num < 1)
		{
			num = 1;
		}
		string text = num.ToString();
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (IsInsideDeepSea(activePlayer))
			{
				activePlayer.ShowToast(GameTip.Styles.Blue_Long, step.message, true, text);
			}
		}
		if (!step.triggerFloatingCityAlarm)
		{
			return;
		}
		foreach (DeepSeaFloatingCity serverFloatingCity in ServerFloatingCities)
		{
			serverFloatingCity.TriggerWipeAlarm(step.alarmEffect);
		}
	}

	public void MoveToDeepSea(BaseEntity entity)
	{
		if (entity == null)
		{
			return;
		}
		if (entity.isClient)
		{
			Debug.LogError("DeepSea: Can't move a clientside entity to the deep sea");
		}
		else if (!IsInsideDeepSea(entity))
		{
			using (PooledList<BasePlayer> passengers = Facepunch.Pool.Get<PooledList<BasePlayer>>())
			{
				BaseVehicle.GetPassengersForVehicle(entity, passengers);
				PreTeleportEntity(entity, passengers, isEnterDeepSea: true);
				Vector3 deepSeaEntrancePosition = GetDeepSeaEntrancePosition(entity);
				TeleportEntity(entity, deepSeaEntrancePosition);
				PostTeleportEntity(entity, passengers);
				NotifyEntityMovedToDeepSea(entity);
			}
		}
	}

	public void MoveToMainIsland(BaseEntity entity)
	{
		if (entity == null)
		{
			return;
		}
		if (entity.isClient)
		{
			Debug.LogError("DeepSea: Can't move a clientside entity to the main island");
			return;
		}
		if (!IsInsideDeepSea(entity))
		{
			Debug.LogWarning($"DeepSea: Trying to remove entity {entity} from the deep sea but not inside it");
			return;
		}
		Vector3 deepSeaExitPosition = GetDeepSeaExitPosition(entity);
		using PooledList<BasePlayer> passengers = Facepunch.Pool.Get<PooledList<BasePlayer>>();
		BaseVehicle.GetPassengersForVehicle(entity, passengers);
		PreTeleportEntity(entity, passengers, isEnterDeepSea: false);
		TeleportEntity(entity, deepSeaExitPosition);
		PostTeleportEntity(entity, passengers);
		NotifyEntityMovedToMainIsland(entity);
	}

	private static void PreTeleportEntity(BaseEntity entity, List<BasePlayer> passengers, bool isEnterDeepSea)
	{
		if (entity is BasePlayer player)
		{
			PreTeleportPlayer(player, isEnterDeepSea);
			return;
		}
		foreach (BasePlayer passenger in passengers)
		{
			PreTeleportPlayer(passenger, isEnterDeepSea);
			if (isEnterDeepSea)
			{
				NotifyEntityMovedToDeepSea(passenger);
			}
			else
			{
				NotifyEntityMovedToMainIsland(passenger);
			}
		}
	}

	private static void PreTeleportPlayer(BasePlayer player, bool isEnterDeepSea)
	{
		if (!player.IsNpcPlayer())
		{
			player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, b: true);
			player.ClientRPC(RpcTarget.Player("StartLoading", player));
			PointEntity<DeepSeaManager>.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_PlayerEnterOrLeaveDeepSea", player), isEnterDeepSea);
			if (!player.isMounted)
			{
				player.StartSleeping();
			}
		}
	}

	private static void TeleportEntity(BaseEntity entity, Vector3 position)
	{
		if (entity is BasePlayer basePlayer && basePlayer.IsNonNpcPlayer())
		{
			if (!basePlayer.isMounted)
			{
				basePlayer.Teleport(position);
			}
		}
		else
		{
			entity.transform.position = position;
			entity.ClientRPC(RpcTarget.NetworkGroup("SnapPositionTo"), position, entity.transform.eulerAngles);
		}
	}

	private static void PostTeleportEntity(BaseEntity entity, List<BasePlayer> passengers)
	{
		Dictionary<BasePlayer, BaseMountable> dict = Facepunch.Pool.Get<Dictionary<BasePlayer, BaseMountable>>();
		foreach (BasePlayer passenger in passengers)
		{
			BaseMountable mounted = passenger.GetMounted();
			if (mounted != null)
			{
				dict[passenger] = mounted;
			}
			passenger.EnsureDismounted();
			passenger.SendNetworkUpdateImmediate();
			if (Rust.GameInfo.HasAchievements)
			{
				passenger.GiveAchievement("ENTER_DEEP_SEA");
			}
		}
		entity.UpdateNetworkGroup();
		foreach (BasePlayer passenger2 in passengers)
		{
			passenger2.UpdateNetworkGroup();
		}
		entity.SendNetworkUpdateImmediate();
		foreach (BasePlayer passenger3 in passengers)
		{
			passenger3.SendNetworkUpdateImmediate();
		}
		foreach (KeyValuePair<BasePlayer, BaseMountable> item in dict)
		{
			BaseMountable value = item.Value;
			if (value != null)
			{
				value.MountPlayer(item.Key);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref dict);
	}

	private static void NotifyEntityMovedToMainIsland(BaseEntity entity)
	{
		if (entity is IReceiveDeepSeaNotifications receiveDeepSeaNotifications)
		{
			receiveDeepSeaNotifications.OnExitDeepSea();
		}
	}

	private static void NotifyEntityMovedToDeepSea(BaseEntity entity)
	{
		if (entity is IReceiveDeepSeaNotifications receiveDeepSeaNotifications)
		{
			receiveDeepSeaNotifications.OnEnterDeepSea();
		}
	}

	private void DestroyRadVolumes()
	{
		if (serverVolumesParent != null && serverVolumesParent.gameObject != null)
		{
			UnityEngine.Object.Destroy(serverVolumesParent.gameObject);
			serverVolumesParent = null;
		}
	}

	private void CreateRadVolumes()
	{
		DestroyRadVolumes();
		if (serverVolumesParent == null)
		{
			serverVolumesParent = new GameObject("Volumes").transform;
			serverVolumesParent.SetParent(base.transform, worldPositionStays: false);
		}
		CardinalDirection num = GetEntrancePortalDirection().Opposite();
		if (num != CardinalDirection.West)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Left", new Vector3(DeepSeaBounds.size.x / -2f, 0f, 0f), new Vector3(250f, DeepSeaBounds.size.y, DeepSeaBounds.size.z));
		}
		if (num != CardinalDirection.East)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Right", new Vector3(DeepSeaBounds.size.x / 2f, 0f, 0f), new Vector3(250f, DeepSeaBounds.size.y, DeepSeaBounds.size.z));
		}
		if (num != CardinalDirection.North)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Front", new Vector3(0f, 0f, DeepSeaBounds.size.z / 2f), new Vector3(DeepSeaBounds.size.x, DeepSeaBounds.size.y, 250f));
		}
		if (num != CardinalDirection.South)
		{
			CreateRadiationVolume("DeepSea_RadVolume_Back", new Vector3(0f, 0f, DeepSeaBounds.size.z / -2f), new Vector3(DeepSeaBounds.size.x, DeepSeaBounds.size.y, 250f));
		}
		wipeRadiationVolume = CreateRadiationVolume("DeepSea_RadVolume_Wipe", Vector3.zero, DeepSeaBounds.size);
		wipeRadiationVolume.SetActive(active: false);
	}

	private TriggerRadiation CreateRadiationVolume(string name, Vector3 pos, Vector3 size)
	{
		BoxCollider boxCollider = CreateCollider(name, pos, size, serverVolumesParent);
		boxCollider.isTrigger = true;
		TransformEx.SetLayerRecursive(boxCollider.gameObject, 18);
		TriggerRadiation triggerRadiation = boxCollider.gameObject.AddComponent<TriggerRadiation>();
		triggerRadiation.InterestLayers = 131072;
		triggerRadiation.usePerAxisFalloff = true;
		triggerRadiation.BypassArmor = true;
		if (size.x == 250f)
		{
			triggerRadiation.falloffPerAxis.x = 60f;
		}
		if (size.y == 250f)
		{
			triggerRadiation.falloffPerAxis.y = 60f;
		}
		if (size.z == 250f)
		{
			triggerRadiation.falloffPerAxis.z = 60f;
		}
		return triggerRadiation;
	}

	public void OpenDeepSea()
	{
		GeneratePortalDirections();
		StopAllCoroutines();
		StartCoroutine(OpenDeepSeaAsync());
		SetFlagLocal(Flags.Reserved2, b: true);
	}

	private IEnumerator OpenDeepSeaAsync()
	{
		Interface.CallHook("OnDeepSeaOpen", this);
		Log("Opening...");
		SetFlagLocal(Flags.Busy, b: true);
		SetTimeToWipe(DeepSea.wipeDuration);
		SetTimeToNextOpening(0f);
		yield return GenerateContentAsync();
		yield return TriggerSpawnGroups();
		yield return GenerateIslandNavMeshes();
		ActivateEntrancePortal(EntrancePortalDirection);
		SpawnExitPortal();
		SpawnBillboards();
		NetworkTreesScale();
		SpawnGhostShipHackableCrate();
		CreateRadVolumes();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, b: true);
		}
		SetFlagLocal(Flags.Busy, b: false);
		ResetWipeSteps();
		DoOpenNotification();
		PlanterBoxStatic.OnDeepSeaSpawned();
		Facepunch.Rust.Analytics.Azure.OnDeepSeaToggled(toggle: true);
		Log("Done!");
		Interface.CallHook("OnDeepSeaOpened", this);
	}

	public void CloseDeepSea(bool forceClose = false)
	{
		if (forceClose || IsOpen())
		{
			StopAllCoroutines();
			StartCoroutine(CloseDeepSeaAsync());
		}
	}

	private IEnumerator CloseDeepSeaAsync()
	{
		Interface.CallHook("OnDeepSeaClose", this);
		Log("Closing...");
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, b: false);
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
		}
		SetFlagLocal(Flags.Busy, b: true);
		TimeToWipe = 0f;
		KillAllBillboards();
		DeactivateAllEntrancePortals();
		KillExitPortal();
		DestroyRadVolumes();
		for (int i = 0; i < 3; i++)
		{
			yield return KillAllEntitiesAsync();
		}
		ClearDeepSeaTerrainData();
		BoatAICoordination.WipeCoordination();
		foodTollPaid.Clear();
		if (wipeRadiationVolume != null)
		{
			wipeRadiationVolume.gameObject.SetActive(value: false);
			wipeRadiationVolume.RadiationAmountOverride = 0f;
		}
		if (BaseGameMode.GetActiveGameMode(serverside: true) == null || !BaseGameMode.GetActiveGameMode(serverside: true).blockMapInDeepSea)
		{
			foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
			{
				if (allPlayer != null)
				{
					allPlayer.ServerClearFog(mainland: false, deepSea: true);
				}
			}
		}
		ResetTimeToNextOpening();
		SetFlagLocal(Flags.Busy, b: false);
		Log("All cleared!");
		if (!DeepSea.enabled)
		{
			Kill();
		}
		Facepunch.Rust.Analytics.Azure.OnDeepSeaToggled(toggle: false);
		Interface.CallHook("OnDeepSeaClosed", this);
	}

	public IEnumerator CloseAndOpenDeepSeaAsync()
	{
		yield return new WaitUntil(() => !Rust.Application.isLoadingSave);
		yield return CloseDeepSeaAsync();
		OpenDeepSea();
	}

	public IEnumerator CloseAndDestroyDeepSea()
	{
		yield return new WaitUntil(() => !Rust.Application.isLoadingSave);
		if (IsOpen())
		{
			Log("Deep sea is disabled, cleaning saved entities and closing...");
			CloseDeepSea();
		}
		else
		{
			Log("Deep sea is disabled, killing");
			Kill();
		}
	}

	private IEnumerator KillAllEntitiesAsync()
	{
		HashSet<BaseEntity> entities = Facepunch.Pool.Get<HashSet<BaseEntity>>();
		GetAllDeepSeaEntities(entities);
		foreach (BasePlayer item in entities.OfType<BasePlayer>().ToList())
		{
			if (item.IsDead() || item.IsDestroyed)
			{
				entities.Remove(item);
				continue;
			}
			if (item.IsNpc)
			{
				item.Kill();
			}
			else
			{
				item.Hurt(1000f, DamageType.Radiation);
				if (item.IsAlive())
				{
					PointEntity<DeepSeaManager>.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_ClearDeepSeaShoreData", item));
				}
			}
			entities.Remove(item);
		}
		foreach (BaseEntity item2 in entities)
		{
			if (!(item2 == null) && !item2.IsDestroyed)
			{
				try
				{
					item2.Kill();
				}
				catch (Exception arg)
				{
					Debug.LogError($"DeepSea: Wipe exception when killing entity {item2.PrefabName}\n{arg}");
				}
				yield return null;
			}
		}
		ServerIslands.Clear();
		ServerGhostShips.Clear();
		ServerFloatingCities.Clear();
		ServerRHIBS.Clear();
		Facepunch.Pool.FreeUnmanaged(ref entities);
	}

	public void GetAllDeepSeaEntities(HashSet<BaseEntity> entities, bool skipParented = true)
	{
		AddAll(ServerIslands);
		AddAll(ServerGhostShips);
		AddAll(ServerFloatingCities);
		AddAll(ServerRHIBS);
		if (BaseNetworkable.DeepSeaGroup.networkables != null)
		{
			foreach (Networkable networkable in BaseNetworkable.DeepSeaGroup.networkables)
			{
				BaseEntity ent2 = BaseNetworkable.serverEntities.Find(networkable.ID) as BaseEntity;
				TryAddEntity(ent2);
			}
		}
		if (BaseNetworkable.GlobalNetworkGroup.networkables != null)
		{
			foreach (Networkable networkable2 in BaseNetworkable.GlobalNetworkGroup.networkables)
			{
				BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(networkable2.ID) as BaseEntity;
				if (baseEntity != null && IsInsideDeepSea(baseEntity))
				{
					TryAddEntity(baseEntity);
				}
			}
		}
		if (BaseNetworkable.LimboNetworkGroup.networkables != null)
		{
			foreach (Networkable networkable3 in BaseNetworkable.LimboNetworkGroup.networkables)
			{
				BaseEntity baseEntity2 = BaseNetworkable.serverEntities.Find(networkable3.ID) as BaseEntity;
				if (baseEntity2 != null && IsInsideDeepSea(baseEntity2))
				{
					TryAddEntity(baseEntity2);
				}
			}
		}
		Network.Net.sv.visibility.ForEachGroup(5, ProcessGroup);
		void AddAll(IEnumerable<BaseEntity> list)
		{
			foreach (BaseEntity item in list)
			{
				TryAddEntity(item);
			}
		}
		void ProcessGroup(Group group)
		{
			if (CollectionEx.IsNullOrEmpty(group.networkables))
			{
				return;
			}
			foreach (Networkable networkable4 in group.networkables)
			{
				BaseEntity ent3 = BaseNetworkable.serverEntities.Find(networkable4.ID) as BaseEntity;
				TryAddEntity(ent3);
			}
		}
		void TryAddEntity(BaseEntity ent)
		{
			if (!(ent == null) && ent.isServer && !(ent is DeepSeaManager) && !(ent is DeepSeaPortal) && (!skipParented || !ent.parentEntity.IsValid(serverside: true)))
			{
				entities.Add(ent);
			}
		}
	}

	private void FillVehicleWhitelist()
	{
		foreach (BaseEntity item in vehicleWhitelist)
		{
			if (!(item == null))
			{
				vehiclePrefabWhitelist.Add(item.prefabID);
			}
		}
	}

	private static bool IsVehicleAllowed(BaseEntity standingOnEnt)
	{
		if (DeepSea.allow_all_vehicles)
		{
			return true;
		}
		return PointEntity<DeepSeaManager>.ServerInstance.vehiclePrefabWhitelist.Contains(standingOnEnt.prefabID);
	}

	private void NetworkTreesScale()
	{
		HashSet<TreeEntity> obj = Facepunch.Pool.Get<HashSet<TreeEntity>>();
		GetAllDeepSeaTrees(obj);
		foreach (TreeEntity item in obj)
		{
			item.networkEntityScale = true;
			item.SendNetworkUpdate();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		static void GetAllDeepSeaTrees(HashSet<TreeEntity> trees)
		{
			Network.Net.sv.visibility.ForEachGroup(5, ProcessGroup);
			void ProcessGroup(Group group)
			{
				if (CollectionEx.IsNullOrEmpty(group.networkables))
				{
					return;
				}
				foreach (Networkable networkable in group.networkables)
				{
					TreeEntity treeEntity = BaseNetworkable.serverEntities.Find(networkable.ID) as TreeEntity;
					if (!(treeEntity == null) && treeEntity.isServer)
					{
						trees.Add(treeEntity);
					}
				}
			}
		}
	}

	public static (bool, Translate.Phrase) CanTeleportToMainIsland(BaseEntity entity)
	{
		return IsAllowedInDeepSea(entity, fromMainLand: false);
	}

	public static (bool, Translate.Phrase) CanTeleportToDeepSea(BaseEntity entity)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance != null && PointEntity<DeepSeaManager>.ServerInstance.TimeToWipe <= DeepSea.wipeRadiationPhaseDuration)
		{
			return (false, AboutToClose_Phrase);
		}
		return IsAllowedInDeepSea(entity, fromMainLand: true);
	}

	private static (bool, Translate.Phrase) IsAllowedInDeepSea(BaseEntity entity, bool fromMainLand)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			return (false, null);
		}
		if (entity.HasParent())
		{
			return (false, null);
		}
		if (entity is BasePlayer basePlayer)
		{
			if (basePlayer.IsNpc)
			{
				return (false, null);
			}
			if (basePlayer.isMounted || basePlayer.IsSleeping() || basePlayer.IsReceivingSnapshot)
			{
				return (false, null);
			}
			BaseEntity standingOnEntity = basePlayer.GetStandingOnEntity(134217728);
			if (standingOnEntity != null)
			{
				if (standingOnEntity is BoatBuildingBlock boatBuildingBlock)
				{
					standingOnEntity = boatBuildingBlock.GetParentEntity();
				}
				if (IsVehicleAllowed(standingOnEntity))
				{
					return (false, null);
				}
			}
			if (basePlayer.IsFlying)
			{
				return (true, null);
			}
			if (DeepSea.allow_swimmers)
			{
				return (true, null);
			}
			return (false, fromMainLand ? NeedBoat_ToDeepSea_Phrase : NeedBoat_ToMainLand_Phrase);
		}
		using (PooledList<BasePlayer> pooledList = Facepunch.Pool.Get<PooledList<BasePlayer>>())
		{
			BaseVehicle.GetPassengersForVehicle(entity, pooledList);
			foreach (BasePlayer item in pooledList)
			{
				if (item.IsNpcPlayer())
				{
					return (false, null);
				}
			}
		}
		if (entity is BaseVehicle baseVehicle)
		{
			if (baseVehicle is BaseBoat)
			{
				bool flag = IsVehicleAllowed(baseVehicle);
				if (flag)
				{
					if (baseVehicle is PlayerBoat playerBoat)
					{
						OBB oBB = playerBoat.WorldSpaceBounds();
						float radius = Mathf.Max(oBB.extents.x, oBB.extents.z, oBB.extents.y);
						using PooledList<BaseVehicle> pooledList2 = Facepunch.Pool.Get<PooledList<BaseVehicle>>();
						Vis.Entities(oBB.position, radius, pooledList2, 32768, QueryTriggerInteraction.Ignore);
						foreach (BaseVehicle item2 in pooledList2)
						{
							if (!item2.isClient && (item2 is PlayerHelicopter || item2 is BaseBoat || item2 is BaseSubmarine) && !IsVehicleAllowed(item2))
							{
								return (false, UnauthorizedVehicle_Phrase);
							}
						}
					}
					else
					{
						foreach (BaseEntity child in entity.children)
						{
							if (child is BaseVehicle standingOnEnt && (child is PlayerHelicopter || child is BaseBoat || child is BaseSubmarine) && !IsVehicleAllowed(standingOnEnt))
							{
								return (false, UnauthorizedVehicle_Phrase);
							}
						}
					}
				}
				return (flag, fromMainLand ? BoatNotStrongEnough_ToDeepSea_Phrase : BoatNotStrongEnough_ToMainLand_Phrase);
			}
			return (false, fromMainLand ? NeedBoat_ToDeepSea_Phrase : NeedBoat_ToMainLand_Phrase);
		}
		return (false, null);
	}

	public void RegisterPaidFoodToll(BasePlayer player)
	{
		if (!(player == null))
		{
			foodTollPaid.Add(player.userID);
			SendNetworkUpdate();
		}
	}

	private void DoOpenNotification()
	{
		BasePlayer.Server_SendWorldNotificationToAllActivePlayers(WorldNotificationConfig.NotificationType.DeepSeaOpen, PortalEntranceTransform.position);
	}

	public new static void Log(string message)
	{
		if (DeepSea.logs)
		{
			Debug.Log("DeepSea: " + message);
		}
	}

	public static bool IsRespawnVariant(SpawnGroup spawnGroup)
	{
		return spawnGroup.gameObject.name.EndsWith("_Respawn", StringComparison.Ordinal);
	}

	public static void ApplyPopulationScale(SpawnGroup spawnGroup, float scale)
	{
		if (scale != 1f)
		{
			spawnGroup.maxPopulation = Mathf.Max(0, Mathf.RoundToInt((float)spawnGroup.maxPopulation * scale));
			spawnGroup.numToSpawnPerTickMin = Mathf.Max(0, Mathf.RoundToInt((float)spawnGroup.numToSpawnPerTickMin * scale));
			spawnGroup.numToSpawnPerTickMax = Mathf.Max(0, Mathf.RoundToInt((float)spawnGroup.numToSpawnPerTickMax * scale));
		}
	}

	private IEnumerator GenerateContentAsync()
	{
		placedPoints.Clear();
		float num = Mathf.Min(DeepSea.floatingcity_radius, DeepSea.island_radius, DeepSea.ghostship_radius, DeepSea.rhib_radius);
		cellSize = num / Mathf.Sqrt(2f);
		gridSizeX = Mathf.CeilToInt(DeepSeaBounds.size.x / cellSize);
		gridSizeY = Mathf.CeilToInt(DeepSeaBounds.size.z / cellSize);
		grid = new List<int>[gridSizeX, gridSizeY];
		if (floatingCityRefs.Length != 0)
		{
			shuffledPrefabs = GetShuffledPrefabQueue(floatingCityRefs);
			yield return ScatterPointsAsync(DeepSea.floatingcity_count, DeepSea.floatingcity_radius, DeepSea.floatingcity_edgeMargin, DeepSea.floatingcity_minDist, delegate(Vector2 point)
			{
				GameObjectRef nextPrefab3 = GetNextPrefab(ref shuffledPrefabs, floatingCityRefs);
				Quaternion rotation3 = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
				DeepSeaFloatingCity deepSeaFloatingCity = SpawnEntityAt(nextPrefab3.resourcePath, new Vector3(point.x, 0f, point.y), rotation3) as DeepSeaFloatingCity;
				if (deepSeaFloatingCity != null)
				{
					ServerFloatingCities.TryAdd(deepSeaFloatingCity);
				}
			});
		}
		Log($"Spawned {ServerFloatingCities.Count} floating cities");
		shuffledPrefabs = GetShuffledPrefabQueue(islandRefs);
		yield return ScatterPointsAsync(DeepSea.island_count, DeepSea.island_radius, DeepSea.island_edgeMargin, DeepSea.island_minDist, delegate(Vector2 point)
		{
			GameObjectRef nextPrefab2 = GetNextPrefab(ref shuffledPrefabs, islandRefs);
			Quaternion rotation2 = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
			DeepSeaIsland deepSeaIsland = SpawnEntityAt(nextPrefab2.resourcePath, new Vector3(point.x, 0f, point.y), rotation2) as DeepSeaIsland;
			if (deepSeaIsland != null)
			{
				ServerIslands.TryAdd(deepSeaIsland);
			}
		});
		Log($"Spawned {ServerIslands.Count} islands");
		if (ghostShipRefs.Length != 0)
		{
			shuffledPrefabs = GetShuffledPrefabQueue(ghostShipRefs);
			yield return ScatterPointsAsync(DeepSea.ghostship_count, DeepSea.ghostship_radius, DeepSea.ghostship_edgeMargin, DeepSea.ghostship_minDist, delegate(Vector2 point)
			{
				GameObjectRef nextPrefab = GetNextPrefab(ref shuffledPrefabs, ghostShipRefs);
				Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
				GhostShip ghostShip = SpawnEntityAt(nextPrefab.resourcePath, new Vector3(point.x, 0f, point.y), rotation) as GhostShip;
				if (ghostShip != null)
				{
					ServerGhostShips.TryAdd(ghostShip);
				}
			});
		}
		Log($"Spawned {ServerGhostShips.Count} ghost ships");
		if (AI.scientist_spawners_enabled)
		{
			yield return ScatterPointsAsync(DeepSea.rhib_count, DeepSea.rhib_radius, DeepSea.rhib_edgeMargin, DeepSea.rhib_minDist, delegate(Vector2 pos)
			{
				Quaternion rot = Quaternion.LookRotation((DeepSeaBounds.center - new Vector3(pos.x, 0f, pos.y)).normalized);
				BoatAI.SpawnBoatGroup(pos, rot, null, registerWithDeepSea: true);
				Log($"Spawning boat AI group at {pos}");
			});
			Log($"Spawned {ServerRHIBS.Count} boat AI groups");
		}
		else
		{
			Debug.Log("Skipping Scientist Boat spawning: ai.scientist_spawners_enabled is false");
		}
	}

	private IEnumerator ScatterPointsAsync(int count, float radius, float margin, float minDist, Action<Vector2> onPlaced)
	{
		int startCount = placedPoints.Count;
		int maxAttempts = count * 50;
		for (int i = 0; i < maxAttempts; i++)
		{
			if (placedPoints.Count >= startCount + count)
			{
				break;
			}
			Vector2 randomPosInBounds = GetRandomPosInBounds(margin);
			if (IsValidPosition(randomPosInBounds, radius, minDist, startCount))
			{
				int count2 = placedPoints.Count;
				placedPoints.Add(new PlacedPoint(randomPosInBounds, radius));
				AddToGrid(randomPosInBounds, count2);
				onPlaced(randomPosInBounds);
				yield return WaitEntitySpawnInterval;
			}
		}
	}

	private bool IsValidPosition(Vector2 candidatePos, float candidateRadius, float minDist, int startCount)
	{
		Vector2Int vector2Int = ToCell(candidatePos);
		int num = Mathf.CeilToInt(Mathf.Max(candidateRadius, minDist) / cellSize);
		int num2 = Mathf.Max(0, vector2Int.x - num);
		int num3 = Mathf.Min(gridSizeX - 1, vector2Int.x + num);
		int num4 = Mathf.Max(0, vector2Int.y - num);
		int num5 = Mathf.Min(gridSizeY - 1, vector2Int.y + num);
		for (int i = num4; i <= num5; i++)
		{
			for (int j = num2; j <= num3; j++)
			{
				List<int> list = grid[j, i];
				if (list == null)
				{
					continue;
				}
				foreach (int item in list)
				{
					PlacedPoint placedPoint = placedPoints[item];
					float num6 = ((item < startCount || minDist == 0f) ? Mathf.Max(placedPoint.radius, candidateRadius) : Mathf.Max(minDist, Mathf.Max(placedPoint.radius, candidateRadius)));
					if ((candidatePos - placedPoint.pos).sqrMagnitude < num6 * num6)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void SpawnGhostShipHackableCrate()
	{
		int num = Mathf.Min(DeepSea.hackablecrate_count, ServerGhostShips.Count);
		if (num > 0)
		{
			List<GhostShip> obj = Facepunch.Pool.Get<List<GhostShip>>();
			obj.AddRange(ServerGhostShips.Values);
			obj.Shuffle((uint)UnityEngine.Random.Range(0, 1000));
			for (int i = 0; i < num; i++)
			{
				obj[i].SpawnHackableLockedCrate();
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
	}

	private void AddToGrid(Vector2 point, int index)
	{
		Vector2Int vector2Int = ToCell(point);
		if (grid[vector2Int.x, vector2Int.y] == null)
		{
			grid[vector2Int.x, vector2Int.y] = new List<int>(32);
		}
		grid[vector2Int.x, vector2Int.y].Add(index);
	}

	private Vector2Int ToCell(Vector2 point)
	{
		float f = (point.x - DeepSeaBounds.min.x) / cellSize;
		return new Vector2Int(y: Mathf.FloorToInt((point.y - DeepSeaBounds.min.z) / cellSize), x: Mathf.FloorToInt(f));
	}

	private Vector2 GetRandomPosInBounds(float edgeMargin)
	{
		float x = UnityEngine.Random.Range(DeepSeaBounds.min.x + edgeMargin, DeepSeaBounds.max.x - edgeMargin);
		float y = UnityEngine.Random.Range(DeepSeaBounds.min.z + edgeMargin, DeepSeaBounds.max.z - edgeMargin);
		return new Vector2(x, y);
	}

	private Queue<GameObjectRef> GetShuffledPrefabQueue(GameObjectRef[] refs)
	{
		return new Queue<GameObjectRef>(refs.OrderBy((GameObjectRef _) => UnityEngine.Random.value));
	}

	private GameObjectRef GetNextPrefab(ref Queue<GameObjectRef> queue, GameObjectRef[] source)
	{
		if (queue.Count > 0)
		{
			return queue.Dequeue();
		}
		return ArrayEx.GetRandom(source);
	}

	public void RegisterRHIBs(HashSet<RHIB> set)
	{
		ServerRHIBS.AddRange(set);
	}

	public BaseEntity SpawnEntityAt(string prefabPath, Vector3 position, Quaternion rotation)
	{
		Log($"Spawning {prefabPath} at {position}");
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabPath, position, rotation);
		if (baseEntity == null)
		{
			return null;
		}
		baseEntity.Spawn();
		if (baseEntity.TryGetComponent<EntityParentSettings>(out var component))
		{
			component.TryDetachChildren(baseEntity);
		}
		baseEntity.UpdateNetworkGroup();
		return baseEntity;
	}

	private IEnumerator TriggerSpawnGroups()
	{
		Log("Populating spawn groups...");
		HashSet<BaseEntity> entities = Facepunch.Pool.Get<HashSet<BaseEntity>>();
		PointEntity<DeepSeaManager>.ServerInstance.GetAllDeepSeaEntities(entities, skipParented: false);
		foreach (BaseEntity item in entities)
		{
			if (item is IDeepSeaSpawner deepSeaSpawner)
			{
				yield return deepSeaSpawner.TriggerSpawnGroups();
				yield return WaitEntitySpawnInterval;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref entities);
	}

	private IEnumerator GenerateIslandNavMeshes()
	{
		Log("Generating islands nav meshes...");
		for (int i = 0; i < ServerIslands.Count; i++)
		{
			DeepSeaIsland deepSeaIsland = ServerIslands[i];
			if (deepSeaIsland == null || deepSeaIsland.monumentNavMesh == null)
			{
				continue;
			}
			if (AI.move)
			{
				yield return deepSeaIsland.StartCoroutine(deepSeaIsland.UpdateNavMesh());
				if (AI.useUnityNavmesh)
				{
					yield return WaitNavMeshInterval;
				}
			}
			else
			{
				deepSeaIsland.GenerateNavMesh();
			}
		}
	}

	private void RestoreIslandNavMeshes()
	{
		Log("Dwellings loaded from save");
		StartCoroutine(GenerateIslandNavMeshes());
	}

	public void CacheWaitForSeconds(bool fast = false)
	{
		if (fast)
		{
			WaitEntitySpawnInterval = new WaitForSeconds(1f);
			WaitSpawnGroupInterval = new WaitForSeconds(0.1f);
			WaitNavMeshInterval = new WaitForSeconds(1f);
		}
		else
		{
			WaitEntitySpawnInterval = new WaitForSeconds(DeepSea.entities_spawninterval);
			WaitSpawnGroupInterval = new WaitForSeconds(DeepSea.spawngroups_spawninterval);
			WaitNavMeshInterval = new WaitForSeconds(DeepSea.navmesh_spawninterval);
		}
	}

	public float GetWipeWeatherLerp()
	{
		float wipeEndProgress = GetWipeEndProgress();
		return EndWipeProgressToWeatherLerp.Evaluate(wipeEndProgress);
	}

	public static DeepSeaManager Get(bool server)
	{
		if (server)
		{
			return PointEntity<DeepSeaManager>.ServerInstance;
		}
		return null;
	}

	public override void InitShared()
	{
		base.InitShared();
		CreateColliders();
		CreateSeaFloor();
		bounds.center = Vector3.zero;
		bounds.size = DeepSeaBounds.size;
	}

	private void CreateColliders()
	{
		if (sharedCollidersParent != null)
		{
			UnityEngine.Object.Destroy(sharedCollidersParent);
			sharedCollidersParent = null;
		}
		if (sharedCollidersParent == null)
		{
			sharedCollidersParent = new GameObject("Colliders").transform;
			sharedCollidersParent.SetParent(base.transform, worldPositionStays: false);
		}
		CreateCollider("DeepSea_Left", new Vector3(DeepSeaBounds.size.x / -2f, 0f, 0f), new Vector3(1f, DeepSeaBounds.size.y, DeepSeaBounds.size.z), sharedCollidersParent);
		CreateCollider("DeepSea_Right", new Vector3(DeepSeaBounds.size.x / 2f, 0f, 0f), new Vector3(1f, DeepSeaBounds.size.y, DeepSeaBounds.size.z), sharedCollidersParent);
		CreateCollider("DeepSea_Back", new Vector3(0f, 0f, DeepSeaBounds.size.z / -2f), new Vector3(DeepSeaBounds.size.x, DeepSeaBounds.size.y, 1f), sharedCollidersParent);
		CreateCollider("DeepSea_Front", new Vector3(0f, 0f, DeepSeaBounds.size.z / 2f), new Vector3(DeepSeaBounds.size.x, DeepSeaBounds.size.y, 1f), sharedCollidersParent);
	}

	private BoxCollider CreateCollider(string colName, Vector3 pos, Vector3 size, Transform parent, bool worldPos = false)
	{
		GameObject obj = new GameObject(colName);
		obj.transform.SetParent(parent, worldPositionStays: false);
		BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
		boxCollider.size = size;
		if (worldPos)
		{
			boxCollider.transform.position = pos;
		}
		else
		{
			boxCollider.center = pos;
		}
		return boxCollider;
	}

	private void CreateSeaFloor()
	{
		Vector3 localPosition = new Vector3(0f, SeaFloorDepth, 0f);
		Vector3 localScale = new Vector3(DeepSeaBounds.size.x, 1f, DeepSeaBounds.size.z);
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
		obj.name = "DeepSea_Bottom";
		obj.transform.SetParent(sharedCollidersParent.transform, worldPositionStays: false);
		obj.transform.localPosition = localPosition;
		obj.transform.localScale = localScale;
		TransformEx.SetLayerRecursive(obj, 16);
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		component.sharedMaterial = SeaFloorMaterial;
		component.shadowCastingMode = ShadowCastingMode.Off;
	}

	public static bool UseDeepSeaVisuals(Vector3 position)
	{
		if (!DeepSea.terrain_everywhere)
		{
			return IsInsideDeepSea(position);
		}
		return true;
	}

	public static bool UseDeepSeaVisuals(BaseNetworkable entity)
	{
		return UseDeepSeaVisuals(entity.transform.position);
	}

	public static bool IsInsideDeepSea(Vector3 position)
	{
		return DeepSeaBounds.Contains(position);
	}

	public static bool IsInsideDeepSea(Bounds bounds)
	{
		return DeepSeaBounds.Intersects(bounds);
	}

	public static bool IsInsideDeepSea(BaseNetworkable entity)
	{
		return IsInsideDeepSea(entity.transform.position);
	}

	public float GetTimeToWipe()
	{
		return TimeToWipe;
	}

	public float GetWipeEndProgress()
	{
		if (TimeToWipe == 0f || TimeToWipe > DeepSea.wipeEndPhaseDuration)
		{
			return 0f;
		}
		return 1f - Mathf.Clamp01(TimeToWipe / DeepSea.wipeEndPhaseDuration);
	}

	public float GetWipeRadiationAmount()
	{
		if (!(wipeRadiationVolume != null))
		{
			return 0f;
		}
		return wipeRadiationVolume.RadiationAmountOverride;
	}

	public static CardinalDirection GetEntrancePortalDirection()
	{
		DeepSeaManager deepSeaManager = null;
		deepSeaManager = PointEntity<DeepSeaManager>.ServerInstance;
		if (deepSeaManager != null)
		{
			return (CardinalDirection)deepSeaManager.CurrentEntrancePortalDirection;
		}
		return CardinalDirection.None;
	}

	public void SetTimeToWipe(float time)
	{
		TimeToWipe = time;
	}

	public void SetTimeToNextOpening(float time)
	{
		TimeToNextOpening = time;
	}

	private void ResetTimeToNextOpening()
	{
		float wipeCooldownMin = DeepSea.wipeCooldownMin;
		float maxInclusive = Mathf.Max(wipeCooldownMin, DeepSea.wipeCooldownMax);
		SetTimeToNextOpening(UnityEngine.Random.Range(wipeCooldownMin, maxInclusive));
	}

	public bool HasPaidFoodToll(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (base.isServer)
		{
			return foodTollPaid.Contains(player.userID);
		}
		return false;
	}

	public static Vector3 NormalizePosInDeepSea(Vector3 worldPos)
	{
		worldPos.x = Mathx.RemapValClamped(worldPos.x, DeepSeaBounds.min.x, DeepSeaBounds.max.x, 0f, 1f);
		worldPos.z = Mathx.RemapValClamped(worldPos.z, DeepSeaBounds.min.z, DeepSeaBounds.max.z, 0f, 1f);
		worldPos.y = 0f;
		return worldPos;
	}

	public static float NormalizeX(float x)
	{
		return (x - DeepSeaBounds.min.x) / DeepSeaBounds.size.x;
	}

	public static float NormalizeZ(float z)
	{
		return (z - DeepSeaBounds.min.z) / DeepSeaBounds.size.z;
	}

	public bool IsAccessible()
	{
		if (IsOpen())
		{
			return !HasFlag(Flags.Reserved1);
		}
		return false;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (old & Flags.Open) == Flags.Open;
		bool flag2 = (next & Flags.Open) == Flags.Open;
		bool flag3 = flag2 && (next & Flags.Reserved1) != Flags.Reserved1;
		if (base.isServer && flag != flag2)
		{
			NPCVendingMachine.RefreshDeepSeaMapMarkers();
		}
	}

	public static void ClearDeepSeaTerrainData()
	{
		if ((bool)TerrainMeta.Texturing)
		{
			TerrainMeta.Texturing.ClearDeepSeaData();
		}
		if ((bool)TerrainMeta.HeightMap)
		{
			TerrainMeta.HeightMap.ResetDeepSeaToFloor();
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.deepSeaManager = Facepunch.Pool.Get<ProtoBuf.DeepSeaManager>();
		if (info.forDisk)
		{
			info.msg.deepSeaManager.foodPaid = Facepunch.Pool.Get<List<ulong>>();
			info.msg.deepSeaManager.foodPaid.AddRange(foodTollPaid);
			return;
		}
		BasePlayer basePlayer = info.forConnection.player as BasePlayer;
		if (basePlayer != null && HasPaidFoodToll(basePlayer))
		{
			info.msg.deepSeaManager.localPlayerPaidFoodToll = true;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.deepSeaManager == null || !base.isServer)
		{
			return;
		}
		if (info.fromDisk && IsBusy())
		{
			if (TimeToWipe <= 0f)
			{
				Log("Loaded save with deep sea closing, closing again...");
				CloseDeepSea(forceClose: true);
			}
			else if (TimeToNextOpening <= 0f)
			{
				Log("Loaded save with deep sea opening, closing and reopening...");
				StartCoroutine(CloseAndOpenDeepSeaAsync());
			}
		}
		foodTollPaid.Clear();
		if (info.msg.deepSeaManager.foodPaid != null)
		{
			foodTollPaid.AddRange(info.msg.deepSeaManager.foodPaid);
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 0:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: TimeToWipe for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_TimeToWipe);
			return true;
		case 1:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: TimeToNextOpening for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_TimeToNextOpening);
			return true;
		case 2:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: CurrentEntrancePortalDirection for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_CurrentEntrancePortalDirection);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_TimeToWipe;
				float _sync_TimeToWipe = reader.Float();
				__sync_TimeToWipe = _sync_TimeToWipe;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_TimeToNextOpening;
				float _sync_TimeToNextOpening = reader.Float();
				__sync_TimeToNextOpening = _sync_TimeToNextOpening;
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_CurrentEntrancePortalDirection;
				int _sync_CurrentEntrancePortalDirection = reader.Int32();
				__sync_CurrentEntrancePortalDirection = _sync_CurrentEntrancePortalDirection;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"TimeToWipe" => 0, 
			"TimeToNextOpening" => 1, 
			"CurrentEntrancePortalDirection" => 2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite obj = Network.Net.sv.StartWrite();
		WriteAutoSaveSyncVars(obj);
		var (src, num) = obj.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Facepunch.Pool.Free(ref obj);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead obj = Facepunch.Pool.Get<NetRead>();
			obj.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(obj);
			Facepunch.Pool.Free(ref obj);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_TimeToWipe = 0f;
		__sync_TimeToNextOpening = 0f;
		__sync_CurrentEntrancePortalDirection = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
