using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class FerryStatus : IDisposable, Pool.IPooled, IProto<FerryStatus>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId entityId;

	[NonSerialized]
	public long timestamp;

	[NonSerialized]
	public string ownerZone;

	[NonSerialized]
	public List<string> schedule;

	[NonSerialized]
	public int scheduleIndex;

	[NonSerialized]
	public int state;

	[NonSerialized]
	public bool isRetiring;

	public static void ResetToPool(FerryStatus instance)
	{
		if (instance.ShouldPool)
		{
			instance.entityId = default(NetworkableId);
			instance.timestamp = 0L;
			instance.ownerZone = string.Empty;
			if (instance.schedule != null)
			{
				List<string> obj = instance.schedule;
				Pool.FreeUnmanaged(ref obj);
				instance.schedule = obj;
			}
			instance.scheduleIndex = 0;
			instance.state = 0;
			instance.isRetiring = false;
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
			throw new Exception("Trying to dispose FerryStatus with ShouldPool set to false!");
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

	public void CopyTo(FerryStatus instance)
	{
		instance.entityId = entityId;
		instance.timestamp = timestamp;
		instance.ownerZone = ownerZone;
		if (schedule != null)
		{
			instance.schedule = Pool.Get<List<string>>();
			for (int i = 0; i < schedule.Count; i++)
			{
				string item = schedule[i];
				instance.schedule.Add(item);
			}
		}
		else
		{
			instance.schedule = null;
		}
		instance.scheduleIndex = scheduleIndex;
		instance.state = state;
		instance.isRetiring = isRetiring;
	}

	public FerryStatus Copy()
	{
		FerryStatus ferryStatus = Pool.Get<FerryStatus>();
		CopyTo(ferryStatus);
		return ferryStatus;
	}

	public static FerryStatus Deserialize(BufferStream stream)
	{
		FerryStatus ferryStatus = Pool.Get<FerryStatus>();
		Deserialize(stream, ferryStatus, isDelta: false);
		return ferryStatus;
	}

	public static FerryStatus DeserializeLengthDelimited(BufferStream stream)
	{
		FerryStatus ferryStatus = Pool.Get<FerryStatus>();
		DeserializeLengthDelimited(stream, ferryStatus, isDelta: false);
		return ferryStatus;
	}

	public static FerryStatus DeserializeLength(BufferStream stream, int length)
	{
		FerryStatus ferryStatus = Pool.Get<FerryStatus>();
		DeserializeLength(stream, length, ferryStatus, isDelta: false);
		return ferryStatus;
	}

	public static FerryStatus Deserialize(byte[] buffer)
	{
		FerryStatus ferryStatus = Pool.Get<FerryStatus>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ferryStatus, isDelta: false);
		return ferryStatus;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, FerryStatus previous)
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

	public static FerryStatus Deserialize(BufferStream stream, FerryStatus instance, bool isDelta)
	{
		if (!isDelta && instance.schedule == null)
		{
			instance.schedule = Pool.Get<List<string>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatus");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.ownerZone = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.scheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.isRetiring = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FerryStatus DeserializeLengthDelimited(BufferStream stream, FerryStatus instance, bool isDelta)
	{
		if (!isDelta && instance.schedule == null)
		{
			instance.schedule = Pool.Get<List<string>>();
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatus");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.ownerZone = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.scheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.isRetiring = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FerryStatus DeserializeLength(BufferStream stream, int length, FerryStatus instance, bool isDelta)
	{
		if (!isDelta && instance.schedule == null)
		{
			instance.schedule = Pool.Get<List<string>>();
		}
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryStatus");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.ownerZone = ProtocolParser.ReadString(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.scheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				instance.isRetiring = ProtocolParser.ReadBool(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryStatus");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, FerryStatus instance, FerryStatus previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		if (instance.ownerZone != previous.ownerZone)
		{
			if (instance.ownerZone == null)
			{
				throw new ArgumentNullException("ownerZone", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.ownerZone);
		}
		if (instance.schedule != null)
		{
			for (int i = 0; i < instance.schedule.Count; i++)
			{
				string val = instance.schedule[i];
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, val);
			}
		}
		if (instance.scheduleIndex != previous.scheduleIndex)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.scheduleIndex);
		}
		if (instance.state != previous.state)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		stream.WriteByte(56);
		ProtocolParser.WriteBool(stream, instance.isRetiring);
	}

	public static void Serialize(BufferStream stream, FerryStatus instance)
	{
		if (instance.entityId != default(NetworkableId))
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		}
		if (instance.timestamp != 0L)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		}
		if (instance.ownerZone == null)
		{
			throw new ArgumentNullException("ownerZone", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WriteString(stream, instance.ownerZone);
		if (instance.schedule != null)
		{
			for (int i = 0; i < instance.schedule.Count; i++)
			{
				string val = instance.schedule[i];
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, val);
			}
		}
		if (instance.scheduleIndex != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.scheduleIndex);
		}
		if (instance.state != 0)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.isRetiring)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.isRetiring);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		action(UidType.NetworkableId, ref entityId.Value);
	}
}
