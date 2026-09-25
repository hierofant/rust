#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Workbench : StorageContainer
{
	[Serializable]
	public struct UpgradeVisualPoint
	{
		public ItemDefinition upgradeItem;

		public Transform point;

		[Tooltip("Optional prevent-building volume to activate when this upgrade is installed.")]
		public BoxCollider preventBuildingVolume;
	}

	[Serializable]
	public struct CachedUpgradeVisualPoint
	{
		public ItemDefinition upgradeItem;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public Vector3 localScale;

		[Tooltip("Optional prevent-building volume to activate when this upgrade is installed. Also used as the clearance zone — upgrade is blocked from installation if this volume is occupied by a deployable.")]
		public BoxCollider preventBuildingVolume;
	}

	[Serializable]
	public struct UpgradeFillerVisual
	{
		[Tooltip("Transform to toggle on/off. Disabled when any of the associated upgrades are installed.")]
		public Transform fillerTransform;

		[Tooltip("When any of these upgrades are installed, this filler is hidden to make room for the upgrade visual.")]
		public ItemDefinition[] upgrades;
	}

	private struct CachedUpgrade
	{
		public Item item;

		public ItemModWorkbenchUpgrade mod;
	}

	[Header("Upgrades")]
	public int upgradeSlotCount = 4;

	public TriggerComfort upgradeComfortTrigger;

	public SphereCollider upgradeComfortCollider;

	[Tooltip("Maps each upgrade item to its visual spawn point on this workbench.")]
	public UpgradeVisualPoint[] upgradeVisualPoints;

	[Tooltip("Cached local-space positions baked from UpgradeVisualPlacement. Used at runtime instead of the transform hierarchy.")]
	public CachedUpgradeVisualPoint[] cachedUpgradeVisualPoints;

	private int clearanceCacheFrame;

	private ItemDefinition clearanceCacheItem;

	private bool clearanceCacheResult;

	[Tooltip("Editor-only transform holding visual placement points. Should be removed at runtime.")]
	public Transform upgradeVisualPlacement;

	[Header("Filler Visuals")]
	[Tooltip("Active when no upgrades are installed at all. Disabled when any upgrade is present.")]
	public Transform fullFillerVisual;

	[Tooltip("Individual filler transforms that are hidden when their associated upgrade is installed.")]
	public UpgradeFillerVisual[] upgradeFillerVisuals;

	private readonly List<CachedUpgrade> cachedServerUpgrades = new List<CachedUpgrade>();

	public static readonly Translate.Phrase RecycleBinNotEmptyPhrase = new Translate.Phrase("workbench.recyclebin.notempty", "Empty the recycle bin before removing it");

	public const int blueprintSlot = 0;

	public const int experimentSlot = 1;

	public const int firstUpgradeSlot = 2;

	public bool Static;

	public int Workbenchlevel;

	public bool isIOBench;

	public TriggerWorkbench WorkbenchTrigger;

	private const string legacyWorkbenchLootPanel = "workbench";

	private const string upgradeWorkbenchLootPanel = "workbench_upgrades";

	private const string recycleBinLootPanel = "generic_resizable";

	private Vector3 originalCraftTriggerSize = Vector3.zero;

	private Vector3 originalCraftTriggerCenter = Vector3.zero;

	private bool craftTriggerCached;

	private float originalComfortBaseValue = -1f;

	private float originalComfortTriggerRadius = -1f;

	private float originalMaxHealth;

	public LootSpawn experimentalItems;

	public GameObjectRef experimentStartEffect;

	public GameObjectRef experimentSuccessEffect;

	public ItemDefinition experimentResource;

	public TechTreeData[] techTrees;

	private float clientTechTreeMultiplier = 1f;

	public static ItemDefinition blueprintBaseDef;

	private ItemDefinition pendingBlueprint;

	private bool creatingBlueprint;

	public int UpgradeSlotCount => Mathf.Max(0, upgradeSlotCount);

	public int RequiredInventorySlots => 2 + UpgradeSlotCount;

	public int InstalledUpgradeCount
	{
		get
		{
			if (base.inventory == null)
			{
				return 0;
			}
			int num = 0;
			for (int i = 2; i < RequiredInventorySlots; i++)
			{
				if (base.inventory.GetSlot(i) != null)
				{
					num++;
				}
			}
			return num;
		}
	}

	public override bool ValidateMeleeColliderAntihack => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Workbench.OnRpcMessage"))
		{
			if (rpc == 2308794761u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_BeginExperiment");
				}
				using (TimeWarning.New("RPC_BeginExperiment"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2308794761u, "RPC_BeginExperiment", this, player, 3f))
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
							RPC_BeginExperiment(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_BeginExperiment");
					}
				}
				return true;
			}
			if (rpc == 2475703927u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenRecycleBin");
				}
				using (TimeWarning.New("RPC_OpenRecycleBin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2475703927u, "RPC_OpenRecycleBin", this, player, 3f))
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
							RPC_OpenRecycleBin(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_OpenRecycleBin");
					}
				}
				return true;
			}
			if (rpc == 2535666051u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenUpgradeInventory");
				}
				using (TimeWarning.New("RPC_OpenUpgradeInventory"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2535666051u, "RPC_OpenUpgradeInventory", this, player, 3f))
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
							RPCMessage msg4 = rPCMessage;
							RPC_OpenUpgradeInventory(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_OpenUpgradeInventory");
					}
				}
				return true;
			}
			if (rpc == 3268333598u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_SendTechTreeMultiplier");
				}
				using (TimeWarning.New("RPC_SendTechTreeMultiplier"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3268333598u, "RPC_SendTechTreeMultiplier", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							RPC_SendTechTreeMultiplier(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_SendTechTreeMultiplier");
					}
				}
				return true;
			}
			if (rpc == 4127240744u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_TechTreeUnlock");
				}
				using (TimeWarning.New("RPC_TechTreeUnlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4127240744u, "RPC_TechTreeUnlock", this, player, 3f))
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
							RPCMessage msg6 = rPCMessage;
							RPC_TechTreeUnlock(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_TechTreeUnlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsUpgradeSlot(int slot)
	{
		if (slot >= 2)
		{
			return slot < RequiredInventorySlots;
		}
		return false;
	}

	private void RefreshPreventBuildingZones(HashSet<ItemDefinition> installedUpgrades)
	{
		if (cachedUpgradeVisualPoints == null)
		{
			return;
		}
		for (int i = 0; i < cachedUpgradeVisualPoints.Length; i++)
		{
			ref CachedUpgradeVisualPoint reference = ref cachedUpgradeVisualPoints[i];
			if (!(reference.preventBuildingVolume == null))
			{
				bool active = reference.upgradeItem != null && installedUpgrades != null && installedUpgrades.Contains(reference.upgradeItem);
				reference.preventBuildingVolume.gameObject.SetActive(active);
			}
		}
	}

	public float GetTechTreeCostMultiplier()
	{
		if (base.isClient)
		{
			return clientTechTreeMultiplier;
		}
		if (base.isServer)
		{
			return CalculateTechTreeCostMultiplier();
		}
		return 1f;
	}

	public bool IsUpgradeBlockedByClearanceZone(ItemDefinition upgradeDef)
	{
		if (ConVar.Workbench.skipclearancechecks)
		{
			return false;
		}
		int frameCount = UnityEngine.Time.frameCount;
		if (frameCount == clearanceCacheFrame && clearanceCacheItem == upgradeDef)
		{
			return clearanceCacheResult;
		}
		clearanceCacheFrame = frameCount;
		clearanceCacheItem = upgradeDef;
		clearanceCacheResult = IsUpgradeBlockedByClearanceZoneInternal(upgradeDef);
		return clearanceCacheResult;
	}

	private bool IsUpgradeBlockedByClearanceZoneInternal(ItemDefinition upgradeDef)
	{
		if (cachedUpgradeVisualPoints == null)
		{
			return false;
		}
		for (int i = 0; i < cachedUpgradeVisualPoints.Length; i++)
		{
			ref CachedUpgradeVisualPoint reference = ref cachedUpgradeVisualPoints[i];
			if (reference.preventBuildingVolume == null || reference.upgradeItem != upgradeDef)
			{
				continue;
			}
			BoxCollider preventBuildingVolume = reference.preventBuildingVolume;
			Transform transform = preventBuildingVolume.transform;
			Vector3 position = transform.TransformPoint(preventBuildingVolume.center);
			Vector3 lossyScale = transform.lossyScale;
			Vector3 size = Vector3.Scale(preventBuildingVolume.size, lossyScale);
			OBB test = new OBB(position, size, transform.rotation);
			using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
			Vis.Entities(test.position, test.extents.magnitude + 0.25f, pooledList, 256, QueryTriggerInteraction.Ignore);
			for (int j = 0; j < pooledList.Count; j++)
			{
				BaseEntity baseEntity = pooledList[j];
				if (baseEntity == null || baseEntity.IsDestroyed || baseEntity.EqualNetID(this))
				{
					continue;
				}
				if (baseEntity.HasParent())
				{
					BaseEntity baseEntity2 = baseEntity.GetParentEntity();
					if ((object)baseEntity2 != null && baseEntity2.EqualNetID(this))
					{
						continue;
					}
				}
				DeployVolume[] array = PrefabAttribute.server.FindAll<DeployVolume>(baseEntity.prefabID);
				if (array == null || array.Length == 0)
				{
					continue;
				}
				using PooledList<DeployVolume> pooledList2 = Facepunch.Pool.Get<PooledList<DeployVolume>>();
				DeployVolume[] array2 = array;
				foreach (DeployVolume deployVolume in array2)
				{
					if ((deployVolume.ignore & ColliderInfo.Flags.OnlyEvaluatePreventBuildingInMonuments) == 0 && DeployVolume.ShouldApplyVolumeForEntity(deployVolume, this))
					{
						pooledList2.Add(deployVolume);
					}
				}
				if (pooledList2.Count == 0 || !DeployVolume.Check(baseEntity.transform.position, baseEntity.transform.rotation, pooledList2, test, 536870912))
				{
					continue;
				}
				return true;
			}
		}
		return false;
	}

	private void RebuildUpgradeCache()
	{
		cachedServerUpgrades.Clear();
		if (base.inventory == null)
		{
			return;
		}
		for (int i = 2; i < RequiredInventorySlots; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null)
			{
				ItemModWorkbenchUpgrade component = slot.info.GetComponent<ItemModWorkbenchUpgrade>();
				if (!(component == null))
				{
					cachedServerUpgrades.Add(new CachedUpgrade
					{
						item = slot,
						mod = component
					});
				}
			}
		}
	}

	private float CalculateTechTreeCostMultiplier()
	{
		float num = 1f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num *= cachedServerUpgrades[i].mod.GetTechTreeCostMultiplier();
		}
		return num;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_SendTechTreeMultiplier(RPCMessage msg)
	{
		ClientRPC(RpcTarget.Player("RPC_TechTreeMultiplier", msg.player), GetTechTreeCostMultiplier());
	}

	public void SaveUpgrades(ProtoBuf.Workbench wbProto)
	{
		if (wbProto != null)
		{
			wbProto.upgradeItemIds = Facepunch.Pool.Get<List<int>>();
			for (int i = 2; i < RequiredInventorySlots; i++)
			{
				Item slot = base.inventory.GetSlot(i);
				wbProto.upgradeItemIds.Add(slot?.info.itemid ?? 0);
			}
		}
	}

	private void RefreshRuntimeUpgrades()
	{
		RefreshReinforcedHealth();
		RefreshCraftRange();
		RefreshComfort();
		RefreshServerPreventBuildingZones();
	}

	private void OnUpgradeAddedOrRemoved(Item item, bool added)
	{
		if (added)
		{
			NotifyUpgradeInstalled(item);
			DoUpgradeEffect(item);
		}
		else
		{
			NotifyUpgradeRemoved(item);
		}
		RebuildUpgradeCache();
		RefreshRuntimeUpgrades();
		SendTechTreeMultiplierToGroup();
		SendNetworkUpdateImmediate();
	}

	private void SendTechTreeMultiplierToGroup()
	{
		ClientRPC(RpcTarget.NetworkGroup("RPC_TechTreeMultiplier"), GetTechTreeCostMultiplier());
	}

	private void RefreshReinforcedHealth()
	{
		if (originalMaxHealth <= 0f)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			if (cachedServerUpgrades[i].mod is ItemModWorkbenchReinforced itemModWorkbenchReinforced)
			{
				num += itemModWorkbenchReinforced.GetHealthBonusForWorkbench(Workbenchlevel, isIOBench);
			}
		}
		if (num > 0f)
		{
			OverrideMaxHealth(originalMaxHealth + num, sendNetworkUpdate: true, clampHealth: false);
		}
		else
		{
			OverrideMaxHealth(0f);
		}
	}

	private void RefreshCraftRange()
	{
		if (craftTriggerCached && !(WorkbenchTrigger == null))
		{
			BoxCollider component = WorkbenchTrigger.GetComponent<BoxCollider>();
			if (!(component == null))
			{
				float rangeMultiplier = GetRangeMultiplier();
				float num = originalCraftTriggerSize.z * (rangeMultiplier - 1f);
				component.size = new Vector3(originalCraftTriggerSize.x, originalCraftTriggerSize.y, originalCraftTriggerSize.z * rangeMultiplier);
				component.center = new Vector3(originalCraftTriggerCenter.x, originalCraftTriggerCenter.y, originalCraftTriggerCenter.z + num * 0.5f);
			}
		}
	}

	private void RefreshComfort()
	{
		if (upgradeComfortTrigger == null)
		{
			return;
		}
		float maxComfortOverride = GetMaxComfortOverride();
		bool flag = maxComfortOverride > 0f;
		upgradeComfortTrigger.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		float a = ((originalComfortBaseValue >= 0f) ? originalComfortBaseValue : 0f);
		upgradeComfortTrigger.baseComfort = Mathf.Max(a, maxComfortOverride);
		if (originalComfortTriggerRadius > 0f)
		{
			float num = (ConVar.Workbench.scalecomfortradius ? GetRangeMultiplier() : 1f);
			float num2 = ((num > 1f) ? (num * ConVar.Workbench.comfortradiusscale) : 1f);
			if (upgradeComfortCollider != null)
			{
				upgradeComfortCollider.radius = originalComfortTriggerRadius * num2;
				upgradeComfortTrigger.triggerSize = upgradeComfortCollider.radius * upgradeComfortTrigger.transform.localScale.y;
			}
		}
	}

	private void RefreshServerPreventBuildingZones()
	{
		if (cachedUpgradeVisualPoints == null || cachedUpgradeVisualPoints.Length == 0)
		{
			return;
		}
		using PooledHashSet<ItemDefinition> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<ItemDefinition>>();
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			pooledHashSet.Add(cachedServerUpgrades[i].item.info);
		}
		RefreshPreventBuildingZones(pooledHashSet);
	}

	private float GetMaxComfortOverride()
	{
		float num = 0f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num = Mathf.Max(num, cachedServerUpgrades[i].mod.GetMinComfortLevel());
		}
		return num;
	}

	public void ApplyUpgradesToCraftedItem(BasePlayer crafter, ItemCraftTask task, Item craftedItem)
	{
		if (craftedItem != null)
		{
			for (int i = 0; i < cachedServerUpgrades.Count; i++)
			{
				CachedUpgrade cachedUpgrade = cachedServerUpgrades[i];
				cachedUpgrade.mod.ApplyToCraftedItem(this, crafter, task, craftedItem, cachedUpgrade.item);
			}
		}
	}

	public void GiveBonusItems(BasePlayer crafter, ItemCraftTask task, Item craftedItem)
	{
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			CachedUpgrade cachedUpgrade = cachedServerUpgrades[i];
			obj.Clear();
			cachedUpgrade.mod.GetBonusItems(this, crafter, task, craftedItem, cachedUpgrade.item, obj);
			foreach (Item item in obj)
			{
				item.OnVirginSpawn(crafter);
				item.SetItemOwnership(crafter, ItemOwnershipPhrases.CraftedPhrase);
				if (!cachedUpgrade.mod.TryGiveBonusItem(this, crafter, cachedUpgrade.item, item) && !crafter.inventory.GiveItem(item))
				{
					item.Drop(crafter.inventory.containerMain.dropPosition, crafter.inventory.containerMain.dropVelocity);
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void CollectBonusItems(BasePlayer crafter, ItemCraftTask task, Item craftedItem, List<Item> overflow, string ownerUsername, Translate.Phrase ownershipReason)
	{
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			CachedUpgrade cachedUpgrade = cachedServerUpgrades[i];
			obj.Clear();
			cachedUpgrade.mod.GetBonusItems(this, crafter, task, craftedItem, cachedUpgrade.item, obj);
			foreach (Item item in obj)
			{
				item.SetItemOwnership(ownerUsername, ownershipReason);
				if (!cachedUpgrade.mod.TryGiveBonusItem(this, crafter, cachedUpgrade.item, item))
				{
					overflow.Add(item);
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void NotifyUpgradeInstalled(Item upgradeItem)
	{
		(upgradeItem?.info.GetComponent<ItemModWorkbenchUpgrade>())?.OnUpgradeInstalled(this, upgradeItem);
	}

	public void NotifyUpgradeRemoved(Item upgradeItem)
	{
		(upgradeItem?.info.GetComponent<ItemModWorkbenchUpgrade>())?.OnUpgradeRemoved(this, upgradeItem);
	}

	public float GetCraftSpeedMultiplier(ItemCraftTask task)
	{
		float num = 1f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num *= cachedServerUpgrades[i].mod.GetCraftSpeedMultiplier(task);
		}
		return num;
	}

	public float GetRangeMultiplier()
	{
		float num = 1f;
		for (int i = 0; i < cachedServerUpgrades.Count; i++)
		{
			num *= cachedServerUpgrades[i].mod.GetRangeMultiplier();
		}
		return num;
	}

	private void DoUpgradeEffect(Item upgradeItem)
	{
		ItemModWorkbenchUpgrade itemModWorkbenchUpgrade = upgradeItem?.info.GetComponent<ItemModWorkbenchUpgrade>();
		if (itemModWorkbenchUpgrade != null && itemModWorkbenchUpgrade.installEffectPrefab.isValid)
		{
			Effect.server.Run(itemModWorkbenchUpgrade.installEffectPrefab.resourcePath, base.transform.position);
		}
	}

	private PooledList<ItemModWorkbenchUpgrade> GetInstalledUpgradeMods()
	{
		PooledList<ItemModWorkbenchUpgrade> pooledList = Facepunch.Pool.Get<PooledList<ItemModWorkbenchUpgrade>>();
		if (base.isServer)
		{
			for (int i = 0; i < cachedServerUpgrades.Count; i++)
			{
				pooledList.Add(cachedServerUpgrades[i].mod);
			}
		}
		return pooledList;
	}

	public bool HasTechTreeBypassUpgrade()
	{
		using PooledList<ItemModWorkbenchUpgrade> pooledList = GetInstalledUpgradeMods();
		for (int i = 0; i < pooledList.Count; i++)
		{
			if (pooledList[i].CanBypassTechTreePath())
			{
				return true;
			}
		}
		return false;
	}

	public float GetTechTreeFailChance()
	{
		float num = 0f;
		using PooledList<ItemModWorkbenchUpgrade> pooledList = GetInstalledUpgradeMods();
		for (int i = 0; i < pooledList.Count; i++)
		{
			num = Mathf.Max(num, pooledList[i].GetTechTreeFailChance());
		}
		return num;
	}

	public float GetBypassCostMultiplier()
	{
		float num = 1f;
		using PooledList<ItemModWorkbenchUpgrade> pooledList = GetInstalledUpgradeMods();
		for (int i = 0; i < pooledList.Count; i++)
		{
			num *= pooledList[i].GetBypassCostMultiplier();
		}
		return num;
	}

	public IEnumerable<TechTreeData> GetTechTrees()
	{
		TechTreeData[] array = techTrees;
		foreach (TechTreeData techTreeData in array)
		{
			if (techTreeData.IsAllowedInEra(ConVar.Server.Era) && techTreeData.IsAllowedInGameMode(base.isServer))
			{
				yield return techTreeData;
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public TechTreeData GetTechTreeForLevel(int level)
	{
		foreach (TechTreeData techTree in GetTechTrees())
		{
			if (techTree.techTreeLevel == level)
			{
				return techTree;
			}
		}
		return null;
	}

	public int GetScrapForExperiment()
	{
		if (Workbenchlevel == 1)
		{
			return 75;
		}
		if (Workbenchlevel == 2)
		{
			return 300;
		}
		if (Workbenchlevel == 3)
		{
			return 1000;
		}
		Debug.LogWarning("GetScrapForExperiment fucked up big time.");
		return 0;
	}

	public bool IsWorking()
	{
		return HasFlag(Flags.On);
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (children.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (string.IsNullOrWhiteSpace(panelToOpen))
		{
			panelToOpen = (ConVar.Server.useLegacyWorkbenchInteraction ? "workbench" : "workbench_upgrades");
		}
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenUpgradeInventory(RPCMessage msg)
	{
		if (isLootable && !Static)
		{
			BasePlayer player = msg.player;
			if ((bool)player && player.CanInteract() && player.CanBuild())
			{
				PlayerOpenLoot(player, "workbench_upgrades");
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_TechTreeUnlock(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		int id = msg.read.Int32();
		int level = msg.read.Int32();
		bool flag = msg.read.Bit();
		TechTreeData techTreeForLevel = GetTechTreeForLevel(level);
		if (techTreeForLevel == null || player.currentCraftLevel <= (float)techTreeForLevel.techTreeLevel || player.IsInTutorial || (flag && !HasTechTreeBypassUpgrade()))
		{
			return;
		}
		TechTreeData.NodeInstance byID = techTreeForLevel.GetByID(id);
		if (byID == null)
		{
			Debug.Log("Node for unlock not found :" + id);
		}
		else
		{
			if (Interface.CallHook("OnTechTreeNodeUnlock", this, byID, player) != null)
			{
				return;
			}
			int itemid = ItemManager.FindItemDefinition("scrap").itemid;
			int amount = player.inventory.GetAmount(itemid);
			if (!flag)
			{
				using (PooledList<TechTreeData.NodeInstance> pooledList = Facepunch.Pool.Get<PooledList<TechTreeData.NodeInstance>>())
				{
					techTreeForLevel.GetNodesRequiredToUnlock(player, byID, pooledList);
					for (int num = pooledList.Count - 1; num >= 0; num--)
					{
						TechTreeData.NodeInstance nodeInstance = pooledList[num];
						if (nodeInstance.itemDef == null || player.blueprints.HasUnlocked(nodeInstance.itemDef))
						{
							pooledList.RemoveAt(num);
						}
					}
					using PooledList<ItemDefinition> pooledList2 = Facepunch.Pool.Get<PooledList<ItemDefinition>>();
					int num2 = 0;
					foreach (TechTreeData.NodeInstance item in pooledList)
					{
						if (item != null && !(item.itemDef == null))
						{
							num2 += ScrapForResearch(item.itemDef, techTreeForLevel.techTreeLevel, out var tax, this);
							num2 += tax;
							pooledList2.Add(item.itemDef);
						}
					}
					if (amount >= num2)
					{
						foreach (TechTreeData.NodeInstance item2 in pooledList)
						{
							if (item2.IsGroup())
							{
								foreach (int output in item2.outputs)
								{
									TechTreeData.NodeInstance byID2 = techTreeForLevel.GetByID(output);
									if (byID2 != null && byID2.itemDef != null)
									{
										player.blueprints.Unlock(byID2.itemDef);
										Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, byID2.itemDef, "techtree", 0, this);
									}
								}
								Debug.Log("Player unlocked group :" + item2.groupName);
							}
						}
						player.inventory.Take(null, itemid, num2);
						player.blueprints.UnlockList(pooledList2);
						Interface.CallHook("OnTechTreeNodeUnlocked", this, byID, player, pooledList2);
						{
							foreach (ItemDefinition item3 in pooledList2)
							{
								int tax2;
								int num3 = ScrapForResearch(item3, techTreeForLevel.techTreeLevel, out tax2, this);
								Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, item3, "techtree", num3 + tax2, this);
							}
							return;
						}
					}
					return;
				}
			}
			if (byID.itemDef == null || player.blueprints.HasUnlocked(byID.itemDef))
			{
				return;
			}
			int tax3;
			int num4 = Mathf.RoundToInt((float)(ScrapForResearch(byID.itemDef, techTreeForLevel.techTreeLevel, out tax3, this) + tax3) * GetBypassCostMultiplier());
			if (amount >= num4)
			{
				player.inventory.Take(null, itemid, num4);
				float techTreeFailChance = GetTechTreeFailChance();
				if (UnityEngine.Random.value < techTreeFailChance)
				{
					ClientRPC(RpcTarget.Player("RPC_PrototypeFailed", player));
					return;
				}
				player.blueprints.Unlock(byID.itemDef);
				Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, byID.itemDef, "techtree_prototype", num4, this);
			}
		}
	}

	public static ItemDefinition GetBlueprintTemplate()
	{
		if (blueprintBaseDef == null)
		{
			blueprintBaseDef = ItemManager.FindItemDefinition("blueprintbase");
		}
		return blueprintBaseDef;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_BeginExperiment(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || IsWorking())
		{
			return;
		}
		PersistantPlayer persistantPlayerInfo = player.PersistantPlayerInfo;
		int num = UnityEngine.Random.Range(0, experimentalItems.subSpawn.Length);
		for (int i = 0; i < experimentalItems.subSpawn.Length; i++)
		{
			int num2 = i + num;
			if (num2 >= experimentalItems.subSpawn.Length)
			{
				num2 -= experimentalItems.subSpawn.Length;
			}
			ItemDefinition itemDef = experimentalItems.subSpawn[num2].category.items[0].itemDef;
			if ((bool)itemDef.Blueprint && !itemDef.Blueprint.defaultBlueprint && itemDef.Blueprint.userCraftable && itemDef.Blueprint.isResearchable && !itemDef.Blueprint.NeedsSteamItem && !itemDef.Blueprint.NeedsSteamDLC && !persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
			{
				pendingBlueprint = itemDef;
				break;
			}
		}
		if (pendingBlueprint == null)
		{
			player.ChatMessage("You have already unlocked everything for this workbench tier.");
		}
		else
		{
			if (Interface.CallHook("OnExperimentStart", this, player) != null)
			{
				return;
			}
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				if (!slot.MoveToContainer(player.inventory.containerMain))
				{
					slot.Drop(GetDropPosition(), GetDropVelocity());
				}
				player.inventory.loot.SendImmediate();
			}
			if (experimentStartEffect.isValid)
			{
				Effect.server.Run(experimentStartEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
			SetFlagLocal(Flags.On, b: true);
			base.inventory.SetLocked(isLocked: true);
			CancelInvoke(ExperimentComplete);
			Invoke(ExperimentComplete, 5f);
			SendNetworkUpdate();
			Interface.CallHook("OnExperimentStarted", this, player);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.inventory != null)
		{
			info.msg.workbench = Facepunch.Pool.Get<ProtoBuf.Workbench>();
			SaveUpgrades(info.msg.workbench);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		CancelInvoke(ExperimentComplete);
	}

	public int GetAvailableExperimentResources()
	{
		Item experimentResourceItem = GetExperimentResourceItem();
		if (experimentResourceItem == null || experimentResourceItem.info != experimentResource)
		{
			return 0;
		}
		return experimentResourceItem.amount;
	}

	public Item GetExperimentResourceItem()
	{
		return base.inventory.GetSlot(1);
	}

	public void ExperimentComplete()
	{
		Item experimentResourceItem = GetExperimentResourceItem();
		int scrapForExperiment = GetScrapForExperiment();
		if (pendingBlueprint == null)
		{
			Debug.LogWarning("Pending blueprint was null!");
		}
		if (Interface.CallHook("OnExperimentEnd", this) != null)
		{
			return;
		}
		if (experimentResourceItem != null && experimentResourceItem.amount >= scrapForExperiment && pendingBlueprint != null)
		{
			experimentResourceItem.UseItem(scrapForExperiment);
			Item item = ItemManager.Create(GetBlueprintTemplate(), 1, 0uL, isServerSide: true, 0uL);
			item.blueprintTarget = pendingBlueprint.itemid;
			creatingBlueprint = true;
			if (!item.MoveToContainer(base.inventory, 0))
			{
				item.Drop(GetDropPosition(), GetDropVelocity());
			}
			creatingBlueprint = false;
			if (experimentSuccessEffect.isValid)
			{
				Effect.server.Run(experimentSuccessEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
		SetFlagLocal(Flags.On, b: false);
		pendingBlueprint = null;
		base.inventory.SetLocked(isLocked: false);
		SendNetworkUpdate();
		Interface.CallHook("OnExperimentEnded", this);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (base.inventory != null)
		{
			base.inventory.SetLocked(isLocked: false);
		}
		RebuildUpgradeCache();
		RefreshRuntimeUpgrades();
	}

	public override void ServerInit()
	{
		inventorySlots = Mathf.Max(inventorySlots, RequiredInventorySlots);
		base.ServerInit();
		base.inventory.canAcceptItem = ItemFilter;
		originalMaxHealth = startHealth;
		CacheOriginalTriggerValues();
	}

	private void CacheOriginalTriggerValues()
	{
		if (WorkbenchTrigger != null && !craftTriggerCached)
		{
			BoxCollider component = WorkbenchTrigger.GetComponent<BoxCollider>();
			if (component != null)
			{
				originalCraftTriggerSize = component.size;
				originalCraftTriggerCenter = component.center;
				craftTriggerCached = true;
			}
		}
		if (upgradeComfortTrigger != null && originalComfortBaseValue < 0f)
		{
			originalComfortBaseValue = upgradeComfortTrigger.baseComfort;
			if (upgradeComfortCollider != null)
			{
				originalComfortTriggerRadius = upgradeComfortCollider.radius;
			}
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (!Rust.Application.isLoadingSave && item != null && base.inventory != null && IsUpgradeSlot(item.position))
		{
			OnUpgradeAddedOrRemoved(item, added);
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if ((targetSlot != 1 || !(item.info == experimentResource)) && targetSlot == 0)
		{
			_ = creatingBlueprint;
		}
		if (IsUpgradeSlot(targetSlot))
		{
			if (Static)
			{
				return false;
			}
			Item slot = base.inventory.GetSlot(targetSlot);
			if (slot != null && slot.contents != null && !slot.contents.IsEmpty())
			{
				return false;
			}
			ItemModWorkbenchUpgrade component = item.info.GetComponent<ItemModWorkbenchUpgrade>();
			if (component != null && component.CanInstallInWorkbench(this, item, targetSlot))
			{
				return !IsUpgradeBlockedByClearanceZone(item.info);
			}
			return false;
		}
		return false;
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		PlayerInventory.CanMoveFromResponse result = base.CanMoveFrom(player, item);
		if (!result.allowed)
		{
			return result;
		}
		if (item.parent == base.inventory && IsUpgradeSlot(item.position) && item.contents != null && !item.contents.IsEmpty())
		{
			return PlayerInventory.CanMoveFromResponse.Failure(RecycleBinNotEmptyPhrase);
		}
		return result;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_OpenRecycleBin(RPCMessage msg)
	{
		if (!isLootable || Static)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!player || !player.CanInteract())
		{
			return;
		}
		Item recycleBinUpgradeItem = GetRecycleBinUpgradeItem();
		if (recycleBinUpgradeItem?.contents != null)
		{
			if (IsLocked() || IsTransferring())
			{
				player.ShowToast(GameTip.Styles.Red_Normal, StorageContainer.LockedMessage, false);
			}
			else if (onlyOneUser && IsOpen())
			{
				player.ShowToast(GameTip.Styles.Red_Normal, StorageContainer.InUseMessage, false);
			}
			else if (CanOpenLootPanel(player, "generic_resizable") && player.inventory.loot.StartLootingEntity(this))
			{
				SetFlagLocal(Flags.Open, b: true);
				player.inventory.loot.AddContainer(recycleBinUpgradeItem.contents);
				player.inventory.loot.SendImmediate();
				player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), "generic_resizable");
				SendNetworkUpdate();
			}
		}
	}

	private Item GetRecycleBinUpgradeItem()
	{
		for (int i = 2; i < RequiredInventorySlots; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null && slot.info.GetComponent<ItemModWorkbenchRecycleBin>() != null)
			{
				return slot;
			}
		}
		return null;
	}

	public static int ScrapForResearch(ItemDefinition info, int workbenchLevel, out int tax, Workbench workbench = null)
	{
		int num = 0;
		if (info.rarity == Rarity.Common)
		{
			num = 15;
		}
		if (info.rarity == Rarity.Uncommon)
		{
			num = 30;
		}
		if (info.rarity == Rarity.Rare)
		{
			num = 60;
		}
		if (info.rarity == Rarity.VeryRare || info.rarity == Rarity.None)
		{
			num = 120;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null)
		{
			BaseGameMode.ResearchCostResult scrapCostForResearch = activeGameMode.GetScrapCostForResearch(info, ResearchTable.ResearchType.TechTree);
			if (scrapCostForResearch.Scale.HasValue)
			{
				num = Mathf.RoundToInt((float)num * scrapCostForResearch.Scale.Value);
			}
			else if (scrapCostForResearch.Amount.HasValue)
			{
				num = scrapCostForResearch.Amount.Value;
			}
		}
		float taxRateForWorkbenchUnlock = ConVar.Server.GetTaxRateForWorkbenchUnlock(workbenchLevel);
		tax = 0;
		if (taxRateForWorkbenchUnlock > 0f)
		{
			tax = Mathf.CeilToInt((float)num * (taxRateForWorkbenchUnlock / 100f));
		}
		if (workbench != null)
		{
			float techTreeCostMultiplier = workbench.GetTechTreeCostMultiplier();
			num = Mathf.RoundToInt((float)num * techTreeCostMultiplier);
			tax = Mathf.RoundToInt((float)tax * techTreeCostMultiplier);
		}
		return num;
	}

	public override void ScaleDamage(HitInfo info)
	{
		base.ScaleDamage(info);
		if (base.inventory == null)
		{
			return;
		}
		for (int i = 2; i < RequiredInventorySlots; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot == null)
			{
				continue;
			}
			ItemModWorkbenchUpgrade component = slot.info.GetComponent<ItemModWorkbenchUpgrade>();
			if (!(component == null))
			{
				float explosiveDamageReduction = component.GetExplosiveDamageReduction();
				if (!(explosiveDamageReduction <= 0f))
				{
					info.damageTypes.Scale(DamageType.Explosion, 1f - Mathf.Clamp01(explosiveDamageReduction));
				}
			}
		}
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
