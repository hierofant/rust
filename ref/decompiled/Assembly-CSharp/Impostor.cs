using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Impostor : MonoBehaviour, IClientComponent, IPrefabPreProcess
{
	public ImpostorAsset asset;

	[Header("Baking")]
	public GameObject reference;

	public float angle;

	public int resolution = 1024;

	public int padding = 32;

	public bool spriteOutlineAsMesh;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	private void OnEnable()
	{
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
