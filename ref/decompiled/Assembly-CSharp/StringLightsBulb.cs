using System;
using UnityEngine;

public class StringLightsBulb : MonoBehaviour
{
	[Serializable]
	public struct EmissionData
	{
		public int materialIndex;

		[ColorUsage(true, true, 0f, 100f, 0f, 100f)]
		public Color onColor;

		[ColorUsage(true, true, 0f, 100f, 0f, 100f)]
		public Color offColor;
	}

	public Renderer bulbRenderer;

	public EmissionData[] emissionColors;
}
