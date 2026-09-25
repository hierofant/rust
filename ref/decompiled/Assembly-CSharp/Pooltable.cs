#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using PoolPhysics;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Pooltable : BaseCombatEntity
{
	private readonly Dictionary<ulong, BaseEntity> playerMountables = new Dictionary<ulong, BaseEntity>();

	private TimeSince timeSinceLastMove;

	[Header("Shared")]
	[SerializeField]
	private float ballRadius;

	[SerializeField]
	private float tableWidth = 1.2f;

	[SerializeField]
	private float tableHeight = 0.6f;

	[SerializeField]
	private float pocketRadius = 0.045f;

	[SerializeField]
	private float mouthWidth = 0.08f;

	[SerializeField]
	private float cueBallStartX = -0.545f;

	[SerializeField]
	private WorldSpline worldSpline;

	[SerializeField]
	[Range(0f, 0.75f)]
	[Tooltip("Fraction of the gap between the walking spline and the table edge to close, so players stand the same bit closer everywhere on the loop.")]
	private float splineTableCloseness = 0.25f;

	[Tooltip("Block walking the mountable into geometry (e.g. an adjacent boat's hull). Turn off to restore pre-check behaviour.")]
	[SerializeField]
	private bool runWalkClippingChecks = true;

	[SerializeField]
	[Tooltip("Player body volume tested at each candidate walk pose, in MOUNTABLE space: origin is the pulled spline point, +z points at the cue ball, y=0 is 1m above the player's feet.")]
	private Bounds walkAreaCheck = new Bounds(new Vector3(0f, 0f, 0.24f), new Vector3(0.55f, 1.3f, 0.44f));

	[SerializeField]
	[Header("Server")]
	private GameObjectRef mountableRef;

	[SerializeField]
	private GameObjectRef winEffect;

	[SerializeField]
	[Header("Client")]
	private List<GameObject> clientRenderingPoolBalls;

	[SerializeField]
	private Transform ballParent;

	[SerializeField]
	private GameObjectRef poolTableUIRef;

	[SerializeField]
	private SoundDefinition cueStrikeSound;

	[SerializeField]
	private SoundDefinition ballCollisionSound;

	[SerializeField]
	private SoundDefinition ballBumperCollisionSound;

	[SerializeField]
	private SoundDefinition ballPocketSound;

	[SerializeField]
	private GameObjectRef resetGameEffect;

	[SerializeField]
	private Vector2 ballCollisionSpeedRange = new Vector2(0.1f, 3f);

	[SerializeField]
	private float ballCollisionSoundInterval = 0.02f;

	[Tooltip("All pocketed balls spawn a fake visual at the start of this path and follow it into the basket.")]
	[Header("Ball Return")]
	[SerializeField]
	private WorldSpline ballReturnPath;

	[Tooltip("Preplaced basket balls enabled in order as balls arrive, independent of ball ID.")]
	[SerializeField]
	private GameObject[] basketBalls;

	[SerializeField]
	private float ballReturnSpeed = 1.5f;

	[SerializeField]
	private float eyeOverrideBehindCueBallOffset = 0.6f;

	[SerializeField]
	private float eyeOverrideHeightOffset = 0.2f;

	protected const Flags Flag_IdleResettable = Flags.Reserved1;

	[ReplicatedVar]
	public static bool debug_pool = false;

	[ReplicatedVar]
	public static float physics_update_rate = 64f;

	[ServerVar(Saved = true, Help = "Show pool game tooltip notifications")]
	public static bool show_tooltips = false;

	[ServerVar(Help = "(Generated) Anyone can reset a pool game nobody has interacted with for this many seconds")]
	public static float idle_reset_seconds = 180f;

	[ServerVar(Help = "(Generated) Seconds the shooter stays seated watching their shot before being dismounted")]
	public static float watch_after_shot_seconds = 2f;

	private Engine physicsEngine;

	private PoolTableGameController gameController;

	private static readonly Translate.Phrase ProcessingTurnPhrase = new Translate.Phrase("poolprocessing", "Processing turn");

	private static readonly Translate.Phrase WaitingForPlayerPhrase = new Translate.Phrase("poolwaiting", "Waiting for player");

	private const float MinShotForce = 1f;

	private const float MaxShotForce = 8f;

	private bool wasMovingLastTick;

	private double lastPhysicsTime;

	private const int MaxCatchUpTicks = 32;

	private const float TableBoundSoftness = 0.15f;

	private const int WalkCheckMask = 1235298561;

	private const float WalkCheckStep = 0.15f;

	private const int WalkCheckMaxSamples = 24;

	private const float WalkCheckScanStep = 0.25f;

	private static readonly int[][] RackRows = new int[5][]
	{
		new int[1] { 1 },
		new int[2] { 9, 2 },
		new int[3] { 10, 8, 3 },
		new int[4] { 11, 4, 12, 5 },
		new int[5] { 13, 6, 14, 15, 7 }
	};

	private float PhysicsRate => 1f / physics_update_rate;

	private Vector2 CueBallStartPosition => new Vector2(cueBallStartX, 0f);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Pooltable.OnRpcMessage"))
		{
			if (rpc == 1237563035 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_JoinGame");
				}
				using (TimeWarning.New("RPC_JoinGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1237563035u, "RPC_JoinGame", this, player, 3f))
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
							RPC_JoinGame(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_JoinGame");
					}
				}
				return true;
			}
			if (rpc == 985964862 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestCancelGame");
				}
				using (TimeWarning.New("RPC_RequestCancelGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(985964862u, "RPC_RequestCancelGame", this, player, 3f))
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
							RPC_RequestCancelGame(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_RequestCancelGame");
					}
				}
				return true;
			}
			if (rpc == 1316133824 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestMount");
				}
				using (TimeWarning.New("RPC_RequestMount"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1316133824u, "RPC_RequestMount", this, player, 3f))
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
							RPC_RequestMount(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_RequestMount");
					}
				}
				return true;
			}
			if (rpc == 170667224 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestResetGame");
				}
				using (TimeWarning.New("RPC_RequestResetGame"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(170667224u, "RPC_RequestResetGame", this, player, 3f))
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
							RPC_RequestResetGame(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_RequestResetGame");
					}
				}
				return true;
			}
			if (rpc == 1741528195 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestShoot");
				}
				using (TimeWarning.New("RPC_RequestShoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1741528195u, "RPC_RequestShoot", this, player, 3f))
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
							RPC_RequestShoot(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_RequestShoot");
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
						if (!RPC_Server.IsVisible.Test(488834035u, "RPC_StartMultiplayerGame", this, player, 3f))
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
							RPC_StartMultiplayerGame(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
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
						if (!RPC_Server.IsVisible.Test(405074458u, "RPC_StartSinglePlayerGame", this, player, 3f))
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
							RPCMessage msg8 = rPCMessage;
							RPC_StartSinglePlayerGame(msg8);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in RPC_StartSinglePlayerGame");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(UpdateIdleResettable, 1f, 1f);
	}

	private void UpdateIdleResettable()
	{
		bool b = gameController != null && gameController.HasGame && (float)timeSinceLastMove > idle_reset_seconds;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b);
	}

	public override void Save(SaveInfo info)
	{
		using (TimeWarning.New("Pooltable.Save"))
		{
			base.Save(info);
			if (!base.isServer || info.forDisk)
			{
				return;
			}
			info.msg.Pooltable = Facepunch.Pool.Get<ProtoBuf.Pooltable>();
			info.msg.Pooltable.poolBalls = Facepunch.Pool.Get<List<PoolBallData>>();
			if (physicsEngine != null && physicsEngine.IsReady)
			{
				info.msg.Pooltable.poolBalls.Clear();
				foreach (PoolPhysics.Data.Ball ball in physicsEngine.Balls)
				{
					PoolBallData poolBallData = Facepunch.Pool.Get<PoolBallData>();
					poolBallData.position = ball.Position;
					poolBallData.velocity = ball.Velocity;
					poolBallData.pocketed = ball.IsKinematic;
					info.msg.Pooltable.poolBalls.Add(poolBallData);
				}
			}
			gameController?.Save(info.msg.Pooltable);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_StartSinglePlayerGame(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanInteract())
		{
			BeginGame(msg.player, solo: true);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_StartMultiplayerGame(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanInteract())
		{
			BeginGame(msg.player, solo: false);
		}
	}

	private void BeginGame(BasePlayer player, bool solo)
	{
		if (!(player == null) && (gameController == null || !gameController.HasGame))
		{
			if (gameController == null)
			{
				gameController = new PoolTableGameController(this);
			}
			gameController.StartNewGame(player.userID, solo);
			timeSinceLastMove = 0f;
			if (solo)
			{
				MountPlayerAtTable(player);
			}
		}
	}

	public void PlayWinEffect()
	{
		if (winEffect.isValid)
		{
			Effect.server.Run(winEffect.resourcePath, base.transform.position, Vector3.up);
		}
	}

	public bool CanPlayerMove(ulong playerId)
	{
		if (gameController != null && gameController.State == PoolTableGameController.GameState.WaitingForShot)
		{
			return gameController.CurrentPlayerId == playerId;
		}
		return false;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_JoinGame(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanInteract() && gameController != null)
		{
			gameController.AddSecondPlayer(msg.player.userID);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RequestMount(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanInteract() && gameController != null && gameController.HasGame && gameController.CanMount(msg.player.userID))
		{
			MountPlayerAtTable(msg.player);
		}
	}

	private void MountPlayerAtTable(BasePlayer player)
	{
		if (!(worldSpline == null) && (physicsEngine == null || !physicsEngine.HasMovingBalls()) && playerMountables.Count <= 0)
		{
			CancelInvoke(DismountAllSeatedPlayers);
			worldSpline.GetClosestPointWorld(player.transform.position, out var distanceOnSpline);
			if (TryFindFreeSplineDistance(distanceOnSpline, out distanceOnSpline) && TryGetSplinePose(distanceOnSpline, out var pos, out var rot))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(mountableRef.resourcePath, pos, rot);
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
				playerMountables[player.userID] = baseEntity;
				PooltableMountable component = baseEntity.GetComponent<PooltableMountable>();
				component.SplineDistance = distanceOnSpline;
				component.MountPlayer(player);
				ClientRPC(RpcTarget.Player("RPC_OpenPoolUI", player));
				gameController?.OnPlayerJoined(player.userID);
				timeSinceLastMove = 0f;
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RequestShoot(RPCMessage msg)
	{
		if (msg.player == null || IsInvoking(DismountAllSeatedPlayers) || gameController == null || !gameController.CanShoot(msg.player.userID))
		{
			return;
		}
		Vector3 vector = msg.read.Vector3();
		float num = msg.read.Float();
		if (!vector.IsNaNOrInfinity() && !num.IsNaNOrInfinity())
		{
			vector.y = 0f;
			vector = Vector3.ClampMagnitude(vector, 1f);
			num = Mathf.Clamp(num, 1f, 8f);
			if (!(vector.sqrMagnitude <= Mathf.Epsilon))
			{
				gameController.OnShotFired();
				timeSinceLastMove = 0f;
				ApplyShotShared(vector, num);
				ClientRPC(RpcTarget.NetworkGroup("ClientOnShotFired"), vector, num, gameController.ShotId);
				SendNetworkUpdateImmediate();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RequestResetGame(RPCMessage msg)
	{
		if (!(msg.player == null) && CanResetGame(msg.player))
		{
			ResetGameState();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_RequestCancelGame(RPCMessage msg)
	{
		if (!(msg.player == null) && CanCancelGame(msg.player))
		{
			ResetGameState();
		}
	}

	private void ResetGameState()
	{
		CancelInvoke(DismountAllSeatedPlayers);
		DismountAllSeatedPlayers();
		ResetBallsToRack();
		if (gameController != null)
		{
			gameController.ResetToInitialState();
		}
		else
		{
			gameController = new PoolTableGameController(this);
			gameController.ResetToInitialState();
		}
		SendNetworkUpdateImmediate();
		if (resetGameEffect.isValid)
		{
			Effect.server.Run(resetGameEffect.resourcePath, base.transform.position, Vector3.up);
		}
	}

	public void OnMountablePlayerLeft(ulong playerId)
	{
		if (playerMountables.TryGetValue(playerId, out var value))
		{
			playerMountables.Remove(playerId);
			if (value != null && !value.IsDestroyed)
			{
				value.Kill();
			}
		}
	}

	private void DismountAllSeatedPlayers()
	{
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		pooledList.AddRange(playerMountables.Values);
		foreach (BaseEntity item in pooledList)
		{
			if (item != null && !item.IsDestroyed)
			{
				(item as PooltableMountable).DismountAllPlayers();
			}
		}
		playerMountables.Clear();
	}

	public override void InitShared()
	{
		base.InitShared();
		if (physicsEngine == null)
		{
			physicsEngine = Facepunch.Pool.Get<Engine>();
		}
		gameController = new PoolTableGameController(this);
		SetupTable();
		StandardPoolBallSetup();
		if (physicsEngine != null)
		{
			Engine engine = physicsEngine;
			engine.OnBallPocketed = (Action<int>)Delegate.Combine(engine.OnBallPocketed, new Action<int>(OnBallPocketed));
		}
		lastPhysicsTime = UnityEngine.Time.timeAsDouble;
		InvokeRepeating(PhysicsTick, 0f, PhysicsRate);
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (IsInvoking(PhysicsTick))
		{
			CancelInvoke(PhysicsTick);
		}
		if (physicsEngine != null)
		{
			Engine engine = physicsEngine;
			engine.OnBallPocketed = (Action<int>)Delegate.Remove(engine.OnBallPocketed, new Action<int>(OnBallPocketed));
		}
		if (physicsEngine != null)
		{
			Facepunch.Pool.Free(ref physicsEngine);
		}
		gameController = null;
		if (IsInvoking(UpdateIdleResettable))
		{
			CancelInvoke(UpdateIdleResettable);
		}
		if (IsInvoking(DismountAllSeatedPlayers))
		{
			CancelInvoke(DismountAllSeatedPlayers);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		ProtoBuf.Pooltable pooltable = info.msg.Pooltable;
		if (pooltable != null && base.isServer)
		{
			gameController?.Load(pooltable);
		}
	}

	public bool IsBallPocketed(int ballId)
	{
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return false;
		}
		return physicsEngine.Balls[ballId].IsKinematic;
	}

	public void RespotCueBallAfterFoul()
	{
		if (physicsEngine != null && physicsEngine.IsReady && physicsEngine.Balls.Count > 0)
		{
			physicsEngine.SetBallPosition(0, CueBallStartPosition);
			physicsEngine.SetBallVelocity(0, Vector2.zero);
			physicsEngine.SetBallIsKinematic(0, isKinematic: false);
			DebugPool("RespotCueBallAfterFoul");
			if (base.isServer)
			{
				SendNetworkUpdateImmediate();
			}
		}
	}

	public bool TryGetCueBallWorldPosition(out Vector3 worldPosition)
	{
		worldPosition = Vector3.zero;
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return false;
		}
		if (physicsEngine.Balls.Count <= 0)
		{
			return false;
		}
		PoolPhysics.Data.Ball ball = physicsEngine.Balls[0];
		if (ball.IsKinematic)
		{
			return false;
		}
		worldPosition = base.transform.TransformPoint(new Vector3(ball.Position.x, 0f, ball.Position.y));
		return true;
	}

	public Vector3 GetShotLineEndPoint(Vector3 worldOrigin, Vector3 worldDirection, float maxDistance, int ignoreBallId = -1, bool includeWalls = true)
	{
		if (maxDistance <= 0f)
		{
			return worldOrigin;
		}
		Vector3 vector = worldDirection;
		vector.y = 0f;
		if (vector.sqrMagnitude <= Mathf.Epsilon)
		{
			return worldOrigin;
		}
		Vector3 normalized = vector.normalized;
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return worldOrigin + normalized * maxDistance;
		}
		Vector3 vector2 = base.transform.InverseTransformPoint(worldOrigin);
		Vector3 vector3 = base.transform.InverseTransformDirection(normalized);
		vector3.y = 0f;
		Vector2 vector4 = new Vector2(vector3.x, vector3.z);
		if (vector4.sqrMagnitude <= Mathf.Epsilon)
		{
			return worldOrigin;
		}
		vector4.Normalize();
		Vector2 vector5 = new Vector2(vector2.x, vector2.z);
		if (physicsEngine.Raycast(vector5, vector4, maxDistance, out var hit, ignoreBallId, includeBalls: true, includeWalls))
		{
			return base.transform.TransformPoint(new Vector3(hit.Point.x, 0f, hit.Point.y));
		}
		Vector2 vector6 = vector5 + vector4 * maxDistance;
		return base.transform.TransformPoint(new Vector3(vector6.x, 0f, vector6.y));
	}

	public Vector3 GetDirToCueBall(Vector3 pos)
	{
		Vector2 position = physicsEngine.Balls[0].Position;
		Vector3 vector = base.transform.TransformPoint(new Vector3(position.x, 0f, position.y)) - pos;
		vector.y = 0f;
		if (vector.sqrMagnitude <= Mathf.Epsilon)
		{
			return base.transform.forward;
		}
		Vector3 normalized = vector.normalized;
		normalized.y = 0f;
		return normalized;
	}

	public float GetSplineDistanceForPosition(Vector3 worldPos)
	{
		worldSpline.GetClosestPointWorld(worldPos, out var distanceOnSpline);
		return distanceOnSpline;
	}

	public void MoveMountableAlongSpline(PooltableMountable mountable, float movement)
	{
		float length = worldSpline.GetData().Length;
		if (!(length <= 0f))
		{
			movement = ClampWalkDistance(mountable, movement, length);
			if (!(Mathf.Abs(movement) <= Mathf.Epsilon))
			{
				mountable.SplineDistance = Mathf.Repeat(mountable.SplineDistance + movement, length);
				PositionMountableOnSpline(mountable);
			}
		}
	}

	public void MoveMountableTowardSplineDistance(PooltableMountable mountable, float targetDistance, float maxWalkDistance)
	{
		float length = worldSpline.GetData().Length;
		if (!(length <= 0f))
		{
			float num = Mathf.Repeat(targetDistance - mountable.SplineDistance, length);
			if (num > length * 0.5f)
			{
				num -= length;
			}
			num = Mathf.Clamp(num, 0f - maxWalkDistance, maxWalkDistance);
			num = ClampWalkDistance(mountable, num, length);
			if (!(Mathf.Abs(num) <= Mathf.Epsilon))
			{
				mountable.SplineDistance = Mathf.Repeat(mountable.SplineDistance + num, length);
				PositionMountableOnSpline(mountable);
			}
		}
	}

	private static float SoftPositive(float t)
	{
		return (t + Mathf.Sqrt(t * t + 0.0225f)) * 0.5f;
	}

	private static float PullAxisTowardBound(float v, float halfExtent, float closeness)
	{
		return v - (SoftPositive(v - halfExtent) - SoftPositive(0f - halfExtent - v)) * closeness;
	}

	private Vector3 PullSplinePointTowardTable(Vector3 worldPos)
	{
		if (splineTableCloseness <= 0f)
		{
			return worldPos;
		}
		Vector3 position = base.transform.InverseTransformPoint(worldPos);
		position.x = PullAxisTowardBound(position.x, tableWidth * 0.5f, splineTableCloseness);
		position.z = PullAxisTowardBound(position.z, tableHeight * 0.5f, splineTableCloseness);
		return base.transform.TransformPoint(position);
	}

	public bool TryGetSplinePose(float splineDistance, out Vector3 pos, out Quaternion rot)
	{
		pos = default(Vector3);
		rot = default(Quaternion);
		if (worldSpline == null)
		{
			return false;
		}
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return false;
		}
		if (physicsEngine.Balls.Count <= 0)
		{
			return false;
		}
		pos = PullSplinePointTowardTable(worldSpline.GetPointCubicHermiteWorld(splineDistance));
		rot = Quaternion.LookRotation(GetDirToCueBall(pos));
		return true;
	}

	private void PositionMountableOnSpline(PooltableMountable mountable)
	{
		if (TryGetSplinePose(mountable.SplineDistance, out var pos, out var rot))
		{
			mountable.transform.position = pos;
			mountable.transform.rotation = rot;
		}
	}

	public bool IsWalkPoseBlocked(Vector3 pos, Quaternion rot)
	{
		if (!runWalkClippingChecks)
		{
			return false;
		}
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		GamePhysics.OverlapOBB(new OBB(pos, rot, walkAreaCheck), obj, 1235298561);
		BaseEntity rootParentEntity = GetRootParentEntity();
		bool result = false;
		foreach (Collider item in obj)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
			if (baseEntity == null)
			{
				result = true;
				break;
			}
			if (baseEntity.isServer == base.isServer && !(baseEntity.GetRootParentEntity() == rootParentEntity))
			{
				result = true;
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	private float ClampWalkDistance(PooltableMountable mountable, float delta, float splineLength)
	{
		if (!runWalkClippingChecks)
		{
			return delta;
		}
		if (Mathf.Abs(delta) <= Mathf.Epsilon)
		{
			return delta;
		}
		float splineDistance = mountable.SplineDistance;
		if (TryGetSplinePose(splineDistance, out var pos, out var rot) && IsWalkPoseBlocked(pos, rot))
		{
			return delta;
		}
		float num = Mathf.Min(Mathf.Abs(delta), 3.6000001f);
		float num2 = Mathf.Sign(delta);
		float num3 = 0f;
		for (int i = 1; i <= 24; i++)
		{
			float num4 = Mathf.Min((float)i * 0.15f, num);
			float splineDistance2 = Mathf.Repeat(splineDistance + num2 * num4, splineLength);
			if (!TryGetSplinePose(splineDistance2, out var pos2, out var rot2) || IsWalkPoseBlocked(pos2, rot2))
			{
				break;
			}
			num3 = num4;
			if (num4 >= num)
			{
				break;
			}
		}
		return num2 * num3;
	}

	public bool TryFindFreeSplineDistance(float preferred, out float result)
	{
		result = preferred;
		if (!runWalkClippingChecks)
		{
			return true;
		}
		float num = ((worldSpline != null) ? worldSpline.GetData().Length : 0f);
		if (num <= 0f)
		{
			return true;
		}
		if (TryGetSplinePose(preferred, out var pos, out var rot) && !IsWalkPoseBlocked(pos, rot))
		{
			return true;
		}
		int num2 = Mathf.CeilToInt(num * 0.5f / 0.25f);
		for (int i = 1; i <= num2; i++)
		{
			float num3 = (float)i * 0.25f;
			for (int j = 0; j < 2; j++)
			{
				float num4 = Mathf.Repeat(preferred + ((j == 0) ? num3 : (0f - num3)), num);
				if (TryGetSplinePose(num4, out var pos2, out var rot2) && !IsWalkPoseBlocked(pos2, rot2))
				{
					result = num4;
					return true;
				}
			}
		}
		return false;
	}

	private void ApplyShotShared(Vector3 dir, float force)
	{
		force = Mathf.Clamp(force, 0f, 10f);
		Vector2 force2 = new Vector2(dir.x, dir.z) * force;
		physicsEngine.ApplyForce(0, force2);
		lastPhysicsTime = UnityEngine.Time.timeAsDouble;
	}

	private void PhysicsTick()
	{
		using (TimeWarning.New("Pooltable.PhysicsTick"))
		{
			if (physicsEngine == null || !physicsEngine.IsReady)
			{
				return;
			}
			bool flag = physicsEngine.HasMovingBalls();
			if (flag)
			{
				int num = Mathf.Clamp((int)((UnityEngine.Time.timeAsDouble - lastPhysicsTime) / (double)PhysicsRate), 0, 32);
				using (TimeWarning.New("Pooltable.PhysicsTick.Simulate"))
				{
					for (int i = 0; i < num; i++)
					{
						physicsEngine.Tick(PhysicsRate);
					}
				}
				lastPhysicsTime += (double)num * (double)PhysicsRate;
				if (UnityEngine.Time.timeAsDouble - lastPhysicsTime > (double)(PhysicsRate * 32f))
				{
					lastPhysicsTime = UnityEngine.Time.timeAsDouble;
				}
				if (base.isServer && num > 0)
				{
					using (TimeWarning.New("Pooltable.PhysicsTick.SendNetworkUpdate"))
					{
						SendNetworkUpdate();
					}
				}
			}
			else
			{
				lastPhysicsTime = UnityEngine.Time.timeAsDouble;
				if (wasMovingLastTick)
				{
					DebugPool("PhysicsTick transition moving->stopped");
					using (TimeWarning.New("Pooltable.PhysicsTick.OnBallsStopped"))
					{
						gameController?.OnBallsStopped();
						if (base.isServer)
						{
							CancelInvoke(DismountAllSeatedPlayers);
							Invoke(DismountAllSeatedPlayers, watch_after_shot_seconds);
						}
					}
				}
			}
			wasMovingLastTick = flag;
		}
	}

	private List<(int id, Vector2 position)> BuildStartingLayout()
	{
		List<(int, Vector2)> list = new List<(int, Vector2)>();
		list.Add((0, CueBallStartPosition));
		float num = ballRadius * 2f + 0.01f;
		float rowSpacingX = num * 0.866f;
		float rowSpacingY = num;
		Vector2 rackOrigin = new Vector2(0.4f, 0f);
		Vector2[] array = new Vector2[16];
		for (int i = 0; i < RackRows.Length; i++)
		{
			int[] array2 = RackRows[i];
			for (int j = 0; j < array2.Length; j++)
			{
				array[array2[j]] = slot(i, j, array2.Length);
			}
		}
		for (int k = 1; k <= 15; k++)
		{
			list.Add((k, array[k]));
		}
		return list;
		Vector2 slot(int row, int index, int rowCount)
		{
			return rackOrigin + new Vector2(rowSpacingX * (float)row, ((float)index - (float)(rowCount - 1) * 0.5f) * rowSpacingY);
		}
	}

	private void StandardPoolBallSetup()
	{
		foreach (var (id, position) in BuildStartingLayout())
		{
			physicsEngine.AddBall(new PoolPhysics.Data.Ball
			{
				Id = id,
				Position = position,
				Velocity = Vector2.zero,
				Radius = ballRadius,
				IsKinematic = false
			});
		}
		if (base.isServer)
		{
			Invoke(base.SendNetworkUpdateImmediate, 0.1f);
		}
	}

	private void ResetBallsToRack()
	{
		if (physicsEngine == null || !physicsEngine.IsReady)
		{
			return;
		}
		foreach (var (id, pos) in BuildStartingLayout())
		{
			physicsEngine.SetBallIsKinematic(id, isKinematic: false);
			physicsEngine.SetBallPosition(id, pos);
		}
	}

	private void SetupTable()
	{
		float num = tableWidth / 2f;
		float num2 = tableHeight / 2f;
		float num3 = mouthWidth;
		float num4 = mouthWidth * 0.8f;
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f - num, 0f - num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(num, 0f - num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f - num, num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(num, num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f, 0f - num2),
			Radius = pocketRadius
		});
		physicsEngine.AddPocket(new PoolPhysics.Data.Pocket
		{
			Position = new Vector2(0f, num2),
			Radius = pocketRadius
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(0f - num + num3, 0f - num2),
			B = new Vector2((0f - num4) / 2f, 0f - num2),
			Normal = Vector2.up
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(num4 / 2f, 0f - num2),
			B = new Vector2(num - num3, 0f - num2),
			Normal = Vector2.up
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(0f - num + num3, num2),
			B = new Vector2((0f - num4) / 2f, num2),
			Normal = Vector2.down
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(num4 / 2f, num2),
			B = new Vector2(num - num3, num2),
			Normal = Vector2.down
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(0f - num, 0f - num2 + num3),
			B = new Vector2(0f - num, num2 - num3),
			Normal = Vector2.right
		});
		physicsEngine.AddWall(new PoolPhysics.Data.Wall
		{
			A = new Vector2(num, 0f - num2 + num3),
			B = new Vector2(num, num2 - num3),
			Normal = Vector2.left
		});
	}

	private bool CanResetGame(BasePlayer player)
	{
		if (gameController == null || !gameController.HasGame)
		{
			return false;
		}
		if (gameController.State == PoolTableGameController.GameState.NotPlaying || gameController.State == PoolTableGameController.GameState.WaitingForPlayers || gameController.State == PoolTableGameController.GameState.BallsMoving)
		{
			return false;
		}
		if (gameController.State == PoolTableGameController.GameState.GameOver)
		{
			return true;
		}
		if (!HasFlag(Flags.Reserved1))
		{
			return gameController.IsParticipant(player.userID);
		}
		return true;
	}

	private bool CanCancelGame(BasePlayer player)
	{
		if (gameController != null && gameController.State == PoolTableGameController.GameState.WaitingForPlayers)
		{
			return gameController.IsParticipant(player.userID);
		}
		return false;
	}

	private void OnBallPocketed(int ballId)
	{
		physicsEngine.SetBallIsKinematic(ballId, isKinematic: true);
		gameController?.OnBallPocketed(ballId);
		DebugPool($"OnBallPocketed ballId={ballId}");
		if (base.isServer)
		{
			SendNetworkUpdateImmediate();
		}
	}

	private void DebugPool(string message)
	{
		if (debug_pool)
		{
			string text = ((net != null) ? net.ID.Value.ToString() : "n/a");
			string text2 = ((gameController != null) ? gameController.State.ToString() : "null");
			int num = ((gameController != null) ? gameController.CurrentIndex : (-1));
			uint num2 = ((gameController != null) ? gameController.ShotId : 0u);
			string text3 = "predictedShotId=n/a lastApplied=n/a";
			Debug.Log($"[PoolTable] table={text} gcState={text2} currentIndex={num} serverShotId={num2} {text3} | {message}");
		}
	}
}
