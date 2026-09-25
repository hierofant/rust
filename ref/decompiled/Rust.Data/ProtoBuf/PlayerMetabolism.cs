using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerMetabolism : IDisposable, Pool.IPooled, IProto<PlayerMetabolism>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float health;

	[NonSerialized]
	public float calories;

	[NonSerialized]
	public float hydration;

	[NonSerialized]
	public float heartrate;

	[NonSerialized]
	public float temperature;

	[NonSerialized]
	public float poison;

	[NonSerialized]
	public float radiation_level;

	[NonSerialized]
	public float wetness;

	[NonSerialized]
	public float dirtyness;

	[NonSerialized]
	public float oxygen;

	[NonSerialized]
	public float bleeding;

	[NonSerialized]
	public float radiation_poisoning;

	[NonSerialized]
	public float comfort;

	[NonSerialized]
	public float pending_health;

	[NonSerialized]
	public uint changed_mask;

	public static void ResetToPool(PlayerMetabolism instance)
	{
		if (instance.ShouldPool)
		{
			instance.health = 0f;
			instance.calories = 0f;
			instance.hydration = 0f;
			instance.heartrate = 0f;
			instance.temperature = 0f;
			instance.poison = 0f;
			instance.radiation_level = 0f;
			instance.wetness = 0f;
			instance.dirtyness = 0f;
			instance.oxygen = 0f;
			instance.bleeding = 0f;
			instance.radiation_poisoning = 0f;
			instance.comfort = 0f;
			instance.pending_health = 0f;
			instance.changed_mask = 0u;
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
			throw new Exception("Trying to dispose PlayerMetabolism with ShouldPool set to false!");
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

	public void CopyTo(PlayerMetabolism instance)
	{
		instance.health = health;
		instance.calories = calories;
		instance.hydration = hydration;
		instance.heartrate = heartrate;
		instance.temperature = temperature;
		instance.poison = poison;
		instance.radiation_level = radiation_level;
		instance.wetness = wetness;
		instance.dirtyness = dirtyness;
		instance.oxygen = oxygen;
		instance.bleeding = bleeding;
		instance.radiation_poisoning = radiation_poisoning;
		instance.comfort = comfort;
		instance.pending_health = pending_health;
		instance.changed_mask = changed_mask;
	}

	public PlayerMetabolism Copy()
	{
		PlayerMetabolism playerMetabolism = Pool.Get<PlayerMetabolism>();
		CopyTo(playerMetabolism);
		return playerMetabolism;
	}

	public static PlayerMetabolism Deserialize(BufferStream stream)
	{
		PlayerMetabolism playerMetabolism = Pool.Get<PlayerMetabolism>();
		Deserialize(stream, playerMetabolism, isDelta: false);
		return playerMetabolism;
	}

	public static PlayerMetabolism DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerMetabolism playerMetabolism = Pool.Get<PlayerMetabolism>();
		DeserializeLengthDelimited(stream, playerMetabolism, isDelta: false);
		return playerMetabolism;
	}

	public static PlayerMetabolism DeserializeLength(BufferStream stream, int length)
	{
		PlayerMetabolism playerMetabolism = Pool.Get<PlayerMetabolism>();
		DeserializeLength(stream, length, playerMetabolism, isDelta: false);
		return playerMetabolism;
	}

	public static PlayerMetabolism Deserialize(byte[] buffer)
	{
		PlayerMetabolism playerMetabolism = Pool.Get<PlayerMetabolism>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerMetabolism, isDelta: false);
		return playerMetabolism;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerMetabolism previous)
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

	public static PlayerMetabolism Deserialize(BufferStream stream, PlayerMetabolism instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.PlayerMetabolism");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.health = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.calories = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.hydration = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.heartrate = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.temperature = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.poison = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.radiation_level = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.wetness = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.dirtyness = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.oxygen = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.bleeding = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.radiation_poisoning = ProtocolParser.ReadSingle(stream);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.comfort = ProtocolParser.ReadSingle(stream);
				continue;
			case 117:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.pending_health = ProtocolParser.ReadSingle(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.changed_mask = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerMetabolism DeserializeLengthDelimited(BufferStream stream, PlayerMetabolism instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerMetabolism");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.health = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.calories = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.hydration = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.heartrate = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.temperature = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.poison = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.radiation_level = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.wetness = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.dirtyness = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.oxygen = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.bleeding = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.radiation_poisoning = ProtocolParser.ReadSingle(stream);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.comfort = ProtocolParser.ReadSingle(stream);
				continue;
			case 117:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.pending_health = ProtocolParser.ReadSingle(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.changed_mask = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerMetabolism DeserializeLength(BufferStream stream, int length, PlayerMetabolism instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerMetabolism");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.health = ProtocolParser.ReadSingle(stream);
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.calories = ProtocolParser.ReadSingle(stream);
				continue;
			case 29:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.hydration = ProtocolParser.ReadSingle(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.heartrate = ProtocolParser.ReadSingle(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.temperature = ProtocolParser.ReadSingle(stream);
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.poison = ProtocolParser.ReadSingle(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.radiation_level = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.wetness = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.dirtyness = ProtocolParser.ReadSingle(stream);
				continue;
			case 85:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.oxygen = ProtocolParser.ReadSingle(stream);
				continue;
			case 93:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.bleeding = ProtocolParser.ReadSingle(stream);
				continue;
			case 101:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.radiation_poisoning = ProtocolParser.ReadSingle(stream);
				continue;
			case 109:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.comfort = ProtocolParser.ReadSingle(stream);
				continue;
			case 117:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.pending_health = ProtocolParser.ReadSingle(stream);
				continue;
			case 120:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				instance.changed_mask = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 15u:
				stream.ValidateFieldOrder(ref lastFieldId, 15u, fieldIsRepeated: false, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerMetabolism");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerMetabolism instance, PlayerMetabolism previous)
	{
		if (instance.health != previous.health)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.health);
		}
		if (instance.calories != previous.calories)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.calories);
		}
		if (instance.hydration != previous.hydration)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.hydration);
		}
		if (instance.heartrate != previous.heartrate)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.heartrate);
		}
		if (instance.temperature != previous.temperature)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.temperature);
		}
		if (instance.poison != previous.poison)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.poison);
		}
		if (instance.radiation_level != previous.radiation_level)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.radiation_level);
		}
		if (instance.wetness != previous.wetness)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.wetness);
		}
		if (instance.dirtyness != previous.dirtyness)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.dirtyness);
		}
		if (instance.oxygen != previous.oxygen)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.oxygen);
		}
		if (instance.bleeding != previous.bleeding)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.bleeding);
		}
		if (instance.radiation_poisoning != previous.radiation_poisoning)
		{
			stream.WriteByte(101);
			ProtocolParser.WriteSingle(stream, instance.radiation_poisoning);
		}
		if (instance.comfort != previous.comfort)
		{
			stream.WriteByte(109);
			ProtocolParser.WriteSingle(stream, instance.comfort);
		}
		if (instance.pending_health != previous.pending_health)
		{
			stream.WriteByte(117);
			ProtocolParser.WriteSingle(stream, instance.pending_health);
		}
		if (instance.changed_mask != previous.changed_mask)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteUInt32(stream, instance.changed_mask);
		}
	}

	public static void Serialize(BufferStream stream, PlayerMetabolism instance)
	{
		if (instance.health != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.health);
		}
		if (instance.calories != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.calories);
		}
		if (instance.hydration != 0f)
		{
			stream.WriteByte(29);
			ProtocolParser.WriteSingle(stream, instance.hydration);
		}
		if (instance.heartrate != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.heartrate);
		}
		if (instance.temperature != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.temperature);
		}
		if (instance.poison != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.poison);
		}
		if (instance.radiation_level != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.radiation_level);
		}
		if (instance.wetness != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.wetness);
		}
		if (instance.dirtyness != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.dirtyness);
		}
		if (instance.oxygen != 0f)
		{
			stream.WriteByte(85);
			ProtocolParser.WriteSingle(stream, instance.oxygen);
		}
		if (instance.bleeding != 0f)
		{
			stream.WriteByte(93);
			ProtocolParser.WriteSingle(stream, instance.bleeding);
		}
		if (instance.radiation_poisoning != 0f)
		{
			stream.WriteByte(101);
			ProtocolParser.WriteSingle(stream, instance.radiation_poisoning);
		}
		if (instance.comfort != 0f)
		{
			stream.WriteByte(109);
			ProtocolParser.WriteSingle(stream, instance.comfort);
		}
		if (instance.pending_health != 0f)
		{
			stream.WriteByte(117);
			ProtocolParser.WriteSingle(stream, instance.pending_health);
		}
		if (instance.changed_mask != 0)
		{
			stream.WriteByte(120);
			ProtocolParser.WriteUInt32(stream, instance.changed_mask);
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
