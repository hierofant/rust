using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentBuilding : IDisposable, Pool.IPooled, IProto<ApartmentBuilding>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int smallRoomsLeft;

	[NonSerialized]
	public int mediumRoomsLeft;

	[NonSerialized]
	public int largeRoomsLeft;

	[NonSerialized]
	public NetworkableId clientRoomId;

	[NonSerialized]
	public string clientRoomNumber;

	[NonSerialized]
	public int clientRent;

	[NonSerialized]
	public int clientRoomSize;

	public static void ResetToPool(ApartmentBuilding instance)
	{
		if (instance.ShouldPool)
		{
			instance.smallRoomsLeft = 0;
			instance.mediumRoomsLeft = 0;
			instance.largeRoomsLeft = 0;
			instance.clientRoomId = default(NetworkableId);
			instance.clientRoomNumber = string.Empty;
			instance.clientRent = 0;
			instance.clientRoomSize = 0;
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
			throw new Exception("Trying to dispose ApartmentBuilding with ShouldPool set to false!");
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

	public void CopyTo(ApartmentBuilding instance)
	{
		instance.smallRoomsLeft = smallRoomsLeft;
		instance.mediumRoomsLeft = mediumRoomsLeft;
		instance.largeRoomsLeft = largeRoomsLeft;
		instance.clientRoomId = clientRoomId;
		instance.clientRoomNumber = clientRoomNumber;
		instance.clientRent = clientRent;
		instance.clientRoomSize = clientRoomSize;
	}

	public ApartmentBuilding Copy()
	{
		ApartmentBuilding apartmentBuilding = Pool.Get<ApartmentBuilding>();
		CopyTo(apartmentBuilding);
		return apartmentBuilding;
	}

	public static ApartmentBuilding Deserialize(BufferStream stream)
	{
		ApartmentBuilding apartmentBuilding = Pool.Get<ApartmentBuilding>();
		Deserialize(stream, apartmentBuilding, isDelta: false);
		return apartmentBuilding;
	}

	public static ApartmentBuilding DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentBuilding apartmentBuilding = Pool.Get<ApartmentBuilding>();
		DeserializeLengthDelimited(stream, apartmentBuilding, isDelta: false);
		return apartmentBuilding;
	}

	public static ApartmentBuilding DeserializeLength(BufferStream stream, int length)
	{
		ApartmentBuilding apartmentBuilding = Pool.Get<ApartmentBuilding>();
		DeserializeLength(stream, length, apartmentBuilding, isDelta: false);
		return apartmentBuilding;
	}

	public static ApartmentBuilding Deserialize(byte[] buffer)
	{
		ApartmentBuilding apartmentBuilding = Pool.Get<ApartmentBuilding>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentBuilding, isDelta: false);
		return apartmentBuilding;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentBuilding previous)
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

	public static ApartmentBuilding Deserialize(BufferStream stream, ApartmentBuilding instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentBuilding");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.smallRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.mediumRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.largeRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRent = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentBuilding DeserializeLengthDelimited(BufferStream stream, ApartmentBuilding instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentBuilding");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.smallRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.mediumRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.largeRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRent = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentBuilding DeserializeLength(BufferStream stream, int length, ApartmentBuilding instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentBuilding");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.smallRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.mediumRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.largeRoomsLeft = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRent = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				instance.clientRoomSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentBuilding instance, ApartmentBuilding previous)
	{
		if (instance.smallRoomsLeft != previous.smallRoomsLeft)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.smallRoomsLeft);
		}
		if (instance.mediumRoomsLeft != previous.mediumRoomsLeft)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.mediumRoomsLeft);
		}
		if (instance.largeRoomsLeft != previous.largeRoomsLeft)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.largeRoomsLeft);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.clientRoomId.Value);
		if (instance.clientRoomNumber != null && instance.clientRoomNumber != previous.clientRoomNumber)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.clientRoomNumber);
		}
		if (instance.clientRent != previous.clientRent)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clientRent);
		}
		if (instance.clientRoomSize != previous.clientRoomSize)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clientRoomSize);
		}
	}

	public static void Serialize(BufferStream stream, ApartmentBuilding instance)
	{
		if (instance.smallRoomsLeft != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.smallRoomsLeft);
		}
		if (instance.mediumRoomsLeft != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.mediumRoomsLeft);
		}
		if (instance.largeRoomsLeft != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.largeRoomsLeft);
		}
		if (instance.clientRoomId != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.clientRoomId.Value);
		}
		if (instance.clientRoomNumber != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.clientRoomNumber);
		}
		if (instance.clientRent != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clientRent);
		}
		if (instance.clientRoomSize != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clientRoomSize);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref clientRoomId.Value);
	}
}
