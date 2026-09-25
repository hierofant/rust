using UnityEngine;

public class SocketMod_TerrainBoundsCheck : SocketMod
{
	private static Translate.Phrase lastError = new Translate.Phrase();

	protected override Translate.Phrase ErrorPhrase => lastError;

	public static bool IsInTerrainBounds(Vector3 vPoint)
	{
		return !TerrainMeta.OutOfBounds(vPoint);
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 vPoint = place.position + place.rotation * worldPosition;
		lastError = null;
		if (IsInTerrainBounds(vPoint))
		{
			return true;
		}
		if (lastError == null)
		{
			lastError = ConstructionErrors.NotInTerrain;
		}
		return false;
	}
}
