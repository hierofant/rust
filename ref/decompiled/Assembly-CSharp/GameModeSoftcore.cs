using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Rust;
using UnityEngine;

public class GameModeSoftcore : GameModeVanilla
{
	public GameObjectRef reclaimManagerPrefab;

	public const Flags Flag_RaidWindowOpen = Flags.Reserved4;

	public SoundDefinition raidWindowOpenSound;

	public SoundDefinition raidWindowCloseSound;

	[ServerVar]
	public static bool allow_tc_corpse_no_building = true;

	[ServerVar(Help = "(Generated) Fraction of belt slot items that are preserved in a softcore death reclaim backpack; default 0.5 (50%)")]
	public static float reclaim_fraction_belt = 0.5f;

	[ServerVar(Help = "(Generated) Fraction of clothing/armour items that are preserved in a softcore death reclaim backpack; default 1.0 (100%)")]
	public static float reclaim_fraction_wear = 1f;

	[ServerVar(Help = "(Generated) Fraction of main inventory items preserved in a softcore death reclaim backpack; default 0.5 (50%)")]
	public static float reclaim_fraction_main = 0.5f;

	[ServerVar(Help = "(Generated) When enabled, items are kept in a reclaim backpack even if the player died by suicide (F1 kill)")]
	public static bool reclaim_suicide = false;

	[ServerVar(Help = "(Generated) When enabled, items are kept in a reclaim backpack even if the player died while inside their own authorised base")]
	public static bool reclaim_building_auth = false;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "(Generated) Global multiplier for resources gathered in Softcore mode from ores, trees, logs, wild plants and animals; 1.0 = default, 2.0 = double yield")]
	public static float gather_rate = 2f;

	private static string[] StartingItems = new string[5] { "rock", "torch", "cakefiveyear", "partyhat", "snowball" };

	private static ItemDefinition[] StartingItemDefs = null;

	private ulong __sync_RaidWindowNextChangeAt;

	[Sync(Pack = false, Autosave = true)]
	public ulong RaidWindowNextChangeAt
	{
		[CompilerGenerated]
		get
		{
			return __sync_RaidWindowNextChangeAt;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_RaidWindowNextChangeAt, value))
			{
				__sync_RaidWindowNextChangeAt = value;
				byte nameID = __GetWeaverID("RaidWindowNextChangeAt");
				SV_SyncVarSend(nameID);
			}
		}
	}

	protected override void OnCreated()
	{
		base.OnCreated();
		SingletonComponent<ServerMgr>.Instance.CreateImportantEntity<ReclaimManager>(reclaimManagerPrefab.resourcePath);
		Invoke(UpdateRaidWindow, 1f);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public void UpdateRaidWindow()
	{
		CancelInvoke(UpdateRaidWindow);
		bool b = Softcore.raidwindow_enabled && RaidWindow.IsWindowOpenNow();
		RaidWindowNextChangeAt = (ulong)(Softcore.raidwindow_enabled ? RaidWindow.NextChangeUnixUtc() : 0);
		SetFlagLocal(Flags.Reserved4, b);
		SendNetworkUpdate();
		BuildingPrivlidge.RefreshAllRaidStatus();
		Invoke(UpdateRaidWindow, Mathf.Max(1f, RaidWindow.SecondsUntilNextChange()));
	}

	public void ReturnItemsTo(List<Item> source, ItemContainer itemContainer)
	{
		foreach (Item item in source)
		{
			item.MoveToContainer(itemContainer);
		}
	}

	public override void OnPlayerDeath(BasePlayer instigator, BasePlayer victim, HitInfo deathInfo = null)
	{
		if (victim != null && (victim.IsInTutorial || (victim.net != null && victim.net.group != null && victim.net.group.restricted) || (deathInfo != null && deathInfo.damageTypes.GetMajorityDamageType() == DamageType.Suicide && !victim.IsWounded() && !reclaim_suicide)))
		{
			return;
		}
		if (victim != null && !victim.IsNpc && !victim.IsInTutorial)
		{
			if (!reclaim_building_auth)
			{
				BuildingPrivlidge buildingPrivilege = victim.GetBuildingPrivilege();
				if (buildingPrivilege != null && buildingPrivilege.IsAuthed(victim))
				{
					return;
				}
			}
			SetInventoryLocked(victim, wantsLocked: false);
			if (ReclaimManager.instance == null)
			{
				Debug.LogWarning("No reclaim manage for softcore");
				return;
			}
			List<Item> obj = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj2 = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj3 = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj4 = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj5 = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj6 = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj7 = Facepunch.Pool.Get<List<Item>>();
			List<Item> obj8 = Facepunch.Pool.Get<List<Item>>();
			if (StartingItemDefs == null)
			{
				StartingItemDefs = new ItemDefinition[StartingItems.Length];
				for (int i = 0; i < StartingItems.Length; i++)
				{
					StartingItemDefs[i] = ItemManager.FindItemDefinition(StartingItems[i]);
				}
			}
			victim.inventory.containerBelt.RemoveItemsFromContainer(obj5, StartingItemDefs);
			victim.inventory.containerMain.RemoveItemsFromContainer(obj7, StartingItemDefs);
			victim.inventory.containerWear.RemoveItemsFromContainer(obj6, StartingItemDefs);
			victim.inventory.containerBelt.RemoveFractionOfContainer(obj, 1f - reclaim_fraction_belt);
			victim.inventory.containerMain.RemoveFractionOfContainer(obj3, 1f - reclaim_fraction_main);
			Item backpackWithInventory = victim.inventory.GetBackpackWithInventory();
			if (backpackWithInventory != null)
			{
				backpackWithInventory.contents.RemoveItemsFromContainer(obj8, StartingItemDefs);
				backpackWithInventory.contents.RemoveFractionOfContainer(obj4, 1f - reclaim_fraction_main);
				if (obj4.Count > 0)
				{
					backpackWithInventory.contents.MergeAllStacks();
				}
				ReturnItemsTo(obj8, backpackWithInventory.contents);
			}
			victim.inventory.containerWear.RemoveFractionOfContainer(obj2, 1f - reclaim_fraction_wear);
			if (obj.Count > 0 || obj2.Count > 0 || obj3.Count > 0)
			{
				ReclaimManager.instance.AddPlayerReclaim(victim.userID, obj, obj2, obj3, obj4);
			}
			ReturnItemsTo(obj5, victim.inventory.containerBelt);
			ReturnItemsTo(obj7, victim.inventory.containerMain);
			ReturnItemsTo(obj6, victim.inventory.containerWear);
			if (backpackWithInventory != null && obj2.Contains(backpackWithInventory))
			{
				victim.inventory.containerMain.MergeAllStacks();
				backpackWithInventory.contents.MoveAllItems(victim.inventory.containerMain);
				backpackWithInventory.contents.MoveAllItems(victim.inventory.containerBelt);
				if (backpackWithInventory.contents.itemList.Count > 0)
				{
					backpackWithInventory.contents.Drop(ReclaimManager.ReclaimCorpsePrefab, victim.GetDropPosition(), victim.transform.rotation, 0f);
				}
			}
			Facepunch.Pool.Free(ref obj, freeElements: false);
			Facepunch.Pool.Free(ref obj2, freeElements: false);
			Facepunch.Pool.Free(ref obj3, freeElements: false);
			Facepunch.Pool.Free(ref obj4, freeElements: false);
			Facepunch.Pool.Free(ref obj5, freeElements: false);
			Facepunch.Pool.Free(ref obj6, freeElements: false);
			Facepunch.Pool.Free(ref obj7, freeElements: false);
			Facepunch.Pool.Free(ref obj8, freeElements: false);
		}
		base.OnPlayerDeath(instigator, victim, deathInfo);
	}

	public override void OnPlayerRespawn(BasePlayer player)
	{
		base.OnPlayerRespawn(player);
		if (!(ReclaimManager.instance == null))
		{
			ReclaimManager.instance.GetReclaim(player.userID)?.GiveToPlayer(player);
		}
	}

	public override PooledList<SleepingBag> FindSleepingBagsForPlayer(ulong playerID, bool ignoreTimers)
	{
		return SleepingBag.FindForPlayer(playerID, ignoreTimers);
	}

	public override float CorpseRemovalTime(BaseCorpse corpse)
	{
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument != null && monument.IsSafeZone && monument.Bounds.Contains(corpse.transform.position))
			{
				return 30f;
			}
		}
		return ConVar.Server.corpsedespawn;
	}

	public void SetInventoryLocked(BasePlayer player, bool wantsLocked)
	{
		player.inventory.containerMain.SetLocked(wantsLocked);
		player.inventory.containerBelt.SetLocked(wantsLocked);
		player.inventory.containerWear.SetLocked(wantsLocked);
	}

	public override void OnPlayerWounded(BasePlayer instigator, BasePlayer victim, HitInfo info)
	{
		base.OnPlayerWounded(instigator, victim, info);
		SetInventoryLocked(victim, wantsLocked: true);
	}

	public override void OnPlayerRevived(BasePlayer instigator, BasePlayer victim)
	{
		if (!victim.IsRestrained)
		{
			SetInventoryLocked(victim, wantsLocked: false);
		}
		base.OnPlayerRevived(instigator, victim);
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveItemsFrom(PlayerInventory inv, BaseEntity source, Item item)
	{
		if (item.parent != null && item.parent.HasFlag(ItemContainer.Flag.IsPlayer))
		{
			return new PlayerInventory.CanMoveFromResponse(!item.parent.IsLocked(), PlayerInventoryErrors.InventoryLockedError);
		}
		return base.CanMoveItemsFrom(inv, source, item);
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: RaidWindowNextChangeAt for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_RaidWindowNextChangeAt);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_RaidWindowNextChangeAt;
				ulong _sync_RaidWindowNextChangeAt = reader.UInt64();
				__sync_RaidWindowNextChangeAt = _sync_RaidWindowNextChangeAt;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "RaidWindowNextChangeAt")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
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
		__sync_RaidWindowNextChangeAt = 0uL;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
