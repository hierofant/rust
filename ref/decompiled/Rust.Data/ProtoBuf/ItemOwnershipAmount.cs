using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ItemOwnershipAmount : IDisposable, Pool.IPooled, IProto<ItemOwnershipAmount>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string username;

	[NonSerialized]
	public string reason;

	[NonSerialized]
	public int amount;

	public static void ResetToPool(ItemOwnershipAmount instance)
	{
		if (instance.ShouldPool)
		{
			instance.username = string.Empty;
			instance.reason = string.Empty;
			instance.amount = 0;
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
			throw new Exception("Trying to dispose ItemOwnershipAmount with ShouldPool set to false!");
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

	public void CopyTo(ItemOwnershipAmount instance)
	{
		instance.username = username;
		instance.reason = reason;
		instance.amount = amount;
	}

	public ItemOwnershipAmount Copy()
	{
		ItemOwnershipAmount itemOwnershipAmount = Pool.Get<ItemOwnershipAmount>();
		CopyTo(itemOwnershipAmount);
		return itemOwnershipAmount;
	}

	public static ItemOwnershipAmount Deserialize(BufferStream stream)
	{
		ItemOwnershipAmount itemOwnershipAmount = Pool.Get<ItemOwnershipAmount>();
		Deserialize(stream, itemOwnershipAmount, isDelta: false);
		return itemOwnershipAmount;
	}

	public static ItemOwnershipAmount DeserializeLengthDelimited(BufferStream stream)
	{
		ItemOwnershipAmount itemOwnershipAmount = Pool.Get<ItemOwnershipAmount>();
		DeserializeLengthDelimited(stream, itemOwnershipAmount, isDelta: false);
		return itemOwnershipAmount;
	}

	public static ItemOwnershipAmount DeserializeLength(BufferStream stream, int length)
	{
		ItemOwnershipAmount itemOwnershipAmount = Pool.Get<ItemOwnershipAmount>();
		DeserializeLength(stream, length, itemOwnershipAmount, isDelta: false);
		return itemOwnershipAmount;
	}

	public static ItemOwnershipAmount Deserialize(byte[] buffer)
	{
		ItemOwnershipAmount itemOwnershipAmount = Pool.Get<ItemOwnershipAmount>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, itemOwnershipAmount, isDelta: false);
		return itemOwnershipAmount;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ItemOwnershipAmount previous)
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

	public static ItemOwnershipAmount Deserialize(BufferStream stream, ItemOwnershipAmount instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ItemOwnershipAmount");
			switch (num)
			{
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.username = ProtocolParser.ReadString(stream);
				continue;
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.reason = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ItemOwnershipAmount DeserializeLengthDelimited(BufferStream stream, ItemOwnershipAmount instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemOwnershipAmount");
			switch (num2)
			{
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.username = ProtocolParser.ReadString(stream);
				continue;
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.reason = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ItemOwnershipAmount DeserializeLength(BufferStream stream, int length, ItemOwnershipAmount instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemOwnershipAmount");
			switch (num2)
			{
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.username = ProtocolParser.ReadString(stream);
				continue;
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.reason = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemOwnershipAmount");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ItemOwnershipAmount instance, ItemOwnershipAmount previous)
	{
		if (instance.username != null && instance.username != previous.username)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.username);
		}
		if (instance.reason != null && instance.reason != previous.reason)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.reason);
		}
		if (instance.amount != previous.amount)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
		}
	}

	public static void Serialize(BufferStream stream, ItemOwnershipAmount instance)
	{
		if (instance.username != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.username);
		}
		if (instance.reason != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.reason);
		}
		if (instance.amount != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
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
