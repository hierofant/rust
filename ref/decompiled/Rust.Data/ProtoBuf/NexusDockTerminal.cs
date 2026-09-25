using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class NexusDockTerminal : IDisposable, Pool.IPooled, IProto<NexusDockTerminal>, IProto
{
	public class ScheduleEntry : IDisposable, Pool.IPooled, IProto<ScheduleEntry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int nextZoneId;

		[NonSerialized]
		public int estimate;

		public static void ResetToPool(ScheduleEntry instance)
		{
			if (instance.ShouldPool)
			{
				instance.nextZoneId = 0;
				instance.estimate = 0;
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
				throw new Exception("Trying to dispose ScheduleEntry with ShouldPool set to false!");
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

		public void CopyTo(ScheduleEntry instance)
		{
			instance.nextZoneId = nextZoneId;
			instance.estimate = estimate;
		}

		public ScheduleEntry Copy()
		{
			ScheduleEntry scheduleEntry = Pool.Get<ScheduleEntry>();
			CopyTo(scheduleEntry);
			return scheduleEntry;
		}

		public static ScheduleEntry Deserialize(BufferStream stream)
		{
			ScheduleEntry scheduleEntry = Pool.Get<ScheduleEntry>();
			Deserialize(stream, scheduleEntry, isDelta: false);
			return scheduleEntry;
		}

		public static ScheduleEntry DeserializeLengthDelimited(BufferStream stream)
		{
			ScheduleEntry scheduleEntry = Pool.Get<ScheduleEntry>();
			DeserializeLengthDelimited(stream, scheduleEntry, isDelta: false);
			return scheduleEntry;
		}

		public static ScheduleEntry DeserializeLength(BufferStream stream, int length)
		{
			ScheduleEntry scheduleEntry = Pool.Get<ScheduleEntry>();
			DeserializeLength(stream, length, scheduleEntry, isDelta: false);
			return scheduleEntry;
		}

		public static ScheduleEntry Deserialize(byte[] buffer)
		{
			ScheduleEntry scheduleEntry = Pool.Get<ScheduleEntry>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, scheduleEntry, isDelta: false);
			return scheduleEntry;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ScheduleEntry previous)
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

		public static ScheduleEntry Deserialize(BufferStream stream, ScheduleEntry instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.NexusDockTerminal.ScheduleEntry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					instance.nextZoneId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					instance.estimate = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ScheduleEntry DeserializeLengthDelimited(BufferStream stream, ScheduleEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.NexusDockTerminal.ScheduleEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					instance.nextZoneId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					instance.estimate = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static ScheduleEntry DeserializeLength(BufferStream stream, int length, ScheduleEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.NexusDockTerminal.ScheduleEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					instance.nextZoneId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					instance.estimate = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal.ScheduleEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ScheduleEntry instance, ScheduleEntry previous)
		{
			if (instance.nextZoneId != previous.nextZoneId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.nextZoneId);
			}
			if (instance.estimate != previous.estimate)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.estimate);
			}
		}

		public static void Serialize(BufferStream stream, ScheduleEntry instance)
		{
			if (instance.nextZoneId != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.nextZoneId);
			}
			if (instance.estimate != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.estimate);
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

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<ScheduleEntry> schedule;

	public static void ResetToPool(NexusDockTerminal instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.schedule != null)
		{
			for (int i = 0; i < instance.schedule.Count; i++)
			{
				if (instance.schedule[i] != null)
				{
					instance.schedule[i].ResetToPool();
					instance.schedule[i] = null;
				}
			}
			List<ScheduleEntry> obj = instance.schedule;
			Pool.Free(ref obj, freeElements: false);
			instance.schedule = obj;
		}
		Pool.Free(ref instance);
	}

	public void ResetToPool()
	{
		ResetToPool(this);
	}

	public virtual void Dispose()
	{
		if (!ShouldPool)
		{
			throw new Exception("Trying to dispose NexusDockTerminal with ShouldPool set to false!");
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

	public void CopyTo(NexusDockTerminal instance)
	{
		if (schedule != null)
		{
			instance.schedule = Pool.Get<List<ScheduleEntry>>();
			for (int i = 0; i < schedule.Count; i++)
			{
				ScheduleEntry item = schedule[i].Copy();
				instance.schedule.Add(item);
			}
		}
		else
		{
			instance.schedule = null;
		}
	}

	public NexusDockTerminal Copy()
	{
		NexusDockTerminal nexusDockTerminal = Pool.Get<NexusDockTerminal>();
		CopyTo(nexusDockTerminal);
		return nexusDockTerminal;
	}

	public static NexusDockTerminal Deserialize(BufferStream stream)
	{
		NexusDockTerminal nexusDockTerminal = Pool.Get<NexusDockTerminal>();
		Deserialize(stream, nexusDockTerminal, isDelta: false);
		return nexusDockTerminal;
	}

	public static NexusDockTerminal DeserializeLengthDelimited(BufferStream stream)
	{
		NexusDockTerminal nexusDockTerminal = Pool.Get<NexusDockTerminal>();
		DeserializeLengthDelimited(stream, nexusDockTerminal, isDelta: false);
		return nexusDockTerminal;
	}

	public static NexusDockTerminal DeserializeLength(BufferStream stream, int length)
	{
		NexusDockTerminal nexusDockTerminal = Pool.Get<NexusDockTerminal>();
		DeserializeLength(stream, length, nexusDockTerminal, isDelta: false);
		return nexusDockTerminal;
	}

	public static NexusDockTerminal Deserialize(byte[] buffer)
	{
		NexusDockTerminal nexusDockTerminal = Pool.Get<NexusDockTerminal>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, nexusDockTerminal, isDelta: false);
		return nexusDockTerminal;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, NexusDockTerminal previous)
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

	public static NexusDockTerminal Deserialize(BufferStream stream, NexusDockTerminal instance, bool isDelta)
	{
		if (!isDelta && instance.schedule == null)
		{
			instance.schedule = Pool.Get<List<ScheduleEntry>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.NexusDockTerminal");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ScheduleEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static NexusDockTerminal DeserializeLengthDelimited(BufferStream stream, NexusDockTerminal instance, bool isDelta)
	{
		if (!isDelta && instance.schedule == null)
		{
			instance.schedule = Pool.Get<List<ScheduleEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.NexusDockTerminal");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ScheduleEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static NexusDockTerminal DeserializeLength(BufferStream stream, int length, NexusDockTerminal instance, bool isDelta)
	{
		if (!isDelta && instance.schedule == null)
		{
			instance.schedule = Pool.Get<List<ScheduleEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.NexusDockTerminal");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				stream.ConsumeRepeatedElement();
				instance.schedule.Add(ScheduleEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.NexusDockTerminal");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, NexusDockTerminal instance, NexusDockTerminal previous)
	{
		if (instance.schedule == null)
		{
			return;
		}
		for (int i = 0; i < instance.schedule.Count; i++)
		{
			ScheduleEntry scheduleEntry = instance.schedule[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ScheduleEntry.SerializeDelta(stream, scheduleEntry, scheduleEntry);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field schedule (ProtoBuf.NexusDockTerminal.ScheduleEntry)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, NexusDockTerminal instance)
	{
		if (instance.schedule == null)
		{
			return;
		}
		for (int i = 0; i < instance.schedule.Count; i++)
		{
			ScheduleEntry instance2 = instance.schedule[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			ScheduleEntry.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field schedule (ProtoBuf.NexusDockTerminal.ScheduleEntry)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (schedule != null)
		{
			for (int i = 0; i < schedule.Count; i++)
			{
				schedule[i]?.InspectUids(action);
			}
		}
	}
}
