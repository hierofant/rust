using ConVar;
using Facepunch;
using Spatial;
using UnityEngine;

public class OverfishedArea : BaseEntity
{
	public static Grid<OverfishedArea> OverfishedGrid = new Grid<OverfishedArea>();

	public override void ServerInit()
	{
		base.ServerInit();
		OverfishedGrid.Add(this, base.transform.position.x, base.transform.position.z);
		Invoke(KillMe, 60f * Fishing.overfishedAreaDurationMinutes);
	}

	private void KillMe()
	{
		Kill();
	}

	internal override void DoServerDestroy()
	{
		OverfishedGrid.Remove(this);
		base.DoServerDestroy();
	}

	public static OverfishedArea GetOverfishedAreaAtPosition(Vector3 position)
	{
		using (TimeWarning.New("OverfishedArea.GetOverfishedAreaAtPosition()"))
		{
			using PooledList<OverfishedArea> pooledList = Facepunch.Pool.Get<PooledList<OverfishedArea>>();
			OverfishedGrid.Query(position.x, position.z, Fishing.overfishedAreaRadius, pooledList);
			foreach (OverfishedArea item in pooledList)
			{
				if (Fishing.debugOverfishing)
				{
					Debug.Log($"OVERFISHED AREA QUERY | Found an area at position {position}, at distance: {Vector3.Distance(item.transform.position, position)}");
				}
				if (Vector3.Distance(item.transform.position, position) < Fishing.overfishedAreaRadius)
				{
					if (Fishing.debugOverfishing)
					{
						Debug.Log($"OVERFISHED AREA QUERY | Accepting area at position {position} as overfished!", item);
					}
					return item;
				}
			}
			return null;
		}
	}
}
