using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CompanionServer.Cameras;

[BurstCompile]
public struct RaycastRayProcessingJob : IJobParallelFor
{
	public const byte WaterMaterialIndex = 2;

	public float3 cameraForward;

	public float farPlane;

	public bool oceanEnabled;

	public float oceanLevel;

	public int oceanTopologyMask;

	public int topologyRes;

	public float2 topologyOrigin;

	public float2 topologyOneOverSize;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly topology;

	[Unity.Collections.ReadOnly]
	public NativeArray<RaycastCommand> raycastCommands;

	[Unity.Collections.ReadOnly]
	public NativeArray<RaycastHit> raycastHits;

	[Unity.Collections.ReadOnly]
	public NativeArray<int> colliderIds;

	[Unity.Collections.ReadOnly]
	public NativeArray<byte> colliderMaterials;

	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<int> colliderHits;

	[NativeMatchesParallelForLength]
	[WriteOnly]
	public NativeArray<int> outputs;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> foundCollidersIndex;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> foundColliders;

	public void Execute(int index)
	{
		ref readonly RaycastHit @readonly = ref BurstUtil.GetReadonly(in raycastHits, index);
		int colliderId = @readonly.GetColliderId();
		bool flag = colliderId != 0;
		byte b = 0;
		if (flag)
		{
			int num = Interlocked.Increment(ref BurstUtil.Get(in foundCollidersIndex, 0));
			if (num <= foundColliders.Length)
			{
				foundColliders[num - 1] = colliderId;
			}
			int num2 = BinarySearch(colliderIds, colliderId);
			if (num2 >= 0)
			{
				b = colliderMaterials[num2];
				Interlocked.Increment(ref BurstUtil.Get(in colliderHits, num2));
			}
		}
		float num3 = (flag ? @readonly.distance : farPlane);
		float3 y = (flag ? ((float3)@readonly.normal) : float3.zero);
		if (oceanEnabled)
		{
			float3 @float = raycastCommands[index].from;
			float num4 = -1f;
			float3 float2 = float3.zero;
			if (flag)
			{
				float3 end = @readonly.point;
				if (@float.y > oceanLevel && end.y < oceanLevel)
				{
					float num5 = (@float.y - oceanLevel) / (@float.y - end.y);
					num4 = @readonly.distance * num5;
					float2 = math.lerp(@float, end, num5);
				}
			}
			else
			{
				float3 float3 = raycastCommands[index].direction;
				if (@float.y > oceanLevel && float3.y < 0f)
				{
					float num6 = (oceanLevel - @float.y) / float3.y;
					if (num6 < farPlane)
					{
						num4 = num6;
						float2 = @float + float3 * num6;
					}
				}
			}
			if (num4 >= 0f)
			{
				int upperBound = topologyRes - 1;
				int num7 = math.clamp((int)((float2.x - topologyOrigin.x) * topologyOneOverSize.x * (float)topologyRes), 0, upperBound);
				int num8 = math.clamp((int)((float2.z - topologyOrigin.y) * topologyOneOverSize.y * (float)topologyRes), 0, upperBound);
				if ((topology[num8 * topologyRes + num7] & oceanTopologyMask) != 0)
				{
					b = 2;
					num3 = num4;
					y = new float3(0f, 1f, 0f);
				}
			}
		}
		float num9 = math.clamp(num3 / farPlane, 0f, 1f);
		float num10 = math.max(math.dot(cameraForward, y), 0f);
		ushort num11 = (ushort)(num9 * 1023f);
		byte b2 = (byte)(num10 * 63f);
		outputs[index] = (num11 >> 8 << 24) | ((num11 & 0xFF) << 16) | (b2 << 8) | b;
	}

	private static int BinarySearch(NativeArray<int> haystack, int needle)
	{
		int num = 0;
		int num2 = haystack.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num / 2);
			int num4 = Compare(haystack[num3], needle);
			if (num4 == 0)
			{
				return num3;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		return ~num;
	}

	private static int Compare(int x, int y)
	{
		if (x < y)
		{
			return -1;
		}
		if (x > y)
		{
			return 1;
		}
		return 0;
	}
}
