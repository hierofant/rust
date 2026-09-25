using UnityEngine;

public class WearableReplacementByRace : MonoBehaviour, IClientComponent
{
	[ReadOnly]
	public GameObjectRef[] replacements;

	[SerializeField]
	private SkinnedMeshRenderer SkinnedRenderer;

	[SerializeField]
	private Mesh[] ReplacementMesh;

	[SerializeField]
	private SkinSet[] ReplaceForSkin;
}
