using UnityEngine;

[ExecuteAlways]
public class DeepSeaEditorScene : SingletonComponent<DeepSeaEditorScene>, IEditorComponent
{
	[Tooltip("When emulating deepsea with source terrain, need to use main textures since deepsea shore textures/data don't exist yet")]
	[SerializeField]
	private bool terrainIgnoreDeepseaFlag;

	private SpawnGroup[] spawnGroups;

	public static bool TerrainIgnoreDeepseaFlag
	{
		get
		{
			if (SingletonComponent<DeepSeaEditorScene>.Instance != null)
			{
				return SingletonComponent<DeepSeaEditorScene>.Instance.terrainIgnoreDeepseaFlag;
			}
			return false;
		}
	}
}
