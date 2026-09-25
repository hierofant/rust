using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class RentableShop : IDisposable, Pool.IPooled, IProto<RentableShop>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool hasStoredItems;

	[NonSerialized]
	public List<ItemContainer> storedItems;

	[NonSerialized]
	public List<ulong> storedItemIds;

	[NonSerialized]
	public float nextRentDue;

	[NonSerialized]
	public float timeSinceShopOpened;

	[NonSerialized]
	public bool isLocalPlayerIntruder;

	[NonSerialized]
	public List<long> storedItemTimestamp;

	[NonSerialized]
	public int shopNumber;

	public static void ResetToPool(RentableShop instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.hasStoredItems = false;
		if (instance.storedItems != null)
		{
			for (int i = 0; i < instance.storedItems.Count; i++)
			{
				if (instance.storedItems[i] != null)
				{
					instance.storedItems[i].ResetToPool();
					instance.storedItems[i] = null;
				}
			}
			List<ItemContainer> obj = instance.storedItems;
			Pool.Free(ref obj, freeElements: false);
			instance.storedItems = obj;
		}
		if (instance.storedItemIds != null)
		{
			List<ulong> obj2 = instance.storedItemIds;
			Pool.FreeUnmanaged(ref obj2);
			instance.storedItemIds = obj2;
		}
		instance.nextRentDue = 0f;
		instance.timeSinceShopOpened = 0f;
		instance.isLocalPlayerIntruder = false;
		if (instance.storedItemTimestamp != null)
		{
			List<long> obj3 = instance.storedItemTimestamp;
			Pool.FreeUnmanaged(ref obj3);
			instance.storedItemTimestamp = obj3;
		}
		instance.shopNumber = 0;
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
			throw new Exception("Trying to dispose RentableShop with ShouldPool set to false!");
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

	public void CopyTo(RentableShop instance)
	{
		instance.hasStoredItems = hasStoredItems;
		if (storedItems != null)
		{
			instance.storedItems = Pool.Get<List<ItemContainer>>();
			for (int i = 0; i < storedItems.Count; i++)
			{
				ItemContainer item = storedItems[i].Copy();
				instance.storedItems.Add(item);
			}
		}
		else
		{
			instance.storedItems = null;
		}
		if (storedItemIds != null)
		{
			instance.storedItemIds = Pool.Get<List<ulong>>();
			for (int j = 0; j < storedItemIds.Count; j++)
			{
				ulong item2 = storedItemIds[j];
				instance.storedItemIds.Add(item2);
			}
		}
		else
		{
			instance.storedItemIds = null;
		}
		instance.nextRentDue = nextRentDue;
		instance.timeSinceShopOpened = timeSinceShopOpened;
		instance.isLocalPlayerIntruder = isLocalPlayerIntruder;
		if (storedItemTimestamp != null)
		{
			instance.storedItemTimestamp = Pool.Get<List<long>>();
			for (int k = 0; k < storedItemTimestamp.Count; k++)
			{
				long item3 = storedItemTimestamp[k];
				instance.storedItemTimestamp.Add(item3);
			}
		}
		else
		{
			instance.storedItemTimestamp = null;
		}
		instance.shopNumber = shopNumber;
	}

	public RentableShop Copy()
	{
		RentableShop rentableShop = Pool.Get<RentableShop>();
		CopyTo(rentableShop);
		return rentableShop;
	}

	public static RentableShop Deserialize(BufferStream stream)
	{
		RentableShop rentableShop = Pool.Get<RentableShop>();
		Deserialize(stream, rentableShop, isDelta: false);
		return rentableShop;
	}

	public static RentableShop DeserializeLengthDelimited(BufferStream stream)
	{
		RentableShop rentableShop = Pool.Get<RentableShop>();
		DeserializeLengthDelimited(stream, rentableShop, isDelta: false);
		return rentableShop;
	}

	public static RentableShop DeserializeLength(BufferStream stream, int length)
	{
		RentableShop rentableShop = Pool.Get<RentableShop>();
		DeserializeLength(stream, length, rentableShop, isDelta: false);
		return rentableShop;
	}

	public static RentableShop Deserialize(byte[] buffer)
	{
		RentableShop rentableShop = Pool.Get<RentableShop>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, rentableShop, isDelta: false);
		return rentableShop;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, RentableShop previous)
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

	public static RentableShop Deserialize(BufferStream stream, RentableShop instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.storedItems == null)
			{
				instance.storedItems = Pool.Get<List<ItemContainer>>();
			}
			if (instance.storedItemIds == null)
			{
				instance.storedItemIds = Pool.Get<List<ulong>>();
			}
			if (instance.storedItemTimestamp == null)
			{
				instance.storedItemTimestamp = Pool.Get<List<long>>();
			}
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.RentableShop");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.hasStoredItems = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItems.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItemIds.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.nextRentDue = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.timeSinceShopOpened = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.isLocalPlayerIntruder = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItemTimestamp.Add((long)ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.shopNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RentableShop DeserializeLengthDelimited(BufferStream stream, RentableShop instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.storedItems == null)
			{
				instance.storedItems = Pool.Get<List<ItemContainer>>();
			}
			if (instance.storedItemIds == null)
			{
				instance.storedItemIds = Pool.Get<List<ulong>>();
			}
			if (instance.storedItemTimestamp == null)
			{
				instance.storedItemTimestamp = Pool.Get<List<long>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.RentableShop");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.hasStoredItems = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItems.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItemIds.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.nextRentDue = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.timeSinceShopOpened = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.isLocalPlayerIntruder = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItemTimestamp.Add((long)ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.shopNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RentableShop DeserializeLength(BufferStream stream, int length, RentableShop instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.storedItems == null)
			{
				instance.storedItems = Pool.Get<List<ItemContainer>>();
			}
			if (instance.storedItemIds == null)
			{
				instance.storedItemIds = Pool.Get<List<ulong>>();
			}
			if (instance.storedItemTimestamp == null)
			{
				instance.storedItemTimestamp = Pool.Get<List<long>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.RentableShop");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.hasStoredItems = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItems.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItemIds.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.nextRentDue = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.timeSinceShopOpened = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.isLocalPlayerIntruder = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				stream.ConsumeRepeatedElement();
				instance.storedItemTimestamp.Add((long)ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				instance.shopNumber = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RentableShop");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, RentableShop instance, RentableShop previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.hasStoredItems);
		if (instance.storedItems != null)
		{
			for (int i = 0; i < instance.storedItems.Count; i++)
			{
				ItemContainer itemContainer = instance.storedItems[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ItemContainer.SerializeDelta(stream, itemContainer, itemContainer);
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
		if (instance.storedItemIds != null)
		{
			for (int j = 0; j < instance.storedItemIds.Count; j++)
			{
				ulong val2 = instance.storedItemIds[j];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, val2);
			}
		}
		if (instance.nextRentDue != previous.nextRentDue)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.nextRentDue);
		}
		if (instance.timeSinceShopOpened != previous.timeSinceShopOpened)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.timeSinceShopOpened);
		}
		stream.WriteByte(48);
		ProtocolParser.WriteBool(stream, instance.isLocalPlayerIntruder);
		if (instance.storedItemTimestamp != null)
		{
			for (int k = 0; k < instance.storedItemTimestamp.Count; k++)
			{
				long val3 = instance.storedItemTimestamp[k];
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)val3);
			}
		}
		if (instance.shopNumber != previous.shopNumber)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.shopNumber);
		}
	}

	public static void Serialize(BufferStream stream, RentableShop instance)
	{
		if (instance.hasStoredItems)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.hasStoredItems);
		}
		if (instance.storedItems != null)
		{
			for (int i = 0; i < instance.storedItems.Count; i++)
			{
				ItemContainer instance2 = instance.storedItems[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ItemContainer.Serialize(stream, instance2);
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
		if (instance.storedItemIds != null)
		{
			for (int j = 0; j < instance.storedItemIds.Count; j++)
			{
				ulong val2 = instance.storedItemIds[j];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, val2);
			}
		}
		if (instance.nextRentDue != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.nextRentDue);
		}
		if (instance.timeSinceShopOpened != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.timeSinceShopOpened);
		}
		if (instance.isLocalPlayerIntruder)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.isLocalPlayerIntruder);
		}
		if (instance.storedItemTimestamp != null)
		{
			for (int k = 0; k < instance.storedItemTimestamp.Count; k++)
			{
				long val3 = instance.storedItemTimestamp[k];
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)val3);
			}
		}
		if (instance.shopNumber != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.shopNumber);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (storedItems != null)
		{
			for (int i = 0; i < storedItems.Count; i++)
			{
				storedItems[i]?.InspectUids(action);
			}
		}
	}
}
