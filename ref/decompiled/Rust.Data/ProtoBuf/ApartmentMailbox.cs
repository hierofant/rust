using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentMailbox : IDisposable, Pool.IPooled, IProto<ApartmentMailbox>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string roomNumber;

	[NonSerialized]
	public NetworkableId roomId;

	public static void ResetToPool(ApartmentMailbox instance)
	{
		if (instance.ShouldPool)
		{
			instance.roomNumber = string.Empty;
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
			throw new Exception("Trying to dispose ApartmentMailbox with ShouldPool set to false!");
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

	public void CopyTo(ApartmentMailbox instance)
	{
		instance.roomNumber = roomNumber;
		instance.roomId = roomId;
	}

	public ApartmentMailbox Copy()
	{
		ApartmentMailbox apartmentMailbox = Pool.Get<ApartmentMailbox>();
		CopyTo(apartmentMailbox);
		return apartmentMailbox;
	}

	public static ApartmentMailbox Deserialize(BufferStream stream)
	{
		ApartmentMailbox apartmentMailbox = Pool.Get<ApartmentMailbox>();
		Deserialize(stream, apartmentMailbox, isDelta: false);
		return apartmentMailbox;
	}

	public static ApartmentMailbox DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentMailbox apartmentMailbox = Pool.Get<ApartmentMailbox>();
		DeserializeLengthDelimited(stream, apartmentMailbox, isDelta: false);
		return apartmentMailbox;
	}

	public static ApartmentMailbox DeserializeLength(BufferStream stream, int length)
	{
		ApartmentMailbox apartmentMailbox = Pool.Get<ApartmentMailbox>();
		DeserializeLength(stream, length, apartmentMailbox, isDelta: false);
		return apartmentMailbox;
	}

	public static ApartmentMailbox Deserialize(byte[] buffer)
	{
		ApartmentMailbox apartmentMailbox = Pool.Get<ApartmentMailbox>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentMailbox, isDelta: false);
		return apartmentMailbox;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentMailbox previous)
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

	public static ApartmentMailbox Deserialize(BufferStream stream, ApartmentMailbox instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentMailbox");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				instance.roomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentMailbox DeserializeLengthDelimited(BufferStream stream, ApartmentMailbox instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentMailbox");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				instance.roomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ApartmentMailbox DeserializeLength(BufferStream stream, int length, ApartmentMailbox instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentMailbox");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				instance.roomNumber = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				instance.roomId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentMailbox");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentMailbox instance, ApartmentMailbox previous)
	{
		if (instance.roomNumber != null && instance.roomNumber != previous.roomNumber)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.roomId.Value);
	}

	public static void Serialize(BufferStream stream, ApartmentMailbox instance)
	{
		if (instance.roomNumber != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.roomNumber);
		}
		if (instance.roomId != default(NetworkableId))
		{
			stream.WriteByte(16);
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
