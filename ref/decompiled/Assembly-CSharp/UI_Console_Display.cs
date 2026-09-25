using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

public class UI_Console_Display : FacepunchBehaviour
{
	public FlexElement consoleAbsoluteFlex;

	public FlexVirtualScroll virtualScroll;

	public ScrollRect scrollRect;

	[Range(0f, 64f)]
	public float UpdateTickRate;

	[Header("Realm Colours")]
	public Color ClientColour;

	public Color ServerColour;

	public Color SharedColour;

	public Color RconColour;

	[Space]
	public Color DescriptionColour;

	public Color DefaultValueColour;
}
