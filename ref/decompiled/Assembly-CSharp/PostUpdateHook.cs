using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PostUpdateHook : MonoBehaviour
{
	public static Action OnUpdate;

	public static Action OnLateUpdate;

	public static Action OnFixedUpdate;

	public static Action EndOfFrame;

	private void Update()
	{
		OnUpdate?.Invoke();
	}

	private void LateUpdate()
	{
		OnLateUpdate?.Invoke();
	}

	private void FixedUpdate()
	{
		OnFixedUpdate?.Invoke();
	}

	private void Start()
	{
		StartCoroutine(EndOfFrameRoutine());
	}

	private IEnumerator EndOfFrameRoutine()
	{
		while (Application.isPlaying)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			EndOfFrame?.Invoke();
		}
	}
}
