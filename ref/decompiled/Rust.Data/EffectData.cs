using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public class EffectData : IDisposable, Pool.IPooled, IProto<EffectData>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint type;

	[NonSerialized]
	public uint pooledstringid;

	[NonSerialized]
	public int number;

	[NonSerialized]
	public Vector3 origin;

	[NonSerialized]
	public Vector3 normal;

	[NonSerialized]
	public float scale;

	[NonSerialized]
	public NetworkableId entity;

	[NonSerialized]
	public uint bone;

	[NonSerialized]
	public ulong source;

	[NonSerialized]
	public float distanceOverride;

	[NonSerialized]
	public bool ignoreMaxSpawnDistance;

	[NonSerialized]
	public NetworkableId sourceEntity;

	public static void ResetToPool(EffectData instance)
	{
		if (instance.ShouldPool)
		{
			instance.type = 0u;
			instance.pooledstringid = 0u;
			instance.number = 0;
			instance.origin = default(Vector3);
			instance.normal = default(Vector3);
			instance.scale = 0f;
			instance.entity = default(NetworkableId);
			instance.bone = 0u;
			instance.source = 0uL;
			instance.distanceOverride = 0f;
			instance.ignoreMaxSpawnDistance = false;
			instance.sourceEntity = default(NetworkableId);
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
			throw new Exception("Trying to dispose EffectData with ShouldPool set to false!");
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

	public void CopyTo(EffectData instance)
	{
		instance.type = type;
		instance.pooledstringid = pooledstringid;
		instance.number = number;
		instance.origin = origin;
		instance.normal = normal;
		instance.scale = scale;
		instance.entity = entity;
		instance.bone = bone;
		instance.source = source;
		instance.distanceOverride = distanceOverride;
		instance.ignoreMaxSpawnDistance = ignoreMaxSpawnDistance;
		instance.sourceEntity = sourceEntity;
	}

	public EffectData Copy()
	{
		EffectData effectData = Pool.Get<EffectData>();
		CopyTo(effectData);
		return effectData;
	}

	public static EffectData Deserialize(BufferStream stream)
	{
		EffectData effectData = Pool.Get<EffectData>();
		Deserialize(stream, effectData, isDelta: false);
		return effectData;
	}

	public static EffectData DeserializeLengthDelimited(BufferStream stream)
	{
		EffectData effectData = Pool.Get<EffectData>();
		DeserializeLengthDelimited(stream, effectData, isDelta: false);
		return effectData;
	}

	public static EffectData DeserializeLength(BufferStream stream, int length)
	{
		EffectData effectData = Pool.Get<EffectData>();
		DeserializeLength(stream, length, effectData, isDelta: false);
		return effectData;
	}

	public static EffectData Deserialize(byte[] buffer)
	{
		EffectData effectData = Pool.Get<EffectData>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, effectData, isDelta: false);
		return effectData;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, EffectData previous)
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

	public static EffectData Deserialize(BufferStream stream, EffectData instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("global::EffectData");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::EffectData");
				instance.type = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::EffectData");
				instance.pooledstringid = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "global::EffectData");
				instance.number = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::EffectData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.origin, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::EffectData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::EffectData");
				instance.scale = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::EffectData");
				instance.entity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::EffectData");
				instance.bone = ProtocolParser.ReadUInt32(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "global::EffectData");
				instance.source = ProtocolParser.ReadUInt64(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::EffectData");
				instance.distanceOverride = ProtocolParser.ReadSingle(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::EffectData");
				instance.ignoreMaxSpawnDistance = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::EffectData");
				instance.sourceEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static EffectData DeserializeLengthDelimited(BufferStream stream, EffectData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("global::EffectData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::EffectData");
				instance.type = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::EffectData");
				instance.pooledstringid = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "global::EffectData");
				instance.number = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::EffectData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.origin, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::EffectData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::EffectData");
				instance.scale = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::EffectData");
				instance.entity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::EffectData");
				instance.bone = ProtocolParser.ReadUInt32(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "global::EffectData");
				instance.source = ProtocolParser.ReadUInt64(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::EffectData");
				instance.distanceOverride = ProtocolParser.ReadSingle(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::EffectData");
				instance.ignoreMaxSpawnDistance = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::EffectData");
				instance.sourceEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static EffectData DeserializeLength(BufferStream stream, int length, EffectData instance, bool isDelta)
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
			stream.ConsumeFieldOperation("global::EffectData");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::EffectData");
				instance.type = ProtocolParser.ReadUInt32(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::EffectData");
				instance.pooledstringid = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "global::EffectData");
				instance.number = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::EffectData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.origin, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::EffectData");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.normal, isDelta);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::EffectData");
				instance.scale = ProtocolParser.ReadSingle(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::EffectData");
				instance.entity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::EffectData");
				instance.bone = ProtocolParser.ReadUInt32(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "global::EffectData");
				instance.source = ProtocolParser.ReadUInt64(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::EffectData");
				instance.distanceOverride = ProtocolParser.ReadSingle(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::EffectData");
				instance.ignoreMaxSpawnDistance = ProtocolParser.ReadBool(stream);
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::EffectData");
				instance.sourceEntity = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "global::EffectData");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, EffectData instance, EffectData previous)
	{
		if (instance.type != previous.type)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.type);
		}
		if (instance.pooledstringid != previous.pooledstringid)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.pooledstringid);
		}
		if (instance.number != previous.number)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.number);
		}
		if (instance.origin != previous.origin)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.origin, previous.origin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field origin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.normal != previous.normal)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.normal, previous.normal);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field normal (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.scale != previous.scale)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.scale);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteUInt64(stream, instance.entity.Value);
		if (instance.bone != previous.bone)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt32(stream, instance.bone);
		}
		if (instance.source != previous.source)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, instance.source);
		}
		if (instance.distanceOverride != previous.distanceOverride)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.distanceOverride);
		}
		stream.WriteByte(88);
		ProtocolParser.WriteBool(stream, instance.ignoreMaxSpawnDistance);
		stream.WriteByte(96);
		ProtocolParser.WriteUInt64(stream, instance.sourceEntity.Value);
	}

	public static void Serialize(BufferStream stream, EffectData instance)
	{
		if (instance.type != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.type);
		}
		if (instance.pooledstringid != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.pooledstringid);
		}
		if (instance.number != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.number);
		}
		if (instance.origin != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.origin);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field origin (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.normal != default(Vector3))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.normal);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field normal (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.scale != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.scale);
		}
		if (instance.entity != default(NetworkableId))
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.entity.Value);
		}
		if (instance.bone != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt32(stream, instance.bone);
		}
		if (instance.source != 0L)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt64(stream, instance.source);
		}
		if (instance.distanceOverride != 0f)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.distanceOverride);
		}
		if (instance.ignoreMaxSpawnDistance)
		{
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.ignoreMaxSpawnDistance);
		}
		if (instance.sourceEntity != default(NetworkableId))
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, instance.sourceEntity.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entity.Value);
		action(UidType.NetworkableId, ref sourceEntity.Value);
	}
}
