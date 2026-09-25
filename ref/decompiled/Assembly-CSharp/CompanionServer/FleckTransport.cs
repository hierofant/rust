using System.Net;
using Fleck;

namespace CompanionServer;

public sealed class FleckTransport : IConnectionTransport
{
	private readonly IWebSocketConnection _connection;

	public IPAddress Address => _connection.ConnectionInfo.ClientIpAddress;

	public bool IsAvailable
	{
		get
		{
			if (_connection != null)
			{
				return _connection.IsAvailable;
			}
			return false;
		}
	}

	public FleckTransport(IWebSocketConnection connection)
	{
		_connection = connection;
	}

	public void Send(MemoryBuffer data)
	{
		_connection.Send(data);
	}

	public void Close()
	{
		_connection?.Close();
	}
}
