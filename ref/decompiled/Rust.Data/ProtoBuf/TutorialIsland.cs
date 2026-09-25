using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TutorialIsland : IDisposable, Pool.IPooled, IProto<TutorialIsland>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId targetPlayer;

	[NonSerialized]
	public float disconnectDuration;

	[NonSerialized]
	public int spawnLocationIndex;

	[NonSerialized]
	public float tutorialDuration;

	public static void ResetToPool(TutorialIsland instance)
	{
		if (instance.ShouldPool)
		{
			instance.targetPlayer = default(NetworkableId);
			instance.disconnectDuration = 0f;
			instance.spawnLocationIndex = 0;
			instance.tutorialDuration = 0f;
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
			throw new Exception("Trying to dispose TutorialIsland with ShouldPool set to false!");
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

	public void CopyTo(TutorialIsland instance)
	{
		instance.targetPlayer = targetPlayer;
		instance.disconnectDuration = disconnectDuration;
		instance.spawnLocationIndex = spawnLocationIndex;
		instance.tutorialDuration = tutorialDuration;
	}

	public TutorialIsland Copy()
	{
		TutorialIsland tutorialIsland = Pool.Get<TutorialIsland>();
		CopyTo(tutorialIsland);
		return tutorialIsland;
	}

	public static TutorialIsland Deserialize(BufferStream stream)
	{
		TutorialIsland tutorialIsland = Pool.Get<TutorialIsland>();
		Deserialize(stream, tutorialIsland, isDelta: false);
		return tutorialIsland;
	}

	public static TutorialIsland DeserializeLengthDelimited(BufferStream stream)
	{
		TutorialIsland tutorialIsland = Pool.Get<TutorialIsland>();
		DeserializeLengthDelimited(stream, tutorialIsland, isDelta: false);
		return tutorialIsland;
	}

	public static TutorialIsland DeserializeLength(BufferStream stream, int length)
	{
		TutorialIsland tutorialIsland = Pool.Get<TutorialIsland>();
		DeserializeLength(stream, length, tutorialIsland, isDelta: false);
		return tutorialIsland;
	}

	public static TutorialIsland Deserialize(byte[] buffer)
	{
		TutorialIsland tutorialIsland = Pool.Get<TutorialIsland>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, tutorialIsland, isDelta: false);
		return tutorialIsland;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TutorialIsland previous)
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

	public static TutorialIsland Deserialize(BufferStream stream, TutorialIsland instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TutorialIsland");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.targetPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.disconnectDuration = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.spawnLocationIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.tutorialDuration = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TutorialIsland DeserializeLengthDelimited(BufferStream stream, TutorialIsland instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TutorialIsland");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.targetPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.disconnectDuration = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.spawnLocationIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.tutorialDuration = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TutorialIsland DeserializeLength(BufferStream stream, int length, TutorialIsland instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TutorialIsland");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.targetPlayer = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.disconnectDuration = ProtocolParser.ReadSingle(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.spawnLocationIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				instance.tutorialDuration = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TutorialIsland");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TutorialIsland instance, TutorialIsland previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.targetPlayer.Value);
		if (instance.disconnectDuration != previous.disconnectDuration)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.disconnectDuration);
		}
		if (instance.spawnLocationIndex != previous.spawnLocationIndex)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.spawnLocationIndex);
		}
		if (instance.tutorialDuration != previous.tutorialDuration)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.tutorialDuration);
		}
	}

	public static void Serialize(BufferStream stream, TutorialIsland instance)
	{
		if (instance.targetPlayer != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.targetPlayer.Value);
		}
		if (instance.disconnectDuration != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.disconnectDuration);
		}
		if (instance.spawnLocationIndex != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.spawnLocationIndex);
		}
		if (instance.tutorialDuration != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.tutorialDuration);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref targetPlayer.Value);
	}
}
