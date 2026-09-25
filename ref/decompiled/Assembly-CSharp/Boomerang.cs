using Network;
using UnityEngine;

public class Boomerang : BaseMelee
{
	public static readonly int CatchHash = Animator.StringToHash("catch");

	private static readonly int CaughtHash = Animator.StringToHash("caught");

	private bool hasThrown;

	public bool HasThrown => hasThrown;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Boomerang.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void SetHeld(bool bHeld)
	{
		base.SetHeld(bHeld);
		if (!hasThrown)
		{
			return;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			ownerItem.UseItem();
			if (ownerItem.amount == 0)
			{
				ownerItem.SetParent(null);
			}
		}
	}

	public void SetHasThrown(bool thrown)
	{
		hasThrown = thrown;
		ClientRPC(RpcTarget.NetworkGroup("RPC_Caught"), thrown);
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		if (!hasThrown)
		{
			base.ServerUse(parameters);
		}
	}

	protected override bool VerifyClientAttack(BasePlayer player)
	{
		if (hasThrown)
		{
			return false;
		}
		return base.VerifyClientAttack(player);
	}

	protected override void OnEntityThrow(BaseEntity ent)
	{
		base.OnEntityThrow(ent);
		SetHasThrown(thrown: true);
		ThrownBoomerang thrownBoomerang = ent as ThrownBoomerang;
		if (thrownBoomerang != null)
		{
			Item ownerItem = GetOwnerItem();
			if (ownerItem != null)
			{
				thrownBoomerang.Condition = ownerItem.condition;
			}
		}
	}
}
