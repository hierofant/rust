using UnityEngine;

public class SpaceCheckingSpawnPoint : GenericSpawnPoint
{
	public bool useCustomBoundsCheckMask;

	public LayerMask customBoundsCheckMask;

	public float customBoundsCheckScale = 1f;

	public override bool IsAvailableTo(GameObject prefab)
	{
		if (!base.IsAvailableTo(prefab))
		{
			return false;
		}
		if (customBoundsCheckScale == 0f)
		{
			return true;
		}
		if (useCustomBoundsCheckMask)
		{
			return SpawnHandler.CheckBounds(prefab, base.transform.position, base.transform.rotation, Vector3.one * customBoundsCheckScale, customBoundsCheckMask);
		}
		if (SingletonComponent<SpawnHandler>.Instance != null)
		{
			return SingletonComponent<SpawnHandler>.Instance.CheckBounds(prefab, base.transform.position, base.transform.rotation, Vector3.one * customBoundsCheckScale);
		}
		return false;
	}
}
