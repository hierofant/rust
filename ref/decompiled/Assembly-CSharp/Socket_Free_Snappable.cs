using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class Socket_Free_Snappable : Socket_Free
{
	private struct SnapResult
	{
		public bool Valid;

		public float Score;

		public Construction.Placement Placement;

		public string Label;

		public static SnapResult Invalid
		{
			get
			{
				SnapResult result = default(SnapResult);
				result.Valid = false;
				result.Score = float.MaxValue;
				return result;
			}
		}
	}

	private struct BuildingBlockPadding
	{
		public enum PaddingType
		{
			WeaksideOnly,
			StrongsideOnly,
			Both
		}

		public float YPadding;

		public float NormalPadding;

		public PaddingType PaddingMode;
	}

	[ClientVar(Saved = true, Help = "The current snapping mode for deployables.")]
	public static int SnappingMode = 2;

	[ClientVar(Help = "(Generated) When enabled, draws debug visualisations for deployable snapping calculations showing candidate snap points and distances")]
	public static bool DebugSnapping = false;

	[Range(-1f, 1f)]
	[SerializeField]
	[Header("Snapping - General")]
	private float generalPadding;

	[Range(-1f, 1f)]
	[SerializeField]
	[Header("Snapping - Walls")]
	private float snappingPadding;

	[SerializeField]
	[Header("Snapping - Corners")]
	private bool allowSnappingToCorners = true;

	[SerializeField]
	[Range(-1f, 1f)]
	private float cornerPadding = -0.01f;

	[SerializeField]
	[Header("Snapping - Same Deployable")]
	private bool allowSnappingToSameDeployable = true;

	[Range(-1f, 1f)]
	[SerializeField]
	private float sameDeployablePadding;

	private BaseEntity staticEntity;

	private Construction staticConstruction;

	private static List<SnapResult> results = new List<SnapResult>();

	private const int SNAP_MASK = 136314880;

	private static readonly Dictionary<string, BuildingBlockPadding> _buildingBlockPaddingDatabase = new Dictionary<string, BuildingBlockPadding>
	{
		{
			"assets/prefabs/building core/foundation/foundation.container.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0.02f,
				NormalPadding = 0f,
				PaddingMode = BuildingBlockPadding.PaddingType.Both
			}
		},
		{
			"assets/prefabs/building core/wall/wall.wood.full.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0f,
				NormalPadding = 0.1f,
				PaddingMode = BuildingBlockPadding.PaddingType.StrongsideOnly
			}
		},
		{
			"assets/prefabs/building core/wall/wall.twig.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0f,
				NormalPadding = 0.1f,
				PaddingMode = BuildingBlockPadding.PaddingType.Both
			}
		},
		{
			"assets/prefabs/building boat/wall/wall.wood.full.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0.02f,
				NormalPadding = 0.02f,
				PaddingMode = BuildingBlockPadding.PaddingType.Both
			}
		}
	};

	private void AddDirections(Construction.Target target, PooledList<Vector3> directions, bool rayAligned)
	{
		directions.Clear();
		Vector3 up = GetUp(target);
		Vector3 vector;
		Vector3 vector2;
		if (rayAligned)
		{
			vector = target.ray.direction;
			vector -= up * Vector3.Dot(vector, up);
			vector2 = -Vector3.Cross(vector, up);
		}
		else
		{
			vector = target.entity.transform.forward;
			vector2 = target.entity.transform.right;
		}
		directions.Add(vector);
		directions.Add(-vector);
		directions.Add(vector2);
		directions.Add(-vector2);
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		staticEntity = rootObj.GetComponent<BaseEntity>();
		staticConstruction = rootObj.GetComponent<Construction>();
	}

	public override Construction.Placement DoPlacement(Construction.Target target)
	{
		if (target.snappingMode == 0)
		{
			return base.DoPlacement(target);
		}
		if (!target.isHoldingShift || target.entity == null)
		{
			return base.DoPlacement(target);
		}
		if (target.buildingBlocked)
		{
			return base.DoPlacement(target);
		}
		Vector3 up = GetUp(target);
		Vector3 direction = -up;
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement"))
		{
			Vector3 origin = target.position + target.normal * 0.15f;
			Ray ray = new Ray(origin, direction);
			using PooledList<RaycastHit> pooledList = Pool.Get<PooledList<RaycastHit>>();
			GamePhysics.TraceAll(ray, 0f, pooledList, 2f, 136314880);
			if (pooledList.Count > 0)
			{
				foreach (RaycastHit item in pooledList)
				{
					if (GamePhysics.LineOfSight(target.ray.origin, item.point + up * 0.1f, 136314880) && !(Vector3Ex.Distance2D(target.ray.origin, item.point) > staticConstruction.maxplaceDistance))
					{
						float num = staticEntity.bounds.extents.y - staticEntity.bounds.center.y;
						Vector3 vector = up * num;
						target.position = item.point + vector;
						float buildingBlockPadding = GetBuildingBlockPadding(item.collider.name, yPadding: true, item.collider.transform, item.normal);
						target.position += up * buildingBlockPadding;
					}
				}
				results.Clear();
				if (target.snappingMode == 2)
				{
					results.Add(TryCornerSnap(target));
					results.Add(TryMatchingDeployableSnap(target));
				}
				results.Add(TryWallSnap(target));
				SnapResult snapResult = SnapResult.Invalid;
				foreach (SnapResult result in results)
				{
					if (DebugSnapping && result.Valid)
					{
						Debug.Log($"[Snapping] Placement:{result.Label} (score: {result.Score:F3} (valid: {result.Valid})");
					}
					if (result.Valid && result.Score < snapResult.Score && ContainerCorpse.IsValidPointForEntity(staticEntity.prefabID, result.Placement.position, result.Placement.rotation))
					{
						if (DebugSnapping)
						{
							Debug.Log($"Selected best: {result.Label}, Score: {result.Score}");
						}
						snapResult = result;
					}
				}
				if (DebugSnapping)
				{
					Debug.Log($"Final Best Valid: {snapResult.Valid}, Score: {snapResult.Score}, Label: {snapResult.Label}");
				}
				if (snapResult.Valid)
				{
					if (DebugSnapping)
					{
						Debug.Log($"[Snapping] Best placement: {snapResult.Label} (score: {snapResult.Score:F3})");
					}
					return snapResult.Placement;
				}
				target.valid = false;
				return base.DoPlacement(target);
			}
			target.valid = false;
			return base.DoPlacement(target);
		}
	}

	private float GetMaxDistance()
	{
		return 2.5f;
	}

	private float GetBuildingBlockPadding(string name, bool yPadding, Transform buildingBlockTransform, Vector3 rayNormal)
	{
		if (_buildingBlockPaddingDatabase.TryGetValue(name, out var value))
		{
			if (yPadding)
			{
				return value.YPadding;
			}
			if (ShouldAddPadding(buildingBlockTransform, rayNormal, value.PaddingMode))
			{
				return value.NormalPadding;
			}
		}
		return 0f;
	}

	private bool ShouldAddPadding(Transform buildingBlockTransform, Vector3 rayNormal, BuildingBlockPadding.PaddingType type)
	{
		if (type == BuildingBlockPadding.PaddingType.Both)
		{
			return true;
		}
		Vector3 b = buildingBlockTransform.worldToLocalMatrix.MultiplyVector(-rayNormal);
		float num = worldForward.DotDegrees(b);
		return type switch
		{
			BuildingBlockPadding.PaddingType.WeaksideOnly => num > 90f, 
			BuildingBlockPadding.PaddingType.StrongsideOnly => num < 90f, 
			_ => false, 
		};
	}

	private SnapResult TryWallSnap(Construction.Target target)
	{
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement.TryWallSnap"))
		{
			if (TryFindBestSnappingHit(target, out var bestHit))
			{
				Quaternion targetRotation = ComputeSnappedRotation(target, bestHit);
				Vector3 vector = ComputeSnappedPosition(target, bestHit, targetRotation);
				float score = Vector3Ex.Distance2D(target.position, bestHit.point);
				SnapResult result = default(SnapResult);
				result.Valid = true;
				result.Score = score;
				result.Placement = new Construction.Placement(target)
				{
					position = vector,
					rotation = targetRotation
				};
				result.Label = "Wall";
				return result;
			}
			return SnapResult.Invalid;
		}
	}

	private bool TryFindBestSnappingHit(Construction.Target target, out RaycastHit bestHit)
	{
		bestHit = default(RaycastHit);
		using PooledList<Vector3> pooledList = Pool.Get<PooledList<Vector3>>();
		AddDirections(target, pooledList, rayAligned: false);
		Vector3 up = GetUp(target);
		Vector3 vector = target.position + up * 0.15f;
		using PooledList<RaycastHit> pooledList3 = Pool.Get<PooledList<RaycastHit>>();
		foreach (Vector3 item in pooledList)
		{
			Ray ray = new Ray(vector, item);
			using PooledList<RaycastHit> pooledList2 = Pool.Get<PooledList<RaycastHit>>();
			GamePhysics.TraceAll(ray, 0f, pooledList2, GetMaxDistance(), 136314880);
			if (pooledList2.Count <= 0)
			{
				continue;
			}
			foreach (RaycastHit item2 in pooledList2)
			{
				pooledList3.Add(item2);
			}
		}
		float num = float.MaxValue;
		foreach (RaycastHit item3 in pooledList3)
		{
			if (!(Vector3.Distance(item3.point, target.ray.origin) > staticConstruction.maxplaceDistance))
			{
				float num2 = Vector3Ex.Distance2D(vector, item3.point);
				if (num2 < num)
				{
					num = num2;
					bestHit = item3;
				}
			}
		}
		return Math.Abs(num - float.MaxValue) > Mathf.Epsilon;
	}

	private Quaternion ComputeSnappedRotation(Construction.Target target, RaycastHit bestHit)
	{
		Vector3 up = GetUp(target);
		Vector3 normal = bestHit.normal;
		Quaternion quaternion = ((normal == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(normal, up));
		Vector3 vector = -(bestHit.point - target.ray.origin).normalized;
		vector -= up * Vector3.Dot(vector, up);
		Quaternion quaternion2 = Quaternion.LookRotation(vector, up) * Quaternion.Euler(target.rotation);
		Quaternion quaternion3 = quaternion * Quaternion.Euler(target.rotation);
		Vector3 vector2 = quaternion3 * bestHit.normal;
		Vector3 vector3 = quaternion2 * bestHit.normal;
		if (Mathf.Abs(Vector3.Dot(vector3, vector2)) < 0.5f)
		{
			Quaternion quaternion4 = Quaternion.AngleAxis(Mathf.Round(Vector3.SignedAngle(vector2, vector3, up) / 90f) * 90f, up);
			quaternion3 *= quaternion4;
		}
		return quaternion3;
	}

	private Vector3 ComputeSnappedPosition(Construction.Target target, RaycastHit bestHit, Quaternion targetRotation)
	{
		Vector3 up = GetUp(target);
		Vector3 normal = bestHit.normal;
		Matrix4x4 matrix4x = Matrix4x4.TRS(target.position, targetRotation, staticEntity.transform.lossyScale);
		Vector3 vector = Vector3.Scale(matrix4x.inverse.MultiplyVector(normal), staticEntity.bounds.extents);
		Vector3 vector2 = matrix4x.MultiplyVector(vector);
		Vector3 lhs = targetRotation * staticEntity.bounds.center;
		lhs -= up * Vector3.Dot(lhs, up);
		float num = Vector3.Dot(lhs, normal);
		Vector3 vector3 = bestHit.point + normal * snappingPadding + vector2 - normal * num;
		vector3 += normal * generalPadding;
		if (bestHit.collider != null)
		{
			float buildingBlockPadding = GetBuildingBlockPadding(bestHit.collider.name, yPadding: false, bestHit.collider.transform, bestHit.normal);
			vector3 += normal * buildingBlockPadding;
		}
		float num2 = Vector3.Dot(target.position, up);
		float num3 = Vector3.Dot(vector3, up);
		return vector3 + up * (num2 - num3);
	}

	private SnapResult TryCornerSnap(Construction.Target target)
	{
		if (!allowSnappingToCorners)
		{
			return SnapResult.Invalid;
		}
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement.TryCornerSnap"))
		{
			if (TryFindCornerHits(target, out var hitA, out var hitB))
			{
				RaycastHit bestHit = ((Vector3Ex.Distance2D(target.position, hitA.point) < Vector3Ex.Distance2D(target.position, hitB.point)) ? hitA : hitB);
				Quaternion targetRotation = ComputeSnappedRotation(target, bestHit);
				Vector3 vector = ComputeCornerSnappedPosition(target, hitA, hitB, targetRotation);
				float a = Vector3Ex.Distance2D(target.position, hitA.point);
				float b = Vector3Ex.Distance2D(target.position, hitB.point);
				float num = Mathf.Min(a, b);
				num *= 0.7f;
				SnapResult result = default(SnapResult);
				result.Valid = true;
				result.Score = num;
				result.Placement = new Construction.Placement(target)
				{
					position = vector,
					rotation = targetRotation
				};
				result.Label = "Corner";
				return result;
			}
			return SnapResult.Invalid;
		}
	}

	private bool TryFindCornerHits(Construction.Target target, out RaycastHit hitA, out RaycastHit hitB)
	{
		hitA = default(RaycastHit);
		hitB = default(RaycastHit);
		using PooledList<Vector3> pooledList = Pool.Get<PooledList<Vector3>>();
		AddDirections(target, pooledList, rayAligned: false);
		Vector3 up = GetUp(target);
		Vector3 vector = target.position + up * 0.5f;
		float num = float.MaxValue;
		using PooledList<RaycastHit> pooledList3 = Pool.Get<PooledList<RaycastHit>>();
		foreach (Vector3 item in pooledList)
		{
			Ray ray = new Ray(vector, item);
			using PooledList<RaycastHit> pooledList2 = Pool.Get<PooledList<RaycastHit>>();
			GamePhysics.TraceAll(ray, 0f, pooledList2, GetMaxDistance(), 136315136);
			if (pooledList2.Count <= 0)
			{
				continue;
			}
			foreach (RaycastHit item2 in pooledList2)
			{
				pooledList3.Add(item2);
			}
		}
		for (int i = 0; i < pooledList3.Count; i++)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(pooledList3[i]);
			if (entity == null || entity.net == null)
			{
				continue;
			}
			for (int j = i + 1; j < pooledList3.Count; j++)
			{
				BaseEntity entity2 = RaycastHitEx.GetEntity(pooledList3[j]);
				if (!(entity2 == null) && entity2.net != null && !(entity.net.ID == entity2.net.ID))
				{
					Vector3 normal = pooledList3[i].normal;
					Vector3 normal2 = pooledList3[j].normal;
					float num2 = Mathf.Abs(Vector3.Dot(normal.normalized, normal2.normalized));
					float num3 = Vector3Ex.Distance2D(vector, pooledList3[i].point);
					num3 += Vector3Ex.Distance2D(vector, pooledList3[j].point);
					if (num2 < 0.3f && num3 < num)
					{
						hitA = pooledList3[i];
						hitB = pooledList3[j];
						num = num3;
					}
				}
			}
		}
		return Math.Abs(num - float.MaxValue) > Mathf.Epsilon;
	}

	private Vector3 GetPlaneIntersectionPoint(Vector3 normal1, Vector3 point1, Vector3 normal2, Vector3 point2)
	{
		Vector3 vector = Vector3.Cross(normal1, normal2);
		if (vector.sqrMagnitude < Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		float num = Vector3.Dot(normal1, point1);
		float num2 = Vector3.Dot(normal2, point2);
		Vector3 vector2 = Vector3.Cross(normal2, vector) * num + Vector3.Cross(vector, normal1) * num2;
		float num3 = vector.magnitude * vector.magnitude;
		return vector2 / num3;
	}

	private Vector3 ComputeCornerSnappedPosition(Construction.Target target, RaycastHit hitA, RaycastHit hitB, Quaternion targetRotation)
	{
		Vector3 normal = hitA.normal;
		Vector3 normal2 = hitB.normal;
		Vector3 up = GetUp(target);
		normal -= up * Vector3.Dot(normal, up);
		normal2 -= up * Vector3.Dot(normal2, up);
		Vector3 planeIntersectionPoint = GetPlaneIntersectionPoint(normal, hitA.point, normal2, hitB.point);
		float num = Vector3.Dot(target.position, up);
		float num2 = Vector3.Dot(planeIntersectionPoint, up);
		Vector3 vector = planeIntersectionPoint + up * (num - num2);
		Vector3 normalized = (normal + normal2).normalized;
		Vector3 vector2 = targetRotation * staticEntity.bounds.center;
		Matrix4x4 matrix4x = Matrix4x4.TRS(vector + vector2, targetRotation, staticEntity.transform.lossyScale);
		Vector3 vector3 = Vector3.Scale(matrix4x.inverse.MultiplyVector(normal), staticEntity.bounds.extents);
		Vector3 vector4 = Vector3.Scale(matrix4x.inverse.MultiplyVector(normal2), staticEntity.bounds.extents);
		Vector3 vector5 = matrix4x.MultiplyVector(vector3 + vector4);
		Vector3 vector6 = vector + vector5 + normalized * cornerPadding + normalized * generalPadding;
		float num3 = 0f;
		if (hitA.collider != null)
		{
			num3 = Mathf.Max(num3, GetBuildingBlockPadding(hitA.collider.name, yPadding: false, hitA.collider.transform, hitA.normal));
		}
		if (hitB.collider != null)
		{
			num3 = Mathf.Max(num3, GetBuildingBlockPadding(hitB.collider.name, yPadding: false, hitB.collider.transform, hitB.normal));
		}
		Vector3 vector7 = vector6 + normalized * num3;
		float num4 = Vector3.Dot(target.position, up);
		float num5 = Vector3.Dot(vector7, up);
		return vector7 + up * (num4 - num5);
	}

	private SnapResult TryMatchingDeployableSnap(Construction.Target target)
	{
		if (!allowSnappingToSameDeployable)
		{
			return SnapResult.Invalid;
		}
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement.TryMatchingDeployableSnap"))
		{
			if (TryFindMatchingDeployables(target, out var bestHit))
			{
				Quaternion targetRotation = bestHit.transform.rotation;
				Vector3 vector = ComputeSnappedMatchingDeployablePosition(target, bestHit, targetRotation);
				float num = Vector3Ex.Distance2D(target.position, bestHit.point);
				if (target.entity.prefabID == staticEntity.prefabID)
				{
					num *= 0.9f;
				}
				SnapResult result = default(SnapResult);
				result.Valid = true;
				result.Score = num;
				result.Placement = new Construction.Placement(target)
				{
					position = vector,
					rotation = targetRotation
				};
				result.Label = "Deployable";
				return result;
			}
			return SnapResult.Invalid;
		}
	}

	private Vector3 ComputeSnappedMatchingDeployablePosition(Construction.Target target, RaycastHit bestHit, Quaternion targetRotation)
	{
		BaseEntity entity = RaycastHitEx.GetEntity(bestHit);
		if (entity == null)
		{
			return target.position;
		}
		OBB oBB = entity.WorldSpaceBounds();
		Vector3 lhs = bestHit.point - oBB.position;
		Vector3 up = GetUp(target);
		lhs -= up * Vector3.Dot(lhs, up);
		Vector3[] obj = new Vector3[4]
		{
			oBB.right,
			-oBB.right,
			oBB.forward,
			-oBB.forward
		};
		Vector3 vector = oBB.forward;
		float num = -1f;
		Vector3[] array = obj;
		foreach (Vector3 vector2 in array)
		{
			float num2 = Vector3.Dot(lhs, vector2);
			if (num2 > num)
			{
				num = num2;
				vector = vector2;
			}
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(target.position, targetRotation, staticEntity.transform.lossyScale);
		Vector3 vector3 = Vector3.Scale(matrix4x.inverse.MultiplyVector(vector.normalized), staticEntity.bounds.size);
		Vector3 vector4 = matrix4x.MultiplyVector(vector3);
		Vector3 lhs2 = targetRotation * staticEntity.bounds.center;
		lhs2 -= up * Vector3.Dot(lhs2, up);
		float num3 = Vector3.Dot(lhs2, vector);
		return entity.transform.position + vector * sameDeployablePadding + vector4 - vector * num3 + vector * generalPadding;
	}

	private bool TryFindMatchingDeployables(Construction.Target target, out RaycastHit bestHit)
	{
		bestHit = default(RaycastHit);
		using PooledList<Vector3> pooledList = Pool.Get<PooledList<Vector3>>();
		AddDirections(target, pooledList, rayAligned: false);
		Vector3 up = GetUp(target);
		Vector3 vector = target.position + up * 0.05f;
		using PooledList<RaycastHit> pooledList3 = Pool.Get<PooledList<RaycastHit>>();
		foreach (Vector3 item in pooledList)
		{
			Ray ray = new Ray(vector, item);
			using PooledList<RaycastHit> pooledList2 = Pool.Get<PooledList<RaycastHit>>();
			GamePhysics.TraceAll(ray, 0f, pooledList2, GetMaxDistance() * 1.5f, 256);
			if (pooledList2.Count <= 0)
			{
				continue;
			}
			foreach (RaycastHit item2 in pooledList2)
			{
				if (!(item2.collider == null))
				{
					BaseEntity entity = RaycastHitEx.GetEntity(item2);
					if (!(entity == null) && ShouldDeployableSnap(staticEntity, entity))
					{
						pooledList3.Add(item2);
					}
				}
			}
		}
		float num = float.MaxValue;
		foreach (RaycastHit item3 in pooledList3)
		{
			if (!(Vector3.Distance(item3.point, target.ray.origin) > staticConstruction.maxplaceDistance))
			{
				float num2 = Vector3Ex.Distance2D(vector, item3.point);
				if (num2 < num)
				{
					num = num2;
					bestHit = item3;
				}
			}
		}
		return Math.Abs(num - float.MaxValue) > Mathf.Epsilon;
	}

	private Vector3 GetUp(Construction.Target target)
	{
		if (target.entity != null)
		{
			return target.entity.transform.up;
		}
		return Vector3.up;
	}

	private bool ShouldDeployableSnap(BaseEntity ent, BaseEntity other)
	{
		if (ent is BaseCombatEntity { pickup: { enabled: not false } pickup } && other is BaseCombatEntity { pickup: { enabled: not false } pickup2 })
		{
			if (pickup.itemTarget == null || pickup2.itemTarget == null)
			{
				return false;
			}
			ItemDefinition obj = ((pickup.itemTarget.isRedirectOf == null) ? pickup.itemTarget : pickup.itemTarget.isRedirectOf);
			ItemDefinition itemDefinition = ((pickup2.itemTarget.isRedirectOf == null) ? pickup2.itemTarget : pickup2.itemTarget.isRedirectOf);
			return obj == itemDefinition;
		}
		return ent.prefabID == other.prefabID;
	}
}
