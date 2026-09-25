using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public struct DemoShotFloatKeyframe : IProto<DemoShotFloatKeyframe>, IProto
{
	[NonSerialized]
	public float keyframeTime;

	[NonSerialized]
	public float keyFrameValue;

	public static void ResetToPool(DemoShotFloatKeyframe instance)
	{
		instance.keyframeTime = 0f;
		instance.keyFrameValue = 0f;
	}

	public void CopyTo(DemoShotFloatKeyframe instance)
	{
		instance.keyframeTime = keyframeTime;
		instance.keyFrameValue = keyFrameValue;
	}

	public DemoShotFloatKeyframe Copy()
	{
		DemoShotFloatKeyframe demoShotFloatKeyframe = default(DemoShotFloatKeyframe);
		CopyTo(demoShotFloatKeyframe);
		return demoShotFloatKeyframe;
	}

	public static DemoShotFloatKeyframe Deserialize(BufferStream stream)
	{
		DemoShotFloatKeyframe instance = default(DemoShotFloatKeyframe);
		Deserialize(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotFloatKeyframe DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotFloatKeyframe instance = default(DemoShotFloatKeyframe);
		DeserializeLengthDelimited(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotFloatKeyframe DeserializeLength(BufferStream stream, int length)
	{
		DemoShotFloatKeyframe instance = default(DemoShotFloatKeyframe);
		DeserializeLength(stream, length, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotFloatKeyframe Deserialize(byte[] buffer)
	{
		DemoShotFloatKeyframe instance = default(DemoShotFloatKeyframe);
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

	public void WriteToStreamDelta(BufferStream stream, DemoShotFloatKeyframe previous)
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

	public static DemoShotFloatKeyframe Deserialize(BufferStream stream, ref DemoShotFloatKeyframe instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotFloatKeyframe");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				instance.keyFrameValue = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotFloatKeyframe DeserializeLengthDelimited(BufferStream stream, ref DemoShotFloatKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotFloatKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				instance.keyFrameValue = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotFloatKeyframe DeserializeLength(BufferStream stream, int length, ref DemoShotFloatKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotFloatKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				instance.keyFrameValue = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotFloatKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotFloatKeyframe instance, DemoShotFloatKeyframe previous)
	{
		if (instance.keyframeTime != previous.keyframeTime)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameValue != previous.keyFrameValue)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValue);
		}
	}

	public static void Serialize(BufferStream stream, DemoShotFloatKeyframe instance)
	{
		if (instance.keyframeTime != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameValue != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValue);
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
