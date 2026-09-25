using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class BoatAICoordination
{
	private static readonly HashSet<BoatAI> ActiveBoats = new HashSet<BoatAI>();

	private static readonly Dictionary<BaseEntity, BoatAI> TargetClaims = new Dictionary<BaseEntity, BoatAI>();

	private static readonly Dictionary<int, ListHashSet<BoatAI>> Groups = new Dictionary<int, ListHashSet<BoatAI>>();

	private static int _nextGroupId = 1;

	public static int GetNextGroupId()
	{
		return _nextGroupId++;
	}

	public static void Register(BoatAI boat)
	{
		ActiveBoats.Add(boat);
	}

	public static void Unregister(BoatAI boat)
	{
		ActiveBoats.Remove(boat);
		using PooledList<BaseEntity> pooledList = Pool.Get<PooledList<BaseEntity>>();
		foreach (KeyValuePair<BaseEntity, BoatAI> targetClaim in TargetClaims)
		{
			if (targetClaim.Value == boat)
			{
				pooledList.Add(targetClaim.Key);
			}
		}
		foreach (BaseEntity item in pooledList)
		{
			TargetClaims.Remove(item);
		}
	}

	public static bool TryClaimTarget(BoatAI boat, BasePlayer ply)
	{
		if (BoatAI.PRINT_DEBUGS)
		{
			Debug.Log("BoatAI " + boat.name + " is trying to claim target " + ply.displayName);
		}
		BaseEntity claimEntity = GetClaimEntity(ply);
		if (claimEntity == null)
		{
			return false;
		}
		if (!TargetClaims.ContainsKey(claimEntity))
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log(boat.name + " claimed entity " + claimEntity.name);
			}
			TargetClaims[claimEntity] = boat;
			return true;
		}
		return false;
	}

	public static void ReleaseClaim(BoatAI boat, BasePlayer ply)
	{
		BaseEntity claimEntity = GetClaimEntity(ply);
		if (!(claimEntity == null) && TargetClaims.TryGetValue(claimEntity, out var value) && !(value != boat))
		{
			TargetClaims.Remove(claimEntity);
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log(boat.name + " released entity " + claimEntity.name);
			}
			BoatAI boatAI = FindFreeBoatWithSameTarget(boat, ply);
			if ((bool)boatAI)
			{
				boatAI.OnTargetClaimAvailable(ply);
			}
		}
	}

	private static BaseEntity GetClaimEntity(BasePlayer ply)
	{
		if (ply == null)
		{
			return null;
		}
		if (ply.isMounted)
		{
			BaseMountable mounted = ply.GetMounted();
			if (mounted != null)
			{
				return mounted;
			}
		}
		return ply;
	}

	public static bool IsTargetClaimed(BasePlayer ply)
	{
		BaseEntity claimEntity = GetClaimEntity(ply);
		if (claimEntity == null)
		{
			return false;
		}
		return TargetClaims.ContainsKey(claimEntity);
	}

	public static bool IsTargetClaimedByAnotherGroup(BoatAI currentBoat, BasePlayer ply)
	{
		BaseEntity claimEntity = GetClaimEntity(ply);
		if (claimEntity == null)
		{
			return false;
		}
		if (!TargetClaims.TryGetValue(claimEntity, out var value))
		{
			return false;
		}
		if (currentBoat.GroupId != value.GroupId)
		{
			return true;
		}
		return false;
	}

	private static BoatAI FindFreeBoatWithSameTarget(BoatAI currentBoat, BasePlayer ply)
	{
		foreach (BoatAI activeBoat in ActiveBoats)
		{
			if (!(currentBoat == activeBoat) && (!TargetClaims.TryGetValue(GetClaimEntity(ply), out var value) || !(value == activeBoat)) && activeBoat.ActiveTarget is PlayerTarget playerTarget && playerTarget.Player.userID.Get() == ply.userID.Get())
			{
				return activeBoat;
			}
		}
		return null;
	}

	public static void AddToGroup(BoatAI boat, int groupId)
	{
		if (!Groups.TryGetValue(groupId, out var value))
		{
			value = new ListHashSet<BoatAI>();
			Groups[groupId] = value;
		}
		value.Add(boat);
		boat.OnGroupChanged(groupId);
	}

	public static void RemoveFromGroup(BoatAI boat, int groupId)
	{
		if (Groups.TryGetValue(groupId, out var value) && value != null && value.Contains(boat))
		{
			value.Remove(boat);
			boat.OnGroupChanged(-1);
		}
	}

	public static ListHashSet<BoatAI> GetGroupMembers(int groupId)
	{
		if (Groups.TryGetValue(groupId, out var value))
		{
			return value;
		}
		return null;
	}

	public static void WipeCoordination()
	{
		ActiveBoats.Clear();
		TargetClaims.Clear();
		Groups.Clear();
		_nextGroupId = 1;
	}
}
