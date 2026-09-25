using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShadowQualityPresetsConfig", menuName = "Scriptable Object/ShadowQualityPresetsConfig")]
public class ShadowQualityPresetsConfig : ScriptableObject
{
	public enum Cascades
	{
		TwoCascades = 2,
		FourCascades = 4
	}

	[Serializable]
	public struct Preset
	{
		public string name;

		public Cascades numCascades;

		[Range(0f, 3f)]
		public int filteringQuality;

		[Range(0f, 1000f)]
		public float shadowDistance;

		public ShadowResolution shadowResolution;

		public bool grassShadowsEnabled;
	}

	[SerializeField]
	private List<Preset> presets;

	public List<Preset> Presets => presets;
}
