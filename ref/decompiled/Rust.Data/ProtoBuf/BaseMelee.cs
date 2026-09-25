using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BaseMelee : IDisposable, Pool.IPooled, IProto<BaseMelee>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool canThrowAsProjectile;

	[NonSerialized]
	public bool onlyThrowAsProjectile;

	public static void ResetToPool(BaseMelee instance)
	{
		if (instance.ShouldPool)
		{
			instance.canThrowAsProjectile = false;
			instance.onlyThrowAsProjectile = false;
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
			throw new Exception("Trying to dispose BaseMelee with ShouldPool set to false!");
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

	public void CopyTo(BaseMelee instance)
	{
		instance.canThrowAsProjectile = canThrowAsProjectile;
		instance.onlyThrowAsProjectile = onlyThrowAsProjectile;
	}

	public BaseMelee Copy()
	{
		BaseMelee baseMelee = Pool.Get<BaseMelee>();
		CopyTo(baseMelee);
		return baseMelee;
	}

	public static BaseMelee Deserialize(BufferStream stream)
	{
		BaseMelee baseMelee = Pool.Get<BaseMelee>();
		Deserialize(stream, baseMelee, isDelta: false);
		return baseMelee;
	}

	public static BaseMelee DeserializeLengthDelimited(BufferStream stream)
	{
		BaseMelee baseMelee = Pool.Get<BaseMelee>();
		DeserializeLengthDelimited(stream, baseMelee, isDelta: false);
		return baseMelee;
	}

	public static BaseMelee DeserializeLength(BufferStream stream, int length)
	{
		BaseMelee baseMelee = Pool.Get<BaseMelee>();
		DeserializeLength(stream, length, baseMelee, isDelta: false);
		return baseMelee;
	}

	public static BaseMelee Deserialize(byte[] buffer)
	{
		BaseMelee baseMelee = Pool.Get<BaseMelee>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, baseMelee, isDelta: false);
		return baseMelee;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BaseMelee previous)
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

	public static BaseMelee Deserialize(BufferStream stream, BaseMelee instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BaseMelee");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				instance.canThrowAsProjectile = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				instance.onlyThrowAsProjectile = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BaseMelee DeserializeLengthDelimited(BufferStream stream, BaseMelee instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseMelee");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				instance.canThrowAsProjectile = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				instance.onlyThrowAsProjectile = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BaseMelee DeserializeLength(BufferStream stream, int length, BaseMelee instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseMelee");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				instance.canThrowAsProjectile = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				instance.onlyThrowAsProjectile = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseMelee");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BaseMelee instance, BaseMelee previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.canThrowAsProjectile);
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.onlyThrowAsProjectile);
	}

	public static void Serialize(BufferStream stream, BaseMelee instance)
	{
		if (instance.canThrowAsProjectile)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.canThrowAsProjectile);
		}
		if (instance.onlyThrowAsProjectile)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.onlyThrowAsProjectile);
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
