using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class RawImagePreserveAspect : MonoBehaviour, IClientComponent
{
	public RawImage rawImage;

	public RectTransform rectTransform;

	private Texture lastTexture;
}
