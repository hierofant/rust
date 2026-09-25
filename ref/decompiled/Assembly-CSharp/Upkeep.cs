using System;

public class Upkeep : PrefabAttribute
{
	public float upkeepMultiplier = 1f;

	public bool highWall;

	protected override Type GetIndexedType()
	{
		return typeof(Upkeep);
	}
}
