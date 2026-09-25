using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class RespawnInformation : IDisposable, Pool.IPooled, IProto<RespawnInformation>, IProto
{
	public class SpawnOptions : IDisposable, Pool.IPooled, IProto<SpawnOptions>, IProto
	{
		public enum RespawnType
		{
			SleepingBag = 1,
			Bed,
			BeachTowel,
			Camper,
			Static
		}

		public enum RespawnState
		{
			OK = 1,
			Occupied,
			Underwater,
			InNoRespawnZone
		}

		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public RespawnType type;

		[NonSerialized]
		public NetworkableId id;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public float unlockSeconds;

		[NonSerialized]
		public Vector3 worldPosition;

		[NonSerialized]
		public RespawnState respawnState;

		[NonSerialized]
		public bool mobile;

		[NonSerialized]
		public string nexusZone;

		[NonSerialized]
		public bool corpse;

		[NonSerialized]
		public bool deepSea;

		[NonSerialized]
		public bool showOnCompass;

		[NonSerialized]
		public bool favourite;

		public static void ResetToPool(SpawnOptions instance)
		{
			if (instance.ShouldPool)
			{
				instance.type = (RespawnType)0;
				instance.id = default(NetworkableId);
				instance.name = string.Empty;
				instance.unlockSeconds = 0f;
				instance.worldPosition = default(Vector3);
				instance.respawnState = (RespawnState)0;
				instance.mobile = false;
				instance.nexusZone = string.Empty;
				instance.corpse = false;
				instance.deepSea = false;
				instance.showOnCompass = false;
				instance.favourite = false;
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
				throw new Exception("Trying to dispose SpawnOptions with ShouldPool set to false!");
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

		public void CopyTo(SpawnOptions instance)
		{
			instance.type = type;
			instance.id = id;
			instance.name = name;
			instance.unlockSeconds = unlockSeconds;
			instance.worldPosition = worldPosition;
			instance.respawnState = respawnState;
			instance.mobile = mobile;
			instance.nexusZone = nexusZone;
			instance.corpse = corpse;
			instance.deepSea = deepSea;
			instance.showOnCompass = showOnCompass;
			instance.favourite = favourite;
		}

		public SpawnOptions Copy()
		{
			SpawnOptions spawnOptions = Pool.Get<SpawnOptions>();
			CopyTo(spawnOptions);
			return spawnOptions;
		}

		public static SpawnOptions Deserialize(BufferStream stream)
		{
			SpawnOptions spawnOptions = Pool.Get<SpawnOptions>();
			Deserialize(stream, spawnOptions, isDelta: false);
			return spawnOptions;
		}

		public static SpawnOptions DeserializeLengthDelimited(BufferStream stream)
		{
			SpawnOptions spawnOptions = Pool.Get<SpawnOptions>();
			DeserializeLengthDelimited(stream, spawnOptions, isDelta: false);
			return spawnOptions;
		}

		public static SpawnOptions DeserializeLength(BufferStream stream, int length)
		{
			SpawnOptions spawnOptions = Pool.Get<SpawnOptions>();
			DeserializeLength(stream, length, spawnOptions, isDelta: false);
			return spawnOptions;
		}

		public static SpawnOptions Deserialize(byte[] buffer)
		{
			SpawnOptions spawnOptions = Pool.Get<SpawnOptions>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, spawnOptions, isDelta: false);
			return spawnOptions;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, SpawnOptions previous)
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

		public static SpawnOptions Deserialize(BufferStream stream, SpawnOptions instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.respawnState = RespawnState.OK;
			}
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.RespawnInformation.SpawnOptions");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.type = (RespawnType)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.id = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.unlockSeconds = ProtocolParser.ReadSingle(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldPosition, isDelta);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.respawnState = (RespawnState)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.mobile = ProtocolParser.ReadBool(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.nexusZone = ProtocolParser.ReadString(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.corpse = ProtocolParser.ReadBool(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.deepSea = ProtocolParser.ReadBool(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.showOnCompass = ProtocolParser.ReadBool(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.favourite = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SpawnOptions DeserializeLengthDelimited(BufferStream stream, SpawnOptions instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.respawnState = RespawnState.OK;
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
				stream.ConsumeFieldOperation("ProtoBuf.RespawnInformation.SpawnOptions");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.type = (RespawnType)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.id = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.unlockSeconds = ProtocolParser.ReadSingle(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldPosition, isDelta);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.respawnState = (RespawnState)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.mobile = ProtocolParser.ReadBool(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.nexusZone = ProtocolParser.ReadString(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.corpse = ProtocolParser.ReadBool(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.deepSea = ProtocolParser.ReadBool(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.showOnCompass = ProtocolParser.ReadBool(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.favourite = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SpawnOptions DeserializeLength(BufferStream stream, int length, SpawnOptions instance, bool isDelta)
		{
			if (!isDelta)
			{
				instance.respawnState = RespawnState.OK;
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
				stream.ConsumeFieldOperation("ProtoBuf.RespawnInformation.SpawnOptions");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.type = (RespawnType)ProtocolParser.ReadUInt64(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.id = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 37:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.unlockSeconds = ProtocolParser.ReadSingle(stream);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.worldPosition, isDelta);
					continue;
				case 48:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.respawnState = (RespawnState)ProtocolParser.ReadUInt64(stream);
					continue;
				case 56:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.mobile = ProtocolParser.ReadBool(stream);
					continue;
				case 66:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.nexusZone = ProtocolParser.ReadString(stream);
					continue;
				case 72:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.corpse = ProtocolParser.ReadBool(stream);
					continue;
				case 80:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.deepSea = ProtocolParser.ReadBool(stream);
					continue;
				case 88:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.showOnCompass = ProtocolParser.ReadBool(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					instance.favourite = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RespawnInformation.SpawnOptions");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, SpawnOptions instance, SpawnOptions previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, instance.id.Value);
			if (instance.name != previous.name)
			{
				if (instance.name == null)
				{
					throw new ArgumentNullException("name", "Required by proto specification.");
				}
				stream.WriteByte(26);
				ProtocolParser.WriteString(stream, instance.name);
			}
			if (instance.unlockSeconds != previous.unlockSeconds)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.unlockSeconds);
			}
			if (instance.worldPosition != previous.worldPosition)
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.worldPosition, previous.worldPosition);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field worldPosition (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.respawnState);
			stream.WriteByte(56);
			ProtocolParser.WriteBool(stream, instance.mobile);
			if (instance.nexusZone != null && instance.nexusZone != previous.nexusZone)
			{
				stream.WriteByte(66);
				ProtocolParser.WriteString(stream, instance.nexusZone);
			}
			stream.WriteByte(72);
			ProtocolParser.WriteBool(stream, instance.corpse);
			stream.WriteByte(80);
			ProtocolParser.WriteBool(stream, instance.deepSea);
			stream.WriteByte(88);
			ProtocolParser.WriteBool(stream, instance.showOnCompass);
			stream.WriteByte(96);
			ProtocolParser.WriteBool(stream, instance.favourite);
		}

		public static void Serialize(BufferStream stream, SpawnOptions instance)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			if (instance.id != default(NetworkableId))
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt64(stream, instance.id.Value);
			}
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(26);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.unlockSeconds != 0f)
			{
				stream.WriteByte(37);
				ProtocolParser.WriteSingle(stream, instance.unlockSeconds);
			}
			if (instance.worldPosition != default(Vector3))
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.Serialize(stream, instance.worldPosition);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field worldPosition (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			stream.WriteByte(48);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.respawnState);
			if (instance.mobile)
			{
				stream.WriteByte(56);
				ProtocolParser.WriteBool(stream, instance.mobile);
			}
			if (instance.nexusZone != null)
			{
				stream.WriteByte(66);
				ProtocolParser.WriteString(stream, instance.nexusZone);
			}
			if (instance.corpse)
			{
				stream.WriteByte(72);
				ProtocolParser.WriteBool(stream, instance.corpse);
			}
			if (instance.deepSea)
			{
				stream.WriteByte(80);
				ProtocolParser.WriteBool(stream, instance.deepSea);
			}
			if (instance.showOnCompass)
			{
				stream.WriteByte(88);
				ProtocolParser.WriteBool(stream, instance.showOnCompass);
			}
			if (instance.favourite)
			{
				stream.WriteByte(96);
				ProtocolParser.WriteBool(stream, instance.favourite);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			action(UidType.NetworkableId, ref id.Value);
		}
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<SpawnOptions> spawnOptions;

	[NonSerialized]
	public PlayerLifeStory previousLife;

	[NonSerialized]
	public bool fadeIn;

	[NonSerialized]
	public bool loading;

	[NonSerialized]
	public List<Vector3> shelterPositions;

	public static void ResetToPool(RespawnInformation instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.spawnOptions != null)
		{
			for (int i = 0; i < instance.spawnOptions.Count; i++)
			{
				if (instance.spawnOptions[i] != null)
				{
					instance.spawnOptions[i].ResetToPool();
					instance.spawnOptions[i] = null;
				}
			}
			List<SpawnOptions> obj = instance.spawnOptions;
			Pool.Free(ref obj, freeElements: false);
			instance.spawnOptions = obj;
		}
		if (instance.previousLife != null)
		{
			instance.previousLife.ResetToPool();
			instance.previousLife = null;
		}
		instance.fadeIn = false;
		instance.loading = false;
		if (instance.shelterPositions != null)
		{
			List<Vector3> obj2 = instance.shelterPositions;
			Pool.FreeUnmanaged(ref obj2);
			instance.shelterPositions = obj2;
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
			throw new Exception("Trying to dispose RespawnInformation with ShouldPool set to false!");
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

	public void CopyTo(RespawnInformation instance)
	{
		if (spawnOptions != null)
		{
			instance.spawnOptions = Pool.Get<List<SpawnOptions>>();
			for (int i = 0; i < spawnOptions.Count; i++)
			{
				SpawnOptions item = spawnOptions[i].Copy();
				instance.spawnOptions.Add(item);
			}
		}
		else
		{
			instance.spawnOptions = null;
		}
		if (previousLife != null)
		{
			if (instance.previousLife == null)
			{
				instance.previousLife = previousLife.Copy();
			}
			else
			{
				previousLife.CopyTo(instance.previousLife);
			}
		}
		else
		{
			instance.previousLife = null;
		}
		instance.fadeIn = fadeIn;
		instance.loading = loading;
		if (shelterPositions != null)
		{
			instance.shelterPositions = Pool.Get<List<Vector3>>();
			for (int j = 0; j < shelterPositions.Count; j++)
			{
				Vector3 item2 = shelterPositions[j];
				instance.shelterPositions.Add(item2);
			}
		}
		else
		{
			instance.shelterPositions = null;
		}
	}

	public RespawnInformation Copy()
	{
		RespawnInformation respawnInformation = Pool.Get<RespawnInformation>();
		CopyTo(respawnInformation);
		return respawnInformation;
	}

	public static RespawnInformation Deserialize(BufferStream stream)
	{
		RespawnInformation respawnInformation = Pool.Get<RespawnInformation>();
		Deserialize(stream, respawnInformation, isDelta: false);
		return respawnInformation;
	}

	public static RespawnInformation DeserializeLengthDelimited(BufferStream stream)
	{
		RespawnInformation respawnInformation = Pool.Get<RespawnInformation>();
		DeserializeLengthDelimited(stream, respawnInformation, isDelta: false);
		return respawnInformation;
	}

	public static RespawnInformation DeserializeLength(BufferStream stream, int length)
	{
		RespawnInformation respawnInformation = Pool.Get<RespawnInformation>();
		DeserializeLength(stream, length, respawnInformation, isDelta: false);
		return respawnInformation;
	}

	public static RespawnInformation Deserialize(byte[] buffer)
	{
		RespawnInformation respawnInformation = Pool.Get<RespawnInformation>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, respawnInformation, isDelta: false);
		return respawnInformation;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, RespawnInformation previous)
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

	public static RespawnInformation Deserialize(BufferStream stream, RespawnInformation instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.spawnOptions == null)
			{
				instance.spawnOptions = Pool.Get<List<SpawnOptions>>();
			}
			if (instance.shelterPositions == null)
			{
				instance.shelterPositions = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.RespawnInformation");
			switch (num)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				stream.ConsumeRepeatedElement();
				instance.spawnOptions.Add(SpawnOptions.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				if (instance.previousLife == null)
				{
					instance.previousLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerLifeStory.DeserializeLengthDelimited(stream, instance.previousLife, isDelta);
				}
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				instance.fadeIn = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				instance.loading = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.shelterPositions.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RespawnInformation DeserializeLengthDelimited(BufferStream stream, RespawnInformation instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.spawnOptions == null)
			{
				instance.spawnOptions = Pool.Get<List<SpawnOptions>>();
			}
			if (instance.shelterPositions == null)
			{
				instance.shelterPositions = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.RespawnInformation");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				stream.ConsumeRepeatedElement();
				instance.spawnOptions.Add(SpawnOptions.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				if (instance.previousLife == null)
				{
					instance.previousLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerLifeStory.DeserializeLengthDelimited(stream, instance.previousLife, isDelta);
				}
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				instance.fadeIn = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				instance.loading = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.shelterPositions.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static RespawnInformation DeserializeLength(BufferStream stream, int length, RespawnInformation instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.spawnOptions == null)
			{
				instance.spawnOptions = Pool.Get<List<SpawnOptions>>();
			}
			if (instance.shelterPositions == null)
			{
				instance.shelterPositions = Pool.Get<List<Vector3>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.RespawnInformation");
			switch (num2)
			{
			case 10:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				stream.ConsumeRepeatedElement();
				instance.spawnOptions.Add(SpawnOptions.DeserializeLengthDelimited(stream));
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				if (instance.previousLife == null)
				{
					instance.previousLife = PlayerLifeStory.DeserializeLengthDelimited(stream);
				}
				else
				{
					PlayerLifeStory.DeserializeLengthDelimited(stream, instance.previousLife, isDelta);
				}
				continue;
			case 24:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				instance.fadeIn = ProtocolParser.ReadBool(stream);
				continue;
			case 32:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				instance.loading = ProtocolParser.ReadBool(stream);
				continue;
			case 42:
			{
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				stream.ConsumeRepeatedElement();
				Vector3 instance2 = default(Vector3);
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance2, isDelta);
				instance.shelterPositions.Add(instance2);
				continue;
			}
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.RespawnInformation");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, RespawnInformation instance, RespawnInformation previous)
	{
		if (instance.spawnOptions != null)
		{
			for (int i = 0; i < instance.spawnOptions.Count; i++)
			{
				SpawnOptions spawnOptions = instance.spawnOptions[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				SpawnOptions.SerializeDelta(stream, spawnOptions, spawnOptions);
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
		if (instance.previousLife != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PlayerLifeStory.SerializeDelta(stream, instance.previousLife, previous.previousLife);
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
		stream.WriteByte(24);
		ProtocolParser.WriteBool(stream, instance.fadeIn);
		stream.WriteByte(32);
		ProtocolParser.WriteBool(stream, instance.loading);
		if (instance.shelterPositions == null)
		{
			return;
		}
		for (int j = 0; j < instance.shelterPositions.Count; j++)
		{
			Vector3 vector = instance.shelterPositions[j];
			stream.WriteByte(42);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, vector, vector);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field shelterPositions (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
	}

	public static void Serialize(BufferStream stream, RespawnInformation instance)
	{
		if (instance.spawnOptions != null)
		{
			for (int i = 0; i < instance.spawnOptions.Count; i++)
			{
				SpawnOptions instance2 = instance.spawnOptions[i];
				stream.WriteByte(10);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				SpawnOptions.Serialize(stream, instance2);
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
		if (instance.previousLife != null)
		{
			stream.WriteByte(18);
			BufferStream.RangeHandle range2 = stream.GetRange(5);
			int position2 = stream.Position;
			PlayerLifeStory.Serialize(stream, instance.previousLife);
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
		if (instance.fadeIn)
		{
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.fadeIn);
		}
		if (instance.loading)
		{
			stream.WriteByte(32);
			ProtocolParser.WriteBool(stream, instance.loading);
		}
		if (instance.shelterPositions == null)
		{
			return;
		}
		for (int j = 0; j < instance.shelterPositions.Count; j++)
		{
			Vector3 instance3 = instance.shelterPositions[j];
			stream.WriteByte(42);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance3);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field shelterPositions (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (spawnOptions != null)
		{
			for (int i = 0; i < spawnOptions.Count; i++)
			{
				spawnOptions[i]?.InspectUids(action);
			}
		}
		previousLife?.InspectUids(action);
	}
}
