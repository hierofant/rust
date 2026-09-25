using UnityEngine;

public class TwoTierRadiationZoneBox : TwoTierRadiationZone
{
	public BoxCollider Inner;

	public BoxCollider Outer;

	public override void Apply(Bounds inner, Bounds outer)
	{
		base.Apply(inner, outer);
		Inner.center = inner.center;
		Inner.size = inner.size;
		Outer.center = outer.center;
		Outer.size = outer.size;
	}
}
