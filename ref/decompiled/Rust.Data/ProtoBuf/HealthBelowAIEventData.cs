using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class HealthBelowAIEventData : IDisposable, Pool.IPooled, IProto<HealthBelowAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float healthFraction;

	public static void ResetToPool(HealthBelowAIEventData instance)
	{
		if (instance.ShouldPool)
		{
			instance.healthFraction = 0f;
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
			throw new Exception("Trying to dispose HealthBelowAIEventData with ShouldPool set to false!");
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

	public void CopyTo(HealthBelowAIEventData instance)
	{
		instance.healthFraction = healthFraction;
	}

	public HealthBelowAIEventData Copy()
	{
		HealthBelowAIEventData healthBelowAIEventData = Pool.Get<HealthBelowAIEventData>();
		CopyTo(healthBelowAIEventData);
		return healthBelowAIEventData;
	}

	public static HealthBelowAIEventData Deserialize(BufferStream stream)
	{
		HealthBelowAIEventData healthBelowAIEventData = Pool.Get<HealthBelowAIEventData>();
		Deserialize(stream, healthBelowAIEventData, isDelta: false);
		return healthBelowAIEventData;
	}

	public static HealthBelowAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		HealthBelowAIEventData healthBelowAIEventData = Pool.Get<HealthBelowAIEventData>();
		DeserializeLengthDelimited(stream, healthBelowAIEventData, isDelta: false);
		return healthBelowAIEventData;
	}

	public static HealthBelowAIEventData DeserializeLength(BufferStream stream, int length)
	{
		HealthBelowAIEventData healthBelowAIEventData = Pool.Get<HealthBelowAIEventData>();
		DeserializeLength(stream, length, healthBelowAIEventData, isDelta: false);
		return healthBelowAIEventData;
	}

	public static HealthBelowAIEventData Deserialize(byte[] buffer)
	{
		HealthBelowAIEventData healthBelowAIEventData = Pool.Get<HealthBelowAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, healthBelowAIEventData, isDelta: false);
		return healthBelowAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, HealthBelowAIEventData previous)
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

	public static HealthBelowAIEventData Deserialize(BufferStream stream, HealthBelowAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.HealthBelowAIEventData");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HealthBelowAIEventData");
				instance.healthFraction = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HealthBelowAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HealthBelowAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static HealthBelowAIEventData DeserializeLengthDelimited(BufferStream stream, HealthBelowAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HealthBelowAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HealthBelowAIEventData");
				instance.healthFraction = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HealthBelowAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HealthBelowAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static HealthBelowAIEventData DeserializeLength(BufferStream stream, int length, HealthBelowAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HealthBelowAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HealthBelowAIEventData");
				instance.healthFraction = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HealthBelowAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HealthBelowAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, HealthBelowAIEventData instance, HealthBelowAIEventData previous)
	{
		if (instance.healthFraction != previous.healthFraction)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.healthFraction);
		}
	}

	public static void Serialize(BufferStream stream, HealthBelowAIEventData instance)
	{
		if (instance.healthFraction != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.healthFraction);
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
