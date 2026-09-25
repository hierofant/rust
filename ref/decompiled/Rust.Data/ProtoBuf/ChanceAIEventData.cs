using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ChanceAIEventData : IDisposable, Pool.IPooled, IProto<ChanceAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float value;

	public static void ResetToPool(ChanceAIEventData instance)
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
			throw new Exception("Trying to dispose ChanceAIEventData with ShouldPool set to false!");
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

	public void CopyTo(ChanceAIEventData instance)
	{
		instance.value = value;
	}

	public ChanceAIEventData Copy()
	{
		ChanceAIEventData chanceAIEventData = Pool.Get<ChanceAIEventData>();
		CopyTo(chanceAIEventData);
		return chanceAIEventData;
	}

	public static ChanceAIEventData Deserialize(BufferStream stream)
	{
		ChanceAIEventData chanceAIEventData = Pool.Get<ChanceAIEventData>();
		Deserialize(stream, chanceAIEventData, isDelta: false);
		return chanceAIEventData;
	}

	public static ChanceAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		ChanceAIEventData chanceAIEventData = Pool.Get<ChanceAIEventData>();
		DeserializeLengthDelimited(stream, chanceAIEventData, isDelta: false);
		return chanceAIEventData;
	}

	public static ChanceAIEventData DeserializeLength(BufferStream stream, int length)
	{
		ChanceAIEventData chanceAIEventData = Pool.Get<ChanceAIEventData>();
		DeserializeLength(stream, length, chanceAIEventData, isDelta: false);
		return chanceAIEventData;
	}

	public static ChanceAIEventData Deserialize(byte[] buffer)
	{
		ChanceAIEventData chanceAIEventData = Pool.Get<ChanceAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, chanceAIEventData, isDelta: false);
		return chanceAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ChanceAIEventData previous)
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

	public static ChanceAIEventData Deserialize(BufferStream stream, ChanceAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ChanceAIEventData");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChanceAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChanceAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChanceAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ChanceAIEventData DeserializeLengthDelimited(BufferStream stream, ChanceAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ChanceAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChanceAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChanceAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChanceAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ChanceAIEventData DeserializeLength(BufferStream stream, int length, ChanceAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ChanceAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChanceAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChanceAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChanceAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ChanceAIEventData instance, ChanceAIEventData previous)
	{
		if (instance.value != previous.value)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.value);
		}
	}

	public static void Serialize(BufferStream stream, ChanceAIEventData instance)
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
