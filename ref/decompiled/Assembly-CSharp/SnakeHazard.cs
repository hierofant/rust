using System.Collections.Generic;
using Network;
using UnityEngine;

public class SnakeHazard : WildlifeHazard
{
	public static Translate.Phrase SnakeHazardFailedTipPhrase = new Translate.Phrase("toast.snake_hazard_failed", "Jump immediately when a Snake hisses to avoid its attack.");

	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 5f;

	public List<ModifierDefintion> FailModifierEffects;

	private BasePlayer playerToAttack;

	private float slitherRate = 0.05f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SnakeHazard.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected override void OnHazardFailed(BasePlayer player)
	{
		base.OnHazardFailed(player);
		if (!(player == null))
		{
			ClientRPC(RpcTarget.Player("CL_SnakeHazardFailed", player));
			if (GamePhysics.LineOfSight(base.transform.position + Vector3.up * 0.25f, player.transform.position + Vector3.up * 0.25f, 1075904769))
			{
				playerToAttack = player;
				Invoke(ApplyAttackToPlayer, 0.3f);
				ClientRPC(RpcTarget.NetworkGroup("CL_Attack"));
			}
		}
	}

	private void ApplyAttackToPlayer()
	{
		if (playerToAttack == null)
		{
			return;
		}
		if (!playerToAttack.OnAttacked(Damage, DamageType, this, ignoreShield: false))
		{
			playerToAttack = null;
			return;
		}
		if (FailModifierEffects != null && playerToAttack.modifiers != null)
		{
			playerToAttack.modifiers.Add(FailModifierEffects);
		}
		playerToAttack = null;
	}

	public override void StartReposition()
	{
		base.StartReposition();
		if (!base.IsCorpse)
		{
			if (base.isServer)
			{
				ClientRPC(RpcTarget.NetworkGroup("CL_RepositionDisappear"), repositionTo);
			}
			InvokeRepeating(SlitherTick, 0.2f, slitherRate);
			Invoke(StartDelayedTeleport, SlitherDuration + 0.2f);
		}
	}

	private void SlitherTick()
	{
		Vector3 vector = Vector3.MoveTowards(base.transform.position, repositionTo, SlitherSpeed * slitherRate);
		if (Physics.Raycast(vector + Vector3.up * 1f, Vector3.down, out var hitInfo, 5f, 8388608))
		{
			vector = hitInfo.point;
		}
		base.transform.position = vector;
		try
		{
			syncPosition = true;
			NetworkPositionTick();
		}
		finally
		{
			syncPosition = false;
		}
	}

	private void StartDelayedTeleport()
	{
		if (!base.IsCorpse)
		{
			CancelInvoke(SlitherTick);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Disabled, b: true);
			}
			Invoke(EndDelayedTeleport, 2f);
		}
	}

	private void EndDelayedTeleport()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		ServerPosition = repositionTo;
		SendNetworkUpdate_Position();
		if (base.isServer)
		{
			if (PrefabRepositionEffect != null && PrefabRepositionEffect.isValid)
			{
				Effect.server.Run(PrefabReappearEffect.resourcePath, ServerPosition, Vector3.up);
			}
			ClientRPC(RpcTarget.NetworkGroup("CL_RepositionReappear"), repositionLookAtPos);
		}
	}

	protected override bool ShouldStartHazard(BasePlayer player)
	{
		if (!base.ShouldStartHazard(player))
		{
			return false;
		}
		if (IsInvoking(SlitherTick))
		{
			return false;
		}
		if (IsInvoking(StartDelayedTeleport))
		{
			return false;
		}
		if (IsInvoking(EndDelayedTeleport))
		{
			return false;
		}
		return true;
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		CancelSnakeInvokes();
	}

	public override void OnKilled()
	{
		base.OnKilled();
		CancelSnakeInvokes();
	}

	private void CancelSnakeInvokes()
	{
		CancelInvoke(SlitherTick);
		CancelInvoke(StartDelayedTeleport);
		CancelInvoke(EndDelayedTeleport);
		CancelInvoke(ApplyAttackToPlayer);
	}
}
