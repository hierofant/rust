using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Paintball Colour Lookup")]
public class PaintballColorLookup : BaseScriptableObject
{
	[Serializable]
	public class ColorLookupData
	{
		public Translate.Phrase colorNamePhrase;

		public Translate.Phrase colorDescriptionPhrase;

		public Sprite overallsIconSprite;

		public Color pieMenuColor;

		public Color paintballViewmodelColor;

		public Color paintballDecalColor;

		public Color paintballImpactParticleColor;

		public Color overallsColor;
	}

	private static PaintballColorLookup _instance;

	[SerializeField]
	private ColorLookupData[] colors;

	[SerializeField]
	public ItemDefinition paintballGunItemDefinition;

	[SerializeField]
	public ItemDefinition overallsItemDefinition;

	public static PaintballColorLookup instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<PaintballColorLookup>("assets/prefabs/weapons/paintball_gun/paintballcolourlookup.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load PaintballColorLookup");
			}
			return _instance;
		}
	}

	public ColorLookupData GetColorData(int index)
	{
		return colors[Mathf.Clamp(index, 0, colors.Length - 1)];
	}

	public int GetColorsCount()
	{
		return colors.Length;
	}
}
