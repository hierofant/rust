using System.Net;
using Fleck;

namespace CompanionServer;

public sealed class ChannelTransport : IConnectionTransport
{
	private readonly BackhaulConnection _backhaul;

	private readonly uint _channelId;

	public IPAddress Address { get; }

	public bool IsAvailable => _backhaul.IsAvailable;

	public ChannelTransport(BackhaulConnection backhaul, uint channelId, IPAddress address)
	{
		_backhaul = backhaul;
		_channelId = channelId;
		Address = address;
	}

	public void Send(MemoryBuffer data)
	{
		_backhaul.SendData(_channelId, data);
	}

	public void Close()
	{
		_backhaul.CloseChannelLocal(_channelId);
	}
}
