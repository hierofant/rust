using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SimpleUInt : IDisposable, Pool.IPooled, IProto<SimpleUInt>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint value;

	public static void ResetToPool(SimpleUInt instance)
	{
		if (instance.ShouldPool)
		{
			instance.value = 0u;
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
			throw new Exception("Trying to dispose SimpleUInt with ShouldPool set to false!");
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

	public void CopyTo(SimpleUInt instance)
	{
		instance.value = value;
	}

	public SimpleUInt Copy()
	{
		SimpleUInt simpleUInt = Pool.Get<SimpleUInt>();
		CopyTo(simpleUInt);
		return simpleUInt;
	}

	public static SimpleUInt Deserialize(BufferStream stream)
	{
		SimpleUInt simpleUInt = Pool.Get<SimpleUInt>();
		Deserialize(stream, simpleUInt, isDelta: false);
		return simpleUInt;
	}

	public static SimpleUInt DeserializeLengthDelimited(BufferStream stream)
	{
		SimpleUInt simpleUInt = Pool.Get<SimpleUInt>();
		DeserializeLengthDelimited(stream, simpleUInt, isDelta: false);
		return simpleUInt;
	}

	public static SimpleUInt DeserializeLength(BufferStream stream, int length)
	{
		SimpleUInt simpleUInt = Pool.Get<SimpleUInt>();
		DeserializeLength(stream, length, simpleUInt, isDelta: false);
		return simpleUInt;
	}

	public static SimpleUInt Deserialize(byte[] buffer)
	{
		SimpleUInt simpleUInt = Pool.Get<SimpleUInt>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, simpleUInt, isDelta: false);
		return simpleUInt;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SimpleUInt previous)
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

	public static SimpleUInt Deserialize(BufferStream stream, SimpleUInt instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SimpleUInt");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SimpleUInt");
				instance.value = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SimpleUInt");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SimpleUInt");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SimpleUInt DeserializeLengthDelimited(BufferStream stream, SimpleUInt instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SimpleUInt");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SimpleUInt");
				instance.value = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SimpleUInt");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SimpleUInt");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SimpleUInt DeserializeLength(BufferStream stream, int length, SimpleUInt instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SimpleUInt");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SimpleUInt");
				instance.value = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SimpleUInt");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SimpleUInt");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SimpleUInt instance, SimpleUInt previous)
	{
		if (instance.value != previous.value)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.value);
		}
	}

	public static void Serialize(BufferStream stream, SimpleUInt instance)
	{
		if (instance.value != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.value);
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
