using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class EntityList : IDisposable, Pool.IPooled, IProto<EntityList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Entity> entity;

	public static void ResetToPool(EntityList instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.entity != null)
		{
			for (int i = 0; i < instance.entity.Count; i++)
			{
				if (instance.entity[i] != null)
				{
					instance.entity[i].ResetToPool();
					instance.entity[i] = null;
				}
			}
			List<Entity> obj = instance.entity;
			Pool.Free(ref obj, freeElements: false);
			instance.entity = obj;
		}
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose EntityList with ShouldPool set to false!");
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

	public void CopyTo(EntityList instance)
	{
		if (entity != null)
		{
			instance.entity = Pool.Get<List<Entity>>();
			for (int i = 0; i < entity.Count; i++)
			{
				Entity item = entity[i].Copy();
				instance.entity.Add(item);
			}
		}
		else
		{
			instance.entity = null;
		}
	}

	public EntityList Copy()
	{
		EntityList entityList = Pool.Get<EntityList>();
		CopyTo(entityList);
		return entityList;
	}

	public static EntityList Deserialize(BufferStream stream)
	{
		EntityList entityList = Pool.Get<EntityList>();
		Deserialize(stream, entityList, isDelta: false);
		return entityList;
	}

	public static EntityList DeserializeLengthDelimited(BufferStream stream)
	{
		EntityList entityList = Pool.Get<EntityList>();
		DeserializeLengthDelimited(stream, entityList, isDelta: false);
		return entityList;
	}

	public static EntityList DeserializeLength(BufferStream stream, int length)
	{
		EntityList entityList = Pool.Get<EntityList>();
		DeserializeLength(stream, length, entityList, isDelta: false);
		return entityList;
	}

	public static EntityList Deserialize(byte[] buffer)
	{
		EntityList entityList = Pool.Get<EntityList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, entityList, isDelta: false);
		return entityList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, EntityList previous)
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

	public static EntityList Deserialize(BufferStream stream, EntityList instance, bool isDelta)
	{
		if (!isDelta && instance.entity == null)
		{
			instance.entity = Pool.Get<List<Entity>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.EntityList");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityList");
				stream.ConsumeRepeatedElement();
				instance.entity.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntityList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static EntityList DeserializeLengthDelimited(BufferStream stream, EntityList instance, bool isDelta)
	{
		if (!isDelta && instance.entity == null)
		{
			instance.entity = Pool.Get<List<Entity>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.EntityList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityList");
				stream.ConsumeRepeatedElement();
				instance.entity.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntityList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static EntityList DeserializeLength(BufferStream stream, int length, EntityList instance, bool isDelta)
	{
		if (!isDelta && instance.entity == null)
		{
			instance.entity = Pool.Get<List<Entity>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.EntityList");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityList");
				stream.ConsumeRepeatedElement();
				instance.entity.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.EntityList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.EntityList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, EntityList instance, EntityList previous)
	{
		if (instance.entity == null)
		{
			return;
		}
		for (int i = 0; i < instance.entity.Count; i++)
		{
			Entity entity = instance.entity[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Entity.SerializeDelta(stream, entity, entity);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, EntityList instance)
	{
		if (instance.entity == null)
		{
			return;
		}
		for (int i = 0; i < instance.entity.Count; i++)
		{
			Entity instance2 = instance.entity[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Entity.Serialize(stream, instance2);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (entity != null)
		{
			for (int i = 0; i < entity.Count; i++)
			{
				entity[i]?.InspectUids(action);
			}
		}
	}
}
