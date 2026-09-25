using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class Attack : IDisposable, Pool.IPooled, IProto<Attack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Vector3 pointStart;

	[NonSerialized]
	public Vector3 pointEnd;

	[NonSerialized]
	public NetworkableId hitID;

	[NonSerialized]
	public uint hitBone;

	[NonSerialized]
	public Vector3 hitNormalLocal;

	[NonSerialized]
	public Vector3 hitPositionLocal;

	[NonSerialized]
	public Vector3 hitNormalWorld;

	[NonSerialized]
	public Vector3 hitPositionWorld;

	[NonSerialized]
	public uint hitPartID;

	[NonSerialized]
	public uint hitMaterialID;

	[NonSerialized]
	public NetworkableId srcParentID;

	[NonSerialized]
	public NetworkableId dstParentID;

	public static void ResetToPool(Attack instance)
	{
		if (instance.ShouldPool)
		{
			instance.pointStart = default(Vector3);
			instance.pointEnd = default(Vector3);
			instance.hitID = default(NetworkableId);
			instance.hitBone = 0u;
			instance.hitNormalLocal = default(Vector3);
			instance.hitPositionLocal = default(Vector3);
			instance.hitNormalWorld = default(Vector3);
			instance.hitPositionWorld = default(Vector3);
			instance.hitPartID = 0u;
			instance.hitMaterialID = 0u;
			instance.srcParentID = default(NetworkableId);
			instance.dstParentID = default(NetworkableId);
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
			throw new Exception("Trying to dispose Attack with ShouldPool set to false!");
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

	public void CopyTo(Attack instance)
	{
		instance.pointStart = pointStart;
		instance.pointEnd = pointEnd;
		instance.hitID = hitID;
		instance.hitBone = hitBone;
		instance.hitNormalLocal = hitNormalLocal;
		instance.hitPositionLocal = hitPositionLocal;
		instance.hitNormalWorld = hitNormalWorld;
		instance.hitPositionWorld = hitPositionWorld;
		instance.hitPartID = hitPartID;
		instance.hitMaterialID = hitMaterialID;
		instance.srcParentID = srcParentID;
		instance.dstParentID = dstParentID;
	}

	public Attack Copy()
	{
		Attack attack = Pool.Get<Attack>();
		CopyTo(attack);
		return attack;
	}

	public static Attack Deserialize(BufferStream stream)
	{
		Attack attack = Pool.Get<Attack>();
		Deserialize(stream, attack, isDelta: false);
		return attack;
	}

	public static Attack DeserializeLengthDelimited(BufferStream stream)
	{
		Attack attack = Pool.Get<Attack>();
		DeserializeLengthDelimited(stream, attack, isDelta: false);
		return attack;
	}

	public static Attack DeserializeLength(BufferStream stream, int length)
	{
		Attack attack = Pool.Get<Attack>();
		DeserializeLength(stream, length, attack, isDelta: false);
		return attack;
	}

	public static Attack Deserialize(byte[] buffer)
	{
		Attack attack = Pool.Get<Attack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, attack, isDelta: false);
		return attack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Attack previous)
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

	public static Attack Deserialize(BufferStream stream, Attack instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Attack");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.pointStart, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.pointEnd, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitBone = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormalLocal, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPositionLocal, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormalWorld, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPositionWorld, isDelta);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitPartID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitMaterialID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.srcParentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.dstParentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Attack DeserializeLengthDelimited(BufferStream stream, Attack instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Attack");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.pointStart, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.pointEnd, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitBone = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormalLocal, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPositionLocal, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormalWorld, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPositionWorld, isDelta);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitPartID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitMaterialID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.srcParentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.dstParentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Attack DeserializeLength(BufferStream stream, int length, Attack instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Attack");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.pointStart, isDelta);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.pointEnd, isDelta);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitBone = ProtocolParser.ReadUInt32(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormalLocal, isDelta);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPositionLocal, isDelta);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormalWorld, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Attack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPositionWorld, isDelta);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitPartID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 80:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.hitMaterialID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 88:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.srcParentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 96:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Attack");
				instance.dstParentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Attack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Attack instance, Attack previous)
	{
		if (instance.pointStart != previous.pointStart)
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.pointStart, previous.pointStart);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field pointStart (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.pointEnd != previous.pointEnd)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.pointEnd, previous.pointEnd);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field pointEnd (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteUInt64(stream, instance.hitID.Value);
		if (instance.hitBone != previous.hitBone)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.hitBone);
		}
		if (instance.hitNormalLocal != previous.hitNormalLocal)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitNormalLocal, previous.hitNormalLocal);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitNormalLocal (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.hitPositionLocal != previous.hitPositionLocal)
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitPositionLocal, previous.hitPositionLocal);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitPositionLocal (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.hitNormalWorld != previous.hitNormalWorld)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitNormalWorld, previous.hitNormalWorld);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitNormalWorld (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.hitPositionWorld != previous.hitPositionWorld)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitPositionWorld, previous.hitPositionWorld);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitPositionWorld (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.hitPartID != previous.hitPartID)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.hitPartID);
		}
		if (instance.hitMaterialID != previous.hitMaterialID)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt32(stream, instance.hitMaterialID);
		}
		stream.WriteByte(88);
		ProtocolParser.WriteUInt64(stream, instance.srcParentID.Value);
		stream.WriteByte(96);
		ProtocolParser.WriteUInt64(stream, instance.dstParentID.Value);
	}

	public static void Serialize(BufferStream stream, Attack instance)
	{
		if (instance.pointStart != default(Vector3))
		{
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.pointStart);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field pointStart (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.pointEnd != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.pointEnd);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field pointEnd (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.hitID != default(NetworkableId))
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.hitID.Value);
		}
		if (instance.hitBone != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt32(stream, instance.hitBone);
		}
		if (instance.hitNormalLocal != default(Vector3))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitNormalLocal);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitNormalLocal (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.hitPositionLocal != default(Vector3))
		{
			stream.WriteByte(50);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitPositionLocal);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitPositionLocal (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.hitNormalWorld != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range5 = stream.GetRange(1);
			int position5 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitNormalWorld);
			int num5 = stream.Position - position5;
			if (num5 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitNormalWorld (UnityEngine.Vector3)");
			}
			Span<byte> span5 = range5.GetSpan();
			ProtocolParser.WriteUInt32((uint)num5, span5, 0);
		}
		if (instance.hitPositionWorld != default(Vector3))
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range6 = stream.GetRange(1);
			int position6 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitPositionWorld);
			int num6 = stream.Position - position6;
			if (num6 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitPositionWorld (UnityEngine.Vector3)");
			}
			Span<byte> span6 = range6.GetSpan();
			ProtocolParser.WriteUInt32((uint)num6, span6, 0);
		}
		if (instance.hitPartID != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteUInt32(stream, instance.hitPartID);
		}
		if (instance.hitMaterialID != 0)
		{
			stream.WriteByte(80);
			ProtocolParser.WriteUInt32(stream, instance.hitMaterialID);
		}
		if (instance.srcParentID != default(NetworkableId))
		{
			stream.WriteByte(88);
			ProtocolParser.WriteUInt64(stream, instance.srcParentID.Value);
		}
		if (instance.dstParentID != default(NetworkableId))
		{
			stream.WriteByte(96);
			ProtocolParser.WriteUInt64(stream, instance.dstParentID.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref hitID.Value);
		action(UidType.NetworkableId, ref srcParentID.Value);
		action(UidType.NetworkableId, ref dstParentID.Value);
	}
}
