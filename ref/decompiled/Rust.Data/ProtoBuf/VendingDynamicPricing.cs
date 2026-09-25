using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VendingDynamicPricing : IDisposable, Pool.IPooled, IProto<VendingDynamicPricing>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<SalesData> allSalesData;

	[NonSerialized]
	public float timeToNextSalesUpdate;

	public static void ResetToPool(VendingDynamicPricing instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.allSalesData != null)
		{
			for (int i = 0; i < instance.allSalesData.Count; i++)
			{
				if (instance.allSalesData[i] != null)
				{
					instance.allSalesData[i].ResetToPool();
					instance.allSalesData[i] = null;
				}
			}
			List<SalesData> obj = instance.allSalesData;
			Pool.Free(ref obj, freeElements: false);
			instance.allSalesData = obj;
		}
		instance.timeToNextSalesUpdate = 0f;
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
			throw new Exception("Trying to dispose VendingDynamicPricing with ShouldPool set to false!");
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

	public void CopyTo(VendingDynamicPricing instance)
	{
		if (allSalesData != null)
		{
			instance.allSalesData = Pool.Get<List<SalesData>>();
			for (int i = 0; i < allSalesData.Count; i++)
			{
				SalesData item = allSalesData[i].Copy();
				instance.allSalesData.Add(item);
			}
		}
		else
		{
			instance.allSalesData = null;
		}
		instance.timeToNextSalesUpdate = timeToNextSalesUpdate;
	}

	public VendingDynamicPricing Copy()
	{
		VendingDynamicPricing vendingDynamicPricing = Pool.Get<VendingDynamicPricing>();
		CopyTo(vendingDynamicPricing);
		return vendingDynamicPricing;
	}

	public static VendingDynamicPricing Deserialize(BufferStream stream)
	{
		VendingDynamicPricing vendingDynamicPricing = Pool.Get<VendingDynamicPricing>();
		Deserialize(stream, vendingDynamicPricing, isDelta: false);
		return vendingDynamicPricing;
	}

	public static VendingDynamicPricing DeserializeLengthDelimited(BufferStream stream)
	{
		VendingDynamicPricing vendingDynamicPricing = Pool.Get<VendingDynamicPricing>();
		DeserializeLengthDelimited(stream, vendingDynamicPricing, isDelta: false);
		return vendingDynamicPricing;
	}

	public static VendingDynamicPricing DeserializeLength(BufferStream stream, int length)
	{
		VendingDynamicPricing vendingDynamicPricing = Pool.Get<VendingDynamicPricing>();
		DeserializeLength(stream, length, vendingDynamicPricing, isDelta: false);
		return vendingDynamicPricing;
	}

	public static VendingDynamicPricing Deserialize(byte[] buffer)
	{
		VendingDynamicPricing vendingDynamicPricing = Pool.Get<VendingDynamicPricing>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vendingDynamicPricing, isDelta: false);
		return vendingDynamicPricing;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VendingDynamicPricing previous)
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

	public static VendingDynamicPricing Deserialize(BufferStream stream, VendingDynamicPricing instance, bool isDelta)
	{
		if (!isDelta && instance.allSalesData == null)
		{
			instance.allSalesData = Pool.Get<List<SalesData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VendingDynamicPricing");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				stream.ConsumeRepeatedElement();
				instance.allSalesData.Add(SalesData.DeserializeLengthDelimited(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingDynamicPricing");
				instance.timeToNextSalesUpdate = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingDynamicPricing DeserializeLengthDelimited(BufferStream stream, VendingDynamicPricing instance, bool isDelta)
	{
		if (!isDelta && instance.allSalesData == null)
		{
			instance.allSalesData = Pool.Get<List<SalesData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingDynamicPricing");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				stream.ConsumeRepeatedElement();
				instance.allSalesData.Add(SalesData.DeserializeLengthDelimited(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingDynamicPricing");
				instance.timeToNextSalesUpdate = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VendingDynamicPricing DeserializeLength(BufferStream stream, int length, VendingDynamicPricing instance, bool isDelta)
	{
		if (!isDelta && instance.allSalesData == null)
		{
			instance.allSalesData = Pool.Get<List<SalesData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.VendingDynamicPricing");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				stream.ConsumeRepeatedElement();
				instance.allSalesData.Add(SalesData.DeserializeLengthDelimited(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingDynamicPricing");
				instance.timeToNextSalesUpdate = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VendingDynamicPricing");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VendingDynamicPricing instance, VendingDynamicPricing previous)
	{
		if (instance.allSalesData != null)
		{
			for (int i = 0; i < instance.allSalesData.Count; i++)
			{
				SalesData salesData = instance.allSalesData[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				SalesData.SerializeDelta(stream, salesData, salesData);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field allSalesData (ProtoBuf.SalesData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.timeToNextSalesUpdate != previous.timeToNextSalesUpdate)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.timeToNextSalesUpdate);
		}
	}

	public static void Serialize(BufferStream stream, VendingDynamicPricing instance)
	{
		if (instance.allSalesData != null)
		{
			for (int i = 0; i < instance.allSalesData.Count; i++)
			{
				SalesData instance2 = instance.allSalesData[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				SalesData.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field allSalesData (ProtoBuf.SalesData)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.timeToNextSalesUpdate != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.timeToNextSalesUpdate);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (allSalesData != null)
		{
			for (int i = 0; i < allSalesData.Count; i++)
			{
				allSalesData[i]?.InspectUids(action);
			}
		}
	}
}
