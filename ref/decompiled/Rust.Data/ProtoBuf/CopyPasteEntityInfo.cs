using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CopyPasteEntityInfo : IDisposable, Pool.IPooled, IProto<CopyPasteEntityInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Entity> entities;

	[NonSerialized]
	public byte[] screenshot;

	[NonSerialized]
	public long createdTimestamp;

	[NonSerialized]
	public int entityCount;

	public static void ResetToPool(CopyPasteEntityInfo instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				if (instance.entities[i] != null)
				{
					instance.entities[i].ResetToPool();
					instance.entities[i] = null;
				}
			}
			List<Entity> obj = instance.entities;
			Pool.Free(ref obj, freeElements: false);
			instance.entities = obj;
		}
		instance.screenshot = null;
		instance.createdTimestamp = 0L;
		instance.entityCount = 0;
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
			throw new Exception("Trying to dispose CopyPasteEntityInfo with ShouldPool set to false!");
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

	public void CopyTo(CopyPasteEntityInfo instance)
	{
		if (entities != null)
		{
			instance.entities = Pool.Get<List<Entity>>();
			for (int i = 0; i < entities.Count; i++)
			{
				Entity item = entities[i].Copy();
				instance.entities.Add(item);
			}
		}
		else
		{
			instance.entities = null;
		}
		if (screenshot == null)
		{
			instance.screenshot = null;
		}
		else
		{
			instance.screenshot = new byte[screenshot.Length];
			Array.Copy(screenshot, instance.screenshot, instance.screenshot.Length);
		}
		instance.createdTimestamp = createdTimestamp;
		instance.entityCount = entityCount;
	}

	public CopyPasteEntityInfo Copy()
	{
		CopyPasteEntityInfo copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
		CopyTo(copyPasteEntityInfo);
		return copyPasteEntityInfo;
	}

	public static CopyPasteEntityInfo Deserialize(BufferStream stream)
	{
		CopyPasteEntityInfo copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
		Deserialize(stream, copyPasteEntityInfo, isDelta: false);
		return copyPasteEntityInfo;
	}

	public static CopyPasteEntityInfo DeserializeLengthDelimited(BufferStream stream)
	{
		CopyPasteEntityInfo copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
		DeserializeLengthDelimited(stream, copyPasteEntityInfo, isDelta: false);
		return copyPasteEntityInfo;
	}

	public static CopyPasteEntityInfo DeserializeLength(BufferStream stream, int length)
	{
		CopyPasteEntityInfo copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
		DeserializeLength(stream, length, copyPasteEntityInfo, isDelta: false);
		return copyPasteEntityInfo;
	}

	public static CopyPasteEntityInfo Deserialize(byte[] buffer)
	{
		CopyPasteEntityInfo copyPasteEntityInfo = Pool.Get<CopyPasteEntityInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, copyPasteEntityInfo, isDelta: false);
		return copyPasteEntityInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CopyPasteEntityInfo previous)
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

	public static CopyPasteEntityInfo Deserialize(BufferStream stream, CopyPasteEntityInfo instance, bool isDelta)
	{
		if (!isDelta && instance.entities == null)
		{
			instance.entities = Pool.Get<List<Entity>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CopyPasteEntityInfo");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.screenshot = ProtocolParser.ReadBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.createdTimestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.entityCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CopyPasteEntityInfo DeserializeLengthDelimited(BufferStream stream, CopyPasteEntityInfo instance, bool isDelta)
	{
		if (!isDelta && instance.entities == null)
		{
			instance.entities = Pool.Get<List<Entity>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CopyPasteEntityInfo");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.screenshot = ProtocolParser.ReadBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.createdTimestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.entityCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CopyPasteEntityInfo DeserializeLength(BufferStream stream, int length, CopyPasteEntityInfo instance, bool isDelta)
	{
		if (!isDelta && instance.entities == null)
		{
			instance.entities = Pool.Get<List<Entity>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CopyPasteEntityInfo");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.screenshot = ProtocolParser.ReadBytes(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.createdTimestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				instance.entityCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CopyPasteEntityInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CopyPasteEntityInfo instance, CopyPasteEntityInfo previous)
	{
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity entity = instance.entities[i];
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
		if (instance.screenshot != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, instance.screenshot);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.createdTimestamp);
		if (instance.entityCount != previous.entityCount)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.entityCount);
		}
	}

	public static void Serialize(BufferStream stream, CopyPasteEntityInfo instance)
	{
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity instance2 = instance.entities[i];
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
		if (instance.screenshot != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteBytes(stream, instance.screenshot);
		}
		if (instance.createdTimestamp != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.createdTimestamp);
		}
		if (instance.entityCount != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.entityCount);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (entities != null)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				entities[i]?.InspectUids(action);
			}
		}
	}
}
