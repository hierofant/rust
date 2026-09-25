using Facepunch;
using UnityEngine;

public class DroneStorage : StorageContainer
{
	[Header("Drone Storage")]
	public Transform AttachPoint;

	public Vector3 ReleaseVelocity;

	public float GrenadeWeaponDelayMod = 3f;

	public float ThrownWeaponDelayMod = 1f;

	private static readonly Translate.Phrase FailPhrase = new Translate.Phrase("drone_storage.fail", "Drone is stuck, can't access inventory");

	private const float DroneBoxOffset = 0.14f;

	public Drone Drone { get; set; }

	public void UpdateFlags()
	{
		using FlagsUpdateScope flagsUpdateScope = Drone.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (Drone.HasFlag(Flags.Reserved5))
		{
			if (!TryGetItem(out var item) || !TryGetHeldEntity(item, out var held) || !(held is ThrownWeapon thrownWeapon))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: false);
			}
			else
			{
				flagsUpdateScope.Set(Flags.Reserved5, thrownWeapon.HasAttackCooldown());
			}
		}
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		if (!base.ItemFilter(player, item, targetSlot))
		{
			return false;
		}
		if (!Drone.HasFlag(Flags.Reserved3) && Drone.body.SweepTest(Drone.transform.up, out var _, 0.14f))
		{
			return false;
		}
		return true;
	}

	public override PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		if (Drone.body.SweepTest(Drone.transform.up, out var _, 0.14f))
		{
			return PlayerInventory.CanMoveFromResponse.Failure(FailPhrase);
		}
		return base.CanMoveFrom(player, item);
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if (!base.CanOpenLootPanel(player, panelName))
		{
			return false;
		}
		if (Drone.body.SweepTest(Drone.transform.up, out var _, 0.14f))
		{
			player.ShowToast(GameTip.Styles.Error, FailPhrase, false);
			return false;
		}
		return true;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (added && !Drone.HasFlag(Flags.Reserved2) && !Drone.HasFlag(Flags.Reserved3))
		{
			Drone.body.position += Drone.transform.up * 0.14f;
		}
		Drone.body.WakeUp();
		Drone.body.isKinematic = false;
	}

	public bool TryServerDrop()
	{
		if (!TryGetItem(out var item))
		{
			return false;
		}
		bool flag = false;
		if (TryGetHeldEntity(item, out var held) && held is ThrownWeapon weapon)
		{
			return TryServerWeaponDrop(base.inventory.GetSlot(0), weapon);
		}
		return TryServerItemDrop(base.inventory.GetSlot(0));
	}

	private bool TryGetItem(out Item item)
	{
		item = null;
		if (base.inventory.IsEmpty())
		{
			return false;
		}
		item = base.inventory.GetSlot(0);
		if (item == null)
		{
			return false;
		}
		return true;
	}

	private bool TryGetHeldEntity(Item item, out BaseEntity held)
	{
		held = null;
		if (item == null)
		{
			return false;
		}
		held = item.GetHeldEntity();
		if (held == null)
		{
			return false;
		}
		return true;
	}

	private bool TryServerWeaponDrop(Item item, ThrownWeapon weapon)
	{
		if (item.amount <= 0 || weapon.HasAttackCooldown())
		{
			return false;
		}
		if (Drone == null)
		{
			return false;
		}
		AttachPoint.GetPositionAndRotation(out var position, out var rotation);
		Vector3 throwVelocityOverride = GetInheritedThrowVelocity(rotation * Vector3.down) + ReleaseVelocity;
		BasePlayer owningPlayer = Drone.ToPlayer();
		weapon.DoThrowImpl(position, rotation * Vector3.down, owningPlayer, out var thrownEntity, 1f, throwVelocityOverride, item);
		if (thrownEntity is TimedExplosive timedExplosive)
		{
			timedExplosive.wasDroneDropped = true;
		}
		if (weapon is GrenadeWeapon)
		{
			weapon.StartAttackCooldown(weapon.repeatDelay * GrenadeWeaponDelayMod);
		}
		else
		{
			weapon.StartAttackCooldown(weapon.repeatDelay * ThrownWeaponDelayMod);
		}
		item.UseItem();
		TempIgnoreParent(thrownEntity);
		Drone.MarkHostileFor();
		if (weapon.HasAttackCooldown())
		{
			using FlagsUpdateScope flagsUpdateScope = Drone.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved5, b: true);
		}
		SendNetworkUpdateImmediate();
		return true;
	}

	private bool TryServerItemDrop(Item item)
	{
		AttachPoint.GetPositionAndRotation(out var position, out var rotation);
		BaseEntity ent = item.Drop(position, GetInheritedProjectileVelocity(rotation * Vector3.down) + ReleaseVelocity);
		TempIgnoreParent(ent);
		return true;
	}

	private void TempIgnoreParent(BaseEntity ent)
	{
		if (ent == null || !parentEntity.IsValid(serverside: true))
		{
			return;
		}
		ent.gameObject.SetIgnoreCollisions(parentEntity.Get(serverside: true).gameObject, ignore: true);
		Invoke(delegate
		{
			BaseEntity baseEntity = ent;
			if (!(baseEntity == null))
			{
				BaseEntity baseEntity2 = parentEntity.Get(serverside: true);
				if (!(baseEntity2 == null))
				{
					baseEntity2.gameObject.SetIgnoreCollisions(baseEntity.gameObject, ignore: false);
				}
			}
		}, 2f);
	}
}
