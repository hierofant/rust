using UnityEngine;

public struct PlayerOneShotHandle
{
	public double StartTime { get; private set; }

	public float Length { get; private set; }

	public bool Valid { get; private set; }

	public static PlayerOneShotHandle InvalidHandle
	{
		get
		{
			PlayerOneShotHandle result = default(PlayerOneShotHandle);
			result.Valid = false;
			return result;
		}
	}

	public readonly float GetNormalizedTime()
	{
		if (!Valid || Length <= Mathf.Epsilon)
		{
			return 0f;
		}
		return Mathf.Clamp01((float)((Time.timeAsDouble - StartTime) / (double)Length));
	}

	public static PlayerOneShotHandle Create(float length)
	{
		PlayerOneShotHandle result = default(PlayerOneShotHandle);
		result.StartTime = Time.timeAsDouble;
		result.Length = length;
		result.Valid = true;
		return result;
	}

	public static implicit operator bool(PlayerOneShotHandle handle)
	{
		return handle.Valid;
	}
}
