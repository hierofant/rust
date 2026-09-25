#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch.Rust;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class FogMachine : ContainerIOEntity, IAlwaysOn
{
	public const Flags FogFieldOn = Flags.Reserved10;

	public const Flags MotionMode = Flags.Reserved9;

	public const Flags Emitting = Flags.Reserved6;

	public const Flags Flag_HasJuice = Flags.Reserved5;

	public const Flags AlwaysOn = Flags.Reserved11;

	public float fogLength = 60f;

	public float nozzleBlastDuration = 5f;

	public float fuelPerSec = 1f;

	public TriggerBase motionTrigger;

	private BuildingPrivlidge _cachedTc;

	private float _cacheTimeout;

	private Action _startFoggingCB;

	private Action EnableFogFieldCB;

	private Action DisableNozzleCB;

	private Action FinishFoggingCB;

	private float pendingFuel;

	private Action StartFoggingCB => StartFogging;

	public bool IsEmitting()
	{
		return HasFlag(Flags.Reserved6);
	}

	public bool HasJuice()
	{
		return HasFlag(Flags.Reserved5);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetFogOn(RPCMessage msg)
	{
		if (!IsEmitting() && !IsOn() && HasFuel() && msg.player.CanBuild())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
			}
			InvokeRepeating(StartFoggingCB, 0f, fogLength - 1f);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetFogOff(RPCMessage msg)
	{
		if (!IsOn() || !msg.player.CanBuild())
		{
			return;
		}
		CancelInvoke(StartFoggingCB);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SetMotionDetection(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (!msg.player.CanBuild())
		{
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved9, flag);
			if (flag)
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
		UpdateMotionMode();
	}

	public void UpdateMotionMode()
	{
		if (HasFlag(Flags.Reserved9))
		{
			InvokeRandomized(CheckTrigger, UnityEngine.Random.Range(0f, 1.5f), 1f, 0.5f);
		}
		else
		{
			CancelInvoke(CheckTrigger);
		}
	}

	public void CheckTrigger()
	{
		if (IsEmitting() || motionTrigger == null || !motionTrigger.HasAnyEntityContents)
		{
			return;
		}
		BuildingPrivlidge cachedTc = GetCachedTc();
		Vector3 targetPos = base.transform.position + Vector3.up * 0.1f;
		foreach (BaseEntity entityContent in motionTrigger.entityContents)
		{
			BasePlayer basePlayer = entityContent as BasePlayer;
			if (basePlayer != null && !basePlayer.IsSleeping() && basePlayer.IsAlive() && (cachedTc == null || !cachedTc.IsAuthed(basePlayer)) && basePlayer.CanSee(basePlayer.eyes.position, targetPos))
			{
				StartFogging();
				break;
			}
		}
	}

	private BuildingPrivlidge GetCachedTc()
	{
		if (_cachedTc != null && _cachedTc.IsDestroyed)
		{
			_cachedTc = null;
		}
		if (_cachedTc == null || UnityEngine.Time.realtimeSinceStartup > _cacheTimeout)
		{
			_cachedTc = null;
			BuildingManager.Building building = GetBuilding();
			if (building != null)
			{
				_cachedTc = building.GetDominatingBuildingPrivilege();
			}
			if (_cachedTc == null)
			{
				return GetNearestBuildingPrivilege(cached: true, 10f);
			}
			_cacheTimeout = UnityEngine.Time.realtimeSinceStartup + 3f;
		}
		return _cachedTc;
	}

	public void StartFogging()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (!UseFuel(1f))
			{
				CancelInvoke(StartFoggingCB);
				flagsUpdateScope.Set(Flags.On, b: false);
				return;
			}
			flagsUpdateScope.Set(Flags.Reserved6, b: true);
		}
		if (EnableFogFieldCB == null)
		{
			EnableFogFieldCB = EnableFogField;
		}
		Invoke(EnableFogFieldCB, 1f);
		if (DisableNozzleCB == null)
		{
			DisableNozzleCB = DisableNozzle;
		}
		Invoke(DisableNozzleCB, nozzleBlastDuration);
		if (FinishFoggingCB == null)
		{
			FinishFoggingCB = FinishFogging;
		}
		Invoke(FinishFoggingCB, fogLength);
	}

	public virtual void EnableFogField()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved10, b: true);
	}

	public void DisableNozzle()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
	}

	public virtual void FinishFogging()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved10, b: false);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved10, b: false);
			flagsUpdateScope.Set(Flags.Reserved6, b: false);
			flagsUpdateScope.Set(Flags.Reserved5, HasFuel());
		}
		if (IsOn())
		{
			InvokeRepeating(StartFoggingCB, 0f, fogLength - 1f);
		}
		UpdateMotionMode();
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, HasFuel());
		}
		base.PlayerStoppedLooting(player);
	}

	public int GetFuelAmount()
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return 0;
		}
		return slot.amount;
	}

	public bool HasFuel()
	{
		if (IsAlwaysOn())
		{
			return true;
		}
		return GetFuelAmount() >= 1;
	}

	public bool UseFuel(float seconds)
	{
		if (IsAlwaysOn())
		{
			return true;
		}
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return false;
		}
		pendingFuel += seconds * fuelPerSec;
		if (pendingFuel >= 1f)
		{
			int num = Mathf.FloorToInt(pendingFuel);
			slot.UseItem(num);
			Analytics.Azure.AddPendingItems(this, slot.info.shortname, num, "fog");
			pendingFuel -= num;
		}
		return true;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		bool flag = false;
		switch (inputSlot)
		{
		case 0:
			flag = inputAmount > 0;
			break;
		case 1:
			if (inputAmount == 0)
			{
				return;
			}
			flag = true;
			break;
		case 2:
			if (inputAmount == 0)
			{
				return;
			}
			flag = false;
			break;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (flag)
		{
			if (!IsEmitting() && !IsOn() && HasFuel())
			{
				flagsUpdateScope.Set(Flags.On, b: true);
				InvokeRepeating(StartFoggingCB, 0f, fogLength - 1f);
			}
		}
		else if (IsOn())
		{
			CancelInvoke(StartFoggingCB);
			flagsUpdateScope.Set(Flags.On, b: false);
		}
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public virtual bool IsAlwaysOn()
	{
		if (HasFlag(Flags.Reserved11))
		{
			return Creative.alwaysOnEnabled;
		}
		return false;
	}

	public void SetAlwaysOn(bool flag)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved11, flag);
		}
		AlwaysOnToggled(flag);
	}

	public void AlwaysOnToggled(bool flag)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (flag)
		{
			if (!IsEmitting() && !IsOn() && HasFuel())
			{
				flagsUpdateScope.Set(Flags.On, b: true);
				InvokeRepeating(StartFoggingCB, 0f, fogLength - 1f);
			}
		}
		else if (IsOn())
		{
			CancelInvoke(StartFoggingCB);
			flagsUpdateScope.Set(Flags.On, b: false);
		}
	}

	public virtual bool MotionModeEnabled()
	{
		return true;
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FogMachine.OnRpcMessage"))
		{
			if (rpc == 2788115565u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetFogOff");
				}
				using (TimeWarning.New("SetFogOff"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2788115565u, "SetFogOff", this, player, 3f))
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
							RPCMessage fogOff = rPCMessage;
							SetFogOff(fogOff);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetFogOff");
					}
				}
				return true;
			}
			if (rpc == 3905831928u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetFogOn");
				}
				using (TimeWarning.New("SetFogOn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3905831928u, "SetFogOn", this, player, 3f))
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
							RPCMessage fogOn = rPCMessage;
							SetFogOn(fogOn);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SetFogOn");
					}
				}
				return true;
			}
			if (rpc == 1773639087 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetMotionDetection");
				}
				using (TimeWarning.New("SetMotionDetection"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1773639087u, "SetMotionDetection", this, player, 3f))
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
							RPCMessage motionDetection = rPCMessage;
							SetMotionDetection(motionDetection);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in SetMotionDetection");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
