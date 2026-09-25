using UnityEngine;
using UnityEngine.UI;

public class AirfieldTerminalScreen : MonoBehaviour, INotifyLOD, IClientComponent
{
	public RawImage largeIcon;

	public Text mainText;

	public Text lowerText;

	public Text centerText;

	public CanvasGroup canvasGroup;
}
