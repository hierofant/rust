using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class CreateBuilding : IDisposable, Pool.IPooled, IProto<CreateBuilding>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId entity;

	[NonSerialized]
	public uint socket;

	[NonSerialized]
	public bool onterrain;

	[NonSerialized]
	public Vector3 position;

	[NonSerialized]
	public Vector3 normal;

	[NonSerialized]
	public Ray ray;

	[NonSerialized]
	public uint blockID;

	[NonSerialized]
	public Vector3 rotation;

	[NonSerialized]
	public bool isHoldingShift;

	[NonSerialized]
	public int setToGrade;

	[NonSerialized]
	public bool isSnapped;

	[NonSerialized]
	public Vector3 snappedPosition;

	[NonSerialized]
	public Vector3 snappedRotation;

	[NonSerialized]
	public int snappingMode;

	public static void ResetToPool(CreateBuilding instance)
	{
		if (instance.ShouldPool)
		{
			instance.entity = default(NetworkableId);
			instance.socket = 0u;
			instance.onterrain = false;
			instance.position = default(Vector3);
			instance.normal = default(Vector3);
			instance.ray = default(Ray);
			instance.blockID = 0u;
			instance.rotation = default(Vector3);
			instance.isHoldingShift = false;
			instance.setToGrade = 0;
			instance.isSnapped = false;
			instance.snappedPosition = default(Vector3);
			instance.snappedRotation = default(Vector3);
			instance.snappingMode = 0;
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
			throw new Exception("Trying to dispose CreateBuilding with ShouldPool set to false!");
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

	public void CopyTo(CreateBuilding instance)
	{
		instance.entity = entity;
		instance.socket = socket;
		instance.onterrain = onterrain;
		instance.position = position;
		instance.normal = normal;
		instance.ray = ray;
		instance.blockID = blockID;
		instance.rotation = rotation;
		instance.isHoldingShift = isHoldingShift;
		instance.setToGrade = setToGrade;
		instance.isSnapped = isSnapped;
		instance.snappedPosition = snappedPosition;
		instance.snappedRotation = snappedRotation;
		instance.snappingMode = snappingMode;
	}

	public CreateBuilding Copy()
	{
		CreateBuilding createBuilding = Pool.Get<CreateBuilding>();
		CopyTo(createBuilding);
		return createBuilding;
	}

	public static CreateBuilding Deserialize(BufferStream stream)
	{
		CreateBuilding createBuilding = Pool.Get<CreateBuilding>();
		Deserialize(stream, createBuilding, isDelta: false);
		return createBuilding;
	}

	public static CreateBuilding DeserializeLengthDelimited(BufferStream stream)
	{
		CreateBuilding createBuilding = Pool.Get<CreateBuilding>();
		DeserializeLengthDelimited(stream, createBuilding, isDelta: false);
		return createBuilding;
	}

	public static CreateBuilding DeserializeLength(BufferStream stream, int length)
	{
		CreateBuilding createBuilding = Pool.Get<CreateBuilding>();
		DeserializeLength(stream, length, createBuilding, isDelta: false);
		return createBuilding;
	}

	public static CreateBuilding Deserialize(byte[] buffer)
	{
		CreateBuilding createBuilding = Pool.Get<CreateBuilding>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, createBuilding, isDelta: false);
		return createBuilding;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CreateBuilding previous)
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

	public static CreateBuilding Deserialize(BufferStream stream, CreateBuilding instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CreateBuilding");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.entity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.socket = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.onterrain = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				RaySerialized.DeserializeLengthDelimited(stream, ref instance.ray, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.blockID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.isHoldingShift = ProtocolParser.ReadBool(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.setToGrade = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.isSnapped = ProtocolParser.ReadBool(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.snappedPosition, isDelta);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.snappedRotation, isDelta);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.snappingMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CreateBuilding DeserializeLengthDelimited(BufferStream stream, CreateBuilding instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CreateBuilding");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.entity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.socket = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.onterrain = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				RaySerialized.DeserializeLengthDelimited(stream, ref instance.ray, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.blockID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.isHoldingShift = ProtocolParser.ReadBool(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.setToGrade = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.isSnapped = ProtocolParser.ReadBool(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.snappedPosition, isDelta);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.snappedRotation, isDelta);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.snappingMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CreateBuilding DeserializeLength(BufferStream stream, int length, CreateBuilding instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.CreateBuilding");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.entity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.socket = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.onterrain = ProtocolParser.ReadBool(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				RaySerialized.DeserializeLengthDelimited(stream, ref instance.ray, isDelta);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.blockID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.isHoldingShift = ProtocolParser.ReadBool(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.setToGrade = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.isSnapped = ProtocolParser.ReadBool(stream);
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.snappedPosition, isDelta);
				continue;
			case 106:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.snappedRotation, isDelta);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				instance.snappingMode = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CreateBuilding");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CreateBuilding instance, CreateBuilding previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.entity.Value);
		if (instance.socket != previous.socket)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.socket);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.onterrain);
		if (instance.position != previous.position)
		{
			stream.WriteByte(34);
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
		if (instance.normal != previous.normal)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.normal, previous.normal);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field normal (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		stream.WriteByte(50);
		BufferStream.RangeHandle range3 = stream.GetRange(1);
		int num5 = stream.Position;
		RaySerialized.SerializeDelta(stream, instance.ray, previous.ray);
		int num6 = stream.Position - num5;
		if (num6 > 127)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ray (UnityEngine.Ray)");
		}
		Span<byte> span3 = range3.GetSpan();
		ProtocolParser.WriteUInt32((uint)num6, span3, 0);
		if (instance.blockID != previous.blockID)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt32(stream, instance.blockID);
		}
		if (instance.rotation != previous.rotation)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int num7 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.rotation, previous.rotation);
			int num8 = stream.Position - num7;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rotation (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span4, 0);
		}
		stream.WriteByte(72);
		ProtocolParser.WriteBool(stream, instance.isHoldingShift);
		if (instance.setToGrade != previous.setToGrade)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.setToGrade);
		}
		stream.WriteByte(88);
		ProtocolParser.WriteBool(stream, instance.isSnapped);
		if (instance.snappedPosition != previous.snappedPosition)
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int num9 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.snappedPosition, previous.snappedPosition);
			int num10 = stream.Position - num9;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field snappedPosition (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span5, 0);
		}
		if (instance.snappedRotation != previous.snappedRotation)
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int num11 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.snappedRotation, previous.snappedRotation);
			int num12 = stream.Position - num11;
			if (num12 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field snappedRotation (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num12, span6, 0);
		}
		if (instance.snappingMode != previous.snappingMode)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.snappingMode);
		}
	}

	public static void Serialize(BufferStream stream, CreateBuilding instance)
	{
		if (instance.entity != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.entity.Value);
		}
		if (instance.socket != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.socket);
		}
		if (instance.onterrain)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.onterrain);
		}
		if (instance.position != default(Vector3))
		{
			stream.WriteByte(34);
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
		if (instance.normal != default(Vector3))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int num3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.normal);
			int num4 = stream.Position - num3;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field normal (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span2, 0);
		}
		if (instance.ray.origin != default(Vector3) && instance.ray.direction != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int num5 = stream.Position;
			RaySerialized.Serialize(stream, instance.ray);
			int num6 = stream.Position - num5;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ray (UnityEngine.Ray)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span3, 0);
		}
		if (instance.blockID != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt32(stream, instance.blockID);
		}
		if (instance.rotation != default(Vector3))
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int num7 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.rotation);
			int num8 = stream.Position - num7;
			if (num8 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rotation (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num8, span4, 0);
		}
		if (instance.isHoldingShift)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteBool(stream, instance.isHoldingShift);
		}
		if (instance.setToGrade != 0)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.setToGrade);
		}
		if (instance.isSnapped)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.isSnapped);
		}
		if (instance.snappedPosition != default(Vector3))
		{
			stream.WriteByte(98);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int num9 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.snappedPosition);
			int num10 = stream.Position - num9;
			if (num10 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field snappedPosition (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num10, span5, 0);
		}
		if (instance.snappedRotation != default(Vector3))
		{
			stream.WriteByte(106);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int num11 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.snappedRotation);
			int num12 = stream.Position - num11;
			if (num12 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field snappedRotation (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num12, span6, 0);
		}
		if (instance.snappingMode != 0)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.snappingMode);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entity.Value);
	}
}
