using System.Collections.Generic;
using UnityEngine;

public class WearableNotifyConditionalClothing : WearableNotify
{
	public enum CheckClothingOption
	{
		under,
		over
	}

	public List<GameObject> MinClothing = new List<GameObject>();

	public List<GameObject> MaxClothing = new List<GameObject>();

	[Tooltip("Check succeeds if another clothing part can be found in this direction relative to the wearable with a matching occupation slot.\n\nExample: This will succeed if value is set to Under, wearable.occupationOver contains Face, and another clothing part occupationUnder also contains Face.")]
	public CheckClothingOption checkForClothing;

	[Tooltip("Check succeeds only if there is a slot match under/over the wearable with specifically any of these slots.")]
	public Wearable.OccupationSlots occupationSlot = (Wearable.OccupationSlots)(-1);
}
