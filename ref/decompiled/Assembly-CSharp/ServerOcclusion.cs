using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ConVar;
using Facepunch;
using Network.Visibility;
using ServerOcclusionJobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class ServerOcclusion
{
	public class Group : ListHashSet<BaseNetworkable>, Facepunch.Pool.IPooled
	{
		void Facepunch.Pool.IPooled.EnterPool()
		{
			Clear();
		}

		void Facepunch.Pool.IPooled.LeavePool()
		{
		}
	}

	public readonly struct Grid : IEquatable<Grid>
	{
		public readonly int x;

		public readonly int y;

		public readonly int z;

		public const float Resolution = 16f;

		public const float HalfResolution = 8f;

		public Grid(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static Grid FromIndex(int index)
		{
			int num = index / (ChunkCountX * ChunkCountY);
			index -= num * (ChunkCountX * ChunkCountY);
			int num2 = index / ChunkCountX;
			index -= num2 * ChunkCountX;
			return new Grid(index, num2, num);
		}

		public static int GetOffset(float axis)
		{
			return Mathf.RoundToInt(axis / 2f / 16f);
		}

		public Vector3 GetCenterPoint()
		{
			return new Vector3((float)(x - GetOffset(TerrainMeta.Size.x)) * 16f, (float)(y - GetOffset(MaxY)) * 16f, (float)(z - GetOffset(TerrainMeta.Size.z)) * 16f);
		}

		public override string ToString()
		{
			return $"(x: {x}, y: {y}, z: {z})";
		}

		public bool Equals(Grid other)
		{
			if (x == other.x && y == other.y)
			{
				return z == other.z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public bool IsBlocked()
		{
			return GamePhysics.CheckBounds(new Bounds(GetCenterPoint(), new Vector3(16f, 16f, 16f)), 8388608);
		}

		public int GetIndex()
		{
			return GetGridIndex(x, y, z);
		}
	}

	public readonly struct SubGrid : IEquatable<SubGrid>
	{
		public readonly int x;

		public readonly int y;

		public readonly int z;

		public const float Resolution = 2f;

		public const float HalfResolution = 1f;

		public SubGrid(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public SubGrid(int3 p)
		{
			x = p.x;
			y = p.y;
			z = p.z;
		}

		public static int GetOffset(float axis)
		{
			return Mathf.RoundToInt(axis / 2f / 2f);
		}

		public Vector3 GetCenterPoint()
		{
			return new Vector3((float)(x - GetOffset(TerrainMeta.Size.x)) * 2f, (float)(y - GetOffset(MaxY)) * 2f, (float)(z - GetOffset(TerrainMeta.Size.z)) * 2f);
		}

		public override string ToString()
		{
			return $"(x: {x}, y: {y}, z: {z}) - {GetCenterPoint()}, {IsBlocked()}";
		}

		public bool Equals(SubGrid other)
		{
			if (x == other.x && y == other.y)
			{
				return z == other.z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y, z);
		}

		public bool IsBlocked()
		{
			Vector3[] gridOffsets;
			if (OcclusionIncludeRocks)
			{
				bool flag = true;
				gridOffsets = GridOffsets;
				foreach (Vector3 vector in gridOffsets)
				{
					if (!flag)
					{
						break;
					}
					Vector3 pos = GetCenterPoint() + vector;
					flag &= AntiHack.IsInsideMesh(pos);
					if (flag)
					{
						GameObject gameObject = AntiHack.isInsideRayHit.collider.gameObject;
						flag &= gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement);
					}
				}
				if (flag)
				{
					return true;
				}
			}
			gridOffsets = GridOffsets;
			foreach (Vector3 vector2 in gridOffsets)
			{
				if (AntiHack.TestInsideTerrain(GetCenterPoint() + vector2))
				{
					return true;
				}
			}
			return false;
		}

		public int GetIndex()
		{
			return GetSubGridIndex(x, y, z);
		}

		public int GetDistance(SubGrid other)
		{
			return Mathf.Abs(x - other.x) + Mathf.Abs(y - other.y) + Mathf.Abs(z - other.z);
		}
	}

	public const int CacheVersion = 3;

	public static int MaxY = 200;

	public static int ChunkCountX;

	public static int ChunkCountY;

	public static int ChunkCountZ;

	public static int SubChunkCountX;

	public static int SubChunkCountY;

	public static int SubChunkCountZ;

	public static float AxisX;

	public static float AxisY;

	public static float AxisZ;

	public static LimitDictionary<(int, int), bool> OcclusionCache = new LimitDictionary<(int, int), bool>(32768);

	public static NativeArray<NativeBitArray> OcclusionSubGridBlocked;

	public static NativeReference<bool> ReturnHolder;

	public const int OcclusionChunkSize = 16;

	public const int OcclusionChunkResolution = 8;

	public static Dictionary<Network.Visibility.Group, Group> Occludees = new Dictionary<Network.Visibility.Group, Group>();

	public static readonly Vector3[] GridOffsets = new Vector3[2]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f)
	};

	public static readonly (int, int, int)[] neighbours = new(int, int, int)[6]
	{
		(1, 0, 0),
		(-1, 0, 0),
		(0, 1, 0),
		(0, -1, 0),
		(0, 0, 1),
		(0, 0, -1)
	};

	public static bool OcclusionEnabled { get; set; } = true;


	public static bool OcclusionIncludeRocks { get; set; } = true;


	public static float OcclusionPollRate => 2f;

	public static int MinOcclusionDistance => 25;

	public static string SubGridFilePath
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return string.Format("{0}/{1}_occlusion_{2}.dat", Server.rootFolder, World.MapFileName.Replace(".map", ""), 3);
		}
	}

	public static int GetGridIndex(int x, int y, int z)
	{
		return z * ChunkCountX * ChunkCountY + y * ChunkCountX + x;
	}

	public static int GetSubGridIndex(int x, int y, int z)
	{
		return z * SubChunkCountX * SubChunkCountY + y * SubChunkCountX + x;
	}

	public static int GetGrid(float position, float axis)
	{
		return Mathf.RoundToInt(position / 16f + axis / 16f);
	}

	public static Grid GetGrid(Vector3 position)
	{
		int grid = GetGrid(position.x, AxisX);
		int grid2 = GetGrid(position.y, AxisY);
		int grid3 = GetGrid(position.z, AxisZ);
		if (IsValidGrid(grid, grid2, grid3))
		{
			return new Grid(grid, grid2, grid3);
		}
		return default(Grid);
	}

	public static int GetSubGrid(float position, float axis)
	{
		return Mathf.RoundToInt(position / 2f + axis / 2f);
	}

	public static SubGrid GetSubGrid(Vector3 position)
	{
		int subGrid = GetSubGrid(position.x, AxisX);
		int subGrid2 = GetSubGrid(position.y, AxisY);
		int subGrid3 = GetSubGrid(position.z, AxisZ);
		if (IsValidSubGrid(subGrid, subGrid2, subGrid3))
		{
			return new SubGrid(subGrid, subGrid2, subGrid3);
		}
		return default(SubGrid);
	}

	public static bool IsBlocked(int x, int y, int z)
	{
		int result;
		int x2 = Math.DivRem(x, 8, out result);
		int result2;
		int y2 = Math.DivRem(y, 8, out result2);
		int result3;
		int z2 = Math.DivRem(z, 8, out result3);
		int gridIndex = GetGridIndex(x2, y2, z2);
		NativeBitArray nativeBitArray = (IsValidGrid(x2, y2, z2) ? OcclusionSubGridBlocked[gridIndex] : default(NativeBitArray));
		int pos = result3 * 8 * 8 + result2 * 8 + result;
		if (nativeBitArray.IsCreated)
		{
			return nativeBitArray.IsSet(pos);
		}
		return false;
	}

	public static bool IsBlocked(SubGrid sub)
	{
		return IsBlocked(sub.x, sub.y, sub.z);
	}

	public static bool IsValidGrid(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
		{
			return false;
		}
		if (x >= ChunkCountX || y >= ChunkCountY || z >= ChunkCountZ)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidSubGrid(int x, int y, int z)
	{
		if (x < 0 || y < 0 || z < 0)
		{
			return false;
		}
		if (x >= SubChunkCountX || y >= SubChunkCountY || z >= SubChunkCountZ)
		{
			return false;
		}
		return true;
	}

	public static void CalculatePathBetweenGrids(SubGrid grid1, SubGrid grid2, out bool pathBlocked)
	{
		pathBlocked = false;
		NativeReference<bool> returnHolder = ReturnHolder;
		CalculatePathBetweenGridsJob calculatePathBetweenGridsJob = default(CalculatePathBetweenGridsJob);
		calculatePathBetweenGridsJob.From = grid1;
		calculatePathBetweenGridsJob.To = grid2;
		calculatePathBetweenGridsJob.PathBlocked = returnHolder;
		calculatePathBetweenGridsJob.Grid = new GridDefinition
		{
			OcclusionSubGridBlocked = OcclusionSubGridBlocked.AsReadOnly(),
			ChunkCount = new int3(ChunkCountX, ChunkCountY, ChunkCountZ),
			SubChunkCount = new int3(SubChunkCountX, SubChunkCountY, SubChunkCountZ)
		};
		calculatePathBetweenGridsJob.BlockedGridThreshold = ConVar.AntiHack.server_occlusion_blocked_grid_threshold;
		calculatePathBetweenGridsJob.NeighbourThreshold = ConVar.AntiHack.server_occlusion_neighbour_threshold;
		calculatePathBetweenGridsJob.UseNeighbourThresholds = ConVar.AntiHack.server_occlusion_use_neighbour_thresholds;
		CalculatePathBetweenGridsJob jobData = calculatePathBetweenGridsJob;
		IJobExtensions.RunByRef(ref jobData);
		pathBlocked = returnHolder.Value;
	}

	public static JobHandle CalculatePathsBetweenGridsJob(NativeArray<(SubGrid from, SubGrid to)>.ReadOnly paths, NativeArray<bool> pathsBlocked)
	{
		CalculatePathsBetweenGridsJob calculatePathsBetweenGridsJob = default(CalculatePathsBetweenGridsJob);
		calculatePathsBetweenGridsJob.Paths = paths;
		calculatePathsBetweenGridsJob.PathsBlocked = pathsBlocked;
		calculatePathsBetweenGridsJob.Grid = new GridDefinition
		{
			OcclusionSubGridBlocked = OcclusionSubGridBlocked.AsReadOnly(),
			ChunkCount = new int3(ChunkCountX, ChunkCountY, ChunkCountZ),
			SubChunkCount = new int3(SubChunkCountX, SubChunkCountY, SubChunkCountZ)
		};
		calculatePathsBetweenGridsJob.BlockedGridThreshold = ConVar.AntiHack.server_occlusion_blocked_grid_threshold;
		calculatePathsBetweenGridsJob.NeighbourThreshold = ConVar.AntiHack.server_occlusion_neighbour_threshold;
		calculatePathsBetweenGridsJob.UseNeighbourThresholds = ConVar.AntiHack.server_occlusion_use_neighbour_thresholds;
		CalculatePathsBetweenGridsJob jobData = calculatePathsBetweenGridsJob;
		return IJobParallelForBatchExtensions.ScheduleBatchByRef(ref jobData, paths.Length, 64);
	}

	public static void SetupGrid()
	{
		Vector3 size = TerrainMeta.Size;
		ChunkCountX = Mathf.Max(Mathf.CeilToInt(size.x / 16f), 1);
		ChunkCountY = Mathf.Max(Mathf.CeilToInt((float)MaxY / 16f), 1);
		ChunkCountZ = Mathf.Max(Mathf.CeilToInt(size.z / 16f), 1);
		SubChunkCountX = Mathf.Max(Mathf.CeilToInt(size.x / 2f), 1);
		SubChunkCountY = Mathf.Max(Mathf.CeilToInt((float)MaxY / 2f), 1);
		SubChunkCountZ = Mathf.Max(Mathf.CeilToInt(size.z / 2f), 1);
		AxisX = TerrainMeta.Size.x / 2f;
		AxisY = MaxY / 2;
		AxisZ = TerrainMeta.Size.z / 2f;
		NativeReferenceEx.SafeDispose(ref ReturnHolder);
		ReturnHolder = new NativeReference<bool>(Allocator.Persistent);
		bool server_occlusion_save_grid = ConVar.AntiHack.server_occlusion_save_grid;
		if (!server_occlusion_save_grid || !ReadGridFromFile(SubGridFilePath))
		{
			GenerateOcclusionGrid();
			if (server_occlusion_save_grid)
			{
				WriteGridToFile(ChunkCountX * ChunkCountY * ChunkCountZ, OcclusionSubGridBlocked);
			}
		}
		foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
		{
			if (OcclusionEnabled && allPlayer.SupportsServerOcclusion())
			{
				allPlayer.SubGrid = GetSubGrid(allPlayer.GetOcclusionOffset());
			}
		}
	}

	public static void Dispose()
	{
		if (OcclusionSubGridBlocked.IsCreated)
		{
			for (int i = 0; i < OcclusionSubGridBlocked.Length; i++)
			{
				NativeBitArray nativeBitArray = OcclusionSubGridBlocked[i];
				if (nativeBitArray.IsCreated)
				{
					nativeBitArray.Dispose();
				}
			}
			OcclusionSubGridBlocked.Dispose();
		}
		if (ReturnHolder.IsCreated)
		{
			ReturnHolder.Dispose();
		}
	}

	private static void WriteGridToFile(int length, NativeArray<NativeBitArray> data)
	{
		try
		{
			using BinaryWriter binaryWriter = new BinaryWriter(File.Open(SubGridFilePath, FileMode.Create));
			binaryWriter.Write(length);
			binaryWriter.Write(OcclusionIncludeRocks);
			foreach (NativeBitArray item in data)
			{
				if (!item.IsCreated)
				{
					binaryWriter.Write(0);
					continue;
				}
				binaryWriter.Write(item.Length);
				byte[] array = new byte[(item.Length + 7) / 8];
				item.AsNativeArray<byte>().CopyTo(array);
				binaryWriter.Write(array);
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.Message);
		}
	}

	public static bool ReadGridFromFile(string path)
	{
		try
		{
			if (!File.Exists(path))
			{
				return false;
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			using (BinaryReader binaryReader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				int num = binaryReader.ReadInt32();
				if (binaryReader.ReadBoolean() != OcclusionIncludeRocks)
				{
					UnityEngine.Debug.LogWarning("Grid file and occlusion parameters don't match, rebuilding grid");
					binaryReader.Close();
					File.Delete(path);
					return false;
				}
				OcclusionSubGridBlocked = new NativeArray<NativeBitArray>(num, Allocator.Persistent);
				for (int i = 0; i < num; i++)
				{
					int num2 = binaryReader.ReadInt32();
					if (num2 != 0)
					{
						byte[] array = binaryReader.ReadBytes((num2 + 7) / 8);
						OcclusionSubGridBlocked[i] = new NativeBitArray(num2, Allocator.Persistent);
						OcclusionSubGridBlocked[i].AsNativeArray<byte>().CopyFrom(array);
					}
				}
				UnityEngine.Debug.Log($"Loaded {num} occlusion sub-chunks from file - took {stopwatch.Elapsed.TotalMilliseconds / 1000.0} seconds");
			}
			return true;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.Message);
			return false;
		}
	}

	[ServerVar(Help = "Tests occlusion visibility between two positions")]
	public static string serverocclusiondebug(ConsoleSystem.Arg arg)
	{
		Vector3 vector = arg.GetVector3(0) + PlayerEyes.EyeOffset;
		Vector3 vector2 = arg.GetVector3(1) + PlayerEyes.EyeOffset;
		SubGrid subGrid = GetSubGrid(vector);
		SubGrid subGrid2 = GetSubGrid(vector2);
		if (subGrid.Equals(default(SubGrid)) || subGrid2.Equals(default(SubGrid)))
		{
			return "Path not blocked due to one of positions being outside of grid";
		}
		NativeList<(int3, Color)> cells = new NativeList<(int3, Color)>(Allocator.Temp);
		bool flag = DebugPath(vector, vector2, cells);
		cells.Dispose();
		return $"Grid 1: {subGrid}, Grid 2: {subGrid2}\nPath blocked: {flag}";
	}

	[ServerVar(Help = "(Generated) Validates that all server occlusion network groups are correctly mapped to their occlusion data; reports any inconsistencies found")]
	public static void OcclusionValidateGroups(ConsoleSystem.Arg arg)
	{
		if (!OcclusionEnabled)
		{
			arg.ReplyWith("ServerOcclusion disabled");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (var (arg2, group3) in Occludees)
		{
			if (stringBuilder.Length > 1024)
			{
				break;
			}
			if (CollectionEx.IsEmpty(group3))
			{
				stringBuilder.AppendLine($"Occlusion group for {arg2} is empty - it should've been cleaned up!");
				continue;
			}
			foreach (BaseNetworkable item in group3)
			{
				if (stringBuilder.Length > 1024)
				{
					break;
				}
				if (item == null)
				{
					stringBuilder.AppendLine($"Occlusion group for {arg2} has a null networkable!");
					continue;
				}
				if (!item.SupportsServerOcclusion())
				{
					stringBuilder.AppendLine($"Occlusion group for {arg2} has a {item} that doesn't support server occlusion!");
					continue;
				}
				foreach (BaseNetworkable occlusionGroupRef in item.OcclusionGroupRefs)
				{
					if (stringBuilder.Length > 1024)
					{
						break;
					}
					if (occlusionGroupRef == null)
					{
						stringBuilder.AppendLine($"Occlusion group for {item} had a null referrer!");
					}
					else if (!occlusionGroupRef.OcclusionGroup.Contains(item))
					{
						stringBuilder.AppendLine($"Occlusion group for referrer-{occlusionGroupRef} of {item} was desynced!");
					}
				}
				Group occlusionGroup = item.OcclusionGroup;
				if (item.net.connection == null)
				{
					if (occlusionGroup.Count != 1 || !occlusionGroup.Contains(item))
					{
						stringBuilder.AppendLine($"Occlusion group for sleeper-{item} has other participants!");
					}
					continue;
				}
				bool flag = false;
				foreach (BaseNetworkable item2 in occlusionGroup)
				{
					if (stringBuilder.Length > 1024)
					{
						break;
					}
					if (item2 == null)
					{
						stringBuilder.AppendLine($"Occlusion group for {item} has a null!");
					}
					else if (item2 == item)
					{
						flag = true;
					}
					else if (!item.net.subscriber.IsSubscribed(item2.net.group))
					{
						stringBuilder.AppendLine($"Occlusion group for {item} has a stale participant!");
					}
				}
				if (!flag)
				{
					stringBuilder.AppendLine($"Occlusion group for {item} doesn't have an owner!");
				}
			}
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (stringBuilder.Length > 1024)
			{
				break;
			}
			if (!(activePlayer == null))
			{
				bool flag2 = activePlayer.SupportsServerOcclusion();
				bool flag3 = activePlayer.OcclusionGroup != null;
				if (flag2 != flag3)
				{
					stringBuilder.AppendLine($"Active {activePlayer} SupportsServerOcclusion:{flag2} but hasLocalGroup: {flag3}");
				}
			}
		}
		foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
		{
			if (stringBuilder.Length > 1024)
			{
				break;
			}
			if (!(sleepingPlayer == null))
			{
				bool flag4 = sleepingPlayer.SupportsServerOcclusion();
				bool flag5 = sleepingPlayer.OcclusionGroup != null;
				if (flag4 != flag5)
				{
					stringBuilder.AppendLine($"Sleeper {sleepingPlayer} SupportsServerOcclusion:{flag4} but hasLocalGroup: {flag5}");
				}
			}
		}
		if (stringBuilder.Length > 0)
		{
			arg.ReplyWith(stringBuilder.ToString());
		}
		else
		{
			arg.ReplyWith($"All {Occludees.Count} server occlusion groups are valid");
		}
	}

	public static bool DebugPath(Vector3 p1, Vector3 p2, NativeList<(int3, Color)> cells)
	{
		SubGrid subGrid = GetSubGrid(p1);
		SubGrid subGrid2 = GetSubGrid(p2);
		GridDefinition gridDefinition = default(GridDefinition);
		gridDefinition.OcclusionSubGridBlocked = OcclusionSubGridBlocked.AsReadOnly();
		gridDefinition.ChunkCount = new int3(ChunkCountX, ChunkCountY, ChunkCountZ);
		gridDefinition.SubChunkCount = new int3(SubChunkCountX, SubChunkCountY, SubChunkCountZ);
		GridDefinition gridDef = gridDefinition;
		int3 from = new int3(subGrid.x, subGrid.y, subGrid.z);
		int3 to = new int3(subGrid2.x, subGrid2.y, subGrid2.z);
		int server_occlusion_blocked_grid_threshold = ConVar.AntiHack.server_occlusion_blocked_grid_threshold;
		int server_occlusion_neighbour_threshold = ConVar.AntiHack.server_occlusion_neighbour_threshold;
		bool server_occlusion_use_neighbour_thresholds = ConVar.AntiHack.server_occlusion_use_neighbour_thresholds;
		return Algorithm.Gather(from, to, in gridDef, server_occlusion_blocked_grid_threshold, server_occlusion_neighbour_threshold, server_occlusion_use_neighbour_thresholds, cells);
	}

	public static bool GetCachedVisibility(SubGrid from, SubGrid to, out bool isVisible)
	{
		int num = from.GetIndex();
		int num2 = to.GetIndex();
		if (num > num2)
		{
			int num3 = num2;
			int num4 = num;
			num = num3;
			num2 = num4;
		}
		return OcclusionCache.TryGetValue((num, num2), out isVisible);
	}

	public static void CacheVisibility(SubGrid from, SubGrid to, bool isVisible)
	{
		int num = from.GetIndex();
		int num2 = to.GetIndex();
		if (num > num2)
		{
			int num3 = num2;
			int num4 = num;
			num = num3;
			num2 = num4;
		}
		OcclusionCache.TryAdd((num, num2), isVisible);
	}

	private static void GenerateOcclusionGrid()
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int num = ChunkCountX * ChunkCountY * ChunkCountZ;
		OcclusionSubGridBlocked = new NativeArray<NativeBitArray>(num, Allocator.Persistent);
		UnityEngine.Debug.Log($"Preparing Occlusion Grid ({SubChunkCountX}, {SubChunkCountY}, {SubChunkCountZ})");
		NativeList<int> cellsToCheck = new NativeList<int>(1024, Allocator.TempJob);
		GenerateOcclusionBroadPhase(cellsToCheck, num);
		int num2 = (cellsToCheck.Length + 128 - 1) / 128;
		NativeList<SubGrid> subGridCells = new NativeList<SubGrid>(65536, Allocator.TempJob);
		UnityEngine.Debug.Log($"Processing {num2} batches({cellsToCheck.Length} broadphase cells total)...");
		for (int i = 0; i < num2; i++)
		{
			_ = stopwatch.Elapsed.TotalSeconds;
			subGridCells.Clear();
			int num3 = i * 128;
			int num4 = Math.Min(num3 + 128, cellsToCheck.Length);
			for (int j = num3; j < num4; j++)
			{
				Grid grid = Grid.FromIndex(cellsToCheck[j]);
				int num5 = grid.x * 8;
				int num6 = grid.y * 8;
				int num7 = grid.z * 8;
				for (int k = 0; k < 8; k++)
				{
					for (int l = 0; l < 8; l++)
					{
						for (int m = 0; m < 8; m++)
						{
							SubGrid value = new SubGrid(m + num5, l + num6, k + num7);
							subGridCells.AddNoResize(value);
						}
					}
				}
			}
			GenerateOcclusionNarrowPhase(subGridCells);
		}
		subGridCells.Dispose();
		cellsToCheck.Dispose();
		UnityEngine.Debug.Log($"Initialized {SubChunkCountX * SubChunkCountY * SubChunkCountZ} occlusion sub-chunks - took {stopwatch.Elapsed.TotalSeconds}s");
	}

	private static void GenerateOcclusionBroadPhase(NativeList<int> cellsToCheck, int chunkTotalCount)
	{
		NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(chunkTotalCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> nativeArray2 = new NativeArray<Vector3>(chunkTotalCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<int> nativeArray3 = new NativeArray<int>(chunkTotalCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		Vector3 value = new Vector3(8f, 8f, 8f);
		for (int i = 0; i < chunkTotalCount; i++)
		{
			nativeArray[i] = Grid.FromIndex(i).GetCenterPoint();
			nativeArray2[i] = value;
			nativeArray3[i] = 8388608;
		}
		NativeArray<bool> results = new NativeArray<bool>(chunkTotalCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GamePhysics.CheckBounds(nativeArray.AsReadOnly(), nativeArray2.AsReadOnly(), nativeArray3.AsReadOnly(), results, QueryTriggerInteraction.Ignore, GamePhysics.MasksToValidate.Terrain);
		nativeArray3.Dispose();
		nativeArray2.Dispose();
		nativeArray.Dispose();
		for (int j = 0; j < results.Length; j++)
		{
			Grid grid = Grid.FromIndex(j);
			bool num = results[j];
			bool flag = false;
			if (grid.y < ChunkCountY - 1)
			{
				int index = new Grid(grid.x, grid.y + 1, grid.z).GetIndex();
				flag = results[index];
			}
			if (OcclusionSubGridBlocked[j].IsCreated)
			{
				OcclusionSubGridBlocked[j].Dispose();
			}
			if (num || flag)
			{
				OcclusionSubGridBlocked[j] = new NativeBitArray(512, Allocator.Persistent);
				cellsToCheck.Add(in j);
			}
		}
		results.Dispose();
	}

	private static void GenerateOcclusionNarrowPhase(NativeList<SubGrid> subGridCells)
	{
		int num = GridOffsets.Length;
		NativeArray<Vector3> posi = new NativeArray<Vector3>(subGridCells.Length * num, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(GridOffsets, Allocator.TempJob);
		if (!subGridCells.IsEmpty)
		{
			CalculateSubGridSamplePointsJob calculateSubGridSamplePointsJob = default(CalculateSubGridSamplePointsJob);
			calculateSubGridSamplePointsJob.Posi = posi;
			calculateSubGridSamplePointsJob.SubGridCells = subGridCells.AsReadOnly();
			calculateSubGridSamplePointsJob.GridOffsets = nativeArray.AsReadOnly();
			calculateSubGridSamplePointsJob.CellOffset = new Vector3(SubGrid.GetOffset(TerrainMeta.Size.x), SubGrid.GetOffset(MaxY), SubGrid.GetOffset(TerrainMeta.Size.z));
			calculateSubGridSamplePointsJob.ScheduleParallel(innerloopBatchCount: ThreadUtils.GetBatchSize(subGridCells.Length), arrayLength: subGridCells.Length, dependency: default(JobHandle)).Complete();
			NativeArray<bool> results = new NativeArray<bool>(posi.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			AntiHack.TestInsideTerrain(posi.AsReadOnly(), results);
			int length = 0;
			for (int i = 0; i < subGridCells.Length; i++)
			{
				SubGrid value = subGridCells[i];
				bool flag = true;
				for (int j = 0; j < num; j++)
				{
					int index = i * num + j;
					flag &= results[index];
				}
				if (flag)
				{
					int result;
					int x = Math.DivRem(value.x, 8, out result);
					int result2;
					int y = Math.DivRem(value.y, 8, out result2);
					int result3;
					int z = Math.DivRem(value.z, 8, out result3);
					int gridIndex = GetGridIndex(x, y, z);
					int pos = result3 * 8 * 8 + result2 * 8 + result;
					OcclusionSubGridBlocked[gridIndex].Set(pos, value: true);
				}
				else
				{
					int num2 = length++;
					subGridCells[num2] = value;
					for (int k = 0; k < num; k++)
					{
						posi[num2 * num + k] = posi[i * num + k];
					}
				}
			}
			subGridCells.ResizeUninitialized(length);
			results.Dispose();
		}
		if (OcclusionIncludeRocks && !subGridCells.IsEmpty)
		{
			NativeArray<Vector3> subArray = posi.GetSubArray(0, subGridCells.Length * num);
			NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(subArray.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			AntiHack.AreInsideMesh(subArray.AsReadOnly(), hits);
			Span<int> span = stackalloc int[num];
			int num3 = 0;
			for (int l = 0; l < subGridCells.Length; l++)
			{
				bool flag2 = true;
				for (int m = 0; m < num; m++)
				{
					RaycastHit raycastHit = hits[l * num + m];
					int colliderInstanceID = raycastHit.colliderInstanceID;
					flag2 = flag2 && colliderInstanceID != 0;
					if (!flag2)
					{
						break;
					}
					bool flag3 = false;
					for (int n = 0; n < num3; n++)
					{
						if (span[n] == colliderInstanceID)
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3)
					{
						GameObject gameObject = raycastHit.collider.gameObject;
						flag2 &= gameObject.HasCustomTag(GameObjectTag.AllowBarricadePlacement);
						if (!flag2)
						{
							break;
						}
						span[num3++] = colliderInstanceID;
					}
				}
				num3 = 0;
				if (flag2)
				{
					SubGrid subGrid = subGridCells[l];
					int result4;
					int x2 = Math.DivRem(subGrid.x, 8, out result4);
					int result5;
					int y2 = Math.DivRem(subGrid.y, 8, out result5);
					int result6;
					int z2 = Math.DivRem(subGrid.z, 8, out result6);
					int gridIndex2 = GetGridIndex(x2, y2, z2);
					int pos2 = result6 * 8 * 8 + result5 * 8 + result4;
					OcclusionSubGridBlocked[gridIndex2].Set(pos2, value: true);
				}
			}
			hits.Dispose();
		}
		nativeArray.Dispose();
		posi.Dispose();
	}
}
