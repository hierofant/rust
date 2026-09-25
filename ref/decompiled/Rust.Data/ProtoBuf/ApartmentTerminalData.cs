using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentTerminalData : IDisposable, Pool.IPooled, IProto<ApartmentTerminalData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ApartmentPlotEntry> plots;

	[NonSerialized]
	public int totalAvailable;

	[NonSerialized]
	public int totalOccupied;

	[NonSerialized]
	public List<ApartmentPlotEntry> shops;

	[NonSerialized]
	public int totalShopsAvailable;

	[NonSerialized]
	public int totalShopsOccupied;

	public static void ResetToPool(ApartmentTerminalData instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.plots != null)
		{
			for (int i = 0; i < instance.plots.Count; i++)
			{
				if (instance.plots[i] != null)
				{
					instance.plots[i].ResetToPool();
					instance.plots[i] = null;
				}
			}
			List<ApartmentPlotEntry> obj = instance.plots;
			Pool.Free(ref obj, freeElements: false);
			instance.plots = obj;
		}
		instance.totalAvailable = 0;
		instance.totalOccupied = 0;
		if (instance.shops != null)
		{
			for (int j = 0; j < instance.shops.Count; j++)
			{
				if (instance.shops[j] != null)
				{
					instance.shops[j].ResetToPool();
					instance.shops[j] = null;
				}
			}
			List<ApartmentPlotEntry> obj2 = instance.shops;
			Pool.Free(ref obj2, freeElements: false);
			instance.shops = obj2;
		}
		instance.totalShopsAvailable = 0;
		instance.totalShopsOccupied = 0;
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
			throw new Exception("Trying to dispose ApartmentTerminalData with ShouldPool set to false!");
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

	public void CopyTo(ApartmentTerminalData instance)
	{
		if (plots != null)
		{
			instance.plots = Pool.Get<List<ApartmentPlotEntry>>();
			for (int i = 0; i < plots.Count; i++)
			{
				ApartmentPlotEntry item = plots[i].Copy();
				instance.plots.Add(item);
			}
		}
		else
		{
			instance.plots = null;
		}
		instance.totalAvailable = totalAvailable;
		instance.totalOccupied = totalOccupied;
		if (shops != null)
		{
			instance.shops = Pool.Get<List<ApartmentPlotEntry>>();
			for (int j = 0; j < shops.Count; j++)
			{
				ApartmentPlotEntry item2 = shops[j].Copy();
				instance.shops.Add(item2);
			}
		}
		else
		{
			instance.shops = null;
		}
		instance.totalShopsAvailable = totalShopsAvailable;
		instance.totalShopsOccupied = totalShopsOccupied;
	}

	public ApartmentTerminalData Copy()
	{
		ApartmentTerminalData apartmentTerminalData = Pool.Get<ApartmentTerminalData>();
		CopyTo(apartmentTerminalData);
		return apartmentTerminalData;
	}

	public static ApartmentTerminalData Deserialize(BufferStream stream)
	{
		ApartmentTerminalData apartmentTerminalData = Pool.Get<ApartmentTerminalData>();
		Deserialize(stream, apartmentTerminalData, isDelta: false);
		return apartmentTerminalData;
	}

	public static ApartmentTerminalData DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentTerminalData apartmentTerminalData = Pool.Get<ApartmentTerminalData>();
		DeserializeLengthDelimited(stream, apartmentTerminalData, isDelta: false);
		return apartmentTerminalData;
	}

	public static ApartmentTerminalData DeserializeLength(BufferStream stream, int length)
	{
		ApartmentTerminalData apartmentTerminalData = Pool.Get<ApartmentTerminalData>();
		DeserializeLength(stream, length, apartmentTerminalData, isDelta: false);
		return apartmentTerminalData;
	}

	public static ApartmentTerminalData Deserialize(byte[] buffer)
	{
		ApartmentTerminalData apartmentTerminalData = Pool.Get<ApartmentTerminalData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentTerminalData, isDelta: false);
		return apartmentTerminalData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentTerminalData previous)
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

	public static ApartmentTerminalData Deserialize(BufferStream stream, ApartmentTerminalData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.plots == null)
			{
				instance.plots = Pool.Get<List<ApartmentPlotEntry>>();
			}
			if (instance.shops == null)
			{
				instance.shops = Pool.Get<List<ApartmentPlotEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentTerminalData");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				stream.ConsumeRepeatedElement();
				instance.plots.Add(ApartmentPlotEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalAvailable = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalOccupied = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				stream.ConsumeRepeatedElement();
				instance.shops.Add(ApartmentPlotEntry.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalShopsAvailable = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalShopsOccupied = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentTerminalData DeserializeLengthDelimited(BufferStream stream, ApartmentTerminalData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.plots == null)
			{
				instance.plots = Pool.Get<List<ApartmentPlotEntry>>();
			}
			if (instance.shops == null)
			{
				instance.shops = Pool.Get<List<ApartmentPlotEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentTerminalData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				stream.ConsumeRepeatedElement();
				instance.plots.Add(ApartmentPlotEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalAvailable = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalOccupied = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				stream.ConsumeRepeatedElement();
				instance.shops.Add(ApartmentPlotEntry.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalShopsAvailable = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalShopsOccupied = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentTerminalData DeserializeLength(BufferStream stream, int length, ApartmentTerminalData instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.plots == null)
			{
				instance.plots = Pool.Get<List<ApartmentPlotEntry>>();
			}
			if (instance.shops == null)
			{
				instance.shops = Pool.Get<List<ApartmentPlotEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentTerminalData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				stream.ConsumeRepeatedElement();
				instance.plots.Add(ApartmentPlotEntry.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalAvailable = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalOccupied = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				stream.ConsumeRepeatedElement();
				instance.shops.Add(ApartmentPlotEntry.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalShopsAvailable = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				instance.totalShopsOccupied = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentTerminalData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentTerminalData instance, ApartmentTerminalData previous)
	{
		if (instance.plots != null)
		{
			for (int i = 0; i < instance.plots.Count; i++)
			{
				ApartmentPlotEntry apartmentPlotEntry = instance.plots[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ApartmentPlotEntry.SerializeDelta(stream, apartmentPlotEntry, apartmentPlotEntry);
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
		if (instance.totalAvailable != previous.totalAvailable)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalAvailable);
		}
		if (instance.totalOccupied != previous.totalOccupied)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalOccupied);
		}
		if (instance.shops != null)
		{
			for (int j = 0; j < instance.shops.Count; j++)
			{
				ApartmentPlotEntry apartmentPlotEntry2 = instance.shops[j];
				stream.WriteByte(34);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				ApartmentPlotEntry.SerializeDelta(stream, apartmentPlotEntry2, apartmentPlotEntry2);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.totalShopsAvailable != previous.totalShopsAvailable)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalShopsAvailable);
		}
		if (instance.totalShopsOccupied != previous.totalShopsOccupied)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalShopsOccupied);
		}
	}

	public static void Serialize(BufferStream stream, ApartmentTerminalData instance)
	{
		if (instance.plots != null)
		{
			for (int i = 0; i < instance.plots.Count; i++)
			{
				ApartmentPlotEntry instance2 = instance.plots[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				ApartmentPlotEntry.Serialize(stream, instance2);
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
		if (instance.totalAvailable != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalAvailable);
		}
		if (instance.totalOccupied != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalOccupied);
		}
		if (instance.shops != null)
		{
			for (int j = 0; j < instance.shops.Count; j++)
			{
				ApartmentPlotEntry instance3 = instance.shops[j];
				stream.WriteByte(34);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				ApartmentPlotEntry.Serialize(stream, instance3);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.totalShopsAvailable != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalShopsAvailable);
		}
		if (instance.totalShopsOccupied != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.totalShopsOccupied);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (plots != null)
		{
			for (int i = 0; i < plots.Count; i++)
			{
				plots[i]?.InspectUids(action);
			}
		}
		if (shops != null)
		{
			for (int j = 0; j < shops.Count; j++)
			{
				shops[j]?.InspectUids(action);
			}
		}
	}
}
