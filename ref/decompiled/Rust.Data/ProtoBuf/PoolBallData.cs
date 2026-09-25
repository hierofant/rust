using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PoolBallData : IDisposable, Pool.IPooled, IProto<PoolBallData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 position;

	[NonSerialized]
	public Vector3 velocity;

	[NonSerialized]
	public bool pocketed;

	public static void ResetToPool(PoolBallData instance)
	{
		if (instance.ShouldPool)
		{
			instance.position = default(Vector3);
			instance.velocity = default(Vector3);
			instance.pocketed = false;
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
			throw new Exception("Trying to dispose PoolBallData with ShouldPool set to false!");
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

	public void CopyTo(PoolBallData instance)
	{
		instance.position = position;
		instance.velocity = velocity;
		instance.pocketed = pocketed;
	}

	public PoolBallData Copy()
	{
		PoolBallData poolBallData = Pool.Get<PoolBallData>();
		CopyTo(poolBallData);
		return poolBallData;
	}

	public static PoolBallData Deserialize(BufferStream stream)
	{
		PoolBallData poolBallData = Pool.Get<PoolBallData>();
		Deserialize(stream, poolBallData, isDelta: false);
		return poolBallData;
	}

	public static PoolBallData DeserializeLengthDelimited(BufferStream stream)
	{
		PoolBallData poolBallData = Pool.Get<PoolBallData>();
		DeserializeLengthDelimited(stream, poolBallData, isDelta: false);
		return poolBallData;
	}

	public static PoolBallData DeserializeLength(BufferStream stream, int length)
	{
		PoolBallData poolBallData = Pool.Get<PoolBallData>();
		DeserializeLength(stream, length, poolBallData, isDelta: false);
		return poolBallData;
	}

	public static PoolBallData Deserialize(byte[] buffer)
	{
		PoolBallData poolBallData = Pool.Get<PoolBallData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, poolBallData, isDelta: false);
		return poolBallData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PoolBallData previous)
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

	public static PoolBallData Deserialize(BufferStream stream, PoolBallData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PoolBallData");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.velocity, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				instance.pocketed = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PoolBallData DeserializeLengthDelimited(BufferStream stream, PoolBallData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PoolBallData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.velocity, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				instance.pocketed = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PoolBallData DeserializeLength(BufferStream stream, int length, PoolBallData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PoolBallData");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.velocity, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				instance.pocketed = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PoolBallData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PoolBallData instance, PoolBallData previous)
	{
		if (instance.position != previous.position)
		{
			stream.WriteByte(10);
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
		if (instance.velocity != previous.velocity)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.velocity, previous.velocity);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field velocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.pocketed);
	}

	public static void Serialize(BufferStream stream, PoolBallData instance)
	{
		if (instance.position != default(Vector3))
		{
			stream.WriteByte(10);
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
		if (instance.velocity != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.velocity);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field velocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		if (instance.pocketed)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.pocketed);
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
