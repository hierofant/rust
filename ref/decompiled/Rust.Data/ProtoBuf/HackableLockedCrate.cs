using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class HackableLockedCrate : IDisposable, Pool.IPooled, IProto<HackableLockedCrate>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float hackSeconds;

	[NonSerialized]
	public ulong originalHackerPlayerId;

	public static void ResetToPool(HackableLockedCrate instance)
	{
		if (instance.ShouldPool)
		{
			instance.hackSeconds = 0f;
			instance.originalHackerPlayerId = 0uL;
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
			throw new Exception("Trying to dispose HackableLockedCrate with ShouldPool set to false!");
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

	public void CopyTo(HackableLockedCrate instance)
	{
		instance.hackSeconds = hackSeconds;
		instance.originalHackerPlayerId = originalHackerPlayerId;
	}

	public HackableLockedCrate Copy()
	{
		HackableLockedCrate hackableLockedCrate = Pool.Get<HackableLockedCrate>();
		CopyTo(hackableLockedCrate);
		return hackableLockedCrate;
	}

	public static HackableLockedCrate Deserialize(BufferStream stream)
	{
		HackableLockedCrate hackableLockedCrate = Pool.Get<HackableLockedCrate>();
		Deserialize(stream, hackableLockedCrate, isDelta: false);
		return hackableLockedCrate;
	}

	public static HackableLockedCrate DeserializeLengthDelimited(BufferStream stream)
	{
		HackableLockedCrate hackableLockedCrate = Pool.Get<HackableLockedCrate>();
		DeserializeLengthDelimited(stream, hackableLockedCrate, isDelta: false);
		return hackableLockedCrate;
	}

	public static HackableLockedCrate DeserializeLength(BufferStream stream, int length)
	{
		HackableLockedCrate hackableLockedCrate = Pool.Get<HackableLockedCrate>();
		DeserializeLength(stream, length, hackableLockedCrate, isDelta: false);
		return hackableLockedCrate;
	}

	public static HackableLockedCrate Deserialize(byte[] buffer)
	{
		HackableLockedCrate hackableLockedCrate = Pool.Get<HackableLockedCrate>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, hackableLockedCrate, isDelta: false);
		return hackableLockedCrate;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, HackableLockedCrate previous)
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

	public static HackableLockedCrate Deserialize(BufferStream stream, HackableLockedCrate instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.HackableLockedCrate");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				instance.hackSeconds = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				instance.originalHackerPlayerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static HackableLockedCrate DeserializeLengthDelimited(BufferStream stream, HackableLockedCrate instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HackableLockedCrate");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				instance.hackSeconds = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				instance.originalHackerPlayerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static HackableLockedCrate DeserializeLength(BufferStream stream, int length, HackableLockedCrate instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.HackableLockedCrate");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				instance.hackSeconds = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				instance.originalHackerPlayerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.HackableLockedCrate");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, HackableLockedCrate instance, HackableLockedCrate previous)
	{
		if (instance.hackSeconds != previous.hackSeconds)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.hackSeconds);
		}
		if (instance.originalHackerPlayerId != previous.originalHackerPlayerId)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.originalHackerPlayerId);
		}
	}

	public static void Serialize(BufferStream stream, HackableLockedCrate instance)
	{
		if (instance.hackSeconds != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.hackSeconds);
		}
		if (instance.originalHackerPlayerId != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.originalHackerPlayerId);
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
