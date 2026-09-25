using UnityEngine;

public class FlyoverCurve
{
	private const int Samples = 64;

	public Vector3 P0;

	public Vector3 P1;

	public Vector3 P2;

	public float TotalLength;

	public float Duration;

	public float Elapsed;

	public bool Active;

	private float[] arcLengths;

	public void Build(Vector3 p0, Vector3 p1, Vector3 p2, float duration)
	{
		P0 = p0;
		P1 = p1;
		P2 = p2;
		Duration = Mathf.Max(1f, duration);
		Elapsed = 0f;
		Active = true;
		arcLengths = new float[65];
		Vector3 a = p0;
		for (int i = 1; i <= 64; i++)
		{
			Vector3 vector = Eval(p0, p1, p2, (float)i / 64f);
			arcLengths[i] = arcLengths[i - 1] + Vector3.Distance(a, vector);
			a = vector;
		}
		TotalLength = arcLengths[64];
	}

	public static Vector3 Eval(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		float num = 1f - t;
		return num * num * p0 + 2f * num * t * p1 + t * t * p2;
	}

	public float ElapsedArcDistance()
	{
		return TotalLength * Mathf.Clamp01(Elapsed / Duration);
	}

	public Vector3 EvalAtDistance(float s)
	{
		return Eval(P0, P1, P2, TAtDistance(s));
	}

	public float TAtDistance(float s)
	{
		if (s <= 0f)
		{
			return 0f;
		}
		if (s >= TotalLength)
		{
			return 1f;
		}
		for (int i = 1; i <= 64; i++)
		{
			if (!(s > arcLengths[i]))
			{
				float num = arcLengths[i] - arcLengths[i - 1];
				float num2 = ((num > 0.0001f) ? ((s - arcLengths[i - 1]) / num) : 0f);
				return ((float)(i - 1) + num2) / 64f;
			}
		}
		return 1f;
	}

	public static float ApproximateLength(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float num = 0f;
		Vector3 a = p0;
		for (int i = 1; i <= 64; i++)
		{
			Vector3 vector = Eval(p0, p1, p2, (float)i / 64f);
			num += Vector3.Distance(a, vector);
			a = vector;
		}
		return num;
	}
}
