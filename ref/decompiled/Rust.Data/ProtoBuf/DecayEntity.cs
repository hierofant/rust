using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DecayEntity : IDisposable, Pool.IPooled, IProto<DecayEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float decayTimer;

	[NonSerialized]
	public uint buildingID;

	[NonSerialized]
	public float upkeepTimer;

	public static void ResetToPool(DecayEntity instance)
	{
		if (instance.ShouldPool)
		{
			instance.decayTimer = 0f;
			instance.buildingID = 0u;
			instance.upkeepTimer = 0f;
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
			throw new Exception("Trying to dispose DecayEntity with ShouldPool set to false!");
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

	public void CopyTo(DecayEntity instance)
	{
		instance.decayTimer = decayTimer;
		instance.buildingID = buildingID;
		instance.upkeepTimer = upkeepTimer;
	}

	public DecayEntity Copy()
	{
		DecayEntity decayEntity = Pool.Get<DecayEntity>();
		CopyTo(decayEntity);
		return decayEntity;
	}

	public static DecayEntity Deserialize(BufferStream stream)
	{
		DecayEntity decayEntity = Pool.Get<DecayEntity>();
		Deserialize(stream, decayEntity, isDelta: false);
		return decayEntity;
	}

	public static DecayEntity DeserializeLengthDelimited(BufferStream stream)
	{
		DecayEntity decayEntity = Pool.Get<DecayEntity>();
		DeserializeLengthDelimited(stream, decayEntity, isDelta: false);
		return decayEntity;
	}

	public static DecayEntity DeserializeLength(BufferStream stream, int length)
	{
		DecayEntity decayEntity = Pool.Get<DecayEntity>();
		DeserializeLength(stream, length, decayEntity, isDelta: false);
		return decayEntity;
	}

	public static DecayEntity Deserialize(byte[] buffer)
	{
		DecayEntity decayEntity = Pool.Get<DecayEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, decayEntity, isDelta: false);
		return decayEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DecayEntity previous)
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

	public static DecayEntity Deserialize(BufferStream stream, DecayEntity instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DecayEntity");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.decayTimer = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.buildingID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.upkeepTimer = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DecayEntity DeserializeLengthDelimited(BufferStream stream, DecayEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DecayEntity");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.decayTimer = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.buildingID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.upkeepTimer = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DecayEntity DeserializeLength(BufferStream stream, int length, DecayEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DecayEntity");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.decayTimer = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.buildingID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				instance.upkeepTimer = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DecayEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DecayEntity instance, DecayEntity previous)
	{
		if (instance.decayTimer != previous.decayTimer)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.decayTimer);
		}
		if (instance.buildingID != previous.buildingID)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.buildingID);
		}
		if (instance.upkeepTimer != previous.upkeepTimer)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.upkeepTimer);
		}
	}

	public static void Serialize(BufferStream stream, DecayEntity instance)
	{
		if (instance.decayTimer != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.decayTimer);
		}
		if (instance.buildingID != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.buildingID);
		}
		if (instance.upkeepTimer != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.upkeepTimer);
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
