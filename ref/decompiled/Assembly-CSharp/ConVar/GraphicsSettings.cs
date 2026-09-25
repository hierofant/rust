using System.Linq;
using UnityEngine;

namespace ConVar;

[Factory("graphicssettings")]
public class GraphicsSettings : ConsoleSystem
{
	private static int _shadowQualityPreset = 3;

	private static ShadowQualityPresetsConfig _shadowQualityPresetsConfig;

	[ClientVar(Help = "The maximum number of pixel lights that should affect any object.", Saved = true)]
	public static int pixelLightCount
	{
		get
		{
			return QualitySettings.pixelLightCount;
		}
		set
		{
			QualitySettings.pixelLightCount = Mathf.Clamp(value, 0, 8);
		}
	}

	[ClientVar(Help = "Indicates how many of the highest-resolution mips of each texture Unity does not upload at the given quality level.", Saved = true)]
	public static int globalTextureMipmapLimit
	{
		get
		{
			return QualitySettings.globalTextureMipmapLimit;
		}
		set
		{
			value = Mathf.Clamp(value, 0, 3);
			bool num = QualitySettings.globalTextureMipmapLimit != value;
			QualitySettings.globalTextureMipmapLimit = value;
			if (num && SingletonComponent<FoliageGrid>.Instance != null)
			{
				SingletonComponent<FoliageGrid>.Instance.OnGlobalTextureMipmapLimitChange();
			}
		}
	}

	[ClientVar(Help = "Global anisotropic filtering mode. 0-2. Disabled, enabled per-texture, force-enabled for all textures.", Saved = true)]
	public static int anisotropicFiltering
	{
		get
		{
			return (int)QualitySettings.anisotropicFiltering;
		}
		set
		{
			QualitySettings.anisotropicFiltering = (AnisotropicFiltering)Mathf.Clamp(value, 0, 2);
		}
	}

	[ClientVar(Help = "Should soft blending be used for particles?", Saved = true)]
	public static bool softParticles
	{
		get
		{
			return QualitySettings.softParticles;
		}
		set
		{
			QualitySettings.softParticles = value;
		}
	}

	[ClientVar(Help = "Budget for how many ray casts can be performed per frame for approximate collision testing.", Saved = true)]
	public static int particleRaycastBudget
	{
		get
		{
			return QualitySettings.particleRaycastBudget;
		}
		set
		{
			QualitySettings.particleRaycastBudget = value;
		}
	}

	[ClientVar(Help = "If enabled, billboards will face towards camera position rather than camera orientation.", Saved = true)]
	public static bool billboardsFaceCameraPosition
	{
		get
		{
			return QualitySettings.billboardsFaceCameraPosition;
		}
		set
		{
			QualitySettings.billboardsFaceCameraPosition = value;
		}
	}

	[ClientVar(Help = "The preset that determines all shadow settings. Low for performance, Max for the best visuals.", Saved = true)]
	public static int shadowQualityPreset
	{
		get
		{
			return _shadowQualityPreset;
		}
		set
		{
			if ((object)_shadowQualityPresetsConfig == null)
			{
				_shadowQualityPresetsConfig = Resources.Load<ShadowQualityPresetsConfig>("ShadowQualityPresetsConfig");
			}
			if (_shadowQualityPresetsConfig == null)
			{
				Debug.LogError("Failed to load Shadow Quality Preset Config!");
				return;
			}
			_shadowQualityPreset = Mathf.Clamp(value, 0, 3);
			ShadowQualityPresetsConfig.Preset preset = _shadowQualityPresetsConfig.Presets[_shadowQualityPreset];
			QualitySettings.shadowCascades = (int)preset.numCascades;
			QualitySettings.shadowDistance = preset.shadowDistance;
			QualitySettings.shadowResolution = preset.shadowResolution;
			Graphics.shadowFilterQuality = preset.filteringQuality;
			Graphics.grassShadowsEnabled = preset.grassShadowsEnabled;
			QualitySettings.shadowNearPlaneOffset = 2f;
			QualitySettings.shadowCascade2Split = 0.1f;
			QualitySettings.shadowCascade4Split = new Vector3(0.01f, 0.03f, 0.1f);
		}
	}

	[ClientVar(Help = "The default resolution of shadow maps. 0 = Low, 1 = Medium, 2 = High, 3 = Very High", DeveloperOnly = true)]
	public static int shadowMapResolution
	{
		get
		{
			return (int)QualitySettings.shadowResolution;
		}
		set
		{
			QualitySettings.shadowResolution = (ShadowResolution)Mathf.Clamp(value, 0, 3);
		}
	}

	[ClientVar(Help = "Shadow drawing distance", DeveloperOnly = true)]
	public static float shadowDistance
	{
		get
		{
			return QualitySettings.shadowDistance;
		}
		set
		{
			QualitySettings.shadowDistance = value;
		}
	}

	[ClientVar(Help = "Number of cascades to use for directional light shadows. 1 = None, 2 = Two, 4 = Four", DeveloperOnly = true)]
	public static int numShadowCascades
	{
		get
		{
			return QualitySettings.shadowCascades;
		}
		set
		{
			QualitySettings.shadowCascades = ForceOption(value, 2, 2, 4);
		}
	}

	[ClientVar(Help = "Enables or disables LOD Cross Fade.", Saved = true)]
	public static bool enableLODCrossFade
	{
		get
		{
			return QualitySettings.enableLODCrossFade;
		}
		set
		{
			QualitySettings.enableLODCrossFade = value;
		}
	}

	public static void SetMandatoryDefaults()
	{
		QualitySettings.useLegacyDetailDistribution = true;
		QualitySettings.terrainQualityOverrides = TerrainQualityOverrides.None;
		QualitySettings.shadowNearPlaneOffset = 2f;
		QualitySettings.shadowCascade2Split = 0.1f;
		QualitySettings.shadowCascade4Split = new Vector3(0.01f, 0.03f, 0.1f);
		QualitySettings.asyncUploadTimeSlice = 2;
		QualitySettings.asyncUploadBufferSize = 4;
		QualitySettings.asyncUploadPersistentBuffer = true;
		QualitySettings.maximumLODLevel = 0;
		QualitySettings.enableLODCrossFade = true;
		QualitySettings.skinWeights = SkinWeights.FourBones;
		QualitySettings.resolutionScalingFixedDPIFactor = 1f;
		QualitySettings.shadows = ShadowQuality.All;
		QualitySettings.shadowProjection = ShadowProjection.StableFit;
	}

	private static int ForceOption(int value, int defaultValue, params int[] options)
	{
		if (options == null || options.Length == 0 || !options.Contains(value))
		{
			return defaultValue;
		}
		return value;
	}
}
