using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIBoatBuildingRequiredItemIcon : MonoBehaviour
{
	public Image Icon;

	public RustIcon Tick;

	public Image TileBG;

	public ItemDefinition ItemDef;

	public void Init(ItemDefinition itemDef)
	{
		ItemDef = itemDef;
		Icon.sprite = itemDef.iconSprite;
	}

	public void Highlight(bool flag)
	{
		if (flag)
		{
			TileBG.color = new Color(0.3647f, 0.4471f, 0.2235f, 1f);
			Icon.color = new Color(1f, 1f, 1f, 1f);
		}
		else
		{
			TileBG.color = new Color(0.788f, 0.753f, 0.722f, 0.1f);
			Icon.color = new Color(1f, 1f, 1f, 0.5f);
		}
		Tick.enabled = flag;
	}

	public void SetVisible(bool flag)
	{
		base.gameObject.SetActive(flag);
	}
}
