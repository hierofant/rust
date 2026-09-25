using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class FerryRetireRequest : IDisposable, Pool.IPooled, IProto<FerryRetireRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId entityId;

	[NonSerialized]
	public long timestamp;

	public static void ResetToPool(FerryRetireRequest instance)
	{
		if (instance.ShouldPool)
		{
			instance.entityId = default(NetworkableId);
			instance.timestamp = 0L;
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
			throw new Exception("Trying to dispose FerryRetireRequest with ShouldPool set to false!");
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

	public void CopyTo(FerryRetireRequest instance)
	{
		instance.entityId = entityId;
		instance.timestamp = timestamp;
	}

	public FerryRetireRequest Copy()
	{
		FerryRetireRequest ferryRetireRequest = Pool.Get<FerryRetireRequest>();
		CopyTo(ferryRetireRequest);
		return ferryRetireRequest;
	}

	public static FerryRetireRequest Deserialize(BufferStream stream)
	{
		FerryRetireRequest ferryRetireRequest = Pool.Get<FerryRetireRequest>();
		Deserialize(stream, ferryRetireRequest, isDelta: false);
		return ferryRetireRequest;
	}

	public static FerryRetireRequest DeserializeLengthDelimited(BufferStream stream)
	{
		FerryRetireRequest ferryRetireRequest = Pool.Get<FerryRetireRequest>();
		DeserializeLengthDelimited(stream, ferryRetireRequest, isDelta: false);
		return ferryRetireRequest;
	}

	public static FerryRetireRequest DeserializeLength(BufferStream stream, int length)
	{
		FerryRetireRequest ferryRetireRequest = Pool.Get<FerryRetireRequest>();
		DeserializeLength(stream, length, ferryRetireRequest, isDelta: false);
		return ferryRetireRequest;
	}

	public static FerryRetireRequest Deserialize(byte[] buffer)
	{
		FerryRetireRequest ferryRetireRequest = Pool.Get<FerryRetireRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ferryRetireRequest, isDelta: false);
		return ferryRetireRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, FerryRetireRequest previous)
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

	public static FerryRetireRequest Deserialize(BufferStream stream, FerryRetireRequest instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryRetireRequest");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FerryRetireRequest DeserializeLengthDelimited(BufferStream stream, FerryRetireRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryRetireRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FerryRetireRequest DeserializeLength(BufferStream stream, int length, FerryRetireRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryRetireRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryRetireRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, FerryRetireRequest instance, FerryRetireRequest previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
	}

	public static void Serialize(BufferStream stream, FerryRetireRequest instance)
	{
		if (instance.entityId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		}
		if (instance.timestamp != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entityId.Value);
	}
}
