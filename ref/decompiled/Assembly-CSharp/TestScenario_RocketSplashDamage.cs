using System;
using UnityEngine;

public class TestScenario_RocketSplashDamage : TestScenarioPrefab
{
	[Serializable]
	public class TestCase
	{
		public CopyPasteDataAsset copyPasteAsset;

		public ServerProjectileTestSpawner rocketSpawner;

		public int[] notDamagedEntities;

		public int[] damagedEntities;
	}

	public TestCase[] testCases;

	public Material notDamagedMaterial;

	public Material damagedMaterial;

	public Material defaultMaterial;

	[HideInInspector]
	[SerializeField]
	public int currentTestCaseIndex;

	[Space]
	public bool showImpact;
}
