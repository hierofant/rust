namespace Rust.Safety;

public static class CheckExtensions
{
	public static bool IsValidAttackTarget(this BasePlayer ply)
	{
		return Check.IsValidAttackTarget(ply);
	}

	public static bool IsInsideDeepSea(this BaseNetworkable entity)
	{
		return DeepSeaManager.IsInsideDeepSea(entity.transform.position);
	}
}
