using Rust.UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace ModelViewer;

public class modelviewer : MonoBehaviour, IEditorComponent
{
	public GameObject cameraTarget;

	[Header("                ")]
	public ModelViewer_RenderParameters Render;

	public ModelViewer_MainCameraParameters MainCamera;

	public ModelViewer_SkyParameters Sky;

	[Header("                ")]
	[Header("                ")]
	[Header("Junk to clean up:")]
	public GameObject ruleOfThirds;

	public GameObject goldenRatio;

	public GameObject crosshair;

	public GameObject aspectRatio;

	public GameObject safeFrames;

	private GameObject enabledOverlay;

	public AspectRatioFitter aspectRatioFitter;

	public AspectRatioFitter safeFrameAspectRatioFigger;

	public RustText aspectRatioLabel;

	private Vector3 startpos = new Vector3(0f, 130f, 60f);

	public Camera maincamera;

	public GameObject modelHolder;

	public Transform skyDome;

	public float skyrotateSpeed;

	public Light sunLight;

	public NGSS_Directional sunLightSoftShadows;

	private GameObject orbitVector;

	private Quaternion orbt_rot_original;

	private Vector3 orbt_xform_original;

	private SEScreenSpaceShadows screenSpaceShadows;

	private AmplifyOcclusionBase ambientOcclusion;

	public ReflectionProbe[] reflectionProbes;

	private TOD_Sky todSky;

	private PostProcessVolume[] colorGradingVolumes;

	public PostProcessVolume basePostProcess;

	public PostProcessVolume sharpenvignettePostProcess;

	public PostProcessVolume colorGradingPostProcess;

	private LensDistortion lensDistortion;

	private ChromaticAberration chromaticAbberation;

	private SharpenAndVignette sharpenAndVignette;

	private Bloom bloom;

	private Grain grain;

	private ColorGrading colorGrading;

	private GodRays godRays;

	private MotionBlur motionBlur;

	public Vector3 skyRotation;

	public Vector3 cameraPosition;

	public Vector3 cameraRotation;

	public float cameraZoom;

	public Vector3 orbitVectorPosition;
}
