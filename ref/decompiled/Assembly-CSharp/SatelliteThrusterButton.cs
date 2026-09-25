using Rust.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SatelliteThrusterButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public RustText labelText;

	public Button button;

	[Header("Colours")]
	public Color normalColor = Color.white;

	public Color firedColor = new Color(0.1f, 1f, 0.2f, 1f);

	public Color disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

	[Header("Hover")]
	public GameObject hoverFill;

	public Color hoverLabelColor = new Color(0.12668449f, 0.12668449f, 0.12668449f, 1f);

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}
}
