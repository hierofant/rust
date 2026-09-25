using UnityEngine;

public class EffectSilencerSelect : MonoBehaviour
{
	public GameObjectRef MilitaryEffect;

	public GameObjectRef OilFilterEffect;

	public GameObjectRef SodaCanEffect;

	public bool GetEffectForSilencerType(ProjectileWeaponMod.SilencerType silencerType, out GameObjectRef result)
	{
		result = null;
		switch (silencerType)
		{
		case ProjectileWeaponMod.SilencerType.Military:
			result = MilitaryEffect;
			break;
		case ProjectileWeaponMod.SilencerType.OilFilter:
			result = OilFilterEffect;
			break;
		case ProjectileWeaponMod.SilencerType.SodaCan:
			result = SodaCanEffect;
			break;
		}
		if (result != null)
		{
			return result.isValid;
		}
		return false;
	}
}
