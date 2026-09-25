using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BaseBoat : IDisposable, Pool.IPooled, IProto<BaseBoat>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float shoreDriftTimerValue;

	public static void ResetToPool(BaseBoat instance)
	{
		if (instance.ShouldPool)
		{
			instance.shoreDriftTimerValue = 0f;
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
			throw new Exception("Trying to dispose BaseBoat with ShouldPool set to false!");
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

	public void CopyTo(BaseBoat instance)
	{
		instance.shoreDriftTimerValue = shoreDriftTimerValue;
	}

	public BaseBoat Copy()
	{
		BaseBoat baseBoat = Pool.Get<BaseBoat>();
		CopyTo(baseBoat);
		return baseBoat;
	}

	public static BaseBoat Deserialize(BufferStream stream)
	{
		BaseBoat baseBoat = Pool.Get<BaseBoat>();
		Deserialize(stream, baseBoat, isDelta: false);
		return baseBoat;
	}

	public static BaseBoat DeserializeLengthDelimited(BufferStream stream)
	{
		BaseBoat baseBoat = Pool.Get<BaseBoat>();
		DeserializeLengthDelimited(stream, baseBoat, isDelta: false);
		return baseBoat;
	}

	public static BaseBoat DeserializeLength(BufferStream stream, int length)
	{
		BaseBoat baseBoat = Pool.Get<BaseBoat>();
		DeserializeLength(stream, length, baseBoat, isDelta: false);
		return baseBoat;
	}

	public static BaseBoat Deserialize(byte[] buffer)
	{
		BaseBoat baseBoat = Pool.Get<BaseBoat>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, baseBoat, isDelta: false);
		return baseBoat;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BaseBoat previous)
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

	public static BaseBoat Deserialize(BufferStream stream, BaseBoat instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BaseBoat");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseBoat");
				instance.shoreDriftTimerValue = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseBoat");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseBoat");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BaseBoat DeserializeLengthDelimited(BufferStream stream, BaseBoat instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseBoat");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseBoat");
				instance.shoreDriftTimerValue = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseBoat");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseBoat");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BaseBoat DeserializeLength(BufferStream stream, int length, BaseBoat instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseBoat");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseBoat");
				instance.shoreDriftTimerValue = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseBoat");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseBoat");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BaseBoat instance, BaseBoat previous)
	{
		if (instance.shoreDriftTimerValue != previous.shoreDriftTimerValue)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.shoreDriftTimerValue);
		}
	}

	public static void Serialize(BufferStream stream, BaseBoat instance)
	{
		if (instance.shoreDriftTimerValue != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.shoreDriftTimerValue);
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
