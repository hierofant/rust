using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Skin Set Collection")]
public class SkinSetCollection : ScriptableObject
{
	[SerializeField]
	private SkinSetSubSet[] Skins;

	public int GetIndex(float meshNumber)
	{
		return Mathf.Clamp(Mathf.FloorToInt(meshNumber * (float)Skins.Length), 0, Skins.Length - 1);
	}

	public SkinSet Get(float meshNumber, float subNumber)
	{
		return Skins[GetIndex(meshNumber)].GetSkinSet(subNumber);
	}
}
