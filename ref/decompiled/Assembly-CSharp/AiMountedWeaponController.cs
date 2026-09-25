using ConVar;
using Rust.Safety;
using UnityEngine;

public class AiMountedWeaponController : FacepunchBehaviour
{
	[SerializeField]
	private float _maxAttackDistance = 200f;

	[SerializeField]
	private float _accuracy = 1f;

	[SerializeField]
	private MountedWeapon _mountedWeapon;

	[SerializeField]
	private bool _invertedForward;

	[SerializeField]
	private bool _flipPitch;

	[SerializeField]
	private Vector3 _offset;

	[ServerVar]
	public static float time_between_bursts = 3f;

	[ServerVar]
	public static float fire_rate = 0.125f;

	[ServerVar]
	public static float burst_length = 3f;

	private MountedWeaponSeat _cachedSeat;

	private BasePlayer _extraTarget;

	private float lastBurstTime = float.NegativeInfinity;

	private float lastFireTime = float.NegativeInfinity;

	public void SetExtraTarget(BasePlayer target)
	{
		_extraTarget = target;
	}

	private void Start()
	{
		InvokeRepeating(UpdateLoop, 0f, 0.05f);
	}

	private void UpdateLoop()
	{
		using (TimeWarning.New("AiMountedWeaponController.UpdateLoop"))
		{
			if (!AI.move || _mountedWeapon == null)
			{
				return;
			}
			if (_cachedSeat == null)
			{
				_cachedSeat = _mountedWeapon.GetSeat();
			}
			if (!(_cachedSeat == null) && _cachedSeat.GetMounted() is HumanNPC)
			{
				HumanNPC humanNPC = _cachedSeat.GetMounted() as HumanNPC;
				if (!(humanNPC == null) && !humanNPC.InSafeZone())
				{
					UpdateTurret(humanNPC);
				}
			}
		}
	}

	private void UpdateTurret(HumanNPC npc)
	{
		if (npc.Brain == null || npc.Brain.Navigator == null)
		{
			return;
		}
		BaseEntity baseEntity = npc.Brain.Navigator.FacingDirectionEntity;
		bool flag = false;
		if (baseEntity == null)
		{
			flag = true;
		}
		if (!(baseEntity is BasePlayer))
		{
			flag = true;
		}
		if (!(baseEntity as BasePlayer).IsValidAttackTarget())
		{
			flag = true;
		}
		if (flag)
		{
			if (!(_extraTarget != null) || !_extraTarget.IsValidAttackTarget())
			{
				return;
			}
			baseEntity = _extraTarget;
			if (_extraTarget == null)
			{
				return;
			}
		}
		if (baseEntity == null)
		{
			return;
		}
		Vector3 position = _mountedWeapon.PitchPivot.position;
		Vector3 vector = baseEntity.transform.position + _offset - position;
		if (!(Mathf.Abs(vector.magnitude) < 0.1f) && !(Mathf.Abs(vector.sqrMagnitude) > _maxAttackDistance * _maxAttackDistance))
		{
			if (!_mountedWeapon.IsReloading)
			{
				Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(_accuracy, vector.normalized);
				_mountedWeapon.AimAt(_mountedWeapon.PitchPivot.position, modifiedAimConeDirection, _flipPitch);
			}
			if (UnityEngine.Time.time - lastBurstTime > burst_length + time_between_bursts && GamePhysics.LineOfSight(_mountedWeapon.PitchPivot.position, baseEntity.transform.position, 1218519297, _mountedWeapon))
			{
				lastBurstTime = UnityEngine.Time.time;
			}
			if (UnityEngine.Time.time < lastBurstTime + burst_length && UnityEngine.Time.time - lastFireTime >= fire_rate && Vector3.Dot(_invertedForward ? (-_mountedWeapon.PitchPivot.forward) : _mountedWeapon.PitchPivot.forward, vector.normalized) >= 0.9f)
			{
				lastFireTime = UnityEngine.Time.time;
				_mountedWeapon.Fire(isAi: true);
			}
			_mountedWeapon.CheckAiReload();
		}
	}
}
