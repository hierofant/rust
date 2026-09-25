using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class PlayerTeam : IDisposable, Pool.IPooled, IProto<PlayerTeam>, IProto
{
	public class TeamMember : IDisposable, Pool.IPooled, IProto<TeamMember>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string displayName;

		[NonSerialized]
		public ulong userID;

		[NonSerialized]
		public float healthFraction;

		[NonSerialized]
		public Vector3 position;

		[NonSerialized]
		public bool online;

		[NonSerialized]
		public bool wounded;

		[NonSerialized]
		public ulong teamID;

		public static void ResetToPool(TeamMember instance)
		{
			if (instance.ShouldPool)
			{
				instance.displayName = string.Empty;
				instance.userID = 0uL;
				instance.healthFraction = 0f;
				instance.position = default(Vector3);
				instance.online = false;
				instance.wounded = false;
				instance.teamID = 0uL;
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
				throw new Exception("Trying to dispose TeamMember with ShouldPool set to false!");
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

		public void CopyTo(TeamMember instance)
		{
			instance.displayName = displayName;
			instance.userID = userID;
			instance.healthFraction = healthFraction;
			instance.position = position;
			instance.online = online;
			instance.wounded = wounded;
			instance.teamID = teamID;
		}

		public TeamMember Copy()
		{
			TeamMember teamMember = Pool.Get<TeamMember>();
			CopyTo(teamMember);
			return teamMember;
		}

		public static TeamMember Deserialize(BufferStream stream)
		{
			TeamMember teamMember = Pool.Get<TeamMember>();
			Deserialize(stream, teamMember, isDelta: false);
			return teamMember;
		}

		public static TeamMember DeserializeLengthDelimited(BufferStream stream)
		{
			TeamMember teamMember = Pool.Get<TeamMember>();
			DeserializeLengthDelimited(stream, teamMember, isDelta: false);
			return teamMember;
		}

		public static TeamMember DeserializeLength(BufferStream stream, int length)
		{
			TeamMember teamMember = Pool.Get<TeamMember>();
			DeserializeLength(stream, length, teamMember, isDelta: false);
			return teamMember;
		}

		public static TeamMember Deserialize(byte[] buffer)
		{
			TeamMember teamMember = Pool.Get<TeamMember>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, teamMember, isDelta: false);
			return teamMember;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, TeamMember previous)
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

		public static TeamMember Deserialize(BufferStream stream, TeamMember instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PlayerTeam.TeamMember");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.userID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.healthFraction = ProtocolParser.ReadSingle(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.online = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.wounded = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.teamID = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static TeamMember DeserializeLengthDelimited(BufferStream stream, TeamMember instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerTeam.TeamMember");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.userID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.healthFraction = ProtocolParser.ReadSingle(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.online = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.wounded = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.teamID = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static TeamMember DeserializeLength(BufferStream stream, int length, TeamMember instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerTeam.TeamMember");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.userID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.healthFraction = ProtocolParser.ReadSingle(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.online = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.wounded = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					instance.teamID = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerTeam.TeamMember");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, TeamMember instance, TeamMember previous)
		{
			if (instance.displayName != null && instance.displayName != previous.displayName)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.userID != previous.userID)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.userID);
			}
			if (instance.healthFraction != previous.healthFraction)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.healthFraction);
			}
			if (instance.position != previous.position)
			{
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int num = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.position, previous.position);
				int num2 = stream.Position - num;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span, 0);
			}
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.online);
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.wounded);
			if (instance.teamID != previous.teamID)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, instance.teamID);
			}
		}

		public static void Serialize(BufferStream stream, TeamMember instance)
		{
			if (instance.displayName != null)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.userID != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.userID);
			}
			if (instance.healthFraction != 0f)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.healthFraction);
			}
			if (instance.position != default(Vector3))
			{
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int num = stream.Position;
				Vector3Serialized.Serialize(stream, instance.position);
				int num2 = stream.Position - num;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field position (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span, 0);
			}
			if (instance.online)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteBool(stream, instance.online);
			}
			if (instance.wounded)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteBool(stream, instance.wounded);
			}
			if (instance.teamID != 0L)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteUInt64(stream, instance.teamID);
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
	public ulong teamID;

	[NonSerialized]
	public string teamName;

	[NonSerialized]
	public ulong teamLeader;

	[NonSerialized]
	public List<TeamMember> members;

	[NonSerialized]
	public float teamLifetime;

	[NonSerialized]
	public List<MapNote> leaderMapNotes;

	[NonSerialized]
	public List<MapNote> teamPings;

	[NonSerialized]
	public List<ulong> invites;

	[NonSerialized]
	public string joinKey;

	public static void ResetToPool(PlayerTeam instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.teamID = 0uL;
		instance.teamName = string.Empty;
		instance.teamLeader = 0uL;
		if (instance.members != null)
		{
			for (int i = 0; i < instance.members.Count; i++)
			{
				if (instance.members[i] != null)
				{
					instance.members[i].ResetToPool();
					instance.members[i] = null;
				}
			}
			List<TeamMember> obj = instance.members;
			Pool.Free(ref obj, freeElements: false);
			instance.members = obj;
		}
		instance.teamLifetime = 0f;
		if (instance.leaderMapNotes != null)
		{
			for (int j = 0; j < instance.leaderMapNotes.Count; j++)
			{
				if (instance.leaderMapNotes[j] != null)
				{
					instance.leaderMapNotes[j].ResetToPool();
					instance.leaderMapNotes[j] = null;
				}
			}
			List<MapNote> obj2 = instance.leaderMapNotes;
			Pool.Free(ref obj2, freeElements: false);
			instance.leaderMapNotes = obj2;
		}
		if (instance.teamPings != null)
		{
			for (int k = 0; k < instance.teamPings.Count; k++)
			{
				if (instance.teamPings[k] != null)
				{
					instance.teamPings[k].ResetToPool();
					instance.teamPings[k] = null;
				}
			}
			List<MapNote> obj3 = instance.teamPings;
			Pool.Free(ref obj3, freeElements: false);
			instance.teamPings = obj3;
		}
		if (instance.invites != null)
		{
			List<ulong> obj4 = instance.invites;
			Pool.FreeUnmanaged(ref obj4);
			instance.invites = obj4;
		}
		instance.joinKey = string.Empty;
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
			throw new Exception("Trying to dispose PlayerTeam with ShouldPool set to false!");
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

	public void CopyTo(PlayerTeam instance)
	{
		instance.teamID = teamID;
		instance.teamName = teamName;
		instance.teamLeader = teamLeader;
		if (members != null)
		{
			instance.members = Pool.Get<List<TeamMember>>();
			for (int i = 0; i < members.Count; i++)
			{
				TeamMember item = members[i].Copy();
				instance.members.Add(item);
			}
		}
		else
		{
			instance.members = null;
		}
		instance.teamLifetime = teamLifetime;
		if (leaderMapNotes != null)
		{
			instance.leaderMapNotes = Pool.Get<List<MapNote>>();
			for (int j = 0; j < leaderMapNotes.Count; j++)
			{
				MapNote item2 = leaderMapNotes[j].Copy();
				instance.leaderMapNotes.Add(item2);
			}
		}
		else
		{
			instance.leaderMapNotes = null;
		}
		if (teamPings != null)
		{
			instance.teamPings = Pool.Get<List<MapNote>>();
			for (int k = 0; k < teamPings.Count; k++)
			{
				MapNote item3 = teamPings[k].Copy();
				instance.teamPings.Add(item3);
			}
		}
		else
		{
			instance.teamPings = null;
		}
		if (invites != null)
		{
			instance.invites = Pool.Get<List<ulong>>();
			for (int l = 0; l < invites.Count; l++)
			{
				ulong item4 = invites[l];
				instance.invites.Add(item4);
			}
		}
		else
		{
			instance.invites = null;
		}
		instance.joinKey = joinKey;
	}

	public PlayerTeam Copy()
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		CopyTo(playerTeam);
		return playerTeam;
	}

	public static PlayerTeam Deserialize(BufferStream stream)
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		Deserialize(stream, playerTeam, isDelta: false);
		return playerTeam;
	}

	public static PlayerTeam DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		DeserializeLengthDelimited(stream, playerTeam, isDelta: false);
		return playerTeam;
	}

	public static PlayerTeam DeserializeLength(BufferStream stream, int length)
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		DeserializeLength(stream, length, playerTeam, isDelta: false);
		return playerTeam;
	}

	public static PlayerTeam Deserialize(byte[] buffer)
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerTeam, isDelta: false);
		return playerTeam;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerTeam previous)
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

	public static PlayerTeam Deserialize(BufferStream stream, PlayerTeam instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<TeamMember>>();
			}
			if (instance.leaderMapNotes == null)
			{
				instance.leaderMapNotes = Pool.Get<List<MapNote>>();
			}
			if (instance.teamPings == null)
			{
				instance.teamPings = Pool.Get<List<MapNote>>();
			}
			if (instance.invites == null)
			{
				instance.invites = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerTeam");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamLeader = ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.members.Add(TeamMember.DeserializeLengthDelimited(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamLifetime = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.leaderMapNotes.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.teamPings.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.invites.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.joinKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerTeam DeserializeLengthDelimited(BufferStream stream, PlayerTeam instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<TeamMember>>();
			}
			if (instance.leaderMapNotes == null)
			{
				instance.leaderMapNotes = Pool.Get<List<MapNote>>();
			}
			if (instance.teamPings == null)
			{
				instance.teamPings = Pool.Get<List<MapNote>>();
			}
			if (instance.invites == null)
			{
				instance.invites = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerTeam");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamLeader = ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.members.Add(TeamMember.DeserializeLengthDelimited(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamLifetime = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.leaderMapNotes.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.teamPings.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.invites.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.joinKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerTeam DeserializeLength(BufferStream stream, int length, PlayerTeam instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<TeamMember>>();
			}
			if (instance.leaderMapNotes == null)
			{
				instance.leaderMapNotes = Pool.Get<List<MapNote>>();
			}
			if (instance.teamPings == null)
			{
				instance.teamPings = Pool.Get<List<MapNote>>();
			}
			if (instance.invites == null)
			{
				instance.invites = Pool.Get<List<ulong>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerTeam");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamID = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamName = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamLeader = ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.members.Add(TeamMember.DeserializeLengthDelimited(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.teamLifetime = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.leaderMapNotes.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.teamPings.Add(MapNote.DeserializeLengthDelimited(stream));
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				stream.ConsumeRepeatedElement();
				instance.invites.Add(ProtocolParser.ReadUInt64(stream));
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				instance.joinKey = ProtocolParser.ReadString(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerTeam");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerTeam instance, PlayerTeam previous)
	{
		if (instance.teamID != previous.teamID)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.teamID);
		}
		if (instance.teamName != null && instance.teamName != previous.teamName)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.teamName);
		}
		if (instance.teamLeader != previous.teamLeader)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.teamLeader);
		}
		if (instance.members != null)
		{
			for (int i = 0; i < instance.members.Count; i++)
			{
				TeamMember teamMember = instance.members[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				TeamMember.SerializeDelta(stream, teamMember, teamMember);
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
		if (instance.teamLifetime != previous.teamLifetime)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.teamLifetime);
		}
		if (instance.leaderMapNotes != null)
		{
			for (int j = 0; j < instance.leaderMapNotes.Count; j++)
			{
				MapNote mapNote = instance.leaderMapNotes[j];
				stream.WriteByte(58);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				MapNote.SerializeDelta(stream, mapNote, mapNote);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
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
		if (instance.teamPings != null)
		{
			for (int k = 0; k < instance.teamPings.Count; k++)
			{
				MapNote mapNote2 = instance.teamPings[k];
				stream.WriteByte(66);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				MapNote.SerializeDelta(stream, mapNote2, mapNote2);
				int val3 = stream.Position - position3;
				Span<byte> span3 = range3.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
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
		if (instance.invites != null)
		{
			for (int l = 0; l < instance.invites.Count; l++)
			{
				ulong val4 = instance.invites[l];
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, val4);
			}
		}
		if (instance.joinKey != null && instance.joinKey != previous.joinKey)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteString(stream, instance.joinKey);
		}
	}

	public static void Serialize(BufferStream stream, PlayerTeam instance)
	{
		if (instance.teamID != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.teamID);
		}
		if (instance.teamName != null)
		{
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.teamName);
		}
		if (instance.teamLeader != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, instance.teamLeader);
		}
		if (instance.members != null)
		{
			for (int i = 0; i < instance.members.Count; i++)
			{
				TeamMember instance2 = instance.members[i];
				stream.WriteByte(34);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				TeamMember.Serialize(stream, instance2);
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
		if (instance.teamLifetime != 0f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.teamLifetime);
		}
		if (instance.leaderMapNotes != null)
		{
			for (int j = 0; j < instance.leaderMapNotes.Count; j++)
			{
				MapNote instance3 = instance.leaderMapNotes[j];
				stream.WriteByte(58);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				MapNote.Serialize(stream, instance3);
				int val2 = stream.Position - position2;
				Span<byte> span2 = range2.GetSpan();
				int num2 = ProtocolParser.WriteUInt32((uint)val2, span2, 0);
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
		if (instance.teamPings != null)
		{
			for (int k = 0; k < instance.teamPings.Count; k++)
			{
				MapNote instance4 = instance.teamPings[k];
				stream.WriteByte(66);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				MapNote.Serialize(stream, instance4);
				int val3 = stream.Position - position3;
				Span<byte> span3 = range3.GetSpan();
				int num3 = ProtocolParser.WriteUInt32((uint)val3, span3, 0);
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
		if (instance.invites != null)
		{
			for (int l = 0; l < instance.invites.Count; l++)
			{
				ulong val4 = instance.invites[l];
				stream.WriteByte(72);
				ProtocolParser.WriteUInt64(stream, val4);
			}
		}
		if (instance.joinKey != null)
		{
			stream.WriteByte(82);
			ProtocolParser.WriteString(stream, instance.joinKey);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (members != null)
		{
			for (int i = 0; i < members.Count; i++)
			{
				members[i]?.InspectUids(action);
			}
		}
		if (leaderMapNotes != null)
		{
			for (int j = 0; j < leaderMapNotes.Count; j++)
			{
				leaderMapNotes[j]?.InspectUids(action);
			}
		}
		if (teamPings != null)
		{
			for (int k = 0; k < teamPings.Count; k++)
			{
				teamPings[k]?.InspectUids(action);
			}
		}
	}
}
