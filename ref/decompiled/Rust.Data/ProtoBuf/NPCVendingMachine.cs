using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class NPCVendingMachine : IDisposable, Pool.IPooled, IProto<NPCVendingMachine>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId attachedNpc;

	[NonSerialized]
	public float nextRefresh;

	public static void ResetToPool(NPCVendingMachine instance)
	{
		if (instance.ShouldPool)
		{
			instance.attachedNpc = default(NetworkableId);
			instance.nextRefresh = 0f;
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
			throw new Exception("Trying to dispose NPCVendingMachine with ShouldPool set to false!");
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

	public void CopyTo(NPCVendingMachine instance)
	{
		instance.attachedNpc = attachedNpc;
		instance.nextRefresh = nextRefresh;
	}

	public NPCVendingMachine Copy()
	{
		NPCVendingMachine nPCVendingMachine = Pool.Get<NPCVendingMachine>();
		CopyTo(nPCVendingMachine);
		return nPCVendingMachine;
	}

	public static NPCVendingMachine Deserialize(BufferStream stream)
	{
		NPCVendingMachine nPCVendingMachine = Pool.Get<NPCVendingMachine>();
		Deserialize(stream, nPCVendingMachine, isDelta: false);
		return nPCVendingMachine;
	}

	public static NPCVendingMachine DeserializeLengthDelimited(BufferStream stream)
	{
		NPCVendingMachine nPCVendingMachine = Pool.Get<NPCVendingMachine>();
		DeserializeLengthDelimited(stream, nPCVendingMachine, isDelta: false);
		return nPCVendingMachine;
	}

	public static NPCVendingMachine DeserializeLength(BufferStream stream, int length)
	{
		NPCVendingMachine nPCVendingMachine = Pool.Get<NPCVendingMachine>();
		DeserializeLength(stream, length, nPCVendingMachine, isDelta: false);
		return nPCVendingMachine;
	}

	public static NPCVendingMachine Deserialize(byte[] buffer)
	{
		NPCVendingMachine nPCVendingMachine = Pool.Get<NPCVendingMachine>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, nPCVendingMachine, isDelta: false);
		return nPCVendingMachine;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NPCVendingMachine previous)
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

	public static NPCVendingMachine Deserialize(BufferStream stream, NPCVendingMachine instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NPCVendingMachine");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				instance.attachedNpc = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				instance.nextRefresh = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NPCVendingMachine DeserializeLengthDelimited(BufferStream stream, NPCVendingMachine instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.NPCVendingMachine");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				instance.attachedNpc = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				instance.nextRefresh = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NPCVendingMachine DeserializeLength(BufferStream stream, int length, NPCVendingMachine instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.NPCVendingMachine");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				instance.attachedNpc = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 21:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				instance.nextRefresh = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NPCVendingMachine");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NPCVendingMachine instance, NPCVendingMachine previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.attachedNpc.Value);
		if (instance.nextRefresh != previous.nextRefresh)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.nextRefresh);
		}
	}

	public static void Serialize(BufferStream stream, NPCVendingMachine instance)
	{
		if (instance.attachedNpc != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.attachedNpc.Value);
		}
		if (instance.nextRefresh != 0f)
		{
			stream.WriteByte(21);
			ProtocolParser.WriteSingle(stream, instance.nextRefresh);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref attachedNpc.Value);
	}
}
