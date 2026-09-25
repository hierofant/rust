#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BoatBuildingBlock : BuildingBlock, IPlacementDirectionProvider
{
	public List<Transform> SnapCardinalDirections;

	public List<Vector3> CachedLocalSnapCardinalDirections;

	[ServerVar(Help = "(Generated) When enabled, damage dealt to a building block attached to a boat is forwarded up to the parent boat entity")]
	public static bool ForwardDamageToParentBoat = true;

	[ReplicatedVar]
	public static bool AlwaysDemolishable = true;

	[ReplicatedVar]
	public static bool AlwayRotatable = true;

	public bool ProvidesParentingTrigger;

	public bool Hull;

	public bool Floor;

	public float ContributingMass = 100f;

	public float ContributingHealth = 50f;

	public Transform[] DismountPoints;

	public float damageTaken;

	public ParticleSystem[] CardinalSplashFx = new ParticleSystem[0];

	public GameObject WaterDisplacement;

	[HideInInspector]
	public bool SendNetworkUpdateOnHealthChanged = true;

	private List<TriggerParent> parentTriggers;

	private List<Transform> originalTriggerParents;

	public override bool AlsoVisCheckParent => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BoatBuildingBlock.OnRpcMessage"))
		{
			if (rpc == 2419844654u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_WantsPushParentBoat");
				}
				using (TimeWarning.New("RPC_WantsPushParentBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2419844654u, "RPC_WantsPushParentBoat", this, player, 5f))
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
							RPC_WantsPushParentBoat(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_WantsPushParentBoat");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool HasAnySlot()
	{
		return false;
	}

	public Vector3 GetDismountCheckStart()
	{
		return base.transform.position + Vector3.up * 1f;
	}

	public List<Vector3> GetSnapForwardDirections()
	{
		return CachedLocalSnapCardinalDirections;
	}

	public bool IsFullyInsideOBB(OBB otherOBB)
	{
		OBB oBB = WorldSpaceBounds();
		if (!otherOBB.Contains(oBB.GetPoint(-1f, 0f, -1f)))
		{
			return false;
		}
		if (!otherOBB.Contains(oBB.GetPoint(1f, 0f, -1f)))
		{
			return false;
		}
		if (!otherOBB.Contains(oBB.GetPoint(1f, 0f, 1f)))
		{
			return false;
		}
		if (!otherOBB.Contains(oBB.GetPoint(-1f, 0f, 1f)))
		{
			return false;
		}
		return true;
	}

	public override bool IsDemolishable()
	{
		if (AlwaysDemolishable)
		{
			return true;
		}
		return base.IsDemolishable();
	}

	public override bool HasRotateFlag()
	{
		if (AlwayRotatable)
		{
			return true;
		}
		return base.HasRotateFlag();
	}

	public override float MaxHealth()
	{
		if (maxHealthOverride > 0f)
		{
			return maxHealthOverride;
		}
		return ContributingHealth;
	}

	public override bool Interactable()
	{
		return parentEntity.Get(base.isServer) == null;
	}

	public void SwitchToVehicle(bool loading)
	{
		if (!loading)
		{
			damageTaken = 0f;
		}
		if (currentSkin == null)
		{
			UpdateSkin(force: true);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (baseEntity != null && baseEntity is PlayerBoat playerBoat)
		{
			playerBoat.OnSubChildAdded(child);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk && !PlayerBoat.IsChildOfFinishedPlayerBoat(this))
		{
			DisableParentTrigger();
		}
		if (info.msg.boatBuildingBlock != null)
		{
			damageTaken = info.msg.boatBuildingBlock.damageTaken;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			DisableParentTrigger();
		}
		SetMaxHealth(ContributingHealth);
		SetHealthToMax();
	}

	protected override bool CanRotate(BasePlayer player)
	{
		if (PlayerBoat.GetParentPlayerBoat(this) != null)
		{
			return false;
		}
		return base.CanRotate(player);
	}

	protected override void OnSkinRefresh()
	{
		if (!IsFullySpawned() || (parentEntity.Get(serverside: true) is PlayerBoat playerBoat && (bool)playerBoat))
		{
			return;
		}
		Invoke(delegate
		{
			if (!(parentEntity.Get(serverside: true) is PlayerBoat playerBoat2) || !playerBoat2)
			{
				DisableParentTrigger();
			}
		}, 0f);
	}

	private void DisableParentTrigger()
	{
		EnsurePopulatedTriggersList();
		foreach (TriggerParent parentTrigger in parentTriggers)
		{
			parentTrigger.gameObject.SetActive(value: false);
		}
	}

	private void EnsurePopulatedTriggersList()
	{
		using (TimeWarning.New("EnsurePopulatedTriggersList"))
		{
			List<TriggerParent> list = parentTriggers;
			if (list != null && list.Count > 0)
			{
				bool flag = false;
				foreach (TriggerParent parentTrigger in parentTriggers)
				{
					if (parentTrigger == null)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			bool flag2 = false;
			BoatConstructionSkin boatConstructionSkin = currentSkin as BoatConstructionSkin;
			Debug.Assert((bool)boatConstructionSkin || flag2);
			if (parentTriggers == null)
			{
				parentTriggers = new List<TriggerParent>();
			}
			parentTriggers.Clear();
			if ((bool)boatConstructionSkin.parentTrigger)
			{
				parentTriggers.Add(boatConstructionSkin.parentTrigger);
			}
			foreach (GameObject conditional in boatConstructionSkin.conditionals)
			{
				if (conditional.TryGetComponent<BoatConstructionSkin>(out var component) && (bool)component.parentTrigger)
				{
					parentTriggers.Add(component.parentTrigger);
				}
			}
		}
	}

	public List<TriggerParent> SetTriggerParent(PlayerBoat boat)
	{
		BoatConstructionSkin obj = currentSkin as BoatConstructionSkin;
		Debug.Assert(obj);
		TriggerHurtNotChild hurtTrigger = obj.hurtTrigger;
		if ((bool)hurtTrigger)
		{
			hurtTrigger.SetSourceEntity(boat);
		}
		EnsurePopulatedTriggersList();
		foreach (TriggerParent parentTrigger in parentTriggers)
		{
			parentTrigger.gameObject.SetActive(value: true);
		}
		originalTriggerParents = new List<Transform>(parentTriggers.Count);
		for (int i = 0; i < parentTriggers.Count; i++)
		{
			originalTriggerParents.Add((parentTriggers[i] != null) ? parentTriggers[i].transform.parent : null);
		}
		foreach (TriggerParent parentTrigger2 in parentTriggers)
		{
			parentTrigger2.transform.SetParent(boat.transform);
			parentTrigger2.associatedMountable = boat;
		}
		return parentTriggers;
	}

	public void ResetTriggerParent()
	{
		Debug.Assert(parentTriggers.Count == originalTriggerParents.Count);
		for (int i = 0; i < parentTriggers.Count; i++)
		{
			parentTriggers[i].transform.SetParent(originalTriggerParents[i]);
			parentTriggers[i].gameObject.SetActive(value: false);
		}
		TriggerHurtNotChild triggerHurtNotChild = (currentSkin as BoatConstructionSkin)?.hurtTrigger;
		if ((bool)triggerHurtNotChild)
		{
			triggerHurtNotChild.ClearSourceEntity();
		}
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if (ForwardDamageToParentBoat && parentPlayerBoat != null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBuildingBlockHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public void RecordDamageTaken(float amount)
	{
		damageTaken += Mathf.Abs(amount);
	}

	[RPC_Server.MaxDistance(5f)]
	[RPC_Server]
	public void RPC_WantsPushParentBoat(RPCMessage msg)
	{
		if (!(msg.player == null))
		{
			PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
			if (!(parentPlayerBoat == null))
			{
				parentPlayerBoat.RPC_WantsPush(msg);
			}
		}
	}

	public override void AdminKill()
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity != null)
		{
			baseEntity.AdminKill();
		}
		else
		{
			base.AdminKill();
		}
	}

	public override void DoRepair(BasePlayer player)
	{
		if (!PlayerBoat.IsChildOfFinishedPlayerBoat(this) || PlayerBoat.HammerRepairEnabled)
		{
			base.DoRepair(player);
		}
	}

	public override bool ShouldRepairViaParent()
	{
		if (!PlayerBoat.HammerRepairEnabled)
		{
			return false;
		}
		return PlayerBoat.GetParentPlayerBoat(this) != null;
	}

	public override BaseCombatEntity GetRepairableParent()
	{
		return PlayerBoat.GetParentPlayerBoat(this);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.boatBuildingBlock = Facepunch.Pool.Get<ProtoBuf.BoatBuildingBlock>();
			info.msg.boatBuildingBlock.damageTaken = damageTaken;
		}
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		if (base.isServer && Mathf.RoundToInt(oldvalue) != Mathf.RoundToInt(newvalue) && SendNetworkUpdateOnHealthChanged)
		{
			SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
		}
	}

	public override bool HasDemolishPrivilege(BasePlayer player)
	{
		if (PlayerBoat.GetParentPlayerBoat(this) != null)
		{
			return false;
		}
		if (base.isServer)
		{
			if (base.OwnerID == (ulong)player.userID)
			{
				return true;
			}
			BoatBuildingStation forPlayer = BoatBuildingStation.GetForPlayer(player);
			if (forPlayer != null)
			{
				return forPlayer.CanPlayerDemolish(player);
			}
			return false;
		}
		return false;
	}
}
