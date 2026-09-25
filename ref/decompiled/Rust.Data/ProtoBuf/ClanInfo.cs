using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class ClanInfo : IDisposable, Pool.IPooled, IProto<ClanInfo>, IProto
{
	public class Invite : IDisposable, Pool.IPooled, IProto<Invite>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong steamId;

		[NonSerialized]
		public ulong recruiter;

		[NonSerialized]
		public long timestamp;

		public static void ResetToPool(Invite instance)
		{
			if (instance.ShouldPool)
			{
				instance.steamId = 0uL;
				instance.recruiter = 0uL;
				instance.timestamp = 0L;
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
				throw new Exception("Trying to dispose Invite with ShouldPool set to false!");
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

		public void CopyTo(Invite instance)
		{
			instance.steamId = steamId;
			instance.recruiter = recruiter;
			instance.timestamp = timestamp;
		}

		public Invite Copy()
		{
			Invite invite = Pool.Get<Invite>();
			CopyTo(invite);
			return invite;
		}

		public static Invite Deserialize(BufferStream stream)
		{
			Invite invite = Pool.Get<Invite>();
			Deserialize(stream, invite, isDelta: false);
			return invite;
		}

		public static Invite DeserializeLengthDelimited(BufferStream stream)
		{
			Invite invite = Pool.Get<Invite>();
			DeserializeLengthDelimited(stream, invite, isDelta: false);
			return invite;
		}

		public static Invite DeserializeLength(BufferStream stream, int length)
		{
			Invite invite = Pool.Get<Invite>();
			DeserializeLength(stream, length, invite, isDelta: false);
			return invite;
		}

		public static Invite Deserialize(byte[] buffer)
		{
			Invite invite = Pool.Get<Invite>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, invite, isDelta: false);
			return invite;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Invite previous)
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

		public static Invite Deserialize(BufferStream stream, Invite instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.steamId = 0uL;
				instance.recruiter = 0uL;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Invite");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.recruiter = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Invite DeserializeLengthDelimited(BufferStream stream, Invite instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.steamId = 0uL;
				instance.recruiter = 0uL;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Invite");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.recruiter = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Invite DeserializeLength(BufferStream stream, int length, Invite instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.steamId = 0uL;
				instance.recruiter = 0uL;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Invite");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.recruiter = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					instance.timestamp = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Invite");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Invite instance, Invite previous)
		{
			if (instance.steamId != previous.steamId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.recruiter != previous.recruiter)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.recruiter);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
		}

		public static void Serialize(BufferStream stream, Invite instance)
		{
			if (instance.steamId != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.recruiter != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.recruiter);
			}
			if (instance.timestamp != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.timestamp);
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

	public class Member : IDisposable, Pool.IPooled, IProto<Member>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong steamId;

		[NonSerialized]
		public int roleId;

		[NonSerialized]
		public long joined;

		[NonSerialized]
		public long lastSeen;

		[NonSerialized]
		public string notes;

		[NonSerialized]
		public bool online;

		public static void ResetToPool(Member instance)
		{
			if (instance.ShouldPool)
			{
				instance.steamId = 0uL;
				instance.roleId = 0;
				instance.joined = 0L;
				instance.lastSeen = 0L;
				instance.notes = string.Empty;
				instance.online = false;
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
				throw new Exception("Trying to dispose Member with ShouldPool set to false!");
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

		public void CopyTo(Member instance)
		{
			instance.steamId = steamId;
			instance.roleId = roleId;
			instance.joined = joined;
			instance.lastSeen = lastSeen;
			instance.notes = notes;
			instance.online = online;
		}

		public Member Copy()
		{
			Member member = Pool.Get<Member>();
			CopyTo(member);
			return member;
		}

		public static Member Deserialize(BufferStream stream)
		{
			Member member = Pool.Get<Member>();
			Deserialize(stream, member, isDelta: false);
			return member;
		}

		public static Member DeserializeLengthDelimited(BufferStream stream)
		{
			Member member = Pool.Get<Member>();
			DeserializeLengthDelimited(stream, member, isDelta: false);
			return member;
		}

		public static Member DeserializeLength(BufferStream stream, int length)
		{
			Member member = Pool.Get<Member>();
			DeserializeLength(stream, length, member, isDelta: false);
			return member;
		}

		public static Member Deserialize(byte[] buffer)
		{
			Member member = Pool.Get<Member>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, member, isDelta: false);
			return member;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Member previous)
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

		public static Member Deserialize(BufferStream stream, Member instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.steamId = 0uL;
				instance.roleId = 0;
				instance.joined = 0L;
				instance.lastSeen = 0L;
				instance.online = false;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Member");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.roleId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.joined = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.lastSeen = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.notes = ProtocolParser.ReadString(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.online = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Member DeserializeLengthDelimited(BufferStream stream, Member instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.steamId = 0uL;
				instance.roleId = 0;
				instance.joined = 0L;
				instance.lastSeen = 0L;
				instance.online = false;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Member");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.roleId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.joined = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.lastSeen = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.notes = ProtocolParser.ReadString(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.online = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Member DeserializeLength(BufferStream stream, int length, Member instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.steamId = 0uL;
				instance.roleId = 0;
				instance.joined = 0L;
				instance.lastSeen = 0L;
				instance.online = false;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Member");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.roleId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.joined = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.lastSeen = (long)ProtocolParser.ReadUInt64(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.notes = ProtocolParser.ReadString(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					instance.online = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Member instance, Member previous)
		{
			if (instance.steamId != previous.steamId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.roleId != previous.roleId)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.roleId);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.joined);
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.lastSeen);
			if (instance.notes != null && instance.notes != previous.notes)
			{
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, instance.notes);
			}
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.online);
		}

		public static void Serialize(BufferStream stream, Member instance)
		{
			if (instance.steamId != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.roleId != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.roleId);
			}
			if (instance.joined != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.joined);
			}
			if (instance.lastSeen != 0L)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.lastSeen);
			}
			if (instance.notes != null)
			{
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, instance.notes);
			}
			if (instance.online)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteBool(stream, instance.online);
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

	public class Role : IDisposable, Pool.IPooled, IProto<Role>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int roleId;

		[NonSerialized]
		public int rank;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public bool canSetMotd;

		[NonSerialized]
		public bool canSetLogo;

		[NonSerialized]
		public bool canInvite;

		[NonSerialized]
		public bool canKick;

		[NonSerialized]
		public bool canPromote;

		[NonSerialized]
		public bool canDemote;

		[NonSerialized]
		public bool canSetPlayerNotes;

		[NonSerialized]
		public bool canAccessLogs;

		[NonSerialized]
		public bool canAccessScoreEvents;

		public static void ResetToPool(Role instance)
		{
			if (instance.ShouldPool)
			{
				instance.roleId = 0;
				instance.rank = 0;
				instance.name = string.Empty;
				instance.canSetMotd = false;
				instance.canSetLogo = false;
				instance.canInvite = false;
				instance.canKick = false;
				instance.canPromote = false;
				instance.canDemote = false;
				instance.canSetPlayerNotes = false;
				instance.canAccessLogs = false;
				instance.canAccessScoreEvents = false;
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
				throw new Exception("Trying to dispose Role with ShouldPool set to false!");
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

		public void CopyTo(Role instance)
		{
			instance.roleId = roleId;
			instance.rank = rank;
			instance.name = name;
			instance.canSetMotd = canSetMotd;
			instance.canSetLogo = canSetLogo;
			instance.canInvite = canInvite;
			instance.canKick = canKick;
			instance.canPromote = canPromote;
			instance.canDemote = canDemote;
			instance.canSetPlayerNotes = canSetPlayerNotes;
			instance.canAccessLogs = canAccessLogs;
			instance.canAccessScoreEvents = canAccessScoreEvents;
		}

		public Role Copy()
		{
			Role role = Pool.Get<Role>();
			CopyTo(role);
			return role;
		}

		public static Role Deserialize(BufferStream stream)
		{
			Role role = Pool.Get<Role>();
			Deserialize(stream, role, isDelta: false);
			return role;
		}

		public static Role DeserializeLengthDelimited(BufferStream stream)
		{
			Role role = Pool.Get<Role>();
			DeserializeLengthDelimited(stream, role, isDelta: false);
			return role;
		}

		public static Role DeserializeLength(BufferStream stream, int length)
		{
			Role role = Pool.Get<Role>();
			DeserializeLength(stream, length, role, isDelta: false);
			return role;
		}

		public static Role Deserialize(byte[] buffer)
		{
			Role role = Pool.Get<Role>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, role, isDelta: false);
			return role;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Role previous)
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

		public static Role Deserialize(BufferStream stream, Role instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.roleId = 0;
				instance.rank = 0;
				instance.canSetMotd = false;
				instance.canSetLogo = false;
				instance.canInvite = false;
				instance.canKick = false;
				instance.canPromote = false;
				instance.canDemote = false;
				instance.canSetPlayerNotes = false;
				instance.canAccessLogs = false;
				instance.canAccessScoreEvents = false;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Role");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.roleId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.rank = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetMotd = ProtocolParser.ReadBool(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetLogo = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canInvite = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canKick = ProtocolParser.ReadBool(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canPromote = ProtocolParser.ReadBool(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canDemote = ProtocolParser.ReadBool(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetPlayerNotes = ProtocolParser.ReadBool(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canAccessLogs = ProtocolParser.ReadBool(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canAccessScoreEvents = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Role DeserializeLengthDelimited(BufferStream stream, Role instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.roleId = 0;
				instance.rank = 0;
				instance.canSetMotd = false;
				instance.canSetLogo = false;
				instance.canInvite = false;
				instance.canKick = false;
				instance.canPromote = false;
				instance.canDemote = false;
				instance.canSetPlayerNotes = false;
				instance.canAccessLogs = false;
				instance.canAccessScoreEvents = false;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Role");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.roleId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.rank = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetMotd = ProtocolParser.ReadBool(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetLogo = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canInvite = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canKick = ProtocolParser.ReadBool(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canPromote = ProtocolParser.ReadBool(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canDemote = ProtocolParser.ReadBool(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetPlayerNotes = ProtocolParser.ReadBool(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canAccessLogs = ProtocolParser.ReadBool(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canAccessScoreEvents = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Role DeserializeLength(BufferStream stream, int length, Role instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.roleId = 0;
				instance.rank = 0;
				instance.canSetMotd = false;
				instance.canSetLogo = false;
				instance.canInvite = false;
				instance.canKick = false;
				instance.canPromote = false;
				instance.canDemote = false;
				instance.canSetPlayerNotes = false;
				instance.canAccessLogs = false;
				instance.canAccessScoreEvents = false;
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
				stream.ConsumeFieldOperation("ProtoBuf.ClanInfo.Role");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.roleId = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.rank = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetMotd = ProtocolParser.ReadBool(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetLogo = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canInvite = ProtocolParser.ReadBool(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canKick = ProtocolParser.ReadBool(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canPromote = ProtocolParser.ReadBool(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canDemote = ProtocolParser.ReadBool(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canSetPlayerNotes = ProtocolParser.ReadBool(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canAccessLogs = ProtocolParser.ReadBool(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					instance.canAccessScoreEvents = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo.Role");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Role instance, Role previous)
		{
			if (instance.roleId != previous.roleId)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.roleId);
			}
			if (instance.rank != previous.rank)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.rank);
			}
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.name);
			}
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.canSetMotd);
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.canSetLogo);
			stream.WriteByte(48);
			ProtocolParser.WriteBool(stream, instance.canInvite);
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.canKick);
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.canPromote);
			stream.WriteByte(72);
			ProtocolParser.WriteBool(stream, instance.canDemote);
			stream.WriteByte(80);
			ProtocolParser.WriteBool(stream, instance.canSetPlayerNotes);
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.canAccessLogs);
			stream.WriteByte(96);
			ProtocolParser.WriteBool(stream, instance.canAccessScoreEvents);
		}

		public static void Serialize(BufferStream stream, Role instance)
		{
			if (instance.roleId != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.roleId);
			}
			if (instance.rank != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.rank);
			}
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.canSetMotd)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteBool(stream, instance.canSetMotd);
			}
			if (instance.canSetLogo)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteBool(stream, instance.canSetLogo);
			}
			if (instance.canInvite)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteBool(stream, instance.canInvite);
			}
			if (instance.canKick)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteBool(stream, instance.canKick);
			}
			if (instance.canPromote)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteBool(stream, instance.canPromote);
			}
			if (instance.canDemote)
			{
				stream.WriteByte(72);
				ProtocolParser.WriteBool(stream, instance.canDemote);
			}
			if (instance.canSetPlayerNotes)
			{
				stream.WriteByte(80);
				ProtocolParser.WriteBool(stream, instance.canSetPlayerNotes);
			}
			if (instance.canAccessLogs)
			{
				stream.WriteByte(88);
				ProtocolParser.WriteBool(stream, instance.canAccessLogs);
			}
			if (instance.canAccessScoreEvents)
			{
				stream.WriteByte(96);
				ProtocolParser.WriteBool(stream, instance.canAccessScoreEvents);
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
	public string name;

	[NonSerialized]
	public long created;

	[NonSerialized]
	public ulong creator;

	[NonSerialized]
	public string motd;

	[NonSerialized]
	public long motdTimestamp;

	[NonSerialized]
	public ulong motdAuthor;

	[NonSerialized]
	public byte[] logo;

	[NonSerialized]
	public int color;

	[NonSerialized]
	public List<Role> roles;

	[NonSerialized]
	public List<Member> members;

	[NonSerialized]
	public List<Invite> invites;

	[NonSerialized]
	public int maxMemberCount;

	[NonSerialized]
	public long score;

	public static void ResetToPool(ClanInfo instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.clanId = 0L;
		instance.name = string.Empty;
		instance.created = 0L;
		instance.creator = 0uL;
		instance.motd = string.Empty;
		instance.motdTimestamp = 0L;
		instance.motdAuthor = 0uL;
		instance.logo = null;
		instance.color = 0;
		if (instance.roles != null)
		{
			for (int i = 0; i < instance.roles.Count; i++)
			{
				if (instance.roles[i] != null)
				{
					instance.roles[i].ResetToPool();
					instance.roles[i] = null;
				}
			}
			List<Role> obj = instance.roles;
			Pool.Free(ref obj, freeElements: false);
			instance.roles = obj;
		}
		if (instance.members != null)
		{
			for (int j = 0; j < instance.members.Count; j++)
			{
				if (instance.members[j] != null)
				{
					instance.members[j].ResetToPool();
					instance.members[j] = null;
				}
			}
			List<Member> obj2 = instance.members;
			Pool.Free(ref obj2, freeElements: false);
			instance.members = obj2;
		}
		if (instance.invites != null)
		{
			for (int k = 0; k < instance.invites.Count; k++)
			{
				if (instance.invites[k] != null)
				{
					instance.invites[k].ResetToPool();
					instance.invites[k] = null;
				}
			}
			List<Invite> obj3 = instance.invites;
			Pool.Free(ref obj3, freeElements: false);
			instance.invites = obj3;
		}
		instance.maxMemberCount = 0;
		instance.score = 0L;
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
			throw new Exception("Trying to dispose ClanInfo with ShouldPool set to false!");
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

	public void CopyTo(ClanInfo instance)
	{
		instance.clanId = clanId;
		instance.name = name;
		instance.created = created;
		instance.creator = creator;
		instance.motd = motd;
		instance.motdTimestamp = motdTimestamp;
		instance.motdAuthor = motdAuthor;
		if (logo == null)
		{
			instance.logo = null;
		}
		else
		{
			instance.logo = new byte[logo.Length];
			Array.Copy(logo, instance.logo, instance.logo.Length);
		}
		instance.color = color;
		if (roles != null)
		{
			instance.roles = Pool.Get<List<Role>>();
			for (int i = 0; i < roles.Count; i++)
			{
				Role item = roles[i].Copy();
				instance.roles.Add(item);
			}
		}
		else
		{
			instance.roles = null;
		}
		if (members != null)
		{
			instance.members = Pool.Get<List<Member>>();
			for (int j = 0; j < members.Count; j++)
			{
				Member item2 = members[j].Copy();
				instance.members.Add(item2);
			}
		}
		else
		{
			instance.members = null;
		}
		if (invites != null)
		{
			instance.invites = Pool.Get<List<Invite>>();
			for (int k = 0; k < invites.Count; k++)
			{
				Invite item3 = invites[k].Copy();
				instance.invites.Add(item3);
			}
		}
		else
		{
			instance.invites = null;
		}
		instance.maxMemberCount = maxMemberCount;
		instance.score = score;
	}

	public ClanInfo Copy()
	{
		ClanInfo clanInfo = Pool.Get<ClanInfo>();
		CopyTo(clanInfo);
		return clanInfo;
	}

	public static ClanInfo Deserialize(BufferStream stream)
	{
		ClanInfo clanInfo = Pool.Get<ClanInfo>();
		Deserialize(stream, clanInfo, isDelta: false);
		return clanInfo;
	}

	public static ClanInfo DeserializeLengthDelimited(BufferStream stream)
	{
		ClanInfo clanInfo = Pool.Get<ClanInfo>();
		DeserializeLengthDelimited(stream, clanInfo, isDelta: false);
		return clanInfo;
	}

	public static ClanInfo DeserializeLength(BufferStream stream, int length)
	{
		ClanInfo clanInfo = Pool.Get<ClanInfo>();
		DeserializeLength(stream, length, clanInfo, isDelta: false);
		return clanInfo;
	}

	public static ClanInfo Deserialize(byte[] buffer)
	{
		ClanInfo clanInfo = Pool.Get<ClanInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, clanInfo, isDelta: false);
		return clanInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ClanInfo previous)
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

	public static ClanInfo Deserialize(BufferStream stream, ClanInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			instance.created = 0L;
			instance.creator = 0uL;
			instance.motdTimestamp = 0L;
			instance.motdAuthor = 0uL;
			instance.color = 0;
			if (instance.roles == null)
			{
				instance.roles = Pool.Get<List<Role>>();
			}
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<Member>>();
			}
			if (instance.invites == null)
			{
				instance.invites = Pool.Get<List<Invite>>();
			}
			instance.maxMemberCount = 0;
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanInfo");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.created = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.creator = ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motd = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motdTimestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motdAuthor = ProtocolParser.ReadUInt64(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.logo = ProtocolParser.ReadBytes(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.color = ProtocolParser.ReadZInt32(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.roles.Add(Role.DeserializeLengthDelimited(stream));
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.members.Add(Member.DeserializeLengthDelimited(stream));
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.invites.Add(Invite.DeserializeLengthDelimited(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.maxMemberCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.score = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanInfo DeserializeLengthDelimited(BufferStream stream, ClanInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			instance.created = 0L;
			instance.creator = 0uL;
			instance.motdTimestamp = 0L;
			instance.motdAuthor = 0uL;
			instance.color = 0;
			if (instance.roles == null)
			{
				instance.roles = Pool.Get<List<Role>>();
			}
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<Member>>();
			}
			if (instance.invites == null)
			{
				instance.invites = Pool.Get<List<Invite>>();
			}
			instance.maxMemberCount = 0;
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.created = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.creator = ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motd = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motdTimestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motdAuthor = ProtocolParser.ReadUInt64(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.logo = ProtocolParser.ReadBytes(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.color = ProtocolParser.ReadZInt32(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.roles.Add(Role.DeserializeLengthDelimited(stream));
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.members.Add(Member.DeserializeLengthDelimited(stream));
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.invites.Add(Invite.DeserializeLengthDelimited(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.maxMemberCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.score = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ClanInfo DeserializeLength(BufferStream stream, int length, ClanInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.clanId = 0L;
			instance.created = 0L;
			instance.creator = 0uL;
			instance.motdTimestamp = 0L;
			instance.motdAuthor = 0uL;
			instance.color = 0;
			if (instance.roles == null)
			{
				instance.roles = Pool.Get<List<Role>>();
			}
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<Member>>();
			}
			if (instance.invites == null)
			{
				instance.invites = Pool.Get<List<Invite>>();
			}
			instance.maxMemberCount = 0;
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
			stream.ConsumeFieldOperation("ProtoBuf.ClanInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.clanId = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.name = ProtocolParser.ReadString(stream);
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.created = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.creator = ProtocolParser.ReadUInt64(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motd = ProtocolParser.ReadString(stream);
				continue;
			case 48:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motdTimestamp = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			case 56:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.motdAuthor = ProtocolParser.ReadUInt64(stream);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.logo = ProtocolParser.ReadBytes(stream);
				continue;
			case 72:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.color = ProtocolParser.ReadZInt32(stream);
				continue;
			case 82:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.roles.Add(Role.DeserializeLengthDelimited(stream));
				continue;
			case 90:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.members.Add(Member.DeserializeLengthDelimited(stream));
				continue;
			case 98:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				stream.ConsumeRepeatedElement();
				instance.invites.Add(Invite.DeserializeLengthDelimited(stream));
				continue;
			case 104:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.maxMemberCount = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 112:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				instance.score = (long)ProtocolParser.ReadUInt64(stream);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 10u:
				stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 11u:
				stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 12u:
				stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 13u:
				stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 14u:
				stream.ValidateFieldOrder(ref lastFieldId, 14u, fieldIsRepeated: false, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ClanInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ClanInfo instance, ClanInfo previous)
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
		ProtocolParser.WriteUInt64(stream, (ulong)instance.created);
		if (instance.creator != previous.creator)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.creator);
		}
		if (instance.motd != null && instance.motd != previous.motd)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.motd);
		}
		stream.WriteByte(48);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.motdTimestamp);
		if (instance.motdAuthor != previous.motdAuthor)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.motdAuthor);
		}
		if (instance.logo != null)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteBytes(stream, instance.logo);
		}
		stream.WriteByte(72);
		ProtocolParser.WriteZInt32(stream, instance.color);
		if (instance.roles != null)
		{
			for (int i = 0; i < instance.roles.Count; i++)
			{
				Role role = instance.roles[i];
				stream.WriteByte(82);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Role.SerializeDelta(stream, role, role);
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
		if (instance.members != null)
		{
			for (int j = 0; j < instance.members.Count; j++)
			{
				Member member = instance.members[j];
				stream.WriteByte(90);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				Member.SerializeDelta(stream, member, member);
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
		if (instance.invites != null)
		{
			for (int k = 0; k < instance.invites.Count; k++)
			{
				Invite invite = instance.invites[k];
				stream.WriteByte(98);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int position3 = stream.Position;
				Invite.SerializeDelta(stream, invite, invite);
				int num3 = stream.Position - position3;
				if (num3 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field invites (ProtoBuf.ClanInfo.Invite)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			}
		}
		if (instance.maxMemberCount != previous.maxMemberCount)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.maxMemberCount);
		}
		stream.WriteByte(112);
		ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
	}

	public static void Serialize(BufferStream stream, ClanInfo instance)
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
		if (instance.created != 0L)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.created);
		}
		if (instance.creator != 0L)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteUInt64(stream, instance.creator);
		}
		if (instance.motd != null)
		{
			stream.WriteByte(42);
			ProtocolParser.WriteString(stream, instance.motd);
		}
		if (instance.motdTimestamp != 0L)
		{
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.motdTimestamp);
		}
		if (instance.motdAuthor != 0L)
		{
			stream.WriteByte(56);
			ProtocolParser.WriteUInt64(stream, instance.motdAuthor);
		}
		if (instance.logo != null)
		{
			stream.WriteByte(66);
			ProtocolParser.WriteBytes(stream, instance.logo);
		}
		if (instance.color != 0)
		{
			stream.WriteByte(72);
			ProtocolParser.WriteZInt32(stream, instance.color);
		}
		if (instance.roles != null)
		{
			for (int i = 0; i < instance.roles.Count; i++)
			{
				Role instance2 = instance.roles[i];
				stream.WriteByte(82);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Role.Serialize(stream, instance2);
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
		if (instance.members != null)
		{
			for (int j = 0; j < instance.members.Count; j++)
			{
				Member instance3 = instance.members[j];
				stream.WriteByte(90);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				Member.Serialize(stream, instance3);
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
		if (instance.invites != null)
		{
			for (int k = 0; k < instance.invites.Count; k++)
			{
				Invite instance4 = instance.invites[k];
				stream.WriteByte(98);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int position3 = stream.Position;
				Invite.Serialize(stream, instance4);
				int num3 = stream.Position - position3;
				if (num3 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field invites (ProtoBuf.ClanInfo.Invite)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num3, span3, 0);
			}
		}
		if (instance.maxMemberCount != 0)
		{
			stream.WriteByte(104);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.maxMemberCount);
		}
		if (instance.score != 0L)
		{
			stream.WriteByte(112);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.score);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (roles != null)
		{
			for (int i = 0; i < roles.Count; i++)
			{
				roles[i]?.InspectUids(action);
			}
		}
		if (members != null)
		{
			for (int j = 0; j < members.Count; j++)
			{
				members[j]?.InspectUids(action);
			}
		}
		if (invites != null)
		{
			for (int k = 0; k < invites.Count; k++)
			{
				invites[k]?.InspectUids(action);
			}
		}
	}
}
