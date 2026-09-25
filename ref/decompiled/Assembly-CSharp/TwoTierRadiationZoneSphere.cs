using UnityEngine;

public class TwoTierRadiationZoneSphere : TwoTierRadiationZone
{
	public SphereCollider Inner;

	public SphereCollider Outer;

	public override void Apply(float inner, float outer)
	{
		base.Apply(inner, outer);
		Inner.radius = inner;
		Outer.radius = outer;
	}
}
