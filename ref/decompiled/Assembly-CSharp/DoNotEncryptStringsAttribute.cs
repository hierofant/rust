using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, Inherited = false)]
public sealed class DoNotEncryptStringsAttribute : Attribute
{
}
