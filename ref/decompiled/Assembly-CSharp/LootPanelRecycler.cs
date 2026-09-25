using Rust.UI;
using UnityEngine;

public class LootPanelRecycler : LootPanel
{
	public GameObject controlsDisabled;

	public GameObject controlsOff;

	public GameObject controlsOn;

	public RustText recycler_stats;

	public Color goodStatsColor = new Color(0.584712f, 0.75f, 0.2922794f);

	public Color badStatsColor = new Color(41f / 51f, 0.254902f, 0.1686275f);
}
