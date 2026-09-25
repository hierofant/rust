using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class SleepingBagDestroyRequest : IDisposable, Pool.IPooled, IProto<SleepingBagDestroyRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong userId;

	[NonSerialized]
	public NetworkableId sleepingBagId;

	public static void ResetToPool(SleepingBagDestroyRequest instance)
	{
		if (instance.ShouldPool)
		{
			instance.userId = 0uL;
			instance.sleepingBagId = default(NetworkableId);
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
			throw new Exception("Trying to dispose SleepingBagDestroyRequest with ShouldPool set to false!");
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

	public void CopyTo(SleepingBagDestroyRequest instance)
	{
		instance.userId = userId;
		instance.sleepingBagId = sleepingBagId;
	}

	public SleepingBagDestroyRequest Copy()
	{
		SleepingBagDestroyRequest sleepingBagDestroyRequest = Pool.Get<SleepingBagDestroyRequest>();
		CopyTo(sleepingBagDestroyRequest);
		return sleepingBagDestroyRequest;
	}

	public static SleepingBagDestroyRequest Deserialize(BufferStream stream)
	{
		SleepingBagDestroyRequest sleepingBagDestroyRequest = Pool.Get<SleepingBagDestroyRequest>();
		Deserialize(stream, sleepingBagDestroyRequest, isDelta: false);
		return sleepingBagDestroyRequest;
	}

	public static SleepingBagDestroyRequest DeserializeLengthDelimited(BufferStream stream)
	{
		SleepingBagDestroyRequest sleepingBagDestroyRequest = Pool.Get<SleepingBagDestroyRequest>();
		DeserializeLengthDelimited(stream, sleepingBagDestroyRequest, isDelta: false);
		return sleepingBagDestroyRequest;
	}

	public static SleepingBagDestroyRequest DeserializeLength(BufferStream stream, int length)
	{
		SleepingBagDestroyRequest sleepingBagDestroyRequest = Pool.Get<SleepingBagDestroyRequest>();
		DeserializeLength(stream, length, sleepingBagDestroyRequest, isDelta: false);
		return sleepingBagDestroyRequest;
	}

	public static SleepingBagDestroyRequest Deserialize(byte[] buffer)
	{
		SleepingBagDestroyRequest sleepingBagDestroyRequest = Pool.Get<SleepingBagDestroyRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sleepingBagDestroyRequest, isDelta: false);
		return sleepingBagDestroyRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SleepingBagDestroyRequest previous)
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

	public static SleepingBagDestroyRequest Deserialize(BufferStream stream, SleepingBagDestroyRequest instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SleepingBagDestroyRequest");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				instance.sleepingBagId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SleepingBagDestroyRequest DeserializeLengthDelimited(BufferStream stream, SleepingBagDestroyRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SleepingBagDestroyRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				instance.sleepingBagId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SleepingBagDestroyRequest DeserializeLength(BufferStream stream, int length, SleepingBagDestroyRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SleepingBagDestroyRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				instance.sleepingBagId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SleepingBagDestroyRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SleepingBagDestroyRequest instance, SleepingBagDestroyRequest previous)
	{
		if (instance.userId != previous.userId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userId);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.sleepingBagId.Value);
	}

	public static void Serialize(BufferStream stream, SleepingBagDestroyRequest instance)
	{
		if (instance.userId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userId);
		}
		if (instance.sleepingBagId != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.sleepingBagId.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref sleepingBagId.Value);
	}
}
