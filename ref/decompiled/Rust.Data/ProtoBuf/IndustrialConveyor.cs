using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class IndustrialConveyor : IDisposable, Pool.IPooled, IProto<IndustrialConveyor>, IProto
{
	public class ItemFilter : IDisposable, Pool.IPooled, IProto<ItemFilter>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemDef;

		[NonSerialized]
		public int itemCategory;

		[NonSerialized]
		public int maxAmountInDestination;

		[NonSerialized]
		public int isBlueprint;

		[NonSerialized]
		public int bufferAmount;

		[NonSerialized]
		public int retainMinimum;

		[NonSerialized]
		public int bufferTransferRemaining;

		public static void ResetToPool(ItemFilter instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemDef = 0;
				instance.itemCategory = 0;
				instance.maxAmountInDestination = 0;
				instance.isBlueprint = 0;
				instance.bufferAmount = 0;
				instance.retainMinimum = 0;
				instance.bufferTransferRemaining = 0;
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
				throw new Exception("Trying to dispose ItemFilter with ShouldPool set to false!");
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

		public void CopyTo(ItemFilter instance)
		{
			instance.itemDef = itemDef;
			instance.itemCategory = itemCategory;
			instance.maxAmountInDestination = maxAmountInDestination;
			instance.isBlueprint = isBlueprint;
			instance.bufferAmount = bufferAmount;
			instance.retainMinimum = retainMinimum;
			instance.bufferTransferRemaining = bufferTransferRemaining;
		}

		public ItemFilter Copy()
		{
			ItemFilter itemFilter = Pool.Get<ItemFilter>();
			CopyTo(itemFilter);
			return itemFilter;
		}

		public static ItemFilter Deserialize(BufferStream stream)
		{
			ItemFilter itemFilter = Pool.Get<ItemFilter>();
			Deserialize(stream, itemFilter, isDelta: false);
			return itemFilter;
		}

		public static ItemFilter DeserializeLengthDelimited(BufferStream stream)
		{
			ItemFilter itemFilter = Pool.Get<ItemFilter>();
			DeserializeLengthDelimited(stream, itemFilter, isDelta: false);
			return itemFilter;
		}

		public static ItemFilter DeserializeLength(BufferStream stream, int length)
		{
			ItemFilter itemFilter = Pool.Get<ItemFilter>();
			DeserializeLength(stream, length, itemFilter, isDelta: false);
			return itemFilter;
		}

		public static ItemFilter Deserialize(byte[] buffer)
		{
			ItemFilter itemFilter = Pool.Get<ItemFilter>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, itemFilter, isDelta: false);
			return itemFilter;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ItemFilter previous)
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

		public static ItemFilter Deserialize(BufferStream stream, ItemFilter instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor.ItemFilter");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.itemDef = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.itemCategory = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.maxAmountInDestination = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.isBlueprint = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.bufferAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.retainMinimum = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.bufferTransferRemaining = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ItemFilter DeserializeLengthDelimited(BufferStream stream, ItemFilter instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor.ItemFilter");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.itemDef = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.itemCategory = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.maxAmountInDestination = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.isBlueprint = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.bufferAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.retainMinimum = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.bufferTransferRemaining = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ItemFilter DeserializeLength(BufferStream stream, int length, ItemFilter instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor.ItemFilter");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.itemDef = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.itemCategory = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.maxAmountInDestination = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.isBlueprint = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.bufferAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.retainMinimum = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					instance.bufferTransferRemaining = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilter");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ItemFilter instance, ItemFilter previous)
		{
			if (instance.itemDef != previous.itemDef)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemDef);
			}
			if (instance.itemCategory != previous.itemCategory)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemCategory);
			}
			if (instance.maxAmountInDestination != previous.maxAmountInDestination)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.maxAmountInDestination);
			}
			if (instance.isBlueprint != previous.isBlueprint)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.isBlueprint);
			}
			if (instance.bufferAmount != previous.bufferAmount)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.bufferAmount);
			}
			if (instance.retainMinimum != previous.retainMinimum)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.retainMinimum);
			}
			if (instance.bufferTransferRemaining != previous.bufferTransferRemaining)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.bufferTransferRemaining);
			}
		}

		public static void Serialize(BufferStream stream, ItemFilter instance)
		{
			if (instance.itemDef != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemDef);
			}
			if (instance.itemCategory != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemCategory);
			}
			if (instance.maxAmountInDestination != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.maxAmountInDestination);
			}
			if (instance.isBlueprint != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.isBlueprint);
			}
			if (instance.bufferAmount != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.bufferAmount);
			}
			if (instance.retainMinimum != 0)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.retainMinimum);
			}
			if (instance.bufferTransferRemaining != 0)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.bufferTransferRemaining);
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

	public class ItemFilterList : IDisposable, Pool.IPooled, IProto<ItemFilterList>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<ItemFilter> filters;

		public static void ResetToPool(ItemFilterList instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			if (instance.filters != null)
			{
				for (int i = 0; i < instance.filters.Count; i++)
				{
					if (instance.filters[i] != null)
					{
						instance.filters[i].ResetToPool();
						instance.filters[i] = null;
					}
				}
				List<ItemFilter> obj = instance.filters;
				Pool.Free(ref obj, freeElements: false);
				instance.filters = obj;
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
				throw new Exception("Trying to dispose ItemFilterList with ShouldPool set to false!");
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

		public void CopyTo(ItemFilterList instance)
		{
			if (filters != null)
			{
				instance.filters = Pool.Get<List<ItemFilter>>();
				for (int i = 0; i < filters.Count; i++)
				{
					ItemFilter item = filters[i].Copy();
					instance.filters.Add(item);
				}
			}
			else
			{
				instance.filters = null;
			}
		}

		public ItemFilterList Copy()
		{
			ItemFilterList itemFilterList = Pool.Get<ItemFilterList>();
			CopyTo(itemFilterList);
			return itemFilterList;
		}

		public static ItemFilterList Deserialize(BufferStream stream)
		{
			ItemFilterList itemFilterList = Pool.Get<ItemFilterList>();
			Deserialize(stream, itemFilterList, isDelta: false);
			return itemFilterList;
		}

		public static ItemFilterList DeserializeLengthDelimited(BufferStream stream)
		{
			ItemFilterList itemFilterList = Pool.Get<ItemFilterList>();
			DeserializeLengthDelimited(stream, itemFilterList, isDelta: false);
			return itemFilterList;
		}

		public static ItemFilterList DeserializeLength(BufferStream stream, int length)
		{
			ItemFilterList itemFilterList = Pool.Get<ItemFilterList>();
			DeserializeLength(stream, length, itemFilterList, isDelta: false);
			return itemFilterList;
		}

		public static ItemFilterList Deserialize(byte[] buffer)
		{
			ItemFilterList itemFilterList = Pool.Get<ItemFilterList>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, itemFilterList, isDelta: false);
			return itemFilterList;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ItemFilterList previous)
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

		public static ItemFilterList Deserialize(BufferStream stream, ItemFilterList instance, bool isDelta)
		{
			if (!isDelta && instance.filters == null)
			{
				instance.filters = Pool.Get<List<ItemFilter>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor.ItemFilterList");
				if (num == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					stream.ConsumeRepeatedElement();
					instance.filters.Add(ItemFilter.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static ItemFilterList DeserializeLengthDelimited(BufferStream stream, ItemFilterList instance, bool isDelta)
		{
			if (!isDelta && instance.filters == null)
			{
				instance.filters = Pool.Get<List<ItemFilter>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor.ItemFilterList");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					stream.ConsumeRepeatedElement();
					instance.filters.Add(ItemFilter.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static ItemFilterList DeserializeLength(BufferStream stream, int length, ItemFilterList instance, bool isDelta)
		{
			if (!isDelta && instance.filters == null)
			{
				instance.filters = Pool.Get<List<ItemFilter>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor.ItemFilterList");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					stream.ConsumeRepeatedElement();
					instance.filters.Add(ItemFilter.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor.ItemFilterList");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ItemFilterList instance, ItemFilterList previous)
		{
			if (instance.filters == null)
			{
				return;
			}
			for (int i = 0; i < instance.filters.Count; i++)
			{
				ItemFilter itemFilter = instance.filters[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ItemFilter.SerializeDelta(stream, itemFilter, itemFilter);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field filters (ProtoBuf.IndustrialConveyor.ItemFilter)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}

		public static void Serialize(BufferStream stream, ItemFilterList instance)
		{
			if (instance.filters == null)
			{
				return;
			}
			for (int i = 0; i < instance.filters.Count; i++)
			{
				ItemFilter instance2 = instance.filters[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ItemFilter.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field filters (ProtoBuf.IndustrialConveyor.ItemFilter)");
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
			if (filters != null)
			{
				for (int i = 0; i < filters.Count; i++)
				{
					filters[i]?.InspectUids(action);
				}
			}
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ItemFilter> filters;

	[NonSerialized]
	public int conveyorMode;

	public static void ResetToPool(IndustrialConveyor instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.filters != null)
		{
			for (int i = 0; i < instance.filters.Count; i++)
			{
				if (instance.filters[i] != null)
				{
					instance.filters[i].ResetToPool();
					instance.filters[i] = null;
				}
			}
			List<ItemFilter> obj = instance.filters;
			Pool.Free(ref obj, freeElements: false);
			instance.filters = obj;
		}
		instance.conveyorMode = 0;
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
			throw new Exception("Trying to dispose IndustrialConveyor with ShouldPool set to false!");
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

	public void CopyTo(IndustrialConveyor instance)
	{
		if (filters != null)
		{
			instance.filters = Pool.Get<List<ItemFilter>>();
			for (int i = 0; i < filters.Count; i++)
			{
				ItemFilter item = filters[i].Copy();
				instance.filters.Add(item);
			}
		}
		else
		{
			instance.filters = null;
		}
		instance.conveyorMode = conveyorMode;
	}

	public IndustrialConveyor Copy()
	{
		IndustrialConveyor industrialConveyor = Pool.Get<IndustrialConveyor>();
		CopyTo(industrialConveyor);
		return industrialConveyor;
	}

	public static IndustrialConveyor Deserialize(BufferStream stream)
	{
		IndustrialConveyor industrialConveyor = Pool.Get<IndustrialConveyor>();
		Deserialize(stream, industrialConveyor, isDelta: false);
		return industrialConveyor;
	}

	public static IndustrialConveyor DeserializeLengthDelimited(BufferStream stream)
	{
		IndustrialConveyor industrialConveyor = Pool.Get<IndustrialConveyor>();
		DeserializeLengthDelimited(stream, industrialConveyor, isDelta: false);
		return industrialConveyor;
	}

	public static IndustrialConveyor DeserializeLength(BufferStream stream, int length)
	{
		IndustrialConveyor industrialConveyor = Pool.Get<IndustrialConveyor>();
		DeserializeLength(stream, length, industrialConveyor, isDelta: false);
		return industrialConveyor;
	}

	public static IndustrialConveyor Deserialize(byte[] buffer)
	{
		IndustrialConveyor industrialConveyor = Pool.Get<IndustrialConveyor>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, industrialConveyor, isDelta: false);
		return industrialConveyor;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, IndustrialConveyor previous)
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

	public static IndustrialConveyor Deserialize(BufferStream stream, IndustrialConveyor instance, bool isDelta)
	{
		if (!isDelta && instance.filters == null)
		{
			instance.filters = Pool.Get<List<ItemFilter>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				if (instance.filters.Count >= 30)
				{
					throw new ProtocolBufferException("Repeated field filters exceeded max count of 30");
				}
				stream.ConsumeRepeatedElement();
				instance.filters.Add(ItemFilter.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor");
				instance.conveyorMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IndustrialConveyor DeserializeLengthDelimited(BufferStream stream, IndustrialConveyor instance, bool isDelta)
	{
		if (!isDelta && instance.filters == null)
		{
			instance.filters = Pool.Get<List<ItemFilter>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				if (instance.filters.Count >= 30)
				{
					throw new ProtocolBufferException("Repeated field filters exceeded max count of 30");
				}
				stream.ConsumeRepeatedElement();
				instance.filters.Add(ItemFilter.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor");
				instance.conveyorMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IndustrialConveyor DeserializeLength(BufferStream stream, int length, IndustrialConveyor instance, bool isDelta)
	{
		if (!isDelta && instance.filters == null)
		{
			instance.filters = Pool.Get<List<ItemFilter>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				if (instance.filters.Count >= 30)
				{
					throw new ProtocolBufferException("Repeated field filters exceeded max count of 30");
				}
				stream.ConsumeRepeatedElement();
				instance.filters.Add(ItemFilter.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor");
				instance.conveyorMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, IndustrialConveyor instance, IndustrialConveyor previous)
	{
		if (instance.filters != null)
		{
			if (instance.filters.Count > 30)
			{
				throw new InvalidOperationException("Repeated field filters exceeded max count of 30");
			}
			for (int i = 0; i < instance.filters.Count; i++)
			{
				ItemFilter itemFilter = instance.filters[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ItemFilter.SerializeDelta(stream, itemFilter, itemFilter);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field filters (ProtoBuf.IndustrialConveyor.ItemFilter)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.conveyorMode != previous.conveyorMode)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.conveyorMode);
		}
	}

	public static void Serialize(BufferStream stream, IndustrialConveyor instance)
	{
		if (instance.filters != null)
		{
			if (instance.filters.Count > 30)
			{
				throw new InvalidOperationException("Repeated field filters exceeded max count of 30");
			}
			for (int i = 0; i < instance.filters.Count; i++)
			{
				ItemFilter instance2 = instance.filters[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ItemFilter.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field filters (ProtoBuf.IndustrialConveyor.ItemFilter)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.conveyorMode != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.conveyorMode);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (filters != null)
		{
			for (int i = 0; i < filters.Count; i++)
			{
				filters[i]?.InspectUids(action);
			}
		}
	}
}
