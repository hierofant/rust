using System.Collections.Generic;
using ConVar;
using Rust;
using UnityEngine;

public abstract class BaseModifiers<T> : EntityComponent<T> where T : BaseCombatEntity
{
	public List<Modifier> All = new List<Modifier>();

	protected Dictionary<Modifier.ModifierType, float> totalValues = new Dictionary<Modifier.ModifierType, float>();

	protected Dictionary<Modifier.ModifierType, float> modifierVariables = new Dictionary<Modifier.ModifierType, float>();

	protected Dictionary<Modifier.ModifierSource, int> sourceCounts = new Dictionary<Modifier.ModifierSource, int>();

	protected T owner;

	protected bool dirty = true;

	protected double timeSinceLastTick;

	protected double lastTickTime;

	public int ActiveModifierCount => All.Count;

	public int ActiveTeaCount
	{
		get
		{
			int num = 0;
			foreach (Modifier item in All)
			{
				if (item != null && item.Source == Modifier.ModifierSource.Tea)
				{
					num++;
				}
			}
			return num;
		}
	}

	public void Add(List<ModifierDefintion> modDefs, float effectScale = 1f, float durationScale = 1f)
	{
		foreach (ModifierDefintion modDef in modDefs)
		{
			if (IsCompatible(modDef.type))
			{
				Add(modDef, effectScale, durationScale);
			}
		}
	}

	protected virtual bool IsCompatible(Modifier.ModifierType modType)
	{
		return true;
	}

	protected void Add(ModifierDefintion def, float effectScale = 1f, float durationScale = 1f)
	{
		Modifier modifier = new Modifier();
		modifier.Init(def.type, def.source, def.value * effectScale, def.duration * durationScale, def.duration * durationScale);
		Add(modifier);
	}

	protected void Add(Modifier modifier)
	{
		if (!CanAdd(modifier))
		{
			return;
		}
		int maxModifierCount = GetMaxModifierCount(modifier);
		if (GetActiveCount(modifier.Type, modifier.Source) >= maxModifierCount)
		{
			Modifier shortestLifeModifier = GetShortestLifeModifier(modifier.Type, modifier.Source);
			if (shortestLifeModifier == null)
			{
				return;
			}
			Remove(shortestLifeModifier);
		}
		All.Add(modifier);
		if (!modifier.IsHiddenModifier())
		{
			AdjustSourceCount(modifier.Source, 1);
		}
		ApplyModifierValue(modifier);
		SetDirty(flag: true);
	}

	private void AdjustSourceCount(Modifier.ModifierSource source, int adjustBy)
	{
		sourceCounts.TryGetValue(source, out var value);
		sourceCounts[source] = value + 1;
	}

	public int GetSourceCount(Modifier.ModifierSource source)
	{
		return sourceCounts.GetValueOrDefault(source, 0);
	}

	private void ApplyModifierValue(Modifier modifier)
	{
		float value = modifier.Value;
		if (!totalValues.ContainsKey(modifier.Type))
		{
			totalValues.Add(modifier.Type, value);
		}
		else
		{
			totalValues[modifier.Type] += value;
		}
		totalValues[modifier.Type] = GetClampedValue(modifier, totalValues[modifier.Type]);
	}

	private bool CanAdd(Modifier modifier)
	{
		if (All.Contains(modifier))
		{
			return false;
		}
		if (modifier.HasNegativeSource() && owner != null && owner is BasePlayer basePlayer && basePlayer.IsGod())
		{
			return false;
		}
		return true;
	}

	private int GetMaxModifiersForSourceType(Modifier.ModifierSource source)
	{
		if (source == Modifier.ModifierSource.Tea || source == Modifier.ModifierSource.Interaction)
		{
			return 1;
		}
		return int.MaxValue;
	}

	protected virtual int GetMaxModifierCount(Modifier modifier)
	{
		if (modifier == null)
		{
			return 0;
		}
		switch (modifier.Source)
		{
		case Modifier.ModifierSource.Tea:
		case Modifier.ModifierSource.Interaction:
		case Modifier.ModifierSource.MedicalSyringe:
			return 1;
		default:
			return int.MaxValue;
		}
	}

	protected virtual float GetClampedValue(Modifier modifier, float value)
	{
		return value;
	}

	private int GetActiveCount(Modifier.ModifierType type, Modifier.ModifierSource source)
	{
		int num = 0;
		foreach (Modifier item in All)
		{
			if (item.Type == type && item.Source == source)
			{
				num++;
			}
		}
		return num;
	}

	private Modifier GetShortestLifeModifier(Modifier.ModifierType type, Modifier.ModifierSource source)
	{
		Modifier modifier = null;
		foreach (Modifier item in All)
		{
			if (item.Type == type && item.Source == source)
			{
				if (modifier == null)
				{
					modifier = item;
				}
				else if (item.TimeRemaining < modifier.TimeRemaining)
				{
					modifier = item;
				}
			}
		}
		return modifier;
	}

	private void Remove(Modifier toRemove)
	{
		bool flag = false;
		bool flag2 = false;
		Modifier.ModifierType type = toRemove.Type;
		foreach (Modifier item in All)
		{
			if (toRemove == item)
			{
				flag = true;
			}
			else if (item.Type == type)
			{
				flag2 = true;
			}
		}
		if (flag)
		{
			All.Remove(toRemove);
			AdjustSourceCount(toRemove.Source, -1);
			if (!flag2)
			{
				totalValues.Remove(toRemove.Type);
			}
			SetDirty(flag: true);
		}
	}

	public void RemoveAll()
	{
		All.Clear();
		totalValues.Clear();
		sourceCounts.Clear();
		SetDirty(flag: true);
	}

	public void RemoveFromSource(Modifier.ModifierSource source)
	{
		for (int num = All.Count - 1; num >= 0; num--)
		{
			Modifier modifier = All[num];
			if (modifier != null && modifier.Source == source)
			{
				Remove(modifier);
			}
		}
	}

	public void RemoveAllExceptFromSource(Modifier.ModifierSource source)
	{
		for (int num = All.Count - 1; num >= 0; num--)
		{
			Modifier modifier = All[num];
			if (modifier != null && modifier.Source != source)
			{
				Remove(modifier);
			}
		}
	}

	public float GetValue(Modifier.ModifierType type, float defaultValue = 0f)
	{
		float num = 1f;
		if (IsModifierCompatibleWithDigestionBoost(type))
		{
			num = GetValue(Modifier.ModifierType.DigestionBoost, 1f);
		}
		if (totalValues.TryGetValue(type, out var value))
		{
			return value * num;
		}
		return defaultValue * num;
	}

	public void SetValue(Modifier.ModifierSource source, Modifier.ModifierType type, float value)
	{
		Modifier modifier = new Modifier();
		bool flag = false;
		foreach (Modifier item in All)
		{
			if (item != null && item.Source == source && item.Type == type)
			{
				modifier.Init(item.Type, item.Source, value, item.Duration, item.TimeRemaining);
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Add(modifier);
		}
	}

	public float GetVariableValue(Modifier.ModifierType type, float defaultValue)
	{
		if (modifierVariables.TryGetValue(type, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public void SetVariableValue(Modifier.ModifierType type, float value)
	{
		if (modifierVariables.TryGetValue(type, out var _))
		{
			modifierVariables[type] = value;
		}
		else
		{
			modifierVariables.Add(type, value);
		}
	}

	public void RemoveVariable(Modifier.ModifierType type)
	{
		modifierVariables.Remove(type);
	}

	protected virtual void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			owner = null;
		}
	}

	public void SetDirty(bool flag)
	{
		dirty = flag;
	}

	public virtual void ServerInit(T owner)
	{
		this.owner = owner;
		ResetTicking();
		RemoveAll();
	}

	public void ResetTicking()
	{
		lastTickTime = UnityEngine.Time.timeAsDouble;
		timeSinceLastTick = 0.0;
	}

	public virtual void ServerUpdate(BaseCombatEntity ownerEntity)
	{
		double num = UnityEngine.Time.timeAsDouble - lastTickTime;
		lastTickTime = UnityEngine.Time.timeAsDouble;
		timeSinceLastTick += num;
		if (!(timeSinceLastTick <= (double)ConVar.Server.modifierTickRate))
		{
			if (owner != null && !owner.IsDead())
			{
				TickModifiers(ownerEntity, timeSinceLastTick);
			}
			timeSinceLastTick = 0.0;
		}
	}

	protected virtual void TickModifiers(BaseCombatEntity ownerEntity, double delta)
	{
		for (int num = All.Count - 1; num >= 0; num--)
		{
			Modifier modifier = All[num];
			float num2 = (IsModifierCompatibleWithDigestionBoost(modifier.Type) ? GetValue(Modifier.ModifierType.DigestionBoostTimeMod, 1f) : 1f);
			modifier.Tick(ownerEntity, delta * (double)num2);
			if (modifier.Expired)
			{
				Remove(modifier);
			}
		}
	}

	protected bool IsModifierCompatibleWithDigestionBoost(Modifier.ModifierType modifierType)
	{
		if ((uint)modifierType <= 1u || modifierType == Modifier.ModifierType.Scrap_Yield || modifierType == Modifier.ModifierType.Harvesting)
		{
			return true;
		}
		return false;
	}
}
