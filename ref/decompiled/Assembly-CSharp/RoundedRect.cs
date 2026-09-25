using System;
using System.Collections.Generic;
using UnityEngine;

public static class RoundedRect
{
	public static void AppendPerimeter(List<Vector2> points, Rect rect, float topLeft, float topRight, float bottomRight, float bottomLeft, int segmentsPerCorner)
	{
		float max = Mathf.Min(rect.width, rect.height) * 0.5f;
		float num = Mathf.Clamp(topLeft, 0f, max);
		float num2 = Mathf.Clamp(topRight, 0f, max);
		float num3 = Mathf.Clamp(bottomRight, 0f, max);
		float num4 = Mathf.Clamp(bottomLeft, 0f, max);
		AppendArc(points, new Vector2(rect.xMax - num3, rect.yMin + num3), num3, 270f, segmentsPerCorner);
		AppendArc(points, new Vector2(rect.xMax - num2, rect.yMax - num2), num2, 0f, segmentsPerCorner);
		AppendArc(points, new Vector2(rect.xMin + num, rect.yMax - num), num, 90f, segmentsPerCorner);
		AppendArc(points, new Vector2(rect.xMin + num4, rect.yMin + num4), num4, 180f, segmentsPerCorner);
	}

	private static void AppendArc(List<Vector2> points, Vector2 center, float radius, float startAngleDeg, int segments)
	{
		for (int i = 0; i <= segments; i++)
		{
			float f = (startAngleDeg + 90f * (float)i / (float)segments) * (MathF.PI / 180f);
			points.Add(center + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * radius);
		}
	}
}
