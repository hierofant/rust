using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ConnectedSpeaker : IDisposable, Pool.IPooled, IProto<ConnectedSpeaker>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId connectedTo;

	public static void ResetToPool(ConnectedSpeaker instance)
	{
		if (instance.ShouldPool)
		{
			instance.connectedTo = default(NetworkableId);
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
			throw new Exception("Trying to dispose ConnectedSpeaker with ShouldPool set to false!");
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

	public void CopyTo(ConnectedSpeaker instance)
	{
		instance.connectedTo = connectedTo;
	}

	public ConnectedSpeaker Copy()
	{
		ConnectedSpeaker connectedSpeaker = Pool.Get<ConnectedSpeaker>();
		CopyTo(connectedSpeaker);
		return connectedSpeaker;
	}

	public static ConnectedSpeaker Deserialize(BufferStream stream)
	{
		ConnectedSpeaker connectedSpeaker = Pool.Get<ConnectedSpeaker>();
		Deserialize(stream, connectedSpeaker, isDelta: false);
		return connectedSpeaker;
	}

	public static ConnectedSpeaker DeserializeLengthDelimited(BufferStream stream)
	{
		ConnectedSpeaker connectedSpeaker = Pool.Get<ConnectedSpeaker>();
		DeserializeLengthDelimited(stream, connectedSpeaker, isDelta: false);
		return connectedSpeaker;
	}

	public static ConnectedSpeaker DeserializeLength(BufferStream stream, int length)
	{
		ConnectedSpeaker connectedSpeaker = Pool.Get<ConnectedSpeaker>();
		DeserializeLength(stream, length, connectedSpeaker, isDelta: false);
		return connectedSpeaker;
	}

	public static ConnectedSpeaker Deserialize(byte[] buffer)
	{
		ConnectedSpeaker connectedSpeaker = Pool.Get<ConnectedSpeaker>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, connectedSpeaker, isDelta: false);
		return connectedSpeaker;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ConnectedSpeaker previous)
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

	public static ConnectedSpeaker Deserialize(BufferStream stream, ConnectedSpeaker instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ConnectedSpeaker");
			if (num == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ConnectedSpeaker");
				instance.connectedTo = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ConnectedSpeaker");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ConnectedSpeaker");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ConnectedSpeaker DeserializeLengthDelimited(BufferStream stream, ConnectedSpeaker instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ConnectedSpeaker");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ConnectedSpeaker");
				instance.connectedTo = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ConnectedSpeaker");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ConnectedSpeaker");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ConnectedSpeaker DeserializeLength(BufferStream stream, int length, ConnectedSpeaker instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ConnectedSpeaker");
			if (num2 == 8)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ConnectedSpeaker");
				instance.connectedTo = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ConnectedSpeaker");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ConnectedSpeaker");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ConnectedSpeaker instance, ConnectedSpeaker previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.connectedTo.Value);
	}

	public static void Serialize(BufferStream stream, ConnectedSpeaker instance)
	{
		if (instance.connectedTo != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.connectedTo.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref connectedTo.Value);
	}
}
