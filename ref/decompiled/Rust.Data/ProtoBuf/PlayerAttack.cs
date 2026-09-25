using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerAttack : IDisposable, Pool.IPooled, IProto<PlayerAttack>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public Attack attack;

	[NonSerialized]
	public int projectileID;

	public static void ResetToPool(PlayerAttack instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.attack != null)
			{
				instance.attack.ResetToPool();
				instance.attack = null;
			}
			instance.projectileID = 0;
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
			throw new Exception("Trying to dispose PlayerAttack with ShouldPool set to false!");
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

	public void CopyTo(PlayerAttack instance)
	{
		if (attack != null)
		{
			if (instance.attack == null)
			{
				instance.attack = attack.Copy();
			}
			else
			{
				attack.CopyTo(instance.attack);
			}
		}
		else
		{
			instance.attack = null;
		}
		instance.projectileID = projectileID;
	}

	public PlayerAttack Copy()
	{
		PlayerAttack playerAttack = Pool.Get<PlayerAttack>();
		CopyTo(playerAttack);
		return playerAttack;
	}

	public static PlayerAttack Deserialize(BufferStream stream)
	{
		PlayerAttack playerAttack = Pool.Get<PlayerAttack>();
		Deserialize(stream, playerAttack, isDelta: false);
		return playerAttack;
	}

	public static PlayerAttack DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerAttack playerAttack = Pool.Get<PlayerAttack>();
		DeserializeLengthDelimited(stream, playerAttack, isDelta: false);
		return playerAttack;
	}

	public static PlayerAttack DeserializeLength(BufferStream stream, int length)
	{
		PlayerAttack playerAttack = Pool.Get<PlayerAttack>();
		DeserializeLength(stream, length, playerAttack, isDelta: false);
		return playerAttack;
	}

	public static PlayerAttack Deserialize(byte[] buffer)
	{
		PlayerAttack playerAttack = Pool.Get<PlayerAttack>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerAttack, isDelta: false);
		return playerAttack;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerAttack previous)
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

	public static PlayerAttack Deserialize(BufferStream stream, PlayerAttack instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerAttack");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				if (instance.attack == null)
				{
					instance.attack = Attack.DeserializeLengthDelimited(stream);
				}
				else
				{
					Attack.DeserializeLengthDelimited(stream, instance.attack, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerAttack DeserializeLengthDelimited(BufferStream stream, PlayerAttack instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerAttack");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				if (instance.attack == null)
				{
					instance.attack = Attack.DeserializeLengthDelimited(stream);
				}
				else
				{
					Attack.DeserializeLengthDelimited(stream, instance.attack, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerAttack DeserializeLength(BufferStream stream, int length, PlayerAttack instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerAttack");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				if (instance.attack == null)
				{
					instance.attack = Attack.DeserializeLengthDelimited(stream);
				}
				else
				{
					Attack.DeserializeLengthDelimited(stream, instance.attack, isDelta);
				}
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerAttack");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerAttack instance, PlayerAttack previous)
	{
		if (instance.attack == null)
		{
			throw new ArgumentNullException("attack", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(2);
		int position = stream.Position;
		Attack.SerializeDelta(stream, instance.attack, previous.attack);
		int num = stream.Position - position;
		if (num > 16383)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field attack (ProtoBuf.Attack)");
		}
		Span<byte> span = range.GetSpan();
		if (ProtocolParser.WriteUInt32((uint)num, span, 0) < 2)
		{
			span[0] |= 128;
			span[1] = 0;
		}
		if (instance.projectileID != previous.projectileID)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
		}
	}

	public static void Serialize(BufferStream stream, PlayerAttack instance)
	{
		if (instance.attack == null)
		{
			throw new ArgumentNullException("attack", "Required by proto specification.");
		}
		stream.WriteByte(10);
		BufferStream.RangeHandle range = stream.GetRange(2);
		int position = stream.Position;
		Attack.Serialize(stream, instance.attack);
		int num = stream.Position - position;
		if (num > 16383)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field attack (ProtoBuf.Attack)");
		}
		Span<byte> span = range.GetSpan();
		if (ProtocolParser.WriteUInt32((uint)num, span, 0) < 2)
		{
			span[0] |= 128;
			span[1] = 0;
		}
		if (instance.projectileID != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		attack?.InspectUids(action);
	}
}
