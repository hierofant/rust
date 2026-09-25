#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class FarmableAnimal : BaseCombatEntity
{
	public class FarmableAnimalNeedsWorkQueue : ObjectWorkQueue<FarmableAnimal>
	{
		protected override void RunJob(FarmableAnimal entity)
		{
			entity.TickHappiness();
		}
	}

	[Header("Client")]
	public Animator AnimalAnimator;

	public Renderer[] AllRenderers;

	public Gradient RendererColourGradient;

	public GameObjectRef CorpsePrefab;

	public GestureConfig PettingGesture;

	[Header("Movements")]
	public float RepositionMinTime = 10f;

	public float RepositionMaxTime = 30f;

	public float AnimalMoveSpeed = 0.75f;

	[Header("Needs")]
	public float HungerDecayRate = 0.5f;

	public float ThirstDecayRate = 0.1f;

	public float LoveDecayRate = 0.1f;

	public float SunlightDecayRate = 0.05f;

	public float SunlightIncreaseRate = 0.05f;

	[Header("Eating")]
	public float MinimumTimeBetweenEating = 30f;

	[Range(0f, 1f)]
	public float AmountPerFood = 0.4f;

	[Header("Drinking")]
	public float MinimumTimeBetweenDrinking = 10f;

	[Range(0f, 1f)]
	public float AmountPerDrink = 0.1f;

	[Header("Health")]
	public float HealthChangeRate = 10f;

	public float HealthChangeScale = 1f;

	[Header("Egg Production")]
	public float MinimumMinutesBetweenProduction = 10f;

	public float MaximumMinutesBetweenProduction = 50f;

	public ItemDefinition ItemToCreate;

	public const float StatCount = 4f;

	public const Flags RecentlyPetted = Flags.Reserved1;

	public const Flags Moving = Flags.Reserved2;

	public const Flags Sleeping = Flags.Reserved3;

	public const float MaxHappinessStat = 100f;

	public const int MaxNameLength = 12;

	private Vector3 currentMoveTarget;

	private TimeSince lastHappinessTick;

	private Action moveChickenAction;

	private Action moveToNewLocationAction;

	private TimeSince lastEat;

	private TimeSince lastDrink;

	private TimeUntil nextEggProduction;

	private TimeUntil nextHealthCheck;

	private TimeSince wokenUp;

	public static FarmableAnimalNeedsWorkQueue NeedsWorkQueue = new FarmableAnimalNeedsWorkQueue();

	public float AnimalHunger { get; private set; } = 100f;


	public float AnimalThirst { get; private set; } = 100f;


	public float AnimalLove { get; private set; } = 100f;


	public float AnimalSunlight { get; private set; } = 100f;


	public string AnimalName { get; private set; } = string.Empty;


	public float HappinessNormalised => (AnimalHunger + AnimalThirst + AnimalLove + AnimalSunlight) / 4f / 100f;

	private ChickenCoop ParentCoop => parentEntity.Get(base.isServer) as ChickenCoop;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FarmableAnimal.OnRpcMessage"))
		{
			if (rpc == 3115049114u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestNameChange");
				}
				using (TimeWarning.New("RequestNameChange"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3115049114u, "RequestNameChange", this, player, 3f, checkParent: true))
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
							RequestNameChange(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RequestNameChange");
					}
				}
				return true;
			}
			if (rpc == 2457655601u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerPetChicken");
				}
				using (TimeWarning.New("ServerPetChicken"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2457655601u, "ServerPetChicken", this, player, 3f))
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
							ServerPetChicken(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ServerPetChicken");
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
		moveToNewLocationAction = MoveToNewLocation;
		Invoke(moveToNewLocationAction, UnityEngine.Random.Range(RepositionMinTime, RepositionMaxTime));
		base.transform.localPosition = SnapMovementPos(base.transform.localPosition);
		lastHappinessTick = 0f;
		nextEggProduction = UnityEngine.Random.Range(MinimumMinutesBetweenProduction, MaximumMinutesBetweenProduction) * 60f;
		InvokeRepeating(QueueHappinessTick, 10f, 1f);
	}

	private void QueueHappinessTick()
	{
		NeedsWorkQueue.Add(this);
	}

	private float ConvertDecayRateToSecondMultiplier(float rate)
	{
		return rate / 60f / 60f;
	}

	private void TickHappiness()
	{
		float num = lastHappinessTick;
		lastHappinessTick = 0f;
		AnimalHunger = Mathf.Clamp(AnimalHunger - num * ConvertDecayRateToSecondMultiplier(HungerDecayRate), 0f, 100f);
		AnimalThirst = Mathf.Clamp(AnimalThirst - num * ConvertDecayRateToSecondMultiplier(ThirstDecayRate), 0f, 100f);
		AnimalLove = Mathf.Clamp(AnimalLove - num * ConvertDecayRateToSecondMultiplier(LoveDecayRate), 0f, 100f);
		if (ParentCoop != null && ParentCoop.IsInSun)
		{
			AnimalSunlight = Mathf.Clamp(AnimalSunlight + num * ConvertDecayRateToSecondMultiplier(SunlightIncreaseRate), 0f, 100f);
		}
		else
		{
			AnimalSunlight = Mathf.Clamp(AnimalSunlight - num * ConvertDecayRateToSecondMultiplier(SunlightDecayRate), 0f, 100f);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (TOD_Sky.Instance.IsNight && !HasFlag(Flags.Reserved2))
			{
				flagsUpdateScope.Set(Flags.Reserved3, b: true);
			}
			else if (!TOD_Sky.Instance.IsNight)
			{
				if (HasFlag(Flags.Reserved3))
				{
					wokenUp = 0f;
				}
				flagsUpdateScope.Set(Flags.Reserved3, b: false);
			}
		}
		if ((float)lastEat > MinimumTimeBetweenEating && AnimalHunger / 100f <= 1f - AmountPerFood * 0.2f)
		{
			lastEat = 0f;
			if (ParentCoop != null)
			{
				Item currentFoodItem = ParentCoop.CurrentFoodItem;
				if (currentFoodItem != null)
				{
					currentFoodItem.UseItem();
					AnimalHunger = Mathf.Clamp(AnimalHunger + AmountPerFood * 100f, 0f, 100f);
				}
			}
		}
		if ((float)lastDrink > MinimumTimeBetweenDrinking && AnimalThirst / 100f <= 1f - AmountPerDrink * 0.5f)
		{
			lastDrink = 0f;
			if (ParentCoop != null)
			{
				Item currentWaterItem = ParentCoop.CurrentWaterItem;
				if (currentWaterItem != null)
				{
					currentWaterItem.UseItem();
					AnimalThirst = Mathf.Clamp(AnimalThirst + AmountPerDrink * 100f, 0f, 100f);
				}
			}
		}
		if ((float)nextEggProduction < 0f && ItemToCreate != null)
		{
			nextEggProduction = UnityEngine.Random.Range(MinimumMinutesBetweenProduction, MaximumMinutesBetweenProduction) * 60f;
			if (base.healthFraction > 0.75f)
			{
				float num2 = Mathx.RemapValClamped(HappinessNormalised, 0.75f, 1f, 0f, 1f);
				if (UnityEngine.Random.Range(0f, 1f) < num2 && ParentCoop != null)
				{
					ParentCoop.SpawnFoodProduction(ItemToCreate, 1);
				}
			}
		}
		if ((float)nextHealthCheck <= 0f)
		{
			nextHealthCheck = HealthChangeRate;
			if (HappinessNormalised < 0.5f)
			{
				float num3 = 1f - Mathx.RemapValClamped(HappinessNormalised, 0f, 0.5f, 0f, 1f);
				float amount = HealthChangeScale * num3;
				Hurt(amount);
			}
			else
			{
				float num4 = Mathx.RemapValClamped(HappinessNormalised, 0.5f, 1f, 0f, 1f);
				float amount2 = HealthChangeScale * num4;
				Heal(amount2);
			}
		}
	}

	private void MoveToNewLocation()
	{
		if (HasFlag(Flags.Reserved3) || (float)wokenUp <= 2f)
		{
			Invoke(moveToNewLocationAction, UnityEngine.Random.Range(RepositionMinTime, RepositionMaxTime));
			return;
		}
		ChickenCoop parentCoop = ParentCoop;
		if (parentCoop != null)
		{
			Vector3 randomMovePoint = parentCoop.GetRandomMovePoint();
			currentMoveTarget = randomMovePoint;
			if (moveChickenAction == null)
			{
				moveChickenAction = MoveChicken;
			}
			InvokeRepeating(moveChickenAction, 0f, 0f);
		}
	}

	private void MoveChicken()
	{
		Vector3 localPosition = base.transform.localPosition;
		Quaternion localRotation = base.transform.localRotation;
		localPosition = SnapMovementPos(Vector3.MoveTowards(localPosition, currentMoveTarget, AnimalMoveSpeed * UnityEngine.Time.deltaTime));
		if (ParentCoop != null && !ParentCoop.IsLocationClear(localPosition, 0.25f, this))
		{
			StopMoving();
			return;
		}
		if (Vector3.Distance(currentMoveTarget, localPosition) > 0.3f)
		{
			localRotation = Quaternion.LookRotation((currentMoveTarget.WithY(localPosition.y) - localPosition).normalized);
		}
		base.transform.SetLocalPositionAndRotation(localPosition, localRotation);
		if (Vector3.Distance(localPosition, currentMoveTarget.WithY(localPosition.y)) < 0.1f)
		{
			StopMoving();
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved2, b: true);
	}

	private void StopMoving()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
		}
		CancelInvoke(moveChickenAction);
		CancelInvoke(moveToNewLocationAction);
		Invoke(moveToNewLocationAction, UnityEngine.Random.Range(RepositionMinTime, RepositionMaxTime));
	}

	private Vector3 SnapMovementPos(Vector3 desiredPos)
	{
		if (ParentCoop == null)
		{
			return desiredPos;
		}
		desiredPos = ParentCoop.transform.TransformPoint(desiredPos);
		if (ParentCoop != null)
		{
			if (ParentCoop.IsOnTerrain)
			{
				desiredPos = desiredPos.WithY(TerrainMeta.HeightMap.GetHeight(desiredPos));
			}
			else
			{
				ParentCoop.UpdateMovementPlane();
				desiredPos.y = ParentCoop.MovementPlane.ClosestPointOnPlane(desiredPos).y;
			}
		}
		return ParentCoop.transform.InverseTransformPoint(desiredPos);
	}

	public void ApplyStartingStats(string defaultName)
	{
		AnimalName = defaultName;
		AnimalHunger = UnityEngine.Random.Range(20f, 40f);
		AnimalThirst = UnityEngine.Random.Range(20f, 40f);
		AnimalLove = UnityEngine.Random.Range(20f, 40f);
		AnimalSunlight = UnityEngine.Random.Range(20f, 40f);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void ServerPetChicken(RPCMessage msg)
	{
		if (CanPetChicken(msg.player))
		{
			AnimalLove = Mathf.Clamp(AnimalLove + 15f, 0f, 100f);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ClearPettedFlag, 5f);
			StopMoving();
			ClientRPC(RpcTarget.NetworkGroup("OnPetted"));
			base.transform.rotation = Quaternion.LookRotation((msg.player.transform.position.WithY(base.transform.position.y) - base.transform.position).normalized, base.transform.up);
			if (PettingGesture != null && msg.player != null)
			{
				msg.player.Server_StartGesture(PettingGesture, BasePlayer.GestureStartSource.ServerAction, bypassOwnershipCheck: true);
			}
		}
	}

	private void ClearPettedFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.farmableAnimal = Facepunch.Pool.Get<ProtoBuf.FarmableAnimal>();
		SaveToData(info.msg.farmableAnimal);
	}

	public void SaveToData(ProtoBuf.FarmableAnimal data)
	{
		data.hunger = AnimalHunger;
		data.thirst = AnimalThirst;
		data.love = AnimalLove;
		data.sunlight = AnimalSunlight;
		data.animalName = AnimalName;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f, CheckParent = true)]
	private void RequestNameChange(RPCMessage msg)
	{
		if (msg.player.CanBuild(cached: true))
		{
			string text = msg.read.String(12);
			if (!string.IsNullOrWhiteSpace(text))
			{
				AnimalName = text;
				SendNetworkUpdate();
			}
		}
	}

	public override void OnDied(HitInfo info)
	{
		if (ParentCoop != null)
		{
			ParentCoop.OnAnimalDied(this);
		}
		BaseCorpse baseCorpse = DropCorpse(CorpsePrefab.resourcePath);
		if ((bool)baseCorpse)
		{
			baseCorpse.Spawn();
			baseCorpse.TakeChildren(this);
		}
		base.OnDied(info);
	}

	public override void AdminKill()
	{
		if (ParentCoop != null)
		{
			ParentCoop.OnAnimalDied(this);
		}
		base.AdminKill();
	}

	[ServerVar(Help = "Simulates the provided number of hours on all farm animals within 10m")]
	public static void SimHours(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		int @int = arg.GetInt(0);
		using PooledList<FarmableAnimal> pooledList = Facepunch.Pool.Get<PooledList<FarmableAnimal>>();
		Vis.Entities(basePlayer.transform.position, 10f, pooledList, 2048);
		foreach (FarmableAnimal item in pooledList)
		{
			if (item.isServer)
			{
				item.lastHappinessTick = (float)item.lastHappinessTick + (float)@int * 60f * 60f;
				item.TickHappiness();
				item.SendNetworkUpdate();
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.farmableAnimal != null)
		{
			LoadFromData(info.msg.farmableAnimal);
		}
		if (base.isServer)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
		}
	}

	public void LoadFromData(ProtoBuf.FarmableAnimal data)
	{
		AnimalHunger = data.hunger;
		AnimalThirst = data.thirst;
		AnimalLove = data.love;
		AnimalSunlight = data.sunlight;
		AnimalName = data.animalName;
	}

	private bool CanPetChicken(BasePlayer bp)
	{
		if (!HasFlag(Flags.Reserved1) && !HasFlag(Flags.Reserved3) && bp != null && !bp.isMounted && !bp.modelState.blocking)
		{
			return !bp.InGesture;
		}
		return false;
	}

	public ChickenCoop.AnimalStatus GetStatus()
	{
		if (ParentCoop != null)
		{
			foreach (ChickenCoop.AnimalStatus animal in ParentCoop.Animals)
			{
				EntityRef<FarmableAnimal> spawnedAnimal = animal.SpawnedAnimal;
				if (spawnedAnimal.uid == net.ID)
				{
					return animal;
				}
			}
		}
		return default(ChickenCoop.AnimalStatus);
	}
}
