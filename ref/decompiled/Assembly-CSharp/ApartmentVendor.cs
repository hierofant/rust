using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class ApartmentVendor : NPCTalking
{
	public enum ApartmentConversationUpgrade
	{
		None,
		Small_Medium,
		Small_Large,
		Medium_Large,
		Last
	}

	private EntityRef<ApartmentBuilding> __sync_BuildingRef;

	[Sync(Autosave = true)]
	public EntityRef<ApartmentBuilding> BuildingRef
	{
		[CompilerGenerated]
		get
		{
			return __sync_BuildingRef;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_BuildingRef, value))
			{
				__sync_BuildingRef = value;
				byte nameID = __GetWeaverID("BuildingRef");
				QueueSyncVar(nameID);
			}
		}
	}

	public override void OnConversationAction(BasePlayer player, string action)
	{
		base.OnConversationAction(player, action);
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		switch (action)
		{
		case "PurchaseSmall":
			TryPurchaseApartment(player, ApartmentSize.Small);
			break;
		case "PurchaseMedium":
			TryPurchaseApartment(player, ApartmentSize.Medium);
			break;
		case "PurchaseLarge":
			TryPurchaseApartment(player, ApartmentSize.Large);
			break;
		case "UpgradeMedium":
			apartmentBuilding.TryUpgradeRoom(player, ApartmentSize.Medium);
			break;
		case "UpgradeLarge":
			apartmentBuilding.TryUpgradeRoom(player, ApartmentSize.Large);
			break;
		case "Checkout":
			apartmentBuilding.TryCheckout(player);
			break;
		}
	}

	private void TryPurchaseApartment(BasePlayer player, ApartmentSize size)
	{
		BuildingRef.Get(serverside: true).TryPurchaseRoom(player, size);
	}

	public bool Conversation_CanUpgradeApartment(BasePlayer player, int size)
	{
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		if (apartmentBuilding == null)
		{
			Debug.LogError($"ApartmentVendor.Conversation_CanUpgradeApartment: BuildingRef is null for vendor {this}", this);
			return false;
		}
		switch ((ApartmentSize)size)
		{
		case ApartmentSize.Small:
			return apartmentBuilding.CanUpgradeRoom(player, ApartmentSize.Small);
		case ApartmentSize.Medium:
			return apartmentBuilding.CanUpgradeRoom(player, ApartmentSize.Medium);
		case ApartmentSize.Large:
			return apartmentBuilding.CanUpgradeRoom(player, ApartmentSize.Large);
		case ApartmentSize.Any:
		{
			ApartmentSize playerApartmentSize = apartmentBuilding.GetPlayerApartmentSize(player);
			return playerApartmentSize == ApartmentSize.Small || playerApartmentSize == ApartmentSize.Medium;
		}
		default:
			return false;
		}
	}

	public bool Conversation_CanPurchaseApartment(BasePlayer player, int size)
	{
		ApartmentBuilding apartmentBuilding = BuildingRef.Get(serverside: true);
		if (apartmentBuilding == null)
		{
			Debug.LogError($"ApartmentVendor.Conversation_CanPurchaseApartment: BuildingRef is null for vendor {this}", this);
			return false;
		}
		return (ApartmentSize)size switch
		{
			ApartmentSize.Small => apartmentBuilding.CanBuyRoom(player, ApartmentSize.Small), 
			ApartmentSize.Medium => apartmentBuilding.CanBuyRoom(player, ApartmentSize.Medium), 
			ApartmentSize.Large => apartmentBuilding.CanBuyRoom(player, ApartmentSize.Large), 
			ApartmentSize.Any => apartmentBuilding.GetPlayerApartmentSize(player) == ApartmentSize.None, 
			_ => false, 
		};
	}

	public bool Conversation_CanCheckoutApartment(BasePlayer player)
	{
		if (!TryGetBuilding(out var building, "Conversation_CanCheckoutApartment"))
		{
			return false;
		}
		return building.GetPlayerApartmentSize(player) != ApartmentSize.None;
	}

	public bool Conversation_OwnsApartment(BasePlayer player, ApartmentSize size)
	{
		if (!TryGetBuilding(out var building, "Conversation_OwnsApartment"))
		{
			return false;
		}
		ApartmentSize playerApartmentSize = building.GetPlayerApartmentSize(player);
		if (size == ApartmentSize.Any)
		{
			return playerApartmentSize != ApartmentSize.None;
		}
		return playerApartmentSize == size;
	}

	public bool Conversation_CanAffordApartment(BasePlayer player, ApartmentSize size)
	{
		if (!TryGetBuilding(out var building, "Conversation_CanAffordApartment"))
		{
			return false;
		}
		return building.CanAffordRoom(player, size);
	}

	public bool Conversation_CanAffordUpgrade(BasePlayer player, ApartmentConversationUpgrade upgrade)
	{
		if (!TryGetBuilding(out var building, "Conversation_CanAffordUpgrade"))
		{
			return false;
		}
		return upgrade switch
		{
			ApartmentConversationUpgrade.Small_Medium => building.CanAffordUpgrade(player, ApartmentSize.Small, ApartmentSize.Medium), 
			ApartmentConversationUpgrade.Small_Large => building.CanAffordUpgrade(player, ApartmentSize.Small, ApartmentSize.Large), 
			ApartmentConversationUpgrade.Medium_Large => building.CanAffordUpgrade(player, ApartmentSize.Medium, ApartmentSize.Large), 
			_ => false, 
		};
	}

	public bool Conversation_HasAnyRoomAvailable()
	{
		if (!TryGetBuilding(out var building, "Conversation_HasAnyRoomAvailable"))
		{
			return false;
		}
		if (building.GetRemainingRoomCount(ApartmentSize.Small) <= 0 && building.GetRemainingRoomCount(ApartmentSize.Medium) <= 0)
		{
			return building.GetRemainingRoomCount(ApartmentSize.Large) > 0;
		}
		return true;
	}

	public bool TryGetBuilding(out ApartmentBuilding building, string logErrorPrefix = "")
	{
		if (!BuildingRef.TryGet(serverside: true, out building))
		{
			string arg = (string.IsNullOrWhiteSpace(logErrorPrefix) ? string.Empty : ("ApartmentVendor." + logErrorPrefix + ": "));
			Debug.LogError($"{arg} BuildingRef is null for vendor {this}", this);
			return false;
		}
		return true;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: BuildingRef for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_BuildingRef);
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
				_ = __sync_BuildingRef;
				EntityRef<ApartmentBuilding> _sync_BuildingRef = NetworkReadEx.EntityRef<ApartmentBuilding>(reader);
				__sync_BuildingRef = _sync_BuildingRef;
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
		if (propertyName == "BuildingRef")
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
		__sync_BuildingRef = default(EntityRef<ApartmentBuilding>);
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
