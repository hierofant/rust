using UnityEngine;

public sealed class TransformTarget : IAITarget
{
	private readonly Transform _t;

	private readonly float _radius;

	public Transform Transform => _t;

	public Vector3? Position
	{
		get
		{
			if (!_t)
			{
				return null;
			}
			return _t.position;
		}
	}

	public TransformTarget(Transform t, float radius = 0.5f)
	{
		_t = t;
		_radius = Mathf.Max(0.1f, radius);
	}

	public bool IsValid(BoatAI self)
	{
		if (_t != null)
		{
			return Position.HasValue;
		}
		return false;
	}

	public bool IsReached(BoatAI self)
	{
		if (_t != null)
		{
			return Vector3Ex.Distance2D(self.transform.position, _t.position) <= _radius;
		}
		return false;
	}
}
