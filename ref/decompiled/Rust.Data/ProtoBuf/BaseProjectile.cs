using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BaseProjectile : IDisposable, Pool.IPooled, IProto<BaseProjectile>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Magazine primaryMagazine;

	public static void ResetToPool(BaseProjectile instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.primaryMagazine != null)
			{
				instance.primaryMagazine.ResetToPool();
				instance.primaryMagazine = null;
			}
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
			throw new Exception("Trying to dispose BaseProjectile with ShouldPool set to false!");
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

	public void CopyTo(BaseProjectile instance)
	{
		if (primaryMagazine != null)
		{
			if (instance.primaryMagazine == null)
			{
				instance.primaryMagazine = primaryMagazine.Copy();
			}
			else
			{
				primaryMagazine.CopyTo(instance.primaryMagazine);
			}
		}
		else
		{
			instance.primaryMagazine = null;
		}
	}

	public BaseProjectile Copy()
	{
		BaseProjectile baseProjectile = Pool.Get<BaseProjectile>();
		CopyTo(baseProjectile);
		return baseProjectile;
	}

	public static BaseProjectile Deserialize(BufferStream stream)
	{
		BaseProjectile baseProjectile = Pool.Get<BaseProjectile>();
		Deserialize(stream, baseProjectile, isDelta: false);
		return baseProjectile;
	}

	public static BaseProjectile DeserializeLengthDelimited(BufferStream stream)
	{
		BaseProjectile baseProjectile = Pool.Get<BaseProjectile>();
		DeserializeLengthDelimited(stream, baseProjectile, isDelta: false);
		return baseProjectile;
	}

	public static BaseProjectile DeserializeLength(BufferStream stream, int length)
	{
		BaseProjectile baseProjectile = Pool.Get<BaseProjectile>();
		DeserializeLength(stream, length, baseProjectile, isDelta: false);
		return baseProjectile;
	}

	public static BaseProjectile Deserialize(byte[] buffer)
	{
		BaseProjectile baseProjectile = Pool.Get<BaseProjectile>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, baseProjectile, isDelta: false);
		return baseProjectile;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BaseProjectile previous)
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

	public static BaseProjectile Deserialize(BufferStream stream, BaseProjectile instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BaseProjectile");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseProjectile");
				if (instance.primaryMagazine == null)
				{
					instance.primaryMagazine = Magazine.DeserializeLengthDelimited(stream);
				}
				else
				{
					Magazine.DeserializeLengthDelimited(stream, instance.primaryMagazine, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseProjectile");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseProjectile");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BaseProjectile DeserializeLengthDelimited(BufferStream stream, BaseProjectile instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseProjectile");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseProjectile");
				if (instance.primaryMagazine == null)
				{
					instance.primaryMagazine = Magazine.DeserializeLengthDelimited(stream);
				}
				else
				{
					Magazine.DeserializeLengthDelimited(stream, instance.primaryMagazine, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseProjectile");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseProjectile");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static BaseProjectile DeserializeLength(BufferStream stream, int length, BaseProjectile instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BaseProjectile");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseProjectile");
				if (instance.primaryMagazine == null)
				{
					instance.primaryMagazine = Magazine.DeserializeLengthDelimited(stream);
				}
				else
				{
					Magazine.DeserializeLengthDelimited(stream, instance.primaryMagazine, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BaseProjectile");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BaseProjectile");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BaseProjectile instance, BaseProjectile previous)
	{
		if (instance.primaryMagazine != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Magazine.SerializeDelta(stream, instance.primaryMagazine, previous.primaryMagazine);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field primaryMagazine (ProtoBuf.Magazine)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, BaseProjectile instance)
	{
		if (instance.primaryMagazine != null)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Magazine.Serialize(stream, instance.primaryMagazine);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field primaryMagazine (ProtoBuf.Magazine)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		primaryMagazine?.InspectUids(action);
	}
}
