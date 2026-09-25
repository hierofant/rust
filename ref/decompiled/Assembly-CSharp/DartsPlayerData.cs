using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

public class DartsPlayerData : IDisposable
{
	public enum DartsPlayerState
	{
		None,
		StartingGame,
		InGame,
		EndingGame
	}

	public DartsPlayerState State;

	public int DartsThrown;

	public int DartsThrownThisTurn;

	public int Score;

	public int ScoreThisTurn;

	public int Turn;

	public float TimeTaken;

	public ulong UserID { get; private set; }

	public bool HasUser => UserID != 0;

	public BasePlayer Player
	{
		get
		{
			if (isServer)
			{
				if (UserID == 0L)
				{
					return null;
				}
				return BasePlayer.FindByID(UserID);
			}
			return null;
		}
	}

	public bool HasUserReadyToStart => State == DartsPlayerState.StartingGame;

	public bool HasUserInGame => State == DartsPlayerState.InGame;

	public bool HasUserEndGame => State == DartsPlayerState.EndingGame;

	public bool isServer { get; private set; }

	public int mountIndex { get; private set; }

	public List<int> scoreHistory { get; private set; } = new List<int>();


	public DartsPlayerData(bool isServer, int mountIndex)
	{
		this.isServer = isServer;
		this.mountIndex = mountIndex;
	}

	public void JoinGame(ulong userID)
	{
		ClearData();
		UserID = userID;
		State = DartsPlayerState.StartingGame;
	}

	public void ClearData()
	{
		UserID = 0uL;
		State = DartsPlayerState.None;
		DartsThrown = 0;
		DartsThrownThisTurn = 0;
		Score = 0;
		ScoreThisTurn = 0;
		Turn = 0;
		TimeTaken = 0f;
		scoreHistory.Clear();
	}

	public void LeaveGame(ulong userID)
	{
		if (HasUser && userID == UserID)
		{
			ClearData();
		}
	}

	public void Dispose()
	{
		ClearData();
	}

	public void Save(DartsGame msg)
	{
		DartsGame.DartsPlayerData dartsPlayerData = Pool.Get<DartsGame.DartsPlayerData>();
		dartsPlayerData.userid = UserID;
		dartsPlayerData.state = (int)State;
		dartsPlayerData.score = Score;
		dartsPlayerData.scoreThisTurn = ScoreThisTurn;
		dartsPlayerData.dartsThrown = DartsThrown;
		dartsPlayerData.dartsThrownThisTurn = DartsThrownThisTurn;
		dartsPlayerData.timeTaken = TimeTaken;
		dartsPlayerData.turn = Turn;
		dartsPlayerData.scoreHistory = Pool.Get<List<int>>();
		dartsPlayerData.scoreHistory.AddRange(scoreHistory);
		msg.players.Add(dartsPlayerData);
	}
}
