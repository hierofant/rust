using System;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreTakeover : MonoBehaviour
{
	public bool ignoreFeaturingTakeovers;

	public ItemStoreTakeover[] Takeovers = Array.Empty<ItemStoreTakeover>();
}
