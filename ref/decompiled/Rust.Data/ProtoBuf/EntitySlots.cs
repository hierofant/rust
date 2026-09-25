using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class EntitySlots : IDisposable, Pool.IPooled, IProto<EntitySlots>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId slotLock;

	[NonSerialized]
	public NetworkableId slotFireMod;

	[NonSerialized]
	public NetworkableId slotUpperModification;

	[NonSerialized]
	public NetworkableId centerDecoration;

	[NonSerialized]
	public NetworkableId lowerCenterDecoration;

	[NonSerialized]
	public NetworkableId storageMonitor;

	public static void ResetToPool(EntitySlots instance)
	{
		if (instance.ShouldPool)
		{
			instance.slotLock = default(NetworkableId);
			instance.slotFireMod = default(NetworkableId);
			instance.slotUpperModification = default(NetworkableId);
			instance.centerDecoration = default(NetworkableId);
			instance.lowerCenterDecoration = default(NetworkableId);
			instance.storageMonitor = default(NetworkableId);
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
			throw new Exception("Trying to dispose EntitySlots with ShouldPool set to false!");
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

	public void CopyTo(EntitySlots instance)
	{
		instance.slotLock = slotLock;
		instance.slotFireMod = slotFireMod;
		instance.slotUpperModification = slotUpperModification;
		instance.centerDecoration = centerDecoration;
		instance.lowerCenterDecoration = lowerCenterDecoration;
		instance.storageMonitor = storageMonitor;
	}

	public EntitySlots Copy()
	{
		EntitySlots entitySlots = Pool.Get<EntitySlots>();
		CopyTo(entitySlots);
		return entitySlots;
	}

	public static EntitySlots Deserialize(BufferStream stream)
	{
		EntitySlots entitySlots = Pool.Get<EntitySlots>();
		Deserialize(stream, entitySlots, isDelta: false);
		return entitySlots;
	}

	public static EntitySlots DeserializeLengthDelimited(BufferStream stream)
	{
		EntitySlots entitySlots = Pool.Get<EntitySlots>();
		DeserializeLengthDelimited(stream, entitySlots, isDelta: false);
		return entitySlots;
	}

	public static EntitySlots DeserializeLength(BufferStream stream, int length)
	{
		EntitySlots entitySlots = Pool.Get<EntitySlots>();
		DeserializeLength(stream, length, entitySlots, isDelta: false);
		return entitySlots;
	}

	public static EntitySlots Deserialize(byte[] buffer)
	{
		EntitySlots entitySlots = Pool.Get<EntitySlots>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, entitySlots, isDelta: false);
		return entitySlots;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, EntitySlots previous)
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

	public static EntitySlots Deserialize(BufferStream stream, EntitySlots instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.EntitySlots");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotLock = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotFireMod = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotUpperModification = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.centerDecoration = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.lowerCenterDecoration = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.storageMonitor = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static EntitySlots DeserializeLengthDelimited(BufferStream stream, EntitySlots instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.EntitySlots");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotLock = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotFireMod = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotUpperModification = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.centerDecoration = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.lowerCenterDecoration = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.storageMonitor = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static EntitySlots DeserializeLength(BufferStream stream, int length, EntitySlots instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.EntitySlots");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotLock = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotFireMod = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.slotUpperModification = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.centerDecoration = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.lowerCenterDecoration = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				instance.storageMonitor = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntitySlots");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, EntitySlots instance, EntitySlots previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.slotLock.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.slotFireMod.Value);
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, instance.slotUpperModification.Value);
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.centerDecoration.Value);
		stream.WriteByte(40);
		ProtocolParser.WriteUInt64(stream, instance.lowerCenterDecoration.Value);
		stream.WriteByte(48);
		ProtocolParser.WriteUInt64(stream, instance.storageMonitor.Value);
	}

	public static void Serialize(BufferStream stream, EntitySlots instance)
	{
		if (instance.slotLock != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.slotLock.Value);
		}
		if (instance.slotFireMod != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.slotFireMod.Value);
		}
		if (instance.slotUpperModification != default(NetworkableId))
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.slotUpperModification.Value);
		}
		if (instance.centerDecoration != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.centerDecoration.Value);
		}
		if (instance.lowerCenterDecoration != default(NetworkableId))
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.lowerCenterDecoration.Value);
		}
		if (instance.storageMonitor != default(NetworkableId))
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.storageMonitor.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref slotLock.Value);
		action(UidType.NetworkableId, ref slotFireMod.Value);
		action(UidType.NetworkableId, ref slotUpperModification.Value);
		action(UidType.NetworkableId, ref centerDecoration.Value);
		action(UidType.NetworkableId, ref lowerCenterDecoration.Value);
		action(UidType.NetworkableId, ref storageMonitor.Value);
	}
}
