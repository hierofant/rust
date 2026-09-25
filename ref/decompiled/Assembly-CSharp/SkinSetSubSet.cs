using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skin Set Sub Set")]
public class SkinSetSubSet : ScriptableObject
{
	public SkinSet[] SubSkins;

	private int GetIndex(float meshNumber)
	{
		return Mathf.Clamp(Mathf.FloorToInt(meshNumber * (float)SubSkins.Length), 0, SubSkins.Length - 1);
	}

	public SkinSet GetSkinSet(float meshNumber)
	{
		int index = GetIndex(meshNumber);
		return SubSkins[index];
	}
}
