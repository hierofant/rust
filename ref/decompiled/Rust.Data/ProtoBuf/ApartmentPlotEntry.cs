using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentPlotEntry : IDisposable, Pool.IPooled, IProto<ApartmentPlotEntry>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string roomNumber;

	[NonSerialized]
	public bool occupied;

	[NonSerialized]
	public int storageSlots;

	[NonSerialized]
	public int roomCount;

	[NonSerialized]
	public int estimatedValue;

	[NonSerialized]
	public int size;

	[NonSerialized]
	public NetworkableId roomId;

	public static void ResetToPool(ApartmentPlotEntry instance)
	{
		if (instance.ShouldPool)
		{
			instance.roomNumber = string.Empty;
			instance.occupied = false;
			instance.storageSlots = 0;
			instance.roomCount = 0;
			instance.estimatedValue = 0;
			instance.size = 0;
			instance.roomId = default(NetworkableId);
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
			throw new Exception("Trying to dispose ApartmentPlotEntry with ShouldPool set to false!");
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

	public void CopyTo(ApartmentPlotEntry instance)
	{
		instance.roomNumber = roomNumber;
		instance.occupied = occupied;
		instance.storageSlots = storageSlots;
		instance.roomCount = roomCount;
		instance.estimatedValue = estimatedValue;
		instance.size = size;
		instance.roomId = roomId;
	}

	public ApartmentPlotEntry Copy()
	{
		ApartmentPlotEntry apartmentPlotEntry = Pool.Get<ApartmentPlotEntry>();
		CopyTo(apartmentPlotEntry);
		return apartmentPlotEntry;
	}

	public static ApartmentPlotEntry Deserialize(BufferStream stream)
	{
		ApartmentPlotEntry apartmentPlotEntry = Pool.Get<ApartmentPlotEntry>();
		Deserialize(stream, apartmentPlotEntry, isDelta: false);
		return apartmentPlotEntry;
	}

	public static ApartmentPlotEntry DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentPlotEntry apartmentPlotEntry = Pool.Get<ApartmentPlotEntry>();
		DeserializeLengthDelimited(stream, apartmentPlotEntry, isDelta: false);
		return apartmentPlotEntry;
	}

	public static ApartmentPlotEntry DeserializeLength(BufferStream stream, int length)
	{
		ApartmentPlotEntry apartmentPlotEntry = Pool.Get<ApartmentPlotEntry>();
		DeserializeLength(stream, length, apartmentPlotEntry, isDelta: false);
		return apartmentPlotEntry;
	}

	public static ApartmentPlotEntry Deserialize(byte[] buffer)
	{
		ApartmentPlotEntry apartmentPlotEntry = Pool.Get<ApartmentPlotEntry>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentPlotEntry, isDelta: false);
		return apartmentPlotEntry;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentPlotEntry previous)
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

	public static ApartmentPlotEntry Deserialize(BufferStream stream, ApartmentPlotEntry instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentPlotEntry");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.occupied = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.storageSlots = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.estimatedValue = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.size = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentPlotEntry DeserializeLengthDelimited(BufferStream stream, ApartmentPlotEntry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentPlotEntry");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.occupied = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.storageSlots = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.estimatedValue = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.size = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentPlotEntry DeserializeLength(BufferStream stream, int length, ApartmentPlotEntry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentPlotEntry");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.occupied = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.storageSlots = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.estimatedValue = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.size = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				instance.roomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentPlotEntry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentPlotEntry instance, ApartmentPlotEntry previous)
	{
		if (instance.roomNumber != null && instance.roomNumber != previous.roomNumber)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.occupied);
		if (instance.storageSlots != previous.storageSlots)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.storageSlots);
		}
		if (instance.roomCount != previous.roomCount)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.roomCount);
		}
		if (instance.estimatedValue != previous.estimatedValue)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.estimatedValue);
		}
		if (instance.size != previous.size)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.size);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteUInt64(stream, instance.roomId.Value);
	}

	public static void Serialize(BufferStream stream, ApartmentPlotEntry instance)
	{
		if (instance.roomNumber != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		if (instance.occupied)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.occupied);
		}
		if (instance.storageSlots != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.storageSlots);
		}
		if (instance.roomCount != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.roomCount);
		}
		if (instance.estimatedValue != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.estimatedValue);
		}
		if (instance.size != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.size);
		}
		if (instance.roomId != default(NetworkableId))
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.roomId.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref roomId.Value);
	}
}
