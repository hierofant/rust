#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Cysharp.Threading.Tasks;
using Development.Attributes;
using Facepunch;
using GamePhysicsJobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public static class GamePhysics
{
	public enum Realm
	{
		Client,
		Server
	}

	[Flags]
	public enum MasksToValidate : byte
	{
		None = 0,
		Terrain = 1,
		Water = 2,
		All = 3
	}

	public const int BufferLength = 32768;

	private static RaycastHit[] hitBuffer = new RaycastHit[32768];

	private static RaycastHit[] hitBufferB = new RaycastHit[32768];

	private static Collider[] colBuffer = new Collider[32768];

	[ServerVar(Help = "How many results to collect per command - DONT set this too low or you'll risk missing results", Default = "48")]
	public static int DefaultMaxResultsPerQuery = 48;

	public static bool CheckSphere(Realm realm, Vector3 position, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		return CheckSphere(position, radius, layerMask, triggerInteraction);
	}

	public static bool CheckSphere(Vector3 position, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleIgnoreCollision(position, layerMask);
		return UnityEngine.Physics.CheckSphere(position, radius, layerMask, triggerInteraction);
	}

	public static JobHandle CheckSpheres(NativeArray<Vector3>.ReadOnly pos, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, NativeArray<bool> results, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All)
	{
		using (TimeWarning.New("GamePhysics.CheckSpheres"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(pos.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			JobHandle dependsOn = OverlapSpheres(pos, radii, layerMasks, hits, 1, triggerInteraction, validate);
			CheckHitsJob checkHitsJob = default(CheckHitsJob);
			checkHitsJob.Results = results;
			checkHitsJob.Hits = hits.AsReadOnly();
			CheckHitsJob jobData = checkHitsJob;
			JobHandle jobHandle = IJobExtensions.ScheduleByRef(ref jobData, dependsOn);
			hits.Dispose(jobHandle);
			return jobHandle;
		}
	}

	public static bool CheckCapsule(Realm realm, Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		return CheckCapsule(start, end, radius, layerMask, triggerInteraction);
	}

	public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		layerMask = HandleIgnoreCollision((start + end) * 0.5f, layerMask);
		if (ignoreEntity == null)
		{
			return UnityEngine.Physics.CheckCapsule(start, end, radius, layerMask, triggerInteraction);
		}
		int num = UnityEngine.Physics.OverlapCapsuleNonAlloc(start, end, radius, colBuffer, layerMask, triggerInteraction);
		for (int i = 0; i < num; i++)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(colBuffer[i]);
			if (baseEntity == null)
			{
				return true;
			}
			if (!(baseEntity == ignoreEntity) && baseEntity.isServer == ignoreEntity.isServer)
			{
				return true;
			}
		}
		return false;
	}

	public static JobHandle CheckCapsules(NativeArray<Vector3>.ReadOnly starts, NativeArray<Vector3>.ReadOnly ends, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, NativeArray<bool> results, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All, bool mitigateSpheres = true)
	{
		using (TimeWarning.New("GamePhysics.CheckCapsules"))
		{
			NativeArray<int>.ReadOnly layerMasks2 = layerMasks;
			NativeArray<int> array = default(NativeArray<int>);
			if (validate != 0)
			{
				array = new NativeArray<int>(layerMasks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				layerMasks.CopyTo(array);
				NativeArray<Vector3> results2 = new NativeArray<Vector3>(starts.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				CalcMidpoingJob calcMidpoingJob = default(CalcMidpoingJob);
				calcMidpoingJob.Results = results2;
				calcMidpoingJob.From = starts;
				calcMidpoingJob.To = ends;
				CalcMidpoingJob jobData = calcMidpoingJob;
				IJobExtensions.RunByRef(ref jobData);
				HandleIgnoreCollision(results2.AsReadOnly(), array, validate);
				results2.Dispose();
				layerMasks2 = array.AsReadOnly();
			}
			NativeArray<OverlapCapsuleCommand> nativeArray = new NativeArray<OverlapCapsuleCommand>(starts.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GenerateOverlapCapsuleCommandsJob generateOverlapCapsuleCommandsJob = default(GenerateOverlapCapsuleCommandsJob);
			generateOverlapCapsuleCommandsJob.CapsuleCommands = nativeArray;
			generateOverlapCapsuleCommandsJob.From = starts;
			generateOverlapCapsuleCommandsJob.To = ends;
			generateOverlapCapsuleCommandsJob.Radiii = radii;
			generateOverlapCapsuleCommandsJob.LayerMasks = layerMasks2;
			generateOverlapCapsuleCommandsJob.TriggerInteraction = triggerInteraction;
			generateOverlapCapsuleCommandsJob.HitBackfaces = false;
			generateOverlapCapsuleCommandsJob.HitMultipleFaces = false;
			GenerateOverlapCapsuleCommandsJob jobData2 = generateOverlapCapsuleCommandsJob;
			IJobExtensions.RunByRef(ref jobData2);
			NativeArrayEx.SafeDispose(ref array);
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(starts.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			JobHandle jobHandle = default(JobHandle);
			jobHandle = ((!mitigateSpheres) ? ExecuteOverlapCapsuleCommands(nativeArray, hits, 1) : MitigateSphereCapsuleCommands(nativeArray, hits, 1));
			nativeArray.Dispose(jobHandle);
			CheckHitsJob checkHitsJob = default(CheckHitsJob);
			checkHitsJob.Results = results;
			checkHitsJob.Hits = hits.AsReadOnly();
			CheckHitsJob jobData3 = checkHitsJob;
			JobHandle jobHandle2 = IJobExtensions.ScheduleByRef(ref jobData3, jobHandle);
			hits.Dispose(jobHandle2);
			return jobHandle2;
		}
	}

	public static bool CheckOBB(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		return UnityEngine.Physics.CheckBox(obb.position, obb.extents, obb.rotation, layerMask, triggerInteraction);
	}

	public static bool CheckOBBAndEntity(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		int num = UnityEngine.Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction);
		for (int i = 0; i < num; i++)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(colBuffer[i]);
			if (!(baseEntity != null) || !(ignoreEntity != null) || (baseEntity.isServer == ignoreEntity.isServer && !(baseEntity == ignoreEntity)))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckBounds(Bounds bounds, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleIgnoreCollision(bounds.center, layerMask);
		return UnityEngine.Physics.CheckBox(bounds.center, bounds.extents, Quaternion.identity, layerMask, triggerInteraction);
	}

	public static void CheckBounds(NativeArray<Vector3>.ReadOnly centers, NativeArray<Vector3>.ReadOnly halfExtents, NativeArray<int>.ReadOnly layerMasks, NativeArray<bool> results, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All)
	{
		NativeArray<int>.ReadOnly layerMasks2 = layerMasks;
		NativeArray<int> array = default(NativeArray<int>);
		if (validate != 0)
		{
			array = new NativeArray<int>(layerMasks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			layerMasks.CopyTo(array);
			HandleIgnoreCollision(centers, array, validate);
			layerMasks2 = array.AsReadOnly();
		}
		NativeArray<OverlapBoxCommand> nativeArray = new NativeArray<OverlapBoxCommand>(centers.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GenerateOverlapBoxCommandsJob jobData = default(GenerateOverlapBoxCommandsJob);
		jobData.BoxCommands = nativeArray;
		jobData.Centers = centers;
		jobData.Extents = halfExtents;
		jobData.LayerMasks = layerMasks2;
		jobData.TriggerInteraction = triggerInteraction;
		jobData.HitBackfaces = false;
		jobData.HitMultipleFaces = false;
		jobData.Run();
		NativeArrayEx.SafeDispose(ref array);
		NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(centers.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		ExecuteOverlapBoxCommands(nativeArray, hits, 1).Complete();
		nativeArray.Dispose();
		CheckHitsJob checkHitsJob = default(CheckHitsJob);
		checkHitsJob.Results = results;
		checkHitsJob.Hits = hits.AsReadOnly();
		CheckHitsJob jobData2 = checkHitsJob;
		IJobExtensions.RunByRef(ref jobData2);
		hits.Dispose();
	}

	private static JobHandle ExecuteOverlapBoxCommands(NativeArray<OverlapBoxCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast)
	{
		if (Debug.isDebugBuild)
		{
			NativeList<int> invalidIndices = new NativeList<int>(commands.Length, Allocator.TempJob);
			ValidateOverlapBoxCommandsJob validateOverlapBoxCommandsJob = default(ValidateOverlapBoxCommandsJob);
			validateOverlapBoxCommandsJob.InvalidIndices = invalidIndices;
			validateOverlapBoxCommandsJob.Commands = commands.AsReadOnly();
			ValidateOverlapBoxCommandsJob jobData = validateOverlapBoxCommandsJob;
			IJobExtensions.RunByRef(ref jobData);
			if (!invalidIndices.IsEmpty)
			{
				int num = invalidIndices[0];
				OverlapBoxCommand overlapBoxCommand = commands[num];
				Debug.LogError(string.Concat(string.Concat(string.Concat($"OverlapBox has {invalidIndices.Length} invalid box commands!" + $"\nFirst one was at index {num}:", $"\n\tCenter: {overlapBoxCommand.center}"), $"\n\tExtents: {overlapBoxCommand.halfExtents}"), "\nThese queries will be skipped!"));
			}
			invalidIndices.Dispose();
		}
		int batchSize = ThreadUtils.GetBatchSize(commands.Length);
		return OverlapBoxCommand.ScheduleBatch(commands, hits, batchSize, maxResPerCast);
	}

	public static bool CheckInsideNonConvexMesh(Vector3 point, int layerMask = -5)
	{
		bool queriesHitBackfaces = UnityEngine.Physics.queriesHitBackfaces;
		UnityEngine.Physics.queriesHitBackfaces = true;
		int num = UnityEngine.Physics.RaycastNonAlloc(point, Vector3.up, hitBuffer, 100f, layerMask);
		int num2 = UnityEngine.Physics.RaycastNonAlloc(point, -Vector3.up, hitBufferB, 100f, layerMask);
		if (num >= hitBuffer.Length)
		{
			Debug.LogWarning("CheckInsideNonConvexMesh query is exceeding hitBuffer length.");
			return false;
		}
		if (num2 > hitBufferB.Length)
		{
			Debug.LogWarning("CheckInsideNonConvexMesh query is exceeding hitBufferB length.");
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (hitBuffer[i].collider == hitBufferB[j].collider)
				{
					UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
					return true;
				}
			}
		}
		UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
		return false;
	}

	public static bool CheckInsideAnyCollider(Vector3 point, int layerMask = -5)
	{
		if (UnityEngine.Physics.CheckSphere(point, 0f, layerMask))
		{
			return true;
		}
		if (CheckInsideNonConvexMesh(point, layerMask))
		{
			return true;
		}
		if (TerrainMeta.HeightMap != null && TerrainMeta.HeightMap.GetHeight(point) > point.y)
		{
			return true;
		}
		return false;
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapSphere(Vector3 position, float radius, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleIgnoreCollision(position, layerMask);
		int count = UnityEngine.Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static bool ContainsEntity(List<Collider> colliders, BaseEntity entity)
	{
		for (int i = 0; i < colliders.Count; i++)
		{
			if (CompareEntity(GameObjectEx.ToBaseEntity(colliders[i]), entity))
			{
				return true;
			}
		}
		return false;
	}

	public static bool OverlapSphereHasEntity(Vector3 position, float radius, BaseEntity entity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		OverlapSphere(position, radius, obj, layerMask, triggerInteraction);
		bool result = ContainsEntity(obj, entity);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public static JobHandle OverlapSpheres(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, NativeArray<ColliderHit> hits, int maxResPerCast, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All)
	{
		using (TimeWarning.New("GamePhysics.OverlapSpheres"))
		{
			NativeArray<int>.ReadOnly layerMasks2 = layerMasks;
			NativeArray<int> array = default(NativeArray<int>);
			if (validate != 0)
			{
				array = new NativeArray<int>(layerMasks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				layerMasks.CopyTo(array);
				HandleIgnoreCollision(positions, array, validate);
				layerMasks2 = array.AsReadOnly();
			}
			NativeArray<OverlapSphereCommand> nativeArray = new NativeArray<OverlapSphereCommand>(positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GenerateOverlapSphereCommandsJob generateOverlapSphereCommandsJob = default(GenerateOverlapSphereCommandsJob);
			generateOverlapSphereCommandsJob.SphereCommands = nativeArray;
			generateOverlapSphereCommandsJob.Pos = positions;
			generateOverlapSphereCommandsJob.Radiii = radii;
			generateOverlapSphereCommandsJob.LayerMasks = layerMasks2;
			generateOverlapSphereCommandsJob.TriggerInteraction = triggerInteraction;
			generateOverlapSphereCommandsJob.HitBackfaces = false;
			generateOverlapSphereCommandsJob.HitMultipleFaces = false;
			GenerateOverlapSphereCommandsJob jobData = generateOverlapSphereCommandsJob;
			IJobExtensions.RunByRef(ref jobData);
			NativeArrayEx.SafeDispose(ref array);
			JobHandle jobHandle = ExecuteOverlapSphereCommands(nativeArray, hits, maxResPerCast);
			nativeArray.Dispose(jobHandle);
			return jobHandle;
		}
	}

	private static JobHandle ExecuteOverlapSphereCommands(NativeArray<OverlapSphereCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast, JobHandle dependsOn = default(JobHandle))
	{
		if (Debug.isDebugBuild)
		{
			NativeList<int> invalidIndices = new NativeList<int>(commands.Length, Allocator.TempJob);
			ValidateOverlapSphereCommandsJob validateOverlapSphereCommandsJob = default(ValidateOverlapSphereCommandsJob);
			validateOverlapSphereCommandsJob.InvalidIndices = invalidIndices;
			validateOverlapSphereCommandsJob.Commands = commands.AsReadOnly();
			ValidateOverlapSphereCommandsJob jobData = validateOverlapSphereCommandsJob;
			IJobExtensions.ScheduleByRef(ref jobData, dependsOn).Complete();
			if (!invalidIndices.IsEmpty)
			{
				int num = invalidIndices[0];
				OverlapSphereCommand overlapSphereCommand = commands[num];
				Debug.LogError(string.Concat(string.Concat(string.Concat($"OverlapSpheres has {invalidIndices.Length} invalid sphere commands!" + $"\nFirst one was at index {num}:", $"\n\tPos: {overlapSphereCommand.point}"), $"\n\tRadius: {overlapSphereCommand.radius}"), "\nThese queries will be skipped!"));
			}
			invalidIndices.Dispose();
		}
		int batchSize = ThreadUtils.GetBatchSize(commands.Length);
		return OverlapSphereCommand.ScheduleBatch(commands, hits, batchSize, maxResPerCast, dependsOn);
	}

	[PoolAnalyzerNonCaching]
	public static void OBBSweep(OBB obb, Vector3 direction, float distance, List<RaycastHit> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		HitBufferToList(UnityEngine.Physics.BoxCastNonAlloc(obb.position, obb.extents, direction, hitBuffer, obb.rotation, distance, layerMask, triggerInteraction), list);
	}

	[PoolAnalyzerNonCaching]
	public static void CapsuleSweep(Vector3 position0, Vector3 position1, float radius, Vector3 direction, float distance, List<RaycastHit> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleIgnoreCollision(position0, layerMask);
		layerMask = HandleIgnoreCollision(position1, layerMask);
		HitBufferToList(UnityEngine.Physics.CapsuleCastNonAlloc(position0, position1, radius, direction, hitBuffer, distance, layerMask, triggerInteraction), list);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapCapsule(Vector3 point0, Vector3 point1, float radius, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleIgnoreCollision(point0, layerMask);
		layerMask = HandleIgnoreCollision(point1, layerMask);
		int count = UnityEngine.Physics.OverlapCapsuleNonAlloc(point0, point1, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static JobHandle OverlapCapsules(NativeArray<Vector3>.ReadOnly starts, NativeArray<Vector3>.ReadOnly ends, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, NativeArray<ColliderHit> hits, int maxResPerCast, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All, bool mitigateSpheres = true)
	{
		using (TimeWarning.New("GamePhysics.OverlapCapsules"))
		{
			NativeArray<int>.ReadOnly layerMasks2 = layerMasks;
			NativeArray<int> array = default(NativeArray<int>);
			if (validate != 0)
			{
				array = new NativeArray<int>(layerMasks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				layerMasks.CopyTo(array);
				HandleIgnoreCollision(starts, array, validate);
				HandleIgnoreCollision(ends, array, validate);
				layerMasks2 = array.AsReadOnly();
			}
			NativeArray<OverlapCapsuleCommand> nativeArray = new NativeArray<OverlapCapsuleCommand>(starts.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GenerateOverlapCapsuleCommandsJob generateOverlapCapsuleCommandsJob = default(GenerateOverlapCapsuleCommandsJob);
			generateOverlapCapsuleCommandsJob.CapsuleCommands = nativeArray;
			generateOverlapCapsuleCommandsJob.From = starts;
			generateOverlapCapsuleCommandsJob.To = ends;
			generateOverlapCapsuleCommandsJob.Radiii = radii;
			generateOverlapCapsuleCommandsJob.LayerMasks = layerMasks2;
			generateOverlapCapsuleCommandsJob.TriggerInteraction = triggerInteraction;
			generateOverlapCapsuleCommandsJob.HitBackfaces = false;
			generateOverlapCapsuleCommandsJob.HitMultipleFaces = false;
			GenerateOverlapCapsuleCommandsJob jobData = generateOverlapCapsuleCommandsJob;
			IJobExtensions.RunByRef(ref jobData);
			NativeArrayEx.SafeDispose(ref array);
			JobHandle jobHandle = default(JobHandle);
			jobHandle = ((!mitigateSpheres) ? ExecuteOverlapCapsuleCommands(nativeArray, hits, maxResPerCast) : MitigateSphereCapsuleCommands(nativeArray, hits, maxResPerCast));
			nativeArray.Dispose(jobHandle);
			return jobHandle;
		}
	}

	private static JobHandle MitigateSphereCapsuleCommands(NativeArray<OverlapCapsuleCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast)
	{
		NativeList<int> nativeList = new NativeList<int>(commands.Length, Allocator.TempJob);
		FindSphereCmdsInCapsuleCmdsJob findSphereCmdsInCapsuleCmdsJob = default(FindSphereCmdsInCapsuleCmdsJob);
		findSphereCmdsInCapsuleCmdsJob.SphereIndices = nativeList;
		findSphereCmdsInCapsuleCmdsJob.Commands = commands.AsReadOnly();
		FindSphereCmdsInCapsuleCmdsJob jobData = findSphereCmdsInCapsuleCmdsJob;
		IJobExtensions.RunByRef(ref jobData);
		if (nativeList.IsEmpty)
		{
			nativeList.Dispose();
			return ExecuteOverlapCapsuleCommands(commands, hits, maxResPerCast);
		}
		int num = Math.Max(nativeList.Length, commands.Length - nativeList.Length);
		NativeArray<ColliderHit> hits2 = new NativeArray<ColliderHit>(num * maxResPerCast, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		bool num2 = nativeList.Length != commands.Length;
		NativeArray<OverlapSphereCommand> nativeArray = new NativeArray<OverlapSphereCommand>(nativeList.Length, Allocator.TempJob);
		GenerateSphereCmdsFromCapsuleCmdsJob jobData2 = new GenerateSphereCmdsFromCapsuleCmdsJob
		{
			SphereCommands = nativeArray,
			Commands = commands.AsReadOnly(),
			Indices = nativeList.AsReadOnly()
		};
		JobHandle jobHandle = ExecuteOverlapSphereCommands(dependsOn: IJobExtensions.ScheduleByRef(ref jobData2), commands: nativeArray, hits: hits2, maxResPerCast: maxResPerCast);
		nativeArray.Dispose(jobHandle);
		ScatterColliderHitsJob jobData3 = new ScatterColliderHitsJob
		{
			To = hits,
			From = hits2.AsReadOnly(),
			Indices = nativeList.AsReadOnly(),
			MaxHitsPerRay = maxResPerCast
		};
		JobHandle jobHandle2 = IJobExtensions.ScheduleByRef(ref jobData3, jobHandle);
		if (!num2)
		{
			nativeList.Dispose(jobHandle2);
			hits2.Dispose(jobHandle2);
			return jobHandle2;
		}
		NativeArray<bool> workBuffer = new NativeArray<bool>(commands.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		InvertIndexListJob invertIndexListJob = default(InvertIndexListJob);
		invertIndexListJob.Indices = nativeList;
		invertIndexListJob.WorkBuffer = workBuffer;
		InvertIndexListJob jobData4 = invertIndexListJob;
		JobHandle dependsOn2 = IJobExtensions.ScheduleByRef(ref jobData4, jobHandle2);
		dependsOn2.Complete();
		workBuffer.Dispose();
		NativeArray<OverlapCapsuleCommand> nativeArray2 = new NativeArray<OverlapCapsuleCommand>(nativeList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GatherJob<OverlapCapsuleCommand> gatherJob = default(GatherJob<OverlapCapsuleCommand>);
		gatherJob.Results = nativeArray2;
		gatherJob.Source = commands.AsReadOnly();
		gatherJob.Indices = nativeList.AsReadOnly();
		GatherJob<OverlapCapsuleCommand> jobData5 = gatherJob;
		JobHandle dependsOn3 = IJobExtensions.ScheduleByRef(ref jobData5, dependsOn2);
		JobHandle jobHandle3 = ExecuteOverlapCapsuleCommands(nativeArray2, hits2, maxResPerCast, dependsOn3);
		ScatterColliderHitsJob scatterColliderHitsJob = default(ScatterColliderHitsJob);
		scatterColliderHitsJob.To = hits;
		scatterColliderHitsJob.From = hits2.AsReadOnly();
		scatterColliderHitsJob.Indices = nativeList.AsReadOnly();
		scatterColliderHitsJob.MaxHitsPerRay = maxResPerCast;
		ScatterColliderHitsJob jobData6 = scatterColliderHitsJob;
		nativeArray2.Dispose(jobHandle3);
		JobHandle jobHandle4 = IJobExtensions.ScheduleByRef(ref jobData6, jobHandle3);
		nativeList.Dispose(jobHandle4);
		hits2.Dispose(jobHandle4);
		return jobHandle4;
	}

	private static JobHandle ExecuteOverlapCapsuleCommands(NativeArray<OverlapCapsuleCommand> commands, NativeArray<ColliderHit> hits, int maxResPerCast, JobHandle dependsOn = default(JobHandle))
	{
		if (Debug.isDebugBuild)
		{
			NativeList<int> invalidIndices = new NativeList<int>(commands.Length, Allocator.TempJob);
			ValidateOverlapCapsuleCommandsJob validateOverlapCapsuleCommandsJob = default(ValidateOverlapCapsuleCommandsJob);
			validateOverlapCapsuleCommandsJob.InvalidIndices = invalidIndices;
			validateOverlapCapsuleCommandsJob.Commands = commands.AsReadOnly();
			ValidateOverlapCapsuleCommandsJob jobData = validateOverlapCapsuleCommandsJob;
			IJobExtensions.ScheduleByRef(ref jobData, dependsOn).Complete();
			if (!invalidIndices.IsEmpty)
			{
				int num = invalidIndices[0];
				OverlapCapsuleCommand overlapCapsuleCommand = commands[num];
				Debug.LogError(string.Concat(string.Concat(string.Concat(string.Concat($"OverlapCapsules has {invalidIndices.Length} invalid sphere commands!" + $"\nFirst one was at index {num}:", $"\n\tPoint0: {overlapCapsuleCommand.point0}"), $"\n\tPoint1: {overlapCapsuleCommand.point1}"), $"\n\tRadius: {overlapCapsuleCommand.radius}"), "\nThese queries will be skipped!"));
			}
			invalidIndices.Dispose();
		}
		int batchSize = ThreadUtils.GetBatchSize(commands.Length);
		return OverlapCapsuleCommand.ScheduleBatch(commands, hits, batchSize, maxResPerCast, dependsOn);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapOBB(OBB obb, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		int count = UnityEngine.Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static JobHandle OverlapOBBs(NativeArray<OBB>.ReadOnly obbs, NativeArray<int>.ReadOnly layerMasks, NativeArray<ColliderHit> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All)
	{
		NativeArray<int>.ReadOnly layerMasks2 = layerMasks;
		NativeArray<int> array = default(NativeArray<int>);
		if (validate != 0)
		{
			array = new NativeArray<int>(layerMasks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			layerMasks.CopyTo(array);
			using NativeArray<Vector3> posi = new NativeArray<Vector3>(obbs.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GatherPosFromOBBsJob gatherPosFromOBBsJob = default(GatherPosFromOBBsJob);
			gatherPosFromOBBsJob.Posi = posi;
			gatherPosFromOBBsJob.OBBs = obbs;
			GatherPosFromOBBsJob jobData = gatherPosFromOBBsJob;
			IJobExtensions.RunByRef(ref jobData);
			HandleIgnoreCollision(posi.AsReadOnly(), array, validate);
			layerMasks2 = array.AsReadOnly();
		}
		NativeArray<OverlapBoxCommand> nativeArray = new NativeArray<OverlapBoxCommand>(obbs.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		GenerateOverlapBoxCommandsFromOBBsJob jobData2 = default(GenerateOverlapBoxCommandsFromOBBsJob);
		jobData2.BoxCommands = nativeArray;
		jobData2.OBBs = obbs;
		jobData2.LayerMasks = layerMasks2;
		jobData2.TriggerInteraction = triggerInteraction;
		jobData2.HitBackfaces = false;
		jobData2.HitMultipleFaces = false;
		jobData2.Run();
		NativeArrayEx.SafeDispose(ref array);
		JobHandle jobHandle = ExecuteOverlapBoxCommands(nativeArray, results, maxResPerCast);
		nativeArray.Dispose(jobHandle);
		return jobHandle;
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapBounds(Bounds bounds, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleIgnoreCollision(bounds.center, layerMask);
		int count = UnityEngine.Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, colBuffer, Quaternion.identity, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	private static void BufferToList(Collider[] buffer, int count, List<Collider> list)
	{
		for (int i = 0; i < count; i++)
		{
			list.Add(buffer[i]);
			buffer[i] = null;
		}
	}

	public static bool CheckSphere<T>(Vector3 pos, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		OverlapSphere(pos, radius, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public static void CheckSpheres<T>(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, NativeArray<bool> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All) where T : Component
	{
		using (TimeWarning.New("GamePhysics.CheckSpheres<T>"))
		{
			using NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(positions.Length * maxResPerCast, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			OverlapSpheres(positions, radii, layerMasks, hits, maxResPerCast, triggerInteraction, validate).Complete();
			FindComponent<T>(hits.AsReadOnly(), maxResPerCast, results);
		}
	}

	private static void FindComponent<T>(NativeArray<ColliderHit>.ReadOnly hits, int maxResPerCast, NativeArray<bool> results)
	{
		using (TimeWarning.New("FindComponent<T>"))
		{
			int num = hits.Length / maxResPerCast;
			int batchSize = ThreadUtils.GetBatchSize(num);
			int num2 = (num + batchSize - 1) / batchSize;
			if (num2 >= 4)
			{
				List<UniTask> list = new List<UniTask>();
				for (int i = 0; i < num2; i++)
				{
					int num3 = i * batchSize;
					int end2 = Math.Min(num3 + batchSize, num);
					list.Add(FindCompAsync(hits, num3, end2, maxResPerCast, results));
				}
				ThreadUtils.WaitForTasks(list);
			}
			else
			{
				FindComp(hits, 0, num, maxResPerCast, results);
			}
		}
		static void FindComp(NativeArray<ColliderHit>.ReadOnly hits, int start, int end, int maxResPerCast, NativeArray<bool> results)
		{
			for (int j = start; j < end; j++)
			{
				bool value = false;
				int num4 = j * maxResPerCast;
				for (int k = 0; k < maxResPerCast; k++)
				{
					ColliderHit colliderHit = hits[num4 + k];
					if (colliderHit.instanceID == 0)
					{
						break;
					}
					if (colliderHit.collider.TryGetComponent<T>(out var _))
					{
						value = true;
						break;
					}
				}
				results[j] = value;
			}
		}
		static async UniTask FindCompAsync(NativeArray<ColliderHit>.ReadOnly hits, int start, int end, int maxResPerCast, NativeArray<bool> results)
		{
			await UnsafeScriptingAccess.SwitchToMultithreading();
			using (TimeWarning.New("FindComponentAsync<T>"))
			{
				using (UnsafeScriptingAccess.Start())
				{
					FindComp(hits, start, end, maxResPerCast, results);
				}
			}
		}
	}

	public static bool CheckCapsule<T>(Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		OverlapCapsule(start, end, radius, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public static bool CheckOBB<T>(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		OverlapOBB(obb, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public static bool CheckBounds<T>(Bounds bounds, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		OverlapBounds(bounds, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	private static bool CheckComponent<T>(List<Collider> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].gameObject.TryGetComponent<T>(out var _))
			{
				return true;
			}
		}
		return false;
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapSphere<T>(Vector3 position, float radius, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleIgnoreCollision(position, layerMask);
		int count = UnityEngine.Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	public static void CheckCapsules<T>(NativeArray<Vector3>.ReadOnly starts, NativeArray<Vector3>.ReadOnly ends, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly layerMasks, Span<bool> results, int maxResPerCast, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore, MasksToValidate validate = MasksToValidate.All, bool mitigateSpheres = true) where T : Component
	{
		using (TimeWarning.New("GamePhysics.CheckCapsules<T>"))
		{
			NativeArray<ColliderHit> hits = new NativeArray<ColliderHit>(starts.Length * maxResPerCast, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			OverlapCapsules(starts, ends, radii, layerMasks, hits, maxResPerCast, triggerInteraction, validate, mitigateSpheres).Complete();
			using (TimeWarning.New("FindComponent"))
			{
				for (int i = 0; i < starts.Length; i++)
				{
					bool flag = false;
					int num = i * maxResPerCast;
					for (int j = 0; j < maxResPerCast; j++)
					{
						ColliderHit colliderHit = hits[num + j];
						if (colliderHit.instanceID == 0)
						{
							break;
						}
						if (colliderHit.collider.TryGetComponent<T>(out var _))
						{
							flag = true;
							break;
						}
					}
					results[i] = flag;
				}
				hits.Dispose();
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapCapsule<T>(Vector3 point0, Vector3 point1, float radius, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleIgnoreCollision(point0, layerMask);
		layerMask = HandleIgnoreCollision(point1, layerMask);
		int count = UnityEngine.Physics.OverlapCapsuleNonAlloc(point0, point1, radius, colBuffer, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapOBB<T>(OBB obb, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleIgnoreCollision(obb.position, layerMask);
		int count = UnityEngine.Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	public static void OverlapBounds<T>(Bounds bounds, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleIgnoreCollision(bounds.center, layerMask);
		int count = UnityEngine.Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, colBuffer, Quaternion.identity, layerMask, triggerInteraction);
		BufferToList(colBuffer, count, list);
	}

	[PoolAnalyzerNonCaching]
	private static void BufferToList<T>(Collider[] buffer, int count, List<T> list) where T : Component
	{
		for (int i = 0; i < count; i++)
		{
			if (buffer[i].TryGetComponent<T>(out var component))
			{
				list.Add(component);
			}
			buffer[i] = null;
		}
	}

	[PoolAnalyzerNonCaching]
	private static void HitBufferToList(int count, List<RaycastHit> list)
	{
		if (count >= hitBuffer.Length)
		{
			Debug.LogWarning("Physics query is exceeding collider buffer length.");
		}
		for (int i = 0; i < count; i++)
		{
			list.Add(hitBuffer[i]);
		}
	}

	public static bool TraceRealm(Realm realm, Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		if (Trace(ray, radius, out var hitInfo2, maxDistance, layerMask, triggerInteraction, ignoreEntity))
		{
			hitInfo = hitInfo2;
			return true;
		}
		hitInfo = default(RaycastHit);
		return false;
	}

	public static void TraceRealmRays(Realm realm, NativeArray<RaycastCommand> cmds, NativeArray<RaycastHit> hits, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		TraceRays(cmds, hits, 1, traceWater, ignoreEntities);
	}

	public static BaseNetworkable TraceRealmEntity(Realm realm, Ray ray, float radius = 0f, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		if (!Trace(ray, radius, out var hitInfo, maxDistance, layerMask, triggerInteraction, ignoreEntity))
		{
			return null;
		}
		return RaycastHitEx.GetEntity(hitInfo);
	}

	public static bool Trace(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
		TraceAllUnordered(ray, radius, obj, maxDistance, layerMask, triggerInteraction, ignoreEntity);
		if (obj.Count == 0)
		{
			hitInfo = default(RaycastHit);
			Facepunch.Pool.FreeUnmanaged(ref obj);
			return false;
		}
		float num = obj[0].distance;
		int index = 0;
		for (int i = 1; i < obj.Count; i++)
		{
			float distance = obj[i].distance;
			if (distance < num)
			{
				num = distance;
				index = i;
			}
		}
		hitInfo = obj[index];
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return true;
	}

	[PoolAnalyzerNonCaching]
	public static void TraceAll(Ray ray, float radius, List<RaycastHit> hits, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		TraceAllUnordered(ray, radius, hits, maxDistance, layerMask, triggerInteraction, ignoreEntity);
		Sort(hits);
	}

	[PoolAnalyzerNonCaching]
	public static void TraceAllUnordered(Ray ray, float radius, List<RaycastHit> hits, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal, BaseEntity ignoreEntity = null)
	{
		int num = ((radius != 0f) ? UnityEngine.Physics.SphereCastNonAlloc(ray, radius, hitBuffer, maxDistance, layerMask, triggerInteraction) : UnityEngine.Physics.RaycastNonAlloc(ray, hitBuffer, maxDistance, layerMask, triggerInteraction));
		if (num < hitBuffer.Length && ((uint)layerMask & 0x10u) != 0 && WaterSystem.Trace(ray, out var position, out var normal, maxDistance))
		{
			RaycastHit raycastHit = default(RaycastHit);
			raycastHit.point = position;
			raycastHit.normal = normal;
			raycastHit.distance = (position - ray.origin).magnitude;
			RaycastHit raycastHit2 = raycastHit;
			hitBuffer[num++] = raycastHit2;
		}
		if (num == 0)
		{
			return;
		}
		if (num >= hitBuffer.Length)
		{
			Debug.LogWarning("Physics query is exceeding hit buffer length.");
		}
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit3 = hitBuffer[i];
			if (Verify(raycastHit3, ray.origin, ignoreEntity))
			{
				hits.Add(raycastHit3);
			}
		}
	}

	public static void TraceRaysUnordered(NativeArray<RaycastCommand> rays, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		using (TimeWarning.New("GamePhysics.TraceRaysUnordered"))
		{
			int batchSize = ThreadUtils.GetBatchSize(rays.Length);
			JobHandle jobHandle = RaycastCommand.ScheduleBatch(rays, hits, batchSize, maxHitsPerTrace);
			if (traceWater)
			{
				if (false)
				{
					JobHandle.ScheduleBatchedJobs();
					NativeArray<RaycastHit> hits2 = new NativeArray<RaycastHit>(rays.Length, Allocator.TempJob);
					JobHandle job = TraceWaterRaysDeferred(hits2, rays, 1, default(JobHandle));
					JobHandle dependsOn = JobHandle.CombineDependencies(jobHandle, job);
					GamePhysicsJobs.AppendRaycastHitsJob appendRaycastHitsJob = default(GamePhysicsJobs.AppendRaycastHitsJob);
					appendRaycastHitsJob.Dst = hits;
					appendRaycastHitsJob.Src = hits2.AsReadOnly();
					appendRaycastHitsJob.DstMaxHitsPerBatch = maxHitsPerTrace;
					appendRaycastHitsJob.SrcMaxHitsPerBatch = 1;
					GamePhysicsJobs.AppendRaycastHitsJob jobData = appendRaycastHitsJob;
					jobHandle = IJobExtensions.ScheduleByRef(ref jobData, dependsOn);
					hits2.Dispose(jobHandle);
				}
				else
				{
					jobHandle = TraceWaterRaysDeferred(hits, rays, maxHitsPerTrace, jobHandle);
				}
			}
			jobHandle.Complete();
			VerifyRays(hits, rays, maxHitsPerTrace, ignoreEntities);
		}
	}

	public static JobHandle TraceWaterRaysDeferred(NativeArray<RaycastHit> hits, NativeArray<RaycastCommand> rays, int maxHitsPerTrace, JobHandle inputDeps)
	{
		using (TimeWarning.New("ScheduleTraceWaterRaysDeferred"))
		{
			if (!rays.IsCreated || rays.Length == 0)
			{
				return inputDeps;
			}
			NativeList<Vector2i> waterIndices = new NativeList<Vector2i>(rays.Length, Allocator.TempJob);
			NativeList<Ray> nativeList = new NativeList<Ray>(rays.Length, Allocator.TempJob);
			NativeList<int> deepIndices = new NativeList<int>(rays.Length, Allocator.TempJob);
			NativeList<int> mainIndices = new NativeList<int>(rays.Length, Allocator.TempJob);
			NativeArray<bool> nativeArray = new NativeArray<bool>(rays.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<float> nativeArray2 = new NativeArray<float>(rays.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vector3> nativeArray3 = new NativeArray<Vector3>(rays.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vector3> nativeArray4 = new NativeArray<Vector3>(rays.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GamePhysicsJobs.PreProcessWaterRaysJob preProcessWaterRaysJob = default(GamePhysicsJobs.PreProcessWaterRaysJob);
			preProcessWaterRaysJob.hits = hits.AsReadOnly();
			preProcessWaterRaysJob.rays = rays.AsReadOnly();
			preProcessWaterRaysJob.maxHitsPerTrace = maxHitsPerTrace;
			preProcessWaterRaysJob.WaterIndices = waterIndices;
			preProcessWaterRaysJob.WaterRays = nativeList;
			preProcessWaterRaysJob.WaterMaxDists = nativeArray2;
			preProcessWaterRaysJob.DeepIndices = deepIndices;
			preProcessWaterRaysJob.MainIndices = mainIndices;
			preProcessWaterRaysJob.DeepSeaBounds = DeepSeaManager.DeepSeaBounds;
			GamePhysicsJobs.PreProcessWaterRaysJob jobData = preProcessWaterRaysJob;
			inputDeps = IJobExtensions.ScheduleByRef(ref jobData, inputDeps);
			inputDeps = WaterSystem.ScheduleTraceBatchDefer(nativeList, nativeArray2, nativeArray, nativeArray3, nativeArray4, deepIndices, mainIndices, inputDeps);
			GamePhysicsJobs.PostProcessWaterRaysJob postProcessWaterRaysJob = default(GamePhysicsJobs.PostProcessWaterRaysJob);
			postProcessWaterRaysJob.hits = hits;
			postProcessWaterRaysJob.rays = nativeList.AsDeferredJobArray();
			postProcessWaterRaysJob.WaterIndices = waterIndices;
			postProcessWaterRaysJob.hitsSub = nativeArray;
			postProcessWaterRaysJob.positionsSub = nativeArray3;
			postProcessWaterRaysJob.normalsSub = nativeArray4;
			GamePhysicsJobs.PostProcessWaterRaysJob jobData2 = postProcessWaterRaysJob;
			inputDeps = IJobExtensions.ScheduleByRef(ref jobData2, inputDeps);
			waterIndices.Dispose(inputDeps);
			nativeList.Dispose(inputDeps);
			deepIndices.Dispose(inputDeps);
			mainIndices.Dispose(inputDeps);
			nativeArray.Dispose(inputDeps);
			nativeArray2.Dispose(inputDeps);
			nativeArray3.Dispose(inputDeps);
			nativeArray4.Dispose(inputDeps);
			return inputDeps;
		}
	}

	public static void VerifyRays(NativeArray<RaycastHit> hits, NativeArray<RaycastCommand> rays, int maxHitsPerCast, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		if (rays.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifyRays"))
		{
			Debug.Assert(ignoreEntities.Length == 0 || rays.Length == ignoreEntities.Length);
			hits = hits.GetSubArray(0, rays.Length * maxHitsPerCast);
			using NativeList<RaycastHit> colliderHits = new NativeList<RaycastHit>(hits.Length, Allocator.TempJob);
			using NativeList<int> colliderIndices = new NativeList<int>(hits.Length, Allocator.TempJob);
			using NativeList<Vector3> waterHits = new NativeList<Vector3>(hits.Length, Allocator.TempJob);
			using NativeList<int> waterIndices = new NativeList<int>(hits.Length, Allocator.TempJob);
			FilterRaycastHitsJob filterRaycastHitsJob = default(FilterRaycastHitsJob);
			filterRaycastHitsJob.ColliderHits = colliderHits;
			filterRaycastHitsJob.ColliderIndices = colliderIndices;
			filterRaycastHitsJob.WaterHits = waterHits;
			filterRaycastHitsJob.WaterIndices = waterIndices;
			filterRaycastHitsJob.Hits = hits.AsReadOnly();
			filterRaycastHitsJob.HitsPerBatch = maxHitsPerCast;
			FilterRaycastHitsJob jobData = filterRaycastHitsJob;
			IJobExtensions.RunByRef(ref jobData);
			NativeArray<bool> results = new NativeArray<bool>(hits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			using NativeArray<bool> res = new NativeArray<bool>(waterHits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			VerifyWaterHits(waterHits.AsReadOnly(), res);
			ScatterToJob<bool> scatterToJob = default(ScatterToJob<bool>);
			scatterToJob.Results = results;
			scatterToJob.Source = res.AsReadOnly();
			scatterToJob.Indices = waterIndices.AsReadOnly();
			ScatterToJob<bool> jobData2 = scatterToJob;
			IJobExtensions.RunByRef(ref jobData2);
			using (TimeWarning.New("VerifyColliderHits"))
			{
				waterHits.Clear();
				if (waterHits.Capacity < colliderHits.Length)
				{
					waterHits.SetCapacity(colliderHits.Length);
				}
				waterIndices.Clear();
				if (waterIndices.Capacity < colliderHits.Length)
				{
					waterIndices.SetCapacity(colliderHits.Length);
				}
				for (int i = 0; i < colliderIndices.Length; i++)
				{
					RaycastHit raycastHit = colliderHits[i];
					int num = colliderIndices[i];
					Collider collider = raycastHit.collider;
					if (collider is TerrainCollider)
					{
						Vector3 vector = raycastHit.point;
						if (vector == Vector3.zero && raycastHit.distance == 0f)
						{
							int index = num / maxHitsPerCast;
							vector = rays[index].from;
						}
						waterHits.AddNoResize(vector);
						waterIndices.AddNoResize(num);
						continue;
					}
					bool flag = true;
					if (ignoreEntities != default(ReadOnlySpan<BaseEntity>))
					{
						int index2 = num / maxHitsPerCast;
						BaseEntity b = ignoreEntities[index2];
						if (CompareEntity(GameObjectEx.ToBaseEntity(collider), b))
						{
							flag = false;
						}
					}
					if (flag)
					{
						flag = collider.enabled;
					}
					results[num] = flag;
				}
			}
			using NativeArray<bool> res2 = new NativeArray<bool>(hits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			VerifyTerrainColliderHits(waterHits.AsReadOnly(), res2);
			scatterToJob = default(ScatterToJob<bool>);
			scatterToJob.Results = results;
			scatterToJob.Source = res2.AsReadOnly();
			scatterToJob.Indices = waterIndices.AsReadOnly();
			ScatterToJob<bool> jobData3 = scatterToJob;
			IJobExtensions.RunByRef(ref jobData3);
			RemoveInvalidRaycastHitsJob removeInvalidRaycastHitsJob = default(RemoveInvalidRaycastHitsJob);
			removeInvalidRaycastHitsJob.Hits = hits;
			removeInvalidRaycastHitsJob.AreValid = results.AsReadOnly();
			removeInvalidRaycastHitsJob.HitsPerBatch = maxHitsPerCast;
			RemoveInvalidRaycastHitsJob jobData4 = removeInvalidRaycastHitsJob;
			IJobExtensions.RunByRef(ref jobData4);
			results.Dispose();
		}
	}

	private static void VerifyWaterHits(NativeArray<Vector3>.ReadOnly hits, NativeArray<bool> res)
	{
		if (hits.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifyWaterHits"))
		{
			if (WaterSystem.Collision == null)
			{
				FillJob<bool> fillJob = default(FillJob<bool>);
				fillJob.Values = res;
				fillJob.Value = true;
				FillJob<bool> jobData = fillJob;
				IJobExtensions.RunByRef(ref jobData);
				return;
			}
			using NativeArray<float> values = new NativeArray<float>(hits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			FillJob<float> fillJob2 = default(FillJob<float>);
			fillJob2.Values = values;
			fillJob2.Value = 0.01f;
			FillJob<float> jobData2 = fillJob2;
			IJobExtensions.RunByRef(ref jobData2);
			WaterSystem.Collision.GetIgnore(hits, values.AsReadOnly(), res);
			FlipBoolJob flipBoolJob = default(FlipBoolJob);
			flipBoolJob.Values = res;
			FlipBoolJob jobData3 = flipBoolJob;
			IJobExtensions.RunByRef(ref jobData3);
		}
	}

	private static void VerifyTerrainColliderHits(NativeArray<Vector3>.ReadOnly hits, NativeArray<bool> res)
	{
		if (hits.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifyTerrainColliderHits"))
		{
			if (TerrainMeta.Collision == null)
			{
				FillJob<bool> fillJob = default(FillJob<bool>);
				fillJob.Values = res;
				fillJob.Value = true;
				FillJob<bool> jobData = fillJob;
				IJobExtensions.RunByRef(ref jobData);
				return;
			}
			using NativeArray<float> values = new NativeArray<float>(hits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			FillJob<float> fillJob2 = default(FillJob<float>);
			fillJob2.Values = values;
			fillJob2.Value = 0.01f;
			FillJob<float> jobData2 = fillJob2;
			IJobExtensions.RunByRef(ref jobData2);
			TerrainMeta.Collision.GetIgnore(hits, values.AsReadOnly(), res);
			FlipBoolJob flipBoolJob = default(FlipBoolJob);
			flipBoolJob.Values = res;
			FlipBoolJob jobData3 = flipBoolJob;
			IJobExtensions.RunByRef(ref jobData3);
		}
	}

	public static void TraceRays(NativeArray<RaycastCommand> rays, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		if (rays.Length != 0)
		{
			int num = Mathf.Max(32, maxHitsPerTrace * 2);
			NativeArray<RaycastHit> nativeArray = new NativeArray<RaycastHit>(rays.Length * num, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			TraceRaysUnordered(rays, nativeArray, num, traceWater, ignoreEntities);
			SelectNearest(maxHitsPerTrace, nativeArray, hits, rays.Length, num).Complete();
			nativeArray.Dispose();
		}
	}

	public static void TraceSpheresUnordered(NativeArray<SpherecastCommand> spheres, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		using (TimeWarning.New("GamePhysics.TraceSpheresUnordered"))
		{
			int batchSize = ThreadUtils.GetBatchSize(spheres.Length);
			JobHandle inputDeps = SpherecastCommand.ScheduleBatch(spheres, hits, batchSize, maxHitsPerTrace);
			if (traceWater)
			{
				inputDeps = TraceWaterSpheresDeferred(hits, spheres, maxHitsPerTrace, inputDeps);
			}
			inputDeps.Complete();
			VerifySpheres(hits, spheres, maxHitsPerTrace, ignoreEntities);
		}
	}

	public static JobHandle TraceWaterSpheresDeferred(NativeArray<RaycastHit> hits, NativeArray<SpherecastCommand> spheres, int maxHitsPerTrace, JobHandle inputDeps)
	{
		using (TimeWarning.New("ScheduleTraceWaterSpheresDeferred"))
		{
			if (!spheres.IsCreated || spheres.Length == 0)
			{
				return inputDeps;
			}
			NativeList<Vector2i> waterIndices = new NativeList<Vector2i>(spheres.Length, Allocator.TempJob);
			NativeList<Ray> nativeList = new NativeList<Ray>(spheres.Length, Allocator.TempJob);
			NativeList<int> deepIndices = new NativeList<int>(spheres.Length, Allocator.TempJob);
			NativeList<int> mainIndices = new NativeList<int>(spheres.Length, Allocator.TempJob);
			NativeArray<bool> nativeArray = new NativeArray<bool>(spheres.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<float> nativeArray2 = new NativeArray<float>(spheres.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vector3> nativeArray3 = new NativeArray<Vector3>(spheres.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<Vector3> nativeArray4 = new NativeArray<Vector3>(spheres.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			GamePhysicsJobs.PreProcessWaterSpheresJob preProcessWaterSpheresJob = default(GamePhysicsJobs.PreProcessWaterSpheresJob);
			preProcessWaterSpheresJob.hits = hits.AsReadOnly();
			preProcessWaterSpheresJob.rays = spheres.AsReadOnly();
			preProcessWaterSpheresJob.maxHitsPerTrace = maxHitsPerTrace;
			preProcessWaterSpheresJob.WaterIndices = waterIndices;
			preProcessWaterSpheresJob.WaterRays = nativeList;
			preProcessWaterSpheresJob.WaterMaxDists = nativeArray2;
			preProcessWaterSpheresJob.DeepIndices = deepIndices;
			preProcessWaterSpheresJob.MainIndices = mainIndices;
			preProcessWaterSpheresJob.DeepSeaBounds = DeepSeaManager.DeepSeaBounds;
			GamePhysicsJobs.PreProcessWaterSpheresJob jobData = preProcessWaterSpheresJob;
			inputDeps = IJobExtensions.ScheduleByRef(ref jobData, inputDeps);
			inputDeps = WaterSystem.ScheduleTraceBatchDefer(nativeList, nativeArray2, nativeArray, nativeArray3, nativeArray4, deepIndices, mainIndices, inputDeps);
			GamePhysicsJobs.PostProcessWaterRaysJob postProcessWaterRaysJob = default(GamePhysicsJobs.PostProcessWaterRaysJob);
			postProcessWaterRaysJob.hits = hits;
			postProcessWaterRaysJob.rays = nativeList.AsDeferredJobArray();
			postProcessWaterRaysJob.WaterIndices = waterIndices;
			postProcessWaterRaysJob.hitsSub = nativeArray;
			postProcessWaterRaysJob.positionsSub = nativeArray3;
			postProcessWaterRaysJob.normalsSub = nativeArray4;
			GamePhysicsJobs.PostProcessWaterRaysJob jobData2 = postProcessWaterRaysJob;
			inputDeps = IJobExtensions.ScheduleByRef(ref jobData2, inputDeps);
			waterIndices.Dispose(inputDeps);
			nativeList.Dispose(inputDeps);
			deepIndices.Dispose(inputDeps);
			mainIndices.Dispose(inputDeps);
			nativeArray.Dispose(inputDeps);
			nativeArray2.Dispose(inputDeps);
			nativeArray3.Dispose(inputDeps);
			nativeArray4.Dispose(inputDeps);
			return inputDeps;
		}
	}

	public static void VerifySpheres(NativeArray<RaycastHit> hits, NativeArray<SpherecastCommand> spheres, int maxHitsPerCast, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		if (spheres.Length == 0)
		{
			return;
		}
		using (TimeWarning.New("VerifySpheres"))
		{
			Debug.Assert(ignoreEntities.IsEmpty || spheres.Length == ignoreEntities.Length);
			hits = hits.GetSubArray(0, spheres.Length * maxHitsPerCast);
			using NativeList<RaycastHit> colliderHits = new NativeList<RaycastHit>(hits.Length, Allocator.TempJob);
			using NativeList<int> colliderIndices = new NativeList<int>(hits.Length, Allocator.TempJob);
			using NativeList<Vector3> waterHits = new NativeList<Vector3>(hits.Length, Allocator.TempJob);
			using NativeList<int> waterIndices = new NativeList<int>(hits.Length, Allocator.TempJob);
			FilterRaycastHitsJob filterRaycastHitsJob = default(FilterRaycastHitsJob);
			filterRaycastHitsJob.ColliderHits = colliderHits;
			filterRaycastHitsJob.ColliderIndices = colliderIndices;
			filterRaycastHitsJob.WaterHits = waterHits;
			filterRaycastHitsJob.WaterIndices = waterIndices;
			filterRaycastHitsJob.Hits = hits.AsReadOnly();
			filterRaycastHitsJob.HitsPerBatch = maxHitsPerCast;
			FilterRaycastHitsJob jobData = filterRaycastHitsJob;
			IJobExtensions.RunByRef(ref jobData);
			NativeArray<bool> results = new NativeArray<bool>(hits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			using NativeArray<bool> res = new NativeArray<bool>(waterHits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			VerifyWaterHits(waterHits.AsReadOnly(), res);
			ScatterToJob<bool> scatterToJob = default(ScatterToJob<bool>);
			scatterToJob.Results = results;
			scatterToJob.Source = res.AsReadOnly();
			scatterToJob.Indices = waterIndices.AsReadOnly();
			ScatterToJob<bool> jobData2 = scatterToJob;
			IJobExtensions.RunByRef(ref jobData2);
			using (TimeWarning.New("VerifyColliderHits"))
			{
				waterHits.Clear();
				if (waterHits.Capacity < colliderHits.Length)
				{
					waterHits.SetCapacity(colliderHits.Length);
				}
				waterIndices.Clear();
				if (waterIndices.Capacity < colliderHits.Length)
				{
					waterIndices.SetCapacity(colliderHits.Length);
				}
				for (int i = 0; i < colliderIndices.Length; i++)
				{
					RaycastHit raycastHit = colliderHits[i];
					int num = colliderIndices[i];
					Collider collider = raycastHit.collider;
					if (collider is TerrainCollider)
					{
						Vector3 vector = raycastHit.point;
						if (vector == Vector3.zero && raycastHit.distance == 0f)
						{
							int index = num / maxHitsPerCast;
							vector = spheres[index].origin;
						}
						waterHits.AddNoResize(vector);
						waterIndices.AddNoResize(num);
						continue;
					}
					bool flag = true;
					if (ignoreEntities != default(ReadOnlySpan<BaseEntity>))
					{
						int index2 = num / maxHitsPerCast;
						BaseEntity b = ignoreEntities[index2];
						if (CompareEntity(GameObjectEx.ToBaseEntity(collider), b))
						{
							flag = false;
						}
					}
					if (flag)
					{
						flag = collider.enabled;
					}
					results[num] = flag;
				}
			}
			using NativeArray<bool> res2 = new NativeArray<bool>(hits.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			VerifyTerrainColliderHits(waterHits.AsReadOnly(), res2);
			scatterToJob = default(ScatterToJob<bool>);
			scatterToJob.Results = results;
			scatterToJob.Source = res2.AsReadOnly();
			scatterToJob.Indices = waterIndices.AsReadOnly();
			ScatterToJob<bool> jobData3 = scatterToJob;
			IJobExtensions.RunByRef(ref jobData3);
			RemoveInvalidRaycastHitsJob removeInvalidRaycastHitsJob = default(RemoveInvalidRaycastHitsJob);
			removeInvalidRaycastHitsJob.Hits = hits;
			removeInvalidRaycastHitsJob.AreValid = results.AsReadOnly();
			removeInvalidRaycastHitsJob.HitsPerBatch = maxHitsPerCast;
			RemoveInvalidRaycastHitsJob jobData4 = removeInvalidRaycastHitsJob;
			IJobExtensions.RunByRef(ref jobData4);
			results.Dispose();
		}
	}

	public static void TraceSpheres(NativeArray<SpherecastCommand> spheres, NativeArray<RaycastHit> hits, int maxHitsPerTrace, bool traceWater = true, ReadOnlySpan<BaseEntity> ignoreEntities = default(ReadOnlySpan<BaseEntity>))
	{
		if (spheres.Length != 0)
		{
			int num = Mathf.Max(32, maxHitsPerTrace * 2);
			NativeArray<RaycastHit> nativeArray = new NativeArray<RaycastHit>(spheres.Length * num, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			TraceSpheresUnordered(spheres, nativeArray, num, traceWater, ignoreEntities);
			SelectNearest(maxHitsPerTrace, nativeArray, hits, spheres.Length, num).Complete();
			nativeArray.Dispose();
		}
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding0, float padding1, BaseEntity ignoreEntity = null)
	{
		return LineOfSightInternal(p0, p1, layerMask, radius, padding0, padding1, ignoreEntity);
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding, BaseEntity ignoreEntity = null)
	{
		return LineOfSightInternal(p0, p1, layerMask, radius, padding, padding, ignoreEntity);
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, BaseEntity ignoreEntity = null)
	{
		return LineOfSightInternal(p0, p1, layerMask, radius, 0f, 0f, ignoreEntity);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, float padding0, float padding1, BaseEntity ignoreEntity = null)
	{
		return LineOfSightRadius(p0, p1, layerMask, 0f, padding0, padding1, ignoreEntity);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, float padding, BaseEntity ignoreEntity = null)
	{
		return LineOfSightRadius(p0, p1, layerMask, 0f, padding, padding, ignoreEntity);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, BaseEntity ignoreEntity = null)
	{
		return LineOfSightRadius(p0, p1, layerMask, 0f, 0f, 0f, ignoreEntity);
	}

	private static bool LineOfSightInternal(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding0, float padding1, BaseEntity ignoreEntity = null)
	{
		if (!ValidBounds.TestOuterBounds(p0))
		{
			return false;
		}
		if (!ValidBounds.TestOuterBounds(p1))
		{
			return false;
		}
		Vector3 vector = p1 - p0;
		float magnitude = vector.magnitude;
		if (magnitude <= padding0 + padding1)
		{
			return true;
		}
		Vector3 vector2 = vector / magnitude;
		Ray ray = new Ray(p0 + vector2 * padding0, vector2);
		float maxDistance = magnitude - padding0 - padding1;
		bool flag;
		RaycastHit hitInfo;
		if (!ignoreEntity.IsRealNull() || ((uint)layerMask & 0x800000u) != 0)
		{
			flag = Trace(ray, 0f, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore, ignoreEntity);
			if (radius > 0f && !flag)
			{
				flag = Trace(ray, radius, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore, ignoreEntity);
			}
		}
		else
		{
			flag = UnityEngine.Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
			if (radius > 0f && !flag)
			{
				flag = UnityEngine.Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
			}
		}
		if (!flag)
		{
			if (ConVar.Vis.lineofsight)
			{
				ConsoleNetwork.BroadcastToAllClients("ddraw.line", 60f, Color.green, p0, p1);
			}
			return true;
		}
		if (ConVar.Vis.lineofsight)
		{
			ConsoleNetwork.BroadcastToAllClients("ddraw.line", 60f, Color.red, p0, p1);
			ConsoleNetwork.BroadcastToAllClients("ddraw.text", 60f, Color.white, hitInfo.point, hitInfo.collider.name);
		}
		return false;
	}

	public static bool Verify(RaycastHit hitInfo, Vector3 rayOrigin, BaseEntity ignoreEntity = null)
	{
		Vector3 vector = hitInfo.point;
		if (hitInfo.collider is TerrainCollider && vector == Vector3.zero && hitInfo.distance == 0f)
		{
			vector = rayOrigin;
		}
		return Verify(hitInfo.collider, vector, ignoreEntity);
	}

	public static bool Verify(Collider collider, Vector3 point, BaseEntity ignoreEntity = null)
	{
		if (collider == null)
		{
			if ((bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(point))
			{
				return false;
			}
			return true;
		}
		if (collider is TerrainCollider)
		{
			if ((bool)TerrainMeta.Collision && TerrainMeta.Collision.GetIgnore(point))
			{
				return false;
			}
			return true;
		}
		if (!ignoreEntity.IsRealNull() && CompareEntity(GameObjectEx.ToBaseEntity(collider), ignoreEntity))
		{
			return false;
		}
		return collider.enabled;
	}

	public static bool CompareEntity(BaseEntity a, BaseEntity b)
	{
		if (a.IsRealNull() || b.IsRealNull())
		{
			return false;
		}
		if (a == b)
		{
			return true;
		}
		return false;
	}

	public static int HandleIgnoreCollision(Vector3 position, int layerMask)
	{
		int num = 8388608;
		if ((layerMask & num) != 0 && (bool)TerrainMeta.Collision && TerrainMeta.Collision.GetIgnore(position))
		{
			layerMask &= ~num;
		}
		int num2 = 16;
		if ((layerMask & num2) != 0 && (bool)WaterSystem.Collision && WaterSystem.Collision.GetIgnore(position))
		{
			layerMask &= ~num2;
		}
		return layerMask;
	}

	public static void HandleIgnoreTerrain(NativeArray<Vector3>.ReadOnly positions, NativeArray<bool> hitIgnoreVolumes)
	{
		NativeArray<float> values = new NativeArray<float>(positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		FillJob<float> fillJob = default(FillJob<float>);
		fillJob.Values = values;
		fillJob.Value = 0.01f;
		FillJob<float> jobData = fillJob;
		IJobExtensions.RunByRef(ref jobData);
		TerrainMeta.Collision.GetIgnore(positions, values.AsReadOnly(), hitIgnoreVolumes);
		values.Dispose();
	}

	public static void HandleIgnoreWater(NativeArray<Vector3>.ReadOnly positions, NativeArray<bool> hitIgnoreVolumes)
	{
		NativeArray<float> values = new NativeArray<float>(positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		FillJob<float> fillJob = default(FillJob<float>);
		fillJob.Values = values;
		fillJob.Value = 0.01f;
		FillJob<float> jobData = fillJob;
		IJobExtensions.RunByRef(ref jobData);
		WaterSystem.Collision.GetIgnore(positions, values.AsReadOnly(), hitIgnoreVolumes);
		values.Dispose();
	}

	public static void HandleIgnoreCollision(NativeArray<Vector3>.ReadOnly positions, NativeArray<int> layerMasks, MasksToValidate validate = MasksToValidate.All)
	{
		if ((validate & MasksToValidate.Terrain) == MasksToValidate.Terrain)
		{
			NativeArray<bool> hitIgnoreVolumes = new NativeArray<bool>(positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			HandleIgnoreTerrain(positions, hitIgnoreVolumes);
			RemoveLayerMaskJob removeLayerMaskJob = default(RemoveLayerMaskJob);
			removeLayerMaskJob.LayerMasks = layerMasks;
			removeLayerMaskJob.ShouldIgnore = hitIgnoreVolumes.AsReadOnly();
			removeLayerMaskJob.MaskToRemove = 8388608;
			RemoveLayerMaskJob jobData = removeLayerMaskJob;
			IJobExtensions.RunByRef(ref jobData);
			hitIgnoreVolumes.Dispose();
		}
		if ((validate & MasksToValidate.Water) == MasksToValidate.Water)
		{
			NativeArray<bool> hitIgnoreVolumes2 = new NativeArray<bool>(positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			HandleIgnoreWater(positions, hitIgnoreVolumes2);
			RemoveLayerMaskJob removeLayerMaskJob = default(RemoveLayerMaskJob);
			removeLayerMaskJob.LayerMasks = layerMasks;
			removeLayerMaskJob.ShouldIgnore = hitIgnoreVolumes2.AsReadOnly();
			removeLayerMaskJob.MaskToRemove = 16;
			RemoveLayerMaskJob jobData2 = removeLayerMaskJob;
			IJobExtensions.RunByRef(ref jobData2);
			hitIgnoreVolumes2.Dispose();
		}
	}

	[PoolAnalyzerNonCaching]
	public static void Sort(List<RaycastHit> hits)
	{
		hits.Sort((RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
	}

	public static void Sort(RaycastHit[] hits)
	{
		Array.Sort(hits, (RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
	}

	public static void Sort(NativeArray<RaycastHit> hits, int queryCount, int maxHitsPerQuery)
	{
		using (TimeWarning.New("GamePhysics.Sort"))
		{
			SortDeferred(hits, queryCount, maxHitsPerQuery).Complete();
		}
	}

	public static JobHandle SortDeferred(NativeArray<RaycastHit> hits, int queryCount, int maxHitsPerQuery, JobHandle dependsOn = default(JobHandle))
	{
		using (TimeWarning.New("GamePhysics.SortDeferred"))
		{
			SortHitsJob<RaycastHitComparer> sortHitsJob = default(SortHitsJob<RaycastHitComparer>);
			sortHitsJob.Hits = hits;
			sortHitsJob.MaxHitsPerRay = maxHitsPerQuery;
			sortHitsJob.Comp = default(RaycastHitComparer);
			SortHitsJob<RaycastHitComparer> jobData = sortHitsJob;
			int batchSize = ThreadUtils.GetBatchSize(queryCount);
			return IJobForExtensions.ScheduleParallelByRef(ref jobData, queryCount, batchSize, dependsOn);
		}
	}

	public static JobHandle SelectNearest(int nearestCount, NativeArray<RaycastHit> from, NativeArray<RaycastHit> to, int queryCount, int maxHitsPerQuery, JobHandle dependsOn = default(JobHandle))
	{
		Debug.Assert(maxHitsPerQuery > 0, "Invalid maxHitsPerQuery param!");
		Debug.Assert(nearestCount < maxHitsPerQuery, "Invalid nearestCount param!");
		if (nearestCount == 1)
		{
			SelectNearestHitsJob selectNearestHitsJob = default(SelectNearestHitsJob);
			selectNearestHitsJob.Results = to;
			selectNearestHitsJob.Hits = from.AsReadOnly();
			selectNearestHitsJob.HitsPerBatch = maxHitsPerQuery;
			SelectNearestHitsJob jobData = selectNearestHitsJob;
			return IJobExtensions.ScheduleByRef(ref jobData, dependsOn);
		}
		JobHandle dependsOn2 = SortDeferred(from, queryCount, maxHitsPerQuery, dependsOn);
		SelectNearestNHitsJob selectNearestNHitsJob = default(SelectNearestNHitsJob);
		selectNearestNHitsJob.Results = to;
		selectNearestNHitsJob.Hits = from.AsReadOnly();
		selectNearestNHitsJob.HitsPerBatch = maxHitsPerQuery;
		selectNearestNHitsJob.SelectCount = nearestCount;
		SelectNearestNHitsJob jobData2 = selectNearestNHitsJob;
		return IJobExtensions.ScheduleByRef(ref jobData2, dependsOn2);
	}
}
