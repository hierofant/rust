#define UNITY_ASSERTIONS
using System;
using Facepunch;
using LZ4;
using ProtoBuf;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class SculptFormat
{
	private static readonly byte[] _decompressArr = new byte[81920];

	public unsafe static void SerializeToSculpt(BaseSculpture sculpture, BufferStream bufferStream)
	{
		using (TimeWarning.New("SculptFormat.SerializeToSculpt"))
		{
			using Sculpt sculpt = Pool.Get<Sculpt>();
			sculpt.bounds = sculpture.GridResolution;
			NativeArray<byte> flatArray = sculpture.SDFSet.Chunks[0].AcquireDataArray().FlatArray;
			byte[] array = BufferStream.Shared.ArrayPool.Rent(flatArray.Length);
			Debug.Assert(array.Length >= flatArray.Length);
			fixed (byte* ptr = array)
			{
				void* destination = ptr;
				UnsafeUtility.MemCpy(destination, flatArray.GetUnsafePtr(), flatArray.Length * UnsafeUtility.SizeOf<byte>());
			}
			sculpt.data = new ArraySegment<byte>(array, 0, flatArray.Length);
			sculpt.ownerId = sculpture.net.ID;
			sculpt.ToProto(bufferStream);
		}
	}

	public static byte[] GetStorageReadyBuffer(BufferStream bs)
	{
		using (TimeWarning.New("SculptFormat.GetStorageReadyBuffer"))
		{
			ArraySegment<byte> buffer = bs.GetBuffer();
			return LZ4Codec.Encode(buffer.Array, buffer.Offset, buffer.Count);
		}
	}

	public static Sculpt LoadDisposableSculptFromStorage(ArraySegment<byte> encoded)
	{
		using (TimeWarning.New("SculptFormat.LoadFromStorage"))
		{
			int length = 0;
			using (TimeWarning.New("LZ4.Decode"))
			{
				length = LZ4Codec.Decode(encoded.Array, encoded.Offset, encoded.Count, _decompressArr, 0, _decompressArr.Length);
			}
			using BufferStream bufferStream = Pool.Get<BufferStream>().Initialize();
			bufferStream.Clear();
			Sculpt sculpt = Pool.Get<Sculpt>();
			bufferStream.Initialize(_decompressArr, length);
			Sculpt.Deserialize(bufferStream, sculpt, isDelta: false);
			return sculpt;
		}
	}

	public static Sculpt LoadDisposableSculptFromStorage(byte[] encoded)
	{
		return LoadDisposableSculptFromStorage(new ArraySegment<byte>(encoded));
	}
}
