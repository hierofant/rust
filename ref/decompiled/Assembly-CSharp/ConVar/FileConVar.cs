namespace ConVar;

[Factory("file")]
public class FileConVar : ConsoleSystem
{
	[ClientVar(Help = "(Generated) Enables verbose debug logging for the file system abstraction layer, logging all file read/write operations to the console")]
	public static bool debug
	{
		get
		{
			return FileSystem.LogDebug;
		}
		set
		{
			FileSystem.LogDebug = value;
		}
	}

	[ClientVar(Help = "(Generated) When enabled, measures and logs the time taken for each file system operation, helping identify slow asset or config file loading")]
	public static bool time
	{
		get
		{
			return FileSystem.LogTime;
		}
		set
		{
			FileSystem.LogTime = value;
		}
	}
}
