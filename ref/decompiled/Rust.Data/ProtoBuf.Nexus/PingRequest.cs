using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class PingRequest : IDisposable, Pool.IPooled, IProto<PingRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	public static void ResetToPool(PingRequest instance)
	{
		if (instance.ShouldPool)
		{
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
			throw new Exception("Trying to dispose PingRequest with ShouldPool set to false!");
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

	public void CopyTo(PingRequest instance)
	{
	}

	public PingRequest Copy()
	{
		PingRequest pingRequest = Pool.Get<PingRequest>();
		CopyTo(pingRequest);
		return pingRequest;
	}

	public static PingRequest Deserialize(BufferStream stream)
	{
		PingRequest pingRequest = Pool.Get<PingRequest>();
		Deserialize(stream, pingRequest, isDelta: false);
		return pingRequest;
	}

	public static PingRequest DeserializeLengthDelimited(BufferStream stream)
	{
		PingRequest pingRequest = Pool.Get<PingRequest>();
		DeserializeLengthDelimited(stream, pingRequest, isDelta: false);
		return pingRequest;
	}

	public static PingRequest DeserializeLength(BufferStream stream, int length)
	{
		PingRequest pingRequest = Pool.Get<PingRequest>();
		DeserializeLength(stream, length, pingRequest, isDelta: false);
		return pingRequest;
	}

	public static PingRequest Deserialize(byte[] buffer)
	{
		PingRequest pingRequest = Pool.Get<PingRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, pingRequest, isDelta: false);
		return pingRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PingRequest previous)
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

	public static PingRequest Deserialize(BufferStream stream, PingRequest instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PingRequest");
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PingRequest");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static PingRequest DeserializeLengthDelimited(BufferStream stream, PingRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PingRequest");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PingRequest");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static PingRequest DeserializeLength(BufferStream stream, int length, PingRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.PingRequest");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.PingRequest");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PingRequest instance, PingRequest previous)
	{
	}

	public static void Serialize(BufferStream stream, PingRequest instance)
	{
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
