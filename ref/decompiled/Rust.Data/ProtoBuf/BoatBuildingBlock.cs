using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BoatBuildingBlock : IDisposable, Pool.IPooled, IProto<BoatBuildingBlock>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float damageTaken;

	public static void ResetToPool(BoatBuildingBlock instance)
	{
		if (instance.ShouldPool)
		{
			instance.damageTaken = 0f;
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
			throw new Exception("Trying to dispose BoatBuildingBlock with ShouldPool set to false!");
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

	public void CopyTo(BoatBuildingBlock instance)
	{
		instance.damageTaken = damageTaken;
	}

	public BoatBuildingBlock Copy()
	{
		BoatBuildingBlock boatBuildingBlock = Pool.Get<BoatBuildingBlock>();
		CopyTo(boatBuildingBlock);
		return boatBuildingBlock;
	}

	public static BoatBuildingBlock Deserialize(BufferStream stream)
	{
		BoatBuildingBlock boatBuildingBlock = Pool.Get<BoatBuildingBlock>();
		Deserialize(stream, boatBuildingBlock, isDelta: false);
		return boatBuildingBlock;
	}

	public static BoatBuildingBlock DeserializeLengthDelimited(BufferStream stream)
	{
		BoatBuildingBlock boatBuildingBlock = Pool.Get<BoatBuildingBlock>();
		DeserializeLengthDelimited(stream, boatBuildingBlock, isDelta: false);
		return boatBuildingBlock;
	}

	public static BoatBuildingBlock DeserializeLength(BufferStream stream, int length)
	{
		BoatBuildingBlock boatBuildingBlock = Pool.Get<BoatBuildingBlock>();
		DeserializeLength(stream, length, boatBuildingBlock, isDelta: false);
		return boatBuildingBlock;
	}

	public static BoatBuildingBlock Deserialize(byte[] buffer)
	{
		BoatBuildingBlock boatBuildingBlock = Pool.Get<BoatBuildingBlock>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, boatBuildingBlock, isDelta: false);
		return boatBuildingBlock;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BoatBuildingBlock previous)
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

	public static BoatBuildingBlock Deserialize(BufferStream stream, BoatBuildingBlock instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BoatBuildingBlock");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingBlock");
				instance.damageTaken = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingBlock");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BoatBuildingBlock");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BoatBuildingBlock DeserializeLengthDelimited(BufferStream stream, BoatBuildingBlock instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BoatBuildingBlock");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingBlock");
				instance.damageTaken = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingBlock");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BoatBuildingBlock");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BoatBuildingBlock DeserializeLength(BufferStream stream, int length, BoatBuildingBlock instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BoatBuildingBlock");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingBlock");
				instance.damageTaken = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BoatBuildingBlock");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BoatBuildingBlock");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BoatBuildingBlock instance, BoatBuildingBlock previous)
	{
		if (instance.damageTaken != previous.damageTaken)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.damageTaken);
		}
	}

	public static void Serialize(BufferStream stream, BoatBuildingBlock instance)
	{
		if (instance.damageTaken != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.damageTaken);
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
