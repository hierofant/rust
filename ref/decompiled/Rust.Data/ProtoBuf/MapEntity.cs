using System;
using System.Collections.Generic;
using System.IO;
using Facepunch;
using SilentOrbit.ProtocolBuffers;

namespace ProtoBuf;

public class MapEntity : IDisposable, Pool.IPooled, IProto<MapEntity>, IProto
{
	public bool ShouldPool = true;

	private bool _disposed;

	[NonSerialized]
	public List<uint> fogImages;

	[NonSerialized]
	public List<uint> paintImages;

	public static void ResetToPool(MapEntity instance)
	{
		if (instance.ShouldPool)
		{
			if (instance.fogImages != null)
			{
				List<uint> obj = instance.fogImages;
				Pool.FreeUnmanaged(ref obj);
				instance.fogImages = obj;
			}
			if (instance.paintImages != null)
			{
				List<uint> obj2 = instance.paintImages;
				Pool.FreeUnmanaged(ref obj2);
				instance.paintImages = obj2;
			}
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
			throw new Exception("Trying to dispose MapEntity with ShouldPool set to false!");
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

	public void CopyTo(MapEntity instance)
	{
		if (fogImages != null)
		{
			instance.fogImages = Pool.Get<List<uint>>();
			for (int i = 0; i < fogImages.Count; i++)
			{
				uint item = fogImages[i];
				instance.fogImages.Add(item);
			}
		}
		else
		{
			instance.fogImages = null;
		}
		if (paintImages != null)
		{
			instance.paintImages = Pool.Get<List<uint>>();
			for (int j = 0; j < paintImages.Count; j++)
			{
				uint item2 = paintImages[j];
				instance.paintImages.Add(item2);
			}
		}
		else
		{
			instance.paintImages = null;
		}
	}

	public MapEntity Copy()
	{
		MapEntity mapEntity = Pool.Get<MapEntity>();
		CopyTo(mapEntity);
		return mapEntity;
	}

	public static MapEntity Deserialize(BufferStream stream)
	{
		MapEntity mapEntity = Pool.Get<MapEntity>();
		Deserialize(stream, mapEntity, isDelta: false);
		return mapEntity;
	}

	public static MapEntity DeserializeLengthDelimited(BufferStream stream)
	{
		MapEntity mapEntity = Pool.Get<MapEntity>();
		DeserializeLengthDelimited(stream, mapEntity, isDelta: false);
		return mapEntity;
	}

	public static MapEntity DeserializeLength(BufferStream stream, int length)
	{
		MapEntity mapEntity = Pool.Get<MapEntity>();
		DeserializeLength(stream, length, mapEntity, isDelta: false);
		return mapEntity;
	}

	public static MapEntity Deserialize(byte[] buffer)
	{
		MapEntity mapEntity = Pool.Get<MapEntity>();
		using BufferStream stream = Pool.Get<BufferStream>().Initialize(buffer);
		Deserialize(stream, mapEntity, isDelta: false);
		return mapEntity;
	}

	public void FromProto(BufferStream stream, bool isDelta = false)
	{
		Deserialize(stream, this, isDelta);
	}

	public virtual void WriteToStream(BufferStream stream)
	{
		Serialize(stream, this);
	}

	public virtual void WriteToStreamDelta(BufferStream stream, MapEntity previous)
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

	public static MapEntity Deserialize(BufferStream stream, MapEntity instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.fogImages == null)
			{
				instance.fogImages = Pool.Get<List<uint>>();
			}
			if (instance.paintImages == null)
			{
				instance.paintImages = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MapEntity");
			switch (num)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				stream.ConsumeRepeatedElement();
				instance.fogImages.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				stream.ConsumeRepeatedElement();
				instance.paintImages.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MapEntity DeserializeLengthDelimited(BufferStream stream, MapEntity instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.fogImages == null)
			{
				instance.fogImages = Pool.Get<List<uint>>();
			}
			if (instance.paintImages == null)
			{
				instance.paintImages = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MapEntity");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				stream.ConsumeRepeatedElement();
				instance.fogImages.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				stream.ConsumeRepeatedElement();
				instance.paintImages.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static MapEntity DeserializeLength(BufferStream stream, int length, MapEntity instance, bool isDelta)
	{
		if (!isDelta)
		{
			if (instance.fogImages == null)
			{
				instance.fogImages = Pool.Get<List<uint>>();
			}
			if (instance.paintImages == null)
			{
				instance.paintImages = Pool.Get<List<uint>>();
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
			stream.ConsumeFieldOperation("ProtoBuf.MapEntity");
			switch (num2)
			{
			case 8:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				stream.ConsumeRepeatedElement();
				instance.fogImages.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			case 16:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				stream.ConsumeRepeatedElement();
				instance.paintImages.Add(ProtocolParser.ReadUInt32(stream));
				continue;
			}
			Key key = ProtocolParser.ReadKey((byte)num2, stream);
			switch (key.Field)
			{
			case 1u:
				stream.ValidateFieldOrder(ref lastFieldId, 1u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			case 2u:
				stream.ValidateFieldOrder(ref lastFieldId, 2u, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			default:
				stream.ValidateFieldOrder(ref lastFieldId, key.Field, fieldIsRepeated: true, "ProtoBuf.MapEntity");
				ProtocolParser.SkipKey(stream, key);
				break;
			}
		}
		return instance;
	}

	public static void SerializeDelta(BufferStream stream, MapEntity instance, MapEntity previous)
	{
		if (instance.fogImages != null)
		{
			for (int i = 0; i < instance.fogImages.Count; i++)
			{
				uint val = instance.fogImages[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.paintImages != null)
		{
			for (int j = 0; j < instance.paintImages.Count; j++)
			{
				uint val2 = instance.paintImages[j];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, val2);
			}
		}
	}

	public static void Serialize(BufferStream stream, MapEntity instance)
	{
		if (instance.fogImages != null)
		{
			for (int i = 0; i < instance.fogImages.Count; i++)
			{
				uint val = instance.fogImages[i];
				stream.WriteByte(8);
				ProtocolParser.WriteUInt32(stream, val);
			}
		}
		if (instance.paintImages != null)
		{
			for (int j = 0; j < instance.paintImages.Count; j++)
			{
				uint val2 = instance.paintImages[j];
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, val2);
			}
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
