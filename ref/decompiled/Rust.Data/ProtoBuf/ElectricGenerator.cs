using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ElectricGenerator : IDisposable, Pool.IPooled, IProto<ElectricGenerator>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int requiredPowergridStage;

	public static void ResetToPool(ElectricGenerator instance)
	{
		if (instance.ShouldPool)
		{
			instance.requiredPowergridStage = 0;
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
			throw new Exception("Trying to dispose ElectricGenerator with ShouldPool set to false!");
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

	public void CopyTo(ElectricGenerator instance)
	{
		instance.requiredPowergridStage = requiredPowergridStage;
	}

	public ElectricGenerator Copy()
	{
		ElectricGenerator electricGenerator = Pool.Get<ElectricGenerator>();
		CopyTo(electricGenerator);
		return electricGenerator;
	}

	public static ElectricGenerator Deserialize(BufferStream stream)
	{
		ElectricGenerator electricGenerator = Pool.Get<ElectricGenerator>();
		Deserialize(stream, electricGenerator, isDelta: false);
		return electricGenerator;
	}

	public static ElectricGenerator DeserializeLengthDelimited(BufferStream stream)
	{
		ElectricGenerator electricGenerator = Pool.Get<ElectricGenerator>();
		DeserializeLengthDelimited(stream, electricGenerator, isDelta: false);
		return electricGenerator;
	}

	public static ElectricGenerator DeserializeLength(BufferStream stream, int length)
	{
		ElectricGenerator electricGenerator = Pool.Get<ElectricGenerator>();
		DeserializeLength(stream, length, electricGenerator, isDelta: false);
		return electricGenerator;
	}

	public static ElectricGenerator Deserialize(byte[] buffer)
	{
		ElectricGenerator electricGenerator = Pool.Get<ElectricGenerator>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, electricGenerator, isDelta: false);
		return electricGenerator;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ElectricGenerator previous)
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

	public static ElectricGenerator Deserialize(BufferStream stream, ElectricGenerator instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ElectricGenerator");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElectricGenerator");
				instance.requiredPowergridStage = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElectricGenerator");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ElectricGenerator");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ElectricGenerator DeserializeLengthDelimited(BufferStream stream, ElectricGenerator instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ElectricGenerator");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElectricGenerator");
				instance.requiredPowergridStage = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElectricGenerator");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ElectricGenerator");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ElectricGenerator DeserializeLength(BufferStream stream, int length, ElectricGenerator instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ElectricGenerator");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElectricGenerator");
				instance.requiredPowergridStage = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ElectricGenerator");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ElectricGenerator");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ElectricGenerator instance, ElectricGenerator previous)
	{
		if (instance.requiredPowergridStage != previous.requiredPowergridStage)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.requiredPowergridStage);
		}
	}

	public static void Serialize(BufferStream stream, ElectricGenerator instance)
	{
		if (instance.requiredPowergridStage != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.requiredPowergridStage);
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
