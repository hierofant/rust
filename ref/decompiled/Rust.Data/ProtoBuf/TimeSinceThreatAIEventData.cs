using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TimeSinceThreatAIEventData : IDisposable, Pool.IPooled, IProto<TimeSinceThreatAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float value;

	public static void ResetToPool(TimeSinceThreatAIEventData instance)
	{
		if (instance.ShouldPool)
		{
			instance.value = 0f;
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
			throw new Exception("Trying to dispose TimeSinceThreatAIEventData with ShouldPool set to false!");
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

	public void CopyTo(TimeSinceThreatAIEventData instance)
	{
		instance.value = value;
	}

	public TimeSinceThreatAIEventData Copy()
	{
		TimeSinceThreatAIEventData timeSinceThreatAIEventData = Pool.Get<TimeSinceThreatAIEventData>();
		CopyTo(timeSinceThreatAIEventData);
		return timeSinceThreatAIEventData;
	}

	public static TimeSinceThreatAIEventData Deserialize(BufferStream stream)
	{
		TimeSinceThreatAIEventData timeSinceThreatAIEventData = Pool.Get<TimeSinceThreatAIEventData>();
		Deserialize(stream, timeSinceThreatAIEventData, isDelta: false);
		return timeSinceThreatAIEventData;
	}

	public static TimeSinceThreatAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		TimeSinceThreatAIEventData timeSinceThreatAIEventData = Pool.Get<TimeSinceThreatAIEventData>();
		DeserializeLengthDelimited(stream, timeSinceThreatAIEventData, isDelta: false);
		return timeSinceThreatAIEventData;
	}

	public static TimeSinceThreatAIEventData DeserializeLength(BufferStream stream, int length)
	{
		TimeSinceThreatAIEventData timeSinceThreatAIEventData = Pool.Get<TimeSinceThreatAIEventData>();
		DeserializeLength(stream, length, timeSinceThreatAIEventData, isDelta: false);
		return timeSinceThreatAIEventData;
	}

	public static TimeSinceThreatAIEventData Deserialize(byte[] buffer)
	{
		TimeSinceThreatAIEventData timeSinceThreatAIEventData = Pool.Get<TimeSinceThreatAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, timeSinceThreatAIEventData, isDelta: false);
		return timeSinceThreatAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TimeSinceThreatAIEventData previous)
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

	public static TimeSinceThreatAIEventData Deserialize(BufferStream stream, TimeSinceThreatAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TimeSinceThreatAIEventData");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimeSinceThreatAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimeSinceThreatAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimeSinceThreatAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static TimeSinceThreatAIEventData DeserializeLengthDelimited(BufferStream stream, TimeSinceThreatAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TimeSinceThreatAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimeSinceThreatAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimeSinceThreatAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimeSinceThreatAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static TimeSinceThreatAIEventData DeserializeLength(BufferStream stream, int length, TimeSinceThreatAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TimeSinceThreatAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimeSinceThreatAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimeSinceThreatAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimeSinceThreatAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TimeSinceThreatAIEventData instance, TimeSinceThreatAIEventData previous)
	{
		if (instance.value != previous.value)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.value);
		}
	}

	public static void Serialize(BufferStream stream, TimeSinceThreatAIEventData instance)
	{
		if (instance.value != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.value);
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
