using System;
using UnityEngine;

[Serializable]
public class VolumeCloudsCurlNoiseConfig
{
	public Vector2 Frequency = Vector2.one;

	public float Strength;

	public int Octaves = 1;

	public void CopyFrom(VolumeCloudsCurlNoiseConfig copy)
	{
		Frequency = copy.Frequency;
		Strength = copy.Strength;
		Octaves = copy.Octaves;
	}
}
