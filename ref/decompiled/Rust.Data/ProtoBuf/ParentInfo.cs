using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ParentInfo : IDisposable, Pool.IPooled, IProto<ParentInfo>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId uid;

	[NonSerialized]
	public uint bone;

	public static void ResetToPool(ParentInfo instance)
	{
		if (instance.ShouldPool)
		{
			instance.uid = default(NetworkableId);
			instance.bone = 0u;
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
			throw new Exception("Trying to dispose ParentInfo with ShouldPool set to false!");
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

	public void CopyTo(ParentInfo instance)
	{
		instance.uid = uid;
		instance.bone = bone;
	}

	public ParentInfo Copy()
	{
		ParentInfo parentInfo = Pool.Get<ParentInfo>();
		CopyTo(parentInfo);
		return parentInfo;
	}

	public static ParentInfo Deserialize(BufferStream stream)
	{
		ParentInfo parentInfo = Pool.Get<ParentInfo>();
		Deserialize(stream, parentInfo, isDelta: false);
		return parentInfo;
	}

	public static ParentInfo DeserializeLengthDelimited(BufferStream stream)
	{
		ParentInfo parentInfo = Pool.Get<ParentInfo>();
		DeserializeLengthDelimited(stream, parentInfo, isDelta: false);
		return parentInfo;
	}

	public static ParentInfo DeserializeLength(BufferStream stream, int length)
	{
		ParentInfo parentInfo = Pool.Get<ParentInfo>();
		DeserializeLength(stream, length, parentInfo, isDelta: false);
		return parentInfo;
	}

	public static ParentInfo Deserialize(byte[] buffer)
	{
		ParentInfo parentInfo = Pool.Get<ParentInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, parentInfo, isDelta: false);
		return parentInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ParentInfo previous)
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

	public static ParentInfo Deserialize(BufferStream stream, ParentInfo instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ParentInfo");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				instance.uid = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				instance.bone = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ParentInfo DeserializeLengthDelimited(BufferStream stream, ParentInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ParentInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				instance.uid = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				instance.bone = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ParentInfo DeserializeLength(BufferStream stream, int length, ParentInfo instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.ParentInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				instance.uid = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				instance.bone = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ParentInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ParentInfo instance, ParentInfo previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.uid.Value);
		if (instance.bone != previous.bone)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.bone);
		}
	}

	public static void Serialize(BufferStream stream, ParentInfo instance)
	{
		if (instance.uid != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.uid.Value);
		}
		if (instance.bone != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.bone);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref uid.Value);
	}
}
