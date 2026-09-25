#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseMelee : AttackEntity
{
	[Serializable]
	public class MaterialFX
	{
		public string materialName;

		public GameObjectRef fx;
	}

	[Header("Throwing")]
	public bool canThrowAsProjectile;

	public bool canThrowAsEntity;

	public bool canAiHearIt;

	public bool canScareAiWhenAimed;

	public bool onlyThrowAsProjectile;

	public bool ThrowFullStack = true;

	[Header("Melee")]
	public DamageProperties damageProperties;

	public List<DamageTypeEntry> damageTypes;

	public List<DamageTypeEntry> deployableDamageOverrides;

	public float maxDistance = 1.5f;

	public float attackRadius = 0.3f;

	public bool isAutomatic = true;

	public bool blockSprintOnAttack = true;

	public bool canUntieCrates;

	public bool longResourceForgiveness;

	[Header("Third Person Animation")]
	public MeleeWeaponAnimationSubSystem PlayerAnimSystem;

	[Header("Effects")]
	public GameObjectRef strikeFX;

	public bool useStandardHitEffects = true;

	[Header("NPCUsage")]
	public float aiStrikeDelay = 0.2f;

	public GameObjectRef swingEffect;

	public List<MaterialFX> materialStrikeFX = new List<MaterialFX>();

	[Range(0f, 1f)]
	[Header("Other")]
	public float heartStress = 0.5f;

	public ResourceDispenser.GatherProperties gathering;

	public bool canThrowCheck
	{
		get
		{
			if (!canThrowAsProjectile)
			{
				return canThrowAsEntity;
			}
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMelee.OnRpcMessage"))
		{
			if (rpc == 2215098782u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - CLEntityThrow");
				}
				using (TimeWarning.New("CLEntityThrow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2215098782u, "CLEntityThrow", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2215098782u, "CLEntityThrow", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							CLEntityThrow(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in CLEntityThrow");
					}
				}
				return true;
			}
			if (rpc == 3168282921u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - CLProject");
				}
				using (TimeWarning.New("CLProject"))
				{
					using (msg.read.UseRepeatedElementLimit(1))
					{
						using (TimeWarning.New("Conditions"))
						{
							if (!RPC_Server.FromOwner.Test(3168282921u, "CLProject", this, player))
							{
								return true;
							}
							if (!RPC_Server.IsActiveItem.Test(3168282921u, "CLProject", this, player))
							{
								return true;
							}
						}
						try
						{
							using (TimeWarning.New("Call"))
							{
								RPCMessage rPCMessage = default(RPCMessage);
								rPCMessage.connection = msg.connection;
								rPCMessage.player = player;
								rPCMessage.read = msg.read;
								RPCMessage msg3 = rPCMessage;
								CLProject(msg3);
							}
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
							player.Kick("RPC Error in CLProject");
						}
					}
				}
				return true;
			}
			if (rpc == 4088326849u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - PlayerAttack");
				}
				using (TimeWarning.New("PlayerAttack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4088326849u, "PlayerAttack", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							PlayerAttack(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in PlayerAttack");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player, Vector3 direction)
	{
		return player.GetInheritedThrowVelocity(direction);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void CLEntityThrow(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else
		{
			if (player == null || player.IsHeadUnderwater())
			{
				return;
			}
			if (!canThrowAsEntity)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Not throwable (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "not_throwable");
				return;
			}
			Item item = GetItem();
			if (item == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "item_missing");
				return;
			}
			ItemModEntityThrow component = item.info.GetComponent<ItemModEntityThrow>();
			if (component == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "mod_missing");
				return;
			}
			Vector3 vector = msg.read.Vector3();
			Vector3 normalized = msg.read.Vector3().normalized;
			if (msg.player.isMounted || msg.player.HasParent())
			{
				vector = msg.player.eyes.position;
			}
			else if (!ValidateEyePos(msg.player, vector))
			{
				return;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(component.entityPrefab.resourcePath, vector, Quaternion.LookRotation(normalized));
			if (baseEntity == null)
			{
				return;
			}
			baseEntity.SetCreatorEntity(player);
			baseEntity.skinID = skinID;
			baseEntity.OwnerID = player.userID;
			baseEntity.SetVelocity(GetInheritedVelocity(msg.player, normalized) + normalized * component.throwVelocity + msg.player.estimatedVelocity * 0.5f);
			baseEntity.Spawn();
			if (component.consumeOnThrow)
			{
				if (ThrowFullStack)
				{
					item.SetParent(null);
				}
				else
				{
					item.UseItem();
					if (item.amount == 0)
					{
						item.SetParent(null);
					}
				}
			}
			SingletonComponent<NpcNoiseManager>.Instance.OnWeaponThrown(player, this, canAiHearIt);
			OnEntityThrow(baseEntity);
		}
	}

	protected virtual void OnEntityThrow(BaseEntity ent)
	{
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.MaxRepeatedElements(1)]
	[RPC_Server.FromOwner]
	private void CLProject(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else
		{
			if (player == null || player.IsHeadUnderwater())
			{
				return;
			}
			if (!canThrowAsProjectile)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Not throwable (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "not_throwable");
				return;
			}
			Item item = GetItem();
			if (item == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "item_missing");
				return;
			}
			ItemModProjectile component = item.info.GetComponent<ItemModProjectile>();
			if (!(component == null))
			{
				using (ProjectileShoot projectileShoot = msg.read.Proto<ProjectileShoot>())
				{
					if (projectileShoot.projectiles.Count != 1)
					{
						AntiHack.Log(player, AntiHackType.ProjectileHack, "Projectile count mismatch (" + base.ShortPrefabName + ")");
						player.stats.combat.LogInvalid(player, this, "count_mismatch");
					}
					else
					{
						player.CleanupExpiredProjectiles();
						Guid projectileGroupId = Guid.NewGuid();
						foreach (ProjectileShoot.Projectile projectile in projectileShoot.projectiles)
						{
							if (player.HasFiredProjectile(projectile.projectileID))
							{
								AntiHack.Log(player, AntiHackType.ProjectileHack, $"Duplicate ID ({projectile.projectileID})");
								player.stats.combat.LogInvalid(player, this, "duplicate_id");
							}
							else
							{
								Vector3 positionOffset = Vector3.zero;
								if (ConVar.AntiHack.projectile_positionoffset && (player.isMounted || player.HasParent()))
								{
									if (!ValidateEyePos(player, projectile.startPos, checkLineOfSight: false))
									{
										continue;
									}
									Vector3 position = player.eyes.position;
									positionOffset = position - projectile.startPos;
									projectile.startPos = position;
								}
								else if (!ValidateEyePos(player, projectile.startPos))
								{
									continue;
								}
								Item pickupItem = (ThrowFullStack ? item : ItemManager.CreateByItemID(item.info.itemid, 1, 0uL, 0uL));
								player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, this, item.info, projectileGroupId, positionOffset, pickupItem);
								Effect effect = new Effect();
								effect.Init(Effect.Type.Projectile, projectile.startPos, projectile.startVel, msg.connection);
								effect.scale = 1f;
								effect.pooledString = component.GetOverrideProjectile(this).resourcePath;
								effect.number = projectile.seed;
								EffectNetwork.Send(effect);
							}
						}
						if (ThrowFullStack)
						{
							item.SetParent(null);
						}
						else
						{
							item.UseItem();
							if (item.amount == 0)
							{
								item.SetParent(null);
							}
						}
						Interface.CallHook("OnMeleeThrown", player, item);
						SingletonComponent<NpcNoiseManager>.Instance.OnWeaponThrown(player, this, canAiHearIt);
					}
					return;
				}
			}
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "mod_missing");
		}
	}

	public override void GetAttackStats(HitInfo info)
	{
		List<DamageTypeEntry> entries = damageTypes;
		if (deployableDamageOverrides != null && deployableDamageOverrides.Count > 0 && info.HitEntity is DecayEntity && !(info.HitEntity is BuildingBlock) && !(info.HitEntity is BasePlayer) && !(info.HitEntity is Door) && !(info.HitEntity is SimpleBuildingBlock) && !(info.HitEntity is LootContainer))
		{
			entries = deployableDamageOverrides;
		}
		info.damageTypes.Add(entries);
		info.CanGather = gathering.Any();
	}

	public virtual void DoAttackShared(HitInfo info)
	{
		if (Interface.CallHook("OnPlayerAttack", GetOwnerPlayer(), info) != null)
		{
			return;
		}
		GetAttackStats(info);
		if (info.HitEntity != null)
		{
			using (TimeWarning.New("OnAttacked", 50))
			{
				info.HitEntity.OnAttacked(info);
			}
		}
		if (info.DoHitEffects && base.isServer)
		{
			using (TimeWarning.New("ImpactEffect", 20))
			{
				Effect.server.ImpactEffect(info);
			}
			if (!base.IsDestroyed)
			{
				SingletonComponent<NpcNoiseManager>.Instance.OnMeleeHit(this, info);
			}
		}
		if (base.isServer && !base.IsDestroyed)
		{
			using (TimeWarning.New("UpdateItemCondition", 50))
			{
				UpdateItemCondition(info);
			}
			StartAttackCooldown(repeatDelay);
		}
	}

	public ResourceDispenser.GatherPropertyEntry GetGatherInfoFromIndex(ResourceDispenser.GatherType index)
	{
		return gathering.GetFromIndex(index);
	}

	public virtual bool CanHit(HitTest info)
	{
		return true;
	}

	public float TotalDamage()
	{
		float num = 0f;
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			if (!(damageType.amount <= 0f))
			{
				num += damageType.amount;
			}
		}
		return num;
	}

	public bool IsItemBroken()
	{
		return GetOwnerItem()?.isBroken ?? true;
	}

	public void LoseCondition(float amount)
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null && !base.UsingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(amount);
		}
	}

	public virtual float GetConditionLoss()
	{
		return 1f;
	}

	public void UpdateItemCondition(HitInfo info)
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null || !ownerItem.hasCondition || info == null || !info.DidHit || info.DidGather)
		{
			return;
		}
		float conditionLoss = GetConditionLoss();
		float num = 0f;
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			if (!(damageType.amount <= 0f))
			{
				num += Mathf.Clamp(damageType.amount - info.damageTypes.Get(damageType.type), 0f, damageType.amount);
			}
		}
		conditionLoss += num * 0.2f;
		if (!base.UsingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(conditionLoss);
		}
	}

	private static bool MeleeLineOfSightEntity(Vector3 p0_playerEyesCenter, Vector3 p1_playerEyes, Vector3 p2_hitRaycastStartPos, Vector3 p3_closestRayPos, Vector3 p4_worldHitPos, int lineOfSightLayerMask)
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		Vector3 vector3 = Vector3.zero;
		if (ConVar.AntiHack.melee_backtracking > 0f)
		{
			vector = (p1_playerEyes - p0_playerEyesCenter).normalized * ConVar.AntiHack.melee_backtracking;
			vector2 = (p2_hitRaycastStartPos - p1_playerEyes).normalized * ConVar.AntiHack.melee_backtracking;
			vector3 = (p3_closestRayPos - p2_hitRaycastStartPos).normalized * ConVar.AntiHack.melee_backtracking;
		}
		if (!GamePhysics.LineOfSight(p0_playerEyesCenter - vector, p1_playerEyes + vector, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1_playerEyes - vector2, p2_hitRaycastStartPos + vector2, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p2_hitRaycastStartPos - vector3, p3_closestRayPos, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p3_closestRayPos, p4_worldHitPos, lineOfSightLayerMask))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1_playerEyes, p4_worldHitPos, lineOfSightLayerMask))
		{
			return false;
		}
		return true;
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void PlayerAttack(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		using (TimeWarning.New("PlayerAttack", 50))
		{
			using PlayerAttack playerAttack = msg.read.Proto<PlayerAttack>();
			if (playerAttack == null)
			{
				return;
			}
			HitInfo obj = Facepunch.Pool.Get<HitInfo>();
			obj.LoadFromAttack(playerAttack.attack, serverSide: true);
			obj.Initiator = player;
			obj.Weapon = this;
			obj.WeaponPrefab = this;
			obj.Predicted = msg.connection;
			obj.damageProperties = damageProperties;
			if (Interface.CallHook("OnMeleeAttack", player, obj) != null)
			{
				return;
			}
			if (obj.IsNaNOrInfinity())
			{
				string shortPrefabName = base.ShortPrefabName;
				AntiHack.Log(player, AntiHackType.MeleeHack, "Contains NaN (" + shortPrefabName + ")");
				player.stats.combat.LogInvalid(obj, "melee_nan");
				return;
			}
			BaseEntity hitEntity = obj.HitEntity;
			BasePlayer basePlayer = obj.HitEntity as BasePlayer;
			bool flag = basePlayer != null;
			bool flag2 = flag && basePlayer.IsSleeping();
			bool flag3 = flag && basePlayer.IsWounded();
			bool flag4 = flag && basePlayer.isMounted;
			bool flag5 = flag && basePlayer.HasParent();
			bool flag6 = hitEntity != null;
			bool flag7 = flag6 && hitEntity.IsNpc;
			if (ConVar.AntiHack.melee_protection > 0)
			{
				bool flag8 = true;
				float num = 1f + ConVar.AntiHack.melee_forgiveness;
				float melee_clientframes = ConVar.AntiHack.melee_clientframes;
				float melee_serverframes = ConVar.AntiHack.melee_serverframes;
				float num2 = melee_clientframes / 60f;
				float num3 = melee_serverframes * Mathx.Max(UnityEngine.Time.deltaTime, UnityEngine.Time.smoothDeltaTime, UnityEngine.Time.fixedDeltaTime);
				float num4 = (player.desyncTimeClamped + num2 + num3) * num;
				int num5 = 1075904512;
				if (ConVar.AntiHack.melee_terraincheck)
				{
					num5 |= 0x800000;
				}
				if (ConVar.AntiHack.melee_vehiclecheck)
				{
					num5 |= 0x8000000;
				}
				if (flag && obj.boneArea == (HitArea)(-1))
				{
					string shortPrefabName2 = base.ShortPrefabName;
					string shortPrefabName3 = basePlayer.ShortPrefabName;
					AntiHack.Log(player, AntiHackType.MeleeHack, $"Bone is invalid  ({shortPrefabName2} on {shortPrefabName3} bone {obj.HitBone})");
					player.stats.combat.LogInvalid(obj, "melee_bone");
					flag8 = false;
				}
				if (ConVar.AntiHack.melee_protection >= 2)
				{
					if (flag6)
					{
						float num6 = hitEntity.AntiHackVelocity() + hitEntity.GetParentVelocity().magnitude;
						float num7 = hitEntity.AntiHackPadding() + num4 * num6;
						float num8 = hitEntity.Distance(obj.HitPositionWorld);
						if (num8 > num7)
						{
							string shortPrefabName4 = base.ShortPrefabName;
							string shortPrefabName5 = hitEntity.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Entity too far away ({shortPrefabName4} on {shortPrefabName5} with {num8}m > {num7}m in {num4}s)");
							player.stats.combat.LogInvalid(obj, "melee_target");
							flag8 = false;
						}
					}
					if (ConVar.AntiHack.melee_protection >= 4 && flag8 && flag && !flag7 && !flag2 && !flag3 && !flag4 && !flag5)
					{
						float magnitude = basePlayer.GetParentVelocity().magnitude;
						float num9 = basePlayer.AntiHackPadding() + num4 * magnitude + ConVar.AntiHack.tickhistoryforgiveness;
						float num10 = basePlayer.tickHistory.Distance(basePlayer, obj.HitPositionWorld);
						if (num10 > num9)
						{
							string shortPrefabName6 = base.ShortPrefabName;
							string shortPrefabName7 = basePlayer.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Player too far away ({shortPrefabName6} on {shortPrefabName7} with {num10}m > {num9}m in {num4}s)");
							player.stats.combat.LogInvalid(obj, "player_distance");
							flag8 = false;
						}
					}
					if (ConVar.AntiHack.melee_protection >= 4 && flag8 && flag && !flag7 && !flag2 && !flag3 && flag5 && ConVar.AntiHack.parenthistory && basePlayer.tickHistory.ParentCount > 0)
					{
						float magnitude2 = basePlayer.GetParentVelocity().magnitude;
						float num11 = basePlayer.AntiHackPadding() + num4 * magnitude2 + ConVar.AntiHack.tickhistoryforgiveness;
						float num12 = basePlayer.tickHistory.DistanceParented(basePlayer, obj.HitPositionWorld);
						if (num12 > num11)
						{
							string shortPrefabName8 = base.ShortPrefabName;
							string shortPrefabName9 = basePlayer.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Player (parented) too far away ({shortPrefabName8} on {shortPrefabName9} with {num12}m > {num11}m in {num4}s)");
							player.stats.combat.LogInvalid(obj, "player_distance_parent");
							flag8 = false;
						}
					}
				}
				if (ConVar.AntiHack.melee_protection >= 1)
				{
					if (ConVar.AntiHack.melee_protection >= 4 && player.HasParent() && ConVar.AntiHack.parenthistory && player.tickHistory.ParentCount > 0)
					{
						float magnitude3 = player.GetParentVelocity().magnitude;
						float num13 = player.AntiHackPadding() + num4 * magnitude3 + num * maxDistance;
						float num14 = player.tickHistory.DistanceParented(player, obj.HitPositionWorld);
						if (num14 > num13)
						{
							string shortPrefabName10 = base.ShortPrefabName;
							string text = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Initiator too far away (parent tick history) ({shortPrefabName10} on {text} with {num14}m > {num13}m in {num4}s)");
							player.stats.combat.LogInvalid(obj, "melee_initiator_tick_parent");
							flag8 = false;
						}
					}
					else if (ConVar.AntiHack.melee_protection >= 4)
					{
						float magnitude4 = player.GetParentVelocity().magnitude;
						float num15 = player.AntiHackPadding() + num4 * magnitude4 + num * maxDistance;
						float num16 = player.tickHistory.Distance(player, obj.HitPositionWorld);
						if (num16 > num15)
						{
							string shortPrefabName11 = base.ShortPrefabName;
							string text2 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Initiator too far away (tick history) ({shortPrefabName11} on {text2} with {num16}m > {num15}m in {num4}s)");
							player.stats.combat.LogInvalid(obj, "melee_initiator_tick");
							flag8 = false;
						}
					}
					else
					{
						float num17 = player.AntiHackVelocity() + player.GetParentVelocity().magnitude;
						float num18 = player.AntiHackPadding() + num4 * num17 + num * maxDistance;
						float num19 = player.Distance(obj.HitPositionWorld);
						if (num19 > num18)
						{
							string shortPrefabName12 = base.ShortPrefabName;
							string text3 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Initiator too far away ({shortPrefabName12} on {text3} with {num19}m > {num18}m in {num4}s)");
							player.stats.combat.LogInvalid(obj, "melee_initiator");
							flag8 = false;
						}
					}
				}
				if (ConVar.AntiHack.melee_protection >= 3)
				{
					if (flag6)
					{
						Vector3 center = player.eyes.center;
						Vector3 position = player.eyes.position;
						Vector3 pointStart = obj.PointStart;
						Vector3 hitPositionWorld = obj.HitPositionWorld;
						hitPositionWorld -= (hitPositionWorld - pointStart).normalized * 0.001f;
						Vector3 vector = obj.PositionOnRay(hitPositionWorld);
						bool flag9 = MeleeLineOfSightEntity(center, position, pointStart, vector, hitPositionWorld, num5);
						string text4 = hitEntity.Categorize();
						string value = string.Empty;
						switch (text4)
						{
						case "player":
							value = (flag9 ? "hit_player_direct_los" : "hit_player_indirect_los");
							break;
						case "building":
							value = (flag9 ? "hit_building_direct_los" : "hit_building_indirect_los");
							break;
						case "entity":
							value = (flag9 ? "hit_entity_direct_los" : "hit_entity_indirect_los");
							break;
						}
						if (!string.IsNullOrEmpty(value))
						{
							player.stats.Add(value, 1, Stats.Server);
						}
						if (!flag9)
						{
							string shortPrefabName13 = base.ShortPrefabName;
							string shortPrefabName14 = hitEntity.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Line of sight entity ({shortPrefabName13} on {shortPrefabName14}) {center} {position} {pointStart} {vector} {hitPositionWorld}");
							player.stats.combat.LogInvalid(obj, "melee_los_entity");
							flag8 = false;
						}
					}
					if (flag6 && !flag && ConVar.AntiHack.melee_los_entity_realpos)
					{
						Vector3 position2 = player.eyes.position;
						float melee_losforgiveness = ConVar.AntiHack.melee_losforgiveness;
						Vector3 hitPositionWorld2 = obj.HitPositionWorld;
						Vector3 vector2 = hitEntity.ClosestPoint(hitPositionWorld2);
						float num20 = Vector3.Distance(vector2, hitPositionWorld2);
						if (!GamePhysics.LineOfSight(position2, vector2, num5, 0f, melee_losforgiveness, hitEntity) || !GamePhysics.LineOfSight(vector2, position2, num5, melee_losforgiveness, 0f, hitEntity) || num20 > ConVar.AntiHack.melee_los_entity_realpos_distance)
						{
							string shortPrefabName15 = base.ShortPrefabName;
							string shortPrefabName16 = hitEntity.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Line of sight entity real position ({shortPrefabName15} on {shortPrefabName16}) {position2} {vector2} {num20}");
							player.stats.combat.LogInvalid(obj, "melee_los_entity_realpos");
							flag8 = false;
						}
					}
					if (flag8 && flag && !flag7)
					{
						Vector3 hitPositionWorld3 = obj.HitPositionWorld;
						Vector3 position3 = basePlayer.eyes.position;
						Vector3 vector3 = basePlayer.CenterPoint();
						float melee_losforgiveness2 = ConVar.AntiHack.melee_losforgiveness;
						bool flag10 = GamePhysics.LineOfSight(hitPositionWorld3, position3, num5, 0f, melee_losforgiveness2) && GamePhysics.LineOfSight(position3, hitPositionWorld3, num5, melee_losforgiveness2, 0f);
						if (!flag10)
						{
							flag10 = GamePhysics.LineOfSight(hitPositionWorld3, vector3, num5, 0f, melee_losforgiveness2) && GamePhysics.LineOfSight(vector3, hitPositionWorld3, num5, melee_losforgiveness2, 0f);
						}
						if (!flag10)
						{
							string shortPrefabName17 = base.ShortPrefabName;
							string shortPrefabName18 = basePlayer.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, $"Line of sight player ({shortPrefabName17} on {shortPrefabName18}) {hitPositionWorld3} {position3} or {hitPositionWorld3} {vector3}");
							player.stats.combat.LogInvalid(obj, "melee_los_player");
							flag8 = false;
						}
					}
				}
				if (ConVar.AntiHack.melee_protection >= 5 && flag8 && flag6 && !flag && hitEntity.AntiHackVelocity() == 0f && !hitEntity.IsOnMovingObject() && !(hitEntity is ResourceEntity) && !(hitEntity is CollectibleEntity) && !(hitEntity is BaseLock) && !(hitEntity is BaseCombatEntity { ValidateMeleeColliderAntihack: false }))
				{
					Vector3 hitPositionWorld4 = obj.HitPositionWorld;
					float melee_entity_bounds_radius = ConVar.AntiHack.melee_entity_bounds_radius;
					if (!GamePhysics.OverlapSphereHasEntity(hitPositionWorld4, melee_entity_bounds_radius, hitEntity, 1270440705))
					{
						string shortPrefabName19 = base.ShortPrefabName;
						string shortPrefabName20 = hitEntity.ShortPrefabName;
						AntiHack.Log(player, AntiHackType.MeleeHack, $"Entity hit too far from collider ({shortPrefabName19} on {shortPrefabName20}) {hitPositionWorld4} with {melee_entity_bounds_radius} radius");
						player.stats.combat.LogInvalid(obj, "melee_collider_entity");
						flag8 = false;
					}
				}
				if (!flag8)
				{
					AntiHack.AddViolation(player, AntiHackType.MeleeHack, ConVar.AntiHack.melee_penalty);
					return;
				}
			}
			player.metabolism.UseHeart(heartStress * 0.2f);
			using (TimeWarning.New("DoAttackShared", 50))
			{
				DoAttackShared(obj);
			}
			Facepunch.Pool.Free(ref obj);
		}
	}

	public override bool CanBeUsedInWater()
	{
		return true;
	}

	public virtual string GetStrikeEffectPath(string materialName)
	{
		for (int i = 0; i < materialStrikeFX.Count; i++)
		{
			if (materialStrikeFX[i].materialName == materialName && materialStrikeFX[i].fx.isValid)
			{
				return materialStrikeFX[i].fx.resourcePath;
			}
		}
		return strikeFX.resourcePath;
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		if (base.isClient || HasAttackCooldown())
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!(ownerPlayer == null))
		{
			StartAttackCooldown(repeatDelay * 2f);
			ownerPlayer.SignalBroadcast(Signal.Attack, string.Empty);
			if (swingEffect.isValid)
			{
				Effect.server.Run(swingEffect.resourcePath, base.transform.position, Vector3.forward, ownerPlayer.net.connection);
			}
			if (IsInvoking(ServerUse_Strike))
			{
				CancelInvoke(ServerUse_Strike);
			}
			Invoke(ServerUse_Strike, aiStrikeDelay);
		}
	}

	public virtual void ServerUse_OnHit(HitInfo info)
	{
	}

	public void ServerUse_Strike()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return;
		}
		Vector3 position = ownerPlayer.eyes.position;
		Vector3 vector = ownerPlayer.eyes.BodyForward();
		for (int i = 0; i < 2; i++)
		{
			List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
			GamePhysics.TraceAll(new Ray(position - vector * ((i == 0) ? 0f : 0.2f), vector), (i == 0) ? 0f : attackRadius, obj, effectiveRange + 0.2f, 1220225809);
			bool flag = false;
			for (int j = 0; j < obj.Count; j++)
			{
				RaycastHit hit = obj[j];
				BaseEntity entity = RaycastHitEx.GetEntity(hit);
				if (entity == null || (entity != null && (entity == ownerPlayer || entity.EqualNetID(ownerPlayer))) || (entity != null && entity.isClient) || entity.Categorize() == ownerPlayer.Categorize())
				{
					continue;
				}
				float num = 0f;
				foreach (DamageTypeEntry damageType in damageTypes)
				{
					num += damageType.amount;
				}
				entity.OnAttacked(new HitInfo(ownerPlayer, entity, DamageType.Slash, num * npcDamageScale));
				HitInfo obj2 = Facepunch.Pool.Get<HitInfo>();
				obj2.HitEntity = entity;
				obj2.HitPositionWorld = hit.point;
				obj2.HitNormalWorld = -vector;
				if (entity is BaseNpc || entity is BasePlayer)
				{
					obj2.HitMaterial = StringPool.Get("Flesh");
				}
				else
				{
					obj2.HitMaterial = StringPool.Get((RaycastHitEx.GetCollider(hit).sharedMaterial != null) ? AssetNameCache.GetName(RaycastHitEx.GetCollider(hit).sharedMaterial) : "generic");
				}
				ServerUse_OnHit(obj2);
				Effect.server.ImpactEffect(obj2);
				Facepunch.Pool.Free(ref obj2);
				flag = true;
				if (!(entity != null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
			if (flag)
			{
				break;
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.baseMelee = Facepunch.Pool.Get<ProtoBuf.BaseMelee>();
			info.msg.baseMelee.canThrowAsProjectile = canThrowAsProjectile;
			info.msg.baseMelee.onlyThrowAsProjectile = onlyThrowAsProjectile;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}
}
