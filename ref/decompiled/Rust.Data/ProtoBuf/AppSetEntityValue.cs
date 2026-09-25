using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppSetEntityValue : IDisposable, Pool.IPooled, IProto<AppSetEntityValue>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool value;

	public static void ResetToPool(AppSetEntityValue instance)
	{
		if (instance.ShouldPool)
		{
			instance.value = false;
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
			throw new Exception("Trying to dispose AppSetEntityValue with ShouldPool set to false!");
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

	public void CopyTo(AppSetEntityValue instance)
	{
		instance.value = value;
	}

	public AppSetEntityValue Copy()
	{
		AppSetEntityValue appSetEntityValue = Pool.Get<AppSetEntityValue>();
		CopyTo(appSetEntityValue);
		return appSetEntityValue;
	}

	public static AppSetEntityValue Deserialize(BufferStream stream)
	{
		AppSetEntityValue appSetEntityValue = Pool.Get<AppSetEntityValue>();
		Deserialize(stream, appSetEntityValue, isDelta: false);
		return appSetEntityValue;
	}

	public static AppSetEntityValue DeserializeLengthDelimited(BufferStream stream)
	{
		AppSetEntityValue appSetEntityValue = Pool.Get<AppSetEntityValue>();
		DeserializeLengthDelimited(stream, appSetEntityValue, isDelta: false);
		return appSetEntityValue;
	}

	public static AppSetEntityValue DeserializeLength(BufferStream stream, int length)
	{
		AppSetEntityValue appSetEntityValue = Pool.Get<AppSetEntityValue>();
		DeserializeLength(stream, length, appSetEntityValue, isDelta: false);
		return appSetEntityValue;
	}

	public static AppSetEntityValue Deserialize(byte[] buffer)
	{
		AppSetEntityValue appSetEntityValue = Pool.Get<AppSetEntityValue>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appSetEntityValue, isDelta: false);
		return appSetEntityValue;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppSetEntityValue previous)
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

	public static AppSetEntityValue Deserialize(BufferStream stream, AppSetEntityValue instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.value = false;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppSetEntityValue");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSetEntityValue");
				instance.value = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSetEntityValue");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSetEntityValue");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppSetEntityValue DeserializeLengthDelimited(BufferStream stream, AppSetEntityValue instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.value = false;
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.AppSetEntityValue");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSetEntityValue");
				instance.value = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSetEntityValue");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSetEntityValue");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppSetEntityValue DeserializeLength(BufferStream stream, int length, AppSetEntityValue instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.value = false;
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.AppSetEntityValue");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSetEntityValue");
				instance.value = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSetEntityValue");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSetEntityValue");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppSetEntityValue instance, AppSetEntityValue previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.value);
	}

	public static void Serialize(BufferStream stream, AppSetEntityValue instance)
	{
		if (instance.value)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.value);
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
