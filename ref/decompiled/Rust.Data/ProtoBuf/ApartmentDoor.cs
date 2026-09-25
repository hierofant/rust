using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentDoor : IDisposable, Pool.IPooled, IProto<ApartmentDoor>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string roomNumber;

	[NonSerialized]
	public NetworkableId apartmentId;

	public static void ResetToPool(ApartmentDoor instance)
	{
		if (instance.ShouldPool)
		{
			instance.roomNumber = string.Empty;
			instance.apartmentId = default(NetworkableId);
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
			throw new Exception("Trying to dispose ApartmentDoor with ShouldPool set to false!");
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

	public void CopyTo(ApartmentDoor instance)
	{
		instance.roomNumber = roomNumber;
		instance.apartmentId = apartmentId;
	}

	public ApartmentDoor Copy()
	{
		ApartmentDoor apartmentDoor = Pool.Get<ApartmentDoor>();
		CopyTo(apartmentDoor);
		return apartmentDoor;
	}

	public static ApartmentDoor Deserialize(BufferStream stream)
	{
		ApartmentDoor apartmentDoor = Pool.Get<ApartmentDoor>();
		Deserialize(stream, apartmentDoor, isDelta: false);
		return apartmentDoor;
	}

	public static ApartmentDoor DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentDoor apartmentDoor = Pool.Get<ApartmentDoor>();
		DeserializeLengthDelimited(stream, apartmentDoor, isDelta: false);
		return apartmentDoor;
	}

	public static ApartmentDoor DeserializeLength(BufferStream stream, int length)
	{
		ApartmentDoor apartmentDoor = Pool.Get<ApartmentDoor>();
		DeserializeLength(stream, length, apartmentDoor, isDelta: false);
		return apartmentDoor;
	}

	public static ApartmentDoor Deserialize(byte[] buffer)
	{
		ApartmentDoor apartmentDoor = Pool.Get<ApartmentDoor>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentDoor, isDelta: false);
		return apartmentDoor;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentDoor previous)
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

	public static ApartmentDoor Deserialize(BufferStream stream, ApartmentDoor instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentDoor");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				instance.apartmentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentDoor DeserializeLengthDelimited(BufferStream stream, ApartmentDoor instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentDoor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				instance.apartmentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentDoor DeserializeLength(BufferStream stream, int length, ApartmentDoor instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentDoor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				instance.apartmentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentDoor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentDoor instance, ApartmentDoor previous)
	{
		if (instance.roomNumber != null && instance.roomNumber != previous.roomNumber)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.apartmentId.Value);
	}

	public static void Serialize(BufferStream stream, ApartmentDoor instance)
	{
		if (instance.roomNumber != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		if (instance.apartmentId != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.apartmentId.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref apartmentId.Value);
	}
}
