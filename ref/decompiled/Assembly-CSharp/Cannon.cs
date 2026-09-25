#define UNITY_ASSERTIONS
using System;
using ConVar;
using JetBrains.Annotations;
using Network;
using Oxide.Core;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class Cannon : BallistaGun
{
	public GameObjectRef AmmoPrefab;

	public Transform FirePoint;

	public Transform backBlockTargetHeight;

	public Transform backBlockVisual;

	public Transform backBlockHandle;

	public float handleSpinAmount = 1f;

	public OneShotAnimationSubSystem recoilOneShot;

	public ChildAnimatorSubSystem reloadSubSystem;

	public GameObjectRef fireEffect;

	public ParticleSystem[] additionalFireEffects;

	public float reloadAimDirHeight;

	public float fuseLightTime = 3f;

	public ParticleSystem fuseEffect;

	public Renderer fuseBurnRenderer;

	public Transform middleGroundCheck;

	public ProtectionProperties mountedProtection;

	[Header("Player Camera Animation")]
	public Transform cameraAnimation;

	public Animator cameraAnimationController;

	private Vector3 defaultLocalEyePosition;

	private Quaternion defaultLocalEyeRotation;

	public float disableLegsVerticalAngle;

	[Header("Cannon ball reload settings")]
	public Vector2 reloadProgressCannonBallVisibilityRange;

	public GameObject cannonBallVisualPrefab;

	public float3x2 reloadCannonBallPositionScaleOffset;

	[Header("Fuse")]
	public GameObject fuseVisual;

	public AnimationCurve fuseBurnCurve;

	public ParticleSystem fusePoof;

	private GameObject cannonBallVisualInstance;

	public bool allowOnLand;

	private const Signal Signal_LightFuse = Signal.Deploy;

	private const Flags Flag_FuseLit = Flags.Reserved6;

	private static readonly int AlphaCutoff = Shader.PropertyToID("_Cutoff");

	private MaterialPropertyBlock fuseBurnAlphaPropBlock;

	public Transform[] wheelTransforms;

	[ServerVar(Help = "Allows mounting cannons outside of boats for testing.")]
	public static bool ignore_boat_mount_restrictions = false;

	protected virtual bool HasReloadHandle => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Cannon.OnRpcMessage"))
		{
			if (rpc == 2658947749u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
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

	public bool HasAmmo()
	{
		return HasFlag(Flags.On);
	}

	protected override bool HasGround(Transform checkTransform)
	{
		Vector3 vector = checkTransform.up * 0.6f;
		Vector3 start = middleGroundCheck.position + vector;
		Vector3 end = checkTransform.position + vector;
		bool num = !UnityEngine.Physics.Linecast(start, end, 1503731969);
		bool? flag = null;
		if (num)
		{
			return flag ?? base.HasGround(checkTransform);
		}
		return false;
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if (!(parentPlayerBoat != null) || !parentPlayerBoat.IsDying)
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		mountedProtection.Scale(info.damageTypes);
	}

	protected override void Server_OnReloadStarted()
	{
		base.Server_OnReloadStarted();
		aimDir.y = reloadAimDirHeight;
		SendAimDirImmediate(force: true);
	}

	protected override bool UnableToStartReloadServer(BasePlayer player)
	{
		if (GetAmmoFromPlayerInventory(player) == null)
		{
			return true;
		}
		return false;
	}

	[UsedImplicitly]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	public virtual void RequestLightFuse(RPCMessage msg)
	{
		if (!IsFireRPCInvalid(msg, msg.player, out var _, out var _) && CanLightFuse())
		{
			LightFuse(msg);
		}
	}

	protected override bool IsFireRPCInvalid(RPCMessage msg, BasePlayer player, out ItemDefinition ammoItem, out ItemModProjectile itemModProjectile)
	{
		if (!base.IsFireRPCInvalid(msg, player, out ammoItem, out itemModProjectile))
		{
			return !CanSeeFirePoint(player, 0.05f);
		}
		return true;
	}

	private bool CanLightFuse()
	{
		object obj = Interface.CallHook("CanLightCannonFuse", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return !HasFlag(Flags.Reserved6);
	}

	private void LightFuse(RPCMessage msg)
	{
		SignalBroadcast(Signal.Deploy);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: true);
		}
		Invoke(delegate
		{
			using FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			if (IsFireRPCInvalid(msg, msg.player, out var _, out var _))
			{
				flagsUpdateScope2.Set(Flags.Reserved6, b: false);
			}
			else
			{
				Fire(msg.player);
				flagsUpdateScope2.Set(Flags.Reserved6, b: false);
				SignalBroadcast(Signal.Attack);
			}
		}, fuseLightTime);
	}

	protected virtual void Fire(BasePlayer firingPlayer, float minSpeed = 100f)
	{
		if (FireProjectile(AmmoPrefab, FirePoint.position, FirePoint.forward, firingPlayer, 0.25f, minSpeed, out var _))
		{
			SERVER_OnProjectileFired(firingPlayer.Connection, firingPlayer);
		}
	}

	[ServerVar]
	public static string AdminFire(ConsoleSystem.Arg arg)
	{
		if (ArgEx.Player(arg) == null)
		{
			return "Player is null";
		}
		Cannon[] array = Util.FindAll<Cannon>();
		foreach (Cannon cannon in array)
		{
			if (cannon.IsLoaded())
			{
				cannon.Fire(ArgEx.Player(arg));
			}
		}
		return $"Fired {array.Length} cannons.";
	}

	public override void StopReload()
	{
		base.StopReload();
		if (!Mathf.Approximately(reloadProgress, 1f))
		{
			reloadProgress = 0f;
		}
		SendNetworkUpdateImmediate();
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfFinishedPlayerBoat(this);
		}
		return false;
	}

	public override bool OwnerIsWaterlogged()
	{
		return WaterFactor() >= 1f;
	}

	public override float WaterFactorForPlayer(BasePlayer player, out WaterLevel.WaterInfo info)
	{
		return player.GetUnmountedWaterFactor(out info);
	}
}
