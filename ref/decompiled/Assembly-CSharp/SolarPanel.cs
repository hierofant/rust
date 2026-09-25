using Oxide.Core;
using UnityEngine;

public class SolarPanel : IOEntity
{
	public class SunUpdateWorkQueue : PersistentObjectWorkQueue<SolarPanel>
	{
		protected override void RunJob(SolarPanel entity)
		{
			if (!((float)entity.lastSunUpdate < 5f))
			{
				entity.lastSunUpdate = UnityEngine.Random.Range(-2f, 0f);
				entity.SunUpdate();
			}
		}
	}

	public Transform sunSampler;

	public int maximalPowerOutput = 10;

	public float dot_minimum = 0.1f;

	public float dot_maximum = 0.6f;

	[ServerVar(Saved = true)]
	public static float sunUpdateBudgetMs = 0.05f;

	public static SunUpdateWorkQueue WorkQueue = new SunUpdateWorkQueue();

	private TimeSince lastSunUpdate;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int MaximalPowerOutput()
	{
		return maximalPowerOutput;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		lastSunUpdate = -4f;
		WorkQueue.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		WorkQueue.Remove(this);
	}

	public void SunUpdate()
	{
		int num;
		if (TOD_Sky.Instance.IsNight)
		{
			num = 0;
		}
		else
		{
			Vector3 sunDirection = TOD_Sky.Instance.SunDirection;
			float value = Vector3.Dot(sunSampler.forward, sunDirection);
			float num2 = Mathf.InverseLerp(dot_minimum, dot_maximum, value);
			if (num2 > 0f && !IsVisible(sunSampler.position + sunDirection * 100f, 101f))
			{
				num2 = 0f;
			}
			num = Mathf.FloorToInt((float)maximalPowerOutput * num2 * base.healthFraction);
		}
		bool num3 = currentEnergy != num;
		currentEnergy = num;
		if (num3 && Interface.CallHook("OnSolarPanelSunUpdate", this, num) == null)
		{
			MarkDirty();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0)
		{
			return 0;
		}
		return currentEnergy;
	}
}
