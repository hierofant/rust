using Rust.Interpolation;
using UnityEngine;

public struct ServersideMountedWeaponInterpolatorTarget : ISnapshot<ServersideMountedWeaponInterpolatorTarget>
{
	public float yaw;

	public float pitch;

	public float Time { get; set; }

	public ServersideMountedWeaponInterpolatorTarget(float time, float yaw, float pitch)
	{
		Time = time;
		this.yaw = yaw;
		this.pitch = pitch;
	}

	public void MatchValuesTo(ServersideMountedWeaponInterpolatorTarget entry)
	{
		yaw = entry.yaw;
		pitch = entry.pitch;
	}

	public void Lerp(ServersideMountedWeaponInterpolatorTarget prev, ServersideMountedWeaponInterpolatorTarget next, float delta)
	{
		yaw = Mathf.LerpAngle(prev.yaw, next.yaw, delta);
		pitch = Mathf.LerpAngle(prev.pitch, next.pitch, delta);
	}

	public ServersideMountedWeaponInterpolatorTarget GetNew()
	{
		return default(ServersideMountedWeaponInterpolatorTarget);
	}
}
