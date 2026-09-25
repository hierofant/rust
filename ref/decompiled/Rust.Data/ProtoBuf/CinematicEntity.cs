using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CinematicEntity : IDisposable, Pool.IPooled, IProto<CinematicEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId targetPlayer;

	public static void ResetToPool(CinematicEntity instance)
	{
		if (instance.ShouldPool)
		{
			instance.targetPlayer = default(NetworkableId);
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
			throw new Exception("Trying to dispose CinematicEntity with ShouldPool set to false!");
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

	public void CopyTo(CinematicEntity instance)
	{
		instance.targetPlayer = targetPlayer;
	}

	public CinematicEntity Copy()
	{
		CinematicEntity cinematicEntity = Pool.Get<CinematicEntity>();
		CopyTo(cinematicEntity);
		return cinematicEntity;
	}

	public static CinematicEntity Deserialize(BufferStream stream)
	{
		CinematicEntity cinematicEntity = Pool.Get<CinematicEntity>();
		Deserialize(stream, cinematicEntity, isDelta: false);
		return cinematicEntity;
	}

	public static CinematicEntity DeserializeLengthDelimited(BufferStream stream)
	{
		CinematicEntity cinematicEntity = Pool.Get<CinematicEntity>();
		DeserializeLengthDelimited(stream, cinematicEntity, isDelta: false);
		return cinematicEntity;
	}

	public static CinematicEntity DeserializeLength(BufferStream stream, int length)
	{
		CinematicEntity cinematicEntity = Pool.Get<CinematicEntity>();
		DeserializeLength(stream, length, cinematicEntity, isDelta: false);
		return cinematicEntity;
	}

	public static CinematicEntity Deserialize(byte[] buffer)
	{
		CinematicEntity cinematicEntity = Pool.Get<CinematicEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, cinematicEntity, isDelta: false);
		return cinematicEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CinematicEntity previous)
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

	public static CinematicEntity Deserialize(BufferStream stream, CinematicEntity instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CinematicEntity");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CinematicEntity");
				instance.targetPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CinematicEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CinematicEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CinematicEntity DeserializeLengthDelimited(BufferStream stream, CinematicEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CinematicEntity");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CinematicEntity");
				instance.targetPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CinematicEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CinematicEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static CinematicEntity DeserializeLength(BufferStream stream, int length, CinematicEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CinematicEntity");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CinematicEntity");
				instance.targetPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CinematicEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CinematicEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CinematicEntity instance, CinematicEntity previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.targetPlayer.Value);
	}

	public static void Serialize(BufferStream stream, CinematicEntity instance)
	{
		if (instance.targetPlayer != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.targetPlayer.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref targetPlayer.Value);
	}
}
