using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MissionAcceptState : IDisposable, Pool.IPooled, IProto<MissionAcceptState>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId providerNetId;

	[NonSerialized]
	public uint missionID;

	[NonSerialized]
	public bool canAccept;

	public static void ResetToPool(MissionAcceptState instance)
	{
		if (instance.ShouldPool)
		{
			instance.providerNetId = default(NetworkableId);
			instance.missionID = 0u;
			instance.canAccept = false;
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
			throw new Exception("Trying to dispose MissionAcceptState with ShouldPool set to false!");
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

	public void CopyTo(MissionAcceptState instance)
	{
		instance.providerNetId = providerNetId;
		instance.missionID = missionID;
		instance.canAccept = canAccept;
	}

	public MissionAcceptState Copy()
	{
		MissionAcceptState missionAcceptState = Pool.Get<MissionAcceptState>();
		CopyTo(missionAcceptState);
		return missionAcceptState;
	}

	public static MissionAcceptState Deserialize(BufferStream stream)
	{
		MissionAcceptState missionAcceptState = Pool.Get<MissionAcceptState>();
		Deserialize(stream, missionAcceptState, isDelta: false);
		return missionAcceptState;
	}

	public static MissionAcceptState DeserializeLengthDelimited(BufferStream stream)
	{
		MissionAcceptState missionAcceptState = Pool.Get<MissionAcceptState>();
		DeserializeLengthDelimited(stream, missionAcceptState, isDelta: false);
		return missionAcceptState;
	}

	public static MissionAcceptState DeserializeLength(BufferStream stream, int length)
	{
		MissionAcceptState missionAcceptState = Pool.Get<MissionAcceptState>();
		DeserializeLength(stream, length, missionAcceptState, isDelta: false);
		return missionAcceptState;
	}

	public static MissionAcceptState Deserialize(byte[] buffer)
	{
		MissionAcceptState missionAcceptState = Pool.Get<MissionAcceptState>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, missionAcceptState, isDelta: false);
		return missionAcceptState;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MissionAcceptState previous)
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

	public static MissionAcceptState Deserialize(BufferStream stream, MissionAcceptState instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.MissionAcceptState");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.providerNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.missionID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.canAccept = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionAcceptState DeserializeLengthDelimited(BufferStream stream, MissionAcceptState instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionAcceptState");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.providerNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.missionID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.canAccept = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MissionAcceptState DeserializeLength(BufferStream stream, int length, MissionAcceptState instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.MissionAcceptState");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.providerNetId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.missionID = ProtocolParser.ReadUInt32(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				instance.canAccept = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MissionAcceptState");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MissionAcceptState instance, MissionAcceptState previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.providerNetId.Value);
		if (instance.missionID != previous.missionID)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.missionID);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.canAccept);
	}

	public static void Serialize(BufferStream stream, MissionAcceptState instance)
	{
		if (instance.providerNetId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.providerNetId.Value);
		}
		if (instance.missionID != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.missionID);
		}
		if (instance.canAccept)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.canAccept);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref providerNetId.Value);
	}
}
