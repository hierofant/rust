using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Facepunch.Extend;
using Rust.Components.Camera;
using Rust.ImageEffects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rust;

[ExecuteInEditMode]
public class PropRenderer : MonoBehaviour, IClientComponent
{
	[Serializable]
	public class SkinViewerSettings
	{
		[Tooltip("If non-zero, will be used as a pivot point instead of the centre of the enclosing bounds")]
		public Vector3 customLocalPivot = Vector3.zero;

		[Tooltip("Additional camera offset only used in the skin viewer (as opposed to icon generation)")]
		public Vector3 camPosOffset;

		[Tooltip("For objects that rotate weirdly in other skin viewer pivot modes")]
		public bool forceCamUpPivot;

		public bool HasCustomPivot => customLocalPivot != Vector3.zero;
	}

	public delegate float LightIntensityScale(float intensity);

	public bool HideLowLods = true;

	public bool HideUnskinnable = true;

	public bool Outline = true;

	public bool ForceIdleAnimation;

	[Header("Rotation")]
	public Vector3 Rotation = Vector3.zero;

	public Vector3 PostRotation = Vector3.zero;

	[Header("Position Offset")]
	public Vector3 PositionalTweak = Vector3.zero;

	[Header("Misc")]
	public GameObject[] HideDuringRender = new GameObject[0];

	public float FieldOfView = 40f;

	public float farClipPlane = 100f;

	public bool ForceLookAtProp;

	public Vector3 LookDirection = new Vector3(0.2f, 0.4f, 1f);

	public Vector3 UpDirection = Vector3.up;

	[Range(0f, 1f)]
	public float Light1IntensityMultiplier = 1f;

	[Range(-360f, 360f)]
	public float Light1Rotation = 60f;

	[Range(0f, 1f)]
	public float Light2IntensityMultiplier = 1f;

	[Range(-360f, 360f)]
	public float Light2Rotation = -35f;

	public string PathOverride = "";

	[Header("Bind Pose")]
	public bool UseLegacyBindPose;

	private const string LegacyBindPosePrefabPath = "Assets/Content/Player/IconRenderPose.prefab";

	private const string NewBindPosePrefabPath = "Assets/Content/Player/Animations/RustPlayer.fbx";

	public SkinViewerSettings skinViewerSettings;

	private static void GetTransformsRootToChild(List<Transform> transforms, Transform start)
	{
		Transform transform = start;
		while (transform != null)
		{
			transforms.Add(transform);
			transform = transform.parent;
		}
		transforms.Reverse();
	}

	private static Transform FindMatchingTransforms(List<Transform> sourceHierarchy, Transform target)
	{
		Transform transform = target;
		if (sourceHierarchy == null || sourceHierarchy.Count == 0)
		{
			return null;
		}
		if (target.name != sourceHierarchy[0].name)
		{
			Debug.LogWarning("PropRenderer: Target object " + target.name + " does not match the first object in the source hierarchy " + sourceHierarchy[0].name + ". Skipping.");
			return null;
		}
		for (int i = 1; i < sourceHierarchy.Count; i++)
		{
			Transform transform2 = sourceHierarchy[i];
			Transform transform3 = transform.Find(transform2.name);
			if (transform3 == null)
			{
				return null;
			}
			transform = transform3;
		}
		return transform;
	}

	public void SyncLegacyBindPoseState()
	{
		Type type = Type.GetType("Wearable,Assembly-CSharp");
		if (!(type == null) && !(GetComponentInChildren(type, includeInactive: true) == null))
		{
			if (UseLegacyBindPose)
			{
				ApplyPoseFromAsset("Assets/Content/Player/IconRenderPose.prefab");
			}
			else
			{
				ApplyPoseFromAsset("Assets/Content/Player/Animations/RustPlayer.fbx");
			}
		}
	}

	private void ApplyPoseFromAsset(string assetPath)
	{
		GameObject gameObject = Global.LoadPrefab?.Invoke(assetPath);
		if (gameObject == null)
		{
			Debug.LogWarning("PropRenderer: Could not load bind pose asset at " + assetPath, this);
			return;
		}
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			dictionary[transform.name] = transform;
		}
		componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform2 in componentsInChildren)
		{
			if (!(transform2 == gameObject.transform) && dictionary.TryGetValue(transform2.name, out var value))
			{
				value.localPosition = transform2.localPosition;
				value.localRotation = transform2.localRotation;
				value.localScale = transform2.localScale;
			}
		}
	}

	private static GameObject[] MatchGameObjectsBetweenObjects(GameObject[] sourceObjects, GameObject target)
	{
		List<GameObject> obj = Pool.Get<List<GameObject>>();
		foreach (GameObject gameObject in sourceObjects)
		{
			if (gameObject == null)
			{
				obj.Add(null);
				continue;
			}
			List<Transform> obj2 = Pool.Get<List<Transform>>();
			GetTransformsRootToChild(obj2, gameObject.transform);
			Transform transform = FindMatchingTransforms(obj2, target.transform);
			if (transform != null)
			{
				obj.Add(transform.gameObject);
			}
			else
			{
				Debug.LogWarning("PropRenderer: Could not find matching hierarchy for " + gameObject.name + " in target object " + target.name + ". Skipping.");
			}
			Pool.FreeUnmanaged(ref obj2);
		}
		GameObject[] result = obj.ToArray();
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public void CopySettingsTo(PropRenderer target)
	{
		target.farClipPlane = farClipPlane;
		target.FieldOfView = FieldOfView;
		target.ForceLookAtProp = ForceLookAtProp;
		target.HideLowLods = HideLowLods;
		target.HideUnskinnable = HideUnskinnable;
		target.Light1IntensityMultiplier = Light1IntensityMultiplier;
		target.Light1Rotation = Light1Rotation;
		target.Light2IntensityMultiplier = Light2IntensityMultiplier;
		target.Light2Rotation = Light2Rotation;
		target.LookDirection = LookDirection;
		target.Outline = Outline;
		target.PositionalTweak = PositionalTweak;
		target.PostRotation = PostRotation;
		target.Rotation = Rotation;
		target.UpDirection = UpDirection;
		target.UseLegacyBindPose = UseLegacyBindPose;
		if (HideDuringRender != null && HideDuringRender.Length != 0)
		{
			GameObject[] source = MatchGameObjectsBetweenObjects(HideDuringRender, target.gameObject);
			target.HideDuringRender = source.ToArray();
		}
	}

	public void DebugAlign()
	{
		PreRender();
		Camera main = Camera.main;
		main.fieldOfView = FieldOfView;
		PositionCamera(main, isSkinViewer: true);
		PostRender();
	}

	public void PositionCamera(Camera cam, bool isSkinViewer = false)
	{
		Vector3 vector = Quaternion.Euler(Rotation) * LookDirection.normalized;
		Vector3 vector2 = Quaternion.Euler(Rotation) * UpDirection.normalized;
		vector = Quaternion.Euler(PostRotation) * vector;
		vector2 = Quaternion.Euler(PostRotation) * vector2;
		cam.fieldOfView = FieldOfView;
		cam.nearClipPlane = 0.01f;
		cam.farClipPlane = farClipPlane;
		cam.FocusOnRenderer(base.gameObject, vector, vector2, 2048);
		cam.transform.position += PositionalTweak.x * cam.transform.right * 0.01f;
		cam.transform.position += PositionalTweak.y * cam.transform.up * 0.01f;
		cam.transform.position += PositionalTweak.z * cam.transform.forward * 0.1f;
		if (isSkinViewer)
		{
			cam.transform.position += skinViewerSettings.camPosOffset.x * cam.transform.right * 0.01f;
			cam.transform.position += skinViewerSettings.camPosOffset.y * cam.transform.up * 0.01f;
			cam.transform.position += skinViewerSettings.camPosOffset.z * cam.transform.forward * 0.1f;
		}
	}

	public void PreRender()
	{
		PreRender(base.gameObject, HideLowLods, HideUnskinnable, HideDuringRender);
		SyncLegacyBindPoseState();
	}

	public static void PreRender(GameObject gameObject, bool hideLowLODs, bool hideUnskinnable, GameObject[] hideDuringRender = null)
	{
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer is ParticleSystemRenderer) && !renderer.gameObject.CompareTag("StripFromStorePreview") && (!hideLowLODs || (!renderer.gameObject.name.EndsWith("lod01", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod02", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod03", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod04", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod1", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod2", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod3", StringComparison.InvariantCultureIgnoreCase) && !renderer.gameObject.name.EndsWith("lod4", StringComparison.InvariantCultureIgnoreCase))))
			{
				renderer.gameObject.layer = 11;
				SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
				if ((bool)skinnedMeshRenderer)
				{
					skinnedMeshRenderer.updateWhenOffscreen = true;
				}
			}
		}
	}

	public void PostRender()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer is ParticleSystemRenderer))
			{
				renderer.gameObject.layer = 0;
				SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
				if ((bool)skinnedMeshRenderer)
				{
					skinnedMeshRenderer.updateWhenOffscreen = false;
				}
			}
		}
	}

	[ContextMenu("Create 512x512 Icon")]
	public void CreateIcon()
	{
		GameObject gameObject = base.gameObject;
		Debug.Log("Saving To " + UnityEngine.Application.dataPath + "/" + gameObject.name + ".icon.png");
		ScreenshotToDisk(UnityEngine.Application.dataPath + "/" + gameObject.name + ".icon.png", 512, 512, 4);
	}

	[ContextMenu("Create Large Render")]
	public void CreateRender()
	{
		GameObject gameObject = base.gameObject;
		Debug.Log("Saving To " + UnityEngine.Application.dataPath + "/" + gameObject.name + ".large.png");
		ScreenshotToDisk(UnityEngine.Application.dataPath + "/" + gameObject.name + ".large.png", 4096, 4096);
	}

	public static float DefaultLightIntensityScale(float intensity)
	{
		return Mathf.GammaToLinearSpace(intensity) * MathF.PI;
	}

	public Texture2D ScreenshotToTexture(int width, int height, int superSampleSize, LightIntensityScale lightIntensityScale = null)
	{
		bool streamingTextureForceLoadAll = Texture.streamingTextureForceLoadAll;
		Texture.streamingTextureForceLoadAll = true;
		GameObject gameObject = new GameObject("Temporary Camera");
		Camera camera = gameObject.AddComponent<Camera>();
		camera.clearFlags = CameraClearFlags.Depth;
		camera.backgroundColor = new Color(1f, 1f, 1f, 0f);
		camera.allowHDR = true;
		camera.allowMSAA = false;
		Type type = Type.GetType("DeferredIndirectLightingPass,Assembly-CSharp");
		if (type != null)
		{
			gameObject.AddComponent(type);
		}
		gameObject.AddComponent<StreamingController>();
		Type type2 = Type.GetType("DeferredExtension,Assembly-CSharp");
		if (type2 != null)
		{
			gameObject.AddComponent(type2);
		}
		Type type3 = Type.GetType("DeferredDecalRenderer,Assembly-CSharp");
		if (type2 != null)
		{
			gameObject.AddComponent(type3);
		}
		if (ReflectionProbe.defaultTexture != null)
		{
			Shader.SetGlobalTexture("global_SkyReflection", ReflectionProbe.defaultTexture);
			Shader.SetGlobalVector("global_SkyReflection_HDR", new Vector2(0.2f, 0.01f));
		}
		if (Outline)
		{
			gameObject.AddComponent<IconOutline>();
		}
		LightingOverride lightingOverride = gameObject.AddComponent<LightingOverride>();
		lightingOverride.overrideAmbientLight = true;
		lightingOverride.ambientMode = AmbientMode.Flat;
		lightingOverride.ambientLight = new Color(0.4f, 0.4f, 0.4f, 1f);
		lightingOverride.overrideSkyReflection = true;
		GameObject obj = new GameObject("Temporary Light");
		obj.transform.SetParent(camera.transform);
		CreateSunLight(obj, this, lightIntensityScale);
		GameObject obj2 = new GameObject("Temporary Light");
		obj2.transform.SetParent(camera.transform);
		CreateGeneralLight(obj2, this, lightIntensityScale);
		PreRender();
		try
		{
			camera.cullingMask = 2048;
			PositionCamera(camera);
			return camera.ScreenshotToTexture(width, height, transparent: true, superSampleSize, camera.backgroundColor);
		}
		finally
		{
			Texture.streamingTextureForceLoadAll = streamingTextureForceLoadAll;
			UnityEngine.Object.DestroyImmediate(gameObject);
			PostRender();
		}
	}

	public void ScreenshotToDisk(string filename, int width, int height, int superSampleSize = 1, LightIntensityScale lightIntensityScale = null)
	{
		Texture2D texture2D = ScreenshotToTexture(width, height, superSampleSize, lightIntensityScale);
		CameraEx.SavePNG(filename, texture2D);
		UnityEngine.Object.DestroyImmediate(texture2D);
	}

	public static Light CreateSunLight(GameObject lightgo, PropRenderer propRenderer, LightIntensityScale lightIntensityScale = null)
	{
		if (lightIntensityScale == null)
		{
			lightIntensityScale = DefaultLightIntensityScale;
		}
		Light light = lightgo.GetComponent<Light>();
		if (light == null)
		{
			light = lightgo.AddComponent<Light>();
		}
		lightgo.transform.localRotation = Quaternion.Euler(115f, propRenderer.Light2Rotation, 0f);
		light.type = LightType.Directional;
		light.color = new Color(1f, 1f, 0.96f);
		light.cullingMask = 2048;
		light.shadows = LightShadows.Soft;
		light.shadowBias = 0.01f;
		light.shadowNormalBias = 0.01f;
		light.shadowStrength = 0.9f;
		light.intensity = 2f * lightIntensityScale(propRenderer.Light2IntensityMultiplier);
		return light;
	}

	public static Light CreateGeneralLight(GameObject lightgo, PropRenderer propRenderer, LightIntensityScale lightIntensityScale = null)
	{
		if (lightIntensityScale == null)
		{
			lightIntensityScale = DefaultLightIntensityScale;
		}
		Light light = lightgo.GetComponent<Light>();
		if (light == null)
		{
			light = lightgo.AddComponent<Light>();
		}
		lightgo.transform.localRotation = Quaternion.Euler(5f, propRenderer.Light1Rotation, 0f);
		light.type = LightType.Directional;
		light.color = new Color(1f, 1f, 1f);
		light.cullingMask = 2048;
		light.shadows = LightShadows.Soft;
		light.shadowBias = 0.01f;
		light.shadowNormalBias = 0.01f;
		light.shadowStrength = 0.9f;
		light.intensity = lightIntensityScale(propRenderer.Light1IntensityMultiplier);
		return light;
	}

	public static bool RenderScreenshot(GameObject prefab, string filename, int width, int height, int SuperSampleSize = 1)
	{
		if (prefab == null)
		{
			Debug.Log("RenderScreenshot - prefab is null", prefab);
			return false;
		}
		PropRenderer propRenderer = null;
		PropRenderer propRenderer2 = prefab.GetComponent<PropRenderer>();
		if (propRenderer2 == null)
		{
			propRenderer = prefab.AddComponent<PropRenderer>();
			propRenderer2 = propRenderer;
		}
		propRenderer2.ScreenshotToDisk(filename, width, height, SuperSampleSize);
		if (propRenderer != null)
		{
			UnityEngine.Object.DestroyImmediate(propRenderer);
		}
		return true;
	}
}
