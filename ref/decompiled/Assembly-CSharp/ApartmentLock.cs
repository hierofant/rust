using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ApartmentLock : BaseLock
{
	public ApartmentRoom Room;

	private bool HasAccess(BasePlayer player)
	{
		if (Room == null)
		{
			return false;
		}
		if (!Room.IsAuthed(player.userID))
		{
			return Room.IsBreakInActive();
		}
		return true;
	}

	private bool IsFrontDoorAndPlayerInside(BasePlayer player)
	{
		if (Room == null)
		{
			return false;
		}
		if (IsFrontDoor())
		{
			return Room.IsInsideRoom(player);
		}
		return false;
	}

	private bool IsFrontDoor()
	{
		if (Room == null || Room.FrontDoor == null)
		{
			return false;
		}
		return GetParentEntity() == Room.FrontDoor;
	}

	public override bool OnTryToOpen(BasePlayer player)
	{
		if (HasAccess(player))
		{
			return true;
		}
		if (IsFrontDoorAndPlayerInside(player))
		{
			ScheduleAutoClose();
			return true;
		}
		return false;
	}

	public override bool OnTryToClose(BasePlayer player)
	{
		if (HasAccess(player))
		{
			if (IsFrontDoor())
			{
				CancelAutoClose();
			}
			return true;
		}
		return false;
	}

	private void ScheduleAutoClose()
	{
		CancelAutoClose();
		ApartmentDoor apartmentDoor = Room?.FrontDoor;
		if (apartmentDoor != null)
		{
			apartmentDoor.Invoke(apartmentDoor.CloseRequest, 5f);
		}
	}

	private void CancelAutoClose()
	{
		ApartmentDoor apartmentDoor = Room?.FrontDoor;
		if (apartmentDoor != null)
		{
			apartmentDoor.CancelInvoke(apartmentDoor.CloseRequest);
		}
	}

	public override bool HasLockPermission(BasePlayer player)
	{
		return HasAccess(player);
	}

	public override bool GetPlayerLockPermission(BasePlayer player)
	{
		return HasAccess(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.msg.apartmentLock != null)
		{
			NetworkableId apartmentId = info.msg.apartmentLock.apartmentId;
			Room = BaseNetworkable.serverEntities.Find(apartmentId) as ApartmentRoom;
			if (Room == null)
			{
				Debug.LogWarning($"ApartmentLock {this} couldn't find apartment room '{apartmentId}' when loading");
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.apartmentLock = Pool.Get<ProtoBuf.ApartmentLock>();
		if (Room != null)
		{
			info.msg.apartmentLock.apartmentId = Room.net.ID;
		}
	}
}
