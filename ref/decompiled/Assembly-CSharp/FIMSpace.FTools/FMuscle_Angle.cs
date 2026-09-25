using UnityEngine;

namespace FIMSpace.FTools;

public class FMuscle_Angle : FMuscle_Motor
{
	protected override float GetDiff(float current, float desired)
	{
		return Mathf.DeltaAngle(current, desired);
	}
}
