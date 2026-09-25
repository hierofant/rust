using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonuments : ProceduralComponent
{
	private readonly struct PlacementRung
	{
		public readonly float Spacing;

		public readonly int MinCorners;

		public readonly bool AllowAdjacentTier;

		public PlacementRung(float spacing, int minCorners, bool allowAdjacentTier)
		{
			Spacing = spacing;
			MinCorners = minCorners;
			AllowAdjacentTier = allowAdjacentTier;
		}
	}

	public struct WorldSizeInfo
	{
		public int WorldSizeMin;

		public int WorldSizeMax;

		public int TargetCount;
	}

	public struct SpawnInfo
	{
		public Prefab<MonumentInfo> prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public bool dungeonEntrance;

		public Vector3 dungeonEntrancePos;
	}

	private struct DistanceInfo
	{
		public float minDistanceSameType;

		public float maxDistanceSameType;

		public float minDistanceDifferentType;

		public float maxDistanceDifferentType;

		public float minDistanceDungeonEntrance;

		public float maxDistanceDungeonEntrance;
	}

	public enum DistanceMode
	{
		Any,
		Min,
		Max
	}

	public SpawnFilter Filter;

	[Tooltip("Use this to spawn all monuments in a folder.")]
	public string ResourceFolder = string.Empty;

	[Tooltip("Use this to spawn specific monument prefabs.")]
	public GameObjectRef[] Monuments = Array.Empty<GameObjectRef>();

	public int TargetCount;

	public AnimationCurve TargetCountWorldSizeMultiplier = AnimationCurve.Constant(1000f, 6000f, 1f);

	[FormerlySerializedAs("MinDistance")]
	public int MinDistanceSameType = 500;

	public int MinDistanceDifferentType;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	[Tooltip("Distance to monuments of the same type")]
	public DistanceMode DistanceSameType = DistanceMode.Max;

	[Tooltip("Distance to monuments of a different type")]
	public DistanceMode DistanceDifferentType;

	[Tooltip("Enable to only spawn these monuments when running as a nexus")]
	public bool NexusOnly;

	private const PrefabPriority RequiredPriority = PrefabPriority.Highest;

	private const int MinDistanceFloor = 50;

	public const int GroupCandidates = 8;

	public const int IndividualCandidates = 8;

	public const int Attempts = 10000;

	private const int RetryCandidates = 8;

	private const float DistanceScoreWeight = 0.1f;

	private const float SameTypeDistanceWeight = 2f;

	private const float DifferentTypeDistanceWeight = 1f;

	private const float DistanceScoreScale = 1f / 60f;

	private static readonly float[] RelaxationTiers = new float[4] { 1f, 0.75f, 0.5f, 0.25f };

	private static readonly PlacementRung[] PlacementRungs = new PlacementRung[9]
	{
		new PlacementRung(1f, 3, allowAdjacentTier: false),
		new PlacementRung(0.75f, 3, allowAdjacentTier: false),
		new PlacementRung(0.5f, 3, allowAdjacentTier: false),
		new PlacementRung(1f, 2, allowAdjacentTier: false),
		new PlacementRung(0.5f, 2, allowAdjacentTier: false),
		new PlacementRung(1f, 1, allowAdjacentTier: false),
		new PlacementRung(0.5f, 1, allowAdjacentTier: false),
		new PlacementRung(1f, 3, allowAdjacentTier: true),
		new PlacementRung(0.5f, 3, allowAdjacentTier: true)
	};

	private const int RemainingAttemptMultiplier = 4;

	private const int MaxDepth = 100000;

	public override void Process(uint seed)
	{
		if (NexusOnly && !World.Nexus)
		{
			return;
		}
		string[] array = Array.Empty<string>();
		if (!string.IsNullOrWhiteSpace(ResourceFolder))
		{
			array = (from folder in ResourceFolder.Split(',')
				select "assets/bundled/prefabs/autospawn/" + folder + "/").ToArray();
		}
		if (World.Networked)
		{
			World.Spawn("Monument", array, Monuments);
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			PathFinder pathFinder = null;
			List<PathFinder.Point> pathTargets = null;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("underwater_lab") || World.Config.UnderwaterLabs)
				{
					Prefab<MonumentInfo>[] array3 = Prefab.Load<MonumentInfo>(text);
					ArrayEx.Shuffle(array3, ref seed);
					list.AddRange(array3);
				}
			}
			int num = Monuments.Length;
			if (num > 0)
			{
				GameObjectRef[] array4 = Monuments;
				if (num > 1)
				{
					GameObjectRef[] array5 = Monuments.ToArray();
					ArrayEx.Shuffle(array5, ref seed);
					array4 = array5;
				}
				for (int j = 0; j < num; j++)
				{
					GameObjectRef gameObjectRef = array4[j];
					if (gameObjectRef.isValid)
					{
						Prefab<MonumentInfo> item = Prefab.Load<MonumentInfo>(gameObjectRef.resourceID);
						list.Add(item);
					}
				}
			}
			Prefab<MonumentInfo>[] array6 = list.ToArray();
			if (array6 == null || array6.Length == 0)
			{
				return;
			}
			ArrayEx.BubbleSort(array6);
			long num2 = 0L;
			int num3 = 0;
			List<SpawnInfo> a = new List<SpawnInfo>();
			long num4 = 0L;
			int num5 = 0;
			List<SpawnInfo> b = new List<SpawnInfo>();
			int num6 = Mathf.RoundToInt((float)TargetCount * TargetCountWorldSizeMultiplier.Evaluate(World.Size));
			int num7 = 0;
			Prefab<MonumentInfo>[] array7 = array6;
			foreach (Prefab<MonumentInfo> prefab in array7)
			{
				if (!(prefab.Component == null) && World.Size >= prefab.Component.MinWorldSize && GetPriority(prefab) >= PrefabPriority.Highest)
				{
					num7++;
				}
			}
			if (num6 > 0)
			{
				num7 = Mathf.Min(num7, num6);
			}
			int num8 = 8 + ((num7 > 0) ? (RelaxationTiers.Length * 8) : 0);
			for (int k = 0; k < num8; k++)
			{
				bool flag = k >= 8;
				if (flag && num5 >= num7)
				{
					break;
				}
				float relaxation = (flag ? RelaxationTiers[(k - 8) / 8] : 1f);
				float num9 = RelaxDistance(MinDistanceSameType, relaxation);
				float num10 = RelaxDistance(MinDistanceDifferentType, relaxation);
				num2 = 0L;
				num3 = 0;
				a.Clear();
				bool flag2 = false;
				array7 = array6;
				foreach (Prefab<MonumentInfo> prefab2 in array7)
				{
					MonumentInfo component = prefab2.Component;
					if (component == null || World.Size < component.MinWorldSize)
					{
						continue;
					}
					_ = component.DungeonEntrance;
					PrefabPriority priority = GetPriority(prefab2);
					bool flag3 = priority >= PrefabPriority.Highest;
					int num11 = (int)(priority + 1);
					int priorityScore = 100000 * num11 * num11 * num11 * num11;
					float minDistanceSameType = (flag3 ? num9 : ((float)MinDistanceSameType));
					float minDistanceDifferentType = (flag3 ? num10 : ((float)MinDistanceDifferentType));
					if (TryFindSpawn(prefab2, a, ref seed, ref pathFinder, ref pathTargets, minDistanceSameType, minDistanceDifferentType, priorityScore, PlacementRungs[0].MinCorners, PlacementRungs[0].AllowAdjacentTier, 10000, out var resultSpawn, out var resultScore))
					{
						a.Add(resultSpawn);
						num2 += resultScore;
						if (flag3)
						{
							num3++;
						}
					}
					else if (flag3 && flag)
					{
						flag2 = true;
						break;
					}
					if (num6 > 0 && a.Count >= num6)
					{
						break;
					}
				}
				if (!flag2 && (num3 > num5 || (num3 == num5 && num2 > num4)))
				{
					num5 = num3;
					num4 = num2;
					GenericsUtil.Swap(ref a, ref b);
				}
			}
			PlaceRemainingMonuments(array6, b, ref seed, ref pathFinder, ref pathTargets, num6);
			foreach (SpawnInfo item2 in b)
			{
				World.AddPrefab("Monument", item2.prefab, item2.position, item2.rotation, item2.scale);
			}
		}
	}

	private void PlaceRemainingMonuments(Prefab<MonumentInfo>[] prefabs, List<SpawnInfo> spawns, ref uint seed, ref PathFinder pathFinder, ref List<PathFinder.Point> pathTargets, int targetCount)
	{
		foreach (Prefab<MonumentInfo> prefab in prefabs)
		{
			if (targetCount > 0 && spawns.Count >= targetCount)
			{
				break;
			}
			MonumentInfo component = prefab.Component;
			if (component == null || World.Size < component.MinWorldSize)
			{
				continue;
			}
			bool flag = false;
			foreach (SpawnInfo spawn in spawns)
			{
				if (spawn.prefab == prefab)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			int num = (int)(GetPriority(prefab) + 1);
			int priorityScore = 100000 * num * num * num * num;
			PlacementRung[] placementRungs = PlacementRungs;
			for (int j = 0; j < placementRungs.Length; j++)
			{
				PlacementRung placementRung = placementRungs[j];
				float minDistanceSameType = RelaxDistance(MinDistanceSameType, placementRung.Spacing);
				float minDistanceDifferentType = RelaxDistance(MinDistanceDifferentType, placementRung.Spacing);
				if (TryFindSpawn(prefab, spawns, ref seed, ref pathFinder, ref pathTargets, minDistanceSameType, minDistanceDifferentType, priorityScore, placementRung.MinCorners, placementRung.AllowAdjacentTier, 40000, out var resultSpawn, out var _))
				{
					spawns.Add(resultSpawn);
					break;
				}
			}
		}
	}

	private bool TryFindSpawn(Prefab<MonumentInfo> prefab, List<SpawnInfo> spawns, ref uint seed, ref PathFinder pathFinder, ref List<PathFinder.Point> pathTargets, float minDistanceSameType, float minDistanceDifferentType, int priorityScore, int minCorners, bool allowAdjacentTier, int attempts, out SpawnInfo resultSpawn, out int resultScore)
	{
		MonumentInfo component = prefab.Component;
		DungeonGridInfo dungeonEntrance = component.DungeonEntrance;
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float x = position.x;
		float z = position.z;
		float max = position.x + size.x;
		float max2 = position.z + size.z;
		int num = 0;
		bool result = false;
		int num2 = int.MinValue;
		SpawnInfo spawnInfo = default(SpawnInfo);
		for (int i = 0; i < attempts; i++)
		{
			float x2 = SeedRandom.Range(ref seed, x, max);
			float z2 = SeedRandom.Range(ref seed, z, max2);
			float normX = TerrainMeta.NormalizeX(x2);
			float normZ = TerrainMeta.NormalizeZ(z2);
			float num3 = SeedRandom.Value(ref seed);
			float factor = Filter.GetFactor(normX, normZ);
			if (factor * factor < num3)
			{
				continue;
			}
			float height = heightMap.GetHeight(normX, normZ);
			Vector3 pos = new Vector3(x2, height, z2);
			Quaternion rot = prefab.Object.transform.localRotation;
			Vector3 scale = prefab.Object.transform.localScale;
			Vector3 vector = pos;
			prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
			DistanceInfo distanceInfo = GetDistanceInfo(spawns, prefab, pos, rot, scale, vector);
			if (distanceInfo.minDistanceSameType < minDistanceSameType || distanceInfo.minDistanceDifferentType < minDistanceDifferentType || ((bool)dungeonEntrance && distanceInfo.minDistanceDungeonEntrance < dungeonEntrance.MinDistance))
			{
				continue;
			}
			int num4 = priorityScore;
			if (distanceInfo.minDistanceSameType != float.MaxValue)
			{
				float num5 = distanceInfo.minDistanceSameType / (float)World.Size;
				int num6 = Mathf.RoundToInt((float)priorityScore * num5 * num5 * (1f / 60f) * 2f);
				if (DistanceSameType == DistanceMode.Min)
				{
					num4 -= num6;
				}
				else if (DistanceSameType == DistanceMode.Max)
				{
					num4 += num6;
				}
			}
			if (distanceInfo.minDistanceDifferentType != float.MaxValue)
			{
				float num7 = distanceInfo.minDistanceDifferentType / (float)World.Size;
				int num8 = Mathf.RoundToInt((float)priorityScore * num7 * num7 * (1f / 60f) * 1f);
				if (DistanceDifferentType == DistanceMode.Min)
				{
					num4 -= num8;
				}
				else if (DistanceDifferentType == DistanceMode.Max)
				{
					num4 += num8;
				}
			}
			if (num4 <= num2 || !prefab.ApplyTerrainFilters(pos, rot, scale) || !prefab.ApplyTerrainAnchors(ref pos, rot, scale, Filter) || !component.CheckPlacement(pos, rot, scale, minCorners, allowAdjacentTier))
			{
				continue;
			}
			if ((bool)dungeonEntrance)
			{
				Vector3 vector2 = pos + rot * Vector3.Scale(scale, dungeonEntrance.transform.position);
				Vector3 vector3 = dungeonEntrance.SnapPosition(vector2);
				pos += vector3 - vector2;
				if (!dungeonEntrance.IsValidSpawnPosition(vector3))
				{
					continue;
				}
				vector = vector3;
			}
			if (!prefab.ApplyTerrainChecks(pos, rot, scale, Filter) || !prefab.ApplyWaterChecks(pos, rot, scale) || !prefab.ApplyEnvironmentVolumeChecks(pos, rot, scale) || prefab.CheckEnvironmentVolumes(pos, rot, scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
			{
				continue;
			}
			bool flag = false;
			TerrainPathConnect[] componentsInChildren = prefab.Object.GetComponentsInChildren<TerrainPathConnect>(includeInactive: true);
			foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
			{
				if (terrainPathConnect.Type == InfrastructureType.Boat)
				{
					if (pathFinder == null)
					{
						int[,] array = TerrainPath.CreateBoatCostmap(4f);
						int length = array.GetLength(0);
						pathFinder = new PathFinder(array);
						pathTargets = new List<PathFinder.Point>
						{
							new PathFinder.Point(0, 0),
							new PathFinder.Point(0, length / 2),
							new PathFinder.Point(0, length - 1),
							new PathFinder.Point(length / 2, 0),
							new PathFinder.Point(length / 2, length - 1),
							new PathFinder.Point(length - 1, 0),
							new PathFinder.Point(length - 1, length / 2),
							new PathFinder.Point(length - 1, length - 1)
						};
					}
					PathFinder.Point point = PathFinder.GetPoint(pos + rot * Vector3.Scale(scale, terrainPathConnect.transform.localPosition), pathFinder.GetResolution(0));
					if (pathFinder.FindPathUndirected(new List<PathFinder.Point> { point }, pathTargets, 100000) == null)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				SpawnInfo spawnInfo2 = default(SpawnInfo);
				spawnInfo2.prefab = prefab;
				spawnInfo2.position = pos;
				spawnInfo2.rotation = rot;
				spawnInfo2.scale = scale;
				if ((bool)dungeonEntrance)
				{
					spawnInfo2.dungeonEntrance = true;
					spawnInfo2.dungeonEntrancePos = vector;
				}
				num2 = num4;
				spawnInfo = spawnInfo2;
				result = true;
				num++;
				if (num >= 8 || DistanceDifferentType == DistanceMode.Any)
				{
					break;
				}
			}
		}
		resultSpawn = spawnInfo;
		resultScore = num2;
		return result;
	}

	private static PrefabPriority GetPriority(Prefab<MonumentInfo> prefab)
	{
		if (!prefab.Parameters)
		{
			return PrefabPriority.Lowest;
		}
		return prefab.Parameters.Priority;
	}

	private static float RelaxDistance(int configured, float relaxation)
	{
		if (configured <= 50)
		{
			return configured;
		}
		return Mathf.Max((float)configured * relaxation, 50f);
	}

	public DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale, Vector3 dungeonPos)
	{
		DistanceInfo result = default(DistanceInfo);
		result.minDistanceSameType = float.MaxValue;
		result.maxDistanceSameType = float.MinValue;
		result.minDistanceDifferentType = float.MaxValue;
		result.maxDistanceDifferentType = float.MinValue;
		result.minDistanceDungeonEntrance = float.MaxValue;
		result.maxDistanceDungeonEntrance = float.MinValue;
		OBB oBB = new OBB(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				float num = new OBB(spawn.position, spawn.scale, spawn.rotation, spawn.prefab.Component.Bounds).SqrDistance(oBB);
				if (spawn.prefab.Folder == prefab.Folder)
				{
					if (num < result.minDistanceSameType)
					{
						result.minDistanceSameType = num;
					}
					if (num > result.maxDistanceSameType)
					{
						result.maxDistanceSameType = num;
					}
				}
				else
				{
					if (num < result.minDistanceDifferentType)
					{
						result.minDistanceDifferentType = num;
					}
					if (num > result.maxDistanceDifferentType)
					{
						result.maxDistanceDifferentType = num;
					}
				}
			}
			foreach (SpawnInfo spawn2 in spawns)
			{
				if (spawn2.dungeonEntrance)
				{
					float sqrMagnitude = (spawn2.dungeonEntrancePos - dungeonPos).sqrMagnitude;
					if (sqrMagnitude < result.minDistanceDungeonEntrance)
					{
						result.minDistanceDungeonEntrance = sqrMagnitude;
					}
					if (sqrMagnitude > result.maxDistanceDungeonEntrance)
					{
						result.maxDistanceDungeonEntrance = sqrMagnitude;
					}
				}
			}
		}
		if (TerrainMeta.Path != null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				float num2 = monument.SqrDistance(oBB);
				if (num2 < result.minDistanceDifferentType)
				{
					result.minDistanceDifferentType = num2;
				}
				if (num2 > result.maxDistanceDifferentType)
				{
					result.maxDistanceDifferentType = num2;
				}
			}
			foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
			{
				float num3 = dungeonGridEntrance.SqrDistance(dungeonPos);
				if (num3 < result.minDistanceDungeonEntrance)
				{
					result.minDistanceDungeonEntrance = num3;
				}
				if (num3 > result.maxDistanceDungeonEntrance)
				{
					result.maxDistanceDungeonEntrance = num3;
				}
			}
		}
		if (result.minDistanceSameType != float.MaxValue)
		{
			result.minDistanceSameType = Mathf.Sqrt(result.minDistanceSameType);
		}
		if (result.maxDistanceSameType != float.MinValue)
		{
			result.maxDistanceSameType = Mathf.Sqrt(result.maxDistanceSameType);
		}
		if (result.minDistanceDifferentType != float.MaxValue)
		{
			result.minDistanceDifferentType = Mathf.Sqrt(result.minDistanceDifferentType);
		}
		if (result.maxDistanceDifferentType != float.MinValue)
		{
			result.maxDistanceDifferentType = Mathf.Sqrt(result.maxDistanceDifferentType);
		}
		if (result.minDistanceDungeonEntrance != float.MaxValue)
		{
			result.minDistanceDungeonEntrance = Mathf.Sqrt(result.minDistanceDungeonEntrance);
		}
		if (result.maxDistanceDungeonEntrance != float.MinValue)
		{
			result.maxDistanceDungeonEntrance = Mathf.Sqrt(result.maxDistanceDungeonEntrance);
		}
		return result;
	}
}
