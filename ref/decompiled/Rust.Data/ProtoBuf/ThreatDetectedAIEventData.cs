using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ThreatDetectedAIEventData : IDisposable, Pool.IPooled, IProto<ThreatDetectedAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float range;

	public static void ResetToPool(ThreatDetectedAIEventData instance)
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
			throw new Exception("Trying to dispose ThreatDetectedAIEventData with ShouldPool set to false!");
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

	public void CopyTo(ThreatDetectedAIEventData instance)
	{
		instance.range = range;
	}

	public ThreatDetectedAIEventData Copy()
	{
		ThreatDetectedAIEventData threatDetectedAIEventData = Pool.Get<ThreatDetectedAIEventData>();
		CopyTo(threatDetectedAIEventData);
		return threatDetectedAIEventData;
	}

	public static ThreatDetectedAIEventData Deserialize(BufferStream stream)
	{
		ThreatDetectedAIEventData threatDetectedAIEventData = Pool.Get<ThreatDetectedAIEventData>();
		Deserialize(stream, threatDetectedAIEventData, isDelta: false);
		return threatDetectedAIEventData;
	}

	public static ThreatDetectedAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		ThreatDetectedAIEventData threatDetectedAIEventData = Pool.Get<ThreatDetectedAIEventData>();
		DeserializeLengthDelimited(stream, threatDetectedAIEventData, isDelta: false);
		return threatDetectedAIEventData;
	}

	public static ThreatDetectedAIEventData DeserializeLength(BufferStream stream, int length)
	{
		ThreatDetectedAIEventData threatDetectedAIEventData = Pool.Get<ThreatDetectedAIEventData>();
		DeserializeLength(stream, length, threatDetectedAIEventData, isDelta: false);
		return threatDetectedAIEventData;
	}

	public static ThreatDetectedAIEventData Deserialize(byte[] buffer)
	{
		ThreatDetectedAIEventData threatDetectedAIEventData = Pool.Get<ThreatDetectedAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, threatDetectedAIEventData, isDelta: false);
		return threatDetectedAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ThreatDetectedAIEventData previous)
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

	public static ThreatDetectedAIEventData Deserialize(BufferStream stream, ThreatDetectedAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ThreatDetectedAIEventData");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ThreatDetectedAIEventData");
				instance.range = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ThreatDetectedAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ThreatDetectedAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ThreatDetectedAIEventData DeserializeLengthDelimited(BufferStream stream, ThreatDetectedAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ThreatDetectedAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ThreatDetectedAIEventData");
				instance.range = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ThreatDetectedAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ThreatDetectedAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ThreatDetectedAIEventData DeserializeLength(BufferStream stream, int length, ThreatDetectedAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ThreatDetectedAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ThreatDetectedAIEventData");
				instance.range = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ThreatDetectedAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ThreatDetectedAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ThreatDetectedAIEventData instance, ThreatDetectedAIEventData previous)
	{
		if (instance.range != previous.range)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.range);
		}
	}

	public static void Serialize(BufferStream stream, ThreatDetectedAIEventData instance)
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
