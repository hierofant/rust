using System;
using UnityEngine;

public class WaitUntilWithTimeout : CustomYieldInstruction
{
	private readonly float timeoutTime;

	private readonly Func<bool> condition;

	private readonly string message;

	private readonly Func<string> messageBuilder;

	public string Message
	{
		get
		{
			if (messageBuilder == null)
			{
				return message;
			}
			return messageBuilder();
		}
	}

	public bool TimedOut { get; private set; }

	public Exception Exception { get; private set; }

	public override bool keepWaiting
	{
		get
		{
			try
			{
				if (condition())
				{
					return false;
				}
			}
			catch (Exception exception)
			{
				Exception = exception;
				return false;
			}
			if (Time.time >= timeoutTime)
			{
				TimedOut = true;
				return false;
			}
			return true;
		}
	}

	public WaitUntilWithTimeout(Func<bool> condition, float timeoutSeconds, string message = null)
	{
		this.condition = condition;
		timeoutTime = Time.time + timeoutSeconds;
		this.message = message;
	}

	public WaitUntilWithTimeout(Func<bool> condition, float timeoutSeconds, Func<string> messageBuilder)
	{
		this.condition = condition;
		timeoutTime = Time.time + timeoutSeconds;
		this.messageBuilder = messageBuilder;
	}
}
