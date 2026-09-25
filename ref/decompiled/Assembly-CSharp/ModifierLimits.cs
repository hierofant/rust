using System;
using System.Collections.Generic;

[Serializable]
public class ModifierLimits
{
	public Modifier.ModifierType type;

	public float minValue;

	public float maxValue;

	public List<ModifierLimit> limits;
}
