using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class CargoPlane : IDisposable, Pool.IPooled, IProto<CargoPlane>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 startPos;

	[NonSerialized]
	public Vector3 endPos;

	[NonSerialized]
	public float secondsToTake;

	[NonSerialized]
	public float secondsTaken;

	[NonSerialized]
	public bool dropped;

	[NonSerialized]
	public Vector3 dropPosition;

	public static void ResetToPool(CargoPlane instance)
	{
		if (instance.ShouldPool)
		{
			instance.startPos = default(Vector3);
			instance.endPos = default(Vector3);
			instance.secondsToTake = 0f;
			instance.secondsTaken = 0f;
			instance.dropped = false;
			instance.dropPosition = default(Vector3);
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
			throw new Exception("Trying to dispose CargoPlane with ShouldPool set to false!");
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

	public void CopyTo(CargoPlane instance)
	{
		instance.startPos = startPos;
		instance.endPos = endPos;
		instance.secondsToTake = secondsToTake;
		instance.secondsTaken = secondsTaken;
		instance.dropped = dropped;
		instance.dropPosition = dropPosition;
	}

	public CargoPlane Copy()
	{
		CargoPlane cargoPlane = Pool.Get<CargoPlane>();
		CopyTo(cargoPlane);
		return cargoPlane;
	}

	public static CargoPlane Deserialize(BufferStream stream)
	{
		CargoPlane cargoPlane = Pool.Get<CargoPlane>();
		Deserialize(stream, cargoPlane, isDelta: false);
		return cargoPlane;
	}

	public static CargoPlane DeserializeLengthDelimited(BufferStream stream)
	{
		CargoPlane cargoPlane = Pool.Get<CargoPlane>();
		DeserializeLengthDelimited(stream, cargoPlane, isDelta: false);
		return cargoPlane;
	}

	public static CargoPlane DeserializeLength(BufferStream stream, int length)
	{
		CargoPlane cargoPlane = Pool.Get<CargoPlane>();
		DeserializeLength(stream, length, cargoPlane, isDelta: false);
		return cargoPlane;
	}

	public static CargoPlane Deserialize(byte[] buffer)
	{
		CargoPlane cargoPlane = Pool.Get<CargoPlane>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, cargoPlane, isDelta: false);
		return cargoPlane;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CargoPlane previous)
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

	public static CargoPlane Deserialize(BufferStream stream, CargoPlane instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CargoPlane");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startPos, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.endPos, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.secondsToTake = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.secondsTaken = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.dropped = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.dropPosition, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CargoPlane DeserializeLengthDelimited(BufferStream stream, CargoPlane instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CargoPlane");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startPos, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.endPos, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.secondsToTake = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.secondsTaken = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.dropped = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.dropPosition, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CargoPlane DeserializeLength(BufferStream stream, int length, CargoPlane instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CargoPlane");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startPos, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.endPos, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.secondsToTake = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.secondsTaken = ProtocolParser.ReadSingle(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				instance.dropped = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.dropPosition, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CargoPlane");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CargoPlane instance, CargoPlane previous)
	{
		if (instance.startPos != previous.startPos)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.startPos, previous.startPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field startPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.endPos != previous.endPos)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.endPos, previous.endPos);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field endPos (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.secondsToTake != previous.secondsToTake)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.secondsToTake);
		}
		if (instance.secondsTaken != previous.secondsTaken)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.secondsTaken);
		}
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.dropped);
		if (instance.dropPosition != previous.dropPosition)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.dropPosition, previous.dropPosition);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field dropPosition (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
	}

	public static void Serialize(BufferStream stream, CargoPlane instance)
	{
		if (instance.startPos != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.startPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field startPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.endPos != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.endPos);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field endPos (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.secondsToTake != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.secondsToTake);
		}
		if (instance.secondsTaken != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.secondsTaken);
		}
		if (instance.dropped)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.dropped);
		}
		if (instance.dropPosition != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.dropPosition);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field dropPosition (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
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
