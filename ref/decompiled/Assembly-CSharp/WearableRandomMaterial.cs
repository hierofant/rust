using System;
using UnityEngine;

public class WearableRandomMaterial : MonoBehaviour
{
	[Serializable]
	public struct MaterialOption
	{
		public int Chance;

		public Material ToApply;
	}

	[Serializable]
	public struct TargetRenderer
	{
		public Renderer Renderer;

		public int MaterialIndex;
	}

	public TargetRenderer[] TargetRenderers;

	public MaterialOption[] MaterialOptions;

	public bool RunRandomisation;
}
