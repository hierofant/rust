using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public class TerrainCollision : TerrainExtension
{
	public TerrainIgnoreGrid TerrainIgnoreGrid;

	public ListDictionary<Collider, List<Collider>> ignoredColliders;

	public TerrainCollider terrainCollider;

	public const float IgnoreRadius = 0.01f;

	public override void Setup()
	{
		ignoredColliders = new ListDictionary<Collider, List<Collider>>();
		terrainCollider = terrainRenderer.gameObject.GetComponent<TerrainCollider>();
		TerrainIgnoreGrid = new TerrainIgnoreGrid();
	}

	private void OnDestroy()
	{
		TerrainIgnoreGrid.Dispose();
	}

	public void Clear()
	{
		if (!terrainCollider)
		{
			return;
		}
		foreach (Collider key in ignoredColliders.Keys)
		{
			Physics.IgnoreCollision(key, terrainCollider, ignore: false);
		}
		ignoredColliders.Clear();
	}

	public void Reset(Collider collider)
	{
		if ((bool)terrainCollider && (bool)collider)
		{
			Physics.IgnoreCollision(collider, terrainCollider, ignore: false);
			ignoredColliders.Remove(collider);
		}
	}

	public bool GetIgnore(Vector3 pos, float radius = 0.01f)
	{
		using (TimeWarning.New("TerrainCollision.GetIgnore"))
		{
			if (TerrainIgnoreGrid != null && !TerrainIgnoreGrid.Check(pos, radius))
			{
				return false;
			}
			return GamePhysics.CheckSphere<TerrainCollisionTrigger>(pos, radius, 262144, QueryTriggerInteraction.Collide);
		}
	}

	public void GetIgnore(NativeArray<Vector3>.ReadOnly positions, NativeArray<float>.ReadOnly radii, NativeArray<bool> results)
	{
		using (TimeWarning.New("TerrainCollision.GetIgnore"))
		{
			FillJob<bool> fillJob = default(FillJob<bool>);
			fillJob.Values = results;
			fillJob.Value = false;
			FillJob<bool> jobData = fillJob;
			IJobExtensions.RunByRef(ref jobData);
			NativeList<int> nativeList = new NativeList<int>(positions.Length, Allocator.TempJob);
			JobHandle jobHandle;
			if (TerrainIgnoreGrid != null)
			{
				jobHandle = TerrainIgnoreGrid.Check(positions, radii, nativeList);
			}
			else
			{
				GenerateAscSeqListJob jobData2 = default(GenerateAscSeqListJob);
				jobData2.Values = nativeList;
				jobData2.Start = 0;
				jobData2.Step = 1;
				jobData2.Count = positions.Length;
				jobHandle = jobData2.Schedule();
			}
			jobHandle.Complete();
			if (!nativeList.IsEmpty)
			{
				NativeArray<Vector3> results2 = new NativeArray<Vector3>(nativeList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<Vector3> gatherJob = default(GatherJob<Vector3>);
				gatherJob.Results = results2;
				gatherJob.Source = positions;
				gatherJob.Indices = nativeList.AsReadOnly();
				GatherJob<Vector3> jobData3 = gatherJob;
				IJobExtensions.RunByRef(ref jobData3);
				NativeArray<float> results3 = new NativeArray<float>(nativeList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				GatherJob<float> gatherJob2 = default(GatherJob<float>);
				gatherJob2.Results = results3;
				gatherJob2.Source = radii;
				gatherJob2.Indices = nativeList.AsReadOnly();
				GatherJob<float> jobData4 = gatherJob2;
				IJobExtensions.RunByRef(ref jobData4);
				NativeArray<int> values = new NativeArray<int>(nativeList.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				FillJob<int> fillJob2 = default(FillJob<int>);
				fillJob2.Values = values;
				fillJob2.Value = 262144;
				FillJob<int> jobData5 = fillJob2;
				IJobExtensions.RunByRef(ref jobData5);
				new QueryParameters(262144, hitMultipleFaces: false, QueryTriggerInteraction.Collide);
				GamePhysics.CheckSpheres<TerrainCollisionTrigger>(results2.AsReadOnly(), results3.AsReadOnly(), values.AsReadOnly(), results, GamePhysics.DefaultMaxResultsPerQuery, QueryTriggerInteraction.Collide, GamePhysics.MasksToValidate.None);
				Span<bool> values2 = results;
				NativeArray<int>.ReadOnly source = nativeList.AsReadOnly();
				CollectionUtil.ScatterOutInplace(values2, source, defValue: false);
				values.Dispose();
				results3.Dispose();
				results2.Dispose();
			}
			nativeList.Dispose();
		}
	}

	public bool GetIgnore(RaycastHit hit)
	{
		using (TimeWarning.New("TerrainCollision.GetIgnore"))
		{
			if (!(hit.collider is TerrainCollider))
			{
				return false;
			}
			if (!TerrainIgnoreGrid.Check(hit.point))
			{
				return false;
			}
			return hit.collider is TerrainCollider && GetIgnore(hit.point);
		}
	}

	public bool GetIgnore(Collider collider)
	{
		if (!terrainCollider || !collider)
		{
			return false;
		}
		return ignoredColliders.Contains(collider);
	}

	public void SetIgnore(Collider collider, Collider trigger, bool ignore = true)
	{
		if (!terrainCollider || !collider)
		{
			return;
		}
		if (!GetIgnore(collider))
		{
			if (ignore)
			{
				List<Collider> val = new List<Collider> { trigger };
				Physics.IgnoreCollision(collider, terrainCollider, ignore: true);
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
				Physics.IgnoreCollision(key, terrainCollider, ignore: false);
				ignoredColliders.RemoveAt(i--);
			}
		}
	}
}
