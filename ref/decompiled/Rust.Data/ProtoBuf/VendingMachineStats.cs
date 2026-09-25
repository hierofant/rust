using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VendingMachineStats : IDisposable, Pool.IPooled, IProto<VendingMachineStats>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<VendingMachinePurchaseHistoryEntryMessage> purchaseHistory;

	[NonSerialized]
	public List<ulong> customers;

	[NonSerialized]
	public List<int> customersVisits;

	public static void ResetToPool(VendingMachineStats instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.purchaseHistory != null)
		{
			for (int i = 0; i < instance.purchaseHistory.Count; i++)
			{
				if (instance.purchaseHistory[i] != null)
				{
					instance.purchaseHistory[i].ResetToPool();
					instance.purchaseHistory[i] = null;
				}
			}
			List<VendingMachinePurchaseHistoryEntryMessage> obj = instance.purchaseHistory;
			Pool.Free(ref obj, freeElements: false);
			instance.purchaseHistory = obj;
		}
		if (instance.customers != null)
		{
			List<ulong> obj2 = instance.customers;
			Pool.FreeUnmanaged(ref obj2);
			instance.customers = obj2;
		}
		if (instance.customersVisits != null)
		{
			List<int> obj3 = instance.customersVisits;
			Pool.FreeUnmanaged(ref obj3);
			instance.customersVisits = obj3;
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
			throw new Exception("Trying to dispose VendingMachineStats with ShouldPool set to false!");
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

	public void CopyTo(VendingMachineStats instance)
	{
		if (purchaseHistory != null)
		{
			instance.purchaseHistory = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			for (int i = 0; i < purchaseHistory.Count; i++)
			{
				VendingMachinePurchaseHistoryEntryMessage item = purchaseHistory[i].Copy();
				instance.purchaseHistory.Add(item);
			}
		}
		else
		{
			instance.purchaseHistory = null;
		}
		if (customers != null)
		{
			instance.customers = Pool.Get<List<ulong>>();
			for (int j = 0; j < customers.Count; j++)
			{
				ulong item2 = customers[j];
				instance.customers.Add(item2);
			}
		}
		else
		{
			instance.customers = null;
		}
		if (customersVisits != null)
		{
			instance.customersVisits = Pool.Get<List<int>>();
			for (int k = 0; k < customersVisits.Count; k++)
			{
				int item3 = customersVisits[k];
				instance.customersVisits.Add(item3);
			}
		}
		else
		{
			instance.customersVisits = null;
		}
	}

	public VendingMachineStats Copy()
	{
		VendingMachineStats vendingMachineStats = Pool.Get<VendingMachineStats>();
		CopyTo(vendingMachineStats);
		return vendingMachineStats;
	}

	public static VendingMachineStats Deserialize(BufferStream stream)
	{
		VendingMachineStats vendingMachineStats = Pool.Get<VendingMachineStats>();
		Deserialize(stream, vendingMachineStats, isDelta: false);
		return vendingMachineStats;
	}

	public static VendingMachineStats DeserializeLengthDelimited(BufferStream stream)
	{
		VendingMachineStats vendingMachineStats = Pool.Get<VendingMachineStats>();
		DeserializeLengthDelimited(stream, vendingMachineStats, isDelta: false);
		return vendingMachineStats;
	}

	public static VendingMachineStats DeserializeLength(BufferStream stream, int length)
	{
		VendingMachineStats vendingMachineStats = Pool.Get<VendingMachineStats>();
		DeserializeLength(stream, length, vendingMachineStats, isDelta: false);
		return vendingMachineStats;
	}

	public static VendingMachineStats Deserialize(byte[] buffer)
	{
		VendingMachineStats vendingMachineStats = Pool.Get<VendingMachineStats>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vendingMachineStats, isDelta: false);
		return vendingMachineStats;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VendingMachineStats previous)
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

	public static VendingMachineStats Deserialize(BufferStream stream, VendingMachineStats instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.purchaseHistory == null)
			{
				instance.purchaseHistory = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			}
			if (instance.customers == null)
			{
				instance.customers = Pool.Get<List<ulong>>();
			}
			if (instance.customersVisits == null)
			{
				instance.customersVisits = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachineStats");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.purchaseHistory.Add(VendingMachinePurchaseHistoryEntryMessage.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.customers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.customersVisits.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachineStats DeserializeLengthDelimited(BufferStream stream, VendingMachineStats instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.purchaseHistory == null)
			{
				instance.purchaseHistory = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			}
			if (instance.customers == null)
			{
				instance.customers = Pool.Get<List<ulong>>();
			}
			if (instance.customersVisits == null)
			{
				instance.customersVisits = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachineStats");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.purchaseHistory.Add(VendingMachinePurchaseHistoryEntryMessage.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.customers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.customersVisits.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachineStats DeserializeLength(BufferStream stream, int length, VendingMachineStats instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.purchaseHistory == null)
			{
				instance.purchaseHistory = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			}
			if (instance.customers == null)
			{
				instance.customers = Pool.Get<List<ulong>>();
			}
			if (instance.customersVisits == null)
			{
				instance.customersVisits = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachineStats");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.purchaseHistory.Add(VendingMachinePurchaseHistoryEntryMessage.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.customers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				stream.ConsumeRepeatedElement();
				instance.customersVisits.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachineStats");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VendingMachineStats instance, VendingMachineStats previous)
	{
		if (instance.purchaseHistory != null)
		{
			for (int i = 0; i < instance.purchaseHistory.Count; i++)
			{
				VendingMachinePurchaseHistoryEntryMessage vendingMachinePurchaseHistoryEntryMessage = instance.purchaseHistory[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				VendingMachinePurchaseHistoryEntryMessage.SerializeDelta(stream, vendingMachinePurchaseHistoryEntryMessage, vendingMachinePurchaseHistoryEntryMessage);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field purchaseHistory (ProtoBuf.VendingMachinePurchaseHistoryEntryMessage)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.customers != null)
		{
			for (int j = 0; j < instance.customers.Count; j++)
			{
				ulong val = instance.customers[j];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.customersVisits != null)
		{
			for (int k = 0; k < instance.customersVisits.Count; k++)
			{
				int num2 = instance.customersVisits[k];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)num2);
			}
		}
	}

	public static void Serialize(BufferStream stream, VendingMachineStats instance)
	{
		if (instance.purchaseHistory != null)
		{
			for (int i = 0; i < instance.purchaseHistory.Count; i++)
			{
				VendingMachinePurchaseHistoryEntryMessage instance2 = instance.purchaseHistory[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				VendingMachinePurchaseHistoryEntryMessage.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field purchaseHistory (ProtoBuf.VendingMachinePurchaseHistoryEntryMessage)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.customers != null)
		{
			for (int j = 0; j < instance.customers.Count; j++)
			{
				ulong val = instance.customers[j];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.customersVisits != null)
		{
			for (int k = 0; k < instance.customersVisits.Count; k++)
			{
				int num2 = instance.customersVisits[k];
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)num2);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (purchaseHistory != null)
		{
			for (int i = 0; i < purchaseHistory.Count; i++)
			{
				purchaseHistory[i]?.InspectUids(action);
			}
		}
	}
}
