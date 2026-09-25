using System;
using System.Collections.Generic;
using Facepunch;
using Network;

public struct RpcTarget
{
	[Flags]
	public enum RpcTargetFlags
	{
		Player = 1,
		Spectators = 2,
		ClientDemoRecorders = 4,
		All = -1
	}

	public string Function;

	public SendInfo Connections;

	public bool ToNetworkGroup;

	public bool UsingPooledConnections;

	public static RpcTarget NetworkGroup(string funcName)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.ToNetworkGroup = true;
		return result;
	}

	public static RpcTarget NetworkGroup(string funcName, BaseNetworkable entity)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = new SendInfo(entity.net.group.subscribers);
		return result;
	}

	public static RpcTarget NetworkGroup(string funcName, BaseNetworkable entity, SendMethod method, Priority priority)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = new SendInfo(entity.net.group.subscribers)
		{
			method = method,
			priority = priority
		};
		return result;
	}

	public static RpcTarget Player(string funcName, BasePlayer target)
	{
		return Player(funcName, target.IsValid() ? target.net.connection : null);
	}

	public static RpcTarget Player(string funcName, Connection connection)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = new SendInfo(connection);
		return result;
	}

	public static RpcTarget Players(string funcName, List<Connection> connections)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = new SendInfo(connections);
		return result;
	}

	public static RpcTarget Players(string funcName, List<Connection> connections, SendMethod method, Priority priority)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = new SendInfo(connections)
		{
			method = method,
			priority = priority
		};
		return result;
	}

	public static RpcTarget SendInfo(string funcName, SendInfo sendInfo)
	{
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = sendInfo;
		return result;
	}

	public static RpcTarget FromFlags(RpcTargetFlags rpcTargetFlags, string funcName, BasePlayer player)
	{
		if (!player.IsValid() || rpcTargetFlags == (RpcTargetFlags)0)
		{
			return default(RpcTarget);
		}
		List<Connection> list = Pool.Get<List<Connection>>();
		HashSet<Connection> obj = Pool.Get<HashSet<Connection>>();
		if ((rpcTargetFlags & RpcTargetFlags.Player) != 0)
		{
			Connection connection = player.net.connection;
			if (connection != null && obj.Add(connection))
			{
				list.Add(player.net.connection);
			}
		}
		if ((rpcTargetFlags & RpcTargetFlags.Spectators) != 0 && player.IsBeingSpectated)
		{
			ReadOnlySpan<BasePlayer> spectators = player.GetSpectators();
			for (int i = 0; i < spectators.Length; i++)
			{
				BasePlayer basePlayer = spectators[i];
				Connection connection2 = basePlayer.net.connection;
				if (connection2 != null && obj.Add(connection2))
				{
					list.Add(basePlayer.net.connection);
				}
			}
		}
		if ((rpcTargetFlags & RpcTargetFlags.ClientDemoRecorders) != 0)
		{
			for (int j = 0; j < BasePlayer.playersRecordingClientDemos.Count; j++)
			{
				BasePlayer basePlayer2 = BasePlayer.playersRecordingClientDemos[j];
				Connection connection3 = basePlayer2.net.connection;
				if (connection3 != null && !obj.Contains(connection3) && player.ShouldNetworkToSkipOcclusion(basePlayer2))
				{
					obj.Add(connection3);
					list.Add(basePlayer2.net.connection);
				}
			}
		}
		Pool.FreeUnmanaged(ref obj);
		RpcTarget result = default(RpcTarget);
		result.Function = funcName;
		result.Connections = new SendInfo(list);
		result.UsingPooledConnections = true;
		return result;
	}

	public static RpcTarget PlayerAndSpectators(string funcName, BasePlayer player)
	{
		return FromFlags(RpcTargetFlags.Player | RpcTargetFlags.Spectators, funcName, player);
	}
}
