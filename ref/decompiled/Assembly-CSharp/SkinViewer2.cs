using System.Collections.Generic;
using Rust.Workshop;
using UnityEngine;
using UnityEngine.Rendering;

public class SkinViewer2 : SingletonComponent<SkinViewer2>
{
	public Camera cam;

	public Camera viewmodelCam;

	private ViewModelCamera viewmodelCamComponent;

	[SerializeField]
	private GameObject parent;

	[SerializeField]
	private GameObject modelsParent;

	[SerializeField]
	private GameObject defaultLightingRig;

	[SerializeField]
	private GameObject vmLightingRig;

	[SerializeField]
	private Cubemap reflectionCubemap;

	private Cubemap originalReflectionCubemap;

	private DefaultReflectionMode originalReflectionMode;

	private bool hasStoredReflectionSettings;

	public List<SkinViewerRenderSettings> renderSettings;

	[Space]
	[SerializeField]
	private SkinViewerRenderSettings charmTemplateSettings;

	[SerializeField]
	private Vector3 charmSpawnPos;

	[SerializeField]
	private Vector3 charmSpawnRot;

	private CoverImage targetImage;

	private static ItemSchema.Item[] schemaItems;

	private readonly Dictionary<Transform, Quaternion> authoredModelRotations = new Dictionary<Transform, Quaternion>();

	private BaseViewModel currentBaseViewModel;

	private ItemDefinition currentItemDef;

	private AccessoryItem currentAccessoryItem;

	private GameObject currentCharmWorldModel;

	private bool currentItemIsMelee;

	private bool currentItemHasADS;

	private GameObject currentEntityPrefab;

	private int currentSkinID;

	private ulong currentWorkshopID;

	private int setGeneration;

	private static readonly List<string> requiredAssetScenes = new List<string> { "AssetScene-prefabs" };

	public GameObject currentSkinGameObject { get; private set; }

	public GameObject currentViewmodelGameObject { get; private set; }

	public bool isShowingViewmodel { get; private set; }

	public bool isViewmodelAdsing { get; private set; }

	public bool IsOpen => parent.activeSelf;
}
