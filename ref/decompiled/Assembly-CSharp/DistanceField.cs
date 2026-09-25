using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public class DistanceField
{
	[BurstCompile(FloatMode = FloatMode.Fast)]
	private struct GenerateJob : IJob
	{
		public int size;

		public byte threshold;

		public NativeArray<byte>.ReadOnly image;

		public NativeArray<float> distanceField;

		public void Execute()
		{
			int num = size + 2;
			NativeArray<int> nativeArray = new NativeArray<int>(num * num, Allocator.Temp);
			NativeArray<int> nativeArray2 = new NativeArray<int>(num * num, Allocator.Temp);
			NativeArray<float> nativeArray3 = new NativeArray<float>(num * num, Allocator.Temp);
			int i = 0;
			int num2 = 0;
			for (; i < num; i++)
			{
				int num3 = 0;
				while (num3 < num)
				{
					nativeArray[num2] = -1;
					nativeArray2[num2] = -1;
					nativeArray3[num2] = float.PositiveInfinity;
					num3++;
					num2++;
				}
			}
			int num4 = 1;
			int num5 = num4 * size;
			int num6 = num4 * num;
			while (num4 < size - 2)
			{
				int num7 = 1;
				int num8 = num5 + num7;
				int num9 = num6 + num7;
				while (num7 < size - 2)
				{
					int index = num9 + num + 1;
					bool flag = image[num8] > threshold;
					if (flag && (image[num8 - 1] > threshold != flag || image[num8 + 1] > threshold != flag || image[num8 - size] > threshold != flag || image[num8 + size] > threshold != flag))
					{
						nativeArray[index] = num7 + 1;
						nativeArray2[index] = num4 + 1;
						nativeArray3[index] = 0f;
					}
					num7++;
					num8++;
					num9++;
				}
				num4++;
				num5 += size;
				num6 += num;
			}
			int num10 = 1;
			int num11 = num10 * num;
			while (num10 < num - 1)
			{
				int num12 = 1;
				int num13 = num11 + num12;
				while (num12 < num - 1)
				{
					int index2 = num13 - 1;
					int num14 = num13 - num;
					int index3 = num14 - 1;
					int index4 = num14 + 1;
					float num15 = nativeArray3[num13];
					if (nativeArray3[index3] + 1.4142135f < num15)
					{
						int num17 = (nativeArray[num13] = nativeArray[index3]);
						int num18 = num17;
						num17 = (nativeArray2[num13] = nativeArray2[index3]);
						int num20 = num17;
						float num22 = (nativeArray3[num13] = Vector2Ex.Length(num12 - num18, num10 - num20));
						num15 = num22;
					}
					if (nativeArray3[num14] + 1f < num15)
					{
						int num17 = (nativeArray[num13] = nativeArray[num14]);
						int num24 = num17;
						num17 = (nativeArray2[num13] = nativeArray2[num14]);
						int num26 = num17;
						float num22 = (nativeArray3[num13] = Vector2Ex.Length(num12 - num24, num10 - num26));
						num15 = num22;
					}
					if (nativeArray3[index4] + 1.4142135f < num15)
					{
						int num17 = (nativeArray[num13] = nativeArray[index4]);
						int num29 = num17;
						num17 = (nativeArray2[num13] = nativeArray2[index4]);
						int num31 = num17;
						float num22 = (nativeArray3[num13] = Vector2Ex.Length(num12 - num29, num10 - num31));
						num15 = num22;
					}
					if (nativeArray3[index2] + 1f < num15)
					{
						int num17 = (nativeArray[num13] = nativeArray[index2]);
						int num34 = num17;
						num17 = (nativeArray2[num13] = nativeArray2[index2]);
						int num36 = num17;
						float num22 = (nativeArray3[num13] = Vector2Ex.Length(num12 - num34, num10 - num36));
						num15 = num22;
					}
					num12++;
					num13++;
				}
				num10++;
				num11 += num;
			}
			int num38 = num - 2;
			int num39 = num38 * num;
			while (num38 >= 1)
			{
				int num40 = num - 2;
				int num41 = num39 + num40;
				while (num40 >= 1)
				{
					int index5 = num41 + 1;
					int num42 = num41 + num;
					int index6 = num42 - 1;
					int index7 = num42 + 1;
					float num43 = nativeArray3[num41];
					if (nativeArray3[index5] + 1f < num43)
					{
						int num17 = (nativeArray[num41] = nativeArray[index5]);
						int num45 = num17;
						num17 = (nativeArray2[num41] = nativeArray2[index5]);
						int num47 = num17;
						float num22 = (nativeArray3[num41] = Vector2Ex.Length(num40 - num45, num38 - num47));
						num43 = num22;
					}
					if (nativeArray3[index6] + 1.4142135f < num43)
					{
						int num17 = (nativeArray[num41] = nativeArray[index6]);
						int num50 = num17;
						num17 = (nativeArray2[num41] = nativeArray2[index6]);
						int num52 = num17;
						float num22 = (nativeArray3[num41] = Vector2Ex.Length(num40 - num50, num38 - num52));
						num43 = num22;
					}
					if (nativeArray3[num42] + 1f < num43)
					{
						int num17 = (nativeArray[num41] = nativeArray[num42]);
						int num55 = num17;
						num17 = (nativeArray2[num41] = nativeArray2[num42]);
						int num57 = num17;
						float num22 = (nativeArray3[num41] = Vector2Ex.Length(num40 - num55, num38 - num57));
						num43 = num22;
					}
					if (nativeArray3[index7] + 1f < num43)
					{
						int num17 = (nativeArray[num41] = nativeArray[index7]);
						int num60 = num17;
						num17 = (nativeArray2[num41] = nativeArray2[index7]);
						int num62 = num17;
						float num22 = (nativeArray3[num41] = Vector2Ex.Length(num40 - num60, num38 - num62));
						num43 = num22;
					}
					num40--;
					num41--;
				}
				num38--;
				num39 -= num;
			}
			int num64 = 0;
			int num65 = 0;
			int num66 = num;
			while (num64 < size)
			{
				int num67 = 0;
				int num68 = num66 + 1;
				while (num67 < size)
				{
					distanceField[num65] = ((image[num65] > threshold) ? (0f - nativeArray3[num68]) : nativeArray3[num68]);
					num67++;
					num65++;
					num68++;
				}
				num64++;
				num66 += num;
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	private struct SobelGradientsJob : IJobParallelFor
	{
		public int size;

		public NativeArray<float>.ReadOnly distanceField;

		public NativeArray<Vector4> vectorField;

		public void Execute(int index)
		{
			int num = index % size;
			int num2 = index / size;
			float z = SampleClamped(distanceField, size, num, num2);
			float num3 = SampleClamped(distanceField, size, num - 1, num2 - 1);
			float num4 = SampleClamped(distanceField, size, num - 1, num2);
			float num5 = SampleClamped(distanceField, size, num - 1, num2 + 1);
			float num6 = SampleClamped(distanceField, size, num, num2 - 1);
			float num7 = SampleClamped(distanceField, size, num, num2 + 1);
			float num8 = SampleClamped(distanceField, size, num + 1, num2 - 1);
			float num9 = SampleClamped(distanceField, size, num + 1, num2);
			float num10 = SampleClamped(distanceField, size, num + 1, num2 + 1);
			float num11 = num8 + 2f * num9 + num10 - (num3 + 2f * num4 + num5);
			float num12 = num5 + 2f * num7 + num10 - (num3 + 2f * num6 + num8);
			Vector2 normalized = new Vector2(0f - num11, 0f - num12).normalized;
			vectorField[index] = new Vector4(normalized.x, normalized.y, z, 0f);
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	private struct FixBoundaryGradientsJob : IJob
	{
		public int size;

		public NativeArray<Vector4> vectorField;

		public void Execute()
		{
			for (int i = 1; i < size - 1; i++)
			{
				vectorField[0 + i] = SampleClamped(vectorField, size, i, 1);
				vectorField[(size - 1) * size + i] = SampleClamped(vectorField, size, i, size - 2);
			}
			for (int j = 0; j < size; j++)
			{
				vectorField[j * size] = SampleClamped(vectorField, size, 1, j);
				vectorField[j * size + size - 1] = SampleClamped(vectorField, size, size - 2, j);
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	private struct BlurHorizontalJob : IJobParallelFor
	{
		public int size;

		public NativeArray<float>.ReadOnly src;

		[WriteOnly]
		public NativeArray<float> dst;

		public void Execute(int index)
		{
			int num = index % size;
			int num2 = index / size;
			int num3 = size - 1;
			int num4 = num2 * size;
			float num5 = 0f;
			for (int i = 0; i < 7; i++)
			{
				int num6 = num + GaussOffsets[i];
				num6 = ((num6 >= 0) ? ((num6 > num3) ? num3 : num6) : 0);
				num5 += src[num4 + num6] * GaussWeights[i];
			}
			dst[index] = num5;
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	private struct BlurVerticalJob : IJobParallelFor
	{
		public int size;

		public NativeArray<float>.ReadOnly src;

		[WriteOnly]
		public NativeArray<float> dst;

		public void Execute(int index)
		{
			int num = index % size;
			int num2 = index / size;
			int num3 = size - 1;
			float num4 = 0f;
			for (int i = 0; i < 7; i++)
			{
				int num5 = num2 + GaussOffsets[i];
				num5 = ((num5 >= 0) ? ((num5 > num3) ? num3 : num5) : 0);
				num4 += src[num5 * size + num] * GaussWeights[i];
			}
			dst[index] = num4;
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	private struct GaussianBlurJob : IJob
	{
		public int size;

		public NativeArray<float> distanceField;

		public int steps;

		public void Execute()
		{
			NativeArray<float> nativeArray = new NativeArray<float>(size * size, Allocator.Temp);
			int num = size - 1;
			for (int i = 0; i < steps; i++)
			{
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				while (num2 < size)
				{
					int num5 = 0;
					while (num5 < size)
					{
						float num6 = 0f;
						for (int j = 0; j < 7; j++)
						{
							int num7 = num5 + GaussOffsets[j];
							num7 = ((num7 >= 0) ? num7 : 0);
							num7 = ((num7 <= num) ? num7 : num);
							num6 += distanceField[num4 + num7] * GaussWeights[j];
						}
						nativeArray[num3] = num6;
						num5++;
						num3++;
					}
					num2++;
					num4 += size;
				}
				int k = 0;
				int num8 = 0;
				for (; k < size; k++)
				{
					int num9 = 0;
					while (num9 < size)
					{
						float num10 = 0f;
						for (int l = 0; l < 7; l++)
						{
							int num11 = k + GaussOffsets[l];
							num11 = ((num11 >= 0) ? num11 : 0);
							num11 = ((num11 <= num) ? num11 : num);
							num10 += nativeArray[num11 * size + num9] * GaussWeights[l];
						}
						distanceField[num8] = num10;
						num9++;
						num8++;
					}
				}
			}
		}
	}

	private static readonly int[] GaussOffsets = new int[7] { -6, -4, -2, 0, 2, 4, 6 };

	private static readonly float[] GaussWeights = new float[7]
	{
		1f / 32f,
		7f / 64f,
		7f / 32f,
		9f / 32f,
		7f / 32f,
		7f / 64f,
		1f / 32f
	};

	public static JobHandle GenerateNative(in int size, in byte threshold, in NativeArray<byte>.ReadOnly image, in NativeArray<float> distanceFieldOut, JobHandle inputDeps)
	{
		GenerateJob generateJob = default(GenerateJob);
		generateJob.size = size;
		generateJob.threshold = threshold;
		generateJob.image = image;
		generateJob.distanceField = distanceFieldOut;
		GenerateJob jobData = generateJob;
		return IJobExtensions.ScheduleByRef(ref jobData, inputDeps);
	}

	public static void Generate(in int size, in byte threshold, in byte[] image, ref float[] distanceField)
	{
		int num = size + 2;
		int[] array = new int[num * num];
		int[] array2 = new int[num * num];
		float[] array3 = new float[num * num];
		int i = 0;
		int num2 = 0;
		for (; i < num; i++)
		{
			int num3 = 0;
			while (num3 < num)
			{
				array[num2] = -1;
				array2[num2] = -1;
				array3[num2] = float.PositiveInfinity;
				num3++;
				num2++;
			}
		}
		int num4 = 1;
		int num5 = num4 * size;
		int num6 = num4 * num;
		while (num4 < size - 2)
		{
			int num7 = 1;
			int num8 = num5 + num7;
			int num9 = num6 + num7;
			while (num7 < size - 2)
			{
				int num10 = num9 + num + 1;
				bool flag = image[num8] > threshold;
				if (flag && (image[num8 - 1] > threshold != flag || image[num8 + 1] > threshold != flag || image[num8 - size] > threshold != flag || image[num8 + size] > threshold != flag))
				{
					array[num10] = num7 + 1;
					array2[num10] = num4 + 1;
					array3[num10] = 0f;
				}
				num7++;
				num8++;
				num9++;
			}
			num4++;
			num5 += size;
			num6 += num;
		}
		int num11 = 1;
		int num12 = num11 * num;
		while (num11 < num - 1)
		{
			int num13 = 1;
			int num14 = num12 + num13;
			while (num13 < num - 1)
			{
				int num15 = num14 - 1;
				int num16 = num14 - num;
				int num17 = num16 - 1;
				int num18 = num16 + 1;
				float num19 = array3[num14];
				if (array3[num17] + 1.4142135f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length(num13 - (array[num14] = array[num17]), num11 - (array2[num14] = array2[num17])));
				}
				if (array3[num16] + 1f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length(num13 - (array[num14] = array[num16]), num11 - (array2[num14] = array2[num16])));
				}
				if (array3[num18] + 1.4142135f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length(num13 - (array[num14] = array[num18]), num11 - (array2[num14] = array2[num18])));
				}
				if (array3[num15] + 1f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length(num13 - (array[num14] = array[num15]), num11 - (array2[num14] = array2[num15])));
				}
				num13++;
				num14++;
			}
			num11++;
			num12 += num;
		}
		int num20 = num - 2;
		int num21 = num20 * num;
		while (num20 >= 1)
		{
			int num22 = num - 2;
			int num23 = num21 + num22;
			while (num22 >= 1)
			{
				int num24 = num23 + 1;
				int num25 = num23 + num;
				int num26 = num25 - 1;
				int num27 = num25 + 1;
				float num28 = array3[num23];
				if (array3[num24] + 1f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length(num22 - (array[num23] = array[num24]), num20 - (array2[num23] = array2[num24])));
				}
				if (array3[num26] + 1.4142135f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length(num22 - (array[num23] = array[num26]), num20 - (array2[num23] = array2[num26])));
				}
				if (array3[num25] + 1f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length(num22 - (array[num23] = array[num25]), num20 - (array2[num23] = array2[num25])));
				}
				if (array3[num27] + 1f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length(num22 - (array[num23] = array[num27]), num20 - (array2[num23] = array2[num27])));
				}
				num22--;
				num23--;
			}
			num20--;
			num21 -= num;
		}
		int num29 = 0;
		int num30 = 0;
		int num31 = num;
		while (num29 < size)
		{
			int num32 = 0;
			int num33 = num31 + 1;
			while (num32 < size)
			{
				distanceField[num30] = ((image[num30] > threshold) ? (0f - array3[num33]) : array3[num33]);
				num32++;
				num30++;
				num33++;
			}
			num29++;
			num31 += num;
		}
	}

	private static float SampleClamped(float[] data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static Vector4 SampleClamped(Vector4[] data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static ushort SampleClamped(ushort[] data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static float SampleClamped(NativeArray<float>.ReadOnly data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static Vector4 SampleClamped(NativeArray<Vector4> data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	public static JobHandle GenerateVectorsNative(in int size, NativeArray<float>.ReadOnly distanceField, NativeArray<Vector4> vectorFieldOut, JobHandle inputDeps)
	{
		SobelGradientsJob sobelGradientsJob = default(SobelGradientsJob);
		sobelGradientsJob.size = size;
		sobelGradientsJob.distanceField = distanceField;
		sobelGradientsJob.vectorField = vectorFieldOut;
		SobelGradientsJob jobData = sobelGradientsJob;
		inputDeps = ParallelJobEx.ScheduleParallel(ref jobData, vectorFieldOut.Length, inputDeps);
		FixBoundaryGradientsJob jobData2 = default(FixBoundaryGradientsJob);
		jobData2.size = size;
		jobData2.vectorField = vectorFieldOut;
		inputDeps = jobData2.Schedule(inputDeps);
		return inputDeps;
	}

	public static void GenerateVectors(in int size, in float[] distanceField, ref Vector4[] vectorField)
	{
		for (int i = 1; i < size - 1; i++)
		{
			for (int j = 1; j < size - 1; j++)
			{
				float z = SampleClamped(distanceField, size, i, j);
				float num = SampleClamped(distanceField, size, i - 1, j - 1);
				float num2 = SampleClamped(distanceField, size, i - 1, j);
				float num3 = SampleClamped(distanceField, size, i - 1, j + 1);
				float num4 = SampleClamped(distanceField, size, i, j - 1);
				float num5 = SampleClamped(distanceField, size, i, j + 1);
				float num6 = SampleClamped(distanceField, size, i + 1, j - 1);
				float num7 = SampleClamped(distanceField, size, i + 1, j);
				float num8 = SampleClamped(distanceField, size, i + 1, j + 1);
				float num9 = num6 + 2f * num7 + num8 - (num + 2f * num2 + num3);
				float num10 = num3 + 2f * num5 + num8 - (num + 2f * num4 + num6);
				Vector2 normalized = new Vector2(0f - num9, 0f - num10).normalized;
				vectorField[j * size + i] = new Vector4(normalized.x, normalized.y, z, 0f);
			}
		}
		for (int k = 1; k < size - 1; k++)
		{
			vectorField[k] = SampleClamped(vectorField, size, k, 1);
			vectorField[(size - 1) * size + k] = SampleClamped(vectorField, size, k, size - 2);
		}
		for (int l = 0; l < size; l++)
		{
			vectorField[l * size] = SampleClamped(vectorField, size, 1, l);
			vectorField[l * size + size - 1] = SampleClamped(vectorField, size, size - 2, l);
		}
	}

	public static JobHandle ApplyGaussianBlurNative(int size, NativeArray<float> distanceField, int steps = 1, JobHandle inputDeps = default(JobHandle))
	{
		if (steps <= 0)
		{
			return inputDeps;
		}
		NativeArray<float> dst = new NativeArray<float>(size * size, Allocator.TempJob);
		for (int i = 0; i < steps; i++)
		{
			BlurHorizontalJob blurHorizontalJob = default(BlurHorizontalJob);
			blurHorizontalJob.size = size;
			blurHorizontalJob.src = distanceField.AsReadOnly();
			blurHorizontalJob.dst = dst;
			BlurHorizontalJob jobData = blurHorizontalJob;
			inputDeps = ParallelJobEx.ScheduleParallel(ref jobData, size * size, inputDeps);
			BlurVerticalJob blurVerticalJob = default(BlurVerticalJob);
			blurVerticalJob.size = size;
			blurVerticalJob.src = dst.AsReadOnly();
			blurVerticalJob.dst = distanceField;
			BlurVerticalJob jobData2 = blurVerticalJob;
			inputDeps = ParallelJobEx.ScheduleParallel(ref jobData2, size * size, inputDeps);
		}
		dst.Dispose(inputDeps);
		return inputDeps;
	}

	public static void ApplyGaussianBlur(int size, float[] distanceField, int steps = 1)
	{
		if (steps <= 0)
		{
			return;
		}
		float[] array = new float[size * size];
		int num = size - 1;
		for (int i = 0; i < steps; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			while (num2 < size)
			{
				int num5 = 0;
				while (num5 < size)
				{
					float num6 = 0f;
					for (int j = 0; j < 7; j++)
					{
						int num7 = num5 + GaussOffsets[j];
						num7 = ((num7 >= 0) ? num7 : 0);
						num7 = ((num7 <= num) ? num7 : num);
						num6 += distanceField[num4 + num7] * GaussWeights[j];
					}
					array[num3] = num6;
					num5++;
					num3++;
				}
				num2++;
				num4 += size;
			}
			int k = 0;
			int num8 = 0;
			for (; k < size; k++)
			{
				int num9 = 0;
				while (num9 < size)
				{
					float num10 = 0f;
					for (int l = 0; l < 7; l++)
					{
						int num11 = k + GaussOffsets[l];
						num11 = ((num11 >= 0) ? num11 : 0);
						num11 = ((num11 <= num) ? num11 : num);
						num10 += array[num11 * size + num9] * GaussWeights[l];
					}
					distanceField[num8] = num10;
					num9++;
					num8++;
				}
			}
		}
	}
}
