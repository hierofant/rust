using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class CH47PathFinder : BasePathFinder
{
	public List<Vector3> visitedPatrolPoints = new List<Vector3>();

	public override Vector3 GetRandomPatrolPoint()
	{
		Vector3 zero = Vector3.zero;
		MonumentInfo monumentInfo = null;
		if (TerrainMeta.Path != null && TerrainMeta.Path.Monuments != null && TerrainMeta.Path.Monuments.Count > 0)
		{
			using PooledList<MonumentInfo> pooledList = Pool.Get<PooledList<MonumentInfo>>();
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				if (monument.Type == MonumentType.Cave || monument.Type == MonumentType.WaterWell || monument.Tier == MonumentTier.Tier0 || monument.IsSafeZone || (monument.Tier & MonumentTier.Tier0) > (MonumentTier)0)
				{
					continue;
				}
				bool flag = false;
				foreach (Vector3 visitedPatrolPoint in visitedPatrolPoints)
				{
					if (Vector3Ex.Distance2D(monument.transform.position, visitedPatrolPoint) < 100f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					pooledList.Add(monument);
				}
			}
			if (pooledList.Count > 0)
			{
				int index = Random.Range(0, pooledList.Count);
				monumentInfo = pooledList[index];
			}
			if (monumentInfo == null)
			{
				visitedPatrolPoints.Clear();
				monumentInfo = GetRandomValidMonumentInfo();
			}
		}
		if (monumentInfo != null)
		{
			visitedPatrolPoints.Add(monumentInfo.transform.position);
			zero = monumentInfo.transform.position;
		}
		else
		{
			float x = TerrainMeta.Size.x;
			float y = 30f;
			zero = Vector3Ex.Range(-1f, 1f);
			zero.y = 0f;
			zero.Normalize();
			zero *= x * Random.Range(0f, 0.75f);
			zero.y = y;
		}
		float waterOrTerrainSurface = WaterLevel.GetWaterOrTerrainSurface(zero, waves: false, volumes: false);
		float num = waterOrTerrainSurface;
		if (Physics.SphereCast(zero + new Vector3(0f, 200f, 0f), 20f, Vector3.down, out var hitInfo, 300f, 1218511105))
		{
			num = Mathf.Max(hitInfo.point.y, waterOrTerrainSurface);
		}
		zero.y = num + 30f;
		return zero;
	}

	private MonumentInfo GetRandomValidMonumentInfo()
	{
		int count = TerrainMeta.Path.Monuments.Count;
		int num = Random.Range(0, count);
		for (int i = 0; i < count; i++)
		{
			int num2 = i + num;
			if (num2 >= count)
			{
				num2 -= count;
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.Monuments[num2];
			if (monumentInfo.Type != 0 && monumentInfo.Type != MonumentType.WaterWell && monumentInfo.Tier != MonumentTier.Tier0 && !monumentInfo.IsSafeZone)
			{
				return monumentInfo;
			}
		}
		return null;
	}
}
