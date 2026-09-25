using UnityEngine;

public class TickHistory
{
	private Deque<Vector3> points;

	private Deque<Vector3> parentPoints;

	public int Count => points.Count;

	public int ParentCount => parentPoints.Count;

	public Vector3 this[int index] => points[index];

	public TickHistory(int capacity)
	{
		points = new Deque<Vector3>(capacity);
		parentPoints = new Deque<Vector3>(capacity);
	}

	public Vector3 GetHistoryAtIndex(int index)
	{
		return points[index];
	}

	public Vector3 GetParentHistoryAtIndex(int index)
	{
		return parentPoints[index];
	}

	public void Reset()
	{
		points.Clear();
		parentPoints.Clear();
	}

	public void Reset(Vector3 point)
	{
		Reset();
		AddPoint(point);
	}

	public float Distance(BasePlayer player, Vector3 point)
	{
		if (points.Count == 0)
		{
			return player.Distance(point);
		}
		Vector3 position = player.transform.position;
		Quaternion rotation = player.transform.rotation;
		Bounds bounds = player.bounds;
		Matrix4x4 tickHistoryMatrix = player.tickHistoryMatrix;
		float num = float.MaxValue;
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 point2 = tickHistoryMatrix.MultiplyPoint3x4(points[i]);
			Vector3 point3 = ((i == points.Count - 1) ? position : tickHistoryMatrix.MultiplyPoint3x4(points[i + 1]));
			Vector3 position2 = new Line(point2, point3).ClosestPoint(point);
			num = Mathf.Min(num, new OBB(position2, rotation, bounds).Distance(point));
		}
		return num;
	}

	public float DistanceParented(BasePlayer player, Vector3 point)
	{
		int count = points.Count;
		int count2 = parentPoints.Count;
		if (count == 0 || count2 == 0)
		{
			return player.Distance(point);
		}
		int num = Mathf.Min(count, count2);
		int num2 = count - num;
		int num3 = count2 - num;
		Quaternion rotation = player.transform.rotation;
		Bounds bounds = player.bounds;
		Vector3 vector = ((player.transform.parent != null) ? player.transform.parent.position : player.transform.position);
		Matrix4x4 tickHistoryMatrix = player.tickHistoryMatrix;
		float num4 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			Vector3 point2 = points[num2 + i];
			Vector3 vector2 = parentPoints[num3 + i];
			Vector3 point3 = tickHistoryMatrix.MultiplyPoint3x4(point2) + (vector2 - vector);
			Vector3 point5;
			if (i < num - 1)
			{
				Vector3 point4 = points[num2 + i + 1];
				Vector3 vector3 = parentPoints[num3 + i + 1];
				point5 = tickHistoryMatrix.MultiplyPoint3x4(point4) + (vector3 - vector);
			}
			else
			{
				point5 = player.transform.position;
			}
			Vector3 position = new Line(point3, point5).ClosestPoint(point);
			num4 = Mathf.Min(num4, new OBB(position, rotation, bounds).Distance(point));
		}
		return num4;
	}

	public void AddPoint(Vector3 point, int limit = -1)
	{
		while (limit > 0 && points.Count >= limit)
		{
			points.PopFront();
		}
		points.PushBack(point);
	}

	public void AddParentPoint(Vector3 point, int limit = -1)
	{
		while (limit > 0 && parentPoints.Count >= limit)
		{
			parentPoints.PopFront();
		}
		parentPoints.PushBack(point);
	}

	public void TransformEntries(Matrix4x4 matrix)
	{
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 point = points[i];
			point = matrix.MultiplyPoint3x4(point);
			points[i] = point;
		}
	}
}
