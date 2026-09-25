#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class DartsGameBoard : BaseCombatEntity
{
	public enum GameType
	{
		None,
		SinglePlayer,
		Multiplayer
	}

	[Header("Dependencies")]
	public GameObjectRef ReticlePrefab;

	public GameObjectRef DartPrefab;

	public GameObjectRef UIPrefab;

	[Header("References")]
	public List<DartsGameScoreDisplayUI> scoreDisplayUIs;

	public GameObjectRef winEffect;

	public ParticleSystemContainer bullseyeEffect;

	public SoundPlayer bullseyeEffectSound;

	[Header("Board Setup Parameters")]
	public Transform center;

	[Tooltip("Only used when determining rotation of darts loaded in for clients not playing the game")]
	public Transform simulatedThrowPoint;

	public float radius = 1f;

	public float bandSize = 0.1f;

	public float tripleBandRadiusOffset = 0.1f;

	public float bullseyeRadius = 0.1f;

	public float bullRadius = 0.1f;

	public float angleOffset = -1f;

	[Header("Gizmos")]
	public bool showGizmos;

	[Header("Throwing Reticle Options")]
	public AnimationCurve accuracyCurve = AnimationCurve.Constant(0f, 1f, 1f);

	public List<DartsGameLeaderboard.DartsGameLeaderboardEntry> Leaderboard;

	public static readonly int[] ScoreSlices = new int[20]
	{
		6, 13, 4, 18, 1, 20, 5, 12, 9, 14,
		11, 8, 16, 7, 19, 3, 17, 2, 15, 10
	};

	public static readonly int BullScore = 25;

	private bool _disposed;

	public IDartsGameController GameController;

	private SinglePlayerDartsGameController _singlePlayerDartsGameController;

	private MultiplayerDartsGameController _multiplayerDartsGameController;

	private EntityRef<DartsGameMountable> mountableRef;

	private DartsGameMountable dgm;

	[HideInInspector]
	public DartsGameReticle Reticle;

	private static Vector3 DartSyncResetPosition = Vector3.one * -999f;

	private int lastUsedDart;

	private Vector3 __sync_dartPosition0;

	private Vector3 __sync_dartPosition1;

	private Vector3 __sync_dartPosition2;

	public int scoreTarget => ConVar.DartsGame.scoreTarget;

	public float cooldownBetweenThrows => ConVar.DartsGame.cooldownBetweenThrows;

	public float holdFocusDuration => ConVar.DartsGame.holdFocusDuration;

	public float reticleSpawnPointRadiusOffset => ConVar.DartsGame.reticleSpawnPointRadiusOffset;

	public float reticleSpawnPointRadius => radius + reticleSpawnPointRadiusOffset;

	public float RadiusWithoutBands => radius - bullRadius - bandSize * 2f;

	public float EmptySpaceLength => RadiusWithoutBands / 2f;

	public GameType gameType { get; private set; }

	public bool isPlaying => gameType > GameType.None;

	public bool isSinglePlayer => gameType == GameType.SinglePlayer;

	public bool isMultiPlayer => gameType == GameType.Multiplayer;

	public DartsGameMountable mountable
	{
		get
		{
			if (dgm == null)
			{
				dgm = mountableRef.Get(base.isServer);
			}
			return dgm;
		}
	}

	[Sync(Autosave = false, RequireChange = false)]
	public Vector3 dartPosition0
	{
		[CompilerGenerated]
		get
		{
			return __sync_dartPosition0;
		}
		[CompilerGenerated]
		private set
		{
			__sync_dartPosition0 = value;
			byte nameID = __GetWeaverID("dartPosition0");
			QueueSyncVar(nameID);
		}
	}

	[Sync(Autosave = false, RequireChange = false)]
	public Vector3 dartPosition1
	{
		[CompilerGenerated]
		get
		{
			return __sync_dartPosition1;
		}
		[CompilerGenerated]
		private set
		{
			__sync_dartPosition1 = value;
			byte nameID = __GetWeaverID("dartPosition1");
			QueueSyncVar(nameID);
		}
	}

	[Sync(Autosave = false, RequireChange = false)]
	public Vector3 dartPosition2
	{
		[CompilerGenerated]
		get
		{
			return __sync_dartPosition2;
		}
		[CompilerGenerated]
		private set
		{
			__sync_dartPosition2 = value;
			byte nameID = __GetWeaverID("dartPosition2");
			QueueSyncVar(nameID);
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DartsGameBoard.OnRpcMessage"))
		{
			if (rpc == 112085967 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_EndGame");
				}
				using (TimeWarning.New("RPC_EndGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(112085967u, "RPC_EndGame", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(112085967u, "RPC_EndGame", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_EndGame(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_EndGame");
					}
				}
				return true;
			}
			if (rpc == 1898904248 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReceiveDartHit");
				}
				using (TimeWarning.New("RPC_ReceiveDartHit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1898904248u, "RPC_ReceiveDartHit", this, player, 1uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RPC_ReceiveDartHit(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_ReceiveDartHit");
					}
				}
				return true;
			}
			if (rpc == 3181726187u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReceiveDartThrow");
				}
				using (TimeWarning.New("RPC_ReceiveDartThrow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3181726187u, "RPC_ReceiveDartThrow", this, player, 1uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							RPC_ReceiveDartThrow(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_ReceiveDartThrow");
					}
				}
				return true;
			}
			if (rpc == 488834035 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_StartMultiplayerGame");
				}
				using (TimeWarning.New("RPC_StartMultiplayerGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(488834035u, "RPC_StartMultiplayerGame", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(488834035u, "RPC_StartMultiplayerGame", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg5 = rPCMessage;
							RPC_StartMultiplayerGame(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_StartMultiplayerGame");
					}
				}
				return true;
			}
			if (rpc == 405074458 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_StartSinglePlayerGame");
				}
				using (TimeWarning.New("RPC_StartSinglePlayerGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(405074458u, "RPC_StartSinglePlayerGame", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(405074458u, "RPC_StartSinglePlayerGame", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg6 = rPCMessage;
							RPC_StartSinglePlayerGame(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_StartSinglePlayerGame");
					}
				}
				return true;
			}
			if (rpc == 1532838903 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_UpdateThrowTimer");
				}
				using (TimeWarning.New("RPC_UpdateThrowTimer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1532838903u, "RPC_UpdateThrowTimer", this, player, 1uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg7 = rPCMessage;
							RPC_UpdateThrowTimer(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in RPC_UpdateThrowTimer");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void DestroyShared()
	{
		if (!_disposed)
		{
			_disposed = true;
			base.DestroyShared();
			EndGame();
			FreeLeaderboardEntries();
		}
	}

	private void FreeLeaderboardEntries()
	{
		if (Leaderboard == null)
		{
			return;
		}
		foreach (DartsGameLeaderboard.DartsGameLeaderboardEntry item in Leaderboard)
		{
			item?.ResetToPool();
		}
		Leaderboard.Clear();
	}

	public void EndGame()
	{
		if (GameController != null)
		{
			if (mountable != null)
			{
				mountable.DismountAllPlayers();
			}
			gameType = GameType.None;
			GameController.ForceLeaveGame();
			NewTurn();
			GameController.Dispose();
			GameController = null;
			_singlePlayerDartsGameController = null;
			_multiplayerDartsGameController = null;
			SendNetworkUpdate();
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.TryGetComponent<DartsGameMountable>(out var component))
		{
			mountableRef.Set(component);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (Leaderboard != null)
		{
			if (info.msg.dartsGameLeaderboard == null)
			{
				info.msg.dartsGameLeaderboard = Facepunch.Pool.Get<DartsGameLeaderboard>();
			}
			DartsGameLeaderboard dartsGameLeaderboard = info.msg.dartsGameLeaderboard;
			if (dartsGameLeaderboard.entries == null)
			{
				dartsGameLeaderboard.entries = Facepunch.Pool.Get<List<DartsGameLeaderboard.DartsGameLeaderboardEntry>>();
			}
			foreach (DartsGameLeaderboard.DartsGameLeaderboardEntry item in Leaderboard)
			{
				if (item != null)
				{
					info.msg.dartsGameLeaderboard.entries.Add(item.Copy());
				}
			}
		}
		if (!info.forDisk)
		{
			if (info.msg.dartsGame == null)
			{
				info.msg.dartsGame = Facepunch.Pool.Get<ProtoBuf.DartsGame>();
			}
			info.msg.dartsGame.mountableId = mountableRef.uid;
			info.msg.dartsGame.players = Facepunch.Pool.Get<List<ProtoBuf.DartsGame.DartsPlayerData>>();
			info.msg.dartsGame.gameType = (int)gameType;
			if (GameController != null)
			{
				GameController.Save(info.msg.dartsGame);
			}
			else
			{
				info.msg.dartsGame.state = 0;
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.dartsGameLeaderboard?.entries == null)
		{
			return;
		}
		if (Leaderboard == null)
		{
			Leaderboard = new List<DartsGameLeaderboard.DartsGameLeaderboardEntry>();
		}
		FreeLeaderboardEntries();
		foreach (DartsGameLeaderboard.DartsGameLeaderboardEntry entry in info.msg.dartsGameLeaderboard.entries)
		{
			if (entry != null)
			{
				Leaderboard.Add(entry.Copy());
			}
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_StartSinglePlayerGame(RPCMessage msg)
	{
		if (GameController == null)
		{
			gameType = GameType.SinglePlayer;
			_singlePlayerDartsGameController = new SinglePlayerDartsGameController(this);
			GameController = _singlePlayerDartsGameController;
			GameController.JoinGame(msg.player);
			GameController.StartPreGame();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_StartMultiplayerGame(RPCMessage msg)
	{
		if (GameController == null)
		{
			gameType = GameType.Multiplayer;
			_multiplayerDartsGameController = new MultiplayerDartsGameController(this);
			GameController = _multiplayerDartsGameController;
			GameController.StartPreGame();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_EndGame(RPCMessage msg)
	{
		EndGame();
	}

	public void NewTurn(bool switchedPlayers = false)
	{
		dartPosition0 = DartSyncResetPosition;
		dartPosition1 = DartSyncResetPosition;
		dartPosition2 = DartSyncResetPosition;
		lastUsedDart = 0;
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void RPC_ReceiveDartThrow(RPCMessage msg)
	{
		if (!(msg.player == null) && GameController != null && GameController.IsGameOngoing && GameController.IsPlayersTurn(msg.player) && GameController.IsAtBoard(msg.player))
		{
			Vector3 dartThrowSyncVar = msg.read.Vector3();
			SetDartThrowSyncVar(dartThrowSyncVar);
		}
	}

	private void SetDartThrowSyncVar(Vector3 aimLocation)
	{
		switch (lastUsedDart)
		{
		case 0:
			DartsDebug($"[DartsGameBoard] Server setting SyncVar dartPosition0 to {aimLocation}");
			dartPosition0 = aimLocation;
			lastUsedDart++;
			break;
		case 1:
			DartsDebug($"[DartsGameBoard] Server setting SyncVar dartPosition1 to {aimLocation}");
			dartPosition1 = aimLocation;
			lastUsedDart++;
			break;
		case 2:
			DartsDebug($"[DartsGameBoard] Server setting SyncVar dartPosition2 to {aimLocation}");
			dartPosition2 = aimLocation;
			lastUsedDart++;
			break;
		default:
			lastUsedDart = 0;
			SetDartThrowSyncVar(aimLocation);
			break;
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void RPC_UpdateThrowTimer(RPCMessage msg)
	{
		float timeTaken = msg.read.Float();
		if (GameController != null)
		{
			GameController.ServerReceivedUpdatedTimer(msg.player, timeTaken);
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void RPC_ReceiveDartHit(RPCMessage msg)
	{
		int points = msg.read.Int32();
		int pointsModifier = msg.read.Int32();
		GameController.ServerReceivedPlayerDartThrow(msg.player, points, pointsModifier);
	}

	public void SendBullseye()
	{
		ClientRPC(RpcTarget.NetworkGroup("PlayBullseyeEffect"));
	}

	public Vector3 WorldToLocalDartPosition(Vector3 worldPosition)
	{
		return center.InverseTransformPoint(worldPosition);
	}

	public Vector3 LocalToWorldDartPosition(Vector3 localPosition)
	{
		return center.TransformPoint(localPosition);
	}

	public (int pointSlice, int pointModifier) GetBoardScoreFromPosition(Vector3 worldPosition)
	{
		Vector3 vector = center.InverseTransformPoint(worldPosition);
		vector = new Vector3(0f - vector.x, vector.y, 0f);
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f + angleOffset;
		if (num < 0f)
		{
			num += 360f;
		}
		int num2 = Mathf.RoundToInt(num / 18f) % 20;
		float magnitude = vector.magnitude;
		DartsDebug($"[DartsGameBoard] Scoring dart hit. Local Position: {vector}, Angle: {num}, Distance: {magnitude}, Slice Index: {num2}");
		if (magnitude <= bullseyeRadius)
		{
			DartsDebug("[DartsGameBoard] Hit Bullseye!");
			return (pointSlice: BullScore, pointModifier: 2);
		}
		if (magnitude <= bullRadius)
		{
			DartsDebug("[DartsGameBoard] Hit Bull!");
			return (pointSlice: BullScore, pointModifier: 1);
		}
		if (magnitude <= bullRadius + EmptySpaceLength + tripleBandRadiusOffset)
		{
			DartsDebug($"[DartsGameBoard] Hit Single {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 1);
		}
		if (magnitude <= bullRadius + bandSize + EmptySpaceLength + tripleBandRadiusOffset)
		{
			DartsDebug($"[DartsGameBoard] Hit Triple {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 3);
		}
		if (magnitude <= bullRadius + bandSize + EmptySpaceLength * 2f)
		{
			DartsDebug($"[DartsGameBoard] Hit Single {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 1);
		}
		if (magnitude <= bullRadius + bandSize * 2f + EmptySpaceLength * 2f)
		{
			DartsDebug($"[DartsGameBoard] Hit Double {ScoreSlices[num2]}");
			return (pointSlice: ScoreSlices[num2], pointModifier: 2);
		}
		return (pointSlice: 0, pointModifier: 0);
	}

	public int GetGameScore(DartsPlayerData playerData)
	{
		return scoreTarget - playerData.Score;
	}

	public int GetGameScore(int score)
	{
		return scoreTarget - score;
	}

	public int GetGameScoreWithTurn(DartsPlayerData playerData)
	{
		if (playerData.State != DartsPlayerData.DartsPlayerState.InGame)
		{
			return scoreTarget - playerData.Score;
		}
		return scoreTarget - playerData.Score - playerData.ScoreThisTurn;
	}

	[HideInCallstack]
	public void DartsDebug(string message)
	{
	}

	[HideInCallstack]
	public void DartsDebugLeaderboard()
	{
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: dartPosition0 for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_dartPosition0);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: dartPosition1 for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_dartPosition1);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: dartPosition2 for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_dartPosition2);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_dartPosition0;
				Vector3 _sync_dartPosition2 = reader.Vector3();
				__sync_dartPosition0 = _sync_dartPosition2;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_dartPosition1;
				Vector3 _sync_dartPosition3 = reader.Vector3();
				__sync_dartPosition1 = _sync_dartPosition3;
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_dartPosition2;
				Vector3 _sync_dartPosition = reader.Vector3();
				__sync_dartPosition2 = _sync_dartPosition;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"dartPosition0" => 0, 
			"dartPosition1" => 1, 
			"dartPosition2" => 2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_dartPosition0 = default(Vector3);
		__sync_dartPosition1 = default(Vector3);
		__sync_dartPosition2 = default(Vector3);
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}
