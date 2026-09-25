using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class TemporaryRagdoll : IDisposable, Pool.IPooled, IProto<TemporaryRagdoll>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId parentID;

	[NonSerialized]
	public int mountPose;

	public static void ResetToPool(TemporaryRagdoll instance)
	{
		if (instance.ShouldPool)
		{
			instance.parentID = default(NetworkableId);
			instance.mountPose = 0;
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
			throw new Exception("Trying to dispose TemporaryRagdoll with ShouldPool set to false!");
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

	public void CopyTo(TemporaryRagdoll instance)
	{
		instance.parentID = parentID;
		instance.mountPose = mountPose;
	}

	public TemporaryRagdoll Copy()
	{
		TemporaryRagdoll temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		CopyTo(temporaryRagdoll);
		return temporaryRagdoll;
	}

	public static TemporaryRagdoll Deserialize(BufferStream stream)
	{
		TemporaryRagdoll temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		Deserialize(stream, temporaryRagdoll, isDelta: false);
		return temporaryRagdoll;
	}

	public static TemporaryRagdoll DeserializeLengthDelimited(BufferStream stream)
	{
		TemporaryRagdoll temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		DeserializeLengthDelimited(stream, temporaryRagdoll, isDelta: false);
		return temporaryRagdoll;
	}

	public static TemporaryRagdoll DeserializeLength(BufferStream stream, int length)
	{
		TemporaryRagdoll temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		DeserializeLength(stream, length, temporaryRagdoll, isDelta: false);
		return temporaryRagdoll;
	}

	public static TemporaryRagdoll Deserialize(byte[] buffer)
	{
		TemporaryRagdoll temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, temporaryRagdoll, isDelta: false);
		return temporaryRagdoll;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, TemporaryRagdoll previous)
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

	public static TemporaryRagdoll Deserialize(BufferStream stream, TemporaryRagdoll instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.TemporaryRagdoll");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				instance.mountPose = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TemporaryRagdoll DeserializeLengthDelimited(BufferStream stream, TemporaryRagdoll instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TemporaryRagdoll");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				instance.mountPose = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static TemporaryRagdoll DeserializeLength(BufferStream stream, int length, TemporaryRagdoll instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.TemporaryRagdoll");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				instance.parentID = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				instance.mountPose = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.TemporaryRagdoll");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, TemporaryRagdoll instance, TemporaryRagdoll previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.parentID.Value);
		if (instance.mountPose != previous.mountPose)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.mountPose);
		}
	}

	public static void Serialize(BufferStream stream, TemporaryRagdoll instance)
	{
		if (instance.parentID != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.parentID.Value);
		}
		if (instance.mountPose != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.mountPose);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref parentID.Value);
	}
}
