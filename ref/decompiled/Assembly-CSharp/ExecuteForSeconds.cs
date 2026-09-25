using System;
using UnityEngine;

public class ExecuteForSeconds : CustomYieldInstruction
{
	private readonly float endTime;

	private readonly Action action;

	public override bool keepWaiting
	{
		get
		{
			if (Time.time >= endTime)
			{
				return false;
			}
			action?.Invoke();
			return true;
		}
	}

	public ExecuteForSeconds(Action action, float duration)
	{
		this.action = action;
		endTime = Time.time + duration;
	}
}
