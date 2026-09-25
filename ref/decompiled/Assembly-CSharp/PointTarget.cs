using UnityEngine;

public sealed class PointTarget : IAITarget
{
	private readonly float _radius;

	public Vector3? Position { get; }

	public PointTarget(Vector3 pos, float radius = 3f)
	{
		Position = pos;
		_radius = Mathf.Max(0.1f, radius);
	}

	public bool IsValid(BoatAI boat)
	{
		return Position.HasValue;
	}

	public bool IsReached(BoatAI boat)
	{
		if (Position.HasValue)
		{
			return Vector3.Distance(boat.transform.position, Position.Value) <= _radius;
		}
		return false;
	}
}
