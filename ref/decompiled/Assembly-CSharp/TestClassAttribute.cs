using System;

[AttributeUsage(AttributeTargets.Class)]
public class TestClassAttribute : Attribute
{
	public string Category;

	public string GetCategory()
	{
		return Category ?? "Uncategorized";
	}
}
