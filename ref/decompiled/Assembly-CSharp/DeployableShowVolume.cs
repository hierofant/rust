using System;
using System.Collections.Generic;
using UnityEngine;

public class DeployableShowVolume : PrefabAttribute, IClientComponent
{
	public enum VolumeType
	{
		PrefabHemisphere,
		PrefabSphere,
		Sphere,
		Hemisphere,
		Cone
	}

	[Serializable]
	public class Volume
	{
		public VolumeType type;

		[Tooltip("Adding a sphere collider here will automatically fill out the fields below in the PreProcess step.")]
		public SphereCollider associatedCollider;

		public Vector3 localPosition;

		public Vector3 rotation;

		public float size;
	}

	public List<Volume> volumes = new List<Volume>();

	protected override Type GetIndexedType()
	{
		return typeof(DeployableShowVolume);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (volumes == null || volumes.Count == 0)
		{
			return;
		}
		foreach (Volume volume in volumes)
		{
			if (volume != null && !(volume.associatedCollider == null))
			{
				volume.size = volume.associatedCollider.radius * 2f;
				Transform transform = volume.associatedCollider.transform;
				volume.localPosition = transform.localPosition;
				volume.rotation = transform.localEulerAngles;
			}
		}
	}
}
