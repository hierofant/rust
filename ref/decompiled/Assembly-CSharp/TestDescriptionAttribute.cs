using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TestDescriptionAttribute : Attribute
{
	public string Description { get; set; }

	public TestDescriptionAttribute(string description)
	{
		Description = description;
	}
}
