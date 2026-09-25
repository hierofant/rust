using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ResourceExtractor : IDisposable, Pool.IPooled, IProto<ResourceExtractor>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ItemContainer fuelContents;

	[NonSerialized]
	public ItemContainer outputContents;

	public static void ResetToPool(ResourceExtractor instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.fuelContents != null)
			{
				instance.fuelContents.ResetToPool();
				instance.fuelContents = null;
			}
			if (instance.outputContents != null)
			{
				instance.outputContents.ResetToPool();
				instance.outputContents = null;
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
			throw new Exception("Trying to dispose ResourceExtractor with ShouldPool set to false!");
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

	public void CopyTo(ResourceExtractor instance)
	{
		if (fuelContents != null)
		{
			if (instance.fuelContents == null)
			{
				instance.fuelContents = fuelContents.Copy();
			}
			else
			{
				fuelContents.CopyTo(instance.fuelContents);
			}
		}
		else
		{
			instance.fuelContents = null;
		}
		if (outputContents != null)
		{
			if (instance.outputContents == null)
			{
				instance.outputContents = outputContents.Copy();
			}
			else
			{
				outputContents.CopyTo(instance.outputContents);
			}
		}
		else
		{
			instance.outputContents = null;
		}
	}

	public ResourceExtractor Copy()
	{
		ResourceExtractor resourceExtractor = Pool.Get<ResourceExtractor>();
		CopyTo(resourceExtractor);
		return resourceExtractor;
	}

	public static ResourceExtractor Deserialize(BufferStream stream)
	{
		ResourceExtractor resourceExtractor = Pool.Get<ResourceExtractor>();
		Deserialize(stream, resourceExtractor, isDelta: false);
		return resourceExtractor;
	}

	public static ResourceExtractor DeserializeLengthDelimited(BufferStream stream)
	{
		ResourceExtractor resourceExtractor = Pool.Get<ResourceExtractor>();
		DeserializeLengthDelimited(stream, resourceExtractor, isDelta: false);
		return resourceExtractor;
	}

	public static ResourceExtractor DeserializeLength(BufferStream stream, int length)
	{
		ResourceExtractor resourceExtractor = Pool.Get<ResourceExtractor>();
		DeserializeLength(stream, length, resourceExtractor, isDelta: false);
		return resourceExtractor;
	}

	public static ResourceExtractor Deserialize(byte[] buffer)
	{
		ResourceExtractor resourceExtractor = Pool.Get<ResourceExtractor>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, resourceExtractor, isDelta: false);
		return resourceExtractor;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ResourceExtractor previous)
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

	public static ResourceExtractor Deserialize(BufferStream stream, ResourceExtractor instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ResourceExtractor");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				if (instance.fuelContents == null)
				{
					instance.fuelContents = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.fuelContents, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				if (instance.outputContents == null)
				{
					instance.outputContents = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.outputContents, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ResourceExtractor DeserializeLengthDelimited(BufferStream stream, ResourceExtractor instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ResourceExtractor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				if (instance.fuelContents == null)
				{
					instance.fuelContents = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.fuelContents, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				if (instance.outputContents == null)
				{
					instance.outputContents = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.outputContents, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ResourceExtractor DeserializeLength(BufferStream stream, int length, ResourceExtractor instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ResourceExtractor");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				if (instance.fuelContents == null)
				{
					instance.fuelContents = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.fuelContents, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				if (instance.outputContents == null)
				{
					instance.outputContents = ItemContainer.DeserializeLengthDelimited(stream);
				}
				else
				{
					ItemContainer.DeserializeLengthDelimited(stream, instance.outputContents, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ResourceExtractor");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ResourceExtractor instance, ResourceExtractor previous)
	{
		if (instance.fuelContents != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.SerializeDelta(stream, instance.fuelContents, previous.fuelContents);
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
		if (instance.outputContents == null)
		{
			return;
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		ItemContainer.SerializeDelta(stream, instance.outputContents, previous.outputContents);
		int val2 = stream.Position - position2;
		Span<byte> span2 = range2.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
		if (num2 < 5)
		{
			span2[num2 - 1] |= 128;
			while (num2 < 4)
			{
				span2[num2++] = 128;
			}
			span2[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, ResourceExtractor instance)
	{
		if (instance.fuelContents != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ItemContainer.Serialize(stream, instance.fuelContents);
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
		if (instance.outputContents == null)
		{
			return;
		}
		stream.WriteByte(18);
		BufferStream.RangeHandle range2 = stream.GetRange(5);
		int position2 = stream.Position;
		ItemContainer.Serialize(stream, instance.outputContents);
		int val2 = stream.Position - position2;
		Span<byte> span2 = range2.GetSpan();
		int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
		if (num2 < 5)
		{
			span2[num2 - 1] |= 128;
			while (num2 < 4)
			{
				span2[num2++] = 128;
			}
			span2[4] = 0;
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		fuelContents?.InspectUids(action);
		outputContents?.InspectUids(action);
	}
}
