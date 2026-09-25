using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class PlayerLifeStory : IDisposable, Pool.IPooled, IProto<PlayerLifeStory>, IProto
{
	public class DeathInfo : IDisposable, Pool.IPooled, IProto<DeathInfo>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string attackerName;

		[NonSerialized]
		public ulong attackerSteamID;

		[NonSerialized]
		public string hitBone;

		[NonSerialized]
		public string inflictorName;

		[NonSerialized]
		public int lastDamageType;

		[NonSerialized]
		public float attackerDistance;

		public static void ResetToPool(DeathInfo instance)
		{
			if (instance.ShouldPool)
			{
				instance.attackerName = string.Empty;
				instance.attackerSteamID = 0uL;
				instance.hitBone = string.Empty;
				instance.inflictorName = string.Empty;
				instance.lastDamageType = 0;
				instance.attackerDistance = 0f;
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
				throw new Exception("Trying to dispose DeathInfo with ShouldPool set to false!");
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

		public void CopyTo(DeathInfo instance)
		{
			instance.attackerName = attackerName;
			instance.attackerSteamID = attackerSteamID;
			instance.hitBone = hitBone;
			instance.inflictorName = inflictorName;
			instance.lastDamageType = lastDamageType;
			instance.attackerDistance = attackerDistance;
		}

		public DeathInfo Copy()
		{
			DeathInfo deathInfo = Pool.Get<DeathInfo>();
			CopyTo(deathInfo);
			return deathInfo;
		}

		public static DeathInfo Deserialize(BufferStream stream)
		{
			DeathInfo deathInfo = Pool.Get<DeathInfo>();
			Deserialize(stream, deathInfo, isDelta: false);
			return deathInfo;
		}

		public static DeathInfo DeserializeLengthDelimited(BufferStream stream)
		{
			DeathInfo deathInfo = Pool.Get<DeathInfo>();
			DeserializeLengthDelimited(stream, deathInfo, isDelta: false);
			return deathInfo;
		}

		public static DeathInfo DeserializeLength(BufferStream stream, int length)
		{
			DeathInfo deathInfo = Pool.Get<DeathInfo>();
			DeserializeLength(stream, length, deathInfo, isDelta: false);
			return deathInfo;
		}

		public static DeathInfo Deserialize(byte[] buffer)
		{
			DeathInfo deathInfo = Pool.Get<DeathInfo>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, deathInfo, isDelta: false);
			return deathInfo;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, DeathInfo previous)
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

		public static DeathInfo Deserialize(BufferStream stream, DeathInfo instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.DeathInfo");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerSteamID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.hitBone = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.inflictorName = ProtocolParser.ReadString(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.lastDamageType = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 53:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerDistance = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DeathInfo DeserializeLengthDelimited(BufferStream stream, DeathInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.DeathInfo");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerSteamID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.hitBone = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.inflictorName = ProtocolParser.ReadString(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.lastDamageType = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 53:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerDistance = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static DeathInfo DeserializeLength(BufferStream stream, int length, DeathInfo instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.DeathInfo");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerSteamID = ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.hitBone = ProtocolParser.ReadString(stream);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.inflictorName = ProtocolParser.ReadString(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.lastDamageType = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 53:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					instance.attackerDistance = ProtocolParser.ReadSingle(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.DeathInfo");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, DeathInfo instance, DeathInfo previous)
		{
			if (instance.attackerName != null && instance.attackerName != previous.attackerName)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.attackerName);
			}
			if (instance.attackerSteamID != previous.attackerSteamID)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.attackerSteamID);
			}
			if (instance.hitBone != null && instance.hitBone != previous.hitBone)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.hitBone);
			}
			if (instance.inflictorName != null && instance.inflictorName != previous.inflictorName)
			{
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, instance.inflictorName);
			}
			if (instance.lastDamageType != previous.lastDamageType)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.lastDamageType);
			}
			if (instance.attackerDistance != previous.attackerDistance)
			{
				stream.WriteByte(53);
				ProtocolParser.WriteSingle(stream, instance.attackerDistance);
			}
		}

		public static void Serialize(BufferStream stream, DeathInfo instance)
		{
			if (instance.attackerName != null)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.attackerName);
			}
			if (instance.attackerSteamID != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.attackerSteamID);
			}
			if (instance.hitBone != null)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.hitBone);
			}
			if (instance.inflictorName != null)
			{
				stream.WriteByte(34);
				ProtocolParser.WriteString(stream, instance.inflictorName);
			}
			if (instance.lastDamageType != 0)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.lastDamageType);
			}
			if (instance.attackerDistance != 0f)
			{
				stream.WriteByte(53);
				ProtocolParser.WriteSingle(stream, instance.attackerDistance);
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

	public class GenericStat : IDisposable, Pool.IPooled, IProto<GenericStat>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string key;

		[NonSerialized]
		public int value;

		public static void ResetToPool(GenericStat instance)
		{
			if (instance.ShouldPool)
			{
				instance.key = string.Empty;
				instance.value = 0;
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
				throw new Exception("Trying to dispose GenericStat with ShouldPool set to false!");
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

		public void CopyTo(GenericStat instance)
		{
			instance.key = key;
			instance.value = value;
		}

		public GenericStat Copy()
		{
			GenericStat genericStat = Pool.Get<GenericStat>();
			CopyTo(genericStat);
			return genericStat;
		}

		public static GenericStat Deserialize(BufferStream stream)
		{
			GenericStat genericStat = Pool.Get<GenericStat>();
			Deserialize(stream, genericStat, isDelta: false);
			return genericStat;
		}

		public static GenericStat DeserializeLengthDelimited(BufferStream stream)
		{
			GenericStat genericStat = Pool.Get<GenericStat>();
			DeserializeLengthDelimited(stream, genericStat, isDelta: false);
			return genericStat;
		}

		public static GenericStat DeserializeLength(BufferStream stream, int length)
		{
			GenericStat genericStat = Pool.Get<GenericStat>();
			DeserializeLength(stream, length, genericStat, isDelta: false);
			return genericStat;
		}

		public static GenericStat Deserialize(byte[] buffer)
		{
			GenericStat genericStat = Pool.Get<GenericStat>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, genericStat, isDelta: false);
			return genericStat;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, GenericStat previous)
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

		public static GenericStat Deserialize(BufferStream stream, GenericStat instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.GenericStat");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					instance.key = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					instance.value = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static GenericStat DeserializeLengthDelimited(BufferStream stream, GenericStat instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.GenericStat");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					instance.key = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					instance.value = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static GenericStat DeserializeLength(BufferStream stream, int length, GenericStat instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.GenericStat");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					instance.key = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					instance.value = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.GenericStat");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, GenericStat instance, GenericStat previous)
		{
			if (instance.key != previous.key)
			{
				if (instance.key == null)
				{
					throw new ArgumentNullException("key", "Required by proto specification.");
				}
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.key);
			}
			if (instance.value != previous.value)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.value);
			}
		}

		public static void Serialize(BufferStream stream, GenericStat instance)
		{
			if (instance.key == null)
			{
				throw new ArgumentNullException("key", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.key);
			if (instance.value != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.value);
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

	public class WeaponStats : IDisposable, Pool.IPooled, IProto<WeaponStats>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public string weaponName;

		[NonSerialized]
		public ulong shotsFired;

		[NonSerialized]
		public ulong shotsHit;

		public static void ResetToPool(WeaponStats instance)
		{
			if (instance.ShouldPool)
			{
				instance.weaponName = string.Empty;
				instance.shotsFired = 0uL;
				instance.shotsHit = 0uL;
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
				throw new Exception("Trying to dispose WeaponStats with ShouldPool set to false!");
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

		public void CopyTo(WeaponStats instance)
		{
			instance.weaponName = weaponName;
			instance.shotsFired = shotsFired;
			instance.shotsHit = shotsHit;
		}

		public WeaponStats Copy()
		{
			WeaponStats weaponStats = Pool.Get<WeaponStats>();
			CopyTo(weaponStats);
			return weaponStats;
		}

		public static WeaponStats Deserialize(BufferStream stream)
		{
			WeaponStats weaponStats = Pool.Get<WeaponStats>();
			Deserialize(stream, weaponStats, isDelta: false);
			return weaponStats;
		}

		public static WeaponStats DeserializeLengthDelimited(BufferStream stream)
		{
			WeaponStats weaponStats = Pool.Get<WeaponStats>();
			DeserializeLengthDelimited(stream, weaponStats, isDelta: false);
			return weaponStats;
		}

		public static WeaponStats DeserializeLength(BufferStream stream, int length)
		{
			WeaponStats weaponStats = Pool.Get<WeaponStats>();
			DeserializeLength(stream, length, weaponStats, isDelta: false);
			return weaponStats;
		}

		public static WeaponStats Deserialize(byte[] buffer)
		{
			WeaponStats weaponStats = Pool.Get<WeaponStats>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, weaponStats, isDelta: false);
			return weaponStats;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, WeaponStats previous)
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

		public static WeaponStats Deserialize(BufferStream stream, WeaponStats instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.WeaponStats");
				switch (num)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.weaponName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.shotsFired = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.shotsHit = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static WeaponStats DeserializeLengthDelimited(BufferStream stream, WeaponStats instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.WeaponStats");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.weaponName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.shotsFired = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.shotsHit = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static WeaponStats DeserializeLength(BufferStream stream, int length, WeaponStats instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory.WeaponStats");
				switch (num2)
				{
				case 10:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.weaponName = ProtocolParser.ReadString(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.shotsFired = ProtocolParser.ReadUInt64(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					instance.shotsHit = ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory.WeaponStats");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, WeaponStats instance, WeaponStats previous)
		{
			if (instance.weaponName != previous.weaponName)
			{
				if (instance.weaponName == null)
				{
					throw new ArgumentNullException("weaponName", "Required by proto specification.");
				}
				stream.WriteByte(10);
				ProtocolParser.WriteString(stream, instance.weaponName);
			}
			if (instance.shotsFired != previous.shotsFired)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.shotsFired);
			}
			if (instance.shotsHit != previous.shotsHit)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.shotsHit);
			}
		}

		public static void Serialize(BufferStream stream, WeaponStats instance)
		{
			if (instance.weaponName == null)
			{
				throw new ArgumentNullException("weaponName", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteString(stream, instance.weaponName);
			if (instance.shotsFired != 0L)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.shotsFired);
			}
			if (instance.shotsHit != 0L)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt64(stream, instance.shotsHit);
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
	public float secondsAlive;

	[NonSerialized]
	public float metersWalked;

	[NonSerialized]
	public float metersRun;

	[NonSerialized]
	public float secondsSleeping;

	[NonSerialized]
	public uint timeBorn;

	[NonSerialized]
	public uint timeDied;

	[NonSerialized]
	public float secondsWilderness;

	[NonSerialized]
	public float secondsSwimming;

	[NonSerialized]
	public float secondsInBase;

	[NonSerialized]
	public float secondsInMonument;

	[NonSerialized]
	public float secondsFlying;

	[NonSerialized]
	public float secondsBoating;

	[NonSerialized]
	public float secondsDriving;

	[NonSerialized]
	public float totalDamageTaken;

	[NonSerialized]
	public float totalHealing;

	[NonSerialized]
	public DeathInfo deathInfo;

	[NonSerialized]
	public List<WeaponStats> weaponStats;

	[NonSerialized]
	public int killedPlayers;

	[NonSerialized]
	public int killedScientists;

	[NonSerialized]
	public int killedAnimals;

	[NonSerialized]
	public List<GenericStat> genericStats;

	[NonSerialized]
	public string wipeId;

	public static void ResetToPool(PlayerLifeStory instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.secondsAlive = 0f;
		instance.metersWalked = 0f;
		instance.metersRun = 0f;
		instance.secondsSleeping = 0f;
		instance.timeBorn = 0u;
		instance.timeDied = 0u;
		instance.secondsWilderness = 0f;
		instance.secondsSwimming = 0f;
		instance.secondsInBase = 0f;
		instance.secondsInMonument = 0f;
		instance.secondsFlying = 0f;
		instance.secondsBoating = 0f;
		instance.secondsDriving = 0f;
		instance.totalDamageTaken = 0f;
		instance.totalHealing = 0f;
		if (instance.deathInfo != null)
		{
			instance.deathInfo.ResetToPool();
			instance.deathInfo = null;
		}
		if (instance.weaponStats != null)
		{
			for (int i = 0; i < instance.weaponStats.Count; i++)
			{
				if (instance.weaponStats[i] != null)
				{
					instance.weaponStats[i].ResetToPool();
					instance.weaponStats[i] = null;
				}
			}
			List<WeaponStats> obj = instance.weaponStats;
			Pool.Free(ref obj, freeElements: false);
			instance.weaponStats = obj;
		}
		instance.killedPlayers = 0;
		instance.killedScientists = 0;
		instance.killedAnimals = 0;
		if (instance.genericStats != null)
		{
			for (int j = 0; j < instance.genericStats.Count; j++)
			{
				if (instance.genericStats[j] != null)
				{
					instance.genericStats[j].ResetToPool();
					instance.genericStats[j] = null;
				}
			}
			List<GenericStat> obj2 = instance.genericStats;
			Pool.Free(ref obj2, freeElements: false);
			instance.genericStats = obj2;
		}
		instance.wipeId = string.Empty;
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
			throw new Exception("Trying to dispose PlayerLifeStory with ShouldPool set to false!");
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

	public void CopyTo(PlayerLifeStory instance)
	{
		instance.secondsAlive = secondsAlive;
		instance.metersWalked = metersWalked;
		instance.metersRun = metersRun;
		instance.secondsSleeping = secondsSleeping;
		instance.timeBorn = timeBorn;
		instance.timeDied = timeDied;
		instance.secondsWilderness = secondsWilderness;
		instance.secondsSwimming = secondsSwimming;
		instance.secondsInBase = secondsInBase;
		instance.secondsInMonument = secondsInMonument;
		instance.secondsFlying = secondsFlying;
		instance.secondsBoating = secondsBoating;
		instance.secondsDriving = secondsDriving;
		instance.totalDamageTaken = totalDamageTaken;
		instance.totalHealing = totalHealing;
		if (deathInfo != null)
		{
			if (instance.deathInfo == null)
			{
				instance.deathInfo = deathInfo.Copy();
			}
			else
			{
				deathInfo.CopyTo(instance.deathInfo);
			}
		}
		else
		{
			instance.deathInfo = null;
		}
		if (weaponStats != null)
		{
			instance.weaponStats = Pool.Get<List<WeaponStats>>();
			for (int i = 0; i < weaponStats.Count; i++)
			{
				WeaponStats item = weaponStats[i].Copy();
				instance.weaponStats.Add(item);
			}
		}
		else
		{
			instance.weaponStats = null;
		}
		instance.killedPlayers = killedPlayers;
		instance.killedScientists = killedScientists;
		instance.killedAnimals = killedAnimals;
		if (genericStats != null)
		{
			instance.genericStats = Pool.Get<List<GenericStat>>();
			for (int j = 0; j < genericStats.Count; j++)
			{
				GenericStat item2 = genericStats[j].Copy();
				instance.genericStats.Add(item2);
			}
		}
		else
		{
			instance.genericStats = null;
		}
		instance.wipeId = wipeId;
	}

	public PlayerLifeStory Copy()
	{
		PlayerLifeStory playerLifeStory = Pool.Get<PlayerLifeStory>();
		CopyTo(playerLifeStory);
		return playerLifeStory;
	}

	public static PlayerLifeStory Deserialize(BufferStream stream)
	{
		PlayerLifeStory playerLifeStory = Pool.Get<PlayerLifeStory>();
		Deserialize(stream, playerLifeStory, isDelta: false);
		return playerLifeStory;
	}

	public static PlayerLifeStory DeserializeLengthDelimited(BufferStream stream)
	{
		PlayerLifeStory playerLifeStory = Pool.Get<PlayerLifeStory>();
		DeserializeLengthDelimited(stream, playerLifeStory, isDelta: false);
		return playerLifeStory;
	}

	public static PlayerLifeStory DeserializeLength(BufferStream stream, int length)
	{
		PlayerLifeStory playerLifeStory = Pool.Get<PlayerLifeStory>();
		DeserializeLength(stream, length, playerLifeStory, isDelta: false);
		return playerLifeStory;
	}

	public static PlayerLifeStory Deserialize(byte[] buffer)
	{
		PlayerLifeStory playerLifeStory = Pool.Get<PlayerLifeStory>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, playerLifeStory, isDelta: false);
		return playerLifeStory;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, PlayerLifeStory previous)
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

	public static PlayerLifeStory Deserialize(BufferStream stream, PlayerLifeStory instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.weaponStats == null)
			{
				instance.weaponStats = Pool.Get<List<WeaponStats>>();
			}
			if (instance.genericStats == null)
			{
				instance.genericStats = Pool.Get<List<GenericStat>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory");
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsAlive = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 101u:
				stream.ValidateFieldOrder(ref lastFieldId, 101u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.metersWalked = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 102u:
				stream.ValidateFieldOrder(ref lastFieldId, 102u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.metersRun = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 103u:
				stream.ValidateFieldOrder(ref lastFieldId, 103u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsSleeping = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 104u:
				stream.ValidateFieldOrder(ref lastFieldId, 104u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.timeBorn = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 105u:
				stream.ValidateFieldOrder(ref lastFieldId, 105u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.timeDied = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 110u:
				stream.ValidateFieldOrder(ref lastFieldId, 110u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsWilderness = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 111u:
				stream.ValidateFieldOrder(ref lastFieldId, 111u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsSwimming = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 112u:
				stream.ValidateFieldOrder(ref lastFieldId, 112u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsInBase = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 113u:
				stream.ValidateFieldOrder(ref lastFieldId, 113u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsInMonument = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 114u:
				stream.ValidateFieldOrder(ref lastFieldId, 114u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsFlying = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 115u:
				stream.ValidateFieldOrder(ref lastFieldId, 115u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsBoating = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 116u:
				stream.ValidateFieldOrder(ref lastFieldId, 116u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsDriving = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 150u:
				stream.ValidateFieldOrder(ref lastFieldId, 150u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.totalDamageTaken = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 151u:
				stream.ValidateFieldOrder(ref lastFieldId, 151u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.totalHealing = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 200u:
				stream.ValidateFieldOrder(ref lastFieldId, 200u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.deathInfo == null)
					{
						instance.deathInfo = DeathInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						DeathInfo.DeserializeLengthDelimited(stream, instance.deathInfo, isDelta);
					}
				}
				break;
			case 300u:
				stream.ValidateFieldOrder(ref lastFieldId, 300u, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.weaponStats.Add(WeaponStats.DeserializeLengthDelimited(stream));
				}
				break;
			case 301u:
				stream.ValidateFieldOrder(ref lastFieldId, 301u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedPlayers = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 302u:
				stream.ValidateFieldOrder(ref lastFieldId, 302u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedScientists = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 303u:
				stream.ValidateFieldOrder(ref lastFieldId, 303u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedAnimals = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 400u:
				stream.ValidateFieldOrder(ref lastFieldId, 400u, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.genericStats.Add(GenericStat.DeserializeLengthDelimited(stream));
				}
				break;
			case 401u:
				stream.ValidateFieldOrder(ref lastFieldId, 401u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.wipeId = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerLifeStory DeserializeLengthDelimited(BufferStream stream, PlayerLifeStory instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.weaponStats == null)
			{
				instance.weaponStats = Pool.Get<List<WeaponStats>>();
			}
			if (instance.genericStats == null)
			{
				instance.genericStats = Pool.Get<List<GenericStat>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsAlive = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 101u:
				stream.ValidateFieldOrder(ref lastFieldId, 101u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.metersWalked = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 102u:
				stream.ValidateFieldOrder(ref lastFieldId, 102u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.metersRun = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 103u:
				stream.ValidateFieldOrder(ref lastFieldId, 103u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsSleeping = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 104u:
				stream.ValidateFieldOrder(ref lastFieldId, 104u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.timeBorn = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 105u:
				stream.ValidateFieldOrder(ref lastFieldId, 105u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.timeDied = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 110u:
				stream.ValidateFieldOrder(ref lastFieldId, 110u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsWilderness = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 111u:
				stream.ValidateFieldOrder(ref lastFieldId, 111u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsSwimming = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 112u:
				stream.ValidateFieldOrder(ref lastFieldId, 112u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsInBase = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 113u:
				stream.ValidateFieldOrder(ref lastFieldId, 113u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsInMonument = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 114u:
				stream.ValidateFieldOrder(ref lastFieldId, 114u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsFlying = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 115u:
				stream.ValidateFieldOrder(ref lastFieldId, 115u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsBoating = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 116u:
				stream.ValidateFieldOrder(ref lastFieldId, 116u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsDriving = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 150u:
				stream.ValidateFieldOrder(ref lastFieldId, 150u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.totalDamageTaken = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 151u:
				stream.ValidateFieldOrder(ref lastFieldId, 151u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.totalHealing = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 200u:
				stream.ValidateFieldOrder(ref lastFieldId, 200u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.deathInfo == null)
					{
						instance.deathInfo = DeathInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						DeathInfo.DeserializeLengthDelimited(stream, instance.deathInfo, isDelta);
					}
				}
				break;
			case 300u:
				stream.ValidateFieldOrder(ref lastFieldId, 300u, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.weaponStats.Add(WeaponStats.DeserializeLengthDelimited(stream));
				}
				break;
			case 301u:
				stream.ValidateFieldOrder(ref lastFieldId, 301u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedPlayers = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 302u:
				stream.ValidateFieldOrder(ref lastFieldId, 302u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedScientists = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 303u:
				stream.ValidateFieldOrder(ref lastFieldId, 303u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedAnimals = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 400u:
				stream.ValidateFieldOrder(ref lastFieldId, 400u, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.genericStats.Add(GenericStat.DeserializeLengthDelimited(stream));
				}
				break;
			case 401u:
				stream.ValidateFieldOrder(ref lastFieldId, 401u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.wipeId = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static PlayerLifeStory DeserializeLength(BufferStream stream, int length, PlayerLifeStory instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.weaponStats == null)
			{
				instance.weaponStats = Pool.Get<List<WeaponStats>>();
			}
			if (instance.genericStats == null)
			{
				instance.genericStats = Pool.Get<List<GenericStat>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.PlayerLifeStory");
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 100u:
				stream.ValidateFieldOrder(ref lastFieldId, 100u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsAlive = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 101u:
				stream.ValidateFieldOrder(ref lastFieldId, 101u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.metersWalked = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 102u:
				stream.ValidateFieldOrder(ref lastFieldId, 102u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.metersRun = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 103u:
				stream.ValidateFieldOrder(ref lastFieldId, 103u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsSleeping = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 104u:
				stream.ValidateFieldOrder(ref lastFieldId, 104u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.timeBorn = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 105u:
				stream.ValidateFieldOrder(ref lastFieldId, 105u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.timeDied = ProtocolParser.ReadUInt32(stream);
				}
				break;
			case 110u:
				stream.ValidateFieldOrder(ref lastFieldId, 110u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsWilderness = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 111u:
				stream.ValidateFieldOrder(ref lastFieldId, 111u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsSwimming = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 112u:
				stream.ValidateFieldOrder(ref lastFieldId, 112u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsInBase = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 113u:
				stream.ValidateFieldOrder(ref lastFieldId, 113u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsInMonument = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 114u:
				stream.ValidateFieldOrder(ref lastFieldId, 114u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsFlying = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 115u:
				stream.ValidateFieldOrder(ref lastFieldId, 115u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsBoating = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 116u:
				stream.ValidateFieldOrder(ref lastFieldId, 116u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.secondsDriving = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 150u:
				stream.ValidateFieldOrder(ref lastFieldId, 150u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.totalDamageTaken = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 151u:
				stream.ValidateFieldOrder(ref lastFieldId, 151u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Fixed32)
				{
					instance.totalHealing = ProtocolParser.ReadSingle(stream);
				}
				break;
			case 200u:
				stream.ValidateFieldOrder(ref lastFieldId, 200u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					if (instance.deathInfo == null)
					{
						instance.deathInfo = DeathInfo.DeserializeLengthDelimited(stream);
					}
					else
					{
						DeathInfo.DeserializeLengthDelimited(stream, instance.deathInfo, isDelta);
					}
				}
				break;
			case 300u:
				stream.ValidateFieldOrder(ref lastFieldId, 300u, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.weaponStats.Add(WeaponStats.DeserializeLengthDelimited(stream));
				}
				break;
			case 301u:
				stream.ValidateFieldOrder(ref lastFieldId, 301u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedPlayers = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 302u:
				stream.ValidateFieldOrder(ref lastFieldId, 302u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedScientists = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 303u:
				stream.ValidateFieldOrder(ref lastFieldId, 303u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.Varint)
				{
					instance.killedAnimals = (int)ProtocolParser.ReadUInt64(stream);
				}
				break;
			case 400u:
				stream.ValidateFieldOrder(ref lastFieldId, 400u, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					stream.ConsumeRepeatedElement();
					instance.genericStats.Add(GenericStat.DeserializeLengthDelimited(stream));
				}
				break;
			case 401u:
				stream.ValidateFieldOrder(ref lastFieldId, 401u, fieldIsRepeated: false, "ProtoBuf.PlayerLifeStory");
				if (key.WireType == Wire.LengthDelimited)
				{
					instance.wipeId = ProtocolParser.ReadString(stream);
				}
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.PlayerLifeStory");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, PlayerLifeStory instance, PlayerLifeStory previous)
	{
		if (instance.secondsAlive != previous.secondsAlive)
		{
			stream.WriteByte(165);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsAlive);
		}
		if (instance.metersWalked != previous.metersWalked)
		{
			stream.WriteByte(173);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.metersWalked);
		}
		if (instance.metersRun != previous.metersRun)
		{
			stream.WriteByte(181);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.metersRun);
		}
		if (instance.secondsSleeping != previous.secondsSleeping)
		{
			stream.WriteByte(189);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsSleeping);
		}
		if (instance.timeBorn != previous.timeBorn)
		{
			stream.WriteByte(192);
			stream.WriteByte(6);
			ProtocolParser.WriteUInt32(stream, instance.timeBorn);
		}
		if (instance.timeDied != previous.timeDied)
		{
			stream.WriteByte(200);
			stream.WriteByte(6);
			ProtocolParser.WriteUInt32(stream, instance.timeDied);
		}
		if (instance.secondsWilderness != previous.secondsWilderness)
		{
			stream.WriteByte(245);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsWilderness);
		}
		if (instance.secondsSwimming != previous.secondsSwimming)
		{
			stream.WriteByte(253);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsSwimming);
		}
		if (instance.secondsInBase != previous.secondsInBase)
		{
			stream.WriteByte(133);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsInBase);
		}
		if (instance.secondsInMonument != previous.secondsInMonument)
		{
			stream.WriteByte(141);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsInMonument);
		}
		if (instance.secondsFlying != previous.secondsFlying)
		{
			stream.WriteByte(149);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsFlying);
		}
		if (instance.secondsBoating != previous.secondsBoating)
		{
			stream.WriteByte(157);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsBoating);
		}
		if (instance.secondsDriving != previous.secondsDriving)
		{
			stream.WriteByte(165);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsDriving);
		}
		if (instance.totalDamageTaken != previous.totalDamageTaken)
		{
			stream.WriteByte(181);
			stream.WriteByte(9);
			ProtocolParser.WriteSingle(stream, instance.totalDamageTaken);
		}
		if (instance.totalHealing != previous.totalHealing)
		{
			stream.WriteByte(189);
			stream.WriteByte(9);
			ProtocolParser.WriteSingle(stream, instance.totalHealing);
		}
		if (instance.deathInfo != null)
		{
			stream.WriteByte(194);
			stream.WriteByte(12);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			DeathInfo.SerializeDelta(stream, instance.deathInfo, previous.deathInfo);
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
		if (instance.weaponStats != null)
		{
			for (int i = 0; i < instance.weaponStats.Count; i++)
			{
				WeaponStats weaponStats = instance.weaponStats[i];
				stream.WriteByte(226);
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				WeaponStats.SerializeDelta(stream, weaponStats, weaponStats);
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
		if (instance.killedPlayers != previous.killedPlayers)
		{
			stream.WriteByte(232);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.killedPlayers);
		}
		if (instance.killedScientists != previous.killedScientists)
		{
			stream.WriteByte(240);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.killedScientists);
		}
		if (instance.killedAnimals != previous.killedAnimals)
		{
			stream.WriteByte(248);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.killedAnimals);
		}
		if (instance.genericStats != null)
		{
			for (int j = 0; j < instance.genericStats.Count; j++)
			{
				GenericStat genericStat = instance.genericStats[j];
				stream.WriteByte(130);
				stream.WriteByte(25);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				GenericStat.SerializeDelta(stream, genericStat, genericStat);
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
		if (instance.wipeId != null && instance.wipeId != previous.wipeId)
		{
			stream.WriteByte(138);
			stream.WriteByte(25);
			ProtocolParser.WriteString(stream, instance.wipeId);
		}
	}

	public static void Serialize(BufferStream stream, PlayerLifeStory instance)
	{
		if (instance.secondsAlive != 0f)
		{
			stream.WriteByte(165);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsAlive);
		}
		if (instance.metersWalked != 0f)
		{
			stream.WriteByte(173);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.metersWalked);
		}
		if (instance.metersRun != 0f)
		{
			stream.WriteByte(181);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.metersRun);
		}
		if (instance.secondsSleeping != 0f)
		{
			stream.WriteByte(189);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsSleeping);
		}
		if (instance.timeBorn != 0)
		{
			stream.WriteByte(192);
			stream.WriteByte(6);
			ProtocolParser.WriteUInt32(stream, instance.timeBorn);
		}
		if (instance.timeDied != 0)
		{
			stream.WriteByte(200);
			stream.WriteByte(6);
			ProtocolParser.WriteUInt32(stream, instance.timeDied);
		}
		if (instance.secondsWilderness != 0f)
		{
			stream.WriteByte(245);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsWilderness);
		}
		if (instance.secondsSwimming != 0f)
		{
			stream.WriteByte(253);
			stream.WriteByte(6);
			ProtocolParser.WriteSingle(stream, instance.secondsSwimming);
		}
		if (instance.secondsInBase != 0f)
		{
			stream.WriteByte(133);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsInBase);
		}
		if (instance.secondsInMonument != 0f)
		{
			stream.WriteByte(141);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsInMonument);
		}
		if (instance.secondsFlying != 0f)
		{
			stream.WriteByte(149);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsFlying);
		}
		if (instance.secondsBoating != 0f)
		{
			stream.WriteByte(157);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsBoating);
		}
		if (instance.secondsDriving != 0f)
		{
			stream.WriteByte(165);
			stream.WriteByte(7);
			ProtocolParser.WriteSingle(stream, instance.secondsDriving);
		}
		if (instance.totalDamageTaken != 0f)
		{
			stream.WriteByte(181);
			stream.WriteByte(9);
			ProtocolParser.WriteSingle(stream, instance.totalDamageTaken);
		}
		if (instance.totalHealing != 0f)
		{
			stream.WriteByte(189);
			stream.WriteByte(9);
			ProtocolParser.WriteSingle(stream, instance.totalHealing);
		}
		if (instance.deathInfo != null)
		{
			stream.WriteByte(194);
			stream.WriteByte(12);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			DeathInfo.Serialize(stream, instance.deathInfo);
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
		if (instance.weaponStats != null)
		{
			for (int i = 0; i < instance.weaponStats.Count; i++)
			{
				WeaponStats instance2 = instance.weaponStats[i];
				stream.WriteByte(226);
				stream.WriteByte(18);
				BufferStream.RangeHandle range2 = stream.GetRange(5);
				int position2 = stream.Position;
				WeaponStats.Serialize(stream, instance2);
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
		if (instance.killedPlayers != 0)
		{
			stream.WriteByte(232);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.killedPlayers);
		}
		if (instance.killedScientists != 0)
		{
			stream.WriteByte(240);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.killedScientists);
		}
		if (instance.killedAnimals != 0)
		{
			stream.WriteByte(248);
			stream.WriteByte(18);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.killedAnimals);
		}
		if (instance.genericStats != null)
		{
			for (int j = 0; j < instance.genericStats.Count; j++)
			{
				GenericStat instance3 = instance.genericStats[j];
				stream.WriteByte(130);
				stream.WriteByte(25);
				BufferStream.RangeHandle range3 = stream.GetRange(5);
				int position3 = stream.Position;
				GenericStat.Serialize(stream, instance3);
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
		if (instance.wipeId != null)
		{
			stream.WriteByte(138);
			stream.WriteByte(25);
			ProtocolParser.WriteString(stream, instance.wipeId);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		deathInfo?.InspectUids(action);
		if (weaponStats != null)
		{
			for (int i = 0; i < weaponStats.Count; i++)
			{
				weaponStats[i]?.InspectUids(action);
			}
		}
		if (genericStats != null)
		{
			for (int j = 0; j < genericStats.Count; j++)
			{
				genericStats[j]?.InspectUids(action);
			}
		}
	}
}
