using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class AppMarker : IDisposable, Pool.IPooled, IProto<AppMarker>, IProto
{
	public class SellOrder : IDisposable, Pool.IPooled, IProto<SellOrder>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int itemId;

		[NonSerialized]
		public int quantity;

		[NonSerialized]
		public int currencyId;

		[NonSerialized]
		public int costPerItem;

		[NonSerialized]
		public int amountInStock;

		[NonSerialized]
		public bool itemIsBlueprint;

		[NonSerialized]
		public bool currencyIsBlueprint;

		[NonSerialized]
		public float itemCondition;

		[NonSerialized]
		public float itemConditionMax;

		[NonSerialized]
		public float priceMultiplier;

		[NonSerialized]
		public float receivedQuantityMultiplier;

		public static void ResetToPool(SellOrder instance)
		{
			if (instance.ShouldPool)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.currencyId = 0;
				instance.costPerItem = 0;
				instance.amountInStock = 0;
				instance.itemIsBlueprint = false;
				instance.currencyIsBlueprint = false;
				instance.itemCondition = 0f;
				instance.itemConditionMax = 0f;
				instance.priceMultiplier = 0f;
				instance.receivedQuantityMultiplier = 0f;
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
			instance.itemId = itemId;
			instance.quantity = quantity;
			instance.currencyId = currencyId;
			instance.costPerItem = costPerItem;
			instance.amountInStock = amountInStock;
			instance.itemIsBlueprint = itemIsBlueprint;
			instance.currencyIsBlueprint = currencyIsBlueprint;
			instance.itemCondition = itemCondition;
			instance.itemConditionMax = itemConditionMax;
			instance.priceMultiplier = priceMultiplier;
			instance.receivedQuantityMultiplier = receivedQuantityMultiplier;
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
			if (!isDelta)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.currencyId = 0;
				instance.costPerItem = 0;
				instance.amountInStock = 0;
				instance.itemIsBlueprint = false;
				instance.currencyIsBlueprint = false;
				instance.itemCondition = 0f;
				instance.itemConditionMax = 0f;
				instance.priceMultiplier = 1f;
				instance.receivedQuantityMultiplier = 1f;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AppMarker.SellOrder");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.quantity = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.currencyId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.costPerItem = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.amountInStock = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.currencyIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				case 69:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemCondition = ProtocolParser.ReadSingle(stream);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemConditionMax = ProtocolParser.ReadSingle(stream);
					continue;
				case 85:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.priceMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				case 93:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.receivedQuantityMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SellOrder DeserializeLengthDelimited(BufferStream stream, SellOrder instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.currencyId = 0;
				instance.costPerItem = 0;
				instance.amountInStock = 0;
				instance.itemIsBlueprint = false;
				instance.currencyIsBlueprint = false;
				instance.itemCondition = 0f;
				instance.itemConditionMax = 0f;
				instance.priceMultiplier = 1f;
				instance.receivedQuantityMultiplier = 1f;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppMarker.SellOrder");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.quantity = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.currencyId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.costPerItem = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.amountInStock = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.currencyIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				case 69:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemCondition = ProtocolParser.ReadSingle(stream);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemConditionMax = ProtocolParser.ReadSingle(stream);
					continue;
				case 85:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.priceMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				case 93:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.receivedQuantityMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SellOrder DeserializeLength(BufferStream stream, int length, SellOrder instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.itemId = 0;
				instance.quantity = 0;
				instance.currencyId = 0;
				instance.costPerItem = 0;
				instance.amountInStock = 0;
				instance.itemIsBlueprint = false;
				instance.currencyIsBlueprint = false;
				instance.itemCondition = 0f;
				instance.itemConditionMax = 0f;
				instance.priceMultiplier = 1f;
				instance.receivedQuantityMultiplier = 1f;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppMarker.SellOrder");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.quantity = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.currencyId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.costPerItem = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.amountInStock = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.currencyIsBlueprint = ProtocolParser.ReadBool(stream);
					continue;
				case 69:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemCondition = ProtocolParser.ReadSingle(stream);
					continue;
				case 77:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.itemConditionMax = ProtocolParser.ReadSingle(stream);
					continue;
				case 85:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.priceMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				case 93:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					instance.receivedQuantityMultiplier = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMarker.SellOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, SellOrder instance, SellOrder previous)
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
			if (instance.currencyId != previous.currencyId)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.currencyId);
			}
			if (instance.costPerItem != previous.costPerItem)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.costPerItem);
			}
			if (instance.amountInStock != previous.amountInStock)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.amountInStock);
			}
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.itemIsBlueprint);
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.currencyIsBlueprint);
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
			if (instance.priceMultiplier != previous.priceMultiplier)
			{
				stream.WriteByte(85);
				ProtocolParser.WriteSingle(stream, instance.priceMultiplier);
			}
			if (instance.receivedQuantityMultiplier != previous.receivedQuantityMultiplier)
			{
				stream.WriteByte(93);
				ProtocolParser.WriteSingle(stream, instance.receivedQuantityMultiplier);
			}
		}

		public static void Serialize(BufferStream stream, SellOrder instance)
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
			if (instance.currencyId != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.currencyId);
			}
			if (instance.costPerItem != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.costPerItem);
			}
			if (instance.amountInStock != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.amountInStock);
			}
			if (instance.itemIsBlueprint)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteBool(stream, instance.itemIsBlueprint);
			}
			if (instance.currencyIsBlueprint)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteBool(stream, instance.currencyIsBlueprint);
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
			if (instance.priceMultiplier != 1f)
			{
				stream.WriteByte(85);
				ProtocolParser.WriteSingle(stream, instance.priceMultiplier);
			}
			if (instance.receivedQuantityMultiplier != 1f)
			{
				stream.WriteByte(93);
				ProtocolParser.WriteSingle(stream, instance.receivedQuantityMultiplier);
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
	public NetworkableId id;

	[NonSerialized]
	public AppMarkerType type;

	[NonSerialized]
	public float x;

	[NonSerialized]
	public float y;

	[NonSerialized]
	public ulong steamId;

	[NonSerialized]
	public float rotation;

	[NonSerialized]
	public float radius;

	[NonSerialized]
	public Vector4 color1;

	[NonSerialized]
	public Vector4 color2;

	[NonSerialized]
	public float alpha;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public bool outOfStock;

	[NonSerialized]
	public List<SellOrder> sellOrders;

	public static void ResetToPool(AppMarker instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.id = default(NetworkableId);
		instance.type = AppMarkerType.Undefined;
		instance.x = 0f;
		instance.y = 0f;
		instance.steamId = 0uL;
		instance.rotation = 0f;
		instance.radius = 0f;
		instance.color1 = default(Vector4);
		instance.color2 = default(Vector4);
		instance.alpha = 0f;
		instance.name = string.Empty;
		instance.outOfStock = false;
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
			throw new Exception("Trying to dispose AppMarker with ShouldPool set to false!");
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

	public void CopyTo(AppMarker instance)
	{
		instance.id = id;
		instance.type = type;
		instance.x = x;
		instance.y = y;
		instance.steamId = steamId;
		instance.rotation = rotation;
		instance.radius = radius;
		instance.color1 = color1;
		instance.color2 = color2;
		instance.alpha = alpha;
		instance.name = name;
		instance.outOfStock = outOfStock;
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

	public AppMarker Copy()
	{
		AppMarker appMarker = Pool.Get<AppMarker>();
		CopyTo(appMarker);
		return appMarker;
	}

	public static AppMarker Deserialize(BufferStream stream)
	{
		AppMarker appMarker = Pool.Get<AppMarker>();
		Deserialize(stream, appMarker, isDelta: false);
		return appMarker;
	}

	public static AppMarker DeserializeLengthDelimited(BufferStream stream)
	{
		AppMarker appMarker = Pool.Get<AppMarker>();
		DeserializeLengthDelimited(stream, appMarker, isDelta: false);
		return appMarker;
	}

	public static AppMarker DeserializeLength(BufferStream stream, int length)
	{
		AppMarker appMarker = Pool.Get<AppMarker>();
		DeserializeLength(stream, length, appMarker, isDelta: false);
		return appMarker;
	}

	public static AppMarker Deserialize(byte[] buffer)
	{
		AppMarker appMarker = Pool.Get<AppMarker>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appMarker, isDelta: false);
		return appMarker;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppMarker previous)
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

	public static AppMarker Deserialize(BufferStream stream, AppMarker instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0f;
			instance.y = 0f;
			instance.steamId = 0uL;
			instance.rotation = 0f;
			instance.radius = 0f;
			instance.alpha = 0f;
			instance.outOfStock = false;
			if (instance.sellOrders == null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMarker");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.id = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.type = (AppMarkerType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.x = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.y = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.rotation = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.radius = ProtocolParser.ReadSingle(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.color1, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.color2, isDelta);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.alpha = ProtocolParser.ReadSingle(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.outOfStock = ProtocolParser.ReadBool(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				stream.ConsumeRepeatedElement();
				instance.sellOrders.Add(SellOrder.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppMarker DeserializeLengthDelimited(BufferStream stream, AppMarker instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0f;
			instance.y = 0f;
			instance.steamId = 0uL;
			instance.rotation = 0f;
			instance.radius = 0f;
			instance.alpha = 0f;
			instance.outOfStock = false;
			if (instance.sellOrders == null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMarker");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.id = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.type = (AppMarkerType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.x = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.y = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.rotation = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.radius = ProtocolParser.ReadSingle(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.color1, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.color2, isDelta);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.alpha = ProtocolParser.ReadSingle(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.outOfStock = ProtocolParser.ReadBool(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				stream.ConsumeRepeatedElement();
				instance.sellOrders.Add(SellOrder.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppMarker DeserializeLength(BufferStream stream, int length, AppMarker instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.x = 0f;
			instance.y = 0f;
			instance.steamId = 0uL;
			instance.rotation = 0f;
			instance.radius = 0f;
			instance.alpha = 0f;
			instance.outOfStock = false;
			if (instance.sellOrders == null)
			{
				instance.sellOrders = Pool.Get<List<SellOrder>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppMarker");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.id = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.type = (AppMarkerType)ProtocolParser.ReadUInt64(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.x = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.y = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.rotation = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.radius = ProtocolParser.ReadSingle(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.color1, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				Vector4Serialized.DeserializeLengthDelimited(stream, ref instance.color2, isDelta);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.alpha = ProtocolParser.ReadSingle(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				instance.outOfStock = ProtocolParser.ReadBool(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				stream.ConsumeRepeatedElement();
				instance.sellOrders.Add(SellOrder.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppMarker");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppMarker instance, AppMarker previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.id.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
		if (instance.x != previous.x)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.x);
		}
		if (instance.y != previous.y)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.y);
		}
		if (instance.steamId != previous.steamId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.steamId);
		}
		if (instance.rotation != previous.rotation)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.rotation);
		}
		if (instance.radius != previous.radius)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.radius);
		}
		if (instance.color1 != previous.color1)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector4Serialized.SerializeDelta(stream, instance.color1, previous.color1);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color1 (UnityEngine.Vector4)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.color2 != previous.color2)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector4Serialized.SerializeDelta(stream, instance.color2, previous.color2);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color2 (UnityEngine.Vector4)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.alpha != previous.alpha)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.alpha);
		}
		if (instance.name != null && instance.name != previous.name)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.name);
		}
		stream.WriteByte(96);
		ProtocolParser.WriteBool(stream, instance.outOfStock);
		if (instance.sellOrders == null)
		{
			return;
		}
		for (int i = 0; i < instance.sellOrders.Count; i++)
		{
			SellOrder sellOrder = instance.sellOrders[i];
			stream.WriteByte(106);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			SellOrder.SerializeDelta(stream, sellOrder, sellOrder);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sellOrders (ProtoBuf.AppMarker.SellOrder)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
	}

	public static void Serialize(BufferStream stream, AppMarker instance)
	{
		if (instance.id != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.id.Value);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
		if (instance.x != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.x);
		}
		if (instance.y != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.y);
		}
		if (instance.steamId != 0L)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.steamId);
		}
		if (instance.rotation != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.rotation);
		}
		if (instance.radius != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.radius);
		}
		if (instance.color1 != default(Vector4))
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector4Serialized.Serialize(stream, instance.color1);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color1 (UnityEngine.Vector4)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.color2 != default(Vector4))
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector4Serialized.Serialize(stream, instance.color2);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field color2 (UnityEngine.Vector4)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.alpha != 0f)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.alpha);
		}
		if (instance.name != null)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.outOfStock)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteBool(stream, instance.outOfStock);
		}
		if (instance.sellOrders == null)
		{
			return;
		}
		for (int i = 0; i < instance.sellOrders.Count; i++)
		{
			SellOrder instance2 = instance.sellOrders[i];
			stream.WriteByte(106);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			SellOrder.Serialize(stream, instance2);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sellOrders (ProtoBuf.AppMarker.SellOrder)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref id.Value);
		if (sellOrders != null)
		{
			for (int i = 0; i < sellOrders.Count; i++)
			{
				sellOrders[i]?.InspectUids(action);
			}
		}
	}
}
