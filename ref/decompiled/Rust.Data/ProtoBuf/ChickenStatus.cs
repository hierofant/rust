using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ChickenStatus : IDisposable, Pool.IPooled, IProto<ChickenStatus>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId spawnedChicken;

	[NonSerialized]
	public float timeUntilHatch;

	public static void ResetToPool(ChickenStatus instance)
	{
		if (instance.ShouldPool)
		{
			instance.spawnedChicken = default(NetworkableId);
			instance.timeUntilHatch = 0f;
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
			throw new Exception("Trying to dispose ChickenStatus with ShouldPool set to false!");
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

	public void CopyTo(ChickenStatus instance)
	{
		instance.spawnedChicken = spawnedChicken;
		instance.timeUntilHatch = timeUntilHatch;
	}

	public ChickenStatus Copy()
	{
		ChickenStatus chickenStatus = Pool.Get<ChickenStatus>();
		CopyTo(chickenStatus);
		return chickenStatus;
	}

	public static ChickenStatus Deserialize(BufferStream stream)
	{
		ChickenStatus chickenStatus = Pool.Get<ChickenStatus>();
		Deserialize(stream, chickenStatus, isDelta: false);
		return chickenStatus;
	}

	public static ChickenStatus DeserializeLengthDelimited(BufferStream stream)
	{
		ChickenStatus chickenStatus = Pool.Get<ChickenStatus>();
		DeserializeLengthDelimited(stream, chickenStatus, isDelta: false);
		return chickenStatus;
	}

	public static ChickenStatus DeserializeLength(BufferStream stream, int length)
	{
		ChickenStatus chickenStatus = Pool.Get<ChickenStatus>();
		DeserializeLength(stream, length, chickenStatus, isDelta: false);
		return chickenStatus;
	}

	public static ChickenStatus Deserialize(byte[] buffer)
	{
		ChickenStatus chickenStatus = Pool.Get<ChickenStatus>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, chickenStatus, isDelta: false);
		return chickenStatus;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ChickenStatus previous)
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

	public static ChickenStatus Deserialize(BufferStream stream, ChickenStatus instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ChickenStatus");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				instance.spawnedChicken = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				instance.timeUntilHatch = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ChickenStatus DeserializeLengthDelimited(BufferStream stream, ChickenStatus instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ChickenStatus");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				instance.spawnedChicken = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				instance.timeUntilHatch = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ChickenStatus DeserializeLength(BufferStream stream, int length, ChickenStatus instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ChickenStatus");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				instance.spawnedChicken = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				instance.timeUntilHatch = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ChickenStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ChickenStatus instance, ChickenStatus previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.spawnedChicken.Value);
		if (instance.timeUntilHatch != previous.timeUntilHatch)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.timeUntilHatch);
		}
	}

	public static void Serialize(BufferStream stream, ChickenStatus instance)
	{
		if (instance.spawnedChicken != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.spawnedChicken.Value);
		}
		if (instance.timeUntilHatch != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.timeUntilHatch);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref spawnedChicken.Value);
	}
}
