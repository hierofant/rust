using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class ArcadeGame : IDisposable, Pool.IPooled, IProto<ArcadeGame>, IProto
{
	public class arcadeEnt : IDisposable, Pool.IPooled, IProto<arcadeEnt>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public uint id;

		[NonSerialized]
		public string name;

		[NonSerialized]
		public uint spriteID;

		[NonSerialized]
		public uint soundID;

		[NonSerialized]
		public bool visible;

		[NonSerialized]
		public Vector3 position;

		[NonSerialized]
		public Vector3 heading;

		[NonSerialized]
		public bool enabled;

		[NonSerialized]
		public Vector3 scale;

		[NonSerialized]
		public Vector3 colliderScale;

		[NonSerialized]
		public float alpha;

		[NonSerialized]
		public uint prefabID;

		[NonSerialized]
		public uint parentID;

		public static void ResetToPool(arcadeEnt instance)
		{
			if (instance.ShouldPool)
			{
				instance.id = 0u;
				instance.name = string.Empty;
				instance.spriteID = 0u;
				instance.soundID = 0u;
				instance.visible = false;
				instance.position = default(Vector3);
				instance.heading = default(Vector3);
				instance.enabled = false;
				instance.scale = default(Vector3);
				instance.colliderScale = default(Vector3);
				instance.alpha = 0f;
				instance.prefabID = 0u;
				instance.parentID = 0u;
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
				throw new Exception("Trying to dispose arcadeEnt with ShouldPool set to false!");
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

		public void CopyTo(arcadeEnt instance)
		{
			instance.id = id;
			instance.name = name;
			instance.spriteID = spriteID;
			instance.soundID = soundID;
			instance.visible = visible;
			instance.position = position;
			instance.heading = heading;
			instance.enabled = enabled;
			instance.scale = scale;
			instance.colliderScale = colliderScale;
			instance.alpha = alpha;
			instance.prefabID = prefabID;
			instance.parentID = parentID;
		}

		public arcadeEnt Copy()
		{
			arcadeEnt arcadeEnt = Pool.Get<arcadeEnt>();
			CopyTo(arcadeEnt);
			return arcadeEnt;
		}

		public static arcadeEnt Deserialize(BufferStream stream)
		{
			arcadeEnt arcadeEnt = Pool.Get<arcadeEnt>();
			Deserialize(stream, arcadeEnt, isDelta: false);
			return arcadeEnt;
		}

		public static arcadeEnt DeserializeLengthDelimited(BufferStream stream)
		{
			arcadeEnt arcadeEnt = Pool.Get<arcadeEnt>();
			DeserializeLengthDelimited(stream, arcadeEnt, isDelta: false);
			return arcadeEnt;
		}

		public static arcadeEnt DeserializeLength(BufferStream stream, int length)
		{
			arcadeEnt arcadeEnt = Pool.Get<arcadeEnt>();
			DeserializeLength(stream, length, arcadeEnt, isDelta: false);
			return arcadeEnt;
		}

		public static arcadeEnt Deserialize(byte[] buffer)
		{
			arcadeEnt arcadeEnt = Pool.Get<arcadeEnt>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, arcadeEnt, isDelta: false);
			return arcadeEnt;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, arcadeEnt previous)
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

		public static arcadeEnt Deserialize(BufferStream stream, arcadeEnt instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ArcadeGame.arcadeEnt");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.id = ProtocolParser.ReadUInt32(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.spriteID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.soundID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.visible = ProtocolParser.ReadBool(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.heading, isDelta);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.enabled = ProtocolParser.ReadBool(stream);
					continue;
				case 74:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.scale, isDelta);
					continue;
				case 82:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.colliderScale, isDelta);
					continue;
				case 93:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.alpha = ProtocolParser.ReadSingle(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.prefabID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 104:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.parentID = ProtocolParser.ReadUInt32(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static arcadeEnt DeserializeLengthDelimited(BufferStream stream, arcadeEnt instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ArcadeGame.arcadeEnt");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.id = ProtocolParser.ReadUInt32(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.spriteID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.soundID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.visible = ProtocolParser.ReadBool(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.heading, isDelta);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.enabled = ProtocolParser.ReadBool(stream);
					continue;
				case 74:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.scale, isDelta);
					continue;
				case 82:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.colliderScale, isDelta);
					continue;
				case 93:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.alpha = ProtocolParser.ReadSingle(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.prefabID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 104:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.parentID = ProtocolParser.ReadUInt32(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static arcadeEnt DeserializeLength(BufferStream stream, int length, arcadeEnt instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ArcadeGame.arcadeEnt");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.id = ProtocolParser.ReadUInt32(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.spriteID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.soundID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 40:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.visible = ProtocolParser.ReadBool(stream);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 58:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.heading, isDelta);
					continue;
				case 64:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.enabled = ProtocolParser.ReadBool(stream);
					continue;
				case 74:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.scale, isDelta);
					continue;
				case 82:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.colliderScale, isDelta);
					continue;
				case 93:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.alpha = ProtocolParser.ReadSingle(stream);
					continue;
				case 96:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.prefabID = ProtocolParser.ReadUInt32(stream);
					continue;
				case 104:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					instance.parentID = ProtocolParser.ReadUInt32(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 7u:
					stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 8u:
					stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 9u:
					stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 10u:
					stream.ValidateFieldOrder(ref lastFieldId, 10u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 11u:
					stream.ValidateFieldOrder(ref lastFieldId, 11u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 12u:
					stream.ValidateFieldOrder(ref lastFieldId, 12u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 13u:
					stream.ValidateFieldOrder(ref lastFieldId, 13u, fieldIsRepeated: false, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeGame.arcadeEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, arcadeEnt instance, arcadeEnt previous)
		{
			if (instance.id != previous.id)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, instance.id);
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
			if (instance.spriteID != previous.spriteID)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, instance.spriteID);
			}
			if (instance.soundID != previous.soundID)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, instance.soundID);
			}
			stream.WriteByte(40);
			ProtocolParser.WriteBool(stream, instance.visible);
			if (instance.position != previous.position)
			{
				stream.WriteByte(50);
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
			if (instance.heading != previous.heading)
			{
				stream.WriteByte(58);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int num3 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.heading, previous.heading);
				int num4 = stream.Position - num3;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field heading (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span2, 0);
			}
			stream.WriteByte(64);
			ProtocolParser.WriteBool(stream, instance.enabled);
			if (instance.scale != previous.scale)
			{
				stream.WriteByte(74);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int num5 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.scale, previous.scale);
				int num6 = stream.Position - num5;
				if (num6 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field scale (UnityEngine.Vector3)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num6, span3, 0);
			}
			if (instance.colliderScale != previous.colliderScale)
			{
				stream.WriteByte(82);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int num7 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.colliderScale, previous.colliderScale);
				int num8 = stream.Position - num7;
				if (num8 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field colliderScale (UnityEngine.Vector3)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num8, span4, 0);
			}
			if (instance.alpha != previous.alpha)
			{
				stream.WriteByte(93);
				ProtocolParser.WriteSingle(stream, instance.alpha);
			}
			if (instance.prefabID != previous.prefabID)
			{
				stream.WriteByte(96);
				ProtocolParser.WriteUInt32(stream, instance.prefabID);
			}
			if (instance.parentID != previous.parentID)
			{
				stream.WriteByte(104);
				ProtocolParser.WriteUInt32(stream, instance.parentID);
			}
		}

		public static void Serialize(BufferStream stream, arcadeEnt instance)
		{
			if (instance.id != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, instance.id);
			}
			if (instance.name == null)
			{
				throw new ArgumentNullException("name", "Required by proto specification.");
			}
			stream.WriteByte(18);
			ProtocolParser.WriteString(stream, instance.name);
			if (instance.spriteID != 0)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteUInt32(stream, instance.spriteID);
			}
			if (instance.soundID != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, instance.soundID);
			}
			if (instance.visible)
			{
				stream.WriteByte(40);
				ProtocolParser.WriteBool(stream, instance.visible);
			}
			if (instance.position != default(Vector3))
			{
				stream.WriteByte(50);
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
			if (instance.heading != default(Vector3))
			{
				stream.WriteByte(58);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int num3 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.heading);
				int num4 = stream.Position - num3;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field heading (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span2, 0);
			}
			if (instance.enabled)
			{
				stream.WriteByte(64);
				ProtocolParser.WriteBool(stream, instance.enabled);
			}
			if (instance.scale != default(Vector3))
			{
				stream.WriteByte(74);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int num5 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.scale);
				int num6 = stream.Position - num5;
				if (num6 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field scale (UnityEngine.Vector3)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num6, span3, 0);
			}
			if (instance.colliderScale != default(Vector3))
			{
				stream.WriteByte(82);
				BufferStream.RangeHandle range4 = stream.GetRange(1);
				int num7 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.colliderScale);
				int num8 = stream.Position - num7;
				if (num8 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field colliderScale (UnityEngine.Vector3)");
				}
				Span<byte> span4 = range4.GetSpan();
				ProtocolParser.WriteUInt32((uint)num8, span4, 0);
			}
			if (instance.alpha != 0f)
			{
				stream.WriteByte(93);
				ProtocolParser.WriteSingle(stream, instance.alpha);
			}
			if (instance.prefabID != 0)
			{
				stream.WriteByte(96);
				ProtocolParser.WriteUInt32(stream, instance.prefabID);
			}
			if (instance.parentID != 0)
			{
				stream.WriteByte(104);
				ProtocolParser.WriteUInt32(stream, instance.parentID);
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
	public List<arcadeEnt> arcadeEnts;

	public static void ResetToPool(ArcadeGame instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.arcadeEnts != null)
		{
			for (int i = 0; i < instance.arcadeEnts.Count; i++)
			{
				if (instance.arcadeEnts[i] != null)
				{
					instance.arcadeEnts[i].ResetToPool();
					instance.arcadeEnts[i] = null;
				}
			}
			List<arcadeEnt> obj = instance.arcadeEnts;
			Pool.Free(ref obj, freeElements: false);
			instance.arcadeEnts = obj;
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
			throw new Exception("Trying to dispose ArcadeGame with ShouldPool set to false!");
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

	public void CopyTo(ArcadeGame instance)
	{
		if (arcadeEnts != null)
		{
			instance.arcadeEnts = Pool.Get<List<arcadeEnt>>();
			for (int i = 0; i < arcadeEnts.Count; i++)
			{
				arcadeEnt item = arcadeEnts[i].Copy();
				instance.arcadeEnts.Add(item);
			}
		}
		else
		{
			instance.arcadeEnts = null;
		}
	}

	public ArcadeGame Copy()
	{
		ArcadeGame arcadeGame = Pool.Get<ArcadeGame>();
		CopyTo(arcadeGame);
		return arcadeGame;
	}

	public static ArcadeGame Deserialize(BufferStream stream)
	{
		ArcadeGame arcadeGame = Pool.Get<ArcadeGame>();
		Deserialize(stream, arcadeGame, isDelta: false);
		return arcadeGame;
	}

	public static ArcadeGame DeserializeLengthDelimited(BufferStream stream)
	{
		ArcadeGame arcadeGame = Pool.Get<ArcadeGame>();
		DeserializeLengthDelimited(stream, arcadeGame, isDelta: false);
		return arcadeGame;
	}

	public static ArcadeGame DeserializeLength(BufferStream stream, int length)
	{
		ArcadeGame arcadeGame = Pool.Get<ArcadeGame>();
		DeserializeLength(stream, length, arcadeGame, isDelta: false);
		return arcadeGame;
	}

	public static ArcadeGame Deserialize(byte[] buffer)
	{
		ArcadeGame arcadeGame = Pool.Get<ArcadeGame>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, arcadeGame, isDelta: false);
		return arcadeGame;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ArcadeGame previous)
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

	public static ArcadeGame Deserialize(BufferStream stream, ArcadeGame instance, bool isDelta)
	{
		if (!isDelta && instance.arcadeEnts == null)
		{
			instance.arcadeEnts = Pool.Get<List<arcadeEnt>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ArcadeGame");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
				if (instance.arcadeEnts.Count >= 64)
				{
					throw new ProtocolBufferException("Repeated field arcadeEnts exceeded max count of 64");
				}
				stream.ConsumeRepeatedElement();
				instance.arcadeEnts.Add(arcadeEnt.DeserializeLengthDelimited(stream));
			}
			else
			{
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
					ProtocolParser.SkipKey(stream, key);
				}
			}
		}
		return instance;
	}

	public static ArcadeGame DeserializeLengthDelimited(BufferStream stream, ArcadeGame instance, bool isDelta)
	{
		if (!isDelta && instance.arcadeEnts == null)
		{
			instance.arcadeEnts = Pool.Get<List<arcadeEnt>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ArcadeGame");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
				if (instance.arcadeEnts.Count >= 64)
				{
					throw new ProtocolBufferException("Repeated field arcadeEnts exceeded max count of 64");
				}
				stream.ConsumeRepeatedElement();
				instance.arcadeEnts.Add(arcadeEnt.DeserializeLengthDelimited(stream));
			}
			else
			{
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
					ProtocolParser.SkipKey(stream, key);
				}
			}
		}
		return instance;
	}

	public static ArcadeGame DeserializeLength(BufferStream stream, int length, ArcadeGame instance, bool isDelta)
	{
		if (!isDelta && instance.arcadeEnts == null)
		{
			instance.arcadeEnts = Pool.Get<List<arcadeEnt>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ArcadeGame");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
				if (instance.arcadeEnts.Count >= 64)
				{
					throw new ProtocolBufferException("Repeated field arcadeEnts exceeded max count of 64");
				}
				stream.ConsumeRepeatedElement();
				instance.arcadeEnts.Add(arcadeEnt.DeserializeLengthDelimited(stream));
			}
			else
			{
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				if (key.Field == 1)
				{
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
					ProtocolParser.SkipKey(stream, key);
				}
				else
				{
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ArcadeGame");
					ProtocolParser.SkipKey(stream, key);
				}
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ArcadeGame instance, ArcadeGame previous)
	{
		if (instance.arcadeEnts == null)
		{
			return;
		}
		if (instance.arcadeEnts.Count > 64)
		{
			throw new InvalidOperationException("Repeated field arcadeEnts exceeded max count of 64");
		}
		for (int i = 0; i < instance.arcadeEnts.Count; i++)
		{
			arcadeEnt arcadeEnt = instance.arcadeEnts[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			arcadeEnt.SerializeDelta(stream, arcadeEnt, arcadeEnt);
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

	public static void Serialize(BufferStream stream, ArcadeGame instance)
	{
		if (instance.arcadeEnts == null)
		{
			return;
		}
		if (instance.arcadeEnts.Count > 64)
		{
			throw new InvalidOperationException("Repeated field arcadeEnts exceeded max count of 64");
		}
		for (int i = 0; i < instance.arcadeEnts.Count; i++)
		{
			arcadeEnt instance2 = instance.arcadeEnts[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(5);
			int position = stream.Position;
			arcadeEnt.Serialize(stream, instance2);
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
		if (arcadeEnts != null)
		{
			for (int i = 0; i < arcadeEnts.Count; i++)
			{
				arcadeEnts[i]?.InspectUids(action);
			}
		}
	}
}
