using System;
using UnityEngine;

public class LEDScreenScroller : MonoBehaviour, IClientComponent, INotifyLOD
{
	[Serializable]
	public struct CharacterGlyph
	{
		public char Character;

		public Mesh Mesh;
	}

	[Header("Glyphs")]
	[Tooltip("One pre-made mesh plane per supported letter/number, UV mapped into the shared atlas. Matching is case-insensitive.")]
	public CharacterGlyph[] Glyphs;

	[Tooltip("Shown for spaces and any character with no entry in Glyphs (e.g. punctuation).")]
	public Mesh BlankGlyph;

	[Tooltip("Shared material/atlas used by every glyph slot.")]
	public Material GlyphMaterial;

	[Header("Layout")]
	[Tooltip("Horizontal distance between glyph centers, in local units.")]
	public float GlyphSlotWidth = 0.1f;

	[Tooltip("Visible width of the LED screen, in local units. Determines how many glyph slots are pooled.")]
	public float ScreenWidth = 1f;

	[Tooltip("Extra blank glyph slots inserted between repeats of the text so it doesn't run into itself when it loops.")]
	[Range(0f, 12f)]
	public int LoopGapSlots = 3;

	[Header("Scrolling")]
	[Tooltip("Local units per second.")]
	public float ScrollSpeed = 0.05f;

	[Tooltip("Flip travel direction (left-to-right instead of right-to-left) without touching letter order.")]
	public bool ReverseDirection;

	[Tooltip("Quantizes the visual scroll position to multiples of this value, so the strip jumps between fixed positions instead of sliding smoothly - set to match the physical spacing between individual LEDs on the screen mesh. 0 = smooth, unstepped scroll.")]
	public float StepSize = 0.00933f;

	[Header("Text Source")]
	[Tooltip("Used verbatim if ReadStationNameFromBoomBox is false, and as a fallback whenever no station name can be resolved.")]
	public string OverrideText = "RUST RADIO";

	[Tooltip("Shown while a cassette is loaded, taking priority over any tuned station. Only used when ReadStationNameFromBoomBox is enabled.")]
	public string CassetteText = "Playing Cassette Tape";

	[Tooltip("When enabled, reads the tuned station's name off SourceBoomBox every frame (cheap - only does dictionary work when the tuned station actually changes).")]
	public bool ReadStationNameFromBoomBox = true;

	[Tooltip("Optional explicit reference. Auto-found via GetComponentInParent<BoomBox>() if left unset.")]
	public BoomBox SourceBoomBox;

	[Tooltip("Vertical size of the scene-view bounds gizmo only - purely visual, has no effect on clipping (which is horizontal-only).")]
	[Header("Editor Debug")]
	public float DebugGizmoHeight = 0.2f;
}
