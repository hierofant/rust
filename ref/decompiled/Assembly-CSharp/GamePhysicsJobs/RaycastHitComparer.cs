using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace GamePhysicsJobs;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[Preserve]
public struct RaycastHitComparer : IComparer<RaycastHit>
{
	public int Compare(RaycastHit x, RaycastHit y)
	{
		return x.distance.CompareTo(y.distance);
	}
}
