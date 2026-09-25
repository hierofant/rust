using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

namespace Facepunch.Extend;

public static class TransformEx
{
	public static class Unsafe
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static TransformAccess* ExtractTransformAccess(TransformHandle handle)
		{
			if (handle.Equals(TransformHandle.None))
			{
				throw new ArgumentNullException("handle", "A None/default handle has been passed in, it can't be used for transform access!");
			}
			IntPtr* ptr = (IntPtr*)UnsafeUtility.AddressOf(ref handle);
			return (TransformAccess*)(void*)(*ptr);
		}

		public unsafe static Vector3 GetLocalPosMT(in TransformHandle handle)
		{
			return ExtractTransformAccess(handle)->localPosition;
		}

		public unsafe static Quaternion GetLocalRotMT(in TransformHandle handle)
		{
			return ExtractTransformAccess(handle)->localRotation;
		}

		public unsafe static Vector3 GetLocalScaleMT(in TransformHandle handle)
		{
			return ExtractTransformAccess(handle)->localScale;
		}

		public unsafe static Vector3 GetPosMT(in TransformHandle handle)
		{
			return ExtractTransformAccess(handle)->position;
		}

		public unsafe static Quaternion GetRotMT(in TransformHandle handle)
		{
			return ExtractTransformAccess(handle)->rotation;
		}
	}

	private static PointerEventData _pointerEvent;

	public static Transform FindChildRecursive(this Transform transform, string name)
	{
		Transform transform2 = transform.Find(name);
		for (int i = 0; i < transform.childCount; i++)
		{
			if (!(transform2 == null))
			{
				break;
			}
			transform2 = transform.GetChild(i).FindChildRecursive(name);
		}
		return transform2;
	}

	public static T GetOrAddComponent<T>(this Transform transform) where T : Component
	{
		T val = transform.GetComponent<T>();
		if (val == null)
		{
			val = transform.gameObject.AddComponent<T>();
		}
		return val;
	}

	public static void DestroyAllChildren(this Transform transform, bool immediate = false)
	{
		for (int num = transform.childCount; num > 0; num--)
		{
			Transform child = transform.GetChild(num - 1);
			if (!child.CompareTag("persist"))
			{
				if (immediate)
				{
					UnityEngine.Object.DestroyImmediate(child.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
		}
	}

	public static float AngleToPos(this Transform transform, Vector3 targetPos)
	{
		float y = transform.eulerAngles.y;
		Vector3 vector = targetPos - transform.position;
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f - y;
		if (num > 180f)
		{
			num -= 360f;
		}
		else if (num < -180f)
		{
			num += 360f;
		}
		return num;
	}

	public static int GetDepth(this Transform transform)
	{
		int num = 0;
		Transform parent = transform.parent;
		while (parent != null)
		{
			num++;
			parent = parent.parent;
		}
		return num;
	}

	public static bool ClickedInsideTransformOrChild(this Transform t, int? mouseButton = null)
	{
		if (mouseButton.HasValue && mouseButton.Value switch
		{
			0 => Mouse.current.leftButton.isPressed ? 1 : 0, 
			1 => Mouse.current.rightButton.isPressed ? 1 : 0, 
			2 => Mouse.current.middleButton.isPressed ? 1 : 0, 
			_ => 0, 
		} == 0)
		{
			return false;
		}
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return false;
		}
		if (_pointerEvent == null)
		{
			_pointerEvent = new PointerEventData(current);
		}
		_pointerEvent.position = Mouse.current.position.ReadValue();
		List<RaycastResult> obj = Pool.Get<List<RaycastResult>>();
		EventSystem.current.RaycastAll(_pointerEvent, obj);
		foreach (RaycastResult item in obj)
		{
			if (item.gameObject.transform.IsChildOf(t))
			{
				Pool.FreeUnmanaged(ref obj);
				return true;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return false;
	}
}
