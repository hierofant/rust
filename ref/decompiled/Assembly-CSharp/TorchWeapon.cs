using System;
using Rust;
using UnityEngine;

public class TorchWeapon : ToggleableLightWeapon
{
	[NonSerialized]
	public const float FuelTickAmount = 1f / 12f;

	[Header("TorchWeapon")]
	public GameObjectRef litStrikeFX;

	public const Flags IsInHolder = Flags.Reserved1;

	public override void GetAttackStats(HitInfo info)
	{
		base.GetAttackStats(info);
		if (HasFlag(Flags.On))
		{
			info.damageTypes.Add(DamageType.Heat, 1f);
		}
	}

	public override float GetConditionLoss()
	{
		return base.GetConditionLoss() + (HasFlag(Flags.On) ? 6f : 0f);
	}

	public override void SetIsOn(bool isOn)
	{
		if (isOn)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: true);
				flagsUpdateScope.Set(Flags.OnFire, b: true);
			}
			InvokeRepeating(UseFuel, 1f, 1f);
		}
		else
		{
			using (FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope2.Set(Flags.On, b: false);
				flagsUpdateScope2.Set(Flags.OnFire, b: false);
			}
			CancelInvoke(UseFuel);
		}
	}

	public void UseFuel()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			ownerItem.LoseCondition(1f / 12f);
			SingletonComponent<NpcFireManager>.Instance.Move(this);
		}
	}

	public override string GetStrikeEffectPath(string materialName)
	{
		for (int i = 0; i < materialStrikeFX.Count; i++)
		{
			if (materialStrikeFX[i].materialName == materialName && materialStrikeFX[i].fx.isValid)
			{
				return materialStrikeFX[i].fx.resourcePath;
			}
		}
		if (HasFlag(Flags.On) && litStrikeFX.isValid)
		{
			return litStrikeFX.resourcePath;
		}
		return strikeFX.resourcePath;
	}
}
