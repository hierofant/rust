using System;
using System.IO;
using System.Text;

namespace SilentOrbit.ProtocolBuffers;

public static class ProtocolParser
{
	private const int staticBufferSize = 131072;

	[ThreadStatic]
	private static byte[] _staticBuffer;

	private static byte[] GetStaticBuffer()
	{
		return _staticBuffer ?? (_staticBuffer = new byte[131072]);
	}

	public static int ReadFixedInt32(BufferStream stream)
	{
		return stream.Read<int>();
	}

	public static void WriteFixedInt32(BufferStream stream, int i)
	{
		stream.Write(i);
	}

	public static long ReadFixedInt64(BufferStream stream)
	{
		return stream.Read<long>();
	}

	public static void WriteFixedInt64(BufferStream stream, long i)
	{
		stream.Write(i);
	}

	public static float ReadSingle(BufferStream stream)
	{
		float num = stream.Read<float>();
		stream.ValidateFiniteValue(num);
		return num;
	}

	public static void WriteSingle(BufferStream stream, float f)
	{
		stream.Write(f);
	}

	public static double ReadDouble(BufferStream stream)
	{
		double num = stream.Read<double>();
		stream.ValidateFiniteValue(num);
		return num;
	}

	public static void WriteDouble(BufferStream stream, double f)
	{
		stream.Write(f);
	}

	public static int ReadLength(BufferStream stream)
	{
		uint num = ReadUInt32(stream);
		int num2 = stream.Length - stream.Position;
		if (num2 < 0 || num > (uint)num2)
		{
			throw new ProtocolBufferException("Length exceeds remaining bytes");
		}
		return (int)num;
	}

	public unsafe static string ReadString(BufferStream stream)
	{
		try
		{
			int num = ReadLength(stream);
			if (num <= 0)
			{
				return "";
			}
			Span<byte> span = stream.GetRange(num).GetSpan();
			fixed (byte* bytes = &span[0])
			{
				return Encoding.UTF8.GetString(bytes, num);
			}
		}
		finally
		{
		}
	}

	public static void WriteString(BufferStream stream, string val)
	{
		byte[] staticBuffer = GetStaticBuffer();
		int bytes = Encoding.UTF8.GetBytes(val, 0, val.Length, staticBuffer, 0);
		WriteUInt32(stream, (uint)bytes);
		if (bytes > 0)
		{
			new Span<byte>(staticBuffer, 0, bytes).CopyTo(stream.GetRange(bytes).GetSpan());
		}
	}

	public static byte[] ReadBytes(BufferStream stream)
	{
		try
		{
			int num = ReadLength(stream);
			byte[] array = new byte[num];
			ReadBytesInto(stream, array, num);
			return array;
		}
		finally
		{
		}
	}

	public static ArraySegment<byte> ReadPooledBytes(BufferStream stream)
	{
		try
		{
			int num = ReadLength(stream);
			byte[] array = BufferStream.Shared.ArrayPool.Rent(num);
			ReadBytesInto(stream, array, num);
			return new ArraySegment<byte>(array, 0, num);
		}
		finally
		{
		}
	}

	private static void ReadBytesInto(BufferStream stream, byte[] buffer, int length)
	{
		stream.GetRange(length).GetSpan().CopyTo(buffer);
	}

	public static void SkipBytes(BufferStream stream)
	{
		int count = ReadLength(stream);
		stream.Skip(count);
	}

	public static void WriteBytes(BufferStream stream, byte[] val)
	{
		WriteUInt32(stream, (uint)val.Length);
		new Span<byte>(val).CopyTo(stream.GetRange(val.Length).GetSpan());
	}

	public static void WritePooledBytes(BufferStream stream, ArraySegment<byte> segment)
	{
		if (segment.Array == null)
		{
			WriteUInt32(stream, 0u);
			return;
		}
		WriteUInt32(stream, (uint)segment.Count);
		new Span<byte>(segment.Array, segment.Offset, segment.Count).CopyTo(stream.GetRange(segment.Count).GetSpan());
	}

	public static Key ReadKey(BufferStream stream)
	{
		uint num = ReadUInt32(stream);
		return new Key(num >> 3, (Wire)((int)num & 7));
	}

	public static Key ReadKey(byte firstByte, BufferStream stream)
	{
		if (firstByte < 128)
		{
			return new Key((uint)(firstByte >> 3), (Wire)(firstByte & 7));
		}
		return new Key((ReadUInt32(stream) << 4) | ((uint)(firstByte >> 3) & 0xFu), (Wire)(firstByte & 7));
	}

	public static void WriteKey(BufferStream stream, Key key)
	{
		uint val = (key.Field << 3) | (uint)key.WireType;
		WriteUInt32(stream, val);
	}

	public static void SkipKey(BufferStream stream, Key key)
	{
		switch (key.WireType)
		{
		case Wire.Fixed32:
			stream.Skip(4);
			break;
		case Wire.Fixed64:
			stream.Skip(8);
			break;
		case Wire.LengthDelimited:
			stream.Skip(ReadLength(stream));
			break;
		case Wire.Varint:
			ReadSkipVarInt(stream);
			break;
		default:
			throw new NotImplementedException("Unknown wire type: " + key.WireType);
		}
	}

	public static void ReadSkipVarInt(BufferStream stream)
	{
		int num;
		do
		{
			num = stream.ReadByte();
			if (num < 0)
			{
				throw new IOException("Stream ended too early");
			}
		}
		while (((uint)num & 0x80u) != 0);
	}

	public static uint ReadUInt32(Span<byte> array, int pos, out int length)
	{
		uint num = 0u;
		length = 0;
		for (int i = 0; i < 5; i++)
		{
			length++;
			if (pos >= array.Length)
			{
				break;
			}
			int num2 = array[pos++];
			if (num2 < 0)
			{
				throw new IOException("Stream ended too early");
			}
			if (i == 4 && ((uint)num2 & 0xF0u) != 0)
			{
				throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
			}
			if ((num2 & 0x80) == 0)
			{
				return num | (uint)(num2 << 7 * i);
			}
			num |= (uint)((num2 & 0x7F) << 7 * i);
		}
		throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
	}

	public static int WriteUInt32(uint val, Span<byte> array, int pos)
	{
		int num = 0;
		while (pos < array.Length)
		{
			num++;
			byte b = (byte)(val & 0x7Fu);
			val >>= 7;
			if (val == 0)
			{
				array[pos++] = b;
				break;
			}
			b = (byte)(b | 0x80u);
			array[pos++] = b;
		}
		return num;
	}

	public static int ReadZInt32(BufferStream stream)
	{
		uint num = ReadUInt32(stream);
		return (int)(num >> 1) ^ ((int)(num << 31) >> 31);
	}

	public static void WriteZInt32(BufferStream stream, int val)
	{
		WriteUInt32(stream, (uint)((val << 1) ^ (val >> 31)));
	}

	public static uint ReadUInt32(BufferStream stream)
	{
		uint num = 0u;
		for (int i = 0; i < 5; i++)
		{
			int num2 = stream.ReadByte();
			if (num2 < 0)
			{
				throw new IOException("Stream ended too early");
			}
			if (i == 4 && ((uint)num2 & 0xF0u) != 0)
			{
				throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
			}
			if ((num2 & 0x80) == 0)
			{
				return num | (uint)(num2 << 7 * i);
			}
			num |= (uint)((num2 & 0x7F) << 7 * i);
		}
		throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
	}

	public static void WriteUInt32(BufferStream stream, uint val)
	{
		byte b;
		while (true)
		{
			b = (byte)(val & 0x7Fu);
			val >>= 7;
			if (val == 0)
			{
				break;
			}
			b = (byte)(b | 0x80u);
			stream.WriteByte(b);
		}
		stream.WriteByte(b);
	}

	public static uint ReadUInt32(Stream stream)
	{
		uint num = 0u;
		for (int i = 0; i < 5; i++)
		{
			int num2 = stream.ReadByte();
			if (num2 < 0)
			{
				throw new IOException("Stream ended too early");
			}
			if (i == 4 && ((uint)num2 & 0xF0u) != 0)
			{
				throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
			}
			if ((num2 & 0x80) == 0)
			{
				return num | (uint)(num2 << 7 * i);
			}
			num |= (uint)((num2 & 0x7F) << 7 * i);
		}
		throw new ProtocolBufferException("Got larger VarInt than 32bit unsigned");
	}

	public static void WriteUInt32(Stream stream, uint val)
	{
		byte b;
		while (true)
		{
			b = (byte)(val & 0x7Fu);
			val >>= 7;
			if (val == 0)
			{
				break;
			}
			b = (byte)(b | 0x80u);
			stream.WriteByte(b);
		}
		stream.WriteByte(b);
	}

	public static long ReadZInt64(BufferStream stream)
	{
		ulong num = ReadUInt64(stream);
		return (long)(num >> 1) ^ ((long)(num << 63) >> 63);
	}

	public static void WriteZInt64(BufferStream stream, long val)
	{
		WriteUInt64(stream, (ulong)((val << 1) ^ (val >> 63)));
	}

	public static ulong ReadUInt64(BufferStream stream)
	{
		ulong num = 0uL;
		for (int i = 0; i < 10; i++)
		{
			int num2 = stream.ReadByte();
			if (num2 < 0)
			{
				throw new IOException("Stream ended too early");
			}
			if (i == 9 && ((uint)num2 & 0xFEu) != 0)
			{
				throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
			}
			if ((num2 & 0x80) == 0)
			{
				return num | (ulong)((long)num2 << 7 * i);
			}
			num |= (ulong)((long)(num2 & 0x7F) << 7 * i);
		}
		throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
	}

	public static void WriteUInt64(BufferStream stream, ulong val)
	{
		byte b;
		while (true)
		{
			b = (byte)(val & 0x7F);
			val >>= 7;
			if (val == 0L)
			{
				break;
			}
			b = (byte)(b | 0x80u);
			stream.WriteByte(b);
		}
		stream.WriteByte(b);
	}

	public static ulong ReadUInt64(Span<byte> array, int pos, out int length)
	{
		ulong num = 0uL;
		length = 0;
		for (int i = 0; i < 10; i++)
		{
			length++;
			int num2 = array[pos++];
			if (num2 < 0)
			{
				throw new IOException("Stream ended too early");
			}
			if (i == 9 && ((uint)num2 & 0xFEu) != 0)
			{
				throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
			}
			if ((num2 & 0x80) == 0)
			{
				return num | (ulong)((long)num2 << 7 * i);
			}
			num |= (ulong)((long)(num2 & 0x7F) << 7 * i);
		}
		throw new ProtocolBufferException("Got larger VarInt than 64 bit unsigned");
	}

	public static int WriteUInt64(ulong val, Span<byte> buffer, int pos)
	{
		int num = 0;
		byte b;
		while (true)
		{
			num++;
			b = (byte)(val & 0x7F);
			val >>= 7;
			if (val == 0L)
			{
				break;
			}
			b = (byte)(b | 0x80u);
			buffer[pos++] = b;
		}
		buffer[pos] = b;
		return num;
	}

	public static bool ReadBool(BufferStream stream)
	{
		int num = stream.ReadByte();
		if (num < 0)
		{
			throw new IOException("Stream ended too early");
		}
		return num switch
		{
			1 => true, 
			0 => false, 
			_ => throw new ProtocolBufferException("Invalid boolean value"), 
		};
	}

	public static void WriteBool(BufferStream stream, bool val)
	{
		stream.WriteByte((byte)(val ? 1 : 0));
	}
}
