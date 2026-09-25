using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class LegacyShelter : IDisposable, Pool.IPooled, IProto<LegacyShelter>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId doorID;

	[NonSerialized]
	public float timeSinceInteracted;

	[NonSerialized]
	public ulong ownerId;

	public static void ResetToPool(LegacyShelter instance)
	{
		if (instance.ShouldPool)
		{
			instance.doorID = default(NetworkableId);
			instance.timeSinceInteracted = 0f;
			instance.ownerId = 0uL;
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
			throw new Exception("Trying to dispose LegacyShelter with ShouldPool set to false!");
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

	public void CopyTo(LegacyShelter instance)
	{
		instance.doorID = doorID;
		instance.timeSinceInteracted = timeSinceInteracted;
		instance.ownerId = ownerId;
	}

	public LegacyShelter Copy()
	{
		LegacyShelter legacyShelter = Pool.Get<LegacyShelter>();
		CopyTo(legacyShelter);
		return legacyShelter;
	}

	public static LegacyShelter Deserialize(BufferStream stream)
	{
		LegacyShelter legacyShelter = Pool.Get<LegacyShelter>();
		Deserialize(stream, legacyShelter, isDelta: false);
		return legacyShelter;
	}

	public static LegacyShelter DeserializeLengthDelimited(BufferStream stream)
	{
		LegacyShelter legacyShelter = Pool.Get<LegacyShelter>();
		DeserializeLengthDelimited(stream, legacyShelter, isDelta: false);
		return legacyShelter;
	}

	public static LegacyShelter DeserializeLength(BufferStream stream, int length)
	{
		LegacyShelter legacyShelter = Pool.Get<LegacyShelter>();
		DeserializeLength(stream, length, legacyShelter, isDelta: false);
		return legacyShelter;
	}

	public static LegacyShelter Deserialize(byte[] buffer)
	{
		LegacyShelter legacyShelter = Pool.Get<LegacyShelter>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, legacyShelter, isDelta: false);
		return legacyShelter;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, LegacyShelter previous)
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

	public static LegacyShelter Deserialize(BufferStream stream, LegacyShelter instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.LegacyShelter");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.doorID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.timeSinceInteracted = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.ownerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LegacyShelter DeserializeLengthDelimited(BufferStream stream, LegacyShelter instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.LegacyShelter");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.doorID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.timeSinceInteracted = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.ownerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static LegacyShelter DeserializeLength(BufferStream stream, int length, LegacyShelter instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.LegacyShelter");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.doorID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.timeSinceInteracted = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				instance.ownerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.LegacyShelter");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, LegacyShelter instance, LegacyShelter previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.doorID.Value);
		if (instance.timeSinceInteracted != previous.timeSinceInteracted)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.timeSinceInteracted);
		}
		if (instance.ownerId != previous.ownerId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.ownerId);
		}
	}

	public static void Serialize(BufferStream stream, LegacyShelter instance)
	{
		if (instance.doorID != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.doorID.Value);
		}
		if (instance.timeSinceInteracted != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.timeSinceInteracted);
		}
		if (instance.ownerId != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.ownerId);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref doorID.Value);
	}
}
