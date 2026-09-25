using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CargoShipContainer : IDisposable, Pool.IPooled, IProto<CargoShipContainer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int dressingVariant;

	public static void ResetToPool(CargoShipContainer instance)
	{
		if (instance.ShouldPool)
		{
			instance.dressingVariant = 0;
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
			throw new Exception("Trying to dispose CargoShipContainer with ShouldPool set to false!");
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

	public void CopyTo(CargoShipContainer instance)
	{
		instance.dressingVariant = dressingVariant;
	}

	public CargoShipContainer Copy()
	{
		CargoShipContainer cargoShipContainer = Pool.Get<CargoShipContainer>();
		CopyTo(cargoShipContainer);
		return cargoShipContainer;
	}

	public static CargoShipContainer Deserialize(BufferStream stream)
	{
		CargoShipContainer cargoShipContainer = Pool.Get<CargoShipContainer>();
		Deserialize(stream, cargoShipContainer, isDelta: false);
		return cargoShipContainer;
	}

	public static CargoShipContainer DeserializeLengthDelimited(BufferStream stream)
	{
		CargoShipContainer cargoShipContainer = Pool.Get<CargoShipContainer>();
		DeserializeLengthDelimited(stream, cargoShipContainer, isDelta: false);
		return cargoShipContainer;
	}

	public static CargoShipContainer DeserializeLength(BufferStream stream, int length)
	{
		CargoShipContainer cargoShipContainer = Pool.Get<CargoShipContainer>();
		DeserializeLength(stream, length, cargoShipContainer, isDelta: false);
		return cargoShipContainer;
	}

	public static CargoShipContainer Deserialize(byte[] buffer)
	{
		CargoShipContainer cargoShipContainer = Pool.Get<CargoShipContainer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, cargoShipContainer, isDelta: false);
		return cargoShipContainer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CargoShipContainer previous)
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

	public static CargoShipContainer Deserialize(BufferStream stream, CargoShipContainer instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CargoShipContainer");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoShipContainer");
				instance.dressingVariant = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoShipContainer");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CargoShipContainer");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CargoShipContainer DeserializeLengthDelimited(BufferStream stream, CargoShipContainer instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CargoShipContainer");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoShipContainer");
				instance.dressingVariant = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoShipContainer");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CargoShipContainer");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CargoShipContainer DeserializeLength(BufferStream stream, int length, CargoShipContainer instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CargoShipContainer");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoShipContainer");
				instance.dressingVariant = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoShipContainer");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CargoShipContainer");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CargoShipContainer instance, CargoShipContainer previous)
	{
		if (instance.dressingVariant != previous.dressingVariant)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.dressingVariant);
		}
	}

	public static void Serialize(BufferStream stream, CargoShipContainer instance)
	{
		if (instance.dressingVariant != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.dressingVariant);
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
