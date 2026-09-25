using System.Net;
using Fleck;

namespace CompanionServer;

public interface IConnectionTransport
{
	IPAddress Address { get; }

	bool IsAvailable { get; }

	void Send(MemoryBuffer data);

	void Close();
}
