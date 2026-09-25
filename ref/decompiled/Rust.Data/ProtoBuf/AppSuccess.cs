using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppSuccess : IDisposable, Pool.IPooled, IProto<AppSuccess>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	public static void ResetToPool(AppSuccess instance)
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
			throw new Exception("Trying to dispose AppSuccess with ShouldPool set to false!");
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

	public void CopyTo(AppSuccess instance)
	{
	}

	public AppSuccess Copy()
	{
		AppSuccess appSuccess = Pool.Get<AppSuccess>();
		CopyTo(appSuccess);
		return appSuccess;
	}

	public static AppSuccess Deserialize(BufferStream stream)
	{
		AppSuccess appSuccess = Pool.Get<AppSuccess>();
		Deserialize(stream, appSuccess, isDelta: false);
		return appSuccess;
	}

	public static AppSuccess DeserializeLengthDelimited(BufferStream stream)
	{
		AppSuccess appSuccess = Pool.Get<AppSuccess>();
		DeserializeLengthDelimited(stream, appSuccess, isDelta: false);
		return appSuccess;
	}

	public static AppSuccess DeserializeLength(BufferStream stream, int length)
	{
		AppSuccess appSuccess = Pool.Get<AppSuccess>();
		DeserializeLength(stream, length, appSuccess, isDelta: false);
		return appSuccess;
	}

	public static AppSuccess Deserialize(byte[] buffer)
	{
		AppSuccess appSuccess = Pool.Get<AppSuccess>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appSuccess, isDelta: false);
		return appSuccess;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppSuccess previous)
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

	public static AppSuccess Deserialize(BufferStream stream, AppSuccess instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppSuccess");
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSuccess");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static AppSuccess DeserializeLengthDelimited(BufferStream stream, AppSuccess instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppSuccess");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSuccess");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static AppSuccess DeserializeLength(BufferStream stream, int length, AppSuccess instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppSuccess");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSuccess");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppSuccess instance, AppSuccess previous)
	{
	}

	public static void Serialize(BufferStream stream, AppSuccess instance)
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
