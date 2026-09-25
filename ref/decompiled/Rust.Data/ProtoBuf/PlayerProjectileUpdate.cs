using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PlayerProjectileUpdate : IDisposable, Pool.IPooled, IProto<PlayerProjectileUpdate>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int projectileID;

	[NonSerialized]
	public Vector3 curPosition;

	[NonSerialized]
	public Vector3 curVelocity;

	[NonSerialized]
	public float travelTime;

	public static void ResetToPool(PlayerProjectileUpdate instance)
	{
		if (instance.ShouldPool)
		{
			instance.projectileID = 0;
			instance.curPosition = default(Vector3);
			instance.curVelocity = default(Vector3);
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
			throw new Exception("Trying to dispose PlayerProjectileUpdate with ShouldPool set to false!");
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

	public void CopyTo(PlayerProjectileUpdate instance)
	{
		instance.projectileID = projectileID;
		instance.curPosition = curPosition;
		instance.curVelocity = curVelocity;
		instance.travelTime = travelTime;
	}

	public PlayerProjectileUpdate Copy()
	{
		PlayerProjectileUpdate playerProjectileUpdate = Pool.Get<PlayerProjectileUpdate>();
		CopyTo(playerProjectileUpdate);
		return playerProjectileUpdate;
	}

	public static PlayerProjectileUpdate Deserialize(BufferStream stream)
	{
		PlayerProjectileUpdate playerProjectileUpdate = Pool.Get<PlayerProjectileUpdate>();
		Deserialize(stream, playerProjectileUpdate, isDelta: false);
		return playerProjectileUpdate;
	}

	public static PlayerProjectileUpdate DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerProjectileUpdate playerProjectileUpdate = Pool.Get<PlayerProjectileUpdate>();
		DeserializeLengthDelimited(stream, playerProjectileUpdate, isDelta: false);
		return playerProjectileUpdate;
	}

	public static PlayerProjectileUpdate DeserializeLength(BufferStream stream, int length)
	{
		PlayerProjectileUpdate playerProjectileUpdate = Pool.Get<PlayerProjectileUpdate>();
		DeserializeLength(stream, length, playerProjectileUpdate, isDelta: false);
		return playerProjectileUpdate;
	}

	public static PlayerProjectileUpdate Deserialize(byte[] buffer)
	{
		PlayerProjectileUpdate playerProjectileUpdate = Pool.Get<PlayerProjectileUpdate>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerProjectileUpdate, isDelta: false);
		return playerProjectileUpdate;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerProjectileUpdate previous)
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

	public static PlayerProjectileUpdate Deserialize(BufferStream stream, PlayerProjectileUpdate instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileUpdate");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.curPosition, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.curVelocity, isDelta);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerProjectileUpdate DeserializeLengthDelimited(BufferStream stream, PlayerProjectileUpdate instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileUpdate");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.curPosition, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.curVelocity, isDelta);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerProjectileUpdate DeserializeLength(BufferStream stream, int length, PlayerProjectileUpdate instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileUpdate");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.curPosition, isDelta);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.curVelocity, isDelta);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileUpdate");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerProjectileUpdate instance, PlayerProjectileUpdate previous)
	{
		if (instance.projectileID != previous.projectileID)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
		}
		if (instance.curPosition != previous.curPosition)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.curPosition, previous.curPosition);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field curPosition (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.curVelocity != previous.curVelocity)
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.curVelocity, previous.curVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field curVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.travelTime != previous.travelTime)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.travelTime);
		}
	}

	public static void Serialize(BufferStream stream, PlayerProjectileUpdate instance)
	{
		if (instance.projectileID != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
		}
		if (instance.curPosition != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Vector3Serialized.Serialize(stream, instance.curPosition);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field curPosition (UnityEngine.Vector3)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
		if (instance.curVelocity != default(Vector3))
		{
			stream.WriteByte(26);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.curVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field curVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.travelTime != 0f)
		{
			stream.WriteByte(37);
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
