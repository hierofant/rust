using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestMethodAsyncAttribute : TestMethodAttribute
{
	public TestMethodAsyncAttribute(params object[] testParameters)
		: base(testParameters)
	{
	}
}
