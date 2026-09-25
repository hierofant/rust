using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ElevatorLift : IDisposable, Pool.IPooled, IProto<ElevatorLift>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId owner;

	[NonSerialized]
	public float topElevatorHeight;

	public static void ResetToPool(ElevatorLift instance)
	{
		if (instance.ShouldPool)
		{
			instance.owner = default(NetworkableId);
			instance.topElevatorHeight = 0f;
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
			throw new Exception("Trying to dispose ElevatorLift with ShouldPool set to false!");
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

	public void CopyTo(ElevatorLift instance)
	{
		instance.owner = owner;
		instance.topElevatorHeight = topElevatorHeight;
	}

	public ElevatorLift Copy()
	{
		ElevatorLift elevatorLift = Pool.Get<ElevatorLift>();
		CopyTo(elevatorLift);
		return elevatorLift;
	}

	public static ElevatorLift Deserialize(BufferStream stream)
	{
		ElevatorLift elevatorLift = Pool.Get<ElevatorLift>();
		Deserialize(stream, elevatorLift, isDelta: false);
		return elevatorLift;
	}

	public static ElevatorLift DeserializeLengthDelimited(BufferStream stream)
	{
		ElevatorLift elevatorLift = Pool.Get<ElevatorLift>();
		DeserializeLengthDelimited(stream, elevatorLift, isDelta: false);
		return elevatorLift;
	}

	public static ElevatorLift DeserializeLength(BufferStream stream, int length)
	{
		ElevatorLift elevatorLift = Pool.Get<ElevatorLift>();
		DeserializeLength(stream, length, elevatorLift, isDelta: false);
		return elevatorLift;
	}

	public static ElevatorLift Deserialize(byte[] buffer)
	{
		ElevatorLift elevatorLift = Pool.Get<ElevatorLift>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, elevatorLift, isDelta: false);
		return elevatorLift;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ElevatorLift previous)
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

	public static ElevatorLift Deserialize(BufferStream stream, ElevatorLift instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ElevatorLift");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				instance.owner = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				instance.topElevatorHeight = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ElevatorLift DeserializeLengthDelimited(BufferStream stream, ElevatorLift instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ElevatorLift");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				instance.owner = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				instance.topElevatorHeight = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ElevatorLift DeserializeLength(BufferStream stream, int length, ElevatorLift instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ElevatorLift");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				instance.owner = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				instance.topElevatorHeight = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ElevatorLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ElevatorLift instance, ElevatorLift previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.owner.Value);
		if (instance.topElevatorHeight != previous.topElevatorHeight)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.topElevatorHeight);
		}
	}

	public static void Serialize(BufferStream stream, ElevatorLift instance)
	{
		if (instance.owner != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.owner.Value);
		}
		if (instance.topElevatorHeight != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.topElevatorHeight);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref owner.Value);
	}
}
