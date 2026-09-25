using System;
using UnityEngine;

public class TerrainFootprint : PrefabAttribute
{
	private delegate void BandTexel(int x, int z, float lift, bool inside);

	[HideInInspector]
	public Vector3[] Ring = Array.Empty<Vector3>();

	[HideInInspector]
	public Vector3 Center = Vector3.zero;

	[HideInInspector]
	public int[] RunStarts = Array.Empty<int>();

	[HideInInspector]
	public bool[] RunClosed = Array.Empty<bool>();

	[Tooltip("Where the fill stops, relative to the ring. 0 lands the ground exactly on the line, which is the point of drawing the line where you want the ground: the fill can then never come up above it. Positive buries the base that much deeper, negative leaves it standing that much prouder.\n\nMetres, taken literally - not multiplied by the prefab's scale or widened by tilt, so what you type is what you get on every instance.")]
	[Header("Seating")]
	public float FillOffset;

	[Header("Fill")]
	[Tooltip("Safety ceiling on how far any one point of ground may be raised, in metres. Keep it ABOVE the deepest gutter you want bridged - the plane the ring lies in is already the natural ceiling, so this is a backstop rather than a tuning dial.\n\nSet it too low and it does real damage: the shallow ground either side of a gutter reaches the plane while the gutter itself stops short, leaving a step between them. A 2.5 m gutter under a footprint clamped at 1.5 m leaves a 1.65 m wall; at 4 m it comes out flush.")]
	public float MaxFill = 4f;

	[Tooltip("Reject the placement outright when the ground has to come up further than this to meet the rock, in metres. 0 disables rejection, which is the default: the footprint then only ever fills, and cannot change which rocks the generator spawns.")]
	public float RejectAboveGap;

	[Tooltip("How far inward from the ring the fill reaches when FillInterior is off. The fill eases to nothing across this distance rather than stopping dead, so the band still meets untouched ground smoothly.")]
	public float RimWidth = 2f;

	[Tooltip("Falloff distance outside the ring, so filled ground meets untouched terrain without a lip.")]
	public float Feather = 3f;

	[Tooltip("Strength of the raise. Below 1 the fill only partially closes the gap.")]
	[Range(0f, 1f)]
	public float Opacity = 1f;

	[Tooltip("Fill the whole interior of a closed ring, not just the band at its edge. On is almost always right: a gutter that runs under the rock has to be filled all the way across, or its two banks get raised and the trench between them is left behind as a pair of shoulders. Turn it off only for arches and overhangs, whose passable space this would seal.")]
	public bool FillInterior = true;

	[Header("Tilt")]
	[Tooltip("Above this much tilt the footprint does nothing at all - no gap test, no fill - and placement behaves exactly as it did before. A rock aligned to steep terrain swings its body out past the captured ring by roughly its height times sin(tilt), so the silhouette stops describing where it really meets the ground.\n\nWhat that costs above the gate is coverage, not correctness - the fill simply does not reach ground the rock overhangs - so set it generously. Measured across 278 placed rocks on a 4.5k map, tilt ran 2-34 degrees with a median of 16, so 25 was standing down on the steepest 9% for no benefit. Steeper biomes will skew higher than that sample.")]
	public float MaxTiltDegrees = 35f;

	[NonSerialized]
	private Vector3[] rootRing;

	[NonSerialized]
	private Vector3 rootCenter;

	[NonSerialized]
	private bool rootRingValid;

	public bool HasRing
	{
		get
		{
			if (Ring != null)
			{
				return Ring.Length >= 2;
			}
			return false;
		}
	}

	public int RunCount
	{
		get
		{
			if (RunStarts == null || RunStarts.Length == 0)
			{
				return 1;
			}
			return RunStarts.Length;
		}
	}

	public float GetTilt(Quaternion rotation)
	{
		return Vector3.Angle(Vector3.up, rotation * Vector3.up);
	}

	public bool IsActive(Quaternion rotation)
	{
		if (HasRing)
		{
			return GetTilt(rotation) <= MaxTiltDegrees;
		}
		return false;
	}

	public void InvalidateRootRing()
	{
		rootRingValid = false;
	}

	public void GetRun(int run, out int start, out int end, out bool closed)
	{
		int num = ((Ring != null) ? Ring.Length : 0);
		if (RunStarts == null || RunStarts.Length == 0)
		{
			start = 0;
			end = num;
			closed = true;
		}
		else
		{
			start = Mathf.Clamp(RunStarts[run], 0, num);
			end = ((run + 1 < RunStarts.Length) ? Mathf.Clamp(RunStarts[run + 1], start, num) : num);
			closed = RunClosed != null && run < RunClosed.Length && RunClosed[run];
		}
	}

	private void EnsureRootRing()
	{
		if (!rootRingValid || rootRing == null || rootRing.Length != Ring.Length)
		{
			if (rootRing == null || rootRing.Length != Ring.Length)
			{
				rootRing = new Vector3[Ring.Length];
			}
			for (int i = 0; i < Ring.Length; i++)
			{
				rootRing[i] = worldPosition + worldRotation * Ring[i];
			}
			rootCenter = worldPosition + worldRotation * Center;
			rootRingValid = true;
		}
	}

	public float MeasureGap(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		float worst = 0f;
		ForEachBandTexel(pos, rot, scale, delegate(int x, int z, float lift, bool inside)
		{
			if (inside && lift > worst)
			{
				worst = lift;
			}
		});
		return worst;
	}

	public void Fill(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		if (Opacity <= 0f)
		{
			return;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if (!heightMap)
		{
			return;
		}
		int minX = int.MaxValue;
		int maxX = int.MinValue;
		int minZ = int.MaxValue;
		int maxZ = int.MinValue;
		ForEachBandTexel(pos, rot, scale, delegate(int x, int z, float lift, bool inside)
		{
			if (!(lift <= 0f))
			{
				if (x < minX)
				{
					minX = x;
				}
				if (x > maxX)
				{
					maxX = x;
				}
				if (z < minZ)
				{
					minZ = z;
				}
				if (z > maxZ)
				{
					maxZ = z;
				}
			}
		});
		if (minX > maxX)
		{
			return;
		}
		int width = maxX - minX + 1;
		float[] lifts = new float[width * (maxZ - minZ + 1)];
		ForEachBandTexel(pos, rot, scale, delegate(int x, int z, float lift, bool inside)
		{
			if (!(lift <= 0f))
			{
				int num2 = (z - minZ) * width + (x - minX);
				if (lift > lifts[num2])
				{
					lifts[num2] = lift;
				}
			}
		});
		for (int i = minZ; i <= maxZ; i++)
		{
			for (int j = minX; j <= maxX; j++)
			{
				float num = lifts[(i - minZ) * width + (j - minX)];
				if (!(num <= 0f))
				{
					float y = heightMap.GetHeight(j, i) + Mathf.Min(num * Opacity, MaxFill);
					heightMap.RaiseHeight(j, i, TerrainMeta.NormalizeY(y), 1f);
				}
			}
		}
	}

	private void ForEachBandTexel(Vector3 pos, Quaternion rot, Vector3 scale, BandTexel action)
	{
		if (!IsActive(rot))
		{
			return;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		if (!heightMap)
		{
			return;
		}
		EnsureRootRing();
		float fillOffset = FillOffset;
		float rim = Mathf.Max(RimWidth, 0.01f);
		float feather = Mathf.Max(Feather, 0.01f);
		Vector3 normal = rot * Vector3.up;
		if (Mathf.Abs(normal.y) < 0.0001f)
		{
			return;
		}
		Vector3 center = pos + rot * Vector3.Scale(rootCenter, scale);
		Vector3[] array = new Vector3[Ring.Length];
		for (int i = 0; i < Ring.Length; i++)
		{
			array[i] = pos + rot * Vector3.Scale(rootRing[i], scale);
		}
		for (int j = 0; j < RunCount; j++)
		{
			GetRun(j, out var start, out var end, out var closed);
			int num = end - start;
			if (num < 2)
			{
				continue;
			}
			bool flag = FillInterior && closed && num >= 3;
			int num2 = (closed ? num : (num - 1));
			for (int k = 0; k < num2; k++)
			{
				Vector3 a = array[start + k];
				Vector3 b = array[start + (k + 1) % num];
				SweepSegment(heightMap, a, b, center, normal, fillOffset, rim, feather, flag, action);
			}
			if (flag)
			{
				for (int l = 0; l < num; l++)
				{
					Vector3 a2 = array[start + l];
					Vector3 b2 = array[start + (l + 1) % num];
					FanTriangle(heightMap, center, a2, b2, normal, fillOffset, action);
				}
			}
		}
	}

	private static float PlaneHeightAt(Vector3 normal, Vector3 center, float x, float z)
	{
		return center.y - (normal.x * (x - center.x) + normal.z * (z - center.z)) / normal.y;
	}

	private static void SweepSegment(TerrainHeightMap heightmap, Vector3 a, Vector3 b, Vector3 center, Vector3 normal, float offset, float rim, float feather, bool interiorFilled, BandTexel action)
	{
		Vector3 inwardA = Inward(a, center);
		Vector3 inwardB = Inward(b, center);
		Vector3 v = a - inwardA * feather;
		Vector3 v2 = b - inwardB * feather;
		Vector3 v3 = a + inwardA * rim;
		Vector3 v4 = b + inwardB * rim;
		Vector2 ab = new Vector2(b.x - a.x, b.z - a.z);
		float abSqr = ab.sqrMagnitude;
		ForEachQuad(heightmap, v, v2, v3, v4, delegate(int x, int z)
		{
			float num = TerrainMeta.DenormalizeX(heightmap.Coordinate(x));
			float num2 = TerrainMeta.DenormalizeZ(heightmap.Coordinate(z));
			float t = ((abSqr > 1E-06f) ? Mathf.Clamp01(Vector2.Dot(new Vector2(num - a.x, num2 - a.z), ab) / abSqr) : 0f);
			Vector3 vector = Vector3.Lerp(a, b, t);
			Vector2 lhs = new Vector2(num - vector.x, num2 - vector.z);
			float magnitude = lhs.magnitude;
			Vector3 vector2 = Vector3.Lerp(inwardA, inwardB, t);
			bool flag = Vector2.Dot(lhs, new Vector2(vector2.x, vector2.z)) >= 0f;
			float num3 = ((!flag) ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - magnitude / feather)) : (interiorFilled ? 1f : Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(magnitude / rim))));
			if (!(num3 <= 0f))
			{
				float num4 = (PlaneHeightAt(normal, center, num, num2) + offset - heightmap.GetHeight(x, z)) * num3;
				if (!(num4 <= 0f))
				{
					action(x, z, num4, flag);
				}
			}
		});
	}

	private static void FanTriangle(TerrainHeightMap heightmap, Vector3 center, Vector3 a, Vector3 b, Vector3 normal, float offset, BandTexel action)
	{
		ForEachTriangle(heightmap, center, a, b, delegate(int x, int z)
		{
			float x2 = TerrainMeta.DenormalizeX(heightmap.Coordinate(x));
			float z2 = TerrainMeta.DenormalizeZ(heightmap.Coordinate(z));
			float num = PlaneHeightAt(normal, center, x2, z2) + offset - heightmap.GetHeight(x, z);
			if (!(num <= 0f))
			{
				action(x, z, num, inside: true);
			}
		});
	}

	private static void ForEachQuad(TerrainHeightMap heightmap, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Action<int, int> action)
	{
		if (SignedArea(v0, v1, v2) < 0f)
		{
			heightmap.ForEach(v0, v2, v1, v3, action);
		}
		else
		{
			heightmap.ForEach(v0, v1, v2, v3, action);
		}
	}

	private static void ForEachTriangle(TerrainHeightMap heightmap, Vector3 v0, Vector3 v1, Vector3 v2, Action<int, int> action)
	{
		if (SignedArea(v0, v1, v2) < 0f)
		{
			heightmap.ForEach(v0, v2, v1, action);
		}
		else
		{
			heightmap.ForEach(v0, v1, v2, action);
		}
	}

	private static float SignedArea(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
	}

	private static Vector3 Inward(Vector3 point, Vector3 center)
	{
		Vector3 vector = center - point;
		vector.y = 0f;
		if (!(vector.sqrMagnitude > 1E-06f))
		{
			return Vector3.zero;
		}
		return vector.normalized;
	}

	protected override Type GetIndexedType()
	{
		return typeof(TerrainFootprint);
	}
}
