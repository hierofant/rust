using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class EngineStorage : IDisposable, Pool.IPooled, IProto<EngineStorage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool isUsable;

	[NonSerialized]
	public float accelerationBoost;

	[NonSerialized]
	public float topSpeedBoost;

	[NonSerialized]
	public float fuelEconomyBoost;

	public static void ResetToPool(EngineStorage instance)
	{
		if (instance.ShouldPool)
		{
			instance.isUsable = false;
			instance.accelerationBoost = 0f;
			instance.topSpeedBoost = 0f;
			instance.fuelEconomyBoost = 0f;
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
			throw new Exception("Trying to dispose EngineStorage with ShouldPool set to false!");
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

	public void CopyTo(EngineStorage instance)
	{
		instance.isUsable = isUsable;
		instance.accelerationBoost = accelerationBoost;
		instance.topSpeedBoost = topSpeedBoost;
		instance.fuelEconomyBoost = fuelEconomyBoost;
	}

	public EngineStorage Copy()
	{
		EngineStorage engineStorage = Pool.Get<EngineStorage>();
		CopyTo(engineStorage);
		return engineStorage;
	}

	public static EngineStorage Deserialize(BufferStream stream)
	{
		EngineStorage engineStorage = Pool.Get<EngineStorage>();
		Deserialize(stream, engineStorage, isDelta: false);
		return engineStorage;
	}

	public static EngineStorage DeserializeLengthDelimited(BufferStream stream)
	{
		EngineStorage engineStorage = Pool.Get<EngineStorage>();
		DeserializeLengthDelimited(stream, engineStorage, isDelta: false);
		return engineStorage;
	}

	public static EngineStorage DeserializeLength(BufferStream stream, int length)
	{
		EngineStorage engineStorage = Pool.Get<EngineStorage>();
		DeserializeLength(stream, length, engineStorage, isDelta: false);
		return engineStorage;
	}

	public static EngineStorage Deserialize(byte[] buffer)
	{
		EngineStorage engineStorage = Pool.Get<EngineStorage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, engineStorage, isDelta: false);
		return engineStorage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, EngineStorage previous)
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

	public static EngineStorage Deserialize(BufferStream stream, EngineStorage instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.EngineStorage");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.isUsable = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.accelerationBoost = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.topSpeedBoost = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.fuelEconomyBoost = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static EngineStorage DeserializeLengthDelimited(BufferStream stream, EngineStorage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.EngineStorage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.isUsable = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.accelerationBoost = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.topSpeedBoost = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.fuelEconomyBoost = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static EngineStorage DeserializeLength(BufferStream stream, int length, EngineStorage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.EngineStorage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.isUsable = ProtocolParser.ReadBool(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.accelerationBoost = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.topSpeedBoost = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				instance.fuelEconomyBoost = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EngineStorage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, EngineStorage instance, EngineStorage previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.isUsable);
		if (instance.accelerationBoost != previous.accelerationBoost)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.accelerationBoost);
		}
		if (instance.topSpeedBoost != previous.topSpeedBoost)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.topSpeedBoost);
		}
		if (instance.fuelEconomyBoost != previous.fuelEconomyBoost)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.fuelEconomyBoost);
		}
	}

	public static void Serialize(BufferStream stream, EngineStorage instance)
	{
		if (instance.isUsable)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.isUsable);
		}
		if (instance.accelerationBoost != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.accelerationBoost);
		}
		if (instance.topSpeedBoost != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.topSpeedBoost);
		}
		if (instance.fuelEconomyBoost != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.fuelEconomyBoost);
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
