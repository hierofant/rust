using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppEntityPayload : IDisposable, Pool.IPooled, IProto<AppEntityPayload>, IProto
{
	public class Item : IDisposable, Pool.IPooled, IProto<Item>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemId;

		[NonSerialized]
		public int quantity;

		[NonSerialized]
		public bool itemIsBlueprint;

		public static void ResetToPool(Item instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.itemIsBlueprint = false;
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
				throw new Exception("Trying to dispose Item with ShouldPool set to false!");
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

		public void CopyTo(Item instance)
		{
			instance.itemId = itemId;
			instance.quantity = quantity;
			instance.itemIsBlueprint = itemIsBlueprint;
		}

		public Item Copy()
		{
			Item item = Pool.Get<Item>();
			CopyTo(item);
			return item;
		}

		public static Item Deserialize(BufferStream stream)
		{
			Item item = Pool.Get<Item>();
			Deserialize(stream, item, isDelta: false);
			return item;
		}

		public static Item DeserializeLengthDelimited(BufferStream stream)
		{
			Item item = Pool.Get<Item>();
			DeserializeLengthDelimited(stream, item, isDelta: false);
			return item;
		}

		public static Item DeserializeLength(BufferStream stream, int length)
		{
			Item item = Pool.Get<Item>();
			DeserializeLength(stream, length, item, isDelta: false);
			return item;
		}

		public static Item Deserialize(byte[] buffer)
		{
			Item item = Pool.Get<Item>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, item, isDelta: false);
			return item;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Item previous)
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

		public static Item Deserialize(BufferStream stream, Item instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.itemIsBlueprint = false;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AppEntityPayload.Item");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.quantity = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.itemIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Item DeserializeLengthDelimited(BufferStream stream, Item instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.itemIsBlueprint = false;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppEntityPayload.Item");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.quantity = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.itemIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Item DeserializeLength(BufferStream stream, int length, Item instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.itemIsBlueprint = false;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppEntityPayload.Item");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.quantity = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					instance.itemIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload.Item");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Item instance, Item previous)
		{
			if (instance.itemId != previous.itemId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.quantity != previous.quantity)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.quantity);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.itemIsBlueprint);
		}

		public static void Serialize(BufferStream stream, Item instance)
		{
			if (instance.itemId != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.quantity != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.quantity);
			}
			if (instance.itemIsBlueprint)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteBool(stream, instance.itemIsBlueprint);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool value;

	[NonSerialized]
	public List<Item> items;

	[NonSerialized]
	public int capacity;

	[NonSerialized]
	public bool hasProtection;

	[NonSerialized]
	public uint protectionExpiry;

	public static void ResetToPool(AppEntityPayload instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.value = false;
		if (instance.items != null)
		{
			for (int i = 0; i < instance.items.Count; i++)
			{
				if (instance.items[i] != null)
				{
					instance.items[i].ResetToPool();
					instance.items[i] = null;
				}
			}
			List<Item> obj = instance.items;
			Pool.Free(ref obj, freeElements: false);
			instance.items = obj;
		}
		instance.capacity = 0;
		instance.hasProtection = false;
		instance.protectionExpiry = 0u;
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
			throw new Exception("Trying to dispose AppEntityPayload with ShouldPool set to false!");
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

	public void CopyTo(AppEntityPayload instance)
	{
		instance.value = value;
		if (items != null)
		{
			instance.items = Pool.Get<List<Item>>();
			for (int i = 0; i < items.Count; i++)
			{
				Item item = items[i].Copy();
				instance.items.Add(item);
			}
		}
		else
		{
			instance.items = null;
		}
		instance.capacity = capacity;
		instance.hasProtection = hasProtection;
		instance.protectionExpiry = protectionExpiry;
	}

	public AppEntityPayload Copy()
	{
		AppEntityPayload appEntityPayload = Pool.Get<AppEntityPayload>();
		CopyTo(appEntityPayload);
		return appEntityPayload;
	}

	public static AppEntityPayload Deserialize(BufferStream stream)
	{
		AppEntityPayload appEntityPayload = Pool.Get<AppEntityPayload>();
		Deserialize(stream, appEntityPayload, isDelta: false);
		return appEntityPayload;
	}

	public static AppEntityPayload DeserializeLengthDelimited(BufferStream stream)
	{
		AppEntityPayload appEntityPayload = Pool.Get<AppEntityPayload>();
		DeserializeLengthDelimited(stream, appEntityPayload, isDelta: false);
		return appEntityPayload;
	}

	public static AppEntityPayload DeserializeLength(BufferStream stream, int length)
	{
		AppEntityPayload appEntityPayload = Pool.Get<AppEntityPayload>();
		DeserializeLength(stream, length, appEntityPayload, isDelta: false);
		return appEntityPayload;
	}

	public static AppEntityPayload Deserialize(byte[] buffer)
	{
		AppEntityPayload appEntityPayload = Pool.Get<AppEntityPayload>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appEntityPayload, isDelta: false);
		return appEntityPayload;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppEntityPayload previous)
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

	public static AppEntityPayload Deserialize(BufferStream stream, AppEntityPayload instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.value = false;
			if (instance.items == null)
			{
				instance.items = Pool.Get<List<Item>>();
			}
			instance.capacity = 0;
			instance.protectionExpiry = 0u;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppEntityPayload");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.value = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				stream.ConsumeRepeatedElement();
				instance.items.Add(Item.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.capacity = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.hasProtection = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.protectionExpiry = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppEntityPayload DeserializeLengthDelimited(BufferStream stream, AppEntityPayload instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.value = false;
			if (instance.items == null)
			{
				instance.items = Pool.Get<List<Item>>();
			}
			instance.capacity = 0;
			instance.protectionExpiry = 0u;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppEntityPayload");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.value = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				stream.ConsumeRepeatedElement();
				instance.items.Add(Item.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.capacity = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.hasProtection = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.protectionExpiry = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppEntityPayload DeserializeLength(BufferStream stream, int length, AppEntityPayload instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.value = false;
			if (instance.items == null)
			{
				instance.items = Pool.Get<List<Item>>();
			}
			instance.capacity = 0;
			instance.protectionExpiry = 0u;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppEntityPayload");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.value = ProtocolParser.ReadBool(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				stream.ConsumeRepeatedElement();
				instance.items.Add(Item.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.capacity = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.hasProtection = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				instance.protectionExpiry = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEntityPayload");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppEntityPayload instance, AppEntityPayload previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.value);
		if (instance.items != null)
		{
			for (int i = 0; i < instance.items.Count; i++)
			{
				Item item = instance.items[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Item.SerializeDelta(stream, item, item);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field items (ProtoBuf.AppEntityPayload.Item)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.capacity != previous.capacity)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.capacity);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.hasProtection);
		if (instance.protectionExpiry != previous.protectionExpiry)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.protectionExpiry);
		}
	}

	public static void Serialize(BufferStream stream, AppEntityPayload instance)
	{
		if (instance.value)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.value);
		}
		if (instance.items != null)
		{
			for (int i = 0; i < instance.items.Count; i++)
			{
				Item instance2 = instance.items[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Item.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field items (ProtoBuf.AppEntityPayload.Item)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.capacity != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.capacity);
		}
		if (instance.hasProtection)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.hasProtection);
		}
		if (instance.protectionExpiry != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.protectionExpiry);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (items != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i]?.InspectUids(action);
			}
		}
	}
}
