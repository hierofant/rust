using Facepunch.Ping;

namespace ConVar;

[Factory("ping")]
public class Ping : ConsoleSystem
{
	[ClientVar(Help = "(Generated) Number of ping samples collected per estimation cycle; more samples give a more accurate average latency but take longer to complete")]
	[ServerVar(Help = "(Generated) Number of ping samples collected per estimation cycle; more samples give a more accurate average latency but take longer to complete")]
	public static int ping_samples
	{
		get
		{
			return PingEstimater.numSamples;
		}
		set
		{
			PingEstimater.numSamples = value;
		}
	}

	[ClientVar(Help = "(Generated) When enabled, ping estimation sends samples to all servers in parallel for faster results; uses more bandwidth simultaneously")]
	[ServerVar(Help = "(Generated) When enabled, ping estimation sends samples to all servers in parallel for faster results; uses more bandwidth simultaneously")]
	public static bool ping_parallel
	{
		get
		{
			return PingEstimater.parallel;
		}
		set
		{
			PingEstimater.parallel = value;
		}
	}

	[ClientVar(Help = "(Generated) Interval in minutes between automatic background ping estimation refreshes for server list region latency sorting")]
	[ServerVar(Help = "(Generated) Interval in minutes between automatic background ping estimation refreshes for server list region latency sorting")]
	public static int ping_refresh_interval
	{
		get
		{
			return PingEstimater.refreshIntervalMinutes;
		}
		set
		{
			PingEstimater.refreshIntervalMinutes = value;
		}
	}

	[ServerVar(Help = "(Generated) When enabled, region ping estimates are automatically refreshed in the background to keep server list latency data up to date")]
	[ClientVar(Help = "(Generated) When enabled, region ping estimates are automatically refreshed in the background to keep server list latency data up to date")]
	public static bool auto_refresh_region
	{
		get
		{
			return PingEstimater.AutoRefresh;
		}
		set
		{
			PingEstimater.AutoRefresh = value;
		}
	}

	[ServerVar(Help = "(Generated) When enabled, logs ping estimation results to the console; useful for debugging regional latency measurement accuracy")]
	[ClientVar(Help = "(Generated) When enabled, logs ping estimation results to the console; useful for debugging regional latency measurement accuracy")]
	public static bool ping_estimate_logging
	{
		get
		{
			return PingEstimater.logging;
		}
		set
		{
			PingEstimater.logging = value;
		}
	}

	[ServerVar(Help = "(Generated) When enabled, the ping estimator collects latency samples to regional servers; disable to suppress background ping traffic")]
	[ClientVar(Help = "(Generated) When enabled, the ping estimator collects latency samples to regional servers; disable to suppress background ping traffic")]
	public static bool ping_estimation
	{
		get
		{
			return PingEstimater.enabled;
		}
		set
		{
			PingEstimater.enabled = value;
		}
	}
}
