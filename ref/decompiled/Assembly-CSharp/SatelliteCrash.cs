using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Serialization;

public class SatelliteCrash : BaseCombatEntity
{
	private Vector3 descentStartPos;

	private float descentSecondsToTake;

	private float descentSecondsTaken;

	private const float phase1NetInterval = 0.5f;

	private float phase1NetTimer;

	private const float DebugDrawLife = 0.1f;

	private const float SafetyDespawnMargin = 30f;

	private const float ImpactDebugSphereSeconds = 180f;

	private const float DescentLogInterval = 5f;

	private float descentLogTimer;

	private readonly FlyoverCurve curve = new FlyoverCurve();

	private Vector3 descentVelocity;

	private const float phase2NetInterval = 0.25f;

	private float phase2NetTimer;

	private const float EntryDirCenterDeadzone = 25f;

	private static readonly Translate.Phrase CrashEventTitlePhrase = new Translate.Phrase("satellite.event.title", "SATELLITE EVENT");

	private static readonly Translate.Phrase CrashEventBodyPhrase = new Translate.Phrase("satellite.event.crashed", "A satellite has crashed at {0}!");

	private bool hasCrashed;

	private bool computerNotified;

	private Vector3 crashTarget;

	private NetworkableId controlComputerId;

	private bool hasNoOwner;

	private int fuelAtLaunch = -1;

	private int fuelAtLockIn = -1;

	private const float DebrisVelocityInheritFraction = 0.25f;

	private const float CrateScatterMinRadius = 3f;

	private const float CrateScatterMaxRadius = 10f;

	private const float CrateDropHeight = 0.5f;

	private const int GroundedScatterAttempts = 8;

	private const int ConstructionMask = 136347904;

	private const int VegetationMask = 1141374977;

	public const float CrateClearanceRadius = 0.75f;

	private const int CrateClearanceMask = 1210263809;

	private const int CrateFloorMask = 8454145;

	private const float CrateFloorRayHeight = 30f;

	private const float CrateClearanceSkin = 0.1f;

	private static readonly Collider[] crateClearanceBuffer = new Collider[16];

	private const float CrateMaxFloorSlope = 30f;

	[Header("Movement")]
	public float speed = 80f;

	[Header("Descent")]
	public float finalDescentSpeed = 106f;

	[Tooltip("Fraction of an in-game day the descent takes (1.0 = full day)")]
	public float descentDayFraction = 1f;

	[Header("Crash Effects")]
	public GameObjectRef explosionEffect;

	public GameObjectRef fireBall;

	public SoundDefinition orbitalHumLoopSound;

	public SoundDefinition reentryWhooshLoopSound;

	public SoundDefinition closeApproachSound;

	[Range(50f, 500f)]
	public float reentryWhooshStartDistance = 200f;

	[Tooltip("Crate prefabs to drop at the crash site. Each spawned crate picks one of these at random.")]
	public GameObjectRef[] cratesToDrop;

	public GameObjectRef debrisFieldMarker;

	public GameObjectRef impactSoundEffect;

	public GameObjectRef reentryTrailEffect;

	[Tooltip("Visual effect played at the crash position when the satellite hits the ground")]
	public GameObjectRef groundImpactEffect;

	[Header("Crash Config")]
	[Tooltip("Loot budget at 1.0x mass scale, in crate-equivalents. Multiplied by the mass-to-loot curve; the result spawns as crates up to Max Crates Per Crash, with any overflow going into extra items per crate.")]
	[FormerlySerializedAs("maxCratesToSpawn")]
	public int baselineCrateSpawnCount = 6;

	public int maxFireballs = 10;

	public float terrainImpactOffset = 5f;

	public float safetyDespawnTime = 120f;

	public float crateLifetimeMinutes = 30f;

	public float debrisMarkerDurationMinutes = 30f;

	public float startHeight = 200f;

	[Header("Loot Scaling")]
	[Tooltip("Maps satellite mass (kg) to the loot multiplier. The multiplier scales fireball count and the total crate loot budget (crate count up to Max Crates Per Crash, overflow into extra items per crate). Flat outside the first/last key.")]
	public AnimationCurve massToLootScale = AnimationCurve.Linear(1000f, 0.5f, 6000f, 3f);

	[Tooltip("Hard cap on crates spawned per crash, regardless of the loot multiplier. Budget beyond this goes into extra items per crate.")]
	public int maxCratesPerCrash = 5;

	[Header("Impact Entity")]
	public GameObjectRef impactEntityPrefab;

	[HideInInspector]
	public float satelliteMass = 2000f;

	[Header("Visual")]
	public Transform visualTransform;

	public GameObject dishArtwork;

	public GameObject billboardArtwork;

	public GameObject reentryArtwork;

	public GameObject crashingArtwork;

	public static float DayLengthMinutes
	{
		get
		{
			if (!(TOD_Sky.Instance != null))
			{
				return 30f;
			}
			return TOD_Sky.Instance.Components.Time.DayLengthInMinutes;
		}
	}

	private float FinalDescentSeconds => Mathf.Clamp(Satellite.final_descent_seconds, 1f, descentSecondsToTake);

	private float FinalDescentSpeed => Mathf.Max(1f, (Satellite.final_descent_speed > 0f) ? Satellite.final_descent_speed : finalDescentSpeed);

	private static float FlyoverDiveDistance => Mathf.Max(100f, Satellite.flyover_dive_distance);

	private float FuelFractionAtLockIn
	{
		get
		{
			if (fuelAtLaunch <= 0 || fuelAtLockIn < 0)
			{
				return 0.5f;
			}
			return Mathf.Clamp01((float)fuelAtLockIn / (float)fuelAtLaunch);
		}
	}

	public bool IsDescending { get; private set; }

	public override Vector3 GetLocalVelocityServer()
	{
		return descentVelocity;
	}

	public void InitOrbit(SatelliteData sat, Vector3 target, NetworkableId computerId = default(NetworkableId), int fuelRemainingAtLockIn = -1)
	{
		crashTarget = target;
		satelliteMass = sat.mass;
		controlComputerId = computerId;
		hasNoOwner = !computerId.IsValid;
		fuelAtLaunch = sat.fuel;
		fuelAtLockIn = fuelRemainingAtLockIn;
		IsDescending = true;
		descentSecondsToTake = GetScheduledDescentSeconds(descentDayFraction);
		descentSecondsTaken = 0f;
		descentStartPos = ComputeDescentStartPos(target);
		base.transform.position = Phase1StartPos();
	}

	public static float GetScheduledDescentSeconds(float dayFraction)
	{
		return Mathf.Max((Satellite.descent_seconds > 0f) ? Satellite.descent_seconds : (DayLengthMinutes * 60f * dayFraction), Mathf.Max(Satellite.final_descent_seconds, 10f));
	}

	private Vector3 Phase1StartPos()
	{
		Vector3 normalized = (crashTarget - descentStartPos).normalized;
		return descentStartPos - normalized * Satellite.phase1_extra_distance;
	}

	private Vector3 ComputeDescentStartPos(Vector3 target)
	{
		Vector3 vector = ComputeEntryDir(target);
		if (Satellite.flyover_altitude > 0f)
		{
			return ComputeFlyoverEntryPos(target, vector);
		}
		float f = Mathf.Clamp(Satellite.descent_angle, 0f, 45f) * (MathF.PI / 180f);
		Vector3 vector2 = Vector3.up * Mathf.Cos(f) + vector * Mathf.Sin(f);
		float num = FinalDescentSpeed * FinalDescentSeconds;
		return target + vector2 * num;
	}

	private Vector3 ComputeFlyoverEntryPos(Vector3 target, Vector3 entryDir)
	{
		float num = FinalDescentSpeed * FinalDescentSeconds;
		float flyover_altitude = Satellite.flyover_altitude;
		float flyoverDiveDistance = FlyoverDiveDistance;
		float num2 = Mathf.Max(num, flyoverDiveDistance * 2f);
		for (int i = 0; i < 4; i++)
		{
			Vector3 p = target + entryDir * num2 + Vector3.up * flyover_altitude;
			Vector3 p2 = target + entryDir * flyoverDiveDistance + Vector3.up * flyover_altitude;
			float b = FlyoverCurve.ApproximateLength(p, p2, target);
			num2 = Mathf.Max(flyoverDiveDistance * 2f, num2 * (num / Mathf.Max(1f, b)));
		}
		return target + entryDir * num2 + Vector3.up * flyover_altitude;
	}

	private static Vector3 ComputeEntryDir(Vector3 target)
	{
		Vector3 vector = TerrainMeta.Center - target;
		vector.y = 0f;
		if (vector.sqrMagnitude < 625f)
		{
			return RandomHorizontalDir();
		}
		return vector.normalized;
	}

	public void InitDirectDescent(Vector3 target)
	{
		crashTarget = target;
		descentSecondsToTake = Mathf.Max(1f, Satellite.final_descent_seconds);
		descentStartPos = ComputeDescentStartPos(target);
		base.transform.position = descentStartPos;
	}

	public void InitLateDescent(SatelliteData sat, Vector3 target, float secondsToImpact)
	{
		crashTarget = target;
		crashTarget.y = TerrainMeta.HeightMap.GetHeight(crashTarget);
		satelliteMass = sat.mass;
		hasNoOwner = true;
		descentSecondsToTake = GetScheduledDescentSeconds(descentDayFraction);
		descentStartPos = ComputeDescentStartPos(crashTarget);
		float num = Mathf.Clamp(secondsToImpact, 1f, FinalDescentSeconds);
		descentSecondsTaken = descentSecondsToTake - num;
		base.transform.position = ScheduledPhase2Pos(num);
	}

	private Vector3 ScheduledPhase2Pos(float secondsLeft)
	{
		float num = Mathf.Clamp01(secondsLeft / FinalDescentSeconds);
		if (Satellite.flyover_altitude > 0f)
		{
			Vector3 vector = descentStartPos - crashTarget;
			vector.y = 0f;
			vector = ((vector.sqrMagnitude > 0.01f) ? vector.normalized : RandomHorizontalDir());
			Vector3 p = crashTarget + vector * FlyoverDiveDistance;
			p.y = descentStartPos.y;
			FlyoverCurve flyoverCurve = new FlyoverCurve();
			flyoverCurve.Build(descentStartPos, p, crashTarget, FinalDescentSeconds);
			return flyoverCurve.EvalAtDistance(flyoverCurve.TotalLength * (1f - num));
		}
		return Vector3.Lerp(crashTarget, descentStartPos, num);
	}

	private void EnterDescentState()
	{
		if (!IsDescending)
		{
			StartFinalDescent();
		}
	}

	private void LogDescentProgress(float dt, string phase, float secondsToImpact)
	{
		if (Satellite.debug)
		{
			descentLogTimer += dt;
			if (!(descentLogTimer < 5f))
			{
				descentLogTimer = 0f;
				Vector3 position = base.transform.position;
				Debug.Log($"[SatelliteCrash] {phase} — impact in {secondsToImpact:F0}s, pos=({position.x:F1}, {position.y:F1}, {position.z:F1})");
			}
		}
	}

	private void Phase1Tick(float dt)
	{
		descentSecondsTaken += dt;
		DrawDescentDebugLine();
		LogDescentProgress(dt, "Phase 1 (orbital slide)", descentSecondsToTake - descentSecondsTaken);
		float num = descentSecondsToTake - FinalDescentSeconds;
		if (descentSecondsTaken < num)
		{
			float t = ((num > 0.01f) ? Mathf.Clamp01(descentSecondsTaken / num) : 1f);
			base.transform.position = Vector3.Lerp(Phase1StartPos(), descentStartPos, t);
			phase1NetTimer += dt;
			if (phase1NetTimer >= 0.5f)
			{
				phase1NetTimer = 0f;
				SendNetworkUpdate();
			}
		}
		else
		{
			base.transform.position = descentStartPos;
			TransitionToCrash();
		}
	}

	private void TransitionToCrash()
	{
		IsDescending = false;
		SendNetworkUpdate();
		if (explosionEffect.isValid)
		{
			Effect.server.Run(explosionEffect.resourcePath, base.transform.position, Vector3.up, null, broadcast: true);
		}
		crashTarget.y = TerrainMeta.HeightMap.GetHeight(crashTarget);
		StartFinalDescent();
	}

	private void StartFinalDescent()
	{
		float time = safetyDespawnTime;
		if (crashTarget != Vector3.zero)
		{
			float num = Mathf.Clamp(descentSecondsToTake - descentSecondsTaken, 1f, FinalDescentSeconds);
			if (ShouldFlyover())
			{
				StartFlyoverCurve(num);
			}
			else
			{
				Vector3 vector = crashTarget - base.transform.position;
				descentVelocity = vector.normalized * (vector.magnitude / num);
			}
			base.transform.rotation = Quaternion.LookRotation(descentVelocity.normalized);
			time = Mathf.Max(safetyDespawnTime, num + 30f);
			DrawImpactDebugSphere(crashTarget);
		}
		else
		{
			Vector3 position = base.transform.position;
			CalculateEntry(position, out var startPos, out var velocity);
			base.transform.position = startPos;
			descentVelocity = velocity;
			base.transform.rotation = Quaternion.LookRotation(velocity.normalized);
			DrawImpactDebugSphere(position);
		}
		Invoke(SafetyDespawn, time);
	}

	private bool ShouldFlyover()
	{
		if (Satellite.flyover_altitude <= 0f || crashTarget == Vector3.zero)
		{
			return false;
		}
		Vector3 vector = base.transform.position - crashTarget;
		vector.y = 0f;
		return vector.magnitude > FlyoverDiveDistance * 1.5f;
	}

	private void StartFlyoverCurve(float duration)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = position - crashTarget;
		vector.y = 0f;
		vector = ((vector.sqrMagnitude > 0.01f) ? vector.normalized : RandomHorizontalDir());
		Vector3 vector2 = crashTarget + vector * FlyoverDiveDistance;
		vector2.y = position.y;
		curve.Build(position, vector2, crashTarget, duration);
		descentVelocity = (vector2 - position).normalized * (curve.TotalLength / curve.Duration);
	}

	private void FixedUpdate()
	{
		if (!base.isServer || hasCrashed)
		{
			return;
		}
		if (IsDescending)
		{
			Phase1Tick(UnityEngine.Time.fixedDeltaTime);
			return;
		}
		descentSecondsTaken += UnityEngine.Time.fixedDeltaTime;
		DrawDescentDebugLine();
		if (curve.Active)
		{
			curve.Elapsed += UnityEngine.Time.fixedDeltaTime;
			Vector3 vector = curve.EvalAtDistance(curve.ElapsedArcDistance());
			descentVelocity = (vector - base.transform.position) / UnityEngine.Time.fixedDeltaTime;
			phase2NetTimer += UnityEngine.Time.fixedDeltaTime;
			if (phase2NetTimer >= 0.25f)
			{
				phase2NetTimer = 0f;
				SendNetworkUpdate();
			}
		}
		if (crashTarget != Vector3.zero)
		{
			Vector3 lhs = crashTarget - base.transform.position;
			float num = descentVelocity.magnitude * UnityEngine.Time.fixedDeltaTime;
			LogDescentProgress(UnityEngine.Time.fixedDeltaTime, "Phase 2 (final descent)", curve.Active ? (curve.Duration - curve.Elapsed) : (lhs.magnitude / Mathf.Max(0.01f, descentVelocity.magnitude)));
			if (lhs.magnitude <= num || Vector3.Dot(lhs, descentVelocity) <= 0f)
			{
				base.transform.position = crashTarget;
				PerformCrash();
				return;
			}
		}
		else
		{
			float height = TerrainMeta.HeightMap.GetHeight(base.transform.position);
			LogDescentProgress(UnityEngine.Time.fixedDeltaTime, "Phase 2 (final descent)", (base.transform.position.y - height - terrainImpactOffset) / Mathf.Max(0.01f, 0f - descentVelocity.y));
			if (base.transform.position.y <= height + terrainImpactOffset)
			{
				PerformCrash();
				return;
			}
		}
		base.transform.position += descentVelocity * UnityEngine.Time.fixedDeltaTime;
		if (descentVelocity.sqrMagnitude > 1f)
		{
			base.transform.rotation = Quaternion.LookRotation(descentVelocity.normalized);
		}
	}

	private void SafetyDespawn()
	{
		if (!hasCrashed)
		{
			NotifyControlComputer(crashed: false);
			Kill();
		}
	}

	private void CalculateEntry(Vector3 targetPos, out Vector3 startPos, out Vector3 velocity)
	{
		startPos = targetPos + RandomHorizontalDir() * startHeight + Vector3.up * startHeight;
		velocity = (targetPos - startPos).normalized * speed;
	}

	private void DrawImpactDebugSphere(Vector3 target)
	{
	}

	private void DrawDescentDebugLine()
	{
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.satelliteCrash = Facepunch.Pool.Get<ProtoBuf.SatelliteCrash>();
		info.msg.satelliteCrash.satelliteMass = satelliteMass;
		info.msg.satelliteCrash.isDescending = IsDescending;
		info.msg.satelliteCrash.descentStartPos = descentStartPos;
		info.msg.satelliteCrash.descentSecondsToTake = descentSecondsToTake;
		info.msg.satelliteCrash.descentSecondsTaken = descentSecondsTaken;
		info.msg.satelliteCrash.crashTarget = crashTarget;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		hasCrashed = false;
		computerNotified = false;
		globalBroadcast = true;
		if (!Rust.Application.isLoadingSave)
		{
			EnterDescentState();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		EnterDescentState();
	}

	public override void OnKilled()
	{
		base.OnKilled();
		NotifyControlComputer(crashed: false);
	}

	private void NotifyControlComputer(bool crashed)
	{
		if (computerNotified)
		{
			return;
		}
		computerNotified = true;
		if (!hasNoOwner)
		{
			SatelliteControlComputer satelliteControlComputer = (controlComputerId.IsValid ? (BaseNetworkable.serverEntities.Find(controlComputerId) as SatelliteControlComputer) : SatelliteControlComputer.ActiveDescending);
			if (satelliteControlComputer != null)
			{
				satelliteControlComputer.OnSatelliteCrashed(crashed);
			}
		}
	}

	private void PerformCrash()
	{
		if (!base.isServer || hasCrashed)
		{
			return;
		}
		hasCrashed = true;
		CancelInvoke(SafetyDespawn);
		Vector3 position = base.transform.position;
		position.y = TerrainMeta.HeightMap.GetHeight(position);
		base.transform.position = position;
		Vector3 scatterVelocity = descentVelocity * 0.25f;
		ClearArea(position);
		float num = massToLootScale.Evaluate(satelliteMass);
		int count = Mathf.RoundToInt((float)maxFireballs * num);
		float num2 = (float)baselineCrateSpawnCount * num;
		int num3 = Mathf.Clamp(Mathf.RoundToInt(num2), 1, Mathf.Max(1, maxCratesPerCrash));
		float lootScale = num2 / (float)num3;
		if (impactSoundEffect.isValid)
		{
			Effect.server.Run(impactSoundEffect.resourcePath, position, Vector3.up, null, broadcast: true);
		}
		if (debrisFieldMarker.isValid)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(debrisFieldMarker.resourcePath, position);
			if (baseEntity != null)
			{
				baseEntity.Spawn();
				baseEntity.SendMessage("SetDuration", debrisMarkerDurationMinutes, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (groundImpactEffect.isValid)
		{
			Vector3 forward = descentVelocity;
			forward.y = 0f;
			float f = ((forward.sqrMagnitude > 0.01f) ? Quaternion.LookRotation(forward).eulerAngles.y : 0f);
			Effect.server.Run(groundImpactEffect.resourcePath, position, Vector3.up, null, broadcast: true, null, Mathf.RoundToInt(f));
		}
		using PooledList<Collider> fireColliders = Facepunch.Pool.Get<PooledList<Collider>>();
		SpawnFireballs(position, scatterVelocity, count, fireColliders);
		SatelliteCrashRemains satelliteCrashRemains = null;
		if (impactEntityPrefab.isValid)
		{
			BaseEntity baseEntity2 = GameManager.server.CreateEntity(impactEntityPrefab.resourcePath, position, GetRemainsRotation(position, descentVelocity));
			if (baseEntity2 != null)
			{
				satelliteCrashRemains = baseEntity2 as SatelliteCrashRemains;
				if (satelliteCrashRemains != null)
				{
					satelliteCrashRemains.tooHotSeconds = Satellite.wreck_fire_duration;
					satelliteCrashRemains.thrusterModuleFuelFraction = FuelFractionAtLockIn;
				}
				baseEntity2.Spawn();
			}
		}
		SpawnLootCrates(position, fireColliders, num3, lootScale, satelliteCrashRemains);
		NotifyControlComputer(crashed: true);
		Kill(DestroyMode.Gib);
	}

	private static Quaternion GetRemainsRotation(Vector3 crashPos, Vector3 impactVelocity)
	{
		Vector3 up = Vector3.up;
		if (CrashSpotSearch.SampleFootprintPlane(crashPos, Satellite.site_footprint_radius, out var _, out var normal, out var _))
		{
			up = normal;
		}
		Vector3 vector = impactVelocity;
		vector.y = 0f;
		if (vector.sqrMagnitude < 0.01f)
		{
			vector = Vector3.forward;
		}
		return QuaternionEx.LookRotationForcedUp(vector.normalized, up);
	}

	private void ClearArea(Vector3 crashPos)
	{
		float kill_radius = Satellite.kill_radius;
		if (base.isServer && !(kill_radius <= 0f))
		{
			List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
			Vis.Entities(crashPos, kill_radius, obj, 1277853953);
			KillPlayers(obj);
			KillConstruction(obj);
			KillVegetation(obj);
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
	}

	private void KillConstruction(List<BaseEntity> entities)
	{
		foreach (BaseEntity entity in entities)
		{
			if (!(entity == null) && !entity.isClient && !entity.IsDestroyed && !(entity == this) && ((uint)(1 << entity.gameObject.layer) & 0x8208100u) != 0)
			{
				entity.Kill(DestroyMode.Gib);
			}
		}
	}

	private void KillVegetation(List<BaseEntity> entities)
	{
		foreach (BaseEntity entity in entities)
		{
			if (!(entity == null) && !entity.isClient && !entity.IsDestroyed && (entity is ResourceEntity || entity is CollectibleEntity || entity is BushEntity))
			{
				entity.Kill();
			}
		}
	}

	private void KillPlayers(List<BaseEntity> entities)
	{
		foreach (BaseEntity entity in entities)
		{
			if (entity is BasePlayer basePlayer && !basePlayer.IsDead() && !basePlayer.IsNpc && !basePlayer.isClient)
			{
				basePlayer.Hurt(1000f, DamageType.Explosion, this);
			}
		}
	}

	public static Vector3 ClampAwayFromMonuments(Vector3 pos, float exclusionDistance)
	{
		if (TerrainMeta.Path == null || TerrainMeta.Path.Monuments == null)
		{
			return pos;
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (!(monument == null) && monument.Distance(pos) < exclusionDistance)
			{
				Vector3 vector = pos - monument.transform.position;
				vector.y = 0f;
				if (vector.sqrMagnitude < 0.01f)
				{
					vector = Vector3.forward;
				}
				vector.Normalize();
				pos = monument.transform.position + vector * exclusionDistance;
				pos.y = TerrainMeta.HeightMap.GetHeight(pos);
			}
		}
		return pos;
	}

	private static Vector3 FindGroundedScatterPosition(Vector3 crashPos, float minRadius, float maxRadius)
	{
		for (int i = 0; i < 8; i++)
		{
			Vector3 pos = crashPos + RandomHorizontalDir() * UnityEngine.Random.Range(minRadius, maxRadius);
			pos = FindCrateFloor(pos, out var floorCollider, out var floorNormal);
			if (IsCrateSpotUsable(pos, floorCollider, floorNormal))
			{
				return pos;
			}
		}
		Collider floorCollider2;
		Vector3 floorNormal2;
		return FindCrateFloor(crashPos, out floorCollider2, out floorNormal2);
	}

	private static Vector3 RandomHorizontalDir()
	{
		float f = UnityEngine.Random.Range(0f, 360f) * (MathF.PI / 180f);
		return new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
	}

	private static Vector3 RandomUpwardScatter()
	{
		Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
		onUnitSphere.y = Mathf.Abs(onUnitSphere.y);
		return onUnitSphere;
	}

	private static void ConfigureScatterRigidbody(Rigidbody rb, float mass, float drag, float angularDrag, bool useGravity)
	{
		rb.useGravity = useGravity;
		rb.mass = mass;
		rb.drag = drag;
		rb.angularDrag = angularDrag;
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
	}

	private void SpawnFireballs(Vector3 crashPos, Vector3 scatterVelocity, int count, List<Collider> fireColliders)
	{
		if (!fireBall.isValid)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(fireBall.resourcePath, crashPos);
			if (!(baseEntity == null))
			{
				Vector3 vector = RandomUpwardScatter();
				baseEntity.transform.position = crashPos + Vector3.up * 1.5f + vector * UnityEngine.Random.Range(1f, 8f);
				baseEntity.Spawn();
				baseEntity.SetVelocity(scatterVelocity + vector * UnityEngine.Random.Range(3f, 15f));
				Collider component = baseEntity.GetComponent<Collider>();
				if (component != null)
				{
					fireColliders.Add(component);
				}
			}
		}
	}

	private static void GetShuffledCrateSpawnPoints(SatelliteCrashRemains remains, List<Transform> points)
	{
		if (remains == null || remains.crateSpawnPoints == null)
		{
			return;
		}
		Transform[] crateSpawnPoints = remains.crateSpawnPoints;
		foreach (Transform transform in crateSpawnPoints)
		{
			if (transform != null)
			{
				points.Add(transform);
			}
		}
		for (int num = points.Count - 1; num > 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num + 1);
			int i = num;
			int index = num2;
			Transform transform2 = points[num2];
			Transform transform3 = points[num];
			Transform transform5 = (points[i] = transform2);
			transform5 = (points[index] = transform3);
		}
	}

	private static Vector3 FindCrateFloor(Vector3 pos, out Collider floorCollider, out Vector3 floorNormal)
	{
		floorCollider = null;
		float height = TerrainMeta.HeightMap.GetHeight(pos);
		if (UnityEngine.Physics.Raycast(new Vector3(pos.x, Mathf.Max(pos.y, height) + 30f, pos.z), Vector3.down, out var hitInfo, 60f, 8454145, QueryTriggerInteraction.Ignore))
		{
			floorCollider = hitInfo.collider;
			floorNormal = hitInfo.normal;
			return hitInfo.point;
		}
		floorNormal = TerrainMeta.HeightMap.GetNormal(pos);
		pos.y = height;
		return pos;
	}

	private static bool IsCrateSpotUsable(Vector3 floorPos, Collider floorCollider, Vector3 floorNormal)
	{
		if (CrashSpotSearch.IsOutOfBounds(floorPos) || CrashSpotSearch.IsInWater(floorPos))
		{
			return false;
		}
		if (Vector3.Angle(floorNormal, Vector3.up) > 30f)
		{
			return false;
		}
		return IsCrateSpotClear(floorPos, floorCollider);
	}

	private static bool IsCrateSpotClear(Vector3 floorPos, Collider floorCollider)
	{
		int num = UnityEngine.Physics.OverlapSphereNonAlloc(floorPos + Vector3.up * 0.85f, 0.75f, crateClearanceBuffer, 1210263809, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < num; i++)
		{
			Collider collider = crateClearanceBuffer[i];
			if (!(collider == null) && !(collider == floorCollider))
			{
				return false;
			}
		}
		return true;
	}

	private static bool TryTakeCrateSpawnPoint(List<Transform> points, out Vector3 groundPos)
	{
		groundPos = default(Vector3);
		while (points.Count > 0)
		{
			Transform transform = points[points.Count - 1];
			points.RemoveAt(points.Count - 1);
			if (!(transform == null))
			{
				Collider floorCollider;
				Vector3 floorNormal;
				Vector3 vector = FindCrateFloor(transform.position, out floorCollider, out floorNormal);
				if (IsCrateSpotUsable(vector, floorCollider, floorNormal))
				{
					groundPos = vector;
					return true;
				}
			}
		}
		return false;
	}

	private void SpawnLootCrates(Vector3 crashPos, List<Collider> fireColliders, int count, float lootScale, SatelliteCrashRemains remains)
	{
		if (cratesToDrop == null || cratesToDrop.Length == 0)
		{
			return;
		}
		using PooledList<Transform> points = Facepunch.Pool.Get<PooledList<Transform>>();
		GetShuffledCrateSpawnPoints(remains, points);
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			GameObjectRef gameObjectRef = cratesToDrop[UnityEngine.Random.Range(0, cratesToDrop.Length)];
			if (gameObjectRef == null || !gameObjectRef.isValid)
			{
				continue;
			}
			if (!TryTakeCrateSpawnPoint(points, out var groundPos))
			{
				groundPos = FindGroundedScatterPosition(crashPos, 3f, 10f);
			}
			Vector3 pos = groundPos + Vector3.up * 0.5f;
			BaseEntity baseEntity = GameManager.server.CreateEntity(gameObjectRef.resourcePath, pos, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
			if (baseEntity == null)
			{
				continue;
			}
			LootContainer lootContainer = baseEntity as LootContainer;
			if (lootContainer != null && lootScale > 1f)
			{
				float num2 = (float)lootContainer.maxDefinitionsToSpawn * lootScale + num;
				int num3 = Mathf.Max(1, Mathf.RoundToInt(num2));
				num = num2 - (float)num3;
				lootContainer.maxDefinitionsToSpawn = num3;
				lootContainer.scrapAmount = Mathf.RoundToInt((float)lootContainer.scrapAmount * lootScale);
			}
			if (lootContainer != null)
			{
				lootContainer.clanScoreEventForFirstLooter = ClanScoreEventType.LootSatellite;
			}
			baseEntity.Spawn();
			if (lootContainer != null)
			{
				lootContainer.Invoke(lootContainer.RemoveMe, crateLifetimeMinutes * 60f);
			}
			Rigidbody rigidbody = baseEntity.gameObject.AddComponent<Rigidbody>();
			ConfigureScatterRigidbody(rigidbody, 2f, 0.2f, 0.080000006f, useGravity: true);
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			if (this.fireBall.isValid)
			{
				FireBall fireBall = GameManager.server.CreateEntity(this.fireBall.resourcePath) as FireBall;
				if (fireBall != null)
				{
					fireBall.SetParent(baseEntity);
					fireBall.Spawn();
					fireBall.GetComponent<Rigidbody>().isKinematic = true;
					fireBall.GetComponent<Collider>().enabled = false;
					fireBall.CancelInvoke(fireBall.TryToSpread);
					fireBall.CancelInvoke(fireBall.Extinguish);
					fireBall.Invoke(fireBall.Extinguish, Satellite.crate_fire_duration);
					baseEntity.SendMessage("SetLockingEnt", fireBall, SendMessageOptions.DontRequireReceiver);
				}
			}
			Collider component = baseEntity.GetComponent<Collider>();
			if (component == null)
			{
				continue;
			}
			foreach (Collider fireCollider in fireColliders)
			{
				if (fireCollider != null)
				{
					UnityEngine.Physics.IgnoreCollision(component, fireCollider, ignore: true);
				}
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.satelliteCrash != null)
		{
			satelliteMass = info.msg.satelliteCrash.satelliteMass;
			IsDescending = info.msg.satelliteCrash.isDescending;
			_ = info.msg.satelliteCrash.descentStartPos;
			descentStartPos = info.msg.satelliteCrash.descentStartPos;
			descentSecondsToTake = info.msg.satelliteCrash.descentSecondsToTake;
			descentSecondsTaken = info.msg.satelliteCrash.descentSecondsTaken;
			_ = info.msg.satelliteCrash.crashTarget;
			crashTarget = info.msg.satelliteCrash.crashTarget;
		}
	}
}
