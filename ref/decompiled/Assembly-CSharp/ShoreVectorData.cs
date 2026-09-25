using UnityEngine;

public class ShoreVectorData : BaseScriptableObject
{
	[ReadOnly]
	public float WorldSize;

	[Header("Shore Vectors")]
	[ReadOnly]
	public float[] Distances;

	[ReadOnly]
	public Vector4[] Vectors;

	[Header("Slope Data")]
	[ReadOnly]
	public Vector2[] SlopeData;

	[Header("WaterHeight")]
	[ReadOnly]
	public float[] WaterHeightData;

	[Header("HeightData")]
	[ReadOnly]
	public short[] HeightData;

	[ReadOnly]
	public Vector2 HeightInfo;

	public int ShoreVectorDimension
	{
		get
		{
			float[] distances = Distances;
			return (int)Mathf.Sqrt((distances != null) ? distances.Length : 0);
		}
	}

	public int SlopeDataDimension
	{
		get
		{
			Vector2[] slopeData = SlopeData;
			return (int)Mathf.Sqrt((slopeData != null) ? slopeData.Length : 0);
		}
	}

	public int WaterHeightDimension
	{
		get
		{
			float[] waterHeightData = WaterHeightData;
			return (int)Mathf.Sqrt((waterHeightData != null) ? waterHeightData.Length : 0);
		}
	}

	public int HeightDimension
	{
		get
		{
			short[] heightData = HeightData;
			return (int)Mathf.Sqrt((heightData != null) ? heightData.Length : 0);
		}
	}
}
