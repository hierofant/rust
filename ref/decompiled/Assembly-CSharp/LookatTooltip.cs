using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class LookatTooltip : MonoBehaviour
{
	public static bool Enabled = true;

	[NonSerialized]
	public BaseEntity currentlyLookingAt;

	public RustText textLabel;

	public RustText moreOptionsLabel;

	public Image icon;

	public Translate.Phrase moreOptionsDefaultPhrase;

	public CanvasGroup canvasGroup;

	public CanvasGroup infoGroup;

	public CanvasGroup minimiseGroup;

	[ClientVar(Saved = true, Help = "Changes the interaction crosshair visuals (0 = default, 1 = no texts, 2 = no texts and no icon)")]
	public static int crosshairMode = 0;
}
