using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class InRangeAIEventData : IDisposable, Pool.IPooled, IProto<InRangeAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float range;

	public static void ResetToPool(InRangeAIEventData instance)
	{
		if (instance.ShouldPool)
		{
			instance.range = 0f;
			Pool.Free(ref instance);
		}
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose InRangeAIEventData with ShouldPool set to false!");
		}
		if (!_disposed)
		{
			ResetToPool();
			_disposed = true;
		}
	}

	public virtual void EnterPool()
	{
		_disposed = true;
	}

	public virtual void LeavePool()
	{
		_disposed = false;
	}

	public void CopyTo(InRangeAIEventData instance)
	{
		instance.range = range;
	}

	public InRangeAIEventData Copy()
	{
		InRangeAIEventData inRangeAIEventData = Pool.Get<InRangeAIEventData>();
		CopyTo(inRangeAIEventData);
		return inRangeAIEventData;
	}

	public static InRangeAIEventData Deserialize(BufferStream stream)
	{
		InRangeAIEventData inRangeAIEventData = Pool.Get<InRangeAIEventData>();
		Deserialize(stream, inRangeAIEventData, isDelta: false);
		return inRangeAIEventData;
	}

	public static InRangeAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		InRangeAIEventData inRangeAIEventData = Pool.Get<InRangeAIEventData>();
		DeserializeLengthDelimited(stream, inRangeAIEventData, isDelta: false);
		return inRangeAIEventData;
	}

	public static InRangeAIEventData DeserializeLength(BufferStream stream, int length)
	{
		InRangeAIEventData inRangeAIEventData = Pool.Get<InRangeAIEventData>();
		DeserializeLength(stream, length, inRangeAIEventData, isDelta: false);
		return inRangeAIEventData;
	}

	public static InRangeAIEventData Deserialize(byte[] buffer)
	{
		InRangeAIEventData inRangeAIEventData = Pool.Get<InRangeAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, inRangeAIEventData, isDelta: false);
		return inRangeAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, InRangeAIEventData previous)
	{
		if (previous == null)
		{
			Serialize(stream, this);
		}
		else
		{
			SerializeDelta(stream, this, previous);
		}
	}

	public virtual void ReadFromStream(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void ReadFromStream(BufferStream stream, int size, bool isDelta = false)
	{
		DeserializeLength(stream, size, this, isDelta);
	}

	public static InRangeAIEventData Deserialize(BufferStream stream, InRangeAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.InRangeAIEventData");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.InRangeAIEventData");
				instance.range = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.InRangeAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InRangeAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static InRangeAIEventData DeserializeLengthDelimited(BufferStream stream, InRangeAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.InRangeAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.InRangeAIEventData");
				instance.range = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.InRangeAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InRangeAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static InRangeAIEventData DeserializeLength(BufferStream stream, int length, InRangeAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.InRangeAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.InRangeAIEventData");
				instance.range = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.InRangeAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.InRangeAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, InRangeAIEventData instance, InRangeAIEventData previous)
	{
		if (instance.range != previous.range)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.range);
		}
	}

	public static void Serialize(BufferStream stream, InRangeAIEventData instance)
	{
		if (instance.range != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.range);
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
