using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
[AddComponentMenu("UI/Effects/Rounded Corners")]
[DisallowMultipleComponent]
public class UIRoundedCorners : BaseMeshEffect
{
	[SerializeField]
	private float topLeft = 16f;

	[SerializeField]
	private float topRight = 16f;

	[SerializeField]
	private float bottomRight = 16f;

	[SerializeField]
	private float bottomLeft = 16f;

	[SerializeField]
	[Range(1f, 32f)]
	private int segmentsPerCorner = 8;

	private static readonly List<UIVertex> stream = new List<UIVertex>();

	private static readonly List<Vector2> points = new List<Vector2>();

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}
		vh.GetUIVertexStream(stream);
		if (stream.Count == 0)
		{
			return;
		}
		Color32 color = stream[0].color;
		Vector2 vector = stream[0].uv0;
		Vector2 vector2 = stream[0].uv0;
		foreach (UIVertex item in stream)
		{
			vector = Vector2.Min(vector, (Vector2)item.uv0);
			vector2 = Vector2.Max(vector2, (Vector2)item.uv0);
		}
		Rect rect = base.graphic.rectTransform.rect;
		points.Clear();
		RoundedRect.AppendPerimeter(points, rect, topLeft, topRight, bottomRight, bottomLeft, segmentsPerCorner);
		vh.Clear();
		vh.AddVert(MakeVert(rect.center, color, rect, vector, vector2));
		foreach (Vector2 point in points)
		{
			vh.AddVert(MakeVert(point, color, rect, vector, vector2));
		}
		for (int i = 0; i < points.Count; i++)
		{
			int num = (i + 1) % points.Count;
			vh.AddTriangle(0, i + 1, num + 1);
		}
	}

	private static UIVertex MakeVert(Vector2 pos, Color32 color, Rect rect, Vector2 uvMin, Vector2 uvMax)
	{
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.position = pos;
		simpleVert.color = color;
		float t = Mathf.InverseLerp(rect.xMin, rect.xMax, pos.x);
		float t2 = Mathf.InverseLerp(rect.yMin, rect.yMax, pos.y);
		simpleVert.uv0 = new Vector2(Mathf.Lerp(uvMin.x, uvMax.x, t), Mathf.Lerp(uvMin.y, uvMax.y, t2));
		return simpleVert;
	}
}
