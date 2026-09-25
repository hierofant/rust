using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreFeaturedTab : UI_StoreTabBase
{
	[Space]
	[SerializeField]
	private UI_StoreCountdown countdown;

	[SerializeField]
	private List<UI_StoreItemGrid> gridSpawnOrder = new List<UI_StoreItemGrid>();
}
