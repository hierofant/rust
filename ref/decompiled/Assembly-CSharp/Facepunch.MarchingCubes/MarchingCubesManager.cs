using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UtilityJobs;

namespace Facepunch.MarchingCubes;

public class MarchingCubesManager : FacepunchBehaviour
{
	private static MarchingCubesManager instance;

	private ListHashSet<IMarchingCubesTarget> allCubesList;

	private ListHashSet<IMarchingCubesTarget> generationQueue;

	private BufferList<IMarchingCubesTarget> awaitingPhysicsAssignment;

	private JobHandle physicsBakeHandle;

	private MarchingCubesGenerator[] generators;

	[ServerVar]
	[ClientVar]
	public static bool DebugLog = false;

	private static int _generatorPoolCount;

	private static int _colliderMipLevel = 1;

	public static MarchingCubesManager Instance
	{
		get
		{
			if ((object)instance != null)
			{
				return instance;
			}
			GameObject obj = new GameObject("MarchingCubeManager");
			Object.DontDestroyOnLoad(obj);
			instance = obj.AddComponent<MarchingCubesManager>();
			return instance;
		}
	}

	[ServerVar(Default = "4", Help = "[1-16] - each generator has constant memory overhead, but will allow more to process at once")]
	[ClientVar(Default = "4", Help = "[1-16] - each generator has constant memory overhead, but will allow more to process at once")]
	public static int GeneratorPoolCount
	{
		get
		{
			return Mathf.Max(1, _generatorPoolCount);
		}
		set
		{
			_generatorPoolCount = Mathf.Clamp(value, 1, 16);
			Instance.InitGeneratorPool();
		}
	}

	[ServerVar(Default = "1", Help = "[0-2] - mip level the sculpture collision mesh is marched at. Each level is ~4x fewer collision triangles and a correspondingly cheaper physics bake, at the cost of the collider drifting slightly from the visual surface")]
	[ClientVar(Default = "1", Help = "[0-2] - mip level the sculpture collision mesh is marched at. Each level is ~4x fewer collision triangles and a correspondingly cheaper physics bake, at the cost of the collider drifting slightly from the visual surface")]
	public static int ColliderMipLevel
	{
		get
		{
			return Mathf.Clamp(_colliderMipLevel, 0, 2);
		}
		set
		{
			_colliderMipLevel = Mathf.Clamp(value, 0, 2);
		}
	}

	private void Awake()
	{
		allCubesList = new ListHashSet<IMarchingCubesTarget>();
		generationQueue = new ListHashSet<IMarchingCubesTarget>();
		awaitingPhysicsAssignment = new BufferList<IMarchingCubesTarget>();
		InitGeneratorPool();
	}

	private void InitGeneratorPool()
	{
		DisposeGenerators();
		generators = new MarchingCubesGenerator[GeneratorPoolCount];
		for (int i = 0; i < generators.Length; i++)
		{
			generators[i] = new MarchingCubesGenerator(null, null, null, float3.zero, 0f);
		}
	}

	private void DisposeGenerators()
	{
		if (generators == null)
		{
			return;
		}
		for (int i = 0; i < generators.Length; i++)
		{
			if (generators[i] != null)
			{
				generators[i].Dispose();
			}
		}
	}

	private void OnDestroy()
	{
		DisposeGenerators();
	}

	public void Add(IMarchingCubesTarget target)
	{
		allCubesList.Add(target);
	}

	public void Remove(IMarchingCubesTarget target)
	{
		allCubesList.Remove(target);
	}

	public void FixedUpdate()
	{
		if (awaitingPhysicsAssignment.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PhysicsBakeComplete"))
		{
			physicsBakeHandle.Complete();
			physicsBakeHandle = default(JobHandle);
		}
		using (TimeWarning.New("PhysicsMeshAssign"))
		{
			foreach (IMarchingCubesTarget item in awaitingPhysicsAssignment)
			{
				if (!(item.TargetMeshCollider == null))
				{
					item.TargetMeshCollider.sharedMesh = item.TargetMeshForCollision;
					if (item.TargetMeshCollider.convex != item.WantsConvexCollider)
					{
						item.TargetMeshCollider.convex = item.WantsConvexCollider;
					}
				}
			}
		}
		awaitingPhysicsAssignment.Clear();
	}

	public void LateUpdate()
	{
		ProcessQueue();
	}

	public void Enqueue(IMarchingCubesTarget target)
	{
		generationQueue.TryAdd(target);
	}

	private void ProcessQueue()
	{
		using (TimeWarning.New("FreeMemoryCheck"))
		{
			MarchingCubesGenerator[] array = generators;
			foreach (MarchingCubesGenerator marchingCubesGenerator in array)
			{
				if (marchingCubesGenerator.UsedSinceLastFree && (float)marchingCubesGenerator.SinceLastUse > 10f)
				{
					int num = marchingCubesGenerator.TotalNativeMemoryUsage();
					marchingCubesGenerator.ZeroOutAllocations();
					if (DebugLog)
					{
						Debug.Log($"[MARCHING CUBES] Resized generator from {(float)num / 1024f / 1024f}MB to {(float)marchingCubesGenerator.TotalNativeMemoryUsage() / 1024f / 1024f}MB");
					}
				}
			}
		}
		using (TimeWarning.New("PruneStaleTargets"))
		{
			for (int num2 = generationQueue.Count - 1; num2 >= 0; num2--)
			{
				IMarchingCubesTarget marchingCubesTarget = generationQueue[num2];
				if (marchingCubesTarget.SDFSet == null || !marchingCubesTarget.SDFSet.IsCreated)
				{
					generationQueue.RemoveAt(num2);
				}
			}
		}
		if (generationQueue.Count == 0)
		{
			return;
		}
		int num3 = Mathf.Min(GeneratorPoolCount, generationQueue.Count);
		using PooledList<IMarchingCubesTarget> pooledList = Pool.Get<PooledList<IMarchingCubesTarget>>();
		NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(num3, Allocator.Temp);
		NativeList<Mesh.MeshDataArray> results = new NativeList<Mesh.MeshDataArray>(num3 * 4, Allocator.Temp);
		NativeArray<int> nativeArray = new NativeArray<int>(num3, Allocator.Temp);
		NativeArray<int> nativeArray2 = new NativeArray<int>(num3, Allocator.Temp);
		bool censored = false;
		using (TimeWarning.New("SchedulingMarches"))
		{
			for (int j = 0; j < num3; j++)
			{
				IMarchingCubesTarget marchingCubesTarget2 = generationQueue.Values[j];
				MarchingCubesGenerator marchingCubesGenerator2 = generators[j];
				pooledList.Add(marchingCubesTarget2);
				marchingCubesGenerator2.Mesh = marchingCubesTarget2.TargetMesh;
				marchingCubesGenerator2.MeshForCollision = marchingCubesTarget2.TargetMeshForCollision;
				marchingCubesGenerator2.MeshCollider = marchingCubesTarget2.TargetMeshCollider;
				marchingCubesGenerator2.Offset = marchingCubesTarget2.VertexOffset;
				marchingCubesGenerator2.Scale = marchingCubesTarget2.VertexScale;
				nativeArray[j] = results.Length;
				nativeArray2[j] = RenderMeshCount(marchingCubesTarget2);
				jobs[j] = marchingCubesGenerator2.ScheduleMarchChain(marchingCubesTarget2.SDFSet, nativeArray2[j], ColliderMipLevel, censored, results, marchingCubesTarget2.SDFSet.ConsumeDataDependency());
			}
		}
		JobHandle.CompleteAll(jobs);
		NativeArray<int> nativeArray3 = new NativeArray<int>(num3, Allocator.TempJob);
		NativeArray<bool> nativeArray4 = new NativeArray<bool>(num3, Allocator.TempJob);
		using (TimeWarning.New("Apply Updates"))
		{
			for (int k = 0; k < num3; k++)
			{
				IMarchingCubesTarget marchingCubesTarget3 = pooledList[k];
				MarchingCubesGenerator marchingCubesGenerator3 = generators[k];
				int num4 = nativeArray[k];
				marchingCubesGenerator3.ApplyMeshData(results[num4], marchingCubesTarget3.TargetMeshForCollision);
				nativeArray3[k] = marchingCubesGenerator3.CollisionMeshInstanceId;
				nativeArray4[k] = marchingCubesTarget3.WantsConvexCollider;
				awaitingPhysicsAssignment.Add(marchingCubesTarget3);
				if (nativeArray2[k] > 0)
				{
					marchingCubesGenerator3.ApplyMeshData(results[num4 + 1], marchingCubesTarget3.TargetMesh);
					for (int l = 1; l < nativeArray2[k]; l++)
					{
						marchingCubesGenerator3.ApplyMeshData(results[num4 + 1 + l], marchingCubesTarget3.GetLodMesh(l));
					}
					marchingCubesTarget3.OnRenderMeshesUpdated();
				}
			}
		}
		using (TimeWarning.New("Schedule Physics Bake"))
		{
			physicsBakeHandle = IJobParallelForExtensions.Schedule(new UtilityJobs.BakePhysicsMeshesJob
			{
				MeshIds = nativeArray3.AsReadOnly(),
				Convex = nativeArray4.AsReadOnly()
			}, num3, 1, physicsBakeHandle);
			nativeArray3.Dispose(physicsBakeHandle);
			nativeArray4.Dispose(physicsBakeHandle);
			JobHandle.ScheduleBatchedJobs();
		}
		using (TimeWarning.New("Dequeue"))
		{
			for (int m = 0; m < pooledList.Count; m++)
			{
				generationQueue.Remove(pooledList[m]);
			}
		}
		results.Dispose();
	}

	private static int RenderMeshCount(IMarchingCubesTarget target)
	{
		if (!target.isClient)
		{
			return 0;
		}
		return MarchingCubesGenerator.ClampRenderMeshCount(1 + target.LodMeshCount);
	}
}
