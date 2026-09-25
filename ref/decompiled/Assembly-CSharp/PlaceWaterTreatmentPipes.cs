using System.Collections.Generic;
using UnityEngine;

public class PlaceWaterTreatmentPipes : PlaceDecorRoadside
{
	private struct BlockedVolume
	{
		public OBB Obb;

		public float Radius;
	}

	[Tooltip("Clearance from a prevent building volume. Only the pivot is tested.")]
	public float PreventBuildingPadding = 2f;

	private bool doesWaterTreatmentExist;

	private List<BlockedVolume> preventBuildingVolumes = new List<BlockedVolume>();

	protected override bool ShouldPlace()
	{
		CheckMonuments();
		return doesWaterTreatmentExist;
	}

	protected override bool IsValidLocation(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		foreach (BlockedVolume preventBuildingVolume in preventBuildingVolumes)
		{
			float num = PreventBuildingPadding + preventBuildingVolume.Radius;
			if (!((pos - preventBuildingVolume.Obb.position).sqrMagnitude > num * num))
			{
				OBB obb = preventBuildingVolume.Obb;
				if (obb.Distance(pos) < PreventBuildingPadding)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void CheckMonuments()
	{
		doesWaterTreatmentExist = false;
		preventBuildingVolumes.Clear();
		if (TerrainMeta.Path == null || TerrainMeta.Path.Monuments == null)
		{
			Debug.LogError("[PlaceWaterTreatmentPipes] PROCESSING: TerrainMeta.Path.Monuments is null, cannot check for water-treatment monument, skipping placement.");
			return;
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument.IsWaterTreatmentPlant())
			{
				doesWaterTreatmentExist = true;
			}
		}
		foreach (PreventBuildingMonumentTag item in PreventBuildingMonumentTag.All)
		{
			if (item.TryGetVolume(out var result))
			{
				preventBuildingVolumes.Add(new BlockedVolume
				{
					Obb = result,
					Radius = result.extents.magnitude
				});
			}
		}
	}
}
