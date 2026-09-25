using System;
using UnityEngine;

public sealed class AssertTrueForDuration : CustomYieldInstruction
{
	private readonly float endTime;

	private readonly Func<bool> condition;

	public string Message { get; private set; }

	public bool Failed { get; private set; }

	public Exception Exception { get; private set; }

	public override bool keepWaiting
	{
		get
		{
			try
			{
				if (!condition())
				{
					Failed = true;
					return false;
				}
			}
			catch (Exception exception)
			{
				Exception = exception;
				Failed = true;
				return false;
			}
			return Time.time < endTime;
		}
	}

	public AssertTrueForDuration(Func<bool> condition, float durationSeconds, string message = null)
	{
		this.condition = condition;
		endTime = Time.time + durationSeconds;
		Message = message;
	}
}
