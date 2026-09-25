using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MarketTerminal : IDisposable, Pool.IPooled, IProto<MarketTerminal>, IProto
{
	public class PendingOrder : IDisposable, Pool.IPooled, IProto<PendingOrder>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public NetworkableId vendingMachineId;

		[NonSerialized]
		public NetworkableId droneId;

		[NonSerialized]
		public float timeUntilExpiry;

		public static void ResetToPool(PendingOrder instance)
		{
			if (instance.ShouldPool)
			{
				instance.vendingMachineId = default(NetworkableId);
				instance.droneId = default(NetworkableId);
				instance.timeUntilExpiry = 0f;
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
				throw new Exception("Trying to dispose PendingOrder with ShouldPool set to false!");
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

		public void CopyTo(PendingOrder instance)
		{
			instance.vendingMachineId = vendingMachineId;
			instance.droneId = droneId;
			instance.timeUntilExpiry = timeUntilExpiry;
		}

		public PendingOrder Copy()
		{
			PendingOrder pendingOrder = Pool.Get<PendingOrder>();
			CopyTo(pendingOrder);
			return pendingOrder;
		}

		public static PendingOrder Deserialize(BufferStream stream)
		{
			PendingOrder pendingOrder = Pool.Get<PendingOrder>();
			Deserialize(stream, pendingOrder, isDelta: false);
			return pendingOrder;
		}

		public static PendingOrder DeserializeLengthDelimited(BufferStream stream)
		{
			PendingOrder pendingOrder = Pool.Get<PendingOrder>();
			DeserializeLengthDelimited(stream, pendingOrder, isDelta: false);
			return pendingOrder;
		}

		public static PendingOrder DeserializeLength(BufferStream stream, int length)
		{
			PendingOrder pendingOrder = Pool.Get<PendingOrder>();
			DeserializeLength(stream, length, pendingOrder, isDelta: false);
			return pendingOrder;
		}

		public static PendingOrder Deserialize(byte[] buffer)
		{
			PendingOrder pendingOrder = Pool.Get<PendingOrder>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, pendingOrder, isDelta: false);
			return pendingOrder;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, PendingOrder previous)
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

		public static PendingOrder Deserialize(BufferStream stream, PendingOrder instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.MarketTerminal.PendingOrder");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.vendingMachineId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.droneId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.timeUntilExpiry = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PendingOrder DeserializeLengthDelimited(BufferStream stream, PendingOrder instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.MarketTerminal.PendingOrder");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.vendingMachineId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.droneId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.timeUntilExpiry = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PendingOrder DeserializeLength(BufferStream stream, int length, PendingOrder instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.MarketTerminal.PendingOrder");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.vendingMachineId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.droneId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					instance.timeUntilExpiry = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MarketTerminal.PendingOrder");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, PendingOrder instance, PendingOrder previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.vendingMachineId.Value);
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.droneId.Value);
			if (instance.timeUntilExpiry != previous.timeUntilExpiry)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.timeUntilExpiry);
			}
		}

		public static void Serialize(BufferStream stream, PendingOrder instance)
		{
			if (instance.vendingMachineId != default(NetworkableId))
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.vendingMachineId.Value);
			}
			if (instance.droneId != default(NetworkableId))
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.droneId.Value);
			}
			if (instance.timeUntilExpiry != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.timeUntilExpiry);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			action(UidType.NetworkableId, ref vendingMachineId.Value);
			action(UidType.NetworkableId, ref droneId.Value);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong customerSteamId;

	[NonSerialized]
	public NetworkableId marketplaceId;

	[NonSerialized]
	public List<PendingOrder> orders;

	[NonSerialized]
	public string customerName;

	[NonSerialized]
	public float timeUntilExpiry;

	[NonSerialized]
	public int deliveryFeeCurrency;

	[NonSerialized]
	public int deliveryFeeAmount;

	public static void ResetToPool(MarketTerminal instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.customerSteamId = 0uL;
		instance.marketplaceId = default(NetworkableId);
		if (instance.orders != null)
		{
			for (int i = 0; i < instance.orders.Count; i++)
			{
				if (instance.orders[i] != null)
				{
					instance.orders[i].ResetToPool();
					instance.orders[i] = null;
				}
			}
			List<PendingOrder> obj = instance.orders;
			Pool.Free(ref obj, freeElements: false);
			instance.orders = obj;
		}
		instance.customerName = string.Empty;
		instance.timeUntilExpiry = 0f;
		instance.deliveryFeeCurrency = 0;
		instance.deliveryFeeAmount = 0;
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
			throw new Exception("Trying to dispose MarketTerminal with ShouldPool set to false!");
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

	public void CopyTo(MarketTerminal instance)
	{
		instance.customerSteamId = customerSteamId;
		instance.marketplaceId = marketplaceId;
		if (orders != null)
		{
			instance.orders = Pool.Get<List<PendingOrder>>();
			for (int i = 0; i < orders.Count; i++)
			{
				PendingOrder item = orders[i].Copy();
				instance.orders.Add(item);
			}
		}
		else
		{
			instance.orders = null;
		}
		instance.customerName = customerName;
		instance.timeUntilExpiry = timeUntilExpiry;
		instance.deliveryFeeCurrency = deliveryFeeCurrency;
		instance.deliveryFeeAmount = deliveryFeeAmount;
	}

	public MarketTerminal Copy()
	{
		MarketTerminal marketTerminal = Pool.Get<MarketTerminal>();
		CopyTo(marketTerminal);
		return marketTerminal;
	}

	public static MarketTerminal Deserialize(BufferStream stream)
	{
		MarketTerminal marketTerminal = Pool.Get<MarketTerminal>();
		Deserialize(stream, marketTerminal, isDelta: false);
		return marketTerminal;
	}

	public static MarketTerminal DeserializeLengthDelimited(BufferStream stream)
	{
		MarketTerminal marketTerminal = Pool.Get<MarketTerminal>();
		DeserializeLengthDelimited(stream, marketTerminal, isDelta: false);
		return marketTerminal;
	}

	public static MarketTerminal DeserializeLength(BufferStream stream, int length)
	{
		MarketTerminal marketTerminal = Pool.Get<MarketTerminal>();
		DeserializeLength(stream, length, marketTerminal, isDelta: false);
		return marketTerminal;
	}

	public static MarketTerminal Deserialize(byte[] buffer)
	{
		MarketTerminal marketTerminal = Pool.Get<MarketTerminal>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, marketTerminal, isDelta: false);
		return marketTerminal;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MarketTerminal previous)
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

	public static MarketTerminal Deserialize(BufferStream stream, MarketTerminal instance, bool isDelta)
	{
		if (!isDelta && instance.orders == null)
		{
			instance.orders = Pool.Get<List<PendingOrder>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MarketTerminal");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.customerSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.marketplaceId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				stream.ConsumeRepeatedElement();
				instance.orders.Add(PendingOrder.DeserializeLengthDelimited(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.customerName = ProtocolParser.ReadString(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.timeUntilExpiry = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.deliveryFeeCurrency = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.deliveryFeeAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MarketTerminal DeserializeLengthDelimited(BufferStream stream, MarketTerminal instance, bool isDelta)
	{
		if (!isDelta && instance.orders == null)
		{
			instance.orders = Pool.Get<List<PendingOrder>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MarketTerminal");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.customerSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.marketplaceId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				stream.ConsumeRepeatedElement();
				instance.orders.Add(PendingOrder.DeserializeLengthDelimited(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.customerName = ProtocolParser.ReadString(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.timeUntilExpiry = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.deliveryFeeCurrency = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.deliveryFeeAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MarketTerminal DeserializeLength(BufferStream stream, int length, MarketTerminal instance, bool isDelta)
	{
		if (!isDelta && instance.orders == null)
		{
			instance.orders = Pool.Get<List<PendingOrder>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MarketTerminal");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.customerSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.marketplaceId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				stream.ConsumeRepeatedElement();
				instance.orders.Add(PendingOrder.DeserializeLengthDelimited(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.customerName = ProtocolParser.ReadString(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.timeUntilExpiry = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.deliveryFeeCurrency = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				instance.deliveryFeeAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MarketTerminal");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MarketTerminal instance, MarketTerminal previous)
	{
		if (instance.customerSteamId != previous.customerSteamId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.customerSteamId);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, instance.marketplaceId.Value);
		if (instance.orders != null)
		{
			for (int i = 0; i < instance.orders.Count; i++)
			{
				PendingOrder pendingOrder = instance.orders[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				PendingOrder.SerializeDelta(stream, pendingOrder, pendingOrder);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field orders (ProtoBuf.MarketTerminal.PendingOrder)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.customerName != null && instance.customerName != previous.customerName)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.customerName);
		}
		if (instance.timeUntilExpiry != previous.timeUntilExpiry)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.timeUntilExpiry);
		}
		if (instance.deliveryFeeCurrency != previous.deliveryFeeCurrency)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.deliveryFeeCurrency);
		}
		if (instance.deliveryFeeAmount != previous.deliveryFeeAmount)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.deliveryFeeAmount);
		}
	}

	public static void Serialize(BufferStream stream, MarketTerminal instance)
	{
		if (instance.customerSteamId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.customerSteamId);
		}
		if (instance.marketplaceId != default(NetworkableId))
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.marketplaceId.Value);
		}
		if (instance.orders != null)
		{
			for (int i = 0; i < instance.orders.Count; i++)
			{
				PendingOrder instance2 = instance.orders[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				PendingOrder.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field orders (ProtoBuf.MarketTerminal.PendingOrder)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.customerName != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.customerName);
		}
		if (instance.timeUntilExpiry != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.timeUntilExpiry);
		}
		if (instance.deliveryFeeCurrency != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.deliveryFeeCurrency);
		}
		if (instance.deliveryFeeAmount != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.deliveryFeeAmount);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref marketplaceId.Value);
		if (orders != null)
		{
			for (int i = 0; i < orders.Count; i++)
			{
				orders[i]?.InspectUids(action);
			}
		}
	}
}
