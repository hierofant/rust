using System.Linq;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using UnityEngine;

public class AirfieldCallChinookTerminal : ChargeUpIOEntity
{
	[Header("Airfield Call Chinook Terminal")]
	[Tooltip("Only used for reference")]
	public EventSchedule chinookEventPrefab;

	public CH47DropZone associatedDropZone;

	public AirfieldTerminalScreen infoScreen;

	public AirfieldTerminalScreen chargingScreen;

	public ToggleBlink quarterChargeBlinker;

	public ToggleBlink halfChargeBlinker;

	public ToggleBlink threeQuarterChargeBlinker;

	public ToggleBlink activeBlinker;

	public static bool isActive;

	public static CH47HelicopterAIController selectedChinook;

	public static CH47DropZone selectedChinookDropZone;

	[ServerVar]
	public static void force_charge(ConsoleSystem.Arg arg)
	{
		int @int = arg.GetInt(0, 9999999);
		using PooledList<AirfieldCallChinookTerminal> pooledList = Pool.Get<PooledList<AirfieldCallChinookTerminal>>();
		Query.Server.GetInSphere(ArgEx.Player(arg).transform.position, 100f, pooledList, Query.DistanceCheckType.Bounds);
		foreach (AirfieldCallChinookTerminal item in pooledList)
		{
			item.AddCharge(@int);
		}
		arg.ReplyWith($"Force charged {pooledList.Count} AirfieldCallChinookTerminals.");
	}

	[ServerVar]
	public static void force_shortcircuit(ConsoleSystem.Arg arg)
	{
		using PooledList<AirfieldCallChinookTerminal> pooledList = Pool.Get<PooledList<AirfieldCallChinookTerminal>>();
		Query.Server.GetInSphere(ArgEx.Player(arg).transform.position, 100f, pooledList, Query.DistanceCheckType.Bounds);
		foreach (AirfieldCallChinookTerminal item in pooledList)
		{
			if (item.isActivated)
			{
				item.Deactivate();
				item.UpdateChargeFlags();
				arg.ReplyWith($"Deactivated AirfieldCallChinookTerminal at {item.transform.position}.");
			}
			else
			{
				item.AddCharge(-9999f);
				arg.ReplyWith($"Drained charge from AirfieldCallChinookTerminal at {item.transform.position}");
			}
		}
	}

	public override void Activate()
	{
		base.Activate();
		isActive = true;
		selectedChinook = null;
		selectedChinookDropZone = ((associatedDropZone != null) ? associatedDropZone : CH47DropZone.GetClosest(base.transform.position));
		if (CH47HelicopterAIController.activeScientistCH47s.Count == 0 || CH47HelicopterAIController.activeScientistCH47s.All((CH47HelicopterAIController c) => !c.CanDropCrate()))
		{
			TriggerChinookEvent();
		}
		Analytics.Azure.OnAirfieldChinookCall();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		isActive = false;
		selectedChinook = null;
		selectedChinookDropZone = null;
	}

	private void TriggerChinookEvent()
	{
		EventSchedule eventSchedule = EventSchedule.enabledEvents.FindWith((EventSchedule e) => e.Key, chinookEventPrefab.Key);
		if (eventSchedule != null)
		{
			eventSchedule.Trigger();
		}
	}
}
