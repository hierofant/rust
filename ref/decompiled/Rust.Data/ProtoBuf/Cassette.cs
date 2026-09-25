using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class Cassette : IDisposable, Pool.IPooled, IProto<Cassette>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint audioId;

	[NonSerialized]
	public NetworkableId holder;

	[NonSerialized]
	public ulong creatorSteamId;

	[NonSerialized]
	public int preloadAudioId;

	public static void ResetToPool(Cassette instance)
	{
		if (instance.ShouldPool)
		{
			instance.audioId = 0u;
			instance.holder = default(NetworkableId);
			instance.creatorSteamId = 0uL;
			instance.preloadAudioId = 0;
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
			throw new Exception("Trying to dispose Cassette with ShouldPool set to false!");
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

	public void CopyTo(Cassette instance)
	{
		instance.audioId = audioId;
		instance.holder = holder;
		instance.creatorSteamId = creatorSteamId;
		instance.preloadAudioId = preloadAudioId;
	}

	public Cassette Copy()
	{
		Cassette cassette = Pool.Get<Cassette>();
		CopyTo(cassette);
		return cassette;
	}

	public static Cassette Deserialize(BufferStream stream)
	{
		Cassette cassette = Pool.Get<Cassette>();
		Deserialize(stream, cassette, isDelta: false);
		return cassette;
	}

	public static Cassette DeserializeLengthDelimited(BufferStream stream)
	{
		Cassette cassette = Pool.Get<Cassette>();
		DeserializeLengthDelimited(stream, cassette, isDelta: false);
		return cassette;
	}

	public static Cassette DeserializeLength(BufferStream stream, int length)
	{
		Cassette cassette = Pool.Get<Cassette>();
		DeserializeLength(stream, length, cassette, isDelta: false);
		return cassette;
	}

	public static Cassette Deserialize(byte[] buffer)
	{
		Cassette cassette = Pool.Get<Cassette>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, cassette, isDelta: false);
		return cassette;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, Cassette previous)
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

	public static Cassette Deserialize(BufferStream stream, Cassette instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Cassette");
			switch (num)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.audioId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.holder = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.creatorSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.preloadAudioId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Cassette DeserializeLengthDelimited(BufferStream stream, Cassette instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Cassette");
			switch (num2)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.audioId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.holder = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.creatorSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.preloadAudioId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static Cassette DeserializeLength(BufferStream stream, int length, Cassette instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Cassette");
			switch (num2)
			{
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.audioId = ProtocolParser.ReadUInt32(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.holder = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.creatorSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				instance.preloadAudioId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Cassette");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, Cassette instance, Cassette previous)
	{
		if (instance.audioId != previous.audioId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.audioId);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteUInt64(stream, instance.holder.Value);
		if (instance.creatorSteamId != previous.creatorSteamId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.creatorSteamId);
		}
		if (instance.preloadAudioId != previous.preloadAudioId)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.preloadAudioId);
		}
	}

	public static void Serialize(BufferStream stream, Cassette instance)
	{
		if (instance.audioId != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt32(stream, instance.audioId);
		}
		if (instance.holder != default(NetworkableId))
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.holder.Value);
		}
		if (instance.creatorSteamId != 0L)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.creatorSteamId);
		}
		if (instance.preloadAudioId != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.preloadAudioId);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref holder.Value);
	}
}
