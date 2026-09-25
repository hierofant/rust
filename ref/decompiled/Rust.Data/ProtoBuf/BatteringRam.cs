using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BatteringRam : IDisposable, Pool.IPooled, IProto<BatteringRam>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId fuelStorageID;

	[NonSerialized]
	public NetworkableId headID;

	[NonSerialized]
	public float steerInput;

	[NonSerialized]
	public float driveWheelVel;

	[NonSerialized]
	public float throttleInput;

	[NonSerialized]
	public float brakeInput;

	[NonSerialized]
	public float fuelFraction;

	[NonSerialized]
	public float doorAngle;

	[NonSerialized]
	public float time;

	public static void ResetToPool(BatteringRam instance)
	{
		if (instance.ShouldPool)
		{
			instance.fuelStorageID = default(NetworkableId);
			instance.headID = default(NetworkableId);
			instance.steerInput = 0f;
			instance.driveWheelVel = 0f;
			instance.throttleInput = 0f;
			instance.brakeInput = 0f;
			instance.fuelFraction = 0f;
			instance.doorAngle = 0f;
			instance.time = 0f;
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
			throw new Exception("Trying to dispose BatteringRam with ShouldPool set to false!");
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

	public void CopyTo(BatteringRam instance)
	{
		instance.fuelStorageID = fuelStorageID;
		instance.headID = headID;
		instance.steerInput = steerInput;
		instance.driveWheelVel = driveWheelVel;
		instance.throttleInput = throttleInput;
		instance.brakeInput = brakeInput;
		instance.fuelFraction = fuelFraction;
		instance.doorAngle = doorAngle;
		instance.time = time;
	}

	public BatteringRam Copy()
	{
		BatteringRam batteringRam = Pool.Get<BatteringRam>();
		CopyTo(batteringRam);
		return batteringRam;
	}

	public static BatteringRam Deserialize(BufferStream stream)
	{
		BatteringRam batteringRam = Pool.Get<BatteringRam>();
		Deserialize(stream, batteringRam, isDelta: false);
		return batteringRam;
	}

	public static BatteringRam DeserializeLengthDelimited(BufferStream stream)
	{
		BatteringRam batteringRam = Pool.Get<BatteringRam>();
		DeserializeLengthDelimited(stream, batteringRam, isDelta: false);
		return batteringRam;
	}

	public static BatteringRam DeserializeLength(BufferStream stream, int length)
	{
		BatteringRam batteringRam = Pool.Get<BatteringRam>();
		DeserializeLength(stream, length, batteringRam, isDelta: false);
		return batteringRam;
	}

	public static BatteringRam Deserialize(byte[] buffer)
	{
		BatteringRam batteringRam = Pool.Get<BatteringRam>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, batteringRam, isDelta: false);
		return batteringRam;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BatteringRam previous)
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

	public static BatteringRam Deserialize(BufferStream stream, BatteringRam instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BatteringRam");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.headID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.steerInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.driveWheelVel = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.throttleInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.brakeInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.fuelFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.doorAngle = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.time = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BatteringRam DeserializeLengthDelimited(BufferStream stream, BatteringRam instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BatteringRam");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.headID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.steerInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.driveWheelVel = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.throttleInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.brakeInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.fuelFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.doorAngle = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.time = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BatteringRam DeserializeLength(BufferStream stream, int length, BatteringRam instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BatteringRam");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.headID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.steerInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.driveWheelVel = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.throttleInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.brakeInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.fuelFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.doorAngle = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				instance.time = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BatteringRam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BatteringRam instance, BatteringRam previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.fuelStorageID.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.headID.Value);
		if (instance.steerInput != previous.steerInput)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.steerInput);
		}
		if (instance.driveWheelVel != previous.driveWheelVel)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.driveWheelVel);
		}
		if (instance.throttleInput != previous.throttleInput)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.throttleInput);
		}
		if (instance.brakeInput != previous.brakeInput)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.brakeInput);
		}
		if (instance.fuelFraction != previous.fuelFraction)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.fuelFraction);
		}
		if (instance.doorAngle != previous.doorAngle)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.doorAngle);
		}
		if (instance.time != previous.time)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.time);
		}
	}

	public static void Serialize(BufferStream stream, BatteringRam instance)
	{
		if (instance.fuelStorageID != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.fuelStorageID.Value);
		}
		if (instance.headID != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.headID.Value);
		}
		if (instance.steerInput != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.steerInput);
		}
		if (instance.driveWheelVel != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.driveWheelVel);
		}
		if (instance.throttleInput != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.throttleInput);
		}
		if (instance.brakeInput != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.brakeInput);
		}
		if (instance.fuelFraction != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.fuelFraction);
		}
		if (instance.doorAngle != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.doorAngle);
		}
		if (instance.time != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.time);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref fuelStorageID.Value);
		action(UidType.NetworkableId, ref headID.Value);
	}
}
