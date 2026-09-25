using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class DemoHeader : IDisposable, Pool.IPooled, IProto<DemoHeader>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint version;

	[NonSerialized]
	public string level;

	[NonSerialized]
	public uint levelSeed;

	[NonSerialized]
	public uint levelSize;

	[NonSerialized]
	public string checksum;

	[NonSerialized]
	public ulong localclient;

	[NonSerialized]
	public Vector3 position;

	[NonSerialized]
	public Vector3 rotation;

	[NonSerialized]
	public string levelUrl;

	[NonSerialized]
	public long recordedTime;

	[NonSerialized]
	public long length;

	[NonSerialized]
	public List<FileStorageCacheData> fileStorage;

	[NonSerialized]
	public bool nexus;

	[NonSerialized]
	public uint manifestCRC;

	public static void ResetToPool(DemoHeader instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.version = 0u;
		instance.level = string.Empty;
		instance.levelSeed = 0u;
		instance.levelSize = 0u;
		instance.checksum = string.Empty;
		instance.localclient = 0uL;
		instance.position = default(Vector3);
		instance.rotation = default(Vector3);
		instance.levelUrl = string.Empty;
		instance.recordedTime = 0L;
		instance.length = 0L;
		if (instance.fileStorage != null)
		{
			for (int i = 0; i < instance.fileStorage.Count; i++)
			{
				if (instance.fileStorage[i] != null)
				{
					instance.fileStorage[i].ResetToPool();
					instance.fileStorage[i] = null;
				}
			}
			List<FileStorageCacheData> obj = instance.fileStorage;
			Pool.Free(ref obj, freeElements: false);
			instance.fileStorage = obj;
		}
		instance.nexus = false;
		instance.manifestCRC = 0u;
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose DemoHeader with ShouldPool set to false!");
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

	public void CopyTo(DemoHeader instance)
	{
		instance.version = version;
		instance.level = level;
		instance.levelSeed = levelSeed;
		instance.levelSize = levelSize;
		instance.checksum = checksum;
		instance.localclient = localclient;
		instance.position = position;
		instance.rotation = rotation;
		instance.levelUrl = levelUrl;
		instance.recordedTime = recordedTime;
		instance.length = length;
		if (fileStorage != null)
		{
			instance.fileStorage = Pool.Get<List<FileStorageCacheData>>();
			for (int i = 0; i < fileStorage.Count; i++)
			{
				FileStorageCacheData item = fileStorage[i].Copy();
				instance.fileStorage.Add(item);
			}
		}
		else
		{
			instance.fileStorage = null;
		}
		instance.nexus = nexus;
		instance.manifestCRC = manifestCRC;
	}

	public DemoHeader Copy()
	{
		DemoHeader demoHeader = Pool.Get<DemoHeader>();
		CopyTo(demoHeader);
		return demoHeader;
	}

	public static DemoHeader Deserialize(BufferStream stream)
	{
		DemoHeader demoHeader = Pool.Get<DemoHeader>();
		Deserialize(stream, demoHeader, isDelta: false);
		return demoHeader;
	}

	public static DemoHeader DeserializeLengthDelimited(BufferStream stream)
	{
		DemoHeader demoHeader = Pool.Get<DemoHeader>();
		DeserializeLengthDelimited(stream, demoHeader, isDelta: false);
		return demoHeader;
	}

	public static DemoHeader DeserializeLength(BufferStream stream, int length)
	{
		DemoHeader demoHeader = Pool.Get<DemoHeader>();
		DeserializeLength(stream, length, demoHeader, isDelta: false);
		return demoHeader;
	}

	public static DemoHeader Deserialize(byte[] buffer)
	{
		DemoHeader demoHeader = Pool.Get<DemoHeader>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, demoHeader, isDelta: false);
		return demoHeader;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DemoHeader previous)
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

	public static DemoHeader Deserialize(BufferStream stream, DemoHeader instance, bool isDelta)
	{
		if (!isDelta && instance.fileStorage == null)
		{
			instance.fileStorage = Pool.Get<List<FileStorageCacheData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DemoHeader");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.version = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.level = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelSeed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.checksum = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.localclient = ProtocolParser.ReadUInt64(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelUrl = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.recordedTime = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.length = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				stream.ConsumeRepeatedElement();
				instance.fileStorage.Add(FileStorageCacheData.DeserializeLengthDelimited(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.nexus = ProtocolParser.ReadBool(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.manifestCRC = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoHeader DeserializeLengthDelimited(BufferStream stream, DemoHeader instance, bool isDelta)
	{
		if (!isDelta && instance.fileStorage == null)
		{
			instance.fileStorage = Pool.Get<List<FileStorageCacheData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoHeader");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.version = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.level = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelSeed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.checksum = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.localclient = ProtocolParser.ReadUInt64(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelUrl = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.recordedTime = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.length = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				stream.ConsumeRepeatedElement();
				instance.fileStorage.Add(FileStorageCacheData.DeserializeLengthDelimited(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.nexus = ProtocolParser.ReadBool(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.manifestCRC = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DemoHeader DeserializeLength(BufferStream stream, int length, DemoHeader instance, bool isDelta)
	{
		if (!isDelta && instance.fileStorage == null)
		{
			instance.fileStorage = Pool.Get<List<FileStorageCacheData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DemoHeader");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.version = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.level = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelSeed = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelSize = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.checksum = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.localclient = ProtocolParser.ReadUInt64(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.levelUrl = ProtocolParser.ReadString(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.recordedTime = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.length = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				stream.ConsumeRepeatedElement();
				instance.fileStorage.Add(FileStorageCacheData.DeserializeLengthDelimited(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.nexus = ProtocolParser.ReadBool(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				instance.manifestCRC = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DemoHeader");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DemoHeader instance, DemoHeader previous)
	{
		if (instance.version != previous.version)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.version);
		}
		if (instance.level != previous.level)
		{
			if (instance.level == null)
			{
				throw new ArgumentNullException("level", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.level);
		}
		if (instance.levelSeed != previous.levelSeed)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.levelSeed);
		}
		if (instance.levelSize != previous.levelSize)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.levelSize);
		}
		if (instance.checksum != null && instance.checksum != previous.checksum)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.checksum);
		}
		if (instance.localclient != previous.localclient)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.localclient);
		}
		if (instance.position != previous.position)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int num = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.position, previous.position);
			int num2 = stream.Position - num;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span, 0);
		}
		if (instance.rotation != previous.rotation)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.rotation, previous.rotation);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rotation (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		if (instance.levelUrl != previous.levelUrl)
		{
			if (instance.levelUrl == null)
			{
				throw new ArgumentNullException("levelUrl", "Required by proto specification.");
			}
			stream.WriteByte(74);
			ProtocolParser.WriteString(stream, instance.levelUrl);
		}
		stream.WriteByte(80);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.recordedTime);
		stream.WriteByte(88);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.length);
		if (instance.fileStorage != null)
		{
			for (int i = 0; i < instance.fileStorage.Count; i++)
			{
				FileStorageCacheData fileStorageCacheData = instance.fileStorage[i];
				stream.WriteByte(98);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int num5 = stream.Position;
				FileStorageCacheData.SerializeDelta(stream, fileStorageCacheData, fileStorageCacheData);
				int val = stream.Position - num5;
				Span<byte> span3 = range3.GetSpan();
				int num6 = ProtocolParser.WriteUInt32((uint)val, span3, 0);
				if (num6 < 5)
				{
					span3[num6 - 1] |= 128;
					while (num6 < 4)
					{
						span3[num6++] = 128;
					}
					span3[4] = 0;
				}
			}
		}
		stream.WriteByte(104);
		ProtocolParser.WriteBool(stream, instance.nexus);
		if (instance.manifestCRC != previous.manifestCRC)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt32(stream, instance.manifestCRC);
		}
	}

	public static void Serialize(BufferStream stream, DemoHeader instance)
	{
		if (instance.version != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.version);
		}
		if (instance.level == null)
		{
			throw new ArgumentNullException("level", "Required by proto specification.");
		}
		stream.WriteByte(18);
		ProtocolParser.WriteString(stream, instance.level);
		if (instance.levelSeed != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.levelSeed);
		}
		if (instance.levelSize != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.levelSize);
		}
		if (instance.checksum != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.checksum);
		}
		if (instance.localclient != 0L)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.localclient);
		}
		if (instance.position != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int num = stream.Position;
			Vector3Serialized.Serialize(stream, instance.position);
			int num2 = stream.Position - num;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span, 0);
		}
		if (instance.rotation != default(Vector3))
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.rotation);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rotation (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		if (instance.levelUrl == null)
		{
			throw new ArgumentNullException("levelUrl", "Required by proto specification.");
		}
		stream.WriteByte(74);
		ProtocolParser.WriteString(stream, instance.levelUrl);
		if (instance.recordedTime != 0L)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.recordedTime);
		}
		if (instance.length != 0L)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.length);
		}
		if (instance.fileStorage != null)
		{
			for (int i = 0; i < instance.fileStorage.Count; i++)
			{
				FileStorageCacheData instance2 = instance.fileStorage[i];
				stream.WriteByte(98);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int num5 = stream.Position;
				FileStorageCacheData.Serialize(stream, instance2);
				int val = stream.Position - num5;
				Span<byte> span3 = range3.GetSpan();
				int num6 = ProtocolParser.WriteUInt32((uint)val, span3, 0);
				if (num6 < 5)
				{
					span3[num6 - 1] |= 128;
					while (num6 < 4)
					{
						span3[num6++] = 128;
					}
					span3[4] = 0;
				}
			}
		}
		if (instance.nexus)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteBool(stream, instance.nexus);
		}
		if (instance.manifestCRC != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt32(stream, instance.manifestCRC);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (fileStorage != null)
		{
			for (int i = 0; i < fileStorage.Count; i++)
			{
				fileStorage[i]?.InspectUids(action);
			}
		}
	}
}
