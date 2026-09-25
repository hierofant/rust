using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppNewTeamMessage : IDisposable, Pool.IPooled, IProto<AppNewTeamMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public AppTeamMessage message;

	public static void ResetToPool(AppNewTeamMessage instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.message != null)
			{
				instance.message.ResetToPool();
				instance.message = null;
			}
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
			throw new Exception("Trying to dispose AppNewTeamMessage with ShouldPool set to false!");
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

	public void CopyTo(AppNewTeamMessage instance)
	{
		if (message != null)
		{
			if (instance.message == null)
			{
				instance.message = message.Copy();
			}
			else
			{
				message.CopyTo(instance.message);
			}
		}
		else
		{
			instance.message = null;
		}
	}

	public AppNewTeamMessage Copy()
	{
		AppNewTeamMessage appNewTeamMessage = Pool.Get<AppNewTeamMessage>();
		CopyTo(appNewTeamMessage);
		return appNewTeamMessage;
	}

	public static AppNewTeamMessage Deserialize(BufferStream stream)
	{
		AppNewTeamMessage appNewTeamMessage = Pool.Get<AppNewTeamMessage>();
		Deserialize(stream, appNewTeamMessage, isDelta: false);
		return appNewTeamMessage;
	}

	public static AppNewTeamMessage DeserializeLengthDelimited(BufferStream stream)
	{
		AppNewTeamMessage appNewTeamMessage = Pool.Get<AppNewTeamMessage>();
		DeserializeLengthDelimited(stream, appNewTeamMessage, isDelta: false);
		return appNewTeamMessage;
	}

	public static AppNewTeamMessage DeserializeLength(BufferStream stream, int length)
	{
		AppNewTeamMessage appNewTeamMessage = Pool.Get<AppNewTeamMessage>();
		DeserializeLength(stream, length, appNewTeamMessage, isDelta: false);
		return appNewTeamMessage;
	}

	public static AppNewTeamMessage Deserialize(byte[] buffer)
	{
		AppNewTeamMessage appNewTeamMessage = Pool.Get<AppNewTeamMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appNewTeamMessage, isDelta: false);
		return appNewTeamMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppNewTeamMessage previous)
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

	public static AppNewTeamMessage Deserialize(BufferStream stream, AppNewTeamMessage instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppNewTeamMessage");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewTeamMessage");
				if (instance.message == null)
				{
					instance.message = AppTeamMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamMessage.DeserializeLengthDelimited(stream, instance.message, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewTeamMessage");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNewTeamMessage");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppNewTeamMessage DeserializeLengthDelimited(BufferStream stream, AppNewTeamMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppNewTeamMessage");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewTeamMessage");
				if (instance.message == null)
				{
					instance.message = AppTeamMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamMessage.DeserializeLengthDelimited(stream, instance.message, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewTeamMessage");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNewTeamMessage");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppNewTeamMessage DeserializeLength(BufferStream stream, int length, AppNewTeamMessage instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppNewTeamMessage");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewTeamMessage");
				if (instance.message == null)
				{
					instance.message = AppTeamMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppTeamMessage.DeserializeLengthDelimited(stream, instance.message, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewTeamMessage");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNewTeamMessage");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppNewTeamMessage instance, AppNewTeamMessage previous)
	{
		if (instance.message == null)
		{
			throw new ArgumentNullException("message", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		AppTeamMessage.SerializeDelta(stream, instance.message, previous.message);
		int val = stream.Position - position;
		Span<byte> span = range.GetSpan();
		int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
		if (num < 5)
		{
			span[num - 1] |= 128;
			while (num < 4)
			{
				span[num++] = 128;
			}
			span[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, AppNewTeamMessage instance)
	{
		if (instance.message == null)
		{
			throw new ArgumentNullException("message", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		AppTeamMessage.Serialize(stream, instance.message);
		int val = stream.Position - position;
		Span<byte> span = range.GetSpan();
		int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
		if (num < 5)
		{
			span[num - 1] |= 128;
			while (num < 4)
			{
				span[num++] = 128;
			}
			span[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		message?.InspectUids(action);
	}
}
