using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ItemContainer : IDisposable, Pool.IPooled, IProto<ItemContainer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemContainerId UID;

	[NonSerialized]
	public int slots;

	[NonSerialized]
	public float temperature;

	[NonSerialized]
	public int flags;

	[NonSerialized]
	public int allowedContents;

	[NonSerialized]
	public int maxStackSize;

	[NonSerialized]
	public List<int> allowedItems;

	[NonSerialized]
	public List<int> availableSlots;

	[NonSerialized]
	public int volume;

	[NonSerialized]
	public bool allowItemsToIncreaseToMaxStackSize;

	[NonSerialized]
	public List<Item> contents;

	public static void ResetToPool(ItemContainer instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.UID = default(ItemContainerId);
		instance.slots = 0;
		instance.temperature = 0f;
		instance.flags = 0;
		instance.allowedContents = 0;
		instance.maxStackSize = 0;
		if (instance.allowedItems != null)
		{
			List<int> obj = instance.allowedItems;
			Pool.FreeUnmanaged(ref obj);
			instance.allowedItems = obj;
		}
		if (instance.availableSlots != null)
		{
			List<int> obj2 = instance.availableSlots;
			Pool.FreeUnmanaged(ref obj2);
			instance.availableSlots = obj2;
		}
		instance.volume = 0;
		instance.allowItemsToIncreaseToMaxStackSize = false;
		if (instance.contents != null)
		{
			for (int i = 0; i < instance.contents.Count; i++)
			{
				if (instance.contents[i] != null)
				{
					instance.contents[i].ResetToPool();
					instance.contents[i] = null;
				}
			}
			List<Item> obj3 = instance.contents;
			Pool.Free(ref obj3, freeElements: false);
			instance.contents = obj3;
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
			throw new Exception("Trying to dispose ItemContainer with ShouldPool set to false!");
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

	public void CopyTo(ItemContainer instance)
	{
		instance.UID = UID;
		instance.slots = slots;
		instance.temperature = temperature;
		instance.flags = flags;
		instance.allowedContents = allowedContents;
		instance.maxStackSize = maxStackSize;
		if (allowedItems != null)
		{
			instance.allowedItems = Pool.Get<List<int>>();
			for (int i = 0; i < allowedItems.Count; i++)
			{
				int item = allowedItems[i];
				instance.allowedItems.Add(item);
			}
		}
		else
		{
			instance.allowedItems = null;
		}
		if (availableSlots != null)
		{
			instance.availableSlots = Pool.Get<List<int>>();
			for (int j = 0; j < availableSlots.Count; j++)
			{
				int item2 = availableSlots[j];
				instance.availableSlots.Add(item2);
			}
		}
		else
		{
			instance.availableSlots = null;
		}
		instance.volume = volume;
		instance.allowItemsToIncreaseToMaxStackSize = allowItemsToIncreaseToMaxStackSize;
		if (contents != null)
		{
			instance.contents = Pool.Get<List<Item>>();
			for (int k = 0; k < contents.Count; k++)
			{
				Item item3 = contents[k].Copy();
				instance.contents.Add(item3);
			}
		}
		else
		{
			instance.contents = null;
		}
	}

	public ItemContainer Copy()
	{
		ItemContainer itemContainer = Pool.Get<ItemContainer>();
		CopyTo(itemContainer);
		return itemContainer;
	}

	public static ItemContainer Deserialize(BufferStream stream)
	{
		ItemContainer itemContainer = Pool.Get<ItemContainer>();
		Deserialize(stream, itemContainer, isDelta: false);
		return itemContainer;
	}

	public static ItemContainer DeserializeLengthDelimited(BufferStream stream)
	{
		ItemContainer itemContainer = Pool.Get<ItemContainer>();
		DeserializeLengthDelimited(stream, itemContainer, isDelta: false);
		return itemContainer;
	}

	public static ItemContainer DeserializeLength(BufferStream stream, int length)
	{
		ItemContainer itemContainer = Pool.Get<ItemContainer>();
		DeserializeLength(stream, length, itemContainer, isDelta: false);
		return itemContainer;
	}

	public static ItemContainer Deserialize(byte[] buffer)
	{
		ItemContainer itemContainer = Pool.Get<ItemContainer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, itemContainer, isDelta: false);
		return itemContainer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ItemContainer previous)
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

	public static ItemContainer Deserialize(BufferStream stream, ItemContainer instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.allowedItems == null)
			{
				instance.allowedItems = Pool.Get<List<int>>();
			}
			if (instance.availableSlots == null)
			{
				instance.availableSlots = Pool.Get<List<int>>();
			}
			if (instance.contents == null)
			{
				instance.contents = Pool.Get<List<Item>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemContainer");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.UID = new ItemContainerId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.slots = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.temperature = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.allowedContents = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.maxStackSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				stream.ConsumeRepeatedElement();
				instance.allowedItems.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				stream.ConsumeRepeatedElement();
				instance.availableSlots.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.volume = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.allowItemsToIncreaseToMaxStackSize = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.contents.Add(Item.DeserializeLengthDelimited(stream));
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ItemContainer DeserializeLengthDelimited(BufferStream stream, ItemContainer instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.allowedItems == null)
			{
				instance.allowedItems = Pool.Get<List<int>>();
			}
			if (instance.availableSlots == null)
			{
				instance.availableSlots = Pool.Get<List<int>>();
			}
			if (instance.contents == null)
			{
				instance.contents = Pool.Get<List<Item>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemContainer");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.UID = new ItemContainerId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.slots = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.temperature = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.allowedContents = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.maxStackSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				stream.ConsumeRepeatedElement();
				instance.allowedItems.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				stream.ConsumeRepeatedElement();
				instance.availableSlots.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.volume = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.allowItemsToIncreaseToMaxStackSize = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.contents.Add(Item.DeserializeLengthDelimited(stream));
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ItemContainer DeserializeLength(BufferStream stream, int length, ItemContainer instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.allowedItems == null)
			{
				instance.allowedItems = Pool.Get<List<int>>();
			}
			if (instance.availableSlots == null)
			{
				instance.availableSlots = Pool.Get<List<int>>();
			}
			if (instance.contents == null)
			{
				instance.contents = Pool.Get<List<Item>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ItemContainer");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.UID = new ItemContainerId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.slots = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.temperature = ProtocolParser.ReadSingle(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.flags = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.allowedContents = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.maxStackSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				stream.ConsumeRepeatedElement();
				instance.allowedItems.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				stream.ConsumeRepeatedElement();
				instance.availableSlots.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.volume = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				instance.allowItemsToIncreaseToMaxStackSize = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.contents.Add(Item.DeserializeLengthDelimited(stream));
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ItemContainer instance, ItemContainer previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.UID.Value);
		if (instance.slots != previous.slots)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.slots);
		}
		if (instance.temperature != previous.temperature)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.temperature);
		}
		if (instance.flags != previous.flags)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.flags);
		}
		if (instance.allowedContents != previous.allowedContents)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.allowedContents);
		}
		if (instance.maxStackSize != previous.maxStackSize)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.maxStackSize);
		}
		if (instance.allowedItems != null)
		{
			for (int i = 0; i < instance.allowedItems.Count; i++)
			{
				int num = instance.allowedItems[i];
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
			}
		}
		if (instance.availableSlots != null)
		{
			for (int j = 0; j < instance.availableSlots.Count; j++)
			{
				int num2 = instance.availableSlots[j];
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)num2);
			}
		}
		if (instance.volume != previous.volume)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.volume);
		}
		stream.WriteByte(80);
		ProtocolParser.WriteBool(stream, instance.allowItemsToIncreaseToMaxStackSize);
		if (instance.contents == null)
		{
			return;
		}
		for (int k = 0; k < instance.contents.Count; k++)
		{
			Item item = instance.contents[k];
			stream.WriteByte(162);
			stream.WriteByte(6);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Item.SerializeDelta(stream, item, item);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num3 < 5)
			{
				span[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span[num3++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, ItemContainer instance)
	{
		if (instance.UID != default(ItemContainerId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.UID.Value);
		}
		if (instance.slots != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.slots);
		}
		if (instance.temperature != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.temperature);
		}
		if (instance.flags != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.flags);
		}
		if (instance.allowedContents != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.allowedContents);
		}
		if (instance.maxStackSize != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.maxStackSize);
		}
		if (instance.allowedItems != null)
		{
			for (int i = 0; i < instance.allowedItems.Count; i++)
			{
				int num = instance.allowedItems[i];
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
			}
		}
		if (instance.availableSlots != null)
		{
			for (int j = 0; j < instance.availableSlots.Count; j++)
			{
				int num2 = instance.availableSlots[j];
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)num2);
			}
		}
		if (instance.volume != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.volume);
		}
		if (instance.allowItemsToIncreaseToMaxStackSize)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteBool(stream, instance.allowItemsToIncreaseToMaxStackSize);
		}
		if (instance.contents == null)
		{
			return;
		}
		for (int k = 0; k < instance.contents.Count; k++)
		{
			Item instance2 = instance.contents[k];
			stream.WriteByte(162);
			stream.WriteByte(6);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Item.Serialize(stream, instance2);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num3 < 5)
			{
				span[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span[num3++] = 128;
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
		action(UidType.ItemContainerId, ref UID.Value);
		if (contents != null)
		{
			for (int i = 0; i < contents.Count; i++)
			{
				contents[i]?.InspectUids(action);
			}
		}
	}
}
