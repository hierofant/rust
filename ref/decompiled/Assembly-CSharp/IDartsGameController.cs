using System;
using System.Collections.Generic;
using ProtoBuf;

public interface IDartsGameController : IDisposable
{
	public enum DartsGameState
	{
		NotPlaying,
		PreGame,
		InGame,
		PostGame
	}

	DartsGameBoard Board { get; }

	DartsGameState State { get; }

	bool HasGameInProgress => State >= DartsGameState.PreGame;

	bool IsGameStarting => State == DartsGameState.PreGame;

	bool IsGameOngoing => State == DartsGameState.InGame;

	bool IsGameEnding => State == DartsGameState.PostGame;

	List<DartsPlayerData> PlayerData { get; }

	int activePlayerIndex { get; }

	bool CanPlay(BasePlayer player);

	bool IsAtBoard(BasePlayer player);

	bool IsPlayersTurn(BasePlayer player);

	bool IsInGame(BasePlayer player);

	bool IsInGame(ulong userID);

	DartsPlayerData GetActivePlayerData();

	void Save(DartsGame syncData);

	void JoinGame(BasePlayer player);

	void JoinGame(ulong userID);

	void LeaveGame(BasePlayer player);

	void LeaveGame(ulong userID);

	void ForceLeaveGame();

	void StartPreGame();

	void StartGame();

	void StartPostGame();

	void EndGame();

	void ServerReceivedPlayerDartThrow(BasePlayer player, int points, int pointsModifier);

	void ServerReceivedUpdatedTimer(BasePlayer player, float timeTaken);
}
