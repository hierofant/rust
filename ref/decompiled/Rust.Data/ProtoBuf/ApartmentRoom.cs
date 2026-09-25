using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentRoom : IDisposable, Pool.IPooled, IProto<ApartmentRoom>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string roomNumber;

	[NonSerialized]
	public List<NetworkableId> furnitureIds;

	[NonSerialized]
	public int upkeepSeconds;

	[NonSerialized]
	public NetworkableId upkeepTerminalId;

	[NonSerialized]
	public float dailyUpkeepCost;

	[NonSerialized]
	public float outstandingRent;

	[NonSerialized]
	public float timeRentOverdue;

	[NonSerialized]
	public List<ulong> owners;

	[NonSerialized]
	public List<ulong> guests;

	[NonSerialized]
	public List<TimedUserAccess> intruders;

	public static void ResetToPool(ApartmentRoom instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.roomNumber = string.Empty;
		if (instance.furnitureIds != null)
		{
			List<NetworkableId> obj = instance.furnitureIds;
			Pool.FreeUnmanaged(ref obj);
			instance.furnitureIds = obj;
		}
		instance.upkeepSeconds = 0;
		instance.upkeepTerminalId = default(NetworkableId);
		instance.dailyUpkeepCost = 0f;
		instance.outstandingRent = 0f;
		instance.timeRentOverdue = 0f;
		if (instance.owners != null)
		{
			List<ulong> obj2 = instance.owners;
			Pool.FreeUnmanaged(ref obj2);
			instance.owners = obj2;
		}
		if (instance.guests != null)
		{
			List<ulong> obj3 = instance.guests;
			Pool.FreeUnmanaged(ref obj3);
			instance.guests = obj3;
		}
		if (instance.intruders != null)
		{
			for (int i = 0; i < instance.intruders.Count; i++)
			{
				if (instance.intruders[i] != null)
				{
					instance.intruders[i].ResetToPool();
					instance.intruders[i] = null;
				}
			}
			List<TimedUserAccess> obj4 = instance.intruders;
			Pool.Free(ref obj4, freeElements: false);
			instance.intruders = obj4;
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
			throw new Exception("Trying to dispose ApartmentRoom with ShouldPool set to false!");
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

	public void CopyTo(ApartmentRoom instance)
	{
		instance.roomNumber = roomNumber;
		if (furnitureIds != null)
		{
			instance.furnitureIds = Pool.Get<List<NetworkableId>>();
			for (int i = 0; i < furnitureIds.Count; i++)
			{
				NetworkableId item = furnitureIds[i];
				instance.furnitureIds.Add(item);
			}
		}
		else
		{
			instance.furnitureIds = null;
		}
		instance.upkeepSeconds = upkeepSeconds;
		instance.upkeepTerminalId = upkeepTerminalId;
		instance.dailyUpkeepCost = dailyUpkeepCost;
		instance.outstandingRent = outstandingRent;
		instance.timeRentOverdue = timeRentOverdue;
		if (owners != null)
		{
			instance.owners = Pool.Get<List<ulong>>();
			for (int j = 0; j < owners.Count; j++)
			{
				ulong item2 = owners[j];
				instance.owners.Add(item2);
			}
		}
		else
		{
			instance.owners = null;
		}
		if (guests != null)
		{
			instance.guests = Pool.Get<List<ulong>>();
			for (int k = 0; k < guests.Count; k++)
			{
				ulong item3 = guests[k];
				instance.guests.Add(item3);
			}
		}
		else
		{
			instance.guests = null;
		}
		if (intruders != null)
		{
			instance.intruders = Pool.Get<List<TimedUserAccess>>();
			for (int l = 0; l < intruders.Count; l++)
			{
				TimedUserAccess item4 = intruders[l].Copy();
				instance.intruders.Add(item4);
			}
		}
		else
		{
			instance.intruders = null;
		}
	}

	public ApartmentRoom Copy()
	{
		ApartmentRoom apartmentRoom = Pool.Get<ApartmentRoom>();
		CopyTo(apartmentRoom);
		return apartmentRoom;
	}

	public static ApartmentRoom Deserialize(BufferStream stream)
	{
		ApartmentRoom apartmentRoom = Pool.Get<ApartmentRoom>();
		Deserialize(stream, apartmentRoom, isDelta: false);
		return apartmentRoom;
	}

	public static ApartmentRoom DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentRoom apartmentRoom = Pool.Get<ApartmentRoom>();
		DeserializeLengthDelimited(stream, apartmentRoom, isDelta: false);
		return apartmentRoom;
	}

	public static ApartmentRoom DeserializeLength(BufferStream stream, int length)
	{
		ApartmentRoom apartmentRoom = Pool.Get<ApartmentRoom>();
		DeserializeLength(stream, length, apartmentRoom, isDelta: false);
		return apartmentRoom;
	}

	public static ApartmentRoom Deserialize(byte[] buffer)
	{
		ApartmentRoom apartmentRoom = Pool.Get<ApartmentRoom>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentRoom, isDelta: false);
		return apartmentRoom;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentRoom previous)
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

	public static ApartmentRoom Deserialize(BufferStream stream, ApartmentRoom instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.furnitureIds == null)
			{
				instance.furnitureIds = Pool.Get<List<NetworkableId>>();
			}
			if (instance.owners == null)
			{
				instance.owners = Pool.Get<List<ulong>>();
			}
			if (instance.guests == null)
			{
				instance.guests = Pool.Get<List<ulong>>();
			}
			if (instance.intruders == null)
			{
				instance.intruders = Pool.Get<List<TimedUserAccess>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentRoom");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.furnitureIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.upkeepSeconds = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.upkeepTerminalId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.dailyUpkeepCost = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.outstandingRent = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.timeRentOverdue = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.owners.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.guests.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.intruders.Add(TimedUserAccess.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentRoom DeserializeLengthDelimited(BufferStream stream, ApartmentRoom instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.furnitureIds == null)
			{
				instance.furnitureIds = Pool.Get<List<NetworkableId>>();
			}
			if (instance.owners == null)
			{
				instance.owners = Pool.Get<List<ulong>>();
			}
			if (instance.guests == null)
			{
				instance.guests = Pool.Get<List<ulong>>();
			}
			if (instance.intruders == null)
			{
				instance.intruders = Pool.Get<List<TimedUserAccess>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentRoom");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.furnitureIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.upkeepSeconds = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.upkeepTerminalId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.dailyUpkeepCost = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.outstandingRent = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.timeRentOverdue = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.owners.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.guests.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.intruders.Add(TimedUserAccess.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentRoom DeserializeLength(BufferStream stream, int length, ApartmentRoom instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.furnitureIds == null)
			{
				instance.furnitureIds = Pool.Get<List<NetworkableId>>();
			}
			if (instance.owners == null)
			{
				instance.owners = Pool.Get<List<ulong>>();
			}
			if (instance.guests == null)
			{
				instance.guests = Pool.Get<List<ulong>>();
			}
			if (instance.intruders == null)
			{
				instance.intruders = Pool.Get<List<TimedUserAccess>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentRoom");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.furnitureIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.upkeepSeconds = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.upkeepTerminalId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.dailyUpkeepCost = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.outstandingRent = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				instance.timeRentOverdue = ProtocolParser.ReadSingle(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.owners.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.guests.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				stream.ConsumeRepeatedElement();
				instance.intruders.Add(TimedUserAccess.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentRoom");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentRoom instance, ApartmentRoom previous)
	{
		if (instance.roomNumber != null && instance.roomNumber != previous.roomNumber)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		if (instance.furnitureIds != null)
		{
			for (int i = 0; i < instance.furnitureIds.Count; i++)
			{
				NetworkableId networkableId = instance.furnitureIds[i];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.upkeepSeconds != previous.upkeepSeconds)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.upkeepSeconds);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.upkeepTerminalId.Value);
		if (instance.dailyUpkeepCost != previous.dailyUpkeepCost)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.dailyUpkeepCost);
		}
		if (instance.outstandingRent != previous.outstandingRent)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.outstandingRent);
		}
		if (instance.timeRentOverdue != previous.timeRentOverdue)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.timeRentOverdue);
		}
		if (instance.owners != null)
		{
			for (int j = 0; j < instance.owners.Count; j++)
			{
				ulong val = instance.owners[j];
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.guests != null)
		{
			for (int k = 0; k < instance.guests.Count; k++)
			{
				ulong val2 = instance.guests[k];
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, val2);
			}
		}
		if (instance.intruders == null)
		{
			return;
		}
		for (int l = 0; l < instance.intruders.Count; l++)
		{
			TimedUserAccess timedUserAccess = instance.intruders[l];
			stream.WriteByte(82);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			TimedUserAccess.SerializeDelta(stream, timedUserAccess, timedUserAccess);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field intruders (ProtoBuf.TimedUserAccess)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, ApartmentRoom instance)
	{
		if (instance.roomNumber != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		if (instance.furnitureIds != null)
		{
			for (int i = 0; i < instance.furnitureIds.Count; i++)
			{
				NetworkableId networkableId = instance.furnitureIds[i];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.upkeepSeconds != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.upkeepSeconds);
		}
		if (instance.upkeepTerminalId != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.upkeepTerminalId.Value);
		}
		if (instance.dailyUpkeepCost != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.dailyUpkeepCost);
		}
		if (instance.outstandingRent != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.outstandingRent);
		}
		if (instance.timeRentOverdue != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.timeRentOverdue);
		}
		if (instance.owners != null)
		{
			for (int j = 0; j < instance.owners.Count; j++)
			{
				ulong val = instance.owners[j];
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
		if (instance.guests != null)
		{
			for (int k = 0; k < instance.guests.Count; k++)
			{
				ulong val2 = instance.guests[k];
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, val2);
			}
		}
		if (instance.intruders == null)
		{
			return;
		}
		for (int l = 0; l < instance.intruders.Count; l++)
		{
			TimedUserAccess instance2 = instance.intruders[l];
			stream.WriteByte(82);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			TimedUserAccess.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field intruders (ProtoBuf.TimedUserAccess)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (furnitureIds != null)
		{
			for (int i = 0; i < furnitureIds.Count; i++)
			{
				NetworkableId value = furnitureIds[i];
				action(UidType.NetworkableId, ref value.Value);
				furnitureIds[i] = value;
			}
		}
		action(UidType.NetworkableId, ref upkeepTerminalId.Value);
		if (intruders != null)
		{
			for (int j = 0; j < intruders.Count; j++)
			{
				intruders[j]?.InspectUids(action);
			}
		}
	}
}
