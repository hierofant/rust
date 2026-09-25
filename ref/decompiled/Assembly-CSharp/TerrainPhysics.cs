using UnityEngine;

public class TerrainPhysics : TerrainExtension
{
	private TerrainSplatMap splat;

	private PhysicsMaterial[] materials;

	public override void Setup()
	{
		splat = terrainRenderer.gameObject.GetComponent<TerrainSplatMap>();
		materials = config.GetPhysicMaterials();
	}

	public PhysicsMaterial GetMaterial(Vector3 worldPos)
	{
		if (splat == null || materials.Length == 0)
		{
			return null;
		}
		return materials[splat.GetSplatMaxIndex(worldPos)];
	}
}
