using System.Collections.Generic;
using Rust;
using UnityEngine;

public class HelicopterDebris : ServerGib
{
	public ItemDefinition metalFragments;

	public ItemDefinition hqMetal;

	public ItemDefinition charcoal;

	[Tooltip("Divide mass by this amount to produce a scalar of resources, default = 5")]
	public float massReductionScalar = 5f;

	private ResourceDispenser resourceDispenser;

	private const int coolDownTime = 480;

	private static Translate.Phrase TooHotToHarvestPhrase = new Translate.Phrase("debris_too_hot", "The debris is too hot to harvest! Wait for it to cool off");

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.OnFire, b: true);
		}
		Invoke(OnCooledDown, 480f);
	}

	public void OnCooledDown()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, b: false);
	}

	public override void PhysicsInit(Mesh mesh)
	{
		base.PhysicsInit(mesh);
		if (!base.isServer)
		{
			return;
		}
		resourceDispenser = GetComponent<ResourceDispenser>();
		float num = Mathf.Clamp01(GetComponent<Rigidbody>().mass / massReductionScalar);
		resourceDispenser.containedItems = new List<ItemAmount>();
		if (num > 0.75f && hqMetal != null)
		{
			resourceDispenser.containedItems.Add(new ItemAmount(hqMetal, Mathf.CeilToInt(7f * num)));
		}
		if (num > 0f)
		{
			if (metalFragments != null)
			{
				resourceDispenser.containedItems.Add(new ItemAmount(metalFragments, Mathf.CeilToInt(150f * num)));
			}
			if (charcoal != null)
			{
				resourceDispenser.containedItems.Add(new ItemAmount(charcoal, Mathf.CeilToInt(80f * num)));
			}
		}
		resourceDispenser.Initialize();
	}

	public bool IsTooHot()
	{
		return HasFlag(Flags.OnFire);
	}

	public override void OnAttacked(HitInfo info)
	{
		if (IsTooHot() && info.WeaponPrefab is BaseMelee)
		{
			if (info.Initiator is BasePlayer basePlayer)
			{
				HitInfo hitInfo = new HitInfo();
				hitInfo.damageTypes.Add(DamageType.Heat, 5f);
				hitInfo.DoHitEffects = true;
				hitInfo.DidHit = true;
				hitInfo.HitBone = 0u;
				hitInfo.Initiator = this;
				hitInfo.PointStart = base.transform.position;
				Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/fire.prefab", info.Initiator, 0u, new Vector3(0f, 1f, 0f), Vector3.up);
				basePlayer.ShowToast(GameTip.Styles.Red_Normal, TooHotToHarvestPhrase, false);
			}
		}
		else
		{
			if ((bool)resourceDispenser)
			{
				resourceDispenser.OnResourceDispenserAttacked(info);
			}
			base.OnAttacked(info);
		}
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}
}
