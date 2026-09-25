using System;
using ConVar;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class SatelliteCrashRemains : BaseCombatEntity
{
	[Tooltip("Minutes the crash remains persist before despawning. 0 or less = stays until destroyed.")]
	public float lifetimeMinutes = 30f;

	[Tooltip("Candidate spawn points for the crash loot crates, placed to avoid the wreck geometry. Each crate picks one at random, snapped to terrain height and clearance-checked; unsuitable points are skipped. Only the XZ position is used.")]
	public Transform[] crateSpawnPoints;

	[Tooltip("Seconds the wreck stays too hot to harvest after impact. SatelliteCrash overwrites this at spawn with the satellite.wreck_fire_duration convar — this value only matters if the remains are spawned standalone.")]
	public float tooHotSeconds = 120f;

	[Tooltip("Objects hidden once the wreck is fully harvested")]
	public GameObject[] harvestedHideObjects;

	[Tooltip("Objects hidden once the wreck has cooled off (flame effects etc.)")]
	public GameObject[] cooledHideObjects;

	public Collider harvestCollider;

	public const Flags Flag_Harvested = Flags.Reserved1;

	public const Flags Flag_Cooled = Flags.Reserved2;

	private float despawnTime;

	private bool despawnScheduled;

	[NonSerialized]
	public float thrusterModuleFuelFraction = 0.5f;

	private ResourceDispenser resourceDispenser;

	private float tooHotUntil;

	private static readonly Translate.Phrase TooHotToHarvestPhrase = new Translate.Phrase("satcrashremains_too_hot", "The wreckage is too hot to harvest! Wait for it to cool off");

	private const string TooHotEffect = "assets/bundled/prefabs/fx/impacts/additive/fire.prefab";

	public bool IsFullyHarvested => HasFlag(Flags.Reserved1);

	public bool HasCooled => HasFlag(Flags.Reserved2);

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		SetVisible(harvestedHideObjects, (next & Flags.Reserved1) == 0);
		SetVisible(cooledHideObjects, (next & Flags.Reserved2) == 0);
	}

	private static void SetVisible(GameObject[] objects, bool visible)
	{
		if (objects == null)
		{
			return;
		}
		foreach (GameObject gameObject in objects)
		{
			if (gameObject != null && gameObject.activeSelf != visible)
			{
				gameObject.SetActive(visible);
			}
		}
	}

	private void GiveThrusterModules(BasePlayer player)
	{
		float conditionNormalized = Mathf.Clamp(thrusterModuleFuelFraction, Satellite.thruster_module_min_condition, 1f);
		for (int i = 0; i < Satellite.thruster_module_count; i++)
		{
			Item item = ItemManager.CreateByName("thruster.module", 1, 0uL);
			if (item == null)
			{
				break;
			}
			item.conditionNormalized = conditionNormalized;
			if (player != null)
			{
				player.GiveItem(item, GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
			}
			else
			{
				item.Drop(base.transform.position + Vector3.up, Vector3.zero);
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave && lifetimeMinutes > 0f)
		{
			ScheduleDespawn(lifetimeMinutes * 60f);
		}
		tooHotUntil = UnityEngine.Time.time + tooHotSeconds;
		Invoke(SetCooled, tooHotSeconds);
	}

	private void SetCooled()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved2, b: true);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasCooled)
		{
			tooHotUntil = 0f;
			CancelInvoke(SetCooled);
		}
	}

	public override void Hurt(HitInfo info)
	{
		float num = info.damageTypes.Total();
		float num2 = Health() - 1f;
		if (num > num2)
		{
			if (num2 <= 0f)
			{
				return;
			}
			info.damageTypes.ScaleAll(num2 / num);
		}
		base.Hurt(info);
	}

	public bool IsTooHot()
	{
		return tooHotUntil > UnityEngine.Time.time;
	}

	public override void OnAttacked(HitInfo info)
	{
		if (info.WeaponPrefab is BaseMelee && !IsFullyHarvested && IsHarvestCollider(info.HitBone))
		{
			if (IsTooHot())
			{
				if (info.Initiator is BasePlayer basePlayer)
				{
					Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/fire.prefab", basePlayer, 0u, new Vector3(0f, 1f, 0f), Vector3.up);
					basePlayer.ShowToast(GameTip.Styles.Red_Normal, TooHotToHarvestPhrase, false);
				}
			}
			else
			{
				if (resourceDispenser == null)
				{
					resourceDispenser = GetComponent<ResourceDispenser>();
				}
				if (resourceDispenser != null)
				{
					resourceDispenser.OnResourceDispenserAttacked(info);
				}
			}
		}
		if (IsFullyHarvested || (resourceDispenser != null && resourceDispenser.fractionRemaining <= 0f))
		{
			if (!IsFullyHarvested)
			{
				GiveThrusterModules(info.InitiatorPlayer);
			}
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
			return;
		}
		base.OnAttacked(info);
	}

	private bool IsHarvestCollider(uint hitBone)
	{
		if (harvestCollider == null)
		{
			return true;
		}
		return string.Equals(StringPool.Get(hitBone), harvestCollider.name, StringComparison.OrdinalIgnoreCase);
	}

	private void ScheduleDespawn(float seconds)
	{
		despawnTime = UnityEngine.Time.time + seconds;
		despawnScheduled = true;
		Invoke(KillRemains, seconds);
	}

	private void KillRemains()
	{
		despawnScheduled = false;
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.satelliteCrashRemains = Facepunch.Pool.Get<ProtoBuf.SatelliteCrashRemains>();
		info.msg.satelliteCrashRemains.lifetimeRemaining = (despawnScheduled ? Mathf.Max(0f, despawnTime - info.cachedTime.Time) : (-1f));
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.satelliteCrashRemains != null)
		{
			float lifetimeRemaining = info.msg.satelliteCrashRemains.lifetimeRemaining;
			if (lifetimeRemaining >= 0f)
			{
				CancelInvoke(KillRemains);
				ScheduleDespawn(lifetimeRemaining);
			}
		}
	}
}
