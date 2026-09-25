using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public struct DemoShotQuaternionKeyframe : IProto<DemoShotQuaternionKeyframe>, IProto
{
	[NonSerialized]
	public float keyframeTime;

	[NonSerialized]
	public float keyFrameValueX;

	[NonSerialized]
	public float keyFrameValueY;

	[NonSerialized]
	public float keyFrameValueZ;

	[NonSerialized]
	public float keyFrameValueW;

	public static void ResetToPool(DemoShotQuaternionKeyframe instance)
	{
		instance.keyframeTime = 0f;
		instance.keyFrameValueX = 0f;
		instance.keyFrameValueY = 0f;
		instance.keyFrameValueZ = 0f;
		instance.keyFrameValueW = 0f;
	}

	public void CopyTo(DemoShotQuaternionKeyframe instance)
	{
		instance.keyframeTime = keyframeTime;
		instance.keyFrameValueX = keyFrameValueX;
		instance.keyFrameValueY = keyFrameValueY;
		instance.keyFrameValueZ = keyFrameValueZ;
		instance.keyFrameValueW = keyFrameValueW;
	}

	public DemoShotQuaternionKeyframe Copy()
	{
		DemoShotQuaternionKeyframe demoShotQuaternionKeyframe = default(DemoShotQuaternionKeyframe);
		CopyTo(demoShotQuaternionKeyframe);
		return demoShotQuaternionKeyframe;
	}

	public static DemoShotQuaternionKeyframe Deserialize(BufferStream stream)
	{
		DemoShotQuaternionKeyframe instance = default(DemoShotQuaternionKeyframe);
		Deserialize(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotQuaternionKeyframe DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotQuaternionKeyframe instance = default(DemoShotQuaternionKeyframe);
		DeserializeLengthDelimited(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotQuaternionKeyframe DeserializeLength(BufferStream stream, int length)
	{
		DemoShotQuaternionKeyframe instance = default(DemoShotQuaternionKeyframe);
		DeserializeLength(stream, length, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotQuaternionKeyframe Deserialize(byte[] buffer)
	{
		DemoShotQuaternionKeyframe instance = default(DemoShotQuaternionKeyframe);
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

	public void WriteToStreamDelta(BufferStream stream, DemoShotQuaternionKeyframe previous)
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

	public static DemoShotQuaternionKeyframe Deserialize(BufferStream stream, ref DemoShotQuaternionKeyframe instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotQuaternionKeyframe");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueX = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueY = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueZ = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueW = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotQuaternionKeyframe DeserializeLengthDelimited(BufferStream stream, ref DemoShotQuaternionKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotQuaternionKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueX = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueY = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueZ = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueW = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotQuaternionKeyframe DeserializeLength(BufferStream stream, int length, ref DemoShotQuaternionKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotQuaternionKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueX = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueY = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueZ = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				instance.keyFrameValueW = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotQuaternionKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotQuaternionKeyframe instance, DemoShotQuaternionKeyframe previous)
	{
		if (instance.keyframeTime != previous.keyframeTime)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameValueX != previous.keyFrameValueX)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueX);
		}
		if (instance.keyFrameValueY != previous.keyFrameValueY)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueY);
		}
		if (instance.keyFrameValueZ != previous.keyFrameValueZ)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueZ);
		}
		if (instance.keyFrameValueW != previous.keyFrameValueW)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueW);
		}
	}

	public static void Serialize(BufferStream stream, DemoShotQuaternionKeyframe instance)
	{
		if (instance.keyframeTime != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameValueX != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueX);
		}
		if (instance.keyFrameValueY != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueY);
		}
		if (instance.keyFrameValueZ != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueZ);
		}
		if (instance.keyFrameValueW != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.keyFrameValueW);
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
