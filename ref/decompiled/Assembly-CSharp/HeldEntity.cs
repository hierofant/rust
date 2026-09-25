#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class HeldEntity : BaseEntity
{
	[Serializable]
	public struct HoldInfo
	{
		public bool UseCustomSpineOffset;

		public Vector3 CustomSpineOffset;

		public Vector3 ShieldBoneArmRotationAdditive;
	}

	[Serializable]
	public class HolsterInfo
	{
		public enum HolsterSlot
		{
			BACK,
			RIGHT_THIGH,
			LEFT_THIGH,
			BACK_SHIELD
		}

		public enum BackpackSlot
		{
			LEFT,
			RIGHT
		}

		public HolsterSlot slot;

		public BackpackSlot backpackSlot;

		public bool displayWhenHolstered;

		public string holsterBone = "spine3";

		public Vector3 holsterOffset;

		public Vector3 holsterRotationOffset;

		public Vector3 backpackHolsterOffset;

		public Vector3 backpackHolsterRotationOffset;

		public Quaternion holsterRotationOffsetQ
		{
			get
			{
				if (!(holsterRotationOffset == Vector3.zero))
				{
					return Quaternion.Euler(holsterRotationOffset);
				}
				return Quaternion.identity;
			}
		}

		public Quaternion backpackHolsterRotationOffsetQ
		{
			get
			{
				if (!(backpackHolsterRotationOffset == Vector3.zero))
				{
					return Quaternion.Euler(backpackHolsterRotationOffset);
				}
				return Quaternion.identity;
			}
		}
	}

	public static class HeldEntityFlags
	{
		public const Flags Deployed = Flags.Reserved4;

		public const Flags LightsOn = Flags.Reserved5;

		public const Flags ShieldEquipped = Flags.Reserved13;
	}

	public enum heldEntityVisState
	{
		UNSET,
		Invis,
		Hand,
		Holster,
		GenericVis
	}

	private static int param_deploy = Animator.StringToHash("deploy");

	public Animator worldModelAnimator;

	public SoundDefinition thirdPersonDeploySound;

	public SoundDefinition thirdPersonAimSound;

	public SoundDefinition thirdPersonAimEndSound;

	public bool hideViewmodel;

	public const Flags Flag_ForceVisible = Flags.Reserved8;

	[Header("Held Entity")]
	public string handBone = "r_prop";

	public AnimatorOverrideController HoldAnimationOverride;

	public HoldInfo holdInfo;

	public bool isBuildingTool;

	public bool canBeUsedWithShield;

	[ReadOnly]
	public bool ForwardSignalsFromPlayer;

	public BasePlayer forcedOwner;

	public bool useOwnerForward = true;

	public bool CanSetLightsOn;

	public HolsterInfo holsterInfo;

	[Header("Camera")]
	public BasePlayer.CameraMode HeldCameraMode;

	[Header("Hostility")]
	public float hostileScore;

	public Vector3 FirstPersonArmOffset;

	public Vector3 FirstPersonArmRotation;

	[Range(0f, 1f)]
	public float FirstPersonRotationStrength = 1f;

	[Header("Attachments")]
	public Transform AttachmentAnchor;

	private Item cachedItem;

	public bool holsterVisible;

	public bool genericVisible;

	private heldEntityVisState currentVisState;

	public ItemId ownerItemUID;

	public bool hostile => hostileScore > 0f;

	public virtual Transform MuzzleTransform => null;

	public virtual ItemModWearable WearableWhileEquipped => null;

	public virtual bool IsShield => false;

	protected bool UsingInfiniteAmmoCheat
	{
		get
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer == null || (!ownerPlayer.IsAdmin && !ownerPlayer.IsDeveloper))
			{
				return false;
			}
			return ownerPlayer.GetInfoBool("player.infiniteammo", defaultVal: false);
		}
	}

	public virtual bool IsUsableByTurret => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HeldEntity.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void SendPunch(Vector3 amount, float duration)
	{
		ClientRPC(RpcTarget.Player("CL_Punch", GetOwnerPlayer()), amount, duration);
	}

	public bool LightsOn()
	{
		return HasFlag(Flags.Reserved5);
	}

	public bool IsDeployed()
	{
		return HasFlag(Flags.Reserved4);
	}

	public bool HasShieldEquipped()
	{
		return HasFlag(Flags.Reserved13);
	}

	public BasePlayer GetOwnerPlayer()
	{
		if (forcedOwner != null)
		{
			return forcedOwner;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (!baseEntity.IsValid())
		{
			return null;
		}
		BasePlayer basePlayer = baseEntity.ToPlayer();
		if (basePlayer == null)
		{
			return null;
		}
		if (basePlayer.IsDead())
		{
			return null;
		}
		return basePlayer;
	}

	public bool TryGetOwner(out BaseEntity owner)
	{
		owner = null;
		BaseEntity baseEntity = GetParentEntity();
		if (!baseEntity.IsValid())
		{
			return false;
		}
		BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
		if (baseCombatEntity == null)
		{
			return false;
		}
		if (baseCombatEntity.IsDead())
		{
			return false;
		}
		owner = baseCombatEntity;
		return true;
	}

	public bool TryGetOwnerPlayer(out BasePlayer ownerPlayer)
	{
		ownerPlayer = GetOwnerPlayer();
		return ownerPlayer != null;
	}

	public Connection GetOwnerConnection()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return null;
		}
		if (ownerPlayer.net == null)
		{
			return null;
		}
		return ownerPlayer.net.connection;
	}

	public Item GetCachedItem()
	{
		return cachedItem;
	}

	public void OnItemChanged(Item item)
	{
		cachedItem = item;
	}

	public virtual void SetOwnerPlayer(BasePlayer player)
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		Assert.IsTrue(player.isServer, "Player should be serverside!");
		TransformEx.Identity(base.gameObject);
		SetParent(player, handBone);
		if (!IsShield)
		{
			SetHeld(bHeld: false);
		}
	}

	public virtual void ClearOwnerPlayer()
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		SetHeld(bHeld: false);
		SetParent(null);
	}

	public virtual void SetVisibleWhileHolstered(bool visible)
	{
		if (holsterInfo.displayWhenHolstered)
		{
			holsterVisible = visible;
			genericVisible = false;
			UpdateHeldItemVisibility();
		}
	}

	public virtual void SetGenericVisible(bool wantsVis)
	{
		genericVisible = wantsVis;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, wantsVis);
		}
		UpdateHeldItemVisibility();
	}

	public uint GetBone(string bone)
	{
		return StringPool.Get(bone);
	}

	public virtual void SetLightsOn(bool isOn)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, isOn);
	}

	public void UpdateHeldItemVisibility()
	{
		bool flag = false;
		if (!genericVisible && (bool)GetOwnerPlayer())
		{
			bool flag2 = GetOwnerPlayer().GetHeldEntity() == this || (IsShield && IsDeployed());
			flag = ((!ConVar.Server.showHolsteredItems && !flag2) ? UpdateVisiblity_Invis() : (flag2 ? UpdateVisibility_Hand() : ((!holsterVisible) ? UpdateVisiblity_Invis() : UpdateVisiblity_Holster())));
		}
		else if (genericVisible)
		{
			flag = UpdateVisibility_GenericVis();
		}
		else if (!genericVisible)
		{
			flag = UpdateVisiblity_Invis();
		}
		if (flag)
		{
			SendNetworkUpdate();
		}
	}

	public bool UpdateVisibility_Hand()
	{
		if (currentVisState == heldEntityVisState.Hand)
		{
			return false;
		}
		currentVisState = heldEntityVisState.Hand;
		base.limitNetworking = false;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		SetParent(GetOwnerPlayer(), GetBone(handBone));
		return true;
	}

	public bool UpdateVisibility_GenericVis()
	{
		if (currentVisState == heldEntityVisState.GenericVis)
		{
			return false;
		}
		currentVisState = heldEntityVisState.GenericVis;
		base.limitNetworking = false;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		return true;
	}

	public bool UpdateVisiblity_Holster()
	{
		if (currentVisState == heldEntityVisState.Holster)
		{
			return false;
		}
		currentVisState = heldEntityVisState.Holster;
		base.limitNetworking = false;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		SetParent(GetOwnerPlayer(), GetBone(holsterInfo.holsterBone));
		return true;
	}

	public bool UpdateVisiblity_Invis()
	{
		if (currentVisState == heldEntityVisState.Invis)
		{
			return false;
		}
		currentVisState = heldEntityVisState.Invis;
		SetParent(GetOwnerPlayer(), GetBone(handBone));
		base.limitNetworking = true;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: true);
		}
		return true;
	}

	public virtual void SetHeld(bool bHeld)
	{
		Assert.IsTrue(base.isServer, "Should be server!");
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			bool num = HasFlag(Flags.Reserved4);
			flagsUpdateScope.Set(Flags.Reserved4, bHeld);
			if (!bHeld)
			{
				UpdateVisiblity_Invis();
			}
			if (num || bHeld)
			{
				UpdateShieldState(bHeld && canBeUsedWithShield);
			}
			base.limitNetworking = !bHeld;
			flagsUpdateScope.Set(Flags.Disabled, !bHeld);
		}
		SendNetworkUpdate();
		OnHeldChanged();
	}

	public void UpdateShieldState(bool bHeld)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		Item anyBackpack = ownerPlayer.inventory.GetAnyBackpack();
		if (anyBackpack != null && anyBackpack.info.TryGetComponent<ItemModShield>(out var _))
		{
			HeldEntity heldEntity = anyBackpack.GetHeldEntity() as HeldEntity;
			if (anyBackpack.isBroken)
			{
				bHeld = false;
			}
			if (canBeUsedWithShield && bHeld && heldEntity != null && !ownerPlayer.WantsShieldOnBack())
			{
				using (FlagsUpdateScope flagsUpdateScope = heldEntity.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					heldEntity.UpdateVisibility_Hand();
					flagsUpdateScope.Set(Flags.Reserved4, b: true);
					flagsUpdateScope2.Set(Flags.Reserved13, b: true);
					return;
				}
			}
			if (heldEntity != null)
			{
				heldEntity.UpdateVisiblity_Holster();
				using (FlagsUpdateScope flagsUpdateScope3 = heldEntity.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope3.Set(Flags.Reserved4, b: false);
				}
				flagsUpdateScope2.Set(Flags.Reserved13, b: false);
			}
		}
		else
		{
			flagsUpdateScope2.Set(Flags.Reserved13, b: false);
		}
	}

	public virtual void OnHeldChanged()
	{
	}

	public virtual bool CanBeUsedInWater()
	{
		return false;
	}

	public virtual bool BlocksGestures()
	{
		return false;
	}

	protected Item GetOwnerItem()
	{
		return GetOwnerItem(GetOwnerPlayer());
	}

	protected Item GetOwnerItem(BasePlayer ownerPlayer)
	{
		if (ownerPlayer == null || ownerPlayer.inventory == null)
		{
			return null;
		}
		return ownerPlayer.inventory.FindItemByUID(ownerItemUID);
	}

	public override Item GetItem()
	{
		return GetOwnerItem();
	}

	public ItemDefinition GetOwnerItemDefinition()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			Debug.LogWarning("GetOwnerItem - null!", this);
			return null;
		}
		return ownerItem.info;
	}

	public ItemDefinition GetOwnerItemDefinition(BasePlayer ownerPlayer)
	{
		return GetOwnerItem(ownerPlayer)?.info;
	}

	public virtual void CollectedForCrafting(Item item, BasePlayer crafter)
	{
	}

	public virtual void ReturnedFromCancelledCraft(Item item, BasePlayer crafter)
	{
	}

	public virtual void ServerCommand(Item item, string command, BasePlayer player)
	{
	}

	public virtual void SetupHeldEntity(Item item)
	{
		ownerItemUID = item.uid;
		InitOwnerPlayer();
	}

	public override void PostServerLoad()
	{
		InitOwnerPlayer();
	}

	private void InitOwnerPlayer()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer != null)
		{
			SetOwnerPlayer(ownerPlayer);
		}
		else
		{
			ClearOwnerPlayer();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.heldEntity = Facepunch.Pool.Get<ProtoBuf.HeldEntity>();
		info.msg.heldEntity.itemUID = ownerItemUID;
	}

	public void DestroyThis()
	{
		GetOwnerItem()?.Remove();
	}

	protected bool HasItemAmount()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			return ownerItem.amount > 0;
		}
		return false;
	}

	protected bool UseItemAmount(int iAmount, bool reduceItemOwnership = true)
	{
		if (iAmount <= 0)
		{
			return false;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null)
		{
			ClientRPC(RpcTarget.NetworkGroup("Client_OnPreDestroyViaItemSpent"));
			DestroyThis();
			return true;
		}
		ownerItem.amount -= iAmount;
		ownerItem.MarkDirty();
		if (reduceItemOwnership)
		{
			ownerItem.ReduceItemOwnership(iAmount);
		}
		if (ownerItem.amount <= 0)
		{
			ClientRPC(RpcTarget.NetworkGroup("Client_OnPreDestroyViaItemSpent"));
			DestroyThis();
			return true;
		}
		return false;
	}

	public void ServerUse()
	{
		ServerUse(HeldEntityServerUseParams.Default);
	}

	public virtual void ServerUse(HeldEntityServerUseParams parameters)
	{
	}

	public virtual void ServerTick(BasePlayer byPlayer)
	{
	}

	public override void SwitchParent(BaseEntity ent)
	{
	}

	public virtual bool IsInstrument()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.heldEntity != null)
		{
			ownerItemUID = info.msg.heldEntity.itemUID;
		}
	}
}
