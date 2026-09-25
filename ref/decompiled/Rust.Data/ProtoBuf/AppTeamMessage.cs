using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppTeamMessage : IDisposable, Pool.IPooled, IProto<AppTeamMessage>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong steamId;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public string message;

	[NonSerialized]
	public string color;

	[NonSerialized]
	public uint time;

	public static void ResetToPool(AppTeamMessage instance)
	{
		if (instance.ShouldPool)
		{
			instance.steamId = 0uL;
			instance.name = string.Empty;
			instance.message = string.Empty;
			instance.color = string.Empty;
			instance.time = 0u;
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
			throw new Exception("Trying to dispose AppTeamMessage with ShouldPool set to false!");
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

	public void CopyTo(AppTeamMessage instance)
	{
		instance.steamId = steamId;
		instance.name = name;
		instance.message = message;
		instance.color = color;
		instance.time = time;
	}

	public AppTeamMessage Copy()
	{
		AppTeamMessage appTeamMessage = Pool.Get<AppTeamMessage>();
		CopyTo(appTeamMessage);
		return appTeamMessage;
	}

	public static AppTeamMessage Deserialize(BufferStream stream)
	{
		AppTeamMessage appTeamMessage = Pool.Get<AppTeamMessage>();
		Deserialize(stream, appTeamMessage, isDelta: false);
		return appTeamMessage;
	}

	public static AppTeamMessage DeserializeLengthDelimited(BufferStream stream)
	{
		AppTeamMessage appTeamMessage = Pool.Get<AppTeamMessage>();
		DeserializeLengthDelimited(stream, appTeamMessage, isDelta: false);
		return appTeamMessage;
	}

	public static AppTeamMessage DeserializeLength(BufferStream stream, int length)
	{
		AppTeamMessage appTeamMessage = Pool.Get<AppTeamMessage>();
		DeserializeLength(stream, length, appTeamMessage, isDelta: false);
		return appTeamMessage;
	}

	public static AppTeamMessage Deserialize(byte[] buffer)
	{
		AppTeamMessage appTeamMessage = Pool.Get<AppTeamMessage>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appTeamMessage, isDelta: false);
		return appTeamMessage;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppTeamMessage previous)
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

	public static AppTeamMessage Deserialize(BufferStream stream, AppTeamMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.steamId = 0uL;
			instance.time = 0u;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamMessage");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.color = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.time = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppTeamMessage DeserializeLengthDelimited(BufferStream stream, AppTeamMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.steamId = 0uL;
			instance.time = 0u;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.color = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.time = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppTeamMessage DeserializeLength(BufferStream stream, int length, AppTeamMessage instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.steamId = 0uL;
			instance.time = 0u;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamMessage");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.message = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.color = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				instance.time = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamMessage");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppTeamMessage instance, AppTeamMessage previous)
	{
		if (instance.steamId != previous.steamId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamId);
		}
		if (instance.name != previous.name)
		{
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.message != previous.message)
		{
			if (instance.message == null)
			{
				throw new ArgumentNullException("message", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.message);
		}
		if (instance.color != previous.color)
		{
			if (instance.color == null)
			{
				throw new ArgumentNullException("color", "Required by proto specification.");
			}
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.color);
		}
		if (instance.time != previous.time)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.time);
		}
	}

	public static void Serialize(BufferStream stream, AppTeamMessage instance)
	{
		if (instance.steamId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamId);
		}
		if (instance.name == null)
		{
			throw new ArgumentNullException("name", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.name);
		if (instance.message == null)
		{
			throw new ArgumentNullException("message", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteString(stream, instance.message);
		if (instance.color == null)
		{
			throw new ArgumentNullException("color", "Required by proto specification.");
		}
		stream.WriteByte(34);
		ProtocolParser.WriteString(stream, instance.color);
		if (instance.time != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.time);
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
