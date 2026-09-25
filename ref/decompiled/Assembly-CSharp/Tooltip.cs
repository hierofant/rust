using Rust.Localization;
using UnityEngine;

public class Tooltip : BaseMonoBehaviour, IClientComponent, ILocalize
{
	public enum DelayType
	{
		Short,
		Long
	}

	public static TooltipContainer Current;

	public GameObject TooltipObject;

	public Translate.Phrase phrase;

	[Space(10f)]
	[Header("Additional Settings - Delay")]
	[Tooltip("Delay timing before the tooltip appears. Short is 0.15 seconds, Long is 0.5 seconds.")]
	public DelayType delayBeforeAppearing;

	[Space(10f)]
	[Tooltip("What position relative to the object that the tooltip should be spawned on.")]
	[Header("Advanced Settings - Position")]
	public TooltipContainer.PositionMode positionMode;

	[Tooltip("Spawn the tooltip relative to the mouse position rather than the objects.")]
	public bool useMousePosition;

	[Tooltip("Anchor tooltip at the centre of the object rather than a specific side.")]
	public bool useCentre;

	[Tooltip("Use percentage of source width/Height for horizontal offset in Left/Right or Up/Down mode.")]
	public bool usePercentageOffset;

	[Tooltip("Percentage of the source rect width to offset (0.0 - 1.0).")]
	public float offsetPercent;

	[Tooltip("How far to spawn from the objects position")]
	public Vector2 offset = new Vector2(8f, 8f);

	private object[] localizationArguments;

	public string Text
	{
		get
		{
			return phrase.english;
		}
		set
		{
			phrase.english = value;
		}
	}

	public string LanguageToken => phrase.token;

	public string LanguageEnglish => phrase.english;
}
