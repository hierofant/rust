using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentVolume : MonoBehaviour, IPrefabPreProcess
{
	public enum VolumeShape
	{
		Cube,
		Sphere,
		Capsule
	}

	private static readonly Vector3[] volumeCorners = new Vector3[8]
	{
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f)
	};

	[InspectorFlags]
	public EnvironmentType Type = EnvironmentType.Underground;

	[InspectorFlags]
	public NetworkGroupType NetworkType;

	public Vector3 Center = Vector3.zero;

	public Vector3 Size = Vector3.one;

	[NonSerialized]
	public float4x4 VolumeTransformation;

	[NonSerialized]
	public float4x4 VolumeTransformationInverse;

	[NonSerialized]
	public float3 VolumePosition;

	[NonSerialized]
	public Bounds VolumeBounds;

	[field: Tooltip("Controls the falloff amount of the positive axes of spatially aware volumes.")]
	[field: SerializeField]
	public Vector3 FalloffPositive { get; private set; } = Vector3.zero;


	[field: Tooltip("Controls the falloff amount of the negative axes of spatially aware volumes.")]
	[field: SerializeField]
	public Vector3 FalloffNegative { get; private set; } = Vector3.zero;


	[field: SerializeField]
	public VolumeShape SpatialVolumeShape { get; private set; }

	public float AmbientMultiplier { get; private set; }

	public float ReflectionMultiplier { get; private set; }

	public float CombinedMultiplier { get; private set; }

	public bool NoSunlight { get; private set; }

	public bool PropertiesCached { get; private set; }

	[field: SerializeField]
	public bool IsDynamic { get; private set; }

	public Collider trigger { get; private set; }

	public bool IsSpatialVolume => (Type & EnvironmentType.SpatiallyAware) != 0;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	private void OnValidate()
	{
		PropertiesCached = false;
		UpdateVolumeTransformationAndBounds();
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside && IsSpatialVolume && !(base.gameObject == null) && GetComponent<EnvironmentVolumeLOD>() == null)
		{
			base.gameObject.AddComponent<EnvironmentVolumeLOD>();
		}
	}

	protected void Awake()
	{
		UpdateTrigger();
	}

	protected void OnEnable()
	{
		if ((bool)trigger && !trigger.enabled)
		{
			trigger.enabled = true;
		}
		UpdateVolumeTransformationAndBounds();
		if (IsDynamic && (bool)SingletonComponent<EnvironmentManager>.Instance)
		{
			SingletonComponent<EnvironmentManager>.Instance.RegisterDynamicVolume(this);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UpdateVolumeTransformationAndBounds()
	{
		float3 size = Size;
		float3 center = Center;
		float4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		bool isCapsule = IsSpatialVolume && SpatialVolumeShape == VolumeShape.Capsule;
		EnvironmentVolumeMath.UpdateVolumeTransformationAndBoundsBurst(in size, in center, in localToWorldMatrix, in isCapsule, out VolumeTransformation, out VolumeTransformationInverse, out VolumePosition, out VolumeBounds);
	}

	protected void OnDisable()
	{
		if ((bool)trigger && trigger.enabled)
		{
			trigger.enabled = false;
		}
		if (IsDynamic && (bool)SingletonComponent<EnvironmentManager>.Instance)
		{
			SingletonComponent<EnvironmentManager>.Instance.UnregisterDynamicVolume(this);
		}
	}

	public void CacheVolumeProperties(EnvironmentVolumePropertiesCollection properties)
	{
		if (!PropertiesCached)
		{
			PropertiesCached = true;
			NoSunlight = (Type & EnvironmentType.NoSunlight) != 0 || (Type & EnvironmentType.TrainTunnels) != 0;
			CombinedMultiplier = AmbientMultiplier * ReflectionMultiplier;
		}
	}

	public void UpdateTrigger()
	{
		if (!trigger)
		{
			trigger = base.gameObject.GetComponent<Collider>();
		}
		if (!trigger)
		{
			trigger = base.gameObject.AddComponent<BoxCollider>();
		}
		trigger.isTrigger = true;
		BoxCollider boxCollider = trigger as BoxCollider;
		if ((bool)boxCollider)
		{
			boxCollider.center = Center;
			boxCollider.size = Size;
		}
	}
}
