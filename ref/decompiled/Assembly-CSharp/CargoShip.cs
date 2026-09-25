using System;
using System.Collections.Generic;
using System.Text;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class CargoShip : BaseEntity, ILargeVehicleForProjectiles
{
	[Serializable]
	public struct CargoshipCamera
	{
		public Transform cameraPoint;

		public Transform cameraLookAtPoint;

		public string cameraIdentifier;
	}

	public struct HarborInfo
	{
		public BasePath harborPath;

		public Transform harborTransform;

		public int approachNode;
	}

	public int targetNodeIndex = -1;

	public GameObject wakeParent;

	public GameObjectRef scientistTurretPrefab;

	public Transform[] scientistSpawnPoints;

	public List<Transform> crateSpawns;

	public GameObjectRef lockedCratePrefab;

	public GameObjectRef militaryCratePrefab;

	public GameObjectRef eliteCratePrefab;

	public GameObjectRef junkCratePrefab;

	public Transform waterLine;

	public Transform rudder;

	public Transform propeller;

	public GameObjectRef escapeBoatPrefab;

	public GameObjectRef primitiveEscapeBoatPrefab;

	public Transform escapeBoatPoint;

	public GameObjectRef microphonePrefab;

	public Transform microphonePoint;

	public GameObjectRef speakerPrefab;

	public Transform[] speakerPoints;

	public GameObject radiation;

	public GameObjectRef mapMarkerEntityPrefab;

	public GameObject hornOrigin;

	public SoundDefinition hornDef;

	public CargoShipSounds cargoShipSounds;

	public GameObject[] layouts;

	public GameObjectRef playerTest;

	public Transform bowPoint;

	private uint layoutChoice;

	public const Flags IsDocked = Flags.Reserved1;

	public const Flags HasDocked = Flags.Reserved2;

	public const Flags DockedHarborIndex0 = Flags.Reserved3;

	public const Flags DockedHarborIndex1 = Flags.Reserved4;

	public const Flags Egressing = Flags.Reserved8;

	public GameObjectRef cctvCameraPrefab;

	public CargoshipCamera[] cctvCameras;

	[ServerVar]
	public static bool docking_debug = false;

	[ServerVar]
	public static bool should_dock = true;

	[ServerVar]
	public static float dock_time = 480f;

	[ServerVar]
	public static bool event_enabled = true;

	[ServerVar]
	public static float event_duration_minutes = 50f;

	[ServerVar]
	public static float egress_duration_minutes = 10f;

	[ServerVar]
	public static int loot_rounds = 3;

	[ServerVar]
	public static float loot_round_spacing_minutes = 10f;

	[ServerVar]
	public static bool refresh_loot_on_dock = true;

	[ServerVar]
	public static bool cargo_escape_boat_rhib = true;

	public static List<HarborInfo> harbors = new List<HarborInfo>();

	public int currentHarborApproachNode;

	public int harborIndex;

	public bool isDoingHarborApproach;

	private int dockCount;

	private bool shouldLookAhead;

	private float lifetime;

	private List<int> availableCrateSpawnIndices = new List<int>();

	private CargoShipContainerDestination[] containerDestinations;

	private HashSet<ulong> boardedPlayerIds = new HashSet<ulong>();

	public static bool hasCalculatedApproaches = false;

	public BaseEntity mapMarkerInstance;

	public Vector3 currentVelocity = Vector3.zero;

	public float currentThrottle;

	public float currentTurnSpeed;

	public float turnScale;

	public int lootRoundsPassed;

	private List<CCTV_RC> cameraEntities;

	public int hornCount;

	public float currentRadiation;

	public bool egressing;

	public BasePath harborApproachPath;

	public HarborProximityManager proxManager;

	private float lastSpeed = 0.3f;

	public bool IsShipDocked => HasFlag(Flags.Reserved1);

	public static int TotalAvailableHarborDockingPaths => harbors.Count;

	private bool HasFinishedDocking
	{
		get
		{
			if (!should_dock)
			{
				return true;
			}
			return dockCount == harbors.Count;
		}
	}

	private float EventTimeRemaining => event_duration_minutes * 60f - lifetime;

	[ServerVar]
	public static void egress(ConsoleSystem.Arg arg)
	{
		CargoShip[] array = UnityEngine.Object.FindObjectsByType<CargoShip>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (CargoShip cargoShip in array)
		{
			if (cargoShip.isServer && !cargoShip.egressing)
			{
				cargoShip.StartEgress();
			}
		}
	}

	public static List<Vector3> GetCargoApproachPath(int index)
	{
		if (index >= TotalAvailableHarborDockingPaths)
		{
			return null;
		}
		CalculateHarborApproachNodes();
		List<Vector3> list = new List<Vector3>();
		list.Add(TerrainMeta.Path.OceanPatrolFar[harbors[index].approachNode]);
		foreach (BasePathNode node in harbors[index].harborPath.nodes)
		{
			list.Add(node.Position);
		}
		return list;
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	[ServerVar]
	public static void debug_info(ConsoleSystem.Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Harbor Positions:");
		for (int i = 0; i < harbors.Count; i++)
		{
			stringBuilder.AppendLine($"harbor {i} is at {harbors[i].harborTransform.position.x}, {harbors[i].harborTransform.position.y}, {harbors[i].harborTransform.position.z}, approach index: {harbors[i].approachNode}");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar]
	public static void debug_cargo_status(ConsoleSystem.Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is CargoShip cargoShip)
			{
				stringBuilder.AppendLine("Cargoship States:");
				stringBuilder.AppendLine("");
				stringBuilder.AppendLine($"Cargoship [{num}] dump");
				stringBuilder.AppendLine($"is at [{cargoShip.transform.position}]");
				stringBuilder.AppendLine($"dock count [{cargoShip.dockCount}]");
				stringBuilder.AppendLine($"is docked [{cargoShip.IsShipDocked}]");
				stringBuilder.AppendLine($"current approach node [{cargoShip.currentHarborApproachNode}]");
				stringBuilder.AppendLine($"is doing approach [{cargoShip.isDoingHarborApproach}]");
				stringBuilder.AppendLine($"chosen harbor is [{cargoShip.harborIndex}]");
				stringBuilder.AppendLine($"is egressing: {cargoShip.egressing}");
				arg.ReplyWith(stringBuilder.ToString());
				stringBuilder.Clear();
				num++;
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.cargoShip == null)
		{
			return;
		}
		layoutChoice = info.msg.cargoShip.layout;
		if (!base.isServer)
		{
			return;
		}
		isDoingHarborApproach = info.msg.cargoShip.isDoingHarborApproach;
		harborIndex = info.msg.cargoShip.harborIndex;
		CalculateHarborApproachNodes();
		if (isDoingHarborApproach && HasFlag(Flags.Reserved1))
		{
			Invoke(LeaveHarbor, dock_time);
			Invoke(PreHarborLeaveHorn, dock_time - 60f);
		}
		currentHarborApproachNode = info.msg.cargoShip.currentHarborApproachNode;
		dockCount = info.msg.cargoShip.dockCount;
		shouldLookAhead = info.msg.cargoShip.shouldLookAhead;
		lifetime = info.msg.cargoShip.lifetime;
		lootRoundsPassed = info.msg.cargoShip.lootRoundsPassed;
		if (info.msg.cargoShip.isEgressing)
		{
			StartEgress();
		}
		if (HasFinishedDocking)
		{
			Invoke(StartEgress, Mathf.Max(EventTimeRemaining, 120f));
		}
		if (lootRoundsPassed < loot_rounds)
		{
			float num = loot_round_spacing_minutes * 60f;
			float time = num - Mathf.Repeat(lifetime, num);
			InvokeRepeating(RespawnLoot, time, 60f * loot_round_spacing_minutes);
		}
		if (HasFlag(Flags.Reserved1) && !IsInvoking(LeaveHarbor))
		{
			Invoke(LeaveHarbor, dock_time);
		}
		boardedPlayerIds.Clear();
		foreach (ulong playerId in info.msg.cargoShip.playerIds)
		{
			boardedPlayerIds.Add(playerId);
		}
		if (info.msg.cargoShip.availableCrateSpawnIndices != null)
		{
			availableCrateSpawnIndices.Clear();
			availableCrateSpawnIndices.AddRange(info.msg.cargoShip.availableCrateSpawnIndices);
		}
	}

	public void RefreshActiveLayout()
	{
		for (int i = 0; i < layouts.Length; i++)
		{
			layouts[i].SetActive(layoutChoice == i);
		}
		if (base.isServer)
		{
			containerDestinations = GetComponentsInChildren<CargoShipContainerDestination>();
		}
	}

	public static void RegisterHarbor(BasePath path, Transform tf)
	{
		harbors.Add(new HarborInfo
		{
			harborPath = path,
			harborTransform = tf
		});
		if (docking_debug)
		{
			Debug.Log("Added " + tf.name + " to harbor list");
		}
	}

	public void TriggeredEventSpawn()
	{
		Vector3 vector = TerrainMeta.RandomPointOffshore(avoidDeepSeaPortal: true, avoidDeepSea: true);
		vector.y = WaterLevel.GetWaterSurface(vector, waves: false, volumes: false);
		base.transform.position = vector;
		if (should_dock)
		{
			CalculateHarborApproachNodes();
		}
		if (!event_enabled || event_duration_minutes == 0f)
		{
			Invoke(DelayedDestroy, 1f);
		}
	}

	public void TriggeredEventSpawnDockingTest(int index)
	{
		if (harbors.Count <= 0 || !should_dock)
		{
			TriggeredEventSpawn();
			Debug.Log("No harbors registered.");
			return;
		}
		if (harbors.Count <= 0 || index > harbors.Count - 1)
		{
			Debug.Log("Wrong harbor index or no harbors on map.");
			return;
		}
		CalculateHarborApproachNodes();
		if (harbors.Count > 0)
		{
			if (harbors != null)
			{
				int approachNode = harbors[index].approachNode;
				Vector3 vector = TerrainMeta.Path.OceanPatrolFar[approachNode + 5];
				vector.y = WaterLevel.GetWaterSurface(vector, waves: false, volumes: false);
				base.transform.position = vector;
				base.transform.LookAt(harbors[index].harborPath.nodes[0].Position);
			}
			if (!event_enabled || event_duration_minutes == 0f)
			{
				Invoke(DelayedDestroy, 1f);
			}
		}
	}

	private static void CalculateHarborApproachNodes()
	{
		if (hasCalculatedApproaches)
		{
			return;
		}
		hasCalculatedApproaches = true;
		for (int i = 0; i < harbors.Count; i++)
		{
			HarborInfo value = harbors[i];
			float num = float.MaxValue;
			int num2 = -1;
			for (int j = 0; j < TerrainMeta.Path.OceanPatrolFar.Count; j++)
			{
				Vector3 vector = TerrainMeta.Path.OceanPatrolFar[j];
				Vector3 position = value.harborPath.nodes[0].Position;
				float num3 = Vector3.Distance(vector, position);
				_ = docking_debug;
				float num4 = num3;
				Vector3 vector2 = Vector3.up * 3f;
				if (!GamePhysics.LineOfSightRadius(vector + vector2, position + vector2, 1084293377, 3f))
				{
					num4 *= 20f;
				}
				if (num4 < num)
				{
					num = num4;
					num2 = j;
				}
			}
			if (num2 == -1)
			{
				Debug.LogWarning("Cargo couldn't find harbor approach node. Are you sure ocean paths have been generated?");
				break;
			}
			value.approachNode = num2;
			harbors[i] = value;
		}
	}

	public void OnArrivedAtHarbor()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
		List<Transform> obj = Pool.Get<List<Transform>>();
		float num = UnityEngine.Random.Range(dock_time * 0.05f, dock_time * 0.1f);
		foreach (HarborCraneContainerPickup allCrane in HarborCraneContainerPickup.AllCranes)
		{
			if (allCrane == null || allCrane.isClient || allCrane.Distance2D(this) > 150f)
			{
				continue;
			}
			obj.Clear();
			CargoShipContainerDestination[] array = containerDestinations;
			foreach (CargoShipContainerDestination cargoShipContainerDestination in array)
			{
				if (allCrane.IsDestinationValidForCrane(cargoShipContainerDestination))
				{
					obj.Add(cargoShipContainerDestination.transform);
				}
			}
			if (obj.Count > 0)
			{
				allCrane.AssignDestination(obj, this, num);
				num += dock_time * UnityEngine.Random.Range(0.1f, 0.15f);
			}
		}
		Pool.FreeUnmanaged(ref obj);
		Invoke(PreHarborLeaveHorn, dock_time - 60f);
		if (refresh_loot_on_dock)
		{
			RespawnLoot();
		}
		if (harborIndex == 0)
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		else if (harborIndex == 1)
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
		}
		Invoke(LeaveHarbor, dock_time);
		Interface.CallHook("OnCargoShipHarborArrived", this);
	}

	private void ClearAllHarborEntitiesOnShip()
	{
		List<BaseEntity> obj = Pool.Get<List<BaseEntity>>();
		foreach (BaseEntity child in children)
		{
			if (child is CargoShipContainer)
			{
				obj.Add(child);
			}
		}
		foreach (BaseEntity item in obj)
		{
			item.Kill();
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public void CreateMapMarker()
	{
		if ((bool)mapMarkerInstance)
		{
			mapMarkerInstance.Kill();
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerEntityPrefab.resourcePath, Vector3.zero, Quaternion.identity);
		baseEntity.Spawn();
		baseEntity.SetParent(this);
		mapMarkerInstance = baseEntity;
	}

	public void DisableCollisionTest()
	{
	}

	public void SpawnCrate(string resourcePath)
	{
		if (availableCrateSpawnIndices.Count == 0)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, availableCrateSpawnIndices.Count);
		int index2 = availableCrateSpawnIndices[index];
		Vector3 position = crateSpawns[index2].position;
		Quaternion rotation = crateSpawns[index2].rotation;
		availableCrateSpawnIndices.RemoveAt(index);
		BaseEntity baseEntity = GameManager.server.CreateEntity(resourcePath, position, rotation);
		if ((bool)baseEntity)
		{
			baseEntity.enableSaving = true;
			baseEntity.SendMessage("SetWasDropped", SendMessageOptions.DontRequireReceiver);
			baseEntity.Spawn();
			baseEntity.SetParent(this, worldPositionStays: true);
			Rigidbody component = baseEntity.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = true;
			}
		}
	}

	public void RespawnLoot()
	{
		if (Interface.CallHook("OnCargoShipSpawnCrate", this) == null)
		{
			InvokeRepeating(PlayHorn, 0f, 8f);
			SpawnCrate(lockedCratePrefab.resourcePath);
			SpawnCrate(eliteCratePrefab.resourcePath);
			for (int i = 0; i < 4; i++)
			{
				SpawnCrate(militaryCratePrefab.resourcePath);
			}
			for (int j = 0; j < 4; j++)
			{
				SpawnCrate(junkCratePrefab.resourcePath);
			}
			lootRoundsPassed++;
			if (lootRoundsPassed >= loot_rounds)
			{
				CancelInvoke(RespawnLoot);
			}
		}
	}

	public void SpawnSubEntities()
	{
		if (!Rust.Application.isLoadingSave)
		{
			string strPrefab = (cargo_escape_boat_rhib ? escapeBoatPrefab.resourcePath : primitiveEscapeBoatPrefab.resourcePath);
			BaseEntity baseEntity = GameManager.server.CreateEntity(strPrefab, escapeBoatPoint.position, escapeBoatPoint.rotation);
			if ((bool)baseEntity)
			{
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
				RHIB component = baseEntity.GetComponent<RHIB>();
				component.SetToKinematic();
				if ((bool)component)
				{
					component.AddFuel(50);
				}
			}
		}
		MicrophoneStand microphoneStand = GameManager.server.CreateEntity(microphonePrefab.resourcePath, microphonePoint.position, microphonePoint.rotation) as MicrophoneStand;
		if ((bool)microphoneStand)
		{
			microphoneStand.enableSaving = false;
			microphoneStand.SetParent(this, worldPositionStays: true);
			microphoneStand.Spawn();
			microphoneStand.SpawnChildEntity();
			IOEntity iOEntity = microphoneStand.ioEntity.Get(serverside: true);
			Transform[] array = speakerPoints;
			foreach (Transform transform in array)
			{
				IOEntity iOEntity2 = GameManager.server.CreateEntity(speakerPrefab.resourcePath, transform.position, transform.rotation) as IOEntity;
				iOEntity2.enableSaving = false;
				iOEntity2.SetParent(this, worldPositionStays: true);
				iOEntity2.Spawn();
				iOEntity.outputs[0].connectedTo.Set(iOEntity2);
				iOEntity2.inputs[0].connectedTo.Set(iOEntity);
				iOEntity = iOEntity2;
			}
			microphoneStand.ioEntity.Get(serverside: true).MarkDirtyForceUpdateOutputs();
		}
		cameraEntities = Pool.Get<List<CCTV_RC>>();
		CargoshipCamera[] array2 = cctvCameras;
		for (int i = 0; i < array2.Length; i++)
		{
			CargoshipCamera cargoshipCamera = array2[i];
			CCTV_RC cCTV_RC = GameManager.server.CreateEntity(cctvCameraPrefab.resourcePath, cargoshipCamera.cameraPoint.position, cargoshipCamera.cameraPoint.rotation) as CCTV_RC;
			if ((bool)cCTV_RC)
			{
				cCTV_RC.enableSaving = false;
				cCTV_RC.SetParent(this, worldPositionStays: true);
				cCTV_RC.Spawn();
				cCTV_RC.rcIdentifier = cargoshipCamera.cameraIdentifier;
				cCTV_RC.SetLookingAtPoint(cargoshipCamera.cameraLookAtPoint.position);
				cameraEntities.Add(cCTV_RC);
			}
		}
	}

	internal override void DoServerDestroy()
	{
		Pool.FreeUnmanaged(ref cameraEntities);
		base.DoServerDestroy();
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (!base.isServer)
		{
			return;
		}
		if (Rust.Application.isLoadingSave && child is RHIB rHIB)
		{
			Vector3 localPosition = rHIB.transform.localPosition;
			Vector3 b = base.transform.InverseTransformPoint(escapeBoatPoint.transform.position);
			if (Vector3.Distance(localPosition, b) < 1f)
			{
				rHIB.SetToKinematic();
			}
		}
		if (Rust.Application.isLoadingSave)
		{
			return;
		}
		List<BasePlayer> obj = Pool.Get<List<BasePlayer>>();
		child.GetComponentsInChildren(obj);
		foreach (BasePlayer item in obj)
		{
			if (!item.IsBot && !item.IsNpc && item.IsConnected && boardedPlayerIds.Add(item.userID) && item.serverClan != null)
			{
				item.AddClanScore(ClanScoreEventType.ReachedCargoShip);
			}
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.cargoShip = Pool.Get<ProtoBuf.CargoShip>();
		info.msg.cargoShip.layout = layoutChoice;
		info.msg.cargoShip.currentHarborApproachNode = currentHarborApproachNode;
		info.msg.cargoShip.isDoingHarborApproach = isDoingHarborApproach;
		info.msg.cargoShip.dockCount = dockCount;
		info.msg.cargoShip.shouldLookAhead = shouldLookAhead;
		info.msg.cargoShip.isEgressing = egressing;
		info.msg.cargoShip.harborIndex = harborIndex;
		if (info.forDisk)
		{
			info.msg.cargoShip.playerIds = Pool.Get<List<ulong>>();
			info.msg.cargoShip.playerIds.AddRange(boardedPlayerIds);
			info.msg.cargoShip.lifetime = lifetime;
			info.msg.cargoShip.lootRoundsPassed = lootRoundsPassed;
			info.msg.cargoShip.availableCrateSpawnIndices = Pool.Get<List<int>>();
			info.msg.cargoShip.availableCrateSpawnIndices.AddRange(availableCrateSpawnIndices);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		RefreshActiveLayout();
	}

	public void PlayHorn()
	{
		ClientRPC(RpcTarget.NetworkGroup("DoHornSound"));
		hornCount++;
		if (hornCount >= 3)
		{
			hornCount = 0;
			CancelInvoke(PlayHorn);
		}
	}

	public override void Spawn()
	{
		if (!Rust.Application.isLoadingSave)
		{
			layoutChoice = (uint)UnityEngine.Random.Range(0, layouts.Length);
			SendNetworkUpdate();
			RefreshActiveLayout();
		}
		base.Spawn();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		CalculateHarborApproachNodes();
		Invoke(FindInitialNode, 2f);
		InvokeRepeating(BuildingCheck, 1f, 5f);
		Invoke(DisableCollisionTest, 10f);
		float waterSurface = WaterLevel.GetWaterSurface(base.transform.position, waves: false, volumes: false);
		Vector3 vector = base.transform.InverseTransformPoint(waterLine.transform.position);
		base.transform.position = new Vector3(base.transform.position.x, waterSurface - vector.y, base.transform.position.z);
		bool flag = false;
		if (!flag)
		{
			SpawnSubEntities();
		}
		for (int i = 0; i < crateSpawns.Count; i++)
		{
			availableCrateSpawnIndices.Add(i);
		}
		if (!Rust.Application.isLoadingSave)
		{
			if (HasFinishedDocking)
			{
				Invoke(StartEgress, Mathf.Max(EventTimeRemaining, 120f));
			}
			InvokeRepeating(RespawnLoot, 10f, 60f * loot_round_spacing_minutes);
		}
		if (!flag)
		{
			CreateMapMarker();
		}
	}

	public void UpdateRadiation()
	{
		currentRadiation += 1f;
		TriggerRadiation[] componentsInChildren = radiation.GetComponentsInChildren<TriggerRadiation>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RadiationAmountOverride = currentRadiation;
		}
	}

	public void StartEgress()
	{
		if (isDoingHarborApproach || egressing)
		{
			return;
		}
		egressing = true;
		if (Interface.CallHook("OnCargoShipEgress", this) == null)
		{
			CancelInvoke(PlayHorn);
			radiation.SetActive(value: true);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: true);
			}
			InvokeRepeating(UpdateRadiation, 10f, 1f);
			Invoke(DelayedDestroy, 60f * egress_duration_minutes);
		}
	}

	public void DelayedDestroy()
	{
		Kill();
	}

	public void FindInitialNode()
	{
		targetNodeIndex = GetClosestNodeToUs();
	}

	private int GetHackableCrateCount()
	{
		int num = 0;
		foreach (BaseEntity child in children)
		{
			if (child is HackableLockedCrate)
			{
				num++;
			}
		}
		return num;
	}

	public void BuildingCheck()
	{
		List<BaseEntity> obj = Pool.Get<List<BaseEntity>>();
		Vis.Entities(WorldSpaceBounds(), obj, 2162689);
		foreach (BaseEntity item in obj)
		{
			if (!(item is JunkPileWater junkPileWater))
			{
				if (item is DecayEntity decayEntity && decayEntity.parentEntity.Get(serverside: true) != this && decayEntity.isServer && decayEntity.IsAlive() && !decayEntity.AllowOnCargoShip && !PlayerBoat.IsChildOfFinishedPlayerBoat(decayEntity))
				{
					decayEntity.Kill(DestroyMode.Gib);
				}
			}
			else
			{
				junkPileWater.SinkAndDestroy();
			}
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public void FixedUpdate()
	{
		if (!base.isClient)
		{
			UpdateMovement();
			lifetime += Time.fixedDeltaTime;
		}
	}

	public void UpdateMovement()
	{
		if (IsOceanPatrolPathAvailable() && IsValidTargetNode())
		{
			InitializeHarborApproach();
			Vector3 approachRotationNode = Vector3.zero;
			CalculateDesiredNodes(out var desiredMoveNode, out approachRotationNode);
			float num = 0f;
			num = CalculateDesiredThrottle(desiredMoveNode);
			UpdateShip(num, desiredMoveNode, approachRotationNode);
			UpdateCameraNetworkGroups();
			float num2 = (isDoingHarborApproach ? 8f : 80f);
			if (Vector3.Distance(base.transform.position, desiredMoveNode) < num2)
			{
				HandleNodeArrival(desiredMoveNode);
			}
			UpdateHarborApproachProgress();
		}
	}

	private void UpdateCameraNetworkGroups()
	{
		if (cameraEntities == null)
		{
			return;
		}
		foreach (CCTV_RC cameraEntity in cameraEntities)
		{
			if (cameraEntity != null && !cameraEntity.IsDestroyed)
			{
				cameraEntity.UpdateNetworkGroup();
			}
		}
	}

	[ContextMenu("Break")]
	public void Break()
	{
		CalculateDesiredNodes(out var desiredMoveNode, out var _);
		Vector3 normalized = (base.transform.position - desiredMoveNode).normalized;
		base.transform.forward = normalized;
		currentTurnSpeed = 0f;
	}

	private void UpdateShip(float desiredThrottle, Vector3 desiredWaypoint, Vector3 approachRotationNode)
	{
		Vector3 normalized = (desiredWaypoint - base.transform.position).normalized;
		normalized.y = 0f;
		float num = Vector3.Dot(base.transform.right, normalized);
		float num2 = (isDoingHarborApproach ? 6.5f : 2.5f);
		float num3 = Mathf.InverseLerp(0.05f, 0.5f, Mathf.Abs(num));
		if (num3 == 0f && Vector3.Dot(normalized, -base.transform.forward) >= 0.95f)
		{
			num3 = 1f;
		}
		turnScale = Mathf.Lerp(turnScale, num3, Time.deltaTime * 0.2f);
		float num4 = ((!(num < 0f)) ? 1 : (-1));
		currentTurnSpeed = num2 * turnScale * num4;
		if (!isDoingHarborApproach)
		{
			base.transform.Rotate(Vector3.up, Time.deltaTime * currentTurnSpeed, Space.World);
		}
		currentThrottle = Mathf.Lerp(currentThrottle, desiredThrottle, Time.deltaTime * 0.2f);
		currentVelocity = base.transform.forward * (8f * currentThrottle);
		if (isDoingHarborApproach)
		{
			currentVelocity = normalized * currentThrottle * 5f;
			Vector3 normalized2 = (approachRotationNode - base.transform.position).normalized;
			normalized2.y = 0f;
			if (normalized2 != Vector3.zero)
			{
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.LookRotation(normalized2), Time.deltaTime * 0.1f);
			}
		}
		if (HasFlag(Flags.Reserved1))
		{
			currentVelocity = Vector3.zero;
		}
		base.transform.position += currentVelocity * Time.deltaTime;
	}

	private void UpdateHarborApproachProgress()
	{
		if (isDoingHarborApproach && harborApproachPath != null && harborApproachPath.TryGetComponent<HarborProximityManager>(out var component))
		{
			float pathLength = harborApproachPath.GetPathLength();
			float pathProgress = harborApproachPath.GetPathProgress(base.transform.position);
			component.UpdateNormalisedState(Mathf.Clamp01(pathProgress / pathLength));
		}
	}

	private void InitializeHarborApproach(bool forceInit = false)
	{
		if (forceInit || harbors.Count > 0)
		{
			harborApproachPath = harbors[harborIndex].harborPath;
			proxManager = harborApproachPath.GetComponent<HarborProximityManager>();
		}
	}

	private float CalculateDesiredThrottle(Vector3 desiredMoveNode)
	{
		Vector3 normalized = (desiredMoveNode - base.transform.position).normalized;
		float value = Vector3.Dot(base.transform.forward, normalized);
		float num = Mathf.InverseLerp(0f, 1f, value);
		if (isDoingHarborApproach)
		{
			if (harborApproachPath.nodes[currentHarborApproachNode].maxVelocityOnApproach > 0f)
			{
				lastSpeed = harborApproachPath.nodes[currentHarborApproachNode].maxVelocityOnApproach;
			}
			num = Mathf.Clamp(num, 0.1f, lastSpeed);
		}
		return num;
	}

	private void CalculateDesiredNodes(out Vector3 desiredMoveNode, out Vector3 approachRotationNode)
	{
		if (isDoingHarborApproach)
		{
			int index = (shouldLookAhead ? Mathf.Min(currentHarborApproachNode + 1, harborApproachPath.nodes.Count - 1) : currentHarborApproachNode);
			approachRotationNode = harborApproachPath.nodes[index].Position;
			desiredMoveNode = harborApproachPath.nodes[currentHarborApproachNode].Position;
			return;
		}
		desiredMoveNode = TerrainMeta.Path.OceanPatrolFar[targetNodeIndex];
		if (egressing)
		{
			Vector3 normalized = base.transform.position.WithY(0f).normalized;
			Vector3 normalized2 = base.transform.forward.WithY(0f).normalized;
			Vector3 origin = base.transform.position + Vector3.up * 5f;
			Ray[] array = new Ray[2]
			{
				new Ray(origin, (Quaternion.Euler(0f, -7f, 0f) * normalized2).normalized),
				new Ray(origin, (Quaternion.Euler(0f, 7f, 0f) * normalized2).normalized)
			};
			int num = 0;
			List<RaycastHit> obj = Pool.Get<List<RaycastHit>>();
			for (int i = 0; i < array.Length; i++)
			{
				GamePhysics.TraceAll(array[i], 10f, obj, 600f, 262144, QueryTriggerInteraction.Collide);
				bool flag = false;
				foreach (RaycastHit item in obj)
				{
					BaseEntity entity = RaycastHitEx.GetEntity(item);
					if ((!(entity != null) || (!(entity == this) && !entity.EqualNetID(this))) && !(entity is CargoShip) && item.collider.isTrigger && item.collider.CompareTag("FerryAvoid"))
					{
						num = ((i != 0) ? 1 : (-1));
						flag = true;
						break;
					}
				}
				obj.Clear();
				if (flag)
				{
					break;
				}
			}
			if (num != 0)
			{
				Vector3 normalized3 = (Quaternion.Euler(0f, -45f, 0f) * normalized2).normalized;
				Vector3 normalized4 = (Quaternion.Euler(0f, 45f, 0f) * normalized2).normalized;
				Vector3 vector = ((num == -1) ? normalized4 : normalized3);
				desiredMoveNode = base.transform.position + vector * 10000f;
			}
			else
			{
				desiredMoveNode = base.transform.position + normalized * 10000f;
			}
			Pool.FreeUnmanaged(ref obj);
			if (base.transform.position.sqrMagnitude > 100000000f)
			{
				Debug.LogWarning("Immediately deleting cargo as it is a long way out of bounds");
				Kill();
			}
			if (DeepSeaManager.IsInsideDeepSea(base.transform.position))
			{
				Debug.LogWarning("Immediately deleting cargo as it reached the deep sea");
				Kill();
			}
		}
		approachRotationNode = Vector3.zero;
	}

	private void HandleNodeArrival(Vector3 waypointPosition)
	{
		if (isDoingHarborApproach)
		{
			if (currentHarborApproachNode == harborApproachPath.nodes.Count - 1)
			{
				EndHarborApproach();
			}
			else
			{
				AdvanceHarborApproach();
			}
			return;
		}
		targetNodeIndex = (targetNodeIndex - 1 + TerrainMeta.Path.OceanPatrolFar.Count) % TerrainMeta.Path.OceanPatrolFar.Count;
		if (HasFinishedDocking)
		{
			return;
		}
		for (int i = 0; i < harbors.Count; i++)
		{
			HarborInfo harborInfo = harbors[i];
			if (harborInfo.harborPath != null && harborInfo.approachNode == targetNodeIndex)
			{
				CargoNotifier component = harborInfo.harborPath.GetComponent<CargoNotifier>();
				harborApproachPath = harborInfo.harborPath;
				harborIndex = i;
				if (component != null)
				{
					StartHarborApproach(component);
					break;
				}
			}
		}
	}

	public void StartHarborApproach(CargoNotifier cn)
	{
		if (Interface.CallHook("OnCargoShipHarborApproach", this, cn) != null)
		{
			return;
		}
		PlayHorn();
		isDoingHarborApproach = true;
		dockCount++;
		shouldLookAhead = false;
		if (proxManager != null)
		{
			proxManager.StartMovement();
		}
		ClearAllHarborEntitiesOnShip();
		foreach (HarborCraneContainerPickup allCrane in HarborCraneContainerPickup.AllCranes)
		{
			if (!(allCrane == null) && !allCrane.isClient && !(allCrane.Distance2D(harborApproachPath.nodes[harborApproachPath.nodes.Count / 2].Position) > 150f))
			{
				allCrane.ReplenishContainers();
			}
		}
	}

	private float GetTimeRemainingFromCrates()
	{
		float requiredHackSeconds = HackableLockedCrate.requiredHackSeconds;
		if (GetHackableCrateCount() != 0)
		{
			return requiredHackSeconds + requiredHackSeconds * 0.3f;
		}
		return 120f;
	}

	private void EndHarborApproach()
	{
		PlayHorn();
		isDoingHarborApproach = false;
		currentHarborApproachNode = 0;
		FindInitialNode();
		if (proxManager != null)
		{
			proxManager.EndMovement();
		}
		if (HasFinishedDocking)
		{
			if (docking_debug)
			{
				Debug.Log($"Finished all docking: {EventTimeRemaining}s left in event");
			}
			Invoke(StartEgress, Mathf.Max(EventTimeRemaining, GetTimeRemainingFromCrates()));
		}
	}

	private void AdvanceHarborApproach()
	{
		if (currentHarborApproachNode + 1 < harborApproachPath.nodes.Count)
		{
			currentHarborApproachNode++;
		}
		if (!shouldLookAhead)
		{
			shouldLookAhead = true;
		}
		if (harborApproachPath.nodes[currentHarborApproachNode].maxVelocityOnApproach == 0f)
		{
			OnArrivedAtHarbor();
		}
	}

	private bool IsOceanPatrolPathAvailable()
	{
		if (TerrainMeta.Path.OceanPatrolFar != null)
		{
			return TerrainMeta.Path.OceanPatrolFar.Count > 0;
		}
		return false;
	}

	private bool IsValidTargetNode()
	{
		return targetNodeIndex != -1;
	}

	private void PreHarborLeaveHorn()
	{
		PlayHorn();
	}

	private void LeaveHarbor()
	{
		if (docking_debug)
		{
			Debug.Log("Cargo is leaving harbor.");
		}
		PlayHorn();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
		}
		currentHarborApproachNode++;
		Interface.CallHook("OnCargoShipHarborLeave", this);
	}

	public int GetClosestNodeToUs()
	{
		int num = -1;
		float num2 = float.PositiveInfinity;
		int num3 = -1;
		float num4 = float.PositiveInfinity;
		Vector3 position = base.transform.position;
		for (int i = 0; i < TerrainMeta.Path.OceanPatrolFar.Count; i++)
		{
			Vector3 vector = TerrainMeta.Path.OceanPatrolFar[i];
			float sqrMagnitude = (vector - position).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				num2 = sqrMagnitude;
				num = i;
			}
			if (!SegmentIntersectsOilRig(position, vector) && sqrMagnitude < num4)
			{
				num4 = sqrMagnitude;
				num3 = i;
			}
		}
		if (num3 == -1)
		{
			if (num == -1)
			{
				return 0;
			}
			return num;
		}
		return num3;
	}

	private bool SegmentIntersectsOilRig(Vector3 from, Vector3 to)
	{
		float num = Vector3.Distance(from, to);
		if (num <= 0.1f)
		{
			return false;
		}
		Vector3 normalized = (to - from).normalized;
		Vector3 origin = from + Vector3.up * 5f;
		List<RaycastHit> obj = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(origin, normalized), 10f, obj, num + 5f, 262144, QueryTriggerInteraction.Collide);
		bool result = false;
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!(entity != null) || (!(entity == this) && !entity.EqualNetID(this))) && item.collider.isTrigger && item.collider.CompareTag("FerryAvoid"))
			{
				result = true;
				break;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		return currentVelocity;
	}

	public override Quaternion GetAngularVelocityServer()
	{
		return Quaternion.Euler(0f, currentTurnSpeed, 0f);
	}

	public override float InheritedVelocityScale()
	{
		return 1f;
	}

	public override bool BlocksWaterFor(BasePlayer player)
	{
		return true;
	}

	public override float AntiHackVelocity()
	{
		return 8f;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool ForceDeployableSetParent()
	{
		return true;
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CargoShip.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
