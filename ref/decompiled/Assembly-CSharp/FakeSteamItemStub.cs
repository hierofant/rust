using System;
using System.Collections.Generic;
using Rust.UI.MainMenu;
using UnityEngine;

public class FakeSteamItemStub : IPlayerItemDefinition, IEquatable<IPlayerItemDefinition>
{
	private readonly SteamInventoryItem _steamItemSource;

	private readonly IPlayerItemDefinition _itemSource;

	public readonly ItemStoreTakeover TakeoverSource;

	private string tags;

	public IPlayerItemDefinition Source => _itemSource;

	public int DefinitionId => _steamItemSource.id;

	public string Name
	{
		get
		{
			if (!TakeoverSource.IsValid())
			{
				return _steamItemSource.displayName.translated;
			}
			return TakeoverSource.NameOverride.english;
		}
	}

	public string Description => string.Empty;

	public string Type => _itemSource?.Type;

	public string IconUrl => string.Empty;

	public int LocalPrice => 0;

	public string LocalPriceFormatted => _itemSource?.LocalPriceFormatted;

	public string PriceCategory => _itemSource?.PriceCategory;

	public bool IsGenerator => false;

	public bool IsTradable => false;

	public bool IsMarketable => false;

	public string StoreTags => tags;

	public DateTime Created { get; }

	public DateTime Modified { get; }

	public string ItemShortName => _steamItemSource.itemname;

	public ulong WorkshopId
	{
		get
		{
			if (!(_steamItemSource is ItemSkin itemSkin))
			{
				return 0uL;
			}
			return itemSkin.workshopID;
		}
	}

	public ulong WorkshopDownload { get; }

	public FakeSteamItemStub(ItemStoreTakeover backing, IPlayerItemDefinition itemDef)
	{
		if (backing.Item != null)
		{
			TakeoverSource = backing;
			_steamItemSource = backing.Item;
			_itemSource = itemDef;
		}
	}

	public FakeSteamItemStub(SteamInventoryItem steamInventoryItemSource, string tags = null)
	{
		_steamItemSource = steamInventoryItemSource;
		this.tags = tags;
	}

	public FakeSteamItemStub(SteamDLCItem dlcItemBacking)
	{
		SteamInventoryItem steamInventoryItem = ScriptableObject.CreateInstance<SteamInventoryItem>();
		steamInventoryItem.id = dlcItemBacking.dlcAppID;
		steamInventoryItem.displayName = dlcItemBacking.dlcName;
		tags = "DLC";
		_steamItemSource = steamInventoryItem;
	}

	public bool Equals(IPlayerItemDefinition other)
	{
		return other.DefinitionId == DefinitionId;
	}

	public IEnumerable<PlayerItemRecipe> GetRecipesContainingThis()
	{
		return null;
	}
}
