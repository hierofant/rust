using Rust.UI;
using UnityEngine;

public class DartsGameDialogUI : UIDialog
{
	[Header("Prefabs")]
	public DartsGameUILeaderboardRow LeaderboardRowPrefab;

	[Header("Hierarchy References")]
	public RustSlider FocusSlider;

	public RustText FocusText;

	public RustSlider TimerSlider;

	public RustText TimerText;

	public CanvasGroup focusCanvasGroup;

	public Transform Leaderboard;

	public Transform LeaderboardPanel;

	public RustText LeaderboardTimeText;

	[Tooltip("On while the leaderboard has no entries.")]
	public GameObject LeaderboardNoEntries;

	[Tooltip("On while the leaderboard has at least one entry. The inverse of the above.")]
	public GameObject LeaderboardHasEntries;
}
