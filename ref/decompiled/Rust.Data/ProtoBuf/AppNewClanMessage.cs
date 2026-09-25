using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppNewClanMessage : IDisposable, Pool.IPooled, IProto<AppNewClanMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public long clanId;

	[NonSerialized]
	public AppClanMessage message;

	public static void ResetToPool(AppNewClanMessage instance)
	{
		if (instance.ShouldPool)
		{
			instance.clanId = 0L;
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
			throw new Exception("Trying to dispose AppNewClanMessage with ShouldPool set to false!");
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

	public void CopyTo(AppNewClanMessage instance)
	{
		instance.clanId = clanId;
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

	public AppNewClanMessage Copy()
	{
		AppNewClanMessage appNewClanMessage = Pool.Get<AppNewClanMessage>();
		CopyTo(appNewClanMessage);
		return appNewClanMessage;
	}

	public static AppNewClanMessage Deserialize(BufferStream stream)
	{
		AppNewClanMessage appNewClanMessage = Pool.Get<AppNewClanMessage>();
		Deserialize(stream, appNewClanMessage, isDelta: false);
		return appNewClanMessage;
	}

	public static AppNewClanMessage DeserializeLengthDelimited(BufferStream stream)
	{
		AppNewClanMessage appNewClanMessage = Pool.Get<AppNewClanMessage>();
		DeserializeLengthDelimited(stream, appNewClanMessage, isDelta: false);
		return appNewClanMessage;
	}

	public static AppNewClanMessage DeserializeLength(BufferStream stream, int length)
	{
		AppNewClanMessage appNewClanMessage = Pool.Get<AppNewClanMessage>();
		DeserializeLength(stream, length, appNewClanMessage, isDelta: false);
		return appNewClanMessage;
	}

	public static AppNewClanMessage Deserialize(byte[] buffer)
	{
		AppNewClanMessage appNewClanMessage = Pool.Get<AppNewClanMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appNewClanMessage, isDelta: false);
		return appNewClanMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppNewClanMessage previous)
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

	public static AppNewClanMessage Deserialize(BufferStream stream, AppNewClanMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppNewClanMessage");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				if (instance.message == null)
				{
					instance.message = AppClanMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanMessage.DeserializeLengthDelimited(stream, instance.message, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppNewClanMessage DeserializeLengthDelimited(BufferStream stream, AppNewClanMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppNewClanMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				if (instance.message == null)
				{
					instance.message = AppClanMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanMessage.DeserializeLengthDelimited(stream, instance.message, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppNewClanMessage DeserializeLength(BufferStream stream, int length, AppNewClanMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppNewClanMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				if (instance.message == null)
				{
					instance.message = AppClanMessage.DeserializeLengthDelimited(stream);
				}
				else
				{
					AppClanMessage.DeserializeLengthDelimited(stream, instance.message, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNewClanMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppNewClanMessage instance, AppNewClanMessage previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		if (instance.message == null)
		{
			throw new ArgumentNullException("message", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		AppClanMessage.SerializeDelta(stream, instance.message, previous.message);
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

	public static void Serialize(BufferStream stream, AppNewClanMessage instance)
	{
		if (instance.clanId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		}
		if (instance.message == null)
		{
			throw new ArgumentNullException("message", "Required by proto specification.");
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range = stream.GetRange(5);
		int position = stream.Position;
		AppClanMessage.Serialize(stream, instance.message);
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
