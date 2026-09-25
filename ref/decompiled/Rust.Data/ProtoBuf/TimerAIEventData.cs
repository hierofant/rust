using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TimerAIEventData : IDisposable, Pool.IPooled, IProto<TimerAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float duration;

	[NonSerialized]
	public float durationMax;

	public static void ResetToPool(TimerAIEventData instance)
	{
		if (instance.ShouldPool)
		{
			instance.duration = 0f;
			instance.durationMax = 0f;
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
			throw new Exception("Trying to dispose TimerAIEventData with ShouldPool set to false!");
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

	public void CopyTo(TimerAIEventData instance)
	{
		instance.duration = duration;
		instance.durationMax = durationMax;
	}

	public TimerAIEventData Copy()
	{
		TimerAIEventData timerAIEventData = Pool.Get<TimerAIEventData>();
		CopyTo(timerAIEventData);
		return timerAIEventData;
	}

	public static TimerAIEventData Deserialize(BufferStream stream)
	{
		TimerAIEventData timerAIEventData = Pool.Get<TimerAIEventData>();
		Deserialize(stream, timerAIEventData, isDelta: false);
		return timerAIEventData;
	}

	public static TimerAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		TimerAIEventData timerAIEventData = Pool.Get<TimerAIEventData>();
		DeserializeLengthDelimited(stream, timerAIEventData, isDelta: false);
		return timerAIEventData;
	}

	public static TimerAIEventData DeserializeLength(BufferStream stream, int length)
	{
		TimerAIEventData timerAIEventData = Pool.Get<TimerAIEventData>();
		DeserializeLength(stream, length, timerAIEventData, isDelta: false);
		return timerAIEventData;
	}

	public static TimerAIEventData Deserialize(byte[] buffer)
	{
		TimerAIEventData timerAIEventData = Pool.Get<TimerAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, timerAIEventData, isDelta: false);
		return timerAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TimerAIEventData previous)
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

	public static TimerAIEventData Deserialize(BufferStream stream, TimerAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TimerAIEventData");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				instance.duration = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				instance.durationMax = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TimerAIEventData DeserializeLengthDelimited(BufferStream stream, TimerAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TimerAIEventData");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				instance.duration = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				instance.durationMax = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TimerAIEventData DeserializeLength(BufferStream stream, int length, TimerAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TimerAIEventData");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				instance.duration = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				instance.durationMax = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimerAIEventData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TimerAIEventData instance, TimerAIEventData previous)
	{
		if (instance.duration != previous.duration)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.duration);
		}
		if (instance.durationMax != previous.durationMax)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.durationMax);
		}
	}

	public static void Serialize(BufferStream stream, TimerAIEventData instance)
	{
		if (instance.duration != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.duration);
		}
		if (instance.durationMax != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.durationMax);
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
