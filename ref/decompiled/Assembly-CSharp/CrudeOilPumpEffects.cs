using UnityEngine;

public class CrudeOilPumpEffects : ListComponent<CrudeOilPumpEffects>, IClientComponent
{
	public GameObject Target;

	public SoundDefinition PumpLeakSound;
}
