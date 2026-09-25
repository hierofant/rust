using System;
using Rust.Workshop;
using UnityEngine;

public class ConditionalModelWallpaper : ConditionalModel
{
	public bool softSide;

	public override GameObject InstantiateSkin(BaseEntity parent)
	{
		if (!onServer && isServer)
		{
			return null;
		}
		GameObject gameObject = gameManager.CreatePrefab(prefab.resourcePath, parent.transform, active: false);
		if (gameObject != null)
		{
			gameObject.transform.localPosition = worldPosition;
			gameObject.transform.localRotation = worldRotation;
			BuildingBlock buildingBlock = parent as BuildingBlock;
			if (buildingBlock != null)
			{
				ItemDefinition itemDefForCategory = WallpaperSettings.GetItemDefForCategory(WallpaperPlanner.Settings.GetCategory(buildingBlock, (!softSide) ? 1 : 0));
				Rust.Workshop.WorkshopSkin.Reset(gameObject);
				SkinHelpers.SetSkin(gameObject, itemDefForCategory, softSide ? buildingBlock.wallpaperID : buildingBlock.wallpaperID2);
				float num = (softSide ? buildingBlock.wallpaperRotation : buildingBlock.wallpaperRotation2);
				if (num != 0f)
				{
					Vector3 localEulerAngles = gameObject.transform.localEulerAngles;
					localEulerAngles.y += num;
					gameObject.transform.localRotation = Quaternion.Euler(localEulerAngles);
				}
				PoolableEx.AwakeFromInstantiate(gameObject);
			}
		}
		return gameObject;
	}

	protected override Type GetIndexedType()
	{
		return typeof(ConditionalModel);
	}
}
