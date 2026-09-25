using System;

[AttributeUsage(AttributeTargets.Property)]
public class SyncAttribute : Attribute
{
	public bool Pack { get; set; } = true;


	public bool Autosave { get; set; }

	public bool DontRunCallbacksOnLoad { get; set; }

	public bool RequireChange { get; set; } = true;


	public bool InvalidateCache { get; set; } = true;

}
