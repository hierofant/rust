using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class AppTeamInfo : IDisposable, Pool.IPooled, IProto<AppTeamInfo>, IProto
{
	public class Member : IDisposable, Pool.IPooled, IProto<Member>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong steamId;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public float x;

		[NonSerialized]
		public float y;

		[NonSerialized]
		public bool isOnline;

		[NonSerialized]
		public uint spawnTime;

		[NonSerialized]
		public bool isAlive;

		[NonSerialized]
		public uint deathTime;

		public static void ResetToPool(Member instance)
		{
			if (instance.ShouldPool)
			{
				instance.steamId = 0uL;
				instance.name = string.Empty;
				instance.x = 0f;
				instance.y = 0f;
				instance.isOnline = false;
				instance.spawnTime = 0u;
				instance.isAlive = false;
				instance.deathTime = 0u;
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
			instance.name = name;
			instance.x = x;
			instance.y = y;
			instance.isOnline = isOnline;
			instance.spawnTime = spawnTime;
			instance.isAlive = isAlive;
			instance.deathTime = deathTime;
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
				instance.x = 0f;
				instance.y = 0f;
				instance.isOnline = false;
				instance.spawnTime = 0u;
				instance.isAlive = false;
				instance.deathTime = 0u;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo.Member");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.isOnline = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.spawnTime = ProtocolParser.ReadUInt32(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.isAlive = ProtocolParser.ReadBool(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.deathTime = ProtocolParser.ReadUInt32(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo.Member");
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
				instance.x = 0f;
				instance.y = 0f;
				instance.isOnline = false;
				instance.spawnTime = 0u;
				instance.isAlive = false;
				instance.deathTime = 0u;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo.Member");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.isOnline = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.spawnTime = ProtocolParser.ReadUInt32(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.isAlive = ProtocolParser.ReadBool(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.deathTime = ProtocolParser.ReadUInt32(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo.Member");
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
				instance.x = 0f;
				instance.y = 0f;
				instance.isOnline = false;
				instance.spawnTime = 0u;
				instance.isAlive = false;
				instance.deathTime = 0u;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo.Member");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.steamId = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.isOnline = ProtocolParser.ReadBool(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.spawnTime = ProtocolParser.ReadUInt32(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.isAlive = ProtocolParser.ReadBool(stream);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					instance.deathTime = ProtocolParser.ReadUInt32(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Member");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo.Member");
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
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(18);
				ProtocolParser.WriteString(stream, instance.name);
			}
			if (instance.x != previous.x)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.x);
			}
			if (instance.y != previous.y)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.y);
			}
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.isOnline);
			if (instance.spawnTime != previous.spawnTime)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt32(stream, instance.spawnTime);
			}
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.isAlive);
			if (instance.deathTime != previous.deathTime)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt32(stream, instance.deathTime);
			}
		}

		public static void Serialize(BufferStream stream, Member instance)
		{
			if (instance.steamId != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.steamId);
			}
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.x != 0f)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.x);
			}
			if (instance.y != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.y);
			}
			if (instance.isOnline)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteBool(stream, instance.isOnline);
			}
			if (instance.spawnTime != 0)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt32(stream, instance.spawnTime);
			}
			if (instance.isAlive)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteBool(stream, instance.isAlive);
			}
			if (instance.deathTime != 0)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteUInt32(stream, instance.deathTime);
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

	public class Note : IDisposable, Pool.IPooled, IProto<Note>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int type;

		[NonSerialized]
		public float x;

		[NonSerialized]
		public float y;

		[NonSerialized]
		public int icon;

		[NonSerialized]
		public int colourIndex;

		[NonSerialized]
		public string label;

		public static void ResetToPool(Note instance)
		{
			if (instance.ShouldPool)
			{
				instance.type = 0;
				instance.x = 0f;
				instance.y = 0f;
				instance.icon = 0;
				instance.colourIndex = 0;
				instance.label = string.Empty;
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
				throw new Exception("Trying to dispose Note with ShouldPool set to false!");
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

		public void CopyTo(Note instance)
		{
			instance.type = type;
			instance.x = x;
			instance.y = y;
			instance.icon = icon;
			instance.colourIndex = colourIndex;
			instance.label = label;
		}

		public Note Copy()
		{
			Note note = Pool.Get<Note>();
			CopyTo(note);
			return note;
		}

		public static Note Deserialize(BufferStream stream)
		{
			Note note = Pool.Get<Note>();
			Deserialize(stream, note, isDelta: false);
			return note;
		}

		public static Note DeserializeLengthDelimited(BufferStream stream)
		{
			Note note = Pool.Get<Note>();
			DeserializeLengthDelimited(stream, note, isDelta: false);
			return note;
		}

		public static Note DeserializeLength(BufferStream stream, int length)
		{
			Note note = Pool.Get<Note>();
			DeserializeLength(stream, length, note, isDelta: false);
			return note;
		}

		public static Note Deserialize(byte[] buffer)
		{
			Note note = Pool.Get<Note>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, note, isDelta: false);
			return note;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Note previous)
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

		public static Note Deserialize(BufferStream stream, Note instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.type = 0;
				instance.x = 0f;
				instance.y = 0f;
				instance.icon = 0;
				instance.colourIndex = 0;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo.Note");
				switch (num)
				{
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.icon = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.colourIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.label = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Note DeserializeLengthDelimited(BufferStream stream, Note instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.type = 0;
				instance.x = 0f;
				instance.y = 0f;
				instance.icon = 0;
				instance.colourIndex = 0;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo.Note");
				switch (num2)
				{
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.icon = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.colourIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.label = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Note DeserializeLength(BufferStream stream, int length, Note instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.type = 0;
				instance.x = 0f;
				instance.y = 0f;
				instance.icon = 0;
				instance.colourIndex = 0;
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
				stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo.Note");
				switch (num2)
				{
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 29:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.x = ProtocolParser.ReadSingle(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.y = ProtocolParser.ReadSingle(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.icon = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.colourIndex = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					instance.label = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo.Note");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Note instance, Note previous)
		{
			if (instance.type != previous.type)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.x != previous.x)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.x);
			}
			if (instance.y != previous.y)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.y);
			}
			if (instance.icon != previous.icon)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.icon);
			}
			if (instance.colourIndex != previous.colourIndex)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.colourIndex);
			}
			if (instance.label != null && instance.label != previous.label)
			{
				stream.WriteByte(58);
				ProtocolParser.WriteString(stream, instance.label);
			}
		}

		public static void Serialize(BufferStream stream, Note instance)
		{
			if (instance.type != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.x != 0f)
			{
				stream.WriteByte(29);
				ProtocolParser.WriteSingle(stream, instance.x);
			}
			if (instance.y != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.y);
			}
			if (instance.icon != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.icon);
			}
			if (instance.colourIndex != 0)
			{
				stream.WriteByte(48);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.colourIndex);
			}
			if (instance.label != null)
			{
				stream.WriteByte(58);
				ProtocolParser.WriteString(stream, instance.label);
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
	public ulong leaderSteamId;

	[NonSerialized]
	public List<Member> members;

	[NonSerialized]
	public List<Note> mapNotes;

	[NonSerialized]
	public List<Note> leaderMapNotes;

	public static void ResetToPool(AppTeamInfo instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.leaderSteamId = 0uL;
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
			List<Member> obj = instance.members;
			Pool.Free(ref obj, freeElements: false);
			instance.members = obj;
		}
		if (instance.mapNotes != null)
		{
			for (int j = 0; j < instance.mapNotes.Count; j++)
			{
				if (instance.mapNotes[j] != null)
				{
					instance.mapNotes[j].ResetToPool();
					instance.mapNotes[j] = null;
				}
			}
			List<Note> obj2 = instance.mapNotes;
			Pool.Free(ref obj2, freeElements: false);
			instance.mapNotes = obj2;
		}
		if (instance.leaderMapNotes != null)
		{
			for (int k = 0; k < instance.leaderMapNotes.Count; k++)
			{
				if (instance.leaderMapNotes[k] != null)
				{
					instance.leaderMapNotes[k].ResetToPool();
					instance.leaderMapNotes[k] = null;
				}
			}
			List<Note> obj3 = instance.leaderMapNotes;
			Pool.Free(ref obj3, freeElements: false);
			instance.leaderMapNotes = obj3;
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
			throw new Exception("Trying to dispose AppTeamInfo with ShouldPool set to false!");
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

	public void CopyTo(AppTeamInfo instance)
	{
		instance.leaderSteamId = leaderSteamId;
		if (members != null)
		{
			instance.members = Pool.Get<List<Member>>();
			for (int i = 0; i < members.Count; i++)
			{
				Member item = members[i].Copy();
				instance.members.Add(item);
			}
		}
		else
		{
			instance.members = null;
		}
		if (mapNotes != null)
		{
			instance.mapNotes = Pool.Get<List<Note>>();
			for (int j = 0; j < mapNotes.Count; j++)
			{
				Note item2 = mapNotes[j].Copy();
				instance.mapNotes.Add(item2);
			}
		}
		else
		{
			instance.mapNotes = null;
		}
		if (leaderMapNotes != null)
		{
			instance.leaderMapNotes = Pool.Get<List<Note>>();
			for (int k = 0; k < leaderMapNotes.Count; k++)
			{
				Note item3 = leaderMapNotes[k].Copy();
				instance.leaderMapNotes.Add(item3);
			}
		}
		else
		{
			instance.leaderMapNotes = null;
		}
	}

	public AppTeamInfo Copy()
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		CopyTo(appTeamInfo);
		return appTeamInfo;
	}

	public static AppTeamInfo Deserialize(BufferStream stream)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		Deserialize(stream, appTeamInfo, isDelta: false);
		return appTeamInfo;
	}

	public static AppTeamInfo DeserializeLengthDelimited(BufferStream stream)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		DeserializeLengthDelimited(stream, appTeamInfo, isDelta: false);
		return appTeamInfo;
	}

	public static AppTeamInfo DeserializeLength(BufferStream stream, int length)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		DeserializeLength(stream, length, appTeamInfo, isDelta: false);
		return appTeamInfo;
	}

	public static AppTeamInfo Deserialize(byte[] buffer)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appTeamInfo, isDelta: false);
		return appTeamInfo;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppTeamInfo previous)
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

	public static AppTeamInfo Deserialize(BufferStream stream, AppTeamInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.leaderSteamId = 0uL;
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<Member>>();
			}
			if (instance.mapNotes == null)
			{
				instance.mapNotes = Pool.Get<List<Note>>();
			}
			if (instance.leaderMapNotes == null)
			{
				instance.leaderMapNotes = Pool.Get<List<Note>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo");
				instance.leaderSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.members.Add(Member.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.mapNotes.Add(Note.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.leaderMapNotes.Add(Note.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppTeamInfo DeserializeLengthDelimited(BufferStream stream, AppTeamInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.leaderSteamId = 0uL;
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<Member>>();
			}
			if (instance.mapNotes == null)
			{
				instance.mapNotes = Pool.Get<List<Note>>();
			}
			if (instance.leaderMapNotes == null)
			{
				instance.leaderMapNotes = Pool.Get<List<Note>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo");
				instance.leaderSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.members.Add(Member.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.mapNotes.Add(Note.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.leaderMapNotes.Add(Note.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppTeamInfo DeserializeLength(BufferStream stream, int length, AppTeamInfo instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.leaderSteamId = 0uL;
			if (instance.members == null)
			{
				instance.members = Pool.Get<List<Member>>();
			}
			if (instance.mapNotes == null)
			{
				instance.mapNotes = Pool.Get<List<Note>>();
			}
			if (instance.leaderMapNotes == null)
			{
				instance.leaderMapNotes = Pool.Get<List<Note>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.AppTeamInfo");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo");
				instance.leaderSteamId = ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.members.Add(Member.DeserializeLengthDelimited(stream));
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.mapNotes.Add(Note.DeserializeLengthDelimited(stream));
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				stream.ConsumeRepeatedElement();
				instance.leaderMapNotes.Add(Note.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppTeamInfo");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppTeamInfo instance, AppTeamInfo previous)
	{
		if (instance.leaderSteamId != previous.leaderSteamId)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.leaderSteamId);
		}
		if (instance.members != null)
		{
			for (int i = 0; i < instance.members.Count; i++)
			{
				Member member = instance.members[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Member.SerializeDelta(stream, member, member);
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
		if (instance.mapNotes != null)
		{
			for (int j = 0; j < instance.mapNotes.Count; j++)
			{
				Note note = instance.mapNotes[j];
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				Note.SerializeDelta(stream, note, note);
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
		if (instance.leaderMapNotes == null)
		{
			return;
		}
		for (int k = 0; k < instance.leaderMapNotes.Count; k++)
		{
			Note note2 = instance.leaderMapNotes[k];
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			Note.SerializeDelta(stream, note2, note2);
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

	public static void Serialize(BufferStream stream, AppTeamInfo instance)
	{
		if (instance.leaderSteamId != 0L)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.leaderSteamId);
		}
		if (instance.members != null)
		{
			for (int i = 0; i < instance.members.Count; i++)
			{
				Member instance2 = instance.members[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Member.Serialize(stream, instance2);
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
		if (instance.mapNotes != null)
		{
			for (int j = 0; j < instance.mapNotes.Count; j++)
			{
				Note instance3 = instance.mapNotes[j];
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				Note.Serialize(stream, instance3);
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
		if (instance.leaderMapNotes == null)
		{
			return;
		}
		for (int k = 0; k < instance.leaderMapNotes.Count; k++)
		{
			Note instance4 = instance.leaderMapNotes[k];
			stream.WriteByte(34);
			BufferStream.RangeHandle range3 = stream.GetRange(5);
			int position3 = stream.Position;
			Note.Serialize(stream, instance4);
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
		if (mapNotes != null)
		{
			for (int j = 0; j < mapNotes.Count; j++)
			{
				mapNotes[j]?.InspectUids(action);
			}
		}
		if (leaderMapNotes != null)
		{
			for (int k = 0; k < leaderMapNotes.Count; k++)
			{
				leaderMapNotes[k]?.InspectUids(action);
			}
		}
	}
}
