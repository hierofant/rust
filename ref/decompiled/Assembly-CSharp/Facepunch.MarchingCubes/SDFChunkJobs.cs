using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

[BurstCompile]
internal static class SDFChunkJobs
{
	[BurstCompile(FloatMode = FloatMode.Fast, DisableSafetyChecks = true)]
	internal struct CleanupIslandsJob : IJob
	{
		public QuantizedFloatData3DArray DataArray;

		public float Iso;

		public void Execute()
		{
			int length = DataArray.FlatArray.Length;
			NativeArray<byte> touched = new NativeArray<byte>(length, Allocator.Temp);
			NativeArray<int4> queue = new NativeArray<int4>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			int num = 0;
			int tail = 0;
			int num2 = 1;
			int num3 = DataArray.Width - 2;
			int num4 = 1;
			int num5 = DataArray.Height - 2;
			int num6 = 1;
			int num7 = DataArray.Depth - 2;
			for (int i = num6; i <= num7; i++)
			{
				for (int j = num2; j <= num3; j++)
				{
					int num8 = DataArray.ToIndex(j, 0, i);
					touched[num8] = 1;
					queue[tail++] = new int4(j, 0, i, num8);
				}
			}
			int width = DataArray.Width;
			int widthHeight = DataArray.WidthHeight;
			while (num < tail)
			{
				int4 @int = queue[num++];
				int w = @int.w;
				if (@int.x > num2)
				{
					TryTouch(w - 1, @int.x - 1, @int.y, @int.z, touched, queue, ref tail);
				}
				if (@int.x < num3)
				{
					TryTouch(w + 1, @int.x + 1, @int.y, @int.z, touched, queue, ref tail);
				}
				if (@int.y > num4)
				{
					TryTouch(w - width, @int.x, @int.y - 1, @int.z, touched, queue, ref tail);
				}
				if (@int.y < num5)
				{
					TryTouch(w + width, @int.x, @int.y + 1, @int.z, touched, queue, ref tail);
				}
				if (@int.z > num6)
				{
					TryTouch(w - widthHeight, @int.x, @int.y, @int.z - 1, touched, queue, ref tail);
				}
				if (@int.z < num7)
				{
					TryTouch(w + widthHeight, @int.x, @int.y, @int.z + 1, touched, queue, ref tail);
				}
			}
			for (int k = 0; k < length; k++)
			{
				if (touched[k] == 0)
				{
					DataArray.FlatArray[k] = byte.MaxValue;
				}
			}
			queue.Dispose();
			touched.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TryTouch(int idx, int x, int y, int z, NativeArray<byte> touched, NativeArray<int4> queue, ref int tail)
		{
			if (touched[idx] == 0)
			{
				touched[idx] = 1;
				if (DataArray.Sample(idx) < Iso)
				{
					queue[tail++] = new int4(x, y, z, idx);
				}
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast, DisableSafetyChecks = true)]
	internal struct AccumulateCensorBoundsJob : IJobParallelForBatch
	{
		[NativeDisableContainerSafetyRestriction]
		public QuantizedFloatData3DArray SrcData;

		public NativeStream.Writer ShapeStream;

		public int SegmentsX;

		public int SegmentsY;

		public int SegmentsZ;

		public float iso;

		public int batchSize;

		public void Execute(int startIndex, int count)
		{
			int num = startIndex / batchSize;
			ShapeStream.PatchMinMaxRange(num);
			ShapeStream.BeginForEachIndex(num);
			NativeList<int3> nativeList = new NativeList<int3>(Allocator.Temp);
			int x = SrcData.Width / SegmentsX;
			int y = SrcData.Height / SegmentsY;
			int z = SrcData.Depth / SegmentsZ;
			int3 @int = new int3(x, y, z);
			int3 int2 = new int3(1);
			int num2 = SegmentsX * SegmentsY;
			for (int i = startIndex; i < startIndex + count; i++)
			{
				int x2 = i % SegmentsX;
				int y2 = i / SegmentsX % SegmentsY;
				int z2 = i / num2;
				int3 int3 = math.max(@int * new int3(x2, y2, z2) - int2, 0);
				int3 int4 = math.min(@int * new int3(x2, y2, z2) + @int + int2, SrcData.Bounds - 1);
				nativeList.Clear();
				int3 x3 = new int3(int.MaxValue);
				int3 x4 = new int3(int.MinValue);
				int3 zero = int3.zero;
				for (int j = int3.z; j <= int4.z; j++)
				{
					for (int k = int3.y; k <= int4.y; k++)
					{
						for (int l = int3.x; l <= int4.x; l++)
						{
							int3 value = new int3(l, k, j);
							if (SrcData.Sample(value) < iso)
							{
								nativeList.Add(in value);
								x3 = math.min(x3, value);
								x4 = math.max(x4, value);
								zero += value;
							}
						}
					}
				}
				if (nativeList.Length >= 3)
				{
					float3 @float = zero / nativeList.Length;
					float3x3 zero2 = float3x3.zero;
					for (int m = 0; m < nativeList.Length; m++)
					{
						float3 float2 = nativeList[m] - @float;
						zero2.c0 += float2 * float2.x;
						zero2.c1 += float2 * float2.y;
						zero2.c2 += float2 * float2.z;
					}
					zero2 /= (float)(nativeList.Length - 1);
					EigenDecomposition(zero2, out var V);
					float3 float3 = math.normalize(V.c0);
					float3 float4 = math.normalize(V.c1);
					float3 float5 = math.normalize(V.c2);
					float3 float6 = new float3(float.MaxValue);
					float3 float7 = new float3(float.MinValue);
					for (int n = 0; n < nativeList.Length; n++)
					{
						float3 x5 = nativeList[n] - @float;
						float3 y3 = new float3(math.dot(x5, float3), math.dot(x5, float4), math.dot(x5, float5));
						float6 = math.min(float6, y3);
						float7 = math.max(float7, y3);
					}
					float3 extents = (float7 - float6) * 0.5f;
					float3 float8 = (float7 + float6) * 0.5f;
					@float = @float + float3 * float8.x + float4 * float8.y + float5 * float8.z;
					ShapeStream.Write(new Shape(ShapeType.OBB, @float, extents, quaternion.LookRotation(float5, float4), isAdditive: true, 0.2f));
				}
			}
			ShapeStream.EndForEachIndex();
		}

		private static void EigenDecomposition(float3x3 A, out float3x3 V)
		{
			V = float3x3.identity;
			for (int i = 0; i < 32; i++)
			{
				float num = math.abs(A.c1.x);
				float num2 = math.abs(A.c2.x);
				float num3 = math.abs(A.c2.y);
				int index;
				int index2;
				if (num > num2 && num > num3)
				{
					index = 0;
					index2 = 1;
				}
				else if (num2 > num3)
				{
					index = 0;
					index2 = 2;
				}
				else
				{
					index = 1;
					index2 = 2;
				}
				if (!(math.abs(A[index][index2]) < 1E-10f))
				{
					float num4 = A[index][index];
					float num5 = A[index2][index2];
					float num6 = A[index][index2];
					float x = 0.5f * math.atan2(2f * num6, num5 - num4);
					float value = math.cos(x);
					float num7 = math.sin(x);
					float3x3 identity = float3x3.identity;
					identity[index][index] = value;
					identity[index2][index2] = value;
					identity[index][index2] = num7;
					identity[index2][index] = 0f - num7;
					A = math.mul(math.transpose(identity), math.mul(A, identity));
					V = math.mul(V, identity);
					continue;
				}
				break;
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast, DisableSafetyChecks = true)]
	internal struct ApplyCensorBoundsJob : IJobParallelFor
	{
		[NativeDisableContainerSafetyRestriction]
		public QuantizedFloatData3DArray OutputArray;

		public NativeStream.Reader ShapeStream;

		public unsafe void Execute(int z)
		{
			byte* unsafePtr = (byte*)OutputArray.FlatArray.GetUnsafePtr();
			UnsafeUtility.MemSet(unsafePtr + z * OutputArray.WidthHeight, byte.MaxValue, OutputArray.WidthHeight);
			for (int i = 0; i < ShapeStream.ForEachCount; i++)
			{
				ShapeStream.BeginForEachIndex(i);
				while (ShapeStream.RemainingItemCount > 0)
				{
					ref Shape reference = ref ShapeStream.Read<Shape>();
					Bounds worldFloatBounds = reference.GetBounds();
					OutputArray.ToLocalIntBounds(in worldFloatBounds, out var min, out var max);
					if (z < min.z || z >= max.z)
					{
						continue;
					}
					for (int j = min.y; j < max.y; j++)
					{
						for (int k = min.x; k < max.x; k++)
						{
							float num = reference.OBBDistance(new float3(k, j, z));
							if (!(num > 2.5f))
							{
								byte val = OutputArray.Compress(num);
								byte b = System.Math.Min(OutputArray.GetByte(k, j, z), val);
								OutputArray.SetByte(k, j, z, b);
							}
						}
					}
				}
				ShapeStream.EndForEachIndex();
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	internal struct ClearBoundariesJob : IJob
	{
		public QuantizedFloatData3DArray DataArray;

		public void Execute()
		{
			for (int i = 0; i < DataArray.Width; i++)
			{
				for (int j = 0; j < DataArray.Height; j++)
				{
					DataArray[i, j, 0] = 255f;
					DataArray[i, j, DataArray.Depth - 1] = 255f;
				}
			}
			for (int k = 0; k < DataArray.Width; k++)
			{
				for (int l = 0; l < DataArray.Depth; l++)
				{
					DataArray[k, 0, l] = 255f;
					DataArray[k, DataArray.Height - 1, l] = 255f;
				}
			}
			for (int m = 0; m < DataArray.Height; m++)
			{
				for (int n = 0; n < DataArray.Depth; n++)
				{
					DataArray[0, m, n] = 255f;
					DataArray[DataArray.Width - 1, m, n] = 255f;
				}
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	internal struct CalculateDistanceFieldJob : IJob
	{
		public float3 Origin;

		public Bounds ChunkBounds;

		public NativeArray<Shape>.ReadOnly Mods;

		public QuantizedFloatData3DArray DataArray;

		private const float TaubinInflateScale = -1.03f;

		public void Execute()
		{
			for (int i = 0; i < Mods.Length; i++)
			{
				Shape mod = Mods[i];
				Apply(in mod);
			}
		}

		private void Apply(in Shape mod)
		{
			Bounds worldFloatBounds = mod.GetBounds();
			if (ChunkBounds.Intersects(worldFloatBounds))
			{
				DataArray.ToLocalIntBounds(in worldFloatBounds, out var min, out var max);
				switch (mod.Type)
				{
				case ShapeType.Sphere:
					ApplyDistanceOps<Facepunch.MarchingCubes.SphereSdf>(in mod, in min, in max);
					break;
				case ShapeType.AABB:
					ApplyDistanceOps<Facepunch.MarchingCubes.AABBSdf>(in mod, in min, in max);
					break;
				case ShapeType.OBB:
					ApplyDistanceOps<Facepunch.MarchingCubes.OBBSdf>(in mod, in min, in max);
					break;
				case ShapeType.SharpOBB:
					ApplyDistanceOps<Facepunch.MarchingCubes.SharpOBBSdf>(in mod, in min, in max);
					break;
				case ShapeType.Cylinder:
					ApplyDistanceOps<Facepunch.MarchingCubes.CylinderSdf>(in mod, in min, in max);
					break;
				case ShapeType.Capsule:
					ApplyDistanceOps<Facepunch.MarchingCubes.CapsuleSdf>(in mod, in min, in max);
					break;
				case ShapeType.Cone:
					ApplyDistanceOps<Facepunch.MarchingCubes.ConeSdf>(in mod, in min, in max);
					break;
				case ShapeType.HexPrism:
					ApplyDistanceOps<Facepunch.MarchingCubes.HexPrismSdf>(in mod, in min, in max);
					break;
				case ShapeType.Bulge:
					ApplyBulgeOp(in mod, in min, in max);
					break;
				case ShapeType.Smooth:
					ApplySmoothOp(in mod, in min, in max);
					break;
				}
			}
		}

		private void ApplyHardOp(int x, int y, int z, float d, bool isAddtive)
		{
			byte b = DataArray.Compress(d);
			byte @byte = DataArray.GetByte(x, y, z);
			byte b2 = (isAddtive ? System.Math.Min(@byte, b) : System.Math.Max(@byte, (byte)(255 - b)));
			DataArray.SetByte(x, y, z, b2);
		}

		private void ApplySmoothedOp(int x, int y, int z, float d, float k, bool isAdditive)
		{
			float num = Decompress(DataArray.GetByte(x, y, z));
			float f = (isAdditive ? SmoothMin(num, d, k) : SmoothMax(d, num, k));
			byte b = DataArray.Compress(f);
			DataArray.SetByte(x, y, z, b);
		}

		private void ApplyBulgeOp(in Shape mod, in int3 min, in int3 max)
		{
			float num = 1f / mod.Extents.x;
			float y = mod.Extents.y;
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					for (int k = min.z; k <= max.z; k++)
					{
						float num2 = math.length(new float3(i, j, k) + Origin - mod.Position) * num;
						if (!(num2 >= 1f))
						{
							float num3 = 1f - num2 * num2 * (3f - 2f * num2);
							float num4 = math.select(y * num3, (0f - y) * num3, mod.IsAdditive);
							byte @byte = DataArray.GetByte(i, j, k);
							byte b = DataArray.Compress(Decompress(@byte) + num4);
							DataArray.SetByte(i, j, k, b);
						}
					}
				}
			}
		}

		private static float Decompress(byte b)
		{
			return ((float)(int)b / 255f - 0.5f) / 0.2f;
		}

		private void ApplySmoothOp(in Shape mod, in int3 min, in int3 max)
		{
			float invRadius = 1f / mod.Extents.x;
			float y = mod.Extents.y;
			SmoothPass(in mod, in min, in max, invRadius, y);
			SmoothPass(in mod, in min, in max, invRadius, y * -1.03f);
		}

		private void SmoothPass(in Shape mod, in int3 min, in int3 max, float invRadius, float strength)
		{
			float num = invRadius * invRadius;
			int width = DataArray.Width;
			int widthHeight = DataArray.WidthHeight;
			int3 @int = DataArray.Bounds - 1;
			float3 @float = Origin - mod.Position;
			for (int i = min.z; i <= max.z; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					float num2 = (float)i + @float.z;
					float num3 = (float)j + @float.y;
					float num4 = (num2 * num2 + num3 * num3) * num;
					if (num4 >= 1f)
					{
						continue;
					}
					int num5 = DataArray.ToIndex(min.x, j, i);
					int num6 = min.x;
					while (num6 <= max.x)
					{
						float num7 = (float)num6 + @float.x;
						float num8 = num4 + num7 * num7 * num;
						if (!(num8 >= 1f))
						{
							float t = strength * (1f - num8 * (3f - 2f * math.sqrt(num8)));
							int3 int2 = new int3(num6, j, i);
							float num9 = (int)DataArray.FlatArray[num5];
							float num10 = ((!(math.all(int2 > int3.zero) & math.all(int2 < @int))) ? (num9 + ClampedTap(int2 + new int3(1, 0, 0)) + ClampedTap(int2 - new int3(1, 0, 0)) + ClampedTap(int2 + new int3(0, 1, 0)) + ClampedTap(int2 - new int3(0, 1, 0)) + ClampedTap(int2 + new int3(0, 0, 1)) + ClampedTap(int2 - new int3(0, 0, 1))) : (num9 + (float)(int)DataArray.FlatArray[num5 - 1] + (float)(int)DataArray.FlatArray[num5 + 1] + (float)(int)DataArray.FlatArray[num5 - width] + (float)(int)DataArray.FlatArray[num5 + width] + (float)(int)DataArray.FlatArray[num5 - widthHeight] + (float)(int)DataArray.FlatArray[num5 + widthHeight]));
							float x = math.clamp(math.lerp(num9, num10 * (1f / 7f), t), 0f, 255f);
							DataArray.FlatArray[num5] = (byte)math.round(x);
						}
						num6++;
						num5++;
					}
				}
			}
		}

		private float ClampedTap(int3 c)
		{
			c = math.clamp(c, int3.zero, DataArray.Bounds - 1);
			return (int)DataArray.GetByte(c.x, c.y, c.z);
		}

		private void ApplyDistanceOps<TSdf>(in Shape mod, in int3 min, in int3 max) where TSdf : struct, Facepunch.MarchingCubes.ISdf
		{
			TSdf val = default(TSdf);
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					for (int k = min.z; k <= max.z; k++)
					{
						float num = val.Distance(in mod, new float3(i, j, k) + Origin);
						if (!(num > 2.5f))
						{
							if (mod.Smoothing > 0f)
							{
								ApplySmoothedOp(i, j, k, num, mod.Smoothing, mod.IsAdditive);
							}
							else
							{
								ApplyHardOp(i, j, k, num, mod.IsAdditive);
							}
						}
					}
				}
			}
		}

		private float SmoothMin(float a, float b, float k)
		{
			k *= 4f;
			float num = math.max(k - math.abs(a - b), 0f);
			return math.min(a, b) - num * num * 0.25f / k;
		}

		private float SmoothMax(float a, float b, float k)
		{
			return 0f - SmoothMin(a, 0f - b, k);
		}
	}
}
