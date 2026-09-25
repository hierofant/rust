using System;

namespace FIMSpace.Generating;

[Serializable]
public struct MinMaxF
{
	public float Min;

	public float Max;

	public MinMaxF(float min, float max)
	{
		Min = min;
		Max = max;
	}
}
