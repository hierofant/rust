using UnityEngine;

public class PlayerModelHairCap : MonoBehaviour
{
	[InspectorFlags]
	public HairCapMask hairCapMask;

	public void SetupHairCap(SkinSetCollection skin, float hairNum, float meshNum, float dyeNumber, float meshSubNumber, MaterialPropertyBlock block)
	{
		SkinSet skinSet = skin.Get(meshNum, meshSubNumber);
		if (skinSet == null || skinSet.HairCollection == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			if (((uint)hairCapMask & (uint)(1 << i)) == 0)
			{
				continue;
			}
			PlayerModelHair.GetRandomVariation(hairNum, i, dyeNumber, out var typeNum, out var dyeNum);
			HairType hairType = (HairType)i;
			HairSetCollection.HairSetEntry hairSetEntry = skinSet.HairCollection.Get(hairType, typeNum);
			if (!(hairSetEntry.HairSet == null))
			{
				HairDyeCollection hairDyeCollection = hairSetEntry.HairDyeCollection;
				if (!(hairDyeCollection == null))
				{
					hairDyeCollection.Get(dyeNum)?.ApplyCap(hairDyeCollection, hairType, block);
				}
			}
		}
	}
}
