#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseProjectile : AttackEntity
{
	[Serializable]
	public class Magazine
	{
		[Serializable]
		public struct Definition
		{
			[Tooltip("Set to 0 to not use inbuilt mag")]
			public int builtInSize;

			[InspectorFlags]
			[Tooltip("If using inbuilt mag, will accept these types of ammo")]
			public AmmoTypes ammoTypes;
		}

		public Definition definition;

		public int capacity;

		public int contents;

		[ItemSelector]
		public ItemDefinition ammoType;

		public bool allowPlayerReloading = true;

		public bool allowAmmoSwitching = true;

		public void ServerInit()
		{
			if (definition.builtInSize > 0)
			{
				capacity = definition.builtInSize;
			}
		}

		public ProtoBuf.Magazine Save()
		{
			ProtoBuf.Magazine magazine = Facepunch.Pool.Get<ProtoBuf.Magazine>();
			if (ammoType == null)
			{
				magazine.capacity = capacity;
				magazine.contents = 0;
				magazine.ammoType = 0;
			}
			else
			{
				magazine.capacity = capacity;
				magazine.contents = contents;
				magazine.ammoType = ammoType.itemid;
			}
			return magazine;
		}

		public void Load(ProtoBuf.Magazine mag)
		{
			contents = mag.contents;
			capacity = mag.capacity;
			ammoType = ItemManager.FindItemDefinition(mag.ammoType);
		}

		public bool CanReload(IAmmoContainer ammoSource)
		{
			if (contents >= capacity)
			{
				return false;
			}
			return ammoSource.HasAmmo(definition.ammoTypes);
		}
	}

	public enum WeaponCrosshairSettings
	{
		Standard,
		StandardWithAiming,
		StandardWithReloading,
		StandardWithAimingAndReloading,
		Always
	}

	public static class BaseProjectileFlags
	{
		public const Flags BurstToggle = Flags.Reserved6;
	}

	[Header("NPC Info")]
	public float NoiseRadius = 100f;

	[Header("Projectile")]
	[Tooltip("Scales the damage of the projectile across all ranges.")]
	public float damageScale = 1f;

	[Tooltip("Scales the damage falloff window of the projectile.")]
	public float distanceScale = 1f;

	[Tooltip("Overrides the projectile's far damage multiplier. Negative values use the projectile default.")]
	public float farDamageScale = -1f;

	[Tooltip("Scales only the projectile's far falloff distance. Negative values use the projectile default.")]
	public float farDistanceScale = -1f;

	public float projectileVelocityScale = 1f;

	public bool automatic;

	public bool usableByTurret = true;

	[Tooltip("Final damage is scaled by this amount before being applied to a target when this weapon is mounted to a turret")]
	public float turretDamageScale = 0.35f;

	public bool largeTurretWeapon;

	public float turretReloadDurationOverride = -1f;

	[Tooltip("How far away this attack effect can be heard")]
	[Header("Effects")]
	public float maxAttackEffectDistance = 400f;

	public GameObjectRef attackFX;

	public GameObjectRef silencedAttack;

	public GameObjectRef muzzleBrakeAttack;

	public SoundDefinition fireModeSound;

	public Transform MuzzlePoint;

	[Header("Reloading")]
	public float reloadTime = 1f;

	public bool canUnloadAmmo = true;

	public Magazine primaryMagazine;

	public bool fractionalReload;

	public float reloadStartDuration;

	public float reloadFractionDuration;

	public float reloadEndDuration;

	public float alternateDryFireRate;

	public bool sendReloadSignalFromServer;

	[Header("Recoil")]
	public float aimSway = 3f;

	public float aimSwaySpeed = 1f;

	public RecoilProperties recoil;

	[Header("Aim Cone")]
	public AnimationCurve aimconeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public float aimCone;

	public float hipAimCone = 1.8f;

	public float aimconePenaltyPerShot;

	public float aimConePenaltyMax;

	public float aimconePenaltyRecoverTime = 0.1f;

	public float aimconePenaltyRecoverDelay = 0.1f;

	public float stancePenaltyScale = 1f;

	[Header("Iconsights")]
	public bool hasADS = true;

	public bool noAimingWhileCycling;

	public bool manualCycle;

	public WeaponCrosshairSettings CrosshairSettings;

	[NonSerialized]
	protected bool needsCycle;

	[NonSerialized]
	protected bool isCycling;

	[NonSerialized]
	public bool aiming;

	[Header("Burst Information")]
	public bool isBurstWeapon;

	public bool canChangeFireModes = true;

	public bool defaultOn = true;

	public float internalBurstRecoilScale = 0.8f;

	public float internalBurstFireRateScale = 0.8f;

	public float internalBurstAimConeScale = 0.8f;

	public float resetDuration = 0.3f;

	public int numShotsFired;

	public const float maxDistance = 300f;

	[NonSerialized]
	private EncryptedValue<float> nextReloadTime = float.NegativeInfinity;

	[NonSerialized]
	private EncryptedValue<float> startReloadTime = float.NegativeInfinity;

	private float lastReloadTime = -10f;

	private bool modsChangedInitialized;

	private float stancePenalty;

	private float aimconePenalty;

	private uint cachedModHash;

	private float sightAimConeScale = 1f;

	private float sightAimConeOffset;

	private float hipAimConeScale = 1f;

	private float hipAimConeOffset;

	protected bool reloadStarted;

	protected bool reloadFinished;

	private int fractionalInsertCounter;

	private static readonly Effect reusableInstance = new Effect();

	public RecoilProperties recoilProperties
	{
		get
		{
			if (!(recoil == null))
			{
				return recoil.GetRecoil();
			}
			return null;
		}
	}

	public bool isSemiAuto => !automatic;

	public override Transform MuzzleTransform => MuzzlePoint;

	public override bool IsUsableByTurret => usableByTurret;

	protected virtual bool CanRefundAmmo => true;

	protected virtual ItemDefinition PrimaryMagazineAmmo => primaryMagazine.ammoType;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseProjectile.OnRpcMessage"))
		{
			if (rpc == 3168282921u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - CLProject");
				}
				using (TimeWarning.New("CLProject"))
				{
					using (msg.read.UseRepeatedElementLimit(64))
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
								RPCMessage msg2 = rPCMessage;
								CLProject(msg2);
							}
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							player.Kick("RPC Error in CLProject");
						}
					}
				}
				return true;
			}
			if (rpc == 1720368164 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Reload");
				}
				using (TimeWarning.New("Reload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1720368164u, "Reload", this, player))
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
							Reload(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Reload");
					}
				}
				return true;
			}
			if (rpc == 240404208 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerFractionalReloadInsert");
				}
				using (TimeWarning.New("ServerFractionalReloadInsert"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(240404208u, "ServerFractionalReloadInsert", this, player))
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
							ServerFractionalReloadInsert(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in ServerFractionalReloadInsert");
					}
				}
				return true;
			}
			if (rpc == 555589155 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - StartReload");
				}
				using (TimeWarning.New("StartReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(555589155u, "StartReload", this, player))
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
							RPCMessage msg5 = rPCMessage;
							StartReload(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in StartReload");
					}
				}
				return true;
			}
			if (rpc == 1918419884 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SwitchAmmoTo");
				}
				using (TimeWarning.New("SwitchAmmoTo"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1918419884u, "SwitchAmmoTo", this, player))
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
							RPCMessage msg6 = rPCMessage;
							SwitchAmmoTo(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in SwitchAmmoTo");
					}
				}
				return true;
			}
			if (rpc == 3327286961u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ToggleFireMode");
				}
				using (TimeWarning.New("ToggleFireMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3327286961u, "ToggleFireMode", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3327286961u, "ToggleFireMode", this, player))
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
							RPCMessage msg7 = rPCMessage;
							ToggleFireMode(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in ToggleFireMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected bool TryReload(IAmmoContainer ammoSource, int desiredAmount, bool canRefundAmmo = true)
	{
		using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
		ammoSource.FindItemsByItemID(pooledList, primaryMagazine.ammoType.itemid);
		if (pooledList.Count == 0 && !primaryMagazine.allowAmmoSwitching)
		{
			return false;
		}
		if (pooledList.Count == 0)
		{
			Item item = ammoSource.FindAmmo(primaryMagazine.definition.ammoTypes);
			if (item == null)
			{
				return false;
			}
			ammoSource.FindItemsByItemID(pooledList, item.info.itemid);
			if (pooledList.Count == 0)
			{
				return false;
			}
			if (primaryMagazine.contents > 0)
			{
				if (canRefundAmmo)
				{
					ammoSource.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				}
				SetAmmoCount(0);
			}
			primaryMagazine.ammoType = pooledList[0].info;
		}
		int num = desiredAmount;
		if (num == -1)
		{
			num = primaryMagazine.capacity - primaryMagazine.contents;
		}
		for (int i = 0; i < pooledList.Count; i++)
		{
			Item item2 = pooledList[i];
			_ = item2.amount;
			int num2 = Mathf.Min(num, item2.amount);
			item2.UseItem(num2);
			ModifyAmmoCount(num2);
			num -= num2;
			if (num <= 0)
			{
				break;
			}
		}
		return true;
	}

	public void SwitchAmmoTypesIfNeeded(IAmmoContainer ammoSource)
	{
		Item item = ammoSource.FindItemByItemID(primaryMagazine.ammoType.itemid);
		if (item != null)
		{
			return;
		}
		Item item2 = ammoSource.FindAmmo(primaryMagazine.definition.ammoTypes);
		if (item2 == null)
		{
			return;
		}
		item = ammoSource.FindItemByItemID(item2.info.itemid);
		if (item != null)
		{
			if (primaryMagazine.contents > 0)
			{
				ammoSource.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				SetAmmoCount(0);
			}
			primaryMagazine.ammoType = item.info;
		}
	}

	public static void StripAmmoToType(ref List<Item> ammos, ItemDefinition onlyAllowed)
	{
		if (!(onlyAllowed != null))
		{
			return;
		}
		for (int num = ammos.Count - 1; num >= 0; num--)
		{
			if (ammos[num].info != onlyAllowed)
			{
				ammos.RemoveAt(num);
			}
		}
	}

	public void SetAmmoCount(int newCount)
	{
		primaryMagazine.contents = newCount;
		GetItem()?.MarkDirty();
	}

	public void ModifyAmmoCount(int amount)
	{
		SetAmmoCount(primaryMagazine.contents + amount);
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player, Vector3 direction)
	{
		return player.GetInheritedProjectileVelocity(direction);
	}

	public virtual float GetDamageScale(bool getMax = false)
	{
		return damageScale;
	}

	public virtual float GetDistanceScale(bool getMax = false)
	{
		return distanceScale;
	}

	public virtual float GetFarDamageScale(bool getMax = false)
	{
		return farDamageScale;
	}

	public virtual float GetFarDistanceScale(bool getMax = false)
	{
		return farDistanceScale;
	}

	public virtual float GetProjectileVelocityScale(bool getMax = false)
	{
		return projectileVelocityScale;
	}

	public virtual float GetOverrideProjectileThickness(Projectile projectile)
	{
		if (projectile == null)
		{
			return 0f;
		}
		return projectile.thickness;
	}

	protected void StartReloadCooldown(float cooldown)
	{
		nextReloadTime = CalculateCooldownTime(nextReloadTime, cooldown, catchup: false, unscaledTime: true);
		startReloadTime = (float)nextReloadTime - cooldown;
	}

	protected void ResetReloadCooldown()
	{
		nextReloadTime = float.NegativeInfinity;
	}

	protected bool HasReloadCooldown()
	{
		return UnityEngine.Time.unscaledTime < (float)nextReloadTime;
	}

	protected float GetReloadCooldown()
	{
		return Mathf.Max((float)nextReloadTime - UnityEngine.Time.unscaledTime, 0f);
	}

	protected float GetReloadIdle()
	{
		return Mathf.Max(UnityEngine.Time.unscaledTime - (float)nextReloadTime, 0f);
	}

	private void OnDrawGizmos()
	{
		if (base.isClient && MuzzlePoint != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(MuzzlePoint.position, MuzzlePoint.position + MuzzlePoint.forward * 10f);
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if ((bool)ownerPlayer)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(MuzzlePoint.position, MuzzlePoint.position + ownerPlayer.eyes.rotation * Vector3.forward * 10f);
			}
		}
	}

	public virtual RecoilProperties GetRecoil()
	{
		return recoilProperties;
	}

	public override float AmmoFraction()
	{
		return (float)primaryMagazine.contents / (float)primaryMagazine.capacity;
	}

	public virtual void DidAttackServerside()
	{
	}

	public override bool ServerIsReloading()
	{
		return UnityEngine.Time.time < lastReloadTime + reloadTime;
	}

	public override bool CanReload()
	{
		return primaryMagazine.contents < primaryMagazine.capacity;
	}

	public override void TopUpAmmo()
	{
		SetAmmoCount(primaryMagazine.capacity);
	}

	public void SkipReload()
	{
		lastReloadTime = UnityEngine.Time.time - reloadTime;
		ResetReloadCooldown();
		StartAttackCooldown(0f);
		StartAttackCooldownRaw(0f);
	}

	public override void ServerReload()
	{
		if (ServerIsReloading())
		{
			return;
		}
		lastReloadTime = UnityEngine.Time.time;
		StartAttackCooldown(reloadTime);
		if (sendReloadSignalFromServer)
		{
			SignalBroadcast(Signal.Reload);
		}
		else
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer != null)
			{
				ownerPlayer.SignalBroadcast(Signal.Reload);
			}
		}
		SetAmmoCount(primaryMagazine.capacity);
	}

	public override bool ServerTryReload(IAmmoContainer ammoSource)
	{
		if (ServerIsReloading())
		{
			return false;
		}
		if (TryReloadMagazine(ammoSource, -1, shouldUpdateShieldState: false))
		{
			if (sendReloadSignalFromServer)
			{
				SignalBroadcast(Signal.Reload);
			}
			else
			{
				BasePlayer ownerPlayer = GetOwnerPlayer();
				if (ownerPlayer != null)
				{
					ownerPlayer.SignalBroadcast(Signal.Reload);
				}
			}
			lastReloadTime = UnityEngine.Time.time;
			StartAttackCooldown(reloadTime);
			return true;
		}
		return false;
	}

	public override Vector3 ModifyAIAim(Vector3 eulerInput, float swayModifier = 1f)
	{
		float num = UnityEngine.Time.time * (aimSwaySpeed * 1f + aiAimSwayOffset);
		float num2 = Mathf.Sin(UnityEngine.Time.time * 2f);
		float num3 = ((num2 < 0f) ? (1f - Mathf.Clamp(Mathf.Abs(num2) / 1f, 0f, 1f)) : 1f);
		float num4 = (false ? 0.6f : 1f);
		float num5 = (aimSway * 1f + aiAimSwayOffset) * num4 * num3 * swayModifier;
		eulerInput.y += (Mathf.PerlinNoise(num, num) - 0.5f) * num5 * UnityEngine.Time.deltaTime;
		eulerInput.x += (Mathf.PerlinNoise(num + 0.1f, num + 0.2f) - 0.5f) * num5 * UnityEngine.Time.deltaTime;
		return eulerInput;
	}

	public float GetAIAimcone()
	{
		NPCPlayer nPCPlayer = GetOwnerPlayer() as NPCPlayer;
		if ((bool)nPCPlayer)
		{
			return nPCPlayer.GetAimConeScale() * aiAimCone;
		}
		return aiAimCone;
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		if (base.isClient || HasAttackCooldown())
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		bool flag = ownerPlayer != null;
		if (primaryMagazine.contents <= 0)
		{
			SignalBroadcast(Signal.DryFire);
			StartAttackCooldownRaw(1f);
			return;
		}
		ModifyAmmoCount(-1);
		if (primaryMagazine.contents < 0)
		{
			SetAmmoCount(0);
		}
		bool flag2 = flag && ownerPlayer.IsNpc;
		BaseEntity owner;
		BaseNPC2 castedUnityObject;
		bool flag3 = TryGetOwner(out owner) && BaseNetworkableEx.Is<BaseNPC2>(owner, out castedUnityObject);
		bool flag4 = flag2 || flag3;
		if (flag2 && (ownerPlayer.isMounted || ownerPlayer.GetParentEntity() != null))
		{
			NPCPlayer nPCPlayer = ownerPlayer as NPCPlayer;
			if (nPCPlayer != null)
			{
				nPCPlayer.SetAimDirection(nPCPlayer.GetAimDirection());
			}
		}
		StartAttackCooldownRaw(repeatDelay);
		Vector3 vector = (flag ? ownerPlayer.eyes.position : MuzzlePoint.transform.position);
		Vector3 vector2 = MuzzlePoint.transform.forward;
		if (parameters.originOverride.HasValue)
		{
			vector = parameters.originOverride.Value.GetPosition();
			vector2 = parameters.originOverride.Value.MultiplyVector(Vector3.forward);
		}
		ItemModProjectile ammoInfo2 = primaryMagazine.ammoType.GetComponent<ItemModProjectile>();
		SignalBroadcast(Signal.Attack, string.Empty, null, GetAttackEffect(), maxAttackEffectDistance);
		Projectile component = ammoInfo2.projectileObject.Get().GetComponent<Projectile>();
		float num = ammoInfo2.projectileVelocity * parameters.speedModifier;
		bool flag5 = GetParentEntity() is BasePlayer;
		BaseEntity baseEntity = null;
		if (flag && useOwnerForward)
		{
			vector2 = ownerPlayer.eyes.BodyForward();
		}
		for (int i = 0; i < ammoInfo2.numProjectiles; i++)
		{
			Vector3 vector3 = (flag2 ? AimConeUtil.GetModifiedAimConeDirection(ammoInfo2.projectileSpread + GetAimCone() + GetAIAimcone(), vector2) : ((!flag3) ? AimConeUtil.GetModifiedAimConeDirection(ammoInfo2.projectileSpread + GetAimCone(), vector2) : vector2));
			float radius = (parameters.useBulletThickness ? GetOverrideProjectileThickness(component) : 0f);
			List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
			Ray ray = new Ray(vector, vector3);
			GamePhysics.TraceAll(ray, radius, obj, 300f, 1220225793, QueryTriggerInteraction.Ignore, ownerPlayer);
			float distanceOverride = 0f;
			for (int j = 0; j < obj.Count && parameters.damageModifier != 0f; j++)
			{
				RaycastHit hit = obj[j];
				BaseEntity entity = RaycastHitEx.GetEntity(hit);
				if (flag5)
				{
					if (entity != null && (entity == this || entity.EqualNetID(this)))
					{
						continue;
					}
				}
				else if (entity != null && this.HasEntityInParents(entity))
				{
					continue;
				}
				if (entity is BasePlayer basePlayer && basePlayer.TryGetActiveShield(out var foundShield) && foundShield.RaycastAgainstColliders(ray, 300f))
				{
					Vector3 vector4 = base.transform.InverseTransformPoint(foundShield.transform.position);
					Vector3 vector5 = base.transform.InverseTransformPoint(basePlayer.CenterPoint());
					if (vector4.sqrMagnitude < vector5.sqrMagnitude)
					{
						continue;
					}
				}
				if (entity != null && entity.isClient)
				{
					continue;
				}
				ColliderInfo component2 = hit.collider.GetComponent<ColliderInfo>();
				if (component2 != null && !component2.HasFlag(ColliderInfo.Flags.Shootable))
				{
					continue;
				}
				BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
				if ((entity != null && entity.IsNpc && flag4 && baseCombatEntity != null && baseCombatEntity.GetFaction() != BaseCombatEntity.Faction.Horror && !(entity is BasePet)) || !(entity != null) || (!(baseEntity == null) && !(entity == baseEntity) && !entity.EqualNetID(baseEntity)) || !entity.IsVisible(vector, hit.point, 300f))
				{
					continue;
				}
				HitInfo info2 = Facepunch.Pool.Get<HitInfo>();
				AssignInitiator(info2);
				info2.Weapon = this;
				info2.WeaponPrefab = base.gameManager.FindPrefab(base.PrefabName).GetComponent<AttackEntity>();
				info2.IsPredicting = false;
				info2.DoHitEffects = component.doHitEffects;
				info2.DidHit = true;
				info2.ProjectileVelocity = vector3 * 300f;
				info2.PointStart = MuzzlePoint.position;
				info2.PointEnd = hit.point;
				info2.HitPositionWorld = hit.point;
				info2.HitNormalWorld = hit.normal;
				info2.HitEntity = entity;
				info2.UseProtection = true;
				info2.UseProtectionForNPCs = parameters.useProtectionForNPCs;
				distanceOverride = hit.distance;
				component.CalculateDamage(info2, GetProjectileModifier(), 1f);
				info2.damageTypes.ScaleAll(GetDamageScale() * parameters.damageModifier * (flag4 ? npcDamageScale : turretDamageScale));
				float num2 = ((num > 0f) ? (hit.distance / num) : 0f);
				if (num2 > 0.2f)
				{
					Invoke(delegate
					{
						ProcessHit(info2, ammoInfo2);
					}, num2);
				}
				else
				{
					ProcessHit(info2, ammoInfo2);
				}
				if (!(entity != null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
			Vector3 vector6 = ((flag && ownerPlayer.isMounted) ? (vector3 * 6f) : Vector3.zero);
			CreateProjectileEffectClientside(ammoInfo2.GetOverrideProjectile(this).resourcePath, vector + vector6, vector3 * num, UnityEngine.Random.Range(1, 100), null, IsSilenced(), forceClientsideEffects: true, null, distanceOverride);
		}
		static void ProcessHit(HitInfo info, ItemModProjectile ammoInfo)
		{
			if (!info.Weapon.IsValid() || !info.HitEntity.IsValid())
			{
				Facepunch.Pool.Free(ref info);
			}
			else
			{
				info.HitEntity.OnAttacked(info);
				ammoInfo.ServerProjectileHit(info);
				ammoInfo.ServerProjectileHitEntity(info);
				Shield shield = info.HitEntity as Shield;
				if (info.HitEntity is BasePlayer || info.HitEntity is BaseNpc || shield != null)
				{
					info.HitPositionLocal = info.HitEntity.transform.InverseTransformPoint(info.HitPositionWorld);
					info.HitNormalLocal = info.HitEntity.transform.InverseTransformDirection(info.HitNormalWorld);
					info.HitMaterial = StringPool.Get((shield != null) ? shield.GetHitMaterialString() : "Flesh");
					Effect.server.ImpactEffect(info);
				}
				Facepunch.Pool.Free(ref info);
			}
		}
	}

	private void AssignInitiator(HitInfo info)
	{
		info.Initiator = GetOwnerPlayer();
		if (info.Initiator == null)
		{
			info.Initiator = GetParentEntity();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		primaryMagazine.ServerInit();
		Invoke(DelayedModSetup, 0.1f);
	}

	public void DelayedModSetup()
	{
		if (!modsChangedInitialized)
		{
			Item item = GetCachedItem();
			if (item != null && item.contents != null)
			{
				ItemContainer contents = item.contents;
				contents.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(contents.onItemAddedRemoved, new Action<Item, bool>(ModsChanged));
				modsChangedInitialized = true;
			}
		}
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			Item item = GetCachedItem();
			if (item != null && item.contents != null)
			{
				ItemContainer contents = item.contents;
				contents.onItemAddedRemoved = (Action<Item, bool>)Delegate.Remove(contents.onItemAddedRemoved, new Action<Item, bool>(ModsChanged));
				modsChangedInitialized = false;
			}
		}
		base.DestroyShared();
	}

	public void ModsChanged(Item item, bool added)
	{
		Invoke(DelayedModsChanged, 0.1f);
	}

	public void ForceModsChanged()
	{
		Invoke(DelayedModSetup, 0f);
		Invoke(DelayedModsChanged, 0.2f);
	}

	public void DelayedModsChanged()
	{
		if (Interface.CallHook("OnWeaponModChange", this, GetOwnerPlayer()) != null)
		{
			return;
		}
		int num = Mathf.CeilToInt(ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectMagCap, ProjectileWeaponMod.SelectScalar) * (float)primaryMagazine.definition.builtInSize);
		if (num == primaryMagazine.capacity)
		{
			return;
		}
		if (primaryMagazine.contents > 0 && primaryMagazine.contents > num)
		{
			_ = primaryMagazine.ammoType;
			int contents = primaryMagazine.contents;
			BasePlayer ownerPlayer = GetOwnerPlayer();
			ItemContainer itemContainer = null;
			if (ownerPlayer != null)
			{
				itemContainer = ownerPlayer.inventory.containerMain;
			}
			else if (GetCachedItem() != null)
			{
				itemContainer = GetCachedItem().parent;
			}
			SetAmmoCount(0);
			if (itemContainer != null)
			{
				Item item = ItemManager.Create(primaryMagazine.ammoType, contents, 0uL, isServerSide: true, 0uL);
				if (!item.MoveToContainer(itemContainer))
				{
					Vector3 vPos = base.transform.position;
					if (itemContainer.entityOwner != null)
					{
						vPos = itemContainer.entityOwner.transform.position + Vector3.up * 0.25f;
					}
					item.Drop(vPos, Vector3.up * 5f);
				}
			}
		}
		primaryMagazine.capacity = num;
		SendNetworkUpdate();
	}

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		if (item != null && command == "unload_ammo" && !HasReloadCooldown())
		{
			UnloadAmmo(item, player);
		}
	}

	public void UnloadAmmo(Item item, BasePlayer player)
	{
		BaseProjectile component = item.GetHeldEntity().GetComponent<BaseProjectile>();
		if (!component.canUnloadAmmo || Interface.CallHook("OnAmmoUnload", component, item, player) != null || !component)
		{
			return;
		}
		int num = component.primaryMagazine.contents;
		if (num <= 0)
		{
			return;
		}
		component.SetAmmoCount(0);
		item.MarkDirty();
		SendNetworkUpdateImmediate();
		int stackable = component.primaryMagazine.ammoType.stackable;
		if (num > stackable)
		{
			int num2 = Mathf.FloorToInt(num / component.primaryMagazine.ammoType.stackable);
			num %= stackable;
			for (int i = 0; i < num2; i++)
			{
				Item item2 = ItemManager.Create(component.primaryMagazine.ammoType, stackable, 0uL, isServerSide: true, 0uL);
				player.GiveItem(item2);
			}
		}
		if (num > 0)
		{
			Item item3 = ItemManager.Create(component.primaryMagazine.ammoType, num, 0uL, isServerSide: true, 0uL);
			player.GiveItem(item3);
		}
	}

	public override void CollectedForCrafting(Item item, BasePlayer crafter)
	{
		if (!(crafter == null) && item != null)
		{
			UnloadAmmo(item, crafter);
		}
	}

	public override void ReturnedFromCancelledCraft(Item item, BasePlayer crafter)
	{
		if (!(crafter == null) && item != null)
		{
			BaseProjectile component = item.GetHeldEntity().GetComponent<BaseProjectile>();
			if ((bool)component)
			{
				component.SetAmmoCount(0);
			}
		}
	}

	public override void SetLightsOn(bool isOn)
	{
		base.SetLightsOn(isOn);
		UpdateAttachmentsState();
	}

	protected override bool BroadcastSignalFromClientFilter(Signal signal)
	{
		return signal == Signal.Attack;
	}

	public void UpdateAttachmentsState()
	{
		_ = flags;
		bool b = ShouldLightsBeOn();
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
			if (projectileWeaponMod != null && projectileWeaponMod.isLight)
			{
				using FlagsUpdateScope flagsUpdateScope = projectileWeaponMod.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.On, b);
			}
		}
	}

	private bool ShouldLightsBeOn()
	{
		if (LightsOn())
		{
			if (!IsDeployed())
			{
				return parentEntity.Get(base.isServer) is AutoTurret;
			}
			return true;
		}
		return false;
	}

	protected override void OnChildRemoved(BaseEntity child)
	{
		base.OnChildRemoved(child);
		if (child is ProjectileWeaponMod { isLight: not false })
		{
			using (FlagsUpdateScope flagsUpdateScope = child.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
			SetLightsOn(isOn: false);
		}
	}

	public bool CanAiAttack()
	{
		return true;
	}

	public virtual float GetAimCone()
	{
		uint num = 0u;
		foreach (BaseEntity child in children)
		{
			num += (uint)(int)child.net.ID.Value;
			num += (uint)child.flags;
		}
		uint num2 = CRC.Compute32(0u, num);
		if (num2 != cachedModHash)
		{
			sightAimConeScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectSightAimCone, ProjectileWeaponMod.SelectScalar);
			sightAimConeOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectSightAimCone, ProjectileWeaponMod.SelectOffset);
			hipAimConeScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectHipAimCone, ProjectileWeaponMod.SelectScalar);
			hipAimConeOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectHipAimCone, ProjectileWeaponMod.SelectOffset);
			cachedModHash = num2;
		}
		float num3 = aimCone;
		num3 *= (UsingInternalBurstMode() ? internalBurstAimConeScale : 1f);
		if (recoilProperties != null && recoilProperties.overrideAimconeWithCurve && primaryMagazine.capacity > 0)
		{
			num3 += recoilProperties.aimconeCurve.Evaluate((float)numShotsFired / (float)primaryMagazine.capacity % 1f) * recoilProperties.aimconeCurveScale;
			aimconePenalty = 0f;
		}
		if (aiming || base.isServer)
		{
			return (num3 + aimconePenalty + stancePenalty * stancePenaltyScale) * sightAimConeScale + sightAimConeOffset;
		}
		return (num3 + aimconePenalty + stancePenalty * stancePenaltyScale) * sightAimConeScale + sightAimConeOffset + hipAimCone * hipAimConeScale + hipAimConeOffset;
	}

	public float ScaleRepeatDelay(float delay)
	{
		float num = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectRepeatDelay, ProjectileWeaponMod.SelectScalar);
		float num2 = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectRepeatDelay, ProjectileWeaponMod.SelectOffset);
		float num3 = (UsingInternalBurstMode() ? internalBurstFireRateScale : 1f);
		return delay * num * num3 + num2;
	}

	public Projectile.Modifier GetProjectileModifier()
	{
		Projectile.Modifier result = default(Projectile.Modifier);
		result.damageOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectDamage, ProjectileWeaponMod.SelectOffset);
		result.damageScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectDamage, ProjectileWeaponMod.SelectScalar) * GetDamageScale();
		result.distanceOffset = ProjectileWeaponMod.Sum(this, ProjectileWeaponMod.SelectDistance, ProjectileWeaponMod.SelectOffset);
		result.distanceScale = ProjectileWeaponMod.Mult(this, ProjectileWeaponMod.SelectDistance, ProjectileWeaponMod.SelectScalar) * GetDistanceScale();
		result.farDamageScale = GetFarDamageScale();
		result.farDistanceScale = GetFarDistanceScale();
		return result;
	}

	public bool IsBurstModeOnly()
	{
		if (isBurstWeapon)
		{
			return !canChangeFireModes;
		}
		return false;
	}

	public bool UsingBurstMode()
	{
		if (IsBurstDisabled())
		{
			return false;
		}
		return IsBurstEligable();
	}

	public bool UsingInternalBurstMode()
	{
		if (IsBurstDisabled())
		{
			return false;
		}
		return isBurstWeapon;
	}

	public bool IsBurstEligable()
	{
		if (isBurstWeapon)
		{
			return true;
		}
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if (projectileWeaponMod != null && projectileWeaponMod.burstCount > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float TimeBetweenBursts()
	{
		return repeatDelay * 2f;
	}

	public int GetBurstModeCount()
	{
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if (projectileWeaponMod != null && projectileWeaponMod.burstCount > 0)
				{
					return projectileWeaponMod.burstCount;
				}
			}
		}
		return 3;
	}

	public virtual bool CanAttack()
	{
		if (ProjectileWeaponMod.HasBrokenWeaponMod(this))
		{
			return false;
		}
		return true;
	}

	public virtual float GetTurretReloadDuration()
	{
		if (turretReloadDurationOverride == -1f)
		{
			return GetReloadDuration() * 0.5f;
		}
		return turretReloadDurationOverride;
	}

	public virtual float GetReloadDuration()
	{
		if (fractionalReload)
		{
			int num = Mathf.Min(primaryMagazine.capacity - primaryMagazine.contents, GetAvailableAmmo());
			return reloadStartDuration + reloadEndDuration + reloadFractionDuration * (float)num;
		}
		return reloadTime;
	}

	public int GetAvailableAmmo()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return primaryMagazine.contents;
		}
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		ownerPlayer.inventory.FindAmmo(obj, primaryMagazine.definition.ammoTypes);
		int num = 0;
		if (obj.Count != 0)
		{
			for (int i = 0; i < obj.Count; i++)
			{
				Item item = obj[i];
				if (item.info == primaryMagazine.ammoType)
				{
					num += item.amount;
				}
			}
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
		return num;
	}

	public bool IsBurstDisabled()
	{
		return HasFlag(Flags.Reserved6) == defaultOn;
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void ToggleFireMode(RPCMessage msg)
	{
		if (canChangeFireModes && IsBurstEligable())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.Reserved6, !HasFlag(Flags.Reserved6));
			}
			Facepunch.Rust.Analytics.Azure.OnBurstModeToggled(msg.player, this, HasFlag(Flags.Reserved6));
		}
	}

	public virtual bool TryReloadMagazine(IAmmoContainer ammoSource, int desiredAmount = -1, bool shouldUpdateShieldState = true)
	{
		object obj = Interface.CallHook("OnMagazineReload", this, ammoSource, GetOwnerPlayer());
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!TryReload(ammoSource, desiredAmount))
		{
			return false;
		}
		SendNetworkUpdateImmediate();
		ItemManager.DoRemoves();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer != null)
		{
			ownerPlayer.inventory.ServerUpdate(0f);
		}
		if (shouldUpdateShieldState)
		{
			if (!fractionalReload)
			{
				UpdateShieldState(bHeld: true);
			}
			else if (primaryMagazine.contents == primaryMagazine.capacity || !ammoSource.HasAmmo(primaryMagazine.definition.ammoTypes))
			{
				UpdateShieldState(bHeld: true);
			}
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void SwitchAmmoTo(RPCMessage msg)
	{
		if (!TryGetOwnerPlayer(out var ownerPlayer))
		{
			return;
		}
		int num = msg.read.Int32();
		if (num == primaryMagazine.ammoType.itemid)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(num);
		if (itemDefinition == null)
		{
			return;
		}
		ItemModProjectile component = itemDefinition.GetComponent<ItemModProjectile>();
		if ((bool)component && component.IsAmmo(primaryMagazine.definition.ammoTypes) && Interface.CallHook("OnAmmoSwitch", this, ownerPlayer, itemDefinition) == null)
		{
			if (primaryMagazine.contents > 0)
			{
				ownerPlayer.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				SetAmmoCount(0);
			}
			primaryMagazine.ammoType = itemDefinition;
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			ownerPlayer.inventory.ServerUpdate(0f);
		}
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		reloadStarted = false;
		reloadFinished = false;
		fractionalInsertCounter = 0;
		UpdateAttachmentsState();
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void StartReload(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			reloadStarted = false;
			reloadFinished = false;
		}
		else if (Interface.CallHook("OnWeaponReload", this, player) == null)
		{
			reloadFinished = false;
			reloadStarted = true;
			fractionalInsertCounter = 0;
			if (CanRefundAmmo)
			{
				SwitchAmmoTypesIfNeeded(player.inventory);
			}
			OnReloadStarted();
			StartReloadCooldown(GetReloadDuration());
		}
	}

	protected virtual void OnReloadStarted()
	{
		UpdateShieldState(bHeld: false);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void ServerFractionalReloadInsert(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (!fractionalReload)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload not allowed (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_type");
			return;
		}
		if (!reloadStarted)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload request skipped (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_skip");
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (GetReloadIdle() > 3f)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, $"T+{GetReloadIdle()}s ({base.ShortPrefabName})");
			player.stats.combat.LogInvalid(player, this, "reload_time");
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (UnityEngine.Time.unscaledTime < (float)startReloadTime + reloadStartDuration)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload too early (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_fraction_too_early");
			reloadStarted = false;
			reloadFinished = false;
		}
		if (UnityEngine.Time.unscaledTime < (float)startReloadTime + reloadStartDuration + (float)fractionalInsertCounter * reloadFractionDuration)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Fractional reload rate too high (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_fraction_rate");
			reloadStarted = false;
			reloadFinished = false;
		}
		else
		{
			fractionalInsertCounter++;
			if (primaryMagazine.contents < primaryMagazine.capacity)
			{
				TryReloadMagazine(player.inventory, 1);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void Reload(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (!reloadStarted)
		{
			AntiHack.Log(player, AntiHackType.ReloadHack, "Request skipped (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_skip");
			reloadStarted = false;
			reloadFinished = false;
			return;
		}
		if (!fractionalReload)
		{
			if (GetReloadCooldown() > 1f)
			{
				AntiHack.Log(player, AntiHackType.ReloadHack, $"T-{GetReloadCooldown()}s ({base.ShortPrefabName})");
				player.stats.combat.LogInvalid(player, this, "reload_time");
				reloadStarted = false;
				reloadFinished = false;
				return;
			}
			if (GetReloadIdle() > 1.5f)
			{
				AntiHack.Log(player, AntiHackType.ReloadHack, $"T+{GetReloadIdle()}s ({base.ShortPrefabName})");
				player.stats.combat.LogInvalid(player, this, "reload_time");
				reloadStarted = false;
				reloadFinished = false;
				return;
			}
		}
		if (fractionalReload)
		{
			ResetReloadCooldown();
			UpdateShieldState(bHeld: true);
		}
		reloadStarted = false;
		reloadFinished = true;
		if (!fractionalReload)
		{
			TryReloadMagazine(player.inventory);
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server.MaxRepeatedElements(64)]
	private void CLProject(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		if (reloadFinished && HasReloadCooldown())
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Reloading (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "reload_cooldown");
			return;
		}
		reloadStarted = false;
		reloadFinished = false;
		if (primaryMagazine.contents <= 0 && !base.UsingInfiniteAmmoCheat)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Project magazine empty (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "magazine_empty_project");
			return;
		}
		ItemDefinition primaryMagazineAmmo = PrimaryMagazineAmmo;
		using ProjectileShoot projectileShoot = msg.read.Proto<ProjectileShoot>();
		if (primaryMagazineAmmo.itemid != projectileShoot.ammoType)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Ammo mismatch (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "ammo_mismatch");
			return;
		}
		if (!base.UsingInfiniteAmmoCheat)
		{
			ModifyAmmoCount(-1);
		}
		ItemModProjectile component = primaryMagazineAmmo.GetComponent<ItemModProjectile>();
		if (component == null)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "mod_missing");
			return;
		}
		if (projectileShoot.projectiles.Count > component.numProjectiles)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Count mismatch (" + base.ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, this, "count_mismatch");
			return;
		}
		Interface.CallHook("OnWeaponFired", this, msg.player, component, projectileShoot);
		if (player.InGesture)
		{
			return;
		}
		SignalBroadcast(Signal.Attack, string.Empty, msg.connection, GetAttackEffect(), maxAttackEffectDistance);
		player.CleanupExpiredProjectiles();
		Guid projectileGroupId = Guid.NewGuid();
		foreach (ProjectileShoot.Projectile projectile in projectileShoot.projectiles)
		{
			if (player.HasFiredProjectile(projectile.projectileID))
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, $"Duplicate ID ({projectile.projectileID})");
				player.stats.combat.LogInvalid(player, this, "duplicate_id");
				continue;
			}
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
			player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, this, primaryMagazineAmmo, projectileGroupId, positionOffset);
			if (!player.limitNetworking)
			{
				CreateProjectileEffectClientside(component.GetOverrideProjectile(this).resourcePath, projectile.startPos, projectile.startVel, projectile.seed, msg.connection, IsSilenced());
			}
		}
		player.MakeNoise(player.transform.position, BaseCombatEntity.ActionVolume.Loud);
		SingletonComponent<NpcNoiseManager>.Instance.OnWeaponShot(player, this);
		player.stats.Add(component.category + "_fired", projectileShoot.projectiles.Count, (Stats)5);
		player.LifeStoryShotFired(this);
		StartAttackCooldown(ScaleRepeatDelay(repeatDelay) + animationDelay);
		player.MarkHostileFor();
		UpdateItemCondition();
		DidAttackServerside();
		BaseMountable mounted = player.GetMounted();
		if (mounted != null)
		{
			mounted.OnWeaponFired(this);
		}
		EACServer.LogPlayerUseWeapon(player, this);
	}

	public void CreateProjectileEffectClientside(string prefabName, Vector3 pos, Vector3 velocity, int seed, Connection sourceConnection, bool silenced = false, bool forceClientsideEffects = false, List<Connection> targets = null, float distanceOverride = 0f)
	{
		if (Interface.CallHook("OnClientProjectileEffectCreate", sourceConnection, this, prefabName) == null)
		{
			Effect effect = reusableInstance;
			effect.InitWithSourceEntity(Effect.Type.Projectile, this, pos, velocity, sourceConnection);
			effect.scale = (silenced ? 0f : 1f);
			if (forceClientsideEffects)
			{
				effect.scale = 2f;
			}
			effect.pooledString = prefabName;
			effect.number = seed;
			effect.targets = targets;
			effect.distanceOverride = distanceOverride;
			EffectNetwork.Send(effect);
		}
	}

	public void UpdateItemCondition()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			return;
		}
		float barrelConditionLoss = primaryMagazine.ammoType.GetComponent<ItemModProjectile>().barrelConditionLoss;
		float num = 0.25f;
		bool usingInfiniteAmmoCheat = base.UsingInfiniteAmmoCheat;
		if (!usingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(num + barrelConditionLoss);
		}
		if (ownerItem.contents == null || ownerItem.contents.itemList == null)
		{
			return;
		}
		for (int num2 = ownerItem.contents.itemList.Count - 1; num2 >= 0; num2--)
		{
			Item item = ownerItem.contents.itemList[num2];
			if (item != null && !usingInfiniteAmmoCheat)
			{
				float num3 = 1f;
				ProjectileWeaponMod projectileWeaponMod = item.GetHeldEntity() as ProjectileWeaponMod;
				if (projectileWeaponMod != null)
				{
					num3 = projectileWeaponMod.ConditionLossMultiplier;
				}
				item.LoseCondition((num + barrelConditionLoss) * num3);
			}
		}
	}

	public bool IsSilenced()
	{
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if (projectileWeaponMod != null && projectileWeaponMod.isSilencer && !projectileWeaponMod.IsBroken())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AllowsPingUsage()
	{
		using (TimeWarning.New("AllowsPingUsage"))
		{
			if (children != null)
			{
				foreach (BaseEntity child in children)
				{
					ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
					if (projectileWeaponMod != null && projectileWeaponMod.allowPings && !projectileWeaponMod.IsBroken())
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public string GetAttackEffectAdditive()
	{
		string result = "";
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if (!(projectileWeaponMod == null) && projectileWeaponMod.additiveEffect.isValid)
				{
					result = projectileWeaponMod.additiveEffect.resourcePath;
					break;
				}
			}
		}
		return result;
	}

	protected string GetAttackEffect()
	{
		string resourcePath = attackFX.resourcePath;
		if (primaryMagazine.ammoType != null)
		{
			ItemModProjectile component = primaryMagazine.ammoType.GetComponent<ItemModProjectile>();
			if (component.attackEffectOverride.isValid)
			{
				resourcePath = component.attackEffectOverride.resourcePath;
			}
		}
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				ProjectileWeaponMod projectileWeaponMod = child as ProjectileWeaponMod;
				if (projectileWeaponMod == null)
				{
					continue;
				}
				if (projectileWeaponMod.isSilencer)
				{
					resourcePath = projectileWeaponMod.defaultSilencerEffect.resourcePath;
					if (silencedAttack.isValid)
					{
						resourcePath = silencedAttack.resourcePath;
						GameObject gameObject = silencedAttack.Get();
						if (gameObject != null && gameObject.TryGetComponent<EffectSilencerSelect>(out var component2) && component2.GetEffectForSilencerType(projectileWeaponMod.silencerType, out var result))
						{
							resourcePath = result.resourcePath;
						}
					}
					break;
				}
				if (projectileWeaponMod.isMuzzleBrake)
				{
					if (muzzleBrakeAttack.isValid)
					{
						resourcePath = muzzleBrakeAttack.resourcePath;
					}
					break;
				}
			}
		}
		return resourcePath;
	}

	public BaseEntity FindWeaponModEntity(Item weaponModItem)
	{
		if (weaponModItem == null)
		{
			return null;
		}
		ItemModEntity component = weaponModItem.info.GetComponent<ItemModEntity>();
		object obj;
		if ((object)component == null)
		{
			obj = null;
		}
		else
		{
			GameObjectRef entityPrefab = component.entityPrefab;
			if (entityPrefab == null)
			{
				obj = null;
			}
			else
			{
				GameObject obj2 = entityPrefab.Get();
				obj = (((object)obj2 != null) ? GameObjectEx.ToBaseEntity(obj2) : null);
			}
		}
		BaseEntity baseEntity = (BaseEntity)obj;
		if (baseEntity == null)
		{
			return null;
		}
		foreach (BaseEntity child in children)
		{
			if (!(child == null) && child.prefabID == baseEntity.prefabID)
			{
				return child;
			}
		}
		return null;
	}

	public override bool CanUseNetworkCache(Connection sendingTo)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null || ownerPlayer.net == null)
		{
			return true;
		}
		if (ownerPlayer.IsBeingSpectated)
		{
			return false;
		}
		Connection connection = ownerPlayer.net.connection;
		if (sendingTo == null || connection == null)
		{
			return true;
		}
		return sendingTo != connection;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseProjectile = Facepunch.Pool.Get<ProtoBuf.BaseProjectile>();
		if (info.forDisk || info.SendingTo(GetOwnerConnection()) || ForceSendMagazine(info))
		{
			info.msg.baseProjectile.primaryMagazine = primaryMagazine.Save();
		}
	}

	public virtual bool ForceSendMagazine(SaveInfo saveInfo)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((bool)ownerPlayer && ownerPlayer.IsBeingSpectated)
		{
			ReadOnlySpan<BasePlayer> spectators = ownerPlayer.GetSpectators();
			for (int i = 0; i < spectators.Length; i++)
			{
				if (spectators[i].net.connection == saveInfo.forConnection)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseProjectile != null && info.msg.baseProjectile.primaryMagazine != null)
		{
			primaryMagazine.Load(info.msg.baseProjectile.primaryMagazine);
		}
	}
}
