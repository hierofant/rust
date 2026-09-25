using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Mannequin : IDisposable, Pool.IPooled, IProto<Mannequin>, IProto
{
	public class ClothingItem : IDisposable, Pool.IPooled, IProto<ClothingItem>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemId;

		[NonSerialized]
		public ulong skin;

		[NonSerialized]
		public ItemId uid;

		[NonSerialized]
		public int reserved_int_0;

		public static void ResetToPool(ClothingItem instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemId = 0;
				instance.skin = 0uL;
				instance.uid = default(ItemId);
				instance.reserved_int_0 = 0;
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
				throw new Exception("Trying to dispose ClothingItem with ShouldPool set to false!");
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

		public void CopyTo(ClothingItem instance)
		{
			instance.itemId = itemId;
			instance.skin = skin;
			instance.uid = uid;
			instance.reserved_int_0 = reserved_int_0;
		}

		public ClothingItem Copy()
		{
			ClothingItem clothingItem = Pool.Get<ClothingItem>();
			CopyTo(clothingItem);
			return clothingItem;
		}

		public static ClothingItem Deserialize(BufferStream stream)
		{
			ClothingItem clothingItem = Pool.Get<ClothingItem>();
			Deserialize(stream, clothingItem, isDelta: false);
			return clothingItem;
		}

		public static ClothingItem DeserializeLengthDelimited(BufferStream stream)
		{
			ClothingItem clothingItem = Pool.Get<ClothingItem>();
			DeserializeLengthDelimited(stream, clothingItem, isDelta: false);
			return clothingItem;
		}

		public static ClothingItem DeserializeLength(BufferStream stream, int length)
		{
			ClothingItem clothingItem = Pool.Get<ClothingItem>();
			DeserializeLength(stream, length, clothingItem, isDelta: false);
			return clothingItem;
		}

		public static ClothingItem Deserialize(byte[] buffer)
		{
			ClothingItem clothingItem = Pool.Get<ClothingItem>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, clothingItem, isDelta: false);
			return clothingItem;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ClothingItem previous)
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

		public static ClothingItem Deserialize(BufferStream stream, ClothingItem instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.Mannequin.ClothingItem");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.skin = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.uid = new ItemId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.reserved_int_0 = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ClothingItem DeserializeLengthDelimited(BufferStream stream, ClothingItem instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.Mannequin.ClothingItem");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.skin = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.uid = new ItemId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.reserved_int_0 = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ClothingItem DeserializeLength(BufferStream stream, int length, ClothingItem instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.Mannequin.ClothingItem");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.skin = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.uid = new ItemId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					instance.reserved_int_0 = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mannequin.ClothingItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ClothingItem instance, ClothingItem previous)
		{
			if (instance.itemId != previous.itemId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.skin != previous.skin)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.skin);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.uid.Value);
			if (instance.reserved_int_0 != previous.reserved_int_0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.reserved_int_0);
			}
		}

		public static void Serialize(BufferStream stream, ClothingItem instance)
		{
			if (instance.itemId != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.skin != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.skin);
			}
			if (instance.uid != default(ItemId))
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.uid.Value);
			}
			if (instance.reserved_int_0 != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.reserved_int_0);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			action(UidType.ItemId, ref uid.Value);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ClothingItem> clothingItems;

	public static void ResetToPool(Mannequin instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.clothingItems != null)
		{
			for (int i = 0; i < instance.clothingItems.Count; i++)
			{
				if (instance.clothingItems[i] != null)
				{
					instance.clothingItems[i].ResetToPool();
					instance.clothingItems[i] = null;
				}
			}
			List<ClothingItem> obj = instance.clothingItems;
			Pool.Free(ref obj, freeElements: false);
			instance.clothingItems = obj;
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
			throw new Exception("Trying to dispose Mannequin with ShouldPool set to false!");
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

	public void CopyTo(Mannequin instance)
	{
		if (clothingItems != null)
		{
			instance.clothingItems = Pool.Get<List<ClothingItem>>();
			for (int i = 0; i < clothingItems.Count; i++)
			{
				ClothingItem item = clothingItems[i].Copy();
				instance.clothingItems.Add(item);
			}
		}
		else
		{
			instance.clothingItems = null;
		}
	}

	public Mannequin Copy()
	{
		Mannequin mannequin = Pool.Get<Mannequin>();
		CopyTo(mannequin);
		return mannequin;
	}

	public static Mannequin Deserialize(BufferStream stream)
	{
		Mannequin mannequin = Pool.Get<Mannequin>();
		Deserialize(stream, mannequin, isDelta: false);
		return mannequin;
	}

	public static Mannequin DeserializeLengthDelimited(BufferStream stream)
	{
		Mannequin mannequin = Pool.Get<Mannequin>();
		DeserializeLengthDelimited(stream, mannequin, isDelta: false);
		return mannequin;
	}

	public static Mannequin DeserializeLength(BufferStream stream, int length)
	{
		Mannequin mannequin = Pool.Get<Mannequin>();
		DeserializeLength(stream, length, mannequin, isDelta: false);
		return mannequin;
	}

	public static Mannequin Deserialize(byte[] buffer)
	{
		Mannequin mannequin = Pool.Get<Mannequin>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, mannequin, isDelta: false);
		return mannequin;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Mannequin previous)
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

	public static Mannequin Deserialize(BufferStream stream, Mannequin instance, bool isDelta)
	{
		if (!isDelta && instance.clothingItems == null)
		{
			instance.clothingItems = Pool.Get<List<ClothingItem>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Mannequin");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				stream.ConsumeRepeatedElement();
				instance.clothingItems.Add(ClothingItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Mannequin DeserializeLengthDelimited(BufferStream stream, Mannequin instance, bool isDelta)
	{
		if (!isDelta && instance.clothingItems == null)
		{
			instance.clothingItems = Pool.Get<List<ClothingItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Mannequin");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				stream.ConsumeRepeatedElement();
				instance.clothingItems.Add(ClothingItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static Mannequin DeserializeLength(BufferStream stream, int length, Mannequin instance, bool isDelta)
	{
		if (!isDelta && instance.clothingItems == null)
		{
			instance.clothingItems = Pool.Get<List<ClothingItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Mannequin");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				stream.ConsumeRepeatedElement();
				instance.clothingItems.Add(ClothingItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Mannequin");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Mannequin instance, Mannequin previous)
	{
		if (instance.clothingItems == null)
		{
			return;
		}
		for (int i = 0; i < instance.clothingItems.Count; i++)
		{
			ClothingItem clothingItem = instance.clothingItems[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ClothingItem.SerializeDelta(stream, clothingItem, clothingItem);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field clothingItems (ProtoBuf.Mannequin.ClothingItem)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, Mannequin instance)
	{
		if (instance.clothingItems == null)
		{
			return;
		}
		for (int i = 0; i < instance.clothingItems.Count; i++)
		{
			ClothingItem instance2 = instance.clothingItems[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ClothingItem.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field clothingItems (ProtoBuf.Mannequin.ClothingItem)");
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
		if (clothingItems != null)
		{
			for (int i = 0; i < clothingItems.Count; i++)
			{
				clothingItems[i]?.InspectUids(action);
			}
		}
	}
}
