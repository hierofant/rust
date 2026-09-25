using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppClanChat : IDisposable, Pool.IPooled, IProto<AppClanChat>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<AppClanMessage> messages;

	public static void ResetToPool(AppClanChat instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.messages != null)
		{
			for (int i = 0; i < instance.messages.Count; i++)
			{
				if (instance.messages[i] != null)
				{
					instance.messages[i].ResetToPool();
					instance.messages[i] = null;
				}
			}
			List<AppClanMessage> obj = instance.messages;
			Pool.Free(ref obj, freeElements: false);
			instance.messages = obj;
		}
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose AppClanChat with ShouldPool set to false!");
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

	public void CopyTo(AppClanChat instance)
	{
		if (messages != null)
		{
			instance.messages = Pool.Get<List<AppClanMessage>>();
			for (int i = 0; i < messages.Count; i++)
			{
				AppClanMessage item = messages[i].Copy();
				instance.messages.Add(item);
			}
		}
		else
		{
			instance.messages = null;
		}
	}

	public AppClanChat Copy()
	{
		AppClanChat appClanChat = Pool.Get<AppClanChat>();
		CopyTo(appClanChat);
		return appClanChat;
	}

	public static AppClanChat Deserialize(BufferStream stream)
	{
		AppClanChat appClanChat = Pool.Get<AppClanChat>();
		Deserialize(stream, appClanChat, isDelta: false);
		return appClanChat;
	}

	public static AppClanChat DeserializeLengthDelimited(BufferStream stream)
	{
		AppClanChat appClanChat = Pool.Get<AppClanChat>();
		DeserializeLengthDelimited(stream, appClanChat, isDelta: false);
		return appClanChat;
	}

	public static AppClanChat DeserializeLength(BufferStream stream, int length)
	{
		AppClanChat appClanChat = Pool.Get<AppClanChat>();
		DeserializeLength(stream, length, appClanChat, isDelta: false);
		return appClanChat;
	}

	public static AppClanChat Deserialize(byte[] buffer)
	{
		AppClanChat appClanChat = Pool.Get<AppClanChat>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appClanChat, isDelta: false);
		return appClanChat;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppClanChat previous)
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

	public static AppClanChat Deserialize(BufferStream stream, AppClanChat instance, bool isDelta)
	{
		if (!isDelta && instance.messages == null)
		{
			instance.messages = Pool.Get<List<AppClanMessage>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppClanChat");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				stream.ConsumeRepeatedElement();
				instance.messages.Add(AppClanMessage.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppClanChat DeserializeLengthDelimited(BufferStream stream, AppClanChat instance, bool isDelta)
	{
		if (!isDelta && instance.messages == null)
		{
			instance.messages = Pool.Get<List<AppClanMessage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppClanChat");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				stream.ConsumeRepeatedElement();
				instance.messages.Add(AppClanMessage.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppClanChat DeserializeLength(BufferStream stream, int length, AppClanChat instance, bool isDelta)
	{
		if (!isDelta && instance.messages == null)
		{
			instance.messages = Pool.Get<List<AppClanMessage>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppClanChat");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				stream.ConsumeRepeatedElement();
				instance.messages.Add(AppClanMessage.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppClanChat");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppClanChat instance, AppClanChat previous)
	{
		if (instance.messages == null)
		{
			return;
		}
		for (int i = 0; i < instance.messages.Count; i++)
		{
			AppClanMessage appClanMessage = instance.messages[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppClanMessage.SerializeDelta(stream, appClanMessage, appClanMessage);
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
	}

	public static void Serialize(BufferStream stream, AppClanChat instance)
	{
		if (instance.messages == null)
		{
			return;
		}
		for (int i = 0; i < instance.messages.Count; i++)
		{
			AppClanMessage instance2 = instance.messages[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			AppClanMessage.Serialize(stream, instance2);
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
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (messages != null)
		{
			for (int i = 0; i < messages.Count; i++)
			{
				messages[i]?.InspectUids(action);
			}
		}
	}
}
