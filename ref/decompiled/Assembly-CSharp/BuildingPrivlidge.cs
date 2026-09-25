#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConVar;
using Facepunch;
using Facepunch.Math;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingPrivlidge : StorageContainer, IPrivilege
{
	public class UpkeepBracket
	{
		public int objectsUpTo;

		public float fraction;

		public float blocksTaxPaid;

		public UpkeepBracket(int numObjs, float frac)
		{
			objectsUpTo = numObjs;
			fraction = frac;
			blocksTaxPaid = 0f;
		}
	}

	public struct BuildingPrivilegePreserveInfo
	{
		public HashSet<ulong> authedPlayers;

		public Dictionary<ulong, uint> recentGroupMembers;
	}

	public const Flags Flag_Raidable = Flags.Reserved7;

	private static HashSet<BuildingPrivlidge> raidStatusSet;

	private const int RefreshBatchSize = 20;

	private const float RefreshBatchInterval = 0.2f;

	public float cachedProtectedMinutes;

	private float cachedGroupUpkeepMultiplier = 1f;

	private int cachedGroupAuthCount;

	public float nextProtectedCalcTime;

	private int _threadSafeCounter = -1;

	public static UpkeepBracket[] upkeepBrackets = new UpkeepBracket[4]
	{
		new UpkeepBracket(ConVar.Decay.bracket_0_blockcount, ConVar.Decay.bracket_0_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_1_blockcount, ConVar.Decay.bracket_1_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_2_blockcount, ConVar.Decay.bracket_2_costfraction),
		new UpkeepBracket(0, ConVar.Decay.bracket_3_costfraction)
	};

	private static UpkeepBracket[] doorUpkeepBrackets = new UpkeepBracket[4]
	{
		new UpkeepBracket(ConVar.Decay.bracket_0_doorcount, ConVar.Decay.bracket_0_doorfraction),
		new UpkeepBracket(ConVar.Decay.bracket_1_doorcount, ConVar.Decay.bracket_1_doorfraction),
		new UpkeepBracket(ConVar.Decay.bracket_2_doorcount, ConVar.Decay.bracket_2_doorfraction),
		new UpkeepBracket(0, ConVar.Decay.bracket_3_doorfraction)
	};

	public List<ItemAmount> upkeepBuffer = new List<ItemAmount>();

	public const float Radius = 16f;

	public GameObject assignDialog;

	[NonSerialized]
	public HashSet<ulong> authorizedPlayers = new HashSet<ulong>();

	public const Flags Flag_MaxAuths = Flags.Reserved5;

	public const Flags Flag_BlockAllFromBuilding = Flags.Reserved6;

	public List<ItemDefinition> allowedConstructionItems = new List<ItemDefinition>();

	public bool IsInvisibleAuth;

	public static Grid<BuildingPrivlidge> InvisibleAuthGrid = new Grid<BuildingPrivlidge>();

	private static ListHashSet<ItemDefinition> allowedConstructionFast = null;

	[NonSerialized]
	public Dictionary<ulong, uint> recentGroupMembers = new Dictionary<ulong, uint>();

	private Action delayedUpdateCB;

	public override bool PreserveChildrenWhenReskinning => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BuildingPrivlidge.OnRpcMessage"))
		{
			if (rpc == 82205621 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - AddAuthorize");
				}
				using (TimeWarning.New("AddAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(82205621u, "AddAuthorize", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							AddAuthorize(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddAuthorize");
					}
				}
				return true;
			}
			if (rpc == 253307592 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClearList");
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(253307592u, "ClearList", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							ClearList(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RemoveSelfAuthorize");
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							RemoveSelfAuthorize(rpc4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 2051750736 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Rotate");
				}
				using (TimeWarning.New("RPC_Rotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2051750736u, "RPC_Rotate", this, player, 3f))
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
							RPC_Rotate(msg2);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_Rotate");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnDeployableCorpseSpawned(BaseEntity corpse)
	{
		base.OnDeployableCorpseSpawned(corpse);
		BuildingPrivlidge componentInChildren = corpse.GetComponentInChildren<BuildingPrivlidge>();
		if (componentInChildren == null)
		{
			Debug.LogError("Not able to transfer auth of TC to corpse: BuildingPrivlidge component not found in child");
			return;
		}
		componentInChildren.SetAuthListFrom(this);
		componentInChildren.AttachToBuilding(buildingID);
	}

	public override bool ShouldDropDeployableCorpse(HitInfo info)
	{
		if (!StorageContainer.dropCorpseOnDeath || !GameModeSoftcore.allow_tc_corpse_no_building)
		{
			BuildingManager.Building building = GetBuilding();
			if (building == null)
			{
				return false;
			}
			if (building.buildingBlocks == null || building.buildingBlocks.Count == 0)
			{
				return false;
			}
		}
		return base.ShouldDropDeployableCorpse(info);
	}

	private void OnDestroyShared_Softcore()
	{
		if (base.isServer)
		{
			raidStatusSet?.Remove(this);
		}
	}

	private void SetDeployedBy(BasePlayer player)
	{
		if (!base.isClient && BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
		{
			timePlaced = GetNetworkTime();
		}
	}

	private void OnServerInit_Softcore()
	{
		if (raidStatusSet == null)
		{
			raidStatusSet = new HashSet<BuildingPrivlidge>();
		}
		raidStatusSet.Add(this);
		if (BaseGameMode.GetActiveGameMode(serverside: true) is GameModeSoftcore)
		{
			Invoke(UpdateRaidableFlag, 0.25f);
		}
	}

	public static void RefreshAllRaidStatus()
	{
		if (raidStatusSet == null)
		{
			return;
		}
		int num = 0;
		foreach (BuildingPrivlidge item in raidStatusSet)
		{
			item.CancelInvoke(item.UpdateRaidableFlag);
			item.Invoke(item.UpdateRaidableFlag, (float)(num / 20) * 0.2f);
			num++;
		}
	}

	private void UpdateRaidableFlag()
	{
		CancelInvoke(UpdateRaidableFlag);
		bool flag = Softcore.raidwindow_fresh_tc_seconds > 0f && timePlaced > 0f && UnityEngine.Time.time - timePlaced <= Softcore.raidwindow_fresh_tc_seconds;
		bool flag2 = Softcore.raidwindow_enabled && (RaidWindow.IsWindowOpenNow() || flag);
		if (HasFlag(Flags.Reserved7) != flag2)
		{
			SetFlagLocal(Flags.Reserved7, flag2);
			SendNetworkUpdate_Flags();
		}
		if (flag)
		{
			Invoke(UpdateRaidableFlag, Softcore.raidwindow_fresh_tc_seconds - (UnityEngine.Time.time - timePlaced) + 0.1f);
		}
	}

	public float CalculateUpkeepPeriodMinutes()
	{
		if (base.isServer)
		{
			return ConVar.Decay.upkeep_period_minutes;
		}
		return 0f;
	}

	public float CalculateUpkeepCostFraction(bool doors)
	{
		if (base.isServer)
		{
			return (doors ? CalculateDoorTaxRate() : CalculateBuildingTaxRate()) * cachedGroupUpkeepMultiplier;
		}
		return 0f;
	}

	public float GetGroupUpkeepMultiplier()
	{
		return cachedGroupUpkeepMultiplier;
	}

	public int GetGroupAuthCount()
	{
		return cachedGroupAuthCount;
	}

	public float GetEntityUpkeepMultiplier(DecayEntity entity, float buildingFraction, float doorFraction)
	{
		if (entity.Upkeep != null && entity.Upkeep.highWall)
		{
			return ConVar.Decay.high_wall_upkeep * cachedGroupUpkeepMultiplier;
		}
		if (!(entity is Door))
		{
			return buildingFraction;
		}
		return doorFraction;
	}

	public void CalculateUpkeepCostAmounts(List<ItemAmount> itemAmounts)
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		float buildingFraction = CalculateUpkeepCostFraction(doors: false);
		float doorFraction = CalculateUpkeepCostFraction(doors: true);
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			float entityUpkeepMultiplier = GetEntityUpkeepMultiplier(decayEntity, buildingFraction, doorFraction);
			decayEntity.CalculateUpkeepCostAmounts(itemAmounts, entityUpkeepMultiplier);
		}
	}

	public float GetProtectedMinutes(bool force = false)
	{
		if (base.isServer)
		{
			if (!force && UnityEngine.Time.realtimeSinceStartup < nextProtectedCalcTime)
			{
				return cachedProtectedMinutes;
			}
			nextProtectedCalcTime = UnityEngine.Time.realtimeSinceStartup + 60f;
			cachedProtectedMinutes = CalculateProtectedMinutes();
			Interface.CallHook("OnCupboardProtectionCalculated", this, cachedProtectedMinutes);
			return cachedProtectedMinutes;
		}
		return 0f;
	}

	private float GetOrUpdateProtectedMinutes(in ThreadSafeTime time)
	{
		if (BaseNetworkable.UseParallelSaves)
		{
			if (time.RealTimeSinceStartup >= nextProtectedCalcTime)
			{
				int threadSafeCounter = _threadSafeCounter;
				if (threadSafeCounter != time.FrameCount && Interlocked.CompareExchange(ref _threadSafeCounter, time.FrameCount, threadSafeCounter) == threadSafeCounter)
				{
					cachedProtectedMinutes = CalculateProtectedMinutes();
					nextProtectedCalcTime = time.RealTimeSinceStartup + 60f;
				}
			}
			return cachedProtectedMinutes;
		}
		return GetProtectedMinutes();
	}

	private float CalculateProtectedMinutes()
	{
		List<ItemAmount> obj = Facepunch.Pool.Get<List<ItemAmount>>();
		CalculateUpkeepCostAmounts(obj);
		float num = CalculateUpkeepPeriodMinutes();
		float num2 = -1f;
		if (base.inventory != null)
		{
			using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
			foreach (ItemAmount item in obj)
			{
				pooledList.Clear();
				base.inventory.FindItemsByItemID(pooledList, item.itemid);
				int num3 = pooledList.Sum((Item x) => x.amount);
				if (num3 > 0 && item.amount > 0f)
				{
					float num4 = (float)num3 / item.amount * num;
					if (num2 == -1f || num4 < num2)
					{
						num2 = num4;
					}
				}
				else
				{
					num2 = 0f;
				}
			}
			if (num2 == -1f)
			{
				num2 = 0f;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return num2;
	}

	public override void OnDied(HitInfo info)
	{
		if (info != null && info.InitiatorPlayer != null && !info.InitiatorPlayer.IsNpc && info.InitiatorPlayer.serverClan != null)
		{
			IReadOnlyList<ClanMember> members = info.InitiatorPlayer.serverClan.Members;
			bool flag = false;
			foreach (ClanMember item in members)
			{
				if (item.SteamId == base.OwnerID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				HandleKilledByClanMember(info.InitiatorPlayer);
			}
		}
		UnlinkDoorControllers();
		base.OnDied(info);
	}

	public override void Die(HitInfo info = null)
	{
		if (!IsDead())
		{
			if (ConVar.Decay.upkeep_grief_protection > 0f)
			{
				PurchaseAntiGriefTime(ConVar.Decay.upkeep_grief_protection * 60f);
			}
			base.Die(info);
		}
	}

	private async void HandleKilledByClanMember(BasePlayer player)
	{
		try
		{
			ClanValueResult<IClan> clanValueResult = await ClanManager.ServerInstance.Backend.GetByMember(base.OwnerID);
			IClan clan = (clanValueResult.IsSuccess ? clanValueResult.Value : null);
			if (clan != null)
			{
				player.AddClanScore(ClanScoreEventType.DestroyedToolCupboard, 1, null, clan);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public override void DecayTick(bool force = false)
	{
		RecalculateGroupUpkeep();
		BuildingBlock nearbyBuildingBlock = GetNearbyBuildingBlock();
		if (nearbyBuildingBlock != null)
		{
			BuildingManager.Building building = nearbyBuildingBlock.GetBuilding();
			if (building != null && building.ID != buildingID)
			{
				AttachToBuilding(building.ID);
			}
		}
		else
		{
			bool flag = true;
			BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
			if (activeGameMode != null && activeGameMode is GameModeSoftcore && GetParentEntity() is ContainerCorpse)
			{
				flag = false;
			}
			if (flag)
			{
				Kill(DestroyMode.Gib);
			}
		}
		if (EnsurePrimary())
		{
			base.DecayTick(force);
		}
	}

	public bool EnsurePrimary()
	{
		BuildingManager.Building building = GetBuilding();
		if (building != null)
		{
			BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
			if (dominatingBuildingPrivilege != null && dominatingBuildingPrivilege != this)
			{
				Kill(DestroyMode.Gib);
				return false;
			}
		}
		return true;
	}

	public void MarkProtectedMinutesDirty(float delay = 0f)
	{
		nextProtectedCalcTime = UnityEngine.Time.realtimeSinceStartup + delay;
	}

	public static float CalculateGroupUpkeepMultiplier(int authCount)
	{
		if (!ConVar.Decay.upkeep_group_scaling)
		{
			return 1f;
		}
		if (authCount <= 0)
		{
			return 1f;
		}
		int num = authCount;
		float num2 = 0f;
		for (int i = 0; i < 3; i++)
		{
			int b;
			float num3;
			switch (i)
			{
			case 0:
				b = ConVar.Decay.upkeep_group_tier_0_playercount;
				num3 = ConVar.Decay.upkeep_group_tier_0_increase;
				break;
			case 1:
				b = ConVar.Decay.upkeep_group_tier_1_playercount;
				num3 = ConVar.Decay.upkeep_group_tier_1_increase;
				break;
			default:
				b = int.MaxValue;
				num3 = ConVar.Decay.upkeep_group_tier_2_increase;
				break;
			}
			if (num <= 0)
			{
				break;
			}
			int num4 = Mathf.Min(num, Mathf.Max(0, b));
			num -= num4;
			num2 += (float)num4 * num3;
		}
		return Mathf.Min(1f + num2, ConVar.Decay.upkeep_group_max_multiplier);
	}

	public void RecalculateGroupUpkeep()
	{
		uint current = (uint)Epoch.Current;
		RefreshDoorGroupMembers(current);
		PruneGroupMembers(current);
		int num = authorizedPlayers.Count + recentGroupMembers.Count;
		float num2 = CalculateGroupUpkeepMultiplier(num);
		if (num != cachedGroupAuthCount || num2 != cachedGroupUpkeepMultiplier)
		{
			cachedGroupAuthCount = num;
			cachedGroupUpkeepMultiplier = num2;
			AddDelayedUpdate();
		}
	}

	private void RefreshDoorGroupMembers(uint now)
	{
		if (!ConVar.Decay.upkeep_group_count_locks)
		{
			return;
		}
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return;
		}
		foreach (Door door in building.doors)
		{
			if (door.GetSlot(Slot.Lock) is CodeLock codeLock && codeLock.whitelistPlayers.Count + codeLock.guestPlayers.Count > ConVar.Decay.upkeep_lock_min_users)
			{
				RefreshDoorGroupMembers(codeLock.whitelistPlayers, now);
				RefreshDoorGroupMembers(codeLock.guestPlayers, now);
			}
		}
	}

	private void RefreshDoorGroupMembers(List<ulong> users, uint now)
	{
		foreach (ulong user in users)
		{
			if (!authorizedPlayers.Contains(user))
			{
				recentGroupMembers[user] = now;
			}
		}
	}

	private void PruneGroupMembers(uint now)
	{
		if (recentGroupMembers.Count == 0)
		{
			return;
		}
		long num = (long)Mathf.Max(0f, ConVar.Decay.upkeep_group_window_hours * 3600f);
		using PooledList<ulong> pooledList = Facepunch.Pool.Get<PooledList<ulong>>();
		foreach (KeyValuePair<ulong, uint> recentGroupMember in recentGroupMembers)
		{
			if (authorizedPlayers.Contains(recentGroupMember.Key))
			{
				pooledList.Add(recentGroupMember.Key);
				continue;
			}
			long num2 = (long)now - (long)recentGroupMember.Value;
			if (num2 >= num || num2 < 0)
			{
				pooledList.Add(recentGroupMember.Key);
			}
		}
		foreach (ulong item in pooledList)
		{
			recentGroupMembers.Remove(item);
		}
		TrimGroupMembersToMax();
	}

	private void TrimGroupMembersToMax()
	{
		int num = Mathf.Max(0, ConVar.Decay.upkeep_group_history_max);
		while (recentGroupMembers.Count > num)
		{
			ulong key = 0uL;
			uint num2 = 0u;
			bool flag = false;
			foreach (KeyValuePair<ulong, uint> recentGroupMember in recentGroupMembers)
			{
				if (!flag || recentGroupMember.Value < num2)
				{
					num2 = recentGroupMember.Value;
					key = recentGroupMember.Key;
					flag = true;
				}
			}
			if (flag)
			{
				recentGroupMembers.Remove(key);
				continue;
			}
			break;
		}
	}

	private static float CalculateTaxRate(int entityCount, bool blocks)
	{
		if (entityCount == 0)
		{
			if (!blocks)
			{
				return ConVar.Decay.bracket_0_doorfraction;
			}
			return ConVar.Decay.bracket_0_costfraction;
		}
		int num = entityCount;
		float num2 = 0f;
		for (int i = 0; i < 4; i++)
		{
			float num3 = (blocks ? ConVar.Decay.bracket_0_costfraction : ConVar.Decay.bracket_0_doorfraction);
			int b = (blocks ? ConVar.Decay.bracket_0_blockcount : ConVar.Decay.bracket_0_doorcount);
			switch (i)
			{
			case 1:
				num3 = (blocks ? ConVar.Decay.bracket_1_costfraction : ConVar.Decay.bracket_1_doorfraction);
				b = (blocks ? ConVar.Decay.bracket_1_blockcount : ConVar.Decay.bracket_1_doorcount);
				break;
			case 2:
				num3 = (blocks ? ConVar.Decay.bracket_2_costfraction : ConVar.Decay.bracket_1_doorfraction);
				b = (blocks ? ConVar.Decay.bracket_2_blockcount : ConVar.Decay.bracket_1_doorcount);
				break;
			case 3:
				num3 = (blocks ? ConVar.Decay.bracket_3_costfraction : ConVar.Decay.bracket_1_doorfraction);
				b = int.MaxValue;
				break;
			}
			if (num > 0)
			{
				int num4 = Mathf.Min(num, b);
				num -= num4;
				num2 += (float)num4 * num3;
			}
		}
		return num2 /= (float)entityCount;
	}

	private float CalculateDoorTaxRate()
	{
		if (!ConVar.Decay.use_door_upkeep_brackets)
		{
			return CalculateBuildingTaxRate();
		}
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return ConVar.Decay.bracket_0_doorfraction;
		}
		if (!building.HasDecayEntities())
		{
			return ConVar.Decay.bracket_0_doorfraction;
		}
		return CalculateTaxRate(building.doors.Count, blocks: false);
	}

	public float CalculateBuildingTaxRate()
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return ConVar.Decay.bracket_0_costfraction;
		}
		if (!building.HasBuildingBlocks())
		{
			return ConVar.Decay.bracket_0_costfraction;
		}
		return CalculateTaxRate(building.buildingBlocks.Count, blocks: true);
	}

	public void ApplyUpkeepPayment()
	{
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		for (int i = 0; i < upkeepBuffer.Count; i++)
		{
			ItemAmount itemAmount = upkeepBuffer[i];
			int num = (int)itemAmount.amount;
			if (num < 1)
			{
				continue;
			}
			base.inventory.Take(obj, itemAmount.itemid, num);
			Facepunch.Rust.Analytics.Azure.AddPendingItems(this, itemAmount.itemDef.shortname, num, "upkeep", consumed: true, perEntity: true);
			foreach (Item item in obj)
			{
				if (IsDebugging())
				{
					Debug.Log(ToString() + ": Using " + item.amount + " of " + item.info.shortname);
				}
				item.UseItem(item.amount);
			}
			obj.Clear();
			itemAmount.amount -= num;
			upkeepBuffer[i] = itemAmount;
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
	}

	public void QueueUpkeepPayment(List<ItemAmount> itemAmounts)
	{
		for (int i = 0; i < itemAmounts.Count; i++)
		{
			ItemAmount itemAmount = itemAmounts[i];
			bool flag = false;
			foreach (ItemAmount item in upkeepBuffer)
			{
				if (item.itemDef == itemAmount.itemDef)
				{
					item.amount += itemAmount.amount;
					if (IsDebugging())
					{
						Debug.Log(ToString() + ": Adding " + itemAmount.amount + " of " + itemAmount.itemDef.shortname + " to " + item.amount);
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (IsDebugging())
				{
					Debug.Log(ToString() + ": Adding " + itemAmount.amount + " of " + itemAmount.itemDef.shortname);
				}
				upkeepBuffer.Add(new ItemAmount(itemAmount.itemDef, itemAmount.amount));
			}
		}
	}

	public bool CanAffordUpkeepPayment(List<ItemAmount> itemAmounts)
	{
		for (int i = 0; i < itemAmounts.Count; i++)
		{
			ItemAmount itemAmount = itemAmounts[i];
			if ((float)base.inventory.GetAmount(itemAmount.itemid, onlyUsableAmounts: true) < itemAmount.amount)
			{
				if (IsDebugging())
				{
					Debug.Log(ToString() + ": Can't afford " + itemAmount.amount + " of " + itemAmount.itemDef.shortname);
				}
				return false;
			}
		}
		return true;
	}

	public float PurchaseUpkeepTime(DecayEntity entity, float deltaTime)
	{
		float buildingFraction = CalculateUpkeepCostFraction(doors: false);
		float doorFraction = CalculateUpkeepCostFraction(doors: true);
		float num = CalculateUpkeepPeriodMinutes() * 60f;
		float multiplier = GetEntityUpkeepMultiplier(entity, buildingFraction, doorFraction) * deltaTime / num;
		List<ItemAmount> obj = Facepunch.Pool.Get<List<ItemAmount>>();
		entity.CalculateUpkeepCostAmounts(obj, multiplier);
		bool num2 = CanAffordUpkeepPayment(obj);
		QueueUpkeepPayment(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		ApplyUpkeepPayment();
		if (!num2)
		{
			return 0f;
		}
		return deltaTime;
	}

	public void PurchaseUpkeepTime(float deltaTime)
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		float num = Mathf.Min(GetProtectedMinutes(force: true) * 60f, deltaTime);
		if (!(num > 0f))
		{
			return;
		}
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			float protectedSeconds = decayEntity.GetProtectedSeconds();
			if (num > protectedSeconds)
			{
				float time = PurchaseUpkeepTime(decayEntity, num - protectedSeconds);
				decayEntity.AddUpkeepTime(time);
				if (IsDebugging())
				{
					Debug.Log(ToString() + " purchased upkeep time for " + decayEntity.ToString() + ": " + protectedSeconds + " + " + time + " = " + decayEntity.GetProtectedSeconds());
				}
			}
		}
	}

	public void PurchaseAntiGriefTime(float deltaTime)
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			float protectedSeconds = decayEntity.GetProtectedSeconds();
			float num = Mathf.Max(0f, deltaTime - protectedSeconds);
			if (num > 0f)
			{
				float time = PurchaseUpkeepTime(decayEntity, num);
				decayEntity.AddUpkeepTime(time);
				if (IsDebugging())
				{
					Debug.Log(ToString() + " purchased upkeep time for " + decayEntity.ToString() + ": " + protectedSeconds + " + " + time + " = " + decayEntity.GetProtectedSeconds());
				}
			}
		}
	}

	public static string FormatUpkeepMinutes(float minutes)
	{
		int num = Mathf.FloorToInt(minutes / 60f);
		int num2 = Mathf.FloorToInt(minutes - (float)num * 60f);
		int num3 = Mathf.FloorToInt(minutes * 60f % 60f);
		if (num >= 72)
		{
			string text = Translate.Get("days", "days");
			int num4 = num / 24;
			if (num4 >= 30)
			{
				return "> 30 " + text;
			}
			return $"{num4:N0} {text}";
		}
		if (num >= 12)
		{
			return $"{num:N0} hrs";
		}
		if (num >= 1)
		{
			return $"{num:N0}h{num2:N0}m";
		}
		if (minutes >= 1f)
		{
			return $"{num2:N0}m{num3:N0}s";
		}
		return $"{minutes * 60f:N0}s";
	}

	public override void ResetState()
	{
		base.ResetState();
		authorizedPlayers.Clear();
		cachedGroupUpkeepMultiplier = 1f;
		cachedGroupAuthCount = 0;
		if (base.isServer)
		{
			recentGroupMembers.Clear();
		}
	}

	public bool CanBuild(BasePlayer player)
	{
		if (HasFlag(Flags.Reserved6))
		{
			return false;
		}
		return IsAuthed(player);
	}

	public bool IsAuthed(BasePlayer player)
	{
		return IsAuthed(player.userID);
	}

	public bool IsAuthed(ulong userId)
	{
		return authorizedPlayers.Contains(userId);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (IsInvisibleAuth && InvisibleAuthGrid.Contains(this))
		{
			InvisibleAuthGrid.Remove(this);
		}
		OnDestroyShared_Softcore();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (IsInvisibleAuth && !InvisibleAuthGrid.Contains(this))
		{
			Vector3 position = base.transform.position;
			InvisibleAuthGrid.Add(this, position.x, position.z);
		}
		RecalculateGroupUpkeep();
		OnServerInit_Softcore();
	}

	public void SetAuthListFrom(BuildingPrivlidge source)
	{
		authorizedPlayers = new HashSet<ulong>();
		foreach (ulong authorizedPlayer in source.authorizedPlayers)
		{
			authorizedPlayers.Add(authorizedPlayer);
		}
		recentGroupMembers.Clear();
		foreach (KeyValuePair<ulong, uint> recentGroupMember in source.recentGroupMembers)
		{
			recentGroupMembers.Add(recentGroupMember.Key, recentGroupMember.Value);
		}
		RecalculateGroupUpkeep();
		UpdatePrivilegeReceivers();
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		using (TimeWarning.New("BuildPrivItemFilter"))
		{
			if (allowedConstructionFast == null)
			{
				allowedConstructionFast = new ListHashSet<ItemDefinition>();
				allowedConstructionFast.AddRange(allowedConstructionItems);
			}
			bool flag = allowedConstructionFast.Contains(item.info);
			if (!flag && targetSlot == -1)
			{
				int num = 0;
				foreach (Item item2 in base.inventory.itemList)
				{
					if (!allowedConstructionItems.Contains(item2.info) && (item2.info != item.info || item2.amount == item2.MaxStackable()))
					{
						num++;
					}
				}
				if (num >= 24)
				{
					return false;
				}
			}
			if (targetSlot >= 24 && targetSlot <= 28)
			{
				return flag;
			}
			return base.ItemFilter(player, item, targetSlot);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingPrivilege = Facepunch.Pool.Get<BuildingPrivilege>();
		if (!info.forDisk)
		{
			float num = CalculateUpkeepPeriodMinutes();
			float orUpdateProtectedMinutes = GetOrUpdateProtectedMinutes(in info.cachedTime);
			if ((double)ConVar.Decay.scale > 0.01)
			{
				info.msg.buildingPrivilege.upkeepPeriodMinutes = num / ConVar.Decay.scale;
				info.msg.buildingPrivilege.protectedMinutes = orUpdateProtectedMinutes / ConVar.Decay.scale;
			}
			else
			{
				info.msg.buildingPrivilege.upkeepPeriodMinutes = num;
				info.msg.buildingPrivilege.protectedMinutes = orUpdateProtectedMinutes;
			}
			info.msg.buildingPrivilege.costFraction = CalculateUpkeepCostFraction(doors: false);
			info.msg.buildingPrivilege.doorCostFraction = CalculateUpkeepCostFraction(doors: true);
			info.msg.buildingPrivilege.clientAuthed = IsAuthed(info.forConnection.userid);
			info.msg.buildingPrivilege.clientAnyAuthed = AnyAuthed();
			info.msg.buildingPrivilege.groupUpkeepMultiplier = GetGroupUpkeepMultiplier();
			info.msg.buildingPrivilege.groupAuthCount = GetGroupAuthCount();
		}
		else if (recentGroupMembers.Count > 0)
		{
			info.msg.buildingPrivilege.recentGroupMembers = Facepunch.Pool.Get<List<BuildingPrivilegeGroupMember>>();
			foreach (KeyValuePair<ulong, uint> recentGroupMember in recentGroupMembers)
			{
				BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = Facepunch.Pool.Get<BuildingPrivilegeGroupMember>();
				buildingPrivilegeGroupMember.userid = recentGroupMember.Key;
				buildingPrivilegeGroupMember.lastCountedTime = recentGroupMember.Value;
				info.msg.buildingPrivilege.recentGroupMembers.Add(buildingPrivilegeGroupMember);
			}
		}
		if (!info.forDisk && !info.msg.buildingPrivilege.clientAuthed)
		{
			return;
		}
		info.msg.buildingPrivilege.users = Facepunch.Pool.Get<List<PlayerNameID>>();
		foreach (ulong authorizedPlayer in authorizedPlayers)
		{
			PlayerNameID playerNameID = Facepunch.Pool.Get<PlayerNameID>();
			playerNameID.userid = authorizedPlayer;
			info.msg.buildingPrivilege.users.Add(playerNameID);
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		authorizedPlayers.Clear();
		if (info.msg.buildingPrivilege == null)
		{
			return;
		}
		if (info.msg.buildingPrivilege.users != null)
		{
			foreach (PlayerNameID user in info.msg.buildingPrivilege.users)
			{
				authorizedPlayers.Add(user.userid);
			}
		}
		if (!info.fromDisk)
		{
			cachedProtectedMinutes = info.msg.buildingPrivilege.protectedMinutes;
			cachedGroupUpkeepMultiplier = Mathf.Max(1f, info.msg.buildingPrivilege.groupUpkeepMultiplier);
			cachedGroupAuthCount = info.msg.buildingPrivilege.groupAuthCount;
		}
		else
		{
			if (!base.isServer)
			{
				return;
			}
			recentGroupMembers.Clear();
			if (info.msg.buildingPrivilege.recentGroupMembers != null)
			{
				foreach (BuildingPrivilegeGroupMember recentGroupMember in info.msg.buildingPrivilege.recentGroupMembers)
				{
					recentGroupMembers[recentGroupMember.userid] = recentGroupMember.lastCountedTime;
				}
			}
			RecalculateGroupUpkeep();
		}
	}

	public void BuildingDirty()
	{
		if (base.isServer)
		{
			AddDelayedUpdate();
		}
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void UpdateMaxAuthCapacity()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode && activeGameMode.limitTeamAuths)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize());
			}
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		AddDelayedUpdate();
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		AddDelayedUpdate();
	}

	public void AddDelayedUpdate()
	{
		if (delayedUpdateCB == null)
		{
			delayedUpdateCB = DelayedUpdate;
		}
		if (IsInvoking(delayedUpdateCB))
		{
			CancelInvoke(delayedUpdateCB);
		}
		Invoke(delayedUpdateCB, 1f);
	}

	private void DelayedUpdate()
	{
		MarkProtectedMinutesDirty();
		SendNetworkUpdate();
	}

	public bool CanAdministrate(BasePlayer player)
	{
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock == null)
		{
			return true;
		}
		return baseLock.OnTryToOpen(player);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void AddAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player))
		{
			ulong num = rpc.read.UInt64();
			if (Interface.CallHook("IOnCupboardAuthorize", num, rpc.player, this) == null)
			{
				AddPlayer(rpc.player, num);
				SendNetworkUpdate();
			}
		}
	}

	public void AddPlayer(BasePlayer granter, ulong targetPlayerId)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.Add(targetPlayerId);
			recentGroupMembers.Remove(targetPlayerId);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, granter, authorizedPlayers, "added", targetPlayerId);
			RecalculateGroupUpkeep();
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
		}
	}

	public void RemovePlayer(BasePlayer fromPly, ulong targetPlayerId)
	{
		if (authorizedPlayers.Remove(targetPlayerId))
		{
			recentGroupMembers[targetPlayerId] = (uint)Epoch.Current;
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, fromPly, authorizedPlayers, "removed", targetPlayerId);
			RecalculateGroupUpkeep();
			UpdateMaxAuthCapacity();
			UpdatePrivilegeReceivers();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RemoveSelfAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player) && Interface.CallHook("OnCupboardDeauthorize", this, rpc.player) == null)
		{
			RemovePlayer(rpc.player, rpc.player.userID);
			SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void ClearList(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player) && Interface.CallHook("OnCupboardClearList", this, rpc.player) == null)
		{
			ClearAuthList();
			SendNetworkUpdate();
		}
	}

	public void ClearAuthList()
	{
		uint current = (uint)Epoch.Current;
		foreach (ulong authorizedPlayer in authorizedPlayers)
		{
			recentGroupMembers[authorizedPlayer] = current;
		}
		authorizedPlayers.Clear();
		RecalculateGroupUpkeep();
		UpdateMaxAuthCapacity();
		UpdatePrivilegeReceivers();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_Rotate(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.CanBuild() && (bool)player.GetHeldEntity() && player.GetHeldEntity().GetComponent<Hammer>() != null && (GetSlot(Slot.Lock) == null || !GetSlot(Slot.Lock).IsLocked()) && !HasAttachedStorageAdaptor() && !HasAttachedStorageMonitor())
		{
			base.transform.rotation = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			SendNetworkUpdate();
			Deployable component = GetComponent<Deployable>();
			if (component != null && component.placeEffect.isValid)
			{
				Effect.server.Run(component.placeEffect.resourcePath, base.transform.position, Vector3.up);
			}
		}
		BaseEntity slot = GetSlot(Slot.Lock);
		if (slot != null)
		{
			slot.SendNetworkUpdate();
		}
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		if (item != null && item.info != null && allowedConstructionItems.Contains(item.info))
		{
			if (player != null && player.IsInTutorial)
			{
				return 0;
			}
			for (int i = 24; i <= 27; i++)
			{
				if (base.inventory.GetSlot(i) == null)
				{
					return i;
				}
			}
		}
		return base.GetIdealSlot(player, container, item);
	}

	private void UnlinkDoorControllers()
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return;
		}
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			if (!(decayEntity is Door door))
			{
				continue;
			}
			foreach (BaseEntity child in door.children)
			{
				if (child is CustomDoorManipulator customDoorManipulator)
				{
					customDoorManipulator.SetTargetDoor(null);
				}
			}
		}
	}

	private void UpdatePrivilegeReceivers()
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return;
		}
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			if (decayEntity is IPrivilegeUpdateReceiver privilegeUpdateReceiver)
			{
				privilegeUpdateReceiver.OnPrivilegeUpdated(this, authorizedPlayers);
			}
		}
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Preserve(ref preserveInfo);
		ref BuildingPrivilegePreserveInfo buildingPrivilegePreserve = ref preserveInfo.buildingPrivilegePreserve;
		buildingPrivilegePreserve.authedPlayers = Facepunch.Pool.Get<HashSet<ulong>>();
		foreach (ulong authorizedPlayer in authorizedPlayers)
		{
			buildingPrivilegePreserve.authedPlayers.Add(authorizedPlayer);
		}
		buildingPrivilegePreserve.recentGroupMembers = Facepunch.Pool.Get<Dictionary<ulong, uint>>();
		foreach (KeyValuePair<ulong, uint> recentGroupMember in recentGroupMembers)
		{
			buildingPrivilegePreserve.recentGroupMembers.Add(recentGroupMember.Key, recentGroupMember.Value);
		}
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Restore(ref preserveInfo);
		ref BuildingPrivilegePreserveInfo buildingPrivilegePreserve = ref preserveInfo.buildingPrivilegePreserve;
		foreach (ulong authedPlayer in buildingPrivilegePreserve.authedPlayers)
		{
			authorizedPlayers.Add(authedPlayer);
		}
		Facepunch.Pool.FreeUnmanaged(ref buildingPrivilegePreserve.authedPlayers);
		foreach (KeyValuePair<ulong, uint> recentGroupMember in buildingPrivilegePreserve.recentGroupMembers)
		{
			recentGroupMembers[recentGroupMember.Key] = recentGroupMember.Value;
		}
		Facepunch.Pool.FreeUnmanaged(ref buildingPrivilegePreserve.recentGroupMembers);
		RecalculateGroupUpkeep();
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
