using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BoatBuildingStation : IDisposable, Pool.IPooled, IProto<BoatBuildingStation>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong ownerId;

	public static void ResetToPool(BoatBuildingStation instance)
	{
		if (instance.ShouldPool)
		{
			instance.ownerId = 0uL;
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
			throw new Exception("Trying to dispose BoatBuildingStation with ShouldPool set to false!");
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

	public void CopyTo(BoatBuildingStation instance)
	{
		instance.ownerId = ownerId;
	}

	public BoatBuildingStation Copy()
	{
		BoatBuildingStation boatBuildingStation = Pool.Get<BoatBuildingStation>();
		CopyTo(boatBuildingStation);
		return boatBuildingStation;
	}

	public static BoatBuildingStation Deserialize(BufferStream stream)
	{
		BoatBuildingStation boatBuildingStation = Pool.Get<BoatBuildingStation>();
		Deserialize(stream, boatBuildingStation, isDelta: false);
		return boatBuildingStation;
	}

	public static BoatBuildingStation DeserializeLengthDelimited(BufferStream stream)
	{
		BoatBuildingStation boatBuildingStation = Pool.Get<BoatBuildingStation>();
		DeserializeLengthDelimited(stream, boatBuildingStation, isDelta: false);
		return boatBuildingStation;
	}

	public static BoatBuildingStation DeserializeLength(BufferStream stream, int length)
	{
		BoatBuildingStation boatBuildingStation = Pool.Get<BoatBuildingStation>();
		DeserializeLength(stream, length, boatBuildingStation, isDelta: false);
		return boatBuildingStation;
	}

	public static BoatBuildingStation Deserialize(byte[] buffer)
	{
		BoatBuildingStation boatBuildingStation = Pool.Get<BoatBuildingStation>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, boatBuildingStation, isDelta: false);
		return boatBuildingStation;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BoatBuildingStation previous)
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

	public static BoatBuildingStation Deserialize(BufferStream stream, BoatBuildingStation instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BoatBuildingStation");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingStation");
				instance.ownerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingStation");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BoatBuildingStation");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BoatBuildingStation DeserializeLengthDelimited(BufferStream stream, BoatBuildingStation instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BoatBuildingStation");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingStation");
				instance.ownerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingStation");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BoatBuildingStation");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BoatBuildingStation DeserializeLength(BufferStream stream, int length, BoatBuildingStation instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BoatBuildingStation");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingStation");
				instance.ownerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingStation");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BoatBuildingStation");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BoatBuildingStation instance, BoatBuildingStation previous)
	{
		if (instance.ownerId != previous.ownerId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.ownerId);
		}
	}

	public static void Serialize(BufferStream stream, BoatBuildingStation instance)
	{
		if (instance.ownerId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.ownerId);
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
