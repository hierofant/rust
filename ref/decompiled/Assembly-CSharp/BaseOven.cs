#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class BaseOven : StorageContainer, ISplashable, IIndustrialStorage, IAlwaysOn
{
	public class CookingItem
	{
		public ItemId itemInstanceID;

		public int slotIndex;

		public float cookingProgress;

		public int itemID;

		public int byproductItemID;

		public int initialStackSize;
	}

	public enum TemperatureType
	{
		Normal,
		Warming,
		Cooking,
		Smelting,
		Fractioning,
		Compost
	}

	public enum IndustrialSlotMode
	{
		Furnace,
		LargeFurnace,
		OilRefinery,
		ElectricFurnace
	}

	public class BaseOvenWorkQueue : PersistentObjectWorkQueue<BaseOven>
	{
		protected override void RunJob(BaseOven oven)
		{
			if ((float)oven.lastCookUpdate > 0.5f)
			{
				float num = Mathf.Floor((float)oven.lastCookUpdate / 0.5f);
				oven.lastCookUpdate = (float)oven.lastCookUpdate - num * 0.5f;
				oven.Cook(0.5f * num);
			}
			if (oven.visualFood && (float)oven.lastCookVisualsUpdate > 0.05f)
			{
				oven.lastCookVisualsUpdate = 0f;
				oven.CookVisuals();
			}
		}
	}

	public struct MinMax
	{
		public int Min;

		public int Max;

		public MinMax(int min, int max)
		{
			Min = min;
			Max = max;
		}
	}

	public enum OvenItemType
	{
		Burnable,
		Byproduct,
		MaterialInput,
		MaterialOutput
	}

	private List<CookingItem> cookingItems = new List<CookingItem>();

	public bool visualFood;

	public SoundDefinition sizzlingSoundDef;

	private Sound sizzlingSound;

	private static Dictionary<float, HashSet<ItemDefinition>> _materialOutputCache;

	[Header("Base Oven Settings")]
	public TemperatureType temperature;

	public Menu.Option switchOnMenu;

	public Menu.Option switchOffMenu;

	public ItemAmount[] startupContents;

	public bool allowByproductCreation = true;

	public ItemDefinition fuelType;

	[FormerlySerializedAs("canModFire")]
	public bool hasFireDeploySlot;

	public bool hasOpenFlame;

	public bool disabledBySplash = true;

	public int smeltSpeed = 1;

	public int fuelSlots = 1;

	public int inputSlots = 1;

	public int outputSlots = 1;

	public IndustrialSlotMode IndustrialMode;

	public const Flags Flag_CookingPaused = Flags.Reserved8;

	public const Flags AlwaysOn = Flags.Reserved9;

	public const Flags Flag_IsCooking = Flags.Reserved10;

	public int _activeCookingSlot = -1;

	public int _inputSlotIndex;

	public int _outputSlotIndex;

	public static BaseOvenWorkQueue cookQueue = new BaseOvenWorkQueue();

	private TimeSince lastCookUpdate;

	private TimeSince lastCookVisualsUpdate;

	public const float UpdateRate = 0.5f;

	private const float VisualUpdateRate = 0.05f;

	public virtual string TitleItemShortname => "furnace";

	public virtual bool ShowFuelInLootPanel => true;

	protected virtual bool AutomaticallyStartCooking => false;

	public virtual bool CanRunWithNoFuel
	{
		get
		{
			if (IsAlwaysOn())
			{
				return true;
			}
			return false;
		}
	}

	public ItemContainer Container => base.inventory;

	public BaseEntity IndustrialEntity => this;

	public float cookingTemperature => temperature switch
	{
		TemperatureType.Fractioning => 1500f, 
		TemperatureType.Cooking => 200f, 
		TemperatureType.Smelting => 1000f, 
		TemperatureType.Warming => 50f, 
		TemperatureType.Compost => 30f, 
		_ => 15f, 
	};

	public override bool PreserveChildrenWhenReskinning => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseOven.OnRpcMessage"))
		{
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SVSwitch");
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4167839872u, "SVSwitch", this, player, 3f))
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
							SVSwitch(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void OnItemAddedOrRemovedVisualFood(Item item, bool bAdded)
	{
		if (bAdded)
		{
			for (int i = _inputSlotIndex; i <= _inputSlotIndex + inputSlots - 1; i++)
			{
				if (base.inventory.GetSlot(i) == item)
				{
					AddVisualFood(item, i - _inputSlotIndex);
					break;
				}
			}
		}
		if (!bAdded)
		{
			if (item.uid.Value != 0L)
			{
				TryRemoveVisualFood(item);
			}
			RemoveFinishedVisualFood(item);
		}
	}

	public override void OnItemPositionChanged(Item item, int from, int to)
	{
		base.OnItemPositionChanged(item, from, to);
		if (from < _inputSlotIndex || to > _inputSlotIndex + inputSlots)
		{
			return;
		}
		foreach (CookingItem cookingItem in cookingItems)
		{
			if (cookingItem.itemInstanceID.Value == item.uid.Value)
			{
				MoveVisualFood(item, to);
			}
		}
	}

	public override void OnItemAddedToStack(Item item, int amount)
	{
		UpdateVisualFoodAmount(item);
	}

	public override void OnItemRemovedFromStack(Item item, int amount)
	{
		UpdateVisualFoodAmount(item);
	}

	private void UpdateVisualFoodAmount(Item item)
	{
		foreach (CookingItem cookingItem in cookingItems)
		{
			if (cookingItem.itemInstanceID.Value == item.uid.Value)
			{
				cookingItem.cookingProgress = 0f;
				cookingItem.initialStackSize = item.amount;
			}
		}
	}

	private void MoveVisualFood(Item item, int to)
	{
		foreach (CookingItem cookingItem in cookingItems)
		{
			if (cookingItem.itemInstanceID.Value == item.uid.Value)
			{
				cookingItem.slotIndex = to - _inputSlotIndex;
			}
		}
		SendNetworkUpdate();
	}

	public void AddVisualFood(Item item, int slot)
	{
		if (!visualFood)
		{
			return;
		}
		CookingItem cookingItem = cookingItems.Find((CookingItem x) => x.slotIndex == slot);
		if (cookingItem != null)
		{
			if (cookingItem.cookingProgress != 1f)
			{
				return;
			}
			TryRemoveVisualFood(cookingItem.itemInstanceID);
		}
		if (item != null)
		{
			CookingItem item2 = new CookingItem
			{
				itemInstanceID = item.uid,
				itemID = item.info.itemid,
				byproductItemID = item.info.ItemModCookable.becomeOnCooked.itemid,
				slotIndex = slot,
				initialStackSize = item.amount,
				cookingProgress = 0f
			};
			cookingItems.Add(item2);
		}
		SendNetworkUpdate();
	}

	public void TryRemoveVisualFood(Item item)
	{
		TryRemoveVisualFood(item.uid);
	}

	public void TryRemoveVisualFood(ItemId itemId)
	{
		for (int num = cookingItems.Count - 1; num >= 0; num--)
		{
			if (cookingItems[num].itemInstanceID == itemId)
			{
				cookingItems.RemoveAt(num);
			}
		}
		SendNetworkUpdate();
	}

	private void RemoveFinishedVisualFood(Item cookedItem)
	{
		for (int num = cookingItems.Count - 1; num >= 0; num--)
		{
			if (cookingItems[num].cookingProgress == 1f && cookingItems[num].byproductItemID == cookedItem.info.itemid)
			{
				bool flag = true;
				for (int i = _outputSlotIndex; i <= _outputSlotIndex + outputSlots - 1; i++)
				{
					Item slot = base.inventory.GetSlot(i);
					if (slot != null && slot.info.itemid == cookedItem.info.itemid)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					cookingItems.RemoveAt(num);
				}
			}
		}
		SendNetworkUpdate();
	}

	private void SaveVisualFood(SaveInfo info)
	{
		info.msg.baseOven.cookingItems = Facepunch.Pool.Get<List<ProtoBuf.BaseOven.CookingItem>>();
		foreach (CookingItem cookingItem2 in cookingItems)
		{
			ProtoBuf.BaseOven.CookingItem cookingItem = Facepunch.Pool.Get<ProtoBuf.BaseOven.CookingItem>();
			cookingItem.itemID = cookingItem2.itemID;
			cookingItem.slotIndex = cookingItem2.slotIndex;
			cookingItem.initialStackSize = cookingItem2.initialStackSize;
			cookingItem.cookingProgress = cookingItem2.cookingProgress;
			info.msg.baseOven.cookingItems.Add(cookingItem);
		}
	}

	private void CookVisuals()
	{
		foreach (CookingItem cookingItem in cookingItems)
		{
			if (base.inventory != null && cookingItem != null && cookingItem.cookingProgress != 1f)
			{
				Item slot = base.inventory.GetSlot(cookingItem.slotIndex + _inputSlotIndex);
				if (slot == null)
				{
					cookingItem.cookingProgress = 1f;
					continue;
				}
				float cookTime = slot.info.ItemModCookable.cookTime;
				float cookingProgress = ((float)(cookingItem.initialStackSize - slot.amount) + (1f - slot.cookTimeLeft / cookTime)) / (float)cookingItem.initialStackSize;
				cookingItem.cookingProgress = cookingProgress;
			}
		}
	}

	public override void PreInitShared()
	{
		base.PreInitShared();
		_inputSlotIndex = fuelSlots;
		_outputSlotIndex = _inputSlotIndex + inputSlots;
		_activeCookingSlot = _inputSlotIndex;
	}

	public override void ServerInit()
	{
		inventorySlots = fuelSlots + inputSlots + outputSlots;
		base.ServerInit();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (AutomaticallyStartCooking)
		{
			UpdateAutoState();
		}
		if (IsOn())
		{
			StartCooking();
		}
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		cookQueue.Remove(this);
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public override void OnInventoryFirstCreated(ItemContainer container)
	{
		base.OnInventoryFirstCreated(container);
		if (startupContents != null)
		{
			ItemAmount[] array = startupContents;
			foreach (ItemAmount itemAmount in array)
			{
				ItemManager.Create(itemAmount.itemDef, (int)itemAmount.amount, 0uL, isServerSide: true, 0uL).MoveToContainer(container);
			}
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		if (item != null)
		{
			CookableItemInfo cookableItemInfo = ((this is Composter) ? item.info.ItemModCompostable : item.info.ItemModCookable);
			if (cookableItemInfo != null)
			{
				item.cookTimeLeft = cookableItemInfo.cookTime;
			}
			if (item.HasFlag(Item.Flag.OnFire))
			{
				item.SetFlag(Item.Flag.OnFire, b: false);
				item.MarkDirty();
			}
			if (item.HasFlag(Item.Flag.Cooking))
			{
				item.SetFlag(Item.Flag.Cooking, b: false);
				item.MarkDirty();
			}
		}
		if (AutomaticallyStartCooking)
		{
			UpdateAutoState();
		}
		if (visualFood)
		{
			OnItemAddedOrRemovedVisualFood(item, bAdded);
		}
		UpdateIsCooking();
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (!base.ItemFilter(player, item, targetSlot))
		{
			return false;
		}
		if (targetSlot == -1)
		{
			return false;
		}
		if (IsOutputItem(item) && player != null)
		{
			return false;
		}
		MinMax? allowedSlots = GetAllowedSlots(item);
		if (!allowedSlots.HasValue)
		{
			return false;
		}
		if (targetSlot >= allowedSlots.Value.Min)
		{
			return targetSlot <= allowedSlots.Value.Max;
		}
		return false;
	}

	public MinMax? GetAllowedSlots(Item item)
	{
		int num = 0;
		int num2 = 0;
		if (IsBurnableItem(item))
		{
			num2 = fuelSlots;
		}
		else if (IsOutputItem(item))
		{
			num = _outputSlotIndex;
			num2 = num + outputSlots;
		}
		else
		{
			if (!IsMaterialInput(item))
			{
				return null;
			}
			num = _inputSlotIndex;
			num2 = num + inputSlots;
		}
		return new MinMax(num, num2 - 1);
	}

	public MinMax GetOutputSlotRange()
	{
		return new MinMax(_outputSlotIndex, _outputSlotIndex + outputSlots - 1);
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		MinMax? allowedSlots = GetAllowedSlots(item);
		if (!allowedSlots.HasValue)
		{
			return -1;
		}
		for (int i = allowedSlots.Value.Min; i <= allowedSlots.Value.Max; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot == null || (slot.CanStack(item) && slot.amount < slot.MaxStackable()))
			{
				return i;
			}
		}
		return base.GetIdealSlot(player, container, item);
	}

	public virtual void OvenFull()
	{
		StopCooking();
	}

	public int GetFuelRate()
	{
		return 1;
	}

	public int GetCharcoalRate()
	{
		return 1;
	}

	public void Cook(float deltaTime)
	{
		if (HasFlag(Flags.Reserved8))
		{
			return;
		}
		Item item = FindBurnable();
		if (Interface.CallHook("OnOvenCook", this, item) != null)
		{
			return;
		}
		if (item == null && !CanRunWithNoFuel)
		{
			StopCooking();
			return;
		}
		foreach (Item item2 in base.inventory.itemList)
		{
			if (item2.position >= _inputSlotIndex && item2.position < _inputSlotIndex + inputSlots && !item2.HasFlag(Item.Flag.Cooking))
			{
				item2.SetFlag(Item.Flag.Cooking, b: true);
				item2.MarkDirty();
			}
		}
		IncreaseCookTime(deltaTime * GetSmeltingSpeed());
		BaseEntity slot = GetSlot(Slot.FireMod);
		if ((bool)slot)
		{
			slot.SendMessage("Cook", deltaTime, SendMessageOptions.DontRequireReceiver);
		}
		if (item != null)
		{
			ItemModBurnable itemModBurnable = item.info.ItemModBurnable;
			item.fuel -= deltaTime * (cookingTemperature / 200f);
			if (!item.HasFlag(Item.Flag.OnFire))
			{
				item.SetFlag(Item.Flag.OnFire, b: true);
				item.MarkDirty();
			}
			if (item.fuel <= 0f)
			{
				ConsumeFuel(item, itemModBurnable);
			}
		}
		OnCooked();
		Interface.CallHook("OnOvenCooked", this, item, slot);
	}

	protected virtual void OnCooked()
	{
	}

	public void ConsumeFuel(Item fuel, ItemModBurnable burnable)
	{
		if (Interface.CallHook("OnFuelConsume", this, fuel, burnable) != null)
		{
			return;
		}
		if (allowByproductCreation && burnable.byproductItem != null && UnityEngine.Random.Range(0f, 1f) > burnable.byproductChance)
		{
			int num = burnable.byproductAmount * GetCharcoalRate();
			bool flag = false;
			foreach (Item item2 in base.inventory.itemList)
			{
				if (item2.info == burnable.byproductItem && item2.amount + num < item2.info.stackable)
				{
					item2.amount += num;
					item2.MarkDirty();
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Item item = ItemManager.Create(burnable.byproductItem, num, 0uL, isServerSide: true, 0uL);
				if (!item.MoveToContainer(base.inventory))
				{
					OvenFull();
					item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
				}
			}
		}
		if (fuel.amount <= GetFuelRate())
		{
			fuel.Remove();
			return;
		}
		int fuelRate = GetFuelRate();
		fuel.fuel = burnable.fuelAmount;
		fuel.UseItem(fuelRate);
		Interface.CallHook("OnFuelConsumed", this, fuel, burnable);
		Facepunch.Rust.Analytics.Azure.AddPendingItems(this, fuel.info.shortname, fuelRate, "smelt");
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	protected virtual void SVSwitch(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (Interface.CallHook("OnOvenToggle", this, msg.player) != null || flag == IsOn())
		{
			return;
		}
		BaseLock @lock = GetLock();
		if (@lock != null && !@lock.GetPlayerLockPermission(msg.player))
		{
			msg.player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
		}
		else
		{
			if (needsBuildingPrivilegeToUse && !msg.player.CanBuild())
			{
				return;
			}
			if (flag)
			{
				StartCooking();
				if (msg.player != null)
				{
					msg.player.ProcessMissionEvent(BaseMission.MissionEventType.STARTOVEN, new BaseMission.MissionEventPayload
					{
						UintIdentifier = prefabID,
						NetworkIdentifier = net.ID
					}, 1f);
				}
			}
			else
			{
				StopCooking();
			}
		}
	}

	public float GetTemperature(int slot)
	{
		object obj = Interface.CallHook("OnOvenTemperature", this, slot);
		if (obj is float)
		{
			return (float)obj;
		}
		if (!HasFlag(Flags.On))
		{
			return 15f;
		}
		return cookingTemperature;
	}

	public void UpdateAttachmentTemperature()
	{
		BaseEntity slot = GetSlot(Slot.FireMod);
		if ((bool)slot)
		{
			slot.SendMessage("ParentTemperatureUpdate", base.inventory.temperature, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected bool HasInputItems()
	{
		if (base.inventory == null)
		{
			return false;
		}
		foreach (Item item in base.inventory.itemList)
		{
			if (item.position >= _inputSlotIndex && item.position < _inputSlotIndex + inputSlots)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateIsCooking()
	{
		if (!IsOn())
		{
			SetFlagLocal(Flags.Reserved10, b: false);
			SendNetworkUpdate_Flags();
		}
		else
		{
			SetFlagLocal(Flags.Reserved10, HasInputItems());
			SendNetworkUpdate_Flags();
		}
	}

	public virtual void StartCooking()
	{
		if (Interface.CallHook("OnOvenStart", this) == null && (FindBurnable() != null || CanRunWithNoFuel))
		{
			base.inventory.temperature = cookingTemperature;
			UpdateAttachmentTemperature();
			cookQueue.Add(this);
			lastCookUpdate = 0f;
			lastCookVisualsUpdate = 0f;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			Interface.CallHook("OnOvenStarted", this);
			if (hasOpenFlame)
			{
				SingletonComponent<NpcFireManager>.Instance.Add(this);
			}
			UpdateIsCooking();
		}
	}

	public virtual void StopCooking()
	{
		UpdateAttachmentTemperature();
		if (base.inventory != null)
		{
			base.inventory.temperature = 15f;
			foreach (Item item in base.inventory.itemList)
			{
				if (item.HasFlag(Item.Flag.OnFire))
				{
					item.SetFlag(Item.Flag.OnFire, b: false);
					item.MarkDirty();
				}
				else if (item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: false);
					item.MarkDirty();
				}
			}
		}
		cookQueue.Remove(this);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
			flagsUpdateScope.Set(Flags.Reserved10, b: false);
		}
		if (hasOpenFlame)
		{
			SingletonComponent<NpcFireManager>.Instance.Remove(this);
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (!base.IsDestroyed && IsOn())
		{
			return disabledBySplash;
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		StopCooking();
		return Mathf.Min(200, amount);
	}

	public Item FindBurnable()
	{
		object obj = Interface.CallHook("OnFindBurnable", this);
		if (obj is Item)
		{
			return (Item)obj;
		}
		using (TimeWarning.New("FindBurnable"))
		{
			if (base.inventory == null)
			{
				return null;
			}
			foreach (Item item in base.inventory.itemList)
			{
				if (IsBurnableItem(item))
				{
					return item;
				}
			}
			return null;
		}
	}

	public void IncreaseCookTime(float amount)
	{
		using (TimeWarning.New("IncreaseCookTime"))
		{
			List<Item> obj = Facepunch.Pool.Get<List<Item>>();
			foreach (Item item in base.inventory.itemList)
			{
				if (item.HasFlag(Item.Flag.Cooking))
				{
					obj.Add(item);
				}
			}
			float delta = amount / (float)obj.Count;
			foreach (Item item2 in obj)
			{
				item2.OnCycle(delta);
			}
			Facepunch.Pool.Free(ref obj, freeElements: false);
		}
	}

	public Vector2i InputSlotRange(int slotIndex)
	{
		if (IndustrialMode == IndustrialSlotMode.LargeFurnace)
		{
			return new Vector2i(0, 6);
		}
		if (IndustrialMode == IndustrialSlotMode.OilRefinery)
		{
			return new Vector2i(0, 1);
		}
		if (IndustrialMode == IndustrialSlotMode.ElectricFurnace)
		{
			return new Vector2i(0, 1);
		}
		return new Vector2i(0, 2);
	}

	public Vector2i OutputSlotRange(int slotIndex)
	{
		if (IndustrialMode == IndustrialSlotMode.LargeFurnace)
		{
			return new Vector2i(7, 16);
		}
		if (IndustrialMode == IndustrialSlotMode.OilRefinery)
		{
			return new Vector2i(2, 4);
		}
		if (IndustrialMode == IndustrialSlotMode.ElectricFurnace)
		{
			return new Vector2i(2, 4);
		}
		return new Vector2i(3, 5);
	}

	public void OnStorageItemTransferBegin()
	{
	}

	public void OnStorageItemTransferEnd()
	{
	}

	private void UpdateAutoState()
	{
		bool flag = HasInputItems();
		if (flag && !IsOn())
		{
			StartCooking();
		}
		else if (!flag && IsOn())
		{
			StopCooking();
			SendNetworkUpdate();
		}
	}

	public float GetSmeltingSpeed()
	{
		if (base.isServer)
		{
			return smeltSpeed;
		}
		throw new Exception("No way it should be able to get here?");
	}

	public bool IsBurnableItem(Item item)
	{
		if ((bool)item.info.ItemModBurnable && (fuelType == null || item.info == fuelType))
		{
			return true;
		}
		return false;
	}

	public bool IsBurnableByproduct(Item item)
	{
		ItemModBurnable itemModBurnable = fuelType?.ItemModBurnable;
		if (itemModBurnable == null)
		{
			return false;
		}
		return item.info == itemModBurnable.byproductItem;
	}

	public bool IsMaterialInput(Item item)
	{
		CookableItemInfo cookableItemInfo = item.info.ItemModCookable;
		if (this is Composter)
		{
			cookableItemInfo = item.info.ItemModCompostable;
		}
		if (cookableItemInfo == null || (float)cookableItemInfo.lowTemp > cookingTemperature || (float)cookableItemInfo.highTemp < cookingTemperature)
		{
			return false;
		}
		return true;
	}

	public bool IsMaterialOutput(Item item)
	{
		if (_materialOutputCache == null)
		{
			BuildMaterialOutputCache();
		}
		if (!_materialOutputCache.TryGetValue(cookingTemperature, out var value))
		{
			Debug.LogError("Can't find smeltable item list for oven");
			return true;
		}
		return value.Contains(item.info);
	}

	public bool IsOutputItem(Item item)
	{
		if (!IsMaterialOutput(item))
		{
			return IsBurnableByproduct(item);
		}
		return true;
	}

	private void BuildMaterialOutputCache()
	{
		_materialOutputCache = new Dictionary<float, HashSet<ItemDefinition>>();
		float[] array = (from x in GameManager.server.preProcessed.prefabList.Values
			select x.GetComponent<BaseOven>() into x
			where x != null
			select x.cookingTemperature).Distinct().ToArray();
		foreach (float temperature in array)
		{
			HashSet<ItemDefinition> validOutputs = new HashSet<ItemDefinition>();
			_materialOutputCache[temperature] = validOutputs;
			foreach (ItemDefinition item in ItemManager.itemList)
			{
				TryAddCookable(item.ItemModCookable);
				TryAddCookable(item.ItemModCompostable);
			}
			void TryAddCookable(CookableItemInfo cookable)
			{
				if (cookable != null && cookable.CanBeCookedByAtTemperature(temperature))
				{
					validOutputs.Add(cookable.becomeOnCooked);
				}
			}
		}
	}

	public override bool HasSlot(Slot slot)
	{
		if (hasFireDeploySlot && slot == Slot.FireMod)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.baseOven = Facepunch.Pool.Get<ProtoBuf.BaseOven>();
			info.msg.baseOven.cookSpeed = GetSmeltingSpeed();
			if (visualFood)
			{
				SaveVisualFood(info);
			}
		}
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		ItemAmount[] array = startupContents;
		foreach (ItemAmount itemAmount in array)
		{
			base.inventory.Take(null, itemAmount.itemid, (int)itemAmount.amount);
		}
		base.Reskin_Restore(ref preserveInfo);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (!CanPickupOven())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	protected virtual bool CanPickupOven()
	{
		return children.Count == 0;
	}

	public virtual void SetAlwaysOn(bool flag)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved9, flag);
		}
		AlwaysOnToggled(flag);
	}

	public void AlwaysOnToggled(bool flag)
	{
		if (flag)
		{
			if (!IsOn())
			{
				StartCooking();
			}
		}
		else
		{
			StopCooking();
		}
	}

	public virtual bool IsAlwaysOn()
	{
		if (HasFlag(Flags.Reserved9))
		{
			return Creative.alwaysOnEnabled;
		}
		return false;
	}
}
