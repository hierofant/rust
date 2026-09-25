using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAchievementAdvanced : TriggerBase
{
	public enum AchievementTriggerMode
	{
		Enter,
		Exit
	}

	[Flags]
	public enum ExitSideMask
	{
		Any = 0,
		Front = 1,
		Bottom = 2,
		Left = 4,
		Right = 8,
		Back = 0x10,
		Top = 0x20
	}

	public string statToIncrease = "";

	public string achievementName = "";

	public string requiredVehicleName = "";

	public bool allowDuringTutorial;

	[SerializeField]
	private BoxCollider boxCollider;

	public AchievementTriggerMode triggerMode;

	public ExitSideMask requiredExitSides;

	public BasePlayer.PlayerFlags requiredPlayerFlags;

	public string[] requireWearingItemNames = Array.Empty<string>();

	[NonSerialized]
	private List<ulong> triggeredPlayers = new List<ulong>();

	public void OnPuzzleReset()
	{
		Reset();
	}

	public void Reset()
	{
		triggeredPlayers.Clear();
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	protected virtual bool VerifyAdditionalConditions(BasePlayer ply, bool onExit)
	{
		if (requireWearingItemNames.Length != 0 && !IsWearingRequiredItems(ply))
		{
			return false;
		}
		if (requiredPlayerFlags != 0 && !ply.HasPlayerFlag(requiredPlayerFlags))
		{
			return false;
		}
		return true;
	}

	private bool IsWearingRequiredItems(BasePlayer ply)
	{
		if (ply == null || ply.inventory == null || ply.inventory.containerWear == null)
		{
			return false;
		}
		bool result = true;
		string[] array = requireWearingItemNames;
		foreach (string text in array)
		{
			if (ply.inventory.containerWear.FindItemByItemName(text) == null)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		TryGrant(ent, onExit: false);
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		TryGrant(ent, onExit: true);
	}

	private void TryGrant(BaseEntity ent, bool onExit)
	{
		if ((!onExit || triggerMode == AchievementTriggerMode.Exit) && (onExit || triggerMode == AchievementTriggerMode.Enter) && !(ent == null) && !ent.isClient && IsValidPlayer(ent, out var ply) && (requiredExitSides == ExitSideMask.Any || IsAllowedExitSide(ply)) && VerifyAdditionalConditions(ply, onExit))
		{
			GrantToPlayer(ply);
		}
	}

	private bool IsValidPlayer(BaseEntity ent, out BasePlayer ply)
	{
		ply = null;
		if (ent == null)
		{
			return false;
		}
		ply = ent.GetComponent<BasePlayer>();
		if (ply == null || !ply.IsAlive() || ply.IsSleeping() || ply.IsNpc)
		{
			return false;
		}
		if (triggeredPlayers.Contains(ply.userID))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(requiredVehicleName))
		{
			BaseVehicle mountedVehicle = ply.GetMountedVehicle();
			if (mountedVehicle == null)
			{
				return false;
			}
			if (!mountedVehicle.ShortPrefabName.Contains(requiredVehicleName))
			{
				return false;
			}
		}
		return true;
	}

	private void GrantToPlayer(BasePlayer ply)
	{
		if (!ply.isClient)
		{
			if (!string.IsNullOrEmpty(achievementName))
			{
				ply.GiveAchievement(achievementName, allowDuringTutorial);
			}
			if (!string.IsNullOrEmpty(statToIncrease))
			{
				ply.stats.Add(statToIncrease, 1);
				ply.stats.Save(forceSteamSave: true);
			}
			triggeredPlayers.Add(ply.userID);
		}
	}

	private bool IsAllowedExitSide(BasePlayer ply)
	{
		if (requiredExitSides == ExitSideMask.Any)
		{
			return true;
		}
		ExitSideMask exitSide = GetExitSide(ply);
		if (exitSide == ExitSideMask.Any)
		{
			return false;
		}
		return (requiredExitSides & exitSide) != 0;
	}

	private ExitSideMask GetExitSide(BasePlayer ply)
	{
		if (!boxCollider)
		{
			return ExitSideMask.Any;
		}
		Vector3 vector = boxCollider.transform.InverseTransformPoint(ply.transform.position) - boxCollider.center;
		Vector3 vector2 = boxCollider.size * 0.5f;
		float num = ((vector2.x > 0f) ? (Mathf.Abs(vector.x) / vector2.x) : (-1f));
		float num2 = ((vector2.y > 0f) ? (Mathf.Abs(vector.y) / vector2.y) : (-1f));
		float num3 = ((vector2.z > 0f) ? (Mathf.Abs(vector.z) / vector2.z) : (-1f));
		if (num >= num2 && num >= num3)
		{
			if (!(vector.x >= 0f))
			{
				return ExitSideMask.Left;
			}
			return ExitSideMask.Right;
		}
		if (num2 >= num && num2 >= num3)
		{
			if (!(vector.y >= 0f))
			{
				return ExitSideMask.Bottom;
			}
			return ExitSideMask.Top;
		}
		if (!(vector.z >= 0f))
		{
			return ExitSideMask.Back;
		}
		return ExitSideMask.Front;
	}

	private void OnDrawGizmosSelected()
	{
		BoxCollider boxCollider = (this.boxCollider ? this.boxCollider : GetComponent<BoxCollider>());
		if ((bool)boxCollider)
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = boxCollider.transform.localToWorldMatrix;
			Vector3 center = boxCollider.center;
			Vector3 size = boxCollider.size;
			float markerSize = Mathf.Max(Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.08f, 0.03f);
			Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.75f);
			Gizmos.DrawWireCube(center, size);
			DrawSideGizmo(center, size, Vector3.forward, ExitSideMask.Front, markerSize);
			DrawSideGizmo(center, size, Vector3.back, ExitSideMask.Back, markerSize);
			DrawSideGizmo(center, size, Vector3.left, ExitSideMask.Left, markerSize);
			DrawSideGizmo(center, size, Vector3.right, ExitSideMask.Right, markerSize);
			DrawSideGizmo(center, size, Vector3.up, ExitSideMask.Top, markerSize);
			DrawSideGizmo(center, size, Vector3.down, ExitSideMask.Bottom, markerSize);
			Gizmos.matrix = matrix;
		}
	}

	private void DrawSideGizmo(Vector3 center, Vector3 size, Vector3 normal, ExitSideMask side, float markerSize)
	{
		Gizmos.color = ((requiredExitSides == ExitSideMask.Any || (requiredExitSides & side) != 0) ? new Color(0.2f, 1f, 0.35f, 0.9f) : new Color(1f, 0.25f, 0.25f, 0.7f));
		Vector3 b = size * 0.5f;
		Vector3 vector = center + Vector3.Scale(normal, b);
		Vector3 vector2 = vector + normal * (markerSize * 3f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawSphere(vector2, markerSize);
	}
}
