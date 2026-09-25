#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Prefabs.Deployable.Mortar;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Mortar : Cannon
{
	[Header("Mortar")]
	[SerializeField]
	private Vector2 minMaxDistance = new Vector2(0f, 200f);

	[SerializeField]
	private AnimationCurve distanceRandomnessCurve;

	[SerializeField]
	private AnimationCurve distanceRandomnessXCurve;

	[SerializeField]
	private AnimationCurve distanceRandomnessZCurve;

	[SerializeField]
	private Vector2 shotPitchRecoilMinMax = new Vector2(0f, 2.5f);

	[Header("Mortar Animation")]
	public ChildAnimatorSubSystem mortarAnim;

	[SerializeField]
	private AnimationCurve reloadHandIkWeightCurve;

	[SerializeField]
	private AnimationCurve firingHandIkWeightCurve;

	[SerializeField]
	private AnimationCurve reloadPitchBlendCurve;

	[SerializeField]
	private float remoteAimDirSmoothSpeed;

	[SerializeField]
	[Header("Condition")]
	private float conditionLossPerShot;

	[SerializeField]
	[Header("Recoil")]
	private AnimationClip recoilLowAnimation;

	[SerializeField]
	private AnimationClip recoilMediumAnimation;

	[SerializeField]
	private AnimationClip recoilHighAnimation;

	[SerializeField]
	private AnimationCurve recoilPitchCurve;

	[SerializeField]
	private float recoilPitchDuration;

	[SerializeField]
	[Header("Mortar Handle")]
	private Transform handleBone;

	[SerializeField]
	private AnimationCurve handleMinMaxRotation;

	[SerializeField]
	[Header("Display")]
	private MortarDisplay mortarDisplayPrefab;

	[ClientVar(ClientAdmin = true)]
	public static bool DebugDistanceUi;

	public override bool RunInLateUpdate
	{
		get
		{
			if (runInLateUpdate)
			{
				return base.isClient;
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Mortar.OnRpcMessage"))
		{
			if (rpc == 2658947749u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestLightFuse");
				}
				using (TimeWarning.New("RequestLightFuse"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2658947749u, "RequestLightFuse", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2658947749u, "RequestLightFuse", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2658947749u, "RequestLightFuse", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RequestLightFuse(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RequestLightFuse");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private float GetDesiredDistance()
	{
		return Mathf.Lerp(minMaxDistance.x, minMaxDistance.y, GetPitch01());
	}

	private float GetPitch01()
	{
		float aimingPitch = GetAimingPitch();
		float value = Mathf.Clamp(Mathf.DeltaAngle(0f, aimingPitch), pitchClamp.x, pitchClamp.y);
		return Mathf.InverseLerp(pitchClamp.x, pitchClamp.y, value);
	}

	private float GetAimingPitch()
	{
		if (aimDir == Vector3.zero)
		{
			return 0f;
		}
		return Quaternion.LookRotation(aimDir, base.transform.up).eulerAngles.x;
	}

	protected override bool TryGetPitchOverride(float basePitch, out float overridePitch, out float overrideWeight)
	{
		overridePitch = basePitch;
		overrideWeight = 0f;
		return false;
	}

	protected override bool ShouldApplyAimDir()
	{
		return true;
	}

	protected override bool UnableToStartReloadServer(BasePlayer player)
	{
		if (!base.UnableToStartReloadServer(player))
		{
			return !CanSeeFirePoint(player, 0.05f);
		}
		return true;
	}

	protected override void Server_OnReloadStarted()
	{
	}

	protected override void LoadAmmo(BasePlayer player)
	{
		if (!CanSeeFirePoint(player, 0.05f))
		{
			reloadProgress = 0f;
			return;
		}
		base.LoadAmmo(player);
		if (!IsLoaded())
		{
			reloadProgress = 0f;
			return;
		}
		Fire(player);
		ApplyServerPitchRecoil();
	}

	protected override void Fire(BasePlayer firingPlayer, float minSpeed = 100f)
	{
		Vector2 randomFireOffset = GetRandomFireOffset();
		float requiredVelocity = GetRequiredVelocity(randomFireOffset.y);
		if (magazine.ammoType.TryGetComponent<ItemModProjectile>(out var component) && FireProjectile(component.GetOverrideProjectile(this), FirePoint.position, FirePoint.forward, firingPlayer, 0.25f, requiredVelocity, out var projectile))
		{
			SERVER_OnProjectileFired(firingPlayer.Connection, firingPlayer);
			Hurt(conditionLossPerShot, DamageType.Generic, null, useProtection: false);
			if (projectile is MortarServerProjectile projectile2)
			{
				ApplyLateralCurve(projectile2, randomFireOffset.x, requiredVelocity);
			}
		}
	}

	private Vector2 GetRandomFireOffset()
	{
		float pitch = GetPitch01();
		float num = distanceRandomnessCurve.Evaluate(pitch);
		float x = UnityEngine.Random.Range(0f - num, num) * distanceRandomnessXCurve.Evaluate(pitch);
		float y = UnityEngine.Random.Range(0f - num, num) * distanceRandomnessZCurve.Evaluate(pitch);
		return new Vector2(x, y);
	}

	private void ApplyLateralCurve(MortarServerProjectile projectile, float lateralOffset, float forwardSpeed)
	{
		if (Mathf.Abs(lateralOffset) <= Mathf.Epsilon)
		{
			return;
		}
		Vector3 forward = FirePoint.forward;
		float magnitude = new Vector2(forward.x, forward.z).magnitude;
		float num = forwardSpeed * magnitude;
		if (!(num <= Mathf.Epsilon))
		{
			float num2 = GetDesiredDistance() / num;
			if (!(num2 <= Mathf.Epsilon))
			{
				Vector3 normalized = new Vector3(forward.x, 0f, forward.z).normalized;
				Vector3 vector = Vector3.Cross(Vector3.up, normalized);
				float num3 = 2f * lateralOffset / (num2 * num2);
				projectile.StartLateralCurve(vector * num3, num2);
			}
		}
	}

	public override void RequestLightFuse(RPCMessage msg)
	{
	}

	private float GetRequiredVelocity(float distanceOffset = 0f)
	{
		ServerProjectile serverProjectile = AmmoPrefab.Get()?.GetComponent<ServerProjectile>();
		Vector3 initialVelocity = ((serverProjectile != null) ? serverProjectile.initialVelocity : Vector3.zero);
		float gravityModifier = ((serverProjectile != null) ? serverProjectile.gravityModifier : 1f);
		float num = ((serverProjectile != null) ? (serverProjectile.speed + Vector3.Dot(serverProjectile.initialVelocity, FirePoint.forward)) : 0f);
		float num2 = CalculateDesiredLaunchVelocity(FirePoint.position, GetProjectileDestination(distanceOffset), FirePoint.forward, initialVelocity, gravityModifier);
		if (!float.IsFinite(num2))
		{
			return Mathf.Max(num, 0f);
		}
		return Mathf.Max(num2, num, 0f);
	}

	private Vector3 GetProjectileDestination(float distanceOffset = 0f)
	{
		return base.transform.position + base.transform.forward * (GetDesiredDistance() + distanceOffset);
	}

	private void ApplyServerPitchRecoil()
	{
		float num = UnityEngine.Random.Range(shotPitchRecoilMinMax.x, shotPitchRecoilMinMax.y);
		if (!(num <= Mathf.Epsilon))
		{
			Vector3 eulerAngles = Quaternion.LookRotation(aimDir, base.transform.up).eulerAngles;
			float num2 = Mathf.Clamp(Mathf.DeltaAngle(0f, eulerAngles.x) - num, pitchClamp.x, pitchClamp.y);
			if (num2 < 0f)
			{
				num2 += 360f;
			}
			eulerAngles.x = num2;
			aimDir = Quaternion.Euler(eulerAngles) * Vector3.forward;
			SendAimDirImmediate(force: true);
		}
	}

	private static float CalculateDesiredLaunchVelocity(Vector3 throwPos, Vector3 targetPos, Vector3 aimDir, Vector3 initialVelocity, float gravityModifier)
	{
		aimDir = aimDir.normalized;
		Vector3 vector = targetPos - throwPos;
		float magnitude = new Vector2(vector.x, vector.z).magnitude;
		float y = vector.y;
		float magnitude2 = new Vector2(aimDir.x, aimDir.z).magnitude;
		if (magnitude <= Mathf.Epsilon || magnitude2 <= Mathf.Epsilon)
		{
			return 0f;
		}
		float y2 = aimDir.y;
		float num = UnityEngine.Physics.gravity.y * gravityModifier;
		float num2 = Vector3.Dot(initialVelocity, aimDir);
		Vector3 vector2 = initialVelocity - aimDir * num2;
		float num3 = Vector2.Dot(rhs: new Vector2(aimDir.x, aimDir.z) / magnitude2, lhs: new Vector2(vector2.x, vector2.z));
		float y3 = vector2.y;
		float num4 = y2 / magnitude2;
		float num5 = y - magnitude * num4;
		float num6 = (0f - magnitude) * (y3 - num4 * num3);
		float num7 = -0.5f * num * magnitude * magnitude;
		float num8;
		if (Mathf.Abs(num5) <= Mathf.Epsilon)
		{
			if (Mathf.Abs(num6) <= Mathf.Epsilon)
			{
				return 0f;
			}
			num8 = (0f - num7) / num6;
		}
		else
		{
			float num9 = num6 * num6 - 4f * num5 * num7;
			if (num9 < 0f)
			{
				return 0f;
			}
			float num10 = Mathf.Sqrt(num9);
			float num11 = -0.5f * (num6 + Mathf.Sign(num6) * num10);
			float num12 = num11 / num5;
			float num13 = ((Mathf.Abs(num11) > Mathf.Epsilon) ? (num7 / num11) : float.PositiveInfinity);
			num8 = float.PositiveInfinity;
			if (num12 > 0f)
			{
				num8 = num12;
			}
			if (num13 > 0f && num13 < num8)
			{
				num8 = num13;
			}
			if (!float.IsFinite(num8))
			{
				return 0f;
			}
		}
		return (num8 - num3) / magnitude2;
	}
}
