using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TestParameterSourceAttribute : Attribute
{
	public string SourceName { get; private set; }

	public bool FilterForCI { get; set; }

	public TestParameterSourceAttribute(string sourceName)
	{
		SourceName = sourceName;
	}
}
