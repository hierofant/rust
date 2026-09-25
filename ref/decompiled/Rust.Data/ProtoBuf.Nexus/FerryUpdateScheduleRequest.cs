using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf.Nexus;

public class FerryUpdateScheduleRequest : IDisposable, Pool.IPooled, IProto<FerryUpdateScheduleRequest>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public NetworkableId entityId;

	[NonSerialized]
	public long timestamp;

	[NonSerialized]
	public List<string> schedule;

	public static void ResetToPool(FerryUpdateScheduleRequest instance)
	{
		if (instance.ShouldPool)
		{
			instance.entityId = default(NetworkableId);
			instance.timestamp = 0L;
			if (instance.schedule != null)
			{
				List<string> obj = instance.schedule;
				Pool.FreeUnmanaged(ref obj);
				instance.schedule = obj;
			}
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
			throw new Exception("Trying to dispose FerryUpdateScheduleRequest with ShouldPool set to false!");
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

	public void CopyTo(FerryUpdateScheduleRequest instance)
	{
		instance.entityId = entityId;
		instance.timestamp = timestamp;
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
	}

	public FerryUpdateScheduleRequest Copy()
	{
		FerryUpdateScheduleRequest ferryUpdateScheduleRequest = Pool.Get<FerryUpdateScheduleRequest>();
		CopyTo(ferryUpdateScheduleRequest);
		return ferryUpdateScheduleRequest;
	}

	public static FerryUpdateScheduleRequest Deserialize(BufferStream stream)
	{
		FerryUpdateScheduleRequest ferryUpdateScheduleRequest = Pool.Get<FerryUpdateScheduleRequest>();
		Deserialize(stream, ferryUpdateScheduleRequest, isDelta: false);
		return ferryUpdateScheduleRequest;
	}

	public static FerryUpdateScheduleRequest DeserializeLengthDelimited(BufferStream stream)
	{
		FerryUpdateScheduleRequest ferryUpdateScheduleRequest = Pool.Get<FerryUpdateScheduleRequest>();
		DeserializeLengthDelimited(stream, ferryUpdateScheduleRequest, isDelta: false);
		return ferryUpdateScheduleRequest;
	}

	public static FerryUpdateScheduleRequest DeserializeLength(BufferStream stream, int length)
	{
		FerryUpdateScheduleRequest ferryUpdateScheduleRequest = Pool.Get<FerryUpdateScheduleRequest>();
		DeserializeLength(stream, length, ferryUpdateScheduleRequest, isDelta: false);
		return ferryUpdateScheduleRequest;
	}

	public static FerryUpdateScheduleRequest Deserialize(byte[] buffer)
	{
		FerryUpdateScheduleRequest ferryUpdateScheduleRequest = Pool.Get<FerryUpdateScheduleRequest>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, ferryUpdateScheduleRequest, isDelta: false);
		return ferryUpdateScheduleRequest;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, FerryUpdateScheduleRequest previous)
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

	public static FerryUpdateScheduleRequest Deserialize(BufferStream stream, FerryUpdateScheduleRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryUpdateScheduleRequest");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FerryUpdateScheduleRequest DeserializeLengthDelimited(BufferStream stream, FerryUpdateScheduleRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryUpdateScheduleRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static FerryUpdateScheduleRequest DeserializeLength(BufferStream stream, int length, FerryUpdateScheduleRequest instance, bool isDelta)
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
			stream.ConsumeFieldOperation("ProtoBuf.Nexus.FerryUpdateScheduleRequest");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.Nexus.FerryUpdateScheduleRequest");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, FerryUpdateScheduleRequest instance, FerryUpdateScheduleRequest previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
		stream.WriteByte(16);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		if (instance.schedule != null)
		{
			for (int i = 0; i < instance.schedule.Count; i++)
			{
				string val = instance.schedule[i];
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, val);
			}
		}
	}

	public static void Serialize(BufferStream stream, FerryUpdateScheduleRequest instance)
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
		if (instance.schedule != null)
		{
			for (int i = 0; i < instance.schedule.Count; i++)
			{
				string val = instance.schedule[i];
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, val);
			}
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
