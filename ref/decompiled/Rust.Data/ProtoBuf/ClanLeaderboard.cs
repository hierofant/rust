using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanLeaderboard : IDisposable, Pool.IPooled, IProto<ClanLeaderboard>, IProto
{
	public class Entry : IDisposable, Pool.IPooled, IProto<Entry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public long clanId;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public long score;

		public static void ResetToPool(Entry instance)
		{
			if (instance.ShouldPool)
			{
				instance.clanId = 0L;
				instance.name = string.Empty;
				instance.score = 0L;
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
			instance.clanId = clanId;
			instance.name = name;
			instance.score = score;
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
				instance.clanId = 0L;
				instance.score = 0L;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClanLeaderboard.Entry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.score = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard.Entry");
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
				instance.clanId = 0L;
				instance.score = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanLeaderboard.Entry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.score = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard.Entry");
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
				instance.clanId = 0L;
				instance.score = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanLeaderboard.Entry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					instance.score = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Entry instance, Entry previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.name);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
		}

		public static void Serialize(BufferStream stream, Entry instance)
		{
			if (instance.clanId != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
			}
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.score != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
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
	public List<Entry> entries;

	public static void ResetToPool(ClanLeaderboard instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.entries != null)
		{
			for (int i = 0; i < instance.entries.Count; i++)
			{
				if (instance.entries[i] != null)
				{
					instance.entries[i].ResetToPool();
					instance.entries[i] = null;
				}
			}
			List<Entry> obj = instance.entries;
			Pool.Free(ref obj, freeElements: false);
			instance.entries = obj;
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
			throw new Exception("Trying to dispose ClanLeaderboard with ShouldPool set to false!");
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

	public void CopyTo(ClanLeaderboard instance)
	{
		if (entries != null)
		{
			instance.entries = Pool.Get<List<Entry>>();
			for (int i = 0; i < entries.Count; i++)
			{
				Entry item = entries[i].Copy();
				instance.entries.Add(item);
			}
		}
		else
		{
			instance.entries = null;
		}
	}

	public ClanLeaderboard Copy()
	{
		ClanLeaderboard clanLeaderboard = Pool.Get<ClanLeaderboard>();
		CopyTo(clanLeaderboard);
		return clanLeaderboard;
	}

	public static ClanLeaderboard Deserialize(BufferStream stream)
	{
		ClanLeaderboard clanLeaderboard = Pool.Get<ClanLeaderboard>();
		Deserialize(stream, clanLeaderboard, isDelta: false);
		return clanLeaderboard;
	}

	public static ClanLeaderboard DeserializeLengthDelimited(BufferStream stream)
	{
		ClanLeaderboard clanLeaderboard = Pool.Get<ClanLeaderboard>();
		DeserializeLengthDelimited(stream, clanLeaderboard, isDelta: false);
		return clanLeaderboard;
	}

	public static ClanLeaderboard DeserializeLength(BufferStream stream, int length)
	{
		ClanLeaderboard clanLeaderboard = Pool.Get<ClanLeaderboard>();
		DeserializeLength(stream, length, clanLeaderboard, isDelta: false);
		return clanLeaderboard;
	}

	public static ClanLeaderboard Deserialize(byte[] buffer)
	{
		ClanLeaderboard clanLeaderboard = Pool.Get<ClanLeaderboard>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanLeaderboard, isDelta: false);
		return clanLeaderboard;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanLeaderboard previous)
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

	public static ClanLeaderboard Deserialize(BufferStream stream, ClanLeaderboard instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<Entry>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ClanLeaderboard");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanLeaderboard DeserializeLengthDelimited(BufferStream stream, ClanLeaderboard instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanLeaderboard");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static ClanLeaderboard DeserializeLength(BufferStream stream, int length, ClanLeaderboard instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanLeaderboard");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanLeaderboard instance, ClanLeaderboard previous)
	{
		if (instance.entries == null)
		{
			return;
		}
		for (int i = 0; i < instance.entries.Count; i++)
		{
			Entry entry = instance.entries[i];
			stream.WriteByte(10);
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

	public static void Serialize(BufferStream stream, ClanLeaderboard instance)
	{
		if (instance.entries == null)
		{
			return;
		}
		for (int i = 0; i < instance.entries.Count; i++)
		{
			Entry instance2 = instance.entries[i];
			stream.WriteByte(10);
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
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				entries[i]?.InspectUids(action);
			}
		}
	}
}
