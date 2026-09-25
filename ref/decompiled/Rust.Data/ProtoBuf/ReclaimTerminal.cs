using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ReclaimTerminal : IDisposable, Pool.IPooled, IProto<ReclaimTerminal>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int itemCount;

	public static void ResetToPool(ReclaimTerminal instance)
	{
		if (instance.ShouldPool)
		{
			instance.itemCount = 0;
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
			throw new Exception("Trying to dispose ReclaimTerminal with ShouldPool set to false!");
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

	public void CopyTo(ReclaimTerminal instance)
	{
		instance.itemCount = itemCount;
	}

	public ReclaimTerminal Copy()
	{
		ReclaimTerminal reclaimTerminal = Pool.Get<ReclaimTerminal>();
		CopyTo(reclaimTerminal);
		return reclaimTerminal;
	}

	public static ReclaimTerminal Deserialize(BufferStream stream)
	{
		ReclaimTerminal reclaimTerminal = Pool.Get<ReclaimTerminal>();
		Deserialize(stream, reclaimTerminal, isDelta: false);
		return reclaimTerminal;
	}

	public static ReclaimTerminal DeserializeLengthDelimited(BufferStream stream)
	{
		ReclaimTerminal reclaimTerminal = Pool.Get<ReclaimTerminal>();
		DeserializeLengthDelimited(stream, reclaimTerminal, isDelta: false);
		return reclaimTerminal;
	}

	public static ReclaimTerminal DeserializeLength(BufferStream stream, int length)
	{
		ReclaimTerminal reclaimTerminal = Pool.Get<ReclaimTerminal>();
		DeserializeLength(stream, length, reclaimTerminal, isDelta: false);
		return reclaimTerminal;
	}

	public static ReclaimTerminal Deserialize(byte[] buffer)
	{
		ReclaimTerminal reclaimTerminal = Pool.Get<ReclaimTerminal>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, reclaimTerminal, isDelta: false);
		return reclaimTerminal;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ReclaimTerminal previous)
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

	public static ReclaimTerminal Deserialize(BufferStream stream, ReclaimTerminal instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ReclaimTerminal");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimTerminal");
				instance.itemCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ReclaimTerminal DeserializeLengthDelimited(BufferStream stream, ReclaimTerminal instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ReclaimTerminal");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimTerminal");
				instance.itemCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ReclaimTerminal DeserializeLength(BufferStream stream, int length, ReclaimTerminal instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ReclaimTerminal");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimTerminal");
				instance.itemCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ReclaimTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ReclaimTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ReclaimTerminal instance, ReclaimTerminal previous)
	{
		if (instance.itemCount != previous.itemCount)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemCount);
		}
	}

	public static void Serialize(BufferStream stream, ReclaimTerminal instance)
	{
		if (instance.itemCount != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemCount);
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
