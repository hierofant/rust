using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class ClanChatBatchRequest : IDisposable, Pool.IPooled, IProto<ClanChatBatchRequest>, IProto
{
	public class Message : IDisposable, Pool.IPooled, IProto<Message>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public long clanId;

		[NonSerialized]
		public ulong userId;

		[NonSerialized]
		public string text;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public long timestamp;

		public static void ResetToPool(Message instance)
		{
			if (instance.ShouldPool)
			{
				instance.clanId = 0L;
				instance.userId = 0uL;
				instance.text = string.Empty;
				instance.name = string.Empty;
				instance.timestamp = 0L;
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
				throw new Exception("Trying to dispose Message with ShouldPool set to false!");
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

		public void CopyTo(Message instance)
		{
			instance.clanId = clanId;
			instance.userId = userId;
			instance.text = text;
			instance.name = name;
			instance.timestamp = timestamp;
		}

		public Message Copy()
		{
			Message message = Pool.Get<Message>();
			CopyTo(message);
			return message;
		}

		public static Message Deserialize(BufferStream stream)
		{
			Message message = Pool.Get<Message>();
			Deserialize(stream, message, isDelta: false);
			return message;
		}

		public static Message DeserializeLengthDelimited(BufferStream stream)
		{
			Message message = Pool.Get<Message>();
			DeserializeLengthDelimited(stream, message, isDelta: false);
			return message;
		}

		public static Message DeserializeLength(BufferStream stream, int length)
		{
			Message message = Pool.Get<Message>();
			DeserializeLength(stream, length, message, isDelta: false);
			return message;
		}

		public static Message Deserialize(byte[] buffer)
		{
			Message message = Pool.Get<Message>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, message, isDelta: false);
			return message;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Message previous)
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

		public static Message Deserialize(BufferStream stream, Message instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.Nexus.ClanChatBatchRequest.Message");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.userId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.text = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Message DeserializeLengthDelimited(BufferStream stream, Message instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.Nexus.ClanChatBatchRequest.Message");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.userId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.text = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Message DeserializeLength(BufferStream stream, int length, Message instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.Nexus.ClanChatBatchRequest.Message");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.userId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.text = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest.Message");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Message instance, Message previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
			if (instance.userId != previous.userId)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.userId);
			}
			if (instance.text != previous.text)
			{
				if (instance.text == null)
				{
					throw new ArgumentNullException("text", "Required by proto specification.");
				}
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.text);
			}
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, instance.name);
			}
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		}

		public static void Serialize(BufferStream stream, Message instance)
		{
			if (instance.clanId != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
			}
			if (instance.userId != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.userId);
			}
			if (instance.text == null)
			{
				throw new ArgumentNullException("text", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.text);
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.timestamp != 0L)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<Message> messages;

	public static void ResetToPool(ClanChatBatchRequest instance)
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
			List<Message> obj = instance.messages;
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
			throw new Exception("Trying to dispose ClanChatBatchRequest with ShouldPool set to false!");
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

	public void CopyTo(ClanChatBatchRequest instance)
	{
		if (messages != null)
		{
			instance.messages = Pool.Get<List<Message>>();
			for (int i = 0; i < messages.Count; i++)
			{
				Message item = messages[i].Copy();
				instance.messages.Add(item);
			}
		}
		else
		{
			instance.messages = null;
		}
	}

	public ClanChatBatchRequest Copy()
	{
		ClanChatBatchRequest clanChatBatchRequest = Pool.Get<ClanChatBatchRequest>();
		CopyTo(clanChatBatchRequest);
		return clanChatBatchRequest;
	}

	public static ClanChatBatchRequest Deserialize(BufferStream stream)
	{
		ClanChatBatchRequest clanChatBatchRequest = Pool.Get<ClanChatBatchRequest>();
		Deserialize(stream, clanChatBatchRequest, isDelta: false);
		return clanChatBatchRequest;
	}

	public static ClanChatBatchRequest DeserializeLengthDelimited(BufferStream stream)
	{
		ClanChatBatchRequest clanChatBatchRequest = Pool.Get<ClanChatBatchRequest>();
		DeserializeLengthDelimited(stream, clanChatBatchRequest, isDelta: false);
		return clanChatBatchRequest;
	}

	public static ClanChatBatchRequest DeserializeLength(BufferStream stream, int length)
	{
		ClanChatBatchRequest clanChatBatchRequest = Pool.Get<ClanChatBatchRequest>();
		DeserializeLength(stream, length, clanChatBatchRequest, isDelta: false);
		return clanChatBatchRequest;
	}

	public static ClanChatBatchRequest Deserialize(byte[] buffer)
	{
		ClanChatBatchRequest clanChatBatchRequest = Pool.Get<ClanChatBatchRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanChatBatchRequest, isDelta: false);
		return clanChatBatchRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanChatBatchRequest previous)
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

	public static ClanChatBatchRequest Deserialize(BufferStream stream, ClanChatBatchRequest instance, bool isDelta)
	{
		if (!isDelta && instance.messages == null)
		{
			instance.messages = Pool.Get<List<Message>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.ClanChatBatchRequest");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				stream.ConsumeRepeatedElement();
				instance.messages.Add(Message.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanChatBatchRequest DeserializeLengthDelimited(BufferStream stream, ClanChatBatchRequest instance, bool isDelta)
	{
		if (!isDelta && instance.messages == null)
		{
			instance.messages = Pool.Get<List<Message>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.ClanChatBatchRequest");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				stream.ConsumeRepeatedElement();
				instance.messages.Add(Message.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanChatBatchRequest DeserializeLength(BufferStream stream, int length, ClanChatBatchRequest instance, bool isDelta)
	{
		if (!isDelta && instance.messages == null)
		{
			instance.messages = Pool.Get<List<Message>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.ClanChatBatchRequest");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				stream.ConsumeRepeatedElement();
				instance.messages.Add(Message.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.ClanChatBatchRequest");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanChatBatchRequest instance, ClanChatBatchRequest previous)
	{
		if (instance.messages == null)
		{
			return;
		}
		for (int i = 0; i < instance.messages.Count; i++)
		{
			Message message = instance.messages[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Message.SerializeDelta(stream, message, message);
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

	public static void Serialize(BufferStream stream, ClanChatBatchRequest instance)
	{
		if (instance.messages == null)
		{
			return;
		}
		for (int i = 0; i < instance.messages.Count; i++)
		{
			Message instance2 = instance.messages[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Message.Serialize(stream, instance2);
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
