using Rust.UI;

public class UIClanScoreEventsEntry : BaseMonoBehaviour, IUIPlayerRefreshCallback
{
	public static readonly Translate.Phrase ClanPlayerKilledEvent = new TokenisedPhrase("clan.score_event.clan_player_killed", "{0} killed {1}, member of clan {2}.");

	public static readonly Translate.Phrase ClanPlayerDiedEvent = new TokenisedPhrase("clan.score_event.clan_player_died", "{0} was killed by {1}, member of clan {2}.");

	public static readonly Translate.Phrase KilledUnarmedEvent = new TokenisedPhrase("clan.score_event.killed_unarmed", "{0} killed {1} when they were unarmed.");

	public static readonly Translate.Phrase DestroyedToolCupboardEvent = new TokenisedPhrase("clan.score_event.destroyed_tool_cupboard", "{0} destroyed a Tool Cupboard owned by clan {1}.");

	public static readonly Translate.Phrase HackedCrateEvent = new TokenisedPhrase("clan.score_event.hacked_crate", "{0} hacked a locked crate.");

	public static readonly Translate.Phrase OpenedHackedCrateEvent = new TokenisedPhrase("clan.score_event.opened_hacked_crate", "{0} opened a hacked crate.");

	public static readonly Translate.Phrase DestroyedBradleyEvent = new TokenisedPhrase("clan.score_event.destroyed_bradley", "{0} destroyed Bradley APC.");

	public static readonly Translate.Phrase RanExcavatorEvent = new TokenisedPhrase("clan.score_event.ran_excavator", "{0} ran the Giant Excavator.");

	public static readonly Translate.Phrase ReachedCargoShipEvent = new TokenisedPhrase("clan.score_event.boarded_cargo_ship", "{0} has boarded a Cargo Ship.");

	public static readonly Translate.Phrase LootedEliteCrateEvent = new TokenisedPhrase("clan.score_event.looted_elite_crate", "{0} has looted an Elite Crate.");

	public static readonly Translate.Phrase DestroyedPatrolHeliEvent = new TokenisedPhrase("clan.score_event.destroyed_patrol_heli", "{0} destroyed patrol helicopter.");

	public static readonly Translate.Phrase SwipedRedKeycardEvent = new TokenisedPhrase("clan.score_event.swiped_red_keycard", "{0} swiped a red keycard.");

	public static readonly Translate.Phrase InsertHeavyFuseInPowerPlant = new TokenisedPhrase("clan.score_event.inserted_heavy_fuse_power_plant", "{0} inserted a heavy fuse at powerplant.");

	public static readonly Translate.Phrase LootSatelliteEvent = new TokenisedPhrase("clan.score_event.loot_satellite", "{0} has looted a crashed satellite.");

	public static readonly Translate.Phrase EnableWaterTreatmentPlantEvent = new TokenisedPhrase("clan.score_event.enable_water_treatment_plant", "{0} ran the Water Treatment Plant.");

	public static readonly Translate.Phrase LaunchSatelliteEvent = new TokenisedPhrase("clan.score_event.launch_satellite", "{0} launched a satellite.");

	public static readonly Translate.Phrase StartedOilRigFuelSwitchEvent = new TokenisedPhrase("clan.score_event.started_oil_rig_fuel_switch", "{0} started the Oil Rig fuel switch.");

	public static readonly Translate.Phrase UnknownEvent = new TokenisedPhrase("clan.score_event.unknown", "{0} did something (event type = {1}).");

	public RustText Event;

	public RustText Score;

	public RustText Multiplier;

	public RustText Time;
}
