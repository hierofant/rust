using System;
using UnityEngine;

public class DemoShotPlayback : MonoBehaviour
{
	[Flags]
	public enum TrackMask
	{
		None = 0,
		Position = 1,
		Fov = 4,
		Dof = 8,
		Parent = 0x10,
		RotationXTilt = 0x20,
		RotationYPan = 0x40,
		RotationZRoll = 0x80,
		All = 0xFD
	}
}
