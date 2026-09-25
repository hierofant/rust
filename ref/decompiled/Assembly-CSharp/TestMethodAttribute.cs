using System;
using JetBrains.Annotations;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestMethodAttribute : Attribute
{
	public object[] TestParameters { get; private set; }

	public TestMethodAttribute(params object[] testParameters)
	{
		TestParameters = testParameters;
	}
}
