using System;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class SleepingBag : IDisposable, Pool.IPooled, IProto<SleepingBag>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public string name;

	[NonSerialized]
	public ulong deployerID;

	[NonSerialized]
	public bool clientAssigned;

	[NonSerialized]
	public bool isAssigned;

	[NonSerialized]
	public string teamMemberName;

	[NonSerialized]
	public ulong teamMemberId;

	[NonSerialized]
	public bool showOnCompass;

	[NonSerialized]
	public bool favourite;

	public static void ResetToPool(SleepingBag instance)
	{
		if (instance.ShouldPool)
		{
			instance.name = string.Empty;
			instance.deployerID = 0uL;
			instance.clientAssigned = false;
			instance.isAssigned = false;
			instance.teamMemberName = string.Empty;
			instance.teamMemberId = 0uL;
			instance.showOnCompass = false;
			instance.favourite = false;
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
			throw new Exception("Trying to dispose SleepingBag with ShouldPool set to false!");
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

	public void CopyTo(SleepingBag instance)
	{
		instance.name = name;
		instance.deployerID = deployerID;
		instance.clientAssigned = clientAssigned;
		instance.isAssigned = isAssigned;
		instance.teamMemberName = teamMemberName;
		instance.teamMemberId = teamMemberId;
		instance.showOnCompass = showOnCompass;
		instance.favourite = favourite;
	}

	public SleepingBag Copy()
	{
		SleepingBag sleepingBag = Pool.Get<SleepingBag>();
		CopyTo(sleepingBag);
		return sleepingBag;
	}

	public static SleepingBag Deserialize(BufferStream stream)
	{
		SleepingBag sleepingBag = Pool.Get<SleepingBag>();
		Deserialize(stream, sleepingBag, isDelta: false);
		return sleepingBag;
	}

	public static SleepingBag DeserializeLengthDelimited(BufferStream stream)
	{
		SleepingBag sleepingBag = Pool.Get<SleepingBag>();
		DeserializeLengthDelimited(stream, sleepingBag, isDelta: false);
		return sleepingBag;
	}

	public static SleepingBag DeserializeLength(BufferStream stream, int length)
	{
		SleepingBag sleepingBag = Pool.Get<SleepingBag>();
		DeserializeLength(stream, length, sleepingBag, isDelta: false);
		return sleepingBag;
	}

	public static SleepingBag Deserialize(byte[] buffer)
	{
		SleepingBag sleepingBag = Pool.Get<SleepingBag>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, sleepingBag, isDelta: false);
		return sleepingBag;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, SleepingBag previous)
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

	public static SleepingBag Deserialize(BufferStream stream, SleepingBag instance, bool isDelta)
	{
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.SleepingBag");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.deployerID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.clientAssigned = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.isAssigned = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.teamMemberName = ProtocolParser.ReadString(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.teamMemberId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.showOnCompass = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.favourite = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SleepingBag DeserializeLengthDelimited(BufferStream stream, SleepingBag instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SleepingBag");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.deployerID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.clientAssigned = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.isAssigned = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.teamMemberName = ProtocolParser.ReadString(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.teamMemberId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.showOnCompass = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.favourite = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static SleepingBag DeserializeLength(BufferStream stream, int length, SleepingBag instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.SleepingBag");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.deployerID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.clientAssigned = ProtocolParser.ReadBool(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.isAssigned = ProtocolParser.ReadBool(stream);
				continue;
			case 50:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.teamMemberName = ProtocolParser.ReadString(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.teamMemberId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.showOnCompass = ProtocolParser.ReadBool(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				instance.favourite = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.SleepingBag");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, SleepingBag instance, SleepingBag previous)
	{
		if (instance.name != null && instance.name != previous.name)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.deployerID != previous.deployerID)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.deployerID);
		}
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.clientAssigned);
		stream.WriteByte(40);
		ProtocolParser.WriteBool(stream, instance.isAssigned);
		if (instance.teamMemberName != null && instance.teamMemberName != previous.teamMemberName)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteString(stream, instance.teamMemberName);
		}
		if (instance.teamMemberId != previous.teamMemberId)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.teamMemberId);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteBool(stream, instance.showOnCompass);
		stream.WriteByte(72);
		ProtocolParser.WriteBool(stream, instance.favourite);
	}

	public static void Serialize(BufferStream stream, SleepingBag instance)
	{
		if (instance.name != null)
		{
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}
		if (instance.deployerID != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.deployerID);
		}
		if (instance.clientAssigned)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.clientAssigned);
		}
		if (instance.isAssigned)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.isAssigned);
		}
		if (instance.teamMemberName != null)
		{
			stream.WriteByte(50);
			ProtocolParser.WriteString(stream, instance.teamMemberName);
		}
		if (instance.teamMemberId != 0L)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.teamMemberId);
		}
		if (instance.showOnCompass)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.showOnCompass);
		}
		if (instance.favourite)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteBool(stream, instance.favourite);
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
