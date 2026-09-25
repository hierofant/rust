using System.IO;
using SilentOrbit.ProtocolBuffers;

namespace UnityEngine;

public class Vector2Serialized
{
	public static void ResetToPool(Vector2 instance)
	{
		instance.x = 0f;
		instance.y = 0f;
	}

	public static Vector2 Deserialize(BufferStream stream, ref Vector2 instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0f;
			instance.y = 0f;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("UnityEngine.Vector2");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Vector2");
				instance.x = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Vector2");
				instance.y = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Vector2 DeserializeLengthDelimited(BufferStream stream, ref Vector2 instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0f;
			instance.y = 0f;
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
			stream.ConsumeFieldOperation("UnityEngine.Vector2");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Vector2");
				instance.x = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Vector2");
				instance.y = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Vector2 DeserializeLength(BufferStream stream, int length, ref Vector2 instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0f;
			instance.y = 0f;
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
			stream.ConsumeFieldOperation("UnityEngine.Vector2");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Vector2");
				instance.x = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Vector2");
				instance.y = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Vector2");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Vector2 instance, Vector2 previous)
	{
		if (instance.x != previous.x)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.x);
		}
		if (instance.y != previous.y)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.y);
		}
	}

	public static void Serialize(BufferStream stream, Vector2 instance)
	{
		if (instance.x != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.x);
		}
		if (instance.y != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.y);
		}
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
