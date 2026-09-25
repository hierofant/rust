using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class OilSwitchBroadcast : IDisposable, Pool.IPooled, IProto<OilSwitchBroadcast>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float oilOutputMultiplier;

	public static void ResetToPool(OilSwitchBroadcast instance)
	{
		if (instance.ShouldPool)
		{
			instance.oilOutputMultiplier = 0f;
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
			throw new Exception("Trying to dispose OilSwitchBroadcast with ShouldPool set to false!");
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

	public void CopyTo(OilSwitchBroadcast instance)
	{
		instance.oilOutputMultiplier = oilOutputMultiplier;
	}

	public OilSwitchBroadcast Copy()
	{
		OilSwitchBroadcast oilSwitchBroadcast = Pool.Get<OilSwitchBroadcast>();
		CopyTo(oilSwitchBroadcast);
		return oilSwitchBroadcast;
	}

	public static OilSwitchBroadcast Deserialize(BufferStream stream)
	{
		OilSwitchBroadcast oilSwitchBroadcast = Pool.Get<OilSwitchBroadcast>();
		Deserialize(stream, oilSwitchBroadcast, isDelta: false);
		return oilSwitchBroadcast;
	}

	public static OilSwitchBroadcast DeserializeLengthDelimited(BufferStream stream)
	{
		OilSwitchBroadcast oilSwitchBroadcast = Pool.Get<OilSwitchBroadcast>();
		DeserializeLengthDelimited(stream, oilSwitchBroadcast, isDelta: false);
		return oilSwitchBroadcast;
	}

	public static OilSwitchBroadcast DeserializeLength(BufferStream stream, int length)
	{
		OilSwitchBroadcast oilSwitchBroadcast = Pool.Get<OilSwitchBroadcast>();
		DeserializeLength(stream, length, oilSwitchBroadcast, isDelta: false);
		return oilSwitchBroadcast;
	}

	public static OilSwitchBroadcast Deserialize(byte[] buffer)
	{
		OilSwitchBroadcast oilSwitchBroadcast = Pool.Get<OilSwitchBroadcast>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, oilSwitchBroadcast, isDelta: false);
		return oilSwitchBroadcast;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, OilSwitchBroadcast previous)
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

	public static OilSwitchBroadcast Deserialize(BufferStream stream, OilSwitchBroadcast instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.OilSwitchBroadcast");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OilSwitchBroadcast");
				instance.oilOutputMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OilSwitchBroadcast");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OilSwitchBroadcast");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static OilSwitchBroadcast DeserializeLengthDelimited(BufferStream stream, OilSwitchBroadcast instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.OilSwitchBroadcast");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OilSwitchBroadcast");
				instance.oilOutputMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OilSwitchBroadcast");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OilSwitchBroadcast");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static OilSwitchBroadcast DeserializeLength(BufferStream stream, int length, OilSwitchBroadcast instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.OilSwitchBroadcast");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OilSwitchBroadcast");
				instance.oilOutputMultiplier = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OilSwitchBroadcast");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OilSwitchBroadcast");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, OilSwitchBroadcast instance, OilSwitchBroadcast previous)
	{
		if (instance.oilOutputMultiplier != previous.oilOutputMultiplier)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.oilOutputMultiplier);
		}
	}

	public static void Serialize(BufferStream stream, OilSwitchBroadcast instance)
	{
		if (instance.oilOutputMultiplier != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.oilOutputMultiplier);
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
