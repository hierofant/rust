using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreCarrouselButton : MonoBehaviour
{
	public RustButton button;

	[Space]
	public RustText titleText;

	public RustText subtitleText;

	public CoverImage coverImage;

	public GameObject videoIcon;

	[Space]
	public GameObject gaugeParent;

	public Image gaugeImage;

	public GameObject variantGroup;

	public RustText variantText;

	[Space]
	public GameObject skinViewerGroup;

	public HttpImage httpImage;

	public void UpdateGauge(float fillAmount)
	{
		if (fillAmount != 0f && !gaugeParent.activeInHierarchy)
		{
			gaugeParent.SetActive(value: true);
		}
		else if (fillAmount == 0f && gaugeParent.activeInHierarchy)
		{
			gaugeParent.SetActive(value: false);
		}
		gaugeImage.fillAmount = fillAmount;
	}

	public void Init(UI_StoreItemOverlayPage.PageElement element, UI_StoreItemOverlayPage page)
	{
		if (!element.UseSkinViewer)
		{
			if (element.Name != null && !string.IsNullOrEmpty(element.Name.english))
			{
				titleText.SetPhrase(element.Name);
			}
			else if (element.Item != null)
			{
				titleText.SetPhrase(element.Item.displayName);
			}
		}
		skinViewerGroup.SetActive(element.UseSkinViewer);
		videoIcon.SetActive(element.isVideo);
		string text = element.GalleryImageUrl ?? element.FullscreenImageUrl;
		if (!string.IsNullOrEmpty(text) && httpImage != null)
		{
			httpImage.enabled = true;
			httpImage.Load(text);
		}
		else
		{
			httpImage.enabled = false;
			coverImage.texture = null;
			Sprite sprite = ((element.GallerySprite == null) ? element.FullscreenSprite : element.GallerySprite);
			Sprite sprite2 = null;
			if (sprite != null && page.SmallAtlas != null && sprite.rect.width > 512f && sprite.rect.height > 512f)
			{
				sprite2 = page.SmallAtlas.GetSprite(sprite.name);
			}
			coverImage.sprite = ((sprite2 != null) ? sprite2 : sprite);
		}
		variantGroup.SetActive(element.VariantCount > 0);
		variantText.SetText(element.VariantCount.ToString());
	}

	public void Init(string imageURL)
	{
		if (httpImage != null)
		{
			httpImage.Load(imageURL);
		}
	}
}
