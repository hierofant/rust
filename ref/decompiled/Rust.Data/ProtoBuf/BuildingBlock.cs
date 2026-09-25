using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class BuildingBlock : IDisposable, Pool.IPooled, IProto<BuildingBlock>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public ulong model;

	[NonSerialized]
	public int grade;

	[NonSerialized]
	public bool beingDemolished;

	[NonSerialized]
	public ulong wallpaperID;

	[NonSerialized]
	public float wallpaperHealth;

	[NonSerialized]
	public ulong wallpaperID2;

	[NonSerialized]
	public float wallpaperHealth2;

	[NonSerialized]
	public float wallpaperRotation;

	[NonSerialized]
	public float wallpaperRotation2;

	public static void ResetToPool(BuildingBlock instance)
	{
		if (instance.ShouldPool)
		{
			instance.model = 0uL;
			instance.grade = 0;
			instance.beingDemolished = false;
			instance.wallpaperID = 0uL;
			instance.wallpaperHealth = 0f;
			instance.wallpaperID2 = 0uL;
			instance.wallpaperHealth2 = 0f;
			instance.wallpaperRotation = 0f;
			instance.wallpaperRotation2 = 0f;
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
			throw new Exception("Trying to dispose BuildingBlock with ShouldPool set to false!");
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

	public void CopyTo(BuildingBlock instance)
	{
		instance.model = model;
		instance.grade = grade;
		instance.beingDemolished = beingDemolished;
		instance.wallpaperID = wallpaperID;
		instance.wallpaperHealth = wallpaperHealth;
		instance.wallpaperID2 = wallpaperID2;
		instance.wallpaperHealth2 = wallpaperHealth2;
		instance.wallpaperRotation = wallpaperRotation;
		instance.wallpaperRotation2 = wallpaperRotation2;
	}

	public BuildingBlock Copy()
	{
		BuildingBlock buildingBlock = Pool.Get<BuildingBlock>();
		CopyTo(buildingBlock);
		return buildingBlock;
	}

	public static BuildingBlock Deserialize(BufferStream stream)
	{
		BuildingBlock buildingBlock = Pool.Get<BuildingBlock>();
		Deserialize(stream, buildingBlock, isDelta: false);
		return buildingBlock;
	}

	public static BuildingBlock DeserializeLengthDelimited(BufferStream stream)
	{
		BuildingBlock buildingBlock = Pool.Get<BuildingBlock>();
		DeserializeLengthDelimited(stream, buildingBlock, isDelta: false);
		return buildingBlock;
	}

	public static BuildingBlock DeserializeLength(BufferStream stream, int length)
	{
		BuildingBlock buildingBlock = Pool.Get<BuildingBlock>();
		DeserializeLength(stream, length, buildingBlock, isDelta: false);
		return buildingBlock;
	}

	public static BuildingBlock Deserialize(byte[] buffer)
	{
		BuildingBlock buildingBlock = Pool.Get<BuildingBlock>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, buildingBlock, isDelta: false);
		return buildingBlock;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, BuildingBlock previous)
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

	public static BuildingBlock Deserialize(BufferStream stream, BuildingBlock instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.BuildingBlock");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.model = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.grade = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.beingDemolished = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperHealth = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperID2 = ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperHealth2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperRotation = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperRotation2 = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingBlock DeserializeLengthDelimited(BufferStream stream, BuildingBlock instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingBlock");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.model = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.grade = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.beingDemolished = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperHealth = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperID2 = ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperHealth2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperRotation = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperRotation2 = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static BuildingBlock DeserializeLength(BufferStream stream, int length, BuildingBlock instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.BuildingBlock");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.model = ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.grade = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.beingDemolished = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 45:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperHealth = ProtocolParser.ReadSingle(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperID2 = ProtocolParser.ReadUInt64(stream);
				continue;
			case 61:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperHealth2 = ProtocolParser.ReadSingle(stream);
				continue;
			case 69:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperRotation = ProtocolParser.ReadSingle(stream);
				continue;
			case 77:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				instance.wallpaperRotation2 = ProtocolParser.ReadSingle(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.BuildingBlock");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, BuildingBlock instance, BuildingBlock previous)
	{
		if (instance.model != previous.model)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.model);
		}
		if (instance.grade != previous.grade)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.grade);
		}
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.beingDemolished);
		if (instance.wallpaperID != previous.wallpaperID)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.wallpaperID);
		}
		if (instance.wallpaperHealth != previous.wallpaperHealth)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.wallpaperHealth);
		}
		if (instance.wallpaperID2 != previous.wallpaperID2)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.wallpaperID2);
		}
		if (instance.wallpaperHealth2 != previous.wallpaperHealth2)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.wallpaperHealth2);
		}
		if (instance.wallpaperRotation != previous.wallpaperRotation)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.wallpaperRotation);
		}
		if (instance.wallpaperRotation2 != previous.wallpaperRotation2)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.wallpaperRotation2);
		}
	}

	public static void Serialize(BufferStream stream, BuildingBlock instance)
	{
		if (instance.model != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.model);
		}
		if (instance.grade != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.grade);
		}
		if (instance.beingDemolished)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.beingDemolished);
		}
		if (instance.wallpaperID != 0L)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.wallpaperID);
		}
		if (instance.wallpaperHealth != 0f)
		{
			stream.WriteByte(45);
			ProtocolParser.WriteSingle(stream, instance.wallpaperHealth);
		}
		if (instance.wallpaperID2 != 0L)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.wallpaperID2);
		}
		if (instance.wallpaperHealth2 != 0f)
		{
			stream.WriteByte(61);
			ProtocolParser.WriteSingle(stream, instance.wallpaperHealth2);
		}
		if (instance.wallpaperRotation != 0f)
		{
			stream.WriteByte(69);
			ProtocolParser.WriteSingle(stream, instance.wallpaperRotation);
		}
		if (instance.wallpaperRotation2 != 0f)
		{
			stream.WriteByte(77);
			ProtocolParser.WriteSingle(stream, instance.wallpaperRotation2);
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
