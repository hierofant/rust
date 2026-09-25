using Facepunch;
using ProtoBuf;
using UnityEngine;

public class Beehive : StorageContainer, IHeatSourceListener, ISplashable
{
	[Header("Beehive Settings")]
	public ItemDefinition HoneyCombDefinition;

	public ItemDefinition BeeNucleusDefinition;

	public float growthRate = 0.05f;

	public float beeStingTime = 2f;

	[Header("References")]
	public TriggerHurtEx hurtTrigger;

	public GameObjectRef masterSwarm;

	public const Flags HasNucleus = Flags.Reserved12;

	public const Flags HasBees = Flags.Reserved13;

	public const Flags AngryBees = Flags.Reserved14;

	[ServerVar(Help = "How long before a Beehive will update")]
	public static float updateHiveInterval = 120f;

	[ServerVar(Help = "How long before the Beehive will perform temperature and inside checks")]
	public static float updateHiveStatsInterval = 120f;

	[ServerVar(Help = "How much the Nucleus's XP should be increased per honeycomb generated")]
	public static int xpIncreasePerHoneycomb = 2;

	private static Vector3[] outsideLookupDirs = new Vector3[5]
	{
		new Vector3(0f, 1f, 0f).normalized,
		new Vector3(1f, 0f, 0f).normalized,
		new Vector3(0f, 0f, 1f).normalized,
		new Vector3(-1f, 0f, 0f).normalized,
		new Vector3(0f, 0f, -1f).normalized
	};

	private bool hasNucleus;

	private float createNewCombAccumulator;

	private float honeyCombProductionMultiplier = 2f;

	private TimeSince timeSinceAngryBees;

	private TimeCachedValue<float> temperatureExposure;

	private TimeCachedValue<float> humidityExposure;

	private TimeCachedValue<bool> outsideCheck;

	private float serverHumidity;

	private float serverTemperature;

	private bool serverOutside;

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (HasFlag(Flags.Reserved13) || HasFlag(Flags.Reserved12))
		{
			pickupErrorToFormat = (format: PickupErrors.ItemMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public bool IsOutsideAccurate()
	{
		return SocketMod_Inside.IsOutside(base.transform.position + Vector3.up * 0.2f, Quaternion.identity, outsideLookupDirs);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk && info.msg.beehive != null)
		{
			createNewCombAccumulator = info.msg.beehive.currentProgress;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.beehive = Pool.Get<ProtoBuf.Beehive>();
		info.msg.beehive.currentProgress = createNewCombAccumulator;
		if (!info.forDisk)
		{
			info.msg.beehive.temperature = serverTemperature;
			info.msg.beehive.inside = serverOutside;
			info.msg.beehive.humidity = serverHumidity;
		}
	}

	public override void OnItemRemovedFromStack(Item item, int amount)
	{
		base.OnItemRemovedFromStack(item, amount);
		OnItemAddedOrRemoved(item, added: false);
	}

	public override void OnItemAddedToStack(Item item, int amount)
	{
		base.OnItemAddedToStack(item, amount);
		OnItemAddedOrRemoved(item, added: true);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		CheckNucleus();
		Flags flags = base.flags;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			float num = base.inventory.GetAmount(HoneyCombDefinition.itemid);
			flagsUpdateScope.Set(Flags.Reserved13, num > 0f && hasNucleus);
			if (!added && item.info == HoneyCombDefinition)
			{
				BasePlayer basePlayer = BasePlayer.FindByID(base.LastLootedBy);
				if (basePlayer != null && basePlayer.IsAlive() && !basePlayer.IsNpc && basePlayer.isServer)
				{
					timeSinceAngryBees = 0f;
					flagsUpdateScope.Set(Flags.Reserved14, b: true);
				}
			}
		}
		if (base.inventory.IsFull(checkForPartialStacks: true))
		{
			StopHive();
		}
		else if (flags != base.flags)
		{
			SendNetworkUpdate();
		}
	}

	private void OnPhysicsNeighbourChanged()
	{
		using (TimeWarning.New("Beehive.OnPhysicsNeighbourChanged"))
		{
			CalculateQualifiers(force: true);
			SendNetworkUpdate();
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (!base.isClient)
		{
			CheckNucleus();
			Sprinkler.SplashableGrid.RegisterEntity(this);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		CalculateQualifiers(force: true);
		CheckNucleus();
		InvokeRepeating(HiveUpdateTick, 0f, 1f);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public void OnHeatSourceChanged()
	{
		CalculateQualifiers(force: true);
		SendNetworkUpdate();
	}

	private void HiveUpdateTick()
	{
		if ((float)timeSinceAngryBees > beeStingTime)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved14, b: false);
			}
		}
	}

	private void GenerateHoneyComb()
	{
		float num = base.inventory.GetAmount(HoneyCombDefinition.itemid);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved13, num > 0f && hasNucleus);
		}
		createNewCombAccumulator += growthRate * honeyCombProductionMultiplier;
		if (!(createNewCombAccumulator >= 1f))
		{
			return;
		}
		createNewCombAccumulator = 0f;
		if (hasNucleus)
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				int dataInt = slot.instanceData.dataInt;
				if (NucleusGrading.XpToGrade(dataInt) != NucleusGrading.NucleusGrade.Grade1)
				{
					dataInt += xpIncreasePerHoneycomb;
					SetNucleusData(slot, dataInt);
				}
			}
		}
		Item item = ItemManager.Create(HoneyCombDefinition, 1, 0uL, isServerSide: true, 0uL);
		if (!item.MoveToContainer(base.inventory))
		{
			StopHive();
			item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (targetSlot == 0)
		{
			return item.info.shortname.Equals(allowedItem.shortname);
		}
		if (targetSlot > 0)
		{
			return item.info.shortname.Equals(allowedItem2.shortname);
		}
		return base.ItemFilter(player, item, targetSlot);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		return splashType == WaterTypes.RadioactiveWaterItemDef;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		if (splashType == WaterTypes.RadioactiveWaterItemDef)
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				hasNucleus = slot.info.GetComponent<ItemModBeehiveNucleus>() != null;
				if (hasNucleus)
				{
					base.inventory.Remove(slot);
					slot.Remove();
				}
			}
			return amount;
		}
		return amount;
	}

	private void SetNucleusData(Item targetItem, int xp)
	{
		if (targetItem != null)
		{
			targetItem.instanceData = new ProtoBuf.Item.InstanceData
			{
				ShouldPool = false,
				dataInt = xp
			};
		}
	}

	private void CheckNucleus()
	{
		if (base.inventory == null)
		{
			return;
		}
		Item slot = base.inventory.GetSlot(0);
		if (slot != null)
		{
			hasNucleus = slot.info.GetComponent<ItemModBeehiveNucleus>() != null;
			if (slot == null || slot.instanceData == null || (slot.instanceData.dataInt == 0 && slot.instanceData.dataFloat == 0f))
			{
				SetNucleusData(slot, 0);
			}
			createNewCombAccumulator = 0f;
		}
		else
		{
			hasNucleus = false;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, hasNucleus);
		}
		if (hasNucleus)
		{
			StartHive();
		}
		else
		{
			StopHive();
		}
	}

	private void StartHive()
	{
		if (!IsInvoking(UpdateGrowthRate))
		{
			InvokeRepeating(UpdateGrowthRate, 0f, updateHiveInterval);
		}
		if (!IsInvoking(GenerateHoneyComb))
		{
			InvokeRepeating(GenerateHoneyComb, updateHiveInterval, updateHiveInterval);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: true);
		}
		CalculateQualifiers(force: true);
		SendNetworkUpdate();
	}

	private void StopHive()
	{
		if (IsInvoking(UpdateGrowthRate))
		{
			CancelInvoke(UpdateGrowthRate);
		}
		if (IsInvoking(GenerateHoneyComb))
		{
			CancelInvoke(GenerateHoneyComb);
		}
		SetFlagLocal(Flags.On, b: false);
		SendNetworkUpdate();
	}

	public float CalculateRain()
	{
		using PooledList<BaseEntity> pooledList = Pool.Get<PooledList<BaseEntity>>();
		SingletonComponent<NpcFireManager>.Instance.GetFiresAround(base.transform.position, 2f, pooledList);
		if (pooledList.Count > 0)
		{
			return 0f;
		}
		if (!IsOutside())
		{
			return 0f;
		}
		return Climate.GetRain(base.transform.position);
	}

	public float CalculateTemperature()
	{
		float temperature = Climate.GetTemperature(base.transform.position);
		float num = GrowableEntity.CalculateArtificialTemperature(base.transform);
		bool flag = num >= 10f;
		bool flag2 = temperature < 10f;
		bool flag3 = temperature < 16f && !flag2;
		if (flag)
		{
			if (flag3)
			{
				return 20f;
			}
			if (flag2)
			{
				return 16f;
			}
		}
		return temperature + num;
	}

	private void CalculateQualifiers(bool force = false)
	{
		using (TimeWarning.New("Beehive.CalculateQualifiers"))
		{
			if (temperatureExposure == null)
			{
				temperatureExposure = new TimeCachedValue<float>
				{
					refreshCooldown = updateHiveStatsInterval,
					refreshRandomRange = 5f,
					updateValue = CalculateTemperature
				};
			}
			if (outsideCheck == null)
			{
				outsideCheck = new TimeCachedValue<bool>
				{
					refreshCooldown = updateHiveStatsInterval,
					refreshRandomRange = 5f,
					updateValue = IsOutsideAccurate
				};
			}
			if (humidityExposure == null)
			{
				humidityExposure = new TimeCachedValue<float>
				{
					refreshCooldown = updateHiveStatsInterval,
					refreshRandomRange = 5f,
					updateValue = CalculateRain
				};
			}
			serverHumidity = humidityExposure.Get(force);
			serverTemperature = temperatureExposure.Get(force);
			serverOutside = outsideCheck.Get(force);
		}
	}

	private void UpdateGrowthRate()
	{
		using (TimeWarning.New("Beehive.UpdateGrowthRate"))
		{
			CalculateQualifiers();
			float num = serverTemperature;
			float num2 = ((num < 28f) ? ((num < 10f) ? 0.010000001f : ((!(num < 16f)) ? 0.1f : 0.05f)) : ((!(num < 40f)) ? 0.010000001f : 0.05f));
			growthRate = num2;
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				switch (NucleusGrading.XpToGrade(slot.instanceData.dataInt))
				{
				case NucleusGrading.NucleusGrade.Grade2:
					growthRate *= 2f;
					break;
				case NucleusGrading.NucleusGrade.Grade1:
					growthRate *= 3f;
					break;
				}
			}
			if (serverHumidity >= 0.5f)
			{
				growthRate *= 0.5f;
			}
			if (!serverOutside)
			{
				growthRate = 0f;
			}
			SendNetworkUpdate();
		}
	}

	public override void DropItems(BaseEntity initiator = null)
	{
		bool flag = false;
		int index = -1;
		for (int i = 0; i < base.inventory.itemList.Count; i++)
		{
			if (base.inventory.itemList[i].info == BeeNucleusDefinition)
			{
				flag = true;
				index = i;
			}
		}
		if (flag && base.inventory.Remove(base.inventory.itemList[index]))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(masterSwarm.resourcePath, base.transform.position + Vector3.up * 1.5f, Quaternion.identity);
			if (creatorEntity is BasePlayer basePlayer)
			{
				baseEntity.creatorEntity = basePlayer;
				baseEntity.OwnerID = basePlayer.userID;
			}
			baseEntity.Spawn();
		}
		base.DropItems(initiator);
	}
}
