using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Draw Colliders Config")]
public class DrawCollidersConfig : BaseScriptableObject
{
	private static DrawCollidersConfig _instance;

	public Material defaultMaterial;

	public Material worldMaterial;

	public Material treeMaterial;

	public Material bushHarvestableMaterial;

	public Material deployedMaterial;

	public Material vehicleMaterial;

	public Material preventBuildingMaterial;

	public Material triggerMaterial;

	public Material constructionMaterial;

	public Material aiMaterial;

	public static DrawCollidersConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<DrawCollidersConfig>("assets/content/developer/drawcollidersconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load DrawCollidersConfig");
			}
			return _instance;
		}
	}

	public Material GetMaterial(Collider col)
	{
		if (col.isTrigger)
		{
			return triggerMaterial;
		}
		return col.gameObject.layer switch
		{
			16 => worldMaterial, 
			8 => deployedMaterial, 
			19 => bushHarvestableMaterial, 
			26 => bushHarvestableMaterial, 
			30 => treeMaterial, 
			27 => vehicleMaterial, 
			13 => vehicleMaterial, 
			15 => vehicleMaterial, 
			21 => constructionMaterial, 
			18 => triggerMaterial, 
			29 => preventBuildingMaterial, 
			11 => aiMaterial, 
			_ => defaultMaterial, 
		};
	}
}
