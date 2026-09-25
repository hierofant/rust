using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanScoreEvents : IDisposable, Pool.IPooled, IProto<ClanScoreEvents>, IProto
{
	public class Entry : IDisposable, Pool.IPooled, IProto<Entry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public long timestamp;

		[NonSerialized]
		public int type;

		[NonSerialized]
		public int score;

		[NonSerialized]
		public int multiplier;

		[NonSerialized]
		public ulong steamId;

		[NonSerialized]
		public ulong otherSteamId;

		[NonSerialized]
		public long otherClanId;

		[NonSerialized]
		public string arg1;

		[NonSerialized]
		public string arg2;

		public static void ResetToPool(Entry instance)
		{
			if (instance.ShouldPool)
			{
				instance.timestamp = 0L;
				instance.type = 0;
				instance.score = 0;
				instance.multiplier = 0;
				instance.steamId = 0uL;
				instance.otherSteamId = 0uL;
				instance.otherClanId = 0L;
				instance.arg1 = string.Empty;
				instance.arg2 = string.Empty;
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
			instance.type = type;
			instance.score = score;
			instance.multiplier = multiplier;
			instance.steamId = steamId;
			instance.otherSteamId = otherSteamId;
			instance.otherClanId = otherClanId;
			instance.arg1 = arg1;
			instance.arg2 = arg2;
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
				instance.type = 0;
				instance.score = 0;
				instance.multiplier = 0;
				instance.steamId = 0uL;
				instance.otherSteamId = 0uL;
				instance.otherClanId = 0L;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClanScoreEvents.Entry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.multiplier = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.otherSteamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.otherClanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.arg1 = ProtocolParser.ReadString(stream);
					continue;
				case 74:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.arg2 = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents.Entry");
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
				instance.type = 0;
				instance.score = 0;
				instance.multiplier = 0;
				instance.steamId = 0uL;
				instance.otherSteamId = 0uL;
				instance.otherClanId = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanScoreEvents.Entry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.multiplier = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.otherSteamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.otherClanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.arg1 = ProtocolParser.ReadString(stream);
					continue;
				case 74:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.arg2 = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents.Entry");
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
				instance.type = 0;
				instance.score = 0;
				instance.multiplier = 0;
				instance.steamId = 0uL;
				instance.otherSteamId = 0uL;
				instance.otherClanId = 0L;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanScoreEvents.Entry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.multiplier = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.otherSteamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.otherClanId = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.arg1 = ProtocolParser.ReadString(stream);
					continue;
				case 74:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					instance.arg2 = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents.Entry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents.Entry");
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
			if (instance.type != previous.type)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.score != previous.score)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
			}
			if (instance.multiplier != previous.multiplier)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.multiplier);
			}
			if (instance.steamId != previous.steamId)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.otherSteamId != previous.otherSteamId)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, instance.otherSteamId);
			}
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.otherClanId);
			if (instance.arg1 != null && instance.arg1 != previous.arg1)
			{
				stream.WriteByte(66);
				ProtocolParser.WriteString(stream, instance.arg1);
			}
			if (instance.arg2 != null && instance.arg2 != previous.arg2)
			{
				stream.WriteByte(74);
				ProtocolParser.WriteString(stream, instance.arg2);
			}
		}

		public static void Serialize(BufferStream stream, Entry instance)
		{
			if (instance.timestamp != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
			}
			if (instance.type != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.score != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
			}
			if (instance.multiplier != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.multiplier);
			}
			if (instance.steamId != 0L)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.otherSteamId != 0L)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, instance.otherSteamId);
			}
			if (instance.otherClanId != 0L)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.otherClanId);
			}
			if (instance.arg1 != null)
			{
				stream.WriteByte(66);
				ProtocolParser.WriteString(stream, instance.arg1);
			}
			if (instance.arg2 != null)
			{
				stream.WriteByte(74);
				ProtocolParser.WriteString(stream, instance.arg2);
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
	public List<Entry> scoreEvents;

	public static void ResetToPool(ClanScoreEvents instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.clanId = 0L;
		if (instance.scoreEvents != null)
		{
			for (int i = 0; i < instance.scoreEvents.Count; i++)
			{
				if (instance.scoreEvents[i] != null)
				{
					instance.scoreEvents[i].ResetToPool();
					instance.scoreEvents[i] = null;
				}
			}
			List<Entry> obj = instance.scoreEvents;
			Pool.Free(ref obj, freeElements: false);
			instance.scoreEvents = obj;
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
			throw new Exception("Trying to dispose ClanScoreEvents with ShouldPool set to false!");
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

	public void CopyTo(ClanScoreEvents instance)
	{
		instance.clanId = clanId;
		if (scoreEvents != null)
		{
			instance.scoreEvents = Pool.Get<List<Entry>>();
			for (int i = 0; i < scoreEvents.Count; i++)
			{
				Entry item = scoreEvents[i].Copy();
				instance.scoreEvents.Add(item);
			}
		}
		else
		{
			instance.scoreEvents = null;
		}
	}

	public ClanScoreEvents Copy()
	{
		ClanScoreEvents clanScoreEvents = Pool.Get<ClanScoreEvents>();
		CopyTo(clanScoreEvents);
		return clanScoreEvents;
	}

	public static ClanScoreEvents Deserialize(BufferStream stream)
	{
		ClanScoreEvents clanScoreEvents = Pool.Get<ClanScoreEvents>();
		Deserialize(stream, clanScoreEvents, isDelta: false);
		return clanScoreEvents;
	}

	public static ClanScoreEvents DeserializeLengthDelimited(BufferStream stream)
	{
		ClanScoreEvents clanScoreEvents = Pool.Get<ClanScoreEvents>();
		DeserializeLengthDelimited(stream, clanScoreEvents, isDelta: false);
		return clanScoreEvents;
	}

	public static ClanScoreEvents DeserializeLength(BufferStream stream, int length)
	{
		ClanScoreEvents clanScoreEvents = Pool.Get<ClanScoreEvents>();
		DeserializeLength(stream, length, clanScoreEvents, isDelta: false);
		return clanScoreEvents;
	}

	public static ClanScoreEvents Deserialize(byte[] buffer)
	{
		ClanScoreEvents clanScoreEvents = Pool.Get<ClanScoreEvents>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanScoreEvents, isDelta: false);
		return clanScoreEvents;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanScoreEvents previous)
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

	public static ClanScoreEvents Deserialize(BufferStream stream, ClanScoreEvents instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			if (instance.scoreEvents == null)
			{
				instance.scoreEvents = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanScoreEvents");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				stream.ConsumeRepeatedElement();
				instance.scoreEvents.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanScoreEvents DeserializeLengthDelimited(BufferStream stream, ClanScoreEvents instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			if (instance.scoreEvents == null)
			{
				instance.scoreEvents = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanScoreEvents");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				stream.ConsumeRepeatedElement();
				instance.scoreEvents.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanScoreEvents DeserializeLength(BufferStream stream, int length, ClanScoreEvents instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			if (instance.scoreEvents == null)
			{
				instance.scoreEvents = Pool.Get<List<Entry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanScoreEvents");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				stream.ConsumeRepeatedElement();
				instance.scoreEvents.Add(Entry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanScoreEvents");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanScoreEvents instance, ClanScoreEvents previous)
	{
		stream.WriteByte(8);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		if (instance.scoreEvents == null)
		{
			return;
		}
		for (int i = 0; i < instance.scoreEvents.Count; i++)
		{
			Entry entry = instance.scoreEvents[i];
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

	public static void Serialize(BufferStream stream, ClanScoreEvents instance)
	{
		if (instance.clanId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.clanId);
		}
		if (instance.scoreEvents == null)
		{
			return;
		}
		for (int i = 0; i < instance.scoreEvents.Count; i++)
		{
			Entry instance2 = instance.scoreEvents[i];
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
		if (scoreEvents != null)
		{
			for (int i = 0; i < scoreEvents.Count; i++)
			{
				scoreEvents[i]?.InspectUids(action);
			}
		}
	}
}
