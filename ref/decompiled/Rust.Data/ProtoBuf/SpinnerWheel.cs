using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class SpinnerWheel : IDisposable, Pool.IPooled, IProto<SpinnerWheel>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 spin;

	public static void ResetToPool(SpinnerWheel instance)
	{
		if (instance.ShouldPool)
		{
			instance.spin = default(Vector3);
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
			throw new Exception("Trying to dispose SpinnerWheel with ShouldPool set to false!");
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

	public void CopyTo(SpinnerWheel instance)
	{
		instance.spin = spin;
	}

	public SpinnerWheel Copy()
	{
		SpinnerWheel spinnerWheel = Pool.Get<SpinnerWheel>();
		CopyTo(spinnerWheel);
		return spinnerWheel;
	}

	public static SpinnerWheel Deserialize(BufferStream stream)
	{
		SpinnerWheel spinnerWheel = Pool.Get<SpinnerWheel>();
		Deserialize(stream, spinnerWheel, isDelta: false);
		return spinnerWheel;
	}

	public static SpinnerWheel DeserializeLengthDelimited(BufferStream stream)
	{
		SpinnerWheel spinnerWheel = Pool.Get<SpinnerWheel>();
		DeserializeLengthDelimited(stream, spinnerWheel, isDelta: false);
		return spinnerWheel;
	}

	public static SpinnerWheel DeserializeLength(BufferStream stream, int length)
	{
		SpinnerWheel spinnerWheel = Pool.Get<SpinnerWheel>();
		DeserializeLength(stream, length, spinnerWheel, isDelta: false);
		return spinnerWheel;
	}

	public static SpinnerWheel Deserialize(byte[] buffer)
	{
		SpinnerWheel spinnerWheel = Pool.Get<SpinnerWheel>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, spinnerWheel, isDelta: false);
		return spinnerWheel;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SpinnerWheel previous)
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

	public static SpinnerWheel Deserialize(BufferStream stream, SpinnerWheel instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SpinnerWheel");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpinnerWheel");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.spin, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpinnerWheel");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpinnerWheel");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpinnerWheel DeserializeLengthDelimited(BufferStream stream, SpinnerWheel instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SpinnerWheel");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpinnerWheel");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.spin, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpinnerWheel");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpinnerWheel");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static SpinnerWheel DeserializeLength(BufferStream stream, int length, SpinnerWheel instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SpinnerWheel");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpinnerWheel");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.spin, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SpinnerWheel");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SpinnerWheel");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SpinnerWheel instance, SpinnerWheel previous)
	{
		if (instance.spin != previous.spin)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.spin, previous.spin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, SpinnerWheel instance)
	{
		if (instance.spin != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.spin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field spin (UnityEngine.Vector3)");
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
	}
}
