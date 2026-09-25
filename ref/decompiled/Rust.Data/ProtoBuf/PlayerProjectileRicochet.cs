using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PlayerProjectileRicochet : IDisposable, Pool.IPooled, IProto<PlayerProjectileRicochet>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int projectileID;

	[NonSerialized]
	public Vector3 hitPosition;

	[NonSerialized]
	public Vector3 inVelocity;

	[NonSerialized]
	public Vector3 outVelocity;

	[NonSerialized]
	public Vector3 hitNormal;

	[NonSerialized]
	public float travelTime;

	public static void ResetToPool(PlayerProjectileRicochet instance)
	{
		if (instance.ShouldPool)
		{
			instance.projectileID = 0;
			instance.hitPosition = default(Vector3);
			instance.inVelocity = default(Vector3);
			instance.outVelocity = default(Vector3);
			instance.hitNormal = default(Vector3);
			instance.travelTime = 0f;
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
			throw new Exception("Trying to dispose PlayerProjectileRicochet with ShouldPool set to false!");
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

	public void CopyTo(PlayerProjectileRicochet instance)
	{
		instance.projectileID = projectileID;
		instance.hitPosition = hitPosition;
		instance.inVelocity = inVelocity;
		instance.outVelocity = outVelocity;
		instance.hitNormal = hitNormal;
		instance.travelTime = travelTime;
	}

	public PlayerProjectileRicochet Copy()
	{
		PlayerProjectileRicochet playerProjectileRicochet = Pool.Get<PlayerProjectileRicochet>();
		CopyTo(playerProjectileRicochet);
		return playerProjectileRicochet;
	}

	public static PlayerProjectileRicochet Deserialize(BufferStream stream)
	{
		PlayerProjectileRicochet playerProjectileRicochet = Pool.Get<PlayerProjectileRicochet>();
		Deserialize(stream, playerProjectileRicochet, isDelta: false);
		return playerProjectileRicochet;
	}

	public static PlayerProjectileRicochet DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerProjectileRicochet playerProjectileRicochet = Pool.Get<PlayerProjectileRicochet>();
		DeserializeLengthDelimited(stream, playerProjectileRicochet, isDelta: false);
		return playerProjectileRicochet;
	}

	public static PlayerProjectileRicochet DeserializeLength(BufferStream stream, int length)
	{
		PlayerProjectileRicochet playerProjectileRicochet = Pool.Get<PlayerProjectileRicochet>();
		DeserializeLength(stream, length, playerProjectileRicochet, isDelta: false);
		return playerProjectileRicochet;
	}

	public static PlayerProjectileRicochet Deserialize(byte[] buffer)
	{
		PlayerProjectileRicochet playerProjectileRicochet = Pool.Get<PlayerProjectileRicochet>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerProjectileRicochet, isDelta: false);
		return playerProjectileRicochet;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerProjectileRicochet previous)
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

	public static PlayerProjectileRicochet Deserialize(BufferStream stream, PlayerProjectileRicochet instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileRicochet");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPosition, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.inVelocity, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.outVelocity, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormal, isDelta);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerProjectileRicochet DeserializeLengthDelimited(BufferStream stream, PlayerProjectileRicochet instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileRicochet");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPosition, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.inVelocity, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.outVelocity, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormal, isDelta);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerProjectileRicochet DeserializeLength(BufferStream stream, int length, PlayerProjectileRicochet instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileRicochet");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitPosition, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.inVelocity, isDelta);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.outVelocity, isDelta);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitNormal, isDelta);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileRicochet");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerProjectileRicochet instance, PlayerProjectileRicochet previous)
	{
		if (instance.projectileID != previous.projectileID)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
		}
		if (instance.hitPosition != previous.hitPosition)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitPosition, previous.hitPosition);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitPosition (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.inVelocity != previous.inVelocity)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.inVelocity, previous.inVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.outVelocity != previous.outVelocity)
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.outVelocity, previous.outVelocity);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field outVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.hitNormal != previous.hitNormal)
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitNormal, previous.hitNormal);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitNormal (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.travelTime != previous.travelTime)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.travelTime);
		}
	}

	public static void Serialize(BufferStream stream, PlayerProjectileRicochet instance)
	{
		if (instance.projectileID != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
		}
		if (instance.hitPosition != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitPosition);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitPosition (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.inVelocity != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.inVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field inVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.outVelocity != default(Vector3))
		{
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.outVelocity);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field outVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.hitNormal != default(Vector3))
		{
			stream.WriteByte(42);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitNormal);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitNormal (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
		if (instance.travelTime != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.travelTime);
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
