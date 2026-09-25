using System.Collections.Generic;
using System.Linq;
using Rust.UI.MainMenu;
using UnityEngine;

public class KeybindCategory : MonoBehaviour
{
	private List<UI_SettingsTweakKeyBind> keybinds = new List<UI_SettingsTweakKeyBind>();

	private void Awake()
	{
		for (int i = base.transform.GetSiblingIndex() + 1; i < base.transform.parent.childCount; i++)
		{
			Transform child = base.transform.parent.GetChild(i);
			if (!(child.GetComponent<KeybindCategory>() != null))
			{
				UI_SettingsTweakKeyBind component = child.GetComponent<UI_SettingsTweakKeyBind>();
				if (!(component == null))
				{
					keybinds.Add(component);
				}
				continue;
			}
			break;
		}
	}

	public void UpdateVisibility()
	{
		base.gameObject.SetActive(keybinds.Any((UI_SettingsTweakKeyBind x) => x.isActiveAndEnabled));
	}
}
