using Rust.UI;
using UnityEngine;

public class DartsGameScoreDisplayUI : FacepunchBehaviour
{
	[Header("Prefabs")]
	public DartsGameUIScoreRow ScoreRowPrefab;

	[Header("Hierarchy References")]
	public Transform RoundScoresPanel;

	public Transform GameScorePanel;

	public DartsGameUIScoreRow ScoreTargetRow;

	public RustText PlayerNameText;

	public GameObject GameOverPanel;

	public GameObject GameResultPanel;

	public RustText GameResultText;

	public int maxNumberOfRows = 10;

	[Header("What Player # is this panel for (0 indexed).")]
	public int PlayerIndex;
}
