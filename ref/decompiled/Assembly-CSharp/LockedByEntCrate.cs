using System;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class LockedByEntCrate : LootContainer
{
	[NonSerialized]
	public BaseEntity lockingEnt;

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk && lockingEnt.IsValid())
		{
			info.msg.lockedByEntCrate = Pool.Get<ProtoBuf.LockedByEntCrate>();
			info.msg.lockedByEntCrate.lockingEntId = lockingEnt.net.ID;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.lockedByEntCrate != null && info.msg.lockedByEntCrate.lockingEntId != default(NetworkableId))
		{
			lockingEnt = BaseNetworkable.serverEntities.Find(info.msg.lockedByEntCrate.lockingEntId) as BaseEntity;
			SetLockingEnt(lockingEnt);
		}
	}

	public void SetLockingEnt(BaseEntity ent)
	{
		CancelInvoke(Think);
		SetLocked(isLocked: false);
		lockingEnt = ent;
		if (lockingEnt != null)
		{
			InvokeRepeating(Think, UnityEngine.Random.Range(0f, 1f), 1f);
			SetLocked(isLocked: true);
		}
	}

	public void SetLocked(bool isLocked)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, isLocked);
		flagsUpdateScope.Set(Flags.Locked, isLocked);
	}

	public void Think()
	{
		if (lockingEnt == null && IsLocked())
		{
			SetLockingEnt(null);
		}
	}
}
