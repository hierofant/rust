using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Hole Shape")]
public class HoleShapeAsset : ScriptableObject
{
	public int ID;

	public int startInstanceCount = 256;

	public Mesh meshAsset;
}
