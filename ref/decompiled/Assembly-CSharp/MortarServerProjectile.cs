using UnityEngine;

public class MortarServerProjectile : ServerProjectile
{
	private Vector3 lateralAcceleration;

	private float curveTimeRemaining;

	public void StartLateralCurve(Vector3 acceleration, float duration)
	{
		lateralAcceleration = acceleration;
		curveTimeRemaining = duration;
	}

	public override Vector3 GetVelocityStep()
	{
		Vector3 velocityStep = base.GetVelocityStep();
		if (curveTimeRemaining > 0f)
		{
			float num = Time.fixedDeltaTime * Time.timeScale;
			velocityStep += lateralAcceleration * num;
			curveTimeRemaining -= num;
		}
		return velocityStep;
	}
}
