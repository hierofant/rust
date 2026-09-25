using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class SatelliteCrash : IDisposable, Pool.IPooled, IProto<SatelliteCrash>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float satelliteMass;

	[NonSerialized]
	public bool isDescending;

	[NonSerialized]
	public Vector3 descentStartPos;

	[NonSerialized]
	public Vector3 descentEndPos;

	[NonSerialized]
	public float descentSecondsToTake;

	[NonSerialized]
	public float descentSecondsTaken;

	[NonSerialized]
	public Vector3 crashTarget;

	public static void ResetToPool(SatelliteCrash instance)
	{
		if (instance.ShouldPool)
		{
			instance.satelliteMass = 0f;
			instance.isDescending = false;
			instance.descentStartPos = default(Vector3);
			instance.descentEndPos = default(Vector3);
			instance.descentSecondsToTake = 0f;
			instance.descentSecondsTaken = 0f;
			instance.crashTarget = default(Vector3);
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
			throw new Exception("Trying to dispose SatelliteCrash with ShouldPool set to false!");
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

	public void CopyTo(SatelliteCrash instance)
	{
		instance.satelliteMass = satelliteMass;
		instance.isDescending = isDescending;
		instance.descentStartPos = descentStartPos;
		instance.descentEndPos = descentEndPos;
		instance.descentSecondsToTake = descentSecondsToTake;
		instance.descentSecondsTaken = descentSecondsTaken;
		instance.crashTarget = crashTarget;
	}

	public SatelliteCrash Copy()
	{
		SatelliteCrash satelliteCrash = Pool.Get<SatelliteCrash>();
		CopyTo(satelliteCrash);
		return satelliteCrash;
	}

	public static SatelliteCrash Deserialize(BufferStream stream)
	{
		SatelliteCrash satelliteCrash = Pool.Get<SatelliteCrash>();
		Deserialize(stream, satelliteCrash, isDelta: false);
		return satelliteCrash;
	}

	public static SatelliteCrash DeserializeLengthDelimited(BufferStream stream)
	{
		SatelliteCrash satelliteCrash = Pool.Get<SatelliteCrash>();
		DeserializeLengthDelimited(stream, satelliteCrash, isDelta: false);
		return satelliteCrash;
	}

	public static SatelliteCrash DeserializeLength(BufferStream stream, int length)
	{
		SatelliteCrash satelliteCrash = Pool.Get<SatelliteCrash>();
		DeserializeLength(stream, length, satelliteCrash, isDelta: false);
		return satelliteCrash;
	}

	public static SatelliteCrash Deserialize(byte[] buffer)
	{
		SatelliteCrash satelliteCrash = Pool.Get<SatelliteCrash>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, satelliteCrash, isDelta: false);
		return satelliteCrash;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SatelliteCrash previous)
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

	public static SatelliteCrash Deserialize(BufferStream stream, SatelliteCrash instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SatelliteCrash");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.satelliteMass = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.isDescending = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.descentStartPos, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.descentEndPos, isDelta);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.descentSecondsToTake = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.descentSecondsTaken = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.crashTarget, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SatelliteCrash DeserializeLengthDelimited(BufferStream stream, SatelliteCrash instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SatelliteCrash");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.satelliteMass = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.isDescending = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.descentStartPos, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.descentEndPos, isDelta);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.descentSecondsToTake = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.descentSecondsTaken = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.crashTarget, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SatelliteCrash DeserializeLength(BufferStream stream, int length, SatelliteCrash instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SatelliteCrash");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.satelliteMass = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.isDescending = ProtocolParser.ReadBool(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.descentStartPos, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.descentEndPos, isDelta);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.descentSecondsToTake = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				instance.descentSecondsTaken = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.crashTarget, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SatelliteCrash");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SatelliteCrash instance, SatelliteCrash previous)
	{
		if (instance.satelliteMass != previous.satelliteMass)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.satelliteMass);
		}
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.isDescending);
		if (instance.descentStartPos != previous.descentStartPos)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.descentStartPos, previous.descentStartPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field descentStartPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.descentEndPos != previous.descentEndPos)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.descentEndPos, previous.descentEndPos);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field descentEndPos (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.descentSecondsToTake != previous.descentSecondsToTake)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.descentSecondsToTake);
		}
		if (instance.descentSecondsTaken != previous.descentSecondsTaken)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.descentSecondsTaken);
		}
		if (instance.crashTarget != previous.crashTarget)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.crashTarget, previous.crashTarget);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field crashTarget (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
	}

	public static void Serialize(BufferStream stream, SatelliteCrash instance)
	{
		if (instance.satelliteMass != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.satelliteMass);
		}
		if (instance.isDescending)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.isDescending);
		}
		if (instance.descentStartPos != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.descentStartPos);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field descentStartPos (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.descentEndPos != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.descentEndPos);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field descentEndPos (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.descentSecondsToTake != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.descentSecondsToTake);
		}
		if (instance.descentSecondsTaken != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.descentSecondsTaken);
		}
		if (instance.crashTarget != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.crashTarget);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field crashTarget (UnityEngine.Vector3)");
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
