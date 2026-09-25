using ConVar;
using Oxide.Core;
using Rust;
using UnityEngine;

public class SupplyDrop : LootContainer
{
	public const Flags FlagNightLight = Flags.Reserved1;

	private const Flags ShowParachute = Flags.Reserved3;

	public GameObject ParachuteRoot;

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		isLootable = false;
		Invoke(MakeLootable, 300f);
		InvokeRepeating(CheckNightLight, 0f, 30f);
	}

	public void RemoveParachute()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public void MakeLootable()
	{
		isLootable = true;
	}

	private void OnCollisionEnter(Collision collision)
	{
		bool flag = ((1 << collision.collider.gameObject.layer) & 0x40A10111) > 0;
		bool num = ((1 << collision.collider.gameObject.layer) & 0x8000000) > 0;
		BaseEntity entity = CollisionEx.GetEntity(collision);
		if (num && entity is Tugboat)
		{
			flag = true;
		}
		if (num && (entity is BoatBuildingBlock || PlayerBoat.IsChildOfFinishedPlayerBoat(entity)))
		{
			flag = true;
		}
		if (flag)
		{
			RemoveParachute();
			MakeLootable();
		}
		Interface.CallHook("OnSupplyDropLanded", this);
	}

	public void CheckNightLight()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, Env.time > 20f || Env.time < 7f);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (ParachuteRoot != null)
		{
			ParachuteRoot.SetActive((next & Flags.Reserved3) == Flags.Reserved3);
		}
	}
}
