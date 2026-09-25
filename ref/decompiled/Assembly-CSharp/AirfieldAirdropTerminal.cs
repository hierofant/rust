using Facepunch;
using Facepunch.Extend;
using UnityEngine;

public class AirfieldAirdropTerminal : ChargeUpIOEntity
{
	[Header("Airfield Airdrop Terminal")]
	public float airDropTickRateWhenActive = 2f;

	[Tooltip("Only used for reference")]
	public EventSchedule airdropEventPrefab;

	public AirfieldTerminalScreen infoScreen;

	public AirfieldTerminalScreen chargingScreen;

	public AirfieldTerminalScreen detailsScreen;

	public ToggleBlink quarterChargeBlinker;

	public ToggleBlink halfChargeBlinker;

	public ToggleBlink threeQuarterChargeBlinker;

	public ToggleBlink activeBlinker;

	private bool isOn;

	[ServerVar]
	public static void force_charge(ConsoleSystem.Arg arg)
	{
		int @int = arg.GetInt(0, 9999999);
		using PooledList<AirfieldAirdropTerminal> pooledList = Pool.Get<PooledList<AirfieldAirdropTerminal>>();
		Query.Server.GetInSphere(ArgEx.Player(arg).transform.position, 100f, pooledList, Query.DistanceCheckType.Bounds);
		foreach (AirfieldAirdropTerminal item in pooledList)
		{
			item.AddCharge(@int);
		}
		arg.ReplyWith($"Force charged {pooledList.Count} AirfieldAirdropTerminals.");
	}

	[ServerVar]
	public static void force_shortcircuit(ConsoleSystem.Arg arg)
	{
		using PooledList<AirfieldAirdropTerminal> pooledList = Pool.Get<PooledList<AirfieldAirdropTerminal>>();
		Query.Server.GetInSphere(ArgEx.Player(arg).transform.position, 100f, pooledList, Query.DistanceCheckType.Bounds);
		foreach (AirfieldAirdropTerminal item in pooledList)
		{
			if (item.isActivated)
			{
				item.Deactivate();
				item.UpdateChargeFlags();
				arg.ReplyWith($"Deactivated AirfieldAirdropTerminal at {item.transform.position}.");
			}
			else
			{
				item.AddCharge(-9999f);
				arg.ReplyWith($"Drained charge from AirfieldAirdropTerminal at {item.transform.position}");
			}
		}
	}

	public void SetAirdropEventTickRate(bool on)
	{
		if (isOn != on)
		{
			isOn = on;
			float tickRate = (isOn ? airDropTickRateWhenActive : 1f);
			if (EventSchedule.allEvents.FindWith((EventSchedule e) => e.Key, airdropEventPrefab.Key) is EventScheduleDynamicTickrate eventScheduleDynamicTickrate)
			{
				eventScheduleDynamicTickrate.tickRate = tickRate;
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		SetAirdropEventTickRate(IsOn());
	}
}
