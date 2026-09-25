using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace Rust.Rendering.IndirectInstancing;

public static class MotionToggleEx
{
	public static void BroadcastMotionStartSlow(this GameObject go)
	{
		List<IMotionToggle> obj = Pool.Get<List<IMotionToggle>>();
		go.GetComponentsInChildren(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].MotionStart();
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public static void BroadcastBeforeMaterialChange(this GameObject go)
	{
		List<IMotionToggle> obj = Pool.Get<List<IMotionToggle>>();
		go.GetComponentsInChildren(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].OnBeforeMaterialChange();
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public static void BroadcastAfterMaterialChange(this GameObject go)
	{
		List<IMotionToggle> obj = Pool.Get<List<IMotionToggle>>();
		go.GetComponentsInChildren(obj);
		for (int i = 0; i < obj.Count; i++)
		{
			obj[i].OnAfterMaterialChange();
		}
		Pool.FreeUnmanaged(ref obj);
	}
}
