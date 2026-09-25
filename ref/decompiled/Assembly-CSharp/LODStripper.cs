using System;
using UnityEngine;

public class LODStripper
{
	private static void StripLods(GameObject target)
	{
		StripLods(target, new Type[5]
		{
			typeof(Mesh),
			typeof(Collision),
			typeof(Animator),
			typeof(DrawSkeleton),
			typeof(ObjectMotionVectorFix)
		});
	}

	public static void StripLods(GameObject target, Type[] keepComponents)
	{
		if (target == null)
		{
			Debug.LogWarning("You have to select something first Paddy...");
			return;
		}
		RendererLOD[] componentsInChildren = target.GetComponentsInChildren<RendererLOD>();
		LODGroup[] componentsInChildren2 = target.GetComponentsInChildren<LODGroup>();
		RendererLOD[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			StripLod(array[i]);
		}
		LODGroup[] array2 = componentsInChildren2;
		for (int i = 0; i < array2.Length; i++)
		{
			StripLod(array2[i]);
		}
		StripLod(target.GetComponent<RendererLOD>());
		StripLod(target.GetComponent<LODGroup>());
		StripComponentsNotOfType(target, keepComponents);
	}

	private static void StripLod(RendererLOD lod)
	{
		if (lod == null)
		{
			return;
		}
		for (int i = 1; i < lod.States.Length; i++)
		{
			if (lod.States[i].renderer != null)
			{
				Destroy(lod.States[i].renderer.gameObject);
			}
		}
		Destroy(lod);
	}

	private static void StripLod(LODGroup lod)
	{
		if (lod == null)
		{
			return;
		}
		LOD[] lODs = lod.GetLODs();
		for (int i = 1; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			foreach (Renderer renderer in renderers)
			{
				if (renderer != null)
				{
					Destroy(renderer.gameObject);
				}
			}
		}
		Destroy(lod);
	}

	private static void Destroy(UnityEngine.Object target)
	{
		if (!Application.isPlaying)
		{
			UnityEngine.Object.DestroyImmediate(target);
		}
		else
		{
			UnityEngine.Object.Destroy(target);
		}
	}

	private static void StripComponentsOfType(GameObject go, Type component)
	{
		Component component2 = go.GetComponent(component);
		if (component2 != null)
		{
			Destroy(component2);
		}
		Component[] componentsInChildren = go.GetComponentsInChildren(component);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Destroy(componentsInChildren[i]);
		}
	}

	private static void StripComponentsNotOfType(GameObject go, Type[] components)
	{
		MonoBehaviour[] components2 = go.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour target in components2)
		{
			if (IsComponentNotOfType(target, components))
			{
				Destroy(target);
			}
		}
		foreach (Transform item in go.transform)
		{
			StripComponentsNotOfType(item.gameObject, components);
		}
	}

	private static bool IsComponentNotOfType(MonoBehaviour target, Type[] components)
	{
		foreach (Type type in components)
		{
			if (type == target.GetType() || target.GetType().IsSubclassOf(type))
			{
				return false;
			}
		}
		return true;
	}
}
