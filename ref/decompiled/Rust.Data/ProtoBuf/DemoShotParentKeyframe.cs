using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public struct DemoShotParentKeyframe : IProto<DemoShotParentKeyframe>, IProto
{
	[NonSerialized]
	public float keyframeTime;

	[NonSerialized]
	public NetworkableId keyFrameParentId;

	[NonSerialized]
	public string keyFrameParentName;

	public static void ResetToPool(DemoShotParentKeyframe instance)
	{
		instance.keyframeTime = 0f;
		instance.keyFrameParentId = default(NetworkableId);
		instance.keyFrameParentName = string.Empty;
	}

	public void CopyTo(DemoShotParentKeyframe instance)
	{
		instance.keyframeTime = keyframeTime;
		instance.keyFrameParentId = keyFrameParentId;
		instance.keyFrameParentName = keyFrameParentName;
	}

	public DemoShotParentKeyframe Copy()
	{
		DemoShotParentKeyframe demoShotParentKeyframe = default(DemoShotParentKeyframe);
		CopyTo(demoShotParentKeyframe);
		return demoShotParentKeyframe;
	}

	public static DemoShotParentKeyframe Deserialize(BufferStream stream)
	{
		DemoShotParentKeyframe instance = default(DemoShotParentKeyframe);
		Deserialize(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotParentKeyframe DeserializeLengthDelimited(BufferStream stream)
	{
		DemoShotParentKeyframe instance = default(DemoShotParentKeyframe);
		DeserializeLengthDelimited(stream, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotParentKeyframe DeserializeLength(BufferStream stream, int length)
	{
		DemoShotParentKeyframe instance = default(DemoShotParentKeyframe);
		DeserializeLength(stream, length, ref instance, isDelta: false);
		return instance;
	}

	public static DemoShotParentKeyframe Deserialize(byte[] buffer)
	{
		DemoShotParentKeyframe instance = default(DemoShotParentKeyframe);
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

	public void WriteToStreamDelta(BufferStream stream, DemoShotParentKeyframe previous)
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

	public static DemoShotParentKeyframe Deserialize(BufferStream stream, ref DemoShotParentKeyframe instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotParentKeyframe");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyFrameParentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyFrameParentName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotParentKeyframe DeserializeLengthDelimited(BufferStream stream, ref DemoShotParentKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotParentKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyFrameParentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyFrameParentName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoShotParentKeyframe DeserializeLength(BufferStream stream, int length, ref DemoShotParentKeyframe instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoShotParentKeyframe");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyframeTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyFrameParentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				instance.keyFrameParentName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoShotParentKeyframe");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoShotParentKeyframe instance, DemoShotParentKeyframe previous)
	{
		if (instance.keyframeTime != previous.keyframeTime)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.keyFrameParentId.Value);
		if (instance.keyFrameParentName != previous.keyFrameParentName)
		{
			if (instance.keyFrameParentName == null)
			{
				throw new ArgumentNullException("keyFrameParentName", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.keyFrameParentName);
		}
	}

	public static void Serialize(BufferStream stream, DemoShotParentKeyframe instance)
	{
		if (instance.keyframeTime != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.keyframeTime);
		}
		if (instance.keyFrameParentId != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.keyFrameParentId.Value);
		}
		if (instance.keyFrameParentName == null)
		{
			throw new ArgumentNullException("keyFrameParentName", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteString(stream, instance.keyFrameParentName);
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref keyFrameParentId.Value);
	}
}
