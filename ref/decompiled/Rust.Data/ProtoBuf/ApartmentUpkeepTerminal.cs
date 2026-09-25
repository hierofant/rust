using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ApartmentUpkeepTerminal : IDisposable, Pool.IPooled, IProto<ApartmentUpkeepTerminal>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId apartmentId;

	public static void ResetToPool(ApartmentUpkeepTerminal instance)
	{
		if (instance.ShouldPool)
		{
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
			throw new Exception("Trying to dispose ApartmentUpkeepTerminal with ShouldPool set to false!");
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

	public void CopyTo(ApartmentUpkeepTerminal instance)
	{
		instance.apartmentId = apartmentId;
	}

	public ApartmentUpkeepTerminal Copy()
	{
		ApartmentUpkeepTerminal apartmentUpkeepTerminal = Pool.Get<ApartmentUpkeepTerminal>();
		CopyTo(apartmentUpkeepTerminal);
		return apartmentUpkeepTerminal;
	}

	public static ApartmentUpkeepTerminal Deserialize(BufferStream stream)
	{
		ApartmentUpkeepTerminal apartmentUpkeepTerminal = Pool.Get<ApartmentUpkeepTerminal>();
		Deserialize(stream, apartmentUpkeepTerminal, isDelta: false);
		return apartmentUpkeepTerminal;
	}

	public static ApartmentUpkeepTerminal DeserializeLengthDelimited(BufferStream stream)
	{
		ApartmentUpkeepTerminal apartmentUpkeepTerminal = Pool.Get<ApartmentUpkeepTerminal>();
		DeserializeLengthDelimited(stream, apartmentUpkeepTerminal, isDelta: false);
		return apartmentUpkeepTerminal;
	}

	public static ApartmentUpkeepTerminal DeserializeLength(BufferStream stream, int length)
	{
		ApartmentUpkeepTerminal apartmentUpkeepTerminal = Pool.Get<ApartmentUpkeepTerminal>();
		DeserializeLength(stream, length, apartmentUpkeepTerminal, isDelta: false);
		return apartmentUpkeepTerminal;
	}

	public static ApartmentUpkeepTerminal Deserialize(byte[] buffer)
	{
		ApartmentUpkeepTerminal apartmentUpkeepTerminal = Pool.Get<ApartmentUpkeepTerminal>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, apartmentUpkeepTerminal, isDelta: false);
		return apartmentUpkeepTerminal;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ApartmentUpkeepTerminal previous)
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

	public static ApartmentUpkeepTerminal Deserialize(BufferStream stream, ApartmentUpkeepTerminal instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentUpkeepTerminal");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentUpkeepTerminal");
				instance.apartmentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentUpkeepTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentUpkeepTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ApartmentUpkeepTerminal DeserializeLengthDelimited(BufferStream stream, ApartmentUpkeepTerminal instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentUpkeepTerminal");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentUpkeepTerminal");
				instance.apartmentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentUpkeepTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentUpkeepTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ApartmentUpkeepTerminal DeserializeLength(BufferStream stream, int length, ApartmentUpkeepTerminal instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ApartmentUpkeepTerminal");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentUpkeepTerminal");
				instance.apartmentId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ApartmentUpkeepTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ApartmentUpkeepTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ApartmentUpkeepTerminal instance, ApartmentUpkeepTerminal previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.apartmentId.Value);
	}

	public static void Serialize(BufferStream stream, ApartmentUpkeepTerminal instance)
	{
		if (instance.apartmentId != default(NetworkableId))
		{
			stream.WriteByte(8);
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
