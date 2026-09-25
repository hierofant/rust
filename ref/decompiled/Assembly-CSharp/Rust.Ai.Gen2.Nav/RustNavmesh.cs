using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2.Nav;

public class RustNavmesh : IDisposable
{
	private static Vector3[] TilePolysBuffer = new Vector3[12288];

	private static Vector3[] PathBuffer = new Vector3[256];

	private static Vector3[] CornerBuffer = new Vector3[256];

	private static Vector3[] DonutPointsBuffer = new Vector3[64];

	public NavMeshBuildParams BuildParams = new NavMeshBuildParams(dummy: true);

	public NavMeshBuildParams BuildParamsHiRes = new NavMeshBuildParams(dummy: true);

	public int PathfindingMaxIterations = 1000;

	public Bounds CurrentNavmeshBounds;

	public Tile[] tiles;

	public IntPtr NavMeshHandle = IntPtr.Zero;

	public string debugName = "unnamed";

	public long workerBuildTicks;

	public double lastFullBuildSeconds = -1.0;

	private float cachedMaxBorderMeters;

	private double builtStartTime;

	private int numBuiltTiles;

	private Vector2Int tileNum;

	private BackgroundTileBuilder tileBuilder;

	public bool EmitTileChangeEvents;

	public bool ForceHiRes;

	public int NumBuiltTiles => numBuiltTiles;

	public int TotalTiles
	{
		get
		{
			if (tiles == null)
			{
				return 0;
			}
			return tiles.Length;
		}
	}

	public bool CullTilesFarFromShore { get; private set; }

	public int TileChangeVersion { get; private set; }

	public bool IsValid()
	{
		return NavMeshHandle != IntPtr.Zero;
	}

	public RustNavmesh(BackgroundTileBuilder tileBuilder, NavMeshBuildParams? buildParamsOverride = null, NavMeshBuildParams? buildParamsHiResOverride = null, Bounds? boundsOverride = null, bool shouldBuild = true, bool synchronous = false, bool forceHiRes = false, bool cullTilesFarFromShore = false)
	{
		if (AI.useUnityNavmesh)
		{
			return;
		}
		if (tileBuilder == null)
		{
			RustNavigation.LogError("BackgroundTileBuilder is required to create a RustNavmesh");
			return;
		}
		this.tileBuilder = tileBuilder;
		ForceHiRes = forceHiRes;
		CullTilesFarFromShore = cullTilesFarFromShore;
		if (boundsOverride.HasValue)
		{
			CurrentNavmeshBounds = boundsOverride.Value;
		}
		else
		{
			CurrentNavmeshBounds = new Bounds(TerrainMeta.Center, TerrainMeta.Size);
		}
		if (buildParamsOverride.HasValue)
		{
			BuildParams = buildParamsOverride.Value;
		}
		else
		{
			BuildParams = RustNavigation.Instance.BuildParams;
		}
		if (buildParamsHiResOverride.HasValue)
		{
			BuildParamsHiRes = buildParamsHiResOverride.Value;
		}
		else
		{
			BuildParamsHiRes = RustNavigation.Instance.BuildParamsHiRes;
		}
		cachedMaxBorderMeters = Mathf.Max(BorderMeters(in BuildParams), BorderMeters(in BuildParamsHiRes));
		float num = BuildParams.tileSize * BuildParams.cellSize;
		float num2 = BuildParamsHiRes.tileSize * BuildParamsHiRes.cellSize;
		if (Mathf.Abs(num - num2) > 0.001f)
		{
			RustNavigation.LogError($"Tile world size mismatch: lo {num:F6} hi {num2:F6}");
			return;
		}
		ref NavMeshBuildParams buildParams = ref BuildParams;
		Vector3 bmin = CurrentNavmeshBounds.min;
		Vector3 bmax = CurrentNavmeshBounds.max;
		NavMeshHandle = RecastWrapper.CreateEmptyNavMesh(in buildParams, in bmin, in bmax);
		if (NavMeshHandle == IntPtr.Zero)
		{
			RustNavigation.LogError("Failed to create empty navmesh");
			Dispose();
			return;
		}
		tileNum = rcCalcTileNum();
		tiles = new Tile[tileNum.x * tileNum.y];
		for (int i = 0; i < tileNum.y; i++)
		{
			for (int j = 0; j < tileNum.x; j++)
			{
				Tile tile = new Tile(j, i);
				tiles[Mathx.FlattenArrayCoord(j, i, tileNum.x)] = tile;
			}
		}
		if (!shouldBuild)
		{
			return;
		}
		builtStartTime = UnityEngine.Time.realtimeSinceStartupAsDouble;
		int num3 = 0;
		for (int k = 0; k < tileNum.y; k++)
		{
			for (int l = 0; l < tileNum.x; l++)
			{
				if (!tileBuilder.EnqueueOnMainThread(this, l, k, synchronous))
				{
					num3++;
				}
			}
		}
		if (num3 > 0)
		{
			RustNavigation.Log($"Dropped {num3} of {tiles.Length} tiles for sitting more than {RustNav.maxShoreDistance:0.#}m out to sea.");
		}
	}

	private RustNavmesh(BackgroundTileBuilder tileBuilder, IntPtr loadedHandle, in ManagedNavPayload payload, bool cullTilesFarFromShore)
	{
		if (AI.useUnityNavmesh)
		{
			return;
		}
		this.tileBuilder = tileBuilder;
		CullTilesFarFromShore = cullTilesFarFromShore;
		CurrentNavmeshBounds = payload.currentNavmeshBounds;
		BuildParams = payload.buildParams;
		BuildParamsHiRes = payload.buildParamsHiRes;
		cachedMaxBorderMeters = Mathf.Max(BorderMeters(in BuildParams), BorderMeters(in BuildParamsHiRes));
		NavMeshHandle = loadedHandle;
		Vector2Int vector2Int = rcCalcTileNum();
		if (payload.tileNum != vector2Int)
		{
			RustNavigation.LogError($"Loaded navmesh tile grid {payload.tileNum} does not match bounds/params ({vector2Int})");
			NavMeshHandle = IntPtr.Zero;
			return;
		}
		tileNum = payload.tileNum;
		tiles = new Tile[tileNum.x * tileNum.y];
		for (int i = 0; i < tileNum.y; i++)
		{
			for (int j = 0; j < tileNum.x; j++)
			{
				tiles[Mathx.FlattenArrayCoord(j, i, tileNum.x)] = new Tile(j, i);
			}
		}
	}

	public void SetTileBuilder(BackgroundTileBuilder tileBuilder)
	{
		this.tileBuilder = tileBuilder;
	}

	public bool IsBuilt()
	{
		if (NavMeshHandle == IntPtr.Zero)
		{
			return false;
		}
		return numBuiltTiles == tiles.Length;
	}

	private void MarkTileAsBuilt(Tile tile)
	{
		if (tile == null)
		{
			return;
		}
		TileChangeVersion++;
		if (EmitTileChangeEvents)
		{
			RustNavigation.NotifyDefaultNavmeshTileChanged(tile.tx, tile.ty);
		}
		if (!tile.wasBuiltOnce)
		{
			numBuiltTiles++;
			tile.wasBuiltOnce = true;
			if (IsBuilt())
			{
				lastFullBuildSeconds = UnityEngine.Time.realtimeSinceStartupAsDouble - builtStartTime;
				RustNavigation.Log($"Navmesh '{debugName}' is now fully built in {lastFullBuildSeconds:F2} seconds ({numBuiltTiles} tiles).");
			}
		}
	}

	public void FailTile(int tx, int ty)
	{
		if (tiles == null)
		{
			return;
		}
		Tile tile = GetTile(tx, ty);
		if (tile == null)
		{
			RustNavigation.LogError($"FailTile: tile coordinates out of range: {tx},{ty}");
			return;
		}
		if (tile.hasData && NavMeshHandle != IntPtr.Zero)
		{
			RecastWrapper.RemoveTileFromNavMesh(NavMeshHandle, tx, ty);
		}
		tile.hasData = false;
		MarkTileAsBuilt(tile);
	}

	public bool AddTile(int tx, int ty, IntPtr tileData, int dataSize)
	{
		if (tiles == null)
		{
			return false;
		}
		if (!RecastWrapper.AddPrebuiltTileToNavMesh(NavMeshHandle, tx, ty, tileData, dataSize))
		{
			FailTile(tx, ty);
			return false;
		}
		Tile tile = GetTile(tx, ty);
		if (tile == null)
		{
			RustNavigation.LogError($"AddTile: tile coordinates out of range: {tx},{ty}");
			return false;
		}
		tile.hasData = true;
		MarkTileAsBuilt(tile);
		return true;
	}

	public Tile GetTile(int tx, int ty)
	{
		if (tx < 0 || ty < 0 || tx >= tileNum.x || ty >= tileNum.y)
		{
			return null;
		}
		return tiles[Mathx.FlattenArrayCoord(tx, ty, tileNum.x)];
	}

	public void GetTilesInBounds(Bounds bounds, List<Vector2Int> tiles)
	{
		tiles.Clear();
		Vector2Int vector2Int = rcCalcTileCoordFromPos(bounds.min);
		Vector2Int vector2Int2 = rcCalcTileCoordFromPos(bounds.max);
		for (int i = vector2Int.x; i <= vector2Int2.x; i++)
		{
			for (int j = vector2Int.y; j <= vector2Int2.y; j++)
			{
				tiles.Add(new Vector2Int(i, j));
			}
		}
	}

	public void RebuildTilesInBounds(Bounds rebuildBounds, bool synchronous)
	{
		using (TimeWarning.New("RustNavmesh.RebuildTilesInBounds"))
		{
			if (!IsValid())
			{
				return;
			}
			rebuildBounds = rcExpandTileBounds(rebuildBounds);
			if (!CurrentNavmeshBounds.Intersects(rebuildBounds))
			{
				return;
			}
			Vector2Int vector2Int = rcCalcTileCoordFromPos(rebuildBounds.min);
			Vector2Int vector2Int2 = rcCalcTileCoordFromPos(rebuildBounds.max);
			for (int i = vector2Int.x; i <= vector2Int2.x; i++)
			{
				for (int j = vector2Int.y; j <= vector2Int2.y; j++)
				{
					tileBuilder.EnqueueOnMainThread(this, i, j, synchronous);
				}
			}
		}
	}

	public Vector2Int rcCalcTileCoordFromPos(Vector3 pos)
	{
		float num = BuildParams.tileSize * BuildParams.cellSize;
		int value = Mathf.FloorToInt((pos.x - CurrentNavmeshBounds.min.x) / num);
		int value2 = Mathf.FloorToInt((pos.z - CurrentNavmeshBounds.min.z) / num);
		int x = Mathf.Clamp(value, 0, tileNum.x - 1);
		value2 = Mathf.Clamp(value2, 0, tileNum.y - 1);
		return new Vector2Int(x, value2);
	}

	private Vector2Int rcCalcTileNum()
	{
		int num = (int)((CurrentNavmeshBounds.max.x - CurrentNavmeshBounds.min.x) / BuildParams.cellSize + 0.5f);
		int num2 = (int)((CurrentNavmeshBounds.max.z - CurrentNavmeshBounds.min.z) / BuildParams.cellSize + 0.5f);
		int x = (int)(((float)num + BuildParams.tileSize - 1f) / BuildParams.tileSize);
		int y = (int)(((float)num2 + BuildParams.tileSize - 1f) / BuildParams.tileSize);
		return new Vector2Int(x, y);
	}

	public Bounds rcCalcTileBounds(Vector2Int tileCoord)
	{
		float num = BuildParams.tileSize * BuildParams.cellSize;
		Vector3 vector = new Vector3(CurrentNavmeshBounds.min.x + (float)tileCoord.x * num, CurrentNavmeshBounds.min.y, CurrentNavmeshBounds.min.z + (float)tileCoord.y * num);
		Vector3 vector2 = new Vector3(CurrentNavmeshBounds.min.x + (float)(tileCoord.x + 1) * num, CurrentNavmeshBounds.max.y, CurrentNavmeshBounds.min.z + (float)(tileCoord.y + 1) * num);
		return new Bounds((vector + vector2) * 0.5f, vector2 - vector);
	}

	private static float BorderMeters(in NavMeshBuildParams p)
	{
		return (float)(Mathf.CeilToInt(p.agentRadius / p.cellSize) + 3) * p.cellSize;
	}

	public Bounds rcExpandTileBounds(Bounds tileBounds)
	{
		Vector3 vector = new Vector3(cachedMaxBorderMeters, 0f, cachedMaxBorderMeters);
		tileBounds.min -= vector;
		tileBounds.max += vector;
		return tileBounds;
	}

	public bool IsTileFarFromShore(int tx, int ty)
	{
		if (!CullTilesFarFromShore)
		{
			return false;
		}
		float maxShoreDistance = RustNav.maxShoreDistance;
		if (maxShoreDistance <= 0f)
		{
			return false;
		}
		TerrainTexturing texturing = TerrainMeta.Texturing;
		if (texturing == null || !texturing.TexturesInitialized)
		{
			return false;
		}
		Bounds worldBounds = rcExpandTileBounds(rcCalcTileBounds(new Vector2Int(tx, ty)));
		float coarseDistanceToShore = texturing.GetCoarseDistanceToShore(worldBounds.center);
		if (!float.IsFinite(coarseDistanceToShore))
		{
			return false;
		}
		float magnitude = new Vector2(worldBounds.extents.x, worldBounds.extents.z).magnitude;
		if (coarseDistanceToShore - magnitude <= maxShoreDistance)
		{
			return false;
		}
		if (!RustNavigation.HasTunnelRegions || RustNavigation.Instance == null)
		{
			return true;
		}
		return !RustNavigation.Instance.IsInTunnelRegion(worldBounds);
	}

	public bool GetTilePolysInternal(int tx, int ty, List<Vector3> polys)
	{
		using (TimeWarning.New("RustNavmesh.GetTilePolysInternal"))
		{
			using (TimeWarning.New("ClearBuffer"))
			{
				for (int i = 0; i < TilePolysBuffer.Length; i++)
				{
					TilePolysBuffer[i] = Vector3.zero;
				}
			}
			if (!IsValid())
			{
				return false;
			}
			if (!RecastWrapper.GetTilePolys(NavMeshHandle, tx, ty, TilePolysBuffer, 2048, out var outPolyCount))
			{
				return false;
			}
			using (TimeWarning.New("ApplyPolys"))
			{
				for (int j = 0; j < outPolyCount * 6; j++)
				{
					polys.Add(TilePolysBuffer[j]);
				}
			}
			return true;
		}
	}

	private bool FillPathFromPathBuffer(List<NavVector3> path, int pathCount)
	{
		if (!IsValid())
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("NavMesh has not been built yet.");
			}
			return false;
		}
		path.Clear();
		path.Capacity = Mathf.Max(path.Capacity, pathCount);
		for (int i = 0; i < pathCount; i++)
		{
			path.Add(new NavVector3(PathBuffer[i]));
		}
		return true;
	}

	public bool SamplePosition(NavVector3 position, out NavHit hit, Vector3 extents)
	{
		ulong nearestPolyRef;
		return SamplePositionPoly(position, out hit, extents, out nearestPolyRef);
	}

	public bool SamplePositionPoly(NavVector3 position, out NavHit hit, Vector3 extents, out ulong nearestPolyRef)
	{
		using (TimeWarning.New("RustNavmesh.SamplePosition"))
		{
			hit = default(NavHit);
			nearestPolyRef = 0uL;
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.SamplePosition(NavMeshHandle, in position.Value, in extents, out var nearestPosition, out nearestPolyRef))
			{
				return false;
			}
			if (nearestPosition == Vector3.zero)
			{
				return false;
			}
			hit = new NavHit
			{
				position = new NavVector3(nearestPosition)
			};
			return true;
		}
	}

	public bool Raycast(NavVector3 startPos, NavVector3 endPos, out NavHit hit)
	{
		ulong startRef = 0uL;
		return Raycast(ref startRef, startPos, endPos, out hit);
	}

	public bool Raycast(ref ulong startRef, NavVector3 startPos, NavVector3 endPos, out NavHit hit)
	{
		using (TimeWarning.New("RustNavmesh.Raycast"))
		{
			hit = default(NavHit);
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.Raycast(NavMeshHandle, ref startRef, in startPos.Value, in endPos.Value, out var hitLocation, out var hitNormal))
			{
				return false;
			}
			hit = new NavHit
			{
				position = new NavVector3(hitLocation),
				normal = new NavVector3(hitNormal)
			};
			return true;
		}
	}

	public bool Move(ref ulong polyRef, NavVector3 startPos, NavVector3 endPos, out NavVector3 movedPos)
	{
		using (TimeWarning.New("RustNavmesh.Move"))
		{
			movedPos = startPos;
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.Move(NavMeshHandle, ref polyRef, in startPos.Value, in endPos.Value, out var movedPos2))
			{
				return false;
			}
			movedPos = new NavVector3(movedPos2);
			return true;
		}
	}

	public bool CalculatePath(NavVector3 start, NavVector3 end, RustNavMeshPath path)
	{
		ulong startRef = 0uL;
		return CalculatePath(ref startRef, start, end, path);
	}

	public bool CalculatePath(ref ulong startRef, NavVector3 start, NavVector3 end, RustNavMeshPath path)
	{
		path.Reset();
		if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
		{
			return false;
		}
		int pathLength;
		DtStatus dtStatus = RecastWrapper.FindPath(NavMeshHandle, ref startRef, in start.Value, in end.Value, PathBuffer, out pathLength, path.polyRefs, out path.polyRefCount, PathfindingMaxIterations);
		if (((uint)dtStatus & 0x80000000u) == 2147483648u)
		{
			return false;
		}
		if (pathLength <= 0)
		{
			return false;
		}
		if (!FillPathFromPathBuffer(path.corners, pathLength))
		{
			return false;
		}
		path.status = (((dtStatus & DtStatus.PartialResult) == DtStatus.PartialResult) ? NavMeshPathStatus.PathPartial : NavMeshPathStatus.PathComplete);
		return true;
	}

	public bool IsValidPolyRef(ulong polyRef)
	{
		if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
		{
			return false;
		}
		return RecastWrapper.IsValidPolyRef(NavMeshHandle, polyRef);
	}

	public bool CorridorMove(IntPtr corridor, NavVector3 desiredPos, out NavVector3 resultPos, out ulong firstPolyRef)
	{
		using (TimeWarning.New("RustNavmesh.CorridorMove"))
		{
			resultPos = desiredPos;
			firstPolyRef = 0uL;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorMove(NavMeshHandle, corridor, in desiredPos.Value, out var resultPos2, out firstPolyRef))
			{
				return false;
			}
			resultPos = new NavVector3(resultPos2);
			return true;
		}
	}

	public bool CorridorOptimizeAndMove(IntPtr corridor, NavVector3 optimizeNextNS, float optimizationRange, NavVector3 desiredPosNS, out NavVector3 resultPosNS)
	{
		using (TimeWarning.New("RustNavmesh.CorridorOptimizeAndMove"))
		{
			resultPosNS = desiredPosNS;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorOptimizeAndMove(NavMeshHandle, corridor, in optimizeNextNS.Value, optimizationRange, in desiredPosNS.Value, out var resultPos, out var _))
			{
				return false;
			}
			resultPosNS = new NavVector3(resultPos);
			return true;
		}
	}

	public bool CorridorMoveTargetPosition(IntPtr corridor, NavVector3 desiredTargetNS, out NavVector3 resultTargetNS)
	{
		using (TimeWarning.New("RustNavmesh.CorridorMoveTargetPosition"))
		{
			resultTargetNS = desiredTargetNS;
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			if (!RecastWrapper.CorridorMoveTargetPosition(NavMeshHandle, corridor, in desiredTargetNS.Value, out var resultTarget))
			{
				return false;
			}
			resultTargetNS = new NavVector3(resultTarget);
			return true;
		}
	}

	public int CorridorFindCorners(IntPtr corridor, List<NavVector3> corners, int maxCorners, out bool endReached)
	{
		using (TimeWarning.New("RustNavmesh.CorridorFindCorners"))
		{
			endReached = false;
			corners.Clear();
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return 0;
			}
			int num = RecastWrapper.CorridorFindCorners(NavMeshHandle, corridor, CornerBuffer, maxCorners, out endReached);
			for (int i = 0; i < num; i++)
			{
				corners.Add(new NavVector3(CornerBuffer[i]));
			}
			return num;
		}
	}

	public bool CorridorIsValid(IntPtr corridor, int maxLookAhead)
	{
		using (TimeWarning.New("RustNavmesh.CorridorIsValid"))
		{
			if (!RustNavigation.EnsureNewNavmesh() || !IsValid())
			{
				return false;
			}
			return RecastWrapper.CorridorIsValid(NavMeshHandle, corridor, maxLookAhead);
		}
	}

	public void CorridorOptimizeVisibility(IntPtr corridor, NavVector3 next, float optimizationRange)
	{
		using (TimeWarning.New("RustNavmesh.CorridorOptimizeVisibility"))
		{
			if (RustNavigation.EnsureNewNavmesh() && IsValid())
			{
				RecastWrapper.CorridorOptimizeVisibility(NavMeshHandle, corridor, in next.Value, optimizationRange);
			}
		}
	}

	public bool FindDistanceToWall(ref ulong startRef, NavVector3 centerPos, float maxRadius, out NavHit hit)
	{
		using (TimeWarning.New("RustNavmesh.FindDistanceToWall"))
		{
			hit = default(NavHit);
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			if (!RecastWrapper.FindDistanceToWall(NavMeshHandle, ref startRef, in centerPos.Value, maxRadius, out var hitDistance, out var hitLocation, out var hitNormal))
			{
				return false;
			}
			hit = new NavHit
			{
				position = new NavVector3(hitLocation),
				normal = new NavVector3(hitNormal),
				distance = hitDistance,
				hit = true
			};
			return true;
		}
	}

	public bool FindDonutPointsInCircle(ref ulong startRef, NavVector3 centerNS, float maxRadius, float minRadius, float angleOffset, int count, List<NavVector3> resultsNS)
	{
		using (TimeWarning.New("RustNavmesh.FindDonutPointsInCircle"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError("NavMesh has not been built yet.");
				}
				return false;
			}
			count = Mathf.Min(count, 64);
			if (!RecastWrapper.FindDonutPointsInCircle(NavMeshHandle, ref startRef, in centerNS.Value, maxRadius, minRadius, angleOffset, count, DonutPointsBuffer, out var numFound))
			{
				return false;
			}
			for (int i = 0; i < numFound; i++)
			{
				resultsNS.Add(new NavVector3(DonutPointsBuffer[i]));
			}
			return numFound > 0;
		}
	}

	public unsafe bool Save(string path)
	{
		using (TimeWarning.New("RustNavmesh.Save"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			long timestamp = Stopwatch.GetTimestamp();
			if (!IsValid())
			{
				RustNavigation.Log("Navmesh not built, nothing to save.");
				return false;
			}
			using PooledList<(int, int)> pooledList = Facepunch.Pool.Get<PooledList<(int, int)>>();
			tileBuilder.GetPendingTilesForNavmeshOnMainThread(this, pooledList);
			int num = sizeof(ManagedNavPayload) + pooledList.Count * 4 * 2;
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			bool flag;
			try
			{
				ManagedNavPayload managedNavPayload = default(ManagedNavPayload);
				managedNavPayload.payloadVersion = 5;
				managedNavPayload.buildParams = BuildParams;
				managedNavPayload.buildParamsHiRes = BuildParamsHiRes;
				managedNavPayload.currentNavmeshBounds = CurrentNavmeshBounds;
				managedNavPayload.tileNum = tileNum;
				managedNavPayload.pendingTileCount = pooledList.Count;
				ManagedNavPayload managedNavPayload2 = managedNavPayload;
				*(ManagedNavPayload*)(void*)intPtr = managedNavPayload2;
				int* ptr = (int*)((byte*)(void*)intPtr + sizeof(ManagedNavPayload));
				foreach (var (num2, num3) in pooledList)
				{
					*(ptr++) = num2;
					*(ptr++) = num3;
				}
				int num4 = 2;
				if (RustNav.saveCompression)
				{
					num4 |= 1;
				}
				IntPtr navMeshHandle = NavMeshHandle;
				ref NavMeshBuildParams buildParams = ref BuildParams;
				Vector3 bmin = CurrentNavmeshBounds.min;
				Vector3 bmax = CurrentNavmeshBounds.max;
				flag = RecastWrapper.SaveNavMesh(path, navMeshHandle, in buildParams, in bmin, in bmax, intPtr, num, num4, RustNav.saveThreads);
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (!flag)
			{
				RustNavigation.LogError("Failed to save navmesh to " + path);
				return false;
			}
			double num5 = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
			RustNavigation.Log($"Successfully saved navmesh ({pooledList.Count} pending tiles) in {num5} ms");
			return true;
		}
	}

	public unsafe static RustNavmesh Load(string path, BackgroundTileBuilder tileBuilder, bool synchronous = false, bool cullTilesFarFromShore = false)
	{
		using (TimeWarning.New("RustNavmesh.Load"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return null;
			}
			long timestamp = Stopwatch.GetTimestamp();
			IntPtr managedBlob;
			int managedBlobSize;
			IntPtr intPtr = RecastWrapper.LoadNavMesh(path, out managedBlob, out managedBlobSize, RustNav.saveThreads);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			RustNavmesh rustNavmesh = null;
			try
			{
				if (managedBlob == IntPtr.Zero || managedBlobSize < sizeof(ManagedNavPayload))
				{
					RustNavigation.LogError($"Navmesh file has an invalid managed payload ({managedBlobSize} bytes)");
					return null;
				}
				ManagedNavPayload payload = *(ManagedNavPayload*)(void*)managedBlob;
				if (payload.payloadVersion != 5)
				{
					RustNavigation.LogWarning($"Saved navmesh is payload version {payload.payloadVersion}, this build wants {5}. Rebuilding from scratch.");
					return null;
				}
				if (payload.pendingTileCount < 0 || managedBlobSize != sizeof(ManagedNavPayload) + payload.pendingTileCount * 4 * 2)
				{
					RustNavigation.LogError($"Managed payload size mismatch ({managedBlobSize} bytes for {payload.pendingTileCount} pending tiles)");
					return null;
				}
				if (payload.tileNum.x <= 0 || payload.tileNum.y <= 0)
				{
					RustNavigation.LogError($"Invalid tile dimensions: {payload.tileNum.x}x{payload.tileNum.y}");
					return null;
				}
				rustNavmesh = new RustNavmesh(tileBuilder, intPtr, in payload, cullTilesFarFromShore);
				if (!rustNavmesh.IsValid())
				{
					rustNavmesh = null;
					return null;
				}
				intPtr = IntPtr.Zero;
				int navMeshTileCoords = RecastWrapper.GetNavMeshTileCoords(rustNavmesh.NavMeshHandle, IntPtr.Zero, 0);
				if (navMeshTileCoords > 0)
				{
					IntPtr intPtr2 = Marshal.AllocHGlobal(navMeshTileCoords * 4 * 2);
					try
					{
						RecastWrapper.GetNavMeshTileCoords(rustNavmesh.NavMeshHandle, intPtr2, navMeshTileCoords);
						int* ptr = (int*)(void*)intPtr2;
						for (int i = 0; i < navMeshTileCoords; i++)
						{
							int num = *(ptr++);
							int num2 = *(ptr++);
							Tile tile = rustNavmesh.GetTile(num, num2);
							if (tile == null)
							{
								RustNavigation.LogError($"Loaded tile {num},{num2} is outside the tile grid");
								return null;
							}
							tile.hasData = true;
							rustNavmesh.MarkTileAsBuilt(tile);
						}
					}
					finally
					{
						Marshal.FreeHGlobal(intPtr2);
					}
				}
				int* ptr2 = (int*)((byte*)(void*)managedBlob + sizeof(ManagedNavPayload));
				for (int j = 0; j < payload.pendingTileCount; j++)
				{
					int num3 = *(ptr2++);
					int num4 = *(ptr2++);
					if (num3 < 0 || num4 < 0 || num3 >= payload.tileNum.x || num4 >= payload.tileNum.y)
					{
						RustNavigation.LogError($"Invalid pending tile coordinates: {num3},{num4} (max: {payload.tileNum.x - 1},{payload.tileNum.y - 1})");
						return null;
					}
					tileBuilder.EnqueueOnMainThread(rustNavmesh, num3, num4, synchronous);
				}
				using PooledList<(int, int)> pooledList = Facepunch.Pool.Get<PooledList<(int, int)>>();
				tileBuilder.GetPendingTilesForNavmeshOnMainThread(rustNavmesh, pooledList);
				using PooledHashSet<(int, int)> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<(int, int)>>();
				foreach (var item in pooledList)
				{
					pooledHashSet.Add(item);
				}
				for (int k = 0; k < payload.tileNum.y; k++)
				{
					for (int l = 0; l < payload.tileNum.x; l++)
					{
						Tile tile2 = rustNavmesh.GetTile(l, k);
						if (rustNavmesh.IsTileFarFromShore(l, k))
						{
							rustNavmesh.FailTile(l, k);
						}
						else if ((tile2 == null || !tile2.hasData) && !pooledHashSet.Contains((l, k)))
						{
							rustNavmesh.FailTile(l, k);
						}
					}
				}
				double num5 = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
				RustNavigation.Log($"Successfully loaded navmesh with {navMeshTileCoords} tiles in {num5} ms");
				RustNavmesh result = rustNavmesh;
				rustNavmesh = null;
				return result;
			}
			catch (Exception ex)
			{
				RustNavigation.LogError("Failed to load navmesh: " + ex.Message);
				return null;
			}
			finally
			{
				rustNavmesh?.Dispose();
				if (intPtr != IntPtr.Zero)
				{
					RecastWrapper.DestroyNavMesh(intPtr);
				}
				if (managedBlob != IntPtr.Zero)
				{
					RecastWrapper.FreeManagedBlob(managedBlob);
				}
			}
		}
	}

	public bool FillDebugDrawProto(ProtoBuf.NavMeshData navMeshData, Bounds bounds, Matrix4x4? transform = null, Vector3? sectionPivot = null, Vector3 sectionSign = default(Vector3))
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return false;
		}
		if (!IsValid())
		{
			return false;
		}
		using PooledList<Vector2Int> pooledList = Facepunch.Pool.Get<PooledList<Vector2Int>>();
		GetTilesInBounds(bounds, pooledList);
		using PooledList<Vector3> pooledList3 = Facepunch.Pool.Get<PooledList<Vector3>>();
		foreach (Vector2Int item in pooledList)
		{
			using PooledList<Vector3> pooledList2 = Facepunch.Pool.Get<PooledList<Vector3>>();
			GetTilePolysInternal(item.x, item.y, pooledList2);
			if (!sectionPivot.HasValue)
			{
				pooledList3.AddRange(pooledList2);
				continue;
			}
			Vector3 value = sectionPivot.Value;
			for (int i = 0; i < pooledList2.Count; i += 6)
			{
				Vector3 zero = Vector3.zero;
				int num = 0;
				for (int j = 0; j < 6; j++)
				{
					Vector3 vector = pooledList2[i + j];
					if (vector == Vector3.zero)
					{
						break;
					}
					zero += vector;
					num++;
				}
				if (num == 0)
				{
					continue;
				}
				Vector3 vector2 = zero / num;
				float num2 = ((vector2.x >= value.x) ? 1f : (-1f));
				float num3 = ((vector2.z >= value.z) ? 1f : (-1f));
				if (num2 == sectionSign.x && num3 == sectionSign.z)
				{
					for (int k = 0; k < 6; k++)
					{
						pooledList3.Add(pooledList2[i + k]);
					}
				}
			}
		}
		for (int l = 0; l < pooledList3.Count; l += 6)
		{
			VectorList vectorList = Facepunch.Pool.Get<VectorList>();
			vectorList.vectorPoints = Facepunch.Pool.Get<List<Vector3>>();
			for (int m = 0; m < 6; m++)
			{
				Vector3 vector3 = pooledList3[l + m];
				if (vector3 == Vector3.zero)
				{
					break;
				}
				if (transform.HasValue)
				{
					vector3 = transform.Value.MultiplyPoint3x4(vector3);
				}
				vectorList.vectorPoints.Add(vector3);
			}
			navMeshData.polygons.Add(vectorList);
		}
		return true;
	}

	public bool FillDebugDrawProtoForTile(ProtoBuf.NavMeshData navMeshData, int tx, int ty, Matrix4x4? transform = null)
	{
		using (TimeWarning.New("RustNavmesh.FillDebugDrawProtoForTile"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (!IsValid())
			{
				return false;
			}
			using PooledList<Vector3> pooledList = Facepunch.Pool.Get<PooledList<Vector3>>();
			GetTilePolysInternal(tx, ty, pooledList);
			for (int i = 0; i < pooledList.Count; i += 6)
			{
				VectorList vectorList = Facepunch.Pool.Get<VectorList>();
				vectorList.vectorPoints = Facepunch.Pool.Get<List<Vector3>>();
				for (int j = 0; j < 6; j++)
				{
					Vector3 vector = pooledList[i + j];
					if (vector == Vector3.zero)
					{
						break;
					}
					if (transform.HasValue)
					{
						vector = transform.Value.MultiplyPoint3x4(vector);
					}
					vectorList.vectorPoints.Add(vector);
				}
				navMeshData.polygons.Add(vectorList);
			}
			return true;
		}
	}

	public void Dispose()
	{
		RustNavigation.Log("Disposing navmesh...");
		tileBuilder.CancelPendingTilesForOnMainThread(this);
		if (NavMeshHandle != IntPtr.Zero)
		{
			RecastWrapper.DestroyNavMesh(NavMeshHandle);
			NavMeshHandle = IntPtr.Zero;
		}
		tiles = null;
	}
}
