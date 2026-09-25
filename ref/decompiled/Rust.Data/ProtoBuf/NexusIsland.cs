using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class NexusIsland : IDisposable, Pool.IPooled, IProto<NexusIsland>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string zoneKey;

	public static void ResetToPool(NexusIsland instance)
	{
		if (instance.ShouldPool)
		{
			instance.zoneKey = string.Empty;
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
			throw new Exception("Trying to dispose NexusIsland with ShouldPool set to false!");
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

	public void CopyTo(NexusIsland instance)
	{
		instance.zoneKey = zoneKey;
	}

	public NexusIsland Copy()
	{
		NexusIsland nexusIsland = Pool.Get<NexusIsland>();
		CopyTo(nexusIsland);
		return nexusIsland;
	}

	public static NexusIsland Deserialize(BufferStream stream)
	{
		NexusIsland nexusIsland = Pool.Get<NexusIsland>();
		Deserialize(stream, nexusIsland, isDelta: false);
		return nexusIsland;
	}

	public static NexusIsland DeserializeLengthDelimited(BufferStream stream)
	{
		NexusIsland nexusIsland = Pool.Get<NexusIsland>();
		DeserializeLengthDelimited(stream, nexusIsland, isDelta: false);
		return nexusIsland;
	}

	public static NexusIsland DeserializeLength(BufferStream stream, int length)
	{
		NexusIsland nexusIsland = Pool.Get<NexusIsland>();
		DeserializeLength(stream, length, nexusIsland, isDelta: false);
		return nexusIsland;
	}

	public static NexusIsland Deserialize(byte[] buffer)
	{
		NexusIsland nexusIsland = Pool.Get<NexusIsland>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, nexusIsland, isDelta: false);
		return nexusIsland;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NexusIsland previous)
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

	public static NexusIsland Deserialize(BufferStream stream, NexusIsland instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NexusIsland");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusIsland");
				instance.zoneKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusIsland");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusIsland");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static NexusIsland DeserializeLengthDelimited(BufferStream stream, NexusIsland instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.NexusIsland");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusIsland");
				instance.zoneKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusIsland");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusIsland");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static NexusIsland DeserializeLength(BufferStream stream, int length, NexusIsland instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.NexusIsland");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusIsland");
				instance.zoneKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusIsland");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusIsland");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NexusIsland instance, NexusIsland previous)
	{
		if (instance.zoneKey != null && instance.zoneKey != previous.zoneKey)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.zoneKey);
		}
	}

	public static void Serialize(BufferStream stream, NexusIsland instance)
	{
		if (instance.zoneKey != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.zoneKey);
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
