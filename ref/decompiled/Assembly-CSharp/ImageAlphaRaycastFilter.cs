using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ImageAlphaRaycastFilter : UIBehaviour, ICanvasRaycastFilter
{
	[NonSerialized]
	private RawImage m_rawImage;

	public float rChannelHitTestMinimumThreshold = 1f;

	protected RawImage rawImage => m_rawImage ?? (m_rawImage = GetComponent<RawImage>());

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (rChannelHitTestMinimumThreshold <= 0f)
		{
			return true;
		}
		if (rChannelHitTestMinimumThreshold > 1f)
		{
			return false;
		}
		Texture2D texture2D = rawImage.mainTexture as Texture2D;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, screenPoint, eventCamera, out var localPoint))
		{
			return false;
		}
		Rect pixelAdjustedRect = rawImage.GetPixelAdjustedRect();
		localPoint.x += rawImage.rectTransform.pivot.x * pixelAdjustedRect.width;
		localPoint.y += rawImage.rectTransform.pivot.y * pixelAdjustedRect.height;
		localPoint = new Vector2(localPoint.x / pixelAdjustedRect.width, localPoint.y / pixelAdjustedRect.height);
		if (texture2D != null && !texture2D.isReadable)
		{
			return false;
		}
		try
		{
			return texture2D.GetPixelBilinear(localPoint.x, localPoint.y).r <= rChannelHitTestMinimumThreshold;
		}
		catch (UnityException ex)
		{
			Debug.LogError("Using alphaHitTestMinimumThreshold greater than 0 on Graphic whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
			return true;
		}
	}
}
