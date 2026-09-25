using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_PoolTable : UIDialog, IShadowGroupVisibility
{
	private const int BallsPerPlayer = 7;

	private const int EightBallDisplayIndex = 7;

	[SerializeField]
	private RustSlider powerBar;

	[SerializeField]
	[Header("Instructions")]
	private GameObject holdInstruction;

	[SerializeField]
	private GameObject flickInstruction;

	[SerializeField]
	private GameObject cancelInstruction;

	[Header("Players")]
	[SerializeField]
	private RustText localPlayerNameText;

	[SerializeField]
	private RustText opponentNameText;

	[SerializeField]
	private RawImage localPlayerAvatar;

	[SerializeField]
	private RawImage opponentAvatar;

	[SerializeField]
	[Header("Current Turn")]
	private GameObject localTurnBall;

	[SerializeField]
	private GameObject localTurnHighlight;

	[SerializeField]
	private GameObject opponentTurnBall;

	[SerializeField]
	private GameObject opponentTurnHighlight;

	[Header("Ball Display")]
	[SerializeField]
	private Color localBallColour = new Color(0.11f, 0.37f, 0.58f, 1f);

	[SerializeField]
	private Color opponentBallColour = new Color(0.8f, 0.25f, 0.17f, 1f);

	[SerializeField]
	private Color eightBallColour = new Color(0.13f, 0.13f, 0.13f, 1f);

	[Tooltip("Contains the 15 ball displays in left-to-right order.")]
	[SerializeField]
	private Transform ballDisplayRoot;

	public bool ShouldUpdateShadows => true;
}
