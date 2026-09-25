#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Text;
using Network;
using Network.Relay;
using Network.Visibility;
using UnityEngine;

public static class RustRelayFakePlayer
{
	private sealed class RelaySubscriberStrategy : ISubscriberStrategy
	{
		public void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			GatherSubscriptions(net, visible);
		}

		public void GatherSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			Manager manager = Net.sv?.visibility;
			if (manager != null)
			{
				AddGroup(visible, manager.Get(0u));
				AddGroup(visible, manager.Get(2u));
				AddGroup(visible, manager.Get(3u));
				AddGroup(visible, manager.Get(1u));
				for (int i = 100; i < 1000; i++)
				{
					AddGroupIfExists(visible, manager, i);
				}
				AddLayer(visible, manager, 0);
				AddLayer(visible, manager, 1);
				AddLayer(visible, manager, 2);
				AddLayer(visible, manager, 3);
				AddLayer(visible, manager, 4);
				AddLayer(visible, manager, 5);
			}
		}

		private static void AddLayer(ListHashSet<Group> visible, Manager visibility, int layer)
		{
			visibility.AddGroups(layer, visible, create: true);
		}

		private static void AddGroupIfExists(ListHashSet<Group> visible, Manager visibility, int groupId)
		{
			AddGroup(visible, visibility.TryGet((uint)groupId));
		}

		private static void AddGroup(ListHashSet<Group> visible, Group group)
		{
			if (group != null)
			{
				visible.TryAdd(group);
			}
		}
	}

	private const string PlayerPrefab = "assets/prefabs/player/player.prefab";

	private const string PlayerName = "RustRelay";

	private const ulong ConnectionGuid = 18446744072328424364uL;

	private static readonly RelaySubscriberStrategy SubscriberStrategy = new RelaySubscriberStrategy();

	private static BasePlayer _player;

	private static Connection _connection;

	private static volatile bool _dirty;

	private static readonly int[] _playerIndex = new int[1];

	private static readonly BasePlayer[] _players = new BasePlayer[1];

	public static void SetDirty()
	{
		_dirty = true;
	}

	public static void SyncWithConfig()
	{
		_dirty = false;
		if (!RustRelay.Config.Enabled || !RustRelay.Config.FakePlayer)
		{
			Shutdown();
		}
		else
		{
			Ensure();
		}
	}

	private static void Ensure()
	{
		if ((!(_player != null) || _player.IsDestroyed || _connection == null) && GameManager.server != null && Net.sv?.visibility != null)
		{
			Shutdown();
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", Vector3.zero);
			_player = ((baseEntity != null) ? baseEntity.ToPlayer() : null);
			if (_player == null)
			{
				Debug.LogWarning("[RustRelay] Failed to create fake player entity");
				return;
			}
			_players[0] = _player;
			_player.enableSaving = false;
			_player.limitNetworking = true;
			_player.health = 0f;
			_player.lifestate = BaseCombatEntity.LifeState.Dead;
			_player.ResetLifeStateOnSpawn = false;
			_player.Spawn();
			_player.displayName = "RustRelay";
			_player.isInvisible = true;
			_connection = new Connection
			{
				guid = 18446744072328424364uL,
				userid = _player.userID,
				ownerid = _player.userID,
				username = "RustRelay",
				active = true,
				connected = true,
				state = Connection.State.Connected,
				globalNetworking = true,
				authStatusSteam = "ok",
				authStatusEAC = "ok",
				authStatusNexus = "ok",
				authStatusCentralizedBans = "ok",
				authStatusPremiumServer = "ok",
				connectionTime = TimeEx.realtimeSinceStartup
			};
			_connection.ipaddress = "127.0.0.1:0";
			_connection.player = _player;
			_player.net.SubStrategy = SubscriberStrategy;
			_player.net.OnConnected(_connection);
			RustRelay.RegisterFakeConnection(_connection);
			_player.net.StartSubscriber();
			PrioritizeFakeConnection();
			Debug.Log($"[RustRelay] Fake player created with network id {_player.net.ID}");
		}
	}

	public static void Tick()
	{
		if (_dirty)
		{
			SyncWithConfig();
		}
		if (!(_player == null) && _player.net?.connection != null)
		{
			BasePlayer.SendEntityUpdates(_players, _playerIndex);
		}
	}

	public static bool IsFakePlayer(BasePlayer player)
	{
		if (player != null)
		{
			return player == _player;
		}
		return false;
	}

	public static string GetStatusReport()
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		BasePlayer player = _player;
		Connection connection = _connection;
		stringBuilder.AppendLine("Fake player");
		stringBuilder.AppendLine($"  Requested by config: {RustRelay.Config.Enabled && RustRelay.Config.FakePlayer}");
		stringBuilder.AppendLine($"  Player exists: {player != null}");
		stringBuilder.AppendLine($"  Connection exists: {connection != null}");
		stringBuilder.AppendLine($"  Registered fake connection active: {RustRelay.HasActiveFakeConnection}");
		stringBuilder.AppendLine($"  _players[0] set: {_players[0] != null}");
		if (player != null)
		{
			stringBuilder.AppendLine($"  Player destroyed: {player.IsDestroyed}");
			stringBuilder.AppendLine("  Player net id: " + (player.net?.ID.ToString() ?? "null"));
			stringBuilder.AppendLine($"  Player user id: {player.userID}");
			stringBuilder.AppendLine("  Player display name: " + player.displayName);
			stringBuilder.AppendLine($"  Player invisible: {player.isInvisible}");
			stringBuilder.AppendLine($"  Player saving enabled: {player.enableSaving}");
			stringBuilder.AppendLine($"  Player limit networking: {player.limitNetworking}");
			stringBuilder.AppendLine($"  Player life state: {player.lifestate}");
			stringBuilder.AppendLine($"  Player supports server occlusion: {player.SupportsServerOcclusion()}");
			stringBuilder.AppendLine($"  Player net connection set: {player.net?.connection != null}");
			stringBuilder.AppendLine($"  Player subscriber set: {player.net?.subscriber != null}");
			stringBuilder.AppendLine("  Player group id: " + (player.net?.group?.ID.ToString() ?? "null"));
		}
		if (connection != null)
		{
			stringBuilder.AppendLine($"  Connection guid: {connection.guid}");
			stringBuilder.AppendLine($"  Connection userid: {connection.userid}");
			stringBuilder.AppendLine($"  Connection active: {connection.active}");
			stringBuilder.AppendLine($"  Connection connected: {connection.connected}");
			stringBuilder.AppendLine($"  Connection state: {connection.state}");
			stringBuilder.AppendLine($"  Connection global networking: {connection.globalNetworking}");
			stringBuilder.AppendLine($"  Connection registered with RustRelay: {RustRelay.IsFakeConnection(connection)}");
		}
		Subscriber subscriber = player?.net?.subscriber;
		if (subscriber != null)
		{
			int count = subscriber.subscribed.Count;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			foreach (Group item in subscriber.subscribed)
			{
				List<Connection> subscribers = item.subscribers;
				if (subscribers == null)
				{
					num++;
				}
				else if (subscribers.Count > 0 && RustRelay.IsFakeConnection(subscribers[0]))
				{
					num2++;
					if (subscribers.Count == 1)
					{
						num3++;
					}
					else
					{
						num4++;
					}
				}
				else
				{
					num5++;
				}
			}
			stringBuilder.AppendLine("  Subscriptions");
			stringBuilder.AppendLine($"    Subscribed groups: {count}");
			stringBuilder.AppendLine($"    Groups missing subscriber list: {num}");
			stringBuilder.AppendLine($"    Groups with fake first: {num2}");
			stringBuilder.AppendLine($"    Groups with fake only: {num3}");
			stringBuilder.AppendLine($"    Groups with fake + real subscribers: {num4}");
			stringBuilder.AppendLine($"    Groups where fake is missing/not first: {num5}");
		}
		return stringBuilder.ToString();
	}

	public static void Shutdown()
	{
		BasePlayer player = _player;
		if (!(player == null))
		{
			player.ClearEntityQueue();
			Connection connection = _connection;
			Debug.Assert(connection != null, "RustRelay fake player missing fake connection during shutdown");
			player.net.OnDisconnected();
			RustRelay.ClearFakeConnection(connection);
			if (!player.IsDestroyed)
			{
				player.Kill();
			}
			connection.OnDisconnected();
			_connection = null;
			_player = null;
			_players[0] = null;
		}
	}

	private static void PrioritizeFakeConnection()
	{
		foreach (Group item in _player.net.subscriber.subscribed)
		{
			List<Connection> subscribers = item.subscribers;
			if (subscribers[0] != _connection)
			{
				int index = subscribers.Count - 1;
				Debug.Assert(subscribers[index] == _connection, "RustRelay fake connection should be the last subscriber before prioritization");
				subscribers[index] = subscribers[0];
				subscribers[0] = _connection;
			}
		}
	}
}
