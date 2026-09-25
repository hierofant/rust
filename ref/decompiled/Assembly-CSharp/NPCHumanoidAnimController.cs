using Network;
using UnityEngine;

public class NPCHumanoidAnimController : EntityComponent<BaseEntity>
{
	public Animator animator;

	public bool IsAiming
	{
		get
		{
			return (base.baseEntity.flags & BaseEntity.Flags.Reserved3) == BaseEntity.Flags.Reserved3;
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved3, value);
		}
	}

	public bool IsRelaxed
	{
		get
		{
			return (base.baseEntity.flags & BaseEntity.Flags.Reserved4) == BaseEntity.Flags.Reserved4;
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved4, value);
		}
	}

	public bool IsCrouching
	{
		get
		{
			return (base.baseEntity.flags & BaseEntity.Flags.Reserved5) == BaseEntity.Flags.Reserved5;
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved5, value);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCHumanoidAnimController.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
