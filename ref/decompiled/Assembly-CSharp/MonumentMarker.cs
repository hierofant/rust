using Facepunch;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class MonumentMarker : MonoBehaviour
{
	public RustText text;

	public Image imageBackground;

	public Image image;

	public Color dayColor;

	public Color nightColor;

	public void Setup(LandmarkInfo info)
	{
		if (info.displayPhrase.IsValid())
		{
			text.SetPhrase(info.displayPhrase);
		}
		else
		{
			string str = GetFallbackName(info.transform.root);
			text.SetText(str, localized: false);
		}
		if (imageBackground != null)
		{
			if (info.mapIcon != null)
			{
				image.sprite = info.mapIcon;
				text.SetActive(active: false);
				imageBackground.SetActive(active: true);
			}
			else
			{
				text.SetActive(active: true);
				imageBackground.SetActive(active: false);
			}
		}
		SetNightMode(nightMode: false);
		static string GetFallbackName(Transform t)
		{
			GameObject item = t.gameObject;
			foreach (var (result, hashSet2) in World.SpawnedPrefabs)
			{
				if (hashSet2.Contains(item))
				{
					return result;
				}
			}
			return t.name;
		}
	}

	public void SetNightMode(bool nightMode)
	{
		Color color = (nightMode ? nightColor : dayColor);
		Color color2 = (nightMode ? dayColor : nightColor);
		if (text != null)
		{
			text.color = color;
		}
		if (image != null)
		{
			image.color = color;
		}
		if (imageBackground != null)
		{
			imageBackground.color = color2;
		}
	}
}
