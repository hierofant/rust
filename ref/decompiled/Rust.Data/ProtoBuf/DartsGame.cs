using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class DartsGame : IDisposable, Pool.IPooled, IProto<DartsGame>, IProto
{
	public class DartsPlayerData : IDisposable, Pool.IPooled, IProto<DartsPlayerData>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong userid;

		[NonSerialized]
		public int dartsThrown;

		[NonSerialized]
		public int dartsThrownThisTurn;

		[NonSerialized]
		public float timeTaken;

		[NonSerialized]
		public int score;

		[NonSerialized]
		public int scoreThisTurn;

		[NonSerialized]
		public int state;

		[NonSerialized]
		public int turn;

		[NonSerialized]
		public List<int> scoreHistory;

		public static void ResetToPool(DartsPlayerData instance)
		{
			if (instance.ShouldPool)
			{
				instance.userid = 0uL;
				instance.dartsThrown = 0;
				instance.dartsThrownThisTurn = 0;
				instance.timeTaken = 0f;
				instance.score = 0;
				instance.scoreThisTurn = 0;
				instance.state = 0;
				instance.turn = 0;
				if (instance.scoreHistory != null)
				{
					List<int> obj = instance.scoreHistory;
					Pool.FreeUnmanaged(ref obj);
					instance.scoreHistory = obj;
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
				throw new Exception("Trying to dispose DartsPlayerData with ShouldPool set to false!");
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

		public void CopyTo(DartsPlayerData instance)
		{
			instance.userid = userid;
			instance.dartsThrown = dartsThrown;
			instance.dartsThrownThisTurn = dartsThrownThisTurn;
			instance.timeTaken = timeTaken;
			instance.score = score;
			instance.scoreThisTurn = scoreThisTurn;
			instance.state = state;
			instance.turn = turn;
			if (scoreHistory != null)
			{
				instance.scoreHistory = Pool.Get<List<int>>();
				for (int i = 0; i < scoreHistory.Count; i++)
				{
					int item = scoreHistory[i];
					instance.scoreHistory.Add(item);
				}
			}
			else
			{
				instance.scoreHistory = null;
			}
		}

		public DartsPlayerData Copy()
		{
			DartsPlayerData dartsPlayerData = Pool.Get<DartsPlayerData>();
			CopyTo(dartsPlayerData);
			return dartsPlayerData;
		}

		public static DartsPlayerData Deserialize(BufferStream stream)
		{
			DartsPlayerData dartsPlayerData = Pool.Get<DartsPlayerData>();
			Deserialize(stream, dartsPlayerData, isDelta: false);
			return dartsPlayerData;
		}

		public static DartsPlayerData DeserializeLengthDelimited(BufferStream stream)
		{
			DartsPlayerData dartsPlayerData = Pool.Get<DartsPlayerData>();
			DeserializeLengthDelimited(stream, dartsPlayerData, isDelta: false);
			return dartsPlayerData;
		}

		public static DartsPlayerData DeserializeLength(BufferStream stream, int length)
		{
			DartsPlayerData dartsPlayerData = Pool.Get<DartsPlayerData>();
			DeserializeLength(stream, length, dartsPlayerData, isDelta: false);
			return dartsPlayerData;
		}

		public static DartsPlayerData Deserialize(byte[] buffer)
		{
			DartsPlayerData dartsPlayerData = Pool.Get<DartsPlayerData>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, dartsPlayerData, isDelta: false);
			return dartsPlayerData;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, DartsPlayerData previous)
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

		public static DartsPlayerData Deserialize(BufferStream stream, DartsPlayerData instance, bool isDelta)
		{
			if (!isDelta && instance.scoreHistory == null)
			{
				instance.scoreHistory = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.DartsGame.DartsPlayerData");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.dartsThrown = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.dartsThrownThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.timeTaken = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.scoreThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.state = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.turn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					stream.ConsumeRepeatedElement();
					instance.scoreHistory.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DartsPlayerData DeserializeLengthDelimited(BufferStream stream, DartsPlayerData instance, bool isDelta)
		{
			if (!isDelta && instance.scoreHistory == null)
			{
				instance.scoreHistory = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.DartsGame.DartsPlayerData");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.dartsThrown = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.dartsThrownThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.timeTaken = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.scoreThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.state = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.turn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					stream.ConsumeRepeatedElement();
					instance.scoreHistory.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DartsPlayerData DeserializeLength(BufferStream stream, int length, DartsPlayerData instance, bool isDelta)
		{
			if (!isDelta && instance.scoreHistory == null)
			{
				instance.scoreHistory = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.DartsGame.DartsPlayerData");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.dartsThrown = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.dartsThrownThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.timeTaken = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.scoreThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.state = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					instance.turn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					stream.ConsumeRepeatedElement();
					instance.scoreHistory.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGame.DartsPlayerData");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, DartsPlayerData instance, DartsPlayerData previous)
		{
			if (instance.userid != previous.userid)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.dartsThrown != previous.dartsThrown)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dartsThrown);
			}
			if (instance.dartsThrownThisTurn != previous.dartsThrownThisTurn)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dartsThrownThisTurn);
			}
			if (instance.timeTaken != previous.timeTaken)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.timeTaken);
			}
			if (instance.score != previous.score)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
			}
			if (instance.scoreThisTurn != previous.scoreThisTurn)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.scoreThisTurn);
			}
			if (instance.state != previous.state)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
			}
			if (instance.turn != previous.turn)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.turn);
			}
			if (instance.scoreHistory != null)
			{
				for (int i = 0; i < instance.scoreHistory.Count; i++)
				{
					int num = instance.scoreHistory[i];
					stream.WriteByte(72);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
		}

		public static void Serialize(BufferStream stream, DartsPlayerData instance)
		{
			if (instance.userid != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.dartsThrown != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dartsThrown);
			}
			if (instance.dartsThrownThisTurn != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dartsThrownThisTurn);
			}
			if (instance.timeTaken != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.timeTaken);
			}
			if (instance.score != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
			}
			if (instance.scoreThisTurn != 0)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.scoreThisTurn);
			}
			if (instance.state != 0)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
			}
			if (instance.turn != 0)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.turn);
			}
			if (instance.scoreHistory != null)
			{
				for (int i = 0; i < instance.scoreHistory.Count; i++)
				{
					int num = instance.scoreHistory[i];
					stream.WriteByte(72);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
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
	public int gameType;

	[NonSerialized]
	public int state;

	[NonSerialized]
	public int activePlayerIndex;

	[NonSerialized]
	public List<DartsPlayerData> players;

	[NonSerialized]
	public NetworkableId mountableId;

	public static void ResetToPool(DartsGame instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.gameType = 0;
		instance.state = 0;
		instance.activePlayerIndex = 0;
		if (instance.players != null)
		{
			for (int i = 0; i < instance.players.Count; i++)
			{
				if (instance.players[i] != null)
				{
					instance.players[i].ResetToPool();
					instance.players[i] = null;
				}
			}
			List<DartsPlayerData> obj = instance.players;
			Pool.Free(ref obj, freeElements: false);
			instance.players = obj;
		}
		instance.mountableId = default(NetworkableId);
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
			throw new Exception("Trying to dispose DartsGame with ShouldPool set to false!");
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

	public void CopyTo(DartsGame instance)
	{
		instance.gameType = gameType;
		instance.state = state;
		instance.activePlayerIndex = activePlayerIndex;
		if (players != null)
		{
			instance.players = Pool.Get<List<DartsPlayerData>>();
			for (int i = 0; i < players.Count; i++)
			{
				DartsPlayerData item = players[i].Copy();
				instance.players.Add(item);
			}
		}
		else
		{
			instance.players = null;
		}
		instance.mountableId = mountableId;
	}

	public DartsGame Copy()
	{
		DartsGame dartsGame = Pool.Get<DartsGame>();
		CopyTo(dartsGame);
		return dartsGame;
	}

	public static DartsGame Deserialize(BufferStream stream)
	{
		DartsGame dartsGame = Pool.Get<DartsGame>();
		Deserialize(stream, dartsGame, isDelta: false);
		return dartsGame;
	}

	public static DartsGame DeserializeLengthDelimited(BufferStream stream)
	{
		DartsGame dartsGame = Pool.Get<DartsGame>();
		DeserializeLengthDelimited(stream, dartsGame, isDelta: false);
		return dartsGame;
	}

	public static DartsGame DeserializeLength(BufferStream stream, int length)
	{
		DartsGame dartsGame = Pool.Get<DartsGame>();
		DeserializeLength(stream, length, dartsGame, isDelta: false);
		return dartsGame;
	}

	public static DartsGame Deserialize(byte[] buffer)
	{
		DartsGame dartsGame = Pool.Get<DartsGame>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, dartsGame, isDelta: false);
		return dartsGame;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, DartsGame previous)
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

	public static DartsGame Deserialize(BufferStream stream, DartsGame instance, bool isDelta)
	{
		if (!isDelta && instance.players == null)
		{
			instance.players = Pool.Get<List<DartsPlayerData>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.DartsGame");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.gameType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.activePlayerIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				stream.ConsumeRepeatedElement();
				instance.players.Add(DartsPlayerData.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.mountableId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DartsGame DeserializeLengthDelimited(BufferStream stream, DartsGame instance, bool isDelta)
	{
		if (!isDelta && instance.players == null)
		{
			instance.players = Pool.Get<List<DartsPlayerData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DartsGame");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.gameType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.activePlayerIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				stream.ConsumeRepeatedElement();
				instance.players.Add(DartsPlayerData.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.mountableId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static DartsGame DeserializeLength(BufferStream stream, int length, DartsGame instance, bool isDelta)
	{
		if (!isDelta && instance.players == null)
		{
			instance.players = Pool.Get<List<DartsPlayerData>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.DartsGame");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.gameType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.activePlayerIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				stream.ConsumeRepeatedElement();
				instance.players.Add(DartsPlayerData.DeserializeLengthDelimited(stream));
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				instance.mountableId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.DartsGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, DartsGame instance, DartsGame previous)
	{
		if (instance.gameType != previous.gameType)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.gameType);
		}
		if (instance.state != previous.state)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.activePlayerIndex != previous.activePlayerIndex)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.activePlayerIndex);
		}
		if (instance.players != null)
		{
			for (int i = 0; i < instance.players.Count; i++)
			{
				DartsPlayerData dartsPlayerData = instance.players[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				DartsPlayerData.SerializeDelta(stream, dartsPlayerData, dartsPlayerData);
				int num = stream.Position - position;
				if (num > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field players (ProtoBuf.DartsGame.DartsPlayerData)");
				}
				Span<byte> span = range.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
				if (num2 < 3)
				{
					span[num2 - 1] |= 128;
					while (num2 < 2)
					{
						span[num2++] = 128;
					}
					span[2] = 0;
				}
			}
		}
		stream.WriteByte(40);
		ProtocolParser.WriteUInt64(stream, instance.mountableId.Value);
	}

	public static void Serialize(BufferStream stream, DartsGame instance)
	{
		if (instance.gameType != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.gameType);
		}
		if (instance.state != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
		}
		if (instance.activePlayerIndex != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.activePlayerIndex);
		}
		if (instance.players != null)
		{
			for (int i = 0; i < instance.players.Count; i++)
			{
				DartsPlayerData instance2 = instance.players[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				DartsPlayerData.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field players (ProtoBuf.DartsGame.DartsPlayerData)");
				}
				Span<byte> span = range.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)num, span, 0);
				if (num2 < 3)
				{
					span[num2 - 1] |= 128;
					while (num2 < 2)
					{
						span[num2++] = 128;
					}
					span[2] = 0;
				}
			}
		}
		if (instance.mountableId != default(NetworkableId))
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, instance.mountableId.Value);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (players != null)
		{
			for (int i = 0; i < players.Count; i++)
			{
				players[i]?.InspectUids(action);
			}
		}
		action(UidType.NetworkableId, ref mountableId.Value);
	}
}
