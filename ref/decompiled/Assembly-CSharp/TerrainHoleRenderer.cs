using UnityEngine;

[ExecuteAlways]
public class TerrainHoleRenderer : SingletonComponent<TerrainHoleRenderer>, IClientComponent
{
	[SerializeField]
	private Material matMask;

	[SerializeField]
	private Material matMap;
}
