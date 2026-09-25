using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class WantedPoster : IDisposable, Pool.IPooled, IProto<WantedPoster>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public uint imageCrc;

	[NonSerialized]
	public string playerName;

	[NonSerialized]
	public ulong playerId;

	public static void ResetToPool(WantedPoster instance)
	{
		if (instance.ShouldPool)
		{
			instance.imageCrc = 0u;
			instance.playerName = string.Empty;
			instance.playerId = 0uL;
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
			throw new Exception("Trying to dispose WantedPoster with ShouldPool set to false!");
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

	public void CopyTo(WantedPoster instance)
	{
		instance.imageCrc = imageCrc;
		instance.playerName = playerName;
		instance.playerId = playerId;
	}

	public WantedPoster Copy()
	{
		WantedPoster wantedPoster = Pool.Get<WantedPoster>();
		CopyTo(wantedPoster);
		return wantedPoster;
	}

	public static WantedPoster Deserialize(BufferStream stream)
	{
		WantedPoster wantedPoster = Pool.Get<WantedPoster>();
		Deserialize(stream, wantedPoster, isDelta: false);
		return wantedPoster;
	}

	public static WantedPoster DeserializeLengthDelimited(BufferStream stream)
	{
		WantedPoster wantedPoster = Pool.Get<WantedPoster>();
		DeserializeLengthDelimited(stream, wantedPoster, isDelta: false);
		return wantedPoster;
	}

	public static WantedPoster DeserializeLength(BufferStream stream, int length)
	{
		WantedPoster wantedPoster = Pool.Get<WantedPoster>();
		DeserializeLength(stream, length, wantedPoster, isDelta: false);
		return wantedPoster;
	}

	public static WantedPoster Deserialize(byte[] buffer)
	{
		WantedPoster wantedPoster = Pool.Get<WantedPoster>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, wantedPoster, isDelta: false);
		return wantedPoster;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, WantedPoster previous)
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

	public static WantedPoster Deserialize(BufferStream stream, WantedPoster instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.WantedPoster");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.imageCrc = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WantedPoster DeserializeLengthDelimited(BufferStream stream, WantedPoster instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WantedPoster");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.imageCrc = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static WantedPoster DeserializeLength(BufferStream stream, int length, WantedPoster instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.WantedPoster");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.imageCrc = ProtocolParser.ReadUInt32(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.playerName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				instance.playerId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.WantedPoster");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, WantedPoster instance, WantedPoster previous)
	{
		if (instance.imageCrc != previous.imageCrc)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.imageCrc);
		}
		if (instance.playerName != null && instance.playerName != previous.playerName)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.playerName);
		}
		if (instance.playerId != previous.playerId)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.playerId);
		}
	}

	public static void Serialize(BufferStream stream, WantedPoster instance)
	{
		if (instance.imageCrc != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt32(stream, instance.imageCrc);
		}
		if (instance.playerName != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.playerName);
		}
		if (instance.playerId != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.playerId);
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
