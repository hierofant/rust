using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MiningQuarry : IDisposable, Pool.IPooled, IProto<MiningQuarry>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ResourceExtractor extractor;

	[NonSerialized]
	public int staticType;

	public static void ResetToPool(MiningQuarry instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.extractor != null)
			{
				instance.extractor.ResetToPool();
				instance.extractor = null;
			}
			instance.staticType = 0;
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
			throw new Exception("Trying to dispose MiningQuarry with ShouldPool set to false!");
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

	public void CopyTo(MiningQuarry instance)
	{
		if (extractor != null)
		{
			if (instance.extractor == null)
			{
				instance.extractor = extractor.Copy();
			}
			else
			{
				extractor.CopyTo(instance.extractor);
			}
		}
		else
		{
			instance.extractor = null;
		}
		instance.staticType = staticType;
	}

	public MiningQuarry Copy()
	{
		MiningQuarry miningQuarry = Pool.Get<MiningQuarry>();
		CopyTo(miningQuarry);
		return miningQuarry;
	}

	public static MiningQuarry Deserialize(BufferStream stream)
	{
		MiningQuarry miningQuarry = Pool.Get<MiningQuarry>();
		Deserialize(stream, miningQuarry, isDelta: false);
		return miningQuarry;
	}

	public static MiningQuarry DeserializeLengthDelimited(BufferStream stream)
	{
		MiningQuarry miningQuarry = Pool.Get<MiningQuarry>();
		DeserializeLengthDelimited(stream, miningQuarry, isDelta: false);
		return miningQuarry;
	}

	public static MiningQuarry DeserializeLength(BufferStream stream, int length)
	{
		MiningQuarry miningQuarry = Pool.Get<MiningQuarry>();
		DeserializeLength(stream, length, miningQuarry, isDelta: false);
		return miningQuarry;
	}

	public static MiningQuarry Deserialize(byte[] buffer)
	{
		MiningQuarry miningQuarry = Pool.Get<MiningQuarry>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, miningQuarry, isDelta: false);
		return miningQuarry;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MiningQuarry previous)
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

	public static MiningQuarry Deserialize(BufferStream stream, MiningQuarry instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MiningQuarry");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				if (instance.extractor == null)
				{
					instance.extractor = ResourceExtractor.DeserializeLengthDelimited(stream);
				}
				else
				{
					ResourceExtractor.DeserializeLengthDelimited(stream, instance.extractor, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				instance.staticType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MiningQuarry DeserializeLengthDelimited(BufferStream stream, MiningQuarry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MiningQuarry");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				if (instance.extractor == null)
				{
					instance.extractor = ResourceExtractor.DeserializeLengthDelimited(stream);
				}
				else
				{
					ResourceExtractor.DeserializeLengthDelimited(stream, instance.extractor, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				instance.staticType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MiningQuarry DeserializeLength(BufferStream stream, int length, MiningQuarry instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MiningQuarry");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				if (instance.extractor == null)
				{
					instance.extractor = ResourceExtractor.DeserializeLengthDelimited(stream);
				}
				else
				{
					ResourceExtractor.DeserializeLengthDelimited(stream, instance.extractor, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				instance.staticType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MiningQuarry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MiningQuarry instance, MiningQuarry previous)
	{
		if (instance.extractor != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ResourceExtractor.SerializeDelta(stream, instance.extractor, previous.extractor);
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
		if (instance.staticType != previous.staticType)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.staticType);
		}
	}

	public static void Serialize(BufferStream stream, MiningQuarry instance)
	{
		if (instance.extractor != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			ResourceExtractor.Serialize(stream, instance.extractor);
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
		if (instance.staticType != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.staticType);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		extractor?.InspectUids(action);
	}
}
