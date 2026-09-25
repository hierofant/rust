using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class SpawnOptionsRequest : IDisposable, Pool.IPooled, IProto<SpawnOptionsRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong userId;

	public static void ResetToPool(SpawnOptionsRequest instance)
	{
		if (instance.ShouldPool)
		{
			instance.userId = 0uL;
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
			throw new Exception("Trying to dispose SpawnOptionsRequest with ShouldPool set to false!");
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

	public void CopyTo(SpawnOptionsRequest instance)
	{
		instance.userId = userId;
	}

	public SpawnOptionsRequest Copy()
	{
		SpawnOptionsRequest spawnOptionsRequest = Pool.Get<SpawnOptionsRequest>();
		CopyTo(spawnOptionsRequest);
		return spawnOptionsRequest;
	}

	public static SpawnOptionsRequest Deserialize(BufferStream stream)
	{
		SpawnOptionsRequest spawnOptionsRequest = Pool.Get<SpawnOptionsRequest>();
		Deserialize(stream, spawnOptionsRequest, isDelta: false);
		return spawnOptionsRequest;
	}

	public static SpawnOptionsRequest DeserializeLengthDelimited(BufferStream stream)
	{
		SpawnOptionsRequest spawnOptionsRequest = Pool.Get<SpawnOptionsRequest>();
		DeserializeLengthDelimited(stream, spawnOptionsRequest, isDelta: false);
		return spawnOptionsRequest;
	}

	public static SpawnOptionsRequest DeserializeLength(BufferStream stream, int length)
	{
		SpawnOptionsRequest spawnOptionsRequest = Pool.Get<SpawnOptionsRequest>();
		DeserializeLength(stream, length, spawnOptionsRequest, isDelta: false);
		return spawnOptionsRequest;
	}

	public static SpawnOptionsRequest Deserialize(byte[] buffer)
	{
		SpawnOptionsRequest spawnOptionsRequest = Pool.Get<SpawnOptionsRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, spawnOptionsRequest, isDelta: false);
		return spawnOptionsRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SpawnOptionsRequest previous)
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

	public static SpawnOptionsRequest Deserialize(BufferStream stream, SpawnOptionsRequest instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SpawnOptionsRequest");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SpawnOptionsRequest");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SpawnOptionsRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpawnOptionsRequest DeserializeLengthDelimited(BufferStream stream, SpawnOptionsRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SpawnOptionsRequest");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SpawnOptionsRequest");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SpawnOptionsRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpawnOptionsRequest DeserializeLength(BufferStream stream, int length, SpawnOptionsRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.SpawnOptionsRequest");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SpawnOptionsRequest");
				instance.userId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.SpawnOptionsRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.SpawnOptionsRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SpawnOptionsRequest instance, SpawnOptionsRequest previous)
	{
		if (instance.userId != previous.userId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userId);
		}
	}

	public static void Serialize(BufferStream stream, SpawnOptionsRequest instance)
	{
		if (instance.userId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userId);
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
