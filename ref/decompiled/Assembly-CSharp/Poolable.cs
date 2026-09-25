using System;
using System.Linq;
using ConVar;
using Facepunch;
using UnityEngine;

public class Poolable : MonoBehaviour, IClientComponent, IPrefabPostProcess
{
	public bool restoreHierarchy;

	[HideInInspector]
	public uint prefabID;

	[HideInInspector]
	public Behaviour[] behaviours;

	[HideInInspector]
	public Rigidbody[] rigidbodies;

	[HideInInspector]
	public Collider[] colliders;

	[HideInInspector]
	public LODGroup[] lodgroups;

	[HideInInspector]
	public Renderer[] renderers;

	[HideInInspector]
	public ParticleSystem[] particles;

	[HideInInspector]
	public bool[] behaviourStates;

	[HideInInspector]
	public bool[] rigidbodyStates;

	[HideInInspector]
	public bool[] colliderStates;

	[HideInInspector]
	public bool[] lodgroupStates;

	[HideInInspector]
	public bool[] rendererStates;

	[HideInInspector]
	public bool[] childActiveStates;

	public int ClientCount
	{
		get
		{
			if ((bool)GetComponent<CodeLock>())
			{
				return 200;
			}
			if (GetComponent<LootPanel>() != null)
			{
				return 1;
			}
			if (GetComponent<DecorComponent>() != null)
			{
				return 100;
			}
			if (GetComponent<BuildingBlock>() != null)
			{
				return 100;
			}
			if (GetComponent<Door>() != null)
			{
				if ((bool)GetComponent<Construction>())
				{
					return 100;
				}
				return 1;
			}
			if (GetComponent<Projectile>() != null)
			{
				return 100;
			}
			if (GetComponent<Gib>() != null)
			{
				return 100;
			}
			if (GetComponent<BaseVehicleModule>() != null)
			{
				return 8;
			}
			if (GetComponent<BaseVehicleMountPoint>() != null)
			{
				return 8;
			}
			if (GetComponent<BaseVehicle>() != null)
			{
				return 2;
			}
			if ((bool)GetComponent<UIMapVendingMachineMarker>())
			{
				return 25;
			}
			if ((bool)GetComponent<UIMapVendingMachineMarkerCluster>())
			{
				return 25;
			}
			if (GetComponent<CollectableEasterEgg>() != null)
			{
				return 50;
			}
			if ((bool)GetComponent<SlotMachinePayoutWidget>())
			{
				return 24;
			}
			return 1;
		}
	}

	public int ServerCount => 0;

	public void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!bundling && !(base.gameObject != rootObj))
		{
			Initialize(StringPool.Get(name));
		}
	}

	public void Initialize(uint id)
	{
		prefabID = id;
		behaviours = base.gameObject.GetComponentsInChildren(typeof(Behaviour), includeInactive: true).OfType<Behaviour>().ToArray();
		rigidbodies = base.gameObject.GetComponentsInChildren<Rigidbody>(includeInactive: true);
		colliders = base.gameObject.GetComponentsInChildren<Collider>(includeInactive: true);
		lodgroups = base.gameObject.GetComponentsInChildren<LODGroup>(includeInactive: true);
		renderers = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		particles = base.gameObject.GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		if (behaviours.Length == 0)
		{
			behaviours = Array.Empty<Behaviour>();
		}
		if (rigidbodies.Length == 0)
		{
			rigidbodies = Array.Empty<Rigidbody>();
		}
		if (colliders.Length == 0)
		{
			colliders = Array.Empty<Collider>();
		}
		if (lodgroups.Length == 0)
		{
			lodgroups = Array.Empty<LODGroup>();
		}
		if (renderers.Length == 0)
		{
			renderers = Array.Empty<Renderer>();
		}
		if (particles.Length == 0)
		{
			particles = Array.Empty<ParticleSystem>();
		}
		behaviourStates = ArrayEx.New<bool>(behaviours.Length);
		rigidbodyStates = ArrayEx.New<bool>(rigidbodies.Length);
		colliderStates = ArrayEx.New<bool>(colliders.Length);
		lodgroupStates = ArrayEx.New<bool>(lodgroups.Length);
		rendererStates = ArrayEx.New<bool>(renderers.Length);
		CaptureComponentStates();
		childActiveStates = new bool[CountTransforms(base.transform)];
		CaptureActiveStates(base.transform, childActiveStates, 0);
	}

	private void CaptureComponentStates()
	{
		for (int i = 0; i < behaviours.Length; i++)
		{
			behaviourStates[i] = behaviours[i].enabled;
		}
		for (int j = 0; j < renderers.Length; j++)
		{
			rendererStates[j] = renderers[j].enabled;
		}
		for (int k = 0; k < lodgroups.Length; k++)
		{
			lodgroupStates[k] = lodgroups[k].enabled;
		}
		for (int l = 0; l < colliders.Length; l++)
		{
			colliderStates[l] = colliders[l].enabled;
		}
		for (int m = 0; m < rigidbodies.Length; m++)
		{
			rigidbodyStates[m] = rigidbodies[m].isKinematic;
		}
	}

	private static int CountTransforms(Transform root)
	{
		int num = 1;
		for (int i = 0; i < root.childCount; i++)
		{
			num += CountTransforms(root.GetChild(i));
		}
		return num;
	}

	private static int CaptureActiveStates(Transform root, bool[] states, int index)
	{
		states[index++] = root.gameObject.activeSelf;
		for (int i = 0; i < root.childCount; i++)
		{
			index = CaptureActiveStates(root.GetChild(i), states, index);
		}
		return index;
	}

	private static int RestoreActiveStates(Transform root, bool[] states, int index)
	{
		if (index < 0 || index >= states.Length)
		{
			return -1;
		}
		if (index > 0 && root.gameObject.activeSelf != states[index])
		{
			root.gameObject.SetActive(states[index]);
		}
		index++;
		for (int i = 0; i < root.childCount; i++)
		{
			index = RestoreActiveStates(root.GetChild(i), states, index);
			if (index < 0)
			{
				return -1;
			}
		}
		return index;
	}

	private void RestoreChildActiveStates()
	{
		if (childActiveStates != null && RestoreActiveStates(base.transform, childActiveStates, 0) != childActiveStates.Length)
		{
			Debug.LogError("Pooled prefab changed its game object hierarchy at runtime, which pooling can't restore: " + base.name + " (prefab has " + childActiveStates.Length + " transforms, instance has " + CountTransforms(base.transform) + ")", this);
		}
	}

	private void RestoreHierarchy(GameObject prefab)
	{
		if (!(prefab == null))
		{
			RestoreHierarchy(base.transform, prefab.transform);
		}
	}

	private static void RestoreHierarchy(Transform instance, Transform prefab)
	{
		int childCount = prefab.childCount;
		for (int num = instance.childCount - 1; num >= childCount; num--)
		{
			Transform child = instance.GetChild(num);
			OnParentDestroyingEx.SendOnParentDestroying(child.gameObject);
			if (child.parent == instance)
			{
				child.SetParent(null, worldPositionStays: true);
			}
		}
		if (instance.childCount >= childCount)
		{
			for (int i = 0; i < childCount; i++)
			{
				Transform child2 = instance.GetChild(i);
				Transform child3 = prefab.GetChild(i);
				RestorePose(child2, child3);
				RestoreHierarchy(child2, child3);
			}
		}
	}

	private static void RestorePose(Transform instance, Transform prefab)
	{
		if (!(instance is RectTransform))
		{
			prefab.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
			if (instance.localPosition != localPosition || instance.localRotation != localRotation)
			{
				instance.SetLocalPositionAndRotation(localPosition, localRotation);
			}
			if (instance.localScale != prefab.localScale)
			{
				instance.localScale = prefab.localScale;
			}
		}
	}

	public void EnterPool(GameObject prefab)
	{
		if (base.transform.parent != null)
		{
			base.transform.SetParent(null, worldPositionStays: false);
		}
		if (restoreHierarchy)
		{
			RestoreHierarchy(prefab);
		}
		if (ConVar.Pool.mode <= 1)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			SetBehaviourEnabled(state: false);
			SetComponentEnabled(state: false);
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		CancelAllInvokes();
	}

	private void CancelAllInvokes()
	{
		if (behaviours == null || behaviours.Length == 0)
		{
			return;
		}
		using PooledHashSet<Behaviour> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<Behaviour>>();
		for (int i = 0; i < behaviours.Length; i++)
		{
			Behaviour behaviour = behaviours[i];
			if ((bool)behaviour)
			{
				pooledHashSet.Add(behaviour);
			}
		}
		if (pooledHashSet.Count != 0)
		{
			if ((bool)SingletonComponent<InvokeHandler>.Instance)
			{
				SingletonComponent<InvokeHandler>.Instance.CancelInvokes(pooledHashSet);
			}
			if ((bool)SingletonComponent<InvokeHandlerFixedTime>.Instance)
			{
				SingletonComponent<InvokeHandlerFixedTime>.Instance.CancelInvokes(pooledHashSet);
			}
			if ((bool)SingletonComponent<InvokeHandlerUnscaledTime>.Instance)
			{
				SingletonComponent<InvokeHandlerUnscaledTime>.Instance.CancelInvokes(pooledHashSet);
			}
		}
	}

	public void LeavePool()
	{
		if (restoreHierarchy)
		{
			RestoreChildActiveStates();
		}
		if (ConVar.Pool.mode > 1)
		{
			SetComponentEnabled(state: true);
		}
	}

	public void SetBehaviourEnabled(bool state)
	{
		try
		{
			if (!state)
			{
				for (int i = 0; i < behaviours.Length; i++)
				{
					behaviours[i].enabled = false;
				}
				for (int j = 0; j < particles.Length; j++)
				{
					ParticleSystem obj = particles[j];
					obj.Stop();
					obj.Clear();
				}
				return;
			}
			for (int k = 0; k < particles.Length; k++)
			{
				ParticleSystem particleSystem = particles[k];
				if (particleSystem.playOnAwake)
				{
					particleSystem.Play();
				}
			}
			for (int l = 0; l < behaviours.Length; l++)
			{
				behaviours[l].enabled = behaviourStates[l];
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Pooling error: " + base.name + " (" + ex.Message + ")");
		}
	}

	private static bool CanToggleCollider(Collider collider)
	{
		if (collider is WheelCollider wheelCollider)
		{
			return wheelCollider.attachedRigidbody != null;
		}
		return true;
	}

	public void SetComponentEnabled(bool state)
	{
		try
		{
			if (!state)
			{
				for (int i = 0; i < renderers.Length; i++)
				{
					renderers[i].enabled = false;
				}
				for (int j = 0; j < lodgroups.Length; j++)
				{
					lodgroups[j].enabled = false;
				}
				for (int k = 0; k < colliders.Length; k++)
				{
					Collider collider = colliders[k];
					if (CanToggleCollider(collider))
					{
						collider.enabled = false;
					}
				}
				for (int l = 0; l < rigidbodies.Length; l++)
				{
					Rigidbody obj = rigidbodies[l];
					obj.isKinematic = true;
					obj.detectCollisions = false;
				}
				return;
			}
			for (int m = 0; m < renderers.Length; m++)
			{
				renderers[m].enabled = rendererStates[m];
			}
			for (int n = 0; n < lodgroups.Length; n++)
			{
				lodgroups[n].enabled = lodgroupStates[n];
			}
			for (int num = 0; num < colliders.Length; num++)
			{
				Collider collider2 = colliders[num];
				if (CanToggleCollider(collider2))
				{
					collider2.enabled = colliderStates[num];
				}
			}
			for (int num2 = 0; num2 < rigidbodies.Length; num2++)
			{
				Rigidbody obj2 = rigidbodies[num2];
				obj2.isKinematic = rigidbodyStates[num2];
				obj2.detectCollisions = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Pooling error: " + base.name + " (" + ex.Message + ")");
		}
	}
}
