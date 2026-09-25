using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UtilityJobs;

public class ScopedOcean
{
	private NativeArray<int> originalTopologyCopy;

	public bool IsScoped { get; set; }

	public void SetFlatOcean()
	{
		IsScoped = true;
		SetupFlatOcean();
	}

	public void Restore()
	{
		if (IsScoped)
		{
			RevertOcean();
		}
	}

	private void SetupFlatOcean()
	{
		originalTopologyCopy = new NativeArray<int>(TerrainMeta.TopologyMap.src, Allocator.Persistent);
		TerrainMeta.TopologyMap.Push();
		FillJob<int> fillJob = default(FillJob<int>);
		fillJob.Values = TerrainMeta.TopologyMap.dst;
		fillJob.Value = 128;
		FillJob<int> jobData = fillJob;
		IJobExtensions.RunByRef(ref jobData);
		TerrainMeta.TopologyMap.Pop();
		TerrainMeta.Texturing.Setup();
		ref TerrainTexturing.ShoreData mapByRef = ref TerrainMeta.Texturing.GetMapByRef(isDeepSea: false);
		mapByRef.DefaultVector = new Vector4(1f, 1f, 1f, 1f);
		mapByRef.FillWithDefault();
	}

	private void RevertOcean()
	{
		TerrainMeta.TopologyMap.Push();
		TerrainMeta.TopologyMap.dst.CopyFrom(originalTopologyCopy);
		TerrainMeta.TopologyMap.Pop();
		TerrainMeta.Texturing.Setup();
		NativeArrayEx.SafeDispose(ref originalTopologyCopy);
	}
}
