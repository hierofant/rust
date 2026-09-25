using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppSendMessage : IDisposable, Pool.IPooled, IProto<AppSendMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string message;

	public static void ResetToPool(AppSendMessage instance)
	{
		if (instance.ShouldPool)
		{
			instance.message = string.Empty;
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
			throw new Exception("Trying to dispose AppSendMessage with ShouldPool set to false!");
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

	public void CopyTo(AppSendMessage instance)
	{
		instance.message = message;
	}

	public AppSendMessage Copy()
	{
		AppSendMessage appSendMessage = Pool.Get<AppSendMessage>();
		CopyTo(appSendMessage);
		return appSendMessage;
	}

	public static AppSendMessage Deserialize(BufferStream stream)
	{
		AppSendMessage appSendMessage = Pool.Get<AppSendMessage>();
		Deserialize(stream, appSendMessage, isDelta: false);
		return appSendMessage;
	}

	public static AppSendMessage DeserializeLengthDelimited(BufferStream stream)
	{
		AppSendMessage appSendMessage = Pool.Get<AppSendMessage>();
		DeserializeLengthDelimited(stream, appSendMessage, isDelta: false);
		return appSendMessage;
	}

	public static AppSendMessage DeserializeLength(BufferStream stream, int length)
	{
		AppSendMessage appSendMessage = Pool.Get<AppSendMessage>();
		DeserializeLength(stream, length, appSendMessage, isDelta: false);
		return appSendMessage;
	}

	public static AppSendMessage Deserialize(byte[] buffer)
	{
		AppSendMessage appSendMessage = Pool.Get<AppSendMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appSendMessage, isDelta: false);
		return appSendMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppSendMessage previous)
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

	public static AppSendMessage Deserialize(BufferStream stream, AppSendMessage instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppSendMessage");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSendMessage");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSendMessage");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSendMessage");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppSendMessage DeserializeLengthDelimited(BufferStream stream, AppSendMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppSendMessage");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSendMessage");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSendMessage");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSendMessage");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppSendMessage DeserializeLength(BufferStream stream, int length, AppSendMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppSendMessage");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSendMessage");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppSendMessage");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppSendMessage");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppSendMessage instance, AppSendMessage previous)
	{
		if (instance.message != previous.message)
		{
			if (instance.message == null)
			{
				throw new ArgumentNullException("message", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.message);
		}
	}

	public static void Serialize(BufferStream stream, AppSendMessage instance)
	{
		if (instance.message == null)
		{
			throw new ArgumentNullException("message", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.message);
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
