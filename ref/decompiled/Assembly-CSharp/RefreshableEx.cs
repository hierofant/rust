using System.Collections.Generic;
using Facepunch;
using UnityEngine;

internal static class RefreshableEx
{
	public static void BroadcastRefresh(this GameObject go)
	{
		List<IRefreshable> obj = Pool.Get<List<IRefreshable>>();
		go.GetComponentsInChildren(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].Refresh();
		}
		Pool.FreeUnmanaged(ref obj);
	}
}
