using System.Collections.Generic;
using ConVar;
using UnityEngine;

public class DartsGameMountable : BaseMountable
{
	[Header("DartsGameMountable")]
	public ViewModel DartsViewModel;

	public DartsGameAnimationSubSystem animationSubSystem;

	public Transform DartHeldProp;

	public Transform DartOffhandProp;

	public List<Renderer> DartPropRenderers;

	public List<Transform> OffhandDarts;

	public float hideDartPropForSeconds = 1f;

	public Color player1Colour;

	public Color player2Colour;

	private DartsGameBoard _gameBoard;

	public DartsGameBoard Board
	{
		get
		{
			if (_gameBoard == null)
			{
				_gameBoard = GetParentEntity() as DartsGameBoard;
			}
			return _gameBoard;
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		BasePlayer mounted = GetMounted();
		if (mounted != null && Board.GameController != null && Board.GameController.HasGameInProgress && ((!DartsGame.debugCanPlayAgainstSelf && !Board.GameController.IsInGame(mounted)) || (DartsGame.debugCanPlayAgainstSelf && Board.GameController.GetActivePlayerData().UserID == 0L)))
		{
			Board.GameController.JoinGame(mounted);
		}
		CancelInvoke(TimeoutGame);
		Board.SendNetworkUpdate();
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		Invoke(TimeoutGame, DartsGame.idleKickSeconds);
		Board.SendNetworkUpdate();
	}

	private void TimeoutGame()
	{
		Board.EndGame();
	}
}
