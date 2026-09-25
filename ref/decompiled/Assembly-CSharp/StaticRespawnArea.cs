using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class StaticRespawnArea : SleepingBag
{
	public Transform[] spawnAreas;

	public bool allowHostileSpawns;

	public bool requireAuth;

	[NonSerialized]
	private HashSet<ulong> authorizedUsers = new HashSet<ulong>();

	public static HashSet<StaticRespawnArea> staticRespawnAreas = new HashSet<StaticRespawnArea>();

	public override void ServerInit()
	{
		base.ServerInit();
		staticRespawnAreas.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		staticRespawnAreas.Remove(this);
	}

	public bool IsAuthed(ulong playerID)
	{
		if (!requireAuth)
		{
			return true;
		}
		return authorizedUsers.Contains(playerID);
	}

	public void Authorize(ulong playerID)
	{
		authorizedUsers.Add(playerID);
	}

	public void Deauthorize(ulong playerID)
	{
		authorizedUsers.Remove(playerID);
	}

	public override bool ValidForPlayer(ulong playerID, bool ignoreTimers)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(playerID);
		if (basePlayer != null && basePlayer.IsInTutorial)
		{
			return false;
		}
		if (ignoreTimers || allowHostileSpawns)
		{
			return true;
		}
		return basePlayer.GetHostileDuration() <= 0f;
	}

	public override void GetSpawnPos(out Vector3 pos, out Quaternion rot)
	{
		Transform transform = spawnAreas[UnityEngine.Random.Range(0, spawnAreas.Length)];
		pos = transform.transform.position + spawnOffset;
		rot = Quaternion.Euler(0f, transform.transform.rotation.eulerAngles.y, 0f);
	}

	public override void SetUnlockTime(float newTime)
	{
		unlockTime = 0f;
	}

	public override float GetUnlockSeconds(ulong playerID)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(playerID);
		if (basePlayer == null || allowHostileSpawns)
		{
			return base.GetUnlockSeconds(playerID);
		}
		return Mathf.Max(basePlayer.GetHostileDuration(), base.GetUnlockSeconds(playerID));
	}

	public override void Save(SaveInfo info)
	{
		if (info.forDisk)
		{
			info.msg.staticRespawn = Pool.Get<StaticRespawnAreaData>();
			info.msg.staticRespawn.authorizedUsers = Pool.Get<List<ulong>>();
			info.msg.staticRespawn.authorizedUsers.AddRange(authorizedUsers);
		}
		base.Save(info);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk || info.msg.staticRespawn == null || info.msg.staticRespawn.authorizedUsers == null)
		{
			return;
		}
		authorizedUsers.Clear();
		foreach (ulong authorizedUser in info.msg.staticRespawn.authorizedUsers)
		{
			authorizedUsers.Add(authorizedUser);
		}
	}
}
