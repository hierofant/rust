using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreItemTileWeeklySkin : UI_StoreItemTile
{
	[SerializeField]
	private bool applyIconColor;

	[SerializeField]
	private bool animatedShimmers;

	[SerializeField]
	private Image coloredGradient;

	private static readonly Dictionary<IPlayerItemDefinition, Color> CachedGradientColors = new Dictionary<IPlayerItemDefinition, Color>();

	private static readonly int ShimmerStrength = Shader.PropertyToID("_ShimmerStrength");

	private static readonly int ShimmerSpeed = Shader.PropertyToID("_ShimmerSpeed");

	[SerializeField]
	private float idleShimmerStrength;

	[SerializeField]
	private float idleShimmerSpeed;

	[SerializeField]
	private float hoverShimmerStrength = 0.154f;

	[SerializeField]
	private float hoverShimmerSpeed = 0.03f;
}
