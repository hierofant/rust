using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class FerryStatusRequest : IDisposable, Pool.IPooled, IProto<FerryStatusRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	public static void ResetToPool(FerryStatusRequest instance)
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
			throw new Exception("Trying to dispose FerryStatusRequest with ShouldPool set to false!");
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

	public void CopyTo(FerryStatusRequest instance)
	{
	}

	public FerryStatusRequest Copy()
	{
		FerryStatusRequest ferryStatusRequest = Pool.Get<FerryStatusRequest>();
		CopyTo(ferryStatusRequest);
		return ferryStatusRequest;
	}

	public static FerryStatusRequest Deserialize(BufferStream stream)
	{
		FerryStatusRequest ferryStatusRequest = Pool.Get<FerryStatusRequest>();
		Deserialize(stream, ferryStatusRequest, isDelta: false);
		return ferryStatusRequest;
	}

	public static FerryStatusRequest DeserializeLengthDelimited(BufferStream stream)
	{
		FerryStatusRequest ferryStatusRequest = Pool.Get<FerryStatusRequest>();
		DeserializeLengthDelimited(stream, ferryStatusRequest, isDelta: false);
		return ferryStatusRequest;
	}

	public static FerryStatusRequest DeserializeLength(BufferStream stream, int length)
	{
		FerryStatusRequest ferryStatusRequest = Pool.Get<FerryStatusRequest>();
		DeserializeLength(stream, length, ferryStatusRequest, isDelta: false);
		return ferryStatusRequest;
	}

	public static FerryStatusRequest Deserialize(byte[] buffer)
	{
		FerryStatusRequest ferryStatusRequest = Pool.Get<FerryStatusRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ferryStatusRequest, isDelta: false);
		return ferryStatusRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, FerryStatusRequest previous)
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

	public static FerryStatusRequest Deserialize(BufferStream stream, FerryStatusRequest instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatusRequest");
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusRequest");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static FerryStatusRequest DeserializeLengthDelimited(BufferStream stream, FerryStatusRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatusRequest");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusRequest");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static FerryStatusRequest DeserializeLength(BufferStream stream, int length, FerryStatusRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatusRequest");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatusRequest");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, FerryStatusRequest instance, FerryStatusRequest previous)
	{
	}

	public static void Serialize(BufferStream stream, FerryStatusRequest instance)
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
