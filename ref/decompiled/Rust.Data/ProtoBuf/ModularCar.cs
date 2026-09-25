using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ModularCar : IDisposable, Pool.IPooled, IProto<ModularCar>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float steerAngle;

	[NonSerialized]
	public float driveWheelVel;

	[NonSerialized]
	public float throttleInput;

	[NonSerialized]
	public float brakeInput;

	[NonSerialized]
	public NetworkableId fuelStorageID;

	[NonSerialized]
	public float fuelFraction;

	[NonSerialized]
	public bool hasLock;

	[NonSerialized]
	public string lockCode;

	[NonSerialized]
	public List<ulong> whitelistUsers;

	public static void ResetToPool(ModularCar instance)
	{
		if (instance.ShouldPool)
		{
			instance.steerAngle = 0f;
			instance.driveWheelVel = 0f;
			instance.throttleInput = 0f;
			instance.brakeInput = 0f;
			instance.fuelStorageID = default(NetworkableId);
			instance.fuelFraction = 0f;
			instance.hasLock = false;
			instance.lockCode = string.Empty;
			if (instance.whitelistUsers != null)
			{
				List<ulong> obj = instance.whitelistUsers;
				Pool.FreeUnmanaged(ref obj);
				instance.whitelistUsers = obj;
			}
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
			throw new Exception("Trying to dispose ModularCar with ShouldPool set to false!");
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

	public void CopyTo(ModularCar instance)
	{
		instance.steerAngle = steerAngle;
		instance.driveWheelVel = driveWheelVel;
		instance.throttleInput = throttleInput;
		instance.brakeInput = brakeInput;
		instance.fuelStorageID = fuelStorageID;
		instance.fuelFraction = fuelFraction;
		instance.hasLock = hasLock;
		instance.lockCode = lockCode;
		if (whitelistUsers != null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
			for (int i = 0; i < whitelistUsers.Count; i++)
			{
				ulong item = whitelistUsers[i];
				instance.whitelistUsers.Add(item);
			}
		}
		else
		{
			instance.whitelistUsers = null;
		}
	}

	public ModularCar Copy()
	{
		ModularCar modularCar = Pool.Get<ModularCar>();
		CopyTo(modularCar);
		return modularCar;
	}

	public static ModularCar Deserialize(BufferStream stream)
	{
		ModularCar modularCar = Pool.Get<ModularCar>();
		Deserialize(stream, modularCar, isDelta: false);
		return modularCar;
	}

	public static ModularCar DeserializeLengthDelimited(BufferStream stream)
	{
		ModularCar modularCar = Pool.Get<ModularCar>();
		DeserializeLengthDelimited(stream, modularCar, isDelta: false);
		return modularCar;
	}

	public static ModularCar DeserializeLength(BufferStream stream, int length)
	{
		ModularCar modularCar = Pool.Get<ModularCar>();
		DeserializeLength(stream, length, modularCar, isDelta: false);
		return modularCar;
	}

	public static ModularCar Deserialize(byte[] buffer)
	{
		ModularCar modularCar = Pool.Get<ModularCar>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, modularCar, isDelta: false);
		return modularCar;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ModularCar previous)
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

	public static ModularCar Deserialize(BufferStream stream, ModularCar instance, bool isDelta)
	{
		if (!isDelta && instance.whitelistUsers == null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ModularCar");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.steerAngle = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.driveWheelVel = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.throttleInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.brakeInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.fuelFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				stream.ConsumeRepeatedElement();
				instance.whitelistUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ModularCar DeserializeLengthDelimited(BufferStream stream, ModularCar instance, bool isDelta)
	{
		if (!isDelta && instance.whitelistUsers == null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ModularCar");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.steerAngle = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.driveWheelVel = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.throttleInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.brakeInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.fuelFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				stream.ConsumeRepeatedElement();
				instance.whitelistUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ModularCar DeserializeLength(BufferStream stream, int length, ModularCar instance, bool isDelta)
	{
		if (!isDelta && instance.whitelistUsers == null)
		{
			instance.whitelistUsers = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ModularCar");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.steerAngle = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.driveWheelVel = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.throttleInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.brakeInput = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.fuelFraction = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.hasLock = ProtocolParser.ReadBool(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				instance.lockCode = ProtocolParser.ReadString(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				stream.ConsumeRepeatedElement();
				instance.whitelistUsers.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ModularCar");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ModularCar instance, ModularCar previous)
	{
		if (instance.steerAngle != previous.steerAngle)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.steerAngle);
		}
		if (instance.driveWheelVel != previous.driveWheelVel)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.driveWheelVel);
		}
		if (instance.throttleInput != previous.throttleInput)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.throttleInput);
		}
		if (instance.brakeInput != previous.brakeInput)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.brakeInput);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteUInt64(stream, instance.fuelStorageID.Value);
		if (instance.fuelFraction != previous.fuelFraction)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.fuelFraction);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.hasLock);
		if (instance.lockCode != null && instance.lockCode != previous.lockCode)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteString(stream, instance.lockCode);
		}
		if (instance.whitelistUsers != null)
		{
			for (int i = 0; i < instance.whitelistUsers.Count; i++)
			{
				ulong val = instance.whitelistUsers[i];
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, ModularCar instance)
	{
		if (instance.steerAngle != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.steerAngle);
		}
		if (instance.driveWheelVel != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.driveWheelVel);
		}
		if (instance.throttleInput != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.throttleInput);
		}
		if (instance.brakeInput != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.brakeInput);
		}
		if (instance.fuelStorageID != default(NetworkableId))
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.fuelStorageID.Value);
		}
		if (instance.fuelFraction != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.fuelFraction);
		}
		if (instance.hasLock)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.hasLock);
		}
		if (instance.lockCode != null)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteString(stream, instance.lockCode);
		}
		if (instance.whitelistUsers != null)
		{
			for (int i = 0; i < instance.whitelistUsers.Count; i++)
			{
				ulong val = instance.whitelistUsers[i];
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, val);
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref fuelStorageID.Value);
	}
}
