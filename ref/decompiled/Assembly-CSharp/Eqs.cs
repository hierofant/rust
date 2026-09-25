using System;
using System.Collections.Generic;
using Rust.Ai.Gen2;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

public static class Eqs
{
	public sealed class PooledScoreList : BasePooledList<(NavVector3 pos, float score), PooledScoreList>
	{
		public void SortByScoreDesc(BaseEntity baseEntity = null, string debugCategory = "navigation")
		{
			using (TimeWarning.New("SortByScoreDesc"))
			{
				Sort(((NavVector3 pos, float score) a, (NavVector3 pos, float score) b) => b.score.CompareTo(a.score));
			}
		}

		public void Reorder(List<NavVector3> positions)
		{
			using (TimeWarning.New("Reorder"))
			{
				for (int i = 0; i < positions.Count; i++)
				{
					positions[i] = base[i].pos;
				}
			}
		}
	}

	public static bool SampleNavigablePositions(RustNavMeshAgent agent, NavVector3 center, List<NavVector3> sampledPositions, float outerRadius, float innerRadius, int numPoints, bool preValidate = true)
	{
		using (TimeWarning.New("SampleNavigablePositions"))
		{
			if (preValidate && agent.SampleConnectedPositions(center, outerRadius, innerRadius, numPoints, sampledPositions))
			{
				return true;
			}
			sampledPositions.Clear();
			if (innerRadius <= 0f || Mathf.Approximately(innerRadius, outerRadius))
			{
				SamplePositionsInDonutShape(center, sampledPositions, outerRadius, numPoints);
			}
			else
			{
				SamplePositionsInMultiDonutShape(center, sampledPositions, outerRadius, innerRadius, 2, Mathf.Max(1, numPoints / 2));
			}
			return false;
		}
	}

	public static void SamplePositionsInDonutShape(NavVector3 center, List<NavVector3> sampledPositions, float radius = 10f, int itemsPerRing = 8)
	{
		using (TimeWarning.New("SamplePositionsInDonutShape"))
		{
			float num = UnityEngine.Random.Range(0f, MathF.PI * 2f);
			for (int i = 0; i < itemsPerRing; i++)
			{
				float f = MathF.PI * 2f * (float)i / (float)itemsPerRing + num;
				NavVector3 item = new NavVector3(center.Value + new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * radius);
				sampledPositions.Add(item);
			}
			sampledPositions.Shuffle((uint)Environment.TickCount);
		}
	}

	public static void SamplePositionsInMultiDonutShape(NavVector3 center, List<NavVector3> sampledPositions, float outerRadius = 10f, float innerRadius = 10f, int numRings = 1, int itemsPerRing = 8)
	{
		using (TimeWarning.New("SamplePositionsInMultiDonutShape"))
		{
			float num = UnityEngine.Random.Range(0f, MathF.PI * 2f);
			for (int i = 0; i < numRings; i++)
			{
				float num2 = ((numRings != 1) ? Mathf.Lerp(innerRadius, outerRadius, (float)i / (float)(numRings - 1)) : outerRadius);
				for (int j = 0; j < itemsPerRing; j++)
				{
					float num3 = num + (float)i * MathF.PI / (float)numRings;
					float f = MathF.PI * 2f * (float)j / (float)itemsPerRing + num3;
					NavVector3 item = new NavVector3(center.Value + new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * num2);
					sampledPositions.Add(item);
				}
			}
			sampledPositions.Shuffle((uint)Environment.TickCount);
		}
	}
}
