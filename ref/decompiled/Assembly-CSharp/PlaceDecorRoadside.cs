using System.Collections.Generic;
using UnityEngine;

public class PlaceDecorRoadside : ProceduralComponent
{
	public enum RoadMode
	{
		SideRoadOrRingRoad,
		SideRoad,
		RingRoad,
		SideRoadOrDesireTrail,
		DesireTrail,
		AnyRoadOrTrail
	}

	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public TerrainAnchorMode AnchorMode;

	[Tooltip("Which roads to walk - hierarchy 0 is the ring road, 1 side roads, 2 dirt trails")]
	public RoadMode RoadType;

	[Tooltip("Target metres between placements along a road")]
	public float Distance = 100f;

	[Tooltip("Metres from the road edge out to the prefab pivot")]
	public float SideOffset = 5f;

	[Tooltip("Ground steeper than this is rejected")]
	public float MaxSlope = 30f;

	[Tooltip("Metres the candidate position is randomly jittered each placement attempt")]
	public float Dithering;

	[Min(1f)]
	[Tooltip("Dithered positions to try per side before giving up on this road step")]
	public int PlacementAttempts = 1;

	private static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	protected List<Vector3> placedPositions = new List<Vector3>();

	private int rejectedFilter;

	private int rejectedAnchors;

	private int rejectedSlope;

	private int rejectedWater;

	private int rejectedLocation;

	public override void Process(uint seed)
	{
		if (World.Networked)
		{
			World.Spawn("Decor", "assets/bundled/prefabs/autospawn/" + ResourceFolder + "/");
		}
		else
		{
			if (!ShouldPlace() || TerrainMeta.Path == null || TerrainMeta.Path.Roads == null)
			{
				return;
			}
			Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + ResourceFolder);
			if (array != null && array.Length != 0)
			{
				placedPositions.Clear();
				rejectedFilter = (rejectedAnchors = (rejectedSlope = (rejectedWater = (rejectedLocation = 0))));
				float num = 0f;
				{
					foreach (PathList road in TerrainMeta.Path.Roads)
					{
						if (IsValidRoad(road))
						{
							num += road.Path.Length;
							SpawnAlongRoad(ref seed, road, array);
						}
					}
					return;
				}
			}
			Debug.LogError("[" + GetType().Name + "] Empty decor folder: " + ResourceFolder);
		}
	}

	protected virtual bool ShouldPlace()
	{
		return true;
	}

	protected virtual bool IsValidLocation(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		return true;
	}

	private bool IsValidRoad(PathList road)
	{
		switch (RoadType)
		{
		case RoadMode.SideRoadOrRingRoad:
			if (road.Hierarchy != 0)
			{
				return road.Hierarchy == 1;
			}
			return true;
		case RoadMode.SideRoad:
			return road.Hierarchy == 1;
		case RoadMode.RingRoad:
			return road.Hierarchy == 0;
		case RoadMode.SideRoadOrDesireTrail:
			if (road.Hierarchy != 1)
			{
				return road.Hierarchy == 2;
			}
			return true;
		case RoadMode.DesireTrail:
			return road.Hierarchy == 2;
		case RoadMode.AnyRoadOrTrail:
			return road.Hierarchy <= 2;
		default:
			return false;
		}
	}

	private void SpawnAlongRoad(ref uint seed, PathList road, Prefab[] prefabs)
	{
		PathInterpolator path = road.Path;
		float num = road.Width * 0.5f + SideOffset;
		float num2 = Distance * 0.25f;
		float num3 = Distance * 0.25f;
		float num4 = path.StartOffset + num3;
		float num5 = path.Length - path.EndOffset - num3;
		for (float num6 = num4; num6 <= num5; num6 += num2)
		{
			Vector3 vector = (road.Spline ? path.GetPointCubicHermite(num6) : path.GetPoint(num6));
			Vector3 normalized = path.GetTangent(num6).XZ3D().normalized;
			Vector3 vector2 = rot90 * normalized;
			int num7 = SeedRandom.Range(ref seed, 0, 2);
			Prefab random = ArrayEx.GetRandom(prefabs, ref seed);
			Quaternion localRotation = random.Object.transform.localRotation;
			bool flag = false;
			for (int i = 0; i < 2; i++)
			{
				int num8 = (((num7 + i) % 2 != 0) ? 1 : (-1));
				for (int j = 0; j < PlacementAttempts; j++)
				{
					float num9 = SeedRandom.Range(ref seed, 0f - Dithering, Dithering);
					float num10 = Mathf.Abs(SeedRandom.Range(ref seed, 0f - Dithering, Dithering));
					if (!flag)
					{
						float num11 = (num + num10) * (float)num8;
						Vector3 vector3 = vector;
						vector3.x += vector2.x * num11 + normalized.x * num9;
						vector3.z += vector2.z * num11 + normalized.z * num9;
						vector3.y = TerrainMeta.HeightMap.GetHeight(vector3);
						flag = TryPlace(random, vector3, localRotation);
					}
				}
			}
		}
	}

	private bool TryPlace(Prefab prefab, Vector3 position, Quaternion rotation)
	{
		Vector3 pos = position;
		Quaternion rot = rotation;
		Vector3 scale = prefab.Object.transform.localScale;
		if (Filter.GetFactor(pos) == 0f)
		{
			rejectedFilter++;
			return false;
		}
		prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
		if (!prefab.ApplyTerrainAnchors(ref pos, rot, scale, AnchorMode, Filter))
		{
			rejectedAnchors++;
			return false;
		}
		if (!prefab.ApplyTerrainChecks(pos, rot, scale, Filter))
		{
			rejectedAnchors++;
			return false;
		}
		if (TerrainMeta.HeightMap.GetSlope(pos) > MaxSlope)
		{
			rejectedSlope++;
			return false;
		}
		if (!prefab.ApplyWaterChecks(pos, rot, scale) || !prefab.ApplyEnvironmentVolumeChecks(pos, rot, scale))
		{
			rejectedWater++;
			return false;
		}
		if (!IsValidLocation(pos, rot, scale))
		{
			rejectedLocation++;
			return false;
		}
		World.AddPrefab("Decor", prefab, pos, rot, scale);
		placedPositions.Add(pos);
		return true;
	}
}
