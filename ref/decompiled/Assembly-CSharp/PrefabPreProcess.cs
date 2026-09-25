using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.BurstCloth;
using Rust.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using VLB;

public class PrefabPreProcess : IPrefabProcessor
{
	public static Type[] clientsideOnlyTypes = new Type[41]
	{
		typeof(IClientComponent),
		typeof(SkeletonSkinLod),
		typeof(SkeletonSkin),
		typeof(ImageEffectLayer),
		typeof(NGSS_Directional),
		typeof(VolumetricDustParticles),
		typeof(VolumetricLightBeam),
		typeof(Cloth),
		typeof(TextMeshPro),
		typeof(MeshFilter),
		typeof(Renderer),
		typeof(AudioLowPassFilter),
		typeof(AudioSource),
		typeof(AudioListener),
		typeof(ParticleSystemRenderer),
		typeof(ParticleSystem),
		typeof(ParticleEmitFromParentObject),
		typeof(ImpostorShadows),
		typeof(Light),
		typeof(LODGroup),
		typeof(Animator),
		typeof(AnimationEvents),
		typeof(PlayerVoiceSpeaker),
		typeof(VoiceProcessor),
		typeof(PlayerVoiceRecorder),
		typeof(ParticleScaler),
		typeof(PostEffectsBase),
		typeof(TOD_ImageEffect),
		typeof(TOD_Scattering),
		typeof(TOD_Rays),
		typeof(UnityEngine.Tree),
		typeof(Projector),
		typeof(HttpImage),
		typeof(EventTrigger),
		typeof(InputSystemUIInputModule),
		typeof(UIBehaviour),
		typeof(Canvas),
		typeof(CanvasRenderer),
		typeof(CanvasGroup),
		typeof(GraphicRaycaster),
		typeof(BurstClothConstraint)
	};

	public static Type[] serversideOnlyTypes = new Type[5]
	{
		typeof(IServerComponent),
		typeof(NavMeshLink),
		typeof(NavMeshSurface),
		typeof(NavMeshObstacle),
		typeof(NavMeshModifierVolume)
	};

	public bool isClientside;

	public bool isServerside;

	public bool isBundling;

	public Dictionary<string, GameObject> prefabList = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

	private GameObject parentObject;

	public List<Component> destroyList = new List<Component>();

	public List<GameObject> cleanupList = new List<GameObject>();

	private List<GameObject> deleteList = new List<GameObject>();

	public PrefabPreProcess(bool clientside, bool serverside, bool bundling = false)
	{
		isClientside = clientside;
		isServerside = serverside;
		isBundling = bundling;
	}

	public GameObject Find(string strPrefab)
	{
		if (prefabList.TryGetValue(strPrefab, out var value))
		{
			if (value == null)
			{
				prefabList.Remove(strPrefab);
				return null;
			}
			return value;
		}
		return null;
	}

	public bool NeedsProcessing(GameObject go, PreProcessPrefabOptions options)
	{
		if (go.CompareTag("NoPreProcessing"))
		{
			return false;
		}
		if (options.PreProcess && HasComponents<IPrefabPreProcess>(go.transform))
		{
			return true;
		}
		if (options.PostProcess && HasComponents<IPrefabPostProcess>(go.transform))
		{
			return true;
		}
		if (options.StripComponents && HasComponents<IEditorComponent>(go.transform))
		{
			return true;
		}
		if (!isClientside)
		{
			if (options.StripComponents && clientsideOnlyTypes.Any((Type type) => HasComponents(go.transform, type)))
			{
				return true;
			}
			if (options.StripComponents && HasComponents<IClientComponentEx>(go.transform))
			{
				return true;
			}
		}
		if (!isServerside)
		{
			if (options.StripComponents && serversideOnlyTypes.Any((Type type) => HasComponents(go.transform, type)))
			{
				return true;
			}
			if (options.StripComponents && HasComponents<IServerComponentEx>(go.transform))
			{
				return true;
			}
		}
		return false;
	}

	public void ProcessObject(string name, GameObject go, PreProcessPrefabOptions options)
	{
		StringPool.Get(name);
		StripEmptyChildren component;
		bool flag = go.TryGetComponent<StripEmptyChildren>(out component) && Render.IsInstancingEnabled;
		if (options.StripComponents)
		{
			if (!isClientside)
			{
				Type[] array = clientsideOnlyTypes;
				foreach (Type t in array)
				{
					DestroyComponents(t, go, isClientside, isServerside);
				}
				foreach (IClientComponentEx item in FindIComponents<IClientComponentEx>(go.transform))
				{
					item.PreClientComponentCull(this);
				}
			}
			if (!isServerside)
			{
				Type[] array = serversideOnlyTypes;
				foreach (Type t2 in array)
				{
					DestroyComponents(t2, go, isClientside, isServerside);
				}
				foreach (IServerComponentEx item2 in FindIComponents<IServerComponentEx>(go.transform))
				{
					item2.PreServerComponentCull(this);
				}
			}
			DestroyComponents(typeof(IEditorComponent), go, isClientside, isServerside);
		}
		if (options.ResetLocalTransform)
		{
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
		}
		List<Transform> list = FindComponents<Transform>(go.transform);
		list.Reverse();
		if (options.UpdateMeshCooking)
		{
			MeshColliderCookingOptions meshColliderCookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
			MeshColliderCookingOptions cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices | MeshColliderCookingOptions.UseFastMidphase;
			MeshColliderCookingOptions meshColliderCookingOptions2 = (MeshColliderCookingOptions)(-1);
			foreach (MeshCollider item3 in FindComponents<MeshCollider>(go.transform))
			{
				if (item3.cookingOptions == meshColliderCookingOptions || item3.cookingOptions == meshColliderCookingOptions2)
				{
					item3.cookingOptions = cookingOptions;
				}
			}
		}
		if (options.DisableInstancingOnParticleSystems)
		{
			foreach (ParticleSystemRenderer item4 in FindComponents<ParticleSystemRenderer>(go.transform))
			{
				if (!(item4 == null))
				{
					item4.enableGPUInstancing = false;
				}
			}
		}
		if (options.PreProcess)
		{
			foreach (IPrefabPreProcess item5 in FindIComponents<IPrefabPreProcess>(go.transform))
			{
				if (!isBundling || item5.CanRunDuringBundling)
				{
					item5.PreProcess(this, go, name, isServerside, isClientside, isBundling);
					MarkPropertiesDirty((UnityEngine.Object)item5);
				}
			}
			if (isClientside && !isServerside)
			{
				RemoveChildEntities(go);
			}
		}
		if (!isServerside)
		{
			DisableWheelColliders(go);
		}
		if (options.StripEmptyChildren)
		{
			foreach (Transform item6 in list)
			{
				if (!item6 || !item6.gameObject)
				{
					continue;
				}
				if (isServerside && item6.gameObject.CompareTag("Server Cull"))
				{
					RemoveComponents(item6.gameObject);
					NominateForDeletion(item6.gameObject);
				}
				if (isClientside)
				{
					bool num = item6.gameObject.CompareTag("Client Cull");
					BaseEntity component2;
					bool flag2 = item6 != go.transform && item6.gameObject.TryGetComponent<BaseEntity>(out component2);
					if (num || flag2)
					{
						RemoveComponents(item6.gameObject);
						NominateForDeletion(item6.gameObject);
					}
					else if (flag)
					{
						NominateForDeletion(item6.gameObject);
					}
				}
			}
		}
		RunCleanupQueue(go);
		if (!options.PostProcess)
		{
			return;
		}
		foreach (IPrefabPostProcess item7 in FindIComponents<IPrefabPostProcess>(go.transform))
		{
			item7.PostProcess(this, go, name, isServerside, isClientside, isBundling);
		}
	}

	public void Process(string name, GameObject go, bool forceInPlace = false)
	{
		PreProcessPrefabOptions assetSceneRuntime = PreProcessPrefabOptions.AssetSceneRuntime;
		GameObject gameObject = go;
		if (UnityEngine.Application.isPlaying && !gameObject.CompareTag("NoPreProcessing"))
		{
			bool num = PrefabNeedsCopy(gameObject);
			bool flag = NeedsProcessing(gameObject, assetSceneRuntime);
			bool flag2 = num && flag;
			if (!forceInPlace && flag2)
			{
				GameObject obj2;
				Transform parent = (TryGetHierarchyGroup(out obj2) ? obj2.transform : null);
				go = Instantiate.GameObject(gameObject, parent);
				go.name = gameObject.name;
			}
			if (flag)
			{
				ProcessObject(name, go, assetSceneRuntime);
			}
			if (!forceInPlace)
			{
				AddPrefab(name, go);
			}
		}
		static bool PrefabNeedsCopy(GameObject obj)
		{
			if (obj.TryGetComponent<Wearable>(out var component) && !component.disableRigStripping)
			{
				return true;
			}
			return false;
		}
	}

	public void Invalidate(string name)
	{
		if (prefabList.TryGetValue(name, out var value))
		{
			prefabList.Remove(name);
			if (value != null)
			{
				UnityEngine.Object.DestroyImmediate(value, allowDestroyingAssets: true);
			}
		}
	}

	public void InvalidateAll()
	{
		foreach (var (_, gameObject2) in prefabList)
		{
			if (gameObject2 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject2, allowDestroyingAssets: true);
			}
		}
		prefabList.Clear();
	}

	private void RemoveChildEntities(GameObject go)
	{
		if (go.GetComponent<BaseNetworkable>() == null)
		{
			return;
		}
		BaseNetworkable[] componentsInChildren = go.GetComponentsInChildren<BaseNetworkable>();
		foreach (BaseNetworkable baseNetworkable in componentsInChildren)
		{
			if (!(baseNetworkable == null) && !(baseNetworkable.gameObject == go))
			{
				UnityEngine.Object.DestroyImmediate(baseNetworkable.gameObject, allowDestroyingAssets: true);
			}
		}
	}

	private void DisableWheelColliders(GameObject go)
	{
		List<WheelCollider> obj = Facepunch.Pool.Get<List<WheelCollider>>();
		FindComponents(go.transform, obj);
		foreach (WheelCollider item in obj)
		{
			if (!(item.GetComponentInParent<Rigidbody>(includeInactive: true) != null))
			{
				item.enabled = false;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private bool TryGetHierarchyGroup(out GameObject obj)
	{
		if (isBundling || !UnityEngine.Application.isPlaying)
		{
			obj = null;
			return false;
		}
		if (parentObject == null)
		{
			string name = ((isClientside && isServerside) ? "PrefabPreProcess - Generic" : (isServerside ? "PrefabPreProcess - Server" : "PrefabPreProcess - Client"));
			parentObject = new GameObject(name);
			parentObject.SetActive(value: false);
			UnityEngine.Object.DontDestroyOnLoad(parentObject);
		}
		obj = parentObject;
		return true;
	}

	public void AddPrefab(string name, GameObject go)
	{
		go.SetActive(value: false);
		prefabList.Add(name, go);
	}

	private void DestroyComponents(Type t, GameObject go, bool client, bool server)
	{
		List<Component> list = new List<Component>();
		FindComponents(go.transform, list, t);
		list.Reverse();
		foreach (Component item in list)
		{
			if (!item.TryGetComponent<RealmedRemove>(out var component) || component.ShouldDelete(item, client, server))
			{
				if (!item.gameObject.CompareTag("persist"))
				{
					NominateForDeletion(item.gameObject);
				}
				UnityEngine.Object.DestroyImmediate(item, allowDestroyingAssets: true);
			}
		}
	}

	private bool ShouldExclude(Transform transform)
	{
		if (transform.TryGetComponent<BaseEntity>(out var _))
		{
			return true;
		}
		return false;
	}

	private void GatherExcludedTransf(Transform root, HashSet<Transform> excludeSet)
	{
		List<BaseEntity> list = new List<BaseEntity>();
		root.GetComponentsInChildren(includeInactive: true, list);
		int i = 0;
		if (list.Count > 0 && list[0].transform == root)
		{
			i = 1;
		}
		for (; i < list.Count; i++)
		{
			ExcludeChildHierarchy(list[i].transform, excludeSet);
		}
		static void ExcludeChildHierarchy(Transform transf, HashSet<Transform> set)
		{
			set.Add(transf);
			foreach (Transform item in transf)
			{
				ExcludeChildHierarchy(item, set);
			}
		}
	}

	private bool HasComponents<T>(Transform transform)
	{
		if (transform.TryGetComponent<T>(out var _))
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item) && HasComponents<T>(item))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasComponents(Transform transform, Type t)
	{
		if (transform.TryGetComponent(t, out var _))
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item) && HasComponents(item, t))
			{
				return true;
			}
		}
		return false;
	}

	public List<T> FindComponents<T>(Transform transform) where T : Component
	{
		List<T> list = new List<T>();
		FindComponents(transform, list);
		return list;
	}

	public void FindComponents<T>(Transform transform, List<T> list) where T : Component
	{
		List<T> list2 = new List<T>();
		transform.GetComponentsInChildren(includeInactive: true, list2);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		GatherExcludedTransf(transform, hashSet);
		try
		{
			foreach (T item in list2)
			{
				Transform transform2 = item.transform;
				if (!hashSet.Contains(transform2))
				{
					list.Add(item);
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public List<T> FindIComponents<T>(Transform transform)
	{
		List<T> list = new List<T>();
		FindIComponents(transform, list);
		return list;
	}

	public void FindIComponents<T>(Transform transform, List<T> list)
	{
		list.AddRange(transform.GetComponents<T>());
		foreach (Transform item in transform)
		{
			if (!ShouldExclude(item))
			{
				FindIComponents(item, list);
			}
		}
	}

	public List<Component> FindComponents(Transform transform, Type t)
	{
		List<Component> list = new List<Component>();
		FindComponents(transform, list, t);
		return list;
	}

	public void FindComponents(Transform transform, List<Component> list, Type t)
	{
		Component[] componentsInChildren = transform.GetComponentsInChildren(t, includeInactive: true);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		GatherExcludedTransf(transform, hashSet);
		Component[] array = componentsInChildren;
		foreach (Component component in array)
		{
			Transform transform2 = component.transform;
			if (!hashSet.Contains(transform2))
			{
				list.Add(component);
			}
		}
	}

	public void RemoveComponent(Component c)
	{
		if (!(c == null))
		{
			destroyList.Add(c);
		}
	}

	public void RemoveComponents(GameObject gameObj)
	{
		Component[] components = gameObj.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (!(component is Transform))
			{
				destroyList.Add(component);
			}
		}
	}

	public void NominateForDeletion(GameObject gameObj)
	{
		cleanupList.Add(gameObj);
	}

	public void DeleteGameObject(GameObject obj)
	{
		if (!(obj == null))
		{
			deleteList.Add(obj);
		}
	}

	public void MarkPropertiesDirty(UnityEngine.Object obj)
	{
	}

	public void RunCleanupQueue(GameObject rootGo)
	{
		foreach (Component destroy in destroyList)
		{
			UnityEngine.Object.DestroyImmediate(destroy, allowDestroyingAssets: true);
		}
		destroyList.Clear();
		foreach (GameObject delete in deleteList)
		{
			UnityEngine.Object.DestroyImmediate(delete, allowDestroyingAssets: true);
		}
		deleteList.Clear();
		foreach (GameObject cleanup in cleanupList)
		{
			if ((object)cleanup != rootGo)
			{
				DoCleanup(cleanup);
			}
		}
		cleanupList.Clear();
	}

	public void DoCleanup(GameObject go)
	{
		if (!(go == null) && go.GetComponentsInChildren<Component>(includeInactive: true).Length <= 1)
		{
			UnityEngine.Object.DestroyImmediate(go, allowDestroyingAssets: true);
		}
	}
}
