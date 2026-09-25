using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class OwnerInfo : IDisposable, Pool.IPooled, IProto<OwnerInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong steamid;

	public static void ResetToPool(OwnerInfo instance)
	{
		if (instance.ShouldPool)
		{
			instance.steamid = 0uL;
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
			throw new Exception("Trying to dispose OwnerInfo with ShouldPool set to false!");
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

	public void CopyTo(OwnerInfo instance)
	{
		instance.steamid = steamid;
	}

	public OwnerInfo Copy()
	{
		OwnerInfo ownerInfo = Pool.Get<OwnerInfo>();
		CopyTo(ownerInfo);
		return ownerInfo;
	}

	public static OwnerInfo Deserialize(BufferStream stream)
	{
		OwnerInfo ownerInfo = Pool.Get<OwnerInfo>();
		Deserialize(stream, ownerInfo, isDelta: false);
		return ownerInfo;
	}

	public static OwnerInfo DeserializeLengthDelimited(BufferStream stream)
	{
		OwnerInfo ownerInfo = Pool.Get<OwnerInfo>();
		DeserializeLengthDelimited(stream, ownerInfo, isDelta: false);
		return ownerInfo;
	}

	public static OwnerInfo DeserializeLength(BufferStream stream, int length)
	{
		OwnerInfo ownerInfo = Pool.Get<OwnerInfo>();
		DeserializeLength(stream, length, ownerInfo, isDelta: false);
		return ownerInfo;
	}

	public static OwnerInfo Deserialize(byte[] buffer)
	{
		OwnerInfo ownerInfo = Pool.Get<OwnerInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ownerInfo, isDelta: false);
		return ownerInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, OwnerInfo previous)
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

	public static OwnerInfo Deserialize(BufferStream stream, OwnerInfo instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.OwnerInfo");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OwnerInfo");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OwnerInfo");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OwnerInfo");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static OwnerInfo DeserializeLengthDelimited(BufferStream stream, OwnerInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.OwnerInfo");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OwnerInfo");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OwnerInfo");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OwnerInfo");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static OwnerInfo DeserializeLength(BufferStream stream, int length, OwnerInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.OwnerInfo");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OwnerInfo");
				instance.steamid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.OwnerInfo");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.OwnerInfo");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, OwnerInfo instance, OwnerInfo previous)
	{
		if (instance.steamid != previous.steamid)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamid);
		}
	}

	public static void Serialize(BufferStream stream, OwnerInfo instance)
	{
		if (instance.steamid != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamid);
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
