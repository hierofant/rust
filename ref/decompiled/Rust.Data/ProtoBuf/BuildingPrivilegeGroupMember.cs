using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BuildingPrivilegeGroupMember : IDisposable, Pool.IPooled, IProto<BuildingPrivilegeGroupMember>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong userid;

	[NonSerialized]
	public uint lastCountedTime;

	public static void ResetToPool(BuildingPrivilegeGroupMember instance)
	{
		if (instance.ShouldPool)
		{
			instance.userid = 0uL;
			instance.lastCountedTime = 0u;
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
			throw new Exception("Trying to dispose BuildingPrivilegeGroupMember with ShouldPool set to false!");
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

	public void CopyTo(BuildingPrivilegeGroupMember instance)
	{
		instance.userid = userid;
		instance.lastCountedTime = lastCountedTime;
	}

	public BuildingPrivilegeGroupMember Copy()
	{
		BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = Pool.Get<BuildingPrivilegeGroupMember>();
		CopyTo(buildingPrivilegeGroupMember);
		return buildingPrivilegeGroupMember;
	}

	public static BuildingPrivilegeGroupMember Deserialize(BufferStream stream)
	{
		BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = Pool.Get<BuildingPrivilegeGroupMember>();
		Deserialize(stream, buildingPrivilegeGroupMember, isDelta: false);
		return buildingPrivilegeGroupMember;
	}

	public static BuildingPrivilegeGroupMember DeserializeLengthDelimited(BufferStream stream)
	{
		BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = Pool.Get<BuildingPrivilegeGroupMember>();
		DeserializeLengthDelimited(stream, buildingPrivilegeGroupMember, isDelta: false);
		return buildingPrivilegeGroupMember;
	}

	public static BuildingPrivilegeGroupMember DeserializeLength(BufferStream stream, int length)
	{
		BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = Pool.Get<BuildingPrivilegeGroupMember>();
		DeserializeLength(stream, length, buildingPrivilegeGroupMember, isDelta: false);
		return buildingPrivilegeGroupMember;
	}

	public static BuildingPrivilegeGroupMember Deserialize(byte[] buffer)
	{
		BuildingPrivilegeGroupMember buildingPrivilegeGroupMember = Pool.Get<BuildingPrivilegeGroupMember>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, buildingPrivilegeGroupMember, isDelta: false);
		return buildingPrivilegeGroupMember;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BuildingPrivilegeGroupMember previous)
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

	public static BuildingPrivilegeGroupMember Deserialize(BufferStream stream, BuildingPrivilegeGroupMember instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeGroupMember");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				instance.lastCountedTime = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilegeGroupMember DeserializeLengthDelimited(BufferStream stream, BuildingPrivilegeGroupMember instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeGroupMember");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				instance.lastCountedTime = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilegeGroupMember DeserializeLength(BufferStream stream, int length, BuildingPrivilegeGroupMember instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeGroupMember");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				instance.userid = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				instance.lastCountedTime = ProtocolParser.ReadUInt32(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeGroupMember");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BuildingPrivilegeGroupMember instance, BuildingPrivilegeGroupMember previous)
	{
		if (instance.userid != previous.userid)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userid);
		}
		if (instance.lastCountedTime != previous.lastCountedTime)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.lastCountedTime);
		}
	}

	public static void Serialize(BufferStream stream, BuildingPrivilegeGroupMember instance)
	{
		if (instance.userid != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.userid);
		}
		if (instance.lastCountedTime != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt32(stream, instance.lastCountedTime);
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
