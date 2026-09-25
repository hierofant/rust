using System;
using System.Reflection;

namespace Facepunch.Extend;

public static class ReflectionExtensions
{
	public static bool HasAttribute(this MemberInfo method, Type attribute)
	{
		return method.GetCustomAttributes(attribute, inherit: true).Length != 0;
	}
}
