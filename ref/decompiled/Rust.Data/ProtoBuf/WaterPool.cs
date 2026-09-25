using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WaterPool : IDisposable, Pool.IPooled, IProto<WaterPool>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float fillAmount;

	public static void ResetToPool(WaterPool instance)
	{
		if (instance.ShouldPool)
		{
			instance.fillAmount = 0f;
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
			throw new Exception("Trying to dispose WaterPool with ShouldPool set to false!");
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

	public void CopyTo(WaterPool instance)
	{
		instance.fillAmount = fillAmount;
	}

	public WaterPool Copy()
	{
		WaterPool waterPool = Pool.Get<WaterPool>();
		CopyTo(waterPool);
		return waterPool;
	}

	public static WaterPool Deserialize(BufferStream stream)
	{
		WaterPool waterPool = Pool.Get<WaterPool>();
		Deserialize(stream, waterPool, isDelta: false);
		return waterPool;
	}

	public static WaterPool DeserializeLengthDelimited(BufferStream stream)
	{
		WaterPool waterPool = Pool.Get<WaterPool>();
		DeserializeLengthDelimited(stream, waterPool, isDelta: false);
		return waterPool;
	}

	public static WaterPool DeserializeLength(BufferStream stream, int length)
	{
		WaterPool waterPool = Pool.Get<WaterPool>();
		DeserializeLength(stream, length, waterPool, isDelta: false);
		return waterPool;
	}

	public static WaterPool Deserialize(byte[] buffer)
	{
		WaterPool waterPool = Pool.Get<WaterPool>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, waterPool, isDelta: false);
		return waterPool;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WaterPool previous)
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

	public static WaterPool Deserialize(BufferStream stream, WaterPool instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.WaterPool");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WaterPool");
				instance.fillAmount = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WaterPool");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WaterPool");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static WaterPool DeserializeLengthDelimited(BufferStream stream, WaterPool instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WaterPool");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WaterPool");
				instance.fillAmount = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WaterPool");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WaterPool");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static WaterPool DeserializeLength(BufferStream stream, int length, WaterPool instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WaterPool");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WaterPool");
				instance.fillAmount = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WaterPool");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WaterPool");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WaterPool instance, WaterPool previous)
	{
		if (instance.fillAmount != previous.fillAmount)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.fillAmount);
		}
	}

	public static void Serialize(BufferStream stream, WaterPool instance)
	{
		if (instance.fillAmount != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.fillAmount);
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
