using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SalesData : IDisposable, Pool.IPooled, IProto<SalesData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong totalSales;

	[NonSerialized]
	public ulong totalIntervals;

	[NonSerialized]
	public ulong soldThisInterval;

	[NonSerialized]
	public float currentMultiplier;

	[NonSerialized]
	public bool isForReceivedQuantity;

	public static void ResetToPool(SalesData instance)
	{
		if (instance.ShouldPool)
		{
			instance.totalSales = 0uL;
			instance.totalIntervals = 0uL;
			instance.soldThisInterval = 0uL;
			instance.currentMultiplier = 0f;
			instance.isForReceivedQuantity = false;
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
			throw new Exception("Trying to dispose SalesData with ShouldPool set to false!");
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

	public void CopyTo(SalesData instance)
	{
		instance.totalSales = totalSales;
		instance.totalIntervals = totalIntervals;
		instance.soldThisInterval = soldThisInterval;
		instance.currentMultiplier = currentMultiplier;
		instance.isForReceivedQuantity = isForReceivedQuantity;
	}

	public SalesData Copy()
	{
		SalesData salesData = Pool.Get<SalesData>();
		CopyTo(salesData);
		return salesData;
	}

	public static SalesData Deserialize(BufferStream stream)
	{
		SalesData salesData = Pool.Get<SalesData>();
		Deserialize(stream, salesData, isDelta: false);
		return salesData;
	}

	public static SalesData DeserializeLengthDelimited(BufferStream stream)
	{
		SalesData salesData = Pool.Get<SalesData>();
		DeserializeLengthDelimited(stream, salesData, isDelta: false);
		return salesData;
	}

	public static SalesData DeserializeLength(BufferStream stream, int length)
	{
		SalesData salesData = Pool.Get<SalesData>();
		DeserializeLength(stream, length, salesData, isDelta: false);
		return salesData;
	}

	public static SalesData Deserialize(byte[] buffer)
	{
		SalesData salesData = Pool.Get<SalesData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, salesData, isDelta: false);
		return salesData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SalesData previous)
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

	public static SalesData Deserialize(BufferStream stream, SalesData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SalesData");
			switch (num)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.totalSales = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.totalIntervals = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.soldThisInterval = ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.currentMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.isForReceivedQuantity = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SalesData DeserializeLengthDelimited(BufferStream stream, SalesData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SalesData");
			switch (num2)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.totalSales = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.totalIntervals = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.soldThisInterval = ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.currentMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.isForReceivedQuantity = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SalesData DeserializeLength(BufferStream stream, int length, SalesData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SalesData");
			switch (num2)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.totalSales = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.totalIntervals = ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.soldThisInterval = ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.currentMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				instance.isForReceivedQuantity = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SalesData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SalesData instance, SalesData previous)
	{
		if (instance.totalSales != previous.totalSales)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.totalSales);
		}
		if (instance.totalIntervals != previous.totalIntervals)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.totalIntervals);
		}
		if (instance.soldThisInterval != previous.soldThisInterval)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.soldThisInterval);
		}
		if (instance.currentMultiplier != previous.currentMultiplier)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.currentMultiplier);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.isForReceivedQuantity);
	}

	public static void Serialize(BufferStream stream, SalesData instance)
	{
		if (instance.totalSales != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.totalSales);
		}
		if (instance.totalIntervals != 0L)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.totalIntervals);
		}
		if (instance.soldThisInterval != 0L)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.soldThisInterval);
		}
		if (instance.currentMultiplier != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.currentMultiplier);
		}
		if (instance.isForReceivedQuantity)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.isForReceivedQuantity);
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
