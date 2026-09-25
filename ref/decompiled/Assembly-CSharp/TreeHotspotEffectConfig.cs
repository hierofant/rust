using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Tree Hotspot Effect Config")]
public class TreeHotspotEffectConfig : BaseScriptableObject
{
	private static TreeHotspotEffectConfig _instance;

	public GameObjectRef hitEffect;

	public SoundDefinition hitEffectSound;

	public static TreeHotspotEffectConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<TreeHotspotEffectConfig>("assets/content/nature/treesprefabs/trees/minigame/treehotspoteffectconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load TreeHotspotEffectConfig");
			}
			return _instance;
		}
	}
}
