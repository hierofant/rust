using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VendingMachinePurchaseHistoryEntrySmallMessage : IDisposable, Pool.IPooled, IProto<VendingMachinePurchaseHistoryEntrySmallMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int itemID;

	[NonSerialized]
	public int amount;

	[NonSerialized]
	public int priceID;

	[NonSerialized]
	public int price;

	[NonSerialized]
	public bool itemIsBp;

	[NonSerialized]
	public bool priceIsBp;

	public static void ResetToPool(VendingMachinePurchaseHistoryEntrySmallMessage instance)
	{
		if (instance.ShouldPool)
		{
			instance.itemID = 0;
			instance.amount = 0;
			instance.priceID = 0;
			instance.price = 0;
			instance.itemIsBp = false;
			instance.priceIsBp = false;
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
			throw new Exception("Trying to dispose VendingMachinePurchaseHistoryEntrySmallMessage with ShouldPool set to false!");
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

	public void CopyTo(VendingMachinePurchaseHistoryEntrySmallMessage instance)
	{
		instance.itemID = itemID;
		instance.amount = amount;
		instance.priceID = priceID;
		instance.price = price;
		instance.itemIsBp = itemIsBp;
		instance.priceIsBp = priceIsBp;
	}

	public VendingMachinePurchaseHistoryEntrySmallMessage Copy()
	{
		VendingMachinePurchaseHistoryEntrySmallMessage vendingMachinePurchaseHistoryEntrySmallMessage = Pool.Get<VendingMachinePurchaseHistoryEntrySmallMessage>();
		CopyTo(vendingMachinePurchaseHistoryEntrySmallMessage);
		return vendingMachinePurchaseHistoryEntrySmallMessage;
	}

	public static VendingMachinePurchaseHistoryEntrySmallMessage Deserialize(BufferStream stream)
	{
		VendingMachinePurchaseHistoryEntrySmallMessage vendingMachinePurchaseHistoryEntrySmallMessage = Pool.Get<VendingMachinePurchaseHistoryEntrySmallMessage>();
		Deserialize(stream, vendingMachinePurchaseHistoryEntrySmallMessage, isDelta: false);
		return vendingMachinePurchaseHistoryEntrySmallMessage;
	}

	public static VendingMachinePurchaseHistoryEntrySmallMessage DeserializeLengthDelimited(BufferStream stream)
	{
		VendingMachinePurchaseHistoryEntrySmallMessage vendingMachinePurchaseHistoryEntrySmallMessage = Pool.Get<VendingMachinePurchaseHistoryEntrySmallMessage>();
		DeserializeLengthDelimited(stream, vendingMachinePurchaseHistoryEntrySmallMessage, isDelta: false);
		return vendingMachinePurchaseHistoryEntrySmallMessage;
	}

	public static VendingMachinePurchaseHistoryEntrySmallMessage DeserializeLength(BufferStream stream, int length)
	{
		VendingMachinePurchaseHistoryEntrySmallMessage vendingMachinePurchaseHistoryEntrySmallMessage = Pool.Get<VendingMachinePurchaseHistoryEntrySmallMessage>();
		DeserializeLength(stream, length, vendingMachinePurchaseHistoryEntrySmallMessage, isDelta: false);
		return vendingMachinePurchaseHistoryEntrySmallMessage;
	}

	public static VendingMachinePurchaseHistoryEntrySmallMessage Deserialize(byte[] buffer)
	{
		VendingMachinePurchaseHistoryEntrySmallMessage vendingMachinePurchaseHistoryEntrySmallMessage = Pool.Get<VendingMachinePurchaseHistoryEntrySmallMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vendingMachinePurchaseHistoryEntrySmallMessage, isDelta: false);
		return vendingMachinePurchaseHistoryEntrySmallMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VendingMachinePurchaseHistoryEntrySmallMessage previous)
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

	public static VendingMachinePurchaseHistoryEntrySmallMessage Deserialize(BufferStream stream, VendingMachinePurchaseHistoryEntrySmallMessage instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.priceID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.price = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.itemIsBp = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.priceIsBp = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachinePurchaseHistoryEntrySmallMessage DeserializeLengthDelimited(BufferStream stream, VendingMachinePurchaseHistoryEntrySmallMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.priceID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.price = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.itemIsBp = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.priceIsBp = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachinePurchaseHistoryEntrySmallMessage DeserializeLength(BufferStream stream, int length, VendingMachinePurchaseHistoryEntrySmallMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.amount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.priceID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.price = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.itemIsBp = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				instance.priceIsBp = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VendingMachinePurchaseHistoryEntrySmallMessage instance, VendingMachinePurchaseHistoryEntrySmallMessage previous)
	{
		if (instance.itemID != previous.itemID)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
		}
		if (instance.amount != previous.amount)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
		}
		if (instance.priceID != previous.priceID)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.priceID);
		}
		if (instance.price != previous.price)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.price);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.itemIsBp);
		stream.WriteByte(48);
		ProtocolParser.WriteBool(stream, instance.priceIsBp);
	}

	public static void Serialize(BufferStream stream, VendingMachinePurchaseHistoryEntrySmallMessage instance)
	{
		if (instance.itemID != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
		}
		if (instance.amount != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
		}
		if (instance.priceID != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.priceID);
		}
		if (instance.price != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.price);
		}
		if (instance.itemIsBp)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.itemIsBp);
		}
		if (instance.priceIsBp)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.priceIsBp);
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
