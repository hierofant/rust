using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class GameMode : IDisposable, Pool.IPooled, IProto<GameMode>, IProto
{
	public class PlayerScore : IDisposable, Pool.IPooled, IProto<PlayerScore>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string playerName;

		[NonSerialized]
		public ulong userid;

		[NonSerialized]
		public List<int> scores;

		[NonSerialized]
		public int team;

		public static void ResetToPool(PlayerScore instance)
		{
			if (instance.ShouldPool)
			{
				instance.playerName = string.Empty;
				instance.userid = 0uL;
				if (instance.scores != null)
				{
					List<int> obj = instance.scores;
					Pool.FreeUnmanaged(ref obj);
					instance.scores = obj;
				}
				instance.team = 0;
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
				throw new Exception("Trying to dispose PlayerScore with ShouldPool set to false!");
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

		public void CopyTo(PlayerScore instance)
		{
			instance.playerName = playerName;
			instance.userid = userid;
			if (scores != null)
			{
				instance.scores = Pool.Get<List<int>>();
				for (int i = 0; i < scores.Count; i++)
				{
					int item = scores[i];
					instance.scores.Add(item);
				}
			}
			else
			{
				instance.scores = null;
			}
			instance.team = team;
		}

		public PlayerScore Copy()
		{
			PlayerScore playerScore = Pool.Get<PlayerScore>();
			CopyTo(playerScore);
			return playerScore;
		}

		public static PlayerScore Deserialize(BufferStream stream)
		{
			PlayerScore playerScore = Pool.Get<PlayerScore>();
			Deserialize(stream, playerScore, isDelta: false);
			return playerScore;
		}

		public static PlayerScore DeserializeLengthDelimited(BufferStream stream)
		{
			PlayerScore playerScore = Pool.Get<PlayerScore>();
			DeserializeLengthDelimited(stream, playerScore, isDelta: false);
			return playerScore;
		}

		public static PlayerScore DeserializeLength(BufferStream stream, int length)
		{
			PlayerScore playerScore = Pool.Get<PlayerScore>();
			DeserializeLength(stream, length, playerScore, isDelta: false);
			return playerScore;
		}

		public static PlayerScore Deserialize(byte[] buffer)
		{
			PlayerScore playerScore = Pool.Get<PlayerScore>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, playerScore, isDelta: false);
			return playerScore;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, PlayerScore previous)
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

		public static PlayerScore Deserialize(BufferStream stream, PlayerScore instance, bool isDelta)
		{
			if (!isDelta && instance.scores == null)
			{
				instance.scores = Pool.Get<List<int>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.PlayerScore");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.playerName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					stream.ConsumeRepeatedElement();
					instance.scores.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.team = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerScore DeserializeLengthDelimited(BufferStream stream, PlayerScore instance, bool isDelta)
		{
			if (!isDelta && instance.scores == null)
			{
				instance.scores = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.PlayerScore");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.playerName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					stream.ConsumeRepeatedElement();
					instance.scores.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.team = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerScore DeserializeLength(BufferStream stream, int length, PlayerScore instance, bool isDelta)
		{
			if (!isDelta && instance.scores == null)
			{
				instance.scores = Pool.Get<List<int>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.PlayerScore");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.playerName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.userid = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					stream.ConsumeRepeatedElement();
					instance.scores.Add((int)ProtocolParser.ReadUInt64(stream));
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					instance.team = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.PlayerScore");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, PlayerScore instance, PlayerScore previous)
		{
			if (instance.playerName != previous.playerName)
			{
				if (instance.playerName == null)
				{
					throw new ArgumentNullException("playerName", "Required by proto specification.");
				}
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.playerName);
			}
			if (instance.userid != previous.userid)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.scores != null)
			{
				for (int i = 0; i < instance.scores.Count; i++)
				{
					int num = instance.scores[i];
					stream.WriteByte(24);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.team != previous.team)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.team);
			}
		}

		public static void Serialize(BufferStream stream, PlayerScore instance)
		{
			if (instance.playerName == null)
			{
				throw new ArgumentNullException("playerName", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.playerName);
			if (instance.userid != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.userid);
			}
			if (instance.scores != null)
			{
				for (int i = 0; i < instance.scores.Count; i++)
				{
					int num = instance.scores[i];
					stream.WriteByte(24);
					ProtocolParser.WriteUInt64(stream, (ulong)num);
				}
			}
			if (instance.team != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.team);
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

	public class ScoreColumn : IDisposable, Pool.IPooled, IProto<ScoreColumn>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string name;

		public static void ResetToPool(ScoreColumn instance)
		{
			if (instance.ShouldPool)
			{
				instance.name = string.Empty;
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
				throw new Exception("Trying to dispose ScoreColumn with ShouldPool set to false!");
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

		public void CopyTo(ScoreColumn instance)
		{
			instance.name = name;
		}

		public ScoreColumn Copy()
		{
			ScoreColumn scoreColumn = Pool.Get<ScoreColumn>();
			CopyTo(scoreColumn);
			return scoreColumn;
		}

		public static ScoreColumn Deserialize(BufferStream stream)
		{
			ScoreColumn scoreColumn = Pool.Get<ScoreColumn>();
			Deserialize(stream, scoreColumn, isDelta: false);
			return scoreColumn;
		}

		public static ScoreColumn DeserializeLengthDelimited(BufferStream stream)
		{
			ScoreColumn scoreColumn = Pool.Get<ScoreColumn>();
			DeserializeLengthDelimited(stream, scoreColumn, isDelta: false);
			return scoreColumn;
		}

		public static ScoreColumn DeserializeLength(BufferStream stream, int length)
		{
			ScoreColumn scoreColumn = Pool.Get<ScoreColumn>();
			DeserializeLength(stream, length, scoreColumn, isDelta: false);
			return scoreColumn;
		}

		public static ScoreColumn Deserialize(byte[] buffer)
		{
			ScoreColumn scoreColumn = Pool.Get<ScoreColumn>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, scoreColumn, isDelta: false);
			return scoreColumn;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, ScoreColumn previous)
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

		public static ScoreColumn Deserialize(BufferStream stream, ScoreColumn instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.ScoreColumn");
				if (num == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.ScoreColumn");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.ScoreColumn");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.ScoreColumn");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static ScoreColumn DeserializeLengthDelimited(BufferStream stream, ScoreColumn instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.ScoreColumn");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.ScoreColumn");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.ScoreColumn");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.ScoreColumn");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static ScoreColumn DeserializeLength(BufferStream stream, int length, ScoreColumn instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.ScoreColumn");
				if (num2 == 10)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.ScoreColumn");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.ScoreColumn");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.ScoreColumn");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, ScoreColumn instance, ScoreColumn previous)
		{
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.name);
			}
		}

		public static void Serialize(BufferStream stream, ScoreColumn instance)
		{
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.name);
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
		}
	}

	public class TeamInfo : IDisposable, Pool.IPooled, IProto<TeamInfo>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int score;

		public static void ResetToPool(TeamInfo instance)
		{
			if (instance.ShouldPool)
			{
				instance.score = 0;
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
				throw new Exception("Trying to dispose TeamInfo with ShouldPool set to false!");
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

		public void CopyTo(TeamInfo instance)
		{
			instance.score = score;
		}

		public TeamInfo Copy()
		{
			TeamInfo teamInfo = Pool.Get<TeamInfo>();
			CopyTo(teamInfo);
			return teamInfo;
		}

		public static TeamInfo Deserialize(BufferStream stream)
		{
			TeamInfo teamInfo = Pool.Get<TeamInfo>();
			Deserialize(stream, teamInfo, isDelta: false);
			return teamInfo;
		}

		public static TeamInfo DeserializeLengthDelimited(BufferStream stream)
		{
			TeamInfo teamInfo = Pool.Get<TeamInfo>();
			DeserializeLengthDelimited(stream, teamInfo, isDelta: false);
			return teamInfo;
		}

		public static TeamInfo DeserializeLength(BufferStream stream, int length)
		{
			TeamInfo teamInfo = Pool.Get<TeamInfo>();
			DeserializeLength(stream, length, teamInfo, isDelta: false);
			return teamInfo;
		}

		public static TeamInfo Deserialize(byte[] buffer)
		{
			TeamInfo teamInfo = Pool.Get<TeamInfo>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, teamInfo, isDelta: false);
			return teamInfo;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, TeamInfo previous)
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

		public static TeamInfo Deserialize(BufferStream stream, TeamInfo instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.TeamInfo");
				if (num == 8)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.TeamInfo");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.TeamInfo");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.TeamInfo");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static TeamInfo DeserializeLengthDelimited(BufferStream stream, TeamInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.TeamInfo");
				if (num2 == 8)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.TeamInfo");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.TeamInfo");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.TeamInfo");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static TeamInfo DeserializeLength(BufferStream stream, int length, TeamInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.GameMode.TeamInfo");
				if (num2 == 8)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.TeamInfo");
					instance.score = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GameMode.TeamInfo");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode.TeamInfo");
					ProtocolParser.SkipKey(stream, key);
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, TeamInfo instance, TeamInfo previous)
		{
			if (instance.score != previous.score)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
			}
		}

		public static void Serialize(BufferStream stream, TeamInfo instance)
		{
			if (instance.score != 0)
			{
				stream.WriteByte(8);
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
	public List<TeamInfo> teams;

	[NonSerialized]
	public List<ScoreColumn> scoreColumns;

	[NonSerialized]
	public List<PlayerScore> playerScores;

	public static void ResetToPool(GameMode instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.teams != null)
		{
			for (int i = 0; i < instance.teams.Count; i++)
			{
				if (instance.teams[i] != null)
				{
					instance.teams[i].ResetToPool();
					instance.teams[i] = null;
				}
			}
			List<TeamInfo> obj = instance.teams;
			Pool.Free(ref obj, freeElements: false);
			instance.teams = obj;
		}
		if (instance.scoreColumns != null)
		{
			for (int j = 0; j < instance.scoreColumns.Count; j++)
			{
				if (instance.scoreColumns[j] != null)
				{
					instance.scoreColumns[j].ResetToPool();
					instance.scoreColumns[j] = null;
				}
			}
			List<ScoreColumn> obj2 = instance.scoreColumns;
			Pool.Free(ref obj2, freeElements: false);
			instance.scoreColumns = obj2;
		}
		if (instance.playerScores != null)
		{
			for (int k = 0; k < instance.playerScores.Count; k++)
			{
				if (instance.playerScores[k] != null)
				{
					instance.playerScores[k].ResetToPool();
					instance.playerScores[k] = null;
				}
			}
			List<PlayerScore> obj3 = instance.playerScores;
			Pool.Free(ref obj3, freeElements: false);
			instance.playerScores = obj3;
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
			throw new Exception("Trying to dispose GameMode with ShouldPool set to false!");
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

	public void CopyTo(GameMode instance)
	{
		if (teams != null)
		{
			instance.teams = Pool.Get<List<TeamInfo>>();
			for (int i = 0; i < teams.Count; i++)
			{
				TeamInfo item = teams[i].Copy();
				instance.teams.Add(item);
			}
		}
		else
		{
			instance.teams = null;
		}
		if (scoreColumns != null)
		{
			instance.scoreColumns = Pool.Get<List<ScoreColumn>>();
			for (int j = 0; j < scoreColumns.Count; j++)
			{
				ScoreColumn item2 = scoreColumns[j].Copy();
				instance.scoreColumns.Add(item2);
			}
		}
		else
		{
			instance.scoreColumns = null;
		}
		if (playerScores != null)
		{
			instance.playerScores = Pool.Get<List<PlayerScore>>();
			for (int k = 0; k < playerScores.Count; k++)
			{
				PlayerScore item3 = playerScores[k].Copy();
				instance.playerScores.Add(item3);
			}
		}
		else
		{
			instance.playerScores = null;
		}
	}

	public GameMode Copy()
	{
		GameMode gameMode = Pool.Get<GameMode>();
		CopyTo(gameMode);
		return gameMode;
	}

	public static GameMode Deserialize(BufferStream stream)
	{
		GameMode gameMode = Pool.Get<GameMode>();
		Deserialize(stream, gameMode, isDelta: false);
		return gameMode;
	}

	public static GameMode DeserializeLengthDelimited(BufferStream stream)
	{
		GameMode gameMode = Pool.Get<GameMode>();
		DeserializeLengthDelimited(stream, gameMode, isDelta: false);
		return gameMode;
	}

	public static GameMode DeserializeLength(BufferStream stream, int length)
	{
		GameMode gameMode = Pool.Get<GameMode>();
		DeserializeLength(stream, length, gameMode, isDelta: false);
		return gameMode;
	}

	public static GameMode Deserialize(byte[] buffer)
	{
		GameMode gameMode = Pool.Get<GameMode>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, gameMode, isDelta: false);
		return gameMode;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, GameMode previous)
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

	public static GameMode Deserialize(BufferStream stream, GameMode instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.teams == null)
			{
				instance.teams = Pool.Get<List<TeamInfo>>();
			}
			if (instance.scoreColumns == null)
			{
				instance.scoreColumns = Pool.Get<List<ScoreColumn>>();
			}
			if (instance.playerScores == null)
			{
				instance.playerScores = Pool.Get<List<PlayerScore>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.GameMode");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.teams.Add(TeamInfo.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.scoreColumns.Add(ScoreColumn.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.playerScores.Add(PlayerScore.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static GameMode DeserializeLengthDelimited(BufferStream stream, GameMode instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.teams == null)
			{
				instance.teams = Pool.Get<List<TeamInfo>>();
			}
			if (instance.scoreColumns == null)
			{
				instance.scoreColumns = Pool.Get<List<ScoreColumn>>();
			}
			if (instance.playerScores == null)
			{
				instance.playerScores = Pool.Get<List<PlayerScore>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.GameMode");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.teams.Add(TeamInfo.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.scoreColumns.Add(ScoreColumn.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.playerScores.Add(PlayerScore.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static GameMode DeserializeLength(BufferStream stream, int length, GameMode instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.teams == null)
			{
				instance.teams = Pool.Get<List<TeamInfo>>();
			}
			if (instance.scoreColumns == null)
			{
				instance.scoreColumns = Pool.Get<List<ScoreColumn>>();
			}
			if (instance.playerScores == null)
			{
				instance.playerScores = Pool.Get<List<PlayerScore>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.GameMode");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.teams.Add(TeamInfo.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.scoreColumns.Add(ScoreColumn.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				stream.ConsumeRepeatedElement();
				instance.playerScores.Add(PlayerScore.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GameMode");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, GameMode instance, GameMode previous)
	{
		if (instance.teams != null)
		{
			for (int i = 0; i < instance.teams.Count; i++)
			{
				TeamInfo teamInfo = instance.teams[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				TeamInfo.SerializeDelta(stream, teamInfo, teamInfo);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field teams (ProtoBuf.GameMode.TeamInfo)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.scoreColumns != null)
		{
			for (int j = 0; j < instance.scoreColumns.Count; j++)
			{
				ScoreColumn scoreColumn = instance.scoreColumns[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				ScoreColumn.SerializeDelta(stream, scoreColumn, scoreColumn);
				int val = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.playerScores == null)
		{
			return;
		}
		for (int k = 0; k < instance.playerScores.Count; k++)
		{
			PlayerScore playerScore = instance.playerScores[k];
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			PlayerScore.SerializeDelta(stream, playerScore, playerScore);
			int val2 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val2, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
	}

	public static void Serialize(BufferStream stream, GameMode instance)
	{
		if (instance.teams != null)
		{
			for (int i = 0; i < instance.teams.Count; i++)
			{
				TeamInfo instance2 = instance.teams[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				TeamInfo.Serialize(stream, instance2);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field teams (ProtoBuf.GameMode.TeamInfo)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
		}
		if (instance.scoreColumns != null)
		{
			for (int j = 0; j < instance.scoreColumns.Count; j++)
			{
				ScoreColumn instance3 = instance.scoreColumns[j];
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				ScoreColumn.Serialize(stream, instance3);
				int val = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val, span2, 0);
				if (num2 < 5)
				{
					span2[num2 - 1] |= 128;
					while (num2 < 4)
					{
						span2[num2++] = 128;
					}
					span2[4] = 0;
				}
			}
		}
		if (instance.playerScores == null)
		{
			return;
		}
		for (int k = 0; k < instance.playerScores.Count; k++)
		{
			PlayerScore instance4 = instance.playerScores[k];
			stream.WriteByte(26);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			PlayerScore.Serialize(stream, instance4);
			int val2 = stream.Position - position3;
			Span<byte> span3 = range3.GetSpan();
			int num3 = ProtocolParser.WriteUInt32((uint)val2, span3, 0);
			if (num3 < 5)
			{
				span3[num3 - 1] |= 128;
				while (num3 < 4)
				{
					span3[num3++] = 128;
				}
				span3[4] = 0;
			}
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (teams != null)
		{
			for (int i = 0; i < teams.Count; i++)
			{
				teams[i]?.InspectUids(action);
			}
		}
		if (scoreColumns != null)
		{
			for (int j = 0; j < scoreColumns.Count; j++)
			{
				scoreColumns[j]?.InspectUids(action);
			}
		}
		if (playerScores != null)
		{
			for (int k = 0; k < playerScores.Count; k++)
			{
				playerScores[k]?.InspectUids(action);
			}
		}
	}
}
