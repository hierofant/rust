using UnityEngine;

namespace Facepunch.UI;

public class ESPCanvas : SingletonComponent<ESPCanvas>
{
	public Canvas canvas;

	[Tooltip("Amount of times per second we should update the visible panels")]
	public float RefreshRate = 5f;

	[Tooltip("This object will be duplicated in place")]
	public ESPPlayerInfo Source;

	[Header("Nameplate Properties")]
	public Gradient gradientNormal;

	public Gradient gradientTeam;

	public AccessibilityColourCollection TeamLookup;

	public AccessibilityColourCollection ClanLookup;

	public AccessibilityColourCollection AllyLookup;

	public AccessibilityColourCollection EnemyLookup;

	[ClientVar(ClientAdmin = true, Help = "Admin-only: Should the nameplates size dynamically change based on distance?")]
	public static bool NameplateAutosize;

	[ClientVar(ClientAdmin = true, Help = "Admin-only: Nameplate falloff distance for autosizing (smaller values cause the text to shrink at closer distances).")]
	public static float NameplateAutosize_Falloff = 50f;

	private static int NameplateCount = 32;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Admin-only: overrides the maximum distance at which ESP player info elements are shown; 0 = use default distance")]
	public static float OverrideMaxDisplayDistance = 0f;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Admin-only: when enabled, occlusion checks are skipped for ESP player info elements so they are always visible regardless of walls")]
	public static bool DisableOcclusionChecks = false;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Admin-only: when enabled, player health values are shown in ESP player info elements above each player")]
	public static bool ShowHealth = false;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Admin-only: when enabled, ESP player info elements are coloured by team membership using the configured team colour IDs")]
	public static bool ColourCodeTeams = false;

	[ClientVar(ClientAdmin = true, Help = "(Generated) Admin-only: when enabled, ESP player info team colours are randomised per team rather than using the configured team ID colour mapping")]
	public static bool UseRandomTeamColours = false;

	[ClientVar(ClientAdmin = true, Help = "Admin-only: when enabled, an icon will be displayed as part of ESP player info elements for each player that is currently communicating over in-game voice chat")]
	public static bool ShowVoip = false;

	[ClientVar(ClientAdmin = true, Help = "Max amount of nameplates to show at once")]
	public static int MaxNameplates
	{
		get
		{
			return NameplateCount;
		}
		set
		{
			NameplateCount = Mathf.Clamp(value, 16, 150);
		}
	}
}
