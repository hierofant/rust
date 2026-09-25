using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TirednessAboveAIEventData : IDisposable, Pool.IPooled, IProto<TirednessAboveAIEventData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float value;

	public static void ResetToPool(TirednessAboveAIEventData instance)
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
			throw new Exception("Trying to dispose TirednessAboveAIEventData with ShouldPool set to false!");
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

	public void CopyTo(TirednessAboveAIEventData instance)
	{
		instance.value = value;
	}

	public TirednessAboveAIEventData Copy()
	{
		TirednessAboveAIEventData tirednessAboveAIEventData = Pool.Get<TirednessAboveAIEventData>();
		CopyTo(tirednessAboveAIEventData);
		return tirednessAboveAIEventData;
	}

	public static TirednessAboveAIEventData Deserialize(BufferStream stream)
	{
		TirednessAboveAIEventData tirednessAboveAIEventData = Pool.Get<TirednessAboveAIEventData>();
		Deserialize(stream, tirednessAboveAIEventData, isDelta: false);
		return tirednessAboveAIEventData;
	}

	public static TirednessAboveAIEventData DeserializeLengthDelimited(BufferStream stream)
	{
		TirednessAboveAIEventData tirednessAboveAIEventData = Pool.Get<TirednessAboveAIEventData>();
		DeserializeLengthDelimited(stream, tirednessAboveAIEventData, isDelta: false);
		return tirednessAboveAIEventData;
	}

	public static TirednessAboveAIEventData DeserializeLength(BufferStream stream, int length)
	{
		TirednessAboveAIEventData tirednessAboveAIEventData = Pool.Get<TirednessAboveAIEventData>();
		DeserializeLength(stream, length, tirednessAboveAIEventData, isDelta: false);
		return tirednessAboveAIEventData;
	}

	public static TirednessAboveAIEventData Deserialize(byte[] buffer)
	{
		TirednessAboveAIEventData tirednessAboveAIEventData = Pool.Get<TirednessAboveAIEventData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, tirednessAboveAIEventData, isDelta: false);
		return tirednessAboveAIEventData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TirednessAboveAIEventData previous)
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

	public static TirednessAboveAIEventData Deserialize(BufferStream stream, TirednessAboveAIEventData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TirednessAboveAIEventData");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TirednessAboveAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TirednessAboveAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TirednessAboveAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static TirednessAboveAIEventData DeserializeLengthDelimited(BufferStream stream, TirednessAboveAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TirednessAboveAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TirednessAboveAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TirednessAboveAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TirednessAboveAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static TirednessAboveAIEventData DeserializeLength(BufferStream stream, int length, TirednessAboveAIEventData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TirednessAboveAIEventData");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TirednessAboveAIEventData");
				instance.value = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TirednessAboveAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TirednessAboveAIEventData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TirednessAboveAIEventData instance, TirednessAboveAIEventData previous)
	{
		if (instance.value != previous.value)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.value);
		}
	}

	public static void Serialize(BufferStream stream, TirednessAboveAIEventData instance)
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
