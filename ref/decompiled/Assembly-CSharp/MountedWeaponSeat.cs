using Facepunch;
using UnityEngine;

public class MountedWeaponSeat : BaseVehicleSeat
{
	private const float MOUNTED_WEAPON_SEARCH_RANGE = 2f;

	private float _searchCount;

	private GameObject _mountedWeaponGameObject;

	private float _nextSearchTime;

	private MountedWeapon _owner;

	public GameObject MountedWeaponGameObject => _mountedWeaponGameObject;

	public MountedWeapon Owner
	{
		get
		{
			if (_owner == null)
			{
				_owner = FindMountedWeapon();
				if (_owner != null)
				{
					_owner.AssignSeat(this);
				}
				if (_owner == null)
				{
					if (Time.time < _nextSearchTime)
					{
						return null;
					}
					if (_searchCount > 30f)
					{
						return null;
					}
					_nextSearchTime = Time.time + 2f;
					_searchCount += 1f;
				}
			}
			return _owner;
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		Invoke(delegate
		{
			if (Owner != null)
			{
				Owner.AssignSeat(this);
				eyePositionOverride = Owner.GetCustomEyes();
				eyeCenterOverride = Owner.GetCustomEyes();
			}
		}, 0.05f);
	}

	public override Transform GetEyeOverride()
	{
		if (Owner == null)
		{
			return base.GetEyeOverride();
		}
		return Owner.GetCustomEyes();
	}

	private MountedWeapon FindMountedWeapon()
	{
		using PooledList<BaseEntity> pooledList = Pool.Get<PooledList<BaseEntity>>();
		Vis.Entities(base.transform.position, 2f, pooledList);
		BaseEntity baseEntity = GetParentEntity();
		foreach (BaseEntity item in pooledList)
		{
			if (!(item == null) && (!(baseEntity != null) || baseEntity.isServer == item.isServer) && item is MountedWeapon result)
			{
				_mountedWeaponGameObject = item.gameObject;
				return result;
			}
		}
		return null;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (Owner != null)
		{
			Owner.PlayerServerInput(inputState, player);
		}
	}

	public override void LightToggle(BasePlayer player)
	{
		if (!(Owner == null))
		{
			Owner.LightToggle(player);
		}
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if (Owner != null)
		{
			Owner.OnPlayerDismounted(player);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		if (Owner != null)
		{
			Owner.OnPlayerMounted();
		}
	}

	public override bool CanHoldItems()
	{
		return false;
	}

	public override Vector2 GetPitchClamp()
	{
		return pitchClamp;
	}

	public override Vector2 GetYawClamp()
	{
		return yawClamp;
	}
}
