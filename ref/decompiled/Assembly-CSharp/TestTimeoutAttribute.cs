using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TestTimeoutAttribute : Attribute
{
	public float TimeoutSeconds { get; }

	public TestTimeoutAttribute(float timeoutSeconds)
	{
		TimeoutSeconds = Math.Max(0.1f, timeoutSeconds);
	}
}
