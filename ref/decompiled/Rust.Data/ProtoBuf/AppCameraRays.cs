using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class AppCameraRays : IDisposable, Pool.IPooled, IProto<AppCameraRays>, IProto
{
	public class Entity : IDisposable, Pool.IPooled, IProto<Entity>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public NetworkableId entityId;

		[NonSerialized]
		public EntityType type;

		[NonSerialized]
		public Vector3 position;

		[NonSerialized]
		public Vector3 rotation;

		[NonSerialized]
		public Vector3 size;

		[NonSerialized]
		public string name;

		public static void ResetToPool(Entity instance)
		{
			if (instance.ShouldPool)
			{
				instance.entityId = default(NetworkableId);
				instance.type = (EntityType)0;
				instance.position = default(Vector3);
				instance.rotation = default(Vector3);
				instance.size = default(Vector3);
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
				throw new Exception("Trying to dispose Entity with ShouldPool set to false!");
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

		public void CopyTo(Entity instance)
		{
			instance.entityId = entityId;
			instance.type = type;
			instance.position = position;
			instance.rotation = rotation;
			instance.size = size;
			instance.name = name;
		}

		public Entity Copy()
		{
			Entity entity = Pool.Get<Entity>();
			CopyTo(entity);
			return entity;
		}

		public static Entity Deserialize(BufferStream stream)
		{
			Entity entity = Pool.Get<Entity>();
			Deserialize(stream, entity, isDelta: false);
			return entity;
		}

		public static Entity DeserializeLengthDelimited(BufferStream stream)
		{
			Entity entity = Pool.Get<Entity>();
			DeserializeLengthDelimited(stream, entity, isDelta: false);
			return entity;
		}

		public static Entity DeserializeLength(BufferStream stream, int length)
		{
			Entity entity = Pool.Get<Entity>();
			DeserializeLength(stream, length, entity, isDelta: false);
			return entity;
		}

		public static Entity Deserialize(byte[] buffer)
		{
			Entity entity = Pool.Get<Entity>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, entity, isDelta: false);
			return entity;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Entity previous)
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

		public static Entity Deserialize(BufferStream stream, Entity instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.AppCameraRays.Entity");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.type = (EntityType)ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.size, isDelta);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Entity DeserializeLengthDelimited(BufferStream stream, Entity instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.AppCameraRays.Entity");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.type = (EntityType)ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.size, isDelta);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Entity DeserializeLength(BufferStream stream, int length, Entity instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.AppCameraRays.Entity");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.entityId = new NetworkableId(ProtocolParser.ReadUInt64(stream));
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.type = (EntityType)ProtocolParser.ReadUInt64(stream);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.position, isDelta);
					continue;
				case 34:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.rotation, isDelta);
					continue;
				case 42:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.size, isDelta);
					continue;
				case 50:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					instance.name = ProtocolParser.ReadString(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 5u:
					stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 6u:
					stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppCameraRays.Entity");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Entity instance, Entity previous)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			if (instance.position != previous.position)
			{
				stream.WriteByte(26);
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
			if (instance.rotation != previous.rotation)
			{
				stream.WriteByte(34);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int num3 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.rotation, previous.rotation);
				int num4 = stream.Position - num3;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rotation (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span2, 0);
			}
			if (instance.size != previous.size)
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int num5 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.size, previous.size);
				int num6 = stream.Position - num5;
				if (num6 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field size (UnityEngine.Vector3)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num6, span3, 0);
			}
			if (instance.name != null && instance.name != previous.name)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteString(stream, instance.name);
			}
		}

		public static void Serialize(BufferStream stream, Entity instance)
		{
			if (instance.entityId != default(NetworkableId))
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, instance.entityId.Value);
			}
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.type);
			if (instance.position != default(Vector3))
			{
				stream.WriteByte(26);
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
			if (instance.rotation != default(Vector3))
			{
				stream.WriteByte(34);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int num3 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.rotation);
				int num4 = stream.Position - num3;
				if (num4 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field rotation (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num4, span2, 0);
			}
			if (instance.size != default(Vector3))
			{
				stream.WriteByte(42);
				BufferStream.RangeHandle range3 = stream.GetRange(1);
				int num5 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.size);
				int num6 = stream.Position - num5;
				if (num6 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field size (UnityEngine.Vector3)");
				}
				Span<byte> span3 = range3.GetSpan();
				ProtocolParser.WriteUInt32((uint)num6, span3, 0);
			}
			if (instance.name != null)
			{
				stream.WriteByte(50);
				ProtocolParser.WriteString(stream, instance.name);
			}
		}

		public void ToProto(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public void InspectUids(UidInspector<ulong> action)
		{
			action(UidType.NetworkableId, ref entityId.Value);
		}
	}

	public enum EntityType
	{
		Tree = 1,
		Player
	}

	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public float verticalFov;

	[NonSerialized]
	public int sampleOffset;

	[NonSerialized]
	public ArraySegment<byte> rayData;

	[NonSerialized]
	public float distance;

	[NonSerialized]
	public List<Entity> entities;

	[NonSerialized]
	public float timeOfDay;

	[NonSerialized]
	public Vector3 cameraPosition;

	[NonSerialized]
	public Vector3 cameraRotation;

	[NonSerialized]
	public Vector3 sampleRotation;

	public static void ResetToPool(AppCameraRays instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.verticalFov = 0f;
		instance.sampleOffset = 0;
		if (instance.rayData.Array != null)
		{
			BufferStream.Shared.ArrayPool.Return(instance.rayData.Array);
		}
		instance.rayData = default(ArraySegment<byte>);
		instance.distance = 0f;
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				if (instance.entities[i] != null)
				{
					instance.entities[i].ResetToPool();
					instance.entities[i] = null;
				}
			}
			List<Entity> obj = instance.entities;
			Pool.Free(ref obj, freeElements: false);
			instance.entities = obj;
		}
		instance.timeOfDay = 0f;
		instance.cameraPosition = default(Vector3);
		instance.cameraRotation = default(Vector3);
		instance.sampleRotation = default(Vector3);
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
			throw new Exception("Trying to dispose AppCameraRays with ShouldPool set to false!");
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

	public void CopyTo(AppCameraRays instance)
	{
		instance.verticalFov = verticalFov;
		instance.sampleOffset = sampleOffset;
		if (rayData.Array == null)
		{
			instance.rayData = default(ArraySegment<byte>);
		}
		else
		{
			byte[] array = BufferStream.Shared.ArrayPool.Rent(rayData.Count);
			Array.Copy(rayData.Array, 0, array, 0, rayData.Count);
			instance.rayData = new ArraySegment<byte>(array, 0, rayData.Count);
		}
		instance.distance = distance;
		if (entities != null)
		{
			instance.entities = Pool.Get<List<Entity>>();
			for (int i = 0; i < entities.Count; i++)
			{
				Entity item = entities[i].Copy();
				instance.entities.Add(item);
			}
		}
		else
		{
			instance.entities = null;
		}
		instance.timeOfDay = timeOfDay;
		instance.cameraPosition = cameraPosition;
		instance.cameraRotation = cameraRotation;
		instance.sampleRotation = sampleRotation;
	}

	public AppCameraRays Copy()
	{
		AppCameraRays appCameraRays = Pool.Get<AppCameraRays>();
		CopyTo(appCameraRays);
		return appCameraRays;
	}

	public static AppCameraRays Deserialize(BufferStream stream)
	{
		AppCameraRays appCameraRays = Pool.Get<AppCameraRays>();
		Deserialize(stream, appCameraRays, isDelta: false);
		return appCameraRays;
	}

	public static AppCameraRays DeserializeLengthDelimited(BufferStream stream)
	{
		AppCameraRays appCameraRays = Pool.Get<AppCameraRays>();
		DeserializeLengthDelimited(stream, appCameraRays, isDelta: false);
		return appCameraRays;
	}

	public static AppCameraRays DeserializeLength(BufferStream stream, int length)
	{
		AppCameraRays appCameraRays = Pool.Get<AppCameraRays>();
		DeserializeLength(stream, length, appCameraRays, isDelta: false);
		return appCameraRays;
	}

	public static AppCameraRays Deserialize(byte[] buffer)
	{
		AppCameraRays appCameraRays = Pool.Get<AppCameraRays>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, appCameraRays, isDelta: false);
		return appCameraRays;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, AppCameraRays previous)
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

	public static AppCameraRays Deserialize(BufferStream stream, AppCameraRays instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.verticalFov = 0f;
			instance.sampleOffset = 0;
			instance.distance = 0f;
			if (instance.entities == null)
			{
				instance.entities = Pool.Get<List<Entity>>();
			}
			instance.timeOfDay = 1f;
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.AppCameraRays");
			switch (num)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.verticalFov = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.sampleOffset = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.rayData = ProtocolParser.ReadPooledBytes(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.distance = ProtocolParser.ReadSingle(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.timeOfDay = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.cameraPosition, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.cameraRotation, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.sampleRotation, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppCameraRays DeserializeLengthDelimited(BufferStream stream, AppCameraRays instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.verticalFov = 0f;
			instance.sampleOffset = 0;
			instance.distance = 0f;
			if (instance.entities == null)
			{
				instance.entities = Pool.Get<List<Entity>>();
			}
			instance.timeOfDay = 1f;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppCameraRays");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.verticalFov = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.sampleOffset = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.rayData = ProtocolParser.ReadPooledBytes(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.distance = ProtocolParser.ReadSingle(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.timeOfDay = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.cameraPosition, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.cameraRotation, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.sampleRotation, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static AppCameraRays DeserializeLength(BufferStream stream, int length, AppCameraRays instance, bool isDelta)
	{
		if (!isDelta)
		{
			instance.verticalFov = 0f;
			instance.sampleOffset = 0;
			instance.distance = 0f;
			if (instance.entities == null)
			{
				instance.entities = Pool.Get<List<Entity>>();
			}
			instance.timeOfDay = 1f;
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
			stream.ConsumeFieldOperation("ProtoBuf.AppCameraRays");
			switch (num2)
			{
			case 13:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.verticalFov = ProtocolParser.ReadSingle(stream);
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.sampleOffset = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 26:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.rayData = ProtocolParser.ReadPooledBytes(stream);
				continue;
			case 37:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.distance = ProtocolParser.ReadSingle(stream);
				continue;
			case 42:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				stream.ConsumeRepeatedElement();
				instance.entities.Add(Entity.DeserializeLengthDelimited(stream));
				continue;
			case 53:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				instance.timeOfDay = ProtocolParser.ReadSingle(stream);
				continue;
			case 58:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.cameraPosition, isDelta);
				continue;
			case 66:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.cameraRotation, isDelta);
				continue;
			case 74:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.sampleRotation, isDelta);
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 3u:
				stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 4u:
				stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 5u:
				stream.ValidateFieldOrder(ref lastFieldId, 5u, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 6u:
				stream.ValidateFieldOrder(ref lastFieldId, 6u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 7u:
				stream.ValidateFieldOrder(ref lastFieldId, 7u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 8u:
				stream.ValidateFieldOrder(ref lastFieldId, 8u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 9u:
				stream.ValidateFieldOrder(ref lastFieldId, 9u, fieldIsRepeated: false, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.AppCameraRays");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, AppCameraRays instance, AppCameraRays previous)
	{
		if (instance.verticalFov != previous.verticalFov)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.verticalFov);
		}
		if (instance.sampleOffset != previous.sampleOffset)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.sampleOffset);
		}
		if (instance.rayData.Array == null)
		{
			throw new ArgumentNullException("rayData", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WritePooledBytes(stream, instance.rayData);
		if (instance.distance != previous.distance)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.distance);
		}
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity entity = instance.entities[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Entity.SerializeDelta(stream, entity, entity);
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
		if (instance.timeOfDay != previous.timeOfDay)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.timeOfDay);
		}
		if (instance.cameraPosition != previous.cameraPosition)
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.cameraPosition, previous.cameraPosition);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraPosition (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.cameraRotation != previous.cameraRotation)
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.cameraRotation, previous.cameraRotation);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraRotation (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.sampleRotation != previous.sampleRotation)
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.SerializeDelta(stream, instance.sampleRotation, previous.sampleRotation);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sampleRotation (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
	}

	public static void Serialize(BufferStream stream, AppCameraRays instance)
	{
		if (instance.verticalFov != 0f)
		{
			stream.WriteByte(13);
			ProtocolParser.WriteSingle(stream, instance.verticalFov);
		}
		if (instance.sampleOffset != 0)
		{
			stream.WriteByte(16);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.sampleOffset);
		}
		if (instance.rayData.Array == null)
		{
			throw new ArgumentNullException("rayData", "Required by proto specification.");
		}
		stream.WriteByte(26);
		ProtocolParser.WritePooledBytes(stream, instance.rayData);
		if (instance.distance != 0f)
		{
			stream.WriteByte(37);
			ProtocolParser.WriteSingle(stream, instance.distance);
		}
		if (instance.entities != null)
		{
			for (int i = 0; i < instance.entities.Count; i++)
			{
				Entity instance2 = instance.entities[i];
				stream.WriteByte(42);
				BufferStream.RangeHandle range = stream.GetRange(5);
				int position = stream.Position;
				Entity.Serialize(stream, instance2);
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
		if (instance.timeOfDay != 1f)
		{
			stream.WriteByte(53);
			ProtocolParser.WriteSingle(stream, instance.timeOfDay);
		}
		if (instance.cameraPosition != default(Vector3))
		{
			stream.WriteByte(58);
			BufferStream.RangeHandle range2 = stream.GetRange(1);
			int position2 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.cameraPosition);
			int num2 = stream.Position - position2;
			if (num2 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraPosition (UnityEngine.Vector3)");
			}
			Span<byte> span2 = range2.GetSpan();
			ProtocolParser.WriteUInt32((uint)num2, span2, 0);
		}
		if (instance.cameraRotation != default(Vector3))
		{
			stream.WriteByte(66);
			BufferStream.RangeHandle range3 = stream.GetRange(1);
			int position3 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.cameraRotation);
			int num3 = stream.Position - position3;
			if (num3 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field cameraRotation (UnityEngine.Vector3)");
			}
			Span<byte> span3 = range3.GetSpan();
			ProtocolParser.WriteUInt32((uint)num3, span3, 0);
		}
		if (instance.sampleRotation != default(Vector3))
		{
			stream.WriteByte(74);
			BufferStream.RangeHandle range4 = stream.GetRange(1);
			int position4 = stream.Position;
			Vector3Serialized.Serialize(stream, instance.sampleRotation);
			int num4 = stream.Position - position4;
			if (num4 > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field sampleRotation (UnityEngine.Vector3)");
			}
			Span<byte> span4 = range4.GetSpan();
			ProtocolParser.WriteUInt32((uint)num4, span4, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (entities != null)
		{
			for (int i = 0; i < entities.Count; i++)
			{
				entities[i]?.InspectUids(action);
			}
		}
	}
}
