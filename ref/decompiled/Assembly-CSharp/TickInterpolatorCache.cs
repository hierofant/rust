#define UNITY_ASSERTIONS
using System;
using Unity.Collections;
using UnityEngine;

public class TickInterpolatorCache
{
	public struct PlayerInfo
	{
		public int Count;

		public float Length;
	}

	public struct Segment
	{
		public Vector3 point;

		public readonly float length;

		public Segment(Vector3 a, Vector3 b)
		{
			point = b;
			length = Vector3.Distance(a, b);
		}

		public Segment(Vector3 b)
		{
			point = b;
			length = 0f;
		}
	}

	public struct ReadOnlyState
	{
		public readonly NativeArray<Segment>.ReadOnly Segments;

		public readonly NativeArray<PlayerInfo>.ReadOnly Infos;

		public readonly int BufferSize;

		public ReadOnlyState(NativeArray<Segment>.ReadOnly playerSegments, NativeArray<PlayerInfo>.ReadOnly playerInfos, int bufferSize)
		{
			Segments = playerSegments;
			Infos = playerInfos;
			BufferSize = bufferSize;
		}
	}

	public struct PlayerTickIterator
	{
		private readonly ReadOnlyState state;

		private readonly int playerIndex;

		private Vector3 currPoint;

		private int segmentIndex;

		public Vector3 CurrentPoint => currPoint;

		public Vector3 StartPoint => GetStartPoint(state, playerIndex);

		public Vector3 EndPoint => GetEndPoint(state, playerIndex);

		public float Length => state.Infos[playerIndex].Length;

		public PlayerTickIterator(ReadOnlyState state, int playerIndex)
		{
			this.state = state;
			this.playerIndex = playerIndex;
			segmentIndex = 0;
			currPoint = GetStartPoint(state, playerIndex);
		}

		public bool MoveNext(float distance)
		{
			float num = 0f;
			int num2 = playerIndex * state.BufferSize + 1;
			while (num < distance && HasNext())
			{
				Segment segment = state.Segments[num2 + segmentIndex];
				currPoint = segment.point;
				num += segment.length;
				segmentIndex++;
			}
			return num > 0f;
		}

		public void Reset()
		{
			segmentIndex = 0;
			currPoint = StartPoint;
		}

		public bool HasNext()
		{
			return segmentIndex < state.Infos[playerIndex].Count;
		}
	}

	private NativeArray<Segment> playerSegments;

	private NativeArray<PlayerInfo> playerInfos;

	private int bufferSize = 9;

	public ReadOnlyState ReadOnly => new ReadOnlyState(playerSegments.AsReadOnly(), playerInfos.AsReadOnly(), bufferSize);

	public TickInterpolatorCache(int capacity = 32)
	{
		playerSegments = new NativeArray<Segment>(bufferSize * capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		playerInfos = new NativeArray<PlayerInfo>(capacity, Allocator.Persistent);
	}

	public void Dispose()
	{
		NativeArrayEx.SafeDispose(ref playerSegments);
		NativeArrayEx.SafeDispose(ref playerInfos);
	}

	public void ReplacePlayer(int index)
	{
		playerInfos[index] = default(PlayerInfo);
	}

	public void MovePlayer(int from, int to)
	{
		PlayerInfo value = playerInfos[from];
		playerInfos[to] = value;
		int start = from * bufferSize;
		int length = value.Count + 1;
		NativeArray<Segment> subArray = playerSegments.GetSubArray(start, length);
		int start2 = to * bufferSize;
		NativeArray<Segment> subArray2 = playerSegments.GetSubArray(start2, length);
		subArray.CopyTo(subArray2);
	}

	public void AddTick(BasePlayer player, Vector3 point)
	{
		int activePlayerInd = player.ActivePlayerInd;
		AddTick(activePlayerInd, point);
	}

	public void AddTick(int playerIndex, Vector3 point)
	{
		ref PlayerInfo reference = ref playerInfos.AsSpan()[playerIndex];
		int num = reference.Count + 1;
		if (num >= bufferSize)
		{
			GrowSegments(playerInfos.Length);
		}
		int num2 = playerIndex * bufferSize;
		Vector3 point2 = playerSegments[num2 + num - 1].point;
		Segment value = new Segment(point2, point);
		reference.Length += value.length;
		playerSegments[num2 + num] = value;
		reference.Count++;
	}

	public void Reset(BasePlayer player, Vector3 point)
	{
		int activePlayerInd = player.ActivePlayerInd;
		Reset(activePlayerInd, point);
	}

	public void Reset(int playerIndex, Vector3 point)
	{
		playerInfos[playerIndex] = default(PlayerInfo);
		int index = playerIndex * bufferSize;
		playerSegments[index] = new Segment(point);
	}

	public void Expand(int newCap)
	{
		int length = playerInfos.Length;
		if (newCap > length)
		{
			NativeArrayEx.Expand(ref playerInfos, newCap);
			GrowSegments(length);
		}
	}

	public static Vector3 GetStartPoint(ReadOnlyState state, int playerIndex)
	{
		return state.Segments[playerIndex * state.BufferSize].point;
	}

	public static Vector3 GetEndPoint(ReadOnlyState state, int playerIndex)
	{
		PlayerInfo info = state.Infos[playerIndex];
		return GetEndPoint(state, playerIndex, info);
	}

	public static Vector3 GetEndPoint(ReadOnlyState state, int playerIndex, PlayerInfo info)
	{
		return state.Segments[playerIndex * state.BufferSize + info.Count].point;
	}

	public void TransformEntries(int playerIndex, in Matrix4x4 matrix)
	{
		PlayerInfo info = playerInfos[playerIndex];
		TransformEntries(playerIndex, info, in matrix);
	}

	public void TransformEntries(int playerIndex, PlayerInfo info, in Matrix4x4 matrix)
	{
		Span<Segment> span = playerSegments.GetSubArray(playerIndex * bufferSize, info.Count + 1).AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref Segment reference = ref span[i];
			reference.point = matrix.MultiplyPoint3x4(reference.point);
		}
	}

	public static PlayerTickIterator GetPlayerTickIterator(ReadOnlyState state, int playerIndex)
	{
		Debug.Assert(playerIndex >= 0 && playerIndex < state.Infos.Length);
		return new PlayerTickIterator(state, playerIndex);
	}

	private void GrowSegments(int oldPlayerCap)
	{
		int length = playerInfos.Length;
		int num = bufferSize;
		if (length == oldPlayerCap)
		{
			bufferSize += 4;
		}
		NativeArray<Segment> nativeArray = new NativeArray<Segment>(length * bufferSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < oldPlayerCap; i++)
		{
			int count = playerInfos[i].Count;
			if (count > 0)
			{
				NativeArray<Segment> subArray = playerSegments.GetSubArray(i * num, count + 1);
				NativeArray<Segment> subArray2 = nativeArray.GetSubArray(i * bufferSize, count + 1);
				subArray.CopyTo(subArray2);
			}
			else
			{
				nativeArray[i * bufferSize] = playerSegments[i * num];
			}
		}
		playerSegments.Dispose();
		playerSegments = nativeArray;
	}
}
