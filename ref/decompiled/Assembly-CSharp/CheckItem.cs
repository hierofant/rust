using System.Collections.Generic;
using System.Linq;
using Facepunch;
using ProtoBuf;

public static class CheckItem
{
	private static ItemContainer _testFromContainer;

	private static ItemContainer _testToContainer;

	private static void PrepareTestContainers()
	{
		if (_testFromContainer == null)
		{
			_testFromContainer = new ItemContainer();
		}
		if (_testToContainer == null)
		{
			_testToContainer = new ItemContainer();
		}
		_testFromContainer.Clear();
		_testToContainer.Clear();
	}

	public static bool CanMoveToContainer(ItemContainer from, ItemContainer to)
	{
		PrepareTestContainers();
		if (from == null || to == null)
		{
			return false;
		}
		if (to.IsFull())
		{
			return false;
		}
		if (from.IsEmpty())
		{
			return false;
		}
		ProtoBuf.ItemContainer container = from.Save(bIncludeContainer: false);
		_testFromContainer.Load(container);
		foreach (Item item in from.itemList)
		{
			_testFromContainer.AddItem(item.info, item.amount, item.skin);
		}
		ProtoBuf.ItemContainer container2 = to.Save(bIncludeContainer: false);
		_testToContainer.Load(container2);
		foreach (Item item2 in to.itemList)
		{
			_testToContainer.AddItem(item2.info, item2.amount, item2.skin);
		}
		return _testFromContainer.TryMoveAllItems(_testToContainer);
	}

	public static bool CanMoveToContainer(ItemContainer to, List<Item> items)
	{
		PrepareTestContainers();
		if (to == null)
		{
			return false;
		}
		if (to.IsFull())
		{
			return false;
		}
		ProtoBuf.ItemContainer container = to.Save(bIncludeContainer: false);
		_testToContainer.Load(container);
		using PooledList<Item> pooledList = Pool.Get<PooledList<Item>>();
		pooledList.AddRange(to.itemList.Select((Item item) => ItemManager.Create(item.info, item.amount, item.skin, isServerSide: true, 0uL)));
		bool result = true;
		foreach (Item item in pooledList)
		{
			if (!item.MoveToContainer(_testToContainer))
			{
				result = false;
				break;
			}
		}
		int count = pooledList.Count;
		while (count-- > 0)
		{
			pooledList[count].Remove();
		}
		return result;
	}

	public static bool CanFitSimple(ItemContainer to, List<Item> items)
	{
		if (to == null || items == null)
		{
			return false;
		}
		if (to.IsFull())
		{
			return false;
		}
		int num = to.capacity - to.itemList.Count;
		int count = items.Count;
		return num >= count;
	}

	public static bool CanFitSimple(ItemContainer to, int numberOfSlots)
	{
		if (to == null)
		{
			return false;
		}
		if (to.IsFull())
		{
			return false;
		}
		return to.capacity - to.itemList.Count >= numberOfSlots;
	}

	public static bool IsVirginItem(Item item)
	{
		return item.parent == null;
	}

	public static bool IsInContainer(Item item)
	{
		return item.parent != null;
	}
}
