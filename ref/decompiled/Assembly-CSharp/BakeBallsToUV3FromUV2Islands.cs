using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BakeBallsToUV3FromUV2Islands : MonoBehaviour
{
	public enum StoreMode
	{
		BottomY,
		CenterY,
		TopY
	}

	public MeshFilter meshFilter;

	[Tooltip("Which value to store in UV3.y per ball")]
	public StoreMode store;

	[Tooltip("Group key = floor(uv2.y + islandOffset). If your islands aren't exactly on integers, tweak this.")]
	public float islandOffset;

	public bool normalize01 = true;

	[ContextMenu("Bake UV3 from UV2 islands")]
	public void Bake()
	{
		if (!meshFilter)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		if (!meshFilter || !meshFilter.sharedMesh)
		{
			Debug.LogError("No MeshFilter/sharedMesh.");
			return;
		}
		Mesh sharedMesh = meshFilter.sharedMesh;
		Mesh mesh = Object.Instantiate(sharedMesh);
		mesh.name = sharedMesh.name + " (baked UV3)";
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv2;
		if (uv == null || uv.Length != vertices.Length)
		{
			Debug.LogError("Mesh needs UV2 for island grouping.");
			return;
		}
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		for (int i = 0; i < vertices.Length; i++)
		{
			int key = Mathf.FloorToInt(uv[i].y + islandOffset + 0.0001f);
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = (dictionary[key] = new List<int>());
			}
			value.Add(i);
		}
		Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
		Dictionary<int, float> dictionary3 = new Dictionary<int, float>();
		foreach (KeyValuePair<int, List<int>> item in dictionary)
		{
			List<int> value2 = item.Value;
			Vector3 zero = Vector3.zero;
			foreach (int item2 in value2)
			{
				zero += vertices[item2];
			}
			zero /= (float)value2.Count;
			float num = 0f;
			foreach (int item3 in value2)
			{
				float num2 = Vector3.Distance(vertices[item3], zero);
				if (num2 > num)
				{
					num = num2;
				}
			}
			dictionary2[item.Key] = zero.y;
			dictionary3[item.Key] = num;
		}
		Dictionary<int, float> dictionary4 = new Dictionary<int, float>();
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = 0f;
		foreach (int key2 in dictionary.Keys)
		{
			float num6 = dictionary2[key2];
			float num7 = dictionary3[key2];
			float num9 = (dictionary4[key2] = store switch
			{
				StoreMode.CenterY => num6, 
				StoreMode.TopY => num6 + num7, 
				_ => num6 - num7, 
			});
			if (num9 < num3)
			{
				num3 = num9;
			}
			if (num9 > num4)
			{
				num4 = num9;
			}
			if (num7 > num5)
			{
				num5 = num7;
			}
		}
		float num10 = Mathf.Max(1E-06f, num4 - num3);
		Vector2[] array = mesh.uv3;
		if (array == null || array.Length != vertices.Length)
		{
			array = new Vector2[vertices.Length];
		}
		foreach (KeyValuePair<int, List<int>> item4 in dictionary)
		{
			float num11 = dictionary4[item4.Key];
			if (normalize01)
			{
				num11 = Mathf.Clamp01((num11 - num3) / num10);
			}
			foreach (int item5 in item4.Value)
			{
				array[item5] = new Vector2(0f, num11);
			}
		}
		mesh.uv3 = array;
		meshFilter.sharedMesh = mesh;
		Debug.Log($"Baked {dictionary.Count} balls into UV3.y (mode={store}, normalized={normalize01}).");
	}
}
