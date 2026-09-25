using UnityEngine;
using UnityEngine.Serialization;

public class OverrideAnimationSubSystem : AnimationSubSystem
{
	[FormerlySerializedAs("TelephoneIdleClip")]
	[SerializeField]
	private AnimationClip ClipToPlay;
}
