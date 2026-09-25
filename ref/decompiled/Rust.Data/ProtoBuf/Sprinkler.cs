using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Sprinkler : IDisposable, Pool.IPooled, IProto<Sprinkler>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int currentFuelType;

	public static void ResetToPool(Sprinkler instance)
	{
		if (instance.ShouldPool)
		{
			instance.currentFuelType = 0;
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
			throw new Exception("Trying to dispose Sprinkler with ShouldPool set to false!");
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

	public void CopyTo(Sprinkler instance)
	{
		instance.currentFuelType = currentFuelType;
	}

	public Sprinkler Copy()
	{
		Sprinkler sprinkler = Pool.Get<Sprinkler>();
		CopyTo(sprinkler);
		return sprinkler;
	}

	public static Sprinkler Deserialize(BufferStream stream)
	{
		Sprinkler sprinkler = Pool.Get<Sprinkler>();
		Deserialize(stream, sprinkler, isDelta: false);
		return sprinkler;
	}

	public static Sprinkler DeserializeLengthDelimited(BufferStream stream)
	{
		Sprinkler sprinkler = Pool.Get<Sprinkler>();
		DeserializeLengthDelimited(stream, sprinkler, isDelta: false);
		return sprinkler;
	}

	public static Sprinkler DeserializeLength(BufferStream stream, int length)
	{
		Sprinkler sprinkler = Pool.Get<Sprinkler>();
		DeserializeLength(stream, length, sprinkler, isDelta: false);
		return sprinkler;
	}

	public static Sprinkler Deserialize(byte[] buffer)
	{
		Sprinkler sprinkler = Pool.Get<Sprinkler>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sprinkler, isDelta: false);
		return sprinkler;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Sprinkler previous)
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

	public static Sprinkler Deserialize(BufferStream stream, Sprinkler instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Sprinkler");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sprinkler");
				instance.currentFuelType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sprinkler");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sprinkler");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Sprinkler DeserializeLengthDelimited(BufferStream stream, Sprinkler instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Sprinkler");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sprinkler");
				instance.currentFuelType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sprinkler");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sprinkler");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Sprinkler DeserializeLength(BufferStream stream, int length, Sprinkler instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Sprinkler");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sprinkler");
				instance.currentFuelType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Sprinkler");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Sprinkler");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Sprinkler instance, Sprinkler previous)
	{
		if (instance.currentFuelType != previous.currentFuelType)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentFuelType);
		}
	}

	public static void Serialize(BufferStream stream, Sprinkler instance)
	{
		if (instance.currentFuelType != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.currentFuelType);
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
