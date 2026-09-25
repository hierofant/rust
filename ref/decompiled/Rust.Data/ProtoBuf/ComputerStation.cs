using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ComputerStation : IDisposable, Pool.IPooled, IProto<ComputerStation>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string bookmarks;

	public static void ResetToPool(ComputerStation instance)
	{
		if (instance.ShouldPool)
		{
			instance.bookmarks = string.Empty;
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
			throw new Exception("Trying to dispose ComputerStation with ShouldPool set to false!");
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

	public void CopyTo(ComputerStation instance)
	{
		instance.bookmarks = bookmarks;
	}

	public ComputerStation Copy()
	{
		ComputerStation computerStation = Pool.Get<ComputerStation>();
		CopyTo(computerStation);
		return computerStation;
	}

	public static ComputerStation Deserialize(BufferStream stream)
	{
		ComputerStation computerStation = Pool.Get<ComputerStation>();
		Deserialize(stream, computerStation, isDelta: false);
		return computerStation;
	}

	public static ComputerStation DeserializeLengthDelimited(BufferStream stream)
	{
		ComputerStation computerStation = Pool.Get<ComputerStation>();
		DeserializeLengthDelimited(stream, computerStation, isDelta: false);
		return computerStation;
	}

	public static ComputerStation DeserializeLength(BufferStream stream, int length)
	{
		ComputerStation computerStation = Pool.Get<ComputerStation>();
		DeserializeLength(stream, length, computerStation, isDelta: false);
		return computerStation;
	}

	public static ComputerStation Deserialize(byte[] buffer)
	{
		ComputerStation computerStation = Pool.Get<ComputerStation>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, computerStation, isDelta: false);
		return computerStation;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ComputerStation previous)
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

	public static ComputerStation Deserialize(BufferStream stream, ComputerStation instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ComputerStation");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ComputerStation");
				instance.bookmarks = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ComputerStation");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ComputerStation");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ComputerStation DeserializeLengthDelimited(BufferStream stream, ComputerStation instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ComputerStation");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ComputerStation");
				instance.bookmarks = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ComputerStation");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ComputerStation");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ComputerStation DeserializeLength(BufferStream stream, int length, ComputerStation instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ComputerStation");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ComputerStation");
				instance.bookmarks = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ComputerStation");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ComputerStation");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ComputerStation instance, ComputerStation previous)
	{
		if (instance.bookmarks != null && instance.bookmarks != previous.bookmarks)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.bookmarks);
		}
	}

	public static void Serialize(BufferStream stream, ComputerStation instance)
	{
		if (instance.bookmarks != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.bookmarks);
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
