using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class NexusFerry : IDisposable, Pool.IPooled, IProto<NexusFerry>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

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

	[NonSerialized]
	public List<NetworkableId> transferredIds;

	[NonSerialized]
	public int nextScheduleIndex;

	public static void ResetToPool(NexusFerry instance)
	{
		if (instance.ShouldPool)
		{
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
			if (instance.transferredIds != null)
			{
				List<NetworkableId> obj2 = instance.transferredIds;
				Pool.FreeUnmanaged(ref obj2);
				instance.transferredIds = obj2;
			}
			instance.nextScheduleIndex = 0;
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
			throw new Exception("Trying to dispose NexusFerry with ShouldPool set to false!");
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

	public void CopyTo(NexusFerry instance)
	{
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
		if (transferredIds != null)
		{
			instance.transferredIds = Pool.Get<List<NetworkableId>>();
			for (int j = 0; j < transferredIds.Count; j++)
			{
				NetworkableId item2 = transferredIds[j];
				instance.transferredIds.Add(item2);
			}
		}
		else
		{
			instance.transferredIds = null;
		}
		instance.nextScheduleIndex = nextScheduleIndex;
	}

	public NexusFerry Copy()
	{
		NexusFerry nexusFerry = Pool.Get<NexusFerry>();
		CopyTo(nexusFerry);
		return nexusFerry;
	}

	public static NexusFerry Deserialize(BufferStream stream)
	{
		NexusFerry nexusFerry = Pool.Get<NexusFerry>();
		Deserialize(stream, nexusFerry, isDelta: false);
		return nexusFerry;
	}

	public static NexusFerry DeserializeLengthDelimited(BufferStream stream)
	{
		NexusFerry nexusFerry = Pool.Get<NexusFerry>();
		DeserializeLengthDelimited(stream, nexusFerry, isDelta: false);
		return nexusFerry;
	}

	public static NexusFerry DeserializeLength(BufferStream stream, int length)
	{
		NexusFerry nexusFerry = Pool.Get<NexusFerry>();
		DeserializeLength(stream, length, nexusFerry, isDelta: false);
		return nexusFerry;
	}

	public static NexusFerry Deserialize(byte[] buffer)
	{
		NexusFerry nexusFerry = Pool.Get<NexusFerry>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, nexusFerry, isDelta: false);
		return nexusFerry;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NexusFerry previous)
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

	public static NexusFerry Deserialize(BufferStream stream, NexusFerry instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.schedule == null)
			{
				instance.schedule = Pool.Get<List<string>>();
			}
			if (instance.transferredIds == null)
			{
				instance.transferredIds = Pool.Get<List<NetworkableId>>();
			}
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NexusFerry");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.ownerZone = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.scheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.isRetiring = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				stream.ConsumeRepeatedElement();
				instance.transferredIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.nextScheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NexusFerry DeserializeLengthDelimited(BufferStream stream, NexusFerry instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.schedule == null)
			{
				instance.schedule = Pool.Get<List<string>>();
			}
			if (instance.transferredIds == null)
			{
				instance.transferredIds = Pool.Get<List<NetworkableId>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.NexusFerry");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.ownerZone = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.scheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.isRetiring = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				stream.ConsumeRepeatedElement();
				instance.transferredIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.nextScheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static NexusFerry DeserializeLength(BufferStream stream, int length, NexusFerry instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.schedule == null)
			{
				instance.schedule = Pool.Get<List<string>>();
			}
			if (instance.transferredIds == null)
			{
				instance.transferredIds = Pool.Get<List<NetworkableId>>();
			}
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
			stream.ConsumeFieldOperation("ProtoBuf.NexusFerry");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.ownerZone = ProtocolParser.ReadString(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ProtocolParser.ReadString(stream));
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.scheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.isRetiring = ProtocolParser.ReadBool(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				stream.ConsumeRepeatedElement();
				instance.transferredIds.Add(new NetworkableId(ProtocolParser.ReadUInt64(stream)));
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				instance.nextScheduleIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusFerry");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NexusFerry instance, NexusFerry previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		if (instance.ownerZone != null && instance.ownerZone != previous.ownerZone)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.ownerZone);
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
		if (instance.scheduleIndex != previous.scheduleIndex)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.scheduleIndex);
		}
		if (instance.state != previous.state)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		stream.WriteByte(48);
		ProtocolParser.WriteBool(stream, instance.isRetiring);
		if (instance.transferredIds != null)
		{
			for (int j = 0; j < instance.transferredIds.Count; j++)
			{
				NetworkableId networkableId = instance.transferredIds[j];
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.nextScheduleIndex != previous.nextScheduleIndex)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.nextScheduleIndex);
		}
	}

	public static void Serialize(BufferStream stream, NexusFerry instance)
	{
		if (instance.timestamp != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		}
		if (instance.ownerZone != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.ownerZone);
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
		if (instance.scheduleIndex != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.scheduleIndex);
		}
		if (instance.state != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.isRetiring)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.isRetiring);
		}
		if (instance.transferredIds != null)
		{
			for (int j = 0; j < instance.transferredIds.Count; j++)
			{
				NetworkableId networkableId = instance.transferredIds[j];
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, networkableId.Value);
			}
		}
		if (instance.nextScheduleIndex != 0)
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.nextScheduleIndex);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (transferredIds != null)
		{
			for (int i = 0; i < transferredIds.Count; i++)
			{
				NetworkableId value = transferredIds[i];
				action(UidType.NetworkableId, ref value.Value);
				transferredIds[i] = value;
			}
		}
	}
}
