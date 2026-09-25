using UnityEngine;

public class ScentEmission : EntityComponent<BaseCombatEntity>, IClientComponent
{
	[SerializeField]
	private bool startSampling = true;

	[SerializeField]
	private bool generateFakeTrail = true;

	[SerializeField]
	private float sampleIntervalSeconds = 1f;

	[SerializeField]
	[Space]
	private int minInitialPoints = 5;

	[SerializeField]
	private Vector2 initialDistanceRange = new Vector2(50f, 100f);

	[SerializeField]
	private float startAngleDeviation = 45f;

	[SerializeField]
	private float wiggleIntensity = 1f;

	[Space]
	[SerializeField]
	private LineRenderer[] lineRenderers;

	[SerializeField]
	private float heightOffset = 0.5f;

	[SerializeField]
	private bool snapToTerrain = true;
}
