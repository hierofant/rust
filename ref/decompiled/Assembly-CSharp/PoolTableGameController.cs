using Facepunch;
using ProtoBuf;
using UnityEngine;

public class PoolTableGameController
{
	public enum GameState
	{
		NotPlaying,
		WaitingForShot,
		BallsMoving,
		GameOver,
		WaitingForPlayers
	}

	public enum BallGroup
	{
		None,
		Solids,
		Stripes
	}

	private readonly ulong[] playerIds = new ulong[2];

	private readonly BallGroup[] playerGroups = new BallGroup[2];

	private int currentPlayerIndex;

	private PooledList<int> pocketedThisShot;

	private readonly Pooltable owner;

	public const int CueBallId = 0;

	public const int EightBallId = 8;

	private static readonly Translate.Phrase PottedYou = new Translate.Phrase("pooltable.potted.you", "You potted a ball");

	private static readonly Translate.Phrase PottedOpponent = new Translate.Phrase("pooltable.potted.opponent", "Opponent potted a ball");

	private static readonly Translate.Phrase PottedCueYou = new Translate.Phrase("pooltable.potted.cue.you", "You potted the cue ball");

	private static readonly Translate.Phrase PottedCueOpponent = new Translate.Phrase("pooltable.potted.cue.opponent", "Opponent potted the cue ball");

	private static readonly Translate.Phrase PottedWrongYou = new Translate.Phrase("pooltable.potted.wrong.you", "You potted your opponent's ball");

	private static readonly Translate.Phrase PottedWrongOpponent = new Translate.Phrase("pooltable.potted.wrong.opponent", "Opponent potted your ball");

	private static readonly Translate.Phrase ScratchYou = new Translate.Phrase("pooltable.scratch.you", "Scratch! Turn passes");

	private static readonly Translate.Phrase ScratchOpponent = new Translate.Phrase("pooltable.scratch.opponent", "Opponent scratched - your turn");

	private static readonly Translate.Phrase WinYou = new Translate.Phrase("pooltable.win", "You win!");

	private static readonly Translate.Phrase LoseYou = new Translate.Phrase("pooltable.lose", "You lose");

	private static readonly Translate.Phrase SoloWin = new Translate.Phrase("pooltable.solo.win", "{0} win!");

	private static readonly Translate.Phrase YourTurn = new Translate.Phrase("pooltable.turn.you", "Your turn");

	private static readonly Translate.Phrase YourTurnGroup = new Translate.Phrase("pooltable.turn.you.group", "Your turn ({0})");

	private static readonly Translate.Phrase OpponentTurn = new Translate.Phrase("pooltable.turn.opponent", "Opponent's turn");

	private static readonly Translate.Phrase JoinedYou = new Translate.Phrase("pooltable.join.you", "You joined as {0}");

	private static readonly Translate.Phrase JoinedOther = new Translate.Phrase("pooltable.join.other", "{0} joined as {1}");

	public GameState State { get; private set; }

	public uint ShotId { get; private set; }

	public bool HasGame => State != GameState.NotPlaying;

	public int CurrentIndex => currentPlayerIndex;

	public bool IsSoloGame { get; private set; }

	public ulong CurrentPlayerId => playerIds[currentPlayerIndex];

	public ulong PlayerId(int index)
	{
		return playerIds[index];
	}

	public BallGroup PlayerGroup(int index)
	{
		return playerGroups[index];
	}

	public PoolTableGameController(Pooltable owner)
	{
		this.owner = owner;
		pocketedThisShot = Pool.Get<PooledList<int>>();
		DebugLog("Created controller");
	}

	~PoolTableGameController()
	{
		if (pocketedThisShot != null)
		{
			Pool.Free(ref pocketedThisShot);
		}
	}

	public void StartNewGame(ulong hostId, bool solo)
	{
		if (hostId != 0L && !HasGame)
		{
			playerIds[0] = hostId;
			playerIds[1] = (solo ? hostId : 0);
			IsSoloGame = solo;
			StartGame(!solo);
			AnnounceJoin(0);
			DebugLog($"StartNewGame host={hostId} solo={solo}");
		}
	}

	public bool CanJoinAsSecondPlayer(ulong playerId)
	{
		if (State == GameState.WaitingForPlayers && playerId != 0L && playerId != playerIds[0])
		{
			return playerIds[1] == 0;
		}
		return false;
	}

	public bool AddSecondPlayer(ulong playerId)
	{
		if (!CanJoinAsSecondPlayer(playerId))
		{
			return false;
		}
		playerIds[1] = playerId;
		IsSoloGame = false;
		State = GameState.WaitingForShot;
		currentPlayerIndex = 0;
		if (owner.isServer)
		{
			AnnounceJoin(1);
			owner.ClientRPC(RpcTarget.NetworkGroup("ClientOnGameStarted"), playerIds[0], playerIds[1]);
			owner.SendNetworkUpdateImmediate();
			NotifyTurnStarted();
		}
		DebugLog($"AddSecondPlayer player={playerId} -> begin play");
		return true;
	}

	public bool OnPlayerJoined(ulong playerId)
	{
		if (playerId == 0L)
		{
			return false;
		}
		DebugLog($"OnPlayerJoined player={playerId}");
		if (!HasGame)
		{
			playerIds[0] = playerId;
			playerIds[1] = 0uL;
			IsSoloGame = false;
			StartGame();
			AnnounceJoin(0);
			DebugLog($"Player 1 joined player={playerId}");
			return true;
		}
		int num = currentPlayerIndex;
		if (playerIds[num] == playerId)
		{
			return true;
		}
		if (playerIds[num] == 0L)
		{
			playerIds[num] = playerId;
			IsSoloGame = playerIds[1 - num] == playerId;
			if (owner.isServer)
			{
				owner.SendNetworkUpdate();
			}
			AnnounceJoin(num);
			DebugLog($"Player {num + 1} joined player={playerId}");
			return true;
		}
		DebugLog($"Join rejected player={playerId} (not their turn)");
		return false;
	}

	public void OnPlayerLeft(ulong playerId)
	{
		DebugLog($"OnPlayerLeft player={playerId}");
		for (int i = 0; i < 2; i++)
		{
			if (playerIds[i] == playerId)
			{
				playerIds[i] = 0uL;
				if (HasGame)
				{
					EndGame(-1);
				}
				else if (owner.isServer)
				{
					owner.SendNetworkUpdate();
				}
				DebugLog($"Player removed from slot={i}");
				break;
			}
		}
	}

	private void AnnounceJoin(int slot)
	{
		if (owner == null || !owner.isServer)
		{
			return;
		}
		ulong num = playerIds[slot];
		if (num != 0L)
		{
			string text = ((slot == 0) ? "Player 1" : "Player 2");
			Toast(num, GameTip.Styles.Blue_Normal, JoinedYou, text);
			ulong num2 = playerIds[1 - slot];
			if (num2 != 0L && num2 != num)
			{
				BasePlayer basePlayer = BasePlayer.FindByID(num);
				string text2 = ((basePlayer != null) ? basePlayer.displayName : text);
				Toast(num2, GameTip.Styles.Blue_Normal, JoinedOther, text2, text);
			}
		}
	}

	public void Save(ProtoBuf.Pooltable proto)
	{
		proto.state = (int)State;
		proto.player1Id = playerIds[0];
		proto.player2Id = playerIds[1];
		proto.currentPlayer = currentPlayerIndex;
		proto.player1Group = (int)playerGroups[0];
		proto.player2Group = (int)playerGroups[1];
		proto.isSoloGame = IsSoloGame;
		proto.shotId = ShotId;
	}

	public void Load(ProtoBuf.Pooltable proto)
	{
		if (proto != null)
		{
			State = (GameState)proto.state;
			playerIds[0] = proto.player1Id;
			playerIds[1] = proto.player2Id;
			currentPlayerIndex = proto.currentPlayer;
			playerGroups[0] = (BallGroup)proto.player1Group;
			playerGroups[1] = (BallGroup)proto.player2Group;
			IsSoloGame = proto.isSoloGame;
			ShotId = proto.shotId;
		}
	}

	public void ResetToInitialState()
	{
		State = GameState.NotPlaying;
		playerIds[0] = 0uL;
		playerIds[1] = 0uL;
		playerGroups[0] = BallGroup.None;
		playerGroups[1] = BallGroup.None;
		currentPlayerIndex = 0;
		IsSoloGame = false;
		ShotId = 0u;
		pocketedThisShot.Clear();
		DebugLog("ResetToInitialState");
	}

	private void StartGame(bool waitingForPlayers = false)
	{
		playerGroups[0] = BallGroup.None;
		playerGroups[1] = BallGroup.None;
		currentPlayerIndex = 0;
		ShotId = 0u;
		State = ((!waitingForPlayers) ? GameState.WaitingForShot : GameState.WaitingForPlayers);
		pocketedThisShot.Clear();
		if (owner.isServer)
		{
			owner.ClientRPC(RpcTarget.NetworkGroup("ClientOnGameStarted"), playerIds[0], playerIds[1]);
			owner.SendNetworkUpdateImmediate();
			if (!waitingForPlayers)
			{
				NotifyTurnStarted();
			}
		}
		DebugLog($"StartGame waitingForPlayers={waitingForPlayers}");
	}

	private void EndGame(int winnerIndex)
	{
		State = GameState.GameOver;
		ulong num = ((winnerIndex >= 0) ? playerIds[winnerIndex] : 0);
		if (owner.isServer)
		{
			owner.SendNetworkUpdateImmediate();
			owner.ClientRPC(RpcTarget.NetworkGroup("ClientOnGameEnded"), num);
			if (winnerIndex >= 0)
			{
				owner.PlayWinEffect();
				if (IsSoloGame)
				{
					Toast(playerIds[winnerIndex], GameTip.Styles.Blue_Normal, SoloWin, GroupName(playerGroups[winnerIndex]));
				}
				else
				{
					Toast(playerIds[winnerIndex], GameTip.Styles.Blue_Normal, WinYou);
					Toast(playerIds[1 - winnerIndex], GameTip.Styles.Red_Normal, LoseYou);
				}
			}
		}
		DebugLog($"EndGame winnerIndex={winnerIndex} winnerId={num}");
	}

	public bool CanMount(ulong playerId)
	{
		if (playerId == 0L)
		{
			return false;
		}
		if (!HasGame)
		{
			return false;
		}
		if (State == GameState.WaitingForPlayers)
		{
			return false;
		}
		if (State != GameState.WaitingForShot)
		{
			return false;
		}
		if (playerIds[currentPlayerIndex] == 0L)
		{
			return true;
		}
		return CurrentPlayerId == playerId;
	}

	public bool IsParticipant(ulong playerId)
	{
		if (playerId != 0L)
		{
			if (playerIds[0] != playerId)
			{
				return playerIds[1] == playerId;
			}
			return true;
		}
		return false;
	}

	public bool CanShoot(ulong playerId)
	{
		bool flag = State == GameState.WaitingForShot && CurrentPlayerId == playerId;
		DebugLog($"CanShoot player={playerId} result={flag}");
		return flag;
	}

	public void OnShotFired()
	{
		if (State == GameState.WaitingForShot)
		{
			pocketedThisShot.Clear();
			State = GameState.BallsMoving;
			ShotId++;
			DebugLog($"OnShotFired -> BallsMoving (shotId={ShotId})");
		}
	}

	public void OnBallPocketed(int ballId)
	{
		if (State == GameState.BallsMoving)
		{
			pocketedThisShot.Add(ballId);
			DebugLog($"OnBallPocketed ball={ballId}");
			if (owner != null && owner.isServer)
			{
				ToastShooterOpponent(PocketToastPhrase(ballId, forShooter: true), PocketToastStyle(ballId), PocketToastPhrase(ballId, forShooter: false), GameTip.Styles.Red_Normal);
			}
		}
	}

	public void OnBallsStopped()
	{
		if (State == GameState.BallsMoving)
		{
			DebugLog("OnBallsStopped");
			if (owner != null && owner.isServer)
			{
				EvaluateShot();
			}
		}
	}

	private void Toast(ulong playerId, GameTip.Styles style, Translate.Phrase phrase, params string[] args)
	{
		if (Pooltable.show_tooltips && playerId != 0L)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(playerId);
			if (basePlayer != null)
			{
				basePlayer.ShowToast(style, phrase, overlay: false, args);
			}
		}
	}

	private void ToastShooterOpponent(Translate.Phrase shooterPhrase, GameTip.Styles shooterStyle, Translate.Phrase opponentPhrase, GameTip.Styles opponentStyle)
	{
		ulong num = playerIds[currentPlayerIndex];
		ulong num2 = playerIds[1 - currentPlayerIndex];
		Toast(num, shooterStyle, shooterPhrase);
		if (num2 != num)
		{
			Toast(num2, opponentStyle, opponentPhrase);
		}
	}

	private GameTip.Styles PocketToastStyle(int ballId)
	{
		if (IsCueBall(ballId))
		{
			return GameTip.Styles.Red_Normal;
		}
		BallGroup shotPlayerGroup = GetShotPlayerGroup();
		if (IsEightBall(ballId))
		{
			if (!IsGroupCleared(shotPlayerGroup))
			{
				return GameTip.Styles.Red_Normal;
			}
			return GameTip.Styles.Blue_Normal;
		}
		if (GroupOf(ballId) != shotPlayerGroup)
		{
			return GameTip.Styles.Red_Normal;
		}
		return GameTip.Styles.Blue_Normal;
	}

	private Translate.Phrase PocketToastPhrase(int ballId, bool forShooter)
	{
		if (IsCueBall(ballId))
		{
			if (!forShooter)
			{
				return PottedCueOpponent;
			}
			return PottedCueYou;
		}
		BallGroup ballGroup = GroupOf(ballId);
		if (ballGroup != 0 && ballGroup != GetShotPlayerGroup())
		{
			if (!forShooter)
			{
				return PottedWrongOpponent;
			}
			return PottedWrongYou;
		}
		if (!forShooter)
		{
			return PottedOpponent;
		}
		return PottedYou;
	}

	private BallGroup GetShotPlayerGroup()
	{
		BallGroup ballGroup = playerGroups[currentPlayerIndex];
		if (ballGroup != 0)
		{
			return ballGroup;
		}
		foreach (int item in pocketedThisShot)
		{
			ballGroup = GroupOf(item);
			if (ballGroup != 0)
			{
				return ballGroup;
			}
		}
		return BallGroup.None;
	}

	private void NotifyTurnStarted()
	{
		ulong num = playerIds[currentPlayerIndex];
		ulong num2 = playerIds[1 - currentPlayerIndex];
		BallGroup ballGroup = playerGroups[currentPlayerIndex];
		if (ballGroup == BallGroup.None)
		{
			Toast(num, GameTip.Styles.Blue_Normal, YourTurn);
		}
		else
		{
			Toast(num, GameTip.Styles.Blue_Normal, YourTurnGroup, GroupName(ballGroup));
		}
		if (num2 != num)
		{
			Toast(num2, GameTip.Styles.Red_Normal, OpponentTurn);
		}
	}

	private static string GroupName(BallGroup group)
	{
		if (group != BallGroup.Stripes)
		{
			return "Solids";
		}
		return "Stripes";
	}

	private void EvaluateShot()
	{
		using (TimeWarning.New("PoolTableGameController.EvaluateShot"))
		{
			DebugLog("EvaluateShot begin");
			bool num = pocketedThisShot.Contains(0);
			bool flag = pocketedThisShot.Contains(8);
			if (num)
			{
				if (flag)
				{
					DebugLog("Foul: cue ball + eight ball pocketed -> opponent wins");
					EndGame(1 - currentPlayerIndex);
					return;
				}
				owner.RespotCueBallAfterFoul();
				DebugLog("Foul: cue ball pocketed -> respot + pass turn");
				ToastShooterOpponent(ScratchYou, GameTip.Styles.Red_Normal, ScratchOpponent, GameTip.Styles.Blue_Normal);
				PassTurn();
				return;
			}
			if (flag)
			{
				if (IsGroupCleared(playerGroups[currentPlayerIndex]))
				{
					DebugLog("Eight ball pocketed legally -> current player wins");
					EndGame(currentPlayerIndex);
				}
				else
				{
					DebugLog("Eight ball pocketed early -> opponent wins");
					EndGame(1 - currentPlayerIndex);
				}
				return;
			}
			bool flag2 = false;
			bool flag3 = false;
			foreach (int item in pocketedThisShot)
			{
				if (!IsCueBall(item) && !IsEightBall(item))
				{
					BallGroup ballGroup = GroupOf(item);
					if (playerGroups[currentPlayerIndex] == BallGroup.None && ballGroup != 0)
					{
						playerGroups[currentPlayerIndex] = ballGroup;
						playerGroups[1 - currentPlayerIndex] = OpponentGroupOf(ballGroup);
						DebugLog($"Groups assigned current={playerGroups[currentPlayerIndex]} opponent={playerGroups[1 - currentPlayerIndex]} from ball={item}");
						owner.ClientRPC(RpcTarget.NetworkGroup("ClientOnGroupsAssigned"), (int)playerGroups[0], (int)playerGroups[1]);
					}
					if (ballGroup == playerGroups[currentPlayerIndex])
					{
						flag2 = true;
					}
					else
					{
						flag3 = true;
					}
				}
			}
			if (flag2 && !flag3)
			{
				DebugLog("EvaluateShot result: keep turn");
				KeepTurn();
			}
			else
			{
				DebugLog("EvaluateShot result: pass turn");
				PassTurn();
			}
		}
	}

	private void PassTurn()
	{
		currentPlayerIndex = 1 - currentPlayerIndex;
		State = GameState.WaitingForShot;
		if (owner.isServer)
		{
			owner.SendNetworkUpdateImmediate();
		}
		NotifyTurnStarted();
		DebugLog("PassTurn");
	}

	private void KeepTurn()
	{
		State = GameState.WaitingForShot;
		if (owner.isServer)
		{
			owner.SendNetworkUpdateImmediate();
		}
		DebugLog("KeepTurn");
	}

	private bool IsSolid(int ballId)
	{
		if (ballId >= 1)
		{
			return ballId <= 7;
		}
		return false;
	}

	private bool IsStripe(int ballId)
	{
		if (ballId >= 9)
		{
			return ballId <= 15;
		}
		return false;
	}

	private bool IsEightBall(int id)
	{
		return id == 8;
	}

	private bool IsCueBall(int id)
	{
		return id == 0;
	}

	private BallGroup GroupOf(int ballId)
	{
		if (IsSolid(ballId))
		{
			return BallGroup.Solids;
		}
		if (IsStripe(ballId))
		{
			return BallGroup.Stripes;
		}
		return BallGroup.None;
	}

	private BallGroup OpponentGroupOf(BallGroup g)
	{
		return g switch
		{
			BallGroup.Solids => BallGroup.Stripes, 
			BallGroup.Stripes => BallGroup.Solids, 
			_ => BallGroup.None, 
		};
	}

	private bool IsGroupCleared(BallGroup group)
	{
		int num;
		switch (group)
		{
		case BallGroup.None:
			return false;
		default:
			num = 9;
			break;
		case BallGroup.Solids:
			num = 1;
			break;
		}
		int num2 = ((group == BallGroup.Solids) ? 7 : 15);
		for (int i = num; i <= num2; i++)
		{
			if (!owner.IsBallPocketed(i))
			{
				return false;
			}
		}
		return true;
	}

	private void DebugLog(string message)
	{
		if (Pooltable.debug_pool)
		{
			Debug.Log("[PoolTable][GC] " + message + " | " + BuildStateSnapshot());
		}
	}

	private string BuildStateSnapshot()
	{
		string pocketedShotBalls = GetPocketedShotBalls();
		string text = ((owner != null && owner.net != null) ? owner.net.ID.Value.ToString() : "n/a");
		return $"table={text} state={State} currentIndex={currentPlayerIndex} currentPlayer={CurrentPlayerId} p1={playerIds[0]}({playerGroups[0]}) p2={playerIds[1]}({playerGroups[1]}) solo={IsSoloGame} pocketedThisShot=[{pocketedShotBalls}]";
	}

	private string GetPocketedShotBalls()
	{
		if (pocketedThisShot == null || pocketedThisShot.Count == 0)
		{
			return "-";
		}
		string text = pocketedThisShot[0].ToString();
		for (int i = 1; i < pocketedThisShot.Count; i++)
		{
			text += $",{pocketedThisShot[i]}";
		}
		return text;
	}
}
