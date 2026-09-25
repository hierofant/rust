using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class EntityIdList : IDisposable, Pool.IPooled, IProto<EntityIdList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<NetworkableId> entityIds;

	public static void ResetToPool(EntityIdList instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.entityIds != null)
			{
				List<NetworkableId> obj = instance.entityIds;
				Pool.FreeUnmanaged(ref obj);
				instance.entityIds = obj;
			}
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
			throw new Exception("Trying to dispose EntityIdList with ShouldPool set to false!");
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

	public void CopyTo(EntityIdList instance)
	{
		if (entityIds != null)
		{
			instance.entityIds = Pool.Get<List<NetworkableId>>();
			for (int i = 0; i < entityIds.Count; i++)
			{
				NetworkableId item = entityIds[i];
				instance.entityIds.Add(item);
			}
		}
		else
		{
			instance.entityIds = null;
		}
	}

	public EntityIdList Copy()
	{
		EntityIdList entityIdList = Pool.Get<EntityIdList>();
		CopyTo(entityIdList);
		return entityIdList;
	}

	public static EntityIdList Deserialize(BufferStream stream)
	{
		EntityIdList entityIdList = Pool.Get<EntityIdList>();
		Deserialize(stream, entityIdList, isDelta: false);
		return entityIdList;
	}

	public static EntityIdList DeserializeLengthDelimited(BufferStream stream)
	{
		EntityIdList entityIdList = Pool.Get<EntityIdList>();
		DeserializeLengthDelimited(stream, entityIdList, isDelta: false);
		return entityIdList;
	}

	public static EntityIdList DeserializeLength(BufferStream stream, int length)
	{
		EntityIdList entityIdList = Pool.Get<EntityIdList>();
		DeserializeLength(stream, length, entityIdList, isDelta: false);
		return entityIdList;
	}

	public static EntityIdList Deserialize(byte[] buffer)
	{
		EntityIdList entityIdList = Pool.Get<EntityIdList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, entityIdList, isDelta: false);
		return entityIdList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, EntityIdList previous)
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

	public static EntityIdList Deserialize(BufferStream stream, EntityIdList instance, bool isDelta)
	{
		if (!isDelta && instance.entityIds == null)
		{
			instance.entityIds = Pool.Get<List<NetworkableId>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.EntityIdList");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				stream.ConsumeRepeatedElement();
				instance.entityIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static EntityIdList DeserializeLengthDelimited(BufferStream stream, EntityIdList instance, bool isDelta)
	{
		if (!isDelta && instance.entityIds == null)
		{
			instance.entityIds = Pool.Get<List<NetworkableId>>();
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.EntityIdList");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				stream.ConsumeRepeatedElement();
				instance.entityIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static EntityIdList DeserializeLength(BufferStream stream, int length, EntityIdList instance, bool isDelta)
	{
		if (!isDelta && instance.entityIds == null)
		{
			instance.entityIds = Pool.Get<List<NetworkableId>>();
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.EntityIdList");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				stream.ConsumeRepeatedElement();
				instance.entityIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntityIdList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, EntityIdList instance, EntityIdList previous)
	{
		if (instance.entityIds != null)
		{
			for (int i = 0; i < instance.entityIds.Count; i++)
			{
				NetworkableId networkableId = instance.entityIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public static void Serialize(BufferStream stream, EntityIdList instance)
	{
		if (instance.entityIds != null)
		{
			for (int i = 0; i < instance.entityIds.Count; i++)
			{
				NetworkableId networkableId = instance.entityIds[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (entityIds != null)
		{
			for (int i = 0; i < entityIds.Count; i++)
			{
				NetworkableId value = entityIds[i];
				action(UidType.NetworkableId, ref value.Value);
				entityIds[i] = value;
			}
		}
	}
}
