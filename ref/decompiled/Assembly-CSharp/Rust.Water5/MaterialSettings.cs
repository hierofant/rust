using System;
using UnityEngine;

namespace Rust.Water5;

[Serializable]
public struct MaterialSettings
{
	public Color color;

	public Color specColor;

	public float smoothness;

	public Color waterColor;

	public Color waterExtinction;

	public float scatteringCoefficient;

	public Color subSurfaceColor;

	public float subSurfaceFalloff;

	public float subSurfaceBase;

	public float subSurfaceSun;

	public float subSurfaceAmount;

	public float foamAmount;

	public float foamScale;

	public Color foamColor;

	public Color baseFoamColor;
}
