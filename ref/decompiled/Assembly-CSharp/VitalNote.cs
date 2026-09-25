using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VitalNote : MonoBehaviour, IClientComponent, IVitalNotice
{
	public enum Vital
	{
		Comfort,
		Radiation,
		Poison,
		Cold,
		Bleeding,
		Hot,
		Oxygen,
		Wet,
		Hygiene,
		Starving,
		Dehydration,
		Custom
	}

	public Vital VitalType;

	public FloatConditions showIf;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI valueText;

	public Image backgroundImage;

	public Image iconImage;
}
