using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public struct Half3 : IEquatable<Half3>, IProto<Half3>, IProto
{
	[NonSerialized]
	public uint x;

	[NonSerialized]
	public uint y;

	[NonSerialized]
	public uint z;

	public bool Equals(Half3 other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Half3)
		{
			return Equals((Half3)obj);
		}
		return false;
	}

	public static bool operator ==(Half3 a, Half3 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Half3 a, Half3 b)
	{
		return !a.Equals(b);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y, z);
	}

	public static void ResetToPool(Half3 instance)
	{
		instance.x = 0u;
		instance.y = 0u;
		instance.z = 0u;
	}

	public void CopyTo(Half3 instance)
	{
		instance.x = x;
		instance.y = y;
		instance.z = z;
	}

	public Half3 Copy()
	{
		Half3 half = default(Half3);
		CopyTo(half);
		return half;
	}

	public static Half3 Deserialize(BufferStream stream)
	{
		Half3 instance = default(Half3);
		Deserialize(stream, ref instance, isDelta: false);
		return instance;
	}

	public static Half3 DeserializeLengthDelimited(BufferStream stream)
	{
		Half3 instance = default(Half3);
		DeserializeLengthDelimited(stream, ref instance, isDelta: false);
		return instance;
	}

	public static Half3 DeserializeLength(BufferStream stream, int length)
	{
		Half3 instance = default(Half3);
		DeserializeLength(stream, length, ref instance, isDelta: false);
		return instance;
	}

	public static Half3 Deserialize(byte[] buffer)
	{
		Half3 instance = default(Half3);
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ref instance, isDelta: false);
		return instance;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, ref this, isDelta);
	}

	public void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void WriteToStreamDelta(BufferStream stream, Half3 previous)
	{
		SerializeDelta(stream, this, previous);
	}

	public void ReadFromStream(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, ref this, isDelta);
	}

	public void ReadFromStream(BufferStream stream, int size, bool isDelta = false)
	{
		DeserializeLength(stream, size, ref this, isDelta);
	}

	public static Half3 Deserialize(BufferStream stream, ref Half3 instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0u;
			instance.y = 0u;
			instance.z = 0u;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Half3");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.x = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.y = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.z = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Half3 DeserializeLengthDelimited(BufferStream stream, ref Half3 instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0u;
			instance.y = 0u;
			instance.z = 0u;
		}
		long num = ProtocolParser.ReadUInt32(stream);
		num += stream.Position;
		uint lastFieldId = 0u;
		while (true)
		{
			if (stream.Position >= num)
			{
				if (stream.Position == num)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int num2 = stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException();
			}
			stream.ConsumeFieldOperation("ProtoBuf.Half3");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.x = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.y = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.z = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Half3 DeserializeLength(BufferStream stream, int length, ref Half3 instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0u;
			instance.y = 0u;
			instance.z = 0u;
		}
		long num = stream.Position + length;
		uint lastFieldId = 0u;
		while (true)
		{
			if (stream.Position >= num)
			{
				if (stream.Position == num)
				{
					break;
				}
				throw new ProtocolBufferException("Read past max limit");
			}
			int num2 = stream.ReadByte();
			if (num2 == -1)
			{
				throw new EndOfStreamException();
			}
			stream.ConsumeFieldOperation("ProtoBuf.Half3");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.x = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.y = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Half3");
				instance.z = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Half3");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Half3 instance, Half3 previous)
	{
		if (instance.x != previous.x)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.x);
		}
		if (instance.y != previous.y)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.y);
		}
		if (instance.z != previous.z)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.z);
		}
	}

	public static void Serialize(BufferStream stream, Half3 instance)
	{
		if (instance.x != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.x);
		}
		if (instance.y != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.y);
		}
		if (instance.z != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.z);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
