#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ChickenCoop : StorageContainer
{
	public struct AnimalStatus
	{
		public EntityRef<FarmableAnimal> SpawnedAnimal;

		public TimeUntil TimeUntilHatch;

		public void CopyTo(ChickenStatus status, ThreadSafeTime time)
		{
			status.spawnedChicken = SpawnedAnimal.uid;
			status.timeUntilHatch = TimeUntilHatch.LeftFrom(time.Time);
		}

		public void CopyFrom(ChickenStatus status)
		{
			SpawnedAnimal.uid = status.spawnedChicken;
			TimeUntilHatch = status.timeUntilHatch;
		}
	}

	public class ChickenCoopWorkQueue : ObjectWorkQueue<ChickenCoop>
	{
		protected override void RunJob(ChickenCoop entity)
		{
			entity.QueuedWorkJob();
		}
	}

	public Transform[] SpawnPoints;

	public GameObjectRef ChickenPrefab;

	public int MaxChickens = 4;

	public float ChickenHatchTimeMinutes = 30f;

	public float SunCheckRate = 10f;

	public Transform SunSampler;

	public List<AnimalStatus> Animals = new List<AnimalStatus>();

	public const Flags Hatching = Flags.Reserved1;

	public const Flags Full = Flags.Reserved3;

	public const int EggInsertSlot = 0;

	public const int FoodSlot = 1;

	public const int WaterSlot = 2;

	public const int FoodProductionSlot = 3;

	public GameObjectRef hatchEffect;

	private static ItemDefinition _eggDef;

	private float currentSunValue;

	public Plane MovementPlane;

	private Func<Item, int, bool> reservedSlotCallback;

	public static ChickenCoopWorkQueue CoopWorkQueue = new ChickenCoopWorkQueue();

	private static ItemDefinition EggDef
	{
		get
		{
			if (_eggDef == null)
			{
				_eggDef = ItemManager.FindItemDefinition("egg");
			}
			return _eggDef;
		}
	}

	public bool IsInSun => currentSunValue > 0f;

	public bool IsOnTerrain { get; private set; }

	public Item CurrentFoodItem => base.inventory?.GetSlot(1);

	public Item CurrentWaterItem => base.inventory?.GetSlot(2);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ChickenCoop.OnRpcMessage"))
		{
			if (rpc == 3418655327u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestAnimalStats");
				}
				using (TimeWarning.New("RequestAnimalStats"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3418655327u, "RequestAnimalStats", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3418655327u, "RequestAnimalStats", this, player, 3f))
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
							RequestAnimalStats(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RequestAnimalStats");
					}
				}
				return true;
			}
			if (rpc == 1409078750 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SubmitEggForHatching");
				}
				using (TimeWarning.New("SubmitEggForHatching"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1409078750u, "SubmitEggForHatching", this, player, 3f))
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
							RPCMessage msg3 = rPCMessage;
							SubmitEggForHatching(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SubmitEggForHatching");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(ScheduleWorkQueue, UnityEngine.Random.Range(0f, SunCheckRate), SunCheckRate);
		if (GamePhysics.Trace(new Ray(base.transform.position + base.transform.up, -base.transform.up), 0f, out var hitInfo, 1.1f, 10485760))
		{
			IsOnTerrain = ColliderEx.IsOnLayer(hitInfo.collider, Rust.Layer.Terrain);
			UpdateMovementPlane();
		}
		if (reservedSlotCallback == null)
		{
			reservedSlotCallback = SlotIsReserved;
		}
		base.inventory.slotIsReserved = reservedSlotCallback;
	}

	public void UpdateMovementPlane()
	{
		if (!IsOnTerrain)
		{
			MovementPlane = new Plane(base.transform.up, base.transform.position);
		}
	}

	public void SpawnFoodProduction(ItemDefinition def, int count)
	{
		try
		{
			base.inventory.slotIsReserved = null;
			Item item = ItemManager.Create(def, 1, 0uL, isServerSide: true, 0uL);
			if (!item.MoveToContainer(base.inventory, 3))
			{
				item.Remove();
			}
		}
		finally
		{
			base.inventory.slotIsReserved = reservedSlotCallback;
		}
	}

	private bool SlotIsReserved(Item item, int slot)
	{
		if (slot == 3)
		{
			return true;
		}
		return false;
	}

	private void ScheduleWorkQueue()
	{
		CoopWorkQueue.Add(this);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SubmitEggForHatching(RPCMessage msg)
	{
		if (HasFlag(Flags.Reserved3) || HasFlag(Flags.Reserved1))
		{
			return;
		}
		Item slot = base.inventory.GetSlot(0);
		if (slot != null && !(slot.info != EggDef) && !(msg.player.inventory.loot.entitySource != this))
		{
			slot.UseItem();
			Animals.Add(new AnimalStatus
			{
				TimeUntilHatch = ChickenHatchTimeMinutes * 60f
			});
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
				flagsUpdateScope.Set(Flags.Reserved3, Animals.Count >= MaxChickens);
			}
			if (!IsInvoking(CheckEggHatchState))
			{
				InvokeRepeating(CheckEggHatchState, 10f, 10f);
			}
			SendNetworkUpdate();
		}
	}

	private void CheckEggHatchState()
	{
		bool flag = false;
		for (int i = 0; i < Animals.Count; i++)
		{
			AnimalStatus value = Animals[i];
			if (!value.SpawnedAnimal.IsSet && (float)value.TimeUntilHatch <= 0f)
			{
				FarmableAnimal farmableAnimal = SpawnChicken(i);
				value.SpawnedAnimal.Set(farmableAnimal);
				flag = true;
				Animals[i] = value;
				if (hatchEffect != null && hatchEffect.isValid)
				{
					Effect.server.Run(hatchEffect.resourcePath, farmableAnimal.transform.position);
				}
			}
		}
		if (flag)
		{
			SetFlagLocal(Flags.Reserved1, b: false);
			CancelInvoke(CheckEggHatchState);
			SendNetworkUpdate();
		}
	}

	private FarmableAnimal SpawnChicken(int index)
	{
		FarmableAnimal obj = base.gameManager.CreateEntity(ChickenPrefab.resourcePath, base.transform.TransformPoint(GetRandomMovePoint()), Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f)) as FarmableAnimal;
		obj.SetParent(this, worldPositionStays: true);
		string text = RandomUsernames.Get(UnityEngine.Random.Range(0, 1000));
		text = text[0].ToString().ToUpper() + text.Substring(1);
		obj.ApplyStartingStats(text);
		obj.Spawn();
		return obj;
	}

	public Vector3 GetRandomMovePoint()
	{
		if (ConVar.Server.farmChickenLocalAvoidance)
		{
			int num = 10;
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = base.transform.InverseTransformPoint(SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)].position);
				if (IsLocationClear(vector, 0.25f))
				{
					return vector;
				}
			}
		}
		return base.transform.InverseTransformPoint(SpawnPoints[UnityEngine.Random.Range(0, SpawnPoints.Length)].position);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.chickenCoop = Facepunch.Pool.Get<ProtoBuf.ChickenCoop>();
		info.msg.chickenCoop.chickens = Facepunch.Pool.Get<List<ChickenStatus>>();
		foreach (AnimalStatus animal in Animals)
		{
			ChickenStatus chickenStatus = Facepunch.Pool.Get<ChickenStatus>();
			animal.CopyTo(chickenStatus, info.cachedTime);
			info.msg.chickenCoop.chickens.Add(chickenStatus);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasFlag(Flags.Reserved1) && !IsInvoking(CheckEggHatchState))
		{
			InvokeRepeating(CheckEggHatchState, 10f, 10f);
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (targetSlot == 0 && item.info != EggDef)
		{
			return false;
		}
		if (targetSlot == 1 && !IsValidFoodItem(item))
		{
			return false;
		}
		switch (targetSlot)
		{
		case 2:
			return item.info.shortname == "water";
		case 3:
			if (item.info != EggDef)
			{
				return false;
			}
			break;
		}
		return base.ItemFilter(player, item, targetSlot);
	}

	private void QueuedWorkJob()
	{
		if (Animals.Count != 0)
		{
			UpdateSunValue();
		}
	}

	private void UpdateSunValue()
	{
		if (TOD_Sky.Instance.IsNight)
		{
			currentSunValue = 0f;
			return;
		}
		Vector3 sunDirection = TOD_Sky.Instance.SunDirection;
		float value = Vector3.Dot(SunSampler.forward, sunDirection);
		currentSunValue = Mathf.InverseLerp(0.1f, 0.6f, value);
		if (currentSunValue > 0f && !CanSee(SunSampler.position, SunSampler.position + sunDirection * 100f))
		{
			currentSunValue = 0f;
		}
	}

	public void DebugFillCoop()
	{
		while (Animals.Count < MaxChickens)
		{
			AnimalStatus item = default(AnimalStatus);
			FarmableAnimal entity = SpawnChicken(Animals.Count);
			item.SpawnedAnimal.Set(entity);
			Animals.Add(item);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: true);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void RequestAnimalStats(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		using ChickenCoopStatusUpdate chickenCoopStatusUpdate = Facepunch.Pool.Get<ChickenCoopStatusUpdate>();
		chickenCoopStatusUpdate.animals = Facepunch.Pool.Get<List<FarmableAnimalStatus>>();
		foreach (AnimalStatus animal in Animals)
		{
			FarmableAnimalStatus farmableAnimalStatus = Facepunch.Pool.Get<FarmableAnimalStatus>();
			farmableAnimalStatus.data = Facepunch.Pool.Get<ProtoBuf.FarmableAnimal>();
			EntityRef<FarmableAnimal> spawnedAnimal = animal.SpawnedAnimal;
			if (spawnedAnimal.Get(serverside: true) != null)
			{
				spawnedAnimal = animal.SpawnedAnimal;
				spawnedAnimal.Get(serverside: true).SaveToData(farmableAnimalStatus.data);
				spawnedAnimal = animal.SpawnedAnimal;
				farmableAnimalStatus.animal = spawnedAnimal.uid;
				chickenCoopStatusUpdate.animals.Add(farmableAnimalStatus);
			}
		}
		ClientRPC(RpcTarget.Player("OnReceivedChickenStats", player), chickenCoopStatusUpdate);
	}

	public void OnAnimalDied(FarmableAnimal animal)
	{
		for (int i = 0; i < Animals.Count; i++)
		{
			if (Animals[i].SpawnedAnimal.uid == animal.net.ID)
			{
				Animals.RemoveAt(i);
				break;
			}
		}
		SetFlagLocal(Flags.Reserved3, Animals.Count >= MaxChickens);
		SendNetworkUpdate();
	}

	public override void DropItems(BaseEntity initiator = null)
	{
		base.inventory.GetSlot(2)?.Remove();
		base.DropItems(initiator);
	}

	public bool IsLocationClear(Vector3 pos, float radius, FarmableAnimal ignoreAnimal = null)
	{
		foreach (AnimalStatus animal in Animals)
		{
			FarmableAnimal farmableAnimal = animal.SpawnedAnimal.Get(serverside: true);
			if (farmableAnimal != null && farmableAnimal != ignoreAnimal && (farmableAnimal.transform.localPosition - pos).sqrMagnitude < radius * radius)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsValidFoodItem(Item item)
	{
		if (item != null && item.info.TryGetComponent<ItemModConsumable>(out var component))
		{
			return component.chickenCoopFood;
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		Animals.Clear();
		if (info.msg.chickenCoop == null || info.msg.chickenCoop.chickens == null)
		{
			return;
		}
		foreach (ChickenStatus chicken in info.msg.chickenCoop.chickens)
		{
			AnimalStatus item = default(AnimalStatus);
			item.CopyFrom(chicken);
			Animals.Add(item);
		}
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (Animals.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}
}
