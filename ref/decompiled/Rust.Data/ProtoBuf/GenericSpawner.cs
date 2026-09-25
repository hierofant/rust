using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class GenericSpawner : IDisposable, Pool.IPooled, IProto<GenericSpawner>, IProto
{
	public class SpawnedEnt : IDisposable, Pool.IPooled, IProto<SpawnedEnt>, IProto
	{
		public bool ShouldPool = true;

		private bool _disposed;

		[NonSerialized]
		public uint uid;

		[NonSerialized]
		public uint spawnPointIndex;

		[NonSerialized]
		public bool mobile;

		public static void ResetToPool(SpawnedEnt instance)
		{
			if (instance.ShouldPool)
			{
				instance.uid = 0u;
				instance.spawnPointIndex = 0u;
				instance.mobile = false;
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
				throw new Exception("Trying to dispose SpawnedEnt with ShouldPool set to false!");
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

		public void CopyTo(SpawnedEnt instance)
		{
			instance.uid = uid;
			instance.spawnPointIndex = spawnPointIndex;
			instance.mobile = mobile;
		}

		public SpawnedEnt Copy()
		{
			SpawnedEnt spawnedEnt = Pool.Get<SpawnedEnt>();
			CopyTo(spawnedEnt);
			return spawnedEnt;
		}

		public static SpawnedEnt Deserialize(BufferStream stream)
		{
			SpawnedEnt spawnedEnt = Pool.Get<SpawnedEnt>();
			Deserialize(stream, spawnedEnt, isDelta: false);
			return spawnedEnt;
		}

		public static SpawnedEnt DeserializeLengthDelimited(BufferStream stream)
		{
			SpawnedEnt spawnedEnt = Pool.Get<SpawnedEnt>();
			DeserializeLengthDelimited(stream, spawnedEnt, isDelta: false);
			return spawnedEnt;
		}

		public static SpawnedEnt DeserializeLength(BufferStream stream, int length)
		{
			SpawnedEnt spawnedEnt = Pool.Get<SpawnedEnt>();
			DeserializeLength(stream, length, spawnedEnt, isDelta: false);
			return spawnedEnt;
		}

		public static SpawnedEnt Deserialize(byte[] buffer)
		{
			SpawnedEnt spawnedEnt = Pool.Get<SpawnedEnt>();
			using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
			Deserialize(stream, spawnedEnt, isDelta: false);
			return spawnedEnt;
		}

		public void FromProto(BufferStream stream, bool isDelta = false)
		{
			Deserialize(stream, this, isDelta);
		}

		public virtual void WriteToStream(BufferStream stream)
		{
			Serialize(stream, this);
		}

		public virtual void WriteToStreamDelta(BufferStream stream, SpawnedEnt previous)
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

		public static SpawnedEnt Deserialize(BufferStream stream, SpawnedEnt instance, bool isDelta)
		{
			uint lastFieldId = 0u;
			while (true)
			{
				int num = stream.ReadByte();
				if (num == -1 || num == 0)
				{
					break;
				}
				stream.ConsumeFieldOperation("ProtoBuf.GenericSpawner.SpawnedEnt");
				switch (num)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.uid = ProtocolParser.ReadUInt32(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.spawnPointIndex = ProtocolParser.ReadUInt32(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.mobile = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SpawnedEnt DeserializeLengthDelimited(BufferStream stream, SpawnedEnt instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.GenericSpawner.SpawnedEnt");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.uid = ProtocolParser.ReadUInt32(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.spawnPointIndex = ProtocolParser.ReadUInt32(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.mobile = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static SpawnedEnt DeserializeLength(BufferStream stream, int length, SpawnedEnt instance, bool isDelta)
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
				stream.ConsumeFieldOperation("ProtoBuf.GenericSpawner.SpawnedEnt");
				switch (num2)
				{
				case 8:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.uid = ProtocolParser.ReadUInt32(stream);
					continue;
				case 16:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.spawnPointIndex = ProtocolParser.ReadUInt32(stream);
					continue;
				case 24:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					instance.mobile = ProtocolParser.ReadBool(stream);
					continue;
				}
				Key key = ProtocolParser.ReadKey((byte)num2, stream);
				switch (key.Field)
				{
				case 1u:
					stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 2u:
					stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				case 3u:
					stream.ValidateFieldOrder(ref lastFieldId, 3u, fieldIsRepeated: false, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				default:
					stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GenericSpawner.SpawnedEnt");
					ProtocolParser.SkipKey(stream, key);
					break;
				}
			}
			return instance;
		}

		public static void SerializeDelta(BufferStream stream, SpawnedEnt instance, SpawnedEnt previous)
		{
			if (instance.uid != previous.uid)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, instance.uid);
			}
			if (instance.spawnPointIndex != previous.spawnPointIndex)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, instance.spawnPointIndex);
			}
			stream.WriteByte(24);
			ProtocolParser.WriteBool(stream, instance.mobile);
		}

		public static void Serialize(BufferStream stream, SpawnedEnt instance)
		{
			if (instance.uid != 0)
			{
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, instance.uid);
			}
			if (instance.spawnPointIndex != 0)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, instance.spawnPointIndex);
			}
			if (instance.mobile)
			{
				stream.WriteByte(24);
				ProtocolParser.WriteBool(stream, instance.mobile);
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
	public List<SpawnedEnt> ents;

	public static void ResetToPool(GenericSpawner instance)
	{
		if (!instance.ShouldPool)
		{
			return;
		}
		if (instance.ents != null)
		{
			for (int i = 0; i < instance.ents.Count; i++)
			{
				if (instance.ents[i] != null)
				{
					instance.ents[i].ResetToPool();
					instance.ents[i] = null;
				}
			}
			List<SpawnedEnt> obj = instance.ents;
			Pool.Free(ref obj, freeElements: false);
			instance.ents = obj;
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
			throw new Exception("Trying to dispose GenericSpawner with ShouldPool set to false!");
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

	public void CopyTo(GenericSpawner instance)
	{
		if (ents != null)
		{
			instance.ents = Pool.Get<List<SpawnedEnt>>();
			for (int i = 0; i < ents.Count; i++)
			{
				SpawnedEnt item = ents[i].Copy();
				instance.ents.Add(item);
			}
		}
		else
		{
			instance.ents = null;
		}
	}

	public GenericSpawner Copy()
	{
		GenericSpawner genericSpawner = Pool.Get<GenericSpawner>();
		CopyTo(genericSpawner);
		return genericSpawner;
	}

	public static GenericSpawner Deserialize(BufferStream stream)
	{
		GenericSpawner genericSpawner = Pool.Get<GenericSpawner>();
		Deserialize(stream, genericSpawner, isDelta: false);
		return genericSpawner;
	}

	public static GenericSpawner DeserializeLengthDelimited(BufferStream stream)
	{
		GenericSpawner genericSpawner = Pool.Get<GenericSpawner>();
		DeserializeLengthDelimited(stream, genericSpawner, isDelta: false);
		return genericSpawner;
	}

	public static GenericSpawner DeserializeLength(BufferStream stream, int length)
	{
		GenericSpawner genericSpawner = Pool.Get<GenericSpawner>();
		DeserializeLength(stream, length, genericSpawner, isDelta: false);
		return genericSpawner;
	}

	public static GenericSpawner Deserialize(byte[] buffer)
	{
		GenericSpawner genericSpawner = Pool.Get<GenericSpawner>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, genericSpawner, isDelta: false);
		return genericSpawner;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, GenericSpawner previous)
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

	public static GenericSpawner Deserialize(BufferStream stream, GenericSpawner instance, bool isDelta)
	{
		if (!isDelta && instance.ents == null)
		{
			instance.ents = Pool.Get<List<SpawnedEnt>>();
		}
		uint lastFieldId = 0u;
		while (true)
		{
			int num = stream.ReadByte();
			if (num == -1 || num == 0)
			{
				break;
			}
			stream.ConsumeFieldOperation("ProtoBuf.GenericSpawner");
			if (num == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				stream.ConsumeRepeatedElement();
				instance.ents.Add(SpawnedEnt.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static GenericSpawner DeserializeLengthDelimited(BufferStream stream, GenericSpawner instance, bool isDelta)
	{
		if (!isDelta && instance.ents == null)
		{
			instance.ents = Pool.Get<List<SpawnedEnt>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.GenericSpawner");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				stream.ConsumeRepeatedElement();
				instance.ents.Add(SpawnedEnt.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static GenericSpawner DeserializeLength(BufferStream stream, int length, GenericSpawner instance, bool isDelta)
	{
		if (!isDelta && instance.ents == null)
		{
			instance.ents = Pool.Get<List<SpawnedEnt>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.GenericSpawner");
			if (num2 == 10)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				stream.ConsumeRepeatedElement();
				instance.ents.Add(SpawnedEnt.DeserializeLengthDelimited(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			if (key.Field == 1)
			{
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				ProtocolParser.SkipKey(stream, key);
			}
			else
			{
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.GenericSpawner");
				ProtocolParser.SkipKey(stream, key);
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, GenericSpawner instance, GenericSpawner previous)
	{
		if (instance.ents == null)
		{
			return;
		}
		for (int i = 0; i < instance.ents.Count; i++)
		{
			SpawnedEnt spawnedEnt = instance.ents[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			SpawnedEnt.SerializeDelta(stream, spawnedEnt, spawnedEnt);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ents (ProtoBuf.GenericSpawner.SpawnedEnt)");
			}
			Span<byte> span = range.GetSpan();
			ProtocolParser.WriteUInt32((uint)num, span, 0);
		}
	}

	public static void Serialize(BufferStream stream, GenericSpawner instance)
	{
		if (instance.ents == null)
		{
			return;
		}
		for (int i = 0; i < instance.ents.Count; i++)
		{
			SpawnedEnt instance2 = instance.ents[i];
			stream.WriteByte(10);
			BufferStream.RangeHandle range = stream.GetRange(1);
			int position = stream.Position;
			SpawnedEnt.Serialize(stream, instance2);
			int num = stream.Position - position;
			if (num > 127)
			{
				throw new InvalidOperationException("Not enough space was reserved for the length prefix of field ents (ProtoBuf.GenericSpawner.SpawnedEnt)");
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
		if (ents != null)
		{
			for (int i = 0; i < ents.Count; i++)
			{
				ents[i]?.InspectUids(action);
			}
		}
	}
}
