using System.Collections.Generic;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UICompass : MonoBehaviour
{
	public Canvas compassCanvas;

	public CanvasGroup compassGroup;

	public RawImage compassStrip;

	public List<CompassMapMarker> CompassMarkers;

	public List<CompassMapMarker> TeamCompassMarkers;

	public List<CompassMissionMarker> MissionMarkers;

	public CompassMapMarker DeathMarker;

	public CompassMapMarker DeepSeaExitMarker;

	public CompassBagMarker SleepingBagMarker;

	public static readonly Translate.Phrase IslandInfoPhrase = new Translate.Phrase("nexus.compass.island_info", "Continue for {distance} to travel to {zone}");

	private static readonly int CompassScroll = Shader.PropertyToID("_CompassScroll");

	public RectTransform IslandInfoContainer;

	public RustText IslandInfoText;

	public float IslandInfoDistanceThreshold = 250f;

	public float IslandLookThreshold = -0.8f;

	public RectTransform IslandInfoFullContainer;

	public List<CompassMapMarker> LocalPings;

	public List<CompassMapMarker> TeamPings;

	public Image LeftPingPulse;

	public Image RightPingPulse;
}
