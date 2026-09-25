using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class BaseRagdoll : BaseMountable
{
	[Header("Ragdolling")]
	[SerializeField]
	private Ragdoll Ragdoll;

	[SerializeField]
	private PlayerBonePosData BonePosData;

	[SerializeField]
	private List<DamageTypeEntry> impactDamage;

	[SerializeField]
	private List<Rigidbody> flailBodies;

	private EntityRef<BasePlayer> parentPlayer;

	private BaseEntity initiator;

	private bool dieOnImpact;

	private float lastMovingTime;

	private float largestNegYVelocityOnCollision;

	private bool inTheAir;

	private bool flailInAir;

	private float spinDampening;

	private Vector3 ragdollSpinDirection;

	private bool matchPlayerGravity;

	private int clippedFrameCount;

	private Vector3 lastTransformPos;

	private Vector3 lastEyePos;

	private Vector3 lastPelvisPoint;

	private List<(Vector3, Quaternion)> lastRagdollRbPosRot;

	public GameObjectRef fleshImpact;

	[ClientVar(Help = "(Generated) When enabled, draws debug visualisations for ragdoll physics state including bone positions and velocities")]
	public static bool debug_vis;

	protected override bool BypassClothingMountBlocks => true;

	public override bool DirectlyMountable()
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		if (parentPlayer.IsValid(base.isServer))
		{
			info.msg.temporaryRagdoll.parentID = parentPlayer.uid;
			info.msg.temporaryRagdoll.mountPose = (int)mountPose;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.temporaryRagdoll != null)
		{
			Load(info.msg.temporaryRagdoll);
		}
	}

	private void Load(TemporaryRagdoll tempRagdoll)
	{
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		player.SetPlayerFlag(BasePlayer.PlayerFlags.Ragdolling, b: false);
		player.eyes.rotation = Quaternion.Euler(player.eyes.rotation.eulerAngles.WithX(0f));
		if (dieOnImpact)
		{
			KillPlayerImpact(player, doRadiusDamage: true);
		}
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Rigidbody rigidbody = GetComponent<Rigidbody>();
		if (rigidbody == null)
		{
			rigidbody = base.gameObject.AddComponent<Rigidbody>();
			rigidbody.mass = 10f;
			rigidbody.linearDamping = 0f;
			rigidbody.angularDamping = 0f;
		}
		rigidbody.useGravity = true;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		rigidbody.sleepThreshold = Mathf.Max(0.05f, Physics.sleepThreshold);
		lastMovingTime = Time.time;
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		base.gameObject.SetIgnoreCollisions(GetMounted().gameObject, ignore: true);
		Invoke(StopRagdolling, 10f);
	}

	public override void VehicleFixedUpdate()
	{
		using (TimeWarning.New("BaseRagdoll.VehicleFixedUpdate"))
		{
			base.VehicleFixedUpdate();
			BasePlayer mounted = GetMounted();
			if (mounted == null)
			{
				Kill();
				return;
			}
			AdjustForClipping();
			if (rigidBody.linearVelocity.magnitude > 2f || rigidBody.angularVelocity.magnitude > 2f)
			{
				lastMovingTime = Time.time;
			}
			if (matchPlayerGravity)
			{
				Vector3 force = 2.5f * Physics.gravity - Physics.gravity;
				foreach (Rigidbody rigidbody in Ragdoll.rigidbodies)
				{
					rigidbody.AddForce(force, ForceMode.Acceleration);
				}
			}
			if (inTheAir && flailInAir)
			{
				foreach (Rigidbody flailBody in flailBodies)
				{
					Vector3 vector = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)) * (UnityEngine.Random.Range(5f, 10f) * spinDampening);
					flailBody.AddForce(vector * 15f, ForceMode.Acceleration);
				}
				rigidBody.AddTorque(ragdollSpinDirection * spinDampening, ForceMode.Acceleration);
				spinDampening *= 0.98f;
			}
			if (largestNegYVelocityOnCollision < 0f)
			{
				if ((bool)mounted)
				{
					mounted.ApplyFallDamageFromVelocity(largestNegYVelocityOnCollision);
				}
				largestNegYVelocityOnCollision = 0f;
			}
			if (Time.time > lastMovingTime + 1.25f)
			{
				CancelInvoke(StopRagdolling);
				StopRagdolling();
			}
		}
	}

	private void AdjustForClipping()
	{
		using (TimeWarning.New("AdjustForClipping"))
		{
			Vector3 position = lastTransformPos;
			lastTransformPos = base.transform.position;
			Vector3 start = lastEyePos;
			Vector3 end = (lastEyePos = GetMounted().eyes.position);
			Vector3 start2 = lastPelvisPoint;
			Vector3 end2 = (lastPelvisPoint = Ragdoll.primaryBody.position);
			BasePlayer basePlayer = parentPlayer.Get(serverside: true);
			bool flag = rigidBody.linearVelocity.sqrMagnitude > 3f;
			bool flag2 = false;
			bool flag3 = false;
			List<RaycastHit> hits = Pool.Get<List<RaycastHit>>();
			bool flag4 = flag && ClippedOnPath(start, end, 0.3f, in hits, basePlayer);
			bool flag5 = ClippedOnPath(start, end, 0f, in hits, basePlayer);
			flag2 = flag2 || flag4 || flag5;
			flag3 = flag3 || flag5;
			bool flag6 = flag && ClippedOnPath(start2, end2, 0.3f, in hits, basePlayer);
			bool flag7 = ClippedOnPath(start2, end2, 0f, in hits, basePlayer);
			flag2 = flag2 || flag6 || flag7;
			flag3 = flag3 || flag7;
			Pool.FreeUnmanaged(ref hits);
			if (!flag2)
			{
				for (int i = 0; i < Ragdoll.rigidbodies.Count; i++)
				{
					Rigidbody rigidbody = Ragdoll.rigidbodies[i];
					lastRagdollRbPosRot[i] = (rigidbody.position, rigidbody.rotation);
				}
				return;
			}
			if (flag3 && ++clippedFrameCount >= 3)
			{
				basePlayer.Hurt(new HitInfo(initiator, basePlayer, DamageType.Blunt, 1000f));
				StopRagdolling();
				return;
			}
			for (int j = 0; j < Ragdoll.rigidbodies.Count; j++)
			{
				Rigidbody rigidbody2 = Ragdoll.rigidbodies[j];
				if (!(rigidbody2 == null))
				{
					(Vector3, Quaternion) tuple = lastRagdollRbPosRot[j];
					Vector3 item = tuple.Item1;
					Quaternion item2 = tuple.Item2;
					rigidbody2.position = item;
					rigidbody2.rotation = item2;
					rigidbody2.linearVelocity = Vector3.zero;
					rigidbody2.angularVelocity = Vector3.zero;
				}
			}
			base.transform.position = position;
			lastTransformPos = position;
			lastEyePos = start;
			lastPelvisPoint = start2;
		}
	}

	private bool ClippedOnPath(Vector3 start, Vector3 end, float radius, in List<RaycastHit> hits, BasePlayer ignorePlayer)
	{
		bool result = false;
		Vector3 direction = end - start;
		float magnitude = direction.magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return false;
		}
		direction /= magnitude;
		Ray ray = new Ray(start, direction);
		hits.Clear();
		GamePhysics.TraceAllUnordered(ray, radius, hits, magnitude, -910884607, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit hit in hits)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (!GamePhysics.CompareEntity(entity, this) && !GamePhysics.CompareEntity(entity, ignorePlayer))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void InitFromPlayer(BasePlayer bp, Vector3 velocityOverride = default(Vector3), bool matchPlayerGravity = true, bool flailInAir = false, bool dieOnImpact = false, BaseEntity initiator = null)
	{
		parentPlayer.Set(bp);
		lastEyePos = bp.eyes.position;
		if (bp.isMounted)
		{
			mountPose = bp.GetMounted().mountPose;
		}
		PlayerBonePosData.BonePosData bonePositionData = BonePosData.GetBonePositionData(bp.playerFlags, bp.modelState);
		if (bonePositionData != null)
		{
			model.skeleton.CopyFrom(bonePositionData.bonePositions, bonePositionData.boneRotations, localSpace: true);
			model.skeleton.Bones[0].transform.localEulerAngles += bonePositionData.rootRotationOffset;
		}
		Quaternion rotation = Quaternion.Euler(bp.transform.eulerAngles.x, bp.eyes.bodyRotation.eulerAngles.y, bp.transform.eulerAngles.z);
		base.transform.SetPositionAndRotation(bp.transform.position, rotation);
		lastTransformPos = base.transform.position;
		Ragdoll.ServerInit();
		rigidBody.linearDamping = 0f;
		rigidBody.angularDamping = 0f;
		inTheAir = true;
		Vector3 vector = ((velocityOverride != Vector3.zero) ? velocityOverride : (bp.isMounted ? bp.GetMountVelocity() : bp.estimatedVelocity));
		rigidBody.AddForce(vector, ForceMode.Impulse);
		lastRagdollRbPosRot = new List<(Vector3, Quaternion)>(Ragdoll.rigidbodies.Count);
		foreach (Rigidbody rigidbody in Ragdoll.rigidbodies)
		{
			rigidbody.linearDamping = 0f;
			rigidbody.angularDamping = 0f;
			rigidbody.AddForceAtPosition(vector * rigidbody.mass, rigidbody.transform.position, ForceMode.Impulse);
			rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			lastRagdollRbPosRot.Add((rigidbody.position, rigidbody.rotation));
		}
		lastPelvisPoint = Ragdoll.primaryBody.position;
		this.flailInAir = flailInAir;
		if (flailInAir)
		{
			spinDampening = 1f;
			Vector3 zero = Vector3.zero;
			zero[UnityEngine.Random.Range(0, 3)] = 1f;
			ragdollSpinDirection = zero * 0.8f;
		}
		if ((bool)initiator)
		{
			base.gameObject.SetIgnoreCollisions(initiator.gameObject, ignore: true);
		}
		this.matchPlayerGravity = matchPlayerGravity;
		this.initiator = initiator;
		this.dieOnImpact = dieOnImpact;
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res, bool silent = false)
	{
		List<Collider> obj = Pool.Get<List<Collider>>();
		GetComponentsInChildren(obj);
		foreach (Collider item in obj)
		{
			item.enabled = false;
		}
		bool dismountPosition = base.GetDismountPosition(player, out res, silent);
		foreach (Collider item2 in obj)
		{
			item2.enabled = true;
		}
		Pool.FreeUnmanaged(ref obj);
		return dismountPosition;
	}

	private void StopRagdolling()
	{
		BasePlayer mounted = GetMounted();
		if (mounted != null)
		{
			mounted.SetPlayerFlag(BasePlayer.PlayerFlags.Ragdolling, b: false);
		}
		DismountAllPlayers();
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override bool AllowPlayerInstigatedDismount(BasePlayer player)
	{
		object obj = Interface.CallHook("CanRagdollDismount", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}

	protected void ProcessCollision(Collision collision, BaseEntity hitEntity, Rigidbody ourRigidbody)
	{
		if (base.isClient || collision == null || collision.gameObject == null || collision.gameObject == null)
		{
			return;
		}
		BasePlayer mounted = GetMounted();
		if (mounted == null)
		{
			return;
		}
		if (dieOnImpact)
		{
			KillPlayerImpact(mounted, doRadiusDamage: true);
		}
		else
		{
			largestNegYVelocityOnCollision = Mathf.Min(largestNegYVelocityOnCollision, 0f - collision.relativeVelocity.y);
		}
		if (!inTheAir)
		{
			return;
		}
		inTheAir = false;
		if (!flailInAir)
		{
			return;
		}
		rigidBody.linearDamping = 1f;
		rigidBody.angularDamping = 1f;
		foreach (Rigidbody rigidbody in Ragdoll.rigidbodies)
		{
			rigidbody.linearDamping = 1f;
			rigidbody.angularDamping = 1f;
		}
	}

	private void KillPlayerImpact(BasePlayer mounted, bool doRadiusDamage)
	{
		Effect.server.Run(mounted.fallDamageEffect.resourcePath, base.transform.position, Vector3.zero);
		Effect.server.Run(fleshImpact.resourcePath, base.transform.position, Vector3.zero);
		if (doRadiusDamage)
		{
			DamageUtil.RadiusDamage(mounted, initiator, mounted.transform.position, 1f, 3.5f, impactDamage, 133120, useLineOfSight: true, ignoreAI: false, ignoreAttackingPlayer: false, extendedLineOfSight: false, null, removeWallpaper: false, includeBoatBuildingPieces: true, mounted);
		}
		Invoke(delegate
		{
			StopRagdolling();
			mounted.Hurt(new HitInfo(initiator, mounted, DamageType.Blunt, 1000f));
		}, 1f);
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (base.isServer)
		{
			ProcessCollision(collision, hitEntity, rigidBody);
		}
	}
}
