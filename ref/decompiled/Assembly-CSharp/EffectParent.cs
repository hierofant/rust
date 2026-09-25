using UnityEngine;

public class EffectParent : EntityComponent<BaseEntity>, IClientComponent
{
	public GameObject effect;

	[Tooltip("If true the effect isn't our direct child and is in another hierarchy - USE WITH CAUTION")]
	public bool separatedEffect;

	[Header("USING COLLIDERS? - COLLIDER SAFETY")]
	[Tooltip("If true the effect will be forced to the safe layer that doesn't interact with player colliders")]
	public bool forceSafePlayerCollisionLayer;
}
