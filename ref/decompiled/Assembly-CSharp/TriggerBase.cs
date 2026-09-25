using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Oxide.Core;
using Rust;
using UnityEngine;

public class TriggerBase : BaseMonoBehaviour
{
	[SerializeField]
	private LayerMask interestLayers;

	[NonSerialized]
	public HashSet<GameObject> contents;

	[NonSerialized]
	public HashSet<BaseEntity> entityContents;

	public Action<BaseNetworkable> OnEntityEnterTrigger;

	public Action<BaseNetworkable> OnEntityLeaveTrigger;

	private static bool _useExcludeLayers;

	private static readonly List<TriggerBase> _allTriggerBase = new List<TriggerBase>();

	public LayerMask InterestLayers
	{
		get
		{
			return interestLayers;
		}
		set
		{
			interestLayers = value;
			UpdateExcludeLayers();
		}
	}

	public bool HasAnyContents => !CollectionEx.IsNullOrEmpty(contents);

	public bool HasAnyEntityContents => !CollectionEx.IsNullOrEmpty(entityContents);

	[ClientVar(Help = "(Generated) When enabled, triggers use an exclude layer mask to filter out specific physics layers from trigger detection; toggling clears or sets all active triggers")]
	[ServerVar(Help = "(Generated) When enabled, triggers use an exclude layer mask to filter out specific physics layers from trigger detection; toggling clears or sets all active triggers")]
	public static bool UseExcludeLayers
	{
		get
		{
			return _useExcludeLayers;
		}
		set
		{
			if (_useExcludeLayers != value)
			{
				if (_useExcludeLayers)
				{
					ClearExcludeLayers();
				}
				if (!_useExcludeLayers)
				{
					SetExcludeLayers();
				}
			}
			_useExcludeLayers = value;
		}
	}

	protected bool IsBeingDisabled { get; private set; }

	protected virtual void Awake()
	{
		_allTriggerBase.Add(this);
		UpdateExcludeLayers();
	}

	private void UpdateExcludeLayers()
	{
		if (!UseExcludeLayers)
		{
			return;
		}
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		base.gameObject.GetComponentsInChildren(obj);
		int num = ~(int)interestLayers;
		foreach (Collider item in obj)
		{
			if (item.isTrigger)
			{
				item.excludeLayers = num;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Removes the exclude layer configuration from all registered TriggerBase instances, resetting them to detect all layers")]
	public static void ClearExcludeLayers()
	{
		Debug.Log($"Clearing ExcludeLayers for {_allTriggerBase.Count} triggers");
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		foreach (TriggerBase item in _allTriggerBase)
		{
			if (item == null)
			{
				continue;
			}
			item.gameObject.GetComponentsInChildren(obj);
			foreach (Collider item2 in obj)
			{
				if (item2.isTrigger)
				{
					item2.excludeLayers = 0;
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Applies the configured exclude layer mask to all registered TriggerBase instances to filter out unwanted layer detections")]
	public static void SetExcludeLayers()
	{
		Debug.Log($"Setting ExcludeLayers for {_allTriggerBase.Count} triggers");
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		foreach (TriggerBase item in _allTriggerBase)
		{
			if (item == null)
			{
				continue;
			}
			item.gameObject.GetComponentsInChildren(obj);
			int num = ~(int)item.interestLayers;
			foreach (Collider item2 in obj)
			{
				if (item2.isTrigger)
				{
					item2.excludeLayers = num;
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public virtual GameObject InterestedInObject(GameObject obj)
	{
		int num = 1 << obj.layer;
		if ((interestLayers.value & num) != num)
		{
			return null;
		}
		return obj;
	}

	internal virtual GameObject InterestedInObjectExitOnly(GameObject obj)
	{
		return InterestedInObject(obj);
	}

	internal virtual GameObject InterestedInObjectEnterOnly(GameObject obj)
	{
		return InterestedInObject(obj);
	}

	protected virtual void OnDisable()
	{
		if (!Rust.Application.isQuitting && contents != null)
		{
			IsBeingDisabled = true;
			GameObject[] array = contents.ToArray();
			foreach (GameObject targetObj in array)
			{
				OnTriggerExitImpl(targetObj);
			}
			IsBeingDisabled = false;
			contents = null;
		}
	}

	public virtual void OnEntityEnter(BaseEntity ent)
	{
		if (!(ent == null))
		{
			if (entityContents == null)
			{
				entityContents = new HashSet<BaseEntity>();
			}
			if (Interface.CallHook("OnEntityEnter", this, ent) == null)
			{
				entityContents.Add(ent);
				OnEntityEnterTrigger?.Invoke(ent);
			}
		}
	}

	public virtual void OnEntityLeave(BaseEntity ent)
	{
		if (entityContents != null && Interface.CallHook("OnEntityLeave", this, ent) == null)
		{
			entityContents.Remove(ent);
			OnEntityLeaveTrigger?.Invoke(ent);
		}
	}

	public virtual void OnObjectAdded(GameObject obj, Collider col)
	{
		if (!(obj == null))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
			if ((bool)baseEntity)
			{
				baseEntity.EnterTrigger(this);
				OnEntityEnter(baseEntity);
			}
		}
	}

	public virtual void OnObjectRemoved(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj, allowDestroyed: true);
		if (!baseEntity)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (GameObject content in contents)
		{
			if (content == null)
			{
				flag2 = true;
			}
			else if (GameObjectEx.ToBaseEntity(content, allowDestroyed: true) == baseEntity)
			{
				flag = true;
				break;
			}
		}
		if (flag2)
		{
			int num = contents.RemoveWhere((GameObject x) => x == null);
			Debug.LogWarning($"Trigger {ToString()} contained {num} null objects, cleaned up");
		}
		if (!flag)
		{
			baseEntity.LeaveTrigger(this);
			OnEntityLeave(baseEntity);
		}
	}

	public void RemoveInvalidEntities()
	{
		if (CollectionEx.IsNullOrEmpty(entityContents))
		{
			return;
		}
		Collider component = GetComponent<Collider>();
		if (component == null)
		{
			return;
		}
		Bounds bounds = component.bounds;
		bounds.Expand(1f);
		List<BaseEntity> obj = null;
		foreach (BaseEntity entityContent in entityContents)
		{
			if (entityContent == null)
			{
				if (Debugging.checktriggers)
				{
					Debug.LogWarning("Trigger " + ToString() + " contains destroyed entity.");
				}
				if (obj == null)
				{
					obj = Facepunch.Pool.Get<List<BaseEntity>>();
				}
				obj.Add(entityContent);
			}
			else if (!bounds.Contains(entityContent.ClosestPoint(base.transform.position)))
			{
				if (Debugging.checktriggers)
				{
					Debug.LogWarning("Trigger " + ToString() + " contains entity that is too far away: " + entityContent.ToString());
				}
				if (obj == null)
				{
					obj = Facepunch.Pool.Get<List<BaseEntity>>();
				}
				obj.Add(entityContent);
			}
		}
		if (obj == null)
		{
			return;
		}
		foreach (BaseEntity item in obj)
		{
			RemoveEntity(item);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public bool CheckEntity(BaseEntity ent)
	{
		if (ent == null)
		{
			return true;
		}
		Collider component = GetComponent<Collider>();
		if (component == null)
		{
			return true;
		}
		Bounds bounds = component.bounds;
		bounds.Expand(1f);
		return bounds.Contains(ent.ClosestPoint(base.transform.position));
	}

	public virtual void OnObjects()
	{
	}

	public virtual void OnEmpty()
	{
		contents = null;
		entityContents = null;
	}

	public void RemoveObject(GameObject obj)
	{
		if (!(obj == null))
		{
			Collider component = obj.GetComponent<Collider>();
			if (!(component == null))
			{
				OnTriggerExit(component);
			}
		}
	}

	public void RemoveEntity(BaseEntity ent)
	{
		if (this == null || contents == null || ent == null)
		{
			return;
		}
		List<GameObject> obj = Facepunch.Pool.Get<List<GameObject>>();
		foreach (GameObject content in contents)
		{
			if (content != null && GameObjectEx.ToBaseEntity(content, allowDestroyed: true) == ent)
			{
				obj.Add(content);
			}
		}
		foreach (GameObject item in obj)
		{
			OnTriggerExitImpl(item);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void OnTriggerEnter(Collider collider)
	{
		if (this == null || !base.enabled)
		{
			return;
		}
		using (TimeWarning.New("TriggerBase.OnTriggerEnter"))
		{
			GameObject gameObject = InterestedInObjectEnterOnly(collider.gameObject);
			if (gameObject == null)
			{
				return;
			}
			if (contents == null)
			{
				contents = new HashSet<GameObject>();
			}
			if (contents.Contains(gameObject))
			{
				return;
			}
			int count = contents.Count;
			contents.Add(gameObject);
			OnObjectAdded(gameObject, collider);
			if (count == 0 && contents != null && contents.Count == 1)
			{
				OnObjects();
			}
		}
		if (Debugging.checktriggers)
		{
			RemoveInvalidEntities();
		}
	}

	internal virtual bool SkipOnTriggerExit(Collider collider)
	{
		return false;
	}

	public void OnTriggerExit(Collider collider)
	{
		if (this == null || collider == null || SkipOnTriggerExit(collider))
		{
			return;
		}
		GameObject gameObject = InterestedInObjectExitOnly(collider.gameObject);
		if (!(gameObject == null))
		{
			OnTriggerExitImpl(gameObject);
			if (Debugging.checktriggers)
			{
				RemoveInvalidEntities();
			}
		}
	}

	public void OnTriggerExitImpl(GameObject targetObj)
	{
		if (contents != null && contents.Contains(targetObj))
		{
			contents.Remove(targetObj);
			OnObjectRemoved(targetObj);
			if (contents == null || contents.Count == 0)
			{
				OnEmpty();
			}
		}
	}
}
