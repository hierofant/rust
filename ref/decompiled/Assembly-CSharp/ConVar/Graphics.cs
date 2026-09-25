using Rust.Workshop;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConVar;

[Factory("graphics")]
public class Graphics : ConsoleSystem
{
	private static readonly int shadowFilterQualityId = Shader.PropertyToID("_ShadowFilterQuality");

	[ClientVar(Help = "(Generated) When enabled, outputs detailed per-pass profiling data for the post-processing stack to the profiler; useful for identifying expensive post-processing passes")]
	public static bool detailed_postprocessing_profiling = false;

	[ClientVar(Saved = true, Help = "(Generated) Shadow quality mode: 1 = hard, 2 = soft (PCF), 3 = high quality (PCSS), 4 = very high quality; higher modes are more expensive")]
	public static int shadowmode = 2;

	private static int _shadowlights = 1;

	private static int _shadowFilterQuality = 1;

	[ClientVar(DeveloperOnly = true, Help = "(Generated) Developer-only: when enabled, grass geometry casts shadows; disabled by default due to performance cost on GPU shadow rendering")]
	public static bool grassShadowsEnabled = false;

	[ClientVar(Saved = true, Help = "(Generated) When enabled, renders screen-space contact shadows to add fine-detail shadows at object contact points not covered by shadow maps")]
	public static bool contactshadows = false;

	[ClientVar(Saved = true, Help = "(Generated) Maximum camera draw distance in metres; objects beyond this distance are culled; lower values improve GPU performance at the cost of view distance")]
	public static float drawdistance = 2500f;

	private static EncryptedValue<float> _fov = 75f;

	[ClientVar(Help = "(Generated) When enabled, the in-game HUD is visible; disable to hide all HUD elements (health bar, hotbar, etc.) for screenshots")]
	public static bool hud = true;

	[ClientVar(Saved = true, Help = "(Generated) When enabled, the chat window is displayed; also gated by global.canChat")]
	public static bool chat = true;

	[ClientVar(Saved = true, Help = "(Generated) When enabled, shows the Facepunch/Rust branding watermark on screen")]
	public static bool branding = true;

	[ClientVar(Saved = true, Help = "Compass display mode: 0 = hidden, 1 = always visible, 2 = visible whilst Compass keybind is held, 3 = visible when Compass keybind is toggled")]
	public static int compass = 1;

	[ClientVar(Saved = true, Help = "(Generated) When enabled, activates the depth-of-field post-processing effect, blurring objects outside the focal distance")]
	public static bool dof = false;

	[ClientVar(Saved = true, Help = "(Generated) Depth-of-field aperture (f-stop); lower values produce a shallower depth of field with more background blur")]
	public static float dof_aper = 12f;

	[ClientVar(Saved = true, Help = "(Generated) Depth-of-field blur intensity multiplier; higher values produce a stronger blur on out-of-focus areas")]
	public static float dof_blur = 1f;

	[ClientVar(Saved = true, Help = "0 = auto 1 = manual 2 = dynamic based on target")]
	public static int dof_mode = 0;

	[ClientVar(Saved = true, Help = "distance from camera to focus on")]
	public static float dof_focus_dist = 10f;

	[ClientVar(Saved = true, Help = "(Generated) Time in seconds for the depth-of-field focus distance to lerp to a new target when auto or dynamic mode is active")]
	public static float dof_focus_time = 0.2f;

	[ClientVar(Saved = true, ClientAdmin = true, Help = "(Generated) Admin/cinematic: anamorphic squeeze factor applied to DoF bokeh shapes; 0 = circular, positive values = oval/anamorphic")]
	public static float dof_squeeze = 0f;

	[ClientVar(Saved = true, ClientAdmin = true, Help = "(Generated) Admin/cinematic: barrel distortion amount applied to the depth-of-field effect")]
	public static float dof_barrel = 0f;

	[ClientVar(Saved = true, ClientAdmin = true, Help = "(Generated) Admin/cinematic: when enabled, draws a debug overlay showing the depth buffer and circle of confusion for DoF tuning")]
	public static bool dof_debug = false;

	[ClientVar(Saved = true, Help = "Goes from 0 - 3, higher = more dof samples but slower perf")]
	public static int dof_kernel_count = 0;

	public static BaseEntity dof_focus_target_entity = null;

	[ClientVar(Saved = true, Help = "Whether to scale vm models with fov")]
	public static bool vm_fov_scale = true;

	[ClientVar(Saved = true, Help = "FLips viewmodels horizontally (for left handed players)")]
	public static bool vm_horizontal_flip = false;

	private static float _uiscale = 1f;

	private static int _anisotropic = 1;

	private static int _parallax = 0;

	[ClientVar(Help = "Represents the number of vertical syncs that should pass between each frame. An integer in the range of 0-4. ", Saved = true)]
	public static int vsync
	{
		get
		{
			return QualitySettings.vSyncCount;
		}
		set
		{
			QualitySettings.vSyncCount = Mathf.Clamp(value, 0, 4);
		}
	}

	[ClientVar(Saved = true, Help = "(Generated) Maximum number of lights that can cast real-time shadows simultaneously; clamped to a minimum of 1; higher values improve shadow coverage at GPU cost")]
	public static int shadowlights
	{
		get
		{
			return _shadowlights;
		}
		set
		{
			_shadowlights = Mathf.Max(value, 1);
		}
	}

	[ClientVar(DeveloperOnly = true, Help = "(Generated) Developer-only: shadow PCF filtering quality 0-3; higher values use more shadow map samples for softer edges; also updates shadowmode and shader keywords")]
	public static int shadowFilterQuality
	{
		get
		{
			return _shadowFilterQuality;
		}
		set
		{
			_shadowFilterQuality = Mathf.Clamp(value, 0, 3);
			shadowmode = _shadowFilterQuality + 1;
			bool flag = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
			KeywordUtil.EnsureKeywordState("SHADOW_QUALITY_HIGH", !flag && _shadowFilterQuality == 2);
			KeywordUtil.EnsureKeywordState("SHADOW_QUALITY_VERYHIGH", !flag && _shadowFilterQuality == 3);
			Shader.SetGlobalInt(shadowFilterQualityId, _shadowFilterQuality);
		}
	}

	[ClientVar(Saved = true, Help = "(Generated) Camera field of view in degrees, clamped between 70 and 90; higher values provide a wider view but can cause edge distortion")]
	public static float fov
	{
		get
		{
			return _fov;
		}
		set
		{
			_fov = Mathf.Clamp(value, 70f, 90f);
		}
	}

	[ClientVar(Help = "Global multiplier for the LOD's switching distance. A larger value leads to a longer view distance before a lower resolution LOD is picked.", Saved = true)]
	public static float lodBias
	{
		get
		{
			return QualitySettings.lodBias;
		}
		set
		{
			QualitySettings.lodBias = Mathf.Clamp(value, 0.5f, 5f);
		}
	}

	[ClientVar(Saved = true, Help = "(Generated) UI scale multiplier clamped between 0.5 and 1.0; lower values shrink all UI elements to fit more on screen")]
	public static float uiscale
	{
		get
		{
			return _uiscale;
		}
		set
		{
			_uiscale = Mathf.Clamp(value, 0.5f, 1f);
		}
	}

	[ClientVar(Saved = true, Help = "(Generated) Anisotropic filtering level 1-16; higher values improve texture sharpness at oblique angles at a minor GPU cost; 1 = disabled")]
	public static int af
	{
		get
		{
			return _anisotropic;
		}
		set
		{
			value = Mathf.Clamp(value, 1, 16);
			UnityEngine.Texture.SetGlobalAnisotropicFilteringLimits(1, value);
			if (value <= 1)
			{
				UnityEngine.Texture.anisotropicFiltering = AnisotropicFiltering.Disable;
			}
			if (value > 1)
			{
				UnityEngine.Texture.anisotropicFiltering = AnisotropicFiltering.Enable;
			}
			_anisotropic = value;
		}
	}

	[ClientVar(Saved = true, Help = "(Generated) Terrain parallax mapping mode: 0 = off, 1 = parallax offset, 2 = parallax occlusion; higher modes improve terrain depth at GPU cost")]
	public static int parallax
	{
		get
		{
			return _parallax;
		}
		set
		{
			switch (value)
			{
			default:
				Shader.DisableKeyword("TERRAIN_PARALLAX_OFFSET");
				Shader.DisableKeyword("TERRAIN_PARALLAX_OCCLUSION");
				break;
			case 1:
				Shader.EnableKeyword("TERRAIN_PARALLAX_OFFSET");
				Shader.DisableKeyword("TERRAIN_PARALLAX_OCCLUSION");
				break;
			case 2:
				Shader.DisableKeyword("TERRAIN_PARALLAX_OFFSET");
				Shader.EnableKeyword("TERRAIN_PARALLAX_OCCLUSION");
				break;
			}
			_parallax = value;
		}
	}

	[ClientVar(ClientAdmin = true, Help = "(Generated) When enabled, Steam Workshop skins are applied to items; disable to revert all items to default appearance")]
	public static bool itemskins
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.AllowApply;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.AllowApply = value;
		}
	}

	[ClientVar(Help = "(Generated) When enabled, unreferenced workshop skin assets are unloaded from memory after the unload delay elapses")]
	public static bool itemskinunload
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.AllowUnload;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.AllowUnload = value;
		}
	}

	[ClientVar(Help = "The time in seconds before a unreferenced skin is unloaded.")]
	public static float itemskinunloaddelay
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.UnloadDelay;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.UnloadDelay = value;
		}
	}

	[ClientVar(ClientAdmin = true, Help = "(Generated) Timeout in seconds before a workshop skin download is considered failed and abandoned")]
	public static float itemskintimeout
	{
		get
		{
			return Rust.Workshop.WorkshopSkin.DownloadTimeout;
		}
		set
		{
			Rust.Workshop.WorkshopSkin.DownloadTimeout = value;
		}
	}

	[ClientVar(ClientAdmin = true, Help = "(Generated) Sets a player or entity as the live depth-of-field focus target by name/entity ID; the DoF focus distance tracks the target while set")]
	public static void dof_focus_target(Arg arg)
	{
	}

	[ClientVar(Help = "(Generated) Adjusts the current manual depth-of-field focus distance by the given amount in metres; negative values bring focus closer")]
	public static void dof_nudge(Arg arg)
	{
		float @float = arg.GetFloat(0);
		dof_focus_dist += @float;
		if (dof_focus_dist < 0f)
		{
			dof_focus_dist = 0f;
		}
	}
}
