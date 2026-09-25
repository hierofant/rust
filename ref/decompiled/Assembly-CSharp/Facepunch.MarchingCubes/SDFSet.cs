using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UtilityJobs;

namespace Facepunch.MarchingCubes;

public class SDFSet : FacepunchBehaviour, IDisposable
{
	public GameObject TargetRoot;

	public GameObjectRef ChunkPrefab;

	public float ChunkScale;

	public float iso;

	[NonSerialized]
	private NativeList<Shape> Modifications;

	[NonSerialized]
	public List<SDFChunk> Chunks;

	[NonSerialized]
	public List<SDFChunk> CensorChunks;

	public const float SoftnessFraction = 0.15f;

	public JobHandle DataDependency { get; private set; }

	public bool IsCreated { get; private set; }

	public JobHandle ConsumeDataDependency()
	{
		JobHandle dataDependency = DataDependency;
		DataDependency = default(JobHandle);
		return dataDependency;
	}

	public void AddDataDependency(JobHandle handle)
	{
		DataDependency = JobHandle.CombineDependencies(DataDependency, handle);
	}

	public void CompleteDataJobs()
	{
		DataDependency.Complete();
		DataDependency = default(JobHandle);
	}

	public void Init()
	{
		Modifications = new NativeList<Shape>(Allocator.Persistent);
		Chunks = new List<SDFChunk>();
		iso = 128f;
		IsCreated = true;
	}

	public SDFChunk AddChunk(int3 origin, int3 bounds)
	{
		SDFChunk component = ChunkPrefab.Instantiate(TargetRoot.transform).GetComponent<SDFChunk>();
		component.Init(this, Chunks.Count, origin, bounds, ChunkScale, iso);
		Chunks.Add(component);
		return component;
	}

	public SDFChunk AddCensorChunk(SDFChunk copyOf)
	{
		SDFChunk component = ChunkPrefab.Instantiate(TargetRoot.transform).GetComponent<SDFChunk>();
		component.Init(this, copyOf.ChunkId, copyOf.Origin, copyOf.DataArray.Bounds, ChunkScale, iso);
		CensorChunks.Add(component);
		return component;
	}

	public void ClearAllMods()
	{
		CompleteDataJobs();
		Modifications.Clear();
	}

	public void ScheduleClearAllMods()
	{
		ClearListJob<Shape> clearListJob = default(ClearListJob<Shape>);
		clearListJob.List = Modifications;
		ClearListJob<Shape> jobData = clearListJob;
		DataDependency = jobData.Schedule(DataDependency);
	}

	public void ClearChunks()
	{
		CompleteDataJobs();
		foreach (SDFChunk chunk in Chunks)
		{
			chunk.FillEmpty();
		}
	}

	public static float SmoothingForRadius(float softness, float radius)
	{
		return math.min(softness * 0.15f * radius, 0.625f);
	}

	public void AddMod(in Shape shape)
	{
		CompleteDataJobs();
		Modifications.Add(in shape);
	}

	public void AddSphereMod(float3 blockSpacePos, float radius, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.Sphere, blockSpacePos, new float3(radius), quaternion.identity, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddBulgeMod(float3 blockSpacePos, float radius, float strength, bool isPull)
	{
		Shape shape = new Shape(ShapeType.Bulge, blockSpacePos, new float3(radius, strength, 0f), quaternion.identity, isPull, 0f);
		AddMod(in shape);
	}

	public void AddSmoothMod(float3 blockSpacePos, float radius, float strength)
	{
		Shape shape = new Shape(ShapeType.Smooth, blockSpacePos, new float3(radius, strength, 0f), quaternion.identity, isAdditive: false, 0f);
		AddMod(in shape);
	}

	public void AddAABBMod(float3 blockSpacePos, float3 extents, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.AABB, blockSpacePos, extents, quaternion.identity, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddOBBMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.OBB, blockSpacePos, extents, rotation, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddSharpOBBMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.SharpOBB, blockSpacePos, extents, rotation, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddCylinderMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.Cylinder, blockSpacePos, extents, rotation, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddCapsuleMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.Capsule, blockSpacePos, extents, rotation, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddConeMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.Cone, blockSpacePos, extents, rotation, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void AddHexPrismMod(float3 blockSpacePos, float3 extents, quaternion rotation, bool isAdditive, float smoothing = 0f)
	{
		Shape shape = new Shape(ShapeType.HexPrism, blockSpacePos, extents, rotation, isAdditive, smoothing);
		AddMod(in shape);
	}

	public void RegenerateAllChunks()
	{
		ScheduleRegenerateAllChunks();
		CompleteDataJobs();
	}

	public void ScheduleRegenerateAllChunks()
	{
		if (Chunks.Count != 0)
		{
			JobHandle jobHandle = DataDependency;
			NativeArray<Shape>.ReadOnly mods = Modifications.AsReadOnly();
			for (int i = 0; i < Chunks.Count; i++)
			{
				JobHandle job = Chunks[i].GenerateChunkData(mods, DataDependency);
				jobHandle = JobHandle.CombineDependencies(jobHandle, job);
			}
			DataDependency = jobHandle;
		}
	}

	public int GetMaxYLayer()
	{
		CompleteDataJobs();
		NativeReference<int> maxYLayer = new NativeReference<int>(Allocator.TempJob);
		FindMaxYLayerJob findMaxYLayerJob = default(FindMaxYLayerJob);
		findMaxYLayerJob.data = Chunks[0].DataArray;
		findMaxYLayerJob.iso = iso;
		findMaxYLayerJob.maxYLayer = maxYLayer;
		FindMaxYLayerJob jobData = findMaxYLayerJob;
		IJobExtensions.RunByRef(ref jobData);
		int value = maxYLayer.Value;
		maxYLayer.Dispose(default(JobHandle));
		return value;
	}

	public void OnDestroy()
	{
		Dispose();
	}

	public void Dispose()
	{
		IsCreated = false;
		CompleteDataJobs();
		NativeListEx.SafeDispose(ref Modifications);
		if (Chunks != null)
		{
			foreach (SDFChunk chunk in Chunks)
			{
				if ((bool)chunk)
				{
					chunk.Dispose();
				}
			}
		}
		if (CensorChunks == null)
		{
			return;
		}
		foreach (SDFChunk censorChunk in CensorChunks)
		{
			if ((bool)censorChunk)
			{
				censorChunk.Dispose();
			}
		}
	}
}
