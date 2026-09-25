using System.Collections.Generic;
using System.Text;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace ConVar;

[Factory("player")]
public class Player : ConsoleSystem
{
	public const string serverTickRateDefaultString = "16";

	public static int serverTickRate = 16;

	public const int serverTickRateDefault = 16;

	public const int serverTickRateMin = 16;

	public const int serverTickRateMax = 128;

	public static float serverTickInterval = 0.0625f;

	public const string clientTickRateDefaultString = "32";

	public const int clientTickRateDefault = 32;

	public const int clientTickRateMin = 16;

	public const int clientTickRateMax = 128;

	public static EncryptedValue<int> clientTickRate = 32;

	public static EncryptedValue<float> clientTickInterval = 1f / 32f;

	private static bool _infiniteAmmo = false;

	[ServerVar(Help = "(Generated) When enabled, tea/buff effects active on a player at the time of death are carried over to their next life instead of being lost")]
	public static bool keepteaondeath = false;

	[ServerVar(Help = "(Generated) When enabled, players drop their backpack as a loot bag when they die; disable to prevent backpack loot from appearing on death")]
	public static bool dropbackpackondeath = true;

	[ServerVar(Help = "(Generated) When enabled, players drop their backpack when downed/wounded; disable to keep the backpack on the body until death or recovery")]
	public static bool dropbackpackondowned = true;

	[ServerVar(Help = "Should admins be allowed to loot incapacitated players (or their corpse / bag) in safe-zones?")]
	public static bool adminsafezonelooting = false;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Whether the crawling state expires")]
	public static bool woundforever = false;

	[ReplicatedVar(Default = "16")]
	public static int tickrate_sv
	{
		get
		{
			return serverTickRate;
		}
		set
		{
			serverTickRate = Mathf.Clamp(value, 16, 128);
			serverTickInterval = 1f / (float)serverTickRate;
		}
	}

	[ReplicatedVar(Default = "32")]
	public static int tickrate_cl
	{
		get
		{
			return clientTickRate;
		}
		set
		{
			clientTickRate = Mathf.Clamp(value, 16, 128);
			clientTickInterval = 1f / (float)(int)clientTickRate;
		}
	}

	[ClientVar(ClientInfo = true, Help = "(Generated) When enabled, the local player has unlimited ammo and never needs to reload; intended for testing and cinematic use only")]
	public static bool InfiniteAmmo
	{
		get
		{
			return _infiniteAmmo;
		}
		set
		{
			_infiniteAmmo = value;
		}
	}

	[ServerUserVar]
	[ClientVar(AllowRunFromServer = true)]
	public static void cinematic_play(Arg arg)
	{
		if (!arg.HasArgs() || !arg.IsServerside)
		{
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			string strCommand = string.Empty;
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
			{
				strCommand = arg.cmd.FullName + " " + arg.FullString.ToString() + " " + basePlayer.UserIDString;
			}
			else if (Server.cinematic)
			{
				strCommand = arg.cmd.FullName + " " + arg.GetString(0) + " " + basePlayer.UserIDString;
			}
			if (Server.cinematic)
			{
				ConsoleNetwork.BroadcastToAllClients(strCommand);
			}
			else if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
			{
				ConsoleNetwork.SendClientCommand(arg.Connection, strCommand);
			}
		}
	}

	[ClientVar(AllowRunFromServer = true)]
	[ServerUserVar]
	public static void cinematic_stop(Arg arg)
	{
		if (!arg.IsServerside)
		{
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			string strCommand = string.Empty;
			if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
			{
				strCommand = arg.cmd.FullName + " " + arg.FullString.ToString() + " " + basePlayer.UserIDString;
			}
			else if (Server.cinematic)
			{
				strCommand = arg.cmd.FullName + " " + basePlayer.UserIDString;
			}
			if (Server.cinematic)
			{
				ConsoleNetwork.BroadcastToAllClients(strCommand);
			}
			else if (basePlayer.IsAdmin || basePlayer.IsDeveloper)
			{
				ConsoleNetwork.SendClientCommand(arg.Connection, strCommand);
			}
		}
	}

	[ServerUserVar]
	public static void cinematic_gesture(Arg arg)
	{
		if (Server.cinematic)
		{
			string @string = arg.GetString(0);
			BasePlayer basePlayer = ArgEx.GetPlayer(arg, 1);
			if (basePlayer == null)
			{
				basePlayer = ArgEx.Player(arg);
			}
			basePlayer.UpdateActiveItem(default(ItemId));
			basePlayer.SignalBroadcast(BaseEntity.Signal.Gesture, @string);
		}
	}

	[ServerUserVar]
	public static void copyrotation(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
		{
			uint uInt = arg.GetUInt(0);
			BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
			if (basePlayer2 == null)
			{
				basePlayer2 = BasePlayer.FindBot(uInt);
			}
			if (basePlayer2 != null)
			{
				basePlayer2.CopyRotation(basePlayer);
				Debug.Log("Copied rotation of " + basePlayer2.UserIDString);
			}
		}
	}

	[ServerUserVar]
	public static void abandonmission(Arg arg)
	{
		ArgEx.Player(arg).AbandonActiveMission();
	}

	[ServerUserVar]
	public static void mount(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
		{
			return;
		}
		uint uInt = arg.GetUInt(0);
		BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
		if (basePlayer2 == null)
		{
			basePlayer2 = BasePlayer.FindBot(uInt);
		}
		if (!basePlayer2 || !UnityEngine.Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), out var hitInfo, 5f, 10496, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
		if (!entity)
		{
			return;
		}
		BaseMountable baseMountable = entity.GetComponent<BaseMountable>();
		if (!baseMountable)
		{
			BaseVehicle baseVehicle = entity.GetComponentInParent<BaseVehicle>();
			if ((bool)baseVehicle)
			{
				if (!baseVehicle.isServer)
				{
					baseVehicle = BaseNetworkable.serverEntities.Find(baseVehicle.net.ID) as BaseVehicle;
				}
				baseVehicle.AttemptMount(basePlayer2);
				return;
			}
		}
		if ((bool)baseMountable && !baseMountable.isServer)
		{
			baseMountable = BaseNetworkable.serverEntities.Find(baseMountable.net.ID) as BaseMountable;
		}
		if ((bool)baseMountable)
		{
			baseMountable.AttemptMount(basePlayer2);
		}
	}

	[ServerVar(Help = "(Generated) Forces the specified sleeping player (by ID) to enter the sleep state; admin/developer/cinematic mode only; also works on bots")]
	public static void gotosleep(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
		{
			return;
		}
		uint uInt = arg.GetUInt(0);
		BasePlayer basePlayer2 = BasePlayer.FindSleeping(uInt.ToString());
		if (!basePlayer2)
		{
			basePlayer2 = BasePlayer.FindBotClosestMatch(uInt.ToString());
			if (basePlayer2.IsSleeping())
			{
				basePlayer2 = null;
			}
		}
		if ((bool)basePlayer2)
		{
			basePlayer2.StartSleeping();
		}
	}

	[ServerVar(Help = "optional param {player}", ClientAdmin = true)]
	public static void ragdoll(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
		{
			BasePlayer basePlayer2 = ArgEx.GetPlayerOrSleeperOrBot(arg, 0) ?? basePlayer;
			if (!(basePlayer2 == null))
			{
				basePlayer2.Ragdoll();
			}
		}
	}

	[ServerVar(Help = "Ragdolls a player you're looking at", ClientAdmin = true)]
	public static void ragdollother(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic) && GamePhysics.Trace(basePlayer.eyes.HeadRay(), 0.5f, out var hitInfo, 5f, 1218652417, QueryTriggerInteraction.UseGlobal, basePlayer) && RaycastHitEx.GetEntity(hitInfo) is BasePlayer { isClient: false } basePlayer2)
		{
			basePlayer2.Ragdoll();
		}
	}

	[ServerVar(Help = "ragdolls")]
	public static void ragdollall(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
		{
			return;
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			activePlayer.Ragdoll();
		}
		foreach (BasePlayer bot in BasePlayer.bots)
		{
			bot.Ragdoll();
		}
	}

	[ServerVar(Help = "ragdolls everyone except player")]
	public static void ragdollallbutme(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
		{
			return;
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == basePlayer))
			{
				activePlayer.Ragdoll();
			}
		}
		foreach (BasePlayer bot in BasePlayer.bots)
		{
			bot.Ragdoll();
		}
	}

	[ServerVar(Help = "(Generated) Forces the specified player (by ID) to dismount from any vehicle or mountable they are currently seated in; admin/developer/cinematic mode only")]
	public static void dismount(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
		{
			uint uInt = arg.GetUInt(0);
			BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
			if (basePlayer2 == null)
			{
				basePlayer2 = BasePlayer.FindBot(uInt);
			}
			if ((bool)basePlayer2 && (bool)basePlayer2 && basePlayer2.isMounted)
			{
				basePlayer2.GetMounted().DismountPlayer(basePlayer2);
			}
		}
	}

	[ServerVar(Help = "(Generated) Moves the specified player (by ID) to the given seat index on the vehicle they are mounted in; admin/developer/cinematic mode only")]
	public static void swapseat(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
		{
			return;
		}
		uint uInt = arg.GetUInt(0);
		BasePlayer basePlayer2 = BasePlayer.FindByID(uInt);
		if (basePlayer2 == null)
		{
			basePlayer2 = BasePlayer.FindBot(uInt);
		}
		if ((bool)basePlayer2)
		{
			int @int = arg.GetInt(1);
			if ((bool)basePlayer2 && basePlayer2.isMounted && (bool)basePlayer2.GetMounted().VehicleParent())
			{
				basePlayer2.GetMounted().VehicleParent().SwapSeats(basePlayer2, @int);
			}
		}
	}

	[ServerVar(Help = "(Generated) Wakes up the specified sleeping player (by ID), ending their sleep state immediately; admin/developer/cinematic mode only")]
	public static void wakeup(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic)
		{
			BasePlayer basePlayer2 = BasePlayer.FindSleeping(arg.GetUInt(0).ToString());
			if ((bool)basePlayer2)
			{
				basePlayer2.EndSleeping();
			}
		}
	}

	[ServerVar(Help = "(Generated) Wakes up all sleeping players on the server at once; admin/developer/cinematic mode only; useful for clearing the sleeping player list after a wipe")]
	public static void wakeupall(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic)
		{
			return;
		}
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
		{
			obj.Add(sleepingPlayer);
		}
		foreach (BasePlayer item in obj)
		{
			item.EndSleeping();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Prints detailed life-story stats for the calling player including time alive, distances travelled, damage taken/healed, kills, and per-weapon accuracy")]
	public static void printstats(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsAlive:F1}s alive");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsSleeping:F1}s sleeping");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsSwimming:F1}s swimming");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsInBase:F1}s in base");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsWilderness:F1}s in wilderness");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsInMonument:F1}s in monuments");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsFlying:F1}s flying");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsBoating:F1}s boating");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.secondsDriving:F1}s driving");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.metersRun:F1}m run");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.metersWalked:F1}m walked");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.totalDamageTaken:F1} damage taken");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.totalHealing:F1} damage healed");
		stringBuilder.AppendLine("===");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.killedPlayers} other players killed");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.killedScientists} scientists killed");
		stringBuilder.AppendLine($"{basePlayer.lifeStory.killedAnimals} animals killed");
		stringBuilder.AppendLine("===");
		stringBuilder.AppendLine("Weapon stats:");
		if (basePlayer.lifeStory.weaponStats != null)
		{
			foreach (PlayerLifeStory.WeaponStats weaponStat in basePlayer.lifeStory.weaponStats)
			{
				float num = (float)weaponStat.shotsHit / (float)weaponStat.shotsFired;
				num *= 100f;
				stringBuilder.AppendLine($"{weaponStat.weaponName} - shots fired: {weaponStat.shotsFired} shots hit: {weaponStat.shotsHit} accuracy: {num:F1}%");
			}
		}
		stringBuilder.AppendLine("===");
		stringBuilder.AppendLine("Misc stats:");
		if (basePlayer.lifeStory.genericStats != null)
		{
			foreach (PlayerLifeStory.GenericStat genericStat in basePlayer.lifeStory.genericStats)
			{
				stringBuilder.AppendLine($"{genericStat.key} = {genericStat.value}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Prints the calling player's current time-category presence flags: whether they are in wilderness, base, monument, swimming, boating, or flying")]
	public static void printpresence(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		bool flag = (basePlayer.currentTimeCategory & 1) != 0;
		bool flag2 = (basePlayer.currentTimeCategory & 4) != 0;
		bool flag3 = (basePlayer.currentTimeCategory & 2) != 0;
		bool flag4 = (basePlayer.currentTimeCategory & 0x20) != 0;
		bool flag5 = (basePlayer.currentTimeCategory & 0x10) != 0;
		bool flag6 = (basePlayer.currentTimeCategory & 8) != 0;
		arg.ReplyWith($"Wilderness:{flag} Base:{flag2} Monument:{flag3} Swimming: {flag4} Boating: {flag5} Flying: {flag6}");
	}

	[ServerVar(Help = "Resets the PlayerState of the given player")]
	public static void resetstate(Arg args)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(args, 0);
		if (playerOrSleeper == null)
		{
			args.ReplyWith("Player not found");
			return;
		}
		playerOrSleeper.ResetPlayerState();
		args.ReplyWith("Player state reset");
	}

	[ServerVar(Help = "Resets the saved missions progress of the given player")]
	public static void resetmissions(Arg args)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(args, 0);
		if (playerOrSleeper == null)
		{
			args.ReplyWith("Player not found");
		}
		else
		{
			playerOrSleeper.WipeMissions(saveImmediately: true);
		}
	}

	[ServerVar(Help = "Resets the saved missions progress of all player states on this server (online and offline players). Must be entered as \"resetmissions_all Y\" to execute.")]
	public static void resetmissions_all(Arg args)
	{
		string @string = args.GetString(0);
		if (@string != "Y" && @string != "y")
		{
			args.ReplyWith("Please input the command as \"resetmissions_all Y\" to execute.");
			return;
		}
		for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
		{
			BasePlayer basePlayer = BasePlayer.activePlayerList[i];
			if (!(basePlayer == null))
			{
				basePlayer.WipeMissions(saveImmediately: true);
			}
		}
		for (int j = 0; j < BasePlayer.sleepingPlayerList.Count; j++)
		{
			BasePlayer basePlayer2 = BasePlayer.sleepingPlayerList[j];
			if (!(basePlayer2 == null))
			{
				basePlayer2.WipeMissions(saveImmediately: true);
			}
		}
		foreach (ulong allPlayerID in SingletonComponent<ServerMgr>.Instance.persistance.GetAllPlayerIDs())
		{
			PlayerState playerState = SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(allPlayerID);
			if (BasePlayer.FindAwakeOrSleepingByID(allPlayerID) == null)
			{
				playerState?.missions?.Dispose();
				SingletonComponent<ServerMgr>.Instance.playerStateManager.SaveState(allPlayerID, playerState);
			}
		}
	}

	[ServerVar(Help = "<fresh/salt/rads> - Fills up liquid container items in your hotbar as well as any liquid containers you are looking at")]
	public static void fillwater(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		string @string = arg.GetString(0);
		ItemDefinition itemDefinition = ((@string == "salt") ? WaterTypes.SaltWaterItemDef : ((!(@string == "rads")) ? WaterTypes.WaterItemDef : WaterTypes.RadioactiveWaterItemDef));
		ItemDefinition itemDefinition2 = itemDefinition;
		int num = 0;
		for (int i = 0; i < PlayerBelt.MaxBeltSlots; i++)
		{
			Item itemInSlot = basePlayer.Belt.GetItemInSlot(i);
			if (itemInSlot != null && itemInSlot.GetHeldEntity() is BaseLiquidVessel baseLiquidVessel && (baseLiquidVessel.hasLid || !basePlayer.GetHeldEntity() != (bool)baseLiquidVessel))
			{
				int amount = 999;
				if (itemInSlot.info.TryGetComponent<ItemModContainer>(out var component))
				{
					amount = component.maxStackSize;
				}
				itemInSlot.contents.Clear();
				baseLiquidVessel.AddLiquid(itemDefinition2, amount);
				num++;
			}
		}
		string text = $"Filled {num} items in the hotbar";
		if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 3f, 1218652417) is LiquidContainer liquidContainer)
		{
			liquidContainer.inventory.Clear();
			Item item = ItemManager.Create(itemDefinition2, liquidContainer.inventory.maxStackSize, 0uL, isServerSide: true, 0uL);
			if (liquidContainer.inventory.GiveItem(item))
			{
				text = text + "\nFilled up the " + liquidContainer.ShortPrefabName;
			}
			else
			{
				item.Remove();
				text = text + "\nThe " + liquidContainer.ShortPrefabName + " does not accept this water type";
			}
		}
		arg.ReplyWith(text);
	}

	[ServerVar(ServerAdmin = true, Help = "(Generated) Fully reloads all projectile weapons, flamethrowers, and liquid weapons in every belt slot of the calling player; useful for testing without consuming ammo")]
	public static void reloadweapons(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		for (int i = 0; i < PlayerBelt.MaxBeltSlots; i++)
		{
			Item itemInSlot = basePlayer.Belt.GetItemInSlot(i);
			if (itemInSlot == null)
			{
				continue;
			}
			if (itemInSlot.GetHeldEntity() is BaseProjectile baseProjectile)
			{
				if (baseProjectile.primaryMagazine != null)
				{
					baseProjectile.SetAmmoCount(baseProjectile.primaryMagazine.capacity);
					baseProjectile.SendNetworkUpdateImmediate();
				}
			}
			else if (itemInSlot.GetHeldEntity() is FlameThrower flameThrower)
			{
				flameThrower.ammo = flameThrower.maxAmmo;
				flameThrower.SendNetworkUpdateImmediate();
			}
			else if (itemInSlot.GetHeldEntity() is LiquidWeapon liquidWeapon)
			{
				liquidWeapon.AddLiquid(WaterTypes.WaterItemDef, 999);
			}
		}
	}

	[ServerVar(ServerAdmin = true, Help = "(Generated) Empties the ammo from all projectile weapons, flamethrowers, and liquid weapons in every belt slot of the calling player; useful for testing reload behaviour")]
	public static void unloadweapons(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		for (int i = 0; i < PlayerBelt.MaxBeltSlots; i++)
		{
			Item itemInSlot = basePlayer.Belt.GetItemInSlot(i);
			if (itemInSlot == null)
			{
				continue;
			}
			if (itemInSlot.GetHeldEntity() is BaseProjectile baseProjectile)
			{
				if (baseProjectile.primaryMagazine != null)
				{
					baseProjectile.SetAmmoCount(0);
					baseProjectile.SendNetworkUpdateImmediate();
				}
			}
			else if (itemInSlot.GetHeldEntity() is FlameThrower flameThrower)
			{
				flameThrower.ammo = 0;
				flameThrower.SendNetworkUpdateImmediate();
			}
			else if (itemInSlot.GetHeldEntity() is LiquidWeapon liquidWeapon)
			{
				liquidWeapon.LoseWater(999);
			}
		}
	}

	[ServerVar(Help = "(Generated) Creates a human skull item named after the given player name (or a random name if none given) and gives it to the calling player's inventory")]
	public static void createskull(Arg arg)
	{
		string text = arg.GetString(0);
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (string.IsNullOrEmpty(text))
		{
			text = RandomUsernames.Get(Random.Range(0, 1000));
		}
		Item item = ItemManager.Create(ItemManager.FindItemDefinition("skull.human"), 1, 0uL, isServerSide: true, 0uL);
		item.name = HumanBodyResourceDispenser.CreateSkullName(text);
		item.streamerName = item.name;
		basePlayer.inventory.GiveItem(item);
	}

	[ServerVar(Help = "(Generated) Creates a trophy head bag item for the specified entity type (by prefab name) and gives it to the calling player; used to generate mount-style trophy items for testing")]
	public static string createTrophy(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		Entity.EntitySpawnRequest spawnEntityFromName = Entity.GetSpawnEntityFromName(arg.GetString(0));
		if (!spawnEntityFromName.Valid)
		{
			return spawnEntityFromName.Error;
		}
		if (GameManager.server.FindPrefab(spawnEntityFromName.PrefabName).TryGetComponent<BaseCombatEntity>(out var component))
		{
			Item item = ItemManager.CreateByName("head.bag", 1, 0uL);
			HeadEntity associatedEntity = ItemModAssociatedEntity<HeadEntity>.GetAssociatedEntity(item);
			if (associatedEntity != null)
			{
				associatedEntity.SetupSourceId(component.prefabID);
			}
			if (basePlayer.inventory.GiveItem(item))
			{
				basePlayer.Command("note.inv", item.info.itemid, 1);
			}
			else
			{
				item.DropAndTossUpwards(basePlayer.eyes.position);
			}
		}
		return "Created head";
	}

	[ServerVar(Help = "(Generated) Admin-only: triggers the trap-think logic on the wildlife trap the calling player is looking at within 5 metres, simulating a catch attempt for testing")]
	public static void trigger_wildlife_trap(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsAdmin)
		{
			return;
		}
		if (GamePhysics.Trace(basePlayer.eyes.HeadRay(), 0.5f, out var hitInfo, 5f, 1218652417, QueryTriggerInteraction.UseGlobal, basePlayer))
		{
			WildlifeTrap wildlifeTrap = RaycastHitEx.GetEntity(hitInfo) as WildlifeTrap;
			if ((object)wildlifeTrap != null)
			{
				if (wildlifeTrap.isClient)
				{
					wildlifeTrap = BaseNetworkable.serverEntities.Find(wildlifeTrap.net.ID) as WildlifeTrap;
				}
				if (!wildlifeTrap.IsTrapActive())
				{
					arg.ReplyWith("Trap is not loaded or active");
					return;
				}
				wildlifeTrap.TrapThink();
				arg.ReplyWith("Trap think triggered");
				return;
			}
		}
		arg.ReplyWith("Not looking at a trap");
	}

	[ServerVar(Help = "(Generated) Forces all players within a given radius (including the caller) to play one of the specified gesture names chosen at random; admin only; args: radius gesture1 [gesture2...]")]
	public static void gesture_radius(Arg arg)
	{
		gesture_radius(arg, includeMe: true);
	}

	[ServerVar(Help = "(Generated) Same as gesture_radius but excludes the calling admin from the gesture; forces all other players within the radius to perform a random gesture from the provided list")]
	public static void gesture_radius_notme(Arg arg)
	{
		gesture_radius(arg, includeMe: false);
	}

	public static void gesture_radius(Arg arg, bool includeMe)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsAdmin)
		{
			return;
		}
		float @float = arg.GetFloat(0);
		List<string> obj = Facepunch.Pool.Get<List<string>>();
		for (int i = 0; i < 5; i++)
		{
			if (!string.IsNullOrEmpty(arg.GetString(i + 1)))
			{
				obj.Add(arg.GetString(i + 1));
			}
		}
		if (obj.Count == 0)
		{
			arg.ReplyWith("No gestures provided. eg. player.gesture_radius 10f cabbagepatch raiseroof");
			Facepunch.Pool.FreeUnmanaged(ref obj);
			return;
		}
		List<BasePlayer> obj2 = Facepunch.Pool.Get<List<BasePlayer>>();
		global::Vis.Entities(basePlayer.transform.position, @float, obj2, 131072);
		foreach (BasePlayer item in obj2)
		{
			if (includeMe || (!(item == basePlayer) && !item.isClient))
			{
				GestureConfig toPlay = GestureCollection.Instance.GestureConvarNameToGesture(obj[Random.Range(0, obj.Count)]);
				item.Server_StartGesture(toPlay, BasePlayer.GestureStartSource.Player, bypassOwnershipCheck: true);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Cancels any active gesture on all players within the specified radius of the calling admin; admin only; useful for stopping mass-gesture cinematics")]
	public static void stopgesture_radius(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsAdmin)
		{
			return;
		}
		float @float = arg.GetFloat(0);
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		global::Vis.Entities(basePlayer.transform.position, @float, obj, 131072);
		foreach (BasePlayer item in obj)
		{
			item.Server_CancelGesture();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Marks the calling player as hostile immediately, triggering the hostile timer as if they had attacked another player; useful for testing hostile-state dependent behaviour")]
	public static void markhostile(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer != null)
		{
			basePlayer.MarkHostileFor();
		}
	}

	[ServerVar(Help = "Clear your hostile flag")]
	public static void clearhostile(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer != null)
		{
			basePlayer.SetHostileDuration(0f);
		}
	}
}
