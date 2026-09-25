using Facepunch;

namespace ConVar;

[Factory("unsafe")]
public class Unsafe : ConsoleSystem
{
	private const string MultithreadedScriptingDoc = "Controls multithreading access to scripting API - can be fast, but unsafe. Disabling can help with instability";

	private const string EnableDebugMTLockDoc = "Controls DebugMTLock checks, which help track down thread races. Has minor perf impact, so prefer to keep disabled";

	private const string DebugMTLockMaxFramesDoc = "How many frames of a stack to emit";

	[ServerVar(Help = "Controls multithreading access to scripting API - can be fast, but unsafe. Disabling can help with instability", Default = "1")]
	[ClientVar(Help = "Controls multithreading access to scripting API - can be fast, but unsafe. Disabling can help with instability", Default = "1")]
	public static bool UseMultithreadedScripting
	{
		get
		{
			return UnsafeScriptingAccess.Enabled;
		}
		set
		{
			UnsafeScriptingAccess.Enabled = value;
		}
	}

	[ServerVar(Help = "Controls DebugMTLock checks, which help track down thread races. Has minor perf impact, so prefer to keep disabled")]
	[ClientVar(Help = "Controls DebugMTLock checks, which help track down thread races. Has minor perf impact, so prefer to keep disabled")]
	public static bool EnableDebugMTLock
	{
		get
		{
			return DebugMTLock.Enabled;
		}
		set
		{
			DebugMTLock.Enabled = value;
		}
	}

	[ServerVar(Help = "How many frames of a stack to emit", Default = "5")]
	[ClientVar(Help = "How many frames of a stack to emit", Default = "5")]
	public static int DebugMTLockMaxFrames
	{
		get
		{
			return DebugMTLock.MaxFrameCountToEmit;
		}
		set
		{
			DebugMTLock.MaxFrameCountToEmit = value;
		}
	}
}
