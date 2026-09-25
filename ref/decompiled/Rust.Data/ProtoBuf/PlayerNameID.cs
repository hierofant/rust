using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerNameID : IDisposable, Pool.IPooled, IProto<PlayerNameID>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string username;

	[NonSerialized]
	public ulong userid;

	public static void ResetToPool(PlayerNameID instance)
	{
		if (instance.ShouldPool)
		{
			instance.username = string.Empty;
			instance.userid = 0uL;
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
			throw new Exception("Trying to dispose PlayerNameID with ShouldPool set to false!");
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

	public void CopyTo(PlayerNameID instance)
	{
		instance.username = username;
		instance.userid = userid;
	}

	public PlayerNameID Copy()
	{
		PlayerNameID playerNameID = Pool.Get<PlayerNameID>();
		CopyTo(playerNameID);
		return playerNameID;
	}

	public static PlayerNameID Deserialize(BufferStream stream)
	{
		PlayerNameID playerNameID = Pool.Get<PlayerNameID>();
		Deserialize(stream, playerNameID, isDelta: false);
		return playerNameID;
	}

	public static PlayerNameID DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerNameID playerNameID = Pool.Get<PlayerNameID>();
		DeserializeLengthDelimited(stream, playerNameID, isDelta: false);
		return playerNameID;
	}

	public static PlayerNameID DeserializeLength(BufferStream stream, int length)
	{
		PlayerNameID playerNameID = Pool.Get<PlayerNameID>();
		DeserializeLength(stream, length, playerNameID, isDelta: false);
		return playerNameID;
	}

	public static PlayerNameID Deserialize(byte[] buffer)
	{
		PlayerNameID playerNameID = Pool.Get<PlayerNameID>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerNameID, isDelta: false);
		return playerNameID;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerNameID previous)
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

	public static PlayerNameID Deserialize(BufferStream stream, PlayerNameID instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerNameID");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				instance.username = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerNameID DeserializeLengthDelimited(BufferStream stream, PlayerNameID instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerNameID");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				instance.username = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerNameID DeserializeLength(BufferStream stream, int length, PlayerNameID instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerNameID");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				instance.username = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerNameID");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerNameID instance, PlayerNameID previous)
	{
		if (instance.username != null && instance.username != previous.username)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.username);
		}
		if (instance.userid != previous.userid)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.userid);
		}
	}

	public static void Serialize(BufferStream stream, PlayerNameID instance)
	{
		if (instance.username != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.username);
		}
		if (instance.userid != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.userid);
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
