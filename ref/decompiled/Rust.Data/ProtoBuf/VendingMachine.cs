using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VendingMachine : IDisposable, Pool.IPooled, IProto<VendingMachine>, IProto
{
	public class SellOrder : IDisposable, Pool.IPooled, IProto<SellOrder>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemToSellID;

		[NonSerialized]
		public int itemToSellAmount;

		[NonSerialized]
		public int currencyID;

		[NonSerialized]
		public int currencyAmountPerItem;

		[NonSerialized]
		public int inStock;

		[NonSerialized]
		public bool currencyIsBP;

		[NonSerialized]
		public bool itemToSellIsBP;

		[NonSerialized]
		public float itemCondition;

		[NonSerialized]
		public float itemConditionMax;

		[NonSerialized]
		public int instanceData;

		[NonSerialized]
		public List<int> attachmentsList;

		[NonSerialized]
		public int totalAttachmentSlots;

		[NonSerialized]
		public float priceMultiplier;

		[NonSerialized]
		public int ammoType;

		[NonSerialized]
		public int ammoCount;

		[NonSerialized]
		public float receivedQuantityMultiplier;

		[NonSerialized]
		public ulong sellSkinId;

		[NonSerialized]
		public ulong costSkinId;

		public static void ResetToPool(SellOrder instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemToSellID = 0;
				instance.itemToSellAmount = 0;
				instance.currencyID = 0;
				instance.currencyAmountPerItem = 0;
				instance.inStock = 0;
				instance.currencyIsBP = false;
				instance.itemToSellIsBP = false;
				instance.itemCondition = 0f;
				instance.itemConditionMax = 0f;
				instance.instanceData = 0;
				if (instance.attachmentsList != null)
				{
					List<int> obj = instance.attachmentsList;
					Pool.FreeUnmanaged(ref obj);
					instance.attachmentsList = obj;
				}
				instance.totalAttachmentSlots = 0;
				instance.priceMultiplier = 0f;
				instance.ammoType = 0;
				instance.ammoCount = 0;
				instance.receivedQuantityMultiplier = 0f;
				instance.sellSkinId = 0uL;
				instance.costSkinId = 0uL;
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
				throw new Exception("Trying to dispose SellOrder with ShouldPool set to false!");
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

		public void CopyTo(SellOrder instance)
		{
			instance.itemToSellID = itemToSellID;
			instance.itemToSellAmount = itemToSellAmount;
			instance.currencyID = currencyID;
			instance.currencyAmountPerItem = currencyAmountPerItem;
			instance.inStock = inStock;
			instance.currencyIsBP = currencyIsBP;
			instance.itemToSellIsBP = itemToSellIsBP;
			instance.itemCondition = itemCondition;
			instance.itemConditionMax = itemConditionMax;
			instance.instanceData = instanceData;
			if (attachmentsList != null)
			{
				instance.attachmentsList = Pool.Get<List<int>>();
				for (int i = 0; i < attachmentsList.Count; i++)
				{
					int item = attachmentsList[i];
					instance.attachmentsList.Add(item);
				}
			}
			else
			{
				instance.attachmentsList = null;
			}
			instance.totalAttachmentSlots = totalAttachmentSlots;
			instance.priceMultiplier = priceMultiplier;
			instance.ammoType = ammoType;
			instance.ammoCount = ammoCount;
			instance.receivedQuantityMultiplier = receivedQuantityMultiplier;
			instance.sellSkinId = sellSkinId;
			instance.costSkinId = costSkinId;
		}

		public SellOrder Copy()
		{
			SellOrder sellOrder = Pool.Get<SellOrder>();
			CopyTo(sellOrder);
			return sellOrder;
		}

		public static SellOrder Deserialize(BufferStream stream)
		{
			SellOrder sellOrder = Pool.Get<SellOrder>();
			Deserialize(stream, sellOrder, isDelta: false);
			return sellOrder;
		}

		public static SellOrder DeserializeLengthDelimited(BufferStream stream)
		{
			SellOrder sellOrder = Pool.Get<SellOrder>();
			DeserializeLengthDelimited(stream, sellOrder, isDelta: false);
			return sellOrder;
		}

		public static SellOrder DeserializeLength(BufferStream stream, int length)
		{
			SellOrder sellOrder = Pool.Get<SellOrder>();
			DeserializeLength(stream, length, sellOrder, isDelta: false);
			return sellOrder;
		}

		public static SellOrder Deserialize(byte[] buffer)
		{
			SellOrder sellOrder = Pool.Get<SellOrder>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, sellOrder, isDelta: false);
			return sellOrder;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, SellOrder previous)
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

		public static SellOrder Deserialize(BufferStream stream, SellOrder instance, bool isDelta)
		{
			if (!isDelta && instance.attachmentsList == null)
			{
				instance.attachmentsList = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.VendingMachine.SellOrder");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyAmountPerItem = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.inStock = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyIsBP = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellIsBP = ProtocolParser.ReadBool(stream);
					continue;
				case 69:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemCondition = ProtocolParser.ReadSingle(stream);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemConditionMax = ProtocolParser.ReadSingle(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.instanceData = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					stream.ConsumeRepeatedElement();
					instance.attachmentsList.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.totalAttachmentSlots = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 109:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.priceMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				case 112:
					stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.ammoType = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 120:
					stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.ammoCount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 14u:
					stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 15u:
					stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 16u:
					stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Fixed32)
					{
						instance.receivedQuantityMultiplier = ProtocolParser.ReadSingle(stream);
					}
					break;
				case 17u:
					stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Varint)
					{
						instance.sellSkinId = ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 18u:
					stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Varint)
					{
						instance.costSkinId = ProtocolParser.ReadUInt64(stream);
					}
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SellOrder DeserializeLengthDelimited(BufferStream stream, SellOrder instance, bool isDelta)
		{
			if (!isDelta && instance.attachmentsList == null)
			{
				instance.attachmentsList = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.VendingMachine.SellOrder");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyAmountPerItem = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.inStock = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyIsBP = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellIsBP = ProtocolParser.ReadBool(stream);
					continue;
				case 69:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemCondition = ProtocolParser.ReadSingle(stream);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemConditionMax = ProtocolParser.ReadSingle(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.instanceData = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					stream.ConsumeRepeatedElement();
					instance.attachmentsList.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.totalAttachmentSlots = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 109:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.priceMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				case 112:
					stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.ammoType = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 120:
					stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.ammoCount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 14u:
					stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 15u:
					stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 16u:
					stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Fixed32)
					{
						instance.receivedQuantityMultiplier = ProtocolParser.ReadSingle(stream);
					}
					break;
				case 17u:
					stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Varint)
					{
						instance.sellSkinId = ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 18u:
					stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Varint)
					{
						instance.costSkinId = ProtocolParser.ReadUInt64(stream);
					}
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SellOrder DeserializeLength(BufferStream stream, int length, SellOrder instance, bool isDelta)
		{
			if (!isDelta && instance.attachmentsList == null)
			{
				instance.attachmentsList = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.VendingMachine.SellOrder");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellAmount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyAmountPerItem = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.inStock = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.currencyIsBP = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemToSellIsBP = ProtocolParser.ReadBool(stream);
					continue;
				case 69:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemCondition = ProtocolParser.ReadSingle(stream);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.itemConditionMax = ProtocolParser.ReadSingle(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.instanceData = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					stream.ConsumeRepeatedElement();
					instance.attachmentsList.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.totalAttachmentSlots = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 109:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.priceMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				case 112:
					stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.ammoType = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 120:
					stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					instance.ammoCount = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 14u:
					stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 15u:
					stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 16u:
					stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Fixed32)
					{
						instance.receivedQuantityMultiplier = ProtocolParser.ReadSingle(stream);
					}
					break;
				case 17u:
					stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Varint)
					{
						instance.sellSkinId = ProtocolParser.ReadUInt64(stream);
					}
					break;
				case 18u:
					stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.VendingMachine.SellOrder");
					if (key.WireType == Wire.Varint)
					{
						instance.costSkinId = ProtocolParser.ReadUInt64(stream);
					}
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, SellOrder instance, SellOrder previous)
		{
			if (instance.itemToSellID != previous.itemToSellID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemToSellID);
			}
			if (instance.itemToSellAmount != previous.itemToSellAmount)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemToSellAmount);
			}
			if (instance.currencyID != previous.currencyID)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.currencyID);
			}
			if (instance.currencyAmountPerItem != previous.currencyAmountPerItem)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.currencyAmountPerItem);
			}
			if (instance.inStock != previous.inStock)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.inStock);
			}
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.currencyIsBP);
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.itemToSellIsBP);
			if (instance.itemCondition != previous.itemCondition)
			{
				stream.WriteByte(69);
				ProtocolParser.WriteSingle(stream, instance.itemCondition);
			}
			if (instance.itemConditionMax != previous.itemConditionMax)
			{
				stream.WriteByte(77);
				ProtocolParser.WriteSingle(stream, instance.itemConditionMax);
			}
			if (instance.instanceData != previous.instanceData)
			{
				stream.WriteByte(80);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.instanceData);
			}
			if (instance.attachmentsList != null)
			{
				for (int i = 0; i < instance.attachmentsList.Count; i++)
				{
					int num = instance.attachmentsList[i];
					stream.WriteByte(88);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.totalAttachmentSlots != previous.totalAttachmentSlots)
			{
				stream.WriteByte(96);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.totalAttachmentSlots);
			}
			if (instance.priceMultiplier != previous.priceMultiplier)
			{
				stream.WriteByte(109);
				ProtocolParser.WriteSingle(stream, instance.priceMultiplier);
			}
			if (instance.ammoType != previous.ammoType)
			{
				stream.WriteByte(112);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoType);
			}
			if (instance.ammoCount != previous.ammoCount)
			{
				stream.WriteByte(120);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoCount);
			}
			if (instance.receivedQuantityMultiplier != previous.receivedQuantityMultiplier)
			{
				stream.WriteByte(133);
				stream.WriteByte(1);
				ProtocolParser.WriteSingle(stream, instance.receivedQuantityMultiplier);
			}
			if (instance.sellSkinId != previous.sellSkinId)
			{
				stream.WriteByte(136);
				stream.WriteByte(1);
				ProtocolParser.WriteUInt64(stream, instance.sellSkinId);
			}
			if (instance.costSkinId != previous.costSkinId)
			{
				stream.WriteByte(144);
				stream.WriteByte(1);
				ProtocolParser.WriteUInt64(stream, instance.costSkinId);
			}
		}

		public static void Serialize(BufferStream stream, SellOrder instance)
		{
			if (instance.itemToSellID != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemToSellID);
			}
			if (instance.itemToSellAmount != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.itemToSellAmount);
			}
			if (instance.currencyID != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.currencyID);
			}
			if (instance.currencyAmountPerItem != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.currencyAmountPerItem);
			}
			if (instance.inStock != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.inStock);
			}
			if (instance.currencyIsBP)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteBool(stream, instance.currencyIsBP);
			}
			if (instance.itemToSellIsBP)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteBool(stream, instance.itemToSellIsBP);
			}
			if (instance.itemCondition != 0f)
			{
				stream.WriteByte(69);
				ProtocolParser.WriteSingle(stream, instance.itemCondition);
			}
			if (instance.itemConditionMax != 0f)
			{
				stream.WriteByte(77);
				ProtocolParser.WriteSingle(stream, instance.itemConditionMax);
			}
			if (instance.instanceData != 0)
			{
				stream.WriteByte(80);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.instanceData);
			}
			if (instance.attachmentsList != null)
			{
				for (int i = 0; i < instance.attachmentsList.Count; i++)
				{
					int num = instance.attachmentsList[i];
					stream.WriteByte(88);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.totalAttachmentSlots != 0)
			{
				stream.WriteByte(96);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.totalAttachmentSlots);
			}
			if (instance.priceMultiplier != 0f)
			{
				stream.WriteByte(109);
				ProtocolParser.WriteSingle(stream, instance.priceMultiplier);
			}
			if (instance.ammoType != 0)
			{
				stream.WriteByte(112);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoType);
			}
			if (instance.ammoCount != 0)
			{
				stream.WriteByte(120);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoCount);
			}
			if (instance.receivedQuantityMultiplier != 0f)
			{
				stream.WriteByte(133);
				stream.WriteByte(1);
				ProtocolParser.WriteSingle(stream, instance.receivedQuantityMultiplier);
			}
			if (instance.sellSkinId != 0L)
			{
				stream.WriteByte(136);
				stream.WriteByte(1);
				ProtocolParser.WriteUInt64(stream, instance.sellSkinId);
			}
			if (instance.costSkinId != 0L)
			{
				stream.WriteByte(144);
				stream.WriteByte(1);
				ProtocolParser.WriteUInt64(stream, instance.costSkinId);
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

	public class SellOrderContainer : IDisposable, Pool.IPooled, IProto<SellOrderContainer>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<SellOrder> sellOrders;

		public static void ResetToPool(SellOrderContainer instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			if (instance.sellOrders != null)
			{
				for (int i = 0; i < instance.sellOrders.Count; i++)
				{
					if (instance.sellOrders[i] != null)
					{
						instance.sellOrders[i].ResetToPool();
						instance.sellOrders[i] = null;
					}
				}
				List<SellOrder> obj = instance.sellOrders;
				Pool.Free(ref obj, freeElements: false);
				instance.sellOrders = obj;
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
				throw new Exception("Trying to dispose SellOrderContainer with ShouldPool set to false!");
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

		public void CopyTo(SellOrderContainer instance)
		{
			if (sellOrders != null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
				for (int i = 0; i < sellOrders.Count; i++)
				{
					SellOrder item = sellOrders[i].Copy();
					instance.sellOrders.Add(item);
				}
			}
			else
			{
				instance.sellOrders = null;
			}
		}

		public SellOrderContainer Copy()
		{
			SellOrderContainer sellOrderContainer = Pool.Get<SellOrderContainer>();
			CopyTo(sellOrderContainer);
			return sellOrderContainer;
		}

		public static SellOrderContainer Deserialize(BufferStream stream)
		{
			SellOrderContainer sellOrderContainer = Pool.Get<SellOrderContainer>();
			Deserialize(stream, sellOrderContainer, isDelta: false);
			return sellOrderContainer;
		}

		public static SellOrderContainer DeserializeLengthDelimited(BufferStream stream)
		{
			SellOrderContainer sellOrderContainer = Pool.Get<SellOrderContainer>();
			DeserializeLengthDelimited(stream, sellOrderContainer, isDelta: false);
			return sellOrderContainer;
		}

		public static SellOrderContainer DeserializeLength(BufferStream stream, int length)
		{
			SellOrderContainer sellOrderContainer = Pool.Get<SellOrderContainer>();
			DeserializeLength(stream, length, sellOrderContainer, isDelta: false);
			return sellOrderContainer;
		}

		public static SellOrderContainer Deserialize(byte[] buffer)
		{
			SellOrderContainer sellOrderContainer = Pool.Get<SellOrderContainer>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, sellOrderContainer, isDelta: false);
			return sellOrderContainer;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, SellOrderContainer previous)
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

		public static SellOrderContainer Deserialize(BufferStream stream, SellOrderContainer instance, bool isDelta)
		{
			if (!isDelta && instance.sellOrders == null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.VendingMachine.SellOrderContainer");
				if (num == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					stream.ConsumeRepeatedElement();
					instance.sellOrders.Add(SellOrder.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static SellOrderContainer DeserializeLengthDelimited(BufferStream stream, SellOrderContainer instance, bool isDelta)
		{
			if (!isDelta && instance.sellOrders == null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.VendingMachine.SellOrderContainer");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					stream.ConsumeRepeatedElement();
					instance.sellOrders.Add(SellOrder.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static SellOrderContainer DeserializeLength(BufferStream stream, int length, SellOrderContainer instance, bool isDelta)
		{
			if (!isDelta && instance.sellOrders == null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.VendingMachine.SellOrderContainer");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					stream.ConsumeRepeatedElement();
					instance.sellOrders.Add(SellOrder.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine.SellOrderContainer");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, SellOrderContainer instance, SellOrderContainer previous)
		{
			if (instance.sellOrders == null)
			{
				return;
			}
			for (int i = 0; i < instance.sellOrders.Count; i++)
			{
				SellOrder sellOrder = instance.sellOrders[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				SellOrder.SerializeDelta(stream, sellOrder, sellOrder);
				int num = stream.Position - position;
				if (num > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sellOrders (ProtoBuf.VendingMachine.SellOrder)");
				}
				Span<byte> span = range.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
				if (num2 < 3)
				{
					span[num2 - 1] |= 128;
					while (num2 < 2)
					{
						span[num2++] = 128;
					}
					span[2] = 0;
				}
			}
		}

		public static void Serialize(BufferStream stream, SellOrderContainer instance)
		{
			if (instance.sellOrders == null)
			{
				return;
			}
			for (int i = 0; i < instance.sellOrders.Count; i++)
			{
				SellOrder instance2 = instance.sellOrders[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				SellOrder.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sellOrders (ProtoBuf.VendingMachine.SellOrder)");
				}
				Span<byte> span = range.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
				if (num2 < 3)
				{
					span[num2 - 1] |= 128;
					while (num2 < 2)
					{
						span[num2++] = 128;
					}
					span[2] = 0;
				}
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			if (sellOrders != null)
			{
				for (int i = 0; i < sellOrders.Count; i++)
				{
					sellOrders[i]?.InspectUids(action);
				}
			}
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public SellOrderContainer sellOrderContainer;

	[NonSerialized]
	public string shopName;

	[NonSerialized]
	public int vmoIndex;

	[NonSerialized]
	public NetworkableId networkID;

	[NonSerialized]
	public string translationToken;

	[NonSerialized]
	public ulong nameLastEditedBy;

	[NonSerialized]
	public bool droneAccessible;

	[NonSerialized]
	public bool inDeepSea;

	public static void ResetToPool(VendingMachine instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.sellOrderContainer != null)
			{
				instance.sellOrderContainer.ResetToPool();
				instance.sellOrderContainer = null;
			}
			instance.shopName = string.Empty;
			instance.vmoIndex = 0;
			instance.networkID = default(NetworkableId);
			instance.translationToken = string.Empty;
			instance.nameLastEditedBy = 0uL;
			instance.droneAccessible = false;
			instance.inDeepSea = false;
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
			throw new Exception("Trying to dispose VendingMachine with ShouldPool set to false!");
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

	public void CopyTo(VendingMachine instance)
	{
		if (sellOrderContainer != null)
		{
			if (instance.sellOrderContainer == null)
			{
				instance.sellOrderContainer = sellOrderContainer.Copy();
			}
			else
			{
				sellOrderContainer.CopyTo(instance.sellOrderContainer);
			}
		}
		else
		{
			instance.sellOrderContainer = null;
		}
		instance.shopName = shopName;
		instance.vmoIndex = vmoIndex;
		instance.networkID = networkID;
		instance.translationToken = translationToken;
		instance.nameLastEditedBy = nameLastEditedBy;
		instance.droneAccessible = droneAccessible;
		instance.inDeepSea = inDeepSea;
	}

	public VendingMachine Copy()
	{
		VendingMachine vendingMachine = Pool.Get<VendingMachine>();
		CopyTo(vendingMachine);
		return vendingMachine;
	}

	public static VendingMachine Deserialize(BufferStream stream)
	{
		VendingMachine vendingMachine = Pool.Get<VendingMachine>();
		Deserialize(stream, vendingMachine, isDelta: false);
		return vendingMachine;
	}

	public static VendingMachine DeserializeLengthDelimited(BufferStream stream)
	{
		VendingMachine vendingMachine = Pool.Get<VendingMachine>();
		DeserializeLengthDelimited(stream, vendingMachine, isDelta: false);
		return vendingMachine;
	}

	public static VendingMachine DeserializeLength(BufferStream stream, int length)
	{
		VendingMachine vendingMachine = Pool.Get<VendingMachine>();
		DeserializeLength(stream, length, vendingMachine, isDelta: false);
		return vendingMachine;
	}

	public static VendingMachine Deserialize(byte[] buffer)
	{
		VendingMachine vendingMachine = Pool.Get<VendingMachine>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vendingMachine, isDelta: false);
		return vendingMachine;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VendingMachine previous)
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

	public static VendingMachine Deserialize(BufferStream stream, VendingMachine instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachine");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				if (instance.sellOrderContainer == null)
				{
					instance.sellOrderContainer = SellOrderContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					SellOrderContainer.DeserializeLengthDelimited(stream, instance.sellOrderContainer, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.shopName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.vmoIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.networkID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.translationToken = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.nameLastEditedBy = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.droneAccessible = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.inDeepSea = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachine DeserializeLengthDelimited(BufferStream stream, VendingMachine instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachine");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				if (instance.sellOrderContainer == null)
				{
					instance.sellOrderContainer = SellOrderContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					SellOrderContainer.DeserializeLengthDelimited(stream, instance.sellOrderContainer, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.shopName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.vmoIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.networkID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.translationToken = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.nameLastEditedBy = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.droneAccessible = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.inDeepSea = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachine DeserializeLength(BufferStream stream, int length, VendingMachine instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachine");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				if (instance.sellOrderContainer == null)
				{
					instance.sellOrderContainer = SellOrderContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					SellOrderContainer.DeserializeLengthDelimited(stream, instance.sellOrderContainer, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.shopName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.vmoIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.networkID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.translationToken = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.nameLastEditedBy = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.droneAccessible = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				instance.inDeepSea = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VendingMachine instance, VendingMachine previous)
	{
		if (instance.sellOrderContainer != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			SellOrderContainer.SerializeDelta(stream, instance.sellOrderContainer, previous.sellOrderContainer);
			int num = stream.Position - position;
			if (num > 34359738367L)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sellOrderContainer (ProtoBuf.VendingMachine.SellOrderContainer)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 5)
			{
				span[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span[num2++] = 128;
				}
				span[4] = 0;
			}
		}
		if (instance.shopName != null && instance.shopName != previous.shopName)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.shopName);
		}
		if (instance.vmoIndex != previous.vmoIndex)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.vmoIndex);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.networkID.Value);
		if (instance.translationToken != null && instance.translationToken != previous.translationToken)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.translationToken);
		}
		if (instance.nameLastEditedBy != previous.nameLastEditedBy)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.nameLastEditedBy);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.droneAccessible);
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.inDeepSea);
	}

	public static void Serialize(BufferStream stream, VendingMachine instance)
	{
		if (instance.sellOrderContainer != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			SellOrderContainer.Serialize(stream, instance.sellOrderContainer);
			int num = stream.Position - position;
			if (num > 34359738367L)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sellOrderContainer (ProtoBuf.VendingMachine.SellOrderContainer)");
			}
			Span<byte> span = range.GetSpan();
			int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
			if (num2 < 5)
			{
				span[num2 - 1] |= 128;
				while (num2 < 4)
				{
					span[num2++] = 128;
				}
				span[4] = 0;
			}
		}
		if (instance.shopName != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.shopName);
		}
		if (instance.vmoIndex != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.vmoIndex);
		}
		if (instance.networkID != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.networkID.Value);
		}
		if (instance.translationToken != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.translationToken);
		}
		if (instance.nameLastEditedBy != 0L)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.nameLastEditedBy);
		}
		if (instance.droneAccessible)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.droneAccessible);
		}
		if (instance.inDeepSea)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.inDeepSea);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		sellOrderContainer?.InspectUids(action);
		action(UidType.NetworkableId, ref networkID.Value);
	}
}
