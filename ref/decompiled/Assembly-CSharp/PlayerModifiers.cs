using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class PlayerModifiers : BaseModifiers<BasePlayer>
{
	public List<ModifierLimits> Limits;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerModifiers.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static void AddToPlayer(BasePlayer player, List<ModifierDefintion> modifiers, float effectScale = 1f, float durationScale = 1f)
	{
		if (!(player == null) && !(player.modifiers == null) && modifiers != null)
		{
			player.modifiers.Add(modifiers, effectScale, durationScale);
		}
	}

	protected override bool IsCompatible(Modifier.ModifierType modType)
	{
		if ((uint)(modType - 20) <= 1u)
		{
			return false;
		}
		return true;
	}

	public override void ServerUpdate(BaseCombatEntity ownerEntity)
	{
		base.ServerUpdate(ownerEntity);
		SendChangesToClient();
	}

	public ProtoBuf.PlayerModifiers Save(bool forDisk)
	{
		ProtoBuf.PlayerModifiers playerModifiers = Pool.Get<ProtoBuf.PlayerModifiers>();
		playerModifiers.modifiers = Pool.Get<List<ProtoBuf.Modifier>>();
		float value = GetValue(Modifier.ModifierType.DigestionBoost, 1f);
		foreach (Modifier item in All)
		{
			if (item != null && (!forDisk || item.Source != Modifier.ModifierSource.Interaction))
			{
				ProtoBuf.Modifier modifier = item.Save();
				if (!forDisk && value > 1f && IsModifierCompatibleWithDigestionBoost(item.Type))
				{
					modifier.duration *= value - 1f;
					modifier.timeRemaining *= value - 1f;
					modifier.value *= value;
				}
				playerModifiers.modifiers.Add(modifier);
			}
		}
		return playerModifiers;
	}

	public void Load(ProtoBuf.PlayerModifiers m, bool fromDisk)
	{
		RemoveAll();
		if (m == null || m.modifiers == null)
		{
			return;
		}
		foreach (ProtoBuf.Modifier modifier2 in m.modifiers)
		{
			if (modifier2 != null && (!fromDisk || modifier2.source != 2))
			{
				Modifier modifier = new Modifier();
				modifier.Init((Modifier.ModifierType)modifier2.type, (Modifier.ModifierSource)modifier2.source, modifier2.value, modifier2.duration, modifier2.timeRemaining);
				Add(modifier);
			}
		}
	}

	protected override int GetMaxModifierCount(Modifier modifier)
	{
		return GetModifierLimitForSourceAndType(modifier)?.MaxApplications ?? base.GetMaxModifierCount(modifier);
	}

	protected override float GetClampedValue(Modifier modifier, float value)
	{
		ModifierLimits modifierLimitsForType = GetModifierLimitsForType(modifier);
		if (modifierLimitsForType != null)
		{
			return Mathf.Clamp(value, modifierLimitsForType.minValue, modifierLimitsForType.maxValue);
		}
		return value;
	}

	protected ModifierLimit GetModifierLimitForSourceAndType(Modifier modifier)
	{
		if (Limits != null)
		{
			foreach (ModifierLimits limit in Limits)
			{
				if (limit == null || limit.type != modifier.Type)
				{
					continue;
				}
				foreach (ModifierLimit limit2 in limit.limits)
				{
					if (limit2 != null && limit2.source == modifier.Source)
					{
						return limit2;
					}
				}
			}
		}
		return null;
	}

	protected ModifierLimits GetModifierLimitsForType(Modifier modifier)
	{
		if (Limits != null)
		{
			foreach (ModifierLimits limit in Limits)
			{
				if (limit != null && limit.type == modifier.Type)
				{
					return limit;
				}
			}
		}
		return null;
	}

	public void SendChangesToClient()
	{
		if (!dirty)
		{
			return;
		}
		SetDirty(flag: false);
		using ProtoBuf.PlayerModifiers arg = Save(forDisk: false);
		base.baseEntity.ClientRPC(RpcTarget.Player("UpdateModifiers", base.baseEntity), arg);
	}
}
