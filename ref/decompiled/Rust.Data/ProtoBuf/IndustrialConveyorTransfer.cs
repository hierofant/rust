using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class IndustrialConveyorTransfer : IDisposable, Pool.IPooled, IProto<IndustrialConveyorTransfer>, IProto
{
	public struct ItemTransfer : IProto<ItemTransfer>, IProto
	{
		[NonSerialized]
		public int itemId;

		[NonSerialized]
		public int amount;

		public static void ResetToPool(ItemTransfer instance)
		{
			instance.itemId = 0;
			instance.amount = 0;
		}

		public void CopyTo(ItemTransfer instance)
		{
			instance.itemId = itemId;
			instance.amount = amount;
		}

		public ItemTransfer Copy()
		{
			ItemTransfer itemTransfer = default(ItemTransfer);
			CopyTo(itemTransfer);
			return itemTransfer;
		}

		public static ItemTransfer Deserialize(BufferStream stream)
		{
			ItemTransfer instance = default(ItemTransfer);
			Deserialize(stream, ref instance, isDelta: false);
			return instance;
		}

		public static ItemTransfer DeserializeLengthDelimited(BufferStream stream)
		{
			ItemTransfer instance = default(ItemTransfer);
			DeserializeLengthDelimited(stream, ref instance, isDelta: false);
			return instance;
		}

		public static ItemTransfer DeserializeLength(BufferStream stream, int length)
		{
			ItemTransfer instance = default(ItemTransfer);
			DeserializeLength(stream, length, ref instance, isDelta: false);
			return instance;
		}

		public static ItemTransfer Deserialize(byte[] buffer)
		{
			ItemTransfer instance = default(ItemTransfer);
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, ref instance, isDelta: false);
			return instance;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, ref this, isDelta);
		}

		public void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void WriteToStreamDelta(BufferStream stream, ItemTransfer previous)
		{
			SerializeDelta(stream, this, previous);
		}

		public void ReadFromStream(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, ref this, isDelta);
		}

		public void ReadFromStream(BufferStream stream, int size, bool isDelta = false)
		{
			DeserializeLength(stream, size, ref this, isDelta);
		}

		public static ItemTransfer Deserialize(BufferStream stream, ref ItemTransfer instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					instance.amount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ItemTransfer DeserializeLengthDelimited(BufferStream stream, ref ItemTransfer instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					instance.amount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ItemTransfer DeserializeLength(BufferStream stream, int length, ref ItemTransfer instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					instance.amount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer.ItemTransfer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ItemTransfer instance, ItemTransfer previous)
		{
			if (instance.itemId != previous.itemId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
			}
			if (instance.amount != previous.amount)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.amount);
			}
		}

		public static void Serialize(BufferStream stream, ItemTransfer instance)
		{
			if (instance.itemId != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemId);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ItemTransfer> ItemTransfers;

	[NonSerialized]
	public List<NetworkableId> inputEntities;

	[NonSerialized]
	public List<NetworkableId> outputEntities;

	public static void ResetToPool(IndustrialConveyorTransfer instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.ItemTransfers != null)
			{
				List<ItemTransfer> obj = instance.ItemTransfers;
				Pool.FreeUnmanaged(ref obj);
				instance.ItemTransfers = obj;
			}
			if (instance.inputEntities != null)
			{
				List<NetworkableId> obj2 = instance.inputEntities;
				Pool.FreeUnmanaged(ref obj2);
				instance.inputEntities = obj2;
			}
			if (instance.outputEntities != null)
			{
				List<NetworkableId> obj3 = instance.outputEntities;
				Pool.FreeUnmanaged(ref obj3);
				instance.outputEntities = obj3;
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
			throw new Exception("Trying to dispose IndustrialConveyorTransfer with ShouldPool set to false!");
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

	public void CopyTo(IndustrialConveyorTransfer instance)
	{
		if (ItemTransfers != null)
		{
			instance.ItemTransfers = Pool.Get<List<ItemTransfer>>();
			for (int i = 0; i < ItemTransfers.Count; i++)
			{
				ItemTransfer item = ItemTransfers[i];
				instance.ItemTransfers.Add(item);
			}
		}
		else
		{
			instance.ItemTransfers = null;
		}
		if (inputEntities != null)
		{
			instance.inputEntities = Pool.Get<List<NetworkableId>>();
			for (int j = 0; j < inputEntities.Count; j++)
			{
				NetworkableId item2 = inputEntities[j];
				instance.inputEntities.Add(item2);
			}
		}
		else
		{
			instance.inputEntities = null;
		}
		if (outputEntities != null)
		{
			instance.outputEntities = Pool.Get<List<NetworkableId>>();
			for (int k = 0; k < outputEntities.Count; k++)
			{
				NetworkableId item3 = outputEntities[k];
				instance.outputEntities.Add(item3);
			}
		}
		else
		{
			instance.outputEntities = null;
		}
	}

	public IndustrialConveyorTransfer Copy()
	{
		IndustrialConveyorTransfer industrialConveyorTransfer = Pool.Get<IndustrialConveyorTransfer>();
		CopyTo(industrialConveyorTransfer);
		return industrialConveyorTransfer;
	}

	public static IndustrialConveyorTransfer Deserialize(BufferStream stream)
	{
		IndustrialConveyorTransfer industrialConveyorTransfer = Pool.Get<IndustrialConveyorTransfer>();
		Deserialize(stream, industrialConveyorTransfer, isDelta: false);
		return industrialConveyorTransfer;
	}

	public static IndustrialConveyorTransfer DeserializeLengthDelimited(BufferStream stream)
	{
		IndustrialConveyorTransfer industrialConveyorTransfer = Pool.Get<IndustrialConveyorTransfer>();
		DeserializeLengthDelimited(stream, industrialConveyorTransfer, isDelta: false);
		return industrialConveyorTransfer;
	}

	public static IndustrialConveyorTransfer DeserializeLength(BufferStream stream, int length)
	{
		IndustrialConveyorTransfer industrialConveyorTransfer = Pool.Get<IndustrialConveyorTransfer>();
		DeserializeLength(stream, length, industrialConveyorTransfer, isDelta: false);
		return industrialConveyorTransfer;
	}

	public static IndustrialConveyorTransfer Deserialize(byte[] buffer)
	{
		IndustrialConveyorTransfer industrialConveyorTransfer = Pool.Get<IndustrialConveyorTransfer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, industrialConveyorTransfer, isDelta: false);
		return industrialConveyorTransfer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, IndustrialConveyorTransfer previous)
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

	public static IndustrialConveyorTransfer Deserialize(BufferStream stream, IndustrialConveyorTransfer instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.ItemTransfers == null)
			{
				instance.ItemTransfers = Pool.Get<List<ItemTransfer>>();
			}
			if (instance.inputEntities == null)
			{
				instance.inputEntities = Pool.Get<List<NetworkableId>>();
			}
			if (instance.outputEntities == null)
			{
				instance.outputEntities = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyorTransfer");
			switch (num)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				ItemTransfer instance2 = default(ItemTransfer);
				ItemTransfer.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.ItemTransfers.Add(instance2);
				continue;
			}
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				instance.inputEntities.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				instance.outputEntities.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IndustrialConveyorTransfer DeserializeLengthDelimited(BufferStream stream, IndustrialConveyorTransfer instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.ItemTransfers == null)
			{
				instance.ItemTransfers = Pool.Get<List<ItemTransfer>>();
			}
			if (instance.inputEntities == null)
			{
				instance.inputEntities = Pool.Get<List<NetworkableId>>();
			}
			if (instance.outputEntities == null)
			{
				instance.outputEntities = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyorTransfer");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				ItemTransfer instance2 = default(ItemTransfer);
				ItemTransfer.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.ItemTransfers.Add(instance2);
				continue;
			}
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				instance.inputEntities.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				instance.outputEntities.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static IndustrialConveyorTransfer DeserializeLength(BufferStream stream, int length, IndustrialConveyorTransfer instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.ItemTransfers == null)
			{
				instance.ItemTransfers = Pool.Get<List<ItemTransfer>>();
			}
			if (instance.inputEntities == null)
			{
				instance.inputEntities = Pool.Get<List<NetworkableId>>();
			}
			if (instance.outputEntities == null)
			{
				instance.outputEntities = Pool.Get<List<NetworkableId>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.IndustrialConveyorTransfer");
			switch (num2)
			{
			case 10:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				ItemTransfer instance2 = default(ItemTransfer);
				ItemTransfer.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.ItemTransfers.Add(instance2);
				continue;
			}
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				instance.inputEntities.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				stream.ConsumeRepeatedElement();
				instance.outputEntities.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.IndustrialConveyorTransfer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, IndustrialConveyorTransfer instance, IndustrialConveyorTransfer previous)
	{
		if (instance.ItemTransfers != null)
		{
			for (int i = 0; i < instance.ItemTransfers.Count; i++)
			{
				ItemTransfer itemTransfer = instance.ItemTransfers[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ItemTransfer.SerializeDelta(stream, itemTransfer, itemTransfer);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ItemTransfers (ProtoBuf.IndustrialConveyorTransfer.ItemTransfer)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.inputEntities != null)
		{
			for (int j = 0; j < instance.inputEntities.Count; j++)
			{
				NetworkableId networkableId = instance.inputEntities[j];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.outputEntities != null)
		{
			for (int k = 0; k < instance.outputEntities.Count; k++)
			{
				NetworkableId networkableId2 = instance.outputEntities[k];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, networkableId2.Value);
			}
		}
	}

	public static void Serialize(BufferStream stream, IndustrialConveyorTransfer instance)
	{
		if (instance.ItemTransfers != null)
		{
			for (int i = 0; i < instance.ItemTransfers.Count; i++)
			{
				ItemTransfer instance2 = instance.ItemTransfers[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				ItemTransfer.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ItemTransfers (ProtoBuf.IndustrialConveyorTransfer.ItemTransfer)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.inputEntities != null)
		{
			for (int j = 0; j < instance.inputEntities.Count; j++)
			{
				NetworkableId networkableId = instance.inputEntities[j];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.outputEntities != null)
		{
			for (int k = 0; k < instance.outputEntities.Count; k++)
			{
				NetworkableId networkableId2 = instance.outputEntities[k];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, networkableId2.Value);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (ItemTransfers != null)
		{
			for (int i = 0; i < ItemTransfers.Count; i++)
			{
				ItemTransfers[i].InspectUids(action);
			}
		}
		if (inputEntities != null)
		{
			for (int j = 0; j < inputEntities.Count; j++)
			{
				NetworkableId value = inputEntities[j];
				action(UidType.NetworkableId, ref value.Value);
				inputEntities[j] = value;
			}
		}
		if (outputEntities != null)
		{
			for (int k = 0; k < outputEntities.Count; k++)
			{
				NetworkableId value2 = outputEntities[k];
				action(UidType.NetworkableId, ref value2.Value);
				outputEntities[k] = value2;
			}
		}
	}
}
