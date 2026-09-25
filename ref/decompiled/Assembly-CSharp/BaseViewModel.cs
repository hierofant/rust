using System.Collections.Generic;
using Rust.Workshop;
using UnityEngine;

public class BaseViewModel : FacepunchBehaviour, IPrefabPreProcess, IViewmodelWorkshopPreview, IWorkshopPreview
{
	public enum ViewmodelType
	{
		Regular,
		Gesture,
		Shield,
		Turret
	}

	[Header("BaseViewModel")]
	public LazyAimProperties lazyaimRegular;

	public LazyAimProperties lazyaimIronsights;

	public Transform pivot;

	public bool useViewModelCamera = true;

	public bool allowSkinViewerInspection = true;

	public bool wantsHeldItemFlags;

	public GameObject[] hideSightMeshes;

	public ViewmodelType viewmodelType;

	public bool createHandMesh;

	public bool shouldOverridePosition;

	public Transform MuzzlePoint;

	[Header("Charms")]
	public Transform charmAnchor;

	public float charmScale;

	public bool useFlatBackCharm;

	public bool usePushedOutCharm;

	[Header("Skin")]
	public SubsurfaceProfile subsurfaceProfile;

	[HideInInspector]
	public List<SkinnedMeshRenderer> baseSkinPieces = new List<SkinnedMeshRenderer>();

	[Header("Shield Overrides")]
	public AnimationCurve leftArmShieldHideCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	void IPrefabPreProcess.PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
