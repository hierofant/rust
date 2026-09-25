using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TimedUserAccess : IDisposable, Pool.IPooled, IProto<TimedUserAccess>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong steamid;

	[NonSerialized]
	public float secondsLeft;

	public static void ResetToPool(TimedUserAccess instance)
	{
		if (instance.ShouldPool)
		{
			instance.steamid = 0uL;
			instance.secondsLeft = 0f;
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
			throw new Exception("Trying to dispose TimedUserAccess with ShouldPool set to false!");
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

	public void CopyTo(TimedUserAccess instance)
	{
		instance.steamid = steamid;
		instance.secondsLeft = secondsLeft;
	}

	public TimedUserAccess Copy()
	{
		TimedUserAccess timedUserAccess = Pool.Get<TimedUserAccess>();
		CopyTo(timedUserAccess);
		return timedUserAccess;
	}

	public static TimedUserAccess Deserialize(BufferStream stream)
	{
		TimedUserAccess timedUserAccess = Pool.Get<TimedUserAccess>();
		Deserialize(stream, timedUserAccess, isDelta: false);
		return timedUserAccess;
	}

	public static TimedUserAccess DeserializeLengthDelimited(BufferStream stream)
	{
		TimedUserAccess timedUserAccess = Pool.Get<TimedUserAccess>();
		DeserializeLengthDelimited(stream, timedUserAccess, isDelta: false);
		return timedUserAccess;
	}

	public static TimedUserAccess DeserializeLength(BufferStream stream, int length)
	{
		TimedUserAccess timedUserAccess = Pool.Get<TimedUserAccess>();
		DeserializeLength(stream, length, timedUserAccess, isDelta: false);
		return timedUserAccess;
	}

	public static TimedUserAccess Deserialize(byte[] buffer)
	{
		TimedUserAccess timedUserAccess = Pool.Get<TimedUserAccess>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, timedUserAccess, isDelta: false);
		return timedUserAccess;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TimedUserAccess previous)
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

	public static TimedUserAccess Deserialize(BufferStream stream, TimedUserAccess instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TimedUserAccess");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				instance.secondsLeft = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TimedUserAccess DeserializeLengthDelimited(BufferStream stream, TimedUserAccess instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TimedUserAccess");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				instance.secondsLeft = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TimedUserAccess DeserializeLength(BufferStream stream, int length, TimedUserAccess instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TimedUserAccess");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				instance.secondsLeft = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TimedUserAccess");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TimedUserAccess instance, TimedUserAccess previous)
	{
		if (instance.steamid != previous.steamid)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamid);
		}
		if (instance.secondsLeft != previous.secondsLeft)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.secondsLeft);
		}
	}

	public static void Serialize(BufferStream stream, TimedUserAccess instance)
	{
		if (instance.steamid != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamid);
		}
		if (instance.secondsLeft != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.secondsLeft);
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
