using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanManager : IDisposable, Pool.IPooled, IProto<ClanManager>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string backendType;

	public static void ResetToPool(ClanManager instance)
	{
		if (instance.ShouldPool)
		{
			instance.backendType = string.Empty;
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
			throw new Exception("Trying to dispose ClanManager with ShouldPool set to false!");
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

	public void CopyTo(ClanManager instance)
	{
		instance.backendType = backendType;
	}

	public ClanManager Copy()
	{
		ClanManager clanManager = Pool.Get<ClanManager>();
		CopyTo(clanManager);
		return clanManager;
	}

	public static ClanManager Deserialize(BufferStream stream)
	{
		ClanManager clanManager = Pool.Get<ClanManager>();
		Deserialize(stream, clanManager, isDelta: false);
		return clanManager;
	}

	public static ClanManager DeserializeLengthDelimited(BufferStream stream)
	{
		ClanManager clanManager = Pool.Get<ClanManager>();
		DeserializeLengthDelimited(stream, clanManager, isDelta: false);
		return clanManager;
	}

	public static ClanManager DeserializeLength(BufferStream stream, int length)
	{
		ClanManager clanManager = Pool.Get<ClanManager>();
		DeserializeLength(stream, length, clanManager, isDelta: false);
		return clanManager;
	}

	public static ClanManager Deserialize(byte[] buffer)
	{
		ClanManager clanManager = Pool.Get<ClanManager>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanManager, isDelta: false);
		return clanManager;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanManager previous)
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

	public static ClanManager Deserialize(BufferStream stream, ClanManager instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ClanManager");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanManager");
				instance.backendType = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanManager DeserializeLengthDelimited(BufferStream stream, ClanManager instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanManager");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanManager");
				instance.backendType = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanManager DeserializeLength(BufferStream stream, int length, ClanManager instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanManager");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanManager");
				instance.backendType = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanManager");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanManager");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanManager instance, ClanManager previous)
	{
		if (instance.backendType != previous.backendType)
		{
			if (instance.backendType == null)
			{
				throw new ArgumentNullException("backendType", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.backendType);
		}
	}

	public static void Serialize(BufferStream stream, ClanManager instance)
	{
		if (instance.backendType == null)
		{
			throw new ArgumentNullException("backendType", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.backendType);
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
