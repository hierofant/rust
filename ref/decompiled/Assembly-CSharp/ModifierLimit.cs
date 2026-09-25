using System;

[Serializable]
public class ModifierLimit
{
	public Modifier.ModifierSource source;

	public int MaxApplications;

	public float minValue;

	public float maxValue;
}
