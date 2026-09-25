using UnityEngine;
using UnityEngine.UI;

public class RHIBScreen : MonoBehaviour, IClientComponent
{
	[SerializeField]
	private ScrollRectEx _scrollRect;

	[SerializeField]
	private Image _backingImage;

	[SerializeField]
	private Canvas _canvas;
}
