#define UNITY_ASSERTIONS
using System;
using Facepunch.NativeMeshSimplification;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Facepunch.MarchingCubes;

public class MarchingCubesGenerator : IDisposable
{
	public const int MaxMipLevel = 2;

	private readonly NativeMeshSimplifier _simplifier;

	private readonly QuantizedFloatData3DArray[] mips = new QuantizedFloatData3DArray[2];

	private int3 mipSourceBounds;

	public Mesh Mesh { get; set; }

	public Mesh MeshForCollision { get; set; }

	public MeshCollider MeshCollider { get; set; }

	public float3 Offset { get; set; }

	public float Scale { get; set; }

	public TimeSince SinceLastUse { get; set; }

	public bool UsedSinceLastFree { get; set; }

	public int MeshInstanceId => Mesh.GetInstanceID();

	public int CollisionMeshInstanceId => MeshForCollision.GetInstanceID();

	public Bounds MeshSpaceBounds { get; private set; }

	public static int ClampRenderMeshCount(int count)
	{
		return math.clamp(count, 0, 3);
	}

	public MarchingCubesGenerator(Mesh meshToUpdate, Mesh meshForCollision, MeshCollider meshCollider, float3 vertexOffset, float vertexScale)
	{
		Mesh = meshToUpdate;
		MeshForCollision = meshForCollision;
		MeshCollider = meshCollider;
		_simplifier = new NativeMeshSimplifier();
		Offset = vertexOffset;
		Scale = vertexScale;
	}

	public int TotalNativeMemoryUsage()
	{
		int num = 0;
		for (int i = 0; i < mips.Length; i++)
		{
			if (mips[i].IsCreated)
			{
				num += mips[i].NumCells;
			}
		}
		return num;
	}

	public void ZeroOutAllocations()
	{
		UsedSinceLastFree = false;
		DisposeMips();
	}

	private void DisposeMips()
	{
		for (int i = 0; i < mips.Length; i++)
		{
			mips[i].Dispose();
			mips[i] = default(QuantizedFloatData3DArray);
		}
		mipSourceBounds = default(int3);
	}

	private void MarkUsed()
	{
		SinceLastUse = 0f;
		UsedSinceLastFree = true;
	}

	public JobHandle ScheduleMarchChain(SDFSet set, int renderMeshCount, int colliderMipLevel, bool censored, NativeList<Mesh.MeshDataArray> results, JobHandle inputDeps)
	{
		Debug.Assert(set.Chunks.Count == 1);
		colliderMipLevel = math.clamp(colliderMipLevel, 0, 2);
		renderMeshCount = ClampRenderMeshCount(renderMeshCount);
		censored &= set.CensorChunks != null && set.CensorChunks.Count > 0;
		QuantizedFloatData3DArray dataArray = set.Chunks[0].DataArray;
		QuantizedFloatData3DArray source = (censored ? set.CensorChunks[0].DataArray : dataArray);
		JobHandle jobHandle = inputDeps;
		int length = results.Length;
		Mesh.MeshDataArray value = default(Mesh.MeshDataArray);
		results.Add(in value);
		bool flag = !censored && colliderMipLevel < renderMeshCount;
		if (!flag)
		{
			jobHandle = ScheduleMipPyramid(dataArray, colliderMipLevel, jobHandle);
			jobHandle = ScheduleLevelMarch(dataArray, set.iso, colliderMipLevel, out var vertices, out var indices, jobHandle);
			jobHandle = ScheduleMeshWrite(vertices, indices, out var meshData, withNormals: false, jobHandle);
			results[length] = meshData;
			vertices.Dispose(jobHandle);
			indices.Dispose(jobHandle);
		}
		if (renderMeshCount == 0)
		{
			set.AddDataDependency(jobHandle);
			return jobHandle;
		}
		jobHandle = ScheduleMipPyramid(source, renderMeshCount - 1, jobHandle);
		JobHandle jobHandle2 = default(JobHandle);
		for (int i = 0; i < renderMeshCount; i++)
		{
			NativeList<float3> vertices2;
			NativeList<int> indices2;
			JobHandle inputDeps2 = ScheduleLevelMarch(source, set.iso, i, out vertices2, out indices2, jobHandle);
			Mesh.MeshDataArray meshData2;
			JobHandle jobHandle3 = ScheduleMeshWrite(vertices2, indices2, out meshData2, withNormals: true, inputDeps2);
			results.Add(in meshData2);
			if (flag && i == colliderMipLevel)
			{
				Mesh.MeshDataArray meshData3;
				JobHandle job = ScheduleMeshWrite(vertices2, indices2, out meshData3, withNormals: false, inputDeps2);
				results[length] = meshData3;
				jobHandle3 = JobHandle.CombineDependencies(jobHandle3, job);
			}
			vertices2.Dispose(jobHandle3);
			indices2.Dispose(jobHandle3);
			jobHandle2 = JobHandle.CombineDependencies(jobHandle2, jobHandle3);
		}
		set.AddDataDependency(jobHandle2);
		return jobHandle2;
	}

	private JobHandle ScheduleLevelMarch(QuantizedFloatData3DArray source, float iso, int level, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		QuantizedFloatData3DArray data3DArray = ((level == 0) ? source : mips[level - 1]);
		return ScheduleSDFMarch(data3DArray, iso, level, source.Bounds, out vertices, out indices, inputDeps);
	}

	public JobHandle ScheduleSDFMarch(SDFSet set, bool isCensored, int mipLevel, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		Debug.Assert(set.Chunks.Count == 1);
		QuantizedFloatData3DArray source = (isCensored ? set.CensorChunks[0].DataArray : set.Chunks[0].DataArray);
		mipLevel = math.clamp(mipLevel, 0, 2);
		if (mipLevel > 0)
		{
			inputDeps = ScheduleMipPyramid(source, mipLevel, inputDeps);
		}
		return ScheduleLevelMarch(source, set.iso, mipLevel, out vertices, out indices, inputDeps);
	}

	private JobHandle ScheduleMipPyramid(QuantizedFloatData3DArray source, int mipLevel, JobHandle inputDeps)
	{
		if (!math.all(mipSourceBounds == source.Bounds))
		{
			DisposeMips();
			mipSourceBounds = source.Bounds;
		}
		JobHandle jobHandle = inputDeps;
		for (int i = 1; i <= mipLevel; i++)
		{
			QuantizedFloatData3DArray quantizedFloatData3DArray = mips[i - 1];
			if (!quantizedFloatData3DArray.IsCreated)
			{
				quantizedFloatData3DArray.Init(source.Origin, QuantizedFloatData3DArray.MipBounds(source.Bounds, i));
				mips[i - 1] = quantizedFloatData3DArray;
			}
			QuantizedFloatData3DArray src = ((i == 1) ? source : mips[i - 2]);
			int numCells = quantizedFloatData3DArray.NumCells;
			int indicesPerJobCount = math.max(1, numCells / JobsUtility.JobWorkerCount);
			Facepunch.MarchingCubes.DownsampleJob jobData = default(Facepunch.MarchingCubes.DownsampleJob);
			jobData.src = src;
			jobData.dst = quantizedFloatData3DArray;
			jobHandle = IJobParallelForBatchExtensions.Schedule(jobData, numCells, indicesPerJobCount, jobHandle);
			jobHandle = SDFChunk.ScheduleClearBoundaries(quantizedFloatData3DArray, jobHandle);
		}
		return jobHandle;
	}

	public JobHandle ScheduleSDFMarch(QuantizedFloatData3DArray data3DArray, float iso, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		return ScheduleSDFMarch(data3DArray, iso, 0, data3DArray.Bounds, out vertices, out indices, inputDeps);
	}

	public JobHandle ScheduleSDFMarch(QuantizedFloatData3DArray data3DArray, float iso, int mipLevel, int3 baseBounds, out NativeList<float3> vertices, out NativeList<int> indices, JobHandle inputDeps)
	{
		MarkUsed();
		float3 @float = new float3((float3)baseBounds * 0.5f) + Offset;
		float3 float2 = -@float * Scale;
		float3 float3 = (baseBounds - 1 - @float) * Scale;
		MeshSpaceBounds = new Bounds((Vector3)((float2 + float3) * 0.5f), (Vector3)(float3 - float2));
		int num = 1 << mipLevel;
		float scale = Scale * (float)num;
		float3 vertexOffset = (@float - (float)(num - 1) * 0.5f) / num;
		JobHandle dependsOn = inputDeps;
		int num2 = data3DArray.WidthHeight * data3DArray.Depth;
		int num3 = math.max(1, num2 / JobsUtility.JobWorkerCount);
		int bufferCount = (num2 + num3 - 1) / num3;
		NativeStream nativeStream = new NativeStream(bufferCount, Allocator.TempJob);
		Facepunch.MarchingCubes.MarchFloatGenerateTrianglesJob jobData = default(Facepunch.MarchingCubes.MarchFloatGenerateTrianglesJob);
		jobData.sampler = data3DArray;
		jobData.edgeStream = nativeStream.AsWriter();
		jobData.iso = iso;
		jobData.vertexOffset = vertexOffset;
		jobData.scale = scale;
		jobData.batchSize = num3;
		dependsOn = IJobParallelForBatchExtensions.Schedule(jobData, num2, num3, dependsOn);
		vertices = new NativeList<float3>(Allocator.TempJob);
		indices = new NativeList<int>(Allocator.TempJob);
		Facepunch.MarchingCubes.ProcessTrianglesJob jobData2 = default(Facepunch.MarchingCubes.ProcessTrianglesJob);
		jobData2.edgeStream = nativeStream.AsReader();
		jobData2.vertices = vertices;
		jobData2.indices = indices;
		jobData2.edgeArraySize = data3DArray.NumCells * 3;
		dependsOn = jobData2.Schedule(dependsOn);
		nativeStream.Dispose(dependsOn);
		return dependsOn;
	}

	public JobHandle ScheduleSimplification(NativeList<float3> verticesIn, NativeList<int> indicesIn, out NativeList<float3> verticesOut, out NativeList<int> indicesOut, JobHandle inputDeps)
	{
		verticesOut = new NativeList<float3>(Allocator.TempJob);
		indicesOut = new NativeList<int>(Allocator.TempJob);
		return _simplifier.ScheduleMeshSimplify(0.4f, verticesIn, indicesIn, verticesOut, indicesOut, inputDeps);
	}

	public JobHandle ScheduleMeshWrite(NativeList<float3> vertices, NativeList<int> indices, out Mesh.MeshDataArray meshData, bool withNormals, JobHandle inputDeps)
	{
		meshData = Mesh.AllocateWritableMeshData(1);
		Facepunch.MarchingCubes.WriteMeshDataJob jobData = default(Facepunch.MarchingCubes.WriteMeshDataJob);
		jobData.vertices = vertices.AsDeferredJobArray();
		jobData.indices = indices.AsDeferredJobArray();
		jobData.meshData = meshData[0];
		jobData.withNormals = withNormals;
		return jobData.Schedule(inputDeps);
	}

	public void ApplyMeshData(Mesh.MeshDataArray meshData, Mesh toMesh)
	{
		using (TimeWarning.New("MarchingCubes.ApplyMeshData"))
		{
			if (toMesh == null)
			{
				meshData.Dispose();
				return;
			}
			Mesh.ApplyAndDisposeWritableMeshData(meshData, toMesh, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
			toMesh.bounds = MeshSpaceBounds;
			if (BaseSculpture.LogMeshStats)
			{
				Debug.Log($"{toMesh.name} : tris({toMesh.GetIndexCount(0) / 3}) verts({toMesh.vertexCount})");
			}
		}
	}

	public void Dispose()
	{
		DisposeMips();
		_simplifier.Dispose();
	}
}
