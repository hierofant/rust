using UnityEngine;

public sealed class PlayerTarget : IAITarget
{
	public bool StayClose;

	private readonly BasePlayer _player;

	private readonly float _acquiredAt;

	private readonly Transform _boat;

	private const float CLOSE_DIST = 15f;

	private const float FAR_DIST = 55f;

	private const float LOS_DROP_TIME = 10f;

	private float _lastLosSeen;

	public BasePlayer Player => _player;

	public Vector3? Position
	{
		get
		{
			if (_player == null)
			{
				return null;
			}
			Vector3 vector = ((!_player.HasParent()) ? _player.transform.position : _player.GetParentEntity().transform.position);
			Vector3 normalized = (_boat.position - vector).normalized;
			if (StayClose)
			{
				vector += normalized * 15f;
			}
			else
			{
				vector += normalized * 55f;
			}
			return vector;
		}
	}

	public PlayerTarget(BasePlayer player, float acquiredAt, Transform boat)
	{
		_player = player;
		_acquiredAt = acquiredAt;
		_boat = boat;
		_lastLosSeen = Time.time;
	}

	public bool IsValid(BoatAI self)
	{
		if (!Position.HasValue)
		{
			return false;
		}
		BasePlayer player = _player;
		if (!self.IsPlayerTargetValid(player))
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log("Leaving seek state - not a valid target anymore.");
			}
			return false;
		}
		if (!self.IsPlayerInRange(player, self.SearchRange * 1.5f))
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log("Leaving seek state - not in range anymore.");
			}
			return false;
		}
		if (Mathf.Abs(player.transform.position.y - self.transform.position.y) > 30f)
		{
			return false;
		}
		if (player.transform.position.y < 12f)
		{
			return false;
		}
		if (Vector3Ex.Distance2D(player.transform.position, self.transform.position) >= self.SearchRange)
		{
			return false;
		}
		if (self.HasLineOfSightToPlayer(_player))
		{
			_lastLosSeen = Time.time;
		}
		else if (Time.time - _lastLosSeen > 10f)
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log($"Leaving seek state - lost line of sight for {10f}s.");
			}
			return false;
		}
		return true;
	}

	public bool IsReached(BoatAI self)
	{
		return false;
	}
}
