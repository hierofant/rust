using UnityEngine;

public class WaterTreatmentFlowRateBroadcast : FacepunchBehaviour, IServerComponent
{
	public static float WaterTreatmentBroadcastFlowRate;

	public static int receivers;

	private static ListHashSet<WaterTreatmentFlowRateBroadcast> broadcasters = new ListHashSet<WaterTreatmentFlowRateBroadcast>();

	private static float forcedFlowRate = -1f;

	private WaterTreatmentWaterTank Tank;

	[ServerVar(Help = "Force the water treatment flow rate per minute. Pass no argument to release the override and return to pressure-derived state.")]
	public static void force_flow_rate(ConsoleSystem.Arg arg)
	{
		forcedFlowRate = arg.GetFloat(0, -1f);
		Refresh();
		if (forcedFlowRate < 0f)
		{
			arg.ReplyWith($"Forced water treatment switch broadcast override cleared. Current state: {WaterTreatmentBroadcastFlowRate}");
		}
		else
		{
			arg.ReplyWith($"Forced water treatment switch broadcast to: {WaterTreatmentBroadcastFlowRate} (override active)");
		}
	}

	[ServerVar]
	public static void get_num_listeners(ConsoleSystem.Arg arg)
	{
		arg.ReplyWith($"Number of listeners: {receivers}");
	}

	[ServerVar]
	public static void get_num_broadcasters(ConsoleSystem.Arg arg)
	{
		arg.ReplyWith($"Number of broadcasters: {broadcasters.Count}");
	}

	public void SetWaterTank(WaterTreatmentWaterTank tank)
	{
		Tank = tank;
		broadcasters.Add(this);
		Refresh();
	}

	public void CleanUp()
	{
		broadcasters.Remove(this);
	}

	public static void Refresh()
	{
		if (forcedFlowRate >= 0f)
		{
			WaterTreatmentBroadcastFlowRate = forcedFlowRate;
			return;
		}
		WaterTreatmentBroadcastFlowRate = 0f;
		foreach (WaterTreatmentFlowRateBroadcast broadcaster in broadcasters)
		{
			if (!(broadcaster == null) && !(broadcaster.Tank == null))
			{
				WaterTreatmentBroadcastFlowRate += broadcaster.Tank.FlowRatePerMinute;
			}
		}
		WaterTreatmentBroadcastFlowRate = Mathf.Min(WaterTreatmentBroadcastFlowRate, WaterTreatmentWaterTank.maxFlowRatePerMinute);
	}
}
