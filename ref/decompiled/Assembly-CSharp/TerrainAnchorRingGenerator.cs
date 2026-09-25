using System.Collections.Generic;
using UnityEngine;

public class TerrainAnchorRingGenerator : MonoBehaviour, IEditorComponent
{
	public List<GameObject> Rocks = new List<GameObject>();

	public float PlaneHeight;

	[Tooltip("Raises (or lowers, if negative) every generated anchor by this much relative to the slice plane, without moving the slice itself - the silhouette is still taken at PlaneHeight, so the gizmo shows the anchor ring floating off the outline by exactly this amount. Lifting the anchors sinks the rock into the terrain by the same distance, since the solve seats the root so the anchors meet the ground.\n\nNot the same as AnchorOffset: that is a slope-aware tolerance, scaled by slopeScale and always vertical. This is baked into the anchor's local position, so it rotates with the rock and shifts the sample points sideways on a tilted one.")]
	public float AnchorLift;

	public float StandOff = 3f;

	public float Spacing = 4f;

	[Range(0.25f, 15f)]
	public float SampleAngleStep = 1.5f;

	[Header("TerrainAnchor Settings")]
	public float AnchorExtents = 0.4f;

	public float AnchorOffset = 0.4f;

	public float AnchorRadius = 0.25f;

	public float AnchorSlopeScale = 1f;

	[HideInInspector]
	public List<TerrainAnchor> GeneratedAnchors = new List<TerrainAnchor>();

	[HideInInspector]
	public TerrainFootprint GeneratedFootprint;
}
