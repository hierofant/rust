using System;
using ConVar;
using Oxide.Core;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Dead : FSMStateBase
{
	[SerializeField]
	private string deathStatName;

	[SerializeField]
	private GameObjectRef CorpsePrefab;

	[SerializeField]
	private RootMotionData staticDeathAnim;

	[SerializeField]
	private RootMotionData forwardMotionDeathAnim;

	[SerializeField]
	private float ragdollWhenAnimRemainingTimeIsBelow = 0.5f;

	[SerializeField]
	private LootContainer.LootSpawnSlot[] LootSpawnSlots;

	private RootMotionPlayer.PlayServerState animState;

	private Action _startRagdollAction;

	private Action StartRagdollAction => StartRagdoll;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (payload.hitInfo == null && AI.logIssues && AI.logIssues)
		{
			Debug.LogError($"Entering {base.Name} without HitInfo payload, this should not happen and may cause issues with stats tracking. Owner: {Owner}", Owner);
		}
		if (payload.hitInfo != null && payload.hitInfo.InitiatorPlayer != null && !payload.hitInfo.InitiatorPlayer.IsNpc)
		{
			BasePlayer initiatorPlayer = payload.hitInfo.InitiatorPlayer;
			if (BaseNetworkableEx.Is<BaseNPC2>(Owner, out var castedUnityObject) && castedUnityObject.IsAnimal)
			{
				initiatorPlayer.GiveAchievement("KILL_ANIMAL");
			}
			if (!string.IsNullOrEmpty(deathStatName))
			{
				initiatorPlayer.stats.Add(deathStatName, 1, (Stats)5);
				initiatorPlayer.stats.Save();
			}
			if (Owner is BaseCombatEntity killed)
			{
				initiatorPlayer.LifeStoryKill(killed);
			}
		}
		if (!CorpsePrefab.isValid)
		{
			Owner.Kill();
			return base.OnStateEnter(payload);
		}
		if (forwardMotionDeathAnim != null && base.Agent.speed > base.Agent.GetSpeedForGait(RustNavMeshAgent.Speeds.Run))
		{
			animState = base.AnimPlayer.PlayServerAndTakeFromPool(forwardMotionDeathAnim);
			float num = Mathf.Max(0f, forwardMotionDeathAnim.inPlaceAnimation.length - ragdollWhenAnimRemainingTimeIsBelow);
			Owner.Invoke(StartRagdollAction, num + AI.defaultInterpolationDelay);
		}
		else if (staticDeathAnim != null && payload.hitInfo != null && Vector3.Dot(payload.hitInfo.attackNormal, Owner.transform.forward) < 0f)
		{
			animState = base.AnimPlayer.PlayServerAndTakeFromPool(staticDeathAnim);
			float num2 = Mathf.Max(0f, staticDeathAnim.inPlaceAnimation.length - ragdollWhenAnimRemainingTimeIsBelow);
			Owner.Invoke(StartRagdollAction, num2 + AI.defaultInterpolationDelay);
		}
		else
		{
			StartRagdoll();
		}
		return base.OnStateEnter(payload);
	}

	private void StartRagdoll()
	{
		BaseCorpse baseCorpse = Owner.DropCorpse(CorpsePrefab.resourcePath);
		if (BaseNetworkableEx.Is<LootableCorpse>(baseCorpse, out var castedUnityObject))
		{
			castedUnityObject.TakeFrom(Owner, CreateInventory());
			PrefabInformation prefabInformation = PrefabAttribute.server.Find<PrefabInformation>(Owner.prefabID);
			if (prefabInformation != null && prefabInformation.title.IsValid())
			{
				castedUnityObject.playerName = prefabInformation.title.translated;
			}
			else
			{
				castedUnityObject.playerName = Owner.GetType().Name;
			}
			castedUnityObject.Spawn();
			baseCorpse.TakeChildren(Owner);
			if (Interface.CallHook("OnCorpsePopulate", Owner, castedUnityObject) == null && LootSpawnSlots.Length != 0)
			{
				LootContainer.LootSpawnSlot[] lootSpawnSlots = LootSpawnSlots;
				for (int i = 0; i < lootSpawnSlots.Length; i++)
				{
					LootContainer.LootSpawnSlot lootSpawnSlot = lootSpawnSlots[i];
					for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
					{
						if (UnityEngine.Random.Range(0f, 1f) <= lootSpawnSlot.probability)
						{
							lootSpawnSlot.definition.SpawnIntoContainer(castedUnityObject.containers[0]);
						}
					}
				}
			}
		}
		else if (baseCorpse != null)
		{
			baseCorpse.Spawn();
			baseCorpse.TakeChildren(Owner);
		}
		Owner.Invoke(Owner.KillMessage, 0.5f);
	}

	public override void OnStateExit()
	{
		base.AnimPlayer.StopServerAndReturnToPool(ref animState);
		base.OnStateExit();
	}

	private ItemContainer CreateInventory()
	{
		ItemContainer itemContainer = new ItemContainer
		{
			entityOwner = Owner
		};
		itemContainer.ServerInitialize(null, 24);
		if (!itemContainer.uid.IsValid)
		{
			itemContainer.GiveUID();
		}
		return itemContainer;
	}
}
