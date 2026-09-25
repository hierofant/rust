using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VehicleLift : IDisposable, Pool.IPooled, IProto<VehicleLift>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool platformIsOccupied;

	[NonSerialized]
	public bool editableOccupant;

	[NonSerialized]
	public bool driveableOccupant;

	[NonSerialized]
	public int occupantLockState;

	public static void ResetToPool(VehicleLift instance)
	{
		if (instance.ShouldPool)
		{
			instance.platformIsOccupied = false;
			instance.editableOccupant = false;
			instance.driveableOccupant = false;
			instance.occupantLockState = 0;
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
			throw new Exception("Trying to dispose VehicleLift with ShouldPool set to false!");
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

	public void CopyTo(VehicleLift instance)
	{
		instance.platformIsOccupied = platformIsOccupied;
		instance.editableOccupant = editableOccupant;
		instance.driveableOccupant = driveableOccupant;
		instance.occupantLockState = occupantLockState;
	}

	public VehicleLift Copy()
	{
		VehicleLift vehicleLift = Pool.Get<VehicleLift>();
		CopyTo(vehicleLift);
		return vehicleLift;
	}

	public static VehicleLift Deserialize(BufferStream stream)
	{
		VehicleLift vehicleLift = Pool.Get<VehicleLift>();
		Deserialize(stream, vehicleLift, isDelta: false);
		return vehicleLift;
	}

	public static VehicleLift DeserializeLengthDelimited(BufferStream stream)
	{
		VehicleLift vehicleLift = Pool.Get<VehicleLift>();
		DeserializeLengthDelimited(stream, vehicleLift, isDelta: false);
		return vehicleLift;
	}

	public static VehicleLift DeserializeLength(BufferStream stream, int length)
	{
		VehicleLift vehicleLift = Pool.Get<VehicleLift>();
		DeserializeLength(stream, length, vehicleLift, isDelta: false);
		return vehicleLift;
	}

	public static VehicleLift Deserialize(byte[] buffer)
	{
		VehicleLift vehicleLift = Pool.Get<VehicleLift>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vehicleLift, isDelta: false);
		return vehicleLift;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VehicleLift previous)
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

	public static VehicleLift Deserialize(BufferStream stream, VehicleLift instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VehicleLift");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.platformIsOccupied = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.editableOccupant = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.driveableOccupant = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.occupantLockState = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VehicleLift DeserializeLengthDelimited(BufferStream stream, VehicleLift instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VehicleLift");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.platformIsOccupied = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.editableOccupant = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.driveableOccupant = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.occupantLockState = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static VehicleLift DeserializeLength(BufferStream stream, int length, VehicleLift instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VehicleLift");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.platformIsOccupied = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.editableOccupant = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.driveableOccupant = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				instance.occupantLockState = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleLift");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VehicleLift instance, VehicleLift previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.platformIsOccupied);
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.editableOccupant);
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.driveableOccupant);
		if (instance.occupantLockState != previous.occupantLockState)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.occupantLockState);
		}
	}

	public static void Serialize(BufferStream stream, VehicleLift instance)
	{
		if (instance.platformIsOccupied)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.platformIsOccupied);
		}
		if (instance.editableOccupant)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.editableOccupant);
		}
		if (instance.driveableOccupant)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.driveableOccupant);
		}
		if (instance.occupantLockState != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.occupantLockState);
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
