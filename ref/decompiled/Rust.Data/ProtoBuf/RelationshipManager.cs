using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class RelationshipManager : IDisposable, Pool.IPooled, IProto<RelationshipManager>, IProto
{
	public class PlayerRelationshipInfo : IDisposable, Pool.IPooled, IProto<PlayerRelationshipInfo>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong playerID;

		[NonSerialized]
		public int type;

		[NonSerialized]
		public int weight;

		[NonSerialized]
		public uint mugshotCrc;

		[NonSerialized]
		public string displayName;

		[NonSerialized]
		public string notes;

		[NonSerialized]
		public float timeSinceSeen;

		public static void ResetToPool(PlayerRelationshipInfo instance)
		{
			if (instance.ShouldPool)
			{
				instance.playerID = 0uL;
				instance.type = 0;
				instance.weight = 0;
				instance.mugshotCrc = 0u;
				instance.displayName = string.Empty;
				instance.notes = string.Empty;
				instance.timeSinceSeen = 0f;
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
				throw new Exception("Trying to dispose PlayerRelationshipInfo with ShouldPool set to false!");
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

		public void CopyTo(PlayerRelationshipInfo instance)
		{
			instance.playerID = playerID;
			instance.type = type;
			instance.weight = weight;
			instance.mugshotCrc = mugshotCrc;
			instance.displayName = displayName;
			instance.notes = notes;
			instance.timeSinceSeen = timeSinceSeen;
		}

		public PlayerRelationshipInfo Copy()
		{
			PlayerRelationshipInfo playerRelationshipInfo = Pool.Get<PlayerRelationshipInfo>();
			CopyTo(playerRelationshipInfo);
			return playerRelationshipInfo;
		}

		public static PlayerRelationshipInfo Deserialize(BufferStream stream)
		{
			PlayerRelationshipInfo playerRelationshipInfo = Pool.Get<PlayerRelationshipInfo>();
			Deserialize(stream, playerRelationshipInfo, isDelta: false);
			return playerRelationshipInfo;
		}

		public static PlayerRelationshipInfo DeserializeLengthDelimited(BufferStream stream)
		{
			PlayerRelationshipInfo playerRelationshipInfo = Pool.Get<PlayerRelationshipInfo>();
			DeserializeLengthDelimited(stream, playerRelationshipInfo, isDelta: false);
			return playerRelationshipInfo;
		}

		public static PlayerRelationshipInfo DeserializeLength(BufferStream stream, int length)
		{
			PlayerRelationshipInfo playerRelationshipInfo = Pool.Get<PlayerRelationshipInfo>();
			DeserializeLength(stream, length, playerRelationshipInfo, isDelta: false);
			return playerRelationshipInfo;
		}

		public static PlayerRelationshipInfo Deserialize(byte[] buffer)
		{
			PlayerRelationshipInfo playerRelationshipInfo = Pool.Get<PlayerRelationshipInfo>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, playerRelationshipInfo, isDelta: false);
			return playerRelationshipInfo;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, PlayerRelationshipInfo previous)
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

		public static PlayerRelationshipInfo Deserialize(BufferStream stream, PlayerRelationshipInfo instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.weight = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.mugshotCrc = ProtocolParser.ReadUInt32(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.notes = ProtocolParser.ReadString(stream);
					continue;
				case 61:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.timeSinceSeen = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerRelationshipInfo DeserializeLengthDelimited(BufferStream stream, PlayerRelationshipInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.weight = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.mugshotCrc = ProtocolParser.ReadUInt32(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.notes = ProtocolParser.ReadString(stream);
					continue;
				case 61:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.timeSinceSeen = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerRelationshipInfo DeserializeLength(BufferStream stream, int length, PlayerRelationshipInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.type = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.weight = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.mugshotCrc = ProtocolParser.ReadUInt32(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.displayName = ProtocolParser.ReadString(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.notes = ProtocolParser.ReadString(stream);
					continue;
				case 61:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					instance.timeSinceSeen = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationshipInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, PlayerRelationshipInfo instance, PlayerRelationshipInfo previous)
		{
			if (instance.playerID != previous.playerID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
			if (instance.type != previous.type)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.weight != previous.weight)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.weight);
			}
			if (instance.mugshotCrc != previous.mugshotCrc)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, instance.mugshotCrc);
			}
			if (instance.displayName != null && instance.displayName != previous.displayName)
			{
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.notes != null && instance.notes != previous.notes)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteString(stream, instance.notes);
			}
			if (instance.timeSinceSeen != previous.timeSinceSeen)
			{
				stream.WriteByte(61);
				ProtocolParser.WriteSingle(stream, instance.timeSinceSeen);
			}
		}

		public static void Serialize(BufferStream stream, PlayerRelationshipInfo instance)
		{
			if (instance.playerID != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
			if (instance.type != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			}
			if (instance.weight != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.weight);
			}
			if (instance.mugshotCrc != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, instance.mugshotCrc);
			}
			if (instance.displayName != null)
			{
				stream.WriteByte(42);
				ProtocolParser.WriteString(stream, instance.displayName);
			}
			if (instance.notes != null)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteString(stream, instance.notes);
			}
			if (instance.timeSinceSeen != 0f)
			{
				stream.WriteByte(61);
				ProtocolParser.WriteSingle(stream, instance.timeSinceSeen);
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

	public class PlayerRelationships : IDisposable, Pool.IPooled, IProto<PlayerRelationships>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public ulong playerID;

		[NonSerialized]
		public List<PlayerRelationshipInfo> relations;

		public static void ResetToPool(PlayerRelationships instance)
		{
			if (!instance.ShouldPool)
			{
				return;
			}
			instance.playerID = 0uL;
			if (instance.relations != null)
			{
				for (int i = 0; i < instance.relations.Count; i++)
				{
					if (instance.relations[i] != null)
					{
						instance.relations[i].ResetToPool();
						instance.relations[i] = null;
					}
				}
				List<PlayerRelationshipInfo> obj = instance.relations;
				Pool.Free(ref obj, freeElements: false);
				instance.relations = obj;
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
				throw new Exception("Trying to dispose PlayerRelationships with ShouldPool set to false!");
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

		public void CopyTo(PlayerRelationships instance)
		{
			instance.playerID = playerID;
			if (relations != null)
			{
				instance.relations = Pool.Get<List<PlayerRelationshipInfo>>();
				for (int i = 0; i < relations.Count; i++)
				{
					PlayerRelationshipInfo item = relations[i].Copy();
					instance.relations.Add(item);
				}
			}
			else
			{
				instance.relations = null;
			}
		}

		public PlayerRelationships Copy()
		{
			PlayerRelationships playerRelationships = Pool.Get<PlayerRelationships>();
			CopyTo(playerRelationships);
			return playerRelationships;
		}

		public static PlayerRelationships Deserialize(BufferStream stream)
		{
			PlayerRelationships playerRelationships = Pool.Get<PlayerRelationships>();
			Deserialize(stream, playerRelationships, isDelta: false);
			return playerRelationships;
		}

		public static PlayerRelationships DeserializeLengthDelimited(BufferStream stream)
		{
			PlayerRelationships playerRelationships = Pool.Get<PlayerRelationships>();
			DeserializeLengthDelimited(stream, playerRelationships, isDelta: false);
			return playerRelationships;
		}

		public static PlayerRelationships DeserializeLength(BufferStream stream, int length)
		{
			PlayerRelationships playerRelationships = Pool.Get<PlayerRelationships>();
			DeserializeLength(stream, length, playerRelationships, isDelta: false);
			return playerRelationships;
		}

		public static PlayerRelationships Deserialize(byte[] buffer)
		{
			PlayerRelationships playerRelationships = Pool.Get<PlayerRelationships>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, playerRelationships, isDelta: false);
			return playerRelationships;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, PlayerRelationships previous)
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

		public static PlayerRelationships Deserialize(BufferStream stream, PlayerRelationships instance, bool isDelta)
		{
			if (!isDelta && instance.relations == null)
			{
				instance.relations = Pool.Get<List<PlayerRelationshipInfo>>();
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager.PlayerRelationships");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationships");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					stream.ConsumeRepeatedElement();
					instance.relations.Add(PlayerRelationshipInfo.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerRelationships DeserializeLengthDelimited(BufferStream stream, PlayerRelationships instance, bool isDelta)
		{
			if (!isDelta && instance.relations == null)
			{
				instance.relations = Pool.Get<List<PlayerRelationshipInfo>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager.PlayerRelationships");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationships");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					stream.ConsumeRepeatedElement();
					instance.relations.Add(PlayerRelationshipInfo.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static PlayerRelationships DeserializeLength(BufferStream stream, int length, PlayerRelationships instance, bool isDelta)
		{
			if (!isDelta && instance.relations == null)
			{
				instance.relations = Pool.Get<List<PlayerRelationshipInfo>>();
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
				stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager.PlayerRelationships");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationships");
					instance.playerID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					stream.ConsumeRepeatedElement();
					instance.relations.Add(PlayerRelationshipInfo.DeserializeLengthDelimited(stream));
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager.PlayerRelationships");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, PlayerRelationships instance, PlayerRelationships previous)
		{
			if (instance.playerID != previous.playerID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
			if (instance.relations == null)
			{
				return;
			}
			for (int i = 0; i < instance.relations.Count; i++)
			{
				PlayerRelationshipInfo playerRelationshipInfo = instance.relations[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerRelationshipInfo.SerializeDelta(stream, playerRelationshipInfo, playerRelationshipInfo);
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

		public static void Serialize(BufferStream stream, PlayerRelationships instance)
		{
			if (instance.playerID != 0L)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.playerID);
			}
			if (instance.relations == null)
			{
				return;
			}
			for (int i = 0; i < instance.relations.Count; i++)
			{
				PlayerRelationshipInfo instance2 = instance.relations[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerRelationshipInfo.Serialize(stream, instance2);
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
			if (relations != null)
			{
				for (int i = 0; i < relations.Count; i++)
				{
					relations[i]?.InspectUids(action);
				}
			}
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<PlayerTeam> teamList;

	[NonSerialized]
	public int maxTeamSize;

	[NonSerialized]
	public List<PlayerRelationships> relationships;

	public static void ResetToPool(RelationshipManager instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.teamList != null)
		{
			for (int i = 0; i < instance.teamList.Count; i++)
			{
				if (instance.teamList[i] != null)
				{
					instance.teamList[i].ResetToPool();
					instance.teamList[i] = null;
				}
			}
			List<PlayerTeam> obj = instance.teamList;
			Pool.Free(ref obj, freeElements: false);
			instance.teamList = obj;
		}
		instance.maxTeamSize = 0;
		if (instance.relationships != null)
		{
			for (int j = 0; j < instance.relationships.Count; j++)
			{
				if (instance.relationships[j] != null)
				{
					instance.relationships[j].ResetToPool();
					instance.relationships[j] = null;
				}
			}
			List<PlayerRelationships> obj2 = instance.relationships;
			Pool.Free(ref obj2, freeElements: false);
			instance.relationships = obj2;
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
			throw new Exception("Trying to dispose RelationshipManager with ShouldPool set to false!");
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

	public void CopyTo(RelationshipManager instance)
	{
		if (teamList != null)
		{
			instance.teamList = Pool.Get<List<PlayerTeam>>();
			for (int i = 0; i < teamList.Count; i++)
			{
				PlayerTeam item = teamList[i].Copy();
				instance.teamList.Add(item);
			}
		}
		else
		{
			instance.teamList = null;
		}
		instance.maxTeamSize = maxTeamSize;
		if (relationships != null)
		{
			instance.relationships = Pool.Get<List<PlayerRelationships>>();
			for (int j = 0; j < relationships.Count; j++)
			{
				PlayerRelationships item2 = relationships[j].Copy();
				instance.relationships.Add(item2);
			}
		}
		else
		{
			instance.relationships = null;
		}
	}

	public RelationshipManager Copy()
	{
		RelationshipManager relationshipManager = Pool.Get<RelationshipManager>();
		CopyTo(relationshipManager);
		return relationshipManager;
	}

	public static RelationshipManager Deserialize(BufferStream stream)
	{
		RelationshipManager relationshipManager = Pool.Get<RelationshipManager>();
		Deserialize(stream, relationshipManager, isDelta: false);
		return relationshipManager;
	}

	public static RelationshipManager DeserializeLengthDelimited(BufferStream stream)
	{
		RelationshipManager relationshipManager = Pool.Get<RelationshipManager>();
		DeserializeLengthDelimited(stream, relationshipManager, isDelta: false);
		return relationshipManager;
	}

	public static RelationshipManager DeserializeLength(BufferStream stream, int length)
	{
		RelationshipManager relationshipManager = Pool.Get<RelationshipManager>();
		DeserializeLength(stream, length, relationshipManager, isDelta: false);
		return relationshipManager;
	}

	public static RelationshipManager Deserialize(byte[] buffer)
	{
		RelationshipManager relationshipManager = Pool.Get<RelationshipManager>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, relationshipManager, isDelta: false);
		return relationshipManager;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, RelationshipManager previous)
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

	public static RelationshipManager Deserialize(BufferStream stream, RelationshipManager instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.teamList == null)
			{
				instance.teamList = Pool.Get<List<PlayerTeam>>();
			}
			if (instance.relationships == null)
			{
				instance.relationships = Pool.Get<List<PlayerRelationships>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager");
			switch (num)
			{
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				stream.ConsumeRepeatedElement();
				instance.teamList.Add(PlayerTeam.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager");
				instance.maxTeamSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				stream.ConsumeRepeatedElement();
				instance.relationships.Add(PlayerRelationships.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RelationshipManager DeserializeLengthDelimited(BufferStream stream, RelationshipManager instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.teamList == null)
			{
				instance.teamList = Pool.Get<List<PlayerTeam>>();
			}
			if (instance.relationships == null)
			{
				instance.relationships = Pool.Get<List<PlayerRelationships>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager");
			switch (num2)
			{
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				stream.ConsumeRepeatedElement();
				instance.teamList.Add(PlayerTeam.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager");
				instance.maxTeamSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				stream.ConsumeRepeatedElement();
				instance.relationships.Add(PlayerRelationships.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RelationshipManager DeserializeLength(BufferStream stream, int length, RelationshipManager instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.teamList == null)
			{
				instance.teamList = Pool.Get<List<PlayerTeam>>();
			}
			if (instance.relationships == null)
			{
				instance.relationships = Pool.Get<List<PlayerRelationships>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.RelationshipManager");
			switch (num2)
			{
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				stream.ConsumeRepeatedElement();
				instance.teamList.Add(PlayerTeam.DeserializeLengthDelimited(stream));
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager");
				instance.maxTeamSize = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 34:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				stream.ConsumeRepeatedElement();
				instance.relationships.Add(PlayerRelationships.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RelationshipManager");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, RelationshipManager instance, RelationshipManager previous)
	{
		if (instance.teamList != null)
		{
			for (int i = 0; i < instance.teamList.Count; i++)
			{
				PlayerTeam playerTeam = instance.teamList[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerTeam.SerializeDelta(stream, playerTeam, playerTeam);
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
		if (instance.maxTeamSize != previous.maxTeamSize)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.maxTeamSize);
		}
		if (instance.relationships == null)
		{
			return;
		}
		for (int j = 0; j < instance.relationships.Count; j++)
		{
			PlayerRelationships playerRelationships = instance.relationships[j];
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PlayerRelationships.SerializeDelta(stream, playerRelationships, playerRelationships);
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

	public static void Serialize(BufferStream stream, RelationshipManager instance)
	{
		if (instance.teamList != null)
		{
			for (int i = 0; i < instance.teamList.Count; i++)
			{
				PlayerTeam instance2 = instance.teamList[i];
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				PlayerTeam.Serialize(stream, instance2);
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
		if (instance.maxTeamSize != 0)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.maxTeamSize);
		}
		if (instance.relationships == null)
		{
			return;
		}
		for (int j = 0; j < instance.relationships.Count; j++)
		{
			PlayerRelationships instance3 = instance.relationships[j];
			stream.WriteByte(34);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PlayerRelationships.Serialize(stream, instance3);
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

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (teamList != null)
		{
			for (int i = 0; i < teamList.Count; i++)
			{
				teamList[i]?.InspectUids(action);
			}
		}
		if (relationships != null)
		{
			for (int j = 0; j < relationships.Count; j++)
			{
				relationships[j]?.InspectUids(action);
			}
		}
	}
}
