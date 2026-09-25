using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class CraftingQueueIcon : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	public Image icon;

	public Image iconCancel;

	[Space]
	public GameObject timeLeft;

	public GameObject craftingCount;

	public RustText timeLeftText;

	public RustText craftingCountText;
}
