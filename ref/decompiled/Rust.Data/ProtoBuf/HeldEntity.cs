using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class HeldEntity : IDisposable, Pool.IPooled, IProto<HeldEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemId itemUID;

	public static void ResetToPool(HeldEntity instance)
	{
		if (instance.ShouldPool)
		{
			instance.itemUID = default(ItemId);
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
			throw new Exception("Trying to dispose HeldEntity with ShouldPool set to false!");
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

	public void CopyTo(HeldEntity instance)
	{
		instance.itemUID = itemUID;
	}

	public HeldEntity Copy()
	{
		HeldEntity heldEntity = Pool.Get<HeldEntity>();
		CopyTo(heldEntity);
		return heldEntity;
	}

	public static HeldEntity Deserialize(BufferStream stream)
	{
		HeldEntity heldEntity = Pool.Get<HeldEntity>();
		Deserialize(stream, heldEntity, isDelta: false);
		return heldEntity;
	}

	public static HeldEntity DeserializeLengthDelimited(BufferStream stream)
	{
		HeldEntity heldEntity = Pool.Get<HeldEntity>();
		DeserializeLengthDelimited(stream, heldEntity, isDelta: false);
		return heldEntity;
	}

	public static HeldEntity DeserializeLength(BufferStream stream, int length)
	{
		HeldEntity heldEntity = Pool.Get<HeldEntity>();
		DeserializeLength(stream, length, heldEntity, isDelta: false);
		return heldEntity;
	}

	public static HeldEntity Deserialize(byte[] buffer)
	{
		HeldEntity heldEntity = Pool.Get<HeldEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, heldEntity, isDelta: false);
		return heldEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, HeldEntity previous)
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

	public static HeldEntity Deserialize(BufferStream stream, HeldEntity instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.HeldEntity");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HeldEntity");
				instance.itemUID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HeldEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HeldEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static HeldEntity DeserializeLengthDelimited(BufferStream stream, HeldEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HeldEntity");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HeldEntity");
				instance.itemUID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HeldEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HeldEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static HeldEntity DeserializeLength(BufferStream stream, int length, HeldEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HeldEntity");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HeldEntity");
				instance.itemUID = new ItemId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HeldEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HeldEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, HeldEntity instance, HeldEntity previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.itemUID.Value);
	}

	public static void Serialize(BufferStream stream, HeldEntity instance)
	{
		if (instance.itemUID != default(ItemId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.itemUID.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.ItemId, ref itemUID.Value);
	}
}
