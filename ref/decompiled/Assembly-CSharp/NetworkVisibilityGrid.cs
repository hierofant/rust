#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConVar;
using Network;
using Network.Visibility;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class NetworkVisibilityGrid : MonoBehaviour, Provider
{
	private class Layer
	{
		private readonly NetworkVisibilityGrid _grid;

		private readonly float _gridSize;

		public readonly int LayerIndex;

		public readonly float CellSize;

		public readonly float HalfGridSize;

		public readonly float HalfCellSize;

		public readonly int CellCount;

		public readonly Group[] Groups;

		public Layer(NetworkVisibilityGrid grid, float gridSize, int layerIndex, float cellSize)
		{
			_grid = grid;
			_gridSize = gridSize;
			LayerIndex = layerIndex;
			CellSize = cellSize;
			HalfGridSize = _gridSize / 2f;
			HalfCellSize = CellSize / 2f;
			CellCount = (int)((_gridSize + CellSize - 0.5f) / CellSize);
			Groups = new Group[CellCount * CellCount];
		}

		public int PositionToGrid(float value)
		{
			return Mathf.Clamp((int)((value + HalfGridSize) / CellSize), 0, CellCount - 1);
		}

		public float GridToPosition(int value)
		{
			return (float)value * CellSize - HalfGridSize;
		}

		public void SetupGroup(Group group)
		{
			(int x, int y, int layer) tuple = _grid.DeconstructGroupId((int)group.ID);
			int item = tuple.x;
			int item2 = tuple.y;
			Vector3 min = new Vector3(GridToPosition(item) - HalfCellSize, 0f, GridToPosition(item2) - HalfCellSize);
			Vector3 max = new Vector3(min.x + CellSize, 0f, min.z + CellSize);
			if (LayerIndex >= 10)
			{
				group.restricted = true;
				int num = LayerIndex - 10;
				min.y = _grid.dynamicDungeonsThreshold + (float)num * _grid.dynamicDungeonsInterval + float.Epsilon;
				max.y = min.y + _grid.dynamicDungeonsInterval;
			}
			else if (LayerIndex == 5)
			{
				min.y = DeepSeaManager.DeepSeaBounds.min.y;
				max.y = _grid.dynamicDungeonsThreshold;
			}
			else if (LayerIndex >= 0 && LayerIndex <= 4)
			{
				min.y = -10000f;
				max.y = _grid.dynamicDungeonsThreshold - float.Epsilon;
			}
			else
			{
				Debug.LogError($"Cannot get bounds for unknown layer {LayerIndex}!", _grid);
			}
			group.bounds = new Bounds
			{
				min = min,
				max = max
			};
		}
	}

	public const int overworldSmallLayer = 0;

	public const int overworldMediumLayer = 1;

	public const int overworldLargeLayer = 2;

	public const int cavesLayer = 3;

	public const int tunnelsLayer = 4;

	public const int deepSeaLayer = 5;

	public const int dynamicDungeonsFirstLayer = 10;

	public const int GlobalId = 0;

	public const int LimboId = 1;

	public const int MainIslandId = 2;

	public const int DeepSeaId = 3;

	public const int TutorialNetworkGroupStart = 100;

	public const int TutorialNetworkGroupEnd = 1000;

	public int startID = 1024;

	public int gridSize = 100;

	public int baseCellSize = 32;

	[FormerlySerializedAs("visibilityRadius")]
	public int visibilityRadiusFar = 2;

	public int visibilityRadiusNear = 1;

	public float switchTolerance = 20f;

	public float cavesThreshold = -0.5f;

	public float tunnelsThreshold = -20f;

	public float dynamicDungeonsThreshold = 1000f;

	public float dynamicDungeonsInterval = 100f;

	public int gizmoLayer;

	private Group[] _hardcodedGroups;

	private Layer[] _layers;

	private static List<ListHashSet<Vector2i>> tileOffsetsByRadius = new List<ListHashSet<Vector2i>>(64);

	public float SmallCellSize => (float)baseCellSize * 0.5f;

	public float DefaultCellSize => baseCellSize;

	public float LargeCellSize => (float)baseCellSize * 2f;

	public float DeepSeaCellSize => (float)baseCellSize * 2f;

	public float DynamicDungeonCellSize => dynamicDungeonsInterval;

	public void Awake()
	{
		Debug.Assert(Network.Net.sv != null, "Network.Net.sv is NULL when creating Visibility Grid");
		Debug.Assert(Network.Net.sv.visibility == null, "Network.Net.sv.visibility is being set multiple times");
		Network.Net.sv.visibility = new Manager(this);
		_hardcodedGroups = new Group[startID];
		_layers = new Layer[15];
		_layers[0] = new Layer(this, gridSize, 0, SmallCellSize);
		_layers[1] = new Layer(this, gridSize, 1, DefaultCellSize);
		_layers[2] = new Layer(this, gridSize, 2, LargeCellSize);
		_layers[3] = new Layer(this, gridSize, 3, DefaultCellSize);
		_layers[4] = new Layer(this, gridSize, 4, DefaultCellSize);
		_layers[5] = new Layer(this, gridSize, 5, LargeCellSize);
		for (int i = 10; i < _layers.Length; i++)
		{
			_layers[i] = new Layer(this, gridSize, i, DynamicDungeonCellSize);
		}
		GetTileOffsets(visibilityRadiusNear);
		GetTileOffsets(visibilityRadiusFar);
		if (ConVar.Net.visibilityRadiusNearOverride > -1)
		{
			GetTileOffsets(ConVar.Net.visibilityRadiusNearOverride);
		}
		if (ConVar.Net.visibilityRadiusFarOverride > -1)
		{
			GetTileOffsets(ConVar.Net.visibilityRadiusFarOverride);
		}
		GetTileOffsets(ConVar.Net.visibilityRadiusDeepSea);
	}

	private void OnDisable()
	{
		if (Rust.Application.isQuitting)
		{
			return;
		}
		if (Network.Net.sv != null && Network.Net.sv.visibility != null)
		{
			Network.Net.sv.visibility.Dispose();
			Network.Net.sv.visibility = null;
		}
		tileOffsetsByRadius = null;
		if (_layers != null)
		{
			Layer[] layers = _layers;
			for (int i = 0; i < layers.Length; i++)
			{
				Cleanup(layers[i].Groups);
			}
			_layers = null;
		}
		if (_hardcodedGroups != null)
		{
			Cleanup(_hardcodedGroups);
			_hardcodedGroups = null;
		}
		static void Cleanup(Group[] groups)
		{
			for (int j = 0; j < groups.Length; j++)
			{
				groups[j]?.Dispose();
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (gizmoLayer >= 0 && gizmoLayer < _layers.Length && _layers[gizmoLayer] != null)
		{
			Gizmos.color = Color.blue;
			Layer layer = _layers[gizmoLayer];
			for (int i = 0; i <= layer.CellCount; i++)
			{
				float num = 0f - layer.HalfGridSize + (float)i * layer.CellSize - layer.HalfCellSize;
				Gizmos.DrawLine(new Vector3(layer.HalfGridSize, 0f, num), new Vector3(0f - layer.HalfGridSize, 0f, num));
				Gizmos.DrawLine(new Vector3(num, 0f, layer.HalfGridSize), new Vector3(num, 0f, 0f - layer.HalfGridSize));
			}
		}
	}

	public int PositionToLayer(float x, float y, float z, EntityNetworkRange range)
	{
		if (y >= dynamicDungeonsThreshold)
		{
			return Mathf.Min(10 + Mathf.FloorToInt((y - dynamicDungeonsThreshold) / dynamicDungeonsInterval), _layers.Length - 1);
		}
		if (DeepSeaManager.IsInsideDeepSea(new Vector3(x, 0f, z)))
		{
			return 5;
		}
		float normX = Mathf.Clamp01(TerrainMeta.NormalizeX(x));
		float normZ = Mathf.Clamp01(TerrainMeta.NormalizeZ(z));
		float num = y - TerrainMeta.HeightMap.GetHeight(normX, normZ);
		if (num < tunnelsThreshold)
		{
			if (!CaveNetworkGroupLayerOverride.Includes(new Vector3(x, y, z)))
			{
				return 4;
			}
			return 3;
		}
		if (num < cavesThreshold)
		{
			return 3;
		}
		return range switch
		{
			EntityNetworkRange.Small => 0, 
			EntityNetworkRange.Medium => 1, 
			EntityNetworkRange.Large => 2, 
			_ => 1, 
		};
	}

	private uint CoordToID(int x, int y, int layer)
	{
		Assert.IsTrue(layer >= 0 && layer < _layers.Length, "layer >= 0 && layer < _layers.Length");
		Assert.IsNotNull(_layers[layer], "_layers[layer] != null");
		Assert.IsTrue(x >= 0 && x < _layers[layer].CellCount, "x >= 0 && x < _layers[layer].CellCount");
		Assert.IsTrue(y >= 0 && y < _layers[layer].CellCount, "y >= 0 && y < _layers[layer].CellCount");
		return CoordToIDUnchecked(x, y, layer);
	}

	private uint CoordToIDUnchecked(int x, int y, int layer)
	{
		Assert.IsTrue(layer >= 0 && layer <= 15, "layer >= 0 && layer <= 0xF");
		Assert.IsTrue(x >= 0 && x <= 16383, "x >= 0 && x <= 0x3FFF");
		Assert.IsTrue(y >= 0 && y <= 16383, "y >= 0 && y <= 0x3FFF");
		int num = ((layer & 0xF) << 28) | ((x & 0x3FFF) << 14) | (y & 0x3FFF);
		return (uint)(startID + num);
	}

	public (int x, int y, int layer) DeconstructGroupId(int groupId)
	{
		groupId -= startID;
		int item = (groupId >> 28) & 0xF;
		int item2 = (groupId >> 14) & 0x3FFF;
		int item3 = groupId & 0x3FFF;
		return (x: item2, y: item3, layer: item);
	}

	public bool IsGroupIdSpecial(uint groupId)
	{
		return groupId < startID;
	}

	public float GetFarDistanceForRange(EntityNetworkRange range)
	{
		int visibilityRadiusFarOverride = ConVar.Net.visibilityRadiusFarOverride;
		int num = ((visibilityRadiusFarOverride > 0) ? visibilityRadiusFarOverride : visibilityRadiusFar);
		return range switch
		{
			EntityNetworkRange.Small => (float)num * SmallCellSize, 
			EntityNetworkRange.Medium => (float)num * DefaultCellSize, 
			EntityNetworkRange.Large => (float)num * LargeCellSize, 
			_ => 0f, 
		};
	}

	public void ForEach(int layerInd, Action<Group> callback)
	{
		Group[] groups = _layers[layerInd].Groups;
		foreach (Group group in groups)
		{
			if (group != null)
			{
				callback(group);
			}
		}
	}

	public void AddGroups(int layerInd, ListHashSet<Group> groups, bool create)
	{
		Layer layer = _layers[layerInd];
		for (int i = 0; i < layer.Groups.Length; i++)
		{
			Group group = layer.Groups[i];
			if (group == null)
			{
				if (!create)
				{
					continue;
				}
				int x = i % layer.CellCount;
				int y = i / layer.CellCount;
				group = GetOrCreateFromLayer(x, y, layer);
			}
			groups.TryAdd(group);
		}
	}

	private uint GetID(Vector3 vPos, EntityNetworkRange range)
	{
		int num = PositionToLayer(vPos.x, vPos.y, vPos.z, range);
		Assert.IsNotNull(_layers[num], "_layers[layerIdx] != null");
		Layer layer = _layers[num];
		int num2 = layer.PositionToGrid(vPos.x);
		int num3 = layer.PositionToGrid(vPos.z);
		if (TerrainMeta.IsPointWithinTutorialBounds(vPos))
		{
			foreach (TutorialIsland.IslandBounds item in TutorialIsland.BoundsListServer)
			{
				if (item.Contains(vPos))
				{
					return item.Id;
				}
			}
		}
		uint num4 = CoordToID(num2, num3, num);
		if (num4 < startID)
		{
			Debug.LogError($"NetworkVisibilityGrid.GetID - group is below range {num2} {num3} {layer} {num4}");
		}
		return num4;
	}

	public bool IsInside(Group group, Vector3 vPos, EntityNetworkRange range)
	{
		bool flag = false || group.ID == 0 || group.bounds.Contains(vPos);
		int item = DeconstructGroupId((int)group.ID).layer;
		if (PositionToLayer(vPos.x, vPos.y, vPos.z, range) != item)
		{
			return false;
		}
		if (!group.restricted)
		{
			flag = flag || group.bounds.SqrDistance(vPos) < switchTolerance;
		}
		return flag;
	}

	public bool IsVisibleFromFar(Group from, Group to)
	{
		int visibilityRadiusFarOverride = ConVar.Net.visibilityRadiusFarOverride;
		int radius = ((visibilityRadiusFarOverride > 0) ? visibilityRadiusFarOverride : visibilityRadiusFar);
		return IsVisibleFrom(from, to, radius);
	}

	public bool IsVisibleFromNear(Group from, Group to)
	{
		int visibilityRadiusNearOverride = ConVar.Net.visibilityRadiusNearOverride;
		int radius = ((visibilityRadiusNearOverride > 0) ? visibilityRadiusNearOverride : visibilityRadiusNear);
		return IsVisibleFrom(from, to, radius);
	}

	private bool IsVisibleFrom(Group from, Group to, int radius)
	{
		if (to.isGlobal)
		{
			return true;
		}
		if (from.ID < startID)
		{
			if (from.restricted)
			{
				return from == to;
			}
			return false;
		}
		var (sourceX, sourceY, num) = DeconstructGroupId((int)from.ID);
		Assert.IsNotNull(_layers[num], "_layers[fromLayer] != null");
		if (num == 5)
		{
			if (to.ID == 3)
			{
				return true;
			}
		}
		else if (to.ID == 2)
		{
			return true;
		}
		if (from.restricted)
		{
			return from == to;
		}
		if (to.ID < startID)
		{
			return false;
		}
		var (num2, num3, num4) = DeconstructGroupId((int)to.ID);
		Assert.IsNotNull(_layers[num4], "_layers[toLayer] != null");
		Vector2i item = ConvertLayerCoords(_layers[num], sourceX, sourceY, num4).Position;
		Vector2i val = new Vector2i(num2 - item.x, num3 - item.y);
		switch (num)
		{
		case 0:
		case 1:
		case 2:
			switch (num4)
			{
			case 0:
			case 1:
			case 2:
				return GetTileOffsets(radius).Contains(val);
			case 3:
				return GetTileOffsets(radius / 2).Contains(val);
			}
			break;
		case 3:
			switch (num4)
			{
			case 3:
				return GetTileOffsets(radius).Contains(val);
			case 0:
			case 1:
			case 2:
			case 4:
				return GetTileOffsets(radius / 2).Contains(val);
			}
			break;
		case 4:
			switch (num4)
			{
			case 4:
				return GetTileOffsets(radius).Contains(val);
			case 3:
				return GetTileOffsets(radius / 2).Contains(val);
			}
			break;
		case 5:
			if (num4 == 5)
			{
				return GetTileOffsets(radius).Contains(val);
			}
			break;
		}
		return false;
	}

	public Group GetGroup(Vector3 vPos, EntityNetworkRange range)
	{
		uint iD = GetID(vPos, range);
		if (iD == 0)
		{
			return null;
		}
		Group group = GetGroup(iD);
		if (ConVar.Net.network_group_debug && !IsInside(group, vPos, range))
		{
			float num = group.bounds.SqrDistance(vPos);
			Debug.Log("Group is inside is all fucked " + iD + "/" + num + "/" + vPos);
		}
		return group;
	}

	public Group GetGroup(uint groupId)
	{
		if (groupId < startID)
		{
			return GetOrCreateFromHardcoded(_hardcodedGroups, groupId);
		}
		var (x, y, layerInd) = DeconstructGroupId((int)groupId);
		return GetOrCreateFromLayer(x, y, layerInd);
	}

	private Group GetOrCreateFromHardcoded(Group[] groupLayer, uint groupId)
	{
		Group group = groupLayer[groupId];
		if (group != null)
		{
			return group;
		}
		group = new Group(Network.Net.sv.visibility, groupId);
		return Interlocked.CompareExchange(ref groupLayer[groupId], group, null) ?? group;
	}

	private Group GetOrCreateFromLayer(int x, int y, int layerInd)
	{
		Layer layer = _layers[layerInd];
		return GetOrCreateFromLayer(x, y, layer);
	}

	private Group GetOrCreateFromLayer(int x, int y, Layer layer)
	{
		int num = y * layer.CellCount + x;
		Group group = layer.Groups[num];
		if (group != null)
		{
			return group;
		}
		uint id = CoordToIDUnchecked(x, y, layer.LayerIndex);
		group = new Group(Network.Net.sv.visibility, id);
		layer.SetupGroup(group);
		return Interlocked.CompareExchange(ref layer.Groups[num], group, null) ?? group;
	}

	public bool TryGetGroup(uint groupId, out Group group)
	{
		Group[] array;
		uint num;
		if (groupId < startID)
		{
			array = _hardcodedGroups;
			num = groupId;
		}
		else
		{
			(int x, int y, int layer) tuple = DeconstructGroupId((int)groupId);
			int item = tuple.x;
			int item2 = tuple.y;
			int item3 = tuple.layer;
			Layer layer = _layers[item3];
			array = layer.Groups;
			num = (uint)(item2 * layer.CellCount + item);
		}
		group = array[num];
		return group != null;
	}

	public void GetVisibleFromDistance(Group group, ListHashSet<Group> groups, float radiusInWorldUnits)
	{
		int radius = Mathf.FloorToInt(Mathf.Min(radiusInWorldUnits / (float)baseCellSize, 1f)) + 1;
		GetVisibleFrom(group, groups, radius);
	}

	public void GetVisibleFromFar(Group group, ListHashSet<Group> groups)
	{
		int num = ConVar.Net.visibilityRadiusFarOverride;
		if (DeconstructGroupId((int)group.ID).layer == 5 && ConVar.Net.visibilityRadiusDeepSea > num)
		{
			num = ConVar.Net.visibilityRadiusDeepSea;
		}
		int radius = ((num > 0) ? num : visibilityRadiusFar);
		GetVisibleFrom(group, groups, radius);
	}

	public void GetVisibleFromNear(Group group, ListHashSet<Group> groups)
	{
		int visibilityRadiusNearOverride = ConVar.Net.visibilityRadiusNearOverride;
		int radius = ((visibilityRadiusNearOverride > 0) ? visibilityRadiusNearOverride : visibilityRadiusNear);
		GetVisibleFrom(group, groups, radius);
	}

	private void GetGlobalNetworkGroups(Group group, ListHashSet<Group> groups)
	{
		groups.Add(GetGroup(0u));
		if (group.ID >= startID)
		{
			if (DeconstructGroupId((int)group.ID).layer == 5)
			{
				groups.Add(BaseNetworkable.DeepSeaGroup);
			}
			else
			{
				groups.Add(BaseNetworkable.MainIslandGroup);
			}
		}
	}

	public void GetVisibleFrom(Group group, ListHashSet<Group> groups, int radius)
	{
		if (Interface.CallHook("OnNetworkSubscriptionsGather", this, group, groups, radius) != null)
		{
			return;
		}
		ListHashSet<Group> groups2 = groups;
		GetGlobalNetworkGroups(group, groups2);
		if (group.restricted)
		{
			groups2.Add(group);
			return;
		}
		int iD = (int)group.ID;
		if (iD >= startID)
		{
			(int x, int y, int layer) tuple = DeconstructGroupId(iD);
			int item = tuple.x;
			int item2 = tuple.y;
			int item3 = tuple.layer;
			Layer layer = _layers[item3];
			Assert.IsNotNull(layer, "layer != null");
			if (item3 == 0 || item3 == 1 || item3 == 2)
			{
				AddLayer(layer, item, item2, 0, radius);
				AddLayer(layer, item, item2, 1, radius);
				AddLayer(layer, item, item2, 2, radius);
				AddLayer(layer, item, item2, 3, radius / 2);
			}
			if (item3 == 3)
			{
				AddLayer(layer, item, item2, 3, radius);
				AddLayer(layer, item, item2, 0, radius / 2);
				AddLayer(layer, item, item2, 1, radius / 2);
				AddLayer(layer, item, item2, 2, radius / 2);
				AddLayer(layer, item, item2, 4, radius / 2);
			}
			if (item3 == 4)
			{
				AddLayer(layer, item, item2, 4, radius);
				AddLayer(layer, item, item2, 3, radius / 2);
			}
			if (item3 == 5)
			{
				AddLayer(layer, item, item2, 5, radius);
			}
			Assert.IsTrue(groups2.Count > 0, "groups.Count > 0");
		}
		void AddLayer(Layer sourceLayer, int sourceX, int sourceY, int targetLayerIdx, int targetLayerRadius)
		{
			var (layer2, vector2i) = ConvertLayerCoords(sourceLayer, sourceX, sourceY, targetLayerIdx);
			foreach (Vector2i value in GetTileOffsets(targetLayerRadius).Values)
			{
				Vector2i vector2i2 = vector2i + value;
				if (vector2i2.x >= 0 && vector2i2.x < layer2.CellCount && vector2i2.y >= 0 && vector2i2.y < layer2.CellCount)
				{
					Group orCreateFromLayer = GetOrCreateFromLayer(vector2i2.x, vector2i2.y, layer2);
					groups2.Add(orCreateFromLayer);
				}
			}
		}
	}

	private (Layer Layer, Vector2i Position) ConvertLayerCoords(Layer sourceLayer, int sourceX, int sourceY, int destLayerIdx)
	{
		Layer layer;
		Vector2i item;
		if (destLayerIdx == sourceLayer.LayerIndex)
		{
			layer = sourceLayer;
			item = new Vector2i(sourceX, sourceY);
		}
		else
		{
			layer = _layers[destLayerIdx];
			if (layer == null)
			{
				throw new InvalidOperationException($"Destination layer {destLayerIdx} is null");
			}
			if (Mathf.Approximately(layer.CellSize, sourceLayer.CellSize))
			{
				item = new Vector2i(sourceX, sourceY);
			}
			else
			{
				Vector2 vector = new Vector2(sourceX, sourceY) * sourceLayer.CellSize + new Vector2(sourceLayer.HalfCellSize - sourceLayer.HalfGridSize, sourceLayer.HalfCellSize - sourceLayer.HalfGridSize);
				item = new Vector2i(layer.PositionToGrid(vector.x), layer.PositionToGrid(vector.y));
			}
		}
		return (Layer: layer, Position: item);
	}

	private static ListHashSet<Vector2i> GetTileOffsets(int radius)
	{
		radius = Mathf.Clamp(radius, 0, 64);
		if (radius < tileOffsetsByRadius.Count)
		{
			return tileOffsetsByRadius[radius];
		}
		while (radius >= tileOffsetsByRadius.Count)
		{
			tileOffsetsByRadius.Add(GenerateTileOffsetsUncached(tileOffsetsByRadius.Count));
		}
		return tileOffsetsByRadius[radius];
	}

	private static ListHashSet<Vector2i> GenerateTileOffsetsUncached(int radius)
	{
		ListHashSet<Vector2i> listHashSet = new ListHashSet<Vector2i>();
		if (radius <= 1)
		{
			listHashSet.Add(Vector2i.zero);
		}
		else
		{
			HashSet<Vector2i> hashSet = new HashSet<Vector2i>();
			int num = radius;
			int num2 = 0;
			int num3 = 1 - (radius << 1);
			int num4 = 0;
			int num5 = 0;
			while (num >= num2)
			{
				for (int i = -num; i <= num; i++)
				{
					hashSet.Add(new Vector2i(i, num2));
					hashSet.Add(new Vector2i(i, -num2));
				}
				for (int j = -num2; j <= num2; j++)
				{
					hashSet.Add(new Vector2i(j, num));
					hashSet.Add(new Vector2i(j, -num));
				}
				num2++;
				num5 += num4;
				num4 += 2;
				if ((num5 << 1) + num3 > 0)
				{
					num--;
					num5 += num3;
					num3 += 2;
				}
			}
			foreach (Vector2i item in hashSet.OrderBy((Vector2i v) => v.x * v.x + v.y * v.y))
			{
				listHashSet.Add(item);
			}
		}
		return listHashSet;
	}
}
