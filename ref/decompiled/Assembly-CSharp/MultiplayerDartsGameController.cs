using System;
using System.Collections.Generic;
using ConVar;
using ProtoBuf;

public class MultiplayerDartsGameController : IDartsGameController, IDisposable
{
	private int _activePlayerIndex;

	public DartsGameBoard Board { get; private set; }

	private bool IsServer => Board.isServer;

	private bool IsClient => Board.isClient;

	public IDartsGameController.DartsGameState State { get; private set; }

	public bool HasGameInProgress => State >= IDartsGameController.DartsGameState.PreGame;

	public bool IsGameStarting => State == IDartsGameController.DartsGameState.PreGame;

	public bool IsGameOngoing => State == IDartsGameController.DartsGameState.InGame;

	public bool IsGameEnding => State == IDartsGameController.DartsGameState.PostGame;

	public List<DartsPlayerData> PlayerData { get; private set; }

	public int activePlayerIndex
	{
		get
		{
			return _activePlayerIndex;
		}
		private set
		{
			_activePlayerIndex = value % 2;
		}
	}

	public DartsPlayerData GetActivePlayerData()
	{
		return PlayerData[activePlayerIndex];
	}

	public bool IsAtBoard(BasePlayer player)
	{
		if (Board.mountable != null)
		{
			return Board.mountable.GetMounted() == player;
		}
		return false;
	}

	public bool CanPlay(BasePlayer player)
	{
		switch (State)
		{
		case IDartsGameController.DartsGameState.NotPlaying:
			return true;
		case IDartsGameController.DartsGameState.PreGame:
		case IDartsGameController.DartsGameState.InGame:
			if (!IsInGame(player) || !IsPlayersTurn(player))
			{
				if (!PlayerData[activePlayerIndex].HasUser)
				{
					if (IsInGame(player))
					{
						return ConVar.DartsGame.debugCanPlayAgainstSelf;
					}
					return true;
				}
				return false;
			}
			return true;
		case IDartsGameController.DartsGameState.PostGame:
			return true;
		default:
			return false;
		}
	}

	public bool IsPlayersTurn(BasePlayer player)
	{
		if (HasGameInProgress)
		{
			return GetActivePlayerData().UserID == (ulong)player.userID;
		}
		return false;
	}

	public bool IsInGame(BasePlayer player)
	{
		return IsInGame(player.userID);
	}

	public bool IsInGame(ulong userID)
	{
		bool flag = false;
		foreach (DartsPlayerData playerDatum in PlayerData)
		{
			flag = flag || playerDatum.UserID == userID;
		}
		return flag;
	}

	public MultiplayerDartsGameController(DartsGameBoard board)
	{
		Board = board;
		State = IDartsGameController.DartsGameState.NotPlaying;
		PlayerData = new List<DartsPlayerData>();
		PlayerData.Add(new DartsPlayerData(IsServer, 0));
		PlayerData.Add(new DartsPlayerData(IsServer, 1));
	}

	public void Dispose()
	{
		State = IDartsGameController.DartsGameState.NotPlaying;
		PlayerData[0].Dispose();
		PlayerData[1].Dispose();
		PlayerData.Clear();
	}

	public void Save(ProtoBuf.DartsGame syncData)
	{
		if (IsServer)
		{
			PlayerData[0].Save(syncData);
			PlayerData[1].Save(syncData);
			syncData.state = (int)State;
			syncData.activePlayerIndex = activePlayerIndex;
		}
	}

	public void JoinGame(BasePlayer player)
	{
		JoinGame(player.userID);
	}

	public void JoinGame(ulong userID)
	{
		if (IsInGame(userID) && !ConVar.DartsGame.debugCanPlayAgainstSelf)
		{
			Board.DartsDebug($"[DartsGameController] Player with userID {userID} is already in the game, cannot join again");
			return;
		}
		Board.DartsDebug($"[DartsGameController] Player with userID {userID} is trying to join the game");
		if (GetActivePlayerData().HasUser)
		{
			Board.DartsDebug($"[DartsGameController] Active player slot is already filled by userID {GetActivePlayerData().UserID}");
			if (Board.mountable != null)
			{
				Board.mountable.DismountAllPlayers();
			}
			return;
		}
		GetActivePlayerData().JoinGame(userID);
		Board.DartsDebug($"[DartsGameController] Player with userID {userID} has joined the game as player: {activePlayerIndex}");
		if (!IsGameOngoing)
		{
			StartGame();
		}
		else
		{
			GetActivePlayerData().State = DartsPlayerData.DartsPlayerState.InGame;
		}
		Board.SendNetworkUpdate();
	}

	public void LeaveGame(BasePlayer player)
	{
		LeaveGame(player.userID);
	}

	public void LeaveGame(ulong userID)
	{
		if (IsInGame(userID))
		{
			DartsPlayerData dartsPlayerData = PlayerData.Find((DartsPlayerData playerData) => playerData.UserID == userID);
			if (dartsPlayerData != null)
			{
				Board.DartsDebug($"[DartsGameController] Player with userID {userID} is leaving the game");
				dartsPlayerData.LeaveGame(userID);
				EndGame();
			}
		}
	}

	public void ForceLeaveGame()
	{
		if (PlayerData != null && PlayerData.Count >= 2)
		{
			PlayerData[0]?.LeaveGame(PlayerData[0].UserID);
			PlayerData[1]?.LeaveGame(PlayerData[1].UserID);
			EndGame();
		}
	}

	public void StartPreGame()
	{
		Board.DartsDebug("[DartsGameController] Pre game starting");
		State = IDartsGameController.DartsGameState.PreGame;
		foreach (DartsPlayerData playerDatum in PlayerData)
		{
			if (playerDatum.HasUser)
			{
				playerDatum.State = DartsPlayerData.DartsPlayerState.StartingGame;
			}
		}
		Board.SendNetworkUpdate();
	}

	public void StartGame()
	{
		Board.DartsDebug("[DartsGameController] Game starting");
		State = IDartsGameController.DartsGameState.InGame;
		activePlayerIndex = 0;
		foreach (DartsPlayerData playerDatum in PlayerData)
		{
			if (playerDatum.HasUser)
			{
				playerDatum.State = DartsPlayerData.DartsPlayerState.InGame;
			}
		}
		Board.NewTurn();
		Board.SendNetworkUpdate();
	}

	public void StartPostGame()
	{
		Board.DartsDebug("[DartsGameController] Post game starting");
		State = IDartsGameController.DartsGameState.PostGame;
		foreach (DartsPlayerData playerDatum in PlayerData)
		{
			if (playerDatum.HasUser)
			{
				playerDatum.State = DartsPlayerData.DartsPlayerState.EndingGame;
			}
		}
		Board.SendNetworkUpdate();
	}

	public void EndGame()
	{
		if (State != 0)
		{
			Board.DartsDebug("[DartsGameController] Ending Game");
			State = IDartsGameController.DartsGameState.NotPlaying;
			PlayerData[0]?.LeaveGame(PlayerData[0].UserID);
			PlayerData[1]?.LeaveGame(PlayerData[1].UserID);
			Board.SendNetworkUpdate();
		}
	}

	public void ServerReceivedPlayerDartThrow(BasePlayer player, int points, int pointsModifier)
	{
		if (!IsGameOngoing || !CanPlay(player) || !IsAtBoard(player))
		{
			return;
		}
		DartsPlayerData activePlayerData = GetActivePlayerData();
		activePlayerData.DartsThrownThisTurn++;
		activePlayerData.DartsThrown++;
		if (points == DartsGameBoard.BullScore && pointsModifier == 2)
		{
			Board.SendBullseye();
		}
		bool flag = activePlayerData.DartsThrownThisTurn == 3;
		bool flag2 = false;
		activePlayerData.ScoreThisTurn += points * pointsModifier;
		if (activePlayerData.Score + activePlayerData.ScoreThisTurn > Board.scoreTarget)
		{
			activePlayerData.ScoreThisTurn = 0;
			flag = true;
			flag2 = true;
		}
		else if (activePlayerData.Score + activePlayerData.ScoreThisTurn == Board.scoreTarget)
		{
			if (pointsModifier == 2 || !ConVar.DartsGame.needDoubleToWin)
			{
				flag = false;
				activePlayerData.Score += activePlayerData.ScoreThisTurn;
				activePlayerData.scoreHistory.Add(activePlayerData.ScoreThisTurn);
				activePlayerData.ScoreThisTurn = 0;
				Board.DartsDebug($"[DartsGameController] Player {activePlayerData.UserID} has won the game with a score of {activePlayerData.Score}!");
				StartPostGame();
				return;
			}
			activePlayerData.ScoreThisTurn = 0;
			flag = true;
			flag2 = true;
		}
		else if (activePlayerData.Score + activePlayerData.ScoreThisTurn == Board.scoreTarget - 1 && ConVar.DartsGame.needDoubleToWin)
		{
			activePlayerData.ScoreThisTurn = 0;
			flag = true;
			flag2 = true;
		}
		if (flag)
		{
			activePlayerData.Score += activePlayerData.ScoreThisTurn;
			activePlayerData.scoreHistory.Add(activePlayerData.ScoreThisTurn);
			activePlayerData.ScoreThisTurn = 0;
			activePlayerData.DartsThrownThisTurn = 0;
			activePlayerData.Turn++;
			MoveToNextPlayersTurns();
		}
		else
		{
			Board.SendNetworkUpdate();
		}
		Board.DartsDebug($"[DartsGameController] Turn [{activePlayerData.Turn}] - Scored [{points * pointsModifier}] points (base: {points}, modifier: {pointsModifier}). Total score this turn: {activePlayerData.ScoreThisTurn}, Total score: {activePlayerData.Score}, Darts thrown this turn: {activePlayerData.DartsThrownThisTurn}, Total darts thrown: {activePlayerData.DartsThrown}, was a Bust: {flag2}");
	}

	public void MoveToNextPlayersTurns()
	{
		if (Board.mountable != null)
		{
			Board.mountable.DismountAllPlayers();
		}
		activePlayerIndex++;
		Board.DartsDebug($"[DartsGameController] Moving to next player's turn, player index: {activePlayerIndex}. Next player is userID {GetActivePlayerData().UserID}");
		Board.NewTurn();
		Board.SendNetworkUpdate();
	}

	public void ServerReceivedUpdatedTimer(BasePlayer player, float timeTaken)
	{
		if (IsInGame(player) && IsPlayersTurn(player) && IsAtBoard(player))
		{
			GetActivePlayerData().TimeTaken += timeTaken;
			Board.DartsDebug($"[DartsGameController] Updating server timer with time taken: {timeTaken}");
			Board.SendNetworkUpdate();
		}
	}
}
