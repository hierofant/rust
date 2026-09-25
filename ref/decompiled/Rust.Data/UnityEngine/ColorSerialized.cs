using System.IO;
using SilentOrbit.ProtocolBuffers;

namespace UnityEngine;

public class ColorSerialized
{
	public static void ResetToPool(Color instance)
	{
		instance.r = 0f;
		instance.g = 0f;
		instance.b = 0f;
		instance.a = 0f;
	}

	public static Color Deserialize(BufferStream stream, ref Color instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.r = 0f;
			instance.g = 0f;
			instance.b = 0f;
			instance.a = 0f;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("UnityEngine.Color");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.r = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.g = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.b = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.a = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Color DeserializeLengthDelimited(BufferStream stream, ref Color instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.r = 0f;
			instance.g = 0f;
			instance.b = 0f;
			instance.a = 0f;
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
			stream.ConsumeFieldOperation("UnityEngine.Color");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.r = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.g = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.b = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.a = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Color DeserializeLength(BufferStream stream, int length, ref Color instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.r = 0f;
			instance.g = 0f;
			instance.b = 0f;
			instance.a = 0f;
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
			stream.ConsumeFieldOperation("UnityEngine.Color");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.r = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.g = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.b = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "UnityEngine.Color");
				instance.a = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "UnityEngine.Color");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Color instance, Color previous)
	{
		if (instance.r != previous.r)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.r);
		}
		if (instance.g != previous.g)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.g);
		}
		if (instance.b != previous.b)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.b);
		}
		if (instance.a != previous.a)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.a);
		}
	}

	public static void Serialize(BufferStream stream, Color instance)
	{
		if (instance.r != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.r);
		}
		if (instance.g != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.g);
		}
		if (instance.b != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.b);
		}
		if (instance.a != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.a);
		}
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
