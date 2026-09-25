using System;
using UnityEngine;

[Serializable]
public class CarSettings
{
	[Header("Vehicle Setup")]
	[Range(0f, 1f)]
	public float rollingResistance = 0.05f;

	[Range(0f, 1f)]
	public float antiRoll;

	public float baseAngularDamping;

	public bool canSleep = true;

	public bool kinematicWhileAsleep;

	[Header("Wheels")]
	public bool tankSteering;

	[Range(0f, 50f)]
	public float maxSteerAngle = 35f;

	public bool steeringAssist = true;

	[Range(0f, 1f)]
	public float steeringAssistRatio = 0.5f;

	public bool steeringLimit;

	[Range(0f, 50f)]
	public float minSteerLimitAngle = 6f;

	[Range(10f, 50f)]
	public float minSteerLimitSpeed = 30f;

	[Range(0f, 1f)]
	public float steerLimitLerpPenalty = 0.7f;

	[Range(0f, 1f)]
	public float rearWheelSteer = 1f;

	public float steerMinLerpSpeed = 75f;

	public float steerMaxLerpSpeed = 150f;

	public float steerReturnLerpSpeed = 200f;

	public bool retainLerpSpeed;

	[Header("Motor")]
	public float maxDriveSlip = 4f;

	public float driveForceToMaxSlip = 1000f;

	public float reversePercentSpeed = 0.3f;

	[Header("Brakes")]
	public float brakeForceMultiplier = 1000f;

	public bool disableHandbrakes;

	[Range(0.01f, 1f)]
	public float handbrakeGripMultiplier = 0.55f;

	[Header("Drift")]
	[Range(0.01f, 1f)]
	public float rearTraction = 0.7f;

	[Range(0.01f, 1f)]
	public float frontTraction = 0.7f;

	public float driftLerpSpeed = 40f;

	public float driftRecoverySpeed = 20f;

	[Header("Front/Rear Vehicle Balance")]
	[Range(0f, 1f)]
	public float handlingBias = 0.5f;
}
