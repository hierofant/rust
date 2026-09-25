using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class BorderGraphic : MaskableGraphic
{
	private float top;

	private float right;

	private float bottom;

	private float left;

	private float topLeft;

	private float topRight;

	private float bottomRight;

	private float bottomLeft;

	private int segmentsPerCorner = 8;

	private static readonly List<Vector2> outer = new List<Vector2>();

	private static readonly List<Vector2> inner = new List<Vector2>();

	public void SetSides(float top, float right, float bottom, float left, Color color)
	{
		this.top = top;
		this.right = right;
		this.bottom = bottom;
		this.left = left;
		this.color = color;
		SetVerticesDirty();
	}

	public void SetCorners(float topLeft, float topRight, float bottomRight, float bottomLeft, int segmentsPerCorner)
	{
		this.topLeft = topLeft;
		this.topRight = topRight;
		this.bottomRight = bottomRight;
		this.bottomLeft = bottomLeft;
		this.segmentsPerCorner = segmentsPerCorner;
		SetVerticesDirty();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Rect rect = base.rectTransform.rect;
		if (topLeft <= 0f && topRight <= 0f && bottomRight <= 0f && bottomLeft <= 0f)
		{
			AddSquareBorder(vh, rect);
			return;
		}
		outer.Clear();
		inner.Clear();
		RoundedRect.AppendPerimeter(outer, rect, topLeft, topRight, bottomRight, bottomLeft, segmentsPerCorner);
		Rect rect2 = Rect.MinMaxRect(rect.xMin + left, rect.yMin + bottom, rect.xMax - right, rect.yMax - top);
		float num = Mathf.Max(0f, topLeft - Mathf.Max(top, left));
		float num2 = Mathf.Max(0f, topRight - Mathf.Max(top, right));
		float num3 = Mathf.Max(0f, bottomRight - Mathf.Max(bottom, right));
		float num4 = Mathf.Max(0f, bottomLeft - Mathf.Max(bottom, left));
		RoundedRect.AppendPerimeter(inner, rect2, num, num2, num3, num4, segmentsPerCorner);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		for (int i = 0; i < outer.Count; i++)
		{
			simpleVert.position = outer[i];
			vh.AddVert(simpleVert);
			simpleVert.position = inner[i];
			vh.AddVert(simpleVert);
		}
		int count = outer.Count;
		for (int j = 0; j < count; j++)
		{
			int num5 = (j + 1) % count;
			int num6 = j * 2;
			int idx = j * 2 + 1;
			int idx2 = num5 * 2;
			int num7 = num5 * 2 + 1;
			vh.AddTriangle(num6, idx2, num7);
			vh.AddTriangle(num7, idx, num6);
		}
	}

	private void AddSquareBorder(VertexHelper vh, Rect rect)
	{
		float xMin = rect.xMin;
		float xMax = rect.xMax;
		float yMin = rect.yMin;
		float yMax = rect.yMax;
		Color color = this.color;
		if (top > 0f)
		{
			AddQuad(vh, xMin, yMax - top, xMax, yMax, color);
		}
		if (bottom > 0f)
		{
			AddQuad(vh, xMin, yMin, xMax, yMin + bottom, color);
		}
		if (left > 0f)
		{
			AddQuad(vh, xMin, yMin + bottom, xMin + left, yMax - top, color);
		}
		if (right > 0f)
		{
			AddQuad(vh, xMax - right, yMin + bottom, xMax, yMax - top, color);
		}
	}

	private static void AddQuad(VertexHelper vh, float xMin, float yMin, float xMax, float yMax, Color color)
	{
		int currentVertCount = vh.currentVertCount;
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = color;
		simpleVert.position = new Vector3(xMin, yMin);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(xMin, yMax);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(xMax, yMax);
		vh.AddVert(simpleVert);
		simpleVert.position = new Vector3(xMax, yMin);
		vh.AddVert(simpleVert);
		vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}
}
