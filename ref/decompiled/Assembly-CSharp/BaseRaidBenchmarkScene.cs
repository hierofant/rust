using System;
using UnityEngine;

public class BaseRaidBenchmarkScene : BenchmarkScene
{
	[Serializable]
	public struct RunConfig
	{
		public string Name;

		public int PlayerCount;

		public int BaseCount;

		public TextAsset[] BaseJsons;

		public bool SpawnBaseInMiddle;

		public TextAsset MiddleBase;
	}

	[Header("Base Raid Benchmark")]
	public GameObject WorldSetupPrefab;

	public TerrainConfig TerrainConfig;

	public GameObjectRef PlayerPrefab;

	public GameObjectRef ClientPrefab;

	public GameObjectRef SoundManagerPrefab;

	public RunConfig[] Runs;
}
