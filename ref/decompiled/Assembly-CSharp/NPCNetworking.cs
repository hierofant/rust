using Facepunch;
using ProtoBuf;
using Rust.Ai.Gen2;
using UnityEngine;

public class NPCNetworking : EntityComponent<BaseEntity>
{
	public const BaseEntity.Flags FLAG_IS_SWIMMING = BaseEntity.Flags.Reserved1;

	public const BaseEntity.Flags FLAG_IS_JUMPING = BaseEntity.Flags.Reserved2;

	public const BaseEntity.Flags FLAG_IS_AIMING = BaseEntity.Flags.Reserved3;

	public const BaseEntity.Flags FLAG_IS_RELAXED = BaseEntity.Flags.Reserved4;

	public const BaseEntity.Flags FLAG_IS_CROUCHING = BaseEntity.Flags.Reserved5;

	public const BaseEntity.Flags FLAG_IS_ALERT = BaseEntity.Flags.Reserved6;

	private SenseComponent _senses;

	private RustNavMeshAgent _agent;

	public Vector3 LookDirection { get; private set; }

	public float DesiredSwimDepth { get; private set; }

	private SenseComponent Senses => _senses ?? (_senses = base.baseEntity.GetComponent<SenseComponent>());

	private RustNavMeshAgent Agent => _agent ?? (_agent = base.baseEntity.GetComponent<RustNavMeshAgent>());

	public override void InitShared()
	{
		if (base.baseEntity.isServer)
		{
			LookDirection = base.transform.forward;
		}
	}

	public void Tick()
	{
		Vector3 lookDirection = LookDirection;
		float desiredSwimDepth = DesiredSwimDepth;
		bool flag = base.baseEntity.HasFlag(BaseEntity.Flags.Reserved6);
		LookDirection = Senses.GetEyeTransform().rotation * Vector3.forward;
		DesiredSwimDepth = Agent.desiredSwimDepth.Value;
		BaseEntity target;
		bool flag2 = Senses.FindTarget(out target);
		if (base.baseEntity.net != null && base.baseEntity.net.group != null && base.baseEntity.net.group.subscribers != null && base.baseEntity.net.group.subscribers.Count > 0 && (lookDirection != LookDirection || desiredSwimDepth != DesiredSwimDepth || flag != flag2))
		{
			base.baseEntity.SetFlagLocal(BaseEntity.Flags.Reserved6, flag2);
			base.baseEntity.SendNetworkUpdate();
		}
	}

	public override void SaveComponent(BaseNetworkable.SaveInfo info)
	{
		base.SaveComponent(info);
		if (base.baseEntity.isServer && !info.forDisk)
		{
			info.msg.npcTargetState = Pool.Get<NPCTargetState>();
			info.msg.npcTargetState.lookDirection = LookDirection;
			info.msg.npcTargetState.desiredSwimDepth = DesiredSwimDepth;
		}
	}
}
