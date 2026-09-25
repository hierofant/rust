using System;
using UnityEngine;

public class SM_DisableHorseProceduralMotion : StateMachineBehaviour
{
	[Flags]
	public enum ProceduralMotionParts
	{
		None = 0,
		LegsAnimatorGlueing = 1,
		Head = 2,
		Spine = 4
	}

	public ProceduralMotionParts AffectedParts = ProceduralMotionParts.LegsAnimatorGlueing;

	[Min(0f)]
	public float fadeInDuration = 0.3f;

	[Min(0f)]
	public float fadeOutDuration = 0.3f;

	[Space]
	public bool waitForEndOfAnimation = true;

	[Min(0f)]
	public float enableBackAfter = 0.9f;
}
