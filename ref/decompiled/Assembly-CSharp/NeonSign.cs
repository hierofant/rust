#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class NeonSign : Signage
{
	private const float FastSpeed = 0.5f;

	private const float MediumSpeed = 1f;

	private const float SlowSpeed = 2f;

	private const float MinSpeed = 0.5f;

	private const float MaxSpeed = 5f;

	[Header("Neon Sign")]
	public Light topLeft;

	public Light topRight;

	public Light bottomLeft;

	public Light bottomRight;

	public float lightIntensity = 2f;

	[Range(1f, 100f)]
	public int powerConsumption = 10;

	public Material activeMaterial;

	public Material inactiveMaterial;

	public float animationSpeed = 1f;

	public int currentFrame;

	public List<ProtoBuf.NeonSign.Lights> frameLighting;

	public const Flags Flag_HasAuxPower = Flags.Reserved9;

	public bool isAnimating;

	public Action animationLoopAction;

	private int[] cachedInputs;

	private readonly List<int> inputFrameHistory = new List<int>();

	public AmbienceEmitter ambientSoundEmitter;

	public SoundDefinition switchSoundDef;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NeonSign.OnRpcMessage"))
		{
			if (rpc == 2433901419u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetAnimationSpeed");
				}
				using (TimeWarning.New("SetAnimationSpeed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2433901419u, "SetAnimationSpeed", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2433901419u, "SetAnimationSpeed", this, player, 3f))
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
							RPCMessage rPCMessage2 = rPCMessage;
							SetAnimationSpeed(rPCMessage2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetAnimationSpeed");
					}
				}
				return true;
			}
			if (rpc == 1919786296 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UpdateNeonColors");
				}
				using (TimeWarning.New("UpdateNeonColors"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1919786296u, "UpdateNeonColors", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1919786296u, "UpdateNeonColors", this, player, 3f))
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
							UpdateNeonColors(msg2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in UpdateNeonColors");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.neonSign == null)
		{
			return;
		}
		if (frameLighting != null)
		{
			foreach (ProtoBuf.NeonSign.Lights item in frameLighting)
			{
				item.Dispose();
			}
			Facepunch.Pool.Free(ref frameLighting, freeElements: false);
		}
		frameLighting = info.msg.neonSign.frameLighting;
		info.msg.neonSign.frameLighting = null;
		currentFrame = Mathf.Clamp(info.msg.neonSign.currentFrame, 0, paintableSources.Length);
		animationSpeed = Mathf.Clamp(info.msg.neonSign.animationSpeed, 0.5f, 5f);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		animationLoopAction = SwitchToNextFrame;
		cachedInputs = new int[inputs.Length];
	}

	public override void ResetState()
	{
		base.ResetState();
		CancelInvoke(animationLoopAction);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
		else
		{
			if (inputSlot <= 0)
			{
				return;
			}
			bool b = false;
			for (int i = 1; i < cachedInputs.Length; i++)
			{
				if (cachedInputs[i] >= powerConsumption)
				{
					b = true;
					break;
				}
			}
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved9, b);
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (inputSlot >= 0 && inputSlot < cachedInputs.Length)
		{
			cachedInputs[inputSlot] = inputAmount;
		}
		base.UpdateFromInput(inputAmount, inputSlot);
		if (inputSlot > 0)
		{
			if (inputAmount > 0)
			{
				inputFrameHistory.Remove(inputSlot);
				inputFrameHistory.Add(inputSlot);
			}
			else
			{
				inputFrameHistory.Remove(inputSlot);
			}
		}
		int num = 0;
		int num2 = -1;
		if (HasFlag(Flags.Reserved9))
		{
			for (int i = 1; i < cachedInputs.Length; i++)
			{
				int num3 = cachedInputs[i];
				if (num3 > num)
				{
					num = num3;
					num2 = i - 1;
				}
				else if (num3 == num && num3 > 0)
				{
					int item = num2 + 1;
					if (inputFrameHistory.IndexOf(i) > inputFrameHistory.IndexOf(item))
					{
						num2 = i - 1;
					}
				}
			}
		}
		if (inputSlot == 0 && cachedInputs[0] != inputAmount)
		{
			cachedInputs[0] = inputAmount;
		}
		bool flag = HasFlag(Flags.Reserved8);
		if (flag && inputSlot == 0)
		{
			cachedInputs[0] = inputAmount;
			if (!isAnimating)
			{
				InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
				isAnimating = true;
			}
			if (num2 >= 0)
			{
				currentFrame = num2;
				ClientRPC(RpcTarget.NetworkGroup("SetFrame"), currentFrame);
			}
		}
		if (HasFlag(Flags.Reserved9))
		{
			bool num4 = !flag && inputSlot == 0;
			bool flag2 = inputSlot > 0 && inputSlot - 1 == num2;
			bool flag3 = inputAmount == 0 && num2 != currentFrame && !flag;
			if (num4 || flag2 || flag3)
			{
				currentFrame = num2;
				if (isAnimating && flag)
				{
					CancelInvoke(animationLoopAction);
					InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
				}
				ClientRPC(RpcTarget.NetworkGroup("SetFrame"), currentFrame);
				if (isAnimating && !flag)
				{
					CancelInvoke(animationLoopAction);
					isAnimating = false;
				}
			}
		}
		if (isAnimating && !flag)
		{
			CancelInvoke(animationLoopAction);
			isAnimating = false;
			MarkDirty();
		}
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		if (inputIndex == 0)
		{
			return powerConsumption;
		}
		if (!HasFlag(Flags.Reserved8))
		{
			return powerConsumption;
		}
		return 0;
	}

	private void SwitchToFrame(int index)
	{
		if (index >= 0 && index < paintableSources.Length)
		{
			int num = currentFrame;
			currentFrame = index;
			if (currentFrame != num && textureIDs[currentFrame] != 0)
			{
				ClientRPC(RpcTarget.NetworkGroup("SetFrame"), currentFrame);
			}
		}
	}

	private void SwitchToNextFrame()
	{
		int index = (currentFrame + 1) % paintableSources.Length;
		SwitchToFrame(index);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		List<ProtoBuf.NeonSign.Lights> list = Facepunch.Pool.Get<List<ProtoBuf.NeonSign.Lights>>();
		if (frameLighting != null)
		{
			foreach (ProtoBuf.NeonSign.Lights item in frameLighting)
			{
				list.Add(item.Copy());
			}
		}
		info.msg.neonSign = Facepunch.Pool.Get<ProtoBuf.NeonSign>();
		info.msg.neonSign.frameLighting = list;
		info.msg.neonSign.currentFrame = currentFrame;
		info.msg.neonSign.animationSpeed = animationSpeed;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(3f)]
	public void SetAnimationSpeed(RPCMessage msg)
	{
		float f = Mathf.Clamp(msg.read.Float(), 0.5f, 5f);
		if (!f.IsNaN())
		{
			animationSpeed = f;
			if (isAnimating)
			{
				CancelInvoke(animationLoopAction);
				InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
			}
			SendNetworkUpdate();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void UpdateNeonColors(RPCMessage msg)
	{
		if (CanUpdateSign(msg.player))
		{
			int num = msg.read.Int32();
			if (num >= 0 && num < paintableSources.Length)
			{
				EnsureInitialized();
				frameLighting[num].topLeft = ClampColor(msg.read.Color());
				frameLighting[num].topRight = ClampColor(msg.read.Color());
				frameLighting[num].bottomLeft = ClampColor(msg.read.Color());
				frameLighting[num].bottomRight = ClampColor(msg.read.Color());
				SendNetworkUpdate();
			}
		}
	}

	public new void EnsureInitialized()
	{
		if (frameLighting == null)
		{
			frameLighting = Facepunch.Pool.Get<List<ProtoBuf.NeonSign.Lights>>();
		}
		while (frameLighting.Count < paintableSources.Length)
		{
			ProtoBuf.NeonSign.Lights lights = Facepunch.Pool.Get<ProtoBuf.NeonSign.Lights>();
			lights.topLeft = Color.clear;
			lights.topRight = Color.clear;
			lights.bottomLeft = Color.clear;
			lights.bottomRight = Color.clear;
			frameLighting.Add(lights);
		}
	}

	private static Color ClampColor(Color color)
	{
		return new Color(color.r.IsNaN() ? 0f : Mathf.Clamp01(color.r), color.g.IsNaN() ? 0f : Mathf.Clamp01(color.g), color.b.IsNaN() ? 0f : Mathf.Clamp01(color.b), color.a.IsNaN() ? 0f : Mathf.Clamp01(color.a));
	}
}
