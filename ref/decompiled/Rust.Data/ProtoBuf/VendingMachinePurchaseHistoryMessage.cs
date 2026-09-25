using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VendingMachinePurchaseHistoryMessage : IDisposable, Pool.IPooled, IProto<VendingMachinePurchaseHistoryMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<VendingMachinePurchaseHistoryEntryMessage> transactions;

	[NonSerialized]
	public List<VendingMachinePurchaseHistoryEntrySmallMessage> smallTransactions;

	public static void ResetToPool(VendingMachinePurchaseHistoryMessage instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.transactions != null)
		{
			for (int i = 0; i < instance.transactions.Count; i++)
			{
				if (instance.transactions[i] != null)
				{
					instance.transactions[i].ResetToPool();
					instance.transactions[i] = null;
				}
			}
			List<VendingMachinePurchaseHistoryEntryMessage> obj = instance.transactions;
			Pool.Free(ref obj, freeElements: false);
			instance.transactions = obj;
		}
		if (instance.smallTransactions != null)
		{
			for (int j = 0; j < instance.smallTransactions.Count; j++)
			{
				if (instance.smallTransactions[j] != null)
				{
					instance.smallTransactions[j].ResetToPool();
					instance.smallTransactions[j] = null;
				}
			}
			List<VendingMachinePurchaseHistoryEntrySmallMessage> obj2 = instance.smallTransactions;
			Pool.Free(ref obj2, freeElements: false);
			instance.smallTransactions = obj2;
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
			throw new Exception("Trying to dispose VendingMachinePurchaseHistoryMessage with ShouldPool set to false!");
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

	public void CopyTo(VendingMachinePurchaseHistoryMessage instance)
	{
		if (transactions != null)
		{
			instance.transactions = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			for (int i = 0; i < transactions.Count; i++)
			{
				VendingMachinePurchaseHistoryEntryMessage item = transactions[i].Copy();
				instance.transactions.Add(item);
			}
		}
		else
		{
			instance.transactions = null;
		}
		if (smallTransactions != null)
		{
			instance.smallTransactions = Pool.Get<List<VendingMachinePurchaseHistoryEntrySmallMessage>>();
			for (int j = 0; j < smallTransactions.Count; j++)
			{
				VendingMachinePurchaseHistoryEntrySmallMessage item2 = smallTransactions[j].Copy();
				instance.smallTransactions.Add(item2);
			}
		}
		else
		{
			instance.smallTransactions = null;
		}
	}

	public VendingMachinePurchaseHistoryMessage Copy()
	{
		VendingMachinePurchaseHistoryMessage vendingMachinePurchaseHistoryMessage = Pool.Get<VendingMachinePurchaseHistoryMessage>();
		CopyTo(vendingMachinePurchaseHistoryMessage);
		return vendingMachinePurchaseHistoryMessage;
	}

	public static VendingMachinePurchaseHistoryMessage Deserialize(BufferStream stream)
	{
		VendingMachinePurchaseHistoryMessage vendingMachinePurchaseHistoryMessage = Pool.Get<VendingMachinePurchaseHistoryMessage>();
		Deserialize(stream, vendingMachinePurchaseHistoryMessage, isDelta: false);
		return vendingMachinePurchaseHistoryMessage;
	}

	public static VendingMachinePurchaseHistoryMessage DeserializeLengthDelimited(BufferStream stream)
	{
		VendingMachinePurchaseHistoryMessage vendingMachinePurchaseHistoryMessage = Pool.Get<VendingMachinePurchaseHistoryMessage>();
		DeserializeLengthDelimited(stream, vendingMachinePurchaseHistoryMessage, isDelta: false);
		return vendingMachinePurchaseHistoryMessage;
	}

	public static VendingMachinePurchaseHistoryMessage DeserializeLength(BufferStream stream, int length)
	{
		VendingMachinePurchaseHistoryMessage vendingMachinePurchaseHistoryMessage = Pool.Get<VendingMachinePurchaseHistoryMessage>();
		DeserializeLength(stream, length, vendingMachinePurchaseHistoryMessage, isDelta: false);
		return vendingMachinePurchaseHistoryMessage;
	}

	public static VendingMachinePurchaseHistoryMessage Deserialize(byte[] buffer)
	{
		VendingMachinePurchaseHistoryMessage vendingMachinePurchaseHistoryMessage = Pool.Get<VendingMachinePurchaseHistoryMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vendingMachinePurchaseHistoryMessage, isDelta: false);
		return vendingMachinePurchaseHistoryMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VendingMachinePurchaseHistoryMessage previous)
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

	public static VendingMachinePurchaseHistoryMessage Deserialize(BufferStream stream, VendingMachinePurchaseHistoryMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.transactions == null)
			{
				instance.transactions = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			}
			if (instance.smallTransactions == null)
			{
				instance.smallTransactions = Pool.Get<List<VendingMachinePurchaseHistoryEntrySmallMessage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachinePurchaseHistoryMessage");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				stream.ConsumeRepeatedElement();
				instance.transactions.Add(VendingMachinePurchaseHistoryEntryMessage.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				stream.ConsumeRepeatedElement();
				instance.smallTransactions.Add(VendingMachinePurchaseHistoryEntrySmallMessage.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachinePurchaseHistoryMessage DeserializeLengthDelimited(BufferStream stream, VendingMachinePurchaseHistoryMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.transactions == null)
			{
				instance.transactions = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			}
			if (instance.smallTransactions == null)
			{
				instance.smallTransactions = Pool.Get<List<VendingMachinePurchaseHistoryEntrySmallMessage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachinePurchaseHistoryMessage");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				stream.ConsumeRepeatedElement();
				instance.transactions.Add(VendingMachinePurchaseHistoryEntryMessage.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				stream.ConsumeRepeatedElement();
				instance.smallTransactions.Add(VendingMachinePurchaseHistoryEntrySmallMessage.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingMachinePurchaseHistoryMessage DeserializeLength(BufferStream stream, int length, VendingMachinePurchaseHistoryMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.transactions == null)
			{
				instance.transactions = Pool.Get<List<VendingMachinePurchaseHistoryEntryMessage>>();
			}
			if (instance.smallTransactions == null)
			{
				instance.smallTransactions = Pool.Get<List<VendingMachinePurchaseHistoryEntrySmallMessage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingMachinePurchaseHistoryMessage");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				stream.ConsumeRepeatedElement();
				instance.transactions.Add(VendingMachinePurchaseHistoryEntryMessage.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				stream.ConsumeRepeatedElement();
				instance.smallTransactions.Add(VendingMachinePurchaseHistoryEntrySmallMessage.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingMachinePurchaseHistoryMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VendingMachinePurchaseHistoryMessage instance, VendingMachinePurchaseHistoryMessage previous)
	{
		if (instance.transactions != null)
		{
			for (int i = 0; i < instance.transactions.Count; i++)
			{
				VendingMachinePurchaseHistoryEntryMessage vendingMachinePurchaseHistoryEntryMessage = instance.transactions[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				VendingMachinePurchaseHistoryEntryMessage.SerializeDelta(stream, vendingMachinePurchaseHistoryEntryMessage, vendingMachinePurchaseHistoryEntryMessage);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field transactions (ProtoBuf.VendingMachinePurchaseHistoryEntryMessage)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.smallTransactions == null)
		{
			return;
		}
		for (int j = 0; j < instance.smallTransactions.Count; j++)
		{
			VendingMachinePurchaseHistoryEntrySmallMessage vendingMachinePurchaseHistoryEntrySmallMessage = instance.smallTransactions[j];
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			VendingMachinePurchaseHistoryEntrySmallMessage.SerializeDelta(stream, vendingMachinePurchaseHistoryEntrySmallMessage, vendingMachinePurchaseHistoryEntrySmallMessage);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field smallTransactions (ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public static void Serialize(BufferStream stream, VendingMachinePurchaseHistoryMessage instance)
	{
		if (instance.transactions != null)
		{
			for (int i = 0; i < instance.transactions.Count; i++)
			{
				VendingMachinePurchaseHistoryEntryMessage instance2 = instance.transactions[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				VendingMachinePurchaseHistoryEntryMessage.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field transactions (ProtoBuf.VendingMachinePurchaseHistoryEntryMessage)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.smallTransactions == null)
		{
			return;
		}
		for (int j = 0; j < instance.smallTransactions.Count; j++)
		{
			VendingMachinePurchaseHistoryEntrySmallMessage instance3 = instance.smallTransactions[j];
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			VendingMachinePurchaseHistoryEntrySmallMessage.Serialize(stream, instance3);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field smallTransactions (ProtoBuf.VendingMachinePurchaseHistoryEntrySmallMessage)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (transactions != null)
		{
			for (int i = 0; i < transactions.Count; i++)
			{
				transactions[i]?.InspectUids(action);
			}
		}
		if (smallTransactions != null)
		{
			for (int j = 0; j < smallTransactions.Count; j++)
			{
				smallTransactions[j]?.InspectUids(action);
			}
		}
	}
}
