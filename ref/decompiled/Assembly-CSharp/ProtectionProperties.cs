using System.Collections.Generic;
using Rust;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Protection Properties")]
public class ProtectionProperties : BaseScriptableObject
{
	[TextArea]
	public string comments;

	[Range(0f, 100f)]
	public float density = 1f;

	[ArrayIndexIsEnumRanged(enumType = typeof(DamageType), min = -4f, max = 3f)]
	public float[] amounts = new float[28];

	private static ProtectionProperties _immortalProtection;

	private static ProtectionProperties _immortalProtectionLowDensity;

	public static ProtectionProperties immortalProtection
	{
		get
		{
			if (_immortalProtection == null)
			{
				_immortalProtection = FileSystem.Load<ProtectionProperties>("Assets/Content/Properties/Protection/Immortal.asset");
			}
			return _immortalProtection;
		}
	}

	public static ProtectionProperties immortalProtectionLowDensity
	{
		get
		{
			if (_immortalProtectionLowDensity == null)
			{
				_immortalProtectionLowDensity = FileSystem.Load<ProtectionProperties>("Assets/Content/Properties/Protection/ImmortalLowDensity.asset");
			}
			return _immortalProtectionLowDensity;
		}
	}

	public void OnValidate()
	{
		if (amounts.Length >= 28)
		{
			return;
		}
		float[] array = new float[28];
		for (int i = 0; i < array.Length; i++)
		{
			if (i >= amounts.Length)
			{
				if (i == 21)
				{
					array[i] = amounts[9];
				}
			}
			else
			{
				array[i] = amounts[i];
			}
		}
		amounts = array;
	}

	public void Clear()
	{
		for (int i = 0; i < amounts.Length; i++)
		{
			amounts[i] = 0f;
		}
	}

	public void Add(float amount)
	{
		for (int i = 0; i < amounts.Length; i++)
		{
			amounts[i] += amount;
		}
	}

	public void Add(DamageType index, float amount)
	{
		amounts[(int)index] += amount;
	}

	public void Add(ProtectionProperties other, float scale)
	{
		for (int i = 0; i < Mathf.Min(other.amounts.Length, amounts.Length); i++)
		{
			amounts[i] += other.amounts[i] * scale;
		}
	}

	public void Add(List<Item> items, HitArea area = (HitArea)(-1), float multiplier = 1f)
	{
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			ItemModWearable component = item.info.GetComponent<ItemModWearable>();
			if (!(component == null) && component.ProtectsArea(area))
			{
				component.CollectProtection(item, this, multiplier);
			}
		}
	}

	public void Multiply(float multiplier)
	{
		for (int i = 0; i < amounts.Length; i++)
		{
			amounts[i] *= multiplier;
		}
	}

	public void Multiply(DamageType index, float multiplier)
	{
		amounts[(int)index] *= multiplier;
	}

	public void Scale(DamageTypeList damageList, float ProtectionAmount = 1f)
	{
		for (int i = 0; i < amounts.Length; i++)
		{
			if (amounts[i] != 0f)
			{
				damageList.Scale((DamageType)i, 1f - Mathf.Clamp(amounts[i] * ProtectionAmount, -1f, 1f));
			}
		}
	}

	public float Get(DamageType damageType)
	{
		return amounts[(int)damageType];
	}

	public static bool IsImmortal(BaseCombatEntity entity)
	{
		if (entity.baseProtection != null)
		{
			if (!(entity.baseProtection == immortalProtection))
			{
				return entity.baseProtection == immortalProtectionLowDensity;
			}
			return true;
		}
		return false;
	}
}
