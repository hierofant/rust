using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_Hero_QuickJoin : UI_Hero_InfoBox
{
	[SerializeField]
	private List<UI_ServerEntry> _quickJoinEntries = new List<UI_ServerEntry>();
}
