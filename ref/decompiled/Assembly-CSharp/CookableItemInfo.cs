public class CookableItemInfo
{
	public ItemDefinition becomeOnCooked;

	public float cookTime;

	public float amountOfBecome;

	public int lowTemp;

	public int highTemp;

	public bool setCookingFlag;

	public CookableItemInfo(ItemModCookable itemMod)
	{
		becomeOnCooked = itemMod.becomeOnCooked;
		cookTime = itemMod.cookTime;
		amountOfBecome = itemMod.amountOfBecome;
		lowTemp = itemMod.lowTemp;
		highTemp = itemMod.highTemp;
		setCookingFlag = itemMod.setCookingFlag;
	}

	public CookableItemInfo(ItemModCompostable itemMod)
	{
		becomeOnCooked = ItemManager.Items.Fertilizer;
		amountOfBecome = itemMod.TotalFertilizerProduced;
		cookTime = 300f;
		lowTemp = 29;
		highTemp = 31;
		setCookingFlag = false;
	}

	public bool CanBeCookedByAtTemperature(float temperature)
	{
		if (temperature > (float)lowTemp)
		{
			return temperature < (float)highTemp;
		}
		return false;
	}
}
