using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PersistentMissionEntityData : IDisposable, Pool.IPooled, IProto<PersistentMissionEntityData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId entityID;

	public static void ResetToPool(PersistentMissionEntityData instance)
	{
		if (instance.ShouldPool)
		{
			instance.entityID = default(NetworkableId);
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
			throw new Exception("Trying to dispose PersistentMissionEntityData with ShouldPool set to false!");
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

	public void CopyTo(PersistentMissionEntityData instance)
	{
		instance.entityID = entityID;
	}

	public PersistentMissionEntityData Copy()
	{
		PersistentMissionEntityData persistentMissionEntityData = Pool.Get<PersistentMissionEntityData>();
		CopyTo(persistentMissionEntityData);
		return persistentMissionEntityData;
	}

	public static PersistentMissionEntityData Deserialize(BufferStream stream)
	{
		PersistentMissionEntityData persistentMissionEntityData = Pool.Get<PersistentMissionEntityData>();
		Deserialize(stream, persistentMissionEntityData, isDelta: false);
		return persistentMissionEntityData;
	}

	public static PersistentMissionEntityData DeserializeLengthDelimited(BufferStream stream)
	{
		PersistentMissionEntityData persistentMissionEntityData = Pool.Get<PersistentMissionEntityData>();
		DeserializeLengthDelimited(stream, persistentMissionEntityData, isDelta: false);
		return persistentMissionEntityData;
	}

	public static PersistentMissionEntityData DeserializeLength(BufferStream stream, int length)
	{
		PersistentMissionEntityData persistentMissionEntityData = Pool.Get<PersistentMissionEntityData>();
		DeserializeLength(stream, length, persistentMissionEntityData, isDelta: false);
		return persistentMissionEntityData;
	}

	public static PersistentMissionEntityData Deserialize(byte[] buffer)
	{
		PersistentMissionEntityData persistentMissionEntityData = Pool.Get<PersistentMissionEntityData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, persistentMissionEntityData, isDelta: false);
		return persistentMissionEntityData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PersistentMissionEntityData previous)
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

	public static PersistentMissionEntityData Deserialize(BufferStream stream, PersistentMissionEntityData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PersistentMissionEntityData");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PersistentMissionEntityData");
				instance.entityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PersistentMissionEntityData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PersistentMissionEntityData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PersistentMissionEntityData DeserializeLengthDelimited(BufferStream stream, PersistentMissionEntityData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PersistentMissionEntityData");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PersistentMissionEntityData");
				instance.entityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PersistentMissionEntityData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PersistentMissionEntityData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static PersistentMissionEntityData DeserializeLength(BufferStream stream, int length, PersistentMissionEntityData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PersistentMissionEntityData");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PersistentMissionEntityData");
				instance.entityID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PersistentMissionEntityData");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PersistentMissionEntityData");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PersistentMissionEntityData instance, PersistentMissionEntityData previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.entityID.Value);
	}

	public static void Serialize(BufferStream stream, PersistentMissionEntityData instance)
	{
		if (instance.entityID != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.entityID.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entityID.Value);
	}
}
