using Rust.UI;
using UnityEngine;

public class EggHuntNote : MonoBehaviour, IClientComponent
{
	public Canvas canvas;

	public CanvasGroup mainGroup;

	public CanvasGroup timerGroup;

	public RustText timerText;

	public SeasonalEventType EventType;

	public static readonly Translate.Phrase startsInPhrase = new Translate.Phrase("egghunt.start", "Starts in: {0}");
}
