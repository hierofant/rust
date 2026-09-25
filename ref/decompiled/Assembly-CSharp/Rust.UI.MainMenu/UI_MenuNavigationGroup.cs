using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_MenuNavigationGroup : UI_RustButtonGroup
{
	[Serializable]
	private class TabButton
	{
		public RustButton Button;

		public string Path;
	}

	[Header("Navigation Groups (IGNORE BUTTON GROUPS - Just Add Here)")]
	[SerializeField]
	private List<TabButton> _navigationGroups = new List<TabButton>();
}
