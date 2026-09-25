#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class IndustrialConveyor : IndustrialEntity
{
	public enum ConveyorMode
	{
		Any,
		And,
		Not
	}

	public struct ActiveBufferTransfer
	{
		public ItemFilter ForFilter;

		public int Remaining;
	}

	[JsonModel]
	public struct ItemFilter : IEquatable<ItemFilter>
	{
		[JsonIgnore]
		public ItemDefinition TargetItem;

		public ItemCategory? TargetCategory;

		public int MaxAmountInOutput;

		public int BufferAmount;

		public int MinAmountInInput;

		public bool IsBlueprint;

		public string TargetItemName
		{
			get
			{
				if (!(TargetItem != null))
				{
					return string.Empty;
				}
				return TargetItem.shortname;
			}
			set
			{
				TargetItem = ItemManager.FindItemDefinition(value);
			}
		}

		public void CopyTo(ProtoBuf.IndustrialConveyor.ItemFilter target)
		{
			if (TargetItem != null)
			{
				target.itemDef = TargetItem.itemid;
			}
			target.maxAmountInDestination = MaxAmountInOutput;
			if (TargetCategory.HasValue)
			{
				target.itemCategory = (int)TargetCategory.Value;
			}
			else
			{
				target.itemCategory = -1;
			}
			target.isBlueprint = (IsBlueprint ? 1 : 0);
			target.bufferAmount = BufferAmount;
			target.retainMinimum = MinAmountInInput;
		}

		public ItemFilter(ProtoBuf.IndustrialConveyor.ItemFilter from)
		{
			this = new ItemFilter
			{
				TargetItem = ItemManager.FindItemDefinition(from.itemDef),
				MaxAmountInOutput = from.maxAmountInDestination
			};
			if (from.itemCategory >= 0)
			{
				TargetCategory = (ItemCategory)from.itemCategory;
			}
			else
			{
				TargetCategory = null;
			}
			IsBlueprint = from.isBlueprint == 1;
			BufferAmount = from.bufferAmount;
			MinAmountInInput = from.retainMinimum;
		}

		public bool Equals(ItemFilter other)
		{
			if (object.Equals(TargetItem, other.TargetItem) && TargetCategory == other.TargetCategory && MaxAmountInOutput == other.MaxAmountInOutput && BufferAmount == other.BufferAmount && MinAmountInInput == other.MinAmountInInput)
			{
				return IsBlueprint == other.IsBlueprint;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ItemFilter other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(TargetItem, TargetCategory, MaxAmountInOutput, BufferAmount, MinAmountInInput, IsBlueprint);
		}
	}

	public int MaxStackSizePerMove = 128;

	public GameObjectRef FilterDialog;

	private const float ScreenUpdateRange = 30f;

	public const Flags FilterPassFlag = Flags.Reserved9;

	public const Flags FilterFailFlag = Flags.Reserved10;

	public const int MaxContainerDepth = 32;

	public SoundDefinition transferItemSoundDef;

	public SoundDefinition transferItemStartSoundDef;

	public List<ItemFilter> filterItems = new List<ItemFilter>();

	public ConveyorMode mode;

	public const int MAX_FILTER_SIZE = 30;

	public SpriteRenderer IconTransferSprite;

	private bool refreshInputOutputs;

	private Action scheduleMoveAction;

	private IIndustrialStorage workerOutput;

	private Func<IIndustrialStorage, int, bool> filterFunc;

	private List<ContainerInputOutput> splitOutputs = new List<ContainerInputOutput>();

	private List<ContainerInputOutput> splitInputs = new List<ContainerInputOutput>();

	private bool? lastFilterState;

	private Stopwatch transferStopWatch = new Stopwatch();

	private bool multiFrameTransferInProcess;

	private int multiFrameOutputIndex;

	private int multiFrameInputIndex;

	private bool isFirstTransfer = true;

	private TimeUntil strictModeUntil;

	private static List<Item> inputItemSortedList = new List<Item>(64);

	private static List<Item> outputItemSortedList = new List<Item>(64);

	private static ListHashSet<ContainerInputOutput> notifyContainerMoveEnd = new ListHashSet<ContainerInputOutput>(64);

	private static Dictionary<Item, (ItemFilter filter, int index)> filterPassList = new Dictionary<Item, (ItemFilter, int)>(256);

	private static Dictionary<Item, ContainerInputOutput> item2ContainerLookup = new Dictionary<Item, ContainerInputOutput>(256);

	private bool filtersNeedParsing = true;

	private Dictionary<ItemDefinition, (ItemFilter filter, int index)> quickItemFilterLookup = new Dictionary<ItemDefinition, (ItemFilter, int)>(32);

	private Dictionary<ItemCategory, (ItemFilter filter, int index)> quickItemCategoryLookup = new Dictionary<ItemCategory, (ItemFilter, int)>(32);

	private Dictionary<ItemDefinition, (ItemFilter filter, int index)> quickItemBlueprintLookup = new Dictionary<ItemDefinition, (ItemFilter, int)>(32);

	private List<ActiveBufferTransfer> activeBuffers = new List<ActiveBufferTransfer>();

	private Action unbusyAction;

	private bool wasOnWhenPowerLost;

	public bool strictMode { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("IndustrialConveyor.OnRpcMessage"))
		{
			if (rpc == 617569194 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ChangeFilters");
				}
				using (TimeWarning.New("RPC_ChangeFilters"))
				{
					using (msg.read.UseRepeatedElementLimit(30))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.CallsPerSecond.Test(617569194u, "RPC_ChangeFilters", this, player, 1uL))
							{
								return true;
							}
							if (!RPC_Server.MaxDistance.Test(617569194u, "RPC_ChangeFilters", this, player, 3f))
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
								RPC_ChangeFilters(msg2);
							}
						}
						catch (Exception exception)
						{
							UnityEngine.Debug.LogException(exception);
							player.Kick("RPC Error in RPC_ChangeFilters");
						}
					}
				}
				return true;
			}
			if (rpc == 3731379386u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestUpToDateFilters");
				}
				using (TimeWarning.New("Server_RequestUpToDateFilters"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3731379386u, "Server_RequestUpToDateFilters", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3731379386u, "Server_RequestUpToDateFilters", this, player, 3f))
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
							Server_RequestUpToDateFilters(msg3);
						}
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
						player.Kick("RPC Error in Server_RequestUpToDateFilters");
					}
				}
				return true;
			}
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					UnityEngine.Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SvSwitch");
				}
				using (TimeWarning.New("SvSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4167839872u, "SvSwitch", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4167839872u, "SvSwitch", this, player, 3f))
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
							SvSwitch(msg4);
						}
					}
					catch (Exception exception3)
					{
						UnityEngine.Debug.LogException(exception3);
						player.Kick("RPC Error in SvSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (next & Flags.On) == Flags.On;
		if ((old & Flags.On) == Flags.On != flag && base.isServer)
		{
			if (scheduleMoveAction == null)
			{
				scheduleMoveAction = ScheduleMove;
			}
			float conveyorMoveFrequency = ConVar.Server.conveyorMoveFrequency;
			if (flag && conveyorMoveFrequency > 0f)
			{
				InvokeRandomized(scheduleMoveAction, conveyorMoveFrequency, conveyorMoveFrequency, conveyorMoveFrequency * 0.5f);
			}
			else
			{
				CancelInvoke(scheduleMoveAction);
			}
		}
	}

	private void ScheduleMove()
	{
		IndustrialEntity.Queue.Add(this);
	}

	private Item GetItemToMove(IIndustrialStorage storage, out ItemFilter associatedFilter, int slot, ItemContainer targetContainer = null)
	{
		associatedFilter = default(ItemFilter);
		(ItemFilter, int) tuple = default((ItemFilter, int));
		if (storage == null || storage.Container == null)
		{
			return null;
		}
		if (storage.Container.IsEmpty())
		{
			return null;
		}
		Vector2i vector2i = storage.OutputSlotRange(slot);
		foreach (Item item in storage.Container.itemList)
		{
			if (item.position < vector2i.x || item.position > vector2i.y)
			{
				continue;
			}
			tuple = default((ItemFilter, int));
			if (item != null && (filterItems.Count == 0 || FilterHasItem(item, out tuple)))
			{
				(associatedFilter, _) = tuple;
				if (targetContainer == null || !(associatedFilter.TargetItem != null) || associatedFilter.MaxAmountInOutput <= 0 || targetContainer.GetTotalItemAmount(item, vector2i.x, vector2i.y) < associatedFilter.MaxAmountInOutput)
				{
					return item;
				}
			}
		}
		return null;
	}

	private bool FilterHasItem(Item item, out (ItemFilter filter, int index) filter)
	{
		filter = default((ItemFilter, int));
		using (TimeWarning.New("FilterHasItem"))
		{
			if (quickItemFilterLookup.TryGetValue(item.info, out filter))
			{
				return true;
			}
			if (item.info.isRedirectOf != null && quickItemFilterLookup.TryGetValue(item.info.isRedirectOf, out filter))
			{
				return true;
			}
			if (quickItemCategoryLookup.TryGetValue(item.info.category, out filter))
			{
				return true;
			}
			if (item.IsBlueprint() && quickItemBlueprintLookup.TryGetValue(item.blueprintTargetDef, out filter))
			{
				return true;
			}
			return false;
		}
	}

	private bool FilterMatches(ItemFilter filter, Item item)
	{
		if (item.IsBlueprint() && filter.IsBlueprint && item.blueprintTargetDef == filter.TargetItem)
		{
			return true;
		}
		if (filter.TargetItem == item.info && !filter.IsBlueprint)
		{
			return true;
		}
		if (filter.TargetItem != null && item.info.isRedirectOf == filter.TargetItem)
		{
			return true;
		}
		if (filter.TargetCategory.HasValue && item.info.category == filter.TargetCategory)
		{
			return true;
		}
		return false;
	}

	private bool FilterContainerInput(IIndustrialStorage storage, int slot)
	{
		ItemFilter associatedFilter;
		return GetItemToMove(storage, out associatedFilter, slot, workerOutput?.Container) != null;
	}

	private int GetBufferRemainingForFilter(ItemFilter f)
	{
		using (TimeWarning.New("GetBufferRemainingForFilter"))
		{
			foreach (ActiveBufferTransfer activeBuffer in activeBuffers)
			{
				if (f.Equals(activeBuffer.ForFilter))
				{
					return activeBuffer.Remaining;
				}
			}
			return 0;
		}
	}

	private void DeductBufferRemaining(ItemFilter f, int amount)
	{
		using (TimeWarning.New("DeductBufferRemaining"))
		{
			for (int num = activeBuffers.Count - 1; num >= 0; num--)
			{
				ActiveBufferTransfer value = activeBuffers[num];
				if (f.Equals(value.ForFilter))
				{
					value.Remaining -= amount;
					if (value.Remaining <= 0)
					{
						activeBuffers.RemoveAt(num);
						num--;
					}
					else
					{
						activeBuffers[num] = value;
					}
				}
			}
		}
	}

	private void ClearBufferForFilter(ItemFilter f)
	{
		using (TimeWarning.New("ClearBufferForFilter"))
		{
			for (int num = activeBuffers.Count - 1; num >= 0; num--)
			{
				if (f.Equals(activeBuffers[num].ForFilter))
				{
					activeBuffers.RemoveAt(num);
				}
			}
		}
	}

	private void LoadContainerIntoItemList(List<Item> targetList, ItemContainer container)
	{
		targetList.Clear();
		for (int i = 0; i < container.capacity; i++)
		{
			targetList.Add(null);
		}
		foreach (Item item in container.itemList)
		{
			if (item != null && item.position >= 0 && item.position < targetList.Count)
			{
				targetList[item.position] = item;
			}
		}
	}

	protected override void RunJob()
	{
		base.RunJob();
		RunConveyor();
	}

	public void RunConveyor()
	{
		if (ConVar.Server.conveyorMoveFrequency <= 0f)
		{
			return;
		}
		if (filterFunc == null)
		{
			filterFunc = FilterContainerInput;
		}
		notifyContainerMoveEnd.Clear();
		if (refreshInputOutputs)
		{
			refreshInputOutputs = false;
			splitInputs.Clear();
			splitOutputs.Clear();
			List<IOEntity> obj = Facepunch.Pool.Get<List<IOEntity>>();
			FindContainerSource(splitInputs, 32, input: true, obj);
			obj.Clear();
			FindContainerSource(splitOutputs, 32, input: false, obj, -1, MaxStackSizePerMove);
			Facepunch.Pool.FreeUnmanaged(ref obj);
			multiFrameTransferInProcess = false;
			multiFrameInputIndex = 0;
			multiFrameOutputIndex = 0;
		}
		using (TimeWarning.New("ParseFilter"))
		{
			if (filtersNeedParsing)
			{
				quickItemFilterLookup.Clear();
				quickItemCategoryLookup.Clear();
				quickItemBlueprintLookup.Clear();
				for (int i = 0; i < filterItems.Count; i++)
				{
					ItemFilter item2 = filterItems[i];
					if (!item2.IsBlueprint && item2.TargetItem != null)
					{
						quickItemFilterLookup.TryAdd(item2.TargetItem, (item2, i));
					}
					if (item2.TargetCategory.HasValue)
					{
						quickItemCategoryLookup.TryAdd(item2.TargetCategory.Value, (item2, i));
					}
					if (item2.IsBlueprint)
					{
						quickItemBlueprintLookup.TryAdd(item2.TargetItem, (item2, i));
					}
				}
				filtersNeedParsing = false;
			}
		}
		bool hasItems = CheckIfAnyInputPassesFilters(splitInputs);
		if ((!lastFilterState.HasValue || hasItems != lastFilterState) && !hasItems)
		{
			UpdateFilterPassthroughs();
		}
		if (!hasItems)
		{
			return;
		}
		transferStopWatch.Restart();
		IndustrialConveyorTransfer transfer = Facepunch.Pool.Get<IndustrialConveyorTransfer>();
		try
		{
			bool flag = false;
			bool flag2 = false;
			transfer.ItemTransfers = Facepunch.Pool.Get<List<IndustrialConveyorTransfer.ItemTransfer>>();
			transfer.inputEntities = Facepunch.Pool.Get<List<NetworkableId>>();
			transfer.outputEntities = Facepunch.Pool.Get<List<NetworkableId>>();
			List<int> obj2 = Facepunch.Pool.Get<List<int>>();
			filterPassList.Clear();
			item2ContainerLookup.Clear();
			foreach (ContainerInputOutput splitInput in splitInputs)
			{
				IIndustrialStorage storage = splitInput.Storage;
				ItemContainer container = storage.Container;
				if (container == null || container.IsEmpty())
				{
					continue;
				}
				Vector2i vector2i = storage.OutputSlotRange(splitInput.SlotIndex);
				foreach (Item item7 in container.itemList)
				{
					if (filterPassList.ContainsKey(item7))
					{
						continue;
					}
					bool flag3 = item7.position >= vector2i.x && item7.position <= vector2i.y;
					(ItemFilter, int) filter2 = default((ItemFilter, int));
					if (filterItems.Count > 0 && flag3)
					{
						if (mode == ConveyorMode.Any || mode == ConveyorMode.And)
						{
							flag3 = FilterHasItem(item7, out filter2);
						}
						if (mode == ConveyorMode.Not)
						{
							flag3 = !FilterHasItem(item7, out filter2);
						}
					}
					if (flag3)
					{
						filterPassList.Add(item7, filter2);
						item2ContainerLookup.Add(item7, splitInput);
					}
				}
			}
			if (filterPassList.Count == 0)
			{
				using (TimeWarning.New("EarlyAbortPreFilter"))
				{
					Facepunch.Pool.FreeUnmanaged(ref obj2);
					return;
				}
			}
			int num = 0;
			int count = splitOutputs.Count;
			bool flag4 = false;
			foreach (ContainerInputOutput splitOutput in splitOutputs)
			{
				using (TimeWarning.New("RunOutput"))
				{
					workerOutput = splitOutput.Storage;
					if (workerOutput == null || workerOutput.Container == null)
					{
						continue;
					}
					if (multiFrameTransferInProcess && multiFrameOutputIndex > num)
					{
						num++;
						continue;
					}
					int num2 = 0;
					LoadContainerIntoItemList(outputItemSortedList, splitOutput.Storage.Container);
					foreach (KeyValuePair<Item, (ItemFilter, int)> filterPass in filterPassList)
					{
						using (TimeWarning.New("AttemptMoveFilteredItem"))
						{
							int num3 = 0;
							num2++;
							if (multiFrameTransferInProcess && num2 < multiFrameInputIndex)
							{
								continue;
							}
							if (multiFrameTransferInProcess)
							{
								multiFrameTransferInProcess = false;
							}
							if (filterPass.Key.parent == splitOutput.Storage.Container)
							{
								continue;
							}
							ItemContainer container2 = splitOutput.Storage.Container;
							ItemContainer container3 = item2ContainerLookup[filterPass.Key].Storage.Container;
							ContainerInputOutput val = item2ContainerLookup[filterPass.Key];
							(ItemFilter, int) value = filterPass.Value;
							notifyContainerMoveEnd.TryAdd(val);
							val.Storage.OnStorageItemTransferBegin();
							Vector2i vector2i2 = val.Storage.OutputSlotRange(val.SlotIndex);
							bool flag5 = !container2.IsFull();
							using (TimeWarning.New("OutSlot"))
							{
								Vector2i range = splitOutput.Storage.InputSlotRange(splitOutput.SlotIndex);
								Item key = filterPass.Key;
								if (key.parent != val.Storage.Container)
								{
									continue;
								}
								if (key == null)
								{
									goto IL_0cb6;
								}
								bool flag6 = true;
								if (flag6 && key.info.stackable == 1 && !flag5)
								{
									flag6 = false;
								}
								if (flag6 && key.info.stackable > 1 && !flag5 && !container2.HasAnyWithSpace(key.info))
								{
									flag6 = false;
								}
								if (!flag6)
								{
									continue;
								}
								bool flag7 = mode == ConveyorMode.And || mode == ConveyorMode.Any;
								using (TimeWarning.New("MinMaxCheck_CountInOutput"))
								{
									if (flag7 && value.Item1.TargetItem != null && value.Item1.MaxAmountInOutput > 0 && splitOutput.Storage.Container.GetTotalItemAmount(key, range.x, range.y) >= value.Item1.MaxAmountInOutput)
									{
										flag = true;
										continue;
									}
								}
								int num4 = (int)((float)Mathf.Min(MaxStackSizePerMove, key.info.stackable) / (float)count);
								using (TimeWarning.New("MinMax_Amounts"))
								{
									if (!flag7 || value.Item1.MinAmountInInput <= 0)
									{
										goto IL_0799;
									}
									if (value.Item1.TargetItem != null && FilterMatchItem(value.Item1, key))
									{
										int totalItemAmount = container3.GetTotalItemAmount(key, vector2i2.x, vector2i2.y);
										num4 = Mathf.Min(num4, totalItemAmount - value.Item1.MinAmountInInput);
									}
									else if (value.Item1.TargetCategory.HasValue)
									{
										num4 = Mathf.Min(num4, container3.GetTotalCategoryAmount(value.Item1.TargetCategory.Value, range.x, range.y) - value.Item1.MinAmountInInput);
									}
									if (num4 > 0)
									{
										goto IL_0799;
									}
									goto end_IL_06d0;
									IL_0799:
									if (key.amount == 1 || (num4 <= 0 && key.amount > 0))
									{
										num4 = 1;
									}
									if (flag7 && value.Item1.BufferAmount > 0)
									{
										num4 = Mathf.Min(num4, GetBufferRemainingForFilter(value.Item1));
									}
									if (flag7 && value.Item1.MaxAmountInOutput > 0)
									{
										if (value.Item1.TargetItem != null && FilterMatchItem(value.Item1, key))
										{
											num4 = Mathf.Min(num4, value.Item1.MaxAmountInOutput - container2.GetTotalItemAmount(key, range.x, range.y));
										}
										else if (value.Item1.TargetCategory.HasValue)
										{
											num4 = Mathf.Min(num4, value.Item1.MaxAmountInOutput - container2.GetTotalCategoryAmount(value.Item1.TargetCategory.Value, range.x, range.y));
										}
										if ((float)num4 <= 0f)
										{
											flag = true;
										}
									}
									goto IL_08b2;
									end_IL_06d0:;
								}
								goto end_IL_0589;
								IL_0cb6:
								if (flag4)
								{
									break;
								}
								goto end_IL_0589;
								IL_08b2:
								float num5 = Mathf.Min(key.amount, num4);
								if (num5 > 0f && num5 < 1f)
								{
									num5 = 1f;
								}
								num4 = (int)num5;
								if (num4 <= 0 || !container2.QuickIndustrialPreCheck(key, range, 0, out var foundSlot))
								{
									continue;
								}
								using (TimeWarning.New("OnStorageItemTransferBegin"))
								{
									splitOutput.Storage.OnStorageItemTransferBegin();
								}
								using (TimeWarning.New("NotifyContainerMoveEnd"))
								{
									notifyContainerMoveEnd.TryAdd(splitOutput);
								}
								bool flag8 = false;
								int amount2 = key.amount;
								Item item3 = ((foundSlot == -1) ? null : outputItemSortedList[foundSlot]);
								if (ConVar.Server.industrialAllowQuickMove && foundSlot >= 0 && item3 != null && !item3.IsRemoved() && item3.info.itemid == key.info.itemid && item3 != key && key.CanStack(item3))
								{
									int num6 = Mathf.Min(num4, item3.info.stackable - item3.amount);
									item3.amount += num6;
									key.UseItem(num6);
									amount2 = num6;
									item3.MarkDirty();
									flag8 = true;
									if (key.amount <= 0)
									{
										flag2 = true;
									}
								}
								Item item4 = null;
								if (!flag8 && key.amount > num4)
								{
									item4 = key.SplitItem(num4);
									amount2 = item4.amount;
								}
								using (TimeWarning.New("AttemptMove"))
								{
									if (!flag8)
									{
										for (int j = range.x; j <= range.y; j++)
										{
											Item item5 = ((j == -1) ? null : outputItemSortedList[j]);
											if (item5 != null && (item5.info.itemid != key.info.itemid || item5.condition != key.condition || item5.amount >= item5.info.stackable))
											{
												continue;
											}
											using (TimeWarning.New("MoveToContainer"))
											{
												if ((item4 ?? key).MoveToContainer(container2, j, allowStack: true, ignoreStackLimit: false, null, allowSwap: false))
												{
													flag8 = true;
													flag5 = !container2.IsFull();
													break;
												}
												flag5 = !container2.IsFull();
											}
										}
									}
								}
								DeductBufferRemaining(value.Item1, amount2);
								using (TimeWarning.New("ItemCleanup"))
								{
									if (!flag8 && item4 != null)
									{
										key.amount += item4.amount;
										key.MarkDirty();
										item4.Remove();
										item4 = null;
									}
								}
								using (TimeWarning.New("UpdateTransfers"))
								{
									if (flag8)
									{
										num3++;
										using (TimeWarning.New("AddTransfer"))
										{
											if (item4 != null)
											{
												AddTransfer(item4.info.itemid, amount2, val.Storage.IndustrialEntity, splitOutput.Storage.IndustrialEntity);
											}
											else
											{
												AddTransfer(key.info.itemid, amount2, val.Storage.IndustrialEntity, splitOutput.Storage.IndustrialEntity);
											}
										}
										using (TimeWarning.New("UpdateHashes"))
										{
											LoadContainerIntoItemList(inputItemSortedList, container3);
											LoadContainerIntoItemList(outputItemSortedList, container2);
										}
									}
									else if (!obj2.Contains(num))
									{
										obj2.Add(num);
									}
								}
								if (strictMode && transferStopWatch.Elapsed.TotalMilliseconds >= (double)(ConVar.Server.industrialFrameBudgetMs * 3f) && !isFirstTransfer)
								{
									flag4 = true;
									multiFrameTransferInProcess = true;
									multiFrameOutputIndex = num;
									multiFrameInputIndex = num2;
									break;
								}
								if (num3 >= ConVar.Server.maxItemStacksMovedPerTickIndustrial)
								{
									break;
								}
								goto IL_0cb6;
								end_IL_0589:;
							}
						}
					}
					if (flag4)
					{
						break;
					}
					if (strictMode && !flag4 && transferStopWatch.Elapsed.TotalMilliseconds >= (double)(ConVar.Server.industrialFrameBudgetMs * 3f) && !isFirstTransfer)
					{
						multiFrameTransferInProcess = true;
						multiFrameOutputIndex = num;
						multiFrameInputIndex = 0;
						break;
					}
					num++;
				}
			}
			if (transfer.ItemTransfers.Count == 0 && hasItems && flag)
			{
				hasItems = false;
			}
			if (!lastFilterState.HasValue || hasItems != lastFilterState)
			{
				UpdateFilterPassthroughs();
			}
			double totalMilliseconds = transferStopWatch.Elapsed.TotalMilliseconds;
			if (ConVar.Server.industrialTransferStrictTimeLimits && !strictMode)
			{
				if (totalMilliseconds >= (double)(ConVar.Server.industrialFrameBudgetMs * 3f))
				{
					using (TimeWarning.New("EnterStrictMode"))
					{
						strictMode = true;
						strictModeUntil = 120f;
					}
				}
				else
				{
					using (TimeWarning.New("NoStrictModeNecessary"))
					{
					}
				}
			}
			else if (strictMode && (float)strictModeUntil < 0f)
			{
				strictMode = false;
			}
			Facepunch.Pool.FreeUnmanaged(ref obj2);
			if (flag2)
			{
				ItemManager.DoRemoves();
			}
			if (transfer.ItemTransfers.Count > 0)
			{
				List<Connection> obj3 = Facepunch.Pool.Get<List<Connection>>();
				BaseNetworkable.GetCloseConnections(base.transform.position, 30f, obj3);
				ClientRPC(RpcTarget.Players("ReceiveItemTransferDetails", obj3), transfer);
				Facepunch.Pool.FreeUnmanaged(ref obj3);
			}
			isFirstTransfer = false;
		}
		finally
		{
			if (transfer != null)
			{
				((IDisposable)transfer).Dispose();
			}
		}
		if (multiFrameTransferInProcess && multiFrameOutputIndex == splitOutputs.Count)
		{
			multiFrameTransferInProcess = false;
		}
		foreach (ContainerInputOutput item8 in notifyContainerMoveEnd)
		{
			item8.Storage.OnStorageItemTransferEnd();
		}
		void AddTransfer(int itemId, int amount, BaseEntity fromEntity, BaseEntity toEntity)
		{
			if (transfer != null && transfer.ItemTransfers != null)
			{
				if (fromEntity != null && !transfer.inputEntities.Contains(fromEntity.net.ID))
				{
					transfer.inputEntities.Add(fromEntity.net.ID);
				}
				if (toEntity != null && !transfer.outputEntities.Contains(toEntity.net.ID))
				{
					transfer.outputEntities.Add(toEntity.net.ID);
				}
				for (int k = 0; k < transfer.ItemTransfers.Count; k++)
				{
					IndustrialConveyorTransfer.ItemTransfer value2 = transfer.ItemTransfers[k];
					if (value2.itemId == itemId)
					{
						value2.amount += amount;
						transfer.ItemTransfers[k] = value2;
						return;
					}
				}
				IndustrialConveyorTransfer.ItemTransfer itemTransfer = default(IndustrialConveyorTransfer.ItemTransfer);
				itemTransfer.itemId = itemId;
				itemTransfer.amount = amount;
				IndustrialConveyorTransfer.ItemTransfer item6 = itemTransfer;
				transfer.ItemTransfers.Add(item6);
			}
		}
		static bool FilterMatchItem(ItemFilter filter, Item item)
		{
			if (filter.TargetItem != null && (filter.TargetItem == item.info || (item.IsBlueprint() == filter.IsBlueprint && filter.TargetItem == item.blueprintTargetDef)))
			{
				return true;
			}
			return false;
		}
		void UpdateFilterPassthroughs()
		{
			lastFilterState = hasItems;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved9, hasItems);
				flagsUpdateScope.Set(Flags.Reserved10, !hasItems);
			}
			ensureOutputsUpdated = true;
			MarkDirty();
		}
	}

	protected override void OnIndustrialNetworkChanged()
	{
		base.OnIndustrialNetworkChanged();
		refreshInputOutputs = true;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		refreshInputOutputs = true;
		filtersNeedParsing = true;
	}

	private bool CheckIfAnyInputPassesFilters(List<ContainerInputOutput> inputs)
	{
		if (filterItems.Count == 0)
		{
			foreach (ContainerInputOutput input in inputs)
			{
				if (GetItemToMove(input.Storage, out var _, input.SlotIndex) != null)
				{
					return true;
				}
			}
		}
		else
		{
			int num = 0;
			int num2 = 0;
			if (mode == ConveyorMode.And)
			{
				num2 = activeBuffers.Count;
			}
			for (int i = 0; i < filterItems.Count; i++)
			{
				ItemFilter itemFilter = filterItems[i];
				int num3 = 0;
				int num4 = 0;
				foreach (ContainerInputOutput input2 in inputs)
				{
					if (input2.Storage.Container == null)
					{
						continue;
					}
					Vector2i vector2i = input2.Storage.OutputSlotRange(input2.SlotIndex);
					foreach (Item item in input2.Storage.Container.itemList)
					{
						if (item == null || item.position < vector2i.x || item.position > vector2i.y)
						{
							continue;
						}
						bool flag = FilterMatches(itemFilter, item);
						if (mode == ConveyorMode.Not)
						{
							flag = !flag;
						}
						if (!flag)
						{
							continue;
						}
						if (itemFilter.BufferAmount > 0)
						{
							num3 += item.amount;
							if (GetBufferRemainingForFilter(itemFilter) > 0)
							{
								num++;
								break;
							}
							if (num3 >= itemFilter.BufferAmount + itemFilter.MinAmountInInput)
							{
								if (mode != ConveyorMode.And)
								{
									activeBuffers.Add(new ActiveBufferTransfer
									{
										ForFilter = itemFilter,
										Remaining = itemFilter.BufferAmount
									});
									filterItems[i] = itemFilter;
								}
								num++;
								break;
							}
						}
						if (itemFilter.MinAmountInInput > 0)
						{
							num4 += item.amount;
							if (num4 > itemFilter.MinAmountInInput + itemFilter.BufferAmount)
							{
								num++;
								break;
							}
						}
						if (itemFilter.BufferAmount == 0 && itemFilter.MinAmountInInput == 0)
						{
							num++;
							break;
						}
					}
					if ((mode == ConveyorMode.Any || mode == ConveyorMode.Not) && num > 0)
					{
						return true;
					}
					if (itemFilter.MinAmountInInput > 0)
					{
						num4 = 0;
					}
				}
				if (GetBufferRemainingForFilter(itemFilter) > 0 && num3 == 0)
				{
					ClearBufferForFilter(itemFilter);
					filterItems[i] = itemFilter;
				}
			}
			if (mode == ConveyorMode.And && num > 0 && (num == filterItems.Count || num == num2))
			{
				if (num2 == 0)
				{
					for (int j = 0; j < filterItems.Count; j++)
					{
						activeBuffers.Add(new ActiveBufferTransfer
						{
							ForFilter = filterItems[j],
							Remaining = filterItems[j].BufferAmount
						});
					}
				}
				return true;
			}
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (filterItems.Count == 0)
		{
			return;
		}
		info.msg.industrialConveyor = Facepunch.Pool.Get<ProtoBuf.IndustrialConveyor>();
		info.msg.industrialConveyor.filters = Facepunch.Pool.Get<List<ProtoBuf.IndustrialConveyor.ItemFilter>>();
		info.msg.industrialConveyor.conveyorMode = (int)mode;
		foreach (ItemFilter filterItem in filterItems)
		{
			ProtoBuf.IndustrialConveyor.ItemFilter itemFilter = Facepunch.Pool.Get<ProtoBuf.IndustrialConveyor.ItemFilter>();
			filterItem.CopyTo(itemFilter);
			info.msg.industrialConveyor.filters.Add(itemFilter);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.MaxRepeatedElements(30)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void RPC_ChangeFilters(RPCMessage msg)
	{
		if (msg.player == null || !msg.player.CanBuild())
		{
			return;
		}
		mode = (ConveyorMode)msg.read.Int32();
		filterItems.Clear();
		using ProtoBuf.IndustrialConveyor.ItemFilterList itemFilterList = msg.read.Proto<ProtoBuf.IndustrialConveyor.ItemFilterList>();
		if (itemFilterList.filters == null || Interface.CallHook("OnConveyorFiltersChange", this, msg.player, itemFilterList) != null)
		{
			return;
		}
		int num = Mathf.Min(itemFilterList.filters.Count, 60);
		for (int i = 0; i < num; i++)
		{
			if (filterItems.Count >= 30)
			{
				break;
			}
			ItemFilter item = new ItemFilter(itemFilterList.filters[i]);
			if ((item.TargetItem != null || item.TargetCategory.HasValue) && !filterItems.Contains(item))
			{
				filterItems.Add(item);
			}
		}
		SendNetworkUpdate();
		filtersNeedParsing = true;
	}

	public void SetFilters(List<ItemFilter> newFilter)
	{
		filterItems.Clear();
		filterItems.AddRange(newFilter);
		SendNetworkUpdate();
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SvSwitch(RPCMessage msg)
	{
		if (Interface.CallHook("OnSwitchToggle", this, msg.player) == null)
		{
			SetSwitch(!IsOn());
			Interface.CallHook("OnSwitchToggled", this, msg.player);
		}
	}

	public virtual void SetSwitch(bool wantsOn)
	{
		if (wantsOn != IsOn())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
			{
				flagsUpdateScope.Set(Flags.On, wantsOn);
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved10, b: false);
				flagsUpdateScope.Set(Flags.Reserved9, b: false);
			}
			if (!wantsOn)
			{
				lastFilterState = null;
			}
			ensureOutputsUpdated = true;
			if (unbusyAction == null)
			{
				unbusyAction = Unbusy;
			}
			Invoke(unbusyAction, 0.5f);
			activeBuffers.Clear();
			SendNetworkUpdateImmediate();
			MarkDirty();
		}
	}

	public void Unbusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (inputSlot == 1)
		{
			bool flag = inputAmount >= ConsumptionAmount() && inputAmount > 0;
			if (IsPowered() && IsOn() && !flag)
			{
				wasOnWhenPowerLost = true;
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, flag);
				if (!flag)
				{
					flagsUpdateScope.Set(Flags.Reserved9, b: false);
					flagsUpdateScope.Set(Flags.Reserved10, b: false);
				}
			}
			currentEnergy = inputAmount;
			ensureOutputsUpdated = true;
			if (inputAmount <= 0 && IsOn())
			{
				SetSwitch(wantsOn: false);
			}
			if (inputAmount > 0 && wasOnWhenPowerLost && !IsOn())
			{
				SetSwitch(wantsOn: true);
				wasOnWhenPowerLost = false;
			}
			MarkDirty();
		}
		if (inputSlot == 2 && !IsOn() && inputAmount > 0 && IsPowered())
		{
			SetSwitch(wantsOn: true);
		}
		if (inputSlot == 3 && IsOn() && inputAmount > 0)
		{
			SetSwitch(wantsOn: false);
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int result = Mathf.Min(1, GetCurrentEnergy());
		switch (outputSlot)
		{
		case 2:
			if (!HasFlag(Flags.Reserved10))
			{
				return 0;
			}
			return result;
		case 3:
			if (!HasFlag(Flags.Reserved9))
			{
				return 0;
			}
			return result;
		case 1:
			return GetCurrentEnergy();
		default:
			return 0;
		}
	}

	public override bool ShouldDrainBattery(IOEntity battery)
	{
		return IsOn();
	}

	public override bool WantsPower(int inputIndex)
	{
		return inputIndex == 1;
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void Server_RequestUpToDateFilters(RPCMessage msg)
	{
		if (!IsOn())
		{
			return;
		}
		using ProtoBuf.IndustrialConveyor.ItemFilterList itemFilterList = Facepunch.Pool.Get<ProtoBuf.IndustrialConveyor.ItemFilterList>();
		itemFilterList.filters = Facepunch.Pool.Get<List<ProtoBuf.IndustrialConveyor.ItemFilter>>();
		foreach (ItemFilter filterItem in filterItems)
		{
			ProtoBuf.IndustrialConveyor.ItemFilter itemFilter = Facepunch.Pool.Get<ProtoBuf.IndustrialConveyor.ItemFilter>();
			filterItem.CopyTo(itemFilter);
			itemFilter.bufferTransferRemaining = GetBufferRemainingForFilter(filterItem);
			itemFilterList.filters.Add(itemFilter);
		}
		ClientRPC(RpcTarget.Player("Client_ReceiveBufferInfo", msg.player), itemFilterList);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		filterItems.Clear();
		if (info.msg.industrialConveyor?.filters == null)
		{
			return;
		}
		mode = (ConveyorMode)info.msg.industrialConveyor.conveyorMode;
		foreach (ProtoBuf.IndustrialConveyor.ItemFilter filter in info.msg.industrialConveyor.filters)
		{
			ItemFilter item = new ItemFilter(filter);
			if (item.TargetItem != null || item.TargetCategory.HasValue)
			{
				filterItems.Add(item);
			}
		}
	}
}
