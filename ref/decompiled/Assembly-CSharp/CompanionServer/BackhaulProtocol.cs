namespace CompanionServer;

public static class BackhaulProtocol
{
	public const string PathPrefix = "/backhaul/";

	public const int HeaderSize = 5;

	public const byte OpData = 0;

	public const byte OpOpen = 1;

	public const byte OpClose = 2;
}
