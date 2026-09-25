using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ConversationResponseStatesList : IDisposable, Pool.IPooled, IProto<ConversationResponseStatesList>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<bool> list;

	public static void ResetToPool(ConversationResponseStatesList instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.list != null)
			{
				List<bool> obj = instance.list;
				Pool.FreeUnmanaged(ref obj);
				instance.list = obj;
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
			throw new Exception("Trying to dispose ConversationResponseStatesList with ShouldPool set to false!");
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

	public void CopyTo(ConversationResponseStatesList instance)
	{
		if (list != null)
		{
			instance.list = Pool.Get<List<bool>>();
			for (int i = 0; i < list.Count; i++)
			{
				bool item = list[i];
				instance.list.Add(item);
			}
		}
		else
		{
			instance.list = null;
		}
	}

	public ConversationResponseStatesList Copy()
	{
		ConversationResponseStatesList conversationResponseStatesList = Pool.Get<ConversationResponseStatesList>();
		CopyTo(conversationResponseStatesList);
		return conversationResponseStatesList;
	}

	public static ConversationResponseStatesList Deserialize(BufferStream stream)
	{
		ConversationResponseStatesList conversationResponseStatesList = Pool.Get<ConversationResponseStatesList>();
		Deserialize(stream, conversationResponseStatesList, isDelta: false);
		return conversationResponseStatesList;
	}

	public static ConversationResponseStatesList DeserializeLengthDelimited(BufferStream stream)
	{
		ConversationResponseStatesList conversationResponseStatesList = Pool.Get<ConversationResponseStatesList>();
		DeserializeLengthDelimited(stream, conversationResponseStatesList, isDelta: false);
		return conversationResponseStatesList;
	}

	public static ConversationResponseStatesList DeserializeLength(BufferStream stream, int length)
	{
		ConversationResponseStatesList conversationResponseStatesList = Pool.Get<ConversationResponseStatesList>();
		DeserializeLength(stream, length, conversationResponseStatesList, isDelta: false);
		return conversationResponseStatesList;
	}

	public static ConversationResponseStatesList Deserialize(byte[] buffer)
	{
		ConversationResponseStatesList conversationResponseStatesList = Pool.Get<ConversationResponseStatesList>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, conversationResponseStatesList, isDelta: false);
		return conversationResponseStatesList;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ConversationResponseStatesList previous)
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

	public static ConversationResponseStatesList Deserialize(BufferStream stream, ConversationResponseStatesList instance, bool isDelta)
	{
		if (!isDelta && instance.list == null)
		{
			instance.list = Pool.Get<List<bool>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ConversationResponseStatesList");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				stream.ConsumeRepeatedElement();
				instance.list.Add(ProtocolParser.ReadBool(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ConversationResponseStatesList DeserializeLengthDelimited(BufferStream stream, ConversationResponseStatesList instance, bool isDelta)
	{
		if (!isDelta && instance.list == null)
		{
			instance.list = Pool.Get<List<bool>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ConversationResponseStatesList");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				stream.ConsumeRepeatedElement();
				instance.list.Add(ProtocolParser.ReadBool(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ConversationResponseStatesList DeserializeLength(BufferStream stream, int length, ConversationResponseStatesList instance, bool isDelta)
	{
		if (!isDelta && instance.list == null)
		{
			instance.list = Pool.Get<List<bool>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ConversationResponseStatesList");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				stream.ConsumeRepeatedElement();
				instance.list.Add(ProtocolParser.ReadBool(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ConversationResponseStatesList");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ConversationResponseStatesList instance, ConversationResponseStatesList previous)
	{
		if (instance.list != null)
		{
			for (int i = 0; i < instance.list.Count; i++)
			{
				bool val = instance.list[i];
				stream.WriteByte(8);
				ProtocolParser.WriteBool(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, ConversationResponseStatesList instance)
	{
		if (instance.list != null)
		{
			for (int i = 0; i < instance.list.Count; i++)
			{
				bool val = instance.list[i];
				stream.WriteByte(8);
				ProtocolParser.WriteBool(stream, val);
			}
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
