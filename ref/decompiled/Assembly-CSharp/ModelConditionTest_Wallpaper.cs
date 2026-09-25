public class ModelConditionTest_Wallpaper : ModelConditionTest
{
	public bool wantsWallpaper = true;

	public bool softSide;

	public override bool DoTest(BaseEntity ent)
	{
		BuildingBlock buildingBlock = ent as BuildingBlock;
		if (buildingBlock == null)
		{
			return false;
		}
		bool flag = buildingBlock.HasWallpaper((!softSide) ? 1 : 0);
		if (!wantsWallpaper)
		{
			return !flag;
		}
		return flag;
	}
}
