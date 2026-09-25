using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TestRepeatCountAttribute : Attribute
{
	public int RepeatCount { get; }

	public TestRepeatCountAttribute(int repeatCount)
	{
		RepeatCount = Math.Max(1, repeatCount);
	}
}
