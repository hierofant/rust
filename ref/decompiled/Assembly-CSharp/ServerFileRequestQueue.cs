using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using UnityEngine;

public static class ServerFileRequestQueue
{
	public enum RequestKind : byte
	{
		GenericFile,
		EntityImage
	}

	private struct QueuedRequest
	{
		public RequestKind Kind;

		public BaseEntity Entity;

		public string ResponseFunction;

		public uint Crc;

		public FileStorage.Type Type;

		public uint Part;

		public bool RespondIfNotFound;
	}

	private class ConnectionState : Facepunch.Pool.IPooled
	{
		public Connection connection;

		public double byteBudget;

		public double lastRefill;

		public readonly Queue<QueuedRequest> queue = new Queue<QueuedRequest>();

		public void EnterPool()
		{
		}

		public void LeavePool()
		{
			connection = null;
			byteBudget = ConVar.Server.filerequestbytesburst;
			lastRefill = UnityEngine.Time.realtimeSinceStartupAsDouble;
			queue.Clear();
		}
	}

	private static readonly Dictionary<Connection, ConnectionState> States = new Dictionary<Connection, ConnectionState>();

	public static void Request(Connection connection, BaseEntity entity, RequestKind kind, uint crc, FileStorage.Type type, string responseFunction, uint part = 0u, bool respondIfNotFound = false)
	{
		if (connection == null || !entity.IsValid())
		{
			return;
		}
		ConnectionState connectionState = FindState(connection);
		if (connectionState == null)
		{
			connectionState = Facepunch.Pool.Get<ConnectionState>();
			connectionState.connection = connection;
			States.Add(connection, connectionState);
		}
		RefillBudget(connectionState);
		if (ConVar.Server.filerequestdebug)
		{
			Debug.Log($"[FileRequest] {connection} requested {type} crc {crc} part {part} from {entity.ShortPrefabName}[{entity.net.ID}] - {UsageString(connectionState)}");
		}
		QueuedRequest queuedRequest = default(QueuedRequest);
		queuedRequest.Kind = kind;
		queuedRequest.Entity = entity;
		queuedRequest.ResponseFunction = responseFunction;
		queuedRequest.Crc = crc;
		queuedRequest.Type = type;
		queuedRequest.Part = part;
		queuedRequest.RespondIfNotFound = respondIfNotFound;
		QueuedRequest request = queuedRequest;
		if (connectionState.queue.Count == 0 && connectionState.byteBudget > 0.0)
		{
			Send(connectionState, in request);
		}
		else if (connectionState.queue.Count < ConVar.Server.filerequestqueuelength)
		{
			connectionState.queue.Enqueue(request);
			if (ConVar.Server.filerequestdebug)
			{
				Debug.Log($"[FileRequest] {connection} over budget, request deferred - {UsageString(connectionState)}");
			}
		}
	}

	public static void Cycle()
	{
		bool filerequestdebug = ConVar.Server.filerequestdebug;
		List<Connection> obj = Facepunch.Pool.Get<List<Connection>>();
		foreach (KeyValuePair<Connection, ConnectionState> state in States)
		{
			ConnectionState value = state.Value;
			if (!value.connection.active)
			{
				obj.Add(value.connection);
				continue;
			}
			RefillBudget(value);
			while (value.queue.Count > 0 && value.byteBudget > 0.0)
			{
				QueuedRequest request = value.queue.Dequeue();
				Send(value, in request);
			}
			if (value.queue.Count == 0 && value.byteBudget >= (double)ConVar.Server.filerequestbytesburst)
			{
				obj.Add(value.connection);
			}
			else if (filerequestdebug)
			{
				Debug.Log($"[FileRequest] {value.connection} - {UsageString(value)}");
			}
		}
		foreach (Connection item in obj)
		{
			ConnectionState obj2 = States[item];
			Facepunch.Pool.Free(ref obj2);
			States.Remove(item);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static void OnDisconnected(Connection connection)
	{
		States.Remove(connection);
	}

	private static ConnectionState FindState(Connection connection)
	{
		States.TryGetValue(connection, out var value);
		return value;
	}

	private static void RefillBudget(ConnectionState state)
	{
		double realtimeSinceStartupAsDouble = UnityEngine.Time.realtimeSinceStartupAsDouble;
		double num = realtimeSinceStartupAsDouble - state.lastRefill;
		state.lastRefill = realtimeSinceStartupAsDouble;
		state.byteBudget = Math.Min(ConVar.Server.filerequestbytesburst, state.byteBudget + num * (double)ConVar.Server.filerequestbytespersecond);
	}

	private static void Send(ConnectionState state, in QueuedRequest request)
	{
		if (request.Entity.IsValid())
		{
			int num = ((request.Kind != RequestKind.EntityImage) ? request.Entity.SendRequestedFile(state.connection, request.ResponseFunction, request.Crc, request.Type, request.Part, request.RespondIfNotFound) : ((request.Entity is ImageStorageEntity imageStorageEntity) ? imageStorageEntity.SendRequestedImage(state.connection) : 0));
			int num2 = num;
			state.byteBudget -= Math.Max(num2, 32768);
			if (ConVar.Server.filerequestdebug)
			{
				Debug.Log($"[FileRequest] {state.connection} sent {num2.FormatBytes()} - {UsageString(state)}");
			}
		}
	}

	private static string UsageString(ConnectionState state)
	{
		long num = ConVar.Server.filerequestbytesburst;
		long input = num - (long)state.byteBudget;
		return $"used {input.FormatBytes()} of {num.FormatBytes()}, queued {state.queue.Count}";
	}
}
