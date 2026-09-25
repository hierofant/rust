using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CompanionServer.Handlers;
using ConVar;
using Facepunch;
using Fleck;
using ProtoBuf;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace CompanionServer;

public class Listener : IDisposable, IBroadcastSender<AppBroadcast>
{
	private struct Message
	{
		public readonly Connection Connection;

		public readonly MemoryBuffer Buffer;

		public Message(Connection connection, MemoryBuffer buffer)
		{
			Connection = connection;
			Buffer = buffer;
		}
	}

	private static readonly ByteArrayStream Stream = new ByteArrayStream();

	private readonly TokenBucketList<IPAddress> _ipTokenBuckets;

	private readonly BanList<IPAddress> _ipBans;

	private readonly TokenBucketList<ulong> _playerTokenBuckets;

	private readonly TokenBucketList<ulong> _pairingTokenBuckets;

	private readonly Queue<Message> _messageQueue;

	private readonly WebSocketServer _server;

	private readonly Stopwatch _stopwatch;

	private readonly SynchronizationContext _syncContext;

	private RealTimeSince _lastCleanup;

	private long _nextConnectionId;

	public readonly IPAddress Address;

	public readonly int Port;

	public readonly SubscriberList<PlayerTarget, AppBroadcast> PlayerSubscribers;

	public readonly SubscriberList<EntityTarget, AppBroadcast> EntitySubscribers;

	public readonly SubscriberList<ClanTarget, AppBroadcast> ClanSubscribers;

	public readonly SubscriberList<CameraTarget, AppBroadcast> CameraSubscribers;

	public Listener(IPAddress ipAddress, int port)
	{
		Address = ipAddress;
		Port = port;
		_ipTokenBuckets = new TokenBucketList<IPAddress>(50.0, 15.0);
		_ipBans = new BanList<IPAddress>();
		_playerTokenBuckets = new TokenBucketList<ulong>(25.0, 3.0);
		_pairingTokenBuckets = new TokenBucketList<ulong>(5.0, 0.1);
		_messageQueue = new Queue<Message>();
		_syncContext = SynchronizationContext.Current;
		_server = new WebSocketServer($"ws://{Address}:{Port}/", supportDualStack: true, 100, App.maxconnections, App.maxconnectionsperip);
		_server.Start(delegate(IWebSocketConnection socket)
		{
			IPAddress clientIpAddress = socket.ConnectionInfo.ClientIpAddress;
			if (_ipBans.IsBanned(clientIpAddress))
			{
				socket.Close();
			}
			else
			{
				socket.OnPing = delegate(Span<byte> data)
				{
					MemoryBuffer memoryBuffer = new MemoryBuffer(data.Length);
					data.CopyTo(memoryBuffer);
					socket.SendPong(memoryBuffer.Slice(data.Length));
				};
				string path = socket.ConnectionInfo.Path;
				if (path != null && path.StartsWith("/backhaul/", StringComparison.Ordinal))
				{
					if (!IsValidBackhaulSecret(path.Substring("/backhaul/".Length)))
					{
						socket.Close();
					}
					else
					{
						BackhaulConnection backhaul = new BackhaulConnection(this, socket, _syncContext);
						socket.OnBinary = backhaul.OnMessage;
						socket.OnClose = delegate
						{
							_syncContext.Post(delegate(object c)
							{
								((BackhaulConnection)c).OnBackhaulClosed();
							}, backhaul);
						};
						socket.OnError = LogSocketError;
					}
				}
				else
				{
					long connectionId = NextConnectionId();
					Connection conn = new Connection(connectionId, this, new FleckTransport(socket), 0uL);
					socket.OnClose = delegate
					{
						_syncContext.Post(delegate(object c)
						{
							((Connection)c).OnClose();
						}, conn);
					};
					socket.OnBinary = conn.OnMessage;
					socket.OnError = LogSocketError;
				}
			}
		});
		_stopwatch = new Stopwatch();
		PlayerSubscribers = new SubscriberList<PlayerTarget, AppBroadcast>(this);
		EntitySubscribers = new SubscriberList<EntityTarget, AppBroadcast>(this);
		ClanSubscribers = new SubscriberList<ClanTarget, AppBroadcast>(this);
		CameraSubscribers = new SubscriberList<CameraTarget, AppBroadcast>(this, 30.0);
	}

	internal long NextConnectionId()
	{
		return Interlocked.Increment(ref _nextConnectionId);
	}

	private static bool IsValidBackhaulSecret(string provided)
	{
		string secret = Server.Secret;
		if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(provided))
		{
			return false;
		}
		if (provided.Length != secret.Length)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < provided.Length; i++)
		{
			num |= provided[i] ^ secret[i];
		}
		return num == 0;
	}

	private static void LogSocketError(Exception ex)
	{
		if (App.logexceptions && !(ex is WebSocketException))
		{
			UnityEngine.Debug.LogError(ex);
		}
	}

	public void Dispose()
	{
		_server?.Dispose();
	}

	internal void Enqueue(Connection connection, MemoryBuffer data)
	{
		lock (_messageQueue)
		{
			if (!App.update || _messageQueue.Count >= App.queuelimit)
			{
				data.Dispose();
				return;
			}
			Message item = new Message(connection, data);
			_messageQueue.Enqueue(item);
		}
	}

	public bool Update()
	{
		if (!App.update)
		{
			return false;
		}
		bool result = false;
		using (TimeWarning.New("CompanionServer.MessageQueue"))
		{
			lock (_messageQueue)
			{
				_stopwatch.Restart();
				while (_messageQueue.Count > 0 && _stopwatch.Elapsed.TotalMilliseconds < 5.0)
				{
					Message message = _messageQueue.Dequeue();
					Dispatch(message);
					result = true;
				}
			}
		}
		if ((float)_lastCleanup >= 3f)
		{
			_lastCleanup = 0f;
			_ipTokenBuckets.Cleanup();
			_ipBans.Cleanup();
			_playerTokenBuckets.Cleanup();
			_pairingTokenBuckets.Cleanup();
		}
		return result;
	}

	private void Dispatch(Message message)
	{
		MemoryBuffer buffer = message.Buffer;
		AppRequest appRequest;
		try
		{
			Stream.SetData(message.Buffer.Data, 0, message.Buffer.Length);
			appRequest = Facepunch.Pool.Get<AppRequest>();
			appRequest.ReadFromStream(Stream);
		}
		catch
		{
			DebugEx.LogWarning($"Malformed companion packet from {message.Connection.Address}");
			message.Connection.Close();
			throw;
		}
		finally
		{
			buffer.Dispose();
		}
		if (!Handle<AppEmpty, Info>(appRequest.getInfo, message.Connection, appRequest) && !Handle<AppEmpty, CompanionServer.Handlers.Time>(appRequest.getTime, message.Connection, appRequest) && !Handle<AppEmpty, Map>(appRequest.getMap, message.Connection, appRequest) && !Handle<AppEmpty, TeamInfo>(appRequest.getTeamInfo, message.Connection, appRequest) && !Handle<AppEmpty, TeamChat>(appRequest.getTeamChat, message.Connection, appRequest) && !Handle<AppSendMessage, SendTeamChat>(appRequest.sendTeamMessage, message.Connection, appRequest) && !Handle<AppEmpty, EntityInfo>(appRequest.getEntityInfo, message.Connection, appRequest) && !Handle<AppSetEntityValue, SetEntityValue>(appRequest.setEntityValue, message.Connection, appRequest) && !Handle<AppEmpty, CheckSubscription>(appRequest.checkSubscription, message.Connection, appRequest) && !Handle<AppFlag, SetSubscription>(appRequest.setSubscription, message.Connection, appRequest) && !Handle<AppEmpty, MapMarkers>(appRequest.getMapMarkers, message.Connection, appRequest) && !Handle<AppPromoteToLeader, PromoteToLeader>(appRequest.promoteToLeader, message.Connection, appRequest) && !Handle<AppEmpty, CompanionServer.Handlers.ClanInfo>(appRequest.getClanInfo, message.Connection, appRequest) && !Handle<AppEmpty, ClanChat>(appRequest.getClanChat, message.Connection, appRequest) && !Handle<AppSendMessage, SetClanMotd>(appRequest.setClanMotd, message.Connection, appRequest) && !Handle<AppSendMessage, SendClanChat>(appRequest.sendClanMessage, message.Connection, appRequest) && !Handle<AppGetNexusAuth, NexusAuth>(appRequest.getNexusAuth, message.Connection, appRequest) && !Handle<AppCameraSubscribe, CameraSubscribe>(appRequest.cameraSubscribe, message.Connection, appRequest) && !Handle<AppEmpty, CameraUnsubscribe>(appRequest.cameraUnsubscribe, message.Connection, appRequest) && !Handle<AppCameraInput, CameraInput>(appRequest.cameraInput, message.Connection, appRequest) && !Handle<AppTeamKick, TeamKick>(appRequest.kickFromTeam, message.Connection, appRequest))
		{
			AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
			appResponse.seq = appRequest.seq;
			appResponse.error = Facepunch.Pool.Get<AppError>();
			appResponse.error.error = "unhandled";
			message.Connection.Send(appResponse);
			appRequest.Dispose();
		}
	}

	private bool Handle<TProto, THandler>(TProto protocol, Connection connection, AppRequest request) where TProto : class, IProto where THandler : BaseHandler<TProto>, new()
	{
		if (protocol == null)
		{
			return false;
		}
		THandler obj = Facepunch.Pool.Get<THandler>();
		obj.Initialize(_playerTokenBuckets, connection, request, protocol);
		try
		{
			ValidationResult validationResult = obj.Validate();
			switch (validationResult)
			{
			case ValidationResult.Rejected:
				connection.Close();
				break;
			default:
				obj.SendError(validationResult.ToErrorCode());
				break;
			case ValidationResult.Success:
			{
				ValueTask execution = obj.Execute();
				if (!execution.IsCompleted)
				{
					AwaitExecution(execution, obj);
					return true;
				}
				execution.GetAwaiter().GetResult();
				break;
			}
			}
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"AppRequest threw an exception: {arg}");
			obj.SendError("server_error");
		}
		Facepunch.Pool.Free(ref obj);
		return true;
	}

	private static async void AwaitExecution<THandler>(ValueTask execution, THandler handler) where THandler : class, CompanionServer.Handlers.IHandler, new()
	{
		try
		{
			await execution;
		}
		catch (Exception arg)
		{
			UnityEngine.Debug.LogError($"AppRequest threw an exception: {arg}");
			try
			{
				handler.SendError("server_error");
			}
			catch (Exception arg2)
			{
				UnityEngine.Debug.LogError($"Failed to send the error response: {arg2}");
			}
		}
		Facepunch.Pool.Free(ref handler);
	}

	public void BroadcastTo(List<Connection> targets, AppBroadcast broadcast)
	{
		MemoryBuffer broadcastBuffer = GetBroadcastBuffer(broadcast);
		foreach (Connection target in targets)
		{
			target.SendRaw(broadcastBuffer.DontDispose());
		}
		broadcastBuffer.Dispose();
	}

	private static MemoryBuffer GetBroadcastBuffer(AppBroadcast broadcast)
	{
		using BufferStream bufferStream = Facepunch.Pool.Get<BufferStream>().Initialize();
		using AppMessage appMessage = Facepunch.Pool.Get<AppMessage>();
		appMessage.broadcast = broadcast;
		appMessage.WriteToStream(bufferStream);
		MemoryBuffer memoryBuffer = new MemoryBuffer(bufferStream.Length);
		bufferStream.GetBuffer().CopyTo(memoryBuffer.Data, 0);
		return memoryBuffer.Slice(bufferStream.Length);
	}

	public bool CanSendPairingNotification(ulong playerId)
	{
		return _pairingTokenBuckets.Get(playerId).TryTake(1.0);
	}
}
