using System;
using UnityEngine;

public class BakedShoreVectors : PrefabAttribute
{
	public ShoreVectorData ShoreVectorData;

	[Header("Only enable this for monuments")]
	public bool OnlyBakeShoreVectors;

	public float ShoreDistanceLimit = -1f;

	protected override Type GetIndexedType()
	{
		return typeof(BakedShoreVectors);
	}
}
