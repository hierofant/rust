using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MemoryBank : IDisposable, Pool.IPooled, IProto<MemoryBank>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int type;

	[NonSerialized]
	public int slotCount;

	[NonSerialized]
	public List<int> slots;

	public static void ResetToPool(MemoryBank instance)
	{
		if (instance.ShouldPool)
		{
			instance.type = 0;
			instance.slotCount = 0;
			if (instance.slots != null)
			{
				List<int> obj = instance.slots;
				Pool.FreeUnmanaged(ref obj);
				instance.slots = obj;
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
			throw new Exception("Trying to dispose MemoryBank with ShouldPool set to false!");
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

	public void CopyTo(MemoryBank instance)
	{
		instance.type = type;
		instance.slotCount = slotCount;
		if (slots != null)
		{
			instance.slots = Pool.Get<List<int>>();
			for (int i = 0; i < slots.Count; i++)
			{
				int item = slots[i];
				instance.slots.Add(item);
			}
		}
		else
		{
			instance.slots = null;
		}
	}

	public MemoryBank Copy()
	{
		MemoryBank memoryBank = Pool.Get<MemoryBank>();
		CopyTo(memoryBank);
		return memoryBank;
	}

	public static MemoryBank Deserialize(BufferStream stream)
	{
		MemoryBank memoryBank = Pool.Get<MemoryBank>();
		Deserialize(stream, memoryBank, isDelta: false);
		return memoryBank;
	}

	public static MemoryBank DeserializeLengthDelimited(BufferStream stream)
	{
		MemoryBank memoryBank = Pool.Get<MemoryBank>();
		DeserializeLengthDelimited(stream, memoryBank, isDelta: false);
		return memoryBank;
	}

	public static MemoryBank DeserializeLength(BufferStream stream, int length)
	{
		MemoryBank memoryBank = Pool.Get<MemoryBank>();
		DeserializeLength(stream, length, memoryBank, isDelta: false);
		return memoryBank;
	}

	public static MemoryBank Deserialize(byte[] buffer)
	{
		MemoryBank memoryBank = Pool.Get<MemoryBank>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, memoryBank, isDelta: false);
		return memoryBank;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MemoryBank previous)
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

	public static MemoryBank Deserialize(BufferStream stream, MemoryBank instance, bool isDelta)
	{
		if (!isDelta && instance.slots == null)
		{
			instance.slots = Pool.Get<List<int>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MemoryBank");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				instance.type = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				instance.slotCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				stream.ConsumeRepeatedElement();
				instance.slots.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MemoryBank DeserializeLengthDelimited(BufferStream stream, MemoryBank instance, bool isDelta)
	{
		if (!isDelta && instance.slots == null)
		{
			instance.slots = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MemoryBank");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				instance.type = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				instance.slotCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				stream.ConsumeRepeatedElement();
				instance.slots.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MemoryBank DeserializeLength(BufferStream stream, int length, MemoryBank instance, bool isDelta)
	{
		if (!isDelta && instance.slots == null)
		{
			instance.slots = Pool.Get<List<int>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MemoryBank");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				instance.type = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				instance.slotCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				stream.ConsumeRepeatedElement();
				instance.slots.Add((int)ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MemoryBank");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MemoryBank instance, MemoryBank previous)
	{
		if (instance.type != previous.type)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
		}
		if (instance.slotCount != previous.slotCount)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.slotCount);
		}
		if (instance.slots != null)
		{
			for (int i = 0; i < instance.slots.Count; i++)
			{
				int num = instance.slots[i];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
			}
		}
	}

	public static void Serialize(BufferStream stream, MemoryBank instance)
	{
		if (instance.type != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
		}
		if (instance.slotCount != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.slotCount);
		}
		if (instance.slots != null)
		{
			for (int i = 0; i < instance.slots.Count; i++)
			{
				int num = instance.slots[i];
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)num);
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
