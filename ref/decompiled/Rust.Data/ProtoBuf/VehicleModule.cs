using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class VehicleModule : IDisposable, Pool.IPooled, IProto<VehicleModule>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int socketIndex;

	public static void ResetToPool(VehicleModule instance)
	{
		if (instance.ShouldPool)
		{
			instance.socketIndex = 0;
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
			throw new Exception("Trying to dispose VehicleModule with ShouldPool set to false!");
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

	public void CopyTo(VehicleModule instance)
	{
		instance.socketIndex = socketIndex;
	}

	public VehicleModule Copy()
	{
		VehicleModule vehicleModule = Pool.Get<VehicleModule>();
		CopyTo(vehicleModule);
		return vehicleModule;
	}

	public static VehicleModule Deserialize(BufferStream stream)
	{
		VehicleModule vehicleModule = Pool.Get<VehicleModule>();
		Deserialize(stream, vehicleModule, isDelta: false);
		return vehicleModule;
	}

	public static VehicleModule DeserializeLengthDelimited(BufferStream stream)
	{
		VehicleModule vehicleModule = Pool.Get<VehicleModule>();
		DeserializeLengthDelimited(stream, vehicleModule, isDelta: false);
		return vehicleModule;
	}

	public static VehicleModule DeserializeLength(BufferStream stream, int length)
	{
		VehicleModule vehicleModule = Pool.Get<VehicleModule>();
		DeserializeLength(stream, length, vehicleModule, isDelta: false);
		return vehicleModule;
	}

	public static VehicleModule Deserialize(byte[] buffer)
	{
		VehicleModule vehicleModule = Pool.Get<VehicleModule>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, vehicleModule, isDelta: false);
		return vehicleModule;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, VehicleModule previous)
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

	public static VehicleModule Deserialize(BufferStream stream, VehicleModule instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.VehicleModule");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleModule");
				instance.socketIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleModule");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleModule");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static VehicleModule DeserializeLengthDelimited(BufferStream stream, VehicleModule instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VehicleModule");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleModule");
				instance.socketIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleModule");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleModule");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static VehicleModule DeserializeLength(BufferStream stream, int length, VehicleModule instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.VehicleModule");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleModule");
				instance.socketIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.VehicleModule");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.VehicleModule");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, VehicleModule instance, VehicleModule previous)
	{
		if (instance.socketIndex != previous.socketIndex)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.socketIndex);
		}
	}

	public static void Serialize(BufferStream stream, VehicleModule instance)
	{
		if (instance.socketIndex != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.socketIndex);
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
