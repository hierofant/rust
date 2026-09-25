using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class UpdateItemContainer : IDisposable, Pool.IPooled, IProto<UpdateItemContainer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int type;

	[NonSerialized]
	public List<ItemContainer> container;

	public static void ResetToPool(UpdateItemContainer instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.type = 0;
		if (instance.container != null)
		{
			for (int i = 0; i < instance.container.Count; i++)
			{
				if (instance.container[i] != null)
				{
					instance.container[i].ResetToPool();
					instance.container[i] = null;
				}
			}
			List<ItemContainer> obj = instance.container;
			Pool.Free(ref obj, freeElements: false);
			instance.container = obj;
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
			throw new Exception("Trying to dispose UpdateItemContainer with ShouldPool set to false!");
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

	public void CopyTo(UpdateItemContainer instance)
	{
		instance.type = type;
		if (container != null)
		{
			instance.container = Pool.Get<List<ItemContainer>>();
			for (int i = 0; i < container.Count; i++)
			{
				ItemContainer item = container[i].Copy();
				instance.container.Add(item);
			}
		}
		else
		{
			instance.container = null;
		}
	}

	public UpdateItemContainer Copy()
	{
		UpdateItemContainer updateItemContainer = Pool.Get<UpdateItemContainer>();
		CopyTo(updateItemContainer);
		return updateItemContainer;
	}

	public static UpdateItemContainer Deserialize(BufferStream stream)
	{
		UpdateItemContainer updateItemContainer = Pool.Get<UpdateItemContainer>();
		Deserialize(stream, updateItemContainer, isDelta: false);
		return updateItemContainer;
	}

	public static UpdateItemContainer DeserializeLengthDelimited(BufferStream stream)
	{
		UpdateItemContainer updateItemContainer = Pool.Get<UpdateItemContainer>();
		DeserializeLengthDelimited(stream, updateItemContainer, isDelta: false);
		return updateItemContainer;
	}

	public static UpdateItemContainer DeserializeLength(BufferStream stream, int length)
	{
		UpdateItemContainer updateItemContainer = Pool.Get<UpdateItemContainer>();
		DeserializeLength(stream, length, updateItemContainer, isDelta: false);
		return updateItemContainer;
	}

	public static UpdateItemContainer Deserialize(byte[] buffer)
	{
		UpdateItemContainer updateItemContainer = Pool.Get<UpdateItemContainer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, updateItemContainer, isDelta: false);
		return updateItemContainer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, UpdateItemContainer previous)
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

	public static UpdateItemContainer Deserialize(BufferStream stream, UpdateItemContainer instance, bool isDelta)
	{
		if (!isDelta && instance.container == null)
		{
			instance.container = Pool.Get<List<ItemContainer>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.UpdateItemContainer");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItemContainer");
				instance.type = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				stream.ConsumeRepeatedElement();
				instance.container.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static UpdateItemContainer DeserializeLengthDelimited(BufferStream stream, UpdateItemContainer instance, bool isDelta)
	{
		if (!isDelta && instance.container == null)
		{
			instance.container = Pool.Get<List<ItemContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.UpdateItemContainer");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItemContainer");
				instance.type = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				stream.ConsumeRepeatedElement();
				instance.container.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static UpdateItemContainer DeserializeLength(BufferStream stream, int length, UpdateItemContainer instance, bool isDelta)
	{
		if (!isDelta && instance.container == null)
		{
			instance.container = Pool.Get<List<ItemContainer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.UpdateItemContainer");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItemContainer");
				instance.type = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				stream.ConsumeRepeatedElement();
				instance.container.Add(ItemContainer.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.UpdateItemContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, UpdateItemContainer instance, UpdateItemContainer previous)
	{
		if (instance.type != previous.type)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
		}
		if (instance.container == null)
		{
			return;
		}
		for (int i = 0; i < instance.container.Count; i++)
		{
			ItemContainer itemContainer = instance.container[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.SerializeDelta(stream, itemContainer, itemContainer);
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

	public static void Serialize(BufferStream stream, UpdateItemContainer instance)
	{
		if (instance.type != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
		}
		if (instance.container == null)
		{
			return;
		}
		for (int i = 0; i < instance.container.Count; i++)
		{
			ItemContainer instance2 = instance.container[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.Serialize(stream, instance2);
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
		if (container != null)
		{
			for (int i = 0; i < container.Count; i++)
			{
				container[i]?.InspectUids(action);
			}
		}
	}
}
