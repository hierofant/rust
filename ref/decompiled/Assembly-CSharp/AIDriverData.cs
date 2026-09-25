using System;
using UnityEngine;

public class AIDriverData : PrefabAttribute, IServerComponent
{
	public Vector3 FrontLocalPosition;

	public Vector3 BackLocalPosition;

	protected override Type GetIndexedType()
	{
		return typeof(AIDriverData);
	}
}
