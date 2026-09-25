using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class BuriedItems : IDisposable, Pool.IPooled, IProto<BuriedItems>, IProto
{
	public class StoredBuriedItem : IDisposable, Pool.IPooled, IProto<StoredBuriedItem>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemId;

		[NonSerialized]
		public ItemOwnershipAmount ownership;

		[NonSerialized]
		public ulong skinId;

		[NonSerialized]
		public long expiryTimeDiff;

		[NonSerialized]
		public Vector2 location;

		[NonSerialized]
		public float condition;

		[NonSerialized]
		public ulong uid;

		public static void ResetToPool(StoredBuriedItem instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemId = 0;
				if (instance.ownership != null)
				{
					instance.ownership.ResetToPool();
					instance.ownership = null;
				}
				instance.skinId = 0uL;
				instance.expiryTimeDiff = 0L;
				instance.location = default(Vector2);
				instance.condition = 0f;
				instance.uid = 0uL;
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
				throw new Exception("Trying to dispose StoredBuriedItem with ShouldPool set to false!");
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

		public void CopyTo(StoredBuriedItem instance)
		{
			instance.itemId = itemId;
			if (ownership != null)
			{
				if (instance.ownership == null)
				{
					instance.ownership = ownership.Copy();
				}
				else
				{
					ownership.CopyTo(instance.ownership);
				}
			}
			else
			{
				instance.ownership = null;
			}
			instance.skinId = skinId;
			instance.expiryTimeDiff = expiryTimeDiff;
			instance.location = location;
			instance.condition = condition;
			instance.uid = uid;
		}

		public StoredBuriedItem Copy()
		{
			StoredBuriedItem storedBuriedItem = Pool.Get<StoredBuriedItem>();
			CopyTo(storedBuriedItem);
			return storedBuriedItem;
		}

		public static StoredBuriedItem Deserialize(BufferStream stream)
		{
			StoredBuriedItem storedBuriedItem = Pool.Get<StoredBuriedItem>();
			Deserialize(stream, storedBuriedItem, isDelta: false);
			return storedBuriedItem;
		}

		public static StoredBuriedItem DeserializeLengthDelimited(BufferStream stream)
		{
			StoredBuriedItem storedBuriedItem = Pool.Get<StoredBuriedItem>();
			DeserializeLengthDelimited(stream, storedBuriedItem, isDelta: false);
			return storedBuriedItem;
		}

		public static StoredBuriedItem DeserializeLength(BufferStream stream, int length)
		{
			StoredBuriedItem storedBuriedItem = Pool.Get<StoredBuriedItem>();
			DeserializeLength(stream, length, storedBuriedItem, isDelta: false);
			return storedBuriedItem;
		}

		public static StoredBuriedItem Deserialize(byte[] buffer)
		{
			StoredBuriedItem storedBuriedItem = Pool.Get<StoredBuriedItem>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, storedBuriedItem, isDelta: false);
			return storedBuriedItem;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, StoredBuriedItem previous)
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

		public static StoredBuriedItem Deserialize(BufferStream stream, StoredBuriedItem instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.BuriedItems.StoredBuriedItem");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					if (instance.ownership == null)
					{
						instance.ownership = ItemOwnershipAmount.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemOwnershipAmount.DeserializeLengthDelimited(stream, instance.ownership, isDelta);
					}
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.skinId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.expiryTimeDiff = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					Vector2Serialized.DeserializeLengthDelimited(stream, ref instance.location, isDelta);
					continue;
				case 53:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.condition = ProtocolParser.ReadSingle(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.uid = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static StoredBuriedItem DeserializeLengthDelimited(BufferStream stream, StoredBuriedItem instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.BuriedItems.StoredBuriedItem");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					if (instance.ownership == null)
					{
						instance.ownership = ItemOwnershipAmount.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemOwnershipAmount.DeserializeLengthDelimited(stream, instance.ownership, isDelta);
					}
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.skinId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.expiryTimeDiff = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					Vector2Serialized.DeserializeLengthDelimited(stream, ref instance.location, isDelta);
					continue;
				case 53:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.condition = ProtocolParser.ReadSingle(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.uid = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static StoredBuriedItem DeserializeLength(BufferStream stream, int length, StoredBuriedItem instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.BuriedItems.StoredBuriedItem");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					if (instance.ownership == null)
					{
						instance.ownership = ItemOwnershipAmount.DeserializeLengthDelimited(stream);
					}
					else
					{
						ItemOwnershipAmount.DeserializeLengthDelimited(stream, instance.ownership, isDelta);
					}
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.skinId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.expiryTimeDiff = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					Vector2Serialized.DeserializeLengthDelimited(stream, ref instance.location, isDelta);
					continue;
				case 53:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.condition = ProtocolParser.ReadSingle(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					instance.uid = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuriedItems.StoredBuriedItem");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, StoredBuriedItem instance, StoredBuriedItem previous)
		{
			if (instance.itemId != previous.itemId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.ownership != null)
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ItemOwnershipAmount.SerializeDelta(stream, instance.ownership, previous.ownership);
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
			if (instance.skinId != previous.skinId)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.skinId);
			}
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.expiryTimeDiff);
			if (instance.location != previous.location)
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector2Serialized.SerializeDelta(stream, instance.location, previous.location);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field location (UnityEngine.Vector2)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.condition != previous.condition)
			{
				stream.WriteByte(53);
				ProtocolParser.WriteSingle(stream, instance.condition);
			}
			if (instance.uid != previous.uid)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, instance.uid);
			}
		}

		public static void Serialize(BufferStream stream, StoredBuriedItem instance)
		{
			if (instance.itemId != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.ownership != null)
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ItemOwnershipAmount.Serialize(stream, instance.ownership);
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
			if (instance.skinId != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.skinId);
			}
			if (instance.expiryTimeDiff != 0L)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.expiryTimeDiff);
			}
			if (instance.location != default(Vector2))
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector2Serialized.Serialize(stream, instance.location);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field location (UnityEngine.Vector2)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.condition != 0f)
			{
				stream.WriteByte(53);
				ProtocolParser.WriteSingle(stream, instance.condition);
			}
			if (instance.uid != 0L)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, instance.uid);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			ownership?.InspectUids(action);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<StoredBuriedItem> buriedItems;

	public static void ResetToPool(BuriedItems instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.buriedItems != null)
		{
			for (int i = 0; i < instance.buriedItems.Count; i++)
			{
				if (instance.buriedItems[i] != null)
				{
					instance.buriedItems[i].ResetToPool();
					instance.buriedItems[i] = null;
				}
			}
			List<StoredBuriedItem> obj = instance.buriedItems;
			Pool.Free(ref obj, freeElements: false);
			instance.buriedItems = obj;
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
			throw new Exception("Trying to dispose BuriedItems with ShouldPool set to false!");
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

	public void CopyTo(BuriedItems instance)
	{
		if (buriedItems != null)
		{
			instance.buriedItems = Pool.Get<List<StoredBuriedItem>>();
			for (int i = 0; i < buriedItems.Count; i++)
			{
				StoredBuriedItem item = buriedItems[i].Copy();
				instance.buriedItems.Add(item);
			}
		}
		else
		{
			instance.buriedItems = null;
		}
	}

	public BuriedItems Copy()
	{
		BuriedItems buriedItems = Pool.Get<BuriedItems>();
		CopyTo(buriedItems);
		return buriedItems;
	}

	public static BuriedItems Deserialize(BufferStream stream)
	{
		BuriedItems buriedItems = Pool.Get<BuriedItems>();
		Deserialize(stream, buriedItems, isDelta: false);
		return buriedItems;
	}

	public static BuriedItems DeserializeLengthDelimited(BufferStream stream)
	{
		BuriedItems buriedItems = Pool.Get<BuriedItems>();
		DeserializeLengthDelimited(stream, buriedItems, isDelta: false);
		return buriedItems;
	}

	public static BuriedItems DeserializeLength(BufferStream stream, int length)
	{
		BuriedItems buriedItems = Pool.Get<BuriedItems>();
		DeserializeLength(stream, length, buriedItems, isDelta: false);
		return buriedItems;
	}

	public static BuriedItems Deserialize(byte[] buffer)
	{
		BuriedItems buriedItems = Pool.Get<BuriedItems>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, buriedItems, isDelta: false);
		return buriedItems;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BuriedItems previous)
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

	public static BuriedItems Deserialize(BufferStream stream, BuriedItems instance, bool isDelta)
	{
		if (!isDelta && instance.buriedItems == null)
		{
			instance.buriedItems = Pool.Get<List<StoredBuriedItem>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuriedItems");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				stream.ConsumeRepeatedElement();
				instance.buriedItems.Add(StoredBuriedItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BuriedItems DeserializeLengthDelimited(BufferStream stream, BuriedItems instance, bool isDelta)
	{
		if (!isDelta && instance.buriedItems == null)
		{
			instance.buriedItems = Pool.Get<List<StoredBuriedItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BuriedItems");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				stream.ConsumeRepeatedElement();
				instance.buriedItems.Add(StoredBuriedItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BuriedItems DeserializeLength(BufferStream stream, int length, BuriedItems instance, bool isDelta)
	{
		if (!isDelta && instance.buriedItems == null)
		{
			instance.buriedItems = Pool.Get<List<StoredBuriedItem>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.BuriedItems");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				stream.ConsumeRepeatedElement();
				instance.buriedItems.Add(StoredBuriedItem.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuriedItems");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BuriedItems instance, BuriedItems previous)
	{
		if (instance.buriedItems == null)
		{
			return;
		}
		for (int i = 0; i < instance.buriedItems.Count; i++)
		{
			StoredBuriedItem storedBuriedItem = instance.buriedItems[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			StoredBuriedItem.SerializeDelta(stream, storedBuriedItem, storedBuriedItem);
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

	public static void Serialize(BufferStream stream, BuriedItems instance)
	{
		if (instance.buriedItems == null)
		{
			return;
		}
		for (int i = 0; i < instance.buriedItems.Count; i++)
		{
			StoredBuriedItem instance2 = instance.buriedItems[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			StoredBuriedItem.Serialize(stream, instance2);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (buriedItems != null)
		{
			for (int i = 0; i < buriedItems.Count; i++)
			{
				buriedItems[i]?.InspectUids(action);
			}
		}
	}
}
