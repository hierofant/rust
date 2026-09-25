using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ServerGib : IDisposable, Pool.IPooled, IProto<ServerGib>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string gibName;

	public static void ResetToPool(ServerGib instance)
	{
		if (instance.ShouldPool)
		{
			instance.gibName = string.Empty;
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
			throw new Exception("Trying to dispose ServerGib with ShouldPool set to false!");
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

	public void CopyTo(ServerGib instance)
	{
		instance.gibName = gibName;
	}

	public ServerGib Copy()
	{
		ServerGib serverGib = Pool.Get<ServerGib>();
		CopyTo(serverGib);
		return serverGib;
	}

	public static ServerGib Deserialize(BufferStream stream)
	{
		ServerGib serverGib = Pool.Get<ServerGib>();
		Deserialize(stream, serverGib, isDelta: false);
		return serverGib;
	}

	public static ServerGib DeserializeLengthDelimited(BufferStream stream)
	{
		ServerGib serverGib = Pool.Get<ServerGib>();
		DeserializeLengthDelimited(stream, serverGib, isDelta: false);
		return serverGib;
	}

	public static ServerGib DeserializeLength(BufferStream stream, int length)
	{
		ServerGib serverGib = Pool.Get<ServerGib>();
		DeserializeLength(stream, length, serverGib, isDelta: false);
		return serverGib;
	}

	public static ServerGib Deserialize(byte[] buffer)
	{
		ServerGib serverGib = Pool.Get<ServerGib>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, serverGib, isDelta: false);
		return serverGib;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ServerGib previous)
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

	public static ServerGib Deserialize(BufferStream stream, ServerGib instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ServerGib");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServerGib");
				instance.gibName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServerGib");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ServerGib");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ServerGib DeserializeLengthDelimited(BufferStream stream, ServerGib instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ServerGib");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServerGib");
				instance.gibName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServerGib");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ServerGib");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ServerGib DeserializeLength(BufferStream stream, int length, ServerGib instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ServerGib");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServerGib");
				instance.gibName = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ServerGib");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ServerGib");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ServerGib instance, ServerGib previous)
	{
		if (instance.gibName != null && instance.gibName != previous.gibName)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.gibName);
		}
	}

	public static void Serialize(BufferStream stream, ServerGib instance)
	{
		if (instance.gibName != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.gibName);
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
