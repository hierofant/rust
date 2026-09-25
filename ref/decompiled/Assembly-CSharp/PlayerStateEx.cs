using Facepunch.Math;
using ProtoBuf;

public static class PlayerStateEx
{
	public static bool IsSaveStale(this PlayerState state)
	{
		int protocol = state.protocol;
		uint seed = state.seed;
		int saveCreatedTime = state.saveCreatedTime;
		int num = Epoch.FromDateTime(SaveRestore.SaveCreatedTime);
		if (288 == protocol && World.Seed == seed)
		{
			return num != saveCreatedTime;
		}
		return true;
	}
}
