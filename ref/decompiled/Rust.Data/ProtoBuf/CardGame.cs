using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class CardGame : IDisposable, Pool.IPooled, IProto<CardGame>, IProto
{
	public class Blackjack : IDisposable, Pool.IPooled, IProto<Blackjack>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<int> dealerCards;

		[NonSerialized]
		public List<BlackjackCardPlayer> players;

		public static void ResetToPool(Blackjack instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			if (instance.dealerCards != null)
			{
				List<int> obj = instance.dealerCards;
				Pool.FreeUnmanaged(ref obj);
				instance.dealerCards = obj;
			}
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
				List<BlackjackCardPlayer> obj2 = instance.players;
				Pool.Free(ref obj2, freeElements: false);
				instance.players = obj2;
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
				throw new Exception("Trying to dispose Blackjack with ShouldPool set to false!");
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

		public void CopyTo(Blackjack instance)
		{
			if (dealerCards != null)
			{
				instance.dealerCards = Pool.Get<List<int>>();
				for (int i = 0; i < dealerCards.Count; i++)
				{
					int item = dealerCards[i];
					instance.dealerCards.Add(item);
				}
			}
			else
			{
				instance.dealerCards = null;
			}
			if (players != null)
			{
				instance.players = Pool.Get<List<BlackjackCardPlayer>>();
				for (int j = 0; j < players.Count; j++)
				{
					BlackjackCardPlayer item2 = players[j].Copy();
					instance.players.Add(item2);
				}
			}
			else
			{
				instance.players = null;
			}
		}

		public Blackjack Copy()
		{
			Blackjack blackjack = Pool.Get<Blackjack>();
			CopyTo(blackjack);
			return blackjack;
		}

		public static Blackjack Deserialize(BufferStream stream)
		{
			Blackjack blackjack = Pool.Get<Blackjack>();
			Deserialize(stream, blackjack, isDelta: false);
			return blackjack;
		}

		public static Blackjack DeserializeLengthDelimited(BufferStream stream)
		{
			Blackjack blackjack = Pool.Get<Blackjack>();
			DeserializeLengthDelimited(stream, blackjack, isDelta: false);
			return blackjack;
		}

		public static Blackjack DeserializeLength(BufferStream stream, int length)
		{
			Blackjack blackjack = Pool.Get<Blackjack>();
			DeserializeLength(stream, length, blackjack, isDelta: false);
			return blackjack;
		}

		public static Blackjack Deserialize(byte[] buffer)
		{
			Blackjack blackjack = Pool.Get<Blackjack>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, blackjack, isDelta: false);
			return blackjack;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Blackjack previous)
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

		public static Blackjack Deserialize(BufferStream stream, Blackjack instance, bool isDelta)
		{
			if (!isDelta)
			{
				if (instance.dealerCards == null)
				{
					instance.dealerCards = Pool.Get<List<int>>();
				}
				if (instance.players == null)
				{
					instance.players = Pool.Get<List<BlackjackCardPlayer>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.Blackjack");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					stream.ConsumeRepeatedElement();
					instance.dealerCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					stream.ConsumeRepeatedElement();
					instance.players.Add(BlackjackCardPlayer.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Blackjack DeserializeLengthDelimited(BufferStream stream, Blackjack instance, bool isDelta)
		{
			if (!isDelta)
			{
				if (instance.dealerCards == null)
				{
					instance.dealerCards = Pool.Get<List<int>>();
				}
				if (instance.players == null)
				{
					instance.players = Pool.Get<List<BlackjackCardPlayer>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.Blackjack");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					stream.ConsumeRepeatedElement();
					instance.dealerCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					stream.ConsumeRepeatedElement();
					instance.players.Add(BlackjackCardPlayer.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Blackjack DeserializeLength(BufferStream stream, int length, Blackjack instance, bool isDelta)
		{
			if (!isDelta)
			{
				if (instance.dealerCards == null)
				{
					instance.dealerCards = Pool.Get<List<int>>();
				}
				if (instance.players == null)
				{
					instance.players = Pool.Get<List<BlackjackCardPlayer>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.Blackjack");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					stream.ConsumeRepeatedElement();
					instance.dealerCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					stream.ConsumeRepeatedElement();
					instance.players.Add(BlackjackCardPlayer.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.Blackjack");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Blackjack instance, Blackjack previous)
		{
			if (instance.dealerCards != null)
			{
				for (int i = 0; i < instance.dealerCards.Count; i++)
				{
					int num = instance.dealerCards[i];
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.players == null)
			{
				return;
			}
			for (int j = 0; j < instance.players.Count; j++)
			{
				BlackjackCardPlayer blackjackCardPlayer = instance.players[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				BlackjackCardPlayer.SerializeDelta(stream, blackjackCardPlayer, blackjackCardPlayer);
				int num2 = stream.Position - position;
				if (num2 > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field players (ProtoBuf.CardGame.BlackjackCardPlayer)");
				}
				Span<byte> span = range.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)num2, span, 0);
				if (num3 < 3)
				{
					span[num3 - 1] |= 128;
					while (num3 < 2)
					{
						span[num3++] = 128;
					}
					span[2] = 0;
				}
			}
		}

		public static void Serialize(BufferStream stream, Blackjack instance)
		{
			if (instance.dealerCards != null)
			{
				for (int i = 0; i < instance.dealerCards.Count; i++)
				{
					int num = instance.dealerCards[i];
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.players == null)
			{
				return;
			}
			for (int j = 0; j < instance.players.Count; j++)
			{
				BlackjackCardPlayer instance2 = instance.players[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				BlackjackCardPlayer.Serialize(stream, instance2);
				int num2 = stream.Position - position;
				if (num2 > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field players (ProtoBuf.CardGame.BlackjackCardPlayer)");
				}
				Span<byte> span = range.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)num2, span, 0);
				if (num3 < 3)
				{
					span[num3 - 1] |= 128;
					while (num3 < 2)
					{
						span[num3++] = 128;
					}
					span[2] = 0;
				}
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
		}
	}

	public class BlackjackCardPlayer : IDisposable, Pool.IPooled, IProto<BlackjackCardPlayer>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<int> splitCards;

		[NonSerialized]
		public int splitBetThisRound;

		[NonSerialized]
		public int insuranceBetThisRound;

		[NonSerialized]
		public bool playingSplitCards;

		public static void ResetToPool(BlackjackCardPlayer instance)
		{
			if (instance.ShouldPool)
			{
				if (instance.splitCards != null)
				{
					List<int> obj = instance.splitCards;
					Pool.FreeUnmanaged(ref obj);
					instance.splitCards = obj;
				}
				instance.splitBetThisRound = 0;
				instance.insuranceBetThisRound = 0;
				instance.playingSplitCards = false;
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
				throw new Exception("Trying to dispose BlackjackCardPlayer with ShouldPool set to false!");
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

		public void CopyTo(BlackjackCardPlayer instance)
		{
			if (splitCards != null)
			{
				instance.splitCards = Pool.Get<List<int>>();
				for (int i = 0; i < splitCards.Count; i++)
				{
					int item = splitCards[i];
					instance.splitCards.Add(item);
				}
			}
			else
			{
				instance.splitCards = null;
			}
			instance.splitBetThisRound = splitBetThisRound;
			instance.insuranceBetThisRound = insuranceBetThisRound;
			instance.playingSplitCards = playingSplitCards;
		}

		public BlackjackCardPlayer Copy()
		{
			BlackjackCardPlayer blackjackCardPlayer = Pool.Get<BlackjackCardPlayer>();
			CopyTo(blackjackCardPlayer);
			return blackjackCardPlayer;
		}

		public static BlackjackCardPlayer Deserialize(BufferStream stream)
		{
			BlackjackCardPlayer blackjackCardPlayer = Pool.Get<BlackjackCardPlayer>();
			Deserialize(stream, blackjackCardPlayer, isDelta: false);
			return blackjackCardPlayer;
		}

		public static BlackjackCardPlayer DeserializeLengthDelimited(BufferStream stream)
		{
			BlackjackCardPlayer blackjackCardPlayer = Pool.Get<BlackjackCardPlayer>();
			DeserializeLengthDelimited(stream, blackjackCardPlayer, isDelta: false);
			return blackjackCardPlayer;
		}

		public static BlackjackCardPlayer DeserializeLength(BufferStream stream, int length)
		{
			BlackjackCardPlayer blackjackCardPlayer = Pool.Get<BlackjackCardPlayer>();
			DeserializeLength(stream, length, blackjackCardPlayer, isDelta: false);
			return blackjackCardPlayer;
		}

		public static BlackjackCardPlayer Deserialize(byte[] buffer)
		{
			BlackjackCardPlayer blackjackCardPlayer = Pool.Get<BlackjackCardPlayer>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, blackjackCardPlayer, isDelta: false);
			return blackjackCardPlayer;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, BlackjackCardPlayer previous)
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

		public static BlackjackCardPlayer Deserialize(BufferStream stream, BlackjackCardPlayer instance, bool isDelta)
		{
			if (!isDelta && instance.splitCards == null)
			{
				instance.splitCards = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.BlackjackCardPlayer");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					stream.ConsumeRepeatedElement();
					instance.splitCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.splitBetThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.insuranceBetThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.playingSplitCards = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static BlackjackCardPlayer DeserializeLengthDelimited(BufferStream stream, BlackjackCardPlayer instance, bool isDelta)
		{
			if (!isDelta && instance.splitCards == null)
			{
				instance.splitCards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.BlackjackCardPlayer");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					stream.ConsumeRepeatedElement();
					instance.splitCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.splitBetThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.insuranceBetThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.playingSplitCards = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static BlackjackCardPlayer DeserializeLength(BufferStream stream, int length, BlackjackCardPlayer instance, bool isDelta)
		{
			if (!isDelta && instance.splitCards == null)
			{
				instance.splitCards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.BlackjackCardPlayer");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					stream.ConsumeRepeatedElement();
					instance.splitCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.splitBetThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.insuranceBetThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					instance.playingSplitCards = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.BlackjackCardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, BlackjackCardPlayer instance, BlackjackCardPlayer previous)
		{
			if (instance.splitCards != null)
			{
				for (int i = 0; i < instance.splitCards.Count; i++)
				{
					int num = instance.splitCards[i];
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.splitBetThisRound != previous.splitBetThisRound)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.splitBetThisRound);
			}
			if (instance.insuranceBetThisRound != previous.insuranceBetThisRound)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.insuranceBetThisRound);
			}
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.playingSplitCards);
		}

		public static void Serialize(BufferStream stream, BlackjackCardPlayer instance)
		{
			if (instance.splitCards != null)
			{
				for (int i = 0; i < instance.splitCards.Count; i++)
				{
					int num = instance.splitCards[i];
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.splitBetThisRound != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.splitBetThisRound);
			}
			if (instance.insuranceBetThisRound != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.insuranceBetThisRound);
			}
			if (instance.playingSplitCards)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteBool(stream, instance.playingSplitCards);
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

	public class CardList : IDisposable, Pool.IPooled, IProto<CardList>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public List<int> cards;

		public static void ResetToPool(CardList instance)
		{
			if (instance.ShouldPool)
			{
				if (instance.cards != null)
				{
					List<int> obj = instance.cards;
					Pool.FreeUnmanaged(ref obj);
					instance.cards = obj;
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
				throw new Exception("Trying to dispose CardList with ShouldPool set to false!");
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

		public void CopyTo(CardList instance)
		{
			if (cards != null)
			{
				instance.cards = Pool.Get<List<int>>();
				for (int i = 0; i < cards.Count; i++)
				{
					int item = cards[i];
					instance.cards.Add(item);
				}
			}
			else
			{
				instance.cards = null;
			}
		}

		public CardList Copy()
		{
			CardList cardList = Pool.Get<CardList>();
			CopyTo(cardList);
			return cardList;
		}

		public static CardList Deserialize(BufferStream stream)
		{
			CardList cardList = Pool.Get<CardList>();
			Deserialize(stream, cardList, isDelta: false);
			return cardList;
		}

		public static CardList DeserializeLengthDelimited(BufferStream stream)
		{
			CardList cardList = Pool.Get<CardList>();
			DeserializeLengthDelimited(stream, cardList, isDelta: false);
			return cardList;
		}

		public static CardList DeserializeLength(BufferStream stream, int length)
		{
			CardList cardList = Pool.Get<CardList>();
			DeserializeLength(stream, length, cardList, isDelta: false);
			return cardList;
		}

		public static CardList Deserialize(byte[] buffer)
		{
			CardList cardList = Pool.Get<CardList>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, cardList, isDelta: false);
			return cardList;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, CardList previous)
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

		public static CardList Deserialize(BufferStream stream, CardList instance, bool isDelta)
		{
			if (!isDelta && instance.cards == null)
			{
				instance.cards = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.CardList");
				if (num == 8)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					stream.ConsumeRepeatedElement();
					instance.cards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static CardList DeserializeLengthDelimited(BufferStream stream, CardList instance, bool isDelta)
		{
			if (!isDelta && instance.cards == null)
			{
				instance.cards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.CardList");
				if (num2 == 8)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					stream.ConsumeRepeatedElement();
					instance.cards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static CardList DeserializeLength(BufferStream stream, int length, CardList instance, bool isDelta)
		{
			if (!isDelta && instance.cards == null)
			{
				instance.cards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.CardList");
				if (num2 == 8)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					stream.ConsumeRepeatedElement();
					instance.cards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.CardList");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, CardList instance, CardList previous)
		{
			if (instance.cards != null)
			{
				for (int i = 0; i < instance.cards.Count; i++)
				{
					int num = instance.cards[i];
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
		}

		public static void Serialize(BufferStream stream, CardList instance)
		{
			if (instance.cards != null)
			{
				for (int i = 0; i < instance.cards.Count; i++)
				{
					int num = instance.cards[i];
					stream.WriteByte(8);
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

	public class CardPlayer : IDisposable, Pool.IPooled, IProto<CardPlayer>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong userid;

		[NonSerialized]
		public List<int> cards;

		[NonSerialized]
		public int scrap;

		[NonSerialized]
		public int state;

		[NonSerialized]
		public int availableInputs;

		[NonSerialized]
		public int betThisRound;

		[NonSerialized]
		public int betThisTurn;

		[NonSerialized]
		public bool leftRoundEarly;

		[NonSerialized]
		public bool sendCardDetails;

		public static void ResetToPool(CardPlayer instance)
		{
			if (instance.ShouldPool)
			{
				instance.userid = 0uL;
				if (instance.cards != null)
				{
					List<int> obj = instance.cards;
					Pool.FreeUnmanaged(ref obj);
					instance.cards = obj;
				}
				instance.scrap = 0;
				instance.state = 0;
				instance.availableInputs = 0;
				instance.betThisRound = 0;
				instance.betThisTurn = 0;
				instance.leftRoundEarly = false;
				instance.sendCardDetails = false;
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
				throw new Exception("Trying to dispose CardPlayer with ShouldPool set to false!");
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

		public void CopyTo(CardPlayer instance)
		{
			instance.userid = userid;
			if (cards != null)
			{
				instance.cards = Pool.Get<List<int>>();
				for (int i = 0; i < cards.Count; i++)
				{
					int item = cards[i];
					instance.cards.Add(item);
				}
			}
			else
			{
				instance.cards = null;
			}
			instance.scrap = scrap;
			instance.state = state;
			instance.availableInputs = availableInputs;
			instance.betThisRound = betThisRound;
			instance.betThisTurn = betThisTurn;
			instance.leftRoundEarly = leftRoundEarly;
			instance.sendCardDetails = sendCardDetails;
		}

		public CardPlayer Copy()
		{
			CardPlayer cardPlayer = Pool.Get<CardPlayer>();
			CopyTo(cardPlayer);
			return cardPlayer;
		}

		public static CardPlayer Deserialize(BufferStream stream)
		{
			CardPlayer cardPlayer = Pool.Get<CardPlayer>();
			Deserialize(stream, cardPlayer, isDelta: false);
			return cardPlayer;
		}

		public static CardPlayer DeserializeLengthDelimited(BufferStream stream)
		{
			CardPlayer cardPlayer = Pool.Get<CardPlayer>();
			DeserializeLengthDelimited(stream, cardPlayer, isDelta: false);
			return cardPlayer;
		}

		public static CardPlayer DeserializeLength(BufferStream stream, int length)
		{
			CardPlayer cardPlayer = Pool.Get<CardPlayer>();
			DeserializeLength(stream, length, cardPlayer, isDelta: false);
			return cardPlayer;
		}

		public static CardPlayer Deserialize(byte[] buffer)
		{
			CardPlayer cardPlayer = Pool.Get<CardPlayer>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, cardPlayer, isDelta: false);
			return cardPlayer;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, CardPlayer previous)
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

		public static CardPlayer Deserialize(BufferStream stream, CardPlayer instance, bool isDelta)
		{
			if (!isDelta && instance.cards == null)
			{
				instance.cards = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.CardPlayer");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					stream.ConsumeRepeatedElement();
					instance.cards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.scrap = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.state = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.availableInputs = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.betThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.betThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.leftRoundEarly = ProtocolParser.ReadBool(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.sendCardDetails = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static CardPlayer DeserializeLengthDelimited(BufferStream stream, CardPlayer instance, bool isDelta)
		{
			if (!isDelta && instance.cards == null)
			{
				instance.cards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.CardPlayer");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					stream.ConsumeRepeatedElement();
					instance.cards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.scrap = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.state = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.availableInputs = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.betThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.betThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.leftRoundEarly = ProtocolParser.ReadBool(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.sendCardDetails = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static CardPlayer DeserializeLength(BufferStream stream, int length, CardPlayer instance, bool isDelta)
		{
			if (!isDelta && instance.cards == null)
			{
				instance.cards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.CardPlayer");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					stream.ConsumeRepeatedElement();
					instance.cards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.scrap = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.state = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.availableInputs = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.betThisRound = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.betThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.leftRoundEarly = ProtocolParser.ReadBool(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					instance.sendCardDetails = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.CardPlayer");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, CardPlayer instance, CardPlayer previous)
		{
			if (instance.userid != previous.userid)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.cards != null)
			{
				for (int i = 0; i < instance.cards.Count; i++)
				{
					int num = instance.cards[i];
					stream.WriteByte(16);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.scrap != previous.scrap)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.scrap);
			}
			if (instance.state != previous.state)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
			}
			if (instance.availableInputs != previous.availableInputs)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.availableInputs);
			}
			if (instance.betThisRound != previous.betThisRound)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.betThisRound);
			}
			if (instance.betThisTurn != previous.betThisTurn)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.betThisTurn);
			}
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.leftRoundEarly);
			stream.WriteByte(72);
			ProtocolParser.WriteBool(stream, instance.sendCardDetails);
		}

		public static void Serialize(BufferStream stream, CardPlayer instance)
		{
			if (instance.userid != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.cards != null)
			{
				for (int i = 0; i < instance.cards.Count; i++)
				{
					int num = instance.cards[i];
					stream.WriteByte(16);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.scrap != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.scrap);
			}
			if (instance.state != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.state);
			}
			if (instance.availableInputs != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.availableInputs);
			}
			if (instance.betThisRound != 0)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.betThisRound);
			}
			if (instance.betThisTurn != 0)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.betThisTurn);
			}
			if (instance.leftRoundEarly)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteBool(stream, instance.leftRoundEarly);
			}
			if (instance.sendCardDetails)
			{
				stream.WriteByte(72);
				ProtocolParser.WriteBool(stream, instance.sendCardDetails);
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

	public class RoundResults : IDisposable, Pool.IPooled, IProto<RoundResults>, IProto
	{
		public class Result : IDisposable, Pool.IPooled, IProto<Result>, IProto
		{
			public bool ShouldPool = true;

			private bool _disposed;

			[NonSerialized]
			public ulong ID;

			[NonSerialized]
			public int winnings;

			[NonSerialized]
			public int resultCode;

			public static void ResetToPool(Result instance)
			{
				if (instance.ShouldPool)
				{
					instance.ID = 0uL;
					instance.winnings = 0;
					instance.resultCode = 0;
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
					throw new Exception("Trying to dispose Result with ShouldPool set to false!");
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

			public void CopyTo(Result instance)
			{
				instance.ID = ID;
				instance.winnings = winnings;
				instance.resultCode = resultCode;
			}

			public Result Copy()
			{
				Result result = Pool.Get<Result>();
				CopyTo(result);
				return result;
			}

			public static Result Deserialize(BufferStream stream)
			{
				Result result = Pool.Get<Result>();
				Deserialize(stream, result, isDelta: false);
				return result;
			}

			public static Result DeserializeLengthDelimited(BufferStream stream)
			{
				Result result = Pool.Get<Result>();
				DeserializeLengthDelimited(stream, result, isDelta: false);
				return result;
			}

			public static Result DeserializeLength(BufferStream stream, int length)
			{
				Result result = Pool.Get<Result>();
				DeserializeLength(stream, length, result, isDelta: false);
				return result;
			}

			public static Result Deserialize(byte[] buffer)
			{
				Result result = Pool.Get<Result>();
				using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
				Deserialize(stream, result, isDelta: false);
				return result;
			}

			public void FromProto(BufferStream stream, bool isDelta = false)
			{
				Deserialize(stream, this, isDelta);
			}

			public virtual void WriteToStream(BufferStream stream)
			{
				Serialize(stream, this);
			}

			public virtual void WriteToStreamDelta(BufferStream stream, Result previous)
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

			public static Result Deserialize(BufferStream stream, Result instance, bool isDelta)
			{
				uint lastFieldId = 0u;
				while (true)
				{
					int num = stream.ReadByte();
					if (num == -1 || num == 0)
					{
						break;
					}
					stream.ConsumeFieldOperation("ProtoBuf.CardGame.RoundResults.Result");
					switch (num)
					{
					case 8:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.ID = ProtocolParser.ReadUInt64(stream);
						continue;
					case 16:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.winnings = (int)ProtocolParser.ReadUInt64(stream);
						continue;
					case 24:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.resultCode = (int)ProtocolParser.ReadUInt64(stream);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static Result DeserializeLengthDelimited(BufferStream stream, Result instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.CardGame.RoundResults.Result");
					switch (num2)
					{
					case 8:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.ID = ProtocolParser.ReadUInt64(stream);
						continue;
					case 16:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.winnings = (int)ProtocolParser.ReadUInt64(stream);
						continue;
					case 24:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.resultCode = (int)ProtocolParser.ReadUInt64(stream);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static Result DeserializeLength(BufferStream stream, int length, Result instance, bool isDelta)
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
					stream.ConsumeFieldOperation("ProtoBuf.CardGame.RoundResults.Result");
					switch (num2)
					{
					case 8:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.ID = ProtocolParser.ReadUInt64(stream);
						continue;
					case 16:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.winnings = (int)ProtocolParser.ReadUInt64(stream);
						continue;
					case 24:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						instance.resultCode = (int)ProtocolParser.ReadUInt64(stream);
						continue;
					}
					Key key = ProtocolParser.ReadKey((byte)num2, stream);
					switch (key.Field)
					{
					case 1u:
						stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 2u:
						stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					case 3u:
						stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					default:
						stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults.Result");
						ProtocolParser.SkipKey(stream, key);
						break;
					}
				}
				return instance;
			}

			public static void SerializeDelta(BufferStream stream, Result instance, Result previous)
			{
				if (instance.ID != previous.ID)
				{
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, instance.ID);
				}
				if (instance.winnings != previous.winnings)
				{
					stream.WriteByte(16);
					ProtocolParser.WriteUInt64(stream, (ulong)instance.winnings);
				}
				if (instance.resultCode != previous.resultCode)
				{
					stream.WriteByte(24);
					ProtocolParser.WriteUInt64(stream, (ulong)instance.resultCode);
				}
			}

			public static void Serialize(BufferStream stream, Result instance)
			{
				if (instance.ID != 0L)
				{
					stream.WriteByte(8);
					ProtocolParser.WriteUInt64(stream, instance.ID);
				}
				if (instance.winnings != 0)
				{
					stream.WriteByte(16);
					ProtocolParser.WriteUInt64(stream, (ulong)instance.winnings);
				}
				if (instance.resultCode != 0)
				{
					stream.WriteByte(24);
					ProtocolParser.WriteUInt64(stream, (ulong)instance.resultCode);
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
		public List<Result> results;

		[NonSerialized]
		public int winningScore;

		public static void ResetToPool(RoundResults instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			if (instance.results != null)
			{
				for (int i = 0; i < instance.results.Count; i++)
				{
					if (instance.results[i] != null)
					{
						instance.results[i].ResetToPool();
						instance.results[i] = null;
					}
				}
				List<Result> obj = instance.results;
				Pool.Free(ref obj, freeElements: false);
				instance.results = obj;
			}
			instance.winningScore = 0;
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
				throw new Exception("Trying to dispose RoundResults with ShouldPool set to false!");
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

		public void CopyTo(RoundResults instance)
		{
			if (results != null)
			{
				instance.results = Pool.Get<List<Result>>();
				for (int i = 0; i < results.Count; i++)
				{
					Result item = results[i].Copy();
					instance.results.Add(item);
				}
			}
			else
			{
				instance.results = null;
			}
			instance.winningScore = winningScore;
		}

		public RoundResults Copy()
		{
			RoundResults roundResults = Pool.Get<RoundResults>();
			CopyTo(roundResults);
			return roundResults;
		}

		public static RoundResults Deserialize(BufferStream stream)
		{
			RoundResults roundResults = Pool.Get<RoundResults>();
			Deserialize(stream, roundResults, isDelta: false);
			return roundResults;
		}

		public static RoundResults DeserializeLengthDelimited(BufferStream stream)
		{
			RoundResults roundResults = Pool.Get<RoundResults>();
			DeserializeLengthDelimited(stream, roundResults, isDelta: false);
			return roundResults;
		}

		public static RoundResults DeserializeLength(BufferStream stream, int length)
		{
			RoundResults roundResults = Pool.Get<RoundResults>();
			DeserializeLength(stream, length, roundResults, isDelta: false);
			return roundResults;
		}

		public static RoundResults Deserialize(byte[] buffer)
		{
			RoundResults roundResults = Pool.Get<RoundResults>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, roundResults, isDelta: false);
			return roundResults;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, RoundResults previous)
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

		public static RoundResults Deserialize(BufferStream stream, RoundResults instance, bool isDelta)
		{
			if (!isDelta && instance.results == null)
			{
				instance.results = Pool.Get<List<Result>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.RoundResults");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					stream.ConsumeRepeatedElement();
					instance.results.Add(Result.DeserializeLengthDelimited(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults");
					instance.winningScore = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static RoundResults DeserializeLengthDelimited(BufferStream stream, RoundResults instance, bool isDelta)
		{
			if (!isDelta && instance.results == null)
			{
				instance.results = Pool.Get<List<Result>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.RoundResults");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					stream.ConsumeRepeatedElement();
					instance.results.Add(Result.DeserializeLengthDelimited(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults");
					instance.winningScore = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static RoundResults DeserializeLength(BufferStream stream, int length, RoundResults instance, bool isDelta)
		{
			if (!isDelta && instance.results == null)
			{
				instance.results = Pool.Get<List<Result>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.RoundResults");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					stream.ConsumeRepeatedElement();
					instance.results.Add(Result.DeserializeLengthDelimited(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults");
					instance.winningScore = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.RoundResults");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, RoundResults instance, RoundResults previous)
		{
			if (instance.results != null)
			{
				for (int i = 0; i < instance.results.Count; i++)
				{
					Result result = instance.results[i];
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Result.SerializeDelta(stream, result, result);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field results (ProtoBuf.CardGame.RoundResults.Result)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}
			if (instance.winningScore != previous.winningScore)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.winningScore);
			}
		}

		public static void Serialize(BufferStream stream, RoundResults instance)
		{
			if (instance.results != null)
			{
				for (int i = 0; i < instance.results.Count; i++)
				{
					Result instance2 = instance.results[i];
					stream.WriteByte(10);
					BufferStream.RangeHandle range = stream.GetRange(1);
					int position = stream.Position;
					Result.Serialize(stream, instance2);
					int num = stream.Position - position;
					if (num > 127)
					{
						throw new InvalidOperationException("Not enough space was reserved for the length prefix of field results (ProtoBuf.CardGame.RoundResults.Result)");
					}
					Span<byte> span = range.GetSpan();
					ProtocolParser.WriteUInt32((uint)num, span, 0);
				}
			}
			if (instance.winningScore != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.winningScore);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			if (results != null)
			{
				for (int i = 0; i < results.Count; i++)
				{
					results[i]?.InspectUids(action);
				}
			}
		}
	}

	public class TexasHoldEm : IDisposable, Pool.IPooled, IProto<TexasHoldEm>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int dealerIndex;

		[NonSerialized]
		public List<int> communityCards;

		[NonSerialized]
		public int biggestRaiseThisTurn;

		public static void ResetToPool(TexasHoldEm instance)
		{
			if (instance.ShouldPool)
			{
				instance.dealerIndex = 0;
				if (instance.communityCards != null)
				{
					List<int> obj = instance.communityCards;
					Pool.FreeUnmanaged(ref obj);
					instance.communityCards = obj;
				}
				instance.biggestRaiseThisTurn = 0;
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
				throw new Exception("Trying to dispose TexasHoldEm with ShouldPool set to false!");
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

		public void CopyTo(TexasHoldEm instance)
		{
			instance.dealerIndex = dealerIndex;
			if (communityCards != null)
			{
				instance.communityCards = Pool.Get<List<int>>();
				for (int i = 0; i < communityCards.Count; i++)
				{
					int item = communityCards[i];
					instance.communityCards.Add(item);
				}
			}
			else
			{
				instance.communityCards = null;
			}
			instance.biggestRaiseThisTurn = biggestRaiseThisTurn;
		}

		public TexasHoldEm Copy()
		{
			TexasHoldEm texasHoldEm = Pool.Get<TexasHoldEm>();
			CopyTo(texasHoldEm);
			return texasHoldEm;
		}

		public static TexasHoldEm Deserialize(BufferStream stream)
		{
			TexasHoldEm texasHoldEm = Pool.Get<TexasHoldEm>();
			Deserialize(stream, texasHoldEm, isDelta: false);
			return texasHoldEm;
		}

		public static TexasHoldEm DeserializeLengthDelimited(BufferStream stream)
		{
			TexasHoldEm texasHoldEm = Pool.Get<TexasHoldEm>();
			DeserializeLengthDelimited(stream, texasHoldEm, isDelta: false);
			return texasHoldEm;
		}

		public static TexasHoldEm DeserializeLength(BufferStream stream, int length)
		{
			TexasHoldEm texasHoldEm = Pool.Get<TexasHoldEm>();
			DeserializeLength(stream, length, texasHoldEm, isDelta: false);
			return texasHoldEm;
		}

		public static TexasHoldEm Deserialize(byte[] buffer)
		{
			TexasHoldEm texasHoldEm = Pool.Get<TexasHoldEm>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, texasHoldEm, isDelta: false);
			return texasHoldEm;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, TexasHoldEm previous)
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

		public static TexasHoldEm Deserialize(BufferStream stream, TexasHoldEm instance, bool isDelta)
		{
			if (!isDelta && instance.communityCards == null)
			{
				instance.communityCards = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.TexasHoldEm");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					instance.dealerIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					stream.ConsumeRepeatedElement();
					instance.communityCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					instance.biggestRaiseThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static TexasHoldEm DeserializeLengthDelimited(BufferStream stream, TexasHoldEm instance, bool isDelta)
		{
			if (!isDelta && instance.communityCards == null)
			{
				instance.communityCards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.TexasHoldEm");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					instance.dealerIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					stream.ConsumeRepeatedElement();
					instance.communityCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					instance.biggestRaiseThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static TexasHoldEm DeserializeLength(BufferStream stream, int length, TexasHoldEm instance, bool isDelta)
		{
			if (!isDelta && instance.communityCards == null)
			{
				instance.communityCards = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.CardGame.TexasHoldEm");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					instance.dealerIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					stream.ConsumeRepeatedElement();
					instance.communityCards.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					instance.biggestRaiseThisTurn = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame.TexasHoldEm");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, TexasHoldEm instance, TexasHoldEm previous)
		{
			if (instance.dealerIndex != previous.dealerIndex)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dealerIndex);
			}
			if (instance.communityCards != null)
			{
				for (int i = 0; i < instance.communityCards.Count; i++)
				{
					int num = instance.communityCards[i];
					stream.WriteByte(16);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.biggestRaiseThisTurn != previous.biggestRaiseThisTurn)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.biggestRaiseThisTurn);
			}
		}

		public static void Serialize(BufferStream stream, TexasHoldEm instance)
		{
			if (instance.dealerIndex != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.dealerIndex);
			}
			if (instance.communityCards != null)
			{
				for (int i = 0; i < instance.communityCards.Count; i++)
				{
					int num = instance.communityCards[i];
					stream.WriteByte(16);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.biggestRaiseThisTurn != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.biggestRaiseThisTurn);
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
	public List<CardPlayer> players;

	[NonSerialized]
	public int state;

	[NonSerialized]
	public int activePlayerIndex;

	[NonSerialized]
	public int pot;

	[NonSerialized]
	public int lastActionId;

	[NonSerialized]
	public ulong lastActionTarget;

	[NonSerialized]
	public int lastActionValue;

	[NonSerialized]
	public NetworkableId potRef;

	[NonSerialized]
	public TexasHoldEm texasHoldEm;

	[NonSerialized]
	public Blackjack blackjack;

	public static void ResetToPool(CardGame instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
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
			List<CardPlayer> obj = instance.players;
			Pool.Free(ref obj, freeElements: false);
			instance.players = obj;
		}
		instance.state = 0;
		instance.activePlayerIndex = 0;
		instance.pot = 0;
		instance.lastActionId = 0;
		instance.lastActionTarget = 0uL;
		instance.lastActionValue = 0;
		instance.potRef = default(NetworkableId);
		if (instance.texasHoldEm != null)
		{
			instance.texasHoldEm.ResetToPool();
			instance.texasHoldEm = null;
		}
		if (instance.blackjack != null)
		{
			instance.blackjack.ResetToPool();
			instance.blackjack = null;
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
			throw new Exception("Trying to dispose CardGame with ShouldPool set to false!");
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

	public void CopyTo(CardGame instance)
	{
		if (players != null)
		{
			instance.players = Pool.Get<List<CardPlayer>>();
			for (int i = 0; i < players.Count; i++)
			{
				CardPlayer item = players[i].Copy();
				instance.players.Add(item);
			}
		}
		else
		{
			instance.players = null;
		}
		instance.state = state;
		instance.activePlayerIndex = activePlayerIndex;
		instance.pot = pot;
		instance.lastActionId = lastActionId;
		instance.lastActionTarget = lastActionTarget;
		instance.lastActionValue = lastActionValue;
		instance.potRef = potRef;
		if (texasHoldEm != null)
		{
			if (instance.texasHoldEm == null)
			{
				instance.texasHoldEm = texasHoldEm.Copy();
			}
			else
			{
				texasHoldEm.CopyTo(instance.texasHoldEm);
			}
		}
		else
		{
			instance.texasHoldEm = null;
		}
		if (blackjack != null)
		{
			if (instance.blackjack == null)
			{
				instance.blackjack = blackjack.Copy();
			}
			else
			{
				blackjack.CopyTo(instance.blackjack);
			}
		}
		else
		{
			instance.blackjack = null;
		}
	}

	public CardGame Copy()
	{
		CardGame cardGame = Pool.Get<CardGame>();
		CopyTo(cardGame);
		return cardGame;
	}

	public static CardGame Deserialize(BufferStream stream)
	{
		CardGame cardGame = Pool.Get<CardGame>();
		Deserialize(stream, cardGame, isDelta: false);
		return cardGame;
	}

	public static CardGame DeserializeLengthDelimited(BufferStream stream)
	{
		CardGame cardGame = Pool.Get<CardGame>();
		DeserializeLengthDelimited(stream, cardGame, isDelta: false);
		return cardGame;
	}

	public static CardGame DeserializeLength(BufferStream stream, int length)
	{
		CardGame cardGame = Pool.Get<CardGame>();
		DeserializeLength(stream, length, cardGame, isDelta: false);
		return cardGame;
	}

	public static CardGame Deserialize(byte[] buffer)
	{
		CardGame cardGame = Pool.Get<CardGame>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, cardGame, isDelta: false);
		return cardGame;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, CardGame previous)
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

	public static CardGame Deserialize(BufferStream stream, CardGame instance, bool isDelta)
	{
		if (!isDelta && instance.players == null)
		{
			instance.players = Pool.Get<List<CardPlayer>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.CardGame");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame");
				stream.ConsumeRepeatedElement();
				instance.players.Add(CardPlayer.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.activePlayerIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.pot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionTarget = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionValue = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.potRef = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				if (instance.texasHoldEm == null)
				{
					instance.texasHoldEm = TexasHoldEm.DeserializeLengthDelimited(stream);
				}
				else
				{
					TexasHoldEm.DeserializeLengthDelimited(stream, instance.texasHoldEm, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				if (instance.blackjack == null)
				{
					instance.blackjack = Blackjack.DeserializeLengthDelimited(stream);
				}
				else
				{
					Blackjack.DeserializeLengthDelimited(stream, instance.blackjack, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CardGame DeserializeLengthDelimited(BufferStream stream, CardGame instance, bool isDelta)
	{
		if (!isDelta && instance.players == null)
		{
			instance.players = Pool.Get<List<CardPlayer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CardGame");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame");
				stream.ConsumeRepeatedElement();
				instance.players.Add(CardPlayer.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.activePlayerIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.pot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionTarget = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionValue = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.potRef = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				if (instance.texasHoldEm == null)
				{
					instance.texasHoldEm = TexasHoldEm.DeserializeLengthDelimited(stream);
				}
				else
				{
					TexasHoldEm.DeserializeLengthDelimited(stream, instance.texasHoldEm, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				if (instance.blackjack == null)
				{
					instance.blackjack = Blackjack.DeserializeLengthDelimited(stream);
				}
				else
				{
					Blackjack.DeserializeLengthDelimited(stream, instance.blackjack, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static CardGame DeserializeLength(BufferStream stream, int length, CardGame instance, bool isDelta)
	{
		if (!isDelta && instance.players == null)
		{
			instance.players = Pool.Get<List<CardPlayer>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.CardGame");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame");
				stream.ConsumeRepeatedElement();
				instance.players.Add(CardPlayer.DeserializeLengthDelimited(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.state = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.activePlayerIndex = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.pot = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 40:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionId = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionTarget = ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.lastActionValue = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 64:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				instance.potRef = new NetworkableId(ProtocolParser.ReadUInt64(stream));
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				if (instance.texasHoldEm == null)
				{
					instance.texasHoldEm = TexasHoldEm.DeserializeLengthDelimited(stream);
				}
				else
				{
					TexasHoldEm.DeserializeLengthDelimited(stream, instance.texasHoldEm, isDelta);
				}
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				if (instance.blackjack == null)
				{
					instance.blackjack = Blackjack.DeserializeLengthDelimited(stream);
				}
				else
				{
					Blackjack.DeserializeLengthDelimited(stream, instance.blackjack, isDelta);
				}
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.CardGame");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, CardGame instance, CardGame previous)
	{
		if (instance.players != null)
		{
			for (int i = 0; i < instance.players.Count; i++)
			{
				CardPlayer cardPlayer = instance.players[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				CardPlayer.SerializeDelta(stream, cardPlayer, cardPlayer);
				int num = stream.Position - position;
				if (num > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field players (ProtoBuf.CardGame.CardPlayer)");
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
		if (instance.pot != previous.pot)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.pot);
		}
		if (instance.lastActionId != previous.lastActionId)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastActionId);
		}
		if (instance.lastActionTarget != previous.lastActionTarget)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.lastActionTarget);
		}
		if (instance.lastActionValue != previous.lastActionValue)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastActionValue);
		}
		stream.WriteByte(64);
		ProtocolParser.WriteUInt64(stream, instance.potRef.Value);
		if (instance.texasHoldEm != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range2 = stream.GetRange(3);
			int position2 = stream.Position;
			TexasHoldEm.SerializeDelta(stream, instance.texasHoldEm, previous.texasHoldEm);
			int num3 = stream.Position - position2;
			if (num3 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field texasHoldEm (ProtoBuf.CardGame.TexasHoldEm)");
			}
			Span<byte> span2 = range2.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)num3, span2, 0);
			if (num4 < 3)
			{
				span2[num4 - 1] |= 128;
				while (num4 < 2)
				{
					span2[num4++] = 128;
				}
				span2[2] = 0;
			}
		}
		if (instance.blackjack == null)
		{
			return;
		}
		stream.WriteByte(82);
		BufferStream.RangeHandle range3 = stream.GetRange(5);
		int position3 = stream.Position;
		Blackjack.SerializeDelta(stream, instance.blackjack, previous.blackjack);
		int num5 = stream.Position - position3;
		if (num5 > 34359738367L)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field blackjack (ProtoBuf.CardGame.Blackjack)");
		}
		Span<byte> span3 = range3.GetSpan();
		int num6 = ProtocolParser.WriteUInt32((uint)num5, span3, 0);
		if (num6 < 5)
		{
			span3[num6 - 1] |= 128;
			while (num6 < 4)
			{
				span3[num6++] = 128;
			}
			span3[4] = 0;
		}
	}

	public static void Serialize(BufferStream stream, CardGame instance)
	{
		if (instance.players != null)
		{
			for (int i = 0; i < instance.players.Count; i++)
			{
				CardPlayer instance2 = instance.players[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(3);
				int position = stream.Position;
				CardPlayer.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 2097151)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field players (ProtoBuf.CardGame.CardPlayer)");
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
		if (instance.pot != 0)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.pot);
		}
		if (instance.lastActionId != 0)
		{
			stream.WriteByte(40);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastActionId);
		}
		if (instance.lastActionTarget != 0L)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, instance.lastActionTarget);
		}
		if (instance.lastActionValue != 0)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastActionValue);
		}
		if (instance.potRef != default(NetworkableId))
		{
			stream.WriteByte(64);
			ProtocolParser.WriteUInt64(stream, instance.potRef.Value);
		}
		if (instance.texasHoldEm != null)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range2 = stream.GetRange(3);
			int position2 = stream.Position;
			TexasHoldEm.Serialize(stream, instance.texasHoldEm);
			int num3 = stream.Position - position2;
			if (num3 > 2097151)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field texasHoldEm (ProtoBuf.CardGame.TexasHoldEm)");
			}
			Span<byte> span2 = range2.GetSpan();
			int num4 = ProtocolParser.WriteUInt32((uint)num3, span2, 0);
			if (num4 < 3)
			{
				span2[num4 - 1] |= 128;
				while (num4 < 2)
				{
					span2[num4++] = 128;
				}
				span2[2] = 0;
			}
		}
		if (instance.blackjack == null)
		{
			return;
		}
		stream.WriteByte(82);
		BufferStream.RangeHandle range3 = stream.GetRange(5);
		int position3 = stream.Position;
		Blackjack.Serialize(stream, instance.blackjack);
		int num5 = stream.Position - position3;
		if (num5 > 34359738367L)
		{
			throw new InvalidOperationException("Not enough space was reserved for the length prefix of field blackjack (ProtoBuf.CardGame.Blackjack)");
		}
		Span<byte> span3 = range3.GetSpan();
		int num6 = ProtocolParser.WriteUInt32((uint)num5, span3, 0);
		if (num6 < 5)
		{
			span3[num6 - 1] |= 128;
			while (num6 < 4)
			{
				span3[num6++] = 128;
			}
			span3[4] = 0;
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
		action(UidType.NetworkableId, ref potRef.Value);
		texasHoldEm?.InspectUids(action);
		blackjack?.InspectUids(action);
	}
}
