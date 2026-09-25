using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_TimestampedConsoleRow : MonoBehaviour
{
	public RustFlexText TimestampText;

	public RustFlexText MessageText;

	public Image BackgroundImage;

	[Space]
	public Color LogColor;

	public Color ErrorColor;

	public Color BackgroundErrorColor;

	public Color WarningColor;

	public Color InputColor;
}
