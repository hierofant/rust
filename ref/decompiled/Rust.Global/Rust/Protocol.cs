namespace Rust;

public static class Protocol
{
	public const int network = 2633;

	public const int save = 288;

	public const int report = 1;

	public const int persistance = 17;

	public const int analytics_db = 1;

	public const int ping = 1;

	public static string printable => 2633 + "." + 288 + "." + 1;
}
