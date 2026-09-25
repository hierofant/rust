using System;
using UnityEngine;

public class PanelLight : SimpleLight
{
	[Serializable]
	public struct ColorSetting
	{
		public Translate.Phrase name;

		public Translate.Phrase desc;

		public Color color;

		public Material mat;
	}

	public ColorSetting[] colorSettings;

	public MeshRenderer lightOnMesh;
}
