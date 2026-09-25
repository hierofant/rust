using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_RegisterNavigation : MonoBehaviour
{
	public UI_MainMenuNavigationEntry NavigationEntry;

	public void Setup()
	{
		if (NavigationEntry != null)
		{
			SetupEntry();
		}
	}

	private void SetupEntry()
	{
		if (!(NavigationEntry.Reference == null) && NavigationEntry.Reference.TryGetComponent<UI_Page>(out var component))
		{
			NavigationEntry.Page = component;
		}
	}
}
