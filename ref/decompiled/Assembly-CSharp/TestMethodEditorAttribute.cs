using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestMethodEditorAttribute : TestMethodAttribute
{
	public TestMethodEditorAttribute(params object[] testParameters)
		: base(testParameters)
	{
	}
}
