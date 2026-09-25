using System;
using Unity.Collections;
using UnityEngine;

public struct WaterVolumeBurstData : IEquatable<WaterVolumeBurstData>, IDisposable
{
	public OBB bounds;

	public NativeArray<Matrix4x4> cutOffPlaneMatrices;

	public NativeArray<Pose> cutOffPlanePoses;

	public bool naturalSource;

	public bool Equals(WaterVolumeBurstData other)
	{
		if (bounds.Equals(other.bounds) && cutOffPlaneMatrices.Equals(other.cutOffPlaneMatrices))
		{
			return naturalSource == other.naturalSource;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is WaterVolumeBurstData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(bounds, cutOffPlaneMatrices, naturalSource);
	}

	public void Dispose()
	{
		NativeArrayEx.SafeDispose(ref cutOffPlaneMatrices);
		NativeArrayEx.SafeDispose(ref cutOffPlanePoses);
	}
}
