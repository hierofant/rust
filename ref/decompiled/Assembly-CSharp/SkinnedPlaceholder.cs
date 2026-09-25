using System.Collections.Generic;
using UnityEngine;

public class SkinnedPlaceholder : FacepunchBehaviour
{
	public List<GameObject> SkinnedObjects;

	public List<GameObject> UnskinnedObjects;

	public Animator Animator;

	public string AnimatorInactiveState;

	public string AnimatorActiveParameter;

	public float UpdateInterval = 5f;

	private bool showSkinnedRenderer;

	private int inactiveStateHash;
}
