using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Facepunch;
using Oxide.Core;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class TerrainMeta : SingletonComponent<TerrainMeta>
{
	public enum PaintMode
	{
		None,
		Splats,
		Biomes,
		Alpha,
		Blend,
		Field,
		Cliff,
		Summit,
		Beachside,
		Beach,
		Forest,
		Forestside,
		Ocean,
		Oceanside,
		Decor,
		Monument,
		Road,
		Roadside,
		Swamp,
		River,
		Riverside,
		Lake,
		Lakeside,
		Offshore,
		Rail,
		Railside,
		Building,
		Cliffside,
		Mountain,
		Clutter,
		Alt,
		Tier0,
		Tier1,
		Tier2,
		Mainland,
		Hilltop
	}

	public struct BurstData
	{
		public Vector3 Position;

		public Vector3 Size;

		public Vector3 OneOverSize;
	}

	public TerrainRenderer terrainRenderer = new TerrainRenderer();

	public TerrainData terrainData;

	public TerrainConfig config;

	public PaintMode paint;

	[HideInInspector]
	public PaintMode currentPaintMode;

	public static readonly SharedStatic<BurstData> sharedBurstData = SharedStatic<BurstData>.GetOrCreateUnsafe(0u, 5411825963348367585L, -2546176521858529784L);

	public static TerrainConfig Config { get; private set; }

	public static TerrainRenderer TerrainRenderer { get; private set; }

	public static Transform Transform { get; private set; }

	public static TerrainData TerrainData { get; private set; }

	public static Vector3 Position { get; private set; }

	public static Vector3 Size { get; private set; }

	public static Vector3 Center => Position + Size * 0.5f;

	public static Vector3 Max => Position + Size;

	public static Vector3 OneOverSize { get; private set; }

	public static Vector3 HighestPoint { get; set; }

	public static Vector3 LowestPoint { get; set; }

	public static float LootAxisAngle { get; private set; }

	public static float BiomeAxisAngle { get; private set; }

	public static TerrainData Data { get; private set; }

	public static TerrainCollider Collider { get; private set; }

	public static TerrainCollision Collision { get; private set; }

	public static TerrainPhysics Physics { get; private set; }

	public static TerrainColors Colors { get; private set; }

	public static TerrainQuality Quality { get; private set; }

	public static TerrainPath Path { get; private set; }

	public static TerrainBiomeMap BiomeMap { get; private set; }

	public static TerrainAlphaMap AlphaMap { get; private set; }

	public static TerrainBlendMap BlendMap { get; private set; }

	public static TerrainHeightMap HeightMap { get; private set; }

	public static TerrainSplatMap SplatMap { get; private set; }

	public static TerrainTopologyMap TopologyMap { get; private set; }

	public static TerrainWaterMap WaterMap { get; private set; }

	public static TerrainWaterFlowMap WaterFlowMap { get; private set; }

	public static TerrainDistanceMap DistanceMap { get; private set; }

	public static TerrainPlacementMap PlacementMap { get; private set; }

	public static TerrainTexturing Texturing { get; private set; }

	public static TerrainHoleRenderer HoleRenderer { get; private set; }

	public static Vector3 MarginSize => Size * 3f;

	public static void SetReflectionProbeUsage(ReflectionProbeUsage value)
	{
		TerrainRenderer.SetReflectionProbeUsage(value);
	}

	public static float SampleTerrainMeshHeight(Vector3 worldPos)
	{
		float x = NormalizeX(worldPos.x);
		float y = NormalizeZ(worldPos.z);
		float interpolatedHeight = TerrainData.GetInterpolatedHeight(x, y);
		return Position.y + interpolatedHeight;
	}

	public static void SampleTerrainMeshHeights(NativeArray<Vector3>.ReadOnly posi, NativeArray<float> heights)
	{
		using (TimeWarning.New("SampleTerrainMeshHeights"))
		{
			int batchSize = ThreadUtils.GetBatchSize(posi.Length, 4, 256);
			int num = (posi.Length + batchSize - 1) / batchSize;
			if (num >= 4)
			{
				List<UniTask> list = new List<UniTask>();
				for (int i = 0; i < num; i++)
				{
					int num2 = i * batchSize;
					int end2 = Math.Min(num2 + batchSize, posi.Length);
					list.Add(SampleAsync(heights, posi, num2, end2));
				}
				ThreadUtils.WaitForTasks(list);
			}
			else
			{
				for (int j = 0; j < posi.Length; j++)
				{
					heights[j] = SampleTerrainMeshHeight(posi[j]);
				}
			}
		}
		static async UniTask SampleAsync(NativeArray<float> heights, NativeArray<Vector3>.ReadOnly posi, int start, int end)
		{
			await UnsafeScriptingAccess.SwitchToMultithreading();
			using (TimeWarning.New("SampleTerrainMeshHeightsAsync"))
			{
				using (UnsafeScriptingAccess.Start())
				{
					for (int k = start; k < end; k++)
					{
						heights[k] = SampleTerrainMeshHeight(posi[k]);
					}
				}
			}
		}
	}

	public static bool OutOfBounds(Vector3 worldPos)
	{
		if (worldPos.x < Position.x)
		{
			return true;
		}
		if (worldPos.z < Position.z)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfBoundsBurst(Vector3 worldPos)
	{
		BurstData data = sharedBurstData.Data;
		Vector3 position = data.Position;
		Vector3 size = data.Size;
		if (worldPos.x < position.x)
		{
			return true;
		}
		if (worldPos.z < position.z)
		{
			return true;
		}
		if (worldPos.x > position.x + size.x)
		{
			return true;
		}
		if (worldPos.z > position.z + size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfMargin(Vector3 worldPos)
	{
		if (worldPos.x < Position.x - Size.x)
		{
			return true;
		}
		if (worldPos.z < Position.z - Size.z)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x + Size.x)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z + Size.z)
		{
			return true;
		}
		return false;
	}

	public static bool OutOfMarginPlusTutorialBounds(Vector3 worldPos)
	{
		if (worldPos.x < Position.x - Size.x - TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		if (worldPos.z < Position.z - Size.z - TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		if (worldPos.x > Position.x + Size.x + Size.x + TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		if (worldPos.z > Position.z + Size.z + Size.z + TutorialIsland.TutorialBoundsSize)
		{
			return true;
		}
		return false;
	}

	public static float InnerDistToEdge2D(Vector3 worldPos)
	{
		float num = Position.x - Size.x;
		float num2 = Position.x + Size.x + Size.x;
		float num3 = Position.z - Size.z;
		float num4 = Position.z + Size.z + Size.z;
		float a = Mathf.Abs(worldPos.x - num);
		float a2 = Mathf.Abs(worldPos.x - num2);
		float a3 = Mathf.Abs(worldPos.z - num3);
		float b = Mathf.Abs(worldPos.z - num4);
		return Mathf.Min(a, Mathf.Min(a2, Mathf.Min(a3, b)));
	}

	public static bool IsPointWithinTutorialBounds(Vector3 worldPos)
	{
		float tutorialBoundsSize = TutorialIsland.TutorialBoundsSize;
		float maximumPointTutorial = ValidBounds.GetMaximumPointTutorial();
		float num = 0f - maximumPointTutorial + tutorialBoundsSize;
		float num2 = maximumPointTutorial - tutorialBoundsSize;
		float num3 = 0f - maximumPointTutorial + tutorialBoundsSize;
		float num4 = maximumPointTutorial - tutorialBoundsSize;
		if (!(worldPos.x < num) && !(worldPos.x > num2) && !(worldPos.z < num3))
		{
			return worldPos.z > num4;
		}
		return true;
	}

	public static bool RandomWaterPointInAnnulus(Vector3 centre, float minRadius, float maxRadius, out Vector3 randomPoint)
	{
		for (int i = 0; i < 100; i++)
		{
			Vector3 vector = UnityEngine.Random.insideUnitCircle;
			float num = UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 vector2 = centre + new Vector3(vector.x, 0f, vector.y) * num;
			float height = HeightMap.GetHeight(vector2);
			float height2 = WaterMap.GetHeight(vector2);
			if (height <= height2)
			{
				randomPoint = vector2;
				return true;
			}
		}
		randomPoint = Vector3.zero;
		return false;
	}

	public static bool IsInBiome(Vector3 worldPos, TerrainBiome.Enum biome)
	{
		return ((uint)((BiomeMap == null) ? 2 : BiomeMap.GetBiomeMaxType(worldPos)) & (uint)biome) != 0;
	}

	public static Vector3 RandomPointOffshore(bool avoidDeepSeaPortal = false, bool avoidDeepSea = false)
	{
		float num = UnityEngine.Random.Range(-1f, 1f);
		Vector3 vector = new Vector3(Mathf.Min(Size.x, 4000f) - 100f, 0f, Mathf.Min(Size.z, 4000f) - 100f);
		avoidDeepSeaPortal = avoidDeepSeaPortal && PointEntity<DeepSeaManager>.ServerInstance != null && PointEntity<DeepSeaManager>.ServerInstance.IsOpen();
		avoidDeepSea = avoidDeepSea && PointEntity<DeepSeaManager>.ServerInstance != null;
		List<CardinalDirection> obj = Pool.Get<List<CardinalDirection>>();
		obj.Add(CardinalDirection.North);
		obj.Add(CardinalDirection.East);
		obj.Add(CardinalDirection.South);
		obj.Add(CardinalDirection.West);
		if (avoidDeepSeaPortal)
		{
			CardinalDirection entrancePortalDirection = DeepSeaManager.GetEntrancePortalDirection();
			obj.Remove(entrancePortalDirection);
		}
		if (avoidDeepSea)
		{
			obj.Remove(CardinalDirection.West);
		}
		Vector3 result = obj.GetRandom() switch
		{
			CardinalDirection.West => Center + new Vector3(0f - vector.x, 0f, num * vector.z), 
			CardinalDirection.East => Center + new Vector3(vector.x, 0f, num * vector.z), 
			CardinalDirection.South => Center + new Vector3(num * vector.x, 0f, 0f - vector.z), 
			_ => Center + new Vector3(num * vector.x, 0f, vector.z), 
		};
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public static Vector3 RandomPoint(bool excludeWater = true)
	{
		Vector3 vector = default(Vector3);
		while (vector.Equals(default(Vector3)) || (excludeWater && WaterLevel.Test(vector, waves: true, volumes: true)))
		{
			float x = UnityEngine.Random.Range(0f, Data.size.x);
			float z = UnityEngine.Random.Range(0f, Data.size.z);
			float height = HeightMap.GetHeight(new Vector3(x, 0f, z));
			vector = new Vector3(x, height, z);
		}
		return vector;
	}

	public static Vector3 Normalize(Vector3 worldPos)
	{
		float x = (worldPos.x - Position.x) * OneOverSize.x;
		float y = (worldPos.y - Position.y) * OneOverSize.y;
		float z = (worldPos.z - Position.z) * OneOverSize.z;
		return new Vector3(x, y, z);
	}

	public static float NormalizeX(float x)
	{
		return (x - Position.x) * OneOverSize.x;
	}

	public static float NormalizeY(float y)
	{
		return (y - Position.y) * OneOverSize.y;
	}

	public static float NormalizeZ(float z)
	{
		return (z - Position.z) * OneOverSize.z;
	}

	public static Vector3 Denormalize(Vector3 normPos)
	{
		float x = Position.x + normPos.x * Size.x;
		float y = Position.y + normPos.y * Size.y;
		float z = Position.z + normPos.z * Size.z;
		return new Vector3(x, y, z);
	}

	public static float DenormalizeX(float normX)
	{
		return Position.x + normX * Size.x;
	}

	public static float DenormalizeY(float normY)
	{
		return Position.y + normY * Size.y;
	}

	public static float DenormalizeZ(float normZ)
	{
		return Position.z + normZ * Size.z;
	}

	protected override void Awake()
	{
		base.Awake();
		if (UnityEngine.Application.isPlaying)
		{
			Shader.DisableKeyword("TERRAIN_PAINTING");
		}
	}

	public void Init(TerrainData terrainDataOverride = null, TerrainConfig configOverride = null)
	{
		if (configOverride != null)
		{
			config = configOverride;
		}
		if (terrainDataOverride != null)
		{
			terrainData = terrainDataOverride;
		}
		TerrainData = terrainData;
		Config = config;
		Transform = base.transform;
		Data = terrainData;
		Size = terrainData.size;
		OneOverSize = Size.Inverse();
		Position = base.transform.position;
		Collider = base.gameObject.GetComponent<TerrainCollider>();
		Collision = base.gameObject.GetComponent<TerrainCollision>();
		Physics = base.gameObject.GetComponent<TerrainPhysics>();
		Colors = base.gameObject.GetComponent<TerrainColors>();
		Quality = base.gameObject.GetComponent<TerrainQuality>();
		Path = base.gameObject.GetComponent<TerrainPath>();
		BiomeMap = base.gameObject.GetComponent<TerrainBiomeMap>();
		AlphaMap = base.gameObject.GetComponent<TerrainAlphaMap>();
		BlendMap = base.gameObject.GetComponent<TerrainBlendMap>();
		HeightMap = base.gameObject.GetComponent<TerrainHeightMap>();
		SplatMap = base.gameObject.GetComponent<TerrainSplatMap>();
		TopologyMap = base.gameObject.GetComponent<TerrainTopologyMap>();
		WaterMap = base.gameObject.GetComponent<TerrainWaterMap>();
		WaterFlowMap = base.gameObject.GetComponent<TerrainWaterFlowMap>();
		DistanceMap = base.gameObject.GetComponent<TerrainDistanceMap>();
		PlacementMap = base.gameObject.GetComponent<TerrainPlacementMap>();
		Texturing = base.gameObject.GetComponent<TerrainTexturing>();
		HoleRenderer = base.gameObject.GetComponent<TerrainHoleRenderer>();
		TerrainRenderer = terrainRenderer;
		if ((bool)terrainRenderer.terrain)
		{
			terrainRenderer.terrain.drawInstanced = false;
		}
		HighestPoint = new Vector3(Position.x, Position.y + Size.y, Position.z);
		LowestPoint = new Vector3(Position.x, Position.y, Position.z);
		TerrainExtension[] components = GetComponents<TerrainExtension>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Init(terrainRenderer, terrainData, config);
		}
		uint seed = World.Seed;
		int num = SeedRandom.Range(ref seed, 0, 4) * 90;
		int num2 = SeedRandom.Range(ref seed, -45, 46);
		int num3 = SeedRandom.Sign(ref seed);
		LootAxisAngle = num;
		BiomeAxisAngle = num + num2 + num3 * 90;
		InitBurstData();
	}

	private static void InitBurstData()
	{
		sharedBurstData.Data = new BurstData
		{
			Position = Position,
			Size = Size,
			OneOverSize = OneOverSize
		};
	}

	public static void InitNoTerrain(bool createPath = false)
	{
		Size = new Vector3(4096f, 4096f, 4096f);
		OneOverSize = Size.Inverse();
		Position = -0.5f * Size;
		InitBurstData();
	}

	public void SetupComponents()
	{
		TerrainExtension[] components = GetComponents<TerrainExtension>();
		foreach (TerrainExtension obj in components)
		{
			obj.Setup();
			obj.isInitialized = true;
		}
	}

	public void PostSetupComponents()
	{
		TerrainExtension[] components = GetComponents<TerrainExtension>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].PostSetup();
		}
		Interface.CallHook("OnTerrainInitialized");
	}

	public void BindShaderProperties()
	{
		if ((bool)config)
		{
			Shader.SetGlobalTexture("Terrain_AlbedoArray", config.AlbedoArray);
			Shader.SetGlobalTexture("Terrain_NormalArray", config.NormalArray);
			Shader.SetGlobalVector("Terrain_TexelSize", new Vector2(1f / config.GetMinSplatTiling(), 1f / config.GetMinSplatTiling()));
			Shader.SetGlobalVector("Terrain_TexelSize0", new Vector4(1f / config.Splats[0].SplatTiling, 1f / config.Splats[1].SplatTiling, 1f / config.Splats[2].SplatTiling, 1f / config.Splats[3].SplatTiling));
			Shader.SetGlobalVector("Terrain_TexelSize1", new Vector4(1f / config.Splats[4].SplatTiling, 1f / config.Splats[5].SplatTiling, 1f / config.Splats[6].SplatTiling, 1f / config.Splats[7].SplatTiling));
			Shader.SetGlobalVector("Splat0_UVMIX", new Vector3(config.Splats[0].UVMIXMult, config.Splats[0].UVMIXStart, 1f / config.Splats[0].UVMIXDist));
			Shader.SetGlobalVector("Splat1_UVMIX", new Vector3(config.Splats[1].UVMIXMult, config.Splats[1].UVMIXStart, 1f / config.Splats[1].UVMIXDist));
			Shader.SetGlobalVector("Splat2_UVMIX", new Vector3(config.Splats[2].UVMIXMult, config.Splats[2].UVMIXStart, 1f / config.Splats[2].UVMIXDist));
			Shader.SetGlobalVector("Splat3_UVMIX", new Vector3(config.Splats[3].UVMIXMult, config.Splats[3].UVMIXStart, 1f / config.Splats[3].UVMIXDist));
			Shader.SetGlobalVector("Splat4_UVMIX", new Vector3(config.Splats[4].UVMIXMult, config.Splats[4].UVMIXStart, 1f / config.Splats[4].UVMIXDist));
			Shader.SetGlobalVector("Splat5_UVMIX", new Vector3(config.Splats[5].UVMIXMult, config.Splats[5].UVMIXStart, 1f / config.Splats[5].UVMIXDist));
			Shader.SetGlobalVector("Splat6_UVMIX", new Vector3(config.Splats[6].UVMIXMult, config.Splats[6].UVMIXStart, 1f / config.Splats[6].UVMIXDist));
			Shader.SetGlobalVector("Splat7_UVMIX", new Vector3(config.Splats[7].UVMIXMult, config.Splats[7].UVMIXStart, 1f / config.Splats[7].UVMIXDist));
		}
		if ((bool)HeightMap)
		{
			Shader.SetGlobalVector("_Terrain_HeightParams", new Vector4(Position.y, Size.y, OneOverSize.y, 0f));
			Shader.SetGlobalTexture("Terrain_Normal", HeightMap.NormalTexture);
		}
		if ((bool)AlphaMap)
		{
			Shader.SetGlobalTexture("Terrain_Alpha", AlphaMap.AlphaTexture);
		}
		if ((bool)BiomeMap)
		{
			Shader.SetGlobalTexture("Terrain_Biome", BiomeMap.BiomeTexture);
		}
		if ((bool)SplatMap)
		{
			Shader.SetGlobalTexture("Terrain_Control0", SplatMap.SplatTexture0);
			Shader.SetGlobalTexture("Terrain_Control1", SplatMap.SplatTexture1);
		}
		_ = (bool)WaterMap;
		_ = (bool)DistanceMap;
		if (!terrainRenderer)
		{
			return;
		}
		Shader.SetGlobalVector("Terrain_Position", Position);
		Shader.SetGlobalVector("Terrain_Size", Size);
		Shader.SetGlobalVector("Terrain_RcpSize", OneOverSize);
		Shader.SetGlobalVector("Terrain_Global_Position", Position);
		Shader.SetGlobalVector("Terrain_Global_Size", Size);
		Shader.SetGlobalVector("Terrain_Global_RcpSize", OneOverSize);
		if ((bool)terrainRenderer.material)
		{
			Material material = terrainRenderer.material;
			if (material.IsKeywordEnabled("_TERRAIN_BLEND_LINEAR"))
			{
				material.DisableKeyword("_TERRAIN_BLEND_LINEAR");
			}
			if (material.IsKeywordEnabled("_TERRAIN_VERTEX_NORMALS"))
			{
				material.DisableKeyword("_TERRAIN_VERTEX_NORMALS");
			}
		}
	}
}
