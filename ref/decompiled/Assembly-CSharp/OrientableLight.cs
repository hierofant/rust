#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class OrientableLight : SimpleLight
{
	public Transform pivotOrigin;

	public Transform yaw;

	public Transform pitch;

	public bool pivotAutoAdjust = true;

	[Space]
	public Vector2 pitchClamp = new Vector2(-50f, 50f);

	public Vector2 yawClamp = new Vector2(-50f, 50f);

	[Space]
	public float serverLerpSpeed = 15f;

	public float clientLerpSpeed = 10f;

	[Space]
	public GameObjectRef reorientEffect;

	public const Flags Flag_FacingDown = Flags.Reserved18;

	private float pitchAmount;

	private float yawAmount;

	private float lastPitchAmount;

	private float lastYawAmount;

	public static Translate.Phrase TipPhrase = new Translate.Phrase("gametip_spotlight", "Use a hammer to adjust the spotlight direction");

	private bool IsFacingDown => HasFlag(Flags.Reserved18);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("OrientableLight.OnRpcMessage"))
		{
			if (rpc == 3353964129u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_SetDir");
				}
				using (TimeWarning.New("SERVER_SetDir"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3353964129u, "SERVER_SetDir", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3353964129u, "SERVER_SetDir", this, player, 3f))
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
							SERVER_SetDir(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_SetDir");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	public void UpdateRotation(float delta)
	{
		if (base.isServer)
		{
			Quaternion to = Quaternion.Euler(pitchAmount, 0f, 0f);
			Quaternion to2 = Quaternion.Euler(0f, yawAmount, 0f);
			pitch.transform.localRotation = Mathx.Lerp(pitch.transform.localRotation, to, serverLerpSpeed, delta);
			yaw.transform.localRotation = Mathx.Lerp(yaw.transform.localRotation, to2, serverLerpSpeed, delta);
		}
	}

	private void ResetRotation()
	{
		lastPitchAmount = 0f;
		lastYawAmount = 0f;
		yaw.transform.localRotation = Quaternion.identity;
		pitch.transform.localRotation = Quaternion.identity;
		SetPivot();
	}

	private void SetPivot()
	{
		if (pivotAutoAdjust)
		{
			if (IsFacingDown)
			{
				pivotOrigin.localRotation = Quaternion.Euler(90f, 0f, 180f);
			}
			else
			{
				pivotOrigin.localRotation = Quaternion.identity;
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		bool b = Mathf.Abs(Vector3.Dot(base.transform.forward, Vector3.up)) > 0.9f;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved18, b);
		}
		SetPivot();
	}

	public void ServerTick()
	{
		if (!base.IsDestroyed)
		{
			UpdateRotation(UnityEngine.Time.deltaTime);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		ResetRotation();
		TryScheduleServerTick();
		ClientRPC(RpcTarget.Player("CLIENT_OnDeployed", deployedBy));
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	public void SERVER_SetDir(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.CanBuild())
		{
			Vector3 a = Vector3Ex.Direction(player.eyes.position, yaw.transform.position);
			Vector3 normalized = player.eyes.HeadRay().direction.normalized;
			Vector3 normalized2 = Vector3.Lerp(a, normalized, 0.3f).normalized;
			Vector3 vector = BaseMountable.ConvertVector(Quaternion.LookRotation(Quaternion.Inverse(pivotOrigin.rotation) * normalized2).eulerAngles);
			float num = vector.x;
			float num2 = vector.y;
			if (!IsFacingDown)
			{
				num = Mathf.Clamp(num, pitchClamp.x, pitchClamp.y);
				num2 = Mathf.Clamp(num2, yawClamp.x, yawClamp.y);
			}
			pitchAmount += Mathf.DeltaAngle(pitchAmount, num);
			yawAmount += Mathf.DeltaAngle(yawAmount, num2);
			if (reorientEffect.isValid)
			{
				Effect.server.Run(reorientEffect.resourcePath, base.transform.position);
			}
			TryScheduleServerTick();
			SendNetworkUpdate();
		}
	}

	private void TryScheduleServerTick()
	{
		if (lastPitchAmount != pitchAmount || lastYawAmount != yawAmount)
		{
			InvokeRepeating(ServerTick, 0f, 0f);
			Invoke(delegate
			{
				CancelInvoke(ServerTick);
			}, 5f);
		}
		lastPitchAmount = pitchAmount;
		lastYawAmount = yawAmount;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.rcEntity == null)
		{
			info.msg.rcEntity = Facepunch.Pool.Get<RCEntity>();
		}
		info.msg.rcEntity.aim.x = pitchAmount;
		info.msg.rcEntity.aim.y = yawAmount;
		info.msg.rcEntity.aim.z = 0f;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null && base.isServer)
		{
			pitchAmount = info.msg.rcEntity.aim.x;
			yawAmount = info.msg.rcEntity.aim.y;
		}
	}
}
