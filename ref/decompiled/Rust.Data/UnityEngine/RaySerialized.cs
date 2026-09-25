using System;
using System.IO;
using SilentOrbit.ProtocolBuffers;

namespace UnityEngine;

public class RaySerialized
{
	public static void ResetToPool(Ray instance)
	{
		instance.origin = default(Vector3);
		instance.direction = default(Vector3);
	}

	public static Ray Deserialize(BufferStream stream, ref Ray instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("UnityEngine.Ray");
			switch (num)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Ray");
				Vector3 instance3 = instance.origin;
				instance.origin = Vector3Serialized.DeserializeLengthDelimited(stream, ref instance3, isDelta);
				continue;
			}
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Ray");
				Vector3 instance2 = instance.direction;
				instance.direction = Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Ray DeserializeLengthDelimited(BufferStream stream, ref Ray instance, bool isDelta)
	{
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
			stream.ConsumeFieldOperation("UnityEngine.Ray");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Ray");
				Vector3 instance3 = instance.origin;
				instance.origin = Vector3Serialized.DeserializeLengthDelimited(stream, ref instance3, isDelta);
				continue;
			}
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Ray");
				Vector3 instance2 = instance.direction;
				instance.direction = Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Ray DeserializeLength(BufferStream stream, int length, ref Ray instance, bool isDelta)
	{
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
			stream.ConsumeFieldOperation("UnityEngine.Ray");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Ray");
				Vector3 instance3 = instance.origin;
				instance.origin = Vector3Serialized.DeserializeLengthDelimited(stream, ref instance3, isDelta);
				continue;
			}
			case 18:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Ray");
				Vector3 instance2 = instance.direction;
				instance.direction = Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Ray");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Ray instance, Ray previous)
	{
		if (instance.origin != previous.origin)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.origin, previous.origin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field origin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.direction != previous.direction)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.direction, previous.direction);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field direction (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public static void Serialize(BufferStream stream, Ray instance)
	{
		if (instance.origin != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.origin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field origin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.direction != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.direction);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field direction (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
