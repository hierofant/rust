using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreBackground : BaseMonoBehaviour
{
	public RectTransform viewport;

	public RectTransform section;

	public Image backgroundImage;

	public float fadeRange = 400f;

	public float fadeSpeed = 5f;
}
