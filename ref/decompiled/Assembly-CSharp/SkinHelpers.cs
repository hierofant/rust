using System.Collections.Generic;
using System.Linq;
using Rust.Workshop;
using UnityEngine;

public static class SkinHelpers
{
	public const int RandomSkinId = -1;

	private static Dictionary<int, int> _redirectSkinIdLookup = new Dictionary<int, int>();

	public static bool IsRandom(int skinId)
	{
		return skinId == -1;
	}

	public static void SetSkin(GameObject itemModel, ItemDefinition itemDef, ulong skinID)
	{
		if (itemDef == null)
		{
			return;
		}
		ItemSkinDirectory.Skin skin = itemDef.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == skinID);
		if ((ulong)skin.id == skinID)
		{
			ItemSkin itemSkin = skin.invItem as ItemSkin;
			if (itemSkin != null)
			{
				itemSkin.ApplySkin(itemModel);
			}
		}
		else if (skinID != 0L)
		{
			Rust.Workshop.WorkshopSkin.Apply(itemModel, skinID);
		}
	}

	public static bool TryGetRedirectSkinId(ItemDefinition itemDef, out int skinId)
	{
		skinId = 0;
		ItemDefinition itemDefinition = itemDef?.isRedirectOf;
		if (itemDefinition != null)
		{
			if (_redirectSkinIdLookup.TryGetValue(itemDef.itemid, out skinId))
			{
				return true;
			}
			ItemSkinDirectory.Skin[] skins = itemDefinition.skins;
			for (int i = 0; i < skins.Length; i++)
			{
				ItemSkinDirectory.Skin skin = skins[i];
				if (skin.invItem is ItemSkin itemSkin && itemSkin.Redirect == itemDef)
				{
					skinId = skin.id;
					_redirectSkinIdLookup[itemDef.itemid] = skin.id;
					return true;
				}
			}
		}
		return false;
	}
}
