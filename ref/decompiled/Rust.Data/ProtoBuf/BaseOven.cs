using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BaseOven : IDisposable, Pool.IPooled, IProto<BaseOven>, IProto
{
	public class CookingItem : IDisposable, Pool.IPooled, IProto<CookingItem>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemID;

		[NonSerialized]
		public int slotIndex;

		[NonSerialized]
		public int initialStackSize;

		[NonSerialized]
		public float cookingProgress;

		public static void ResetToPool(CookingItem instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemID = 0;
				instance.slotIndex = 0;
				instance.initialStackSize = 0;
				instance.cookingProgress = 0f;
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
				throw new Exception("Trying to dispose CookingItem with ShouldPool set to false!");
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

		public void CopyTo(CookingItem instance)
		{
			instance.itemID = itemID;
			instance.slotIndex = slotIndex;
			instance.initialStackSize = initialStackSize;
			instance.cookingProgress = cookingProgress;
		}

		public CookingItem Copy()
		{
			CookingItem cookingItem = Pool.Get<CookingItem>();
			CopyTo(cookingItem);
			return cookingItem;
		}

		public static CookingItem Deserialize(BufferStream stream)
		{
			CookingItem cookingItem = Pool.Get<CookingItem>();
			Deserialize(stream, cookingItem, isDelta: false);
			return cookingItem;
		}

		public static CookingItem DeserializeLengthDelimited(BufferStream stream)
		{
			CookingItem cookingItem = Pool.Get<CookingItem>();
			DeserializeLengthDelimited(stream, cookingItem, isDelta: false);
			return cookingItem;
		}

		public static CookingItem DeserializeLength(BufferStream stream, int length)
		{
			CookingItem cookingItem = Pool.Get<CookingItem>();
			DeserializeLength(stream, length, cookingItem, isDelta: false);
			return cookingItem;
		}

		public static CookingItem Deserialize(byte[] buffer)
		{
			CookingItem cookingItem = Pool.Get<CookingItem>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, cookingItem, isDelta: false);
			return cookingItem;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, CookingItem previous)
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

		public static CookingItem Deserialize(BufferStream stream, CookingItem instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.BaseOven.CookingItem");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.slotIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.initialStackSize = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.cookingProgress = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static CookingItem DeserializeLengthDelimited(BufferStream stream, CookingItem instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.BaseOven.CookingItem");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.slotIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.initialStackSize = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.cookingProgress = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static CookingItem DeserializeLength(BufferStream stream, int length, CookingItem instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.BaseOven.CookingItem");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.slotIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.initialStackSize = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					instance.cookingProgress = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseOven.CookingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, CookingItem instance, CookingItem previous)
		{
			if (instance.itemID != previous.itemID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
			}
			if (instance.slotIndex != previous.slotIndex)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.slotIndex);
			}
			if (instance.initialStackSize != previous.initialStackSize)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.initialStackSize);
			}
			if (instance.cookingProgress != previous.cookingProgress)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.cookingProgress);
			}
		}

		public static void Serialize(BufferStream stream, CookingItem instance)
		{
			if (instance.itemID != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
			}
			if (instance.slotIndex != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.slotIndex);
			}
			if (instance.initialStackSize != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.initialStackSize);
			}
			if (instance.cookingProgress != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.cookingProgress);
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
	public float cookSpeed;

	[NonSerialized]
	public List<CookingItem> cookingItems;

	public static void ResetToPool(BaseOven instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.cookSpeed = 0f;
		if (instance.cookingItems != null)
		{
			for (int i = 0; i < instance.cookingItems.Count; i++)
			{
				if (instance.cookingItems[i] != null)
				{
					instance.cookingItems[i].ResetToPool();
					instance.cookingItems[i] = null;
				}
			}
			List<CookingItem> obj = instance.cookingItems;
			Pool.Free(ref obj, freeElements: false);
			instance.cookingItems = obj;
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
			throw new Exception("Trying to dispose BaseOven with ShouldPool set to false!");
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

	public void CopyTo(BaseOven instance)
	{
		instance.cookSpeed = cookSpeed;
		if (cookingItems != null)
		{
			instance.cookingItems = Pool.Get<List<CookingItem>>();
			for (int i = 0; i < cookingItems.Count; i++)
			{
				CookingItem item = cookingItems[i].Copy();
				instance.cookingItems.Add(item);
			}
		}
		else
		{
			instance.cookingItems = null;
		}
	}

	public BaseOven Copy()
	{
		BaseOven baseOven = Pool.Get<BaseOven>();
		CopyTo(baseOven);
		return baseOven;
	}

	public static BaseOven Deserialize(BufferStream stream)
	{
		BaseOven baseOven = Pool.Get<BaseOven>();
		Deserialize(stream, baseOven, isDelta: false);
		return baseOven;
	}

	public static BaseOven DeserializeLengthDelimited(BufferStream stream)
	{
		BaseOven baseOven = Pool.Get<BaseOven>();
		DeserializeLengthDelimited(stream, baseOven, isDelta: false);
		return baseOven;
	}

	public static BaseOven DeserializeLength(BufferStream stream, int length)
	{
		BaseOven baseOven = Pool.Get<BaseOven>();
		DeserializeLength(stream, length, baseOven, isDelta: false);
		return baseOven;
	}

	public static BaseOven Deserialize(byte[] buffer)
	{
		BaseOven baseOven = Pool.Get<BaseOven>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, baseOven, isDelta: false);
		return baseOven;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BaseOven previous)
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

	public static BaseOven Deserialize(BufferStream stream, BaseOven instance, bool isDelta)
	{
		if (!isDelta && instance.cookingItems == null)
		{
			instance.cookingItems = Pool.Get<List<CookingItem>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BaseOven");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven");
				instance.cookSpeed = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				stream.ConsumeRepeatedElement();
				instance.cookingItems.Add(CookingItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BaseOven DeserializeLengthDelimited(BufferStream stream, BaseOven instance, bool isDelta)
	{
		if (!isDelta && instance.cookingItems == null)
		{
			instance.cookingItems = Pool.Get<List<CookingItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseOven");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven");
				instance.cookSpeed = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				stream.ConsumeRepeatedElement();
				instance.cookingItems.Add(CookingItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BaseOven DeserializeLength(BufferStream stream, int length, BaseOven instance, bool isDelta)
	{
		if (!isDelta && instance.cookingItems == null)
		{
			instance.cookingItems = Pool.Get<List<CookingItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseOven");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven");
				instance.cookSpeed = ProtocolParser.ReadSingle(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				stream.ConsumeRepeatedElement();
				instance.cookingItems.Add(CookingItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseOven");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BaseOven instance, BaseOven previous)
	{
		if (instance.cookSpeed != previous.cookSpeed)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.cookSpeed);
		}
		if (instance.cookingItems == null)
		{
			return;
		}
		for (int i = 0; i < instance.cookingItems.Count; i++)
		{
			CookingItem cookingItem = instance.cookingItems[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			CookingItem.SerializeDelta(stream, cookingItem, cookingItem);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cookingItems (ProtoBuf.BaseOven.CookingItem)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, BaseOven instance)
	{
		if (instance.cookSpeed != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.cookSpeed);
		}
		if (instance.cookingItems == null)
		{
			return;
		}
		for (int i = 0; i < instance.cookingItems.Count; i++)
		{
			CookingItem instance2 = instance.cookingItems[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			CookingItem.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cookingItems (ProtoBuf.BaseOven.CookingItem)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (cookingItems != null)
		{
			for (int i = 0; i < cookingItems.Count; i++)
			{
				cookingItems[i]?.InspectUids(action);
			}
		}
	}
}
