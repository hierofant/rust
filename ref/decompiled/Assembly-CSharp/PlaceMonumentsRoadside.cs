using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonumentsRoadside : ProceduralComponent
{
	public struct SpawnInfo
	{
		public Prefab<MonumentInfo> prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public PathInterpolator path;

		public int pathStartIndex;

		public int pathEndIndex;
	}

	public class SpawnInfoGroup
	{
		public bool processed;

		public Prefab<MonumentInfo> prefab;

		public List<SpawnInfo> candidates;
	}

	private struct DistanceInfo
	{
		public float minDistanceSameType;

		public float maxDistanceSameType;

		public float minDistanceDifferentType;

		public float maxDistanceDifferentType;
	}

	public enum DistanceMode
	{
		Any,
		Min,
		Max
	}

	public enum RoadMode
	{
		SideRoadOrRingRoad,
		SideRoad,
		RingRoad,
		SideRoadOrDesireTrail,
		DesireTrail
	}

	public SpawnFilter Filter;

	[Tooltip("Use this to spawn all monuments in a folder.")]
	public string ResourceFolder = string.Empty;

	[Tooltip("Use this to spawn specific monument prefabs.")]
	public GameObjectRef[] Monuments = Array.Empty<GameObjectRef>();

	public int TargetCount;

	[FormerlySerializedAs("MinDistance")]
	public int MinDistanceSameType = 500;

	public int MinDistanceDifferentType;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	[Tooltip("Distance to monuments of the same type")]
	public DistanceMode DistanceSameType = DistanceMode.Max;

	[Tooltip("Distance to monuments of a different type")]
	public DistanceMode DistanceDifferentType;

	public RoadMode RoadType;

	public const int GroupCandidates = 8;

	public const int IndividualCandidates = 8;

	public static Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);

	public override void Process(uint seed)
	{
		string[] array = Array.Empty<string>();
		if (!string.IsNullOrEmpty(ResourceFolder))
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
			_ = TerrainMeta.HeightMap;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains("tunnel-entrance") || World.Config.BelowGroundRails)
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
			SpawnInfoGroup[] array7 = new SpawnInfoGroup[array6.Length];
			for (int k = 0; k < array6.Length; k++)
			{
				Prefab<MonumentInfo> prefab = array6[k];
				SpawnInfoGroup spawnInfoGroup = null;
				for (int l = 0; l < k; l++)
				{
					SpawnInfoGroup spawnInfoGroup2 = array7[l];
					Prefab<MonumentInfo> prefab2 = spawnInfoGroup2.prefab;
					if (prefab == prefab2)
					{
						spawnInfoGroup = spawnInfoGroup2;
						break;
					}
				}
				if (spawnInfoGroup == null)
				{
					spawnInfoGroup = new SpawnInfoGroup();
					spawnInfoGroup.prefab = array6[k];
					spawnInfoGroup.candidates = new List<SpawnInfo>();
				}
				array7[k] = spawnInfoGroup;
			}
			SpawnInfoGroup[] array8 = array7;
			foreach (SpawnInfoGroup spawnInfoGroup3 in array8)
			{
				if (spawnInfoGroup3.processed)
				{
					continue;
				}
				Prefab<MonumentInfo> prefab3 = spawnInfoGroup3.prefab;
				MonumentInfo component = prefab3.Component;
				if (component == null || World.Size < component.MinWorldSize)
				{
					continue;
				}
				int num2 = 0;
				Vector3 zero = Vector3.zero;
				Vector3 zero2 = Vector3.zero;
				_ = Vector3.zero;
				float num3 = 0f;
				TerrainPathConnect[] componentsInChildren = prefab3.Object.GetComponentsInChildren<TerrainPathConnect>(includeInactive: true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type == InfrastructureType.Road)
					{
						Vector3 vector = terrainPathConnect.transform.position.XZ3D();
						zero += vector;
						num3 += vector.magnitude;
						if (num2 == 0)
						{
							zero2 += vector;
						}
						if (num2 == 1)
						{
							zero2 -= vector;
						}
						num2++;
					}
				}
				zero2 = zero2.normalized;
				_ = rot90 * zero2;
				if (num2 > 1)
				{
					zero /= (float)num2;
					num3 /= (float)num2;
				}
				foreach (PathList road in TerrainMeta.Path.Roads)
				{
					bool flag = false;
					switch (RoadType)
					{
					case RoadMode.SideRoadOrRingRoad:
						flag = road.Hierarchy == 0 || road.Hierarchy == 1;
						break;
					case RoadMode.SideRoad:
						flag = road.Hierarchy == 1;
						break;
					case RoadMode.RingRoad:
						flag = road.Hierarchy == 0;
						break;
					case RoadMode.SideRoadOrDesireTrail:
						flag = road.Hierarchy == 1 || road.Hierarchy == 2;
						break;
					case RoadMode.DesireTrail:
						flag = road.Hierarchy == 2;
						break;
					}
					if (!flag)
					{
						continue;
					}
					PathInterpolator path = road.Path;
					float num4 = 5f;
					float num5 = 5f;
					float num6 = path.StartOffset + num5 + num3;
					float num7 = path.Length - path.EndOffset - num5 - num3;
					for (float num8 = num6; num8 <= num7; num8 += num4)
					{
						float distance = num8 - num3;
						float distance2 = num8 + num3;
						int prevIndex = path.GetPrevIndex(distance);
						int nextIndex = path.GetNextIndex(distance2);
						Vector3 point = path.GetPoint(prevIndex);
						Vector3 point2 = path.GetPoint(nextIndex);
						Vector3 vector2 = (point + point2) * 0.5f;
						Vector3 normalized = (point2 - point).normalized;
						for (int n = -1; n <= 1; n += 2)
						{
							Quaternion quaternion = Quaternion.LookRotation(n * normalized.XZ3D());
							Vector3 position = vector2;
							Quaternion quaternion2 = quaternion;
							Vector3 localScale = prefab3.Object.transform.localScale;
							quaternion2 *= Quaternion.LookRotation(zero2);
							position -= quaternion2 * zero;
							SpawnInfo item2 = default(SpawnInfo);
							item2.prefab = prefab3;
							item2.position = position;
							item2.rotation = quaternion2;
							item2.scale = localScale;
							item2.path = path;
							item2.pathStartIndex = prevIndex;
							item2.pathEndIndex = nextIndex;
							spawnInfoGroup3.candidates.Add(item2);
						}
					}
				}
				spawnInfoGroup3.processed = true;
			}
			int num9 = 0;
			List<SpawnInfo> a = new List<SpawnInfo>();
			int num10 = 0;
			List<SpawnInfo> b = new List<SpawnInfo>();
			for (int num11 = 0; num11 < 8; num11++)
			{
				num9 = 0;
				a.Clear();
				ArrayEx.Shuffle(array7, ref seed);
				array8 = array7;
				foreach (SpawnInfoGroup spawnInfoGroup4 in array8)
				{
					Prefab<MonumentInfo> prefab4 = spawnInfoGroup4.prefab;
					MonumentInfo component2 = prefab4.Component;
					if (component2 == null || World.Size < component2.MinWorldSize)
					{
						continue;
					}
					DungeonGridInfo dungeonEntrance = component2.DungeonEntrance;
					int num12 = (int)((!prefab4.Parameters) ? PrefabPriority.Low : (prefab4.Parameters.Priority + 1));
					int num13 = 100000 * num12 * num12 * num12 * num12;
					int num14 = 0;
					int num15 = 0;
					SpawnInfo item3 = default(SpawnInfo);
					spawnInfoGroup4.candidates.Shuffle(ref seed);
					for (int num16 = 0; num16 < spawnInfoGroup4.candidates.Count; num16++)
					{
						SpawnInfo spawnInfo = spawnInfoGroup4.candidates[num16];
						DistanceInfo distanceInfo = GetDistanceInfo(a, prefab4, spawnInfo.position, spawnInfo.rotation, spawnInfo.scale);
						if (distanceInfo.minDistanceSameType < (float)MinDistanceSameType || distanceInfo.minDistanceDifferentType < (float)MinDistanceDifferentType)
						{
							continue;
						}
						int num17 = num13;
						if (distanceInfo.minDistanceSameType != float.MaxValue)
						{
							if (DistanceSameType == DistanceMode.Min)
							{
								num17 -= Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
							else if (DistanceSameType == DistanceMode.Max)
							{
								num17 += Mathf.RoundToInt(distanceInfo.minDistanceSameType * distanceInfo.minDistanceSameType * 2f);
							}
						}
						if (distanceInfo.minDistanceDifferentType != float.MaxValue)
						{
							if (DistanceDifferentType == DistanceMode.Min)
							{
								num17 -= Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
							else if (DistanceDifferentType == DistanceMode.Max)
							{
								num17 += Mathf.RoundToInt(distanceInfo.minDistanceDifferentType * distanceInfo.minDistanceDifferentType);
							}
						}
						if (num17 <= num15 || !prefab4.ApplyTerrainFilters(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) || !prefab4.ApplyTerrainAnchors(ref spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, Filter) || !component2.CheckPlacement(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale))
						{
							continue;
						}
						if ((bool)dungeonEntrance)
						{
							Vector3 vector3 = spawnInfo.position + spawnInfo.rotation * Vector3.Scale(spawnInfo.scale, dungeonEntrance.transform.position);
							Vector3 vector4 = dungeonEntrance.SnapPosition(vector3);
							spawnInfo.position += vector4 - vector3;
							if (!dungeonEntrance.IsValidSpawnPosition(vector4))
							{
								continue;
							}
						}
						if (prefab4.ApplyTerrainChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, Filter) && prefab4.ApplyWaterChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) && prefab4.ApplyEnvironmentVolumeChecks(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale) && !prefab4.CheckEnvironmentVolumes(spawnInfo.position, spawnInfo.rotation, spawnInfo.scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
						{
							num15 = num17;
							item3 = spawnInfo;
							num14++;
							if (num14 >= 8 || DistanceDifferentType == DistanceMode.Any)
							{
								break;
							}
						}
					}
					if (num15 > 0)
					{
						a.Add(item3);
						num9 += num15;
					}
					if (TargetCount > 0 && a.Count >= TargetCount)
					{
						break;
					}
				}
				if (num9 > num10)
				{
					num10 = num9;
					GenericsUtil.Swap(ref a, ref b);
				}
			}
			foreach (SpawnInfo item4 in b)
			{
				World.AddPrefab("Monument", item4.prefab, item4.position, item4.rotation, item4.scale);
			}
			HashSet<PathInterpolator> hashSet = new HashSet<PathInterpolator>();
			foreach (SpawnInfo item5 in b)
			{
				item5.path.Straighten(item5.pathStartIndex, item5.pathEndIndex);
				hashSet.Add(item5.path);
			}
			foreach (PathInterpolator item6 in hashSet)
			{
				item6.RecalculateLength();
			}
		}
	}

	private DistanceInfo GetDistanceInfo(List<SpawnInfo> spawns, Prefab<MonumentInfo> prefab, Vector3 monumentPos, Quaternion monumentRot, Vector3 monumentScale)
	{
		DistanceInfo result = default(DistanceInfo);
		result.minDistanceDifferentType = float.MaxValue;
		result.maxDistanceDifferentType = float.MinValue;
		result.minDistanceSameType = float.MaxValue;
		result.maxDistanceSameType = float.MinValue;
		OBB oBB = new OBB(monumentPos, monumentScale, monumentRot, prefab.Component.Bounds);
		if (TerrainMeta.Path != null)
		{
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				if (!prefab.Component.HasDungeonLink || (!monument.HasDungeonLink && monument.WantsDungeonLink))
				{
					float num = monument.SqrDistance(oBB);
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
			if (result.minDistanceDifferentType != float.MaxValue)
			{
				result.minDistanceDifferentType = Mathf.Sqrt(result.minDistanceDifferentType);
			}
			if (result.maxDistanceDifferentType != float.MinValue)
			{
				result.maxDistanceDifferentType = Mathf.Sqrt(result.maxDistanceDifferentType);
			}
		}
		if (spawns != null)
		{
			foreach (SpawnInfo spawn in spawns)
			{
				float num2 = new OBB(spawn.position, spawn.scale, spawn.rotation, spawn.prefab.Component.Bounds).SqrDistance(oBB);
				if (num2 < result.minDistanceSameType)
				{
					result.minDistanceSameType = num2;
				}
				if (num2 > result.maxDistanceSameType)
				{
					result.maxDistanceSameType = num2;
				}
			}
			if (prefab.Component.HasDungeonLink)
			{
				foreach (MonumentInfo monument2 in TerrainMeta.Path.Monuments)
				{
					if (monument2.HasDungeonLink || !monument2.WantsDungeonLink)
					{
						float num3 = monument2.SqrDistance(oBB);
						if (num3 < result.minDistanceSameType)
						{
							result.minDistanceSameType = num3;
						}
						if (num3 > result.maxDistanceSameType)
						{
							result.maxDistanceSameType = num3;
						}
					}
				}
				foreach (DungeonGridInfo dungeonGridEntrance in TerrainMeta.Path.DungeonGridEntrances)
				{
					float num4 = dungeonGridEntrance.SqrDistance(monumentPos);
					if (num4 < result.minDistanceSameType)
					{
						result.minDistanceSameType = num4;
					}
					if (num4 > result.maxDistanceSameType)
					{
						result.maxDistanceSameType = num4;
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
		}
		return result;
	}
}
