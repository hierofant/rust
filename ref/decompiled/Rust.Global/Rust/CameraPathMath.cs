using System;

namespace Rust;

public static class CameraPathMath
{
	public const int MaxNodes = 256;

	public static float EvaluateLinear(ReadOnlySpan<float> ticks, ReadOnlySpan<float> values, int n, float t)
	{
		if (n <= 0)
		{
			return 0f;
		}
		if (n == 1 || t <= ticks[0])
		{
			return values[0];
		}
		if (t >= ticks[n - 1])
		{
			return values[n - 1];
		}
		for (int i = 0; i < n - 1; i++)
		{
			float num = ticks[i + 1];
			if (!(t > num))
			{
				float num2 = ticks[i];
				float num3 = num - num2;
				float num4 = ((num3 <= 1E-06f) ? 1f : ((t - num2) / num3));
				return values[i] + (values[i + 1] - values[i]) * num4;
			}
		}
		return values[n - 1];
	}

	public static float EvaluateCubicSpline(ReadOnlySpan<float> ticks, ReadOnlySpan<float> values, int n, float t)
	{
		if (n <= 0)
		{
			return 0f;
		}
		if (n == 1)
		{
			return values[0];
		}
		if (n > 256)
		{
			n = 256;
		}
		if (n == 2)
		{
			return EvaluateLinear(ticks, values, n, t);
		}
		if (t <= ticks[0])
		{
			return values[0];
		}
		if (t >= ticks[n - 1])
		{
			return values[n - 1];
		}
		Span<float> span = stackalloc float[256];
		Span<float> span2 = stackalloc float[256];
		span[0] = -0.5f;
		span2[0] = 3f / (ticks[1] - ticks[0]) * ((values[1] - values[0]) / (ticks[1] - ticks[0]));
		for (int i = 1; i <= n - 2; i++)
		{
			float num = ticks[i - 1];
			float num2 = values[i - 1];
			float num3 = ticks[i];
			float num4 = values[i];
			float num5 = ticks[i + 1];
			float num6 = values[i + 1];
			float num7 = (num3 - num) / (num5 - num);
			float num8 = num7 * span[i - 1] + 2f;
			span[i] = (num7 - 1f) / num8;
			float num9 = (num6 - num4) / (num5 - num3) - (num4 - num2) / (num3 - num);
			span2[i] = (6f * num9 / (num5 - num) - num7 * span2[i - 1]) / num8;
		}
		float num10 = 0.5f;
		float num11 = 3f / (ticks[n - 1] - ticks[n - 2]) * (0f - (values[n - 1] - values[n - 2]) / (ticks[n - 1] - ticks[n - 2]));
		span[n - 1] = (num11 - num10 * span2[n - 2]) / (num10 * span[n - 2] + 1f);
		for (int num12 = n - 2; num12 >= 0; num12--)
		{
			span[num12] = span[num12] * span[num12 + 1] + span2[num12];
		}
		int num13 = 0;
		int num14 = n - 1;
		while (num14 - num13 > 1)
		{
			int num15 = num14 + num13 >> 1;
			if (ticks[num15] > t)
			{
				num14 = num15;
			}
			else
			{
				num13 = num15;
			}
		}
		float num16 = ticks[num14] - ticks[num13];
		if (num16 <= 1E-06f)
		{
			return values[num13];
		}
		float num17 = (ticks[num14] - t) / num16;
		float num18 = (t - ticks[num13]) / num16;
		return num17 * values[num13] + num18 * values[num14] + ((num17 * num17 * num17 - num17) * span[num13] + (num18 * num18 * num18 - num18) * span[num14]) * (num16 * num16) / 6f;
	}

	public static float Evaluate(CameraPathAlgorithm algorithm, ReadOnlySpan<float> ticks, ReadOnlySpan<float> values, int n, float t)
	{
		if (algorithm != CameraPathAlgorithm.CubicSpline)
		{
			return EvaluateLinear(ticks, values, n, t);
		}
		return EvaluateCubicSpline(ticks, values, n, t);
	}
}
