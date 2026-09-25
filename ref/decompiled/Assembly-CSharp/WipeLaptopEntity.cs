#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class WipeLaptopEntity : BaseEntity
{
	public static Translate.Phrase Phrase_Armed = new Translate.Phrase("laptop_armed", "Warhead Status: Armed");

	public static Translate.Phrase Phrase_Disarmed = new Translate.Phrase("laptop_disarmed", "Warhead Status: Disarmed");

	public static Flags ArmedFlag = Flags.Reserved1;

	public float ArmTime = 5f;

	public float DisarmTime = 5f;

	public TimeUntil TimeLeft;

	public Text MiddleText;

	private Translate.Phrase currentMiddlePhrase;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WipeLaptopEntity.OnRpcMessage"))
		{
			if (rpc == 2017018603 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ArmLaptop");
				}
				using (TimeWarning.New("ArmLaptop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2017018603u, "ArmLaptop", this, player, 5f))
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
							ArmLaptop(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ArmLaptop");
					}
				}
				return true;
			}
			if (rpc == 2423597272u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - DefuseLaptop");
				}
				using (TimeWarning.New("DefuseLaptop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2423597272u, "DefuseLaptop", this, player, 5f))
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
							RPCMessage msg3 = rPCMessage;
							DefuseLaptop(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in DefuseLaptop");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		info.msg.wipeLaptop = Facepunch.Pool.Get<WipeLaptop>();
		info.msg.wipeLaptop.timeLeft = (int)TimeLeft.LeftFrom(info.cachedTime.Time);
		info.msg.wipeLaptop.armTime = ArmTime;
		info.msg.wipeLaptop.disarmTime = DisarmTime;
		base.Save(info);
	}

	public override void Load(LoadInfo info)
	{
		if (info.msg.wipeLaptop != null)
		{
			TimeLeft = info.msg.wipeLaptop.timeLeft;
			ArmTime = info.msg.wipeLaptop.armTime;
			DisarmTime = info.msg.wipeLaptop.disarmTime;
		}
		base.Load(info);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(5f)]
	public void ArmLaptop(RPCMessage msg)
	{
		if (msg.read.Int32() == 3 && !IsArmed())
		{
			SetArmed(state: true);
		}
	}

	private void OnArmLaptopStart()
	{
	}

	[RPC_Server]
	[RPC_Server.IsVisible(5f)]
	public void DefuseLaptop(RPCMessage msg)
	{
		if (msg.read.Int32() == 3 && IsArmed())
		{
			SetArmed(state: false);
		}
	}

	private void SetArmed(bool state)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(ArmedFlag, state);
	}

	private bool IsArmed()
	{
		return HasFlag(ArmedFlag);
	}

	public void SetTimeLeft(int seconds)
	{
		TimeLeft = seconds;
		if (base.isServer)
		{
			SendNetworkUpdate();
		}
	}
}
