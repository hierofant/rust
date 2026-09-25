using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PlayerProjectileAttack : IDisposable, Pool.IPooled, IProto<PlayerProjectileAttack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public PlayerAttack playerAttack;

	[NonSerialized]
	public Vector3 hitVelocity;

	[NonSerialized]
	public float hitDistance;

	[NonSerialized]
	public float travelTime;

	public static void ResetToPool(PlayerProjectileAttack instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.playerAttack != null)
			{
				instance.playerAttack.ResetToPool();
				instance.playerAttack = null;
			}
			instance.hitVelocity = default(Vector3);
			instance.hitDistance = 0f;
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
			throw new Exception("Trying to dispose PlayerProjectileAttack with ShouldPool set to false!");
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

	public void CopyTo(PlayerProjectileAttack instance)
	{
		if (playerAttack != null)
		{
			if (instance.playerAttack == null)
			{
				instance.playerAttack = playerAttack.Copy();
			}
			else
			{
				playerAttack.CopyTo(instance.playerAttack);
			}
		}
		else
		{
			instance.playerAttack = null;
		}
		instance.hitVelocity = hitVelocity;
		instance.hitDistance = hitDistance;
		instance.travelTime = travelTime;
	}

	public PlayerProjectileAttack Copy()
	{
		PlayerProjectileAttack playerProjectileAttack = Pool.Get<PlayerProjectileAttack>();
		CopyTo(playerProjectileAttack);
		return playerProjectileAttack;
	}

	public static PlayerProjectileAttack Deserialize(BufferStream stream)
	{
		PlayerProjectileAttack playerProjectileAttack = Pool.Get<PlayerProjectileAttack>();
		Deserialize(stream, playerProjectileAttack, isDelta: false);
		return playerProjectileAttack;
	}

	public static PlayerProjectileAttack DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerProjectileAttack playerProjectileAttack = Pool.Get<PlayerProjectileAttack>();
		DeserializeLengthDelimited(stream, playerProjectileAttack, isDelta: false);
		return playerProjectileAttack;
	}

	public static PlayerProjectileAttack DeserializeLength(BufferStream stream, int length)
	{
		PlayerProjectileAttack playerProjectileAttack = Pool.Get<PlayerProjectileAttack>();
		DeserializeLength(stream, length, playerProjectileAttack, isDelta: false);
		return playerProjectileAttack;
	}

	public static PlayerProjectileAttack Deserialize(byte[] buffer)
	{
		PlayerProjectileAttack playerProjectileAttack = Pool.Get<PlayerProjectileAttack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerProjectileAttack, isDelta: false);
		return playerProjectileAttack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerProjectileAttack previous)
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

	public static PlayerProjectileAttack Deserialize(BufferStream stream, PlayerProjectileAttack instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileAttack");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				if (instance.playerAttack == null)
				{
					instance.playerAttack = PlayerAttack.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerAttack.DeserializeLengthDelimited(stream, instance.playerAttack, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitVelocity, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				instance.hitDistance = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerProjectileAttack DeserializeLengthDelimited(BufferStream stream, PlayerProjectileAttack instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileAttack");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				if (instance.playerAttack == null)
				{
					instance.playerAttack = PlayerAttack.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerAttack.DeserializeLengthDelimited(stream, instance.playerAttack, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitVelocity, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				instance.hitDistance = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerProjectileAttack DeserializeLength(BufferStream stream, int length, PlayerProjectileAttack instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerProjectileAttack");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				if (instance.playerAttack == null)
				{
					instance.playerAttack = PlayerAttack.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerAttack.DeserializeLengthDelimited(stream, instance.playerAttack, isDelta);
				}
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.hitVelocity, isDelta);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				instance.hitDistance = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				instance.travelTime = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerProjectileAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerProjectileAttack instance, PlayerProjectileAttack previous)
	{
		if (instance.playerAttack == null)
		{
			throw new ArgumentNullException("playerAttack", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(2);
		int position = stream.Position;
		PlayerAttack.SerializeDelta(stream, instance.playerAttack, previous.playerAttack);
		int num = stream.Position - position;
		if (num > 16383)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerAttack (ProtoBuf.PlayerAttack)");
		}
		Span<byte> span = range.GetSpan();
		if (ProtocolParser.WriteUInt32((uint)num, span, 0) < 2)
		{
			span[0] |= 128;
			span[1] = 0;
		}
		if (instance.hitVelocity != previous.hitVelocity)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.hitVelocity, previous.hitVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.hitDistance != previous.hitDistance)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.hitDistance);
		}
		if (instance.travelTime != previous.travelTime)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.travelTime);
		}
	}

	public static void Serialize(BufferStream stream, PlayerProjectileAttack instance)
	{
		if (instance.playerAttack == null)
		{
			throw new ArgumentNullException("playerAttack", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(2);
		int position = stream.Position;
		PlayerAttack.Serialize(stream, instance.playerAttack);
		int num = stream.Position - position;
		if (num > 16383)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field playerAttack (ProtoBuf.PlayerAttack)");
		}
		Span<byte> span = range.GetSpan();
		if (ProtocolParser.WriteUInt32((uint)num, span, 0) < 2)
		{
			span[0] |= 128;
			span[1] = 0;
		}
		if (instance.hitVelocity != default(Vector3))
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.hitVelocity);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field hitVelocity (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.hitDistance != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.hitDistance);
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
		playerAttack?.InspectUids(action);
	}
}
