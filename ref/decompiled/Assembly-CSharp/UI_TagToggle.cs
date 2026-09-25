using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_TagToggle : MonoBehaviour
{
	public bool IsOn;

	[Header("Components")]
	public Image Background;

	public Image Icon;

	public Image IconBackground;

	public RustText Text;

	public RustIcon rustIcon;

	[Space]
	public bool ShowAltTextWhenOff;

	public RustText TextAlt;

	[Header("Style")]
	public Color32 BackgroundOn;

	public Color32 BackgroundOff;

	public Color32 IconOn;

	public Sprite IconOnSprite;

	public Color32 IconOff;

	public Sprite IconOffSprite;

	public Color32 IconBackgroundOn;

	public Color32 IconBackgroundOff;

	public Color32 TextOn;

	public Color32 TextOff;
}
