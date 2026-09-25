using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Development.Attributes;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Unity;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace ConVar;

[ResetStaticFields]
[Factory("debug")]
public class Debugging : ConsoleSystem
{
	private const string NO_RECOVER_ARG = "--no-recover";

	[ServerVar(Help = "(Generated) When enabled, validates trigger collider configurations each physics update to catch incorrectly parented or sized trigger volumes")]
	[ClientVar(Help = "(Generated) When enabled, validates trigger collider configurations each physics update to catch incorrectly parented or sized trigger volumes")]
	public static bool checktriggers = false;

	[ServerVar(Help = "(Generated) When enabled, validates that trigger colliders are correctly parented to their entities during physics updates; helps catch mis-parenting bugs")]
	public static bool checkparentingtriggers = true;

	[ClientVar(Saved = false, Help = "Shows some debug info for dismount attempts.")]
	[ServerVar]
	public static bool DebugDismounts = false;

	[ClientVar(ClientAdmin = true, Saved = false, Help = "Duration in seconds to keep ddraw for dismount attempts visible")]
	public static float DebugDismountDuration = 30f;

	[ServerVar(Help = "Shows debug info for what objects are causing clipping checks to fail.")]
	public static bool DebugClippingChecks = false;

	[ServerVar(Help = "Do not damage any items")]
	public static bool disablecondition = false;

	[ServerVar(Help = "(Generated) Minimum seconds that must pass after a tutorial ends before another one can start; prevents back-to-back tutorial spam")]
	public static int tutorial_start_cooldown = 60;

	[ServerVar(Help = "(Generated) When enabled, logs mission NPC speech info (speaker, line, trigger) to the console as mission dialogue events fire")]
	public static bool printMissionSpeakInfo = false;

	[ServerVar(Help = "(Generated) Multiplier applied to all puzzle reset timers; values below 1.0 make puzzles reset faster, above 1.0 slower")]
	public static float puzzleResetTimeMultiplier = 1f;

	[ServerVar(Help = "Whether to parent players immediately on spawning to a boat if the bag is on a boat")]
	public static bool bag_respawn_parenting = true;

	[ServerVar(Help = "(Generated) When true, nav mesh obstacle components on loot containers are disabled in the deep sea zone to improve performance in underwater areas")]
	public static bool disableLootNavObstaclesInDeepSea = true;

	[ServerVar(Help = "(Generated) When enabled, logs debug information about object callback invocations to the console; useful for tracing event callback chains")]
	[ClientVar(Help = "(Generated) When enabled, logs debug information about object callback invocations to the console; useful for tracing event callback chains")]
	public static bool callbacks = false;

	[ClientVar(Help = "(Generated) When enabled, Unity Debug.Log output is written to disk; disabling first logs a final message before suppressing further output")]
	[ServerVar(Help = "(Generated) When enabled, Unity Debug.Log output is written to disk; disabling first logs a final message before suppressing further output")]
	public static bool log
	{
		get
		{
			return UnityEngine.Debug.unityLogger.logEnabled;
		}
		set
		{
			if (!value)
			{
				UnityEngine.Debug.Log("Logging disabled");
			}
			UnityEngine.Debug.unityLogger.logEnabled = value;
			if (value)
			{
				UnityEngine.Debug.Log("Logging enabled");
			}
		}
	}

	[ClientVar(ClientAdmin = true)]
	[ServerVar(Help = "(Generated) Generates and logs a render info report showing draw calls, batch counts, triangle counts, and shadow caster counts for the current frame")]
	public static void renderinfo(Arg arg)
	{
		RenderInfo.GenerateReport();
	}

	[ServerVar(Help = "(Generated) Sends a client RPC to the target player enabling or disabling their movement controls; admin only; useful for testing freeze/lock mechanics")]
	public static void enable_player_movement(Arg arg)
	{
		if (arg.IsAdmin)
		{
			bool @bool = arg.GetBool(0, def: true);
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer == null)
			{
				arg.ReplyWith("Must be called from client with player model");
				return;
			}
			basePlayer.ClientRPC(RpcTarget.Player("TogglePlayerMovement", basePlayer), @bool);
			arg.ReplyWith((@bool ? "enabled" : "disabled") + " player movement");
		}
	}

	[ServerVar(Help = "(Generated) Logs a configurable number of test messages of a given length; used to stress-test console/logging performance and measure output speed")]
	public static void console_spam(Arg arg)
	{
		int num = Mathf.Clamp(arg.GetInt(0, 100), 1, 100000);
		int @int = arg.GetInt(1, 50);
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		for (int i = 0; i < num; i++)
		{
			UnityEngine.Debug.Log(new string((char)(97 + i % 26), @int));
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log($"Took {stopwatch.ElapsedMilliseconds}ms to log {num} lines");
	}

	[ServerVar(Help = "(Generated) Prints a message to the server console using the specified ConsoleColor index; useful for testing coloured console output")]
	public static void console_print_color(Arg arg)
	{
		string @string = arg.GetString(0, "This is a test colored message");
		int @int = arg.GetInt(1, 2);
		ServerConsole.PrintColoured(@string, (ConsoleColor)@int);
	}

	[ServerVar(Help = "(Generated) Stalls the main thread for the given duration in seconds (clamped 0-1); admin-only; used to test timeout handling and watchdog systems")]
	[ClientVar(Help = "(Generated) Stalls the main thread for the given duration in seconds (clamped 0-1); admin-only; used to test timeout handling and watchdog systems")]
	public static void stall(Arg arg)
	{
		float num = Mathf.Clamp(arg.GetFloat(0), 0f, 1f);
		arg.ReplyWith("Stalling for " + num + " seconds...");
		Thread.Sleep(Mathf.RoundToInt(num * 1000f));
	}

	[ClientVar(ClientAdmin = true, Help = "Intentionally crash the process to verify the native crash handler. Optional category: accessviolation (default), fatalerror, abort, purevirtual, monoabort")]
	public static void forcecrash(Arg arg)
	{
		string @string = arg.GetString(0, "accessviolation");
		ForcedCrashCategory forcedCrashCategory;
		switch (@string.ToLowerInvariant())
		{
		case "accessviolation":
			forcedCrashCategory = ForcedCrashCategory.AccessViolation;
			break;
		case "fatalerror":
			forcedCrashCategory = ForcedCrashCategory.FatalError;
			break;
		case "abort":
			forcedCrashCategory = ForcedCrashCategory.Abort;
			break;
		case "purevirtual":
			forcedCrashCategory = ForcedCrashCategory.PureVirtualFunction;
			break;
		case "monoabort":
			forcedCrashCategory = ForcedCrashCategory.MonoAbort;
			break;
		default:
			arg.ReplyWith("Unknown crash category \"" + @string + "\" - use accessviolation, fatalerror, abort, purevirtual or monoabort");
			return;
		}
		UnityEngine.Debug.LogWarning($"debug.forcecrash - intentionally crashing this process with {forcedCrashCategory}");
		Utils.ForceCrash(forcedCrashCategory);
	}

	[ServerVar(Help = "Repair all items in inventory")]
	public static void repair_inventory(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer)
		{
			return;
		}
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		basePlayer.inventory.GetAllItems(obj);
		foreach (Item item in obj)
		{
			if (item != null)
			{
				item.maxCondition = item.info.condition.max;
				item.condition = item.maxCondition;
				item.MarkDirty();
			}
			if (item.contents == null)
			{
				continue;
			}
			foreach (Item item2 in item.contents.itemList)
			{
				item2.maxCondition = item2.info.condition.max;
				item2.condition = item2.maxCondition;
				item2.MarkDirty();
			}
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
	}

	[ServerVar(Help = "(Generated) Spawns a clone of the calling player at a configurable height with a parachute deployed and their belt and wear inventories copied")]
	public static void spawnParachuteTester(Arg arg)
	{
		float @float = arg.GetFloat(0, 50f);
		BasePlayer basePlayer = ArgEx.Player(arg);
		BasePlayer basePlayer2 = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", basePlayer.transform.position + Vector3.up * @float, Quaternion.LookRotation(basePlayer.eyes.BodyForward())) as BasePlayer;
		basePlayer2.Spawn();
		basePlayer2.eyes.rotation = basePlayer.eyes.rotation;
		basePlayer2.SendNetworkUpdate();
		Inventory.copyTo(basePlayer, basePlayer2);
		if (!basePlayer2.HasValidParachuteEquipped())
		{
			basePlayer2.inventory.containerWear.GiveItem(ItemManager.CreateByName("parachute", 1, 0uL));
		}
		basePlayer2.RequestParachuteDeploy();
	}

	[ServerVar(Help = "(Generated) Triggers the tutorial island ending cinematic for the calling player; spawns a kayak at the designated mount point and mounts the player to it")]
	public static string testTutorialCinematic(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsInTutorial)
		{
			return "Requires a player";
		}
		TutorialIsland currentTutorialIsland = basePlayer.GetCurrentTutorialIsland();
		if (currentTutorialIsland == null)
		{
			return "Invalid island";
		}
		Transform transform = currentTutorialIsland.transform.FindChildRecursive("KayakMissionPoint");
		if (transform == null)
		{
			return "Can't find KayakMissionPoint on island";
		}
		Kayak obj = GameManager.server.CreateEntity("assets/content/vehicles/boats/kayak/kayak.prefab", transform.position, transform.rotation) as Kayak;
		obj.Spawn();
		obj.WantsMount(basePlayer);
		currentTutorialIsland.StartEndingCinematic(basePlayer);
		return "Playing cinematic";
	}

	[ServerVar(Help = "If a player ends up stuck on a tutorial for any reason this will clear the island and reset the player (will also kill player)")]
	public static void clearTutorialForPlayer(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (player == null)
		{
			arg.ReplyWith("Please provide a player");
		}
		else if (player.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = player.GetCurrentTutorialIsland();
			if (currentTutorialIsland != null)
			{
				currentTutorialIsland.Return();
			}
			player.ClearTutorial();
			player.Hurt(99999f);
			player.ClearTutorial_PostDeath();
		}
	}

	[ServerVar(Help = "<shortname> (optional: <radius>) - Delete entities with the given short prefab name")]
	public static void deleteEntitiesByShortname(Arg arg)
	{
		string text = arg.GetString(0).ToLower();
		float @float = arg.GetFloat(1);
		BasePlayer basePlayer = ArgEx.Player(arg);
		using PooledList<BaseNetworkable> pooledList = Facepunch.Pool.Get<PooledList<BaseNetworkable>>();
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity.ShortPrefabName == text && (@float == 0f || (basePlayer != null && basePlayer.Distance(serverEntity as BaseEntity) <= @float)))
			{
				pooledList.Add(serverEntity);
			}
		}
		if (CollectionEx.IsEmpty(pooledList))
		{
			arg.ReplyWith("Did not find any " + text);
			return;
		}
		arg.ReplyWith($"Deleting {pooledList.Count} {text}");
		foreach (BaseNetworkable item in pooledList)
		{
			item.Kill();
		}
	}

	[ServerVar(Help = "Delete entities by id. Supports multiple arguments")]
	public static void deleteEntityById(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < arg.Args.Length; i++)
		{
			NetworkableId entityID = ArgEx.GetEntityID(arg, i);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
			if (baseNetworkable != null)
			{
				stringBuilder.AppendLine($"Deleting {baseNetworkable}");
				baseNetworkable.Kill();
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	private static MonumentBlocker FindClosestMonumentBlocker(Arg arg, float range, out float closestDistance)
	{
		closestDistance = float.MaxValue;
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("This command can only be run by a player");
			return null;
		}
		using PooledList<MonumentBlocker> pooledList = Facepunch.Pool.Get<PooledList<MonumentBlocker>>();
		global::Vis.Entities(basePlayer.transform.position, range, pooledList);
		MonumentBlocker monumentBlocker = null;
		foreach (MonumentBlocker item in pooledList)
		{
			if (item.isServer)
			{
				float num = item.Distance(basePlayer.transform.position);
				if (!(num >= closestDistance))
				{
					monumentBlocker = item;
					closestDistance = num;
				}
			}
		}
		if (monumentBlocker == null)
		{
			arg.ReplyWith($"No monument blocker found within {range}m");
		}
		return monumentBlocker;
	}

	[ServerVar(Help = "Prints the health and decay state of the monument blocker closest to you. Optional argument: search range in metres (default 100)")]
	public static void printmonumentblocker(Arg arg)
	{
		float @float = arg.GetFloat(0, 100f);
		float closestDistance;
		MonumentBlocker monumentBlocker = FindClosestMonumentBlocker(arg, @float, out closestDistance);
		if (!(monumentBlocker == null))
		{
			arg.ReplyWith($"Closest monument blocker is {closestDistance:0.##}m away\n{monumentBlocker.GetDebugStatus()}");
		}
	}

	[ServerVar(Help = "<minutes> (optional: <range>) - Adds the given number of minutes to the decay grace period timer of the monument blocker closest to you, making it start decaying that much sooner. Negative values rewind the timer")]
	public static void addmonumentblockergrace(Arg arg)
	{
		float @float = arg.GetFloat(0);
		float float2 = arg.GetFloat(1, 100f);
		float closestDistance;
		MonumentBlocker monumentBlocker = FindClosestMonumentBlocker(arg, float2, out closestDistance);
		if (!(monumentBlocker == null))
		{
			monumentBlocker.AddGracePeriodTime(@float * 60f);
			arg.ReplyWith($"Added {@float} minutes of grace period to the monument blocker {closestDistance:0.##}m away\n{monumentBlocker.GetDebugStatus()}");
		}
	}

	[ServerVar(Help = "(Generated) Logs all server entity network group IDs and prefab names to the console; useful for debugging network visibility and group assignment")]
	public static void printgroups(Arg arg)
	{
		UnityEngine.Debug.Log("Server");
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			UnityEngine.Debug.Log(string.Format("{0}:{1}{2}", serverEntity.PrefabName, serverEntity.net.group.ID, serverEntity.net.group.restricted ? "/Restricted" : string.Empty));
		}
	}

	[ServerVar(Help = "Takes you in and out of your current network group, causing you to delete and then download all entities in your PVS again")]
	public static void flushgroup(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			basePlayer.net.SwitchGroup(BaseNetworkable.LimboNetworkGroup);
			basePlayer.UpdateNetworkGroup();
		}
	}

	[ServerVar(Help = "Break the current held object")]
	public static void breakheld(Arg arg)
	{
		Item activeItem = ArgEx.Player(arg).GetActiveItem();
		activeItem?.LoseCondition(activeItem.condition * 2f);
	}

	[ServerVar(Help = "Breaks the currently held shield")]
	public static void breakshield(Arg arg)
	{
		if (ArgEx.Player(arg).TryGetActiveShield(out var foundShield) && foundShield.GetItem() != null)
		{
			foundShield.GetItem().LoseCondition(999f);
		}
	}

	[ServerVar(Help = "Almost break the current held object")]
	public static void breakheld_almost(Arg arg)
	{
		Item activeItem = ArgEx.Player(arg).GetActiveItem();
		if (activeItem != null && activeItem.hasCondition)
		{
			activeItem.condition = 1f;
		}
	}

	[ServerVar(Help = "Reset all puzzles. Optionally provide a number to only reset puzzles within a radius.")]
	public static void puzzlereset(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (PuzzleReset allReset in PuzzleReset.AllResets)
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if (basePlayer != null)
			{
				StringView[] args = arg.Args;
				if (args != null && args.Length != 0)
				{
					int @int = arg.GetInt(0, int.MaxValue);
					if (Vector3.Distance(allReset.transform.position, basePlayer.transform.position) > (float)@int)
					{
						continue;
					}
				}
			}
			stringBuilder.AppendLine($"Resetting puzzle at: {allReset.transform.position}");
			allReset.DoReset();
			allReset.ResetTimer();
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(EditorOnly = true, Help = "respawn all puzzles from their prefabs")]
	public static void puzzleprefabrespawn(Arg arg)
	{
		foreach (BaseNetworkable item in BaseNetworkable.serverEntities.Where((BaseNetworkable x) => x is IOEntity && PrefabAttribute.server.Find<Construction>(x.prefabID) == null).ToList())
		{
			item.Kill();
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			GameObject gameObject = GameManager.server.FindPrefab(monument.gameObject.name);
			if (gameObject == null)
			{
				continue;
			}
			Dictionary<IOEntity, IOEntity> dictionary = new Dictionary<IOEntity, IOEntity>();
			IOEntity[] componentsInChildren = gameObject.GetComponentsInChildren<IOEntity>(includeInactive: true);
			foreach (IOEntity iOEntity in componentsInChildren)
			{
				Quaternion rot = monument.transform.rotation * iOEntity.transform.rotation;
				Vector3 pos = monument.transform.TransformPoint(iOEntity.transform.position);
				BaseEntity newEntity = GameManager.server.CreateEntity(iOEntity.PrefabName, pos, rot);
				IOEntity iOEntity2 = newEntity as IOEntity;
				if (!(iOEntity2 != null))
				{
					continue;
				}
				dictionary.Add(iOEntity, iOEntity2);
				DoorManipulator doorManipulator = newEntity as DoorManipulator;
				if (doorManipulator != null)
				{
					List<Door> obj = Facepunch.Pool.Get<List<Door>>();
					global::Vis.Entities(newEntity.transform.position, 10f, obj);
					Door door = obj.OrderBy((Door x) => x.Distance(newEntity.transform.position)).FirstOrDefault();
					if (door != null)
					{
						doorManipulator.targetDoor = door;
					}
					Facepunch.Pool.FreeUnmanaged(ref obj);
				}
				CardReader cardReader = newEntity as CardReader;
				if (cardReader != null)
				{
					CardReader cardReader2 = iOEntity as CardReader;
					if (cardReader2 != null)
					{
						cardReader.accessLevel = cardReader2.accessLevel;
						cardReader.accessDuration = cardReader2.accessDuration;
					}
				}
				TimerSwitch timerSwitch = newEntity as TimerSwitch;
				if (timerSwitch != null)
				{
					TimerSwitch timerSwitch2 = iOEntity as TimerSwitch;
					if (timerSwitch2 != null)
					{
						timerSwitch.timerLength = timerSwitch2.timerLength;
					}
				}
			}
			foreach (KeyValuePair<IOEntity, IOEntity> item2 in dictionary)
			{
				IOEntity key = item2.Key;
				IOEntity value = item2.Value;
				for (int j = 0; j < key.outputs.Length; j++)
				{
					if (!(key.outputs[j].connectedTo.ioEnt == null))
					{
						value.outputs[j].connectedTo.ioEnt = dictionary[key.outputs[j].connectedTo.ioEnt];
						value.outputs[j].connectedToSlot = key.outputs[j].connectedToSlot;
					}
				}
			}
			foreach (IOEntity value2 in dictionary.Values)
			{
				value2.Spawn();
			}
		}
	}

	[ServerVar(Help = "Break all the items in your inventory whose name match the passed string")]
	public static void breakitem(Arg arg)
	{
		string @string = arg.GetString(0);
		foreach (Item item in ArgEx.Player(arg).inventory.containerMain.itemList)
		{
			if (item.info.shortname.Contains(@string, CompareOptions.IgnoreCase) && item.hasCondition)
			{
				item.LoseCondition(item.condition * 2f);
			}
		}
	}

	[ServerVar(ClientAdmin = true, Help = "Refills the vital of a target player. eg. debug.refillsvital jim - leave blank to target yourself, can take multiple players at once. Will revive players if they are injured. To disable this, pass in --no-recover as the first argument.")]
	public static void refillvitals(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		int num = 0;
		bool shouldPlayerRecover = true;
		if (arg.GetString(0) == "--no-recover")
		{
			shouldPlayerRecover = false;
			num++;
		}
		arg.TryRemoveKeyBindEventArgs();
		if (arg.Args == null || num >= arg.Args.Length)
		{
			RefillPlayerVitals(basePlayer, shouldPlayerRecover);
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = num; i < arg.Args.Length; i++)
		{
			string @string = arg.GetString(i);
			BasePlayer basePlayer2 = ((!(@string == basePlayer.displayName)) ? (string.IsNullOrEmpty(@string) ? null : ArgEx.GetPlayerOrSleeperOrBot(arg, i)) : basePlayer);
			if (basePlayer2 == null)
			{
				stringBuilder.AppendLine("Could not find player '" + @string + "'");
				continue;
			}
			RefillPlayerVitals(basePlayer2, shouldPlayerRecover);
			stringBuilder.AppendLine("Refilled '" + @string + "' vitals");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(ClientAdmin = true, Help = "Refills the vitals of all active players on the server. Will revive players if they are injured. To disable this, pass in --no-recover as the first argument.")]
	public static void refillvitalsall(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool shouldPlayerRecover = arg.GetString(0) != "--no-recover";
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null))
			{
				RefillPlayerVitals(activePlayer, shouldPlayerRecover);
				stringBuilder.AppendLine("Refilled player '" + activePlayer.displayName + "' vitals");
			}
		}
		foreach (BasePlayer bot in BasePlayer.bots)
		{
			if (!(bot == null))
			{
				RefillPlayerVitals(bot, shouldPlayerRecover);
				stringBuilder.AppendLine("Refilled bot '" + bot.displayName + "' vitals");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	private static void RefillPlayerVitals(BasePlayer player, bool shouldPlayerRecover)
	{
		if (shouldPlayerRecover && player.IsWounded())
		{
			player.StopWounded();
		}
		AdjustHealth(player, 1000f);
		AdjustCalories(player, 1000f);
		AdjustHydration(player, 1000f);
		AdjustRadiation(player, -10000f);
		AdjustBleeding(player, -10000f);
	}

	[ServerVar(Help = "To disable revival if player is downed, pass in --no-recover as the first argument.")]
	public static void heal(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		bool flag = true;
		int num = 0;
		if (arg.GetString(0) == "--no-recover")
		{
			flag = false;
			num++;
		}
		if (flag && basePlayer.IsWounded())
		{
			basePlayer.StopWounded();
		}
		AdjustHealth(basePlayer, arg.GetInt(num, 1));
	}

	[ServerVar(Help = "(Generated) Deals a specified amount of bullet damage to the calling player; optionally targets a named bone to test per-bone hit reactions")]
	public static void hurt(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		int @int = arg.GetInt(0, 1);
		string @string = arg.GetString(1, string.Empty);
		HitInfo hitInfo = new HitInfo(basePlayer, basePlayer, DamageType.Bullet, @int);
		if (!string.IsNullOrEmpty(@string))
		{
			hitInfo.HitBone = StringPool.Get(@string);
		}
		basePlayer.OnAttacked(hitInfo);
	}

	[ServerVar(Help = "(Generated) Adds a specified amount of calories to the calling player at a configurable rate; useful for quickly testing hunger-related mechanics")]
	public static void eat(Arg arg)
	{
		AdjustCalories(ArgEx.Player(arg), arg.GetInt(0, 1), arg.GetInt(1, 1));
	}

	[ServerVar(Help = "(Generated) Adds a specified amount of hydration to the calling player at a configurable rate; useful for quickly testing thirst-related mechanics")]
	public static void drink(Arg arg)
	{
		AdjustHydration(ArgEx.Player(arg), arg.GetInt(0, 1), arg.GetInt(1, 1));
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player health to the specified value; useful for testing low-health or death scenarios")]
	public static void sethealth(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		float @float = arg.GetFloat(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if (usePlayer == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		usePlayer.SetHealth(@float);
		arg.ReplyWith($"Set health to {@float}");
	}

	[ServerVar(Help = "(Generated) Overrides the maximum health of the calling player or a named target; pass 0 to reset to the default value")]
	public static void setmaxhealth(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		int @int = arg.GetInt(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if (usePlayer == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		usePlayer.OverrideMaxHealth(@int);
		if (@int <= 0)
		{
			arg.ReplyWith("Reset max health");
		}
		else
		{
			arg.ReplyWith($"Set max health to {@int}");
		}
	}

	[ServerVar(Help = "(Generated) Deals enough bullet damage to bring the calling player or a named target to the specified health value")]
	public static void setdamage(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		int @int = arg.GetInt(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if ((bool)usePlayer)
		{
			float damageAmount = usePlayer.health - (float)@int;
			HitInfo info = new HitInfo(basePlayer, basePlayer, DamageType.Bullet, damageAmount);
			usePlayer.OnAttacked(info);
		}
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player calorie level to the specified value directly")]
	public static void setfood(Arg arg)
	{
		setattribute(arg, MetabolismAttribute.Type.Calories);
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player hydration level to the specified value directly")]
	public static void setwater(Arg arg)
	{
		setattribute(arg, MetabolismAttribute.Type.Hydration);
	}

	[ServerVar(Help = "(Generated) Sets the calling player or a named target player radiation level to the specified value directly")]
	public static void setradiation(Arg arg)
	{
		setattribute(arg, MetabolismAttribute.Type.Radiation);
	}

	private static void AdjustHealth(BasePlayer player, float amount, string bone = null)
	{
		player.health += amount;
	}

	private static void AdjustCalories(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.ApplyChange(MetabolismAttribute.Type.Calories, amount, time);
	}

	private static void AdjustHydration(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.ApplyChange(MetabolismAttribute.Type.Hydration, amount, time);
	}

	private static void AdjustRadiation(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.SetAttribute(MetabolismAttribute.Type.Radiation, amount);
	}

	private static void AdjustBleeding(BasePlayer player, float amount, float time = 1f)
	{
		player.metabolism.SetAttribute(MetabolismAttribute.Type.Bleeding, amount);
	}

	private static void setattribute(Arg arg, MetabolismAttribute.Type type)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Please enter an amount.");
			return;
		}
		int @int = arg.GetInt(0);
		BasePlayer usePlayer = GetUsePlayer(arg, 1);
		if ((bool)usePlayer)
		{
			usePlayer.metabolism.SetAttribute(type, @int);
		}
	}

	private static BasePlayer GetUsePlayer(Arg arg, int playerArgument)
	{
		BasePlayer basePlayer = null;
		if (arg.HasArgs(playerArgument + 1))
		{
			BasePlayer player = ArgEx.GetPlayer(arg, playerArgument);
			if (!player)
			{
				return null;
			}
			return player;
		}
		return ArgEx.Player(arg);
	}

	[ServerVar(Help = "(Generated) Resets all sleeping bag respawn cooldown timers for the calling player, allowing immediate re-use of all their bags")]
	public static void ResetSleepingBagTimers(Arg arg)
	{
		SleepingBag.ResetTimersForPlayer(ArgEx.Player(arg));
	}

	[ServerVar(Help = "Deducts the given number of hours from all spoilable food on the server")]
	public static void FoodSpoilingDeductTimeHours(Arg arg)
	{
		ItemModFoodSpoiling.DeductTimeFromAll(TimeSpan.FromHours(arg.GetFloat(0)));
	}

	[ServerVar(Help = "Spoils all food on the server")]
	public static void FoodSpoilingSpoilAll()
	{
		ItemModFoodSpoiling.DeductTimeFromAll(TimeSpan.MaxValue);
	}

	[ServerVar(Help = "Applies the given number of hours to all food in the players inventory")]
	public static void FoodSpoilingInventoryHours(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		int @int = arg.GetInt(0);
		PooledList<Item> spoilList = Facepunch.Pool.Get<PooledList<Item>>();
		try
		{
			FindSpoilableItems(basePlayer.inventory.containerMain.itemList);
			FindSpoilableItems(basePlayer.inventory.containerBelt.itemList);
			foreach (Item item in spoilList)
			{
				ItemModFoodSpoiling.FoodSpoilingWorkQueue.DeductTimeFromFoodItem(item, (float)@int * 60f * 60f, setDirty: true);
			}
		}
		finally
		{
			if (spoilList != null)
			{
				((IDisposable)spoilList).Dispose();
			}
		}
		void FindSpoilableItems(List<Item> items)
		{
			foreach (Item item2 in items)
			{
				if (item2.info.TryGetComponent<ItemModFoodSpoiling>(out var _))
				{
					spoilList.Add(item2);
				}
			}
		}
	}

	[ServerVar(Help = "(Generated) Forces all chickens within a given radius of the calling player to immediately spawn an egg; useful for testing egg drop and collection logic")]
	public static void ForceChickensSpawnEgg(Arg arg)
	{
		float @float = arg.GetFloat(0, 50f);
		if (ArgEx.Player(arg) == null)
		{
			return;
		}
		using PooledList<Chicken> pooledList = Facepunch.Pool.Get<PooledList<Chicken>>();
		global::Vis.Entities(ArgEx.Player(arg).transform.position, @float, pooledList, 2048);
		foreach (Chicken item in pooledList)
		{
			if (item.isServer)
			{
				item.SpawnEgg();
			}
		}
	}

	[ServerVar(Help = "(Generated) Drops a specified number of the given item short name as world entities from just in front of the calling player; useful for item physics testing")]
	public static void dropWorldItems(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		int @int = arg.GetInt(0);
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(arg.GetString(1));
		Vector3 point = basePlayer.eyes.HeadRay().GetPoint(1f);
		if (!(itemDefinition == null))
		{
			for (int i = 0; i < @int; i++)
			{
				ItemManager.Create(itemDefinition, 1, 0uL, isServerSide: true, 0uL).Drop(point, Vector3.zero, Quaternion.identity);
				point += Vector3.up * 0.3f;
			}
		}
	}

	[ServerVar(Help = "Spawns one of every deployable in a grid")]
	public static void spawn_all_deployables(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper))
		{
			arg.ReplyWith("Must be called by admin player");
			return;
		}
		arg.ReplyWith("Spawning all deployables");
		bool stability = Server.stability;
		Server.stability = false;
		try
		{
			Vector3 position = basePlayer.transform.position;
			List<ItemModDeployable> list = (from x in ItemManager.itemList
				select x.GetComponent<ItemModDeployable>() into x
				where x != null
				select x).ToList();
			int num = 12;
			float num2 = Mathf.Ceil(Mathf.Sqrt(list.Count));
			float num3 = num2 * (float)num / 2f;
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 pos = new Vector3(position.x - num3 + (float)num * ((float)i % num2), position.y, position.z - num3 + (float)num * Mathf.Floor((float)i / num2));
				GameManager.server.CreateEntity(list[i].entityPrefab.resourcePath, pos)?.Spawn();
			}
		}
		finally
		{
			Server.stability = stability;
		}
	}

	[ServerVar(Help = "(Generated) Scans all static respawn areas and kills any whose centre is within 1 metre of another, eliminating duplicate spawn points")]
	public static void removeOverlappingStaticSpawnPoints(Arg arg)
	{
		using PooledList<StaticRespawnArea> pooledList = Facepunch.Pool.Get<PooledList<StaticRespawnArea>>();
		foreach (StaticRespawnArea staticRespawnArea2 in StaticRespawnArea.staticRespawnAreas)
		{
			pooledList.Add(staticRespawnArea2);
		}
		int num = 0;
		for (int i = 0; i < pooledList.Count; i++)
		{
			StaticRespawnArea staticRespawnArea = pooledList[i];
			bool flag = false;
			foreach (StaticRespawnArea item in pooledList)
			{
				if (item != staticRespawnArea && item.Distance(staticRespawnArea) < 1f)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				pooledList.RemoveAt(i);
				i--;
				num++;
				staticRespawnArea.Kill();
			}
		}
		arg.ReplyWith($"Destroyed {num} overlapping static spawn points");
	}

	[ServerVar(Help = "(Generated) Sets the ore fill percentage on all unloadable train cars within 3 metres of the calling player; updates both inventory amounts and visual ore level")]
	public static void setUnloadableCarFillPercent(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		Vector3 position = basePlayer.transform.position;
		using PooledList<TrainCarUnloadable> pooledList = Facepunch.Pool.Get<PooledList<TrainCarUnloadable>>();
		global::Vis.Entities(position, 3f, pooledList, 8192);
		float num = Mathf.Clamp01(arg.GetFloat(0));
		foreach (TrainCarUnloadable item in pooledList)
		{
			if (!item.isServer)
			{
				continue;
			}
			foreach (Item item2 in item.GetStorageContainer().inventory.itemList)
			{
				item2.amount = Mathf.Max(Mathf.RoundToInt(num), 1);
			}
			item.SetLootPercentage(num);
			item.SetVisualOreLevel(num);
			item.SendNetworkUpdate();
			arg.ReplyWith($"Set ore level to {num} on {item.PrefabName}");
		}
	}

	[ServerVar(Help = "fillTankerModule <item> - Fills the tanker module(s) of the modular car you're looking at with the given liquid (e.g. water, water.salt, crude.oil)")]
	public static void fillTankerModule(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player.");
			return;
		}
		string @string = arg.GetString(0);
		if (string.IsNullOrEmpty(@string))
		{
			arg.ReplyWith("Please provide a liquid item shortname (e.g. water, water.salt, crude.oil).");
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(@string);
		if (itemDefinition == null)
		{
			arg.ReplyWith("Could not find an item with shortname '" + @string + "'.");
			return;
		}
		BaseModularVehicle lookedAtModularCar = GetLookedAtModularCar(basePlayer);
		if (lookedAtModularCar == null)
		{
			arg.ReplyWith("Not looking at a modular car.");
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (BaseVehicleModule attachedModuleEntity in lookedAtModularCar.AttachedModuleEntities)
		{
			if (!(attachedModuleEntity is VehicleModuleStorage vehicleModuleStorage) || !(vehicleModuleStorage.GetContainer() is LiquidContainer { inventory: { } inventory } liquidContainer))
			{
				continue;
			}
			int amount = ((inventory.maxStackSize > 0) ? inventory.maxStackSize : itemDefinition.stackable);
			Item item = liquidContainer.GetLiquidItem();
			if (item != null && item.info != itemDefinition)
			{
				item.Remove();
				item = null;
			}
			if (item != null)
			{
				item.amount = amount;
				item.MarkDirty();
			}
			else
			{
				inventory.AddItem(itemDefinition, amount, 0uL, ItemContainer.LimitStack.All);
			}
			Item liquidItem = liquidContainer.GetLiquidItem();
			if (liquidItem != null && liquidItem.info == itemDefinition)
			{
				if (itemDefinition == VehicleModuleStorage.CrudeItem)
				{
					liquidItem.LockUnlock(bNewState: true);
				}
				num++;
			}
			else
			{
				num2++;
			}
		}
		if (num == 0 && num2 == 0)
		{
			arg.ReplyWith("That car (" + lookedAtModularCar.ShortPrefabName + ") has no tanker (liquid storage) module.");
		}
		else if (num == 0)
		{
			arg.ReplyWith("The tanker module(s) on " + lookedAtModularCar.ShortPrefabName + " would not accept '" + itemDefinition.shortname + "'.");
		}
		else
		{
			string text = $"Filled {num} tanker module(s) on {lookedAtModularCar.ShortPrefabName} with {itemDefinition.shortname}.";
			if (num2 > 0)
			{
				text += $" ({num2} module(s) rejected the item.)";
			}
			arg.ReplyWith(text);
		}
	}

	[ServerVar(Help = "emptyTankerModule - Clears the contents of the tanker module(s) of the modular car you're looking at")]
	public static void emptyTankerModule(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player.");
			return;
		}
		BaseModularVehicle lookedAtModularCar = GetLookedAtModularCar(basePlayer);
		if (lookedAtModularCar == null)
		{
			arg.ReplyWith("Not looking at a modular car.");
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (BaseVehicleModule attachedModuleEntity in lookedAtModularCar.AttachedModuleEntities)
		{
			if (attachedModuleEntity is VehicleModuleStorage vehicleModuleStorage && vehicleModuleStorage.GetContainer() is LiquidContainer liquidContainer)
			{
				num2++;
				Item liquidItem = liquidContainer.GetLiquidItem();
				if (liquidItem != null)
				{
					liquidItem.LockUnlock(bNewState: false);
					liquidItem.Remove();
					num++;
				}
			}
		}
		if (num2 == 0)
		{
			arg.ReplyWith("That car (" + lookedAtModularCar.ShortPrefabName + ") has no tanker (liquid storage) module.");
		}
		else
		{
			arg.ReplyWith((num > 0) ? $"Emptied {num} tanker module(s) on {lookedAtModularCar.ShortPrefabName}." : ("The tanker module(s) on " + lookedAtModularCar.ShortPrefabName + " were already empty."));
		}
	}

	private static BaseModularVehicle GetLookedAtModularCar(BasePlayer player)
	{
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, player.eyes.HeadRay(), 0f, 12f, 1218652417);
		while (baseNetworkable != null)
		{
			if (baseNetworkable is BaseModularVehicle result)
			{
				return result;
			}
			if (baseNetworkable is BaseVehicleModule baseVehicleModule && baseVehicleModule.Vehicle != null)
			{
				return baseVehicleModule.Vehicle;
			}
			baseNetworkable = baseNetworkable.GetParentEntity();
		}
		return null;
	}

	[ServerVar(Help = "fillmounts <radius> - Spawns and mounts a player on every mount point in radius")]
	public static void fillmounts(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		int num = Mathf.Clamp(arg.GetInt(0), 0, 100);
		if (num == 0)
		{
			arg.ReplyWith("Must supply a radius > 0!");
			return;
		}
		Vector3 position = basePlayer.transform.position;
		int layerMask = 1218521345;
		using PooledList<BaseMountable> pooledList = Facepunch.Pool.Get<PooledList<BaseMountable>>();
		StringBuilder obj = Facepunch.Pool.Get<StringBuilder>();
		global::Vis.Entities(position, num, pooledList, layerMask);
		foreach (BaseMountable item in pooledList)
		{
			if (item.isClient)
			{
				continue;
			}
			if (item is RidableHorse ridableHorse)
			{
				if (ridableHorse.HasSingleSaddle)
				{
					TrySpawnAndMountPlayer(ridableHorse.mountPoints[0].mountable, obj);
				}
				else if (ridableHorse.HasDoubleSaddle)
				{
					TrySpawnAndMountPlayer(ridableHorse.mountPoints[1].mountable, obj);
					TrySpawnAndMountPlayer(ridableHorse.mountPoints[2].mountable, obj);
				}
			}
			else if (item is BaseVehicle { allMountPoints: var allMountPoints })
			{
				foreach (BaseVehicle.MountPointInfo item2 in allMountPoints)
				{
					if (item2 != null && item2.mountable != null)
					{
						TrySpawnAndMountPlayer(item2.mountable, obj);
					}
				}
			}
			else
			{
				TrySpawnAndMountPlayer(item, obj);
			}
		}
		if (obj.Length > 0)
		{
			obj.Remove(obj.Length - 1, 1);
		}
		string text = obj.ToString();
		Facepunch.Pool.FreeUnmanaged(ref obj);
		arg.ReplyWith((text.Length > 0) ? text : "Didn't find any eligible/unoccupied mount points in this radius.");
	}

	private static void TrySpawnAndMountPlayer(BaseMountable mountable, StringBuilder sb)
	{
		if (!mountable.AnyMounted())
		{
			BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab") as BasePlayer;
			basePlayer.Spawn();
			mountable.AttemptMount(basePlayer, doMountChecks: false);
			if (!basePlayer.isMounted)
			{
				sb.AppendLine("Failed to mount a player to: " + mountable.ShortPrefabName);
				basePlayer.Kill();
			}
			else
			{
				basePlayer.UpdateNetworkGroup();
				sb.AppendLine("Mounted a player to: " + mountable.ShortPrefabName);
			}
		}
	}

	[ServerVar(Help = "Spawn lots of IO entities to lag the server")]
	public static void bench_io(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsAdmin)
		{
			return;
		}
		int @int = arg.GetInt(0, 50);
		string name = arg.GetString(1, "water_catcher_small");
		List<IOEntity> list = new List<IOEntity>();
		WaterCatcher waterCatcher = null;
		Vector3 position = ArgEx.Player(arg).transform.position;
		string[] array = (from x in GameManifest.Current.entities
			where Path.GetFileNameWithoutExtension(x).Contains(name, CompareOptions.IgnoreCase)
			select x.ToLower()).ToArray();
		if (array.Length == 0)
		{
			arg.ReplyWith("Couldn't find io prefab \"" + array[0] + "\"");
			return;
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(Path.GetFileNameWithoutExtension(x), name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				UnityEngine.Debug.Log($"{arg} failed to find io entity \"{name}\"");
				arg.ReplyWith("Unknown entity - could be:\n\n" + string.Join("\n", array.Select(Path.GetFileNameWithoutExtension).ToArray()));
				return;
			}
			array[0] = text;
		}
		for (int i = 0; i < @int; i++)
		{
			Vector3 pos = position + new Vector3(i * 5, 0f, 0f);
			Quaternion identity = Quaternion.identity;
			BaseEntity baseEntity = GameManager.server.CreateEntity(array[0], pos, identity);
			if (!baseEntity)
			{
				continue;
			}
			baseEntity.Spawn();
			WaterCatcher component = baseEntity.GetComponent<WaterCatcher>();
			if ((bool)component)
			{
				list.Add(component);
				if (waterCatcher != null)
				{
					Connect(waterCatcher, component);
				}
				if (i == @int - 1)
				{
					Connect(component, list.First());
				}
				waterCatcher = component;
			}
		}
		static void Connect(IOEntity InputIOEnt, IOEntity OutputIOEnt)
		{
			int num = 0;
			int num2 = 0;
			WireTool.WireColour wireColour = WireTool.WireColour.Gray;
			IOEntity.IOSlot iOSlot = InputIOEnt.inputs[num];
			IOEntity.IOSlot obj = OutputIOEnt.outputs[num2];
			iOSlot.connectedTo.Set(OutputIOEnt);
			iOSlot.connectedToSlot = num2;
			iOSlot.wireColour = wireColour;
			iOSlot.connectedTo.Init();
			obj.connectedTo.Set(InputIOEnt);
			obj.connectedToSlot = num;
			obj.wireColour = wireColour;
			obj.connectedTo.Init();
			obj.linePoints = new Vector3[2]
			{
				Vector3.zero,
				OutputIOEnt.transform.InverseTransformPoint(InputIOEnt.transform.TransformPoint(iOSlot.handlePosition))
			};
			OutputIOEnt.MarkDirtyForceUpdateOutputs();
			OutputIOEnt.SendNetworkUpdate();
			InputIOEnt.SendNetworkUpdate();
			OutputIOEnt.SendChangedToRoot(forceUpdate: true);
		}
	}

	[Help("Arg0: mission stage (int), Arg1: block objective resetting (bool, default false)")]
	[ServerVar]
	public static void completeMissionStage(Arg arg)
	{
		int @int = arg.GetInt(0, -1);
		bool @bool = arg.GetBool(1);
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer != null) || !basePlayer.TryGetActiveMissionInstance(out var instance))
		{
			return;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
			if (!objectiveStatus.completed && (i == @int || (@int == -1 && !objectiveStatus.completed)))
			{
				MissionObjective missionObjective = instance.GetMission().objectives[i].Get();
				missionObjective.ServerObjectiveStarted(basePlayer, i, instance);
				missionObjective.CompleteObjective(i, instance, basePlayer);
				if (@bool)
				{
					objectiveStatus.blockReset = true;
				}
				break;
			}
		}
	}

	[ServerVar(Help = "(Generated) Completes all incomplete objectives in the calling player active mission, triggering the mission completion flow")]
	public static void completeMission(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer != null) || !basePlayer.TryGetActiveMissionInstance(out var instance))
		{
			return;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			if (!instance.objectiveStatuses[i].completed)
			{
				instance.GetMission().objectives[i].objective.CompleteObjective(i, instance, basePlayer);
			}
		}
	}

	[ServerVar(Help = "Prints out the topologies at your position")]
	public static void print_topologies(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		StringBuilder obj = Facepunch.Pool.Get<StringBuilder>();
		int topology = TerrainMeta.TopologyMap.GetTopology(basePlayer.transform.position);
		foreach (TerrainTopology.Enum value in Enum.GetValues(typeof(TerrainTopology.Enum)))
		{
			int num = (int)value;
			if ((topology & num) == num)
			{
				obj.AppendLine(value.ToString());
			}
		}
		arg.ReplyWith(obj.ToString());
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerUserVar]
	public static void startTutorial(Arg arg)
	{
		if (!Server.tutorialEnabled)
		{
			arg.ReplyWith("Tutorial is not enabled on this server");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer != null && !basePlayer.IsInTutorial)
		{
			basePlayer.StartTutorial(triggerAnalytics: false);
		}
	}

	[ServerVar(Help = "(Generated) Immediately completes the calling player tutorial by triggering the island completion callback; bypasses normal progression")]
	public static void completeTutorial(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer != null && basePlayer.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = basePlayer.GetCurrentTutorialIsland();
			if (currentTutorialIsland != null)
			{
				currentTutorialIsland.OnPlayerCompletedTutorial(basePlayer, isQuit: false, triggerAnalytics: false);
			}
		}
	}

	[ServerUserVar(ServerAdmin = false)]
	public static void quitTutorial(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer != null && basePlayer.IsInTutorial)
		{
			TutorialIsland currentTutorialIsland = basePlayer.GetCurrentTutorialIsland();
			if (currentTutorialIsland != null)
			{
				currentTutorialIsland.OnPlayerCompletedTutorial(basePlayer, isQuit: true, triggerAnalytics: true);
			}
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all active tutorial islands showing index, network group ID, assigned player name, duration, and connection state")]
	public static void tutorialStatus(Arg arg)
	{
		ListHashSet<TutorialIsland> tutorialList = TutorialIsland.GetTutorialList(isServer: true);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumns("Index", "ID", "Player Name", "Duration", "IsConnected");
		int num = 0;
		foreach (TutorialIsland item in tutorialList)
		{
			BasePlayer basePlayer = item.ForPlayer.Get(serverside: true);
			textTable.AddRow(num++.ToString(), (item.net.group.ID - 1).ToString(), (basePlayer != null) ? basePlayer.displayName : "NULL", TimeSpanEx.ToShortString(item.TutorialDuration), (basePlayer != null) ? basePlayer.IsConnected.ToString() : "NULL");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Tutorial islands in use: {num}/{TutorialIsland.MaxTutorialIslandCount}");
		stringBuilder.AppendLine(textTable.ToString());
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Make admin invisible")]
	public static void invis(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		bool @bool = arg.GetBool(0, !basePlayer.isInvisible);
		if (@bool && !basePlayer.isInvisible)
		{
			if (Interface.CallHook("OnPlayerVanish", basePlayer) != null)
			{
				return;
			}
			foreach (Connection subscriber in basePlayer.net.group.subscribers)
			{
				BasePlayer basePlayer2 = subscriber.player as BasePlayer;
				if (subscriber != basePlayer.net.connection && basePlayer.ShouldNetworkTo(basePlayer2) && !basePlayer2.IsSpectating())
				{
					basePlayer.DestroyOnClient(subscriber);
				}
			}
			if (ServerOcclusion.OcclusionEnabled)
			{
				basePlayer.OcclusionMakeSubscribersForget();
			}
			basePlayer.isInvisible = true;
			BasePlayer.invisPlayers.Add(basePlayer);
			basePlayer.DisablePlayerCollider();
			SimpleAIMemory.AddIgnorePlayer(basePlayer);
			BaseEntity.Query.Server.RemovePlayer(basePlayer);
			Interface.CallHook("OnPlayerVanished", basePlayer);
		}
		else if (!@bool && basePlayer.isInvisible)
		{
			if (Interface.CallHook("OnPlayerUnvanish", basePlayer) != null)
			{
				return;
			}
			basePlayer.isInvisible = false;
			BasePlayer.invisPlayers.Remove(basePlayer);
			basePlayer.EnablePlayerCollider();
			if (!ServerOcclusion.OcclusionEnabled)
			{
				foreach (Connection subscriber2 in basePlayer.net.group.subscribers)
				{
					BasePlayer player = subscriber2.player as BasePlayer;
					if (basePlayer.ShouldNetworkTo(player))
					{
						basePlayer.SendAsSnapshotWithChildren(player);
					}
				}
			}
			SimpleAIMemory.RemoveIgnorePlayer(basePlayer);
			BaseEntity.Query.Server.RemovePlayer(basePlayer);
			BaseEntity.Query.Server.AddPlayer(basePlayer);
			Interface.CallHook("OnPlayerUnvanished", basePlayer);
		}
		arg.ReplyWith("Invis: " + basePlayer.isInvisible);
		basePlayer.Command("debug.setinvis_ui", basePlayer.isInvisible);
	}

	[ServerVar(Help = "(Generated) Removes all active modifiers (buffs/debuffs) from the calling player; useful for resetting modifier state during testing")]
	public static void clearPlayerModifiers(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			int count = basePlayer.modifiers.All.Count;
			basePlayer.modifiers.RemoveAll();
			arg.ReplyWith($"Removed {count} modifiers");
		}
	}

	[ServerVar(Help = "(Generated) Sets the visual variant index on the building block the calling player is looking at; useful for testing block randomisation visuals")]
	public static void applyBuildingBlockRandomisation(Arg arg)
	{
		int @int = arg.GetInt(0);
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && GamePhysics.Trace(basePlayer.eyes.HeadRay(), 0f, out var hitInfo, 3f, 2097408) && RaycastHitEx.GetEntity(hitInfo) is SimpleBuildingBlock simpleBuildingBlock)
		{
			simpleBuildingBlock.SetVariant(@int);
		}
	}

	[ServerVar(Help = "(Generated) Prints a summary table of all VineSwingingTree and VineMountable entities on the server, including average destination count per mountable")]
	public static void vineSwingingReport(Arg arg)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is VineSwingingTree)
			{
				num++;
			}
			if (serverEntity is VineMountable vineMountable)
			{
				num2++;
				num3 += vineMountable.DestinationCount;
			}
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumns("Entity", "Count");
		textTable.AddRow("VineTrees", num.ToString());
		textTable.AddRow("VineMountables", num2.ToString());
		textTable.AddRow("VineMountableDirections", ((float)num3 / (float)num2).ToString());
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Sends a highlight RPC to every VineMountable on the server targeting the calling player; used for visually debugging vine placement")]
	public static void vineSwingingHighlight(Arg arg)
	{
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity is VineMountable vineMountable)
			{
				vineMountable.Highlight(ArgEx.Player(arg));
			}
		}
	}

	[ServerVar(Help = "(Generated) Respawns vine trees from their stumps within a given radius of the calling player; reports how many were respawned versus blocked by players")]
	public static void respawnVineTreesInRadius(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		float @float = arg.GetFloat(0);
		using PooledList<Collider> pooledList = Facepunch.Pool.Get<PooledList<Collider>>();
		GamePhysics.OverlapSphere(basePlayer.transform.position, @float, pooledList, 1073741824);
		int num = 0;
		int num2 = 0;
		using PooledList<VineSwingingTreeStump> pooledList2 = Facepunch.Pool.Get<PooledList<VineSwingingTreeStump>>();
		foreach (Collider item in pooledList)
		{
			VineSwingingTreeStump vineSwingingTreeStump = GameObjectEx.ToBaseEntity(item) as VineSwingingTreeStump;
			if (vineSwingingTreeStump != null && vineSwingingTreeStump.isServer && !pooledList2.Contains(vineSwingingTreeStump))
			{
				pooledList2.Add(vineSwingingTreeStump);
				if (vineSwingingTreeStump.RespawnTree())
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		arg.ReplyWith($"Respawned {num} trees in {@float}m, {num2} were blocked by players");
	}

	[ServerVar(Help = "(Generated) Prints the world position of every industrial conveyor running in strict mode; helps locate conveyors that are blocking item flow")]
	public static void conveyorStrictModeReport(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		IndustrialConveyor[] array = BaseEntity.Util.FindAll<IndustrialConveyor>();
		foreach (IndustrialConveyor industrialConveyor in array)
		{
			if (industrialConveyor.strictMode)
			{
				stringBuilder.AppendLine($"{industrialConveyor.transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Sends a configurable number of test custom vital entries to the calling player client for a given duration; used to verify custom vitals UI rendering")]
	public static void test_custom_vitals(Arg arg)
	{
		int num = Mathf.Clamp(arg.GetInt(0, 1), 0, 100);
		int @int = arg.GetInt(1, 60);
		string @string = arg.GetString(2, "ss");
		CustomVitals customVitals = new CustomVitals
		{
			vitals = new List<CustomVitalInfo>()
		};
		for (int i = 0; i < num; i++)
		{
			customVitals.vitals.Add(new CustomVitalInfo
			{
				active = true,
				backgroundColor = Color.red,
				iconColor = Color.green,
				leftTextColor = Color.blue,
				rightTextColor = Color.yellow,
				leftText = "Left",
				rightText = "Right {timeleft:" + @string + "}",
				timeLeft = @int
			});
		}
		CommunityEntity.ServerInstance.SendCustomVitals(ArgEx.Player(arg), customVitals);
	}

	[ServerVar(Help = "0 = can't throw, 1 = can throw & melee, 2 = only throwable")]
	public static void setthrowable(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("This can only be ran by players");
			return;
		}
		BaseMelee baseMelee = basePlayer.GetHeldEntity() as BaseMelee;
		if (baseMelee == null)
		{
			arg.ReplyWith("You must be holding a melee weapon");
			return;
		}
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'setthrowable {0-2}");
			return;
		}
		int @int = arg.GetInt(0);
		switch (@int)
		{
		case 0:
			baseMelee.canThrowAsProjectile = false;
			baseMelee.onlyThrowAsProjectile = false;
			break;
		case 1:
			baseMelee.canThrowAsProjectile = true;
			baseMelee.onlyThrowAsProjectile = false;
			break;
		case 2:
			baseMelee.canThrowAsProjectile = true;
			baseMelee.onlyThrowAsProjectile = true;
			break;
		default:
			arg.ReplyWith($"Invalid throwable value {@int}, must be 0 (not throwable), 1 (throwable) or 2 (only throwable)");
			return;
		}
		baseMelee.SendNetworkUpdate();
		arg.ReplyWith($"Set canThrowAsProjectile to {@int} on {baseMelee.ShortPrefabName}");
	}

	[ServerVar(Help = "(Generated) Applies a debug reset time in seconds to all PuzzleReset objects in the scene, shortening their timers for rapid testing")]
	public static void applyPuzzleResetTime(Arg arg)
	{
		float @float = arg.GetFloat(0);
		PuzzleReset[] array = UnityEngine.Object.FindObjectsByType<PuzzleReset>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DebugApplyPuzzleResetTime(@float);
		}
	}

	[ServerVar(Help = "(Generated) Prints detailed debug info about the PuzzleReset the calling player is currently inside, including timer state and dependency status")]
	public static void puzzleResetInfo(Arg arg)
	{
		BasePlayer bp = ArgEx.Player(arg);
		foreach (PuzzleReset allReset in PuzzleReset.AllResets)
		{
			if (!allReset.IsPlayerInRange(bp))
			{
				continue;
			}
			List<string> list = new List<string>();
			allReset.GetDebugInfo(list);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(UnityEngine.TransformEx.GetRecursiveName(allReset.transform));
			foreach (string item in list)
			{
				stringBuilder.AppendLine(item);
			}
			arg.ReplyWith(stringBuilder.ToString());
			return;
		}
		arg.ReplyWith("Player is not inside any PuzzleResets");
	}

	[ServerVar(Help = "Find how large of a gap there is. <maxDistance> <stepsize> <maxSize> <layer>")]
	public static void findgap(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper))
		{
			return;
		}
		float @float = arg.GetFloat(0, 3f);
		float float2 = arg.GetFloat(1, 0.01f);
		float float3 = arg.GetFloat(2, 0.5f);
		int @int = arg.GetInt(3, 2162688);
		Ray ray = basePlayer.eyes.BodyRay();
		if (GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0.01f, out var _, @float, @int, QueryTriggerInteraction.Ignore))
		{
			arg.ReplyWith($"Reduce max distance: hit before {@float}m");
			return;
		}
		basePlayer.SendConsoleCommand(DDrawCommand.Line(ray.origin, ray.origin + ray.direction * @float, 5f, Color.red));
		for (float num = float2; num <= float3; num += float2)
		{
			if (GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, num, out var _, @float - num, @int, QueryTriggerInteraction.Ignore))
			{
				arg.ReplyWith($"Gap size: {num}m");
				return;
			}
		}
		arg.ReplyWith($"Gap larger than {float3}m (or something went wrong!)");
	}

	[ServerVar(Help = "(Generated) Spawns a 100x10 grid of lit furnaces loaded with wood and metal ore near the calling player; used to stress-test the oven cooking system")]
	public static void spawnOvenStressTest(Arg arg)
	{
		Vector3 position = ArgEx.Player(arg).transform.position;
		for (int i = 0; i < 100; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				Vector3 pos = position + new Vector3((float)i * 1f, 0f, (float)j * 1f);
				BaseOven baseOven = GameManager.server.CreateEntity("Assets/Prefabs/Deployable/Furnace/furnace.prefab", pos, Quaternion.identity) as BaseOven;
				baseOven.Spawn();
				ItemManager.CreateByName("wood", 1000, 0uL).MoveToContainer(baseOven.inventory, 0);
				ItemManager.CreateByName("metal.ore", 1000, 0uL).MoveToContainer(baseOven.inventory, baseOven.fuelSlots);
				baseOven.StartCooking();
			}
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all ObjectWorkQueue instances showing name, total items processed, current queue length, and cumulative execution time")]
	[ClientVar(ClientAdmin = true)]
	public static void printqueues(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.ResizeColumns(5);
		textTable.AddColumn("Name");
		textTable.AddColumn("Processed");
		textTable.AddColumn("Size");
		textTable.AddColumn("Capacity");
		textTable.AddColumn("Execution Time");
		textTable.ResizeRows(ObjectWorkQueue.All.Count);
		foreach (ObjectWorkQueue item in ObjectWorkQueue.All.OrderBy((ObjectWorkQueue x) => x.Name))
		{
			TimeSpan totalExecutionTime = item.TotalExecutionTime;
			string value = ((totalExecutionTime.TotalMilliseconds < 1000.0) ? $"{Math.Floor(totalExecutionTime.TotalMilliseconds)}ms" : $"{Math.Round(totalExecutionTime.TotalSeconds, 2)}s");
			textTable.AddValue(item.Name);
			textTable.AddValue(item.TotalProcessedCount);
			textTable.AddValue(item.QueueLength);
			textTable.AddValue(item.Capacity);
			textTable.AddValue(value);
		}
		arg.ReplyWith(flag ? textTable.ToJson() : textTable.ToString());
	}

	[ClientVar(Help = "Logs a test error and exception for testing error display.")]
	[ServerVar(Help = "Logs a test error and exception for testing error display.")]
	public static void testerror(Arg arg)
	{
		UnityEngine.Debug.LogError("Test error message");
		UnityEngine.Debug.LogException(new NullReferenceException("Test NullReferenceException"));
	}

	[ServerVar(Help = "(Generated) Prints the network visibility layer (overworld, tunnel, underwater, etc.) at the calling player position; helps debug layer-based network group assignment")]
	public static void printgrouplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null))
		{
			Vector3 position = basePlayer.transform.position;
			int? num = Network.Net.sv?.visibility?.PositionToLayer(position.x, position.y, position.z, basePlayer.networkRange);
			string text;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				text = ((valueOrDefault >= 10) ? $"Dynamic Dungeons ({num.Value})" : (valueOrDefault switch
				{
					0 => "Overworld (Small)", 
					1 => "Overworld (Medium)", 
					2 => "Overworld (Large)", 
					3 => "Caves", 
					4 => "Tunnels", 
					5 => "Deep Sea", 
					_ => $"Unknown ({num.Value})", 
				}));
			}
			else
			{
				text = "<null>";
			}
			string text2 = text;
			string strValue = (TerrainMeta.IsPointWithinTutorialBounds(position) ? (text2 + " (but you're in the tutorial bounds)") : text2);
			arg.ReplyWith(strValue);
		}
	}
}
