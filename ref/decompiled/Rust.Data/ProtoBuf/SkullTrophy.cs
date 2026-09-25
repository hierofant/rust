using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SkullTrophy : IDisposable, Pool.IPooled, IProto<SkullTrophy>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string playerName;

	[NonSerialized]
	public string streamerName;

	public static void ResetToPool(SkullTrophy instance)
	{
		if (instance.ShouldPool)
		{
			instance.playerName = string.Empty;
			instance.streamerName = string.Empty;
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
			throw new Exception("Trying to dispose SkullTrophy with ShouldPool set to false!");
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

	public void CopyTo(SkullTrophy instance)
	{
		instance.playerName = playerName;
		instance.streamerName = streamerName;
	}

	public SkullTrophy Copy()
	{
		SkullTrophy skullTrophy = Pool.Get<SkullTrophy>();
		CopyTo(skullTrophy);
		return skullTrophy;
	}

	public static SkullTrophy Deserialize(BufferStream stream)
	{
		SkullTrophy skullTrophy = Pool.Get<SkullTrophy>();
		Deserialize(stream, skullTrophy, isDelta: false);
		return skullTrophy;
	}

	public static SkullTrophy DeserializeLengthDelimited(BufferStream stream)
	{
		SkullTrophy skullTrophy = Pool.Get<SkullTrophy>();
		DeserializeLengthDelimited(stream, skullTrophy, isDelta: false);
		return skullTrophy;
	}

	public static SkullTrophy DeserializeLength(BufferStream stream, int length)
	{
		SkullTrophy skullTrophy = Pool.Get<SkullTrophy>();
		DeserializeLength(stream, length, skullTrophy, isDelta: false);
		return skullTrophy;
	}

	public static SkullTrophy Deserialize(byte[] buffer)
	{
		SkullTrophy skullTrophy = Pool.Get<SkullTrophy>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, skullTrophy, isDelta: false);
		return skullTrophy;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SkullTrophy previous)
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

	public static SkullTrophy Deserialize(BufferStream stream, SkullTrophy instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SkullTrophy");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				instance.streamerName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SkullTrophy DeserializeLengthDelimited(BufferStream stream, SkullTrophy instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SkullTrophy");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				instance.streamerName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SkullTrophy DeserializeLength(BufferStream stream, int length, SkullTrophy instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SkullTrophy");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				instance.streamerName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SkullTrophy");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SkullTrophy instance, SkullTrophy previous)
	{
		if (instance.playerName != null && instance.playerName != previous.playerName)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.playerName);
		}
		if (instance.streamerName != null && instance.streamerName != previous.streamerName)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.streamerName);
		}
	}

	public static void Serialize(BufferStream stream, SkullTrophy instance)
	{
		if (instance.playerName != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.playerName);
		}
		if (instance.streamerName != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.streamerName);
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
