using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class GunWeaponMod : IDisposable, Pool.IPooled, IProto<GunWeaponMod>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int zoomLevel;

	public static void ResetToPool(GunWeaponMod instance)
	{
		if (instance.ShouldPool)
		{
			instance.zoomLevel = 0;
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
			throw new Exception("Trying to dispose GunWeaponMod with ShouldPool set to false!");
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

	public void CopyTo(GunWeaponMod instance)
	{
		instance.zoomLevel = zoomLevel;
	}

	public GunWeaponMod Copy()
	{
		GunWeaponMod gunWeaponMod = Pool.Get<GunWeaponMod>();
		CopyTo(gunWeaponMod);
		return gunWeaponMod;
	}

	public static GunWeaponMod Deserialize(BufferStream stream)
	{
		GunWeaponMod gunWeaponMod = Pool.Get<GunWeaponMod>();
		Deserialize(stream, gunWeaponMod, isDelta: false);
		return gunWeaponMod;
	}

	public static GunWeaponMod DeserializeLengthDelimited(BufferStream stream)
	{
		GunWeaponMod gunWeaponMod = Pool.Get<GunWeaponMod>();
		DeserializeLengthDelimited(stream, gunWeaponMod, isDelta: false);
		return gunWeaponMod;
	}

	public static GunWeaponMod DeserializeLength(BufferStream stream, int length)
	{
		GunWeaponMod gunWeaponMod = Pool.Get<GunWeaponMod>();
		DeserializeLength(stream, length, gunWeaponMod, isDelta: false);
		return gunWeaponMod;
	}

	public static GunWeaponMod Deserialize(byte[] buffer)
	{
		GunWeaponMod gunWeaponMod = Pool.Get<GunWeaponMod>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, gunWeaponMod, isDelta: false);
		return gunWeaponMod;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, GunWeaponMod previous)
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

	public static GunWeaponMod Deserialize(BufferStream stream, GunWeaponMod instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.GunWeaponMod");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GunWeaponMod");
				instance.zoomLevel = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GunWeaponMod");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GunWeaponMod");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static GunWeaponMod DeserializeLengthDelimited(BufferStream stream, GunWeaponMod instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.GunWeaponMod");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GunWeaponMod");
				instance.zoomLevel = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GunWeaponMod");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GunWeaponMod");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static GunWeaponMod DeserializeLength(BufferStream stream, int length, GunWeaponMod instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.GunWeaponMod");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GunWeaponMod");
				instance.zoomLevel = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GunWeaponMod");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GunWeaponMod");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, GunWeaponMod instance, GunWeaponMod previous)
	{
		if (instance.zoomLevel != previous.zoomLevel)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.zoomLevel);
		}
	}

	public static void Serialize(BufferStream stream, GunWeaponMod instance)
	{
		if (instance.zoomLevel != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.zoomLevel);
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
