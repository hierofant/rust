using Facepunch;
using Facepunch.Rust;
using ProtoBuf;

public class OilSwitchBroadcast : IOEntity
{
	public float OilOutputMultiplier = 1f;

	private static ListHashSet<IOilSwitchReceiver> receivers = new ListHashSet<IOilSwitchReceiver>();

	private static ListHashSet<OilSwitchBroadcast> activeSwitches = new ListHashSet<OilSwitchBroadcast>();

	private static float TotalOutputLevel
	{
		get
		{
			float num = 0f;
			foreach (OilSwitchBroadcast activeSwitch in activeSwitches)
			{
				if (activeSwitch != null)
				{
					num += activeSwitch.OilOutputMultiplier;
				}
			}
			return num;
		}
	}

	public static void RegisterReceiver(IOilSwitchReceiver r)
	{
		receivers.TryAdd(r);
	}

	public static void DeregisterReceiver(IOilSwitchReceiver r)
	{
		receivers.Remove(r);
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		bool hasPower = base.GetHasPower(inputAmount, inputSlot);
		bool flag = false;
		if (!hasPower && activeSwitches.Contains(this))
		{
			activeSwitches.Remove(this);
			flag = true;
		}
		else if (hasPower && !activeSwitches.Contains(this))
		{
			activeSwitches.Add(this);
			flag = true;
		}
		if (flag)
		{
			Broadcast();
			if (hasPower && inputs.Length != 0 && inputs[0].IsConnected() && inputs[0].connectedTo.Get(base.isServer) is TimerSwitch timerSwitch && timerSwitch.lastUsedPlayer != null)
			{
				timerSwitch.lastUsedPlayer.AddClanScore(ClanScoreEventType.StartedOilRigFuelSwitch);
				Analytics.Azure.OnOilRigFuelSwitchStarted(timerSwitch.lastUsedPlayer);
			}
		}
		return hasPower;
	}

	private static void Broadcast()
	{
		float totalOutputLevel = TotalOutputLevel;
		foreach (IOilSwitchReceiver receiver in receivers)
		{
			receiver.OnOilSwitchToggled(totalOutputLevel);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.oilswitchBroadcast = Pool.Get<ProtoBuf.OilSwitchBroadcast>();
			info.msg.oilswitchBroadcast.oilOutputMultiplier = OilOutputMultiplier;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.oilswitchBroadcast != null)
		{
			OilOutputMultiplier = info.msg.oilswitchBroadcast.oilOutputMultiplier;
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (activeSwitches.Contains(this))
		{
			activeSwitches.Remove(this);
			Broadcast();
		}
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		if (activeSwitches.Contains(this))
		{
			activeSwitches.Remove(this);
			Broadcast();
		}
	}
}
