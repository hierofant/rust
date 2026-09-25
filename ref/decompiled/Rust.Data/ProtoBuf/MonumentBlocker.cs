using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MonumentBlocker : IDisposable, Pool.IPooled, IProto<MonumentBlocker>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float gracePeriodTimer;

	public static void ResetToPool(MonumentBlocker instance)
	{
		if (instance.ShouldPool)
		{
			instance.gracePeriodTimer = 0f;
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
			throw new Exception("Trying to dispose MonumentBlocker with ShouldPool set to false!");
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

	public void CopyTo(MonumentBlocker instance)
	{
		instance.gracePeriodTimer = gracePeriodTimer;
	}

	public MonumentBlocker Copy()
	{
		MonumentBlocker monumentBlocker = Pool.Get<MonumentBlocker>();
		CopyTo(monumentBlocker);
		return monumentBlocker;
	}

	public static MonumentBlocker Deserialize(BufferStream stream)
	{
		MonumentBlocker monumentBlocker = Pool.Get<MonumentBlocker>();
		Deserialize(stream, monumentBlocker, isDelta: false);
		return monumentBlocker;
	}

	public static MonumentBlocker DeserializeLengthDelimited(BufferStream stream)
	{
		MonumentBlocker monumentBlocker = Pool.Get<MonumentBlocker>();
		DeserializeLengthDelimited(stream, monumentBlocker, isDelta: false);
		return monumentBlocker;
	}

	public static MonumentBlocker DeserializeLength(BufferStream stream, int length)
	{
		MonumentBlocker monumentBlocker = Pool.Get<MonumentBlocker>();
		DeserializeLength(stream, length, monumentBlocker, isDelta: false);
		return monumentBlocker;
	}

	public static MonumentBlocker Deserialize(byte[] buffer)
	{
		MonumentBlocker monumentBlocker = Pool.Get<MonumentBlocker>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, monumentBlocker, isDelta: false);
		return monumentBlocker;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MonumentBlocker previous)
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

	public static MonumentBlocker Deserialize(BufferStream stream, MonumentBlocker instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MonumentBlocker");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MonumentBlocker");
				instance.gracePeriodTimer = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MonumentBlocker");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MonumentBlocker");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static MonumentBlocker DeserializeLengthDelimited(BufferStream stream, MonumentBlocker instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MonumentBlocker");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MonumentBlocker");
				instance.gracePeriodTimer = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MonumentBlocker");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MonumentBlocker");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static MonumentBlocker DeserializeLength(BufferStream stream, int length, MonumentBlocker instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MonumentBlocker");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MonumentBlocker");
				instance.gracePeriodTimer = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MonumentBlocker");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MonumentBlocker");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MonumentBlocker instance, MonumentBlocker previous)
	{
		if (instance.gracePeriodTimer != previous.gracePeriodTimer)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.gracePeriodTimer);
		}
	}

	public static void Serialize(BufferStream stream, MonumentBlocker instance)
	{
		if (instance.gracePeriodTimer != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.gracePeriodTimer);
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
