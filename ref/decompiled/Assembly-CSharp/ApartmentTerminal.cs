#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ApartmentTerminal : ComputerStation
{
	public enum ValueBucket
	{
		None,
		Low,
		Medium,
		High
	}

	private ApartmentBuilding cachedBuilding;

	private List<RentableShop> cachedBlockShops;

	[Header("Apartment Terminal")]
	public float MediumValueThreshold = 100f;

	public float HighValueThreshold = 500f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ApartmentTerminal.OnRpcMessage"))
		{
			if (rpc == 3958206997u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_RequestProperties");
				}
				using (TimeWarning.New("SERVER_RequestProperties"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3958206997u, "SERVER_RequestProperties", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(3958206997u, "SERVER_RequestProperties", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3958206997u, "SERVER_RequestProperties", this, player, 3f))
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
							SERVER_RequestProperties(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_RequestProperties");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private ApartmentBuilding ResolveBuilding()
	{
		if (cachedBuilding != null && !cachedBuilding.IsDestroyed)
		{
			return cachedBuilding;
		}
		ApartmentBuilding apartmentBuilding = null;
		float num = ApartmentBuilding.MaxRadiusSearch;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is ApartmentBuilding apartmentBuilding2)
			{
				float num2 = Vector3.Distance(apartmentBuilding2.transform.position, base.transform.position);
				if (num2 <= num)
				{
					num = num2;
					apartmentBuilding = apartmentBuilding2;
				}
			}
		}
		cachedBuilding = apartmentBuilding;
		return cachedBuilding;
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.FromMounted]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SERVER_RequestProperties(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null)
		{
			return;
		}
		ApartmentBuilding apartmentBuilding = ResolveBuilding();
		if (apartmentBuilding == null)
		{
			return;
		}
		using ApartmentTerminalData apartmentTerminalData = Facepunch.Pool.Get<ApartmentTerminalData>();
		apartmentTerminalData.plots = Facepunch.Pool.Get<List<ApartmentPlotEntry>>();
		int num = 0;
		int num2 = 0;
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (!(room == null))
			{
				bool flag = room.IsCurrentlyRented();
				ApartmentPlotEntry apartmentPlotEntry = Facepunch.Pool.Get<ApartmentPlotEntry>();
				apartmentPlotEntry.roomNumber = room.RoomNumber;
				apartmentPlotEntry.roomId = room.net.ID;
				apartmentPlotEntry.occupied = flag;
				apartmentPlotEntry.size = (int)room.Size;
				apartmentPlotEntry.storageSlots = room.GetTotalStorageCapacity();
				apartmentPlotEntry.roomCount = GetRoomCount(room.Size);
				apartmentPlotEntry.estimatedValue = (int)GetEstimatedValue(room);
				apartmentTerminalData.plots.Add(apartmentPlotEntry);
				if (flag)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		apartmentTerminalData.totalAvailable = num;
		apartmentTerminalData.totalOccupied = num2;
		PopulateShops(apartmentTerminalData, apartmentBuilding);
		ClientRPC(RpcTarget.Player("CLIENT_ReceiveProperties", player), apartmentTerminalData);
	}

	private void PopulateShops(ApartmentTerminalData data, ApartmentBuilding building)
	{
		data.shops = Facepunch.Pool.Get<List<ApartmentPlotEntry>>();
		int num = 0;
		int num2 = 0;
		foreach (RentableShop item in ResolveBlockShops(building))
		{
			if (!(item == null) && !item.IsDestroyed)
			{
				bool flag = item.IsOn();
				VendingMachine serverVendingMachine = item.GetServerVendingMachine();
				ApartmentPlotEntry apartmentPlotEntry = Facepunch.Pool.Get<ApartmentPlotEntry>();
				apartmentPlotEntry.occupied = flag;
				apartmentPlotEntry.storageSlots = 30;
				apartmentPlotEntry.estimatedValue = (int)((serverVendingMachine != null) ? BucketFromSum(SumStorageValue(serverVendingMachine.inventory)) : ValueBucket.None);
				apartmentPlotEntry.roomNumber = item.ShopNumberId.ToString();
				data.shops.Add(apartmentPlotEntry);
				if (flag)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		data.totalShopsAvailable = num;
		data.totalShopsOccupied = num2;
	}

	private List<RentableShop> ResolveBlockShops(ApartmentBuilding building)
	{
		if (cachedBlockShops != null)
		{
			return cachedBlockShops;
		}
		cachedBlockShops = new List<RentableShop>();
		foreach (RentableShop allServerShop in RentableShop.AllServerShops)
		{
			if (!(allServerShop == null) && !allServerShop.IsDestroyed && !(Vector3.Distance(allServerShop.transform.position, building.transform.position) > ApartmentBuilding.MaxRadiusSearch))
			{
				cachedBlockShops.Add(allServerShop);
			}
		}
		return cachedBlockShops;
	}

	private int GetRoomCount(ApartmentSize size)
	{
		return size switch
		{
			ApartmentSize.Small => 1, 
			ApartmentSize.Medium => 2, 
			ApartmentSize.Large => 3, 
			_ => 0, 
		};
	}

	private ValueBucket GetEstimatedValue(ApartmentRoom room)
	{
		float num = 0f;
		foreach (BaseEntity item in room.Furniture)
		{
			if (item is StorageContainer { IsDestroyed: false } storageContainer)
			{
				num += SumStorageValue(storageContainer.inventory);
			}
		}
		num += SumSleepingOwnerValue(room);
		if (room.UpkeepTerminal != null && !room.UpkeepTerminal.IsDestroyed)
		{
			num += SumStorageValue(room.UpkeepTerminal.inventory);
		}
		return BucketFromSum(num);
	}

	private float SumStorageValue(ItemContainer inventory)
	{
		if (inventory == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Item item in inventory.itemList)
		{
			num += GetApartmentTaxValue(item);
		}
		return num;
	}

	private float SumSleepingOwnerValue(ApartmentRoom room)
	{
		float num = 0f;
		List<Item> obj = null;
		foreach (ulong owner in room.Owners)
		{
			BasePlayer basePlayer = BasePlayer.FindSleeping(owner);
			if (basePlayer == null || basePlayer.IsDestroyed || basePlayer.inventory == null || !room.IsInsideRoom(basePlayer))
			{
				continue;
			}
			if (obj == null)
			{
				obj = Facepunch.Pool.Get<List<Item>>();
			}
			basePlayer.inventory.GetAllItems(obj);
			foreach (Item item in obj)
			{
				num += GetApartmentTaxValue(item);
			}
		}
		if (obj != null)
		{
			Facepunch.Pool.Free(ref obj, freeElements: false);
		}
		return num;
	}

	private static float GetApartmentTaxValue(Item item)
	{
		if (item.info.ApartmentTaxPerStack > 0f)
		{
			return item.info.ApartmentTaxPerStack * ((float)item.amount / (float)item.MaxStackable());
		}
		return 0f;
	}

	private ValueBucket BucketFromSum(float sum)
	{
		if (sum <= 0f)
		{
			return ValueBucket.None;
		}
		if (sum >= HighValueThreshold)
		{
			return ValueBucket.High;
		}
		if (sum >= MediumValueThreshold)
		{
			return ValueBucket.Medium;
		}
		return ValueBucket.Low;
	}
}
