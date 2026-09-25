using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppEmpty : IDisposable, Pool.IPooled, IProto<AppEmpty>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	public static void ResetToPool(AppEmpty instance)
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
			throw new Exception("Trying to dispose AppEmpty with ShouldPool set to false!");
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

	public void CopyTo(AppEmpty instance)
	{
	}

	public AppEmpty Copy()
	{
		AppEmpty appEmpty = Pool.Get<AppEmpty>();
		CopyTo(appEmpty);
		return appEmpty;
	}

	public static AppEmpty Deserialize(BufferStream stream)
	{
		AppEmpty appEmpty = Pool.Get<AppEmpty>();
		Deserialize(stream, appEmpty, isDelta: false);
		return appEmpty;
	}

	public static AppEmpty DeserializeLengthDelimited(BufferStream stream)
	{
		AppEmpty appEmpty = Pool.Get<AppEmpty>();
		DeserializeLengthDelimited(stream, appEmpty, isDelta: false);
		return appEmpty;
	}

	public static AppEmpty DeserializeLength(BufferStream stream, int length)
	{
		AppEmpty appEmpty = Pool.Get<AppEmpty>();
		DeserializeLength(stream, length, appEmpty, isDelta: false);
		return appEmpty;
	}

	public static AppEmpty Deserialize(byte[] buffer)
	{
		AppEmpty appEmpty = Pool.Get<AppEmpty>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appEmpty, isDelta: false);
		return appEmpty;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppEmpty previous)
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

	public static AppEmpty Deserialize(BufferStream stream, AppEmpty instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppEmpty");
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEmpty");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static AppEmpty DeserializeLengthDelimited(BufferStream stream, AppEmpty instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppEmpty");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEmpty");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static AppEmpty DeserializeLength(BufferStream stream, int length, AppEmpty instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppEmpty");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			_ = key.Field;
			stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppEmpty");
			ProtocolParser.SkipKey(stream, key);
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppEmpty instance, AppEmpty previous)
	{
	}

	public static void Serialize(BufferStream stream, AppEmpty instance)
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
