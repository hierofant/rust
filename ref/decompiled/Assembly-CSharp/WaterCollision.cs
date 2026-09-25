using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public class WaterCollision : MonoBehaviour
{
	private ListDictionary<Collider, List<Collider>> ignoredColliders;

	private HashSet<Collider> waterColliders;

	private WaterVisibilityGrid visibilityGrid;

	public const float IgnoreRadius = 0.01f;

	private NativeList<int> indicesToCheck;

	public WaterVisibilityGrid VisibilityGrid => visibilityGrid;

	public void Setup()
	{
		ignoredColliders = new ListDictionary<Collider, List<Collider>>();
		waterColliders = new HashSet<Collider>();
		if (visibilityGrid != null)
		{
			visibilityGrid.Dispose();
		}
		visibilityGrid = new WaterVisibilityGrid();
	}

	private void OnDestroy()
	{
		visibilityGrid?.Dispose();
		NativeListEx.SafeDispose(ref indicesToCheck);
	}

	public void Clear()
	{
		if (waterColliders.Count == 0)
		{
			return;
		}
		HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
		while (enumerator.MoveNext())
		{
			foreach (Collider key in ignoredColliders.Keys)
			{
				Physics.IgnoreCollision(key, enumerator.Current, ignore: false);
			}
		}
		ignoredColliders.Clear();
	}

	public void Reset(Collider collider)
	{
		if (waterColliders.Count != 0 && (bool)collider)
		{
			HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Physics.IgnoreCollision(collider, enumerator.Current, ignore: false);
			}
			ignoredColliders.Remove(collider);
		}
	}

	public bool GetIgnore(Vector3 pos, float radius = 0.01f)
	{
		WaterVisibilityGrid waterVisibilityGrid = visibilityGrid;
		if (waterVisibilityGrid != null && !waterVisibilityGrid.Check(pos, radius))
		{
			return false;
		}
		return GamePhysics.CheckSphere<WaterVisibilityTrigger>(pos, radius, 262144, QueryTriggerInteraction.Collide);
	}

	private void PrepareIndiciesToCheckList(int length)
	{
		if (!indicesToCheck.IsCreated)
		{
			indicesToCheck = new NativeList<int>(length, Allocator.Persistent);
			return;
		}
		if (length > indicesToCheck.Capacity)
		{
			indicesToCheck.Capacity = length;
		}
		indicesToCheck.Clear();
	}

	public void GetIgnore(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeArray<bool> results)
	{
		using (TimeWarning.New("WaterCollision.GetIgnore"))
		{
			FillJob<bool> fillJob = default(FillJob<bool>);
			fillJob.Values = results;
			fillJob.Value = false;
			FillJob<bool> jobData = fillJob;
			IJobExtensions.RunByRef(ref jobData);
			PrepareIndiciesToCheckList(positions.Length);
			JobHandle jobHandle;
			if (visibilityGrid != null)
			{
				jobHandle = visibilityGrid.Check(positions, radii, indicesToCheck);
			}
			else
			{
				GenerateAscSeqListJob jobData2 = default(GenerateAscSeqListJob);
				jobData2.Values = indicesToCheck;
				jobData2.Start = 0;
				jobData2.Step = 1;
				jobData2.Count = positions.Length;
				jobHandle = jobData2.Schedule();
			}
			jobHandle.Complete();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> results2 = new NativeArray<Vector3>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<Vector3> gatherJob = default(GatherJob<Vector3>);
				gatherJob.Results = results2;
				gatherJob.Source = positions;
				gatherJob.Indices = indicesToCheck.AsReadOnly();
				GatherJob<Vector3> jobData3 = gatherJob;
				IJobExtensions.RunByRef(ref jobData3);
				NativeArray<float> results3 = new NativeArray<float>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<float> gatherJob2 = default(GatherJob<float>);
				gatherJob2.Results = results3;
				gatherJob2.Source = radii;
				gatherJob2.Indices = indicesToCheck.AsReadOnly();
				GatherJob<float> jobData4 = gatherJob2;
				IJobExtensions.RunByRef(ref jobData4);
				NativeArray<int> values = new NativeArray<int>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				FillJob<int> fillJob2 = default(FillJob<int>);
				fillJob2.Values = values;
				fillJob2.Value = 262144;
				FillJob<int> jobData5 = fillJob2;
				IJobExtensions.RunByRef(ref jobData5);
				GamePhysics.CheckSpheres<WaterVisibilityTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), results, GamePhysics.DefaultMaxResultsPerQuery, QueryTriggerInteraction.Collide, GamePhysics.MasksToValidate.None);
				Span<bool> values2 = results;
				NativeArray<int>.ReadOnly source = indicesToCheck.AsReadOnly();
				CollectionUtil.ScatterOutInplace(values2, source, defValue: false);
				values.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
		}
	}

	public void GetIgnoreIndirect(NativeArray<Vector3>.ReadOnly pos, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly indices, NativeArray<bool> results)
	{
		using (TimeWarning.New("WaterCollision.GetIgnoreIndirect"))
		{
			FillJob<bool> fillJob = default(FillJob<bool>);
			fillJob.Values = results;
			fillJob.Value = false;
			FillJob<bool> jobData = fillJob;
			IJobExtensions.RunByRef(ref jobData);
			PrepareIndiciesToCheckList(indices.Length);
			JobHandle jobHandle = default(JobHandle);
			if (visibilityGrid != null)
			{
				jobHandle = visibilityGrid.CheckIndirect(pos, radii, indices, indicesToCheck);
			}
			else
			{
				indicesToCheck.CopyFrom(in indices);
			}
			jobHandle.Complete();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> results2 = new NativeArray<Vector3>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<Vector3> gatherJob = default(GatherJob<Vector3>);
				gatherJob.Results = results2;
				gatherJob.Source = pos;
				gatherJob.Indices = indicesToCheck.AsReadOnly();
				GatherJob<Vector3> jobData2 = gatherJob;
				IJobExtensions.RunByRef(ref jobData2);
				NativeArray<float> results3 = new NativeArray<float>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<float> gatherJob2 = default(GatherJob<float>);
				gatherJob2.Results = results3;
				gatherJob2.Source = radii;
				gatherJob2.Indices = indicesToCheck.AsReadOnly();
				GatherJob<float> jobData3 = gatherJob2;
				IJobExtensions.RunByRef(ref jobData3);
				NativeArray<int> values = new NativeArray<int>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				FillJob<int> fillJob2 = default(FillJob<int>);
				fillJob2.Values = values;
				fillJob2.Value = 262144;
				FillJob<int> jobData4 = fillJob2;
				IJobExtensions.RunByRef(ref jobData4);
				NativeArray<bool> source = new NativeArray<bool>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GamePhysics.CheckSpheres<WaterVisibilityTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), source, GamePhysics.DefaultMaxResultsPerQuery, QueryTriggerInteraction.Collide, GamePhysics.MasksToValidate.None);
				ReadOnlySpan<bool> from = source;
				Span<bool> to = results;
				NativeArray<int>.ReadOnly source2 = indicesToCheck.AsReadOnly();
				CollectionUtil.ScatterTo(from, to, source2);
				source.Dispose();
				values.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
		}
	}

	public bool GetIgnore(Bounds bounds)
	{
		WaterVisibilityGrid waterVisibilityGrid = visibilityGrid;
		if (waterVisibilityGrid != null && !waterVisibilityGrid.Check(bounds))
		{
			return false;
		}
		return GamePhysics.CheckBounds<WaterVisibilityTrigger>(bounds, 262144, QueryTriggerInteraction.Collide);
	}

	public bool GetIgnore(Vector3 start, Vector3 end, float radius)
	{
		WaterVisibilityGrid waterVisibilityGrid = visibilityGrid;
		if (waterVisibilityGrid != null && !waterVisibilityGrid.Check(start, end, radius))
		{
			return false;
		}
		return GamePhysics.CheckCapsule<WaterVisibilityTrigger>(start, end, radius, 262144, QueryTriggerInteraction.Collide);
	}

	public void GetIgnoreIndirect(NativeArray<Vector3>.ReadOnly starts, NativeArray<Vector3>.ReadOnly ends, NativeArray<float>.ReadOnly radii, NativeArray<int>.ReadOnly indices, NativeArray<bool> results)
	{
		using (TimeWarning.New("WaterCollision.GetIgnoreIndirect"))
		{
			FillJob<bool> fillJob = default(FillJob<bool>);
			fillJob.Values = results;
			fillJob.Value = false;
			FillJob<bool> jobData = fillJob;
			IJobExtensions.RunByRef(ref jobData);
			PrepareIndiciesToCheckList(indices.Length);
			JobHandle jobHandle = default(JobHandle);
			if (visibilityGrid != null)
			{
				jobHandle = visibilityGrid.CheckIndirect(starts, ends, radii, indices, indicesToCheck);
			}
			else
			{
				indicesToCheck.CopyFrom(in indices);
			}
			jobHandle.Complete();
			if (!indicesToCheck.IsEmpty)
			{
				NativeArray<Vector3> results2 = new NativeArray<Vector3>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<Vector3> gatherJob = default(GatherJob<Vector3>);
				gatherJob.Results = results2;
				gatherJob.Source = starts;
				gatherJob.Indices = indicesToCheck.AsReadOnly();
				GatherJob<Vector3> jobData2 = gatherJob;
				IJobExtensions.RunByRef(ref jobData2);
				NativeArray<Vector3> results3 = new NativeArray<Vector3>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				gatherJob = default(GatherJob<Vector3>);
				gatherJob.Results = results3;
				gatherJob.Source = ends;
				gatherJob.Indices = indicesToCheck.AsReadOnly();
				GatherJob<Vector3> jobData3 = gatherJob;
				IJobExtensions.RunByRef(ref jobData3);
				NativeArray<float> results4 = new NativeArray<float>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<float> gatherJob2 = default(GatherJob<float>);
				gatherJob2.Results = results4;
				gatherJob2.Source = radii;
				gatherJob2.Indices = indicesToCheck.AsReadOnly();
				GatherJob<float> jobData4 = gatherJob2;
				IJobExtensions.RunByRef(ref jobData4);
				NativeArray<int> values = new NativeArray<int>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				FillJob<int> fillJob2 = default(FillJob<int>);
				fillJob2.Values = values;
				fillJob2.Value = 262144;
				FillJob<int> jobData5 = fillJob2;
				IJobExtensions.RunByRef(ref jobData5);
				NativeArray<bool> source = new NativeArray<bool>(indicesToCheck.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GamePhysics.CheckCapsules<WaterVisibilityTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), results4.AsReadOnly(), values.AsReadOnly(), source, GamePhysics.DefaultMaxResultsPerQuery, QueryTriggerInteraction.Collide, GamePhysics.MasksToValidate.None);
				ReadOnlySpan<bool> from = source;
				Span<bool> to = results;
				NativeArray<int>.ReadOnly source2 = indicesToCheck.AsReadOnly();
				CollectionUtil.ScatterTo(from, to, source2);
				source.Dispose();
				values.Dispose();
				results4.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
		}
	}

	public bool GetIgnore(RaycastHit hit)
	{
		if (waterColliders.Contains(hit.collider))
		{
			return GetIgnore(hit.point);
		}
		return false;
	}

	public bool GetIgnore(Collider collider)
	{
		if (waterColliders.Count == 0 || !collider)
		{
			return false;
		}
		return ignoredColliders.Contains(collider);
	}

	public void SetIgnore(Collider collider, Collider trigger, bool ignore = true)
	{
		if (waterColliders.Count == 0 || !collider)
		{
			return;
		}
		if (!GetIgnore(collider))
		{
			if (ignore)
			{
				List<Collider> val = new List<Collider> { trigger };
				HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Physics.IgnoreCollision(collider, enumerator.Current, ignore: true);
				}
				ignoredColliders.Add(collider, val);
			}
			return;
		}
		List<Collider> list = ignoredColliders[collider];
		if (ignore)
		{
			if (!list.Contains(trigger))
			{
				list.Add(trigger);
			}
		}
		else if (list.Contains(trigger))
		{
			list.Remove(trigger);
		}
	}

	protected void LateUpdate()
	{
		if (ignoredColliders == null)
		{
			return;
		}
		for (int i = 0; i < ignoredColliders.Count; i++)
		{
			KeyValuePair<Collider, List<Collider>> byIndex = ignoredColliders.GetByIndex(i);
			Collider key = byIndex.Key;
			List<Collider> value = byIndex.Value;
			if (key == null)
			{
				ignoredColliders.RemoveAt(i--);
			}
			else if (value.Count == 0)
			{
				HashSet<Collider>.Enumerator enumerator = waterColliders.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Physics.IgnoreCollision(key, enumerator.Current, ignore: false);
				}
				ignoredColliders.RemoveAt(i--);
			}
		}
	}
}
