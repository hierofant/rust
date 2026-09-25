using System;
using System.Text;
using Facepunch;
using ProtoBuf;

public class Modifier
{
	public enum ModifierType
	{
		Wood_Yield,
		Ore_Yield,
		Radiation_Resistance,
		Radiation_Exposure_Resistance,
		Max_Health,
		Scrap_Yield,
		MoveSpeed,
		ObscureVision,
		Warming,
		Cooling,
		CoreTemperatureMinAdjustment,
		CoreTemperatureMaxAdjustment,
		Crafting_Quality,
		VisionCare,
		MetabolismBooster,
		Harvesting,
		DigestionBoost,
		FishingBoost,
		Collectible_DoubleYield,
		Farming_BetterGenes,
		HorseGallopSpeed,
		HorseDungProductionBoost,
		Comfort,
		Clotting,
		HunterVision,
		Radiation,
		DigestionBoostTimeMod
	}

	public enum ModifierSource
	{
		Tea,
		Dart,
		Interaction,
		NegativeEffect,
		MedicalSyringe
	}

	public static Translate.Phrase WoodYieldPhrase = new Translate.Phrase("mod.woodyield", "Wood Yield");

	public static Translate.Phrase OreYieldPhrase = new Translate.Phrase("mod.oreyield", "Ore Yield");

	public static Translate.Phrase RadiationResistancePhrase = new Translate.Phrase("mod.radiationresistance", "Radiation Resistance");

	public static Translate.Phrase RadiationExposureResistancePhrase = new Translate.Phrase("mod.radiationexposureresistance", "Radiation Exposure Resistance");

	public static Translate.Phrase MaxHealthPhrase = new Translate.Phrase("mod.maxhealth", "Max Health");

	public static Translate.Phrase ScrapYieldPhrase = new Translate.Phrase("mod.scrapyield", "Scrap Yield");

	public static Translate.Phrase MoveSpeedPhrase = new Translate.Phrase("mod.movespeed", "Movement Speed");

	public static Translate.Phrase ObscureVisionPhrase = new Translate.Phrase("mod.ObscureVision", "Obscure Vision");

	public static Translate.Phrase RadiationPhrase = new Translate.Phrase("mod.radiation", "Radiation");

	public static Translate.Phrase CraftingQualityPhrase = new Translate.Phrase("mod.craftingquality", "Crafting Quality");

	public static Translate.Phrase WarmingPhrase = new Translate.Phrase("mod.warming", "Warming");

	public static Translate.Phrase CoolingPhrase = new Translate.Phrase("mod.cooling", "Cooling");

	public static Translate.Phrase CoreTempMinPhrase = new Translate.Phrase("mod.coretempmin", "Min Temp");

	public static Translate.Phrase CoreTempMaxPhrase = new Translate.Phrase("mod.coretempmax", "Max Temp");

	public static Translate.Phrase VisionCarePhrase = new Translate.Phrase("mod.VisionCare", "Vision Care");

	public static Translate.Phrase MetabolismBoosterPhrase = new Translate.Phrase("mod.MetabolismBooster", "Metabolism Booster");

	public static Translate.Phrase HarvestingPhrase = new Translate.Phrase("mod.Harvesting", "Harvesting");

	public static Translate.Phrase DigestionBoostPhrase = new Translate.Phrase("mod.DigestionBoost", "Digestion Boost");

	public static Translate.Phrase FishingBoostPhrase = new Translate.Phrase("mod.FishingBoost", "Fishing Boost");

	public static Translate.Phrase CollectibleYieldPhrase = new Translate.Phrase("mod.CollectibleDoubleYield", "Double Yield Chance");

	public static Translate.Phrase Farming_BetterGenesPhrase = new Translate.Phrase("mod.Farming_BetterGenes", "Better Genes Chance");

	public static Translate.Phrase HorseGallopSpeedPhrase = new Translate.Phrase("mod.HorseGallopSpeed", "Horse Gallop Speed");

	public static Translate.Phrase ComfortPhrase = new Translate.Phrase("mod.Comfort", "Comfort");

	public static Translate.Phrase ClottingPhrase = new Translate.Phrase("mod.Clotting", "Clotting");

	public static Translate.Phrase Temperature = new Translate.Phrase("mod.temperature", "Temperature: ");

	public static Translate.Phrase MinTemp = new Translate.Phrase("mod.mintemp", "Min temperature: ");

	public static Translate.Phrase MaxTemp = new Translate.Phrase("mod.maxtemp", "Max temperature: ");

	public static Translate.Phrase HunterVisionPhrase = new Translate.Phrase("mod.huntervision", "Hunter Vision");

	public static Translate.Phrase Farming_BetterGenesPanelPhrase = new Translate.Phrase("mod.Farming_BetterGenes.panel", "Increase");

	public ModifierType Type { get; private set; }

	public ModifierSource Source { get; private set; }

	public float Value { get; private set; } = 1f;


	public float Duration { get; private set; } = 10f;


	public double TimeRemaining { get; private set; }

	public bool Expired { get; private set; }

	public void Init(ModifierType type, ModifierSource source, float value, float duration, double remaining)
	{
		Type = type;
		Source = source;
		Value = value;
		Duration = duration;
		Expired = false;
		TimeRemaining = remaining;
	}

	public void Tick(BaseCombatEntity ownerEntity, double delta)
	{
		TimeRemaining -= delta;
		Expired = Duration > 0f && TimeRemaining <= 0.0;
	}

	public ProtoBuf.Modifier Save()
	{
		ProtoBuf.Modifier modifier = Pool.Get<ProtoBuf.Modifier>();
		modifier.type = (int)Type;
		modifier.source = (int)Source;
		modifier.value = Value;
		modifier.timeRemaining = TimeRemaining;
		modifier.duration = Duration;
		return modifier;
	}

	public void Load(ProtoBuf.Modifier m)
	{
		Type = (ModifierType)m.type;
		Source = (ModifierSource)m.source;
		Value = m.value;
		TimeRemaining = m.timeRemaining;
		Duration = m.duration;
	}

	public static Translate.Phrase GetPhraseForModType(ModifierType type)
	{
		switch (type)
		{
		case ModifierType.Wood_Yield:
			return WoodYieldPhrase;
		case ModifierType.Ore_Yield:
			return OreYieldPhrase;
		case ModifierType.Radiation_Resistance:
			return RadiationResistancePhrase;
		case ModifierType.Radiation_Exposure_Resistance:
			return RadiationExposureResistancePhrase;
		case ModifierType.Max_Health:
			return MaxHealthPhrase;
		case ModifierType.Scrap_Yield:
			return ScrapYieldPhrase;
		case ModifierType.MoveSpeed:
			return MoveSpeedPhrase;
		case ModifierType.ObscureVision:
			return ObscureVisionPhrase;
		case ModifierType.Crafting_Quality:
			return CraftingQualityPhrase;
		case ModifierType.Warming:
			return WarmingPhrase;
		case ModifierType.Cooling:
			return CoolingPhrase;
		case ModifierType.CoreTemperatureMinAdjustment:
			return CoreTempMinPhrase;
		case ModifierType.CoreTemperatureMaxAdjustment:
			return CoreTempMaxPhrase;
		case ModifierType.VisionCare:
			return VisionCarePhrase;
		case ModifierType.MetabolismBooster:
			return MetabolismBoosterPhrase;
		case ModifierType.Harvesting:
			return HarvestingPhrase;
		case ModifierType.DigestionBoost:
		case ModifierType.HorseDungProductionBoost:
			return DigestionBoostPhrase;
		case ModifierType.FishingBoost:
			return FishingBoostPhrase;
		case ModifierType.Collectible_DoubleYield:
			return CollectibleYieldPhrase;
		case ModifierType.Farming_BetterGenes:
			return Farming_BetterGenesPhrase;
		case ModifierType.HorseGallopSpeed:
			return HorseGallopSpeedPhrase;
		case ModifierType.Comfort:
			return ComfortPhrase;
		case ModifierType.Clotting:
			return ClottingPhrase;
		case ModifierType.HunterVision:
			return HunterVisionPhrase;
		case ModifierType.Radiation:
			return RadiationPhrase;
		default:
			throw new ArgumentOutOfRangeException("type", type, $"Couldn't find a phrase for this modifier! {type}");
		}
	}

	public static Translate.Phrase GetPanelPhraseForModType(ModifierType type)
	{
		if (type == ModifierType.Farming_BetterGenes)
		{
			return Farming_BetterGenesPanelPhrase;
		}
		throw new ArgumentOutOfRangeException("type", type, $"Couldn't find a phrase for this modifier! {type}");
	}

	public static bool TryAppendModifierDescription(Modifier modifier, StringBuilder stringBuilder)
	{
		return TryAppendModifierDescription(modifier.Type, modifier.Value, stringBuilder);
	}

	public static bool TryAppendModifierDescription(ModifierType type, float value, StringBuilder stringBuilder)
	{
		switch (type)
		{
		case ModifierType.Warming:
			stringBuilder.Append(Temperature.translated);
			stringBuilder.Append("+");
			stringBuilder.Append(value);
			return true;
		case ModifierType.Cooling:
			stringBuilder.Append(Temperature.translated);
			stringBuilder.Append(value);
			return true;
		case ModifierType.CoreTemperatureMinAdjustment:
			stringBuilder.Append(MinTemp.translated);
			stringBuilder.Append(value);
			return true;
		case ModifierType.CoreTemperatureMaxAdjustment:
			stringBuilder.Append(MaxTemp.translated);
			stringBuilder.Append(value);
			return true;
		case ModifierType.Farming_BetterGenes:
			stringBuilder.Append(GetPhraseForModType(type).translated);
			return true;
		case ModifierType.HorseGallopSpeed:
		{
			if (value > 0f)
			{
				stringBuilder.Append("+");
			}
			float value2 = value * 60f * 60f / 1000f;
			stringBuilder.Append(value2);
			stringBuilder.Append("km/h ");
			return true;
		}
		default:
			return false;
		}
	}

	public bool IsHiddenModifier()
	{
		return Type == ModifierType.DigestionBoostTimeMod;
	}

	public bool HasNegativeSource()
	{
		if (Source != ModifierSource.Dart)
		{
			return Source == ModifierSource.NegativeEffect;
		}
		return true;
	}
}
