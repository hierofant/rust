using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppTeamKick : IDisposable, Pool.IPooled, IProto<AppTeamKick>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong steamId;

	public static void ResetToPool(AppTeamKick instance)
	{
		if (instance.ShouldPool)
		{
			instance.steamId = 0uL;
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
			throw new Exception("Trying to dispose AppTeamKick with ShouldPool set to false!");
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

	public void CopyTo(AppTeamKick instance)
	{
		instance.steamId = steamId;
	}

	public AppTeamKick Copy()
	{
		AppTeamKick appTeamKick = Pool.Get<AppTeamKick>();
		CopyTo(appTeamKick);
		return appTeamKick;
	}

	public static AppTeamKick Deserialize(BufferStream stream)
	{
		AppTeamKick appTeamKick = Pool.Get<AppTeamKick>();
		Deserialize(stream, appTeamKick, isDelta: false);
		return appTeamKick;
	}

	public static AppTeamKick DeserializeLengthDelimited(BufferStream stream)
	{
		AppTeamKick appTeamKick = Pool.Get<AppTeamKick>();
		DeserializeLengthDelimited(stream, appTeamKick, isDelta: false);
		return appTeamKick;
	}

	public static AppTeamKick DeserializeLength(BufferStream stream, int length)
	{
		AppTeamKick appTeamKick = Pool.Get<AppTeamKick>();
		DeserializeLength(stream, length, appTeamKick, isDelta: false);
		return appTeamKick;
	}

	public static AppTeamKick Deserialize(byte[] buffer)
	{
		AppTeamKick appTeamKick = Pool.Get<AppTeamKick>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appTeamKick, isDelta: false);
		return appTeamKick;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppTeamKick previous)
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

	public static AppTeamKick Deserialize(BufferStream stream, AppTeamKick instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.steamId = 0uL;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamKick");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamKick");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamKick");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamKick");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppTeamKick DeserializeLengthDelimited(BufferStream stream, AppTeamKick instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.steamId = 0uL;
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamKick");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamKick");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamKick");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamKick");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppTeamKick DeserializeLength(BufferStream stream, int length, AppTeamKick instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.steamId = 0uL;
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamKick");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamKick");
				instance.steamId = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamKick");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamKick");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppTeamKick instance, AppTeamKick previous)
	{
		if (instance.steamId != previous.steamId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamId);
		}
	}

	public static void Serialize(BufferStream stream, AppTeamKick instance)
	{
		if (instance.steamId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.steamId);
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
