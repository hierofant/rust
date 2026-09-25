using System;
using System.Linq;
using ConVar;
using Network;
using Rust;
using UnityEngine;

public class WildlifeHazard : BaseCombatEntity, IReceivePlayerTickListener
{
	public const Flags Flag_IsCorpse = Flags.Reserved8;

	[ServerVar(Help = "(Generated) Interval in seconds between client-side tick updates for wildlife hazard entities")]
	public static float ClientTickRate = 0.1f;

	[ServerVar(Help = "(Generated) Global multiplier applied to wildlife hazard reaction time; higher values make hazards slower to react to player presence")]
	public static float ReactionTimeMultiplier = 1f;

	[ServerVar(Help = "(Generated) Global multiplier applied to the per-tick probability that a wildlife hazard attempts to reposition")]
	public static float ChanceToRepositionMultiplier = 1f;

	[ServerVar(Help = "(Generated) Global multiplier for the radius used when choosing a new reposition destination for a wildlife hazard")]
	public static float RepositionRadiusMultiplier = 1f;

	[ServerVar(Help = "(Generated) Global multiplier applied to the cooldown timer between wildlife hazard reposition attempts")]
	public static float RepositionTimerMultiplier = 1f;

	[ServerVar(Help = "(Generated) Maximum number of position candidates sampled when a wildlife hazard searches for a valid reposition destination")]
	public static int RepositionAttempts = 5;

	[Header("Wildlife Hazard")]
	public BUTTON ReactionSaveButton;

	public float SavingReactionTime = 2f;

	public float Damage = 20f;

	public float HazardInterval = 10f;

	public DamageType DamageType = DamageType.Bite;

	public float ChanceToReposition = 0.5f;

	public float RepositionDelay = 1.25f;

	public float RepositionTimer = 2f;

	public float RepositionRadiusMin = 2f;

	public float RepositionRadiusMax = 4f;

	public Transform ClientArtRoot;

	public TriggerQTE QTETrigger;

	public TriggerBase ClientTrigger;

	public float LookSpeed = 10f;

	public float MinTurnDegrees = 45f;

	public float MinFastTurnDistance = 2f;

	public float MaxWaterDepth = 0.1f;

	public float SlitherDuration = 1f;

	public float SlitherSpeed = 2f;

	public GameObjectRef CorpsePrefab;

	public GameObjectRef BitFX;

	[Header("Wildlife Hazad Visuals")]
	public Animator Animator;

	public GameObjectRef PrefabRepositionEffect;

	public GameObjectRef PrefabReappearEffect;

	[Header("Wildlife Hazard Audio")]
	public SoundDefinition HazardTriggeredSound;

	public bool PlayAlertSounds = true;

	public SoundDefinition AlertIntervalSound;

	public SoundDefinition AttackSound;

	public float AlertSoundMinInterval = 3f;

	public float AlertSoundMaxInterval = 5f;

	public SoundDefinition RepositionDisappearSound;

	public SoundDefinition RepositionReappearSound;

	[Header("Wildlife Hazard Corpse")]
	public ResourceDispenser DeadResourceDispenser;

	public ProtectionProperties DeadProtectionProperties;

	[Tooltip("If enabled, only triggers for one player at a time")]
	public bool SingularInteraction = true;

	public float AttackRange = 1.5f;

	public float AlertToIdleCooldown = 5f;

	protected const int placementMask = 8388608;

	protected const int blockMask = 1075904769;

	protected Vector3 repositionLookAtPos;

	protected Vector3 repositionTo;

	protected int failedRepositionAttempts;

	public override bool IsNpc => true;

	public bool IsCorpse => HasFlag(Flags.Reserved8);

	public BasePlayer SingularInteractionPlayer { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WildlifeHazard.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: false);
		}
		DeadResourceDispenser.enabled = false;
		failedRepositionAttempts = 0;
	}

	public virtual void TriggeredByPlayer(BasePlayer player)
	{
		if (ShouldStartHazard(player))
		{
			StartHazard(player);
		}
	}

	protected virtual bool ShouldStartHazard(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (IsCorpse)
		{
			return false;
		}
		if (!IsAlive())
		{
			return false;
		}
		if (IsInvoking(StartReposition))
		{
			return false;
		}
		if (IsInvoking(ReAttackCheck))
		{
			return false;
		}
		if (IsInvoking(FailHazardDelayed))
		{
			return false;
		}
		if (SingularInteraction && SingularInteractionPlayer != null)
		{
			return false;
		}
		if (!CanSeeTarget(player.transform))
		{
			return false;
		}
		return true;
	}

	private void StartHazard(BasePlayer player)
	{
		OnHazardStarted(player);
	}

	protected virtual void OnHazardStarted(BasePlayer player)
	{
		player.AddReceiveTickListener(this);
		if (SingularInteraction)
		{
			SingularInteractionPlayer = player;
		}
		CancelInvoke(ReAttackCheck);
		CancelInvoke(FailHazardDelayed);
		float reactionTime = GetReactionTime(player);
		Invoke(FailHazardDelayed, reactionTime);
		ClientRPC(RpcTarget.Player("Client_StartHazard", player), reactionTime);
	}

	protected void FailHazardDelayed()
	{
		EndHazard(SingularInteractionPlayer, success: false);
	}

	protected void EndHazard(BasePlayer player, bool success)
	{
		if (success)
		{
			OnHazardCompleted(player);
		}
		else
		{
			OnHazardFailed(player);
		}
		OnHazardEnded(player);
	}

	protected virtual void OnHazardCompleted(BasePlayer player)
	{
	}

	protected virtual void OnHazardFailed(BasePlayer player)
	{
	}

	protected virtual void OnHazardEnded(BasePlayer player)
	{
		ClientRPC(RpcTarget.Player("Client_EndHazard", player));
		CancelInvoke(FailHazardDelayed);
		CancelInvoke(ReAttackCheck);
		if (player != null)
		{
			player.RemoveReceiveTickListener(this);
		}
		SingularInteractionPlayer = null;
		if (ShouldReposition())
		{
			if (FindSuitableReposition(out var pos))
			{
				failedRepositionAttempts = 0;
				repositionTo = pos;
				repositionLookAtPos = ((player != null) ? player.transform.position : (base.transform.position + Vector3.forward));
				Invoke(StartReposition, RepositionDelay);
			}
			else
			{
				failedRepositionAttempts++;
				if (failedRepositionAttempts <= 3)
				{
					Invoke(ReAttackCheck, HazardInterval);
				}
				else
				{
					Kill();
				}
			}
		}
		else
		{
			Invoke(ReAttackCheck, HazardInterval);
		}
	}

	private bool ShouldReposition()
	{
		if (IsCorpse)
		{
			return false;
		}
		float num = ChanceToReposition * ChanceToRepositionMultiplier;
		if (num <= 0f)
		{
			return false;
		}
		if (UnityEngine.Random.Range(0f, 1f) > num)
		{
			return false;
		}
		return true;
	}

	public virtual void StartReposition()
	{
	}

	private bool FindSuitableReposition(out Vector3 pos)
	{
		bool flag = true;
		int num = 0;
		while (flag)
		{
			float num2 = UnityEngine.Random.Range(RepositionRadiusMin, RepositionRadiusMax) * RepositionRadiusMultiplier;
			float f = UnityEngine.Random.value * (MathF.PI * 2f);
			Vector3 vector = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
			pos = base.transform.position + vector * num2;
			bool flag2 = ValidatePosition(ref pos);
			if (flag2)
			{
				return true;
			}
			flag = !flag2 && ++num < RepositionAttempts;
		}
		pos = base.transform.position;
		return false;
	}

	private bool ValidatePosition(ref Vector3 pos)
	{
		if (UnityEngine.Physics.Raycast(pos + Vector3.up * 3f, Vector3.down, out var hitInfo, 6f, 8388608))
		{
			if (WaterLevel.GetOverallWaterDepth(hitInfo.point, waves: true, volumes: false) > MaxWaterDepth)
			{
				return false;
			}
			if (!GamePhysics.LineOfSight(hitInfo.point, hitInfo.point + Vector3.up * 4f, 1075904769))
			{
				return false;
			}
			if (!GamePhysics.LineOfSight(base.transform.position + Vector3.up * 0.25f, hitInfo.point + Vector3.up * 0.25f, 1075904769))
			{
				return false;
			}
			pos = hitInfo.point;
			return true;
		}
		return false;
	}

	private void ReAttackCheck()
	{
		if (IsCorpse || QTETrigger.contents == null || QTETrigger.contents.Count == 0)
		{
			CancelInvoke(ReAttackCheck);
			return;
		}
		GameObject gameObject = QTETrigger.contents.Single();
		if (!(gameObject == null))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(gameObject);
			if (!(baseEntity == null))
			{
				TriggeredByPlayer(baseEntity as BasePlayer);
			}
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer)
		{
			if (IsCorpse)
			{
				OnCorpseAttacked(info);
			}
			else
			{
				OnAliveAttacked(info);
			}
		}
	}

	private void OnCorpseAttacked(HitInfo info)
	{
		ResetCorpseRemovalTime();
		if (!(info.Weapon is BaseMelee baseMelee) || baseMelee.GetGatherInfoFromIndex(ResourceDispenser.GatherType.Flesh).gatherDamage != 0f)
		{
			DeadResourceDispenser.DoGather(info);
			if (!info.DidGather)
			{
				base.OnAttacked(info);
			}
		}
	}

	private void OnAliveAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (base.isServer && (bool)info.InitiatorPlayer && !info.damageTypes.IsMeleeType())
		{
			info.InitiatorPlayer.LifeStoryShotHit(info.Weapon);
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();
		if (SingularInteractionPlayer != null)
		{
			SingularInteractionPlayer.RemoveReceiveTickListener(this);
		}
		CancelHazardInvokes();
	}

	public override void OnDied(HitInfo info)
	{
		if (!base.isServer)
		{
			return;
		}
		ClientRPC(RpcTarget.NetworkGroup("CL_Died"));
		CancelInvoke(ReAttackCheck);
		CancelInvoke(FailHazardDelayed);
		if (SingularInteractionPlayer != null)
		{
			SingularInteractionPlayer.RemoveReceiveTickListener(this);
		}
		if (IsCorpse)
		{
			Kill();
			return;
		}
		if (info != null)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (initiatorPlayer != null)
			{
				initiatorPlayer.LifeStoryKill(this);
			}
		}
		TurnIntoCorpse();
	}

	public void TurnIntoCorpse()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
		SetHealth(MaxHealth());
		lifestate = LifeState.Alive;
		DeadResourceDispenser.enabled = true;
		baseProtection = DeadProtectionProperties;
		sendsHitNotification = false;
		ResetCorpseRemovalTime();
	}

	private void CancelHazardInvokes()
	{
		CancelInvoke(FailHazardDelayed);
		CancelInvoke(StartReposition);
		CancelInvoke(ReAttackCheck);
	}

	public void ResetCorpseRemovalTime()
	{
		ResetCorpseRemovalTime(ConVar.Server.corpsedespawn);
	}

	public void ResetCorpseRemovalTime(float dur)
	{
		using (TimeWarning.New("ResetRemovalTime"))
		{
			if (IsInvoking(RemoveCorpse))
			{
				CancelInvoke(RemoveCorpse);
			}
			Invoke(RemoveCorpse, dur);
		}
	}

	public void RemoveCorpse()
	{
		Kill();
	}

	bool IReceivePlayerTickListener.ShouldRemoveOnPlayerDeath()
	{
		return true;
	}

	void IReceivePlayerTickListener.OnReceivePlayerTick(BasePlayer player, PlayerTick msg)
	{
		if (!(player == null) && !(player != SingularInteractionPlayer) && player.serverInput.WasJustPressed(ReactionSaveButton))
		{
			EndHazard(player, success: true);
		}
	}

	public virtual float GetReactionTime(BasePlayer player)
	{
		float num = ((player == null || player.net == null || player.net.connection == null) ? 0f : ((float)Network.Net.sv.GetAveragePing(player.net.connection) / 1000f));
		return SavingReactionTime * ReactionTimeMultiplier + num;
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		if (base.isServer)
		{
			ClientRPC(RpcTarget.NetworkGroup("CL_Hurt"));
		}
	}

	protected bool CanSeeTarget(Transform targetTransform)
	{
		if (targetTransform == null)
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(base.transform.position + Vector3.up * 0.25f, targetTransform.position + Vector3.up * 0.25f, 1075904769))
		{
			return false;
		}
		return true;
	}
}
