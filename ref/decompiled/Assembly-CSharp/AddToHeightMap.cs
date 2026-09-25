using UnityEngine;

public class AddToHeightMap : ProceduralObject
{
	public bool DestroyGameObject;

	public void Apply(bool useTerrainSamplingSettings = false)
	{
		Collider component = GetComponent<Collider>();
		Bounds bounds = component.bounds;
		int num = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeX(bounds.min.x));
		int num2 = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeX(bounds.max.x));
		int num3 = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeZ(bounds.min.z));
		int num4 = TerrainMeta.HeightMap.Index(TerrainMeta.NormalizeZ(bounds.max.z));
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		for (int i = num3; i <= num4; i++)
		{
			float normZ = TerrainMeta.HeightMap.Coordinate(i);
			for (int j = num; j <= num2; j++)
			{
				float normX = TerrainMeta.HeightMap.Coordinate(j);
				bool flag;
				float height;
				if (useTerrainSamplingSettings)
				{
					flag = heightMap.TrySampleColliderHeight01(1 << component.gameObject.layer, position, size, normX, normZ, out height, component);
				}
				else
				{
					Vector3 origin = new Vector3(TerrainMeta.DenormalizeX(normX), bounds.max.y, TerrainMeta.DenormalizeZ(normZ));
					Ray ray = new Ray(origin, Vector3.down);
					flag = component.Raycast(ray, out var hitInfo, bounds.size.y);
					height = (flag ? TerrainMeta.NormalizeY(hitInfo.point.y) : 0f);
				}
				if (flag)
				{
					float height2 = TerrainMeta.HeightMap.GetHeight01(j, i);
					if (height > height2)
					{
						TerrainMeta.HeightMap.SetHeight(j, i, height);
					}
				}
			}
		}
	}

	public override void Process()
	{
		Apply();
		if (DestroyGameObject)
		{
			GameManager.Destroy(base.gameObject);
		}
		else
		{
			GameManager.Destroy(this);
		}
	}
}
