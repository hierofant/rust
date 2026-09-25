using ProtoBuf;
using UnityEngine;

public struct CrashTargeting
{
	public Vector3 center;

	public float radius;

	public Vector3 finalCrashPos;

	public float finalCrashRadius;

	public bool isDescending;

	public float cooldownEndTime;

	public float descentEndTime;

	public static CrashTargeting FromProto(ProtoBuf.SatelliteControlComputer msg, float now)
	{
		CrashTargeting result = default(CrashTargeting);
		result.center = msg.targetingCenter;
		result.radius = msg.targetingRadius;
		result.finalCrashPos = msg.finalCrashPos;
		result.finalCrashRadius = msg.finalCrashRadius;
		result.isDescending = msg.isDescending;
		result.cooldownEndTime = ((msg.cooldownRemaining > 0f) ? (now + msg.cooldownRemaining) : 0f);
		result.descentEndTime = ((msg.descentRemaining > 0f) ? (now + msg.descentRemaining) : 0f);
		return result;
	}
}
