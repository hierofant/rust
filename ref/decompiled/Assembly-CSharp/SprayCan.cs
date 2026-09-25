#define UNITY_ASSERTIONS
using System;
using System.Linq;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class SprayCan : HeldEntity
{
	public struct ReskinPreserveInfo
	{
		public BaseEntityPreserveInfo baseEntityPreserve;

		public BaseCombatEntity.BaseCombatEntityPreserveInfo baseCombatEntityPreserve;

		public DecayEntity.DecayEntityPreserveInfo decayEntityPreserve;

		public CodeLock.CodeLockPreserveInfo codeLockPreserve;

		public PlanterBox.PlanterBoxPreserveInfo planterBoxPreserve;

		public BuildingPrivlidge.BuildingPrivilegePreserveInfo buildingPrivilegePreserve;

		public IItemContainerEntity.ContainerPreserveInfo containerPreserve;

		public IOEntity.IOEntityPreserveInfo ioEntityPreserve;

		public AutoTurret.AutoTurretPreserveInfo autoTurretPreserve;

		public ComputerStation.ComputerStationPreserveInfo computerStationPreserve;

		public ElectricOven.ElectricOvenPreserveInfo electricOvenPreserve;
	}

	public const float MaxFreeSprayDistanceFromStart = 10f;

	public const float MaxFreeSprayStartingDistance = 3f;

	private SprayCanSpray_Freehand paintingLine;

	public const Flags IsFreeSpraying = Flags.Reserved1;

	public static Translate.Phrase LastReskinError = string.Empty;

	public static BaseEntity LastReskinErrorEntity = null;

	public static string LastReskinErrorArgString = string.Empty;

	public static readonly Translate.Phrase FreeSprayNamePhrase = new Translate.Phrase("freespray_radial", "Free Spray");

	public static readonly Translate.Phrase FreeSprayDescPhrase = new Translate.Phrase("freespray_radial_desc", "Spray shapes freely with various colors");

	public static readonly Translate.Phrase BuildingSkinColourPhrase = new Translate.Phrase("buildingskin_colour", "Set colour");

	public static readonly Translate.Phrase BuildingSkinColourDescPhrase = new Translate.Phrase("buildingskin_colour_desc", "Set the block to the highlighted colour");

	public static readonly Translate.Phrase EntityChangeSkinPhrase = new Translate.Phrase("entity_changeskin", "Change skin");

	public static readonly Translate.Phrase EntityChangeSkinDescPhrase = new Translate.Phrase("entity_changeskin_desc", "Open skin selection");

	public static readonly Translate.Phrase EntityChangeColourPhrase = new Translate.Phrase("entity_changecolour", "Change colour");

	public static readonly Translate.Phrase EntityChangeColourDescPhrase = new Translate.Phrase("entity_changecolour_desc", "Open colour selection");

	public static readonly Translate.Phrase DoorMustBeClosed = new Translate.Phrase("error_doormustbeclosed", "Door must be closed");

	public static readonly Translate.Phrase NeedDoorAccess = new Translate.Phrase("error_needdooraccess", "Need door access");

	public static readonly Translate.Phrase CannotReskinThatDoor = new Translate.Phrase("error_cannotreskindoor", "Cannot reskin that door");

	public static readonly Translate.Phrase RecentlyDamaged = new Translate.Phrase("error_reskin_recentlydamaged", "Recently damaged, reskinnable in {0} seconds");

	public static readonly Translate.Phrase ExplosivesActive = new Translate.Phrase("error_explosivesactive", "Cannot reskin an object with explosives attached");

	public static readonly Translate.Phrase PlayerInAir = new Translate.Phrase("error_playerinair", "You must be on the ground");

	public static readonly Translate.Phrase BlockedByPlayer = new Translate.Phrase("error_blockedbyplayer_reskin", "Blocked by intersecting player");

	public static readonly Translate.Phrase BlockedBySomething = new Translate.Phrase("error_blockedbysomething", "Blocked by something");

	public static readonly Translate.Phrase PlayerIsMounted = new Translate.Phrase("error_playerismounted", "Player {0} is mounted");

	public static readonly Translate.Phrase CannotReskinInMonument = new Translate.Phrase("error_reskin_monument", "Cannot reskin objects inside a monument");

	public static readonly Translate.Phrase NeedLockAccess = new Translate.Phrase("error_needlockaccess", "Need lock access");

	public static readonly Translate.Phrase NotAuthorized = new Translate.Phrase("error_notauthorized", "You are not authorized");

	public SoundDefinition SpraySound;

	public GameObjectRef SkinSelectPanel;

	public float SprayCooldown = 2f;

	public float ConditionLossPerSpray = 10f;

	public float ConditionLossPerReskin = 10f;

	public GameObjectRef LinePrefab;

	public Color[] SprayColours = new Color[0];

	public float[] SprayWidths = new float[3] { 0.1f, 0.2f, 0.3f };

	public ParticleSystem worldSpaceSprayFx;

	public GameObjectRef ReskinEffect;

	public ItemDefinition SprayDecalItem;

	public GameObjectRef SprayDecalEntityRef;

	public SteamInventoryItem FreeSprayUnlockItem;

	public ParticleSystem.MinMaxGradient DecalSprayGradient;

	public SoundDefinition SprayLoopDef;

	[FormerlySerializedAs("ShippingCOntainerColourLookup")]
	public ConstructionSkin_ColourLookup ShippingContainerColourLookup;

	public const string ENEMY_BASE_STAT = "sprayed_enemy_base";

	private Translate.Phrase lastSprayError;

	private Action _actionClearBusy;

	private Action actionClearBusy => ClearBusy;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SprayCan.OnRpcMessage"))
		{
			if (rpc == 3490735573u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - BeginFreehandSpray");
				}
				using (TimeWarning.New("BeginFreehandSpray"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Read<int>();
						msg.read.Position = position;
						if (!RPC_Server.IsActiveItem.Test(3490735573u, "BeginFreehandSpray", this, player))
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
							BeginFreehandSpray(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in BeginFreehandSpray");
					}
				}
				return true;
			}
			if (rpc == 151738090 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ChangeItemSkin");
				}
				using (TimeWarning.New("ChangeItemSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(151738090u, "ChangeItemSkin", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(151738090u, "ChangeItemSkin", this, player))
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
							ChangeItemSkin(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ChangeItemSkin");
					}
				}
				return true;
			}
			if (rpc == 688080035 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ChangeWallpaper");
				}
				using (TimeWarning.New("ChangeWallpaper"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(688080035u, "ChangeWallpaper", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(688080035u, "ChangeWallpaper", this, player))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(688080035u, "ChangeWallpaper", this, player, 5f))
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
							ChangeWallpaper(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in ChangeWallpaper");
					}
				}
				return true;
			}
			if (rpc == 396000799 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - CreateSpray");
				}
				using (TimeWarning.New("CreateSpray"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position2 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Position = position2;
						if (!RPC_Server.IsActiveItem.Test(396000799u, "CreateSpray", this, player))
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
							CreateSpray(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in CreateSpray");
					}
				}
				return true;
			}
			if (rpc == 3288478393u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_SetEntityColour");
				}
				using (TimeWarning.New("Server_SetEntityColour"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3288478393u, "Server_SetEntityColour", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3288478393u, "Server_SetEntityColour", this, player))
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
							Server_SetEntityColour(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in Server_SetEntityColour");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(Vector3),
		typeof(int),
		typeof(int)
	})]
	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void BeginFreehandSpray(RPCMessage msg)
	{
		if (IsBusy() || !CanSprayFreehand(msg.player))
		{
			return;
		}
		Vector3 vector = msg.read.Vector3();
		Vector3 atNormal = msg.read.Vector3();
		int num = msg.read.Int32();
		int num2 = msg.read.Int32();
		if (num < 0 || num >= SprayColours.Length || num2 < 0 || num2 >= SprayWidths.Length || Vector3.Distance(vector, GetOwnerPlayer().transform.position) > 3f)
		{
			return;
		}
		SprayCanSpray_Freehand sprayCanSpray_Freehand = GameManager.server.CreateEntity(LinePrefab.resourcePath, vector, Quaternion.identity) as SprayCanSpray_Freehand;
		sprayCanSpray_Freehand.AddInitialPoint(atNormal);
		sprayCanSpray_Freehand.SetColour(SprayColours[num]);
		sprayCanSpray_Freehand.SetWidth(SprayWidths[num2]);
		sprayCanSpray_Freehand.EnableChanges(msg.player);
		sprayCanSpray_Freehand.Spawn();
		paintingLine = sprayCanSpray_Freehand;
		ClientRPC(RpcTarget.NetworkGroup("Client_ChangeSprayColour"), num);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: true);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
	}

	public void ClearPaintingLine(bool allowNewSprayImmediately)
	{
		paintingLine = null;
		if (!base.UsingInfiniteAmmoCheat)
		{
			LoseCondition(ConditionLossPerSpray);
		}
		if (allowNewSprayImmediately)
		{
			ClearBusy();
		}
		else
		{
			Invoke(ClearBusy, 0.1f);
		}
	}

	public bool CanSprayFreehand(BasePlayer player)
	{
		if (FreeSprayUnlockItem != null)
		{
			if (!player.blueprints.steamInventory.HasItem(FreeSprayUnlockItem.id))
			{
				return FreeSprayUnlockItem.HasUnlocked(player);
			}
			return true;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.CallsPerSecond(2uL)]
	private void ChangeItemSkin(RPCMessage msg)
	{
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		BasePlayer player = msg.player;
		BaseEntity baseEntity = baseNetworkable as BaseEntity;
		if ((object)baseEntity == null)
		{
			return;
		}
		LastReskinError = string.Empty;
		LastReskinErrorEntity = null;
		if (!ValidateReskin(player, baseEntity, num))
		{
			ShowLastReskinError(player);
		}
		else
		{
			if (!GetItemDefinitionForEntity(baseEntity, out var def, useRedirect: false))
			{
				return;
			}
			ulong num2 = ItemDefinition.FindSkin((def.isRedirectOf != null) ? def.isRedirectOf.itemid : def.itemid, num);
			if (Interface.CallHook("OnEntityReskin", baseEntity, num2, msg.player) != null)
			{
				return;
			}
			if (!TryFindTargetRedirect(def, num, out var targetRedirect))
			{
				baseEntity.skinID = num2;
			}
			else
			{
				if (!ValidateRedirectSwap(baseEntity, targetRedirect, player))
				{
					ShowLastReskinError(player);
					return;
				}
				if (!GetEntityPrefabPath(targetRedirect, out var resourcePath))
				{
					Debug.LogError("Cannot find resource path of redirect entity to spawn! " + targetRedirect.gameObject.name);
					return;
				}
				baseEntity = DoRedirectSwap(baseEntity, resourcePath, num2);
			}
			if (baseEntity is IReskinCallback reskinCallback)
			{
				reskinCallback.OnReskinned(player);
			}
			baseEntity.SendNetworkUpdate();
			Interface.CallHook("OnEntityReskinned", baseEntity, num2, msg.player);
			ClientRPC(RpcTarget.NetworkGroup("Client_ReskinResult"), 1, baseEntity.net.ID);
			if (!base.UsingInfiniteAmmoCheat)
			{
				LoseCondition(ConditionLossPerReskin);
			}
			Facepunch.Rust.Analytics.Azure.OnEntitySkinChanged(player, baseEntity, num);
			ClientRPC(RpcTarget.NetworkGroup("Client_ChangeSprayColour"), -1);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(actionClearBusy, SprayCooldown);
		}
	}

	private BaseEntity DoRedirectSwap(BaseEntity entity, string newResourcePath, ulong targetSkinID)
	{
		ReskinPreserveInfo preserveInfo = default(ReskinPreserveInfo);
		entity.Reskin_Preserve(ref preserveInfo);
		entity.transform.GetPositionAndRotation(out var position, out var rotation);
		entity.Kill();
		BaseEntity baseEntity = GameManager.server.CreateEntity(newResourcePath, position, rotation);
		baseEntity.Spawn();
		baseEntity.Reskin_Restore(ref preserveInfo);
		if (GetItemDefinitionForEntity(baseEntity, out var def, useRedirect: false) && def.isRedirectOf == null)
		{
			baseEntity.skinID = targetSkinID;
		}
		return baseEntity;
	}

	private bool ValidateReskin(BasePlayer player, BaseEntity targetEnt, int targetSkin)
	{
		if (IsBusy())
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (!player.IsOnGround() && !player.IsFlying)
		{
			LastReskinError = PlayerInAir;
			return false;
		}
		if (targetSkin != 0 && !player.blueprints.CheckSkinOwnership(targetSkin, player))
		{
			LastReskinError = ConstructionErrors.SkinNotOwned;
			return false;
		}
		Vector3 position = targetEnt.WorldSpaceBounds().ClosestPoint(player.eyes.position);
		if (!player.IsVisible(position, 3f))
		{
			LastReskinError = ConstructionErrors.LineOfSightBlocked;
			return false;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (player.IsBuildBlockedByMonument())
		{
			LastReskinError = CannotReskinInMonument;
			return false;
		}
		return targetEnt.CanBeReskinned(player);
	}

	private bool ValidateRedirectSwap(BaseEntity entity, ItemDefinition targetRedirect, BasePlayer player)
	{
		if (!entity.CanBeRedirectSwapped(player))
		{
			return false;
		}
		if (global::SimpleUpgrade.IsUpgradeBlocked(entity, targetRedirect, player))
		{
			if (DeployVolume.LastDeployHit != null)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(DeployVolume.LastDeployHit);
				if (baseEntity != null && !string.IsNullOrEmpty(ConstructionErrors.GetTranslatedNameFromEntity(baseEntity)))
				{
					LastReskinError = ConstructionErrors.BlockedBy;
					LastReskinErrorEntity = baseEntity;
				}
				else
				{
					LastReskinError = BlockedBySomething;
				}
			}
			else
			{
				LastReskinError = Construction.lastPlacementError;
			}
			return false;
		}
		return true;
	}

	public void ShowLastReskinError(BasePlayer player)
	{
		if (!LastReskinError.IsEmpty())
		{
			if (LastReskinErrorEntity != null)
			{
				player.ShowBlockedByEntityToast(LastReskinErrorEntity, BlockedBySomething);
			}
			else if (!string.IsNullOrEmpty(LastReskinErrorArgString))
			{
				player.ShowToast(GameTip.Styles.Error, LastReskinError, false, LastReskinErrorArgString);
			}
			else
			{
				player.ShowToast(GameTip.Styles.Error, LastReskinError, false);
			}
		}
	}

	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.MaxDistance(5f)]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void ChangeWallpaper(RPCMessage msg)
	{
		NetworkableId uid = msg.read.EntityID();
		int targetSkin = msg.read.Int32();
		int side = ((!msg.read.Bool()) ? 1 : 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
		if (baseNetworkable is BuildingBlock buildingBlock && buildingBlock.HasWallpaper(side) && ValidateWallpaperReskin(msg.player, baseNetworkable as BuildingBlock, side, targetSkin))
		{
			ulong id = ItemDefinition.FindSkin(WallpaperSettings.GetItemDefForCategory(WallpaperPlanner.Settings.GetCategory(buildingBlock, side)).itemid, targetSkin);
			buildingBlock.SetWallpaper(id, side);
			Facepunch.Rust.Analytics.Azure.OnWallpaperPlaced(msg.player, buildingBlock, id, side, reskin: true);
			ClientRPC(RpcTarget.NetworkGroup("Client_ReskinResult"), 1, buildingBlock.net.ID);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			Invoke(actionClearBusy, SprayCooldown);
		}
	}

	private bool ValidateWallpaperReskin(BasePlayer player, BuildingBlock block, int side, int targetSkin)
	{
		if (player == null || !player.CanBuild())
		{
			return false;
		}
		if (!player.IsOnGround())
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInAir, false);
			return false;
		}
		if (targetSkin != 0 && !player.blueprints.CheckSkinOwnership(targetSkin, player))
		{
			player.ShowToast(GameTip.Styles.Error, ConstructionErrors.SkinNotOwned, false);
			return false;
		}
		if (!block.HasWallpaper(side))
		{
			return false;
		}
		if (!block.CanSeeWallpaperSocket(player, side))
		{
			return false;
		}
		return true;
	}

	public static bool GetItemDefinitionForEntity(BaseEntity be, out ItemDefinition def, bool useRedirect = true)
	{
		def = null;
		if (be is BaseCombatEntity baseCombatEntity)
		{
			if (baseCombatEntity.pickup.enabled && baseCombatEntity.pickup.itemTarget != null)
			{
				def = baseCombatEntity.pickup.itemTarget;
			}
			else if (baseCombatEntity.repair.enabled && baseCombatEntity.repair.itemTarget != null)
			{
				def = baseCombatEntity.repair.itemTarget;
			}
		}
		if (be is HeldEntity heldEntity)
		{
			def = heldEntity.GetCachedItem()?.info;
		}
		if (be is CodeLock codeLock)
		{
			def = codeLock.itemType;
		}
		if (useRedirect && def != null && def.isRedirectOf != null)
		{
			def = def.isRedirectOf;
		}
		return def != null;
	}

	private bool TryFindTargetRedirect(ItemDefinition itemDef, int targetSkin, out ItemDefinition targetRedirect)
	{
		targetRedirect = null;
		if (((itemDef.isRedirectOf != null) ? itemDef.isRedirectOf : itemDef).skins.FirstOrDefault((ItemSkinDirectory.Skin x) => x.id == targetSkin).invItem is ItemSkin itemSkin)
		{
			if (itemSkin.Redirect != null)
			{
				targetRedirect = itemSkin.Redirect;
			}
			else if (itemDef.isRedirectOf != null)
			{
				targetRedirect = itemDef.isRedirectOf;
			}
		}
		else if (itemDef.isRedirectOf != null)
		{
			targetRedirect = itemDef.isRedirectOf;
		}
		return targetRedirect != null;
	}

	private bool GetEntityPrefabPath(ItemDefinition def, out string resourcePath)
	{
		resourcePath = string.Empty;
		if (def.TryGetComponent<ItemModDeployable>(out var component))
		{
			resourcePath = component.entityPrefab.resourcePath;
			return true;
		}
		if (def.TryGetComponent<ItemModEntity>(out var component2))
		{
			resourcePath = component2.entityPrefab.resourcePath;
			return true;
		}
		if (def.TryGetComponent<ItemModEntityReference>(out var component3))
		{
			resourcePath = component3.entityPrefab.resourcePath;
			return true;
		}
		return false;
	}

	private bool IsSprayBlockedByTrigger(Vector3 pos)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return true;
		}
		TriggerNoSpray triggerNoSpray = ownerPlayer.FindTrigger<TriggerNoSpray>();
		if (triggerNoSpray == null)
		{
			return false;
		}
		return !triggerNoSpray.IsPositionValid(pos);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(Vector3),
		typeof(Vector3),
		typeof(int)
	})]
	private void CreateSpray(RPCMessage msg)
	{
		if (IsBusy())
		{
			return;
		}
		ClientRPC(RpcTarget.NetworkGroup("Client_ChangeSprayColour"), -1);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		Invoke(actionClearBusy, SprayCooldown);
		Vector3 vector = msg.read.Vector3();
		Vector3 vector2 = msg.read.Vector3();
		Vector3 point = msg.read.Vector3();
		int num = msg.read.Int32();
		if (Vector3.Distance(vector, base.transform.position) > 4.5f)
		{
			return;
		}
		Quaternion quaternion = Quaternion.LookRotation((new Plane(vector2, vector).ClosestPointOnPlane(point) - vector).normalized, vector2);
		quaternion *= Quaternion.Euler(0f, 0f, 90f);
		if (num != 0 && !msg.player.blueprints.CheckSkinOwnership(num, msg.player))
		{
			Debug.Log($"SprayCan.ChangeItemSkin player does not have item :{num}:");
		}
		else
		{
			if (Interface.CallHook("OnSprayCreate", this, vector, quaternion) != null)
			{
				return;
			}
			ulong num2 = ItemDefinition.FindSkin(SprayDecalItem.itemid, num);
			BaseEntity baseEntity = GameManager.server.CreateEntity(SprayDecalEntityRef.resourcePath, vector, quaternion);
			baseEntity.skinID = num2;
			baseEntity.OnDeployed(null, GetOwnerPlayer(), GetItem());
			baseEntity.networkEntityScale = true;
			Vector3 one = Vector3.one;
			ItemSkinDirectory.Skin[] skins = SprayDecalItem.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin = skins[i];
				if ((ulong)skin.id == num2 && skin.invItem.SprayScale > 0f)
				{
					one.y = skin.invItem.SprayScale;
					one.z = skin.invItem.SprayScale;
				}
			}
			baseEntity.transform.localScale = one;
			baseEntity.Spawn();
			if (!base.UsingInfiniteAmmoCheat)
			{
				LoseCondition(ConditionLossPerSpray);
			}
		}
	}

	private void LoseCondition(float amount)
	{
		GetOwnerItem()?.LoseCondition(amount);
	}

	public void ClearBusy()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Busy, b: false);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void OnHeldChanged()
	{
		if (IsDisabled())
		{
			ClearBusy();
			if (paintingLine != null)
			{
				paintingLine.Kill();
			}
			paintingLine = null;
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.CallsPerSecond(3uL)]
	private void Server_SetEntityColour(RPCMessage msg)
	{
		NetworkableId uid = msg.read.EntityID();
		uint num = msg.read.UInt32();
		BasePlayer player = msg.player;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		Invoke(actionClearBusy, 0.1f);
		if (player == null || !player.CanBuild())
		{
			return;
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(uid) as BaseEntity;
		if (!(baseEntity != null) || Vector3.SqrMagnitude(player.transform.position - baseEntity.transform.position) > 16f)
		{
			return;
		}
		if (baseEntity is BuildingBlock { customColour: var customColour } buildingBlock)
		{
			buildingBlock.SetCustomColour(num);
			Facepunch.Rust.Analytics.Azure.OnEntityColorChanged(player, buildingBlock, customColour, num);
			return;
		}
		int i = 0;
		for (int count = baseEntity.Components.Count; i < count; i++)
		{
			EntityComponentBase entityComponentBase = baseEntity.Components[i];
			if (entityComponentBase is SprayCanColorChangeEntityComponent { currentColorIndex: var currentColorIndex } sprayCanColorChangeEntityComponent)
			{
				sprayCanColorChangeEntityComponent.Server_UpdateColor(num);
				Facepunch.Rust.Analytics.Azure.OnEntityColorChanged(player, entityComponentBase.GetBaseEntity(), currentColorIndex, num);
				break;
			}
		}
	}
}
