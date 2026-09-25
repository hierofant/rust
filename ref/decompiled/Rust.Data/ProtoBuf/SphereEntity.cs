using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SphereEntity : IDisposable, Pool.IPooled, IProto<SphereEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float radius;

	public static void ResetToPool(SphereEntity instance)
	{
		if (instance.ShouldPool)
		{
			instance.radius = 0f;
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
			throw new Exception("Trying to dispose SphereEntity with ShouldPool set to false!");
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

	public void CopyTo(SphereEntity instance)
	{
		instance.radius = radius;
	}

	public SphereEntity Copy()
	{
		SphereEntity sphereEntity = Pool.Get<SphereEntity>();
		CopyTo(sphereEntity);
		return sphereEntity;
	}

	public static SphereEntity Deserialize(BufferStream stream)
	{
		SphereEntity sphereEntity = Pool.Get<SphereEntity>();
		Deserialize(stream, sphereEntity, isDelta: false);
		return sphereEntity;
	}

	public static SphereEntity DeserializeLengthDelimited(BufferStream stream)
	{
		SphereEntity sphereEntity = Pool.Get<SphereEntity>();
		DeserializeLengthDelimited(stream, sphereEntity, isDelta: false);
		return sphereEntity;
	}

	public static SphereEntity DeserializeLength(BufferStream stream, int length)
	{
		SphereEntity sphereEntity = Pool.Get<SphereEntity>();
		DeserializeLength(stream, length, sphereEntity, isDelta: false);
		return sphereEntity;
	}

	public static SphereEntity Deserialize(byte[] buffer)
	{
		SphereEntity sphereEntity = Pool.Get<SphereEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sphereEntity, isDelta: false);
		return sphereEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SphereEntity previous)
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

	public static SphereEntity Deserialize(BufferStream stream, SphereEntity instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SphereEntity");
			if (num == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SphereEntity");
				instance.radius = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SphereEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SphereEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SphereEntity DeserializeLengthDelimited(BufferStream stream, SphereEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SphereEntity");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SphereEntity");
				instance.radius = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SphereEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SphereEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SphereEntity DeserializeLength(BufferStream stream, int length, SphereEntity instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SphereEntity");
			if (num2 == 13)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SphereEntity");
				instance.radius = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SphereEntity");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SphereEntity");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SphereEntity instance, SphereEntity previous)
	{
		if (instance.radius != previous.radius)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.radius);
		}
	}

	public static void Serialize(BufferStream stream, SphereEntity instance)
	{
		if (instance.radius != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.radius);
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
