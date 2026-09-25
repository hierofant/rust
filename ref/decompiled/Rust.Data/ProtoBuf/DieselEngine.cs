using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DieselEngine : IDisposable, Pool.IPooled, IProto<DieselEngine>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float fuelTime;

	[NonSerialized]
	public ulong startedByPlayer;

	public static void ResetToPool(DieselEngine instance)
	{
		if (instance.ShouldPool)
		{
			instance.fuelTime = 0f;
			instance.startedByPlayer = 0uL;
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
			throw new Exception("Trying to dispose DieselEngine with ShouldPool set to false!");
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

	public void CopyTo(DieselEngine instance)
	{
		instance.fuelTime = fuelTime;
		instance.startedByPlayer = startedByPlayer;
	}

	public DieselEngine Copy()
	{
		DieselEngine dieselEngine = Pool.Get<DieselEngine>();
		CopyTo(dieselEngine);
		return dieselEngine;
	}

	public static DieselEngine Deserialize(BufferStream stream)
	{
		DieselEngine dieselEngine = Pool.Get<DieselEngine>();
		Deserialize(stream, dieselEngine, isDelta: false);
		return dieselEngine;
	}

	public static DieselEngine DeserializeLengthDelimited(BufferStream stream)
	{
		DieselEngine dieselEngine = Pool.Get<DieselEngine>();
		DeserializeLengthDelimited(stream, dieselEngine, isDelta: false);
		return dieselEngine;
	}

	public static DieselEngine DeserializeLength(BufferStream stream, int length)
	{
		DieselEngine dieselEngine = Pool.Get<DieselEngine>();
		DeserializeLength(stream, length, dieselEngine, isDelta: false);
		return dieselEngine;
	}

	public static DieselEngine Deserialize(byte[] buffer)
	{
		DieselEngine dieselEngine = Pool.Get<DieselEngine>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, dieselEngine, isDelta: false);
		return dieselEngine;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DieselEngine previous)
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

	public static DieselEngine Deserialize(BufferStream stream, DieselEngine instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DieselEngine");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				instance.fuelTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				instance.startedByPlayer = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DieselEngine DeserializeLengthDelimited(BufferStream stream, DieselEngine instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DieselEngine");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				instance.fuelTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				instance.startedByPlayer = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DieselEngine DeserializeLength(BufferStream stream, int length, DieselEngine instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DieselEngine");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				instance.fuelTime = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				instance.startedByPlayer = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DieselEngine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DieselEngine instance, DieselEngine previous)
	{
		if (instance.fuelTime != previous.fuelTime)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.fuelTime);
		}
		if (instance.startedByPlayer != previous.startedByPlayer)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.startedByPlayer);
		}
	}

	public static void Serialize(BufferStream stream, DieselEngine instance)
	{
		if (instance.fuelTime != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.fuelTime);
		}
		if (instance.startedByPlayer != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.startedByPlayer);
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
