using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail;

[Serializable]
public class FTail_SkinningVertexData
{
	public Vector3 position;

	public int[] bonesIndexes;

	public int allMeshBonesCount;

	public float[] weights;

	public float[] debugDists;

	public float[] debugDistWeights;

	public float[] debugWeights;

	public FTail_SkinningVertexData(Vector3 pos)
	{
		position = pos;
	}

	public float DistanceToLine(Vector3 pos, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 rhs = pos - lineStart;
		Vector3 normalized = (lineEnd - lineStart).normalized;
		float num = Vector3.Distance(lineStart, lineEnd);
		float num2 = Vector3.Dot(normalized, rhs);
		if (num2 <= 0f)
		{
			return Vector3.Distance(pos, lineStart);
		}
		if (num2 >= num)
		{
			return Vector3.Distance(pos, lineEnd);
		}
		Vector3 vector = normalized * num2;
		Vector3 b = lineStart + vector;
		return Vector3.Distance(pos, b);
	}

	public void CalculateVertexParameters(Vector3[] bonesPos, Quaternion[] bonesRot, Vector3[] boneAreas, int maxWeightedBones, float spread, Vector3 spreadOffset, float spreadPower = 1f)
	{
		allMeshBonesCount = bonesPos.Length;
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < bonesPos.Length; i++)
		{
			Vector3 lineEnd = ((i == bonesPos.Length - 1) ? Vector3.Lerp(bonesPos[i], bonesPos[i] + (bonesPos[i] - bonesPos[i - 1]), 0.9f) : Vector3.Lerp(bonesPos[i], bonesPos[i + 1], 0.9f));
			lineEnd += bonesRot[i] * spreadOffset;
			float y = DistanceToLine(position, bonesPos[i], lineEnd);
			list.Add(new Vector2(i, y));
		}
		list.Sort((Vector2 a, Vector2 b) => a.y.CompareTo(b.y));
		int num = Mathf.Min(maxWeightedBones, bonesPos.Length);
		bonesIndexes = new int[num];
		float[] array = new float[num];
		for (int j = 0; j < num; j++)
		{
			bonesIndexes[j] = (int)list[j].x;
			array[j] = list[j].y;
		}
		float[] array2 = new float[num];
		AutoSetBoneWeights(array2, array, spread, spreadPower, boneAreas);
		float num2 = 1f;
		weights = new float[num];
		for (int k = 0; k < num && (spread != 0f || k <= 0); k++)
		{
			if (num2 <= 0f)
			{
				weights[k] = 0f;
				continue;
			}
			float num3 = array2[k];
			num2 -= num3;
			if (num2 <= 0f)
			{
				num3 += num2;
			}
			else if (k == num - 1)
			{
				num3 += num2;
			}
			weights[k] = num3;
		}
	}

	public void AutoSetBoneWeights(float[] weightForBone, float[] distToBone, float spread, float spreadPower, Vector3[] boneAreas)
	{
		int num = weightForBone.Length;
		float[] array = new float[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = boneAreas[i].magnitude;
		}
		float[] array2 = new float[num];
		for (int j = 0; j < weightForBone.Length; j++)
		{
			weightForBone[j] = 0f;
		}
		float num2 = 0f;
		for (int k = 0; k < num; k++)
		{
			num2 += distToBone[k];
		}
		for (int l = 0; l < num; l++)
		{
			array2[l] = 1f - distToBone[l] / num2;
		}
		debugDists = distToBone;
		if (num == 1 || spread == 0f)
		{
			weightForBone[0] = 1f;
			return;
		}
		if (num == 2)
		{
			float num3 = 1f;
			weightForBone[0] = 1f;
			float num4 = Mathf.InverseLerp(distToBone[0] + array[0] / 1.25f * spread, distToBone[0], distToBone[1]);
			debugDists[0] = num4;
			num3 += (weightForBone[1] = DistributionIn(Mathf.Lerp(0f, 1f, num4), Mathf.Lerp(1.5f, 16f, spreadPower)));
			debugDistWeights = new float[weightForBone.Length];
			weightForBone.CopyTo(debugDistWeights, 0);
			for (int m = 0; m < num; m++)
			{
				weightForBone[m] /= num3;
			}
			debugWeights = weightForBone;
			return;
		}
		float num5 = array[0] / 10f;
		float num6 = array[0] / 2f;
		float num7 = 0f;
		for (int n = 0; n < num; n++)
		{
			float t = Mathf.InverseLerp(0f, num5 + num6 * spread, distToBone[n]);
			float num8 = Mathf.Lerp(1f, 0f, t);
			if (n == 0 && num8 == 0f)
			{
				num8 = 1f;
			}
			weightForBone[n] = num8;
			num7 += num8;
		}
		debugDistWeights = new float[weightForBone.Length];
		weightForBone.CopyTo(debugDistWeights, 0);
		for (int num9 = 0; num9 < num; num9++)
		{
			weightForBone[num9] /= num7;
		}
		debugWeights = weightForBone;
	}

	public static float DistributionIn(float k, float power)
	{
		return Mathf.Pow(k, power + 1f);
	}

	public static Color GetBoneIndicatorColor(int boneIndex, int bonesCount, float s = 0.9f, float v = 0.9f)
	{
		return Color.HSVToRGB(((float)boneIndex * 1.125f / (float)bonesCount + 0.125f * (float)boneIndex + 0.3f) % 1f, s, v);
	}

	public Color GetWeightColor()
	{
		Color color = GetBoneIndicatorColor(bonesIndexes[0], allMeshBonesCount, 1f, 1f);
		for (int i = 1; i < bonesIndexes.Length; i++)
		{
			color = Color.Lerp(color, GetBoneIndicatorColor(bonesIndexes[i], allMeshBonesCount, 1f, 1f), weights[i]);
		}
		return color;
	}
}
