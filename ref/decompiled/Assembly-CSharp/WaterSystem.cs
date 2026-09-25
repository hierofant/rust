using System;
using System.Collections.Generic;
using ConVar;
using Rust;
using Rust.Water5;
using TerrainWaterMapJobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using WaterSystemJobs;

[ExecuteInEditMode]
public class WaterSystem : MonoBehaviour
{
	[Serializable]
	public class RenderingSettings
	{
		[Serializable]
		public class SkyProbe
		{
			public float ProbeUpdateInterval = 1f;

			public bool TimeSlicing = true;
		}

		[Serializable]
		public class SSR
		{
			public float FresnelCutoff = 0.02f;

			public float ThicknessMin = 1f;

			public float ThicknessMax = 20f;

			public float ThicknessStartDist = 40f;

			public float ThicknessEndDist = 100f;
		}

		public Vector4[] TessellationQuality;

		public SkyProbe SkyReflections;

		public SSR ScreenSpaceReflections;
	}

	private struct OceanSimulationWrapper : IDisposable
	{
		internal OceanSimulation mainlandSimulation;

		internal OceanSimulation deepSeaSimulation;

		internal Rust.Water5.NativeOceanDisplacementShort3 mainlandData;

		internal Rust.Water5.NativeOceanDisplacementShort3 deepSeaData;

		private bool sharedSimData;

		internal static OceanSimulationWrapper Init(OceanSettings mainland, OceanSettings deepsea)
		{
			OceanSimulationWrapper result = default(OceanSimulationWrapper);
			result.sharedSimData = mainland == deepsea;
			result.mainlandData = mainland.LoadNativeSimData();
			result.deepSeaData = (result.sharedSimData ? result.mainlandData : deepsea.LoadNativeSimData());
			result.mainlandSimulation = new OceanSimulation(mainland, result.mainlandData);
			result.deepSeaSimulation = new OceanSimulation(mainland, result.deepSeaData);
			return result;
		}

		public OceanSimulation GetOceanSimulation(Vector3 worldPosition)
		{
			if (!DeepSeaManager.IsInsideDeepSea(worldPosition))
			{
				return mainlandSimulation;
			}
			return deepSeaSimulation;
		}

		public OceanSimulation GetOceanSimulation(bool isDeepSea)
		{
			if (!isDeepSea)
			{
				return mainlandSimulation;
			}
			return deepSeaSimulation;
		}

		public void Dispose()
		{
			mainlandSimulation?.Dispose();
			deepSeaSimulation?.Dispose();
			mainlandData.Dispose();
			if (!sharedSimData)
			{
				deepSeaData.Dispose();
			}
		}
	}

	private static float oceanLevel = 0f;

	[Header("Ocean Settings")]
	public OceanSettings oceanSettings;

	private OceanSimulationWrapper oceanSimulationWrapper;

	public WaterQuality Quality = WaterQuality.High;

	public Material oceanMaterial;

	public RenderingSettings Rendering = new RenderingSettings();

	public ComputeShader oceanVFaceShader;

	public OceanVariantMaterial TropicalMaterial;

	public int patchSize = 100;

	public int patchCount = 4;

	public float patchScale = 1f;

	public bool forceDeepSea;

	public static WaterSystem Instance { get; private set; }

	public static WaterCollision Collision { get; private set; }

	public static WaterBody Ocean { get; private set; }

	public static Material OceanMaterial => Instance?.oceanMaterial;

	public static ListHashSet<WaterCamera> WaterCameras { get; } = new ListHashSet<WaterCamera>();


	public static HashSet<WaterBody> WaterBodies { get; } = new HashSet<WaterBody>();


	public static HashSet<WaterDepthMask> DepthMasks { get; } = new HashSet<WaterDepthMask>();


	public static float WaveTime { get; private set; }

	public static ComputeShader OceanVFaceShader => Instance?.oceanVFaceShader;

	public static bool SuspendProcessing => false;

	public static float OceanLevel
	{
		get
		{
			return oceanLevel;
		}
		set
		{
			if (!Mathf.Approximately(oceanLevel, value))
			{
				oceanLevel = value;
				UpdateOceanLevel();
			}
		}
	}

	public bool IsInitialized { get; private set; }

	public int Layer => base.gameObject.layer;

	public int Reflections => Water.reflections;

	public float WindDirection => oceanSettings.windDirection;

	public float[] OctaveScales => oceanSettings.octaveScales;

	private void EditorInitialize()
	{
	}

	private void EditorShutdown()
	{
	}

	public OceanSimulation GetOceanSimulation(Vector3 worldPosition)
	{
		return oceanSimulationWrapper.GetOceanSimulation(worldPosition);
	}

	public OceanSimulation GetOceanSimulation(bool isDeep)
	{
		return oceanSimulationWrapper.GetOceanSimulation(isDeep);
	}

	private void CheckInstance()
	{
		Instance = ((Instance != null) ? Instance : this);
		Collision = ((Collision != null) ? Collision : GetComponent<WaterCollision>());
	}

	private void Awake()
	{
		CheckInstance();
	}

	private void OnEnable()
	{
		CheckInstance();
		oceanSimulationWrapper = OceanSimulationWrapper.Init(oceanSettings, oceanSettings);
		IsInitialized = true;
	}

	private void OnDisable()
	{
		if (!UnityEngine.Application.isPlaying || !Rust.Application.isQuitting)
		{
			oceanSimulationWrapper.Dispose();
			oceanSimulationWrapper = default(OceanSimulationWrapper);
			IsInitialized = false;
			Instance = null;
		}
	}

	private void Update()
	{
		if (SuspendProcessing)
		{
			return;
		}
		using (TimeWarning.New("UpdateWaves"))
		{
			UpdateOceanSimulation();
		}
	}

	public static bool Trace(Ray ray, out Vector3 position, float maxDist = 100f)
	{
		if (Instance == null)
		{
			position = Vector3.zero;
			return false;
		}
		if (Instance.GetOceanSimulation(ray.origin).Trace(ray, maxDist, out position) && TerrainMeta.TopologyMap.GetTopology(position, 384))
		{
			return true;
		}
		return false;
	}

	public static bool Trace(Ray ray, out Vector3 position, out Vector3 normal, float maxDist = 100f)
	{
		if (Instance == null)
		{
			position = Vector3.zero;
			normal = Vector3.zero;
			return false;
		}
		normal = Vector3.up;
		if (Instance.GetOceanSimulation(ray.origin).Trace(ray, maxDist, out position) && TerrainMeta.TopologyMap.GetTopology(position, 384))
		{
			return true;
		}
		return false;
	}

	public static JobHandle ScheduleTraceBatchDefer(NativeList<Ray> rays, NativeArray<float> maxDists, NativeArray<bool> hitResults, NativeArray<Vector3> hitPositions, NativeArray<Vector3> hitNormals, NativeList<int> deepIndices, NativeList<int> mainIndices, JobHandle inputDeps)
	{
		if (Instance == null)
		{
			WaterSystemJobs.FillFalseJobDefer jobData = default(WaterSystemJobs.FillFalseJobDefer);
			jobData.rays = rays;
			jobData.HitResults = hitResults;
			inputDeps = jobData.Schedule(rays, 256, inputDeps);
			return inputDeps;
		}
		NativeArray<Ray> rays2 = rays.AsDeferredJobArray();
		inputDeps = Instance.oceanSimulationWrapper.mainlandSimulation.TraceBatch(mainIndices, rays2, maxDists, hitResults, hitPositions, inputDeps);
		inputDeps = Instance.oceanSimulationWrapper.deepSeaSimulation.TraceBatch(deepIndices, rays2, maxDists, hitResults, hitPositions, inputDeps);
		WaterSystemJobs.AdjustByTopologyJob jobData2 = default(WaterSystemJobs.AdjustByTopologyJob);
		jobData2.rays = rays;
		jobData2.hitResults = hitResults;
		jobData2.hitNormals = hitNormals;
		jobData2.hitPositions = hitPositions.AsReadOnly();
		jobData2.TopologyData = TerrainMeta.TopologyMap.src;
		jobData2.TopologyRes = TerrainMeta.TopologyMap.res;
		jobData2.DataOrigin = new Vector2(TerrainMeta.Position.x, TerrainMeta.Position.z);
		jobData2.DataScale = new Vector2(TerrainMeta.OneOverSize.x, TerrainMeta.OneOverSize.z);
		inputDeps = jobData2.Schedule(rays, 256, inputDeps);
		return inputDeps;
	}

	private static void GetHeightArray_Native(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, ref NativeArray<float> waterHeight, in bool isDeepSea)
	{
		TerrainTexturing.ShoreData shoreData = TerrainTexturing.Instance.GetMap(isDeepSea);
		TerrainHeightMap.HeightMapQueryStructure terrainHeightMapData = TerrainMeta.HeightMap.GetQueryStructure(isDeepSea);
		bool hasHeightMap = TerrainTexturing.Instance != null;
		bool hasWaterAndTopology = Instance != null && (bool)TerrainMeta.WaterMap && (bool)TerrainMeta.TopologyMap;
		bool hasTerrainTexturing = TerrainTexturing.Instance != null;
		GetHeightByUVJob jobData = TerrainMeta.WaterMap.FetchUVsHeightsJob(posUV, waterHeight);
		TerrainTopologyMap.TopologyQueryStructure topologyQueryStructure = TerrainMeta.TopologyMap.GetQueryStructure();
		bool hasWaterSystem = Instance != null;
		OceanSimulation oceanSimulation = (hasWaterSystem ? Instance.GetOceanSimulation(isDeepSea) : null);
		float OceanLevel = WaterSystem.OceanLevel;
		float MaxOceanLevel = (hasWaterSystem ? oceanSimulation.MaxLevel() : OceanLevel);
		TerrainWaterMap.WaterMapQueryStructure waterMapQueryStruct = TerrainMeta.WaterMap.GetQueryStructure();
		NativeArray<Vector2>.ReadOnly posUV2 = posUV.AsReadOnly();
		WaterSystemBurst.GetHeightArray_Burst(in posUV2, ref shore, ref terrainHeight, in shoreData, in terrainHeightMapData, in hasHeightMap, in hasTerrainTexturing);
		IJobExtensions.RunByRef(ref jobData);
		if (hasWaterSystem)
		{
			oceanSimulation.GetHeightBatch(pos, waterHeight, shore, terrainHeight);
		}
		WaterSystemBurst.ComputeOceanSimHeight_Burst(in pos, in posUV, ref shore, ref waterHeight, in isDeepSea, in hasWaterAndTopology, in topologyQueryStructure, in OceanLevel, in MaxOceanLevel, in hasWaterSystem, in waterMapQueryStruct);
	}

	public static void GetHeightArray_Managed(Vector2[] pos, Vector2[] posUV, float[] shore, float[] terrainHeight, float[] waterHeight, bool isDeepSea)
	{
		if (TerrainTexturing.Instance != null)
		{
			TerrainTexturing.ShoreData map = TerrainTexturing.Instance.GetMap(isDeepSea);
			for (int i = 0; i < posUV.Length; i++)
			{
				shore[i] = map.GetCoarseDistanceToShore(posUV[i]);
			}
		}
		else
		{
			Array.Fill(shore, 0f, 0, posUV.Length);
		}
		if (TerrainMeta.HeightMap != null)
		{
			if (isDeepSea)
			{
				Array.Fill(terrainHeight, DeepSeaManager.SeaFloorDepth);
			}
			else
			{
				for (int j = 0; j < posUV.Length; j++)
				{
					terrainHeight[j] = TerrainMeta.HeightMap.GetHeight(posUV[j]);
				}
			}
		}
		else
		{
			Array.Fill(terrainHeight, 0f, 0, posUV.Length);
		}
		if (Instance != null && (bool)TerrainMeta.WaterMap && (bool)TerrainMeta.TopologyMap)
		{
			bool flag = false;
			OceanSimulation oceanSimulation = Instance.GetOceanSimulation(isDeepSea);
			for (int k = 0; k < posUV.Length; k++)
			{
				Vector2 uv = posUV[k];
				float num = TerrainMeta.WaterMap.GetHeightFast(uv, isDeepSea);
				if (num < OceanLevel + oceanSimulation.MaxLevel() && (isDeepSea || TerrainMeta.TopologyMap.GetTopology(uv.x, uv.y, 384)))
				{
					if (!flag)
					{
						oceanSimulation.GetHeightBatch(pos, waterHeight, shore, terrainHeight);
						flag = true;
					}
					float b = waterHeight[k] + OceanLevel;
					num = Mathf.Max(num, b);
				}
				waterHeight[k] = num;
			}
		}
		else if (Instance != null)
		{
			Instance.GetOceanSimulation(isDeepSea).GetHeightBatch(pos, waterHeight, shore, terrainHeight);
			for (int l = 0; l < pos.Length; l++)
			{
				waterHeight[l] += OceanLevel;
			}
		}
		else
		{
			Array.Fill(waterHeight, OceanLevel, 0, pos.Length);
			Array.Fill(shore, 0f, 0, posUV.Length);
		}
	}

	public static void GetHeightArray(Vector2[] pos, Vector2[] posUV, float[] shore, float[] terrainHeight, float[] waterHeight, bool isDeepSea)
	{
		GetHeightArray_Managed(pos, posUV, shore, terrainHeight, waterHeight, isDeepSea);
	}

	public static void GetHeightArray(in NativeArray<Vector2> pos, in NativeArray<Vector2> posUV, ref NativeArray<float> shore, ref NativeArray<float> terrainHeight, ref NativeArray<float> waterHeight, bool isDeepSea)
	{
		GetHeightArray_Native(in pos, in posUV, ref shore, ref terrainHeight, ref waterHeight, in isDeepSea);
	}

	public static void RegisterBody(WaterBody body)
	{
		if (body.Type == WaterBodyType.Ocean)
		{
			if (Ocean == null)
			{
				Ocean = body;
				body.Transform.position = body.Transform.position.WithY(OceanLevel);
			}
			else if (Ocean != body)
			{
				Debug.LogWarning("[Water] Ocean body is already registered. Ignoring call because only one is allowed.");
				return;
			}
		}
		WaterBodies.Add(body);
	}

	public static void UnregisterBody(WaterBody body)
	{
		if (body == Ocean)
		{
			Ocean = null;
		}
		WaterBodies.Remove(body);
	}

	private static void UpdateOceanLevel()
	{
		if (Ocean != null)
		{
			Ocean.Transform.position = Ocean.Transform.position.WithY(OceanLevel);
		}
		foreach (WaterBody waterBody in WaterBodies)
		{
			waterBody.OnOceanLevelChanged(OceanLevel);
		}
	}

	private void UpdateOceanSimulation()
	{
		if (Water.scaled_time)
		{
			WaveTime += UnityEngine.Time.deltaTime;
		}
		else
		{
			WaveTime = UnityEngine.Time.realtimeSinceStartup;
		}
		if (Weather.ocean_time >= 0f)
		{
			WaveTime = Weather.ocean_time;
		}
		float beaufort = (SingletonComponent<Climate>.Instance ? SingletonComponent<Climate>.Instance.WeatherState.OceanScale : 4f);
		oceanSimulationWrapper.mainlandSimulation?.Update(WaveTime, UnityEngine.Time.deltaTime, beaufort);
		float beaufort2 = (SingletonComponent<Climate>.Instance ? SingletonComponent<Climate>.Instance.DeepSeaWeatherState.OceanScale : 4f);
		oceanSimulationWrapper.deepSeaSimulation?.Update(WaveTime, UnityEngine.Time.deltaTime, beaufort2);
	}

	public void Refresh()
	{
		oceanSimulationWrapper.Dispose();
		oceanSimulationWrapper = OceanSimulationWrapper.Init(oceanSettings, oceanSettings);
	}
}
