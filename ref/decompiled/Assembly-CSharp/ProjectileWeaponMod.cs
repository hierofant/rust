#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ProjectileWeaponMod : BaseEntity
{
	public enum SilencerType
	{
		Military,
		OilFilter,
		SodaCan
	}

	[Serializable]
	public struct Modifier
	{
		public bool enabled;

		[Tooltip("1 means no change. 0.5 is half.")]
		public float scalar;

		[Tooltip("Added after the scalar is applied.")]
		public float offset;
	}

	public float ConditionLossMultiplier = 1f;

	[Header("AttackEffectAdditive")]
	public GameObjectRef additiveEffect;

	[Header("Silencer")]
	public GameObjectRef defaultSilencerEffect;

	public bool isSilencer;

	public SilencerType silencerType;

	private static TimeSince lastADSTime;

	private static TimeSince lastToastTime;

	public static Translate.Phrase ToggleZoomToastPhrase = new Translate.Phrase("toast.toggle_zoom", "Press [PageUp] and [PageDown] to toggle scope zoom level");

	[Header("Weapon Basics")]
	public Modifier repeatDelay;

	public Modifier projectileVelocity;

	public Modifier projectileDamage;

	public Modifier projectileDistance;

	[Header("Recoil")]
	public Modifier aimsway;

	public Modifier aimswaySpeed;

	public Modifier recoil;

	[Header("Aim Cone")]
	public Modifier sightAimCone;

	public Modifier hipAimCone;

	[Header("Light Effects")]
	public bool isLight;

	[Header("MuzzleBrake")]
	public bool isMuzzleBrake;

	[Header("MuzzleBoost")]
	public bool isMuzzleBoost;

	[Header("Scope")]
	public bool isScope;

	public float zoomAmountDisplayOnly;

	[Header("Magazine")]
	public Modifier magazineCapacity;

	[Header("Toggling")]
	public bool needsOnForEffects;

	[Header("Burst")]
	public int burstCount = -1;

	public float timeBetweenBursts;

	[Header("Zoom")]
	public float[] zoomLevels;

	public GameObjectRef fovChangeEffect;

	[Header("Targeting")]
	public bool allowPings;

	private int serverZoomLevel;

	private bool hasZoomBeenInit;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectDamage = (ProjectileWeaponMod x) => x.projectileDamage;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectDistance = (ProjectileWeaponMod x) => x.projectileDistance;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectVelocity = (ProjectileWeaponMod x) => x.projectileVelocity;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectMagCap = (ProjectileWeaponMod x) => x.magazineCapacity;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectSightAimCone = (ProjectileWeaponMod x) => x.sightAimCone;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectHipAimCone = (ProjectileWeaponMod x) => x.hipAimCone;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectRepeatDelay = (ProjectileWeaponMod x) => x.repeatDelay;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectRecoil = (ProjectileWeaponMod x) => x.recoil;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectAimSway = (ProjectileWeaponMod x) => x.aimsway;

	public static readonly Func<ProjectileWeaponMod, Modifier> SelectAimSwaySpeed = (ProjectileWeaponMod x) => x.aimswaySpeed;

	public static readonly Func<Modifier, float> SelectOffset = (Modifier x) => x.offset;

	public static readonly Func<Modifier, float> SelectScalar = (Modifier x) => x.scalar;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ProjectileWeaponMod.OnRpcMessage"))
		{
			if (rpc == 3713130066u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetZoomLevel");
				}
				using (TimeWarning.New("SetZoomLevel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3713130066u, "SetZoomLevel", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							int zoomLevel = msg.read.Int32();
							SetZoomLevel(zoomLevel);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetZoomLevel");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: true);
		}
		base.ServerInit();
	}

	public override void PostServerLoad()
	{
		base.limitNetworking = HasFlag(Flags.Disabled);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.projectileWeaponMod = Facepunch.Pool.Get<GunWeaponMod>();
		info.msg.projectileWeaponMod.zoomLevel = serverZoomLevel;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void SetZoomLevel(int zoomLevel)
	{
		serverZoomLevel = zoomLevel;
		SendNetworkUpdate();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.projectileWeaponMod != null)
		{
			serverZoomLevel = info.msg.projectileWeaponMod.zoomLevel;
		}
	}

	public static float Mult(BaseEntity parentEnt, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, bool bypassModToggles = false)
	{
		if (parentEnt.children == null)
		{
			return 1f;
		}
		return MulModValues(parentEnt.children, selector_modifier, selector_value, bypassModToggles);
	}

	public static float Mult(List<ProjectileWeaponMod> mods, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, bool bypassModToggles = false)
	{
		if (mods == null)
		{
			return 1f;
		}
		return MulModValues(mods, selector_modifier, selector_value, bypassModToggles);
	}

	public static float Sum(BaseEntity parentEnt, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, bool bypassModToggles = false)
	{
		if (parentEnt.children == null)
		{
			return 0f;
		}
		return SumModValues(parentEnt.children, selector_modifier, selector_value, bypassModToggles);
	}

	public static float Sum(List<ProjectileWeaponMod> mods, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, bool bypassModToggles = false)
	{
		if (mods == null)
		{
			return 0f;
		}
		return SumModValues(mods, selector_modifier, selector_value, bypassModToggles);
	}

	public static float Average(BaseEntity parentEnt, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, float def, bool bypassModToggles = false)
	{
		if (parentEnt.children == null)
		{
			return def;
		}
		return AvgModValues(parentEnt.children, selector_modifier, selector_value, def, bypassModToggles);
	}

	public static float Average(List<ProjectileWeaponMod> mods, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, float def, bool bypassModToggles = false)
	{
		if (mods == null)
		{
			return def;
		}
		return AvgModValues(mods, selector_modifier, selector_value, def, bypassModToggles);
	}

	public static float Max(BaseEntity parentEnt, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, float def, bool bypassModToggles = false)
	{
		if (parentEnt.children == null)
		{
			return def;
		}
		return MaxModValues(parentEnt.children, selector_modifier, selector_value, def, bypassModToggles);
	}

	public static float Max(List<ProjectileWeaponMod> mods, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, float def, bool bypassModToggles = false)
	{
		if (mods == null)
		{
			return def;
		}
		return MaxModValues(mods, selector_modifier, selector_value, def, bypassModToggles);
	}

	public static float Min(BaseEntity parentEnt, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, float def, bool bypassModToggles = false)
	{
		if (parentEnt.children == null)
		{
			return def;
		}
		return MinModValues(parentEnt.children, selector_modifier, selector_value, def, bypassModToggles);
	}

	public static float Min(List<ProjectileWeaponMod> mods, Func<ProjectileWeaponMod, Modifier> selector_modifier, Func<Modifier, float> selector_value, float def, bool bypassModToggles = false)
	{
		if (mods == null)
		{
			return def;
		}
		return MinModValues(mods, selector_modifier, selector_value, def, bypassModToggles);
	}

	public static float MulModValues<T>(List<T> mods, Func<ProjectileWeaponMod, Modifier> selectModifier, Func<Modifier, float> selectValue, bool bypassModToggles = false) where T : BaseEntity
	{
		float num = 1f;
		foreach (T mod in mods)
		{
			ProjectileWeaponMod projectileWeaponMod = mod as ProjectileWeaponMod;
			if (IsValid(projectileWeaponMod, bypassModToggles))
			{
				Modifier arg = selectModifier(projectileWeaponMod);
				if (arg.enabled)
				{
					num *= selectValue(arg);
				}
			}
		}
		return num;
	}

	public static float SumModValues<T>(List<T> mods, Func<ProjectileWeaponMod, Modifier> selectModifier, Func<Modifier, float> selectValue, bool bypassModToggles = false) where T : BaseEntity
	{
		float num = 0f;
		foreach (T mod in mods)
		{
			ProjectileWeaponMod projectileWeaponMod = mod as ProjectileWeaponMod;
			if (IsValid(projectileWeaponMod, bypassModToggles))
			{
				Modifier arg = selectModifier(projectileWeaponMod);
				if (arg.enabled)
				{
					num += selectValue(arg);
				}
			}
		}
		return num;
	}

	public static float AvgModValues<T>(List<T> mods, Func<ProjectileWeaponMod, Modifier> selectModifier, Func<Modifier, float> selectValue, float def, bool bypassModToggles = false) where T : BaseEntity
	{
		float num = 0f;
		int num2 = 0;
		foreach (T mod in mods)
		{
			ProjectileWeaponMod projectileWeaponMod = mod as ProjectileWeaponMod;
			if (IsValid(projectileWeaponMod, bypassModToggles))
			{
				Modifier arg = selectModifier(projectileWeaponMod);
				if (arg.enabled)
				{
					num += selectValue(arg);
					num2++;
				}
			}
		}
		if (num2 <= 0)
		{
			return def;
		}
		return num / (float)num2;
	}

	public static float MaxModValues<T>(List<T> mods, Func<ProjectileWeaponMod, Modifier> selectModifier, Func<Modifier, float> selectValue, float def, bool bypassModToggles = false) where T : BaseEntity
	{
		float num = float.MinValue;
		foreach (T mod in mods)
		{
			ProjectileWeaponMod projectileWeaponMod = mod as ProjectileWeaponMod;
			if (IsValid(projectileWeaponMod, bypassModToggles))
			{
				Modifier arg = selectModifier(projectileWeaponMod);
				if (arg.enabled)
				{
					num = Mathf.Max(num, selectValue(arg));
				}
			}
		}
		if (num == float.MinValue)
		{
			return def;
		}
		return num;
	}

	public static float MinModValues<T>(List<T> mods, Func<ProjectileWeaponMod, Modifier> selectModifier, Func<Modifier, float> selectValue, float def, bool bypassModToggles = false) where T : BaseEntity
	{
		float num = float.MaxValue;
		foreach (T mod in mods)
		{
			ProjectileWeaponMod projectileWeaponMod = mod as ProjectileWeaponMod;
			if (IsValid(projectileWeaponMod, bypassModToggles))
			{
				Modifier arg = selectModifier(projectileWeaponMod);
				if (arg.enabled)
				{
					num = Mathf.Min(num, selectValue(arg));
				}
			}
		}
		if (num == float.MaxValue)
		{
			return def;
		}
		return num;
	}

	public static bool IsValid(ProjectileWeaponMod mod, bool bypassModToggles = false)
	{
		if (mod != null)
		{
			if (!(!mod.needsOnForEffects || bypassModToggles))
			{
				return mod.HasFlag(Flags.On);
			}
			return true;
		}
		return false;
	}

	public static bool HasBrokenWeaponMod(BaseEntity parentEnt)
	{
		if (parentEnt.children == null)
		{
			return false;
		}
		foreach (BaseEntity child in parentEnt.children)
		{
			if (BaseNetworkableEx.Is<ProjectileWeaponMod>(child, out var castedUnityObject) && castedUnityObject.IsBroken())
			{
				return true;
			}
		}
		return false;
	}
}
