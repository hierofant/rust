using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public struct DemoShotVectorKeyframe : IProto<DemoShotVectorKeyframe>, IProto
{
	[NonSerialized]
	public float keyframeTime;

	[NonSerialized]
	public Vector3 keyFrameValue;

	public static void ResetToPool(DemoShotVectorKeyframe instance)
	{
		instance.keyframeTime = 0f;
		instance.keyFrameValue = default(Vector3);
	}

	public void CopyTo(DemoShotVectorKeyframe instance)
	{
		instance.keyframeTime = keyframeTime;
		instance.keyFrameValue = keyFrameValue;
	}

	public DemoShotVectorKeyframe Copy()
	{
		DemoShotVectorKeyframe demoShotVectorKeyframe = default(DemoShotVectorKeyframe);
		CopyTo(demoShotVectorKeyframe);
		return demoShotVectorKeyframe;
	}

	public static DemoShotVectorKeyframe Deserialize(BufferStream stream)
	{
		DemoShotVectorKeyframe instance = default(DemoShotVectorKeyframe);
		Deserialize(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotVectorKeyframe DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotVectorKeyframe instance = default(DemoShotVectorKeyframe);
		DeserializeLengthDelimited(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotVectorKeyframe DeserializeLength(BufferStream stream, int length)
	{
		DemoShotVectorKeyframe instance = default(DemoShotVectorKeyframe);
		DeserializeLength(stream, length, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotVectorKeyframe Deserialize(byte[] buffer)
	{
		DemoShotVectorKeyframe instance = default(DemoShotVectorKeyframe);
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

	public void WriteToStreamDelta(BufferStream stream, DemoShotVectorKeyframe previous)
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

	public static DemoShotVectorKeyframe Deserialize(BufferStream stream, ref DemoShotVectorKeyframe instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotVectorKeyframe");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.keyFrameValue, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotVectorKeyframe DeserializeLengthDelimited(BufferStream stream, ref DemoShotVectorKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotVectorKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.keyFrameValue, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotVectorKeyframe DeserializeLength(BufferStream stream, int length, ref DemoShotVectorKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotVectorKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.keyFrameValue, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotVectorKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotVectorKeyframe instance, DemoShotVectorKeyframe previous)
	{
		if (instance.keyframeTime != previous.keyframeTime)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameValue != previous.keyFrameValue)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.keyFrameValue, previous.keyFrameValue);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field keyFrameValue (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, DemoShotVectorKeyframe instance)
	{
		if (instance.keyframeTime != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameValue != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.keyFrameValue);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field keyFrameValue (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
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
