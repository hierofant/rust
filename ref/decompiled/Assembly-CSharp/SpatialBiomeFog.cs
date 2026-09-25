using UnityEngine;

[ExecuteAlways]
public class SpatialBiomeFog : SingletonComponent<SpatialBiomeFog>
{
	public ComputeShader FogMarchShader;

	public ComputeShader VoxelBlurShader;

	public ComputeShader StencilShader;

	public Vector3i Resolution = new Vector3i(128, 128, 32);

	public Texture NoiseTexture;

	public float BiomeFogDensityScale = 1f;

	public bool DoVoxelBlur = true;

	public int BlurPasses = 1;

	public float BlurRadius = 1f;

	public int NumDownsamples = 4;

	public float EnvBiomeFogDensity { get; set; } = 1f;


	public float UndergroundFogDensity { get; set; }
}
