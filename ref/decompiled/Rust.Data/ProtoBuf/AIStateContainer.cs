using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AIStateContainer : IDisposable, Pool.IPooled, IProto<AIStateContainer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int id;

	[NonSerialized]
	public int state;

	[NonSerialized]
	public List<AIEventData> events;

	[NonSerialized]
	public int inputMemorySlot;

	public static void ResetToPool(AIStateContainer instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.id = 0;
		instance.state = 0;
		if (instance.events != null)
		{
			for (int i = 0; i < instance.events.Count; i++)
			{
				if (instance.events[i] != null)
				{
					instance.events[i].ResetToPool();
					instance.events[i] = null;
				}
			}
			List<AIEventData> obj = instance.events;
			Pool.Free(ref obj, freeElements: false);
			instance.events = obj;
		}
		instance.inputMemorySlot = 0;
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
			throw new Exception("Trying to dispose AIStateContainer with ShouldPool set to false!");
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

	public void CopyTo(AIStateContainer instance)
	{
		instance.id = id;
		instance.state = state;
		if (events != null)
		{
			instance.events = Pool.Get<List<AIEventData>>();
			for (int i = 0; i < events.Count; i++)
			{
				AIEventData item = events[i].Copy();
				instance.events.Add(item);
			}
		}
		else
		{
			instance.events = null;
		}
		instance.inputMemorySlot = inputMemorySlot;
	}

	public AIStateContainer Copy()
	{
		AIStateContainer aIStateContainer = Pool.Get<AIStateContainer>();
		CopyTo(aIStateContainer);
		return aIStateContainer;
	}

	public static AIStateContainer Deserialize(BufferStream stream)
	{
		AIStateContainer aIStateContainer = Pool.Get<AIStateContainer>();
		Deserialize(stream, aIStateContainer, isDelta: false);
		return aIStateContainer;
	}

	public static AIStateContainer DeserializeLengthDelimited(BufferStream stream)
	{
		AIStateContainer aIStateContainer = Pool.Get<AIStateContainer>();
		DeserializeLengthDelimited(stream, aIStateContainer, isDelta: false);
		return aIStateContainer;
	}

	public static AIStateContainer DeserializeLength(BufferStream stream, int length)
	{
		AIStateContainer aIStateContainer = Pool.Get<AIStateContainer>();
		DeserializeLength(stream, length, aIStateContainer, isDelta: false);
		return aIStateContainer;
	}

	public static AIStateContainer Deserialize(byte[] buffer)
	{
		AIStateContainer aIStateContainer = Pool.Get<AIStateContainer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, aIStateContainer, isDelta: false);
		return aIStateContainer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AIStateContainer previous)
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

	public static AIStateContainer Deserialize(BufferStream stream, AIStateContainer instance, bool isDelta)
	{
		if (!isDelta && instance.events == null)
		{
			instance.events = Pool.Get<List<AIEventData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AIStateContainer");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				stream.ConsumeRepeatedElement();
				instance.events.Add(AIEventData.DeserializeLengthDelimited(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.inputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AIStateContainer DeserializeLengthDelimited(BufferStream stream, AIStateContainer instance, bool isDelta)
	{
		if (!isDelta && instance.events == null)
		{
			instance.events = Pool.Get<List<AIEventData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AIStateContainer");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				stream.ConsumeRepeatedElement();
				instance.events.Add(AIEventData.DeserializeLengthDelimited(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.inputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AIStateContainer DeserializeLength(BufferStream stream, int length, AIStateContainer instance, bool isDelta)
	{
		if (!isDelta && instance.events == null)
		{
			instance.events = Pool.Get<List<AIEventData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AIStateContainer");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.id = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				stream.ConsumeRepeatedElement();
				instance.events.Add(AIEventData.DeserializeLengthDelimited(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				instance.inputMemorySlot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AIStateContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AIStateContainer instance, AIStateContainer previous)
	{
		if (instance.id != previous.id)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.id);
		}
		if (instance.state != previous.state)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.events != null)
		{
			for (int i = 0; i < instance.events.Count; i++)
			{
				AIEventData aIEventData = instance.events[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range = stream.GetRange(2);
				int position = stream.Position;
				AIEventData.SerializeDelta(stream, aIEventData, aIEventData);
				int num = stream.Position - position;
				if (num > 16383)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field events (ProtoBuf.AIEventData)");
				}
				Span<byte> span = range.GetSpan();
				if (ProtocolParser.WriteUInt32((uint)num, span, 0) < 2)
				{
					span[0] |= 128;
					span[1] = 0;
				}
			}
		}
		if (instance.inputMemorySlot != previous.inputMemorySlot)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.inputMemorySlot);
		}
	}

	public static void Serialize(BufferStream stream, AIStateContainer instance)
	{
		if (instance.id != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.id);
		}
		if (instance.state != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.events != null)
		{
			for (int i = 0; i < instance.events.Count; i++)
			{
				AIEventData instance2 = instance.events[i];
				stream.WriteByte(26);
				BufferStream.RangeHandle range = stream.GetRange(2);
				int position = stream.Position;
				AIEventData.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 16383)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field events (ProtoBuf.AIEventData)");
				}
				Span<byte> span = range.GetSpan();
				if (ProtocolParser.WriteUInt32((uint)num, span, 0) < 2)
				{
					span[0] |= 128;
					span[1] = 0;
				}
			}
		}
		if (instance.inputMemorySlot != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.inputMemorySlot);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (events != null)
		{
			for (int i = 0; i < events.Count; i++)
			{
				events[i]?.InspectUids(action);
			}
		}
	}
}
