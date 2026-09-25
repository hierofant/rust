using System.Collections.Generic;

public class CachedBoatParts<T>
{
	private List<T> cached = new List<T>();

	public List<T> Cached => Get();

	public List<T> RecacheAndGet(PlayerBoat boat)
	{
		Recache(boat);
		return Get();
	}

	public void Recache(PlayerBoat boat)
	{
		cached.Clear();
		foreach (BaseEntity child in boat.children)
		{
			if (child is T item)
			{
				cached.Add(item);
			}
		}
	}

	private List<T> Get()
	{
		return cached;
	}
}
