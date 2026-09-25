using UnityEngine;

public static class BaseNetworkableEx
{
	public static bool IsValid(this BaseNetworkable ent)
	{
		if (ent == null)
		{
			return false;
		}
		if (ent.net == null)
		{
			return false;
		}
		return true;
	}

	public static bool Is<T>(this Object unityObject, out T castedUnityObject) where T : Object
	{
		castedUnityObject = null;
		if (unityObject == null)
		{
			return false;
		}
		castedUnityObject = unityObject as T;
		if (castedUnityObject == null)
		{
			return false;
		}
		return true;
	}

	public static bool IsRealNull(this BaseNetworkable ent)
	{
		return (object)ent == null;
	}
}
