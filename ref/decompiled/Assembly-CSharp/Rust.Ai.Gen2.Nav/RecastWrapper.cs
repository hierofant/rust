using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public static class RecastWrapper
{
	public delegate void LogCallback(string message);

	public const int MAX_PATH_SIZE = 256;

	public const int DT_VERTS_PER_POLYGON = 6;

	public const int MAX_POLYS_PER_TILE = 2048;

	private const string DLLName = "RustNative";

	public const int MAX_DONUT_POINTS = 64;

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool GetTilePolys(IntPtr navWrapper, int tx, int ty, [In][Out] Vector3[] outVertices, int maxPolys, out int outPolyCount);

	[DllImport("RustNative")]
	public static extern void DestroyNavMesh(IntPtr navMesh);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool SamplePosition(IntPtr navMesh, in Vector3 position, in Vector3 extents, out Vector3 nearestPosition, out ulong nearestPolyRef);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool Raycast(IntPtr navMesh, ref ulong startRef, in Vector3 startPos, in Vector3 endPos, out Vector3 hitLocation, out Vector3 hitNormal);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool Move(IntPtr navMesh, ref ulong polyRef, in Vector3 startPos, in Vector3 endPos, out Vector3 movedPos);

	[DllImport("RustNative")]
	public static extern DtStatus FindPath(IntPtr navMesh, ref ulong startRef, in Vector3 start, in Vector3 end, [Out] Vector3[] path, out int pathLength, [Out] ulong[] pathPolys, out int pathPolyCount, int maxIterations);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FindDistanceToWall(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxRadius, out float hitDistance, out Vector3 hitLocation, out Vector3 hitNormal);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FindDonutPointsInCircle(IntPtr navMesh, ref ulong startRef, in Vector3 centerPos, float maxRadius, float minRadius, float angleOffset, int maxPoints, [Out] Vector3[] points, out int numFound);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool IsValidPolyRef(IntPtr navMesh, ulong polyRef);

	[DllImport("RustNative")]
	public static extern IntPtr CreateCorridor();

	[DllImport("RustNative")]
	public static extern void FreeCorridor(IntPtr corridor);

	[DllImport("RustNative")]
	public static extern void CorridorReset(IntPtr corridor, ulong polyRef, in Vector3 pos);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorSetPath(IntPtr corridor, [In] ulong[] polys, int npolys, in Vector3 targetPos);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorMove(IntPtr navMesh, IntPtr corridor, in Vector3 desiredPos, out Vector3 resultPos, out ulong firstPolyRef);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorMoveTargetPosition(IntPtr navMesh, IntPtr corridor, in Vector3 desiredTarget, out Vector3 resultTarget);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorOptimizeAndMove(IntPtr navMesh, IntPtr corridor, in Vector3 optimizeNext, float optimizationRange, in Vector3 desiredPos, out Vector3 resultPos, out ulong firstPolyRef);

	[DllImport("RustNative")]
	public static extern int CorridorFindCorners(IntPtr navMesh, IntPtr corridor, [Out] Vector3[] cornerVerts, int maxCorners, [MarshalAs(UnmanagedType.U1)] out bool endReached);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool CorridorIsValid(IntPtr navMesh, IntPtr corridor, int maxLookAhead);

	[DllImport("RustNative")]
	public static extern void CorridorOptimizeVisibility(IntPtr navMesh, IntPtr corridor, in Vector3 next, float optimizationRange);

	[DllImport("RustNative")]
	public static extern ulong CorridorGetFirstPoly(IntPtr corridor);

	[DllImport("RustNative")]
	public static extern void SetLogCallback(LogCallback callback);

	[DllImport("RustNative")]
	public static extern void SetLegacyBuild([MarshalAs(UnmanagedType.U1)] bool enabled);

	[DllImport("RustNative")]
	public static extern IntPtr CreateEmptyNavMesh(in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool RemoveTileFromNavMesh(IntPtr navMeshWrapper, int tx, int ty);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeTileData(IntPtr tileData);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool AddPrebuiltTileToNavMesh(IntPtr navMeshWrapper, int tx, int ty, IntPtr tileData, int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ComputeTriangleYExtent(IntPtr verts, IntPtr tris, int triCount, float minX, float maxX, float minZ, float maxZ, out float outMinY, out float outMaxY);

	[DllImport("RustNative")]
	public static extern IntPtr CreateHeightFieldRaw(in NavMeshBuildParams buildParams, IntPtr verts, int nverts, IntPtr tris, int triCount, in Vector3 bmin, in Vector3 bmax);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeHeightField(IntPtr heightfield);

	[DllImport("RustNative")]
	public static extern IntPtr CreateCompactHeightField(in NavMeshBuildParams buildParams, IntPtr heightField);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeCompactHeightField(IntPtr compactHeightfield);

	[DllImport("RustNative")]
	public static extern IntPtr CreatePolymesh(in NavMeshBuildParams buildParams, IntPtr compactHeightField);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreePolymesh(IntPtr polymesh);

	[DllImport("RustNative")]
	public static extern IntPtr CreateDetailPolymesh(in NavMeshBuildParams buildParams, IntPtr polyMesh, IntPtr compactHeightField, float sampleDistMult, float sampleMaxErrorMult);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool FreeDetailPolymesh(IntPtr detailPolyMesh);

	[DllImport("RustNative")]
	public static extern IntPtr CreateNavData(in NavMeshBuildParams buildParams, int tx, int ty, IntPtr polyMesh, IntPtr detailMesh, out int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ValidateTileData(IntPtr data, int dataSize);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool ValidateNavMesh(IntPtr navWrapper);

	[DllImport("RustNative")]
	[return: MarshalAs(UnmanagedType.U1)]
	public static extern bool SaveNavMesh(string path, IntPtr navWrapper, in NavMeshBuildParams buildParams, in Vector3 bmin, in Vector3 bmax, IntPtr managedBlob, int managedBlobSize, int flags, int threadCount);

	[DllImport("RustNative")]
	public static extern IntPtr LoadNavMesh(string path, out IntPtr managedBlob, out int managedBlobSize, int threadCount);

	[DllImport("RustNative")]
	public static extern void FreeManagedBlob(IntPtr blob);

	[DllImport("RustNative")]
	public static extern int GetNavMeshTileCoords(IntPtr navWrapper, IntPtr outCoords, int maxPairs);
}
