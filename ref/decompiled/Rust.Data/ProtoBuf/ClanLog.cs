using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanLog : IDisposable, Pool.IPooled, IProto<ClanLog>, IProto
{
	public class Entry : IDisposable, Pool.IPooled, IProto<Entry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public long timestamp;

		[NonSerialized]
		public string eventKey;

		[NonSerialized]
		public string arg1;

		[NonSerialized]
		public string arg2;

		[NonSerialized]
		public string arg3;

		[NonSerialized]
		public string arg4;

		public static void ResetToPool(Entry instance)
		{
			if (instance.ShouldPool)
			{
				instance.timestamp = 0L;
				instance.eventKey = string.Empty;
				instance.arg1 = string.Empty;
				instance.arg2 = string.Empty;
				instance.arg3 = string.Empty;
				instance.arg4 = string.Empty;
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
				throw new Exception("Trying to dispose Entry with ShouldPool set to false!");
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

		public void CopyTo(Entry instance)
		{
			instance.timestamp = timestamp;
			instance.eventKey = eventKey;
			instance.arg1 = arg1;
			instance.arg2 = arg2;
			instance.arg3 = arg3;
			instance.arg4 = arg4;
		}

		public Entry Copy()
		{
			Entry entry = Pool.Get<Entry>();
			CopyTo(entry);
			return entry;
		}

		public static Entry Deserialize(BufferStream stream)
		{
			Entry entry = Pool.Get<Entry>();
			Deserialize(stream, entry, isDelta: false);
			return entry;
		}

		public static Entry DeserializeLengthDelimited(BufferStream stream)
		{
			Entry entry = Pool.Get<Entry>();
			DeserializeLengthDelimited(stream, entry, isDelta: false);
			return entry;
		}

		public static Entry DeserializeLength(BufferStream stream, int length)
		{
			Entry entry = Pool.Get<Entry>();
			DeserializeLength(stream, length, entry, isDelta: false);
			return entry;
		}

		public static Entry Deserialize(byte[] buffer)
		{
			Entry entry = Pool.Get<Entry>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, entry, isDelta: false);
			return entry;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Entry previous)
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

		public static Entry Deserialize(BufferStream stream, Entry instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.timestamp = 0L;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClanLog.Entry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.eventKey = ProtocolParser.ReadString(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg1 = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg2 = ProtocolParser.ReadString(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg3 = ProtocolParser.ReadString(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg4 = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Entry DeserializeLengthDelimited(BufferStream stream, Entry instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.timestamp = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanLog.Entry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.eventKey = ProtocolParser.ReadString(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg1 = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg2 = ProtocolParser.ReadString(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg3 = ProtocolParser.ReadString(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg4 = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Entry DeserializeLength(BufferStream stream, int length, Entry instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.timestamp = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanLog.Entry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.eventKey = ProtocolParser.ReadString(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg1 = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg2 = ProtocolParser.ReadString(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg3 = ProtocolParser.ReadString(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					instance.arg4 = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLog.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Entry instance, Entry previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
			if (instance.eventKey != previous.eventKey)
			{
				if (instance.eventKey == null)
				{
					throw new ArgumentNullException("eventKey", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.eventKey);
			}
			if (instance.arg1 != null && instance.arg1 != previous.arg1)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.arg1);
			}
			if (instance.arg2 != null && instance.arg2 != previous.arg2)
			{
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, instance.arg2);
			}
			if (instance.arg3 != null && instance.arg3 != previous.arg3)
			{
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, instance.arg3);
			}
			if (instance.arg4 != null && instance.arg4 != previous.arg4)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteString(stream, instance.arg4);
			}
		}

		public static void Serialize(BufferStream stream, Entry instance)
		{
			if (instance.timestamp != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
			}
			if (instance.eventKey == null)
			{
				throw new ArgumentNullException("eventKey", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.eventKey);
			if (instance.arg1 != null)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.arg1);
			}
			if (instance.arg2 != null)
			{
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, instance.arg2);
			}
			if (instance.arg3 != null)
			{
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, instance.arg3);
			}
			if (instance.arg4 != null)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteString(stream, instance.arg4);
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
	public long clanId;

	[NonSerialized]
	public List<Entry> logEntries;

	public static void ResetToPool(ClanLog instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.clanId = 0L;
		if (instance.logEntries != null)
		{
			for (int i = 0; i < instance.logEntries.Count; i++)
			{
				if (instance.logEntries[i] != null)
				{
					instance.logEntries[i].ResetToPool();
					instance.logEntries[i] = null;
				}
			}
			List<Entry> obj = instance.logEntries;
			Pool.Free(ref obj, freeElements: false);
			instance.logEntries = obj;
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
			throw new Exception("Trying to dispose ClanLog with ShouldPool set to false!");
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

	public void CopyTo(ClanLog instance)
	{
		instance.clanId = clanId;
		if (logEntries != null)
		{
			instance.logEntries = Pool.Get<List<Entry>>();
			for (int i = 0; i < logEntries.Count; i++)
			{
				Entry item = logEntries[i].Copy();
				instance.logEntries.Add(item);
			}
		}
		else
		{
			instance.logEntries = null;
		}
	}

	public ClanLog Copy()
	{
		ClanLog clanLog = Pool.Get<ClanLog>();
		CopyTo(clanLog);
		return clanLog;
	}

	public static ClanLog Deserialize(BufferStream stream)
	{
		ClanLog clanLog = Pool.Get<ClanLog>();
		Deserialize(stream, clanLog, isDelta: false);
		return clanLog;
	}

	public static ClanLog DeserializeLengthDelimited(BufferStream stream)
	{
		ClanLog clanLog = Pool.Get<ClanLog>();
		DeserializeLengthDelimited(stream, clanLog, isDelta: false);
		return clanLog;
	}

	public static ClanLog DeserializeLength(BufferStream stream, int length)
	{
		ClanLog clanLog = Pool.Get<ClanLog>();
		DeserializeLength(stream, length, clanLog, isDelta: false);
		return clanLog;
	}

	public static ClanLog Deserialize(byte[] buffer)
	{
		ClanLog clanLog = Pool.Get<ClanLog>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanLog, isDelta: false);
		return clanLog;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanLog previous)
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

	public static ClanLog Deserialize(BufferStream stream, ClanLog instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			if (instance.logEntries == null)
			{
				instance.logEntries = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanLog");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				stream.ConsumeRepeatedElement();
				instance.logEntries.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanLog DeserializeLengthDelimited(BufferStream stream, ClanLog instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			if (instance.logEntries == null)
			{
				instance.logEntries = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanLog");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				stream.ConsumeRepeatedElement();
				instance.logEntries.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanLog DeserializeLength(BufferStream stream, int length, ClanLog instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			if (instance.logEntries == null)
			{
				instance.logEntries = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanLog");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				stream.ConsumeRepeatedElement();
				instance.logEntries.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLog");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanLog instance, ClanLog previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		if (instance.logEntries == null)
		{
			return;
		}
		for (int i = 0; i < instance.logEntries.Count; i++)
		{
			Entry entry = instance.logEntries[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Entry.SerializeDelta(stream, entry, entry);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, ClanLog instance)
	{
		if (instance.clanId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		}
		if (instance.logEntries == null)
		{
			return;
		}
		for (int i = 0; i < instance.logEntries.Count; i++)
		{
			Entry instance2 = instance.logEntries[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			Entry.Serialize(stream, instance2);
			int val = stream.Position - position;
			Span<byte> span = range.GetSpan();
			int num = ProtocolParser.WriteUInt32((uint)val, span, 0);
			if (num < 5)
			{
				span[num - 1] |= 128;
				while (num < 4)
				{
					span[num++] = 128;
				}
				span[4] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (logEntries != null)
		{
			for (int i = 0; i < logEntries.Count; i++)
			{
				logEntries[i]?.InspectUids(action);
			}
		}
	}
}
