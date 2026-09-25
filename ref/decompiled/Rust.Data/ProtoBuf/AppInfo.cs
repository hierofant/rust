using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppInfo : IDisposable, Pool.IPooled, IProto<AppInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public string headerImage;

	[NonSerialized]
	public string url;

	[NonSerialized]
	public string map;

	[NonSerialized]
	public uint mapSize;

	[NonSerialized]
	public uint wipeTime;

	[NonSerialized]
	public uint players;

	[NonSerialized]
	public uint maxPlayers;

	[NonSerialized]
	public uint queuedPlayers;

	[NonSerialized]
	public uint seed;

	[NonSerialized]
	public uint salt;

	[NonSerialized]
	public string logoImage;

	[NonSerialized]
	public string nexus;

	[NonSerialized]
	public int nexusId;

	[NonSerialized]
	public string nexusZone;

	[NonSerialized]
	public bool camerasEnabled;

	public static void ResetToPool(AppInfo instance)
	{
		if (instance.ShouldPool)
		{
			instance.name = string.Empty;
			instance.headerImage = string.Empty;
			instance.url = string.Empty;
			instance.map = string.Empty;
			instance.mapSize = 0u;
			instance.wipeTime = 0u;
			instance.players = 0u;
			instance.maxPlayers = 0u;
			instance.queuedPlayers = 0u;
			instance.seed = 0u;
			instance.salt = 0u;
			instance.logoImage = string.Empty;
			instance.nexus = string.Empty;
			instance.nexusId = 0;
			instance.nexusZone = string.Empty;
			instance.camerasEnabled = false;
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
			throw new Exception("Trying to dispose AppInfo with ShouldPool set to false!");
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

	public void CopyTo(AppInfo instance)
	{
		instance.name = name;
		instance.headerImage = headerImage;
		instance.url = url;
		instance.map = map;
		instance.mapSize = mapSize;
		instance.wipeTime = wipeTime;
		instance.players = players;
		instance.maxPlayers = maxPlayers;
		instance.queuedPlayers = queuedPlayers;
		instance.seed = seed;
		instance.salt = salt;
		instance.logoImage = logoImage;
		instance.nexus = nexus;
		instance.nexusId = nexusId;
		instance.nexusZone = nexusZone;
		instance.camerasEnabled = camerasEnabled;
	}

	public AppInfo Copy()
	{
		AppInfo appInfo = Pool.Get<AppInfo>();
		CopyTo(appInfo);
		return appInfo;
	}

	public static AppInfo Deserialize(BufferStream stream)
	{
		AppInfo appInfo = Pool.Get<AppInfo>();
		Deserialize(stream, appInfo, isDelta: false);
		return appInfo;
	}

	public static AppInfo DeserializeLengthDelimited(BufferStream stream)
	{
		AppInfo appInfo = Pool.Get<AppInfo>();
		DeserializeLengthDelimited(stream, appInfo, isDelta: false);
		return appInfo;
	}

	public static AppInfo DeserializeLength(BufferStream stream, int length)
	{
		AppInfo appInfo = Pool.Get<AppInfo>();
		DeserializeLength(stream, length, appInfo, isDelta: false);
		return appInfo;
	}

	public static AppInfo Deserialize(byte[] buffer)
	{
		AppInfo appInfo = Pool.Get<AppInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appInfo, isDelta: false);
		return appInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppInfo previous)
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

	public static AppInfo Deserialize(BufferStream stream, AppInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.mapSize = 0u;
			instance.wipeTime = 0u;
			instance.players = 0u;
			instance.maxPlayers = 0u;
			instance.queuedPlayers = 0u;
			instance.seed = 0u;
			instance.salt = 0u;
			instance.nexusId = 0;
			instance.camerasEnabled = false;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppInfo");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.headerImage = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.url = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.map = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.mapSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.wipeTime = ProtocolParser.ReadUInt32(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.players = ProtocolParser.ReadUInt32(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.maxPlayers = ProtocolParser.ReadUInt32(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.queuedPlayers = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.seed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.salt = ProtocolParser.ReadUInt32(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.logoImage = ProtocolParser.ReadString(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexus = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexusId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexusZone = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				if (key.WireType == Wire.Varint)
				{
					instance.camerasEnabled = ProtocolParser.ReadBool(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppInfo DeserializeLengthDelimited(BufferStream stream, AppInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.mapSize = 0u;
			instance.wipeTime = 0u;
			instance.players = 0u;
			instance.maxPlayers = 0u;
			instance.queuedPlayers = 0u;
			instance.seed = 0u;
			instance.salt = 0u;
			instance.nexusId = 0;
			instance.camerasEnabled = false;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppInfo");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.headerImage = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.url = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.map = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.mapSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.wipeTime = ProtocolParser.ReadUInt32(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.players = ProtocolParser.ReadUInt32(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.maxPlayers = ProtocolParser.ReadUInt32(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.queuedPlayers = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.seed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.salt = ProtocolParser.ReadUInt32(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.logoImage = ProtocolParser.ReadString(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexus = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexusId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexusZone = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				if (key.WireType == Wire.Varint)
				{
					instance.camerasEnabled = ProtocolParser.ReadBool(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppInfo DeserializeLength(BufferStream stream, int length, AppInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.mapSize = 0u;
			instance.wipeTime = 0u;
			instance.players = 0u;
			instance.maxPlayers = 0u;
			instance.queuedPlayers = 0u;
			instance.seed = 0u;
			instance.salt = 0u;
			instance.nexusId = 0;
			instance.camerasEnabled = false;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppInfo");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.headerImage = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.url = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.map = ProtocolParser.ReadString(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.mapSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.wipeTime = ProtocolParser.ReadUInt32(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.players = ProtocolParser.ReadUInt32(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.maxPlayers = ProtocolParser.ReadUInt32(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.queuedPlayers = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.seed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.salt = ProtocolParser.ReadUInt32(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.logoImage = ProtocolParser.ReadString(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexus = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexusId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				instance.nexusZone = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.AppInfo");
				if (key.WireType == Wire.Varint)
				{
					instance.camerasEnabled = ProtocolParser.ReadBool(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppInfo instance, AppInfo previous)
	{
		if (instance.name != previous.name)
		{
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.headerImage != previous.headerImage)
		{
			if (instance.headerImage == null)
			{
				throw new ArgumentNullException("headerImage", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.headerImage);
		}
		if (instance.url != previous.url)
		{
			if (instance.url == null)
			{
				throw new ArgumentNullException("url", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.url);
		}
		if (instance.map != previous.map)
		{
			if (instance.map == null)
			{
				throw new ArgumentNullException("map", "Required by proto specification.");
			}
			stream.WriteByte(34);
			ProtocolParser.WriteString(stream, instance.map);
		}
		if (instance.mapSize != previous.mapSize)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.mapSize);
		}
		if (instance.wipeTime != previous.wipeTime)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt32(stream, instance.wipeTime);
		}
		if (instance.players != previous.players)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt32(stream, instance.players);
		}
		if (instance.maxPlayers != previous.maxPlayers)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt32(stream, instance.maxPlayers);
		}
		if (instance.queuedPlayers != previous.queuedPlayers)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.queuedPlayers);
		}
		if (instance.seed != previous.seed)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt32(stream, instance.seed);
		}
		if (instance.salt != previous.salt)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt32(stream, instance.salt);
		}
		if (instance.logoImage != null && instance.logoImage != previous.logoImage)
		{
			stream.WriteByte(98);
			ProtocolParser.WriteString(stream, instance.logoImage);
		}
		if (instance.nexus != null && instance.nexus != previous.nexus)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteString(stream, instance.nexus);
		}
		if (instance.nexusId != previous.nexusId)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.nexusId);
		}
		if (instance.nexusZone != null && instance.nexusZone != previous.nexusZone)
		{
			stream.WriteByte(122);
			ProtocolParser.WriteString(stream, instance.nexusZone);
		}
		stream.WriteByte(128);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.camerasEnabled);
	}

	public static void Serialize(BufferStream stream, AppInfo instance)
	{
		if (instance.name == null)
		{
			throw new ArgumentNullException("name", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.name);
		if (instance.headerImage == null)
		{
			throw new ArgumentNullException("headerImage", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.headerImage);
		if (instance.url == null)
		{
			throw new ArgumentNullException("url", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteString(stream, instance.url);
		if (instance.map == null)
		{
			throw new ArgumentNullException("map", "Required by proto specification.");
		}
		stream.WriteByte(34);
		ProtocolParser.WriteString(stream, instance.map);
		if (instance.mapSize != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt32(stream, instance.mapSize);
		}
		if (instance.wipeTime != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt32(stream, instance.wipeTime);
		}
		if (instance.players != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt32(stream, instance.players);
		}
		if (instance.maxPlayers != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt32(stream, instance.maxPlayers);
		}
		if (instance.queuedPlayers != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.queuedPlayers);
		}
		if (instance.seed != 0)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt32(stream, instance.seed);
		}
		if (instance.salt != 0)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt32(stream, instance.salt);
		}
		if (instance.logoImage != null)
		{
			stream.WriteByte(98);
			ProtocolParser.WriteString(stream, instance.logoImage);
		}
		if (instance.nexus != null)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteString(stream, instance.nexus);
		}
		if (instance.nexusId != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.nexusId);
		}
		if (instance.nexusZone != null)
		{
			stream.WriteByte(122);
			ProtocolParser.WriteString(stream, instance.nexusZone);
		}
		if (instance.camerasEnabled)
		{
			stream.WriteByte(128);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.camerasEnabled);
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
