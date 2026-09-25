using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;

[ExecuteInEditMode]
public class TreeEntity : ResourceEntity, IPrefabPreProcess
{
	private struct HotspotMarkerSpawnValues
	{
		public Vector3 fromWorldPosition;

		public Vector3 hitPositionWorld;

		public Vector3 initiatorCenterPoint;

		public bool isBypassingBonusGame;

		public TreeEntity hitEntity;
	}

	[Header("Falling")]
	public bool fallOnDied = true;

	public float fallDuration = 1.5f;

	public GameObjectRef fallStartSound;

	public GameObjectRef fallImpactSound;

	public GameObjectRef fallImpactParticles;

	public SoundDefinition fallLeavesLoopDef;

	[NonSerialized]
	public bool[] usedHeights = new bool[20];

	public bool impactSoundPlayed;

	private float treeDistanceUponFalling;

	public GameObjectRef prefab;

	public bool hasBonusGame = true;

	public GameObjectRef bonusHitEffect;

	public GameObjectRef bonusHitSound;

	public Collider serverCollider;

	public Collider clientCollider;

	public SoundDefinition smallCrackSoundDef;

	public SoundDefinition medCrackSoundDef;

	private float lastAttackDamage;

	[Header("Tree Addition Settings")]
	public bool spawnTreeAddition;

	public GameObjectRef treeAdditionPrefab;

	public float treeAdditionSpawnChance = 0.1f;

	public Vector3 treeAdditionSpawnPosition;

	public Vector3 treeAdditionSpawnRotation;

	private BaseEntity treeAdditionRef;

	private HotspotMarkerSpawnValues nextHotspotMarkerValues;

	private Action _actionCreateNewHotspotMarker;

	public BaseEntity xMarker;

	private int currentBonusLevel;

	private float lastDirection = -1f;

	private float lastHitTime;

	private int lastHitMarkerIndex = -1;

	private float nextBirdTime;

	private uint birdCycleIndex;

	public virtual bool IncludeInNavmesh => false;

	private Action actionCreateNewHotspotMarker
	{
		get
		{
			if (_actionCreateNewHotspotMarker == null)
			{
				_actionCreateNewHotspotMarker = SpawnNewHotspotMarker;
			}
			return _actionCreateNewHotspotMarker;
		}
	}

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TreeEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override float AntiHackPadding()
	{
		return 1f;
	}

	public override void OnAttacked(HitInfo info)
	{
		bool canGather = info.CanGather;
		float time = UnityEngine.Time.time;
		float num = time - lastHitTime;
		lastHitTime = time;
		DoBirds();
		bool flag = false;
		float num2 = 1f;
		if (info.Weapon != null && info.Weapon.TryGetOwnerPlayer(out var ownerPlayer) && ResourceGatherItemConfig.instance.CanItemBypassHotspotGathering(resourceDispenser.gatherType, info.Weapon.GetOwnerItemDefinition(ownerPlayer), out var itemData))
		{
			num2 = itemData.hotspotGatherBonusScale;
			flag = true;
		}
		if (!hasBonusGame || !canGather || info.Initiator == null || (BonusActive() && !flag && !DidHitMarker(info)))
		{
			base.OnAttacked(info);
			return;
		}
		bool flag2 = xMarker != null;
		if ((flag || flag2) && !info.DidGather && info.gatherScale > 0f)
		{
			Vector3 arg;
			Vector3 arg2;
			if (flag2 && !flag)
			{
				arg = xMarker.transform.position;
				arg2 = xMarker.transform.up;
			}
			else
			{
				arg = info.HitPositionWorld;
				arg2 = info.HitNormalWorld;
			}
			ClientRPC(RpcTarget.NetworkGroup("HotspotHit"), arg, arg2, currentBonusLevel);
			currentBonusLevel++;
			info.gatherScale = 1f + Mathf.Clamp((float)currentBonusLevel * 0.125f, 0f, 1f * num2);
		}
		Vector3 fromWorldPosition = (flag2 ? xMarker.transform.position : info.HitPositionWorld);
		CleanupMarker();
		nextHotspotMarkerValues = new HotspotMarkerSpawnValues
		{
			fromWorldPosition = fromWorldPosition,
			hitPositionWorld = info.HitPositionWorld,
			initiatorCenterPoint = info.Initiator.CenterPoint(),
			isBypassingBonusGame = flag,
			hitEntity = this
		};
		if (num > 5f)
		{
			StartBonusGame();
		}
		base.OnAttacked(info);
		if (health > 0f)
		{
			if (!flag)
			{
				SpawnNewHotspotMarker();
			}
			lastAttackDamage = info.damageTypes.Total();
			int num3 = Mathf.CeilToInt(health / lastAttackDamage);
			if (num3 < 2)
			{
				ClientRPC(RpcTarget.NetworkGroup("CrackSound"), 1);
			}
			else if (num3 < 5)
			{
				ClientRPC(RpcTarget.NetworkGroup("CrackSound"), 0);
			}
		}
	}

	public override void ServerInit()
	{
		if (serverCollider == null)
		{
			serverCollider = clientCollider ?? GetComponentInChildren<Collider>();
		}
		base.ServerInit();
		lastDirection = ((UnityEngine.Random.Range(0, 2) != 0) ? 1 : (-1));
		TryAddTreeAddition();
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		base.ServerInitPostNetworkGroupAssign();
		TreeManager.OnTreeSpawned(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		CleanupMarker();
		TryKillTreeAddition();
		TreeManager.OnTreeDestroyed(this);
	}

	public bool DidHitMarker(HitInfo info)
	{
		if (xMarker == null)
		{
			return false;
		}
		object obj = Interface.CallHook("OnTreeMarkerHit", this, info);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (PrefabAttribute.server.Find<TreeMarkerData>(prefabID) != null)
		{
			if (new Bounds(xMarker.transform.position, Vector3.one * 0.2f).Contains(info.HitPositionWorld))
			{
				return true;
			}
		}
		else
		{
			Vector3 lhs = Vector3Ex.Direction2D(base.transform.position, xMarker.transform.position);
			Vector3 attackNormal = info.attackNormal;
			float num = Vector3.Dot(lhs, attackNormal);
			float num2 = Vector3.SqrMagnitude(xMarker.transform.position - info.HitPositionWorld);
			if (num >= 0.3f && num2 <= 0.040000003f)
			{
				return true;
			}
		}
		return false;
	}

	private void SpawnNewHotspotMarker()
	{
		if (health <= 0f)
		{
			return;
		}
		TreeMarkerData treeMarkerData = PrefabAttribute.server.Find<TreeMarkerData>(prefabID);
		if (treeMarkerData != null)
		{
			Vector3 normal;
			Vector3 closestPoint;
			if (nextHotspotMarkerValues.isBypassingBonusGame)
			{
				if (!treeMarkerData.TryGetClosestPoint(nextHotspotMarkerValues.hitPositionWorld, nextHotspotMarkerValues.hitEntity, out closestPoint, out normal))
				{
					Debug.LogError($"Failed to generate a closest point to spawn new marker from position {nextHotspotMarkerValues.fromWorldPosition}");
					return;
				}
			}
			else
			{
				closestPoint = treeMarkerData.GetNearbyPoint(nextHotspotMarkerValues.fromWorldPosition, nextHotspotMarkerValues.hitEntity, ref lastHitMarkerIndex, out normal);
			}
			closestPoint = base.transform.TransformPoint(closestPoint);
			Quaternion rot = QuaternionEx.LookRotationNormal(base.transform.TransformDirection(normal));
			xMarker = GameManager.server.CreateEntity("assets/content/nature/treesprefabs/trees/effects/tree_marking_nospherecast.prefab", closestPoint, rot);
		}
		else
		{
			Vector3 vector = Vector3Ex.Direction2D(base.transform.position, nextHotspotMarkerValues.fromWorldPosition);
			Vector3 vector2;
			if (nextHotspotMarkerValues.isBypassingBonusGame)
			{
				vector2 = vector;
			}
			else
			{
				Vector3 vector3 = Vector3.Cross(vector, Vector3.up);
				float num = lastDirection;
				float t = UnityEngine.Random.Range(0.5f, 0.5f);
				vector2 = Vector3.Lerp(-vector, vector3 * num, t);
			}
			Vector3 position = base.transform.InverseTransformDirection(vector2.normalized) * 2.5f;
			position = base.transform.InverseTransformPoint(serverCollider.ClosestPoint(base.transform.TransformPoint(position)));
			Vector3 aimFrom = base.transform.TransformPoint(position);
			position.y = base.transform.InverseTransformPoint(nextHotspotMarkerValues.hitPositionWorld).y;
			Vector3 vector4 = base.transform.InverseTransformPoint(nextHotspotMarkerValues.initiatorCenterPoint);
			float min = Mathf.Max(0.75f, vector4.y);
			float max = vector4.y + 0.5f;
			position.y = Mathf.Clamp(position.y + UnityEngine.Random.Range(0.1f, 0.2f) * ((UnityEngine.Random.Range(0, 2) == 0) ? (-1f) : 1f), min, max);
			Vector3 direction = Vector3Ex.Direction2D(base.transform.position, aimFrom);
			direction = base.transform.InverseTransformDirection(direction);
			Quaternion quaternion = QuaternionEx.LookRotationNormal(-direction, Vector3.zero);
			position = base.transform.TransformPoint(position);
			position = serverCollider.ClosestPoint(position);
			quaternion = QuaternionEx.LookRotationNormal(-Vector3Ex.Direction(new Line(serverCollider.transform.TransformPoint(new Vector3(0f, 10f, 0f)), serverCollider.transform.TransformPoint(new Vector3(0f, -10f, 0f))).ClosestPoint(position), position));
			xMarker = GameManager.server.CreateEntity("assets/content/nature/treesprefabs/trees/effects/tree_marking.prefab", position, quaternion);
		}
		xMarker.Spawn();
	}

	private void DelayedHotspotMarkerSpawn()
	{
		CancelInvoke(actionCreateNewHotspotMarker);
		Invoke(actionCreateNewHotspotMarker, 0.5f);
	}

	public float GetLastHitTime()
	{
		return lastHitTime;
	}

	public void StartBonusGame()
	{
		if (IsInvoking(StopBonusGame))
		{
			CancelInvoke(StopBonusGame);
		}
		Invoke(StopBonusGame, 60f);
	}

	public void StopBonusGame()
	{
		CleanupMarker();
		lastHitTime = 0f;
		currentBonusLevel = 0;
	}

	public bool BonusActive()
	{
		return xMarker != null;
	}

	private void DoBirds()
	{
		if (!base.isClient && !(UnityEngine.Time.realtimeSinceStartup < nextBirdTime) && !(bounds.extents.y < 6f))
		{
			uint seed = (uint)(int)net.ID.Value + birdCycleIndex;
			if (SeedRandom.Range(ref seed, 0, 2) == 0)
			{
				Effect.server.Run("assets/prefabs/npc/birds/birdemission.prefab", base.transform.position + Vector3.up * UnityEngine.Random.Range(bounds.extents.y * 0.65f, bounds.extents.y * 0.9f), Vector3.up);
			}
			birdCycleIndex++;
			nextBirdTime = UnityEngine.Time.realtimeSinceStartup + 90f;
		}
	}

	public void CleanupMarker()
	{
		if ((bool)xMarker)
		{
			xMarker.Kill();
		}
		xMarker = null;
	}

	public override void OnDied(HitInfo info)
	{
		if (isKilled)
		{
			return;
		}
		isKilled = true;
		CleanupMarker();
		if (base.isServer)
		{
			StabilityEntity.updateSurroundingsQueue.Add(WorldSpaceBounds().ToBounds());
			TryKillTreeAddition();
		}
		if (fallOnDied)
		{
			Collider collider = serverCollider;
			if ((bool)collider)
			{
				collider.enabled = false;
			}
			Vector3 vector = info.attackNormal;
			if (vector == Vector3.zero)
			{
				vector = Vector3Ex.Direction2D(base.transform.position, info.PointStart);
			}
			using PooledList<TimedExplosive> pooledList = Facepunch.Pool.Get<PooledList<TimedExplosive>>();
			foreach (BaseEntity child in children)
			{
				if (child is TimedExplosive item)
				{
					pooledList.Add(item);
				}
			}
			foreach (TimedExplosive item2 in pooledList)
			{
				item2.UnStick();
			}
			OnFallServer();
			ClientRPC(RpcTarget.NetworkGroup("TreeFall"), vector);
			Invoke(DelayedKill, fallDuration + 1f);
			return;
		}
		DelayedKill();
	}

	protected virtual void OnFallServer()
	{
	}

	public void DelayedKill()
	{
		Kill();
	}

	private void TryAddTreeAddition()
	{
		if (spawnTreeAddition && treeAdditionPrefab.isValid && UnityEngine.Random.value <= treeAdditionSpawnChance && !(treeAdditionRef != null))
		{
			treeAdditionRef = GameManager.server.CreateEntity(treeAdditionPrefab.resourcePath, Vector3.zero, Quaternion.identity);
			treeAdditionRef.transform.position = base.transform.TransformPoint(treeAdditionSpawnPosition);
			treeAdditionRef.transform.rotation = base.transform.rotation * Quaternion.Euler(treeAdditionSpawnRotation);
			if (treeAdditionRef.GetComponent<Poolable>() != null)
			{
				PoolableEx.AwakeFromInstantiate(treeAdditionRef.gameObject);
			}
			treeAdditionRef.Spawn();
			treeAdditionRef.SendNetworkUpdate();
		}
	}

	private void TryKillTreeAddition()
	{
		if (spawnTreeAddition && treeAdditionRef != null)
		{
			if (treeAdditionRef is BaseCombatEntity baseCombatEntity)
			{
				baseCombatEntity.Die();
			}
			else
			{
				treeAdditionRef.Kill(DestroyMode.Gib);
			}
			treeAdditionRef = null;
		}
	}

	public BaseEntity GetBonusGame()
	{
		return xMarker;
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (serverside)
		{
			globalBroadcast = ConVar.Tree.global_broadcast;
		}
	}
}
