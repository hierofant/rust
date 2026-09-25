using System;
using UnityEngine;

[ExecuteAlways]
public class TerrainHoleShape : MonoBehaviour, IClientComponent, ILOD
{
	[Tooltip("Probably don't use this, only added this to be compatible with our old holes. You can use Unity transforms")]
	public float radius = 1f;

	public HoleShapeAsset asset;

	[NonSerialized]
	public int idx = -1;
}
