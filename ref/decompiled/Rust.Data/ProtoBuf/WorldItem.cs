using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WorldItem : IDisposable, Pool.IPooled, IProto<WorldItem>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Item item;

	public static void ResetToPool(WorldItem instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.item != null)
			{
				instance.item.ResetToPool();
				instance.item = null;
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
			throw new Exception("Trying to dispose WorldItem with ShouldPool set to false!");
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

	public void CopyTo(WorldItem instance)
	{
		if (item != null)
		{
			if (instance.item == null)
			{
				instance.item = item.Copy();
			}
			else
			{
				item.CopyTo(instance.item);
			}
		}
		else
		{
			instance.item = null;
		}
	}

	public WorldItem Copy()
	{
		WorldItem worldItem = Pool.Get<WorldItem>();
		CopyTo(worldItem);
		return worldItem;
	}

	public static WorldItem Deserialize(BufferStream stream)
	{
		WorldItem worldItem = Pool.Get<WorldItem>();
		Deserialize(stream, worldItem, isDelta: false);
		return worldItem;
	}

	public static WorldItem DeserializeLengthDelimited(BufferStream stream)
	{
		WorldItem worldItem = Pool.Get<WorldItem>();
		DeserializeLengthDelimited(stream, worldItem, isDelta: false);
		return worldItem;
	}

	public static WorldItem DeserializeLength(BufferStream stream, int length)
	{
		WorldItem worldItem = Pool.Get<WorldItem>();
		DeserializeLength(stream, length, worldItem, isDelta: false);
		return worldItem;
	}

	public static WorldItem Deserialize(byte[] buffer)
	{
		WorldItem worldItem = Pool.Get<WorldItem>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, worldItem, isDelta: false);
		return worldItem;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WorldItem previous)
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

	public static WorldItem Deserialize(BufferStream stream, WorldItem instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.WorldItem");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldItem");
				if (instance.item == null)
				{
					instance.item = Item.DeserializeLengthDelimited(stream);
				}
				else
				{
					Item.DeserializeLengthDelimited(stream, instance.item, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldItem");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldItem");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static WorldItem DeserializeLengthDelimited(BufferStream stream, WorldItem instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldItem");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldItem");
				if (instance.item == null)
				{
					instance.item = Item.DeserializeLengthDelimited(stream);
				}
				else
				{
					Item.DeserializeLengthDelimited(stream, instance.item, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldItem");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldItem");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static WorldItem DeserializeLength(BufferStream stream, int length, WorldItem instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WorldItem");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldItem");
				if (instance.item == null)
				{
					instance.item = Item.DeserializeLengthDelimited(stream);
				}
				else
				{
					Item.DeserializeLengthDelimited(stream, instance.item, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WorldItem");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WorldItem");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WorldItem instance, WorldItem previous)
	{
		if (instance.item == null)
		{
			return;
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		Item.SerializeDelta(stream, instance.item, previous.item);
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

	public static void Serialize(BufferStream stream, WorldItem instance)
	{
		if (instance.item == null)
		{
			return;
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		Item.Serialize(stream, instance.item);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		item?.InspectUids(action);
	}
}
