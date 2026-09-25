using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class LootContainer : IDisposable, Pool.IPooled, IProto<LootContainer>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float countdownTimeRemaining;

	[NonSerialized]
	public int refreshesUntilFullSpawn;

	[NonSerialized]
	public bool openedSinceFullSpawn;

	public static void ResetToPool(LootContainer instance)
	{
		if (instance.ShouldPool)
		{
			instance.countdownTimeRemaining = 0f;
			instance.refreshesUntilFullSpawn = 0;
			instance.openedSinceFullSpawn = false;
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
			throw new Exception("Trying to dispose LootContainer with ShouldPool set to false!");
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

	public void CopyTo(LootContainer instance)
	{
		instance.countdownTimeRemaining = countdownTimeRemaining;
		instance.refreshesUntilFullSpawn = refreshesUntilFullSpawn;
		instance.openedSinceFullSpawn = openedSinceFullSpawn;
	}

	public LootContainer Copy()
	{
		LootContainer lootContainer = Pool.Get<LootContainer>();
		CopyTo(lootContainer);
		return lootContainer;
	}

	public static LootContainer Deserialize(BufferStream stream)
	{
		LootContainer lootContainer = Pool.Get<LootContainer>();
		Deserialize(stream, lootContainer, isDelta: false);
		return lootContainer;
	}

	public static LootContainer DeserializeLengthDelimited(BufferStream stream)
	{
		LootContainer lootContainer = Pool.Get<LootContainer>();
		DeserializeLengthDelimited(stream, lootContainer, isDelta: false);
		return lootContainer;
	}

	public static LootContainer DeserializeLength(BufferStream stream, int length)
	{
		LootContainer lootContainer = Pool.Get<LootContainer>();
		DeserializeLength(stream, length, lootContainer, isDelta: false);
		return lootContainer;
	}

	public static LootContainer Deserialize(byte[] buffer)
	{
		LootContainer lootContainer = Pool.Get<LootContainer>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, lootContainer, isDelta: false);
		return lootContainer;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, LootContainer previous)
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

	public static LootContainer Deserialize(BufferStream stream, LootContainer instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.LootContainer");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.countdownTimeRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.refreshesUntilFullSpawn = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.openedSinceFullSpawn = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LootContainer DeserializeLengthDelimited(BufferStream stream, LootContainer instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.LootContainer");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.countdownTimeRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.refreshesUntilFullSpawn = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.openedSinceFullSpawn = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LootContainer DeserializeLength(BufferStream stream, int length, LootContainer instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.LootContainer");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.countdownTimeRemaining = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.refreshesUntilFullSpawn = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				instance.openedSinceFullSpawn = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LootContainer");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, LootContainer instance, LootContainer previous)
	{
		if (instance.countdownTimeRemaining != previous.countdownTimeRemaining)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.countdownTimeRemaining);
		}
		if (instance.refreshesUntilFullSpawn != previous.refreshesUntilFullSpawn)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.refreshesUntilFullSpawn);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.openedSinceFullSpawn);
	}

	public static void Serialize(BufferStream stream, LootContainer instance)
	{
		if (instance.countdownTimeRemaining != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.countdownTimeRemaining);
		}
		if (instance.refreshesUntilFullSpawn != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.refreshesUntilFullSpawn);
		}
		if (instance.openedSinceFullSpawn)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.openedSinceFullSpawn);
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
