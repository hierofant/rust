using FIMSpace.FLook;
using FIMSpace.FSpine;
using FIMSpace.FTail;
using Network;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NPCAnimController : EntityComponent<BaseEntity>, IClientComponent
{
	public enum AnimatorType
	{
		NoStrafe,
		Strafe
	}

	[SerializeField]
	private string animationsPrefix = "wolf_";

	[SerializeField]
	private string[] animationBlacklist = new string[4] { "prowl", "walk", "trot", "run" };

	[ClientVar(ClientAdmin = true)]
	public static float lookInterpSpeed = 3f;

	[ClientVar(ClientAdmin = true)]
	public static float spineInterpSpeed = 3f;

	[SerializeField]
	private AnimatorType animatorType;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private FSpineAnimator spineAnimator;

	[SerializeField]
	private FLookAnimator lookAnimator;

	[SerializeField]
	private TailAnimator2 tailAnimator;

	[SerializeField]
	private float maxPitchToConformToSlope = 30f;

	[SerializeField]
	private bool onlyConformPitchToSlope = true;

	[SerializeField]
	private float posInterpSpeed = 10f;

	[SerializeField]
	private float rotInterpSpeed = 2f;

	[SerializeField]
	private float snapVerticalOffset;

	[SerializeField]
	public bool enableLookAtDuringLocomotion = true;

	[SerializeField]
	public bool enableLookAtDuringProwl = true;

	[SerializeField]
	public bool canSwim;

	[SerializeField]
	private AnimationClip[] animationsWithLookAt;

	[SerializeField]
	private AnimationClip[] animationsWithSpineDeform;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCAnimController.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
