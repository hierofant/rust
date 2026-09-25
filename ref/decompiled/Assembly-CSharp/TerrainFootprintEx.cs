using UnityEngine;

public static class TerrainFootprintEx
{
	public static bool CheckTerrainFootprint(this Transform transform, TerrainFootprint footprint, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		if (!footprint || footprint.RejectAboveGap <= 0f)
		{
			return true;
		}
		return footprint.MeasureGap(pos, rot, scale) <= footprint.RejectAboveGap;
	}

	public static void FillTerrainFootprint(this Transform transform, TerrainFootprint footprint, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		if ((bool)footprint)
		{
			footprint.Fill(pos, rot, scale);
		}
	}
}
