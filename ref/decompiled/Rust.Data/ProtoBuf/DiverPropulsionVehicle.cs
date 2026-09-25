using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DiverPropulsionVehicle : IDisposable, Pool.IPooled, IProto<DiverPropulsionVehicle>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId fuelStorageID;

	[NonSerialized]
	public int fuelAmount;

	[NonSerialized]
	public int fuelTicks;

	public static void ResetToPool(DiverPropulsionVehicle instance)
	{
		if (instance.ShouldPool)
		{
			instance.fuelStorageID = default(NetworkableId);
			instance.fuelAmount = 0;
			instance.fuelTicks = 0;
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
			throw new Exception("Trying to dispose DiverPropulsionVehicle with ShouldPool set to false!");
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

	public void CopyTo(DiverPropulsionVehicle instance)
	{
		instance.fuelStorageID = fuelStorageID;
		instance.fuelAmount = fuelAmount;
		instance.fuelTicks = fuelTicks;
	}

	public DiverPropulsionVehicle Copy()
	{
		DiverPropulsionVehicle diverPropulsionVehicle = Pool.Get<DiverPropulsionVehicle>();
		CopyTo(diverPropulsionVehicle);
		return diverPropulsionVehicle;
	}

	public static DiverPropulsionVehicle Deserialize(BufferStream stream)
	{
		DiverPropulsionVehicle diverPropulsionVehicle = Pool.Get<DiverPropulsionVehicle>();
		Deserialize(stream, diverPropulsionVehicle, isDelta: false);
		return diverPropulsionVehicle;
	}

	public static DiverPropulsionVehicle DeserializeLengthDelimited(BufferStream stream)
	{
		DiverPropulsionVehicle diverPropulsionVehicle = Pool.Get<DiverPropulsionVehicle>();
		DeserializeLengthDelimited(stream, diverPropulsionVehicle, isDelta: false);
		return diverPropulsionVehicle;
	}

	public static DiverPropulsionVehicle DeserializeLength(BufferStream stream, int length)
	{
		DiverPropulsionVehicle diverPropulsionVehicle = Pool.Get<DiverPropulsionVehicle>();
		DeserializeLength(stream, length, diverPropulsionVehicle, isDelta: false);
		return diverPropulsionVehicle;
	}

	public static DiverPropulsionVehicle Deserialize(byte[] buffer)
	{
		DiverPropulsionVehicle diverPropulsionVehicle = Pool.Get<DiverPropulsionVehicle>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, diverPropulsionVehicle, isDelta: false);
		return diverPropulsionVehicle;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DiverPropulsionVehicle previous)
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

	public static DiverPropulsionVehicle Deserialize(BufferStream stream, DiverPropulsionVehicle instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DiverPropulsionVehicle");
			switch (num)
			{
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelTicks = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DiverPropulsionVehicle DeserializeLengthDelimited(BufferStream stream, DiverPropulsionVehicle instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DiverPropulsionVehicle");
			switch (num2)
			{
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelTicks = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DiverPropulsionVehicle DeserializeLength(BufferStream stream, int length, DiverPropulsionVehicle instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.DiverPropulsionVehicle");
			switch (num2)
			{
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelStorageID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelAmount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				instance.fuelTicks = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DiverPropulsionVehicle");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DiverPropulsionVehicle instance, DiverPropulsionVehicle previous)
	{
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.fuelStorageID.Value);
		if (instance.fuelAmount != previous.fuelAmount)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fuelAmount);
		}
		if (instance.fuelTicks != previous.fuelTicks)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fuelTicks);
		}
	}

	public static void Serialize(BufferStream stream, DiverPropulsionVehicle instance)
	{
		if (instance.fuelStorageID != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.fuelStorageID.Value);
		}
		if (instance.fuelAmount != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fuelAmount);
		}
		if (instance.fuelTicks != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.fuelTicks);
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
