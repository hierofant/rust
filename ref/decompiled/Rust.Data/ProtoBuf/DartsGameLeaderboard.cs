using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DartsGameLeaderboard : IDisposable, Pool.IPooled, IProto<DartsGameLeaderboard>, IProto
{
	public class DartsGameLeaderboardEntry : IDisposable, Pool.IPooled, IProto<DartsGameLeaderboardEntry>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong userid;

		[NonSerialized]
		public string playerName;

		[NonSerialized]
		public int dartsThrown;

		[NonSerialized]
		public float timeTaken;

		public static void ResetToPool(DartsGameLeaderboardEntry instance)
		{
			if (instance.ShouldPool)
			{
				instance.userid = 0uL;
				instance.playerName = string.Empty;
				instance.dartsThrown = 0;
				instance.timeTaken = 0f;
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
				throw new Exception("Trying to dispose DartsGameLeaderboardEntry with ShouldPool set to false!");
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

		public void CopyTo(DartsGameLeaderboardEntry instance)
		{
			instance.userid = userid;
			instance.playerName = playerName;
			instance.dartsThrown = dartsThrown;
			instance.timeTaken = timeTaken;
		}

		public DartsGameLeaderboardEntry Copy()
		{
			DartsGameLeaderboardEntry dartsGameLeaderboardEntry = Pool.Get<DartsGameLeaderboardEntry>();
			CopyTo(dartsGameLeaderboardEntry);
			return dartsGameLeaderboardEntry;
		}

		public static DartsGameLeaderboardEntry Deserialize(BufferStream stream)
		{
			DartsGameLeaderboardEntry dartsGameLeaderboardEntry = Pool.Get<DartsGameLeaderboardEntry>();
			Deserialize(stream, dartsGameLeaderboardEntry, isDelta: false);
			return dartsGameLeaderboardEntry;
		}

		public static DartsGameLeaderboardEntry DeserializeLengthDelimited(BufferStream stream)
		{
			DartsGameLeaderboardEntry dartsGameLeaderboardEntry = Pool.Get<DartsGameLeaderboardEntry>();
			DeserializeLengthDelimited(stream, dartsGameLeaderboardEntry, isDelta: false);
			return dartsGameLeaderboardEntry;
		}

		public static DartsGameLeaderboardEntry DeserializeLength(BufferStream stream, int length)
		{
			DartsGameLeaderboardEntry dartsGameLeaderboardEntry = Pool.Get<DartsGameLeaderboardEntry>();
			DeserializeLength(stream, length, dartsGameLeaderboardEntry, isDelta: false);
			return dartsGameLeaderboardEntry;
		}

		public static DartsGameLeaderboardEntry Deserialize(byte[] buffer)
		{
			DartsGameLeaderboardEntry dartsGameLeaderboardEntry = Pool.Get<DartsGameLeaderboardEntry>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, dartsGameLeaderboardEntry, isDelta: false);
			return dartsGameLeaderboardEntry;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, DartsGameLeaderboardEntry previous)
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

		public static DartsGameLeaderboardEntry Deserialize(BufferStream stream, DartsGameLeaderboardEntry instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.playerName = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.dartsThrown = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.timeTaken = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DartsGameLeaderboardEntry DeserializeLengthDelimited(BufferStream stream, DartsGameLeaderboardEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.playerName = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.dartsThrown = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.timeTaken = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DartsGameLeaderboardEntry DeserializeLength(BufferStream stream, int length, DartsGameLeaderboardEntry instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.playerName = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.dartsThrown = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					instance.timeTaken = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard.DartsGameLeaderboardEntry");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, DartsGameLeaderboardEntry instance, DartsGameLeaderboardEntry previous)
		{
			if (instance.userid != previous.userid)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.playerName != previous.playerName)
			{
				if (instance.playerName == null)
				{
					throw new ArgumentNullException("playerName", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.playerName);
			}
			if (instance.dartsThrown != previous.dartsThrown)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dartsThrown);
			}
			if (instance.timeTaken != previous.timeTaken)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.timeTaken);
			}
		}

		public static void Serialize(BufferStream stream, DartsGameLeaderboardEntry instance)
		{
			if (instance.userid != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.playerName == null)
			{
				throw new ArgumentNullException("playerName", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.playerName);
			if (instance.dartsThrown != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dartsThrown);
			}
			if (instance.timeTaken != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.timeTaken);
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
	public List<DartsGameLeaderboardEntry> entries;

	public static void ResetToPool(DartsGameLeaderboard instance)
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
			List<DartsGameLeaderboardEntry> obj = instance.entries;
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
			throw new Exception("Trying to dispose DartsGameLeaderboard with ShouldPool set to false!");
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

	public void CopyTo(DartsGameLeaderboard instance)
	{
		if (entries != null)
		{
			instance.entries = Pool.Get<List<DartsGameLeaderboardEntry>>();
			for (int i = 0; i < entries.Count; i++)
			{
				DartsGameLeaderboardEntry item = entries[i].Copy();
				instance.entries.Add(item);
			}
		}
		else
		{
			instance.entries = null;
		}
	}

	public DartsGameLeaderboard Copy()
	{
		DartsGameLeaderboard dartsGameLeaderboard = Pool.Get<DartsGameLeaderboard>();
		CopyTo(dartsGameLeaderboard);
		return dartsGameLeaderboard;
	}

	public static DartsGameLeaderboard Deserialize(BufferStream stream)
	{
		DartsGameLeaderboard dartsGameLeaderboard = Pool.Get<DartsGameLeaderboard>();
		Deserialize(stream, dartsGameLeaderboard, isDelta: false);
		return dartsGameLeaderboard;
	}

	public static DartsGameLeaderboard DeserializeLengthDelimited(BufferStream stream)
	{
		DartsGameLeaderboard dartsGameLeaderboard = Pool.Get<DartsGameLeaderboard>();
		DeserializeLengthDelimited(stream, dartsGameLeaderboard, isDelta: false);
		return dartsGameLeaderboard;
	}

	public static DartsGameLeaderboard DeserializeLength(BufferStream stream, int length)
	{
		DartsGameLeaderboard dartsGameLeaderboard = Pool.Get<DartsGameLeaderboard>();
		DeserializeLength(stream, length, dartsGameLeaderboard, isDelta: false);
		return dartsGameLeaderboard;
	}

	public static DartsGameLeaderboard Deserialize(byte[] buffer)
	{
		DartsGameLeaderboard dartsGameLeaderboard = Pool.Get<DartsGameLeaderboard>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, dartsGameLeaderboard, isDelta: false);
		return dartsGameLeaderboard;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DartsGameLeaderboard previous)
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

	public static DartsGameLeaderboard Deserialize(BufferStream stream, DartsGameLeaderboard instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<DartsGameLeaderboardEntry>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DartsGameLeaderboard");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(DartsGameLeaderboardEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static DartsGameLeaderboard DeserializeLengthDelimited(BufferStream stream, DartsGameLeaderboard instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<DartsGameLeaderboardEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DartsGameLeaderboard");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(DartsGameLeaderboardEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static DartsGameLeaderboard DeserializeLength(BufferStream stream, int length, DartsGameLeaderboard instance, bool isDelta)
	{
		if (!isDelta && instance.entries == null)
		{
			instance.entries = Pool.Get<List<DartsGameLeaderboardEntry>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DartsGameLeaderboard");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				stream.ConsumeRepeatedElement();
				instance.entries.Add(DartsGameLeaderboardEntry.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGameLeaderboard");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DartsGameLeaderboard instance, DartsGameLeaderboard previous)
	{
		if (instance.entries == null)
		{
			return;
		}
		for (int i = 0; i < instance.entries.Count; i++)
		{
			DartsGameLeaderboardEntry dartsGameLeaderboardEntry = instance.entries[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			DartsGameLeaderboardEntry.SerializeDelta(stream, dartsGameLeaderboardEntry, dartsGameLeaderboardEntry);
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

	public static void Serialize(BufferStream stream, DartsGameLeaderboard instance)
	{
		if (instance.entries == null)
		{
			return;
		}
		for (int i = 0; i < instance.entries.Count; i++)
		{
			DartsGameLeaderboardEntry instance2 = instance.entries[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			DartsGameLeaderboardEntry.Serialize(stream, instance2);
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
