using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class Sprinkler : IOEntity
{
	public float SplashFrequency = 1f;

	public Transform Eyes;

	public int WaterPerSplash = 1;

	public float DecayPerSplash = 0.8f;

	public const Flags Flag_Radiation = Flags.Reserved3;

	public TriggerSplashable DynamicObjectsTrigger;

	public static PartialMobileStaticGrid<BaseEntity> SplashableGrid = new PartialMobileStaticGrid<BaseEntity>();

	public ItemDefinition currentFuelType;

	private IOEntity currentFuelSource;

	private HashSet<ISplashable> cachedSplashables = new HashSet<ISplashable>();

	private TimeSince updateSplashableCache;

	private bool forceUpdateSplashables;

	private TimeSince timeSinceFuelTypeRequest;

	private Action DoSplashCB;

	public override bool BlockFluidDraining => currentFuelSource != null;

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.sprinkler = Facepunch.Pool.Get<ProtoBuf.Sprinkler>();
			info.msg.sprinkler.currentFuelType = ((currentFuelType != null) ? currentFuelType.itemid : 0);
		}
	}

	public override int ConsumptionAmount()
	{
		return 2;
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		if (currentEnergy < ConsumptionAmount())
		{
			return 0;
		}
		return ConsumptionAmount();
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		RefreshSprinklerState(inputAmount);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return inputAmount;
	}

	public void DoSplash()
	{
		if (currentFuelType == null)
		{
			if ((float)timeSinceFuelTypeRequest > SplashFrequency * 2f)
			{
				timeSinceFuelTypeRequest = 0f;
				SendChangedToRoot(forceUpdate: true);
			}
			return;
		}
		using (TimeWarning.New("SprinklerSplash"))
		{
			int num = WaterPerSplash;
			using PooledList<ISplashable> pooledList = Facepunch.Pool.Get<PooledList<ISplashable>>();
			Vector3 position = Eyes.position;
			if ((float)updateSplashableCache > SplashFrequency * 4f || forceUpdateSplashables)
			{
				cachedSplashables.Clear();
				forceUpdateSplashables = false;
				updateSplashableCache = 0f;
				Vector3 up = base.transform.up;
				float sprinklerEyeHeightOffset = Server.sprinklerEyeHeightOffset;
				float value = Vector3.Angle(up, Vector3.up) / 180f;
				value = Mathf.Clamp(value, 0.2f, 1f);
				sprinklerEyeHeightOffset *= value;
				Vector3 vector = position + up * (Server.sprinklerRadius * 0.5f);
				Vector3 b = position + up * sprinklerEyeHeightOffset;
				List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
				Vector3 vector2 = Vector3.Lerp(vector, b, 0.5f);
				DynamicObjectsTrigger.transform.position = vector2;
				DynamicObjectsTrigger.transform.up = (vector2 - vector).normalized;
				SplashableGrid.UpdateMobileEntities();
				SplashableGrid.Grid.Query(vector2.x, vector2.z, Server.sprinklerRadius, obj);
				if (obj.Count > 0)
				{
					OBB oBB = new OBB(DynamicObjectsTrigger.transform, new Bounds(DynamicObjectsTrigger.Capsule.center, DynamicObjectsTrigger.Capsule.bounds.extents * 2f));
					foreach (BaseEntity item in obj)
					{
						if (item != null && oBB.Intersects(item.WorldSpaceBounds()) && CanEverSplashEntity(item, out var foundSplashable2) && item.IsVisible(position))
						{
							cachedSplashables.Add(foundSplashable2);
						}
					}
				}
				Facepunch.Pool.FreeUnmanaged(ref obj);
			}
			foreach (ISplashable cachedSplashable in cachedSplashables)
			{
				if (!ObjectEx.IsUnityNull(cachedSplashable) && cachedSplashable.WantsSplash(currentFuelType, num))
				{
					pooledList.Add(cachedSplashable);
				}
			}
			using (TimeWarning.New("UpdateDynamicSplashables"))
			{
				if (DynamicObjectsTrigger.entityContents != null)
				{
					foreach (BaseEntity entityContent in DynamicObjectsTrigger.entityContents)
					{
						if (CanEverSplashEntity(entityContent, out var foundSplashable3) && foundSplashable3.WantsSplash(currentFuelType, num))
						{
							if (DynamicObjectsTrigger.ShouldCheckLineOfSight(entityContent))
							{
								DynamicObjectsTrigger.RecordLineOfSight(entityContent, entityContent.IsVisible(position));
							}
							if (DynamicObjectsTrigger.HasLineOfSight(entityContent))
							{
								pooledList.Add(foundSplashable3);
							}
						}
					}
				}
			}
			if (pooledList.Count > 0)
			{
				int num2 = num / pooledList.Count;
				float num3 = (float)(num % pooledList.Count) / (float)pooledList.Count;
				foreach (ISplashable item2 in pooledList)
				{
					int amount = num2 + ((UnityEngine.Random.value < num3) ? 1 : 0);
					if (!ObjectEx.IsUnityNull(item2) && item2.WantsSplash(currentFuelType, amount))
					{
						int num4 = item2.DoSplash(currentFuelType, amount);
						num -= num4;
						if (num <= 0)
						{
							break;
						}
					}
				}
			}
			if (DecayPerSplash > 0f)
			{
				Hurt(DecayPerSplash);
			}
		}
		Interface.CallHook("OnSprinklerSplashed", this);
		bool CanEverSplashEntity(BaseEntity targetEnt, out ISplashable foundSplashable)
		{
			foundSplashable = null;
			if (targetEnt.isClient)
			{
				return false;
			}
			if (targetEnt is ISplashable splashable)
			{
				if (targetEnt is IOEntity entity && IsConnectedTo(entity, IOEntity.backtracking))
				{
					return false;
				}
				if (targetEnt is BasePlayer && currentFuelType.baseRadioactivity > 0f)
				{
					return false;
				}
				foundSplashable = splashable;
				return true;
			}
			return false;
		}
	}

	public void RefreshSprinklerState()
	{
		RefreshSprinklerState(currentEnergy);
	}

	private void RefreshSprinklerState(int availableFlow)
	{
		bool flag = availableFlow >= ConsumptionAmount();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, flag);
			flagsUpdateScope.Set(Flags.Reserved3, flag && currentFuelType != null && currentFuelType.baseRadioactivity > 0f);
		}
		if (DoSplashCB == null)
		{
			DoSplashCB = DoSplash;
		}
		if (flag)
		{
			if (!IsInvoking(DoSplashCB))
			{
				InvokeRandomized(DoSplashCB, SplashFrequency * 0.5f, SplashFrequency, SplashFrequency * 0.2f);
				forceUpdateSplashables = true;
			}
		}
		else
		{
			if (IsInvoking(DoSplashCB))
			{
				CancelInvoke(DoSplashCB);
			}
			currentFuelSource = null;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		RefreshSprinklerState();
	}

	public override void SetFuelType(ItemDefinition def, IOEntity source)
	{
		base.SetFuelType(def, source);
		if (currentFuelType != def)
		{
			forceUpdateSplashables = true;
		}
		currentFuelType = def;
		currentFuelSource = source;
		RefreshSprinklerState();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.sprinkler != null)
		{
			currentFuelType = ItemManager.FindItemDefinition(info.msg.sprinkler.currentFuelType);
		}
	}
}
