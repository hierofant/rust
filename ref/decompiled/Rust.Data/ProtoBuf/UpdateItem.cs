using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class UpdateItem : IDisposable, Pool.IPooled, IProto<UpdateItem>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Item item;

	public static void ResetToPool(UpdateItem instance)
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
			throw new Exception("Trying to dispose UpdateItem with ShouldPool set to false!");
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

	public void CopyTo(UpdateItem instance)
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

	public UpdateItem Copy()
	{
		UpdateItem updateItem = Pool.Get<UpdateItem>();
		CopyTo(updateItem);
		return updateItem;
	}

	public static UpdateItem Deserialize(BufferStream stream)
	{
		UpdateItem updateItem = Pool.Get<UpdateItem>();
		Deserialize(stream, updateItem, isDelta: false);
		return updateItem;
	}

	public static UpdateItem DeserializeLengthDelimited(BufferStream stream)
	{
		UpdateItem updateItem = Pool.Get<UpdateItem>();
		DeserializeLengthDelimited(stream, updateItem, isDelta: false);
		return updateItem;
	}

	public static UpdateItem DeserializeLength(BufferStream stream, int length)
	{
		UpdateItem updateItem = Pool.Get<UpdateItem>();
		DeserializeLength(stream, length, updateItem, isDelta: false);
		return updateItem;
	}

	public static UpdateItem Deserialize(byte[] buffer)
	{
		UpdateItem updateItem = Pool.Get<UpdateItem>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, updateItem, isDelta: false);
		return updateItem;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, UpdateItem previous)
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

	public static UpdateItem Deserialize(BufferStream stream, UpdateItem instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.UpdateItem");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItem");
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
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItem");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.UpdateItem");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static UpdateItem DeserializeLengthDelimited(BufferStream stream, UpdateItem instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.UpdateItem");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItem");
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
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItem");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.UpdateItem");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static UpdateItem DeserializeLength(BufferStream stream, int length, UpdateItem instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.UpdateItem");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItem");
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
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItem");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.UpdateItem");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, UpdateItem instance, UpdateItem previous)
	{
		if (instance.item == null)
		{
			throw new ArgumentNullException("item", "Required by proto specification.");
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

	public static void Serialize(BufferStream stream, UpdateItem instance)
	{
		if (instance.item == null)
		{
			throw new ArgumentNullException("item", "Required by proto specification.");
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
