using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Approval : IDisposable, Pool.IPooled, IProto<Approval>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string level;

	[NonSerialized]
	public string hostname;

	[NonSerialized]
	public bool modded;

	[NonSerialized]
	public bool official;

	[NonSerialized]
	public ulong steamid;

	[NonSerialized]
	public uint ipaddress;

	[NonSerialized]
	public int port;

	[NonSerialized]
	public uint levelSeed;

	[NonSerialized]
	public uint levelSize;

	[NonSerialized]
	public string checksum;

	[NonSerialized]
	public uint encryption;

	[NonSerialized]
	public string levelUrl;

	[NonSerialized]
	public bool levelTransfer;

	[NonSerialized]
	public string version;

	[NonSerialized]
	public string levelConfig;

	[NonSerialized]
	public bool nexus;

	[NonSerialized]
	public string nexusEndpoint;

	[NonSerialized]
	public int nexusId;

	[NonSerialized]
	public string dnsEndpoint;

	public static void ResetToPool(Approval instance)
	{
		if (instance.ShouldPool)
		{
			instance.level = string.Empty;
			instance.hostname = string.Empty;
			instance.modded = false;
			instance.official = false;
			instance.steamid = 0uL;
			instance.ipaddress = 0u;
			instance.port = 0;
			instance.levelSeed = 0u;
			instance.levelSize = 0u;
			instance.checksum = string.Empty;
			instance.encryption = 0u;
			instance.levelUrl = string.Empty;
			instance.levelTransfer = false;
			instance.version = string.Empty;
			instance.levelConfig = string.Empty;
			instance.nexus = false;
			instance.nexusEndpoint = string.Empty;
			instance.nexusId = 0;
			instance.dnsEndpoint = string.Empty;
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
			throw new Exception("Trying to dispose Approval with ShouldPool set to false!");
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

	public void CopyTo(Approval instance)
	{
		instance.level = level;
		instance.hostname = hostname;
		instance.modded = modded;
		instance.official = official;
		instance.steamid = steamid;
		instance.ipaddress = ipaddress;
		instance.port = port;
		instance.levelSeed = levelSeed;
		instance.levelSize = levelSize;
		instance.checksum = checksum;
		instance.encryption = encryption;
		instance.levelUrl = levelUrl;
		instance.levelTransfer = levelTransfer;
		instance.version = version;
		instance.levelConfig = levelConfig;
		instance.nexus = nexus;
		instance.nexusEndpoint = nexusEndpoint;
		instance.nexusId = nexusId;
		instance.dnsEndpoint = dnsEndpoint;
	}

	public Approval Copy()
	{
		Approval approval = Pool.Get<Approval>();
		CopyTo(approval);
		return approval;
	}

	public static Approval Deserialize(BufferStream stream)
	{
		Approval approval = Pool.Get<Approval>();
		Deserialize(stream, approval, isDelta: false);
		return approval;
	}

	public static Approval DeserializeLengthDelimited(BufferStream stream)
	{
		Approval approval = Pool.Get<Approval>();
		DeserializeLengthDelimited(stream, approval, isDelta: false);
		return approval;
	}

	public static Approval DeserializeLength(BufferStream stream, int length)
	{
		Approval approval = Pool.Get<Approval>();
		DeserializeLength(stream, length, approval, isDelta: false);
		return approval;
	}

	public static Approval Deserialize(byte[] buffer)
	{
		Approval approval = Pool.Get<Approval>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, approval, isDelta: false);
		return approval;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Approval previous)
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

	public static Approval Deserialize(BufferStream stream, Approval instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Approval");
			switch (num)
			{
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.level = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.hostname = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.modded = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.official = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.ipaddress = ProtocolParser.ReadUInt32(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.port = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelSeed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.checksum = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.encryption = ProtocolParser.ReadUInt32(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelUrl = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelTransfer = ProtocolParser.ReadBool(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.version = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.levelConfig = ProtocolParser.ReadString(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.Varint)
				{
					instance.nexus = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.nexusEndpoint = ProtocolParser.ReadString(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.Varint)
				{
					instance.nexusId = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.dnsEndpoint = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Approval DeserializeLengthDelimited(BufferStream stream, Approval instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Approval");
			switch (num2)
			{
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.level = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.hostname = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.modded = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.official = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.ipaddress = ProtocolParser.ReadUInt32(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.port = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelSeed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.checksum = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.encryption = ProtocolParser.ReadUInt32(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelUrl = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelTransfer = ProtocolParser.ReadBool(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.version = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.levelConfig = ProtocolParser.ReadString(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.Varint)
				{
					instance.nexus = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.nexusEndpoint = ProtocolParser.ReadString(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.Varint)
				{
					instance.nexusId = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.dnsEndpoint = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Approval DeserializeLength(BufferStream stream, int length, Approval instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Approval");
			switch (num2)
			{
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.level = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.hostname = ProtocolParser.ReadString(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.modded = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.official = ProtocolParser.ReadBool(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.ipaddress = ProtocolParser.ReadUInt32(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.port = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelSeed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.checksum = ProtocolParser.ReadString(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.encryption = ProtocolParser.ReadUInt32(stream);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelUrl = ProtocolParser.ReadString(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.levelTransfer = ProtocolParser.ReadBool(stream);
				continue;
			case 122:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Approval");
				instance.version = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 16u:
				stream.ValidateFieldOrder(ref lastFieldId, 16u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.levelConfig = ProtocolParser.ReadString(stream);
				}
				break;
			case 17u:
				stream.ValidateFieldOrder(ref lastFieldId, 17u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.Varint)
				{
					instance.nexus = ProtocolParser.ReadBool(stream);
				}
				break;
			case 18u:
				stream.ValidateFieldOrder(ref lastFieldId, 18u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.nexusEndpoint = ProtocolParser.ReadString(stream);
				}
				break;
			case 19u:
				stream.ValidateFieldOrder(ref lastFieldId, 19u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.Varint)
				{
					instance.nexusId = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 20u:
				stream.ValidateFieldOrder(ref lastFieldId, 20u, fieldIsRepeated: false, "ProtoBuf.Approval");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.dnsEndpoint = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Approval");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Approval instance, Approval previous)
	{
		if (instance.level != previous.level)
		{
			if (instance.level == null)
			{
				throw new ArgumentNullException("level", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.level);
		}
		if (instance.hostname != null && instance.hostname != previous.hostname)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.hostname);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.modded);
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.official);
		if (instance.steamid != previous.steamid)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.steamid);
		}
		if (instance.ipaddress != previous.ipaddress)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt32(stream, instance.ipaddress);
		}
		if (instance.port != previous.port)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.port);
		}
		if (instance.levelSeed != previous.levelSeed)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.levelSeed);
		}
		if (instance.levelSize != previous.levelSize)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt32(stream, instance.levelSize);
		}
		if (instance.checksum != null && instance.checksum != previous.checksum)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.checksum);
		}
		if (instance.encryption != previous.encryption)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt32(stream, instance.encryption);
		}
		if (instance.levelUrl != null && instance.levelUrl != previous.levelUrl)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteString(stream, instance.levelUrl);
		}
		stream.WriteByte(112);
		ProtocolParser.WriteBool(stream, instance.levelTransfer);
		if (instance.version != null && instance.version != previous.version)
		{
			stream.WriteByte(122);
			ProtocolParser.WriteString(stream, instance.version);
		}
		if (instance.levelConfig != null && instance.levelConfig != previous.levelConfig)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.levelConfig);
		}
		stream.WriteByte(136);
		stream.WriteByte(1);
		ProtocolParser.WriteBool(stream, instance.nexus);
		if (instance.nexusEndpoint != null && instance.nexusEndpoint != previous.nexusEndpoint)
		{
			stream.WriteByte(146);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.nexusEndpoint);
		}
		if (instance.nexusId != previous.nexusId)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.nexusId);
		}
		if (instance.dnsEndpoint != null && instance.dnsEndpoint != previous.dnsEndpoint)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.dnsEndpoint);
		}
	}

	public static void Serialize(BufferStream stream, Approval instance)
	{
		if (instance.level == null)
		{
			throw new ArgumentNullException("level", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.level);
		if (instance.hostname != null)
		{
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.hostname);
		}
		if (instance.modded)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.modded);
		}
		if (instance.official)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.official);
		}
		if (instance.steamid != 0L)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.steamid);
		}
		if (instance.ipaddress != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt32(stream, instance.ipaddress);
		}
		if (instance.port != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.port);
		}
		if (instance.levelSeed != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.levelSeed);
		}
		if (instance.levelSize != 0)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt32(stream, instance.levelSize);
		}
		if (instance.checksum != null)
		{
			stream.WriteByte(90);
			ProtocolParser.WriteString(stream, instance.checksum);
		}
		if (instance.encryption != 0)
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt32(stream, instance.encryption);
		}
		if (instance.levelUrl != null)
		{
			stream.WriteByte(106);
			ProtocolParser.WriteString(stream, instance.levelUrl);
		}
		if (instance.levelTransfer)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteBool(stream, instance.levelTransfer);
		}
		if (instance.version != null)
		{
			stream.WriteByte(122);
			ProtocolParser.WriteString(stream, instance.version);
		}
		if (instance.levelConfig != null)
		{
			stream.WriteByte(130);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.levelConfig);
		}
		if (instance.nexus)
		{
			stream.WriteByte(136);
			stream.WriteByte(1);
			ProtocolParser.WriteBool(stream, instance.nexus);
		}
		if (instance.nexusEndpoint != null)
		{
			stream.WriteByte(146);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.nexusEndpoint);
		}
		if (instance.nexusId != 0)
		{
			stream.WriteByte(152);
			stream.WriteByte(1);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.nexusId);
		}
		if (instance.dnsEndpoint != null)
		{
			stream.WriteByte(162);
			stream.WriteByte(1);
			ProtocolParser.WriteString(stream, instance.dnsEndpoint);
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
