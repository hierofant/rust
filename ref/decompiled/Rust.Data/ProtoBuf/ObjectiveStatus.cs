using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class ObjectiveStatus : IDisposable, Pool.IPooled, IProto<ObjectiveStatus>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public bool started;

	[NonSerialized]
	public bool completed;

	[NonSerialized]
	public bool failed;

	[NonSerialized]
	public float progressCurrent;

	[NonSerialized]
	public float progressTarget;

	[NonSerialized]
	public Vector3 worldLocation;

	[NonSerialized]
	public bool softCompleted;

	[NonSerialized]
	public bool blockReset;

	public static void ResetToPool(ObjectiveStatus instance)
	{
		if (instance.ShouldPool)
		{
			instance.started = false;
			instance.completed = false;
			instance.failed = false;
			instance.progressCurrent = 0f;
			instance.progressTarget = 0f;
			instance.worldLocation = default(Vector3);
			instance.softCompleted = false;
			instance.blockReset = false;
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
			throw new Exception("Trying to dispose ObjectiveStatus with ShouldPool set to false!");
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

	public void CopyTo(ObjectiveStatus instance)
	{
		instance.started = started;
		instance.completed = completed;
		instance.failed = failed;
		instance.progressCurrent = progressCurrent;
		instance.progressTarget = progressTarget;
		instance.worldLocation = worldLocation;
		instance.softCompleted = softCompleted;
		instance.blockReset = blockReset;
	}

	public ObjectiveStatus Copy()
	{
		ObjectiveStatus objectiveStatus = Pool.Get<ObjectiveStatus>();
		CopyTo(objectiveStatus);
		return objectiveStatus;
	}

	public static ObjectiveStatus Deserialize(BufferStream stream)
	{
		ObjectiveStatus objectiveStatus = Pool.Get<ObjectiveStatus>();
		Deserialize(stream, objectiveStatus, isDelta: false);
		return objectiveStatus;
	}

	public static ObjectiveStatus DeserializeLengthDelimited(BufferStream stream)
	{
		ObjectiveStatus objectiveStatus = Pool.Get<ObjectiveStatus>();
		DeserializeLengthDelimited(stream, objectiveStatus, isDelta: false);
		return objectiveStatus;
	}

	public static ObjectiveStatus DeserializeLength(BufferStream stream, int length)
	{
		ObjectiveStatus objectiveStatus = Pool.Get<ObjectiveStatus>();
		DeserializeLength(stream, length, objectiveStatus, isDelta: false);
		return objectiveStatus;
	}

	public static ObjectiveStatus Deserialize(byte[] buffer)
	{
		ObjectiveStatus objectiveStatus = Pool.Get<ObjectiveStatus>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, objectiveStatus, isDelta: false);
		return objectiveStatus;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ObjectiveStatus previous)
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

	public static ObjectiveStatus Deserialize(BufferStream stream, ObjectiveStatus instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ObjectiveStatus");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.started = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.completed = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.failed = ProtocolParser.ReadBool(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.progressCurrent = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.progressTarget = ProtocolParser.ReadSingle(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldLocation, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.softCompleted = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.blockReset = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ObjectiveStatus DeserializeLengthDelimited(BufferStream stream, ObjectiveStatus instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ObjectiveStatus");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.started = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.completed = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.failed = ProtocolParser.ReadBool(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.progressCurrent = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.progressTarget = ProtocolParser.ReadSingle(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldLocation, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.softCompleted = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.blockReset = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ObjectiveStatus DeserializeLength(BufferStream stream, int length, ObjectiveStatus instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ObjectiveStatus");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.started = ProtocolParser.ReadBool(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.completed = ProtocolParser.ReadBool(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.failed = ProtocolParser.ReadBool(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.progressCurrent = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.progressTarget = ProtocolParser.ReadSingle(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldLocation, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.softCompleted = ProtocolParser.ReadBool(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				instance.blockReset = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ObjectiveStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ObjectiveStatus instance, ObjectiveStatus previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteBool(stream, instance.started);
		stream.WriteByte(16);
		ProtocolParser.WriteBool(stream, instance.completed);
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.failed);
		if (instance.progressCurrent != previous.progressCurrent)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.progressCurrent);
		}
		if (instance.progressTarget != previous.progressTarget)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.progressTarget);
		}
		if (instance.worldLocation != previous.worldLocation)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.worldLocation, previous.worldLocation);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field worldLocation (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.softCompleted);
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.blockReset);
	}

	public static void Serialize(BufferStream stream, ObjectiveStatus instance)
	{
		if (instance.started)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteBool(stream, instance.started);
		}
		if (instance.completed)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteBool(stream, instance.completed);
		}
		if (instance.failed)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.failed);
		}
		if (instance.progressCurrent != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.progressCurrent);
		}
		if (instance.progressTarget != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.progressTarget);
		}
		if (instance.worldLocation != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.worldLocation);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field worldLocation (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.softCompleted)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.softCompleted);
		}
		if (instance.blockReset)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.blockReset);
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
