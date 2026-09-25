#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class HBHFSensor : BaseDetector
{
	public int range = 10;

	public const int MIN_RANGE = 2;

	public const int MAX_RANGE = 10;

	public GameObjectRef detectUpEffectPrefab;

	public GameObjectRef detectDownEffectPrefab;

	public GameObjectRef uiPanelPrefab;

	public const Flags Flag_IncludeOthers = Flags.Reserved4;

	public const Flags Flag_IncludeAuthed = Flags.Reserved3;

	[ServerVar(Help = "When enabled, broadcasts debug drawing for HBHFSensor visibility checks (eye position, forward, range, per-player LOS rays).")]
	public static bool DebugDraw;

	private int detectedPlayers;

	private Action UpdatePassthroughAmountCB;

	public int DetectedPlayers => detectedPlayers;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HBHFSensor.OnRpcMessage"))
		{
			if (rpc == 4073303808u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetConfig");
				}
				using (TimeWarning.New("SetConfig"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4073303808u, "SetConfig", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4073303808u, "SetConfig", this, player, 3f))
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
							RPCMessage config = rPCMessage;
							SetConfig(config);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetConfig");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnObjects()
	{
		base.OnObjects();
		UpdatePassthroughAmount();
		if (UpdatePassthroughAmountCB == null)
		{
			UpdatePassthroughAmountCB = UpdatePassthroughAmount;
		}
		InvokeRandomized(UpdatePassthroughAmountCB, 0f, 1f, 0.1f);
	}

	public override void OnEmpty()
	{
		base.OnEmpty();
		UpdatePassthroughAmount();
		if (UpdatePassthroughAmountCB == null)
		{
			UpdatePassthroughAmountCB = UpdatePassthroughAmount;
		}
		CancelInvoke(UpdatePassthroughAmountCB);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return Mathf.Min(detectedPlayers, GetCurrentEnergy());
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount == 0)
		{
			detectedPlayers = 0;
		}
	}

	public void UpdatePassthroughAmount()
	{
		if (base.isClient || !IsPowered())
		{
			return;
		}
		int num = detectedPlayers;
		detectedPlayers = CountDetectedPlayers();
		if (num != detectedPlayers)
		{
			MarkDirty();
			if (detectedPlayers > num)
			{
				Effect.server.Run(detectUpEffectPrefab.resourcePath, base.transform.position, Vector3.up);
			}
			else if (detectedPlayers < num)
			{
				Effect.server.Run(detectDownEffectPrefab.resourcePath, base.transform.position, Vector3.up);
			}
		}
	}

	private int CountDetectedPlayers()
	{
		if (myTrigger.entityContents == null || myTrigger.entityContents.Count == 0)
		{
			return 0;
		}
		IPrivilege privilege = null;
		bool flag = false;
		int num = 0;
		Vector3 vector = base.transform.position + base.transform.forward * 0.1f;
		if (DebugDraw)
		{
			DebugDrawSensor(vector);
		}
		foreach (BaseEntity entityContent in myTrigger.entityContents)
		{
			if (!(entityContent is BasePlayer basePlayer) || Interface.CallHook("OnSensorDetect", this, basePlayer) != null || basePlayer == null || basePlayer.IsDead() || basePlayer.IsSleeping() || !basePlayer.isServer)
			{
				continue;
			}
			if (!flag)
			{
				privilege = GetPrivilege();
				flag = true;
			}
			bool flag2 = privilege?.IsAuthed(basePlayer) ?? false;
			if ((!flag2 || ShouldIncludeAuthorized()) && (flag2 || ShouldIncludeOthers()))
			{
				Vector3 vector2 = basePlayer.ClosestPoint(vector);
				Vector3 normalized = (vector2 - vector).normalized;
				Vector3 vector3 = vector2 + normalized * 0.5f;
				bool flag3 = basePlayer.IsVisible(vector, vector2, range);
				bool flag4 = flag3 && basePlayer.CanSee(vector2, vector3);
				bool num2 = flag3 && flag4;
				if (DebugDraw)
				{
					DebugDrawPlayer(vector, basePlayer, vector2, vector3, flag3, flag4);
				}
				if (num2)
				{
					num++;
				}
			}
		}
		return num;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void SetConfig(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanUse(player))
		{
			bool b = msg.read.Bit();
			bool b2 = msg.read.Bit();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved3, b);
				flagsUpdateScope.Set(Flags.Reserved4, b2);
			}
			int num = msg.read.Int32();
			SetRange(num);
		}
	}

	public void SetRange(int value)
	{
		value = Mathf.Clamp(value, 2, 10);
		range = value;
		SendNetworkUpdate();
	}

	private void DebugDrawSensor(Vector3 eyePos)
	{
		Vector3 position = base.transform.position;
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(position, 2f, Color.white, 0.05f));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(eyePos, 2f, Color.yellow, 0.05f));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(position, eyePos, 2f, Color.yellow));
		Vector3 pos = position + base.transform.forward * 1f;
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(position, pos, 2f, Color.cyan));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(eyePos, 2f, new Color(1f, 1f, 0f, 0.25f), range));
	}

	private void DebugDrawPlayer(Vector3 eyePos, BasePlayer player, Vector3 closestPoint, Vector3 probeEnd, bool sensorSeesPoint, bool rayReachesBody)
	{
		OBB oBB = player.WorldSpaceBounds();
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Box(oBB.position, 2f, Color.white, oBB.extents * 2f, oBB.rotation));
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(eyePos, closestPoint, 2f, sensorSeesPoint ? Color.green : Color.red));
		if (sensorSeesPoint)
		{
			ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Line(closestPoint, probeEnd, 2f, rayReachesBody ? Color.green : Color.red));
		}
		ConsoleNetwork.BroadcastToAdmins(DDrawCommand.Sphere(closestPoint, 2f, Color.yellow, 0.08f));
	}

	public bool CanUse(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseHBHFSensor", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return player.CanBuild();
	}

	public bool ShouldIncludeAuthorized()
	{
		return HasFlag(Flags.Reserved3);
	}

	public bool ShouldIncludeOthers()
	{
		return HasFlag(Flags.Reserved4);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericInt1 = range;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			range = info.msg.ioEntity.genericInt1;
		}
	}
}
