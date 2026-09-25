using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

public static class ProtoStreamExtensions
{
	public static void WriteToStream(this IProto proto, Stream stream, bool lengthDelimited = false, int maxSizeHint = 2097152)
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using BufferStream bufferStream = Pool.Get<BufferStream>().Initialize();
		(int MaxLength, int LengthPrefixSize) lengthPrefixSize = GetLengthPrefixSize(maxSizeHint);
		int item = lengthPrefixSize.MaxLength;
		int item2 = lengthPrefixSize.LengthPrefixSize;
		BufferStream.RangeHandle rangeHandle = default(BufferStream.RangeHandle);
		if (lengthDelimited)
		{
			rangeHandle = bufferStream.GetRange(item2);
		}
		int position = bufferStream.Position;
		proto.WriteToStream(bufferStream);
		if (lengthDelimited)
		{
			int num = bufferStream.Position - position;
			if (num > item)
			{
				throw new InvalidOperationException($"Written proto exceeds maximum size hint (maxSizeHint={maxSizeHint}, actualLength={num})");
			}
			Span<byte> span = rangeHandle.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 != item2)
			{
				span[num2 - 1] |= 128;
				while (num2 < item2 - 1)
				{
					span[num2++] = 128;
				}
				span[num2] = 0;
			}
		}
		if (bufferStream.Length > 0)
		{
			ArraySegment<byte> buffer = bufferStream.GetBuffer();
			stream.Write(buffer.Array, buffer.Offset, buffer.Count);
		}
	}

	private static (int MaxLength, int LengthPrefixSize) GetLengthPrefixSize(int maxSizeHint)
	{
		if (maxSizeHint < 0)
		{
			throw new ArgumentOutOfRangeException("maxSizeHint");
		}
		if (maxSizeHint <= 127)
		{
			return (MaxLength: 127, LengthPrefixSize: 1);
		}
		if (maxSizeHint <= 16383)
		{
			return (MaxLength: 16383, LengthPrefixSize: 2);
		}
		if (maxSizeHint <= 2097151)
		{
			return (MaxLength: 2097151, LengthPrefixSize: 3);
		}
		if (maxSizeHint <= 268435455)
		{
			return (MaxLength: 16777215, LengthPrefixSize: 4);
		}
		throw new ArgumentOutOfRangeException("maxSizeHint");
	}

	public static void ReadFromStream(this IProto proto, Stream stream, bool isDelta = false, int maxSize = 1048576)
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		byte[] array = BufferStream.Shared.ArrayPool.Rent(maxSize);
		int num = 0;
		int num2 = maxSize;
		while (num2 > 0)
		{
			int num3 = stream.Read(array, num, num2);
			if (num3 <= 0)
			{
				break;
			}
			num += num3;
			num2 -= num3;
		}
		using BufferStream bufferStream = Pool.Get<BufferStream>().Initialize(array, num);
		proto.ReadFromStream(bufferStream, isDelta);
		BufferStream.Shared.ArrayPool.Return(array);
		int position2 = bufferStream.Position;
		stream.Position = position + position2;
	}

	public static void ReadFromStream(this IProto proto, Stream stream, int length, bool isDelta = false)
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		byte[] array = BufferStream.Shared.ArrayPool.Rent(length);
		int num = 0;
		int num2 = length;
		while (num2 > 0)
		{
			int num3 = stream.Read(array, num, num2);
			if (num3 <= 0)
			{
				throw new InvalidOperationException("Unexpected end of stream");
			}
			num += num3;
			num2 -= num3;
		}
		using BufferStream stream2 = Pool.Get<BufferStream>().Initialize(array, length);
		proto.ReadFromStream(stream2, isDelta);
		BufferStream.Shared.ArrayPool.Return(array);
	}

	public static void ReadFromStreamLengthDelimited(this IProto proto, Stream stream, bool isDelta = false)
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		int length = (int)ProtocolParser.ReadUInt32(stream);
		proto.ReadFromStream(stream, length, isDelta);
	}

	public static byte[] ToProtoBytes(this IProto proto)
	{
		if (proto == null)
		{
			throw new ArgumentNullException("proto");
		}
		using BufferStream bufferStream = Pool.Get<BufferStream>().Initialize();
		proto.WriteToStream(bufferStream);
		ArraySegment<byte> buffer = bufferStream.GetBuffer();
		byte[] array = new byte[bufferStream.Position];
		new Span<byte>(buffer.Array, buffer.Offset, buffer.Count).CopyTo(array);
		return array;
	}
}
