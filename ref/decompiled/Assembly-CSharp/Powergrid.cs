using System;
using System.Text;
using Facepunch;

public class Powergrid : ConsoleSystem
{
	[ReplicatedVar]
	[Help("If disabled power grid functionality will be disabled.")]
	public static bool enabled = true;

	[ReplicatedVar]
	[Help("Required powergrid stage for green recyclers to return to baseline efficiency. If < 0 then will use the default values.")]
	public static int greenRecyclerFullEfficiencyStage = -1;

	[Help("Pretend there are this many additional heavy fuses currently plugged into the power plant. Can input negative numbers to negate the effect of any currently plugged in fuses.")]
	[ServerVar]
	public static int simulatePowerPlantFuses = 0;

	[ServerVar(Help = "How long a heavy fuse plugged into the power plant lasts while it is decaying at the full rate (how long the worst fuses in the power plant survive for). If <= 0 then fuses last forever.", Saved = true)]
	public static float fuseLifespanSeconds = 9600f;

	private const float defaultFuseLifespanSeconds = 9600f;

	[ServerVar(Help = "How many of the worst condition heavy fuses in the power plant decay at the full rate (burning out after fuseLifespanSeconds). Every other inserted fuse decays slowly instead. If 0 no fuse ever decays at the full rate.", Saved = true)]
	public static int fuseFullDecayCount = 3;

	private const int defaultFuseFullDecayCount = 3;

	[ServerVar(Help = "Minimum fraction (0-1) of the full decay rate applied to heavy fuses that aren't one of the worst fuseFullDecayCount. Each fuse rolls its own fraction between fuseSlowDecayFractionMin and fuseSlowDecayFractionMax and keeps it for its lifetime.", Saved = true)]
	public static float fuseSlowDecayFractionMin = 0.08f;

	private const float defaultFuseSlowDecayFractionMin = 0.08f;

	[ServerVar(Help = "Maximum fraction (0-1) of the full decay rate applied to heavy fuses that aren't one of the worst fuseFullDecayCount. See fuseSlowDecayFractionMin.", Saved = true)]
	public static float fuseSlowDecayFractionMax = 0.12f;

	private const float defaultFuseSlowDecayFractionMax = 0.12f;

	[ServerVar(Help = "Starting power output of powerline poles when 1 heavy fuse is inserted at the power plant.", Saved = true)]
	public static int powerlineBasePowerOutput = 5;

	[ServerVar(Help = "Power output of powerline poles when all possible heavy fuses are inserted at the power plant.", Saved = true)]
	public static int powerlineMaxPowerOutput = 50;

	[ServerVar(Help = "Charge capacity of a drone marketplace's power buffer. A marketplace charges this up from the power plant and bleeds it back out whenever the plant stops carrying it.", Saved = true)]
	public static float marketplaceChargeCapacity = 100f;

	[ServerVar(Help = "Fraction (0-1) of its capacity a drone marketplace needs charged to accept orders. Sets both how long a cold marketplace takes to come online and how long a full one keeps running after the power plant drops out.", Saved = true)]
	public static float marketplaceRequiredChargeFraction = 0.5f;

	[ServerVar(Help = "Heavy fuses that have to be inserted at the power plant before a drone marketplace starts charging. Below this it holds whatever charge it has without building any more.", Saved = true)]
	public static int marketplaceMinimumFusesToCharge = 4;

	[ServerVar(Help = "Charge a drone marketplace gains every second for each heavy fuse inserted at the power plant, once there are at least marketplaceMinimumFusesToCharge of them.", Saved = true)]
	public static float marketplaceChargePerFuse = 0.01f;

	[ServerVar(Help = "Charge a drone marketplace loses every second while there are no heavy fuses inserted at the power plant. A single fuse is enough to stop the bleed.", Saved = true)]
	public static float marketplaceDrainRate = 0.05f;

	[ServerVar(Help = "Max time per frame (ms) to spend notifying powergrid entities of a stage change.", Saved = true)]
	public static float stageChangeWorkQueueBudget = 0.1f;

	[ServerVar(Help = "Time to wait (s) between each individual entity getting notified of a powergrid stage change. Higher values will delay the time it takes for all entities to receive notification of a stage change. Entities can skip this wait with stageChangeWorkQueueGroupJobsDistance", Saved = true)]
	public static float stageChangeWorkQueueDelayBetweenJobs = 0f;

	[ServerVar(Help = "If a powergrid entity is within this range of the first powergrid entity to receive a stage change update this frame, then that entity will also receive an update (skipping stageChangeWorkQueueTimeBetweenJobs)", Saved = true)]
	public static float stageChangeWorkQueueGroupJobsDistance = 20f;

	public static float stageChangeWorkQueueGroupJobsSqrDistance => stageChangeWorkQueueGroupJobsDistance * stageChangeWorkQueueGroupJobsDistance;

	[ServerVar]
	public static void status(Arg arg)
	{
		PowergridManager serverInstance = PointEntity<PowergridManager>.ServerInstance;
		if (serverInstance == null)
		{
			arg.ReplyWith("Failed to retrieve server instance for PowergridManager");
			return;
		}
		StringBuilder obj = Pool.Get<StringBuilder>();
		obj.AppendLine(string.Format("{0}: {1}", "enabled", enabled));
		obj.AppendLine(string.Format("{0}: {1}", "simulatePowerPlantFuses", simulatePowerPlantFuses));
		obj.AppendLine(string.Format("{0}: {1}", "fuseLifespanSeconds", fuseLifespanSeconds));
		obj.AppendLine(string.Format("{0}: {1}", "fuseFullDecayCount", fuseFullDecayCount));
		obj.AppendLine(string.Format("{0}: {1}", "fuseSlowDecayFractionMin", fuseSlowDecayFractionMin));
		obj.AppendLine(string.Format("{0}: {1}", "fuseSlowDecayFractionMax", fuseSlowDecayFractionMax));
		obj.AppendLine(string.Format("{0}: {1}", "powerlineBasePowerOutput", powerlineBasePowerOutput));
		obj.AppendLine(string.Format("{0}: {1}", "powerlineMaxPowerOutput", powerlineMaxPowerOutput));
		obj.AppendLine(string.Format("{0}: {1}", "greenRecyclerFullEfficiencyStage", greenRecyclerFullEfficiencyStage));
		obj.AppendLine(string.Format("{0}: {1}", "stageChangeWorkQueueBudget", stageChangeWorkQueueBudget));
		obj.AppendLine(string.Format("{0}: {1}", "stageChangeWorkQueueDelayBetweenJobs", stageChangeWorkQueueDelayBetweenJobs));
		obj.AppendLine(string.Format("{0}: {1}", "stageChangeWorkQueueGroupJobsDistance", stageChangeWorkQueueGroupJobsDistance));
		obj.AppendLine(string.Format("{0}: {1}", "marketplaceChargeCapacity", marketplaceChargeCapacity));
		obj.AppendLine(string.Format("{0}: {1}", "marketplaceRequiredChargeFraction", marketplaceRequiredChargeFraction));
		obj.AppendLine(string.Format("{0}: {1}", "marketplaceMinimumFusesToCharge", marketplaceMinimumFusesToCharge));
		obj.AppendLine(string.Format("{0}: {1}", "marketplaceChargePerFuse", marketplaceChargePerFuse));
		obj.AppendLine(string.Format("{0}: {1}", "marketplaceDrainRate", marketplaceDrainRate));
		obj.AppendLine();
		obj.AppendLine($"Current stage: {serverInstance.CurrentStage}");
		int numberOfStages = PowergridStageConfig.instance.GetNumberOfStages();
		for (int i = 1; i <= numberOfStages; i++)
		{
			if (PowergridStageConfig.instance.TryGetStageDataForStage(i, out var stageData))
			{
				obj.AppendLine($"Stage {i} required fuses: {stageData.requiredFuses}");
			}
		}
		obj.AppendLine($"Number of inserted PowerPlant fuses: {serverInstance.Server_GetPowerPlantInsertedFuses()}");
		obj.AppendLine($"Number of PowerPlant fuse sockets: {serverInstance.Server_GetFuseSocketsCount()}");
		obj.AppendLine($"Number of Powergrid access points: {PowergridManager.GetNoOfPowergridAccessPoints()}");
		obj.AppendLine($"Number of Powergrid connected entities: {PowergridManager.GetNoOfPowergridEntities()}");
		int j = 0;
		for (int count = Marketplace.serverMarketplaces.Count; j < count; j++)
		{
			Marketplace marketplace = Marketplace.serverMarketplaces[j];
			obj.AppendLine($"Marketplace {marketplace.net.ID}: charge {marketplace.Server_GetCurrentCharge():0.#}/{marketplaceChargeCapacity:0.#} " + $"({marketplace.ChargeFraction:P0}) at {marketplace.Server_GetChargeRate():+0.###;-0.###;0}/s, " + $"accepting orders: {marketplace.Server_CanAcceptOrder()}");
		}
		arg.ReplyWith(obj.ToString());
		Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar]
	public static void fuseStatus(Arg arg)
	{
		if (PointEntity<PowergridManager>.ServerInstance == null)
		{
			arg.ReplyWith("Failed to retrieve server instance for PowergridManager");
			return;
		}
		using PooledList<Item> pooledList = Pool.Get<PooledList<Item>>();
		PowergridManager.Server_GatherInsertedFuses(pooledList);
		if (pooledList.Count > 0)
		{
			StringBuilder obj = Pool.Get<StringBuilder>();
			using PooledList<Item> pooledList2 = Pool.Get<PooledList<Item>>();
			PowergridManager.Server_GatherFullDecayFuses(pooledList2);
			int i = 0;
			for (int count = pooledList.Count; i < count; i++)
			{
				Item item = pooledList[i];
				float num = (pooledList2.Contains(item) ? 1f : PowergridManager.Server_GetSlowDecayRateScale(item));
				obj.Append($"  Fuse {item.uid}: condition {item.conditionNormalized:P0}");
				obj.Append($", decay rate {num:P0}");
				if (fuseLifespanSeconds > 0f && num > 0f)
				{
					float num2 = item.conditionNormalized * fuseLifespanSeconds / num;
					obj.Append($", ~{TimeSpan.FromSeconds(num2):d\\.hh\\:mm\\:ss} left at this rate");
				}
				else
				{
					obj.Append(", never burns out at this rate");
				}
				obj.AppendLine();
			}
			arg.ReplyWith(obj.ToString());
			Pool.FreeUnmanaged(ref obj);
			return;
		}
		arg.ReplyWith("No fuses inserted");
	}
}
