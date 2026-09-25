using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BuildingPrivilegeRetroTool : IDisposable, Pool.IPooled, IProto<BuildingPrivilegeRetroTool>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public int itemID;

	[NonSerialized]
	public ulong skinid;

	public static void ResetToPool(BuildingPrivilegeRetroTool instance)
	{
		if (instance.ShouldPool)
		{
			instance.itemID = 0;
			instance.skinid = 0uL;
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
			throw new Exception("Trying to dispose BuildingPrivilegeRetroTool with ShouldPool set to false!");
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

	public void CopyTo(BuildingPrivilegeRetroTool instance)
	{
		instance.itemID = itemID;
		instance.skinid = skinid;
	}

	public BuildingPrivilegeRetroTool Copy()
	{
		BuildingPrivilegeRetroTool buildingPrivilegeRetroTool = Pool.Get<BuildingPrivilegeRetroTool>();
		CopyTo(buildingPrivilegeRetroTool);
		return buildingPrivilegeRetroTool;
	}

	public static BuildingPrivilegeRetroTool Deserialize(BufferStream stream)
	{
		BuildingPrivilegeRetroTool buildingPrivilegeRetroTool = Pool.Get<BuildingPrivilegeRetroTool>();
		Deserialize(stream, buildingPrivilegeRetroTool, isDelta: false);
		return buildingPrivilegeRetroTool;
	}

	public static BuildingPrivilegeRetroTool DeserializeLengthDelimited(BufferStream stream)
	{
		BuildingPrivilegeRetroTool buildingPrivilegeRetroTool = Pool.Get<BuildingPrivilegeRetroTool>();
		DeserializeLengthDelimited(stream, buildingPrivilegeRetroTool, isDelta: false);
		return buildingPrivilegeRetroTool;
	}

	public static BuildingPrivilegeRetroTool DeserializeLength(BufferStream stream, int length)
	{
		BuildingPrivilegeRetroTool buildingPrivilegeRetroTool = Pool.Get<BuildingPrivilegeRetroTool>();
		DeserializeLength(stream, length, buildingPrivilegeRetroTool, isDelta: false);
		return buildingPrivilegeRetroTool;
	}

	public static BuildingPrivilegeRetroTool Deserialize(byte[] buffer)
	{
		BuildingPrivilegeRetroTool buildingPrivilegeRetroTool = Pool.Get<BuildingPrivilegeRetroTool>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, buildingPrivilegeRetroTool, isDelta: false);
		return buildingPrivilegeRetroTool;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BuildingPrivilegeRetroTool previous)
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

	public static BuildingPrivilegeRetroTool Deserialize(BufferStream stream, BuildingPrivilegeRetroTool instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeRetroTool");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				instance.skinid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilegeRetroTool DeserializeLengthDelimited(BufferStream stream, BuildingPrivilegeRetroTool instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeRetroTool");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				instance.skinid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingPrivilegeRetroTool DeserializeLength(BufferStream stream, int length, BuildingPrivilegeRetroTool instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingPrivilegeRetroTool");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				instance.itemID = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				instance.skinid = ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingPrivilegeRetroTool");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BuildingPrivilegeRetroTool instance, BuildingPrivilegeRetroTool previous)
	{
		if (instance.itemID != previous.itemID)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
		}
		if (instance.skinid != previous.skinid)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.skinid);
		}
	}

	public static void Serialize(BufferStream stream, BuildingPrivilegeRetroTool instance)
	{
		if (instance.itemID != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.itemID);
		}
		if (instance.skinid != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.skinid);
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
