#define UNITY_ASSERTIONS
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

public class SDFChunk : FacepunchBehaviour, IDisposable
{
	[NonSerialized]
	public SDFSet Owner;

	[NonSerialized]
	public int ChunkId;

	[NonSerialized]
	public QuantizedFloatData3DArray DataArray;

	[NonSerialized]
	public int3 Origin;

	[NonSerialized]
	public Bounds ChunkBoundsSetSpace;

	private float _iso;

	public void Init(SDFSet owner, int chunkId, int3 origin, int3 bounds, float scale, float iso)
	{
		Owner = owner;
		ChunkId = chunkId;
		DataArray.Init(origin, bounds);
		Origin = origin;
		base.transform.localPosition = (float3)Origin * scale;
		ChunkBoundsSetSpace = new Bounds((Vector3)(origin + (float3)bounds / 2f), (Vector3)(float3)bounds);
		_iso = iso;
	}

	public unsafe void FillEmpty()
	{
		UnsafeUtility.MemSet(DataArray.FlatArray.GetUnsafePtr(), byte.MaxValue, DataArray.FlatArray.Length);
	}

	public QuantizedFloatData3DArray AcquireDataArray()
	{
		Owner.CompleteDataJobs();
		return DataArray;
	}

	public void CopyFrom(SDFChunk source)
	{
		Owner.CompleteDataJobs();
		source.Owner.CompleteDataJobs();
		DataArray.FlatArray.CopyFrom(source.DataArray.FlatArray);
	}

	public void CopyToByteArray(ref byte[] arr)
	{
		Owner.CompleteDataJobs();
		NativeArray<byte> flatArray = DataArray.FlatArray;
		if (arr.Length < flatArray.Length)
		{
			arr = new byte[flatArray.Length];
		}
		flatArray.CopyTo(arr);
	}

	public unsafe void CopyFromByteArray(ArraySegment<byte> arr)
	{
		Owner.CompleteDataJobs();
		NativeArray<byte> flatArray = DataArray.FlatArray;
		if (arr.Count != flatArray.Length)
		{
			Debug.LogError("Trying to load non-matching sized grid");
			return;
		}
		fixed (byte* ptr = arr.Array)
		{
			void* unsafePtr = flatArray.GetUnsafePtr();
			int num = UnsafeUtility.SizeOf<byte>();
			UnsafeUtility.MemCpy(unsafePtr, ptr + arr.Offset * num, arr.Count * num);
		}
	}

	public JobHandle GenerateCensoredChunk(QuantizedFloatData3DArray srcData, int3 segments, JobHandle inputDeps)
	{
		Debug.Assert(math.all(segments > 0));
		int num = segments.x * segments.y * segments.z;
		int num2 = math.max(1, num / JobsUtility.JobWorkerCount);
		int bufferCount = (num + num2 - 1) / num2;
		NativeStream nativeStream = new NativeStream(bufferCount, Allocator.TempJob);
		Facepunch.MarchingCubes.SDFChunkJobs.AccumulateCensorBoundsJob jobData = default(Facepunch.MarchingCubes.SDFChunkJobs.AccumulateCensorBoundsJob);
		jobData.SrcData = srcData;
		jobData.ShapeStream = nativeStream.AsWriter();
		jobData.SegmentsX = segments.x;
		jobData.SegmentsY = segments.y;
		jobData.SegmentsZ = segments.z;
		jobData.iso = _iso;
		jobData.batchSize = num2;
		JobHandle dependsOn = IJobParallelForBatchExtensions.Schedule(jobData, num, num2, inputDeps);
		Facepunch.MarchingCubes.SDFChunkJobs.ApplyCensorBoundsJob applyCensorBoundsJob = default(Facepunch.MarchingCubes.SDFChunkJobs.ApplyCensorBoundsJob);
		applyCensorBoundsJob.OutputArray = DataArray;
		applyCensorBoundsJob.ShapeStream = nativeStream.AsReader();
		dependsOn = applyCensorBoundsJob.Schedule(innerloopBatchCount: math.max(1, DataArray.Depth / JobsUtility.JobWorkerCount), arrayLength: DataArray.Depth, dependsOn: dependsOn);
		nativeStream.Dispose(dependsOn);
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob clearBoundariesJob = default(Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob);
		clearBoundariesJob.DataArray = DataArray;
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob jobData3 = clearBoundariesJob;
		return IJobExtensions.ScheduleByRef(ref jobData3, dependsOn);
	}

	public static JobHandle ScheduleClearBoundaries(QuantizedFloatData3DArray dataArray, JobHandle inputDeps)
	{
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob clearBoundariesJob = default(Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob);
		clearBoundariesJob.DataArray = dataArray;
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob jobData = clearBoundariesJob;
		return IJobExtensions.ScheduleByRef(ref jobData, inputDeps);
	}

	public JobHandle GenerateChunkData(NativeArray<Shape>.ReadOnly mods, JobHandle inputDeps)
	{
		Facepunch.MarchingCubes.SDFChunkJobs.CalculateDistanceFieldJob calculateDistanceFieldJob = default(Facepunch.MarchingCubes.SDFChunkJobs.CalculateDistanceFieldJob);
		calculateDistanceFieldJob.Origin = Origin;
		calculateDistanceFieldJob.ChunkBounds = ChunkBoundsSetSpace;
		calculateDistanceFieldJob.Mods = mods;
		calculateDistanceFieldJob.DataArray = DataArray;
		Facepunch.MarchingCubes.SDFChunkJobs.CalculateDistanceFieldJob jobData = calculateDistanceFieldJob;
		inputDeps = IJobExtensions.ScheduleByRef(ref jobData, inputDeps);
		Facepunch.MarchingCubes.SDFChunkJobs.CleanupIslandsJob cleanupIslandsJob = default(Facepunch.MarchingCubes.SDFChunkJobs.CleanupIslandsJob);
		cleanupIslandsJob.DataArray = DataArray;
		cleanupIslandsJob.Iso = _iso;
		Facepunch.MarchingCubes.SDFChunkJobs.CleanupIslandsJob jobData2 = cleanupIslandsJob;
		inputDeps = IJobExtensions.ScheduleByRef(ref jobData2, inputDeps);
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob clearBoundariesJob = default(Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob);
		clearBoundariesJob.DataArray = DataArray;
		Facepunch.MarchingCubes.SDFChunkJobs.ClearBoundariesJob jobData3 = clearBoundariesJob;
		inputDeps = IJobExtensions.ScheduleByRef(ref jobData3, inputDeps);
		return inputDeps;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		DataArray.Dispose();
	}
}
