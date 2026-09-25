using System.Collections.Generic;
using UnityEngine;

public class CaveNetworkGroupLayerOverride : MonoBehaviour, IServerComponent
{
	private struct OverrideData
	{
		public Vector3 Position;

		public float RadiusSquared;
	}

	public float Radius = 10f;

	private static readonly HashSet<CaveNetworkGroupLayerOverride> _all = new HashSet<CaveNetworkGroupLayerOverride>();

	private static bool _isDirty;

	private static readonly List<OverrideData> _overrides = new List<OverrideData>();

	protected void OnEnable()
	{
		_all.Add(this);
		_isDirty = true;
	}

	protected void OnDisable()
	{
		_all.Remove(this);
		_isDirty = true;
	}

	public static bool Includes(Vector3 pos)
	{
		if (_isDirty)
		{
			_overrides.Clear();
			foreach (CaveNetworkGroupLayerOverride item in _all)
			{
				_overrides.Add(new OverrideData
				{
					Position = item.transform.position,
					RadiusSquared = item.Radius * item.Radius
				});
			}
		}
		foreach (OverrideData @override in _overrides)
		{
			if ((@override.Position - pos).sqrMagnitude <= @override.RadiusSquared)
			{
				return true;
			}
		}
		return false;
	}

	protected void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, Radius);
	}
}
