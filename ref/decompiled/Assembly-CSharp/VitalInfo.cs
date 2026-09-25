using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class VitalInfo : MonoBehaviour, IClientComponent, IVitalNotice
{
	public enum Vital
	{
		BuildingBlocked,
		CanBuild,
		Crafting,
		CraftLevel1,
		CraftLevel2,
		CraftLevel3,
		DecayProtected,
		Decaying,
		SafeZone,
		Buffed,
		Pet,
		ModifyClan,
		DartEffects,
		NegativeEffects,
		RaidBlocked,
		RaidEnabled,
		RentDue,
		CombatZone
	}

	public HudElement Element;

	public Image InfoImage;

	public Vital VitalType;

	public RustText text;

	public static readonly Translate.Phrase BoostActive = new Translate.Phrase("tea.boostactive", "{0} Boost active");

	public static readonly Translate.Phrase BoostsActive = new Translate.Phrase("tea.boostactive.plural", "{0} Boosts active");

	public static readonly Translate.Phrase DartEffectActive = new Translate.Phrase("dart.effectactive", "{0} dart effect");

	public static readonly Translate.Phrase DartEffectsActive = new Translate.Phrase("dart.effectactive.plural", "{0} dart effects");

	public static readonly Translate.Phrase NegativeEffectActive = new Translate.Phrase("negative.active", "{0} negative effect");

	public static readonly Translate.Phrase NegativeEffectsActive = new Translate.Phrase("negative.active.plural", "{0} negative effects");

	public static readonly Translate.Phrase RaidOpensAt = new Translate.Phrase("raid.opensat", "Raiding opens at {0}");
}
