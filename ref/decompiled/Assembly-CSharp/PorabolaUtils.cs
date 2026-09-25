using UnityEngine;

public static class PorabolaUtils
{
	public static Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t, bool useLevelDirection = false)
	{
		float num = t * 2f - 1f;
		if (Mathf.Abs(start.y - end.y) < 0.1f)
		{
			Vector3 vector = end - start;
			Vector3 result = start + t * vector;
			result.y += ((0f - num) * num + 1f) * height;
			return result;
		}
		Vector3 vector2 = end - start;
		Vector3 rhs = end - new Vector3(start.x, end.y, start.z);
		Vector3 lhs = Vector3.Cross(vector2, rhs);
		Vector3 vector3 = ((!useLevelDirection) ? Vector3.Cross(lhs, vector2) : Vector3.Cross(lhs, rhs));
		if (end.y > start.y)
		{
			vector3 = -vector3;
		}
		return start + t * vector2 + ((0f - num) * num + 1f) * height * vector3.normalized;
	}

	public static float FindT(Vector3 start, Vector3 end, float height, Vector3 targetPosition, bool useLevelDirection = false)
	{
		float num = 0.5f;
		float num2 = 0.01f;
		for (num = 0f; num <= 1f; num += num2)
		{
			if (Vector3.Distance(SampleParabola(start, end, height, num, useLevelDirection), targetPosition) < 0.01f)
			{
				return num;
			}
		}
		return -1f;
	}

	public static Vector3 RotateAroundWorldAxis(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
	{
		Quaternion quaternion = Quaternion.AngleAxis(angle, axis);
		Vector3 vector = point - pivot;
		Vector3 vector2 = quaternion * vector;
		return pivot + vector2;
	}
}
