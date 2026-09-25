using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PlayerMetabolism : BaseMetabolism<BasePlayer>
{
	public const float HotThreshold = 40f;

	public const float ColdThreshold = 5f;

	public const float OxygenHurtThreshold = 0.5f;

	public const float OxygenDepleteTime = 10f;

	public const float OxygenRefillTime = 1f;

	public MetabolismAttribute temperature = new MetabolismAttribute();

	public MetabolismAttribute poison = new MetabolismAttribute();

	public MetabolismAttribute radiation_level = new MetabolismAttribute();

	public MetabolismAttribute radiation_poison = new MetabolismAttribute();

	public MetabolismAttribute wetness = new MetabolismAttribute();

	public MetabolismAttribute dirtyness = new MetabolismAttribute();

	public MetabolismAttribute oxygen = new MetabolismAttribute();

	public MetabolismAttribute bleeding = new MetabolismAttribute();

	public MetabolismAttribute comfort = new MetabolismAttribute();

	public MetabolismAttribute pending_health = new MetabolismAttribute();

	private const int BIT_CALORIES = 0;

	private const int BIT_HYDRATION = 1;

	private const int BIT_HEARTRATE = 2;

	private const int BIT_TEMPERATURE = 3;

	private const int BIT_RADIATION_LEVEL = 4;

	private const int BIT_RADIATION_POISON = 5;

	private const int BIT_WETNESS = 6;

	private const int BIT_OXYGEN = 7;

	private const int BIT_BLEEDING = 8;

	private const int BIT_COMFORT = 9;

	private const int BIT_POISON = 10;

	private const int BIT_PENDING_HEALTH = 11;

	private const int BIT_HEALTH = 12;

	public bool isDirty;

	private float lastConsumeTime;

	private float _lastSentHealth;

	private bool _needsFullSnapshot = true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerMetabolism.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Reset()
	{
		base.Reset();
		poison.Reset();
		radiation_level.Reset();
		radiation_poison.Reset();
		temperature.Reset();
		oxygen.Reset();
		bleeding.Reset();
		wetness.Reset();
		dirtyness.Reset();
		comfort.Reset();
		pending_health.Reset();
		lastConsumeTime = float.NegativeInfinity;
		isDirty = true;
	}

	public override bool ServerUpdate(BaseCombatEntity ownerEntity, float delta)
	{
		bool num = base.ServerUpdate(ownerEntity, delta);
		Interface.CallHook("OnPlayerMetabolize", this, ownerEntity, delta);
		bool flag = num;
		if ((flag || isDirty || _needsFullSnapshot) && owner.IsConnected)
		{
			using (TimeWarning.New("PlayerMetabolism.ServerUpdate"))
			{
				SendChanges();
			}
		}
		return flag;
	}

	private ushort GetChangedMask()
	{
		ushort num = 0;
		if (calories.HasGreatlyChanged())
		{
			num = (ushort)(num | 1u);
		}
		if (hydration.HasGreatlyChanged())
		{
			num = (ushort)(num | 2u);
		}
		if (heartrate.HasChanged())
		{
			num = (ushort)(num | 4u);
		}
		if (temperature.HasGreatlyChanged())
		{
			num = (ushort)(num | 8u);
		}
		if (radiation_level.HasChanged())
		{
			num = (ushort)(num | 0x10u);
		}
		if (radiation_poison.HasChanged())
		{
			num = (ushort)(num | 0x20u);
		}
		if (wetness.HasGreatlyChanged())
		{
			num = (ushort)(num | 0x40u);
		}
		if (oxygen.HasChanged())
		{
			num = (ushort)(num | 0x80u);
		}
		if (bleeding.HasChanged())
		{
			num = (ushort)(num | 0x100u);
		}
		if (comfort.HasGreatlyChanged())
		{
			num = (ushort)(num | 0x200u);
		}
		if (poison.HasChanged())
		{
			num = (ushort)(num | 0x400u);
		}
		if (pending_health.HasChanged())
		{
			num = (ushort)(num | 0x800u);
		}
		if ((bool)owner)
		{
			float num2 = owner.Health();
			if (Mathf.Abs(num2 - _lastSentHealth) > 0.01f)
			{
				num = (ushort)(num | 0x1000u);
				_lastSentHealth = num2;
			}
		}
		return num;
	}

	protected override void DoMetabolismDamage(BaseCombatEntity ownerEntity, float delta)
	{
		if (owner.IsConnected)
		{
			base.DoMetabolismDamage(ownerEntity, delta);
			if (temperature.value < -20f)
			{
				owner.Hurt(Mathf.InverseLerp(1f, -50f, temperature.value) * delta * 1f, DamageType.Cold);
			}
			else if (temperature.value < -10f)
			{
				owner.Hurt(Mathf.InverseLerp(1f, -50f, temperature.value) * delta * 0.3f, DamageType.Cold);
			}
			else if (temperature.value < 1f)
			{
				owner.Hurt(Mathf.InverseLerp(1f, -50f, temperature.value) * delta * 0.1f, DamageType.Cold);
			}
			if (temperature.value > 60f)
			{
				owner.Hurt(Mathf.InverseLerp(60f, 200f, temperature.value) * delta * 5f, DamageType.Heat);
			}
			if (!owner.IsGod() && bleeding.value > 0f)
			{
				float num = delta * (1f / 3f);
				owner.Hurt(num, DamageType.Bleeding);
				bleeding.Subtract(num);
			}
			if (!owner.IsGod() && poison.value > 0f)
			{
				owner.Hurt(poison.value * delta * 0.1f, DamageType.Poison);
			}
			if (ConVar.Server.radiation && radiation_poison.value > 0f)
			{
				float num2 = (1f + Mathf.Clamp01(radiation_poison.value / 25f) * 5f) * (delta / 5f);
				owner.Hurt(num2, DamageType.Radiation);
				radiation_poison.Subtract(num2);
			}
		}
		if (oxygen.value < 0.5f)
		{
			float num3 = (owner.IsConnected ? 1f : 0.01f);
			owner.Hurt(Mathf.InverseLerp(0.5f, 0f, oxygen.value) * delta * 20f * num3, DamageType.Drowned, null, useProtection: false);
		}
	}

	public bool SignificantBleeding()
	{
		return bleeding.value > 0f;
	}

	public void ForceUpdateWorkbenchFlags()
	{
		owner.InvalidateWorkbenchCache();
		UpdateWorkbenchFlags();
	}

	private void UpdateWorkbenchFlags()
	{
		float currentCraftLevel = owner.currentCraftLevel;
		owner.SetPlayerFlag(BasePlayer.PlayerFlags.Workbench1, currentCraftLevel == 1f);
		owner.SetPlayerFlag(BasePlayer.PlayerFlags.Workbench2, currentCraftLevel == 2f);
		owner.SetPlayerFlag(BasePlayer.PlayerFlags.Workbench3, currentCraftLevel == 3f);
		owner.SendActiveWorkbenchIfChanged();
	}

	protected override void RunMetabolism(BaseCombatEntity ownerEntity, float delta)
	{
		if (Interface.CallHook("OnRunPlayerMetabolism", this, ownerEntity, delta) != null)
		{
			return;
		}
		if (owner.IsConnected)
		{
			BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
			float num = owner.currentTemperature;
			float fTarget = owner.currentComfort;
			UpdateWorkbenchFlags();
			owner.SetPlayerFlag(BasePlayer.PlayerFlags.SafeZone, owner.InSafeZone());
			owner.SetPlayerFlag(BasePlayer.PlayerFlags.CombatZone, owner.InSafeCombatZone());
			owner.SetPlayerFlag(BasePlayer.PlayerFlags.NoRespawnZone, owner.InNoRespawnZone());
			owner.SetPlayerFlag(BasePlayer.PlayerFlags.ModifyClan, Clan.enabled && Clan.editsRequireClanTable && owner.CanModifyClan());
			bool num2 = activeGameMode == null || activeGameMode.allowTemperature;
			if (owner.IsInTutorial)
			{
				num = 25f;
			}
			if (num2)
			{
				float num3 = num + GetCoreTempAdjustment() - DeltaWet() * 34f;
				float num4 = Mathf.Clamp(owner.baseProtection.amounts[18] * 1.5f, -1f, 1f);
				float num5 = Mathf.InverseLerp(20f, -50f, num);
				float num6 = Mathf.InverseLerp(20f, 30f, num);
				float fTarget2 = Mathf.Clamp(num3 + num5 * 70f * num4 + num6 * 10f * Mathf.Abs(num4) + heartrate.value * 5f, GetCoreTempMin(), GetCoreTempMax());
				temperature.MoveTowards(fTarget2, delta * 5f);
			}
			else
			{
				temperature.value = 25f;
			}
			if (temperature.value >= 40f)
			{
				fTarget = 0f;
			}
			comfort.MoveTowards(fTarget, delta / 5f);
			float num7 = 0.6f + 0.4f * comfort.value;
			if (calories.value > 100f && owner.healthFraction < num7 && radiation_poison.Fraction() < 0.25f && owner.SecondsSinceAttacked > 10f && !SignificantBleeding() && temperature.value >= 10f && hydration.value > 40f)
			{
				float num8 = Mathf.InverseLerp(calories.min, calories.max, calories.value);
				float num9 = 5f;
				float num10 = num9 * owner.MaxHealth() * 0.8f / 600f;
				num10 += num10 * num8 * 0.5f;
				float num11 = num10 / num9;
				num11 += num11 * comfort.value * 6f;
				ownerEntity.Heal(num11 * delta);
				calories.Subtract(num10 * delta);
				hydration.Subtract(num10 * delta * 0.2f);
			}
			float num12 = owner.estimatedSpeed2D / owner.GetMaxSpeed() * 0.75f;
			float fTarget3 = Mathf.Clamp(0.05f + num12, 0f, 1f);
			heartrate.MoveTowards(fTarget3, delta * 0.1f);
			if (!owner.IsGod())
			{
				float num13 = heartrate.Fraction() * 0.375f;
				calories.MoveTowards(0f, delta * num13);
				float num14 = 1f / 120f;
				num14 += Mathf.InverseLerp(40f, 60f, temperature.value) * (1f / 12f);
				num14 += heartrate.value * (1f / 15f);
				hydration.MoveTowards(0f, delta * num14);
			}
			bool b = hydration.Fraction() <= 0f || radiation_poison.value >= 100f;
			owner.SetPlayerFlag(BasePlayer.PlayerFlags.NoSprint, b);
			if (temperature.value > 40f)
			{
				hydration.Add(Mathf.InverseLerp(40f, 200f, temperature.value) * delta * -1f);
			}
			if (temperature.value < 10f)
			{
				float num15 = Mathf.InverseLerp(20f, -100f, temperature.value);
				heartrate.MoveTowards(Mathf.Lerp(0.2f, 1f, num15), delta * 2f * num15);
			}
			float f = 0f;
			float f2 = 0f;
			if (owner.IsOutside(owner.eyes.position))
			{
				f = Climate.GetRain(owner.eyes.position) * Weather.wetness_rain;
				f2 = Climate.GetSnow(owner.eyes.position) * Weather.wetness_snow;
			}
			bool flag = owner.baseProtection.amounts[4] > 0f;
			float currentEnvironmentalWetness = owner.currentEnvironmentalWetness;
			currentEnvironmentalWetness = Mathf.Clamp(currentEnvironmentalWetness, 0f, 0.8f);
			float num16 = owner.WaterFactor();
			if (!flag && num16 > 0f)
			{
				wetness.value = Mathf.Max(wetness.value, Mathf.Clamp(num16, wetness.min, wetness.max));
			}
			float num17 = Mathx.Max(wetness.value, f, f2, currentEnvironmentalWetness);
			num17 = Mathf.Min(num17, flag ? 0f : num17);
			wetness.MoveTowards(num17, delta * 0.05f);
			if (num16 < wetness.value && currentEnvironmentalWetness <= 0f)
			{
				wetness.MoveTowards(0f, delta * 0.2f * Mathf.InverseLerp(0f, 100f, num));
			}
			poison.MoveTowards(0f, delta * (5f / 9f));
			if (wetness.Fraction() > 0.4f && owner.estimatedSpeed > 0.25f && radiation_level.Fraction() == 0f)
			{
				radiation_poison.Subtract(radiation_poison.value * 0.2f * wetness.Fraction() * delta * 0.2f);
			}
			if (ConVar.Server.radiation)
			{
				if (!owner.IsGod())
				{
					float radiationAfterProtection = Radiation.GetRadiationAfterProtection(owner.modifiers.GetValue(Modifier.ModifierType.Radiation), ownerEntity.RadiationProtection());
					radiation_level.value = owner.radiationLevel + radiationAfterProtection;
					if (radiation_level.value > 0f)
					{
						radiation_poison.Add(radiation_level.value * delta);
					}
				}
				else if (radiation_level.value > 0f)
				{
					radiation_level.value = 0f;
					radiation_poison.value = 0f;
				}
			}
			if (pending_health.value > 0f)
			{
				float num18 = 1f + owner.modifiers.GetValue(Modifier.ModifierType.MetabolismBooster);
				float num19 = Mathf.Min(1f * delta * num18, pending_health.value);
				ownerEntity.Heal(num19);
				if (ownerEntity.healthFraction == 1f)
				{
					pending_health.value = 0f;
				}
				else
				{
					pending_health.Subtract(num19);
				}
			}
		}
		float num20 = owner.AirFactor();
		float num21 = ((num20 > oxygen.value) ? 1f : 0.1f);
		oxygen.MoveTowards(num20, delta * num21);
	}

	private float GetCoreTempAdjustment()
	{
		if (owner == null)
		{
			return 0f;
		}
		PlayerModifiers modifiers = owner.modifiers;
		if (modifiers == null)
		{
			return 0f;
		}
		return 0f + modifiers.GetValue(Modifier.ModifierType.Warming) + modifiers.GetValue(Modifier.ModifierType.Cooling);
	}

	private float GetCoreTempMin()
	{
		if (owner == null)
		{
			return temperature.min;
		}
		PlayerModifiers modifiers = owner.modifiers;
		if (modifiers == null)
		{
			return temperature.min;
		}
		return modifiers.GetValue(Modifier.ModifierType.CoreTemperatureMinAdjustment, temperature.min);
	}

	private float GetCoreTempMax()
	{
		if (owner == null)
		{
			return temperature.max;
		}
		PlayerModifiers modifiers = owner.modifiers;
		if (modifiers == null)
		{
			return temperature.max;
		}
		return modifiers.GetValue(Modifier.ModifierType.CoreTemperatureMaxAdjustment, temperature.max);
	}

	private float DeltaHot()
	{
		return Mathf.InverseLerp(20f, 100f, temperature.value);
	}

	private float DeltaCold()
	{
		return Mathf.InverseLerp(20f, -50f, temperature.value);
	}

	private float DeltaWet()
	{
		return wetness.value;
	}

	public void UseHeart(float frate)
	{
		if (heartrate.value > frate)
		{
			heartrate.Add(frate);
		}
		else
		{
			heartrate.value = frate;
		}
	}

	public void MarkNeedsFullSnapshot()
	{
		_needsFullSnapshot = true;
	}

	public void SendChanges()
	{
		ushort changedMask = GetChangedMask();
		bool flag = _needsFullSnapshot || isDirty;
		_needsFullSnapshot = false;
		isDirty = false;
		if (!flag && changedMask == 0)
		{
			return;
		}
		using ProtoBuf.PlayerMetabolism arg = (flag ? Save() : SaveDelta(changedMask));
		base.baseEntity.ClientRPC(RpcTarget.FromFlags(RpcTarget.RpcTargetFlags.All, "UpdateMetabolism", base.baseEntity), arg);
	}

	public void ForceSendChangesToSpectators()
	{
		isDirty = false;
		using ProtoBuf.PlayerMetabolism arg = Save();
		base.baseEntity.ClientRPC(RpcTarget.FromFlags(RpcTarget.RpcTargetFlags.Spectators, "UpdateMetabolism", base.baseEntity), arg);
	}

	public override void ApplyChange(MetabolismAttribute.Type type, float amount, float time)
	{
		MetabolismAttribute metabolismAttribute = FindAttribute(type);
		if (metabolismAttribute != null)
		{
			metabolismAttribute.Add(amount);
			isDirty = true;
		}
	}

	public bool CanConsume()
	{
		if ((bool)owner && owner.IsHeadUnderwater())
		{
			return false;
		}
		return UnityEngine.Time.time - lastConsumeTime > 1f;
	}

	public void MarkConsumption()
	{
		lastConsumeTime = UnityEngine.Time.time;
	}

	public ProtoBuf.PlayerMetabolism Save()
	{
		ProtoBuf.PlayerMetabolism playerMetabolism = Facepunch.Pool.Get<ProtoBuf.PlayerMetabolism>();
		playerMetabolism.calories = calories.value;
		playerMetabolism.hydration = hydration.value;
		playerMetabolism.heartrate = heartrate.value;
		playerMetabolism.temperature = temperature.value;
		playerMetabolism.radiation_level = radiation_level.value;
		playerMetabolism.radiation_poisoning = radiation_poison.value;
		playerMetabolism.wetness = wetness.value;
		playerMetabolism.dirtyness = dirtyness.value;
		playerMetabolism.oxygen = oxygen.value;
		playerMetabolism.bleeding = bleeding.value;
		playerMetabolism.comfort = comfort.value;
		playerMetabolism.poison = poison.value;
		playerMetabolism.pending_health = pending_health.value;
		if ((bool)owner)
		{
			playerMetabolism.health = owner.Health();
		}
		return playerMetabolism;
	}

	private ProtoBuf.PlayerMetabolism SaveDelta(ushort mask)
	{
		ProtoBuf.PlayerMetabolism playerMetabolism = Facepunch.Pool.Get<ProtoBuf.PlayerMetabolism>();
		playerMetabolism.changed_mask = mask;
		if (((uint)mask & (true ? 1u : 0u)) != 0)
		{
			playerMetabolism.calories = calories.value;
		}
		if ((mask & 2u) != 0)
		{
			playerMetabolism.hydration = hydration.value;
		}
		if ((mask & 4u) != 0)
		{
			playerMetabolism.heartrate = heartrate.value;
		}
		if ((mask & 8u) != 0)
		{
			playerMetabolism.temperature = temperature.value;
		}
		if ((mask & 0x10u) != 0)
		{
			playerMetabolism.radiation_level = radiation_level.value;
		}
		if ((mask & 0x20u) != 0)
		{
			playerMetabolism.radiation_poisoning = radiation_poison.value;
		}
		if ((mask & 0x40u) != 0)
		{
			playerMetabolism.wetness = wetness.value;
		}
		if ((mask & 0x80u) != 0)
		{
			playerMetabolism.oxygen = oxygen.value;
		}
		if ((mask & 0x100u) != 0)
		{
			playerMetabolism.bleeding = bleeding.value;
		}
		if ((mask & 0x200u) != 0)
		{
			playerMetabolism.comfort = comfort.value;
		}
		if ((mask & 0x400u) != 0)
		{
			playerMetabolism.poison = poison.value;
		}
		if ((mask & 0x800u) != 0)
		{
			playerMetabolism.pending_health = pending_health.value;
		}
		if ((mask & 0x1000u) != 0)
		{
			playerMetabolism.health = (owner ? owner.Health() : 0f);
		}
		return playerMetabolism;
	}

	public void Load(ProtoBuf.PlayerMetabolism s)
	{
		uint changed_mask = s.changed_mask;
		if (changed_mask == 0)
		{
			calories.SetValue(s.calories);
			hydration.SetValue(s.hydration);
			comfort.SetValue(s.comfort);
			heartrate.value = s.heartrate;
			temperature.value = s.temperature;
			radiation_level.value = s.radiation_level;
			radiation_poison.value = s.radiation_poisoning;
			wetness.value = s.wetness;
			dirtyness.value = s.dirtyness;
			oxygen.value = s.oxygen;
			bleeding.value = s.bleeding;
			poison.value = s.poison;
			pending_health.value = s.pending_health;
			if ((bool)owner)
			{
				owner.health = s.health;
			}
		}
		else
		{
			if ((changed_mask & (true ? 1u : 0u)) != 0)
			{
				calories.SetValue(s.calories);
			}
			if ((changed_mask & 2u) != 0)
			{
				hydration.SetValue(s.hydration);
			}
			if ((changed_mask & 4u) != 0)
			{
				heartrate.value = s.heartrate;
			}
			if ((changed_mask & 8u) != 0)
			{
				temperature.value = s.temperature;
			}
			if ((changed_mask & 0x10u) != 0)
			{
				radiation_level.value = s.radiation_level;
			}
			if ((changed_mask & 0x20u) != 0)
			{
				radiation_poison.value = s.radiation_poisoning;
			}
			if ((changed_mask & 0x40u) != 0)
			{
				wetness.value = s.wetness;
			}
			if ((changed_mask & 0x80u) != 0)
			{
				oxygen.value = s.oxygen;
			}
			if ((changed_mask & 0x100u) != 0)
			{
				bleeding.value = s.bleeding;
			}
			if ((changed_mask & 0x200u) != 0)
			{
				comfort.SetValue(s.comfort);
			}
			if ((changed_mask & 0x400u) != 0)
			{
				poison.value = s.poison;
			}
			if ((changed_mask & 0x800u) != 0)
			{
				pending_health.value = s.pending_health;
			}
			if ((changed_mask & 0x1000u) != 0 && (bool)owner)
			{
				owner.health = s.health;
			}
		}
		_ = (bool)owner;
	}

	public void SetAttribute(MetabolismAttribute.Type type, float amount)
	{
		MetabolismAttribute metabolismAttribute = FindAttribute(type);
		if (metabolismAttribute != null)
		{
			float num = metabolismAttribute.value - amount;
			metabolismAttribute.Add(0f - num);
			isDirty = true;
		}
	}

	public override MetabolismAttribute FindAttribute(MetabolismAttribute.Type type)
	{
		return type switch
		{
			MetabolismAttribute.Type.Poison => poison, 
			MetabolismAttribute.Type.Bleeding => bleeding, 
			MetabolismAttribute.Type.Radiation => radiation_poison, 
			MetabolismAttribute.Type.HealthOverTime => pending_health, 
			_ => base.FindAttribute(type), 
		};
	}
}
