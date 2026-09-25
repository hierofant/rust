using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class CrashSpotSearch
{
	public enum Status
	{
		InProgress,
		Found,
		Exhausted
	}

	private enum Blocker
	{
		None,
		OutOfBounds,
		Topology,
		Water,
		Uneven,
		Obstruction,
		SafeZone,
		PhysicalUneven
	}

	private readonly Vector3 center;

	private readonly float radius;

	private readonly float clearanceRadius;

	private readonly Vector3 preferredSpot;

	private readonly bool hasPreferredSpot;

	private readonly bool logResult;

	private readonly int n;

	private readonly int[] order;

	private readonly List<Vector3> tcPositions = new List<Vector3>();

	private bool initialized;

	private int nextSample;

	private int bounds;

	private int topo;

	private int water;

	private int uneven;

	private int obstruction;

	private int safezone;

	private int physicalUneven;

	private const float GoldenAngle = 2.3999631f;

	private const int FootprintSampleCount = 13;

	private const int PhysicalFootprintMask = 8454145;

	private const float FootprintRayHeight = 50f;

	private const int TopologyRejectMask = 6374530;

	private static readonly int ClearanceMask = 689963264;

	private const float SeaLevelClearance = 0.5f;

	public int TcsInArea { get; private set; } = -1;


	public int SamplesTested => nextSample;

	public Vector3 Result { get; private set; }

	private bool ShouldLog
	{
		get
		{
			if (logResult)
			{
				return Satellite.debug;
			}
			return false;
		}
	}

	public CrashSpotSearch(Vector3 center, float radius, float clearanceRadius, Vector3 preferredSpot = default(Vector3), bool hasPreferredSpot = false, bool logResult = true)
	{
		this.center = center;
		this.radius = radius;
		this.clearanceRadius = clearanceRadius;
		this.preferredSpot = preferredSpot;
		this.hasPreferredSpot = hasPreferredSpot;
		this.logResult = logResult;
		n = ComputeSampleCount(radius, clearanceRadius);
		order = new int[n];
		Result = center;
	}

	private static int ComputeSampleCount(float radius, float clearanceRadius)
	{
		float num = ((clearanceRadius > 0f) ? (radius / clearanceRadius) : 1f);
		int value = Mathf.CeilToInt(Satellite.targeting_coverage_factor * num * num);
		int min = Mathf.Max(1, Satellite.targeting_min_samples);
		return Mathf.Clamp(value, min, 256);
	}

	public bool TryReusePreferred(out Vector3 result)
	{
		result = preferredSpot;
		if (!Satellite.reuse_last_crash_spot || !hasPreferredSpot)
		{
			return false;
		}
		float num = preferredSpot.x - center.x;
		float num2 = preferredSpot.z - center.z;
		int num3;
		if (num * num + num2 * num2 <= radius * radius)
		{
			num3 = ((EvaluateCrashSpot(preferredSpot, clearanceRadius) == Blocker.None) ? 1 : 0);
			if (num3 != 0)
			{
				Result = preferredSpot;
			}
		}
		else
		{
			num3 = 0;
		}
		return (byte)num3 != 0;
	}

	public Status Step()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (nextSample >= n)
		{
			return Status.Exhausted;
		}
		int index = order[nextSample];
		nextSample++;
		Vector2 vector = SampleDiscPointXZ(center, radius, index, n);
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.y);
		vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
		switch (EvaluateCrashSpot(vector2, clearanceRadius))
		{
		case Blocker.None:
			Result = vector2;
			if (ShouldLog)
			{
				Debug.Log($"[Satellite] Crash target {vector2} found after testing {nextSample}/{n} samples within " + $"{radius:F0}m of {center} (clearance {clearanceRadius:F0}m). Rejected so far: {BlockerTally()}");
			}
			return Status.Found;
		case Blocker.OutOfBounds:
			bounds++;
			break;
		case Blocker.Topology:
			topo++;
			break;
		case Blocker.Water:
			water++;
			break;
		case Blocker.Uneven:
			uneven++;
			break;
		case Blocker.Obstruction:
			obstruction++;
			break;
		case Blocker.SafeZone:
			safezone++;
			break;
		case Blocker.PhysicalUneven:
			physicalUneven++;
			break;
		}
		if (nextSample >= n)
		{
			Result = center;
			if (ShouldLog)
			{
				Debug.LogWarning($"[Satellite] No clear crash target within {radius:F0}m of {center} after all {n} samples " + $"(clearance {clearanceRadius:F0}m). Blockers: {BlockerTally()}");
			}
			return Status.Exhausted;
		}
		return Status.InProgress;
	}

	private string BlockerTally()
	{
		return $"topology={topo}, water={water}, uneven={uneven}, obstruction={obstruction}, " + $"safezone={safezone}, bounds={bounds}, physicalUneven={physicalUneven}.";
	}

	private void Initialize()
	{
		initialized = true;
		for (int i = 0; i < n; i++)
		{
			order[i] = i;
		}
		for (int num = n - 1; num > 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num + 1);
			ref int reference = ref order[num];
			ref int reference2 = ref order[num2];
			int num3 = order[num2];
			int num4 = order[num];
			reference = num3;
			reference2 = num4;
		}
		if (Satellite.obstruction_tc_reorder)
		{
			PrefetchObstructionTcs();
			TcsInArea = tcPositions.Count;
			if (tcPositions.Count > 0)
			{
				ReorderSamplesByTc();
			}
		}
	}

	private void PrefetchObstructionTcs()
	{
		tcPositions.Clear();
		using PooledList<BuildingPrivlidge> pooledList = Facepunch.Pool.Get<PooledList<BuildingPrivlidge>>();
		BaseEntity.Query.Server.GetInSphere(center, radius + clearanceRadius, pooledList);
		for (int i = 0; i < pooledList.Count; i++)
		{
			BuildingPrivlidge buildingPrivlidge = pooledList[i];
			if (IsRealToolCupboard(buildingPrivlidge))
			{
				tcPositions.Add(buildingPrivlidge.transform.position);
			}
		}
	}

	private void ReorderSamplesByTc()
	{
		Span<int> span = stackalloc int[n];
		int num = 0;
		int num2 = n;
		float sqrRadius = clearanceRadius * clearanceRadius;
		for (int i = 0; i < n; i++)
		{
			int num3 = order[i];
			Vector2 vector = SampleDiscPointXZ(center, radius, num3, n);
			if (SampleNearTc(vector.x, vector.y, sqrRadius))
			{
				span[--num2] = num3;
			}
			else
			{
				span[num++] = num3;
			}
		}
		span.CopyTo(order);
	}

	private bool SampleNearTc(float x, float z, float sqrRadius)
	{
		for (int i = 0; i < tcPositions.Count; i++)
		{
			float num = tcPositions[i].x - x;
			float num2 = tcPositions[i].z - z;
			if (num * num + num2 * num2 < sqrRadius)
			{
				return true;
			}
		}
		return false;
	}

	private static Vector2 SampleDiscPointXZ(Vector3 center, float radius, int index, int n)
	{
		float num = radius * Mathf.Sqrt((float)index / (float)n);
		float f = (float)index * 2.3999631f;
		return new Vector2(center.x + Mathf.Cos(f) * num, center.z + Mathf.Sin(f) * num);
	}

	public static bool IsDryLandCandidate(Vector3 pos)
	{
		if (!IsOutOfBounds(pos) && !IsBlockedByTopology(pos))
		{
			return !IsInWater(pos);
		}
		return false;
	}

	public static bool IsSpotOk(Vector3 pos, float clearanceRadius)
	{
		return EvaluateCrashSpot(pos, clearanceRadius) == Blocker.None;
	}

	private static Blocker EvaluateCrashSpot(Vector3 pos, float clearanceRadius)
	{
		if (IsOutOfBounds(pos))
		{
			return Blocker.OutOfBounds;
		}
		if (IsBlockedByTopology(pos))
		{
			return Blocker.Topology;
		}
		if (IsInWater(pos))
		{
			return Blocker.Water;
		}
		if (IsTerrainTooUneven(pos))
		{
			return Blocker.Uneven;
		}
		if (IsInSafeZone(pos, clearanceRadius))
		{
			return Blocker.SafeZone;
		}
		if (IsObstructed(pos, clearanceRadius))
		{
			return Blocker.Obstruction;
		}
		if (Satellite.site_check_physical_geometry && IsPhysicalSurfaceTooUneven(pos))
		{
			return Blocker.PhysicalUneven;
		}
		return Blocker.None;
	}

	private static void GetFootprintOffsetsXZ(float radius, Span<Vector2> offsets)
	{
		offsets[0] = Vector2.zero;
		int num = 1;
		for (int i = 0; i < 4; i++)
		{
			float f = ((float)i * 90f + 45f) * (MathF.PI / 180f);
			offsets[num++] = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * (radius * 0.5f);
		}
		for (int j = 0; j < 8; j++)
		{
			float f2 = (float)j * 45f * (MathF.PI / 180f);
			offsets[num++] = new Vector2(Mathf.Cos(f2), Mathf.Sin(f2)) * radius;
		}
	}

	private static void FitPlane(ReadOnlySpan<Vector3> samples, ReadOnlySpan<Vector3> sampleNormals, out Vector3 centroid, out Vector3 normal, out float maxDeviation)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		for (int i = 0; i < samples.Length; i++)
		{
			zero += samples[i];
			zero2 += sampleNormals[i];
		}
		centroid = zero / samples.Length;
		normal = ((zero2.sqrMagnitude > 0.0001f) ? zero2.normalized : Vector3.up);
		maxDeviation = 0f;
		for (int j = 0; j < samples.Length; j++)
		{
			maxDeviation = Mathf.Max(maxDeviation, Mathf.Abs(Vector3.Dot(samples[j] - centroid, normal)));
		}
	}

	public static bool SampleFootprintPlane(Vector3 center, float radius, out Vector3 centroid, out Vector3 normal, out float maxDeviation)
	{
		centroid = center;
		normal = Vector3.up;
		maxDeviation = 0f;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if (heightMap == null)
		{
			return false;
		}
		Span<Vector2> offsets = stackalloc Vector2[13];
		GetFootprintOffsetsXZ(radius, offsets);
		Span<Vector3> span = stackalloc Vector3[13];
		Span<Vector3> span2 = stackalloc Vector3[13];
		for (int i = 0; i < 13; i++)
		{
			Vector3 vector = center + new Vector3(offsets[i].x, 0f, offsets[i].y);
			vector.y = heightMap.GetHeight(vector);
			span[i] = vector;
			span2[i] = heightMap.GetNormal(vector);
		}
		FitPlane(span, span2, out centroid, out normal, out maxDeviation);
		return true;
	}

	private static bool ExceedsSiteShapeLimits(Vector3 normal, float deviation)
	{
		if (Vector3.Angle(normal, Vector3.up) > Satellite.site_max_slope)
		{
			return true;
		}
		if (deviation > Satellite.site_max_unevenness)
		{
			return true;
		}
		return false;
	}

	private static bool IsTerrainTooUneven(Vector3 pos)
	{
		if (!SampleFootprintPlane(pos, Satellite.site_footprint_radius, out var _, out var normal, out var maxDeviation))
		{
			return false;
		}
		return ExceedsSiteShapeLimits(normal, maxDeviation);
	}

	private static bool IsPhysicalSurfaceTooUneven(Vector3 pos)
	{
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if (heightMap == null)
		{
			return false;
		}
		float site_footprint_radius = Satellite.site_footprint_radius;
		Span<Vector2> offsets = stackalloc Vector2[13];
		GetFootprintOffsetsXZ(site_footprint_radius, offsets);
		Span<Vector3> span = stackalloc Vector3[13];
		Span<Vector3> span2 = stackalloc Vector3[13];
		for (int i = 0; i < 13; i++)
		{
			Vector3 vector = pos + new Vector3(offsets[i].x, 0f, offsets[i].y);
			float height = heightMap.GetHeight(vector);
			if (UnityEngine.Physics.Raycast(new Vector3(vector.x, height + 50f, vector.z), Vector3.down, out var hitInfo, 100f, 8454145, QueryTriggerInteraction.Ignore))
			{
				span[i] = hitInfo.point;
				span2[i] = hitInfo.normal;
			}
			else
			{
				vector.y = height;
				span[i] = vector;
				span2[i] = heightMap.GetNormal(vector);
			}
		}
		FitPlane(span, span2, out var _, out var normal, out var maxDeviation);
		return ExceedsSiteShapeLimits(normal, maxDeviation);
	}

	public static bool IsOutOfBounds(Vector3 pos)
	{
		float num = TerrainMeta.Size.x * 0.5f;
		if (!(Mathf.Abs(pos.x) > num))
		{
			return Mathf.Abs(pos.z) > num;
		}
		return true;
	}

	private static bool IsBlockedByTopology(Vector3 pos)
	{
		if (TerrainMeta.TopologyMap != null)
		{
			return (TerrainMeta.TopologyMap.GetTopology(pos) & 0x614482) != 0;
		}
		return false;
	}

	public static bool IsInWater(Vector3 pos)
	{
		bool flag = WaterLevel.Test(pos, waves: false, volumes: true);
		if (!flag && TerrainMeta.HeightMap != null)
		{
			flag = TerrainMeta.HeightMap.GetHeight(pos) < WaterSystem.OceanLevel - 0.5f;
		}
		return flag;
	}

	private static bool IsInSafeZone(Vector3 pos, float clearanceRadius)
	{
		bool result = false;
		List<TriggerSafeZone> allSafeZones = TriggerSafeZone.allSafeZones;
		for (int i = 0; i < allSafeZones.Count; i++)
		{
			TriggerSafeZone triggerSafeZone = allSafeZones[i];
			if (!(triggerSafeZone == null) && !(triggerSafeZone.triggerCollider == null) && (triggerSafeZone.triggerCollider.ClosestPoint(pos) - pos).sqrMagnitude < clearanceRadius * clearanceRadius && triggerSafeZone.PassesHeightChecks(pos))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private static bool IsObstructed(Vector3 pos, float clearanceRadius)
	{
		return UnityEngine.Physics.CheckSphere(pos, clearanceRadius, ClearanceMask, QueryTriggerInteraction.Collide);
	}

	private static bool IsRealToolCupboard(BuildingPrivlidge tc)
	{
		if (tc != null && tc.isServer)
		{
			return !tc.IsInvisibleAuth;
		}
		return false;
	}

	public static int CountToolCupboards(Vector3 center, float radius)
	{
		using PooledList<BuildingPrivlidge> pooledList = Facepunch.Pool.Get<PooledList<BuildingPrivlidge>>();
		BaseEntity.Query.Server.GetInSphere(center, radius, pooledList);
		int num = 0;
		for (int i = 0; i < pooledList.Count; i++)
		{
			if (IsRealToolCupboard(pooledList[i]))
			{
				num++;
			}
		}
		return num;
	}
}
