#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class TimerSwitch : IOEntity
{
	public float timerLength = 10f;

	public Transform timerDrum;

	private float timePassed;

	private float input1Amount;

	private float serverStartTime = -1f;

	public BasePlayer lastUsedPlayer { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TimerSwitch.OnRpcMessage"))
		{
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SVSwitch");
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4167839872u, "SVSwitch", this, player, 3f))
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
							SVSwitch(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public float GetPassedTime()
	{
		return timePassed;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (IsInvoking(AdvanceTime))
		{
			EndTimer();
		}
	}

	public override bool WantsPassthroughPower()
	{
		if (IsPowered())
		{
			return IsOn();
		}
		return false;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsPowered() || !IsOn())
		{
			return 0;
		}
		return base.GetPassthroughAmount(outputSlot);
	}

	public override bool WantsPower(int inputIndex)
	{
		return inputIndex == 0;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		switch (inputSlot)
		{
		case 0:
			base.UpdateFromInput(inputAmount, inputSlot);
			if (IsPowered())
			{
				if (timePassed != 0f && !IsInvoking(AdvanceTime))
				{
					SetFlagLocal(Flags.On, b: false);
					SwitchPressed();
				}
			}
			else if (IsInvoking(AdvanceTime))
			{
				EndTimer();
				SetFlagLocal(Flags.On, b: false);
				SendNetworkUpdate_Flags();
			}
			break;
		case 1:
			if (input1Amount != (float)inputAmount)
			{
				if (inputAmount > 0)
				{
					SwitchPressed();
				}
				input1Amount = inputAmount;
			}
			break;
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SVSwitch(RPCMessage msg)
	{
		SwitchPressed();
		lastUsedPlayer = msg.player;
	}

	public void SwitchPressed()
	{
		if (!IsOn() && IsPowered())
		{
			StartTimer();
			SetFlagLocal(Flags.On, b: true);
			MarkDirty();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (timePassed == 0f)
		{
			if (IsOn())
			{
				using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.On, b: false);
				}
			}
		}
		else
		{
			SwitchPressed();
		}
	}

	public void AdvanceTime()
	{
		if (timePassed < 0f)
		{
			timePassed = 0f;
		}
		timePassed = UnityEngine.Time.realtimeSinceStartup - serverStartTime;
		if (timePassed >= timerLength)
		{
			EndTimer();
		}
	}

	public void StartTimer()
	{
		serverStartTime = UnityEngine.Time.realtimeSinceStartup;
		timePassed = 0f;
		InvokeRepeating(AdvanceTime, 0f, 0.1f);
	}

	public void EndTimer()
	{
		serverStartTime = 0f;
		timePassed = 0f;
		CancelInvoke(AdvanceTime);
		SetFlagLocal(Flags.On, b: false);
		MarkDirty();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericFloat1 = GetPassedTime();
		info.msg.ioEntity.genericFloat2 = timerLength;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			timePassed = info.msg.ioEntity.genericFloat1;
			timerLength = info.msg.ioEntity.genericFloat2;
		}
	}
}
