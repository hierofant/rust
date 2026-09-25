using System.Collections.Generic;
using UnityEngine;

public class CH47DropZone : MonoBehaviour
{
	public TimeSince lastDropTime;

	public static List<CH47DropZone> dropZones = new List<CH47DropZone>();

	public void Awake()
	{
		if (!dropZones.Contains(this))
		{
			dropZones.Add(this);
		}
		lastDropTime = 100000000f;
	}

	public static CH47DropZone GetClosest(Vector3 pos)
	{
		float num = float.PositiveInfinity;
		CH47DropZone result = null;
		foreach (CH47DropZone dropZone in dropZones)
		{
			float num2 = Vector3Ex.Distance2D(pos, dropZone.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = dropZone;
			}
		}
		return result;
	}

	public void OnDestroy()
	{
		if (dropZones.Contains(this))
		{
			dropZones.Remove(this);
		}
	}

	public void Used()
	{
		lastDropTime = 0f;
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(base.transform.position, 5f);
	}
}
