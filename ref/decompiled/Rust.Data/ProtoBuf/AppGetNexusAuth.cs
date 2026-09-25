using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppGetNexusAuth : IDisposable, Pool.IPooled, IProto<AppGetNexusAuth>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string appKey;

	public static void ResetToPool(AppGetNexusAuth instance)
	{
		if (instance.ShouldPool)
		{
			instance.appKey = string.Empty;
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
			throw new Exception("Trying to dispose AppGetNexusAuth with ShouldPool set to false!");
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

	public void CopyTo(AppGetNexusAuth instance)
	{
		instance.appKey = appKey;
	}

	public AppGetNexusAuth Copy()
	{
		AppGetNexusAuth appGetNexusAuth = Pool.Get<AppGetNexusAuth>();
		CopyTo(appGetNexusAuth);
		return appGetNexusAuth;
	}

	public static AppGetNexusAuth Deserialize(BufferStream stream)
	{
		AppGetNexusAuth appGetNexusAuth = Pool.Get<AppGetNexusAuth>();
		Deserialize(stream, appGetNexusAuth, isDelta: false);
		return appGetNexusAuth;
	}

	public static AppGetNexusAuth DeserializeLengthDelimited(BufferStream stream)
	{
		AppGetNexusAuth appGetNexusAuth = Pool.Get<AppGetNexusAuth>();
		DeserializeLengthDelimited(stream, appGetNexusAuth, isDelta: false);
		return appGetNexusAuth;
	}

	public static AppGetNexusAuth DeserializeLength(BufferStream stream, int length)
	{
		AppGetNexusAuth appGetNexusAuth = Pool.Get<AppGetNexusAuth>();
		DeserializeLength(stream, length, appGetNexusAuth, isDelta: false);
		return appGetNexusAuth;
	}

	public static AppGetNexusAuth Deserialize(byte[] buffer)
	{
		AppGetNexusAuth appGetNexusAuth = Pool.Get<AppGetNexusAuth>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appGetNexusAuth, isDelta: false);
		return appGetNexusAuth;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppGetNexusAuth previous)
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

	public static AppGetNexusAuth Deserialize(BufferStream stream, AppGetNexusAuth instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppGetNexusAuth");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppGetNexusAuth");
				instance.appKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppGetNexusAuth");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppGetNexusAuth");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppGetNexusAuth DeserializeLengthDelimited(BufferStream stream, AppGetNexusAuth instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppGetNexusAuth");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppGetNexusAuth");
				instance.appKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppGetNexusAuth");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppGetNexusAuth");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static AppGetNexusAuth DeserializeLength(BufferStream stream, int length, AppGetNexusAuth instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.AppGetNexusAuth");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppGetNexusAuth");
				instance.appKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppGetNexusAuth");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppGetNexusAuth");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppGetNexusAuth instance, AppGetNexusAuth previous)
	{
		if (instance.appKey != previous.appKey)
		{
			if (instance.appKey == null)
			{
				throw new ArgumentNullException("appKey", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.appKey);
		}
	}

	public static void Serialize(BufferStream stream, AppGetNexusAuth instance)
	{
		if (instance.appKey == null)
		{
			throw new ArgumentNullException("appKey", "Required by proto specification.");
		}
		stream.WriteByte(10);
		ProtocolParser.WriteString(stream, instance.appKey);
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
	}
}
