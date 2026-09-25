using System;
using UnityEngine;

public class RigidbodyInfo : PrefabAttribute, IClientComponent
{
	[ReadOnly]
	public float mass;

	[ReadOnly]
	public float drag;

	[ReadOnly]
	public float angularDrag;

	protected override Type GetIndexedType()
	{
		return typeof(RigidbodyInfo);
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		if (!FileSystem.IsBundled)
		{
			Rigidbody component = rootObj.GetComponent<Rigidbody>();
			if (component == null)
			{
				Debug.LogError(GetType().Name + ": RigidbodyInfo couldn't find a rigidbody on " + name + "! If a RealmedRemove is removing it, make sure this script is above the RealmedRemove script so that this gets processed first.");
				return;
			}
			mass = component.mass;
			drag = component.linearDamping;
			angularDrag = component.angularDamping;
		}
	}
}
