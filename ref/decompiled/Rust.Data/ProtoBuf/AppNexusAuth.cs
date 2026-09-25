using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppNexusAuth : IDisposable, Pool.IPooled, IProto<AppNexusAuth>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string serverId;

	[NonSerialized]
	public int playerToken;

	public static void ResetToPool(AppNexusAuth instance)
	{
		if (instance.ShouldPool)
		{
			instance.serverId = string.Empty;
			instance.playerToken = 0;
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
			throw new Exception("Trying to dispose AppNexusAuth with ShouldPool set to false!");
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

	public void CopyTo(AppNexusAuth instance)
	{
		instance.serverId = serverId;
		instance.playerToken = playerToken;
	}

	public AppNexusAuth Copy()
	{
		AppNexusAuth appNexusAuth = Pool.Get<AppNexusAuth>();
		CopyTo(appNexusAuth);
		return appNexusAuth;
	}

	public static AppNexusAuth Deserialize(BufferStream stream)
	{
		AppNexusAuth appNexusAuth = Pool.Get<AppNexusAuth>();
		Deserialize(stream, appNexusAuth, isDelta: false);
		return appNexusAuth;
	}

	public static AppNexusAuth DeserializeLengthDelimited(BufferStream stream)
	{
		AppNexusAuth appNexusAuth = Pool.Get<AppNexusAuth>();
		DeserializeLengthDelimited(stream, appNexusAuth, isDelta: false);
		return appNexusAuth;
	}

	public static AppNexusAuth DeserializeLength(BufferStream stream, int length)
	{
		AppNexusAuth appNexusAuth = Pool.Get<AppNexusAuth>();
		DeserializeLength(stream, length, appNexusAuth, isDelta: false);
		return appNexusAuth;
	}

	public static AppNexusAuth Deserialize(byte[] buffer)
	{
		AppNexusAuth appNexusAuth = Pool.Get<AppNexusAuth>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appNexusAuth, isDelta: false);
		return appNexusAuth;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppNexusAuth previous)
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

	public static AppNexusAuth Deserialize(BufferStream stream, AppNexusAuth instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.playerToken = 0;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppNexusAuth");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				instance.serverId = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				instance.playerToken = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppNexusAuth DeserializeLengthDelimited(BufferStream stream, AppNexusAuth instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.playerToken = 0;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppNexusAuth");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				instance.serverId = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				instance.playerToken = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppNexusAuth DeserializeLength(BufferStream stream, int length, AppNexusAuth instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.playerToken = 0;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppNexusAuth");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				instance.serverId = ProtocolParser.ReadString(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				instance.playerToken = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppNexusAuth");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppNexusAuth instance, AppNexusAuth previous)
	{
		if (instance.serverId != previous.serverId)
		{
			if (instance.serverId == null)
			{
				throw new ArgumentNullException("serverId", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.serverId);
		}
		if (instance.playerToken != previous.playerToken)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerToken);
		}
	}

	public static void Serialize(BufferStream stream, AppNexusAuth instance)
	{
		if (instance.serverId == null)
		{
			throw new ArgumentNullException("serverId", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.serverId);
		if (instance.playerToken != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.playerToken);
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
