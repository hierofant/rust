using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class FileStorageCacheData : IDisposable, Pool.IPooled, IProto<FileStorageCacheData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public byte[] data;

	[NonSerialized]
	public NetworkableId entityId;

	[NonSerialized]
	public uint numId;

	[NonSerialized]
	public uint crc;

	public static void ResetToPool(FileStorageCacheData instance)
	{
		if (instance.ShouldPool)
		{
			instance.data = null;
			instance.entityId = default(NetworkableId);
			instance.numId = 0u;
			instance.crc = 0u;
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
			throw new Exception("Trying to dispose FileStorageCacheData with ShouldPool set to false!");
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

	public void CopyTo(FileStorageCacheData instance)
	{
		if (data == null)
		{
			instance.data = null;
		}
		else
		{
			instance.data = new byte[data.Length];
			Array.Copy(data, instance.data, instance.data.Length);
		}
		instance.entityId = entityId;
		instance.numId = numId;
		instance.crc = crc;
	}

	public FileStorageCacheData Copy()
	{
		FileStorageCacheData fileStorageCacheData = Pool.Get<FileStorageCacheData>();
		CopyTo(fileStorageCacheData);
		return fileStorageCacheData;
	}

	public static FileStorageCacheData Deserialize(BufferStream stream)
	{
		FileStorageCacheData fileStorageCacheData = Pool.Get<FileStorageCacheData>();
		Deserialize(stream, fileStorageCacheData, isDelta: false);
		return fileStorageCacheData;
	}

	public static FileStorageCacheData DeserializeLengthDelimited(BufferStream stream)
	{
		FileStorageCacheData fileStorageCacheData = Pool.Get<FileStorageCacheData>();
		DeserializeLengthDelimited(stream, fileStorageCacheData, isDelta: false);
		return fileStorageCacheData;
	}

	public static FileStorageCacheData DeserializeLength(BufferStream stream, int length)
	{
		FileStorageCacheData fileStorageCacheData = Pool.Get<FileStorageCacheData>();
		DeserializeLength(stream, length, fileStorageCacheData, isDelta: false);
		return fileStorageCacheData;
	}

	public static FileStorageCacheData Deserialize(byte[] buffer)
	{
		FileStorageCacheData fileStorageCacheData = Pool.Get<FileStorageCacheData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, fileStorageCacheData, isDelta: false);
		return fileStorageCacheData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, FileStorageCacheData previous)
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

	public static FileStorageCacheData Deserialize(BufferStream stream, FileStorageCacheData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.FileStorageCacheData");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.data = ProtocolParser.ReadBytes(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.numId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.crc = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FileStorageCacheData DeserializeLengthDelimited(BufferStream stream, FileStorageCacheData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.FileStorageCacheData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.data = ProtocolParser.ReadBytes(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.numId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.crc = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FileStorageCacheData DeserializeLength(BufferStream stream, int length, FileStorageCacheData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.FileStorageCacheData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.data = ProtocolParser.ReadBytes(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.numId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				instance.crc = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.FileStorageCacheData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, FileStorageCacheData instance, FileStorageCacheData previous)
	{
		if (instance.data == null)
		{
			throw new ArgumentNullException("data", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteBytes(stream, instance.data);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		if (instance.numId != previous.numId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.numId);
		}
		if (instance.crc != previous.crc)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.crc);
		}
	}

	public static void Serialize(BufferStream stream, FileStorageCacheData instance)
	{
		if (instance.data == null)
		{
			throw new ArgumentNullException("data", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteBytes(stream, instance.data);
		if (instance.entityId != default(NetworkableId))
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		}
		if (instance.numId != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.numId);
		}
		if (instance.crc != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.crc);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entityId.Value);
	}
}
