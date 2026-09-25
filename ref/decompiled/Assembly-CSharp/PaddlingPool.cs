using Facepunch;
using ProtoBuf;
using UnityEngine;

public class PaddlingPool : LiquidContainer, ISplashable
{
	public const Flags FilledUp = Flags.Reserved4;

	public Transform poolWaterVolume;

	public GameObject poolWaterVisual;

	public float minimumWaterHeight;

	public float maximumWaterHeight = 1f;

	public WaterVolume waterVolume;

	public bool alignWaterUp = true;

	public GameObjectRef destroyedWithWaterEffect;

	public Transform destroyedWithWaterEffectPos;

	public Collider requireLookAt;

	private float lastFillAmount = -1f;

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		float normalisedFillLevel = GetNormalisedFillLevel();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved4, normalisedFillLevel >= 1f);
		}
		UpdatePoolFillAmount(normalisedFillLevel);
		SendNetworkUpdate();
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		float normalisedFillLevel = GetNormalisedFillLevel();
		UpdatePoolFillAmount(normalisedFillLevel);
		SendNetworkUpdate();
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (base.IsDestroyed)
		{
			return false;
		}
		if (!HasFlag(Flags.Reserved4) && splashType != null)
		{
			for (int i = 0; i < ValidItems.Length; i++)
			{
				if (ValidItems[i] != null && ValidItems[i].itemid == splashType.itemid)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		int amount2 = Mathf.Clamp(Mathf.RoundToInt((float)amount * 0.66f), 1, amount);
		base.inventory.AddItem(splashType, amount2, 0uL);
		return amount;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.WaterPool = Pool.Get<WaterPool>();
		info.msg.WaterPool.fillAmount = GetNormalisedFillLevel();
	}

	private float GetNormalisedFillLevel()
	{
		if (base.inventory.itemList.Count <= 0 || base.inventory.itemList[0] == null)
		{
			return 0f;
		}
		return (float)base.inventory.itemList[0].amount / (float)maxStackSize;
	}

	private void UpdatePoolFillAmount(float normalisedAmount)
	{
		poolWaterVisual.gameObject.SetActive(normalisedAmount > 0f);
		waterVolume.waterEnabled = normalisedAmount > 0f;
		float y = Mathf.Lerp(minimumWaterHeight, maximumWaterHeight, normalisedAmount);
		Vector3 localPosition = poolWaterVolume.localPosition;
		localPosition.y = y;
		poolWaterVolume.localPosition = localPosition;
		if (alignWaterUp)
		{
			poolWaterVolume.up = Vector3.up;
		}
		if (normalisedAmount > 0f && lastFillAmount < normalisedAmount && waterVolume.entityContents != null)
		{
			foreach (BaseEntity entityContent in waterVolume.entityContents)
			{
				if (entityContent is IPoolVehicle poolVehicle)
				{
					poolVehicle.WakeUp();
				}
			}
		}
		lastFillAmount = normalisedAmount;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (!base.isServer)
		{
			return;
		}
		using PooledList<BaseEntity> pooledList = Pool.Get<PooledList<BaseEntity>>();
		OBB oBB = WorldSpaceBounds();
		oBB.extents.y += 0.2f;
		Vis.Entities(oBB, pooledList, 32768);
		foreach (BaseEntity item in pooledList)
		{
			if (item is IPoolVehicle poolVehicle)
			{
				poolVehicle.OnPoolDestroyed();
			}
		}
	}
}
