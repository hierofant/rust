using UnityEngine;
using UnityEngine.UI;

public class ItemModifiableStatValue : MonoBehaviour
{
	public Text text;

	public Slider slider;

	public Slider positiveModificationSlider;

	public Slider negativeModificationSlider;

	public Tooltip modificationNumbersTooltip;

	public bool smallerIsBetter;

	public bool asPercentage;
}
