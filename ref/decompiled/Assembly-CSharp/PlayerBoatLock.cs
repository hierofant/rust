using Rust;
using UnityEngine;

public class PlayerBoatLock
{
	private readonly bool isServer;

	private readonly SteeringWheel owningWheel;

	public const BaseEntity.Flags FLAG_CODE_ENTRY_BLOCKED = BaseEntity.Flags.Reserved10;

	private int wrongCodes;

	private float lastWrongTime = float.NegativeInfinity;

	public bool HasALock
	{
		get
		{
			if (isServer)
			{
				return !string.IsNullOrEmpty(Code);
			}
			return false;
		}
	}

	public string Code { get; set; } = "";


	public PlayerBoatLock(SteeringWheel owner, bool isServer)
	{
		owningWheel = owner;
		this.isServer = isServer;
	}

	public bool CodeEntryBlocked(BasePlayer player)
	{
		if (!HasALock)
		{
			return true;
		}
		if (HasLockPermission(player))
		{
			return false;
		}
		if (owningWheel != null)
		{
			return owningWheel.HasFlag(BaseEntity.Flags.Reserved10);
		}
		return false;
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		if (info.msg.steeringWheel != null)
		{
			Code = info.msg.steeringWheel.lockCode;
		}
		if (Code == null)
		{
			Code = "";
		}
	}

	public bool HasLockPermission(BasePlayer player)
	{
		if (!player.IsValid() || player.IsDead())
		{
			return false;
		}
		if (!HasALock)
		{
			return true;
		}
		return owningWheel.Privilege.IsAuthed(player);
	}

	public void PostServerLoad()
	{
		using BaseEntity.FlagsUpdateScope flagsUpdateScope = owningWheel.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(BaseEntity.Flags.Reserved10, b: false);
	}

	public bool TryAddALock(string code, BasePlayer player)
	{
		if (!isServer)
		{
			return false;
		}
		if (owningWheel.IsDead())
		{
			return false;
		}
		if (TrySetNewCode(code, player))
		{
			DoEffect((owningWheel != null) ? owningWheel.effectLocked.resourcePath : null);
		}
		return HasALock;
	}

	public bool IsValidLockCode(string code)
	{
		if (code != null && code.Length == 4)
		{
			return code.IsNumeric();
		}
		return false;
	}

	public bool TrySetNewCode(string newCode, BasePlayer player)
	{
		if (!IsValidLockCode(newCode))
		{
			return false;
		}
		Code = newCode;
		owningWheel.Privilege.AddPlayer(player);
		owningWheel.SendNetworkUpdate();
		return true;
	}

	public void RemoveLock()
	{
		if (isServer && HasALock)
		{
			Code = "";
			owningWheel.SendNetworkUpdate();
		}
	}

	public bool TryOpenWithCode(BasePlayer player, string codeEntered)
	{
		if (CodeEntryBlocked(player))
		{
			return false;
		}
		if (!(codeEntered == Code))
		{
			if (Time.realtimeSinceStartup > lastWrongTime + 60f)
			{
				wrongCodes = 0;
			}
			DoEffect((owningWheel != null) ? owningWheel.effectDenied.resourcePath : null);
			DoEffect((owningWheel != null) ? owningWheel.effectShock.resourcePath : null);
			player.Hurt((float)(wrongCodes + 1) * 5f, DamageType.ElectricShock, owningWheel, useProtection: false);
			wrongCodes++;
			if (wrongCodes > 5)
			{
				player.ShowToast(GameTip.Styles.Red_Normal, CodeLock.blockwarning, false);
			}
			if ((float)wrongCodes >= CodeLock.maxFailedAttempts)
			{
				using (BaseEntity.FlagsUpdateScope flagsUpdateScope = owningWheel.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(BaseEntity.Flags.Reserved10, b: true);
				}
				owningWheel.Invoke(ClearCodeEntryBlocked, CodeLock.lockoutCooldown);
			}
			lastWrongTime = Time.realtimeSinceStartup;
			return false;
		}
		DoEffect((owningWheel != null) ? owningWheel.effectUnlocked.resourcePath : null);
		wrongCodes = 0;
		owningWheel.SendNetworkUpdate();
		return true;
	}

	private void ClearCodeEntryBlocked()
	{
		using (BaseEntity.FlagsUpdateScope flagsUpdateScope = owningWheel.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved10, b: false);
		}
		wrongCodes = 0;
	}

	public void Save(BaseNetworkable.SaveInfo info)
	{
		info.msg.steeringWheel.hasLock = HasALock;
		if (info.forDisk)
		{
			info.msg.steeringWheel.lockCode = Code;
		}
	}

	internal void DoEffect(string effect)
	{
		if (!(owningWheel == null) && !string.IsNullOrEmpty(effect))
		{
			Effect.server.Run(effect, owningWheel, 0u, owningWheel.EffectLocation.localPosition, Vector3.forward);
		}
	}
}
