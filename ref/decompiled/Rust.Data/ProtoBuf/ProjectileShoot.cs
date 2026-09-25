using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace ProtoBuf;

public class ProjectileShoot : IDisposable, Pool.IPooled, IProto<ProjectileShoot>, IProto
{
	public class Projectile : IDisposable, Pool.IPooled, IProto<Projectile>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public int projectileID;

		[NonSerialized]
		public Vector3 startPos;

		[NonSerialized]
		public Vector3 startVel;

		[NonSerialized]
		public int seed;

		public static void ResetToPool(Projectile instance)
		{
			if (instance.ShouldPool)
			{
				instance.projectileID = 0;
				instance.startPos = default(Vector3);
				instance.startVel = default(Vector3);
				instance.seed = 0;
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
				throw new Exception("Trying to dispose Projectile with ShouldPool set to false!");
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

		public void CopyTo(Projectile instance)
		{
			instance.projectileID = projectileID;
			instance.startPos = startPos;
			instance.startVel = startVel;
			instance.seed = seed;
		}

		public Projectile Copy()
		{
			Projectile projectile = Pool.Get<Projectile>();
			CopyTo(projectile);
			return projectile;
		}

		public static Projectile Deserialize(BufferStream stream)
		{
			Projectile projectile = Pool.Get<Projectile>();
			Deserialize(stream, projectile, isDelta: false);
			return projectile;
		}

		public static Projectile DeserializeLengthDelimited(BufferStream stream)
		{
			Projectile projectile = Pool.Get<Projectile>();
			DeserializeLengthDelimited(stream, projectile, isDelta: false);
			return projectile;
		}

		public static Projectile DeserializeLength(BufferStream stream, int length)
		{
			Projectile projectile = Pool.Get<Projectile>();
			DeserializeLength(stream, length, projectile, isDelta: false);
			return projectile;
		}

		public static Projectile Deserialize(byte[] buffer)
		{
			Projectile projectile = Pool.Get<Projectile>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, projectile, isDelta: false);
			return projectile;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, Projectile previous)
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

		public static Projectile Deserialize(BufferStream stream, Projectile instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.ProjectileShoot.Projectile");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startPos, isDelta);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startVel, isDelta);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					instance.seed = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Projectile DeserializeLengthDelimited(BufferStream stream, Projectile instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ProjectileShoot.Projectile");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startPos, isDelta);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startVel, isDelta);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					instance.seed = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static Projectile DeserializeLength(BufferStream stream, int length, Projectile instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.ProjectileShoot.Projectile");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					instance.projectileID = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				case 18:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startPos, isDelta);
					continue;
				case 26:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					Vector3Serialized.DeserializeLengthDelimited(stream, ref instance.startVel, isDelta);
					continue;
				case 32:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					instance.seed = (int)ProtocolParser.ReadUInt64(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 4u:
					stream.ValidateFieldOrder(ref lastFieldId, 4u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot.Projectile");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, Projectile instance, Projectile previous)
		{
			if (instance.projectileID != previous.projectileID)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
			}
			if (instance.startPos != previous.startPos)
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.startPos, previous.startPos);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field startPos (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.startVel != previous.startVel)
			{
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector3Serialized.SerializeDelta(stream, instance.startVel, previous.startVel);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field startVel (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.seed != previous.seed)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.seed);
			}
		}

		public static void Serialize(BufferStream stream, Projectile instance)
		{
			if (instance.projectileID != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.projectileID);
			}
			if (instance.startPos != default(Vector3))
			{
				stream.WriteByte(18);
				BufferStream.RangeHandle range = stream.GetRange(1);
				int position = stream.Position;
				Vector3Serialized.Serialize(stream, instance.startPos);
				int num = stream.Position - position;
				if (num > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field startPos (UnityEngine.Vector3)");
				}
				Span<byte> span = range.GetSpan();
				ProtocolParser.WriteUInt32((uint)num, span, 0);
			}
			if (instance.startVel != default(Vector3))
			{
				stream.WriteByte(26);
				BufferStream.RangeHandle range2 = stream.GetRange(1);
				int position2 = stream.Position;
				Vector3Serialized.Serialize(stream, instance.startVel);
				int num2 = stream.Position - position2;
				if (num2 > 127)
				{
					throw new InvalidOperationException("Not enough space was reserved for the length prefix of field startVel (UnityEngine.Vector3)");
				}
				Span<byte> span2 = range2.GetSpan();
				ProtocolParser.WriteUInt32((uint)num2, span2, 0);
			}
			if (instance.seed != 0)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt64(stream, (ulong)instance.seed);
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
	public int ammoType;

	[NonSerialized]
	public List<Projectile> projectiles;

	public static void ResetToPool(ProjectileShoot instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		instance.ammoType = 0;
		if (instance.projectiles != null)
		{
			for (int i = 0; i < instance.projectiles.Count; i++)
			{
				if (instance.projectiles[i] != null)
				{
					instance.projectiles[i].ResetToPool();
					instance.projectiles[i] = null;
				}
			}
			List<Projectile> obj = instance.projectiles;
			Pool.Free(ref obj, freeElements: false);
			instance.projectiles = obj;
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
			throw new Exception("Trying to dispose ProjectileShoot with ShouldPool set to false!");
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

	public void CopyTo(ProjectileShoot instance)
	{
		instance.ammoType = ammoType;
		if (projectiles != null)
		{
			instance.projectiles = Pool.Get<List<Projectile>>();
			for (int i = 0; i < projectiles.Count; i++)
			{
				Projectile item = projectiles[i].Copy();
				instance.projectiles.Add(item);
			}
		}
		else
		{
			instance.projectiles = null;
		}
	}

	public ProjectileShoot Copy()
	{
		ProjectileShoot projectileShoot = Pool.Get<ProjectileShoot>();
		CopyTo(projectileShoot);
		return projectileShoot;
	}

	public static ProjectileShoot Deserialize(BufferStream stream)
	{
		ProjectileShoot projectileShoot = Pool.Get<ProjectileShoot>();
		Deserialize(stream, projectileShoot, isDelta: false);
		return projectileShoot;
	}

	public static ProjectileShoot DeserializeLengthDelimited(BufferStream stream)
	{
		ProjectileShoot projectileShoot = Pool.Get<ProjectileShoot>();
		DeserializeLengthDelimited(stream, projectileShoot, isDelta: false);
		return projectileShoot;
	}

	public static ProjectileShoot DeserializeLength(BufferStream stream, int length)
	{
		ProjectileShoot projectileShoot = Pool.Get<ProjectileShoot>();
		DeserializeLength(stream, length, projectileShoot, isDelta: false);
		return projectileShoot;
	}

	public static ProjectileShoot Deserialize(byte[] buffer)
	{
		ProjectileShoot projectileShoot = Pool.Get<ProjectileShoot>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, projectileShoot, isDelta: false);
		return projectileShoot;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, ProjectileShoot previous)
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

	public static ProjectileShoot Deserialize(BufferStream stream, ProjectileShoot instance, bool isDelta)
	{
		if (!isDelta && instance.projectiles == null)
		{
			instance.projectiles = Pool.Get<List<Projectile>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.ProjectileShoot");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot");
				instance.ammoType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				stream.ConsumeRepeatedElement();
				instance.projectiles.Add(Projectile.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ProjectileShoot DeserializeLengthDelimited(BufferStream stream, ProjectileShoot instance, bool isDelta)
	{
		if (!isDelta && instance.projectiles == null)
		{
			instance.projectiles = Pool.Get<List<Projectile>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ProjectileShoot");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot");
				instance.ammoType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				stream.ConsumeRepeatedElement();
				instance.projectiles.Add(Projectile.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static ProjectileShoot DeserializeLength(BufferStream stream, int length, ProjectileShoot instance, bool isDelta)
	{
		if (!isDelta && instance.projectiles == null)
		{
			instance.projectiles = Pool.Get<List<Projectile>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.ProjectileShoot");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot");
				instance.ammoType = (int)ProtocolParser.ReadUInt64(stream);
				continue;
			case 18:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				stream.ConsumeRepeatedElement();
				instance.projectiles.Add(Projectile.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.ProjectileShoot");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, ProjectileShoot instance, ProjectileShoot previous)
	{
		if (instance.ammoType != previous.ammoType)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoType);
		}
		if (instance.projectiles == null)
		{
			return;
		}
		for (int i = 0; i < instance.projectiles.Count; i++)
		{
			Projectile projectile = instance.projectiles[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Projectile.SerializeDelta(stream, projectile, projectile);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field projectiles (ProtoBuf.ProjectileShoot.Projectile)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, ProjectileShoot instance)
	{
		if (instance.ammoType != 0)
		{
			stream.WriteByte(8);
			ProtocolParser.WriteUInt64(stream, (ulong)instance.ammoType);
		}
		if (instance.projectiles == null)
		{
			return;
		}
		for (int i = 0; i < instance.projectiles.Count; i++)
		{
			Projectile instance2 = instance.projectiles[i];
			stream.WriteByte(18);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			Projectile.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field projectiles (ProtoBuf.ProjectileShoot.Projectile)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public void ToProto(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public void InspectUids(UidInspector<ulong> action)
	{
		if (projectiles != null)
		{
			for (int i = 0; i < projectiles.Count; i++)
			{
				projectiles[i]?.InspectUids(action);
			}
		}
	}
}
